# Current working baseline note

Current Field Registry phase: `FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply` completed. Next recommended phase: `FR-DQ-3B-FinalHoverQualityAudit`.

Direct Hover-risk rows after 3A: 0. Direct placeholder rows: 0. Exact `整数型字段`: 0. Exact `数值型字段`: 0. Unresolved guardrail rows are tracked in `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.

Do not modify provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy behavior unless explicitly requested.

# AGENTS.md — RA2IniEditor.IDE

## Project Identity

This repository is **RA2IniEditor.IDE-only**.

The current product is an INI-focused IDE for Red Alert 2 / Yuri's Revenge / Ares / Phobos mod files.

It is **not** the removed legacy table-style editor.

## Active Build Entry

Use the IDE-only solution:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Do not restore, create, or use:

```text
RA2IniEditor.sln
RA2IniEditor.csproj
legacy MainWindow
legacy table-style editor
legacy object workbench
old Key-Value DataGrid editor
old Country / Side manager
old object copy / weapon-chain copy workflows
```

## Current Baseline

Current accepted baseline update:

```text
FR-DQ-3H-LightweightHoverTrustAndDiagnosticPolish completed: field registry quality is preserved as metadata, Hover stays lightweight with risk-only footnotes, Quick Peek shows trust details, and diagnostics now classify wrong-context / obsolete / non-existent / pseudo-field risks. dotnet test was not run because dotnet CLI is unavailable in patch environment.
```


Current accepted baseline:

```text
v0.4.96-pre.2 IDE-only Source Package Stabilization
A15 / AI / Icon polish lines completed through the documented stages in Docs/RA2IniEditor_IDE_Full_Codex_Context.md
FR-DQ-2B-Apply completed: Batch A canonical combat rows applied
FR-DQ-2C-Verify-ManualApply completed: Batch B BuildCat / Crewed / Turret / ThreatPosed verified and applied
FR-DQ-2F-AI-LowQuality-ManualApply completed: direct `数值型字段` Hover rows removed from BuiltIn v3.2
FR-DQ-2F-AI-CrossContext-ManualApply completed: Owner / Prerequisite / Sight AI placeholders and Airstrip AI/Global/Techno rows source-verified and guarded
FR-DQ-2G-AI-Page-Batch-ManualApply completed: ModEnc [AI] page source-batch added 19 [AI] rows, updated 62 [AI] rows, and converted 149 Global/Techno rows to guardrails
FR-DQ-2H-TechnoTypes-Common-ManualApply completed: common TechnoTypes fields Primary / Secondary / Strength / Speed / TechLevel / Cost / Armor / Sight / Owner / Prerequisite source-verified; 39 exact object-context rows added and wrong-context AI/Global/Projectile rows guarded
FR-DQ-2I-TechnoTypes-CombatMobility-ManualApply completed: combat / mobility TechnoTypes fields GuardRange / ROT / Locomotor / MovementZone / SpeedType / MovementRestrictedTo / Reload / Ammo / PipWrap / Passengers / Size / Category source-verified; 39 exact object-context rows added and wrong-context Weapon / broad fallback rows guarded
FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply completed: targeting / transport / deploy / hover TechnoTypes fields SizeLimit / OpenTopped / DeploysInto / UndeploysInto / DeployFire / DeployFireWeapon / DeployTime / DeployToLand / Naval / Underwater / JumpJet / BalloonHover / HoverAttack source-verified; 29 exact object-context rows added and 15 existing rows updated
FR-DQ-2K-TechnoTypes-ProductionVeterancy-ManualApply completed: production / crate / veterancy / bounty / protection behavior fields AllowedToStartInMultiplayer / CrateGoodie / Trainable / Insignificant / NoMovingFire / OpportunityFire / ToProtect / ThreatAvoidanceCoefficient / Soylent / Bounty / VeteranAbilities / EliteAbilities source-verified; 39 exact/context rows added and 11 existing rows updated
FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply completed: cloak / radar / sensor / disguise / immunity behavior fields source-verified; 54 exact/context rows added and 14 existing rows updated
FR-DQ-2M-TechnoTypes-WeaponTargeting-ManualApply completed: weapon targeting / acquisition / retaliation / land-naval targeting fields source-verified; 25 exact/context rows added and 17 existing rows updated
FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply completed: economy / resource / pip / IFV / bunker / crush rows source-verified; 23 exact/context rows added and 13 existing rows updated
FR-DQ-2O-TechnoTypes-JumpjetAndFlightTuning-ManualApply completed: jumpjet / flight tuning rows source-verified; 37 exact/context rows added and 27 existing rows updated
FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply completed: economy / resource / pip / IFV / bunker / crush rows source-verified; 23 exact/context rows added and 13 existing rows updated
FR-DQ-2Q-TechnoTypes-RepairPowerCaptureFactoryRadar-BigBatch-ManualApply completed: repair / power / capture / garrison / factory / radar rows source-verified; 42 exact/context rows added and 48 existing rows updated or guarded
FR-DQ-2S-WarheadCore-BigBatch-ManualApply completed: Warhead core, Ares Warhead extensions, and same-domain Phobos Warhead extension rows source-verified; 8 exact/context rows added and 97 existing rows updated or guarded
FR-DQ-2T-ProjectileCore-BigBatch-ManualApply completed: Projectile core, Ares projectile collision/trench extensions, and Phobos projectile interception/collision/trajectory rows source-verified; 7 exact/context rows added and 129 existing rows updated or guarded
FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply completed: advanced Projectile Airburst / Splits / scatter / Gravity / Parachuted / ReturnWeapon / Shrapnel rows source-verified; 4 exact/context rows added and 52 existing rows updated or guarded
FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply completed: Art / Animation core playback, trailer, spawn, visual, Phobos visibility, Anim-to-Unit, fire animation, animation damage, and debris/splash rows source-verified; 14 exact/context rows added and 93 existing rows updated or guarded
FR-DQ-2W-TechnoTypesRemaining-UnresolvedGuardrail-MegaBatch-ManualApply completed: converted remaining exact Techno placeholder/generic Hover rows to NeedsMoreEvidence guardrails and wrote Docs/FieldRegistryUnresolvedRows_2026-06-03.md
Current active task: FR-DQ-2W completed; next recommended phase is FR-DQ-2X-SuperWeaponSideCountryUIMegaBatch-ManualApply.
```

Recent accepted verification reported by user/GPT patch environment:

```text
dotnet restore: not run in patch environment
dotnet build: not run in patch environment
dotnet test: not run because dotnet CLI is unavailable in patch environment
static JSON validation: passed
legacy: not restored
Shell main layout: unchanged
Field Registry provider priority / runtime lookup: unchanged
FR-DQ-2W static JSON validation: passed; clean package validation: passed
```

## Required Context Loading

Before starting any task, read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
The specific task document named by the user
```

