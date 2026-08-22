# RA2IniEditor IDE Handoff v0.4.23A

## 1. Version Goal

v0.4.23A adds an Active Pack Provenance Read Model for Field Registry Import Preview.

The goal is read-only provenance: preview diff rows can now show where the existing effective field definition comes from:

- Project active field pack
- Global active field pack
- BuiltIn field provider
- None when the field does not exist
- Unknown as defensive fallback

This version does not write field packs and does not implement apply, rollback, backup, GitHub fetch, Completion, save, dirty, or INI editing.

## 2. Provenance Lookup

Lookup order matches the current effective field registry priority:

1. Project
2. Global
3. BuiltIn

Local Project and Global lookup follows the existing provider fallback shape:

1. Exact `Ra2SectionKind`
2. `Global`
3. `Unknown`

This keeps the v0.4.19.1 priority fix intact. For example, if Project has `Owner` for `Unknown` and Global has `Owner` for `Infantry`, an `Infantry + Owner` lookup reports Project provenance.

## 3. New Read Model

Infrastructure now contains an internal provenance model:

- `FieldRegistryProvenanceScope`
- `FieldRegistryProvenanceEntry`
- `FieldRegistryProvenanceLookupResult`
- `IFieldRegistryProvenanceProvider`
- `FieldRegistryProvenanceSnapshot`
- `FieldRegistryProvenanceSnapshotBuilder`

The snapshot is immutable after construction. It does not read files by itself; it only receives local load results and a BuiltIn provider.

## 4. Loader Source Tracking

`LocalFieldRegistryLoader` now records source file information for each loaded definition without changing active pack JSON schema:

- `LocalFieldRegistryLoadedDefinition.Definition`
- `LocalFieldRegistryLoadedDefinition.SourceFileName`
- `LocalFieldRegistryLoadedDefinition.SourceFilePath`

`LocalFieldRegistryLoadResult.Definitions` remains available for existing callers. The additional source tracking is used only by the provenance read model.

## 5. Runtime Integration

`FieldRegistryRuntimeService` now maintains `CurrentProvenanceProvider`.

Important constraints:

- Initial state falls back to BuiltIn provenance.
- `Reload(projectRootPath)` rebuilds the provenance snapshot from the latest Project and Global local load results.
- `CurrentProvider` behavior is unchanged.
- Highlighter provider reload behavior is unchanged.
- Reload remains user-driven; the import preview does not trigger registry reload.

## 6. Field Import Preview Diff

`FieldRegistryHarvestDiffService` keeps the existing effective-provider compare path and adds a provenance-provider compare path.

Diff rows now include:

- `ExistingScope`
- `ExistingSourceName`
- `ExistingSourcePath`

The Field Import Preview `Preview Diff` tab displays:

- Existing Scope
- Existing Source
- Existing SourceKind

This separates registry provenance from `Ra2FieldSourceKind`.

## 7. Guardrails

v0.4.23A intentionally does not include:

- GitHub fetch
- Network access
- active pack write
- apply
- rollback
- backup active pack
- field registry editor
- Completion
- INI save
- dirty state
- INI editing
- ProjectSaveService
- legacy Analysis
- ObjectAggregator
- ProjectLoader
- automatic provider reload

## 8. Tests

Added or updated coverage:

- Provenance lookup priority: Project over Global over BuiltIn.
- Project `Unknown` fallback over Global exact section.
- Global `Unknown` fallback over BuiltIn.
- BuiltIn fallback.
- Not found returns `None`.
- Loader records source file per definition.
- Diff rows include existing scope and source name.
- Field Import Preview VM exposes provenance values on diff rows.
- Runtime service refreshes `CurrentProvenanceProvider`.
- Preview UI guardrail includes provenance columns and still rejects write/network/apply/rollback entry points.

## 9. Next Step

Recommended next phase:

v0.4.23B Apply/Rollback Design Contract

That phase should remain design-only and define target scope, backup strategy, rollback source, user confirmation flow, and guardrail tests before any active pack writer is introduced.
