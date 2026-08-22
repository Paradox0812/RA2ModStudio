# UI-DOCK-4 Layout Persistence Final Contract

Status: Final contract approved by the user on 2026-07-22. UI-DOCK-4A through UI-DOCK-4F completed; the package is closed pending only optional manual multi-monitor/DPI coverage on suitable hardware.

Risk: `R4` — versioned presentation persistence, AvalonDock layout-model replacement, Shell lifecycle ordering, compatibility fallback, and monitor-coordinate recovery.

Authoritative predecessors:

- `Docs/UI-DOCK-1_AvalonDockShellContract.md`
- `Docs/UI-DOCK-1_AvalonDockExactApiInventory.md`
- `Docs/UI-MODERN-1_WpfDimensionSpec.md`
- `Docs/UI-MODERN-1_ResponsiveLayoutSpec.md`

## 1. Functional goal

UI-DOCK-4 persists and restores the user's AvalonDock presentation layout across a normal application restart while preserving the existing Shell-owned content instances and every approved UI-DOCK-3R recovery command.

The stage must support:

- dock topology, tab order, selected/active content, visibility, auto-hide, dock dimensions, floating state, floating size, and floating position;
- the seven approved `ContentId` values only;
- deterministic fallback to the compiled default layout when the persisted file is absent, invalid, incompatible, unsafe, or cannot be restored;
- monitor-aware clamping when a saved floating window is no longer reachable;
- immediate persistence of the compiled default after `Reset Default Layout` succeeds;
- normal close/restart restoration without recreating tool views or view models.

## 2. Non-goals

UI-DOCK-4 does not add:

- named layout profiles, layout import/export, cloud sync, roaming settings, or a Save Layout command;
- continuous/debounced save on every drag operation;
- Search implementation or placeholder removal;
- control modernization, custom floating-window chrome, dark theme, or Shell visual redesign;
- AvalonDock upgrade, extra theme package, MVVM package, DI package, or Windows Forms dependency;
- persistence of editor text, caret, selection, dirty/undo state, current file, project selection, Search results, Issues, Output content, Find References results, AI conversation, AI streaming state, Field Registry data, or view-model state;
- parser, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, AI transport, model selection, or BuiltIn data changes;
- public API or external extension points.

## 3. Architecture decisions

### 3.1 Compiled default authority

`ShellWindow.xaml` is the only authority for the default layout topology, default group dimensions, model capabilities, titles/bindings, and initial content composition.

After the existing default initialization has run, UI-DOCK-4 captures an in-memory AvalonDock XML snapshot of that compiled layout. `Reset Default Layout` restores this snapshot through the same validated restore pipeline used for the persisted layout.

No second hand-built C# default layout tree is allowed.

### 3.2 Content ownership

Shell remains the composition root and owns the complete lifetime of all existing content views and view models. AvalonDock owns only presentation models and presentation topology.

`XmlLayoutSerializer` may replace `LayoutRoot`, `LayoutDocument`, `LayoutAnchorable`, Pane, Group, and floating-window models. It must reconnect only the already-existing Shell-owned `.Content` instances through the allow-listed `ContentId` mapping.

Deserialization must never construct application content, view models, services, or arbitrary project types.

### 3.3 Dynamic identity

Long-lived runtime behavior is keyed by `ContentId`, not by localized title, XAML-generated model field, Pane reference, Pane `Name`, tree position, or serialized object reference.

`LayoutAnchorablePane` and `LayoutAnchorablePaneGroup` are not stable persisted identities. A Pane `Name`, when present, is presentation metadata and is not an authority boundary.

### 3.4 Native serializer boundary

UI-DOCK-4 uses the pinned `Dirkster.AvalonDock 4.74.1` `XmlLayoutSerializer` for topology fidelity. A parallel custom JSON/tree schema is forbidden because it would duplicate AvalonDock topology rules and create a second layout implementation.

The native XML is treated as untrusted local input and is wrapped by project-owned size, XML-reader, ContentId, invariant, failure, and monitor checks.

### 3.5 Rejected alternatives

