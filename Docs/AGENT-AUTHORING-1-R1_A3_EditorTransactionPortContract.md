# AGENT-AUTHORING-1-R1 A3 编辑器事务端口最终契约

状态：最终、用户已授权连续实施；A3 完成后停止  
日期：2026-07-28  
风险：R3  
治理模式：Continuous StagePackage / Deferred Governance  
前置阶段：A2 已完成

## 1. 目标

A3 建立单文档 Authoring Preview 的唯一应用路径：

```text
Ra2IniEditPlan
  -> IRa2IniAuthoringWorkspace.Preview
  -> workspace-owned active Preview
  -> PreviewId + explicit confirmation
  -> Shell-owned IRa2EditorTransactionPort
  -> live currency recheck
  -> one Session update + one editor sync + one semantic Undo unit
```

成功 Apply 只修改当前内存编辑会话和 AvalonEdit 文本，不保存文件。

## 2. 非目标

- 不接入 AI、JSON、工具调用或聊天 UI。
- 不新增 Preview/Diff/确认 UI。
- 不修改 Shell XAML、Dock、布局、AutomationId 或视觉样式。
- 不自动保存，不调用 Save、Preflight、Backup、Rollback 或 Writer。
- 不支持多文件事务、外部 Agent、IPC/MCP 或 Preview 持久化。
- 不重构 parser、diagnostics、Completion、Search 或 Field Registry 语义。
- 不把现有单状态程序化 Undo 扩展为多级事务栈。

## 3. 所有权

| 数据 | 唯一所有者 | 生命周期 |
|---|---|---|
| 活动 Preview | `Ra2IniAuthoringWorkspace` | 新 Preview、失效或确认 Apply |
| Preview generation | Workspace | 单进程、非持久化 |
| 实时 Session/Editor/Registry/Caret | Shell-owned Transaction Port | 提交瞬间 |
| 更新后 Session | Shell | 直到下一次编辑、保存、恢复或切换 |
| 语义 Undo 单元 | Shell | 当前程序化语义操作生命周期 |

调用方只能向 Workspace 提交 PreviewId 和显式确认，不得提供 Session、编辑器文本、
Registry Revision 或 Preview 实例。

## 4. Apply 契约

```csharp
internal enum Ra2IniEditApplyOutcomeKind
{
    Applied = 0,
    PreviewUnavailable,
    ConfirmationRequired,
    StalePreview,
    TransactionRejected,
    UnexpectedFailure
}

internal sealed class Ra2IniEditApplyRequest
{
    public Guid PreviewId { get; }
    public bool ExplicitConfirmationGranted { get; }
}

internal sealed class Ra2IniEditApplyResult
{
    public bool Succeeded { get; }
    public Ra2IniEditApplyOutcomeKind OutcomeKind { get; }
    public Ra2IniEditPreviewCurrencyKind CurrencyKind { get; }
    public Guid PreviewId { get; }
    public Ra2EditableDocumentSession? UpdatedSession { get; }
    public string? TextToSyncToEditor { get; }
    public string? UndoText { get; }
    public string? RedoText { get; }
    public int? UndoCaretOffset { get; }
    public int? RedoCaretOffset { get; }
    public int OperationCount { get; }
    public bool IsDirtyAfterApply { get; }
    public string Message { get; }
}
```

成功结果必须携带完整同步和 Undo/Redo 证据；失败结果不得携带任何可提交证据。

## 5. Workspace

```csharp
internal interface IRa2IniAuthoringWorkspace
{
    Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        CancellationToken cancellationToken = default);

    Ra2IniEditApplyResult Apply(Ra2IniEditApplyRequest request);

    void InvalidateActivePreview();
}
```

规则：

1. Workspace 使用单槽位，不使用 Preview 字典。
2. 每次 Preview 开始或主动失效均递增内部 generation。
3. Preview 完成时只有捕获 generation 仍为当前 generation 才可存储。
4. 失败、取消和过时代次结果不得成为活动 Preview。
5. 未确认 Apply 不消费 Preview。
6. 确认 Apply 在锁内 claim 并移除 Preview，然后才调用事务端口。
7. 确认后的匹配 Apply 无论成功、stale 或失败均不可重放。
8. Apply 结束后再次递增 generation，清除事务期间启动的旧上下文 Preview。
9. 并发 Apply 同一 PreviewId 最多一个能进入事务端口。

## 6. 编辑器事务端口

```csharp
internal interface IRa2EditorTransactionPort
{
    Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview);
}
```

生产实现由 Shell 私有适配器提供。事务端口必须在实际提交瞬间捕获：

- `_editableSession`
- `SourceTextEditor.Document.Text`
- `FieldRegistryRuntimeService.CaptureProviderSnapshot().Revision`
- 当前 Caret

随后调用现有 `Ra2IniEditPreviewCurrencyEvaluator`，不得信任调用方提供的实时状态。

## 7. Session 程序化更新

现有 `IRa2EditorSessionController` 增加：

```csharp
Ra2EditorSessionOperationResult ApplyProgrammaticText(
    Ra2EditorSessionApplyProgrammaticTextRequest request);
```

