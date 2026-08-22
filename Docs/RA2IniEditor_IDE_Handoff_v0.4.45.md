# RA2IniEditor IDE Handoff v0.4.45

## Scope

v0.4.45 upgrades Add Property duplicate-key handling from a warning-only flow into explicit user actions:

- Jump Existing
- Replace Existing
- Insert Duplicate
- Cancel

The feature still operates only on the current document and current section. It remains an in-memory edit preview feature.

## Duplicate Detection

`Ra2DuplicateKeyDetector.FindInCurrentSection` returns the first matching key-value line in the caret's current section range.

Behavior:

- Matching is case-insensitive.
- Comments and raw lines are ignored.
- Other sections are ignored.
- Duplicate section names are scoped by the caret's actual section bounds, not by section name alone.
- Empty values return a zero-length value span after `=`.

## Actions

### Jump Existing

- Moves the AvalonEdit caret to the existing key line.
- Does not change text.
- Does not change dirty state.
- Is allowed in read-only preview.

### Replace Existing

- Replaces the existing value span only.
- Preserves the key text and inline comments.
- Uses `Ra2TextChangeApplier` against the in-memory edit session.
- Does not write disk.

Example:

```ini
Strength=400 ; hp
```

Replacing with `500` produces:

```ini
Strength=500 ; hp
```

### Insert Duplicate

- Keeps the existing Add Property insertion behavior.
- Inserts a new `Key=Value` line.
- Allows intentional duplicate keys.

### Cancel

- Closes the duplicate action flow without changing text.

## UI Notes

When a duplicate is detected, Add Property shows:

- key, line number, and existing value
- duplicate action selector
- Run Action button

Read-only preview allows Jump Existing only. Replace Existing and Insert Duplicate require edit preview.

## Guardrails

- No `ProjectSaveService`.
- No `IniFileService`.
- No Save / Save All.
- No disk INI write.
- No backup or rollback.
- No Core or Infrastructure public API changes.
- No ObjectAggregator or full-project index.
- No Property Grid main editor.
- Localized labels are never inserted into INI.

## Tests

Added or extended coverage for:

- Duplicate match line number, existing value, line span, and value span.
- Current section bounds with duplicate section names.
- Comments/raw lines ignored.
- Empty value matches.
- Duplicate action ViewModel availability in read-only and edit preview.
- Replace Existing value-only text change with inline comment preservation.
- Insert Duplicate still inserts a second key line.
- Shell/UI boundary checks for duplicate action routing and no save-service integration.

## Manual Smoke Checklist

1. Open the IDE shell and load an INI file with:

```ini
[HTNK]
Name=HTNK
Strength=400
Armor=heavy
```

2. In read-only preview, open Add Property and select `Strength`.
3. Confirm duplicate action details show line number and current value.
4. Choose Jump Existing and confirm the caret moves to `Strength=400`.
5. Enter edit preview.
6. Open Add Property, select `Strength`, enter `500`, choose Replace Existing, and run action.
7. Confirm the in-memory text becomes `Strength=500`.
8. Revert.
9. Enter edit preview again, select `Strength`, enter `500`, choose Insert Duplicate, and run action.
10. Confirm both `Strength=400` and `Strength=500` are present in memory.
11. Confirm the disk INI file is unchanged.
