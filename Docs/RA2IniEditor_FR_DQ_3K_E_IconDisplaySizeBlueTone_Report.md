# RA2IniEditor FR-DQ 3K-E Icon Display Size / Blue Tone Polish Report

## 1. 目标

本轮在 3K-D Raster Icon Integration 基础上做最小 UI 修正：

- 将 Project Explorer 树节点图标显示区域从 16x16 试调到 20x20。
- 将右侧顶部 Section / AI 切换按钮与 AI 面板操作按钮从偏白按钮收束为统一浅蓝色调。
- 保持现有 `Icon.*` 资源 key、ViewModel、分类逻辑和 PNG 资源文件不变。

## 2. 修改文件

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
  - Project Explorer 图标外框：18x18 -> 22x22。
  - Project Explorer 图标主体：16x16 -> 20x20。
  - 图标缩放：`NearestNeighbor` -> `HighQuality`。
  - TreeViewItem 最小高度：22 -> 26。
  - 右侧顶部 Section / AI 按钮改用蓝色调 compact button style。
  - AI 面板清空 / 发送 / 取消按钮改用蓝色调 compact button style。
  - AI 进阶 Expander header 改为浅蓝色 chip。
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`
  - 新增 `IdeBlueCompactButtonStyle`。
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
  - AI 回复复制按钮和代码块复制按钮改用蓝色调 compact button style。
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`
  - 更新 Project Explorer 图标尺寸与蓝色按钮样式边界断言。

## 3. 影响范围

本轮不修改：

- `IconImageResources.xaml` 资源 key。
- PNG 文件。
- ProjectExplorerItemViewModel 映射逻辑。
- AI 业务逻辑、发送流程、上下文收集或 provider 选择逻辑。

## 4. 验证说明

当前执行环境未安装 `dotnet`，无法运行 `dotnet build/test`。已完成静态检查：

- XAML XML 可解析。
- `IdeBlueCompactButtonStyle` 已在 `ShellTheme.xaml` 定义。
- `ShellWindow.xaml` 中目标按钮已切换到蓝色调样式。
- Project Explorer 图标区域已切换到 20x20 主体显示。
