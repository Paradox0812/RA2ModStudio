# RA2IniEditor.IDE 当前能力矩阵

更新时间：2026-08-28
本页只记录有源码、阶段台账和验证证据支持的当前能力。未来目标见
`Docs/ProductVisionAndRequirements.md`。

## 1. 总结

当前产品已经是可运行的 source-first INI IDE，并具备真实搜索、字段智能、
诊断、保存安全、DeepSeek 流式助手、受限当前文件结构化编辑闭环，以及可由
`net8.0` 调用方独立消费的 Document Query、Diagnostics 和 Edit Preview 切片。

Minimum HLI-v1 已完成：最小进程内 Capability Gateway、内置 AI consumer、显式 Apply 与
Apply 后当前文件 Problems 刷新已有端到端证据。它还不是最终的自然语言 Mod 生产 Agent：
独立 Agent、素材/图标生成、SHP/VXL 流水线、Job Runtime 和
运行时测试尚未实现。

## 2. 已完成并有验证证据

| 能力 | 当前边界 | 状态与证据 |
|---|---|---|
| IDE-only 工程 | Core、Infrastructure、WPF IDE、非 UI tests、可选 UIA tests；不含 legacy | Completed / Verified；`RA2IniEditor.IDE.sln` |
| Source Editor | AvalonEdit 源码编辑、编辑会话、Dirty、Undo/Redo、程序化同步 | Completed / Verified；现有 editor/session tests |
| Project Explorer / Navigator | 加载规范项目文件、文件与 Section 导航、脏文件离开门禁 | Completed / Verified；FeatureOverview/UserGuide 与 navigation tests |
| Windows `.ini` 启动 | 裸文件参数以直接父目录为项目，并精确载入该顶层 INI；保留 automation folder 参数 | Completed / automated verified；focused 10/10、IDE 2715/2715；Explorer 双击待手工验收，单实例转发未实现 |
| 语言理解 | TextModel、SemanticModel、Section/field/reference 分析 | Completed / Verified；A0/A1 台账 |
| Completion | 字段和值候选、可信度过滤、提交与编辑会话同步 | Completed / Verified；Field Registry/Completion tests |
| Hover / Quick Peek | 轻量字段说明、引用值信息、信任与来源详情 | Completed / Verified；FR-DQ-3H/4 surface gates |
| Diagnostics / Problems | 当前文档和项目诊断、未知字段和上下文风险分类 | Completed / Verified；diagnostic tests |
| Find References | 当前语义模型内引用定位和导航 | Completed / Verified；reference finder tests |
| Save safety | Save Preflight、encoding/writer、backup/rollback、Dirty 同步 | Completed / Verified；save/writer tests |
| Field Registry | Project > Global > BuiltIn、Manager、学习/导入预览、显式 Apply/Rollback | Completed / Verified；Field Registry stage ledgers |
| 字段数据清理 | runtime BuiltIn 2604 行；uniform inferred templates、auto-extracted、空/未知 quality 和精确重复均为 0 | Completed / Verified；`ContextCapsule_FR_DQ_4.md` |
| 项目级文本查找 | 扫描 Project Explorer 规范 `.ini`，支持大小写/全字/正则、稳定结果和导航 | Completed / Verified；`SEARCH-1-R1_StageLedger.md` |
| 当前文件 Replace All | Preview-first、stale 门禁、内存应用、单次 Undo/Redo、不自动保存 | Completed / Verified；SEARCH-1-R1 full tests 2380/2380 |
| AvalonDock 工作区 | 右/底/浮动工具、返回 Home、默认布局重置、v2 布局持久化 | Completed / Verified；UI-DOCK ledgers |
| DeepSeek provider | V4 Flash/Pro、Flash 默认、生产 Mock 移除、配置与隐私硬化 | Completed / Verified；AI-REL-3，full tests 2171/2171，曾各完成一次授权 live smoke |
| 流式对话 | SSE parser、pipeline delta、同卡增量渲染、取消/断流/超时终态 | Completed / Verified；AI-STREAM-0..3 |
| AI 失败恢复 | 失败轮次隔离、恢复提示词、Failure Taxonomy、安全诊断 | Completed / Verified；AI-REL-1..3 |
| A1 只读分析门面 | 不可变文本/Registry snapshot、UI-neutral diagnostic facts | Completed / Verified；A1 full tests 2355/2355 |
| A2 结构化 Preview | UpsertField/ReplaceFieldValue、ChangeSet、证据和诊断差异 | Completed / Verified；A2 full tests 2419/2419 |
| A3 编辑事务 | 活动 Preview、版本门禁、一次消费、内存 Apply、一个 Undo unit | Completed / Verified；A3 full tests 2436/2436 |
| A4-R1 AI 编辑提案 | 官方 endpoint、明确编辑请求、required tool、本地 Preview、提案卡、显式 Apply | Completed / Verified；A4-R1 build 0/0、tests 2519/2519、IdeOnly package 1049 files |
| HLI-0B 最小 Headless 契约 | 冻结四项能力、Host-only 写入边界和最小纵向迁移方向 | Confirmed / contract completed；未改变运行时 |
| HLI-1A0 依赖锥特征化 | 冻结 Query 22 文件闭包、调用方影响、重复 Section/Reference 语义和迁移门禁 | Completed / Verified；characterization tests 7/7 |
| HLI-1A1 Headless Document Query | Core-only `RA2IniEditor.Application`、Section Get、current-document References Find、typed failure/limits/cancellation | Completed / Verified；Application.Tests 31/31、full 2526/2526、IdeOnly package 1086 |
| HLI-1A2 Headless Diagnostics | Application 唯一 neutral diagnostics/FieldTrust core，IDE 单向 ViewModel adapter | Completed / Verified；Application.Tests 47/47、dependency 149/149、full 2526/2526 |
| HLI-1B Headless Edit Preview | 受限字段 Upsert/Replace、candidate text、ordered changes、operation evidence、diagnostic delta、typed failure/limits/cancellation | Completed / Verified；Application.Tests 82/82、A2/A3/A4 88/88、TextModel 390/390、full 2526/2526 |
| HLI-1C Host Boundary | Workspace generation/active slot、Host projection guard、single-use Apply authority | Completed / Verified；Host 53/53、full 2537/2537 |
| HLI-2A Capability Gateway | 固定四能力 catalog、version/risk/limits、typed Query/Preview façade | Completed / Verified；Gateway 12/12、Application 94/94、full 2537/2537 |
| HLI-2B IDE/AI Gateway Consumer | 唯一 Host adapter 经 typed Gateway Preview；descriptor 驱动发送前资源门禁 | Completed / Verified；HLI-2B/A4/HLI-1C focused 78/78、Application 94/94、full 2547/2547 |
| HLI-2C First High-Level Agent Loop | Gateway Query/Validate -> provider structured plan -> Preview -> explicit Apply -> Problems refresh -> re-Validate；不自动 Save | Completed / Verified；Application 94/94、focused 37/37、full 2549/2549、IdeOnly package 1123 |
| CONTENT-1 Semantic Template | Field Schema/Reference Resolve/CreateSection、模板展开到 canonical Preview、AI required tool | Completed / Verified；allowlist 58、catalog 7、Application 146/146 |
| DIFF-REVIEW-1 Main Workspace Review | 默认显示精确 CandidateText 的只读高亮 Result；保留 unified Changes；有界显示同/跨文档直接关联 Section；文档页签、大纲和循环导航；整体 Apply/Dismiss 不变 | Completed / automated verified；focused 19/19、Application 198/198、IDE 2779/2779；物理视觉验收待执行 |
| AI 非严格工具兼容 | 常见且可唯一解释的字段工具格式漂移规范化；含糊/复合输入继续拒绝 | Completed / Verified；focused 88/88、full 2576/2576 |
| Chat / Work 模式 | 默认 Chat；Chat 不暴露编辑工具；Work 隐式指向当前文档并允许本地结构化 Preview | Completed / Verified；真实 complete-tool proposal + 15 参数探针、focused 71/71、full 2588/2588 |
| Direct-fire 完整链路 | 既有 owner + 新 Weapon/Projectile/Warhead；15 项字段操作，一次原子 Preview/Apply | Completed / Verified；Application 147/147、IDE 2580/2580 |
| BuiltIn RA2 Skills | 18 个领域 Skill；普通请求按 domain/extension/trust 选择，项目能力按 capability 强制选择 source-backed Skill；禁止 scripts/external root | Completed / Verified through CONTENT-2E；production loader/catalog/prompt/pipeline tests |
| Projectile / Warhead Profiles | 既有 Weapon + 新 Arcing 或 Homing Projectile；或新 YR core Warhead；精确互斥与范围门禁 | Completed / Verified；Application 151/151、IDE 2601/2601、package 1177 |
| CONTENT-2D-2 Project Transaction | 1..8 文档纯 Preview、唯一 project session owner、原子内存 Apply/rollback、compound Undo/Redo、多文件 Diff、提交后内存态项目诊断刷新 | Completed / Verified；allowlist 63、catalog 8/methods 10、Application 167/167、IDE 2626/2626 |
| CONTENT-2D-3 headless rules/art consumer | 现有 Techno 的 `rules Image` 与 art object `Image/Cameo` 形成两文档 Project Plan，并可由 headless Project Preview 消费 | Completed / Verified；production BuiltIn schema test 通过；不等于 Work/UI 已接入 |
| ASSET-MANIFEST-1 | 不可变 body SHP/Cameo 需求、绑定状态与 plan-operation 闭合证据 | Completed / Verified；Manifest 无 Apply/Save/文件写入权限 |
| FIELD-REGISTRY-ART-1 | `Cameo/AltCameo/Voxel/Remapable` 的 source-backed ArtObject schema；Cameo 进入 Project Plan | Completed / Verified；body/Cameo binding 均 Proposed |
| ASSET-PROVIDER-1 | 显式二进制输入 -> Manifest-closed 内存 Artifact、SHA-256、有限验证级别 | Completed / Verified；allowlist 77；无文件/模型权限 |
| CONTENT-PROJECT-UI-1 | Work 由 DeepSeek 生成通用 rules/art 结构化计划，Field Registry/Diagnostics 只作 advisory，展示 Project Diff 并显式原子 Apply | Completed / verified through KNOWLEDGE-1-R2；Application 188/188、IDE 2660/2660；真实 project GUI 复验待用户执行 |
| CONTENT-2E / WORK-ENTRY-1 SuperWeapon | Ares UnitDelivery/GenericWarhead 保留 typed headless profile 与能力专用 Skill/检索；生产 Work 对所有 SuperWeapon 统一由模型返回 bounded generic project plan，Host 只固定 rules/art target、Preview/Apply/Save 权限和资源界限 | Completed / automated verified；Application 198/198、IDE 2754/2754；真实 DeepSeek/WPF/游戏内行为仍待人工验收 |

