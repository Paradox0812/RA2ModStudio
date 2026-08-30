# ASSET-VOX-4E — Mask-Driven Colour Materialization Final Contract Rev.3

日期：2026-08-30
状态：Approved / 4E-1 implemented and focused-verified / 4E-2..4E-5 not started
实现风险：R4
治理模式：StopForReview
前置审计：`Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationCodeFactAudit.md`
规则研究：`Docs/ASSET-VOX-4E_GroundAirColourTechniqueSourceStudy.md`
Rev.3 原因：在 Rev.2 人工基准色和质量闭环上，增加 DeepSeek 上色前判型、人工确认/纠正和 Ground/Air/
LargeSurface 专用 Skill 路由；重新冻结两阶段模型调用、cache identity、UI 和失效语义。

## 1. 目标结果

上色前，DeepSeek 必须基于 bounded geometry/semantic evidence 提出 Ground/Air/LargeSurface/Unknown 判型及理由；
用户确认或纠正后，Host 只加载与 `ConfirmedUnitClass` 对应的一个专用 colouring Skill。用户还必须从当前 active
PAL/VOX palette 中人工选择合法基准色和一个版本化内置技法。主体颜色家族以该基准色为唯一精确锚点；项目
`VOXEL_STYLE.md` / 本次自然语言要求只补充非主体材质颜色和不冲突的定性意图。最终由本地确定性 pipeline 将这些
输入与 4A/4B/4D semantic mask、canonical snapshot 和 active palette 组合为可审阅候选。

4E 输出普通候选、可选对比度候选、typed role/binding 计划和多维质量报告。只有通过硬门禁的候选才能预览为
有效结果；存在软质量警告时必须显式确认，才能固化或继续使用现有 `.vox` export。

4E 不改变几何、占用、语义权威、4D sidecar、项目 Apply/Save、VOX codec、VXL/HVA writer 或游戏正确性边界。

## 2. 契约加载摘要

### 2.1 当前任务目标

在不复制现有 colourizer 的前提下，完成 DeepSeek 判型/人工确认、class-specific Skill 路由、用户锁定的 palette
基准色、与色相无关的 technique、Ground/Air/LargeSurface/Unknown 适配、语义 binding、分层缓存身份、质量准入
和最小 UI 接线。

### 2.2 允许文件上限

实现时每个子阶段仍需声明精确文件；以下仅为总 allowlist：

- `RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/Ra2VoxelStyleContracts.cs`；
- `Ra2VoxelSemanticMasking.cs`、`Ra2VoxelColourizer.cs`、`Ra2VoxelColourReviewPackage.cs`、
  `Ra2VoxelPaletteContrast.cs`；
- 新增窄命名的 `Ra2VoxelColourTemplate*.cs`、`Ra2VoxelSemanticColour*.cs`、
  `Ra2VoxelColourQuality*.cs`、`Ra2VoxelUnitClass*.cs`、`Ra2VoxelColourSkill*.cs`；
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelStyleCompiler.cs`、`Ra2VoxelStylePlanCache.cs`、
  `Ra2VoxelStylePreviewCoordinator.cs`、`Ra2VoxelStyleSourceResolver.cs`；
- `RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs`；
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml`；
- `RA2IniEditor.IDE/VoxelStyles/templates/**/TECHNIQUE.md` 与最小内容声明；
- `RA2IniEditor.IDE/AgentSkills/ra2-voxel-unit-classification/**`、
  `ra2-ground-voxel-colour-techniques/**`、`ra2-air-voxel-colour-techniques/**`、
  `ra2-large-surface-voxel-colour-techniques/**`，以及现有
  `ra2-voxel-colour-techniques/SKILL.md` 的窄同步和 catalog tests；
- 直接相关 Application/IDE tests；
- 4E Contract、Stage Ledger、DecisionLog、CurrentPhase、Context 和文档索引。

### 2.3 禁止文件与行为

- `ShellWindow.xaml`、`ShellWindow.xaml.cs`、菜单、工具栏、Dock 布局和全局 Shell 生命周期；
- 4D `.semantic.json` v1 schema/store、自动保存/载入、迁移或 merge；
- INI parser、Field Registry、Completion、Hover、Quick Peek、Diagnostics、Save Preflight；
- 项目 Preview/Apply/Undo/Redo/Save、backup/rollback、Artifact Registry；
- Provider/AssetHost 协议、真实付费调用、模型路由，以及把通用 Agent Skill 变成运行时权威；
- public .NET API、依赖、项目格式和 build configuration，除已存在的模板 Markdown content glob 外；
- canonical VOX/VXL reader/writer、VXL/HVA、normal、pivot/mount 或 GameReady 判断；
- 第二套 palette、quantizer、style compiler、semantic composer 或 colourizer。

### 2.4 语义边界

- 4E 只改变已占用 cell 的 palette index，不增加、删除或移动 cell。
- `CellHumanOverride > RegionHumanOverride > AgentSuggestion > Unknown` 保持不变。
- MaterialRole 决定 4E v1 的颜色角色；PartRole 只用于审阅、分组和诊断，不选择独立 palette family。
- Unknown cell 使用 base geometry style，不被伪装成已识别材质。
- remap 只有 `ExplicitlyApproved` 且 active palette 提供 remap range 时才能应用。
- `BaseColourSelection` 是用户对当前 palette 的 session-scoped 权威输入；Provider、模板、文件名和模型主色统计均
  无权替换或自动选择它。
- DeepSeek `UnitClassProposal` 是不可信判型提案；只有用户确认/纠正后的 `ConfirmedUnitClass` 才能决定
  UnitAdaptationPolicy 和专用 colouring Skill。
- v1 只有一个全模型主体基准色，不为 Body/Turret/Barrel 创建独立基准色；该扩展必须另立合同。
- 模板、项目 prose 和模型输出都不能生成 cell 坐标、mask membership、文件路径或写入动作。

### 2.5 AutomationIds

保留全部现有 `VoxelStyle.*` ID，并新增：

```text
VoxelStyle.Template.Selector
VoxelStyle.Template.Description
VoxelStyle.UnitClass.Analyze
VoxelStyle.UnitClass.Status
VoxelStyle.UnitClass.Evidence
VoxelStyle.UnitClass.Selector
VoxelStyle.UnitClass.Confirm
VoxelStyle.UnitClass.Skill
VoxelStyle.BaseColour.Selector
VoxelStyle.BaseColour.Swatch
VoxelStyle.BaseColour.Status
VoxelStyle.ColourQuality.Status
VoxelStyle.ColourQuality.Metrics
VoxelStyle.ColourQuality.Warnings
VoxelStyle.ColourQuality.AcceptWarnings
```

不得新增 Shell AutomationId。

### 2.6 验证命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~VoxelColour|FullyQualifiedName~VoxelStyle|FullyQualifiedName~VoxelSemantic"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~VoxelStyle|FullyQualifiedName~VoxelSemantic"
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

### 2.7 实现批准状态

用户已于 2026-08-31 明确批准本 Rev.3。4E-1 已按本合同实现并通过聚焦自动化验证；4E-2..4E-5 仍须逐阶段满足
停止门。任何超出本合同的 UI、持久化、public API、Apply/Save 或 writer 变更仍必须另行立约。

## 3. 核心架构决策

### 3.1 单一计划与单一 colourizer

模板路径与自然语言路径都必须收敛为现有 `Ra2VoxelStylePlanDefinition`，通过同一个本地
`Ra2VoxelStylePlanCompiler` 验证和 palette resolve，再由 `Ra2VoxelColourizer` 应用。

