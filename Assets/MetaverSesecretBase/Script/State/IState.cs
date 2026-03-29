using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ステートの共通ルール
/// </summary>
public interface IState
{
    public void Enter();
    public void Exit();

    public event Action OnCompleted;
}

/// <summary>
/// 名前入力ステート
/// </summary>
public class InputNameState : IState
{
    private StateContext _stateContext;
    public event Action OnCompleted;

    public InputNameState(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    void IState.Enter()
    {
        _stateContext.NameInputUIController.SetName(_stateContext.PlayerManager.Name);
        _stateContext.NameInputUIController.SetInteractable(true);
        _stateContext.NameInputUIController.NameChanged += _stateContext.PlayerManager.SetName;
        _stateContext.NameInputUIController.Submitted += Complete;
        _stateContext.NameInputUIController.Show();
    }

    void IState.Exit()
    {
        _stateContext.NameInputUIController.NameChanged -= _stateContext.PlayerManager.SetName;
        _stateContext.NameInputUIController.Submitted -= Complete;
        _stateContext.NameInputUIController.Hide();
    }

    private void Complete()
    {
        OnCompleted?.Invoke();
    }
}

/// <summary>
/// キャラクター選択ステート
/// </summary>
public class SelectCharacterState : IState
{
    private StateContext _stateContext;
    public event Action OnCompleted;

    public SelectCharacterState(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    void IState.Enter()
    {
        _stateContext.CharacterSelectUIController.SetInteractable(true);
        _stateContext.CharacterSelectUIController.CharacterSelected += SetCharacterId;
        _stateContext.CharacterSelectUIController.Show();
    }

    void IState.Exit()
    {
        _stateContext.CharacterSelectUIController.CharacterSelected -= SetCharacterId;
        _stateContext.CharacterSelectUIController.Hide();
    }

    private void Complete()
    {
        OnCompleted?.Invoke();
    }

    private void SetCharacterId(int characterID)
    {
        _stateContext.PlayerManager.SetCharacterId(characterID);
        Complete();
    }
}

/// <summary>
/// 部屋一覧ステート
/// </summary>
public class RoomBrowseState : IState
{
    private const int RefreshIntervalMilliseconds = 10000;
    private const int RateLimitCooldownMilliseconds = 15000;

    private StateContext _stateContext;
    private bool _isActive;
    private bool _isProcessing;
    private bool _isRefreshing;
    private int _refreshLoopVersion;
    private DateTime _nextRefreshAllowedAtUtc = DateTime.MinValue;
    public event Action OnCompleted;

