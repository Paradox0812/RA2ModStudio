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

## 17. CONTENT-2D-2 Experimental API

状态：Implemented / verified；R4 architecture review 与用户确认已完成，当前 allowlist 为 63。

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Planned Tests | Notes |
|---|---|---|---|---|---|---|---|
| CONTENT-2D-2 | `Ra2AutomationProjectSnapshot` | Immutable DTO | 绑定同一 project-session/revision 的文档集合 | rules/art、完整 Techno/SuperWeapon、future Host | Experimental | constructor/identity/path/limits/immutability；24/24 focused | 只包装现有 DocumentSnapshot；不是写权限 |
| CONTENT-2D-2 | `Ra2AutomationProjectEditPlan` | Immutable DTO | 将多个现有 EditPlan 绑定为一个有序原子意图 | project Preview | Experimental | membership/order/work limits；24/24 focused | 不携带任意目标路径 |
| CONTENT-2D-2 | `Ra2AutomationProjectEditPreviewFailureKind` | Failure enum | 区分项目/成员/stale/resource/cancel failure | Host diagnostics | Experimental | exact values/no-partial；Application 167/167 | 叶失败另投影既有 failure |
| CONTENT-2D-2 | `Ra2AutomationProjectEditPreviewResult` | Result DTO | 成功态返回有序叶 Preview，失败态保证无 partial payload | project Diff/Host Apply | Experimental | state matrix/no-partial/parity；Application 167/167 | 不包含 Apply/Undo/Save |
| CONTENT-2D-2 | `IRa2AutomationCapabilityGateway.PreviewProject` + `ini.project.edit.preview` | Method/capability | 从唯一 Gateway 调用项目 Preview | built-in Agent/future Host | Experimental | reflection/catalog/delegation/cancel；methods 10/catalog 8 | interface additive compatibility risk；仓库实现均已更新 |

实际 Application exported allowlist 为 63、Gateway catalog 8、methods 10；没有公开 Apply、Undo、
Save、session store 或文件系统能力。

## 18. CONTENT-2D-3 / ASSET-MANIFEST-1 Experimental API

状态：Implemented / verified；当前 allowlist 69、Gateway catalog 9、methods 11。

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| CONTENT-2D-3 | `IRa2AutomationTemplateService.ExpandProjectTemplate`、Gateway 同名方法、`ini.project.content.template.expand` | Method/capability | 从唯一模板/Gateway 入口产生跨文档 Project Plan | Host/AI project proposal | Experimental | pairing/version/cancel/no-partial/Gateway parity | 不公开 Apply/Save |
| CONTENT-2D-3 | `Ra2AutomationProjectTemplateExpansionResult` | Immutable result | 将 Project Plan、Manifest 与失败证据绑定到同一 project revision | Preview/Host | Experimental | state/snapshot/immutability | 失败态零 partial payload |
| ASSET-MANIFEST-1 | `Ra2AutomationAssetManifest`、`Ra2AutomationAssetRequirement`、`Ra2AutomationAssetBindingFact` | Immutable facts | 描述后续素材提供器输入与 INI 绑定证据 | SHP/Cameo/VXL providers | Experimental | limits/path/duplicate/closure | Manifest 无写权限 |
| ASSET-MANIFEST-1 | `Ra2AutomationAssetKind`、`Ra2AutomationAssetBindingState` | Enum | 区分资产家族与 Proposed/PendingSchema 状态 | provider routing/review | Experimental | exact enum/reflection | PendingSchema 不产生 INI operation |

`Ra2AutomationTemplateDescriptor` additive 增加 `IsProjectTemplate`、`ProducesAssetManifest`，
`Ra2AutomationTemplateOutputKind` 追加 `ProjectBinding`；现有六个模板均保持 document-only。

## 19. ASSET-PROVIDER-1 Experimental API

状态：Implemented / verified；Application exported allowlist 69 -> 77。Gateway catalog 9、methods 11
保持不变。