不得创建模板专用 painter、直接 palette-index 循环或仅存在于 ViewModel 的规则字典。

### 3.2 颜色意图与技法规则正交

人工 `BaseColourSelection` 是主体色相和 BodyBase palette index 的最高权威。`VOXEL_STYLE.md`、项目/目录来源和
本次 override 继续表达玻璃、金属、橡胶、灯具、accent 等非主体颜色以及不冲突的定性风格；其中任何主体色文字
都必须标记为被人工基准色覆盖，不能改写 BodyBase。技法模板不提供色相、阵营、战区或具体 RGB；它只冻结相对
明暗、区域应用顺序、边缘处理、材质分离强度和质量阈值。

现有 structured compiler 仍负责把自然语言颜色意图编译成 palette roles 和 binding proposal。随后本地
normalizer 以人工基准色为 BodyBase 精确锚点，按选中 technique 和 unit adaptation 确定性验证或派生
BodyLight/BodyMid/BodyDark/Underside、EdgeOrRidge 和材质对比规则，再回到同一个
`Ra2VoxelStylePlanCompiler`。模板不能成为第二套 style compiler。

Provider 调用改为两个独立、可缓存阶段：判型 cache miss 最多一次 required structured call；人工确认后，style
compile cache miss 最多一次既有 required structured call。两次调用不得合并统计为“一次”。基准色、technique 和
质量评估仍在本地应用；切换它们不得增加模型调用。

### 3.3 DeepSeek 判型与人工确认

判型输入只能是 bounded、textual、Host-owned facts；不得给模型原始 cell coordinates、文件写入权限或把文件名当成
唯一证据。typed input/output 至少包含：

```text
UnitClassEvidence
  ModelIdentity
  GeometryFactsHash
  SemanticFactsHash
  OrientationFacts
  Facts[]: FactId + FactKind + BoundedValue + HostSource
  EvidenceHash

UnitClassProposal
  ProposedClass: Ground | Air | LargeSurface | Unknown
  ConfidenceBand: High | Medium | Low
  EvidenceFactIds[]
  Reason
  ClassifierSkillId + Revision + Hash
  EvidenceHash
  ProposalHash

ConfirmedUnitClass
  Class: Ground | Air | LargeSurface | Unknown
  Source: HumanConfirmedProposal | HumanOverride | ManualWithoutAiAssessment
  ProposalHash?
  EvidenceHash
  ConfirmationHash
```

`GeometryFactsHash` 只绑定有界尺寸、占用拓扑、部件关系和方向事实，不绑定逐 cell 坐标；
`SemanticFactsHash` 只绑定稳定的 PartRole/MaterialRole 存在性、来源和证据身份，不绑定 cell 数量或笔划边界。
文件名可以作为低权重提示，但不得单独决定 class，也不得进入没有其它几何/语义证据支持的结论。

DeepSeek 只能输出 `UnitClassProposal`。UI 必须展示 class、confidence、证据和不确定性；用户确认或纠正后才生成
`ConfirmedUnitClass`。正常流程必须先运行判型；Provider 不可用时允许用户显式使用
`ManualWithoutAiAssessment`，但必须产生 NeedsReview，不能伪装成 DeepSeek 已确认。模型返回 Unknown/Low 也必须
由用户明确确认或纠正。

classification cache key 至少包含 `ModelIdentity + EvidenceHash + ClassifierSkillId/Revision/Hash +
ProviderModelIdentity + ClassifierSchemaRevision`。精确 hit 不调用 Provider；EvidenceHash 或 classifier identity 改变时
proposal/confirmation 失效。任何 class 改变都使 style plan、bundle、candidate、freeze 和 acknowledgement 失效。

### 3.4 Class-specific Skill 与单一规则权威

- C# typed `TechniquePolicy` 与 `UnitAdaptationPolicy` catalog 是运行时数值、revision 和 hash 的唯一权威。
- 判型只加载 `ra2-voxel-unit-classification`；style compiler 只加载 ConfirmedUnitClass 对应的一个专用 Skill：
  - Ground → `ra2-ground-voxel-colour-techniques`；
  - Air → `ra2-air-voxel-colour-techniques`；
  - LargeSurface → `ra2-large-surface-voxel-colour-techniques`；
  - Unknown → 现有保守 `ra2-voxel-colour-techniques`，并强制 NeedsReview。
- Host 按 confirmed enum 进行 exact skill-ID 路由；DeepSeek 不能自选、追加或混合其它 unit-class Skill。style prompt
  必须记录唯一 SkillId/Revision/ContentHash，超过既有 instruction limit 时 fail closed。
- 专用 Skill 保存 class-specific qualitative technique knowledge；typed C# policy 保存数值阈值和硬门，两者职责不同且
  不复制同一数值表。Skill 不能覆盖 BaseColour、palette legality、semantic mask、remap approval 或 geometry invariant。
- DeepSeek 所谓“按 Skill 绘制”只表示输出 bounded colour-role/binding proposal；实际 palette resolve、mask 应用和
  cell 着色仍由本地 normalizer/plan compiler/colourizer 唯一执行。

### 3.5 显式 semantic binding

新增独立的 validated semantic binding plan，而不是把绑定继续隐含在 `RoleCategory.First()` 中。

每个当前出现且非 Unknown 的 MaterialRole 必须恰好绑定一个 validated binding；approved remap 必须恰好绑定
一个 Remap roleId。`PaintedSurface` 使用 `BodyGeometryFamily` binding，不直接覆盖成单个 BodyBase；其余材质使用
`DirectRole` binding。Light 和 Accent 必须在同时出现时绑定两个不同的 `Accent` 类别 role，因此绑定键不能只用
类别推断。

PartRole 不进入 4E v1 binding key。未来若需要 Body/Turret/Barrel 独立 palette family，必须另立 4F/5A 合同。

### 3.6 人工基准色合同

`BaseColourSelection` 是 immutable session input，至少包含：

```text
PaletteProfileHash
PaletteIndex
ResolvedRgba              # 从 active palette 派生，仅作显示/验证
BaseColourSelectionHash
Source: HumanPaletteSelection
```

canonical hash 只序列化 `PaletteProfileHash + PaletteIndex + Source`；`ResolvedRgba` 必须由 palette 回读一致，
不能作为第二份可变颜色真相。

准入规则：

- 用户只能从当前 active palette 的实际条目中选择；不得使用任意 RGB colour picker 后再暗中量化。
- 选择必须是 opaque、non-transparent、non-remap index；remap range 仍只属于批准的 remap mask。
- `BodyBase.PaletteIndex` 必须始终等于人工选择；normalizer 和 contrast optimizer 都不得移动该 index。
- BodyLight/BodyMid/BodyDark/Underside 以该 RGB/luminance/chroma 为共同锚点确定性派生；不得各自跳到无关色带。
- Provider 若为主体角色提出另一 exact index，本地 normalizer 必须拒绝该主体 exact proposal、保留人工锚点并记录
  provenance fact；这不是 Provider clarification，也不能触发第二次调用。
- 没有合法人工基准色时，“编译着色预览”不可用并返回 `BaseColourRequired`；不得自动使用模型主色、文件名、
  `VOXEL_STYLE.md` 中的颜色词或 palette 第一个可用颜色。

基准色只拥有主体 painted family；Glass、Rubber、BareMetal、Light、DarkOpening、Accent 和 Remap 仍按各自
semantic binding 处理，但其非 exact 候选和质量比较都必须以 BodyBase 为共同参照，形成受控材质分离而不是另一套
无关主题。这里的“以基准色为中心”不要求玻璃、橡胶或金属与主体同色相。Unknown cell 继续使用以同一基准色为
锚点的 base geometry family，但仍报告为 Unknown。

