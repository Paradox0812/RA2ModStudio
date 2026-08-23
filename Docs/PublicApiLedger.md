# RA2IniEditor.IDE Public API Ledger

更新时间：2026-08-23
当前阶段：AGENT-MODE-1 / AGENT-KNOWLEDGE-1 Completed / Verified

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
| POST-HLI-0 | None | DocsOnly：裁决 CONTENT-1 先于 HOST-1；候选 API 尚未批准 |
| CONTENT-1A | 新增 Field Schema Query 精确 5-type allowlist、Gateway 方法和 capability | Implemented / Experimental；allowlist 40，catalog 5 |
| CONTENT-1B | 新增 Reference Resolve 精确 5-type allowlist、Gateway 方法和 capability | Implemented / Experimental；allowlist 45，catalog 6 |
| CONTENT-1C | 新增 SectionCreate 精确 2-type allowlist，并 additive 扩展 EditPlan/Result | Implemented / Experimental；allowlist 47，旧构造器保留 |
| CONTENT-1D | None | Verified：internal template domain/compiler；public diff 0 |
| CONTENT-1E | 新增 Template service/descriptors/request/result/warning 精确 11-type allowlist、Gateway 方法和 capability | Implemented / Experimental；allowlist 58，catalog 7，Gateway methods 9 |
| CONTENT-1F / UI-1 | None | Verified：IDE consumer 与 Diff projection 均 internal；Apply/Save public authority 0 change |
| AGENT-MODE-1 | 新增 `Ra2AutomationTemplateOutputKind`，descriptor 增加 immutable `OutputKind` | Implemented / Experimental；allowlist 59；旧构造/模板继续兼容 |
| AGENT-KNOWLEDGE-1 | None | Implemented / Internal；15 个 BuiltIn Skill，不增加 capability、Apply/Save 或 wire API |

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

## 11. POST-HLI-0 历史候选登记

本节是 POST-HLI-0 当时的历史候选。CONTENT-1 已获连续实施授权；当前事实以第 12 节为准。

| Candidate | Status | Earliest Review | Boundary |
|---|---|---|---|
| Field schema request/result/fact/failure | Candidate / Not approved | CONTENT-1A | captured Registry snapshot；不得公开 provider/singleton |
| Resolve reference request/result/fact/failure | Candidate / Not approved | CONTENT-1B | current-document typed query；不得冒充 project-wide |
| Template definition/instance/parameter/failure | Candidate / internal-first | CONTENT-1D | 无持久化/wire/provider DTO 承诺 |
| CreateSection / ApplyTemplate preview | Candidate / Not approved | CONTENT-1C | deterministic Preview only；无 Apply/Save |
| Host wire/session/permission DTO | Explicitly deferred | HOST-1 | 必须独立 R4 契约与版本策略 |

当前 production 事实已变为 47 个 exported Experimental types、六项 capability 和七个 Gateway
方法。`Ra2AutomationFieldRegistrySnapshot.Provider` 明确仍是进程内对象，不得当作 wire shape。

## 12. CONTENT-1 连续契约当前登记

权威契约：`AUTOMATION-CONTENT-1_SemanticTemplateContinuousFinalContract.md`。用户已确认连续执行；
以下状态按实际实现和验证更新。

