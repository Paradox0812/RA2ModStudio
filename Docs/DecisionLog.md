# RA2IniEditor.IDE Decision Log

本文件只记录长期方向和边界决策。实现事实与验证结果仍由 CurrentCapabilities、
CurrentPhase 和对应 Stage Ledger 负责。

## Decision: 自然语言驱动的完整 Mod 内容生产是产品北极星

- Status: Accepted
- Date: 2026-08-22
- Task(s): Project documentation rebaseline
- Context:
  - 当前 IDE 已具备 INI 编辑和受限 AI 提案能力，但用户最终目标覆盖 INI、图标、
    VOX/VXL、SHP 与它们之间的装配。
- Decision:
  - 用户以自然语言表达需求，Agent 通过可审查、可追踪的能力流水线完成素材、
    图标和 INI 的创建、预览、绑定与验证。
- Rejected Alternatives:
  - 只把 AI 做成生成说明或代码块的聊天侧栏；不能满足真实内容生产目标。
  - 为每种产物建设彼此独立、无法编排的工具孤岛；会造成事实和事务分裂。
- Consequences:
  - 需要统一 Capability、Artifact、Job 和 Assembly 边界；近期优先 Headless INI 能力。
  - 当前尚未实现的资产能力必须明确标为路线，而不是产品现状。
- Follow-up:
  - HLI-0B 确认后进入 HLI-1A0；素材阶段在 Gateway 稳定后单独契约。

## Decision: 实际工程文件是唯一项目事实源

- Status: Accepted
- Date: 2026-08-22
- Task(s): Automation architecture alignment
- Context:
  - Agent、CLI、IDE 和素材流水线都需要共享项目状态。
- Decision:
  - INI、MAP 和真实素材文件保持事实源；索引、图和 Manifest 是可重建投影或产物记录。
- Rejected Alternatives:
  - 新增 `.iproj` 或数据库作为必须同步的第二套项目真相。
- Consequences:
  - 所有能力必须从显式 snapshot 工作；写入必须回到现有文档/文件事务。
- Follow-up:
  - CONTENT/ASSEMBLY 阶段定义投影失效与重建规则。

## Decision: Agent 只协调能力，不直接成为写入权威

- Status: Accepted
- Date: 2026-08-22
- Task(s): AGENT-AUTHORING-1-R1, AUTOMATION-HLI
- Context:
  - Provider 输出、WPF 状态和磁盘保存分别属于不同信任域。
- Decision:
  - Agent 调用版本化 capability；本地语义服务生成 Preview；IDE host 拥有 Apply/Undo；
    Save/Backup/Rollback 继续由现有保存链路拥有。
- Rejected Alternatives:
  - 让模型直接写文件、持有 WPF ViewModel 或调用任意 Shell 命令。
- Consequences:
  - “自动完成”需要显式自动化 policy，不能通过跳过事务实现。
- Follow-up:
  - HLI-2A 冻结 Gateway；AUTOMATION-1 冻结授权和 Job policy。

## Decision: Headless 能力采用最小纵向迁移，而不是整体搬迁

- Status: Accepted
- Date: 2026-08-22
- Task(s): AUTOMATION-HLI-0B
- Context:
  - 可复用算法目前大量位于 `net8.0-windows` IDE assembly，独立 Agent/CLI 无法引用。
- Decision:
  - `RA2IniEditor.Application` (`net8.0`) 只依赖 Core，按 Query、Diagnostics、
    Preview 纵向迁移，并让 IDE 消费同一个权威实现。
  - HLI-1A1 的首个 Query 切片只移动已特征化的 22 个语义基础文件；这些类型保持
    internal，通过精确 `InternalsVisibleTo` 和项目级 global using 兼容现有调用方。
  - 新建独立 `RA2IniEditor.Application.Tests` (`net8.0`) 证明真正的无 WPF 边界；
    现有 Windows 测试工程继续承担 IDE integration 验证。
- Rejected Alternatives:
  - 复制算法、Gateway 直接引用 WPF IDE、一次性移动整个 Language/Editing 目录。
- Consequences:
  - 未来实现是 R3，公共高层查询 DTO/API 是 R2；二者需要独立最终契约。
  - raw SemanticModel、Classifier result 和 symbol 不成为 Gateway public API。
  - Diagnostics 的 ViewModel 解耦后置到 HLI-1A2，完整 TextModel/Preview 后置到 HLI-1B。
