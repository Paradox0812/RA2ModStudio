# ASSET-VOX-1 Agent VOX 素材流水线系统侦察与架构提案

状态：Research completed / architecture proposed / implementation not approved  
日期：2026-08-26  
范围：自然语言或参考图生成 RA2 / YR / Ares / Phobos 车辆类 VOX 素材候选，输出可审阅的
VOX、无损切片和 VXLSE III 导入包。本文不宣称已经实现生成、VXL/HVA 编码或项目写入。

## 1. 结论

这条路线可行，但必须把“AI 生成几何”和“游戏格式确定性落地”拆成两个边界：

```text
DeepSeek 规划/Skill 选择
  -> 参考图或多视图候选
  -> 专用 image-to-3D provider
  -> 规范化 Mesh
  -> 确定性体素化与 RA2 palette 映射
  -> Canonical Voxel Scene
  -> MagicaVoxel VOX + 无损 SliceStack
  -> VXLSE III import package
  -> 人工 normals/pivot/HVA 校正与最终 VXL/HVA
```

DeepSeek 不应逐体素输出庞大的坐标数组。它负责理解需求、选择 RA2 素材 Skill、形成结构化设计规格、
检查候选和解释风险；三维生成由专用 3D 模型完成，体素、调色板、切片、哈希和导入包由本地确定性代码完成。

首版继续以 VXLSE III 为最终权威收口工具是可靠的。直接 VXL writer 不应成为 ASSET-VOX-1 的前置；
Vengi 的 C&C VXL/HVA 实现和其它独立解析器可作为交叉验证器，为以后直接编译提供证据，但不能在尚未
通过真实游戏样本矩阵前取代 VXLSE III。

## 2. 当前项目代码事实

### 2.1 已具备

- `Ra2AutomationAssetManifest` 已能表达最终 `VxlModel` / `HvaAnimation` 需求和 INI 绑定。
- `IRa2AutomationAssetProvider` 与 `Ra2AutomationExistingAssetProvider` 能校验已有最终二进制素材的
  requirement identity、扩展名、大小和 SHA-256。
- Work 已具备项目快照、结构化 Project Proposal、完整 Result/Changes/Object Context 审阅、显式 Apply、
  compound Undo 和不自动保存边界。
- 产品愿景已经冻结 `VOX -> 1 pixel = 1 voxel SliceStack -> VXLSE III -> VXL/HVA` 路线。

### 2.2 尚不具备

- 文本/参考图到 3D 的 provider、模型运行 Host、任务进度、取消后的产物登记。
- `.vox` codec、Canonical Voxel Scene、mesh voxelizer、RA2 palette/remap 量化器。
- part、axis、order、origin、pivot、depth 和逐切片哈希契约。
- 旋转 3D 预览、part 隔离、切片 scrub、palette/remap 预览。
- VXLSE III 导入包生成与真实导入矩阵。
- VXL normals、HVA transform 和游戏内渲染验证。
- 素材工作区到项目目录的受控 commit。

### 2.3 现有接口不能被错误复用

当前 `IRa2AutomationAssetProvider.Resolve` 的成功结果必须与最终 Manifest 一一闭合，而且 `VxlModel`
要求 `.vxl`、`HvaAnimation` 要求 `.hva`。VOX、GLB、切片 PNG 和导入说明都是中间产物，不能伪装为
现有 provider 的成功 `VxlModel`。

因此首版应新增独立的“素材制作候选工作流”，最终由用户从 VXLSE III 返回真实 `.vxl/.hva` 后，再复用
Existing Asset Provider 进入既有 Manifest/INI 绑定。只有直接 VXL compiler 通过认证后，才可提供真正
闭合 `VxlModel/HvaAnimation` 的生成 provider。

## 3. 网上现有成果与采用裁决

