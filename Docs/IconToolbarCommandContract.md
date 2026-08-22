# Main Toolbar Command Contract

## 1. Scope And Baseline

This document defines the target command policy for the RA2IniEditor.IDE main Shell toolbar before replacing placeholder letter icons with formal icon resources.

This is a contract / planning document only.

Do not implement UI changes from this document without a later approved implementation phase.

Baseline inputs:

```text
Docs/IconToolbarInventory.md
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/Themes/ShellTheme.xaml
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
```

Current toolbar facts:

```text
Shell.MainToolbar exists below Shell.MainMenu and above the document tab strip.
Current toolbar icons are TextBlock placeholder resources in Themes/ShellTheme.xaml.
Current placeholder icons are letters or punctuation: O, S, U, R, X, E, F, D, !, P.
Current toolbar includes Open / Save / Undo / Redo / Revert / Search / Field Registry / Issues / Project Explorer, plus collapsed EnterEditMode.
Top menu already duplicates many toolbar commands.
```

Non-goals for this contract:

```text
no source changes
no XAML changes
no code-behind changes
no ViewModel changes
no test changes
no script changes
no project or solution file changes
no Field Registry JSON changes
no legacy restore
no icon generation
no placeholder replacement
no command handler changes
no menu entry changes
```

## 2. Current Toolbar Summary

Current main toolbar command inventory:

| Command | Current AutomationId | Handler | Current icon resource | Current status |
|---|---|---|---|---|
| Open Project Folder | `Shell.MainToolbar.OpenFolderButton` | `OpenProjectFolder` | `IconOpenFolder` | Visible |
| Save Current File | `Shell.SourceEditor.SaveCurrentFileButton` | `SaveCurrentFile_OnClick` | `IconSave` | Visible |
| Undo | `Shell.SourceEditor.UndoButton` | `UndoCurrentFile_OnClick` | `IconUndo` | Visible |
| Redo | `Shell.SourceEditor.RedoButton` | `RedoCurrentFile_OnClick` | `IconRedo` | Visible |
| Revert In-Memory Changes | `Shell.SourceEditor.RevertInMemoryChangesButton` | `RevertInMemoryChanges_OnClick` | `IconRevert` | Visible |
| Enter Edit Mode | `Shell.SourceEditor.EnterEditModeButton` | `EnterEditMode_OnClick` | `IconEditMode` | Collapsed |
| Search | `Shell.MainToolbar.SearchButton` | `OpenSearchToolWindow` | `IconSearch` | Visible |
| Field Registry Center | `Shell.MainToolbar.FieldRegistryButton` | `OpenFieldRegistryManagerWindow` | `IconFieldRegistry` | Visible |
| Issues | `Shell.MainToolbar.IssuesButton` | `FocusIssuesToolTab` | `IconIssues` | Visible |
| Project Explorer | `Shell.MainToolbar.ProjectExplorerButton` | `ToggleProjectExplorer` | `IconProjectExplorer` | Visible |

## 3. Menu Duplication Policy

Toolbar and menu duplication is allowed when the command is:

```text
high-frequency
important to source editing
important to diagnostics or registry workflows
safe as a toolbar action
expected in IDE-style layouts
```

Toolbar and menu duplication should be reduced when the command is:

```text
rare
destructive
mostly setup/configuration
domain-specific but not high-frequency
available from a clearer domain menu
```

Policy:

```text
File / Edit / Search menus remain canonical command locations.
The toolbar is for fast access to common IDE actions.
Domain-specific tools may appear on the toolbar only when they are central to the product workflow.
Any duplicate menu removal must be handled by a separate Shell/menu contract.
```

## 4. Final Toolbar Command Set

