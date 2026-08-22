# RA2IniEditor IDE Handoff v0.4.57

## Scope

This slice documents the Source Editor boundary before any controller extraction.

It adds a data-only sync plan model and guardrail tests. Runtime behavior is not
changed.

## What Changed

- Added `Ra2SourceEditorSyncOperationKind`.
- Added `Ra2SourceEditorSyncPlan`.
- Added `RA2IniEditor_IDE_SourceEditor_Boundary_Map_v0.4.57.md`.
- Added tests for the boundary map, sync plan model, and `ShellWindow` guardrails.

## Preserved ShellWindow Responsibilities

`ShellWindow` still owns:

- AvalonEdit `SourceTextEditor` event handlers.
- Programmatic text sync through `SetEditorTextFromProgram`.
- `_isSynchronizingEditorText`.
- AvalonEdit `Document.Text`, caret, focus, readonly state, popup placement, and scroll.
- Completion and Hover transient UI closure.

## Guardrails

- No runtime `Ra2SourceEditorController` extraction.
- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or IniFileService dependency.
- No Completion commit behavior change.
- No Add Property insert / replace behavior change.
- No Revert behavior change.
- No Core or Infrastructure public API change.

## Verification

- `dotnet test -c Release`: passed, 829 tests.
- `dotnet build -c Release --no-incremental`: passed, 0 errors, 26 existing warnings.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open an INI file.
3. Enter Edit Mode.
4. Type in the editor and confirm in-memory modified state updates.
5. Use Completion commit and confirm caret/focus remain stable.
6. Use Add Property insert / replace and confirm editor text updates.
7. Revert and confirm readonly preview returns.
8. Confirm no disk INI file is saved by this preview.
