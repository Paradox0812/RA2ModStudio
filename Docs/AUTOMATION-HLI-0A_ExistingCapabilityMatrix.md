# AUTOMATION-HLI-0A Existing Capability Matrix

状态：Completed audit / No runtime implementation  
日期：2026-08-20  
权威契约：`Docs/AUTOMATION-HLI-0A_ExistingCapabilityAuditContract.md`

## 1. 结论摘要

当前项目已经具备高层接口路线所需的大部分算法基础：TextModel/语义模型、字段定义、诊断、当前文档引用、项目文本搜索、结构化编辑 Preview、显式 Apply 事务和可靠保存链路都是真实实现，不应重写。

当前主要缺口不是算法数量，而是边界位置：除 Core 字段 schema 外，多数可复用服务位于 `RA2IniEditor.IDE` 的 `net8.0-windows` WPF 程序集中，并且使用 internal DTO。它们可以在测试中不创建 UI 控件运行，但不能被独立 `net8.0` CLI、Job Runtime 或外部 Agent 宿主直接引用。

另有三项必须避免误判：

1. `Ra2ProjectSearchService` 是文本搜索，不是项目级语义引用服务；
2. A2 Preview 可无 UI 运行，但 A3 Apply 必须保持 IDE 主机所有权；
3. 当前没有正式的 Capability Registry、Gateway、Job、Event Bus、Artifact ID 或语义 Template Service。

因此，HLI-0B 应先冻结最小只读/Preview application contract，而不是直接把 A1-A4 internal 类型改为 public，也不是先建立大而全的 Automation Runtime。

## 2. 当前程序集事实

```text
RA2IniEditor.Core                 net8.0
        ^
        |
RA2IniEditor.Infrastructure       net8.0
        ^                         references Core
        |
RA2IniEditor.IDE                  net8.0-windows + WPF
                                  references Core + Infrastructure
```

| 程序集 | 当前职责 | 无头宿主可直接引用 | 审计结论 |
|---|---|---:|---|
| `RA2IniEditor.Core` | parser/validator/serializer、字段 schema 等纯领域事实 | Yes | 适合保留稳定领域模型，但不应塞入项目 I/O 或工作流编排 |
| `RA2IniEditor.Infrastructure` | 文件 I/O、字段包/provenance 等基础设施 | Yes | 可作为 application adapter 依赖，不应成为 Agent 直接入口 |
| `RA2IniEditor.IDE` | WPF Shell，同时承载大量 UI-neutral language/editing 算法 | No | 当前最大边界问题；不能因算法无控件依赖就称其为可独立消费 |

## 3. 能力实现与耦合矩阵

缩写：`A-headless` = `Algorithmically headless`，即算法无需创建 UI；
`Host-ready` = `Headless-host consumable`，即当前可被非 WPF 宿主直接项目引用。