### 3.7 ConfirmedUnitClass 与 Unit adaptation 合同

`ConfirmedUnitClass` 唯一决定 session-scoped `UnitAdaptationProfile` 和专用 Skill；不再存在独立、可能与 class
冲突的 adaptation 选择：

```text
Ground
Air
LargeSurface
Unknown
```

不得根据模型名称、ZIP/文件名、阵营或颜色自动路由。DeepSeek proposal 也不能直接路由，必须先成为人工确认结果。
`Unknown` 是可用的保守模式，但候选至少为 NeedsReview。新模型 identity 清空 proposal/confirmation；同一模型
只有 EvidenceHash 未变时才可复用确认，否则必须重新判型或显式手动确认。

| Class/Profile | 唯一 colouring Skill | 主体/Underside 规则 | Top+Under 同 cell | 重点 |
|---|---|---|---|---|
| `Ground` | `ra2-ground-voxel-colour-techniques` | Top 通常更亮，底部默认更暗 | `UnderPreferred` | 车体、底盘、履带/轮组、炮塔轮廓 |
| `Air` | `ra2-air-voxel-colour-techniques` | 机翼/机身分层；Underside 可更亮或更暗但须可区分 | `BodyBase` | 平面轮廓、翼根、前后缘、座舱、进排气口 |
| `LargeSurface` | `ra2-large-surface-voxel-colour-techniques` | 长平面低频分组；下部更暗仅为软目标 | `TopPreferred` | 甲板、船体、上层建筑、稀疏结构强调 |
| `Unknown` | `ra2-voxel-colour-techniques` | conservative generic hierarchy | `BodyBase` | 强制人工审阅 |

`TechniquePolicy × UnitAdaptationPolicy` 共同生成 effective local policy；二者 ID、revision、policy hash 均进入
materialization bundle。LargeSurface v1 不新增 Deck/Hull 材质枚举；缺少相应人工语义时只能报告限制，不能伪装成
已经完成甲板/船体材质识别。

`UnitAdaptationPolicy` 必须包含 `DualSurfacePolicy`。这是必要的 cell-level 冲突裁决：palette index 属于整个 voxel
而不是独立 face，不能同时给同一 cell 写顶面色和底面色。

## 4. 内置技法模板契约

### 4.1 模板定义的是“怎么上色”

模板是只读、版本化 technique policy，包含：

- stable `TechniqueId`、`Revision`、显示名和适用说明；
- geometry rule set 与规则应用顺序；
- 相对 BodyBase 的目标亮度偏移，而不是目标色相/RGB；
- edge、interior、underside、material separation、accent 和 remap discipline；
- quality policy、fallback policy 和内容 hash；
- 用户可读的规则/技法说明文档。

模板明确不得包含：阵营、战区、橄榄绿/蓝灰/沙色等主题、exact palette index、target RGB、材质 mask 或坐标。
主体颜色权威属于人工基准色；非主体颜色和定性意图仍由既有 style source pack 所有。最终 index 始终来自当前
明确 PAL/VOX palette。

### 4.2 v1 内置技法目录

| TechniqueId | 显示名 | 核心规则 | 适用场景 |
|---|---|---|---|
| `balanced-rts-volume` | RTS 均衡体积 | 中等顶光、侧面压暗、深底部、轻边缘提亮 | 默认通用单位 |
| `strong-silhouette-readability` | 强轮廓可读 | 扩大明暗级差、强化底部和外轮廓 | 远距离/较小屏幕 |
| `subtle-matte-shading` | 克制哑光层次 | 较小明暗级差、弱边缘、避免塑料高光 | 大体积或写实哑光 |
| `semantic-material-separation` | 材质分离优先 | 中等体积明暗、加强玻璃/橡胶/金属/灯具区分 | 语义 mask 完整模型 |
| `compact-unit-clarity` | 小型单位清晰化 | 强顶部/底部层级、清晰开口和小面积 accent | 低体素数单位 |

`balanced-rts-volume` 是首次打开工作区的默认选择。不得根据模型名称、文件名、阵营、颜色文字或 AI 猜测自动
切换技法。

### 4.3 Typed technique parameters

所有模板必须定义并本地验证：

```text
TopLuminanceOffset
SideLuminanceOffset
DarkLuminanceOffset
PreferredUndersideLuminanceOffset
EdgePolicy: None | Subtle | Strong
EdgeLuminanceOffset
MaterialSeparationPolicy: Conservative | Balanced | Strong
MinimumBodyLuminanceSeparation
DarkOpeningMinimumDelta
AccentPolicy: PreserveMask | EmphasizeSmallMask
RemapPolicy: ExplicitMaskOnly
QuantizationFallback: WarnAndPreserveIntent | Block
LuminanceMetricId: rec709-srgb-byte-luma-v1
ColourFamilyMetricId: oklab-anchor-v1
```

v1 初始参数：

| TechniqueId | Top | Side | Dark | Under | Edge | 最小 body 级差 |
|---|---:|---:|---:|---:|---:|---:|
| `balanced-rts-volume` | +18 | -8 | -28 | -38 | +24 Subtle | 8 |
| `strong-silhouette-readability` | +28 | -12 | -38 | -52 | +34 Strong | 12 |
| `subtle-matte-shading` | +12 | -5 | -20 | -28 | +15 Subtle | 6 |
| `semantic-material-separation` | +16 | -7 | -26 | -36 | +20 Subtle | 8 |
| `compact-unit-clarity` | +24 | -10 | -34 | -46 | +30 Strong | 10 |

数值是相对人工 BodyBase 的 luminance 目标，不是 RGB 或 palette index。表中的 Under 是 Ground 的首选方向；
Air/Unknown 使用其绝对差异目标但不强制负号，LargeSurface 按 adaptation policy 将方向作为软目标。若 active
palette 无法满足目标，按模板 fallback 产生 NeedsReview 或 Blocked，不得跨 palette、使用透明色、使用 remap
index 或改写人工基准色。

### 4.4 基准色家族选择算法

派生 body roles 使用一个共享、版本化的本地 selector，而不是每个角色独立寻找全 palette 最近色：

1. anchor 固定为人工 `BaseColourSelection`；候选只来自 active palette 的 opaque、non-remap entries。
2. luminance 使用 `0.2126R + 0.7152G + 0.0722B` 的 byte-space v1 事实；不得与
   `srgb-squared-v1` 距离指标混为一谈。
3. family coherence 使用标准 sRGB-to-OKLab 的 `oklab-anchor-v1`：chromatic anchor 的首选候选满足 hue drift
   `<= 30°` 且 chroma delta `<= 0.12`；OKLab chroma `< 0.035` 的 neutral anchor 首选候选 chroma
   `<= 0.055`。
4. 在首选 family 内按 `(relation violation, target luminance error, OKLab distance, palette index)` 稳定排序；
   多个 body roles 必须联合验证顺序和最小间隔，不能各自得到互相冲突的独立最优解。
5. 所需明暗方向没有首选 family 候选时，`WarnAndPreserveIntent` 只能复用首选 family 中关系违例最小的候选
   （最坏可复用 anchor），并产生 `PaletteFamilyFallback`/角色折叠警告进入 NeedsReview；不得跳到 family 外只为
   满足亮度。`Block` 则直接拒绝候选。
6. BodyBase 永远不参与重新选择；contrast candidate 只可调整非 exact 的派生 body roles，并使用同一 selector。

