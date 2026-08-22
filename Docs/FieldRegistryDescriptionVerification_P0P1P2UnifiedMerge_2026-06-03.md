# FR-DQ-3G P0/P1/P2 Unified BuiltIn Merge

## Summary

| Metric | Value |
|---|---:|
| Runtime BuiltIn field count | 4942 |
| DirectFix applied | 308 |
| Guardrail applied | 129 |
| KeepInferred applied | 103 |
| RemoveOrBacklog removed | 167 |
| RemoveOrBacklog safety-kept | 8 |
| ManualReview skipped | 34 |

## Quality distribution

| Quality bucket | Count |
|---|---:|
| `source-verified*` | 2187 |
| `inferred*` | 1590 |
| `auto-extracted*` | 810 |
| `community-reviewed*` | 218 |
| `source` | 65 |
| `manual-curated*` | 39 |
| `community` | 20 |
| `noncanonical` | 8 |
| `empty` | 5 |

## Per-stage validation

| Stage | Field count | Duplicates | Bad appliesTo | Bad editorKind | Bad schema | schema.type=Text | needs-more-evidence | Direct Hover risk |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Initial 3F baseline | 5109 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| After DirectFix | 5109 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| After Guardrail | 5109 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| After KeepInferred | 5109 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| After RemoveOrBacklog | 4942 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |

## Notes

- Applied automatically mergeable buckets from the unified P0/P1/P2 workbook: DirectFix, Guardrail, KeepInferred, and safe RemoveOrBacklog decisions.
- ManualReview rows were intentionally not changed because their workbook decision is to keep them for human review.
- RemoveOrBacklog rows with ambiguous `KeepAsInferredFallbackOrBacklog` were kept as inferred fallback instead of removed.
- `BuildSpeed / Global` was safety-kept because the current row is manual-curated and the previous 3G-B production/economy pass treated it as a real Global field; it should be source-refined in a later targeted pass rather than removed by generic P1 backlog logic.
- No provider priority, lookup/fallback/enrichment, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, user Global active pack, or legacy behavior was changed.

Removed/backlog rows are listed in `Docs/FieldRegistryP0P1P2UnifiedMerge_RemovedOrBacklog_2026-06-03.csv`.

## 3G-A/B/C detailed overlay pass

The unified workbook was applied first, then the more specific 3G-A/B/C phase workbooks were overlaid for rows where the unified sheet had intentionally generic descriptions.

| Metric | Value |
|---|---:|
| A_overlay | 76 |
| B_update | 15 |
| B_removed_wrong_context | 19 |
| C_removed | 45 |
| C_update | 19 |
| Overlay removed wrong-context rows | 64 |
| Final runtime BuiltIn field count | 4878 |

Known corrections from the detailed overlay include BaseDefenseDelay / Global, BuildSpeed / Global, HoverHeight / Global, and removal of corresponding wrong-context inferred duplicates where the detailed phase identified them.
