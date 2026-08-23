# RA2IniEditor.IDE 当前能力矩阵

更新时间：2026-08-23
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
| CONTENT-UI-1 Main Workspace Diff | 临时只读 unified Diff、关闭恢复、Blocked/Stale、整体 Apply/Dismiss、资源门禁 | Completed / Verified；focused 20/20、full 2568/2568；人工视觉验收待执行 |
| AI 非严格工具兼容 | 常见且可唯一解释的字段工具格式漂移规范化；含糊/复合输入继续拒绝 | Completed / Verified；focused 88/88、full 2576/2576 |
| Chat / Work 模式 | 默认 Chat；Chat 不暴露编辑工具；Work 隐式指向当前文档并允许本地结构化 Preview | Completed / Verified；真实 complete-tool proposal + 15 参数探针、focused 71/71、full 2588/2588 |
| Direct-fire 完整链路 | 既有 owner + 新 Weapon/Projectile/Warhead；15 项字段操作，一次原子 Preview/Apply | Completed / Verified；Application 147/147、IDE 2580/2580 |
| BuiltIn RA2 Skills | 15 个领域 Skill，按 domain/extension/trust 分层按需注入；禁止 scripts/external root | Completed / Verified；production loader + catalog/prompt tests |
| Projectile / Warhead Profiles | 既有 Weapon + 新 Arcing 或 Homing Projectile；或新 YR core Warhead；精确互斥与范围门禁 | Completed / Verified；Application 151/151、IDE 2601/2601、package 1177 |

## 3. 已实现但仍有验收边界

| 能力 | 当前状态 | 不能扩大宣称的部分 |
|---|---|---|
| 现代化浅色 UI | 多阶段 XAML、主题、字体、控件模板和布局实现完成 | 若对应 Stage Ledger 标注 visual acceptance pending，则不能称为最终视觉验收通过 |
| Field Registry 二级界面现代化 | M4-R2 与 Visual Fix 自动化门禁完成 | 八个真实 WPF 状态的最终截图验收仍以人工结果为准 |
| Search 浮动窗口 UIA | 打开/隐藏/重开宿主 smoke 通过 | AvalonDock child-HWND 仍阻止外部 UIA 穿透内部控件 |
| 响应式/DPI | 现有 WorkArea、1920/1280 DIP 和主路径自动化证据 | 多显示器混合 DPI 与特定物理设备仍需人工硬件验证 |
| AI 自然语言编辑 | 明确字段修改、关系骨架、direct-fire 武器链、Techno 双链及独立 Projectile/Warhead profile 可形成真实提案、主工作区 Diff 并应用；成功后刷新 Problems | 仍不是任意对象、任意 patch 或多文件 Agent；Ares custom armor 与 Phobos trajectory 尚未 profile 化 |

## 4. 只有部分 Headless 或宿主内实现，尚未成为完整 Agent 能力

| 能力 | 代码事实 | 状态 |
|---|---|---|
| 单文档 Section/Reference query | 已位于 Core-only `RA2IniEditor.Application`，由 typed Gateway 暴露 | Gateway available；尚无独立 Agent/CLI consumer |
| 单文档 Diagnostics query | 唯一算法位于 Core-only Application，由 typed Gateway 暴露 | Gateway available；尚无独立 Agent/CLI consumer |
| 语义 Edit Preview | 唯一 engine 位于 Core-only Application，由 typed Gateway 暴露 | Gateway available；只预览，不 Apply/Save |
| Apply/Undo | A3 在 IDE host 内完整 | Host-only by design |
| Save/Backup/Rollback | 现有服务完整 | Host/user-owned；不是 Agent capability |
| A4 proposal | 当前 WPF 内置 AI 已通过 typed Gateway 生成 semantic Preview | 尚未提供给独立 Agent/CLI；Apply/Save 仍为 Host-only |
| 内容模板 | 6 个目录项：skeleton、single/dual direct-fire、Arcing/Homing Projectile、YR core Warhead，均走 canonical Preview | complete 要求唯一既有 owner，仍不维护注册列表或素材 |
| RA2 Skill | BuiltIn Markdown 通过内部 loader 按领域选取并进入 prompt | 只提供知识与约束；不增加 capability 或写入权限，不支持外部安装/热更新 |

审计证据：`Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`。
迁移证据：`Docs/AUTOMATION-HLI-1A1_StageLedger.md`。

## 5. 尚未实现

