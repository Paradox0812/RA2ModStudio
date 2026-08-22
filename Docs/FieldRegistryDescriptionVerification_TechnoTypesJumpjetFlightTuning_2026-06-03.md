# Field Registry Description Verification - TechnoTypes Jumpjet / Flight Tuning

Phase: FR-DQ-2O-TechnoTypes-JumpjetAndFlightTuning-ManualApply

## 1. Scope

This batch verifies jumpjet / flight tuning / movement acceleration rows in BuiltIn v3.2. It keeps the source-family batching model and only updates Field Registry data, regression assertions, and documentation.

Processed keys:

```text
JumpjetTurnRate
JumpjetSpeed
JumpjetClimb
JumpjetCrash
JumpjetHeight
JumpjetAccel
JumpjetWobbles
JumpjetNoWobbles
JumpjetDeviation
SlowdownDistance
AccelerationFactor
DeaccelerationFactor
Weight
PhysicalSize
```

## 2. Source Summary

| Source | Trust | Usage |
|---|---|---|
| ModEnc Jumpjet flags | Community | Confirms RA2/YR per-unit Jumpjet parameter group and case-sensitive Jumpjet spelling caveat. |
| ModEnc JumpjetSpeed / JumpjetClimb / JumpjetAccel / JumpjetHeight / JumpjetWobbles / JumpjetNoWobbles / JumpjetDeviation / JumpjetCrash | Community | Confirms jumpjet movement parameter meanings and InfantryTypes / VehicleTypes / AircraftTypes applicability. |
| ModEnc SlowdownDistance / AccelerationFactor / DeaccelerationFactor | Community | Confirms movement acceleration/deceleration fields and AircraftTypes / InfantryTypes / VehicleTypes applicability. |
| ModEnc Weight | Community | Confirms VehicleTypes-only voxel vehicle weight semantics. |
| ModEnc PhysicalSize | Community with caveat | Confirms InfantryType display Z-fudge semantics; source page is a DeeZire-derived completeness entry. |
| Phobos Fixed / Improved Logics | Official extension docs | Confirms Warhead-level Jumpjet locomotor parameter overrides for `IsLocomotor=yes` / `Locomotor=Jumpjet`. |

## 3. Verification Matrix

| Key | Context | Result | Description policy |
|---|---|---|---|
| JumpjetTurnRate | Aircraft / Infantry / Vehicle | Verified | Source-backed jumpjet turn-rate row. |
| JumpjetTurnRate | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetTurnRate | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetSpeed | Aircraft / Infantry / Vehicle | Verified | Source-backed jumpjet speed row. |
| JumpjetSpeed | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetSpeed | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetClimb | Aircraft / Infantry / Vehicle | Verified | Source-backed climb / descent speed row. |
| JumpjetClimb | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetClimb | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetCrash | Aircraft / Infantry / Vehicle | Verified | Source-backed crash descent speed row with building-top caveat. |
| JumpjetCrash | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetCrash | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetHeight | Aircraft / Infantry / Vehicle | Verified | Source-backed jumpjet cruise height row. |
| JumpjetHeight | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetHeight | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetAccel | Aircraft / Infantry / Vehicle | Verified | Source-backed jumpjet acceleration / deceleration row with landing caveat. |
| JumpjetAccel | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetAccel | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetWobbles | Aircraft / Infantry / Vehicle | Verified | Source-backed wobble frequency row. |
| JumpjetWobbles | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetWobbles | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetNoWobbles | Aircraft / Infantry / Vehicle | Verified | Source-backed wobble disable row; warns not to use JumpjetWobbles=0. |
| JumpjetNoWobbles | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetNoWobbles | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| JumpjetDeviation | Aircraft / Infantry / Vehicle | Verified | Source-backed wobble amplitude row. |
| JumpjetDeviation | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| JumpjetDeviation | Warhead | Verified Phobos extension | Warhead override for IsLocomotor + Jumpjet. |
| SlowdownDistance | Aircraft / Infantry / Vehicle | Verified | Source-backed slowdown distance row. |
| SlowdownDistance | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| AccelerationFactor | Aircraft / Infantry / Vehicle | Verified | Source-backed movement acceleration row. |
| AccelerationFactor | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| DeaccelerationFactor | Aircraft / Infantry / Vehicle | Verified | Source-backed movement deceleration row. |
| DeaccelerationFactor | Techno | Broad fallback | Keeps source-backed broad fallback with exact-context warning. |
| Weight | Vehicle | Verified | Source-backed voxel VehicleTypes weight row. |
| Weight | Techno | Guardrail | Prevents broad Techno Hover pollution; not canonical for Infantry/Aircraft/Building. |
| PhysicalSize | Infantry | Verified with source caveat | Source-backed InfantryType display Z-fudge row. |
| PhysicalSize | Techno | Guardrail | Prevents broad Techno Hover pollution; not canonical for other objects. |

## 4. Result Summary

```text
BuiltIn v3.2 field count: 4928 -> 4965
New exact/context rows: 37
Updated / guarded existing rows: 27
Target rows with direct placeholder / generic labels: 0
Exact `数值型字段` rows: 0
Exact `整数型字段` rows: 99
```

## 5. Boundary Notes

- `Jumpjet* / Techno` rows are broad fallbacks; exact Hover rows were added for Aircraft, Infantry, and Vehicle.
- `Jumpjet* / Warhead` rows are Phobos extension rows for Warheads that use `IsLocomotor=yes` and `Locomotor=Jumpjet`; they are not original RA2/YR Techno fields.
- `Weight` is VehicleTypes-only; broad Techno is a guardrail.
- `PhysicalSize` is InfantryType display Z-fudge; broad Techno is a guardrail.
- Provider priority, lookup/fallback/enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, and legacy files were not changed.

## 6. Next Step

Recommended next phase:

```text
FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply
```

Candidate fields:

```text
Storage
PipScale
Pip
Points
Bunkerable
IFVMode
Crushable
Crusher
OmniCrusher
OmniCrushResistant
CrushSound
CrushableLevel
CrusherLevel
```