| 能力面 | 当前实现与程序集 | 输入 / 结果与事实源 | WPF / Shell / Disk / mutable coupling | A-headless | Host-ready | 现有测试证据 |
|---|---|---|---|---:|---:|---|
| Project discovery | `RA2IniEditor.IDE/Services/ProjectOpenService.cs`, `ProjectOpenResult`; IDE | 文件夹路径 -> 顶层 `.ini` 描述；磁盘目录是事实源 | 无控件；直接目录枚举；位于 WPF 程序集 | Yes | No | `IdeProjectOpenBoundaryTests`、`ProjectExplorerViewModelTests` |
| Readonly document read | `RA2IniEditor.IDE/Services/ReadonlyIniContentService.cs`; IDE | 路径 -> 文本/加载状态；磁盘或当前编辑器 override | 磁盘 I/O；当前文档 override 由主机提供 | Yes | No | Search、Manual diagnostics 相关测试间接覆盖 |
| Text parse / semantic model | `RA2IniEditor.IDE/TextModel/Ra2IniTextDocumentParser.cs`, `RA2IniEditor.IDE/Language/Ra2DocumentSemanticModelBuilder.cs`; IDE | 精确文本 -> TextDocument/SemanticModel；输入文本是事实源 | 无控件/无磁盘；IDE assembly placement | Yes | No | A0、LanguageAnalysis、TextModel 测试 |
| Unified document analysis | `IRa2IniLanguageAnalysisService`, `Ra2IniLanguageAnalysisService`; IDE | `Ra2LanguageAnalysisRequest` -> `Ra2IniLanguageAnalysisResult`；文本 + Registry snapshot | 无控件/无磁盘，但诊断源先产生 `IdeDiagnosticIssueViewModel` 再映射为 neutral fact | Yes | No | `Ra2IniLanguageAnalysis*Tests` |
| Section / symbol query | `Ra2DocumentSemanticModel`; IDE | SemanticModel 中的 section/symbol facts | 无控件；没有独立 capability facade | Yes | No | Language、navigation、diagnostic tests |
| Definition query | `IRa2DefinitionProvider`, `Ra2DefinitionProvider`; IDE | SemanticModel + caret/selection + field/provenance providers | 无控件/无磁盘；跨 Core + Infrastructure；IDE placement | Yes | No | language navigation / definition boundary tests |
| Current-document reference query | `IRa2ReferenceFinder`, `Ra2ReferenceFinder`, `Ra2ReferenceResult`; IDE | 单个 SemanticModel + caret/selection -> 当前文档 references | 无控件/无磁盘；只覆盖一个 semantic model | Yes | No | `Ra2ReferenceFinderTests`、navigation tests |
| Project semantic reference query | 诊断目录可建立项目 reference catalog，但没有通用 query facade | 预期为 project snapshot + symbol -> cross-file definitions/references | 现有逻辑被诊断编排占用；不是正式查询 API | Partial | No | `Ra2ReferenceDiagnosticServiceTests` 仅证明诊断路径 |
| Field schema | `RA2IniEditor.Core/Schema/Ra2FieldSchema.cs`: `Ra2FieldDefinition`, `IRa2FieldDefinitionProvider`; Core | SectionKind/key -> immutable definition | 无 WPF/磁盘；Core public contract | Yes | Yes | Core schema、Field Registry tests |
| Effective Field Registry snapshot | `FieldRegistryRuntimeService` 及 A1 `Ra2FieldRegistryProviderSnapshot`; Infrastructure/IDE | Project > Global > BuiltIn effective provider + revision | runtime mutable service；Snapshot 本身只读；不得让 Agent解析全局单例 | Snapshot only | No | `FieldRegistryRuntimeServiceTests`、A1/A2 currency tests |
| Project text search | `RA2IniEditor.IDE/Search/Ra2ProjectSearchService.cs`; IDE | canonical project files + query + current text override -> ordered hits | 其他文件磁盘读；当前文件由主机内存覆盖；IDE placement | Yes | No | `Ra2ProjectSearchServiceTests`、Search ViewModel tests |
| Current-document diagnostics | `RA2IniEditor.IDE/Diagnostics/CurrentFileReadonlyDiagnosticService.cs`; IDE | current text/semantic/registry -> issues | 无磁盘，但公开返回 `IdeDiagnosticIssueViewModel`，存在 presentation coupling | Yes | No | `CurrentFileReadonlyDiagnosticServiceTests`、field/reference/chain tests |
| Project diagnostics | `RA2IniEditor.IDE/Diagnostics/ManualFullDiagnosticsService.cs`; IDE | project documents + current in-memory override -> aggregate issues | 磁盘 I/O + ViewModel result + orchestration | Yes | No | `ManualFullDiagnosticsServiceTests` |
| Semantic edit snapshot/plan | `Ra2AuthoringSnapshot`, `Ra2IniEditPlan`; IDE | exact current text, DocumentId/EditRevision, Registry revision + structured operations | Snapshot capture需主机提供 session/editor一致性；DTO 本身无 UI | Yes | No | A2 contract/currency tests |
| Semantic edit preview | `IRa2IniEditPreviewService`, `Ra2IniEditPreviewService`, `Ra2TextChangeSet`; IDE | Snapshot + Plan -> deterministic candidate, changes, evidence, diagnostic delta | 无 WPF/磁盘；依赖 IDE Language/FieldTrust/TextModel | Yes | No | `Ra2IniEditPreview*Tests` |
| Apply transaction | `IRa2IniAuthoringWorkspace`, `IRa2EditorTransactionPort`, A3 Shell transaction glue; IDE | active PreviewId + confirmation + live currency -> typed apply result | Shell/AvalonEdit/session/Undo/UI-thread authority | No | No | `Ra2IniAuthoringWorkspaceTests`、`Ra2AuthoringShellTransactionBoundaryTests` |
| Save/backup/write/rollback | `Ra2SaveCurrentFileService`, orchestrator, writer, rollback; IDE + Infrastructure I/O | editable session/save plan -> disk and updated session | 磁盘、backup、encoding、session ownership；不是 Agent 操作 | Service logic yes | No | `Ra2SaveCurrentFile*Tests`、`Ra2TextFirstFileWriterTests` |
| Built-in Agent proposal | `RA2IniEditor.IDE/AI/Ra2AiAuthoringCoordinator.cs` + A4 adapter/runner; IDE | provider tool args -> locally validated A2/A3 proposal | AI lifecycle、Shell proposal card、official endpoint policy | Coordinator core yes | No | `Ra2AiAuthoringCoordinatorTests`、A4-R1 tests |
| Semantic template | No formal `ITemplateService` or neutral template model | N/A | 搜索到的 template 主要是 WPF templates、prompt draft 或测试命名 | N/A | No | None |
| Capability registry/gateway | NotPresent | N/A | 无 `CapabilityDescriptor` / `CapabilityGateway` | N/A | No | None |
| Job runtime/state | NotPresent | N/A | 无 `AutomationJob` / `JobState` | N/A | No | None |
| Event/artifact transport | NotPresent | N/A | 无 `IEventBus` / `ArtifactId` | N/A | No | None |

