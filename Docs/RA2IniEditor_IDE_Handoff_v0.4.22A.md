# RA2IniEditor IDE Handoff v0.4.22A

## Version

- Target: v0.4.22A Field Registry Harvest Preview UI
- Baseline: v0.4.21 Harvest Normalize / Validate / Preview-only Pipeline

## Completed

v0.4.22A adds a local, manual preview window for field registry harvest experiments.

The user flow is:

```text
Field Registry Manager
  -> Open Harvest Preview
  -> paste raw markdown/text
  -> Parse & Preview
  -> inspect candidates, raw warnings, normalize issues, and preview definitions
```

The feature is preview-only. It does not write local field packs and does not update the active field registry provider.

## New UI

- `FieldRegistryHarvestPreviewWindow`
- Entry point: `FieldRegistryManagerWindow` button `Open Harvest Preview`
- Window title: `Field Registry Harvest Preview - No Apply`
- The window is non-modal.
- Repeated open requests activate the existing window.
- Closing the preview window allows it to be opened again.

## New ViewModels

- `FieldRegistryHarvestPreviewViewModel`
- `FieldRegistryHarvestCandidateViewModel`
- `FieldRegistryHarvestIssueViewModel`
- `FieldRegistryHarvestDefinitionPreviewViewModel`
- `FieldRegistryHarvestWarningViewModel`

The view model composes the existing pure in-memory pipeline:

```text
MarkdownFieldRegistryHarvestParser
FieldRegistryHarvestNormalizer
FieldRegistryHarvestPreviewBuilder
```

## Displayed Data

Tabs:

- Parsed Candidates: key, raw applies-to, raw editor kind, confidence, source, line.
- Normalize Issues: severity, key, source, line, message.
- Preview Definitions: key, applies-to, editor kind, source kind, description.
- Raw Warnings: source, line, message.

Summary shows candidate, definition, issue, error, and warning counts plus future apply eligibility.

## Explicitly Not Implemented

- GitHub fetch
- Network access
- Raw docs downloader
- `active/*.fields.json` writes
- Apply / rollback
- Active pack backup
- Field registry editor
- Completion
- Save / dirty / INI editing
- IDE startup auto-harvest
- Open Folder auto-harvest
- Highlighter provider update
- Field Registry Manager reload integration

## Boundary Notes

`RA2IniEditor.Infrastructure` still keeps harvest contracts internal. `RA2IniEditor.IDE` receives friend assembly access via `InternalsVisibleTo` so the UI can call the prototype pipeline without making harvest types public.

`RA2IniEditor.IDE` also exposes internals to tests for ViewModel coverage.

## Tests

Added tests cover:

- Markdown table preview populates candidates and definitions.
- Empty input clears preview state.
- Invalid key input produces raw parser warning without crashing.
- Duplicate raw candidates keep one definition and expose a warning.
- Clear resets raw text and result collections.
- Guardrails against network, file write, active pack write, apply command, rollback command, ProjectSaveService, and Completion entrypoints.

## Next Suggested Step

Prefer v0.4.22B: preview diff against currently loaded active field definitions while still not applying changes.

GitHub fetch should remain separate until preview and diff semantics are stable.
