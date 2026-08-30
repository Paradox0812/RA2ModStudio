# ASSET-VOX-3C Working Geometry Continuity — Code-Fact Audit

Date: 2026-08-29  
Status: completed / read-only code-fact audit  
Risk: R4 geometry-authority and authoring-state boundary  
Implementation: not started

## 1. Audit conclusion

The reported regression is deterministic and architectural. The workspace can adopt a Refined or Agent Geometry
candidate into `_workingGeometry`, and style compilation correctly consumes that working snapshot. However, the next
quality-candidate generation ignores it, re-voxelizes the original GLB, publishes that new GLB-derived Direct/Refined
branch, and explicitly clears `_workingGeometry`. Structure analysis then consumes that replacement Refined candidate.

Therefore the present pipeline is not a continuous authoring chain. It is a repeated rebuild from immutable GLB evidence:

```text
loaded/generated source -> adopted repair A
old GLB -> new Direct/Refined B -> Agent proposal over B
```

The required 3C pipeline is:

```text
immutable source root -> current working geometry A
current working geometry A + GLB evidence -> candidate B
explicit user adoption -> current working geometry B
```

GLB remains valuable evidence, but it must not regain geometry authority after the user has adopted a candidate.

## 2. Required pre-implementation boundary summary

1. **Current task goal**: make the current adopted geometry the sole session authoring baseline for all later local
   refinement, Agent geometry analysis, style compilation, final-candidate freezing and VOX export.
2. **Allowed production files after approval**:
   - `RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/Ra2VoxelQualityRefinement.cs`;
   - directly required internal voxel evidence/result files in the same folder, only if the existing evidence model cannot
     bind an existing baseline without duplication;
   - `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelStylePreviewCoordinator.cs`;
   - one new IDE-internal working-geometry state file under `RA2IniEditor.IDE/AssetAuthoring/`, if required;
   - `RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs`;
   - the exact one-word toolbar-label correction in
     `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml`;
   - focused Application/IDE tests and `Docs/ASSET-VOX-3C_*.md` plus current governance/status documents.
3. **Forbidden files/areas**: Shell, docking, menus, INI, Field Registry, diagnostics, completion, Work mode, provider
   executables/protocol, AssetHost, project Apply/Save, VOX writer semantics, VXL/HVA, palette file formats and legacy.
4. **Semantic boundary**: a review operation may derive candidates but cannot mutate the working geometry. Only the
   existing explicit `用于本会话` action may advance the working geometry. Export continues to consume only an explicitly
   frozen final candidate.
5. **AutomationIds**: preserve every existing `VoxelStyle.*` ID. No new AutomationId is required. In particular preserve
   `VoxelStyle.Preview.Direct`, `VoxelStyle.Quality.Status`, `VoxelStyle.Quality.UseCandidate`,
   `VoxelStyle.AcceptSession` and `VoxelStyle.ExportVox`.
6. **Validation commands**: IDE-only restore/build, Application tests, IDE tests, AssetHost regression and clean-source
   package, plus focused continuity/state-machine tests defined by the final contract.
7. **Approval**: explicit user approval is required before implementation because this is an R4 geometry-authority change.

## 3. Current implementation facts

### 3.1 The ViewModel has an ambiguous nullable working state

`Ra2VoxelStyleWorkspaceViewModel.cs` currently stores:

- `_source` as the immutable loaded/generated source;
- `_workingGeometry` as an optional adopted snapshot;
- `ActiveGeometrySnapshot => _workingGeometry ?? _source?.Snapshot`.

This nullable fallback means “no adopted geometry” and “working geometry was discarded” have the same representation.
There is no working revision, parent hash, origin or captured-baseline identity.

### 3.2 Quality generation bypasses the active geometry

`GenerateQualityCandidatesAsync()` captures `_source`, not `ActiveGeometrySnapshot`, and calls:

```text
GenerateQualityCandidates(projectRoot, source, glbPath)
GenerateQualityCandidatesFromGenerated(source)
```

After a successful result it executes:

```text
_workingGeometry = null;
_workingGeometryName = "当前模型";
```

It also calls `ClearStylePreview()`, which clears an otherwise still-valid styled review and frozen candidate even though
candidate generation is read-only and has not changed the working snapshot.

