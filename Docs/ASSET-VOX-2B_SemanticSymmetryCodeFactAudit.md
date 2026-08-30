# ASSET-VOX-2B Semantic Symmetry Code Fact Audit

Date: 2026-08-27

State: completed / read-only / implementation pending UI-contract approval

## 1. Conclusion

The requested rule is valid: except for identified attachments and protected thin features, the structural body should be
strictly symmetric around a reviewed lateral plane. The current product does not implement that rule.

Current facts:

- DeepSeek is used only by the natural-language colour/style compiler.
- `Ra2AiRequest` carries text and structured tools; it has no image-message contract.
- quality conversion and symmetry are synchronous local operations in `Ra2VoxelQualityRefiner`;
- the current symmetry suggestion mirrors against the full part-width midpoint and uses only local support counts;
- the current semantic-region rows are review descriptions, not executable cell masks; and
- the colourizer still paints deterministic broad geometry regions. It does not have authoritative glass, tyre, metal,
  attachment or remap masks.

A prompt-only change cannot alter geometry. The smallest reliable design is a text-only, two-round semantic classifier over
host-owned bounded region evidence, followed by a deterministic constrained symmetry executor.

## 2. Existing canonical path to reuse

```text
project VOX/VXL baseline + explicit project GLB
  -> Ra2VoxelStylePreviewCoordinator.GenerateQualityCandidates
  -> Ra2VoxelQualityRefiner.Convert
  -> immutable Direct / Refined candidates + protection facts
  -> Ra2VoxelStyleWorkspaceViewModel
  -> Ra2VoxelViewportSceneBuilder / Ra2VoxelViewport3D
  -> explicit in-memory geometry selection
```

The semantic path must join this flow after local Direct/Refined generation and before optional symmetry generation. It must
not create another snapshot type, renderer, writer, provider client or cache.

## 3. AI transport constraint

`Ra2AiRequest` exposes plain system/user text and structured tool definitions. Changing it to carry binary or multimodal image
parts would be a network-facing contract change outside this stage. Therefore 2B must use bounded textual evidence:

- six deterministic silhouette run summaries;
- candidate lateral planes and mirror mismatch counts;
- host-generated region IDs;
- normalized bounding boxes, cell counts, spans, surface/support and thinness facts;
- attachment/contact/branch facts;
- GLB coverage evidence summaries; and
- protection-zone overlap.

DeepSeek may classify only host-provided region IDs. It may not return voxel coordinates or invent a region.

## 4. Current symmetry limitation

`BuildSymmetryCandidate` currently:

- assumes `X = part.XSize - 1 - X` is the correct plane;
- treats the complete occupied body uniformly;
- has no explicit attachment exclusion;
- has no model-reviewed uncertainty state; and
- may add or remove a cell from local support evidence without proving a semantic body/attachment boundary.

It is therefore suitable only as an optional geometry suggestion, not as the requested semantic symmetry authority.

## 5. Current colouring limitation

The style compiler can express colour roles such as body, glass, rubber, bare metal, accent and remap, but semantic categories
are paintable only when backed by an explicit executable mask. With no such mask, they remain inferred/non-authoritative.
Production colouring is consequently limited to broad deterministic regions such as whole part, top, side, underside, edge
and interior.

2B should make the structural partition reusable by a later material-semantic stage, but must not claim that structural role
equals material identity. A symmetric hull can contain glass or metal; an asymmetric attachment can still use body paint.

## 6. Required ownership

| Concept | Owner | Lifetime | Serialized |
|---|---|---|---|
| region evidence package | Application derived geometry | one quality-review generation | no |
| round-one classification | IDE DeepSeek coordinator | one AI operation | no |
| round-two reviewed partition | Application immutable derived contract | current quality session | no |
| constrained symmetry candidate | `Ra2VoxelSceneSnapshot` | current quality session | no |
| 3D semantic overlay | IDE presentation | current preview mode | no |

The source VOX/VXL and paired GLB remain immutable. Only explicit `Use for this session` selects a candidate for later colour
preview; no file is applied or saved.

## 7. Reuse and extension assessment

| Need | Existing authority |
|---|---|
| canonical occupied geometry | `Ra2VoxelSceneSnapshot` |
| thin/transition protection | `Ra2VoxelFeatureProtectionMask` |
| quality and silhouette facts | `Ra2VoxelQualityAnalyzer` |
| paired mesh evidence | current 2x GLB conversion path |
| DeepSeek tool calls | `IRa2AiClient` + `Ra2AiRequest` |
| cancellation/stale-result ownership | workspace generation token pattern |
| 3D rendering | `Ra2VoxelViewportSceneBuilder` |
| review rows | `Ra2VoxelQualityReviewProjection` |

The later colouring stage can consume the same immutable partition and add a separate material classification without
changing geometry authority.

## 8. Risk result

- Audit/contract: R0 / Immediate.
- Runtime package: R3 / Deferred, explicitly authorized in scope but blocked on exact UI approval.
- Public API: none; all proposed contracts remain internal.
- Persistence/network protocol: unchanged.
- Provider cost: implementation uses fakes; real DeepSeek verification requires separate explicit authorization.

## 9. Stop point

The attached final contract must be approved before source or XAML changes. If implementation requires multimodal transport,
Shell, persistence, project writing, VXL/HVA export or a new provider dependency, stop and re-contract.