For Field Registry UI work, also read:

```text
Docs/FieldRegistrySurfacesUiContract.md
```

Before implementation, summarize:

```text
1. current task goal
2. allowed files
3. forbidden files
4. semantic boundaries
5. AutomationIds to preserve/add
6. validation commands
7. whether user approval is required before implementation
```

Do not modify files during the context-loading step.

## Strict UI Workflow

Codex must not freely redesign UI.

For UI work, follow:

```text
1. Inventory current implementation.
2. Produce exact UI contract.
3. Stop and wait for user approval.
4. Implement only approved files.
5. Request screenshot/manual verification.
6. Run build/test/package.
```

Do not interpret vague requests as permission to redesign XAML:

```text
make it more modern
make it less WPF
polish the frontend
use your judgment
free UI redesign
```

If a UI attempt fails the screenshot requirement, stop and diagnose; do not continue ad-hoc polishing.

## Shell Freeze

Do not modify these unless the user explicitly approves a Shell-specific task:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
main Shell layout
toolbar
menu
Project Explorer
Navigator
bottom tabs
status bar
global docking structure
```

Small ShellWindow.xaml.cs wiring changes are allowed only when a task explicitly says so, for example existing inspector open-path caret positioning. They must be reported clearly.

## Semantic Boundaries

Do not modify unless explicitly asked:

```text
INI parser semantics
Field Registry load/apply/rollback/import/learning semantics
Project > Global > BuiltIn priority
Completion candidate generation
Completion commit behavior
Hover data source
Diagnostics
Save Preflight
Backup / Rollback
Undo / Redo
BuiltIn v3.2 field definitions
```

## Documentation Ownership

Codex is responsible for maintaining most project documentation when it changes project state.

After every completed task, update or propose updates to:

```text
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Any phase-specific contract document that changed
```

Update product docs only when product-facing behavior changes:

```text
Docs/FeatureOverview.md
Docs/UserGuide.md
Docs/ReleaseChecklist.md
Docs/DeveloperNotes.md
```

Update archive/index docs only when historical handoff or doc structure changes:

```text
Docs/HandoffArchiveIndex.md
Docs/DocumentationMaintenance.md
```

Do not edit historical handoff documents just to make them current; index or annotate them instead.

## MCP / Plugin Usage Policy

Use minimal tools by default.

Do not enable or call these unless explicitly needed:

```text
mcp-unity
image_assets
deepseek_worker
node_repl
browser plugin
documents/spreadsheets/presentations plugins
```

This WPF/.NET IDE project normally only needs file access and shell commands for dotnet build/test/package.

## Package Hygiene

Do not hand off a package containing:

```text
.vs/
bin/
obj/
artifacts/
TestResults/
old zip files
```

Generate clean handoff packages through:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## Final Report Format

Every task report must include:

```text
1. Phase completed.
2. Files changed.
3. Commands run.
4. Build/test/package result.
5. Confirmation that legacy was not restored.
6. Confirmation that Shell was not changed unless approved.
7. Confirmation that semantic behavior was unchanged unless approved.
8. Documentation updates made or proposed.
9. Remaining risks.
10. Recommended next phase.
```


- FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply completed: cloak / radar / sensor / disguise / immunity fields source-verified; 54 exact/context rows added and 14 existing rows updated.


- FR-DQ-2O-TechnoTypes-JumpjetAndFlightTuning-ManualApply completed: jumpjet / flight tuning / acceleration fields source-verified; 37 exact/context rows added and 27 existing rows updated or guarded.

- FR-DQ-2T-ProjectileCore-BigBatch-ManualApply completed: Projectile core, Ares projectile collision/trench extensions, and Phobos projectile interception/collision/trajectory rows source-verified; 7 exact/context rows added and 129 existing rows updated or guarded.


## Current Field Registry Data Quality Baseline

Latest completed field-registry phase:

```text
FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply
```

Direct Hover-risk rows after this phase:

```text
273
```

Unresolved / NeedsMoreEvidence rows are tracked in:

```text
Docs/FieldRegistryUnresolvedRows_2026-06-03.md
```

Next recommended phase:

```text
FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply
```

Do not revert to earlier packages. Continue from the clean package produced by FR-DQ-2Z.

- FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply completed
FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply completed: residual direct Hover-risk rows converted to NeedsMoreEvidence guardrails; direct placeholder / integer / numeric generic rows are zero: AttachEffect, Shield, LaserTrail, DigitalDisplay, Insignia, Radiation and related Ares/Phobos extension rows processed; 200 rows affected.


## Current Field Registry Baseline Update - FR-DQ-3C-UnresolvedRecheck-A

Latest clean baseline includes targeted unresolved-row recheck. Direct Hover-risk rows remain 0; unresolved guardrail rows remaining: 1815.


## FR-DQ-3D TeamTypes / AITriggerTypes Schema Recheck completed

- Completed: `FR-DQ-3D-TeamTypes-AITriggerTypes-SchemaRecheck-ManualApply`.
- Added precise `TeamType` / `TaskForce` rows and promoted AI programming legacy rows to source-backed guardrails where reliable ModEnc sources existed.
- Added verification doc: `Docs/FieldRegistryDescriptionVerification_AiSchemaRecheck_2026-06-03.md`.
- Updated unresolved list: `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.
- No provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project file, or legacy behavior changes.
- Current metrics: field count 3519, source-verified rows 2051, unresolved rows 0, direct Hover-risk rows 0.
- Next recommended phase: `FR-DQ-3E-TechnoResidualSourceFamilyRecheck`.