## 4. 复用决策矩阵

下列 capability ID 都是 HLI-0B 的候选标识，不是已存在的 public API 或 wire protocol。

| Candidate capability ID | 用户能力 | 当前覆盖度 | Reuse decision | 最小迁移 | 风险 | 首次建议使用 |
|---|---|---:|---|---|---|---|
| `project.files.list` | 获取当前项目 canonical INI 文件清单 | Full algorithm | `AdapterOnly` | 由 application contract 接收 project snapshot；磁盘枚举留在 host/infrastructure adapter | R2 | HLI-1A |
| `project.document.read` | 读取项目内受限文档文本 | Full algorithm | `ExtractContract` | 路径根约束、大小限制、encoding/failure kind 显式化；当前编辑文档由 host snapshot 覆盖 | R3 | HLI-1A |
| `project.section.get` | 按文档/Section 定位语义片段 | Partial facade | `ExtractContract` | 在中立层定义 request/result，复用现有 parse + SemanticModel，不公开 IDE model | R2 | HLI-1A |
| `project.definition.get` | 查询定义与字段来源 | Full algorithm | `MoveImplementation` | 迁移 UI-neutral provider orchestration；输入使用 captured registry/provenance snapshot | R2 | HLI-1A |
| `project.reference.find` | 项目级语义引用查询 | Current-document only | `ExtractContract` | 先复用 current-document finder；项目级 catalog/query 需单独实现与上限/取消契约 | R3 | HLI-1A limited / later project scope |
| `project.text.search` | 项目文本搜索 | Full algorithm | `MoveImplementation` | 从 IDE 抽离 matcher/order/cancel；磁盘读取由 adapter 注入；不冒充 reference query | R2 | HLI-1A optional |
| `project.field_schema.get` | 查询字段类型、可信度和来源 | Full | `ReuseAsIs` + snapshot adapter | 复用 Core schema；禁止直接暴露可变 `FieldRegistryRuntimeService` | R2 | HLI-1A |
| `diagnostic.validate.document` | 对指定文本运行只读诊断 | Full algorithm | `MoveImplementation` | 先用 neutral diagnostic fact/result 替换 ViewModel 返回；保持现有规则唯一实现 | R3 | HLI-1A |
| `diagnostic.validate.project` | 对项目 snapshot 运行诊断 | Full IDE workflow | `AdapterOnly` | application orchestration + infrastructure reader；current document override 明确化 | R3 | HLI-1A later increment |
| `semantic.value.set.preview` | 对当前文档生成结构化字段变更 Preview | Full for A2 operations | `MoveImplementation` | 迁移 neutral Snapshot/Plan/Preview；复用 A2 planner，不新增 generic patch | R3 | HLI-1B |
| `semantic.proposal.apply` | 把 Preview 应用到活动编辑器 | Full host path | `Deferred` / host-only | 不注册为 Agent capability；Gateway 只返回 proposal handle，由 IDE UI 调用现有 A3 | R3/R4 | HLI-1C host boundary |
| `file.save` / `file.write` | 保存或直接写磁盘 | Full host path | `Deferred` / forbidden | 保持用户/Shell 所有权，不进入早期 Gateway | R4 | Not before separate contract |
| `agent.edit.propose` | 将模型输出转换为本地 proposal | Full for A4 scope | `AdapterOnly` | A4 作为 Gateway consumer；不把 provider DTO 变为核心能力模型 | R3 | HLI-2B |
| `template.*` | 语义模板实例化 | None | `NotPresent` | 先定义真实产品用例、模板事实源和参数 schema；不得从 WPF Template 推断 | R3 | Later |
| `capability.*` | 注册/发现/调用能力 | None | `NotPresent` | HLI-2A 只实现已冻结的最小 registry/gateway | R3 | HLI-2A |
| `job.*` | 长任务、状态、取消、恢复 | None | `Deferred` | 等核心能力稳定后再定义 Job state/event/artifact | R3/R4 | HLI-3 |
| `runtime.*` / `asset.*` / `test.*` | 运行时、资产和测试自动化 | None in this IDE scope | `Deferred` | 需要独立领域契约和宿主；不可借用 INI editor 文件能力 | R4 | HLI-3+ |

