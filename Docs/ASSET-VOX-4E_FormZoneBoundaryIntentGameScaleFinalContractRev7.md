# ASSET-VOX-4E Rev.7 — Form-Zone Tonal Banding, Boundary Intent and Game-Scale Quality Contract

日期：2026-08-31  
状态：Approved / Rev.7-B..G automated completed / VisualAcceptancePending  
风险：R4  
治理模式：Immediate / completed at Rev.7-G stop point  
前置审计：`Docs/ASSET-VOX-4E_FormZoneBoundaryIntentGameScaleCodeFactAuditRev7.md`

## 1. 目标

在保持人工单位类型、人工 Palette 基准色、Host 确定性权威和现有 semantic mask 的前提下，将 Rev.6 的粗粒度
方向面上色升级为：

1. 连续形体区域和主体色阶带；
2. 有类型、有方向、有所有权的边界意图；
3. 受控的材质小色阶族与强调色预算；
4. 真正改变空间策略的五种 technique revision 3；
5. 游戏尺寸、固定方向和可解释风险的多维质量门；
6. 与 typed Host policy 一致的 colouring Skill revision 3。

Rev.7 生成高质量、可诊断的上色候选，不生成 VXL/HVA，不生成或修改法线，不证明 GameReady。

## 2. 权威与不变量

- `HumanManualSelection` 仍是单位类型唯一权威；不恢复 DeepSeek 自动判型。
- 新增 session-only `ForwardDirectionSelection`，仅人工可确定前向；Provider、文件名和最长轴不能替代它。
- `BaseColourSelection` 仍是唯一主体 anchor，`BodyBase` 必须保留 exact palette index。
- technique 仍由人工选择；Provider request 不包含 base colour、technique 数值或 palette index。
- DeepSeek 只输出 bounded qualitative intent/binding，不输出坐标、mask membership、palette index 或写入动作。
- Application Host 是 zone/boundary/family/materialization/quality 的确定性权威。
- 只改变已占用 cell 的 palette index；几何、占用、pivot、normal、HVA 和 4D semantic authority 不变。
- `CellHumanOverride > RegionHumanOverride > AgentSuggestion > Unknown` 不变。
- direct semantic materials 和 explicitly approved remap 必须晚期保护，不允许 boundary/contrast 覆盖。
- 不增加 persisted layer、sidecar schema、自动保存、项目 Apply/Save 或 writer。

## 3. 允许文件上限

每个子阶段必须再声明精确 allowlist。总上限：

