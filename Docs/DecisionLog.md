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

- Status: Proposed / final contract awaiting implementation approval
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
  - 用户确认 HLI-1C 最终契约后执行 1C-0..1C-4；完成后停止，不自动进入 HLI-2A。

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