- 动态 Capability Registry、wire transport 与独立 host（最小固定 typed Gateway 已实现）。
- 独立 Agent、CLI (`ra2tool`) 或进程外协议。
- 通用/持久化语义模板库、完整 Techno 创建、AI/SuperWeapon 等对象和注册列表维护；Ares custom armor 与 Phobos trajectory profile。
- 项目级语义引用 API。
- 项目级或多文件编辑事务。
- 自动 Apply、自动 Save 或无人值守写入策略。
- Automation Job、Event、Artifact Registry 和可恢复任务状态。
- Cameo/Icon 游戏素材生成流水线。
- VOX 生成、SliceStack 导出、VXLSE III 导入包。
- SHP 动画生成和编码/工具适配。
- 素材与 INI 的 Assembly Graph / 自动绑定。
- `RA2TestHost`、`IRuntimeAdapter` 和运行时回归系统。

## 6. 当前主要限制

- Search 不递归发现 Project Explorer 之外的文件；大于 8 MiB 的延迟文件会跳过并报告。
- Replace All 只限当前文件，不自动保存。
- AI 结构化编辑支持当前文件的受限字段 Upsert/Replace、三 Section 骨架、单槽 direct-fire 完整链，
  现有 Techno 的 Primary/Secondary 双链完整 profile（6 Sections、30 operations），以及绑定既有 Weapon
  的 Arcing/Homing Projectile 和 YR core Warhead profile。
- complete profile 工具使用命名参数对象与原生 number/boolean schema；adapter 只兼容可唯一解释的
  scalar 形态漂移，未知参数、嵌套对象/数组、缺失必填参数和低可信字段仍会拒绝。
- proposal 可包含 provider 生成的有界字符串 `message`；该旁路说明经验证后直接丢弃，不能改变
  template id/version、声明参数、字段库门禁、Preview、Apply 或 Save 权限。
- Work 模式每次发送使用两次 DeepSeek 调用：第一阶段只返回本地严格校验的意图事实包，第二阶段
  才按 allowlist 进入既有 authoring tool 或 advisory。Chat 仍只调用一次。第一阶段失败时不会发送
  第二次；当前没有自动重试或 fallback，因此 Work 的延迟与调用成本高于 Chat。
- 双链 profile 不表示循环或交替开火；这类请求在 portable Gattling schema 完成前会在发送前拒绝。
- Arcing 与 Homing 不会混合；Phobos `Trajectory.*`、Vertical、Airburst/Splits 请求会本地拒绝。
- YR core Warhead 固定 11 槽 `Verses`，存在 `[ArmorTypes]` 时拒绝，避免冒充 Ares custom-armor 完整配置。
- explicit clarification 即使混入 proposal-shaped 参数也只显示有界 message，不创建 Plan/Preview；
  完整对象请求未指定调参时，模型应生成可见、可复核的保守草案值，而不是无条件请求澄清。
- 超过 8,388,608 UTF-16 字符的当前文件不能进入 AI 结构化编辑；明确编辑请求会在模型发送前本地拒绝，普通咨询仍可发送截断上下文。
- Custom endpoint 仅允许 advisory，不获得编辑 tool 权限。
- 自动重试和模型 fallback 未实现。
- 仓库没有真实 `.ini` corpus，字段隔离后的真实项目 Unknown Key 增量未知。
- 视觉验收和混合 DPI 仍存在明确的人工验证项。

## 7. 最新可信验证基线

当前最新完整实现证据来自 CONTENT-2B：

```text
dotnet restore: Passed
dotnet build Debug --no-restore: Passed, 0 warnings, 0 errors
focused IDE compile note: existing CS8602 warning at BuiltInFieldRegistryPackLoaderTests.cs:1960
dotnet test RA2IniEditor.Application.Tests: Passed 151/151
dotnet test CONTENT-2B focused: Passed 16/16 + 28/28
dotnet test RA2IniEditor.Tests: Passed 2601/2601
IdeOnly clean package: Passed, 1177 files
Computer control / real DeepSeek / physical DPI visual smoke: NotRun
```

不同子系统的历史验证数量不同，应以各自 Stage Ledger 为证据，不把最新全量
测试数量倒推为所有旧阶段都在同一环境重新验收。

HLI-1A1/1A2/1B 与 HLI-2A/2B/2C 使 Query、Diagnostics 和 Preview 可由普通 `net8.0` 调用方及
内置 AI 经 typed Gateway 消费，并证明 Host explicit Apply 后可立即刷新 Problems；仍没有独立
Agent/CLI 或 public Apply/Save，不能据此宣称完整 Agent 已可用。
