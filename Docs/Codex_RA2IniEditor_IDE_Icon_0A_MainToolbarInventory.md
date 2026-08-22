# Codex Task: RA2IniEditor.IDE Icon-0A Main Toolbar Rationalization / Inventory

## 0. Current Baseline

User uploaded the current project package and a main-shell screenshot.

Observed main-shell UI:

- Top menu already contains 文件 / 编辑 / 视图 / 搜索 / 项目 / 字段库 / 问题 / 工具 / 帮助.
- Main toolbar currently shows letter-placeholder icon buttons:
  - O = Open Folder
  - S = Save
  - U = Undo
  - R = Redo
  - X = Revert
  - F = Search
  - D = Field Registry
  - ! = Issues
  - P = Project Explorer
- In `Themes/ShellTheme.xaml`, these toolbar icons are currently `TextBlock` placeholder resources, for example `IconOpenFolder` = `O`, `IconSave` = `S`, etc.
- In `Views/ShellWindow.xaml`, the main toolbar consumes these resources through `Content="{StaticResource Icon...}"`.

User feedback:

```text
左上角的部分图标功能与目前菜单栏重复。
```

This task is **read-only planning / inventory first**.

Do not implement icons yet.

---

## 1. Goal

Create a main-toolbar rationalization inventory before real icon work begins.

The output should decide:

1. Which toolbar actions should stay as high-frequency quick actions.
2. Which actions should be moved back to menus only.
3. Which actions should be hidden/collapsed when no file/project is active.
4. Which placeholder letter icons must be replaced by real icon resources later.
5. Which AutomationIds / handlers must be preserved.

---

## 2. Hard Boundaries

Do not modify:

- XAML
- code-behind
- ViewModels
- tests
- scripts
- project files
- Field Registry JSON
- legacy files

Do not generate or add icons in this phase.

Do not remove menu commands.

Do not change command handlers or behavior.

---

## 3. Files to Inspect

Read only:

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`
- `RA2IniEditor.Tests/IDE/*Shell*Tests*.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`
- Any existing icon/style resource dictionaries

---

## 4. Recommended Toolbar Policy

### 4.1 Menu vs Toolbar

Menus are allowed to contain complete command coverage.

Toolbar should contain only high-frequency commands.

Duplication is acceptable only for high-frequency actions, such as:

- Open Project
- Save Current File
- Search
- Issues
- Field Registry
- AI Assistant / right tool well
- Toggle Project Explorer

### 4.2 Candidate actions to keep on toolbar

Recommended P0 toolbar buttons:

| Action | Keep? | Reason |
|---|---|---|
| Open Project Folder | Yes | Primary entry point |
| Save Current File | Yes, but disabled/hidden when no file | High-frequency once editing exists |
| Search | Yes | IDE-level high-frequency |
| Issues | Yes | Diagnostics visibility |
| Field Registry Center | Yes | Project core feature |
| AI Assistant | Yes | New core feature |
| Project Explorer Toggle | Maybe | Useful if right panel can collapse |

### 4.3 Candidate actions to demote from toolbar

Recommended to keep in Edit menu or contextual editor UI:

| Action | Toolbar recommendation | Reason |
|---|---|---|
| Undo | Hide/collapse unless editor is active/editable | Menu already has it; confusing in no-file state |
| Redo | Hide/collapse unless editor is active/editable | Same |
| Revert in-memory changes | Hide/collapse unless current file is dirty | Dangerous/rare action |
| Enter Edit Mode | Keep hidden unless supported/currently useful | Currently collapsed in XAML |

### 4.4 No-file / no-project state

When no file/project is open, toolbar should not show irrelevant active-looking commands.

Preferred behavior:

- Open Project remains visible.
- Search may remain visible but disabled if no project.
- Save / Undo / Redo / Revert should be disabled or collapsed.
- Issues / Field Registry may remain accessible if global info is useful, but should not look like primary file actions.

---

## 5. Required Document Output

Create:

```text
Docs/IconToolbarInventory.md
```

Suggested structure:

```markdown
# Main Toolbar Icon Inventory and Rationalization

## 1. Current Toolbar Inventory

| AutomationId | Handler | Current Placeholder | Current Tooltip | Menu Duplicate | Proposed Status | Notes |

## 2. Placeholder Icon Resources

| Resource Key | Current Value | Proposed Icon Semantics | Priority |

## 3. Menu Duplication Assessment

## 4. Recommended Toolbar Set

## 5. Actions to Demote / Hide Contextually

## 6. Required AutomationId Preservation

## 7. Future Icon Resource Plan

## 8. Implementation Phases
```

---

## 6. Future Implementation Phases

After this inventory is approved:

### Icon-0B: Toolbar layout contract

Define final toolbar command set and state rules.

### Icon-1: Icon style guide

Create `Docs/IconStyleGuide.md`.

### Icon-2: Concept generation

Use image2 only after inventory and style guide are approved.

### Icon-3: XAML vector resource implementation

Replace placeholder `TextBlock` resources with `DrawingImage` / `PathGeometry`.

### Icon-4: Toolbar replacement

Update Shell toolbar only, preserving AutomationIds and handlers.

---

## 7. Validation Commands

Documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing, run full validation.

---

## 8. Final Report Format

Report:

```text
1. Phase completed: Icon-0A.
2. Files inspected.
3. Files changed.
4. Current toolbar inventory summary.
5. Recommended toolbar actions.
6. Actions recommended for menu/context only.
7. Placeholder icon resource findings.
8. Commands run.
9. Test/package results.
10. Confirmation no source/XAML behavior changed.
11. Recommended next phase.
```
