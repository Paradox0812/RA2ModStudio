# WPF Vector Icon Resource Contract

## 1. Scope and Baseline

This document defines the WPF vector icon resource contract for RA2IniEditor.IDE.

This is a contract / planning document only. It does not add runtime icon resources and does not replace the current placeholder toolbar icons.

Baseline references:

```text
Docs/IconToolbarInventory.md
Docs/IconToolbarCommandContract.md
Docs/IconStyleGuide.md
Docs/IconConceptReview.md
Docs/IconSystemCoveragePlan.md
Docs/Codex_RA2IniEditor_IDE_Icon_S1_VectorResourceContract.md
```

Current baseline:

```text
The main toolbar still uses placeholder TextBlock icon resources in Themes/ShellTheme.xaml.
The approved toolbar AutomationIds are preserved.
Icon-0C already cleaned up toolbar enabled-state behavior without replacing icons.
Icon planning now covers main toolbar, AI inline actions, section tree, file types, Field Registry surfaces, faction/house/side icons, and status/source/diff icons.
```

Non-goals for this contract:

```text
no source changes
no XAML changes
no ViewModel changes
no test changes
no project or solution file changes
no SVG / PNG / DrawingImage runtime resources added
no placeholder Icon* replacement
no toolbar command changes
no menu changes
no AI Assistant behavior changes
no Field Registry behavior changes
no parser / diagnostics / completion / hover / quick peek / save preflight changes
no legacy restore
```

## 2. Resource Dictionary Location

Preferred future location:

```text
RA2IniEditor.IDE/Themes/IconResources.xaml
```

Rationale:

```text
Themes/ keeps icon resources near ShellTheme.xaml and other WPF theme assets.
A dedicated IconResources.xaml keeps ShellTheme.xaml focused on shell styling.
It provides one central place for icon geometry, icon presenters, and icon brush aliases.
It avoids scattering icon definitions across ShellWindow.xaml and secondary windows.
```

Resource dictionary merge policy:

```text
IconResources.xaml should be merged once through the existing application/theme resource chain in a later approved implementation phase.
The merge location must be chosen to keep current Shell and secondary windows able to resolve icon resources.
Do not change project files unless a future implementation phase confirms the resource is not included by the existing WPF project conventions.
```

ShellTheme.xaml compatibility policy:

```text
ShellTheme.xaml may keep temporary compatibility aliases for existing Icon* keys only if that reduces migration risk.
Long-term icon geometry should live in IconResources.xaml, not in ShellWindow.xaml.
```

Rejected locations for the first implementation:

```text
RA2IniEditor.IDE/Resources/IconResources.xaml is acceptable in principle but less aligned with current theme organization.
Extending ShellTheme.xaml directly is acceptable only as a temporary compatibility bridge, not as the final icon resource system.
Window-local resources are not acceptable for shared P0 icons.
```

## 3. Resource Types

Preferred runtime strategy:

```text
Use WPF vector resources.
Use simple Path / Geometry / DrawingImage shapes.
Use theme-bound brushes.
Avoid bitmap toolbar icons.
```

First replacement strategy for existing toolbar placeholder keys:

```text
Use reusable icon presenter resources that can replace the current Button Content resources with minimal XAML churn.
For current Content="{StaticResource Icon...}" usage, a Viewbox / Grid / Canvas / Path presenter resource with x:Shared="False" is the safest compatibility shape.
Each presenter should render at a stable 16x16 optical size and use theme-bound Path Stroke / Fill values.
```

Reusable geometry strategy:

```text
Store reusable geometry or drawing resources separately from presenter resources when practical.
Use semantic keys such as IconGeometry.Save or IconDrawing.Save only if the implementation phase also defines a clear presenter mapping.
Do not require every consumer to hand-build Path markup around shared geometry.
```

Recommended split inside IconResources.xaml:

```text
1. Icon brush tokens / aliases.
2. Shared geometry or drawing resources.
3. Compatibility presenter resources for existing Icon* keys.
4. New semantic presenter resources for new surfaces.
```

Acceptable alternatives:

```text
DrawingImage resources are acceptable for Image.Source-style consumers.
ControlTemplate + Path is acceptable if a future IconPresenter control/style is introduced in a dedicated phase.
```

Avoid:

```text
PNG toolbar assets
sliced image2 concept sheets
hard-coded Black / White path colors
WebView / HTML icons
NuGet icon libraries for basic IDE icons
SVG files as runtime dependencies unless a later contract explicitly approves an import/conversion workflow
```