- Follow-up:
  - HLI-0B/HLI-1A0 已完成；HLI-1A1 最终契约已生成，等待生产实施确认。

## Decision: HLI-1A1 只公开 Experimental 高层查询事实

- Status: Accepted
- Date: 2026-08-22
- Task(s): AUTOMATION-HLI-1A1
- Context:
  - Section/Reference 的唯一权威算法需要移入 Application，但约 100 个既有调用文件
    仍直接使用 raw SemanticModel、symbols 和 caret context。
  - HLI-0B 对 required occurrence 与 AmbiguousSection 的要求存在表面冲突；重复同名
    Section 的 field 归属也不能只按名称判断。
- Decision:
  - 22 个算法文件移入 Application 后继续保持 internal，通过精确 IVT 和 project-level
    global using 供 IDE/测试消费。
  - Agent/Gateway 只看到 `RA2IniEditor.Application.Automation.Experimental` 中的
    15-type allowlist，不公开 raw model。
  - Section request 使用 nullable occurrence：null 要求唯一，非负值精确选择；字段按
    选中 Section body span 隔离。
  - Query 限制为 8,388,608 chars 和 10,000 result items；取消和 limit failure 不返回
    partial payload。
- Rejected Alternatives:
  - Public 化 raw model：会把内部解析结构永久变成外部兼容负担。
  - 复制 parser/reference 到新 service：会产生第二套语义权威。
  - 保留 IDE/Application 双份实现：回滚容易但必然漂移。
  - 为避免 AmbiguousSection 强制 occurrence：无法支持调用方显式要求唯一的安全查询。
- Consequences:
  - 实施为 R3/R2，必须用 reflection allowlist、Headless tests 和完整 IDE regression 门禁。
  - 首版每次调用重建 invocation-local SemanticModel；不引入隐藏 cache/session。
- Follow-up:
  - HLI-1A1 已完成并通过 headless/full/package 门禁；Diagnostics 继续留给 HLI-1A2，
    需先做只读依赖回归和最终契约。

## Decision: HLI-1A2 扩展现有 Document Query service 并保留 IDE 单向适配

- Status: Accepted
- Date: 2026-08-22
- Task(s): AUTOMATION-HLI-1A2
- Context:
  - 当前 structure/field/reference/chain 规则无盘读，但位于 IDE 且直接构造
    `IdeDiagnosticIssueViewModel`。
  - HLI-0B 已将 document diagnostics 定位为 Query service 的第三个能力；HLI-1A1
    已建立可复用 document/registry snapshot。
- Decision:
  - 原子迁移 9 个 Diagnostics/FieldTrust/neutral-fact internal 文件到 Application，
    建立唯一 neutral diagnostic core。
  - 在现有 `IRa2AutomationDocumentQueryService` 上增加 `Validate`，只新增
    result/failure/fact 3 个 Experimental public types，不新建 service/request DTO。
  - IDE `CurrentFileReadonlyDiagnosticService` 保留 public 兼容入口，但只负责
    Host snapshot、legacy failure 和 ViewModel 投影。
  - project I/O、Problems UI、Save Preflight 和 Apply/Save 权威继续留在 IDE。
- Rejected Alternatives:
  - 新建 `IRa2AutomationDiagnosticsQueryService`；会违背 HLI-0B 的最小 service 边界并增加
    Gateway 注册膨胀。
  - 公开 `IdeDiagnosticIssueViewModel` 或 raw SemanticModel/catalog；会把 presentation/internal
    结构固化为外部契约。
  - 在 Application 重写一套诊断规则；会产生双权威和长期漂移。
- Consequences:
  - 实施是 R3/R2，必须保持 149 项现有行为回归，增加 headless tests 和精确
    18-type reflection allowlist。
  - 对 Experimental interface 加方法对未知自定义 implementer 有兼容风险；仓库内
    只有唯一生产实现。
  - A1 的完整 TextModel orchestration 和双解析性能债务不在迁移中顺手改写。
- Follow-up:
  - HLI-1A2 已完成并通过 headless、149 项依赖集、完整回归和 clean package 门禁；
    当前停止，HLI-1B 另行事实回归和契约。

## Decision: HLI-1B 迁移唯一 Preview 权威并保留 Host Apply 所有权

