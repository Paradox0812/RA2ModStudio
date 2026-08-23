# AUTOMATION-CONTENT-UI-1 Main Workspace Change Preview Final Contract

契约日期：2026-08-23  
状态：Completed / Verified（视觉人工验收待用户后续执行）  
父契约：`Docs/AUTOMATION-CONTENT-1_SemanticTemplateContinuousFinalContract.md`

## 1. 用户可见目标

AI 形成成功、仍有效的结构化修改提案后，主编辑工作区打开一个临时只读 Diff 文档，默认标题：

```text
修改预览：<当前文件名>
```

它以统一差异视图展示原文与 `CandidateText`，提供整份提案的“应用全部”“放弃修改”和
“返回源文件”。右侧 AI 提案卡保留摘要和恢复入口，但不再承担主要差异审阅。

## 2. 不可破坏的权限边界

- 当前源文本仍是唯一文档事实源；Diff 文档只消费 immutable projection。
- Diff 文档不得编辑、不得直接写 TextEditor、不得持有 TransactionPort。
- “应用全部”必须调用现有 `Ra2AiAuthoringCoordinator.ApplyConfirmed`；不得建立第二条 Apply 路径。
- 应用仍是 single-use、一个 Undo 单元、Dirty、Problems refresh、no automatic Save。
- 首版不支持逐行/逐块接受或拒绝；差异块只能导航和审阅。
- 关闭 Diff 标签只隐藏视图，不等于 Dismiss；“放弃修改”才调用现有 Dismiss。
- 文档、字段库、提案代际变化后 projection 立即 stale，所有 Apply 入口禁用。

## 3. 复用与数据模型

| 数据 | Owner | Lifetime | Mutability / persistence |
|---|---|---|---|
| `Ra2IniEditPreview` | existing IDE authoring | proposal-bound | immutable / none |
| source/candidate lines | Diff projection builder | projection build | immutable / none |
| Diff rows/hunks | IDE presentation model | active proposal lifetime | immutable / none |
| selected/visible document | Shell presentation | view lifetime | mutable UI state / not persisted |
| Apply/Dismiss state | existing coordinator + proposal VM | active proposal lifetime | existing lifecycle / none |

禁止把 Diff projection 加入 Application public API、document snapshot、layout persistence 或保存格式。

## 4. 精确视觉结构

主工作区预览由一个临时 AvalonDock `LayoutDocument` 承载，不新增底部或右侧 ToolWindow：

```text
┌ 修改预览：rulesmd.ini ───────────────────────────────────────┐
│ AI 修改预览   3 项更改   +3 / -1   警告 0              [×] │  32 DIP
│ [返回源文件]                         [放弃修改] [应用全部] │  36 DIP
├ old │ new │ change marker │ unified diff text ─────────────┤
│  12 │  12 │               │ [E1]                           │
│  13 │     │ −             │ Strength=125                  │
│     │  13 │ +             │ Strength=150                  │
└─────────────────────────────────────────────────────────────┘
```

### 4.1 固定尺寸与响应规则

- 顶部状态行 `MinHeight=32`，操作行 `MinHeight=36`；主体占其余空间。
- old/new 行号列各 `44 DIP`；差异标记列 `24 DIP`；正文列为 `*`。
- 1920×1080 默认显示标题、统计、诊断摘要和三个操作。
- 可用宽度 `< 900 DIP` 时隐藏详细诊断文本，只保留错误/警告数字；操作不隐藏。
- 可用宽度 `< 640 DIP` 时“返回源文件”改为图标+Tooltip，仍保留自动化名称。
- 主体允许水平/垂直滚动；正文使用等宽字体和 no-wrap，不通过压缩字号适配。

### 4.2 颜色和状态

- 新增行：复用项目现有绿色语义软背景；左侧 `+`。
- 删除行：复用项目现有红色语义软背景；左侧 `−`。
- 未修改上下文：透明背景；正文使用主文本色。
- 差异块标题：Accent soft background；显示 `@@ oldStart,oldCount +newStart,newCount @@`。
- Stale/Blocked：状态条使用 warning/danger 语义色，主体仍可读，应用按钮禁用。
- 不引入图片资产、渐变、阴影、动画或第三方 Diff 控件。

### 4.3 AutomationId

保留全部 `AiAssistant.EditProposalCard.*`，新增：

```text
Shell.AuthoringDiff.Document
Shell.AuthoringDiff.StatusBar
Shell.AuthoringDiff.Stats
Shell.AuthoringDiff.DiagnosticSummary
Shell.AuthoringDiff.ReturnToSourceButton
Shell.AuthoringDiff.DismissButton
Shell.AuthoringDiff.ApplyAllButton
Shell.AuthoringDiff.ScrollViewer
Shell.AuthoringDiff.Rows
Shell.AuthoringDiff.StaleNotice
AiAssistant.EditProposalCard.OpenDiffButton
```

## 5. Diff 算法与资源门禁

- 输入必须来自同一成功 Preview 的 `Snapshot.Text` 和 `CandidateText`。
- 使用确定性 line-based diff；换行拆分必须保留最终空行语义。
- 相邻变化与最多 3 行上下文组成 hunk；hunk 顺序严格按文档位置。
- 最大输入：沿用 Preview 的 8 MiB 字符上限；UI Diff 另设最多 200,000 行。
- 最大输出：20,000 个可视行、2,000 个 hunk；超限不返回 partial projection。
- 支持 `CancellationToken`；后台计算完成前显示“正在生成差异预览”。
- 典型大文档门禁不设置易波动的毫秒断言，但必须证明算法不在 UI Dispatcher 内同步分配
  `O(N×M)` 全矩阵。