阈值和 metric ID 属于 policy hash；实现阶段不得在 optimizer、quality evaluator 和 template 中维护不同副本。

### 4.5 固定规则全集

所有模板都使用同一规范 region vocabulary：

```text
WholePart -> BodyBase
TopExposed -> BodyLight
SideExposed -> BodyMid
UnderExposed -> Underside
Interior -> BodyDark
EdgeOrRidge -> template EdgePolicy
```

确定性应用顺序冻结为：WholePart → Interior → exclusive primary surface → EdgeOrRidge → DirectSemanticMaterial →
ApprovedRemap。primary surface 一般在 Side/Top/Under 中选择；同时 Top+Under 时必须先按当前
`DualSurfacePolicy` 折叠为一个角色，禁止依赖“后执行的规则碰巧覆盖前一个规则”。

PaintedSurface、Glass、Rubber、BareMetal、Light、DarkOpening、Accent 和 approved Remap 继续由显式 semantic
bindings/masks 控制。PaintedSurface mask 只证明该 cell 属于主体 painted family：它保留 WholePart/Top/Side/
Under/Interior/Edge 的几何分层，不追加一个会把所有 cell 涂成 BodyBase 的后置 direct rule。其它材质用后置
DirectRole mask 覆盖，approved Remap 最后覆盖。技法模板只能调整相对对比政策，不能创建或扩大 semantic mask。

### 4.6 与 style source 的关系

技法选择是与既有 source precedence 正交的 trusted local input：

```text
built-in default VOXEL_STYLE.md < project < directory < request override
                          + human BaseColourSelection
                          + ConfirmedUnitClass
                          + exactly one class-specific Skill
                          + derived UnitAdaptationPolicy
                          + selected TechniquePolicy
```

一次只允许一个 confirmed class、一个对应 Skill、一个技法模板和一个基准色。它们不修改 `VOXEL_STYLE.md`，也不
持久化到 4D sidecar；v1 只属于当前 workspace session。人工基准色只覆盖 source pack 的主体颜色意图，不会静默
覆盖合法的非主体材质 exact selection。

## 5. Semantic colour requirements

### 5.1 Requirements projection

由当前 `Ra2VoxelSemanticMaskComposition` 本地投影 immutable requirements，至少包含：

```text
SourceSnapshotHash
CompositionHash
RequirementShapeHash
MaterialCounts[]
UnknownCellCount
ApprovedRemapCellCount
```

`MaterialCounts` 按 MaterialRole 稳定排序；总数必须等于 occupancy count。approved remap 是可与 MaterialRole
重叠的最终覆盖维度，其 count 不再次计入总数。requirements 不包含坐标、基准色或 unit adaptation。

### 5.2 双 hash 目的

- `CompositionHash` 绑定完整逐 cell 组合、质量报告、mask 物化和最终候选 provenance。
- `RequirementShapeHash` 只绑定“出现的非 Unknown MaterialRole 集合 + approved-remap presence”，用于样式编译
  请求等价性和 cache key。

只改变同一材质集合中的 cell 边界时，复用已验证 style/binding plan，并在本地重建 masks/quality；不得再次调用
Provider。新增/删除材质种类或改变 approved remap presence 时，RequirementShapeHash 改变，必须重新编译或命中
精确匹配的新缓存。

模型 prompt 不接收 cell counts 或 composition hash，以保证同一 RequirementShapeHash 的请求确实等价。

## 6. Compiler、binding 与 cache v2

### 6.1 Structured output

自定义 prose 路径的 tool schema 在现有 roles/rules 之外新增 bounded `semantic_bindings`：

```text
material_role: painted_surface | glass | rubber | bare_metal |
               light | dark_opening | accent | approved_remap
binding_mode:  body_geometry_family | direct_role
role_id:       existing role id; painted_surface 必须引用 BodyBase family anchor
```

不得返回 PartRole、cell index、coordinate、mask ID 或 mask membership。

### 6.2 本地 validator

本地验证必须保证：

- 每个 requirement key 恰好一个 binding；无多余、重复或未知 key；
- roleId 存在，且 mode/category 与下表兼容；
- approved remap 只能绑定 Remap role，并要求 active palette 有 remap range；
- PaintedSurface 的 body family 必须包含完整 required geometry roles，但不得生成后置单色 explicit rule；
- Light 与 Accent 同时出现时必须是不同 roleId；
- cached typed bindings 与 prose/source/RequirementShapeHash 一致；
- binding plan、raw style plan、local normalization inputs 和 composition 共同形成 materialization bundle hash。

任何失败均不能回退到 `GroupBy(Category).First()`。

| Material requirement | BindingMode | 允许的 role category / 规则 |
|---|---|---|
| PaintedSurface | BodyGeometryFamily | BodyBase anchor；最终角色由 geometry region 在 body family 内决定 |
| Glass | DirectRole | Glass |
| Rubber | DirectRole | Rubber |
| BareMetal | DirectRole | BareMetal |
| Light | DirectRole | Accent；与 Accent requirement 的 roleId 不同 |
| DarkOpening | DirectRole | BodyDark；必须满足更暗质量门 |
| Accent | DirectRole | Accent；与 Light requirement 的 roleId 不同 |
| ApprovedRemap | DirectRole | Remap；最终优先级最高 |

### 6.3 Cache identity

必须区分 Provider compilation cache 与本地 materialization identity，不能用一个 key 同时决定“是否付费调用”和
“当前候选是否仍有效”。

Provider style cache schema/revision 升级到 v2，在既有 identity 上至少增加：

```text
RequirementShapeHash
BindingSchemaRevision
ConfirmedUnitClassValue
ColourSkillId
ColourSkillRevision
ColourSkillContentHash
```

confirmed class enum 和唯一专用 Skill 会改变 style prompt，因此必须进入 Provider style cache。人工纠正为不同
class 或 Skill revision/hash 改变时，旧 raw plan 安全 miss；EvidenceHash 改变但重新确认得到同一 class/Skill 时，
style cache 可精确复用，新的 evidence/confirmation 仍进入本地 bundle。`BaseColourSelection`、Technique、typed
UnitAdaptation 数值和 QualityPolicy 不进入 style prompt/key；它们在 raw proposal/cache hit 后本地应用，因此只
切换 base/technique 不得触发 Provider。

materialization bundle identity 至少包含：

```text
RawCompiledPlanHash
BindingPlanHash
PaletteProfileHash
BaseColourSelectionHash
UnitClassEvidenceHash
UnitClassConfirmationHash
ClassifierSkillId + ClassifierSkillRevision + ClassifierSkillContentHash
ColourSkillId + ColourSkillRevision + ColourSkillContentHash
TechniqueId + TechniqueRevision + TechniquePolicyHash
UnitAdaptationId + UnitAdaptationRevision + UnitAdaptationPolicyHash
RequirementShapeHash
CompositionHash
BindingSchemaRevision
LuminanceMetricId
ColourFamilyMetricId
QualityPolicyHash
ContrastPolicyRevision             # contrast candidate only
```

旧 v1 cache 安全 miss，不迁移、不删除。source pack、palette、RequirementShapeHash、confirmed class、specialized
Skill、compiler/schema/model identity 变化时才重新 style compile 或命中另一精确缓存；同一材质集合中的 brush
边界变化只重建 composition-bound masks、candidate 和 quality。

## 7. 确定性 materialization