| API | Kind | Reason | Expected Next Use | Stability | Notes |
|---|---|---|---|---|---|
| `IRa2AutomationAssetProvider`、`Ra2AutomationExistingAssetProvider` | Interface/service | 把 Asset Manifest 解析为有界内存 Artifact | provider plugins、Asset Host | Experimental | 不读写文件、不进入 INI Gateway |
| `Ra2AutomationAssetProviderDescriptor` | Immutable descriptor | provider identity/version/supported kinds | provider routing | Experimental | public constructor；supported kinds readonly |
| `Ra2AutomationAssetSource` | Immutable input | Host 显式提交素材内容 | existing asset import | Experimental | 每项 16 MiB；防御性复制 |
| `Ra2AutomationAssetArtifact` | Immutable output | 返回 content length/SHA-256/有限验证级别 | Host persistence/review | Experimental | hash 由内容计算；`CopyContent` 返回副本 |
| `Ra2AutomationAssetProviderResult` | Result/fact | 成功或零产物失败证据 | Host/provider orchestration | Experimental | public success/failure factories；Manifest closure enforced |
| `Ra2AutomationAssetProviderFailureKind`、`Ra2AutomationAssetVerificationLevel` | Enums | 稳定区分失败与有限验证保证 | UI/Host diagnostics | Experimental | 不声称格式/尺寸/调色板已解析 |

Public interface 的 Descriptor、Artifact 和 Result 工厂均可由外部实现消费；不存在“公开接口但无法
构造返回值”的伪扩展点。Apply/Save、文件路径、wire/serialization、Job/Event/Registry 未公开。

### ASSET-VOX-1C candidate boundary

状态：Implemented / automated verified；无 public API 变化。

1C 完成后 Application exported allowlist 仍保持 77；新 AssetHost 类型在首个真实 provider 认证前保持
internal。子进程 protocol v1 是版本化 compatibility contract，但不是 Application public API，也不是插件
承诺。若未来需要第三方程序集实现 Provider，必须在独立阶段重新执行 public API review，而不是直接把
1C internal 类型改为 public。

修订契约冻结的 `IRa2VoxelGenerationHost`、`ProbeAsync`、`RunAsync` 和
`IRa2GenerationWorkspaceLease : IAsyncDisposable` 仍全部是程序集 internal。它们是 1C 实现门禁，
不是第三方扩展承诺；自动门禁确认 Application public allowlist 为 77，AssetHost exported public types 为 0。

## 20. CONTENT-PROJECT-UI-1-NF6 Experimental Preview 语义调整

状态：Implemented / verified；public signature、DTO、enum shape、Gateway catalog/method 数均无变化。

| API | Change Kind | Previous behavior | Current behavior | Stability | Tests |
|---|---|---|---|---|---|
| `Ra2AutomationEditPreviewService.Preview` / `Ra2AutomationEditPreviewFailureKind.BlockedFieldTrust` | Semantic-only | 新 Section 的字段被 Registry 标为 Blocked 时一律失败 | 当 operation evidence 的 `ExpectedSectionKind=Unknown` 时保留 Caution 而不失败；具体 SectionKind 仍 fail closed | Experimental | `Ra2AutomationSectionCreationPreviewTests` + generic project integration |

该变化服务于模型主导的通用 Work Project Plan：Field Registry 不再拥有未知 Section 内容的否决权。
它不增加文件范围、Apply/Save 权限或路径输入；现有强类型 headless template 仍保留旧行为。

## 21. AGENT-KNOWLEDGE-1-R2 public API 零变更确认

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| AGENT-KNOWLEDGE-1-R2 | None | IDE internal Skill data + selection | 为 Work rules/art 项目注入 source-backed 跨文档绑定知识 | Internal / implemented | public allowlist、Gateway catalog/methods、DTO、wire shape 与 Apply/Save authority 不变 |

新增第 16 个 BuiltIn Skill 并更新 `ra2-field-schema-trust` 文本；Skill descriptor、catalog、选择和 prompt
仍全部 IDE internal、瞬态且非序列化。

## 22. AGENT-SKILL-ROUTING-2 public API 零变更确认

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| AGENT-SKILL-ROUTING-2 | None | IDE-internal catalog manifest, intent JSON and prompt orchestration | 让 Work 第一轮推荐 Skill、Host 解析后注入第二轮 | Internal .NET / Experimental provider JSON | Application allowlist、Gateway catalog/methods、Apply/Save authority 不变 |

