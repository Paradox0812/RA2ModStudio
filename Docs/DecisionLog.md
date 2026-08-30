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

## Decision: 项目事务以 IDE session store 为内存权威，Application 只提供纯 Preview

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): CONTENT-2D-2
- Context:
  - 当前 Shell 只有一个 editable session；非活动文件没有可提交、可撤销的内存所有者。
  - Application 已有可靠的单文档 Snapshot/Plan/Preview，但连续调用不能保证跨文件原子性。
- Decision:
  - Application 只增加复用现有叶 DTO/engine 的 Project Snapshot/Plan/Preview，不拥有 Apply/Undo/Save。
  - IDE project document session store 成为活动和非活动 session 的唯一所有者，Shell 仅投影 active session。
  - Apply 使用 validate-all/prepare-all/commit-all；compound Undo/Redo 复用同一原子 replace-many seam。
  - Apply 后项目诊断读取 Preview/committed session 的内存覆盖，不从磁盘回读未保存目标；刷新失败不逆转提交。
  - Apply/Undo/Redo 纯内存且写盘次数为零；磁盘 Save All 和跨文件持久化事务另立契约。
- Rejected Alternatives:
  - 对 rules/art 依次执行两个单文档 Apply：第二步失败会留下半提交且无法统一 Undo。
  - 直接写非活动文件：绕过 Preview/dirty/Undo/显式 Save 权限。
  - 在 Shell 保留 `_editableSession` 同时新增隐藏项目缓存：形成双重 session 权威和切换竞态。
  - 在 Application 公开 Apply/Undo/文件系统接口：破坏 Host authority 边界。
- Consequences:
  - 2D-2 是 R4 owner 迁移，需要独立分阶段实现和完整单文档回归。
  - Gateway 将有 additive Experimental project Preview surface；Host mutation 类型保持 internal。
  - 首个 rules/art profile、AI tool schema、Save All 和退出确认 UI 仍需后续契约。
- Follow-up:
  - 以已验证的 project Preview/session transaction 为底座，单独制定首个 rules/art consumer/profile 契约。
  - Save All、退出确认 UI、独立 Host/wire 继续保持后置。

## Decision: INI Project Plan 与 Asset Manifest 分权

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): CONTENT-2D-3, ASSET-MANIFEST-1
- Context:
  - rules/art 绑定需要确定性 INI 修改，同时后续 SHP/Cameo/VXL 提供器需要独立资产输入。
  - 当前 Field Registry 可安全 author `rules Image` 与 `art Image`，但没有 authorable Cameo/Voxel schema。
- Decision:
  - `Ra2AutomationProjectEditPlan` 继续是唯一 INI 修改真相；Asset Manifest 只保存不可变需求和绑定事实。
  - Proposed binding 必须有精确叶 operation；PendingSchema 必须没有 operation。
  - 首个 project template 精确配对 rulesmd/artmd 或 rules/art，并复用既有 document compiler 与 Project Preview。
- Rejected Alternatives:
  - Manifest 直接写素材或 INI：混淆 proposal、provider 与 Host authority。
  - 为 Cameo/Voxel 绕过 Field Registry：会破坏现有 fail-closed schema/trust 门禁。
  - 新建第二套跨文档 Preview：会造成语义和失败行为分叉。
- Consequences:
  - body SHP 的 rules/art 绑定已可确定性预览；Cameo 需求可交给后续 provider，但绑定仍为 PendingSchema。
  - AI UI 尚未自动选择/展示该项目模板；素材本体也尚未生成。
- Follow-up:
  - 先完成 source-backed Art schema 补强，再把 Cameo/Voxel binding 从 PendingSchema 提升为 Proposed。
  - 后续 ASSET provider 只消费 Manifest，不获得 Apply/Save 权限。

## Decision: Art schema 与 Asset Provider 分权，Provider 只产出有界内存 Artifact

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): FIELD-REGISTRY-ART-1, ASSET-PROVIDER-1
- Context:
  - YR `artmd.ini` 已明确 `Cameo/AltCameo/Voxel/Remapable`，旧 Manifest 因缺少 `Cameo`
    authoring schema 只能保留 PendingSchema。
  - Application 必须保持 Core-only；把本地路径或文件写权限放进 provider 会破坏 Host authority。
- Decision:
  - 四个 YR ArtObject 基础字段进入 source-backed BuiltIn schema；当前 SHP profile 只消费 Cameo。
  - public provider protocol 接受 Host 显式提交的有界内容，返回 Manifest-closed Artifact、SHA-256
    与 `IdentityExtensionAndHash` 级别。
  - Existing provider 全成功或零产物；不加入当前 INI Gateway，不读取/写入磁盘。
- Rejected Alternatives:
  - Manifest 直接携带本地路径或执行复制：引入路径权限、TOCTOU 和隐式写盘。
  - 只公开 interface、隐藏所有返回值构造：外部 provider 无法真正实现接口。
  - 仅凭扩展名宣称 SHP/VXL/HVA 有效：没有 codec/parser 证据。
- Consequences:
  - rules/art Project Preview 现在含 rules Image、art Image、art Cameo 三项操作。
  - Provider plugin boundary 已可用，但素材持久化、格式解析、生成器和 AI UI 仍需后续阶段。

## Decision: Work 项目提案复用现有 Project authority，Asset Manifest 保持非阻塞

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): CONTENT-PROJECT-UI-1
- Context:
  - rules/art Project Template、Preview、原子 Apply、compound Undo 和多文件 Diff 已完成，但 AI
    Work 面板仍只消费单文档 Proposal。
  - INI 可以合法引用尚不存在的素材；把素材存在性作为项目 Apply 前置会错误耦合两个生命周期。
- Decision:
  - 扩展现有唯一 Coordinator/Workspace active proposal 链，使 Proposal 严格区分 Document/Project，
    Project 分支直接复用 `ExpandProjectTemplate`、`PreviewProject`、`ApplyProject` 和现有 Diff Builder。
  - Manifest 只在提案卡显示为非阻塞素材待办，不调用 Provider，不参与 ApplyPolicy、Diagnostics 或 Save。
  - Work 继续使用两次 DeepSeek；第一次选择 project capability，第二次只提交固定模板参数。
- Rejected Alternatives:
  - 新建第二个 Project AI coordinator：会分裂 active proposal、Dismiss 和 supersede 权威。
  - 依次执行两个单文档 Apply：无法保证 no-partial 与 compound Undo。
  - 在 UI 检查或写入素材：混淆 INI proposal 与可选资产工作流。
  - 重写 Project Diff：现有 builder 已支持按文件投影。
- Consequences:
  - Work 现可生成并显示 rules/art 两文件修改；Apply 仍只更新内存且需用户显式确认。
  - v1 仍只绑定现有 Techno，不创建完整对象或素材。
- Follow-up:
  - 自动门禁已通过；真实 DeepSeek、Project Diff、Apply、Ctrl+Z/Ctrl+Y 与 Save Current 由用户按手工脚本验收。

## Decision: 明确项目 capability 权威高于 provider 派生分类元数据

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): CONTENT-PROJECT-UI-1-NF2
- Context:
  - 同一合法 rules/art 请求在不同真实响应中分别得到 `completion_level=complete` 与
    `domain_intent_id=techno`；两者都携带精确 `techno-rules-art-binding` capability。
  - capability 已唯一选择一个只读 Project Snapshot、一个固定五参数工具和一个本地确定性 compiler，
    继续让描述性元数据否决它只会造成 provider 表述漂移被误报为协议损坏。
- Decision:
  - 先严格验证 tool、根字段、枚举、allowlist 与 capability/outcome；随后只对精确
    `techno-rules-art-binding` 把 domain/completion 归一化为 `art-animation + Field`。
  - capability、project availability、tool schema、Application compiler 与 Host transaction 继续是
    权威门禁；归一化不授予任何新增操作、文件、Apply、Save 或素材权限。
- Rejected Alternatives:
  - 为每次真实响应继续追加 domain/completion 单值兼容：无法消除同类返工。
  - 放宽 JSON/tool schema 或 adapter：第二阶段真实响应已完整符合现有契约，没有证据支持放宽。
  - 自动重试或模型 fallback：掩盖本地准入错误并扩大成本与状态复杂度。
- Consequences:
  - provider 可使用任一已声明 domain/completion 描述同一明确项目 capability，但第二阶段始终消费
    canonical metadata；未知 schema 值仍 fail closed。
  - UI 诊断中的 `Deltas=0/Characters=0` 仍只统计正文 delta；tool-call fragment 不是正文，数值为零不表示空响应。
- Follow-up:
  - 用户在新构建上复验原始 prompt；若仍失败，必须保留 RequestId 并按阶段边界诊断，不再扩大准入白名单。

## Decision: 本地 Work 拒绝不得伪装成 Provider/Protocol 失败

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): CONTENT-PROJECT-UI-1-NF3
- Context:
  - 最新真实 rules/art 意图 tool call 已严格通过 parser，但用户仍只看到通用 ProtocolError。
  - Pipeline 已有精确 Project availability 分类和安全消息，Shell 的 FailureKind formatter 却隐藏了它们。
- Decision:
  - 在既有 internal `Ra2AiResponse` 中增加瞬态 `LocalRejection` 分支，不新建平行 DTO。
  - Local rejection 必须无 provider `ErrorMessage`、无 ToolCalls、`FailureKind=None`，只显示本地固定安全原因。
  - Provider、HTTP、SSE 和 transport failure 继续使用既有 FailureKind 与脱敏 UI formatter。
- Rejected Alternatives:
  - 继续扩大 prompt 或 domain/completion 白名单：真实响应已经可解析，无法解决本地原因被吞掉的问题。
  - 把本地原因塞入 provider `ErrorMessage`：会继续混淆信任边界并受 Shell 脱敏规则屏蔽。
  - 遇到 PairMissing 时自动创建 art/rules 文件：超出当前只绑定既有唯一 pair 的契约与 Save 权限。
- Consequences:
  - 用户能区分“模型/网络失败”和“当前项目不满足结构化修改前置条件”。
  - 旧截图无法确定六种准入原因中的哪一种；必须在新构建复验后依据具体文案处理。
  - 无 public API、持久化格式、文件扫描、Preview/Apply/Undo/Save 或资产行为变化。

## Decision: Work 项目成员以 Project Session 为权威，不以 UI 树为权威

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): CONTENT-PROJECT-UI-1-NF4
- Context:
  - 正确项目 `H:\RA2\YR_Test` 含唯一顶层 `rulesmd.ini + artmd.ini`，但用户仍遇到 pair 不可用。
  - Shell 曾从 `ProjectExplorer.Items` 捕获项目文件；该集合是界面投影，不应控制编辑事务准入。
- Decision:
  - 成功打开项目时由 `ShellViewModel` 保存不可变 `ProjectOpenResult.Files`。
  - `Ra2ProjectDocumentSessionStore.MemberFilePaths` 是当前 Work 项目成员与目标配对的唯一运行时权威。
  - UI 树只负责显示；Work 摘要必须显示完整根路径及本地 admission 结果，避免要求用户靠提示词猜状态。
