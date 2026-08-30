# ASSET-VOX-4E — Stage Result Ledger

日期：2026-08-31
状态：4E-1 Completed / focused automated verified；4E-2..4E-5 NotStarted
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

## Stage Result Ledger

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| 4E-1 | contracts/catalogs/Skills/requirements/binding | Application internal contracts、Skill/Technique content、focused tests、governance docs | 13/13 new contract tests；45/45 affected Application tests；18/18 Skill catalog tests；88/88 affected IDE tests；Debug build 1 existing warning / 0 error | Completed | Yes：4E-2 classifier/cache/router/compiler integration |
| 4E-2 | classification/cache + exact Skill router + style compiler/cache v2 | NotTouched | NotRun | NotStarted | No |
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
| Skill Creator helper | Failed (optional environment check) | `quick_validate.py` could not start because both available Python runtimes lack PyYAML；no dependency was installed；authoritative project bundled parser/tests passed |
| Full Application/IDE/AssetHost suites | NotRun | 4E-1 gate requires focused contracts; full suites belong to 4E-5 |
| Clean package | NotRun | deferred to 4E-5 |
| Real DeepSeek / WPF / model visual | NotRun | 4E-1 contains no Provider/UI/materialization integration |

## Boundary audit

- public .NET API：0；全部新类型保持 internal。
- persistence/schema：0；未修改 4D sidecar。
- Provider/AssetHost protocol：0；未进行真实模型调用。
- Shell/XAML/project Save/writer：0。
- existing style compiler/colourizer/semantic composer：0；只复用其 internal types 和 hash/palette contracts。
- legacy：未恢复。

## Deferred Governance Queue

### Public API

- 4E-1 zero-change confirmation 已写入 `Docs/PublicApiLedger.md`。

### Technical debt

- 无新增临时兼容层、TODO 或平行实现。

### Remaining stages

- 4E-2 必须实现两个独立、可取消、可观察的 classification/style cache 阶段，并验证 exact single-Skill routing。
- 4E-3 才可接入 base-centred palette family、materialization、contrast 和 quality admission。
- 4E-4 需截图/人工验证；4E-5 需全量测试、clean package 与真实样本验收。
