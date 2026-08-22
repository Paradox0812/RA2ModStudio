# Field Registry Description Verification - TechnoTypes Production / Veterancy

Phase: FR-DQ-2K-TechnoTypes-ProductionVeterancy-ManualApply

This document records the source-backed verification and manual Field Registry update for the TechnoTypes production, crate, veterancy, bounty, protection, and behavior batch.

## 1. Scope

Scanned and updated BuiltIn v3.2 rows for:

```text
AllowedToStartInMultiplayer
CrateGoodie
Trainable
Insignificant
NoMovingFire
OpportunityFire
ToProtect
ThreatAvoidanceCoefficient
Soylent
Bounty
VeteranAbilities
EliteAbilities
```

This phase updated only BuiltIn field descriptions, editor kinds, schemas, source metadata, tests, and documentation. It did not change provider priority, lookup/fallback/enrichment logic, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML, project files, or legacy code.

## 2. Source Summary

| Key | Source | Verification summary |
|---|---|---|
| AllowedToStartInMultiplayer | https://modenc.renegadeprojects.com/AllowedToStartInMultiplayer | Applies to VehicleTypes and InfantryTypes. Controls whether units are eligible as starting units in skirmish or multiplayer; still limited by Side, TechLevel, and related restrictions. |
| CrateGoodie | https://modenc.renegadeprojects.com/CrateGoodie | Applies to VehicleTypes. Defines whether a vehicle can be selected by the Unit CrateType random drawing process. |
| Trainable | https://modenc.renegadeprojects.com/Trainable | Applies to TechnoTypes: AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls whether the object can be upgraded by experience. |
| Insignificant | https://modenc.renegadeprojects.com/Insignificant | Applies to TechnoTypes and broader ObjectTypes. Makes objects ignored for scoring / kill purposes and affects active targeting / triggers with caveats. |
| NoMovingFire | https://modenc.renegadeprojects.com/NoMovingFire | Applies to VehicleTypes. The page records TS behavior caveats; the row is retained as a VehicleTypes guardrail-quality reference. |
| OpportunityFire | https://modenc.renegadeprojects.com/OpportunityFire | Applies to VehicleTypes, InfantryTypes, and AircraftTypes. Allows firing while performing other actions if the object can aim/fire without turning itself. |
| ToProtect | https://modenc.renegadeprojects.com/ToProtect | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Controls whether the owning AI party provides protection when the unit is attacked. |
| ThreatAvoidanceCoefficient | https://modenc.renegadeprojects.com/ThreatAvoidanceCoefficient | Applies to InfantryTypes, VehicleTypes, and AircraftTypes. Controls how strongly the unit chooses lower-threat paths; AvoidThreats=yes can temporarily emulate 1.0. |
| Soylent | https://modenc.renegadeprojects.com/Soylent | Applies to TechnoTypes: AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Overrides refund / grinder / sale value logic. |
| Bounty | https://ares-developers.github.io/Ares-docs/new/bounty.html | Ares feature. `[TechnoType]►Bounty=` is a boolean that enables bounty for killing enemies. ModEnc also documents an older RockPatch-style `Bounty=` integer; BuiltIn v3.2 uses the Ares boolean interpretation. |
| VeteranAbilities | https://modenc.renegadeprojects.com/VeteranAbilities | Applies to AircraftTypes, BuildingTypes, InfantryTypes, and VehicleTypes. Lists bonuses gained at Veteran rank. |
| EliteAbilities | https://modenc.renegadeprojects.com/EliteAbilities | Lists bonuses gained at Elite rank. Does not stack identical effects with VeteranAbilities; already-gained abilities may remain active. |

## 3. Applied Matrix

| Key | Applied contexts | Editor kind | Result |
|---|---|---|---|
| AllowedToStartInMultiplayer | Techno, Infantry, Vehicle | Boolean | Updated Techno broad row and added exact Infantry / Vehicle rows. |
| CrateGoodie | Techno, Vehicle | Boolean | Updated Techno broad guardrail and added exact Vehicle row. |
| Trainable | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad Techno row and added exact object-context rows. |
| Insignificant | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Updated broad Techno row and added exact object-context rows. |
| NoMovingFire | Vehicle | Boolean | Added exact Vehicle row only; no broad Techno row was added. |
| OpportunityFire | Techno, Aircraft, Infantry, Vehicle | Boolean | Updated Techno broad row and added exact object-context rows. |
| ToProtect | AI, Techno, Aircraft, Building, Infantry, Vehicle | Boolean/Text | Updated Techno and object rows; converted the old AI row to non-canonical guardrail text. |
| ThreatAvoidanceCoefficient | Techno, Aircraft, Infantry, Vehicle | Float | Updated Techno broad row and added exact unit-context rows. |
| Soylent | Techno, Aircraft, Building, Infantry, Vehicle | Integer | Updated Techno row and added exact object-context rows. |
| Bounty | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | Added Ares-compatible boolean rows. |
| VeteranAbilities | Techno, Aircraft, Building, Infantry, Vehicle | MultiSelect | Updated Techno row and added exact object-context rows using existing VeteranAbility metadata. |
| EliteAbilities | Techno, Aircraft, Building, Infantry, Vehicle | MultiSelect | Updated Techno row and added exact object-context rows using existing VeteranAbility metadata. |

## 4. Non-canonical / Guardrail Decisions

- `CrateGoodie / Techno` is kept as a broad fallback guardrail because the verified canonical target is `VehicleTypes`.
- `ToProtect / AI` is kept only as a non-canonical guardrail. The source makes it a TechnoTypes flag, not an `[AI]` section field.
- `NoMovingFire` was added only for `VehicleTypes`, because the source page identifies VehicleTypes and records behavior caveats.
- `Bounty` uses the Ares boolean interpretation. The older ModEnc page also records a RockPatch integer interpretation, so the doc and description explicitly identify this as Ares-compatible.

## 5. Result Summary

```text
BuiltIn v3.2 field count: 4769 -> 4808
New exact/context rows: 39
Updated existing rows: 11
Target rows with direct placeholder / generic labels: 0
Exact `数值型字段` rows: 0
Exact `整数型字段` rows: 99
```

## 6. Next Step

Recommended next phase:

```text
FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply
```

Suggested field family:

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