| Command | Current AutomationId | Final status | State rule | Icon semantic | Notes |
|---|---|---|---|---|---|
| Open Project Folder | `Shell.MainToolbar.OpenFolderButton` | Always visible | Enabled when shell can open a project folder. | folder-open | Primary app entry action. Keep on toolbar and menu. |
| Search | `Shell.MainToolbar.SearchButton` | Always visible | Disabled or no-op-safe when no project/file context is available. | search | Core IDE command. Keep on toolbar and Search menu. |
| Issues | `Shell.MainToolbar.IssuesButton` | Always visible | Enabled when bottom tool area exists; opening/focusing issues should be safe even with no project. | alert-list / warning-list | Core diagnostics entry point. Keep on toolbar and Issues menu. |
| Field Registry Center | `Shell.MainToolbar.FieldRegistryButton` | Always visible | Enabled after Shell initialization; should open read/manage surface without requiring an active file. | database / book / field-library | Core RA2 IDE feature. Keep on toolbar. Menu duplication can be consolidated later. |
| AI Assistant | None on current main toolbar | Future candidate, not approved for current toolbar | If added later, enabled after Shell initialization; provider/network state must not affect opening the panel. | spark / assistant / chat | Do not add in Icon-0B. If added later, use a dedicated Shell UI contract and AutomationId. |
| Save Current File | `Shell.SourceEditor.SaveCurrentFileButton` | Contextual visible | Visible on toolbar; disabled when no file/session or no saveable active file. | save | Keep near source editing actions. Primary style is acceptable. |
| Undo | `Shell.SourceEditor.UndoButton` | Contextual or menu-only candidate | If visible, disabled when no undoable editor action exists. If edit mode is not primary, may be moved to Edit menu only in a future contract. | undo | Current tests preserve toolbar presence. Do not demote without test update. |
| Redo | `Shell.SourceEditor.RedoButton` | Contextual or menu-only candidate | If visible, disabled when no redoable editor action exists. If edit mode is not primary, may be moved to Edit menu only in a future contract. | redo | Current tests preserve toolbar presence. Do not demote without test update. |
| Revert In-Memory Changes | `Shell.SourceEditor.RevertInMemoryChangesButton` | Contextual / destructive | Prefer hidden or disabled unless current file is dirty. If visible, destructive visual treatment and tooltip should be clear. | discard / revert / rotate-back | Rare and destructive. Candidate for menu-only or contextual visibility after explicit contract approval. |
| Enter Edit Mode | `Shell.SourceEditor.EnterEditModeButton` | Hidden / contextual | Remains collapsed unless editable-buffer UX requires explicit edit entry. | pencil / edit | Preserve AutomationId for existing boundary tests while collapsed. |
| Project Explorer | `Shell.MainToolbar.ProjectExplorerButton` | Demote candidate | If kept, enabled when right tool well exists. If toolbar density is reduced, move to View menu / right tool well tab only. | panel-tree | Lower priority than Search / Issues / Field Registry. Demotion requires Shell UI contract and tests. |

## 5. Contextual / Menu-Only Commands

### 5.1 Keep contextual in toolbar for now

```text
Save Current File
Undo
Redo
Revert In-Memory Changes
Enter Edit Mode
Project Explorer
```

These commands already exist in the toolbar and are covered by tests. Their final policy is contextual rather than permanently primary.

### 5.2 Future menu-only candidates

```text
Undo
Redo
Revert In-Memory Changes
Enter Edit Mode
Project Explorer toggle
```

Rationale:

```text
Undo/Redo/Revert are editor-context actions and can dominate the toolbar visually when edit mode is not the central workflow.
Revert is destructive and rare.
Enter Edit Mode is currently collapsed.
Project Explorer toggle is duplicated by the right tool well and View menu.
```

### 5.3 Keep menu-only

