# ASSET-VOX-4B-STROKE-1 — Continuous Semantic Painting Final Contract

状态：Approved / implemented / automated verified / physical WPF acceptance pending  
日期：2026-08-30  
风险：R3 / StopForReview

## 1. 目标与可见结果

在现有 4B/FIX2 语义编辑器中增加可靠的连续表面笔划：

- 左键按下有效模型表面：开始笔划并捕获鼠标。
- 左键拖动：沿屏幕轨迹按固定间距重复执行 FIX2 精确表面命中。
- 左键释放：一次性应用整条笔划。
- 一条笔划最多产生一个 undo 项和一次正式语义场景重建。
- 拖动期间只显示轻量临时路径高亮，不提前修改人工蒙版。
- 语义审阅可在“部件 / 材质”之间切换；不同部件使用固定高对比标注色。

本阶段只编辑可见外露表面的人工作业层，不修改几何、占用、源 VOX/VXL、palette index 或 AI 建议。

## 2. 输入与相机契约

| 输入 | 画笔/擦除模式 | 浏览模式 |
|---|---|---|
| 左键按下有效表面 | 开始笔划，记录首个精确 seed，捕获鼠标 | 立即精确选择区域，不捕获笔划 |
| 左键拖动 | 连续采样；不旋转、不平移、不正式提交 | 不绘制、不移动相机 |
| 左键释放 | 有有效 seed 时原子提交；否则无修改结束 | 无额外动作 |
| 左键按下空白 | 不开始笔划，显示“未命中模型表面” | 不改变选择并显示反馈 |
| 右键拖动 | 若存在未提交笔划，先取消；随后从模型或空白处 Orbit | Orbit |
| Shift+右键/中键拖动 | 若存在未提交笔划，先取消；随后 Pan | Pan |
| 滚轮 | Zoom；不提交笔划 | Zoom |

- 本阶段只在 Paint/Erase 模式把 FIX2 的“左键按下立即应用”改为“左键释放提交”；单击仍等价于一 seed
  笔划，用户可见效果不丢失。
- 右键相机所有权、完整主视图输入表面和现有重置按钮保持不变。
- 笔划期间的右键/中键请求优先取消未提交编辑，再正常开始相机手势；不得出现半提交状态。

## 3. 精确连续采样契约

### 3.1 轨迹采样

- 采样坐标使用 WPF DIP，不使用设备像素；缩放/DPI 不改变逻辑间距。
- 每次 MouseMove 从上一个处理点到当前点进行线性插值，最大相邻采样距离固定为 `4 DIP`，并包含终点。
- 每个插值点必须独立走 FIX2 的 `VisualTreeHelper.HitTest + scene hit map`；只接受当前视角最前面的实际
  外露面，不能把前一次命中延伸到空白，也不能穿透到背面。
- 空白插值点只是不产生 seed，不取消整条笔划；重新进入模型表面后可以继续采样。
- canonical coordinate 使用插入顺序列表与 HashSet 去重。同一个体素在一条笔划中只保留第一次命中。
- 单次 MouseMove 最多处理 `4096` 个插值点；单条笔划最多保留 `8192` 个唯一 seed。超过任一上限时取消
  整条笔划，保留基础 layer，并显示资源上限原因；不得部分提交或静默截断。

### 3.2 场景一致性

笔划开始时冻结：

```text
scene generation
working snapshot hash
manual base-layer hash
edit mode
brush radius
mirror flag
part/material/remap assignment
```

任何场景替换/清空、working hash 变化、semantic evidence/composition 换代、编辑模式切换、控件卸载、Dispose、
失去鼠标捕获或显式相机手势，均取消笔划。取消只清理临时状态，不修改 layer、history、style preview 或文件。

## 4. 笔划事务与 Application 契约

### 4.1 唯一执行入口

扩展现有 `Ra2VoxelSemanticMaskEditor`，增加内部多 seed 入口；不得新建第二个蒙版编辑器：

```text
ApplySurfaceStroke(
  snapshot,
  baseLayer,
  orderedUniqueSeeds,
  radius,
  mirror,
  mode,
  assignment)
  -> immutable result layer + unique changed-cell count + typed failure
```

