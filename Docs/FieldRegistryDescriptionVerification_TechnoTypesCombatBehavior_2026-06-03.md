# Field Registry Description Verification - TechnoTypes Combat Behavior / Immunity

Phase: FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply

This document records the source-backed verification and manual Field Registry update for TechnoTypes cloak, radar, sensor, disguise, and immunity behavior fields.

## 1. Scope

Scanned and updated BuiltIn v3.2 rows for:

```text
Cloakable
CloakingSpeed
RadarInvisible
Sensors
SensorsSight
DetectDisguise
DisguiseWhenStill
CanDisguise
PermaDisguise
ImmuneToVeins
ImmuneToRadiation
ImmuneToPsionics
ImmuneToPsionicWeapons
ImmuneToPoison
TypeImmune
```

This phase updated only BuiltIn field descriptions, editor kinds, schemas, source metadata, tests, and documentation. It did not change provider priority, lookup/fallback/enrichment logic, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML, project files, or legacy code.

## 2. Source Summary

| Key | Source | Verification summary |
|---|---|---|
| Cloakable | https://modenc.renegadeprojects.com/Cloakable | Applies to InfantryTypes, VehicleTypes, and BuildingTypes. Controls whether the object has a cloaking device and how it appears to enemies / minimap. |
| CloakingSpeed | https://modenc.renegadeprojects.com/CloakingSpeed | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls duration in frames for each cloak / decloak stage, with 1 fastest and 10 slowest. |
| RadarInvisible | https://modenc.renegadeprojects.com/RadarInvisible | Applies to TechnoTypes and broader ObjectTypes. BuiltIn v3.2 updates the Techno and object-context rows only; non-Techno ObjectTypes remain outside this batch. |
| Sensors | https://modenc.renegadeprojects.com/Sensors | Applies to TechnoTypes in TS and later. Reveals nearby cloaked objects; independent from SensorsSight with airborne / SensorArray caveats. |
| SensorsSight | https://modenc.renegadeprojects.com/SensorsSight | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Reveals enemy cloaked and subterranean units within a radius; building behavior requires caveats. |
| DetectDisguise | https://modenc.renegadeprojects.com/DetectDisguise | Applies to TechnoTypes. Determines whether the object automatically identifies units using CanDisguise=yes. |
| CanDisguise | https://modenc.renegadeprojects.com/CanDisguise | Applies to TechnoTypes. Enables spy / mirage-style disguise logic; practical use is mainly InfantryTypes and VehicleTypes with PermaDisguise / DisguiseWhenStill caveats. |
| DisguiseWhenStill | https://modenc.renegadeprojects.com/DisguiseWhenStill | Applies to VehicleTypes. Allows voxel vehicles with CanDisguise=yes to disguise as terrain styles while stationary. |
| PermaDisguise | https://modenc.renegadeprojects.com/PermaDisguise | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls whether movement cancels disguise for CanDisguise=yes objects. |
| ImmuneToVeins | https://modenc.renegadeprojects.com/ImmuneToVeins | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls immunity to Veinhole=yes warhead effects; logic is obsolete in RA2/YR. |
| ImmuneToRadiation | https://modenc.renegadeprojects.com/ImmuneToRadiation | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls immunity to Radiation=yes warhead effects and does not affect targeting. |
| ImmuneToPsionics | https://modenc.renegadeprojects.com/ImmuneToPsionics | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls immunity to mind-control / psychedelic / PsychicDominator effects. |
| ImmuneToPsionicWeapons | https://modenc.renegadeprojects.com/ImmuneToPsionicWeapons | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Overrides immunity against PsychicDamage=yes warheads and does not affect targeting. |
| ImmuneToPoison | https://modenc.renegadeprojects.com/ImmuneToPoison | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls immunity to Poison=yes warhead effects and does not affect targeting. |
| TypeImmune | https://modenc.renegadeprojects.com/TypeImmune | Applies to InfantryTypes, VehicleTypes, and BuildingTypes. Makes objects immune to damage from same-identity, same-owner units; ownership changes can alter behavior. |

## 3. Applied Matrix

| Key | Applied contexts | Editor kind | Result |
|---|---|---|---|
| Cloakable | Techno, Infantry, Vehicle, Building | Boolean | Updated Techno broad row and added exact non-Aircraft object rows. |
| CloakingSpeed | Techno, Aircraft, Building, Infantry, Vehicle | Integer | Updated broad row and added exact object-context rows. |
| RadarInvisible | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated Techno/object rows; broader ObjectTypes are left for a later ObjectTypes batch. |
| Sensors | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad row and added exact object-context rows with airborne / building caveats. |
| SensorsSight | Techno, Aircraft, Building, Infantry, Vehicle | Integer | Updated broad row and added exact object-context rows. |
| DetectDisguise | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad row and added exact object-context rows. |
| CanDisguise | Techno, Infantry, Vehicle | Boolean | Updated broad row and added exact practical Infantry / Vehicle rows. |
| DisguiseWhenStill | Techno, Vehicle | Boolean | Converted Techno row to a VehicleTypes guardrail and added exact Vehicle row. |
| PermaDisguise | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad row and added exact object-context rows. |
| ImmuneToVeins | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad row and added exact object-context rows. |
| ImmuneToRadiation | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad row and added exact object-context rows. |
| ImmuneToPsionics | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad row and added exact object-context rows. |
| ImmuneToPsionicWeapons | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad row and added exact object-context rows. |
| ImmuneToPoison | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Added missing broad and exact object-context rows. |
| TypeImmune | Techno, Infantry, Vehicle, Building | Boolean | Converted Techno row to a non-Aircraft guardrail and added exact rows. |

## 4. Non-canonical / Guardrail Decisions

- `Cloakable / Techno` is retained as a broad fallback but warns that the verified applicability is InfantryTypes, VehicleTypes, and BuildingTypes, not AircraftTypes.
- `DisguiseWhenStill / Techno` is a broad guardrail; the exact verified context is VehicleTypes.
- `TypeImmune / Techno` is a broad guardrail; exact verified contexts are InfantryTypes, VehicleTypes, and BuildingTypes.
- `RadarInvisible` also exists for broader ObjectTypes, but this batch only updated TechnoTypes and object-context Techno rows. Overlay / Projectile / Animation-related rows should be handled in a later ObjectTypes batch.
- `Cloakable / AttachEffect` was not modified because AttachEffect semantics belong to a later extension-field batch, not the TechnoTypes combat behavior batch.

## 5. Result Summary

```text
BuiltIn v3.2 field count: 4808 -> 4862
New exact/context rows: 54
Updated existing rows: 14
Target rows with direct placeholder / generic labels: 0
Exact `数值型字段` rows: 0
Exact `整数型字段` rows: 99
```

## 6. Next Step

Recommended next phase:

```text
FR-DQ-2M-TechnoTypes-WeaponTargeting-ManualApply
```

Suggested field family:

```text
OmniFire
DistributedFire
FireAngle
CanPassiveAquire
CanRetaliate
PreventAttackMove
NoAutoFire
Passive
LandTargeting
NavalTargeting
FireOnce
Burst
DecloakToFire
UseFireParticles
```
