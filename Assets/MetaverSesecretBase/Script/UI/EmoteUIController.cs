using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// プレイ中のエモートUIを UI Toolkit で管理
/// </summary>
public class EmoteUIController : MonoBehaviour
{
    private const string LogPrefix = "[EmoteUI]";
    private const string PanelSettingsResourcePath = "UI/RoomBrowserPanelSettings";
    private const string LayoutResourcePath = "UI/Emote";
    private const string StyleResourcePath = "UI/Emote";
    private static readonly Vector2Int AndroidReferenceResolution = new(1920, 1080);
    private const float AndroidScreenMatch = 0f;
    private const float FallbackDpi = 96f;
    private const int SortingOrder = 100;

    private UIDocument _uiDocument;
    private PanelSettings _runtimePanelSettings;
    private VisualElement _root;
    private VisualElement _emoteRoot;
    private VisualElement _buttonRow;
    private bool _isInteractable = true;
    private bool _needsRebuild;
    private bool _hasLoggedMissingPlayer;

    public event Action<int> EmoteSelected;

    /// <summary>
    /// UI Document生成と参照取得を初期化
    /// </summary>
    private void Awake()
    {
        InitializeDocument();
        CacheElements();
        Hide();
    }

    /// <summary>
    /// プレイヤー生成後のボタン再構築を待つ
    /// </summary>
    private void Update()
    {
        if (!_needsRebuild || _emoteRoot == null || _emoteRoot.style.display == DisplayStyle.None)
        {
            return;
        }

        RebuildButtons();
    }

    /// <summary>
    /// 生成したPanelSettingsを破棄
    /// </summary>
    private void OnDestroy()
    {
        if (_runtimePanelSettings != null)
        {
            Destroy(_runtimePanelSettings);
        }
    }

    /// <summary>
    /// UI Documentを生成してUI Toolkitを初期化
    /// </summary>
    private void InitializeDocument()
    {
        PanelSettings panelSettingsAsset = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
        VisualTreeAsset layoutAsset = Resources.Load<VisualTreeAsset>(LayoutResourcePath);
        StyleSheet styleSheet = Resources.Load<StyleSheet>(StyleResourcePath);

        if (panelSettingsAsset == null || layoutAsset == null || styleSheet == null)
        {
            Debug.LogError($"{LogPrefix} Failed to load UI Toolkit resources.");
            return;
        }

        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            _uiDocument = gameObject.AddComponent<UIDocument>();
        }

        _runtimePanelSettings = Instantiate(panelSettingsAsset);
        _runtimePanelSettings.name = $"{panelSettingsAsset.name}_Runtime";
        ApplyAndroidScaleSettings(_runtimePanelSettings);
        _uiDocument.panelSettings = _runtimePanelSettings;
        _uiDocument.sortingOrder = SortingOrder;
        _root = _uiDocument.rootVisualElement;
        _root.Clear();
        layoutAsset.CloneTree(_root);
        _root.styleSheets.Add(styleSheet);
    }

    /// <summary>
    /// Android向けの実行時スケール設定を適用
    /// </summary>
    private void ApplyAndroidScaleSettings(PanelSettings panelSettings)
    {
        panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution = AndroidReferenceResolution;
        panelSettings.match = AndroidScreenMatch;
        panelSettings.referenceDpi = FallbackDpi;
        panelSettings.fallbackDpi = FallbackDpi;
    }

    /// <summary>
    /// UXMLから要素参照を取得
    /// </summary>
    private void CacheElements()
    {
        if (_root == null)
        {
            return;
        }

        _emoteRoot = _root.Q<VisualElement>("emote-root");
        _buttonRow = _root.Q<VisualElement>("emote-button-row");

        if (_emoteRoot == null || _buttonRow == null)
        {
            Debug.LogError($"{LogPrefix} Failed to cache UI elements.");
        }
    }

    /// <summary>
    /// エモートUI全体を表示
    /// </summary>
    public void Show()
    {
        if (_emoteRoot == null)
        {
            return;
        }

        _emoteRoot.style.display = DisplayStyle.Flex;
        _needsRebuild = true;
        RebuildButtons();
    }

    /// <summary>
    /// エモートUI全体を非表示
    /// </summary>
    public void Hide()
    {
        if (_emoteRoot == null)
        {
            return;
        }

        _emoteRoot.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// UIの操作可否を切り替え
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;
        _emoteRoot?.SetEnabled(interactable);
    }

    /// <summary>
    /// 利用可能なエモートボタンを再構築
    /// </summary>
    private void RebuildButtons()
    {
        if (_buttonRow == null)
        {
            return;
        }

        if (!TryResolveNetworkPlayer(out NetworkPlayer networkPlayer))
        {
            return;
        }

        _buttonRow.Clear();

        int emoteCount = networkPlayer.EmoteCount;
        int lastEmoteId = GetLastEmoteId(networkPlayer, emoteCount);
        for (int emoteId = 0; emoteId < emoteCount; emoteId++)
        {
            if (!networkPlayer.TryGetEmoteSprite(emoteId, out Sprite sprite))
            {
                continue;
            }

            _buttonRow.Add(CreateEmoteButton(emoteId, sprite, emoteId == lastEmoteId));
        }

        _needsRebuild = false;
        _hasLoggedMissingPlayer = false;
        SetInteractable(_isInteractable);
    }

    /// <summary>
    /// エモート用ボタンを生成
    /// </summary>
    private Button CreateEmoteButton(int emoteId, Sprite sprite, bool isLast)
    {
        Button button = new();
        button.text = string.Empty;
        button.AddToClassList("emote-button");

        if (isLast)
        {
            button.AddToClassList("emote-button--last");
        }

        button.userData = emoteId;
        button.style.backgroundImage = new StyleBackground(sprite);

        // 押下時に選択したエモートを通知する
        button.clicked += () =>
        {
            if (button.userData is int selectedEmoteId)
            {
                EmoteSelected?.Invoke(selectedEmoteId);
            }
        };

        return button;
    }

    /// <summary>
    /// 最後に表示するエモートIDを返す
    /// </summary>
    private int GetLastEmoteId(NetworkPlayer networkPlayer, int emoteCount)
    {
        for (int emoteId = emoteCount - 1; emoteId >= 0; emoteId--)
        {
            if (networkPlayer.TryGetEmoteSprite(emoteId, out _))
            {
                return emoteId;
            }
        }

        return -1;
    }

    /// <summary>
    /// 表示に使うNetworkPlayerを解決
    /// </summary>
    private bool TryResolveNetworkPlayer(out NetworkPlayer networkPlayer)
    {
        networkPlayer = FindAnyObjectByType<NetworkPlayer>();
        if (networkPlayer != null)
        {
            return true;
        }

        if (!_hasLoggedMissingPlayer)
        {
            Debug.LogWarning($"{LogPrefix} NetworkPlayer not found. Waiting for spawn.");
            _hasLoggedMissingPlayer = true;
        }

        return false;
    }
}
