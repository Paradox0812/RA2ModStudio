# ASSET-VOX-3C Working Geometry Continuity — Final Contract

Date: 2026-08-29  
Status: completed / automated verified / physical acceptance pending  
Risk: R4 geometry-authority and authoring-state boundary  
Governance: approved and executed continuously through 3C-5

## 1. Product outcome

The Voxel Style workspace becomes one continuous, review-first authoring chain. Once the user adopts a Refined or Agent
Geometry candidate through `用于本会话`, that exact canonical snapshot becomes the sole baseline for every subsequent
quality pass, Agent geometry proposal, style compilation, candidate freeze and VOX export.

The continuity rule is:

```text
immutable source root S0
  -> current working geometry W0
  -> derive review batch Q1 from W0 + mesh evidence
  -> explicit adoption of candidate C1
  -> current working geometry W1 = C1
  -> derive review batch Q2 from W1 + mesh evidence
```

GLB is immutable alignment/coverage evidence. It is never silently promoted back to current geometry after W1 exists.
The source root remains available for comparison and provenance, and only a true source/project replacement starts a new
working chain.

## 2. Non-goals

3C does not add:

- automatic application, auto-save or project asset registration;
- VOX writer changes, VXL/HVA materialization, normals or game validation;
- persistent edit history, branches, cross-session recovery or a project format;
- mesh modification, Tencent regeneration or a real provider call;
- unrestricted model-owned coordinates or removal of 2C minimum safety;
- material-semantic colouring improvements;
- Shell/menu/docking/layout changes;
- a generic undo system for voxel authoring.

Reloading the original model remains the explicit way to start over in 3C. A richer geometry history/branch UI requires a
separate contract.

## 3. Authority model

| Authority | Owner | Mutable? | Purpose |
|---|---|---:|---|
| source root | existing `Ra2VoxelStyleSourceLoadResult.Snapshot` | no | admitted file/generated origin and “原始” view |
| working geometry | one IDE-internal `Ra2VoxelWorkingGeometryState` | replaced atomically | sole current session authoring baseline |
| GLB evidence | current admitted file/generated GLB bytes + hash | replaceable input | coverage/alignment evidence only |
| quality batch | immutable result bound to working revision/hash and evidence hash | no | baseline/refined/difference review |
| Agent geometry result | immutable result bound to one quality-batch fingerprint | no | sparse proposed add/remove candidate |
| style result | immutable result bound to working canonical hash | no | recolouring review only |
| final candidate | existing `Ra2VoxelAcceptedCandidate` | explicit replacement only | sole VOX export authority |

The Provider may propose intent. Application creates and validates snapshots. The IDE owns session orchestration. The user
alone advances working geometry and freezes a final candidate.

## 4. Internal data contract

### 4.1 Working state

Add one IDE-internal, non-serialized state family:

```csharp
internal enum Ra2VoxelWorkingGeometryOrigin
{
    LoadedSource,
    GeneratedSource,
    RefinedCandidate,
    AgentGeometryCandidate
}

internal sealed record Ra2VoxelWorkingGeometryState(
    Ra2VoxelSceneSnapshot Snapshot,
    Ra2VoxelWorkingGeometryOrigin Origin,
    string DisplayName,
    long Revision,
    string RootSnapshotHash,
    string? ParentSnapshotHash);
```

Rules:

- a successful source load/generation creates a non-null working state at revision 0;
- `RootSnapshotHash` never changes inside one source session;
- adoption increments revision exactly once and records the previous working hash as parent;
- a no-op adoption of the current hash does not increment revision;
- lineage stays outside `Ra2VoxelSceneSnapshot`, so snapshot schema, canonical serialization and exported VOX do not gain
  session-only metadata;
- the chain stores only current/root/parent facts, not an unbounded history.

### 4.2 Quality-batch identity

Extend the IDE-internal quality result with:

```text
WorkingBaselineHash
WorkingRevision
MeshEvidenceHash
QualityBatchHash
```

