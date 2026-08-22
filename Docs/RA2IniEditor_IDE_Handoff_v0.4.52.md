# RA2IniEditor IDE Handoff v0.4.52

## Scope

This slice extracts the next ShellWindow responsibility-map stage:

- Add Property / Field Browser controller orchestration.

## What Changed

- Added `RA2IniEditor.IDE/Controllers/FieldBrowser/Ra2FieldBrowserController.cs`.
- `ShellWindow` now delegates Add Property request creation, confirmation
  routing, insert/replace planning, and in-memory text change application to
  the field browser controller.
- `ShellWindow` still owns WPF-only work:
  - Opening `Ra2AddPropertyWindow`.
  - Opening `Ra2FieldAnnotationEditorWindow`.
  - Reading AvalonEdit caret offset.
  - Synchronizing the returned text/session/caret result back into AvalonEdit.
  - Updating editor state controls and restoring focus.

## Controller Boundary

`Ra2FieldBrowserController` owns:

- `Ra2AddPropertyViewModel` construction.
- Field annotation load result mapping for the Add Property window.
- Confirmed action routing:
  - cancelled,
  - jump existing,
  - requires edit mode,
  - replace existing,
  - insert duplicate/new field.
- `Ra2AddPropertyInsertPlanner` insert/replace planning.
- `IRa2TextChangeApplier` in-memory application.
- Pure action results containing updated editable session, source text, caret
  offset, and output message.

It does not reference WPF, AvalonEdit, save services, or ObjectAggregator.

## Guardrails

- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or legacy save dependency.
- No Core or Infrastructure public API change.
- No Completion provider or commit behavior change.
- No Hover provider behavior change.
- No editable session service behavior change.
- No disk save behavior.

## Verification

- `dotnet test -c Release`: passed, 797 tests.
- `dotnet build -c Release --no-incremental`: passed, 0 errors, 26 existing warnings.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open an INI file and enter edit mode.
3. Open Add Property.
4. Add a new property and confirm it appears in memory.
5. Try adding a duplicate property:
   - jump existing,
   - replace existing,
   - insert duplicate,
   - cancel.
6. Open annotation editor from Add Property and confirm refresh still works.
7. Smoke Completion, Hover, Go To Definition, Peek Definition, Find References, Edit Mode, and Revert.

## Note

Some damaged legacy mojibake status strings in `ShellWindow.xaml.cs` were
normalized to English while restoring the file back to UTF-8 after a local text
rewrite. This only affects output/status text, not editor state or save behavior.
