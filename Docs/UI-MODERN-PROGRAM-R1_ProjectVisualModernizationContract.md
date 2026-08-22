# UI-MODERN-PROGRAM-R1 — Project Visual Modernization Contract

Status: confirmed by the user on 2026-07-22; continuous StagePackage execution authorized without per-card approval waits.  
Revision: A  
Primary visual baseline: light theme, 1920 x 1080 at 100% scaling.  
Technology: WPF + AvalonEdit + Dirkster.AvalonDock 4.74.1.  

## 1. Authority and succession

This contract is the project-level successor for visual modernization. It does not rewrite completed UI-DOCK or UI-MODERN history.

Authority order:

1. Current user instruction and this confirmed Revision A contract.
2. `UI-MODERN-M1-R2_PreviewParityContract.md` for light-shell hierarchy and Search floating topology boundary.
3. Completed UI-DOCK contracts for ContentIds, Home recovery, close/minimize behavior, persistence and monitor handling.
4. `UI-MODERN-M2-R2_CohesiveShellModernizationContract.md` for accepted floating-host and title/menu lifecycle.
5. M0 WPF dimension and responsive documents, except their obsolete bottom-hosted Search statements.
6. Older illustrative or historical UI documents.

Search is an independent AvalonDock floating tool with preferred 560 x 620 DIP Home geometry. Any M0 text or drawing that describes Search as a default bottom tool is superseded. Problems and Output remain the only default bottom tools. The dark reference is authoritative only for density, hierarchy and proportions; implementation remains light-theme only.

Frozen visual references:

- `Docs/UiVisualBaselines/UI-SHELL-Light.png`
- `Docs/UiVisualBaselines/UI-FR-Light.png`
- `Docs/UiVisualBaselines/UI-SEARCH-Light.png`
- `Docs/UiVisualBaselines/UI-MODERN-M1-R2-LayoutDirection-DarkReference.png`
- `Docs/UiVisualBaselines/UI-SHELL-1920x1080-LayoutReference.png`

## 2. Risk and governance

The complete program is R3 because it changes the shared presentation boundary across Shell, AvalonDock, Field Registry and secondary surfaces. The user explicitly authorized this boundary direction. Every implementation Task Card must be reduced to R1: local, contract-preserving, no public API, no persistence change and no business-semantic change.

Any public API requirement is R2 and stops the current card for contract review. Any Dock persistence, Field Registry authority/write, AI lifecycle, save compatibility or presentation-to-authority change is R3/R4 and stops the package.

Continuous packages use Deferred Governance. Governance is flushed after 3-5 Task Cards, at every package/visual gate, on verification failure, on architecture conflict, or before handoff/context compaction.

## 3. Product goal

Modernize the complete IDE presentation so the real WPF application matches the accepted preview in:

- workspace proportions and editor dominance;
- compact IDE typography and command density;
- flat tool-window hierarchy instead of nested form cards;
- graphical issue severity and filtering;
- coherent Project Explorer, Problems, Output, AI and floating Search surfaces;
- Field Registry navigation/list/details architecture;
- coherent completion, peek, annotation and transactional dialogs;
- keyboard, automation, resolution and DPI behavior.

The task is not a color-only reskin. It establishes one semantic token authority, scoped pattern dictionaries and explicit per-surface adoption.

## 4. Non-goals and protected behavior

The program must not implement or change:

- dark theme or theme switching;
- real Search/Replace, indexing or result navigation (`SEARCH-1` remains separate);
- AI provider, model, timeout, SSE, streaming, cancellation or failure taxonomy;
- INI parser, diagnostics generation, Completion generation/commit, Hover data source or Quick Peek semantics;
- Field Registry provider priority, lookup, import, apply, backup, rollback, cleanup, learning or BuiltIn data;
- save, undo/redo, dirty navigation or Save Preflight semantics;
- AvalonDock ContentIds, Home profiles, layout persistence schema, monitor recovery or close-to-Home lifecycle;
- Shell MVVM rewrite, new UI framework, new dependency or project-file change;
- legacy editor restoration.

