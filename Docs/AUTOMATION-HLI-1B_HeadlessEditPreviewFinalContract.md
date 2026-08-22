# AUTOMATION-HLI-1B Headless Edit Preview Final Contract

契约日期：2026-08-22  
状态：Completed / Verified
前置基线：AUTOMATION-HLI-1A2 Completed / Verified  
事实依据：`Docs/AUTOMATION-HLI-1B_EditPreviewCodeFactAudit.md`

## 1. 目标

建立首个可由 `net8.0` Agent/Gateway 调用的无界面结构化编辑预览能力：

```text
ini.document.edit.preview
```

```text
explicit document snapshot + bounded edit plan
  -> Application semantic preview engine
  -> candidate text + ordered non-overlapping changes
  -> operation evidence + current/candidate diagnostic delta
  -> immutable result
```

它只回答“如果执行这些受限操作，文档会变成什么、风险是什么”。它不修改编辑器、
Session、Undo、脏状态或磁盘。

## 2. 风险与授权门

```text
Implementation risk: R3 assembly/authority migration
Public contract risk: R2 Experimental API expansion
Persistence/wire risk: None
UI risk: None; no XAML or AutomationId change
Shell risk: None; ShellWindow.xaml/.xaml.cs both forbidden
Governance mode: Deferred during continuous cards; flush at HLI-1B stop
```

用户已明确确认本最终契约；HLI-1B-0..1B-6 已按本文连续实施并完成验证。

## 3. 非目标

HLI-1B 不实现：

- Apply、Undo/Redo、Save、Preflight、Backup、Rollback 或 Writer public API；
- active Preview store、generation、claim、proposal handle 或跨调用 session；
- generic text patch、raw text insertion、Section create/delete、field delete；
- 多文件计划、项目级事务或跨文件引用修复；
- AI JSON、provider、聊天 UI、Gateway registry、CLI、MCP、IPC 或 wire DTO；
- 文件读取/枚举、环境变量、API key、进程启动或网络；
- Parser/Diagnostics/Field Registry/Completion 行为重写；
- A4 policy、建议卡视觉、Shell/Dock/XAML/AutomationId 修改；
- 多级程序化 Undo 或自动保存。

## 4. 架构与唯一权威

### 4.1 目标依赖方向

```text
Core
  ^
Application (net8.0, Core-only)
  - internal TextModel/change/insertion/semantic preview engine
  - Experimental public Preview contract
  ^
IDE Host
  - live snapshot capture
  - thin Preview compatibility adapter
  - A3 active slot/currency/apply/undo
  - A4 policy/presentation
```

Application 不得引用 IDE、Infrastructure、WPF、AvalonEdit、Dispatcher、ViewModel、IO
或 runtime singleton。

### 4.2 单一算法原则

- A2 semantic planner 迁入 Application 后，IDE 旧文件不得保留第二套 planner。
- 6 个 TextModel 和 2 个 TextChange 文件原子迁移，旧路径删除。
- IDE 旧 TextModel namespace 只允许保留一个无算法 compatibility marker，以兼容 15 个
  production/43 个 test 的显式 using；真实类型全部来自 Application global using。
- line-after-anchor 插入格式抽为一个 Application internal primitive；IDE AddProperty
  planner 和 headless Preview 都调用它。
- Diagnostics 与 FieldTrust 继续复用 HLI-1A2 唯一实现。
- IDE compatibility 层只投影输入、输出和本地化 message，不重新解析或规划文本。

## 5. 数据所有权

| 数据 | 所有者 | 生命周期 | 可序列化性 |
|---|---|---|---|
| live session/editor/registry | IDE Host | 捕获瞬间 | 否 |
| `Ra2AutomationDocumentSnapshot` | caller | 单次 invocation，只读 | 不是 wire DTO |
| Edit Plan | caller -> Application | 单次 invocation，immutable | 不是 wire DTO |
| SemanticModel/TextModel/diagnostic facts | Application internal | invocation-local | 否 |
| Preview result | caller | immutable；Application 不留 active state | 不是 wire DTO |
| active Preview + generation | IDE A3 workspace | 当前编辑上下文 | 否 |
| Apply/Undo/dirty/save state | IDE Host | 编辑会话 | 否 |

`PreviewId` 由成功 invocation 生成，但 Application 不注册、不持有也不消费它。

## 6. 精确 public API

