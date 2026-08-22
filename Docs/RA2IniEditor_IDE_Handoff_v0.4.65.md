# RA2IniEditor IDE Handoff v0.4.65

## Version

v0.4.65 Save Current File Minimal

## Completed

- Added text-first file writer for the IDE save boundary.
- Added minimal Save Current File service.
- Save service now requires successful backup orchestration before writing.
- Save writes `Ra2EditorSavePlan.Text` directly to the target INI file.
- Save success marks the editable session clean by making saved text the new original text.
- Save failure keeps the existing dirty session and current editor text.

## Save Order

1. Build dry-run save plan from the editable session.
2. Build backup plan.
3. Execute backup.
4. Write `Ra2EditorSavePlan.Text`.
5. On write success, mark session saved.
6. On write failure, preserve dirty session and retain backup.

## Encoding Policy

- `Utf8` writes UTF-8 without BOM.
- `Utf8Bom` writes UTF-8 with BOM.
- `Utf16Le` writes UTF-16 LE.
- `Utf16Be` writes UTF-16 BE.
- `Unknown` falls back to UTF-8 without BOM.
- `SystemDefault` uses `CodePageName` when available, otherwise falls back to UTF-8 without BOM.

## Newline Policy

The writer does not normalize newline characters. `Ra2EditorSavePlan.Text` is written as-is.

## Boundaries

This version does not implement:

- Save All.
- Ctrl+S or Save button integration.
- ProjectSaveService integration.
- legacy IniFileService save integration.
- rollback or restore from backup.
- external file conflict detection.
- UI save command wiring.
- annotation sidecar save semantics.
- dictionary-first INI serialization.

## Tests

Added coverage for:

- UTF-8 no BOM write.
- UTF-8 BOM write.
- UTF-16 LE write.
- UTF-16 BE write.
- Unknown encoding fallback.
- newline preservation.
- writer failure result.
- backup success plus write success.
- backup failure blocking write.
- write failure preserving dirty state and backup.
- save success updating original text and revert baseline.
- duplicate section/key/comment/blank line preservation.
- guardrail against legacy save chain and dictionary serialization.