- Status: Accepted / implemented and verified
- Date: 2026-08-22
- Task(s): AUTOMATION-HLI-1B
- Context:
  - A2 已有纯内存、单文档结构化预览，但算法、TextModel 和 ChangeSet 位于 WPF IDE
    assembly，独立 Agent 无法引用。
  - A3/A4 已正确拥有 active Preview、显式确认、live currency、Apply/Undo 和用户展示；
    这些职责不应因 Headless 抽离而下移。
  - HLI-1A1/1A2 已提供 neutral SemanticModel、Diagnostics、FieldTrust 和 common snapshot。
- Decision:
  - 6 个 TextModel 和 2 个 TextChange 文件原子迁入 Application internal，旧路径删除；
    line insertion 抽为唯一 internal primitive，供 IDE AddProperty 与 Preview 共用。
  - A2 semantic planner 和 diagnostic delta 迁入 Application 唯一 engine；IDE 只保留 Host
    snapshot 投影、中文 presentation 和 A3/A4 compatibility wrapper。
  - 新增精确 11 个 Experimental public types，allowlist 从 18 变为 29；public service
    只有 Preview，无 Apply/Save/store。
  - public Preview 使用 8,388,608 chars、10,000 diagnostics、typed cancellation/limit；
    IDE compatibility path 复用同一 engine 但不静默改变既有 Host budget。
- Rejected Alternatives:
  - 在 Application 复制一套 A2 planner/TextModel；会形成第二语义权威。
  - 把 raw TextModel/SemanticModel 或 Host Preview 直接 public；会固化内部生命周期。
  - 同时迁移 A3/A4/Save；会混淆 Preview 与提交权威并扩大 R3 风险。
  - 为减少 adapter 直接让 Gateway 调用 EditorTransactionPort；会绕过用户确认和 live currency。
- Consequences:
  - 实施为 R3/R2；迁移前 Application 47/47、受影响 88/88 基线通过，迁移后
    Application 82/82、受影响 88/88、TextModel 相关 390/390 和完整 2526/2526 通过。
  - TextModel 移动影响 Save/Search/Completion 的编译依赖，需 global using 和受影响回归；
    不授权行为重构。
  - HLI-1C 只确认 Host adapter 与 A3/A4 生命周期，不再设计第二套 Preview 数据模型。
- Follow-up:
  - HLI-1B 已完成并停止；下一步只允许进入 HLI-1C Host Boundary Confirmation 的
    代码事实回归与最终契约，不自动进入 Gateway 或新增写入通道。

## Decision: HLI-1C 复用 Workspace 包围式 Preview seam，不新增结果注册旁路

- Status: Accepted
- Date: 2026-08-22
- Task(s): AUTOMATION-HLI-1C
- Context:
  - HLI-1B 已让 Application 产生 UI-neutral Preview result；未来 Gateway consumer 仍需
    进入 A3 active slot、显式确认、live currency 和 single-use Apply。
  - `Ra2IniAuthoringWorkspace.Preview` 已在调用 injected preview service 之前建立 generation，
    并只接纳当前代次的成功结果。
- Decision:
  - 未来 IDE Gateway adapter 实现现有 internal `IRa2IniEditPreviewService`，由 Workspace
    包围整个 invocation；adapter 只负责 Gateway 调用与 `FromAutomation` Host 投影。
  - `PreviewId` 只有进入当前 Workspace active slot 后才是一次性 Apply 身份，不是全局
    proposal handle、能力令牌或持久化键。
  - HLI-1C 只增加两处 internal Host 完整性 guard、永久边界测试和治理文档；public API
    diff 为 0，Shell/Gateway/Apply 行为不变。
- Rejected Alternatives:
  - 新增 `RegisterPreview/AdoptPreview`：会绕开 invocation-start generation，使旧异步结果
    可能覆盖新 active slot。
  - 建立 public/global Preview store：会把 Host 生命周期下移并制造新的状态权威。
  - 在 Gateway 暴露 Apply：会泄漏 live editor、确认、Undo 和 Save 边界。
  - HLI-1C 提前实现 Gateway adapter：HLI-2A descriptor/invocation 尚未冻结，会制造临时代码。
- Consequences:
  - HLI-2A 只需定义 typed capability routing；HLI-2B 再实现 IDE adapter，不需要改 A3。
  - 当前 Host unlimited policy 与 public 8M/10k policy 的切换必须在 HLI-2B 明确决策。
  - 若 HLI-1C tests 暴露两处已批准 guard 之外的生产缺口，必须停止并形成 R3 修订契约。
- Follow-up:
  - HLI-1C-0..1C-4 已完成并通过 82/82、53/53、2537/2537；下一步只进入 HLI-2A
    Capability Gateway 的代码事实审计与最终契约，不自动实现 Gateway。

