# ASSET-VOX-UI-R1 Workspace Recomposition and Camera Stability Final Contract

> 状态：已批准并完成实施；自动化验证已完成，真实分辨率 / DPI 手工验收待用户执行。
>
> 风险级别：R3（局部高风险 UI / 生命周期变更）
>
> 前置基线：`ASSET-VOX-3B Accepted Candidate VOX Export` 已完成。
>
> 代码事实：`Docs/ASSET-VOX-UI-R1_WorkspaceRecompositionAndCameraStabilityCodeFactAudit.md`

## 0. 一句话目标

把体素工作区从“所有功能同时展开的长表单”重组为“左侧任务检查器 + 中央大尺寸 3D 审阅 + 下方可调证据页”的现代 IDE 工作区，并消除模式切换、Dock 重挂载和场景刷新造成的非用户触发相机跳变。

## 1. 本阶段用户可见交付

完成后，用户应得到：

1. 3D 模型成为页面绝对视觉主区，而不是固定 360 DIP 的卡片。
2. 生成、几何、风格、导出四组控制按任务分页，不再全部纵向堆叠。
3. 质量、结构、着色、审阅四组证据按页展示，不再同时挤占主视图。
4. 左侧检查器宽度、3D 与证据区高度可拖动调节。
5. 旋转、平移、缩放后的相机在预览模式切换和临时文档切换后保持稳定。
6. “重置视角”仍是唯一明确触发重新取景的常用入口；换入完全不同的模型来源时允许一次自动取景。
7. 页面在 1920×1080 下不出现整体缩放、整页横向滚动、重叠、截断或大面积无效留白。

## 2. 非目标

本阶段明确不做：

- 不修改几何生成、平滑、对称、结构识别、DeepSeek 仲裁或上色算法；
- 不修改 Tencent/DeepSeek Provider、模型选择、调用次数、Token 或费用策略；
- 不修改最终候选固化和 VOX 写出语义；
- 不新增 VXL/HVA 写出；
- 不修改项目 Apply/Save、Undo/Redo 或 INI 能力；
- 不修改 Shell、AvalonDock 全局布局、菜单、工具栏、项目浏览器、底部工具窗或状态栏；
- 不引入 Helix、第三方 Dock/UI/3D 依赖；
- 不持久化内部面板尺寸或相机状态到用户配置；本阶段只保证当前文档会话稳定；
- 不进行真实 Tencent/DeepSeek 调用。

## 3. 架构与所有权契约

### 3.1 权威数据路径

业务数据权威仍为：

```text
Ra2VoxelStyleWorkspaceViewModel
  -> 当前源/候选/语义/差异/着色快照
  -> Ra2VoxelStyleWorkspaceView 选择显示模式
  -> Ra2VoxelViewport3D 异步构建呈现场景
```

本阶段不得复制或重建业务状态，不得在 XAML code-behind 中制造新的候选、蒙版、语义或导出事实。

### 3.2 新增的呈现状态

仅允许增加以下非持久化呈现状态：

- 当前左侧任务页；
- 当前下方证据页；
- 左侧检查器和下方证据区当前 GridLength；
- 当前相机姿态；
- 当前相机所属的几何会话标识；
- 场景是否已经完成首次自动取景。

这些状态只属于当前 `Ra2VoxelStyleWorkspaceView` / `Ra2VoxelViewport3D` 实例，不进入 ViewModel、项目文件、用户配置或公共 API。

### 3.3 相机状态定义

相机姿态至少包含：

```text
Yaw
Pitch
NormalizedTargetX/Y/Z
DistanceToBoundsDiagonalRatio
HasUserInteraction
CameraGroupKey
```

要求：

- 所有数值必须是有限值；非法状态回退到初始取景。
- Target 用旧 Bounds 归一化并在新 Bounds 中恢复，以适配同一模型不同候选的轻微尺寸变化。
- Distance 使用模型包围盒对角线比例恢复，并执行安全上下限钳制。
- 相机状态为 IDE 内部实现，不成为序列化/public contract。

### 3.4 相机分组和自动取景规则

同一个来源加载会话中的以下切换属于同一相机组：

- 原始 / 直接 / 平滑 / 差异；
- 结构区 / 对称；
- 着色结果 / 对比度 / 区域 / 色板；
- 同一来源下的当前候选变化。

这些切换必须保留视角，不得调用无条件 `ResetCamera()`。

只有以下情况允许自动取景：

1. 当前工作区第一次出现有效 3D 场景；
2. 用户选择了不同的 VOX/VXL 来源；
3. 用户开始了新的生成来源会话；
4. 已保存相机状态非法或无法映射到新 Bounds；
5. 用户明确点击“重置视角”或双击视口。

场景构建失败、取消或被更新请求淘汰时，不得覆盖当前相机状态和当前成功场景。

