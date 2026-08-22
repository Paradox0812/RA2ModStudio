# Field Registry Description Verification - TechnoTypes Economy / Resource / Crush

Phase: FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply

This document records the source verification and BuiltIn v3.2 Field Registry patch for the economy, resource pip, IFV mode, bunker, and crush-interaction field family.

No provider priority, lookup/fallback/enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy behavior was changed.

## 1. Scope

Processed keys:

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

`CrushableLevel` and `CrusherLevel` were searched but not added because no reliable RA2/YR ModEnc/Ares/Phobos field page was found for the current BuiltIn v3.2 scope.

## 2. Source Summary

- `Storage` is applicable to InfantryTypes, VehicleTypes, and BuildingTypes, and sets the maximum ore/resource value a unit or building can carry/store.
- `PipScale` is a TechnoTypes pip display selector used for passengers, ammo, Tiberium/storage, mind control, and related pip rendering modes.
- `Pip` is an InfantryTypes field that chooses the passenger pip graphic used when the infantry is loaded into a PipScale=Passengers transport.
- `Points` is a TechnoTypes scoring field, but the underlying scoring logic is obsolete or diversified in TS/RA2/YR.
- `Bunkerable` is VehicleTypes-only and controls whether a vehicle may enter Bunker=yes structures.
- `IFVMode` applies to InfantryTypes and VehicleTypes and selects the Gunner transport weapon/turret mode.
- `Crushable` applies to TechnoTypes and other ObjectTypes and controls whether an object can be crushed by Crusher=yes units.
- `Crusher` is a TechnoType object field used mainly by ground vehicles for crushing logic.
- `OmniCrusher` is VehicleTypes-only and still requires Crusher=yes to take effect.
- `OmniCrushResistant` applies to VehicleTypes and InfantryTypes and protects the object from OmniCrusher=yes vehicles.
- `CrushSound` is an object sound field used when the object is crushed; this batch keeps it conservative as a Techno/object broad fallback plus Infantry/Vehicle exact rows.

## 3. Verification Matrix

| Key | Context Result | Verification Result | Ready For Patch | Notes |
|---|---|---:|---:|---|
| Storage | Techno broad + Building/Vehicle/Infantry exact rows | Verified | Yes | Integer resource storage/carrying capacity. |
| PipScale | Techno broad + Aircraft/Building/Infantry/Vehicle exact rows | Verified | Yes | Enum pip display mode; values include Passengers, Ammo, Tiberium, MindControl, Power. |
| Pip | Infantry exact; Techno changed to non-canonical guardrail | Verified | Yes | Infantry passenger pip graphic only. |
| Points | Techno broad + Aircraft/Building/Infantry/Vehicle exact rows | VerifiedWithCaveat | Yes | RA2/YR scoring logic is diversified / partly obsolete. |
| Bunkerable | Vehicle exact; Techno changed to non-canonical guardrail | Verified | Yes | Requires Bunker=yes structure; Turret/SpeedType caveats preserved. |
| IFVMode | Infantry/Vehicle exact; Techno changed to non-canonical guardrail | Verified | Yes | IFVMode=0 maps to Weapon1, 1 to Weapon2, etc. |
| Crushable | Techno broad + Aircraft/Building/Infantry/Vehicle exact rows | Verified | Yes | Also exists on ObjectTypes outside this TechnoTypes batch. |
| Crusher | Vehicle exact + Techno broad fallback | PartiallyVerified | Yes | Source table and MovementZone notes support the field; behavior is mainly vehicle crushing. |
| OmniCrusher | Vehicle exact; Techno guardrail | Verified | Yes | Requires Crusher=yes; blocked primarily by OmniCrushResistant. |
| OmniCrushResistant | Infantry/Vehicle exact; Techno guardrail | Verified | Yes | Protects against OmniCrusher=yes crushing. |
| CrushSound | Infantry/Vehicle exact + Techno broad fallback | PartiallyVerified | Yes | Source table confirms object Sound entry; no dedicated field page found. |
| CrushableLevel | Not changed | NeedsMoreEvidence | No | No reliable RA2/YR source page found. |
| CrusherLevel | Not changed | NeedsMoreEvidence | No | No reliable RA2/YR source page found. |

## 4. Patch Summary

```text
BuiltIn v3.2 field count: 4965 -> 4988
New exact/context rows: 23
Updated / guarded existing rows: 13
Target rows with direct placeholder / generic labels: 0
Exact “数值型字段” rows: 0
Exact “整数型字段” rows: 99
Placeholder rows: 2304
Source-verified rows after this batch: 737
Strict non-source-verified rows after this batch: 4251
Hover-risk placeholder/generic rows after this batch: 2403
```

## 5. Important Boundaries

- `Pip / Techno`, `Bunkerable / Techno`, `IFVMode / Techno`, `OmniCrusher / Techno`, and `OmniCrushResistant / Techno` are guardrails, not canonical rows.
- `Crushable` has wider ObjectTypes applicability, but this batch only adds Techno/object-context rows for Aircraft, Building, Infantry, and Vehicle.
- `CrushableLevel` and `CrusherLevel` remain unresolved.
- `CrushSound` remains conservative because a dedicated source page was not found.

## 6. Validation

Static validation performed in the patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Target bad placeholder rows: 0
Expected verification doc: present
Clean package validation: passed
```

Not run in this environment:

```text
dotnet restore
dotnet build
dotnet test
```

Reason: dotnet CLI is unavailable in the patch environment.

## 7. Next Step

Recommended next phase:

```text
FR-DQ-2Q-TechnoTypes-RepairAndPower-ManualApply
```

Suggested fields:

```text
Repairable
SelfHealing
TiberiumHeal
Powered
PoweredUnit
PowersUnit
Drainable
Disableable
PoweredBy
Overpowerable
Unsellable
Capturable
NeedsEngineer
EngineerRepairable
```
