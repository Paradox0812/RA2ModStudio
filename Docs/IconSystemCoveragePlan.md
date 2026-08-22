# RA2IniEditor.IDE Icon System Coverage Plan

## 1. Scope

This document expands RA2IniEditor.IDE icon planning from main toolbar replacement to a full IDE icon coverage plan.

This is a planning document only:

```text
no source changes
no XAML changes
no ViewModel changes
no tests changed
no runtime resources added
no SVG / PNG / DrawingImage resources added
no project or solution file changes
no command behavior changes
no parser / diagnostics / Field Registry semantic changes
```

Primary references:

```text
Docs/IconToolbarInventory.md
Docs/IconToolbarCommandContract.md
Docs/IconStyleGuide.md
Docs/IconConceptReview.md
```

The requested `RA2IniEditor_IconScopePriorityPlan.md` was not present in the repository at the time this document was created. This plan therefore uses the existing icon documents plus the current requested coverage categories as the source of truth.

## 2. Global Icon Rules

Use the style direction from `Docs/IconStyleGuide.md`:

```text
modern desktop IDE
monoline / outline-first as the baseline
16x16 readability for dense UI
theme-bound brushes
low visual noise
semantic naming
WPF vector resources in later implementation phases
```

Global accessibility rules:

```text
Icon-only controls must keep AutomationProperties.Name and ToolTip.
Do not replace visible text with icon-only controls when the command is destructive, ambiguous, rare, or workflow-critical.
Do not encode command meaning only through color.
Disabled state must remain visible in light and future dark themes.
Preserve existing AutomationIds unless a later phase explicitly updates tests and automation harnesses.
Do not restore legacy toolbar IDs.
```

Priority levels:

| Priority | Meaning |
|---|---|
| P0 | Needed for first production-quality icon pass or high-value readability. |
| P1 | Important for secondary surfaces after main toolbar and tree basics. |
| P2 | Nice-to-have polish or later domain-specific expansion. |

## 3. Main Toolbar Icons

Main toolbar icons are the first runtime icon replacement target because they are visible on every IDE session and currently use letter placeholders.

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| Open Project Folder | Main Shell toolbar, File menu concept | folder-open, optional project marker | P0 | Yes | No on toolbar; menu keeps text | Normal icon brush | Preserve `Shell.MainToolbar.OpenFolderButton`, ToolTip, Name |
| Save Current File | Main Shell toolbar | save / document-save | P0 | Yes | No on toolbar; menu keeps text | Disabled state must be obvious | Preserve `Shell.SourceEditor.SaveCurrentFileButton` |
| Search | Main Shell toolbar | magnifier | P0 | Yes | No on toolbar; menu keeps text | Normal / hover brush | Preserve `Shell.MainToolbar.SearchButton` |
| Issues | Main Shell toolbar, Issues menu concept | warning list / diagnostics list | P0 | Yes | No on toolbar; menu keeps text | May use warning accent, not full-color noise | Preserve `Shell.MainToolbar.IssuesButton` |
| Field Registry | Main Shell toolbar | field library / catalog / database-book | P0 | Yes | No on toolbar; menu keeps text | Normal icon with optional subtle catalog accent | Preserve `Shell.MainToolbar.FieldRegistryButton` |
| Project Explorer | Main Shell toolbar | side panel / project tree | P1 | Yes | No on toolbar; menu keeps text | Muted normal icon acceptable | Preserve `Shell.MainToolbar.ProjectExplorerButton` if retained |
| Undo | Main Shell toolbar | left curved arrow | P0 | Yes | No on toolbar; menu keeps text | Disabled state common | Preserve `Shell.SourceEditor.UndoButton` |
| Redo | Main Shell toolbar | right curved arrow | P0 | Yes | No on toolbar; menu keeps text | Disabled state common | Preserve `Shell.SourceEditor.RedoButton` |
| Revert | Main Shell toolbar | discard changes / reset document | P0 | Yes, with clear tooltip | Menu keeps text; toolbar no text | Use destructive/muted treatment carefully; not refresh | Preserve `Shell.SourceEditor.RevertInMemoryChangesButton` |
| Edit Mode | Collapsed toolbar entry | pencil / edit | P2 | Yes if ever shown | May need text if exposed in workflow | Muted contextual icon | Preserve `Shell.SourceEditor.EnterEditModeButton` |
| AI Assistant, future | Not currently in main toolbar | assistant chat / spark | P2 | Yes if approved later | Tooltip required | Accent only if restrained | Use future `Shell.MainToolbar.AiAssistantButton`; do not reuse AI panel IDs |

