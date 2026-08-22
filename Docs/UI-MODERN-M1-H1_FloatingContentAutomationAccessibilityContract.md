# UI-MODERN-M1-H1 — Floating Content Automation Accessibility Contract

> Successor note — 2026-07-22: `UI-MODERN-M2-R2` retired the dedicated floating-host maximize button and `Shell.Dock.FloatingHost.MaximizeRestoreButton` while preserving the main Shell maximize/restore control. The H1A identities and evidence below describe the historical probe baseline. Any future H1B must follow the current identity set in `UI-MODERN-M2-R2_CohesiveShellModernizationContract.md` and must not restore the retired floating button.

Status: approved by the user on 2026-07-22. H1A completed as an `H1A-C` evidence/failure stop; H1B is not authorized.

Risk: `R3` for the package because it crosses the AvalonDock floating-window, child-HWND, WPF UI Automation, and UI-test-harness boundary. H1A itself is evidence-first and test-only. H1B is not pre-authorized by approval of H1A.

Governance mode: `StopForReview`. The first mandatory stop is the H1A evidence decision gate. Long-term status documents are updated only after H1A closes or at a failure stop.

## 1. Authority and predecessor boundary

This contract succeeds only the unresolved accessibility item `UI-MODERN-M1-A11Y-001` recorded after `UI-MODERN-M1-R2 + UI-DOCK-5`.

It does not reopen or rewrite:

- `UI-MODERN-M1-R2_PreviewParityContract.md`;
- `UI-DOCK-5_SearchFloatingTopologyContract.md`;
- the accepted light visual result;
- Search Floating Home, v2 persistence, migration, hide/reopen, or reset behavior.

The accepted product baseline remains AvalonDock 4.74.1, seven stable ContentIds, light theme, Search hidden by default, and Search opened as an independent floating anchorable.

## 2. Verified code and dependency facts

The installed `Dirkster.AvalonDock` 4.74.1 package records source commit `028a5f328eb86a7abfb2195bbaae37683c818d1e`.

At that commit:

1. `LayoutFloatingWindowControl.ContentProperty` is coerced into `FloatingWindowContentHost`.
2. `FloatingWindowContentHost` derives from `HwndHost`.
3. `BuildWindowCore` creates a separate child `HwndSource` with `WS_CHILD | WS_VISIBLE` and installs AvalonDock content as that child source's root visual.
4. The project-owned outer floating-window template exposes the custom title chrome in the outer top-level HWND.
5. The current UI smoke enumerates only top-level windows through `EnumWindows`, converts the outer handle to a FlaUI `Window`, and searches that outer UIA subtree.
6. Therefore, failure to find Search controls under the outer UIA `Window` is insufficient evidence that the hosted content is inaccessible. It may be a child-HWND discovery defect in the test harness.

This contract deliberately does not assume that a production AutomationPeer bridge is required.

## 3. Functional goal

Determine, with repeatable evidence, whether floating AvalonDock content is reachable and semantically usable through Windows UI Automation, then apply only the smallest justified correction.

H1 succeeds when one of these terminal results is reached:

- **H1A-A — Harness correction:** Search controls are reachable through the real child HWND and through a normal Desktop/process UIA traversal. The smoke is corrected to use the real boundary; no production code changes.
- **H1A-B — Product accessibility gap proven:** direct child-HWND attachment reaches the controls, but normal Desktop/process UIA traversal cannot. H1A stops and produces an H1B design packet.
- **H1A-C — Framework/provider gap proven:** Search controls are not reachable even from the child HWND, or required control patterns are absent. H1A stops and records an upstream/framework limitation before any production proposal.

## 4. Non-goals

H1 does not implement or change:

- Search execution, Replace, project indexing, results, navigation, cancellation, or commands;
- Search ViewModel state or the temporary compatibility members owned by future `SEARCH-1`;
- visual design, theme, dimensions, title composition, spacing, icons, or responsive layout;
- Dock Home, ContentIds, default visibility, default geometry, v1/v2 migration, serialization, reset, or Return Home;
- parser, editor, AI, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup, rollback, or legacy behavior;
- AvalonDock package version, package fork, source vendoring, or new dependency;
- application-wide implicit styles or secondary-window styles.

