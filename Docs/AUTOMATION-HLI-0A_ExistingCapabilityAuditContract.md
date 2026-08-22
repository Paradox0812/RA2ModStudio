# AUTOMATION-HLI-0A Existing Capability Audit Contract

状态：Completed（DocsOnly capability audit）  
日期：2026-08-20  
所属路线：AGENT-AUTHORING-1-R1 / High-Level Interface  
风险：本阶段 R0；后续程序集边界与 public contract 为 R3，外部进程桥为 R4

## 1. 目标

在设计新的 Automation / Agent 高层接口前，先以当前仓库真实实现为依据完成能力盘点，明确：

1. 哪些能力已经存在并可直接复用；
2. 哪些能力算法上可以无头运行，但因位于 WPF 程序集而不能被独立宿主引用；
3. 哪些能力混入 ViewModel、磁盘、Shell、AvalonEdit 或运行时可变状态；
4. 哪些能力尚不存在，不能通过重命名现有类型来假定已经具备；
5. HLI-0B 最小能力契约应冻结哪些边界，哪些能力必须继续由 IDE 主机独占。

本阶段是审计和决策输入，不创建 Automation Core，不定义可见协议，也不实现 CLI、MCP、Job Runtime 或外部 Agent 桥。

## 2. 权威输入

- `AGENTS.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `Docs/AGENT-AUTHORING-1_HighLevelIniAuthoringArchitectureContract.md`
- `C:/Users/PC/Desktop/RA2IniEditor_Automation_Architecture_v1.md`
- 当前 `RA2IniEditor.Core`、`RA2IniEditor.Infrastructure`、`RA2IniEditor.IDE` 和相关测试源码

外部架构文档用于提供目标方向；仓库源码和已接受的 A1-A4 契约决定当前事实。文档建议与当前实现冲突时，以当前代码和已接受契约为准，并把差异记录为迁移项，而不是修改实现来迎合文档。

## 3. 获批写入范围

只允许修改以下五个文件：

1. `Docs/AUTOMATION-HLI-0A_ExistingCapabilityAuditContract.md`
2. `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`
3. `Docs/AUTOMATION-HLI-0A_StageLedger.md`
4. `Docs/Codex_CurrentPhase.md`
5. `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`

禁止修改任何 `.cs`、`.xaml`、`.csproj`、`.sln`、测试、脚本、项目配置、字段数据或发布资产。禁止新增依赖、目录、public API、AutomationId 或序列化协议。

## 4. 语义与生命周期边界

本审计不得改变：

- INI parser、TextModel、语义模型、诊断和 Completion 语义；
- Field Registry 的 Project > Global > BuiltIn 优先级、运行时快照、Hover、Quick Peek 和 BuiltIn 数据；
- Search / Replace 的当前文件与项目边界；
- A2 Preview、A3 单次事务、Undo/Redo 和显式确认规则；
- A4 provider tool-call、proposal 生命周期和风险策略；
- Save Preflight、Backup、Writer、Rollback 和用户保存所有权；
- Shell、Dock、XAML、AvalonEdit、项目文件和 legacy 状态。

## 5. 审计方法

每项能力必须同时回答以下问题：

| 维度 | 判定要求 |
|---|---|
| Current implementation | 给出真实文件、程序集和主要类型；不存在则明确写 `NotPresent` |
| Source of truth | 指明文本、语义模型、Field Registry snapshot、编辑 Session 或磁盘中的哪一项拥有事实 |
| Coupling | 分开记录 WPF、AvalonEdit、Shell/ViewModel、磁盘和运行时可变状态 |
| Algorithmically headless | 算法本身能否在不创建 UI 控件的情况下执行 |
| Headless-host consumable | 非 Windows/WPF 宿主现在能否通过项目引用直接消费 |
| Tests | 记录现有测试证据；不把测试覆盖推断为公共契约稳定性 |
| Reuse decision | 只能使用 `ReuseAsIs`、`ExtractContract`、`MoveImplementation`、`AdapterOnly`、`NotPresent` 或 `Deferred` |
| Next use | 明确 HLI-0B/HLI-1 的入口，或说明为什么暂不暴露 |

特别约束：`Algorithmically headless = Yes` 不等于 `Headless-host consumable = Yes`。位于 `RA2IniEditor.IDE` 的 internal 服务仍由 `net8.0-windows` WPF 程序集承载，不能直接作为未来 CLI/Job/外部 Agent 的中立接口。

## 6. 必审能力面

- Project discovery / document read
- Section / symbol / definition / reference query
- Field schema and provenance snapshot
- Current-document and project diagnostics
- Text search and semantic reference distinction
- Semantic edit snapshot / plan / preview
- Host-owned Apply transaction
- Save / backup / rollback
- Built-in Agent proposal adapter
- Template service
- Capability registry / gateway
- Job runtime / event transport / artifact identity

## 7. HLI-0B 前的硬门禁

HLI-0A 不冻结代码签名。HLI-0B 必须单独确认：

1. 中立 application 层的程序集位置和依赖方向；
2. capability ID、请求、结果、failure kind、版本状态和取消语义；
3. Project/Document/Registry snapshot 的所有权与生命周期；
4. 项目级引用查询是正式能力还是后置缺口；
5. Agent 只获得查询与 Preview，不获得 Apply、Save 或任意文件写入；
6. 进程内接口与未来 wire DTO 分离，不把 internal IDE DTO 直接序列化；
7. 对 A1-A4 现有路径采用适配/迁移，而不是复制算法。

## 8. 验收标准

- 能力矩阵覆盖第 6 节全部能力面；
- 每个“已存在”结论都能落到真实源码；
- 明确区分文本搜索与语义引用查询、Preview 与 Apply、内存编辑与磁盘保存；
- 明确记录 Capability/Job/Event/Template 基础设施的真实缺口；
- 给出至少三种程序集落点方案及推荐候选，但不把候选写成已批准架构；
- 不新增或修改 public API；
- 精确五文档范围通过静态检查；
- build/test/package 记为 `NotRun（DocsOnly）`，不得伪装为 Passed。

## 9. 停止条件

本阶段在矩阵、阶段账本和当前状态文档完成静态自审后停止。不得自动进入 HLI-0B 或实现任何能力接口；HLI-0B 仍需单独契约和用户确认。
