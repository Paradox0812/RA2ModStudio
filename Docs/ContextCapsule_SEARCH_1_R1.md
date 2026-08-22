# Context Capsule — SEARCH-1-R1

## 当前可信状态

`SEARCH-1-R1` 已实现项目级只读查找和当前文件替换。

- Search 范围：整个已打开项目或当前文件。
- 项目定义：Project Explorer 已发现的项目根目录顶层 `.ini` 清单。
- 匹配：大小写、全字、.NET 正则（单文件 500 ms 超时）。
- 结果：稳定文件/字符顺序，最多 10,000 条，可跨文件导航。
- 当前文件优先搜索 AvalonEdit 内存文本。
- 替换：仅当前文件，先预览后应用；项目级替换不存在。
- 应用：不自动保存；现有 Save/Preflight/Backup/Encoding/Rollback 仍是唯一落盘路径。
- Undo：一次 Replace All 对应一次语义 Undo/Redo。
- stale：`DocumentId + EditRevision + session text + editor text`。

## 关键文件

- `Docs/SEARCH-1-R1_ContinuousContract.md`
- `Docs/SEARCH-1-R1_StageLedger.md`
- `RA2IniEditor.IDE/Search/`
- `RA2IniEditor.IDE/Views/SearchToolView.xaml`
- `RA2IniEditor.IDE/ViewModels/SearchToolWindowViewModel.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/Editing/Ra2EditableDocumentSession.cs`

## 不得误解

- Search 没有递归扫描磁盘，没有扩展 `.map/.mpr` 项目发现。
- Search 没有后台/持久化索引。
- 替换没有项目级或跨文件写入。
- Search 结果不是持久实体；文本改变后必须重新搜索或由导航 stale 检查拒绝。
- `ShellWindow.xaml`、Dock ContentId/Home/persistence 未修改。
- parser、diagnostics、completion 候选/提交、Field Registry、Hover、AI、Save 语义未修改。

## 验证基线

- Debug solution build：0 warning / 0 error。
- 非 UI 测试：2380/2380。
- Search 浮动宿主打开/隐藏/重开 UIA：1/1。
- 浮动宿主内部控件仍有 `SEARCH-UIA-001` 自动化可访问性限制。

## 下一入口

返回 `AGENT-AUTHORING-1-R1 A2`，先依据现已存在的 `DocumentId/EditRevision` 重新基线化 A2 契约，再实现 Agent 写入侧的变更计划与失效门禁。不得把 SEARCH 的当前文件 Replace All 直接泛化为 Agent 多文件写入。
