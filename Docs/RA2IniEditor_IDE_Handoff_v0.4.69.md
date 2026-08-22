# RA2IniEditor IDE Handoff v0.4.69

## 1. Target

v0.4.69 performs a lightweight Chinese UI text cleanup before packaging.

The goal is to remove obvious mojibake and English placeholders from the IDE Shell main path without introducing a full i18n system.

## 2. Scope

Changed user-facing text in:

- Source editor empty/loading/read-failed states.
- Shell project open/load/output status messages.
- Issues status text and issue count text.
- Static guardrail tests for Shell, Add Property, Field Annotation Editor, and AutomationId stability.

## 3. Explicit Non-goals

This version does not modify:

- Save service behavior.
- Save writer, rollback service, backup semantics, or dirty state semantics.
- Completion commit behavior.
- Add Property insert/replace behavior.
- Hover, Definition, or References behavior.
- Legacy `ProjectSaveService`, legacy `IniFileService`, or Save All.
- Core or Infrastructure public API.
- XAML layout structure or AutomationId names.

## 4. AutomationId Status

The existing AutomationIds remain stable, including:

- `Shell.OpenFolderButton`
- `Shell.SourceEditor`
- `Shell.SourceEditor.TextArea`
- `Shell.SourceEditor.EnterEditModeButton`
- `Shell.SourceEditor.SaveCurrentFileButton`
- `Shell.SourceEditor.RevertInMemoryChangesButton`
- `Shell.SourceEditor.EditorStateText`
- `Shell.OutputTextBox`
- `AddProperty.SearchTextBox`
- `AddProperty.AddSelectedButton`

## 5. Validation Notes

The guardrail now asserts real Chinese text instead of mojibake placeholders and rejects common mojibake fragments in the main UI sources.

Recommended validation:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

Optional UIA save smoke:

```powershell
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test RA2IniEditor.UiAutomationTests -c Release --no-restore --filter FullyQualifiedName~Ra2IdeSaveSmokeTests
```

## 6. Manual Smoke Checklist

1. Launch `RA2IniEditor.IDE`.
2. Confirm the empty source editor shows Chinese text.
3. Open a folder and confirm Output uses Chinese status text.
4. Select an INI file and confirm load/diagnostic messages are readable.
5. Open Add Property and Field Annotation Editor and confirm primary labels are Chinese.
6. Confirm Save Current File and Ctrl+S behavior is unchanged.
