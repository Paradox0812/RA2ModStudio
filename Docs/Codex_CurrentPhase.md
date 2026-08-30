# RA2IniEditor.IDE — Current Phase

更新时间：2026-08-30
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

- ASSET-VOX-4D Persistent Semantic Mask：现有体素语义工作区可显式保存/载入项目内
  `<模型文件名>.semantic.json`。v1 分别保留已接受 Agent 建议、人工区域覆盖和稀疏人工体素覆盖，并以
  working snapshot、deterministic evidence、manual layer 三组哈希阻止错模恢复；载入先在临时状态完整校验，
  成功后一次性替换并清除旧 undo/redo，失败保持原状态。无自动保存/载入，不接入项目 Apply/Save，且不写入
  VOX/VXL/HVA。全量验证：IDE 2892/2892、Application 302/302、AssetHost 50/50；最终 Debug build
  0 warning / 0 error；IdeOnly clean package 1422 files。物理 WPF Save/Open 与确认框烟测待用户完成。

- ASSET-VOX-4B Agent-Seeded Human Semantic Editing：4A 的 Agent/区域语义底稿上新增 working-hash-bound 稀疏
  体素人工覆盖。现有 3D 视图提供浏览/画笔/擦除、大小 1–3、镜像原子编辑和 100 项局部撤销/重做；画笔目标
  独立于区域下拉框，避免为修一个局部先污染整区。最终优先级为体素人工 > 区域人工 > 已接受 AI > Unknown，
  并继续走显式 mask、style plan 和 palette-safe colourizer；几何/占用不变。真实 provider 未调用，物理 WPF
  视觉验收待用户重启执行。自动验证：Application 299/299、IDE 2862/2862、AssetHost 50/50；Debug build
  0 error / 1 个既有 nullable warning。

- ASSET-VOX-4A Semantic Part & Material Masking：当前 working geometry 生成哈希绑定的成对空间区域；
  DeepSeek 只消费文本化几何事实，默认两轮、分歧时第三轮仲裁。AI 建议需显式接受，人工覆盖优先且默认
  镜像联动；只有人工可批准阵营色。有效材质通过现有显式掩码和 colourizer 着色，几何/占用不变；3D
  语义预览可单击选择区域。真实 provider 未调用，物理 WPF 视觉验收待用户重启执行。
  自动验证：Application 296/296、IDE 2860/2860、AssetHost 50/50；Debug build 0 error / 1 个既有 nullable
  warning；IdeOnly clean package 1406 files。

- ASSET-VOX-3B Accepted Candidate & VOX Export：原始、Direct、已准入 Refined、Agent 几何、普通/对比度着色
  可显式固化为唯一不可变会话候选，并通过 canonical codec 同目录临时写入、回读字节一致和原子发布导出
  VOX 副本。当前源不可覆盖；Difference/结构区/Mask/Palette 不能导出。定向 23/23、AssetHost 50/50、
  Application 285/285、IDE 2844/2844、Debug build 0 warning/0 error、IdeOnly clean package 1389 files；人工
  WPF Save-As smoke 待执行。

- ASSET-VOX-2C Agent-Led Geometry Proposal：显式结构识别现在由 Agent 返回稀疏 `add_mirror/remove_source`
  操作，Host 只展开已知目标并执行最低几何安全线。主分析可补查一次有界证据，审阅与主分析的可执行
  操作不一致时才调用第三轮仲裁；通常 2 次、分歧 3 次、含补查绝对上限 4 次。结构区只标记实际选中的
  子组件，“对称”视图显示相对 Refined 的真实增删差异。聚焦 Application 11/11、受影响 IDE 32/32、
  Application 285/285、IDE 2830/2830、AssetHost 47/47 通过；Debug build 0 error / 1 个既有 nullable warning，
  IdeOnly 干净包 1379 文件。真实调用和人工视觉验收未执行。

- ASSET-VOX-2B selection/diagnostic self-audit：自动 Refined 候选必须产生可测粗糙度改善，候选按粗糙度而非
  删除量优先；三种候选事实进入现有组合审阅。真实本地样本选择 Conservative（粗糙度
  1.552632→1.542869，+24/-29），SurfacePolish 仅供审阅。Difference 去除未使用的保护掩码依赖；AI
  结构响应错误保留具体工具/JSON/字段/证据/区域原因。focused Application 22/22、IDE 35/35、Debug build
  通过；全量 Application 280/280、IDE 2830/2830、AssetHost 47/47，IdeOnly 干净包 1375 文件；真实
  DeepSeek 和人工截图仍待单独验收。

- ASSET-VOX-1E-UI-R2-FIX1：修复真实 DeepSeek 提案因角色色源二选一契约未写入提示词而落入通用
  `A style colour role is invalid.` 的问题。索引与 RGB 经权威色盘解析为同一格时可无损收敛；缺失、冲突、
  重复或非法角色分别报告具体原因。VOX 仍使用自带色盘且不要求 PAL；focused 20/20、Application
  249/249、IDE 2799/2799、Release build 通过。真实模型复验待用户重启后执行。
- ASSET-VOX-1E-UI-R2：体素风格工作区已统一接收项目内单模型 VOX 或“单 Section VXL + 显式 PAL”；
  两条路径复用 Stage 1B 解码器并汇入同一不可变快照。普通上色不再要求阵营色；仅文本推断且不可执行的
  remap 意图会降级为未决说明，显式/可执行 remap 在无色段时仍安全失败。focused 17/17 + 4/4、
  Application 249/249、IDE 2796/2796、Release build 0 warning / 0 error；真实 DeepSeek 与人工视觉未运行。
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
- DIFF-REVIEW-1：主工作区审阅默认显示完整精确 CandidateText，保留 unified Changes，并以深度一、
  64 项上限显示同/跨项目文档的直接关联 Section；文档页签、大纲、循环导航和窄宽响应已接入。
  Apply/Dismiss/Undo/Save 权威不变；focused 19/19、Application 198/198、IDE 2779/2779，物理视觉待验收。
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
- CONTENT-2D-2：Project Snapshot/Plan/Preview、唯一 IDE project session store、原子内存
  Apply/rollback、compound Undo/Redo 和多文件 Diff 已完成；allowlist 63、catalog 8、Gateway
  methods 10。Debug 0/0、Application 167/167、IDE non-UI 2626/2626。
- CONTENT-2D-3 / ASSET-MANIFEST-1：新增首个 `techno-rules-art-asset-binding` 项目模板，
  精确生成 rules/art 两份叶计划和不可变资产需求 Manifest；body SHP binding 已 Proposed，
  Cameo 因 Art schema 缺口保持 PendingSchema。allowlist 69、catalog 9、methods 11；
  Application 176/176、IDE non-UI 2626/2626。
- FIELD-REGISTRY-ART-1 / ASSET-PROVIDER-1：YR source-backed `Cameo/AltCameo/Voxel/Remapable`
  已进入 ArtObject schema；现有 rules/art Project Plan 同时写入 art `Image` 与 `Cameo`，两项
  Manifest binding 均为 Proposed。新增可外部实现的 Existing-Asset Provider protocol，显式输入可
  转为带 SHA-256 的 Manifest-closed 内存 Artifact；allowlist 77，Gateway catalog 9/methods 11 不变。

### Implemented / Acceptance Pending

- 部分 UI-MODERN/M4-R2/Visual Fix 自动化门禁已完成，但对应真实 WPF 截图或
  特定硬件视觉验收不能由文档整理任务补记为通过。

### In Progress

- CONTENT-2C 已完成代码事实审计；用户要求暂不考虑 AI 写入，继续冻结。

### Contracted / Not Implemented

- CONTENT-2C 最终契约未制定、未确认；AI Programming Tuple Profiles 未实现。
- CONTENT-PROJECT-UI-1 已完成：Work 可选择唯一 rules/art pair，生成项目 Proposal、两文件 Project Diff，
  并经显式 `应用到项目` 原子更新内存 session；一个 Ctrl+Z/Ctrl+Y 可撤销/重做两文件事务。
- CONTENT-PROJECT-UI-1 NF2：NF1 单值兼容已被根因修复取代。真实第一阶段证明 DeepSeek 的完整
  tool call 可返回 capability=`techno-rules-art-binding`、domain=`techno`；本地现以精确 capability
  为权威，并把该能力的派生 domain/completion 归一化为 `art-animation + Field`。未知 schema 值仍
  fail closed；第二阶段真实五参数 proposal 已验证。Debug build 0 errors，Project/Pipeline 39/39、
  `Ra2Ai*` 389/389、Application 186/186、IDE non-UI 2645/2645。
- CONTENT-PROJECT-UI-1 NF3：确认最新真实第一阶段 tool call 已严格通过 parser；持续出现的通用
  “DeepSeek 无法解析”实际由本地 Project availability 被错误包装为 ProtocolError 导致。现以 internal
  `LocalRejection` 区分本地拒绝和 provider/transport failure，并显示项目未打开、pair 缺失/重复、
  snapshot 不可用、只读或资源超限等具体原因；不扩大扫描、文件创建、Preview/Apply/Save 权限。
  Debug build 0 errors，focused 82/82、`Ra2Ai*` 395/395、Application 186/186、IDE non-UI
  2651/2651，IdeOnly clean package 1209 files。
