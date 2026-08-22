# SEARCH-1-R1 连续契约

状态：已确认，可连续执行  
日期：2026-07-23  
风险等级：R3  
治理模式：Deferred Governance Queue

## 1. 功能目标

1. 将现有 Search Dock 从视觉占位改为可用的项目级只读查找。
2. 支持当前文件和整个已打开项目两种范围。
3. 支持区分大小写、全字匹配和 .NET 正则表达式。
4. 查找结果能够导航到对应文件、行、列和字符位置。
5. 在同一 Search Dock 内提供当前文件替换预览与 Replace All。
6. Replace All 只修改当前编辑会话，不自动保存，并作为一个语义事务撤销或重做。
7. 通过文档标识、编辑修订号和原文三重校验拒绝过期替换预览。

## 2. 非目标

- 不实现项目级或多文件替换。
- 不递归扫描项目目录，不建立后台持久化索引。
- 不扩展项目文件发现范围；只搜索项目浏览器中现有的顶层 `.ini` 文件。
- 不搜索当前项目清单以外的 `*.map`、`*.mpr` 或任意文件。
- 不自动保存，不绕过现有保存预检、备份、回滚和编码保持链路。
- 不修改 INI 解析、字段库、补全、Hover、诊断或 AI 语义。
- 不重构 Dock、菜单、工具栏、项目浏览器或主窗口布局。

## 3. 代码事实基线

- `SearchToolWindowViewModel` 当前只有只读空状态，`SearchToolView` 的按钮全部禁用。
- `ProjectOpenService` 只枚举项目根目录的 `*.ini`，Search 必须复用该清单。
- `ReadonlyIniContentService` 是非当前文件的规范只读加载入口；大于 8 MB 的文件会延迟加载。
- 当前编辑文本位于 `Ra2EditableDocumentSession.DocumentState.CurrentText` 和 AvalonEdit 文档中。
- 现有 `ProgrammaticSemanticUndoState` 已为补全和字段插入提供一次语义撤销/重做。
- 现有保存链路只接受当前编辑会话，且负责保存预检、备份、编码和回滚。

## 4. 架构边界

```text
Project Explorer descriptors ─┐
                              ├─ Ra2ProjectSearchService ─ Search result snapshot
Current editor memory text ───┘                              │
                                                            ▼
SearchToolWindowViewModel ← SearchToolView events ← Shell navigation

Current editable session ─ Replace planner ─ immutable ChangeSet / preview
          │                                      │
          └──── DocumentId + EditRevision ─ stale gate
                                                 │
                                                 ▼
                                  Shell single semantic transaction
                                                 │
                                                 ▼
                                   existing Save / Backup / Rollback
```

Search 引擎和替换规划器不依赖 WPF。Shell 只负责提供当前上下文、跨文件导航和将已验证候选文本接入现有编辑会话。

## 5. 数据与接口契约

现有 `SearchToolWindowViewModel` 是 public WPF 绑定类型。本阶段允许把原只读占位属性改为可双向绑定属性，并新增范围、文件模式、替换文本、选择、忙碌和可执行状态等 public 绑定属性。该变化只服务 Search View，不新增 public Search/IO/编辑服务接口，也不改变跨项目序列化契约。

### 5.1 Search

- `Ra2SearchScope`：`Project`、`CurrentFile`。
- `Ra2SearchFailureKind`：`None`、`EmptyQuery`、`InvalidPattern`、`InvalidRegex`、`RegexTimeout`、`NoFiles`、`Canceled`、`Unexpected`。
- `Ra2SearchOptions`：查询文本、范围、大小写、全字、正则、文件模式。
- `Ra2SearchMatch`：文件名、完整路径、行、列、Section、预览、字符起点、长度。
- `Ra2SearchExecutionResult`：匹配项、失败类型、状态文本、扫描/跳过文件数、是否因结果上限截断。
- `Ra2ProjectSearchService.Search(...)`：同步、无 WPF、接受规范文件清单和可选当前内存快照；Shell 可在后台线程调用。

约束：