## Decision: VXL 近期通过 VOX 二维切片和 VXLSE III 完成

- Status: Accepted
- Date: 2026-08-22
- Task(s): Asset generation route clarification
- Context:
  - 用户确认 VXLSE III 已具备通过二维切片构建 VXL 的实际工作流。
- Decision:
  - 首版生成 VOX、无损 SliceStack 和导入 Manifest，由 VXLSE III 完成最终 VXL/HVA
    保存、法线、边界和人工修整。
- Rejected Alternatives:
  - 近期直接开发完整 VOX->VXL 二进制编译器。
- Consequences:
  - 产物必须明确区分 VOX、SliceStack、VXLSE import package 和最终 VXL。
- Follow-up:
  - ASSET-VOX-1 用真实导入样本冻结 axis/order/pivot/palette 契约。

## Decision: HLI-2A 采用固定目录与强类型 Gateway，而非动态 dispatcher

- Status: Accepted / implemented and verified
- Date: 2026-08-22
- Task(s): AUTOMATION-HLI-2A-0
- Context:
  - HLI-1 已完成四项 UI-neutral 能力和 29 个 Experimental 类型，但当前没有生产
    Gateway、descriptor、registry 或 transport。
  - 长期自动化架构还需要 Job/Event/Artifact、wire schema、权限和 tracing；这些数据模型
    尚未冻结，也不是 HLI-2B 内置 AI consumer 的前置必需。
- Decision:
  - HLI-2A 只新增固定四项 immutable capability catalog 与 typed Gateway façade，直接委托
    现有 DocumentQuery/EditPreview service。
  - descriptor 只公开 ID、version、Query/Edit risk、Experimental stability 与现有限制。
  - 不增加 generic `Invoke`、mutable registry、wire schema、统一 failure、Apply/Save 或状态。
- Rejected Alternatives:
  - `Invoke(string, object/dynamic)` 或 reflection router：牺牲编译期边界并迫使提前设计统一
    failure/serialization。
  - 将 `Ra2AiAuthoringToolCatalog` 提升为 Gateway：它是 provider-specific 模型输出 schema，
    不能成为 Application 领域契约。
  - 同时实现 Job/Event/Artifact/permissions：会把当前 R2/R3 切片升级为缺少消费者的 R4 框架。
- Consequences:
  - 首版 Gateway 适合进程内 IDE/Agent/CLI host，尚不声明 wire compatibility。
  - 新增 public 类型候选精确为 6，allowlist 预期 29 -> 35。
  - HLI-2B 仍需单独决定 public 8M/10k budget 与现有 Host budget 的产品兼容策略。
- Follow-up:
  - HLI-2A-1..2A-4 已完成并通过 94/94、2537/2537 与 clean package 门禁；HLI-2B 先审计
    IDE consumer 和 public/Host budget 差异，不自动修改 A4 policy。

## Decision: HLI-2B 统一采用 Gateway public budget，并在 provider 前 fail closed

- Status: Accepted / implemented and verified
- Date: 2026-08-22
- Task(s): AUTOMATION-HLI-2B-0
- Context:
  - 内置 AI 当前经唯一 IDE Host adapter 调用 `PreviewForHost`，资源预算为 `int.MaxValue`；
    HLI-2A Gateway public Preview 为 8,388,608 chars / 10,000 diagnostics / 128 operations。
  - 只替换 adapter 会让超限明确编辑在真实模型请求完成后才失败，产生可预先避免的调用。
- Decision:
  - 原位把现有 `Ra2IniEditPreviewService` 改为 typed Gateway consumer，不新增第二 adapter。
  - 内置 AI 明确采用 Gateway public budget；consumer 切换后删除 internal `PreviewForHost`。
  - Shell composition root 持有并注入同一 Gateway instance，在 provider send 前从 Preview
    descriptor 读取限制并 fail closed；普通 advisory 仍使用现有截断上下文，不因超大文档被禁用。
  - Workspace admission、A4 policy、explicit Apply、Shell transaction/Undo 和 Save authority不变。
- Rejected Alternatives:
  - 超限时回退 `PreviewForHost`：descriptor 与执行不一致并保留双预算旁路。
  - 新增 public Host budget overload：扩大 Experimental API 且允许 caller 自行放大资源限制。
  - 只在 provider 返回后处理 `DocumentTooLarge`：正确性尚可，但成本门禁不足。
  - 新增平行 Gateway adapter：违反 HLI-1C 唯一 admission seam 并制造后续删除工作。