- Rejected Alternatives:
  - 在提示词中要求用户重复绝对路径或 rules/art 文件名：不能修复 Host 已捕获错误上下文的问题。
  - 递归搜索或自动选择最近项目：会引入错误目录、歧义和隐式权限扩张。
  - 自动创建空 art/rules：越过 Preview/Apply/Save 与既有文件边界。
- Consequences:
  - 空 `artmd.ini` 和额外非配对 INI 保持合法；只有 pair 缺失、重复或同时存在两套 pair 才拒绝。
  - 不新增 public API、持久化格式或文件写权限；Shell XAML 与全局布局不变。

## Decision: Source-backed 开放引用不得被观测 Enum 列表封闭

- Status: Accepted / implemented / verified
- Date: 2026-08-24
- Task(s): CONTENT-PROJECT-UI-1-NF5
- Context:
  - Global 用户字段包可能把学习或迁移中见过的 `Image` 值保存为 Enum；这些值是建议/样本，不是 RA2
    素材命名空间的完整集合。
  - project binding compiler 过去直接复用 effective schema 的 Enum 闭集校验，使 `HTNKART` 这类
    合法新引用因未出现在旧列表中而被拒绝，即使模型工具参数、项目配对和 Section 都正确。
- Decision:
  - 模板字段规格拥有 internal 验证策略，默认继续遵循 effective schema。
  - 仅经 source-backed profile 明确声明的 rules/art `Image` 与 `Cameo` 使用开放引用验证：字段 schema
    必须存在、trust/authoring 门禁保持、值必须是安全 identifier，但 observed Enum 不作为闭集。
  - 该策略属于 Application 模板定义，不写入字段库、不序列化、不改变 Project > Global > BuiltIn 优先级。
- Rejected Alternatives:
  - 修改或删除用户 Global 字段包：会破坏用户数据且不能解决未来导入的同类问题。
  - 全局降低 Enum 校验或改变 provider priority：影响 Completion、Hover、Diagnostics 和其他模板。
  - 让模型重试、猜已有枚举值或增加 prompt 约束：根因在本地 authority，不应增加付费调用或限制新 ID。
  - 完全绕过字段 schema/trust：会丢失字段存在性、Blocked 和安全 identifier 门禁。
- Consequences:
  - 新 body/Cameo/art ID 可以形成可复核 Project Proposal，旧观测值仍可作为字段智能提示使用。
  - 其他模板与真正封闭的 Enum 继续按原 schema 校验；开放引用必须由模板逐字段显式声明。

## Decision: DeepSeek 拥有 Work 项目内容语义，Host 只执行最低结构与事务安全

- Status: Accepted / implemented / verified
- Date: 2026-08-25
- Task(s): CONTENT-PROJECT-UI-1-NF6
- Context:
  - NF5 证明模型给出的 `HTNK/HTNKART/HTNKBODY/HTNKICON` 正确，失败来自本地旧 Enum/Profile；
    继续给 Profile 增加例外仍会让本地不完整知识成为第二内容权威。
  - 用户明确要求由 DeepSeek 完成构建或说明不可确定，内置程序除最低安全界限外不得干涉 AI 输出。
- Decision:
  - 生产 Work rules/art 路由使用通用 `preview_ini_project_edit_plan`，由模型决定具体 Section、字段、值、
    注册关系和 rules/art 绑定；无法确定必要目标时返回 `needs_clarification`。
  - Host 只验证结构、资源上限、安全 identifier、符号 rules/art 范围、captured snapshot、canonical
    Preview、显式 Apply、single-use/stale 与原子回滚。Field Registry/Diagnostics 是 advisory evidence，
    不得否决通用模型计划。
  - 旧固定 project template 保留为 headless compatibility surface，但不再是生产 AI 项目路由的内容权威。
- Rejected Alternatives:
  - 持续扩展 Profile/OpenReference 例外：仍会对未知合法 mod 形成封闭世界误拒绝并持续返工。
  - 删除路径、snapshot、Apply、事务和资源门禁：会把模型内容自由错误扩大为越权写入与不可恢复修改。
  - 允许模型提交任意文件路径或完整候选文本：绕过已验证 Project membership 与 semantic Preview。
- Consequences:
  - 新 mod-specific 字段和值即使不在字段库中，也能进入可视化 Project Diff，并由用户决定是否 Apply。
  - 诊断与信任信息仍可提示风险，但不会阻断项目显式 Apply；Apply 仍只修改内存且不自动 Save。
  - Experimental Preview 对 `ExpectedSectionKind.Unknown` 的 blocked trust 行为有语义变更；具体类型的
    headless 模板行为保持不变，详见 Public API Ledger。

## Decision: Rules/Art 跨域知识按 capability 选择专用 Skill

- Status: Accepted / implemented / verified
- Date: 2026-08-25
- Task(s): AGENT-KNOWLEDGE-1-R2
- Context:
  - NF6 正确移除了 Host 内容否决，但真实 DeepSeek 把用户角色词 `Art/Body/Cameo` 直译为 rules 字段。
  - 原路由只加载一个 normalized domain primary；project capability 被归一为 `art-animation` 时，现有 Skill
    只要求“区分概念”，没有提供精确 rules Owner.Image → art Section → asset/cameo 图。
- Decision:
  - 新增 `ra2-rules-art-binding`，以实际 INI、ModEnc 逆向条目及 Ares/Phobos 官方文档为来源，冻结
    Techno rules/art 图、对象家族 body 规则、registration、Voxel/Cameo 所有权及 `ArtImageSwap` 条件。
  - PromptBuilder 对 `ProjectRulesArtBindingPreview` 按 capability 显式选择该 Skill；普通请求继续沿用
    ExactDomainPrimary + extension + trust，不建立第二 loader/resolver。
  - Skill 只影响模型知识，不成为 Host validator；未知事实由模型 clarification，Field Registry 保持 advisory。
- Rejected Alternatives:
  - 在 Adapter 中禁止 `Art/Body/Cameo`：会重新建立用户已拒绝的本地内容权威，且无法覆盖所有错误语义。
  - 只在通用 system prompt 增加当前例句：不可审计、不可版本化，也不能覆盖对象家族与扩展差异。
  - 一次加载所有 16 个 Skill：超出渐进披露原则并引入跨领域冲突与 token 浪费。
- Consequences:
  - 同一 project capability 不再受 provider domain 表述漂移影响；第二阶段始终获得专用绑定知识。
  - 对 Infantry/Vehicle/Aircraft 的不同 ArtSection/BodyAsset，若没有 `ArtImageSwap=true` 等必要事实，
    正确结果可能是 clarification，而不是无条件 proposal。
  - public API、Field Registry 数据、Project Preview/Apply/Save 与 Shell/UI 均不变。

## Decision: Work 第一轮基于同一 Catalog Manifest 推荐 Skill，Host 保留最终解析权

- Status: Accepted / implemented / automated verified
- Date: 2026-08-25
- Task(s): AGENT-SKILL-ROUTING-2
- Context:
  - 旧流程的第一轮只输出 intent/domain，不知道当前 16 个 Skill；第二轮由 PromptBuilder 独立按 domain
    选择，无法表达跨域组合、知识缺口，也无法证明两阶段使用同一目录快照。
  - 直接提供 Skill 文件读取工具会增加第三次 provider 往返并扩大工具面，不符合已接受的两调用 Work 契约。
- Decision:
  - 唯一 `Ra2AgentSkillCatalog` 同时产生不含正文的紧凑 Manifest、解析模型推荐并提供最终正文。
  - 第一轮工具结果增加最多 6 个有序 Skill ID 与 6 个知识缺口；未知 ID 由 Host 记录并省略，不否决整个意图包。
  - Host 按 capability 补齐必选 Skill 与 field trust，稳定去重、校验模式和 14 KiB 正文预算；第二轮只能消费
    这份显式 resolution，不能重新按 domain 选择。
  - Chat 保持单调用与本地选择；Work 保持一次分析加一次执行，不新增 retry、provider 或权限。
- Rejected Alternatives:
  - 第三次 `list/read_skill` 工具循环：增加延迟、成本和失败面。
  - 模型拥有最终 Skill 权威：可能漏掉 project binding 等 capability-critical 知识。
  - 把所有正文塞进第一轮：违背渐进披露并浪费 prompt 预算。
  - 建立第二个 manifest registry：产生目录漂移和重复缓存。
- Consequences:
  - 第一轮推荐可覆盖跨域需求，第二轮注入可审计且与 Manifest 同源；模型遗漏不再移除关键 Skill。
  - Provider tool JSON shape 有 Experimental additive breaking change；真实 DeepSeek schema compliance 尚待人工验收。
  - public API、持久化、Shell/UI、Field Registry 和 Preview/Apply/Save 权限不变。

## Decision: Work 两阶段共享捕获上下文，并在两调用之间执行有界 HLI 只读查询

- Status: Accepted / implemented / automated verified
- Date: 2026-08-25
- Task(s): AGENT-CONTEXT-3
- Context:
  - 第一轮此前看不到最近会话与 rules/art 捕获快照，无法可靠选择需要核实的项目事实；第二轮只能依赖
    光标附近文本、Skill 与模型猜测。
  - 增加第三次模型工具循环会提高延迟、成本和失败面；新建 parser/project index 会复制既有 HLI 权威。
- Decision:
  - 两次 Work 请求使用同一受限会话、当前主题和 request-lifetime 文档投影。
  - 第一轮最多返回 8 个 `get_section` / `resolve_reference` 请求；Host 只对 `current/rules/art` 捕获别名
    调用既有 Gateway，并将有界结果标为不可信数据交给第二轮。
  - 捕获快照在两次调用间不可刷新；Chat 保持单调用，Work 保持两次调用。
- Rejected Alternatives:
  - 第三次 provider query loop：增加成本且破坏已接受的两调用契约。
  - 允许模型提交路径或直接读磁盘：绕过项目成员身份、快照一致性和隐私边界。
  - 在 IDE 复制 Section parser 或建立第二项目索引：造成语义漂移和双重权威。
- Consequences:
  - 模型可在执行前核实现有 Section/引用，而不获得编辑、Apply、Save、shell、网络或路径权限。
  - `context_queries` 是 Experimental provider JSON 的 additive change；真实 DeepSeek schema compliance 仍需手工验收。
  - Application public API、持久化、XAML、Field Registry、Preview/Apply/Undo/Save 语义不变。

## Decision: Work 允许一次有界结构化重规划作为两调用基线的条件性例外

- Status: Accepted / implemented / automated verified
- Date: 2026-08-25
- Task(s): AGENT-REPAIR-1
- Context:
  - `AGENT-SKILL-ROUTING-2` 与 `AGENT-CONTEXT-3` 已接受正常 Work 固定为一次分析加一次执行，并拒绝开放式第三次 provider/query loop。
  - 人工验收已证明 query-target 连续性修复有效，但模型仍可能返回无工具调用、无效参数、错误文档目标或无法通过 canonical preview 的结构化计划；当前只能要求用户重发整条请求。
  - `PreviewRejected` 和多数 `TemplateExpansionRejected` 当前丢失底层 typed failure，不能可靠地用本地化错误文本决定是否修复。