- 现有 `ApplySurfaceBrush` 保留为兼容入口，但必须委托给同一多 seed 核心，传入一个 seed。
- 对每个 seed 复用现有六邻域外露表面半径 0/1/2 规则；所有 footprint 先并集去重，再统一镜像、Paint/Erase。
- 镜像只加入当前 snapshot 中实际存在的 occupied cell。
- Paint 继续要求明确 part/material，remap 仅接受人工批准；Erase 只移除 cell override 并显露下层结果。
- 结果相对 base layer 计算 unique changed-cell count。零变化返回 `NoChange`，不产生历史项。
- 输入为空、seed 不存在、snapshot/layer 不匹配、非法 assignment 和资源超限均为类型化失败；不得弱化为部分成功。

### 4.2 原子历史

- Begin 时保存 base layer 引用，不压入历史。
- Commit 成功且 layer hash 发生变化时：把 base layer 压入 undo 一次、清空 redo 一次、发布 result layer 一次。
- 一条笔划无论包含多少 seed、半径扩展和镜像副本，只产生一个 undo 项。
- 成功 Commit 继续复用现有 4B 失效规则：清除由旧 composition 派生的 style preview 与已固化候选，切回
  Semantics 视图，并要求用户重新编译/审阅着色；整个失效过程只执行一次。
- Commit 失败、NoChange 或 Cancel：undo/redo、manual layer、composition 和 style preview 均不变。
- 继续沿用现有 100 项历史上限；不新增持久化或跨会话撤销。

## 5. 临时预览与正式刷新

- Viewport 持有独立 `StrokePreviewVisual`；它与正式 `SceneVisual`、hit map 和 canonical snapshot 分离。
- 临时高亮只显示已精确命中的去重 seed 路径，明确不冒充半径/镜像扩展后的最终 footprint。
- Paint 路径使用高亮黄 `#FFD400`；Erase 路径使用高亮红 `#FF3B30`。这是手势反馈色，不是部件、材质或最终色板色。
- 临时 overlay 最多以 30 Hz 更新；不得调用正式 `SetSceneAsync`，不得计算 composition，不得写历史。
- overlay 只消费当前 snapshot 的坐标到世界空间转换；应复用现有 SceneBuilder 的体素坐标变换/批处理逻辑，
  不建立第二套模型坐标约定。
- Release 成功后清理 overlay，并对新 composition 只触发一次正式语义场景重建。
- Cancel/NoChange/失败立即清理 overlay；失败文本进入现有 `SemanticEditStatus`，不弹窗。

## 6. 部件 / 材质审阅维度

### 6.1 显示语义

- 新增 IDE-internal、session-only 显示枚举：`Part / Material`。
- 初次进入语义编辑默认 `Part`，因为人工边界修订的首要任务是区分部件；用户切换后在当前工作区会话内保留。
- 切换只改变 SceneBuilder 如何读取现有 effective assignment，不改变 composition/hash、brush target、AI 建议、
  人工覆盖、undo/redo、style plan 或导出候选。
- `Part` 显示 effective `PartRole`；`Material` 保留现有 `MaterialRole` 显示。
- 选中区域/体素可用亮度或 emissive 强调，但不得把类别色改成另一类别色。

### 6.2 冻结部件颜色

| 部件 | 标注色 | RGB |
|---|---|---|
| 车体 `BodyShell` | `#4477AA` | 68, 119, 170 |
| 炮塔 `Turret` | `#AA3377` | 170, 51, 119 |
| 炮管 `Barrel` | `#EE7733` | 238, 119, 51 |
| 车轮 `Wheel` | `#228833` | 34, 136, 51 |
| 履带 `Track` | `#CCBB44` | 204, 187, 68 |
| 天线 `Antenna` | `#33AADD` | 51, 170, 221 |
| 附加部件 `Attachment` | `#EE6677` | 238, 102, 119 |
| 未分类 `Unknown` | `#8A8F98` | 138, 143, 152 |

这些颜色只表示 effective part 分类，不表示来源、置信度、材质、阵营色或风险。

### 6.3 冻结材质颜色

保持现有显示映射，不改变已验收语义：

| 材质 | 标注色 |
|---|---|
| 涂装面 `PaintedSurface` | `#5B9E52` |
| 玻璃 `Glass` | `#2DA8D2` |
| 橡胶 `Rubber` | `#2E3137` |
| 裸金属 `BareMetal` | `#AAB2B8` |
| 灯光 `Light` | `#F6D44B` |
| 暗部 `DarkOpening` | `#241C2B` |
| 强调 `Accent` | `#E0683E` |
| 未分类 `Unknown` | `#945FD2` |

阵营色不是材质类别，不增加独立标注色；它继续由人工批准位和最终 style/palette 链决定。

## 7. 精确 UI 契约

只修改现有“语义”详情页顶部区域：

