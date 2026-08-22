# RA2IniEditor IDE Handoff v0.4.71 Always Editable Source Editor

## Target

This slice removes the visible "Enter Edit Mode" step from the IDE source editor.

## Behavior

- The source editor still starts readonly when no file is selected.
- Read failures and deferred large-file previews stay readonly.
- A successfully loaded INI file automatically creates a clean editable in-memory session.
- The visible Enter Edit Mode button is collapsed.
- User text input updates the editable session and marks it dirty through the existing editor-session path.
- Save Current File and Ctrl+S keep the existing save service semantics.
- A successful save updates the editable session baseline and keeps the editor editable.
- Revert restores the current baseline, clears dirty state, and keeps the editor editable.
- Completion commit and Add Property now require only an editable session, not a user-clicked edit-mode step.
- Dirty navigation guard blocks file switching and Open Folder when the current session has unsaved in-memory changes.

## Preserved Boundaries

- No ProjectSaveService changes.
- No IniFileService changes.
- No Save All implementation.
- No Undo/Redo implementation.
- No Completion provider semantic changes.
- No Add Property search semantic changes.
- No Project Explorer relocation.
- No UIA changes in this slice.

## Manual Smoke

1. Open a project folder.
2. Select a normal INI file.
3. Confirm the editor is immediately editable.
4. Type a small change and confirm the status becomes dirty.
5. Try selecting another file and confirm navigation is blocked with an Output message.
6. Save with Ctrl+S and confirm the editor remains editable.
7. Revert and confirm it returns to the saved baseline, not the original pre-save text.
8. Confirm Completion and Add Property work without clicking an edit-mode button.

## Validation

Recommended commands:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```
