# ASSET-VOX-4B-STROKE-1 — Continuous Semantic Painting Code-Fact Audit

状态：Reviewed / approved contract implemented / automated verified  
日期：2026-08-30

## 1. 当前任务目标

在已验收的 FIX2 精确表面命中基础上，把语义画笔从“每次单击一个原子操作”扩展为“按下、连续拖动、释放后
一次提交”的笔划；同时让语义主视图可在“部件 / 材质”两个审阅维度之间切换，并以固定高对比色区分不同部件。

本文件记录实施前代码事实；其审计结论已被用户批准，运行时交付结果见同阶段最终契约与 Stage Ledger。

## 2. 当前实现事实

### 2.1 指针与精确命中

- `Ra2VoxelViewport3D` 的完整 `InputSurface` 已统一接收鼠标输入。
- 左键按下立即调用精确语义命中；右键 Orbit、Shift+右键/中键 Pan、滚轮 Zoom。
- `Ra2VoxelViewportSceneHitMap` 以当前 `GeometryModel3D` 身份和命中三角形顶点索引解析准确的
  canonical `Ra2VoxelCoordinate`，不存在最近体素中心回退。
- 场景、snapshot、evidence、coordinate index 和 hit map 在同一 scene generation 中替换。

### 2.2 现有画笔与历史

- `Ra2VoxelSemanticMaskEditor.ApplySurfaceBrush` 是 Application 内部的唯一表面画笔实现；它按一个 seed、
  半径 0/1/2、可选 X 镜像和 Paint/Erase 生成新的不可变人工蒙版层。
- `Ra2VoxelStyleWorkspaceViewModel.HandleSemanticCellClick` 每调用一次就执行一次画笔、压入一个 undo 项、
  清空 redo、替换 composition，并触发正式语义场景刷新。
- 因此，直接在 `MouseMove` 中重复调用现有入口会造成大量历史项、重复 composition 和异步整场景重建；
  这不是可靠的连续画笔实现。

### 2.3 当前语义显示

- `SemanticMask` 视图目前只按 `MaterialRole` 着色。
- 当前材质色为：涂装面绿、玻璃青、橡胶深灰、裸金属银灰、灯光黄、暗部深紫、强调橙、Unknown 紫。
- `Ra2VoxelSemanticEffectiveAssignment` 已同时携带 `PartRole` 与 `MaterialRole`，因此增加部件审阅维度不需要
  新语义事实或第二套 composition。
- 当前部件枚举是 `Unknown / BodyShell / Turret / Barrel / Wheel / Track / Antenna / Attachment`。

### 2.4 当前 UI

- 现有“语义”详情页已有浏览/画笔/擦除、大小 1–3、镜像、撤销/重做、部件/材质目标和阵营色人工批准。
- 现有 AutomationIds 必须保留：
  `VoxelStyle.Semantics.EditMode`、`BrushSize`、`MirrorBrush`、`Undo`、`Redo`、`EditStatus`、
  `PartRows`、`MaterialRows`、`RemapApproval`，以及 `VoxelStyle.Preview.Viewport3D`。
- 无需增加窗口、Shell 面板、第三列布局或持久化设置。

## 3. 根因与正确复用路径

连续画笔的缺口不是命中算法，也不是新的语义模型，而是缺少笔划事务：

```text
精确可见表面命中
  -> 有序去重的 seed 路径
  -> 一次确定性多 seed 表面画笔
  -> 一次人工蒙版替换
  -> 一个 undo 项
  -> 一次正式语义场景重建
```

正确复用路径：

- 保留 FIX2 的精确 face hit map，不引入射线猜测或屏幕最近点算法。
- 将现有单 seed 表面画笔收敛到一个多 seed 权威实现；单击路径成为一 seed 的兼容适配。
- Viewport 只收集 canonical seed 并绘制临时路径 overlay；不修改蒙版。
- ViewModel 冻结笔划开始时的工作 hash、基础蒙版和画笔设置，释放时一次提交。
- 正式 composition、着色、固化和 VOX 导出继续复用 4B 既有链路。

## 4. 数据所有权

| 状态 | 唯一所有者 | 生命周期 | 是否权威 |
|---|---|---|---|
| 鼠标捕获、屏幕采样点、去重 seed、临时路径 overlay | Viewport | 一条指针笔划 | 否，presentation-only |
| 基础 layer、冻结画笔设置、snapshot hash、提交/取消、undo/redo | ViewModel | 当前工作几何会话 | 是，会话编辑事务 |
| 多 seed 表面扩展、镜像、Paint/Erase、不可变结果 layer | Application editor | 单次纯函数调用 | 是，确定性语义编辑 |
| 部件/材质审阅维度 | IDE ViewModel | 当前工作区会话 | 否，只改变显示 |
| 最终 effective assignment / composition | 现有 composer | 当前 working hash | 是，既有权威 |

屏幕坐标、颜色和临时 overlay 均不得进入 canonical snapshot、manual layer hash、composition hash 或文件格式。

## 5. 风险分类

- 当前文档变更：`R0 / DocsOnly`。
- 拟议实现：`R3 / StopForReview`。
- 原因：会改变鼠标捕获、笔划生命周期、撤销粒度、正式刷新次数和语义显示维度，但不改变 public API、
  持久化、几何、writer、项目 Apply/Save 或模型调用边界。
- 未发现升级 R4 的条件；若实现中必须修改 public API、VOX/VXL/HVA 写出、持久化或 Shell，应立即停止并重新立约。

## 6. 代码事实审计结论

该功能可以在现有边界内可靠实现。关键不是提高 Host 语义限制，而是把连续手势变成一个 hash-bound、
可取消、一次提交的事务，并让所有最终语义变化仍由现有 Application editor 和 composition 产生。
