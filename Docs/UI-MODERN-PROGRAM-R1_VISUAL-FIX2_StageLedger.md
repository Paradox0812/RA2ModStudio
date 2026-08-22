# UI-MODERN-PROGRAM-R1 VISUAL-FIX2 Stage Ledger

Status: implementation and automated verification completed on 2026-07-23; manual visual acceptance pending.

## 1. Goal and approved boundary

This bounded correction addresses two screenshot/runtime observations:

1. The Field Registry active-pack table left excessive horizontal space between `范围` and `字段`.
2. The hidden floating Search host could become visible for one dispatcher interval during Shell startup.

Approved production/test files:

- `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellDockFloatingChromeController.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`

Frozen boundaries: public API, dependencies/project files, Dock ContentIds, Home profiles, layout persistence schema/migration, Search open/close/Home behavior, Field Registry data/priority/write semantics, parser/editor/AI/Completion/Hover/Diagnostics/Save/BuiltIn and legacy.

## 2. Architecture and lifecycle result

- The active-pack `范围` column is fixed at 88 DIP; `字段` remains 48 DIP. The surrounding responsive pane bounds and bindings are unchanged.
- Startup suppression is owned by the existing `ShellDockFloatingChromeController`, which already owns floating-host registration and chrome lifetime.
- `ShellWindow_OnLoaded` begins suppression before compiled topology creation and completes it in `finally` after persisted-layout restoration.
- Hosts created or rediscovered during startup are temporarily rendered at opacity 0 and have their previous opacity restored.
- The existing asynchronous `Float()` then deferred visibility sequence remains unchanged. The suppression path does not call `Hide()`, avoiding the previously verified AvalonDock immediate `Float()`/`Hide()` crash.
- No timer, Win32 hook, second host registry, persistence field or new dependency was introduced.

## 3. Stage Result Ledger

| Stage | Goal | Files touched | Verification | State after stage | Next entry satisfied |
|---|---|---|---|---|---|
| VISUAL-FIX2-A | Compact active-pack columns | FieldRegistryCenterWindow.xaml; IdeVisualSystemBoundaryTests.cs | XAML parse; build; boundary tests | Completed | Yes |
| VISUAL-FIX2-B | Prevent startup Search host flash without changing Dock lifecycle | ShellDockFloatingChromeController.cs; ShellWindow.xaml.cs; Ra2ShellIdeLayoutBoundaryTests.cs | build; 76 targeted tests; 2335 full non-UI tests | Completed | Yes |
| VISUAL-FIX2-V | Package-level automated verification | No production changes | Debug build and full suite passed | Completed | Yes |
| VISUAL-FIX2-Acceptance | Observe real startup and final Field Registry spacing | None | NotRun: requires manual visual observation | Pending | No |

## 4. Verification Matrix

Selected profile: UI + Runtime + Full.

| Step | Status | Evidence |
|---|---|---|
| XAML XML parse | Passed | `FieldRegistryCenterWindow.xaml` parsed successfully |
| Build / Compile | Passed | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`; 0 errors, 1 pre-existing `CS8602` test warning |
| Targeted Tests | Passed | Shell/visual/layout boundary filter; 76/76 |
| Full Suite | Passed | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build`; 2335/2335 |
| Real WPF startup observation | NotRun | Automated/static tests cannot prove absence of a one-frame visual flash |
| IdeOnly clean package | Passed | `RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-VISUAL-FIX2.Final.zip`; 970 entries; forbidden-entry scan 0; required-file omissions 0 |

The first targeted run reported one failure in the newly added test because its assertion prohibited the controller's existing close-handler `anchorable.Hide()` globally. The assertion was narrowed to the new suppression method only; production code was not changed to satisfy that test. The final targeted run passed 76/76.

## 5. Diff Intent Table

| File | Change type | Reason | In allowed scope |
|---|---|---|---|
| `Views/FieldRegistryCenterWindow.xaml` | Presentation | Compact active-pack column spacing | Yes |
| `Views/ShellDockFloatingChromeController.cs` | Internal startup lifecycle guard | Suppress intermediate floating-host rendering and restore prior opacity | Yes |
| `Views/ShellWindow.xaml.cs` | Approved Shell wiring | Bound suppression to the existing startup/restore lifetime with `try/finally` | Yes |
| `Tests/IDE/IdeVisualSystemBoundaryTests.cs` | Boundary test | Lock 88/48 DIP active-pack columns | Yes |
| `Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs` | Lifecycle boundary test | Lock begin/restore/finally/complete ordering and no `Hide()` in suppression | Yes |

## 6. Deferred Governance Queue

### PublicApiLedger Pending Entries

None. The two suppression methods are internal; no public API changed.

### TechnicalDebt Pending Entries

| Stage | Debt | Reason | Impact | Suggested resolution | Status |
|---|---|---|---|---|---|
| VISUAL-FIX2-Acceptance | Real startup/no-flash and final spacing are not yet visually observed | Computer control was not used for this correction | Automated evidence cannot prove a one-frame presentation property | Start the app three times and inspect Field Registry at compact/default pane widths | Open verification gap |

### DecisionLog Candidate Entries

| Stage | Decision | Status | Reason | Needs human review |
|---|---|---|---|---|
| VISUAL-FIX2-B | Suppress startup host rendering in the existing floating-chrome owner; preserve Dock topology and asynchronous hide order | Accepted by confirmed contract | Lowest-scope solution that respects the verified AvalonDock lifecycle constraint | No |

### CurrentStatus Pending Updates

Flushed to `Docs/Codex_CurrentPhase.md` and `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`.

## 7. Rollback and remaining risk

Pre-change trusted anchor:

```text
artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-VISUAL-FIX1.Final.zip
Entries: 969
SHA-256: F9026D65A120CE5FC849F9ACB0B7576E17E0BCEF04F0F8607F090C5AB7AE54F7
```

Remaining risk is limited to manual visual acceptance. If a host still flashes, stop and capture startup timing evidence; do not reorder the Dock coordinator or reintroduce immediate `Hide()`.

Final clean package:

```text
artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-VISUAL-FIX2.Final.zip
Entries: 970
Forbidden entries: 0
Missing required files: 0
Diff against VISUAL-FIX1 anchor: exactly 10 expected implementation/test/governance files
```