- `RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/Ra2VoxelColourTemplateContracts.cs`
- `Ra2VoxelColourFamily.cs`
- `Ra2VoxelColourizer.cs`
- `Ra2VoxelSemanticMaskEditing.cs`
- `Ra2VoxelSemanticMasking.cs`
- `Ra2VoxelSemanticColourMaterialization.cs`
- `Ra2VoxelColourQuality.cs`
- `Ra2VoxelColourReviewPackage.cs`
- `Ra2VoxelPaletteContrast.cs`
- `Ra2VoxelNormalField.cs` 仅复用，不修改 normal bake semantics；若无需改动则不得触碰
- 新增窄命名的 `Ra2VoxelFormZone*.cs`、`Ra2VoxelBoundaryIntent*.cs`、
  `Ra2VoxelFeatureScale*.cs`、`Ra2VoxelMaterialFamily*.cs`
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelStyleCompilerV2.cs`
- `Ra2VoxelStylePreviewCoordinator.cs`
- `Ra2VoxelViewportSceneBuilder.cs`
- `Ra2VoxelStylePlanCache.cs` 仅限 derived identity 安全失效
- `RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml`
- 四个 colouring Skills 和五个 `TECHNIQUE.md`
- 直接相关 Application/IDE tests
- Rev.7 contract/audit/ledger/status/decision/user docs

## 4. 禁止文件与行为

- `ShellWindow.xaml`、`ShellWindow.xaml.cs`、Shell 菜单/工具栏/Dock/status；
- 4D `.semantic.json` schema/store、自动发现、迁移、merge；
- canonical snapshot schema、VOX/VXL reader/writer、HVA、pivot/mount；
- Provider/AssetHost protocol、模型选择、真实付费调用；
- INI parser、Field Registry、Completion、Hover、Diagnostics、Save Preflight；
- 项目 Apply/Save、backup/rollback、Artifact Registry；
- 新依赖、项目格式、build configuration；
- 第二套 palette、style compiler、semantic composer、colourizer、quality evaluator；
- 从 forum 模型复制 palette layout、贴图、模型或无授权资产；
- 将 WPF face normal/specular 描述为 RA2 VPL 游戏预览。

## 5. Orientation 合同

### 5.1 Typed selection

新增 internal enum：

```text
Unknown
PositiveX
NegativeX
PositiveY
NegativeY
```

`ForwardDirectionSelection` 绑定：

```text
snapshot hash
semantic evidence/composition hash
selected direction
source = HumanManualSelection
selection hash
```

规则：

- direction 必须位于水平轴；
- 当前 unit class 为 Unknown 时允许 Unknown；Ground/Air/LargeSurface 要生成 Front/Rear zone 必须明确方向；
- model/evidence/composition 改变时 selection 失效；palette、base colour、technique 改变不使 direction 失效；
- 未选择 direction 时仍可生成 ordinary candidate，但 Front/Rear 相关质量为 NeedsReview；不得猜测前向。

## 6. Form-zone 数据模型

新增 internal enum：

```text
Interior
UpperPlane
UpperBevel
SideShoulder
SideField
LowerSkirt
FrontEnd
RearEnd
LongitudinalEndUnknown
Recess
ContactShadow
SilhouetteRidge
UnclassifiedSurface
```

新增 immutable `FormZoneProjection`：

```text
source snapshot hash
orientation selection hash or Unknown identity
unit adaptation policy hash
per-cell zone bitset
zone counts
projection hash
diagnostics
```

### 6.1 派生顺序

1. 复用 occupancy/exposed-face facts；
2. 依据明确 ForwardDirection 区分 Front/Rear；缺失则仅 LongitudinalEndUnknown；
3. 以 occupied bounds 内 normalized height 建立 upper/side/lower 候选；
4. 依据相邻外露面、局部台阶和连续 run 派生 bevel/shoulder/ridge；
5. 依据局部包围和有效 part 接触派生 recess/contact shadow；
6. 以连通性、最小 run 和 feature-scale 过滤孤立 zone；
7. 保留 UnclassifiedSurface，不静默伪造高置信语义。

### 6.2 不允许的启发式

- 仅以单个 cell 的 Z 或 exposed-face 直接决定深暗；
- 将 side+under 自动视为 Underside；
- 仅以 RegionId 建立结构；
- 将最长轴的一端自动命名为 Front；
- 在平滑 SideField 内产生不连续 BodyRecess；
- 让 technique 改变 zone 几何事实；technique 只能选择/压缩已派生 zone。

## 7. Tonal-band 与 colour family 合同

### 7.1 主体 roles revision 3

主体 family 至少提供：

```text
BodyHighlight
BodyUpper
BodyBase       (exact human anchor)
BodyLower
BodyShadow
BodyRecess
```

允许兼容映射旧 `BodyLight/Mid/Dark/Underside/EdgeOrRidge`，但 materializer revision 3 必须只消费新的 semantic
role mapping。旧入口可委托新路径，不得保留第二套选择算法。

### 7.2 Palette family rules

- chromatic anchor 优先使用 anchor 所在 indexed ramp；
- 不跨透明、remap 或经 profile 判定的不合法 entry；
- index 顺序不能替代实际 luminance 顺序；
- 默认不跨 16-entry ramp；只有明确连续性事实和 technique policy 允许时才可作为 review candidate；
- BodyBase 永远 exact；任何候选移动 BodyBase 均 Blocked；
- family 稀疏时按 technique fallback `WarnAndPreserveIntent` 或 `Block`，不得随机寻找远色；
- family selection/hash 必须确定性稳定。

### 7.3 Band continuity

- tonal band 必须按 zone 连续应用；
- 不允许在 SideField 产生单 cell light/dark speckle；
- zone 内的小孔洞优先继承相邻主 band，除非属于 Recess/Opening/Material mask；
- colour count 是诊断，不是单一硬门：主体建议 3–6 个 index，整体建议 6–14 个 index；超限进入
  NeedsReview，不能仅因数字阻塞。

这些范围是 Rev.7 初始工程阈值，必须由 fixtures 和人工样本校准，不宣称为论坛或引擎硬规则。

## 8. Material family 合同

以下 direct material 可从一个 exact semantic binding anchor 派生最多三个 family member：

```text
Glass:  highlight / base / recess
Metal:  edge / base / shadow
Rubber: base / detail
Light:  highlight / base
Accent: base / optional highlight
```

规则：

- material mask membership 仍由 effective semantic assignment 决定；
- Provider 只提供 material role/binding，不提供 family index；
- material family 只在自身 validated mask 内应用；
- material anchor/exact role、remap 和 human overrides 受保护；
- mask 太小或 palette 无合法 family 时保留 exact anchor 并发出诊断；
- Rubber/Track 不得默认全黑；Light 不得默认使用 stock-VPL perma-bright；
- v1 未识别的材质保持 PaintedSurface/Unknown 路径，不根据原色猜材质。

## 9. Boundary-intent 合同

新增 internal enum：

```text
None
RaisedBevel
StructuralSeam
DeepOpening
ContactShadow
MaterialInterface
PanelLine
Silhouette
DecorativeMark
```

`BoundaryIntentProjection` 绑定 snapshot、effective semantic assignments、form-zone projection、technique policy，
包含 per-cell intent/owner、counts、hash 和 diagnostics。

### 9.1 行为矩阵

| Intent | 默认颜色行为 | 允许所有者 | 禁止行为 |
|---|---|---|---|
| RaisedBevel | 受光/上侧选择性提亮 | PaintedSurface | 完整包围区域 |
| StructuralSeam | 短、连续暗线 | PaintedSurface/Metal | 双侧同时扩张 |
| DeepOpening | 受限 BodyRecess/dark material | Opening/Recess | 扩展为大黑块 |
| ContactShadow | 接触侧暗带 | lower owner | 覆盖轮胎/履带/玻璃 |
| MaterialInterface | 优先依靠两材质自身差异 | 两侧各自 material | 默认额外亮边 |
| PanelLine | 大平面上的有限暗线 | PaintedSurface | 在小面/低尺度特征使用 |
| Silhouette | 默认无额外 palette 写入 | None | 整圈描边 |
| DecorativeMark | accent budget 内的标记 | Accent/Light | 无语义孤立点 |

### 9.2 边界保护

- RegionId-only 变化继续忽略；
- direct material/remap/human exact cells 不可被 boundary 覆盖；
- one-cell 是最大厚度，不是必须覆盖全部 eligible interface；
- 同一 connected boundary 必须有最小 run；孤立一格默认丢弃；
- technique 只决定 allowlist、coverage 和 value policy，不改变 boundary 的 semantic type；
- 每个 boundary cell 必须记录唯一 owner，禁止两侧重复写入。

## 10. Feature-scale 与 accent budget

### 10.1 Feature-scale facts

使用固定的四个等距斜视和四个正交水平视图，将 surface component 分类为：

```text
Macro
Meso
Micro
SubPixelRisk
```

第一版只使用确定性的 voxel projection bounds/coverage，不依赖 WPF DPI 或当前相机。阈值保存在 typed policy，
由 fixture 校准。

- Macro 不得被 technique 丢弃；
- Meso 可按 technique 保留或压缩；
- Micro 不得获得 Strong edge 或 PanelLine；
- SubPixelRisk 默认继承邻近主 band，除非是 explicit Light/Accent/remap；
- feature-scale 只影响颜色信息密度，不修改几何。

### 10.2 Accent budget

typed policy 至少包含：

```text
maximum visible-area share
maximum connected-component share
maximum local luminance jump
minimum connected run
symmetry expectation
emissive/perma-bright allowed = false in Rev.7
```

初始阈值必须在测试中可读、版本化并按 technique 区分；不得把未经样本校准的比例写成永久引擎规则。

## 11. Technique revision 3

| Technique | 主体 band | Zone 策略 | Boundary allowlist | Detail | Accent |
|---|---:|---|---|---|---|
| RTS 均衡体积 | 4–5 | upper/shoulder/side/lower 均衡 | bevel、必要 seam/contact | Macro+Meso | 克制 |
| 强轮廓可读 | 3–4 | 强化大块 front/side/upper 分离 | bevel/contact；不增加 silhouette outline | Macro 优先 | 小面积高对比 |
| 克制哑光层次 | 4–5 低差 | 宽阔连续 band | 少量 seam/contact | 抑制 Micro | 最低 |
| 材质分离优先 | 3–4 | 主体克制、material family 优先 | MaterialInterface 不描边 | Material Meso | 按材质 |
| 小型单位清晰化 | 3 | 压缩为大色块和少量 front cue | 仅关键 bevel/opening | Macro，折叠 Micro | 少量识别点 |

自动测试必须证明每种 technique 在同一 fixture 上至少有以下一种空间事实不同：zone role distribution、boundary
intent coverage、preserved feature class、material family distribution 或 accent distribution。仅 candidate hash 不同不够。

## 12. Class adaptation revision 3

### Ground

- UpperPlane/SideShoulder/SideField/LowerSkirt 为主要 body zones；
- FrontEnd 必须独立审阅；RearEnd 不复用 Front 的 decorative cue；
- contact shadow 允许用于 turret-ring/wheel-well 类有效 part contact，不允许形成整条 side-under 黑带；
- wheel/track/rubber 使用 material family，不默认纯黑；
- barrel/mantlet 沿 longitudinal continuity，不能逐 cell 斑驳。

### Air

- upper/ventral plan 分离，但 underside 不强制更暗；
- wing-root/nacelle/canopy/material interface 优先于细 panel line；
- paired explicit material/accent 默认要求镜像一致；
- leading edge 不允许完整亮线；Micro control details 受 feature-scale 压缩。

### LargeSurface

- v1 在不修改 semantic enum/persistence 的条件下，以 existing PartRole/MaterialRole + form zones 派生
  deck-like/hull-side/superstructure-like review facts；
- 不把推断事实写回 semantic sidecar；
- waterline-like boundary 只作 review fact，不允许整圈深色带；
- 低频 Macro band 优先，Micro panel detail 默认压缩。

若要正式新增 persisted Deck/Hull/Superstructure MaterialRole，必须另立 4D schema migration contract，不属于 Rev.7。

## 13. Normal / VPL 边界

- 若当前 session 有 snapshot-matched `Ra2VoxelNormalField`，quality/review 可显示 NormalFieldAvailable 和 normal mode；
- colour materialization 不修改 normal field，也不根据 WPF face normal 选择最终 index；
- Rev.7 不新增 VPL parser，不载入 custom VPL，不提供精确 VPL preview；
- `VplCompatibility` 固定为 `NotEvaluated`，并在 review package 中可见；
- `VisualAcceptance` 仍保持 Pending，直到用户完成外部 VXLSE/游戏验收。

## 14. Quality contract revision 3

新增或扩展的多维 facts：

```text
FlatSurfaceDarkSpotCount
IsolatedColourComponentCount
TonalBandContinuity
BodyBandCount
MaterialFamilyDistribution
BoundaryIntentDistribution
BoundaryOwnerViolationCount
AccentVisibleShare
AccentConnectedComponentPeak
FrontRecognitionCoverage / NotAvailable
ProjectedDetailSurvival
TechniqueSpatialDifference
NormalContextState
VplCompatibilityState
```

### 14.1 Blocked

- geometry/occupancy 改变；
- BodyBase 移动；
- palette/policy/orientation identity stale 或 mismatch；
- transparent/remap/illegal index 越权；
- boundary 覆盖 direct material/remap/human exact；
- boundary 无 owner 或多 owner；
- material family 越出 validated mask；
- result/quality/review hash 不一致；
- deterministic replay 不一致。

### 14.2 NeedsReview

- 未选择 forward direction，导致 front facts 不可用；
- SideField 异常暗点或孤立颜色组件；
- band continuity 低；
- accent 预算超出；
- Micro/SubPixelRisk 细节过多；
- material family 退化为单色；
- technique 空间差异不足；
- normal field 缺失；VPL 未评估；
- fixed-view/game-scale 识别风险。

不增加单一总分。阈值和 warning code 必须稳定、可测试、可本地化。

## 15. Skill revision 3

四个 colouring Skill 和五个 technique 文档必须与 typed revision 3 同步，但它们不是数值权威。

Provider 可返回：

- class guidance；
- major recognition surfaces；
- quiet large surfaces；
- material relationships；
- boundary intent preferences；
- details to preserve/compress；
- symmetry/review requirements；
- blockers/uncertainty。

Provider 不可返回：

- unit class authority、forward direction authority；
- base colour、technique 或 typed threshold；
- palette index、cell coordinate、mask membership；
- VPL/normal/GameReady 结论；
- 保存、导出或写入动作。

如果现有 structured schema 不能安全容纳 qualitative intent，Rev.7 第一版保留现有 schema并只同步 Skill 文本；不得
为改善提示词而扩大 Provider protocol。

## 16. 精确 UI 合同

UI仍位于现有五阶段 workspace；不得修改 Shell。

### 16.1 上色阶段左栏

在“人工确认单位类型”之后、“基准色与上色技法”之前新增：

```text
1.5 人工确认前向
[ForwardDirection Selector]
状态：已确认 +X / -X / +Y / -Y，或尚未确认
说明：只用于区分前/后，不改变模型方向或几何
```

- selector 只显示与当前水平坐标系一致的四个方向和“尚未确认”；
- model/evidence 变化时清空；base/technique 变化时保持；
- 不增加 AI 判断按钮；
- 不弹出 modal dialog。

### 16.2 全局预览工具栏

保留当前：

```text
预览模式 selector
分类预览：部件 / 材质
重置视角
切换切片
```

在“部件 / 材质”之后增加紧凑按钮：

```text
形体区
边界
风险
游戏尺寸
```

- 形体区：显示 FormZoneProjection；
- 边界：显示 BoundaryIntentProjection，不显示最终 palette；
- 风险：覆盖异常暗点、孤立色块、accent 超限和 subpixel risk；
- 游戏尺寸：进入固定缩放 review，不改变工作流阶段；再次点击恢复先前相机；
- 任何诊断模式缺少有效 generation 时 disabled，并回退 Semantics/Original 3D；
- 切换 technique、base 或 preview mode 不得切换到 Slice；
- 部件/材质/形体区/边界/风险均保持互斥 review dimension。

### 16.3 审阅与导出阶段

质量区域增加分组：

```text
结构与色阶
边界与材质
强调与细节
游戏尺寸与运行时边界
```

只显示 metrics/warnings，不增加 opaque score。NeedsReview 的 warning acceptance 仍绑定当前 generation hash；任一
输入变化后失效。

### 16.4 AutomationIds

保留全部现有 `VoxelStyle.*`，拟新增：

```text
VoxelStyle.Orientation.Selector
VoxelStyle.Orientation.Status
VoxelStyle.Preview.FormZones
VoxelStyle.Preview.BoundaryIntent
VoxelStyle.Preview.RiskOverlay
VoxelStyle.Preview.GameScale
VoxelStyle.ColourQuality.FormZones
VoxelStyle.ColourQuality.Boundaries
VoxelStyle.ColourQuality.Accents
VoxelStyle.ColourQuality.GameScale
```

不得新增 Shell AutomationId。AutomationId 精确字符串在批准后视为 UI contract，不得实现时改名。

## 17. 生命周期与 cache identity

```text
model/evidence/composition change
  → orientation、zones、boundaries、candidate、quality、warning acceptance 全失效

