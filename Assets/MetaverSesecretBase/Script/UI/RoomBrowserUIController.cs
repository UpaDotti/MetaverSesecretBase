using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 部屋一覧と部屋作成のUI Toolkit表示を単一画面で管理
/// </summary>
public class RoomBrowserUIController : MonoBehaviour
{
    private const string LogPrefix = "[RoomBrowserUI]";
    private const string PanelSettingsResourcePath = "UI/RoomBrowserPanelSettings";
    private const string CommonStyleResourcePath = "UI/CommonMenu";
    private const string LayoutResourcePath = "UI/RoomBrowser";
    private const string StyleResourcePath = "UI/RoomBrowser";
    private const string HiddenClassName = "is-hidden";
    private const string ActiveClassName = "is-active";
    private const float RoomListItemHeight = 122f;
    private static readonly Vector2Int AndroidReferenceResolution = new(1920, 1080);
    private const float AndroidScreenMatch = 0f;
    private const float FallbackDpi = 96f;

    private readonly List<LobbySummary> _rooms = new();

    private UIDocument _uiDocument;
    private PanelSettings _runtimePanelSettings;
    private VisualElement _root;
    private VisualElement _roomListSection;
    private VisualElement _roomCreateSection;
    private Button _showListTabButton;
    private Button _showCreateTabButton;
    private Label _listStatusLabel;
    private Label _emptyLabel;
    private ListView _roomListView;
    private Button _refreshButton;
    private Button _createButton;
    private TextField _roomNameField;
    private Label _createStatusLabel;

    public event Action RefreshRequested;
    public event Action<string> JoinRequested;
    public event Action<string> CreateSubmitted;

    /// <summary>
    /// UI Document生成と参照取得を初期化
    /// </summary>
    private void Awake()
    {
        InitializeDocument();
        CacheElements();
        BindStaticEvents();
        ShowListTab();
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
        LogPanelSettingsState("InitializeDocument");
    }

    /// <summary>
    /// Android向けの実行時スケール設定を適用
    /// </summary>
    private void ApplyAndroidScaleSettings(PanelSettings panelSettings)
    {
        panelSettings.scaleMode = (PanelScaleMode)2;
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

        _roomListSection = _root.Q<VisualElement>("room-list-section");
        _roomCreateSection = _root.Q<VisualElement>("room-create-section");
        _showListTabButton = _root.Q<Button>("show-list-tab-button");
        _showCreateTabButton = _root.Q<Button>("show-create-tab-button");
        _listStatusLabel = _root.Q<Label>("list-status-label");
        _emptyLabel = _root.Q<Label>("empty-label");
        _roomListView = _root.Q<ListView>("room-list-view");
        _refreshButton = _root.Q<Button>("refresh-button");
        _createButton = _root.Q<Button>("create-button");
        _roomNameField = _root.Q<TextField>("room-name-field");
        _createStatusLabel = _root.Q<Label>("create-status-label");

        ConfigureRoomListView();
    }

    /// <summary>
    /// 一覧表示用ListViewを初期設定
    /// </summary>
    private void ConfigureRoomListView()
    {
        if (_roomListView == null)
        {
            return;
        }

        _roomListView.selectionType = SelectionType.None;
        _roomListView.fixedItemHeight = RoomListItemHeight;
        _roomListView.itemsSource = _rooms;
        _roomListView.makeItem = MakeRoomListItem;
        _roomListView.bindItem = BindRoomListItem;
    }

    /// <summary>
    /// 固定ボタンのイベントを購読
    /// </summary>
    private void BindStaticEvents()
    {
        if (_showListTabButton != null)
        {
            _showListTabButton.clicked += ShowListTab;
        }

        if (_showCreateTabButton != null)
        {
            _showCreateTabButton.clicked += ShowCreateTab;
        }

        if (_refreshButton != null)
        {
            _refreshButton.clicked += OnRefreshClicked;
        }

        if (_createButton != null)
        {
            _createButton.clicked += OnCreateClicked;
        }
    }