- Decision:
  - 正常 Work 继续严格为两次 provider 调用；Chat 继续为一次。
  - 仅在第二次执行后的 typed、allowlisted、模型可修正失败上，允许同一 request lifecycle 内追加一次非流式结构化修复调用；Work 总调用上限为三次，修复上限为一次。
  - 修复复用同一 intent、route、Skill resolution、project projection 与 HLI query results；不重跑分析、Skill 选择或本地查询，不切换 provider/model。
  - transport、超时、取消、配置、上下文过期、资源上限、安全拒绝与 unexpected failure 永不触发修复。
  - canonical adapter/template/preview 与显式 Apply/Undo/Save 边界保持唯一权威；Host 不自动 retarget、Apply 或 Save。
- Rejected Alternatives:
  - 在 `DeepSeekRa2AiClient` 中做通用 retry：会把语义失败错误归类为传输失败并隐藏成本。
  - 在 `ShellWindow` 中直接写重试循环：会把 provider 编排和失败政策泄漏到 WPF presentation。
  - 按中文错误消息匹配：不稳定且无法覆盖 project/template leaf failure。
  - 重跑第一轮分析、Skill 或 HLI 查询：扩大成本、漂移 request-lifetime facts，并重新打开已拒绝的 provider query loop。
  - 多轮自愈、模型 fallback 或 Host 静默改写计划：不可预测且破坏用户审阅边界。
- Consequences:
  - 最坏 Work 成本由两次调用增加为三次，但只发生在严格白名单失败路径。
  - IDE 需要内部 typed failure evidence、execution seed、repair policy/orchestrator，以及 `ShellWindow.xaml.cs` 的窄接线；计划 public API 与持久化 diff 为零。
  - 该决策只覆盖 bounded structured replan，不授权联网知识检索、任意文件访问、自动 Apply/Save 或素材生成。

## Decision: 文件关联启动复用项目会话权威，单实例转发延期

- Status: Accepted / implemented / automated verified
- Date: 2026-08-25
- Task(s): SHELL-LAUNCH-1
- Context:
  - Windows 将 `.ini` 默认打开方式指向 IDE 后，会把文件路径作为裸进程参数传入；原 App 仅识别自动化文件夹参数，因此只显示空 Shell。
  - 直接从启动层读取文件会绕过 Project Explorer、编码元数据、Field Registry、高亮、编辑会话和项目事务权威。
- Decision:
  - 单个既存 `.ini` 的直接父目录就是本次项目根，目标必须来自该项目成功打开后的顶层 INI 清单。
  - Shell 完成初始 Dock 生命周期后，启动请求复用菜单项目打开、项目 session store、精确文件加载和现有 editable session 路径。
  - 保留 `--automation-open-folder`；不新增无需求的通用命令行选项。
  - 多实例合并、mutex 与 IPC forwarding 延期到独立 `SHELL-LAUNCH-2` 契约。
- Rejected Alternatives:
  - App 直接 `File.ReadAllText`：形成第二文件读取与编辑权威。
  - 向上猜测项目根或递归扫描：项目归属不确定，且改变现有 top-level ProjectOpen 语义。
  - 本阶段同时实现单实例 IPC：扩大生命周期、并发、脏文档和安全边界，无法由文件关联修复需求证明必要。
- Consequences:
  - Explorer 打开 INI 可进入完整项目上下文与可编辑会话；无自动 Save/Apply/AI 行为。
  - 当前每个 Explorer 请求仍创建独立进程；这是一项显式产品限制，不是本阶段失败。
  - public .NET API、持久化格式、XAML、Dock 布局和 AutomationId 均不变。

## Decision: SuperWeapon 采用 typed common profiles + model-owned generic fallback

- Status: Accepted / implemented / automated verified
- Date: 2026-08-25
- Task(s): CONTENT-2E
- Context:
  - 当前已存在 `[SuperWeaponTypes]` 注册编译、Project Preview、显式 Apply/compound Undo、两阶段 Work、
    Skill 选择与通用模型主导 Project Plan，但没有任何 SuperWeapon complete profile。
  - Ares 官方明确说明不同 SuperWeapon Type 具有不同默认值和专用字段；单一万能模板会混合不兼容语义。
  - 用户已明确拒绝由不完整 Field Registry Enum 否决模型产生的新 mod 内容。
- Decision:
  - `UnitDelivery` 与 `GenericWarhead` 两个常用 Ares 类型使用 source-backed typed complete profile；
    其它明确类型复用 model-owned project plan，并在事实不足时 clarification。
  - typed 与 generic 两轨都进入同一 Project Preview / Diff / Apply / Undo 权威；Field Registry 与
    Diagnostics 对 generic plan 只提供 advisory evidence。
  - SuperWeapon v1 只修改唯一 rules/rulesmd 项目成员；art 和素材不是前置条件，不自动 Apply/Save。
- Rejected Alternatives:
  - 一个万能 typed SuperWeapon 模板：无法安全表达 type-specific defaults 与互斥 targeting。
  - 全部继续 blanket unsupported：浪费现有模型知识、Skill、通用 Project Plan 和用户审阅边界。
  - 全部只走 typed allowlist：重新建立封闭世界内容否决器。
  - 直接允许模型提交路径或完整文件：绕过 snapshot membership 与 canonical Preview。
- Consequences:
  - 常用类型获得确定性对象闭包，其它类型仍可由模型生成可审阅 Proposal；不能宣称所有类型已 typed-certified。
  - 预期 public API、持久化、Shell/XAML、parser、Registry、Apply/Save authority 零变化。
  - 真实 DeepSeek 参数服从度和游戏内行为仍需人工验收。
  - typed profile 先生成既有 document plan，再由 IDE 包装为既有 Project Plan；不伪造 Asset Manifest，
    不改变 public project-template result invariant。
  - 经用户批准，Shell 项目快照捕获允许唯一 rules 目标并把匹配 art 视为可选；XAML、布局和其它 Shell 行为不变。

## Decision: Work 从固定单次精确查询升级为有界语义检索闭环

- Status: Accepted / implemented / automated verified
- Date: 2026-08-25
- Task(s): AGENT-QUERY-2, AGENT-ENTITY-1, AGENT-CONTEXT-4, AGENT-EVAL-1
- Context:
  - 既有 Work 只有分析后的一次 `get_section/resolve_reference` 批次；模型猜错 Section ID 或只知道本地 `Name/UIName` 时，Host 无法搜索和补查。
  - 用户实际验收中必须手工补充 `GAPOWR/E1/FV`，这与项目已捕获完整 rules 快照的事实不相称。
- Decision:
  - 首轮可请求 `search_objects`，Host 基于现有 semantic model 在捕获快照中按规范 ID、`Name`、`UIName` 和短注释确定性搜索。
  - 必要时允许最多两次紧凑 retrieval refinement；重复查询、无进展或两轮上限立即停止，不做 transport retry 或 provider fallback。
  - 唯一高置信结果形成瞬态 canonical binding；同分多义绝不静默选择。
  - SuperWeapon 项目能力在执行前补充 `[SuperWeaponTypes]` 与已绑定实体 Section；所有结果仍只进入现有 Preview/显式 Apply 权威。
  - 项目执行 prompt 删除无关 caret-local 上下文，保留 Skill、project projection、bindings 与 Host facts。
- Supersedes:
  - `AGENT-REPAIR-1` 中“正常 Work 固定两次调用”的成本假设；新的正常 Work 为 2 到 4 次，若随后触发既有一次 repair，绝对上限为 5 次。
- Consequences:
  - public HLI、持久化、parser、Field Registry、Diagnostics、Apply/Save/Undo 和 Shell/XAML 均不变。
  - provider-visible intent query schema 是 additive Experimental change；真实 DeepSeek 服从度仍需人工验收。
  - 本地不存在的中文 CSF 显示文本仍不能由 Host 自行翻译，必须由模型推断候选或请求澄清。

## Decision: Work admission only enforces authority and resource safety

- Status: Accepted / implemented / automated verified
- Date: 2026-08-26
- Task: AGENT-WORK-ENTRY-1
- Context:
  - Real UI use showed HTTP 200 tool calls rejected before execution while ideal fixture JSON passed.
  - The parser treated domain/completion/capability enums, additive fields, list sizes and query placeholders as fatal and
    the pipeline discarded the exact reason.
- Decision:
  - First-call metadata is descriptive. It is normalized, bounded or routed through a generic preview; it cannot veto a
    model-owned INI plan.
  - Fatal admission is limited to the tool envelope, bounded valid JSON object and duplicate root identity.
  - Invalid read-only queries are dropped individually; symbolic target and snapshot membership remain authoritative.
  - Every production current-document capability uses the generic document-operation tool, and every production rules/art
    capability uses the generic project-operation tool. Typed Profile compilers remain compatibility/headless helpers,
    not production semantic gates.
  - Unknown project capability/domain values use the existing bounded project preview when project authority exists.
- Rejected alternatives:
  - Add one more domain/capability whitelist exception: repeats the same failure pattern.
  - Accept arbitrary target/path to improve convenience: violates snapshot authority.
  - Delete canonical Preview/Apply/stale/resource checks: confuses semantic freedom with write authority.
- Consequences:
  - Provider-visible schema remains guidance; Host parser is forward-compatible with additive variants.
  - Generic proposal `summary/message` are non-authoritative presentation metadata. They cannot reject valid
    operations; clarification alone requires a readable bounded message and keeps echoed operations inert.
  - Real provider behavior is still a manual verification item; deterministic tests no longer count as that evidence.

## Decision: CandidateText is the canonical review result

- Status: Accepted / implemented / automated verified
- Date: 2026-08-26
- Task: DIFF-REVIEW-1
- Context:
  - A fixed three-line unified Diff could prove changed lines but could not show the full object/file context users need before applying complex proposals.
- Decision:
  - The default Result mode displays the exact successful Preview `CandidateText`; it never reconstructs content from Plan operations.
  - Unified Changes remains the authority for removed-line evidence. Object Context is a bounded, depth-one, read-only index over captured snapshots and can degrade independently.
  - Review selection, outline and render state remain transient IDE presentation state and never participate in Apply, Undo, Save or layout persistence.
- Rejected alternatives:
  - Increasing unified-diff context to the whole file: obscures deletion evidence and still lacks semantic object navigation.
  - Rebuilding a synthetic object file from Plan operations: can diverge from the exact candidate that Apply will commit.
  - Partial Section/hunk Apply: changes transaction authority and is outside this review-only stage.
- Consequences:
  - Users can inspect complete candidate Sections and direct rules/art references without giving the review UI write authority.
  - Relation indexing may be partial or unavailable without blocking Result, Changes or existing explicit Apply.

## Decision: 分离式 VXL 以装配体为交付单位，格式事实先探针后冻结

- Status: Accepted / Stage 1A implementation completed / executable structural acceptance later closed by Stage 1B
- Date: 2026-08-26
- Task(s): ASSET-VOX-1A
- Context:
  - RA2 载具可以由独立 Body、Turret、Barrel VXL/HVA 文件组成，单个 `VoxelSceneSnapshot` 或单文件
    多 Section 无法表达完整资产身份、父子关系和动画装配。
  - 本机真实样本包含单 Section 空名称 HVA，证明把所有命名偏差都作为致命格式错误会拒绝既有素材。