请求绑定：

- Session
- ExpectedDocumentId
- ExpectedEditRevision
- ExpectedCurrentText
- CandidateText
- RequestedCaretOffset

Controller 必须再次检查身份、修订、只读、原文和 no-op，只调用
`IRa2EditableDocumentSessionService.UpdateText` 一次，并验证返回 Session：

- DocumentId 不变；
- EditRevision 恰好递增一次；
- CurrentText 等于 CandidateText。

成功复用现有 `Ra2EditorSessionOperationResult.AppliedProgrammaticText`。

## 8. Shell 原子提交顺序

1. 捕获实时编辑器状态。
2. currency 复检。
3. 生成更新后 Session，但不立即发布到 `_editableSession`。
4. 生成一个 `ProgrammaticSemanticUndoState`。
5. 在 `_isSynchronizingEditorText` 保护下同步 CandidateText 和 Caret。
6. 清理 AvalonEdit 的碎片化 Undo。
7. 发布新 Session 和语义 Undo 状态。
8. 刷新命令与编辑器状态。
9. 返回 `Applied`。

若编辑器同步异常：

- 尝试恢复原文和原 Caret；
- 保留旧 Session 和旧语义 Undo 状态；
- 恢复失败时退回只读状态，避免 Session/Editor 不一致进入保存链；
- 返回安全固定的 `UnexpectedFailure`，不暴露原始异常文本。

## 9. Preview 失效接点

- 用户文本变化。
- 任意程序化编辑器文本同步。
- 进入或离开 Editable Session。
- Field Registry Reload。
- 新 Preview 请求。
- 已确认 Apply。

## 10. Caret 与 Undo

```text
UndoCaretOffset = clamp(CurrentCaretOffset, 0, OriginalText.Length)
RedoCaretOffset = clamp(CurrentCaretOffset, 0, CandidateText.Length)
```

一个 A3 Apply 对应一个完整语义 Undo/Redo 单元。A3 保留现有单状态模型，不承诺
连续多个程序化事务的多级历史。

## 11. 允许修改

生产代码：

- `RA2IniEditor.IDE/Editing/Ra2IniEditApplyResult.cs`（新增）
- `RA2IniEditor.IDE/Editing/IRa2EditorTransactionPort.cs`（新增）
- `RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs`（新增）
- `RA2IniEditor.IDE/Controllers/EditorSession/Ra2EditorSessionController.cs`
- `RA2IniEditor.IDE/Controllers/EditorSession/Ra2EditorSessionOperationModels.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`（仅事务适配与失效接点）

对应测试及包末治理文档允许新增或更新。每张任务卡最多修改 5 个文件。

## 12. 禁止修改

- `ShellWindow.xaml` 和全部其他 XAML。
- AI、Search 实现与行为。
- Save、Writer、Backup、Rollback。
- parser、diagnostics、Completion、Field Registry 实现与 BuiltIn 数据。
- 项目文件、依赖、目录结构和 legacy。
- 所有 AutomationId。

## 13. 连续任务卡

### A3-P0

修正版最终契约、Exact API Inventory 和改动前 IdeOnly 回滚包。

### A3-A

Apply Request/Result、Workspace 单槽所有权、generation 和并发单次消费。

### A3-B

Session Controller 程序化更新及真实 Session/Revision/Dirty State 测试。

### A3-C

Shell-owned Transaction Port、实时 currency、原子同步、语义 Undo 和失效接点。

### A3-D

并发 Apply、过时代次、同步异常补偿、无 UI/AI/Search/Save/Writer 依赖边界测试。

### A3-E

定向测试、IDE-only build、完整非 UI 测试、clean package、差异审计和治理 flush。

## 14. 验证

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

UIA 不作为 A3 必需门禁，因为 A3 不增加用户入口或控件。

## 15. 回滚锚点

```text
Path: artifacts/RA2IniEditor.IDE.SourceClean.AGENT-AUTHORING-A3.PreChange.Rollback.zip
Entries: 1019
Bytes: 10,609,281
SHA256: 8EC863DEEE4B07F91D207748518436A4E17A8ACDE5DD53F566922DF57BC3B29C
Forbidden entries: 0
```

## 16. 批准与停止

用户已明确授权完成 A3，并要求完成后停止。A3 不得自动进入 A4。

## 17. 执行结果

状态：Completed

- Workspace 单槽、generation、显式确认和一次性消费已实现。
- Session Controller 程序化文本事务已实现，保持 DocumentId 且修订恰好递增一次。
- Shell-owned 私有事务端口、实时 currency 复检、语义 Undo、补偿与失效接点已实现。
- `ShellWindow.xaml`、全部 AutomationId、Dock/布局与用户入口未修改。
- A3 定向及受影响边界测试 23/23；完整非 UI 测试 2436/2436。
- IDE-only Debug build 0 warnings / 0 errors。
- 详细证据见 `Docs/AGENT-AUTHORING-1-R1_A3_StageLedger.md`。
- 按用户要求在 A3 停止，A4 未开始。