- Retaining the current fixed `LayoutRoot`/Pane/model references after deserialization: rejected because AvalonDock replaces the layout model graph.
- Reassigning XAML-generated fields to restored models: rejected because it does not update the XAML namescope and creates two competing identity mechanisms.
- Persisting localized titles as identity: rejected because localization and title changes would break restore.
- Using Pane `Name` as the Home identity: rejected because user-created/rearranged topology may not contain the original Pane and empty Panes may be collected.
- Maintaining a manual C# clone of the default XAML tree: rejected because XAML and reset topology would drift.
- Introducing AvalonDock 5.x or a new docking/theme package: rejected by the pinned dependency contract.

## 4. Data ownership and lifetime

| Concept | Primary owner | Lifetime | Serialized | Identity / lookup |
|---|---|---|---|---|
| Compiled default topology and invariants | `ShellWindow.xaml` | application version | captured in memory only | approved `ContentId` plus compiled structure |
| Tool/editor content instances | Shell | one Shell instance | never | existing object reference, indexed by `ContentId` |
| Current AvalonDock model graph | `DockingManager` | replaceable during Shell lifetime | native XML representation only | current-layout traversal by `ContentId` |
| Tool Home metadata | internal compiled profile | application version | never trusted from XML | `ContentId` + `Bottom`/`Right` |
| User layout XML | internal layout store | per local Windows user, across restarts | yes | schema filename `v1` |
| Compiled-default XML snapshot | internal layout session | one Shell instance | no persistent authority | in-memory string/bytes |
| Restore/save result | internal presentation result | one operation | no | internal failure kind |
| Monitor work areas | internal runtime provider | one restore operation | no | current Windows monitor set |
| Bottom collapse snapshot | Shell transient state | current collapse cycle | never | set of bottom-tool `ContentId` values |

Derived runtime maps, monitor data, event subscriptions, and collapse snapshots must not be serialized.

## 5. Stable content and Home contract

| ContentId | Required model | Home | Default order | Default visible | Preferred floating size (DIP) |
|---|---|---|---:|---|---:|
| `Document.Source` | `LayoutDocument` | Document | 0 | yes | not floating |
| `Tool.Problems` | `LayoutAnchorable` | Bottom | 0 | yes | 880 x 460 |
| `Tool.Output` | `LayoutAnchorable` | Bottom | 1 | yes, default active | 800 x 420 |
| `Tool.Search` | `LayoutAnchorable` | Bottom | 2 | yes | 800 x 420 |
| `Tool.FindReferences` | `LayoutAnchorable` | Bottom | 3 | no | 700 x 460 |
| `Tool.SectionExplorer` | `LayoutAnchorable` | Right | 0 | yes, default selected | 320 x 720 |
| `Tool.AiAssistant` | `LayoutAnchorable` | Right | 1 | yes | 360 x 760 |

Every accepted layout must contain exactly one instance of each ContentId and no other `LayoutContent` identity.

## 6. Persistence schema and file contract

### 6.1 Location and version

```text
%LOCALAPPDATA%\RA2IniEditor\IDE\Layout\shell-layout.v1.xml
```

- The layout is local-machine presentation state and must not use roaming `ApplicationData`.
- The `v1` filename is the schema/version boundary.
- An incompatible future format must use a new filename such as `shell-layout.v2.xml` and a separately approved migration contract.
- UI-DOCK-4 does not scan or guess other versions.

### 6.2 Encoding and bounds

- UTF-8 without BOM.
- Maximum file length: 1 MiB before XML parsing.
- XML declaration may be omitted; if present, it must agree with UTF-8.
- Writes reuse `RA2IniEditor.Infrastructure.IO.AtomicTextFileWriter` with a same-directory unique temporary file.
- A failed write must not delete or truncate the last valid file.
- Multiple application instances use bounded last-successful-close-wins semantics; no process-wide lock is added.

### 6.3 Safe XML input

The file must be read through an `XmlReader` configured with:

- `DtdProcessing = Prohibit`;
- `XmlResolver = null`;
- bounded document characters consistent with the 1 MiB file limit;
- no direct `Deserialize(string filePath)` call on unchecked input.

Before AvalonDock deserialization, a safe preflight pass must verify:

- expected AvalonDock layout root;
- exactly one occurrence of every approved ContentId;
- `Document.Source` is represented as a document;
- all six tools are represented as anchorables;
- no unknown, empty, or duplicate `LayoutContent.ContentId`;
- no malformed, non-finite, or structurally impossible required values that can be rejected before model replacement.

