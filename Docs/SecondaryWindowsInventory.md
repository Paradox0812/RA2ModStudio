# Secondary Windows Inventory

## 1. Scope and Baseline

Baseline: `v0.4.96-pre.2 IDE-only Source Package Stabilization` plus Phase 0 `Docs/HandoffArchiveIndex.md`.

This inventory maps current RA2IniEditor.IDE secondary/tertiary windows, dialogs, popups, context menus, and tool-like surfaces. It is documentation only and does not change UI behavior.

Current package boundary:

- Active product: RA2IniEditor.IDE-only
- Build entry: `RA2IniEditor.IDE.sln`
- Clean package profile: `IdeOnly`
- Legacy table-style editor: intentionally absent and not a source of active requirements

Search basis:

- `RA2IniEditor.IDE/**/*.xaml`
- `RA2IniEditor.IDE/**/*.xaml.cs`
- `RA2IniEditor.IDE/**/*.cs`

## 2. Summary Table

| Surface | Type | Entry Point | Current Role | Future Classification | Risk | Phase |
| --- | --- | --- | --- | --- | --- | --- |
| ShellWindow main shell | main window | App startup | Primary IDE shell | keep as shell, no Phase 1 behavior change | High | Future shell pass |
| Main menu and toolbar | embedded command surface | ShellWindow | Global commands and window launchers | keep embedded panel | Medium | Phase 1 reference |
| Project Explorer / Navigator | embedded tool area | ShellWindow left pane | File/section navigation | keep embedded panel | Medium | Phase 4 or shell pass |
| Bottom Issues tab | docked panel | ShellWindow bottom tabs | Inline diagnostics | dockable tool window | Medium | Phase 3/5 |
| Bottom Output tab | docked panel | ShellWindow bottom tabs | Operation log/status | dockable tool window | Low | Phase 5 |
| Bottom Search Results tab | docked panel | ShellWindow bottom tabs | Inline search result area | dockable tool window | Medium | Phase 4/5 |
| Source Editor context menu | context menu | Source Editor right click | Navigation, peek, find references, completion, add property | keep context menu | Medium | Phase 2/4 |
| Completion dropdown | popup | Typing / completion command | Inline completion list | lightweight popup/card | Medium | Phase 2 |
| Source hover card | popup | Mouse hover over source text | Field/reference quick info | lightweight info popup/card | Medium | Phase 2 |
| Dirty navigation dialog | modal dialog | Leaving dirty file/project | Save/discard/cancel decision | modal confirmation | High | Phase 5 |
| Save preflight dialog | modal dialog | Save with preflight issues | Continue/cancel risky save | modal confirmation | High | Phase 5 |
| Open project folder dialog | system dialog | File > Open project / toolbar | Select project folder | modal system dialog | Low | No redesign |
| Search tool window | non-modal window | Search menu / toolbar | Search query and results | dockable tool window | Medium | Phase 4/5 |
| Issues tool window | non-modal window | Shell code path, currently secondary to bottom issues | Detached issues window | dockable tool window | Medium | Phase 3/5 |
| Peek definition window | non-modal window | Go to definition / peek definition | Read-only definition detail | lightweight info popup/card | Medium | Phase 2 |
| Field quick peek window | non-modal window | Source context menu > field details | Read-only field detail | lightweight info popup/card | Medium | Phase 2 |
| Find references window | non-modal window | Search menu / source context menu | Reference results | dockable tool window | Medium | Phase 4 |
| Add property window | modal dialog | Edit menu / source context menu | Select field and insert/replace property | workflow dialog | High | Phase 3/5 |
| Field annotation editor window | modal dialog | Add property > edit annotation | Edit local field annotation metadata | workflow dialog | Medium | Phase 3 |
| Field Registry Center window | non-modal window | Field registry menu / toolbar | Main field registry browse/status workspace | dockable tool window | High | Phase 3 |
| Field editor window | non-modal window | Field Registry Center > edit/new field | Edit field definition and save to registry | workflow dialog | High | Phase 3 |
| Field Registry Manager window | non-modal window | Field registry > advanced tools | Advanced registry tools, cleanup, rollback | workflow dialog / dockable tool window | High | Phase 3 |
| Field import preview window | non-modal window | Advanced tools > import preview | Import/preview/apply field registry data | workflow dialog | High | Phase 3 |
| Remote source preset editor | modal dialog | Field import preview > preset edit/new | Edit remote source preset metadata | workflow dialog | Medium | Phase 3 |
| Field learning wizard | non-modal window | Field registry learning commands | Parse current/pasted INI and build apply plan | workflow dialog | High | Phase 3 |
| Allowed values editor | modal dialog | Field learning wizard > allowed values | Edit generated allowed values | workflow dialog | Medium | Phase 3 |
| MessageBox confirmations | modal confirmation | Registry cleanup/apply/rollback/preset actions | Blocking yes/no or warning messages | modal confirmation | Medium | Phase 3/5 |
| Completion preview window | not currently opened | XAML exists, no current construction found | Historical/unused completion preview window | not found in active flow | Low | Verify before removal |
| Reference target details surface | implemented through hover/peek | Hover / peek definition | Reference target context | lightweight info popup/card | Medium | Phase 2 |

