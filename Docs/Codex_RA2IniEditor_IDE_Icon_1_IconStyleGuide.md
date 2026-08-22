# Codex Task: RA2IniEditor.IDE Icon-1 Icon Style Guide

## 0. Current Baseline

Icon-0C has been completed.

Reported state:

```text
Main toolbar state cleanup completed.
No icon resources replaced.
No Icon* placeholder resources modified.
No SVG / PNG / DrawingImage resources added.
Open Project / Search / Issues / Field Registry / Project Explorer remain accessible.
Save / Undo / Redo / Revert are now state-aware.
Enter Edit Mode remains collapsed.
FieldImportApplySmokeTests old Shell.FieldRegistryButton reference was updated to Shell.MainToolbar.FieldRegistryButton.
Tests: 1433 passed.
IdeOnly package: passed, packaged file count 770.
```

Next phase:

```text
Icon-1: Icon Style Guide
```

This is a documentation / design-system phase.

Do not implement icon resources yet.

---

## 1. Goal

Create a formal icon style guide for RA2IniEditor.IDE before generating image2 concept sheets or replacing placeholder toolbar icons.

The guide should define:

```text
1. Visual style.
2. Icon sizes.
3. Stroke / fill rules.
4. Color tokens.
5. Light / dark theme behavior.
6. Naming conventions.
7. WPF resource strategy.
8. image2 concept generation prompt rules.
9. Acceptance criteria for future icon assets.
```

---

## 2. Hard Boundaries

Do not modify:

```text
XAML
code-behind
ViewModels
tests
project files
solution files
ShellTheme.xaml icon resources
Field Registry JSON
legacy files
```

Do not:

```text
generate icons
replace toolbar placeholders
add SVG/PNG/DrawingImage resources
change command handlers
change toolbar visibility/state
change menu entries
```

This phase only creates or updates documentation.

---

## 3. Required Input Documents / Files

Read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/IconToolbarInventory.md
Docs/IconToolbarCommandContract.md
```

Optionally inspect current UI screenshots if available, but do not modify UI.

---

## 4. Required Output

Create:

```text
Docs/IconStyleGuide.md
```

Suggested structure:

```markdown
# RA2IniEditor.IDE Icon Style Guide

## 1. Scope and Goals

## 2. Visual Direction

## 3. Icon Sizes

## 4. Stroke and Geometry Rules

## 5. Color Tokens and Theme Behavior

## 6. Icon Categories

## 7. Naming Conventions

## 8. WPF Resource Strategy

## 9. image2 Concept Prompt Rules

## 10. Icon Acceptance Criteria

## 11. Future Implementation Plan
```

---

## 5. Recommended Visual Direction

Use an IDE-oriented minimalist icon style:

```text
monoline / outline-first
modern desktop IDE look
high readability at 16x16
low visual noise
consistent geometry
no realistic/3D style
no colorful app-like cartoon icons
```

Recommended base style:

```text
1. 16x16 primary toolbar icons.
2. 20x20 for tool windows / secondary actions.
3. 24x24 only for large empty-state illustrations or dialogs.
4. 2 px visual stroke at 24x24 equivalent, scaled carefully for 16x16.
5. Rounded corners where appropriate.
6. Avoid excessive detail inside 16x16 icons.
```

---

## 6. Color Strategy

Use theme resources, not hard-coded colors.

Define conceptual tokens:

```text
IconBrush.Normal
IconBrush.Muted
IconBrush.Hover
IconBrush.Disabled
IconBrush.Warning
IconBrush.Error
IconBrush.Success
IconBrush.Accent
```

Guidance:

```text
1. Default icons should be monochrome.
2. Warning / Error / Success may use semantic colors.
3. AI Assistant icon may use Accent only if it does not clash with IDE chrome.
4. Do not embed fixed black paths that break dark theme.
5. Use DynamicResource where feasible in later implementation.
```

---

## 7. Icon Categories

Document required icon categories.

### 7.1 Main Toolbar

```text
Open Project Folder
Save Current File
Search
Issues
Field Registry
Project Explorer
AI Assistant, if promoted to toolbar later
```

### 7.2 Contextual Editing

```text
Undo
Redo
Revert
Edit Mode
```

These are contextual and should not dominate primary toolbar.

### 7.3 Field Registry

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

### 7.4 AI Assistant

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

### 7.5 Status

```text
Info
Warning
Error
Success
Pending
Disabled
```

---

## 8. Naming Conventions

Use stable semantic names, not visual descriptions.

Recommended names:

```text
Icon.OpenFolder
Icon.Save
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

For WPF resource keys, adapt to current project style:

```text
IconOpenFolder
IconSave
IconSearch
IconIssues
IconFieldRegistry
IconProjectExplorer
IconAiAssistant
IconSend
IconCopy
IconCodeBlock
```

Do not name icons after current placeholder letters.

---

## 9. WPF Resource Strategy

Preferred final resource strategy:

```text
1. XAML vector resources.
2. DrawingImage / GeometryDrawing / PathGeometry.
3. ResourceDictionary such as IconResources.xaml or existing ShellTheme icon section.
4. Brushes bound to theme resources.
```

Avoid:

```text
1. Direct PNG toolbar icons.
2. Whole image2 sheet sliced into runtime assets.
3. Hard-coded colors.
4. Icon resources scattered across many windows.
```

image2 concept art should be treated as visual reference, not final asset source.

---

## 10. image2 Concept Prompt Rules

The guide should include a reusable prompt template for image2 concept exploration.

Prompt should request:

```text
1. icon sheet
2. modern IDE style
3. no labels
4. monochrome / outline-first
5. transparent or light neutral background
6. consistent stroke width
7. grid layout
8. include required icon list
```

Generate 2-3 style directions later:

```text
A. Monoline IDE
B. Filled minimal
C. Hybrid line + accent
```

Do not generate image2 output in Icon-1.

---

## 11. Acceptance Criteria

IconStyleGuide is accepted when it defines:

```text
1. final visual direction
2. size/stroke rules
3. color/theme rules
4. naming rules
5. WPF vector resource strategy
6. image2 concept prompt rules
7. future implementation sequence
```

No source or XAML changes should occur in Icon-1.

---

## 12. Validation Commands

Documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing, run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 13. Final Report Format

Report:

```text
1. Phase completed: Icon-1.
2. Files changed.
3. Visual direction summary.
4. Size/stroke/color rules.
5. Naming/resource strategy.
6. image2 concept prompt rules.
7. Commands run.
8. Test/package result.
9. Confirmation no source/XAML behavior changed.
10. Recommended next phase.
```
