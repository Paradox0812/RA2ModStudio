# ASSET-VOX-4A — Semantic Part & Material Masking Final Contract

状态：Approved / Implemented / automated verification complete  
日期：2026-08-29

## 1. 目标

在当前 canonical working geometry 上建立可审阅的部件/材质语义掩码链：确定性 Host 只生成文本化几何证据，
DeepSeek 只提出建议，用户可以逐区域覆盖并显式批准阵营色，最终有效材质掩码复用现有 style compiler、palette
quantization 和 `Ra2VoxelColourizer`。本阶段不修改任何体素坐标、占用、文件或项目状态。

## 2. 权威与优先级

```text
current working geometry hash
  -> deterministic bounded spatial evidence
  -> DeepSeek suggestion layer (optional, text only)
  -> human override layer
  -> effective assignment
  -> explicit masks + existing colourizer
```

有效值优先级固定为：`HumanOverride > AgentSuggestion > Unknown`。AI 的 `Candidate` remap 不是批准；只有用户
在区域行勾选后才能成为 `ExplicitlyApproved`。重新分析同一 working hash 保留人工覆盖；工作几何变化使全部旧
语义证据和覆盖失效。

## 3. 有界词表

- Part：`Unknown / BodyShell / Turret / Barrel / Wheel / Track / Antenna / Attachment`
- Material：`Unknown / PaintedSurface / Glass / Rubber / BareMetal / Light / DarkOpening / Accent`
- Remap：`None / Candidate / ExplicitlyApproved`

Unknown 是正常结果，不得因缺少完整分类阻止预览。

## 4. DeepSeek 契约

- 仅发送模型尺寸、工作快照哈希、证据哈希、区域 ID、边界、体素数、外露率、镜像覆盖与用户文字说明。
- 不发送图片、文件路径、VOX/VXL 字节、坐标列表或 palette colours。
- 第一轮主分析，第二轮独立审阅；归一化语义指纹不同时才执行第三轮仲裁。绝对上限 3 次，无隐藏重试。
- 必须调用 `suggest_ra2_voxel_semantics`；未知/重复区域、错误哈希、无效枚举、越界集合均拒绝。
- 模型不得修改几何、坐标、占用、色板、文件、Apply/Save 或 remap 批准状态。

## 5. 人工覆盖与 3D 审阅

- `语义` 预览按有效材质显示，不代表游戏法线或最终美术质量。
- `仅准备人工区域` 只运行本地证据生成，不创建 provider 请求；AI 未配置或失败也不阻断人工覆盖。
- 单击外露面选择对应 Host 区域并切到语义明细。
- 部件和材质可逐区域修改；默认镜像联动可把同一覆盖应用到配对区域。
- 人工覆盖可逐行撤销；丢弃 AI 建议不删除确定性区域或人工覆盖。
- 阵营色必须逐区域人工批准。

新增/冻结 AutomationIds：

```text
VoxelStyle.Semantics.Card
VoxelStyle.Semantics.Instructions
VoxelStyle.Semantics.Prepare
VoxelStyle.Semantics.Analyze
VoxelStyle.Semantics.Accept
VoxelStyle.Semantics.Discard
VoxelStyle.Semantics.Status
VoxelStyle.Semantics.PartRows
VoxelStyle.Semantics.MaterialRows
VoxelStyle.Semantics.RemapApproval
VoxelStyle.Preview.Semantics
VoxelStyle.Details.Semantics
```

## 6. 着色与物化边界

- 语义区域物化为 snapshot-hash-bound `Ra2VoxelExplicitMask`。
- 材质只映射到当前已编译 style plan 中存在的颜色角色；缺少角色时保留 unresolved review，不猜 palette index。
- 所有显式掩码规则使用 `ExplicitUserMask`，并由原有 colourizer 执行。
- 着色必须保持坐标和占用完全相同；现有 VOX 导出只能固化用户明确选择的着色结果。
- 本阶段不修改 3B VOX writer/round-trip transaction，也不新增 VXL/HVA 写出。

## 7. 冻结边界

未修改：Shell、Tencent/AssetHost、几何算法和操作词表、项目 Apply/Save、VOX/VXL/HVA writer、INI、Field
Registry、public API、持久化格式、legacy。

## 8. 验收

- 确定性证据稳定、区域成对、哈希绑定。
- AI 一致时 2 次，分歧时 3 次；提示明确 text-only/no geometry/no colour invention。
- 人工覆盖优先，AI 不能批准 remap，同一几何重新分析保留人工覆盖。
- 3D 点击可选择区域；镜像联动可用。
- 有效材质掩码进入现有 colourizer，且 geometry/occupancy unchanged。
- 定向、全量 build/test 和 IdeOnly clean package 通过；不执行真实 DeepSeek/Tencent 调用。
