# Field Registry Hover Quality Scan - 2026-06-03

Phase: FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply support scan

## 1. Scope

Scanned file:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
```

## 2. Current Result

```text
Total fields: 5069
Source-verified rows: 1718
Strict non-source-verified rows: 3351
Direct placeholder rows: 0
Exact `整数型字段` rows: 0
Exact `数值型字段` rows: 0
Direct Hover-risk rows: 0
NeedsMoreEvidence / unresolved guardrail rows: 0
```

## 3. Delta From Previous Scan

```text
Rows affected by current phase: 273
Direct Hover-risk rows: 273 -> 0
Direct placeholder rows: 251 -> 0
Exact integer generic rows: 22 -> 0
Exact numeric generic rows: 0 -> 0
Source-verified rows: 1718 -> 1718
NeedsMoreEvidence rows: 1594 -> 1867
```

## 4. Residual Direct Hover Risk By SectionKind

```text
No direct Hover-risk rows remain.
```

## 5. Notes

- FR-DQ-3A intentionally does not claim source verification for the remaining 273 risk rows.
- Rows without reliable page-level evidence were converted to explicit `NeedsMoreEvidence` guardrails to prevent placeholder or generic type labels from surfacing in Hover.
- `Docs/FieldRegistryUnresolvedRows_2026-06-03.md` is now the follow-up queue for unresolved source-family verification.
- Final audit should confirm direct placeholder, exact `整数型字段`, and exact `数值型字段` counts remain zero.


## FR-DQ-3C-UnresolvedRecheck-A Summary

```text
BuiltIn v3.2 field count: 5070
Source-verified rows: 1779
Strict non-source-verified rows: 3291
Direct placeholder rows: 0
Exact integer generic rows: 0
Exact numeric generic rows: 0
Direct Hover-risk rows: 0
NeedsMoreEvidence / unresolved guardrail rows: 0
```

This pass resolved a targeted subset of previously unresolved Aircraft / Weapon / Vehicle / TeamTypes rows through ModEnc, Ares, and Phobos documentation.


## FR-DQ-3D AI Schema Recheck Update

```text
BuiltIn v3.2 field count: 5109
Source-verified rows: 1870
NeedsMoreEvidence / unresolved guardrail rows: 0
Direct placeholder rows: 0
Exact integer generic rows: 0
Exact numeric generic rows: 0
Direct Hover-risk rows: 0
AI unresolved rows remaining: 4
```

FR-DQ-3D added precise `TeamType` / `TaskForce` / `Infantry` / `Building` / `Global` rows for AI programming fields and converted old `[AI]` legacy rows to source-backed guardrails where sources proved they were wrong-context rows.


## FR-DQ-3F Inferred Backlog Recovery Update

```text
BuiltIn v3.2 field count: 5109
Source-verified rows: 2051
Inferred fallback rows: 1590
NeedsMoreEvidence / unresolved guardrail rows: 0
Direct placeholder rows: 0
Exact integer generic rows: 0
Exact numeric generic rows: 0
Direct Hover-risk rows: 0
Unsupported schema.type=Text rows: 0
```

FR-DQ-3F restores the 3E runtime backlog under a relaxed evidence policy. These rows are explicitly marked as inferred fallback and must not be treated as source-verified.

FR-DQ-3F metric correction: field count 5109, source-verified rows 2051, inferred fallback rows 1591, unresolved rows 0, unsupported schema.type=Text rows 0, direct Hover-risk rows 0.