| 成果 | 已证实能力 | 本项目裁决 |
|---|---|---|
| MagicaVoxel `.vox` 官方格式 | RIFF-like chunk；`SIZE`、`XYZI`、可选 `RGBA`；坐标和颜色索引可确定性读取 | 自行实现小型受限 codec 或复用经审计 MIT 实现；`.vox` 是交换格式，不是最终游戏格式 |
| TEXT2VOX | 已公开验证“文本 -> Flux 图像 -> Hunyuan3D-2 GLB -> GLB2VOX -> 多分辨率 VOX”的组合路线 | 证明总体路线可行；不直接 vendoring 其中的 exe/脚本，重新建立可审计 provider 边界 |
| TRELLIS.2 | MIT；高质量 image-to-3D；输出 mesh/PBR；官方实现使用 4B 模型与 O-Voxel | 首选本地/远端 3D provider 候选；与 IDE 进程隔离，不把 CUDA/Python 依赖带进 WPF |
| OpenAI Shap-E | MIT；text/image conditioned 3D；可输出 mesh/隐式表示 | 轻量兼容/研究 fallback；质量不作为首选生产基线 |
| Hunyuan3D-2 | image/text-to-image-to-3D 能力成熟 | 可选 provider；其 Community License 有地域与分发条件，默认不得随 IDE 捆绑 |
| Vengi | MIT；CLI 可自动转换 voxel/image/mesh，多调色板和 C&C VXL/HVA 源码支持 | 外部可选转换器与独立 oracle；首版不将其输出直接视为游戏可用事实 |
| VXLSE III | RA2/TS 专用 VXL 编辑、3D preview、palette、auto-normalizer、multi-section；官方 beta 支持 PNG MagicaVoxel slices | 首版最终人工收口权威；通过版本探针和真实导入样本冻结 importer 参数 |
| Iron Curtain `cnc-formats` | clean-room VXL/HVA parser，MIT/Apache-2.0，当前为 alpha | 可选独立读取校验器；不能单独证明游戏运行正确 |
| Scaffold Diffusion / Voxify3D 等研究 | 直接稀疏 voxel 或 palette-constrained voxel 生成正在快速发展 | 暂不作为首版依赖；保留 provider 插槽，避免绑定单一模型路线 |

关键事实：MagicaVoxel 的颜色体素没有 Westwood VXL 所需的逐体素 normals。社区与 VXLSE III 的既有流程
也明确把 MagicaVoxel 用作形状/颜色制作工具，再由 VXLSE III 完成 normals 等后处理。因此“成功生成 VOX”
不能标记为“VXL 游戏就绪”。

## 4. 推荐系统架构

```text
┌──────────────────────── IDE / Agent Work Mode ────────────────────────┐
│ Asset request -> Design Spec review -> Candidate gallery -> Commit gate│
└──────────────────────────────┬─────────────────────────────────────────┘
                               │ UI-neutral commands/results
┌──────────────────── Application Asset Authoring Core ─────────────────┐
│ Intent/Skill plan | job state | provenance | candidate score          │
│ VoxelScene contracts | SliceStack contracts | validation reports      │
└──────────────┬──────────────────┬───────────────────┬──────────────────┘
               │                  │                   │
┌──────────────▼──────┐ ┌────────▼────────┐ ┌────────▼──────────────────┐
│ Generation Provider │ │ Deterministic    │ │ External Tool Host        │
│ TRELLIS/Hunyuan/... │ │ VOX/voxel/slice │ │ VXLSE/Vengi/validators    │
│ isolated process/API│ │ palette core     │ │ allowlisted & sandboxed   │
└──────────────┬──────┘ └────────┬────────┘ └────────┬──────────────────┘
               └──────────────────┴───────────────────┘
                                  │
                    Content-addressed Asset Workspace
                                  │ explicit review/commit
                       Existing Asset Provider + Manifest
                                  │
                          Project files and INI binding
```

### 4.1 权威边界

