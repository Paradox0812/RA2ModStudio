# Icon-S2B-P2 Main Toolbar Density and Top Chrome Polish

## 0. 目标

本阶段只做顶部菜单栏 / 主工具栏的密度与浅色主题层次优化，不修改命令集合、不修改图标语义、不修改 AutomationId、不改变任何业务行为。

目标效果：

- 顶部菜单栏和工具栏更像同一套 IDE chrome。
- 工具栏按钮更紧凑，但仍然好点击。
- 图标仍保持 16x16 清晰显示。
- 默认按钮不显示厚重边框。
- Hover / Pressed / Focus / Disabled 状态清楚但轻量。
- 内容区与顶部 chrome 有清晰但不突兀的分隔。

---

## 1. 推荐浅色主题层次

### 1.1 顶部颜色结构

建议采用低对比浅色层次：

| 区域 | 建议色值 | 用途 |
|---|---|---|
| Window / Shell background | `#F6F8FA` | 应用内部顶部 chrome 的总背景 |
| Menu bar background | `#F7F9FB` | 文件 / 编辑 / 视图一排 |
| Toolbar background | `#F2F5F8` | 图标工具栏一排 |
| Toolbar separator line | `#DDE3EA` | 工具栏与编辑区之间的轻分隔 |
| Content background | `#FFFFFF` | 编辑器 / 面板主内容 |
| Panel border | `#D8DEE6` | 右侧栏、底部栏分隔 |
| Icon normal | `#2F3A45` | 常规图标 |
| Icon muted / disabled | `#9AA6B2` | 禁用图标 |
| Hover background | `#E8EEF5` | 工具栏按钮 hover |
| Pressed background | `#DCE6F0` | 工具栏按钮 pressed |
| Focus border | `#8CB8E8` | 键盘焦点，细边框 |
| Warning icon | `#B87900` | Issues / warning |
| Error icon | `#C2413A` | error |
| Success icon | `#2F8A4B` | success |
| Accent | `#2F6FDB` | 当前项 / AI / 强调，不要滥用 |

### 1.2 视觉关系

顶部三层应该这样理解：

```text
系统标题栏
  ↓
MenuBar：略浅，负责文本菜单
  ↓
ToolBar：略深一点，负责图标命令
  ↓
Content：纯白或接近白，负责主内容
```

不要让 MenuBar 和 ToolBar 色差过大。它们应该属于同一组顶部 chrome，而不是两个完全割裂的 UI 条。

---

## 2. 推荐尺寸与间距

### 2.1 MenuBar

建议：

```text
Height: 26~28
Horizontal item padding: 8~10
Vertical item padding: 3~4
Font size: 保持当前或 12
Background: #F7F9FB
Bottom border: #E6EBF1 或无
```

### 2.2 Toolbar

建议：

```text
Height: 28~30
Icon visual size: 16x16
Button hit size: 24x24
Button padding: 3~4
Horizontal spacing: 2~4
Group separator margin: 4~6
Background: #F2F5F8
Bottom border: #DDE3EA
```

### 2.3 Toolbar Button

默认状态：

```text
Background: Transparent
BorderBrush: Transparent
BorderThickness: 1 或 0
Padding: 3
MinWidth: 24
MinHeight: 24
```

Hover：

```text
Background: #E8EEF5
BorderBrush: #D4DEEA
```

Pressed：

```text
Background: #DCE6F0
BorderBrush: #C6D5E5
```

Focused：

```text
BorderBrush: #8CB8E8
BorderThickness: 1
```

Disabled：

```text
Foreground/IconBrush: #9AA6B2
Opacity: 0.55~0.65
Background: Transparent
BorderBrush: Transparent
```

---

## 3. 不建议的做法

不要：

- 使用默认 WPF Button 边框。
- 让每个 toolbar button 默认都显示边框。
- 为了紧凑把 hit area 压到低于 22x22。
- 把菜单栏做成纯白、工具栏做成深灰，造成上下割裂。
- 给每个图标使用不同颜色。
- 过度使用 Accent 色。
- 在本阶段修改图标 geometry。
- 在本阶段改命令行为或菜单结构。

---

## 4. Codex 任务：Icon-S2B-P2

### 4.1 任务目标

对顶部菜单栏 / 主工具栏做小范围视觉 polish：

1. 统一 menu bar 与 toolbar 的浅色 chrome。
2. 收紧 toolbar 高度、button padding 和 icon spacing。
3. 去除 toolbar button 默认明显边框。
4. 保留 hover / pressed / focus / disabled 状态。
5. 不改图标资源语义，不改命令，不改 AutomationId。

### 4.2 允许修改文件

仅允许：

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Themes/ShellTheme.xaml
RA2IniEditor.IDE/Themes/IconResources.xaml
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

`IconResources.xaml` 只允许在需要补 brush token / 引用时小改，不得改图标 geometry 语义。

### 4.3 禁止修改

不得：

```text
修改 toolbar command set
修改 click handlers / command semantics
修改 menu entries
修改 AutomationId
恢复 legacy IDs
新增 PNG / SVG runtime assets
修改 AI Assistant 行为
修改 Field Registry 行为
修改 Section Tree / Project Explorer 行为
修改 parser / diagnostics / completion / hover / quick peek / save preflight
修改 solution / project files
修改图标 geometry 语义
```

### 4.4 实现建议

优先在 `ShellTheme.xaml` 中定义或调整：

```text
MenuBar style
ToolBar style
Toolbar button style
Toolbar separator style
Icon brush references if needed
```

如果现有样式已经存在，优先在现有样式上小改，不要新增平行样式体系。

建议使用资源 key，而不是到处硬编码色值。如果当前还没有完整主题 token，可以先在 ShellTheme.xaml 中定义局部 brush，并在文档中标记为未来 Theme token 候选。

### 4.5 测试要求

更新 / 新增 boundary tests，覆盖：

```text
1. 主工具栏 AutomationId 仍存在。
2. 旧 legacy AutomationId 未恢复。
3. 菜单入口仍存在。
4. command handler / click behavior 不变。
5. Save / Undo / Redo / Revert 状态规则仍通过。
6. 不新增 PNG / SVG runtime asset。
7. 不做 pixel-perfect 视觉测试。
```

### 4.6 验证命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 5. 手动验收清单

完成后检查：

```text
1. 菜单栏和工具栏看起来像同一套顶部 chrome。
2. 工具栏高度比上一版更紧凑。
3. 图标按钮默认无明显边框。
4. Hover / Pressed / Focus 状态清楚。
5. Disabled 图标不突兀。
6. 工具栏不挤压编辑区。
7. 图标没有裁切、错位或大小不一致。
8. Open / Search / Issues / Field Registry 行为不变。
9. Save / Undo / Redo / Revert 状态仍正确。
10. 无 missing resource exception。
```

---

## 6. 后续建议

Icon-S2B-P2 完成后，下一阶段可以二选一：

```text
Icon-S2C：AI Assistant inline action icons
Icon-S3：Section Tree / File Type icons
```

如果目标是肉眼改善最大，优先 Icon-S2C。  
如果目标是 IDE 信息结构更强，优先 Icon-S3。
