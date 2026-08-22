# AUTOMATION-HLI-1B Edit Preview Code Fact Audit

审计日期：2026-08-22  
状态：Completed / Read-only code fact audit  
实现状态：Not implemented；等待最终契约确认

## 1. 审计目标和边界

本文档只回归当前 A2 单文档结构化编辑预览、A3 内存事务、A4 AI 建议卡及其依赖，
为 `AUTOMATION-HLI-1B` 最终契约提供事实依据。审计没有修改生产代码、项目文件、
Shell、XAML、AI provider、保存链、Parser 语义或 Field Registry。

本阶段目标能力是：

```text
ini.document.edit.preview
```

它只接收显式内存快照和受限编辑计划，返回候选文本、非重叠变更、逐操作证据和
诊断差异。Apply、Undo、Save、磁盘访问、active Preview 所有权和用户确认都不属于
Application capability。

## 2. 当前权威调用路径

### 2.1 A2 预览算法

```text
Ra2AuthoringSnapshot
  -> Ra2IniEditPlan (1..128 UpsertField / ReplaceFieldValue)
  -> Ra2IniEditPreviewService
       -> Ra2IniLanguageAnalysisService(current)
       -> SemanticModel + TextModel 定位
       -> Ra2AddPropertyInsertPlanner / value-span replacement
       -> Ra2TextChangeSet.Apply
       -> Ra2IniLanguageAnalysisService(candidate)
       -> diagnostic multiset delta
  -> Ra2IniEditPreview
```

该算法是纯内存、UI 无关且不写盘，但位于 `RA2IniEditor.IDE`。它直接依赖 IDE
TextModel、IDE A1 orchestration、Host Registry snapshot 和 IDE Editing 类型，因此
`net8.0` Agent/Gateway 不能引用。

### 2.2 A3 Apply

```text
Ra2IniAuthoringWorkspace active slot
  -> PreviewId + explicit confirmation
  -> Shell-owned IRa2EditorTransactionPort
  -> live session/editor/registry currency recheck
  -> one session revision + one editor sync + one semantic Undo unit
```

A3 已经是正确的 Host-only 边界。它只应用成功 Preview 的 CandidateText，不调用 Save，
也不允许调用方提交任意 Preview 实例。HLI-1B 不应把这一事务端口、active slot、
generation 或 live editor 状态迁入 Application。

### 2.3 A4 消费者

- `Ra2AiAuthoringToolAdapter` 把 provider JSON 映射为两类受限操作。
- `Ra2AiAuthoringCoordinator` 调用 Workspace Preview，按新增 Error/Warning 和 Field Trust
  形成 Normal/Caution/Blocked 策略。
- `Ra2AiEditProposalViewModel` 只展示逐操作证据和原值；它不拥有 Apply 权限。
- 用户确认后仍只通过 A3 的 PreviewId 单次消费路径提交。

因此 HLI-1B 必须保持现有 A4 可见行为，但不把 AI JSON、聊天 UI 或建议卡纳入
Application public API。

## 3. 真实依赖闭包

### 3.1 已在 Application 的可复用权威

HLI-1A1/1A2 已提供：

- `Ra2DocumentSemanticModelBuilder`、Section/KeyValue symbols 和 `Ra2IniLineParser`；
- `Ra2DocumentDiagnosticService` 与 neutral `Ra2DiagnosticFact`；
- `Ra2FieldTrustClassifier`；
- public `Ra2AutomationDocumentSnapshot`、Registry snapshot、text span 和 diagnostic fact。

HLI-1B 不需要第二套 SemanticModel、Diagnostics 或 FieldTrust。

### 3.2 仍位于 IDE、但 UI-neutral 的闭包

