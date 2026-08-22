# RA2IniEditor IDE Handoff v0.4.24C

## Scope

v0.4.24C is Apply UI smoke and hardening. It does not add Rollback, GitHub fetch, Completion, INI save, dirty tracking, editable source text, or field registry editing.

## Apply Status Hardening

The Field Import Preview now keeps explicit readonly result fields after a successful apply:

- `LastApplyTargetFilePath`
- `LastApplyBackupManifestPath`
- `LastApplySummaryText`

Successful apply status includes:

- `Apply completed.`
- Target `user-import.fields.json`
- Backup manifest path, or `None` when no manifest is produced
- Added / Updated / Skipped counts

Writer failures display `Apply failed: {message}` and do not call the reload callback.

## Blocking Rules

Apply remains disabled or blocked when:

- Project target is selected without an open project.
- The apply plan has no add or update operations.
- The apply plan has errors.
- The apply plan has rejected rows.

Apply success does not clear raw text, preview diff, field drafts, or apply plan rows. Users can inspect the result until they click `Clear`.

## Reload And Highlighting

Field Registry Manager reload and Apply success reuse the same ShellWindow refresh helper:

```text
ReloadLocalFieldRegistryForReadonlyHighlighting
```

That helper reloads `FieldRegistryRuntimeService`, refreshes manager status, replaces the old known-field highlighter transformer, and redraws AvalonEdit.

## Smoke Notes

Automated tests cover the write-success and write-failure path with fake writers, including target path, manifest path, reload callback count, all-skip blocking, project-without-root blocking, and reject/error blocking.

Manual WPF smoke should still be run on a desktop session before accepting rollback work:

1. Open IDE Shell.
2. Open a test project folder.
3. Open Field Registry Manager.
4. Open Field Import Preview.
5. Insert sample, parse, select Project target, build plan, apply.
6. Confirm `user-import.fields.json` and backup `manifest.json`.
7. Confirm Field Registry Manager status and readonly highlighter refresh.

Global target smoke can be skipped if writing to the real user AppData field registry is not desired.

## Guardrails

Still not implemented:

- Rollback UI
- GitHub fetch
- Completion
- INI save / dirty / edit chain
- Field registry editor
- Multi target pack selection