- Decision:
  - 完整体素资产以有根、无环的 `Ra2VoxelAssetAssembly` 表达，每个节点拥有独立 VXL/HVA identity；
    `Body -> Turret -> Barrel` 是基础拓扑而非后置特例。
  - Stage 1A 只公开 internal、只读、受限的装配、元数据和 VXLSE Slice Import 契约；同版本源码已冻结
    raster axis addressing、slice order、direct-alpha occupancy 和 palette quantization。
  - Slice importer 会保留透明位置的旧体素且不写 normals，所以只允许导入空 Section，并要求后续重新
    生成 normals；它调用 session-global land/air 的 DefaultTransforms，因此不能用来推断 pivot/mount。
  - 单 Section 空名称 HVA 只在配套 VXL 也唯一且无歧义时兼容；不把兼容性放宽为多义猜测。
  - 用户授权复用 `VoxelNormalForge`；后续只迁移经过审查和测试的 Core 逻辑，不引入其旧 CLI/WPF 层。
- Rejected Alternatives:
  - 把炮塔/炮管只当成同一 VXL 的 Section：不能覆盖常见多文件装配。
  - 直接把整个 VoxelNormalForge 工程作为 IDE 运行时依赖：带入无关 UI/CLI 和并行产品边界。
  - 只凭截图或论坛描述固定坐标轴和切片方向：会把推测固化为持久格式契约；本阶段改为审阅随包源码并
    对公式执行非对称全坐标往返测试。
- Consequences:
  - 1B 可在稳定装配边界内迁移 reader/writer、体素快照和法线算法，无需重做资产身份模型。
  - 真实 Barrel 样本不再作为装配模型门禁；它只影响后续 pivot/视觉/游戏标定。
  - executable GUI import 仍须在 1B 产生确定性 RGBA PNG 后独立验收，但不作为生产自动化协议。
- Follow-up:
  - 进入 1B Canonical Voxel Core 与 RGBA SliceStack exporter，再执行 PNG -> VXLSE -> decoded VXL 验收。

## Decision: Canonical voxel authority is one immutable part snapshot; VXL palette is external

- Status: Accepted / Stage 1B implemented / supplied VXLSE structural acceptance passed
- Date: 2026-08-26
- Task: ASSET-VOX-1B
- Context:
  - Generation, MagicaVoxel exchange, VXLSE slices and decoded VXL need one deterministic comparison surface without
    making any provider or GUI state authoritative.
  - VXLSE source declares the 768-byte VXL header `PaletteData` block never used; RA2 VXL colour interpretation depends
    on an external active palette.
- Decision:
  - One immutable internal `Ra2VoxelSceneSnapshot` describes one part only. Stage 1A continues to own detached assembly.
  - Canonical hashes use a versioned binary encoding, sorted cells/source hashes and copied palette/metadata values.
  - MagicaVoxel v150 is a restricted one-model exchange format; VXL is read-only and requires an explicit palette profile.
  - SliceStack PNG is exact 8-bit RGBA with no scale/interpolation/antialias. Direct VXL compilation remains frozen.
- Rejected alternatives:
  - Treat the VXL reserved palette bytes as actual colours: contradicts VXLSE and would create dark/incorrect slices.
  - Import all VoxelNormalForge code as a project dependency: introduces mutable models/CLI and insufficient input bounds.
  - Let each provider own its own voxel DTO/hash: prevents deterministic replay and cross-validation.
- Consequences:
  - Provider output can be normalized and compared before any game-format claim.
  - Supplied VXLSE structural readback has passed; normal, pivot/mount, HVA and game behavior remain unresolved.

## Decision: High-value voxel algorithms are derived from canonical snapshots, not imported as a parallel VXL model

- Status: Accepted / implemented / automated verified
- Date: 2026-08-27
- Task: ASSET-VOX-1F-CORE-1
- Decision:
  - Migrate the reviewed visible-face extraction and RA2/TS normal palette/baking logic from the user-authorized
    VoxelNormalForge source into Application-internal, bounded algorithms.
  - Surface projections and normal fields are immutable derived data bound to `Ra2VoxelSceneSnapshot.CanonicalHash`.
    They do not extend the canonical cell schema and are never persisted implicitly.
  - VOX and VXL use the same path after existing codecs converge on `Ra2VoxelSceneSnapshot`; VOX receives a generated
    normal review field without pretending to contain VXL `normalIndex` data.
  - Reuse one neighbourhood authority for style geometry, surface extraction and normal estimation.
- Rejected alternatives:
  - Reference the complete VoxelNormalForge project or copy its mutable `VxlModel`: creates a second voxel authority.
  - Export and reload OBJ inside the IDE: loses palette identity and adds unnecessary file I/O.
  - Add `normalIndex` to the canonical snapshot immediately: changes schema/authority before preservation and writer
    contracts are approved.
- Consequences:
  - Native 3D preview can consume exact visible faces without reconstructing geometry from SliceStack PNG.
  - A later VXL writer can consume a reviewed normal field, but existing-normal preservation and file materialization
    remain separately governed.

## Decision: Generation Provider Host uses a separate transient process boundary

- Status: Accepted / implemented / automated verified
- Date: 2026-08-26
- Task: ASSET-VOX-1C
- Context:
  - Application explicitly forbids process/file orchestration and owns deterministic content algorithms.
  - Existing Asset Provider closes final Manifest requirements and cannot represent GLB/VOX/PNG candidates.
  - General Job/Event/Artifact persistence remains deferred, while 1C needs bounded progress, cancellation and artifacts.
- Decision:
  - Add a separate headless AssetHost assembly with an internal versioned local-process protocol and transient workspace.
  - Freeze the internal boundary to `ProbeAsync`, `RunAsync` and an `IAsyncDisposable` read-only artifact lease; probe is
    readiness evidence only and Run repeats hash/identity/capability/license checks.
  - Recover storage only through a dedicated-root marker, per-run marker, exclusive active lock and bounded TTL janitor;
    this is orphan cleanup, not job recovery.
  - Drain stdout/stderr concurrently with fixed bounds and atomically arbitrate cancellation, timeout, protocol terminal,
    process exit and artifact-promotion races before success can be committed.
  - Treat process isolation as crash/resource isolation, not an OS security sandbox; only trusted configured executables run.
  - Keep Application public API/friends, final Asset Provider, project write authority and Stage 1B canonical voxel truth unchanged.
  - Prove the Host with a deterministic managed fixture; certify a real TRELLIS/Hunyuan adapter only in a separately
    authorized provider-specific slice.
- Rejected Alternatives:
  - Extend `IRa2AutomationAssetProvider`: would break final Manifest closure and conflate candidates with final assets.
  - Put `Process`/file workspace logic in Application: violates a tested pure/headless boundary.
  - Build the full persistent Job/Event/Artifact runtime now: speculative and explicitly deferred by the roadmap.
  - Claim working-directory containment is a security sandbox: technically false on Windows without AppContainer/container isolation.
- Consequences:
  - 1C can validate lifecycle and compatibility without installing or paying for a real model.
  - Callers can check readiness without starting generation, but cannot reuse probe as an authorization token.
  - Successful candidates remain usable only while the workspace lease is alive; no raw workspace path crosses the Host seam.
  - Child-process protocol/workspace remain R4 compatibility contracts; the approved internal v1 implementation is now
    covered by 38 AssetHost tests and remains non-public.
  - UI, remote APIs, persistence, project commit and real model quality remain later stages.

## Decision candidate: certify Hunyuan3D-2mini shape-only as the first real local provider

- Status: Proposed / P1-0 audit passed / external authorization required
- Date: 2026-08-26
- Task: ASSET-VOX-1C-P1
- Context:
  - The local workstation has an RTX 4080 SUPER with 16,376 MiB VRAM and Windows; Python ML dependencies and model
    weights are not installed.
  - Current official TRELLIS.2 guidance is Linux-tested and requires at least 24 GB VRAM, so it is not a credible local
    certification baseline for this machine.
  - Current official Hunyuan3D-2 guidance supports Windows and describes a lower shape-only memory requirement plus a
    smaller Hunyuan3D-2mini family, making it the smallest credible local candidate.
  - Hunyuan usage is governed by the Tencent Hunyuan 3D 2.0 Community License; the project cannot accept it for the user.
- Proposed decision:
  - Use one pinned Hunyuan3D-2mini shape-only revision as the first real provider, launched by a self-contained single-file
    adapter through the existing `ra2-voxel-generation/1` protocol.
  - Keep runtime, upstream source and weights in a user-owned external bundle; never install/download during product
    probe/run and never include them in the source package.
  - Treat seed behavior as `BestEffort` until real repeated runs prove stronger behavior; defer texture to avoid conflating
    PBR output with the later RA2 palette pipeline.
- Rejected alternatives:
  - TRELLIS.2 on the current Windows/16 GB machine: does not meet the official local baseline.
  - Persistent local HTTP server: weakens the existing one-run process-tree cancellation/timeout contract.
  - A second generation Host or voxel DTO: duplicates accepted 1C and 1B authorities.
- Decision gate:
  - Candidate becomes Accepted only after explicit user license acceptance and installation/download authorization, then
    successful P1-1 through P1-5 real certification.
- Follow-up:
  - After P1 passes, enter the separately contracted 1D GLB-to-canonical-voxel bridge.

## Decision: Tencent remote generation remains a provider adapter outside AssetHost

- Status: Accepted / P2-1 through P2-4 implemented; P2-3 remote path certified on explicitly authorized call 4
- Date: 2026-08-26
- Task: ASSET-VOX-1C-P2
- Decision:
  - Keep Tencent HTTP, key, billing confirmation, async JobId polling and signed artifact download inside one dedicated
    local adapter executable implementing the existing `ra2-voxel-generation/1` protocol.
  - Keep AssetHost process/workspace/protocol/lease implementation provider-neutral. After clearing inherited environment,
    retain only `SystemRoot`, `WINDIR`, `TEMP` and `TMP`, which are required by the Windows child runtime.
  - Require only the dedicated `RA2INI_HY3D_API_KEY` and explicit free-only confirmation. Never fall back to generic
    OpenAI/DeepSeek/CAM credentials.
  - Submit at most once per Host run; query only the returned JobId; admit only one HTTPS-downloaded GLB candidate.
- Consequences:
  - Remote provider evolution cannot contaminate Application canonical voxel authority or Host lifecycle policy.
  - API keys, proxy settings, `PATH` and arbitrary user environment remain outside the child process boundary.
  - Probe proves local configuration only; Tencent console remains the authority for free balance and postpaid state.
  - A local cancellation cannot cancel an already-submitted remote job until Tencent documents a cancel API; no candidate
    is committed locally and no automatic resubmission occurs.
  - The certified response omitted provider credit-consumption fields; Tencent console state remains authoritative before
    any later explicitly authorized call.

## Decision: GLB conversion is a deterministic internal one-part bridge

- Status: Accepted / implemented / automated verified
- Date: 2026-08-26
- Task: ASSET-VOX-1D
- Context:
  - Stage 1B already owns immutable single-part voxel truth and Stage 1A already owns Body/Turret/Barrel assembly identity.
  - The certified P2 GLB is one watertight connected geometry with no material colour or semantic part separation.
  - Application must remain headless and cannot perform AssetHost lease, file or process orchestration.