    /// <summary>
    /// 固定ボタンのイベント購読を解除
    /// </summary>
    private void UnbindStaticEvents()
    {
        if (_showListTabButton != null)
        {
            _showListTabButton.clicked -= ShowListTab;
        }

        if (_showCreateTabButton != null)
        {
            _showCreateTabButton.clicked -= ShowCreateTab;
        }

        if (_refreshButton != null)
        {
            _refreshButton.clicked -= OnRefreshClicked;
        }

        if (_createButton != null)
        {
            _createButton.clicked -= OnCreateClicked;
        }
    }

    /// <summary>
    /// 部屋一覧1行分のVisualElementを生成
    /// </summary>
    private VisualElement MakeRoomListItem()
    {
        VisualElement row = new();
        row.AddToClassList("room-row");

        VisualElement textContainer = new();
        textContainer.AddToClassList("room-row__text");

        Label roomNameLabel = new() { name = "room-name-label" };
        roomNameLabel.AddToClassList("room-row__name");

        Label roomCapacityLabel = new() { name = "room-capacity-label" };
        roomCapacityLabel.AddToClassList("room-row__capacity");

        Button joinButton = new() { name = "join-button", text = "参加" };
        joinButton.AddToClassList("room-row__button");
        joinButton.clicked += () =>
        {
            if (joinButton.userData is string lobbyId)
            {
                JoinRequested?.Invoke(lobbyId);
            }
        };

        textContainer.Add(roomNameLabel);
        textContainer.Add(roomCapacityLabel);
        row.Add(textContainer);
        row.Add(joinButton);
        return row;
    }

    /// <summary>
    /// 部屋一覧1行へLobby情報を反映
    /// </summary>
    private void BindRoomListItem(VisualElement item, int index)
    {
        if (index < 0 || index >= _rooms.Count)
        {
            return;
        }

        LobbySummary room = _rooms[index];
        item.Q<Label>("room-name-label").text = room.LobbyName;
        item.Q<Label>("room-capacity-label").text = $"{room.PlayerCount} / {room.MaxPlayers} 人が参加中";
        item.Q<Button>("join-button").userData = room.LobbyId;
    }

    /// <summary>
    /// 部屋ブラウザ全体を表示
    /// </summary>
    public void ShowRoomList()
    {
        if (_root == null)
        {
            Debug.LogWarning($"{LogPrefix} ShowRoomList skipped because root is null.");
            return;
        }

        _root.RemoveFromClassList(HiddenClassName);
        ShowListTab();
        SetRoomListInteractable(true);
        SetRoomCreateInteractable(true);
        SetCreateStatus(string.Empty);
        LogLayoutState("ShowRoomList");
    }

    /// <summary>
    /// 部屋ブラウザ全体を非表示
    /// </summary>
    public void Hide()
    {
        _root?.AddToClassList(HiddenClassName);
        Debug.Log($"{LogPrefix} Hide");
    }

    /// <summary>
    /// 部屋一覧を描画
    /// </summary>
    public void SetRooms(IReadOnlyList<LobbySummary> rooms)
    {
        _rooms.Clear();
        _rooms.AddRange(rooms);
        _roomListView?.Rebuild();
        UpdateEmptyState();
    }

    /// <summary>
    /// 部屋一覧画面の状態文言を更新
    /// </summary>
    public void SetListStatus(string message)
    {
        if (_listStatusLabel != null)
        {
            _listStatusLabel.text = message;
        }
    }

    /// <summary>
    /// 部屋作成画面の状態文言を更新
    /// </summary>
    public void SetCreateStatus(string message)
    {
        if (_createStatusLabel != null)
        {
            _createStatusLabel.text = message;
        }
    }

    /// <summary>
    /// 部屋一覧画面の操作可否を切り替え
    /// </summary>
    public void SetRoomListInteractable(bool interactable)
    {
        _roomListSection?.SetEnabled(interactable);
        _refreshButton?.SetEnabled(interactable);
    }

