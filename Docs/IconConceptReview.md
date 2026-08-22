# Icon Concept Review

## 1. Scope

This document prepares visual concept directions for RA2IniEditor.IDE icon work.

This is a concept / review document only:

```text
no runtime icon resources
no XAML changes
no source changes
no placeholder Icon* replacement
no SVG / PNG / DrawingImage resources added to the project
no command handler changes
no toolbar state changes
no menu changes
```

image2 concepts, if generated in a later approved step, are visual references only. They must not be treated as final runtime assets or directly sliced into toolbar icons.

Baseline references:

```text
Docs/IconToolbarInventory.md
Docs/IconToolbarCommandContract.md
Docs/IconStyleGuide.md
```

## 2. Required Icon List

Every concept direction should include the same baseline list so the directions can be compared fairly.

### 2.1 Main Toolbar

```text
Open Project Folder
Save
Search
Issues
Field Registry
Project Explorer
AI Assistant
```

### 2.2 Editing / Contextual

```text
Undo
Redo
Revert
Edit Mode
```

### 2.3 Field Registry

```text
Field Registry Center
Learn Fields
New Field
Edit Field
Import Preview
Rollback
Cleanup
Open Folder
Warning
```

### 2.4 AI Assistant

```text
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

### 2.5 Status

```text
Info
Warning
Error
Success
Pending
Disabled
```

## 3. Concept Directions

### 3.1 Direction A: Monoline IDE

Expected style:

```text
outline-first
single consistent stroke weight
mostly monochrome
minimal interior detail
best fit for 16x16 toolbar readability
closest to a conventional professional IDE command set
```

Design expectations:

```text
Folder, Save, Search, Undo, Redo, and Project Explorer should be immediately recognizable.
Field Registry should read as field library / catalog / database, not generic server administration.
Issues should read as diagnostics list or alert list.
Revert must not look like Refresh.
AI Assistant should read as advisory assistant / chat, not Apply / Insert / auto-fix.
```

Likely strengths:

```text
best WPF vector conversion feasibility
strong light/dark theme compatibility
least visual noise in the existing Shell toolbar
most consistent with IconStyleGuide primary direction
```

Likely risks:

```text
may feel too plain if Field Registry and AI Assistant are not distinctive enough
thin details may disappear if the generator over-compresses 16x16 forms
semantic status icons may need small filled shapes later for contrast
```

image2 prompt:

```text
Create a clean icon concept sheet for a desktop IDE application called RA2IniEditor.IDE.
Direction: Monoline IDE.
Style: modern minimalist professional IDE toolbar icons, outline-first, one consistent stroke weight, neutral monochrome, simple vector-like symbols, readable at 16x16.
Use a strict grid layout with evenly spaced icons, no text labels, no captions, no letters, no numbers.
Use a transparent or very light neutral background.
Keep all icons consistent in stroke width, corner radius, optical size, and perspective.
Use low visual noise and avoid tiny interior details.
Do not use realistic 3D, cartoon style, glossy effects, skeuomorphic rendering, decorative Red Alert unit art, or colorful app icons.
Include icon concepts for:
Open Project Folder, Save, Search, Issues, Field Registry, Project Explorer, AI Assistant,
Undo, Redo, Revert, Edit Mode,
Field Registry Center, Learn Fields, New Field, Edit Field, Import Preview, Rollback, Cleanup, Open Folder, Warning,
Send, Cancel, Copy Message, Copy Code Block, Clear Chat, Model Selector, Markdown, Code Block, Context,
Info, Warning, Error, Success, Pending, Disabled.
Make Revert distinct from Refresh by emphasizing discard/reset of current changes.
Make AI Assistant advisory and chat-like, not file-apply or auto-fix.
Make Field Registry feel like a field library/catalog for an INI editor.
```

### 3.2 Direction B: Filled Minimal

Expected style:

```text
simple filled silhouettes
high contrast
reduced line complexity
stronger recognition at small sizes
useful for status badges and dense actions
```

Design expectations:

```text
Main command silhouettes should remain simple enough for WPF vector conversion.
Status icons may use filled symbol shapes.
Normal toolbar commands should still avoid colorful decoration.
Filled shapes must not make disabled states look too heavy.
```

Likely strengths:

```text
good small-size readability
strong visual separation for warning/error/success/status icons
may improve recognition on lower-DPI displays
```

Likely risks:

```text
can feel visually heavy in the current compact toolbar
may fight existing Shell button styles if every icon has dense fill
harder to theme cleanly if the concept relies on multiple fills
more likely to require hand simplification before WPF vector implementation
```

image2 prompt:

```text
Create a clean icon concept sheet for a desktop IDE application called RA2IniEditor.IDE.
Direction: Filled Minimal.
Style: modern professional IDE icons using simple filled silhouettes, high contrast, minimal shapes, vector-like symbols, readable at 16x16.
Use a strict grid layout with evenly spaced icons, no text labels, no captions, no letters, no numbers.
Use a transparent or very light neutral background.
Keep all icons consistent in optical size, corner radius, silhouette density, and perspective.
Use mostly monochrome neutral filled icons. Semantic status icons may use small warning/error/success accent colors, but normal command icons should remain neutral.
Do not use realistic 3D, cartoon style, glossy effects, skeuomorphic rendering, decorative Red Alert unit art, gradients, or colorful app icons.
Include icon concepts for:
Open Project Folder, Save, Search, Issues, Field Registry, Project Explorer, AI Assistant,
Undo, Redo, Revert, Edit Mode,
Field Registry Center, Learn Fields, New Field, Edit Field, Import Preview, Rollback, Cleanup, Open Folder, Warning,
Send, Cancel, Copy Message, Copy Code Block, Clear Chat, Model Selector, Markdown, Code Block, Context,
Info, Warning, Error, Success, Pending, Disabled.
Make Revert distinct from Refresh by emphasizing discard/reset of current changes.
Make AI Assistant advisory and chat-like, not file-apply or auto-fix.
Make Field Registry feel like a field library/catalog for an INI editor rather than a generic database server.
```

### 3.3 Direction C: Hybrid Line + Accent

Expected style:

```text
outline command icons
small accent details for status, AI, and diagnostic concepts
monochrome base with restrained semantic color
balanced between professional IDE tone and domain clarity
```

Design expectations:

```text
Most toolbar icons should remain outline/monochrome.
Issues, Warning, Error, Success, and AI Assistant may carry small accent marks.
Accent marks should be badge-like or detail-level, not full-icon color fills.
Dark theme compatibility must remain plausible through theme brush mapping.
```

Likely strengths:

```text
more distinctive than pure monoline
can clarify Field Registry / Issues / AI Assistant without making all icons colorful
good candidate if Direction A feels too generic
```

Likely risks:

```text
accent color can become inconsistent or too decorative
more complex WPF resource strategy because some icons need semantic brush parts
dark theme and disabled state behavior need stricter testing
AI accent may imply live/provider status if not carefully designed
```

image2 prompt:

```text
Create a clean icon concept sheet for a desktop IDE application called RA2IniEditor.IDE.
Direction: Hybrid Line + Accent.
Style: modern minimalist professional IDE toolbar icons, outline-first, neutral monochrome base, with small restrained semantic accent details only where helpful.
Use a strict grid layout with evenly spaced icons, no text labels, no captions, no letters, no numbers.
Use a transparent or very light neutral background.
Keep all icons consistent in stroke width, corner radius, optical size, and perspective.
Normal command icons should be mostly monochrome outline icons. Warning, Error, Success, Issues, and AI Assistant may use tiny accent badges or small semantic details.
Do not use realistic 3D, cartoon style, glossy effects, skeuomorphic rendering, decorative Red Alert unit art, gradients, or colorful app-style icons.
Include icon concepts for:
Open Project Folder, Save, Search, Issues, Field Registry, Project Explorer, AI Assistant,
Undo, Redo, Revert, Edit Mode,
Field Registry Center, Learn Fields, New Field, Edit Field, Import Preview, Rollback, Cleanup, Open Folder, Warning,
Send, Cancel, Copy Message, Copy Code Block, Clear Chat, Model Selector, Markdown, Code Block, Context,
Info, Warning, Error, Success, Pending, Disabled.
Make Revert distinct from Refresh by emphasizing discard/reset of current changes.
Make AI Assistant advisory and chat-like, not file-apply, insert, save, or auto-fix.
Make Field Registry feel like a field library/catalog for an INI editor, with a subtle catalog/database/book metaphor.
Keep accent use restrained enough that icons can later be converted into WPF vector resources with theme-bound brushes.
```

## 4. Generated Concept Assets

No concept images have been generated in this documentation step.

| Direction | Asset Path | Notes | Strengths | Risks |
|---|---|---|---|---|
| Direction A: Monoline IDE | Pending user-provided or future image2 output | Prompt prepared only. | Best alignment with style guide and WPF vector conversion. | May be too generic without careful Field Registry / AI treatment. |
| Direction B: Filled Minimal | Pending user-provided or future image2 output | Prompt prepared only. | Strong small-size readability. | Could be visually heavy and harder to theme. |
| Direction C: Hybrid Line + Accent | Pending user-provided or future image2 output | Prompt prepared only. | Better semantic distinction while staying IDE-like. | More resource complexity and dark-theme testing risk. |

If the user later provides generated files, record them here with relative paths and review notes. Do not add runtime resource references in this document.

## 5. Review Checklist

Use this checklist when concept sheets are available:

```text
Readability:
  Can each icon be identified at 16x16 without text labels?
  Are tiny details avoided?

