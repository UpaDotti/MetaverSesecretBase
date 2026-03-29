using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private IState _currentState;
    private StateContext _context;



    /// <summary>
    /// 実行時に必要な参照を集め、状態遷移コンテキストを組み立てます。
    /// </summary>
    private void Awake()
    {
        var networkManager = FindAnyObjectByType<NetworkManager>();
        var nameInputUIController = FindAnyObjectByType<NameInputUIController>();
        var characterSelectUIController = FindAnyObjectByType<CharacterSelectUIController>();
        var roomBrowserUIController = FindAnyObjectByType<RoomBrowserUIController>();
        var emoteUIController = FindAnyObjectByType<EmoteUIController>();
        var playerManager = FindAnyObjectByType<PlayerManager>();
        var relayConnectionService = FindAnyObjectByType<RelayConnectionService>();

        // 名前入力UIの参照を自動補完する
        if (nameInputUIController == null)
        {
            GameObject controllerObject = new GameObject("NameInputUIController");
            nameInputUIController = controllerObject.AddComponent<NameInputUIController>();
        }

        // キャラ選択UIの参照を自動補完する
        if (characterSelectUIController == null)
        {
            GameObject controllerObject = new GameObject("CharacterSelectUIController");
            characterSelectUIController = controllerObject.AddComponent<CharacterSelectUIController>();
        }

        // エモートUIの参照を自動補完する
        if (emoteUIController == null)
        {
            GameObject controllerObject = new GameObject("EmoteUIController");
            emoteUIController = controllerObject.AddComponent<EmoteUIController>();
        }

        _context = new StateContext(nameInputUIController, characterSelectUIController, roomBrowserUIController, emoteUIController, playerManager, networkManager, relayConnectionService);
    }



    /// <summary>
    /// 初期ステートフローを定義し、順次実行を開始します。
    /// </summary>
    private void Start()
    {
        // 初期に入力のステートフロー
        List<IState> stateFlow = new List<IState>
        {
            new InputNameState(_context),
            new SelectCharacterState(_context),
            new RoomBrowseState(_context),
        };

        ExecuteStateFlow(stateFlow);
    }



    /// <summary>
    /// 指定ステート列をコルーチンで実行します。
    /// </summary>
    private void ExecuteStateFlow(List<IState> states)
    {
        StartCoroutine(RunExecuteStateFlow(states));
    }

    /// <summary>
    /// 完了イベントを待ちながら、ステートを順に進めます。
    /// </summary>
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
    /// 現在ステートを終了してから新ステートへ切り替えます。
    /// </summary>
    public void ChangeState(IState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}
