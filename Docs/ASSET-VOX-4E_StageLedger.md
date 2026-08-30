# ASSET-VOX-4E — Stage Result Ledger

日期：2026-08-31
状态：4E-1..4E-2 Completed / focused automated verified；4E-3..4E-5 NotStarted
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

## Stage Result Ledger

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| 4E-1 | contracts/catalogs/Skills/requirements/binding | Application internal contracts、Skill/Technique content、focused tests、governance docs | 13/13 new contract tests；45/45 affected Application tests；18/18 Skill catalog tests；88/88 affected IDE tests；Debug build 1 existing warning / 0 error | Completed | Yes：4E-2 classifier/cache/router/compiler integration |
| 4E-2 | classification/cache + exact Skill router + style compiler/cache v2 | IDE classifier/cache/router、existing compiler partial v2、focused IDE tests、governance docs | 26/26 classifier/router/compiler/cache focused；49/49 affected Application；107/107 affected IDE；final Debug build 0 warning / 0 error | Completed | Yes：4E-3 deterministic base-centred materialization/quality |
| 4E-3 | deterministic materialization + contrast/quality | NotTouched | NotRun | NotStarted | No |
| 4E-4 | approved UI contract | NotTouched | NotRun | NotStarted | No |
| 4E-5 | full verification/package/physical acceptance | NotTouched | NotRun | NotStarted | No |

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
| Skill Creator helper | Failed (optional environment check) | `quick_validate.py` could not start because both available Python runtimes lack PyYAML；no dependency was installed；authoritative project bundled parser/tests passed |
| Full Application/IDE/AssetHost suites | NotRun | 4E-1 gate requires focused contracts; full suites belong to 4E-5 |
| Clean package | NotRun | deferred to 4E-5 |
| Real DeepSeek / WPF / model visual | NotRun | 4E-1 contains no Provider/UI/materialization integration |

## Boundary audit

- public .NET API：0；全部新类型保持 internal。
- persistence/schema：0；未修改 4D sidecar。
- Provider/AssetHost protocol：0；未进行真实模型调用。
- Shell/XAML/project Save/writer：0。
- existing colourizer/semantic composer：0；4E-2 只把 existing style compiler 声明为 partial 并新增 v2 入口；v1 当前工作区路径、colourizer、semantic
  composer 和 XAML 均未接线或改变。
- legacy：未恢复。

## Deferred Governance Queue

### Public API / decision

- 4E-1 zero-change confirmation 已写入 `Docs/PublicApiLedger.md`。
- 4E-2 zero-change confirmation 已写入 `Docs/PublicApiLedger.md`；既有 Rev.3 exact-route 决策已更新实现证据，
  未新增或改变架构方向。

### Technical debt

- 无新增临时兼容层、TODO 或平行实现。

### Remaining stages

- 4E-2 已提供两个独立、可取消、可观察的 classification/style cache 结果与 exact single-Skill routing；4E-4 接线前
  它们尚未由现有 ViewModel/UI 调用。
- 4E-3 才可接入 base-centred palette family、materialization、contrast 和 quality admission；不得提前改 XAML。
- 4E-4 需截图/人工验证；4E-5 需全量测试、clean package 与真实样本验收。