## 3. 已实现但仍有验收边界

| 能力 | 当前状态 | 不能扩大宣称的部分 |
|---|---|---|
| 现代化浅色 UI | 多阶段 XAML、主题、字体、控件模板和布局实现完成 | 若对应 Stage Ledger 标注 visual acceptance pending，则不能称为最终视觉验收通过 |
| Field Registry 二级界面现代化 | M4-R2 与 Visual Fix 自动化门禁完成 | 八个真实 WPF 状态的最终截图验收仍以人工结果为准 |
| Search 浮动窗口 UIA | 打开/隐藏/重开宿主 smoke 通过 | AvalonDock child-HWND 仍阻止外部 UIA 穿透内部控件 |
| 响应式/DPI | 现有 WorkArea、1920/1280 DIP 和主路径自动化证据 | 多显示器混合 DPI 与特定物理设备仍需人工硬件验证 |
| AI 自然语言编辑 | 当前文件与唯一 rules/art pair 的生产 Work 均由模型生成通用结构化计划；Host 生成主工作区 Diff，并在用户确认后原子应用 | 仍不能修改其他项目文件、自动保存或生成素材；真实 DeepSeek 服从度与游戏语义仍需人工验收 |

## 4. 只有部分 Headless 或宿主内实现，尚未成为完整 Agent 能力

