# RA2IniEditor.IDE — Current Phase

更新时间：2026-08-24
状态类型：CurrentStatus / concise index

## 1. 当前产品目标

已确认的最终目标：用户以自然语言描述 Mod 制作需求，Agent 自动编排 INI、
Cameo/Icon、VOX/VXL、SHP 的创建、预览、绑定和验证。当前安全默认仍是本地
Preview + 显式 Apply/Save；更高自治级别需要后续单独契约。

权威需求：`Docs/ProductVisionAndRequirements.md`  
当前能力：`Docs/CurrentCapabilities.md`  
路线图：`Docs/DevelopmentRoadmap.md`

## 2. 最新可信状态

### Completed / Verified

- IDE-only source-first WPF/AvalonDock IDE。
- Field Registry FR-DQ-4 清退和可信度收口。
- AI-STREAM-0..3、AI-REL-1..3。
- UI-DOCK 布局、恢复与持久化主线。
- SEARCH-1-R1 项目查找 + 当前文件 Preview-first Replace All。
- AGENT-AUTHORING A0、A1、A2、A3、A4 和 A4-R1。
- AUTOMATION-HLI-0A 现有能力审计与矩阵。
- AUTOMATION-HLI-0B 最小能力契约已确认并完成契约阶段。
- AUTOMATION-HLI-1A0 Query 依赖锥特征化与门禁测试（7/7）。
- AUTOMATION-HLI-1A1 Headless Document Query（Application、15-type Experimental API、31/31 + 2526/2526）。
- AUTOMATION-HLI-1A2 Headless Diagnostics（唯一 neutral core、IDE adapter、18-type Experimental API、47/47 + 149/149 + 2526/2526）。
- AUTOMATION-HLI-1B Headless Edit Preview（唯一 semantic engine、IDE thin adapter、29-type Experimental API、82/82 + 88/88 + 390/390 + 2526/2526）。
- AUTOMATION-HLI-1C Host Boundary Confirmation（两处 internal admission/projection guard、
  11 个新契约测试、public API 0 change、82/82 + 53/53 + 2537/2537）。
- AUTOMATION-HLI-2A-0 Capability Gateway 代码事实审计与最终契约：确认当前无生产
  Gateway/descriptor/registry，冻结固定四能力目录与强类型门面。
- AUTOMATION-HLI-2A-1..2A-4 最小 Capability Gateway：固定四能力目录、6 个新增
  Experimental public 类型、allowlist 35、12/12 + 94/94 + 2537/2537。
- AUTOMATION-HLI-2B-0 IDE/AI Gateway Consumer 代码事实审计与最终契约：已冻结唯一
  adapter、public budget、发送前成本门禁和 Host authority。
- AUTOMATION-HLI-2B-1..2B-4 IDE/AI Gateway Consumer：唯一 adapter 已切换 typed Gateway，
  删除 unlimited bypass，descriptor-driven preflight 已在 provider 前生效；public API 0 change，
  94/94 + 78/78 + 2547/2547 + clean package。
- AUTOMATION-HLI-2C-0 First High-Level Agent Loop：代码事实审计与最终契约已完成；确认
  当前缺口是端到端闭环证据和 Apply 后 Problems 刷新，不需要新 public Agent façade。
- AUTOMATION-HLI-2C-1..2C-4 First High-Level Agent Loop：确定性 Gateway 与 provider loopback
  闭环、显式单次 Apply、更新后 Validate 和 Problems 刷新已完成；public API 0 change，
  94/94 + 37/37 + 2549/2549 + clean package 1123。
- AUTOMATION-POST-HLI-0 Semantic / Host Priority Audit：只读代码事实确认 CONTENT-1 应先于
  独立 Agent Host，素材侧继续后置；public API 和生产代码 0 change。
- AUTOMATION-CONTENT-1A Field Schema Query：effective provider/trust typed query；allowlist 40、catalog 5。
- AUTOMATION-CONTENT-1B Reference Resolve：current-document typed resolution，不猜 target kind；allowlist 45、catalog 6。
- AUTOMATION-CONTENT-1C Section Creation Preview：additive Section 创建进入唯一 EditPlan/Preview；
  allowlist 47；Application 125/125，阶段 focused gates 通过。
