# RA2IniEditor IDE Handoff v0.4.37

## Editable Buffer / Dirty / Save / Undo Contract Design

v0.4.37 defines the first internal contract layer for future editable source workflows. It does not make the source editor editable and does not implement completion commit.

## Scope

New IDE-internal contract types live under `RA2IniEditor.IDE/Editing`:

- `Ra2EditorDocumentState`
- `Ra2EditableDocumentState`
- `Ra2TextChange`
- `IRa2CompletionCommitPlanner`
- `Ra2CompletionCommitPlanner`
- `IRa2DirtyStateService`
- `Ra2DirtyStateService`
- `IRa2EditorSaveBoundary`
- `Ra2EditorSaveBoundary`

## Contract Rules

- `Ra2TextChange` is the future common representation for text edits.
- `Ra2CompletionCommitPlanner` only creates a `Ra2TextChange` from `ReplacementSpan` and `InsertText`.
- The planner does not apply the change to AvalonEdit or any document.
- The dirty state service is a state machine only.
- `ReadOnlyPreview` does not become dirty from text changes.
- `EditableClean + textChanged` becomes `EditableDirty`.
- `EditableDirty + saved` becomes `EditableClean`.
- The save boundary only answers whether a future editable document may be saved.

## Guardrails

This version still does not do:

- source editor editable mode;
- completion item commit;
- text insertion;
- `Document.Replace`;
- `TextArea.Document.Insert`;
- AvalonEdit `CompletionWindow`;
- real Save or Save All;
- dirty UI;
- undo or redo integration;
- `ProjectLoader`, `ProjectSaveService`, or `ObjectAggregator` integration;
- Core or Infrastructure public API changes.

## Future Save Direction

Future IDE save work must be text-first, not dictionary-first. The editable pipeline must preserve:

- comments;
- blank lines;
- section order;
- duplicate sections;
- duplicate keys;
- encoding;
- newline style.

Legacy save integration requires a dedicated migration contract before implementation.

## Validation

Expected validation commands:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

