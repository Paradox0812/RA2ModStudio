# RA2IniEditor IDE Handoff v0.4.25A

## 1. Version Scope

Target version: v0.4.25A Rollback Service + Backup Manifest Reader.

This version only implements the service layer for reading field registry apply backup manifests and rolling back one manifest to one target file. It does not add rollback UI, user confirmation windows, provider reload, GitHub fetch, completion, INI save, dirty tracking, or editor mutation flows.

## 2. Manifest Reader

`FieldRegistryApplyBackupManifestReader` reads `manifest.json` files produced by the field registry apply writer.

Responsibilities:

- Read and deserialize `FieldRegistryApplyBackupManifest`.
- Validate required manifest fields.
- Enumerate `manifest.json` files under a backups root.
- Return manifest paths sorted by path descending so newer timestamp directories are first.

The reader does not inspect active packs, merge field definitions, or infer rollback intent from current file content.

## 3. Rollback Service

`FieldRegistryRollbackService` accepts a `FieldRegistryRollbackRequest` containing:

- Manifest file path.
- Optional project root path.
- Global field registry root path.

Rollback is based only on manifest state:

- `TargetFileExisted=true`: restore the backup file over the target file.
- `TargetFileExisted=false`: delete the target file created by apply.
- Missing created target: return `NoOp`.

The service returns `FieldRegistryRollbackResult` with operation kind, manifest path, target path, backup path, and a short message. Failures throw exceptions rather than returning partial success.

## 4. Safety Validation

Rollback validates paths before file mutation:

- Manifest path must exist and be a full JSON path.
- Manifest path must be under the allowed Project or Global backups root.
- Target path must be under the allowed active root.
- Target file name must be `user-import.fields.json`.
- Existing-target rollback requires a backup file.
- Backup file must exist.
- Backup file must be under the backups root and in the same backup batch directory as the manifest.
- Created-target rollback rejects non-empty backup paths.

Rollback does not delete active directories, backup directories, manifests, or backup files.

## 5. File Restore Strategy

Restore uses a same-directory temp file:

1. Copy backup file to a target-directory temp file.
2. Prefer `File.Replace` when the target exists.
3. Fall back to overwrite `File.Move` for IO/platform replacement failures.
4. Move temp into place when the target does not exist.
5. Clean temp best-effort without hiding the original failure.

## 6. Tests

Added tests cover:

- Manifest read.
- Manifest enumeration sorted newest path first.
- Missing backup root enumeration.
- Malformed manifest rejection.
- Missing required manifest field rejection.
- Restore existing target from backup.
- Delete created target.
- Created target already missing `NoOp`.
- Missing backup blocks restore and leaves target untouched.
- Manifest outside backups root rejected.
- Target outside active root rejected.
- Unsupported target file name rejected.
- Backup path present for created target rejected.
- Backup path outside manifest batch rejected.
- Writer-created target manifest rolls back by deleting target.
- Writer-existing target manifest rolls back to old content.
- Guardrail scan for UI, network, completion, save service, object aggregation, and project loader references.

## 7. Guardrails

This version does not implement:

- Rollback button.
- Rollback UI.
- GUI confirmation.
- Provider reload.
- Highlighter refresh.
- GitHub fetch.
- Completion.
- Field registry editor.
- INI save.
- Dirty tracking.
- INI editing.
- Legacy Analysis, `ObjectAggregator`, `ProjectLoader`, or `ProjectSaveService` integration.

## 8. Verification

Validated commands:

```text
dotnet test -c Release
dotnet build -c Release --no-incremental
```

Observed results:

- Normal tests passed: 390/390.
- Release build passed: 0 errors.
- Warning count remained at 26 existing warnings.

## 9. Next Step

v0.4.25B can consider a minimal Rollback UI that lists recent manifests, asks the user for confirmation, calls the rollback service, then reloads local registry providers and refreshes readonly highlighting after success.