命名空间：

```csharp
RA2IniEditor.Application.Automation.Experimental
```

### 6.1 Service

```csharp
public interface IRa2AutomationEditPreviewService
{
    Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default);
}

public sealed class Ra2AutomationEditPreviewService
    : IRa2AutomationEditPreviewService
{
    public const int MaximumDocumentCharacters = 8_388_608;
    public const int MaximumDiagnosticItems = 10_000;

    public Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default);
}
```

Service 无跨调用可变状态、无 cache、无 active preview。并发调用之间不得共享派生模型。

### 6.2 Operation 与 Plan

```csharp
public enum Ra2AutomationEditOperationKind
{
    UpsertField = 0,
    ReplaceFieldValue = 1
}

public sealed class Ra2AutomationEditOperation
{
    public const int MaximumSectionNameLength = 256;
    public const int MaximumKeyLength = 256;
    public const int MaximumValueLength = 8192;

    public Ra2AutomationEditOperation(
        Ra2AutomationEditOperationKind kind,
        string sectionName,
        string key,
        string value);

    public Ra2AutomationEditOperationKind Kind { get; }
    public string SectionName { get; }
    public string Key { get; }
    public string Value { get; }
}

public sealed class Ra2AutomationEditPlan
{
    public const int MaximumOperationCount = 128;
    public const int MaximumSummaryLength = 512;
    public const int MaximumOriginLength = 128;

    public Ra2AutomationEditPlan(
        Guid planId,
        Guid expectedDocumentId,
        int expectedVersion,
        long expectedFieldRegistryRevision,
        IEnumerable<Ra2AutomationEditOperation> operations,
        string summary,
        string origin);

    public Guid PlanId { get; }
    public Guid ExpectedDocumentId { get; }
    public int ExpectedVersion { get; }
    public long ExpectedFieldRegistryRevision { get; }
    public IReadOnlyList<Ra2AutomationEditOperation> Operations { get; }
    public string Summary { get; }
    public string Origin { get; }
}
```

构造不变量：

- identity 非空，Version 非负，RegistryRevision 为正；
- Operations 1..128、defensive copy、无 null；
- Section/Key Trim 后非空；Section 禁止 CR/LF/NUL/`[`/`]`，Key 禁止 CR/LF/NUL/`=`；
- Value 保留原样，允许空字符串，禁止 CR/LF/NUL；
- 三项长度使用上面精确上限；
- Summary/Origin Trim 后非空、禁止 CR/LF/NUL并受长度限制；
- 构造器不接受或暴露 `RequiresExplicitConfirmation=false`。

### 6.3 Failure

```csharp
public enum Ra2AutomationEditPreviewFailureKind
{
    None = 0,
    InvalidPlan = 1,
    StalePlanTarget = 2,
    ReadOnly = 3,
    UnsupportedOperation = 4,
    InvalidSection = 5,
    SectionNotFound = 6,
    AmbiguousSection = 7,
    FieldNotFound = 8,
    AmbiguousField = 9,
    ConflictingOperations = 10,
    OverlappingChanges = 11,
    NoChanges = 12,
    Canceled = 13,
    CurrentAnalysisFailed = 14,
    CandidateAnalysisFailed = 15,
    UnexpectedFailure = 16,
    DocumentTooLarge = 17,
    ResultLimitExceeded = 18
}
```

前 0..16 保留现有 A2 顺序和可区分语义；17/18 只增加 public safety budget，不合并
stale、ambiguous、conflict、overlap、no-op 或 current/candidate analysis failure。

### 6.4 Evidence DTO

```csharp
public enum Ra2AutomationEditOperationOutcomeKind
{
    Inserted = 0,
    Replaced = 1
}

public enum Ra2AutomationFieldTrustLevel
{
    Verified = 0,
    VerifiedGuardrail = 1,
    Inferred = 2,
    ManualCurated = 3,
    AutoExtracted = 4,
    Obsolete = 5,
    NonExistent = 6,
    PseudoField = 7,
    Unknown = 8
}

public sealed class Ra2AutomationTextChange
{
    public Ra2AutomationTextSpan Span { get; }
    public string NewText { get; }
    public string Reason { get; }
}

public sealed class Ra2AutomationEditOperationPreview
{
    public int OperationIndex { get; }
    public Ra2AutomationEditOperation Operation { get; }
    public Ra2AutomationEditOperationOutcomeKind OutcomeKind { get; }
    public Ra2SectionKind ResolvedSectionKind { get; }
    public bool IsKnownField { get; }
    public Ra2AutomationFieldTrustLevel FieldTrustLevel { get; }
    public Ra2AutomationTextSpan AffectedOriginalSpan { get; }
    public string Summary { get; }
}
```

