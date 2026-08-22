# RA2IniEditor IDE Handoff v0.4.47.2

## Scope

v0.4.47.2 only polishes the Add Property / Field Browser UI:

- Focus the search box when the Add Property window opens.
- Keep Esc behavior deterministic: clear non-empty search text first, close the window when search is already empty.
- Keep Enter behavior deterministic: confirm only when the current document state allows adding the selected raw key.
- Improve field grid column widths and keep the note column readable.
- Improve empty-result status text for generic search, missing annotations, and current-type-specific scope.

## Guardrails

- No INI disk write.
- No Save / Save All integration.
- No ProjectSaveService or legacy save call.
- No Core or Infrastructure public API change.
- No Completion commit behavior change.
- No Field Registry or annotation business semantic change.

## Implementation Notes

- `Ra2AddPropertyWindow` owns WPF focus and key handling.
- `Ra2AddPropertyViewModel` owns keyboard decisions through `ClearSearchForEscape` and `TryConfirmFromKeyboard`.
- Insert preview and `OptionText` still use the raw field key.
- Recent fields are visually marked with `★` in the existing recent column.

## Verification

- `dotnet test -c Release`: 768 passed.
- `dotnet build -c Release --no-incremental`: 0 errors, 26 existing warnings.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open an INI folder and invoke Add Property from the source editor.
3. Confirm the search box receives focus.
4. Type a query, press Esc, and confirm the query clears.
5. Press Esc again and confirm the window closes.
6. In read-only preview, press Enter on a selected field and confirm no insert happens.
7. In edit mode, select a field and press Enter; confirm the raw key is inserted in memory only.
