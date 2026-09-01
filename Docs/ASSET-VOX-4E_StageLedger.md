# ASSET-VOX-4E — Stage Result Ledger

日期：2026-08-31
状态：4E-1..4E-5 / Rev.7-B..G automated completed；VisualAcceptancePending
批准契约：`Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md` Rev.3

## 4E-1 — Internal contracts, policies and Skill packages

- 新增 session/derived-only 的 `UnitClassEvidence`、validated `UnitClassProposal`、`ConfirmedUnitClass` 与稳定 hash。
- Evidence 只向模型投影有界 geometry/semantic/orientation facts；不暴露坐标、palette 主题或写入能力。
- 人工确认、人工纠正和 Provider 失败后的手工 fallback 使用不同 confirmation source/hash；proposal 不能直接路由。
- 新增人工 BaseColour validator：只接受 active palette 的 opaque、non-remap index，并保存 exact index/hash。
- 新增 5 个版本化 typed technique policies 和 4 个 class-derived adaptation policies；Unknown 强制 NeedsReview。
- 新增 semantic colour requirements/shape hash 和 exact binding validator；PaintedSurface 要求完整 body geometry family，
  Light/Accent 必须使用不同 roleId，approved remap 仍是独立最终覆盖 requirement。
- 新增 classifier、Ground、Air、LargeSurface 四个 BuiltIn Skill；通用 Skill 仅作为 Unknown 的 4E fallback。
- 新增 5 个 `TECHNIQUE.md`，数值权威仍只属于 Application typed catalog。

## 4E-1 权威与生命周期

| 数据 | Owner | Lifetime | Serialized |
|---|---|---|---|
| UnitClassEvidence / Proposal / Confirmation | Application internal contract；IDE Host 将在 4E-2 持有 session state | 当前 working model/evidence | No |
| BaseColourSelection | 人工 session input | 当前 active palette/model | No |
| Technique / UnitAdaptation policies | Application immutable catalog | Process/content revision | No |
| Semantic requirements / binding plan | Application derived contract | 当前 composition/style plan | No |
| Skill/Technique Markdown | IDE bundled content | Build/content revision | Content only；不写项目状态 |

## 4E-2 — Classification/cache, exact Skill route and style cache v2

- 新增独立 `Ra2VoxelUnitClassClassifier`：只消费 4E-1 bounded evidence，使用 required structured tool，输出经
  Application validator 验证的 proposal；cache miss 最多一次调用，hit 为零调用。
- classification cache 精确绑定 model/evidence/classifier Skill/provider/schema identity；cache 是本机可丢弃派生
  数据，不进入项目或 4D sidecar。
- 新增 Host exact router：API 只接受 `ConfirmedUnitClass`，四个 enum 值恰好映射一个 colouring Skill 和一个 typed
  adaptation；stale/missing/oversized/invalid identity 全部 fail closed。
- 在现有 `Ra2VoxelStyleCompiler` 上新增 v2 入口，而非第二套 compiler；prompt 只拼接一个 class-specific Skill，并
  增加 bounded `semantic_bindings` structured output 与本地 binding validator。
- style cache schema/key v2 加入 RequirementShape、BindingSchema、confirmed class 和 colouring Skill identity；
  evidence/confirmation 改变但 class/Skill 不变时 raw plan 可复用，normalization identity 仍随当前 evidence/
  confirmation 改变。
- normalization input identity 已包含 raw plan、binding、evidence/confirmation、classifier/colour Skill、adaptation、
  requirement shape 和 binding schema；不含 BaseColour/Technique，也不执行 palette materialization。
- 旧 v1 compiler 入口暂留作 4E-4 接线前兼容路径；v2 对 v1/corrupt envelope 安全 miss，不迁移、不删除。

## 4E-3 — Deterministic base-centred materialization and quality

- 在既有 compiler/colourizer/contrast 路径上增加共享 OKLab family selector；`BodyBase` 始终等于人工 exact palette
  index，派生 BodyLight/Mid/Dark/Under/Edge 只从 active palette 的 opaque、non-remap entries 确定性选择。
- `TechniquePolicy × UnitAdaptationPolicy` 决定相对层次和 thin-cell `DualSurfacePolicy`；Top+Under 先显式决出一个
  primary surface，再应用 edge，避免依赖偶然规则顺序。
- PaintedSurface 保留 geometry family，不使用晚期 BodyBase mask 压平；direct semantic material 后置，approved
  remap 最后覆盖。
