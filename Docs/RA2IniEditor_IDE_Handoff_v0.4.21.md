# RA2IniEditor IDE Handoff v0.4.21

## Version

- Target: v0.4.21 Harvest Normalize / Validate / Preview-only Pipeline
- Baseline: v0.4.20 Harvest Parser Contract / Prototype

## Completed

v0.4.21 adds a pure in-memory pipeline that converts harvest parser candidates into reviewable preview draft definitions:

```text
raw text
  -> MarkdownFieldRegistryHarvestParser
  -> FieldRegistryHarvestNormalizer
  -> FieldRegistryHarvestPreviewBuilder
  -> FieldRegistryHarvestPreviewDraft
```

The pipeline stays inside `RA2IniEditor.Infrastructure/FieldRegistry/Harvest/` and does not connect to IDE startup, Open Folder, diagnostics, highlighting, or the Field Registry Manager UI.

## New Internal Contracts

- `FieldRegistryHarvestNormalizeOptions`
- `FieldRegistryHarvestNormalizedCandidate`
- `FieldRegistryHarvestValidationSeverity`
- `FieldRegistryHarvestValidationIssue`
- `FieldRegistryHarvestNormalizeResult`
- `IFieldRegistryHarvestNormalizer`
- `FieldRegistryHarvestNormalizer`
- `FieldRegistryHarvestPreviewDraft`
- `IFieldRegistryHarvestPreviewBuilder`
- `FieldRegistryHarvestPreviewBuilder`

No public API was added for IDE consumers.

## Normalize Rules

- Field keys are trimmed and must contain only letters, digits, `_`, `.`, or `-`.
- Empty or invalid keys become validation errors and are skipped.
- `AppliesToRaw` supports common `Ra2SectionKind` names and aliases such as `Inf`, `Veh`, `Bld`, `WH`, `Proj`, `SW`, and `Terr`.
- Multiple applies-to values can be separated by comma, semicolon, or slash.
- Missing applies-to uses the configured default and records an info issue.
- Unknown applies-to maps to `Unknown` with a warning when allowed; otherwise it becomes an error.
- `EditorKindRaw` supports current `FieldEditorKind` values and aliases such as `String`, `Int`, `Double`, `Bool`, `YesNo`, and `List`.
- Missing editor kind uses the configured default and records an info issue.
- Unknown editor kind maps to the default with a warning when allowed; otherwise it becomes an error.
- `SourceKind` is always taken from normalize options. The pipeline does not infer source kind from source names.

## Duplicate Rules

Duplicates are detected by:

```text
case-insensitive key + same appliesTo set
```

When duplicates occur:

- Higher confidence replaces lower confidence.
- Same or lower confidence is skipped.
- A warning is recorded either way.
- Same key with different applies-to values is allowed.

## Preview-only Rules

`FieldRegistryHarvestPreviewBuilder` converts normalized candidates to `Ra2FieldDefinition` draft objects in memory only.

- `CanApplyInFuture = true` when no error issues exist.
- `CanApplyInFuture = false` when at least one error issue exists.
- Warning and info issues do not block future apply.
- No JSON is generated.
- No active pack is written.
- No backup or rollback is performed.

## Explicitly Not Implemented

- GitHub fetch
- Network access
- Raw docs downloader
- `active/*.fields.json` writes
- Preview apply button
- Apply / rollback
- Backup active pack
- Field registry editor
- Completion
- Save / dirty / editing
- IDE startup normalize
- Open Folder normalize
- Highlighter normalize
- Diagnostics normalize

## Tests

Added tests cover:

- Key normalization and invalid key errors.
- Applies-to aliases, multi-value parsing, defaults, unknown handling.
- Editor kind aliases, defaults, unknown handling.
- Duplicate handling by key and applies-to set.
- Preview draft `CanApplyInFuture`, error/warning counts, and definition creation.
- Parser -> normalizer -> preview smoke pipeline.

## Next Suggested Step

The safer next step is v0.4.22A: a user-triggered local preview UI where users paste raw markdown/text and inspect parser candidates, normalize issues, and preview definitions without applying them.

GitHub fetch should remain a separate spike after the preview experience proves useful.