| 能力 | 代码事实 | 状态 |
|---|---|---|
| 单文档 Section/Reference query | 已位于 Core-only `RA2IniEditor.Application`，由 typed Gateway 暴露 | Gateway available；尚无独立 Agent/CLI consumer |
| 单文档 Diagnostics query | 唯一算法位于 Core-only Application，由 typed Gateway 暴露 | Gateway available；尚无独立 Agent/CLI consumer |
| 语义 Edit Preview | 唯一 engine 位于 Core-only Application，由 typed Gateway 暴露 | Gateway available；只预览，不 Apply/Save |
| Apply/Undo | 单文档 A3 与项目多文档原子内存事务均在 IDE host 内；项目事务支持 compound Undo/Redo | Host-only by design；无自动 Save |
| Save/Backup/Rollback | 现有服务完整 | Host/user-owned；不是 Agent capability |
| A4 proposal | 当前 WPF 内置 AI 已通过 typed Gateway 生成 semantic Preview | 尚未提供给独立 Agent/CLI；Apply/Save 仍为 Host-only |
| 内容模板 | 9 个 headless 目录项；两个 rules-only SuperWeapon typed profiles 仅供确定性 headless/兼容调用，生产 Work 统一走 model-owned canonical Project Preview | typed 不再拥有生产 Work 内容否决权；不生成素材 |
| 资产 Manifest / Existing Provider | Gateway 可返回 body SHP/Cameo 需求，两者均有 INI operation；独立 provider 可把显式输入解析为内存 Artifact | 当前无格式 codec、项目落盘、生成器、Artifact Registry 或 AI UI routing |
| RA2 Skill | Work 第一轮读取同一 BuiltIn Catalog 的紧凑 Manifest 并推荐 Skill；Host 合并 capability 必选项、校验模式/预算后，将解析出的正文注入第二轮。Chat 保持本地单轮选择 | 只提供知识与约束；不增加 capability 或写入权限，不支持外部安装/热更新 |