1. 从 active working snapshot/semantic state 构建 bounded `UnitClassEvidence`。
2. 命中 classification cache，或使用 classification Skill 调用 DeepSeek 得到 `UnitClassProposal`；失败不改变旧确认。
3. 用户确认/纠正 proposal，或在 Provider 不可用时显式选择 ManualWithoutAiAssessment；生成 ConfirmedUnitClass。
4. Host 由 confirmed enum 唯一选择 colouring Skill 和 UnitAdaptationPolicy，并验证 SkillId/Revision/Hash。
5. 捕获 active palette、semantic composition、人工基准色、technique 和 style sources；任一缺失或 hash 不匹配时
   fail closed。
6. 构建 requirements 并验证 snapshot/composition/count；approved remap 作为重叠最终覆盖维度单独计数。
7. 使用“只含该 class-specific Skill”的 structured compiler 获取 raw 颜色角色/binding，或命中 Provider style cache。
8. 本地 normalizer 锁定 BodyBase 为人工 index，按 `TechniquePolicy × UnitAdaptationPolicy` 和共享 family selector
   派生/规范化 geometry roles/rules；主体 prose 冲突只记录 provenance，不改变锚点。
9. 本地验证 normalized style plan、semantic binding plan、class/Skill/policy/metric revision 和 bundle identity。
10. 从 composition 稳定物化 mutually-exclusive MaterialRole masks 与独立 approved-remap mask；不把 remap count
   重复算进 MaterialRole occupancy。
11. `PaintedSurface` 由既有 geometry rules 在 body family 内着色，不生成 flattening direct rule；Glass/Rubber/
   BareMetal/Light/DarkOpening/Accent 作为 DirectRole 后置覆盖；approved Remap 最后覆盖；Unknown 不生成 semantic
   rule 并保留 body geometry style。
12. 用现有 `Ra2VoxelColourizer` 的同一确定性应用路径生成 ordinary candidate。
13. 用改为 policy-aware 的现有 contrast optimizer 生成可选 contrast plan；它只能改变非 exact 派生 body roles，
   不得改变 BodyBase、semantic direct roles、remap 或其它 exact palette selection。
14. 对两类候选独立生成 quality report 和 review artifacts，并按 expected effective precedence 核对 cell counts。
15. 原子发布同一 generation 的结果；过期、取消或失败结果不得替换当前有效预览。

## 8. 上色质量保障契约

### 8.1 不使用单一质量分

4E 不提供“82 分”之类不可解释的总分。质量由 correctness、semantic coverage、palette fit、readability、
distribution 和 human review 六个维度组成，最终状态只有：

```text
Blocked      硬错误；无可固化候选
NeedsReview  候选正确但存在可解释的视觉/覆盖警告
ReviewReady  硬门禁通过且无未确认警告
```

`ReviewReady` 只表示“技术上可固化并进入人工视觉审阅”，不表示艺术完成、VXL/HVA 正确或 GameReady。

### 8.2 硬门禁：任一失败即 Blocked

- snapshot、palette、composition、mask、bundle hash 精确匹配；
- UnitClassEvidence、confirmation、classifier Skill 和唯一 colouring Skill identity 精确匹配；proposal 未确认、
  class/Skill 路由不一致或混入多个 class Skill 均 Blocked；
- BaseColourSelection 存在、属于 active palette、opaque/non-remap，且最终 BodyBase index 精确不变；
- Technique、UnitAdaptation、luminance/family metric 和 quality policy revision/hash 全部匹配；
- geometry、coordinate order、occupancy、part identity 完全不变；
- 所有非 Unknown MaterialRole cell 恰好被一个 validated binding 解释；PaintedSurface 由 body family 覆盖，其余
  由 direct semantic rule 覆盖；
- expected effective precedence 为 `ApprovedRemap > DirectSemanticMaterial > BodyGeometryFamily`，最终 applied role
  cell count 必须与该 precedence 计算出的 expected count 一致；
- 任何输出 index 都不是 transparent；remap 只覆盖 explicitly approved cells；
- required geometry roles/rules 完整，known semantic group 无 unresolved；
- plan/binding/cache schema 本地验证通过；
- candidate 和 review package hash 回读一致。

### 8.3 Technique- and adaptation-aware readability policy

- Ground Skill/Policy 要求 `BodyLight > BodyBase > BodyMid > BodyDark > Underside`；palette 稀疏导致的角色折叠只能按
  fallback 进入 NeedsReview/Blocked，不能伪装成顺序成立；
- Air Skill/Policy 要求 body planes 和 underside 达到 policy-defined 最小差异，但 Underside 可更亮或更暗；
- LargeSurface Skill/Policy 将 underside 更暗和长平面节奏作为软目标，禁止用全模型平均值替代局部比较；
- Unknown 使用 conservative hierarchy 并强制 NeedsReview；
- body 最小亮度差采用第 4.3 节当前 TechniquePolicy 的阈值，不再用一个全局模板值；
- BodyBase 必须是人工 exact index；每个派生 body role 报告 anchor OKLab hue/chroma drift 和是否发生 family fallback；
- Glass/Rubber/BareMetal/Light/Accent 与其 template-declared comparison role 至少满足
  `RGB squared distance >= 324` 或 `absolute luminance difference >= 8.0`；
- DarkOpening 与 BodyBase 的亮度差由 technique policy 声明，任何模板都不得低于 `12.0` 且方向为更暗；
- target RGB 到实际 palette 色的 squared error 超过 `1600` 时产生 quantization warning；
- 期望多个角色但结果为 uniform colour 时产生 warning。

v1 comparison role 固定为 BodyBase：Glass、Rubber、BareMetal、Light 和 Accent 分别与人工 BodyBase 比较；
Light 与 Accent 仍使用不同 roleId 并分别报告。不得在 evaluator 内临时改比较对象以使候选通过。

低于目标不是 geometry/palette 正确性错误，因此进入 `NeedsReview`，同时提供 policy-aware deterministic contrast
candidate；若其自身全部 adaptation/family/semantic 门禁达标，它可以独立成为 `ReviewReady`。contrast 不得通过
移动人工 BodyBase 或非主体 semantic colour 来“修分”。

### 8.4 Coverage 与 distribution facts

质量报告必须展示：

- occupancy、Known/Unknown cell count 和比例；
- DeepSeek proposed class/confidence、confirmation source、EvidenceHash，以及 classifier/colouring Skill ID/revision/hash；
- 每个 MaterialRole、roleId、geometry region 的 cell count，以及 Top+Under dual-surface count/裁决结果；
- approved remap count 与实际 remap count；
- distinct palette index count；
- 人工基准 palette index/RGB/luminance、body 最小亮度差、每个派生 role 的 anchor drift/family fallback；
- confirmed unit class、unit adaptation、关键材质对比、最大 palette quantization error；
- ordinary/contrast 各自状态、警告和 base/technique/adaptation/requirements/bundle hash。

distribution facts 还必须包含每个最终 role 的 connected-component count、isolated-cell count、bounding-box spread，
以及仅在已有可信 symmetry evidence 时计算的左右不一致 count。Unknown 大于 0 固定产生 warning；approved remap
大于 0 只显示显著事实，不因存在本身警告。v1 不用未经样本验证的固定面积/碎片阈值自动否决艺术选择；这些事实
只支持人工审阅和后续基于样本立约的 policy。

v1 DirectRole（包括 approved remap）使用单一 palette index。若一个 DirectRole mask 跨越 Top/Side/Under 中两个或
更多 primary regions，必须产生 `FlatSemanticMaterialAcrossRegions` 可见警告；不得暗示玻璃、金属或 remap 已获得
独立材质内明暗家族。多级 semantic/remap family 属于后续合同。