- CONTENT-PROJECT-UI-1 NF4：项目成员权威已从 `ProjectExplorer.Items` UI 投影迁移到成功打开时的
  不可变 `ProjectOpenResult.Files` / session `MemberFilePaths`。Work 现在显示完整项目根路径和唯一
  rules/art pair 就绪状态；空 `artmd.ini` 与同目录其他 INI 不阻塞配对，用户也无需在提示词中重复
  文件名、Preview/Save/asset 限制。Debug build 通过，聚焦回归 88/88；完整验证见阶段 Ledger。
- CONTENT-PROJECT-UI-1 NF5：确认真实 DeepSeek project tool 参数正确，失败来自 Global 用户字段包中
  `Vehicle.Image` 的旧观测 Enum 被模板误当成封闭集合。Application 模板现仅对 source-backed
  rules/art `Image/Cameo` 使用开放引用策略，仍保留字段存在、trust/Blocked 与安全 identifier 门禁；
  project tool 只要求四个 ID，brief 由 Host 生成，并收口大小写、空 brief 与单个 `.shp` 后缀漂移。
  未修改字段库或 provider priority。Application 188/188、IDE non-UI 2656/2656 通过。
- CONTENT-PROJECT-UI-1 NF6：按用户明确裁决将生产 Work rules/art 路由改为 DeepSeek 主导的通用
  `preview_ini_project_edit_plan`。字段库、旧 Enum、SectionKind 与新增 diagnostics 只作 advisory，
  不再否决模型项目计划；Host 仅保留当前 rules/art snapshot、结构/资源、安全 identifier、canonical
  Preview、显式 single-use Apply、stale 与原子回滚门禁。固定 project template 仅保留 headless 兼容。
  Application 188/188、IDE 2659/2659 通过；新工具真实 DeepSeek/WPF 流程待人工复验。
- AGENT-KNOWLEDGE-1-R2：新增第 16 个 `ra2-rules-art-binding` Skill，依据实际 INI、ModEnc 逆向条目和
  Ares/Phobos 官方文档冻结 Techno rules/art 跨文档图。Project capability 现在显式选择该 Skill，
  不再依赖归一化 domain；`Art/Body/Cameo` 作为语义角色而非 rules 键，vanilla 与 Phobos
  `ArtImageSwap` 差异会进入模型决策或 clarification。Debug build 0 errors、focused 84/84、
  Application 188/188、IDE 2660/2660；详见 source audit。
- AGENT-SKILL-ROUTING-2：Work 第一轮读取当前 BuiltIn Skill 紧凑元数据 Manifest，并返回最多 6 个
  有序 Skill 推荐与 6 个知识缺口。Host 使用同一 Catalog 快照补齐 capability 必选 Skill、field trust，
  校验模式与 14 KiB 正文预算，再将显式 resolution 和完整正文交给第二轮；正常 Work 仍两次调用、Chat
  仍一次。Focused 67/67、Application 188/188、IDE 2668/2668，真实 DeepSeek 未运行。
- AGENT-REPAIR-1：Chat 保持一次调用、正常 Work 保持两次；只有第二阶段的类型化白名单结构失败可追加
  一次非流式修复，Work 硬上限三次。修复复用冻结的 intent/route/Skill/project/HLI facts，不重跑分析、
  Skill 或查询；transport/timeout/cancel/stale/resource/safety 失败不修复，仍须显式 Apply 且不 Save。
  Debug build 0 warnings/0 errors、focused 125/125、Application 188/188、IDE 2706/2706；真实 DeepSeek 未运行。
- 独立 Agent/CLI、Job/Event/Artifact、素材/图标/SHP/VXL 流水线和 Runtime Test Host
  均未实现。

## 3. 最新完整实现证据

来源：`Docs/AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_StageLedger.md`

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

CONTENT-2D-3 当期静态证据：16 个 BuiltIn Skill 均通过生产 loader 与测试。CONTENT-2D-3 / ASSET-MANIFEST-1
Debug build 0 errors（1 个既有 test CS8602 warning）；Application 176/176；IDE non-UI
2626/2626；Application exported allowlist 69、Gateway catalog 9/methods 11。Project template
已在 production BuiltIn Image schema 上通过；既有 Apply/Undo/Redo
写盘计数为 0；成功结果含 affected/work/dirty counts，项目 Problems 使用内存态文档刷新；
IdeOnly clean package 1200 files；`ShellWindow.xaml`、布局和 AutomationId 未修改。

FIELD-REGISTRY-ART-1 / ASSET-PROVIDER-1 最终证据：restore up-to-date；Debug build 0 errors
（最终复跑含 1 个既有 test CS8602 warning）；Application 186/186；IDE non-UI 2634/2634；
FRA/AP focused 24/24、field/Core focused 731/731；IdeOnly clean package 1206 files。

## 4. 当前关键边界

- A4-R1 可对明确的当前文件字段编辑请求形成真实本地提案并经用户 Apply。
- Apply 只改当前内存会话并形成一个 Undo 单元；成功后刷新当前文件 Problems；不会自动保存。
- Custom endpoint 只能 advisory；官方 endpoint 才可进入 required authoring tool。
- 当前能力包含关系骨架、single/dual direct-fire、Arcing/Homing Projectile 与 YR core Warhead profiles；
  这仍不等于 Ares custom armor、Phobos trajectory、任意对象、多文件注册维护、素材生成或无人值守写入。
- 通用传输重试、模型 fallback、深色主题、独立 Agent Host 和项目级 Save All 继续后置；仅已批准的
  AGENT-REPAIR-1 单次结构化重规划例外可用。
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
| Complete profile breadth | Controlled | 已覆盖 direct-fire、双槽、原版 Arcing/Homing、YR core Warhead，以及 Ares UnitDelivery/GenericWarhead；其它 SuperWeapon、AI/Faction 与素材仍非 typed complete |
| Registration catalog duplication | Open / controlled | 2D-1 internal registry-kind catalog 与 classifier private 目录同源；新增注册家族前需通过独立复用契约收口 |
| BuiltIn Skill source drift | Controlled | v1 为仓库内置只读 Markdown；无在线热更新，需阶段化复核来源与版本 |
| Chat/Work physical visual acceptance | Manual | 自动 XAML/行为测试通过；真实 WPF 尺寸、键盘和模式切换尚待人工验收 |
| ASSET-MANIFEST-1-D001 | Repaid | Art `Cameo/Voxel/Remapable` 已具 source-backed schema；Cameo binding 已 Proposed |
| ASSET-PROVIDER-1-D001 | Open / explicit | v1 只验证 identity/extension/hash，不解析 SHP/VXL/HVA 格式、尺寸或调色板 |
| ASSET-PROVIDER-1-D002 | Open / explicit | Artifact 仍是内存产物；项目路径、冲突、原子持久化与回滚属于后续 ASSET-HOST-1 |

## 6. 下一安全入口

当前停止点是：

```text
CONTENT-2E completed / automated verified
```

Work 已能在唯一 rules/rulesmd 项目目标上生成 Ares UnitDelivery 与 GenericWarhead 两个 typed complete
SuperWeapon Proposal；其它明确类型进入 model-owned generic Project Plan。两轨复用 Project
Snapshot/Preview/Diff/Apply/compound Undo，不要求 art/素材、不调用 Asset Provider、不自动 Save。
Application 196/196、IDE 2722/2722 已通过；真实 DeepSeek、WPF 与游戏内行为待人工验收。
CONTENT-2C AI 写入继续按用户要求冻结。下一安全入口是先做 CONTENT-2E 人工验收，再决定扩展下一批
source-backed SuperWeapon profile 或转向自动化游戏测试 Host 审计。

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
41. `Docs/AUTOMATION-CONTENT-2D2_ProjectMultiDocumentTransactionFinalContract.md`
42. `Docs/AUTOMATION-CONTENT-2D3_ASSET-MANIFEST-1_ContinuousFinalContract.md`
43. `Docs/AUTOMATION-CONTENT-2D3_ASSET-MANIFEST-1_StageLedger.md`
44. `Docs/AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_ContinuousFinalContract.md`
45. `Docs/AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_StageLedger.md`
46. `Docs/AUTOMATION-CONTENT-PROJECT-UI-1_WorkProjectProposalEndToEndFinalContract.md`
47. `Docs/AUTOMATION-CONTENT-PROJECT-UI-1_StageLedger.md`
48. `Docs/AGENT-SKILL-ROUTING-2_ModelSelectedSkillManifestContinuousFinalContract.md`
49. `Docs/AGENT-SKILL-ROUTING-2_StageLedger.md`
50. `Docs/AGENT-REPAIR-1_BoundedStructuredReplanFinalContract.md`
51. `Docs/AGENT-REPAIR-1_StageLedger.md`
52. `Docs/AUTOMATION-CONTENT-2E_SourceCapabilityMatrix.md`
53. `Docs/AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerContinuousFinalContract.md`
54. `Docs/AUTOMATION-CONTENT-2E_StageLedger.md`

当前阶段：AGENT-QUERY-2 已完成实现和 AI 范围自动门禁。Chat 固定一次调用；Work 正常为意图、零至两轮
语义补查和执行，共 2..4 次；只有 typed allowlist 中模型可修正的执行失败可追加一次 repair，绝对上限
五次。不会切换模型/provider、重试 transport、自动 retarget/Apply/Save。TRACE-1 只读检索摘要与
Search 启动惰性浮窗修复已实现；真实 DeepSeek、物理 UI 间距和启动无闪现仍待人工验收。