- policy-aware contrast 保护 BodyBase、所有 exact role、semantic direct role 与 remap；无法形成合法 family 时
  Warn/Block，不跨色带静默跳转。
- 新增无总分的多维 `Blocked / NeedsReview / ReviewReady` 质量报告；`VisualAcceptance` 独立保持 Pending，review
  package 绑定 candidate hash、指标、警告和分布事实。

## 4E-4 — Approved workspace UI wiring

> Rev.4 amendment: the automatic classification UI and active classifier dependency below are historical Rev.3 behavior.
> Current behavior is manual unit-class selection plus the five-stage task workspace defined in
> `Docs/ASSET-VOX-4E_ManualUnitClassAndWorkspaceUiAmendmentRev4.md`.

- 既有 Voxel Style workspace 新增显式“AI 判断单位类型 → 人工确认/纠正 → 唯一 Skill”路径；未确认 class 禁止 style
  compilation，Provider unavailable/timeout 才开放“未经过 AI 评估”的人工 fallback。
- 新增 active RA2 palette 的 opaque/non-remap 基准色 selector、真实 swatch/status，以及五个规则/技法 selector；
  base/technique 改变只在本地使候选失效，不触发新的分类调用。
- `CompileAsync` 接入 v2 compiler/materializer/review package；质量区域显示状态、指标和警告，NeedsReview 必须对当前
  generation 显式确认后才允许固化。
- UI 严格使用批准的 AutomationIds；未修改 Shell，全局布局和现有语义编辑/持久化路径保持不变。

### 4E-4 UnitClass real-provider compatibility fix

- 修复真实判型提案的 tool arguments 被字符串/包装层编码，或精确五字段中的 enum 使用 `Ground/High` 大小写、
  reason 含换行时被统一误报 malformed 的问题。
- 只规范化 enum token 大小写、hash 大小写等价性和 reason 空白；未知 enum、额外字段、伪造/重复 FactId、越界
  内容、stale evidence 仍 fail closed，不增加重试或调用次数。
- 字段值仍非法时只报告 bounded field 名，不回显 Provider 原文。

## Stage Result Ledger

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| 4E-1 | contracts/catalogs/Skills/requirements/binding | Application internal contracts、Skill/Technique content、focused tests、governance docs | 13/13 new contract tests；45/45 affected Application tests；18/18 Skill catalog tests；88/88 affected IDE tests；Debug build 1 existing warning / 0 error | Completed | Yes：4E-2 classifier/cache/router/compiler integration |
| 4E-2 | classification/cache + exact Skill router + style compiler/cache v2 | IDE classifier/cache/router、existing compiler partial v2、focused IDE tests、governance docs | 26/26 classifier/router/compiler/cache focused；49/49 affected Application；107/107 affected IDE；final Debug build 0 warning / 0 error | Completed | Yes：4E-3 deterministic base-centred materialization/quality |
| 4E-3 | deterministic materialization + contrast/quality | Application family/materializer/quality、existing colourizer/contrast/review package、35 tests | 35/35 new；77/77 affected Application；89/89 affected IDE；Debug build passed | Completed | Yes：4E-4 approved UI contract |
| 4E-4 | approved UI contract | existing coordinator/ViewModel/workspace XAML/code-behind、UI/ViewModel tests | IDE project XAML build 0 warning/0 error；workspace UI/ViewModel 25/25 | Completed / physical visual Pending | Yes：4E-5 automated verification may run；physical acceptance remains explicit |
| 4E-5 | full verification/package/physical acceptance | WPF visual-resource test lifecycle、verification、documentation、package | 临时 Application 改用 explicit shutdown；visual-resource 1/1、视觉边界 17/17、Application 368/368、AssetHost 50/50、IDE 2922/2922；Debug build 0 warning/error；clean package 1470 entries | Automated completed / physical visual pending | Yes：转交用户完成真实模型与 DPI 验收 |
| 4E-UI-R1 Rev.4 | manual unit class + five-stage workspace | Application internal identity、IDE router/compiler/coordinator/ViewModel/XAML、focused tests/docs | Release build passed；Application 39/39；IDE 39/39；physical UI pending | Implemented / focused verified / physical pending | No：wait for user screenshot/manual result before final package |
| 4E-UI-R1-FIX1 | unit-class/base-colour selector visible labels | workspace XAML、UI/ViewModel regression tests、status docs | Isolated Release XAML build 0 warning/0 error；UI/ViewModel 26/26 | Completed / screenshot recheck pending | No：user must rebuild/restart and confirm labels visibly render |
| 4E-Rev5 / UI-R1-FIX2 | indexed ramp、BodyBase 可见性、edge 收敛、可见表面覆盖与失效预览回退 | Application family/materializer/colourizer/quality/semantic coverage、workspace ViewModel、direct tests/docs | Application 353/353；colour focused 45/45；workspace ViewModel 25/25；Debug solution build 0 warning/0 error；IDE full 2919/2920，唯一既有 WPF resource test 单独复跑 1/1 Passed | Implemented / automated focused verified / physical visual pending | No：等待用户真实模型视觉验收；full-suite isolation gate仍未满足 |
| 4E-Rev6 | directional surface、effective semantic boundary、five-technique differentiation、全局分类预览入口 | Application geometry/materializer/quality/Skills、workspace XAML/ViewModel、tests/docs | Debug build passed；Application 358/358；AssetHost 50/50；IDE 2920/2920；VoxelColour 55/55；VoxelStyle workspace 28/28；Skill catalog 18/18；final XAML wrap 后 IDE build + UI contract 3/3 | Implemented / automated verified / physical visual pending | No：等待用户真实模型视觉验收 |
| 4E-Rev7-A | form-zone / boundary-intent / feature-scale / game-scale code fact audit and exact contract | Rev.7 audit/contract、ledger/status/index docs only | docs link/structure/diff audit；build/test NotRun（docs-only） | Completed / exact contract approval pending | No：用户批准精确契约后进入 Rev.7-B；不得提前修改 runtime/XAML |

