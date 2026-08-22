# RA2IniEditor IDE Handoff v0.4.44.1

## Scope

v0.4.44.1 polishes Add Property and field annotation visibility. It keeps the v0.4.44 boundaries: Add Property is still an in-memory edit preview feature, and field annotation metadata remains display-only.

## UX Changes

- Add Property shows annotation sidecar status.
- Add Property shows annotation warnings without blocking the field list.
- Field rows show lightweight Recent and Annotated markers.
- Selecting a field shows a value hint based on field type.
- The window shows an insert preview such as `Preview: Strength=400`.
- The preview always uses the raw key, never the localized display name or aliases.
- Duplicate key detection warns when the current section may already contain the key.
- Duplicate key warnings do not block insertion.
- Read-only preview shows `Enter Edit Mode to insert fields.` and keeps Add Selected disabled.

## Recent Fields

Recent fields are tracked in memory only.

- They are lost when the IDE process closes.
- They are grouped by `Ra2SectionKind`.
- Reusing a field moves it to the front.
- The current cap is 10 items per section-kind query.
- No configuration file is written.

## Insert Behavior

Insert behavior is unchanged from v0.4.44.

- Edit preview inserts through `Ra2TextChangeApplier`.
- The Source Editor is synchronized from the updated in-memory session.
- The caret moves to the inserted line end.
- Disk INI files are not written.
- Save / dirty / backup / rollback pipelines are not connected.

## Guardrails

- No `ProjectSaveService`.
- No `IniFileService`.
- No legacy save facade.
- No Core or Infrastructure public API changes.
- No annotation editor.
- No Save Field Annotations UI.
- No large Property Grid.
- No ObjectAggregator or full-project index.

## Tests

Added or extended coverage for:

- Annotation status text for loaded, missing, and failed sidecars.
- Recent field dedupe, ordering, section-kind filtering, and max count.
- Value hints for integer, boolean, enum, reference, list, and unknown fields.
- Duplicate key detection in current section only.
- Add Property ViewModel preview, read-only hint, annotation status, recent sorting, and duplicate warning.
- Add Property UI boundary checks for new AutomationIds and no save-service integration.

## Manual Smoke Checklist

1. Start the IDE shell and open a folder.
2. Open an INI file.
3. Right-click Source Editor and choose Add Property.
4. Confirm annotation status is visible.
5. Select `Strength` and confirm the value hint is visible.
6. Enter `400` and confirm preview is `Preview: Strength=400`.
7. In read-only preview, confirm Add Selected is disabled.
8. Enter edit preview and insert `Strength=400`.
9. Reopen Add Property and confirm `Strength` is marked as recent.
10. If the current section already contains `Strength`, confirm a duplicate warning is shown.
11. Confirm the disk INI file is unchanged.