    /// <summary>
    /// 部屋作成画面の操作可否を切り替え
    /// </summary>
    public void SetRoomCreateInteractable(bool interactable)
    {
        _roomCreateSection?.SetEnabled(interactable);
        _createButton?.SetEnabled(interactable);
    }

    /// <summary>
    /// 部屋名入力欄を初期化
    /// </summary>
    public void ClearRoomName()
    {
        if (_roomNameField != null)
        {
            _roomNameField.value = string.Empty;
        }
    }

    /// <summary>
    /// 部屋0件時の表示を更新
    /// </summary>
    private void UpdateEmptyState()
    {
        if (_emptyLabel == null)
        {
            return;
        }

        _emptyLabel.EnableInClassList(HiddenClassName, _rooms.Count > 0);
    }

    /// <summary>
    /// 更新ボタン押下を通知
    /// </summary>
    private void OnRefreshClicked()
    {
        RefreshRequested?.Invoke();
    }

    /// <summary>
    /// 作成確定を通知
    /// </summary>
    private void OnCreateClicked()
    {
        CreateSubmitted?.Invoke(_roomNameField?.value ?? string.Empty);
    }

    /// <summary>
    /// 参加タブを表示
    /// </summary>
    private void ShowListTab()
    {
        SetActiveTab(showList: true);
    }

    /// <summary>
    /// 作成タブを表示
    /// </summary>
    private void ShowCreateTab()
    {
        SetActiveTab(showList: false);
    }

    /// <summary>
    /// タブ表示と見た目を切り替える
    /// </summary>
    private void SetActiveTab(bool showList)
    {
        if (_roomListSection == null || _roomCreateSection == null)
        {
            return;
        }

        _roomListSection.EnableInClassList(HiddenClassName, !showList);
        _roomCreateSection.EnableInClassList(HiddenClassName, showList);
        _showListTabButton?.EnableInClassList(ActiveClassName, showList);
        _showCreateTabButton?.EnableInClassList(ActiveClassName, !showList);
    }

    /// <summary>
    /// 表示時点のUIサイズをログへ出力
    /// </summary>
    private void LogLayoutState(string source)
    {
        if (_root == null)
        {
            return;
        }

        float rootWidth = _root.resolvedStyle.width;
        float rootHeight = _root.resolvedStyle.height;
        float listWidth = _roomListSection?.resolvedStyle.width ?? -1f;
        float listHeight = _roomListSection?.resolvedStyle.height ?? -1f;
        bool rootHidden = _root.ClassListContains(HiddenClassName);
        float screenDpi = Screen.dpi > 0f ? Screen.dpi : FallbackDpi;
        Debug.Log($"{LogPrefix} {source}: root=({rootWidth:0.##}, {rootHeight:0.##}) rootHidden={rootHidden} list=({listWidth:0.##}, {listHeight:0.##}) screen=({Screen.width}, {Screen.height}) dpi={screenDpi:0.##}");
    }

    /// <summary>
    /// PanelSettingsと画面情報をログへ出力
    /// </summary>
    private void LogPanelSettingsState(string source)
    {
        if (_runtimePanelSettings == null)
        {
            return;
        }

        float screenDpi = Screen.dpi > 0f ? Screen.dpi : FallbackDpi;
        Vector2 referenceResolution = _runtimePanelSettings.referenceResolution;
        Debug.Log(
            $"{LogPrefix} {source}: panelSettings={_runtimePanelSettings.name} " +
            $"scaleMode={_runtimePanelSettings.scaleMode} referenceResolution=({referenceResolution.x:0.##}, {referenceResolution.y:0.##}) " +
            $"match={_runtimePanelSettings.match:0.##} referenceDpi={_runtimePanelSettings.referenceDpi:0.##} fallbackDpi={_runtimePanelSettings.fallbackDpi:0.##} " +
            $"screen=({Screen.width}, {Screen.height}) dpi={screenDpi:0.##}");
    }
}
