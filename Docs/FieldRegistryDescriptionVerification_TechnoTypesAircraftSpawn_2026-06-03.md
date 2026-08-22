# Field Registry Description Verification - TechnoTypes Aircraft / Spawn

Phase: FR-DQ-2N-TechnoTypes-AircraftAndSpawn-ManualApply

## 1. Scope

This source-family batch verifies aircraft, spawner, docking, landing, missile-spawn, and flight-pitch related hover descriptions for the BuiltIn v3.2 field registry.

Processed keys:

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

This phase updates only Field Registry data, the BuiltIn loader regression tests, and documentation. It does not modify provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML / UI, project files, or legacy files.

## 2. Source Trust Policy

All canonical descriptions in this batch use ModEnc field pages as Community-trust source evidence. Rows where the source proves a field is not valid for a broad context are kept as non-canonical guardrails rather than removed, so old imported rows or broad fallback entries cannot reintroduce placeholder Hover text.

## 3. Verification Matrix

| Key | SectionKind / Schema | Verified meaning | Source | Source trust | Result |
|---|---|---|---|---|---|
| Spawns | Techno, Aircraft, Building, Infantry, Vehicle | Spawner TechnoType uses this field to name the spawned AircraftType. | ModEnc Spawns | Community | Verified |
| Spawns | ArtObject | Animation / art debris context uses this key for spawned debris, not Techno spawner aircraft. | ModEnc Spawns | Community | Verified |
| Spawns | Global | ParticleSystems use Spawns as continuous particle generation; Global row is kept only as guardrail. | ModEnc Spawns | Community | Guardrail |
| SpawnsNumber | Techno, Aircraft, Building, Infantry, Vehicle | Number of spawnees designated by Spawns. | ModEnc SpawnsNumber | Community | Verified |
| SpawnRegenRate | Techno, Aircraft, Building, Infantry, Vehicle | Timer for regenerating destroyed spawnees; MissileSpawn=yes starts counting at launch. | ModEnc SpawnRegenRate | Community | Verified |
| SpawnReloadRate | Techno, Aircraft, Building, Infantry, Vehicle | Reload time after spawnees return; irrelevant for MissileSpawn=yes because missiles do not return. | ModEnc SpawnReloadRate | Community | Verified |
| MissileSpawn | Techno, Aircraft, Building, Infantry, Vehicle | Marks spawned missile / launcher behavior and controls spawned-missile veterancy and launcher creation timing. | ModEnc MissileSpawn | Community | Verified |
| Spawned | Aircraft, Vehicle, Infantry | Marks object as a spawnee with special cursor / selection / EVA behavior. | ModEnc Spawned | Community | Verified |
| Spawned | AI | Not an [AI] field; kept as non-canonical guardrail. | ModEnc Spawned | Community | Guardrail |
| Dock | Aircraft, Vehicle | BuildingType list this moving object can dock with for reloading, repair, unload, bunker, helipad, or related behavior. | ModEnc Dock | Community | Verified |
| AirportBound | Aircraft | Aircraft must return to Dock / Helipad-like structure after mission; losing the last usable dock crashes the aircraft. | ModEnc AirportBound | Community | Verified |
| Landable | Aircraft | Aircraft can land on map or Helipad=yes structures; Landable=no interacts with Selectable and Spawned. | ModEnc Landable | Community | Verified with caveat |
| MoveToShroud | Techno, Aircraft, Building, Infantry, Vehicle | Controls whether the TechnoType may move into shrouded cells. | ModEnc MoveToShroud | Community | Verified |
| Fighter | Aircraft | Allows aircraft to fire as a fighter, skipping some facing / hover-point checks. | ModEnc Fighter | Community | Verified |
| FlyBy | Aircraft | Aircraft flies by target position during attacks instead of slowing to approach for firing. | ModEnc FlyBy | Community | Verified |
| FlyBack | Aircraft | Locks aircraft to its flight path so it does not deviate from height differences or similar disturbances. | ModEnc FlyBack | Community | Verified |
| Crashable | Aircraft, Vehicle, Infantry | Enables crash behavior for aircraft / jumpjet units; non-aircraft jumpjets need Crashable=yes for crash instead of mid-air explosion. | ModEnc AircraftTypes / JumpjetCrash | Community | Verified with caveat |
| PitchSpeed | Techno, Aircraft, Vehicle | Speed ratio threshold at which aircraft / jumpjet vehicle pitches and rolls. | ModEnc PitchSpeed | Community | Verified with caveat |
| PitchAngle | Techno, Aircraft, Vehicle | Forward tilt angle used when PitchSpeed condition is met. | ModEnc PitchAngle | Community | Verified with caveat |

## 4. Canonical Rows Added Or Updated

Exact object-context rows added or source-backed in this batch:

```text
Spawns / Aircraft, Building, Infantry, Vehicle
SpawnsNumber / Aircraft, Building, Infantry, Vehicle
SpawnRegenRate / Aircraft, Building, Infantry, Vehicle
SpawnReloadRate / Aircraft, Building, Infantry, Vehicle
MissileSpawn / Aircraft, Building, Infantry, Vehicle
Spawned / Aircraft, Vehicle, Infantry
Dock / Aircraft, Vehicle
AirportBound / Aircraft
Landable / Aircraft
MoveToShroud / Aircraft, Building, Infantry, Vehicle
Fighter / Aircraft
FlyBy / Aircraft
FlyBack / Aircraft
Crashable / Aircraft, Vehicle, Infantry
PitchSpeed / Aircraft, Vehicle
PitchAngle / Aircraft, Vehicle
```

Broad Techno rows for spawner / shroud / pitch fields were updated with conservative source-backed wording. Aircraft-only broad Techno rows such as `Fighter / Techno`, `FlyBy / Techno`, `FlyBack / Techno`, `AirportBound / Techno`, and `Landable / Techno` were converted into guardrails that point to AircraftTypes semantics.

## 5. Guardrails / Non-canonical Contexts

The following rows are kept or converted to guardrails:

```text
Spawns / Global
Spawned / AI
AirportBound / Techno
Landable / Techno
Fighter / Techno
FlyBy / Techno
FlyBack / Techno
Crashable / Techno
```

They remain present to prevent old imported placeholders from polluting Hover, but they should not be treated as canonical non-aircraft rows.

## 6. Result Summary

```text
BuiltIn v3.2 field count: 4887 -> 4928
New exact/context rows: 41
Updated / guarded existing rows: 17
Target rows with direct placeholder / generic labels: 0
Exact “数值型字段” rows: 0
Exact “整数型字段” rows: 99
```

Static validation performed in patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Expected verification doc: present
```

`dotnet restore`, `dotnet build`, and `dotnet test` were not run in the patch environment because the dotnet CLI is unavailable there.

## 7. Next Step

Recommended next phase:

```text
FR-DQ-2O-TechnoTypes-JumpjetAndFlightTuning-ManualApply
```

Suggested keys:

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