Implementation note:

```text
Main toolbar icon replacement should happen only after Icon-3 defines resource keys, brush tokens, and boundary tests.
```

## 4. Field Registry Secondary / Tertiary Panel Icons

Field Registry surfaces are a major RA2IniEditor.IDE workflow. Icons here should support scanning, source priority, write-risk boundaries, and import/rollback tasks without changing semantics.

### 4.1 Field Registry Center

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| Center / Registry Overview | Window header / entry card | catalog / field library | P1 | No | Yes | Normal brush | Header icon decorative unless actionable |
| Open Manager | Center action | tool window / sliders / catalog | P1 | Optional | Yes | Normal / hover | Button text should remain |
| Project Source | Source priority strip | folder + field marker | P1 | No | Yes | Project color token optional | Tooltip should explain Project source |
| Global Source | Source priority strip | globe / user library | P1 | No | Yes | Muted or accent source token | Tooltip should explain Global source |
| BuiltIn Source | Source priority strip | box / book / shield | P1 | No | Yes | Muted stable token | Tooltip should explain BuiltIn fallback |
| Warning Summary | Warning card | warning triangle / alert list | P1 | No | Yes | Warning token | Do not rely on color only |

### 4.2 Field Registry Manager / Advanced Tools

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| Refresh / Reload Registry | Manager command | reload arrows + library | P1 | Optional | Yes | Normal brush | Keep command text due possible side effects |
| Active Pack | Active pack list | document stack / package | P1 | No | Yes | Source token may vary | Tooltip for scope/path |
| Backup / Manifest | Rollback manifest grid | clock / archive box | P1 | No | Yes | Muted normal | Preserve row text |
| Rollback Selected | Rollback action | rotate-back archive | P1 | Optional | Yes | Destructive token | Must keep text and confirmation wording |
| Cleanup | Cleanup section | broom / cleanup spark | P1 | Optional | Yes | Warning/destructive only on apply | Keep text, especially Apply Cleanup |
| Apply Cleanup | Write/risk action | check + cleanup / guarded apply | P1 | No | Yes | Destructive or warning accent | Never icon-only |
| Advanced Tools | Manager title/entry | wrench / sliders | P2 | No | Yes | Normal | Decorative header only if no command |
| Import Preview | Manager command to import | table + arrow-in | P1 | Optional | Yes | Normal | Text required to avoid Apply confusion |

### 4.3 Field Import Preview

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| Paste / Raw Input | Source area | clipboard / text table | P2 | No | Yes | Muted normal | Text label remains |
| Parse Preview | Parse command | table scan / magic-free parse | P1 | Optional | Yes | Normal | Do not imply AI generation |
| Target Scope | Project / Global target selector | target / scope marker | P1 | No | Yes | Source tokens | Source labels must remain |
| Build Apply Plan | Plan command | checklist / plan document | P1 | Optional | Yes | Normal | Text required |
| Apply Import | Write action | guarded check / import apply | P1 | No | Yes | Warning/destructive accent | Never icon-only; confirmation remains |
| Invalid Row | Preview grid | invalid / x-circle | P1 | No | Yes | Error token | Grid cell text remains |
| Changed Row | Preview grid | delta / pencil | P1 | No | Yes | Accent or warning token | Text still required |
| Added Row | Preview grid | plus / new row | P1 | No | Yes | Success/accent token | Text still required |

