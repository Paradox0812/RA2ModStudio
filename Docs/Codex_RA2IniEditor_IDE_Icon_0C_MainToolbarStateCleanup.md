# Codex Task: RA2IniEditor.IDE Icon-0C Main Toolbar State Cleanup

## 0. Current Baseline

Icon-0B has been completed.

Reported state:

```text
Docs/IconToolbarCommandContract.md created.
No source / XAML behavior changed.
No icon resources added.
No placeholder icon resources replaced.
```

Icon-0B defines the main toolbar policy:

```text
Permanent / primary toolbar actions:
- Open Project Folder
- Search
- Issues
- Field Registry

Contextual toolbar actions:
- Save
- Undo
- Redo
- Revert
- Enter Edit Mode
- Project Explorer

Menu-only / demotion candidates:
- Undo
- Redo
- Revert
- Enter Edit Mode
- Project Explorer
```

Next phase:

```text
Icon-0C: Main Toolbar State Cleanup
```

This is a limited implementation phase.

The goal is to make the current toolbar less confusing before real icon design begins.

---

## 1. Goal

Clean up the main toolbar state and visibility rules while preserving existing behavior and command handlers.

The toolbar should no longer show low-context actions as active primary controls when no project/file/editor state exists.

Required result:

```text
1. Keep high-frequency primary toolbar actions visible.
2. Make file/editor-related actions contextual.
3. Preserve existing AutomationIds for approved toolbar buttons.
4. Do not replace placeholder letter icons yet.
5. Do not introduce new icon resources yet.
6. Do not change command semantics.
```

---

## 2. Hard Boundaries

Do not:

```text
generate icons
replace Icon* resources
add SVG / PNG / DrawingImage resources
change command handlers
remove menu entries
restore legacy toolbar IDs
change Field Registry / diagnostics / parser / save behavior
change AI Assistant behavior
change project explorer behavior beyond toolbar visibility/state
```

Do not modify:

```text
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn Field Registry JSON
legacy files
solution files / project files
```

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if toolbar state already lives there or minimal state is required
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/FieldImportApplySmokeTests.cs, only for AutomationId hygiene if necessary
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not modify:

```text
Themes/ShellTheme.xaml
```

unless a compile/test issue requires a trivial resource reference fix.

This phase should not change the placeholder icon resources.

---

## 4. Toolbar State Rules

### 4.1 Always visible / primary

Keep visible:

```text
Open Project Folder
Search
Issues
Field Registry
```

If a command is not meaningful without a project, prefer disabled over hidden only when this is already how the shell behaves.

### 4.2 Save

Rule:

```text
Save is visible but disabled when no current file/document exists.
Save is enabled only when current document save semantics allow it.
```

Do not change save behavior.

### 4.3 Undo / Redo

Preferred rule:

```text
Undo / Redo are hidden or disabled unless editor/edit mode state makes them meaningful.
```

If existing command availability is not easily observable, use a conservative disabled state rather than creating new behavior.

Do not implement new undo/redo logic.

### 4.4 Revert

Rule:

```text
Revert is hidden or disabled unless current file has dirty/in-memory changes.
```

Because Revert is destructive/rare, it should not look like a primary action in no-file/no-dirty states.

Do not change revert behavior or confirmation flow.

### 4.5 Enter Edit Mode

If currently collapsed, keep collapsed unless the current contract says otherwise.

Do not expose edit mode as a new visible feature in this phase.

### 4.6 Project Explorer

If the toolbar Project Explorer button currently duplicates the right-side Section / AI tool well and View menu, evaluate according to the contract:

```text
Option A: keep visible if it toggles right panel and is high-frequency.
Option B: demote/hide if it is redundant and right panel is already visible by default.
```

Do not remove Project Explorer functionality.

---

## 5. AutomationId Rules

Preserve current approved AutomationIds, especially:

```text
Shell.MainToolbar.OpenFolderButton
Shell.MainToolbar.SaveButton
Shell.MainToolbar.SearchButton
Shell.MainToolbar.FieldRegistryButton
Shell.MainToolbar.IssuesButton
```

Do not restore or reintroduce old legacy ID:

```text
Shell.FieldRegistryButton
```

Known issue from Icon-0B:

```text
FieldImportApplySmokeTests may still reference old Shell.FieldRegistryButton.
```

If tests fail because of this mismatch, update tests to the current approved AutomationId:

```text
Shell.MainToolbar.FieldRegistryButton
```

Do not change production XAML to satisfy old test IDs.

---

## 6. UI Requirements

Keep this phase minimal.

Allowed:

```text
Visibility / IsEnabled / style state changes
small separator adjustments if needed
toolbar grouping cleanup
```

Forbidden:

```text
new visual icon design
large toolbar redesign
new command surfaces
new menus
new keyboard shortcuts
```

The toolbar should feel less misleading in this state:

```text
no project / no file / no active editor
```

---

## 7. Tests

Update boundary tests to cover:

```text
1. Approved toolbar AutomationIds remain.
2. Old Shell.FieldRegistryButton is not restored.
3. No-file/no-project toolbar state does not show inappropriate active edit actions.
4. Save / Undo / Redo / Revert state matches contract.
5. Open Project / Search / Issues / Field Registry remain accessible as designed.
6. Menu entries remain present.
7. Command handlers are still wired.
```

Avoid pixel-perfect tests.

---

## 8. Validation Commands

Run full validation because XAML / tests may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Launch IDE with no project open.
2. Confirm toolbar no longer presents Save/Undo/Redo/Revert as misleading active primary controls.
3. Confirm Open Project remains available.
4. Confirm Search / Issues / Field Registry behavior is unchanged.
5. Open a project/file.
6. Confirm Save state behaves as before.
7. Confirm menu commands still exist.
8. Confirm no placeholder icon replacement occurred.
9. Confirm no legacy toolbar IDs were restored.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: Icon-0C.
2. Files changed.
3. Toolbar state changes.
4. Contextual/menu-only decisions implemented.
5. AutomationIds preserved/updated.
6. Known test hygiene issue handled or deferred.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation no icon replacement/resource generation occurred.
12. Confirmation no command behavior changed.
13. Manual smoke steps or result.
14. Recommended next phase.
```