## Verification Matrix

| Step | Status | Evidence |
|---|---|---|
| Restore | Passed | `dotnet restore .\RA2IniEditor.IDE.sln`；exit 0 |
| Build / Compile | Passed | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`；1 existing CS8602 warning at `BuiltInFieldRegistryPackLoaderTests.cs:1983` / 0 error；warning source不在本次 diff |
| New Contract Tests | Passed | Application UnitClass/Technique/SemanticColour tests 13/13 |
| Affected Application Tests | Passed | VoxelColour/VoxelStyle/VoxelSemantic 45/45 |
| Skill Catalog Tests | Passed | `Ra2AgentSkillCatalogTests` 18/18 |
| Affected IDE Tests | Passed | VoxelStyle/VoxelSemantic/AgentSkillCatalog 88/88 |
| 4E-2 Focused Tests | Passed | classifier/router/compiler/cache 26/26；fake clients only；classification/style call counts and cancellation separately asserted |
| 4E-2 Affected Application Tests | Passed | VoxelColour/VoxelStyle/VoxelSemantic/VoxelUnitClass 49/49 |
| 4E-2 Affected IDE Tests | Passed | VoxelStyle/VoxelSemantic/AgentSkillCatalog/UnitClass/ColourSkill 107/107 |
| 4E-2 Final Debug Build | Passed | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`；0 warning / 0 error |
| 4E-3 New Materialization Tests | Passed | 35/35；含 Technique×UnitClass 20 组、determinism、anchor、dual surface、precedence、contrast protection、sparse/extreme fallback、stale identity、三种质量状态 |
| 4E-3 Affected Tests | Passed | Application 77/77；IDE 89/89 |
| 4E-4 XAML / UI Contract | Passed (automated) | IDE project build 0 warning/0 error；workspace ViewModel/UI contract 25/25 |
| 4E-4 UnitClass compatibility fix | Passed (Release isolated output) | classifier + workspace ViewModel 30/30；Debug 输出被用户当前运行的 IDE 进程锁定，未关闭用户程序；Release build/test exit 0 |
| 4E-5 Restore / Final Build | Passed | restore exit 0；solution Debug build 0 warning/0 error |
| Full Application suite | Passed | 368/368 |
| Full AssetHost suite | Passed | 50/50 |
| WPF resource lifecycle regression | Passed | failing test 1/1；完整 `IdeVisualSystemBoundaryTests` 17/17；临时 Application 使用 explicit shutdown，并在关闭 Popup/Window 后排空 Dispatcher |
| Full IDE suite | Passed | 2922/2922；原 DeferredAppResource/Popup failure 已消除 |
| Skill Creator helper | Failed (optional environment check) | `quick_validate.py` could not start because both available Python runtimes lack PyYAML；no dependency was installed；authoritative project bundled parser/tests passed |
| Clean package | Passed | `artifacts/RA2IniEditor.IDE.SourceClean.zip`；1470 entries；`.vs/bin/obj/artifacts/TestResults` 与嵌套 zip/7z/rar 违规 0 |
| Real DeepSeek / WPF / model visual | NotRun / Pending | 未获真实付费调用授权；WPF 截图和用户提供 ground/air/large-surface 样本的物理视觉验收待用户执行 |
| 4E Rev.5 colour/surface regression | Passed | Application full 353/353；colour/materialization 45/45；workspace ViewModel 25/25；RA2 indexed-ramp、edge policy、interior Unknown、Side→BodyBase、stale-result→3D 均有直接断言 |
| 4E Rev.6 direction/boundary/technique/UI | Passed | Application 358/358；AssetHost 50/50；IDE 2920/2920；direction masks、side-under protection、effective boundary/direct-material protection、RA2 indexed-ramp 五技法五 candidate hash、全局分类预览均有直接断言 |
| 4E Rev.7-A docs contract | Passed (docs-only) | 新 audit/contract 文件存在；权威路由、阶段门、UI AutomationIds、验证命令和人工验收项已冻结；未修改 runtime |
| 4E Rev.7-B FormZone | Passed | 人工前向 hash-bound；Unknown 不猜 front；FormZone focused 5/5，Application build 0 warning/error |
| 4E Rev.7-C Boundary/Material | Passed | semantic boundary intent、局部同 ramp material family、稀疏区域保护；目标测试 62/62 |
| 4E Rev.7-D Scale/Technique | Passed | Macro/Meso/Micro/SubPixelRisk；五种 revision-3 空间策略产生不同分布；目标测试 65/65 |
| 4E Rev.7-E GameScale/Quality | Passed | 八固定视角、normal/VPL 明示状态、多维质量事实；目标测试 94/94，build 0 warning/error |
| 4E Rev.7-F Skill/UI | Passed | 四个 Skill + 五个 TECHNIQUE revision 3；仅第16节 UI；Application 65/65、IDE/Skill/UI 112/112 |
| 4E Rev.7-G final gate | Passed / automated completed | 诊断并修复测试自有 Application 的 `OnLastWindowClose`/Popup dispatcher 生命周期；restore passed；build 0 warning/error；Application 368/368；AssetHost 50/50；IDE 2922/2922；IdeOnly clean package 1470 entries；VisualAcceptancePending |

