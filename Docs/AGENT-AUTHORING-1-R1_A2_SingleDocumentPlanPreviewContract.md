# AGENT-AUTHORING-1-R1 A2 单文档结构化计划与预览最终契约

状态：最终、用户已确认连续实施  
日期：2026-07-28  
包级风险：R3  
治理模式：Continuous StagePackage / Deferred Governance  
前置阶段：A1 与 SEARCH-1-R1 已完成

## 1. 目标

A2 建立 UI 无关、无磁盘写入的单文档结构化编辑计划与预览能力：

```text
editable session + editor text + registry snapshot
  -> Ra2AuthoringSnapshot
  -> Ra2IniEditPlan
  -> UpsertField / ReplaceFieldValue
  -> Ra2TextChangeSet
  -> current/candidate A1 analysis
  -> immutable Ra2IniEditPreview
```

首版支持一个 Plan 中包含多个字段操作。所有操作针对同一原始 Snapshot
解析，不采用逐项修改后再解析的隐式顺序语义。

## 2. 已有事实与复用路径

- `Ra2EditableDocumentSession` 已拥有 `DocumentId`、`EditRevision` 和文本变更生命周期。
- `Ra2FieldRegistryProviderSnapshot` 已绑定 Provider 与单调递增 Revision。
- `IRa2IniLanguageAnalysisService` 已提供 UI 无关的当前文档只读分析。
- `Ra2TextChangeSet` 已提供原文坐标、稳定排序、重叠拒绝和倒序应用。
- `Ra2AddPropertyInsertPlanner` 已提供当前 TextModel 的换行选择与行后插入行为。
- Search Replace Plan 是 Search 专用契约，不泛化为 Authoring Plan。
- `Ra2TextChangeApplier`、Shell 程序化 Undo 和保存链路属于 A3 或以后阶段。

## 3. 非目标

- 不 Apply，不改变编辑会话、AvalonEdit 或脏状态。
- 不实现 Preview Store、Undo/Redo、保存、备份或磁盘写入。
- 不接入 AI JSON、工具调用、聊天 UI 或外部 Agent。
- 不支持 `InsertSection`、`RemoveField`、`InsertRawText`。
- 不支持项目级或跨文件事务。
- 不修改 parser、diagnostics、Completion、Field Registry 优先级或 BuiltIn 数据。
- 不修改 Shell、Dock、XAML、ViewModel 或 AutomationId。
- 不偿还 `AGENT-AUTHORING-A1-TD-001`，除非 A2 性能证据触发单独契约。

## 4. 数据契约

### 4.1 `Ra2AuthoringSnapshot`

不可变、internal，由当前编辑会话、编辑器文本、项目根目录和 Registry Snapshot 捕获。

字段：

- `DocumentId`
- `EditRevision`
- `ProjectRootPath`
- `FilePath`
- `Text`
- `IsEditable`
- `IsDirty`
- `FieldRegistry`

捕获失败必须返回显式 `Ra2AuthoringSnapshotCaptureResult`，不得依靠运行时异常表达
Session/Editor 不一致。

失败类型：

```text
None
NoEditableSession
ReadOnly
EditorSessionTextMismatch
InvalidDocumentIdentity
InvalidRegistrySnapshot
UnexpectedFailure
```

### 4.2 `Ra2IniEditOperation`

使用一个封闭值对象和 `Ra2IniEditOperationKind`：

```text
UpsertField
ReplaceFieldValue
```

字段：`Kind`、`SectionName`、`Key`、`Value`。

约束：

- Section/Key Trim 后非空。
- Section 不得包含 CR、LF、`[`、`]` 或 NUL。
- Key 不得包含 CR、LF、`=` 或 NUL。
- Value 允许空字符串，禁止 CR、LF 和 NUL。
- Section/Key 使用 `OrdinalIgnoreCase` 定位；原文大小写不变。

### 4.3 `Ra2IniEditPlan`

字段：

- `PlanId`
- `ExpectedDocumentId`
- `ExpectedEditRevision`
- `ExpectedFieldRegistryRevision`
- `Operations`
- `Summary`
- `Origin`

约束：

- 标识非空、Revision 合法。
- Operations 1..128，防御性复制。
- Summary/Origin 非空、长度受限、禁止 NUL 和换行。
- Summary/Origin 是不可信展示与审计文本，不参与风险或确认策略。
- 调用方不能提交 `RequiresExplicitConfirmation=false`。

### 4.4 `Ra2IniEditOperationPreview`

每项成功操作必须记录：

- `OperationIndex`
- `Operation`
- `OutcomeKind`：`Inserted` / `Replaced`
- `ResolvedSectionKind`
- `IsKnownField`
- `FieldTrustLevel`
- `AffectedOriginalSpan`
- `Summary`

未知、inferred、guardrail、obsolete、non-existent 和 pseudo-field 不在 A2
硬拒绝，但必须成为 Preview 证据。

### 4.5 `Ra2IniEditPreview`

成功结果：