## 5. 建议的所有权分层

```text
Agent / AI adapter / future CLI
              |
              v
  Automation Capability Gateway       (future; request routing only)
              |
              v
  UI-neutral Application contracts     (query + diagnostics + preview)
       |                         |
       v                         v
  Core semantic facts       Infrastructure adapters
                                  (disk/provenance)

  IDE Host only:
  active editor capture -> user confirmation -> A3 Apply -> Undo -> user Save
```

必须保持的单一权威：

- 文本事实由显式 Document/Project snapshot 拥有；
- 字段事实由 captured effective provider snapshot + revision 拥有；
- Preview 由 application preview service 生成；
- 活动 Preview/单次消费由现有 workspace/host 拥有；
- Apply 和 Undo 由 IDE transaction port 拥有；
- disk Save/Backup/Rollback 由现有保存链路拥有；
- Agent 仅生成/调用受限能力，不拥有文件、Session、Registry singleton 或 UI 控件。

## 6. 程序集落点方案

| 方案 | 结构 | 收益 | 成本/风险 | 审计建议 |
|---|---|---|---|---|
| A. 新建 `RA2IniEditor.Application` (`net8.0`) | references Core；按需通过中立接口使用 Infrastructure；IDE references Application | 最清晰地区分领域事实、应用编排和 WPF host；未来 CLI/Job 可复用 | 新项目/依赖方向/public contract 均需 R3 契约；迁移测试量较大 | **HLI-0B 推荐候选，尚未批准** |
| B. 将 UI-neutral 服务移入 Core | Core 同时承载 language/editing orchestration | 项目更少，短期接入快 | Core 会吸收 provenance、diagnostics orchestration、snapshot workflow；职责膨胀且可能反向依赖基础设施 | 只适合纯领域 facts/algorithms，不建议整体采用 |
| C. 继续留在 IDE，用 adapter 包装 | 新 Gateway 仍引用 IDE | 改动最少 | 无头宿主仍需 `net8.0-windows`/WPF；无法实现文档路线中的独立 Automation host | 仅可作为短期 IDE 内桥，不适合长期架构 |

推荐 HLI-0B 评审方案 A，但不一次迁移所有服务。先定义最小中立 contract，然后按 `section/field/diagnostic -> preview` 的依赖顺序迁移或适配，每一步保持 IDE 原调用路径和测试等价。

## 7. HLI-0B 建议最小面

首个 contract package 建议只冻结四个读取/预览能力：

1. `project.section.get`
2. `project.reference.find`（首版必须明确是 current-document 还是 project scope）
3. `diagnostic.validate.document`
4. `semantic.value.set.preview`

配套但不直接作为 Agent 操作的基础数据：

- `ProjectSnapshotId` / `DocumentSnapshotId`
- document version / registry revision
- typed `CapabilityFailureKind`
- deterministic limits、cancellation 和 result truncation facts
- capability descriptor/version/stability（仅进程内；wire DTO 后置）

HLI-0B 必须明确排除：

- `semantic.proposal.apply`
- `file.save` / `file.write`
- `project.multi_file.edit`
- `runtime.*` / `asset.*` / `test.*`
- 任意路径访问、任意文本 patch、Shell command、WPF/AvalonEdit handle

