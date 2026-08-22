# RA2IniEditor IDE Handoff v0.4.25B

## Version

v0.4.25B Rollback UI Minimal

## Scope

This version connects the v0.4.25A field registry rollback service to the IDE Field Registry Manager with a minimal, explicit UI flow.

Implemented flow:

1. Field Registry Manager reads recent Project and Global backup manifests.
2. The user selects exactly one manifest.
3. The user clicks Rollback Selected.
4. A MessageBox confirmation shows scope, target file, backup file, target existence, and timestamp.
5. On confirmation, the IDE calls the rollback service.
6. On success, the IDE reloads local field registry providers and refreshes readonly source highlighting.
7. On failure, the IDE shows a rollback failure status and does not reload providers.

## UI

Field Registry Manager now includes a Recent Import Backups area:

- Refresh Backups
- Rollback Selected
- Scope
- Timestamp
- Target File
- Existed
- Add
- Update
- Skip
- Manifest

Project manifests are loaded from:

```text
<ProjectRoot>/.ra2inieditor/field-registry/backups
```

Global manifests are loaded from:

```text
<GlobalFieldRegistryRootPath>/backups
```

The list is sorted newest first by timestamp, then manifest path.

## Guardrails

This version does not implement:

- automatic rollback
- batch rollback
- multiple manifest rollback
- rollback merge preview
- GitHub fetch
- Completion
- field registry editing
- INI save
- dirty state
- editable source text
- TextChanged edit chain
- ProjectSaveService integration
- ObjectAggregator / ProjectLoader integration

## Failure Handling

Malformed or unreadable manifest files are skipped with a warning.

Unreadable backup manifest directories are reported as warnings and do not prevent the Field Registry Manager window from opening.

Rollback failure reports:

```text
Rollback failed: {message}
```

Provider reload and highlighter refresh only happen after a successful rollback result.

## Automation

New UI Automation IDs:

- `FieldRegistryManager.RefreshRollbackManifestsButton`
- `FieldRegistryManager.RollbackSelectedButton`
- `FieldRegistryManager.RollbackManifestsGrid`
- `FieldRegistryManager.RollbackStatusText`

UIA smoke coverage includes the intended project flow:

```text
apply project field import -> refresh backups -> select manifest -> rollback -> verify active pack removed/restored
```

UIA smoke remains opt-in through:

```powershell
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test RA2IniEditor.UiAutomationTests -c Release
```
