# RA2IniEditor IDE Handoff v0.4.50-v0.4.51

## Scope

This slice continues the ShellWindow responsibility-map extraction after
v0.4.49 Language Navigation.

Completed stages:

- v0.4.50 Source Editor Hover Controller Extraction.
- v0.4.51 Completion Interaction Controller Extraction.

## What Changed

- Added `RA2IniEditor.IDE/Controllers/Hover/Ra2SourceEditorHoverController.cs`.
- Added `RA2IniEditor.IDE/Controllers/Completion/Ra2CompletionInteractionController.cs`.
- `ShellWindow` now delegates hover decision and completion orchestration to
  IDE-internal controllers.
- `ShellWindow` still owns WPF-only work:
  - AvalonEdit mouse coordinate to document offset.
  - DispatcherTimer start/stop.
  - Hover Popup creation and closing.
  - Completion Popup placement and focus.
  - Source editor text synchronization.
  - Caret restore after completion commit.

## Controller Boundaries

`Ra2SourceEditorHoverController` owns:

- Hover pointer-move decision.
- Pending hover offset state.
- Key-token hover filtering.
- Hover provider invocation.
- Tooltip text creation from hover display view model.

It does not reference WPF, AvalonEdit, save services, or ObjectAggregator.

`Ra2CompletionInteractionController` owns:

- Completion provider invocation.
- Completion display enhancement.
- Completion commit coordinator invocation.
- Skip/failure/success commit messages.

It does not reference WPF, AvalonEdit, save services, or ObjectAggregator.

## Guardrails

- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or legacy save dependency.
- No Core or Infrastructure public API change.
- No Completion provider behavior change.
- No Completion commit planner/coordinator behavior change.
- No Hover provider behavior change.
- No Add Property behavior change.
- No Edit / Revert / dirty behavior change.

## Verification

- `dotnet test -c Release`: passed, 791 tests.
- `dotnet build -c Release --no-incremental`: passed, 0 errors, 26 existing warnings.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open an INI file.
3. Hover a known key and confirm tooltip still appears.
4. Move mouse away, scroll, move caret, and confirm hover closes.
5. Open completion with Ctrl+Space or menu.
6. Commit a completion in edit mode and confirm text/caret behavior.
7. Try committing completion outside edit mode and confirm it is skipped without text changes.
8. Smoke Add Property, Edit Mode, Revert, Go To Definition, Peek Definition, and Find References.

## Next Stage

The next responsibility-map candidate is Add Property / Field Browser extraction.
That stage should be kept separate because it crosses annotation loading,
duplicate-key actions, editable session state, and in-memory text application.