| Stage | API group | Kind | Reason | Expected Next Use | Stability | Planned Tests | Notes |
|---|---|---|---|---|---|---|---|
| CONTENT-1A | FieldSchema Query/Failure/Fact/Result/Disposition | DTO/fact/enum/method/capability | 读取 effective schema/trust | 1C/1D/Host | Implemented / Experimental | 30/30 + consumer 8/8 | allowlist 40；catalog 5 |
| CONTENT-1B | ReferenceResolve Query/Failure/Basis/Fact/Result | DTO/fact/enum/method/capability | 解析 source field 当前目标 | 1D/asset binding | Implemented / Experimental | 38/38 + semantic consumer 37/37 | allowlist 45；catalog 6 |
| CONTENT-1C | SectionCreate Operation/Preview + EditPlan/Preview additions | operation/fact/overload/properties/failure enum tail | 新 Section 进入唯一 Preview | 1D/1E | Implemented / Experimental | 54/54 + IDE/Workspace 48/48 | allowlist 47 |
| CONTENT-1D | Template domain/compiler | Internal | definition/parameter/compiler/value validation | 1E | Implemented / Internal | Application full 146/146 | public diff 0 |
| CONTENT-1E | Template service/descriptors/arguments/results/warnings | service/DTO/fact/enum/method/capability | discovery + expansion to EditPlan | 1F/Host | Implemented / Experimental | source gate/catalog/version/arguments/parity/limits | allowlist 58；catalog 7；Gateway methods 9 |
| CONTENT-1F | None | IDE internal consumer | 复用 Gateway/Workspace/Transaction | product loop | Implemented / Internal | template loop/stale/policy/atomic Apply/no-Save | public diff 0 |
| CONTENT-UI-1 | None | IDE internal Diff projection/view | 主工作区审阅现有 Preview | product review loop | Implemented / Internal | focused 20/20；full 2568/2568 | public diff 0；无第二 Apply authority |

候选数字不得驱动 API 膨胀：若实现发现类型没有后续消费者，应保持 internal，并在实现前调整
Task Card/ledger。不得把 Core provider、SemanticModel、Template definition、Workspace 或 Apply/Save
公开为捷径。

## 13. AGENT-MODE-1 / AGENT-KNOWLEDGE-1 当前登记

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| AGENT-MODE-1 | `Ra2AutomationTemplateOutputKind` | Enum | 机器可读地区分 Skeleton 与 CompleteObject | Mode routing / future Host discovery | Experimental | reflection/catalog/behavior | additive；不表达 Apply 权限 |
| AGENT-MODE-1 | `Ra2AutomationTemplateDescriptor.OutputKind` | Immutable property | 调用方不再从模板 ID 猜完整度 | IDE route / future wire review | Experimental | constructor/catalog | 既有 descriptor 构造保留 |
| AGENT-KNOWLEDGE-1 | None | Internal loader + Markdown content | 领域过程知识按需注入，不污染 public Gateway | CONTENT-2 profiles | Internal | loader/selection/prompt/full regression | BuiltIn only；no scripts/external roots |

Application exported Experimental allowlist 精确为 59。Chat/Work route、Skill descriptor/catalog 和
prompt selection 均保持 IDE internal；Skill 不是 capability，也不改变现有 Gateway catalog 7、methods 9、
Apply/Undo/Save authority 或 wire shape。

## 14. CONTENT-2A 当前登记

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| CONTENT-2A | None | Existing descriptor data + IDE internal route/schema | 增加 Techno dual-armament complete profile | Experimental / implemented | public allowlist 保持 59；Gateway catalog/methods 不变 |

## 15. CONTENT-2B public API 零变更确认

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| CONTENT-2B | None | Existing descriptor data + IDE internal route/schema | 增加 Arcing/Homing Projectile 与 YR core Warhead profiles | Experimental / implemented | public allowlist 保持 59；Gateway catalog 7 / methods 9 不变 |

新增 profile id/version/parameter 是既有 `Ra2AutomationTemplateDescriptor` 目录数据，不新增 DTO、
failure kind、Gateway 方法、wire shape 或 Apply/Save authority。Application 151/151 与完整 IDE
2601/2601 回归通过。

## 16. CONTENT-2D-0/1 public API 零变更确认

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| CONTENT-2D-0/1 | None | Application internal model/compiler extension | 对象闭包声明与当前文档数字注册分配 | Internal / implemented | public allowlist 保持 59；Gateway catalog/methods 不变 |

`Ra2ContentTemplateRegistrationSpec`、注册策略、目录、分配状态和新增 compilation failure kinds
均为 internal；既有 public Template/Gateway/EditPlan/Preview/Apply/Save shape 零变化。证据见
`Docs/AUTOMATION-CONTENT-2D01_StageLedger.md`。
