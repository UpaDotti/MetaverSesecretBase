## 運用ルール
- 上限：80行を超えたら圧縮
- 圧縮：Doneは「直近だけ」にして古い行は消す（重要ならSPEC側に残す）
- Nextは最大3つまで（迷子防止）

## 状態
- 進捗：35%
- Doing：Relay接続の動作確認
- Next：
  1. Host起動時のJoin Code発行確認
  2. Clientの自動Join（環境変数/設定アセット）確認
  3. 2クライアント同期確認
- Done（直近）：RelayConnectionService追加とSelectNetworkStateのRelay化を実装
- Done（直近）：Relay依存の版不整合はユーザー手動対応で解決、失敗ログを事実ベースへ修正
- Done（直近）：メソッド直上コメントを `/// <summary>` 形式へ統一し、関連箇所に反映
- Done（直近）：CLASS_DIAGRAM.mdを追加し、AGENTS/SPECの入口リンクを更新
- Done（直近）：CLASS_DIAGRAM.mdを全クラス列挙・メソッド省略の見やすい構成へ更新
- Blocked（あれば）：
