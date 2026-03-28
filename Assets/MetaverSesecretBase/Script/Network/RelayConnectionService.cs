using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// 部屋一覧に表示するLobby要約情報
/// </summary>
public readonly struct LobbySummary
{
    public readonly string LobbyId;
    public readonly string LobbyName;
    public readonly int PlayerCount;
    public readonly int MaxPlayers;

    public LobbySummary(string lobbyId, string lobbyName, int playerCount, int maxPlayers)
    {
        LobbyId = lobbyId;
        LobbyName = lobbyName;
        PlayerCount = playerCount;
        MaxPlayers = maxPlayers;
    }
}

/// <summary>
/// UnityRelayのネットワーク接続サービス
/// </summary>
public class RelayConnectionService : MonoBehaviour
{
    private const string RelayProtocol = "dtls";
    private const string LobbyJoinCodeKey = "joinCode";
    private const string LobbyNamePrefix = "Room";
    private const float LobbyHeartbeatIntervalSeconds = 15f;
    private static readonly Regex JoinCodeRegex = new("^[6789BCDFGHJKLMNPQRTWbcdfghjklmnpqrtw]{6,12}$", RegexOptions.Compiled);

    private NetworkManager _networkManager;
    private UnityTransport _unityTransport;

    private bool _isInitialized;
    private string _hostLobbyId;
    private float _heartbeatTimer;
    private bool _isSendingHeartbeat;



    private void Update()
    {
        TickHeartbeat(Time.deltaTime);
    }

    private void OnDestroy()
    {
        _ = CleanupHostLobbyAsync();
    }

    /// <summary>
    /// 起動時に参照を解決
    /// </summary>
    private void Awake()
    {
        _networkManager = FindAnyObjectByType<NetworkManager>();
        _unityTransport = _networkManager.NetworkConfig.NetworkTransport as UnityTransport;
    }