- AUTOMATION-CONTENT-1D..1E：internal template compiler 与 source-backed Weapon/Projectile/Warhead
  关系骨架模板；allowlist 58、catalog 7、Gateway methods 9。
- AUTOMATION-CONTENT-1F：内置 AI required template tool 已接入 Gateway/Preview/Coordinator；整案显式 Apply，
  不自动 Save，public diff 0。
- AUTOMATION-CONTENT-UI-1：主工作区临时只读 Diff、关闭恢复、Dismiss/Invalidate、Blocked/Stale 和
  有界投影已完成；focused 20/20，视觉人工验收待执行。
- CONTENT-UI-1 VISUAL-FIX1：修复持久化布局恢复后初始 XAML `Document.Source` 引用失效导致的
  Diff 自动打开/“查看更改”静默无响应；改由 layout session 解析当前文档模型；定向 13/13、
  完整 non-UI 2576/2576、clean package 1147 files，视觉复验待执行。
- AI-AUTHORING-NONSTRICT-1：普通 DeepSeek Tool Calls 的有限格式容忍与分类错误已完成；
  聚焦 88/88、完整 non-UI 2576/2576，仍不自动 Apply/Save。
- AGENT-MODE-1：AI 面板显式提供 Chat / Work 模式且默认 Chat；Chat 零编辑工具，Work 才能进入
  结构化 Preview。普通“搭建可用武器链”改走 complete profile；只有明确要求骨架/框架时才走 skeleton。
- AGENT-KNOWLEDGE-1：联网核验 Agent Skills / DeepSeek Harness 与 ModEnc、Ares、Phobos 资料后，
  已内置 15 个只读 RA2 领域 Skill；按领域按需注入，不授予文件、Apply、Save、网络或 Shell 权限。
- Direct-fire complete profile：要求当前文档中唯一存在的 owner，生成非空 Weapon / Projectile /
  Warhead 三段和 15 项原子操作；Field Registry 仍是字段 schema/trust 事实源。
- AGENT-MODE-1-R1：修复 Work 仍要求提示词重复出现“当前文件”且遗漏“构筑”等动词，导致明确武器链
  请求退化为普通对话的问题。Work 现在隐式以当前文档为目标；截图原句已进入 required complete tool。
- AGENT-MODE-1-R2：修复 complete template 的 provider 参数契约过严导致 Work 已调用工具后仍报
  “结构化修改参数格式无效”。工具 schema 现使用命名参数对象和原生 number/boolean；本地 adapter
  仅归一化可唯一解释的省略 outcome、字符串版本、尾逗号及标量值，仍由模板编译器和字段库门禁裁决。
- AGENT-MODE-1-R3：最小真实 DeepSeek 结构探针确认 provider 会在完整 proposal 与 15 参数之外附带
  非空 `message`，而已声明 schema 允许该属性、旧 adapter 却禁止它。adapter 现只验证并丢弃有界
  proposal message；非字符串/超限值继续拒绝，prompt 的 AA/AG 表述与 JSON boolean schema 对齐。
- AGENT-MODE-1-R4：修复 `needs_clarification` 与 proposal 参数共存时被误判为格式错误。明确
  clarification 现在只返回有界 message，混入参数保持惰性且不会产生 Preview/Apply；完整对象请求
  缺省平衡值时改用可预览的保守 RA2 草案值，只有 owner/slot/ID 无法判定时才澄清。
- AGENT-MODE-2：Work 改为两阶段 DeepSeek 编排。第一次只返回经本地严格校验的意图事实包，第二次
  才按 capability allowlist 使用既有结构化工具；Chat 仍为单调用。关键词路由不再对 Work 的
  ambiguous/unsupported 结果提前终止，且任何模型仍无 Apply/Save 权限。Debug build 通过，定向
  86/86、AI/DeepSeek/ContentTemplate 409/409；真实 DeepSeek 双调用验收待用户执行。
