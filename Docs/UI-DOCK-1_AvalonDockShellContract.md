# UI-DOCK-1 AvalonDock Shell Contract

Status: approved by the user on 2026-07-21; UI-DOCK-3R implementation snapshot exists and passed manual visual review, but package verification is blocked by a pre-existing AI cancellation/timeout race as of 2026-07-21.

## Dependency boundary

- Pin `Dirkster.AvalonDock` to stable version `4.74.1` for the current `net8.0-windows` application.
- Do not use AvalonDock 5.x previews or additional third-party theme/MVVM/DI packages.
- Project-owned WPF resources remain authoritative for the final visual system.

## Layout contract

At the 1920 x 1080, 100% reference canvas the top title/menu and toolbar consume 62 DIP. The 994-DIP workspace contains a 300-DIP full-height right tool group and an editor-side workspace of 1616 DIP. The editor side owns a 260-DIP bottom tool group; that group never extends below the right tool group.

The source editor is a non-closeable, non-floating document. Section Explorer and AI Assistant default to the right. Problems, Output, Search, and Find All References default to the bottom. Tool content may float, re-dock, hide, and be reopened through the existing Shell commands.

## Stable content identities

| Surface | ContentId |
|---|---|
| Source document | `Document.Source` |
| Section Explorer | `Tool.SectionExplorer` |
| AI Assistant | `Tool.AiAssistant` |
| Problems | `Tool.Problems` |
| Output | `Tool.Output` |
| Search | `Tool.Search` |
| Find All References | `Tool.FindReferences` |

Localized titles must never be used as serialized identity.

## Lifecycle contract

- Shell remains the composition root and owns all tool content instances.
- Closing a tool hides it; it does not dispose its view or view model.
- Opening a hidden, docked, or floating tool activates the same instance.
- Find All References preserves the existing language-navigation controller, result model, double-click navigation, and current-file semantics.
- Search changes host only in this package; its placeholder/business semantics are not expanded.
- AvalonDock does not own editor documents, AI requests, search results, parser state, or Field Registry state.

## Persistence boundary

Layout persistence is the separately contracted R4 `UI-DOCK-4` StagePackage. Its final contract is `Docs/UI-DOCK-4_LayoutPersistenceContract.md`, authored on 2026-07-22 and awaiting explicit user approval before implementation. It freezes a versioned user-local presentation-only file, ContentId allow-listed model rebinding, monitor clamping, compiled-default fallback, atomic writes, and a strict prohibition on serializing business/view-model data.

## Public API and semantic boundaries

No external public API is added. New types are internal. Parser, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, undo/redo, Field Registry, AI request/streaming behavior, and BuiltIn data are forbidden scope.

## Visual gates

1. Structural dock layout and interaction at 1920 x 1080.
2. Project-owned modern AvalonDock theme and exact visual parity.
3. Responsive/DPI/multi-monitor/persistence final audit.

Execution pauses for user review after each visual gate. Required build/test/package commands remain those documented in `AGENTS.md`.

## Structural gate result — 2026-07-21

`UI-DOCK-0A`, `UI-DOCK-0B`, `UI-DOCK-1A`, `UI-DOCK-1B`, and `UI-DOCK-2` are implemented through the first visual stop.

- `Dirkster.AvalonDock` is pinned to `4.74.1`; the MS-PL notice is recorded in the repository root.
- The source editor is hosted by `Document.Source` and cannot close, float, or move.
- Section Explorer / AI Assistant share the 300-DIP full-height right group.
- Problems / Output / Search / Find All References share the 260-DIP editor-side bottom group.
- Search and Find All References now use Shell-owned reusable content views; their obsolete standalone Window wrappers were removed.
- The Shell starts maximized. The compiled default activates Output and keeps Find All References hidden until invoked.
- Existing Shell commands show and activate the same dock content instance; parser, editor, Field Registry, diagnostics, save, Completion, Hover, Quick Peek, and AI request semantics were not changed.

Verification at this stop:

```text
IDE project build: passed, 0 warnings / 0 errors.
UI contract and boundary tests: passed, 58/58; one unrelated pre-existing nullable warning is emitted while compiling the full test project.
IDE-only solution build: passed, 0 warnings / 0 errors.
Full non-UI test suite: passed, 2275/2275. One timeout/cancellation race test failed once during the first full run, then passed five consecutive focused reruns and the complete rerun; no AI production code was changed.
Runtime UI smoke: passed for startup/maximize, default Output selection, Search activation/content, and Section Explorer / AI Assistant tab switching.
Reference geometry: 1920x1080 contract remains 300-DIP right / 260-DIP bottom; runtime smoke was performed on the available 2560x1440 display and at the restored 1280x800 fallback size.
Native drag-to-float automation: not claimed; CanFloat and exact Float/Dock APIs are present, but scripted pointer dragging did not reliably create a floating host. This remains a mandatory manual/runtime recheck at the next visual gate.
Clean source package: intentionally deferred until visual approval.
```

Deferred governance queue:

- `UI-DOCK-3`: manual re-docking validation for Search and Find All References remains open; scripted dragging verified floating and movement but did not reliably trigger AvalonDock's docking guides.
- `UI-DOCK-4`: versioned user-local layout persistence, allow-listed restore, monitor clamping, and default-layout fallback.
- Re-evaluate AvalonDock 5.x only after a stable release and a separate dependency contract; do not consume preview packages here.

## UI-DOCK-3R deterministic home recovery amendment — approved 2026-07-21

### Goal and non-goals

UI-DOCK-3R makes the already approved dock shell recoverable when tools are floated, hidden, or closed. It does not implement layout persistence, real Search behavior, Search control modernization, custom floating-window chrome, or any parser/AI/Field Registry/editor semantic change.

The package is split into two continuous cards and stops at one visual-review gate:

1. `UI-DOCK-3R-A`: deterministic Home mapping, floating-close recovery, empty Home reconstruction, initial floating geometry, re-entrancy and Shell-exit protection.
2. `UI-DOCK-3R-B`: toolbar and View-menu layout recovery commands with stable automation anchors.

### Stable Home and ordering contract

| ContentId | Home pane | Default order | Default visible | Initial floating size (DIP) |
|---|---|---:|---|---:|
| `Tool.Problems` | Bottom | 0 | yes | 880 x 460 |
| `Tool.Output` | Bottom | 1 | yes, default active | 800 x 420 |
| `Tool.Search` | Bottom | 2 | yes | 800 x 420 |
| `Tool.FindReferences` | Bottom | 3 | no | 700 x 460 |
| `Tool.SectionExplorer` | Right | 0 | yes, default selected | 320 x 720 |
| `Tool.AiAssistant` | Right | 1 | yes | 360 x 760 |

The mapping and ordering are keyed only by stable `ContentId`; localized titles are forbidden as identity. The compiled default remains a 300-DIP right group and a 260-DIP bottom group.

Initial floating geometry is a preferred size, not a hard maximum. It is clamped to the current Shell viewport with a 16-DIP safety inset when the available viewport is smaller. UI-DOCK-3R does not persist floating coordinates; monitor-aware restoration of saved coordinates remains UI-DOCK-4 scope.

### Lifecycle and state-preservation contract

- Closing a floating managed tool cancels AvalonDock hide, returns that same `LayoutAnchorable` instance to its stable Home, then selects and activates it.
- Closing/hiding an already docked tool preserves the existing hide behavior.
- Shell shutdown bypasses floating-close recovery. A cancelled Shell close re-enables recovery.
- If AvalonDock garbage collection removed an empty Bottom or Right Home, recovery reattaches the approved existing pane/group objects before inserting the tool.
- Repeated recovery and reset calls are idempotent: no duplicate ContentId, duplicate tab, orphan pane, or empty floating host may remain.
- Layout operations move presentation models only. They must not recreate content views/view models, cancel AI streaming, clear AI conversation, replace Search/Issues/Output state, reload Project Explorer, mutate editor text/caret/dirty/undo state, or modify files.

### Window layout command contract

The main toolbar adds one 28 x 28 DIP vector-icon command after Project Explorer and mirrors it under `View > Window Layout`.