Provider-visible `analyze_ra2_authoring_intent` 参数结果新增必填 `selected_skill_ids` 与 `knowledge_gaps`；
它们仅存在于一次请求的瞬态意图包，不持久化、不导出为 Application API。Manifest、resolution、
PromptBuilder interface extension 与 pipeline result 均为 IDE internal。

## 23. AGENT-CONTEXT-3 public API 零变更确认

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| AGENT-CONTEXT-3 | None | IDE-internal request projection, provider JSON and orchestration | 在现有两调用 Work 中共享上下文并执行有界 HLI 查询 | Internal .NET / Experimental provider JSON | Application exported API、Gateway catalog/methods、Apply/Save authority 均不变 |

Provider-visible `analyze_ra2_authoring_intent` 结果新增必填 `context_queries`，Host 兼容缺失该字段的旧有效包并视为空列表。
该字段只接受 `current/rules/art` 符号目标及 `get_section/resolve_reference`；所有投影、请求和结果类型均为 IDE internal、
request-lifetime、非持久化对象。复用现有 `IRa2AutomationCapabilityGateway`，没有新增 public query surface。

## 24. CONTENT-2E public API 收口

状态：Implemented / automated verified。

| Task/Stage | API | Kind | Reason | Expected Stability | Stop rule |
|---|---|---|---|---|---|
| CONTENT-2E | None | Existing descriptor data + Application/IDE internal compiler/route/Skill | 增加两个 typed SuperWeapon profiles 与 generic fallback | Application exported API、Gateway method/catalog、持久化和 Apply/Save authority 零变化 | Application 196/196、IDE 2722/2722 |

provider-visible intent capability enum 已 additive 增加三个 ID，但它仍属于 IDE internal Experimental wire shape；
schema/parse/route 回归已通过并记录在 Stage Ledger。模板目录数据增加两个 descriptor；没有新增 exported
type/method/enum、Gateway method、持久化字段、filesystem/path/Apply/Save 权限。

## 25. AGENT-QUERY-2 public API 零变更确认

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| AGENT-QUERY-2 | None | IDE-internal provider JSON + request-lifetime retrieval facts | 为 Work 增加本地对象搜索和最多两轮补查 | Internal .NET / Experimental provider JSON | Application exported allowlist、Gateway catalog/methods、Apply/Save authority 不变 |

Provider-visible `context_queries` additive 增加 `search_objects`、`search_text`、`entity_role`、
`accepted_kinds` 与 `maximum_results`；Host parser 继续兼容旧 `get_section/resolve_reference` 精确载荷。
新增 canonical binding、retrieval attempt/stop reason 和 pipeline result 均为 IDE internal、非序列化、
request-lifetime 数据。搜索复用 Application internal semantic builder，没有增加 public HLI query method。

## 26. AGENT-WORK-ENTRY-1 provider contract correction

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| AGENT-WORK-ENTRY-1 | None exported | IDE-internal parse result + Experimental provider tool guidance | 将描述元数据与最低安全权限分离 | Internal .NET / Experimental provider JSON | No Application API, persistence, Apply/Save or path authority change |

Provider-visible intent capability guidance additive 增加 `project-rules-art-edit`; context-query item schema 只要求
`kind/target`，其它字段保持可选有界提示。Host 接受附加属性和缺省描述字段，但仍只执行捕获快照中的
`current/rules/art`。`Ra2AiIntentAnalysisParseResult` 与 recovery notes 为 IDE internal、request-lifetime、
非序列化数据。所有生产 current-document typed routes 的第二轮工具改为既有 generic Document Plan，生产
typed SuperWeapon route 改为既有 generic Project Plan；旧 template API 仅保留 headless compatibility；
没有新增 exported type/method、文件格式、任意路径、Apply 或 Save 权限。Generic proposal 的
`summary/message` 明确降为非权威展示元数据：无效 summary 使用本地默认，message 忽略；clarification
仍要求可读 message。该变化不改变 executable operation shape 或 Host authority。