- Consequences:
  - 超 8 MiB 当前文件的 AI 结构化编辑将被本地拒绝且不发送；这是显式兼容性收窄。
  - Application public API 保持 35 个 exported types；不增加新的 failure/DTO。
  - Shell 需要一处经最终契约精确批准的 code-behind preflight，但 XAML/transaction/Save 零 diff。
- Follow-up:
  - HLI-2B-1..2B-4 已完成；94/94、78/78、2547/2547 与 clean package 门禁通过。
  - 下一阶段 HLI-2C 先审计首个高层 Agent 闭环，不在本阶段扩大 public Apply/Save。

## Decision: HLI-2C 复用 Gateway 与现有 Coordinator，不新增 Agent façade

- Status: Accepted / implemented and verified
- Date: 2026-08-23
- Task(s): AUTOMATION-HLI-2C-0..2C-4
- Context:
  - public Gateway 已提供 Agent-facing Query/Validate/Preview；internal AI Coordinator 已拥有
    provider proposal、policy 与 explicit Apply lifecycle。
  - 当前缺少完整闭环证据，且成功 Apply 后 Problems 不会立即刷新。
- Decision:
  - HLI-2C 只补 Query/Validate -> provider plan -> Gateway Preview -> explicit Apply -> post-apply
    diagnostics 的确定性 scenario，以及一个 Shell success-branch refresh。
  - public API 保持 35；不新增 Agent/Workflow/Session/Trace façade。
- Rejected Alternatives:
  - 立即公开 `IAgent` 或 `AgentWorkflow`：外部 host 的 permission/session/wire 尚未冻结。
  - 把 Apply/Save 放入 Gateway：会泄漏 live editor、Undo、backup/rollback authority。
  - 为制造 caller 强制重写 Prompt/Problems/Language UI：不增加 Agent 能力且扩大回归面。
- Consequences:
  - HLI-2C 完成后可关闭 Minimum HLI-v1，但只能宣称当前文件最小闭环。
  - 独立 Agent/CLI、模板、多文件、Job/Artifact、素材与 Runtime Test 仍需独立阶段。
- Follow-up:
  - HLI-2C 已通过 94/94、37/37、2549/2549 和 clean package 1123；下一阶段先审计
    独立 Agent Host 与 CONTENT-1 的优先级，不自动扩大 Gateway authority。

## Decision: CONTENT-1 先于独立 Agent Host，素材侧继续后置

- Status: Accepted / route ordering only
- Date: 2026-08-23
- Task(s): AUTOMATION-POST-HLI-0
- Context:
  - HLI-v1 已提供进程内 typed Gateway，但 snapshot 仍携带
    `IRa2FieldDefinitionProvider`，不是 wire DTO。
  - 当前没有 CLI/IPC/RPC、session、permission、audit 或 IDE-mediated external Apply protocol。
  - 长期语义面仍缺 Field Schema query、ResolveReference、CreateSection 和 ApplyTemplate。
- Decision:
  - 连续路线采用 `CONTENT-1 -> HOST-1 -> ASSET`。
  - CONTENT-1 先复用 canonical Application semantics 补齐 query、模板展开、新 Section Preview
    与既有 IDE explicit Apply；随后才冻结独立 Host 的 wire/session/permission。
  - 素材 provider 的 INI binding 必须回到 Semantic Edit Preview，不得直接修改文本。
- Rejected Alternatives:
  - Host first：会围绕不完整的四能力目录冻结协议，并在 CONTENT-1 后再次扩展。
  - Asset first：会在缺少 template/Section/reference binding 时制造字符串拼接或第二写入权威。
  - Generic raw command/patch：会绕过 typed failure、diagnostic delta 和 Host-owned Apply。
- Consequences:
  - 下一安全入口是 CONTENT-1A 的代码事实回归与最终契约；本决策不批准具体 public API。
  - 独立 Host 维持 R4 后置，Apply/Undo/Save 继续由 IDE Host/User authority 拥有。
- Follow-up:
  - CONTENT-1A/1B 分别评审 `GetFieldSchema` 与 `ResolveReference` 的 current-document typed
    query；CreateSection、模板写入、wire、Job/Event/Artifact 和素材实现不得提前进入。

## Decision: CONTENT-1 模板编译到现有 EditPlan，并诚实保留未知引用类型