### 8.5 自动质量与人工视觉边界

- 自动硬门只证明 invariants、palette/remap legality、binding/coverage 和 hash identity。
- 自动软门证明相对明度、anchor family coherence、材质可分离和可解释 distribution facts，不证明审美完成。
- whole-model 平均 luminance 只能作为诊断；Ground/Air/LargeSurface 的 pass/fail 必须比较同一 painted family 的
  对应局部 geometry regions。
- game-scale silhouette、长平面节奏、座舱/开口识别、视觉噪声和最终材质感必须在 ordinary/contrast 实际预览中
  人工检查；没有截图/人工证据时状态仍可为技术 `ReviewReady`，但报告必须显示 `VisualAcceptance: Pending`。

### 8.6 人工审阅门

- `ReviewReady` 可使用现有“固化最终候选”。
- `NeedsReview` 默认禁止固化；用户勾选“我已审阅质量警告”后，本次 generation 才可固化。
- acknowledgement 只属于当前 session/generation，不写 sidecar，不自动应用到后续候选。
- unit-class evidence/confirmation/Skill、base colour、template、style override、working geometry、palette、semantic
  composition、binding 或 candidate 改变时，
  acknowledgement 立即清空。
- `Blocked` 不显示可用确认入口，也不能固化或导出。

这只能保证数据正确、palette 合法、语义覆盖和基础可读性；游戏内灯光、normal、shadow、HVA、炮塔轴心和最终美术
质量仍需后续 VXL/HVA 与游戏实测，4E 不得宣称 GameReady。

## 9. 精确 UI 契约

### 9.1 Inventory

复用现有中央 `Ra2VoxelStyleWorkspaceView`：左侧“风格”页已经包含语义卡、风格继承、本次要求和编译按钮；
底部“着色”详情页已有颜色角色、区域规则和 palette contrast；顶部已有 Result/Contrast/Region/Palette 预览。

不新增窗口、Dock、Shell 菜单或工具栏。

### 9.2 风格页上色输入卡

在“风格继承”卡之前新增一个同级 `VoxelInspectorSectionStyle` 卡：

- 标题：`上色规则与技法`；
- 第一组为判型：按钮“DeepSeek 判断单位类型”，AutomationId `VoxelStyle.UnitClass.Analyze`；只在用户点击后命中
  classification cache 或调用 Provider，不得在载入模型时静默付费调用；
- 状态 `VoxelStyle.UnitClass.Status` 显示 NotAnalyzed/Analyzing/ProposalReady/Confirmed/ManualFallback/Failed；证据区
  `VoxelStyle.UnitClass.Evidence` 显示 proposed class、confidence、bounded evidence 和 uncertainty；
- `ComboBox` `VoxelStyle.UnitClass.Selector` 初始选择 proposal，允许用户改为 Ground/Air/LargeSurface/Unknown；按钮
  `VoxelStyle.UnitClass.Confirm` 生成确认或 override。只有判型因 Provider unavailable/timeout 明确失败后，才显示
  `ManualWithoutAiAssessment` 手工 fallback；正常未判型或用户取消状态不能绕过判型；
- `VoxelStyle.UnitClass.Skill` 只读显示确认后将使用的唯一 colouring Skill ID/revision，并明确“DeepSeek 提案，Host
  确定性着色”；
- 第二组 `ComboBox`：列出 active palette 中按 index 排序的全部 opaque、non-remap entries，每项显示色块、
  `#index` 和 RGB hex，AutomationId `VoxelStyle.BaseColour.Selector`；不得提供任意 RGB 输入或自动默认项；
- 选中项旁显示至少 24×24 DIP 色块且带边框，AutomationId `VoxelStyle.BaseColour.Swatch`；文本状态显示 palette
  profile/hash short form、index、RGB 和“主体基准色由人工锁定”，AutomationId `VoxelStyle.BaseColour.Status`；
- `ComboBox`：绑定只读 technique list 与 session `SelectedTechnique`，AutomationId
  `VoxelStyle.Template.Selector`；
- 下方只读说明：显示用途、revision、相对明暗/边缘/材质策略，并明确“不改变颜色主题”，AutomationId
  `VoxelStyle.Template.Description`；
- class confirmation、基准色或模板改变后标记 style preview pending，清除旧 raw/normalized plan（按 cache 规则）、
  candidate/freeze/acknowledgement，保留 geometry 和 semantic editing state；只有 class/Skill 改变可能造成 style
  Provider cache miss，base/technique 不得触发模型；
- 新模型 identity 清空 proposal/confirmation/base；palette hash 改变只清空 base；EvidenceHash 改变使 class
  proposal/confirmation stale，同一 EvidenceHash 可复用；
- classification busy 时判型/确认/class selector disabled；style compile busy 时全部上色输入 disabled；没有有效
  ConfirmedUnitClass、合法基准色或 technique 时，“编译着色预览”disabled 并显示本地原因。

### 9.3 着色详情质量区

在现有“着色”详情页的 role/rule 区下方增加：

- 状态文本 `VoxelStyle.ColourQuality.Status`；
- 指标列表 `VoxelStyle.ColourQuality.Metrics`；
- 警告列表 `VoxelStyle.ColourQuality.Warnings`；
- 仅 `NeedsReview` 可见/可用的 CheckBox
  `VoxelStyle.ColourQuality.AcceptWarnings`，文本为“我已审阅以上质量警告，允许固化本候选”。

不得用颜色 alone 表示状态；状态必须有中文文本。现有 Roles、Rules、PaletteContrast、Result/Contrast、
AcceptSession 和 ExportVox 的 AutomationId 与语义保持不变。

### 9.4 可访问性与布局

- 判型/确认按钮、三个 selector、palette item 和 warning checkbox 支持键盘焦点、方向键选择和合理 Tab 顺序；
  confidence/class/Skill/基准色不得只用颜色或图标表达，必须同时提供中文文本和 palette index/RGB；
- 文本允许换行，不以固定高度截断模板说明或警告；
- 100%/125% DPI 下不得遮挡既有编译、固化和导出按钮；
- 不改变 inspector 宽度、splitter、viewport、camera 或顶部 preview toolbar。

## 10. 生命周期与失效

classification 与 style compilation 是两个独立 Provider 阶段，表中 `Provider call` 分别说明；不得用 style cache
命中掩盖 classification cache miss，反之亦然。

| 变化 | Class proposal/confirmation | Style/binding plan | Masks/quality | Frozen candidate | Provider call |
|---|---|---|---|---|---|
| 同材质集合内修改 cell 边界，EvidenceHash 不变 | 保留 | 可复用 | 本地重建 | 失效 | No |
| 新增/删除 MaterialRole，SemanticFactsHash 改变 | 失效 | 失效 | 失效 | 失效 | classification/style 各按 cache 决定 |
| approved remap presence 改变，class evidence 不变 | 保留 | 失效 | 失效 | 失效 | style cache miss 时 Yes |
| EvidenceHash 改变 | proposal/confirmation 失效 | 失效 | 失效 | 失效 | classification cache miss 时 Yes；确认后 style 独立判定 |
| 用户纠正 class 或 colouring Skill identity 改变 | 生成新 confirmation/原 confirmation 失效 | 失效 | 失效 | 失效 | style cache miss 时 Yes；不重跑 classification |
| BaseColour/Technique 改变 | 保留 | raw plan/binding 可复用；normalized plan 失效 | 失效 | 失效 | No |
| 项目 style/override 改变 | 保留 | 失效 | 失效 | 失效 | style cache miss 时 Yes |
| Palette 改变 | class 可保留；清空 BaseColour | 全部失效 | 全部失效 | 失效 | style cache miss 时 Yes |
| 新模型 identity | 清空 proposal/confirmation | 全部失效并清空 BaseColour | 全部失效 | 失效 | classification/style 各按 cache 决定 |
| 仅预览模式/相机改变 | 保留 | 保留 | 保留 | 保留 | No |