### 4.4 Cleanup / Rollback

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| Cleanup Candidate | Cleanup preview | broom / minus document | P1 | No | Yes | Warning token for risky candidates | Keep field/action details visible |
| Rollback Manifest | Rollback list | archive clock | P1 | No | Yes | Muted normal | Tooltip may include path summary |
| Rollback Action | Button / confirmation | rotate-back archive | P1 | No | Yes | Destructive token | Text and confirmation required |
| Backup Created | Status | archive check | P2 | No | Yes | Success token | Status text remains |

### 4.5 Learning Wizard

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| Source Step | Workflow step strip | document / current INI | P1 | No | Yes | Muted step token | Step text remains |
| Parse Step | Workflow step strip | table scan | P1 | No | Yes | Muted / active step token | Step text remains |
| Target / Mode Step | Workflow step strip | target / scope | P1 | No | Yes | Source token possible | Step text remains |
| Review Step | Workflow step strip | checklist | P1 | No | Yes | Normal | Step text remains |
| Apply Plan Step | Workflow step strip | plan document | P1 | No | Yes | Normal | Step text remains |
| Apply Step | Workflow step strip / action | guarded check | P1 | No | Yes | Warning/destructive when writing | Never icon-only |
| Learned Field | Review list | lightbulb / plus field | P2 | No | Yes | Accent optional | Text remains authoritative |

### 4.6 Field Editor / Allowed Values Editor

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| New Field | Field Editor mode/header | plus field | P1 | No | Yes | Accent normal | Header text remains |
| Edit Field | Field Editor mode/header | pencil field | P1 | No | Yes | Normal | Header text remains |
| Required / Validation | Validation row | warning / error marker | P1 | No | Yes | Warning/error token | Text required |
| Allowed Value | Allowed values list | list item / tag | P2 | No | Yes | Muted normal | Value text remains |
| Add Allowed Value | Editor command | plus tag | P2 | Optional | Yes | Accent normal | Button text remains |
| Remove Allowed Value | Editor command | minus tag / x | P2 | Optional | Yes | Destructive muted | Button text remains |

## 5. Section Tree / Project Explorer Node Type Icons

Project Explorer / Section Tree icons should improve scan speed but must not replace section names. The icon is a classifier hint, not the source of truth.

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|---|
| File | Project Explorer file node | INI document | P0 | No | Yes | Normal/muted | File name text remains |
| rulesmd.ini | File node subtype | rules document / gear document | P1 | No | Yes | Subtle accent optional | Tooltip can show type |
| artmd.ini | File node subtype | art document / image document | P1 | No | Yes | Subtle accent optional | Tooltip can show type |
| ai.ini / aimd.ini | File node subtype | AI script document | P2 | No | Yes | Accent optional | Text remains |
| Infantry Section | Section node | small soldier / unit marker | P0 | No | Yes | Neutral; avoid detailed person art | Section ID remains |
| Vehicle Section | Section node | tank / vehicle silhouette | P0 | No | Yes | Neutral | Section ID remains |
| Aircraft Section | Section node | aircraft silhouette | P0 | No | Yes | Neutral | Section ID remains |
| Building Section | Section node | structure / building | P0 | No | Yes | Neutral | Section ID remains |
| Weapon Section | Section node | projectile/weapon line | P0 | No | Yes | Neutral | Section ID remains |
| Warhead Section | Section node | burst / impact marker | P0 | No | Yes | Warning-like only if not confused with error | Section ID remains |
| Projectile Section | Section node | arrow / missile path | P0 | No | Yes | Neutral | Section ID remains |
| Generic Section | Section node | bracketed section / tag | P0 | No | Yes | Muted normal | Section ID remains |
| Current File | Explorer/current marker | dot / focus ring | P1 | No | Yes | Accent current token | Must not replace selection state |
| Current Section | Explorer/current marker | small caret / target | P1 | No | Yes | Accent current token | Must not conflict with selection |
| Dirty File | File node state | small dot / unsaved marker | P1 | No | Yes | Warning/accent | Must not rely only on color |

Classifier caveat:

```text
If section-kind inference is uncertain, use Generic Section. Do not invent object type certainty from naming alone when the parser/model cannot support it.
```

## 6. File Type Icons

