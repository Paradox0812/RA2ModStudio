# UI-DOCK-5 — Search Floating Topology And V2 Layout Contract

Status: completed on 2026-07-22 under the user's continuous-execution authorization.

Risk: R4 presentation persistence, migration, lifecycle, and compatibility.

## 1. Successor boundary

UI-DOCK-4 remains the historical v1 implementation contract. UI-DOCK-5 changes only Search's product-default presentation topology and introduces a v2 presentation file. It preserves every UI-DOCK-4 safety invariant: strict UTF-8 without BOM, 1-MiB bound, DTD prohibition, exact seven-identity validation, Shell-owned content instances, native AvalonDock serialization, atomic writes, cancel-close ordering, geometry recovery, and presentation-only data.

## 2. Stable identities and ownership

All seven identities remain exactly once:

```text
Document.Source
Tool.Problems
Tool.Output
Tool.Search
Tool.FindReferences
Tool.SectionExplorer
Tool.AiAssistant
```

Ownership:

| Concept | Owner | Lifetime |
|---|---|---|
| Product default Home/visibility/order/preferred size | `ShellDockToolProfile` | Shell session definition |
| Current dock tree, visibility and floating geometry | AvalonDock live model | Shell session |
| Persisted presentation topology | `shell-layout.v2.xml` | Current Windows user |
| Content/control instances and bindings | Shell compiled registrations | Shell lifetime |

No ViewModel, Search condition, editor text, AI state, or business value enters the layout file.

## 3. Profile contract

`ShellDockHomeZone` adds internal value `Floating`.

```text
Problems       Bottom   order 0   visible
Output         Bottom   order 1   visible
FindReferences Bottom   order 2   hidden
Search         Floating order 0   hidden   preferred 560 x 620
SectionExplorer Right   order 0   visible
AiAssistant     Right   order 1   visible
```

The document remains non-tool content. `GetTools(Bottom)` must never return Search after activation.

## 4. Default floating placement

Fresh default, Reset, and v1 migration position Search relative to the editor viewport rather than the virtual desktop origin:

- preferred size 560 x 620 DIP;
- horizontally centered in the editor viewport, with at most a small left bias needed to keep editor context visible;
- top begins 48–64 DIP below the editor workspace top;
- current monitor work-area clamp keeps the complete title bar reachable and retains at least a 32-DIP inset where available;
- minimum usable content target 420 x 420 DIP; smaller effective work areas use bounded scrolling rather than off-screen chrome.

Normal v2 restore uses the user's saved dock/floating geometry. Default placement is not reapplied on every open.

## 5. Show, hide, Home and Reset

- initial compiled default creates Search's floating container, applies preferred geometry, hides Search, then captures the compiled default;
- opening hidden Search calls native `Show`, activates the same model/content instance, and restores its valid previous container;
- missing/invalid floating container recovery uses native `LayoutContent.Float()` plus bounded placement; no writable `IsFloating` assumption;
- the floating close button executes AvalonDock hide semantics; it does not destroy the model or return Search to Bottom;
- if a user docks Search, hiding/reopening and v2 persistence preserve that dock choice;
- Return Floating Tools Home returns Bottom/Right-home tools to their zones; a Floating-home Search remains floating and is geometry-corrected only when needed;
- Reset Default Layout restores Problems/Output/Right defaults and Search hidden with default floating placement, then immediately writes v2.

Shell shutdown bypasses hide recovery exactly as UI-DOCK-4 requires.

## 6. V2 authority and migration

Paths:

```text
shell-layout.v2.xml
shell-layout.v2.invalid.xml
shell-layout.v1.xml
shell-layout.v1.invalid.xml
```

Rules:

1. Valid v2 is authoritative; v1 is not read.
2. With no v2, valid v1 is restored through the existing session validator.
3. After v1 restore, only Search is normalized to hidden default floating Home; the other six identities retain their effective visibility, zone, order, size and floating geometry.
4. The migrated live model is atomically serialized to v2. A successful migration never edits or deletes v1.
5. If v2 is invalid, quarantine v2 and use the compiled default; do not fall back to v1 and resurrect the obsolete topology.
6. If v1 is invalid, quarantine v1 and use the compiled default.
7. If migration serialization/write fails, keep the safe in-memory migrated/default layout, report a bounded warning, leave v1 untouched, and retry on a later startup.
8. Accepted close writes v2; cancelled close writes nothing.
9. Two application instances retain the existing last-successful-writer behavior; cross-process locking is a non-goal.

