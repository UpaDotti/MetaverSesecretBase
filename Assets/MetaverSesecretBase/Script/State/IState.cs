using System;
using System.Numerics;
using Unity.Netcode;

public interface IState
{
    public void Enter();
    public void Exit();

    public event Action OnCompleted;
}

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

        // 名前入力を設定
        _stateContext.UIManager.NameInputField.onValueChanged.AddListener(_stateContext.PlayerManager.SetName);
        _stateContext.UIManager.NameFinishButton.onClick.AddListener(Complete);
    }

    void IState.Exit()
    {
        _stateContext.UIManager.ShowUI(UIState.None);

        // 解除
        _stateContext.UIManager.NameInputField.onValueChanged.RemoveListener(_stateContext.PlayerManager.SetName);
        _stateContext.UIManager.NameFinishButton.onClick.RemoveAllListeners();
    }

    private void Complete()
    {
        OnCompleted?.Invoke();
    }
}

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

        // キャラ選択を設定
        _stateContext.UIManager.CharacterButton0.onClick.AddListener(() => SetCharacterId(0));
        _stateContext.UIManager.CharacterButton1.onClick.AddListener(() => SetCharacterId(1));
    }

    void IState.Exit()
    {
        _stateContext.UIManager.ShowUI(UIState.None);

        // 解除
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

public class SelectNetworkState : IState
{
    private StateContext _stateContext;
    public event Action OnCompleted;

    public SelectNetworkState(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    void IState.Enter()
    {
        _stateContext.UIManager.ShowUI(UIState.NetworkSelect);

        // ネットワークを選択
        _stateContext.UIManager.HostButton.onClick.AddListener(StartHost);
        _stateContext.UIManager.ClientButton.onClick.AddListener(StartClient);
    }

    void IState.Exit()
    {
        _stateContext.UIManager.ShowUI(UIState.None);

        // 解除
        _stateContext.UIManager.HostButton.onClick.RemoveAllListeners();
        _stateContext.UIManager.ClientButton.onClick.RemoveAllListeners();
    }

    private void Complete()
    {
        OnCompleted?.Invoke();
    }

    private void StartHost()
    {
        _stateContext.NetworkManager.StartHost();

        Complete();
    }

    private void StartClient()
    {
        _stateContext.NetworkManager.StartClient();

        Complete();
    }
}

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

public class StateContext
{
    public readonly UIManager UIManager;
    public readonly PlayerManager PlayerManager;
    public readonly NetworkManager NetworkManager;

    public StateContext(UIManager uiManager, PlayerManager playerManager, NetworkManager networkManager)
    {
        UIManager = uiManager;
        PlayerManager = playerManager;
        NetworkManager = networkManager;
    }
}