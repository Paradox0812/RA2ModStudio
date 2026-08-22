# Codex Task: RA2IniEditor.IDE Icon-S1 WPF Vector Icon Resource Contract

## 0. Current Baseline

IconSystemCoveragePlan has been completed.

Reported state:

```text
Docs/IconSystemCoveragePlan.md created.
The icon plan now covers:
- Main Toolbar Icons
- Field Registry secondary / tertiary panels
- Section Tree / Project Explorer node type icons
- File type icons
- Faction / House / Side icons
- AI Assistant inline action icons
- Status / Source / Difference icons

No source / XAML / ViewModel / tests / resources / project files changed.
No icons generated.
No runtime resources added.
```

Next phase:

```text
Icon-S1: WPF Vector Icon Resource Contract
```

This is a contract / planning phase.

Do not implement icon resources yet.

---

## 1. Goal

Define how RA2IniEditor.IDE will store, name, theme, and consume WPF vector icons before replacing any placeholder letters or adding real icon resources.

The contract must decide:

1. ResourceDictionary location.
2. Resource key naming.
3. Brush / color token strategy.
4. Compatibility with current `Icon*` placeholder resource keys.
5. P0 implementation scope.
6. Tests and manual smoke requirements.
7. Migration strategy for main toolbar, AI inline actions, Section Tree, Field Registry, and status icons.

---

## 2. Hard Boundaries

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
ShellTheme.xaml
ViewModels
tests
project files
solution files
Field Registry JSON
legacy files
```

Do not:

```text
add SVG / PNG / DrawingImage runtime resources
replace placeholder Icon* resources
change command handlers
change toolbar/menu behavior
change Project Explorer / Section Tree behavior
change AI Assistant behavior
```

This phase only creates a contract document.

---

## 3. Required Input Documents

Read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/IconToolbarInventory.md
Docs/IconToolbarCommandContract.md
Docs/IconStyleGuide.md
Docs/IconConceptReview.md
Docs/IconSystemCoveragePlan.md
```

Use these documents as the source of truth.

---

## 4. Required Output

Create:

```text
Docs/IconVectorResourceContract.md
```

Suggested structure:

```markdown
# WPF Vector Icon Resource Contract

## 1. Scope and Baseline

## 2. Resource Dictionary Location

## 3. Resource Types

## 4. Brush and Theme Token Strategy

## 5. Naming Conventions

## 6. Compatibility with Existing Icon* Resources

## 7. P0 Icon Set

## 8. Usage Patterns

## 9. Accessibility and Tooltip Rules

## 10. Tests to Add / Update

## 11. Manual Smoke Checklist

## 12. Implementation Split

## 13. Acceptance Criteria
```

---

## 5. Resource Dictionary Strategy

Evaluate and choose one preferred location.

Recommended options:

```text
Option A:
  RA2IniEditor.IDE/Themes/IconResources.xaml

Option B:
  RA2IniEditor.IDE/Resources/IconResources.xaml

Option C:
  Extend RA2IniEditor.IDE/Themes/ShellTheme.xaml
```

Preferred recommendation:

```text
Create a dedicated IconResources.xaml in a theme/resource folder.
Keep ShellTheme.xaml focused on shell styling and temporary compatibility aliases.
```

Do not implement this in Icon-S1; only document it.

---

## 6. Resource Type Strategy

Preferred runtime representation:

```text
DrawingImage / GeometryDrawing / PathGeometry
```

Alternative acceptable:

```text
ControlTemplate + Path
```

Avoid:

```text
PNG toolbar assets
sliced image2 icon sheets
hard-coded black/white fills
WebView / HTML-based icons
NuGet dependency for basic icons
```

The contract should specify whether the first implementation should use:

```text
DrawingImage resources
```

or:

```text
Path data resources plus a reusable IconPresenter style
```

---

## 7. Brush / Theme Token Strategy

Define conceptual brushes:

```text
IconBrush.Normal
IconBrush.Muted
IconBrush.Hover
IconBrush.Disabled
IconBrush.Warning
IconBrush.Error
IconBrush.Success
IconBrush.Accent
IconBrush.Project
IconBrush.Global
IconBrush.BuiltIn
```

Rules:

```text
1. Normal command icons are monochrome.
2. Warning / Error / Success may use semantic brushes.
3. Project / Global / BuiltIn source icons may use source brushes.
4. Dark theme compatibility must be possible.
5. Icon geometry must not hard-code black / white.
```

