using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private IState _currentState;
    private StateContext _context;



    private void Awake()
    {
        var networkManager = FindAnyObjectByType<NetworkManager>();
        var uiManager = FindAnyObjectByType<UIManager>();
        var playerManager = FindAnyObjectByType<PlayerManager>();

        _context = new StateContext(uiManager, playerManager, networkManager);
    }



    private void Start()
    {
        // 初期に入力のステートフロー
        List<IState> stateFlow = new List<IState>
        {
            new InputNameState(_context),
            new SelectCharacterState(_context),
            new SelectNetworkState(_context),
        };

        ExecuteStateFlow(stateFlow);
    }



    /// <summary>
    /// ステートフローを実行
    /// </summary>
    /// <param name="states"></param>
    private void ExecuteStateFlow(List<IState> states)
    {
        StartCoroutine(RunExecuteStateFlow(states));
    }

    private IEnumerator RunExecuteStateFlow(List<IState> states)
    {
        foreach (IState state in states)
        {
            ChangeState(state);

            bool isCompleted = false;
            void OnCompleted() => isCompleted = true;

            _currentState.OnCompleted += OnCompleted;
            yield return new WaitUntil(() => isCompleted);
            _currentState.OnCompleted -= OnCompleted;
        }


        ChangeState(new PlayState(_context));
    }



    /// <summary>
    /// ステートを変更
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(IState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}