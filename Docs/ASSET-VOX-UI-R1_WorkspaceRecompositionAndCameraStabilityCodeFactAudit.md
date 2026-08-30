# ASSET-VOX-UI-R1 Workspace Recomposition and Camera Stability Code-Fact Audit

> 状态：代码事实审计完成；未进入实现。
>
> 审计日期：2026-08-28

## 1. 审计目标

本审计回答两个问题：

1. 为什么当前体素工作区呈现出明显的表单堆叠感、空间利用率差；
2. 为什么用户会观察到界面或模型预览偶发“突然放大/缩小”。

本次只读取直接相关实现和测试，没有修改 XAML、相机、Shell、生成链路或模型文件。

## 2. 已确认的布局事实

权威实现：

- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml`
- `RA2IniEditor.IDE/Themes/IdeWorkspaceStyles.xaml`
- `RA2IniEditor.IDE/Themes/IdeCollectionStyles.xaml`

已确认：

1. 主内容被一个同时允许水平和垂直滚动的外层 `ScrollViewer` 包裹。
2. 外层内容 Grid 使用 `MinWidth="780"`、`MinHeight="620"`，并以 `0.34* / 0.66*` 分配左右区域。
3. 左侧使用一个长 `StackPanel`，同时展示生成、来源、几何候选、风格要求、最终候选等多个流程阶段。
4. 右侧 3D 预览使用固定 `Height="360"`，其下继续同时展开质量、语义区域、组合审阅、颜色角色、区域规则和审阅事项。
5. 页面已有统一主题资源，但局部仍大量采用独立卡片边框，信息密度和层级没有按“当前任务 / 主预览 / 次级证据”组织。
6. 当前只有左右区域之间的纵向分隔调节；3D 预览与下方证据区不能独立调整高度。

结论：当前页面是“功能逐批追加后的纵向表单集合”，不是面向审阅任务的 IDE 工作区。外层滚动范围、固定预览高度、最小尺寸和嵌套内容共同导致 Dock 宽高变化时出现明显重排。

## 3. 已确认的相机事实

权威实现：

- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml.cs`

已确认：

1. 相机状态只保存在 `Ra2VoxelViewport3D` 的私有字段：目标点、距离、偏航和俯仰。
2. `SetSceneAsync` 每次成功替换场景模型后无条件调用 `ResetCamera()`。
3. 工作区的渲染键包含快照、差异参考、保护蒙版、区域蒙版、语义分区和颜色模式；切换预览模式通常会产生新渲染键并重建场景。
4. 因而用户旋转、平移或缩放后的视角，会在模式切换或结果更新后被强制恢复到默认视角。
5. 工作区 `Unloaded` 时会清空场景和渲染键；再次 `Loaded` 时会重建场景，而重建又会强制重置相机。
6. AvalonDock 文档切换或可视树重挂载可能触发这种临时卸载/加载路径。
7. 代码中未发现本工作区使用 `Viewbox`、`ScaleTransform` 或 `LayoutTransform`。

结论：模型预览“突然放大/缩小”存在确定的代码原因——场景重建后无条件自动取景。整个页面尺寸跳动则更可能来自外层滚动布局重新测量。两者必须分别修复。

## 4. 生命周期事实

Shell 在打开体素工作区时创建单一不可浮动的 `LayoutDocument`；关闭文档时会释放 ViewModel，但当前不会显式调用 View/Viewport 的 `Dispose()`。

当前 `Unloaded` 负责取消渲染并清空场景，因此不能简单删除该处理。正确方案应区分：

- 临时卸载：取消正在进行的构建，保留轻量相机状态，重新加载后恢复；
- 场景来源改变：清除旧相机分组并进行一次初始取景；
- 真正释放：取消任务并释放场景对象。

本阶段不得为了处理该生命周期去修改 Shell；应在工作区和 Viewport 内建立可验证的会话级相机状态。

## 5. 现有测试缺口

现有测试覆盖：

- 工作区 AutomationId 和关键入口；
- 3D 场景构建、颜色模式和资源限制；
- 非 Helix 的原生 WPF 3D 边界；
- Provider 只在显式调用路径创建。

未覆盖：

- 场景更新后是否保留相机；
- 模式切换后是否保留视角；
- 临时卸载/加载后是否保留视角；
- 新来源是否只自动取景一次；
- 工作区在不同可用宽高下是否出现整页滚动或控件重叠；
- 100%、125%、150% 显示缩放下的人工视觉表现。

## 6. 复用结论

本阶段不需要引入新的 UI 或 3D 依赖：

- 继续使用现有原生 WPF `Viewport3D`；
- 继续使用 `UiTabControlStyle` / `UiTabItemStyle`、`IdeWorkspaceCommandBarStyle`、`IdeWorkspaceCommandButtonStyle`、`UiGridSplitterStyle` 和现有颜色/字体 Token；
- 继续使用现有 ViewModel 命令和所有业务属性；
- 只重组呈现层并补足相机生命周期。

## 7. 审计结论

`ASSET-VOX-UI-R1` 应被评为 R3 局部高风险 UI 修改：视觉范围仅限体素文档，但涉及 WPF 测量、AvalonDock 生命周期、异步场景替换和相机状态。实施前必须有精确契约、自动化回归和 1920×1080 手工截图验收。

