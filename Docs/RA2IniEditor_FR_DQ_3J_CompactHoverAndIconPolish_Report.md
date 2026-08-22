# RA2IniEditor.IDE FR-DQ-3J Compact Hover and Icon Polish Report

## 目标

基于 3I ReleaseIconAndUserDocs 版本，对交付前发现的 UI 细节做最小范围修正：

1. Source Editor Hover 过大，改为接近 VS2022 的单行紧凑信息条。
2. Completion Dropdown 仍是卡片式列表，改为名称 / 类型 / 注释 / 来源四列紧凑行。
3. Project Explorer Section Tree 仍使用文字 badge，补一层轻量 glyph 图标。
4. Right Tool Well 的 Section / AI tab 和 AI 面板标题补齐轻量图标。

## 修改文件

- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/ViewModels/Language/Ra2HoverDisplayViewModel.cs`
- `RA2IniEditor.IDE/Views/Language/Ra2CompletionDropdownView.xaml`
- `RA2IniEditor.IDE/ViewModels/ProjectExplorerItemViewModel.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.Tests/IDE/Ra2SourceEditorHoverBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2CompletionPreviewUiBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`

## Hover 调整

原 Hover 使用多行卡片：标题、说明、示例、来源、适用范围分块显示。3J 改为四列单行：

```text
名称 | 类型 | 注释 | 来源
```

其中注释会把换行压缩为单行，并将示例值合入注释尾部。字段风险脚注仍会作为注释的一部分保留，但不再额外展开成大卡片。

## Completion Dropdown 调整

候选列表从卡片式条目改为四列紧凑行：

```text
名称 | 类型 | 注释 | 来源
```

保留原有选择条、双击、Enter/Tab 提交和 Esc 关闭逻辑，不改 Completion commit 行为。

## Project Explorer 图标调整

新增 `ProjectExplorerItemViewModel.IconGlyph`，保留 `IconText` 作为语义 badge / 测试稳定字段。XAML 绑定 `IconGlyph` 显示轻量图标，`IconText` 仍作为 tooltip。

示例：

- `INI` -> `▤`
- `Inf` -> `♟`
- `Veh` -> `▰`
- `Air` -> `✈`
- `Bld` -> `▥`
- `Wpn` -> `⚔`
- `WH` -> `✹`
- `Proj` -> `➤`
- `AI` -> `◇`

## AI 侧栏图标调整

Right Tool Well 的 Section / AI tab 现在显示轻量图标加文本。AI 助手标题也补了 `◇` 图标。

## 未修改范围

本阶段没有修改：

- BuiltIn 字段库
- Hover Provider 语义逻辑
- Completion Provider / Commit 逻辑
- Diagnostics 逻辑
- 保存链路
- Field Registry runtime
- AI Provider / PromptBuilder

## 静态检查

已完成：

- `ShellWindow.xaml` XML 解析通过
- `Ra2CompletionDropdownView.xaml` XML 解析通过
- SourceClean 包不包含 `bin/obj/.vs/TestResults/artifacts`

未运行：

- `dotnet build`
- `dotnet test`

需要在 Windows 本地执行：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```