- IDE 生成的非空 `PreviewId`
- Snapshot、Plan
- 原文坐标 `Ra2TextChangeSet`
- 完整 CandidateText
- 有序 OperationPreviews
- Current/Candidate A1 Analysis
- Added/Removed diagnostics
- 新增 Error/Warning 计数
- `RequiresExplicitConfirmation = true`

失败结果必须无 CandidateText、ChangeSet 和可应用证据，并返回安全固定摘要。

失败类型：

```text
None
InvalidPlan
StalePlanTarget
ReadOnly
UnsupportedOperation
InvalidSection
SectionNotFound
AmbiguousSection
FieldNotFound
AmbiguousField
ConflictingOperations
OverlappingChanges
NoChanges
Canceled
CurrentAnalysisFailed
CandidateAnalysisFailed
UnexpectedFailure
```

## 5. 规划规则

1. Plan 的 DocumentId/EditRevision/RegistryRevision 必须等于 Snapshot。
2. Current 与 Candidate 分析使用同一个捕获 Provider Snapshot。
3. Section 和 Key 均以 `OrdinalIgnoreCase` 查找。
4. 重名 Section、重复 Key 和同一逻辑字段的多操作均拒绝。
5. `ReplaceFieldValue` 要求字段唯一存在。
6. `UpsertField` 对唯一现有字段替换 Value Span；缺失时插入。
7. 非空 Value 只替换 Value Span，保留 Key、等号空白和行尾注释。
8. 空现值通过等号后的零长度 Span 插入。
9. 缺失字段使用目标 Section 的最后 KeyValue 行作为锚点；无 KeyValue 时使用 Header。
10. 复用 `Ra2AddPropertyInsertPlanner` 的行后插入文本，但重新包装 Authoring Reason。
11. 多个字段落在同一插入点时按 Plan 顺序合并为一个 Change。
12. 所有 Change 基于原文坐标；不得顺序修改后重新定位。
13. 未知字段允许进入 Candidate，由 Field Trust 和诊断证据提示。
14. Planner 接受 `CancellationToken`，在当前分析、规划、候选分析之间检查。
15. 不把原始异常文本暴露为 Preview FailureMessage。

## 6. 诊断差异

采用多重集合差异。诊断指纹包含：

```text
Code + SourceKind + Severity + Message + SectionId + Key
```

排除：

```text
LineNumber + ColumnNumber + AnalysisVersion
```

因此纯行号移动不会制造新增问题；重复诊断按出现次数消费。Added 使用 Candidate
中的实际位置和顺序，Removed 使用 Current 中的实际位置和顺序。

必须通过以下特征测试：

- 仅插入一行导致后续诊断移动。
- 同一字段的引用目标改变导致旧问题移除、新问题新增。
- 同指纹诊断重复出现。

## 7. Preview 所有权与 A3 边界

- A2 Preview 不提供 Apply 方法。
- PreviewId 由 IDE 生成，Preview 仍为 internal。
- A3 必须由 Authoring Workspace 保存 Preview，并只接受 PreviewId。
- A3 不得接受调用方构造或提交的任意 Preview 实例。
- PreviewId 单次消费。
- 文档切换、用户编辑、编辑器/Session 文本分歧或 Registry Reload 均使其失效。
- A2 只提供纯 Currency Evaluator，不实现 Store 或消费。

Currency 依次检查：

```text
Preview succeeded
Current session exists and is editable
DocumentId
EditRevision
Session text
Editor text
Registry revision
```

## 8. 连续任务卡

### A2-P0 PreChangeRollbackAndExactInventory

- 创建 IdeOnly 改动前回滚包。
- 写入本最终契约与 Exact API Inventory。

### A2-A SnapshotCaptureAndFailureContract

- Snapshot、捕获结果与失败类型。
- Session/editor/registry 一致性测试。

### A2-B StructuredOperationAndPlanContract

- Operation、Plan 与输入不变量。
- 防御性复制、操作上限和不可信显示文本测试。

### A2-C PreviewOperationEvidenceAndDiagnosticDelta

- Preview、操作证据、失败类型。
- 诊断多重集合差异和 Field Trust 证据测试。

### A2-D SingleDocumentDeterministicPlanner

- Planner interface/implementation。
- 结构定位、多操作冲突、同点插入合并、换行/注释保持和双分析。

### A2-E PreviewCurrencyAndOwnershipBoundary

- Currency result/evaluator。
- 双文本、双 Revision、身份和失败 Preview 门禁。

### A2-F CancellationPerformanceAndBoundaryGate

- Cancellation phase gate。
- 1 MiB/4 MiB/接近 8 MiB 的记录性性能测试或测量。
- 无 WPF/Shell/Writer/AI/磁盘依赖的源码边界测试。

### A2-G PackageVerificationAndGovernanceFlush

- 定向测试、Build、完整非 UI 测试、IdeOnly clean package。
- Stage Ledger、CurrentPhase、Full Context、Context Capsule。

## 9. 文件预算

