# RA2IniEditor IDE Handoff v0.4.34

## Scope

v0.4.34 adds a manual readonly Completion Preview UI Spike.

This version does not implement a formal completion popup, does not use AvalonEdit `CompletionWindow`, does not commit items, does not edit the source text, and does not connect save, dirty, undo, redo, or project-wide indexing.

## UI Entry

The AvalonEdit source editor context menu now includes:

```text
Show Completion Preview
```

The command builds a current-document completion request from:

- `SourceTextEditor.Document.Text`
- `Ra2DocumentSemanticModelBuilder`
- `Ra2CaretContextService`
- `Ra2CompletionProvider`
- current field registry provider

## Preview Window

The new `Ra2CompletionPreviewWindow` is a non-modal independent window. It displays:

- replacement span start and length;
- item count or empty-result status;
- completion label;
- item kind;
- detail;
- documentation.

The window is display-only. It has no submit button and no double-click commit behavior.

## Guardrails

The completion preview path does not call:

- `Document.Replace`
- `TextArea.Document.Insert`
- AvalonEdit `CompletionWindow`
- save services
- dirty state
- legacy project loading or object aggregation services

## Validation

Expected validation:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

UIA is not required because the feature is a manual preview window and no text commit behavior was added.
