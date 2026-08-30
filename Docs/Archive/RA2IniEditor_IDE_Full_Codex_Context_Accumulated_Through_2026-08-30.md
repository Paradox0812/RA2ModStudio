# RA2IniEditor.IDE — Compact Codex Context（累积快照，截至 2026-08-30）

> Superseded：本文件保留 2026-08-30 文档清理前的累积上下文。当前精简上下文请读取
> `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`，历史阶段细节继续以各自 Contract/Stage Ledger 为准。

更新时间：2026-08-28
用途：为新任务恢复足够但不重复历史的工程上下文。历史阶段细节应读取对应
Contract/Stage Ledger，不再追加到本文件。

## 1. 产品身份

RA2IniEditor.IDE 是面向 RA2 / YR / Ares / Phobos 的 source-first INI IDE。
当前技术栈为 .NET 8、WPF、AvalonEdit 和 AvalonDock。IDE-only solution 是唯一
构建入口；旧表格编辑器和 legacy root solution 不属于产品。

最终目标是自然语言驱动的 Mod 内容生产 Agent：统一编排 INI、Cameo/Icon、
VOX/VXL 和 SHP 产物。当前项目已完成真实 INI IDE、受限当前文件编辑和可审阅项目级 AI 编辑闭环，
素材自动生成和独立 Agent 平台尚未实现。

最新体素样式输入基线为 `ASSET-VOX-1E-UI-R2`：现有中央审阅文档可读取项目内单模型 VOX，或读取
单 Section VXL 并要求用户显式选择对应的 768-byte Westwood PAL；两者都复用 Stage 1B 解码器并汇入
同一 `Ra2VoxelSceneSnapshot`。普通材质上色不依赖阵营色/remap。没有 remap 色段时，仅允许将不可执行、
纯文本推断的 remap 意图保留为未决说明；显式/可执行 remap 继续安全失败。多 Section 选择、语义蒙版、
VXL/HVA 写出、项目 Apply/Save、真实 DeepSeek 与游戏内验证仍未完成。

最新结构编辑基线为 `ASSET-VOX-2C`：显式 AI 结构识别输出绑定证据哈希的稀疏几何操作。主分析和独立
审阅仅在可执行 `(target_id, action)` 集合不同时追加第三轮仲裁；主分析还可请求一次有界、无坐标细节。
Host 不再替 Agent 决定主体/附加件或修改操作方向，只负责确定性目标展开与最低几何安全线。结果仍为
会话内 3D 差异候选，不写文件、不 Apply/Save，也不生成 VXL/HVA。

2C 自动验证基线：focused Application 11/11、affected IDE 32/32、Application full 285/285、IDE full
2830/2830、AssetHost 47/47；Debug build 0 error / 1 个既有 nullable warning；IdeOnly clean package 1379
files。真实 DeepSeek/Tencent 和人工视觉验收未运行。

## 2. 权威文档

| 主题 | 文档 |
|---|---|
| 稳定工程规则 | `AGENTS.md` |
| 文档入口与权威顺序 | `Docs/README.md` |
| 最终需求 | `Docs/ProductVisionAndRequirements.md` |
| 当前实现能力 | `Docs/CurrentCapabilities.md` |
| 当前阶段 | `Docs/Codex_CurrentPhase.md` |
| 路线图 | `Docs/DevelopmentRoadmap.md` |
| 架构决策 | `Docs/DecisionLog.md` |
| 高层接口代码事实 | `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md` |
| Headless 最小能力契约 | `Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md` |
| 最新依赖锥证据 | `Docs/AUTOMATION-HLI-1A0_DependencyConeCharacterizationContract.md` |
| 最新 Headless 实现证据 | `Docs/AUTOMATION-HLI-1A1_StageLedger.md` |
| HLI-1A2 事实证据 | `Docs/AUTOMATION-HLI-1A2_DiagnosticsCodeFactAudit.md` |
| HLI-1A2 最终契约 | `Docs/AUTOMATION-HLI-1A2_HeadlessDiagnosticsFinalContract.md` |
| HLI-1B 事实证据 | `Docs/AUTOMATION-HLI-1B_EditPreviewCodeFactAudit.md` |
| HLI-1B 最终契约 | `Docs/AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md` |
| HLI-1B 完成证据 | `Docs/AUTOMATION-HLI-1B_StageLedger.md` |
| HLI-1C 事实证据 | `Docs/AUTOMATION-HLI-1C_HostBoundaryCodeFactAudit.md` |
| HLI-1C 最终契约 | `Docs/AUTOMATION-HLI-1C_HostBoundaryFinalContract.md` |
| HLI-1C 完成证据 | `Docs/AUTOMATION-HLI-1C_StageLedger.md` |
| HLI-2A 事实证据 | `Docs/AUTOMATION-HLI-2A_CapabilityGatewayCodeFactAudit.md` |
| HLI-2A 最终契约 | `Docs/AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md` |
| HLI-2A 完成证据 | `Docs/AUTOMATION-HLI-2A_StageLedger.md` |
| HLI-2B 事实证据 | `Docs/AUTOMATION-HLI-2B_GatewayConsumerCodeFactAudit.md` |
| HLI-2B 最终契约 | `Docs/AUTOMATION-HLI-2B_GatewayConsumerFinalContract.md` |
| HLI-2B 完成证据 | `Docs/AUTOMATION-HLI-2B_StageLedger.md` |
| HLI-2C 事实证据 | `Docs/AUTOMATION-HLI-2C_FirstAgentLoopCodeFactAudit.md` |
| HLI-2C 最终契约 | `Docs/AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md` |
| HLI-2C 完成证据 | `Docs/AUTOMATION-HLI-2C_StageLedger.md` |
| POST-HLI 优先级审计 | `Docs/AUTOMATION-POST-HLI-0_SemanticHostPriorityCodeFactAudit.md` |
| CONTENT-1 连续契约 | `Docs/AUTOMATION-CONTENT-1_SemanticTemplateContinuousFinalContract.md` |
| CONTENT-1 完成证据 | `Docs/AUTOMATION-CONTENT-1_StageLedger.md` |
| Chat / Work 最终契约 | `Docs/AGENT-MODE-1_ChatWorkModeFinalContract.md` |
| Work 两阶段意图/执行契约 | `Docs/AGENT-MODE-2_TwoStageIntentExecutionFinalContract.md` |
| RA2 Skill 来源审计 | `Docs/AGENT-KNOWLEDGE-1_Ra2LogicAndSkillSourceAudit.md` |
| RA2 Skill 连续契约 | `Docs/AGENT-KNOWLEDGE-1_Ra2BuiltInSkillsContinuousFinalContract.md` |
| 最新 Mode / Skill 证据 | `Docs/AGENT-KNOWLEDGE-1_StageLedger.md` |
| CONTENT-2B 契约与证据 | `Docs/AUTOMATION-CONTENT-2B_ProjectileWarheadProfilesFinalContract.md` + `Docs/AUTOMATION-CONTENT-2B_StageLedger.md` |
| CONTENT-2C 代码事实审计 | `Docs/AUTOMATION-CONTENT-2C_AiProgrammingTupleProfilesCodeFactAudit.md`；契约与实现按用户要求延期 |
| CONTENT-2D-0/1 注册基础 | `Docs/AUTOMATION-CONTENT-2D01_ObjectClosureRegistrationFinalContract.md` + `Docs/AUTOMATION-CONTENT-2D01_StageLedger.md` |
| CONTENT-2D-2 多文档事务 | `Docs/AUTOMATION-CONTENT-2D2_ProjectMultiDocumentTransactionCodeFactAudit.md` + `Docs/AUTOMATION-CONTENT-2D2_ProjectMultiDocumentTransactionFinalContract.md` + `Docs/AUTOMATION-CONTENT-2D2_StageLedger.md`；2D-2A..2F completed |
| CONTENT-2D-3 / Asset Manifest | `Docs/AUTOMATION-CONTENT-2D3_ASSET-MANIFEST-1_ContinuousFinalContract.md` + `Docs/AUTOMATION-CONTENT-2D3_ASSET-MANIFEST-1_StageLedger.md` |
| Art schema / Existing Asset Provider | `Docs/AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_ContinuousFinalContract.md` + `Docs/AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_StageLedger.md` |
| Work project proposal UI | `Docs/AUTOMATION-CONTENT-PROJECT-UI-1_WorkProjectProposalEndToEndFinalContract.md` + `Docs/AUTOMATION-CONTENT-PROJECT-UI-1_StageLedger.md`（Completed / verified） |
| 完整候选审阅 UI | `Docs/DIFF-REVIEW-1_CanonicalResultAndObjectContextFinalContract.md` + `Docs/DIFF-REVIEW-1_StageLedger.md`（Completed / automated verified；manual visual pending） |
| VOX Generation Provider Host | `Docs/ASSET-VOX-1C_GenerationProviderHostCodeFactAudit.md` + `Docs/ASSET-VOX-1C_GenerationProviderHostFinalContract.md` + `Docs/ASSET-VOX-1C_StageLedger.md`（Completed / automated verified） |
| Agent-led voxel geometry | `Docs/ASSET-VOX-2C_AgentLedGeometryProposalCodeFactAudit.md` + `Docs/ASSET-VOX-2C_AgentLedGeometryProposalFinalContract.md` + `Docs/ASSET-VOX-2C_StageLedger.md` |
| Rules/art binding Skill source audit | `Docs/AGENT-KNOWLEDGE-1-R2_RulesArtBindingSourceAudit.md`（Implemented；real provider acceptance pending） |
| Work 模型选 Skill Manifest | `Docs/AGENT-SKILL-ROUTING-2_ModelSelectedSkillManifestContinuousFinalContract.md` + `Docs/AGENT-SKILL-ROUTING-2_StageLedger.md`（Implemented / automated verified） |
| Work 一次性结构化重规划 | `Docs/AGENT-REPAIR-1_BoundedStructuredReplanCodeFactAudit.md` + `Docs/AGENT-REPAIR-1_BoundedStructuredReplanFinalContract.md` + `Docs/AGENT-REPAIR-1_StageLedger.md`（Completed / automated verified） |
| SuperWeapon / 支援技能 profiles | `Docs/AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerCodeFactAudit.md` + `Docs/AUTOMATION-CONTENT-2E_SourceCapabilityMatrix.md` + `Docs/AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerContinuousFinalContract.md` + `Docs/AUTOMATION-CONTENT-2E_StageLedger.md`（Completed / automated verified） |
| 本地 Git 已验证基线 | `Docs/GIT-BASELINE-1_StageLedger.md`；分支 `codex/content-2d-baseline`，标签 `content-2d01-verified` |
| 下一安全入口 | CONTENT-2E 真实 DeepSeek/WPF/游戏内验收；随后审计下一批 SuperWeapon profile 或自动化游戏测试 Host；CONTENT-2C AI 写入继续冻结 |
| Public API 候选与状态 | `Docs/PublicApiLedger.md` |

## 3. Solution 与所有权

```text
RA2IniEditor.Core              net8.0，INI model/parser/schema/validation primitives
RA2IniEditor.Infrastructure    net8.0，Field Registry、BuiltIn 数据、IO helpers
RA2IniEditor.Application       net8.0，Core-only Query/Diagnostics/Edit Preview 与 Experimental API
RA2IniEditor.IDE               net8.0-windows，WPF Shell、editing/AI/search 和 Application consumer
RA2IniEditor.Application.Tests net8.0 headless contract tests
RA2IniEditor.Tests             IDE/non-UI integration tests
RA2IniEditor.UiAutomationTests opt-in UIA smoke
```

`RA2IniEditor.Application` 已通过 HLI-1A1/1A2/1B 迁入 Query、Diagnostics 与 Preview
唯一闭包。HLI-2A 已增加固定四能力目录和 typed Gateway；HLI-2B 已让 IDE 内置 AI 的唯一
Host adapter 消费同一 Gateway，并在 provider 前执行 descriptor-driven 资源门禁。

## 4. 当前已完成能力