1. 用户意图与参考图是输入事实。
2. `AssetDesignSpec` 是本次任务的结构化设计约束，不是模型产物质量证明。
3. `VoxelSceneSnapshot` 是体素阶段唯一权威；VOX、切片和 preview 都从它派生。
4. `SliceStackManifest` 是 VXLSE 导入包权威，逐文件哈希闭合全部切片。
5. VXLSE 导入结果在重新读取并校验前只是外部产物，不自动进入项目。
6. 最终 `.vxl/.hva` 经 identity、结构、section/pivot/normals 和游戏 smoke 后，才进入 Existing Asset Provider。

### 4.2 依赖边界

- WPF/IDE 不引用 Python、CUDA 或模型 SDK。
- 模型通过版本化进程/API adapter；每个 provider 有 capabilities、license、model ID、revision、limits。
- VOX、palette、slice 算法保持本地、确定性、可 headless 测试。
- VXLSE III 通过受限外部工具 Host 启动；不做 GUI 自动点击作为生产协议。
- 首版不把 MagicaVoxel 本体重新分发到安装包；官方许可禁止在其它 package 中销售或分发原程序。

## 5. 数据架构草案

以下为待正式契约冻结的候选类型，不代表本轮新增 public API。

### 5.1 `Ra2VoxelAssetDesignSpec`

- `JobId`, `ProjectSessionId`, `RequestedUnitId`
- `Prompt`, `NegativeConstraints`, `ReferenceArtifactIds`
- `TargetGameProfile`: `Ra2`, `YuriRevenge`, `Ares`, `Phobos`
- `PartSpecs[]`: `Body`, `Turret`, `Barrel`, `Other`
- `TargetBounds`: 逻辑宽/深/高与允许容差
- `Silhouette`, `FactionStyle`, `SymmetryPolicy`, `DamageStatePolicy`
- `PaletteProfileId`, `TransparencyPolicy`, `RemapPolicy`
- `GenerationProviderPreference`, `SeedPolicy`, `CandidateCount`
- `RequestedCompletion`: `DraftVoxel`, `VxlseImportReady`, `GameReady`

`GameReady` 在首版必须降为需要外部 VXLSE 收口，不能由模型自行宣称成功。

### 5.2 `Ra2VoxelSceneSnapshot`

- `SchemaVersion`, `SceneId`, `SourceArtifactHashes`
- `CoordinateSystem`, `AxisTransform`, `VoxelUnitScale`
- `Palette[256]` 与 `PaletteProfileHash`
- `TransparentIndices`, `RemapIndices`
- `Parts[]`
- `CanonicalHash`

每个 `Part` 包含：

- `PartId`, `Role`, `VxlSectionName`, `StableFileStem`
- `Bounds`, `Origin`, `Pivot`, `LocalTransform`
- 稀疏 `VoxelCell[]`: `X`, `Y`, `Z`, `PaletteIndex`
- `OccupancyCount`, `ConnectedComponentFacts`, `SymmetryFacts`

序列化必须固定字段版本、坐标整数范围、part 排序和 cell 排序；同一 Scene 必须生成同一 hash。

### 5.3 `Ra2VoxelSliceStackManifest`

- `SchemaVersion`, `SourceSceneHash`, `PartId`
- `SliceAxis`, `SliceOrder`, `AxisTransform`
- `Width`, `Height`, `Depth`
- `PixelOrigin`, `VoxelOrigin`, `Pivot`
- `ImageEncoding`: 首版只允许无损 PNG；精确 pixel format 由导入探针冻结
- `PaletteProfileId`, `PaletteHash`, `TransparentIndices`, `RemapIndices`
- `Slices[]`: `SliceIndex`, `VoxelCoordinate`, `FileName`, `Sha256`
- `NoScale=true`, `NoInterpolation=true`, `NoAntialias=true`

严禁 JPEG、缩放、抗锯齿、颜色管理隐式转换和未记录的轴翻转。

### 5.4 `Ra2VxlseImportPackage`