## 3. Detailed Inventory

### 3.1 ShellWindow main shell

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: main window
- Entry point: `App.xaml.cs` creates and shows `ShellWindow`.
- Current purpose: Hosts menu, toolbar, Source Editor, Project Explorer, bottom tool tabs, status bar, completion popup, hover popup, and launch points for most secondary surfaces.
- UX concern: It owns many responsibilities and manually creates several popups/windows, making future UX consistency dependent on careful boundary work.
- Recommended classification: keep as shell; do not convert to secondary surface.
- Risk: High
- Recommended migration phase: Future shell pass after Phase 2-4
- Notes: Do not change shell behavior in Phase 1.

### 3.2 Main menu and toolbar

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- Type: embedded tool area
- Entry point: Always visible in ShellWindow.
- Current purpose: Opens project, save/undo/redo/revert, search, field registry, issues, project explorer, completion, navigation, diagnostics, and field registry commands.
- UX concern: Menu entries mix active commands, disabled placeholders, and launchers for multiple window types.
- Recommended classification: keep as embedded panel.
- Risk: Medium
- Recommended migration phase: Phase 1 reference, later shell pass
- Notes: The field registry and search commands are key launch points for later phases.

### 3.3 Project Explorer / Navigator

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- Type: embedded tool area
- Entry point: left pane and View > Project Explorer toggle.
- Current purpose: Displays project/navigation tree and drives Source Editor selection.
- UX concern: It is already embedded, but later navigation sync with reference/search results should be mapped carefully.
- Recommended classification: keep as embedded panel.
- Risk: Medium
- Recommended migration phase: Phase 4 or future shell pass
- Notes: No separate Navigator window was found.

### 3.4 Bottom Issues tab

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- Type: docked panel
- Entry point: View > Issues, Issues menu, toolbar issues button, bottom tab.
- Current purpose: Shows current diagnostics/issues in the shell bottom panel.
- UX concern: There is both an embedded issues tab and an `IssuesToolWindow` class/path, so the future direction should decide whether issues live as docked panel, detached window, or both.
- Recommended classification: dockable tool window.
- Risk: Medium
- Recommended migration phase: Phase 3/5
- Notes: Current bottom grid has refresh, full diagnostics, and clear actions.

### 3.5 Bottom Output tab

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- Type: docked panel
- Entry point: View > Output / bottom tabs.
- Current purpose: Displays output/status log text.
- UX concern: Basic output area; should share dockable-panel language with issues/search.
- Recommended classification: dockable tool window.
- Risk: Low
- Recommended migration phase: Phase 5
- Notes: Not a workflow dialog.

### 3.6 Bottom Search Results tab

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- Type: docked panel
- Entry point: View > Search Results / bottom tabs.
- Current purpose: Shows inline search results in the shell.
- UX concern: There is also a separate `SearchToolWindow`; future UX should avoid duplicate result surfaces.
- Recommended classification: dockable tool window.
- Risk: Medium
- Recommended migration phase: Phase 4/5
- Notes: Search result grouping and jump behavior belong to later UX polish.

### 3.7 Source Editor context menu

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- Type: context menu
- Entry point: right click in AvalonEdit Source Editor.
- Current purpose: Provides Go to Definition, Peek Definition, Field Details Quick Peek, Find References, Completion Preview, and Add Property.
- UX concern: It launches both lightweight info surfaces and workflow dialogs from the same menu.
- Recommended classification: keep context menu.
- Risk: Medium
- Recommended migration phase: Phase 2/4
- Notes: Context-position logic is used for navigation/reference requests.

