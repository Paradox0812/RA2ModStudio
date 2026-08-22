# RA2IniEditor IDE Handoff v0.4.35

## Completion Preview Polish

v0.4.35 keeps the v0.4.34 completion feature as a readonly preview. It improves the preview surface only:

- completion items now carry a source kind;
- field registry key candidates are labeled as `Field Registry`;
- exact current document reference candidates are labeled as `Current Document`;
- unknown fallback reference candidates are labeled as `Current Document - Unclassified`;
- preview items are grouped by completion item kind;
- the preview window displays item count and replacement span metadata;
- empty completion results keep the stable message `No completion items for current caret position.`

## Guardrails

This version does not implement completion commit. The source editor remains readonly and no text replacement or insertion path was added.

Explicitly still out of scope:

- AvalonEdit `CompletionWindow`;
- automatic popup;
- Enter, Tab, double-click, or click-to-commit behavior;
- `Document.Replace`;
- `TextArea.Document.Insert`;
- dirty state;
- save or Save All;
- undo or redo integration;
- cross-file completion;
- full project indexing;
- `ProjectLoader`, `ProjectSaveService`, or `ObjectAggregator` integration.

## Data Shape

The completion language layer now distinguishes item source with `Ra2CompletionItemSourceKind`:

- `FieldRegistry`
- `CurrentDocumentSection`
- `CurrentDocumentUnknownFallback`

The preview ViewModel exposes grouped display data through `Ra2CompletionGroupViewModel` while staying AvalonEdit-free.

## Validation

Expected validation commands:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