H1 must not claim that direct `automation.FromHandle(childHwnd)` access alone proves end-user accessibility. Normal Desktop/process discovery is a separate acceptance condition.

H1A uses Search because it is the only accepted default-Floating tool and already has a stable two-process smoke. H1A evidence is authoritative for that Search surface only; it does not claim that every possible multi-pane or manually floated tool has been accessibility-certified. If H1B later changes production hosting, the implementation must be host-generic and its own verification must add at least one non-Search floating-pane sanity check.

## 5. Architecture and ownership contract

| Concern | Owner | Lifetime |
|---|---|---|
| Floating top-level HWND and model lifecycle | AvalonDock `LayoutFloatingWindowControl` | one visible floating-host lifetime |
| Hosted content child HWND | AvalonDock `FloatingWindowContentHost` / child `HwndSource` | may be recreated after hide, dock, float, or reopen |
| Search control instances and bindings | existing Shell-owned registration | Shell lifetime |
| UI Automation provider tree | WPF/UIA providers across the outer and child HWND boundaries | tied to the current HWND/provider instances |
| HWND/UIA discovery logic | UI automation test assembly only in H1A | test process lifetime |
| Layout persistence authority | existing `shell-layout.v2.xml` path | unchanged |

No new runtime state owner, cache, service, registry, adapter, DTO, or public extension point is authorized.

Automation elements and native handles are ephemeral. Tests must reacquire them after every hide/reopen, dock/float, or process restart and must not use stale FlaUI elements across those boundaries.

## 6. Stable contract-visible identities

H1 preserves exactly these existing UI Automation identities and does not add aliases:

```text
Shell.Dock.FloatingHost
Shell.Dock.FloatingHost.MinimizeButton
Shell.Dock.FloatingHost.MaximizeRestoreButton
Shell.Dock.FloatingHost.CloseButton

Search.View
Search.QueryTextBox
Search.CaseSensitiveCheckBox
Search.WholeWordCheckBox
Search.RegexCheckBox
Search.ScopeComboBox
Search.FilePatternComboBox
Search.FindPreviousButton
Search.FindNextButton
Search.FindAllButton
Search.UnavailableHint
```

No C# public API is added or changed. The AutomationId and accessibility semantics remain contract-visible behavior and must be reflected in the stage record. A separate `PublicApiLedger` file must not be invented; this contract and the completed stage record are the ledger unless implementation unexpectedly adds public C# API, which is an immediate stop.

### 6.1 Class, field, and method surface

H1A has this exact code-shape boundary:

| Kind | Contract |
|---|---|
| New production class | none |
| Modified production class | none |
| New test class | none |
| Modified test class | `Ra2IdeMainPathSmokeTests` only |
| New fields/state | none other than required static Win32 delegate/P/Invoke declarations; no handle or AutomationElement cache |
| Public/internal API | none |
| Private behavior | extend the existing floating-host discovery and Search smoke with child-HWND enumeration, Desktop/process comparison, semantic assertions, reacquisition, and bounded diagnostics |

Any need for a new class, shared fixture, production adapter, field cache, or public/internal member is outside H1A and triggers review.

## 7. H1A — ChildHwndAutomationDiscoveryAndHarnessCorrection

### 7.1 Goal

Measure the actual HWND/UIA topology and, only for terminal result H1A-A, correct the existing UI smoke so it discovers and validates the floating content through the real provider boundary.

### 7.2 Allowed files

Implementation card budget: at most 2 files.

1. `RA2IniEditor.UiAutomationTests/Ra2IdeMainPathSmokeTests.cs`
2. this contract document, only to append the evidence/result ledger at the H1A stop

The existing test file already owns `EnumWindows`, window/process filtering, desktop lookup, retry, tree formatting, and the Search open/hide/reopen smoke. H1A must extend that canonical path instead of adding another helper file.