After deserialization, a second invariant validation is mandatory.

### 6.4 Invalid-file handling

An invalid or incompatible file may be moved to the single bounded diagnostic path:

```text
%LOCALAPPDATA%\RA2IniEditor\IDE\Layout\shell-layout.v1.invalid.xml
```

The previous `.invalid` file may be overwritten. Quarantine failure must not block compiled-default startup.

Raw XML, absolute user paths, or full exception text must not be surfaced in normal UI status.

## 7. Runtime invariant rebinding

The restore callback and post-restore binder must reconnect the original Shell-owned content objects and reapply compiled invariants that persisted XML is not allowed to control.

Required invariants include:

- exact ContentId and expected model type;
- exact original `.Content` object reference;
- localized title source;
- the live `Document.Source` title Binding;
- `Document.Source`: `CanClose=false`, `CanFloat=false`, `CanMove=false`;
- tool `CanClose`, `CanFloat`, `CanHide`, `CanMove`, `CanAutoHide`, and `CanDockAsTabbedDocument` values from the compiled baseline;
- content-model AutomationIds;
- deterministic header/tab AutomationIds already derived by templates from ContentId;
- required event subscriptions, especially Section Explorer visibility synchronization.

Persisted capability flags, titles, AutomationIds, or Binding representations are never authoritative.

## 8. Proposed internal types and methods

Names may receive private implementation-level adjustments only if the responsibilities and boundaries below remain unchanged. No type or member may be made public.

### 8.1 `ShellDockToolProfile`

The current model-reference profile is replaced by immutable metadata:

```csharp
internal readonly record struct ShellDockToolProfile(
    string ContentId,
    ShellDockHomeZone HomeZone,
    int DefaultOrder,
    bool DefaultVisible,
    double PreferredFloatingWidth,
    double PreferredFloatingHeight);
```

`ShellDockHomeZone` contains only `Bottom` and `Right` for tools.

### 8.2 `ShellDockLayoutCoordinator`

Responsibilities:

- resolve current tools from `DockingManager.Layout` by ContentId for every operation;
- show and activate current models;
- recover floating-close behavior;
- return floating tools Home;
- choose an existing non-floating Pane containing the same Home-zone tools, or use `LayoutAnchorable.AddToLayout` with `Bottom`/`Right` when no Home Pane exists;
- preserve re-entrancy and Shell-close guards;
- never read or write files;
- never own content views or view models;
- no longer treat the constructor-time model graph as permanent.

### 8.3 `ShellDockLayoutSession`

Responsibilities:

- capture the original content catalog and compiled invariant metadata;
- capture the compiled-default in-memory layout XML after default initialization;
- serialize the current valid layout to memory;
- validate and restore persisted/default XML;
- rebind content, bindings, capabilities, AutomationIds, and events after model replacement;
- provide current-model lookup by ContentId;
- return internal operation results without exposing raw exceptions.

### 8.4 `ShellDockLayoutStore`

Responsibilities:

- own the v1 LocalAppData paths;
- enforce length and UTF-8 rules;
- return bounded read/write/quarantine results;
- reuse `AtomicTextFileWriter`;
- contain no AvalonDock topology or Shell behavior.

### 8.5 `ShellMonitorWorkAreaProvider`

Responsibilities:

- enumerate current Windows monitor work areas in WPF DIP coordinates;
- identify the Shell's current monitor as deterministic fallback;
- isolate any narrowly scoped Windows API calls;
- return a safe fallback instead of throwing through Shell startup.

### 8.6 Internal operation result

The internal result must distinguish at least:

- `Success`;
- `NotFound`;
- `UnsupportedVersion`;
- `TooLarge`;
- `UnsafeXml`;
- `InvalidContentIdentity`;
- `InvalidLayoutInvariant`;
- `IoFailure`;
- `SerializerFailure`;
- `MonitorFallbackApplied` as non-fatal diagnostic state or equivalent flag.

The result is presentation-only and is not public or serialized.

## 9. Shell lifecycle contract

### 9.1 Startup

The exact order is:

```text
InitializeComponent
→ create profile/catalog services from the compiled model
→ Loaded: apply the existing compiled default initialization
→ capture the compiled-default in-memory snapshot
→ read and preflight the v1 local file
→ deserialize only if preflight passes
→ rebind content and compiled invariants
→ validate the replaced layout graph
→ clamp floating geometry
→ refresh coordinator/event bindings
→ derive Shell parallel state from the accepted layout
→ first usable presentation
```

The existing unconditional Find References hide and Output activation apply only while constructing the compiled default. They must not execute again after a successful user-layout restore.

If no persisted layout exists, the initialized compiled default remains active.

### 9.2 Shell state synchronization

After every complete model replacement:

- clear the transient Bottom collapse snapshot;
- set Bottom-expanded state from whether any Bottom tool is visible;
- select `_lastActiveBottomToolContentId` from the current active/selected Bottom tool, then the first visible Bottom tool, then `Tool.Output`;
- synchronize `ShellViewModel.IsProjectExplorerVisible` with the current `Tool.SectionExplorer` visibility under the existing recursion guard;
- detach Section Explorer visibility handling from the old model and attach it exactly once to the current model;
- do not alter AI, editor, Search, Issues, Output, or Find References content state.

### 9.3 Close and save

The exact order is:

```text
ShellDockLayoutCoordinator.BeginShellClose
→ base.OnClosing(e)
→ if e.Cancel: CancelShellClose and do not save
→ otherwise validate and serialize the still-live current layout
→ atomically save v1
→ continue normal close
```

Save failure must not prevent application exit and must preserve the last valid file.

### 9.4 Reset Default Layout

The existing toolbar and View-menu command must:

1. restore the captured compiled-default snapshot through the validated session pipeline;
2. rebind all content and event identities;
3. restore 300-DIP Right / 260-DIP Bottom geometry, default order/visibility, Section Explorer selection, and Output activation;
4. validate the result;
5. immediately atomically overwrite v1 with that accepted default layout;
6. report a bounded non-fatal status if persistence fails.

Three consecutive resets must remain idempotent.

## 10. Monitor and coordinate recovery

- Legitimate negative coordinates are accepted when they intersect a current monitor work area.
- NaN, Infinity, non-positive sizes, and values outside safe numeric bounds are invalid.
- Select the current work area having the largest intersection with the saved floating rectangle.
- Keep at least a 64 x 32 DIP title/drag region reachable.
- Clamp an oversized floating window to the chosen work area with a 16-DIP safety inset.
- If no current work area intersects the saved rectangle, DPI conversion is unreliable, or monitor enumeration fails, use the profile's preferred floating size and center it on the Shell monitor.
- Monitor identity is not persisted in v1. Current work areas are runtime-derived presentation data.
- Per-monitor conversion must be proven against the current WPF/Windows coordinate behavior in UI-DOCK-4E; failure to establish reliable conversion triggers fallback, not guessed arithmetic.

## 11. Automation contract

No new visible control is introduced and no new AutomationId is required.

The following anchors remain frozen:

- `Shell.BottomToolTabs`
- `Shell.Dock.Tool.Problems`
- `Shell.Dock.Tool.Output`
- `Shell.Dock.Tool.Search`
- `Shell.Dock.Tool.FindReferences`
- `RightToolWell.Root`
- `RightToolWell.ActiveView`
- `RightToolWell.SectionTab`
- `RightToolWell.AiTab`
- `Shell.MainToolbar.WindowLayoutButton`
- `Shell.Menu.WindowLayout`
- `Shell.WindowLayout.ReturnFloatingToolsHome`
- `Shell.WindowLayout.ResetDefaultLayout`
- the existing View-menu mirrors
- `Shell.Dock.Tab.{ContentId}`
- `Shell.Dock.Header.{ContentId}`

After arbitrary user rearrangement, Pane-level AutomationIds bind to the deterministic primary non-floating Pane for that Home zone when such a Pane exists. Content-level and rendered tab/header identities remain ContentId-based.

## 12. Public API and compatibility contract

- External public API change: none.
- Existing public signatures: unchanged.
- New classes, enums, records, methods, and result types: `internal` or private only.
- Persisted v1 XML is an internal, best-effort presentation compatibility contract, not an import/export format.
- A future ContentId addition/removal, topology-version change, or migration requires a separately reviewed schema/version amendment.
- No `PublicApiLedger` entry is required unless implementation unexpectedly introduces a public or externally serialized contract beyond the approved v1 file.