- Status: Accepted / implemented and verified
- Date: 2026-08-23
- Task(s): AUTOMATION-CONTENT-1A..1F
- Context:
  - Field Registry 能表达 Reference/ReferenceList，但没有 target SectionKind schema。
  - 现有 Preview/Workspace/Transaction 已是唯一安全编辑链；另建 Template Preview 会形成第二权威。
- Decision:
  - Schema query 使用 captured effective provider 和既有 FieldTrust classifier，不暴露 provider/singleton。
  - ResolveReference 区分 SemanticKnown 与 FieldSchemaDeclared；通用引用 target kind 可为 Unknown，
    不修改既有 FindReferences/Diagnostics 语义。
  - Section creation 作为 existing EditPlan 的 additive structured input；模板 compiler 只生成同一 plan，
    后续继续由 canonical Gateway Preview 和 IDE explicit Apply 消费。
  - 模板定义 internal-first，不在 CONTENT-1 承诺 JSON/YAML persistence 或 wire shape。
- Rejected Alternatives:
  - 从字段名猜 reference target kind：会把不可靠推断变成语义事实。
  - 新建独立 Template ChangeSet/Apply service：会复制 Preview、currency 和 transaction authority。
  - 把完整对象模板塞入 Field Registry：混淆字段 schema 与对象结构事实源。
  - 让 provider 返回 raw Section body：绕过字段 trust、diagnostics 和结构化操作边界。
- Consequences:
  - 现有 field-only plan 保持兼容；CreateSection/Template 作为 additive Experimental 扩展。
  - 首个真实模板必须通过独立 source gate；无法通过时停止，不以 Mock 模板完成产品阶段。
- Follow-up:
  - CONTENT-1A..1F 已完成；public allowlist 58、catalog 7、Gateway methods 9。
  - 首个模板通过 source gate，仅生成 Weapon/Projectile/Warhead 关系骨架；target-kind enrichment、
    multi-file、注册表维护和 asset binding 后置。

## Decision: CONTENT-UI-1 只投影整案 Diff，不创建第二编辑权威

- Status: Accepted / implemented and verified
- Date: 2026-08-23
- Task(s): AUTOMATION-CONTENT-UI-1
- Context:
  - 用户要求在主视图获得类似现代代码审查界面的可视化编辑预览。
  - 当前提案和 Apply 已由 Coordinator、Workspace 与 TransactionPort 管理，v1 没有 hunk 级计划模型。
- Decision:
  - 在主工作区创建临时 AvalonDock Diff document；源文本和 candidate text 是唯一 Diff 输入。
  - 关闭 document 只隐藏，AI 提案卡可重新打开；只有 Dismiss 才终止提案。
  - Apply All 继续调用现有整案、单次、stale-checked Apply；不伪装 per-hunk acceptance。
  - Diff 采用可取消和资源有界实现，并在 reopen/apply 前重查 document identity、revision 和 stale。
- Rejected Alternatives:
  - 直接编辑 Diff 行：会产生第二文本权威并绕过 canonical Preview。
  - v1 增加逐 hunk 接受：需要重定义 plan、diagnostics、原子性和 Undo，超出已批准阶段。
  - 关闭即 Dismiss：浮动/停靠文档的普通窗口行为会意外丢失提案。
- Consequences:
  - UI 可展示完整结构化改动，但 v1 只能整案 Apply 或 Dismiss。
  - 8 MiB/200k input lines/20k visual rows/2k hunks 是 fail-closed 上限；超限不阻塞编辑器。
- Follow-up:
  - UI-1 已通过有界 Diff、lifecycle、authority、responsive 契约测试和完整 non-UI 回归；
    物理屏幕/混合 DPI 的最终视觉仍由用户后续人工验收。

## Decision: 普通 DeepSeek 工具模式采用严格语义与有限格式容忍

- Status: Accepted / implemented and verified
- Date: 2026-08-23
- Task(s): AI-AUTHORING-NONSTRICT-1 Narrow Boundary Fix
- Context:
  - 官方普通 Tool Calls 不保证参数始终严格符合 JSON Schema；实机连续返回无法通过字段工具解析的参数。
  - 当前适配器把所有结构错误折叠为同一句，提示词要求数值加引号仍不能可靠消除失败。
- Decision:
  - 保留唯一 `Ra2AiAuthoringToolAdapter` 和 canonical Preview/Coordinator authority。
  - 只兼容可唯一解释的格式漂移：尾逗号、由 `operations/message` 唯一推断 outcome、缺失展示摘要、
    单 operation 对象以及 JSON number -> INI 文本。
  - 未知属性、重复属性、布尔/null/对象/数组 value、raw INI、Apply/Save 参数和多工具调用继续拒绝。
  - 结构失败返回不含参数值的分类消息；不得记录或回显完整 provider arguments。
