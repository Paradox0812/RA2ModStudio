# UI-MODERN-PROGRAM-R1 VISUAL-FIX3 最终契约

状态：自审通过，允许按本契约连续实施  
日期：2026-07-23  
基线：`UI-MODERN-PROGRAM-R1 VISUAL-FIX2`

## 1. 功能目标

1. 将 AI 助手中的清空、进阶、发送和取消图标从位图资源切换为项目已有的矢量 Geometry，解决缩放、着色和像素对齐导致的残缺或重影。
2. 修复字段库“活跃字段包”中“字段”列头被裁切的问题，同时保持左侧导航区当前总宽度不增长。
3. 修复字段库管理器中“目标 Section”列的潜在列头裁切。
4. 对生产 XAML 做一次静态 UI 健康审计，处理有确定证据、可在窄边界内修复的问题。

## 2. 非目标

- 不重新设计 AI 助手布局、交互和数据流。
- 不改变 Field Registry 的 Project > Global > BuiltIn 优先级、加载、写入、回滚或清理语义。
- 不改变 Shell Dock、菜单、工具栏、窗口布局持久化或启动流程。
- 不删除历史 PNG 资源，也不修改应用图标和领域图标。
- 不修改共享冻结样式；缺少真实视觉缺陷证据的候选问题只登记，不顺手修复。
- 不在本阶段实现 Agent 写入接口。

## 3. 代码事实与根因

### 3.1 AI 动作图标

`ShellWindow.xaml` 当前仍使用：

- `Icon.Action.Clear`
- `Icon.Action.Advanced`
- `Icon.Action.Send`
- `Icon.Action.Cancel`

这些资源来自 `IconImageResources.xaml` 的 PNG。项目已经在
`IconGeometryResources.xaml` 中提供同名语义的矢量 Geometry：

- `IconGeometry.Action.Clear`
- `IconGeometry.Action.Advanced`
- `IconGeometry.Action.Send`
- `IconGeometry.Action.Cancel`

因此本阶段只替换呈现节点，不新增资源，不改变按钮模板、命令或状态逻辑。

### 3.2 字段库列头

字段库 R2 列头样式包含 14 DIP 水平 Padding；底层列头模板还固定保留
12 DIP 排序箭头槽。“字段”两字在当前字体下的安全宽度约为 50 DIP，
而现有列宽为 48 DIP，因此裁切是确定性布局问题。

修正为：

- `范围`：88 -> 80 DIP
- `字段`：48 -> 56 DIP

总宽度仍为 136 DIP，不扩大左侧来源包区域。

项目级静态审计还发现 `FieldRegistryManagerWindow.xaml` 的
`目标 Section` 列宽为 92 DIP，低于含中英文标题和列头装饰的安全宽度；
本阶段将其调整为 112 DIP。

## 4. 允许修改文件

### Task Card A：运行与测试（最多 5 个文件）

1. `RA2IniEditor.IDE/Views/ShellWindow.xaml`
2. `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml`
3. `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml`
4. `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`
5. `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`

### Task Card A2：失败门禁窄修复（1 个测试文件）

全量测试发现 `IconResourceBoundaryTests` 仍把 Shell 消费旧 PNG 锁定为必需行为，
与本契约的矢量化目标直接冲突。A2 只允许修改：

1. `RA2IniEditor.Tests/IDE/IconResourceBoundaryTests.cs`

A2 保留 Geometry 与历史 Image 资源均已定义的资源完整性断言，只将 Shell
消费端断言更新为 `IconGeometry.Action.*`，并禁止四个旧 `Icon.Action.*`
重新进入 Shell。不得修改任何产品代码。

### 治理收口文档

- 本契约
- 阶段结果 Ledger
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `Docs/Codex_CurrentPhase.md`

治理文档在连续包完成或失败停止时统一更新，不计入运行时代码卡。

## 5. 禁止修改文件与边界

