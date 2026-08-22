# RA2IniEditor IDE Handoff v0.4.23B

## 1. Version Goal

v0.4.23B adds the Apply / Rollback Design Contract foundation for local field registry import.

This version only provides an internal, pure in-memory apply plan. It does not write active field packs, create backups, perform rollback, add UI buttons, or connect any save/dirty/edit lifecycle.

## 2. Scope

Implemented:

- Apply target scope contract: `Project` / `Global`
- Apply mode contract: `AppendOnly` / `AppendOrUpdate` / `SkipExisting`
- Apply operation contract: `Add` / `Update` / `Skip` / `Reject`
- Apply plan issue severity: `Info` / `Warning` / `Error`
- Apply plan request, item, issue, and result DTOs
- Pure in-memory `FieldRegistryApplyPlanBuilder`
- Guardrail tests preventing write/network/UI/save entry points in the apply layer

Not implemented:

- Real active pack writes
- Backup creation
- Rollback
- Apply or Rollback buttons
- GitHub fetch
- Completion
- INI save, dirty, or edit behavior
- Field registry editor

## 3. Apply Plan Contract

The apply planner consumes:

- `FieldRegistryHarvestPreviewDraft`
- `FieldRegistryHarvestDiffResult`
- `FieldRegistryApplyTargetScope`
- `FieldRegistryApplyMode`

It returns:

- one `FieldRegistryApplyPlanItem` per diff row
- plan issues for informational, warning, and error states
- operation counts
- `CanApplyInFuture`, based on whether the plan contains errors

All types are internal and live under:

```text
RA2IniEditor.Infrastructure/FieldRegistry/Apply/
```

## 4. Planning Rules

`Added`:

- `AppendOnly` -> `Add`
- `AppendOrUpdate` -> `Add`
- `SkipExisting` -> `Add`

`Same`:

- all modes -> `Skip` with Info issue

`Changed + AppendOnly`:

- `Skip` with Warning issue

`Changed + SkipExisting`:

- `Skip` with Info issue

`Changed + AppendOrUpdate`:

- `ExistingScope = None` -> `Add`
- `ExistingScope = BuiltIn` -> `Add` with Warning
- `ExistingScope = Global`, target `Global` -> `Update`
- `ExistingScope = Global`, target `Project` -> `Add` with Warning
- `ExistingScope = Project`, target `Project` -> `Update`
- `ExistingScope = Project`, target `Global` -> `Reject` with Error
- `ExistingScope = Unknown` -> `Reject` with Error

`Invalid` / `Conflict`:

- `Reject` with Error

If `FieldRegistryHarvestPreviewDraft.ErrorCount > 0`, all diff rows are rejected and the plan is not future-applyable.

## 5. BuiltIn Override Semantics

BuiltIn definitions are never modified directly.

When a changed BuiltIn field is planned for Project or Global target, the plan emits an `Add` operation with a warning. The warning makes clear that a future apply would create an override in the selected target scope.

## 6. Project / Global Priority Guard

Effective priority remains:

```text
Project > Global > BuiltIn
```

Therefore, when an existing Project definition differs and the selected target is Global, the planner returns `Reject` with an Error. A Global write would not change the effective result while Project still has higher priority.

## 7. Future Backup Contract

No backup is implemented in v0.4.23B.

Future writer phases should use:

Project backup root:

```text
<ProjectRoot>/.ra2inieditor/field-registry/backups/yyyyMMdd-HHmmss/
```

Global backup root:

```text
%AppData%/RA2IniEditor/FieldRegistry/backups/yyyyMMdd-HHmmss/
```

Backup manifest should record:

- target scope
- target active pack path
- backup file path
- timestamp
- apply mode
- operation count
- whether the original target file existed

## 8. Future Target Active Pack

Recommended future default target file:

Project:

```text
<ProjectRoot>/.ra2inieditor/field-registry/active/user-import.fields.json
```

Global:

```text
%AppData%/RA2IniEditor/FieldRegistry/active/user-import.fields.json
```

Future implementation should merge into or create this file only after explicit user confirmation and backup.

## 9. Future Confirmation Flow

Before any future real apply, UI must show:

- Target scope
- Target file
- Apply mode
- Add count
- Update count
- Skip count
- Reject count
- Warnings
- Errors
- Backup location

Apply must be blocked when:

- plan `ErrorCount > 0`
- target is Project and no project is open

## 10. Tests

v0.4.23B adds coverage for:

- Added -> Add
- Same -> Skip + Info
- Changed BuiltIn -> Add override warning
- Changed Global -> Project Add warning
- Changed Global -> Global Update
- Changed Project -> Project Update
- Changed Project -> Global Reject + Error
- AppendOnly changed -> Skip + Warning
- SkipExisting changed -> Skip + Info
- Invalid -> Reject + Error
- Conflict -> Reject + Error
- PreviewDraft errors block future apply
- Apply layer guardrails for forbidden write/network/UI/save APIs

## 11. Next Step

Recommended next step:

```text
v0.4.24A Apply Writer Contract + Backup Manifest Implementation
```

That phase should design and test the writer/backup layer before any UI button is connected.
