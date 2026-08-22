# RA2IniEditor IDE Handoff v0.4.58

## Scope

This slice adds Phase 1 Source Editor sync planning helpers.

Runtime AvalonEdit behavior is unchanged. `ShellWindow` still owns the actual
editor write, caret/focus/scroll, readonly state, transient popup lifecycle, and
`_isSynchronizingEditorText`.

## What Changed

- Added `IRa2SourceEditorSyncPlanner`.
- Added `Ra2SourceEditorSyncPlanner`.
- Added tests for caret clamp behavior, sync plan generation, and boundary
  guardrails.
- Updated the SourceEditor boundary map to record the v0.4.58 planning layer.

## Planner Boundary

`Ra2SourceEditorSyncPlanner` owns only:

- `ClampCaretOffset`.
- Null text normalization for sync plans.
- Optional caret offset clamp before creating a plan.
- Data-only `Ra2SourceEditorSyncPlan` creation.

It does not reference WPF, AvalonEdit, save services, disk IO, Completion UI, or
Add Property UI.

## Preserved ShellWindow Responsibilities

`ShellWindow` still owns:

- `SetEditorTextFromProgram`.
- `_isSynchronizingEditorText`.
- `SourceTextEditor.Document.Text = text`.
- `SourceTextEditor.TextArea.Caret.Offset`.
- `SourceTextEditor.IsReadOnly`.
- `RestoreSourceEditorFocusAtCaret`.
- Hover and Completion transient UI closure.

## Guardrails

- No runtime SourceEditorController extraction.
- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or IniFileService dependency.
- No Completion commit behavior change.
- No Add Property insert / replace behavior change.
- No Revert behavior change.
- No Core or Infrastructure public API change.

## Verification

- `dotnet test -c Release`: passed, 842 tests.
- `dotnet build -c Release --no-incremental`: passed, 0 errors, 26 existing warnings.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open an INI file.
3. Enter Edit Mode.
4. Type manually and confirm Modified in Memory.
5. Commit a Completion item and confirm text and caret look normal.
6. Use Add Property insert / replace and confirm text and caret look normal.
7. Revert and confirm readonly preview returns.
8. Confirm disk INI files are unchanged by the preview.
