# RA2IniEditor IDE Handoff v0.4.41

## 目标

v0.4.41 将 v0.4.40 的 Source Editor 内存编辑状态收束到 IDE internal ViewModel，并新增 text-first save boundary contract。此版本仍不是保存功能，不写磁盘、不接 `ProjectSaveService`、不接 legacy save facade。

## Editor State ViewModel

新增 `Ra2EditorStateViewModel` 和 `Ra2EditorStateViewModelFactory`：

- `ReadOnlyPreview` 显示为 `Read-only Preview`。
- `EditableClean` 显示为 `Editing`。
- `EditableDirty` 显示为 `Modified in Memory`。
- `CanEnterEditMode`、`CanRevertInMemoryChanges`、`CanSavePreview` 均由 ViewModel 派生。
- `SaveHintText` 只提示状态，dirty 时显示 `Modified in memory only. Save is not implemented yet.`。

`ShellWindow` 只通过 factory 生成状态对象，再把 `StateText` / `SaveHintText` 写入现有控件。状态规则不再散落在多个事件处理器里。

## Text-first Save Boundary

新增 `Ra2EditorSavePlan` 和 `Ra2EditorSavePlanBuilder`。它只构建保存预览计划：

```text
Ra2EditableDocumentSession.DocumentState.CurrentText
  -> Ra2EditableDocumentSession.TextDocument.NewLineKind
  -> Ra2EditorSavePlan
```

规则：

- `EditableDirty` 且有 `FilePath` 时，`CanSave = true`。
- `EditableClean`、`ReadOnlyPreview` 或空 `FilePath` 时，`CanSave = false`。
- `Text` 必须来自 `CurrentText`。
- `NewLineKind` 必须来自 `Ra2IniTextDocument.NewLineKind`。
- 不访问磁盘，不调用保存服务，不创建备份。

## 本轮明确未实现

- Save / Save All / Ctrl+S。
- ProjectSaveService / ProjectLoader / legacy save facade。
- backup / rollback 写盘。
- Completion commit。
- 多文件 dirty 管理。
- 全项目索引或 ObjectAggregator。
- Core / Infrastructure public API 变更。

## 后续保存链路待解决

- encoding metadata 从当前 workspace snapshot、file store 还是 editor session 持有。
- mixed newline 是否保留为 per-line 策略，或保存时使用 file-level newline。
- backup / rollback 如何衔接现有 Infrastructure 与 legacy 项目保存语义。
- 保存失败后 session dirty 是否保留。
- 外部文件修改冲突如何提示。
- Save All 的部分失败如何表达。

## 验证重点

1. 打开文件后状态为 `Read-only Preview`。
2. 点击 `Enter Edit Mode` 后状态为 `Editing`。
3. 修改文本后状态为 `Modified in Memory`，并显示 save-not-implemented hint。
4. `Revert In-memory Changes` 后恢复原文并回到只读预览。
5. 没有任何磁盘写入。
6. Completion dropdown 仍不提交文本。