Consistency:
  Do stroke width, fill density, corner radius, and optical size match?
  Does the sheet feel like one icon family?

Semantic clarity:
  Is Save distinct from Import / Apply-like actions?
  Is Revert distinct from Refresh / Undo?
  Is Field Registry clearly a field library/catalog concept?
  Is Issues clearly diagnostics/alerts?
  Is AI Assistant advisory/chat-like rather than Apply/Insert/auto-fix?

Theme readiness:
  Can the icons work with theme-bound brushes?
  Are normal commands mostly monochrome?
  Can warning/error/success accents map to semantic tokens?
  Would disabled state remain visible?

WPF feasibility:
  Can each icon be represented with simple Path / Geometry / DrawingImage resources?
  Are there too many layered colors or bitmap-only effects?
  Would the icon stay crisp at 100%, 125%, and 150% DPI?

Product fit:
  Does the direction feel like an INI-focused IDE?
  Does it avoid legacy table-editor feeling?
  Does it avoid decorative RA2 art inside toolbar controls?
```

## 6. Recommended Direction

No final direction is selected yet because no concept sheets have been generated or reviewed.

Initial recommendation before visual generation:

```text
Start with Direction A: Monoline IDE as the baseline.
Generate Direction C: Hybrid Line + Accent as the likely alternative if Field Registry / Issues / AI need more distinction.
Generate Direction B only to test small-size readability and status-icon contrast; it is less likely to be the final main-toolbar style.
```

Decision rule:

```text
Choose Direction A if the main toolbar icons are readable and Field Registry / AI remain clear.
Choose Direction C if Direction A is too generic but accent use stays restrained.
Avoid Direction B for the main toolbar unless readability gains clearly outweigh visual weight.
```

## 7. Follow-Up Implementation Risks

### 7.1 Runtime resource risk

Concept sheets may produce appealing PNG-like output that is not practical as WPF vector geometry.

Mitigation:

```text
Do not use generated bitmap sheets as runtime assets.
Use concepts only as visual references.
Convert selected icons manually into simple WPF vector resources in a later Icon-4 phase.
```

### 7.2 Theme risk

Concepts may rely on fixed black/white or colored fills.

Mitigation:

```text
Icon-3 must define exact brush tokens and theme binding rules before implementation.
Icon resources should use theme-bound brushes.
```

### 7.3 Toolbar behavior risk

Icon replacement can accidentally alter command layout, AutomationIds, or handlers.

Mitigation:

```text
Icon replacement must preserve current AutomationIds, ToolTips, AutomationProperties.Name, click handlers, and command semantics.
Boundary tests must assert no legacy IDs are restored.
```

### 7.4 Semantic risk

AI or Revert icons may imply actions that are not implemented.

Mitigation:

```text
AI Assistant icons must not imply Apply, Insert, Save, or automatic file mutation.
Revert must not look like Refresh.
Save must not look like Field Registry import/apply.
```

### 7.5 Scope risk

Icon work can drift into Shell redesign.

Mitigation:

```text
Do not change toolbar command set, menu entries, right tool well, AI behavior, Field Registry behavior, parser, diagnostics, completion, hover, quick peek, save preflight, or editor text behavior as part of icon work.
```

## 8. Next Implementation Step

Recommended next phase:

```text
Icon-2R: Generate/review concept sheets and select one direction.
```

After selecting a direction:

```text
Icon-3: WPF Vector Icon Resource Contract
  Define exact ResourceDictionary location.
  Define resource keys and compatibility with existing Icon* keys.
  Define icon brush tokens.
  Define boundary tests.
  Stop for approval before implementation.
```

No source, XAML, runtime resource, or command behavior change should occur before Icon-3 / Icon-4 approval.
