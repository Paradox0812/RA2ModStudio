# UI-MODERN-M2-R2 — Cohesive Shell Modernization Contract

Status: implemented on 2026-07-22 under the user's continuous-execution authorization.

Risk: R3 for floating-host lifecycle/shared Chrome; R1 for typography and menu density. No persistence-schema or business-semantic change.

## 1. Goal

Close the reported floating Search lifecycle defects and replace competing typography/menu authorities with one coherent light-theme path:

- clicking Search restores an already minimized floating host;
- floating tools expose minimize and close, but no dedicated maximize button or Snap hover region;
- a single-content directly hosted floating pane displays one title, while docked and multi-pane/tab structures keep their navigation headers;
- Shell and secondary WPF windows inherit one Chinese-capable UI font family;
- the code editor and code previews retain explicit Consolas;
- the top menu uses one Shell-owned Menu/MenuItem style;
- the visible IDE name is removed from the compact title band, leaving the application icon followed directly by the menu. The Window `Title` binding remains authoritative for the taskbar, Alt+Tab, system menu and accessibility.

The existing Row 1 floating-content placement is the accepted drag baseline and must not be reverted.

## 2. Non-goals and frozen boundaries

This package does not change:

- seven Dock ContentIds, Home zones, visibility defaults, geometry, v1/v2 migration, serialization, reset, or Return Home;
- Search execution, Replace, indexing, result navigation, cancellation, ViewModel compatibility members, or SEARCH-1 scope;
- dark theme, secondary-window templates, control dimensions, application-wide colors, text rendering mode, or font size;
- parser, editor semantics, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, AI, BuiltIn data, or legacy behavior;
- dependencies, project files, public C# API, or directory structure.

## 3. Ownership and lifecycle

| Concern | Authority | Rule |
|---|---|---|
| Dock visibility, selection and location | AvalonDock live model / `ShellDockLayoutCoordinator` | remains unchanged |
| Native minimized state | registered `LayoutFloatingWindowControl` | resolved by ContentId only after normal Dock activation |
| restore/activate/focus ordering | `ShellDockFloatingChromeController` | restore synchronously, then one `DispatcherPriority.Loaded` activation/focus dispatch |
| title visibility | `LayoutAnchorablePaneControl.Model` | collapse only when directly hosted in floating window and `ChildrenCount == 1` |
| native maximize hover region | `ShellWindowChromeController` optional maximize region | main Shell retains it; floating host omits it |
| UI font family | `UiFontFamily` token | Window inheritance only; explicit code fonts win |
| top-level menu density | `IdeMainMenuStyle` / `IdeMainMenuItemStyle` | one ShellTheme authority |

No retry loop, fixed sleep, handle cache, second window registry, parallel menu style, or new service is allowed.

## 4. Contract-visible identities

Preserved:

```text
Shell.Dock.FloatingHost
Shell.Dock.FloatingHost.MinimizeButton
Shell.Dock.FloatingHost.CloseButton
Shell.TitleBar.MaximizeRestoreButton
all existing Search.* identities
```

Retired from floating hosts only:

```text
Shell.Dock.FloatingHost.MaximizeRestoreButton
PART_FloatingMaximizeRestoreButton
```

The floating WindowChrome still retains native edge resizing and caption double-click behavior. The main Shell minimize/maximize/restore/close contract is unchanged.

## 5. Approved stages and files

### M2A/M2B — Floating lifecycle, caption and title

- `RA2IniEditor.IDE/Views/ShellDockFloatingChromeController.cs`
- `RA2IniEditor.IDE/Views/ShellWindowChromeController.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`
- `RA2IniEditor.Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`

### M2C — Global font-family authority

- `RA2IniEditor.IDE/Themes/IdeVisualTokens.xaml`
- `RA2IniEditor.IDE/App.xaml`
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`

Exact UI family:

```text
Segoe UI Variable Text, Microsoft YaHei UI, Segoe UI
```

No `TextFormattingMode`, `TextRenderingMode`, default font-size, or code-font change is authorized.

### M2D — Menu authority and density

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`

Top-level horizontal padding remains 4 DIP per side. Submenu columns remain governed by `UiMenuItemStyle`; no negative margin, scaling, or font shrink is used.

### M2D-Fix1 — IconOnlyTitleBar

User screenshot review showed that even the bounded title column competed with the menu in the 30-DIP title band. The approved narrow fix removes the visible title TextBlock and its grid column, then shifts Menu, drag region and caption-button panel left without changing their behavior or AutomationIds. Allowed implementation files are `ShellWindow.xaml` and `IdeVisualSystemBoundaryTests.cs` only.

## 6. Verification and completion record

| Stage | Result | Evidence |
|---|---|---|
| M2A/M2B | Completed | layout/Chrome boundary tests 19/19; solution compilation included in final build |
| M2C | Completed | visual token/resource tests 5/5 |
| M2D | Completed | visual/Shell boundary tests 36/36 |
| M2D-Fix1 | Completed | icon-only title-band structure covered by the combined UI/Shell boundary tests 55/55 |
| M2E | Completed with manual visual follow-up | combined UI/Shell boundary tests 55/55; full non-UI 2313/2313; restore/build/package passed |

Final commands:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Ra2ShellIdeLayoutBoundaryTests|FullyQualifiedName~IdeVisualSystemBoundaryTests|FullyQualifiedName~IdeShellBoundaryTests"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Observed M2-R2 results: build 0 warnings/0 errors; targeted 55/55; full non-UI 2313/2313. After M2D-Fix1, targeted tests passed 55/55 and the clean package was regenerated with 953 files.

## 7. Remaining verification and debt

- Per the user's request, no computer-control visual smoke was used in this package. A short real-WPF check remains for minimize → Search toolbar restore, repeated three times, and final font/menu appearance at 1920 x 1080.
- Physical 1280 x 800 and 125%/150% DPI checks remain hardware/manual verification.
- `UI-MODERN-M1-A11Y-001` remains separate: AvalonDock's child-HWND provider does not expose the floating Search content subtree to current UIA probing. This package does not attempt an accessibility bridge.
- Real Search execution remains owned by future `SEARCH-1`.

No further ad-hoc style patch is authorized from this contract. A failed visual check must be diagnosed against the single token/menu/chrome authorities above and contracted narrowly.