- 源码编辑、项目浏览、导航、Dirty、Undo/Redo、Save Preflight 和 backup/rollback。
- Completion、轻量 Hover、Quick Peek、Find References 和 current/project diagnostics。
- Field Registry Project > Global > BuiltIn、Manager、学习/导入预览和 FR-DQ-4 数据清理。
- AvalonDock 工作区、浮动 Search、返回 Home、默认布局重置和 v2 持久化。
- 项目文本 Search；当前文件 Preview-first Replace All，不自动保存。
- DeepSeek V4 Flash/Pro、Flash 默认、生产 Mock 移除、流式增量、取消/超时/
  Failure Taxonomy、上下文/隐私/资源边界。
- A1 UI-neutral 只读分析模型、A2 deterministic Preview、A3 host transaction、
  A4-R1 official endpoint structured-edit proposal 和显式 Apply。
- HLI-1A1 Core-only Application：Section Get、current-document References Find、
  15-type Experimental API、typed failure/limits/cancellation。
- HLI-1A2/1B：同一 Core-only Application 提供 Diagnostics 与受限 semantic Edit
  Preview；public allowlist 精确为 29，Preview 不修改 Host 或磁盘。
- HLI-2A：同一 Application 提供固定四能力 immutable catalog 与 typed Gateway；public
  allowlist 精确为 35，执行只委托现有 Query/Preview service。
- HLI-2B：内置 AI 通过唯一 Host adapter 消费 typed Gateway；8 MiB/10k/128 预算统一，超限
  明确编辑在 provider 前本地拒绝，public API 保持 35。
- HLI-2C：Gateway Query/Validate、provider structured plan、Preview、explicit Apply、Problems
  refresh 和 updated-snapshot Validate 已形成确定性闭环；public API 保持 35，不自动 Save。
- CONTENT-1：Field Schema、Reference Resolve、CreateSection、internal template compiler、首个
  source-backed Weapon/Projectile/Warhead 关系骨架、AI template tool 和主工作区 Diff 已完成；
  public allowlist 58、catalog 7、Gateway methods 9，仍不自动 Save。
- CONTENT-UI-1 VISUAL-FIX1：Diff 宿主改为从 layout session 获取恢复后的当前
  `Document.Source`，修复提案自动打开及“查看更改”在持久化布局恢复后无响应；默认 Dock 拓扑、
  比例与 layout schema 不变。定向 13/13、完整 IDE non-UI 2576/2576、clean package 1147 files；
  实际 WPF 视觉复验仍待用户执行。
- AI-AUTHORING-NONSTRICT-1：字段工具只对可唯一解释的非严格 JSON 形态做有限规范化；未知属性、
  复合 value 和任何 Apply/Save 权限仍 fail closed。聚焦 88/88、non-UI 2576/2576。
- AGENT-MODE-1：AI 面板显式区分 Chat / Work 并默认 Chat；普通“搭建可用武器链”进入
  direct-fire complete profile，只有明确骨架意图进入 skeleton。完整 profile 绑定唯一既有 owner，
  创建非空 Weapon/Projectile/Warhead，形成 15 项原子 Preview 操作。
- AGENT-KNOWLEDGE-1：基于联网来源审计与本地代码事实，内置 15 个 RA2/YR/Ares/Phobos
  领域 Skill；按需注入、内容有界、禁止 scripts/external roots，不增加 capability 或写入权限。
- AGENT-MODE-1-R1：Work 模式本身即当前文档 authoring scope，不再要求用户重复写“当前文件”；
  补齐“构筑/建立/生成/组装/装配/加装”等构建动词。用户报告的 HTNK 同轴机枪原句已通过路由回归。
- AGENT-MODE-1-R2：complete-profile provider schema 已改为命名对象和原生 scalar；template adapter
  可安全归一化省略 outcome、字符串版本、number/boolean 与尾逗号。Release focused 70/70、
  IDE non-UI 2585/2585；真实 DeepSeek 复验待用户重启新构建后执行。
- AGENT-MODE-1-R3：两次最小真实 DeepSeek 探针确认完整 proposal 会附带非空 `message`；adapter 现与
  已声明 schema 对齐，只验证后丢弃该旁路字符串。Release focused 167/167、IDE non-UI 2587/2587；
  未改变模板、字段库、Preview、Apply、Undo 或 Save 权限。
- AGENT-MODE-1-R4：clarification 混入 proposal 参数时改为安全显示 message 并保持参数惰性；完整对象
  缺省调参改用保守草案。最新真实探针得到 proposal + 15 参数；focused 71/71、IDE non-UI 2588/2588。
- AGENT-MODE-2 建立的意图/执行分层仍保留；AGENT-QUERY-2 在二者之间允许最多两次紧凑语义补查。
  Chat 仍单调用；Work 正常为 2..4 次，结果不落盘、不进入对话历史，执行仍受 canonical Preview/显式
  Apply 约束。
- CONTENT-2A/2B：现有 Techno 可生成双 direct-fire 链；现有 Weapon 可绑定 Arcing/Homing Projectile
  或 YR core Warhead。弹道族互斥，Ares custom armor/Phobos trajectory 继续 fail closed。
- AGENT-MODE-2 真实 DeepSeek Work 双调用已经用户验收通过。
- CONTENT-2D-0/1：Application internal Template Definition 已支持显式注册声明；编译器从当前
  Snapshot 验证数字注册列表并用 `max + 1` 稳定追加，幂等/重复/畸形/溢出均有确定性处置。
  现有生产 Profile 尚未启用注册，public API 与用户可见行为不变。
- GIT-BASELINE-1：上述已验证实现已形成独立本地分支和注释标签；版本控制卫生与凭据门禁通过。
  当前仍无 Git remote，未向任何外部仓库推送。
- CONTENT-2D-2：纯 Application Project Preview、唯一 IDE project session store、原子内存 Apply/rollback、
  compound Undo/Redo、多文件 Diff 与提交后内存态项目诊断刷新已完成；成功结果提供
  affected/work/dirty counts；allowlist 63、catalog 8、Gateway methods 10。
- CONTENT-2D-3 / ASSET-MANIFEST-1：首个 production headless project template 精确配对 rules/art，
  生成 `rules Image`、`art Image` 两个叶计划和 body SHP/Cameo Manifest；body 为 Proposed，Cameo
  因 schema 缺口为 PendingSchema。allowlist 69、catalog 9、methods 11；Application 176/176、
  IDE 2626/2626、IdeOnly package 1200 files；不写资产或磁盘。
- FIELD-REGISTRY-ART-1 / ASSET-PROVIDER-1：ArtObject 已有 source-backed
  `Cameo/AltCameo/Voxel/Remapable`；project template 现生成 rules Image、art Image、art Cameo
  三项操作，body/Cameo 均 Proposed。Existing-Asset Provider 将 Host 显式提供的有界内容转为
  Manifest-closed 内存 Artifact/SHA-256；allowlist 77，Application 186/186、IDE 2634/2634、
  package 1206；不解析格式、不写磁盘、不调用模型。
- CONTENT-PROJECT-UI-1 NF2：Work rules/art 的明确 capability 是项目工具选择的本地权威；模型返回的
  allowlisted domain/completion 只作派生元数据，并在第二阶段前归一化为 `art-animation + Field`。
  用户原始请求的第一阶段真实响应和第二阶段完整五参数 proposal 均已单独验证；本地 Debug build、
  Project/Pipeline 39/39、`Ra2Ai*` 389/389、Application 186/186、IDE non-UI 2645/2645 通过。
- CONTENT-PROJECT-UI-1 NF3：最新真实第一阶段响应已严格通过现有 parser；旧 UI 把本地意图/项目准入
  拒绝包装为 provider ProtocolError，因而隐藏了具体原因。internal `LocalRejection` 现保持
  provider failure 与本地 rejection 分权，Shell 可显示 NoProject、PairMissing/PairAmbiguous、
  SnapshotUnavailable、ReadOnly、ResourceLimitExceeded 的安全原因；不创建缺失 rules/art 文件，
  不改变顶层 pair 扫描、compiler、Preview、Apply、Save、asset 或 transport 权限。
- CONTENT-PROJECT-UI-1 NF4：Work 的项目成员权威改为项目打开结果与 session 的不可变成员清单，
  不再从 `ProjectExplorer.Items` 重建。上下文摘要显示完整根路径与配对状态；唯一顶层
  `rulesmd.ini + artmd.ini` 即使 art 为空、目录含其他 INI 也可准入。提示词只需提供业务目标与对象 ID。
- CONTENT-PROJECT-UI-1 NF5：真实 DeepSeek 第二阶段返回的四个 ID 与 project template 均正确；旧
  Global `Vehicle.Image` 观测 Enum 才是 Profile 误拒绝 `HTNKART` 的根因。模板 compiler 现把
  source-backed rules/art `Image/Cameo` 作为开放引用：保留 schema/trust/Blocked/identifier 门禁，
  不让样本 Enum 充当闭集。Host 可确定性补齐可选 brief，并只在该 profile 内规范参数名大小写、空 brief
  与单个 `.shp` 后缀；字段库、provider priority、public API 与 Apply/Save 权限未变。
- CONTENT-PROJECT-UI-1 NF6：用户明确指定 DeepSeek 为 Work 项目内容权威。生产 rules/art 路由不再
  暴露固定 project template，改用通用 `preview_ini_project_edit_plan`；模型决定 Section、字段、值、
  注册和跨文档绑定，或返回 clarification。Host 仅验证当前 captured rules/art 范围、结构/资源、安全
  identifier、canonical Preview、显式 single-use Apply、stale 与原子事务。Registry trust、旧 Enum、
  SectionKind 和 diagnostics 对该通用计划只作 advisory；headless 固定模板保持兼容。
- AGENT-KNOWLEDGE-1-R2：NF6 后真实结果暴露知识缺口——模型把角色词 `Art/Body/Cameo` 写成 rules
  字段。新增 source-backed `ra2-rules-art-binding` 并按 project capability 强制选择；运行时 Skill 数为
  16。它冻结 rules Owner.Image → art Section → body/cameo 图、类型列表、Voxel 与 vanilla/Phobos
  `ArtImageSwap` 分界；Field Registry v2 对 generic project 保持 advisory。审计见
  `Docs/AGENT-KNOWLEDGE-1-R2_RulesArtBindingSourceAudit.md`。
- AGENT-SKILL-ROUTING-2：Work 第一轮读取同一 Catalog 的元数据 Manifest，返回有序 Skill 推荐与
  知识缺口；Host 强制合并 capability Skill/field trust、校验模式和 14 KiB 正文预算，再把显式解析结果
  注入执行轮。Chat 仍一次，Skill 不增加工具或写入权限。
- AGENT-REPAIR-1：正常 Work 为 2..4 次调用，只有 allowlisted typed structured failure 可追加一次
  非流式 repair，整个 Work 请求绝对上限五次。repair 复用同一 execution seed，不重跑 intent、Skill 或 HLI
  查询；同一 canonical proposal/Preview 再校验，Shell 仅负责 UI 线程上下文重捕获与最终结果显示。

精确边界与证据见 `Docs/CurrentCapabilities.md`。

## 5. 当前不存在的能力

- CLI 或外部 Agent host。
- 通用/持久化模板库、AI/Faction 完整 profile、全部 SuperWeapon 类型的 typed profile、自动 Apply/Save。
- 生产 SuperWeapon typed profile 已使用 2D-1 注册基础；其它对象家族仍需各自 source-backed profile。
- Job/Event/Artifact Runtime。
- Cameo/Icon、VOX/SliceStack/VXL、SHP 实际生成与项目落盘；当前有 body+Cameo INI binding、
  Manifest 和 Existing-Asset 内存 Provider，但没有 codec/generator/Asset Host transaction。
- RA2TestHost / IRuntimeAdapter / deterministic runtime regression。

## 6. 编辑和信任边界

```text
Provider/model output = untrusted proposal input
Application semantic Preview = deterministic candidate authority
IDE host = active document, currency, Apply and Undo authority
Save pipeline = disk/encoding/backup/rollback authority
User/policy = external cost, overwrite and final commit authority
```

- INI/MAP/真实素材文件是事实源；索引和 Manifest 是投影/产物记录。
- Model/Agent 不直接写文件、不持有 UI 控件、不解析全局 mutable singleton。
- 当前 A4 编辑只支持明确的当前文件字段 Upsert/Replace。
- 当前模板目录含 skeleton、single/dual direct-fire、Arcing/Homing Projectile、YR core Warhead 与
  Ares UnitDelivery/GenericWarhead SuperWeapon；
  complete profile 均要求唯一既有 owner 和完整参数集。
