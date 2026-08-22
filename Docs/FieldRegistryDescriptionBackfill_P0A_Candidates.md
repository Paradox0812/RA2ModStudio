# Field Registry Description Backfill P0A Candidates

Phase: FR-DQ-2A Effective P0 Backfill Candidate Preparation

This candidate list is based only on `Docs/FieldRegistryEffectiveDescriptionAudit.md`. It does not use the raw missing list as patch input and does not modify Field Registry JSON, source code, XAML, Hover, Quick Peek, AI Evidence, or runtime behavior.

## Selection Rules

Included rows meet all of these conditions:

- `Effective Description Status` is `Missing`, `Placeholder`, or `LowQuality`.
- `Needs Backfill = Yes`.
- The field is P0 or listed under P0 effective missing / placeholder themes.

Excluded rows:

- `Effective Description Status = Valid`.
- `Needs Backfill = No`.
- Raw-list false positives such as `Name / Infantry`, `Armor / common object contexts`, `Cost / common object contexts`, `Owner / common object contexts`, `Primary / common object contexts`, and `UIName / common object contexts`.

`SuggestedDescriptionZh` is intentionally not filled with final text. Do not fabricate field descriptions before online/source verification.

## Candidate Summary

Some effective gaps are intentionally listed in more than one thematic batch. For example, `Projectile` / `Verses` / `PercentAtMax` unknown fallback gaps are relevant to both Batch A combat review and Batch C unknown fallback review, and `ThreatPosed / AI` is relevant to both Batch B behavior review and Batch D AI context review.

| Batch | Theme | Listed rows |
|---|---|---:|
| Batch A | Combat / Weapon / Warhead basics | 19 |
| Batch B | Techno fallback and unit behavior gaps | 5 |
| Batch C | Non-canonical / Unknown fallback gaps | 7 |
| Batch D | AI context gaps | 4 |
| Total | Listed candidate rows | 35 |

| Count Type | Missing | Placeholder | LowQuality | Total |
|---|---:|---:|---:|---:|
| Listed rows | 13 | 19 | 3 | 35 |
| Unique effective rows | 10 | 19 | 2 | 31 |

## Batch A: Combat / Weapon / Warhead Basics

| Key | SectionKind / Schema | Effective Source | Effective Description Status | Current Effective Description | Problem Type | Suggested Verification Source | Proposed Source Trust | SuggestedDescriptionZh | NeedsOnlineVerification | ReadyToApply | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Damage | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Verify Weapon meaning separately before deciding target section-kind rows. |
| Damage | ArtObject | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Art text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | ArtObject context may differ from Weapon damage. |
| ROF | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Candidate should verify rate-of-fire semantics and accepted value shape. |
| Range | AI, Aircraft, Animation, ArtObject, Building, Country, Global, Infantry, Projectile, Side, Techno, Unit, Vehicle, VoxelAnimation, Warhead, SuperWeapon, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Important P0 weapon field lacks effective description in this environment. |
| Projectile | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno reference value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Verify Weapon `Projectile=` meaning and whether Techno fallback is appropriate. |
| Projectile | AI, Animation, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Unknown fallback gap; do not backfill from raw missing alone. |
| Warhead | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno reference value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Verify Weapon `Warhead=` meaning and reference target. |
| Warhead | ArtObject | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Art reference value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Art context may need separate verification. |
| Verses | Aircraft, Building, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Warhead damage multiplier semantics need source verification. |
| Verses | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Unknown/global fallback gap. |
| Verses | Infantry | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Local imported row should not be copied forward unchanged. |
| CellSpread | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Warhead area/falloff behavior needs source verification. |
| CellSpread | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Missing outside canonical context. |
| PercentAtMax | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Warhead damage falloff semantics need source verification. |
| PercentAtMax | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Missing outside canonical context. |
| AA | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Verify whether context is Projectile/Weapon/Techno for final backfill target. |
| AA | AI, Animation, ArtObject, Country, Global, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Unknown/global fallback gap. |
| AG | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Verify whether context is Projectile/Weapon/Techno for final backfill target. |
| AG | AI, Animation, ArtObject, Country, Global, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Unknown/global fallback gap. |

## Batch B: Techno Fallback and Unit Behavior Gaps

