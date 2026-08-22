# Field Registry Description Verification - Projectile Core

Phase: FR-DQ-2T-ProjectileCore-BigBatch-ManualApply

This document records the source-backed Projectile Core batch applied after `FR-DQ-2S-WarheadCore-BigBatch-ManualApply`.

## 1. Scope

Processed field family:

```text
AA
AG
ROT
Image
Shadow
Proximity
Ranged
Arcing
Inaccurate
FlakScatter
SubjectToCliffs
SubjectToElevation
SubjectToWalls
SubjectToBuildings
SubjectToTrenches
Acceleration
Vertical
Dropping
Arm
CourseLockDuration
Scalable
Interceptable
SubjectToGround
SubjectToLand
SubjectToLand.Detonate
SubjectToWater
SubjectToWater.Detonate
Trajectory
Trajectory.Bombard.*
Trajectory.Parabola.*
Trajectory.Straight.*
```

This batch intentionally combines vanilla / RA2-YR projectile core flags with the same-domain Ares and Phobos projectile collision / interception / trajectory extensions.

## 2. Sources Used

- ModEnc `Projectile`, `AA`, `AG`, `ROT`, `Image`, `Shadow`, `Proximity`, `Ranged`, `Arcing`, `Inaccurate`, `FlakScatter`, `SubjectToCliffs`, `SubjectToElevation`, `SubjectToWalls`, `Acceleration`, `Vertical`, `Dropping`, `Arm`, `CourseLockDuration`, and `Scalable` pages.
- ModEnc / Ares `SubjectToBuildings` and Ares Urban Combat / Trenches documentation for `SubjectToTrenches`.
- Phobos New / Enhanced Logics documentation for `Interceptable`, `SubjectToGround`, `SubjectToLand`, `SubjectToWater`, and `Trajectory.*`.

## 3. Canonical Projectile Rows

Updated or added canonical Projectile rows include:

```text
AA / Projectile
AG / Projectile
ROT / Projectile
Image / Projectile
Shadow / Projectile
Proximity / Projectile
Ranged / Projectile
Arcing / Projectile
Inaccurate / Projectile
FlakScatter / Projectile
SubjectToCliffs / Projectile
SubjectToElevation / Projectile
SubjectToWalls / Projectile
SubjectToBuildings / Projectile
SubjectToTrenches / Projectile
Acceleration / Projectile
Vertical / Projectile
Dropping / Projectile
Arm / Projectile
CourseLockDuration / Projectile
Scalable / Projectile
```

Notes:

- `AA` and `AG` remain Projectile flags; Weapon / Techno rows are guardrails only.
- `ROT` is valid on both Projectiles and TechnoTypes, but the two meanings differ. Projectile `ROT` controls homing / tracking behavior.
- `Image` and `Shadow` are multi-context fields; Projectile descriptions are now specific to projectile image / shadow behavior.
- `SubjectToBuildings` is Ares-only and `SubjectToTrenches` is Ares Urban Combat logic.
- `Acceleration` is multi-context; the Projectile row specifically describes projectile acceleration, not JumpjetControls.

## 4. Phobos Projectile Extension Rows

Updated source-backed Phobos Projectile rows include:

```text
Interceptable
Interceptable.DeleteOnIntercept
Interceptable.WeaponOverride
SubjectToGround
SubjectToLand
SubjectToLand.Detonate
SubjectToWater
SubjectToWater.Detonate
Trajectory
Trajectory.Speed
Trajectory.Bombard.*
Trajectory.Parabola.*
Trajectory.Straight.*
```

Notes:

- `Trajectory.Bombard.*`, `Trajectory.Parabola.*`, and `Trajectory.Straight.*` remain Projectile-only Phobos extensions.
- Several formerly generic `整数型字段` or `布尔字段` rows were rewritten with source-backed trajectory-family descriptions.
- Floating point / double-like Phobos trajectory settings were normalized to `Float` where the documentation indicates floating point or double values.

## 5. Guardrail Rows

The following wrong-context rows were not deleted. They were rewritten as non-canonical guardrails to prevent old tutorial extraction or broad fallback rows from polluting Hover:

```text
AA / Techno
AA / Weapon
AG / Techno
AG / Weapon
Image / Weapon
Shadow / Weapon
Proximity / Techno
Proximity / Weapon
Ranged / Techno
Ranged / Weapon
Arcing / Techno
Arcing / Weapon
Inaccurate / Techno
Inaccurate / Weapon
FlakScatter / Techno
FlakScatter / Weapon
SubjectToCliffs / Techno
SubjectToCliffs / Weapon
SubjectToElevation / Techno
SubjectToElevation / Weapon
SubjectToWalls / Techno
SubjectToWalls / Weapon
Acceleration / Techno
Acceleration / Weapon
Vertical / Techno
Vertical / Weapon
Dropping / Techno
Dropping / Weapon
Arm / Techno
Arm / Weapon
CourseLockDuration / Techno
CourseLockDuration / Weapon
Scalable / Techno
Scalable / Weapon
```

## 6. Result Summary

```text
BuiltIn v3.2 field count: 5051
Rows affected in FR-DQ-2T: 136
New exact/context rows: 7
Updated / guarded existing rows: 129
Source-verified rows: 1158
Strict non-source-verified rows: 3893
Direct placeholder rows: 2138
Exact integer generic rows: 81
Exact numeric generic rows: 0
Direct Hover-risk placeholder/generic rows: 2219
```

The processed Projectile rows no longer expose:

```text
原始英文说明已移至复核表
不直接用于 Hover
不能直接用于 Hover
布尔字段
整数型字段
数值型字段
```

## 7. Non-goals

This batch did not change:

```text
Field Registry provider priority
provider lookup / fallback / enrichment
Hover code
Quick Peek
AI Evidence
PromptBuilder
parser / diagnostics / completion / save preflight
XAML / UI
project files
legacy editor files
```

## 8. Next Step

Recommended next phase:

```text
FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply
```

Suggested scope:

```text
Airburst.*
AirburstSpread
AirburstWeapon.*
BallisticScatter.*
ClusterScatter.*
Gravity
Parachuted.*
Retarget*
ReturnWeapon*
Shrapnel.*
Splits.*
Trajectory.Disperse.*
Trajectory.Meteor.*
Trajectory.Spiral.*
```