## 27. ASSET-VOX-1C-P1 public API candidate audit

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-1C-P1 | None | External provider adapter using existing internal protocol | Certify one real Hunyuan3D-2mini shape-only provider | Not implemented / authorization blocked | Existing Host interface, protocol v1, Application allowlist 77 and AssetHost exported public type count 0 remain frozen |

P1-0 is docs-only. The proposed adapter is a separately deployable implementation of the existing internal child-process
protocol, not a public provider SDK or plugin contract. No public API candidate is queued until a real provider proves
the boundary and a separate promotion decision is approved.

## 28. ASSET-VOX-1C-P2 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-1C-P2 | None | Internal executable adapter for existing child-process protocol | Tencent Hunyuan 3D remote Geometry candidate | Internal / provider-specific | AssetHost interface/protocol unchanged; provider exported types 0 |

The adapter's HTTP payloads and provider evidence JSON are private implementation/wire details, not application public
API or project persistence. No Application exported type, Gateway, Shell, editor, save or voxel-core surface changed.

## 29. ASSET-VOX-1D public API candidate audit

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-1D | None | Internal GLB reader, transient mesh facts and canonical voxel converter | Bridge certified GLB geometry into the existing 1B single-part truth | Implemented / R4 / verified | Application allowlist remains 77; AssetHost exports remain 0; no Gateway, persistence or project-write surface |

The proposed bridge consumes caller-owned bytes and returns existing internal canonical snapshot/facts. Product composition
between an AssetHost lease and Application remains a later separately reviewed seam; it is not a reason to expose 1D
implementation types or add a friend assembly.

## 30. ASSET-VOX-1E public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-1E | None | Application/IDE-internal style source, compiler, cache, plan, colourizer and review artifacts | Compile natural-language palette intent into deterministic headless voxel recolouring | Internal / implemented / verified | Application allowlist remains 77; AssetHost exports remain 0; no Gateway, project-write or UI surface |

`VOXEL_STYLE.md` and cache schema v1 are versioned authoring/derived-data conventions, not public .NET APIs. The dedicated
compiler reuses the internal AI client but is not registered in the INI Work route. All plan, mask, result and artifact
types stay internal; promoting them to a plugin/third-party contract requires a separate public API review.

## 31. ASSET-VOX-1E-UI public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-1E-UI | None | IDE-internal coordinator, view model, view and Shell composition | Expose existing 1E review artifacts in one product workspace | Internal / implemented / verified | Application allowlist unchanged; no provider, Gateway, persistence, project-write or plugin API delta |

`Document.VoxelStyle` and `VoxelStyle.*` are internal Shell/UI automation identities, not external extension APIs. Any
future accepted-preview handoff to AssetHost, a plugin, project persistence or VXL/HVA production requires a new ledger
entry and compatibility review.

## 32. ASSET-VOX-1E-UI-R2 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-1E-UI-R2 | None | IDE-internal input adapter and proposal normalization | Reuse existing Stage 1B VOX/VXL/PAL readers and Stage 1E plan compiler | Internal / implemented / verified | Application allowlist remains 77; no codec, provider, Gateway, persistence, project-write or plugin API delta |

## 33. ASSET-VOX-2A public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-2A | None | Application/IDE-internal quality facts, candidates and bounded AI coordinator | Improve conversion, symmetry review and palette contrast | Internal / implemented / verified | No exported API, serialized format, provider protocol, persistence or project-write delta |

All new data is request/session lifetime. `Ra2VoxelSceneSnapshot` remains the sole voxel truth; the AI coordinator cache
is process-memory only. Public promotion, plugin consumption or project materialization requires a later ledger review.

## 34. ASSET-VOX-2A-UI public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-2A-UI | None | IDE-internal candidate transaction, review projection and session composition | Expose existing 2A geometry/contrast candidates in the existing workspace | Internal / implemented / verified | No exported API, serializer, provider protocol, project write or persistence delta |

The new result/provenance records, ViewModel modes, presentation rows and `VoxelStyle.Quality.*` AutomationIds are
IDE-internal. `Ra2VoxelSceneSnapshot` remains the canonical data authority. Promotion to an extension API or persisted
workspace/project format requires a separate compatibility review.