If actual brush resources do not yet exist, define this as a future mapping.

---

## 8. Compatibility with Existing Placeholder Keys

Current toolbar consumes existing resources such as:

```text
IconOpenFolder
IconSave
IconSearch
IconIssues
IconFieldRegistry
IconProjectExplorer
IconUndo
IconRedo
IconRevert
```

Contract must decide:

```text
1. Preserve these keys and replace their values with vector icon presenters later.
2. Or introduce new semantic resources and alias old keys during migration.
```

Recommended:

```text
Preserve current Icon* keys for main toolbar P0 migration to minimize XAML churn.
Add new names only for new surfaces.
```

---

## 9. P0 Icon Set

Define the first runtime vector batch.

Recommended P0:

### Main Toolbar

```text
Open Project Folder
Save
Search
Issues
Field Registry
Undo
Redo
Revert
Project Explorer, if retained
```

### AI Inline Actions

```text
Send
Cancel
Copy Message
Copy Code Block
Clear Chat
```

### Status / Source

```text
Warning
Error
Success
Info
Project
Global
BuiltIn
```

### Section Tree Core

```text
File
Generic INI
Generic Section
Infantry
Vehicle
Aircraft
Building
Weapon
Warhead
Projectile
```

If this is too large, contract should split P0 into:

```text
P0A: Main toolbar + AI inline
P0B: Status/source
P0C: Section tree
```

---

## 10. Usage Pattern Rules

For icon-only buttons:

```text
1. Keep or add ToolTip.
2. Keep AutomationProperties.Name.
3. Preserve existing AutomationId.
4. Do not remove visible text from risky write actions.
```

For icon + text buttons:

```text
Use icon as supporting visual only.
Keep text for write/destructive/rare actions.
```

For Section Tree nodes:

```text
Icon is decorative/classification hint.
Node text remains authoritative.
Do not rely only on icon or color.
```

---

## 11. Tests to Plan

Future implementation tests should cover:

```text
1. Existing toolbar AutomationIds remain.
2. Placeholder letter content is no longer present after replacement.
3. Icon resources exist for P0 keys.
4. No legacy Shell.FieldRegistryButton is restored.
5. AI Send / Copy / CopyCode buttons preserve AutomationIds and behavior.
6. No Apply / Insert behavior added.
7. Field Registry semantics unchanged.
8. Menu entries remain.
```

Avoid pixel-perfect tests.

---

## 12. Manual Smoke Checklist

Future icon implementation must verify:

```text
1. Icons readable at 100%, 125%, 150% DPI.
2. Disabled state is visible.
3. Hover/pressed states still work.
4. ToolTips appear.
5. Buttons do not resize unexpectedly.
6. Toolbar layout remains stable.
7. AI panel inline icons do not crowd composer.
8. Section tree node icons do not hide names.
9. No command behavior changed.
```

---

## 13. Implementation Split

Recommended next phases after contract approval:

```text
Icon-S2A: Main toolbar P0 vector resource implementation
Icon-S2B: AI Assistant inline action icons
Icon-S2C: Status/source icons
Icon-S3: Section Tree node icons
Icon-S4: Field Registry surface icons
Icon-S5: Faction / House / Side icons
```

If user wants visible impact quickly:

```text
Do Icon-S2B first for AI Send / Copy / Copy Code / Clear.
```

If user wants shell polish first:

```text
Do Icon-S2A first for main toolbar placeholder replacement.
```

---

## 14. Acceptance Criteria

Icon-S1 is accepted when:

```text
1. Resource location is chosen.
2. Resource type strategy is chosen.
3. Brush/theme token plan is defined.
4. Existing Icon* compatibility strategy is defined.
5. P0 icon list is defined.
6. Tests and smoke checklist are defined.
7. No source/XAML/runtime resource changes are made.
```

---

## 15. Validation Commands

Documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing, run full validation.

---

## 16. Final Report Format

Report:

```text
1. Phase completed: Icon-S1.
2. Files changed.
3. Resource dictionary decision.
4. Resource type strategy.
5. Brush/theme token strategy.
6. Existing Icon* compatibility strategy.
7. P0 icon set.
8. Commands run.
9. Test/package result.
10. Confirmation no source/XAML/runtime icon resources changed.
11. Recommended next phase.
```