File icons should distinguish high-frequency RA2 mod files while preserving file names and extensions.

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|
| Generic INI | Project Explorer, tabs | document with INI mark | P0 | No | Yes | Normal | File name remains |
| rules / rulesmd | Project Explorer, tabs | document + gear/list | P1 | No | Yes | Subtle accent | Tooltip can show role |
| art / artmd | Project Explorer, tabs | document + image/brush | P1 | No | Yes | Subtle accent | Tooltip can show role |
| sound / theme | Project Explorer | document + note | P2 | No | Yes | Muted normal | Text remains |
| AI script | Project Explorer | document + assistant/logic mark | P2 | No | Yes | Accent optional | Avoid implying AI Assistant provider |
| Unknown file | Project Explorer | blank document | P1 | No | Yes | Muted | Text remains |
| Folder | Project Explorer | folder | P0 | No | Yes | Normal | Folder name remains |

## 7. Faction / House / Side Icons

Faction icons should support recognition in future field displays, AI draft summaries, or section metadata, but should not replace text because House / Side values can be mod-defined.

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|
| Allied | Metadata chips, possible field/value displays | star / eagle-like shield / blue accent | P1 | No | Yes | Accent must theme safely; avoid national flags | Text `Allied` / house value remains |
| Soviet | Metadata chips, possible field/value displays | hammer-like abstract mark / red accent | P1 | No | Yes | Avoid political/detail-heavy symbols; red token accessible | Text remains |
| Yuri | Metadata chips, possible field/value displays | psi / swirl / purple accent | P1 | No | Yes | Purple accent restrained | Text remains |
| Neutral | Metadata chips, possible field/value displays | circle / balance / gray marker | P1 | No | Yes | Muted neutral | Text remains |
| Civilian | Metadata chips | house / person-neutral marker | P2 | No | Yes | Muted neutral | Text remains |
| Custom House | Metadata chips | tag / custom marker | P2 | No | Yes | Accent optional | Text is authoritative |
| Side Unknown | Metadata chips | question / generic side | P2 | No | Yes | Muted | Text remains |

Rules:

```text
Do not hard-code faction meaning into parser or Field Registry behavior as part of icon work.
Do not use detailed flags or real-world political symbols.
Use icons as decorative/assistive hints beside text.
```

## 8. Inline Action Icons

Inline action icons are compact actions embedded in panels, cards, chat messages, and code blocks. They must have strong tooltips because pure icon meaning is weaker in dense UI.

### 8.1 AI Assistant Inline Actions

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|
| Send | Composer send button | arrow / paper plane | P0 | Yes | No if button has Name/ToolTip | Accent on enabled primary action | Preserve `AiAssistant.GenerateButton`; tooltip/name required |
| Cancel | Composer busy state | square stop / x | P0 | Yes | No if clear | Warning/destructive muted | Preserve cancel AutomationId if present |
| Copy Message | Assistant message card | copy / overlapping rectangles | P0 | Yes | No | Normal/muted | Each assistant message copy action needs distinct accessible Name |
| Copy Code Block | Code card action | code brackets + copy | P0 | Yes | No | Normal/muted | Must copy only code content |
| Clear Chat | Chat actions | trash / clear list | P1 | Optional | Prefer text or icon+text in action area | Destructive muted | Tooltip/name; no editor mutation |
| Model Selector | Advanced model combo | model / chip / dropdown | P2 | No | Yes | Muted | Text `Mock`/`DeepSeek` remains |
| Markdown | Rendered response indicator, if needed | markdown mark / text lines | P2 | No | Yes or hidden decorative | Muted | Not required for current UI |
| Code Block | Code card header | brackets / terminal | P1 | No | Language text remains | Muted | Code language text remains |
| Context | Context summary | target / document context | P2 | No | Text remains | Muted/accent current | Do not reveal hidden context |

AI-specific guardrails:

```text
No AI icon should imply Apply, Insert, Save, or automatic file mutation.
Copy icons must not imply copying context, raw prompts, provider data, or API information.
Send icon state must follow existing provider/busy behavior.
```