## 4. Brush and Theme Token Strategy

Conceptual icon brush tokens:

| Token | Purpose |
|---|---|
| `IconBrush.Normal` | Default enabled command icons. |
| `IconBrush.Muted` | Secondary, low-priority, or contextual icons. |
| `IconBrush.Hover` | Hover/focused icon state when state-specific brushes are needed. |
| `IconBrush.Pressed` | Pressed/active command state. |
| `IconBrush.Disabled` | Disabled icon state. |
| `IconBrush.Accent` | Restrained accent for current/primary/AI indicators. |
| `IconBrush.Warning` | Warning, Issues, changed/risk indicators. |
| `IconBrush.Error` | Error, invalid, failed validation indicators. |
| `IconBrush.Success` | Success, added, completed indicators. |
| `IconBrush.Destructive` | Revert, discard, rollback, cleanup write-risk indicators. |
| `IconBrush.Project` | Field Registry Project source. |
| `IconBrush.Global` | Field Registry Global source. |
| `IconBrush.BuiltIn` | Field Registry BuiltIn source. |
| `IconBrush.Info` | Neutral informational markers. |
| `IconBrush.Draft` | AI draft/advisory markers when needed. |
| `IconBrush.Advisory` | AI advisory/evidence markers when needed. |

Theme rules:

```text
Normal command icons remain mostly monochrome.
Warning / Error / Success may use semantic brushes.
Project / Global / BuiltIn source icons may use source brushes.
Dark theme compatibility must be possible through resource remapping.
Icon geometry must not hard-code fixed Black, White, Red, or Green.
Disabled icons must remain visible through Disabled brush and/or parent button opacity.
Hover and pressed state should be controlled primarily by button styles, not duplicated icon geometry.
```

Implementation rule:

```text
If exact current Shell brush names are reused, a future implementation phase must inventory them first and map IconBrush.* tokens deliberately.
Use DynamicResource where theme switching or theme resource replacement is expected.
```

## 5. Naming Conventions

Existing WPF-compatible key style:

```text
IconOpenFolder
IconSave
IconUndo
IconRedo
IconRevert
IconEditMode
IconSearch
IconIssues
IconFieldRegistry
IconProjectExplorer
```

New semantic key style:

```text
IconSend
IconCancel
IconCopyMessage
IconCopyCode
IconClearChat
IconFile
IconIniFile
IconSectionGeneric
IconSectionInfantry
IconSectionVehicle
IconSectionAircraft
IconSectionBuilding
IconSectionWeapon
IconSectionWarhead
IconSectionProjectile
IconStatusAdded
IconStatusChanged
IconStatusInvalid
IconStatusWarning
IconStatusError
IconStatusSuccess
IconSourceProject
IconSourceGlobal
IconSourceBuiltIn
```

Rules:

```text
Use command or domain semantics, not placeholder letters.
Do not create keys such as IconO, IconD, or IconBang.
Do not encode AutomationIds in icon resource keys.
Do not encode provider state, API key state, editor dirty state, or Field Registry source priority in geometry keys.
Keep keys stable once runtime XAML and tests reference them.
```

Optional geometry/drawing internal key style:

```text
IconGeometry.Save
IconGeometry.Warning
IconDrawing.Save
IconDrawing.Warning
```

These internal keys should not replace existing public-facing `Icon*` presenter keys unless the implementation phase explicitly migrates consumers and tests.

## 6. Compatibility with Existing Icon* Resources

Compatibility decision:

```text
Preserve current Icon* keys for the main toolbar P0 migration.
Replace their values later with vector presenter resources rather than changing ShellWindow.xaml button references first.
Add new keys only for new surfaces or icons that do not already have placeholder keys.
```

Current placeholder keys to preserve:

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

Preservation requirements:

```text
Do not change existing toolbar AutomationIds.
Do not change existing click handlers.
Do not change existing ToolTips or AutomationProperties.Name except in a later accessibility-specific contract.
Do not restore legacy Shell.FieldRegistryButton.
Do not introduce Shell.MainToolbar.SaveButton to replace Shell.SourceEditor.SaveCurrentFileButton.
Do not use icon replacement as a reason to alter toolbar command order or visibility.
```

Alias strategy:

```text
If new semantic drawing keys are introduced, existing Icon* keys should wrap or reference those drawings.
Example: IconSave remains the toolbar/presenter resource, while IconDrawing.Save may hold reusable drawing data.
```

## 7. P0 Icon Set