取消、超时或过期 completion 不得清除先前仍有效的预览，但必须保持 pending 输入与旧结果身份可见，不得把旧结果
标为当前模板/语义的结果。

## 11. Typed failure 与本地化

实现时新增或扩展 internal failure kind，至少区分：

```text
TemplateUnavailable
TemplateInvalid
UnitClassEvidenceInvalid
UnitClassAssessmentRequired
UnitClassProposalInvalid
UnitClassConfirmationRequired
UnitClassConfirmationStale
ClassifierSkillUnavailable
ColourSkillUnavailable
ColourSkillMismatch
MultipleClassSkillsSelected
BaseColourRequired
BaseColourInvalid
BaseColourPaletteMismatch
UnitAdaptationInvalid
SemanticRequirementsInvalid
SemanticBindingMissing
SemanticBindingDuplicate
SemanticBindingIncompatible
RequirementShapeMismatch
CompositionMismatch
RemapUnavailable
PaletteFamilyUnavailable
PolicyIdentityMismatch
QualityBlocked
CacheCorrupt
StaleGeneration
Cancelled
```

Provider unavailable/timeout/malformed 继续沿用现有 compiler failure。失败消息必须指出失败层和安全恢复动作，不能建议
用户绕过 hash、关闭 validator 或直接写 palette index。

## 12. 自动化测试矩阵

### 12.1 Technique catalog

- 五个 stable ID/revision 唯一、policy hash 稳定；
- classifier Skill 及 Ground/Air/LargeSurface/generic colouring Skill 的 ID/revision/content hash 唯一且稳定；
- 四个 ConfirmedUnitClass 唯一映射四个 UnitAdaptation policy 和恰好一个 colouring Skill；Air underside 不要求固定负方向；
- 每个技法参数、规则全集、fallback 和质量阈值通过本地 validator；
- 人工 BaseColour 对 opaque/non-remap 成功，transparent/remap/wrong-palette/缺失选择分别 fail closed；
- 相同 colour intent/palette/base/requirements/technique/adaptation 输出相同 normalized plan、bundle 和 candidate hash；
- BodyBase 始终等于人工 index，derived roles 满足稳定 family selector/tie-break；
- 切换 base/technique 改变本地规则/明暗而不改变 raw style source、semantic masks 或 Provider call count；
- project/directory/request source precedence 与 scope provenance 保持 1E 行为。

### 12.2 判型、Skill 路由与 cache

- Ground/Air/LargeSurface/Unknown 的 proposal schema、evidence fact 引用、置信度和 malformed/fabricated evidence fail closed；
- classification cache 的 model/evidence/classifier Skill/provider/schema identity 任一变化均安全 miss；
- proposal 不能直接路由；未经人工确认禁止 style compilation；人工 correction 产生新 confirmation hash；
- Provider unavailable 的手工 fallback 明确标记 `ManualWithoutAiAssessment` 并强制 NeedsReview；
- 每个 confirmed class 只装载对应的一个 colouring Skill；wrong/missing/multiple Skill 均 fail closed；
- classification 与 style compilation 在双 cache miss 时各最多一次调用，状态、失败和调用计数分别披露；
- class correction 不重复调用 classification，但会使 style cache/bundle/candidate/freeze/ack 失效。

### 12.3 Requirements、binding 与 style cache

- 所有 MaterialRole 和 approved remap 的 success/failure 矩阵；
- Light/Accent 两个同类别 role 使用显式 binding，无 `First()` 依赖；
- 同材质集合改变 cell count：compiler/cache 可复用、mask/candidate hash 改变；
- 材质集合/remap presence 改变：Provider style cache miss；
- technique/base/policy revision 改变：Provider raw style cache hit 可复用，但 materialization bundle 必须失效重建；
- confirmed class/colouring Skill revision 改变：Provider style cache miss；classification proposal 可继续复用；
- v1 cache、corrupt cache、wrong palette/requirements 安全 miss；wrong class/Skill/technique/base/policy bundle 拒绝发布。

### 12.4 Materialization correctness

- known cell 100% 可解释覆盖、Unknown 保留以人工基准色为锚点的 base geometry style；
- PaintedSurface 的 Top/Side/Under 分层不被后置 BodyBase mask 涂平；
- 单层/薄面 Top+Under cell 按四种 adaptation 的 DualSurfacePolicy 稳定裁决，不依赖规则碰巧覆盖；
- `ApprovedRemap > DirectSemanticMaterial > BodyGeometryFamily` precedence 与 expected applied count 一致；
- geometry/occupancy/order/part identity 不变；
- transparent/remap/hash/shape mismatch fail closed；
- ordinary/contrast 均保留 semantic/remap/精确 palette roles。

### 12.5 Quality admission

- Blocked、NeedsReview、ReviewReady 三态逐项测试；
- 五个 technique × 四个 adaptation 的代表性 readability fixtures；Ground 检查方向，Air 检查双向 underside
  差异，Unknown 强制 NeedsReview；
- 中间、极亮、极暗基准色及稀疏 palette family fallback fixtures；
- DirectRole 跨多个 primary regions 时产生 flat-material warning，不把单色 remap/material 伪装成分层完成；
- contrast candidate 可独立升级为 ReviewReady；
- contrast 不改变人工 BodyBase、semantic direct role、remap 或其它 exact selection；
- warning acknowledgement 只解锁当前 generation，任何输入变化立即清除；
- 无单一 opaque score，报告包含全部规定 facts。

### 12.6 UI 与 export

- 新旧 AutomationIds、判型/证据/确认控件和三个 selector 的键盘可达性、文本/色块双重表达及 enabled 规则；
- 判型只由显式按钮触发；class selector/确认本身不调用 Provider；BaseColour/Technique 不调用 Provider；
- 未确认 class、Skill identity mismatch 或缺失人工基准色/technique 时禁止 style compilation；
- NeedsReview 未确认不能固化，Blocked 不能固化/导出；
- 固化候选经现有 `.vox` export 回读后 palette indices、geometry 和 candidate hash 一致；
- XAML/Shell layout boundary tests 保持通过。

## 13. 人工验收

实现完成后至少使用一个地面、一个空中、一个大型水面真实模型，覆盖 VXLSE III RA2 `unittem.pal` 与至少一个
带 embedded palette 的 VOX，并使用至少三个技法模板验收。用户提供 ZIP 只作为本地验收输入，不提交仓库：

1. 每个模型先由 DeepSeek 给出 class、confidence 和证据，人工确认或纠正；确认后 UI 只显示并装载对应专用 Skill；
2. 每个模型由人工选择合法基准色；BodyBase 在普通/对比度/固化/导出回读后始终保持同一 palette index；
3. 同一基准色切换三种技法，只改变规则/相对明暗，不把颜色主题改成模板预设；
4. Ground/Air/LargeSurface 分别应用对应 Skill/policy；A10 类样本允许 underside 更亮但必须可区分；
5. 人工把一个 proposal 改为另一 class，验证旧 Skill/style/candidate/freeze 全失效且不混用两类 Skill；
6. 使用中间、近最亮和近最暗基准色验证 family fallback、NeedsReview/Blocked 和无跨色带静默跳转；
7. project style override 路径显示完整来源，base/technique 切换不增加模型轮次；
8. Glass/Rubber/BareMetal/Light/Accent 至少覆盖三种材质，检查边界无串色；PaintedSurface 保留几何分层；
9. 同一 EvidenceHash/材质集合修改笔划只本地重着色，不再次调用模型；
10. ordinary/contrast 并排审阅，低对比普通候选正确进入 NeedsReview，contrast 不移动人工基准色；
11. warning acknowledgement 在任何输入变化后重置；
12. 固化并导出 `.vox`，重新载入后视觉、基准色和全部 palette indices 一致；
13. 100%/125% DPI 下判型、证据、确认、三个 selector、质量区和现有按钮均可用。

