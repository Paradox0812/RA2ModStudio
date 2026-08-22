# UI-MODERN-M1-R2 — Light Shell Preview Parity Contract

Status: completed on 2026-07-22 under the user's continuous-execution authorization.

Risk: R4 package overall because it includes the separately owned UI-DOCK-5 presentation-layout migration. The visual-only cards are R1/R3.

## 1. Authority and succession

This document amends, but does not rewrite, `UI-MODERN-M1_VisualSystemFoundationContract.md`.

- M1A through M1C remain accepted foundations.
- The unaccepted M1D visual result is superseded by M1D-R2A/R2B/R2C.
- M1E keeps its original IntegratedShellChrome identity.
- M1F becomes M1F-R1 and adds an explicit single-visible-title invariant.
- M1G retains its original package-verification and governance-closure meaning.
- Search topology and persistence migration are owned by `UI-DOCK-5_SearchFloatingTopologyContract.md`; they are not hidden inside M1G.

The frozen preview is `UiVisualBaselines/UI-MODERN-M1-R2-LayoutDirection-DarkReference.png`, 1672 x 943 pixels, SHA-256 `D3FD7ABC243704A2260BE62D6489B83BC0D798999AA76929B2BCFC8CA1AD869C`.

Only its layout, density, hierarchy, single-title Search host, and tool-region proportions are authoritative. Its dark palette, sample project, sample files, source text, labels that do not exist in the product, and illustrative values are not runtime fixtures. M1-R2 remains light-theme only.

## 2. Product target

At the 1920 x 1080 design baseline:

- compact 30-DIP title/menu band and 32-DIP toolbar;
- editor-side workspace remains the primary area;
- 300-DIP Right tool well spans the workspace height;
- 260-DIP Bottom tool well exists only under the editor column;
- Bottom shows Problems and Output by default; Find References remains on-demand;
- Search is not a default Bottom tab and is opened through UI-DOCK-5 as an independent AvalonDock floating tool;
- document tabs, dock tabs/titles, Project Explorer, Problems, Output, splitters, menus, toolbar, and status bar share one light visual language;
- restrained 1-DIP borders, compact spacing, explicit focus/hover/pressed states, no gradients, glass, large cards, or oversized rounding.

Responsive requirements remain those of M0. Search content must remain usable at 1280 x 800 and 125%/150% DPI without hiding its title bar outside the monitor work area.

## 3. Semantic and API boundaries

No C# public API, dependency, project file, parser, editor, AI, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup, or rollback semantic may change.

UI Automation is contract-visible:

- preserve all seven dock ContentIds and existing Shell/Dock AutomationIds;
- preserve `Search.View`, `Search.QueryTextBox`, `Search.CaseSensitiveCheckBox`, `Search.WholeWordCheckBox`, and `Search.RegexCheckBox`;
- retire visible `Search.ResultsList` and `Search.StatusText` with the mock result surface;
- add `Search.ScopeComboBox`, `Search.FilePatternComboBox`, `Search.FindPreviousButton`, `Search.FindNextButton`, `Search.FindAllButton`, and `Search.UnavailableHint`;
- preserve/add the M1E/M1F window-control anchors defined by the predecessor contract.

This is no C# public API change, but it is a controlled UI Automation contract change and must be reflected in tests and the deferred API ledger.

## 4. Search visual boundary

M1-R2 does not implement real Search or Replace.

- no runtime sample query, result row, result count, or `mock` text may be visible;
- illustrative `Primary` may exist only as design-time `d:` data;
- the command fields and action buttons remain visibly unavailable until SEARCH-1 owns real behavior;
- only the Find-in-files composition is exposed now; no disabled Replace tab is added without a later SEARCH-2 contract;
- the existing public mock view-model members remain temporarily for compatibility but are not bound to a visible result surface; SEARCH-1 is their cleanup trigger;
- future Search results require a separately contracted ContentId and must not reuse `Tool.FindReferences`.

## 5. WPF implementation constraints

- styles remain explicitly keyed; no new application-wide implicit restyling;
- `IdeSecondaryWindowStyles.xaml` and secondary-window XAML remain frozen;
- top-level menu icon/arrow columns collapse without narrowing submenu affordances;
- DataGrid resize thumbs use a transparent project-owned template with a 4-DIP hit area and `SizeWE` cursor; opacity-only hiding is forbidden;
- main/floating windows keep `AllowsTransparency=False` and native DWM composition;
- no full-window `DropShadowEffect`, manual full-window clipping, fake drag threshold, or replacement dock engine;
- WindowChrome must preserve system menu, minimize/maximize/restore/close, Win+Arrow, Snap Layout, work-area maximize, and per-monitor DPI behavior;
- single-pane floating hosts display exactly one title; multi-pane hosts retain their internal tabs without duplicating the active title in outer chrome;
- anchorable X invokes AvalonDock hide semantics, never destructive `Window.Close()` on the managed content model.