- 原始 prompt、Design Spec、provider/model/seed/provenance
- canonical `.vox`
- 每个 part 的切片 PNG 与 Manifest
- palette 文件/预览、remap mask、透明索引说明
- part/pivot/axis 预览图和导入参数说明
- `ValidationReport.json`
- `README-IMPORT.md`
- 全包 `PackageManifest.json` 与 SHA-256

### 5.5 状态机

```text
Planned
 -> ReferenceReady
 -> GeometryCandidateReady
 -> VoxelCandidateReady
 -> PaletteReviewed
 -> SliceStackVerified
 -> VxlseImportReady
 -> ImportedUnverified
 -> VxlHvaValidated
 -> ProjectCommitReady
 -> Committed
```

任一阶段可进入 `Canceled`、`ProviderFailed`、`InvalidArtifact`、`NeedsUserReview`。不得把中间状态提升为
最终成功，也不得用警告替代 normals、pivot、section identity 或 palette 的必选校验。

## 6. Agent 与 Skill 设计

第一轮 DeepSeek intent 调用只接收 Skill 摘要，并选择需要的 Skill；第二轮才注入正文，沿用当前
AGENT-SKILL-ROUTING-2 的模式。建议新增：

1. `ra2-voxel-vehicle-design`：RA2 俯视等距视角、轮廓与可读性、比例和细节密度。
2. `ra2-voxel-parts-and-pivots`：body/turret/barrel 切分、section 命名、pivot 与 HVA 关系。
3. `ra2-voxel-palette-remap`：单位 palette、透明色、remap 区间、颜色量化禁区。
4. `ra2-voxel-generation-provider`：参考图、多视图、seed、候选数和 provider 能力选择。
5. `ra2-voxel-slicestack-vxlse`：axis/order/PNG、导入参数和 VXLSE 收口步骤。
6. `ra2-voxel-quality-gates`：连通性、薄壁、孤立点、尺寸、symmetry、palette、normals/pivot readiness。
7. `ra2-asset-provenance-license`：模型、权重、参考图、第三方工具与输出许可记录。

Skill 不能获得任意文件、命令或项目写权限。模型输出只能生成 `DesignSpec`、候选选择和 review 说明；
本地 Host 决定允许的 provider、路径、资源上限和 commit。

## 7. 生成与转换算法路线

### 7.1 参考图阶段

- 文本直接生成 3D 的效果不稳定时，优先生成透明背景、正交感、低透视的 3/4 参考图。
- 对车辆建议同时生成前、侧、顶或一致多视图，且固定部件边界与比例。
- 参考图先进入用户审阅；不满意时只重生成图像，不浪费 3D/体素计算。

### 7.2 3D 阶段

- 首选 TRELLIS.2 image-to-3D adapter；Hunyuan3D-2 为许可受控可选；Shap-E 为研究 fallback。
- 输出统一规范化为 GLB/GLTF 或受控 mesh snapshot。
- 清理非有限数、退化面、极端边界、悬空碎片；保留原始 artifact 和清理报告。
- 自动 part segmentation 只能产生候选。body/turret/barrel 边界和 pivot 必须可视化确认。

### 7.3 体素化阶段

- 由本地确定性 voxelizer 把规范 mesh 投射到离散网格。
- 同一输入/参数必须产生完全一致的 occupancy 和颜色。
- 同时生成 coarse/medium/fine 候选；分辨率由目标 RA2 尺寸 profile 决定，而不是把 32/64/96
  写死为所有单位的通用标准。
- 进行表面/实心策略、薄壁保留、孔洞处理、连通分量和最小特征检查。

### 7.4 Palette 阶段

- 只量化到明确项目 palette；不从 AI 图像动态生成最终 RA2 palette。
- 分离普通颜色与 remap mask。量化目标函数需支持 remap 保留、感知色差和相邻颜色稳定性。
- 记录每个源色到 palette index 的映射和误差统计。
- 透明索引、remap index 范围必须来自所选 palette profile；不能依赖全局常量猜测。

