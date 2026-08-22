# Field Registry Description Verification - AI Page Batch

Phase: FR-DQ-2G-AI-Page-Batch-ManualApply

This document records the source-page-level verification pass for ModEnc's `[AI]` page. The goal is to replace Hover-facing placeholder / rough imported text for confirmed `[AI]` fields with source-backed Chinese descriptions, and to turn wrong-context Global/Techno rows into explicit non-canonical guardrails.

## 1. Scope

Source page:

```text
https://modenc.renegadeprojects.com/AI
```

Primary source facts:

- ModEnc states that `[AI]` contains the game's artificial intelligence settings in `Rules(md).ini`.
- The page lists Team, Super Weapon, Base & Building, Build*, Defense, Resource, and applicable `[AI]` INI flags.
- The applicable flag table gives section, key, value type, default value, and list-append behavior for many `[AI]` fields.

This phase modifies BuiltIn v3.2 Field Registry data only. It does not change provider priority, lookup / fallback / enrichment behavior, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy files.

## 2. Source Trust Policy

```text
Source: ModEnc AI page
Trust: Community
Use: Accepted for source-backed BuiltIn fallback descriptions with conservative caveats
```

Rows are classified as:

- `CanonicalAI`: the source places the key in `[AI]`; the `[AI]` row can receive a source-backed description.
- `NonCanonicalGuardrail`: the source places the key in `[AI]`; existing `Global` or `Techno` rows are retained only to prevent legacy/fallback pollution.
- `AlreadyVerified`: rows previously verified in AI low-quality / cross-context batches, touched only for source metadata consistency.

## 3. Verification Matrix Summary

| Group | Result |
|---|---|
| Team timing / economy fields | Source-backed `[AI]` descriptions for CloseEnough, Stray, RelaxedStray, GuardModeStray, TeamDelays, AIHateDelays, MultiplayerAICM, AIVirtualPurifiers, HarvestersPerRefinery, AIExtraRefineries, MinimumAIDefensiveTeams, MaximumAIDefensiveTeams, TotalAITeamCap. |
| Base / building behavior fields | Source-backed `[AI]` descriptions for AISafeDistance, AIMinorSuperReadyPercent, base-defense counts, super-defense fields, and side-specific power plant fields. |
| Build* and defense lists | Source-backed `[AI]` descriptions for BuildConst, BuildPower, BuildRefinery, BuildBarracks, BuildTech, BuildWeapons, BuildRadar, BuildHelipad, BuildNavalYard, BuildDefense, BuildPDefense, BuildAA, ConcreteWalls, Gates, Dummy, NeutralTechBuildings, Allied/Soviet/ThirdBaseDefenses. |
| Resource / timing / base-shape fields | Source-backed `[AI]` descriptions for CreditReserve, PowerSurplus, BaseSizeAdd, PowerEmergency, AIBaseSpacing, AttackInterval, AttackDelay, PatrolScan, PathDelay, BlockagePathDelay, AutocreateTime, InfantryReserve, InfantryBaseMult. |
| Ratio / Limit rows | Existing AI low-quality verified descriptions retained and normalized to the AI page source. |
| Non-canonical Global/Techno rows | Existing Global/Techno rows for `[AI]` keys were converted to guardrails where present. |

## 4. Added Canonical `[AI]` Rows

These keys had no direct `[AI]` row in BuiltIn v3.2 and now have source-backed `[AI]` rows:

```text
GuardModeStray
NodRegularPower
GDIPowerPlant
ThirdPowerPlant
BuildHelipad
BuildDefense
BuildPDefense
BuildAA
NSGates
EWGates
BuildDummy
NeutralTechBuildings
AttackInterval
AttackDelay
CompEasyBonus
GDIWallDefense
GDIWallDefenseCoefficient
NodBaseDefenseCoefficient
GDIBaseDefenseCoefficient
```

## 5. Updated Canonical `[AI]` Rows

62 existing `[AI]` rows were updated with source-backed wording from the ModEnc `[AI]` page. This includes existing Build*, defense, timing, resource, base-size, ratio/limit, and super-defense rows.

## 6. Non-canonical Guardrails

149 existing `Global` or `Techno` rows for source-confirmed `[AI]` keys were changed to non-canonical guardrails.

These guardrails intentionally say the key belongs to `[AI]` and should not be treated as `[General] / Global` or `TechnoType` data. They are retained to prevent older imported fallback text from polluting Hover, but they are not canonical descriptions for those contexts.

## 7. Data Delta

```text
BuiltIn v3.2 field count: 4643 -> 4662
New direct [AI] rows: 19
Existing direct [AI] rows updated: 62
Global/Techno guardrail rows updated: 149
Exact `数值型字段` rows: 0 -> 0
Placeholder rows: 2452 -> 2393
```

## 8. Validation

Static validation completed in the patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target AI row validation: passed
Expected verification doc: present
```

Not run in the patch environment:

```text
dotnet restore
dotnet build
dotnet test
```

Reason: dotnet CLI is unavailable in this environment.

## 9. Next Step

Recommended next phase:

```text
FR-DQ-2G-AI-Page-Batch-Review or FR-DQ-2H-TechnoTypes-Common-ManualApply
```

If local dotnet validation passes, continue with a source-page-level TechnoTypes common-field batch rather than returning to 5-field micro-batches.
