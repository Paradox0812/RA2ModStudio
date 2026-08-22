# Codex Task: RA2IniEditor.IDE Icon-S2B Main Toolbar P0 Vector Replacement

## 0. Current Baseline

Icon-S2A has been completed.

Reported state:

```text
RA2IniEditor.IDE/Themes/IconResources.xaml added.
IconBrush.* token scaffold added.
IconSampleCheck sample vector resource added, not consumed by production UI.
App.xaml now merges:
1. Themes/ShellTheme.xaml
2. Themes/IconResources.xaml
3. Resources/Styles/IdeSecondaryWindowStyles.xaml

Existing toolbar placeholder resources remain unchanged in ShellTheme.xaml:
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

Tests: 1435 passed.
IdeOnly package: passed, packaged file count 780.
No visible icon replacement yet.
No command behavior changed.
Legacy AutomationIds not restored.
```

Next phase:

```text
Icon-S2B: Main toolbar P0 vector replacement
```

This is a limited implementation phase.

The goal is to replace main-toolbar placeholder letter/symbol resources with WPF vector icon resources while preserving toolbar behavior.

---

## 1. Goal

Replace current main toolbar placeholder icon resources with real vector icon presenter resources.

Current placeholder icons are letter/symbol `TextBlock` resources such as:

```text
O
S
U
R
X
F
D
!
P
```

Icon-S2B should replace their visual content with vector resources.

Required result:

```text
1. Main toolbar no longer shows letter placeholders for P0 icons.
2. Existing Icon* resource keys remain compatible.
3. Existing toolbar AutomationIds remain unchanged.
4. Existing ToolTips / AutomationProperties.Name remain unchanged.
5. Existing click handlers / command behavior remain unchanged.
6. No PNG / SVG runtime assets are added.
7. No project/solution file changes unless explicitly required and approved.
```

---

## 2. Hard Boundaries

Do not:

```text
change toolbar command set
change toolbar layout beyond icon content fitting
change command handlers
remove menu entries
restore legacy AutomationIds
add PNG/SVG/image runtime assets
use image2 output directly as runtime asset
change AI Assistant behavior
change Field Registry behavior
change parser / diagnostics / completion / hover / quick peek / save preflight behavior
change Project Explorer behavior
```

Do not modify:

```text
Field Registry JSON
legacy files
solution files
project files
```

Do not implement Section Tree icons, Field Registry panel icons, AI inline icons, status/source icons, or faction icons in this phase.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Themes/IconResources.xaml
RA2IniEditor.IDE/Themes/ShellTheme.xaml
RA2IniEditor.Tests/IDE/IconResourceBoundaryTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Only if required for a trivial resource resolution fix:

```text
RA2IniEditor.IDE/App.xaml
```

Do not modify ShellWindow.xaml unless a compile or resource binding issue requires it and the change is strictly limited to resource consumption.

Preferred approach:

```text
Preserve ShellWindow.xaml as-is.
Replace existing Icon* resource values with vector presenter resources compatible with existing Content="{StaticResource Icon...}" usage.
```

---

## 4. Required Icon Keys

Replace these existing placeholder keys with vector presenter resources:

```text
IconOpenFolder
IconSave
IconUndo
IconRedo
IconRevert
IconSearch
IconFieldRegistry
IconIssues
IconProjectExplorer
```

Keep compatibility key:

```text
IconEditMode
```

Because Enter Edit Mode remains collapsed, `IconEditMode` can either remain placeholder for now or be converted if trivial. Prefer converting if it does not broaden scope.

---

## 5. Visual Semantics

### 5.1 Main toolbar P0 icons

| Resource key | Icon semantics |
|---|---|
| IconOpenFolder | folder-open |
| IconSave | save / document-save |
| IconUndo | undo curved arrow |
| IconRedo | redo curved arrow |
| IconRevert | discard/reset current changes; must not look like Refresh |
| IconSearch | magnifier |
| IconFieldRegistry | field catalog / library / database-book |
| IconIssues | diagnostics / warning list |
| IconProjectExplorer | project tree / side panel |
| IconEditMode | pencil / edit mode |