`QualityBatchHash` is a deterministic fingerprint of working baseline hash, mesh evidence hash, target grid/profile and
derivation parameters. It contains no path. Every structure result carries this batch hash in addition to the exact
candidate source hash it already uses.

### 4.3 Public API and persistence

- no exported type or method is added;
- Application internal accessibility and current friend-assembly direction remain unchanged;
- no JSON, settings, project, cache or AvalonDock schema is added;
- no change to `Ra2VoxelSceneSnapshot.CurrentSchemaVersion` is authorized.

If implementation proves an exported type or serialized lineage is necessary, stop 3C and return for a new approval.

## 5. Baseline-preserving refinement contract

### 5.1 Reuse path

Keep `Ra2VoxelQualityRefiner.Convert(mesh, options, ...)` for first-time mesh-to-voxel conversion. Add one Application-
internal entry that reuses the existing analyzers, protection mask, supersampled conversion, surface candidate builder and
admission gates:

```csharp
internal static Ra2VoxelQualityRefinementResult RefineExisting(
    Ra2VoxelSceneSnapshot baseline,
    Ra2MeshSnapshot meshEvidence,
    Ra2MeshVoxelizationOptions evidenceOptions,
    Ra2VoxelRefinementProfile? profile = null,
    CancellationToken cancellationToken = default);
```

No second quality engine or duplicate voxel model is permitted.

### 5.2 Algorithm

`RefineExisting` must:

1. capture and validate the canonical working baseline;
2. project the unchanged GLB into the existing baseline grid and reject an axis/dimension/part mismatch rather than
   replacing the baseline;
3. analyse protection, topology and quality on the working baseline;
4. create bounded supersampled GLB coverage evidence in the same canonical frame;
5. run the existing candidate behaviours against the working baseline coordinates;
6. preserve every unchanged coordinate and palette index exactly;
7. assign a new cell's palette deterministically: mirrored source palette for mirror additions; otherwise majority of
   occupied six-neighbours, then 26-neighbours, then the baseline dominant opaque index;
8. apply existing protected-coordinate, connectivity, cavity, volume, silhouette and quality gates relative to the
   captured working baseline;
9. return the working baseline itself as the batch's Direct/Baseline candidate and a distinct Refined candidate only when
   admitted and non-identical;
10. bind every result and evidence package to the captured baseline hash.

The mesh-derived target-resolution projection is private evidence. It cannot be shown as, adopted as or exported as the
working model in this path.

### 5.3 Meaning of continuity

Continuity protects state ownership, not every individual cell. Local refinement or a later Agent proposal may add/remove
cells only by producing a new candidate relative to the current working baseline. No action may restore an older branch
because it has a preferred roughness score or closer GLB coverage.

## 6. Agent geometry continuity

2C proposal semantics remain authoritative:

- primary/reviewer/conditional arbitration stay bounded;
- the model still addresses Host-known targets, not coordinates;
- Host still expands only the final `add_mirror` / `remove_source` operations;
- protection, connectivity, cavity, volume and silhouette minimum safety remain;
- no Host heuristic substitutes another action.

3C adds these binding rules:

1. structure evidence is built from the Refined candidate of a quality batch whose baseline equals the current working
   revision/hash;
2. the structure result carries the quality-batch hash and exact Refined hash;
3. publication requires the workspace generation, working revision/hash, quality-batch hash and selected model identity
   to remain current;
4. adoption requires the same checks again immediately before advancing working state;
5. stale results remain review data only if already visible; they cannot be adopted, frozen or exported as current work.

No additional DeepSeek call is introduced by 3C.

## 7. State-transition contract