- BuiltIn Skill 只提供过程知识；Field Registry 继续拥有字段 schema/trust，Content Profile 拥有对象完整度，
  IDE Host 拥有 Apply/Undo，Save pipeline 拥有磁盘写入。
- 通用传输重试、模型 fallback 和 custom endpoint tool 均未授权；仅 AGENT-REPAIR-1 的一次性结构化
  重规划例外已实现。

## 7. Field Registry 当前基线

来源：`Docs/ContextCapsule_FR_DQ_4.md`

```text
Runtime BuiltIn rows: 2604
Uniform inferred templates: 0
Auto-extracted rows: 0
Empty/unrecognized quality: 0
Exact identity duplicates: 0
PendingManualReview: 0
```

Diagnostic-only rows保留给 lookup/Hover/Quick Peek/Diagnostics，但不进入 key Completion。
AA/AG Projectile canonical 行保留；错误 Techno/Weapon 上下文只作为 guardrail。

## 8. AI 与 Authoring 当前基线

- 生产模型目录仅 Flash/Pro，Flash 默认。
- SSE streaming、增量 Shell rendering、失败上下文隔离和恢复提示词已完成。
- 官方 endpoint 的明确编辑请求使用 required structured tool。
- Provider prose/raw JSON 不能创建提案；只有本地 A3 Preview 可以。
- Apply 只改内存、一次消费、一个 Undo 单元，永不自动 Save。
- A4-R1 最终证据：build 0/0，non-UI tests 2519/2519，IdeOnly package 1049 files。

## 9. Search 当前基线

- 搜索 Project Explorer 中的规范顶层 `.ini` 文件；当前文件使用内存文本覆盖。
- 支持大小写、全字、500 ms 单文件正则超时、10,000 结果上限。
- 大于 8 MiB 的延迟文件和读取失败会跳过并报告。
- Replace All 只限当前文件，必须 Preview，拒绝 stale，一次 Undo，不自动保存。
- 不存在项目级 Replace 或后台索引。

## 10. UI 当前基线

- 1920x1080 是默认几何基准，主编辑区优先。
- 现代化浅色资源、模板、字体和多个二级界面已实施；具体 Stage Ledger 中标注
  visual acceptance pending 的状态仍需人工验收。
- Search 作为独立浮动 Dock；布局可以 Return Home / Reset / persist。
- 深色主题后置。

## 11. 素材路线事实

当前没有生产素材生成代码。用户确认的 VXL 近期路径是：

```text
VOX -> 1 pixel = 1 voxel 的无损二维切片 -> SliceStack Manifest
-> VXLSE III 导入 -> 最终 VXL/HVA 修整与保存
```

不得把切片包写成“最终 VXL 已生成”。Cameo/Icon 与 SHP 同样需要先冻结中立
artifact contract、palette、manifest、provider adapter 和验证规则。

## 12. 构建与验证

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

不得创建或使用 legacy `RA2IniEditor.sln` / `RA2IniEditor.csproj`。

## 13. 当前下一入口

HLI-1A1 已完成：22 个 internal Classification/Language 文件迁入 Application，IDE
复用同一实现；15-type Experimental allowlist、nullable occurrence、重复 Section
body-span 隔离、Reference 空成功/无法解析失败、8M chars/10k items 和取消门禁均已
通过 31 项 headless、54 项迁移和 2526 项完整测试。

HLI-1A2 已完成：diagnostic/FieldTrust 唯一闭包位于 Application，IDE 保留单向
ViewModel adapter，public allowlist 精确为 18。

HLI-1B 已完成：6 个 TextModel + 2 个 TextChange 已原子迁入 Application internal，
唯一 semantic preview engine 与 11 个新增 Experimental public types 已实现，IDE 保留
thin Host adapter，allowlist 精确为 29。82/82、88/88、390/390 和 2526/2526 均通过。

HLI-1C 已完成：现有 `IRa2IniEditPreviewService` 继续是唯一 Host admission seam；
`FromAutomation` 现验证 operation evidence、span 与 candidate/change 闭合，Workspace 在
active-slot admission 前验证返回 wrapper 与 invocation snapshot/plan 的实例绑定。无需新增
`RegisterPreview`、proposal handle store、public Apply 或 Gateway adapter。Application
allowlist 保持 29；82/82、Host 53/53 和完整非 UI 2537/2537 通过。

HLI-2A 已完成：固定四项 immutable catalog、version=1、Query/Edit risk、现有限制和 typed
façade 已实现；新增精确 6 个 Experimental public 类型，allowlist 29 -> 35。Gateway 只委托
两个 canonical service，无 generic dispatcher、wire schema、Apply/Save 或 Job/Event/Artifact。
12/12、Application 94/94、HLI-1C 11/11 和完整非 UI 2537/2537 均通过。

HLI-2B 已完成：现有唯一 `Ra2IniEditPreviewService` 已改为 typed Gateway consumer，unlimited
`PreviewForHost` 已删除；Shell 使用同一 Gateway descriptor 在 provider 请求前拒绝超 8 MiB
的明确编辑，advisory 继续使用截断上下文。Application 94/94、聚焦 78/78、完整 non-UI
2547/2547 与 clean package 通过；public allowlist 保持 35。

HLI-2C 已完成：确定性 Gateway scenario 与 DeepSeek-compatible loopback 均覆盖
Query/Validate -> structured plan -> Preview -> explicit single-use Apply -> updated-snapshot Validate；
Shell 只在成功事务后用 committed text 刷新当前文件 Problems。Application 94/94、聚焦 37/37、
完整 non-UI 2549/2549、IdeOnly package 1123，public allowlist 保持 35。Minimum HLI-v1 完成。

POST-HLI-0 已完成：当前 Gateway 是可无头进程内消费但不可直接序列化的 Experimental API；
独立 Host 尚缺 wire/session/permission，CONTENT-1 则可复用现有 schema/query/reference/
diagnostics/Preview/Apply 链。路线已裁决为 `CONTENT-1 -> HOST-1 -> ASSET`。

CONTENT-1 已完成：1A Field Schema、1B Reference Resolve、1C Section Creation、1D internal compiler、
1E Template Gateway、1F IDE Agent integration 和 CONTENT-UI-1 主工作区 Diff 均已实现验证。
Application public allowlist 58、Gateway catalog 7、methods 9；字段库继续是 effective schema/trust
事实源，但不承担对象模板或 reference target-kind 推断。Debug build 0/0，Application 146/146，
non-UI 初始 2568/2568；非严格工具兼容修复后 non-UI 2576/2576。电脑操控、自动真实 DeepSeek
和物理 DPI 视觉验收未运行。

AGENT-MODE-1 / AGENT-KNOWLEDGE-1 已完成：Chat/Work 显式模式、direct-fire complete profile、
当时 16 个 BuiltIn RA2 Skill 和按领域/capability 注入均已实现（CONTENT-2E 后当前为 18 个）。Application allowlist 59，Application 147/147、
IDE non-UI 2580/2580、clean package 1171 files；未运行真实 DeepSeek、电脑操控或物理视觉验收。

MODE-1-R1 窄修复后 Release focused 41/41、IDE non-UI 2583/2583。Debug 输出被正在运行的 IDE/Visual
Studio 锁定，未强制关闭用户进程；真实 provider 的 required-tool 复验仍待用户手工执行。

CONTENT-2A/2B 已实现双 direct-fire、原版 Arcing/Homing Projectile 与 YR core Warhead profiles。
CONTENT-2D-0/1 已实现 internal 对象闭包/数字注册基础，门禁为 build 0/0、Compiler/Template 37/37、
Application 162/162、IDE focused 106/106、IDE non-UI 2610/2610；现有生产 Profile 尚未启用注册。
CONTENT-2C AI 写入继续按用户要求冻结。CONTENT-2D-3、FIELD-REGISTRY-ART-1 与
ASSET-PROVIDER-1 已实现 body+Cameo rules/art binding、Manifest 与内存 Existing-Asset Provider；
`CONTENT-PROJECT-UI-1` 的生产 Work 路由在 NF6 已从固定 project template 转为模型主导通用计划，
继续复用项目 Proposal、Project Diff、显式原子 Apply 与 compound Undo。Application 188/188、
IDE 2668/2668；KNOWLEDGE-1-R2 已注入 source-backed binding Skill，AGENT-SKILL-ROUTING-2 已让
第一轮从同一 Manifest 选择并由 Host 解析第二轮 Skill；真实 provider/WPF 验收尚未执行。
Asset Host 不是该接线的前置，素材待办保持非阻塞。
完整 Techno 与 Faction、以及其余 SuperWeapon 类型仍需各自 source-backed profile。不得直接公开 Apply/Save，
或跳到未经契约的 wire、外部可执行 Skill、Job/Event/Artifact 和素材 provider。

MODE-1-R5 已修复 Work 路由的否定意图误判：“不要使用循环或交替开火”允许进入现有 complete
direct-fire profile；正向循环/交替或混合“不要循环、但要交替”仍 fail closed。该修复不新增
循环开火能力，不改变模板、Gateway、Apply/Save 或 public API。

AGENT-CONTEXT-3 已完成：Work 第一次分析/Skill 调用与第二次执行调用共享同一受限会话、当前主题和
`current/rules/art` 捕获快照投影；第一轮可请求最多 8 个命名 Section/引用事实，Host 在两次 provider
调用之间仅通过现有 HLI Gateway 对原快照执行本地只读查询。没有第三次查询/分析调用、任意路径、public API、
XAML、parser、Field Registry、Preview/Apply/Undo/Save 语义变更。Application 188/188、IDE 2673/2673；
真实 DeepSeek 首次 `context_queries` 验收发现 target 查询与执行作用域断链。AGENT-CONTEXT-3-FIX1
已修复：显式查询 `rules/art` 的普通字段编辑进入既有项目预览路由；成功查询的目标写入第二轮执行契约；
模型仍落错文档时只基于同一捕获快照给出跨文档定位并拒绝 Proposal，不自动 retarget/retry/apply/save。
FIX1 聚焦 23/23、Application 188/188、IDE 2675/2675；真实 DeepSeek 同用例复测尚未运行。

停止条件：若需要改变 parser、diagnostics、Field Registry priority、Save、
Apply ownership、public API、程序集方向或持久化格式，必须先形成对应风险契约。

## 14. 历史说明

旧累积 Context 和 CurrentPhase 已移入 `Docs/Archive/`。它们保留完整历史，但不
再参与当前状态判定。需要某阶段细节时读取其 Contract、Stage Ledger 或 Context
Capsule，不把历史 “next phase” 当成当前指令。

## 2026-08-25 AGENT-TRACE-1 / Search startup lifecycle update

- Work 的语义检索结果现在以单行 metadata 摘要展示；计数只来自 Host 已执行的 query batch、规范绑定和
  成功 bounded query result。Chat、无活动、clarification/provider failure 不显示。
- AutomationId 为 `AiAssistant.RetrievalSummary`；没有 XAML、raw prompt、路径或 provider metadata 暴露。
- `ShellDockLayoutCoordinator` 不再在启动默认拓扑中 materialize 默认隐藏的 Floating 工具；Search 首次
  显式打开时才通过 canonical `ShowAndActivate` 创建浮窗。Fix2 在任意持久化布局恢复后、宿主刷新前
  再次规范 Search 为隐藏，避免旧 v2 布局把它恢复到底部或启动可见；显式打开强制回到 Floating Home。
- public API、layout persistence、Search semantics、Preview/Apply/Undo/Save 和 legacy 均不变。
- 自动证据：Fix2 focused 38/38、IDE 2745/2745、Application 198/198、Release build 0/0、
  IdeOnly package 1247 files；真实 DeepSeek/物理 UI 为 NotRun。
## 2026-08-25 AGENT-REPAIR-1 completed stop point

