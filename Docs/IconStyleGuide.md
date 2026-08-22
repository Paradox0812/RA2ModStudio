# RA2IniEditor.IDE Icon Style Guide

## 1. Scope and Goals

This guide defines the icon design system for RA2IniEditor.IDE before any placeholder toolbar icons are replaced.

It applies to future icon work for:

```text
main Shell toolbar
source editor contextual commands
Field Registry windows and workflows
AI Assistant panel actions
status / diagnostics / feedback indicators
```

Goals:

```text
create a consistent IDE-oriented visual language
keep icons readable at 16x16
support light and future dark themes
preserve command semantics and AutomationIds during implementation
avoid bitmap dependency for runtime toolbar icons
provide clear prompt rules for future image2 concept exploration
```

Non-goals for this guide:

```text
no icon generation
no XAML or source modification
no placeholder Icon* resource replacement
no command handler changes
no toolbar visibility/state changes
no menu restructuring
```

## 2. Visual Direction

RA2IniEditor.IDE icons should follow a compact desktop IDE style:

```text
minimalist
monoline / outline-first
geometric but not sterile
clear at 16x16
low visual noise
consistent corner radius and stroke behavior
suited to source editing, diagnostics, and data/reference workflows
```

Avoid:

```text
realistic 3D rendering
cartoon / app-store style iconography
heavy gradients
photo-like textures
decorative RA2 unit art inside toolbar icons
overly detailed 16x16 glyphs
fixed black icons that fail in dark theme
```

Recommended style direction:

```text
Primary: monoline IDE icons
Secondary exploration: filled minimal icons for status badges only
Accent usage: small semantic color accents, not full-color toolbar icons
```

The product identity should come from the IDE shell and RA2 workflow context, not from colorful command icons.

## 3. Icon Sizes

Primary runtime sizes:

| Usage | Size | Notes |
|---|---:|---|
| Main toolbar | 16x16 | Default production toolbar size. Must remain readable at 100% DPI. |
| Dense inline actions | 14x14 or 16x16 | Copy, close, clear, small assistant actions. Prefer 16x16 when space allows. |
| Tool window buttons | 16x16 or 20x20 | Use 20x20 only where existing layout has enough room. |
| Dialog / large action affordances | 20x20 or 24x24 | Field Registry / wizard headers and larger empty states. |
| Empty-state illustration | 24x24 to 48x48 | Not part of main toolbar replacement. Requires separate UI contract. |

Design grid:

```text
Draw master icons on a 24x24 grid.
Export/adapt toolbar icons to 16x16.
Keep the main shape inside a 1 px optical padding at 16x16.
Avoid relying on details smaller than 1 px at runtime size.
```

High-DPI behavior:

```text
Use WPF vector resources for scale independence.
Avoid bitmap toolbar icons unless a later asset contract explicitly approves them.
Test at 100%, 125%, and 150% scaling in manual smoke for icon replacement phases.
```

## 4. Stroke and Geometry Rules

Stroke rules:

```text
Use 2 px visual stroke on a 24x24 design grid.
At 16x16 runtime size, preserve crisp optical weight equivalent to about 1.25-1.5 px.
Use round line caps and round joins where they improve readability.
Keep strokes consistent within each icon family.
Do not mix heavy filled shapes with thin outline details in the same toolbar family.
```

Geometry rules:

```text
Prefer simple silhouettes.
Use straight 45-degree or 90-degree angles for command arrows.
Use rounded rectangles for files, panels, cards, and database/library shapes.
Avoid tiny inner cutouts that disappear at 16x16.
Keep icons centered optically, not only mathematically.
Use consistent arrowhead size for Undo / Redo / Revert.
```

Fill rules:

```text
Default toolbar icons are outline-first.
Small filled areas are allowed for visual anchors, such as warning triangles or database caps.
Semantic status icons may use filled badges when contrast is required.
Do not use decorative fills for normal commands.
```

## 5. Color Tokens and Theme Behavior

Future implementation should use theme-bound brushes instead of hard-coded icon colors.

Conceptual icon color tokens:

| Token | Purpose |
|---|---|
| `IconBrush.Normal` | Default enabled command icons. |
| `IconBrush.Muted` | Low-priority or secondary actions. |
| `IconBrush.Hover` | Hover/focused command icons. |
| `IconBrush.Pressed` | Pressed active command state. |
| `IconBrush.Disabled` | Disabled command icons. |
| `IconBrush.Accent` | Rare accent for selected tool, AI Assistant, or primary state. |
| `IconBrush.Warning` | Warning / Issues indicator. |
| `IconBrush.Error` | Error diagnostics or destructive failure state. |
| `IconBrush.Success` | Successful operation state. |
| `IconBrush.Destructive` | Revert / discard style where needed. |