## 8. 迁移顺序与回归保护

| 后续阶段 | 目标 | 必须复用 | 禁止返工点 |
|---|---|---|---|
| HLI-0B | 冻结最小 application/capability contract | 本矩阵事实、现有 failure/currency 语义 | 不写实现、不决定 wire protocol |
| HLI-1A | 无头只读 query facade | A1 analysis、Core schema、现有 finder/diagnostics | 不复制 parser/diagnostic 规则 |
| HLI-1B | 无头 semantic preview facade | A2 Snapshot/Plan/Preview/ChangeSet | 不新增第二套 planner，不支持 generic patch |
| HLI-1C | host-owned apply adapter | A3 workspace/transaction/Undo | Apply 不进入 Agent capability，不自动 Save |
| HLI-2A | 最小 registry/gateway | 已冻结 capability descriptors | 不先建 Job/Event 大框架 |
| HLI-2B | A4 adapter 改为 Gateway consumer | 当前 endpoint/tool/reliability policy | provider DTO 不成为 application DTO |
| HLI-2C | 首个端到端可用闭环 | query -> preview -> user apply | 单文件、显式确认、内存修改 |
| HLI-3+ | CLI/Job/Event/assets/tests | 稳定 Gateway + 独立契约 | 不扩大 RA2IniEditor.IDE-only 产品边界而无新授权 |

## 9. 已识别债务与风险

| ID | 事实 | 影响 | 建议处理阶段 |
|---|---|---|---|
| `HLI-TD-001` | UI-neutral language/editing algorithms 位于 WPF IDE assembly | 独立宿主不可引用 | HLI-1A/1B 分批迁移 |
| `HLI-TD-002` | Current diagnostics 主入口返回 `IdeDiagnosticIssueViewModel` | presentation DTO 污染 application boundary | HLI-1A neutral result extraction |
| `HLI-TD-003` | A4 coordinator 同时承载 provider proposal 生命周期 | 若直接公开会把 DeepSeek/UI policy 泄漏到 Gateway | HLI-2B adapter-only |
| `HLI-TD-004` | A1 diagnostic reuse 可重复构建 SemanticModel（既有 `AGENT-AUTHORING-A1-TD-001`） | 性能债，不影响正确性 | 继续受控；不在接口迁移时顺手重写 |
| `HLI-TD-005` | `ProjectOpenService`/`ProjectOpenResult` 是 IDE assembly 中的 public 类型 | public 不等于中立或稳定 Automation contract | HLI-0B 不复用其 public surface |
| `HLI-GAP-001` | 没有通用项目级 semantic reference query | `project.reference.find` 不能宣称完整 | HLI-0B 明确首版 scope |
| `HLI-GAP-002` | Template/Capability/Job/Event/Artifact infra 不存在 | 外部路线不能从名称推断实现 | HLI-2/HLI-3 独立契约 |

## 10. Public API Ledger 预登记

本阶段 public API 变更：**None**。

以下只作为 HLI-0B 待审候选，不代表名称、程序集或序列化形状已批准：

| Candidate | Kind | Stability | Owner | 当前状态 |
|---|---|---|---|---|
| application query request/result/failure | internal or public contract candidate | Experimental | future Application layer | Pending HLI-0B |
| semantic preview request/result | internal or public contract candidate | Experimental | future Application layer | Pending HLI-0B |
| capability descriptor/id/version | in-process contract candidate | Experimental | future Gateway | Pending HLI-0B/HLI-2A |
| wire DTO / IPC protocol | serialized public protocol | Unspecified | future external host | Explicitly deferred |

## 11. 审计结论

- Reuse：通过。A1-A4、Search、Field Schema 和 Save 都有真实可复用实现。
- Boundary：发现关键缺口。算法可无头与宿主可消费必须分开，不能直接公开 IDE internal 类型。
- Ownership：通过。Preview、Apply、Save 的所有权可以沿用现有单一路径。
- Gap honesty：通过。Template、Capability、Job、Event、Artifact 明确为不存在/后置。
- Anti-rework：通过。推荐先冻结 application contract，再逐项迁移；不提前决定外部协议或大运行时。
- Next gate：`AUTOMATION-HLI-0B Minimum Capability Contract`，需单独确认后才能进入任何代码实现。
