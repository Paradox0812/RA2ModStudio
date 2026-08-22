# Codex Task: RA2IniEditor.IDE Icon-0B Main Toolbar Command Contract

## 0. Current Baseline

Icon-0A has been completed.

Reported state:

```text
Docs/IconToolbarInventory.md created.
No source / XAML / code-behind / ViewModel / tests / scripts / project files / Field Registry JSON / legacy files changed.
No icons generated.
No UI behavior changed.
```

Icon-0A findings include:

```text
1. Current toolbar uses single-character placeholder icon resources in Themes/ShellTheme.xaml.
2. Current toolbar buttons include Open / Save / Undo / Redo / Revert / Search / Field Registry / Issues / Project Explorer, plus collapsed EnterEditMode.
3. Top menu already covers many of the same commands.
4. FieldImportApplySmokeTests references old Shell.FieldRegistryButton while current XAML exposes Shell.MainToolbar.FieldRegistryButton and boundary tests forbid old ID.
```

Next phase:

```text
Icon-0B: Main Toolbar Command Contract
```

This phase is **contract / planning only**.

Do not modify XAML or code in this task.

---

## 1. Goal

Define the final main-toolbar command policy before replacing placeholder letter icons with real icons.

The goal is to decide:

```text
1. Which actions stay visible on the main toolbar.
2. Which actions move to menu-only.
3. Which actions become contextual / hidden / disabled.
4. Which AutomationIds must be preserved.
5. Which icon resources will be required later.
6. Which test mismatch needs separate cleanup.
```

---

## 2. Hard Boundaries

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
ShellViewModel.cs
Themes/ShellTheme.xaml
tests
project files
solution files
Field Registry JSON
legacy files
```

Do not generate icons.

Do not replace placeholder resources.

Do not change command handlers.

Do not change menu entries.

---

## 3. Required Input

Read:

```text
Docs/IconToolbarInventory.md
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Themes/ShellTheme.xaml
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
```

Use only read-only inspection.

---

## 4. Toolbar Policy to Decide

### 4.1 Recommended stable visible toolbar actions

Recommended final always-visible or primary toolbar actions:

```text
Open Project Folder
Search
Issues
Field Registry
AI Assistant / Right Tool Well access, if currently available from toolbar or planned
```

### 4.2 Contextual actions

Recommended contextual actions:

```text
Save Current File:
  visible but disabled when no file, or hidden until file exists.

Undo / Redo:
  visible only when editor is active/editable, or demoted to Edit menu until full edit mode is stable.

Revert:
  hidden unless current file is dirty.
  Because it is destructive/rare, it should not be a permanent primary toolbar icon.

Project Explorer toggle:
  keep only if right panel collapse/toggle is a common workflow.
  Otherwise demote to View menu.
```

### 4.3 Menu-only candidates

Recommended menu-only / contextual candidates:

```text
Undo
Redo
Revert
Enter Edit Mode
```

The final decision should consider current app state:

```text
If editing mode is still limited / not primary, Undo/Redo/Revert should not dominate the main toolbar.
```

---

## 5. Required Output

Create or update:

```text
Docs/IconToolbarCommandContract.md
```

Suggested structure:

```markdown
# Main Toolbar Command Contract

## 1. Scope and Baseline

## 2. Current Toolbar Summary

## 3. Menu Duplication Policy

## 4. Final Toolbar Command Set

| Command | Current AutomationId | Final Status | State Rule | Icon Semantic | Notes |

## 5. Contextual / Menu-only Commands

## 6. No-project / No-file State Rules

## 7. AutomationId Preservation Rules

## 8. Icon Resource Requirements

## 9. Known Test Hygiene Issue

## 10. Recommended Implementation Split

## 11. Acceptance Criteria
```

---

## 6. Known Test Hygiene Issue

Document this issue explicitly:

```text
FieldImportApplySmokeTests references old Shell.FieldRegistryButton.
Current XAML exposes Shell.MainToolbar.FieldRegistryButton.
Boundary tests forbid the old ID.
```

Do not fix it in Icon-0B unless user explicitly approves a test hygiene task.

Recommended later task:

```text
Icon-0T: FieldRegistry toolbar AutomationId test hygiene
```

or include it in the first implementation phase if tests fail.

---

## 7. Future Implementation Split

After Icon-0B is approved:

### Icon-0C: Main Toolbar State Cleanup

Implement toolbar command visibility / enabled state rules without changing icons.

### Icon-1: Icon Style Guide

Define icon design rules.

### Icon-2: image2 Concept Sheets

Generate visual concepts only after command set is stable.

### Icon-3: XAML Vector Icon Resource Dictionary

Convert selected icons to WPF resources.

### Icon-4: Toolbar Icon Replacement

Replace placeholder text resources with real vector icons.

---

## 8. Validation Commands

Documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Final Report Format

Report:

```text
1. Phase completed: Icon-0B.
2. Files inspected.
3. Files changed.
4. Final toolbar command policy.
5. Contextual/menu-only decisions.
6. No-project/no-file state rules.
7. Known test hygiene issue recorded.
8. Commands run.
9. Test/package result.
10. Confirmation no source/XAML behavior changed.
11. Recommended next phase.
```
