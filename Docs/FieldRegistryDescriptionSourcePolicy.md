# Field Registry Description Source Policy

Phase: FR-DQ-1 Description Source / Trust Policy

This policy defines how RA2IniEditor.IDE should classify and review field description sources before any future Field Registry description backfill. It is a documentation-only contract. It does not modify Field Registry JSON, provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML, or runtime code.

## 1. Baseline Rule

Description backfill candidates must be selected from effective runtime audit results, not raw JSON missing rows.

Use `Docs/FieldRegistryEffectiveDescriptionAudit.md` as the source of truth for candidate preparation. `Docs/FieldRegistryMissingDescriptionList.md` is useful only for raw data hygiene and must not be used directly as a patch input because it contains false positives.

## 2. Trust Levels

| Trust | Meaning | Backfill Use |
|---|---|---|
| Official | Official Westwood / RA2 / YR material, official Ares docs, official Phobos docs, or confirmed project documentation. | May be used as normal Hover / Quick Peek / AI Evidence description after review. |
| Community | ModEnc or reputable community documentation that is widely used and consistent with observed field usage. | May be used after review, with source noted in candidate notes or provenance. |
| Derived | Inferred from usage, examples, related keys, or accepted behavior, but not directly confirmed by official/community docs. | Must be marked as derived and should include caution wording or lower trust. |
| LocalImported | Existing user-import / Global active / Project active field description. | May explain current effective UI behavior, but should not be treated as authoritative without review. |
| Unknown | Not verified, unclear, missing, placeholder-like, or low quality. | Must remain in review status; do not write as final description. |

## 3. Backfill Rules

1. Official and Community descriptions may become normal field descriptions after review.
2. Derived descriptions must remain visibly lower trust. They should not imply official certainty.
3. Unknown fields must remain missing or verify-before-use until a trusted source is found.
4. Low-quality existing descriptions must not be copied forward unchanged.
5. If a source is unclear, conflicting, too generic, or value-like rather than descriptive, keep the candidate in review status.
6. Effective descriptions that are already valid must not be backfilled merely because a raw JSON row was missing.
7. Backfill must preserve Project > Global > BuiltIn priority. A data quality fix must not be implemented by changing provider priority or lookup behavior.

## 4. Explicit False Positive Exclusions

The following effective descriptions are excluded from backfill candidates even if they appeared in the raw missing list:

- `Name / Infantry`, plus `Name` in common object contexts where the effective Global description is valid.
- `Armor` in common object contexts.
- `Cost` in common object contexts.
- `Owner` in common object contexts.
- `Primary` in common object contexts.
- `UIName` in common object contexts.
- Other P0 rows where `Effective Description Status = Valid` and `Needs Backfill = No` in the effective audit.

## 5. Review Workflow

1. Start from `FieldRegistryEffectiveDescriptionAudit.md`.
2. Select only rows where `Effective Description Status` is `Missing`, `Placeholder`, or `LowQuality`, and `Needs Backfill = Yes`.
3. Assign a candidate batch and suggested verification source.
4. Leave `SuggestedDescriptionZh` empty or `待联网核验后填写` until source text is verified.
5. After verification, classify source trust as Official, Community, Derived, LocalImported, or Unknown.
6. Only then prepare a JSON patch phase for the chosen pack rows.

## 6. Runtime Boundaries

This source policy does not authorize changes to:

- Field Registry JSON.
- Field Registry provider priority.
- Field Registry loader / writer / apply / rollback / cleanup behavior.
- Hover, Quick Peek, or AI Evidence code.
- Parser, diagnostics, completion, or save preflight.
- XAML / UI.
- AI provider behavior.
- Project / solution files.
- Legacy files.
