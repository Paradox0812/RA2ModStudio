# Field Registry Description Verification - Warhead Core Big Batch

Phase: FR-DQ-2S-WarheadCore-BigBatch-ManualApply  
Date: 2026-06-03

## 1. Scope

This batch verifies core Warhead fields and same-domain Ares / Phobos Warhead extensions. It updates source-backed Warhead rows and converts old wrong-context Weapon / Techno / Global / ArtObject rows into guardrails where a source shows the field does not belong to that section.

Processed key families:

```text
Verses
CellSpread
PercentAtMax
Wood
Wall
Rocker
AnimList
InfDeath
Conventional
Tiberium
ProneDamage
Sparky
Fire
Bright
CLDisableRed
CLDisableGreen
CLDisableBlue
CombatLightSize
ShakeXlo / ShakeXhi / ShakeYlo / ShakeYhi
AffectsAllies / AffectsEnemies / AffectsOwner / AffectsNeutral
AffectsAbovePercent / AffectsBelowPercent
CellSpread.MaxAffect
Deployed.Damage
InfDeathAnim
Ripple.Radius
AnimList.*
SplashList.*
CombatLightChance / CombatLightDetailLevel / CLIsBlack
Damage*Multiplier
Crit.*
```

## 2. Source Trust

- `ModEnc`: community field reference for vanilla RA2/YR Warhead flags.
- `Ares`: official Ares documentation for AffectsOwner / AffectsEnemies / CellSpread.MaxAffect and related Warhead extensions.
- `Phobos`: official Phobos documentation for combat light customization, AnimList / SplashList extensions, target filters and critical / multiplier logic.
- `Community`: PPM YR combat-system notes used only for the ShakeX/Y rows where a dedicated ModEnc field page was not available.

## 3. Result Summary

```text
BuiltIn v3.2 field count: 5036 -> 5044
Rows affected: 105
New exact/context rows: 8
Updated / guarded existing rows: 97
Source-verified rows: 919 -> 1023
Strict non-source-verified rows: 4117 -> 4021
Direct placeholder rows: 2227 -> 2184
Exact integer generic rows: 94 -> 91
Exact numeric generic rows: 0 -> 0
Direct Hover-risk placeholder/generic rows: 2321 -> 2275
```

## 4. Added Exact Warhead Rows

```text
Fire / Warhead
ShakeXlo / Warhead
ShakeXhi / Warhead
ShakeYlo / Warhead
ShakeYhi / Warhead
AffectsAllies / Warhead
AffectsOwner / Warhead
CellSpread.MaxAffect / Warhead
```

## 5. Canonical Warhead Rows Updated

```text
Verses / Warhead
CellSpread / Warhead
PercentAtMax / Warhead
Wood / Warhead
Wall / Warhead
Rocker / Warhead
AnimList / Warhead
InfDeath / Warhead
Conventional / Warhead
Tiberium / Warhead
ProneDamage / Warhead
Sparky / Warhead
Fire / Warhead
Bright / Warhead
CLDisableRed / Warhead
CLDisableGreen / Warhead
CLDisableBlue / Warhead
CombatLightSize / Warhead
AffectsAllies / Warhead
AffectsEnemies / Warhead
AffectsOwner / Warhead
AffectsNeutral / Warhead
AffectsAbovePercent / Warhead
AffectsBelowPercent / Warhead
Deployed.Damage / Warhead
InfDeathAnim / Warhead
Ripple.Radius / Warhead
```

## 6. Phobos / Ares Extension Rows Updated

```text
AnimList.PickRandom / Warhead
AnimList.CreateAll / Warhead
AnimList.CreationInterval / Warhead
AnimList.ScatterMin / Warhead
AnimList.ScatterMax / Warhead
SplashList / Warhead
SplashList.CreationInterval / Warhead
SplashList.ScatterMin / Warhead
SplashList.ScatterMax / Warhead
CombatLightChance / Warhead
CombatLightDetailLevel / Warhead
CombatLightDetailLevel.CheckColored / Warhead
CLIsBlack / Warhead
DamageAlliesMultiplier / Warhead
DamageEnemiesMultiplier / Warhead
DamageOwnerMultiplier / Warhead
DamageSourceHealthMultiplier / Warhead
DamageTargetHealthMultiplier / Warhead
Crit.Chance / Warhead
Crit.ExtraDamage / Warhead
Crit.Warhead / Warhead
Crit.AnimList / Warhead
Crit.AffectsHouse / Warhead
Crit.AffectsTarget / Warhead
Crit.AffectsAbovePercent / Warhead
Crit.AffectsBelowPercent / Warhead
```

## 7. Guardrail Rows

Wrong-context rows were not deleted; they were rewritten as non-canonical guardrails. This preserves fallback stability while preventing bad Hover text from claiming a field belongs to the wrong section.

Examples:

```text
Verses / Weapon
CellSpread / Weapon
PercentAtMax / Weapon
Wood / Weapon
Wall / Weapon
Rocker / Weapon
AnimList / Weapon
InfDeath / Weapon
Conventional / Weapon
Tiberium / Weapon
ProneDamage / Weapon
Sparky / Weapon
Fire / Weapon
CLDisableRed / Weapon
CLDisableGreen / Weapon
CLDisableBlue / Weapon
CombatLightSize / Weapon
ShakeXlo / Weapon
ShakeXhi / Weapon
ShakeYlo / Weapon
ShakeYhi / Weapon
AffectsAllies / Weapon
```

Broad Techno / Global / ArtObject rows for the same Warhead-only fields were also guarded where they existed.

## 8. Boundaries

- No Field Registry provider priority changes.
- No lookup / fallback / enrichment changes.
- No Hover / Quick Peek / AI Evidence code changes.
- No parser / diagnostics / completion / save preflight changes.
- No XAML / UI changes.
- No project or legacy code changes.
- `Supress / Weapon` remains unresolved from the previous Weapon batch.
- Larger Phobos Shield / AttachEffect / Convert / KillWeapon Warhead rows remain for later extension-specific batches.

## 9. Validation

Static validation completed:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Target bad placeholder rows: 0
Expected verification doc: present
Clean package validation: passed
```

`dotnet restore`, `dotnet build`, and `dotnet test` were not run in the patch environment because dotnet CLI is unavailable.

## 10. Next Step

Recommended next phase:

```text
FR-DQ-2T-ProjectileCore-BigBatch-ManualApply
```

Candidate families:

```text
AA
AG
ROT
Image
Shadow
Proximity
Ranged
Arcing
Inaccurate
FlakScatter
SubjectToCliffs
SubjectToElevation
SubjectToWalls
SubjectToBuildings
SubjectToTrenches
Acceleration
Vertical
Dropping
Arm
CourseLockDuration
Scalable
```
