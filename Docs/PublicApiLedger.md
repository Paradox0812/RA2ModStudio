# RA2IniEditor.IDE Public API Ledger

更新时间：2026-08-23
当前阶段：AUTOMATION-HLI-2B Completed / Verified

本台账只记录跨程序集或未来 Gateway 可见的契约。HLI-1A1 已实现首个
Experimental Document Query public API；它仍不是 JSON/IPC/MCP/CLI 或稳定 SDK。

## 1. 当前变更

| 阶段 | Public API 变更 | 兼容性影响 |
|---|---|---|
| HLI-1A0 | None | None；tests + docs only |
| HLI-1A1 | 新增下列精确 15-type allowlist | Additive / Experimental；reflection、headless 和 full regression 已通过 |
| HLI-1A2 | 扩展 `Validate` + 精确 3-type allowlist | Implemented / Experimental；18-type reflection 与完整回归通过 |
| HLI-1B | 新增 Edit Preview service + 精确 11-type allowlist | Implemented / Experimental；29-type reflection 与完整回归通过 |
| HLI-1C | None | Verified：Host boundary guards/tests 完成；allowlist 精确保持 29 |
| HLI-2A | 新增固定目录 + typed Gateway 的精确 6-type allowlist | Implemented / Experimental；allowlist 35，94/94 + 2537/2537 |
| HLI-2B | None | Verified：IDE consumer 切换与资源门禁完成；allowlist 精确保持 35 |
| HLI-2C | None | Verified：复用 Gateway/Coordinator/Workspace；不新增 Agent façade 或 Apply/Save |

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

## 4. HLI-1B 已实现的 Experimental 契约

以下 API 已按 `AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md` 实现，状态均为
`Implemented / Experimental`：

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
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

Application exported allowlist 已从 18 精确增加到 29；reflection、行为、边界、并发、
取消和完整回归证据见 `AUTOMATION-HLI-1B_StageLedger.md`。

## 5. 延后候选

| 候选 | 状态 | 原因 |
|---|---|---|
| Edit Preview request/result/change DTO | Implemented / Experimental | 已由 typed Gateway 暴露；尚无 wire contract |
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

## 7. HLI-1C 历史零变更确认

HLI-1C public API diff 为 0；Host boundary contract tests 与完整回归通过，Application
exported allowlist 精确保持 29。没有公开 raw TextModel/SemanticModel、Apply/Save，
也没有改变 Section/Reference/Validate/Preview 的公开失败语义。

上述 29-type 是 HLI-1C 完成时的历史事实；HLI-2A 随后已把当前 production allowlist
增加到 35。当前状态以第 8 节和 HLI-2A Stage Ledger 为准。

## 8. HLI-2A 已实现的 Experimental Gateway API

下列 6 个类型已按最终契约实现，状态为 `Implemented / Experimental`：

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Required Tests | Notes |
|---|---|---|---|---|---|---|---|
| HLI-2A | `IRa2AutomationCapabilityGateway` | Interface | 四项能力的 typed discovery/invocation boundary | HLI-2B/2C/future host | Experimental | exact surface/parity/cancel | 无 generic Invoke |
| HLI-2A | `Ra2AutomationCapabilityGateway` | Stateless implementation | 委托现有两个 canonical service | HLI-2B/2C | Experimental | ctor/thread/parity | 无 state/cache/host |
| HLI-2A | `Ra2AutomationCapabilityDescriptor` | Immutable descriptor | 暴露 ID/version/risk/stability/limits | discovery/policy | Experimental | exact properties/immutability | internal ctor |
| HLI-2A | `Ra2AutomationCapabilityIds` | Constants | 冻结四项 ID 与 CurrentVersion=1 | routing/wire adapter later | Experimental | exact constants/order | 无 alias |
| HLI-2A | `Ra2AutomationCapabilityRisk` | Enum | 区分 Query/Edit 声明风险 | future policy | Experimental | exact values | 不是授权票据 |
| HLI-2A | `Ra2AutomationCapabilityStability` | Enum | 声明当前兼容状态 | discovery | Experimental | exact values | 当前只有 Experimental |

Application exported allowlist 已从 29 精确变为 35。12 项 Gateway focused facts、94 项
Application tests、11 项 HLI-1C boundary 和 2537 项完整非 UI 回归均通过。HLI-2A 没有新增
统一 failure/result、wire DTO、Apply/Save、Job/Event/Artifact 或 provider schema；完整签名
和完成证据见 `AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md` 与
`AUTOMATION-HLI-2A_StageLedger.md`。

## 9. HLI-2B public API 零变更确认

HLI-2B 实现与完整回归确认 consumer 切换没有新增或修改 public API：

- Application exported allowlist 必须保持精确 35；
- 复用 `IRa2AutomationCapabilityGateway.Preview`、既有 snapshot/plan/result/failure；
- `Ra2AiEditAvailabilityKind.ResourceLimitExceeded` 仅是 IDE internal 状态；
- 删除 internal `PreviewForHost` 不影响 public surface；
- 不增加 Host budget overload、Apply/Save、unified Gateway failure 或 wire DTO。

状态：`Completed / Verified`。Application exported allowlist 精确为 35；聚焦回归 78/78、
Application 94/94、完整 non-UI 2547/2547。证据见 `AUTOMATION-HLI-2B_StageLedger.md`。

## 10. HLI-2C public API 零变更确认

HLI-2C 首个高层 Agent 闭环只组合既有 public Gateway facts 与 IDE internal authority：

- 不新增 `IAgent`、Agent workflow/session/result、capability 或 failure kind；
- Gateway catalog、五方法 interface、snapshot/plan/result shape 和 allowlist 35 保持不变；
- Apply/Undo/Save、proposal、Workspace 和 transaction 继续 internal/Host-owned；
- 端到端 trace 只作为测试证据，不成为 production/public DTO。

状态：`Completed / Verified`。reflection、loopback、transaction 和完整回归通过；Application
exported allowlist 精确为 35，Gateway catalog/五方法 surface 不变。证据见
`AUTOMATION-HLI-2C_StageLedger.md`。
