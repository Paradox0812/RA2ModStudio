# Field Registry Description Verification - Weapon Core Big Batch

Phase: FR-DQ-2R-WeaponCore-BigBatch-ManualApply

## 1. Scope

This batch verifies source-backed Hover descriptions for core WeaponType fields and closely related Phobos WeaponType extensions.

Primary key scope:

```text
Damage, ROF, Range, MinimumRange, Projectile, Warhead, Report, Anim, Bright, Lobber,
CellRangefinding, RevealOnFire, AreaFire, LimboLaunch, Suicide, TurboBoost, Supress,
Burst, FireOnce, DecloakToFire, OmniFire
```

Extended same-domain scope:

```text
ROF.RandomDelay, Burst.Delays, ChargeTurret.Delays, DiskLaser.Radius, Bolt.Arcs,
DelayedFire.*, ExtraRange.*, ExtraWarheads.*, KeepRange.*, AreaFire.Target,
Strafing.*, VisualScatter.*, CanTarget*, AutoTarget.IronCurtained,
OmniFire.TurnToTarget, CylinderRangefinding, KickOutPassengers, AttackNoThreatBuildings
```

## 2. Sources Used

- ModEnc Damage / ROF / Range / Projectile / Warhead / Report / Anim / Bright / Lobber / CellRangefinding pages.
- ModEnc RevealOnFire / AreaFire / LimboLaunch / Suicide / TurboBoost / Burst / FireOnce / DecloakToFire / OmniFire pages.
- Phobos documentation: New / Enhanced Logics and Fixed / Improved Logics weapon extension sections.

## 3. Verified Canonical Rows

Representative source-backed canonical rows:

| Key | SectionKind | Result |
|---|---|---|
| Damage | Weapon | Weapon base damage, modified by Warhead logic. |
| Damage | Animation | art(md).ini animation frame damage. |
| Damage | VoxelAnim | debris / voxel animation damage. |
| ROF | Weapon | Weapon rearm delay in frames with burst / modifier caveats. |
| Range | Weapon | Weapon range in cells with Range=-2 caveats. |
| Range | Sound | sound(md).ini propagation radius. |
| Range | SuperWeapon | superweapon targeting indicator radius only. |
| MinimumRange | Weapon | minimum allowed firing distance. |
| Projectile | Weapon | reference to Projectile section. |
| Warhead | Weapon | reference to Warhead section used on impact. |
| Warhead | Animation | animation Damage warhead caveat. |
| Report | Weapon | firing SoundList. |
| Report | Animation | animation sound cue. |
| Anim | Weapon | muzzle flash animation. |
| Bright | Weapon / Warhead | combat light / flash effect. |
| Lobber | Weapon | high arc projectile behavior. |
| CellRangefinding | Weapon | cell-center range calculation. |
| RevealOnFire | Weapon | reveal firing unit under shroud. |
| AreaFire | Weapon | detonate Warhead on firer cell. |
| LimboLaunch | Weapon | put firer in Limbo state when firing. |
| Suicide | Weapon | firer self-destructs instead of normal firing. |
| TurboBoost | Weapon | weapon opts into missile speed boost vs air targets. |
| TurboBoost | Global | [CombatDamage] speed multiplier, obsolete parsed no-op caveat. |
| Burst / FireOnce / DecloakToFire / OmniFire | Weapon | existing weapon rows refreshed with current ModEnc-backed wording. |
| Phobos Weapon extensions | Weapon | timing, range, targeting, strafing and visual scatter options source-backed from Phobos docs. |

## 4. Guardrail / Non-canonical Rows

The following row families were converted away from placeholder or misleading old extracted text:

- Weapon-only keys under `Techno` rows: `Projectile`, `MinimumRange`, `Lobber`, `RevealOnFire`, `AreaFire`, `LimboLaunch`, `Burst`, `FireOnce`, `DecloakToFire`, `OmniFire`, etc.
- Multi-meaning rows under wrong broad contexts: `Damage / Global`, `Damage / ArtObject`, `Warhead / Techno`, `Warhead / ArtObject`, `Report / Global`, `Report / ArtObject`.
- `Suicide / AI` is guarded as TeamTypes / Weapons semantics, not a plain `[AI]` section field.
- `Bolt.Arcs / LaserTrail` remains NeedsMoreEvidence because the verified Phobos source covers Weapon electric bolt customization, not LaserTrail semantics.

## 5. Needs More Evidence

- `Supress / Weapon`: legacy extracted text suggests a friendly-target auto-acquire suppression behavior, but no reliable ModEnc field page was found during this batch.
- `Bolt.Arcs / LaserTrail`: kept as unresolved guardrail for a later Ares / Phobos visual-effects batch.

## 6. Data Change Summary

```text
BuiltIn v3.2 field count: 5030 -> 5036
Rows affected in this batch: 104
New exact/context rows: 6
Updated / guarded existing rows: 98
Source-verified rows: 826 -> 919
Direct placeholder rows: 2268 -> 2227
Exact integer generic rows: 99 -> 94
Exact numeric generic rows: 0 -> 0
Direct Hover-risk rows: 2367 -> 2321
```

No provider priority, runtime lookup, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project file, or legacy code was changed.

## 7. Validation

Static validation performed in patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Target bad placeholder rows: 0
Expected verification doc: present
Clean package validation: passed
```

`dotnet restore`, `dotnet build`, and `dotnet test` were not run because the patch environment has no dotnet CLI.

## 8. Next Step

Recommended next phase:

```text
FR-DQ-2S-WarheadCore-BigBatch-ManualApply
```

Suggested scope:

```text
Verses, CellSpread, PercentAtMax, Wood, Wall, Rocker, AnimList, InfDeath,
Conventional, Tiberium, ProneDamage, Sparky, Fire, Bright,
CLDisableRed, CLDisableGreen, CLDisableBlue, AffectsAllies, AffectsOwner,
CombatLightSize, ShakeXlo, ShakeXhi, ShakeYlo, ShakeYhi
```