No raster control asset or image-generation API is required. Icons are project-owned XAML Geometry resources.

## 5. Visual architecture

### 5.1 Semantic source of truth

`IdeVisualTokens.xaml` remains the only palette, typography, density and geometry authority. Domain dictionaries may reference tokens but may not define competing raw palettes.

### 5.2 Resource layers

| Dictionary | Responsibility |
|---|---|
| `IdeControlStyles.xaml` | Button, TextBox, ComboBox, CheckBox, RadioButton and basic input templates |
| `IdeCollectionStyles.xaml` | Tree, List, DataGrid, ScrollBar, Splitter and collection behavior |
| `IdeWorkspaceStyles.xaml` | Shell Problems/Output/tool workspaces, common command bands, logs and flat IDE lists |
| `IdeFieldRegistryStyles.xaml` | Field Registry Center, Manager, Harvest, Learning and editor patterns |
| `IdeEditorAssistStyles.xaml` | Completion, Quick Peek, Find References, annotation and transaction-dialog patterns |
| `ShellTheme.xaml` | Shell and AvalonDock-specific chrome, tabs and floating host visuals |
| `IdeSecondaryWindowStyles.xaml` | Temporary compatibility aliases during migration |

The three scoped pattern dictionaries share the same tokens and base control templates; they are not independent visual systems.

### 5.3 Resource freeze rule

- A resource key accepted at a visual gate becomes frozen.
- Later packages prefer additive keyed styles rather than mutating an accepted shared key.
- Changing a frozen key is R3 and requires regression verification of every referencing accepted surface.
- Compatibility aliases remain until a zero-reference audit passes.
- Application-wide implicit control styles are forbidden; the existing Window font-family authority remains allowed.

### 5.4 Modern WPF rules

Allowed: one-DIP region separators, weak row separators, 2-4 DIP local corner radius, light transient shadow, semantic focus/hover/selected states.  
Forbidden: nested cards, full borders around every group/DataGrid, per-row shadow, dashboard layout, oversized uniform buttons, color-only status, Emoji/character icons and fixed widths used only to hide layout defects.

UI font authority is `Segoe UI Variable Text, Microsoft YaHei UI, Segoe UI`. Code and INI surfaces remain Consolas.

## 6. Geometry and responsive contract

Primary Shell geometry at 1920 x 1080 / 100%:

| Region | DIP |
|---|---:|
| title/menu band | 30 |
| main toolbar | 32 |
| status bar | 24 |
| right tool well | 300 default |
| editor-side bottom tools | 260 default |
| editor-side width | approximately 1616 |
| editor viewport with bottom tools open | approximately 1616 x 700 |

The right and bottom tools remain user-resizable. Added width belongs to the editor before tool wells grow. User-persisted Dock layout is never overwritten by responsive presentation logic.

Field Registry standard frame is 1040 x 700 DIP with default 156 / 552 / 300 navigation, list and details columns. Search preferred floating Home size is 560 x 620 DIP.

Acceptance matrix:

- 1920 x 1080 / 100%: primary screenshot baseline;
- 1920 x 1080 / 125%: common DPI;
- 2560 x 1440 / 150%: high DPI;
- 1280 x 800 / 100%: compact fallback;
- 1366 x 768 / 100%: common minimum screen.

Geometric tolerance:

| Item | Tolerance |
|---|---:|
| title/toolbar/status height | +/- 1 DIP |
| primary workspace columns | +/- 4 DIP |
| Field Registry columns | +/- 6 DIP |
| list row height | +/- 2 DIP |
| toolbar spacing | +/- 2 DIP |
| icon size | +/- 1 DIP |

A second complete border around a primary region is never accepted as a tolerance difference.

## 7. Window chrome matrix

