# SEARCH-1-R1 阶段台账

日期：2026-07-23  
契约：`Docs/SEARCH-1-R1_ContinuousContract.md`  
状态：实现与自动化验证完成

## 阶段结果

| 阶段 | 结果 | 审查与验证 |
|---|---|---|
| SEARCH-1A 查询/结果/失败契约 | 完成 | 类型保持 internal；无 WPF、IO 写入或 public API |
| SEARCH-1B 项目查找引擎 | 完成 | 稳定顺序、内存覆盖、全字/正则、非法模式、大文件/读取失败跳过 |
| SEARCH-1C ViewModel/UI/Shell 导航 | 完成 | 双向条件、结果列表、前后/双击/Enter 导航、脏文件离开门禁 |
| SEARCH-1D 文档身份与修订 | 完成 | 手输、保存、恢复、补全、字段插入均保持身份并正确递增修订 |
| SEARCH-1E ChangeSet/替换规划 | 完成 | 原文坐标、反向应用、重叠/越界、捕获组、零长度/no-op/stale |
| SEARCH-1F 替换 UI/应用/Undo | 完成 | 当前文件限定、预览不修改、四重应用门禁、单次 Undo/Redo、不自动保存 |
| SEARCH-1G 回归与治理 | 完成 | Debug build 0/0；非 UI 测试 2380/2380；Search 浮动宿主 UIA 1/1 |

## Stage Result Ledger

### SEARCH-1A/1B

- 定向测试：8/8。
- 审查结论：项目范围只消费 Project Explorer 的规范顶层 `.ini` 描述符。
- 当前编辑器内存文本覆盖同路径磁盘内容。
- 正则有 500 ms 单文件超时；结果硬上限为 10,000。
- 大于 8 MB 的延迟文件与读取失败文件被跳过并汇总。

### SEARCH-1C

- Search/Shell/视觉边界测试：82/82。
- 保留既有 Search AutomationId，并新增结果与状态 AutomationId。
- `ShellWindow.xaml`、Dock ContentId、Home、持久化和布局未修改。
- 跨文件结果导航复用现有 dirty-navigation 决策链。
- 结果字符内容不再匹配时拒绝过期导航。

### SEARCH-1D

- 会话/保存/补全/字段浏览器定向测试：56/56。
- `DocumentId` 在同一编辑会话内稳定。
- `EditRevision` 只在文本实际变化时递增；同文本同步和正常保存不伪增。
- Completion 和 Field Browser 改用 `ContinueWith`，没有改变其提交语义。

### SEARCH-1E

- Search/ChangeSet/替换规划定向测试：19/19。
- 项目级替换、零长度正则、no-op、无匹配和超上限均显式失败。
- 预览绑定文档标识、修订号和原文，不触碰编辑器或磁盘。

### SEARCH-1F

- Search/Shell/视觉/ViewModel/替换相关测试：97/97。
- 替换只在 `CurrentFile` 范围启用。
- 应用同时验证文档标识、修订号、会话原文和 AvalonEdit 当前文本。
- 整批候选文本通过既有 `ProgrammaticSemanticUndoState` 成为一次 Undo/Redo。
- 不调用 Save、`WriteText` 或 `File.Write*`。

### SEARCH-1G

- `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`：通过，0 warning / 0 error。
- `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug`：通过，2380/2380；测试项目重编译时曾显示一条既有 CS8602，最终 solution build 为 0 warning。
- `SearchTool_OpenHideAndReopen_UsesFloatingHostWithoutMockResults`：通过，1/1。
- 外部 UIA 无法从 AvalonDock 浮动 HWND 穿透到内部 Search WPF 控件；完整键鼠 Find/Replace UIA 未伪造通过。
- clean package 结果记录在本文件末尾的 Package Evidence。

## Diff Intent Table

| 变更组 | 意图 | 非意图 |
|---|---|---|
| `IDE/Search/*` | 查询、结果、失败、项目扫描、替换计划 | 持久化索引、多文件写入 |
| Search ViewModel/View | 条件、结果、替换预览与应用入口 | Dock/主布局重构 |
| `ShellWindow.xaml.cs` | 提供规范上下文、导航、替换事务 | 新保存链路、布局变化 |
| Editable Session | 增加内部 identity/revision | 改变文本/保存业务语义 |
| Tests | 锁定新行为并移除旧占位断言 | 放松 legacy/写盘边界 |
| Docs | 更新产品事实和后续入口 | 重写历史阶段 |

## Public API Ledger

- 新增 Search/Replace 类型全部为 `internal`。
- `Ra2EditableDocumentSession` 本身为 `internal`；新增 `DocumentId`、`EditRevision`、`ContinueWith` 不构成外部产品 API。
- `SearchResultItemViewModel` 保留原 public 构造函数和原 public 属性；新增导航数据保持 internal。
- 现有 public `SearchToolWindowViewModel` 从只读占位模型变为真实 WPF 绑定模型：Query/匹配选项开放 setter，并新增范围、文件模式、替换文本、选择、忙碌和 Can* 绑定属性。这是已授权的 presentation API 变化。
- 无 public Search/IO/编辑服务接口、项目依赖或序列化格式变化。

## Deferred Governance Queue

- `SEARCH-UIA-001`：AvalonDock 浮动 child-HWND 只向外部 UIA 暴露宿主 Chrome，Search 内部控件不可穿透；未来如需自动化/无障碍修复，必须单独契约。
- `SEARCH-PERF-001`：本阶段为按需扫描而非后台索引；只有真实性能证据出现后才考虑索引。
- 大于 8 MB 的延迟加载文件本阶段明确跳过，不属于静默漏搜。

## Package Evidence

- 路径：`artifacts/RA2IniEditor.IDE.SourceClean.zip`
- 文件数：1003
- 禁止条目：0
- 必需入口/契约缺失：0
- 最终归档的大小和 SHA-256 在交付摘要中记录，避免在被归档文档内形成自引用哈希。