1. 保留现有两行工具布局、列表、下拉框和状态文本。
2. 在第一行 `浏览 / 画笔 / 擦除` 之后增加一个紧凑分段切换：`部件 / 材质`。
3. 在第二行画笔目标下方、现有状态文本上方增加一个可自然换行的紧凑图例；图例只显示当前审阅维度的
   `色块 + 中文名称`，不增加面板、DataGrid、横向滚动或固定高度。
4. 笔划期间状态文本显示“正在绘制：N 个表面采样点”；释放后显示实际影响体素数；取消时显示取消原因。
5. 画笔大小、镜像、擦除、部件/材质目标和阵营色控件继续复用现有绑定。

新增并冻结 AutomationIds：

```text
VoxelStyle.Semantics.ReviewDimension
VoxelStyle.Semantics.ReviewPart
VoxelStyle.Semantics.ReviewMaterial
VoxelStyle.Semantics.ReviewLegend
```

保留全部 4A、4B、FIX2 AutomationIds，尤其是 `VoxelStyle.Preview.Viewport3D`。不新增窗口、菜单、Shell
入口、全局快捷键或布局持久化字段。

## 8. 允许文件

```text
RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/Ra2VoxelSemanticMaskEditing.cs
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelViewportSceneBuilder.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs
RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs
RA2IniEditor.Application.Tests/Ra2VoxelSemanticMaskingTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelViewportSceneBuilderTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelViewportCameraStateTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceViewModelTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceUiContractTests.cs
上述目录内一个边界明确的新 stroke pointer 测试文件（仅在现有文件无法清晰承载时）
Docs/ASSET-VOX-4B-STROKE-1_*.md
Docs/Codex_CurrentPhase.md
Docs/DecisionLog.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
必要的既有产品说明文档（仅在行为实现后更新）
```

不得修改项目文件或增加依赖。若实现需要超出该清单，必须停止并说明原因。

## 9. 冻结边界

不得修改：

- ShellWindow、主布局、菜单、工具栏、Project Explorer、底部工具区和 docking。
- DeepSeek/Tencent 工具、prompt、轮次、Provider Host、真实模型调用或网络。
- working geometry、Agent 几何、质量优化、AI 语义建议和仲裁算法。
- canonical snapshot、VOX/VXL/HVA reader/writer、palette quantization、style compiler 和 colourizer 规则。
- 项目 Apply/Save、导出事务、持久化格式、public API、INI parser、Field Registry、Diagnostics、Completion。
- Legacy 工程或旧编辑器。

本阶段不直接写 VOX。只有既有“着色 → 固化最终候选 → 显式导出 VOX”链能物化结果。

## 10. 分阶段实施计划

| 阶段 | 实施内容 | 必选阶段门 |
|---|---|---|
| STROKE-0 | 固化事实审计、最终契约、R3 审批、基线测试列表 | 用户批准前停止；运行时代码 0 change |
| STROKE-1 | Application 多 seed 权威执行；单 seed 入口委托；原子/镜像/擦除/资源/stale 测试 | Application focused 全绿；无第二编辑器 |
| STROKE-2 | Viewport 笔划状态机、4-DIP 精确采样、去重、捕获/取消和临时 overlay | 指针状态机与 scene-generation 定向测试全绿 |
| STROKE-3 | View/ViewModel 开始、提交、取消；一次 undo、一次正式刷新、NoChange/失败不污染状态 | ViewModel/viewport integration 全绿；刷新计数可证 |
| STROKE-4 | 部件/材质维度、冻结颜色、紧凑切换/图例和 AutomationIds | SceneBuilder/UI contract/颜色映射测试全绿 |
| STROKE-5 | 顺序 build/test、差异/边界审计、文档、clean package 和人工烟测清单 | 全部必选门通过才可声称 automated complete |

每个阶段完成后自审：若发现输入竞争、第二套坐标/蒙版、部分提交、跨场景复用、额外正式刷新、颜色写入
palette 或范围漂移，立即停止，不进入下一阶段。

## 11. 自动验证矩阵

### 11.1 Application

- 单 seed 与原 `ApplySurfaceBrush` 结果逐体素、layer hash 和 affected count 相同。
- 多 seed 路径去重；顺序变化不改变最终 layer hash。
- footprint 重叠、镜像重叠、自镜像中轴、Erase、NoChange、非法 assignment、stale hash 和资源上限。
- 一条笔划结果相对 base layer 计算一次，不能把前一个 seed 的中间 layer 当作历史项。

### 11.2 Viewport / ViewModel