## 6. Continuous card order

### M1D-R2A — FoundationDefectCorrection

Allowed files (3): `Themes/IdeControlStyles.xaml`, `Themes/IdeCollectionStyles.xaml`, `Tests/IDE/IdeVisualSystemBoundaryTests.cs`.

Correct top-level menu template columns and DataGrid header resize thumbs. Verify menu roles and column resizing before adoption work continues.

### M1D-R2B — BottomToolComposition

Allowed files (4): `Views/ShellWindow.xaml`, `Themes/ShellTheme.xaml`, `Tests/IDE/IdeShellBoundaryTests.cs`, `Tests/IDE/IdeVisualSystemBoundaryTests.cs`.

Modernize Problems/Output command surfaces, tab density, result/header separators, and output text surface without changing commands or bindings.

### M1D-R2C — ShellWorkspaceComposition

Allowed files (4): `Views/ShellWindow.xaml`, `Themes/ShellTheme.xaml`, `Tests/IDE/IdeShellBoundaryTests.cs`, `Tests/IDE/IdeVisualSystemBoundaryTests.cs`.

Complete editor document strip, Right tools, Project Explorer, splitters, empty/focus states, and status-bar composition. Capture real maximized and compact screenshots. User approval is pre-authorized for continuous execution, but screenshot evidence remains mandatory.

### M1E — IntegratedShellChrome

Allowed files (5): new `Views/ShellWindowChromeController.cs`, `Views/ShellWindow.xaml`, `Views/ShellWindow.xaml.cs`, `Themes/ShellTheme.xaml`, `Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`.

Keep the predecessor M1E rollback archive/hash gate and fail-closed native behavior matrix.

### M1F-R1 — AvalonDockFloatingHostChrome

Allowed files (5): new `Views/ShellDockFloatingChromeController.cs`, `Views/ShellWindow.xaml.cs`, `Themes/ShellTheme.xaml`, `Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`, `Tests/IDE/IdeVisualSystemBoundaryTests.cs`.

Keep the predecessor M1F rollback archive/hash gate; add the single-visible-title and hide-not-destroy invariants.

### UI-DOCK-5A / 5B

Execute the separate successor contract after M1F-R1. This is the only card group allowed to change Search Home and layout-file generation.

### M1D-R2S — SearchCommandSurface

Allowed files (4): `Views/SearchToolView.xaml`, `Themes/ShellTheme.xaml`, `Tests/IDE/IdeShellBoundaryTests.cs`, `Tests/IDE/IdeVisualSystemBoundaryTests.cs`.

Replace the visible mock result table with the light vertical unavailable command surface. Do not alter Search execution semantics.

### M1G — VerificationAndGovernanceClosure

No new runtime feature. Run solution restore/build, targeted UI/Dock tests, full non-UI tests, real WPF smoke, and IdeOnly source package; then flush the stage ledger, automation-contract entry, technical debt, decisions, current status, and context capsule.

## 7. Hard stops

Stop the package if any card loses native Snap/system behavior, AvalonDock drag guides/re-docking, content identity, close-to-hide semantics, v1/v2 safety, editor/AI behavior, or requires weakening an existing assertion. A failed card produces a partial Stage Result Ledger and rollback/handoff rather than continuing.

## 8. Completion record

The accepted light-layout target is implemented through M1D-R2A/B/C, M1E, M1F-R1, UI-DOCK-5A/B, M1D-R2S, and M1G. The Search automation list above includes the contract correction from the approved visual composition: file pattern is an editable ComboBox and the approved previous-result command has its own anchor.

| Stage | Result | Evidence |
|---|---|---|
| M1D-R2A/B/C | Completed | Modern menu/DataGrid foundations and Shell workspace/bottom/right surfaces are covered by visual and Shell boundary tests. |
| M1E | Completed | Project-owned non-transparent Shell chrome, native system commands, title/menu density and stable caption AutomationIds are present. |
| M1F-R1 | Completed | Project-owned floating chrome, single visible title and hide-not-destroy behavior are implemented; rollback archive retained under `artifacts/`. |
| UI-DOCK-5A/B | Completed | Search Floating home, v2 authority, v1 migration, geometry recovery and Bottom exclusion are implemented. |
| M1D-R2S | Completed | Visible Mock query/results/status removed; light vertical unavailable command surface implemented. |
| M1G | Completed | Restore/build, 2313/2313 non-UI tests, 1/1 two-process Search UI smoke and 951-file IdeOnly package passed. |

Known verification limits:

- physical 1280 x 800 and 125%/150% DPI switching were not available; responsive XAML and geometry bounds are statically covered;
- AvalonDock 4.74.1 exposes the floating host chrome to UIA but does not expose the hosted Search content subtree through the top-level floating HWND in the current template. Static AutomationId contracts remain covered; runtime floating-content accessibility needs a separately contracted narrow fix.