    /// <summary>
    /// ホスト接続開始
    /// </summary>
    public async Task<bool> StartHostAsync(string roomName, int maxConnections = 4)
    {
        try
        {
            // 初期化
            await EnsureUnityServicesReadyAsync();

            // アロケーション作成
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            // 参加用のJoinCodeを取得
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Relay情報をTransportへ設定
            RelayServerData relayServerData = allocation.ToRelayServerData(RelayProtocol);
            _unityTransport.SetRelayServerData(relayServerData);

            // ホスト起動
            if (!_networkManager.StartHost())
            {
                Debug.LogError("[Relay] StartHost failed.");
                return false;
            }

            // JoinCodeを持つLobbyを作成
            bool lobbyCreated = await CreateHostLobbyAsync(maxConnections + 1, joinCode, roomName);
            if (!lobbyCreated)
            {
                Debug.LogWarning("[Lobby] Host started, but lobby creation failed.");
            }

            Debug.Log("[Relay] Host started with lobby discovery.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] StartHostAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// クライアント接続開始
    /// </summary>
    public async Task<bool> StartClientAsync(string joinCodeFromUi = null)
    {
        try
        {
            // 初期化
            await EnsureUnityServicesReadyAsync();

            // JoinCode取得（UI入力のみ）
            if (!TryNormalizeJoinCode(joinCodeFromUi, out string normalizedJoinCode))
            {
                Debug.LogError("[Relay] JoinCode is invalid. Use 6-12 chars from 6789BCDFGHJKLMNPQRTW.");
                return false;
            }

            // Relay参加
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(normalizedJoinCode);

            // Relay情報をTransportへ設定
            RelayServerData relayServerData = joinAllocation.ToRelayServerData(RelayProtocol);
            _unityTransport.SetRelayServerData(relayServerData);

            // クライアント起動
            if (!_networkManager.StartClient())
            {
                Debug.LogError("[Relay] StartClient failed.");
                return false;
            }

            Debug.Log($"[Relay] Client started with JoinCode: {normalizedJoinCode}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] StartClientAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lobby一覧から自動参加してクライアント接続開始
    /// </summary>
    public async Task<bool> StartClientFromLobbyAsync(string lobbyId)
    {
        try
        {
            // 初期化
            await EnsureUnityServicesReadyAsync();

            // LobbyId形式を確認
            if (string.IsNullOrWhiteSpace(lobbyId))
            {
                Debug.LogError("[Lobby] LobbyId is empty.");
                return false;
            }

            // 指定Lobbyに参加
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            // LobbyからJoinCodeを取得
            if (joinedLobby.Data == null || !joinedLobby.Data.TryGetValue(LobbyJoinCodeKey, out DataObject joinCodeData))
            {
                Debug.LogError("[Lobby] Joined lobby does not have joinCode.");
                return false;
            }

            // joinCodeを正規化
            if (!TryNormalizeJoinCode(joinCodeData.Value, out string normalizedJoinCode))
            {
                Debug.LogError("[Lobby] Lobby joinCode is invalid.");
                return false;
            }

            // Relay参加
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(normalizedJoinCode);

            // Relay情報をTransportへ設定
            RelayServerData relayServerData = joinAllocation.ToRelayServerData(RelayProtocol);
            _unityTransport.SetRelayServerData(relayServerData);

            if (!_networkManager.StartClient())
            {
                Debug.LogError("[Relay] StartClient failed.");
                return false;
            }

            Debug.Log($"[Relay] Client started via lobby: {joinedLobby.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Lobby] StartClientFromLobbyAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 参加可能なLobby一覧を取得
    /// </summary>
    public async Task<IReadOnlyList<LobbySummary>> QueryAvailableLobbiesAsync(int count = 20)
    {
        // 初期化
        await EnsureUnityServicesReadyAsync();

        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
        {
            // 一度に取得するLobbyの最大数
            Count = count,

            // 参加可能なLobbyのみをフィルタリング
            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
            },

            // 作成日時の降順でソート
            Order = new List<QueryOrder>
            {
                new(true, QueryOrder.FieldOptions.Created),
            },
        });

        // 取得したLobbyをLobbySummaryに変換
        if (queryResponse.Results == null)
        {
            return Array.Empty<LobbySummary>();
        }

        // LobbyのNameが空の場合は、LobbyNamePrefixと作成時間で名前を生成する
        return queryResponse.Results
            .Select(lobby => new LobbySummary(
                lobby.Id,
                string.IsNullOrWhiteSpace(lobby.Name) ? LobbyNamePrefix : lobby.Name,
                lobby.MaxPlayers - lobby.AvailableSlots,
                lobby.MaxPlayers))
            .ToArray();
    }

    /// <summary>
    /// Unity Services初期化と認証
    /// </summary>
    private async Task EnsureUnityServicesReadyAsync()
    {
        if (!_isInitialized)
        {
            // Unity Servicesを初期化
            await UnityServices.InitializeAsync();
            _isInitialized = true;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            // 匿名サインイン
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    /// <summary>
    /// JoinCode形式チェック
    /// </summary>
    private bool TryNormalizeJoinCode(string joinCode, out string normalizedJoinCode)
    {
        normalizedJoinCode = string.Empty;

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            return false;
        }

        normalizedJoinCode = joinCode.Trim().ToUpperInvariant();
        return JoinCodeRegex.IsMatch(normalizedJoinCode);
    }

    /// <summary>
    /// Host用Lobbyを作成
    /// </summary>
    private async Task<bool> CreateHostLobbyAsync(int maxPlayers, string joinCode, string roomName)
    {
        try
        {
            // joinCodeを正規化
            if (!TryNormalizeJoinCode(joinCode, out string normalizedJoinCode))
            {
                Debug.LogError("[Lobby] JoinCode is invalid when creating lobby.");
                return false;
            }

            // Lobby名を生成
            string lobbyName = BuildLobbyName(roomName);

            // Lobby作成オプションを作成
            CreateLobbyOptions options = new()
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    // Relay参加に必要なJoinCodeをLobbyのDataとして保存
                    { LobbyJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, normalizedJoinCode) },
                },
            };

            // Lobbyを作成
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            // 作成したLobbyのIDを保存
            _hostLobbyId = lobby.Id;

            // ハートビートタイマーをリセット
            _heartbeatTimer = LobbyHeartbeatIntervalSeconds;

            Debug.Log($"[Lobby] Lobby created: {lobby.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Lobby] CreateHostLobbyAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 部屋名を正規化して生成
    /// </summary>
    private string BuildLobbyName(string roomName)
    {
        if (!string.IsNullOrWhiteSpace(roomName))
        {
            return roomName.Trim();
        }

        return $"{LobbyNamePrefix}-{DateTime.UtcNow:HHmmss}";
    }

    /// <summary>
    /// ハートビートを更新
    /// </summary>
    private void TickHeartbeat(float deltaTime)
    {
        if (string.IsNullOrEmpty(_hostLobbyId))
        {
            return;
        }

        if (_isSendingHeartbeat)
        {
            return;
        }

        _heartbeatTimer -= deltaTime;
        if (_heartbeatTimer > 0f)
        {
            return;
        }

        _heartbeatTimer = LobbyHeartbeatIntervalSeconds;

        _ = SendHeartbeatAsync();
    }

    /// <summary>
    /// ハートビートを送信してLobbyを維持
    /// </summary>
    private async Task SendHeartbeatAsync()
    {
        _isSendingHeartbeat = true;

        try
        {
            // Lobbyにハートビートを送信して、Lobbyがアクティブな状態を維持する
            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobbyId);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Lobby] Heartbeat failed: {ex.Message}");
        }
        finally
        {
            _isSendingHeartbeat = false;
        }
    }

    /// <summary>
    /// Host用Lobbyをクリーンアップ
    /// </summary>
    private async Task CleanupHostLobbyAsync()
    {
        if (string.IsNullOrEmpty(_hostLobbyId))
        {
            return;
        }

        try
        {
            // Lobbyを削除
            await LobbyService.Instance.DeleteLobbyAsync(_hostLobbyId);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Lobby] Delete lobby failed: {ex.Message}");
        }
        finally
        {
            _hostLobbyId = null;
        }
    }
}