### 3.8 Completion dropdown

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml`, `RA2IniEditor.IDE/Views/Language/Ra2CompletionDropdownView.xaml`
- Type: popup
- Entry point: typing/auto trigger, menu command, source context menu.
- Current purpose: Shows completion candidates near the caret and commits selected completion.
- UX concern: Inline popup must coordinate with hover, caret movement, keyboard focus, and completion commit errors.
- Recommended classification: lightweight popup/card.
- Risk: Medium
- Recommended migration phase: Phase 2
- Notes: `CompletionDropdownPopup.IsOpen` is managed by `ShellWindow.xaml.cs`.

### 3.9 Source hover card

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: popup
- Entry point: mouse hover over source text after delay.
- Current purpose: Shows field details or reference value hover information.
- UX concern: The card is constructed in code-behind rather than a reusable XAML component, making visual consistency harder.
- Recommended classification: lightweight info popup/card.
- Risk: Medium
- Recommended migration phase: Phase 2
- Notes: Shares space with completion; hover closes when completion opens.

### 3.10 Dirty navigation dialog

- Path: `RA2IniEditor.IDE/Views/DirtyNavigation/Ra2DirtyNavigationDialog.xaml`, `RA2IniEditor.IDE/Services/DirtyNavigation/Ra2DirtyNavigationDialogService.cs`
- Type: modal dialog
- Entry point: dirty navigation guard before leaving current file/project.
- Current purpose: Choose save, discard, or cancel when unsaved edits exist.
- UX concern: This is a blocking decision and must remain concise and reliable.
- Recommended classification: modal confirmation.
- Risk: High
- Recommended migration phase: Phase 5
- Notes: Do not change dirty-state semantics during UI redesign.

### 3.11 Save preflight confirmation dialog

- Path: `RA2IniEditor.IDE/Views/SavePreflight/SavePreflightConfirmationDialog.xaml`, `RA2IniEditor.IDE/Services/SavePreflight/Ra2SavePreflightConfirmationService.cs`
- Type: modal confirmation
- Entry point: save command when preflight diagnostics find issues.
- Current purpose: Lets user continue or cancel risky save.
- UX concern: It is a high-trust safety gate; future UI should keep risk summary and clear action labels.
- Recommended classification: modal confirmation.
- Risk: High
- Recommended migration phase: Phase 5
- Notes: Do not change save/preflight semantics.

### 3.12 Open project folder dialog

- Path: `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: modal system dialog
- Entry point: File > Open Project / toolbar open folder button.
- Current purpose: Select project folder via `OpenFolderDialog`.
- UX concern: Standard OS dialog; not part of A15 redesign.
- Recommended classification: modal system dialog.
- Risk: Low
- Recommended migration phase: No redesign
- Notes: Keep behavior unchanged.

### 3.13 Search tool window

- Path: `RA2IniEditor.IDE/Views/SearchToolWindow.xaml`, `RA2IniEditor.IDE/Views/SearchToolWindow.xaml.cs`
- Type: non-modal window
- Entry point: Search menu / toolbar search button.
- Current purpose: Search project/source content and display results.
- UX concern: Separate window overlaps conceptually with the bottom Search Results tab.
- Recommended classification: dockable tool window.
- Risk: Medium
- Recommended migration phase: Phase 4/5
- Notes: Later polish should decide whether query and results stay together or split.

### 3.14 Issues tool window

- Path: `RA2IniEditor.IDE/Views/IssuesToolWindow.xaml`, `RA2IniEditor.IDE/Views/IssuesToolWindow.xaml.cs`
- Type: non-modal window
- Entry point: Code path exists in `ShellWindow.xaml.cs`; current menu focuses bottom issues tab.
- Current purpose: Detached issue browsing and navigation surface.
- UX concern: Potential duplication with embedded bottom issues tab.
- Recommended classification: dockable tool window.
- Risk: Medium
- Recommended migration phase: Phase 3/5
- Notes: Verify active entry points before redesigning; do not remove in Phase 1.

### 3.15 Peek definition window

- Path: `RA2IniEditor.IDE/Views/Language/Ra2PeekDefinitionWindow.xaml`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: non-modal window
- Entry point: Search menu, source context menu, Go to Definition fallback.
- Current purpose: Read-only display of definition target details.
- UX concern: A full non-modal window may be heavy for quick definition inspection.
- Recommended classification: lightweight info popup/card.
- Risk: Medium
- Recommended migration phase: Phase 2
- Notes: Good candidate for consistent info-card design.

### 3.16 Field quick peek window

- Path: `RA2IniEditor.IDE/Views/FieldQuickPeek/Ra2FieldQuickPeekWindow.xaml`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: non-modal window
- Entry point: source context menu > field details.
- Current purpose: Read-only field detail quick peek.
- UX concern: Same category as definition/reference details but currently separate window.
- Recommended classification: lightweight info popup/card.
- Risk: Medium
- Recommended migration phase: Phase 2
- Notes: Pair with hover and peek definition redesign.

### 3.17 Find references window