旧累积状态已保存在：

- `Docs/Archive/Codex_CurrentPhase_Accumulated_Through_2026-08-22.md`
- `Docs/Archive/RA2IniEditor_IDE_Full_Codex_Context_Accumulated_Through_2026-08-22.md`

## 2026-08-25 SHELL-LAUNCH-1 current stop point

- Windows 以单个既存 `.ini` 裸路径启动 IDE 时，以直接父目录作为项目根，并在 Shell 初始布局完成后复用规范项目打开链路精确载入该文件。
- 目标进入现有 project session store、字段高亮、Project Explorer 选择和 editable session；不直接读取、不自动保存、不调用 AI。
- 原 `--automation-open-folder` 保持兼容；无参数仍是普通空 Shell；非法参数只在 Output/status 报错。
- 未实现单实例/mutex/IPC 转发；每次 Explorer 打开目前仍创建新进程。该范围延期到候选 `SHELL-LAUNCH-2`。
- 权威文档：`SHELL-LAUNCH-1_FileAssociationLaunchFinalContract.md` 与 `SHELL-LAUNCH-1_StageLedger.md`。
- 自动证据：focused 10/10、Application 188/188、IDE 2715/2715、Debug build 0 errors/1 existing nullable warning、IdeOnly clean package 1232 files；Explorer 双击待手工验收。

## 2026-08-25 CONTENT-2E completed stop point

- `2E-0 -> 2E-5` 已连续完成：两个 source-backed Ares typed profiles（UnitDelivery、GenericWarhead）
  与 model-owned generic fallback 均已接入 Work Project Proposal。
- typed profile 生成注册、provider/AlwaysGranted、公共字段和效果引用闭包；generic plan 不受陈旧
  Field Registry Enum 否决，但仍受项目成员、标识符、资源、Preview、stale 与显式 Apply 门禁。
- 唯一 rules/rulesmd 即可捕获项目上下文；匹配 art 可选。经用户批准，Shell 只修改项目快照捕获 wiring，
  XAML、布局与 AutomationId 均未改变。
- 自动证据：focused 8/8 + 14/14、Application 196/196、IDE 2722/2722、Debug build 0/0、
  IdeOnly clean package 1241 files；真实 DeepSeek/WPF/游戏内行为未验证。
- 权威文档：`AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerCodeFactAudit.md`、
  `AUTOMATION-CONTENT-2E_SourceCapabilityMatrix.md`、`AUTOMATION-CONTENT-2E_SuperWeaponSupportPowerContinuousFinalContract.md`
  与 `AUTOMATION-CONTENT-2E_StageLedger.md`。

## 2026-08-25 CONTENT-2E-FIX1 current stop point

- 修复 typed/generic SuperWeapon 项目路由在 proposal 阶段错误选用 CurrentDocument Context 的问题。
- 项目作用域现由 `Ra2AiAuthoringToolCatalog.UsesProjectContext` 统一判定，并同时供 PromptBuilder 与
  bounded structured replan coordinator 使用；adapter 的项目工具隔离保持不变。
- 新增项目工具/作用域一致性和三个 SuperWeapon 模式选择 Project Context 的回归测试。
- Release focused 19/19、IDE full 2726/2726、Release build 0/0、IdeOnly package 1241 files；
  Debug focused 被正在运行的 IDE/Visual Studio 锁定，未终止用户进程。
- 无 XAML、Shell、parser、Field Registry、Diagnostics、Completion、Apply/Save 或 public API 变化。

## 2026-08-25 CONTENT-2E-FIX2 current stop point

- 修复自然语言 UnitDelivery 请求在 intent 元数据一致性和既有对象身份解析上的两段失败。
- 三个 SuperWeapon 项目能力统一规范为 `superweapon + Complete`；第一轮被要求推断并查询 canonical Section
  候选，第二轮显示别名仅能按捕获 rules 中精确且唯一的 Section/`Name`/`UIName` 解析。
- 没有硬编码游戏对象、模糊匹配、第二 parser/index、自动 repair 扩容或 Apply/Save 权限变化；多义和缺失
  身份继续拒绝。
- Application focused 10/10、IDE related 61/61、SuperWeapon integration 18/18、Application 198/198、
  IDE 2733/2733、Release build 0 errors / 1 个既有 nullable warning、IdeOnly clean package 1241 files；
  真实 DeepSeek/WPF/游戏内行为待人工验收。
- 无 XAML、Shell、Field Registry、Diagnostics、Completion、Save、public API 或素材行为变化。

## 2026-08-25 AGENT-QUERY-2 current stop point

- QUERY-2A/2B、ENTITY-1、CONTEXT-4、EVAL-1 已完成：Work 可在捕获项目中按 Section ID、`Name`、
  `UIName` 搜索对象，并最多进行两轮紧凑补查后进入既有结构化 Project Preview。
- 唯一高置信结果形成规范实体绑定；同分多义不自动选择。SuperWeapon 路径补充
  `[SuperWeaponTypes]` 与已绑定对象 Section 事实，不硬编码游戏对象。
- 正常 Work provider 调用为 2..4 次；加既有一次 structured repair 时绝对上限 5 次。重复查询立即停止，
  transport 不重试，不切换 provider/model。
- 项目执行 prompt 不再携带无关的光标选区、附近文本、字段证据和诊断；完整 Skill 仅进入执行轮。
- AI-scope Release regression 466/466；Release build 1 existing nullable test warning/0 errors；Application 198/198；IDE 2740/2740；
  IdeOnly clean package 1245 files。
- TRACE-1 后台事实由后续已批准实现消费；阶段结果见
  `AGENT-TRACE-1_CompactRetrievalSummaryUiContract.md` 与 `AGENT-TRACE-1_StageLedger.md`。

## 2026-08-25 AGENT-TRACE-1 / UI-DOCK-SEARCH-STARTUP-FIX1 current stop point

- Work 在实际执行过项目语义检索时显示一行只读摘要：检索轮次、规范实体数、成功 Host 事实数和停止状态；
  Chat、无检索活动、ProviderFailure 与 NeedsClarification 不显示该行。
- 摘要复用现有 AI metadata 字体/颜色，单行省略，不增加卡片、按钮或 Expander；新增
  `AiAssistant.RetrievalSummary`，不把原始 prompt、provider 元数据或绝对路径暴露到 UI。
- 默认隐藏的 `Tool.Search` 启动时只准备浮窗几何，不再调用 `Float()` 创建中间原生宿主；第一次显式
  打开仍复用 `ShowAndActivate` 生成并激活独立浮动 Dock。Fix2 进一步确认布局恢复会覆盖编译默认状态，
  因此 v2/legacy 恢复完成后、刷新浮动宿主前统一把 Search 规范为隐藏；显式打开会覆盖错误的底部位置。
- 未修改 `ShellWindow.xaml`、主题、布局序列化格式、Search 业务、INI/字段库/诊断或
  Preview/Apply/Undo/Save 语义。物理启动无闪现与最终间距待人工验收。
- 自动证据：Fix2 focused 38/38、IDE 2745/2745、Application 198/198、Release build 0/0、
  IdeOnly clean package 1247 files。
- 权威文档：`AGENT-TRACE-1_CompactRetrievalSummaryUiContract.md`、`AGENT-TRACE-1_StageLedger.md`、
  `UI-DOCK-SEARCH-STARTUP-FIX1_FinalContract.md`。

## 2026-08-25 UI-DOCK-SEARCH-STARTUP-FIX3 current stop point

- 真实运行事件确认：隐藏的 `Tool.Search` 直接执行 AvalonDock `Float()` 会触发
  `NullReferenceException`；现改为先恢复到有效 Pane，再迁移到独立浮动宿主。
- 启动闪现的另一根因是原抑制仅覆盖原生浮窗，没有覆盖主窗口内部 DockingManager；主 Dock 现从
  XAML 创建起保持透明且不可交互，直到布局恢复、`Tool.FindReferences`/`Tool.Search` 默认隐藏规则和
  浮窗刷新全部完成后再一次性显示。
- 新增真实 WPF Window + `LayoutFloatingWindowControlCreated` 回归，不再只依据 AvalonDock 模型属性。
- focused Debug tests 3/3、IDE Debug full 2746/2746、Release build 0 errors / 1 existing nullable warning；
  稳定态视觉观察曾发现 `FindReferences` 遗漏并已据此补齐，最终修正版启动首帧仍待用户手工验收。

## 2026-08-26 AGENT-WORK-ENTRY-1 current stop point

- 已纠正先前“理想 JSON 测试通过即代表真实 Work 可靠”的错误结论。真实实现曾把 domain、completion、
  capability 白名单、附加字段和查询占位参数作为整包否决条件，并丢弃具体解析失败原因。
- Work 第一轮现只对唯一工具调用、工具名、有界 JSON 对象、payload/depth 和重复根字段 fail closed；
  其它描述性差异归一化并记录 request-lifetime recovery notes。
- 查询按项接纳；路径/未知 target 永不执行，但不会摧毁整个意图包。语义补查解析采用相同原则。
- 生产 current-document Work 统一使用 model-owned `preview_ini_edit_plan`，生产 rules/art Work 统一使用
  `preview_ini_project_edit_plan`；typed capability 仅选择 Skill/检索策略，旧 typed template 仅供显式
  headless 兼容调用，不再拥有生产内容否决权。当前文件通用 Upsert 会为缺失 Section 建立 Preview 级创建项。