- `AGENT-CONTEXT-3-FIX1` 的 `[HTNKART]` / `Remapable=yes` 真实 DeepSeek 用例已由用户验收通过。
- 已完成 `AGENT-REPAIR-1` 代码事实审计、最终契约和 R1-1 → R1-5 实现；状态为 Completed / automated verified。
- Chat 仍 1 次调用，正常 Work 仍 2 次；只有 typed allowlist 中模型可修正的第二阶段结构化失败可追加 1 次非流式 repair，Work 总上限 3 次。
- repair 不重跑 intent、Skill resolution 或 HLI query，不切换模型/provider，不处理 transport/timeout/cancel/stale/resource/safety failure，不自动 retarget、Apply 或 Save。
- 实现保持 IDE internal；public API、持久化、XAML、Field Registry、parser、diagnostics、preview/apply/save 语义均不变。
- 自动证据：Debug build 0 warnings/0 errors；focused 125/125；Application 188/188；IDE 2706/2706。真实 DeepSeek 与物理 UI 未运行。
- 权威文档：`Docs/AGENT-REPAIR-1_BoundedStructuredReplanCodeFactAudit.md`、`Docs/AGENT-REPAIR-1_BoundedStructuredReplanFinalContract.md` 与 `Docs/AGENT-REPAIR-1_StageLedger.md`。

## 2026-08-25 SHELL-LAUNCH-1 stop point

- 文件关联启动已接通：单个既存 `.ini` 裸参数 -> 直接父目录项目 -> Shell ready gate -> 规范 project/session 初始化 -> 精确文件加载 -> editable source session。
- 启动层不直接读取或写入 INI；Project Explorer、编码、Field Registry、高亮、诊断、Undo/Redo、Save 与 Agent 边界不变。
- 既有 `--automation-open-folder` 兼容保留；未新增通用 CLI 参数；没有 XAML/布局/AutomationId 变化。
- 单实例与向已运行窗口转发未实现，必须由独立 `SHELL-LAUNCH-2` 契约处理。
- 权威文档：`Docs/SHELL-LAUNCH-1_FileAssociationLaunchFinalContract.md` 与 `Docs/SHELL-LAUNCH-1_StageLedger.md`。
- 自动证据：focused 10/10、Application 188/188、IDE 2715/2715、Debug build 0 errors/1 existing nullable warning、IdeOnly clean package 1232 files；物理 Explorer 双击待用户验收。
## 2026-08-25 CONTENT-2E completed context update

- `CONTENT-2E` 已完成：Ares UnitDelivery + GenericWarhead 两个 typed complete profiles，其他明确类型走
  model-owned generic project plan；统一进入现有 Project Preview / Diff / Apply / compound Undo。
- `Action` 没有被伪造为通用默认，typed 请求必须显式提供；provider Building 与 AlwaysGranted 策略互斥。
- Work intent/tool/prompt/adapter 与 18 个 BuiltIn Skill 已接线；Field Registry/Diagnostics 对 generic plan
  仅为 advisory，Host 安全边界不变。唯一 rules/rulesmd 即可工作，art 和素材不是前置条件。
- 自动证据：focused 8/8 + 14/14、Application 196/196、IDE 2722/2722、Debug build 0/0、
  IdeOnly clean package 1241 files；真实 DeepSeek/WPF/游戏内行为未验证。
- 权威文档：`Docs/AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerCodeFactAudit.md`、
  `Docs/AUTOMATION-CONTENT-2E_SourceCapabilityMatrix.md`、
  `Docs/AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerContinuousFinalContract.md`、
  `Docs/AUTOMATION-CONTENT-2E_StageLedger.md`。

## 2026-08-25 CONTENT-2E-FIX1 context update

- 真实 Work 验收暴露的“单文档请求不能调用项目内容模板工具”不是 DeepSeek 参数错误，而是 proposal
  上下文选择器遗漏 CONTENT-2E 新增的三个 SuperWeapon 项目能力。
- `Ra2AiAuthoringToolCatalog.UsesProjectContext` 现在是 IDE-internal 项目作用域权威判定；Prompt 与
  bounded replan proposal 选择共同复用，避免工具路由与上下文路由再次分叉。
- adapter 仍拒绝项目工具搭配文档上下文；未放宽路径、项目成员、stale、资源、Preview、Apply/Save
  或素材边界。
- 新增项目工具/作用域一致性测试及三个 SuperWeapon 项目模式的上下文选择测试；Release focused
  19/19、IDE full 2726/2726、Release build 0/0、IdeOnly package 1241 files。Debug 输出因用户正在
  运行 IDE/Visual Studio 被锁定，未强制关闭进程。

## 2026-08-25 CONTENT-2E-FIX2 context update

- 用户自然语言 UnitDelivery 验收暴露：第一轮可能给正确 capability 附带漂移的 domain/completion，第二轮
  可能把显示名称当作 Section ID；二者分别导致 Work 契约拒绝和 Existing Section 查询失败。
- Intent 层对三个 SuperWeapon 项目能力统一规范为 `superweapon + Complete`，并要求自然/显示名称先推断
  canonical ID 候选、再通过原有 HLI `get_section` 对请求期快照查询。
- typed profile 只在捕获 rules 的现有语义模型上，将精确且唯一的同类 Section/`Name`/`UIName` 别名规范为
  canonical ID；不使用模糊匹配、硬编码对象表、第二 parser/index、缓存或持久化。原类型、引用和 Preview
  门禁保留，多义/缺失对象继续失败。
- 自动证据：Application focused 10/10、IDE related 61/61、SuperWeapon integration 18/18、Application
  198/198、IDE 2733/2733、Release build 0 errors / 1 个既有 nullable warning、IdeOnly clean package
  1241 files；真实 DeepSeek/WPF/游戏内行为待人工验收。
- 未修改 XAML、Shell、Field Registry、Diagnostics、Completion、Save、public API、素材或 legacy。

## 2026-08-25 AGENT-QUERY-2 context update

- Work 的单次精确查询已升级为 IDE-internal 有界语义检索：`search_objects` 复用现有
  `Ra2DocumentSemanticModelBuilder`，只读取同一请求捕获的 `current/rules/art` snapshot。
- 最多两轮 compact refinement；重复 fingerprint、无进展、澄清、provider 失败和轮次上限都有明确停止状态。
  正常 Work 最多 4 次 provider 调用，叠加既有一次 typed structured repair 时绝对上限 5 次。
- 规范绑定以 `(target, entity role)` 为身份，只有唯一 exact ID/`Name`/`UIName` 结果可绑定；歧义继续补查或澄清。
- SuperWeapon evidence pack 自动读取 `[SuperWeaponTypes]` 和已绑定对象 Section，不构造默认值、不否决模型内容。
- Project Work prompt 只保留用户请求、intent、Skills、project projection、bindings 和 Host facts，移除无关 caret-local 上下文。
- public HLI、持久化、parser、Field Registry、Diagnostics、Completion、Preview/Apply/Save/Undo、XAML/Shell 和 legacy 均未改变。
- 自动证据：AI 466/466、Application 198/198、IDE 2740/2740、Release build 1 existing nullable test warning/0 errors、IdeOnly package 1245 files；
  真实 DeepSeek/WPF/游戏运行时为 NotRun。
- 权威文档：`AGENT-QUERY-2_SemanticRetrievalContinuousFinalContract.md`、`AGENT-QUERY-2_StageLedger.md`。

## 2026-08-25 UI-DOCK-SEARCH-STARTUP-FIX3 context update

- 真实 .NET Runtime 事件证明隐藏的 Search 直接调用 AvalonDock `Float()` 会崩溃；浮动 Home 迁移现先
  `Show()` 到有效 Pane，再覆盖旧/底部 PreviousContainer 并创建真实浮动宿主。
- 启动布局恢复期间，原控制器只抑制原生浮窗，未抑制主 Dock 的反序列化中间态。`ShellDockManager`
  现从 XAML 创建时即透明且不可交互，在恢复完成并重新隐藏 `Tool.FindReferences` 与 `Tool.Search` 后
  才一次性开放呈现。
- 新增 loaded WPF Window 级浮窗宿主回归与启动呈现顺序回归；focused Debug tests 3/3、IDE Debug
  full 2746/2746、Release build 0 errors / 1 existing nullable warning。
- Dock XML 格式、其他工具持久化位置、Search/Find References 业务、INI/Field Registry/Diagnostics、
  Preview/Apply/Undo/Save 与 legacy 均未改变；最终首帧无闪现仍由用户手工验收。

## 2026-08-26 AGENT-WORK-ENTRY-1 context update

- Work admission no longer treats provider descriptive metadata as Host authority. Unknown/additive fields, missing optional
  lists/query placeholders, casing/separator variants and unknown domain/capability IDs are bounded and normalized.
- Fatal intent failures are now typed, request-scoped and observable: wrong response/tool/count, oversized payload,
  malformed/non-object JSON and duplicate root properties. Pipeline no longer discards the local failure detail.
- Context-query admission is per item. Only `current/rules/art` can execute; path-like or malformed items are dropped and
  recorded, while valid sibling queries and the Work request continue.
- All production current-document modes now expose the generic model-owned Document Plan tool, and all production rules/art
  modes expose the generic Project Plan tool. Local typed template/Profile compilation remains for explicit
  compatibility/headless use but is no longer the production Work content authority.
- Generic Project Plan accepts additive explanation metadata and common shape variants while preserving duplicate/target,
  resource, identifier, snapshot, Preview, explicit Apply and no-auto-Save boundaries.
- Current-document generic Upsert plans infer missing Section creation into canonical Preview while retaining explicit Apply,
  Undo and no-auto-Save authority. Automated evidence: Debug build 0/0, AI 509/509, Application 198/198, IDE 2756/2756,
  diff check clean. Real DeepSeek/WPF manual verification is still required and is not claimed by fixtures.
- W1-6 further removes generic proposal presentation metadata from execution admission. Document/project `message`
  is ignored and invalid/missing `summary` receives a local default; executable `operations/documents` remain fully
  validated. Clarification alone requires a readable bounded message, and normal plus repair paths share this adapter.
  Automated evidence: focused 74/74, AI 493/493, Application 198/198, IDE 2771/2771, Debug build 0/0; the rebuilt
  DLL contains none of the prior generic proposal-message rejection strings. Real provider/UI retry remains manual.

## 2026-08-26 DIFF-REVIEW-1 and ASSET-VOX-1 investigation update

- DR1-A -> DR1-E is implemented and automated verified. Canonical Result owns exact `CandidateText`; Changes remains
  the existing unified diff; Object Context is direct-depth, snapshot-only and bounded. No partial apply was added.
- Verification: Debug build 0/0, focused 19/19, Application 198/198, IDE 2779/2779, DR1 stage package 1255 files;
  latest handoff package with the VOX research document contains 1256 files.
  Physical visual acceptance remains NotRun.
- VOX investigation confirms the near-term product route but separates model generation from deterministic game-format
  preparation. DeepSeek plans/selects skills; a replaceable image-to-3D provider generates geometry; local code owns
  VoxelScene, palette, VOX and SliceStack; VXLSE III owns first-version normals/final VXL/HVA closure.
- Existing `IRa2AutomationAssetProvider` is not a job/intermediate-artifact API and must not report VOX/PNG as final
  `VxlModel`. A separate bounded authoring workflow is required; final VXL/HVA can then re-enter the existing provider.
- Research and proposed staged route are in `Docs/ASSET-VOX-1_SystemInvestigationAndArchitectureProposal.md`.

## 2026-08-26 ASSET-VOX-1A baseline update

- Application now contains internal, UI-neutral separated voxel assembly contracts and bounded VXL/HVA metadata probes.
  The assembly is a rooted acyclic part graph and supports independent Body, Turret, Barrel and Other VXL/HVA files.
- The probe does not decode voxel spans or write assets. It verifies bounded header/footer metadata, finite transforms,
  dimensions, expected Sections and all-or-nothing VXL/HVA companion closure.
- Four real local VXL/HVA pairs passed the metadata probe; a real `tnkd + tnkdtur` assembly passed closure. The same two
  VXL files were independently read by the user-authorized VoxelNormalForge with matching Section, dimensions and normal type.
- One real `ttank.hva` has a single unnamed Section. The compatibility rule accepts it only when the paired VXL has one
  unambiguous expected Section; ambiguous multi-Section cases remain failures.
- This baseline entry predates the supplied VXLSE package and is superseded by the compatibility completion below.
  A detached Barrel remains optional calibration evidence rather than an assembly-contract prerequisite.
- Verification: focused 9/9, Application 207/207, Debug build 0 errors with one pre-existing nullable warning. The final
  IDE full run was 2778/2779 because an untouched WPF STA resource/Popup teardown test failed; its immediate isolated
  rerun passed 1/1, while an earlier full run passed 2779/2779. IdeOnly clean package contains 1262 files.