- CONTENT-2A：现有 Techno Primary/Secondary 两条完整 direct-fire 链；循环/交替请求本地拒绝。
- CONTENT-2B：现有 Weapon 可绑定独立 Arcing/Homing Projectile 或 YR core Warhead profile；
  弹道族互斥，Ares custom armor 与 Phobos trajectory 不在 v1 范围。
- AGENT-MODE-2 真实验收：用户已确认两阶段 Work 请求能够得到结构化 Preview，真实 provider
  双调用链不再处于 acceptance pending。
- CONTENT-2D-0/1：对象闭包/注册策略已冻结，模板编译器可在当前文档中验证并确定性追加
  显式数字注册项；已有注册幂等，畸形/重复/溢出列表整体失败。新增实现全部 internal，
  public allowlist 59 与现有六个生产 Profile 行为不变。
- GIT-BASELINE-1：截至 CONTENT-2D-0/1 的已验证工作树已固化到本地分支
  `codex/content-2d-baseline`，并以注释标签 `content-2d01-verified` 标记；禁入路径、
  超大文件和凭据门禁通过。未配置或推送远端。

### Implemented / Acceptance Pending

- 部分 UI-MODERN/M4-R2/Visual Fix 自动化门禁已完成，但对应真实 WPF 截图或
  特定硬件视觉验收不能由文档整理任务补记为通过。

### In Progress

- CONTENT-2C 已完成代码事实审计；用户要求暂不考虑 AI 写入，继续冻结。

### Contracted / Not Implemented

- CONTENT-2C 最终契约未制定、未确认；AI Programming Tuple Profiles 未实现。
- CONTENT-2D-2 多文档事务尚未制定最终契约；当前不能原子联动 `rulesmd.ini` 与 `artmd.ini`。
- 独立 Agent/CLI、Job/Event/Artifact、素材/图标/SHP/VXL 流水线和 Runtime Test Host
  均未实现。

## 3. 最新完整实现证据

来源：`Docs/AUTOMATION-CONTENT-2B_StageLedger.md`

```text
Restore: Passed / up-to-date
Debug build: Passed, 0 warnings, 0 errors
Focused IDE compile note: existing test CS8602 warning at BuiltInFieldRegistryPackLoaderTests.cs:1960
Application.Tests: Passed 151/151
CONTENT-2B focused: Passed 16/16 + 28/28
Non-UI tests: Passed 2601/2601
IdeOnly clean package: Passed, 1177 files
Computer control / real DeepSeek / physical DPI visual smoke: NotRun
```

MODE-1-R1 后续窄边界证据：Release focused 41/41、Release IDE non-UI 2583/2583；Debug 验证因用户当前
运行的 IDE/Visual Studio 锁定输出 DLL 而未完成，未关闭用户进程。真实 DeepSeek 复验仍待用户执行。

MODE-1-R2 后续窄边界证据：Release focused 70/70、Release IDE non-UI 2585/2585。未调用真实
DeepSeek，用户需停止当前旧进程、重新构建并启动后复验截图原句。

AGENT-MODE-2 证据：Debug build 0 errors（一个既有 CS8602 test warning）；focused 86/86，
AI/DeepSeek/ContentTemplate 409/409，Application 151/151，IDE non-UI 2610/2610。clean package、
真实 DeepSeek 双调用与 UI smoke 未运行。

MODE-1-R3 后续窄边界证据：两次授权的最小真实 DeepSeek 调用返回 HTTP 200/tool_calls，参数 JSON
可解析并具有 `outcome/template_id/template_version/arguments/message`，其中 message 非空、arguments
完整 15 项；Release focused 167/167、Release IDE non-UI 2587/2587。完整 GUI 复验仍待用户执行。

MODE-1-R4 后续窄边界证据：修正 prompt 后的授权真实 DeepSeek 探针返回 HTTP 200/tool_calls、
`outcome=proposal`、template id/version 和 15 项 arguments；Release focused 71/71、Release IDE
non-UI 2588/2588。完整 GUI 复验仍待用户用新构建执行。

MODE-1-R5 否定意图窄修复：Work 路由不再把“不要使用循环或交替开火”误判为正向循环开火需求；
真正的循环/交替请求仍 fail closed。Debug build 0 errors；CONTENT template focused 29/29，截图原始
提示词程序集复核进入 `CurrentDocumentCompleteTemplatePreview`。

