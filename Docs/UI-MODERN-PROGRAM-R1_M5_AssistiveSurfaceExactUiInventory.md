# UI-MODERN-PROGRAM-R1 — M5 Assistive Surface Exact UI Inventory

Status: Implementation authority for M5 presentation cards  
Parent contract: `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md` Revision A  
Recorded: 2026-07-23

## 1. Current task goal

Modernize the editor-assist and transactional presentation layer so Completion, Quick Peek, Peek Definition, Find References, Dirty Navigation and Save Preflight use the same compact IDE hierarchy established by M3/M4. This inventory authorizes presentation-only reshaping. It does not authorize any editor, navigation, diagnostic, save or registry behavior change.

## 2. Reuse decision

- Introduce `Themes/IdeEditorAssistStyles.xaml` after the semantic/base visual dictionaries and before `ShellTheme.xaml`.
- The new dictionary may use only semantic/base resources from `IdeVisualTokens.xaml`, `IdeControlStyles.xaml`, `IdeCollectionStyles.xaml` and `IdeWorkspaceStyles.xaml`.
- Reuse `UiDataGridStyle`, `IdeToolWindowDataGridStyle`, semantic brushes, focus borders, compact buttons and code typography. Do not introduce a package, custom control library, converter or code-behind styling API.
- `Resources/Styles/IdeSecondaryWindowStyles.xaml` remains a compatibility source during M5-A through M5-C. M5-D may redirect or retire only aliases proven safe by a zero-reference inventory; global replacement without proof is forbidden.

## 3. Exact surface inventory

### 3.1 Completion dropdown — active runtime surface

Files:

- `RA2IniEditor.IDE/Views/Language/Ra2CompletionDropdownView.xaml`
- `RA2IniEditor.IDE/Views/Language/Ra2CompletionDropdownView.xaml.cs`
- Hosted by `ShellWindow.xaml` in `CompletionDropdownPopup`, placed relative to `SourceTextEditor`.

Runtime/lifetime facts:

- Shell owns one long-lived view and one `Ra2CompletionDropdownViewModel`.
- `ShowCompletionDropdown` updates the existing ViewModel, computes caret-relative offsets and sets `Popup.IsOpen=true`.
- Shell owns close/focus/commit behavior. M5 must not change popup placement, `StaysOpen`, focus routing, caret restoration or result lifetime.
- Enter/Tab raise `CompletionCommitRequested`; Escape raises `CompletionCloseRequested`; double-click raises `CompletionItemDoubleClicked`; preview mouse down raises `CompletionDropdownInteracted`.

Required AutomationIds: `Ra2CompletionDropdown.View`, `Ra2CompletionDropdown.ItemsList`.

Required bindings: `Items`, two-way `SelectedIndex`, `Label`, `TypeDisplay`, `AnnotationText`, `SourceDisplayText`.

Allowed presentation work: semantic selection stripe, compact four-column row, non-form border/elevation, source badge treatment, virtualization-preserving list styling.

### 3.2 Completion preview — dormant diagnostic surface

Files:

- `RA2IniEditor.IDE/Views/Language/Ra2CompletionPreviewWindow.xaml`
- `RA2IniEditor.IDE/Views/Language/Ra2CompletionPreviewWindow.xaml.cs`

Runtime/lifetime facts:

- The type has an internal constructor and `Update` method, but there is no production creation/show call.
- M5 may modernize its XAML so the retained diagnostic surface is coherent; it must not add a Shell command, instantiate the window or change its visibility/lifetime.

Required AutomationIds: `Ra2CompletionPreview.Window`, `.CountText`, `.ReplacementText`, `.ItemsGrid`, `.StatusText`.

Required bindings: `CountText`, `ReplacementText`, `Groups`, group `Name`/`Items`, item `Label`, `SourceDisplayText`, `IsFallback`, `Detail`, `Documentation`, `InsertText`, `Priority`, and `StatusText`.

### 3.3 Quick Peek — active caret-relative inspector

Files:

- `RA2IniEditor.IDE/Views/FieldQuickPeek/Ra2FieldQuickPeekWindow.xaml`
- `.xaml.cs`

Lifetime facts:

- Shell reuses the visible instance through `Update`, repositions it through `Ra2FloatingInspectorPlacement`, otherwise creates an owned non-modal window.
- `Closed` clears the Shell field. Escape and the close button call `Close()`.
- Borderless settings are intentionally duplicated in XAML/code: transparent, no taskbar, no resize, size-to-content, manual placement.

Required AutomationIds: `FieldQuickPeek.Window`, `.TitleText`, `.CloseButton`.

Required bindings: `Title`, `ValueKindDisplay`, `SectionKindDisplay`, `SourceDisplay`, `EditorKindDisplay`, `TrustDisplay`, `IsNotFound`, `NotFoundMessage`, `Key`, `DisplayName`, description/trust/example/allowed-value visibility and content bindings.

### 3.4 Peek Definition — active caret-relative inspector

Files:

- `RA2IniEditor.IDE/Views/Language/Ra2PeekDefinitionWindow.xaml`
- `.xaml.cs`

Lifetime facts are identical to Quick Peek: owned non-modal reusable-visible instance, caret placement, Escape/close destruction, and `Closed` field reset.

Required AutomationIds: `Ra2PeekDefinition.Window`, `.TitleText`, `.CloseButton`, `.SourceText`, `.ContentScrollViewer`, `.DetailText`.

Required bindings: `Title`, `Kind`, `LineText`, `SourceName`, `Detail`, `Description`, `SourcePath`.

### 3.5 Find References — active AvalonDock tool

Files:

- `RA2IniEditor.IDE/Views/Language/Ra2FindReferencesView.xaml`
- `.xaml.cs`
- Hosted by `FindReferencesAnchorable`, ContentId `Tool.FindReferences`.

Dock/lifetime facts:

- The existing anchorable remains bottom-home, floatable/hideable, non-closeable, non-document-dockable, 700 × 460 floating profile.
- Shell sets a new `Ra2FindReferencesViewModel`, then restores/activates the registered tool.
- Row double-click raises `ReferenceNavigateRequested`; Shell performs editor navigation.

Required AutomationIds: `Ra2FindReferences.View`, `.TargetText`, `.ReferencesGrid`, `.StatusText`.

Required bindings: `Target`, `StatusText`, `References`, and row `Section`, `Key`, `Value`, `LineText`.

Allowed presentation work: compact target header, result grid, status strip, semantic row/focus states. ContentId, docking profile, double-click event and navigation are frozen.

### 3.6 Dirty Navigation — modal transactional decision

Files:

- `RA2IniEditor.IDE/Views/DirtyNavigation/Ra2DirtyNavigationDialog.xaml`
- `.xaml.cs`
- `Ra2DirtyNavigationDialogService` is read-only.

Lifetime/result facts:

- Service constructs the dialog with `Owner`, calls `ShowDialog`, and maps anything except `true` to Cancel.
- Default `Decision` is Cancel. Save/Discard/Cancel buttons set the corresponding enum then set `DialogResult=true`.
- Window is owner-centered, fixed-width, height-to-content and non-resizable.

Required AutomationIds: `DirtyNavigation.Dialog`, `.SaveButton`, `.DiscardButton`, `.CancelButton`.

Required named element: `FilePathTextBlock` receives the file name and full-path tooltip.

### 3.7 Save Preflight — modal transactional decision

Files:

- `RA2IniEditor.IDE/Views/SavePreflight/SavePreflightConfirmationDialog.xaml`
- `.xaml.cs`
- `Ra2SavePreflightConfirmationService` and diagnostics are read-only.

Lifetime/result facts:

- Service constructs with `Owner`, calls `ShowDialog`, and returns true only for `DialogResult=true`.
- Constructor projects `SummaryText`, `SourceSummaryText` and `SeveritySummaryText`; it does not recompute diagnostics.
- Continue sets true; Cancel sets false. Window is owner-centered, fixed-width, height-to-content and non-resizable.

Required AutomationIds: `SavePreflight.Dialog`, `.SummaryText`, `.ContinueButton`, `.CancelButton`.

Required named elements: `SummaryTextBlock`, `DetailTextBlock`.

## 4. Approved files and card budget

### Foundation (maximum 3 files)

- add `RA2IniEditor.IDE/Themes/IdeEditorAssistStyles.xaml`
- modify `RA2IniEditor.IDE/App.xaml`
- modify/add focused visual boundary tests

### M5-A (maximum 4 files)

- Completion dropdown XAML
- dormant Completion preview XAML
- focused visual/boundary test files only when assertions describe the superseded visual structure

### M5-B (maximum 4 files)

- Quick Peek XAML
- Peek Definition XAML
- Find References XAML
- focused visual/boundary tests

### M5-C (maximum 3 files)

- Dirty Navigation XAML
- Save Preflight XAML
- focused visual/boundary tests

### M5-D (maximum 3 files)

- `IdeSecondaryWindowStyles.xaml` only after exact reference audit
- assist-style dictionary if convergence aliases are required
- focused visual boundary tests

XAML code-behind is not approved for modification unless build evidence proves a presentation-only compile requirement. Any such need is a stop-and-review condition.

## 5. Forbidden files and semantic boundaries

- `ShellWindow.xaml` and `.xaml.cs`, all Dock profiles/content IDs/layout persistence, completion controllers/providers/commit planner/coordinator and positioning.
- Quick Peek/definition services, navigation controllers, editor caret/selection/focus logic and ViewModels.
- Dirty Navigation and Save Preflight services, decisions, diagnostics, save/write/backup behavior and dialog code-behind result mapping.
- Field Registry runtime/data, AI, parser, diagnostics semantics, BuiltIn data, project files/dependencies and legacy.
- No new public API, class, converter, dependency, Window owner rule, `Show`/`ShowDialog`, `DialogResult`, close behavior or keyboard semantic.

## 6. Additive UIA policy

All IDs listed above are mandatory and immutable. Additive landmark IDs are allowed only when they identify a stable logical region rather than a decorative element. Approved prefixes: `Ra2CompletionDropdown.*`, `Ra2CompletionPreview.*`, `FieldQuickPeek.*`, `Ra2PeekDefinition.*`, `Ra2FindReferences.*`, `DirtyNavigation.*`, `SavePreflight.*`. Duplicate IDs within a surface are forbidden.

## 7. Validation commands and acceptance gates

Per card:

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter <focused-filter>
```

Package gate:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly -PackageName RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M5.Accepted.Rollback.zip
```

Static acceptance additionally checks exact AutomationId preservation/no duplicates, no hard-coded color in affected XAML, no production reference added to dormant Completion preview, and unchanged handler/binding sets.

Visual gate requires real WPF evidence for Completion dropdown, one caret-relative inspector, Find References, Dirty Navigation and Save Preflight. A smoke must not commit completion, navigate/write a different file, continue save, discard, fetch, apply or mutate registry data.

## 8. Approval and stop rules

The user already approved continuous execution of Revision A, so no per-card approval is required. Stop only for a public API/lifecycle/semantic change, a required Shell/Dock topology edit, a new dependency, an inability to preserve the exact handler/UIA contract, an R3/R4 architecture conflict, or repeated verification failure that cannot be resolved inside the current card.