- `ShellWindow.xaml.cs`
- `Themes/IdeWorkspaceStyles.xaml`
- `Themes/IdeFieldRegistryStyles.xaml`
- `Themes/IdeCollectionStyles.xaml`
- `Themes/IconGeometryResources.xaml`
- `Themes/IconImageResources.xaml`
- 所有 parser、diagnostics、completion、save、AI provider/streaming 和
  Field Registry service/view-model 代码
- 项目文件和第三方依赖
- legacy 工程或已移除界面

## 6. 精确修改契约

### 6.1 AI 图标

- 每个动作图标用 `Path` 呈现已有 Geometry。
- `Stroke` 绑定到所属 `Button` 或 `ToggleButton` 的 `Foreground`。
- 保留既有尺寸、Margin、ToolTip、Click、Visibility、IsChecked 和 Automation 属性。
- 发送按钮在 Accent 背景下继续使用白色前景；取消按钮继续使用禁用态/普通态前景。
- 不使用字体字符代替图标。

### 6.2 字段库

- 活跃字段包 DataGrid 使用固定列宽 80/56。
- 管理器 `TargetSectionKind` 列宽使用 112。
- 不改变绑定、排序、选择、虚拟化、滚动和双击行为。

## 7. AutomationId 契约

必须原样保留：

- `AiAssistant.ClearButton`
- `AiAssistant.AdvancedButton`
- `AiAssistant.GenerateButton`
- `AiAssistant.CancelButton`
- `AiAssistant.ModelSelector`
- `FieldRegistryCenter.Navigation`
- `FieldRegistryCenter.FieldList`
- `FieldRegistryCenter.Details`
- `FieldRegistryCenter.Details.EmptyState`
- `FieldRegistryCenter.Details.Inspector`

不新增 AutomationId。

## 8. 测试契约

1. `IdeShellBoundaryTests`
   - 断言 AI 面板引用四个 `IconGeometry.Action.*`。
   - 断言 AI 面板不再引用四个 `Icon.Action.*` 位图资源。
   - 保留现有 Click、AutomationId、发送/取消相邻布局和 advisory-only 断言。
2. `IdeVisualSystemBoundaryTests`
   - 将活跃字段包列宽锁定为 80/56。
   - 锁定管理器 `目标 Section` 列宽为 112。
   - 不弱化现有字段库边界断言。
3. 解析全部生产 XAML，任何 XML 错误均失败。
4. 运行 Debug build、定向测试、全量测试和 IdeOnly 清洁打包。

## 9. 验收标准

- 四个 AI 动作按钮不再依赖 PNG 呈现，图标随按钮 Foreground 正确着色。
- “字段”和“目标 Section”标题在设计宽度下具备足够空间。
- 活跃字段包两列总宽度保持 136 DIP。
- 所有既有 AutomationId、事件处理器和语义边界不变。
- XAML 解析、构建、测试和打包通过。
- legacy 未恢复。

## 10. 全项目 UI 审计规则

本阶段的静态审计覆盖生产 XAML：

- 解析有效性；
- 动作控件是否仍误用位图；
- 固定列宽是否小于标题、Padding 和排序槽的估算下限；
- 非主题化颜色和冻结共享样式候选。

只有能由代码事实直接证明且能在 Task Card A 内修正的问题才实施。
`ShellTheme.xaml` 中共享 `IdeSplitterStyle` 的硬编码颜色属于冻结共享资源，
缺少本轮截图缺陷证据，登记为后续主题一致性审计候选，不在本阶段修改。

## 11. 自审结论

- Scope：通过。运行/测试卡严格为 5 个文件。
- Reuse：通过。完全复用已有 Geometry 和现有 DataGrid 样式。
- Public API：通过。无新增或修改 public API。
- Semantic boundary：通过。没有业务、保存、字段库或 AI 数据流修改。
- Evolvability：通过。移除控件对位图缩放的依赖，仍使用集中图标资源。
- Regression：通过。每个修改点均有静态边界测试，且保留全量验证。
- Failure repair：A2 是全量测试发现的直接依赖修复，不弱化资源完整性断言，
  不扩大产品范围。
- 结论：修正版契约足够可靠，可以继续执行。