### 8.2 General Inline Actions

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|
| Close | Custom chrome / dismiss chips | x | P1 | Yes | No | Normal/hover | ToolTip/name required |
| Expand / Collapse | Expanders, advanced sections | chevron | P1 | Yes | Text remains in header | Muted | Keep current Expander semantics |
| Browse Folder | Path controls | folder | P1 | Optional | Yes where command is not obvious | Normal | Text/tooltip |
| Refresh | Refresh commands | circular arrows | P1 | Optional | Yes | Normal | Must remain distinct from Revert |
| Filter | Search/filter panels | funnel | P2 | Optional | Text often remains | Muted | Accessible name |

## 9. Status / Source / Difference Icons

These icons should primarily augment text in grids, summaries, and status areas.

| Icon | Usage location | Suggested semantic | Priority | Pure icon? | Keep text? | Theme notes | Automation / accessibility notes |
|---|---|---|---|---|---|---|
| Added | Import preview, diff summaries | plus circle / new row | P0 | No | Yes | Success/accent | Text remains |
| Changed | Import preview, diff summaries | delta / pencil / modified row | P0 | No | Yes | Accent/warning | Text remains |
| Removed | Cleanup/diff summaries | minus circle / removed row | P1 | No | Yes | Warning/destructive | Text remains |
| Invalid | Import/validation grids | x circle / invalid marker | P0 | No | Yes | Error token | Text remains |
| Warning | Diagnostics, Field Registry warnings | warning triangle | P0 | Sometimes | Usually yes | Warning token | Do not rely only on color |
| Error | Diagnostics, validation | error octagon / x circle | P0 | Sometimes | Usually yes | Error token | Text remains |
| Success | Apply/save/status | check circle | P0 | Sometimes | Usually yes | Success token | Text remains |
| Info | Status / empty states | info circle | P1 | Sometimes | Usually yes | Info/muted token | Text remains |
| Pending | Async/status | clock / spinner-like static mark | P1 | Sometimes | Usually yes | Muted/accent | Avoid animation in first pass |
| Disabled | Disabled reason | slash circle / muted marker | P2 | No | Yes | Disabled brush | Text required |
| Project Source | Field Registry source | folder/project marker | P0 | No | Yes | Source project token | Text remains |
| Global Source | Field Registry source | globe/user library | P0 | No | Yes | Source global token | Text remains |
| BuiltIn Source | Field Registry source | book/box/shield | P0 | No | Yes | Source built-in token | Text remains |
| Advisory | AI evidence/diagnostics summary | info / lightbulb muted | P2 | No | Yes | Muted | Must not imply authority |
| Draft | AI generated draft | document pencil / draft mark | P2 | No | Yes | Muted/accent | Must not imply written file state |

## 10. Coverage Priority Summary

### 10.1 P0

```text
Main toolbar: Open, Save, Search, Issues, Field Registry, Undo, Redo, Revert
Project Explorer: File, Generic INI, Infantry, Vehicle, Aircraft, Building, Weapon, Warhead, Projectile, Generic Section
AI inline: Send, Cancel, Copy Message, Copy Code Block
Status/diff: Added, Changed, Invalid, Warning, Error, Success, Project, Global, BuiltIn
```

### 10.2 P1

```text
Main toolbar: Project Explorer
Field Registry Center / Manager / Import Preview / Cleanup / Rollback / Learning Wizard / Field Editor core icons
File types: rulesmd.ini, artmd.ini
Faction: Allied, Soviet, Yuri, Neutral
General inline: Close, Expand/Collapse, Browse Folder, Refresh
Status: Info, Pending, Removed
```

### 10.3 P2

```text
AI Assistant main toolbar candidate
Edit Mode if exposed
AI panel secondary icons: Model Selector, Markdown, Context
File types: AI script, sound/theme, unknown variants
Faction: Civilian, Custom House, Unknown Side
Secondary Field Registry polish icons
Disabled / Advisory / Draft status refinements
```

## 11. Pure Icon vs Text Rules

Safe pure-icon contexts:

```text
main toolbar commands with stable ToolTips and AutomationProperties.Name
standard inline copy / close / expand actions
AI Send / Cancel where state is visually clear and accessible name exists
```

Must retain text:

```text
Field Registry write actions
Apply Import / Apply Cleanup / Rollback
Learning Wizard workflow steps
Field Editor validation fields
Project Explorer node labels
file names
section IDs
House / Side values
status and diff rows in grids
destructive or rare commands
```

Use icon + text when:

```text
the command is destructive
the command writes files
the command changes registry packs
the icon could be confused with another action
the surface is a wizard, confirmation, or review table
```

## 12. Theme Compatibility Notes

Future icon resources should support:

```text
light theme foreground / hover / disabled states
future dark theme through theme-bound brushes
semantic warning / error / success tokens
source tokens for Project / Global / BuiltIn
accent token for current selection or AI only when restrained
```

Avoid:

```text
fixed black path strokes
fixed white fills
full-color decorative icon families
tiny color-only badges with no shape distinction
low-contrast disabled states
bitmap icons that blur at high DPI
```

Minimum manual theme checks in later implementation:

```text
100%, 125%, and 150% DPI
enabled, hover, pressed, disabled states
light theme contrast
dark theme compatibility if a dark theme exists or is introduced
warning/error/success shape distinction without color
```

## 13. AutomationId / Tooltip / Accessibility Notes

Implementation phases must preserve current AutomationIds unless explicitly contracted.

Current high-sensitivity IDs include:

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

Rules:

```text
Do not restore Shell.FieldRegistryButton.
Do not introduce Shell.MainToolbar.SaveButton unless a later migration contract explicitly approves it.
Icon resources must not encode AutomationIds.
Icon-only buttons need AutomationProperties.Name and ToolTip.
Repeated inline actions, such as per-message Copy, need accessible names that distinguish message or scope where practical.
Tree node icons must not replace node names.
Status icons in grids must not replace text values.
```

## 14. Recommended Implementation Stages

### Icon-S0: System Coverage Plan

```text
Create this document only.
No runtime changes.
```

### Icon-S1: Icon Resource Contract

Define:

```text
ResourceDictionary location
brush token names
existing Icon* compatibility
P0 icon list
theme behavior
test strategy
manual smoke checklist
```

Stop for approval before implementation.

### Icon-S2: P0 Main Toolbar + Status Vector Resources

Implement only:

```text
main toolbar P0 icons
basic status/diff icons
source icons: Project / Global / BuiltIn
```

Preserve:

```text
AutomationIds
handlers
ToolTips
button layout
command behavior
```

### Icon-S3: Section Tree / File Type Icons

Implement:

```text
File / folder / INI type icons
Infantry / Vehicle / Aircraft / Building / Weapon / Warhead / Projectile / Generic Section
Current file / current section / dirty markers if approved
```

Requires a separate UI contract because it touches Project Explorer / Section Tree display.

### Icon-S4: Field Registry Surface Icons

Implement:

```text
Field Registry Center / Manager
Import Preview
Cleanup / Rollback
Learning Wizard
Field Editor
Allowed Values Editor
```

Must preserve Field Registry semantics and write confirmations.

### Icon-S5: AI Assistant Inline Icons

Implement:

```text
Send / Cancel
Copy Message / Copy Code
Clear Chat
Code Block / Context / Model selector hints if approved
```

Must preserve no Apply / Insert / file mutation semantics.

### Icon-S6: Faction / House / Side Icons

Implement only after metadata display locations are contracted:

```text
Allied / Soviet / Yuri / Neutral
Civilian / Custom / Unknown
```

Icons remain adjacent to text values and do not alter parser or Field Registry behavior.

### Icon-S7: Secondary Polish Pass

Implement P2 icons and consistency fixes after screenshot/manual review.

## 15. Non-Goals

This plan does not:

```text
generate icon images
add runtime icon resources
modify XAML
modify code-behind
modify ViewModels
modify tests
modify project files
change command behavior
change Field Registry semantics
change parser / diagnostics / completion / hover / quick peek / save preflight
restore legacy UI
add Apply / Insert behavior
```