### 7.3 Forbidden files

H1A must not modify:

- `RA2IniEditor.IDE/Views/ShellDockFloatingChromeController.cs`;
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`;
- `RA2IniEditor.IDE/Views/ShellWindow.xaml` or `.xaml.cs`;
- `RA2IniEditor.IDE/Views/SearchToolView.xaml`;
- any ViewModel, Dock coordinator/session/store, project file, package reference, source package script, or product documentation;
- any parser/editor/AI/Field Registry or protected semantic file.

### 7.4 Exact API inventory

The implementation must reuse these existing project members from `Ra2IdeMainPathSmokeTests.cs`:

```csharp
private static Window? TryWaitForFloatingHost(
    FlaUIApplication app,
    UIA3Automation automation,
    TimeSpan timeout)

private static Window? FindFloatingHost(int processId, UIA3Automation automation)

private static AutomationElement? TryFindDesktopElement(
    UIA3Automation automation,
    string automationId,
    TimeSpan timeout)

private static string FormatAutomationTree(AutomationElement root, int maxNodes)

[DllImport("user32.dll")]
private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

[DllImport("user32.dll")]
private static extern uint GetWindowThreadProcessId(IntPtr handle, out int processId);

[DllImport("user32.dll")]
private static extern bool IsWindowVisible(IntPtr handle);
```

Installed and already referenced capabilities that may be reused:

```text
FlaUI.Core 4.0.0
FlaUI.UIA3 4.0.0
UIA3Automation.FromHandle(IntPtr)
UIA3Automation.GetDesktop()
ConditionFactory.ByAutomationId(string)
ConditionFactory.ByProcessId(int)
AutomationElement.FindFirstDescendant(...)
AutomationElement.FindAllDescendants(...)
FrameworkAutomationElement.ProcessId
Win32 EnumChildWindows
```

No unlisted project helper, fake window, fake AutomationElement, reflection into AvalonDock internals, or guessed convenience API is authorized.

### 7.5 Required private responsibilities

Exact private method names are not contract-visible, but the implementation must keep these responsibilities separated:

1. Enumerate descendant HWNDs of the identified outer floating host using `EnumChildWindows`.
2. Filter every native handle by the launched IDE process ID and current liveness.
3. Attach FlaUI to each candidate handle and locate `Search.View` or `Search.QueryTextBox` without assuming a fixed class name or child index.
4. Search the Desktop UIA tree for the same AutomationId and reject elements from other process IDs.
5. reacquire all handles/elements after lifecycle transitions.
6. emit bounded diagnostic text containing the native hierarchy and relevant UIA nodes on failure.

Private helpers must remain inside the existing test class. No production wrapper or public test API is allowed.

### 7.6 Discovery order

For each lifecycle checkpoint:

1. Find the visible outer floating host by process ID and its existing chrome AutomationId.
2. Enumerate the host's descendant HWNDs recursively.
3. Attach to descendant HWND candidates and locate the Search content root.
4. Independently locate the same Search identity from the Desktop UIA root and verify its process ID.
5. Compare the directly attached element and Desktop-discovered element by stable identity, process, visibility/bounds, and semantic patterns; do not require runtime IDs to remain stable.
6. Record the outcome as H1A-A, H1A-B, or H1A-C.

The test must not assert a fixed number of intermediate nodes, a fixed HWND class name, a fixed child position, a fixed UIA parent depth, or a fixed RuntimeId.

### 7.7 Accessibility semantics matrix

| AutomationId | Required state/semantics in the current unavailable Search surface |
|---|---|
| `Search.View` | reachable, visible, non-empty bounds; no duplicate element for the same active Search surface |
| `Search.QueryTextBox` | enabled, keyboard-focusable, editable TextBox with Value/Text capability; entering temporary text must not execute Search |
| three Search option checkboxes | enabled, named, keyboard-focusable, Toggle capability and readable checked state |
| `Search.ScopeComboBox` | enabled, named, keyboard-focusable, ExpandCollapse and selection semantics |
| `Search.FilePatternComboBox` | enabled, named, keyboard-focusable, editable value plus ExpandCollapse semantics |
| three Search action buttons | reachable, named, Invoke-capable controls with `IsEnabled == false`; the smoke must not invoke them |
| `Search.UnavailableHint` | readable name/text, visible bounds, and not part of the interactive tab sequence |
| four floating-host chrome identities | remain reachable from the outer host; close continues to hide rather than destroy Search |

If FlaUI exposes equivalent higher-level control semantics rather than a raw pattern property, the existing FlaUI control wrapper may be used. Tests assert behavior and supported semantics, not one incidental wrapper type.

### 7.8 Automated lifecycle matrix

The focused smoke must cover, in order:

1. launch and open Search floating;
2. discover outer chrome and inner Search through both required discovery paths;
3. validate the accessibility semantics matrix;
4. hide Search through `Shell.Dock.FloatingHost.CloseButton`;
5. confirm no visible current Search surface remains; a provider-cached old element may either become unavailable or report non-visible/offscreen state, but it must never be reused as the current surface;
6. reopen and reacquire the new current host/content elements;
7. revalidate at least `Search.View`, query, options, scope, file pattern, buttons, and hint;
8. hide, close the process cleanly, launch a second process, verify Search starts hidden, reopen it, reacquire, and revalidate the inner content;
9. close through hide semantics.

H1A does not automate drag/re-dock because it is a diagnostic/test-harness card and must not add brittle pointer automation. The existing UI-DOCK-5 lifecycle tests remain authoritative for dock topology. If H1B later changes production hosting, drag/re-dock becomes mandatory regression coverage.

### 7.9 Failure diagnostics

On a timeout or assertion failure, the exception output must remain bounded and include:

- lifecycle checkpoint name;
- IDE process ID;
- outer floating HWND and its rectangle;
- recursively enumerated descendant HWND handles, visibility, process ID, and rectangles where available;
- outer and child UIA root AutomationId, Name, ControlType, and process ID;
- found and missing contract AutomationIds;
- a bounded automation-tree dump for each reachable relevant root;
- whether Desktop/process discovery succeeded independently of direct child-handle attachment.

Diagnostics must not dump editor contents, user file contents, API keys, environment variables, or layout XML.

### 7.10 H1A decision gate

H1A must stop after evidence collection and classify exactly one outcome:

#### H1A-A — Harness correction accepted

Required evidence:

- direct child-HWND attachment finds Search content;
- Desktop/process traversal independently finds the same contract identities;
- the accessibility semantics matrix passes;
- hide/reopen and second-process reacquisition pass;
- no production file was changed.

Action: retain the corrected smoke, reclassify `UI-MODERN-M1-A11Y-001` as a test-discovery defect, update the contract result ledger and current context at the governance flush, and close H1 without H1B.

#### H1A-B — Product fragment-navigation gap proven

Evidence shape:

- direct child-HWND attachment finds Search content and patterns;
- Desktop/process traversal cannot discover it after bounded retries;
- diagnostics identify the outer/child boundary where navigation stops.

Action: do not modify production code. Append the evidence, draft a separate exact H1B implementation contract, and stop for explicit user approval.

#### H1A-C — Provider/framework gap proven

Evidence shape:

- direct child-HWND attachment cannot find Search content, or required WPF control semantics are absent;
- failure is repeatable after reopen and a clean second process.

Action: do not modify production code. Record whether the likely owner is WPF `HwndHost`, AvalonDock 4.74.1, or the project template. Prepare options for an upstream issue, package upgrade evaluation, or narrowly owned adapter contract; stop for explicit user direction.

An inconclusive or flaky result is not H1A-A. It is a failure stop with evidence retained.

## 8. Conditional H1B boundary

H1B is a reserved name, not an implementation authorization.

It may be proposed only after H1A-B or H1A-C and must include the actual captured HWND/UIA evidence. Its future contract must choose one canonical correction path and provide its own exact API inventory.

H1B may consider, in this order:

1. a supported project-template or WPF host-provider connection that preserves AvalonDock ownership;
2. a narrow project-owned internal adapter only if the supported path is insufficient;
3. an upstream AvalonDock fix or separately approved dependency upgrade evaluation.

H1B must not:

- synthesize or flatten a replacement Search AutomationPeer tree;
- duplicate interactive UIA nodes;
- mirror Search state into hidden controls;
- replace AvalonDock's DockingManager or floating model;
- replace the real content with a test-only view;
- access private AvalonDock members through reflection;
- fork/vendor AvalonDock or change its version without a separate dependency contract;
- modify layout persistence, ContentIds, Search Home, or close-to-hide semantics.

Likely production files such as `ShellDockFloatingChromeController.cs` or `ShellTheme.xaml` remain forbidden until an H1B contract explicitly justifies them. `ShellWindow.xaml(.cs)` remains frozen unless a later contract proves no narrower owner exists.

## 9. DeepSeek boundary

No H1A or H1B work is delegated to DeepSeek.

Reason:

- the task is framework diagnosis across Win32 HWND, WPF `HwndHost`, UIA fragments, AvalonDock lifecycle, and the existing FlaUI harness;
- previous synthetic-peer attempts failed and were reverted;
- safe implementation depends on live evidence and exact provider behavior rather than bounded boilerplate generation.

No DeepSeek task package or Exact API Inventory for delegation is required.

## 10. Verification plan

### H1A minimum verification

After the approved H1A test-only change:

```powershell
dotnet build .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Debug
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Debug --no-build --filter "FullyQualifiedName~SearchTool_OpenHideAndReopen"
```

The environment variable must be restored or removed after the run.

Because H1A changes only a focused test and no production code, the full 2313-test non-UI suite and clean package are not required before the H1A evidence stop. They are required at H1 closure if production code later changes, or at the next package governance closure.

### H1B verification floor, if separately approved

Any production change requires at minimum:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Debug --no-build --filter "FullyQualifiedName~SearchTool_OpenHideAndReopen"
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

It also requires manual verification of keyboard Tab traversal, IME/text input, close-to-hide, reopen, drag guides, re-dock, Snap/system controls, and no duplicate narration/UIA nodes.

## 11. Hard stop conditions

Stop immediately if any of the following occurs:

- H1A would require a production file change;
- the only passing approach relies on a fixed HWND class name, child index, UIA depth, RuntimeId, arbitrary sleep, or cached stale element;
- a second UIA node is created for an existing interactive Search control;
- a test must be weakened or an existing Dock lifecycle assertion must be removed;
- Search visibility, Home, identity, geometry, persistence, close-to-hide, or `Float()` dispatcher ordering changes;
- drag guides, docking, focus, keyboard, IME, native window commands, Snap, or DPI behavior regresses;
- public C# API, dependency, project file, package version, reflection, or vendored source becomes necessary;
- implementation exceeds the card file budget;
- the result is intermittent or cannot be reproduced in the clean second process.

## 12. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Misclassifying a test lookup defect as a product bug | mandatory direct-child and independent Desktop/process comparison |
| Passing only by attaching to a private child handle | direct attachment is diagnostic; Desktop/process discovery is independently required for H1A-A |
| UIA provider identity changes after hide/reopen | reacquire handles and elements after every lifecycle transition |
| Tests couple to AvalonDock internals | assert stable identities and semantics, not class names/tree depth/runtime IDs |
| Disabled buttons are treated as missing | require reachability and Invoke-capable control semantics while preserving `IsEnabled == false` |
| Diagnostic output leaks project data | dump only HWND/UIA metadata and bounded trees |
| A production workaround damages Dock lifecycle | H1A forbids production changes; H1B requires a new approved contract |
| False confidence from one process | repeat after hide/reopen and a clean second process |

## 13. Acceptance and completion

This contract is accepted only when the user explicitly confirms `UI-MODERN-M1-H1 最终契约`.

After confirmation, execution starts with H1A only. H1A cannot silently continue into H1B.

H1 is complete when:

- H1A-A passes and the original debt is correctly reclassified/closed; or
- a separately approved H1B implementation passes its own full verification and closes the proven product gap.

An H1A-B/H1A-C evidence stop is a valid completed diagnostic stage but not completion of the overall accessibility debt.

## 14. Stage result ledger template

| Stage | Result | Files changed | Evidence | Decision | Next action |
|---|---|---|---|---|---|
| H1A | Completed diagnostic / failure stop | Contract only retained; temporary test probe reverted | Valid outer floating HWND exposed only custom chrome; valid same-process visible child HWND exposed a single unnamed `Pane` root with no Search descendants | H1A-C | Design H1B only after separate review and approval |
| H1B | Not authorized | None | None | Conditional only | Requires separate contract and approval |

## 15. H1A execution evidence — 2026-07-22

The user explicitly approved this contract and authorized H1A.

### 15.1 Probe implementation and correction

A temporary, test-only probe extended `Ra2IdeMainPathSmokeTests` with:

- recursive `EnumChildWindows` discovery below the verified outer floating host;
- direct FlaUI attachment to each same-process visible child HWND;
- an independent Desktop/process UIA lookup path;
- bounded HWND/UIA tree diagnostics.

The first run captured a transient outer handle that AvalonDock destroyed during host creation. Its later PID was 0 and it was no longer visible. This was not accepted as H1A-C evidence. The probe was corrected so every Retry snapshot reacquired the current outer HWND, matching this contract's lifecycle rule.

### 15.2 Valid H1A-C evidence

After the reacquisition correction, the focused smoke reproducibly reached a stable topology at initial Search open:

```text
Outer HWND: same IDE process, visible, 560 x 620
Outer UIA root: Window
Outer UIA descendants: Pane plus project-owned minimize, maximize/restore, and close buttons