Evidence DTO 构造器保持 internal。Field Trust 必须逐值映射 HLI-1A2 classifier，不能从
显示文本、source name 或布尔值反推。

### 6.5 Result

```csharp
public sealed class Ra2AutomationEditPreviewResult
{
    public bool Succeeded { get; }
    public Ra2AutomationEditPreviewFailureKind FailureKind { get; }
    public string Message { get; }

    public Guid DocumentId { get; }
    public int Version { get; }
    public string FilePath { get; }
    public long FieldRegistryRevision { get; }
    public Guid PlanId { get; }

    public Guid PreviewId { get; }
    public string? CandidateText { get; }
    public IReadOnlyList<Ra2AutomationTextChange> Changes { get; }
    public IReadOnlyList<Ra2AutomationEditOperationPreview> OperationPreviews { get; }
    public IReadOnlyList<Ra2AutomationDiagnosticFact> AddedDiagnostics { get; }
    public IReadOnlyList<Ra2AutomationDiagnosticFact> RemovedDiagnostics { get; }
    public int AddedErrorCount { get; }
    public int AddedWarningCount { get; }
    public bool RequiresExplicitConfirmation { get; }
}
```

Result invariant：

- `Succeeded == (FailureKind == None)`；
- identity/version/revision/PlanId 始终来自输入；
- 成功时 PreviewId 非空、CandidateText 非 null、Changes 和 OperationPreviews 非空，且
  OperationPreviews 数量等于 Plan operations；
- 成功时 `RequiresExplicitConfirmation == true`；
- 失败时 PreviewId 为空、CandidateText 为 null、四个列表为空、计数为 0、
  `RequiresExplicitConfirmation == false`；
- 所有列表 defensive copy；失败不得携带 partial/applicable payload；
- Message 是安全文本，不含 raw exception、provider body 或敏感绝对路径。

### 6.6 精确 allowlist

Application exported allowlist 从 18 增加到精确 29。除本节 11 个类型外，不得新增
public type，不得公开 TextModel、SemanticModel、diagnostic core、FieldTrust classifier、
Host capture、Currency、Workspace、TransactionPort、Apply 或 Save。

## 7. 预览语义

### 7.1 前置门禁

1. Snapshot Text 超过 8,388,608 UTF-16 chars -> `DocumentTooLarge`。
2. `IsEditable == false` -> `ReadOnly`。
3. Plan DocumentId/Version/RegistryRevision 与 Snapshot 不同 -> `StalePlanTarget`。
4. 构造器编程错误可抛 `ArgumentException`；运行时业务失败返回 typed result。

若 CandidateText 因插入超过字符上限，同样返回 `DocumentTooLarge` 且无 candidate payload。

### 7.2 结构定位和规划

- Section/Key 用 `OrdinalIgnoreCase`；不改变原文标识符大小写。
- 重复 Section -> `AmbiguousSection`；重复目标 Key -> `AmbiguousField`。
- 同一逻辑 target 多次出现 -> `ConflictingOperations`。
- `ReplaceFieldValue` 缺失 -> `FieldNotFound`。
- `UpsertField` 存在时替换，缺失时插入。
- 所有操作相对同一原始 Snapshot 定位，不做逐项变异后重解析。
- 所有 change 使用原文坐标，排序稳定且不重叠；重叠 -> `OverlappingChanges`。
- no-op -> `NoChanges`。

### 7.3 格式保持

- 值替换只改 value span，保留 Key、等号空白、注释和 line ending。
- 现有空值在等号后的零长度 span 插入。
- 缺失字段以目标 Section 最后 KeyValue 行为锚点；无字段时使用完整 header 行。
- 沿用锚点行的 CRLF/LF/CR；末行无行尾时沿用文档 newline policy，未知则 LF。
- 同点插入按 operation index 合并，CandidateText 与现有 A2 byte-for-byte 等价。

### 7.4 Diagnostics delta