## 13. Allowed files

Production implementation may modify only:

- `RA2IniEditor.IDE/Views/ShellDockLayoutCoordinator.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- new internal `RA2IniEditor.IDE/Views/ShellDockLayoutSession.cs`
- new internal `RA2IniEditor.IDE/Views/ShellDockLayoutStore.cs`
- new internal `RA2IniEditor.IDE/Views/ShellMonitorWorkAreaProvider.cs`

Tests may modify/add only:

- `RA2IniEditor.Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`
- narrowly named `RA2IniEditor.Tests/IDE/ShellDockLayout*Tests.cs`

Governance flush may update only:

- this contract;
- `Docs/UI-DOCK-1_AvalonDockShellContract.md`;
- `Docs/UI-DOCK-1_AvalonDockExactApiInventory.md`;
- `Docs/Codex_CurrentPhase.md`;
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`;
- product-facing release/user/smoke documents only if verified user-visible behavior changes require them at final closure.

Each implementation Task Card must remain within the project default budget: at most five modified files and at most two new classes per card.

## 14. Forbidden files and behavior

Without a contract amendment and user approval, do not modify:

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`;
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`;
- any other XAML or theme resource;
- `RA2IniEditor.IDE/RA2IniEditor.IDE.csproj`, solution files, package references, or AvalonDock version;
- Search/Find References view-model or business behavior;
- AI code, Field Registry code/data, parser, editor core, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, or undo/redo;
- legacy projects or legacy UI;
- Infrastructure source unless a separately proven defect prevents reuse of the existing atomic writer.

Do not weaken existing UI-DOCK-3R assertions merely to accommodate persistence. Tests may be updated only when the old assertion encodes the intentionally superseded fixed-model implementation rather than approved behavior.

## 15. Continuous StagePackage

### UI-DOCK-4A — SerializerReplacementProof

Scope: tests/proof only; no production persistence.

Must prove against AvalonDock 4.74.1:

- deserialize replaces `LayoutRoot` and layout-content model instances;
- callback reuses the original `.Content` objects;
- callback cancellation rejects unknown content;
- Pane/Group identity cannot be treated as durable Home identity;
- `AddToLayout(Bottom/Right)` provides the approved fallback;
- compiled-default round-trip is viable.

Failure stops the package before production refactoring.

### UI-DOCK-4B — LiveIdentityAndBaselineReset

Scope: dynamic ContentId resolution, in-memory compiled-default session, invariant/event rebinding, and baseline Reset. No disk persistence.

Acceptance:

- all current Shell commands operate on the current model graph after one and multiple replacements;
- content instances are reference-equal before and after replacement;
- existing UI-DOCK-3R floating-close, batch-return, and idempotent reset behavior remains intact;
- no business state changes.

### UI-DOCK-4C — VersionedAtomicStore

Scope: v1 paths, UTF-8/size bounds, safe read, atomic write, bounded quarantine, and store tests. No Shell startup/close integration yet.

Acceptance:

- valid file round-trip;
- missing, over-limit, invalid UTF-8, DTD, malformed XML, and I/O failure results are bounded;
- prior valid file survives failed write;
- no new dependency or public API.

This is the first governance flush point after three Task Cards (`4A` through `4C`).

### UI-DOCK-4D — RestoreSaveLifecycle

Scope: Loaded restore sequence, Shell state derivation, close/cancel save ordering, immediate Reset persistence, and event rebinding.

Acceptance:

- valid persisted layout restores without default-state overwrite;
- cancelled close does not write;
- normal close writes once from the accepted live graph;
- Reset persists default immediately;
- load/save failure never produces a blank Shell or blocks close.

### UI-DOCK-4E — MonitorClampAndRecovery

Scope: per-monitor DIP work areas, finite geometry checks, disconnected-monitor fallback, and coordinate tests.

Acceptance:

- valid negative coordinates remain valid;
- disconnected/off-screen windows return to the Shell monitor;
- title/drag region remains reachable;
- 100%, 125%, and 150% smoke does not rely on guessed cross-monitor scaling.

### UI-DOCK-4F — RestartVerificationAndClosure

