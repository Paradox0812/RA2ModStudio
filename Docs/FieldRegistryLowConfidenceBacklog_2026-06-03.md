# Field Registry Low Confidence Backlog - 2026-06-03

本文件已由 `FR-DQ-3F-InferredBacklogRecovery` 接管。

## Current Status

- 3E runtime backlog rows before this pass: 1590
- Rows recovered into BuiltIn runtime as inferred fallback: 1590
- Remaining rows outside runtime backlog: 0
- Runtime `needs-more-evidence*` rows: 0

## Policy Change

本轮根据用户确认，允许使用不完全权威的社区资料线索以及一定程度的字段名推论。因此这些字段不再以 `needs-more-evidence` 形式留在 backlog，而是恢复到 BuiltIn runtime，并统一使用以下低权重 quality 前缀：

- `community-source-assisted-inferred-*`
- `community-reference-inferred-*`
- `name-inferred-*`

这些行仅用于降低 Unknown Key 误报和提供宽松 Hover 兜底，不视为 source-verified 字段。

## Recovered by appliesTo

- Techno: 1215
- ArtObject: 101
- Building: 70
- Warhead: 53
- Global: 38
- Vehicle: 28
- Weapon: 19
- Country: 11
- Infantry: 9
- Sound: 8
- Terrain: 8
- Banner: 7
- Eva: 6
- AI: 4
- Unit: 4
- Side: 3
- Aircraft: 2
- LaserTrail: 1
- ParticleSystem: 1
- Tiberium: 1
- VoxelAnim: 1

## Superseded 3E Backlog Snapshot

<details>
<summary>展开查看 3E 旧 backlog 摘要与行表</summary>

# Field Registry Low Confidence Backlog - 2026-06-03
本文件由 FR-DQ-3E 静态核验生成，记录 BuiltIn v3.2 中仍未完成来源核验的字段行。
这些行尚未被本阶段晋级为 source-verified，已从 BuiltIn runtime pack 迁出，仅作为后续人工核验 backlog 保存。若后续找到 ModEnc / Ares / Phobos 官方来源，可再分批恢复为 source-verified 字段。
## Summary
- Initial BuiltIn fields: 5109
- Initial needs-more-evidence rows: 1771
- FR-DQ-3E processed rows: 181
- Remaining needs-more-evidence rows in runtime BuiltIn: 0
- Fixed schema.type=Text rows: 103

## Remaining by appliesTo
- Techno: 1215
- ArtObject: 101
- Building: 70
- Warhead: 53
- Global: 38
- Vehicle: 30
- Weapon: 19
- Country: 11
- Infantry: 9
- Terrain: 8
- Sound: 8
- Banner: 7
- Eva: 6
- AI: 4
- Unit: 4
- Side: 3
- Aircraft: 2
- LaserTrail: 1
- ParticleSystem: 1
- Tiberium: 1
- VoxelAnim: 1