| Command | Behavior | Toolbar popup AutomationId | View menu AutomationId |
|---|---|---|
| Return Floating Tools Home | Re-homes only currently floating managed tools; preserves hidden state and current dock sizes | `Shell.WindowLayout.ReturnFloatingToolsHome` | `Shell.Menu.WindowLayout.ReturnFloatingToolsHome` |
| Reset Default Layout | Restores all managed tools to compiled Home/order/visibility, 300/260 group geometry, Section Explorer selection, and Output activation | `Shell.WindowLayout.ResetDefaultLayout` | `Shell.Menu.WindowLayout.ResetDefaultLayout` |

The toolbar anchor is `Shell.MainToolbar.WindowLayoutButton`; the View-menu anchor is `Shell.Menu.WindowLayout`. No Save Layout, Manage Layout, placeholder item, keyboard shortcut, serialized file, registry setting, or new dependency is authorized in UI-DOCK-3R.

### Architecture and API boundary

- Shell remains the composition root and owns every existing tool instance.
- An internal layout coordinator may own Home profiles, transient re-entrancy state, and deterministic re-parenting. It must expose no external public API.
- Recovery must reuse AvalonDock `AnchorableHiding`, stable `ContentId`, `ILayoutGroup.InsertChildAt`, `ILayoutContainer.RemoveChild`, and `LayoutRoot.CollectGarbage`; custom pointer-drag code is forbidden.
- Existing drag gestures remain AvalonDock-owned. The package may enlarge the existing tab hit target but must not synthesize mouse capture or drag thresholds.
- XML/JSON layout serialization remains forbidden until UI-DOCK-4.

### Allowed files

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- one internal Shell dock-layout coordinator under `RA2IniEditor.IDE/Views/`
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml` only if the approved toolbar/tab hit target requires it
- existing Shell/AvalonDock boundary tests
- this contract, Exact API Inventory, CurrentPhase, and Full Codex Context at the package governance flush

### Forbidden files and behavior

Search view/view-model behavior, Field Registry, parser, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, editor state, AI request/streaming/model behavior, BuiltIn data, project/package references, legacy projects, and UI-DOCK-4 persistence are forbidden scope.

### Acceptance matrix

- Close one floating Bottom tool and one floating Right tool; each returns to the correct Home as the same content instance.
- Float all Bottom tools and all Right tools; each Home can be reconstructed and reset without duplicates.
- Return Floating Tools Home preserves hidden tools and current dock sizes.
- Reset Default Layout is idempotent across three consecutive calls and restores default order, visibility, selection, and 300/260 geometry.
- Layout recovery during an AI streaming request does not cancel, duplicate, or clear the request/message.
- Shell shutdown does not re-home tools while closing.
- Toolbar/menu commands expose the frozen AutomationIds and remain keyboard accessible.
- 1920 x 1080 primary smoke plus 1280 x 800 fallback smoke pass; initial floating windows are smaller than the previous inherited full-pane geometry.
- IDE build, targeted Shell tests, solution build, full non-UI tests, and manual/runtime dock smoke pass before the visual-review stop.

### Deferred from this package

- Search real behavior: `SEARCH-1`.
- shared Search/control templates: `UI-MODERN-M1A`.
- project-owned floating host chrome: `UI-MODERN-M1B`.
- versioned user-local layout persistence and saved-coordinate per-monitor restore: `UI-DOCK-4`.

## UI-DOCK-3R implementation result — verification blocked 2026-07-21

State: `Implementation Snapshot Exists / Verification Pending`.

Implemented and manually accepted:

- The approved Home profiles, deterministic ordering, empty-Home reconstruction, Shell-close bypass, and re-entrancy protection are owned by the internal `ShellDockLayoutCoordinator`.
- Closing a floating managed tool re-homes and activates the same `LayoutAnchorable`; hiding an already docked tool retains the existing hide behavior.
- The toolbar and `View > Window Layout` expose the frozen Return Floating Tools Home and Reset Default Layout commands.
- Preferred floating geometry is 880 x 460 for Problems, 800 x 420 for Output/Search, 700 x 460 for Find References, 320 x 720 for Section Explorer, and 360 x 760 for AI Assistant.
- Runtime smoke observed Search floating at 800 x 420, floating-close recovery to Bottom, toolbar batch return, and default reset. The user then explicitly confirmed the remaining manual test passed.
- No layout serialization or UI-DOCK-4 persistence was added.

Verification evidence:

```text
dotnet restore .\RA2IniEditor.IDE.sln
Passed.

dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
Passed after the verification-test anchor update: 0 errors, one pre-existing CS8602 warning in BuiltInFieldRegistryPackLoaderTests.cs:1961.

Shell targeted tests
Passed: 39/39.

Full non-UI suite
Passed after the separately authorized `AI-REL-TD-001` narrow reliability fix: two consecutive runs passed 2278/2278.
The exact regression passed 20/20 repeated runs, and the complete DeepSeek client class passed 62/62. No UI-DOCK-3R production code was changed by that fix.

IdeOnly clean package
Passed: 934 source files; `artifacts/RA2IniEditor.IDE.SourceClean.zip`.
```

During the first full run, one old Shell boundary assertion still expected `SectionExplorerAnchorable.Show()`. The production path now intentionally goes through `_dockLayoutCoordinator.ShowAndActivate(SectionExplorerAnchorable)`; that test-only assertion was updated and the combined Shell boundary suite then passed 39/39.

Deferred governance / technical debt:

| Task/Stage | Debt | Area/File | Reason Accepted Now | Impact | Suggested Resolution | Repayment Trigger | Status |
|---|---|---|---|---|---|---|---|
| `AI-REL-TD-001` | Total-timeout versus late user-cancellation first-signal attribution was scheduling-sensitive under full-suite load | `RA2IniEditor.IDE/AI/DeepSeekRa2AiClient.cs`; existing regression in `RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs` | AI transport semantics were forbidden UI-DOCK-3R scope, so the defect was isolated for a separately authorized narrow fix | Previously blocked the full-suite and release-package gate | The request-local termination owner now records the winning cause before propagating cancellation; focused 20/20, client 62/62, full suite twice 2278/2278 | Repaid on 2026-07-21 before package closure | Resolved |

`AI-REL-TD-001` is closed and the UI-DOCK-3R verification/package gate is complete. The separate UI-DOCK-4 final contract now exists at `Docs/UI-DOCK-4_LayoutPersistenceContract.md`; implementation must not start until the user explicitly approves it.

## Modern visual gate result — 2026-07-21

`UI-DOCK-3` is implemented through the second visual stop.

- `ShellTheme.xaml` now owns the AvalonDock anchorable tab, pane-title action, splitter, active accent, hover, and focus presentation; no external theme package was added.
- `ShellWindow.xaml` applies the project-owned header, title, pane, and splitter resources to the existing `DockingManager`; the approved 300-DIP right / 260-DIP bottom geometry and content identities remain unchanged.
- Rendered tool tabs and headers expose deterministic AutomationIds derived from `ContentId`: `Shell.Dock.Tab.{ContentId}` and `Shell.Dock.Header.{ContentId}`.
- Keyboard tab cycling remains contained within each active tool content surface.
- Real-window smoke verified tab switching, Search activation, horizontal and vertical splitter resizing, Search floating, floating maximize/restore, and compiled-default docking after application restart.
- Automated pointer dragging did not reliably expose AvalonDock's docking guide overlay, so Search / Find All References re-docking remains an explicit user manual check at this gate.

Verification at this stop:

```text
IDE project build: passed, 0 warnings / 0 errors.
Shell layout boundary tests: passed, 6/6.
IDE-only solution build: first attempt failed only because two smoke-test processes locked the executable; after both instances were closed, the identical command passed with 0 warnings / 0 errors.
Full non-UI test suite: passed, 2276/2276.
Runtime UI smoke: passed for modern headers/tabs, stable AutomationIds, tab switching, both splitters, floating, maximize/restore, and default-layout restart recovery.
Manual re-dock: pending user confirmation.
Clean source package: intentionally deferred until the second visual gate is approved.
```
