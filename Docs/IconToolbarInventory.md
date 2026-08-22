# Icon Toolbar Inventory

## 1. Scope

This document is a read-only inventory of the current main Shell toolbar and related menu duplication.

Files inspected:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/Themes/ShellTheme.xaml
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2EditableBufferUiBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2EditorStateShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2SaveCurrentFileUiIntegrationTests.cs
RA2IniEditor.Tests/IDE/Ra2UndoRedoUiBoundaryTests.cs
RA2IniEditor.UiAutomationTests/Ra2IdeSaveSmokeTests.cs
RA2IniEditor.UiAutomationTests/Ra2IdeMainPathSmokeTests.cs
RA2IniEditor.UiAutomationTests/FieldImportApplySmokeTests.cs
```

No source, XAML, tests, scripts, project files, Field Registry JSON, or legacy files were modified for this inventory.

## 2. Current Toolbar Structure

The main toolbar is the `Border` with:

```text
AutomationId: Shell.MainToolbar
Style: IdeMainToolbarStyle
Location: ShellWindow.xaml, below Shell.MainMenu and above the document tab strip
```

Toolbar layout:

```text
Open project group
separator
current editor command group
separator
tool/navigation group
```

Toolbar button style:

```text
Normal icon button style: IdeIconCommandButtonStyle
Primary icon button style: IdePrimaryIconCommandButtonStyle
```

The current icon resources are all placeholder `TextBlock` resources in `Themes/ShellTheme.xaml`. They are single-character placeholders, not formal vector or image icons.

## 3. Current Icon Resource Inventory

| Resource key | Current placeholder text | Current usage |
|---|---:|---|
| `IconOpenFolder` | `O` | Open project folder toolbar button |
| `IconSave` | `S` | Save current file toolbar button |
| `IconUndo` | `U` | Undo toolbar button |
| `IconRedo` | `R` | Redo toolbar button |
| `IconRevert` | `X` | Revert in-memory changes toolbar button |
| `IconEditMode` | `E` | Enter edit mode toolbar button, currently collapsed |
| `IconSearch` | `F` | Search toolbar button |
| `IconFieldRegistry` | `D` | Field Registry toolbar button |
| `IconIssues` | `!` | Issues toolbar button |
| `IconProjectExplorer` | `P` | Project Explorer toolbar button |

Assessment:

```text
These placeholders are useful for stable layout and tests, but they read as letter buttons rather than production icons.
Future replacement should use formal icon resources while preserving AutomationIds and command handlers.
```

## 4. Main Toolbar Button Inventory

| Order | Button purpose | AutomationId | Handler | Icon resource | Current visibility | Menu duplicate | Test sensitivity | Recommendation |
|---:|---|---|---|---|---|---|---|---|
| 1 | Open project folder | `Shell.MainToolbar.OpenFolderButton` | `OpenProjectFolder` | `IconOpenFolder` (`O`) | Visible | `Shell.Menu.OpenFolder` / File > Open project | High | Keep visible. Replace placeholder with formal folder/open icon later. |
| 2 | Save current file | `Shell.SourceEditor.SaveCurrentFileButton` | `SaveCurrentFile_OnClick` | `IconSave` (`S`) | Visible | File > Save current file | High | Keep visible and primary. Replace placeholder with formal save icon later. |
| 3 | Undo | `Shell.SourceEditor.UndoButton` | `UndoCurrentFile_OnClick` | `IconUndo` (`U`) | Visible | Edit > Undo | High | Keep visible. Replace placeholder with formal undo icon later. |
| 4 | Redo | `Shell.SourceEditor.RedoButton` | `RedoCurrentFile_OnClick` | `IconRedo` (`R`) | Visible | Edit > Redo | High | Keep visible. Replace placeholder with formal redo icon later. |
| 5 | Revert in-memory changes | `Shell.SourceEditor.RevertInMemoryChangesButton` | `RevertInMemoryChanges_OnClick` | `IconRevert` (`X`) | Visible | Edit > Revert in-memory changes | High | Keep visible but consider stronger destructive-action styling/tooltip in a future UI contract. Do not remove without replacing automation coverage. |
| 6 | Enter edit mode | `Shell.SourceEditor.EnterEditModeButton` | `EnterEditMode_OnClick` | `IconEditMode` (`E`) | Collapsed | No direct visible menu duplicate | High as hidden boundary | Keep hidden/collapsed for current boundary. Future decision depends on editable-buffer UX. |
| 7 | Search | `Shell.MainToolbar.SearchButton` | `OpenSearchToolWindow` | `IconSearch` (`F`) | Visible | `Shell.Menu.Search` / Search > Find | High | Keep visible. Replace placeholder with formal search icon later. |
| 8 | Field Registry Center | `Shell.MainToolbar.FieldRegistryButton` | `OpenFieldRegistryManagerWindow` | `IconFieldRegistry` (`D`) | Visible | View > Field Registry Center; Field Registry > Field Registry Center | High | Keep visible for now because Field Registry is core. Replace placeholder with formal database/book icon later. Consider reducing duplicate menu entries, not toolbar, in a separate contract. |
| 9 | Issues panel | `Shell.MainToolbar.IssuesButton` | `FocusIssuesToolTab` | `IconIssues` (`!`) | Visible | View > Issues; Issues > Open Issues | High | Keep visible. Replace placeholder with formal warning/list icon later. |
| 10 | Project Explorer | `Shell.MainToolbar.ProjectExplorerButton` | `ToggleProjectExplorer` | `IconProjectExplorer` (`P`) | Visible | View > Project Explorer | Medium | Candidate to hide or demote if toolbar density becomes a problem, but keep until a Shell UI contract updates tests. |

## 5. Menu Duplication Inventory

### 5.1 Exact or near-exact duplicates

| Command | Toolbar | Menu entries | Notes |
|---|---|---|---|
| Open project folder | Visible toolbar button | File > Open project | Expected duplicate; keep both. |
| Save current file | Visible toolbar button | File > Save current file | Expected duplicate; keep both. |
| Undo | Visible toolbar button | Edit > Undo | Expected duplicate; keep both. |
| Redo | Visible toolbar button | Edit > Redo | Expected duplicate; keep both. |
| Revert in-memory changes | Visible toolbar button | Edit > Revert in-memory changes | Useful duplicate because command is destructive. |
| Search | Visible toolbar button | Search > Find | Expected duplicate; keep both. |
| Field Registry Center | Visible toolbar button | View > Field Registry Center; Field Registry > Field Registry Center | Duplicate across toolbar plus two menus. Consider consolidating menu placement later. |
| Issues panel | Visible toolbar button | View > Issues; Issues > Open Issues | Duplicate across toolbar plus two menus. Consider keeping toolbar and one menu path. |
| Project Explorer | Visible toolbar button | View > Project Explorer | Expected duplicate; optional toolbar demotion candidate. |

### 5.2 Menu-only commands

These have no main toolbar button and should likely remain menu/context-menu-only unless a later feature demands high-frequency toolbar access:

```text
AI Assistant
Output
Search Results
Bottom Tool Panel toggle
Find current file references
Go to definition
Peek definition
Add property
Show completion candidates
Field learning commands
Advanced Field Registry tools
Reload Field Registry
Diagnostics refresh / full diagnostics / clear issues
Options / logs / help disabled placeholders
```

## 6. Test Boundary Inventory

Current tests explicitly preserve:

```text
Shell.MainToolbar
IdeMainToolbarStyle
Shell.MainToolbar.OpenFolderButton
Shell.SourceEditor.SaveCurrentFileButton
Shell.SourceEditor.UndoButton
Shell.SourceEditor.RedoButton
Shell.SourceEditor.RevertInMemoryChangesButton
Shell.SourceEditor.EnterEditModeButton
Shell.MainToolbar.SearchButton
Shell.MainToolbar.FieldRegistryButton
Shell.MainToolbar.IssuesButton
```

Tests also assert that legacy toolbar IDs are absent:

```text
Shell.Toolbar
Shell.OpenFolderButton
Shell.FieldRegistryButton
Shell.Toolbar.SaveCurrentFileButton
```

Important boundary:

```text
Any future toolbar cleanup must preserve or intentionally update these tests.
```

Observed test mismatch to review in a later test-maintenance task:

```text
RA2IniEditor.UiAutomationTests/FieldImportApplySmokeTests.cs clicks Shell.FieldRegistryButton, while ShellWindow.xaml currently exposes Shell.MainToolbar.FieldRegistryButton and tests explicitly assert the legacy Shell.FieldRegistryButton AutomationId is absent.
```

This inventory does not change that mismatch because the current task is documentation-only.

## 7. Recommendations

### 7.1 Keep visible in main toolbar

```text
Open project folder
Save current file
Undo
Redo
Revert in-memory changes
Search
Field Registry Center
Issues
```

Rationale:

```text
These are high-frequency or safety-critical IDE actions and already have test coverage.
```

### 7.2 Keep hidden / collapsed

```text
Enter edit mode
```

Rationale:

```text
It is part of the editable-buffer boundary but currently not a visible primary command.
```

### 7.3 Candidate for toolbar demotion or conditional visibility

```text
Project Explorer toggle
```

Rationale:

```text
It duplicates View > Project Explorer and is less central than Save / Undo / Search / Issues / Field Registry. However, do not remove it without a Shell UI contract and test update.
```

### 7.4 Candidate for menu consolidation

```text
Field Registry Center duplicated under View and Field Registry menus
Issues duplicated under View and Issues menus
```

Suggested future direction:

```text
Keep toolbar shortcut.
Keep the domain-specific menu path.
Remove or demote the View duplicate only after explicit Shell/menu contract approval.
```

### 7.5 Future icon replacement

Replace placeholder text icons with formal icons in a dedicated UI contract:

| Current resource | Suggested formal icon meaning |
|---|---|
| `IconOpenFolder` | folder-open |
| `IconSave` | save |
| `IconUndo` | undo |
| `IconRedo` | redo |
| `IconRevert` | rotate/back or discard changes |
| `IconEditMode` | pencil/edit |
| `IconSearch` | search |
| `IconFieldRegistry` | database/book/library |
| `IconIssues` | alert/list |
| `IconProjectExplorer` | panel/tree |

Constraints for future replacement:

```text
Do not change command semantics.
Do not change AutomationIds unless tests and automation harnesses are updated deliberately.
Do not add Apply / Insert / AI provider controls to the main toolbar.
Do not restore legacy toolbar IDs.
Do not replace placeholders with generated assets in an inventory-only task.
```

## 8. Non-Goals

This inventory did not:

```text
generate icons
modify ShellWindow.xaml
modify ShellWindow.xaml.cs
modify ShellTheme.xaml
modify tests
run UI automation
change menu layout
change toolbar layout
change command behavior
change parser / diagnostics / Field Registry behavior
restore legacy UI
```
