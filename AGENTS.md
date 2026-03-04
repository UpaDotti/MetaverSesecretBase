## 0. ルール（要点）
- 返答：日本語 / 箇条書き / 難語は短く説明 / 不足時のみ質問1つ
- 更新順：SPEC → TASKLIST → PROGRESS →（必要なら）FAILURE_LOG
- 実装：TASKLISTを上から。変更は小さく、都度確認
- 完了後：TASKLIST更新（[ ]→[x]）/ PROGRESS更新 / 失敗はログ
- 設計：SOLID / 依存は外→内 / コメントは「なぜ」

## 1. ドキュメント入口
- 仕様：./.codex/SPEC.md
- タスク：./.codex/TASKLIST.md
- 進捗：./.codex/PROGRESS.md
- 失敗：./.codex/FAILURE_LOG.md/