- Decision:
  - Add an internal, BCL-only restricted GLB reader and deterministic triangle/AABB plus exterior-fill voxelizer in
    Application.
  - Map glTF right/up/forward explicitly to canonical right/forward/up, require explicit resolution and palette policy,
    and produce one caller-declared part plus mandatory review facts.
  - Reject open, non-manifold, disconnected or unsupported geometry instead of silently repairing it.
  - Keep Application public allowlist 77, AssetHost exports 0 and all project-write/UI boundaries unchanged.
- Rejected alternatives:
  - Add a third-party general 3D runtime: unnecessary package/license surface for the certified restricted path.
  - Infer turret/barrel from connected components or node names: the real mesh is fused and such inference is not authoritative.
  - Recover colour from absent GLB material or default to green/remap: would encode a guess as canonical data.
  - Put voxelization in AssetHost or add a second voxel snapshot: would duplicate the accepted 1B authority.
- Consequences:
  - 1D can reliably produce a reviewable Body voxel candidate and existing VOX/SliceStack outputs.
  - Detached parts, palette recovery, pivot calibration, normals, HVA and final VXL/game validation remain explicit later stages.
- Follow-up:
  - After explicit approval, execute 1D-1 through 1D-5 with stage gates from the final contract.

## Decision: natural-language voxel styles compile to a locally validated deterministic plan

- Status: Accepted / implemented / automated verified
- Date: 2026-08-26
- Task: ASSET-VOX-1E
- Context:
  - Stage 1D produces one deterministic but uniform-colour canonical Body candidate; the accepted geometry-only GLB
    contains no trustworthy material or semantic-region information.
  - Users need an AGENTS-like natural-language style source, but built-in Agent Skills are application knowledge rather
    than mutable project content, and model prose cannot become canonical voxel truth.
- Decision:
  - Use bounded project/user `VOXEL_STYLE.md` files as authoring intent, resolved only along one contained ancestor chain.
  - Compile the ordered source pack through one dedicated structured DeepSeek call into an untrusted proposal, then use
    local schema, palette, region and remap validation to produce an immutable compiled style plan.
  - Recolour the existing `Ra2VoxelSceneSnapshot` deterministically without changing dimensions, coordinates, occupancy,
    part identity or source geometry. Cache only the fully keyed compiled plan as disposable derived data.
  - Permit text-only deterministic geometry shading, but require an explicit reviewed mask/source material/donor evidence
    before painting tyre, glass, accent or remap semantics.
- Rejected alternatives:
  - Register user styles in `Ra2AgentSkillCatalog`: mixes project content with bundled application guidance.
  - Inject style prose into the general INI Work prompt: expands unrelated authority and prompt budget.
  - Let DeepSeek return per-cell colours or paths: makes untrusted output large, brittle and authoritative.
  - Use AssetHost run storage as a cache: contradicts its leased transient-workspace contract.
- Consequences:
  - 1E can add reusable natural-language styling without changing provider, voxel geometry, public API or project Apply.
  - Text-only results remain coarse/review-required; semantic masks, UI, VXL/HVA and project adoption remain separate.
- Verification and remaining gate:
  - User approved 1E-1 through 1E-5 on 2026-08-27. Application 249/249, IDE 2787/2787 and AssetHost 47/47 pass;
    the existing 20,261-cell Body candidate replays to identical review artifacts without geometry/occupancy changes.
  - UI, a real DeepSeek style compile, project Apply/Save, VXL/HVA, normals and game validation remain separate decisions.

## Decision: Voxel style review is a dynamic central document with ephemeral acceptance

- Status: Accepted / implemented / automated verified
- Date: 2026-08-27
- Task: ASSET-VOX-1E-UI
- Decision:
  - Reuse the existing dynamic central `LayoutDocument` pattern rather than add a managed dock tool or separate window.
  - Admit only an explicitly selected project-contained `.vox`; opening/loading remains local and provider-free.
  - Keep provider invocation behind the explicit compile command and publish results atomically through a generation-owned
    headless coordinator.
  - Treat “accept” as an in-memory workspace decision only; do not imply Apply, Save, export, VXL or HVA authority.
- Consequences:
  - The workspace cannot pollute persisted startup layout or appear during layout restoration.
  - The UI projects existing 1E artifacts and cannot become a second colourization implementation.
  - Real DeepSeek use, semantic-mask authoring and downstream artifact handoff remain separately authorized boundaries.

## Decision: VXL style input requires an explicit PAL and converges on canonical voxel truth

- Status: Accepted / implemented / automated verified
- Date: 2026-08-27
- Task: ASSET-VOX-1E-UI-R2
- Context:
  - Users should not need to understand an internal VOX-only staging format when an existing VXL asset is the source.
  - The existing Stage 1B readers already decode MagicaVoxel VOX, Westwood VXL and Westwood PAL data into the same
    immutable `Ra2VoxelSceneSnapshot` authority.
  - A VXL file's reserved palette bytes are not a trustworthy substitute for the theater/unit palette that gives its
    indices meaning, and one VXL may contain more than one Section.
  - Ordinary material recolouring is independent of RA2 team-colour/remap semantics.
- Decision:
  - The style workspace accepts project-contained single-model VOX directly and single-Section VXL only with an explicit
    project-contained 768-byte PAL selected by the user.
  - Both input paths reuse the Stage 1B codecs and converge before compilation; the UI does not own a second parser or
    colourization implementation.
  - No embedded-palette guess, implicit `unittem.pal` search or silent first-Section selection is allowed.
  - When a palette has no remap range, ordinary roles remain valid. Only non-executable, text-inferred remap roles may be
    removed, and their intent must be retained as a bounded unresolved assumption. Explicit or executable remap still
    fails closed.
- Rejected alternatives:
  - Require conversion to VOX before opening: exposes an unnecessary implementation detail and adds user friction.
  - Treat the VXL reserved block as authoritative or guess a PAL by filename: can produce convincing but incorrect colour.
  - Silently use the first VXL Section: loses part identity and makes later turret/barrel work ambiguous.
  - Require remap colours for every recolour: confuses optional faction tinting with ordinary material shading.
- Consequences:
  - Existing VOX workflows remain compatible, while common one-Section VXL assets can be reviewed without pre-conversion.
  - Multi-Section VXL needs a later explicit part/Section selector; VXL/HVA writing remains outside this stage.
  - The extra PAL prompt is intentional evidence collection, not a file-format conversion step.
- Follow-up:
  - Contract a multi-Section part selector only when turret/body/barrel review enters scope.
  - Contract accepted-preview handoff and VXL/HVA materialization separately from this read-only workspace.

## 2026-08-27 — Native 3D is a derived view over the canonical voxel surface

- Task: ASSET-VOX-1E-UI-3D
- Context: SliceStack is valuable for importer diagnosis but does not provide a usable primary spatial review surface.
- Decision:
  - Use native WPF `Viewport3D` and the existing `Ra2VoxelSurfaceProjector`; do not add HelixToolkit or another voxel model.
  - Keep `Ra2VoxelSceneSnapshot` as the sole truth and keep all WPF geometry frozen, cancellable, session-only presentation state.
  - Use 3D for original/result/region, retain Palette as 2D and retain SliceStack as explicit/failure fallback.
  - Label current lighting as geometry review only; it must not imply VXL normal-index or game-lighting fidelity.
- Rejected alternatives:
  - Render one cube per occupied voxel: needlessly emits internal faces and scales poorly.
  - Import the old VoxelNormalForge UI/project wholesale: creates a parallel model, writer and dependency boundary.
  - Replace SliceStack entirely: removes valuable axis/import diagnostics.
- Consequences: current VOX/VXL review becomes spatially usable without changing file or persistence semantics; multipart
  composition and normal/game-lighting review remain separate stages.

## 2026-08-27 — Refine voxel conversion candidates without editing the source model

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-2A
- Context:
  - Direct target-grid rasterization preserved high-frequency bumps and weak X symmetry in the accepted Body candidate.
  - The user explicitly excluded source-model adjustment; current DeepSeek transport remains text/tool-only.
- Decision:
  - Freeze admitted mesh geometry and add a deterministic 2x-supersampled occupancy candidate with one bounded cleanup
    pass, exact protected-coordinate survival and volume/silhouette/connectivity gates.
  - Keep symmetry as a separate local-support `Suggest` candidate; do not add silent enforcement.
  - Add deterministic normal/semantic review facts and a palette-only body contrast candidate that preserves explicit,
    semantic and remap selections.
  - Add an internal one-to-three-round early-stoppable DeepSeek coordinator over structured facts; fake tests only.
- Rejected alternatives:
  - Taubin/bilateral source-mesh displacement: outside the approved source-model freeze and unnecessary for conversion
    aliasing reduction.
  - Model-written cells or palette bytes: makes untrusted output canonical authority.
  - Global side-copy symmetry: can erase intentional details and unsupported thin structures.
  - Treat inferred tyre/glass labels as executable masks: text-only evidence is insufficient.
- Consequences:
  - Application can produce safer direct/refined/symmetry review candidates without provider regeneration or persistence.
  - UI composition, live DeepSeek, authoritative visual masks, VXL/HVA and Apply/Save remain independent later decisions.

## 2026-08-27 — Compose geometry candidates and colour candidates as separate session decisions

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-2A-UI
- Context:
  - The 2A Direct/Refined path requires the admitted GLB; an existing VOX/VXL alone cannot recreate the mesh-derived
    supersampled candidate.
  - Geometry quality selection and colour-plan acceptance have different evidence and must not silently replace each other.
- Decision:
  - Require an explicit project-contained GLB and expose Verified/UserPaired/Mismatch provenance.
  - Keep Current/Direct/Refined/optional Symmetry as immutable geometry views, with a separate explicit session-use action.
  - Compile the existing style pipeline against the selected session geometry and publish ordinary/optional contrast
    results separately; ordinary valid output is never rejected because contrast is unavailable.
  - Keep all state IDE-internal and session-only; no writer, serializer, project mutation or provider call is added.
- Rejected alternatives:
  - Pretend to refine an already rasterized VOX without the GLB: it would misrepresent the 2A algorithm.
  - Automatically replace the baseline with Refined: it hides provenance and removes meaningful comparison.
  - Merge contrast optimization into mandatory colour validation: it would repeat the earlier over-strict validation error.
- Consequences:
  - The user can visually compare and compose geometry and colour candidates in one 3D workspace.
  - A UserPaired GLB remains review-required, and materialization to VOX/VXL/HVA still needs a separate contract.

## 2026-08-27 — Connectivity is a relative candidate-quality fact, not an absolute one-piece rule

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-2A connectivity correction
- Context:
  - The certified Body mesh is one piece, but the 50% downsample candidate contained one detached voxel and was rejected
    by the absolute six-neighbour single-component rule.
  - One disconnected cell among 17,181 occupied cells is not evidence that a vehicle has been split into unsafe parts.
