# ASSET-VOX-3C Working Geometry Continuity — Stage Ledger

Date: 2026-08-29  
State: completed / automated verified / physical acceptance pending  
Risk: R4 geometry-authority and authoring-state boundary

## Outcome

The adopted canonical voxel snapshot is now the sole baseline for every later local quality pass, Agent geometry proposal,
style compilation, explicit final-candidate freeze and VOX export. GLB remains bounded geometry evidence and cannot
silently replace the working snapshot.

## Risk classification

| Area | Risk | Resolution |
|---|---:|---|
| working geometry authority/lifecycle | R4 | one session-only owner with root/current/parent and monotonic revision |
| quality algorithm entry | R4 | reused the canonical refiner through `RefineExisting`; no parallel engine |
| Agent stale-result publication | R3 | working revision/hash + evidence/batch/model identity checked at publish and adopt |
| UI projection | R1 | one text correction (`直接` -> `基线`), all AutomationIds/layout preserved |
| persistence/public API/provider/writer | R0 | unchanged |

## Stage Result Ledger

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| 3C-0 | characterize the deterministic rollback | ViewModel test | new regression failed on the old behavior because active hash returned to the source | Completed | yes |
| 3C-1 | establish explicit working authority and invalidation | working-state record, ViewModel, tests | focused state/ViewModel tests | Completed | yes |
| 3C-2 | derive from working baseline with GLB evidence | quality refiner, coordinator, Application/coordinator tests | quality refinement 19/19 | Completed | yes |
| 3C-3 | bind Agent results and reject stale batches | coordinator, ViewModel, tests | affected IDE coordinator/ViewModel tests | Completed | yes |
| 3C-4 | preserve UI/export continuity | ViewModel, XAML, UI/export tests | affected IDE 41/41; unchanged export service exercised | Completed | yes |
| 3C-5 | full verification, governance and package | docs and approved task files | build/full suites passed; package result below | Completed pending physical UI/sample acceptance | yes |

## Key contract facts

- A successful loaded/generated source creates working revision 0.
- Only `用于本会话` advances the working revision; adopting the same canonical hash is a no-op.
- Selecting GLB, generating quality candidates and running Agent analysis are read-only.
- Each quality batch carries working baseline hash/revision, mesh evidence hash and deterministic batch hash.
- Each structure result carries the same batch identity plus its exact refined-source hash and model identity.
- A stale sibling remains reviewable but is not adoptable or freezable. The exact candidate that became current work remains
  eligible for explicit freeze/export.
- Source root remains immutable and visible as Original; reloading/replacing source starts a new revision-0 chain.

## Verification Matrix

| Step | Status | Evidence |
|---|---|---|
| 3C-0 characterization | Passed as expected-fail before fix | one regression failed because second pass restored source hash |
| Focused Application quality tests | Passed | 19/19 |
| Focused IDE continuity/UI/export tests | Passed | 41/41 |
| Restore | Passed | `dotnet restore .\RA2IniEditor.IDE.sln` |
| Build | Passed | Debug, 0 warnings / 0 errors |
| Application full suite | Passed | 288/288 |
| IDE full suite | Passed | 2855/2855 |
| AssetHost full suite | Passed | 50/50 |
| Real Tencent/DeepSeek | NotRun | explicitly outside the approved contract |
| Physical WPF/user sample | NotRun | requires user restart and visual/manual acceptance |
| IdeOnly clean package | Passed | `artifacts/RA2IniEditor.IDE.SourceClean.zip`, 1398 files; excluded build/cache/archive noise |

## Diff Intent Table

| File | Change Type | Reason | In Allowed Scope |
|---|---|---|---:|
| `Ra2VoxelQualityRefinement.cs` | internal algorithm entry/refactor | reuse existing quality engine against current baseline | yes |
| `Ra2VoxelStylePreviewCoordinator.cs` | internal batch identity/orchestration | bind quality/Agent results to working/evidence identities | yes |
| `Ra2VoxelWorkingGeometryState.cs` | new internal session state | establish one explicit working authority | yes |
| `Ra2VoxelStyleWorkspaceViewModel.cs` | lifecycle/stale checks | preserve read-only state and advance only on explicit adoption | yes |
| `Ra2VoxelStyleWorkspaceView.xaml` | exact text change | name Direct review as Baseline | yes |
| three voxel test files + UI contract test | regression/contract tests | prove continuity, staleness, palette, generated-session and export behavior | yes |
| 3C/governance/status docs | governance flush | record accepted decision and verified stop point | yes |

## Deferred Governance Queue — flushed

### PublicApiLedger

- Flushed zero-change confirmation: all new types/methods/result fields are internal; no serialized/public contract changed.

### Technical debt

- No implementation shortcut or duplicate quality engine was introduced.
- Persistent history/undo, material-semantic colouring and VXL/HVA materialization are explicit non-goals, not hidden debt.

### DecisionLog

- Flushed the accepted decision that current working geometry, not GLB reconstruction, is next-pass authority.

### CurrentStatus / Context

- Flushed implementation status to `Docs/Codex_CurrentPhase.md` and
  `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`.

## Long-term Documents Updated

| Document | Mode | Reason | Timing |
|---|---|---|---|
| `Docs/DecisionLog.md` | accepted decision | preserve geometry-authority rationale | deferred flush at package completion |
| `Docs/PublicApiLedger.md` | zero-change confirmation | record internal-only contract | deferred flush at package completion |
| `Docs/Codex_CurrentPhase.md` | stop-point status | identify latest verified phase | deferred flush at package completion |
| `Docs/RA2IniEditor_IDE_Full_Codex_Context.md` | compact baseline | future handoff | deferred flush at package completion |
| `Docs/FeatureOverview.md`, `Docs/UserGuide.md` | proposed, not edited | final physical wording should follow user acceptance | NotRun by 3C file boundary |

## Remaining acceptance

Restart the rebuilt IDE and use `body-candidate.vox + mesh.glb`:

1. generate/adopt a visibly changed Refined or Agent candidate;
2. record the displayed `rN` and 12-character hash;
3. generate candidates again and verify the Baseline view is that adopted geometry;
4. run Agent recognition and verify Difference is relative to the adopted baseline;
5. freeze/export, reopen the VOX and confirm the adopted shape remains.

No real provider call is implied by this ledger. A real DeepSeek pass remains a user-controlled manual action.

## Stop rule confirmation

The package stops at ASSET-VOX-3C. It did not start material-semantic colouring, persistent history, project Apply/Save,
VXL/HVA, provider changes or another stage.