## FR-DQ-3E-LowConfidenceBurnDown

- Manual GPT-side source verification pass promoted/guardrailed 181 Phobos-supported Techno rows.
- Fixed 103 unsupported `schema.type=Text` values to `schema.type=String` while preserving `editorKind=Text`.
- Current metrics: field count 3519, source-verified rows 2051, unresolved rows 0, direct Hover-risk rows 0.


## FR-DQ-3F-InferredBacklogRecovery

- Manual GPT-side relaxed-evidence pass restored the 3E runtime backlog as inferred fallback rows.
- Recovered rows: 1590.
- Current metrics: field count 5109, source-verified rows 2051, inferred fallback rows 1590, unresolved rows 0, unsupported schema.type=Text rows 0, direct Hover-risk rows 0.
- New verification doc: `Docs/FieldRegistryDescriptionVerification_InferredBacklogRecovery_2026-06-03.md`.
- No provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project file, user Global active pack, or legacy behavior changes.

FR-DQ-3F metric correction: field count 5109, source-verified rows 2051, inferred fallback rows 1591, unresolved rows 0, unsupported schema.type=Text rows 0, direct Hover-risk rows 0.

## FR-DQ-3I-ReleaseIconAndUserDocs

- 基线：FR-DQ-3H Fix2 测试全绿版。
- 本阶段只补齐发布资产：应用图标、窗口 / 任务栏 / exe 图标、v0.5.0-preview 用户说明、Release Notes、Known Issues、字段可信度说明、打包说明和烟测清单。
- 不修改 BuiltIn 字段库、Hover 核心逻辑、Diagnostics 核心逻辑、保存链路或 Completion 行为。
- 发布定位：v0.5.0-preview 技术预览版。

