# ASSET-VOX-4E Rev.7 — Form Zone / Boundary Intent / Game-Scale Code Fact Audit

日期：2026-08-31  
状态：Completed / read-only code fact audit  
用途：为 Rev.7 精确契约提供真实实现边界；本文件不是实现批准。

## 1. 审计结论

Rev.6 的安全边界和单一权威路径可以继续复用，但当前质量上限由三个事实共同决定：

1. `Ra2VoxelColourizer` 只派生 Top / Side / Under / LongitudinalEnd / Edge 等粗粒度几何位；
2. `Ra2VoxelColourTechniquePolicy` 主要表达亮度偏移、edge 强度和材质分离强度，五技法缺少空间策略；
3. `Ra2VoxelSemanticBoundaryProjector` 将有效语义变化压缩为单一 one-cell boundary，无法表达倒角、接缝、
   接触阴影、材质界面等不同意图。

因此 Rev.7 必须扩展现有 typed policy 和派生事实，不应新增第二套 colourizer、第二套 style compiler，或让
DeepSeek 产生坐标与最终 palette index。

## 2. 当前 canonical 路径

```text
HumanUnitClass + HumanBaseColour + HumanTechnique
  → Ra2VoxelStyleCompiler v2
  → Ra2VoxelSemanticStyleIntegrator
  → Ra2VoxelColourFamilySelector
  → Ra2VoxelSemanticColourMaterializer
  → Ra2VoxelColourizer
  → Ra2VoxelColourQualityEvaluator
  → Ra2VoxelStylePreviewCoordinator
  → Ra2VoxelStyleWorkspaceViewModel / existing workspace
```

Rev.7 必须沿此路径扩展。以下均为复用入口：

| 能力 | 当前权威实现 | Rev.7 复用方式 |
|---|---|---|
| 人工基准色 | `Ra2VoxelBaseColourSelection` | 保持 exact anchor，不增加自动基准色 |
| 技法目录 | `Ra2VoxelColourTechniqueCatalog` | 升级 revision 和 typed spatial policy |
| 单位适配 | `Ra2VoxelUnitAdaptationCatalog` | 增加 class-specific zone/detail policy，不建立第二目录 |
| 主体色族 | `Ra2VoxelColourFamilySelector` | 扩充 body/material tonal family，继续共享 deterministic selector |
| 表面事实 | `Ra2VoxelColourizer.BuildGeometryMask` | 抽取/扩展为 form-zone projector；旧 mask 作为兼容输入 |
| 语义边界 | `Ra2VoxelSemanticBoundaryProjector` | 升级为 boundary-intent projection |
| 材质化 | `Ra2VoxelSemanticColourMaterializer` | 保持 single materialization authority |
| 对比候选 | `Ra2VoxelPaletteContrast` | 消费同一 family/zone/boundary 事实，不建立独立上色算法 |
| 质量门 | `Ra2VoxelColourQualityEvaluator` | 增加多维质量事实，不增加 opaque score |
| 法线事实 | `Ra2VoxelNormalField` | 只读复用；不得把法线写入 colour candidate |
| 3D 场景 | `Ra2VoxelViewportSceneBuilder` | 增加 review colour modes；不改变几何或 hit testing 权威 |

## 3. 数据事实

### 3.1 Canonical snapshot 不拥有 normal index

`Ra2VoxelSceneSnapshot` schema v1 的 cell 只保存 `Coordinate + PaletteIndex`。法线存在于独立、非持久化的
`Ra2VoxelNormalField`，以 snapshot hash 绑定。colour pipeline 当前不消费该 field。

结论：Rev.7 不得修改 snapshot schema 或 4D sidecar。Normal-aware review 只能作为可选、派生、只读事实；缺失时
必须明确为 `NotAvailable`，不能用 WPF mesh normal 冒充 RA2 VXL normal。

### 3.2 当前没有 VPL authority

当前 Application/IDE voxel authoring 路径没有 VPL profile、VPL parser 或 stock/custom VPL identity。WPF
`Ra2VoxelViewportSceneBuilder` 使用 face normal、Diffuse 和固定 Specular，不能证明 RA2 游戏内实际亮度。

结论：Rev.7 只能增加 `VplCompatibility = Unknown/NotEvaluated` 和 palette-risk facts；不得宣称精确 VPL 模拟。
VPL 载入、解析和实际游戏光照预览需要独立后续合同。

### 3.3 当前 orientation 只能得到轴，不能得到前向符号