### 7.5 VOX 与 SliceStack 阶段

- `.vox` writer 只输出项目支持并测试的 chunk 子集；reader 对未知 chunk 跳过但保留诊断。
- 所有切片从 `VoxelSceneSnapshot` 生成，不从渲染截图反推。
- 每像素对应一个 cell；颜色必须精确对应 palette entry；空 cell 使用约定透明值。
- 导出后立即反读 PNG 并重建 occupancy，与源 Scene 做逐体素等价校验。

### 7.6 VXLSE 收口

- 首个版本必须用当前实际 VXLSE III beta 做版本探针。
- 用一个 3x4x5 非对称彩色测试体冻结 axis、slice order、pixel origin、颜色匹配和尺寸估算。
- 再用 body/turret/barrel 三 part 样本验证 section、pivot、palette、auto-normalizer 和静态 HVA。
- IDE 输出导入包和说明，不通过 GUI 自动化声称导入成功。

## 8. UI 产品流程

Work 模式建议显示一条可恢复的素材任务，而不是把二进制内容塞进聊天：

1. `需求`：自然语言、参考图、目标单位和项目 palette。
2. `设计规格`：part、尺寸、风格、remap、seed/candidate 数，可编辑并重新规划。
3. `参考图候选`：先筛图。
4. `3D/体素候选`：可旋转预览，coarse/medium/fine 对比。
5. `部件与 Pivot`：body/turret/barrel 单独显隐、轴向和 pivot gizmo。
6. `Palette`：普通色/remap/透明 mask 与误差热图。
7. `Slices`：逐层 scrub、占用数和源 Scene 对照。
8. `导出`：生成 VXLSE package；明确标记“尚未生成最终 VXL/HVA”。
9. `回收结果`：用户导入 VXLSE 产物，IDE 重新校验并形成 Asset Manifest/INI binding Preview。

所有写入项目的动作必须保持显式 Preview/Apply；覆盖、删除、保存和外部付费调用继续需要授权。

## 9. 校验规范

### 9.1 纯算法门禁

- VOX parse/write semantic round-trip。
- unknown/truncated/oversized chunk、重复坐标、越界 palette index 拒绝。
- sparse Scene canonical hash 稳定。
- SliceStack 正反变换逐体素相等。
- PNG 没有 resize、interpolation、alpha/palette 漂移。
- axis/order 的非对称 fixture 覆盖六种方向与正反序。
- palette/remap/transparent mask 完整性。
- cancellation、timeout、provider crash、partial artifact cleanup。

### 9.2 交叉验证

- 同一 VOX 由自有 codec 与独立工具读取，比较 bounds、occupancy、palette 和 part 数。
- 最终 VXL/HVA 至少由两个独立 parser 读取；解析一致不等于游戏可用，但可排除结构错误。
- VXLSE import 后与源 Scene 比较 occupancy、颜色和 section identity。

### 9.3 人工/游戏门禁

- 1920x1080 与窄宽 UI 预览。
- VXLSE III 实际导入、auto-normalize、pivot 和 HVA 保存。
- RA2/YR 实际游戏：8 方向、光照、remap、阴影、turret/barrel 转动、死亡/受击场景。
- 用户确认视觉比例、轮廓辨识和 faction style。

测试 fixture 不应分发原版游戏素材；使用程序生成非对称体和用户自有样本的哈希/测量结果。

## 10. 安全、资源与许可

- 模型/工具在独立进程或远端 adapter 运行；IDE 只允许固定 executable/provider ID 和参数 schema。
- 所有路径 containment、扩展名、magic、尺寸、像素/体素数、压缩比和聚合预算先校验。
- 不允许模型给出任意 shell 命令；不从产物执行脚本。
- 原始输入、中间产物和最终包使用内容寻址；取消/失败产物不会进入项目。
- provider 记录 model/weight revision、seed、prompt hash、依赖版本、license 和来源。
- Hunyuan3D-2 等非 MIT 权重必须由用户显式启用并接受适用许可；不得静默下载或捆绑。
- MagicaVoxel 程序本身不随 IDE 分发；VXLSE/Vengi 的集成形式需在实现前完成第三方通知审计。

