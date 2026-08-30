# ASSET-VOX-4B-FIX2 — Reliable Pointer Ownership and Exact Surface Hit Final Contract

状态：Approved / implemented / automated verified / physical WPF smoke pending  
日期：2026-08-30

## 1. 目标

彻底消除主 3D 视图中左键旋转与语义绘制的竞争，并让每个渲染外露面精确映射回 canonical voxel。完成后，
左键点击模型必须产生选择/绘制/擦除之一的明确结果；右键从主视图任意位置拖动都可旋转。

## 2. 固定鼠标契约

| 输入 | 模型表面 | 主视图空白处 |
|---|---|---|
| 左键单击，浏览模式 | 精确选择命中体素所属语义区域 | 不改变选择，显示“未命中模型表面” |
| 左键单击，画笔模式 | 立即执行一次现有大小 1/2/3 表面画笔 | 不修改蒙版，显示“未命中模型表面” |
| 左键单击，擦除模式 | 立即执行一次现有表面擦除 | 不修改蒙版，显示“未命中模型表面” |
| 左键拖动 | 不旋转、不平移；本阶段不定义连续笔划 | 不执行操作 |
| 右键拖动 | 旋转 | 旋转 |
| Shift + 右键拖动 | 平移 | 平移 |
| 中键拖动 | 平移 | 平移 |
| 滚轮 | 缩放 | 缩放 |

- 左键按下时立即命中并执行，不再等待 MouseUp 或使用 4 DIP 阈值。
- 右键和中键由覆盖完整主视图的输入表面捕获；释放对应按键、失去捕获或控件卸载时必须结束相机手势。
- 本阶段不加入左键连续拖涂，避免在异步场景重建中制造重复笔划和非原子 undo；单击画笔必须先可靠。
- 移除双击左键重置相机。重置继续使用现有“重置视角”按钮，防止双击绘制被解释为相机命令。

## 3. 精确命中契约

SceneBuilder 在构建每个颜色批次时同时记录每个 quad 的 canonical coordinate。构建结果携带一个内部、冻结
语义的命中表：

```text
GeometryModel3D identity
  + hit triangle vertex indices
  -> face ordinal within that model
  -> exact Ra2VoxelCoordinate
```

不变量：

- 每个 quad 继续拥有四个独立顶点和两个三角形；同一三角形的三个 vertex index 必须解析到同一 face ordinal。
- 命中表覆盖 SceneBuilder 发布的每个外露面，不采样、不截断。
- 命中表与 Model3DGroup 属于同一个 scene generation；不得跨场景复用。
- 无映射、场景过期或非模型命中返回明确的本地失败，不退回最近体素算法。
- 该表为 IDE internal、session-only、non-serialized，不进入 canonical snapshot 或 public API。

## 4. 场景与 ViewModel 边界

- Viewport 只负责将 WPF 命中翻译成 canonical coordinate，并从当前 hash-matched evidence 解析 region ID。
- ViewModel 继续负责模式、画笔目标、镜像、人工蒙版、历史、composition 和状态文本。
- 成功的左键画笔继续调用既有 `HandleSemanticCellClick`；不得建立第二套蒙版或着色链。
- 场景刷新必须保留现有相机连续性；命中表与新场景一次性替换，不能出现新模型配旧命中表。

## 5. UI 契约

- 不改变工作区布局、工具条尺寸或 AutomationId。
- `VoxelStyle.Preview.Viewport3D` 保留。
- 主视图左下帮助文字改为：
  `左键选择/绘制 · 右键拖动旋转 · Shift+右键/中键平移 · 滚轮缩放`。
- 画笔成功继续显示影响体素数并启用撤销；空白点击和无法解析的面必须显示可读状态，不能静默无响应。
- 不增加弹窗、侧栏、上下文菜单或全局快捷键。

## 6. 允许文件

```text
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelViewportSceneBuilder.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs
RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs（仅状态反馈/既有入口）
RA2IniEditor.Tests/IDE/Ra2VoxelViewportSceneBuilderTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelViewportCameraStateTests.cs 或同目录新的 pointer contract tests
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceViewModelTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceUiContractTests.cs
Docs/ASSET-VOX-4B-FIX2_*.md 及项目状态/决策文档
```

## 7. 冻结边界

不修改 Shell、Application 几何/蒙版/着色算法、Provider、真实 DeepSeek/Tencent 调用、Apply/Save、VOX/VXL/HVA
写出、项目文件、public API、持久化、INI、Field Registry 或 legacy。

## 8. 实施阶段

| 阶段 | 内容 |
|---|---|
| FIX2-0 | 契约、事实审计、风险和输入所有权审批门。 |
| FIX2-1 | SceneBuilder 增加精确、场景绑定的 face hit map 及核心测试。 |
| FIX2-2 | 主视图输入改为左键操作、右键任意位置旋转、中键/Shift+右键平移。 |
| FIX2-3 | 空白/失败反馈、ViewModel 既有画笔接线和相机连续性审计。 |
| FIX2-4 | 定向测试、Debug build、差异审计、文档和手工验收说明。 |

## 9. 验证门

- SceneBuilder 单体素六面全部可由两个三角形精确解析回同一坐标。
- 多体素、同颜色批次和不同颜色批次的每个 face mapping 均正确。
- XAML contract 断言右键相机、左键操作以及更新后的帮助文本；旧左键 MouseUp/Orbit 接线不存在。
- ViewModel 回归覆盖浏览、画笔、擦除、镜像、undo/redo、无需预选区域行。
- 顺序执行 Debug build 与 affected IDE tests；不得并行写同一个 WPF `obj`。
- 物理 WPF 验收：模型/空白处右键旋转，模型左键立即上色，空白左键有反馈，拖动相机不触发画笔。

## 10. 自审结果

- 方案直接移除输入竞争和最近中心猜测，没有增加阈值、重试或 Host 语义限制。
- 命中身份来自已有外露面权威，数据生命周期清晰，不影响 canonical geometry。
- 没有 public API、序列化、writer 或 Shell 依赖。
- 唯一明确延期是连续拖涂；它需要独立的 stroke/undo/场景刷新节流契约，不应混入本次可靠性修复。
- 结论：契约已按用户批准的 FIX2-0 → FIX2-4 连续实施；自动化门已通过，真实鼠标交互仍需用户在新进程中验收。

## 11. 实施结果

- SceneBuilder 为每个已发布外露 quad 保留场景生命周期内的规范体素坐标，并以命中模型身份和三角形顶点索引精确解析。
- 主输入表面统一拥有鼠标事件：左键只选择/绘制/擦除，右键在模型或空白处旋转，Shift+右键/中键平移，滚轮缩放。
- 清空场景、释放控件或丢失捕获都会终止相机手势；旧左键 Orbit、MouseUp 短点击和最近中心回退均已删除。
- 空白、非语义场景、场景映射过期和未归区体素均返回可读反馈；只有后两类作为错误状态。
- 自动验证：精确命中/相机/ViewModel/UI contract 35/35；Debug build 0 warning / 0 error。详见阶段账本。