Child HWND: same IDE process, visible, 558 x 618
Child UIA root: Pane, no AutomationId
Child UIA descendants: none
Search.View: not found
Search.QueryTextBox: not found
```

The test stopped with:

```text
H1A-C [initial open]: Search content was not reachable from any child HWND.
```

Because direct child-HWND attachment did not expose the hosted WPF subtree, Desktop/process comparison could not satisfy H1A-A and no production correction was attempted.

### 15.3 Files and rollback

The temporary probe was removed after evidence capture so the repository would not retain a deliberately failing UI smoke or dead diagnostic helpers. No production C#, XAML, project file, dependency, package, layout persistence, or Search behavior was modified.

Retained change:

- `Docs/UI-MODERN-M1-H1_FloatingContentAutomationAccessibilityContract.md` — approval and H1A evidence ledger.

### 15.4 Commands and results

```text
dotnet build .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Debug
Passed after the probe and after probe removal; 0 warnings / 0 errors.

RA2INIEDITOR_RUN_UI_AUTOMATION=1
dotnet test .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Debug --no-build --filter "FullyQualifiedName~SearchTool_OpenHideAndReopen"

Probe run: Failed by design at the H1A-C decision gate with the valid topology above.
Post-removal baseline rerun: Failed later during the second-process toolbar click with transient UIA COM error 0x80040201 (`event could not invoke any subscribers`). This did not contradict the captured H1A-C topology, but it means the post-removal baseline smoke is not green in this execution.
```

Full non-UI tests and clean packaging were not run because H1A changed no retained production/test code and stopped at the required R3 evidence gate.

### 15.5 Decision and next boundary

`UI-MODERN-M1-A11Y-001` remains open and is now classified as a proven provider/framework boundary gap, not merely a top-level-window test lookup error.

No H1B implementation is authorized. A future H1B contract must use this evidence to compare supported WPF `HwndHost` provider bridging, AvalonDock upstream behavior, and a separately approved dependency-version option before naming any production file.