审计证据：`Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`。
迁移证据：`Docs/AUTOMATION-HLI-1A1_StageLedger.md`。

## 5. 尚未实现

- 动态 Capability Registry、wire transport 与独立 host（最小固定 typed Gateway 已实现）。
- 独立 Agent、CLI (`ra2tool`) 或进程外协议。
- 通用/持久化语义模板库、完整 Techno 创建、AI/Faction 对象；SuperWeapon 仅 UnitDelivery/GenericWarhead 为 typed complete，其它类型仍是 model-owned proposal；Ares custom armor 与 Phobos trajectory profile。
- 项目级语义引用 API。
- 自动 Apply、自动 Save 或无人值守写入策略。
- Automation Job、Event、Artifact Registry 和可恢复任务状态。
- Cameo/Icon 游戏素材生成流水线。
- VOX 素材的完整项目级端到端入口。当前已有参考图生成、GLB-to-canonical voxel、质量/Agent 几何审阅、
  自然语言上色、Agent 初始语义、人工体素材质蒙版、显式最终候选固化和受验证的 VOX 副本导出；仍没有项目 Apply/Save/自动注册、
  VXL/HVA 写出或游戏验证。
- SHP 动画生成和编码/工具适配。
- 完整素材 Assembly Graph / 自动落盘（body SHP 与 Cameo 已有 INI binding plan，但 Artifact 尚无 Host 持久化事务）。
- `RA2TestHost`、`IRuntimeAdapter` 和运行时回归系统。

## 6. 当前主要限制

- Search 不递归发现 Project Explorer 之外的文件；大于 8 MiB 的延迟文件会跳过并报告。
- Replace All 只限当前文件，不自动保存。
- AI 结构化编辑的生产入口支持当前文件的通用有界 Upsert/Replace 计划，并可在 Preview 中推导模型所需的
  缺失 Section；武器链、双武器、Projectile、Warhead 等能力 ID 只选择 Skill/检索策略，不再选择固定内容模板。
- 旧 complete/skeleton Profile 及其严格命名参数仍保留给确定性 headless/兼容测试，不代表产品 Work 的
  DeepSeek 输入约束，也不得作为生产内容否决器。
- SuperWeapon typed v1 只覆盖 Ares UnitDelivery 与 GenericWarhead 的 headless compatibility profile；生产 Work
  对所有 SuperWeapon 均由模型生成可审阅 project plan。Capability-specific Skills/Host facts 提供依据但不
  替模型决定字段；唯一 rules 目标即可工作，art/素材不是前置条件，也不能宣称所有方案已通过游戏运行时认证。
- 用户可用自然/显示名称描述既有建筑、单位和 Warhead。Work 可在捕获 rules 中按 Section/`Name`/`UIName`
  搜索并最多补查两轮；唯一的同类对象结果绑定为 canonical Section ID。模糊、多义或缺失对象继续补查或
  澄清，不会硬编码对象别名或绕过类型/引用校验。
- proposal 可包含 provider 生成的有界字符串 `message`；该旁路说明经验证后直接丢弃，不能改变
  template id/version、声明参数、字段库门禁、Preview、Apply 或 Save 权限。