Current/Candidate 各运行同一 HLI-1A2 neutral diagnostic core，使用同一 captured Provider
和 Revision。每次最多 10,000 facts；任一侧超限 -> `ResultLimitExceeded`，不返回 partial。

多重集合指纹精确为：

```text
Code + SourceKind + Severity + Message + SectionId + Key
```

排除 line/column/analysis version。Added 使用 Candidate 实际位置和顺序，Removed 使用
Current 实际位置和顺序。Diagnostics code/severity/message/order 不得改变。

### 7.5 取消与异常

检查点至少包括：

- 任何 parse/model/diagnostic 前；
- current analysis 后；
- 每 256 个 operation/diagnostic projection 项；
- changes apply 前后；
- candidate analysis 前后；
- success result 构造前。

token 已请求时的取消 -> `Canceled`，无 payload。非致命异常按阶段映射为
`CurrentAnalysisFailed`、`CandidateAnalysisFailed` 或 `UnexpectedFailure`；不得泄露异常。
OOM、AccessViolation、AppDomainUnloaded、BadImageFormat、StackOverflow 不降级。

### 7.6 确定性

相同 Snapshot/Plan 的 CandidateText、Changes、OperationPreviews 和 delta 必须一致。
PreviewId 每次成功调用不同并被明确排除在 determinism 断言之外。

## 8. IDE Host 兼容契约

### 8.1 Host capture

`Ra2AuthoringSnapshot` 留在 IDE，继续校验：

- editable session 存在且非只读；
- DocumentId/EditRevision 有效；
- editor text 与 session exact text 一致；
- Registry Provider/Revision 有效。

它只新增一个单向投影为 `Ra2AutomationDocumentSnapshot` 的入口；不把 Session、dirty、
ProjectRootPath 或 runtime service 传入 Application。

### 8.2 Preview adapter

IDE `Ra2IniEditPreviewService` 收窄为薄适配器：

1. 投影 Host snapshot；
2. 调用 Application 唯一 semantic engine；
3. 将 result 包装为现有 A3/A4 Host Preview；
4. 将 public safe message 映射为现有中文 presentation message。

它不得重新 parse、重新规划、重新比较 diagnostics 或保留第二套算法。

### 8.3 A3/A4 保持

- Workspace 仍只拥有一个 active Preview 和 generation。
- 只有成功 result 可注册；失败/cancel/stale 不可注册。
- Apply 仍只接受 PreviewId + explicit confirmation。
- live DocumentId/EditRevision/session text/editor text/RegistryRevision 全量 currency recheck 不变。
- 成功仍为一次 Session revision、一次 editor sync、一个 semantic Undo unit。
- Apply 后仍不保存。
- A4 Normal/Caution/Blocked 规则、未知/低可信字段提示和新增 Error 阻断行为不变。

IDE compatibility path 可调用同一 internal engine 的 Host policy，不把 public 10,000-item
上限静默施加到既有 UI；两条路径只有 budget/failure presentation 不同，planner/diagnostic
规则权威相同。

## 9. 内部迁移清单

### 9.1 原子迁入 Application 的现有文件

```text
RA2IniEditor.IDE/TextModel/IRa2IniTextDocumentParser.cs
RA2IniEditor.IDE/TextModel/Ra2IniDocumentLine.cs
RA2IniEditor.IDE/TextModel/Ra2IniDocumentLineKind.cs
RA2IniEditor.IDE/TextModel/Ra2IniNewLineKind.cs
RA2IniEditor.IDE/TextModel/Ra2IniTextDocument.cs
RA2IniEditor.IDE/TextModel/Ra2IniTextDocumentParser.cs
RA2IniEditor.IDE/Editing/Ra2TextChange.cs
RA2IniEditor.IDE/Editing/Ra2TextChangeSet.cs
```

目标：

```text
RA2IniEditor.Application/TextModel/**
RA2IniEditor.Application/Editing/**
```

保持 internal。旧文件删除；IDE/Tests 通过 project-level global using 继续消费。
允许新增 `RA2IniEditor.IDE/TextModel/Ra2TextModelNamespaceCompatibility.cs`，其中只能包含
一个空 marker type，不能定义 parser/model/change 或转发算法。它只让冻结的旧 using
继续解析，不是第二套 TextModel。

### 9.2 取代的旧契约/算法文件

```text
RA2IniEditor.IDE/Editing/Ra2IniEditOperation.cs
RA2IniEditor.IDE/Editing/Ra2IniEditPlan.cs
```

