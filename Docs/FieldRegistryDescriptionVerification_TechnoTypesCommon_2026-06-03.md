# Field Registry Description Verification - TechnoTypes Common Batch

Phase: FR-DQ-2H-TechnoTypes-Common-ManualApply

This document records the source-family verification pass for common `TechnoTypes` fields. The goal is to replace Hover-facing rough / ambiguous descriptions for high-frequency object fields with source-backed Chinese descriptions, add exact object-context rows where ModEnc confirms applicability, and convert wrong-context rows into explicit non-canonical guardrails.

## 1. Scope

Source pages:

```text
https://modenc.renegadeprojects.com/Primary
https://modenc.renegadeprojects.com/Secondary
https://modenc.renegadeprojects.com/Strength
https://modenc.renegadeprojects.com/Speed
https://modenc.renegadeprojects.com/TechLevel
https://modenc.renegadeprojects.com/Cost
https://modenc.renegadeprojects.com/Armor
https://modenc.renegadeprojects.com/Sight
https://modenc.renegadeprojects.com/Owner
https://modenc.renegadeprojects.com/Prerequisite
```

Covered keys:

```text
Primary
Secondary
Strength
Speed
TechLevel
Cost
Armor
Sight
Owner
Prerequisite
```

This phase modifies BuiltIn v3.2 Field Registry data only. It does not change provider priority, lookup / fallback / enrichment behavior, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy files.

## 2. Source Trust Policy

```text
Source: ModEnc field pages
Trust: Community
Use: Accepted for source-backed BuiltIn fallback descriptions with conservative context boundaries
```

Rows are classified as:

- `CanonicalTechnoTypes`: the source places the key on TechnoTypes or specific Aircraft/Building/Infantry/Vehicle contexts; these rows can receive source-backed descriptions.
- `CanonicalOtherContext`: the source confirms another context, such as `Speed / Weapon` or `Strength / Projectile`, but the wording must stay separate from TechnoTypes.
- `NonCanonicalGuardrail`: the source shows that an existing row is the wrong broad context, such as `Owner / AI`, `Cost / Global`, or `Armor / Projectile`; these rows are retained only to prevent legacy/fallback pollution.
- `Unresolved`: source is insufficient or belongs to another system not covered by this phase.

## 3. Verification Matrix Summary

| Key | Verified source meaning | JSON result |
|---|---|---|
| Primary | TechnoTypes main weapon, limited to Weapon references. | Updated `Techno`; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Secondary | TechnoTypes auxiliary weapon, used for targets Primary cannot handle; RA2 adds EliteSecondary. | Updated `Techno`; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Strength | HitPoints for TechnoTypes and multiple ObjectTypes including Projectiles. | Updated `Techno` and `Projectile`; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Speed | Unit movement speed on Infantry/Vehicle/Aircraft; separate Weapon projectile speed context. | Updated `Techno`, `Weapon`, and `Global` guardrail; added `Aircraft`, `Infantry`, `Vehicle` exact rows. |
| TechLevel | Buildings raise tech level; Infantry/Vehicle/Aircraft use it as required tech level; also appears in MultiplayerDialogSettings and map Houses. | Updated `Techno`, `AI` guardrail, `Global` guardrail; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Cost | Base purchase cost for TechnoTypes; affects deduction, sell/grind value, build time, and sidebar ordering. | Updated `Techno`, `Global` guardrail; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Armor | TechnoTypes armor class used by Warhead `Verses`; Armor has other Difficulty/House/Powerups meanings. | Updated `Techno`, `Global` guardrail, `Projectile` guardrail; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Sight | TechnoTypes shroud reveal distance; AI target selection is usually not based on visibility. | Updated `Techno`, `AI` guardrail; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Owner | TechnoTypes build/ownership country list; AI TaskForce production has special limitations. | Updated `Techno`, `AI` guardrail; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |
| Prerequisite | TechnoTypes build prerequisites; comma-separated BuildingTypes or special prerequisite keywords. | Updated `Techno`, `AI` guardrail; added `Aircraft`, `Building`, `Infantry`, `Vehicle` exact rows. |

## 4. Added Exact Object Context Rows

39 new exact context rows were added:

```text
Primary / Aircraft
Primary / Building
Primary / Infantry
Primary / Vehicle
Secondary / Aircraft
Secondary / Building
Secondary / Infantry
Secondary / Vehicle
Strength / Aircraft
Strength / Building
Strength / Infantry
Strength / Vehicle
Speed / Aircraft
Speed / Infantry
Speed / Vehicle
TechLevel / Aircraft
TechLevel / Building
TechLevel / Infantry
TechLevel / Vehicle
Cost / Aircraft
Cost / Building
Cost / Infantry
Cost / Vehicle
Armor / Aircraft
Armor / Building
Armor / Infantry
Armor / Vehicle
Sight / Aircraft
Sight / Building
Sight / Infantry
Sight / Vehicle
Owner / Aircraft
Owner / Building
Owner / Infantry
Owner / Vehicle
Prerequisite / Aircraft
Prerequisite / Building
Prerequisite / Infantry
Prerequisite / Vehicle
```

## 5. Updated Existing Canonical Rows

Updated existing source-backed rows:

```text
Primary / Techno
Secondary / Techno
Strength / Techno
Strength / Projectile
Speed / Techno
Speed / Weapon
TechLevel / Techno
Cost / Techno
Armor / Techno
Sight / Techno
Owner / Techno
Prerequisite / Techno
```

## 6. Non-canonical Guardrails

Updated wrong-context or broad-context guardrail rows:

```text
Speed / Global
TechLevel / AI
TechLevel / Global
Cost / Global
Armor / Global
Armor / Projectile
Sight / AI
Owner / AI
Prerequisite / AI
```

These rows intentionally prevent older imported or broad fallback text from polluting Hover. They are not canonical descriptions for those contexts.

## 7. Deferred / Not Modified

The following known row remains outside this phase:

```text
Strength / Shield
```

Reason: this appears to be a Shield / Phobos-style context rather than a ModEnc TechnoTypes common-field context. It should be verified in a later Ares/Phobos-specific batch instead of being guessed from the TechnoTypes `Strength` page.

## 8. Data Delta

```text
BuiltIn v3.2 field count: 4662 -> 4701
New exact object-context rows: 39
Existing direct rows updated or guarded: 21
Exact `数值型字段` rows: 0 -> 0
Exact `整数型字段` rows: not fully cleared; Strength / Shield remains deferred
```

## 9. Validation

Static validation completed in the patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Expected verification doc: present
```

Not run in the patch environment:

```text
dotnet restore
dotnet build
dotnet test
```

Reason: dotnet CLI is unavailable in this environment.

## 10. Next Step

Recommended next phase:

```text
FR-DQ-2I-TechnoTypes-CombatMobility-ManualApply
```

Suggested next field family:

```text
ROF / Reload / GuardRange / ROT / Locomotor / MovementZone / SpeedType / MovementRestrictedTo
```

Continue source-family batches rather than returning to 5-field micro-batches. Keep Ares/Phobos-specific fields in their own batches.