Scope: regression, real-process restart smoke, manual multi-monitor/DPI smoke, full verification, clean package, and documentation closure.

The package stops for user visual/manual review after 4F. There is no visual redesign inside UI-DOCK-4.

## 16. Verification matrix

### Automated

- seven allow-listed identities: exactly once, correct model type;
- unknown, duplicate, missing, or empty ContentId rejection;
- original content instance preservation;
- Source Document capability and live title Binding restoration;
- capability and AutomationId rebinding;
- default, docked, reordered, hidden, auto-hidden, floating, selected, and active state round-trip;
- dynamic Shell commands after model replacement;
- repeated Reset idempotence;
- safe XML and size-limit cases;
- atomic-write preservation;
- close-cancel and successful-close ordering;
- finite/off-screen/negative-coordinate geometry cases;
- persisted layout failure followed by compiled-default recovery.

### Manual/runtime

- 1920 x 1080 primary layout;
- 1280 x 800 fallback;
- 100%, 125%, and 150% Windows scaling where available;
- secondary monitor to the left of primary;
- save a floating layout on secondary monitor, close, disconnect secondary monitor, restart;
- normal close/restart twice;
- Reset, terminate without another normal close, restart;
- Search, Find References, Problems, Output, Project Explorer, and AI commands after restore;
- floating-close recovery and Return Floating Tools Home after restore.

### Commands

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Ra2ShellIdeLayoutBoundaryTests|FullyQualifiedName~ShellDockLayout"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 17. Stop conditions

Stop continuous execution and report before expanding scope if:

- UI-DOCK-4A contradicts any serializer/model-replacement assumption;
- preserving the same content instances is not reliable;
- compiled-default recovery cannot be proven before disk persistence;
- implementation requires `ShellWindow.xaml`, theme, project-file, package-version, public API, or new-dependency changes;
- safe monitor-coordinate conversion would require guessed arithmetic or broad OS integration;
- a required test can pass only by weakening approved behavior;
- build/targeted tests/full tests/package fail and the fix exceeds the current Task Card;
- the actual change rises beyond this R4 contract or conflicts with `AGENTS.md` semantic boundaries.

## 18. DeepSeek boundary

No UI-DOCK-4 production card is delegated to DeepSeek by default. The core work changes framework-model identity, persistence, fallback, and Shell lifecycle and therefore remains Codex-owned.

A future DeepSeek task would require a separately generated packet and may cover only isolated test-case generation or documentation after Codex supplies the exact real APIs. DeepSeek must not design the schema, lifecycle, identity registry, monitor policy, or public/internal API.

## 19. Governance and approval gate

- Contract risk: R4.
- Contract state: approved by the user on 2026-07-22.
- Implementation mode after approval: continuous StagePackage `4A` through `4F`, with self-review after every card.
- Governance flushes: after `4C`, at any stop condition, and after `4F` completion.
- Public API ledger: no entry expected; existing contract reused.
- Decision record: this contract records the accepted design and rejected alternatives; no parallel DecisionLog is created.
- Technical debt: none accepted by the contract. Any shortcut, partial verification, or compatibility adapter discovered during implementation must be registered or cause a stop.

The user approval gate is satisfied. Each Task Card still obeys its own verification and stop conditions.

## 20. UI-DOCK-4A result — completed 2026-07-22

- Added black-box contract tests against the pinned AvalonDock 4.74.1 serializer and layout APIs.
- Proved that deserialization replaces `LayoutRoot`, `LayoutDocument`, and `LayoutAnchorable` model instances while preserving the original callback-resolved `.Content` object references.
- Proved that `LayoutSerializationCallbackEventArgs.Cancel` removes an unknown ContentId from the restored graph.
- Proved a complete seven-ContentId Document/Bottom/Right contract topology round-trip.
- Proved `LayoutAnchorable.AddToLayout` can create distinct Bottom and Right fallback Panes.
- Targeted `Ra2ShellIdeLayoutBoundaryTests` passed 12/12; build emitted only the pre-existing CS8602 warning in `BuiltInFieldRegistryPackLoaderTests.cs:1961`.
- No production C#, XAML, theme, project, persistence, or business-semantic file changed.

Next entry: `UI-DOCK-4B-LiveIdentityAndBaselineReset`.