| Surface | Chrome | Minimize | Maximize | Close | Owner/lifecycle |
|---|---|---:|---:|---:|---|
| Shell | accepted custom Shell chrome | yes | yes | yes | top level |
| AvalonDock floating tool | accepted Dock floating chrome | yes | no dedicated button | close-to-hide/Home | Shell/Dock owned |
| Field Registry Center/Manager | existing project-owned large-workspace chrome | no | preserve current contract | yes | Shell owned |
| Field Editor/Learning/Allowed Values | existing project-owned workflow chrome | no | no | yes | Registry workflow owned |
| Quick Peek/Peek | borderless transient surface | no | no | existing close/focus behavior | Shell owned |
| Dirty Navigation/Save Preflight | modal client surface | no | no | yes | Shell owner, ShowDialog |

No card may change WindowStyle, Owner, Show/ShowDialog, DialogResult, minimize/close recovery or shutdown handling unless that behavior is explicitly re-contracted.

## 8. Issues and state-expression contract

Problems use graphical severity:

- Error: red circle/cross geometry;
- Warning: amber triangle/exclamation geometry;
- Information: blue circle/information geometry.

Icons must have accessible names/tooltips and must not rely on color alone. The icon column is approximately 32-36 DIP. The top band exposes All/Error/Warning/Information graphical counts using the existing `IssuesViewModel` state. Diagnostics production and navigation semantics remain unchanged.

List states across the program use a common hierarchy: weak hover, explicit selected accent/background, visible keyboard focus, readable disabled state, no thick rounded row cards.

## 9. Exact UI Inventory gate

Before each visual package, a read-only inventory must record:

- named elements and AutomationIds;
- bindings and DataContext ownership;
- event handlers and code-behind element access;
- Owner, Show/ShowDialog, DialogResult and close lifecycle;
- selection, scroll, expanded/tab state that must survive;
- virtualization requirements;
- current tests that assert structure or behavior.

Required inventories: `M3-0 ShellExactUiInventory`, `M4-0 FieldRegistryExactUiInventory`, and `M5-0 AssistiveSurfaceExactUiInventory`. Implementation may only reshape structures proven safe by the corresponding inventory.

## 10. Data ownership and API contract

No new or changed public API is authorized. No new persisted state or external dependency is authorized.

Shell continues to own Dock content, editor coordination, AI lifecycle and language assistance. Runtime-created AI/Hover visual trees may read shared resources, but visual elements never become state owners.

Field Registry Center may add one internal presentation-only type:

```text
internal sealed class FieldRegistryCenterPresentationState
```

It may reuse `Ra2FieldDetailsViewModel` for the selected field and live only for the Window lifetime. It may not write Registry state, alter provider priority, persist selection or expose public API. Any additional C# abstraction requires a contract stop.

## 11. Virtualization and performance

- Field Registry field lists must retain UI virtualization and `CanContentScroll` behavior.
- Large lists may not be replaced by non-virtualized ItemsControls.
- Per-row shadow, deep Border nesting and expensive converters are forbidden.
- Selecting a field updates only the details presentation; it must not rebuild the full list.
- Filtering must not copy or mutate provider definitions.
- Harvest, Learning, rollback and issue lists preserve stable ordering and virtualization where currently available.
- No visual card may introduce continuous timers, polling or per-frame allocation.

## 12. Automation contract

All existing AutomationIds are frozen, including Shell, Dock, Search and Field Registry anchors. Planned additive anchors include:

```text
Shell.BottomIssues.Filter.All
Shell.BottomIssues.Filter.Error
Shell.BottomIssues.Filter.Warning
Shell.BottomIssues.Filter.Info
Shell.BottomIssues.Count.All
Shell.BottomIssues.Count.Error
Shell.BottomIssues.Count.Warning
Shell.BottomIssues.Count.Info
FieldRegistryCenter.Navigation
FieldRegistryCenter.FieldList
FieldRegistryCenter.Details
FieldRegistryCenter.Details.EmptyState
```

DataTemplate instances may not generate duplicate AutomationIds. Row severity uses accessible Name/ItemStatus. Icon-only commands require AutomationId, accessible Name and ToolTip.