## Rows
| appliesTo | key | editorKind | quality | firstSource |
|---|---|---:|---|---|
| AI | `AICaptureWounded` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | YR |
| AI | `SuspendPriority` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | YR |
| AI | `Threat` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | YR |
| AI | `tempValue` | Text | `needs-more-evidence-ares-phobos-extensions-megabatch-20260603` | ManualAudit |
| Aircraft | `Image.ConditionRed` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Aircraft | `Image.ConditionYellow` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| ArtObject | `AddOccupy4` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `AirDeathFalling` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `AirDeathFinish` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `AirDeathStart` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `AltCameo` | Reference | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `BibShape` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Bouncer` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Buildup` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Cameo` | Reference | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `CameoPalette` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `CanBeHidden` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `CanHideThings` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Cheer` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Crater` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Crawl` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `CrawlSounds` | Reference | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Crawls` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DamageFireOffset3` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DamageLevels` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DamageRadius` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DelayedFireDelay` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DemandLoad` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DemandLoadBuildup` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Deploy` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Deployed` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DetailLevel` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Die1` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Die2` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Die3` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Die4` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Die5` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DisableShadowCache` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `DockingOffset3` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Down` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Elasticity` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `EliteSecondaryFireFLH` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `ExpireAnim` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `FireFly` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `FireProne` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `FireUp` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `FiringFrames` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Flat` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Fly` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `ForceBigCraters` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `FreeBuildup` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Guard` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Height` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Hover` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Idle2` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `IsAnimDelayedFire` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `IsMeteor` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Layer` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `LineTrailColor` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `LineTrailColorDecrement` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `MaxXYVel` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `MaxZVel` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `NumParticles` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Paradrop` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `PowerUp3Anim` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `PowerUp3AnimDamaged` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `PowerUp3LocX` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `PowerUp3LocY` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `PowerUp3YSort` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `ProductionAnimY` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `PsiWarning` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `QueueingCell` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Ready` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Recoilless` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `RemoveOccupy4` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Rotates` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Scorch` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `SecondaryFirePixelOffset` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `SecondarySpawnOffset` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `ShadowIndex` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `SpawnDelay` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `SpawnsParticle` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `SpecialAnimThreeYSort` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Sticky` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `SuperAnimY` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Swim` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `TerrainPalette` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `TiberiumSpawnRadius` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `TiberiumSpawnType` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `ToOverlay` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Translucency` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Tread` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Tumble` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `TurretOffset` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Undeploy` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Up` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `UseLineTrail` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Voxel` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `Walk` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `WalkFrames` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `WetAttack` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `WetDie1` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `WetDie2` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `WetIdle1` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `WetIdle2` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `YSortAdjust` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| ArtObject | `ZSortAdjust` | Text | `needs-more-evidence-artobject-visual-megabatch-20260603` |  |
| Banner | `CSF.Color` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Banner | `CSF.VariableFormat` | MultiSelect | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Banner | `Delay` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Banner | `Duration` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Banner | `PCX` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Banner | `SHP` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Banner | `SHP.Palette` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Building | `Adjacent.Allowed` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Adjacent.Disallowed` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Adjacent.Disallowed.ProhibitDistance` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `AllowParallelAIQueues` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `BarracksExitCell` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `BuildingGuardRetryDelay` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `BuildingRepairedSound` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `BunkerDamageMultiplier` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `BunkerROFMultMultiplier` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `BunkerWallsDownSound` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `BunkerWallsUpSound` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `CameoPriority` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DefaultInfantrySelectBox` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DefaultUnitSelectBox` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DeployedPrimaryFireFLH` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DeployedSecondaryFireFLH` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DisplayIncome` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DisplayIncome.Delay` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DisplayIncome.Houses` | Enum | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `DisplayIncome.Offset` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `EngineerRepairAmount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `FactoryPlant.AllowTypes` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `FactoryPlant.DisallowTypes` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `FactoryPlant.MaxCount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Frames` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Grinding.AllowTypes` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Grinding.DisallowTypes` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Grinding.Sound` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Grinding.Weapon.RequiredCredits` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `GroundFrames` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `GroundOffset` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `GroundPalette` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `GroundShape` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `GuardRetryDelay` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `HarvesterCounter.ConditionRed` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `HarvesterCounter.ConditionYellow` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `InitialStrength.Cloning` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `MissingCameo` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `OccupyDamageMultiplier` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `OccupyROFMultiplier` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Offset` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Overpower.ChargeWeapon` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Overpower.KeepOnline` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Palette` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PlacementPreview.Offset` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PlacementPreview.Palette` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PlacementPreview.Remap` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PlacementPreview.Shape` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PlacementPreview.ShapeFrame` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PlacementPreview.Translucency` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PowerPlant.DamageFactor` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PowerPlantEnhancer.Amount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PowerPlantEnhancer.Factor` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PowerPlantEnhancer.MaxCount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PowerPlantEnhancer.PowerPlants` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PowerPlantEnhancer.Range` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `PronePrimaryFireFLH` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `ProneSecondaryFireFLH` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `SellBuildupLength` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Shape` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `SlavesFreeSound` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `SpyEffect.InfiltratorSuperWeapon` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `SpyEffect.VictimSuperWeapon` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Translucency` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Units.RepairPercent` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Units.RepairRate` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `Units.RepairStep` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building | `VisibleToHouses` | Enum | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building,Vehicle | `EVA.Sold` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Building,Vehicle | `SellSound` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Country | `File.Flag` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `File.LoadScreen` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `File.LoadScreenPAL` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `File.Taunt` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `ListIndex` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `LoadScreenText.Brief` | Reference | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `LoadScreenText.Color` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `LoadScreenText.Name` | Reference | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `LoadScreenText.SpecialName` | Reference | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `MenuText.Status` | Reference | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Country | `RandomSelectionWeight` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Eva | `Allied` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Eva | `Priority` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Eva | `Russian` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Eva | `Text` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Eva | `Type` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Eva | `Yuri` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `AdjacentWallDamage` | Integer | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `AircraftLevelLightMultiplier` | Integer | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `AnimRemapDefaultColorScheme` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `ArtImageSwap` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `AttachedParticleSystem` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `BalloonHoverDampen` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `ChronoSparkleBuildingDisplayPositions` | MultiSelect | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `ChronoSparkleDisplayDelay` | Integer | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `DarkGreen` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DarkSky` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DefaultLS640BkgdName` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DefaultLS640BriefLocX` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DefaultLS640BriefLocY` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DefaultLS800BkgdName` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DefaultLS800BkgdPal` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DefaultLS800BriefLocX` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `DefaultLS800BriefLocY` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `FreeMCV.CreditsThreshold` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Global | `Gold` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `Green` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `IsSelectableCombatant` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | YR |
| Global | `JumpjetLevelLightMultiplier` | Integer | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `MetallicDebris` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `PreventAutoDeploy` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | YR |
| Global | `PsychicRevealRadius` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `Purple3` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Global | `Ranking.ParTimeEasy` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Global | `Ranking.ParTimeHard` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Global | `Ranking.ParTimeMedium` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Global | `RepairBaseNodes` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Global | `SecretInfantry` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | YR |
| Global | `SecretUnits` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | YR |
| Global | `Smoke` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `Storage.TiberiumIndex` | Integer | `needs-more-evidence-ares-phobos-extensions-megabatch-20260603` | ManualAudit |
| Global | `UnitCrateVehicleCap` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Global | `WarpAway` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `WarpOut` | Text | `needs-more-evidence-global-visual-megabatch-20260603` |  |
| Global | `Yellow` | Text | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Infantry | `DefaultDisguise` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `EngineerRepairAmount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `InfantryAutoDeploy` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `PowersUp.Buildings` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `PowersUp.Owner` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `ProneSpeed` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `ProneSpeed.Crawls` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `ProneSpeed.NoCrawls` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Infantry | `Slaved.OwnerWhenMasterKilled` | Enum | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| LaserTrail | `Bolt.Arcs` | Text | `needs-more-evidence-weapon-core-20260603` | Reference gap |
| ParticleSystem | `Gas.MaxDriftSpeed` | Integer | `needs-more-evidence-particlesystem-visual-megabatch-20260603` |  |
| Side | `PowerDelta.ConditionRed` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Side | `PowerDelta.ConditionYellow` | Integer | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Side | `ToolTipBlur` | Boolean | `needs-more-evidence-superweapon-side-country-ui-megabatch-20260603` | RA2IniEditor manual audit |
| Sound | `Attack` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Sound | `Control` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Sound | `FShift` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Sound | `Limit` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Sound | `MinVolume` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Sound | `Priority` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Sound | `Type` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Sound | `Volume` | Text | `needs-more-evidence-art-sound-megabatch-20260603` |  |
| Techno | `AARate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIAllToHunt` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIAlternateProductionCreditCutoff` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIAttackMoveTargetingDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIAutoDeployFrameDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIBasePlanningSide` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIBuildThis` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIBuildsWalls` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIDifficulty` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIFireSale` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIFireSaleDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIGuardAreaTargetingDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIIonCannonPlugValue` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIIonCannonTempleValue` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AINavalYardAdjacency` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AINormalTargetingDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIParadropMission` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIPickWallDefensePercent` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIPlayers` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIRestrictReplaceTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AISuperDefenseDistance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AITriggerFailureWeightDelta` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AITriggerSuccessWeightDelta` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AITriggerTrackRecordCoefficient` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AIUseTurbineUpgradeProbability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Accelerates` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Action` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AddPlanningModeCommandSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Adjacent` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AirRangeBonus` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AirShadowBaseScale` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Aircraft.DefaultDigitalDisplayTypes` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AircraftCostBonus` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AircraftFogReveal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Airspeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlliedCrew` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlliedDisguise` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlliedSurvivorDivisor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlliedWallTransparency` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlliesAllowed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AllowAirstrike` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AllowBurrowing` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AllowShroudedSubteranneanMoves` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AllowWeaponSelectAgainstWalls` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AllyParaDropInf` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AllyParaDropNum` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AllyReveal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlphaImage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AltImage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlternateArcticArt` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlternateFLH.ApplyVehicle` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlternateFLH.OnTurret` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlternateFlightLevel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AlwaysConsideredThreat` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmbientChangeRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmbientChangeStep` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmbientDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmbientSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmerParaDropInf` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmerParaDropNum` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Ammo.AutoConvertMaximumAmount` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Ammo.AutoConvertMinimumAmount` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Ammo.AutoConvertType` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmmoCrateDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmmoPipFrame` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmmoPipOffset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmmoPipSize` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AmmoPipWrapStartFrame` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AnimationProbability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AnimationRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AntiXXXValue` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ApproachTargetResetMultiplier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AreaGuardRange` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ArmorDefensesMult` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Artillary` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AssaultAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AtmosphereEntry` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AtomDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AttachedParticleSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AttackCursorOnDisguise` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AttackCursorOnFriendlies` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AttackFriendlies` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AttackingAircraftSightRange` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AutoCrush` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AutoTarget.NoThreatBuildings` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AutoTargetAI.NoThreatBuildings` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AuxBuilding` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AuxSound1` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `AuxSound2` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BalloonHoverAcceleration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BalloonHoverBob` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BalloonHoverBoost` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BalloonHoverBrake` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BalloonHoverDampen` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BalloonHoverHeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BarrelAnimIsVoxel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BarrelDebris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BarrelExplode` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BarrelParticle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BarrelStartPitch` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BaseBias` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BaseDefenseDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BaseNormal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BaseUnderAttackSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BaseUnit` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Bases` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BehavesLike` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Behind` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BerzerkAllowed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BerzerkTargeting` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BlendedFog` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BombAttachSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BombDisarm` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BombSight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BombTickingSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Bombable` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BounceAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BounceSound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Bouncy` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BridgeDestruction` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BridgeExplosions` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BridgeRepairHut` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BridgeStrength` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BridgeVoxelMax` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildAirstrip` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildLimit` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildOffAlly` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildSlowdown` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildTimeDefensesMult` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Buildable` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingAbandonedSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingDamageSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingDieSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingDrop` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingGarrisonedSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingPlace` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingRepairedSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildingSlam` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Buildings.DefaultDigitalDisplayTypes` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Buildup` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildupSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `BuildupTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Bullets` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `C4` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `C4Delay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `C4Warhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Camera` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CameraRange` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CampaignDefaultGameSpeed` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CampaignMoneyDeltaEasy` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CampaignMoneyDeltaHard` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CanApproachTarget` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CanBeach` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CanC4` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CanDetonateDeathBomb` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CanDetonateTimeBomb` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CanDrive` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CanRecalcApproachTarget` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CaptureTheFlag` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CarriesCrate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Carryall` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CausesDelayKill` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CellAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CellInset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChainReaction` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChargeAnim` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChargeToDrainRatio` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChargedAnimTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Charges` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CheerSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoBeam` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoBeamColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoBlast` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoBlastDest` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoHarvTooFarDistance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoInSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoOutSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoPlacement` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoReinfDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoSparkle1` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoSphereDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChronoSpherePreDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Chronoshift.WarpIn` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Chronoshift.WarpOut` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ChuteSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CivEvac` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Civilian` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ClearAllWeapons` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ClickRepairable` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CliffBackImpassability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Climb` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Cloak` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CloakDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CloakGenerator` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CloakRadiusInCells` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CloakSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CloakStop` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CloakingStages` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Cluster` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CollapseChance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Color` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ColorAddUse8BitRGB` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ColorList` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ColorSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ConcentricRadialIndicator` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ConditionRed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ConditionRedSparkingProbability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ConditionYellow` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ConditionYellowSparkingProbability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ConsideredAircraft` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ContentScan` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Convert.ResetMindControl` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CostDefensesMult` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Crash` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrashingSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Crate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateArmourSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateBeneath` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateBeneathIsMoney` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateFireSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateImg` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateMaximum` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateMinimum` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateMoneySound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CratePromoteSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateRegen` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateRevealSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateSpeedSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateTrigger` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrateUnitSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CraterLevel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Craters` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Crates` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CreateAircraftSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CreateInfantrySound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CreateSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CreateUnitSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CreditTicks` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CrewEscape` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CruiseHeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Crush` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Culling` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CustomGS` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CustomGSN.ChangeDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `CustomGSN.DefaultDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Cyborg` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislAcceleration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislAltitude` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislBodyLength` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislEliteDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislEliteWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislLazyCurve` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislPauseFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislPitchFinal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislPitchInitial` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislTiltFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislTurnRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DMislWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamSmkOffScrnRel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamageDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamageFireTypes` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamageParticleSystems` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamageRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamageSmokeOffset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamageSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamageToFirestormDamageCoefficient` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DamagedSpeed` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DarkGreen` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DarkSky` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Deacc` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeadBodies` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeathAnims` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeathWeapon` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeathWeaponDamageModifier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Debris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DebrisTypes` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DebrisTypes.Limit` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultChronoSound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultDebrisSmokeSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultFireStreamSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultFirestormExplosionSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultLargeGreySmokeSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultLargeRedSmokeSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultMirageDisguises` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultRepairParticleSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultSmallGreySmokeSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultSmallRedSmokeSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultSparkSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultTestParticleSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DefaultToGuardArea` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Deform` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeformThreshhold` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Degenerates` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DelayKillAtMax` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DelayKillFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeleteOnStateLimit` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeploySound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DeployToFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DestroyAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DestroyParticleSystems` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DestroySmokeOffset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DestroyWalls` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DetailBufferZoneWidth` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DetailMinFrameRateMovie` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DetailMinFrameRateNormal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DetectDisguiseRange` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DetectionDistance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DetonationAltitude` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Dig` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DigSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DirectRocker` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Disableable` | Text | `needs-more-evidence-repair-power-capture-factory-radar-20260603` | ModEnc |
| Techno | `DisableableFromShell` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DisabledDisguiseDetectionPercent` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DisguiseBlinkingVisibility` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DisguiseFakeBlinkTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DisguiseFireOnly` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Disguised` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DissolveUnfilledTeamDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DistributeTargetingFrame` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DistributeTargetingFrame.AIOnly` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DistributedWeaponFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Doggie` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DoubleOwned` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DrainAnimationType` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DrainMoneyAmount` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DrainMoneyFrameDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DrawBoltAsLaser` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DrawFlat` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DriveLocomotorMakesWake` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DriverKilled.KeptPassengers` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DriverKilled.KillPassengers` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.AirImage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Angle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.AtmosphereEntry` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.GroundAnim` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Height` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Puff` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Speed` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Trailer` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Trailer.Attached` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Trailer.SpawnDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Weapon` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPod.Weapon.HitLandOnly` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPodAngle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPodHeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPodPuff` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPodSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropPodWeapon` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropZoneAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `DropZoneRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Duration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EMEffect` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EMPulseCannon` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EMPulseProjectile` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EMPulseSparkles` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EMPulseWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Elasticity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ElectricAssault` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ElevationBonusCap` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ElevationIncrement` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ElevationIncrementBonus` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EligibileForAllyBuilding` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EligibleForDelayKill` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EliteFlashTimer` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EliteWeaponN` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EmptyAmmoPipFrame` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EmptyReload` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EmptySpawnsPipFrame` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EnableSelectBox` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EndPlanningModeSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EndStateAI` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EnemyHealth` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EnemyHouseThreatBonus` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EnemyInsignia` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EngineerCaptureLevel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `EnterTransportSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExecutePlanSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExitCoord` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExpSpread` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Experience.AirstrikeModifier` | Float | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Experience.FromAirstrike` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Experience.FromPassengers` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Experience.MindControlSelfModifier` | Float | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Experience.MindControlVictimModifier` | Float | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Experience.PassengerModifier` | Float | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Experience.PromotePassengers` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExpireAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExpireSound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Explodes` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Explodes.KillPassengers` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Explosion` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExplosiveVoxelDebris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraAircraftLight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraInfantryLight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraPower` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraThreat.InRange` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraThreat.IsThreat` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraThreatCoefficient.DistanceToLastTarget` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraThreatCoefficient.Facing` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraThreatCoefficient.InRangeDistance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ExtraUnitLight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FLHKEY.BurstN` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Fake` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FallingDownDamage` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FallingDownDamage.AllowEMP` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FallingDownDamage.Water` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Fearless` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FillEarliestTeamProbability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FinalDamageState` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FineDiffControl` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FireSupress` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Firepower` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FirersPalette` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FirestormActiveAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FirestormAirAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FirestormGroundAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FirestormIdleAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FirestormWall` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FirestormWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FixRepairStepCost` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FlameDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FlameDamage2` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FlamingInfantry` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FlashFrameTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FlightLevel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Float` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FloatBeach` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Floater` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FogOfWar` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FogRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Foot` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ForbidParallelAIQueues` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ForceShield.Effect` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ForceShield.EffectOnOrganics` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ForceShield.KillOrganicsWarhead` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ForceShield.KillWarhead` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Fraidycat` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `FreeMCV` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GDI` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GDIBarracks` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GDIFirestormGenerator` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GDIGateOne` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GDIGateTwo` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GDIHunterSeeker` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GDIPowerTurbine` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUIBuildSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUICheckboxSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUICloseSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUIComboCloseSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUIComboOpenSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUIMainButtonSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUIMoveInSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUIMoveOutSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUIOpenSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GUITabSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GainSelfHealAllowMultiplayPassive` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GainSelfHealFromAllies` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GainSelfHealFromPlayerControl` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GameClosed` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GameForming` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GameSpeedBias` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GapGenerator` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GapRadiusInCells` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Gas` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Gate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GateCloseDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GateDown` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GateUp` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GatherWhenMCVDeploy` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GenericBeep` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GenericClick` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Green` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Groundspeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Growth` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GrowthPercentage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GrowthRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GuardArea` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GuardAreaTargetingDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `GuardSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Gunner` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HSBuilding` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HalfDamageSmokeLocation` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HalfDamageSmokeLocation1` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HalfDamageSmokeLocation2` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Harvester.Counted` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HarvesterDumpRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HarvesterLoadRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HarvesterTooFarDistance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HarvesterTruce` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HarvesterUnit` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HasRadialIndicator` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HasSpotlight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HasStupidGuardMode` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HasTurretTooltips` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HealBase` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HealCrateSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HealScanRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HealthBar.Hide` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HealthBar.HidePips` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HealthBar.Permanent` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HealthBar.Permanent.PipScale` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HeightShadowScaling` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HeightShadowScaling.MinScale` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Helipad` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HideSelectBox` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoldsWhat` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HomingScatter` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Hover` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverAcceleration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverBob` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverBoost` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverBrake` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverDampen` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverHeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverLocomotorMakesWake` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HoverPad` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HunterSeeker` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HunterSeekerAscentSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HunterSeekerDescendProximity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HunterSeekerDescentSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HunterSeekerDetonateProximity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `HunterSeekerEmergeSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ICBM` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ICBMLauncher` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IFVTransformSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IRepairRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IRepairStep` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IceBreakingWeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IceCrackSounds` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IceCrackingWeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IceGrowthRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IceSolidifyFrameTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IdleActionFrequency` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IdleRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IgnoreForBaseCenter` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IgnoresFirestorm` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Immune` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ImmuneToCrit` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ImpactLandSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ImpactWaterSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IncomeMult` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Incoming` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IncomingMessage` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Infantry.DefaultDigitalDisplayTypes` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InfantryBlinkDisguiseTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InfantryExplode` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InfantryGainSelfHeal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InfantryGainSelfHealCap` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InfantryHeadPop` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InfantryNuked` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InfiniteMindControl` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InitialAmmo` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InitialSpawnsNumber` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InitialStrength` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Invisible` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `InvisibleInGame` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Inviso` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Invulnerability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IonBeam` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IonBlast` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IonCannonDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IonCannonWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IonSensitive` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IronCurtain.Effect` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IronCurtain.EffectOnOrganics` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IronCurtain.KillOrganicsWarhead` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IronCurtain.KillWarhead` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IronCurtain.Modifier` | Float | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IronCurtainDuration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IronCurtainInvokeAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsARock` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsAlternateColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsAnimated` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsBaseDefense` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsBigLaser` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsCanine` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsChargeTurret` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsDropship` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsElectricBolt` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsFlammable` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsGattling` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsHouseColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsLaser` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsLocomotor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsPlug` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsPowered` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsRadBeam` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsRadEruption` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsRailgun` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsRubble` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsSimpleDeployer` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsSonic` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsTemple` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsThreatRatingNode` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsTilter` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsTrain` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IsVeinholeMonster` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Ivan` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IvanBombAttachToCenter` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IvanDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IvanIconFlickerRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IvanTimedDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `IvanWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `JumpjetClimbIgnoreBuilding` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `JumpjetRotateOnCrash` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Land` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LargeFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LargeVisceroid` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Laser` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserDuration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserFence` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserFencePost` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserInnerColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserOuterColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserOuterSpread` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LaserTargetColor` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LeadershipRating` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LeaveTransportSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LegalTarget` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LeptonsPerFireIncrease` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LeptonsPerSightIncrease` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Level` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Lifetime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightBlueTint` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightGreenTint` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightIntensity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightRedTint` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightSize` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightVisibility` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningCellSpread` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningDeferment` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningHitDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningPrintText` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningRod` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningScatterDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningSeparation` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningSounds` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningStormDuration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LightningWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LineTrailColorOverride` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LocalRadarColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LowDeployPriority` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LowPowerPenaltyModifier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `LowSelectionPriority` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MCVRedeploys` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MakeInfantry` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MakesDisguise` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MakesWake` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ManualControl` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ManualReload` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxAngularVelocity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxDC` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxDebris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxEC` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxGuardRange` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxIQLevels` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxLowPowerProductionSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxMoney` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxUnitCount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxWaypointPathLength` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxXYVel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaxZVel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaximumBuildingPlacementFailures` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaximumCheerRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MaximumQueuedObjects` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MessageCharTyped` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MessageDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MetallicDebris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinAngularVelocity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinDebris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinLowPowerProductionSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinMoney` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinUnitCount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinZVel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MinZVelocity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MindControl` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MindControl.IgnoreSize` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MindControlLink.VisibleToHouse` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MindControlRangeLimit` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MindControlSize` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MissileROTVar` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MissileSafetyAltitude` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MissileSpeedVar` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MobileFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Money` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MoneyIncrement` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MoveFlash` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MovementPerturbutationCoefficient` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MovieOff` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MovieOn` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MovieTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MultiEngineer` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MultiMindControl.ReleaseVictim` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MultiWeapon` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MultiWeapon.SelectCount` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Multiplay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MultiplayPassive` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MultipleFactory` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Mutant` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MuzzleFlash` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `MyEffectivenessCoefficientDefault` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NODBarracks` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NamedCivilians` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Napalm` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NaturalParticleLocation` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NaturalParticleSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NeonLime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NeverUse` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NextParticle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NextParticleOffset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoManualMove` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoParachuteMaxFallRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoQueueUpToEnter` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoQueueUpToEnter.Buildings` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoQueueUpToUnload` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoQueueUpToUnload.Buildings` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoRearm.Temporal` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoRearm.UnderEMP` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoReload.Temporal` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoReload.UnderEMP` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoSecondaryWeaponFallback` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoSecondaryWeaponFallback.AllowAA` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoShadow` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoThreat` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoUseTileLandType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NoWobbles` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Nod` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NodAIBuildsWalls` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NodAdvancedPower` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NodGateOne` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NodGateTwo` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NodHunterSeeker` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Nominal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NonVehicle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NormalTargetingDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NotHuman` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NotWorkingSound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NukeDown` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NukeMaker` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NukeProjectile` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NukeTakeOff` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NukeWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NumLoopFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NumberImpassableRows` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `NumberOfDocks` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Occupier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OnFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OneFrameLight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Operator` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OptionsChanged` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OreGathering.Anims` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OreGathering.Tiberiums` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OrePurifier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OreTwinkle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `OreTwinkleChance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Organic` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Overload.Count` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Overload.Damage` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Overload.DeathSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Overload.Frames` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Overload.ParticleSys` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Overload.ParticleSysCount` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Overrides` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PParatrooper` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PackupSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PadAircraft` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Parachute` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ParachuteMaxFallRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ParadropMission` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ParadropRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Paralyzed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Paralyzes` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Parasite` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Parasiteable` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Paratrooper` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Particle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ParticleCap` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ParticleSystem` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Particles` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ParticlesPerCoord` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PenetratesTransport.Level` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Persistant` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Pilot` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Pips.SelfHeal.Buildings` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Pips.SelfHeal.Buildings.Offset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Pips.SelfHeal.Infantry` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Pips.SelfHeal.Infantry.Offset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Pips.SelfHeal.Units` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Pips.SelfHeal.Units.Offset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlaceAnywhere` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlaceBeaconSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlacementDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlacementGrid.Translucency` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlacementGrid.TranslucencyWithPreview` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlacementPreview` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlacementPreview.Translucency` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerAttackMoveTargetingDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerAutoCrush` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerGuardAreaTargetingDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerJoined` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerLeft` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerNormalTargetingDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerReturnFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PlayerScatter` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Players` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PoseDir` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PositionPerturbutationCoefficient` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PowerUpNLocYY` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PowersUpToLevel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Prefix` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrerequisiteBarracks` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrerequisiteFactory` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrerequisiteOverride` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrerequisitePower` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrerequisiteProc` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrerequisiteRadar` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrerequisiteTech` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PreventAutoDeploy` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PriorityDeployFiltering` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrioritySelectionFiltering` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrismSupportDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrismSupportDuration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrismSupportHeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrismSupportMax` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrismSupportModifier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PrismType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ProduceCashAmount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ProduceCashDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ProduceCashStartup` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Production` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Promote.EliteAnimation` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Promote.IncludeSpawns` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Promote.VeteranAnimation` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ProtectWithWall` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ProtectedDriver` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PsychicDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PsychicDetectionRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PsychicSensorDetectAttach` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PsychicSensorDetectSound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `PurifierBonus` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadApplicationDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadDurationMultiple` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadLevel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadLevelDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadLevelFactor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadLevelMax` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadLightDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadLightFactor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadSiteWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadTintFactor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarCombatFlashTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarEventColorSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarEventDurations` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarEventMinRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarEventRotationSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarEventSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarEventSuppressionDistances` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarEventVisibilityDurations` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarInvisibleToHouse` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarJamAffect` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarJamDelay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarJamHouses` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarJamIgnore` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarJamRadius` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarOff` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarOn` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadarVisible` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadialColor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadialFireSegments` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RadialIndicatorVisibility` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Radiation` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Radius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RailgunDamageRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RateDown.Cover.AmmoBelow` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RateDown.Cover.Value` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RateDown.Delay` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RateDown.Reset` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ReadinessReductionMultiplier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RechargeTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RecountBurst` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Recruitable` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RefnSmokeOffsetOne` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RefnSmokeOffsetTwo` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RefundPercent` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RejoinTeamIfLimboed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ReloadIncrement` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ReloadRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RepairBay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RepairDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RepairPercent` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RepairRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RepairSell` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RepairStep` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RequiresStolenAlliedTech` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RequiresStolenSovietTech` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ReselectIfLimboed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Retaliate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Reveal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RevealByHeight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RevealToAll` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RevealTriggerRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RevengeWeapon.AffectsHouse` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `RollAngle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Rotates` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SabotageCursor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SavourDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Scatter` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ScatterSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ScoldSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Scorches` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Scorches1` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Scorches2` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Scorches3` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Scorches4` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ScoreAnimSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ScrapVoxelDebris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ScrollMultiplier` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SecretBuildings` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SecretLab` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SelectionFlashDuration` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SelfHealGainType` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SelfHealInfantryAmount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SelfHealInfantryFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SelfHealUnitAmount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SelfHealUnitFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SellBack` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SellSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Semi-persistant` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SensorArray` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SeparateAircraft` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShadowGrow` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShadowIndex.Frame` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShadowIndices` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShadowIndices.Frame` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShadowSizeCharacteristicHeight` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShakeScreen` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShareBarrelData` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShareBodyData` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShareSource` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShareTurretData` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShellButtonSlideSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShieldType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShipLocomotorMakesWake` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShipSinkingWeight` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Shipyard` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShortGame` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShowDesignatorRange` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShowFlashOnSelecting` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShowOccupantPips` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShowSpawnsPips` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Shroud` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShroudGrow` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ShroudRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Side` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SideBarImage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SilverCrate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SinkingSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SkirmishUnlimitedColors` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Slowdown` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SmallFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SmallVisceroid` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SmartAI` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SnowOccupationBits` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SoloCrateMoney` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Sonic` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SovParaDropInf` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SovParaDropNum` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SovietCrew` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SovietDisguise` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SovietLoad` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SovietSurvivorDivisor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SparkSpawnFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnCutoff` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnDirection` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnSparkPercentage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnTranslucencyCutoff` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Spawner` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Spawns.Queue` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnsPipFrame` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnsPipOffset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnsPipSize` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpawnsTiberium` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpeakDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpeedAircraftMult` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpeedTypes` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpiralDeltaPerCoord` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpiralRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpotlightAcceleration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpotlightAngle` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpotlightLocationRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpotlightMovementRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpotlightRadius` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpotlightSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Spread` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpreadPercentage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpyCameraFrames` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpyMoneyStealPercent` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpyPlaneCamera` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpyPowerBlackout` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpySatActivationSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SpySatDeactivationSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Squad` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StartColor1` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StartColor2` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StartFrame` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StartPlanningModeSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StartSound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StartStateAI` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StateAIAdvance` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StopSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StormSound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `StupidHunt` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SubterraneanHeight` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SubterraneanSpeed` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Suffix` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuperAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuperGapRadiusInCells` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuperWeaponsAllowed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuppressKillWeapons` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuppressKillWeapons.Types` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuppressionThreshold` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Supress` | Text | `needs-more-evidence-weapon-core-20260603` | Reference gap |
| Techno | `SurvivorRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuspendDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SuspendPriority` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `SystemError` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TalkBubbleTime` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TargetCoordOffset` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TargetDistanceCoefficientDefault` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TargetEffectivenessCoefficientDefault` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TargetLaser` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TargetSpecialThreatCoefficientDefault` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TargetStrengthCoefficientDefault` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TargetZoneScanType` | Enum | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TeamMember.ConsideredAs` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Technician` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Teleporter` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TemperateOccupationBits` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Temporal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TerrainFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TeslaCharge` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TeslaZap` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Thief` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ThreatPerOccupant` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumExplosionDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumExplosive` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumFarScan` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumGrows` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumNearScan` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumProof` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumStrength` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiberiumTransmogrify` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TickTank` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TiltsWhenCrushes` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TimerWarning` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TireVoxelDebris` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ToTile` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TogglePower` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TooBigToFitUnderBridge` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TopYellow` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Track` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TrackedDownhill` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TrackedUphill` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Translucent25State` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Translucent50State` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TreeFire` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TreeFlammability` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TreeStrength` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TreeTargeting` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TunnelSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurnRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnimDamaged` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnimGarrisoned` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnimIsVoxel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnimX` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnimY` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnimYSort` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretAnimZAdjust` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretCount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretRotateSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `TurretSpins` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Type` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `URepairRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UndeployDelay` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UndeploySound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Underground` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Unit` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitCount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitCrateType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitEnterSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitExitSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitPowerDrain` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitReload` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitsGainSelfHeal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnitsGainSelfHealCap` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnloadingClass` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UnloadingHarvester` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UpgradeEliteSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UpgradeVeteranSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Upgrades` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UseChargeDrain` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UseDisguiseMovementSpeed` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UseMinDefenseRule` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `UseSparkParticles` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3EliteWarhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketAcceleration` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketAltitude` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketBodyLength` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketEliteDamage` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketLazyCurve` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketPauseFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketPitchFinal` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketPitchInitial` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketTiltFrames` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketTurnRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3RocketType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `V3Warhead` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Value` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VehicleThief` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VehicleType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Vehicles.DefaultDigitalDisplayTypes` | MultiSelect | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeinholeMonsterStrength` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Velocity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VelocityPerturbationCoefficient` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeryHigh` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Veteran` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranAircraft` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranArmor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranCap` | Integer | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranCombat` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranInfantry` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranROF` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranRatio` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranSight` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VeteranUnits` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoiceCapture` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoiceComment` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoiceCrashing` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoiceDie` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoiceEnter` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoiceFalling` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoiceSinking` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Volatile` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoxelBarrelFile` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoxelBarrelOffsetToBarrelEnd` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoxelBarrelOffsetToBuildingPivotPoint` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoxelBarrelOffsetToPitchPivotPoint` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoxelBarrelOffsetToRotatePivotPoint` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `VoxelIndex` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Wake.Grapple` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Wake.Sinking` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WalkLocomotorMakesWake` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WalkRate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WallAbsoluteDestroyer` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WallBuildSpeedCoefficient` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WallOwner` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WallPenetratorThreshold` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WallTower` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WantsExtraSpace` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Warheads` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WarpInWeapon.UseDistanceAsDamage` | Boolean | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Warpable` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WaterBound` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WaypointAnimationSpeed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeaponCount` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeaponN` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeaponNullifyAnim` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeaponType` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeatherConBoltExplosion` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeatherConBolts` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeatherConClouds` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WeedCapacity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Weeder` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Wheel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WheeledDownhill` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WheeledUphill` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WindDirection` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WindEffect` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Winged` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WobbleDeviation` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WobblesPerSecond` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WonlineTournamentAllowed` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WoodCrate` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WoodCrateImg` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `WorkingSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `XVelocity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `YVelocity` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Yellow` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `YuriMindControlSound` | Reference | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ZFudgeBridge` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ZFudgeCliff` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ZFudgeColumn` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ZFudgeTunnel` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ZVelocityRange` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `Zombie` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Techno | `ZoomInFactor` | Text | `needs-more-evidence-techno-remaining-megabatch-20260603` | RA2IniEditor manual audit |
| Terrain | `AnimationLength` | Integer | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Terrain | `ConditionYellow.Terrain` | Integer | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Terrain | `MinimapColor` | Integer | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Terrain | `Palette` | Text | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Terrain | `SpawnsTiberium.CellsPerAnim` | Integer | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Terrain | `SpawnsTiberium.GrowthStage` | Integer | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Terrain | `SpawnsTiberium.Range` | Integer | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Terrain | `SpawnsTiberium.Type` | Text | `needs-more-evidence-terrain-visual-megabatch-20260603` |  |
| Tiberium | `MinimapColor` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Unit | `Prerequisite.Lists` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Unit | `Prerequisite.Negative` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Unit | `Prerequisite.RequiredTheaters` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Unit | `Prerequisite.StolenTechs` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Vehicle | `Ammo.AddOnDeploy` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `Ammo.AutoDeployMaximumAmount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `Ammo.AutoDeployMinimumAmount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `Ammo.DeployUnlockMaximumAmount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `Ammo.DeployUnlockMinimumAmount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `CrateGoodie.RerollChance` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `CrushForwardTiltPerFrame` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `CrushOverlayExtraForwardTilt` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `CrushSlowdownMultiplier` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `DefaultMirageDisguises` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `DeployDir` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `DeployingAnims` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `DestroyAnim` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `FireUp` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `HarvesterDumpAmount` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `HarvesterDumpRate` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `HarvesterLoadRate` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `HarvesterScanAfterUnload` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `Image.ConditionRed` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `Image.ConditionYellow` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `IsSimpleDeployer.DisallowedLandTypes` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `JumpjetTilt.ForwardAccelFactor` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `JumpjetTilt.ForwardSpeedFactor` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `JumpjetTilt.SidewaysRotationFactor` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `JumpjetTilt.SidewaysSpeedFactor` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `WaterImage.ConditionRed` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `WaterImage.ConditionYellow` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Vehicle | `WeaponGroupAsN` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| VoxelAnim | `Trailer.SpawnDelay` | Integer | `needs-more-evidence-voxelanim-visual-megabatch-20260603` |  |
| Warhead | `AffectsVeterancy` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `AirstrikeTargets` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Ammo.Shared.Group` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `AnimZAdjust` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `ApplyPerTargetEffectsOnDetonate` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `CreateGap` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Crit.ActiveChanceAnims` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DamageAlliesMultiplier.Berzerk` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DamageAlliesMultiplier.NotAffectsEnemies` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DamageEnemiesMultiplier.Berzerk` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DamageOwnerMultiplier.Berzerk` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DamageOwnerMultiplier.NotAffectsEnemies` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DebrisAnims` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DetonateOnAllMapObjects.AffectTypes` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DetonateOnAllMapObjects.AffectsHouse` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DetonateOnAllMapObjects.AffectsTarget` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DetonateOnAllMapObjects.IgnoreTypes` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DisplayIncome.Delay` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `DisplayIncome.Houses` | Enum | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `ElectricAssaultLevel` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `KillDriver` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Warhead | `KillWeapon.AffectsHouse` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `KillWeapon.AffectsTarget` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `KillWeapon.OnFirer.AffectsHouse` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `KillWeapon.OnFirer.AffectsTarget` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `LaunchSW` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `LaunchSW.DisplayMoney.Houses` | Enum | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `LaunchSW.DisplayMoney.Offset` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Malicious` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Warhead | `Parasite.CullingTarget` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Parasite.GrappleAnim` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Parasite.ParticleSystem` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `PenetratesTransport.Level` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `PreImpactAnim` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Warhead | `RemoveParasite.Allow` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `RemoveParasite.Disallow` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `ReturnWarhead` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `ReturnWarhead.AffectsHouse` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `ReturnWarhead.AffectsTarget` | MultiSelect | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `ReturnWarhead.Chance` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `ReturnWarhead.Damage` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Reveal` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Rocker.AmplitudeMultiplier` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Rocker.AmplitudeOverride` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `SuppressReflectDamage.Groups` | MultiSelect | `needs-more-evidence-ares-phobos-extensions-megabatch-20260603` | ManualAudit |
| Warhead | `SuppressReflectDamage.Types` | Reference | `needs-more-evidence-ares-phobos-extensions-megabatch-20260603` | ManualAudit |
| Warhead | `TransactMoney` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `TransactMoney.Display.Houses` | Enum | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `TransactMoney.Display.Offset` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Warhead | `Versus.clingfilm.PassiveAcquire` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Warhead | `Versus.magic.ForceFire` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Warhead | `Versus.steel.Retaliate` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Warhead | `WarheadAnimZAdjust` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Weapon | `AmbientDamage.Warhead` | Reference | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Phobos |
| Weapon | `Beam.Amplitude` | Float | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Beam.Color` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Beam.Duration` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Beam.IsHouseColor` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Bolt.Color1` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Bolt.Color2` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Bolt.Color3` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `LaserThickness` | Integer | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Supress` | Text | `needs-more-evidence-weapon-core-20260603` | Reference gap |
| Weapon | `Wave.Color` | Text | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.IsBigLaser` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.IsHouseColor` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.IsLaser` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.ReverseAgainstAircraft` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.ReverseAgainstBuildings` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.ReverseAgainstInfantry` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.ReverseAgainstOthers` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |
| Weapon | `Wave.ReverseAgainstVehicles` | Boolean | `needs-more-evidence-residual-hover-risk-burndown-20260603` | Ares |

## FR-DQ-3E Runtime Extraction Note

- These rows are intentionally absent from `builtin-yr-ares-phobos-fallback-v3.2.fields.json` after FR-DQ-3E aggressive extraction.
- They should not be used by Hover / Quick Peek / AI Evidence until source-verified.


</details>
