# Field Registry Description Verification - TechnoTypes Weapon Targeting

Phase: FR-DQ-2M-TechnoTypes-WeaponTargeting-ManualApply

This document records the source-backed verification and BuiltIn v3.2 patch for TechnoTypes weapon targeting, automatic acquisition, retaliation, land/naval targeting, and weapon-only firing behavior fields.

## 1. Scope

Processed keys:

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

## 2. Source Summary

Primary verification sources were ModEnc pages for each field family:

- `OmniFire`, `FireOnce`, `Burst`, `DecloakToFire`, `UseSparkParticles`, and `AttachedParticleSystem` for Weapon-only behavior.
- `DistributedFire`, `FireAngle`, `CanPassiveAquire`, `CanRetaliate`, `PreventAttackMove`, `NoAutoFire`, `LandTargeting`, and `NavalTargeting` for TechnoTypes / object targeting behavior.
- No reliable ModEnc field page was found for `Passive`; the row was converted to an unresolved guardrail rather than a canonical description.

## 3. Verification Matrix

| Key | Canonical Contexts | Result | Notes |
|---|---|---|---|
| OmniFire | Weapon | Verified | Techno row converted to non-canonical guardrail. |
| DistributedFire | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Building row preserves the attack-once caveat from ModEnc. |
| FireAngle | Vehicle, Building | Verified | Techno row is a broad guardrail; Aircraft/Infantry were not added. |
| CanPassiveAquire | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Keeps Westwood's `Aquire` spelling note. |
| CanRetaliate | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Retaliation is still limited by target legality, range, active orders, and special effects. |
| PreventAttackMove | Infantry, Vehicle | Verified | Global row converted to guardrail; Techno row states exact context is narrower. |
| NoAutoFire | Techno | PartiallyVerified | ModEnc gives behavior but lacks a complete applicable-to template, so only the existing Techno broad row was updated. |
| Passive | Techno | NeedsMoreEvidence | No canonical row was created; the existing row now explains source insufficiency. |
| LandTargeting | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Values 0/1/2 documented and examples added. |
| NavalTargeting | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Values 0-7 documented and examples added. |
| FireOnce | Weapon | Verified | Techno row converted to non-canonical guardrail. |
| Burst | Weapon | Verified | Techno row converted to non-canonical guardrail. |
| DecloakToFire | Weapon | Verified | Techno row converted to non-canonical guardrail. |
| UseFireParticles | Weapon | PartiallyVerified | Verified through UseSparkParticles and AttachedParticleSystem documentation; Global/Techno rows converted to guardrails. |

## 4. Context Boundaries

- Weapon-only rows are not copied into TechnoTypes. Existing broad Techno rows for `OmniFire`, `FireOnce`, `Burst`, `DecloakToFire`, and `UseFireParticles` are guardrails.
- `FireAngle` is only source-confirmed for VehicleTypes and BuildingTypes.
- `PreventAttackMove` is only source-confirmed for InfantryTypes and VehicleTypes.
- `Passive` remains unresolved and should not be used as authoritative Hover documentation.
- `LandTargeting` and `NavalTargeting` are TechnoTypes targeting fields and were expanded to Aircraft / Building / Infantry / Vehicle exact contexts.

## 5. Patch Summary

```text
BuiltIn v3.2 field count: 4862 -> 4887
New exact/context rows: 25
Updated existing rows: 17
Target rows with direct placeholder / generic labels: 0
Exact `数值型字段` rows: 0
Exact `整数型字段` rows: 99
```

## 6. Next Step

Recommended next phase:

```text
FR-DQ-2N-TechnoTypes-AircraftAndSpawn-ManualApply
```

Candidate fields:

```text
Spawns
SpawnsNumber
SpawnRegenRate
SpawnReloadRate
MissileSpawn
Spawned
Dock
AirportBound
Landable
MoveToShroud
Fighter
FlyBy
FlyBack
Crashable
PitchSpeed
PitchAngle
```