## Boundary audit

- public .NET API：0；全部新类型保持 internal。
- persistence/schema：0；未修改 4D sidecar。
- Provider/AssetHost protocol：0；未进行真实模型调用。
- Shell/project Save/writer：0；仅修改批准的 Voxel Style workspace XAML/ViewModel/coordinator。
- existing colourizer/contrast/review package：按 Rev.3 增加 internal overload/quality projection；旧 v1 入口保留。
- semantic composer/4D persistence：0；materializer 只消费当前 session composition，不改变保存格式。
- legacy：未恢复。

## Deferred Governance Queue

### Public API / decision

- 4E-1 zero-change confirmation 已写入 `Docs/PublicApiLedger.md`。
- 4E-2 zero-change confirmation 已写入 `Docs/PublicApiLedger.md`；既有 Rev.3 exact-route 决策已更新实现证据，
  未新增或改变架构方向。
- 4E-3..4E-4 zero-change confirmation 已写入 `Docs/PublicApiLedger.md`；Rev.3 决策实现证据已更新。

### Technical debt

- 无新增临时兼容层、TODO 或平行实现。

### Remaining stages

- 运行真实 WPF 截图/布局检查和合同第 13 节用户样本验收；没有这些证据时 `VisualAcceptance` 必须保持 Pending。
- 真实 DeepSeek classification/style 双调用需要用户明确付费授权；当前全部 Provider 验证只使用 fake clients。
- automated mandatory gates 与 IdeOnly clean package 已通过；4E 仍不包含 VXL/HVA、项目 Apply/Save 或 GameReady。
- Rev.7 的 normal context 只消费 snapshot-matched 既有法线；当前 colouring source 没有此 authority 时保持
  `NotAvailable`。VPL 始终为 `NotEvaluated`，不解析或模拟游戏 VPL。
