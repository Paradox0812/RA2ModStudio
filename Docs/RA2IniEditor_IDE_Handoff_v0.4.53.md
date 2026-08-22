# RA2IniEditor IDE Handoff v0.4.53

## Scope

This slice continues the ShellWindow responsibility-map extraction:

- Field Annotation refresh / provider / resolver coordination.

## What Changed

- Added `RA2IniEditor.IDE/Controllers/FieldAnnotations/Ra2FieldAnnotationCoordinator.cs`.
- `ShellWindow` now delegates annotation path resolution and annotation refresh
  result construction to the coordinator.
- Annotation save/apply behavior inside `Ra2FieldAnnotationEditorViewModel`
  remains unchanged.
- `ShellWindow` still owns WPF-only work:
  - Opening `Ra2FieldAnnotationEditorWindow`.
  - Wiring `AnnotationSaved`.
  - Refreshing the current Add Property view model.
  - Showing output messages.

## Coordinator Boundary

`Ra2FieldAnnotationCoordinator` owns:

- Project annotation sidecar path resolution.
- Sidecar load through `IRa2FieldAnnotationStore`.
- `Ra2FieldAnnotationProvider` construction.
- `Ra2FieldDisplayResolver` construction.
- `Ra2FieldAnnotationStatusViewModel` construction.
- Warnings/message packaging for refresh results.

It does not reference WPF, AvalonEdit, save services, editor text, or INI dirty state.

## Guardrails

- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or legacy save dependency.
- No Annotation Editor save/apply semantic change.
- No Completion commit behavior change.
- No Hover display format change.
- No Add Property insert/replace behavior change.
- No editable session / dirty state behavior change.
- No Core or Infrastructure public API change.

## Verification

- `dotnet test -c Release`: passed, 803 tests.
- `dotnet build -c Release --no-incremental`: passed, 0 errors, 26 existing warnings.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open an INI file and open Add Property.
3. Select a field and open the annotation editor.
4. Apply an annotation change and confirm the Add Property list refreshes.
5. Save and close an annotation change and confirm the window closes.
6. Confirm INI modified/dirty state is not changed by annotation sidecar save.
7. Smoke Hover and Completion display after annotation refresh.
