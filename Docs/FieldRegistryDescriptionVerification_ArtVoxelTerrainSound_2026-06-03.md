# Field Registry Description Verification - Art / Voxel / Terrain / Sound

Phase: FR-DQ-2Y-ArtVoxelTerrainSound-MegaBatch-ManualApply

## 1. Scope

This batch uses the `FR-DQ-2X` clean source package as baseline and targets remaining direct Hover-risk rows in the visual/audio object family:

```text
ArtObject
VoxelAnim
Terrain
Sound
ParticleSystem
selected Global visual rows
```

This phase does not change provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML, project files, or legacy code.

## 2. Source Strategy

Rows are classified into three outcomes:

- `source-backed`: the current row is supported by a ModEnc / Phobos source.
- `non-canonical guardrail`: a reliable source shows the row belongs to a different section/context.
- `NeedsMoreEvidence`: no reliable field-specific source was found during this batch, so the row is converted from an old placeholder into an explicit unresolved guardrail.

## 3. Sources Used

- ModEnc `ActiveAnim`, `ActiveAnimYSort`, and `ActiveAnimZAdjust` pages for building active animation rows.
- ModEnc `Delay`, `VShift`, `Sounds`, and multi-context `Range` pages for sound-related rows.
- Phobos New / Enhanced Logics for Terrain `DestroyAnim`, amphibious transport access context, `LaserTrail.Types`, shield pip settings, and visual extension context.
- Phobos Fixed / Improved Logics for powered building animation and tint-effect related rows.

## 4. Batch Result

```text
Rows affected: 154
Source-backed rows: 14
Non-canonical guardrail rows: 8
NeedsMoreEvidence guardrail rows: 132
Direct Hover-risk rows: 627 -> 473
Direct placeholder rows: 583 -> 430
Exact integer generic rows: 44 -> 43
Exact numeric generic rows: 0 -> 0
```

## 5. Source-backed Rows

| Key | SectionKind / Schema | Quality |
|---|---|---|
| `ActiveAnimThree` | `ArtObject` | `source-verified-modenc-art-animation-20260603` |
| `ActiveAnimThreeDamaged` | `ArtObject` | `source-verified-modenc-art-animation-20260603` |
| `ActiveAnimThreePowered` | `ArtObject` | `source-verified-phobos-art-animation-20260603` |
| `ActiveAnimThreeX` | `ArtObject` | `source-verified-modenc-art-animation-20260603` |
| `ActiveAnimThreeY` | `ArtObject` | `source-verified-modenc-art-animation-20260603` |
| `ActiveAnimThreeYSort` | `ArtObject` | `source-verified-modenc-art-animation-20260603` |
| `ActiveAnimThreeZAdjust` | `ArtObject` | `source-verified-modenc-art-animation-20260603` |
| `Delay` | `Sound` | `source-verified-modenc-sound-20260603` |
| `DestroyAnim` | `Terrain` | `source-verified-phobos-terrain-20260603` |
| `ForceShield.ExtraTintIntensity` | `Global` | `source-verified-phobos-global-visual-20260603` |
| `IronCurtain.ExtraTintIntensity` | `Global` | `source-verified-phobos-global-visual-20260603` |
| `LaserTrail.Types` | `VoxelAnim` | `source-verified-phobos-voxelanim-20260603` |
| `Sounds` | `Sound` | `source-verified-modenc-sound-20260603` |
| `VShift` | `Sound` | `source-verified-modenc-sound-20260603` |

## 6. Non-canonical Guardrail Rows

| Key | SectionKind / Schema | Quality |
|---|---|---|
| `AmphibiousEnter` | `Terrain` | `noncanonical-guardrail-phobos-terrain-20260603` |
| `AmphibiousUnload` | `Terrain` | `noncanonical-guardrail-phobos-terrain-20260603` |
| `Pips.Shield` | `VoxelAnim` | `noncanonical-guardrail-phobos-voxelanim-20260603` |
| `Pips.Shield.Background` | `VoxelAnim` | `noncanonical-guardrail-phobos-voxelanim-20260603` |
| `Pips.Shield.Building` | `VoxelAnim` | `noncanonical-guardrail-phobos-voxelanim-20260603` |
| `Pips.Shield.Building.Empty` | `VoxelAnim` | `noncanonical-guardrail-phobos-voxelanim-20260603` |
| `Shield.ConditionRed` | `VoxelAnim` | `noncanonical-guardrail-phobos-voxelanim-20260603` |
| `Shield.ConditionYellow` | `VoxelAnim` | `noncanonical-guardrail-phobos-voxelanim-20260603` |

## 7. NeedsMoreEvidence Rows