| Event | Source root | Working state | Quality/structure | Style result | Frozen candidate |
|---|---|---|---|---|---|
| load VOX/VXL | replace | reset to source r0 | clear | clear | clear |
| successful generated source | replace | reset to generated r0 | clear | clear | clear |
| select/replace GLB evidence | keep | keep | invalidate | keep if working hash matches | keep |
| generate quality batch | keep | keep | replace atomically | keep | keep |
| run Agent structure analysis | keep | keep | publish if current | keep | keep |
| navigate review modes | keep | keep | keep | keep | keep |
| adopt Refined/Agent candidate | keep | advance r+1 | mark old batch non-adoptable | clear | clear |
| compile/recompile style | keep | keep | keep | replace atomically | clear on successful new result |
| edit style request | keep | keep | keep | mark stale/clear as current behavior | clear |
| freeze final candidate | keep | keep | keep | keep | replace explicitly |
| export VOX | keep | keep | keep | keep | consume immutable frozen snapshot |
| project/source close/change | clear | clear | clear | clear | clear |

Read-only candidate generation must never call `ClearStylePreview()` or clear the frozen candidate. Actual working
adoption must continue to invalidate both because their geometry binding has changed.

## 8. Concurrency and stale-result contract

- Every asynchronous operation captures workspace generation, working revision, working hash and relevant evidence/model
  identity before starting.
- A result is published only when every captured identity still matches.
- Selecting another GLB, adopting a candidate, changing project/source, starting a newer operation or disposing the
  workspace prevents late publication.
- Cancellation and stale rejection publish no partial candidate and never roll working state backward.
- Working-state replacement and invalidation of geometry-bound results occur as one UI-thread transaction.
- An exception before that transaction leaves the prior working state and frozen candidate intact.

## 9. Review and UI contract

3C makes no layout change. The existing four-task/central-viewport/evidence-tab UI and camera contract remain intact.

Exact presentation changes after approval:

- change only the `VoxelStyle.Preview.Direct` button text from `直接` to `基线`;
- `VoxelStyle.Quality.Status` continues to bind `WorkingGeometryText`, now rendered as a compact summary such as
  `当前几何：Agent 修复 · r2 · A1B2C3D4E5F6`;
- after candidate generation, status must state `基于当前几何 rN`;
- after adoption, status must state that the previous batch is no longer adoptable and a new pass will start from the
  adopted hash;
- stale/old batch adoption produces a local explanation and no mutation.

All existing AutomationIds, bindings, styles, dimensions and camera behavior are preserved. No new AutomationId is added.

## 10. Failure taxonomy

Extend only internal result kinds as needed to distinguish:

```text
InvalidWorkingBaseline
EvidenceGridMismatch
EvidenceSourceMismatch
BaselineStale
QualityBatchStale
NoSafeImprovement
NoSafeCandidate
Canceled
UnexpectedFailure
```

Expected failures return bounded local messages. They do not clear the working geometry, style result or frozen candidate.
Provider transport/tool failures retain the existing 2C meanings and do not trigger fallback to source or GLB geometry.

## 11. Allowed implementation files