- Debug build 0 warnings/0 errors；AI 509/509、Application 198/198、IDE 2756/2756；真实 DeepSeek 与 GUI 手工验收未运行。
  真实 DeepSeek 与 GUI 手工验收未运行。
- 权威契约与阶段证据：`AGENT-WORK-ENTRY-1_MinimumSafetyWorkAdmissionFinalContract.md`、
  `AGENT-WORK-ENTRY-1_StageLedger.md`。
- W1-6 修正 generic Document/Project Proposal 的展示元数据边界：`summary/message` 不再决定可执行
  operations 是否接纳；summary 无效时使用本地默认，proposal message 忽略。Clarification 仍必须有
  可读 message，回显 operations/documents 保持惰性。普通执行与一次 bounded repair 复用同一 adapter。
- W1-6 自动证据：focused 74/74、AI 493/493、Application 198/198、IDE 2771/2771、Debug build
  0 warnings/0 errors；新 DLL 中三条旧 proposal-message 拒绝文案均不存在。真实 DeepSeek/WPF 复验待用户执行。

## 2026-08-26 DIFF-REVIEW-1 / ASSET-VOX-1 research stop point

- DIFF-REVIEW-1 DR1-A -> DR1-E 已完成：Project Proposal 默认显示精确 `CandidateText`，保留虚拟化
  unified Changes，并提供深度一、有界、同快照的 Object Context。Apply 仍只整包进入既有事务。
- Debug build 0/0；focused 19/19、Application 198/198、IDE 2779/2779；DR1 stage package 1255 files，
  加入 VOX 研究文档后的最新 IdeOnly clean package 1256 files。
  1920x1080 与窄宽物理视觉验收待用户执行。
- VOX 流水线已完成一手资料和代码事实侦察。结论为 provider-neutral image-to-3D + 本地确定性
  VoxelScene/VOX/palette/SliceStack + VXLSE III 人工收口；Vengi/独立 parser 作交叉验证。
- 当前没有实现 VOX 生成、模型 Host、SliceStack、VXLSE package 或最终 VXL/HVA 写入。下一安全入口是
  `ASSET-VOX-1A Golden Probe 与契约冻结`，先实测当前 VXLSE III importer 的 axis/order/pixel format。
- 权威提案：`Docs/ASSET-VOX-1_SystemInvestigationAndArchitectureProposal.md`。

## 2026-08-26 ASSET-VOX-1A code baseline stop point

- 已实现 UI-neutral 的分离式体素装配基线：一个 root Body、可挂接 Turret/Barrel/Other，文件 stem、
  VXL/HVA Section、父子图和 companion closure 均有显式约束。
- 新增受限 VXL/HVA metadata probe：复用既有 16 MiB 素材上限，检查签名、数量、offset、bounds、
  dimensions、finite transforms，并对单 Section 匿名 legacy HVA 做无歧义兼容。
- focused 9/9、Application 207/207；Debug build 0 errors / 1 个既有 Field Registry nullable warning；
  最终 IDE full 为 2778/2779，唯一失败是未触及的 WPF STA resource/Popup teardown 测试，立即 isolated rerun
  1/1 通过（最后一次仅改装配闭包前的 full run 为 2779/2779）。IdeOnly clean package 1262 files。
  真实 `tnkd + tnkdtur` 装配闭包通过，新探针与用户授权的 VoxelNormalForge 对 Section、
  尺寸和 normal type 交叉一致。
- 此 stop point 当时尚未取得 VXLSE III；后续已由下方 “VXLSE contract completion update” 取代。
- 下一安全入口已更新为 `ASSET-VOX-1B Canonical Voxel Core`；真实 Barrel 仅为后续视觉/pivot 标定样本。
- 权威契约与账本：`ASSET-VOX-1A_GoldenProbeAndSeparatedAssemblyFinalContract.md`、
  `ASSET-VOX-1A_StageLedger.md`。

## 2026-08-26 ASSET-VOX-1A VXLSE contract completion update

- 用户提供的 VXLSE III MagicalVoxel 导入版已定位：file version `1.3.9.3281`、product version `1.4.0.0`、
  SHA-256 `DB9A882A74E16ECB1D938C6D07EC4C97B28D51EF23975730DF2211E354916458`；随包含 Pascal source 和
  RA2 `unitsno/unittem/uniturb` palettes。
- 新增 internal `Ra2VxlseSliceImportContract`，按同版本源码实现 Downward/Rightward 精确 raster addressing、
  exact dimensions、direct-alpha occupancy、Westwood 6-bit PAL expansion 和 VXLSE nearest-colour tie-break。
- Import preflight 明确要求新建/已清空 Section，并始终要求重新生成 normals。VXLSE Resize 使用 session-global
  land/air DefaultTransforms，因此 world axis、pivot/mount 仍不能从 importer 推断。
- 3x4x5 非对称合成 part 在两种布局完成逐坐标往返；真实 Barrel 样本不再是装配/切片契约门禁，只留作
  后续视觉、pivot 和游戏标定。
- 最终验证：focused 17/17、Application 215/215、IDE 2779/2779；Debug build 0 errors / 1 个既有 Field
  Registry nullable warning；IdeOnly clean package 1264 files。
- executable GUI import 仍未运行，将在 1B deterministic RGBA PNG exporter 可用后作为独立验收执行；生产
  协议不依赖 GUI 点击自动化。下一阶段：`ASSET-VOX-1B Canonical Voxel Core`。

## 2026-08-26 ASSET-VOX-1B current stop point

- `ASSET-VOX-1B Canonical Voxel Core` 已完成 internal 纯核心及用户提供 VXLSE 的结构性回读验收：不可变单部件 snapshot、256 色 palette profile、
  canonical SHA-256、受限 MagicaVoxel VOX v150 reader/writer、受限 Westwood VXL span reader、VXLSE-compatible
  RGBA SliceStack 与 deterministic PNG codec。
- VXL reader 来自用户授权的 VoxelNormalForge 逻辑，但已补齐 offset、length、packet、occupancy 与 allocation
  上限。VXLSE source 证明 VXL header `PaletteData` never used，因此必须显式提供实际外部 palette。
- 用户提供的可执行版本额外执行 `VXL(x,y,z) = (input z, input X-1-x, input y)`；新增独立逆变换桥接，
  不改变通用 SliceStack 或标准 VXL reader。新空 Section 的真实回读为 `Body`、`3x4x5`、occupancy 5，
  5/5 坐标及 palette index 全部一致，canonical hash 为
  `29A4A1150EEFB6305021B29CA37B7C3F58B0B845FEB779C63F93EA0DCF0161C2`。
- 最终 focused 30/30、Application 228/228、Debug build 0 errors / 1 个既有 Field Registry nullable warning；
  IDE full 2778/2779，唯一失败是未触及的已知 WPF STA resource/Popup teardown 偶发项，立即 isolated rerun 1/1 通过；
  IdeOnly clean package 1273 files。
- 未完成且未宣称：视觉质量、pivot/mount、normal、HVA、游戏烟测、模型 provider 和直接 VXL writer。
- 下一安全入口：`ASSET-VOX-1C Generation Provider Host` 的代码事实审计与最终契约；不得把 1B 结构验收
  描述为 `GameReady`。

## 2026-08-26 ASSET-VOX-1C completed stop point

- `ASSET-VOX-1C` 修订版 R4 契约已获批准，1C-1 → 1C-5 已连续完成并自动验证。
- Existing Asset Provider 只能闭合最终 Manifest，Application 又有明确的 Process/File 禁用边界，因此
  1C 采用新的 headless AssetHost 程序集，而不是污染二者。
- 1C 只实现可信配置的本地进程、瞬态 workspace、progress/cancel/timeout/crash、GLB/PNG hash/provenance；
  不实现通用持久化 Job/Event/Artifact、OS sandbox、HTTP/API key、真实模型安装、UI 或项目写入。
- 修订版已冻结唯一 internal `ProbeAsync`/`RunAsync` Host seam 与只读 async-disposable workspace lease；
  Probe 不缓存授权，Run 仍重验 hash/identity/capability/license。
- 专用 workspace 只通过 root/run marker、active lock 和默认 24h TTL 清理孤儿；stdout/stderr 必须并发有界
  排空，并对 cancel/timeout/terminal/exit/promotion 竞态执行确定性裁决。
- 已新增 headless `RA2IniEditor.AssetHost` 与确定性 managed fixture：可探测可信 provider、执行一次有界
  image-to-mesh、验证 GLB/PNG/JSON/hash/size、通过只读 async lease 暴露候选并记录 provenance。
- 门禁：AssetHost 38/38、Application 228/228、IDE 2779/2779；IDE-only build 0 warnings/0 errors；IdeOnly clean
  source package 1295 files；Application public allowlist 77，AssetHost exported public types 0。
- 下一阶段不能把本结果描述为视觉良好、VXL/HVA 或 GameReady。推荐先做 `ASSET-VOX-1C-P1` 真实 provider
  adapter/环境/许可证/质量认证，或先审计 `ASSET-VOX-1D` 候选网格到 1B 规范体素核心的桥接契约。

## 2026-08-26 ASSET-VOX-1C-P1 authorization stop point

- P1-0 环境、上游、许可证与代码复用审计已完成并通过自审；没有安装依赖、下载权重、接受第三方许可证或
  执行真实模型调用。