- Decision:
  - Reuse `Ra2VoxelSceneSnapshot.Connectivity` as the sole truth and admit multiple components only when one dominant
    component contains at least 95% of all occupied cells; reject materially fragmented output.
  - Correct the default coverage threshold to 40%, which makes the certified product-path candidate one component while
    retaining the existing 5% occupied-volume and 3% silhouette gates.
  - Show component count and dominant-body share in review metrics rather than hiding attachment evidence.
- Rejected alternatives:
  - Keep the absolute one-component gate: it confuses voxel adjacency artefacts with semantic vehicle parts.
  - Remove connectivity validation entirely: it would admit genuinely fragmented candidates.
  - Relax volume/silhouette gates: unnecessary because the 40% evidence-backed candidate already passes them.
- Consequences:
  - Small detached details no longer cause a false failure, while candidates without a dominant body still fail closed.
  - No source geometry, canonical snapshot schema, provider, writer, project persistence or public API changed.

## 2026-08-27 — ASSET-VOX-2A-R2 protects topology before smoothing

- Status: Accepted / implemented / automated verified
- Decision:
  - Treat sustained rod/plate components, endpoints and attachment neighbourhoods as frozen/transition evidence.
  - Permit smoothing only on the remaining body field and admit only candidates that are both safe and measurably better.
  - Replace the dominant-body exception with no-new-component and no-new-cavity gates.
  - Return `NoSafeImprovement` and retain Direct when no candidate qualifies.
- Rejected alternatives:
  - Continue deleting all non-protected one-neighbour cells; that already shortened a barrel.
  - Use a weighted quality score; a good aggregate could hide one destroyed critical structure.
  - Let a model or visual guess override cell-level gates; semantic confidence is not topology evidence.
- Consequences:
  - Clean geometry may intentionally remain unchanged.
  - Candidate review gains deterministic difference and structure-protection evidence without changing persistence or writers.

### Physical-review correction

- Protection connectivity must include a directional signature. Axis-agnostic adjacency can merge unrelated roof, hull and
  turret surface patches into one false semantic structure.
- A difference view is meaningful only for an admitted, non-identical candidate with non-zero added/removed cells. A
  Direct fallback must remain labelled Direct and must not expose duplicate Refined/Difference actions.
- Conservative cleanup may remove an endpoint only when its sole neighbour is a well-supported body cell; an endpoint whose
  neighbour is part of a low-degree chain remains preserved even if semantic classification is uncertain.

### Physical-review correction 2

- A candidate may not be called surface refinement when it is produced by sequentially deleting individually eligible
  cells. Scalar roughness/support improvement does not compensate for visually scattered delta topology.
- Geometry changes now require three independent facts: a weighted local surface proposal, matching supersampled GLB
  occupancy evidence for the change direction, and membership in a bounded 26-neighbour delta component.
- Singleton changes are discarded before quality scoring. Conservative and Balanced differ only by minimum coherent
  component size, preserving deterministic selection without two unrelated algorithms.
- Source mesh mutation, learned per-cell output and unrestricted morphology remain rejected; the canonical Direct model,
  frozen structures and existing topology/volume/silhouette gates retain authority.

## 2026-08-27 — AI classifies bounded regions; deterministic code owns symmetry edits

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-2B
- Decision:
  - Keep local Direct/Refined generation provider-free and expose structure recognition as a separate two-request action.
  - Send only host-owned region facts and compact silhouettes; DeepSeek cannot return coordinates or geometry edits.
  - Reconcile two rounds locally. Agreement at confidence >=0.80 may classify a region; disagreement becomes uncertain.
  - Permit edits only on deterministic mirrored pairs belonging to confirmed structural core. Preserve attachment, thin,
    uncertain and transition occupancy exactly and retain all topology/quality gates.
- Rejected alternatives:
  - Force the entire vehicle to be symmetric: this already damaged barrels and intentional accessories.
  - Let DeepSeek return a voxel mask: it would make probabilistic output the geometry authority.
  - Reuse structural labels as material labels: structure does not prove glass, tyre, metal or remap identity.
- Consequence: symmetry is explicit, review-first and disposable; a separate material-semantic colouring stage is required.

## 2026-08-28 — Bound fragmented symmetry evidence without dropping geometry

- Status: Accepted / implemented / automated and real-product-path verified
- Task: ASSET-VOX-2B physical-sample correction
- Context:
  - The certified Body sample creates more than 64 disconnected mismatch/protected components after local refinement.
  - One-region-per-component made valid local candidates impossible to send through the bounded two-round classifier.
- Decision:
  - Preserve every coordinate in Host-owned derived evidence, but summarize mismatch components through deterministic
    lateral, height, depth and morphology buckets; summarize protected components as one exact union.
  - Include connected-component count as an explicit region fact and retain the existing 64-region and prompt limits.
  - Preserve typed evidence failure details through the IDE result so a future boundary failure is observable.
- Rejected alternatives:
  - Raise the tool/prompt region limit: it increases cost and output fragility without improving semantic structure.
  - Truncate small mismatch components: it silently discards geometry and invalidates partition coverage.
  - Call DeepSeek before Host compaction: the current provider protocol does not grant model-owned coordinates or masks.
- Consequences:
  - The real Body sample reaches explicit AI recognition while exact-coordinate symmetry execution and all local gates
    remain authoritative.
  - Aggregated regions may contain multiple disconnected components; the compiler must interpret them as one bounded
    spatial/morphological class, not as one physical connected part.

## 2026-08-28 — Strict semantic identity, tolerant provider representation

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-2B visual/provider correction
- Decision:
  - Normalize common equivalent required-tool JSON forms, but retain exact evidence hash, selected plane, known region IDs,
    complete coverage and bounded disposition/confidence validation.
  - Keep pre-AI geometric difference and post-AI structural semantics visually separate. Blue is not a synonym for
    symmetry; cyan identifies confirmed symmetric core.
  - Add stronger deterministic surface candidates only behind the existing topology and quality gates.
- Rejected alternatives:
  - Treat every extra/missing optional JSON field as a malformed proposal; this rejects semantically valid provider output.
  - Accept incomplete or invented model regions; that would let probabilistic output escape Host-owned evidence.
  - Paint local frozen cells blue in Difference; users reasonably interpret that as AI-recognized symmetry.
- Consequences:
  - Provider formatting variance no longer blocks safe classification, while geometry authority remains deterministic.
  - Difference review is visually stronger and semantically honest; live provider quality still requires manual acceptance.

## 2026-08-28 — Surface cleanup is not automatically equivalent to smoothing

- Status: Accepted / implemented / focused and certified-local-sample verified
- Task: ASSET-VOX-2B selection/diagnostic self-audit
- Decision:
  - Require a material roughness reduction before any candidate may become the automatic Refined result.
  - Rank admitted smoothing candidates by roughness before low-support count; keep aggressive cleanup as review-only when
    it does not satisfy the smoothing threshold.
  - Expose each candidate's delta and quality facts in the existing review surface and include all behavior parameters in
    derivation identity.
  - Preserve exact semantic evidence authority while reporting parser and partition failures precisely.
- Rejected alternatives:
  - Prefer the candidate that removes the most low-support cells; the real sample showed this can reward erosion.
  - Hide non-selected candidates; that makes “no visible difference” impossible to diagnose.
  - Relax missing/unknown semantic regions; that would make model output the coordinate authority.
- Consequences:
  - The selected candidate is smaller but demonstrably smoother; strong cleanup remains observable without being applied.
  - A live provider may still fail its content contract, but the UI now reports whether the cause is a missing tool call,
    invalid JSON/field, or evidence/region mismatch instead of one generic error.

## 2026-08-28 — Model-facing symmetry evidence must describe repair questions, not host conclusions

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-2B neutral repair-evidence correction
- Context:
  - `core` contained only already mirrored cells, while unmatched region IDs described themselves as attached details.
  - Live two-round recognition consequently protected every repair opportunity and could never produce a changed core pair.
- Decision:
  - Keep exact coordinates and execution Host-owned, but present unmatched groups as neutral `repair-*` candidates.
  - Add mirror-target GLB coverage and body-contact facts so DeepSeek can distinguish a missing hull counterpart from a
    genuinely one-sided accessory using bounded evidence.
  - Keep two-round agreement and all local safety gates unchanged; improve the critic instruction instead of weakening
    reconciliation.
- Rejected alternatives:
  - Force every mismatch to become symmetric locally; this would repeat the barrel/accessory damage.
  - Relax disagreement to first-round authority; this removes the independent review boundary.
  - Add a third model call; the missing information was in the evidence contract, not the number of retries.
- Consequences:
  - The model can now admit actual repair regions without coordinates or edit authority.
  - Provider quality still requires one live manual acceptance; no-op remains a safe and explicit result.
- Follow-up:
  - Rebuild/restart the IDE and repeat the same Body sample recognition. Confirm that supported hull repair regions become
    cyan while genuine thin/accessory regions remain blue/amber or violet.

## 2026-08-28 — Agent owns sparse geometry intent; disagreement receives a third arbitration pass

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-2C
- Decision:
  - Replace the production two-round closed region classifier with an Agent-owned sparse operation proposal over
    Host-known aggregate/component target IDs.
  - Permit the primary pass one bounded coordinate-free detail query. An independent reviewer returns its own complete
    proposal; compare only the sorted `(target_id, action)` executable fingerprint.
  - Invoke a third analysis only when those fingerprints differ. Optional query plus arbitration is capped at four total
    calls; there is no hidden retry or provider/model switch.
  - Host expands only the final `add_mirror` / `remove_source` operations and retains stale/bounds/overlap/protection,
    connectivity, cavity, volume and silhouette safety. It no longer changes direction through coverage, roughness,
    support or semantic-label heuristics.
  - Project exact selected subcomponents in the existing structure view and show the final candidate against its refined
    baseline as a real 3D geometry difference.
- Supersedes:
  - The ASSET-VOX-2B rule that local two-round label agreement is the semantic authority for production symmetry edits.
    Its immutable evidence, compaction, protection facts and safety analyzers remain reused.
- Consequences:
  - Intentional asymmetry can be preserved because omitted targets are no-ops and Agent direction is authoritative.
  - Provider cost is usually two analysis calls, three on disagreement, and at most four when one detail query is also used.
  - The result is still session-only and review-first; no Shell, persistence, Apply/Save or VXL/HVA authority is added.

## 2026-08-28 — Image-driven generation enters through a fixed bundle and session-only façade

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-3A
- Decision:
  - The first product generation path is exactly one explicit PNG/JPEG reference image to one fixed bundled Tencent
    Hunyuan 3D Provider job. Text-only generation and arbitrary provider discovery remain unsupported.
  - The IDE references a narrow public AssetHost façade. Host protocol DTOs, process handles, workspace paths and leases
    remain internal; successful artifacts are copied into bounded owned memory before lease disposal.
  - Probe is offline. The remote job starts only after an explicit per-run privacy/cost confirmation. There is no retry,
    persistence, project write or asset writer.
  - Generated GLB becomes a `GeneratedSession` source with no invented file path. Existing 1D voxelization, 2A local
    quality candidates, 1E style inheritance and user-triggered 2C structure recognition are reused.
- Consequences:
  - Natural-language design text is provenance only for the current image-driven Provider and is labelled accordingly.
  - Live provider behavior remains unverified until a separately approved manual probe.

