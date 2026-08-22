# RA2IniEditor.IDE Public API Ledger

更新时间：2026-08-22  
当前阶段：AUTOMATION-HLI-1B Final contract / not implemented

本台账只记录跨程序集或未来 Gateway 可见的契约。HLI-1A1 已实现首个
Experimental Document Query public API；它仍不是 JSON/IPC/MCP/CLI 或稳定 SDK。

## 1. 当前变更

| 阶段 | Public API 变更 | 兼容性影响 |
|---|---|---|
| HLI-1A0 | None | None；tests + docs only |
| HLI-1A1 | 新增下列精确 15-type allowlist | Additive / Experimental；reflection、headless 和 full regression 已通过 |
| HLI-1A2 | 扩展 `Validate` + 精确 3-type allowlist | Implemented / Experimental；18-type reflection 与完整回归通过 |
| HLI-1B | 候选新增 Edit Preview service + 精确 11-type allowlist | Contracted / not implemented；实施后 allowlist 应为 29 |

## 2. HLI-1A1 已实现 Experimental 查询契约

以下是 `AUTOMATION-HLI-1A1` 已实现的精确 allowlist。实现状态为 `Implemented`，
稳定性仍为 `Experimental`：

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| HLI-1A1 | `IRa2AutomationDocumentQueryService` | Interface | 跨程序集调用 Section/Reference | HLI-2A Gateway | Experimental | interface/reflection/behavior | 不含 Diagnostics/Preview |
| HLI-1A1 | `Ra2AutomationDocumentQueryService` | Stateless implementation | 给 IDE/Agent host 可构造的唯一实现 | HLI-2A Gateway | Experimental | thread/boundary/parity | 无 cache/IO/singleton |
| HLI-1A1 | `Ra2AutomationDocumentSnapshot` | Immutable envelope | 绑定文档身份、版本、文本和 Registry generation | all HLI-1 slices | Experimental | constructor/identity/retention | FilePath 不授予 I/O |
| HLI-1A1 | `Ra2AutomationFieldRegistrySnapshot` | Immutable envelope | 绑定 readonly Provider + Revision | all HLI-1 slices | Experimental | constructor/revision/retention | 非 wire DTO |
| HLI-1A1 | `Ra2AutomationTextSpan` | Value DTO | 对外表达零基半开字符区间 | HLI-1A1/1B/2A | Experimental | range/overflow/bounds | 不公开 internal span |
| HLI-1A1 | `Ra2AutomationSectionQuery` | Request DTO | 名称 + nullable zero-based occurrence | HLI-2A | Experimental | unique/ambiguous/occurrence | null 要求唯一 |
| HLI-1A1 | `Ra2AutomationSectionQueryResult` | Result DTO | 结构化成功/失败与文档身份 | HLI-2A | Experimental | state invariants | 失败无 Section payload |
| HLI-1A1 | `Ra2AutomationSectionQueryFailureKind` | Failure enum | 区分 not found/ambiguous/limit/cancel/analysis | HLI-2A | Experimental | every enum path | 不以 message 推断 |
| HLI-1A1 | `Ra2AutomationSectionFact` | Fact DTO | Section kind/occurrence/spans/fields | HLI-2A/2C | Experimental | ordering/spans/duplicate isolation | 不公开 SemanticModel |
| HLI-1A1 | `Ra2AutomationFieldFact` | Fact DTO | 所选 Section 内字段事实 | HLI-2A/2C | Experimental | duplicate key/order/spans | 按 body span 归属 |
| HLI-1A1 | `Ra2AutomationReferenceQuery` | Request DTO | offset + optional selection | HLI-2A | Experimental | offset/selection/fallback | 不含 WPF caret |
| HLI-1A1 | `Ra2AutomationReferenceQueryResult` | Result DTO | 目标和有序 current-document 引用 | HLI-2A | Experimental | empty/unresolved/order | empty 可成功 |
| HLI-1A1 | `Ra2AutomationReferenceQueryFailureKind` | Failure enum | 区分 location/target/limit/cancel/analysis | HLI-2A | Experimental | every enum path | 无 NoReferences failure |
| HLI-1A1 | `Ra2AutomationReferenceTargetFact` | Fact DTO | 已解析目标名称和类型 | HLI-2A/2C | Experimental | header/value/missing definition | 不保证定义存在 |
| HLI-1A1 | `Ra2AutomationReferenceFact` | Fact DTO | current-document 引用来源与 spans | HLI-2A/2C | Experimental | source order/case/spans | 不含 navigation command |

命名空间统一为 `RA2IniEditor.Application.Automation.Experimental`。当前 assembly
exported types 与这 15 项精确相等；证据见 `AUTOMATION-HLI-1A1_StageLedger.md`。

## 3. HLI-1A2 已实现 Experimental Diagnostics 契约

