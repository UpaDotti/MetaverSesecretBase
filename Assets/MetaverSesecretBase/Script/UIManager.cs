using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// uGUIベースのメニューUIとプレイUIをまとめて仲介
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Network Select UI")]
    [SerializeField]
    private GameObject _networkSelectUI;

    [SerializeField]
    private Button _hostButton;

    [SerializeField]
    private Button _clientButton;

    [Header("Emote UI")]
    [SerializeField]
    private GameObject _emoteUI;

    [SerializeField]
    private Button[] _emoteButtons;

    /// <summary>
    /// 起動時に旧uGUIの初期表示を閉じる
    /// </summary>
    private void Awake()
    {
        ShowUI(UIState.None);
        _emoteUI?.SetActive(false);
    }

    /// <summary>
    /// 指定したメニューUIだけを表示
    /// </summary>
    public void ShowUI(UIState state)
    {
        _networkSelectUI?.SetActive(state == UIState.NetworkSelect);
    }

    /// <summary>
    /// プレイ中のエモートUIを表示してイベントを購読
    /// </summary>
    public void ShowPlayUI(Action<int> onClickEmote)
    {
        _emoteUI.SetActive(true);

        for (int i = 0; i < _emoteButtons.Length; i++)
        {
            int emoteId = i;
            _emoteButtons[i].onClick.AddListener(() => onClickEmote.Invoke(emoteId));
        }
    }

    /// <summary>
    /// プレイ中のエモートUIを非表示にしてイベントを解除
    /// </summary>
    public void HidePlayUI()
    {
        for (int i = 0; i < _emoteButtons.Length; i++)
        {
            _emoteButtons[i].onClick.RemoveAllListeners();
        }

        _emoteUI.SetActive(false);
    }
}

public enum UIState
{
    None,
    NetworkSelect
}