- Rejected Alternatives:
  - 只继续调整用户提示词：普通 Tool Calls 仍可能偏离 schema，不能形成可靠产品边界。
  - 直接切换 DeepSeek Beta strict：需要 `/beta` 端点，且现有条件式 schema 与其限制不兼容。
  - 通用 JSON 修复或接受未知字段：会掩盖模型幻觉并扩大不可信输入权限。
- Consequences:
  - 常见非严格返回可继续进入本地 schema/Preview，但不会自动 Apply 或 Save。
  - 对真正含糊或复合的参数仍 fail closed，并给出可诊断但不泄漏内容的消息。
- Follow-up:
  - 若仍出现拒绝，应依据新的结构分类消息新增一个精确刻画案例；不得进一步无证据放宽。

## Decision: Chat / Work 分离，普通完整对象请求不得退化为骨架

- Status: Accepted / implemented and verified
- Date: 2026-08-23
- Task(s): AGENT-MODE-1
- Context:
  - Advisory 对话与结构化编辑此前共用隐式路由，用户无法显式控制授权意图。
  - Weapon 链请求会命中关系 skeleton，导致“搭建可用对象”被错误降级为空骨架。
- Decision:
  - UI 显式提供 Chat / Work，默认 Chat；Chat 永远零编辑工具，Work 才能进入既有 Preview 权限链。
  - 只有明确出现骨架/框架/占位意图才选择 skeleton；普通可用武器链选择 source-gated complete profile。
  - complete profile 绑定唯一既有 owner，生成非空 Weapon/Projectile/Warhead，并原子进入同一 EditPlan。
- Consequences:
  - 新增 public `Ra2AutomationTemplateOutputKind`，allowlist 58 -> 59；不增加 Apply/Save authority。
  - 不支持的完整对象类型必须本地明确拒绝，不能静默退化为 skeleton。

## Decision: RA2 Skill 是有界知识层，不是插件权限层

- Status: Accepted / implemented and verified
- Date: 2026-08-23
- Task(s): AGENT-KNOWLEDGE-1
- Context:
  - Agent 需要稳定理解 RA2/YR/Ares/Phobos 的领域依赖，但把全部知识常驻 prompt 会增加噪声和成本。
  - Field Registry 已拥有字段 schema/trust，Capability Gateway 已拥有可执行边界，二者不能被 Skill 复制。
- Decision:
  - 采用标准 `SKILL.md` 形态和 progressive disclosure；v1 只加载仓库内置、只读 Markdown。
  - 按精确领域选择 primary Skill，按显式 Ares/Phobos 与 field trust 追加辅助 Skill；总注入量有界。
  - Skill 只描述工作流、依赖顺序和停止条件；Field Registry 是字段事实源，Content Profile 是对象完整度事实源，Host 是 Apply/Save 权限源。
  - 禁止 scripts、外部根、热更新和 Skill 直接工具调用；这些能力必须经过后续独立安全/版本契约。
- Consequences:
  - 已内置 15 个领域 Skill，public API diff 为 0，Gateway/Save/Shell 权限不变。
  - 领域知识可逐阶段扩充，而不会把提示词文本误当成可靠编辑算法。
- Follow-up:
  - CONTENT-2A..2D 把高优先级领域 Skill 逐个落为 source-gated complete profiles；之后再冻结 HOST-1。

## Decision: 双武器完整 profile 与循环开火语义必须分离

- Status: Accepted / implemented
- Date: 2026-08-23
- Task(s): CONTENT-2A
- Decision:
  - 双武器 profile 只表达现有 Techno 的 Primary/Secondary 两条完整 direct-fire 引用闭包。
  - `Primary/Secondary` 和 `Burst` 不得被描述为循环/交替开火。
  - 在 BuiltIn source gate 补齐 Gattling 字段前，循环/交替请求在模型调用前 fail closed。
- Consequences:
  - 新增一个 template descriptor，但 public 类型、Gateway 方法、Apply/Undo/Save 权威均不变。
  - 真正 Gattling/Cycle 进入独立后续契约，不允许内部 hard-code 绕过 Field Registry。

## Decision: Projectile 弹道族拆分，Warhead 完整度限定为 YR Core