## 35. ASSET-VOX-2A connectivity correction public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-2A connectivity correction | None | Internal candidate admission and existing-fact UI projection | Replace an absolute component-count rejection with dominant-body evidence | Internal / implemented | No exported type, serialized shape, snapshot schema, provider protocol, persistence or project-write delta |

## 36. ASSET-VOX-2B public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-2B | None | Internal derived evidence, compiler, partition, executor and workspace projection | Add explicit review-first semantic symmetry without making model output geometry authority | Internal / implemented | No exported type, serialized shape, multimodal/provider protocol, persistence, project write, VXL/HVA or Shell delta |
| ASSET-VOX-2B physical-sample correction | None | Internal region aggregation fact and typed IDE-local evidence result | Keep real fragmented geometry within the existing bounded classifier contract without dropping coordinates | Internal / implemented | No exported type, serialization, provider/tool schema, persistence, project write, VXL/HVA or Shell delta |
| ASSET-VOX-2B visual/provider correction | None | Internal candidate selection, tool-response normalization and presentation semantics | Make admitted refinement and explicit structure recognition usable without changing geometry authority | Internal / implemented | No exported API, serialized shape, provider tool schema, persistence, writer, VXL/HVA or Shell delta |

## 37. ASSET-VOX-3A experimental mesh-generation façade

| Task/Stage | API | Kind | Reason | Stability | Tests |
|---|---|---|---|---|---|
| ASSET-VOX-3A | `Ra2MeshGenerationFacade` plus request/result/progress/failure/image-format family | Public experimental in-process façade | Let the IDE use the existing out-of-process Host without exposing protocol DTOs, workspace paths or leases | Experimental / implemented | AssetHost 50/50; IDE 2831/2831 |

The façade accepts exactly one bounded PNG/JPEG reference, returns at most one owned GLB plus optional PNG, and disposes
the internal workspace lease before returning. It is not a persistence, project-apply, plugin or arbitrary-provider API.

## 38. ASSET-VOX-3C public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-3C | None | Application/IDE-internal working-state and existing-baseline refinement path | Preserve adopted geometry across repeated review/Agent passes | Internal / implemented / verified | No exported type, snapshot schema, persistence, provider protocol, writer or project-apply change |

The working revision/lineage data is workspace-session state. It was not added to
`Ra2VoxelSceneSnapshot`, exported VOX, project settings or a public façade. If implementation requires any such promotion,
the stage stops for a separate compatibility review.

## 40. ASSET-VOX-4D semantic sidecar v1 contract

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| ASSET-VOX-4D | `ra2-voxel-semantic-sidecar` version 1 | Project-contained serialized JSON shape | Persist accepted Agent, human region and human cell semantic authoring without changing geometry or palette authority | Reload matching semantic authoring in the existing Voxel Style workspace | Experimental / implemented / automated verified | Strict round-trip, negative schema/path/hash/resource tests and ViewModel atomic restore | No public C# API; exact snapshot/evidence/layer hashes; no migration, merge, autosave or writer embedding |

The R4 contract was approved on 2026-08-30 and implemented without adding a public C# API. The serialized shape is now
experimental persistence surface; incompatible changes require a new compatibility review and version decision.

## 39. ASSET-VOX-3D public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Notes |
|---|---|---|---|---|---|
| ASSET-VOX-3D | None | Application/IDE-internal seam evidence and Agent operation | Bridge bounded one/two-cell center seams without Host auto-fill | Internal / implemented / verified | No exported type, snapshot schema, persistence, provider transport, writer or project-apply change |

`bridge_center_gap`, `seam-gap-*` IDs and their evidence facts are internal request/session contracts. Promotion to a
serialized provider protocol, plugin API or persisted authoring history requires a separate compatibility review.

## 41. ASSET-VOX-4E-1 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| ASSET-VOX-4E-1 | None | Application/IDE-internal UnitClass, BaseColour, technique/adaptation, semantic requirement/binding and bundled Skill contracts | Establish the approved model/human/policy boundary before Provider or UI integration | 4E-2 classifier/cache/router/compiler; 4E-3 deterministic materialization | Internal / experimental / focused-verified | 13/13 new contract tests; 45/45 affected Application; 18/18 Skill catalog; 88/88 affected IDE | No public type, serializer, sidecar, Provider protocol, project-write, writer or XAML delta |