| Key | SectionKind / Schema | Effective Source | Effective Description Status | Current Effective Description | Problem Type | Suggested Verification Source | Proposed Source Trust | SuggestedDescriptionZh | NeedsOnlineVerification | ReadyToApply | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|
| BuildCat | Aircraft, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | `BuildCat / Building` is excluded because its effective description is valid. |
| Crewed | Aircraft, Infantry, Techno, Unit, Vehicle | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | `Crewed / Building` is excluded because its effective description is valid. |
| Turret | Aircraft, Unit, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, Techno text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | BuiltIn fallback contexts need verification. |
| Turret | Building, Infantry, Vehicle | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Local imported placeholder should not be copied forward unchanged. |
| ThreatPosed | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | LowQuality | 数值型字段 | LowQuality | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Common object contexts are excluded because effective descriptions are valid. |

## Batch C: Non-canonical / Unknown Fallback Gaps

| Key | SectionKind / Schema | Effective Source | Effective Description Status | Current Effective Description | Problem Type | Suggested Verification Source | Proposed Source Trust | SuggestedDescriptionZh | NeedsOnlineVerification | ReadyToApply | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Name | Animation, ArtObject, Country, Projectile, Side, Warhead, Weapon, Global, SuperWeapon, Unknown, VoxelAnimation | Global: User Import / User | LowQuality | Great Britain 说明文字,随便添,没什么意义 | LowQuality | Unknown | Unknown | 待联网核验后填写 | true | false | `Name / Infantry`, `Name / Vehicle`, `Name / Aircraft`, and `Name / Building` are explicitly excluded as effective-valid false positives. |
| Strength | AI, Animation, ArtObject, Country, Global, Side, SuperWeapon, VoxelAnimation, Warhead, Weapon, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Common object contexts are excluded because effective descriptions are valid. |
| Sight | Animation, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Warhead, Weapon, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Common object contexts are excluded; `Sight / AI` is tracked in Batch D. |
| Locomotor | AI, Animation, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Vehicle/Infantry/Aircraft contexts are excluded because effective descriptions are valid. |
| Projectile | AI, Animation, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Also included in Batch A because it is a combat field; keep here as the Unknown fallback gap. |
| Verses | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Also included in Batch A because it is a Warhead field. |
| PercentAtMax | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import / User | Missing | (empty) | Missing | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Also included in Batch A because it is a Warhead field. |

## Batch D: AI Context Gaps

| Key | SectionKind / Schema | Effective Source | Effective Description Status | Current Effective Description | Problem Type | Suggested Verification Source | Proposed Source Trust | SuggestedDescriptionZh | NeedsOnlineVerification | ReadyToApply | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Owner | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, AI text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Common object contexts are excluded because effective descriptions are valid. |
| Prerequisite | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, AI text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Common object contexts are excluded because effective descriptions are valid. |
| Sight | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | BuiltIn placeholder: YR reference field, AI text value, raw English moved to review table, not direct Hover text. | Placeholder | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Common object contexts are excluded because effective descriptions are valid. |
| ThreatPosed | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | LowQuality | 数值型字段 | LowQuality | ModEnc / RA2-YR docs | Unknown | 待联网核验后填写 | true | false | Also listed in Batch B as unit behavior gap; AI context needs verification before any backfill. |

## Excluded Effective-Valid False Positives

The following examples are deliberately excluded from the candidate tables:

- `Name / Infantry`, `Name / Vehicle`, `Name / Aircraft`, `Name / Building`.
- `Armor / Aircraft`, `Armor / Building`, `Armor / Infantry`, `Armor / Vehicle`.
- `Cost / Aircraft`, `Cost / Building`, `Cost / Infantry`, `Cost / Vehicle`.
- `Owner / Aircraft`, `Owner / Building`, `Owner / Infantry`, `Owner / Techno`, `Owner / Vehicle`.
- `Primary / Aircraft`, `Primary / Building`, `Primary / Infantry`, `Primary / Techno`, `Primary / Vehicle`.
- `UIName / Aircraft`, `UIName / Building`, `UIName / Infantry`, `UIName / Vehicle`.

These rows must not be re-added as backfill candidates unless a later effective audit shows they are no longer valid.

## Next Review Step

Recommended next phase: online/source verification for Batch A. After verified source text is available, prepare a separate patch plan for the exact pack rows to update. Do not write Field Registry JSON in this phase.