```text
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

## 6. No-Project / No-File State Rules

### 6.1 No project loaded

Expected toolbar behavior:

| Command | No-project rule |
|---|---|
| Open Project Folder | Visible and enabled |
| Search | Visible, disabled or opens empty/search surface safely |
| Issues | Visible and enabled; can focus empty Issues panel |
| Field Registry Center | Visible and enabled; Field Registry Center can open without a project |
| AI Assistant | If added later, visible/enabled only to open panel; send behavior follows AI provider rules |
| Save Current File | Visible but disabled |
| Undo | Visible but disabled |
| Redo | Visible but disabled |
| Revert In-Memory Changes | Hidden or disabled |
| Enter Edit Mode | Hidden/collapsed |
| Project Explorer | Visible only if it has meaningful empty-state behavior; otherwise menu/right-panel only |

### 6.2 Project loaded but no active file

Expected toolbar behavior:

| Command | Project/no-file rule |
|---|---|
| Open Project Folder | Visible and enabled |
| Search | Visible and enabled for project search if supported |
| Issues | Visible and enabled |
| Field Registry Center | Visible and enabled |
| Save Current File | Visible but disabled |
| Undo | Visible but disabled |
| Redo | Visible but disabled |
| Revert In-Memory Changes | Hidden or disabled |
| Enter Edit Mode | Hidden/collapsed |
| Project Explorer | Visible/enabled if right panel toggle remains on toolbar |

### 6.3 Active file present and clean

Expected toolbar behavior:

| Command | Active-clean-file rule |
|---|---|
| Save Current File | Visible; enabled if save operation is allowed by current editor state |
| Undo | Visible; enabled only when source editor can undo |
| Redo | Visible; enabled only when source editor can redo |
| Revert In-Memory Changes | Hidden or disabled |
| Enter Edit Mode | Hidden/collapsed unless edit-mode UX is active |

### 6.4 Active file dirty

Expected toolbar behavior:

| Command | Active-dirty-file rule |
|---|---|
| Save Current File | Visible and enabled |
| Revert In-Memory Changes | Visible/enabled or clearly disabled only when revert is unavailable |
| Undo / Redo | Follow editor undo/redo availability |

## 7. AutomationId Preservation Rules

### 7.1 Preserve stable IDs unless explicitly approved

The following IDs are test-sensitive and should be preserved through icon replacement:

```text
Shell.MainToolbar
Shell.MainToolbar.OpenFolderButton
Shell.SourceEditor.SaveCurrentFileButton
Shell.SourceEditor.UndoButton
Shell.SourceEditor.RedoButton
Shell.SourceEditor.RevertInMemoryChangesButton
Shell.SourceEditor.EnterEditModeButton
Shell.MainToolbar.SearchButton
Shell.MainToolbar.FieldRegistryButton
Shell.MainToolbar.IssuesButton
Shell.MainToolbar.ProjectExplorerButton
```

If a command is hidden or moved to a menu in a later phase, update tests deliberately in the same phase.

### 7.2 Do not restore legacy toolbar IDs

Do not reintroduce:

```text
Shell.Toolbar
Shell.OpenFolderButton
Shell.FieldRegistryButton
Shell.Toolbar.SaveCurrentFileButton
```

### 7.3 Future AI Assistant toolbar ID

If AI Assistant is later added to the main toolbar, use a new explicit ID such as:

```text
Shell.MainToolbar.AiAssistantButton
```

Do not overload existing right tool well IDs:

```text
RightToolWell.AiTab
AiAssistant.GenerateButton
```

## 8. Icon Resource Requirements

### 8.1 Required icon semantics

Future formal icon resources should cover:

| Command | Icon semantic |
|---|---|
| Open Project Folder | folder-open |
| Save Current File | save |
| Undo | undo arrow |
| Redo | redo arrow |
| Revert In-Memory Changes | discard changes / revert / rotate-back |
| Enter Edit Mode | pencil / edit |
| Search | magnifier |
| Field Registry Center | database / book / field-library |
| Issues | warning list / alert list |
| Project Explorer | project tree / side panel |
| AI Assistant, if added later | assistant / chat / spark |

### 8.2 Resource strategy

Recommended future approach:

```text
Define vector icon resources in a dedicated WPF ResourceDictionary.
Keep resource keys stable or migrate with explicit tests.
Use icon-only buttons with AutomationProperties.Name and ToolTip.
Do not use generated bitmap assets unless a later asset contract explicitly approves them.
Do not introduce a new NuGet dependency without approval.
```

### 8.3 Current placeholder keys

Current keys may be preserved and have their values replaced later:

```text
IconOpenFolder
IconSave
IconUndo
IconRedo
IconRevert
IconEditMode
IconSearch
IconFieldRegistry
IconIssues
IconProjectExplorer
```

If new icon resource keys are introduced, map old keys to new resources or update tests deliberately.

## 9. Known Test Hygiene Issue

Known mismatch:

```text
RA2IniEditor.UiAutomationTests/FieldImportApplySmokeTests.cs references old Shell.FieldRegistryButton.
Current ShellWindow.xaml exposes Shell.MainToolbar.FieldRegistryButton.
Boundary tests explicitly forbid old Shell.FieldRegistryButton.
```

Do not fix this in Icon-0B.

Recommended later task:

```text
Icon-0T: FieldRegistry toolbar AutomationId test hygiene
```

Acceptable future cleanup:

```text
Update FieldImportApplySmokeTests to click Shell.MainToolbar.FieldRegistryButton.
Keep boundary tests that forbid the old Shell.FieldRegistryButton ID.
Do not add the old ID back to ShellWindow.xaml.
```

## 10. Recommended Implementation Split

### Icon-0C: Main Toolbar State Cleanup

Implement toolbar visibility/enabled-state rules without changing icon resources.

Scope:

```text
Save / Undo / Redo / Revert state rules
Project Explorer demotion decision if approved
No icon replacement yet
No menu restructuring unless separately approved
```

### Icon-0T: FieldRegistry Toolbar AutomationId Test Hygiene

Fix the UI automation test mismatch:

```text
FieldImportApplySmokeTests: Shell.FieldRegistryButton -> Shell.MainToolbar.FieldRegistryButton
```

### Icon-1: Icon Style Guide

Define visual style:

```text
stroke width
grid size
foreground brush binding
hover/disabled behavior
high-DPI behavior
light theme contrast
```

### Icon-2: Concept Sheets

Generate or choose icon concepts only after the command set is stable.

### Icon-3: XAML Vector Icon Resource Dictionary

Convert chosen icons into WPF resources.

### Icon-4: Toolbar Icon Replacement

Replace placeholder text resources with real vector icons while preserving:

```text
AutomationIds
handlers
ToolTips
AutomationProperties.Name
command behavior
layout stability
```

## 11. Acceptance Criteria

This contract is accepted when it clearly defines:

```text
1. Always-visible toolbar commands.
2. Contextual toolbar commands.
3. Menu-only or demotion candidates.
4. No-project and no-file state rules.
5. AutomationId preservation rules.
6. Future icon resource requirements.
7. Known FieldRegistry automation test mismatch.
8. Future implementation split.
```

No implementation is part of Icon-0B.