| 文件组 | 数量 | 事实 | 结论 |
|---|---:|---|---|
| `IDE/TextModel/**` | 6 | 无 WPF；保留精确行、行尾和 newline kind；22 个 production 文件消费 | 原子迁入 Application internal，通过 global using 保持调用方 |
| `Ra2TextChange.cs` / `Ra2TextChangeSet.cs` | 2 | 无 UI/I/O；同时被 Preview、Search、Completion/AddProperty 使用 | 原子迁入 Application internal，禁止复制实现 |
| `Ra2IniEditOperation.cs` / `Ra2IniEditPlan.cs` | 2 | 已是高层 immutable input；首版形状与 HLI-0B 一致 | 用 Experimental public contract 取代 IDE 副本 |
| `Ra2IniEditPreviewService.cs` | 1 | 唯一 A2 规划算法 | 迁入 Application internal engine；IDE 文件仅留薄适配器 |
| `Ra2IniEditPreview.cs` | 1 | 混合 semantic result 与 Host snapshot/test hooks | 诊断差异迁入 Application；IDE 只保留 A3/A4 兼容 wrapper |
| `Ra2AuthoringSnapshot.cs` | 1 | 引用 editable session 和 IDE Registry snapshot | 留在 IDE，增加单向 public snapshot 投影 |
| `Ra2AddPropertyInsertPlanner.cs` | 1 | 插入格式算法 UI-neutral，但还承担 IDE duplicate warning | 抽取唯一 line-insertion primitive；IDE planner 委托并保留 warnings |

`Ra2AuthoringSnapshot` 当前被 10 个 production、12 个 test 文件引用；
`Ra2IniEditPreview` 被 10 个 production、9 个 test 文件引用。直接删除 Host wrapper
会把 A3/A4 生命周期迁移与 public API 迁移混成一次大改，风险高且没有必要。

另有 15 个 production 和 43 个 test 文件显式写有
`using RA2IniEditor.IDE.TextModel;`。其中包括被冻结的 `ShellWindow.xaml.cs`。可靠迁移不能
机械修改这 58 个文件；应保留一个无算法、无模型的 IDE namespace compatibility marker，
并在 IDE/Tests global using 导入 Application TextModel。静态门禁检查六个旧实现文件为 0，
而不是要求旧 namespace 完全不存在。

当前唯一 production `Ra2IniEditPlan` 创建者是 A4 tool adapter；它已经使用
Section 256、Key 256、Value 8192 的输入上限。因此将这三个上限冻结到 public operation
contract 不改变当前产品可达行为。

### 3.3 不迁移的对象

- editable session、dirty state、caret、AvalonEdit document；
- Workspace active slot、generation、Preview claim；
- Currency evaluator 的 live host facts；
- Editor transaction port、semantic Undo、Shell compensation；
- AI JSON schema、provider request/stream、proposal policy/ViewModel；
- Save/Preflight/Backup/Rollback/Writer。

## 4. 当前可观察语义

### 4.1 Plan 与操作

- `UpsertField`：字段存在时替换 value span，不存在时插入。
- `ReplaceFieldValue`：字段必须唯一存在。
- Section/Key 以 `OrdinalIgnoreCase` 定位，保留原文大小写。
- 一个 Plan 含 1..128 项；全部相对同一原始 Snapshot 解析。
- 同一逻辑字段多次操作、重复 Section、重复 Key、重叠 change 和 no-op 均显式拒绝。
- Section/Key 禁止结构破坏字符；Value 允许空字符串但禁止 CR/LF/NUL。

### 4.2 文本保持

- 替换只修改 value span，保留 Key、等号空白、行尾注释和行尾格式。
- 空值在等号后的零长度 span 插入。
- 缺失字段插入到目标 Section 最后一个 KeyValue 行之后；空 Section 以 header 行为锚点。
- 锚点已有 CRLF/LF/CR 时沿用该行行尾；末行无行尾时使用文档 newline policy。
- 同一插入点按 Plan 顺序合并为一个 change；全部 change 使用原文坐标。

### 4.3 证据和诊断差异

每项操作保留 outcome、resolved section kind、known-field、Field Trust、原文 span 和摘要。
未知、inferred、guardrail、obsolete、non-existent 和 pseudo-field 不被 Preview 静默
升级为 verified，也不在 A2 层一律硬拒绝。