## 2026-08-28 — Explicit immutable candidate is the sole VOX export authority

- Status: Accepted / implemented / automated verification pending final gates
- Task: ASSET-VOX-3B
- Decision:
  - A user action freezes exactly one materializable canonical snapshot as immutable session state. Merely switching review
    modes does not change it; changing the source, adopted geometry or compiled style invalidates it.
  - VOX export is Save-As from that frozen snapshot only. Difference, structure-region, mask and palette projections are
    review surfaces and can never become export authority.
  - Reuse `Ra2MagicaVoxelCodec`; write a same-directory temporary file, physically flush, decode/re-encode with exact byte
    equality, then publish atomically. The currently loaded source VOX cannot be overwritten in this phase.
  - Export remains independent from project Apply/Save, manifests, asset registration and VXL/HVA materialization.
- Rejected alternatives:
  - Export the current visible mode directly; view navigation could silently change file content.
  - Add another VOX writer in the IDE; this would split format authority and round-trip behavior.
  - Treat export as project Save; that would mix an asset-copy operation with document transaction semantics.
- Consequences:
  - Generated/session-only or reviewed candidates can now become real VOX files without giving the Agent direct disk
    authority.
  - VXL/HVA, separated assembly and project binding remain explicit later phases.

## 2026-08-28 — Voxel workspace camera is session presentation state, not authoring truth

- Status: Accepted / implemented / automated verified; physical DPI review pending
- Task: ASSET-VOX-UI-R1
- Context:
  - The current workspace rebuilds the 3D scene for review-mode changes and unconditionally resets the camera after every
    successful build. Temporary AvalonDock unload/load also clears and rebuilds the scene.
  - The page simultaneously measures every workflow and evidence surface inside one two-axis ScrollViewer, making Dock
    resizing and evidence growth appear as whole-workspace scaling or jumping.
- Decision:
  - Keep source/candidate/semantic/export authority in the existing ViewModel and services. Camera pose, selected workflow
    page, selected evidence page and internal splitter lengths are session-only presentation state owned by the current View.
  - Restore camera pose by normalized target and bounds-relative distance within one source session. Auto-fit only on first
    valid scene, a genuinely new source, invalid state or explicit user reset; review-mode changes preserve the view.
  - Recompose the document as a task inspector, dominant adaptive 3D viewport and resizable tabbed evidence area without
    changing Shell or introducing root-level scale transforms.
- Rejected alternatives:
  - Persist camera and internal panel state in project/user settings; the first UI correction does not justify a new format
    or migration surface.
  - Put camera state into the authoring ViewModel; it would mix presentation lifecycle with canonical candidate state.
  - Fix the symptom with fixed DPI-specific sizes, `Viewbox` or repeated SizeChanged auto-fit; these approaches create new
    zoom jumps and make font/input geometry inconsistent.
  - Modify Shell to distinguish close from transient unload; the local View can preserve a lightweight pose and release the
    heavy scene without expanding the frozen Shell boundary.
- Consequences:
  - The same model can be compared across original/direct/refined/difference/structure/colour views without losing the
    user's review angle.
  - Camera state is intentionally lost when the document instance is closed; cross-session layout persistence remains a
    separately approved enhancement.
- Verification:
  - The workspace now uses task/evidence tabs, two bounded splitters and a dominant adaptive 3D viewport. No root
    `Viewbox`, scale transform or full-page two-axis scroller was introduced.
  - Camera pose is restored by normalized target and bounds-relative distance inside one real source identity. Repeated
    `SourcePath` notifications no longer masquerade as source replacement; original-snapshot hash changes still start a
    new camera group for a generated source with the same display label.
  - Automated camera/layout/voxel suites and the build pass. Physical 1920×1080 at 100%/125% remains the user acceptance
    gate before this visual baseline is considered screenshot-certified.

## 2026-08-29 — Current working geometry, not GLB reconstruction, is the next-pass authoring baseline

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-3C
- Context:
  - The workspace can adopt a Refined or Agent candidate, but the next quality pass captures the original source and
    rebuilds Direct/Refined from the old GLB.
  - The success path explicitly clears `_workingGeometry`, so a later Agent pass is correctly hash-bound to the wrong
    branch and can visually restore the earlier model.
- Decision:
  - Keep the admitted source root immutable and introduce one explicit, revisioned, session-only working geometry state.
  - Derive every later local/Agent candidate from the captured working snapshot. Treat GLB only as alignment/coverage
    evidence and reject ambiguous registration instead of rebuilding from it.
  - Advance working state only through the existing explicit adoption action. Read-only candidate generation preserves
    valid style and frozen export candidates; actual working adoption invalidates them.
  - Keep lineage outside canonical snapshot serialization and store only root/current/parent facts, not persistent history.
- Rejected alternatives:
  - Mutate the source snapshot, use exported VOX as a state bus, or patch only the ViewModel call site.
  - Make every previously authored cell permanently immutable; later explicit proposals may still remove cells relative to
    the current baseline.
- Consequences:
  - Repeated refinement/Agent passes form one visible linear chain and cannot silently return to an old GLB branch.
  - A true persistent history/branch/undo system and VXL/HVA materialization remain separate future contracts.

## 2026-08-29 — Center-seam gaps are explicit Agent targets, not automatic Host fill

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-3D
- Context:
  - `add_mirror` cannot repair an empty self-mirrored center cell or an empty two-cell half-plane seam once both occupied
    sides are already symmetric.
- Decision:
  - Derive bounded, hash-bound `seam-gap-*` targets only for one/two empty X-axis center cells with immediate occupied
    anchors on both sides.
  - Add `bridge_center_gap` as an Agent-selected operation. Host code expands only the selected known target and retains
    existing physical safety gates.
  - Keep arbitrary/off-axis/three-cell holes outside this action and keep every omitted seam unchanged.
- Rejected alternatives:
  - Automatically fill every center gap after symmetry; this could seal deliberate windows, rings or apertures.
  - Overload `add_mirror`; a self-mirrored empty cell has no occupied source coordinate and would make the operation
    semantically ambiguous.
  - Return raw empty coordinates to DeepSeek; exact geometry remains Host-owned.
- Consequences:
  - Short seam repair is reviewable and Agent-led without weakening the 2C authority boundary.
  - Longer or off-axis repair needs a separate evidence/action contract rather than silent rule expansion.

## 2026-08-29 — Voxel semantics use AI suggestions plus authoritative human overrides

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-4A
- Context:
  - DeepSeek V4 receives text, not rendered voxel pixels. Geometry alone cannot authoritatively distinguish glass, rubber,
    lights, openings or team-colour intent.
  - The existing colourizer already owns hash-bound explicit masks and palette-safe application; a parallel painter would
    duplicate authority.
- Decision:
  - Host code derives only bounded spatial facts and binary region masks from the current working snapshot. DeepSeek may
    suggest part/material labels through a two-pass, conditional-third-pass text tool.
  - Human overrides outrank AI suggestions. Only a human action can approve remap; Unknown remains valid.
  - Materialize effective assignments through existing `Ra2VoxelExplicitMask` and `Ra2VoxelColourizer`. Re-analysis of the
    same working hash preserves manual overrides; a working-geometry transition invalidates all semantic state.
- Rejected alternatives:
  - Pretend the text model saw the render, infer colours from geometry, or let Host heuristics veto flexible semantic output.
  - Allow AI to approve remap or directly write palette indices.
  - Build a second colourizer or persist semantic session state in project/asset formats.
- Consequences:
  - The workflow is useful with partial AI knowledge and explicit human correction, while geometry and writer authority stay unchanged.
  - Fine semantic boundaries and visual-reference material recognition remain future work rather than false automatic claims.

## 2026-08-29 — Fine voxel semantics are a sparse human overlay over the Agent seed

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-4B
- Context:
  - 4A region-level assignments cannot separate glass, tyre or attachments that share one coarse Host region.
  - Giving a text-only model raw cell coordinates or rebuilding Host partitions around every correction would enlarge the
    wrong authority boundary.
- Decision:
  - Keep accepted Agent suggestions and region human assignments immutable as the seed. Store cell-level human corrections
    in a session-only, working-hash-bound sparse overlay.
  - Resolve `cell human > region human > accepted Agent > Unknown`, then group the final per-cell result into the existing
    explicit-mask/style/colourizer path.
  - Use the existing 3D hit test for a bounded surface brush. Drag/zoom remain camera gestures; a short click edits only in
    explicit paint/erase mode. Mirror is one atomic operation and brush undo/redo is local to this overlay.
- Rejected alternatives:
  - A second painter that writes palette indices directly, persistent semantic metadata, or model-authored cell lists.
  - A new window or dense slice editor in this stage; the current region controls plus a compact surface brush provide the
    required correction path with substantially lower UI complexity.
- Consequences:
  - Material boundaries can be completed manually without a multimodal stage, while AI remains a useful starting point.
  - Hidden/internal voxel painting and persistent mask interchange remain outside this stage; visible material authoring is
    available by rotating the existing 3D model.

## 2026-08-30 — Voxel pointer ownership separates semantic action from camera navigation

- Status: Accepted / implemented / automated verified
- Task: ASSET-VOX-4B-FIX2
- Context:
  - Physical WPF smoke showed that the 4B left-button short-click heuristic can remain silent even after semantic-state
    admission was corrected. Left press currently starts Orbit, while left release later attempts to reinterpret the gesture.
  - The viewport discards the actual hit triangle and guesses a cell by nearest centre.
- Decision:
  - Reserve left click for semantic select/paint/erase and right drag anywhere on the main input surface for Orbit.
  - Preserve Shift+right/middle pan and wheel zoom. Keep reset on the existing button.
  - Carry an IDE-internal scene-lifetime face-to-canonical-coordinate hit map from existing surface projection order; never
    fall back to nearest-cell guessing.
- Rejected alternatives:
  - Increase the 4-DIP threshold, delay painting until mouse-up, retry hit testing, or keep left-button camera ownership.
  - Introduce another geometry picker or external 3D dependency.
  - Add continuous drag painting before single-click identity and undo semantics are proven reliable.
- Consequences:
  - Navigation works from model or blank viewport space and can no longer consume a paint click.
  - Scene results gain derived nonserialized hit metadata; canonical geometry and semantic authority remain unchanged.
  - The pointer portion of the 2026-08-29 4B decision is superseded: left click is semantic action, while camera orbit belongs to right drag.
  - Physical WPF smoke remains required because source/static tests cannot synthesize trustworthy end-to-end WPF 3D mouse input.

## 2026-08-30 — Continuous semantic painting is one cancellable transaction, not repeated clicks

- Status: Accepted / implemented / automated verified / physical acceptance pending
- Task: ASSET-VOX-4B-STROKE-1
- Context:
  - FIX2 provides exact visible-surface hits, but the current click handler immediately creates a layer, undo item,
    composition and full scene refresh for every invocation.
  - Calling it from MouseMove would create fragmented undo history and rebuild storms, while part/material roles already
    exist in the effective assignment but the viewport only visualizes material.
