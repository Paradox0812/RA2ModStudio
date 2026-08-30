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

## 2. 当前继续入口

当前已提交 Git 基线仍为 `ASSET-VOX-4D`，分支 `codex/content-2d-baseline`；当前工作树已完成并验证
`ASSET-VOX-4E-1` internal contracts/Skill packages，但尚未提交。最新文档治理提交为
`ab92d56b9b57f89f3c417b0b0f9a0fbf1086e66d`，其父提交
`5a226ddf1f0dd04dd416bcbae549cc0a648e5d88` 固化 4D 代码。当前已批准 4E Rev.3 并完成 4E-1。
新任务优先读取：

1. `Docs/Codex_CurrentPhase.md`
2. `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
3. `Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`
4. `Docs/ASSET-VOX-4D_StageLedger.md`
5. `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationCodeFactAudit.md`
6. `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md`（Approved / 4E-1 completed）
7. `Docs/ASSET-VOX-4E_StageLedger.md`

旧累积状态已移入 `Docs/Archive/`。历史文件中的 “next phase” 不覆盖上述当前入口。

## 3. 面向不同读者

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
| 查看 RA2 Skill 来源、契约与证据 | `AGENT-KNOWLEDGE-1_Ra2LogicAndSkillSourceAudit.md` + `AGENT-KNOWLEDGE-1_Ra2BuiltInSkillsContinuousFinalContract.md` + `AGENT-KNOWLEDGE-1_StageLedger.md` + `AGENT-KNOWLEDGE-1-R2_RulesArtBindingSourceAudit.md` |
| 查看 Work 模型选 Skill Manifest 契约与结果 | `AGENT-SKILL-ROUTING-2_ModelSelectedSkillManifestContinuousFinalContract.md` + `AGENT-SKILL-ROUTING-2_StageLedger.md`（Implemented / automated verified） |
| 查看 Work 共享上下文与受限本地查询 | `AGENT-CONTEXT-3_SharedConversationAndBoundedProjectQueryFinalContract.md` + `AGENT-CONTEXT-3_StageLedger.md`（Completed / automated verified） |
| 查看 Work 一次性结构化重规划 | `AGENT-REPAIR-1_BoundedStructuredReplanCodeFactAudit.md` + `AGENT-REPAIR-1_BoundedStructuredReplanFinalContract.md` + `AGENT-REPAIR-1_StageLedger.md`（Completed / automated verified） |
| 查看 Work 入口最低安全重构与证据纠正 | `AGENT-WORK-ENTRY-1_MinimumSafetyWorkAdmissionFinalContract.md` + `AGENT-WORK-ENTRY-1_StageLedger.md`（Implemented / automated verified；real provider manual pending） |
| 查看 Windows `.ini` 文件关联启动 | `SHELL-LAUNCH-1_FileAssociationLaunchFinalContract.md` + `SHELL-LAUNCH-1_StageLedger.md`（Completed / automated verified；physical smoke pending） |
| 查看 Projectile / Warhead complete profiles | `AUTOMATION-CONTENT-2B_ProjectileWarheadProfilesCodeFactAudit.md` + `AUTOMATION-CONTENT-2B_ProjectileWarheadProfilesFinalContract.md` + `AUTOMATION-CONTENT-2B_StageLedger.md` |
| 查看已完成 SuperWeapon / 支援技能 Profiles | `AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerCodeFactAudit.md` + `AUTOMATION-CONTENT-2E_SourceCapabilityMatrix.md` + `AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerContinuousFinalContract.md` + `AUTOMATION-CONTENT-2E_StageLedger.md`（Completed / automated verified） |
| 查看 AI Programming Tuple Profiles 当前差距 | `AUTOMATION-CONTENT-2C_AiProgrammingTupleProfilesCodeFactAudit.md`（仅审计；契约与实现延期） |
| 查看对象闭包与当前文档注册基础 | `AUTOMATION-CONTENT-2D01_ObjectClosureRegistrationFinalContract.md` + `AUTOMATION-CONTENT-2D01_StageLedger.md` |
| 项目级多文档事务实现与证据 | `AUTOMATION-CONTENT-2D2_ProjectMultiDocumentTransactionCodeFactAudit.md` + `AUTOMATION-CONTENT-2D2_ProjectMultiDocumentTransactionFinalContract.md` + `AUTOMATION-CONTENT-2D2_StageLedger.md` |
| 首个 rules/art project template 与 Asset Manifest | `AUTOMATION-CONTENT-2D3_ASSET-MANIFEST-1_ContinuousFinalContract.md` + `AUTOMATION-CONTENT-2D3_ASSET-MANIFEST-1_StageLedger.md` |
| Source-backed Art schema 与首个 Existing-Asset Provider | `AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_ContinuousFinalContract.md` + `AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_StageLedger.md` |
| Work 项目级 rules/art Proposal 与 Project Diff 接线 | `AUTOMATION-CONTENT-PROJECT-UI-1_WorkProjectProposalEndToEndFinalContract.md` + `AUTOMATION-CONTENT-PROJECT-UI-1_StageLedger.md`（Completed / verified） |
| 查看完整候选 Result/Changes/Object Context 审阅 | `DIFF-REVIEW-1_CanonicalResultAndObjectContextFinalContract.md` + `DIFF-REVIEW-1_StageLedger.md`（Completed / automated verified；manual visual pending） |
| 查看 Agent VOX 素材流水线侦察与架构提案 | `ASSET-VOX-1_SystemInvestigationAndArchitectureProposal.md`（Research completed） |
| 查看 VOX 分离式装配与 VXLSE 切片基线 | `ASSET-VOX-1A_GoldenProbeAndSeparatedAssemblyFinalContract.md`、`ASSET-VOX-1A_StageLedger.md`（implementation completed；executable structural acceptance closed by 1B） |
| 查看规范体素快照、VOX/VXL/PNG/SliceStack 纯核心 | `ASSET-VOX-1B_CanonicalVoxelCoreFinalContract.md`、`ASSET-VOX-1B_StageLedger.md`（implementation + supplied VXLSE structural acceptance completed；visual/game acceptance deferred） |
| 查看 Generation Provider Host 审计、契约与实现证据 | `ASSET-VOX-1C_GenerationProviderHostCodeFactAudit.md`、`ASSET-VOX-1C_GenerationProviderHostFinalContract.md`、`ASSET-VOX-1C_StageLedger.md`（completed / automated verified） |
| 查看真实 Hunyuan Provider 环境审计、最终契约与授权门 | `ASSET-VOX-1C-P1_RealProviderEnvironmentCodeFactAudit.md`、`ASSET-VOX-1C-P1_HunyuanMiniProviderFinalContract.md`、`ASSET-VOX-1C-P1_StageLedger.md`（P1-0 completed；P1-1..P1-5 blocked on explicit license/install authorization） |
| 查看当前体素语义持久化基线 | `ASSET-VOX-4D_PersistentSemanticMaskCodeFactAudit.md`、`ASSET-VOX-4D_PersistentSemanticMaskFinalContract.md`、`ASSET-VOX-4D_StageLedger.md`（completed / automated verified；Save/Import user-reported passed；wrong-model/dirty/DPI pending） |
| 查看 mask 驱动上色模块侦察与原始代码事实 | `ASSET-VOX-4E_MaskDrivenColourMaterializationCodeFactAudit.md`（audit completed） |
| 审阅 mask 驱动上色技法、规则与质量门契约 | `ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md`（Approved；4E-1 completed） |
| 查看 4E 分阶段实现与验证证据 | `ASSET-VOX-4E_StageLedger.md`（4E-1 completed；4E-2 next） |
| 查看地面/空中单位上色样本、联网来源与内置 Skill 证据 | `ASSET-VOX-4E_GroundAirColourTechniqueSourceStudy.md`（completed；4E-1 specialist Skills completed；compiler 接入待 4E-2） |
| 查看本地已验证 Git 基线 | `GIT-BASELINE-1_StageLedger.md` |
| 查找旧阶段证据 | 对应 `*Contract.md`、`*StageLedger.md`、Context Capsule 或 `Archive/` |

## 4. 状态词

- **Completed / Verified**：实现存在且有阶段台账、测试或用户验收证据。
- **Implemented / Visual Acceptance Pending**：代码和自动化门禁完成，真实视觉验收尚未完成。
- **Partial**：只完成受限子集，不能宣称具备完整产品能力。
- **Proposed**：只有契约或设计，未授权或未开始实现。
- **Not Implemented**：代码事实审计确认不存在。
- **Unknown / Pending Verification**：证据不足，不能猜测。

## 5. 历史文档边界

`Docs/Archive/` 保存被替代的累积状态快照。阶段契约和 Stage Ledger 继续作为
历史证据保留，但它们不自动成为当前需求。不要修改历史台账来配合新计划；
应在当前状态、路线图或 Decision Log 中记录替代关系。
### ASSET-VOX-1E-UI

- `ASSET-VOX-1E-UI_CodeFactAudit.md`
- `ASSET-VOX-1E-UI_FinalContract.md`
- `ASSET-VOX-1E-UI_StageLedger.md`
- `ASSET-VOX-1E-UI-R2_UnifiedVoxelInputAndOptionalRemapFinalContract.md`
- `ASSET-VOX-1E-UI-3D_InteractiveViewportFinalContract.md`
- `ASSET-VOX-1E-UI-3D_StageLedger.md`
- `ASSET-VOX-1F-CORE-1_HighValueVoxelCoreMigrationFinalContract.md`
- `ASSET-VOX-1F-CORE-1_StageLedger.md`
