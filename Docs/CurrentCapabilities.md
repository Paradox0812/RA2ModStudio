# RA2IniEditor.IDE 当前能力矩阵

更新时间：2026-08-22  
本页只记录有源码、阶段台账和验证证据支持的当前能力。未来目标见
`Docs/ProductVisionAndRequirements.md`。

## 1. 总结

当前产品已经是可运行的 source-first INI IDE，并具备真实搜索、字段智能、
诊断、保存安全、DeepSeek 流式助手、受限当前文件结构化编辑闭环，以及可由
`net8.0` 调用方独立消费的 Document Query、Diagnostics 和 Edit Preview 切片。

它还不是最终的自然语言 Mod 生产 Agent：最小进程内 Capability Gateway 已实现，但内置
AI consumer、独立 Agent、素材/图标生成、SHP/VXL 流水线、Job Runtime 和
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

## 3. 已实现但仍有验收边界

| 能力 | 当前状态 | 不能扩大宣称的部分 |
|---|---|---|
| 现代化浅色 UI | 多阶段 XAML、主题、字体、控件模板和布局实现完成 | 若对应 Stage Ledger 标注 visual acceptance pending，则不能称为最终视觉验收通过 |
| Field Registry 二级界面现代化 | M4-R2 与 Visual Fix 自动化门禁完成 | 八个真实 WPF 状态的最终截图验收仍以人工结果为准 |
| Search 浮动窗口 UIA | 打开/隐藏/重开宿主 smoke 通过 | AvalonDock child-HWND 仍阻止外部 UIA 穿透内部控件 |
| 响应式/DPI | 现有 WorkArea、1920/1280 DIP 和主路径自动化证据 | 多显示器混合 DPI 与特定物理设备仍需人工硬件验证 |
| AI 自然语言编辑 | 明确、受支持的当前文件字段修改可形成真实提案并应用 | 不是任意指令、任意 patch、Section 模板或多文件 Agent |

## 4. 只有部分 Headless 或宿主内实现，尚未成为完整 Agent 能力

| 能力 | 代码事实 | 状态 |
|---|---|---|
| 单文档 Section/Reference query | 已位于 Core-only `RA2IniEditor.Application`，由 typed Gateway 暴露 | Gateway available；尚无独立 Agent/CLI consumer |
| 单文档 Diagnostics query | 唯一算法位于 Core-only Application，由 typed Gateway 暴露 | Gateway available；尚无独立 Agent/CLI consumer |
| 语义 Edit Preview | 唯一 engine 位于 Core-only Application，由 typed Gateway 暴露 | Gateway available；只预览，不 Apply/Save |
| Apply/Undo | A3 在 IDE host 内完整 | Host-only by design |
| Save/Backup/Rollback | 现有服务完整 | Host/user-owned；不是 Agent capability |
| A4 proposal | 当前 WPF 内置 AI 可消费 | 尚未通过 Capability Gateway 提供给独立 Agent/CLI |

审计证据：`Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`。
迁移证据：`Docs/AUTOMATION-HLI-1A1_StageLedger.md`。

## 5. 尚未实现

- 动态 Capability Registry、wire transport 与独立 host（最小固定 typed Gateway 已实现）。
- 独立 Agent、CLI (`ra2tool`) 或进程外协议。
- 通用语义模板和完整 Section/对象创建。
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
- AI 结构化编辑只支持当前文件的受限字段 Upsert/Replace。
- Custom endpoint 仅允许 advisory，不获得编辑 tool 权限。
- 自动重试和模型 fallback 未实现。
- 仓库没有真实 `.ini` corpus，字段隔离后的真实项目 Unknown Key 增量未知。
- 视觉验收和混合 DPI 仍存在明确的人工验证项。

## 7. 最新可信验证基线

当前最新完整实现证据来自 HLI-2A：

```text
dotnet restore: Passed
dotnet build Debug: Passed, 0 errors; 1 existing warning in untouched test file
dotnet test RA2IniEditor.Application.Tests: Passed 94/94
dotnet test Gateway focused: Passed 12/12
dotnet test HLI-1C boundary: Passed 11/11
dotnet test RA2IniEditor.Tests: Passed 2537/2537
IdeOnly clean package: Passed, 1115 files
UI / computer control: NotRun because HLI-2A has no UI behavior change
```

不同子系统的历史验证数量不同，应以各自 Stage Ledger 为证据，不把最新全量
测试数量倒推为所有旧阶段都在同一环境重新验收。

HLI-1A1/1A2/1B 与 HLI-2A 使 Query、Diagnostics 和 Preview 可由普通 `net8.0` 调用方经
typed Gateway 消费；内置 AI 尚未切换为 Gateway consumer，也没有独立 Agent/CLI 或 public
Apply/Save，不能据此宣称完整 Agent 已可用。
