## 運用ルール
- 上限：200行を超えたら圧縮
- ここは実行順の目次に保つ
- 変更は小さく、確認しながら進める

## 実行順
- [ ] 1. Unity Editor で Android 横向き想定サイズの RoomBrowser 収まり確認
- [ ] 2. Android 実機で RoomBrowser の文字サイズと操作サイズの最終確認
- [ ] 3. 2クライアントで一覧参加と同期確認
- [ ] 4. 仕上げのドキュメント整合と不要メモ削除

## 完了済み
- [x] Relay + Lobby 導線へ移行し、JoinCode 手入力を不要化
- [x] RoomBrowser を UI Toolkit 構成で追加
- [x] RoomBrowser を Android 専用・横向き前提へ整理
- [x] RoomBrowser を `参加 / 作成` タブ切替へ整理
- [x] PanelSettings を実行時上書きで使う構成へ整理
- [x] RoomBrowser の一覧更新に 429 対策を追加
- [x] 今回の変更は `git staging` で確認できる状態まで反映