由 public Automation 类型取代，并通过受控 alias 降低 IDE churn；旧文件删除。

```text
RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs
```

只允许改为 Host adapter；旧 semantic planning body 必须为 0。

```text
RA2IniEditor.IDE/Editing/Ra2IniEditPreview.cs
```

只允许保留 Host snapshot/plan/result wrapper 和 A3/A4 presentation projection；
diagnostic delta、semantic analysis 和 planning factory 必须迁出。

### 9.3 新 internal foundation

- 一个 exact line-insertion primitive，接收 TextModel anchor、line text/reason，返回 change/caret；
- 一个 semantic preview engine，组合 TextModel、SemanticModel、Diagnostics、FieldTrust 和 change set；
- 一个 diagnostic delta helper；
- 不新增通用 repository/service locator/cache/session。

## 10. 允许文件

实施只允许修改：

```text
RA2IniEditor.Application/Automation/Experimental/** HLI-1B contract/service files
RA2IniEditor.Application/TextModel/** moved files
RA2IniEditor.Application/Editing/** moved/new internal engine files
RA2IniEditor.IDE/TextModel/** six old files (move/delete only) + one namespace marker
RA2IniEditor.IDE/Editing/Ra2TextChange.cs (move/delete only)
RA2IniEditor.IDE/Editing/Ra2TextChangeSet.cs (move/delete only)
RA2IniEditor.IDE/Editing/Ra2AuthoringSnapshot.cs
RA2IniEditor.IDE/Editing/Ra2IniEditOperation.cs (delete)
RA2IniEditor.IDE/Editing/Ra2IniEditPlan.cs (delete)
RA2IniEditor.IDE/Editing/Ra2IniEditPreview.cs
RA2IniEditor.IDE/Editing/IRa2IniEditPreviewService.cs
RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs
RA2IniEditor.IDE/Editing/Ra2AddPropertyInsertPlanner.cs
RA2IniEditor.IDE/GlobalUsings.cs
RA2IniEditor.Tests/GlobalUsings.cs
RA2IniEditor.Application.Tests/** HLI-1B tests
RA2IniEditor.Tests/IDE/** directly affected TextModel/EditPreview/A3/A4 parity tests
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/DevelopmentRoadmap.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/README.md
Docs/AUTOMATION-HLI-1B_StageLedger.md (new at completion)
```

只有编译证明必要时，才允许对直接消费者做 namespace/type-name 机械接线；不得改变其
业务分支。SDK 项目自动纳入新文件，solution/csproj 默认不修改。

## 11. 禁止文件和语义

不得修改：

```text
ShellWindow.xaml / ShellWindow.xaml.cs
all other XAML, Dock, layout, AutomationIds
Core parser/validator/schema behavior
Infrastructure Field Registry runtime/data/provider priority
AI provider/model/streaming/tool schema and visible proposal policy
Search behavior, Completion behavior, AddProperty warnings/visible behavior
Save/Preflight/Writer/Backup/Rollback behavior
A3 active-slot/generation/single-use/currency/apply/undo semantics
legacy solution/project/editor
wire/JSON/IPC/MCP/Gateway contract
```

若迁移无法在这些边界内完成，必须停止并修订契约。

## 12. 连续实施任务卡

### HLI-1B-0 Baseline Guard and Rollback

- 确认 HLI-1A2 clean baseline 和 18-type allowlist。
- 运行 Application 47/47、A2/A3/A4 84/84 基线。
- 生成 exact move/consumer manifest 和 IdeOnly rollback package。
- 任一 gate 失败即停止。

### HLI-1B-1 Public Data Contract

- 新增 operation/plan/failure/outcome/trust/change/evidence/result/interface/service skeleton。
- 锁定构造不变量、defensive copy、failure-no-payload、enum numeric values。
- reflection allowlist 精确更新为 29。
- 不接入 IDE，不实现 Apply。

### HLI-1B-2 Neutral Text and Insertion Foundation

- 原子迁移 6 个 TextModel 和 2 个 TextChange 文件，删除旧路径。
- 增加唯一 namespace marker + IDE/Tests global using；不得修改被冻结的 Shell source。
- 抽取唯一 line-insertion primitive，IDE AddProperty planner 改为委托。
- 运行 TextModel/AddProperty/Search/Completion/Save 直接回归；行为必须逐字节不变。

