## 運用ルール
- 上限：80行を超えたら圧縮
- `Done` は直近だけ残す
- `Next` は最大 3 つまで

## 状態
- 進捗：96%
- Doing：エモート操作UIの移行方針整理
- Next：
  1. エモート操作UIの移行方針整理
  2. 旧uGUI整理の手順整理
  3. 各 UI の遷移とイベント重複確認項目を整理
- Done（直近）：uGUI の接続選択処理を不要化し、コードと仕様から外した
- Done（直近）：キャラ選択UIを UI Toolkit 管理へ分離した
- Done（直近）：SelectCharacterState を UIManager 依存から外した
- Done（直近）：InputNameState を UIManager 依存から外した
- Done（直近）：UIManager を facade のまま意図ベース API へ整理した
- Done（直近）：RoomBrowser の UI Toolkit 管理責務を UIManager から分離して明文化した
- Done（直近）：UI Toolkit 移行タスクを UIごとに分割する方針を整理した
- Done（直近）：RoomBrowser を Android 横向き前提の UI Toolkit 構成へ整理した
- Done（直近）：RoomBrowser を `参加 / 作成` タブ切替へ整理した