- rules/art Work route 由 DeepSeek 产生符号 `rules`/`art` 文档操作，允许创建缺失 Section 和使用字段库
  未知的 mod-specific 字段。Field Registry/Diagnostics 只提供 Caution；Host 仅保留安全 identifier、
  资源上限、captured snapshot、canonical Preview、显式 Apply、stale/single-use 与原子回滚门禁。
- Work 模式先进行一次 DeepSeek 意图分析；若现有项目对象身份不足，可追加最多两次紧凑语义补查，然后再执行结构化预览。第一阶段读取不含正文的 Skill Manifest，并返回本地严格
  校验的意图、Skill 推荐和知识缺口；Host 补齐 capability 必选 Skill、校验模式与 14 KiB 正文预算，
  第二阶段才按 allowlist 进入既有 authoring tool 或 advisory。Chat 仍只调用一次。第一阶段失败时不会发送
  第二次。仅当第二阶段出现类型化、白名单内且可由模型修正的结构化失败时，IDE 才追加一次非流式修复调用；
  正常 Work 为 2..4 次 provider 调用；若随后触发既有一次结构化修复，绝对上限 5 次。不会重跑第一阶段或 Skill 选择，重复查询会立即停止。
- Work 调用共享同一受限会话、当前主题和 `current/rules/art` 捕获快照投影。每个查询批次最多 8 个
  Section/引用/对象搜索事实，最多两轮补查；Host 仅对原捕获快照执行本地只读查询，再将有界结果交给执行。
  模型不能提供路径、枚举目录或刷新快照。修复调用只能复用这组已冻结事实。
- rules/art 第二阶段固定注入 `ra2-rules-art-binding`，不会因第一阶段把 domain 描述为 techno 或
  art-animation 而丢失跨文档知识。`Art/Body/Cameo` 是角色名，不得直接写成 rules 字段；不同
  ArtSection/BodyAsset 对 Infantry/Vehicle/Aircraft 需要已建立的 Phobos `ArtImageSwap=true`，否则模型
  应澄清而不是猜测或静默打开全局开关。
- 双链 profile 不表示循环或交替开火；这类请求在 portable Gattling schema 完成前会在发送前拒绝。
- Arcing 与 Homing 不会混合；Phobos `Trajectory.*`、Vertical、Airburst/Splits 请求会本地拒绝。
- YR core Warhead 固定 11 槽 `Verses`，存在 `[ArmorTypes]` 时拒绝，避免冒充 Ares custom-armor 完整配置。
- explicit clarification 即使混入 proposal-shaped 参数也只显示有界 message，不创建 Plan/Preview；
  完整对象请求未指定调参时，模型应生成可见、可复核的保守草案值，而不是无条件请求澄清。
- 超过 8,388,608 UTF-16 字符的当前文件不能进入 AI 结构化编辑；明确编辑请求会在模型发送前本地拒绝，普通咨询仍可发送截断上下文。
- Custom endpoint 仅允许 advisory，不获得编辑 tool 权限。
- 通用传输重试和模型 fallback 未实现；超时、网络、取消、配置、过期上下文、资源或安全失败不会触发结构化修复。
- 仓库没有真实 `.ini` corpus，字段隔离后的真实项目 Unknown Key 增量未知。
- 视觉验收和混合 DPI 仍存在明确的人工验证项。

## 7. 最新可信验证基线

当前最新完整实现证据来自 ASSET-VOX-1E：

```text
dotnet restore: Passed / up-to-date
dotnet build Debug --no-restore: Passed，0 errors，1 个既有 Field Registry test nullable warning
dotnet test RA2IniEditor.Application.Tests: Passed 249/249
dotnet test RA2IniEditor.Tests: Passed 2787/2787
dotnet test RA2IniEditor.AssetHost.Tests: Passed 47/47
Existing 1D Body style acceptance: Passed 1/1，20,261 cells geometry/occupancy unchanged
IdeOnly clean package: 见 `Docs/ASSET-VOX-1E_StageLedger.md`
Real DeepSeek style compile / product GUI / game smoke: NotRun
```

不同子系统的历史验证数量不同，应以各自 Stage Ledger 为证据，不把最新全量
测试数量倒推为所有旧阶段都在同一环境重新验收。

HLI-1A1/1A2/1B 与 HLI-2A/2B/2C 使 Query、Diagnostics 和 Preview 可由普通 `net8.0` 调用方及
内置 AI 经 typed Gateway 消费，并证明 Host explicit Apply 后可立即刷新 Problems；仍没有独立
Agent/CLI 或 public Apply/Save，不能据此宣称完整 Agent 已可用。