P0 should be split into small implementation batches so icon replacement stays reviewable.

### 7.1 P0A: Main Toolbar

| Resource key | Semantic |
|---|---|
| `IconOpenFolder` | Open project folder / folder-open. |
| `IconSave` | Save current file. |
| `IconSearch` | Search / magnifier. |
| `IconIssues` | Issues / diagnostics list. |
| `IconFieldRegistry` | Field library / catalog / registry. |
| `IconUndo` | Undo arrow. |
| `IconRedo` | Redo arrow. |
| `IconRevert` | Revert / discard in-memory changes, distinct from Refresh. |
| `IconProjectExplorer` | Project tree / side panel, if retained on toolbar. |

`IconEditMode` remains compatibility P2 while the button is collapsed, but the key should not be removed.

### 7.2 P0B: AI Inline Actions

| Resource key | Semantic |
|---|---|
| `IconSend` | Send prompt. |
| `IconCancel` | Cancel in-progress send. |
| `IconCopyMessage` | Copy the corresponding assistant message text. |
| `IconCopyCode` | Copy fenced code block content only. |
| `IconClearChat` | Clear chat history. |

AI guardrail:

```text
These icons must not imply Apply, Insert, Save, auto-fix, or editor mutation.
```

### 7.3 P0C: Status and Source

| Resource key | Semantic |
|---|---|
| `IconStatusAdded` | Added / new item. |
| `IconStatusChanged` | Changed / modified item. |
| `IconStatusInvalid` | Invalid row / failed validation. |
| `IconStatusWarning` | Warning. |
| `IconStatusError` | Error. |
| `IconStatusSuccess` | Success. |
| `IconSourceProject` | Project Field Registry source. |
| `IconSourceGlobal` | Global Field Registry source. |
| `IconSourceBuiltIn` | BuiltIn Field Registry source. |
| `IconInfo` | Neutral informational marker. |

### 7.4 P0D: Section Tree and File Basics

| Resource key | Semantic |
|---|---|
| `IconFile` | Generic file. |
| `IconFolder` | Folder. |
| `IconIniFile` | Generic INI file. |
| `IconSectionGeneric` | Generic INI section. |
| `IconSectionInfantry` | Infantry section classifier hint. |
| `IconSectionVehicle` | Vehicle section classifier hint. |
| `IconSectionAircraft` | Aircraft section classifier hint. |
| `IconSectionBuilding` | Building section classifier hint. |
| `IconSectionWeapon` | Weapon section classifier hint. |
| `IconSectionWarhead` | Warhead section classifier hint. |
| `IconSectionProjectile` | Projectile section classifier hint. |

Tree guardrail:

```text
Icons are classification hints only. File names and section IDs remain authoritative text.
Uncertain section kinds must use IconSectionGeneric.
```

## 8. Usage Patterns

Icon-only toolbar buttons:

```text
Keep existing AutomationId.
Keep ToolTip.
Keep AutomationProperties.Name.
Keep command handler.
Keep layout size stable.
Use 16x16 optical icon size.
```

Icon-only inline actions:

```text
Use clear ToolTip and accessible name.
For repeated copy actions, accessible name should identify the scope where practical.
Do not copy hidden context, raw prompt, provider metadata, API data, or editor text unless the existing behavior already does so.
```

Icon + text buttons:

```text
Use icons as supporting visuals only.
Keep text for write actions, destructive actions, rare commands, wizard steps, confirmations, and Field Registry apply/rollback/cleanup operations.
```

Section tree / Project Explorer nodes:

```text
Node text remains visible.
Icon does not replace section kind text when text is needed.
Do not rely only on icon color for type or state.
```

Status / source / difference icons:

```text
Use semantic icon shapes plus text where possible.
Do not encode Added / Changed / Invalid / Project / Global / BuiltIn only by color.
```

## 9. Accessibility and Tooltip Rules

AutomationId rules:

```text
Preserve current AutomationIds unless a later phase explicitly migrates tests.
Do not restore legacy toolbar IDs.
Do not add a new AutomationId solely because an icon resource changed.
Icon resources themselves do not carry AutomationIds.
```