- Decision:
  - Let the viewport own pointer capture, <=4-DIP exact hit sampling, ordered seed deduplication and a presentation-only
    temporary path overlay. Let the ViewModel own the frozen base layer and atomic begin/commit/cancel lifecycle.
  - Extend the existing Application mask editor with one deterministic multi-seed operation; retain the single-seed method
    only as an adapter to that same implementation.
  - Commit once on left release, create at most one undo item and one formal scene refresh, and cancel without mutation on
    capture/scene/hash/mode/camera transitions.
  - Add an IDE-only Part/Material review dimension. Annotation colours visualize existing effective assignments and never
    become palette indices or semantic authority.
- Rejected alternatives:
  - Repeatedly invoke the click handler, commit on every move, infer missing 3D cells between hits, or mutate the manual
    layer during pointer sampling.
  - Put stroke state in Application, create a second painter/composition, or encode annotation RGB into VOX output.
- Consequences:
  - Fast and slow drag produce one reviewable, undoable surface operation without hidden/back-face painting.
  - Physical WPF input and DPI smoke remains mandatory; hidden/internal voxel editing stays out of scope.

## 2026-08-30 — Persist voxel semantics as provenance-preserving sidecar layers

- Status: Accepted / implemented / automated verified / physical acceptance pending
- Task: ASSET-VOX-4D
- Context:
  - The current semantic authoring state is session-only, while exporting colours cannot preserve part/material identity or provenance.
  - Saving only final effective cells would incorrectly elevate accepted Agent suggestions to human authority.
- Decision:
  - Persist accepted Agent suggestions, human region overrides and human cell overrides as separate layers in a strict,
    project-contained `.semantic.json` v1 sidecar.
  - Bind restoration to exact working snapshot, deterministic evidence and reconstructed manual-layer hashes.
  - Reuse the existing LayerResolver/MaskComposer and atomic text writer; never embed the sidecar into VOX/VXL/HVA or project INI.
- Rejected alternatives:
  - Save review RGB, serialize only region IDs, materialize every effective cell as human, or guess cross-hash migration.
  - Autosave or silently auto-load in the first persistence version.
- Consequences:
  - Exact matching semantic work can survive an IDE restart without changing model or palette authority.
  - A matching geometry file is still required; undo/redo and global Shell close protection remain outside v1.

## 2026-08-30 — Mask-driven colour uses typed technique policies and multidimensional quality admission

- Status: Proposed / refined by the ASSET-VOX-4E Rev.2 decision below / awaiting user approval
- Task: ASSET-VOX-4E
- Context:
  - The existing style compiler and colourizer can apply semantic masks, but semantic requirements currently arrive after
    compilation, cache identity omits them, and the integrator picks the first role in a category.
  - The user clarified that built-in templates mean colouring rules and techniques, not colour/faction/theatre themes.
    Quality still needs an explainable admission model without pretending one numeric score proves artistic correctness.
- Decision:
  - Add five immutable, versioned technique policies controlling relative luminance, region order, edge handling, material
    separation and quality thresholds. They contain no hue, RGB, palette index, faction or theatre theme.
  - Keep colour intent in the existing built-in/project/directory/request `VOXEL_STYLE.md` source pack. Apply the selected
    technique locally after the bounded structured compiler/cache path, without adding another provider call.
  - Project each composition into a full CompositionHash plus a role-set RequirementShapeHash. Reuse a compiled plan when
    only cell boundaries change; recompile when the required material/remap set changes.
  - Replace category-first guessing with a validated MaterialRole-to-roleId binding plan. PartRole remains review-only in
    4E v1, and remap still requires explicit human approval.
  - Admit candidates through Blocked / NeedsReview / ReviewReady facts covering invariants, semantic coverage, palette fit,
    readability and distribution. Soft warnings require an explicit generation-scoped acknowledgement before freeze.
- Rejected alternatives:
  - Colour-themed templates, separate template painter, hard-coded RGB/palette indices, automatic technique guessing,
    opaque total quality score, or treating review annotation colours as output palette truth.
  - Include exact composition counts in the provider cache key and repeat a paid call after every brush stroke.
  - Extend PartRole-specific palette families, persist template selection in the 4D sidecar, or write VXL/HVA in 4E.
- Consequences:
  - The same colour intent can be rendered with different, reproducible shading techniques without changing semantic masks.
  - Provider call count remains governed by the existing style cache/compiler; technique normalization is local.
  - Quality failures and warnings become reproducible and inspectable without claiming GameReady.
  - Compiler/cache schema, binding contract and candidate admission change remain R4 and require approval before code.
- Follow-up:
  - Approve `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md`, then implement 4E-1 through 4E-5 with
    stage gates and physical WPF acceptance.

## 2026-08-30 — Voxel colouring knowledge is a Chat Skill before it becomes compiler policy

- Status: Accepted / implemented / focused automated verified
- Task: ASSET-VOX-4E-R1
- Context:
  - The user supplied eight VXL/VOX model archives, established VXLSE III RA2 palette identity, and requested ground/air
    colouring-rule research plus a built-in Skill for DeepSeek.
  - The general Agent Skill catalog and the dedicated `Ra2VoxelStyleCompiler` are separate prompt paths. Wiring research
    directly into the dedicated compiler would start the still-unapproved R4 4E implementation.
- Decision:
  - Preserve one evidence study and one Chat-only `ra2-voxel-colour-techniques` Skill in the existing auto-discovered
    Agent Skill catalog. Route only prompts containing both voxel-format and colouring/material markers to its domain.
  - Keep the Skill advisory: no coordinates, masks, binary writes, Apply/Save, VXL/HVA or GameReady claims.
  - Treat VXLSE III `RA2/unittem.pal` as the studied palette profile, not a universal theatre palette. Keep remap 16-31
    explicit-mask-only and keep palette/semantic/geometry facts authoritative over prose.
  - Leave the dedicated style compiler, cache, colourizer and 4E UI unchanged until the Proposed Contract is approved.
- Rejected alternatives:
  - Put the same long rule body into both `AgentSkills` and `VoxelStyles/compiler`; duplicate knowledge would drift.
  - Route every VXL/art request to the colour Skill; rules/art bindings and binary asset references are different domains.
  - Copy user or public model assets/palettes into the repository, or infer redistribution rights from public downloads.
- Consequences:
  - General DeepSeek Chat can select a source-backed ground/air colouring knowledge package without new tool authority.
  - The dedicated mask-driven compiler still does not consume the Skill; integrating one authoritative rule source remains
    part of the approved 4E implementation rather than a hidden prompt change.
  - Focused catalog/routing/prompt tests pass 16/16. Full tests, package, real DeepSeek and WPF/game visual validation were
    not run for this content-and-routing slice.

## 2026-08-30 — Voxel body colour is anchored by a human palette selection and adapted by unit class

- Status: Proposed / refined by the ASSET-VOX-4E Rev.3 decision below / awaiting user approval
- Task: ASSET-VOX-4E FinalContract Rev.2
- Context:
  - The user requires the coloured model to be centred on a manually chosen base colour rather than a template colour or
    model-generated guess.
  - Ground/air sample research disproves a universal darker-underside rule, and the original Proposed contract mixed local
    technique identity into cache semantics while leaving the existing contrast optimizer globally hard-coded.
  - Applying PaintedSurface as one late BodyBase mask would flatten the geometry shading that 4E is intended to create.
- Decision:
  - Require a human to select one opaque, non-remap index from the active palette. That exact index owns BodyBase; all
    derived painted-body roles remain in an anchor-coherent family and neither the Provider nor contrast optimization may
    move it.
  - Require an explicit Ground/Air/LargeSurface/Unknown adaptation. Compose it locally with the selected technique; do not
    infer it from model names or send it to the Provider as editable prose.
  - Keep Provider compilation cache identity separate from the local materialization bundle, so base/technique/adaptation
    changes invalidate candidates without causing another model call.
  - Bind PaintedSurface to BodyGeometryFamily, apply direct semantic materials afterwards, and apply approved remap last.
    Resolve Top+Under thin-cell conflicts through an adaptation-owned DualSurfacePolicy.
  - Keep typed C# policies as runtime authority. The existing Chat Skill remains advisory and does not become a second
    runtime rule source.
- Rejected alternatives:
  - Automatically choose the dominant model colour, accept arbitrary RGB outside the active palette, or let prose/model
    output override the human body anchor.
  - Give every PartRole its own base colour in v1, jump to another palette family to satisfy luminance, or use a single
    cache key for both paid compilation and local candidate identity.
  - Paint every PaintedSurface cell with one late BodyBase rule, or rely on incidental rule order for one-cell-thick wings.
- Consequences:
  - Body colour has one explicit human authority and reproducible palette identity; non-body materials remain semantically
    distinct and are evaluated relative to the same anchor.
  - Rev.2 requires policy-aware changes to the existing contrast optimizer and colourizer but still preserves one compiler,
    one normalizer path, one colourizer, current 4D sidecar and current export boundary.
  - Single-index DirectRole materials/remap can still look flat across several geometry regions; Rev.2 requires a visible
    warning and defers multi-level semantic/remap families to a later contract.
- Superseded follow-up:
  - Rev.3 is now the sole approval target; do not approve or implement Rev.2 directly.

## 2026-08-30 — DeepSeek proposes unit class before one class-specific colouring Skill is routed

- Status: Proposed / awaiting user approval
- Task: ASSET-VOX-4E FinalContract Rev.3
- Context:
  - Ground, air and large-surface units need materially different shading and readability techniques, while the model
    cannot safely become the final authority for a classification that changes the rule package.
  - The existing general colouring Skill combines several classes and is Chat-only; the dedicated style compiler is a
    separate prompt path and must not silently mix all class rules.
- Decision:
  - Build bounded geometry/semantic `UnitClassEvidence`, ask DeepSeek for an evidence-referencing `UnitClassProposal`, and
    require a human confirmation or correction before style compilation.
  - Route `ConfirmedUnitClass` deterministically in the Host to exactly one class-specific colouring Skill and one typed
    adaptation policy. The proposal itself has no routing authority; Unknown uses the conservative general Skill and is
    always NeedsReview.
  - Keep classification and style compilation as separately visible, cancellable and cached Provider stages. A double
    cache miss may make at most one call per stage; base colour and technique changes remain local and make no model call.
  - Keep Skill prose qualitative and typed C# policy/validator thresholds authoritative. DeepSeek proposes bounded roles
    and bindings; local normalization, masks, quality gates and colourizer write the actual palette indices.
- Rejected alternatives:
  - Let the model silently select its own rule package, infer class from filename alone, or skip human confirmation for a
    high-confidence proposal.
  - Load Ground, Air and LargeSurface Skills into one style prompt, or expose a separate adaptation selector that can
    contradict the confirmed class.
  - Hide classification and style compilation behind one opaque call count or reuse one cache identity for both stages.
- Consequences:
  - Misclassification is reviewable and correctable before any colouring plan is generated, and class rules cannot mix by
    prompt accident.
  - Worst-case provider cost/latency increases from one to two calls; both stages require independent status, cache,
    failure and cancellation evidence.
  - The classifier Skill and class-specific colouring Skills must be implemented and sample-tested in 4E-1; no runtime or
    XAML change is authorized while the Rev.3 contract remains Proposed.
- Follow-up:
  - Review and explicitly approve `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md` Rev.3 before any
    runtime or XAML implementation starts.
