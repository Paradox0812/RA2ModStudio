# RA2IniEditor IDE Editor Session Boundary Map v0.4.54

## Scope

This slice is an extraction preparation contract. It does not extract a full
`Ra2EditorSessionController` and does not change runtime behavior.

## Current ShellWindow Fields

- `_editableSession`: current in-memory editable session, or null in readonly preview.
- `_editableSessionService`: creates, updates, and reverts editable sessions.
- `_editorStateViewModelFactory`: maps session state to editor state text and button state.
- `_isSynchronizingEditorText`: guards programmatic AvalonEdit text writes from the
  user-input `TextChanged` path.
- `SourceTextEditor`: AvalonEdit UI control that owns `Document.Text`,
  `IsReadOnly`, caret position, focus, and scroll behavior.

## Current ShellWindow Methods And Events

- `EnterEditMode_OnClick`
  - Validates the current snapshot can be edited.
  - Calls `_editableSessionService.StartEditing`.
  - Sets `SourceTextEditor.IsReadOnly = false`.
  - Calls `UpdateEditorStateControls`.
  - Writes an output message.

- `RevertInMemoryChanges_OnClick`
  - Requires an active `_editableSession`.
  - Calls `_editableSessionService.Revert`.
  - Calls `SetEditorTextFromProgram` with original text.
  - Closes completion dropdown.
  - Calls `ResetEditableSessionToReadOnly`.
  - Writes an output message.

- `SourceTextEditor_OnTextChanged`
  - Closes hover.
  - Skips while `_isSynchronizingEditorText` is true.
  - Skips when no editable session exists.
  - Calls `_editableSessionService.UpdateText`.
  - Calls `UpdateEditorStateControls`.

- `SetEditorTextFromProgram`
  - Closes hover.
  - Sets `_isSynchronizingEditorText = true`.
  - Writes AvalonEdit `Document.Text`.
  - Optionally clamps and sets caret offset.
  - Always clears `_isSynchronizingEditorText`.

- `ResetEditableSessionToReadOnly`
  - Clears `_editableSession`.
  - Sets `SourceTextEditor.IsReadOnly = true`.
  - Calls `UpdateEditorStateControls`.

- `UpdateEditorStateControls`
  - Builds `Ra2EditorStateViewModel`.
  - Updates editor state text.
  - Updates save hint text.
  - Updates Enter Edit Mode and Revert button availability.

## Future Ra2EditorSessionController Responsibilities

The controller should decide what editing state should become:

- Enter edit mode state transition.
- Editable session creation.
- User text update and dirty-state transition.
- Revert decision and resulting original text.
- Programmatic text apply result shape.
- Editor operation result messages.
- Whether the operation asks the shell to become readonly or editable.

The controller should not know how AvalonEdit performs the UI work.

## ShellWindow Responsibilities To Keep

ShellWindow must keep UI glue that depends on WPF or AvalonEdit:

- Reading and writing `SourceTextEditor.Document.Text`.
- Setting `SourceTextEditor.IsReadOnly`.
- Reading and setting caret offsets.
- Owning `_isSynchronizingEditorText` while writing text into AvalonEdit.
- Calling `SetEditorTextFromProgram`.
- Closing completion dropdowns and hover popups.
- Opening or closing WPF windows.
- Updating WPF controls and output messages.

## Programmatic Sync Guard Rules

- Programmatic writes must set `_isSynchronizingEditorText = true` before
  changing AvalonEdit `Document.Text`.
- The guard must be cleared in a `finally` block.
- The user-input `TextChanged` path must skip session updates while the guard is
  active.
- Completion commit, Add Property insert/replace, file load, and Revert must all
  route through the guarded programmatic sync path when they update editor text.

## Dirty And Annotation Dirty Boundary

- INI dirty state belongs to the editable session and text-first document state.
- Annotation sidecar edits belong to annotation JSON and must not mark INI text
  dirty.
- Annotation refresh must not clear INI dirty.
- Revert only restores the in-memory INI text to the session original text; it
  must not roll back annotation sidecar changes.

## Completion And Add Property Session Updates

- Completion commit returns an updated editable session and caret offset through
  the completion interaction controller.
- Add Property insert/replace returns an updated editable session and caret
  offset through the field browser controller.
- ShellWindow receives those results, assigns `_editableSession`, then uses the
  guarded programmatic sync path to update AvalonEdit.

## Revert Temporary UI Cleanup

Revert should close temporary UI that can point at stale text:

- Completion dropdown.
- Hover tooltip or popup.
- Future Add Property transient UI if it is made non-modal.

## Why v0.4.54 Does Not Extract The Full Controller

The editor session chain directly affects user text editing, dirty state,
Revert, Completion commit, Add Property insertion, and future save behavior.
Extracting the full controller in one step would make it too easy to alter
runtime semantics. v0.4.54 only defines operation result models and records the
boundary so v0.4.55 can extract the low-risk Enter Edit Mode / Revert decisions
first.

## Guardrails

- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or legacy save dependency.
- No disk write.
- No Completion commit behavior change.
- No Add Property insert/replace behavior change.
- No Annotation Editor behavior change.
- No Core or Infrastructure public API change.
- No full EditorSessionController extraction in this slice.