### 5.2 Style rules

Use style from `Docs/IconStyleGuide.md`:

```text
modern IDE
monoline / outline-first
16x16 readability
low visual noise
theme-bound brushes
no decorative Red Alert art
no 3D / glossy / cartoon style
```

### 5.3 Revert distinction

Important:

```text
IconRevert must not look like Refresh.
```

Preferred metaphor:

```text
document with back arrow
discard/reset document
undo-to-baseline
```

Avoid:

```text
plain circular refresh arrows
```

---

## 6. Resource Implementation Rules

### 6.1 Resource shape

Existing Shell toolbar uses `Content="{StaticResource Icon...}"`.

Therefore each replacement `Icon*` key should be a reusable content presenter resource.

Recommended:

```xml
<Viewbox x:Key="IconSearch" x:Shared="False" Width="16" Height="16">
    ...
</Viewbox>
```

or equivalent:

```xml
<Grid x:Key="IconSearch" x:Shared="False" Width="16" Height="16">
    <Path ... />
</Grid>
```

Rules:

```text
1. Use x:Shared="False" for reusable FrameworkElement resources.
2. Use 16x16 stable size.
3. Use theme-bound icon brush resources.
4. Do not hard-code black/white path colors.
5. Avoid complex geometry.
6. Keep icon resources centralized.
```

### 6.2 Brush usage

Use existing IconBrush tokens from Icon-S2A:

```text
IconBrush.Normal
IconBrush.Warning
IconBrush.Disabled
IconBrush.Muted
```

For normal toolbar icons, prefer:

```text
IconBrush.Normal
```

For Issues, a warning token may be used if subtle and theme-safe.

Do not overuse accent colors.

---

## 7. AutomationId / Behavior Rules

Must preserve existing approved IDs:

```text
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

Must not restore:

```text
Shell.FieldRegistryButton
```

Must preserve:

```text
ToolTips
AutomationProperties.Name
Click handlers
Command semantics
State behavior from Icon-0C
```

---

## 8. Tests

Add/update boundary tests.

Required:

```text
1. Existing main-toolbar AutomationIds still exist.
2. Old Shell.FieldRegistryButton is not restored.
3. P0 Icon* resource keys resolve.
4. P0 Icon* resources are no longer one-character placeholder TextBlocks.
5. Icon resources are vector/presenter resources, not PNG/SVG runtime assets.
6. Main toolbar command handlers remain wired.
7. Menus remain present.
8. Save / Undo / Redo / Revert state rules from Icon-0C still pass.
```

Avoid pixel-perfect tests.

Suggested resource tests:

```text
IconOpenFolder is not TextBlock with Text="O".
IconSave is not TextBlock with Text="S".
IconIssues is not TextBlock with Text="!".
```

Do not make geometry-shape tests too brittle.

---

## 9. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 10. Manual Smoke Checklist

After implementation:

```text
1. Launch IDE.
2. Confirm toolbar no longer shows O / S / U / R / X / F / D / ! / P placeholders.
3. Confirm icons are readable at normal scale.
4. Confirm Open Project opens project folder flow.
5. Confirm Search opens search UI.
6. Confirm Issues opens/focuses issues panel.
7. Confirm Field Registry opens the same surface.
8. Confirm Project Explorer behavior remains unchanged.
9. Confirm Save / Undo / Redo / Revert enabled/disabled behavior remains from Icon-0C.
10. Confirm ToolTips still appear.
11. Confirm no missing resource exceptions.
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: Icon-S2B.
2. Files changed.
3. Icon resources replaced.
4. Resource implementation strategy.
5. AutomationIds preserved.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no PNG/SVG runtime assets added.
11. Confirmation no command behavior changed.
12. Manual smoke steps or result.
13. Remaining risks.
14. Recommended next phase.
```