- Continue from `Docs/ASSET-VOX-1A_GoldenProbeAndSeparatedAssemblyFinalContract.md`; direct VXL/HVA generation remains
  deferred even though the source-backed SliceStack coordinate convention is now frozen.

## 2026-08-26 ASSET-VOX-1A VXLSE compatibility completion

- The supplied VXLSE III executable is file version `1.3.9.3281`, product version `1.4.0.0`, SHA-256
  `DB9A882A74E16ECB1D938C6D07EC4C97B28D51EF23975730DF2211E354916458`. Its adjacent Pascal source and three RA2
  theatre palettes are the Stage 1A compatibility authority.
- `Ra2VxlseSliceImportContract` now freezes the source-backed Downward and Rightward pixel mappings. Highest VXL Y is
  serialized first; X and Z retain increasing local order. A 3x4x5 asymmetric synthetic part round-trips every coordinate
  through both layouts.
- Import packages must use exact dimensions and a direct alpha-channel PNG. Alpha zero is empty and any non-zero value is
  occupied. The target Section must be new/cleared because VXLSE does not clear transparent cells, and normals must always
  be regenerated because the importer does not write them.
- Westwood 768-byte PAL files are decoded as 256 6-bit RGB triples expanded by `*4`; exact palette RGB values avoid
  VXLSE re-quantization drift. The reviewed nearest-colour/tie-break behavior is deterministic and tested.
- VXLSE's importer resets bounds from a session-global land/air setting, so world-axis interpretation, pivot/mount,
  normals/HVA quality and game readiness remain explicitly unresolved. Lack of a real Barrel sample does not block the
  assembly model; it only defers visual and pivot calibration.
- Stage 1B should migrate the reviewed VoxelNormalForge codec into a deterministic `VoxelSceneSnapshot` and RGBA
  SliceStack exporter. Only then should the actual executable PNG -> VXLSE -> decoded VXL acceptance loop run.
- Final Stage 1A verification: focused assembly/slice tests 17/17, Application 215/215, IDE 2779/2779, Debug build
  0 errors with one pre-existing Field Registry nullable warning, IdeOnly clean package 1264 files.

## 2026-08-26 ASSET-VOX-1B canonical voxel core update

- Added an internal immutable single-part `Ra2VoxelSceneSnapshot`. It owns a bounded sparse cell set, a copied 256-entry
  palette profile, deterministic source hashes, connectivity/symmetry facts and a versioned canonical SHA-256 hash.
  Detached Body/Turret/Barrel identity remains in the Stage 1A assembly graph.
- Added a restricted deterministic MagicaVoxel v150 codec for one `SIZE`/`XYZI` model plus explicit `RGBA`. It rejects
  duplicate/out-of-bounds cells, colour index zero, malformed parent/chunk lengths and oversized occupancy; scene graph,
  material and animation semantics are not inferred.
- Migrated and hardened the user-authorized VoxelNormalForge VXL span decoder. All body/table/span offsets and packet
  counts are bounded. It emits one canonical snapshot per VXL Section and exposes no VXL writer.
- VXLSE source review corrected an important palette boundary: the VXL header's 768-byte `PaletteData` is documented as
  never used, so canonical VXL decoding requires the caller's actual external palette profile instead of trusting those
  bytes.
- Added exact Downward/Rightward RGBA SliceStack export/import and a dependency-free, bounded, deterministic PNG codec.
  The decoder accepts standard PNG filters 0..4 but only non-interlaced 8-bit RGBA and validates CRC/chunk/decompression
  bounds.
- Automated evidence: focused Stage 1A+1B 27/27, Application 225/225, IDE 2779/2779; IDE-only Debug build 0 errors with
  one pre-existing Field Registry nullable warning. Clean package result is recorded in `ASSET-VOX-1B_StageLedger.md`.
- The supplied VXLSE executable applies an additional mapping, `VXL(x,y,z) = (input z, input X-1-x, input y)`, beyond
  the reviewed raster reader. An additive inverse bridge now exports canonical `Y,Z,X` import volume without changing
  the generic SliceStack contract or standard VXL reader.
- Real executable structural acceptance passed after import into a fresh empty Section: `Body`, `3x4x5`, 5/5 expected
  cells and palette indices, canonical hash
  `29A4A1150EEFB6305021B29CA37B7C3F58B0B845FEB779C63F93EA0DCF0161C2`.
- Final evidence: focused 30/30, Application 228/228, Debug build 0 errors / 1 pre-existing Field Registry nullable
  warning. IDE full was 2778/2779 due to the unchanged known WPF STA resource/Popup teardown flake; isolated rerun passed
  1/1. IdeOnly clean package passed with 1273 files. Visual/pivot/normal/HVA review and game smoke remain NotRun; this
  result is not `GameReady` certification.

## 2026-08-26 ASSET-VOX-1C completed update

- Code audit confirms the existing `IRa2AutomationAssetProvider` is final-Manifest closure and cannot carry GLB/VOX/PNG
  candidates. Application also explicitly forbids process/file orchestration; no reusable production provider Host exists.
- The proposed boundary is a new headless `RA2IniEditor.AssetHost` assembly with transient run/workspace ownership,
  trusted allowlisted local process execution, protocol v1, bounded progress/cancel/timeout/crash and hash-verified GLB/PNG
  candidates. It is process fault isolation, not an OS sandbox.
- Revised contract freezes one internal `ProbeAsync`/`RunAsync` Host seam plus a read-only async-disposable workspace
  lease. Probe verifies executable/protocol/model/capability/license readiness but never authorizes a later Run; Run repeats
  all security-relevant checks.
- Orphan cleanup is limited to marker-valid, unlocked, TTL-expired direct children of a dedicated workspace root. Process
  stdout/stderr must be drained concurrently through bounded paths, with explicit cancel/timeout/terminal/exit/promotion
  race tests and no deadlock-prone sequential stream read.
- General persistent Job/Event/Artifact Runtime, remote API/secrets, real TRELLIS installation, UI, project writes and
  final VXL/HVA remain outside 1C. The first proof uses a deterministic managed fixture provider.
- The revised R4 contract was approved and 1C-1 through 1C-5 are complete. `RA2IniEditor.AssetHost` and its deterministic
  managed fixture tests implement the internal Host seam, probe, transient lease/workspace, bounded protocol/process
  lifecycle, hash/magic validation, provenance and replay evidence.
- Automated closeout: AssetHost 38/38, Application 228/228, IDE 2779/2779, IDE-only build 0 warnings/0 errors and clean
  source package 1295 files. Application allowlist remains exactly 77; AssetHost exports zero public types.
- Still deferred: real TRELLIS/Hunyuan provider setup, visual-quality certification, OS sandboxing, persistent jobs,
  UI/Work/project integration, voxelization into the 1B canonical core, VXL/HVA and GameReady evidence.

## 2026-08-26 ASSET-VOX-1C-P1 environment/authorization update

- Completed P1-0 docs-only audit. Existing `RA2IniEditor.AssetHost` remains the sole process/workspace/artifact authority,
  and Application-internal `Ra2VoxelSceneSnapshot` remains the sole canonical voxel truth; no duplicate Host, job DTO,
  provider API or voxel model is planned.
- Observed local environment: Windows, RTX 4080 SUPER 16,376 MiB, Python 3.11.9; required ML packages and model cache are
  absent. No real provider was installed or executed.
- Official upstream comparison rejects TRELLIS.2 as the current local baseline because its documented minimum is Linux
  plus at least 24 GB VRAM. The first candidate is Hunyuan3D-2mini shape-only, whose real compatibility remains unverified.
- Frozen P1 design: self-contained single-file adapter executable, fixed external bundle, existing protocol v1, one
  process per probe/run, bundle/model/runtime provenance, `BestEffort` seed, no texture, no product-time download and no
  project write. 1D later consumes GLB bytes and constructs 1B canonical snapshots.
- P1-1 through P1-5 are blocked pending explicit user acceptance of the Tencent Hunyuan 3D 2.0 Community License and
  authorization to create an isolated Python 3.11 environment and download pinned source/dependencies/weights. General
  permission to continue the roadmap does not satisfy this external authorization gate.
- Authoritative docs: `ASSET-VOX-1C-P1_RealProviderEnvironmentCodeFactAudit.md`,
  `ASSET-VOX-1C-P1_HunyuanMiniProviderFinalContract.md`, `ASSET-VOX-1C-P1_StageLedger.md`.

## 2026-08-26 ASSET-VOX-1C-P2 remote provider update

- Added internal executable `RA2IniEditor.AssetProviders.TencentHy3D`. It maps the unchanged Host protocol to the official
  Tencent OpenAI-compatible submit/query endpoints and emits one Hunyuan 3D 3.1 Geometry GLB candidate.
- Dedicated User/process settings are `RA2INI_HY3D_API_KEY`, optional exact official
  `RA2INI_HY3D_BASE_URL`, and `RA2INI_HY3D_FREE_ONLY_CONFIRMED=1`. Generic OpenAI/DeepSeek/CAM keys are never read.
- Existing Host clears inherited child environment, then retains only `SystemRoot`, `WINDIR`, `TEMP` and `TMP`; the
  adapter reads Windows User variables directly. Keys, proxies and arbitrary user variables are not inherited.
- Request submission is at-most-once. Polling addresses only the returned JobId. Artifact downloads are HTTPS-only,
  bounded, omit authorization and end in existing Host hash/magic/lease validation. Evidence excludes keys, signed URLs,
  image bytes, raw responses and absolute paths.
- Automated evidence: build 0/0; AssetHost/provider 47/47; Application 228/228; IDE 2779/2779; clean package 1309;
  provider exported public types 0. Shell/XAML/INI/Field Registry/editor/canonical voxel core unchanged.
- Live P2-3 first exhausted 3/3 attempts with zero JobIds. Non-billable invalid-key probes isolated the child-environment
  root cause; after the four-variable Host fix, the user authorized call 4. It reached `DONE` after 42 polls / about 2m10s
  and produced a Host-validated 8,991,920-byte GLB, 77,888-byte preview and sanitized provider report. The glTF 2.0 asset
  contains one scene/node/mesh/primitive, 249,567 vertices and 499,698 triangles. Provider credit fields were absent, so
  the Tencent console remains authoritative for free-pack consumption. No fifth call is authorized.
- Authoritative docs: `ASSET-VOX-1C-P2_TencentHy3DRemoteProviderCodeFactAudit.md`,
  `ASSET-VOX-1C-P2_TencentHy3DRemoteProviderFinalContract.md`, `ASSET-VOX-1C-P2_StageLedger.md`.

## 2026-08-26 ASSET-VOX-1D completed baseline

- 1D-1 through 1D-5 are implemented and automated verified. Application now owns an internal BCL-only restricted GLB
  reader, transformed immutable mesh/topology facts, explicit glTF-to-canonical normalization, deterministic
  triangle/AABB surface rasterization, watertight exterior fill, palette selection and typed review-required result.
- One request yields one caller-declared part and reuses the Stage 1A assembly plus Stage 1B canonical snapshot/codecs.
  The certified P2 mesh remains Body-only because it has one connected geometry and no colour/material or semantic parts.
- Real result: `29x64x31`, 20,261 occupied cells, canonical hash
  `3FC301CC7B1336635EBD137E8312D85179A32E501CC60E1FB983E2DB4D986D90`; VOX and SliceStack round-trip exactly.
  Final timing after the packed-edge topology fix is 187 ms parse/topology and 99/81 ms for two voxelization passes.
- Focused 10/10, Application 238/238, IDE 2779/2779, AssetHost 47/47 and Debug build 0 errors with one pre-existing
  nullable test warning; IdeOnly clean package contains 1315 files. No external provider call occurred.
- Application allowlist remains 77 and AssetHost exports zero. Gateway, persistence, project-write, UI, INI, Field
  Registry, final VXL/HVA and game-validation authority are unchanged.
- Continue from `Docs/ASSET-VOX-1D_StageLedger.md`. The next independent review is background product composition/preview
  or detached-part/palette review; neither is implied by 1D completion.

## 2026-08-27 ASSET-VOX-1E natural-language style baseline

