# Field Registry Description Verification - Ares / Phobos Extensions MegaBatch

Phase: FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply

## 1. Scope

This batch processes the remaining high-risk Ares / Phobos extension rows around:

- AttachEffect / AttachEffectType
- Shield / ShieldType and Warhead shield interactions
- LaserTrail / LaserTrailType
- DigitalDisplay
- Veterancy Insignia
- Custom RadiationType
- selected Warhead / Weapon / Building / Vehicle extension rows connected to the same source pages

The goal is to remove direct Hover placeholders without inventing definitions. Rows with reliable documentation were changed to source-backed descriptions; rows whose sources remain insufficient were changed to explicit `NeedsMoreEvidence` guardrails.

## 2. Sources Used

- Phobos New / Enhanced Logics: AttachEffect, custom RadiationTypes, Laser Trails, Shields, Warhead shield interactions, Weapon / Warhead / Techno extension hooks.
- Phobos User Interface: Digital Display.
- Phobos Fixed / Improved Logics: custom veterancy insignias and pip customizations.
- Ares Warhead Iron Curtain documentation.
- Ares Permanent Mind-Control documentation.

## 3. Result Summary

| Metric | Value |
|---|---:|
| Rows affected | 200 |
| Source-backed rows | 192 |
| Non-canonical guardrail rows | 4 |
| NeedsMoreEvidence guardrail rows | 4 |
| Direct placeholder / generic target rows remaining | 0 |

## 4. Source-backed Categories

### AttachEffect / AttachEffectType

Updated all direct `AttachEffect` rows in this batch with safe descriptions based on Phobos AttachEffectType semantics, including duration, cumulative behavior, animation behavior, discard conditions, multipliers, tint, reflect damage, revenge weapon and weapon range modifiers.

### Shield / ShieldType

Updated direct `Shield` rows with source-backed shield semantics: shield strength, initial strength, condition thresholds, pip settings, respawn, self-healing, idle / hit / break animations, tint and armor inheritance.

### Warhead shield interactions

Updated `Shield.* / Warhead` rows as Warhead-side shield interaction overrides, including shield penetration, break, hit/break animation override, damage range override, respawn/self-heal override and ShieldType attach/remove filters.

### LaserTrail / LaserTrailType

Updated LaserTrailType rows such as `DrawType`, `FadeDuration`, `SegmentLength`, `Color`, `Thickness`, `IsIntense`, `Beam.*` and `Bolt.*`.

`LaserTrailN.* / LaserTrail` and `LaserTrail.Types / LaserTrail` were changed to non-canonical guardrails because the source places them on Techno/Projectile/VoxelAnim image entries, not on LaserTrailType itself.

### DigitalDisplay

Updated DigitalDisplay rows such as `InfoType`, `InfoIndex`, `Align`, `Anchor.*`, `Offset.*`, `Shape`, `Palette`, `ShowType`, `Text.Color*`, `ValueScaleDivisor` and `VisibleToHouses`.

### Insignia

Updated Insignia rows with Phobos veterancy insignia semantics, including custom insignia files and zero-based frame selection for Rookie / Veteran / Elite stages.

### Radiation

Updated custom RadiationType rows including application delay, building delay, damage max count, level/light factors, color, tint factor and RadSiteWarhead.

## 5. NeedsMoreEvidence Rows

The following rows were intentionally not converted into canonical definitions:

| Key | SectionKind | Reason |
|---|---|---|
| tempValue | AI | Source insufficient for specific AI semantics. |
| VeteranLevel | AI | Source insufficient for AI context semantics. |
| Storage.TiberiumIndex | Global | Found related pip / storage display information, but not enough to confirm Global semantics. |
| AffectsVeterancy | Warhead | Requires follow-up to confirm exact Phobos Warhead veterancy behavior. |

These are tracked in `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.

## 6. Non-canonical Guardrails

The following representative rows are guardrails rather than canonical definitions:

- `LaserTrailN.FLH / LaserTrail`
- `LaserTrailN.Type / LaserTrail`
- `LaserTrailN.IsOnTurret / LaserTrail`
- `LaserTrail.Types / LaserTrail`

They now explain the source-confirmed context and prevent old placeholder text from polluting Hover.

## 7. No Runtime Changes

This batch did not modify:

- Field Registry provider priority
- lookup / fallback / enrichment
- Hover code
- Quick Peek
- AI Evidence
- PromptBuilder
- parser / diagnostics / completion / save preflight
- XAML / UI
- project files
- legacy editor
