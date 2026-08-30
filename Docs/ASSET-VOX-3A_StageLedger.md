# ASSET-VOX-3A Stage Result Ledger

Date: 2026-08-28  
Contract: `Docs/ASSET-VOX-3A_GenerationOrchestrationFinalContract.md`

## Risk classification

Package risk: **R4**. It introduces one public experimental Host façade, a bundled executable boundary, session state,
external-image consent UI and an in-memory GLB-to-voxel handoff. Provider HTTP behavior, project writes and asset writers
remained frozen.

## Stage result ledger

| Stage | Goal | Files touched | Verification | State after stage | Next entry satisfied |
|---|---|---|---|---|---|
| 3A-1 | Bounded public façade and fixed provider bundle | AssetHost façade, IDE project, Host tests | AssetHost build; 50/50 Host tests | Completed | Yes |
| 3A-2 | Session model and headless orchestration | IDE generation orchestrator, preview coordinator | IDE project build | Completed | Yes |
| 3A-3 | Explicit UI, consent and progress | Voxel workspace XAML/code/ViewModel, UI contract test | UI contract 3/3; IDE build | Completed | Yes |
| 3A-4 | Reuse 1D/2A/1E/2C without a parallel pipeline | generated-session source and in-memory GLB quality path | affected voxel tests 23/23 | Completed | Yes |
| 3A-5 | Package verification and governance flush | tests/docs | solution build; Host 50/50; Application 285/285; IDE 2831/2831; clean package 1384 files | Completed | Stop |

## Verification matrix

| Check | Result |
|---|---|
| `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed, 0 errors; 1 existing nullable warning |
| AssetHost tests | Passed 50/50 |
| Application tests | Passed 285/285 |
| IDE tests | Passed 2831/2831 |
| `package-source-clean.ps1 -Profile IdeOnly` | Passed; 1384 files |
| Real Tencent / DeepSeek calls | NotRun by explicit authorization boundary |
| UI manual smoke | NotRun; user validation remains required |

## Deferred governance queue — flushed

### Public API

`Ra2MeshGenerationFacade`, request/result/progress/failure/image-format types were recorded in `Docs/PublicApiLedger.md`.

### Technical debt

| Task/Stage | Debt | Area/File | Reason accepted now | Impact | Repayment trigger | Status |
|---|---|---|---|---|---|---|
| 3A | No live provider/UI smoke in this package | generation UI and remote provider | User explicitly prohibited real calls | Runtime credentials, remote latency and final dialog experience remain unverified | Separate 3A-P1 manual probe approval | Open / bounded |

### Decision log

The fixed bundle, one-job consent boundary and generated-session source decision were recorded in `Docs/DecisionLog.md`.

## Diff intent table

| File/area | Change type | Reason | In approved scope |
|---|---|---|---|
| `RA2IniEditor.AssetHost/Ra2MeshGenerationFacade.cs` | Add | Narrow public façade and owned artifact copy | Yes |
| `RA2IniEditor.IDE/RA2IniEditor.IDE.csproj` | Modify | Reference façade and produce fixed provider bundle | Yes |
| `RA2IniEditor.IDE/AssetAuthoring/*Generation*` | Add | Session-only orchestration | Yes |
| Voxel style coordinator/ViewModel/view | Modify | Adopt generated GLB and expose explicit product flow | Yes |
| AssetHost/IDE tests | Modify | Freeze public and UI boundaries | Yes |
| Current docs and this ledger | Modify/Add | Governance flush and user guidance | Yes |

## Explicit non-changes

- No Shell file was changed by 3A.
- No Apply/Save or project transaction was changed.
- No VOX/VXL/HVA writer was called or changed.
- No Tencent HTTP/provider runtime behavior was changed.
- No DeepSeek integration, INI, Field Registry, parser, diagnostics, completion or legacy path was changed.

## Stop point

3A-1 through 3A-5 are complete. The next safe stage is a separately approved `ASSET-VOX-3A-P1` manual product probe:
one free-package reference-image job, screenshot review, cancel/timeout observation and confirmation that no project file is written.