每卡最多修改 5 个文件、最多新增 2 个生产类。不移动文件、不新增依赖、不修改
目录结构。生产类型放入现有 `RA2IniEditor.IDE/Editing`。

## 10. Exact API Inventory

DeepSeek 或任何局部实现只能使用以下真实契约，不得猜测便利 API。

```csharp
internal sealed class Ra2EditableDocumentSession
{
    public Ra2EditableDocumentState DocumentState { get; }
    public Ra2IniTextDocument TextDocument { get; }
    public Guid DocumentId { get; }
    public int EditRevision { get; }
}

internal sealed class Ra2FieldRegistryProviderSnapshot
{
    public IRa2FieldDefinitionProvider Provider { get; }
    public long Revision { get; }
}

internal interface IRa2IniLanguageAnalysisService
{
    Ra2IniLanguageAnalysisResult Analyze(Ra2LanguageAnalysisRequest request);
}

internal sealed class Ra2LanguageAnalysisRequest
{
    internal Ra2LanguageAnalysisRequest(
        string projectRootPath,
        string filePath,
        string fileName,
        string text,
        int analysisVersion,
        Ra2FieldRegistryProviderSnapshot fieldRegistry);
}

internal sealed class Ra2IniLanguageAnalysisResult
{
    public bool Succeeded { get; }
    public Ra2IniTextDocument? TextDocument { get; }
    public Ra2DocumentSemanticModel? SemanticModel { get; }
    public IReadOnlyList<Ra2DiagnosticFact> Diagnostics { get; }
    public long FieldRegistryRevision { get; }
}

internal sealed class Ra2TextChange
{
    public Ra2TextChange(Ra2TextSpan span, string newText, string reason);
    public Ra2TextSpan Span { get; }
    public string NewText { get; }
}

internal sealed class Ra2TextChangeSet
{
    public Ra2TextChangeSet(IEnumerable<Ra2TextChange> changes);
    public IReadOnlyList<Ra2TextChange> Changes { get; }
    public string Apply(string sourceText);
}

internal sealed class Ra2AddPropertyInsertPlanner
{
    public Ra2AddPropertyInsertPlan PlanInsert(
        Ra2IniTextDocument document,
        int caretOffset,
        string option,
        string? value);
}

public interface IRa2FieldDefinitionProvider
{
    bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition);
    IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind);
    bool IsKnownField(Ra2SectionKind sectionKind, string key);
}
```

禁止猜测：

- Session `TryGetValue`、`CurrentState`、`Version` 或可写 Revision。
- Preview Store、Apply、Undo group 或 Writer API。
- Field Registry 任意写入、Reload 或 provenance API。
- TextModel 任意 mutation API。
- 未列出的 fake、builder 或测试夹具。

## 11. 验证与停止条件

每卡必须通过定向测试且 diff 未越界。生产代码卡后至少执行 Debug build。包末执行：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

以下任一发生即停止：

- 需要修改 Shell/UI/AI/Save/parser/diagnostics 实现。
- 需要 public API、依赖或项目文件变化。
- A1、A0、Search Session identity/revision 回归失败。
- Planner 无法在原文坐标下确定性表达。
- Preview 所有权只能通过调用方持有可应用对象实现。
- 必需测试失败且修复超出当前卡。

## 12. A2-P0 回滚锚点

```text
Path: artifacts/RA2IniEditor.IDE.SourceClean.AGENT-AUTHORING-A2.PreChange.Rollback.zip
Entries: 1003
Bytes: 10,577,142
SHA256: D2D620F396734BB3E4BAA88F4EF86E607BB00A7CCD7FA35E209E9930EFA8007C
Forbidden entries: 0（由 IdeOnly package gate 验证）
```

## 13. 自审

- Architecture：通过；A2 只规划和预览。
- Reuse：通过；不复制 parser、diagnostics、Session 或 ChangeSet。
- Data ownership：通过；Snapshot/Plan/Preview 权限单向。
- Public API：无；全部 internal/Experimental。
- Determinism：原文坐标、冲突拒绝、同点合并和稳定诊断差异。
- Safety：未知字段可见但不硬阻止；所有 Preview 需要显式确认。
- Evolvability：A3 可增加 workspace-owned Preview Store 和事务端口，无需替换 A2。
- R4：未触发。

## 14. 执行结果

状态：2026-07-28 实现与自动化验证完成。

- A2-A 至 A2-E 已按本契约落地；新增类型全部保持 `internal`。
- A2-F 已覆盖边界、取消、1/4/7 MiB 记录型性能测试以及源文本不变性。
- A2 不包含 Preview Store、Apply、Undo/Redo、Save、磁盘写入、AI 或 UI 接线。
- IDE-only Debug 构建通过，0 warnings / 0 errors。
- A2 与复用边界相关回归通过 104/104。
- 全量非 UI 测试通过 2419/2419。
- 详细执行证据见 `Docs/AGENT-AUTHORING-1-R1_A2_StageLedger.md`。
- 当前交接入口见 `Docs/ContextCapsule_AGENT_AUTHORING_1_A2.md`。
