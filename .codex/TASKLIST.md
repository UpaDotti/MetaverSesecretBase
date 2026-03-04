## 運用ルール
- 上限：200行を超えたら圧縮
- 圧縮：細かい手順は ./ .codex/tasks/ に逃がし、ここは「実行順の目次」に戻す
- 実装はこの順で上から進める（小さく変更→確認）

## 実行順（目次）
- [ ] 0. セットアップ
- [ ] 1. 最小動作（UI遷移の単体起動確認）
- [ ] 2. 名前入力フロー確認（InputNameState）
- [ ] 3. キャラ選択フロー確認（SelectCharacterState）
- [ ] 4. Host/Client開始確認（SelectNetworkState）
- [ ] 5. 同期と移動確認（NetworkPlayer/PlayerMoveController）
- [ ] 6. テスト/確認（2クライアントで名前・キャラ同期）
- [ ] 7. 仕上げ（ログ整理とドキュメント整合）

## メモ（必要ならリンク）
- 詳細：./.codex/tasks/（必要になったら作る）