```text
RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/Ra2VoxelQualityRefinement.cs
RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/  directly required internal evidence type only
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelStylePreviewCoordinator.cs
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelWorkingGeometryState.cs      optional new internal file
RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml
RA2IniEditor.Application.Tests/Ra2VoxelQualityRefinementTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStylePreviewCoordinatorTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceViewModelTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceUiContractTests.cs
Docs/ASSET-VOX-3C_*.md
Docs/DecisionLog.md
Docs/PublicApiLedger.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Editing any other production file requires a stop-and-review note explaining why the approved reuse path is insufficient.

## 12. Frozen boundaries

Do not modify:

- `ShellWindow.xaml`, `ShellWindow.xaml.cs`, Shell commands, menus, Dock or layout persistence;
- AssetHost, Tencent adapter, bundle, remote request, credit gate or provider protocol;
- `Ra2MagicaVoxelCodec` writer/export transaction or accepted-candidate disk semantics;
- VXL/HVA reader/writer/assembly/pivot/normal behavior;
- INI parser, Field Registry, Completion, Hover, Diagnostics, Work, Preview/Apply/Undo/Redo/Save;
- public API, snapshot schema, persistent settings, project files or legacy editor behavior.

Real Tencent/DeepSeek calls are not authorized by 3C.

## 13. Continuous implementation plan

### 3C-0 — characterization and boundary lock

- add failing characterization tests that reproduce adopted-candidate reversion;
- assert current source, working hash and quality baseline separately;
- freeze public API/exported type count, AutomationIds and no-writer/no-provider boundaries.

Gate: the new regression test must fail for the current implementation for the documented reason, not because of fixture
setup. No production behavior changes in 3C-0.

### 3C-1 — explicit working geometry state

- replace nullable snapshot/name pairing with one explicit working state;
- initialize/reset only on true source-session changes;
- split quality/structure invalidation from working/style/final-candidate invalidation;
- implement revision/hash/parent checks and no-op adoption.

Gate: deterministic state-machine tests for every row in section 7.

### 3C-2 — baseline-preserving quality derivation

- add `RefineExisting` by reusing the existing refiner internals;
- make GLB target projection evidence-only;
- preserve unchanged palette indices and deterministically colour additions;
- bind review/evidence/admission to the working baseline hash.

Gate: source immutability, baseline continuity, deterministic output, palette preservation, mismatch rejection, topology
and cancellation tests.

### 3C-3 — coordinator and Agent continuity

- pass the captured working snapshot/revision to both file and generated-session quality paths;
- bind structure results to quality-batch and working identities;
- retain 2C calls/arbitration/safety exactly;
- reject stale or sibling-branch adoption without clearing current work.

Gate: fake-client end-to-end tests for agreement, arbitration, failure, cancellation and stale publication; zero network.

### 3C-4 — product projection and export continuity

- apply the exact `直接` -> `基线` text correction and compact working revision/hash status;
- preserve existing layout and AutomationIds;
- prove candidate generation does not clear valid style/frozen state;
- prove actual adoption invalidates geometry-bound style/frozen state;
- prove freeze/export consumes the post-adoption snapshot.

Gate: UI contract/STA construction tests and end-to-end ViewModel/codec round-trip using temporary files only.

### 3C-5 — final verification and documentation

- run focused and full suites once;
- audit the diff against allowed files and forbidden strings/dependencies;
- update Stage Ledger, decision/API/status documentation;
- create an IDE-only clean source package.

Gate: all mandatory checks pass or the stage remains incomplete. A known unrelated flaky test may be isolated once and
reported, but repeated full-suite reruns may not be used to hide it.

## 14. Test matrix

### 14.1 Application

- existing baseline + mesh evidence returns Direct/Baseline with exactly the same canonical hash;
- admitted Refined derives from that baseline, not fresh mesh Direct;
- unchanged coordinates keep exact palette indices;
- added coordinates follow the deterministic palette policy;
- mismatched dimensions/axis/part fail without a candidate;
- protected cells, connectivity, cavities, volume and silhouettes retain existing gates;
- deterministic replay produces identical hashes and review facts;
- cancellation publishes no result.

### 14.2 IDE coordinator/ViewModel

- source S -> Refined R -> adopt -> regenerate yields `WorkingBaselineHash == R.CanonicalHash`;
- source S -> Agent A -> adopt -> regenerate -> Agent B yields a linear parent chain and never returns to S;
- generated-session GLB follows the same rule;
- selecting another GLB preserves working/style/frozen state but invalidates old quality/structure;
- read-only quality/Agent analysis preserves style and frozen candidate;
- adoption clears geometry-bound style/frozen candidate exactly once;
- Direct/Baseline no-op adoption does not increment revision;
- old quality/Agent result cannot be adopted after a newer working revision;
- project/source replacement creates a new root revision 0;
- no hidden Tencent/DeepSeek call is added.

### 14.3 UI/export

- all existing AutomationIds remain exact;
- `VoxelStyle.Preview.Direct` displays `基线`;
- working summary shows origin/revision/hash and updates only on true working transition;
- review navigation/camera behavior remains UI-R1 compliant;
- post-adoption frozen candidate exports the adopted canonical content through the unchanged VOX writer/round-trip gate.

### 14.4 Manual acceptance after automated gates

Using the user's `body-candidate.vox + mesh.glb` pair:

1. generate candidates and create/adopt a visibly changed Agent or Refined candidate;
2. record the displayed working revision/hash;
3. generate candidates again;
4. confirm the new Baseline view exactly matches the adopted model rather than the original GLB conversion;
5. run Agent recognition and confirm Difference is relative to that baseline;
6. freeze/export and reopen the VOX to confirm the adopted result is retained.

This manual test does not authorize a real provider call; a real Agent pass remains separately user-authorized.

## 15. Required verification commands

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 16. Acceptance criteria

3C is complete only when all are true:

1. adopted geometry survives every read-only quality and Agent review action;
2. every later candidate declares and matches the current working baseline hash/revision;
3. GLB-derived geometry is evidence-only after a working state exists;
4. no stale/sibling result can advance working state;
5. unchanged colours remain unchanged and additions have deterministic colour assignment;
6. valid style/frozen candidates survive read-only review and invalidate only on their documented authority changes;
7. source root remains immutable and available as Original;
8. 2C Agent proposal/safety and 3B export authority remain unchanged;
9. no public API, persistence, Shell, project write, VXL/HVA or provider change occurs;
10. mandatory automated gates and documentation/package checks pass.

## 17. Stop and rollback rules

- Stop if an existing-baseline path requires changing snapshot serialization or exposing Application types publicly.
- Stop if GLB evidence cannot be registered to the current grid without an ambiguous transform; do not silently rebuild
  from GLB.
- Stop if preserving continuity requires weakening 2C minimum safety or allowing model-owned coordinates.
- Stop if a stage needs VOX writer, VXL/HVA, Shell, provider, Apply/Save or project persistence changes.
- A failed stage must leave the prior working snapshot and exported file untouched. Revert only files changed by 3C; do
  not reset the user's dirty worktree.

## 18. Self-review

### 18.1 Architecture and reuse — passed

The contract keeps one canonical snapshot, one quality engine, one Agent proposal executor and one VOX writer. The new
entry is an overload/reuse path, not a parallel algorithm.

### 18.2 Data ownership — passed

Source root, working state, review batches and frozen candidate have distinct owners and lifetimes. Session lineage does
not pollute snapshot or file formats.

### 18.3 Stale/concurrency — passed

Revision, canonical hash, evidence hash and batch hash close the gap that a single ViewModel generation counter cannot
express. Publication and adoption both revalidate identities.

### 18.4 User-control boundary — passed

Continuity never auto-applies. A later Agent may intentionally change prior work, but only through a visible candidate and
another explicit adoption.

### 18.5 UI scope — passed

The UI change is exact and minimal: one label and one existing status binding. No layout or camera rework is coupled into
the geometry fix.

### 18.6 Public API/persistence — passed

No exported API or serialized state is needed. Any contrary implementation discovery triggers a stop.

### 18.7 Rework risk — passed conditionally

The highest rework risk is ambiguous GLB-to-current-grid registration. The contract addresses it by making mismatches a
typed failure rather than allowing an implicit rebuild. The second risk is over-preservation; the contract explicitly
allows later, visible, user-adopted removal relative to the current baseline.

Verdict: the contract is implementation-ready after user approval. It closes the observed reversion at the authority,
algorithm-entry, state-invalidation and verification layers rather than patching one button path.

## 19. Approval gate

The user approved continuous 3C-0 through 3C-5 execution. Implementation stayed inside the approved boundaries. No real
Tencent/DeepSeek call, Shell change, project Apply/Save, VOX writer change, VXL/HVA change or persistent voxel history was
introduced. Automated evidence and the remaining physical acceptance are recorded in
`Docs/ASSET-VOX-3C_StageLedger.md`.