## 11. 连续实施阶段

### ASSET-VOX-1A — Golden Probe 与契约冻结

- 生成非对称 fixture；实测当前 VXLSE III PNG importer。
- 冻结 axis/order/pixel format/palette/pivot/part 命名。
- 产出最终数据契约、failure taxonomy、limits 和 AutomationIds。

验收：fixture 经 SliceStack -> VXLSE -> VXL 后 occupancy/颜色/方向证据闭合。

### ASSET-VOX-1B — Canonical Voxel Core

- 实现 UI-neutral `VoxelSceneSnapshot`、受限 VOX reader/writer、canonical hash。
- 实现 palette profile、quantizer、SliceStack exporter/importer 和属性测试。

验收：纯算法和恶意输入矩阵通过；无 WPF/模型/文件任意写入。

### ASSET-VOX-1C — Generation Provider Host

- 实现 provider descriptor、capability/limits/license、隔离进程/API、progress/cancel。
- 首个 provider 建议 TRELLIS.2 image-to-3D；参考图 provider 独立。
- 输出规范 mesh 和 provenance，不直接输出可信 VXL。

验收：固定 seed candidate 可重放；超时/崩溃/取消不污染项目。

### ASSET-VOX-1D — Mesh/Voxel Candidate Pipeline

- mesh 清理、尺度/朝向、part 候选、multi-resolution voxelization、palette/remap。
- 生成可旋转预览和质量报告。

验收：至少 body-only、body+turret、body+turret+barrel 三类样本通过。

### ASSET-VOX-1E — VXLSE Import Package 与 UI

- 生成 package、切片 scrub、part/pivot/palette 审阅、导入说明。
- 回收用户导入的 `.vxl/.hva`，独立解析验证后接 Existing Asset Provider。

验收：端到端人工 smoke；UI 不把 `VxlseImportReady` 显示为 `GameReady`。

### ASSET-VOX-1F — Direct Compiler Feasibility（可选、后置）

- 用 Vengi/VXLSE 输出和自有 writer 做差分，验证 normals、bounds、span、section、HVA。
- 只有黄金矩阵与游戏 smoke 通过，才考虑直接闭合 `VxlModel/HvaAnimation` provider。

## 12. 可行性评估

| 目标 | 可行度 | 判断 |
|---|---|---|
| 自然语言/参考图生成可辨识 3D 候选 | 中高 | 已有多个开源 image-to-3D 模型；质量与硬件/模型强相关 |
| 3D 候选确定性体素化为 VOX | 高 | 算法成熟，TEXT2VOX/Vengi 等已有先例 |
| VOX 无损转换为受控 SliceStack | 很高 | `.vox` 格式简单，逐体素/逐像素等价可自动证明 |
| SliceStack 进入 VXLSE III | 中高 | 官方 beta 已支持 PNG 导入；具体轴/顺序仍必须由当前版本探针冻结 |
| 自动得到视觉合格的 RA2 VXL | 中 | normals、palette、比例、pivot 和部件切分仍需要专用规则与人工审阅 |
| 自动得到完整 HVA 动画 | 中低 | 静态 identity HVA 较简单；复杂炮塔、履带/旋翼和多帧动作需独立动画阶段 |
| 完全无人干预生成游戏可用素材 | 当前不可靠 | 可以作为长期目标，不应成为 ASSET-VOX-1 首版承诺 |

最现实的首个产品验收点是：用户用自然语言提出载具需求，Agent 生成 2~4 个候选，用户选择并调整
part/pivot/palette，IDE 生成可重复、可校验的 VOX + VXLSE import package；用户在 VXLSE 完成 normals
和最终保存后，IDE 回收并验证 VXL/HVA，再与 INI Manifest 一起预览和提交。

