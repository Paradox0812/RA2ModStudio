# UI-MODERN-PROGRAM-R1 — M4 Stage Result Ledger

Status: Completed  
Package: M4 Field Registry Family Modernization  
Authority: `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md` Revision A  
Last updated: 2026-07-23

## Stage checkpoints

| Card | Result | Scope | Verification evidence | Next safe entry |
|---|---|---|---|---|
| M4-0 | Completed | Exact Field Registry surface, binding, handler, UIA, lifetime and virtualization inventory | `Docs/UI-MODERN-PROGRAM-R1_M4_FieldRegistryExactUiInventory.md` | M4 Foundation |
| M4 Foundation | Completed | Added scoped `IdeFieldRegistryStyles.xaml` and resource boundary tests | Debug build passed; `IdeVisualSystemBoundaryTests` 7/7 at foundation gate | M4-A |
| M4-A | Completed | Center 156 / adaptive / 300 navigation-list-details workspace | Debug build passed; affected Shell/Center/visual tests 52/52 | M4-B |
| M4-B | Completed | Manager status, rollback and cleanup maintenance workspaces | Debug build passed; manager/rollback/visual tests 62/62 after preserving exact legacy labels/dimensions | M4-C |
| M4-C | Completed | Import Preview source-review-plan-result hierarchy and advanced-source migration | Debug build passed; Harvest/guardrail/visual tests 32/32 | M4-D |
| M4-D | Completed | Learning source/review/apply hierarchy | Debug build passed; Learning/visual tests 24/24 | M4-E |
| M4-E | Completed | Field Editor and Allowed Values modern form/preview/write boundaries | Debug build passed; editor/learning/visual tests 52/52 | M4-F |
| M4-F | Completed | Add Property and Annotation modern presentation | Debug build passed; UI/annotation/text/visual tests 28/28 | M4-G |
| M4-G | Completed | Remote Preset compact modern form | Debug build passed; preset/Harvest/visual tests 37/37 | M4-V |
| M4-V | Completed | Full build, regression, real WPF evidence, clean package | Debug build 0 warnings/0 errors; full non-UI 2322/2322; four real screenshots; IdeOnly package passed | M5-0 |

## Protected boundary result

- No public C# API, ViewModel, service, dependency, project file, Shell/Dock or persistence change.
- Provider priority remains Project > Global > BuiltIn.
- Reload, import diff, apply plan, backup manifest, rollback, cleanup, learning and field-save behavior remain unchanged.
- All pre-M4 AutomationIds remain present with no duplicates. Additive Center landmarks are `Navigation`, `FieldList` and `Details`.
- Owner, Show/ShowDialog, DialogResult, dirty-close and MessageBox confirmation behavior remain unchanged.
- Completion, Hover, Quick Peek, Diagnostics, Save Preflight, parser/editor/AI, BuiltIn data and legacy remain unchanged.
- Field/pack/rollback/diff/plan/value lists retain the `UiDataGridStyle` virtualization path: row/column virtualization, `CanContentScroll` and Recycling.

## Visual evidence

| Artifact | Size | SHA-256 | Result |
|---|---:|---|---|
| `artifacts/M4-RegistryCenter-Default.png` | 1040 × 700 | `2E31899438FD126805149DCE567ADA61ECBDBEB17366E5B97E2C5757FFB5E854` | Accepted: three-pane proportions, hierarchy and empty details state |
| `artifacts/M4-RegistryCenter-FieldSelected.png` | 1040 × 700 | `C1E50E7B4AEB020E20E55CB4AB679DAECFE6B391BF83C40349F5CE0501810B03` | Accepted: selected row and details projection |
| `artifacts/M4-RegistryManager-Rollback.png` | 1120 × 880 | `8E67B68D0D665E9814740482E538390E640495A9169A3D720715766498B79A36` | Accepted: maintenance tabs, risk band and rollback grid |
| `artifacts/M4-Harvest-DiffReview.png` | 1040 × 720 | `996B1AD7BEE9850CA44652E65F43AE43173143A298F64301E5ECAD74B407B93D` | Accepted: source, plan boundary and populated diff review |

Capture host was the current interactive Windows session. The smoke inserted the built-in sample and parsed it only. It did not fetch, build/apply a write plan, save, rollback or change registry files.

## Verification Matrix

Selected profile: Full package gate for presentation-only WPF changes.

| Step | Status | Evidence |
|---|---|---|
| Targeted card tests | Passed | Per-card counts recorded above; failures were fixed within visual/assertion scope and never replaced by weaker checks |
| Build / Compile | Passed | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`; 0 warnings, 0 errors |
| Full Suite | Passed | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build`; 2322/2322 |
| Real WPF visual smoke | Passed | Four artifacts above; field selected and import sample diff populated through UI Automation anchors |
| Network/write/destructive smoke | NotRun | Explicitly outside the presentation-only contract; no operation was authorized or necessary |
| IdeOnly clean package | Passed | Final Accepted package contains 960 entries; SHA-256 `0EE0E4F5F3A2EF966C9C6408393109F4E4F4ABEA2CCF6E2344643EB46BE075F5` |

Final rollback package: `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4.Accepted.Rollback.zip` (10,441,307 bytes, 960 entries).

## Deferred Governance Queue

### Public API ledger

None.

### Technical debt

- Scoped `IdeSecondary*` migration aliases remain in Harvest, Learning, Field Editor and Allowed Values so large existing XAML can adopt the new visual layer without application-wide compatibility mutation. M6-B owns the zero-reference/safe-alias audit; these aliases do not change runtime semantics.
- Exact compact-screen and 150% hardware review remains an M6 responsive matrix item. M4 screenshots prove configured window sizes and current-host rendering only.

### Decision log candidate

Accepted by Revision A: domain-specific Field Registry styles are scoped and explicit; compatibility aliases are retired only after zero-reference verification.

## Next safe entry

Stage: M5-0 Assistive Surface Exact UI Inventory.  
Allowed: read-only inventory of Completion, Quick Peek, Peek Definition, Find References, Dirty Navigation and Save Preflight presentation boundaries.  
Forbidden: Completion/Peek/navigation/save semantics, editor mutation, public API, persistence, new dependencies, Shell/Dock topology and Field Registry behavior.