- 1E-1 through 1E-5 are implemented and automated verified. The IDE resolves bounded project-contained
  `VOXEL_STYLE.md` sources, owns a dedicated one-call structured compiler and a fully keyed disposable cache.
- Application locally validates immutable plans and recolours only existing canonical cell palette indices. Geometry,
  occupancy, part identity, source snapshot and the accepted 1B VOX/SliceStack authority are unchanged.
- Text-only profiles paint deterministic top/side/underside/edge/interior regions. Semantic glass/tyre/accent/remap rules
  remain unresolved without an explicit mask; no model-generated per-cell data is accepted.
- The existing 20,261-cell Body candidate replays deterministically to snapshot hash
  `1693CB306125C1701B368DCCF8F2280534C96BE73F887DA792F162B3F876DA4A`; review artifacts are under excluded
  `artifacts/asset-vox-1e-acceptance/p2-body-64/`.
- Full gates pass: Application 249/249, IDE 2787/2787, AssetHost 47/47, build 0 errors with one pre-existing warning.
  Application allowlist remains 77 and AssetHost exports remain 0.
- No UI/Shell, real DeepSeek call, project Apply/Save, VXL/HVA/normals or game validation was added. Continue from
  `Docs/ASSET-VOX-1E_StageLedger.md`; each deferred boundary requires a separate approval.

## 2026-08-27 ASSET-VOX-1E-UI product composition baseline

- The completed 1E style pipeline now has a single-instance central IDE workspace opened from
  `Tools -> Voxel Style Preview`. It is a dynamic non-floating `LayoutDocument`, is absent from managed dock profiles,
  and is closed before layout serialization.
- Source admission is read-only and limited to a bounded single-model `.vox` inside the active project. Loading produces
  the original SliceStack locally and never creates a provider request.
- An explicit compile resolves built-in/project/directory/request style sources, uses the currently selected DeepSeek
  model through the dedicated compiler/cache, locally colourizes, and atomically publishes immutable review artifacts
  only when the source/style generation is still current.
- The workspace renders original/result/region-mask/palette images plus roles, rules, assumptions, review flags and
  geometry invariants. Session acceptance is deliberately ephemeral and has no apply/save/export/VXL/HVA authority.
- Gates: focused 6/6, Application 249/249, IDE 2793/2793, AssetHost 47/47, solution build 0 warnings/0 errors and IdeOnly
  clean package 1340 files. No real DeepSeek call or physical visual smoke was performed.
- Authoritative docs: `Docs/ASSET-VOX-1E-UI_CodeFactAudit.md`,
  `Docs/ASSET-VOX-1E-UI_FinalContract.md`, and `Docs/ASSET-VOX-1E-UI_StageLedger.md`.

## 2026-08-27 ASSET-VOX-1E-UI-FIX1 runtime compatibility update

- First physical open found a XAML runtime-only style mismatch: a WPF `GridSplitter` referenced the AvalonDock-only
  `IdeDockSplitterStyle` (`LayoutGridResizerControl` target).
- The workspace now uses the existing WPF `UiGridSplitterStyle`; no shared theme, Shell/layout, AutomationId, provider,
  project-write, VXL/HVA or style-pipeline behavior changed.
- Regression coverage now combines an exact XAML resource-key contract, the closed collection-style adoption allowlist,
  and real STA construction of `Ra2VoxelStyleWorkspaceView` through `InitializeComponent()`.
- Evidence: focused 5/5, IDE 2793/2793 and solution build 0 warnings / 0 errors. Physical reopen is pending user confirmation.

## 2026-08-27 ASSET-VOX-1F-CORE-1 high-value core migration update

- Selectively migrated the user-authorized VoxelNormalForge visible-face algorithm and exact RA2/TS normal palettes into
  Application-internal bounded code; the old project, mutable model, CLI, OBJ bridge and Writer are not dependencies.
- Surface projection and normal baking consume only `Ra2VoxelSceneSnapshot`, so existing VOX and VXL decoders converge on
  one implementation. Results are immutable, source-hash-bound derived data and do not modify snapshot schema or files.
- Shared neighbourhood checks now serve style geometry, surface extraction and normal estimation without changing colourizer
  ordering. VOX receives a generated normal review field; no existing VXL `normalIndex` preservation is claimed.
- Evidence: new 6/6, voxel 48/48, Application 255/255, AssetHost 47/47, Release build 0 errors / one pre-existing warning;
  IDE full 2798/2799 hit the documented WPF Popup teardown flake and its isolated rerun passed 1/1. The final clean-package
  command is recorded in the task delivery report.
- Next safe entry: approved native 3D viewport contract consuming the surface projection. Normal visualization and VXL write
  remain independent contracts.

## 2026-08-27 ASSET-VOX-1E-UI-3D interactive viewport update

- The style workspace now uses native WPF interactive 3D for original/result/geometry-region review. Palette remains 2D;
  SliceStack remains explicit diagnostic and automatic resource-failure fallback.
- The IDE adapter consumes the existing canonical snapshot, surface projector and region colours. WPF geometry is frozen,
  cancellable, generation guarded and session-only; no dependency, second voxel model, parser or writer was introduced.
- Camera supports bounded orbit/pan/zoom/reset and states `X right / Y depth / Z up`. Lighting is explicitly geometry-only,
  not VXL normal-index or game-lighting evidence.
- Evidence: focused 29/29, voxel 48/48, Application 255/255, AssetHost 47/47, Release build 0 errors / one pre-existing
  warning. IDE full 2801/2802 hit the documented WPF Popup teardown flake; isolated rerun passed 1/1.
- Physical 1920x1080 interaction/screenshot remains pending. Multi-part composition, normal visualization, VXL/HVA writer,
  project Apply/Save and game validation remain deferred.

## 2026-08-27 ASSET-VOX-2A refinement baseline

- The user excluded original-model adjustment. The admitted mesh, Tencent artifacts and direct canonical candidate remain
  immutable evidence; no provider call or source-vertex operation was added.
- Application now owns internal deterministic quality facts, six fixed silhouettes, exact-coordinate thin-feature
  protection, bounded 2x conversion/downsampling, one cleanup pass, local-support symmetry suggestion, normal comparison,
  provenance-tagged semantic review regions and palette-only body contrast optimisation.
- IDE owns an internal maximum-three-round DeepSeek diagnosis/plan/review seam. It supports early stop and exact in-memory
  cache hits but is not yet product-wired and was verified only with fake clients.
- Full gates: Application 264/264, IDE 2807/2807, AssetHost 47/47; Debug solution build 0 errors / one pre-existing warning.
  IdeOnly clean source package contains 1361 files. Default binaries were locked by the running IDE, so final verification
  used isolated output directories.
- No exported API, persistence, Shell/XAML, INI/Field Registry, Work/Apply/Save, Host/provider, VXL/HVA or game-validation
  authority changed. Continue from `Docs/ASSET-VOX-2A_StageLedger.md`.

## 2026-08-27 ASSET-VOX-2A-UI contract baseline

- Read-only product audit found that the completed 2A Direct/Refined path requires an admitted GLB mesh, while the current
  Voxel Style workspace admits only VOX or single-Section VXL plus PAL. An existing VOX can support quality analysis and a
  symmetry suggestion, but cannot reconstruct the true supersampled refined candidate.
- The self-reviewed R3 UI contract therefore adds an explicit project-contained GLB quality source beside the existing
  VOX/VXL baseline. Local option derivation reuses baseline identity, palette and longest dimension; source pairing is
  labelled Verified/UserPaired/Mismatch rather than silently asserted.
- Candidate composition reuses canonical snapshots, the existing 2A refiner, style compiler/colourizer, palette contrast
  optimizer and native 3D viewport. Geometry-candidate selection and styled-preview acceptance remain separate in-memory
  actions and neither writes files.
- Implementation is awaiting user approval. Shell/layout, real DeepSeek/Tencent, Application algorithms, project
  Apply/Save, VXL/HVA, public API and persistence remain frozen.
- Authority: `Docs/ASSET-VOX-2A-UI_ReviewCandidateCompositionCodeFactAudit.md`,
  `Docs/ASSET-VOX-2A-UI_ReviewCandidateCompositionFinalContract.md` and
  `Docs/ASSET-VOX-2A-UI_StageLedger.md`.

## 2026-08-27 ASSET-VOX-2A-UI implementation update

- The existing Voxel Style workspace now admits one explicit project-contained GLB as quality evidence beside the loaded
  VOX or single-Section VXL/PAL baseline. Admission is bounded/reparse-safe and source pairing is exposed as
  Verified/UserPaired/Mismatch; candidate generation creates no provider and writes no file.
- Existing 2A algorithms produce immutable Direct, Refined and optional Symmetry snapshots. The ViewModel owns one
  generation/cancellation guard and a separate working-geometry selection; baseline/project reload clears all derived state.
- The existing explicit style compile receives the working snapshot while style-source resolution retains the admitted
  source path. Ordinary Styled output is authoritative, with a separate optional local palette-contrast candidate.
- The existing native 3D viewport displays all geometry and colour candidates. New compact surfaces expose quality metrics,
  normal comparison, semantic provenance and contrast facts without a DataGrid or a second voxel model.
- Full verification: Application 264/264, IDE 2814/2814, AssetHost 47/47; IDE-only solution build 0 errors / one existing
  nullable warning. Real DeepSeek/Tencent was NotRun by contract. Physical 1920x1080 acceptance remains pending.
- Frozen boundaries remained intact: no Shell/layout, Apply/Save/export, VXL/HVA, public API, persistence, INI/Field
  Registry or provider protocol change. Continue from `Docs/ASSET-VOX-2A-UI_StageLedger.md`.

## 2026-08-27 ASSET-VOX-2A connectivity correction

- The original refinement gate incorrectly required every occupied cell to belong to one six-neighbour component. The
  certified user sample showed that one detached voxel among 17,181 cells was the only cause of rejection.
- Candidate connectivity now reuses canonical snapshot facts and requires a dominant component (at least 95% of occupied
  cells) rather than absolute single-component topology. Component count and dominant-body share are projected in UI.
- An isolated product-path probe over `body-candidate.vox + mesh.glb` selected 40% as the default coverage. The normal
  coordinator produces a 17,397-cell, one-component refined candidate and still passes the unchanged 5% volume and 3%
  silhouette gates. Temporary absolute-path probe code was removed after verification.
- Verification passes: focused core 9/9, focused IDE 16/16, Application 267/267, IDE 2814/2814, AssetHost 47/47,
  sequential IDE-only build with 0 errors/one pre-existing warning, and a 1366-file clean package.
- No source model, Shell/XAML, provider, real call, Apply/Save, VXL/HVA, public API, persistence or INI behavior changed.

## 2026-08-27 ASSET-VOX-2A-R2 topology-protected refinement

- Superseded the destructive one-neighbour cleanup and the temporary dominant-body connectivity exception. Sustained rods
  and plates are graph-classified as frozen structures, including degree-one endpoints; occupied neighbours form a
  transition zone, while isolated one-cell bumps remain unprotected noise.
- Conservative and Balanced candidates use deterministic masked discrete-distance filtering. Candidate selection is hard
  gate plus lexicographic/Pareto ordering, not a weighted score. New components, enclosed cavities, missing frozen cells,
  excessive volume/silhouette change, quality regression or absence of measurable improvement reject a candidate.
- `NoSafeImprovement` is a successful review outcome that retains Direct and disables applying an unadmitted Refined
  candidate. The native 3D viewport now supports a union difference projection with added/removed/frozen/unchanged colours.
- Verification: focused core 13/13, focused IDE 11/11, Application 271/271, Debug solution build 0 errors. IDE full passed
  2815 tests and hit the documented WPF Popup teardown flake once; its isolated rerun passed 1/1.
- Authority: `Docs/ASSET-VOX-2A-R2_TopologyProtectedRefinement_FinalContract.md` and
  `Docs/ASSET-VOX-2A-R2_StageLedger.md`. Shell, real providers, Apply/Save, VXL/HVA, persistence, public API and INI remain frozen.

## 2026-08-27 ASSET-VOX-2A-R2 physical review correction 1

- Physical review proved that the first R2 protection grouping was too broad: adjacent thin-looking surface cells could
  merge into one large protection component, while `NoSafeImprovement` still exposed duplicate Refined/Difference views.