These rows were previously direct Hover-risk placeholders or generic labels. They now use explicit unresolved guardrail text and are also tracked in `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.

| Key | SectionKind / Schema | Quality |
|---|---|---|
| `AddOccupy4` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `AdjacentWallDamage` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `AircraftLevelLightMultiplier` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `AirDeathFalling` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `AirDeathFinish` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `AirDeathStart` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `AltCameo` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `AnimationLength` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `ArtImageSwap` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `AttachedParticleSystem` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `Attack` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `BalloonHoverDampen` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `BibShape` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Bouncer` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Buildup` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Cameo` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `CameoPalette` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `CanBeHidden` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `CanHideThings` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Cheer` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `ChronoSparkleBuildingDisplayPositions` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `ChronoSparkleDisplayDelay` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `ConditionYellow.Terrain` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `Control` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `Crater` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Crawl` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Crawls` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `CrawlSounds` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DamageFireOffset3` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DamageLevels` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DamageRadius` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DelayedFireDelay` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DemandLoad` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DemandLoadBuildup` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Deploy` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Deployed` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DetailLevel` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Die1` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Die2` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Die3` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Die4` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Die5` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DisableShadowCache` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `DockingOffset3` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Down` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Elasticity` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `EliteSecondaryFireFLH` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `ExpireAnim` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `FireFly` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `FireProne` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `FireUp` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `FiringFrames` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Flat` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Fly` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `ForceBigCraters` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `FreeBuildup` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `FShift` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `Gas.MaxDriftSpeed` | `ParticleSystem` | `needs-more-evidence-particlesystem-visual-megabatch-20260603` |
| `Guard` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Height` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Hover` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Idle2` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `IsAnimDelayedFire` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `IsMeteor` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `JumpjetLevelLightMultiplier` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `Layer` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Limit` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `LineTrailColor` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `LineTrailColorDecrement` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `MaxXYVel` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `MaxZVel` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `MetallicDebris` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `MinimapColor` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `MinVolume` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `NumParticles` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Palette` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `Paradrop` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `PowerUp3Anim` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `PowerUp3AnimDamaged` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `PowerUp3LocX` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `PowerUp3LocY` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `PowerUp3YSort` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Priority` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `ProductionAnimY` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `PsiWarning` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `PsychicRevealRadius` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `QueueingCell` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Ready` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Recoilless` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `RemoveOccupy4` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Rotates` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Scorch` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `SecondaryFirePixelOffset` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `SecondarySpawnOffset` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `ShadowIndex` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Smoke` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `SpawnDelay` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `SpawnsParticle` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `SpawnsTiberium.CellsPerAnim` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `SpawnsTiberium.GrowthStage` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `SpawnsTiberium.Range` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `SpawnsTiberium.Type` | `Terrain` | `needs-more-evidence-terrain-visual-megabatch-20260603` |
| `SpecialAnimThreeYSort` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Sticky` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `SuperAnimY` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Swim` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `TerrainPalette` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `TiberiumSpawnRadius` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `TiberiumSpawnType` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `ToOverlay` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Trailer.SpawnDelay` | `VoxelAnim` | `needs-more-evidence-voxelanim-visual-megabatch-20260603` |
| `Translucency` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Tread` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Tumble` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `TurretOffset` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Type` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `Undeploy` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Up` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `UseLineTrail` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Volume` | `Sound` | `needs-more-evidence-art-sound-megabatch-20260603` |
| `Voxel` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `Walk` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `WalkFrames` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `WarpAway` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `WarpOut` | `Global` | `needs-more-evidence-global-visual-megabatch-20260603` |
| `WetAttack` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `WetDie1` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `WetDie2` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `WetIdle1` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `WetIdle2` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `YSortAdjust` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |
| `ZSortAdjust` | `ArtObject` | `needs-more-evidence-artobject-visual-megabatch-20260603` |

## 8. Context Notes

- `ActiveAnimThree* / ArtObject` rows are source-backed as building active-animation variants, but other art sequence rows remain unresolved unless a field-specific source was found.
- `AmphibiousEnter` / `AmphibiousUnload` in `Terrain` are guarded because Phobos documents them under `[General]` defaults and `VehicleType` transport overrides, not `TerrainType`.
- `Pips.Shield* / VoxelAnim` rows are guarded because Phobos documents them under `[AudioVisual]` / `[ShieldType]`, not `VoxelAnim`.
- `LaserTrail.Types / VoxelAnim` is source-backed because Phobos explicitly lists it under `[SOMEVOXELANIM]`.
- Broad Global visual rows without field-specific support are kept as NeedsMoreEvidence guardrails rather than invented descriptions.

## 9. Next Step

Recommended next phase:

```text
FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply
```

Priority should be the remaining direct Hover-risk rows in `AttachEffect`, `Shield`, `LaserTrail`, `DigitalDisplay`, `Insignia`, and `Radiation`.