- Path: `RA2IniEditor.IDE/Views/Language/Ra2FindReferencesWindow.xaml`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: non-modal window
- Entry point: Search menu / source context menu > Find References.
- Current purpose: Shows reference result rows and supports navigation requests.
- UX concern: Should support grouping, file/section context, empty state, and source jump consistency.
- Recommended classification: dockable tool window.
- Risk: Medium
- Recommended migration phase: Phase 4
- Notes: Later A13/A14 UX polish target.

### 3.18 Add property window

- Path: `RA2IniEditor.IDE/Views/FieldBrowser/Ra2AddPropertyWindow.xaml`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: modal dialog
- Entry point: Edit menu / source context menu > Add Property.
- Current purpose: Browse field definitions, choose value, handle duplicate action, and insert/replace in current source.
- UX concern: Multi-step editing workflow with validation and optional annotation editing; should remain deliberate.
- Recommended classification: workflow dialog.
- Risk: High
- Recommended migration phase: Phase 3/5
- Notes: Do not change add-property insert/replace semantics.

### 3.19 Field annotation editor window

- Path: `RA2IniEditor.IDE/Views/FieldAnnotations/Ra2FieldAnnotationEditorWindow.xaml`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: modal dialog
- Entry point: Add Property window > Edit Annotation.
- Current purpose: Edit local field annotation metadata.
- UX concern: Nested modal flow from Add Property could become hard to follow.
- Recommended classification: workflow dialog.
- Risk: Medium
- Recommended migration phase: Phase 3
- Notes: Keep field annotation persistence semantics unchanged.

### 3.20 Field Registry Center window

- Path: `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml`, `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml.cs`
- Type: non-modal window
- Entry point: toolbar field registry button, View/Field Registry menu.
- Current purpose: Main field registry browser/status surface with active packs, field rows, reload, field learning, edit field, and advanced tools actions.
- UX concern: This is a core management workspace and should feel like an IDE tool window rather than a loose form.
- Recommended classification: dockable tool window.
- Risk: High
- Recommended migration phase: Phase 3
- Notes: Phase 3 A15-2 target.

### 3.21 Field editor window

- Path: `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml`, `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml.cs`
- Type: non-modal window
- Entry point: Field Registry Center > New Field/Edit Field or double-click field row.
- Current purpose: Edit field definition metadata, preview save, and save to project/global registry.
- UX concern: Contains write actions and apply-result paths; future UX should make target scope, validation, backup, and disabled reasons obvious.
- Recommended classification: workflow dialog.
- Risk: High
- Recommended migration phase: Phase 3
- Notes: Do not change registry write/apply behavior.

### 3.22 Field Registry Manager window

- Path: `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml`, `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml.cs`
- Type: non-modal window
- Entry point: Field Registry Center > Advanced Tools, field registry menu > Advanced Field Registry Tools.
- Current purpose: Advanced registry operations: reload, import preview, cleanup plan/apply, relearn current INI, folder openers, rollback manifests, warnings.
- UX concern: It mixes management, cleanup, rollback, and folder operations in a dense form.
- Recommended classification: workflow dialog / dockable tool window.
- Risk: High
- Recommended migration phase: Phase 3
- Notes: This is the primary A15-2 redesign candidate.

### 3.23 Field import preview window

- Path: `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml`, `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml.cs`
- Type: non-modal window
- Entry point: Field Registry Manager > Open Field Import Preview.
- Current purpose: Parse/import field registry sources, preview diff, manage remote presets, build/apply plans.
- UX concern: Multi-step write-capable workflow; needs explicit target scope, preview, warnings, and apply boundaries.
- Recommended classification: workflow dialog.
- Risk: High
- Recommended migration phase: Phase 3
- Notes: Includes remote preset editor dialogs and message-box confirmations.

### 3.24 Remote source preset editor

- Path: `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml`, `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml.cs`
- Type: modal dialog
- Entry point: Field import preview > add/edit remote preset.
- Current purpose: Edit remote source preset name, URL, description, tags, and enabled state.
- UX concern: Nested workflow dialog inside import preview; should keep validation and cancel/apply actions clear.
- Recommended classification: workflow dialog.
- Risk: Medium
- Recommended migration phase: Phase 3
- Notes: Do not add network behavior in this inventory phase.

### 3.25 Field learning wizard

