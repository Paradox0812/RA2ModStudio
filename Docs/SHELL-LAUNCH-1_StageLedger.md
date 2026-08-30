# SHELL-LAUNCH-1 Stage Ledger

Status: Completed / automated verified / physical Explorer smoke pending

Date: 2026-08-25

Contract: `SHELL-LAUNCH-1_FileAssociationLaunchFinalContract.md`

## 1. Delivered slices

| Slice | Result | Evidence |
|---|---|---|
| L1 startup request | Completed | Internal non-persistent request and strict parser accept one raw existing `.ini` or the existing automation folder option. |
| L2 Shell lifecycle | Completed | Startup waits for initial dock layout completion and reuses one project-open core shared with the menu path. |
| L3 exact file activation | Completed | The direct parent project is opened, the exact top-level target is loaded, its existing editable session is activated, and its tree item is selected. |
| L4 scope preservation | Completed | No XAML, registry, IPC, public API, persistence, Save, Field Registry, parser semantics or Agent behavior changed. |
| L5 verification | Completed | Focused 10/10, Application 188/188, IDE 2715/2715 and clean package passed; physical Explorer launch remains manual. |

## 2. Changed files

- `RA2IniEditor.IDE/Startup/Ra2LaunchRequest.cs`
- `RA2IniEditor.IDE/Startup/Ra2LaunchRequestParser.cs`
- `RA2IniEditor.IDE/App.xaml.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs` (approved Shell startup wiring only)
- `RA2IniEditor.Tests/IDE/Ra2LaunchRequestParserTests.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`
- current product/governance documentation listed in the final report

## 3. Verification ledger

| Gate | Result |
|---|---|
| focused launch and compatibility tests | Passed: 10/10 |
| IDE solution build | Passed: 0 errors, 1 existing nullable warning |
| Application tests | Passed: 188/188 |
| IDE tests | Passed: 2715/2715 |
| IdeOnly clean package | Passed: 1232 files |
| physical Explorer double-click | Manual acceptance pending |

## 4. Deferred boundary

`SHELL-LAUNCH-2` may add a single-instance coordinator and IPC forwarding only under a
separate contract. The current stage intentionally opens a new IDE process for each
Windows launch request.