- Structure grouping now carries a directional rod/plate signature. Rod traversal follows the major axis; plate traversal
  stays within its thin plane. A separate topology-safe pass removes only locally redundant attachments and fills only
  strongly enclosed cells; endpoints adjacent to low-degree rod neighbours cannot be removed.
- Refined/Difference availability is derived from admitted, non-identical snapshots with a non-zero delta. Direct remains
  available when no safe improvement exists.
- Product-path evidence on the user's actual pair: Conservative admitted; Direct 18,301, Refined 18,267, +30/-64,
  Frozen 126, Transition 50. The temporary absolute-path probe was removed after observation.
- Final gates: core focused 15/15, affected IDE 21/21, Application 273/273, IDE 2816/2816, Debug build 0 errors and one
  unrelated existing nullable warning. No Shell, writer, provider, persistence, public API or INI boundary changed.

## 2026-08-27 ASSET-VOX-2A-R2 physical review correction 2

- The admitted direct-grid Conservative pass still produced scattered salt-and-pepper removals rather than continuous
  surface refinement. It and the unused masked-distance implementation were removed from production selection.
- Conservative/Balanced now share a weighted local surface proposal, per-direction 2x GLB occupancy confirmation and a
  minimum 26-neighbour delta-component size of three/two. Frozen and transition coordinates remain exact anchors.
- Real product-core evidence: Balanced admitted; 18,301 -> 18,286, +34/-49, zero singleton delta components, one body
  component, zero cavities, maximum silhouette delta 1.21%, roughness 1.5526 -> 1.5348 and low-support 76 -> 62.
- After the running IDE was closed, the standard Debug build passed with zero errors / zero warnings; Application passed
  273/273 and the full IDE suite passed 2816/2816. Temporary physical-study code was removed.
- No UI/Shell, provider call, writer, persistence, public API, INI or Field Registry boundary changed.

## 2026-08-27 ASSET-VOX-2B semantic symmetry baseline

- Host evidence now includes a selected half-cell X plane, six compact silhouettes, <=64 stable region IDs, GLB coverage
  summaries and canonical hashes. Prompts contain neither absolute paths nor coordinate arrays.
- The IDE-internal compiler performs exactly two sequential required-tool calls, validates every host region before local
  reconciliation and never retries or weakens malformed results.
- Only confirmed `SymmetricCore` pairs can change. Coverage evidence chooses add/remove; asymmetric attachments, thin
  structures, uncertain cells and their transition seam remain unchanged.
- The existing workspace keeps `生成候选` provider-free and adds explicit `AI 识别结构`, `结构区`, fixed overlay colours
  and a semantic review summary. Results remain session-only and read-only.
- Verification: Application 278/278, IDE 2825/2825, build 0 errors / one pre-existing warning. No real provider call was
  run. Manual 1920x1080 and real-sample/provider acceptance remain pending.
- Authority: `Docs/ASSET-VOX-2B_SemanticSymmetryContinuousFinalContract.md` and
  `Docs/ASSET-VOX-2B_StageLedger.md`.

## 2026-08-28 ASSET-VOX-2B visual/provider correction

- Manual review showed that valid bounded evidence alone did not satisfy product acceptance: Refined remained visually
  subtle, pre-AI frozen cells were misread as blue symmetric regions, and a real DeepSeek tool response failed the exact-
  property parser.
- Candidate selection now includes a bounded SurfacePolish threshold. On the certified local pair it produces 167 actual
  changes and improves low-support surface facts while all topology/volume/silhouette/frozen gates remain authoritative.
- Difference presentation is now geometry-only (added green, removed red, unchanged translucent grey). AI semantic blue
  means protected thin geometry and may be one-sided; cyan alone means confirmed symmetric core.
- Provider JSON representation is tolerant, content identity is not: hash, plane and complete known-region coverage still
  fail closed before `Ra2VoxelSemanticSymmetryExecutor` can edit any coordinate.
- The recognition button reports missing configuration on click instead of becoming an unexplained disabled control.

## 2026-08-28 ASSET-VOX-2B selection/diagnostic self-audit correction

- Self-review rejected cleanup-first automatic selection. A candidate must now materially reduce roughness, then wins by
  roughness before low-support/unmatched/delta counts. SurfacePolish can remain a safe review-only alternative.
- Candidate provenance includes behavior kind, cluster threshold and occupancy threshold. The existing `组合审阅` surface
  lists Conservative/Balanced/SurfacePolish admission state, delta and quality facts.
- The certified local `body-candidate.vox + mesh.glb` pair now selects Conservative: roughness
  `1.552632 -> 1.542869`, low-support `76 -> 70`, `+24/-29`. SurfacePolish (`+14/-153`) is not auto-selected.
- Difference rendering no longer requires an unused protection mask. Semantic parser failures preserve canonical
  duplicate/missing/type/value/JSON/tool-count and evidence/region mismatch reasons through localized status text.
- Focused verification passed: Application 22/22 and affected IDE 35/35; Debug build passed with one existing nullable
  warning. Full suites passed: Application 280/280, IDE 2830/2830 and AssetHost 47/47. IdeOnly clean package contains
  1375 files. No real provider call, Shell, Apply/Save, VXL/HVA, persistence or public API change occurred.

## 2026-08-28 ASSET-VOX-2B physical-sample correction

- The first evidence builder failed the real Body sample because it mapped every disconnected mismatch/protected component
  to one model region and exceeded the unchanged 64-region limit. Quality candidates were valid but AI recognition stayed
  disabled.
- Exact coordinates now remain Host-owned while model-facing mismatch components use deterministic
  side/height/depth/morphology buckets (maximum 48 mismatch / 50 total regions). Protected coordinates are summarized in
  one exact union and every region exposes connected-component count. No cell is truncated or sampled.
- The real `H:\RA2\YR_Test\body-candidate.vox + mesh.glb` path now produces bounded evidence covering all 18,286 Refined
  cells exactly once. Temporary physical-probe code was removed.
- Verification: semantic focused 6/6, affected IDE 27/27, Application 279/279, IDE 2825/2825 and Debug build 0 errors / one
  existing nullable warning. No provider call, Shell/XAML, Apply/Save, VXL/HVA, persistence, public API, INI or Field
  Registry change occurred.

## 2026-08-28 ASSET-VOX-2B neutral repair-evidence correction

- Live output exposed a semantic deadlock: `core` contained only matched pairs and every unmatched region ID asserted an
  attached/detail morphology. DeepSeek protected all actual repair opportunities; the deterministic executor then had no
  asymmetric core pair to process.
- Model-facing unmatched identities are now neutral `repair-*` buckets (`detached/slender/compact/broad`). Exact Host-owned
  coordinate coverage is unchanged.
- Region facts now include source and mirrored-target GLB coverage plus mirrored-target body contact. The prompt explains
  that an absent opposite voxel is the repair question, not ambiguity or attachment evidence by itself.
- Round two verifies supported round-one decisions and must not manufacture disagreement. Local reconciliation still
  requires two decisions at >=0.80; disagreement remains uncertain.
- No-candidate output is localized to explain that no repair region achieved two-round agreement. Direct/Refined remain.
- Verification: Application semantic 6/6; IDE compiler/coordinator 20/20; Application full 280/280; IDE full 2830/2830;
  AssetHost 47/47; isolated solution build 0 errors / one existing nullable warning. No live provider call was made.

## 2026-08-28 ASSET-VOX-3A generation orchestration baseline

- A fixed `Providers/TencentHy3D/provider.bundle.json` and hashed executable bundle are emitted with the IDE build.
- `Ra2MeshGenerationFacade` is the only exported AssetHost surface. It owns returned bytes and never exposes a run lease.
- Voxel Style UI accepts one explicit PNG/JPEG reference, optional provenance brief/negative notes, 32/48/64/96/128
  resolution and 1-20 minute timeout. It probes locally, then requires a one-run consent before generation.
- Successful GLB stays in memory as a `GeneratedSession` source and reuses canonical voxelization, local quality review,
  style inheritance and explicit structure analysis. No project or asset file is written.
- Automated baseline: build passed; AssetHost 50/50, Application 285/285, IDE 2831/2831; clean package 1384 files.
- Live Tencent/DeepSeek and UI smoke were not run by contract. See `Docs/ASSET-VOX-3A_StageLedger.md`.

## 2026-08-28 ASSET-VOX-3B accepted candidate and export baseline

- The Voxel Style workspace now freezes exactly one materializable canonical snapshot as immutable session authority.
  Original, Direct, admitted Refined, successful Agent Geometry, Styled and Contrast Styled are eligible; Difference,
  Structure Regions, Region Mask and Palette are not.
- Review navigation does not alter the frozen snapshot. Source replacement, working-geometry adoption or style recompilation
  invalidates it; another materializable candidate may replace it only through an explicit user action.
- VOX export reuses `Ra2MagicaVoxelCodec`, writes and physically flushes a same-directory temporary file, requires exact
  decode/re-encode bytes, then atomically publishes. The current source VOX is never overwritten by this phase.
- Export is independent of project Apply/Save, manifests, registration, VXL/HVA and Agent/provider authority. No Shell,
  INI, Field Registry, public API or legacy change occurred.
- Verification: focused 23/23, AssetHost 50/50, Application 285/285, IDE 2844/2844, Debug build 0 warnings / 0 errors;
  IdeOnly clean package 1389 files. Physical WPF Save-As smoke remains manual.
- Authority: `Docs/ASSET-VOX-3B_AcceptedCandidateVoxExportFinalContract.md` and
  `Docs/ASSET-VOX-3B_StageLedger.md`.

## 2026-08-29 ASSET-VOX-UI-R1 workspace and camera baseline

- The Voxel Style document is now composed as four left workflow tabs, one dominant adaptive interactive 3D viewport and
  four lower evidence tabs. The inspector and evidence height have bounded splitters; the root no longer uses a whole-page
  two-axis scroller, `Viewbox` or scale transform.
- Existing commands, bindings and AutomationIds remain. New layout/workflow/detail AutomationIds are recorded in
  `Docs/ASSET-VOX-UI-R1_WorkspaceRecompositionAndCameraStabilityFinalContract.md`.
- Viewport camera state is session-only. Compatible scene swaps restore yaw, pitch, normalized target and bounds-relative
  distance. Repeated broad `SourcePath` property notifications do not reset the view; a genuinely new file/generated
  original canonical hash starts a new group and auto-fits once.
- Automated evidence: build passed; camera/workspace 14/14, affected IDE 88/88, affected Application 87/87, AssetHost
  50/50 and Application full 285/285. IDE full recorded 2849/2850 due to one unrelated intermittent WPF ContextMenu
  open-state assertion; the exact failed test immediately passed 1/1 in isolation. IdeOnly clean packaging passed with
  1394 source files.
- No Shell, ViewModel, SceneBuilder, provider, geometry/colour algorithm, Apply/Save, VOX writer semantics, VXL/HVA,
  persistence, public API, INI, Field Registry or legacy behavior changed. Physical 1920×1080 100%/125% review remains.
- Authority: `Docs/ASSET-VOX-UI-R1_WorkspaceRecompositionAndCameraStabilityCodeFactAudit.md`,
  `Docs/ASSET-VOX-UI-R1_WorkspaceRecompositionAndCameraStabilityFinalContract.md` and
  `Docs/ASSET-VOX-UI-R1_StageLedger.md`.
- Manual-launch correction: WPF `Run.Text` has two-way default metadata. All dynamic runs in the workspace now explicitly
  bind `OneWay`, preventing the header and evidence rows from writing into read-only presentation properties.

## 2026-08-29 ASSET-VOX-3C contract baseline

- Read-only audit confirms a geometry-authority discontinuity: quality generation captures the immutable source instead
  of the adopted working snapshot, re-voxelizes the old GLB and explicitly clears `_workingGeometry`; the next Agent pass
  is therefore bound to a newly reconstructed old branch.
- The self-reviewed R4 contract introduces an IDE-internal revisioned working state and an Application-internal
  existing-baseline refinement path. GLB remains coverage/alignment evidence and cannot become the next-pass geometry
  authority after adoption.
- Candidate generation/Agent analysis remain read-only. Only explicit adoption advances working geometry; only explicit
  final-candidate freeze authorizes the unchanged VOX export path. Session lineage is not serialized.
