# RA2IniEditor IDE Handoff v0.4.39

## Editable Buffer In-memory Apply Spike

v0.4.39 adds IDE-internal in-memory text change application. It applies `Ra2TextChange` to `Ra2EditableDocumentState.CurrentText`, rebuilds `Ra2IniTextDocument`, and updates the document state through the dirty state service.

This version still does not edit AvalonEdit, does not commit completion from the UI, and does not save files.

## Scope

New IDE-internal editing types:

- `Ra2TextChangeApplyResult`
- `IRa2TextChangeApplier`
- `Ra2TextChangeApplier`
- `Ra2EditableDocumentSession`

## Apply Rules

- `ReadOnlyPreview` rejects any change.
- Span start and length must be non-negative.
- Span start must be within the current text length.
- Span start plus length must be within the current text length.
- `span.Length == 0` represents insertion.
- `span.Length > 0` with non-empty new text represents replacement.
- `span.Length > 0` with empty new text represents deletion.
- No-op changes do not make a clean document dirty.
- Dirty documents remain dirty after no-op changes.
- Successful apply returns a new editable document state and a rebuilt text-first document.

## Guardrails

This version still does not do:

- source editor editable mode;
- completion dropdown commit;
- AvalonEdit document writes;
- `Document.Replace`;
- `TextArea.Document.Insert`;
- `TextEditor.Document.Text = ...`;
- Save or Save All;
- disk writes;
- `ProjectLoader`, `ProjectSaveService`, or `ObjectAggregator` integration;
- undo or redo UI;
- dirty UI;
- Core or Infrastructure public API changes.

## Relationship To Existing Features

`Ra2CompletionCommitPlanner` can now be tested together with `Ra2TextChangeApplier`:

```text
completion result + selected item
  -> Ra2TextChange
  -> apply to in-memory editable buffer
  -> rebuild Ra2IniTextDocument
  -> mark dirty
```

The floating completion dropdown remains preview-only.

## Validation

Expected validation commands:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