- Path: `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- Type: non-modal window
- Entry point: Field Registry Center learning actions, field registry menu learning commands, advanced manager relearn current INI.
- Current purpose: Learn fields from current section/current INI/pasted text, review drafts, build apply plan, and apply.
- UX concern: Multi-step workflow with write capability; should make preview/apply boundaries and target scope clearer.
- Recommended classification: workflow dialog.
- Risk: High
- Recommended migration phase: Phase 3
- Notes: Uses `AllowedValuesEditorWindow` as nested dialog.

### 3.26 Allowed values editor

- Path: `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml`, `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs`
- Type: modal dialog
- Entry point: Field learning wizard > edit allowed values.
- Current purpose: Edit allowed values, display names, descriptions, dedupe/sort, append BuiltIn, restore scanned values.
- UX concern: Dense table editor inside field learning workflow; should remain scoped to value list editing.
- Recommended classification: workflow dialog.
- Risk: Medium
- Recommended migration phase: Phase 3
- Notes: This is not the removed legacy table-style editor; it is a focused field-value editing dialog.

### 3.27 MessageBox confirmations and warnings

- Path: multiple code-behind files under `RA2IniEditor.IDE/Views`
- Type: modal confirmation
- Entry point: Field registry cleanup/apply/rollback, remote preset deletion/clear history, field annotation library creation, folder/copy failures.
- Current purpose: Lightweight yes/no confirmations and warning/error notices.
- UX concern: MessageBox usage is quick but inconsistent for complex registry risks.
- Recommended classification: modal confirmation.
- Risk: Medium
- Recommended migration phase: Phase 3/5
- Notes: Later redesign can replace only where richer risk summary is needed.

### 3.28 Completion preview window

- Path: `RA2IniEditor.IDE/Views/Language/Ra2CompletionPreviewWindow.xaml`
- Type: not found in active flow
- Entry point: No construction of `Ra2CompletionPreviewWindow` found in current `RA2IniEditor.IDE` source scan.
- Current purpose: XAML and ViewModel exist, but current "Show Completion Preview" commands route to the completion dropdown.
- UX concern: Potential stale/unused window; verify before removing or redesigning.
- Recommended classification: not found in active flow.
- Risk: Low
- Recommended migration phase: Verify before Phase 2
- Notes: Do not delete in Phase 1.

### 3.29 Reference target details surface

- Path: `RA2IniEditor.IDE/Language/Ra2ReferenceValueDetailService.cs`, `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`, `RA2IniEditor.IDE/Views/Language/Ra2PeekDefinitionWindow.xaml`
- Type: popup/window combination
- Entry point: source hover and definition/peek commands.
- Current purpose: Shows recognized reference value context through hover or peek/definition surfaces.
- UX concern: Reference detail, field detail, and definition detail should converge visually.
- Recommended classification: lightweight info popup/card.
- Risk: Medium
- Recommended migration phase: Phase 2
- Notes: No standalone `ReferenceTargetDetailsWindow` was found.

## 4. Cross-cutting UI Problems

- Lightweight read-only information is split across hover cards, peek definition windows, field quick peek windows, and reference hover output.
- Persistent work areas are split between embedded bottom tabs and detached tool windows.
- Field registry workflows are spread across Field Registry Center, Advanced Tools, Import Preview, Field Learning Wizard, Field Editor, Allowed Values Editor, remote preset dialogs, and MessageBox confirmations.
- Several write-capable workflows need consistent target scope, preview, disabled reasons, backup, and rollback language.
- ShellWindow owns many UI creation paths directly, so later redesigns should avoid mixing behavior changes with visual reshaping.

## 5. Recommended Redesign Order

1. Phase 2 / A15-1: unify lightweight information surfaces: Source hover card, Peek Definition, Field Quick Peek, Reference Target Details.
2. Phase 3 / A15-2: redesign Field Registry Center and Field Registry Manager as IDE management surfaces.
3. Phase 3 continuation: align Field Import Preview, Field Learning Wizard, Field Editor, Allowed Values Editor, and Remote Source Preset Editor as explicit workflow dialogs.
4. Phase 4 / A13-A14: polish Find References and search/reference results, including grouping, navigation, empty states, and source jump consistency.
5. Phase 5: review modal confirmations: Save Preflight, Dirty Navigation, registry rollback/apply confirmations, and packaging smoke checklist.

## 6. Non-goals

This phase does not change:

- XAML layout or visual styles
- code-behind behavior
- field registry resolution
- completion behavior
- hover behavior
- Quick Peek behavior
- Find References semantics
- diagnostics semantics
- save preflight behavior
- backup / rollback semantics
- ShellWindow layout
- source files, tests, project files, package scripts, or BuiltIn field definitions

This phase does not restore:

- legacy root `RA2IniEditor.sln`
- legacy root `RA2IniEditor.csproj`
- legacy MainWindow
- legacy table-style editor
- legacy object workbench

## 7. Validation

Run after this documentation-only inventory:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```
