## 運用ルール
- 上限：150行を超えたら圧縮
- 圧縮：長文は「結論1行＋理由1行」にする
- 詳細が必要なら：`./.codex/spec/` に逃がし、ここにはリンクだけ残す

## 仕様
- 目的：Unity Netcode を使った 2D マルチプレイヤー基盤を整備する
- 成功条件：名前入力 → キャラ選択 → RoomBrowser で参加 / 作成 → 移動開始まで一連で動く
- 対象プラットフォーム：Android のみ
- 画面向き：横向き前提

## 現在の正
- ステート駆動で画面遷移する
- 依存方向は外 → 内を維持する
- 接続は Unity Relay + Lobby を使う
- Client は Lobby 一覧から参加し、Join Code は Lobby Data から取得する
- メニューUIは UIごとに分割して段階的に UI Toolkit へ寄せる
- 名前入力UIは UI Toolkit への移行が完了済みとする
- `UIManager` は未移行の uGUI メニューUIと Play UI の公開窓口として残し、State は意図ベース API 経由で触る
- RoomBrowser は UI Toolkit への移行が完了済みとする
- `NameInputUIController` は 名前入力専用の UI Toolkit 管理責務として `UIManager` から分離して扱う
- `RoomBrowserUIController` は RoomBrowser 専用の UI Toolkit 管理責務として `UIManager` から分離して扱う
- 未移行のメニューUIは `キャラ選択 / 接続選択 / エモート操作` として扱う
- `Player.prefab` の頭上UIは今回の UI Toolkit 移行対象に含めない
- RoomBrowser は UI Toolkit の共通 UXML / USS / C# 構成で管理する
- RoomBrowser は `参加 / 作成` のタブ切替を基本とする
- RoomBrowser の表示倍率は Android 実機を正とし、PanelSettings は実行時に横向き基準へ上書きして使う
- RoomBrowser の一覧更新はレート制限を避けるため間隔を緩め、429 時はクールダウンする
- コードコメントはメソッド直上の `/// <summary>` を基本とする

## 確認メモ
- 今回の変更は `git staging` で確認できる状態を正とする
- クラス図：`./.codex/CLASS_DIAGRAM.md`