## 13. StagePackage plan

### P0 — Authority and rollback

- P0-A: persist this contract and authority map.
- P0-B: create a uniquely named clean-source rollback zip, SHA-256 and entry-count record.

### M3 — Shell workspace

- M3-0: exact Shell/Dock/Issues/Output/AI/Search inventory.
- M3-A: add `IdeWorkspaceStyles.xaml` and resource-load tests.
- M3-B: graphical Problems and standalone Issues parity.
- M3-C: flat Output and Project Explorer.
- M3-D: unify static and runtime-created AI/Hover presentation within named method boundaries only.
- M3-E: compact toolbar, Dock tabs/floating title and Search presentation.
- Visual gate and uniquely named M3 accepted rollback package.

`ShellWindow.xaml.cs` edits in M3-D are limited to AI message/Markdown/list/table/code-block and source-hover visual-construction methods plus at most one small private required-resource helper. AI send/cancel, stream subscription, provider/model, Dock, Search restore and editor/session methods are forbidden.

### M4 — Field Registry family

Completion note (2026-07-23): M4-0, Foundation and M4-A through M4-V completed under Revision A. The authoritative implementation/evidence record is `Docs/UI-MODERN-PROGRAM-R1_M4_StageLedger.md`; M5-0 is the next safe entry.

M4-R2 convergence note (2026-07-23): the user-confirmed successor contract was implemented across Center, Manager, Import Preview, Learning, Field Editor, Allowed Values, Remote Preset, Add Property and Annotation. The implementation and automated gates are complete: Debug build passed with 0 warnings/0 errors, full non-UI tests passed 2334/2334, changed XAML/resource/AutomationId/virtualization gates passed, and the legacy solution was not restored. The authoritative evidence record is `Docs/UI-MODERN-PROGRAM-R1_M4R2_StageLedger.md`. The eight required real-WPF screenshots remain NotRun, so visual acceptance is pending and must not be inferred from automated verification.

- M4-0: exact Center/Manager/Harvest/Learning/Editor inventory.
- M4-A: Center left-navigation / field-list / details workspace.
- M4-B: Manager maintenance workspace.
- M4-C: Harvest source/candidate/diff/plan/result hierarchy.
- M4-D: Learning hierarchy.
- M4-E: Field Editor and Allowed Values.
- M4-F: Add Property and Annotation.
- M4-G: Remote Preset.
- Two visual checkpoints and uniquely named M4 accepted rollback package.

`IdeFieldRegistryStyles.xaml` is introduced at M4 and may only reference semantic/base resources. Field Registry ViewModels and services remain read-only.

### M5 — Assistive and transactional surfaces

Completion note (2026-07-23): M5-0, Foundation and M5-A through M5-V completed under Revision A. The authoritative implementation/evidence record is `Docs/UI-MODERN-PROGRAM-R1_M5_StageLedger.md`; M6-A is the next safe entry.

- M5-0: exact Completion/Peek/References/dialog inventory.
- M5-A: Completion dropdown/preview.
- M5-B: Quick Peek, Peek Definition and Find References.
- M5-C: Dirty Navigation and Save Preflight client presentation.
- M5-D: secondary-style compatibility convergence.
- Visual gate and uniquely named M5 accepted rollback package.

`IdeEditorAssistStyles.xaml` is introduced at M5 and may only reference semantic/base resources.

### M6 — Closure

- M6-A: responsive, keyboard and additive UIA smoke.
- M6-B: zero-reference audit and safe legacy-style cleanup.
- M6-C: full verification, screenshot index, governance closure and final clean package.

M6-B completion note (2026-07-23): the exact audit is `Docs/UI-MODERN-PROGRAM-R1_M6B_ZeroReferenceAudit.md` and the authoritative stage record is `Docs/UI-MODERN-PROGRAM-R1_M6_StageLedger.md`. M6-B removed 14 safe Shell history keys, retired the complete `IdeSecondary*` compatibility layer after canonical consumer migration, and preserved all dynamically addressed, implicit, frozen or positive-test-contract resources. Debug build passed with 0 warnings/0 errors; affected boundary tests passed 64/64; full non-UI tests passed 2332/2332. M6-C is the next safe entry and has not started.

