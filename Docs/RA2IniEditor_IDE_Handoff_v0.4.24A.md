# RA2IniEditor IDE Handoff v0.4.24A

## 1. Version Goal

v0.4.24A implements the file-layer apply writer for local field registry imports.

This version writes only field registry active pack files through an infrastructure service. It does not add UI buttons, does not reload providers, does not touch INI save/dirty/edit behavior, and does not implement rollback UI.

## 2. New Writer Layer

New types live under:

```text
RA2IniEditor.Infrastructure/FieldRegistry/Apply/IO/
```

Main contracts:

- `FieldRegistryApplyWriteRequest`
- `FieldRegistryApplyWriteResult`
- `FieldRegistryApplyBackupManifest`
- `IFieldRegistryApplyPathResolver`
- `FieldRegistryApplyPathResolver`
- `IFieldRegistryApplyWriter`
- `FieldRegistryApplyWriter`

All new types are internal.

## 3. Target Paths

Project target:

```text
<ProjectRoot>/.ra2inieditor/field-registry/active/user-import.fields.json
```

Project backup:

```text
<ProjectRoot>/.ra2inieditor/field-registry/backups/yyyyMMdd-HHmmss/
```

Global target:

```text
<GlobalFieldRegistryRootPath>/active/user-import.fields.json
```

Global backup:

```text
<GlobalFieldRegistryRootPath>/backups/yyyyMMdd-HHmmss/
```

The writer does not read real AppData by itself. The global root is always supplied by the caller.

## 4. Write Rules

The writer accepts a `FieldRegistryApplyPlan` from v0.4.23B.

Validation:

- request cannot be null
- plan must be `CanApplyInFuture`
- plan `ErrorCount` must be `0`
- plan `RejectCount` must be `0`
- Project target requires `ProjectRootPath`
- global root path must be supplied
- target pack file name must not contain path separators

Operations:

- `Add`: appends a new `FieldRegistryFieldDto`
- `Update`: updates matching `key + appliesTo` entry
- update miss: falls back to Add and records a warning
- `Skip`: does not write
- `Reject`: blocks the whole write through plan validation

If the plan contains only skips, no target file or backup directory is created.

## 5. JSON Schema Compatibility

The writer uses the existing DTO schema:

- `FieldRegistryPackDto`
- `FieldRegistryFieldDto`

New pack defaults:

```json
{
  "name": "User Import",
  "kind": "User",
  "version": "local-user-import",
  "fields": []
}
```

Written packs can be loaded by `LocalFieldRegistryLoader`.

## 6. Backup Manifest

Before writing a target pack, the writer creates a unique backup directory.

If the target file exists:

- copy target pack into backup directory
- set `targetFileExisted = true`
- record backup file path

If the target file does not exist:

- do not copy a backup file
- set `targetFileExisted = false`
- backup file path is null

The manifest is written as:

```text
manifest.json
```

Manifest fields include:

- schema version
- target scope
- target file path
- backup file path
- target file existed
- timestamp UTC
- add count
- update count
- skip count
- mode

Timestamp collisions use suffixes such as `-001`.

## 7. Atomic Write Strategy

The writer uses the existing `AtomicTextFileWriter`.

Both target pack and manifest are written as UTF-8, indented JSON. The writer does not silently swallow manifest write failures.

## 8. Guardrails

This version still does not implement:

- Field Import Preview Apply button
- Rollback button
- GUI confirmation dialog
- provider reload
- highlighter refresh
- GitHub fetch
- Completion
- INI save
- dirty state
- INI editing
- field registry editor UI

Boundary tests allow file write APIs inside the writer layer but still reject network/UI/save/reload entry points.

## 9. Tests

Added coverage:

- Project path resolver
- Global path resolver
- Create new project pack
- Update existing pack
- Add plus update in one write
- Reject plan blocks write
- Project target without project root blocks write
- Target pack file name path traversal rejected
- All skip does not write or create backup
- Backup timestamp collision suffix
- Global target writes under supplied global root
- Writer output can be loaded by `LocalFieldRegistryLoader`
- Apply writer boundary guardrail

## 10. Next Step

Recommended next step:

```text
v0.4.24B Field Import Preview Apply UI Minimal
```

That phase should add user-facing target scope and mode selection, build the plan, show confirmation, call the writer, and explicitly reload the field registry only after a successful user-confirmed write.
