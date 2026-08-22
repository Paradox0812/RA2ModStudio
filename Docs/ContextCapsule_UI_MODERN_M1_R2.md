# Context Capsule — UI-MODERN-M1-R2 / UI-DOCK-5

## 1. Scope

- Project: RA2IniEditor.IDE-only
- Completed package: UI-MODERN-M1-R2 plus UI-DOCK-5
- Updated: 2026-07-22

## 2. Current product state

- Shell and AvalonDock floating hosts use the project-owned light modern visual system.
- Seven ContentIds remain exact and Shell-owned.
- Search is `Tool.Search`, hidden by default, Floating home, preferred 560 x 620 DIP.
- Bottom tools are Problems, Output, and on-demand Find References; Search does not participate in Bottom collapse state.
- `shell-layout.v2.xml` is authoritative; v1 is read only for one-way migration.
- Search shows an unavailable command surface without sample query/results/count; no real search or replace exists.

## 3. Key invariants

- Do not restore `RA2IniEditor.sln`, the legacy table editor, or old standalone Search/Find windows.
- Do not change parser, Field Registry, Completion, Hover, Diagnostics, Save Preflight, editor or AI semantics from this UI baseline.
- Keep AvalonDock 4.74.1, exact content identity validation, strict UTF-8/no-BOM, 1-MiB limit, atomic writes and Shell-owned content rebinding.
- Never immediately hide a newly floated anchorable in the same call stack as `Float()`; wait for AvalonDock's dispatcher-created host.

## 4. Verification baseline

| Check | Result |
|---|---|
| IDE-only restore | Passed |
| Debug solution build | Passed, 0 warnings / 0 errors |
| Full non-UI tests | 2313/2313 passed |
| Search hide/reopen and hidden-v2 restart UI smoke | 1/1 passed |
| IdeOnly clean package | Passed, 951 files |

Screenshot: `artifacts/UI-DOCK-5-search-smoke.png`.

## 5. Key files

- `Docs/UI-MODERN-M1-R2_PreviewParityContract.md`: completed visual contract and stage ledger.
- `Docs/UI-DOCK-5_SearchFloatingTopologyContract.md`: completed topology/migration contract.
- `Views/ShellDockLayoutCoordinator.cs`: Home placement, visibility phases and geometry.
- `Views/ShellDockLayoutSession.cs`: exact identity/content restore and derived Home automation.
- `Views/ShellDockLayoutStore.cs`: v2 store and v1 migration source.
- `Views/ShellWindow.xaml(.cs)`: Shell composition, async startup/migration and Search activation.
- `Views/SearchToolView.xaml`: unavailable Search command surface.

## 6. Open risks and debt

- `UI-MODERN-M1-A11Y-001`: AvalonDock floating HWND exposes custom chrome AutomationIds but not hosted Search content AutomationIds. Do not synthesize a replacement peer tree casually; contract and test the narrow bridge first.
- Physical 1280 x 800 and 125%/150% DPI smoke are NotRun on current hardware; bounded layout and ScrollViewer behavior are statically present.
- Search ViewModel compatibility members remain public and empty until SEARCH-1 owns their cleanup.

## 7. Next recommended task

Stage: `UI-MODERN-M1-H1 FloatingContentAutomationAccessibility`.

Allowed scope: floating-host automation boundary, focused UI automation test, contract/debt closure.

Forbidden scope: visual redesign, new dependency, dock engine replacement, Search execution/results, parser/editor/AI/Field Registry semantics, public API expansion, legacy.

After H1, begin a separately confirmed `SEARCH-1` contract for real project-index search.