## 21. UI-DOCK-4B result — completed 2026-07-22

- Replaced long-lived AvalonDock model references with current-layout resolution by stable `ContentId`.
- Added the internal `ShellDockLayoutSession` to capture the compiled default, restore through the native serializer, preserve Shell-owned content instances, and reapply compiled capabilities, bindings, and AutomationIds.
- Proved repeated default Reset is idempotent, hidden managed tools remain in the snapshot, and a coordinator created before model replacement still resolves the current graph.
- `Ra2ShellIdeLayoutBoundaryTests` passed 13/13 and the combined Shell filter passed 113/113.
- No disk persistence, XAML, dependency, public API, or business-semantic change was introduced.

## 22. UI-DOCK-4C result — completed 2026-07-22

- Added the internal v1 user-local layout store at the approved LocalAppData path.
- Reused `AtomicTextFileWriter`; writes are UTF-8 without BOM, bounded to 1 MiB, and preserve the prior valid file on a failed write.
- Safe reads reject BOM, invalid UTF-8, non-UTF-8 declarations, DTDs, malformed XML, unexpected roots, and over-limit files before AvalonDock sees the input.
- Invalid layouts may be moved to the one bounded `.invalid.xml` diagnostic path.
- Store and serializer-boundary tests passed 23/23. The only warning observed while compiling the tests was the pre-existing CS8602 warning in `BuiltInFieldRegistryPackLoaderTests.cs:1961`.
- No Shell startup/close integration, XAML, dependency, project-file, public API, or business-semantic change was introduced.

Next entry: `UI-DOCK-4D-RestoreSaveLifecycle`.

## 23. UI-DOCK-4D result — completed 2026-07-22

- Integrated safe startup restore after compiled-default capture, with identity preflight before AvalonDock model replacement.
- Invalid, incompatible, unsafe, or failed restores keep or restore the compiled default and use the single bounded quarantine path.
- Rebound Section Explorer visibility and derived Bottom visibility/last-active Shell state after every accepted replacement.
- Cancelled close performs no write; an accepted close serializes and atomically saves once; Reset immediately persists the accepted compiled default.
- Dock/lifecycle targeted tests passed 25/25 and the combined Shell filter passed 125/125.

## 24. UI-DOCK-4E result — completed 2026-07-22

- Added an internal Windows monitor work-area provider and pure floating-geometry recovery rules.
- Valid negative coordinates remain when they intersect a connected same-DPI monitor; off-screen, invalid, or oversized rectangles are recovered with the required reachable title region and safety inset.
- Mixed-DPI cross-monitor coordinates are treated as unreliable and fall back to the Shell monitor instead of using guessed scaling.
- Geometry/Dock targeted tests passed 34/34 and the combined Shell filter passed 134/134.

## 25. UI-DOCK-4F result — completed 2026-07-22

- Added a permanent session-to-versioned-store integration test and retained the existing IDE legacy-save guard without weakening it.
- A narrowly scoped internal `AtomicTextFileWriter.WriteAtomically` alias allows approved presentation persistence while the existing `WriteText` entry remains a compatibility delegate; write semantics are unchanged.
- Real Debug-process smoke proved default Reset, normal-close persistence, a second usable startup, exactly one of each seven approved ContentIds, UTF-8 without BOM, and absence of business-state markers. Because Computer Use launched the process from packaged Codex, Windows virtualized LocalAppData under the Codex package `LocalCache\\Local`; ordinary desktop startup continues to use the contracted `%LOCALAPPDATA%` path.
- Final automated evidence: restore passed; Debug solution build passed with zero errors; Dock targeted tests passed 35/35; full non-UI suite passed 2305/2305; IdeOnly clean package passed with 940 source files.
- Manual disconnected-secondary-monitor and explicit 125%/150% scaling smokes were not available on the current hardware/session. Their unsafe cases remain covered by deterministic geometry tests and conservative fallback.
- No XAML/theme/project/dependency/public API, Search, AI, Field Registry, parser, Completion, Hover, Diagnostics, Save Preflight, editor, or legacy behavior changed.

Package result: `UI-DOCK-4` completed. The next UI line is the separately gated modern-control/XAML work; do not start it without its own visual-stage approval.