### 3.5 生命周期规则

- `Unloaded`：取消未完成的场景构建，保留当前相机姿态；允许清除重型场景对象。
- `Loaded`：恢复同一文档实例的场景，并按相机分组策略恢复视角。
- DataContext 改为不同工作区实例：丢弃旧相机组，首次成功场景自动取景。
- 关闭文档后：该 View 实例随文档释放，不跨文档或跨进程保留相机状态。
- 不通过修改 Shell 来区分关闭与临时卸载。

## 4. 最终布局契约

### 4.1 1920×1080 主布局

目标结构：

```text
┌────────────────────────────────────────────────────────────────────┐
│ 体素工作区标题 / 来源摘要 / 状态                         主操作区 │  40–48
├────────────────┬───────────────────────────────────────────────────┤
│ 任务检查器     │ 3D 审阅工具栏：原始 直接 平滑 差异 … 重置视角 │
│                ├───────────────────────────────────────────────────┤
│ 生成│几何      │                                                   │
│ 风格│输出      │                  交互式 3D                         │  *
│                │                                                   │
│ 当前页独立滚动 ├───────────────────────────────────────────────────┤
│                │ ⇕ 可拖动水平分隔                                  │  5
│                ├───────────────────────────────────────────────────┤
│                │ 质量│结构│着色│审阅      当前证据页独立滚动     │ 220–300
├────────────────┴───────────────────────────────────────────────────┤
│ 单行状态 / 错误摘要                                                │  24–30
└────────────────────────────────────────────────────────────────────┘
```

尺寸约束：

- 左侧检查器默认宽度 312 DIP，允许 260–420 DIP。
- 左右分隔条 5–6 DIP，可键盘聚焦和拖动。
- 主 3D 区不得设置固定 Height；在 1920×1080 常见 Shell 布局中目标可见高度不少于 420 DIP。
- 下方证据区默认 240 DIP，允许 160–360 DIP。
- 工作区根不再使用同时包裹全部内容的双向 `ScrollViewer`。
- 只有左侧当前页、下方当前证据页以及确实超宽的局部数据允许独立滚动。
- 不使用 `Viewbox`、`ScaleTransform`、`LayoutTransform` 或改变全局字号来“适配”空间。

### 4.2 左侧任务检查器

使用项目已有现代 Tab 样式，四页内容严格为：

| 页 | 现有内容 |
|---|---|
| 生成 | 参考图、PAL、设计说明、高级参数、Provider 进度、生成按钮 |
| 几何 | 当前来源、GLB 候选、生成候选、AI 识别结构、来源/候选事实 |
| 风格 | 自然语言风格要求、继承来源、编译预览入口 |
| 输出 | 最终候选状态、固化最终候选、导出 VOX、导出状态 |

规则：

- 不删除任何已有入口、提示、禁用条件或确认门。
- 当前页之外的控件不参与布局测量。
- 每页最多一个主动作按钮；次要动作保持普通紧凑按钮。
- 长说明默认使用简短摘要；现有合规/费用/不写盘提示必须保留，允许放入页内次级说明区，但不得隐藏关键风险。

### 4.3 中央 3D 审阅区

- 模式按钮放在一行可换行的紧凑工具栏，不使用等宽大按钮。
- 3D 视口填满剩余空间。
- 当前模式、颜色图例和交互提示使用轻量 Overlay，不占用额外固定行。
- 鼠标交互维持：拖动旋转、中键或 Shift+拖动平移、滚轮缩放、双击重置。
- 切片诊断保留为显式辅助入口，不重新成为默认主预览。
- 场景构建中只覆盖视口内状态层，不让整页重新测量。

### 4.4 下方证据区

四页内容严格为：

| 页 | 现有内容 |
|---|---|
| 质量 | 质量指标、候选比较、准入事实 |
| 结构 | 语义区域、结构事实、差异/结构图例 |
| 着色 | 颜色角色、区域规则、色板/对比度事实 |
| 审阅 | 组合审阅、风险、待确认事项 |

要求：

- 默认打开“审阅”页；如没有审阅内容，打开“质量”页。
- 页签显示精简计数或状态，不重复大段标题。
- 长列表在页内滚动，禁止推动整个 3D 区向上或向下跳动。
- 不改任何质量判定、结构识别、颜色角色或风险文本本身。

### 4.5 状态与错误

- 顶部标题区只保留稳定来源摘要和主操作。
- 普通成功/进行中状态压缩为单行，可截断并提供 ToolTip。
- 错误状态使用现有危险色，并允许最多两行；不得因为长错误文本改变主布局。
- Provider 调用进度只在“生成”页和全局进度条中出现，不重复堆叠。

## 5. 分辨率、DPI 与稳定性契约