ASSET-VOX-1B 的独立验证基线为 focused 27/27、Application 225/225、IDE 2779/2779、Debug build
0 errors / 1 个既有 Field Registry nullable warning。该证据只证明纯算法与格式边界，不证明 VXLSE GUI、
视觉质量、HVA 或游戏运行结果。

ASSET-VOX-1E 在 1B/1D 基线上新增 headless natural-language style source/compiler/cache/colourizer/review package。
它证明已有 Body 候选可确定性完成 palette 分层，但没有 UI/Work 入口；玻璃、轮胎、标识和 remap 仍需显式
蒙版，且 VXL/HVA、normals、项目写入和游戏运行结果仍未完成。
## ASSET-VOX-1E-UI

- Available: read-only project-contained single-model VOX selection or single-Section VXL selection with an explicit
  project-contained Westwood PAL, local original SliceStack, inherited natural-language style
  sources, explicit structured style compilation, coloured/region/palette review, plan/risk projection and in-session
  acceptance.
- Ordinary VOX palettes do not require an RA2 remap range. Text-only team-colour intent is kept as an unresolved review
  item; only an explicit executable remap still requires valid remap indices and mask evidence.
- Available in the current workspace: accepted Agent seed plus region-level and sparse 3D surface-brush semantic masks for
  glass, tyre, metal, light, accent and explicitly human-approved remap roles. Cell edits are hash-bound and undoable.
- Not available: persistent mask interchange, hidden/internal slice painting, review-package export, project Apply/Save, VXL/HVA/normals generation or game
  validation. No provider call occurs until the user explicitly compiles.

## ASSET-VOX-1F-CORE-1

- Available internally: deterministic visible-face projection with palette identity, exact RA2/TS normal direction
  palettes, bounded geometry-derived normal estimation/smoothing/quantization, source binding, cancellation and resource limits.
- Both MagicaVoxel VOX and Westwood VXL use these algorithms after decoding to `Ra2VoxelSceneSnapshot`.
- Normal fields are review data only. Existing VXL normal preservation, normal visualization, VXL/HVA writing, project
  Apply/Save and game validation are not available.

## ASSET-VOX-1E-UI-3D

- Original, coloured-result and geometry-region review modes now use one native WPF interactive 3D viewport with bounded
  orbit, pan, zoom and fit/reset. Palette remains an exact 2D swatch and SliceStack remains an explicit diagnostic fallback.
- The scene is a cancellable, generation-guarded, frozen WPF projection of the canonical snapshot's exposed faces. It is
  presentation-only and works identically after VOX or single-Section VXL decode.
- Lighting is geometry-only review lighting. VXL normal-index display, multi-part composition, VXL/HVA writing, project
  Apply/Save and game validation remain unavailable.

## ASSET-VOX-2A

- Available internally: deterministic quality/silhouette/symmetry facts, exact thin-feature coordinate protection,
  bounded supersampled conversion, one cleanup pass, optional local symmetry candidate, normal comparison and semantic
  review regions. Source mesh/provider output is never edited.
- Available internally: palette-only body contrast candidate that preserves explicit, semantic and remap selections.
- Available as a headless seam: one-to-three structured DeepSeek diagnosis/plan/review rounds with early stop and exact
  in-memory cache. It is fake-client verified and not yet wired to the product or live provider.
- Verification baseline: Application 264/264, IDE 2807/2807, AssetHost 47/47; build 0 errors / one pre-existing warning.
- Superseded 2A baseline note: UI switching and live semantic symmetry were not available before the 2A-UI/2B product
  wiring described below. VXL/HVA writing, project Apply/Save and game validation remain unavailable.

## ASSET-VOX-2B capability update

- Available in the existing Voxel Style workspace: local Direct/Refined generation with no provider call, followed by an
  explicit two-request AI structure-recognition action.
- Available for review: host-owned structural-core/attachment/thin/uncertain partition, fixed 3D overlay, two-round
  agreement summary and a deterministic constrained-symmetry candidate when every hard gate passes.
- Failure isolation: timeout, cancellation, malformed output, unavailable configuration or a failed symmetry gate leaves
  Direct/Refined intact and publishes no partial model result.
- Real-sample correction: highly fragmented mismatch/protection evidence is summarized into at most 50 deterministic
  semantic regions while retaining every coordinate internally. The workspace reports the ready region count instead of
  silently disabling structure recognition on the certified Body sample.