Theme behavior:

```text
Light theme icons should use neutral slate-like foregrounds with sufficient contrast.
Dark theme icons should invert through brush resources, not through separate icon geometry.
Disabled icons should reduce opacity or use a disabled brush, never disappear due to low contrast.
Warning / Error / Success colors should meet contrast expectations against toolbar backgrounds.
Hover and pressed states should be controlled by button style resources, not by duplicating icon geometries.
```

Implementation guidance:

```text
Prefer DynamicResource for icon brushes when theme switching is supported.
Use current Shell theme brush names only after a dedicated resource implementation phase inventories them.
Do not embed fixed Black / White / Red / Green values in Path.Fill or Stroke for reusable icons.
```

## 6. Icon Categories

### 6.1 Main Toolbar

Required icon semantics:

| Command | Semantic direction |
|---|---|
| Open Project Folder | folder-open, optional small project marker |
| Save Current File | disk/save, document-save, or IDE save glyph |
| Search | magnifier |
| Issues | warning list, alert list, or diagnostic panel |
| Field Registry | database, book/library, or fields catalog |
| Project Explorer | project tree, side panel, or hierarchy |
| AI Assistant, if later promoted | assistant chat, spark, or tool-window assistant glyph |

### 6.2 Contextual Editing

Required icon semantics:

| Command | Semantic direction |
|---|---|
| Undo | left curved arrow |
| Redo | right curved arrow |
| Revert In-Memory Changes | discard, rotate-back, reset document |
| Enter Edit Mode | pencil/edit |

These icons should be visually quieter than the main app entry commands. Revert should not look like a harmless refresh action.

### 6.3 Field Registry

Required icon semantics for later phases:

```text
Field Registry Center
Advanced Tools
Learn Fields
New Field
Edit Field
Import Preview
Rollback
Cleanup
Open Folder
Warning
```

Field Registry icons should feel like reference data / schema / catalog tools, not like database administration software.

### 6.4 AI Assistant

Required icon semantics for later phases:

```text
AI Assistant
Send
Cancel
Copy Message
Copy Code Block
Clear Chat
Model Selector
Markdown
Code Block
Context
```

AI Assistant icons must preserve draft/advisory semantics. Do not use icons that imply Apply, Insert, Save, or automatic file mutation.

### 6.5 Status and Diagnostics

Required icon semantics:

```text
Info
Warning
Error
Success
Pending
Disabled
```

Status icons may use semantic color, but normal toolbar command icons should remain mostly monochrome.

## 7. Naming Conventions

Use semantic names, not current placeholder letters or visual implementation details.

Preferred conceptual names:

```text
Icon.OpenFolder
Icon.Save
Icon.Undo
Icon.Redo
Icon.Revert
Icon.EditMode
Icon.Search
Icon.Issues
Icon.FieldRegistry
Icon.ProjectExplorer
Icon.AiAssistant
Icon.Send
Icon.Copy
Icon.CodeBlock
Icon.Warning
Icon.Error
Icon.Success
```

Current WPF-compatible resource key style:

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
IconAiAssistant
IconSend
IconCopy
IconCodeBlock
IconWarning
IconError
IconSuccess
```

Rules:

```text
Do not name icons after placeholder letters such as IconO or IconD.
Do not name icons after shape-only descriptions such as IconCircleArrow unless the semantic command is unclear.
Keep resource keys stable once tests and XAML reference them.
If new keys replace old keys, provide a deliberate migration plan and boundary tests.
```

## 8. WPF Resource Strategy

Preferred final runtime strategy:

```text
Use XAML vector resources.
Use Path / Geometry / DrawingImage resources, depending on what fits existing WPF style.
Centralize reusable icons in a dedicated ResourceDictionary such as IconResources.xaml, or in a clearly separated icon section of ShellTheme.xaml.
Bind icon stroke/fill to theme brushes.
Keep toolbar buttons icon-only with ToolTip and AutomationProperties.Name.
Preserve existing AutomationIds and command handlers during replacement.
```

Current placeholder keys may be preserved and have their values replaced later:

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

Recommended resource shape:

```text
Brush tokens:
  IconBrush.Normal
  IconBrush.Muted
  IconBrush.Disabled
  IconBrush.Warning
  IconBrush.Error
  IconBrush.Success
  IconBrush.Accent