### 3.3 The coordinator treats the GLB conversion as the candidate baseline

Both coordinator entry points read the GLB, derive options from `baseline.Snapshot`, and invoke
`Ra2VoxelQualityRefiner.Convert(mesh, options)`. The loaded VOX/VXL snapshot is analysed only for baseline metrics and
source provenance; it is not the Direct/Refined candidate source.

### 3.4 The Application refiner has no existing-snapshot refinement entry

`Ra2VoxelQualityRefiner.Convert(...)` always:

1. voxelizes the mesh at target resolution to create `direct`;
2. analyses and protects that mesh-derived `direct`;
3. voxelizes the same mesh at 2x resolution;
4. derives every candidate from the mesh-derived `direct`.

`BuildMeshEvidenceSurfaceCandidate(...)` can already operate on an arbitrary canonical snapshot plus supersampled mesh
evidence, but the production entry never supplies the current working snapshot. The algorithm can be reused; a second
refiner should not be created.

### 3.5 Agent structure analysis inherits the wrong branch

`AnalyzeStructureAsync(...)` requires `quality.RefinedCandidate` and builds the Agent result from that candidate. Its hash
checks are internally consistent, but they bind the result to the fresh GLB branch rather than the current adopted branch.
The model is not reverting the repair by itself; it is being shown an older Host-selected geometry baseline.

### 3.6 Style and export already have the correct narrow authority

- `CompileAsync()` creates a source copy whose snapshot is `ActiveGeometrySnapshot`; style compilation therefore already
  follows the adopted geometry.
- `Ra2VoxelAcceptedCandidate` freezes an immutable canonical snapshot, and VOX export consumes that object rather than the
  visible review mode.

These boundaries should be preserved, not replaced.

### 3.7 Existing tests do not cover a second authoring round

Current ViewModel tests cover:

- generating a quality candidate;
- adopting it and compiling style;
- clearing state after a true source reload;
- freezing/exporting a candidate.

They do not cover:

```text
adopt Agent/Refined candidate -> generate quality candidates again -> run Agent analysis -> adopt/export
```

The regression therefore passes all present fixtures.

## 4. Root cause classification

| Cause | Classification | Evidence |
|---|---|---|
| Working geometry is optional rather than an explicit state | data-model defect | nullable fallback erases lineage and revision |
| Quality generation captures `_source` | orchestration defect | active working snapshot is not passed |
| Refiner always creates Direct from GLB | algorithm-entry defect | no existing-baseline overload |
| Success path sets `_workingGeometry = null` | destructive lifecycle defect | adopted geometry is explicitly discarded |
| Read-only generation clears style/final candidate | invalidation defect | derived review is invalidated without authority change |
| No second-round continuity test | verification gap | only one-pass lifecycle is tested |

## 5. Options reviewed

### Option A — replace `_source.Snapshot` with the adopted candidate

Rejected. It conflates immutable source provenance with mutable session work, can make source reload/export semantics
ambiguous, and would make “原始” cease to mean the admitted source.

### Option B — export/reload the repaired VOX before every next pass

Rejected as a product architecture. It uses disk as an implicit state bus, adds avoidable user work and still allows the
old GLB path to rebuild another branch.

### Option C — keep `_workingGeometry` but patch one call site

Rejected. Passing the current snapshot into only the coordinator would leave ambiguous invalidation, stale publication,
Agent result binding and accepted-candidate behavior ungoverned.

### Option D — explicit session working state plus baseline-preserving refinement

Selected. Keep source root immutable, make working geometry explicit and revisioned, treat GLB as evidence, derive every
candidate from a captured working baseline, and advance the chain only through explicit adoption.

## 6. Audit verdict

The requested continuity is feasible without a new public API, persistence format, writer, provider protocol or Shell
change. The change is still R4 because it changes which geometry is authoritative throughout the authoring pipeline.

The final contract must prevent two opposite mistakes:

- **silent reversion**: no review action may replace current working geometry with an old GLB branch;
- **over-rigidity**: continuity does not mean every previously added cell is immortal. A later explicit Agent proposal may
  remove or replace cells, but the change must be derived from the current baseline, visible in Difference and adopted by
  the user.

Implementation remains blocked until the final contract is approved.