- 左键单击 = 一 seed 笔划；按下不改 layer，释放才提交。
- 慢速/高速拖动按 <=4 DIP 采样，路径连续；重复体素去重。
- 轨迹穿过空白不会桥接或命中背面。
- LostCapture、ClearScene、SetScene、Dispose、模式切换、右/中键相机请求均取消且零修改。
- 成功释放只压入一个 undo、清空 redo 一次、正式刷新一次；Undo 一次恢复整条笔划。
- NoChange/失败/cancel 不改 layer、history、composition、style preview。
- 右键任意位置 Orbit、Shift+右键/中键 Pan、滚轮 Zoom 回归全绿。

### 11.3 显示与 UI

- 八个部件和八个材质映射到契约固定 RGB；Unknown 独立可见。
- 切换维度只改变显示，composition hash/manual layer/undo 不变。
- 四个新 AutomationId 唯一存在；所有既有 AutomationId 和 FIX2 帮助文案保留并更新为包含“左键拖动绘制”。
- XAML 在 1920×1080、100%/125% DPI 下自然换行，无页面级缩放、横向灾难或新增复杂面板。

### 11.4 命令

实现完成后顺序执行，避免 WPF `obj` 竞争：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-restore --filter FullyQualifiedName~Ra2VoxelSemanticMaskingTests
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~Ra2VoxelViewport|FullyQualifiedName~Ra2VoxelStyleWorkspace"
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

真实 DeepSeek/Tencent 调用不是本阶段验证项。

## 12. 物理 WPF 验收

1. 完全关闭旧进程，启动新 Debug 构建并进入语义画笔。
2. 左键单击模型：释放后一次生效；一次撤销完全恢复。
3. 慢速和快速连续拖动长线：拖动中有路径高亮，释放后无明显断线；一次撤销恢复整条线。
4. 从模型拖入空白再拖回模型：空白段不修改隐藏/背面体素，返回表面后继续。
5. 开启大小 3 和镜像：释放后 footprint/mirror 与现有规则一致，仍只有一个撤销项。
6. 擦除连续拖动：仅移除人工 cell override，下层 Agent/区域语义重新显现。
7. 拖动中按右键或切换场景：笔划取消、无半成品，右键随后可旋转。
8. 切换“部件 / 材质”：类别色、图例和模型一致；切换不改变着色候选或撤销历史。
9. 在 100% 和 125% DPI 下检查工具条/图例可读性与相机连续性。

## 13. 自审结果

### 已通过

- **权威闭合**：屏幕轨迹只生成 seed，最终 cell 变更仍由唯一 Application editor 计算。
- **原子性闭合**：base layer 在 Begin 冻结，Commit 一次，Cancel 零修改；不会产生逐 MouseMove 历史。
- **命中可靠性闭合**：只复用 FIX2 精确 face hit map，不引入最近点、射线穿透或 Host 部件判断。
- **刷新闭合**：拖动只有独立临时 overlay；正式 composition/scene 只在成功释放后各发布一次。
- **显示闭合**：部件/材质颜色只读 existing effective assignment，不影响语义来源和最终 palette。
- **生命周期闭合**：scene/hash/capture/mode/camera 切换均定义为取消，不允许 stale/partial commit。
- **资源闭合**：采样和 seed 均有明确上限；超限整体取消而不是截断。
- **兼容闭合**：单击仍是一 seed 笔划；右键相机与现有大小/镜像/擦除/undo 语义保留。

### 明确不声称

- 不支持隐藏体素、内部切片、套索/填充、压力感应或跨模型笔划。
- 临时 overlay 只表示命中 seed 路径，不是完整半径/镜像 footprint；正式结果在释放后出现。
- 不证明最终艺术质量，不自动判断部件或材质，也不让标注色直接成为游戏色。
- 自动化无法替代真实 WPF 3D 指针、DPI 和视觉验收。

### 审查结论

契约充分覆盖了此前最可能返工的四处：指针所有权、事务/撤销粒度、异步场景刷新和标注色/游戏色混淆。
未发现需要扩大到 Shell、writer、public API 或持久化的依赖。用户已批准，STROKE-0 → STROKE-5 已按本契约
实现并通过自动验证；真实 WPF 指针、DPI 和视觉效果仍由用户手动验收。

## 14. 已执行的审批口令

```text
批准 ASSET-VOX-4B-STROKE-1 最终契约，连续执行 STROKE-0 → STROKE-5；不进行真实 DeepSeek/Tencent 调用，不修改 Shell、Apply/Save、VOX/VXL/HVA 写出、public API 或持久化。
```
