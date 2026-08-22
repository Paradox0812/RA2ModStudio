# RA2IniEditor IDE Handoff v0.4.38

## Text-first INI Document Model Foundation

v0.4.38 introduces an IDE-internal text-first INI document model. It is a readonly structural model for future editing and safe-save work.

This version does not make the source editor editable and does not save files.

## Scope

New IDE-internal text model types live under `RA2IniEditor.IDE/TextModel`:

- `Ra2IniDocumentLineKind`
- `Ra2IniNewLineKind`
- `Ra2IniDocumentLine`
- `Ra2IniTextDocument`
- `IRa2IniTextDocumentParser`
- `Ra2IniTextDocumentParser`

## Model Rules

- The model is line-first, not dictionary-first.
- The parser preserves the original document text.
- Each line preserves its original text without the line break.
- Each line preserves its own line break.
- Line spans are based on the full original document text.
- Section headers, key-value lines, comment lines, blank lines, and raw lines are classified.
- Section name, key, value, and inline comment spans are retained when available.
- Duplicate sections and duplicate keys are not merged, overwritten, or reordered.
- Newline style is detected as LF, CRLF, CR, Mixed, or Unknown.

## Guardrails

This version still does not do:

- source editor editable mode;
- completion commit;
- text insertion;
- `Document.Replace`;
- `TextArea.Document.Insert`;
- Save or Save All;
- disk writes;
- `ProjectLoader`, `ProjectSaveService`, or `ObjectAggregator` integration;
- full project indexing;
- undo or redo UI;
- dirty UI;
- Core or Infrastructure public API changes.

## Relationship To Existing Models

`Ra2IniTextDocument` is a text-first structural model for future editing and safe-save work.

`Ra2DocumentSemanticModel` remains the language semantic model used by hover, definition, references, and completion. v0.4.38 does not replace or refactor it.

Future flow should be:

```text
Ra2TextChange
  -> apply to text buffer
  -> rebuild Ra2IniTextDocument
  -> mark dirty
  -> later safe save
```

## Validation

Expected validation commands:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

