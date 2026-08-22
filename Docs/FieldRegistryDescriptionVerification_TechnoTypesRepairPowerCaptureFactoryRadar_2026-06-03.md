# Field Registry Description Verification - TechnoTypes Repair / Power / Capture / Factory / Radar

Phase: FR-DQ-2Q-TechnoTypes-RepairPowerCaptureFactoryRadar-BigBatch-ManualApply

This document records the source-backed verification pass for repair, power, capture, garrison, factory, radar, superweapon, refinery, absorb, hospital, armory, and cloning-related Field Registry rows.

## 1. Scope

Processed keys:

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
CanBeOccupied
MaxNumberOccupants
CanOccupyFire
LeaveRubble
Bib
FreeUnit
Factory
WeaponsFactory
UnitRepair
Radar
SpySat
SuperWeapon
SuperWeapon2
SuperWeapons
NukeSilo
Refinery
Harvester
DockUnload
UnitAbsorb
InfantryAbsorb
Hospital
Armory
Cloning
ConstructionYard
```

Rows intentionally left unresolved:

```text
CapturableBy
CanOccupyFireWeapon
```

`Disableable / Techno` was not promoted to a canonical description because the pass found table/index evidence but no reliable RA2/YR field page with stable semantics.

## 2. Sources Used

Primary verification sources:

- ModEnc RA2/YR field pages for repair, power, capture, garrison, building production, radar, superweapon, refinery, harvester, absorb, hospital, armory, and cloning fields.
- Ares documentation for `PoweredBy`, `Unsellable` extension behavior, `EngineerRepairable`, and `SuperWeapons` building extensions.
- Phobos documentation for shield `SelfHealing` / `Powered`.

## 3. Result Summary

```text
BuiltIn v3.2 field count: 4988 -> 5030
Rows affected: 90
New exact/context rows: 42
Updated / guarded existing rows: 48
Target rows with direct placeholder / generic labels: 0
Exact “数值型字段” rows: 0
Exact “整数型字段” rows: 99
Placeholder rows: 2268
Source-verified rows: 826
Strict non-source-verified rows: 4204
Hover-risk placeholder/generic rows: 2367
```

This meets the new 80-140 JSON-row batch target while staying inside one closely related semantic area.

## 4. Canonical Rows Added or Updated

Representative canonical rows:

```text
Repairable / Building
SelfHealing / Aircraft, Building, Infantry, Vehicle
SelfHealing / Shield
TiberiumHeal / Global, Aircraft, Infantry, Vehicle
Powered / Building
Powered / Shield
PoweredUnit / Vehicle
PowersUnit / Building
Drainable / Building
PoweredBy / Unit
Overpowerable / Building
Unsellable / Building, Techno, Aircraft, Infantry, Vehicle
Capturable / Building
NeedsEngineer / Building
EngineerRepairable / Building
CanBeOccupied / Building
MaxNumberOccupants / Building
CanOccupyFire / Building
LeaveRubble / Building
Bib / Building
FreeUnit / Building
Factory / Building
WeaponsFactory / Building
UnitRepair / Building
Radar / Building
SpySat / Building
SuperWeapon / Building
SuperWeapon2 / Building
SuperWeapons / Building
SuperWeapons / Global
NukeSilo / Building
Refinery / Building
Harvester / Vehicle
DockUnload / Building
UnitAbsorb / Building
InfantryAbsorb / Building
Hospital / Building
Armory / Building
Cloning / Building
ConstructionYard / Building
```

## 5. Guardrail Rows

The following broad or wrong-context rows were kept as Field Registry guardrails instead of being deleted:

```text
Repairable / Techno
TiberiumHeal / Techno
Powered / Techno
PoweredUnit / Techno
PowersUnit / Techno
Drainable / Techno
Disableable / Techno
Overpowerable / Techno
Capturable / Techno
Capturable / AI
NeedsEngineer / Techno
CanBeOccupied / Techno
CanBeOccupied / AI
MaxNumberOccupants / Techno
CanOccupyFire / AI
LeaveRubble / Techno
Bib / Techno
FreeUnit / Techno
Factory / Techno
WeaponsFactory / Techno
UnitRepair / Techno
Radar / Techno
SpySat / Techno
SuperWeapon / Techno
SuperWeapon2 / Techno
SuperWeapons / Techno
SuperWeapons / AI
NukeSilo / Techno
Refinery / Techno
Harvester / Techno
Harvester / Global
Harvester / AI
DockUnload / Techno
UnitAbsorb / Techno
InfantryAbsorb / Techno
Hospital / Techno
Armory / Techno
Cloning / Techno
ConstructionYard / Techno
```

Guardrails explicitly say why the row should not be treated as a canonical section-context definition.

## 6. Important Semantic Boundaries

- `Repairable`, `Powered`, `Capturable`, `NeedsEngineer`, `CanBeOccupied`, `MaxNumberOccupants`, `CanOccupyFire`, `LeaveRubble`, `Bib`, `Factory`, `WeaponsFactory`, `UnitRepair`, `Radar`, `SpySat`, `SuperWeapon`, `SuperWeapon2`, `NukeSilo`, `Refinery`, `DockUnload`, `UnitAbsorb`, `InfantryAbsorb`, `Hospital`, `Armory`, `Cloning`, and `ConstructionYard` are BuildingTypes-centered in this batch.
- `Harvester` is exact for VehicleTypes.
- `PoweredUnit` is exact for VehicleTypes; `PowersUnit` is exact for BuildingTypes.
- `TiberiumHeal` has a Tiberian Sun / RA2-YR caveat and should not be presented as a fully active RA2/YR Techno healing mechanic.
- `Unsellable` is BuildingTypes in vanilla and TechnoTypes through Ares extension behavior.
- `SelfHealing / Shield` and `Powered / Shield` are Phobos shield rows, not vanilla TechnoTypes rows.
- `SuperWeapons / Global` is the `[IQ]` minimum-IQ field, while `SuperWeapons / Building` is Ares building multi-superweapon extension.
- `CapturableBy` and `CanOccupyFireWeapon` require more evidence and were not written into JSON in this phase.

## 7. Runtime Boundary

This phase changed only Field Registry data, Field Registry data tests, and documentation.

No provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML / UI, project file, or legacy editor behavior was changed.

## 8. Next Step

Recommended next phase:

```text
FR-DQ-2R-WeaponCore-BigBatch-ManualApply
```

The TechnoTypes repair / power / capture / factory / radar group is now sufficiently covered for the current Field Registry Hover quality pass. The next high-yield batch should move to Weapon core rows unless the user wants to continue clearing remaining TechnoTypes infrastructure fields first.