真实 DeepSeek 调用不是自动门禁，只有用户明确授权才运行。VXL/HVA 和游戏内验收不属于 4E。

## 14. 阶段计划与停止门

| 阶段 | 目标 | 停止门 |
|---|---|---|
| 4E-1 | UnitClass evidence/proposal/confirmation + BaseColour/Technique/policy + requirements/binding models + specialist Skill packages | Application focused tests |
| 4E-2 | classification/cache + exact Skill router + Provider style compiler/cache v2 + normalization identity | IDE classifier/compiler/cache focused tests |
| 4E-3 | Base-centred deterministic integration + policy-aware contrast/quality evaluator | correctness/quality focused tests |
| 4E-4 | Exact approved classification/confirmation/base/technique UI + selected Skill/quality projection | XAML compile + UI contract tests + screenshot/manual request |
| 4E-5 | Full verification, package, physical acceptance | all mandatory gates and user smoke |

任一阶段发现需要修改 4D sidecar、Shell、public API、project Save 或 VOX/VXL writer，立即停止并请求新合同。
不得通过放宽测试、吞掉 binding 错误或把 NeedsReview 当 ReviewReady 来继续。

## 15. Rev.3 全面自审

自审范围：从 UI/session input → Provider cache → local normalization → semantic materialization → quality admission →
freeze/export/invalidation 做正向和反向走查，并与现有 style contracts、colourizer rule order、contrast optimizer、
4E-R1 ground/air evidence 和 4D authority boundary 对照。4E-1 行仅代表 internal contracts/catalogs/Skills 已验证；
不代表 4E-2..4E-5 运行时、UI 或物理视觉已经实现或验证。

| 审查项 | 结论 | Rev.3 证据/裁决 |
|---|---|---|
| 权威唯一性 | Pass | DeepSeek 只提案，人工 ConfirmedUnitClass 决定路由；人工 BaseColour 拥有 BodyBase；typed policy 是数值规则权威 |
| Palette 合法性 | Pass | 只能选择 active palette opaque/non-remap index；hash/RGBA 回读绑定；无任意 RGB 量化入口 |
| “以基准色为中心” | Pass | BodyBase exact 锁定；body roles 使用共享 OKLab anchor family；禁止为亮度跨 family |
| Ground/Air/LargeSurface | Pass | evidence-bound proposal → 人工确认/纠正 → 单一专用 Skill/policy；Air underside 双向可区分；Unknown 强制 NeedsReview |
| 薄面体素冲突 | Pass | Top+Under 用 adaptation-owned DualSurfacePolicy，不依赖旧覆盖顺序 |
| PaintedSurface 层次 | Pass | BodyGeometryFamily 保留 Top/Side/Under；不生成 flattening BodyBase direct mask |
| Semantic/remap precedence | Pass | ApprovedRemap > DirectSemanticMaterial > BodyGeometryFamily；重叠 count 单独核对 |
| Light/Accent 歧义 | Pass | 同时出现必须是不同 roleId；binding compatibility table 已冻结 |
| Provider 成本与 cache | Pass | classification/style 是两个独立 cache/状态；双 miss 最多两次调用；base/technique 切换为零模型调用 |
| Skill/compiler 漂移 | Pass | 判型与 colouring Skill 职责分离；style prompt 恰好一个 class Skill；typed catalog 唯一保存 runtime 数值硬门 |
| Contrast 可靠性 | Contract pass / runtime NotImplemented | allowlist 已加入现有 optimizer；必须复用 policy/family selector且不能移动 BodyBase |
| 质量状态诚实性 | Pass | 无 opaque 总分；ReviewReady 仅技术可固化；VisualAcceptance 单独 Pending/人工确认 |
| UI 可执行性 | Contract pass / physical NotRun | 显式判型、证据、人工确认/纠正、selected Skill、三个 selector 和 AutomationIds 已精确定义 |
| Persistence/Shell/writer | Pass | session-only；不写 4D sidecar；不改 Shell、项目 Save、VOX/VXL/HVA writer |
| 自动/人工验证 | 4E-1 focused Pass / 4E-2..5 Pending | contract/catalog/Skill tests 已通过；真实 ground/air/large-surface 验收仍未运行 |

Rev.3 保留了 Rev.2 对固定 air underside 方向、contrast allowlist/全局阈值冲突、template-only 无颜色来源、
Provider cache 与本地候选 identity 混用及 PaintedSurface/Top+Under 覆盖缺陷的修正，并进一步消除了四类风险：
模型判型直接成为权威、Ground/Air 规则混装、人工 class 与 adaptation 双选择冲突，以及把判型与 style 编译伪装成
一次不透明模型调用。

### 15.1 已知剩余风险

- `oklab-anchor-v1` 的初始 family 阈值已冻结但尚未在所有 PAL/VOX palette 上实测；4E-3 若样本证明失真，
  必须回到 Contract amendment，不得在代码中静默调参。
- v1 DirectRole 材质/remap 仍是单 index；跨多个 primary regions 会明确 NeedsReview，但多级材质/remap family
  仍需后续合同。
- LargeSurface v1 没有 Deck/Hull 独立 MaterialRole；它只能改 geometry policy 和报告限制。
- DeepSeek 只能看有界几何/语义事实而不是三维视觉本身，判型仍可能错误；人工确认是必选安全门，不能省略。
- 双 cache 都 miss 时最多需要两次 DeepSeek 调用，成本和延迟高于 Rev.2；实现必须分别披露、可取消且不得静默重试。
- 三个专用 colouring Skill 和 classifier Skill 已创建并通过 bundled catalog/内容契约测试，但尚未经过真实 DeepSeek
  与真实 ground/air/large-surface 模型的行为回归。
- v1 只有一个全模型基准色；多部件独立涂装、VXL/HVA、normal、游戏光照和 GameReady 均未解决。
- 4E-1 internal contracts/Skill diff、聚焦测试和 Debug build 已有证据；classification/style Provider、实际
  materialization、WPF 截图、真实 DeepSeek 和游戏实测仍无证据，必须在 4E-2 至 4E-5 分阶段产生。

### 15.2 自审结论

```text
Contract architecture: Approved
Runtime implementation: Partial (4E-1 contracts/catalogs/Skills only)
Automated verification: FocusedPassed (see ASSET-VOX-4E_StageLedger.md)
Physical visual acceptance: NotRun
GameReady: OutOfScope
```

## 16. 批准门

本 Rev.3 已批准，4E-1 已完成。后续门禁：

- 4E-2 只能接入 classification/cache、exact Skill router 和 style compiler/cache v2；
- 4E-3 只能在 4E-2 验证通过后接入本地 materialization/quality；
- 4E-4 必须严格按第 9 节 UI 契约实现，并请求截图/人工验收；
- 4E-5 才执行全量验证、clean package 和真实模型物理验收。