### 5.1 主验收分辨率

主验收基准：1920×1080，Windows 100% 和 125% 显示缩放。

补充压缩验证：

- 1600×900 / 100%；
- 1366×768 / 100%；
- 若环境可用，1920×1080 / 150%。

### 5.2 响应式规则

- 可用文档宽度 ≥ 1180 DIP：默认 312 DIP 左栏。
- 900–1179 DIP：左栏默认 280 DIP，模式栏允许换行，下方证据页保持。
- < 900 DIP：维持最小 260 DIP 左栏和 520 DIP 预览；只允许工作区主体出现局部水平滚动作为最后降级，不得缩放字体或 3D。
- 高度不足时，优先压缩下方证据区到 160 DIP；左栏和证据页分别滚动，主 3D 区不得低于 260 DIP。

### 5.3 禁止的伪适配

- 禁止按分辨率动态改变 `FontSize`。
- 禁止对根元素或视口使用缩放变换。
- 禁止在 `SizeChanged` 中持续调用自动取景。
- 禁止通过隐藏主功能来通过截图验收。

## 6. 视觉语言契约

- 复用现有 `Ui*` 和 `IdeWorkspace*` Token，不新增平行主题体系。
- 根区使用连续工作区背景和 1 DIP 分隔线；减少多层圆角卡片嵌套。
- 字号、字体、按钮高度沿用全局现代化基线。
- 选中态使用 Accent 下划线/软背景；禁用态仍需可辨识。
- 3D 区视觉权重最高；左侧是工具检查器，下方是证据，不得三者同权。
- 不使用原生默认 TabControl、Button、ComboBox 外观；必须显式复用项目主题样式。

## 7. AutomationId 契约

现有 `Ra2VoxelStyleWorkspaceUiContractTests` 中全部 `VoxelStyle.*` AutomationId 必须保留，名称和语义不得变化。

新增：

```text
VoxelStyle.Layout.InspectorSplitter
VoxelStyle.Layout.DetailsSplitter
VoxelStyle.Workflow.Tabs
VoxelStyle.Workflow.Generation
VoxelStyle.Workflow.Geometry
VoxelStyle.Workflow.Style
VoxelStyle.Workflow.Output
VoxelStyle.Preview.Toolbar
VoxelStyle.Details.Tabs
VoxelStyle.Details.Quality
VoxelStyle.Details.Structure
VoxelStyle.Details.Colour
VoxelStyle.Details.Review
```

键盘要求：

- Tab 顺序按“顶栏 → 左侧当前任务 → 预览工具栏 → 下方当前证据”执行。
- 两个 GridSplitter 可获得键盘焦点并使用方向键调整。
- 页签支持方向键切换。
- 3D 视口获得焦点后不吞掉全局菜单快捷键。

## 8. 允许修改的文件

实施阶段只允许：

```text
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceUiContractTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelViewportCameraStateTests.cs              # 如需要，新建
Docs/ASSET-VOX-UI-R1_*.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/DecisionLog.md
Docs/UserGuide.md                                                       # 仅在用户可见操作变化后
```

若实现证明必须增加一个 IDE-internal、非序列化的相机状态类型，可在以下目录新增单一文件：

```text
RA2IniEditor.IDE/Views/AssetAuthoring/
```

该类型不得被 Application/Core/Automation 引用。

## 9. 禁止修改的文件与语义