- Not yet available: real-provider acceptance, authoritative material segmentation or production-quality colouring,
  VXL/HVA writing, project Apply/Save or game validation.
- Visual/provider correction: local SurfacePolish provides a stronger admitted candidate when safe; Difference focuses on
  green/red geometry deltas over translucent unchanged geometry. Equivalent DeepSeek tool JSON representation is accepted,
  while semantic identity and complete region coverage remain strict.
- Selection hardening: automatic Refined selection now requires measurable roughness reduction and ranks roughness before
  cleanup counts. All three candidate facts are visible in `组合审阅`; an aggressive cleanup can remain review-only.
  Difference does not require a protection mask. Provider-format and evidence mismatch failures are now reported with a
  concrete local reason. Live-provider success is still not certified by this local correction.

## ASSET-VOX-3A — session-only reference-image generation

- Available: choose one PNG/JPEG reference, probe the fixed Tencent Hunyuan 3D bundle, review a per-run consent dialog,
  generate one GLB candidate and convert it to the existing 3D voxel review in memory.
- Available after generation: existing local quality candidates, natural-language style preview and explicit structure review.
- Not available: text-only geometry generation, automatic retry, project Apply/Save, VXL/HVA export or game-ready claim.
- Live provider/UI acceptance is pending a separately approved manual probe.

## ASSET-VOX-3B — accepted candidate and verified VOX export

- Available: explicitly freeze Original, Direct, admitted Refined, successful Agent Geometry, Styled or Contrast Styled as
  one immutable session candidate. Review-only Difference/Structure/Mask/Palette modes cannot become export authority.
- Available: export that frozen candidate to a MagicaVoxel `.vox` copy. The IDE reuses the canonical codec, flushes a
  same-directory temporary file, decodes/re-encodes it byte-exactly, and only then publishes atomically.
- Safety: the currently loaded source VOX cannot be overwritten; another existing target requires the native overwrite
  confirmation. Failure/cancellation keeps the previous target and cleans the temporary file.
- Still unavailable: project Apply/Save/registration, multi-part Body/Turret/Barrel materialization, VXL/HVA writing and
  game validation. Physical WPF Save-As smoke remains manual.

## ASSET-VOX-3C/3D — continuous working geometry and center-seam repair

- Available: an explicitly adopted Refined/Agent candidate becomes the sole revisioned working baseline for later quality,
  Agent, style, freeze and VOX-export work; GLB remains evidence rather than silently restoring an older branch.
- Available: structure evidence exposes bounded `seam-gap-*` targets for exactly one empty cell on an integer X symmetry
  plane or two empty cells on a half-cell plane when occupied anchors exist on both sides.
- Available: DeepSeek may select `bridge_center_gap`; the Host then fills only those bound coordinates and applies the
  existing topology/volume/silhouette safety gates. The Host never auto-fills gaps.
- Excluded: three-cell/off-axis/arbitrary holes, persistent history, automatic adoption, project Apply/Save and VXL/HVA.

## ASSET-VOX-4B-STROKE-1 — continuous semantic surface painting

- Available: hold and drag the left mouse button to paint or erase exact visible-surface voxel seeds; fast movement is
  resampled at a bounded 4-DIP interval and gaps/background are never inferred as hidden geometry.
- Available: each stroke is atomic, produces at most one undo item and uses the existing brush size/mirror/erase rules.
- Available: switch semantic review between fixed Part and Material annotation palettes with a matching compact legend.
- Safety: drag preview is session-only presentation; annotation colours never become VOX palette colours. Scene/capture/
  mode/hash transitions cancel an unfinished stroke without partial edits.
- Pending: physical WPF pointer and 100%/125% DPI acceptance. Hidden/internal painting, fill/lasso and persistent masks remain unavailable.
## ASSET-VOX-4D：语义分划持久化

- 体素语义页可显式保存和载入项目内 `.semantic.json`。
- sidecar 分层保留已接受 Agent 建议、人工区域覆盖和人工体素画笔覆盖，不把 AI 来源提升为人工来源。
- 只有当前工作模型的 canonical hash、确定性区域 evidence hash 和人工层 hash 全部匹配时才会恢复。
- 载入失败不会改变当前会话；载入成功会清空旧画笔撤销/重做并失效旧着色候选。
- 当前没有自动保存/自动载入、跨模型迁移、Shell 关闭保护或 VOX/VXL/HVA 内嵌。