- 正则使用显式超时；一个文件超时不终止其他文件。
- 结果按项目文件顺序、文件内字符顺序稳定输出。
- 硬上限为 10,000 条，达到后返回截断状态。
- 当前文件内存文本覆盖同路径的磁盘读取结果。
- 文件模式只过滤已有描述符，不触发磁盘发现。
- 大文件延迟和读取失败计入跳过数，不把错误提示文本当作源文本搜索。

### 5.2 编辑会话身份

`Ra2EditableDocumentSession` 新增内部只读值：

- `Guid DocumentId`
- `int EditRevision`

生命周期：

- `StartEditing` 创建新 `DocumentId`，初始 `EditRevision = 0`。
- 文本实际变化时修订号加一；同文本更新不增加。
- `MarkSaved` 不改变文档标识；只在保存文本与当前文本不同时增加修订。
- `Revert` 在文本实际变化时增加修订。
- 补全、字段插入及其他既有程序化编辑必须保留标识并正确增加修订。

### 5.3 当前文件替换

- `Ra2TextChangeSet`：只保存基于同一原文坐标的有序、互不重叠变更；应用时从后向前。
- `Ra2CurrentFileReplacePlanner`：复用 Search 匹配规则，产出候选文本、匹配数和 ChangeSet。
- `Ra2CurrentFileReplacePlan`：绑定 `DocumentId`、`EditRevision`、原文、候选文本、变更集和失败状态。

应用门禁必须同时满足：

1. 当前仍存在可编辑会话。
2. `DocumentId` 相同。
3. `EditRevision` 相同。
4. 当前文本与预览原文逐字符相同。
5. 候选文本与原文不同。

正则替换使用 `Match.Result(replacement)` 语义；拒绝产生零长度匹配的 Replace All 计划，避免插入爆炸和位置歧义。

## 6. UI 契约

保留现有 `Search.View` 与以下 AutomationId：

- `Search.QueryTextBox`
- `Search.CaseSensitiveCheckBox`
- `Search.WholeWordCheckBox`
- `Search.RegexCheckBox`
- `Search.ScopeComboBox`
- `Search.FilePatternComboBox`
- `Search.FindPreviousButton`
- `Search.FindNextButton`
- `Search.FindAllButton`

新增：

- `Search.ReplaceTextBox`
- `Search.ResultsList`
- `Search.StatusText`
- `Search.PreviewReplaceAllButton`
- `Search.ApplyReplaceAllButton`

行为：

- 查询和选项全部双向绑定。
- “查找全部”执行当前条件；结果双击或 Enter 导航。
- “上一个/下一个”在当前结果快照中循环导航。
- 项目范围下替换控件禁用，并明确显示“仅当前文件可替换”。
- “预览替换”只生成候选和数量，不修改文本。
- “应用替换”只对仍有效的预览开放。
- UI 不再显示旧的“能力尚未接入”占位说明。

## 7. 允许修改文件

连续包以 Task Card 为边界，每张卡最多修改 5 个文件。预计允许范围：

- `RA2IniEditor.IDE/Search/**`（新增内部 Search/Replace 核心）
- `RA2IniEditor.IDE/ViewModels/SearchToolWindowViewModel.cs`
- `RA2IniEditor.IDE/ViewModels/SearchResultItemViewModel.cs`
- `RA2IniEditor.IDE/Views/SearchToolView.xaml`
- `RA2IniEditor.IDE/Views/SearchToolView.xaml.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`（只限 Search 事件、上下文、导航、替换应用）
- `RA2IniEditor.IDE/Editing/Ra2EditableDocumentSession.cs`
- `RA2IniEditor.IDE/Editing/Ra2EditableDocumentSessionService.cs`
- `RA2IniEditor.IDE/Editing/Ra2CompletionCommitCoordinator.cs`
- `RA2IniEditor.IDE/Controllers/FieldBrowser/Ra2FieldBrowserController.cs`
- 对应的 `RA2IniEditor.Tests/IDE/**` 测试文件
- 本契约、阶段台账、CurrentPhase、Full Context 和上下文胶囊

