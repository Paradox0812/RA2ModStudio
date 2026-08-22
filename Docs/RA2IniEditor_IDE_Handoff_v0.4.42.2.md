# RA2IniEditor IDE Handoff v0.4.42.2

## Scope

v0.4.42.2 fixes a completion result lifecycle issue in the IDE shell. The bug could show `Completion commit skipped: completion result is unavailable.` even when the dropdown was visible and edit mode was active.

## Root Cause

`ShowCompletionDropdown` stored `_lastCompletionResult` before popup positioning. The positioning helper then normalized the AvalonEdit caret by assigning `SourceTextEditor.TextArea.Caret.Offset`. That caret assignment triggered `SourceTextEditorCaret_OnPositionChanged`, which closed the dropdown and cleared `_lastCompletionResult`. The popup was opened again afterward, leaving visible candidates without the replacement span needed for commit.

## Fixed Behavior

- Popup positioning no longer assigns to the AvalonEdit caret.
- `ShowCompletionDropdown` stores `_lastCompletionResult` after popup positioning and immediately before opening the popup.
- `CloseCompletionDropdown` now supports optional result preservation through `clearCompletionResult`.
- User-driven caret movement still closes the dropdown and clears the active completion result.

## Guardrails

- No INI save pipeline changes.
- No dirty state changes.
- No disk writes.
- No AvalonEdit `CompletionWindow`.
- No Core or Infrastructure public API changes.
- No CompletionProvider semantic changes.
- No legacy save, ObjectAggregator, ProjectLoader, or ProjectSaveService integration.

## Tests

Added guardrails for:

- Popup positioning must not assign caret offset.
- `ShowCompletionDropdown` must assign `_lastCompletionResult` after positioning.
- `CloseCompletionDropdown` must allow preserving completion result when needed.

## Manual Smoke Checklist

1. Open the IDE shell and load an INI file.
2. Enter edit mode.
3. Type `Pr`.
4. Press `Ctrl+Space`.
5. Commit `Primary` with Enter.
6. Revert in-memory changes.
7. Repeat with Tab.
8. Repeat by double-clicking `Primary`.
9. Confirm `Completion commit skipped: completion result is unavailable.` no longer appears during edit-mode commit.
10. Confirm the file on disk is not changed.
