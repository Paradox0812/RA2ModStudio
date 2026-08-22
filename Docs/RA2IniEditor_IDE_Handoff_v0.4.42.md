# RA2IniEditor IDE Handoff v0.4.42

## 目标

v0.4.42 实现 `Completion Commit Preview in Edit Mode`。Completion dropdown 仍可在只读预览中打开，但只有用户显式进入编辑模式后，Enter / Tab / 双击候选才会把候选提交到 IDE internal editable session，并同步回 AvalonEdit 当前文本。

## Commit 流程

```text
Ra2CompletionResult + selected Ra2CompletionItem
  -> Ra2CompletionCommitPlanner
  -> Ra2TextChange
  -> Ra2TextChangeApplier
  -> Ra2EditableDocumentSession
  -> SourceTextEditor.Document.Text
  -> caret offset at inserted text end
```

提交成功后：

- internal session 更新为新的 text-first model；
- editor state 显示 `Modified in Memory`；
- completion dropdown 关闭；
- caret 移动到插入文本末尾。

## Read-only 行为

只读预览状态下：

- `Ctrl+Space` 可以打开 completion dropdown；
- Up / Down 可以选择候选；
- Enter / Tab / 双击不会写入文本；
- dropdown 会关闭；
- editor state 保持 `Read-only Preview`。

## 明确未实现

- Save / Save All / Ctrl+S。
- ProjectSaveService / legacy save facade。
- backup / rollback 写盘。
- 多文件 dirty 管理。
- AvalonEdit `CompletionWindow`。
- 自动弹出 completion。
- Completion commit 的自定义 Undo / Redo 策略。

## 风险记录

本轮用程序同步 `SourceTextEditor.Document.Text` 来应用 completion commit，并用 `_isSynchronizingEditorText` 避免二次触发 TextChanged 更新。AvalonEdit 自身 undo stack 对程序设值的记录策略本轮不处理，后续如果要做真实编辑体验，需要单独设计 undo/redo。

## 验证重点

1. Read-only preview 下 Enter / Tab / 双击 completion 不修改文本。
2. Edit mode 下 Enter / Tab 可提交当前候选。
3. Edit mode 下双击候选可提交。
4. ReplacementSpan 覆盖 prefix，避免 `12120mm` 这类重复插入。
5. Commit 后状态为 `Modified in Memory`。
6. Revert 仍恢复 original text。
7. 不写磁盘，不调用保存链路。