Downgrade consequence: after v2 exists, an older application may continue changing v1, but a later current application continues to prefer v2. Re-importing downgraded v1 changes is not automatic.

## 7. Home Automation

`ShellDockLayoutSession` must derive Bottom/Right candidate identities from profiles instead of hard-coded arrays. Floating profiles do not own a dock pane AutomationId. After deserialization, pane/group AutomationIds are rebound only to matching non-floating Home zones.

## 8. Allowed cards

### UI-DOCK-5A — ModelAndStoreCapability

Allowed files (4):

- `Views/ShellDockLayoutCoordinator.cs`
- `Views/ShellDockLayoutSession.cs`
- `Views/ShellDockLayoutStore.cs`
- `Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`

Add inactive-compatible Floating Home and v1/v2 store/session capability with focused tests.

### UI-DOCK-5B — ShellActivationAndMigration

Allowed files (5):

- `Views/ShellWindow.xaml`
- `Views/ShellWindow.xaml.cs`
- `Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`
- `Tests/IDE/IdeShellBoundaryTests.cs`
- `Themes/ShellTheme.xaml` only if required for the active Search host boundary

Activate the new profile, startup order, migration orchestration, close/hide exception, Bottom-state exclusion, and deterministic default placement.

## 9. Verification matrix

Automated coverage must include v1-only, v2-only, both, invalid v1, invalid v2 plus valid v1, migration write failure, atomic-write failure, all Search v1 placements, repeated startup, cancelled close, Reset, exact identities, other-tool preservation, Bottom exclusion, hide/reopen, and deterministic placement calculations.

Real WPF smoke must include float/move/resize/hide/reopen, dock/hide/reopen, restart persistence, Reset, Return Floating Tools Home, drag guides, 1920 x 1080, 1280 x 800, and one non-100% DPI scale when available.

## 10. Non-goals

No Search execution, Replace, new SearchResults ContentId, dependency, public C# API, business serialization, dark theme, secondary-window restyle, or old-version two-way synchronization.

## 11. Completion evidence

- `ShellDockHomeZone.Floating` and the accepted six-tool profiles are active; `GetTools(Bottom)` excludes Search.
- fresh/default topology creates the floating container and applies hidden visibility only after AvalonDock completes its asynchronous host creation, avoiding the verified `Float()`/immediate-`Hide()` crash.
- `shell-layout.v2.xml` is authoritative; v1 remains a read-only migration source and valid migration leaves v1 untouched.
- Search opens, closes through hide semantics, reopens floating, persists hidden state, and reopens floating after a second process start.
- Return Floating Tools Home preserves valid Search geometry and only repairs unusable or unreachable bounds.
- Debug solution build passed; Dock/Shell coverage is included in the 2313/2313 full non-UI result; the focused real WPF test passed 1/1.
- IdeOnly clean package passed with 951 files.

Remaining non-blocking verification gaps are physical 1280 x 800 / non-100% DPI coverage and AvalonDock floating-content UIA subtree reachability. The latter requires a narrow accessibility contract; attempts to synthesize or flatten AvalonDock peers were rejected and reverted.

## 12. VISUAL-FIX2 startup-presentation successor note

On 2026-07-23 the user confirmed `UI-MODERN-PROGRAM-R1 VISUAL-FIX2` after observing a brief Search-window flash during application startup.

The correction does not change this contract's topology or persistence behavior. The existing `Float()` -> dispatcher -> hidden-visibility ordering remains authoritative because immediate `Float()`/`Hide()` is unsafe. Instead, the existing floating-chrome controller temporarily suppresses rendering of hosts created during the initial topology/persisted-layout transition and restores each surviving host's prior opacity in `finally`.

No Search profile, ContentId, Home behavior, v1/v2 migration, serialization, close/hide behavior or floating geometry changed. Real no-flash behavior remains a manual visual acceptance item; see `Docs/UI-MODERN-PROGRAM-R1_VISUAL-FIX2_StageLedger.md`.
