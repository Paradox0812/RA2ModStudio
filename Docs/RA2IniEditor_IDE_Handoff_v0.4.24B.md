# RA2IniEditor IDE Handoff v0.4.24B

## Scope

v0.4.24B adds the minimal Field Import Preview apply UI. The feature remains opt-in: users must parse and preview raw field docs, build an apply plan, confirm the apply dialog, and only then write `user-import.fields.json`.

## Flow

1. Field Registry Manager opens Field Import Preview.
2. Preview parses raw text and builds the existing diff against provenance.
3. User selects target scope and apply mode.
4. `Build Apply Plan` calls `FieldRegistryApplyPlanBuilder`.
5. `Apply` shows a confirmation dialog with target file, mode, counts, backup note, and override warnings.
6. After confirmation, `FieldRegistryApplyWriter` writes the active pack and backup manifest.
7. The IDE reloads local field registry state and redraws readonly highlighting.

## Modified Areas

- `FieldRegistryHarvestPreviewViewModel`
  - Stores current preview draft and diff.
  - Builds and exposes apply plan rows.
  - Calls the writer only from `ApplyConfirmed`.
  - Catches apply failures and leaves UI state intact.

- `FieldRegistryHarvestPreviewWindow`
  - Adds target scope and apply mode selectors.
  - Adds `Build Apply Plan` and `Apply` buttons.
  - Adds an `Apply Plan` tab.
  - Uses `MessageBox` for explicit confirmation.

- `ShellWindow`
  - Passes project/global registry roots and a reload callback to the preview window.
  - Reuses the existing readonly highlighter reload path after successful apply.

- `FieldRegistryRuntimeService`
  - Exposes the global field registry root path so the writer can resolve `active/user-import.fields.json`.

## Guardrails

- No GitHub fetch.
- No rollback UI.
- No Completion.
- No save, dirty, or editable source editor path.
- No `ProjectSaveService`, `ProjectLoader`, `ObjectAggregator`, or legacy Analysis integration.
- Apply does not run automatically after preview.

## Tests

New and adjusted tests cover:

- Apply plan creation from preview.
- Project target blocked when no project root exists.
- Apply writer and reload callback invocation.
- Writer failure without reload.
- Clear resets apply state.
- Confirmation text includes backup and override warnings.
- Window and Shell guardrails for explicit apply and no rollback/legacy project service usage.

## Manual Smoke Checklist

1. Open IDE Shell.
2. Open Field Registry Manager.
3. Open Field Import Preview.
4. Paste a small markdown table with a new field.
5. Click `Parse & Preview`.
6. Select Global or Project target.
7. Click `Build Apply Plan`.
8. Confirm `Apply`.
9. Verify `user-import.fields.json` appears in the target active folder.
10. Verify local field registry count/highlighting reloads.