orientation change
  → zones、boundaries、candidate、quality、warning acceptance 失效

palette/base change
  → family、candidate、quality、warning acceptance 失效

technique change
  → materialization/boundary selection/detail filtering/candidate/quality 失效
  → raw Provider plan 不重新调用

preview mode/camera/game-scale toggle
  → 不使 candidate 或 quality 失效
```

cache identity 必须包含 form-zone/boundary/feature policy revision 和 projection hash。旧 envelope 安全 miss；不迁移、
不删除用户文件、不触发 Provider 调用。

## 18. 分阶段执行

| Stage | 目标 | 代码/文档 | 必须验证 | 停止门 |
|---|---|---|---|---|
| Rev.7-A | audit + exact contract | docs only | link/structure/diff audit | 等待精确批准 |
| Rev.7-B | orientation + form-zone facts | Application + focused tests | deterministic zones、no geometry change | targeted fail 即停 |
| Rev.7-C | tonal/material family + boundary intent materialization | Application + focused tests | anchor/direct/remap protection、flat-field fixtures | targeted/build fail 即停 |
| Rev.7-D | technique v3 + feature/accent budgets | Application + tests + technique docs | spatial difference, budget facts | targeted/build fail 即停 |
| Rev.7-E | quality v3 + fixed-view/game-scale facts | Application/IDE non-XAML + tests | hash, warnings, fixed projection | targeted/build fail 即停 |
| Rev.7-F | Skill v3 + approved UI wiring | Skills/ViewModel/XAML + tests | Skill catalog、UI contract、XAML build | automated fail 即停；物理验收 Pending |
| Rev.7-G | package verification / manual handoff | tests/docs/package | full suites + clean package | full gate fail 不得 claim complete |

每个 stage 使用现有 canonical path；不自动跨越失败门。真实 DeepSeek、VPL 和游戏测试不在自动验证内。

## 19. 测试矩阵

### Contract/model

- orientation identity、stale/mismatch、Unknown；
- policy revision/hash、invalid enum/threshold fail closed；
- zone/boundary/family projection stable ordering/hash。

### Geometry/form zones

- flat box、stepped hull、turret-on-body、thin wing、large deck fixtures；
- mirrored orientation；front/rear swap 只交换方向 zone；
- side+under 不得变成连续 underside band；
- flat SideField 不产生孤立 Recess。

### Materialization

- BodyBase exact；geometry/occupancy unchanged；
- direct Glass/Metal/Rubber/Light/remap protected；
- material family 不越 mask；
- boundary unique owner；RegionId-only ignored；
- no complete outline/no isolated boundary pixel；
- same input deterministic replay/hash。

### Technique

- 五技法在同 fixture 具有可解释的 spatial facts 差异；
- matte 的 accent/edge 不得高于 strong/compact；
- compact 压缩 Micro；material technique 保留最多 material family differentiation；
- sparse palette fallback 按 typed policy Warn/Block。

### Quality/review

- flat dark spot、isolated colour、band continuity、accent share、feature survival；
- NormalContext/VplCompatibility 状态准确；
- warning acceptance 绑定 generation，输入改变即失效；
- diagnostic preview 不改变 candidate。

### UI

- 精确 AutomationIds；
- orientation selector lifecycle；
- preview mode 不切 Slice、不改变阶段；
- missing/stale projection disabled/fallback；
- game-scale toggle 恢复相机；
- 100%/125% DPI 和真实样本由用户手动验收。

## 20. 验证命令

子阶段优先最小可信范围，package gate 才运行全套：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~VoxelColour|FullyQualifiedName~VoxelFormZone|FullyQualifiedName~VoxelBoundary|FullyQualifiedName~VoxelFeatureScale"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~VoxelStyle|FullyQualifiedName~VoxelViewport|FullyQualifiedName~AgentSkillCatalog"
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

不得用更弱命令替换失败门；全套只在 Rev.7-G 运行一次，除非代码实质变化需要复跑。

## 21. 人工验收

用户至少使用一个 ground、一个 air、一个 large-surface 真实样本检查：

- 平滑侧面无无原因黑块；
- 前/后方向和主要识别特征清楚；
- 无整圈亮边/黑边；
- 黄色/灯光/accent 不淹没主体；
- 五技法在正常视距下可辨别；
- 材质分类、形体区、边界、风险预览入口可找到且状态正确；
- 切换 technique 不进入 Slice；
- 游戏尺寸预览仍能识别单位；
- 100%/125% DPI 无裁切或空白 selector。

没有用户物理结论时只能写 `AutomatedVerified / VisualAcceptancePending`。

## 22. 自审与批准门

本合同不恢复自动判型，不让 Provider 取得 palette/cell authority，不修改持久化或 writer，不以 WPF 光照冒充
RA2 VPL。新增 orientation 是解决 front/rear 可靠性的最小人工输入；form zones、boundaries、feature facts 全部是
Application 派生数据，可丢弃、可重建、hash-bound。

本精确合同已由用户使用以下语句批准：

```text
批准并执行 ASSET-VOX-4E Rev.7 精确契约，按 Rev.7-B 至 Rev.7-G 的停止门连续推进；UI 仅按第16节实现。
```

## 23. 执行结果（2026-08-31）

- Rev.7-B..G：Completed / automated verified / VisualAcceptancePending。
- 第16节 UI：已按十个精确 AutomationId 实现；Shell 未改。
- Rev.7-G：临时 WPF `Application` 的默认 `OnLastWindowClose` 会在第一组控件关闭唯一宿主窗后清空
  `Application.Current`，遗留 Popup/DeferredAppResource 回调随后失败。测试现改为在断言生命周期内使用
  `OnExplicitShutdown`，先关闭 Popup 并排空 Dispatcher，再恢复资源并显式 Shutdown；产品主题和 Shell 未改。
- 最终门：restore、Debug solution build 0 warning/0 error、Application 368/368、AssetHost 50/50、IDE 2922/2922。
- clean package：Passed，`artifacts/RA2IniEditor.IDE.SourceClean.zip`，1470 entries，禁入目录/压缩包检查 0 违规。
  真实 DeepSeek 未运行；用户物理视觉验收仍为 Pending。
