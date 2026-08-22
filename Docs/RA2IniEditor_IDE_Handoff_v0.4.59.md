# RA2IniEditor IDE Handoff v0.4.59

## Scope

This slice adds a UIA smoke foundation for IDE refactor regression safety.

UI automation remains opt-in. Normal `dotnet test -c Release` does not launch
the IDE window.

## What Changed

- Added `Ra2IdeMainPathSmokeTests`.
- Added `AddProperty.Window` AutomationId for stable window discovery.
- Extended `WpfAutomationHarnessBoundaryTests` to cover the main Source Editor
  smoke IDs and opt-in UIA guard.

## UIA Smoke Coverage

The new smoke test is designed to cover:

- Launch IDE with `--automation-open-folder`.
- Open a temporary INI project.
- Select `rulesmd.ini` from Project Explorer.
- Enter Edit Mode.
- Type into the Source Editor.
- Try Completion preview / commit if the dropdown opens.
- Try Add Property if the dialog opens.
- Revert in-memory changes.
- Assert the source INI on disk is unchanged.

The Completion and Add Property steps are intentionally tolerant in this
foundation slice. They exercise available UIA routes without making ordinary
CI/developer test runs depend on popup timing.

## Opt-in Run Command

Run from the solution root in an interactive desktop session:

```powershell
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Release
```

Without `RA2INIEDITOR_RUN_UI_AUTOMATION=1`, UIA tests compile and report skipped.

## Guardrails

- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or IniFileService dependency.
- No SourceEditor runtime extraction.
- No `SetEditorTextFromProgram` behavior change.
- No `_isSynchronizingEditorText` behavior change.
- No Completion commit behavior change.
- No Add Property insert / replace behavior change.
- No Hover semantics change.
- No Core or Infrastructure public API change.

## Verification

- `dotnet test -c Release`: passed, 843 tests.
- `dotnet test .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Release`: passed with 4 UIA tests skipped by opt-in guard.
- `dotnet build -c Release --no-incremental`: passed, 0 errors, 26 existing warnings.

UIA was not enabled during this handoff to avoid interrupting the interactive
desktop. The enabled run command above is ready for a targeted smoke pass.

Post-handoff interactive note: the first enabled UIA run showed the main path
smoke attempted Enter Edit Mode before selecting an INI file. The smoke was
updated to select `rulesmd.ini` from Project Explorer before clicking Enter Edit
Mode.

Follow-up UIA note: enabled runs then showed Project Explorer exposes the file
name on a child `Text` element rather than the `TreeItem.Name`. The smoke now
matches the child text and clicks that element. This static fix has not been
re-run with UIA in this handoff because the agreed retry limit was reached.

v0.4.59.1 note: Project Explorer items now expose stable automation name/id
bindings, and the main path smoke first looks for
`Shell.ProjectExplorer.File.rulesmd.ini`. The Source Editor state text mojibake
was also repaired. A follow-up enabled UIA run reached Add Property, where the
Add button remained disabled because no field row had been selected; the smoke
now cancels the Add Property window when the button is disabled, keeping this
foundation test focused on the main path rather than row-selection details.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open a folder containing `rulesmd.ini`.
3. Enter Edit Mode.
4. Type a small in-memory edit.
5. Open Completion preview and commit one item.
6. Open Add Property and insert a raw key.
7. Revert.
8. Confirm the source INI file on disk is unchanged.