## 14. Task Card budget and test rules

Each implementation card modifies at most five files, adds at most two classes and adds no public methods. It may not move files, change directories or format unrelated content.

Existing tests may only be changed when an assertion explicitly describes visual structure superseded by this contract. Behavior, AutomationId, lifecycle and semantic assertions may not be removed or weakened. Every changed assertion records the previous intent, replacement intent and contract basis in the test name or nearby comment where non-obvious.

Build success cannot substitute for screenshot acceptance. Screenshot acceptance cannot substitute for build, UIA, keyboard or behavior verification.

## 15. Visual evidence

Required deterministic names include:

```text
M3-Shell-1920x1080-100.png
M3-Problems-ErrorsWarningsInfo.png
M3-AI-Streaming.png
M3-Search-FloatingHome.png
M4-RegistryCenter-Default.png
M4-RegistryCenter-FieldSelected.png
M4-RegistryManager-Rollback.png
M4-Harvest-DiffReview.png
```

At each visual gate review region proportions, hierarchy, border/card count, typography, density, focus/selection, empty/error/long-text states and window chrome. If general computer control is avoided, existing project automation and user-supplied screenshots are preferred; without real WPF evidence a stage is recorded as visually unverified rather than completed.

## 16. Rollback policy

The repository has no Git metadata. A unique clean-source rollback package is required at P0 and after accepted M3, M4 and M5 gates. Each anchor records package path, SHA-256, source entry count, verification state, accepted screenshots and next safe entry. Existing archives are never overwritten.

## 17. Verification

Per card: compile plus the smallest credible targeted boundary tests.  
Per package: IDE-only build plus affected package tests.  
Final:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 18. Stop conditions

Stop and flush partial governance if any card requires:

- public API, ViewModel/business-semantic or persistence changes;
- modification of Dock Store/Session/Coordinator/Chrome controllers;
- Field Registry provider/apply/rollback/learning changes;
- AI stream/provider/failure changes;
- new dependency or project-file change;
- weakening existing behavioral assertions;
- more than five files or unbounded shared-style regression;
- recovery from a failed build/test outside the allowed card scope.

The user's no-intermediate-approval instruction removes approval waits between passing cards and packages; it does not waive these safety stops or the requirement to record visual evidence honestly.

## 19. VISUAL-FIX1 completion addendum

On 2026-07-23 the user approved and executed the bounded `VISUAL-FIX1` correction under this contract. Its authoritative implementation and verification record is:

```text
Docs/UI-MODERN-PROGRAM-R1_VISUAL-FIX1_StageLedger.md
```

The correction is presentation-only: Field Editor close geometry, Field Registry Center responsive proportions, and the AI panel's concise context/welcome/footer composition. It adds no Expander, state, public API, dependency or product semantic change. Automated gates are complete; manual screenshot acceptance remains required.

## 20. VISUAL-FIX2 completion addendum

On 2026-07-23 the user confirmed the bounded `VISUAL-FIX2` successor correction. Its authoritative result is:

```text
Docs/UI-MODERN-PROGRAM-R1_VISUAL-FIX2_StageLedger.md
```

This approved exception to the generic controller stop condition changes only startup presentation wiring in `ShellDockFloatingChromeController` and `ShellWindow.xaml.cs`. It preserves the existing Dock coordinator, Store/Session, ContentIds, Home profiles, persistence format/migration and asynchronous floating-host visibility order.

The Field Registry active-pack table uses compact 88/48 DIP columns. During initial topology/restoration, the existing floating-chrome owner suppresses intermediate host rendering and restores prior opacity in `finally`; the suppression path never performs the unsafe immediate `Hide()`. Automated gates are complete; real startup/no-flash and final spacing remain manual visual acceptance items.
