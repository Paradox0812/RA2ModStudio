# Codex Task: RA2IniEditor.IDE AI-5B-Fix2 Draft Evidence Profiles

## 0. Context

User confirmed that simple seed keys are useful but not enough for complex units.

Problem:

```text
For simple requests such as "轻型防空车", common seed keys like Strength / Armor / Primary / Speed are enough.
For complex units, special mechanics need additional fields, and a single fixed seed list will be too narrow.
```

This task refines AI draft evidence retrieval from a single seed list into layered evidence profiles.

---

## 1. Goal

Improve Field Registry evidence retrieval for AI draft generation by introducing intent/mechanic-aware seed profiles.

The assistant should retrieve field evidence from multiple small profiles depending on the user's request.

Example:

```text
"轻型防空车" -> UnitCore + VehicleCore + AntiAirWeapon
"隐形侦察车" -> UnitCore + VehicleCore + StealthScout
"部署后变成防空炮" -> UnitCore + VehicleCore + DeployTransform + AntiAirWeapon
"运输车" -> UnitCore + VehicleCore + Transport
```

The fix must keep evidence bounded and provider-confirmed.

---

## 2. Hard Boundaries

Do not implement:

```text
DeepSeek adapter changes
provider switching changes
PromptBuilder rewrite beyond evidence-related text if needed
Apply / Insert
file modification
Field Registry writes
whole-project context
auto-send context
diagnostic auto-fix
```

Do not modify:

```text
Field Registry loader / writer / apply / rollback / import / learning services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn JSON
legacy files
solution / project files
```

---

## 3. Core Design

### 3.1 Evidence profiles

Add small internal evidence profiles. Each profile is a named group of candidate field keys.

Suggested profile categories:

```text
UnitCore
VehicleCore
InfantryCore
BuildingCore
WeaponCore
ProjectileCore
WarheadCore
AntiAirWeapon
GroundAttackWeapon
DeployTransform
Transport
StealthScout
Sensor / Detector
Garrison / Passenger
SelfRepair / Regeneration
BuildLimit / TechPrerequisite
Veterancy
ArtVoxel
ArtSHP
```

Do not return a seed key unless the active provider confirms it.

### 3.2 Profile selection

Select profiles using simple local keyword matching.

Examples:

```text
防空 / 对空 / 飞机 / 空军 / AA -> AntiAirWeapon
部署 / 展开 / 变形 / deploy -> DeployTransform
运输 / 载员 / passengers -> Transport
隐形 / 潜行 / stealth / cloak -> StealthScout
侦察 / 视野 / 雷达 / detector -> Sensor / Detector
维修 / 自修 / repair -> SelfRepair
老兵 / 升级 / elite -> Veterancy
```

Keep this lightweight. No embeddings, no DeepSeek, no network.

### 3.3 Always include core profile

For draft-like unit requests, always include:

```text
UnitCore
```

Then add type-specific profile if inferred:

```text
VehicleCore / InfantryCore / BuildingCore
```

Then add mechanic profiles from keywords.

### 3.4 Evidence budget

Keep result bounded.

Suggested:

```text
Max profiles: 4 to 6
Max candidate keys before provider confirmation: 80
Returned evidence: Top 12 or configurable existing hard cap
```

If more profiles match, prioritize:

```text
1. explicit current caret key
2. user prompt direct key matches
3. unit type profile
4. mechanic profile
5. general core fields
```

---

## 4. Suggested Profile Seeds

### UnitCore

```text
Name
UIName
Image
Prerequisite
Primary
Secondary
Strength
Armor
Speed
Sight
Cost
TechLevel
Owner
RequiredHouses
ForbiddenHouses
Category
BuildCat
Trainable
ThreatPosed
```

### VehicleCore

```text
Turret
Crusher
Crewed
Weight
Size
Locomotor
MovementZone
SpeedType
ROT
Accelerates
IsTilter
Tracked
```

### WeaponCore

```text
Damage
ROF
Range
Projectile
Speed
Warhead
Report
Anim
Burst
```

### ProjectileCore

```text
AA
AG
Arm
Shadow
Proximity
Ranged
Image
Rotates
Inviso
SubjectToCliffs
SubjectToElevation
SubjectToWalls
```

### WarheadCore

```text
Verses
CellSpread
PercentAtMax
InfDeath
Wall
Wood
Conventional
ProneDamage
```

### AntiAirWeapon

```text
Primary
Secondary
ElitePrimary
EliteSecondary
Projectile
Warhead
AA
AG
Range
GuardRange
```

### DeployTransform

```text
DeploysInto
UndeploysInto
DeployToFire
DeployFire
IsSimpleDeployer
Deployer
DeployTime
```

### Transport

```text
Passengers
PipScale
OpenTopped
SizeLimit
EnterTransportSound
LeaveTransportSound
```

### StealthScout

```text
Cloakable
CloakingSpeed
Sensors
SensorsSight
DetectDisguise
DefaultToGuardArea
Sight
Speed
```

Only confirmed fields should become evidence.

---

## 5. Implementation Options

Preferred:

```text
Add Ra2AiDraftEvidenceProfile / Ra2AiDraftEvidenceProfileSelector.
Use them inside Ra2FieldRegistryAiEvidenceProvider.
```

Alternative:

```text
Keep profiles as private static arrays in Ra2FieldRegistryAiEvidenceProvider if the implementation remains small.
```

Do not put profile logic in ShellWindow.xaml.cs.

---

## 6. PromptBuilder Note

PromptBuilder should continue to state:

```text
Clean INI blocks should use evidence-backed field keys.
Unverified field keys must go to optional / verify-before-use.
```

This fix should increase available evidence, not weaken that rule.

---

## 7. Tests

Required tests:

```text
1. Draft-like vehicle prompt selects UnitCore + VehicleCore.
2. "轻型防空车" selects AntiAirWeapon profile.
3. "部署后变成防空炮" selects DeployTransform + AntiAirWeapon.
4. "运输车" selects Transport.
5. "隐形侦察车" selects StealthScout.
6. Unavailable seed keys are not returned as evidence.
7. Returned evidence count remains bounded.
8. Existing exact key lookup still has priority.
9. Non-draft field explanation behavior remains unchanged.
10. No provider reload / file IO / mutation occurs.
```

---

## 8. Validation Commands

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Acceptance Criteria

This phase is accepted when:

```text
1. Simple unit drafts receive common field evidence.
2. Complex unit mechanics receive relevant additional evidence profiles.
3. Evidence remains bounded and provider-confirmed.
4. PromptBuilder no-hallucinated-fields rule remains intact.
5. No DeepSeek/provider/UI/file-write behavior changes.
```