- 本机事实：RTX 4080 SUPER 16,376 MiB、Windows、Python 3.11.9 可用；Python 3.11 中尚无 Torch、Pillow、
  trimesh、Hugging Face 或 provider 依赖，也未发现可复用模型缓存。
- 官方 TRELLIS.2 当前基线为 Linux-tested、至少 24 GB VRAM，不适合作为本机首个 Provider。首选候选改为
  Hunyuan3D-2mini shape-only；是否真正兼容仍须安装后实跑，不能由 README 配置推断。
- 最终契约复用既有 1C Host/协议/Lease，并要求单文件自包含 Adapter、外置固定 Provider bundle、完整
  bundle/model provenance、一次进程一次 run、无产品时下载和无项目写入。1D 仍独占 GLB 到 1B canonical
  snapshot 的解析/规范化/体素化职责。
- P1 风险为 R4。P1-1 → P1-5 因 Tencent Hunyuan 3D 2.0 Community License 接受以及较大依赖/源码/权重
  安装授权缺失而强制停止；不得提前声称 real provider、视觉质量、VXL/HVA 或 GameReady 已完成。
- 权威文件：`ASSET-VOX-1C-P1_RealProviderEnvironmentCodeFactAudit.md`、
  `ASSET-VOX-1C-P1_HunyuanMiniProviderFinalContract.md`、`ASSET-VOX-1C-P1_StageLedger.md`。
- 下一步：用户明确接受许可证并授权隔离安装/下载/真实本地测试后，按 P1-1 → P1-5 连续执行并逐段自审；
  P1 通过后才进入 `ASSET-VOX-1D`。

## 2026-08-26 ASSET-VOX-1C-P2 remote provider stop point

- 腾讯混元生 3D OpenAI 兼容远程适配器 P2-1/P2-2/P2-4 已完成：固定 3.1 `Geometry`、单图单候选、
  submit at-most-once、同 JobId 有界查询、HTTPS/redirect/size 门禁、GLB/PNG/provider JSON 与零 public API。
- AssetHost 公共协议、workspace/lease authority、1B canonical voxel core、Shell/XAML、INI/字段库/编辑保存语义
  均未修改；Host 内部仅增加安全失败消息透传和四项 Windows child-runtime 环境白名单。
- 自动验证：build 0/0、AssetHost/provider 47/47、Application 228/228、IDE 2779/2779、clean package 1309。
- P2-3 原授权 3/3 次尝试未生成 Job；非计费探针确认根因是 Host 清空了子进程所需的
  `SystemRoot/WINDIR/TEMP/TMP`。修复后用户另行批准第 4 次，真实 Job 在约 2m10s/42 polls 后 `DONE`，
  输出 8,991,920-byte GLB、77,888-byte preview 与脱敏报告；Host 校验通过。
- GLB 为 glTF 2.0，1 scene/node/mesh/primitive，249,567 vertices、499,698 triangles。响应未返回积分消耗
  字段，免费包扣减仍以腾讯控制台为准；不得自动进行第 5 次调用。
- 下一安全入口：`ASSET-VOX-1D` GLB-to-canonical-voxel bridge 的代码事实审计与契约。当前结果只认证
  远程 Shape/Host 产物链，不代表视觉质量、voxelization、VXL/HVA 或 GameReady。

## 2026-08-26 ASSET-VOX-1D completed stop point

- 1D-1 → 1D-5 已按批准的 R4 契约完成：Application 内新增 BCL-only 受限 GLB reader、节点 TRS/matrix
  flatten、typed topology facts、显式轴向/比例规范化、triangle/AABB surface rasterization、watertight fill、
  palette policy 和 review-required canonical result。
- 当前真实单连通、无材质 GLB 被诚实地转换为 Body 候选；不自动猜测 Turret/Barrel、原色、最终 pivot、
  normals、HVA 或 GameReady。
- 真实 P2 GLB 结果为 `29x64x31`、20,261 cells、canonical hash
  `3FC301CC7B1336635EBD137E8312D85179A32E501CC60E1FB983E2DB4D986D90`；VOX 与 SliceStack exact round-trip。
- 首轮真实验收暴露边字典拓扑统计退化至约 214 秒；改用排序 packed-edge array 后 parser/topology 187ms，
  两次 voxelization 99ms/81ms，输出哈希未改变。
- focused 10/10、Application 238/238、IDE 2779/2779、AssetHost 47/47；Debug build 0 errors / 1 个既有
  Field Registry nullable warning；IdeOnly clean package 1315 files。无外部模型调用。
- Application public allowlist 保持 77，AssetHost exports 保持 0；Shell/XAML、INI、字段库、编辑保存、Host
  和 Provider 协议均未修改。
- 权威文档：`ASSET-VOX-1D_GlbToCanonicalVoxelCodeFactAudit.md`、
  `ASSET-VOX-1D_GlbToCanonicalVoxelFinalContract.md`、`ASSET-VOX-1D_StageLedger.md`。
- 下一安全入口：后台 product composition/preview 审计，或独立 `ASSET-VOX-1E` 分件与调色板审阅阶段。

## 2026-08-27 ASSET-VOX-1E completed stop point

- 1E-1 → 1E-5 已完成：项目祖先链 `VOXEL_STYLE.md`、单次结构化编译 seam、全键缓存、本地 plan/palette
  校验、确定性几何区域上色和无路径的 headless review package 均已实现。
- 真实 1D Body 候选的 20,261 个占用单元与坐标完全不变；重复上色与审阅包哈希一致，输出由单色变为
  顶/侧/底/边缘/内部五类角色。玻璃没有显式蒙版，因此保持未解析且未着色。
- 已生成排除在源码包外的 VOX、SliceStack、palette swatch、region mask 与三份安全 JSON；没有写入 mod
  项目，没有生成 VXL/HVA/normals，也没有执行真实 DeepSeek 或游戏验证。
- 自动门禁：Application 249/249、IDE 2787/2787、AssetHost 47/47；build 0 errors / 1 个既有 nullable
  warning。Application public allowlist 77、AssetHost exports 0 保持不变。
- UI、真实 DeepSeek 风格编译、项目 Apply/Save、语义蒙版编辑、VXL/HVA 和 game smoke 仍需单独批准；
  当前下一安全入口是用户审阅 `artifacts/asset-vox-1e-acceptance/p2-body-64/`，或单独制定 UI/蒙版契约。
- 权威文档：`ASSET-VOX-1E_NaturalLanguageStyleProfileFinalContract.md`、`ASSET-VOX-1E_StageLedger.md`。

## 2026-08-27 ASSET-VOX-1E-UI completed stop point

- 已新增 `工具 -> 体素风格预览`：打开单实例、不可浮动的中央文档，不加入 AvalonDock 默认工具配置，
  因此不会参与启动布局恢复或产生启动闪现。
- 用户可只读选择当前项目内的单模型 `.vox`，查看原始切片、继承风格来源，并用自然语言给出本次风格要求。
  选择文件或打开界面不会调用 DeepSeek；只有显式“编译预览”会调用既有 1E 专用结构化编译器。
- 编译成功后可切换原始、着色结果、几何区域和色板视图，并审阅角色、规则、未解析假设、色板误差与
  几何/占用不变事实。接受只保留在当前内存会话，不写入、导出或生成 VXL/HVA。
- 路径、取消、迟到响应、缓存和 review package 仍由独立 headless coordinator 管理；没有复制 1E 算法。
- 自动门禁：focused 6/6、Application 249/249、IDE 2793/2793、AssetHost 47/47、build 0/0、clean package
  1340 files。真实 DeepSeek 与物理截图验收均为 NotRun。
- 下一安全入口：用户先手工审阅 1920x1080 UI 与一次显式真实编译；之后再单独规划显式语义蒙版编辑或
  accepted preview 到 VXL/HVA 流水线的会话级 handoff。

## 2026-08-27 ASSET-VOX-1E-UI-FIX1 runtime compatibility stop point

- 首次物理打开暴露 `Ra2VoxelStyleWorkspaceView` 第 154 行的运行时样式类型错误：WPF `GridSplitter`
  误用了仅面向 AvalonDock `LayoutGridResizerControl` 的 `IdeDockSplitterStyle`。
- 工作区现复用既有 `UiGridSplitterStyle`；未修改共享主题、Shell、布局、AutomationId 或 1E 业务语义。
- STA 视觉资源门禁现在会真实执行该视图的 `InitializeComponent()`，并由精确采用白名单锁定工作区与
  WPF splitter style 的关系。
- 自动证据：focused 5/5、IDE 2793/2793、solution build 0 warnings / 0 errors。用户重建后重新打开的
  物理确认仍待执行。

## 2026-08-27 ASSET-VOX-1F-CORE-1 stop point

- 已选择性迁移 VoxelNormalForge 的外露面提取和 RA2/TS 法线表/烘焙算法；没有引入旧工程依赖、mutable
  VXL 模型、CLI、OBJ 中间文件或 Writer。
- 新算法只消费 canonical `Ra2VoxelSceneSnapshot`，因此 VOX 与 VXL 在既有解码后走同一路径；派生表面和
  normal field 绑定来源哈希，不修改 snapshot schema，也不写文件。
