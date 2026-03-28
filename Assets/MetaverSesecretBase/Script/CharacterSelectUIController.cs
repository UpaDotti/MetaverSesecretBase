using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// キャラクター選択専用のUI Toolkit表示を管理
/// </summary>
public class CharacterSelectUIController : MonoBehaviour
{
    private const string LogPrefix = "[CharacterSelectUI]";
    private const string PanelSettingsResourcePath = "UI/RoomBrowserPanelSettings";
    private const string CommonStyleResourcePath = "UI/CommonMenu";
    private const string LayoutResourcePath = "UI/CharacterSelect";
    private const string StyleResourcePath = "UI/CharacterSelect";
    private const string HiddenClassName = "is-hidden";
    private const float CharacterListItemHeight = 148f;
    private static readonly Vector2Int AndroidReferenceResolution = new(1920, 1080);
    private const float AndroidScreenMatch = 0f;
    private const float FallbackDpi = 96f;

    [SerializeField]
    private CharacterSpriteDB _characterSpriteDB;

    private readonly List<Sprite> _characters = new();

    private UIDocument _uiDocument;
    private PanelSettings _runtimePanelSettings;
    private VisualElement _root;
    private VisualElement _listSection;
    private Label _statusLabel;
    private ListView _characterListView;
    private bool _isInteractable = true;

    public event Action<int> CharacterSelected;

    /// <summary>
    /// UI Document生成と参照取得を初期化
    /// </summary>
    private void Awake()
    {
        InitializeDocument();
        CacheElements();
        ConfigureCharacterListView();
        RefreshCharacters();
        Hide();
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
        StyleSheet commonStyleSheet = Resources.Load<StyleSheet>(CommonStyleResourcePath);
        VisualTreeAsset layoutAsset = Resources.Load<VisualTreeAsset>(LayoutResourcePath);
        StyleSheet styleSheet = Resources.Load<StyleSheet>(StyleResourcePath);

        if (panelSettingsAsset == null || commonStyleSheet == null || layoutAsset == null || styleSheet == null)
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

        _listSection = _root.Q<VisualElement>("character-list-section");
        _statusLabel = _root.Q<Label>("character-status-label");
        _characterListView = _root.Q<ListView>("character-list-view");
    }

    /// <summary>
    /// 一覧表示用ListViewを初期設定
    /// </summary>
    private void ConfigureCharacterListView()
    {
        if (_characterListView == null)
        {
            return;
        }

        _characterListView.selectionType = SelectionType.None;
        _characterListView.fixedItemHeight = CharacterListItemHeight;
        _characterListView.itemsSource = _characters;
        _characterListView.makeItem = MakeCharacterListItem;
        _characterListView.bindItem = BindCharacterListItem;
    }

    /// <summary>
    /// キャラクター選択UI全体を表示
    /// </summary>
    public void Show()
    {
        if (_root == null)
        {
            return;
        }

        RefreshCharacters();
        _root.RemoveFromClassList(HiddenClassName);
    }

    /// <summary>
    /// キャラクター選択UI全体を非表示
    /// </summary>
    public void Hide()
    {
        _root?.AddToClassList(HiddenClassName);
    }

    /// <summary>
    /// UIの操作可否を切り替え
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;
        ApplyInteractableState();
        _characterListView?.Rebuild();
    }

    /// <summary>
    /// キャラクター一覧1行分のVisualElementを生成
    /// </summary>
    private VisualElement MakeCharacterListItem()
    {
        VisualElement row = new();
        row.AddToClassList("character-row");

        VisualElement preview = new() { name = "character-preview" };
        preview.AddToClassList("character-row__preview");

        VisualElement textContainer = new();
        textContainer.AddToClassList("character-row__text");

        Label nameLabel = new() { name = "character-name-label" };
        nameLabel.AddToClassList("character-row__name");

        Label captionLabel = new() { name = "character-caption-label" };
        captionLabel.AddToClassList("character-row__caption");

        Button selectButton = new() { name = "character-select-button", text = "このキャラを使う" };
        selectButton.AddToClassList("character-row__button");
        selectButton.clicked += () =>
        {
            if (selectButton.userData is int characterIndex)
            {
                CharacterSelected?.Invoke(characterIndex);
            }
        };

        textContainer.Add(nameLabel);
        textContainer.Add(captionLabel);
        row.Add(preview);
        row.Add(textContainer);
        row.Add(selectButton);
        return row;
    }

    /// <summary>
    /// キャラクター一覧1行へ表示情報を反映
    /// </summary>
    private void BindCharacterListItem(VisualElement item, int index)
    {
        if (index < 0 || index >= _characters.Count)
        {
            return;
        }

        Sprite sprite = _characters[index];
        item.Q<VisualElement>("character-preview").style.backgroundImage = new StyleBackground(sprite);
        item.Q<Label>("character-name-label").text = $"キャラクター {index + 1}";
        item.Q<Label>("character-caption-label").text = "タップするとこのキャラで次へ進みます";

        Button selectButton = item.Q<Button>("character-select-button");
        selectButton.userData = index;
        selectButton.SetEnabled(_isInteractable);
    }

    /// <summary>
    /// 利用可能なキャラクター一覧を再取得
    /// </summary>
    private void RefreshCharacters()
    {
        _characters.Clear();

        if (!TryResolveCharacterSpriteDb())
        {
            SetStatus("キャラクター設定が見つかりません");
            ApplyInteractableState();
            _characterListView?.Rebuild();
            return;
        }

        if (_characterSpriteDB.Characters == null)
        {
            SetStatus("選べるキャラクターがありません");
            ApplyInteractableState();
            _characterListView?.Rebuild();
            return;
        }

        foreach (Sprite sprite in _characterSpriteDB.Characters)
        {
            if (sprite != null)
            {
                _characters.Add(sprite);
            }
        }

        if (_characters.Count == 0)
        {
            SetStatus("選べるキャラクターがありません");
        }
        else
        {
            SetStatus($"{_characters.Count} 体から選択してください");
        }

        ApplyInteractableState();
        _characterListView?.Rebuild();
    }

    /// <summary>
    /// CharacterSpriteDB参照を自動解決
    /// </summary>
    private bool TryResolveCharacterSpriteDb()
    {
        if (_characterSpriteDB != null)
        {
            return true;
        }

        NetworkPlayer[] networkPlayers = Resources.FindObjectsOfTypeAll<NetworkPlayer>();
        foreach (NetworkPlayer networkPlayer in networkPlayers)
        {
            if (networkPlayer.CharacterSpriteDB != null)
            {
                _characterSpriteDB = networkPlayer.CharacterSpriteDB;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 状態文言を更新
    /// </summary>
    private void SetStatus(string message)
    {
        if (_statusLabel != null)
        {
            _statusLabel.text = message;
        }
    }

    /// <summary>
    /// 一覧の操作可否を現在状態へ反映
    /// </summary>
    private void ApplyInteractableState()
    {
        bool canInteract = _isInteractable && _characters.Count > 0;
        _listSection?.SetEnabled(canInteract);
    }
}