`BuildGeometryMask` 以 `YSize >= XSize` 选择 longitudinal axis，但不能确定 `+X/-X/+Y/-Y` 哪一侧是车头/机鼻/舰首。

结论：可靠的 front recognition 需要 session-only 人工 `ForwardDirection`。缺失时只允许 `LongitudinalEnd`，不得将
任一端自动命名为 Front 或 Rear。

### 3.4 当前 technique differentiation 仍是数值策略

revision 2 技法具有不同的 luminance offset、edge policy、material separation 和 accent policy；自动测试保证
candidate hash 不同，但不能保证区域选择、细节预算或视觉构图显著不同。

结论：revision 3 必须增加 band count、zone participation、boundary allowlist、detail budget 和 accent budget，
并用分布事实测试空间差异。

### 3.5 当前 semantic material 是单 role/index

MaterialRole 通过 exact semantic binding 映射到一个 style role。direct material 和 remap 后置保护已经正确，但
Glass/Metal/Rubber/Light 等材质不能在自身 mask 内形成受控 light/base/shadow family。

结论：Rev.7 的 material family 必须由 Host 从一个 exact anchor/binding 派生，保持 material mask 与 direct
protection；Provider 不能输出多个颜色索引。

## 4. 当前可见缺陷对应代码原因

| 视觉缺陷 | 直接代码事实 |
|---|---|
| 平滑侧面出现暗块 | cell 同时匹配粗粒度 geometry bits，缺少 flat-field / recess 区分 |
| 前侧无识别度 | 只有 longitudinal end，没有人工 front sign 和 front-zone 事实 |
| 一体素亮线意义不明 | effective semantic change 被压缩成单一 `EdgeOrRidge` |
| 五技法相似 | technique policy 没有 zone/detail/band spatial fields |
| 材质平板 | 每个 material binding 只有单 palette role |
| 大图可看、游戏尺寸不可读 | quality evaluator 没有 projected feature survival / fixed-view facts |
| 黄色等强调色过强 | accent policy 只有 Preserve/Emphasize，没有面积、对比和组件上限 |

## 5. 数据所有权

| 新概念 | Primary owner | Lifetime | Serialized | Consumer |
|---|---|---|---|---|
| ForwardDirection | IDE session input，经 Application validator 形成 immutable selection | 当前 model/evidence | No | projector/cache identity/UI |
| FormZoneProjection | Application derived fact | 当前 snapshot/orientation/policy | No | materializer/quality/review |
| BoundaryIntentProjection | Application derived fact | 当前 snapshot/semantics/form zones/policy | No | materializer/quality/review |
| FeatureScaleFacts | Application derived fact | 当前 snapshot + fixed projections | No | quality/technique filtering |
| AccentBudgetPolicy | Application immutable technique catalog | content revision | No | materializer/quality |
| MaterialFamilySelection | Application derived contract | 当前 palette/binding/technique | No | materializer/quality |
| GameScaleReviewFacts | Application derived presentation-neutral facts | candidate generation | No | review package/UI |
| VplCompatibility | Application quality fact | candidate generation | No | review only |

拒绝的 owner：Skill Markdown、ViewModel、本地 XAML state、4D sidecar、canonical snapshot、Provider output。

## 6. Public API / persistence / cache 影响

- 所有新增 .NET 类型保持 `internal`；public .NET API 预期为零。
- 不修改 `.semantic.json`、VOX codec、VXL/HVA writer、project save 或 snapshot schema。
- style/materialization/cache identity 必须升级 revision；旧 cache 安全 miss，不迁移、不删除。
- 新 orientation 是 session-only；model/palette/semantic identity 改变时失效。
- review overlay 是派生 presentation，不得反向改变 candidate。

## 7. 复用结论

搜索词：`ColourTechniquePolicy`、`ColourFamily`、`GeometryRegionBits`、`SemanticBoundary`、`NormalField`、
`QualityEvaluator`、`ViewportColourMode`、`StylePreviewCoordinator`。

决定：

- 扩展现有 policy/catalog、projector、materializer、quality 和 viewport mode；
- 不新增 provider、colourizer、palette parser、VPL parser、snapshot 或 persistence path；
- 仅当现有文件职责过大时，新增窄命名的 derived-fact projector 文件，并由现有 canonical path 调用。

## 8. 审计边界

本审计不证明算法阈值正确，也不证明 forum 经验在所有模型上成立。阈值必须在 Rev.7 contract fixtures 和用户真实
ground/air/large-surface 样本上校准。真实 VPL、游戏内渲染、VXL normal、HVA、pivot/mount 和 GameReady 不属于
Rev.7。