- 新增门禁 6/6、全部 voxel 48/48、Application 255/255、AssetHost 47/47、Release build 0 errors / 1 个
  既有 nullable warning。IDE 2798/2799 命中文档化 WPF Popup teardown flake，隔离重跑 1/1 通过。
- 下一安全入口：`ASSET-VOX-1E-UI-3D`，只让现有体素风格工作区消费新的外露面投影；法线写回和 VXL/HVA
  materialization 仍需独立契约。

## 2026-08-27 ASSET-VOX-1E-UI-3D stop point

- 体素风格工作区的原始、着色结果和几何区域主预览已替换为原生 WPF 交互式 3D；色板保持 2D，切片保留为
  显式诊断与资源上限失败回退。
- 3D 只消费 canonical snapshot 与 1F 外露面投影，支持有界旋转、平移、缩放、重置；异步场景有取消与代次
  隔离，冻结后才原子替换可见模型。
- 没有新增依赖、第二套体素模型、Shell/layout、项目写入、VXL/HVA Writer 或游戏法线宣称。
- 自动证据：focused 29/29、voxel 48/48、Application 255/255、AssetHost 47/47、Release build 0 errors / 1 个
  既有 warning；IDE full 2801/2802 命中已知 Popup teardown flake，隔离重跑 1/1。
- 当前门禁：用户执行 1920x1080 旋转/平移/缩放/重置/切片返回与截图验收。之后建议审计多部件
  Body/Turret/Barrel 组合预览，而不是直接进入 Writer。

## 2026-08-27 ASSET-VOX-2A completed stop point

- 2A-1 → 2A-5 已按修订边界完成；用户明确排除了原模型调整，源 mesh/provider 产物保持不变。
- 新增确定性质量事实、六视图轮廓、薄结构逐坐标保护、2x 超采样/一次有界清理、局部 X 对称建议、法线
  对比、四类语义审阅区域和弱对比体色优化候选。
- DeepSeek seam 最多三轮且每轮不同结构契约；可首轮停止、精确缓存零调用、无重试和第四轮。只用 fake
  client 验证，未执行真实 DeepSeek/Tencent 调用。
- 自动证据：Application 264/264、IDE 2807/2807、AssetHost 47/47；solution build 0 errors / 1 个既有 nullable
  warning；IdeOnly clean package 1361 files。运行中的 IDE 锁住默认 DLL，因此最终门禁使用隔离输出目录。
- public API、持久化、Shell/XAML、INI/字段库、Work/Apply/Save、Host/provider、VXL/HVA 均未改变。
- 下一安全入口：`ASSET-VOX-2A-UI Review Candidate Composition`，把 direct/refined/symmetry 与质量事实接入
  既有 3D 工作区；物理视觉验收和真实 DeepSeek 仍需独立授权。

## 2026-08-27 ASSET-VOX-2A-UI contract stop point

- 已完成只读代码事实回归和最终 UI 契约自审；实现尚未开始，XAML/Shell/产品行为未改变。
- 关键事实：现有工作区只接受 VOX/VXL，而真正的 2A Direct/Refined 候选需要原始 GLB；仅凭已生成 VOX
  不能重建超采样候选。契约因此增加显式、项目内、只读 GLB 质量源，不伪造 VOX 后处理结果。
- 设计复用现有 canonical snapshot、2A refiner、style compiler/colourizer、palette contrast optimizer 和原生
  3D viewport；候选选用与风格结果接受保持两个独立的会话级决定。
- 风险为 `R3 / StopForReview`。真实 DeepSeek/Tencent、Shell、Apply/Save、VXL/HVA、public API 和持久化均冻结。
- 权威文件：`ASSET-VOX-2A-UI_ReviewCandidateCompositionCodeFactAudit.md`、
  `ASSET-VOX-2A-UI_ReviewCandidateCompositionFinalContract.md`、`ASSET-VOX-2A-UI_StageLedger.md`。
- 下一步：用户批准最终契约后连续执行 `UI-1 -> UI-5`，每阶段自审，最终交付 1920x1080 手工视觉验收。

## 2026-08-27 ASSET-VOX-2A-UI completed stop point

- `UI-1 -> UI-5` 已连续完成：工作区可显式配对项目内 GLB，生成 Direct/Refined/可选 Symmetry，并显示
  Verified/UserPaired/Mismatch 来源状态、质量指标、法线对比和语义区域证据。
- 几何候选必须点击“用于本会话”才参与既有显式风格编译；普通着色与可选对比度着色并列审阅，任何
  contrast 无结果/失败都不会否定普通有效结果。
- 所有新状态均为 IDE 会话内投影；无真实 DeepSeek/Tencent、无文件写入、无 Shell/layout、Apply/Save、
  VXL/HVA、public API 或持久化变化。
- 自动证据：UI-1 9/9、UI-2 11/11、UI-3 13/13、UI-4 33/33；Application 264/264、IDE 2814/2814、
  AssetHost 47/47；solution build 0 errors / 1 个既有 nullable warning。
- 当前门禁：用户在 1920x1080 下手工确认按钮换行、Current/Direct/Refined/Symmetry、3D 交互、会话选用和
  Styled/Contrast 视觉差异。下一安全阶段应先基于这次验收决定多部件组合或素材 materialization 契约。

## 2026-08-27 ASSET-VOX-2A connectivity correction stop point

- 物理验收发现绝对单组件门把 17,181 个细化体素中的 1 个孤立体素误判为整车碎裂；根因已回归到
  `Ra2VoxelQualityRefiner`，不是 GLB、VOX、3D 预览或用户操作错误。
- 连通性门现按 canonical connectivity 相对判断：单组件直接通过；多组件仅在最大组件不足总占用 95%
  时拒绝。UI 同步展示组件数与主体占比。
- 真实 `H:\RA2\YR_Test\body-candidate.vox + mesh.glb` 的参数扫描证明 40% coverage 可通过未放宽的 5%
  体积门和 3% 六视图轮廓门；正常 coordinator 产品路径输出 17,397 体素、1 组件、主体占比 100%。
- 自动门禁：focused core 9/9、focused IDE 16/16、Application 267/267、IDE 2814/2814、AssetHost
  47/47；串行 solution build 0 errors / 1 个既有 nullable warning；clean package 1366 files。
- 未修改 XAML/Shell、provider/真实调用、Apply/Save、VXL/HVA、public API、持久化或原始模型。

## 2026-08-27 ASSET-VOX-2A-R2 completed stop point

- `R2-0 -> R2-4` 已连续完成。旧的 `face-neighbour <= 1` 无条件删除路径已移除，长杆、炮管、天线和薄板按
  连通结构整体冻结，端点与相邻过渡带进入显式质量事实。
- 细化器现生成 Conservative / Balanced 两个确定性候选，只在冻结结构之外执行离散距离过滤；原始
  canonical VOX/VXL 快照和 GLB 证据保持不可变。
- 准入改为硬门禁：禁止新增断裂组件/封闭孔洞，冻结坐标必须原位保留，体积与六视图轮廓受限，低支撑、
  粗糙度和对称性不得退化，且至少一项质量事实必须可测量改善。无安全改善时返回 `NoSafeImprovement`
  并保留 Direct，不再用任意变化冒充优化。
- 既有 3D 工作区增加差异审阅：新增绿、移除红、冻结结构蓝、未改变灰；同时显示准入结论与结构保护摘要。
  未准入的 Refined 不可用于本会话。
- 自动证据：core focused 13/13、IDE focused 11/11、Application full 271/271、solution build 0 errors；IDE full
  2815/2816 命中既有 WPF Popup teardown flake，隔离重跑 1/1 通过。
- 未执行真实 DeepSeek/Tencent；未修改 Shell、Apply/Save、VXL/HVA、public API、持久化、INI 或字段库。
- 下一阶段建议：用用户的车体/炮塔/炮管真实样本执行 1920x1080 手工差异验收，再决定是否进入多部件组合精修。

## 2026-08-27 ASSET-VOX-2A-R2 physical review correction 1

- 修正无安全改善时仍把 Direct 暴露成“平滑/差异”的误导状态：只有已准入且 canonical hash 不同、delta 非零时
  才启用平滑和差异视图。
- 薄结构识别改为带方向签名的杆/板组件，避免车体表面网络互相串联成大面积蓝色保护区；新增拓扑安全表面
  通行证，附着噪点可移除，但炮管末端和连接路径保持。
- 真实 `H:\RA2\YR_Test\body-candidate.vox + mesh.glb` 产品路径：Conservative 已准入；18,301 → 18,267，
  新增 30、移除 64、冻结 126、过渡 50。临时物理探针代码已删除。
- 自动证据：核心 15/15、受影响 IDE 21/21、Application 273/273、IDE full 2816/2816、构建 0 errors / 1 个
  既有 nullable warning。
- Shell、Apply/Save、VXL/HVA、真实 DeepSeek/Tencent、持久化、public API、INI/字段库均未改变。

## 2026-08-27 ASSET-VOX-2A-R2 physical review correction 2

- 第二次物理截图确认旧 Conservative 并非连续曲面平滑，而是按扫描顺序逐体素删除，造成车体表面的红色
  椒盐差异；该路径与未使用的距离过滤实现均已退出生产候选生成。
- 新候选统一使用局部加权曲面提案，并要求 2x GLB 占用证据确认增删方向；最终只保留至少 2/3 个体素的
  26 邻域连续差异组件。孤立变化不能再因改善单项统计值而获准。
