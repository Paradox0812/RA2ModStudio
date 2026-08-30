# ASSET-VOX-4B-FIX2 — Reliable Pointer Interaction Code-Fact Audit

状态：Reviewed / contract gate only / runtime unchanged  
日期：2026-08-30

## 1. 用户可见故障

- 用户已进入画笔并选择有效部位/材质，但左键短点击仍可能完全没有反馈。
- 左键同时承担相机旋转和语义绘制。绘制只在鼠标抬起且移动距离不超过 4 DIP 时尝试，因此轻微手抖、捕获
  变化或事件顺序都可能把一次绘制吞掉。
- 当前帮助文案写“拖动旋转”，没有说明左键与语义画笔之间的冲突。
- 用户要求右键旋转，并且鼠标位于整个主视图的模型或空白区域时都能开始旋转。

## 2. 当前实现事实

### 2.1 输入所有权冲突

`Ra2VoxelViewport3D` 在 `MouseLeftButtonDown` 立即捕获鼠标并进入 Orbit；`MouseLeftButtonUp` 才以 4 DIP 阈值
判定是否属于语义短点击。同一个左键手势因此先是相机手势，之后才可能被重新解释为画笔。

### 2.2 命中坐标是猜测值

`VisualTreeHelper.HitTest` 返回实际命中的渲染三角形，但现实现丢弃了三角形身份，转而遍历全部 occupied cell，
用 `PointHit` 到体素中心的距离选择最近体素。该过程是 O(N)，并在共边、共角、深度接近和大模型情况下存在
歧义，不能作为绘制权威。

### 2.3 SceneBuilder 已具备精确事实

`Ra2VoxelSurfaceProjector` 已为每个外露 quad 提供 canonical `Ra2VoxelCoordinate`。SceneBuilder 为每个面追加
四个独立顶点和两个三角形，且按颜色批次保留稳定追加顺序。因此无需新几何算法：只需让构建结果保留
`GeometryModel3D/triangle -> canonical coordinate` 的派生命中表。

### 2.4 主视图空白区输入

鼠标事件当前挂在 `Viewport3D` 上。外层 Grid 已有非透明背景并覆盖完整主视图，适合作为统一输入表面；由该
Grid 捕获右键/中键可保证从模型或空白处都能开始相机拖动。

## 3. 根因判定

前一轮修正只解决了语义区域准备和区域行预选依赖，没有消除左键的双重所有权，也没有修正猜测式命中。
因此自动 ViewModel 测试可通过，而真实 WPF 指针仍不可靠。必须同时修正输入所有权和面坐标映射；继续调整
短点击阈值或增加重试不会解决根因。

## 4. 可复用路径

- 复用 `Ra2VoxelSurfaceProjector` 的外露面顺序与 coordinate。
- 复用 SceneBuilder 的 `MeshBatch`、每面四个独立顶点和颜色批次。
- 复用 WPF `RayMeshGeometry3DHitTestResult.ModelHit` 与三个 vertex index。
- 复用现有 `SemanticCellSelected`、ViewModel 画笔、镜像、undo/redo 和 composition。
- 不引入第三方 3D/拾取依赖，不回退到屏幕空间最近点启发式。

## 5. 风险与边界

- 风险：R3；改变主视图输入生命周期和 IDE 内部场景结果契约。
- 新命中表只属于 presentation-derived scene lifetime；场景替换/取消/清空时同步失效。
- 不修改 Application 体素几何、蒙版、着色或 snapshot；不持久化命中表。
- 不修改 Shell、Provider、Apply/Save、VOX/VXL/HVA writer、INI、Field Registry 或 public API。

