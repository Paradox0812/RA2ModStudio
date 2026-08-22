# RA2IniEditor IDE Handoff v0.4.44

## Scope

v0.4.44 adds a Field Library Browser / Add Property preview path and a user annotation overlay for IDE display surfaces.

This version is still text-first and in-memory only. It does not save INI files, does not write active field packs, and does not insert Chinese labels or annotation text into source content.

## Field Annotation Overlay

- Annotation sidecars are represented by `Ra2FieldAnnotationPack` and `Ra2FieldAnnotationEntry`.
- `Ra2FieldAnnotationProvider` resolves exact section-kind annotations first, then `*` wildcard annotations.
- `Ra2FieldAnnotationJsonStore` can load and save annotation sidecar JSON, reporting malformed JSON and duplicate entries as controlled warnings.
- `Ra2FieldDisplayResolver` merges field registry definitions with annotations.
- Annotation display names, aliases, and notes are display-only metadata.
- Raw field keys remain the source of truth for text insertion and completion commit.

## Add Property Preview

The Add Property window is available from the Source Editor context menu.

- It lists fields for the current section kind.
- Search matches raw key, display name, aliases, note, and description.
- Selecting a field writes the raw key to the option box.
- In read-only preview, insertion is disabled.
- In edit preview, insertion creates an in-memory text change only.
- Inserted text is `Key=Value` or `Key=`.
- Duplicate keys in the current section produce a warning, but are not blocked.

## Hover And Completion Display

- Hover for annotated keys uses display name and note when available.
- Hover detail still includes the raw key.
- Completion dropdown display can show annotation display names, aliases, and notes.
- Completion `Label`, `InsertText`, and `ReplacementSpan` remain raw-key based.
- Smart key completion still commits `RawKey=` and never commits localized labels.

## Guardrails

- No `ProjectSaveService`.
- No `IniFileService`.
- No INI disk write.
- No save / dirty / backup / rollback integration.
- No Core or Infrastructure public API changes.
- No legacy field database integration.
- No ObjectAggregator or full-project index.
- No large property-grid main editor.

## Tests

Coverage added for:

- Annotation provider exact and wildcard lookup.
- Annotation JSON load/save failure and duplicate warnings.
- Display resolver annotation precedence and raw fallback.
- Field discovery search over key, display name, aliases, note, and description.
- Add Property ViewModel filtering, selection, and read-only gating.
- Add Property insert planner line placement, newline preservation, raw key insertion, and duplicate warning.
- Shell boundary: Add Property uses in-memory apply and does not touch save services.
- Hover display annotations.
- Completion display enhancement with raw insert text preservation.

## Manual Smoke Checklist

1. Open the IDE shell and load an INI file.
2. Open the Source Editor context menu and choose Add Property.
3. Confirm the field list appears and search can find fields by key or annotation text.
4. In read-only preview, confirm Add Selected is disabled.
5. Enter edit mode.
6. Open Add Property, select a field, enter a value, and add it.
7. Confirm the source buffer changes in memory only and inserted text uses the raw key.
8. Open completion and confirm display metadata can show annotations while commit still inserts raw keys.
9. Confirm disk file content is unchanged unless a future save feature is explicitly implemented.