    public RoomBrowseState(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    /// <summary>
    /// 部屋一覧UIを表示してイベント購読を開始します。
    /// </summary>
    void IState.Enter()
    {
        Debug.Log("[RoomBrowseState] Enter");
        _isActive = true;
        _isProcessing = false;

        _stateContext.RoomBrowserUIController.RefreshRequested += OnRefreshRequested;
        _stateContext.RoomBrowserUIController.JoinRequested += OnJoinRequested;
        _stateContext.RoomBrowserUIController.CreateSubmitted += OnCreateSubmitted;

        _stateContext.RoomBrowserUIController.ShowRoomList();
        _stateContext.RoomBrowserUIController.SetRoomListInteractable(true);
        _stateContext.RoomBrowserUIController.SetRoomCreateInteractable(true);
        _stateContext.RoomBrowserUIController.ClearRoomName();
        _stateContext.RoomBrowserUIController.SetListStatus("部屋一覧を取得しています...");
        _stateContext.RoomBrowserUIController.SetCreateStatus(string.Empty);

        _ = RefreshRoomListAsync(true);
        _ = RunAutoRefreshLoopAsync(++_refreshLoopVersion);
    }

    /// <summary>
    /// イベント購読と表示を解除します。
    /// </summary>
    void IState.Exit()
    {
        _isActive = false;
        _refreshLoopVersion++;

        _stateContext.RoomBrowserUIController.RefreshRequested -= OnRefreshRequested;
        _stateContext.RoomBrowserUIController.JoinRequested -= OnJoinRequested;
        _stateContext.RoomBrowserUIController.CreateSubmitted -= OnCreateSubmitted;

        _stateContext.RoomBrowserUIController.Hide();
        _stateContext.RoomBrowserUIController.SetListStatus(string.Empty);
        _stateContext.RoomBrowserUIController.SetCreateStatus(string.Empty);
    }

    /// <summary>
    /// ステート完了を通知します。
    /// </summary>
    private void Complete()
    {
        OnCompleted?.Invoke();
    }

    /// <summary>
    /// 一覧更新要求を処理します。
    /// </summary>
    private void OnRefreshRequested()
    {
        _ = RefreshRoomListAsync(true);
    }

    /// <summary>
    /// 部屋参加要求を処理します。
    /// </summary>
    private void OnJoinRequested(string lobbyId)
    {
        _ = JoinRoomAsync(lobbyId);
    }

    /// <summary>
    /// 部屋作成要求を処理します。
    /// </summary>
    private void OnCreateSubmitted(string roomName)
    {
        _ = CreateRoomAsync(roomName);
    }

    /// <summary>
    /// 一覧自動更新を繰り返し実行します。
    /// </summary>
    private async Task RunAutoRefreshLoopAsync(int loopVersion)
    {
        while (_isActive && loopVersion == _refreshLoopVersion)
        {
            await Task.Delay(RefreshIntervalMilliseconds);

            if (!_isActive || loopVersion != _refreshLoopVersion || _isProcessing)
            {
                continue;
            }

            await RefreshRoomListAsync(false);
        }
    }

    /// <summary>
    /// 部屋一覧を取得してUIへ反映します。
    /// </summary>
    private async Task RefreshRoomListAsync(bool showRefreshingMessage)
    {
        if (!_isActive || _isProcessing || _isRefreshing)
        {
            return;
        }

        if (DateTime.UtcNow < _nextRefreshAllowedAtUtc)
        {
            if (showRefreshingMessage)
            {
                _stateContext.RoomBrowserUIController.SetListStatus("少し待ってから再度更新してください");
            }

            return;
        }

        try
        {
            _isRefreshing = true;

            if (showRefreshingMessage)
            {
                _stateContext.RoomBrowserUIController.SetListStatus("部屋一覧を更新しています...");
            }

            var rooms = await _stateContext.RelayConnectionService.QueryAvailableLobbiesAsync();

            if (!_isActive)
            {
                return;
            }

            _stateContext.RoomBrowserUIController.SetRooms(rooms);
            _stateContext.RoomBrowserUIController.SetListStatus($"部屋数: {rooms.Count}");
        }
        catch (Exception ex)
        {
            if (IsRateLimitError(ex))
            {
                _nextRefreshAllowedAtUtc = DateTime.UtcNow.AddMilliseconds(RateLimitCooldownMilliseconds);
                Debug.LogWarning($"[RoomBrowse] Refresh rate limited. cooldownMs={RateLimitCooldownMilliseconds}");
                _stateContext.RoomBrowserUIController.SetListStatus("更新が多いため少し待ってから再試行します");
                return;
            }

            Debug.LogWarning($"[RoomBrowse] Refresh failed: {ex.Message}");
            _stateContext.RoomBrowserUIController.SetListStatus("部屋一覧の取得に失敗しました");
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    /// <summary>
    /// Lobby一覧更新のレート制限を検出
    /// </summary>
    private bool IsRateLimitError(Exception ex)
    {
        return ex.Message.Contains("Too Many Requests", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 選択した部屋へ接続します。
    /// </summary>
    private async Task JoinRoomAsync(string lobbyId)
    {
        if (_isProcessing || !_isActive)
        {
            return;
        }

        _isProcessing = true;
        _stateContext.RoomBrowserUIController.SetRoomListInteractable(false);
        _stateContext.RoomBrowserUIController.SetListStatus("部屋に参加しています...");

        bool started = await _stateContext.RelayConnectionService.StartClientFromLobbyAsync(lobbyId);
        if (started)
        {
            Complete();
            return;
        }

        _isProcessing = false;
        _stateContext.RoomBrowserUIController.SetRoomListInteractable(true);
        _stateContext.RoomBrowserUIController.SetListStatus("部屋への参加に失敗しました");
    }

    /// <summary>
    /// 新しい部屋を作成してホスト開始します。
    /// </summary>
    private async Task CreateRoomAsync(string roomName)
    {
        if (_isProcessing || !_isActive)
        {
            return;
        }

        _isProcessing = true;
        _stateContext.RoomBrowserUIController.SetRoomCreateInteractable(false);
        _stateContext.RoomBrowserUIController.SetCreateStatus("部屋を作成しています...");

        bool started = await _stateContext.RelayConnectionService.StartHostAsync(roomName);
        if (started)
        {
            Complete();
            return;
        }

        _isProcessing = false;
        _stateContext.RoomBrowserUIController.SetRoomCreateInteractable(true);
        _stateContext.RoomBrowserUIController.SetCreateStatus("部屋の作成に失敗しました");
    }
}

/// <summary>
/// プレイステート
/// </summary>
public class PlayState : IState
{
    private StateContext _stateContext;
    public event Action OnCompleted;

    public PlayState(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    void IState.Enter()
    {
        _stateContext.EmoteUIController.SetInteractable(true);
        _stateContext.EmoteUIController.EmoteSelected += _stateContext.PlayerManager.SendEmote;
        _stateContext.EmoteUIController.Show();
    }

    void IState.Exit()
    {
        _stateContext.EmoteUIController.EmoteSelected -= _stateContext.PlayerManager.SendEmote;
        _stateContext.EmoteUIController.Hide();
    }

    private void Complete()
    {
        OnCompleted?.Invoke();
    }
}

/// <summary>
/// ステート間で共有されるコンテキスト
/// </summary>
public class StateContext
{
    public readonly NameInputUIController NameInputUIController;
    public readonly CharacterSelectUIController CharacterSelectUIController;
    public readonly RoomBrowserUIController RoomBrowserUIController;
    public readonly EmoteUIController EmoteUIController;
    public readonly PlayerManager PlayerManager;
    public readonly NetworkManager NetworkManager;
    public readonly RelayConnectionService RelayConnectionService;

    public StateContext(
        NameInputUIController nameInputUIController,
        CharacterSelectUIController characterSelectUIController,
        RoomBrowserUIController roomBrowserUIController,
        EmoteUIController emoteUIController,
        PlayerManager playerManager,
        NetworkManager networkManager,
        RelayConnectionService relayConnectionService)
    {
        NameInputUIController = nameInputUIController;
        CharacterSelectUIController = characterSelectUIController;
        RoomBrowserUIController = roomBrowserUIController;
        EmoteUIController = emoteUIController;
        PlayerManager = playerManager;
        NetworkManager = networkManager;
        RelayConnectionService = relayConnectionService;
    }
}
