# ASSET-VOX-2A-UI Review Candidate Composition Code Fact Audit

Date: 2026-08-27

State: completed / read-only / implementation not authorized

## 1. Audit conclusion

The existing Voxel Style workspace is a suitable composition surface, and the completed 2A quality types can be reused
without another voxel model or another renderer. However, the current product path cannot yet create the full 2A candidate
set:

- the workspace admits only project-contained `.vox` and single-Section `.vxl` plus an explicit `.pal`;
- `Ra2VoxelQualityRefiner.Convert` requires an admitted `Ra2MeshSnapshot` and `Ra2MeshVoxelizationOptions`;
- only `SuggestSymmetry` can operate on an already-created `Ra2VoxelSceneSnapshot`;
- there is no product call site for `Ra2VoxelQualityRefiner.Convert` outside tests; and
- the internal three-round `Ra2VoxelRefinementAiCoordinator` is also test-only.

Therefore a UI that merely adds Direct/Refined buttons to the current VOX/VXL session would be false. The smallest useful
product bridge is an explicit, read-only project-contained companion GLB selection. The current VOX/VXL remains the visual
baseline and palette/identity source; the GLB is locally converted into direct/refined/symmetry review candidates.

## 2. Current product data flow

```text
project-contained VOX
  or project-contained single-Section VXL + explicit PAL
    -> Ra2VoxelStylePreviewCoordinator.LoadSource
    -> immutable Ra2VoxelSceneSnapshot
    -> Ra2VoxelStyleWorkspaceViewModel
    -> Ra2VoxelViewport3D / SliceStack
    -> explicit DeepSeek style compile
    -> deterministic colour result + region mask + palette/review artifacts
    -> in-memory style acceptance only
```

Facts:

- `Ra2VoxelStylePreviewCoordinator` owns path admission, bounded reads, explicit compile and read-only review publication.
- `Ra2VoxelStyleWorkspaceViewModel` owns cancellation generation, visible mode, session acceptance and stale-result guards.
- `Ra2VoxelViewport3D` consumes only `Ra2VoxelSceneSnapshot` through the existing scene builder.
- Existing acceptance means a style preview accepted in memory; it is not a geometry-candidate selection.
- Existing source and style changes clear acceptance and invalidate older generations.
- No current workspace action writes a file.

## 3. Completed 2A data available for composition

`Ra2VoxelQualityRefinementResult` already contains:

- `DirectCandidate`;
- `RefinedCandidate`;
- optional `SymmetryCandidate`; and
- `Ra2VoxelRefinementReviewPackage` with direct/refined/symmetry facts, normal comparison and semantic-region proposals.

All candidates are canonical `Ra2VoxelSceneSnapshot` instances. The 3D viewport therefore requires no new geometry DTO or
rendering path.

The palette contrast optimizer also already returns an immutable review plan and before/after contrast facts. It is not
yet used by `CompilePreviewAsync`.

## 4. Missing product seams

1. No bounded GLB quality-source admission exists in the workspace.
2. No workspace coordinator invokes `Ra2GlbMeshReader` and `Ra2VoxelQualityRefiner.Convert`.
3. No ViewModel state distinguishes baseline, direct, refined, symmetry and styled candidates.
4. No UI exposes quality metrics, normal comparison, semantic-region provenance or source-pairing confidence.
5. No explicit command selects a geometry candidate for the current style session.
6. Palette contrast output is headless and has no review mode.

## 5. Provenance limitation

The GLB-to-voxel converter records `mesh.glb -> SHA-256` in the in-memory snapshot, but ordinary VOX and VXL files do not
preserve that source relationship. When a user pairs an existing VOX/VXL with a GLB, the IDE usually cannot prove they came
from the same generation run.

The UI must therefore distinguish:

```text
Verified       current in-memory snapshot carries the same mesh.glb hash
UserPaired     user explicitly selected both files; equality is not provable
Mismatch       an available source hash conflicts
Unavailable    no quality source selected
```

`UserPaired` may be reviewed but must never be displayed as verified. `Mismatch` publishes no candidate session.

## 6. Candidate option derivation

The bounded local conversion can reuse current baseline facts:

- scene/part/role/Section/stable stem: current snapshot descriptors;
- target longest dimension: current snapshot's longest dimension;
- palette: current snapshot palette;
- target palette index: deterministic most-frequent occupied palette index, ties resolved by lowest index;
- padding: frozen value `2` for this UI bridge;
- refinement profile: completed `asset-vox-2a/refinement-v1`;
- symmetry mode: `Suggest`.

These rules avoid adding another settings form and produce a deterministic review candidate. They do not claim that the
reconstructed direct candidate is byte-identical to an earlier conversion whose original options are unavailable.

## 7. Acceptance and style boundary

Geometry candidate selection and style-result acceptance are different decisions and must remain separate:

- `Use for this session` chooses Direct, Refined or Symmetry as the current in-memory working geometry;
- choosing a geometry candidate clears any compiled style result and previous style acceptance;
- a later explicit style compile consumes the working geometry while style-source resolution remains rooted at the
  admitted VOX/VXL file path;
- `Accept Preview` continues to accept only the compiled style result in the current session; and
- neither action writes, exports, applies, saves, generates VXL/HVA or changes the admitted source file.

## 8. Reuse assessment

| Need | Reused authority |
|---|---|
| GLB parsing | `Ra2GlbMeshReader` |
| direct/refined/symmetry production | `Ra2VoxelQualityRefiner` |
| canonical truth | `Ra2VoxelSceneSnapshot` |
| quality facts | `Ra2VoxelRefinementReviewPackage` |
| style plan and colour | existing compiler/colourizer/review package |
| contrast candidate | `Ra2VoxelPaletteContrastOptimizer` |
| 3D rendering | existing viewport and scene builder |
| async ownership | existing workspace generation/cancellation pattern |
| source containment | existing coordinator path-normalization rules |

No new parser, voxel type, palette type, renderer, project writer, cache or public API is justified.

## 9. Risk classification

- Audit and contract documents: `R0 / Immediate`.
- Future implementation: `R3 / StopForReview` because it changes the workspace lifecycle and selects which immutable
  candidate is fed into style compilation.
- It does not cross R4 persistence or provider-cost boundaries because no source is mutated, no file is written and no
  real model call is authorized.

## 10. Audit stop point

Implementation must not start until the exact UI contract is approved. If implementation requires Shell, public API,
persistence, provider execution, writer or Application algorithm changes, stop and re-contract instead of expanding this
phase.