CONTENT-2D-0/1 证据：Debug build 0 warnings/0 errors；Compiler/Template focused 37/37；
Application 162/162；IDE Agent/Template focused 106/106；IDE non-UI 2610/2610。新增注册模型、
目录、allocator 与 failure kinds 全部 internal，现有生产 Template/Gateway/Apply/Save 零行为变化。

GIT-BASELINE-1 复用上述同一工作树验证；版本控制专项检查确认无删除、无超过 5 MiB 的候选文件、
无敏感扩展名、无软链接、无已跟踪构建产物/压缩包，唯一凭据签名命中为 loopback 测试占位值。
阶段证据见 `Docs/GIT-BASELINE-1_StageLedger.md`。

最新静态证据：Application exported allowlist 精确为 59；Gateway catalog 7/methods 9；
15 个 BuiltIn Skill 均通过生产 loader 与测试。Template 与 Diff 继续复用现有
Preview/Coordinator/Transaction，legacy 和 Save authority 零变化。

## 4. 当前关键边界

- A4-R1 可对明确的当前文件字段编辑请求形成真实本地提案并经用户 Apply。
- Apply 只改当前内存会话并形成一个 Undo 单元；成功后刷新当前文件 Problems；不会自动保存。
- Custom endpoint 只能 advisory；官方 endpoint 才可进入 required authoring tool。
- 当前能力包含关系骨架、single/dual direct-fire、Arcing/Homing Projectile 与 YR core Warhead profiles；
  这仍不等于 Ares custom armor、Phobos trajectory、任意对象、多文件注册维护、素材生成或无人值守写入。
- 自动重试、模型 fallback、深色主题、项目级替换继续后置。
- Legacy 不得恢复。

## 5. 当前风险与债务

| ID/Area | 状态 | 影响 |
|---|---|---|
| HLI-TD-001 | Repaid through HLI-2B | Section/Reference/Diagnostics/Preview 唯一权威与 typed Gateway 均在 Application；IDE consumer 已接入 |
| HLI-TD-002 | Repaid | diagnostic core 已 neutral；IDE 只保留单向 ViewModel adapter |
| HLI-2B budget transition | Closed / verified | 已统一为 public 8 MiB/10k/128，并在 provider 调用前 fail closed |
| AGENT-AUTHORING-A1-TD-001 | Open / controlled | SemanticModel 可能重复构建，只影响潜在性能 |
| SEARCH-UIA-001 | Open | AvalonDock 浮动 child-HWND 阻止外部 UIA 穿透 Search 内容 |
| Mixed-DPI visual coverage | Manual | 特定多屏硬件状态未由自动化覆盖 |
| Real project Field Registry delta | Unknown | 仓库无真实 `.ini` corpus，无法统计实际 Unknown Key 增量 |
| CONTENT-UI-1 physical visual acceptance | Manual | 自动契约覆盖 900/640 DIP 和资源边界；真实屏幕/混合 DPI 尚待用户验收 |
| Complete profile breadth | Controlled | 已覆盖 direct-fire、双槽、原版 Arcing/Homing 与 YR core Warhead；AI/SuperWeapon/注册列表/素材仍无完整 profile |
| Registration catalog duplication | Open / controlled | 2D-1 internal registry-kind catalog 与 classifier private 目录同源；新增注册家族前需通过独立复用契约收口 |
| BuiltIn Skill source drift | Controlled | v1 为仓库内置只读 Markdown；无在线热更新，需阶段化复核来源与版本 |
| Chat/Work physical visual acceptance | Manual | 自动 XAML/行为测试通过；真实 WPF 尺寸、键盘和模式切换尚待人工验收 |

## 6. 下一安全入口

当前停止点是：

```text
CONTENT-2D-0/1 Object Closure + Current-Document Registration completed
```

CONTENT-2D-0/1 已完成 internal typed registration 基础，但没有生产 Profile 启用它。
下一阶段是 `CONTENT-2D-2 Project Multi-Document Transaction` 的代码事实审计与最终契约：
冻结项目 Snapshot currency、按文件 Diff、原子 Apply/rollback 与 compound Undo 后，才允许
rules/art binding、完整 Techno 和 SuperWeapon Profile。CONTENT-2C AI 写入继续冻结；独立 HOST-1、
自动 Save、Job/Event/Artifact 和素材实现不得无契约进入。