High-sensitivity IDs to preserve:

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
AiAssistant.GenerateButton
AiAssistant.ChatHistory
AiAssistant.ContextSummary
```

Tooltip rules:

```text
Every icon-only command must have a ToolTip.
ToolTips must describe the command, not the icon shape.
Destructive commands must keep clear text and/or tooltip wording.
AI icons must not suggest Apply / Insert / Save.
```

Accessibility rules:

```text
AutomationProperties.Name must remain meaningful for icon-only controls.
Repeated inline actions should have scoped names where practical, such as copy assistant message or copy code block.
Tree node icons must not replace node names.
Status icons must not replace status text in grids or summaries.
```

## 10. Tests to Add / Update

Future implementation tests should cover:

```text
Existing approved toolbar AutomationIds still exist.
Legacy Shell.FieldRegistryButton is not restored.
Existing command handlers remain bound.
Existing menu entries remain available.
P0 icon resource keys exist.
Current placeholder letter resources are no longer present after the replacement phase.
Toolbar button content resolves to vector icon presenters rather than placeholder TextBlock letters.
AI Send / Cancel / Copy Message / Copy Code / Clear preserve existing behavior.
No AiAssistant.ApplyButton or AiAssistant.InsertButton is introduced.
No API key UI is introduced through icon work.
No Field Registry behavior changes through icon work.
No parser / diagnostics / completion / hover / quick peek / save preflight behavior changes through icon work.
```

Test hygiene:

```text
Avoid pixel-perfect tests.
Prefer resource-key, AutomationId, command-boundary, and absence-of-forbidden-control tests.
If a resource dictionary is merged, add a boundary test that the expected P0 keys resolve.
If placeholder replacement is implemented, add a boundary test that old placeholder letters are not used as toolbar icon content.
```

## 11. Manual Smoke Checklist

Future icon implementation phases must manually verify:

```text
Toolbar icons are readable at 100%, 125%, and 150% DPI.
Toolbar layout does not shift or resize unexpectedly.
Disabled Save / Undo / Redo / Revert states remain visibly disabled.
Hover and pressed states still work.
ToolTips appear for icon-only buttons.
Field Registry button still opens the same surface.
Issues button still focuses the same panel.
Search button still opens the same search UI.
AI Send / Cancel / Copy / Copy Code icons do not crowd the composer or message cards.
Copy Message still copies only the corresponding assistant message.
Copy Code still copies only fenced code content.
Section tree icons do not hide file names or section IDs.
Status/source icons remain distinguishable without relying only on color.
No command behavior changes.
No legacy AutomationIds are restored.
```

## 12. Implementation Split

Recommended next phases:

```text
Icon-S2A: Resource dictionary scaffold and brush token mapping
  Add IconResources.xaml and merge it.
  Add only minimal harmless test resources or no visible replacement if possible.
  Confirm resource resolution and theme brush mapping.

Icon-S2B: Main toolbar P0 vector replacement
  Replace existing placeholder Icon* values with vector presenter resources.
  Preserve AutomationIds, handlers, ToolTips, AutomationProperties.Name, layout, and command behavior.

Icon-S2C: AI Assistant inline action icons
  Replace or add Send / Cancel / Copy Message / Copy Code / Clear icons.
  Preserve AI provider behavior, no Apply / Insert, no editor mutation.

Icon-S2D: Status/source icons
  Add Added / Changed / Invalid / Warning / Error / Success / Project / Global / BuiltIn resources.
  Use icons only as supporting visuals next to text.

Icon-S3: Section Tree / Project Explorer node icons
  Add file, folder, INI, and core section-kind icons.
  Requires a separate UI contract because it touches tree item rendering.

Icon-S4: Field Registry surface icons
  Add Field Registry Center / Manager / Import Preview / Cleanup / Rollback / Learning Wizard / Field Editor icon usage.
  Preserve all Field Registry write confirmations and semantics.

Icon-S5: Faction / House / Side icons
  Add Allied / Soviet / Yuri / Neutral only after display locations are contracted.
  Icons remain adjacent to text values.

Icon-S6: Secondary polish pass
  Add P2 icons and consistency fixes after screenshot/manual review.
```

Fast visible-impact option:

```text
Do Icon-S2B first if shell polish is the priority.
Do Icon-S2C first if AI Assistant daily-use polish is the priority.
```

## 13. Acceptance Criteria

Icon-S1 is accepted when this document defines:

```text
1. Preferred ResourceDictionary location.
2. Resource type strategy.
3. Brush/theme token strategy.
4. Naming conventions.
5. Compatibility strategy for existing Icon* placeholder keys.
6. P0 icon batches.
7. Usage, accessibility, and tooltip rules.
8. Future tests and manual smoke checklist.
9. Follow-up implementation split.
10. Confirmation that no source, XAML, tests, runtime resources, project files, or command behavior were changed in Icon-S1.
```

