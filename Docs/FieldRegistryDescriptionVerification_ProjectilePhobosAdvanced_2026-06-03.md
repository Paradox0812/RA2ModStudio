# Field Registry Description Verification - Projectile Phobos Advanced

Phase: FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply  
Date: 2026-06-03

## 1. Scope

This batch verifies the advanced Projectile family after FR-DQ-2T Projectile Core. It focuses on Airburst / Splits, scatter / gravity, parachuted projectiles, return weapons, and shrapnel extensions.

Processed keys:

```text
Airburst
AirburstWeapon
Airburst.*
AirburstSpread
AroundTarget
Splits
Splits.*
RetargetAccuracy
RetargetSelf
RetargetSelf.Probability
BallisticScatter
BallisticScatter.Min / Max
ClusterScatter.Min / Max
Gravity
Parachuted
Parachuted.FallRate
Parachuted.MaxFallRate
BombParachute
ReturnWeapon
ReturnWeapon.ApplyFirepowerMult
ShrapnelWeapon
ShrapnelCount
Shrapnel.*
AirstrikeLineColor / AISuperWeaponDelay wrong Projectile guardrails
```

This batch does not modify provider priority, lookup, fallback, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy files.

## 2. Sources Used

- ModEnc Airburst / AirburstWeapon / ShrapnelWeapon / ShrapnelCount / BallisticScatter / Parachuted pages.
- Ares Splits and Airburst documentation.
- Phobos Fixed / Improved Logics documentation, especially Projectile Airburst & Splits, Cluster scatter, Gravity, Return weapon, and Shrapnel enhancements.
- Phobos New / Enhanced Logics documentation, especially Parabombs and related projectile extension examples.

## 3. Canonical Projectile Rows

The following rows were added or updated as source-backed Projectile rows:

```text
Airburst / Projectile
AirburstWeapon / Projectile
Airburst.RandomClusters / Projectile
Airburst.TargetAsSource / Projectile
Airburst.TargetAsSource.SkipHeight / Projectile
Airburst.UseCluster / Projectile
AirburstSpread / Projectile
AirburstWeapon.ApplyFirepowerMult / Projectile
AirburstWeapon.SourceScatterMin / Projectile
AirburstWeapon.SourceScatterMax / Projectile
AirburstWeapon.UseFiringEffects / Projectile
AroundTarget / Projectile
Splits / Projectile
RetargetAccuracy / Projectile
RetargetSelf / Projectile
RetargetSelf.Probability / Projectile
Splits.TargetingDistance / Projectile
Splits.TargetingDistance.Cylindrical / Projectile
Splits.TargetCellRange / Projectile
Splits.AllowRepeatTargets / Projectile
Splits.UseWeaponTargeting / Projectile
BallisticScatter.Min / Projectile
BallisticScatter.Max / Projectile
ClusterScatter.Min / Projectile
ClusterScatter.Max / Projectile
Gravity / Projectile
Parachuted / Projectile
Parachuted.FallRate / Projectile
Parachuted.MaxFallRate / Projectile
BombParachute / Projectile
ReturnWeapon / Projectile
ReturnWeapon.ApplyFirepowerMult / Projectile
ShrapnelWeapon / Projectile
ShrapnelCount / Projectile
Shrapnel.AffectsGround / Projectile
Shrapnel.AffectsBuildings / Projectile
Shrapnel.UseWeaponTargeting / Projectile
Shrapnel.IgnoreHitBuildings / Projectile
```

## 4. Guardrail Rows

The following wrong-context or broad fallback rows were converted to guardrails instead of being deleted:

```text
Airburst / Techno
Airburst / Weapon
AirburstWeapon / Techno
AirburstWeapon / Weapon
BallisticScatter / Techno
Gravity / Global
Gravity / Techno
Parachuted / Weapon
RetargetAccuracy / Techno
ShrapnelWeapon / Techno
ShrapnelWeapon / Weapon
ShrapnelCount / Techno
ShrapnelCount / Weapon
AirstrikeLineColor / Projectile
AirstrikeLineColor / Techno
AISuperWeaponDelay / Projectile
BombParachute / Techno
```

Important distinctions:

- `Airburst`, `AirburstWeapon`, `Splits`, `Retarget*`, `AroundTarget`, `ClusterScatter.*`, `Gravity`, `Parachuted*`, `ReturnWeapon*`, and `Shrapnel*` are Projectile-context logic in this batch.
- `BallisticScatter / Global` remains source-backed as `[CombatDamage]` global scatter configuration, while `BallisticScatter.Min/Max` are Phobos Projectile overrides.
- `AirstrikeLineColor` and `AISuperWeaponDelay` are not Projectile fields and were only guarded in wrong contexts.

## 5. Result Summary

```text
BuiltIn v3.2 field count: 5051 -> 5055
Rows affected in FR-DQ-2U: 56
New exact/context rows: 4
Updated / guarded existing rows: 52
Source-verified rows: 1158 -> 1212
Strict non-source-verified rows: 3893 -> 3843
Direct placeholder rows: 2138 -> 2115
Exact integer generic rows: 81 -> 80
Exact numeric generic rows: 0 -> 0
Direct Hover-risk placeholder/generic rows: 2219 -> 2195
```

## 6. Remaining Work Snapshot

After this batch:

```text
BuiltIn v3.2 total rows: 5055
Source-verified rows: 1212
Strict non-source-verified rows: 3843
Direct placeholder rows: 2115
Exact integer generic rows: 80
Exact numeric generic rows: 0
Direct Hover-risk placeholder/generic rows: 2195
```

The practical high-priority remainder is the direct Hover-risk set: 2195 rows. The stricter 3843-row remainder includes many auto-extracted rows that are not direct placeholder/generic Hover pollution and can be handled later.

## 7. Validation

Static validation performed in the patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Target bad placeholder rows: 0
Expected verification doc: present
Clean package validation: passed
```

`dotnet restore`, `dotnet build`, and `dotnet test` were not run because the patch environment does not provide the dotnet CLI.

## 8. Next Step

Recommended next phase:

```text
FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply
```

Suggested focus:

```text
Image
Normalized
Theater
LoopStart
LoopEnd
LoopCount
Rate
RandomRate
Start
End
Trailer
TrailerAnim
TrailerSeperation
Spawns
SpawnCount
Damage
Warhead
Report
Shadow
Translucent
UseNormalLight
AltPalette
AnimPalette
Rate
Next
```

This should continue the 80-140 row workflow while staying inside a single Art/Animation semantic domain.