## 7. 最小继续阅读集

1. `AGENTS.md`
2. `Docs/README.md`
3. `Docs/ProductVisionAndRequirements.md`
4. `Docs/CurrentCapabilities.md`
5. 本文件
6. `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`
7. `Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md`
8. `Docs/AUTOMATION-HLI-1A0_DependencyConeCharacterizationContract.md`
9. `Docs/PublicApiLedger.md`
10. `Docs/AUTOMATION-HLI-1A1_DocumentQuerySliceFinalContract.md`
11. `Docs/AUTOMATION-HLI-1A1_StageLedger.md`
12. `Docs/AUTOMATION-HLI-1A2_DiagnosticsCodeFactAudit.md`
13. `Docs/AUTOMATION-HLI-1A2_HeadlessDiagnosticsFinalContract.md`
14. `Docs/AUTOMATION-HLI-1A2_StageLedger.md`
15. `Docs/AUTOMATION-HLI-1B_EditPreviewCodeFactAudit.md`
16. `Docs/AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md`
17. `Docs/AUTOMATION-HLI-1B_StageLedger.md`
18. `Docs/AUTOMATION-HLI-1C_HostBoundaryCodeFactAudit.md`
19. `Docs/AUTOMATION-HLI-1C_HostBoundaryFinalContract.md`
20. `Docs/AUTOMATION-HLI-1C_StageLedger.md`
21. `Docs/AUTOMATION-HLI-2A_CapabilityGatewayCodeFactAudit.md`
22. `Docs/AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md`
23. `Docs/AUTOMATION-HLI-2A_StageLedger.md`
24. `Docs/AUTOMATION-HLI-2B_GatewayConsumerCodeFactAudit.md`
25. `Docs/AUTOMATION-HLI-2B_GatewayConsumerFinalContract.md`
26. `Docs/AUTOMATION-HLI-2B_StageLedger.md`
27. `Docs/AUTOMATION-HLI-2C_FirstAgentLoopCodeFactAudit.md`
28. `Docs/AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md`
29. `Docs/AUTOMATION-HLI-2C_StageLedger.md`
30. `Docs/AUTOMATION-POST-HLI-0_SemanticHostPriorityCodeFactAudit.md`
31. `Docs/AUTOMATION-CONTENT-1_SemanticTemplateContinuousFinalContract.md`
32. `Docs/AUTOMATION-CONTENT-2A_TechnoCompleteProfileCodeFactAudit.md`
33. `Docs/AUTOMATION-CONTENT-2A_TechnoCompleteProfileFinalContract.md`
34. `Docs/AUTOMATION-CONTENT-2A_StageLedger.md`
35. `Docs/AUTOMATION-CONTENT-2B_ProjectileWarheadProfilesCodeFactAudit.md`
36. `Docs/AUTOMATION-CONTENT-2B_ProjectileWarheadProfilesFinalContract.md`
37. `Docs/AUTOMATION-CONTENT-2B_StageLedger.md`
38. `Docs/AUTOMATION-CONTENT-2C_AiProgrammingTupleProfilesCodeFactAudit.md`
39. `Docs/AUTOMATION-CONTENT-2D01_ObjectClosureRegistrationFinalContract.md`
40. `Docs/AUTOMATION-CONTENT-2D01_StageLedger.md`

当前阶段：CONTENT-2D-0/1 已完成。现有生产 Profile 保持不变；internal Template Definition
现可声明显式数字注册并通过当前 Snapshot 确定性分配索引。Application 162/162、IDE 2610/2610；
CONTENT-2C AI 写入继续冻结，下一入口是 CONTENT-2D-2 多文档事务契约。

旧累积状态已保存在：

- `Docs/Archive/Codex_CurrentPhase_Accumulated_Through_2026-08-22.md`
- `Docs/Archive/RA2IniEditor_IDE_Full_Codex_Context_Accumulated_Through_2026-08-22.md`