### HLI-1B-3 Semantic Preview Engine

- 将 A2 planner、diagnostic delta 和 FieldTrust evidence 迁入 Application。
- 实现 public Preview service 的 8M/10k/cancellation/failure 契约。
- 删除 IDE semantic planner body，静态证明算法副本为 0。
- 运行独立 net8.0 headless parity/limits/determinism tests。

### HLI-1B-4 IDE Host Adapter

- Host snapshot 增加单向 Automation projection。
- IDE Preview service/wrapper 收窄为 adapter；保持中文 presentation 和 A3/A4 shape。
- 验证 active slot、single-use、currency、Apply/Undo、A4 policy 与迁移前等价。
- 不修改 Shell 或 UI。

### HLI-1B-5 Integration and Regression

- Application.Tests、迁移前 84 项、TextModel 受影响集和完整非 UI suite 全绿。
- 静态验证 Application Core-only、无 WPF/IDE/Infrastructure/IO。
- 检查旧路径 0、semantic planner 副本 0、public allowlist 29、diff scope 和 `git diff --check`。

### HLI-1B-6 Governance, Package and Stop

- 更新 PublicApiLedger/DecisionLog/Roadmap/CurrentPhase/Full Context。
- 生成 Stage Ledger、Verification Matrix 和 IdeOnly clean package。
- 检查禁止条目。
- 停止于 HLI-1B，不自动进入 HLI-1C。

## 13. 测试契约

### 13.1 Headless Application tests

至少覆盖：

- operation/plan 每个构造不变量与 defensive copy；
- replace 格式/注释/空值保持；
- upsert 空 Section、末行无 newline、CRLF/LF/CR/mixed newline；
- 同点插入顺序、非重叠/原文坐标；
- duplicate section/key、missing replace、conflict、no-op、stale、readonly；
- known/unknown 与 9 个 trust level 映射；
- diagnostic multiset delta 的位置漂移、message 变化和重复项；
- current/candidate analysis failure 区分；
- 8,388,608 char 输入/candidate 边界和 10,000 diagnostics 边界；
- pre/mid cancellation、nonfatal/fatal exception；
- result identity、immutability、failure-no-payload；
- repeated invocation semantic determinism 与不同 PreviewId；
- stateless/thread-safe parallel invocation；
- exported allowlist 精确 29；
- Application 仅引用 Core，无 UI/IO/runtime singleton。

### 13.2 IDE parity tests

必须保持：

- 迁移前 Application 47/47；
- 迁移前 A2/A3/A4 定向 84/84；
- 现有 A2 CandidateText、change span/order、evidence、failure、delta 全等；
- AddProperty duplicate warning/caret/newline；
- Search current-file replace plan；
- Completion commit/change apply；
- Save newline metadata/TextModel parse；
- A3 single-use/generation/currency/live recheck/session revision/Undo；
- A4 Normal/Caution/Blocked、dismiss/invalidate/replay；
- Shell transaction static boundary（Shell source diff 必须为 0）。

### 13.3 收口命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build `
  --filter "FullyQualifiedName~Ra2IniEditPreview|FullyQualifiedName~Ra2IniEditPlan|FullyQualifiedName~Ra2AuthoringSnapshot|FullyQualifiedName~Ra2IniAuthoringWorkspace|FullyQualifiedName~Ra2IniEditApply|FullyQualifiedName~Ra2InMemoryApply|FullyQualifiedName~Ra2AiAuthoring|FullyQualifiedName~Ra2EditorSessionControllerProgrammaticText|FullyQualifiedName~Ra2AuthoringShellTransaction"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

R3 权威迁移必须跑完整非 UI suite 和 clean package，不能只跑新增测试。

## 14. 静态门禁

- Application target `net8.0`、ProjectReference 仍仅 Core。
- Application production source无 WPF/AvalonEdit/AvalonDock/IDE/Infrastructure、
  `File/Directory/Process/Dispatcher/Clipboard`。
- exported allowlist 精确 29。
- 六个旧 TextModel 实现文件和两个旧 TextChange 文件为 0；旧 namespace 只允许 marker。
- 旧 Plan/Operation 文件为 0；IDE semantic planner body 为 0。
- public Preview interface 无 Apply/Save/Dispose/session/store 方法。
- Shell/XAML/Core/Infrastructure/Field Registry data/legacy diff 为 0。
- clean package 无 `.vs/bin/obj/artifacts/TestResults/old zip`。