- 真实样本产品核心路径：Balanced 已准入，18,301 -> 18,286，+34/-49；孤立差异组件 0，组件数 1，
  空腔 0，最大轮廓变化 1.21%，粗糙度 1.5526 -> 1.5348，低支撑表面 76 -> 62。
- 进程关闭后，标准 Debug 构建通过（0 errors / 0 warnings），Application 273/273、IDE 全量 2816/2816；
  临时真实路径诊断代码已删除。
- 未修改 UI/Shell、Provider/真实调用、Apply/Save、VXL/HVA、持久化、public API、INI 或字段库。

## 2026-08-27 ASSET-VOX-2B completed stop point

- `2B-0 -> 2B-4` 已连续完成：本地候选生成有界六视图/区域证据但不调用模型；显式“AI 识别结构”执行
  两次 required-tool DeepSeek 回合，第二轮审阅第一轮标准化结果。
- 只有两轮确认的主体对称区可由确定性 GLB 覆盖证据执行增删；非对称附加件、受保护薄结构、不确定区和
  一格过渡带保持原占用。切换 GLB/项目/模型或取消时，陈旧结果不会回写。
- 自动证据：focused core 20/20、affected IDE 30/30、Application 278/278、IDE 2825/2825；Debug build
  0 errors / 1 个既有 nullable warning。真实 DeepSeek/Tencent 未运行；1920x1080 人工验收待用户执行。
- Shell、Apply/Save、VXL/HVA、public API、持久化、INI、字段库和 legacy 均未改变。当前上色仍为既有
  粗粒度几何角色算法；下一阶段建议独立设计 material-semantic colouring。

## 2026-08-28 ASSET-VOX-2B physical-sample correction stop point

- 用户截图中的“没有效果”已确认不是模型或按钮问题：真实平滑候选包含超过 64 个零散差异/保护连通块，
  旧证据构建器因此返回 `EvidenceTooLarge`，导致 `AI 识别结构` 被禁用。
- 模型侧区域现按左右、三段高度、前后纵深和四类形态确定性聚合；Host 仍保留全部坐标，并新增每个区域的
  连通块数量。最大区域数由构造保证不超过 50，没有提高 prompt/tool 上限，也没有截掉小体素。
- 真实 `H:\RA2\YR_Test\body-candidate.vox + mesh.glb` 产品路径复验通过：18,286 个 Refined 体素全部且仅一次
  进入有界证据。临时绝对路径探针已删除。
- 自动证据：semantic core 6/6、affected IDE 27/27、Application 279/279、IDE 2825/2825、Debug build
  0 errors / 1 个既有 nullable warning。真实 DeepSeek/Tencent 未调用。
- 下一步：用户用重建后的程序重新点击“生成候选”，状态应显示“结构证据已就绪（N 个区域）”，随后
  `AI 识别结构` 可点击；结构识别质量仍需一次真实双轮手工验收。

## 2026-08-28 ASSET-VOX-2B visual/provider correction stop point

- Refined now evaluates three bounded surface behaviours. The certified local pair selects SurfacePolish with 167 actual
  cell changes (+14/-153) and improves low-support surface cells from 76 to 58 without weakening any hard gate.
- Difference mode no longer paints asymmetric local protection facts blue. Unchanged body is translucent grey; only
  geometry additions/removals are green/red. Post-AI protected thin features retain blue and may be intentionally one-sided.
- The explicit AI action remains clickable after evidence generation. Equivalent DeepSeek tool JSON representations are
  normalized, but hash/axis/known-region/full-coverage validation remains strict.
- Build and focused tests passed; full-suite and clean-package results are recorded in the 2B ledger after final gates.

## 2026-08-28 ASSET-VOX-3A stop point

- `ASSET-VOX-3A Generation Orchestration` 3A-1 through 3A-5 completed.
- The existing Voxel Style workspace now has an explicit reference-image generation card, offline provider probe,
  per-run consent, progress/cancel flow and session-only GLB-to-voxel adoption.
- Generated candidates have no fake project path and can continue through existing local quality/style/structure review.
- Verification: solution build passed; AssetHost 50/50, Application 285/285, IDE 2831/2831; clean package 1384 files.
- No real Tencent/DeepSeek call, Shell, Apply/Save, VOX/VXL/HVA writer or INI semantic change occurred.
- Latest ledger: `Docs/ASSET-VOX-3A_StageLedger.md`. Next safe phase: separately approved `ASSET-VOX-3A-P1` manual probe.

## 2026-08-28 ASSET-VOX-2B neutral repair-evidence correction stop point

- 已消除模型输入中的预分类闭环：`core` 仍是已镜像匹配上下文，不匹配区域改为中性 `repair-*`，不再用
  `attached/detail/bodylike` 名称诱导两轮把全部修复机会判为附加件。
- 每个区域新增镜像目标的 GLB coverage 与主体接触事实；DeepSeek 两轮可据此确认缺失的对称主体，而不是仅
  凭“单侧存在”降级为不确定。
- 双轮一致性、0.80 置信门、分歧转 Uncertain、非主体原位保护和全部本地几何门未放宽。
- 自动验证：semantic Application 6/6、compiler/coordinator 20/20、Application 280/280、IDE 2830/2830、
  AssetHost 47/47、隔离 solution build 0 errors / 1 个既有 nullable warning。
- 真实 DeepSeek 未由本阶段调用；下一步是重启新构建后对同一 `body-candidate.vox + mesh.glb` 执行一次真实
  双轮验收，观察 repair 区是否形成可审阅的 cyan 对称候选。

## 2026-08-28 ASSET-VOX-3B stop point

- 3B-1 → 3B-5 已按批准契约实施：会话内最终候选和 VOX Save-As 成为独立、显式的产品动作。
- 导出只消费固化时的 canonical snapshot，不会因切换审阅视图而漂移；真实输入变化会使旧候选失效。
- 导出不覆盖当前源，其他覆盖使用原生确认；临时 VOX 必须回读并确定性重编码一致后才原子发布。
- 没有修改 Shell、INI、Field Registry、项目 Apply/Save、public API、Provider、VXL/HVA 或 legacy。
- 自动验证：focused 23/23、AssetHost 50/50、Application 285/285、IDE 2844/2844、Debug build 0 warning / 0 error；
  IdeOnly clean package 1389 files。WPF 人工交互待用户验收。

## 2026-08-28 ASSET-VOX-UI-R1 contract gate

- 已完成体素工作区代码事实审计和 `ASSET-VOX-UI-R1 Workspace Recomposition and Camera Stability` 最终契约；
  当前只新增文档，尚未修改运行时代码。
- 已确认两类独立根因：全页双向滚动、固定预览高度和长 StackPanel 导致工作区重排；每次场景成功重建后
  无条件 `ResetCamera()`、临时 Unloaded/Loaded 重建导致用户视角跳变。
- 契约冻结为“左侧四任务页 + 中央自适应 3D + 下方四证据页 + 两个 splitter”，并规定同一来源会话的模式
  切换保留归一化相机姿态；新来源或用户显式重置时才自动取景。
- 风险分级 R3；Shell、业务 ViewModel、SceneBuilder、Provider、算法、Apply/Save、VOX/VXL/HVA 写出均不在范围内。
- 当前状态：等待用户批准 `ASSET-VOX-UI-R1` 后才进入 UI-R1-0 → UI-R1-5 连续实施。

## 2026-08-29 ASSET-VOX-UI-R1 implementation stop point

- 用户已批准，UI-R1-0 → UI-R1-5 连续实施完成。体素工作区现在是左侧四任务页、中央主 3D、下方四证据页，
  两个局部分隔条可调，不再依赖整页双向滚动或任何根级缩放变换。
- 相机姿态按真实源身份保存：同一源的模式/候选切换与临时卸载保持 yaw、pitch、归一化 target 和包围盒相对
  距离；新源（包括显示名不变但原始 canonical hash 改变的会话源）才触发首次自动取景。
- 自动验证：Debug build 通过；camera/workspace 14/14、affected IDE 88/88、affected Application 87/87、
  AssetHost 50/50、Application full 285/285。IDE full 2849/2850，唯一失败为无关的临时 ContextMenu open-state
  断言，立即定点复跑 1/1 通过；未通过重复全量测试刷绿。IdeOnly 干净包通过，共 1394 个文件。
- Shell、ViewModel、SceneBuilder、Provider、几何/上色算法、Apply/Save、VOX 写出语义、VXL/HVA、INI、字段库、
  public API、持久化和 legacy 均未改变。
- 下一门：用户在 1920×1080 的 100%/125% 下手工验收整体比例、两个 splitter 和跨模式/跨文档相机稳定性。
- 首次手工启动发现并修复 `Run.Text` 默认双向绑定向只读 `SourceName` 回写的运行时异常；工作区全部动态
  `Run.Text` 已显式设为 `OneWay`，并新增静态防回归断言。未给 ViewModel 添加伪 setter。

## 2026-08-29 ASSET-VOX-3C contract gate

- 已完成 Working Geometry Continuity 代码事实审计和最终契约，自审通过；尚未修改运行时代码。
- 当前缺陷已确认：下一次质量生成绕过 `ActiveGeometrySnapshot`，从旧 GLB 重建 Direct/Refined，并在成功后
  主动清空 `_workingGeometry`；后续 Agent 因而在旧分支上正确执行，而不是模型自行撤销修复。
