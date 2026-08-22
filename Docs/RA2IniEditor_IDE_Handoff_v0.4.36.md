# RA2IniEditor IDE Handoff v0.4.36

## Floating Completion Dropdown Preview

v0.4.36 changes the primary completion preview interaction from a standalone preview window to a floating readonly dropdown near the source editor caret.

The feature remains preview-only:

- `Ctrl+Space` opens the floating dropdown manually;
- the source editor context menu opens the same dropdown;
- candidates reuse the v0.4.35 source and fallback labels;
- the dropdown displays label, kind, source, detail, item count, and replacement span;
- empty results keep the stable message `No completion items for current caret position.`;
- the first item is selected by default;
- Up and Down move selection without crossing item bounds;
- Esc closes the dropdown;
- Enter and Tab close the dropdown without committing text;
- editor focus loss, caret movement, scrolling, project open, and file switch close the dropdown.

## Guardrails

This version still does not implement completion commit.

Explicitly still out of scope:

- AvalonEdit `CompletionWindow`;
- automatic popup while typing;
- text insertion;
- replacement span application;
- Enter, Tab, double-click, or mouse click commit;
- dirty state;
- save or Save All;
- undo or redo integration;
- cross-file completion;
- full project indexing;
- `ProjectLoader`, `ProjectSaveService`, or `ObjectAggregator` integration.

## Implementation Notes

The dropdown uses a regular WPF `Popup` hosted by `ShellWindow`. It does not use AvalonEdit completion infrastructure.

New readonly display types:

- `Ra2CompletionDropdownViewModel`
- `Ra2CompletionDropdownItemViewModel`
- `Ra2CompletionDropdownPositioning`
- `Ra2CompletionDropdownView`

The existing `Ra2CompletionProvider` remains the completion data source.

## Validation

Expected validation commands:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

UIA is not required for this version.

