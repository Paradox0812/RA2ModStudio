# RA2IniEditor IDE SourceEditor Boundary Map v0.4.57

## Scope

This slice documents the current Source Editor boundary and adds guardrails for
future extraction. It does not extract a runtime `Ra2SourceEditorController`.

Runtime behavior is unchanged:

- No Save Current File.
- No Save / Save All.
- No disk write path.
- No dirty/save integration beyond the existing in-memory editable session.
- No Completion behavior change.
- No Add Property insert / replace behavior change.
- No Revert behavior change.
- No AvalonEdit event routing change.

## Current Owner

`ShellWindow` still owns the AvalonEdit UI glue for `SourceTextEditor`.

Important fields:

- `SourceTextEditor`: the AvalonEdit `TextEditor` instance.
- `_boundSourceEditor`: the current view-model text binding source.
- `_isSynchronizingEditorText`: guard for programmatic text writes.
- `_editableSession`: current in-memory editable document session.
- `_completionDropdownViewModel`: transient Completion dropdown state.
- `_currentHoverPopup`: transient Hover popup state.

## Current Event Surface

`ShellWindow` subscribes directly to Source Editor UI events:

- `SourceTextEditor.TextChanged`
- `SourceTextEditor.PreviewKeyDown`
- `SourceTextEditor.TextArea.PreviewKeyDown`
- `SourceTextEditor.LostKeyboardFocus`
- `SourceTextEditor.MouseMove`
- `SourceTextEditor.MouseLeave`
- `SourceTextEditor.TextArea.Caret.PositionChanged`
- `SourceTextEditor.TextArea.TextView.ScrollOffsetChanged`
- `DataContextChanged` for binding the current `SourceEditorViewModel`

These remain WPF/AvalonEdit responsibilities and should not move into a pure
decision controller.

## Text Read Paths

Current Source Editor reads are UI-local and use `SourceTextEditor.Document.Text`
or `SourceTextEditor.Text`:

- Enter Edit Mode creates an editable session from current editor text.
- User `TextChanged` sends current text to `Ra2EditorSessionController.UpdateTextFromUser`.
- Add Property parses current text and uses the current caret offset.
- Completion builds context from current text and caret offset.
- Hover / Go To Definition / Peek / References build language context from current text.
- Project Explorer section jump resolves target section from current editor text.
- Issues jump resolves and scrolls within the current editor text.
- Completion dropdown placement reads caret visual position from AvalonEdit.

## Text Write Paths

All programmatic Source Editor writes should continue to funnel through
`SetEditorTextFromProgram`.

Current callers:

- Readonly file load / `SourceEditorViewModel.Text` refresh via `SetReadonlySourceText`.
- Revert in-memory changes.
- Completion commit.
- Add Property insert / replace.
- Future external reload or undo/redo style programmatic sync.

## Programmatic Sync Guard

`ShellWindow` owns `_isSynchronizingEditorText`.

Required order:

1. Close transient hover before the write.
2. Set `_isSynchronizingEditorText = true`.
3. Write `SourceTextEditor.Document.Text`.
4. Optionally set caret and scroll for programmatic insert / replace.
5. Reset `_isSynchronizingEditorText = false` in `finally`.

`SourceTextEditor_OnTextChanged` must ignore changes while
`_isSynchronizingEditorText` is true. This prevents programmatic writes from
being interpreted as user edits.

## Readonly / Editable Rules

Readonly/editable state is still UI glue:

- File load and project open call `ResetEditableSessionToReadOnly`.
- Revert calls `SetEditorTextFromProgram` and then resets readonly state when requested.
- Enter Edit Mode sets `_editableSession` and flips `SourceTextEditor.IsReadOnly = false`.
- `ResetEditableSessionToReadOnly` clears `_editableSession`, sets `SourceTextEditor.IsReadOnly = true`, and refreshes editor state controls.

Future controller extraction may return a readonly/editable request, but it
must not mutate AvalonEdit directly.

## Caret / Focus / Scroll Rules

Caret and focus stay in `ShellWindow` because they depend on AvalonEdit:

- `RestoreSourceEditorFocusAtCaret` focuses the editor and positions the caret.
- Section and Issue jumps resolve a character index and scroll through AvalonEdit APIs.
- Completion commit and Add Property insert / replace call
  `SetEditorTextFromProgram(..., caretOffset)` and then restore focus.

The controller boundary may describe a requested caret offset, but the actual
focus, selection, visual line, and scroll operations stay in the view.

## Transient UI Rules

Transient editor UI is closed by `ShellWindow`:

- Hover closes on text change, caret movement, scroll, mouse leave, focus loss, file load, and programmatic sync.
- Completion closes on caret movement, scroll, focus loss, file load, Revert, and successful Add Property / Completion commit.

These are UI lifecycle rules and should not be mixed into Core, Infrastructure,
or save services.

## Future SourceEditorController Boundary

A future `Ra2SourceEditorController` may own pure decisions such as:

- Creating a `Ra2SourceEditorSyncPlan` for LoadFile, Revert, Completion commit,
  Add Property insert / replace, or ExternalReload.
- Normalizing null text to empty text.
- Validating that a sync request does not ask for readonly and editable at the same time.
- Returning requested caret offsets as data.
- Naming which transient UI should be closed as a data-only result.

v0.4.58 adds this first pure planning layer as `Ra2SourceEditorSyncPlanner`.
It owns caret offset clamping and `Ra2SourceEditorSyncPlan` creation only.

It must not own:

- AvalonEdit `TextEditor`, `TextArea`, `Document`, caret, visual line, focus, or scroll APIs.
- WPF `Dispatcher`, `Popup`, `Window`, `MessageBox`, routed events, or keyboard events.
- `SetEditorTextFromProgram` implementation details.
- `_isSynchronizingEditorText`.
- Save, dirty persistence, ProjectSaveService, IniFileService, or disk IO.

## ShellWindow Retained UI Glue

`ShellWindow` must continue to own:

- AvalonEdit event subscription and event handlers.
- `SetEditorTextFromProgram`.
- `_isSynchronizingEditorText`.
- `SourceTextEditor.Document.Text` writes.
- `SourceTextEditor.IsReadOnly`.
- Caret, focus, visual positioning, popup placement, and scroll.
- Completion and Hover transient UI closure.

## Why No Runtime Extraction In v0.4.57

The Source Editor currently touches many AvalonEdit-specific behaviors:

- Programmatic sync guard.
- TextChanged user edit routing.
- Completion commit and keyboard routing.
- Add Property insert / replace.
- Hover lifecycle.
- Section and Issue jumps.
- Readonly/editable UI state.

Extracting a controller before documenting these boundaries would risk moving UI
lifecycle concerns into a pure controller or subtly changing editor behavior.
This version only records the boundary and adds tests.

## Future Phases

- v0.4.58: Completed pure SourceEditor sync planning helpers.
- v0.4.59: Move additional non-UI request/result shaping out of `ShellWindow`
  only after the planner boundary remains stable.
- Later: Consider editor adapter abstractions only after save/dirty/editing
  semantics are explicitly designed and accepted.
