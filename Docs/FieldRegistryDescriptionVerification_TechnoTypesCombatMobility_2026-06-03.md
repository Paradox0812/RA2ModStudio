# Field Registry Description Verification - TechnoTypes Combat / Mobility

Phase: `FR-DQ-2I-TechnoTypes-CombatMobility-ManualApply`

This document records the manual source verification and BuiltIn v3.2 application for the next TechnoTypes combat / mobility field family.

The phase updates source-backed Hover descriptions and guardrails for:

```text
GuardRange
ROT
Locomotor
MovementZone
SpeedType
MovementRestrictedTo
Reload
Ammo
PipWrap
Passengers
Size
Category
```

No provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML / UI, project file, or legacy file was changed.

## 1. Source Trust Policy

Primary source: ModEnc RA2/YR field pages.

Source trust used in this phase:

```text
Community
```

Rows that are source-confirmed for the current context were written as canonical descriptions. Rows whose existing context is not confirmed by source were converted to explicit non-canonical guardrails rather than being deleted.

## 2. Verification Matrix

| Key | Contexts updated / added | Verification result | Notes |
|---|---|---|---|
| GuardRange | Techno, Aircraft, Building, Infantry, Vehicle | Verified | TechnoTypes scan / guard acquisition range, defaulting to the farther Primary/Secondary weapon Range. |
| ROT | Techno, Aircraft, Building, Infantry, Vehicle, Projectile, Weapon | Verified + Guardrail | TechnoTypes ROT controls object/turret turn rate; Projectile ROT controls homing/turning. Weapon row is non-canonical guardrail. |
| Locomotor | Techno, Aircraft, Building, Infantry, Vehicle, Warhead, Weapon | Verified + Guardrail | TechnoTypes Locomotor controls movement CLSID; Warhead context applies to IsLocomotor=yes; Weapon row is non-canonical guardrail. |
| MovementZone | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Controls movement/pathfinding zone; SpeedType still determines terrain passability and movement speed effects. |
| SpeedType | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Controls movement speed type over LandTypes. |
| MovementRestrictedTo | Vehicle, Techno | Verified + Guardrail | Source confirms VehicleTypes only. Techno row remains broad fallback guardrail. |
| Reload | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Base frame delay between automatic reload of Ammo rounds. |
| Ammo | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Ammunition count; -1 means unlimited. Context caveats are preserved for aircraft, vehicles, infantry and buildings. |
| PipWrap | Techno, Aircraft, Building, Infantry, Vehicle | Verified | Ammo pip display grouping when PipScale=Ammo. |
| Passengers | Techno, Aircraft, Building, Vehicle | Verified + Guardrail | Source confirms VehicleTypes, AircraftTypes and BuildingTypes. Techno row is broad fallback and warns not to apply to Infantry/Unit. |
| Size | Techno, Aircraft, Infantry, Vehicle | Verified + Guardrail | Source confirms InfantryTypes, VehicleTypes and AircraftTypes. Techno row warns not to apply to Building. |
| Category | Techno, Aircraft, Infantry, Vehicle | PartiallyVerified | Source page lacks a complete flag template but documents unit tactical categories used by AI targeting / teams / construction logic. No Building row was added. |

## 3. Canonical Rows Added

This phase added exact object-context rows for source-confirmed contexts:

```text
GuardRange / Aircraft, Building, Infantry, Vehicle
Locomotor / Aircraft, Building, Infantry, Vehicle, Warhead
MovementZone / Aircraft, Building, Infantry, Vehicle
SpeedType / Aircraft, Building, Infantry, Vehicle
MovementRestrictedTo / Vehicle
Reload / Aircraft, Building, Infantry, Vehicle
Ammo / Aircraft, Building, Infantry, Vehicle
PipWrap / Aircraft, Building, Infantry, Vehicle
Passengers / Aircraft, Building, Vehicle
Size / Aircraft, Infantry, Vehicle
Category / Aircraft, Infantry, Vehicle
```

## 4. Existing Rows Updated

Existing source-confirmed rows were updated with Hover-quality Chinese descriptions:

```text
GuardRange / Techno
ROT / Aircraft, Building, Infantry, Projectile, Techno, Vehicle
Locomotor / Techno
MovementZone / Techno
SpeedType / Techno
Reload / Techno
Ammo / Techno
PipWrap / Techno
Passengers / Techno
Size / Techno
Category / Techno
```

## 5. Non-canonical Guardrails

The following old-context rows were kept but changed to explicit guardrails:

```text
ROT / Weapon
Locomotor / Weapon
MovementRestrictedTo / Techno
Passengers / Techno
Size / Techno
```

`Passengers / Techno` and `Size / Techno` remain broad fallback rows because the effective lookup may still reach Techno for unknown object sections, but their descriptions now explicitly state the source-confirmed concrete contexts.

## 6. Source Notes

- `GuardRange`: ModEnc confirms it is a TechnoTypes flag for AircraftTypes, BuildingTypes, InfantryTypes and VehicleTypes, specifying target scan range in cells.
- `ROT`: ModEnc confirms separate TechnoTypes and Projectiles semantics; TechnoTypes use it for object/turret turn rate, Projectiles use it for homing / turning.
- `Locomotor`: ModEnc confirms TechnoTypes use Locomotor CLSIDs for movement and Warheads with `IsLocomotor=yes` can assign a locomotor to target units.
- `MovementZone`: ModEnc confirms it controls where the unit is allowed to go and assists AI pathfinding.
- `SpeedType`: ModEnc confirms it controls movement behavior over LandTypes.
- `MovementRestrictedTo`: ModEnc confirms VehicleTypes only.
- `Reload`, `Ammo`, `PipWrap`: ModEnc confirms TechnoTypes ammo/reload/pip display semantics and records context-specific caveats.
- `Passengers` and `Size`: ModEnc confirms transport capacity / passenger size semantics and context limitations.
- `Category`: ModEnc documents tactical unit classification and valid categories, but the page itself has a missing value-type template, so this is treated as source-backed with caveat.

## 7. Validation Summary

Static validation performed in the patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Expected verification doc: present
Clean package validation: pending package step
```

`dotnet restore`, `dotnet build`, and `dotnet test` were not run in the patch environment because `dotnet` CLI is unavailable.

## 8. Next Step

Recommended next source-family batch:

```text
FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply
```

Suggested fields:

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
