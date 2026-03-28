## クラス図
- 目的：現状コードに存在するクラス/インターフェースの関係を可視化する
- ルール：仕様（SPEC）と分離し、このファイルで設計詳細を管理する
- 関係の見方：
  - `implements`：インターフェース実装
  - `has`：参照を保持（コンテキストとして持つ）
  - `uses`：処理内で利用
  - `controls`：開始/制御を担当

```mermaid
classDiagram
    direction LR

    class IState {
        <<interface>>
    }

    class StateManager
    class StateContext
    class InputNameState
    class SelectCharacterState
    class SelectNetworkState
    class RoomBrowseState
    class PlayState

    class RelayConnectionService
    class RelayRuntimeConfig
    class PlayerManager
    class PlayerMoveController
    class NetworkPlayer
    class UIManager
    class NameInputUIController
    class CharacterSelectUIController
    class RoomBrowserUIController
    class CharacterSpriteDB

    StateManager --> IState : manages state flow
    StateManager --> StateContext : has

    IState <|.. InputNameState : implements
    IState <|.. SelectCharacterState : implements
    IState <|.. SelectNetworkState : implements
    IState <|.. RoomBrowseState : implements
    IState <|.. PlayState : implements

    InputNameState --> StateContext : uses
    SelectCharacterState --> StateContext : uses
    SelectNetworkState --> StateContext : uses
    RoomBrowseState --> StateContext : uses
    PlayState --> StateContext : uses

    StateContext --> UIManager : has
    StateContext --> NameInputUIController : has
    StateContext --> CharacterSelectUIController : has
    StateContext --> RoomBrowserUIController : has
    StateContext --> PlayerManager : has
    StateContext --> NetworkManager : has (external)
    StateContext --> RelayConnectionService : has

    RelayConnectionService --> RelayRuntimeConfig : uses
    RelayConnectionService --> NetworkManager : uses (external)
    PlayerManager --> PlayerMoveController : controls
    PlayerManager --> NetworkPlayer : initializes
    NetworkPlayer --> CharacterSpriteDB : uses
    CharacterSelectUIController --> CharacterSpriteDB : uses
```

## 関係の要点
- `StateManager` が状態遷移のオーケストレーター（全体進行役）です。
- 各 `*State` は `StateContext` 経由で必要機能にアクセスします。
- `RelayConnectionService` は Relay 接続の専用責務で、`SelectNetworkState` から呼ばれます。
- `PlayerManager` はローカル入力結果を `NetworkPlayer` 初期化と移動開始に反映します。
- `StateContext` は `UIManager` と `NameInputUIController` と `CharacterSelectUIController` と `RoomBrowserUIController` を保持します。
- `UIManager` は未移行の uGUI メニューUIと Play UI の facade です。
- `NameInputUIController` は 名前入力専用の UI Toolkit 管理を担当します。
- `CharacterSelectUIController` は キャラ選択専用の UI Toolkit 管理を担当します。
- `RoomBrowserUIController` は RoomBrowser 専用の UI Toolkit 管理を担当します。
