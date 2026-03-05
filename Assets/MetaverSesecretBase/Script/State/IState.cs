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
        _stateContext.UIManager.ShowUI(UIState.NameInput);

        _stateContext.UIManager.NameInputField.onValueChanged.AddListener(_stateContext.PlayerManager.SetName);
        _stateContext.UIManager.NameFinishButton.onClick.AddListener(Complete);
    }

    void IState.Exit()
    {
        _stateContext.UIManager.ShowUI(UIState.None);

        _stateContext.UIManager.NameInputField.onValueChanged.RemoveListener(_stateContext.PlayerManager.SetName);
        _stateContext.UIManager.NameFinishButton.onClick.RemoveAllListeners();
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
        _stateContext.UIManager.ShowUI(UIState.CharacterSelect);

        _stateContext.UIManager.CharacterButton0.onClick.AddListener(() => SetCharacterId(0));
        _stateContext.UIManager.CharacterButton1.onClick.AddListener(() => SetCharacterId(1));
    }

    void IState.Exit()
    {
        _stateContext.UIManager.ShowUI(UIState.None);

        _stateContext.UIManager.CharacterButton0.onClick.RemoveAllListeners();
        _stateContext.UIManager.CharacterButton1.onClick.RemoveAllListeners();
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
/// ネットワーク選択ステート
/// </summary>
public class SelectNetworkState : IState
{
    private StateContext _stateContext;
    private bool _isConnecting;
    public event Action OnCompleted;

    public SelectNetworkState(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    /// <summary>
    /// ネットワーク選択UIを表示し、Host/Clientの入力受付を開始します。
    /// </summary>
    void IState.Enter()
    {
        _stateContext.UIManager.ShowUI(UIState.NetworkSelect);

        _stateContext.UIManager.HostButton.onClick.AddListener(StartHost);
        _stateContext.UIManager.ClientButton.onClick.AddListener(StartClient);
    }

    /// <summary>
    /// イベントを解除し、次回再入時の多重登録を防ぎます。
    /// </summary>
    void IState.Exit()
    {
        _stateContext.UIManager.ShowUI(UIState.None);

        _stateContext.UIManager.HostButton.onClick.RemoveAllListeners();
        _stateContext.UIManager.ClientButton.onClick.RemoveAllListeners();
        SetNetworkButtonsInteractable(true);
    }

    /// <summary>
    /// ステート完了を通知して次フェーズへ進めます。
    /// </summary>
    private void Complete()
    {
        OnCompleted?.Invoke();
    }

    /// <summary>
    /// Host接続の非同期開始を起動します。
    /// </summary>
    private void StartHost()
    {
        _ = StartHostAsync();
    }

    /// <summary>
    /// Client接続の非同期開始を起動します。
    /// </summary>
    private void StartClient()
    {
        _stateContext.IsClientSelected = true;
        Complete();
    }

    /// <summary>
    /// Host開始の排他制御とUI制御を行い、成功時に完了通知します。
    /// </summary>
    private async Task StartHostAsync()
    {
        // 多重実行防止
        if (_isConnecting)
        {
            return;
        }

        //　接続開始
        _isConnecting = true;
        SetNetworkButtonsInteractable(false);

        // ホスト接続開始
        bool started = await _stateContext.RelayConnectionService.StartHostAsync();

        //　接続完了
        if (started)
        {
            Complete();
            return;
        }

        // 接続失敗時は再度選択できるようにする
        _isConnecting = false;
        SetNetworkButtonsInteractable(true);
    }

    /// <summary>
    /// 接続中の誤操作防止のため、ボタン活性状態を切り替えます。
    /// </summary>
    private void SetNetworkButtonsInteractable(bool interactable)
    {
        _stateContext.UIManager.HostButton.interactable = interactable;
        _stateContext.UIManager.ClientButton.interactable = interactable;
    }
}

/// <summary>
/// JoinCode入力ステート
/// </summary>
public class InputJoinCodeState : IState
{
    private StateContext _stateContext;
    private bool _isConnecting;
    public event Action OnCompleted;

    public InputJoinCodeState(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    void IState.Enter()
    {
        if (!_stateContext.IsClientSelected)
        {
            Complete();
            return;
        }

        _stateContext.UIManager.ShowUI(UIState.JoinCodeInput);
        _stateContext.UIManager.JoinCodeFinishButton.onClick.AddListener(StartClient);
    }

    void IState.Exit()
    {
        _stateContext.UIManager.JoinCodeFinishButton.onClick.RemoveAllListeners();
        _stateContext.UIManager.JoinCodeFinishButton.interactable = true;
    }

    private void Complete()
    {
        OnCompleted?.Invoke();
    }

    private void StartClient()
    {
        _ = StartClientAsync();
    }

    private async Task StartClientAsync()
    {
        if (_isConnecting)
        {
            return;
        }

        _isConnecting = true;
        _stateContext.UIManager.JoinCodeFinishButton.interactable = false;

        string joinCode = _stateContext.UIManager.JoinCodeInputField != null
            ? _stateContext.UIManager.JoinCodeInputField.text
            : null;
        bool started = await _stateContext.RelayConnectionService.StartClientAsync(joinCode);

        if (started)
        {
            _stateContext.IsClientSelected = false;
            Complete();
            return;
        }

        _isConnecting = false;
        _stateContext.UIManager.JoinCodeFinishButton.interactable = true;
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
        _stateContext.UIManager.ShowUI(UIState.None);
    }

    void IState.Exit()
    {
        _stateContext.UIManager.ShowUI(UIState.None);
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
    public readonly UIManager UIManager;
    public readonly PlayerManager PlayerManager;
    public readonly NetworkManager NetworkManager;
    public readonly RelayConnectionService RelayConnectionService;
    public bool IsClientSelected;

    public StateContext(
        UIManager uiManager,
        PlayerManager playerManager,
        NetworkManager networkManager,
        RelayConnectionService relayConnectionService)
    {
        UIManager = uiManager;
        PlayerManager = playerManager;
        NetworkManager = networkManager;
        RelayConnectionService = relayConnectionService;
    }
}

