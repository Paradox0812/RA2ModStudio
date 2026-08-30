# ASSET-VOX-4B — Agent-Seeded Human Semantic Editing Final Contract

状态：Approved / Implemented / automated verification complete  
日期：2026-08-29

## 1. 目标

在 4A DeepSeek 粗粒度部件/材质建议之上增加简洁、可撤销的人工体素蒙版覆盖。用户可旋转 3D 模型，在可见
表面使用画笔或擦除工具修正错误边界；最终有效蒙版进入既有 palette-safe 着色链。本阶段不修改任何体素坐标、
占用、源文件或项目状态。

## 2. 权威顺序

```text
current working geometry hash
  -> deterministic 4A evidence
  -> accepted DeepSeek suggestion
  -> region-level human assignment
  -> cell-level human mask override
  -> effective semantic mask composition
  -> existing style integrator / colourizer
```

优先级固定为：`CellHumanOverride > RegionHumanOverride > AgentSuggestion > Unknown`。AI 层和区域层保持不可变；
画笔只记录稀疏的 cell override。工作几何 hash 变化必须使旧编辑会话失效。

## 3. 编辑模型

- 身份：`SourceSnapshotHash + sorted cell index`；不使用屏幕坐标作为权威。
- 画笔：从用户点击的占用体素开始，在当前外露表面内按六邻域距离选择半径 0/1/2。
- 镜像：默认启用；仅在镜像坐标确实存在时加入同一原子操作。
- 擦除：移除选中体素的 cell override，恢复下层区域/AI/Unknown 结果，不删除 AI 建议。
- 撤销/重做：保存有界、原子 delta；每次画笔动作和镜像副本属于同一个历史项。
- 资源上限：最多保留 100 个历史项；单次操作不得超过当前 occupancy count。
- Unknown 是合法结果，不因局部未分类阻止预览或着色其他已分类区域。

## 4. UI 契约

只修改现有体素工作区内部的“语义”详情页：

- 顶部一行工具条：`浏览 / 画笔 / 擦除`、`大小 1/2/3`、`镜像`、`撤销`、`重做`。
- 第二行只显示当前工具、目标部件/材质、影响体素数和简短操作提示。
- 下方继续使用现有区域列表和部件/材质下拉框；列表不复制、不横向增加新列。
- 浏览模式保持现有点击选区和相机交互。
- 画笔/擦除模式只在“短点击”时执行；拖动仍然旋转/平移，滚轮仍然缩放。
- 未选择有效区域、目标仍为 Unknown 或预览不是语义模式时，不执行画笔并给出明确状态。
- 3D 预览在每次有效编辑后刷新，但保留同一 working source 的相机姿态。

新增并冻结 AutomationIds：

```text
VoxelStyle.Semantics.EditMode
VoxelStyle.Semantics.BrushSize
VoxelStyle.Semantics.MirrorBrush
VoxelStyle.Semantics.Undo
VoxelStyle.Semantics.Redo
VoxelStyle.Semantics.EditStatus
```

4A AutomationIds 全部保留。

## 5. 着色与导出边界

- 最终 composition 必须覆盖当前 snapshot 的 cell ordering，并携带 source hash 和 composition hash。
- 同一 cell 只能有一个有效 assignment；同角色体素可聚合为一个显式 mask。
- style plan 缺少相应颜色角色时继续标记 unresolved，不猜 palette index。
- 阵营色仍需人工批准；细粒度画笔继承当前区域的人工 remap 状态。
- 着色前后坐标、占用和 part descriptor 必须完全相同。
- 现有固化与 VOX 导出可以消费着色结果，但本阶段不修改 writer 或事务语义。

## 6. 阶段

| Stage | 目标 |
|---|---|
| 4B-0 | 代码事实审计、最终契约、自审门禁。 |
| 4B-1 | 哈希绑定的 cell override、composition、画笔 delta 和 undo/redo 核心。 |
| 4B-2 | 3D 坐标点击、浏览/画笔/擦除、镜像和简洁工具条。 |
| 4B-3 | 最终 composition 接入语义预览和既有 palette-safe 着色。 |
| 4B-4 | 核心、ViewModel、viewport、UI contract 测试和阶段自审。 |
| 4B-5 | Debug build、全量测试、IdeOnly clean package 与文档收口。 |

## 7. 冻结边界

不进行真实 DeepSeek/Tencent 调用；不修改 Shell、项目 Apply/Save、几何算法、VOX/VXL/HVA writer、INI、
Field Registry、public API、持久化格式或 legacy。

## 8. 验收

- AI 初稿可独立存在，人工画笔不会回写或覆盖 AI 结果。
- 体素人工覆盖优先级、镜像、擦除、撤销/重做和 stale hash 均有测试。
- 3D 短点击编辑与拖动相机不冲突。
- 最终 composition 进入现有 colourizer，geometry/occupancy unchanged。
- 工具条在 1920×1080、100%/125% DPI 下保持单行或自然换行，不引入页面级缩放与横向滚动。
- 定向测试、全量测试、Debug build 和 IdeOnly clean package 通过；物理 WPF 交互由用户重启后验收。

## 9. Physical-smoke correction

首次物理验收暴露了假激活状态：没有语义证据时仍可进入画笔，且画笔额外要求先选择区域行。修正后，三个
编辑入口会按需复用本地确定性区域准备；短点击以实际 3D 命中区域为准，画笔部件/材质继续独立。离开语义
预览会回到浏览模式。该修正没有扩大本契约的 Provider、几何、writer、持久化或 Shell 边界。