All new C# types remain internal and session/derived-only. Skill/Technique Markdown is bundled content, not a public .NET,
plugin or persistence API. Promotion to a public façade or serialized workspace format requires a separate compatibility review.

## 42. ASSET-VOX-4E-2 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| ASSET-VOX-4E-2 | None | IDE-internal classifier/cache/router/style compiler v2/binding and normalization identity | Implement the approved two-stage proposal path without changing external contracts | 4E-3 local materialization；4E-4 exact UI wiring | Internal / experimental / focused-verified | 26/26 focused；49/49 affected Application；107/107 affected IDE | No public type, project serialization, 4D sidecar, Provider/AssetHost protocol, project write, writer or XAML delta |

All new caches are discardable local derived data. Their JSON envelopes are not project persistence or public interchange
formats. Public promotion or cross-version cache compatibility would require a separate ledger and compatibility review.

## 43. ASSET-VOX-4E-3..4E-4 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| ASSET-VOX-4E-3..4E-4 | None | Application-internal palette family/materialization/quality and IDE-internal workspace projection | Complete the approved deterministic colour path and exact UI without widening external authority | 4E-5 physical acceptance and later separately contracted output work | Internal / experimental / automated-focused verified | 35/35 new materialization；77/77 affected Application；89/89 affected IDE；workspace UI/ViewModel 25/25 | No public type, 4D sidecar/project serialization, Provider/AssetHost protocol, project write, writer or Shell delta |

The quality JSON is an internal review-package artifact bound to a candidate hash, not a public interchange or persistence
format. The new AutomationIds are UI-test surface only. Promoting materialization, quality admission, class confirmation or
base-colour selection to a public façade/persisted project model requires a separate compatibility review.

## 44. ASSET-VOX-4E Rev.4 UI-R1 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Tests | Notes |
|---|---|---|---|---|---|---|
| ASSET-VOX-4E Rev.4 UI-R1 | None | Application/IDE-internal confirmation source, routing identity and session-only workspace stage | Replace active Provider classification with explicit human class selection and reorganize existing UI | Internal / focused-verified / physical pending | Application 39/39；IDE 39/39；Release XAML build passed | No public type, serializer, sidecar, Provider protocol, project write, writer, dependency or Shell delta |

`HumanManualSelection` and `SelectedWorkflowStage` remain internal session state. Promotion to persisted project state,
plugin/public API or cross-version interchange requires a separate compatibility review.

## 45. ASSET-VOX-4E Rev.5 / UI-R1-FIX2 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Tests | Notes |
|---|---|---|---|---|---|---|
| ASSET-VOX-4E Rev.5 / UI-R1-FIX2 | None | Application-internal indexed ramp/materialization/quality/surface coverage and IDE-internal preview/workflow projection | Correct real-palette colour quality and remove all-region completion requirement | Internal / focused-verified / physical pending | Application 353/353；workspace ViewModel 25/25；Debug solution build passed | No public type, serializer, 4D sidecar, Provider protocol, project write, writer, dependency, AutomationId or Shell delta |

The revised hashes invalidate only derived local colour candidates/reports. Persisted project and semantic-sidecar
compatibility is unchanged; public promotion still requires a separate compatibility review.

## 46. ASSET-VOX-4E Rev.6 public API zero-change confirmation

| Task/Stage | API | Kind | Reason | Stability | Tests | Notes |
|---|---|---|---|---|---|---|
| ASSET-VOX-4E Rev.6 | None | Application-internal directional geometry/semantic boundary/quality and IDE-internal preview projection | Correct side/end/under shading, distinguish techniques and restore classification preview | Internal / automated-verified / physical pending | Debug build；Application 358/358；AssetHost 50/50；IDE 2920/2920 | No public type, serializer, 4D sidecar, Provider protocol, project write, writer or Shell delta |

All new masks and metrics are derived session data. Technique/Skill revision changes invalidate derived local caches only;
the preserved AutomationIds and one new toolbar container do not create a public or persisted API.