## 13. 主要剩余风险

1. 当前 VXLSE III build 的 PNG 排布、切片顺序、Alpha 与 palette 规则已由随包源码和确定性测试冻结；
   真实 executable 导入仍需在 1B PNG exporter 完成后作独立验收，且 world-axis/pivot 不能由 importer 推断。
2. 自动 part segmentation 和 pivot 推断的错误会比几何细节缺失更影响游戏使用。
3. 3D 模型的高频表面细节在低分辨率体素化时会产生噪点和薄壁断裂。
4. RA2 palette/remap 不是普通图像量化问题，需要项目 palette profile 和视觉审阅。
5. VXL normals 与 HVA 不是 VOX 自带信息，不能从“VOX 成功”推断。
6. 本地 4B image-to-3D 模型的 GPU/安装成本较高，应允许远端或轻量 provider，但付费调用必须授权。
7. 当前 Asset Provider 只适合最终二进制 passthrough；中间产物需要独立 job/artifact contract。
8. Vengi、VXLSE 和模型权重的分发/许可必须分别审计，不能因仓库源码可见就假定可捆绑。

## 14. 一手资料

- MagicaVoxel 官方格式：<https://github.com/ephtracy/voxel-model/blob/master/MagicaVoxel-file-format-vox.txt>
- MagicaVoxel 官方站点/许可说明：<https://github.com/ephtracy/ephtracy.github.io/blob/master/mv_main.html>
- TEXT2VOX 实现：<https://github.com/gfodor/text2vox>
- Microsoft TRELLIS.2：<https://github.com/microsoft/TRELLIS.2>
- OpenAI Shap-E：<https://github.com/openai/shap-e>
- Tencent Hunyuan3D-2：<https://github.com/Tencent-Hunyuan/Hunyuan3D-2>
- Vengi 与 voxconvert：<https://github.com/vengi-voxel/vengi>、<https://vengi-voxel.github.io/vengi/voxconvert/Usage/>
- Vengi C&C VXL/HVA 源码目录：<https://github.com/vengi-voxel/vengi/tree/master/src/modules/voxelformat/private/commandconquer>
- VXLSE III 官方说明：<https://www.ppmsite.com/vxlseinfo/>
- VXLSE III MagicaVoxel PNG import：<https://ppmforums.com/topic-47997/voxel-section-editor-iii-officially-imports-magickavoxel-art/>
- VXLSE III 源码镜像：<https://github.com/hathlife/voxel_section_editor>
- Westwood voxel normals 研究：<https://www.ppmsite.com/sibgrapi2007_files/finding_surface_normals_from_voxels.pdf>
- Iron Curtain clean-room C&C parsers：<https://github.com/iron-curtain-engine/cnc-formats>
- Scaffold Diffusion：<https://github.com/jsjung00/scaffold-diffusion>
- Voxify3D：<https://github.com/yichuanH/Voxify3D_official>

## 15. 2026-08-26 分离式装配补充决策

`VoxelSceneSnapshot` 只描述一个体素部件，不能单独承担车辆完整资产身份。完整交付单位升级为
`Ra2VoxelAssetAssembly`，其部件图至少支持 `Body -> Turret -> Barrel`，每个节点可以拥有独立 VXL/HVA、
局部坐标、pivot、安装点和动画轴。单文件多 Section 只是节点内部表示之一，不能替代多文件装配层。

Stage 1A 已建立 internal 装配拓扑、VXL/HVA 元数据探针和同版本 VXLSE Slice Import 契约。源码已冻结
Downward/Rightward 的 raster addressing、direct-alpha occupancy 和 palette 量化行为；pivot/mount、normals、HVA
和游戏表现仍由 `ASSET-VOX-1A_GoldenProbeAndSeparatedAssemblyFinalContract.md` 的后续验收门禁控制。