- 不能引入外部 NuGet；优先复用已有 span/change 信息缩小比较范围。若采用新算法，必须为 internal、
  可取消，并提供 `TooLarge/ResultLimitExceeded/Canceled/InvalidPreview` 失败种类。
- Diff 失败时保留 AI 摘要卡及整体 Apply；显示明确失败原因，不冻结或清空提案。

## 6. 生命周期与竞态

### 6.1 打开

- 成功提案创建后自动打开并激活 Diff 文档。
- 同一 `ProposalId/PreviewId` 重复打开时复用/重建同一内容，不创建多个标签。
- 新提案到达时旧提案 `Superseded`，旧 Diff 只读标记失效并关闭/替换。

### 6.2 关闭与恢复

- 用户关闭标签：仅释放 View；proposal/coordinator 状态保持 Ready/Blocked。
- AI 卡片出现“查看更改”按钮；仍有效时可重建 Diff 文档。
- 用户点击“放弃修改”：调用 Dismiss，关闭 Diff，卡片显示已忽略，不能恢复。
- Shell 关闭、切换文档、文本变化或字段库 revision 变化：走既有 Invalidate，关闭或标记 stale。

### 6.3 应用

- 点击应用前依次校验 `ProposalId`、`PreviewId`、`DocumentId`、`EditRevision`、
  `FieldRegistryRevision` 和 coordinator active identity。
- 成功：现有事务提交、一个 Undo、Problems refresh；Diff 关闭并聚焦源文档对应变更起点。
- 失败：源文本不变；Diff 保持可读并显示失败/失效状态；不得静默重试。

## 7. 允许与禁止文件

允许新增/修改：

```text
RA2IniEditor.IDE/AuthoringDiff/**
RA2IniEditor.IDE/ViewModels/AI/Ra2AiEditProposalViewModel.cs
RA2IniEditor.IDE/Views/AI/Ra2AiEditProposalView.xaml(.cs)
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/Themes/IdeWorkspaceStyles.xaml（仅新增/复用 Diff 样式）
RA2IniEditor.Tests/IDE/*AuthoringDiff*Tests.cs
RA2IniEditor.Tests/IDE/Ra2AiEditProposalViewModelTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAuthoringShellBoundaryTests.cs
Docs/** 当前阶段治理文档
```

Shell 允许的唯一结构变化：在现有主 `LayoutDocumentPane` 中动态增加/移除一个临时 Diff
`LayoutDocument`，或在现有 source document 内容内增加等价的双状态宿主。不得修改菜单、工具栏、
右侧/底部区域、默认 Dock 比例、布局恢复注册表或 layout persistence schema。

禁止：parser、Field Registry 数据/priority、Diagnostics rule、Completion/Hover、Save/Backup/Rollback、
project/dependency 文件、legacy、搜索和素材模块。

## 8. 测试与验收

- Diff：insert/delete/replace、首尾行、混合换行、无最终换行、多 hunk、稳定顺序、取消、资源超限。
- Projection：统计、行号、hunk header、诊断摘要、stale/blocked/apply state。
- Lifecycle：自动打开、重复打开、关闭后恢复、Dismiss 不可恢复、新提案 supersede、文档/registry stale。
- Authority：Diff 控件无直接 editor/TransactionPort 写入；Apply/Dismiss 只经现有 coordinator。
- Compatibility：右侧卡片既有摘要、Apply、Dismiss、AutomationId 保持；普通字段提案同样可打开 Diff。
- Shell boundary：默认 Dock 比例、工具窗注册、layout store schema 零变化。
- Build、完整 non-UI tests、clean package 必须通过；电脑操控与真实 DeepSeek 不是必选门禁。

## 9. 停止条件

- 需要逐块提交、跨文档 Diff、Diff 持久化或新的 Apply authority；
- 需要修改 AvalonDock layout persistence/默认比例；
- 无法在不阻塞 UI 的情况下满足资源上限；
- 现有 Preview 缺少可靠原文/候选/identity，且修复会升为 R4；
- XAML 实际效果需要超出本契约的自由重设计。

## 10. 完成定义

只有主工作区真实显示可审阅统一 Diff、关闭可恢复、失效可靠、整体应用复用既有事务且所有门禁通过，
`CONTENT-UI-1` 才能标记 Completed。仅有聊天卡片或文本摘要不得算完成。

## 11. 实施与验证结果

- 已实现临时 `LayoutDocument`、自动打开、同提案复用、关闭释放、卡片恢复、Dismiss/Invalidate 终止。
- VISUAL-FIX1 修复布局恢复后的宿主定位：Shell 通过 `ShellDockLayoutSession` 解析当前
  `Document.Source`，不再从已被 AvalonDock 替换的 XAML 初始模型读取 `Parent`；自动打开和
  “查看更改”因此均进入当前主 `LayoutDocumentPane`。重置布局和关闭 Shell 前先释放临时 Diff，
  仍不将其写入布局持久化。VISUAL-FIX1 定向 Diff/layout 测试 13/13、完整 IDE non-UI
  2576/2576、Debug build 0 errors（1 个既有字段库测试可空性 warning）、clean package 1147 files。
- Diff 投影基于现有有序 TextChange，保持线性有界，不构造 `O(N×M)` 矩阵；候选文本不一致会 fail closed。
- Blocked/Stale 状态、成功后返回源文件 caret、900/640 DIP 紧凑规则及全部 AutomationId 已锁定。
- 聚焦 Diff/UI/Proposal/Shell 门禁 20/20；完整 non-UI 2568/2568；Debug build 0 warning / 0 error。
- 物理屏幕、混合 DPI 和真实 WPF 截图未自动验收；用户可在后续手动视觉验收，不回写为自动通过。
