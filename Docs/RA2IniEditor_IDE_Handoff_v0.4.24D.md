# RA2IniEditor IDE Handoff v0.4.24D

## 1. Version Scope

Target version: v0.4.24D WPF UI Automation Harness + Highlight Smoke Diagnostics.

This version only adds WPF UI automation anchors, an automation-only IDE startup argument, a separate FlaUI smoke test project, and documentation for readonly highlighting diagnostics. It does not implement rollback UI, GitHub integration, completion, INI save, dirty tracking, or editor mutation flows.

## 2. UI Automation Anchors

Stable `AutomationProperties.AutomationId` values were added to the IDE shell and field registry windows so smoke tests can locate controls without depending on localized text or visual layout.

Key IDs:

- `Shell.Window`
- `Shell.OpenFolderButton`
- `Shell.FieldRegistryButton`
- `Shell.SourceEditor`
- `Shell.ProjectExplorer`
- `Shell.OutputTextBox`
- `FieldRegistryManager.Window`
- `FieldRegistryManager.ReloadButton`
- `FieldRegistryManager.OpenFieldImportPreviewButton`
- `FieldRegistryManager.PacksGrid`
- `FieldRegistryManager.StatusText`
- `FieldImportPreview.Window`
- `FieldImportPreview.RawTextBox`
- `FieldImportPreview.ParsePreviewButton`
- `FieldImportPreview.TargetScopeComboBox`
- `FieldImportPreview.ApplyModeComboBox`
- `FieldImportPreview.BuildApplyPlanButton`
- `FieldImportPreview.ApplyButton`
- `FieldImportPreview.ApplyStatusText`
- `FieldImportPreview.LastApplyManifestPathText`

These IDs are intended to be stable test contracts. Avoid renaming them unless the UI automation tests and handoff docs are updated together.

## 3. Automation Startup Argument

The IDE now supports:

```text
--automation-open-folder <path>
```

When provided, the IDE opens the folder after the shell window is created. When absent, normal startup behavior is unchanged. Invalid or missing paths are reported to the shell output area and do not crash startup.

This argument is only a test harness convenience. It does not open dialogs, apply field imports, save INI files, mark dirty state, or alter runtime behavior outside the requested folder load.

## 4. UI Automation Tests

A separate test project was added:

```text
RA2IniEditor.UiAutomationTests
```

It references FlaUI and `RA2IniEditor.IDE`. It is intentionally not part of the ordinary solution test path so normal test runs do not launch WPF windows.

Default run:

```text
dotnet test RA2IniEditor.UiAutomationTests -c Release
```

The smoke tests compile and are skipped by default unless this environment variable is set:

```text
RA2INIEDITOR_RUN_UI_AUTOMATION=1
```

Explicit enabled run example:

```text
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test RA2IniEditor.UiAutomationTests -c Release
```

The smoke tests cover the field import preview path at UIA level:

- Launch IDE with `--automation-open-folder`.
- Wait until the automation-opened project reports discovered INI files.
- Open Field Registry Manager.
- Open Field Import Preview.
- Paste markdown harvest text.
- Parse preview.
- Select Project target and AppendOrUpdate mode.
- Build apply plan.
- Apply and confirm the message box.
- Verify project active pack and manifest are written.
- Verify manager status can be reloaded.

The confirmation dialog lookup searches both the application top-level windows and the desktop UIA tree. This is intentional because WPF `MessageBox` can be missed by `Application.GetAllTopLevelWindows(...)` in some UIA sessions.

## 5. Highlight Smoke Diagnostics

AvalonEdit visual colors are not asserted through UI Automation. UIA does not provide a reliable contract for text color spans inside AvalonEdit.

Readonly highlighting diagnostics remain covered through tokenizer and transformer-oriented tests:

- Known field tokenization.
- Unknown field tokenization.
- Section kind inference.
- Registry-driven section classification.
- BuiltIn / Local / Composite provider behavior.

Future highlighter regressions should be diagnosed by source-level tokenizer tests first, then by manual visual smoke checks in the IDE.

## 6. Guardrails

This version keeps the readonly IDE boundaries:

- No Rollback UI.
- No GitHub fetch.
- No completion.
- No INI save.
- No dirty tracking.
- No editor mutation chain.
- No legacy `ProjectLoader`, `ProjectSaveService`, `ObjectAggregator`, or Analysis integration.

`SourceEditor.Text` continues to be treated as readonly display content for the IDE shell.

## 7. Verification

Validated commands:

```text
dotnet test -c Release
dotnet build -c Release --no-incremental
dotnet test RA2IniEditor.UiAutomationTests -c Release
```

Observed results:

- Normal tests passed: 373/373.
- Release build passed: 0 errors.
- Warning count remained at 26 existing warnings.
- UI automation project compiled.
- UI automation smoke tests were skipped by default when `RA2INIEDITOR_RUN_UI_AUTOMATION` was not set: 0 failed, 0 passed, 2 skipped.
- UI automation smoke tests passed in an interactive desktop session with `RA2INIEDITOR_RUN_UI_AUTOMATION=1`: 2/2 passed.

## 8. Next Step

Keep the UIA smoke project outside ordinary solution test execution unless the team explicitly decides to move it into a separate CI job with interactive desktop support.
