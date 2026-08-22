# Field Registry Description Verification - TechnoTypes Targeting / Transport / Deploy / Hover

Phase: FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply

This document records the source-family verification and JSON application for the next TechnoTypes targeting / transport / deploy / hover field batch.

## 1. Scope

Verified and applied these keys:

```text
SizeLimit
OpenTopped
DeploysInto
UndeploysInto
DeployFire
DeployFireWeapon
DeployTime
DeployToLand
Naval
Underwater
JumpJet
BalloonHover
HoverAttack
```

This phase updates BuiltIn v3.2 field descriptions, editor kinds, source metadata, and exact object-context rows only. It does not change provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML / UI, project files, or legacy code.

## 2. Source Trust Policy

All canonical rows in this batch use `SourceTrust = Community` semantics through ModEnc source-backed documentation. Rows with broad `Techno` semantics are kept as conservative fallback descriptions where ModEnc marks a flag as TechnoTypes-level or where exact object usage still requires caution.

## 3. Verification Matrix

| Key | Canonical / Guardrail Contexts | Editor Kind | Source | Verification Result | Notes |
|---|---|---|---|---|---|
| SizeLimit | Techno fallback, Vehicle, Aircraft | Integer | https://modenc.renegadeprojects.com/SizeLimit | Verified | Maximum passenger `Size` accepted by transports; source confirms VehicleTypes and AircraftTypes. |
| OpenTopped | Techno fallback, Vehicle, Aircraft, Building | Boolean | https://modenc.renegadeprojects.com/OpenTopped | Verified | Allows passengers to fire from a passenger-capable object; Building context has caveats. |
| DeploysInto | Techno fallback, Vehicle | Reference | https://modenc.renegadeprojects.com/DeploysInto | PartiallyVerified | Source template says TechnoTypes, but description is vehicle-to-building deployment; only Vehicle exact row is added. |
| UndeploysInto | Techno fallback, Building | Reference | https://modenc.renegadeprojects.com/UndeploysInto | Verified | Building undeploys into VehicleType; `none` disables undeploy. |
| DeployFire | Techno fallback, Infantry, Vehicle | Boolean | https://modenc.renegadeprojects.com/DeployFire | VerifiedWithCaveat | Infantry / Vehicle deploy-fire behavior; page warns RA2 cannot use this tag with VehicleTypes. |
| DeployFireWeapon | Techno fallback, Infantry, Vehicle | Integer | https://modenc.renegadeprojects.com/DeployFireWeapon | Verified | Selects deployed weapon slot for DeployFire / IsSimpleDeployer units. |
| DeployTime | Techno fallback, Vehicle, Building | Float | https://modenc.renegadeprojects.com/DeployTime | Verified | Time in minutes for deploying passengers or produced objects; also controls weapon factory door animation. |
| DeployToLand | Techno fallback, Vehicle | Boolean | https://modenc.renegadeprojects.com/DeployToLand | Verified | VehicleTypes field for hover / fly vehicles that require explicit deploy order to land. |
| Naval | Techno, Building, Vehicle | Boolean | https://modenc.renegadeprojects.com/Naval | Verified | Factory naval distinction on buildings; naval build and targeting classification on vehicles. |
| Underwater | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | https://modenc.renegadeprojects.com/Underwater | Verified | Underwater object classification; affects wake, sinking death logic, and recloak caveats. |
| JumpJet | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | https://modenc.renegadeprojects.com/JumpJet | Verified | Uses jumpjet controls to determine movement; requires Locomotor / jumpjet controls to evaluate actual motion. |
| BalloonHover | Techno fallback, Infantry, Vehicle, Aircraft | Boolean | https://modenc.renegadeprojects.com/BalloonHover | Verified | Jumpjet-locomotor units never land by default and attack by moving above target; RA2/YR trigger caveats preserved. |
| HoverAttack | Techno, Aircraft, Building, Infantry, Vehicle | Boolean | https://modenc.renegadeprojects.com/HoverAttack | VerifiedWithCaveat | Attempts takeoff / hover attack during attack mission; non-flying objects can behave strangely. |

## 4. Applied Row Summary

```text
BuiltIn v3.2 field count: 4740 -> 4769
New exact object-context rows: 29
Updated existing rows: 15
Exact “数值型字段” rows: 0 -> 0
Exact “整数型字段” rows: 99 -> 99
```

## 5. Context Decisions

- `DeploysInto / Vehicle` is added as the exact source-backed context. The broad `Techno` row remains conservative because ModEnc lists TechnoTypes, but the described use is VehicleType deploying into BuildingType.
- `UndeploysInto / Building` is added as the exact source-backed context. The broad `Techno` row remains conservative because the active semantic is BuildingType undeploying into VehicleType.
- `DeployFire / Vehicle` is source-backed but caveated because the ModEnc page warns RA2 cannot use this tag with VehicleTypes.
- `DeployToLand / Vehicle` is exact; broad `Techno` is only fallback.
- `Naval` is split into Building and Vehicle exact rows because the two contexts have distinct behavior.
- `Underwater`, `JumpJet`, and `HoverAttack` are retained as TechnoTypes-level fields and also receive exact object-context rows.

## 6. Runtime Behavior Check

No runtime behavior changed. This phase only changes BuiltIn field metadata and tests.

## 7. Next Step

Recommended next phase:

```text
FR-DQ-2K-TechnoTypes-ProductionVeterancy-ManualApply
```

Candidate field family:

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