Icon resources:
  one resource per semantic icon
  no command handler or AutomationId encoded in the icon resource
  no window-specific geometry duplication unless the icon is truly local
```

Avoid:

```text
PNG toolbar icons
runtime slicing from image2 concept sheets
new NuGet icon libraries without approval
hard-coded path colors
scattered icon definitions across many windows
replacing AutomationIds to match icon names
```

## 9. image2 Concept Prompt Rules

image2 output should be used only for concept exploration, not as final runtime asset output.

Do not generate image2 concepts in Icon-1. Use the following prompt format only in a later approved Icon-2 phase.

Base prompt template:

```text
Create an icon concept sheet for a desktop IDE application called RA2IniEditor.IDE.
Style: modern minimalist IDE toolbar icons, monoline outline-first, consistent geometry, low visual noise, readable at 16x16.
Use a clean grid layout with no text labels and no captions.
Use monochrome neutral stroke icons with optional small semantic accents for warning/error/success only.
Use a transparent or light neutral background.
Keep all icons consistent in stroke width, corner radius, optical size, and perspective.
Do not use realistic 3D, cartoon, glossy, skeuomorphic, or colorful app-style icons.
Include concepts for: Open Project Folder, Save Current File, Undo, Redo, Revert In-Memory Changes, Search, Issues, Field Registry, Project Explorer, AI Assistant, Send, Copy, Code Block, Warning, Error, Success.
```

Recommended concept batches:

```text
A. Monoline IDE: pure outline, neutral monochrome.
B. Filled Minimal: small filled silhouettes for stronger 16x16 readability.
C. Hybrid Line + Accent: outline commands with semantic accent badges for Issues / Status / AI.
```

Concept review criteria:

```text
Can each icon be recognized at 16x16?
Does the sheet feel like one family?
Are normal commands mostly monochrome?
Does Revert avoid looking like Refresh?
Does Field Registry read as field/library/catalog rather than generic database admin?
Does AI Assistant avoid implying automatic Apply/Insert?
Can the concept be converted into simple WPF vector geometry?
```

## 10. Icon Acceptance Criteria

Future icon assets are acceptable only when they satisfy:

```text
readable at 16x16
consistent stroke/fill style
theme brush compatible
no hard-coded runtime colors except explicitly approved semantic accents
no loss of AutomationIds
no command handler changes
no menu behavior changes
no Apply / Insert implication in AI icons
no visual ambiguity between Revert and Refresh
no dependency on bitmap scaling for toolbar icons
no new NuGet dependency unless separately approved
```

Manual visual smoke for future icon replacement:

```text
Open IDE at 100% DPI and 125% DPI.
Confirm toolbar icons are recognizable without text labels.
Confirm disabled Save / Undo / Redo / Revert states remain visibly disabled.
Confirm warning/status icons remain legible.
Confirm light theme contrast is acceptable.
Confirm no icon shifts toolbar layout.
Confirm no legacy AutomationIds were restored.
```

## 11. Future Implementation Plan

Recommended sequence:

```text
Icon-1: Icon Style Guide
  Create this document only.

Icon-2: Icon Concept Sheets
  Generate or choose 2-3 concept directions using image2 or manual references.
  No runtime resources yet.

Icon-2R: Concept Review
  Pick one direction and freeze the approved command icon list.

Icon-3: WPF Vector Resource Contract
  Define exact ResourceDictionary location, resource keys, brush tokens, and tests.
  Stop for approval before implementation.

Icon-4: WPF Vector Icon Resource Implementation
  Add vector resources only.
  Preserve current placeholder key compatibility or migrate keys explicitly.
  No command behavior changes.

Icon-5: Main Toolbar Placeholder Replacement
  Replace placeholder text resources with approved vector icons.
  Preserve AutomationIds, ToolTips, AutomationProperties.Name, handlers, and layout.

Icon-6: Secondary Surface Icon Pass
  Apply the same style to Field Registry, AI Assistant, dialogs, and status surfaces.
```

Implementation guardrails for all future phases:

```text
Do not modify parser, diagnostics, Field Registry semantics, save preflight, AI provider behavior, or editor text behavior as part of icon work.
Do not restore legacy toolbar IDs.
Do not add Apply / Insert controls through icon work.
Do not change solution or project files unless a future resource dictionary phase explicitly requires it and is approved.
```
