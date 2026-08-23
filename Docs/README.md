# RA2IniEditor.IDE 文档入口

本页是项目文档的唯一入口。若历史契约、阶段台账与当前状态冲突，按下列
权威顺序判断，不得从旧文档推断当前能力。

## 1. 权威阅读顺序

1. `AGENTS.md`：稳定授权边界、IDE-only 规则和验证命令。
2. `Docs/ProductVisionAndRequirements.md`：用户确认的最终产品目标与需求边界。
3. `Docs/CurrentCapabilities.md`：已经完成、部分完成和尚未实现的能力矩阵。
4. `Docs/Codex_CurrentPhase.md`：当前阶段、最近可信证据和下一安全入口。
5. `Docs/DevelopmentRoadmap.md`：从当前实现到最终目标的阶段路线。
6. 当前任务明确点名的契约、Stage Ledger 或 Context Capsule。

## 2. 面向不同读者

| 目的 | 首选文档 |
|---|---|
| 了解产品最终要做什么 | `ProductVisionAndRequirements.md` |
| 判断现在能否完成某项操作 | `CurrentCapabilities.md` |
| 继续下一阶段开发 | `Codex_CurrentPhase.md` + 当前契约 |
| 恢复精简工程上下文 | `RA2IniEditor_IDE_Full_Codex_Context.md` |
| 使用当前 IDE | `UserGuide.md` |
| 查看当前用户功能 | `FeatureOverview.md` |
| 开发和验证 | `DeveloperNotes.md` |
| 查找架构决策 | `DecisionLog.md` |
| 查找 Public API 候选与兼容状态 | `PublicApiLedger.md` |
| 查看已完成 Headless Query 迁移 | `AUTOMATION-HLI-1A1_DocumentQuerySliceFinalContract.md` + `AUTOMATION-HLI-1A1_StageLedger.md` |
| 审查 Headless Diagnostics 事实 | `AUTOMATION-HLI-1A2_DiagnosticsCodeFactAudit.md` |
| 查看已完成 Headless Diagnostics | `AUTOMATION-HLI-1A2_HeadlessDiagnosticsFinalContract.md` + `AUTOMATION-HLI-1A2_StageLedger.md` |
| 查看已完成 Headless Edit Preview | `AUTOMATION-HLI-1B_EditPreviewCodeFactAudit.md` + `AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md` + `AUTOMATION-HLI-1B_StageLedger.md` |
| 查看已完成 Host Apply 边界 | `AUTOMATION-HLI-1C_HostBoundaryCodeFactAudit.md` + `AUTOMATION-HLI-1C_HostBoundaryFinalContract.md` + `AUTOMATION-HLI-1C_StageLedger.md` |
| 查看已完成最小 Capability Gateway | `AUTOMATION-HLI-2A_CapabilityGatewayCodeFactAudit.md` + `AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md` + `AUTOMATION-HLI-2A_StageLedger.md` |
| 查看已完成 HLI-2B IDE/AI Gateway consumer | `AUTOMATION-HLI-2B_GatewayConsumerCodeFactAudit.md` + `AUTOMATION-HLI-2B_GatewayConsumerFinalContract.md` + `AUTOMATION-HLI-2B_StageLedger.md` |
| 查看已完成 HLI-2C 首个高层 Agent 闭环 | `AUTOMATION-HLI-2C_FirstAgentLoopCodeFactAudit.md` + `AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md` + `AUTOMATION-HLI-2C_StageLedger.md` |
| 查看语义层、独立 Host 与素材的优先级裁决 | `AUTOMATION-POST-HLI-0_SemanticHostPriorityCodeFactAudit.md` |
| 查看已完成 CONTENT-1 语义模板层 | `AUTOMATION-CONTENT-1_SemanticTemplateContinuousFinalContract.md` + `AUTOMATION-CONTENT-1_StageLedger.md` |
| 查看 Chat/Work 模式与完整武器链 | `AGENT-MODE-1_ChatWorkModeFinalContract.md` + `AGENT-MODE-1A_DirectFireCompleteProfileSourceAudit.md` |
| 查看 RA2 Skill 来源、契约与证据 | `AGENT-KNOWLEDGE-1_Ra2LogicAndSkillSourceAudit.md` + `AGENT-KNOWLEDGE-1_Ra2BuiltInSkillsContinuousFinalContract.md` + `AGENT-KNOWLEDGE-1_StageLedger.md` |
| 查看 Projectile / Warhead complete profiles | `AUTOMATION-CONTENT-2B_ProjectileWarheadProfilesCodeFactAudit.md` + `AUTOMATION-CONTENT-2B_ProjectileWarheadProfilesFinalContract.md` + `AUTOMATION-CONTENT-2B_StageLedger.md` |
| 查看 AI Programming Tuple Profiles 当前差距 | `AUTOMATION-CONTENT-2C_AiProgrammingTupleProfilesCodeFactAudit.md`（仅审计；契约与实现延期） |
| 查看对象闭包与当前文档注册基础 | `AUTOMATION-CONTENT-2D01_ObjectClosureRegistrationFinalContract.md` + `AUTOMATION-CONTENT-2D01_StageLedger.md` |
| 查看本地已验证 Git 基线 | `GIT-BASELINE-1_StageLedger.md` |
| 查找旧阶段证据 | 对应 `*Contract.md`、`*StageLedger.md`、Context Capsule 或 `Archive/` |

## 3. 状态词

- **Completed / Verified**：实现存在且有阶段台账、测试或用户验收证据。
- **Implemented / Visual Acceptance Pending**：代码和自动化门禁完成，真实视觉验收尚未完成。
- **Partial**：只完成受限子集，不能宣称具备完整产品能力。
- **Proposed**：只有契约或设计，未授权或未开始实现。
- **Not Implemented**：代码事实审计确认不存在。
- **Unknown / Pending Verification**：证据不足，不能猜测。

## 4. 历史文档边界

`Docs/Archive/` 保存被替代的累积状态快照。阶段契约和 Stage Ledger 继续作为
历史证据保留，但它们不自动成为当前需求。不要修改历史台账来配合新计划；
应在当前状态、路线图或 Decision Log 中记录替代关系。