HLI-1A2 扩展现有 `IRa2AutomationDocumentQueryService`，没有新建 Diagnostics service
或空 request DTO。以下 API 状态均为 `Implemented / Experimental`：

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| HLI-1A2 | `IRa2AutomationDocumentQueryService.Validate` | Method | 从显式文档快照运行当前文档诊断 | HLI-2A/2C | Experimental | interface/reflection/behavior | 扩展既有 interface，对自定义 implementer 有兼容风险 |
| HLI-1A2 | `Ra2AutomationDocumentDiagnosticsResult` | Result DTO | 返回 identity/revision、typed failure 和 immutable facts | HLI-2A/2C | Experimental | state/identity/empty/failure | failure 无 partial facts |
| HLI-1A2 | `Ra2AutomationDocumentDiagnosticsFailureKind` | Failure enum | 区分 large/limit/cancel/analysis | HLI-2A | Experimental | every enum path | None=0，不以 message 推断 |
| HLI-1A2 | `Ra2AutomationDiagnosticFact` | Fact DTO | UI-neutral code/source/severity/location/version | HLI-1B/2A/2C | Experimental | parity/order/immutability | 不公开 ViewModel/raw model |

Application exported allowlist 已从 15 精确更新为 18；证据见
`AUTOMATION-HLI-1A2_StageLedger.md`。

## 4. HLI-1B 已冻结、尚未实现的 Experimental 契约

以下候选已由 `AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md` 冻结，但尚未
写入生产代码，不能标记为 Implemented：

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Required Tests | Notes |
|---|---|---|---|---|---|---|---|
| HLI-1B | `IRa2AutomationEditPreviewService` | Interface | Headless 单文档结构化预览入口 | HLI-2A | Experimental | interface/boundary/parity | 无 Apply/Save/store |
| HLI-1B | `Ra2AutomationEditPreviewService` | Stateless implementation | 唯一 public Preview 实现 | HLI-1C/2A | Experimental | limits/cancel/thread/parity | 8M chars/10k diagnostics |
| HLI-1B | `Ra2AutomationEditOperationKind` | Input enum | 两类受限字段操作 | HLI-2A/2C | Experimental | every enum path | Upsert/Replace only |
| HLI-1B | `Ra2AutomationEditOperation` | Input DTO | Section/Key/Value 受限意图 | HLI-2A/2C | Experimental | invariants/bounds | 无 raw patch |
| HLI-1B | `Ra2AutomationEditPlan` | Input DTO | 绑定 identity/version/registry/operations | HLI-2A/2C | Experimental | defensive copy/stale | 1..128 operations |
| HLI-1B | `Ra2AutomationEditPreviewFailureKind` | Failure enum | 保留 A2 失败分类并增加 large/limit | HLI-2A | Experimental | every enum path | 0..16 保持既有顺序 |
| HLI-1B | `Ra2AutomationEditOperationOutcomeKind` | Evidence enum | 区分 inserted/replaced | HLI-2A/2C | Experimental | parity | 不表达 Apply 状态 |
| HLI-1B | `Ra2AutomationFieldTrustLevel` | Evidence enum | typed Field Trust 投影 | HLI-2A/2C | Experimental | exact mapping | 不公开 classifier |
| HLI-1B | `Ra2AutomationTextChange` | Fact DTO | 表达有序原文坐标变化 | HLI-1C/2A | Experimental | overlap/order/immutability | 构造器 internal |
| HLI-1B | `Ra2AutomationEditOperationPreview` | Fact DTO | 逐操作 outcome/span/trust 证据 | HLI-1C/2C | Experimental | parity/order | 构造器 internal |
| HLI-1B | `Ra2AutomationEditPreviewResult` | Result DTO | candidate/changes/delta/typed failure | HLI-1C/2A/2C | Experimental | state/no-partial/determinism | failure 无可应用 payload |

实施完成后 Application exported allowlist 必须从 18 精确增加到 29；在实现前仍应保持 18。

## 5. 延后候选

| 候选 | 状态 | 原因 |
|---|---|---|
| Edit Preview request/result/change DTO | Contracted in HLI-1B / not implemented | 最终形状已冻结；等待 R3/R2 实施授权 |
| Apply/Undo API | Host-only by design | 继续由 IDE active session/currency/transaction 拥有 |
| Save/Backup/Rollback API | Host/user-owned by design | 不成为 Headless Application 的写盘接口 |

## 6. 明确不公开的实现基础

下列现有类型在 HLI-1A1 中即使移动，也应保持 internal，并通过精确
`InternalsVisibleTo` 仅供 IDE、现有 Tests 和新 Application.Tests 使用：

- `Ra2IniLineParser`
- `Ra2SectionClassifier` 及 classification result/warning
- `Ra2DocumentSemanticModelBuilder`、`Ra2DocumentSemanticModel`
- `Ra2CaretContextService`、`Ra2ReferenceFinder`
- 相关 symbol/span/reference internal records
- `Ra2DiagnosticFact`、FieldTrust classifier/info/level
- Field/reference/chain diagnostic services 和 reference catalog
- `Ra2IniTextDocument`、document line/newline/parser
- internal text change set、line insertion primitive 和 semantic Preview engine
- `Ra2AuthoringSnapshot`、Currency evaluator、Workspace 和 TransactionPort

禁止用公开这些内部模型的方式减少迁移改动；未来 Gateway 只能依赖上面的高层
Experimental DTO/service。

## 7. 下一次台账更新门禁

下一次状态更新门禁是 HLI-1B 实施完成和精确 29-type reflection 证据。不得在实现前
把候选标为 Implemented，也不得借 HLI-1B 公开 raw TextModel/SemanticModel、Apply/Save，
或改变已实现 Section/Reference/Validate 的失败语义。