- 修正方案冻结为：不可变 source root + 显式 revisioned working state；GLB 仅作证据；本地/Agent 候选绑定
  working hash/revision/batch hash；只有“用于本会话”可以推进链路。
- 只读候选生成不得再清空有效的 style/frozen candidate；实际 working adoption 仍必须使二者失效。
- 风险等级 R4，治理状态 StopForReview。等待用户明确批准后才可执行 3C-0 → 3C-5；真实 Provider、Shell、
  Apply/Save、VOX writer、VXL/HVA、public API 和持久化均不在授权范围。
- 权威文档：`Docs/ASSET-VOX-3C_WorkingGeometryContinuityCodeFactAudit.md` 与
  `Docs/ASSET-VOX-3C_WorkingGeometryContinuityFinalContract.md`。

## 2026-08-29 ASSET-VOX-3C implementation stop point

- 用户批准后，3C-0 → 3C-5 已连续完成。工作区现在用一个非持久化、带 revision/root/parent 哈希的工作几何
  状态作为后续质量、Agent、着色、固化和导出的唯一会话基线。
- `RefineExisting` 复用原有分析器、保护掩码、超采样证据、候选生成和硬门禁；GLB 的目标分辨率投影只用于
  注册/覆盖证据，Direct/“基线”始终是捕获的工作快照。网格不匹配返回本地类型化失败，不重建旧分支。
- 质量批次和 Agent 结果绑定 working hash/revision、mesh evidence hash、profile 和 batch hash。显式采纳后，
  旧批次可继续查看但不能再次采纳或固化；采纳的精确快照仍可固化并通过既有 VOX 回读门导出。
- 只读 GLB 选择、候选生成和 Agent 分析不再清除有效着色预览或冻结候选；真正采纳几何时仍按契约清除。
- 自动验证：Debug build 0 warning / 0 error；Application 288/288、IDE 2855/2855、AssetHost 50/50。
  真实 Tencent/DeepSeek 未调用。物理 UI/真实样本复验仍待用户执行。
- Shell、Apply/Save、VOX writer、VXL/HVA、Provider、public API、持久化、INI、字段库和 legacy 均未改变。
- 权威交付：`Docs/ASSET-VOX-3C_StageLedger.md`。下一阶段建议在真实样本验收通过后，单独设计
  material-semantic colouring 或 VXL/HVA materialization，不自动展开。

## 2026-08-29 ASSET-VOX-3D center-seam bridge stop point

- 已补齐 2C 操作空间中的中轴短缝缺口：整数对称面的一格空缝和半格对称面的两格空缝现在会成为独立、
  哈希绑定的 `seam-gap-*` 证据目标。
- DeepSeek 只有明确返回 `bridge_center_gap` 才会补缝；Host 不自动填洞。普通 `add_mirror/remove_source` 与
  短缝目标互斥，三格缺口、离轴孔洞和任意内部空腔不会被该规则升级为可执行目标。
- 执行只加入目标中的空体素，复用六邻域/26 邻域/主体色号回退和既有连通、空腔、体积、六视图轮廓门。
- 自动验证：focused Application 16/16、focused IDE 9/9、Application 293/293、IDE 2856/2856、Debug build
  0 warning / 0 error。真实 DeepSeek/Tencent 未调用；物理样本待用户重启验收。
- Shell/XAML、AutomationId、Apply/Save、VOX writer、VXL/HVA、public API、持久化、INI、字段库和 legacy 未改变。
- 权威交付：`Docs/ASSET-VOX-3D_CenterSeamBridgeFinalContract.md` 与 `Docs/ASSET-VOX-3D_StageLedger.md`。

## 2026-08-29 ASSET-VOX-4B brush interaction correction

- 用户物理验收发现画笔在原始预览/无语义证据状态下呈激活但短点击无响应。已确认这是 4B 接线缺陷，
  不是用户操作错误。
- 浏览、画笔和擦除入口会在需要时自动准备确定性的本地语义区域；没有真实 DeepSeek/Tencent 调用。
- 画笔直接使用 3D 命中区域，不再要求先选择下方区域行；离开语义预览会回到浏览状态，避免假激活。
- 定向 workspace/ViewModel/viewport/UI 28/28，顺序 Debug build 0 warnings / 0 errors。Shell、Application 算法、
  Apply/Save、VOX/VXL/HVA、INI、Field Registry 与 public API 未改变。仍需用户重启后做真实短点击验收。

## 2026-08-30 ASSET-VOX-4B-FIX2 contract gate

- 第二次物理验收确认前一修正只解决语义状态进入，左键旋转与 MouseUp 短点击仍竞争，且命中坐标仍由全模型
  最近中心猜测；当前画笔可靠性不合格。
- 已完成 FIX2 事实审计和最终契约。拟改为左键语义操作、右键在主视图任意位置旋转、Shift+右键/中键平移，
  并由 SceneBuilder 发布场景绑定的精确外露面坐标命中表。
- 风险 R3 / StopForReview；当前只新增/更新文档，运行时代码尚未修改。等待用户明确批准 FIX2-0 → FIX2-4。

## 2026-08-30 ASSET-VOX-4B-FIX2 implementation stop point

- 用户批准后，FIX2-0 → FIX2-4 已连续完成。左键现在只负责语义选择/单击画笔/擦除；右键可从模型或主视图
  空白处旋转，Shift+右键/中键平移，滚轮缩放，重置仍使用既有按钮。
- SceneBuilder 为每个颜色批次中的外露 quad 发布 IDE-internal、scene-lifetime 精确命中表；WPF 三角形命中按
  `GeometryModel3D` 身份和四顶点 face ordinal 返回 canonical coordinate。旧最近中心猜测已删除。
- 清场、释放控件和丢失捕获会结束相机手势；空白、非语义、场景过期和未归区命中都有可读反馈。
- 自动验证：受影响 IDE 35/35，Debug build 0 warning / 0 error，IdeOnly clean package 通过。真实 Provider 未调用。
- Shell、Application 算法、Apply/Save、VOX/VXL/HVA writer、public API、持久化、INI、Field Registry 和 legacy
  均未改变。物理 WPF 鼠标烟测待用户在新进程中执行；权威交付见 `Docs/ASSET-VOX-4B-FIX2_StageLedger.md`。

## 2026-08-30 ASSET-VOX-4B-STROKE-1 contract gate

- 已完成连续语义画笔的代码事实审计、详细契约和自审；当前只修改文档，运行时代码尚未开始。
- 拟议实现把左键拖动建模为一个可取消事务：<=4 DIP 精确表面采样、有序去重、轻量临时路径高亮，释放后
  由唯一 Application 多 seed 画笔一次提交；整条笔划最多一个 undo 项和一次正式场景刷新。
- 语义视图拟增加紧凑“部件 / 材质”维度与固定高对比图例；标注色不进入 VOX palette，也不改变 AI/人工
  语义优先级。
- 风险 R3 / StopForReview。等待用户批准 STROKE-0 → STROKE-5；Shell、真实 provider、Apply/Save、writer、
  public API、持久化、INI、Field Registry 和 legacy 均冻结。

## 2026-08-30 ASSET-VOX-4B-STROKE-1 implementation stop point

- 用户批准后，STROKE-0 → STROKE-5 已完成。左键可连续拖动绘制/擦除当前可见外露表面；视口按最大
  4 DIP 间距执行精确 hit-map 采样、有序去重，并在释放时由 Application 唯一蒙版编辑器原子提交。
- 拖动期间仅显示黄色 Paint/红色 Erase 临时 seed 路径；不提前修改 layer、history 或 composition。整条
  笔划最多一个 undo 项和一次正式语义场景刷新；捕获、场景、模式、相机或 hash 变化会安全取消。
- 语义页新增紧凑“部件 / 材质”审阅切换与固定 8+8 色图例；颜色只用于显示，不进入 VOX 色板。
- 自动验证：focused Application 9/9、affected IDE 57/57、Application 302/302、IDE 2885/2885、
  AssetHost 50/50；Debug build 0 error，仅有一条无关既有 CS8602 warning。真实 Provider 未调用。
- Shell、Apply/Save、VOX/VXL/HVA writer、public API、持久化、INI、Field Registry 和 legacy 未改变。
  物理 WPF 鼠标/DPI/视觉验收待用户执行；权威交付见 `Docs/ASSET-VOX-4B-STROKE-1_StageLedger.md`。

## 2026-08-30 ASSET-VOX-4D contract gate

- 用户同意把语义分划保存为模型哈希绑定的 sidecar；已完成代码事实调查、R4 数据模型/兼容性设计和自审。
- 提议的 `.semantic.json` v1 分开保存已接受 Agent、人工区域和人工 cell 三层，不保存颜色截图、几何或 undo。
- 载入要求 snapshot/evidence/manual-layer 三重哈希完全匹配；不支持强制载入、部分恢复或跨哈希猜测迁移。
- UI 只拟在现有语义工具行增加“保存分划… / 载入分划…”和一条状态；Shell、writer、Provider、Apply/Save
  与 Application schema 冻结。
- 风险 R4 / StopForReview。等待用户批准 4D-0 → 4D-5；当前运行时代码尚未修改。
- 权威文档：`Docs/ASSET-VOX-4D_PersistentSemanticMaskCodeFactAudit.md` 与
  `Docs/ASSET-VOX-4D_PersistentSemanticMaskFinalContract.md`。