诊断差异为多重集合差：

```text
Code + SourceKind + Severity + Message + SectionId + Key
```

不把 line/column/version 纳入指纹，因此纯行号移动不产生伪新增问题；Added 保留
candidate 顺序，Removed 保留 current 顺序。

### 4.4 确定性与 PreviewId

相同 snapshot + plan 的 CandidateText、changes、operation evidence 和 diagnostic delta
必须相同。`PreviewId` 每次成功调用重新生成，故它是故意不确定的 opaque identity，
不参与 semantic determinism 比较。

## 5. 现有缺口和风险

1. A2 权威仍在 WPF IDE assembly，独立 Agent 无法直接调用。
2. A2 current/candidate analysis 仍通过 IDE orchestration 和 ViewModel compatibility path；
   HLI-1A2 已具备 neutral diagnostics，可以移除这层反向往返。
3. TextModel 与 change set 虽无 UI 依赖，仍位于 IDE；复制到 Application 会形成双权威。
4. public Preview 需要 Field Trust typed evidence；现有 internal enum 不能直接暴露，需一个
   最小 `Ra2AutomationFieldTrustLevel` 投影。
5. 现有 A2 没有 public 8,388,608-character/10,000-diagnostic budget；新 public service
   必须有 typed limits，但 IDE compatibility path 不得因此静默改变现有 Host 行为。
6. 现有测试通过 `CurrentAnalysis/CandidateAnalysis` 注入错误；这是 test hook，不是产品
   消费面。迁移后应改测 neutral result/adapter，不得为保留测试便利公开 raw analysis。
7. TextModel 移动会触及 Save/Search/Completion 的编译依赖；必须用全量回归证明纯位置迁移，
   不能只跑新增 headless tests。

## 6. Public API 事实

当前 Application exported allowlist 为精确 18 个类型。HLI-1B 不能公开 raw TextModel、
SemanticModel、diagnostic core、FieldTrust classifier、Host snapshot 或 transaction port。

满足 Gateway 首版所需的最小新增面为 11 个类型：

```text
IRa2AutomationEditPreviewService
Ra2AutomationEditPreviewService
Ra2AutomationEditOperationKind
Ra2AutomationEditOperation
Ra2AutomationEditPlan
Ra2AutomationEditPreviewFailureKind
Ra2AutomationEditOperationOutcomeKind
Ra2AutomationFieldTrustLevel
Ra2AutomationTextChange
Ra2AutomationEditOperationPreview
Ra2AutomationEditPreviewResult
```

实施后精确 allowlist 应为 29，不增加 Apply/Save/Gateway/wire 类型。

## 7. 迁移前测试基线

审计时执行：

```powershell
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj `
  -c Debug --no-build
```

结果：47 passed / 0 failed / 0 skipped。

另执行 A2/A3/A4/Session/Shell transaction 定向过滤集，结果：

```text
84 passed / 0 failed / 0 skipped
```

实施后两组基线都必须保持，并增加独立 headless Preview、TextModel relocation、
public reflection allowlist 和 IDE parity tests。

## 8. 审计结论

1. HLI-1B 有必要：当前能力算法已存在，缺口是程序集位置和高层 contract，而不是再写
   一套 Agent 编辑器。
2. 最小可靠路线是迁移唯一算法权威，并让 IDE 通过薄适配器继续走 A3/A4；复制 planner
   或把 A3 transaction 下移都会制造返工。
3. `Ra2AuthoringSnapshot` 必须留在 Host；它只负责捕获 live state 并投影为
   `Ra2AutomationDocumentSnapshot`。
4. TextModel/change set 需要原子迁移并覆盖 Save/Search/Completion 回归；line insertion
   需要抽取唯一 primitive 供 AddProperty 和 headless Preview 共用。
5. public contract 可以在 HLI-1B 一次冻结为 29-type allowlist；Apply/Undo/Save 继续后置且
   Host-only。
