# RA2IniEditor IDE Handoff v0.4.25C

## Version

v0.4.25C Rollback Hardening + UX Polish

## Scope

This version hardens the Field Registry Manager rollback UI added in v0.4.25B. It does not change field import apply behavior, GitHub fetch, Completion, INI save, dirty state, or editable source text.

## Rollback Result Summary

Successful rollback now shows a detailed summary in the Field Registry Manager rollback status area:

```text
Rollback completed.
Operation: RestoreBackup / DeleteCreatedTarget / NoOp
Target: <target file path>
Backup: <backup file path or None>
Manifest: <manifest path>
<service message>
```

After success, the IDE still reloads local field registry providers and refreshes readonly source highlighting. After failure, provider reload and highlighter refresh are not run.

## Manifest Status

Rollback manifest rows now expose:

- `Status`
- `StatusMessage`
- `CanRollback`

Supported statuses include:

- `Ready`
- `Malformed`
- `MissingBackup`
- `UnsupportedTarget`
- `InvalidPath`
- `MissingTarget`
- `UnknownError`

Only `Ready` rows can be rolled back. Malformed manifests are shown as disabled rows instead of silently disappearing. Missing backup files and unsupported target pack names also disable rollback.

## Path Actions

The Recent Import Backups area now includes:

- Open Target
- Open Manifest
- Open Backup

These actions only open existing directories through shell execution. They do not create, modify, delete, or save files. Open failures are reported in the rollback status text and the IDE remains open.

## Automation

New or confirmed AutomationIds:

- `FieldRegistryManager.OpenRollbackTargetFolderButton`
- `FieldRegistryManager.OpenRollbackManifestFolderButton`
- `FieldRegistryManager.OpenRollbackBackupFolderButton`
- `FieldRegistryManager.RollbackStatusText`
- `FieldRegistryManager.RollbackManifestsGrid`
- `FieldRegistryManager.RefreshRollbackManifestsButton`
- `FieldRegistryManager.RollbackSelectedButton`

UIA smoke now verifies that apply -> rollback exposes operation, target, and manifest summary text.

## Guardrails

This version does not implement:

- batch rollback
- automatic rollback
- rollback diff preview
- multi-target pack selection
- GitHub fetch
- Completion
- field registry editor
- INI save / dirty / edit chain
- ProjectSaveService integration
- ObjectAggregator / ProjectLoader integration