- No implementation has started. User approval is required for 3C-0 through 3C-5. Shell, provider calls, Apply/Save,
  VOX writer changes, VXL/HVA, public API, persistence, INI and Field Registry remain frozen.
- Authority: `Docs/ASSET-VOX-3C_WorkingGeometryContinuityCodeFactAudit.md` and
  `Docs/ASSET-VOX-3C_WorkingGeometryContinuityFinalContract.md`.

## 2026-08-29 ASSET-VOX-3C working-geometry continuity baseline

- 3C-0 through 3C-5 are implemented. The immutable loaded/generated source remains the Original view; one IDE-internal
  `Ra2VoxelWorkingGeometryState` owns the session's current canonical snapshot, origin, revision, root hash and parent hash.
- Local quality generation calls Application-internal `Ra2VoxelQualityRefiner.RefineExisting`. The captured working
  snapshot is returned as Direct/Baseline unchanged, while the unchanged GLB supplies bounded registration and
  supersampled coverage evidence. Existing protection/connectivity/cavity/volume/silhouette/quality gates remain.
- Quality and Agent results carry working hash/revision and deterministic evidence/batch identities. Publication and
  adoption both reject stale identities. An adopted candidate starts the next pass; an old sibling branch cannot be
  adopted, frozen or exported.
- Read-only quality/Agent work preserves valid style and frozen candidates. A real working transition invalidates
  geometry-bound style/frozen state. The exact adopted current candidate can then be frozen and exported through the
  unchanged 3B VOX transaction and round-trip verification.
- The existing `VoxelStyle.Preview.Direct` AutomationId remains; its visible label is now `基线`. No layout, camera or
  Shell change was made.
- Verification: Debug build passed with 0 warnings/0 errors; Application 288/288, IDE 2855/2855 and AssetHost 50/50.
  No live Tencent/DeepSeek call was made. See `Docs/ASSET-VOX-3C_StageLedger.md`.

## 2026-08-29 ASSET-VOX-3D center-seam bridge baseline

- Agent geometry evidence now exposes exact Host-owned `seam-gap-*` targets for one-cell integer-plane and two-cell
  half-plane X-axis gaps with occupied anchors on both sides. Connected patches stay separate within a 24-target bound;
  deterministic overflow grouping retains every coordinate.
- The internal tool contract adds `bridge_center_gap`. It is valid only for seam targets; add/remove remain occupied-target
  operations. The Agent must explicitly select it, and omitted seams remain unchanged.
- Execution reuses the existing added-cell palette resolver and protection/connectivity/cavity/volume/silhouette gates.
  Three-cell, off-axis and arbitrary interior holes are not eligible.
- Automated evidence: focused Application 16/16, focused IDE 9/9, Application 293/293, IDE 2856/2856, Debug build
  0 warnings / 0 errors. Live DeepSeek/Tencent and physical sample review were NotRun.
- No Shell/XAML/AutomationId, Apply/Save, VOX writer, VXL/HVA, public API, persistence, INI, Field Registry or legacy
  change occurred. See `Docs/ASSET-VOX-3D_StageLedger.md`.

## 2026-08-29 ASSET-VOX-4A semantic part/material masking baseline

- Current working geometry now produces a deterministic, snapshot-hash-bound 2×4×3 spatial evidence partition with
  mirror-paired region IDs, bounded masks, bounds, surface ratio and mirror coverage. It is derived session state only.
- DeepSeek remains text-only. `suggest_ra2_voxel_semantics` receives no image/path/asset bytes or palette colours. Primary
  and reviewer run every time; normalized semantic disagreement alone triggers a third arbitrator. No hidden retry exists.
- AI output is a suggestion layer. Human per-region part/material overrides outrank it, default to mirror-linked editing,
  survive re-analysis of the same working hash and are invalidated by a real working-geometry transition. AI cannot approve remap.
- Effective material regions reuse `Ra2VoxelExplicitMask`, existing compiled colour roles and `Ra2VoxelColourizer`; missing
  roles remain unresolved instead of inventing palette indices. Geometry and occupancy remain unchanged.
- The workspace adds explicit analyze/accept/discard controls, a semantic evidence tab and a 3D semantic mode. Clicking a
  visible surface selects the Host region for manual correction. This is review UI, not a persistent semantic editor.
- No public API, persistence, Shell, provider host, geometry algorithm, Apply/Save, VOX/VXL/HVA writer, INI, Field Registry
  or legacy behavior changed. See `Docs/ASSET-VOX-4A_SemanticPartAndMaterialMaskingFinalContract.md` and stage ledger.
- Verification: Application 296/296, IDE 2860/2860, AssetHost 50/50; Debug build passed with 0 errors and one unrelated
  pre-existing nullable warning; IdeOnly clean package contains 1406 source files. Live calls and physical WPF smoke were NotRun.

## 2026-08-29 ASSET-VOX-4B agent-seeded human semantic editing baseline

- A session-only `Ra2VoxelSemanticManualMaskLayer` stores sparse cell overrides against the exact working snapshot hash and
  sorted occupied-cell ordering. A derived composition resolves cell human > region human > accepted Agent > Unknown.
- The existing semantic detail page adds one compact toolbar: browse/paint/erase, brush size 1–3, mirror, undo and redo.
  Brush target part/material/remap is separate from region assignment. Short clicks edit exposed surface cells; orbit, pan,
  zoom and camera continuity are unchanged.
- Mirror edits are atomic, erase reveals the lower seed, and the last 100 brush states are locally undoable/redoable. A real
  working-geometry change clears the overlay; same-hash re-analysis preserves it.
- Final per-cell assignments are grouped into non-overlapping `Ra2VoxelExplicitMask` values and use the existing style plan,
  palette roles, colourizer, review, frozen-candidate and VOX export path. No palette index is authored directly.
- No live provider call, Shell, Apply/Save, geometry algorithm, writer, public API, persistence, INI, Field Registry or legacy
  change occurred. Automated evidence: Application 299/299, IDE 2862/2862, AssetHost 50/50; Debug build 0 errors / one
  pre-existing nullable warning. Physical 100%/125% WPF brush smoke remains for the user.
- Physical-smoke correction: selecting browse/paint/erase now prepares deterministic local semantic evidence automatically
  when absent, without a provider call. A cell brush uses the region hit in 3D and no longer requires a preselected list row;
  leaving semantic preview returns to honest browse state. Focused IDE 28/28 and sequential Debug build passed 0/0.

## 2026-08-30 ASSET-VOX-4B-FIX2 reliable pointer contract gate

- A second physical smoke still found no paint response. Code audit confirms the remaining root is architectural: left down
  captures Orbit while left up conditionally reinterprets the same gesture, and actual triangle identity is discarded in favour
  of an O(N) nearest-cell-centre guess.
- The reviewed R3 contract reserves left click for semantic select/paint/erase, right drag anywhere on the full viewport input
  surface for Orbit, Shift+right/middle for pan and wheel for zoom. Existing reset remains a button action.
- SceneBuilder will retain a scene-lifetime, IDE-internal mapping from its existing exposed quad order and WPF model/triangle
  identity to exact canonical coordinates. It is derived, nonserialized and cannot alter voxel geometry or semantic authority.
- No runtime implementation has started. User approval is required for FIX2-0 through FIX2-4. See
  `Docs/ASSET-VOX-4B-FIX2_ReliablePointerInteractionCodeFactAudit.md` and final contract.

## 2026-08-30 ASSET-VOX-4B-FIX2 reliable pointer baseline

- FIX2-0 through FIX2-4 are implemented. The full viewport `InputSurface` owns pointer routing: left down performs semantic
  selection/paint/erase only, right drag orbits from model or background, Shift+right/middle pans and wheel zooms.
- Every emitted surface quad is paired with its canonical voxel coordinate inside the scene result. WPF hit model identity and
  triangle vertex indices resolve the exact face; missing, cross-face or stale mappings fail readably and never use a nearest-cell fallback.
- Scene model, snapshot, evidence, coordinate index and hit map change with the same scene generation. Clear/dispose/lost capture
  terminate camera gestures. `VoxelStyle.Preview.Viewport3D` and the existing layout remain unchanged.
- Automated evidence: affected IDE tests 35/35 and sequential Debug build 0 warning / 0 error. IdeOnly clean package passed;
  live providers were not called. Physical WPF pointer smoke remains pending. See `Docs/ASSET-VOX-4B-FIX2_StageLedger.md`.
- No Shell, Application geometry/semantic/colour algorithm, Apply/Save, VOX/VXL/HVA writer, public API, persistence, INI,
  Field Registry or legacy change occurred.

## 2026-08-30 ASSET-VOX-4B-STROKE-1 contract gate

- Read-only audit confirms that continuous painting must not repeat the current click handler: each invocation currently
  creates its own layer/history/composition and formal scene refresh.
- The self-reviewed R3 contract defines one cancellable stroke transaction. Viewport owns exact visible-surface seed
  sampling and a temporary path overlay; ViewModel owns base-layer/history lifecycle; the existing Application editor
  becomes the sole deterministic multi-seed executor.
- Left release commits once, one stroke yields at most one undo item and one formal scene rebuild, and capture/scene/hash/
  mode/camera transitions cancel without partial state. Sampling is <=4 DIP with explicit non-truncating resource limits.
- Semantic review gains session-only Part/Material display dimensions. Fixed annotation colours read existing effective
  assignments and never write palette indices or alter composition authority.
- Runtime implementation has not started. User approval is required for STROKE-0 through STROKE-5. Shell, providers,
  Apply/Save, writers, public API, persistence, INI, Field Registry and legacy remain frozen.
- Authority: `Docs/ASSET-VOX-4B-STROKE-1_ContinuousSemanticPaintingCodeFactAudit.md` and
  `Docs/ASSET-VOX-4B-STROKE-1_ContinuousSemanticPaintingFinalContract.md`.

## 2026-08-30 ASSET-VOX-4B-STROKE-1 continuous painting baseline

- STROKE-0 → STROKE-5 are implemented. Paint/Erase owns one cancellable pointer transaction: exact visible-surface
  samples are interpolated at no more than 4 DIP, deduplicated and committed once on left release by the existing
  Application semantic-mask editor. Single-click remains a one-seed stroke.
- The viewport has a separate throttled seed overlay and never mutates the manual layer during drag. A successful stroke
  creates one undo record and one formal semantic publication; stale scene/hash/mode/capture/camera transitions cancel
  without partial state.
- Semantic review now switches between fixed Part and Material annotation palettes with a compact legend. These colours
  are IDE presentation only and never become semantic authority or palette indices.
- Automated evidence: focused Application 9/9, affected IDE 57/57, Application full 302/302, IDE full 2885/2885,
  AssetHost 50/50, Debug build 0 errors (one unrelated existing CS8602 warning). Physical WPF input/DPI review remains manual.
- No real provider, Shell, Apply/Save, VOX/VXL/HVA writer, public API, persistence, INI, Field Registry or legacy change.
  See `Docs/ASSET-VOX-4B-STROKE-1_StageLedger.md`.

## 2026-08-30 — ASSET-VOX-4D Persistent Semantic Mask

- 状态：Completed / automated verified / physical WPF acceptance pending。
- 新增 IDE-internal `Ra2VoxelSemanticSidecarStore`，实现严格 UTF-8、32 MiB 上限、项目内且无 reparse point 的
  `.semantic.json` v1 保存/载入，并复用 Infrastructure `AtomicTextFileWriter`。
- sidecar 分别保存已接受 Agent 区域建议、人工区域覆盖、稀疏人工体素覆盖；不保存几何、色板、RGB、相机、
  undo/redo 或临时笔划。
- 恢复要求 source canonical hash、deterministic evidence package hash、cell count 和重建 manual layer hash 全部
  精确匹配；未知/重复/缺失属性、非法枚举/索引/资源、项目外路径均拒绝。
- UI 在现有“语义”页新增“保存分划/载入分划”和持久化状态；本地破坏性模型替换与 sidecar 载入前提示未保存
  修改。Shell 全局关闭/项目切换保护仍不在本阶段。
- 未修改 Shell、项目 Apply/Save、Provider、public C# API、VOX/VXL/HVA writer 或几何/色板语义。
- 验证：IDE 2892/2892、Application 302/302、AssetHost 50/50；最终 Debug build 0 warning / 0 error；
  IdeOnly clean package 1422 files。
