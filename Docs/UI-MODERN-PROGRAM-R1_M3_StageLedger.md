# UI-MODERN-PROGRAM-R1 — M3 Stage Result Ledger

Status: Completed  
Package: M3 Shell Workspace Modernization  
Authority: `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md` Revision A  
Last updated: 2026-07-23

## Stage checkpoints

| Card | Result | Scope | Verification evidence | Next safe entry |
|---|---|---|---|---|
| P0 | Completed | Final authority/succession contract and unique clean-source rollback anchor | IdeOnly package hygiene passed; 954 source entries; SHA-256 `979D2B9FC341C6C96AA176B7C34B4489ACF332EC9DF52E166FE7C01D27EFC02E` | M3-0 |
| M3-0 | Completed | Exact Shell/Dock/Problems/Output/Project Explorer/AI/Hover/Search UI inventory | Source/XAML inventory captured in `Docs/UI-MODERN-PROGRAM-R1_M3_ShellExactUiInventory.md` | M3-A |
| M3-A | Completed | Scoped workspace resource dictionary and resource-boundary tests | Debug build passed; `IdeVisualSystemBoundaryTests` 6/6 | M3-B |
| M3-B | Completed | Graphical Problems severity, standalone Issues parity, preserved UIA/handlers/bindings | Debug build passed; combined Shell/visual/layout tests 56/56 | M3-C |
| M3-C | Completed | Flat Project Explorer collection density and Output log surface | Debug build passed with 0 warnings/0 errors; combined Shell/visual/layout tests 56/56 | M3-D |
| M3-D | Completed | AI workspace/message/composer and Hover card presentation moved behind scoped workspace styles | Debug build passed; combined Shell/visual/layout tests 56/56 | M3-E |
| M3-E | Completed | Compact menu/toolbar/Dock density and modern floating Search composition | Debug build passed; combined Shell/visual/layout tests 56/56; real Search screenshot captured | M3-E-Fix1 |
| M3-E-Fix1 | Completed | Corrected `UiContextMenuStyle` so explicit separators no longer receive a `MenuItem` container style | Runtime STA ContextMenu open test passed; Debug build passed; combined Shell/visual/layout tests 56/56 | M3 verification |
| M3-V | Completed | Package-level build, full non-UI regression, clean rollback package and evidence audit | Debug build passed; 2314/2314 tests passed; IdeOnly package 957 entries; SHA-256 `1DB7A07AAE8D6770D0E40287EA1D97B1DC73F0E2ACAC28F02DD7F9B903E6E161` | M4-0 |

## Protected boundary result

- No public C# API, dependency or project-file change.
- All seven AvalonDock ContentIds, Home profiles, persistence and floating lifecycle remain unchanged.
- Problems/Issues bindings, commands and filter semantics remain unchanged; severity is a presentation-only icon/template change.
- Project Explorer item source, selection handler and virtualization remain unchanged.
- Output binding and read-only behavior remain unchanged.
- AI lifecycle, Search behavior, parser/editor/language services, Field Registry semantics, save behavior, BuiltIn data and legacy remain unchanged.
- The ContextMenu correction changes presentation-style routing only; command routing and Window Layout behavior are unchanged.

## Deferred Governance Queue

### PublicApiLedger Pending Entries

None.

### TechnicalDebt Pending Entries

No product debt was introduced. Physical 1920 × 1080 at 100%, 1280 × 800 and 150% DPI remain hardware visual checks. The captured machine was 2560 × 1440 at 125%; therefore `M3-Shell-1920x1080-100.png` is retained as a naming-compatible artifact but is not accepted as exact 1920 × 1080 / 100% evidence.

The pre-existing `UI-MODERN-M1-A11Y-001` child-HWND UI Automation provider gap remains open: the Search UIA smoke could see the top-level window but not `Shell.MainToolbar.SearchButton`. It is not treated as a Search lifecycle failure and was not expanded inside M3.

### DecisionLog Candidate Entries

| Stage | Decision | Status | Reason | Needs Human Review |
|---|---|---|---|---|
| P0/M3-A | Use scoped `IdeWorkspaceStyles.xaml` over application-wide implicit workspace styles | Accepted by confirmed Revision A | Prevents later Field Registry and assistive surfaces from silently inheriting Shell-specific geometry | No |

### CurrentStatus Pending Updates

Flushed at package completion: M3 is the latest verified package; M4-0 is the next safe entry.

### Superseded Docs Pending Entries

None.

## Visual evidence audit

- `artifacts/M3-Shell-CurrentResolution.png`: real Shell capture on 2560 × 1440 / 125% host; accepted for current-host composition only.
- `artifacts/M3-Search-FloatingHome.png`: real 560 × 620 floating Search capture; accepted for single-title, no-maximize and modern surface composition.
- `artifacts/M3-AI-Workspace.png`: real AI workspace capture; accepted for resource-adopted visual hierarchy.
- `artifacts/M3-Problems-ErrorsWarningsInfo.png`: file exists, but the Problems pane was not reliably activated; **NotRun** for populated severity-row visual proof.
- Exact 1920 × 1080 / 100%, 1280 × 800 and 150% DPI: **NotRun** on unavailable hardware/display modes.
- Search minimize/reopen UIA automation: blocked by the pre-existing child-HWND provider gap; static/lifecycle boundary tests passed, but this is not claimed as new runtime UIA evidence.

## Next safe entry

Stage: M4-0 Field Registry Exact UI Inventory  
Allowed scope: read-only inventory of Field Registry surfaces, shared style adoption points, AutomationIds, bindings, commands and virtualization boundaries; create the M4 inventory document.  
Forbidden scope: registry provider priority, apply/import/rollback/learning/write semantics, Completion/Hover/Quick Peek/Diagnostics behavior, BuiltIn data and public API.  
Verification: source/XAML fact cross-check; implementation starts only after the exact inventory demonstrates a presentation-only path under the already confirmed continuous contract.  
