# RA2IniEditor IDE Handoff v0.4.72

## Target

v0.4.72 implements the first automatic completion trigger for the IDE source editor.

## Behavior

- Ctrl+Space remains the explicit manual completion trigger.
- The right-click completion entry remains available as a fallback.
- Automatic completion is scheduled only after user text input.
- Automatic completion uses a 220 ms debounce.
- Automatic completion is allowed only when:
  - an editable in-memory session exists;
  - the source editor is not readonly;
  - the completion dropdown is not already open;
  - keyboard focus is still in the source editor.
- Automatic completion opens silently and does not write an Output message.
- The dropdown clears stale selection before refreshing items, then selects the first completion item so Tab or Enter can commit immediately and the selected row is visually emphasized.
- The inline dropdown displays only the item list; diagnostic count/replacement/status chrome is omitted.
- Completion item fields use fixed visual columns for label, type, source, and annotation alignment.
- The selected completion row uses a lightweight left accent stripe plus row background highlight.
- The source editor disables Tab focus navigation while the completion popup is openable from the editor, so Tab can commit the selected item.
- Completion popup positioning translates the AvalonEdit caret point from `TextView` to the source editor to keep the dropdown below the caret after scrolling.

## Preserved Boundaries

- No save pipeline changes.
- No dirty-state policy changes.
- No completion commit semantics changes.
- No Add Property behavior changes.
- No new dependency.
- No ProjectSaveService, ProjectLoader, ObjectAggregator, or legacy analysis integration.

## Lifecycle Notes

The pending auto-trigger timer is stopped when:

- source text is programmatically synchronized;
- the editor is reset to readonly preview;
- save/revert/project open starts;
- source editor focus leaves the editor;
- source editor scrolls;
- the shell closes.

The timer opens completions through the same existing completion provider and dropdown path used by Ctrl+Space, but with `showOutputMessage: false`.

## Validation

Recommended commands:

```powershell
dotnet test -c Release --filter FullyQualifiedName~Completion
dotnet test -c Release
dotnet build -c Release --no-incremental
```

Manual smoke:

1. Open an INI file.
2. Enter edit mode.
3. Type a partial key such as `Arm`.
4. Confirm the completion dropdown appears after a short pause.
5. Use Ctrl+Space to confirm manual trigger still works.
6. Confirm Save/Revert/readonly preview do not auto-open completions.
