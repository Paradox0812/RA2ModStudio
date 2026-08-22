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

- Status: Proposed / Awaiting implementation approval
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
  - 用户确认 `AUTOMATION-HLI-1A2_HeadlessDiagnosticsFinalContract.md` 后才实施；
    完成后停止，HLI-1B 另行契约。

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