- Status: Accepted / implemented and verified
- Date: 2026-08-23
- Task(s): CONTENT-2B
- Context:
  - 原版 `Arcing` 与非零 `ROT` 组合存在错误行为；Phobos `Trajectory` 也明确排斥
    `Arcing/ROT/Vertical/Inviso`。
  - YR `Verses` 是固定 11 槽，而 Ares custom ArmorTypes 使用动态 `Versus.*`。
- Decision:
  - Arcing 与 Homing 使用两个独立 template id、capability 和精确 tool schema，不使用条件式大 schema。
  - YR core Warhead 只覆盖 11 个原生 ArmorTypes；存在 `[ArmorTypes]` 时拒绝，不生成动态 key。
  - 三个 profile 均只绑定唯一既有 Weapon，并继续进入 canonical Plan/Preview/Host Apply。
- Consequences:
  - public allowlist、Gateway、Apply/Undo/Save 与 UI 均不变。
  - Ares custom armor、Phobos trajectory 与 Airburst/Splits 必须在后续独立 source-backed profile 中实现。

## Decision: Work 采用两阶段模型意图分析与结构化执行

- Status: Accepted / implemented; real-provider acceptance pending
- Date: 2026-08-23
- Task(s): AGENT-MODE-2
- Context:
  - 中文自然语言中的否定范围、完整对象与骨架意图不能由持续扩张的本地关键词表可靠覆盖。
  - 原路由会把“其他字段不要修改”误判为全局 advisory，也会因裸 `为/成` 赋值标记错分能力。
- Decision:
  - Chat 保持一次 advisory 调用；Work 在同一请求生命周期内先调用 required intent-analysis tool，
    本地严格校验有界事实包后，再发起 advisory 或既有 required authoring tool 调用。
  - 第一次调用不输出思维链、不显示、不持久化、不进入聊天历史，也不拥有任何编辑权限。
  - 本地 capability allowlist、Field Registry、template compiler、snapshot currency、Preview 与显式
    Apply/Save 权威继续生效；分析包只能选择已存在能力，不能创建能力。
- Rejected Alternatives:
  - 继续追加关键词和正则：边界组合会持续增长，无法稳定覆盖自然语言作用域。
  - 让第一次调用直接生成修改：混合意图判断与执行，失败诊断和权限边界更差。
  - 解析自由文本或模型思维链：不可稳定校验，也不应作为产品接口。
- Consequences:
  - Work 每次正常请求增加一次模型调用、延迟与 token 成本，并新增第一阶段失败点。
  - 无效/多工具/越界分析包 fail closed；第一阶段失败不会产生第二次请求或部分修改。
  - 本地自动化只能证明协议和编排，实际模型遵循度仍需真实 DeepSeek 手工验收。
- Follow-up:
  - 完成截图原句的真实双调用验收；若 provider 输出漂移，只增加精确分析包适配测试，不放宽能力白名单。

## Decision: 对象注册属于 typed Content Template，不属于 Field Registry

- Status: Accepted / implemented foundation
- Date: 2026-08-24
- Task(s): CONTENT-2D-0/1
- Context:
  - 新建 Techno/SuperWeapon 等对象需要数字类型列表，而现有模板只支持固定字段 Key。
  - Field Registry 的职责是字段 schema/trust；把 `0/1/...` 或项目对象 ID 塞入字段库会污染全局字段事实。
- Decision:
  - Registration 作为 internal Template Definition 声明，在编译时读取当前 Snapshot 并产生普通
    `UpsertField` operation；继续复用唯一 Preview/Apply/Undo 链。
  - 分配规则固定为 `max(existing index) + 1`，保留顺序和空洞；已有唯一注册幂等，畸形或重复列表 fail closed。
  - `ReferenceReachable`、tuple 和 cross-file artifact 作为不同闭包策略，不用同一个“万能注册”算法处理。
- Rejected Alternatives:
  - 在 BuiltIn Field Registry 枚举数字 Key：无法表达索引生命周期且污染 Completion/Hover/Diagnostics。
  - 让模型直接生成 raw 注册行：绕过确定性冲突、索引和 Snapshot 门禁。
  - 分别写 rules/art：无法提供原子 Preview、Apply 和 Undo。
- Consequences:
  - 2D-1 只提供当前文档内部基础；现有生产 Profile 与 public API 零变化。
  - 2D-2 必须先定义项目级多文档事务，之后才允许 rules/art 绑定和完整 Techno/SuperWeapon Profile。