## 8. 禁止修改文件与语义

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- AvalonDock 布局配置和布局持久化
- `ProjectOpenService` 的文件发现规则
- 保存、备份、回滚、编码检测实现
- parser、diagnostics、completion 生成/提交语义、字段库、Hover、Quick Peek、AI
- 旧版 solution、旧版编辑器和所有 legacy 项目
- 项目文件和第三方依赖

## 9. 连续阶段

### SEARCH-1A：查询、结果和失败契约

- 引入内部 Search 数据契约。
- 覆盖空查询、文件模式、稳定结果和失败状态测试。

验收：核心契约无 WPF 依赖且保持 internal；除已声明的 Search ViewModel 绑定面外无公开产品 API，契约可表达全部失败和截断状态。

### SEARCH-1B：项目级查找引擎

- 实现文字、全字和正则搜索。
- 接入规范描述符、当前内存覆盖、读取失败/大文件跳过、结果上限。

验收：测试覆盖大小写、全字、正则超时/非法、内存覆盖、项目顺序和跳过计数。

### SEARCH-1C：Search ViewModel 与项目查找 UI

- 移除 Mock/占位状态。
- 接入查询、选项、结果、状态、前后导航和跨文件导航。

验收：AutomationId 保持；项目搜索能显示并导航；不改变 Dock 布局。

### SEARCH-1D：文档身份与修订

- 为编辑会话添加 `DocumentId`、`EditRevision`。
- 修正所有既有会话重建点以保留身份和修订。

验收：手输、补全、字段插入、保存、恢复的身份/修订测试通过。

### SEARCH-1E：ChangeSet 与替换规划

- 实现 ChangeSet 校验/应用。
- 实现文字和正则当前文件替换预览、零长度保护和 stale 数据绑定。

验收：不修改 Shell/磁盘；覆盖重叠、越界、正则替换和 no-op。

### SEARCH-1F：替换 UI、应用与单次 Undo

- 接入替换预览和应用。
- 使用现有程序化语义撤销状态，将 Replace All 作为一次撤销/重做。

验收：预览不改文本；过期预览拒绝；应用后脏状态正确；一次 Ctrl+Z/Ctrl+Y 完整撤销/重做；不自动保存。

### SEARCH-1G：回归、烟测与文档收口

- 运行定向测试、构建、全测和 clean package。
- 更新阶段台账、CurrentPhase、Full Context、FeatureOverview/UserGuide/ReleaseChecklist（仅按实际产品行为）。

验收：无 legacy；无新增依赖；除明确 Search/Shell 接线外无 Shell 变化；Search 之外语义保持。

## 10. 验证矩阵

| 层级 | 验证 |
|---|---|
| 纯逻辑 | Search/Regex/Pattern/ChangeSet/Replace planner 单元测试 |
| 生命周期 | Session identity/revision、stale、dirty、save/revert 测试 |
| UI 边界 | XAML AutomationId、双向绑定、按钮接线、无占位文本 |
| Shell 边界 | 只使用规范项目清单、内存覆盖、导航、单语义 Undo、无磁盘写入 |
| 编译 | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` |
| 全测 | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` |
| 交付 | `powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly` |

## 11. 自审结论

### 已修正的问题

1. 将原“项目级查找”收束为项目浏览器规范文件清单，避免功能范围和 Project Explorer 不一致。
2. 明确内存文本覆盖磁盘文本，避免未保存内容搜索错误。
3. 增加正则超时、结果上限、大文件/读取失败汇总，避免 UI 卡死或静默漏项。
4. Replace All 增加三重 stale 门禁，避免预览后继续编辑导致错误覆盖。
5. 明确单次语义 Undo 和不自动保存，避免绕过现有保存安全链路。
6. 通过阶段卡拆分编辑会话改动和 UI 接线，降低跨层一次性改动风险。

### 剩余可接受风险

- 搜索为按需扫描，不是后台索引；大型项目首次查询可能有可感知延迟，但在后台线程执行。
- 大于 8 MB 的延迟文件本阶段不搜索，会在状态中明确报告。
- UI 自动化可以确认结构和事件边界，但最终键鼠手感仍建议在 1920×1080 下做一次人工烟测。

结论：契约可实施，未发现需要扩大到项目级替换、持久化索引或 Shell 布局重构的前置依赖。