## 15. 回滚和停止规则

- 无持久化格式或用户数据迁移；每卡必须保持可构建。
- 不用 IDE/Application 双份 planner 作为临时回滚。
- TextModel 原子移动未通过受影响回归时，回滚该卡并停止，不继续 public service。
- 若必须公开 raw TextModel/SemanticModel/diagnostic core 才能实现，停止并重议 API。
- 若 A3/A4 需要改变 active ownership、confirmation、currency、Undo 或 Save，停止并转入
  HLI-1C 契约，不在 1B 扩权。
- 若 8M/10k policy 会改变 IDE 既有行为，保留同一 engine 的 Host policy，不削弱旧断言。

## 16. 自审结果

| 项目 | 结果 | 处理 |
|---|---|---|
| 是否复制 planner/parser | Passed by contract | 原子 move + IDE thin adapter，旧算法路径必须为 0 |
| public API 是否过度膨胀 | Passed | 只增 11 个 high-level types，精确 allowlist 29 |
| 是否复用 1A1/1A2 | Passed | SemanticModel、Diagnostics、FieldTrust、common snapshot 全复用 |
| Host snapshot 是否误下移 | Passed | Capture 留 IDE，只做单向投影 |
| Apply/Save 权威是否泄漏 | Passed | public service 只有 Preview；A3/A4 留 Host |
| 格式/换行是否覆盖 | Passed by contract | exact TextModel + shared insertion primitive + byte parity tests |
| failure 是否可区分 | Passed | 保留 A2 0..16，新增 typed large/limit |
| failure 是否携带可应用数据 | Passed | 一律空 payload |
| 是否处理 limits/cancel | Passed | 8M/10k + 有界检查点 |
| 是否考虑现有 A4 | Passed | Host wrapper 保持 policy/presentation，UI 不改 |
| 是否考虑 Search/Completion/Save 旁路 | Passed | TextModel/change move 纳入受影响回归和 full suite |
| 是否锁定确定性 | Passed | semantic payload 稳定，PreviewId 明确排除 |
| 是否引入 wire/持久化债 | None | 仍是进程内 Experimental CLR API |

审查结论：该契约足够进入实现；未发现必须在 HLI-1C 或 Gateway 阶段返工的
Preview 数据、所有权或程序集边界问题。剩余主要风险是 TextModel 的跨程序集迁移面，
已通过原子卡、旧路径清零、84 项基线、受影响回归和完整测试控制。

## 17. 这些阶段分别在做什么

### 已完成

- **HLI-1A1 Document Query**：让 Agent 能从显式内存文本中读取 Section、字段和单文档引用；只读。
- **HLI-1A2 Diagnostics**：让 Agent 能运行与 IDE 相同的结构、字段、引用和链路诊断；只读。

### 当前阶段

- **HLI-1B Edit Preview**：让 Agent 提交受限字段修改计划，得到“修改后的候选文本、精确变化和风险”；仍不修改 IDE。

### 后续阶段

- **HLI-1C Host Boundary Confirmation**：确认成功 Preview 只能进入现有 A3 单槽位、显式确认、一次 Apply/Undo 路径；不新增另一条写入通道。
- **HLI-2A Capability Gateway**：把 Section/Reference/Diagnostics/Preview 四个 typed service 注册为统一 capability descriptor；不重写算法。
- **HLI-2B Built-in AI Gateway Consumer**：让当前右侧 AI 不再直接依赖 IDE internal 服务，而是调用 Gateway，同时保留 A4 安全策略。
- **HLI-2C First Agent Closed Loop**：形成自然语言 -> 查询 -> 计划 -> 预览 -> 用户确认 -> 内存应用 -> 再诊断的首个完整闭环。
- **CONTENT/ASSET 后续**：在同一高层对象与 Job/Artifact 架构上扩展 INI 对象模板、Icon、SHP、VOX/VXLSE III 切片包；不在 HLI-1B 提前实现。

## 18. Stop Rule

HLI-1B 已完成并停止。不得从本契约推断 HLI-1C、Gateway、Apply/Save public API、
独立 Agent/CLI 或自动写盘已经实现；进入 HLI-1C 前必须单独完成代码事实回归与契约。
完成证据见 `Docs/AUTOMATION-HLI-1B_StageLedger.md`。
