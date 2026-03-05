using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// UnityRelayのネットワーク接続サービス
/// </summary>
public class RelayConnectionService : MonoBehaviour
{
    private const string RelayProtocol = "dtls";
    private static readonly Regex JoinCodeRegex = new("^[6789BCDFGHJKLMNPQRTWbcdfghjklmnpqrtw]{6,12}$", RegexOptions.Compiled);

    private NetworkManager _networkManager;
    private UnityTransport _unityTransport;

    private bool _isInitialized;



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
    public async Task<bool> StartHostAsync(int maxConnections = 4)
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

            Debug.Log($"[Relay] Host started. Share this JoinCode via chat: {joinCode}");
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
    /// Unity Services初期化と認証
    /// </summary>
    private async Task EnsureUnityServicesReadyAsync()
    {
        if (!_isInitialized)
        {
            await UnityServices.InitializeAsync();
            _isInitialized = true;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
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
}
