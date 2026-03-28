using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 名前入力専用のUI Toolkit表示を管理
/// </summary>
public class NameInputUIController : MonoBehaviour
{
    private const string PanelSettingsResourcePath = "UI/RoomBrowserPanelSettings";
    private const string CommonStyleResourcePath = "UI/CommonMenu";
    private const string LayoutResourcePath = "UI/NameInput";
    private const string StyleResourcePath = "UI/NameInput";
    private static readonly Vector2Int AndroidReferenceResolution = new Vector2Int(1920, 1080);
    private const float AndroidScreenMatch = 0f;
    private const float FallbackDpi = 96f;

    private UIDocument _uiDocument;
    private PanelSettings _runtimePanelSettings;
    private VisualElement _root;
    private TextField _nameField;
    private Button _confirmButton;

    public event Action<string> NameChanged;
    public event Action Submitted;

    /// <summary>
    /// UI Document生成と参照取得を初期化
    /// </summary>
    private void Awake()
    {
        InitializeDocument();
        CacheElements();
        BindStaticEvents();
        Hide();
    }

    /// <summary>
    /// 購読したイベントを破棄
    /// </summary>
    private void OnDestroy()
    {
        UnbindStaticEvents();

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
        StyleSheet commonStyleSheet = Resources.Load<StyleSheet>(CommonStyleResourcePath);
        VisualTreeAsset layoutAsset = Resources.Load<VisualTreeAsset>(LayoutResourcePath);
        StyleSheet styleSheet = Resources.Load<StyleSheet>(StyleResourcePath);

        if (panelSettingsAsset == null || commonStyleSheet == null || layoutAsset == null || styleSheet == null)
        {
            Debug.LogError("[NameInputUI] Failed to load UI Toolkit resources.");
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
        _root = _uiDocument.rootVisualElement;
        _root.Clear();
        layoutAsset.CloneTree(_root);
        _root.styleSheets.Add(commonStyleSheet);
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

        _nameField = _root.Q<TextField>("name-input-field");
        _confirmButton = _root.Q<Button>("name-confirm-button");
    }

    /// <summary>
    /// 固定イベントを購読
    /// </summary>
    private void BindStaticEvents()
    {
        if (_nameField != null)
        {
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            _nameField.RegisterCallback<KeyDownEvent>(OnNameFieldKeyDown);
        }

        if (_confirmButton != null)
        {
            _confirmButton.clicked += OnConfirmClicked;
        }
    }

    /// <summary>
    /// 固定イベントの購読を解除
    /// </summary>
    private void UnbindStaticEvents()
    {
        if (_nameField != null)
        {
            _nameField.UnregisterValueChangedCallback(OnNameChanged);
            _nameField.UnregisterCallback<KeyDownEvent>(OnNameFieldKeyDown);
        }

        if (_confirmButton != null)
        {
            _confirmButton.clicked -= OnConfirmClicked;
        }
    }

    /// <summary>
    /// 名前入力UI全体を表示
    /// </summary>
    public void Show()
    {
        if (_root == null)
        {
            return;
        }

        _root.style.display = DisplayStyle.Flex;
        _nameField?.Focus();
    }

    /// <summary>
    /// 名前入力UI全体を非表示
    /// </summary>
    public void Hide()
    {
        if (_root == null)
        {
            return;
        }

        _root.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// 入力欄へ現在名を反映
    /// </summary>
    public void SetName(string playerName)
    {
        if (_nameField == null)
        {
            return;
        }

        _nameField.SetValueWithoutNotify(playerName ?? string.Empty);
    }

    /// <summary>
    /// UIの操作可否を切り替え
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _root?.SetEnabled(interactable);
    }

    /// <summary>
    /// 入力変更を通知
    /// </summary>
    private void OnNameChanged(ChangeEvent<string> evt)
    {
        NameChanged?.Invoke(evt.newValue);
    }

    /// <summary>
    /// Enterキー押下で確定を通知
    /// </summary>
    private void OnNameFieldKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
        {
            return;
        }

        evt.StopPropagation();
        Submitted?.Invoke();
    }

    /// <summary>
    /// 確定ボタン押下を通知
    /// </summary>
    private void OnConfirmClicked()
    {
        Submitted?.Invoke();
    }
}