禁止：

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/Themes/*.xaml                      # 本阶段使用现有 Token，避免全局连带变化
RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelViewportSceneBuilder.cs
RA2IniEditor.Application/**
RA2IniEditor.Core/**
字段库、INI、AI Work、Project Apply/Save、Undo/Redo 相关文件
```

若实施中发现必须触及其中任一项，停止该子阶段并回报，不以“UI 所需”为由扩大范围。

## 10. 连续实施计划

### UI-R1-0 Characterization Gate

- 固化当前 AutomationId、Provider 边界和场景构建测试。
- 添加相机状态纯逻辑测试入口或可测内部契约。
- 证明当前无根级 Viewbox/ScaleTransform。
- 不产生用户可见变化。

### UI-R1-1 Camera Stability

- 建立相机姿态捕获、归一化恢复和相机分组。
- 场景更新改为显式 `Fit` / `Preserve` 决策，不再无条件重置。
- 处理取消、过期构建、临时 Unloaded/Loaded。
- 完成相机单测后审查再进入下一步。

### UI-R1-2 Workspace Recomposition

- 移除全页双向 ScrollViewer。
- 建立左侧检查器、主预览、下方证据区及两个 splitter。
- 先保持所有现有控件和绑定原样迁移。
- 完成 XAML 编译和 AutomationId 审查后进入下一步。

### UI-R1-3 Task and Evidence Navigation

- 将现有功能移动到四个任务页和四个证据页。
- 设置默认页、焦点顺序、空状态和错误状态。
- 不修改 ViewModel 业务属性和命令。

### UI-R1-4 Responsive and Visual Polish

- 应用现有主题样式、压缩冗余卡片、校准分隔与留白。
- 加入不依赖缩放变换的窄宽/低高降级。
- 审查 3D 视口最小尺寸和滚动边界。

### UI-R1-5 Verification and Handoff

- 运行聚焦测试、全量测试、构建和干净包。
- 请求用户进行 1920×1080 的 100%/125% 手工截图和相机稳定性验收。
- 更新上下文、当前阶段和用户指南。

每个子阶段完成后执行自审；契约批准后连续实施，不因普通低风险调整再次请求批准。只有越界、必选测试失败无法修复或需要真实模型调用时停止。

## 11. 自动化验收

### 11.1 必须新增/调整的测试

至少覆盖：

1. 现有和新增 AutomationId 全部存在。
2. 根工作区不存在全页双向 ScrollViewer、Viewbox 或缩放变换。
3. 左侧和下方存在独立 splitter 与现代 Tab 样式。
4. 同一相机组替换场景后 yaw/pitch/normalized target/distance ratio 保持。
5. 新来源组第一次加载会自动取景。
6. 用户 Reset 后相机回到有效默认视角。
7. 非法/NaN/Infinity 相机状态安全回退。
8. 取消或过期场景构建不覆盖成功场景和相机状态。
9. `Unloaded -> Loaded` 后同一文档实例恢复视角。
10. Provider、写盘、导出、结构识别调用边界不因 UI 重排改变。

### 11.2 命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Ra2VoxelStyleWorkspace|FullyQualifiedName~Ra2VoxelViewport"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

任何失败不得以“UI 修改不影响逻辑”为由跳过。

## 12. 手工验收矩阵

| 场景 | 验收标准 |
|---|---|
| 1920×1080 / 100% | 3D 为主视觉区；无整页横滚；控件无截断/重叠 |
| 1920×1080 / 125% | 字体和控件不整体缩放跳变；两个 splitter 可用 |
| 1366×768 / 100% | 各页可独立滚动；主 3D 高度不低于 260 DIP |
| Dock 宽度连续拖动 | 页面只重排，不突然改变字体或相机距离 |
| 用户旋转并缩放后切换全部预览模式 | 视角保持，不自动复位 |
| 切换到别的文档再返回 | 同一工作区实例恢复原视角 |
| 选择新的模型来源 | 首次场景自动取景一次 |
| 场景生成中快速切换模式 | 旧结果不覆盖新模式；视角不被过期任务重置 |
| 左/下 splitter 拖到边界 | 受最小/最大值限制，不覆盖其他区域 |
| 导出 VOX | 入口、禁用态、确认和结果与 3B 基线一致 |

## 13. 回滚规则

- 相机稳定性和布局重组必须是可分别回滚的提交/补丁边界。
- 若相机测试失败，保留旧布局，不进入视觉重组。
- 若 XAML 编译或 AutomationId 回归失败，停止并恢复该子阶段，不修改业务层绕过。
- 若 1920×1080 主截图未达到布局契约，停在视觉审查，不继续改生成/算法模块。

## 14. 自审结果

### 14.1 已解决的原计划缺口

- 将“页面缩放”和“相机缩放”拆成两个独立根因与验收项。
- 明确临时 Unloaded 与新来源加载的相机行为，避免简单删除清理逻辑造成资源问题。
- 不把相机状态塞入业务 ViewModel 或用户配置，避免持久化返工。
- 不修改 Shell 即可完成生命周期修复，符合 Shell Freeze。
- 所有现有功能按任务页迁移而非删除，避免 UI 美化导致能力回退。
- 主分辨率、窄屏和 DPI 验收均有量化边界。
- 明确禁用根级缩放变换和持续自动取景，避免“视觉上暂时正常”的伪修复。

### 14.2 仍需人工验证的风险

1. WPF/AvalonDock 在真实 125%/150% 环境中的布局只能通过实际截图最终确认。
2. 纯源码测试能证明相机策略，但鼠标手感和 splitter 灵敏度仍需人工验收。
3. 当前 Shell 不显式 Dispose Viewport；本契约通过会话级状态和 Unloaded 清理规避，不在本阶段扩大 Shell 修改。
4. 大模型场景构建耗时可能影响模式切换感受，但本阶段不改变 SceneBuilder 性能；只保证异步结果和相机不会乱序覆盖。

### 14.3 最终裁决

契约通过自审，满足实施前提；它不会要求修改 Shell、业务 ViewModel、Provider、算法或写盘链路。实现仍属于 R3，因此必须在用户明确批准 `ASSET-VOX-UI-R1` 后开始。
