# ASSET-VOX-1D GLB-to-Canonical-Voxel Bridge Final Contract

Date: 2026-08-26  
Risk: R4  
State: Accepted / implemented / automated verified

## 0. Delivery statement

ASSET-VOX-1D converts one trusted-input, Host-validated GLB artifact into one deterministic, review-required canonical
voxel part. It does not generate geometry, split semantic parts, write project files or claim a game-ready asset.

## 1. Acceptance outcome

After 1D-1 through 1D-5, the repository shall be able to:

1. parse the restricted glTF 2.0 binary geometry used by the certified Tencent artifact;
2. validate bounds, transforms, indices, triangle topology and resource limits;
3. map glTF right/up/forward axes to canonical RA2 right/forward/up axes;
4. normalize the mesh deterministically to an explicit target voxel resolution;
5. rasterize triangle surfaces and fill a watertight interior;
6. create one `Ra2VoxelSceneSnapshot` using an explicit RA2 palette policy;
7. round-trip that snapshot through the existing VOX v150 and VXLSE SliceStack codecs; and
8. expose typed facts that prevent the candidate from being mistaken for final VXL/HVA output.

## 2. Scope

### 2.1 Allowed production files

```text
RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/
```

### 2.2 Allowed tests

```text
RA2IniEditor.Application.Tests/
```

### 2.3 Allowed governance documents

```text
Docs/ASSET-VOX-1D_*.md
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/DeveloperNotes.md              only if product/developer guidance changes
```

### 2.4 Forbidden changes

- `RA2IniEditor.AssetHost` protocol, lifecycle, workspace or lease semantics;
- Tencent request/poll/download behavior or any new paid call;
- Application public types, friend assemblies, capability Gateway or asset-provider API;
- INI parser, Field Registry, diagnostics, completion, Preview/Apply/Undo/Save;
- Shell, XAML, docking, fonts, menus or AutomationIds;
- current VOX, VXL reader, PNG or SliceStack semantics except a demonstrated 1D compatibility defect;
- direct VXL writer, normals, HVA writer or game launch automation;
- third-party runtime packages.

## 3. Ownership and lifetime

```text
AssetHost lease/file owner
    -> caller copies bounded GLB bytes while lease is alive
    -> Application restricted GLB reader
    -> transient immutable mesh snapshot
    -> deterministic voxelizer
    -> existing immutable Ra2VoxelSceneSnapshot
    -> existing VOX / SliceStack derived outputs
```

- Application receives bytes, declared source hash and explicit conversion options. It performs no I/O.
- Mesh snapshots and voxelization results are request-lifetime, in-memory and internal.
- `Ra2VoxelSceneSnapshot` remains the sole persistent comparison truth for one part.
- `Ra2VoxelAssetAssemblySpec` remains the sole Body/Turret/Barrel relationship truth.
- No candidate becomes a project asset until a later Host/UI review and persistence stage.

## 4. Exact internal model

Implementation may split files for readability, but shall provide the following exact responsibilities without parallel
authorities:

| Internal type | Responsibility |
|---|---|
| `Ra2GlbMeshReader` | bounded GLB v2 parsing and node-transform flattening |
| `Ra2MeshSnapshot` | immutable transformed positions, indexed triangles, source hash and bounds |
| `Ra2MeshTopologyFacts` | counts for components, boundary/non-manifold edges and degenerates |
| `Ra2MeshVoxelizationOptions` | explicit identity, role, resolution, padding, palette and colour/index policy |
| `Ra2MeshVoxelizer` | normalization, surface intersection, solid fill and canonical snapshot construction |
| `Ra2MeshVoxelizationResult` | success/failure, snapshot, transform/quality facts and review flags |
| `Ra2MeshVoxelizationFailureKind` | typed, stable internal failure categories |

No type above is public, serialized, a Gateway capability or a provider plugin contract.

## 5. Restricted GLB contract

### 5.1 Admission limits

| Resource | Limit |
|---|---:|
| GLB bytes | 16 MiB |
| JSON chunk | 1 MiB |
| scenes | exactly 1 selected scene |
| nodes | 64 |
| hierarchy depth | 16 |
| meshes | 16 |
| triangle primitives | 64 |
| transformed vertices | 500,000 |
| triangles | 1,000,000 |
| buffer views/accessors | 256 each |
| canonical occupied cells | existing 1,000,000 limit |

The 8.99 MiB / 249,567-vertex / 499,698-triangle certified artifact fits these limits.

### 5.2 Required structure

- GLB magic `glTF`, version 2 and declared length equal to actual input length.
- Exactly one JSON chunk followed by exactly one BIN chunk; duplicate, unknown required or trailing chunks fail.
- Selected scene graph must be acyclic and all referenced indices must be in range.
- Node `matrix` and TRS are mutually exclusive. All values and composed transforms must be finite.
- Mesh primitives must use `TRIANGLES` mode, required indexed geometry and a `POSITION` accessor of
  non-normalized `VEC3/FLOAT`.
- Indices may be unsigned 16-bit or unsigned 32-bit `SCALAR`; every index must address a position.
- Sparse accessors, morph targets, skins, Draco/meshopt compression, instancing and required extensions are unsupported
  in 1D and fail with a typed reason.
- Optional normals, UVs, colours, material, texture, cameras, lights and animations do not affect geometry truth and are
  ignored only when they do not require an unsupported extension.

### 5.3 Topology facts and solid admission

After transforms and exact-index assembly, the reader computes:

- zero-area and repeated-index triangle counts;
- vertex-connected component count;
- undirected edge incidence;
- boundary-edge count;
- non-manifold-edge count; and
- finite axis-aligned bounds.

The 1D production path requires exactly one connected component, zero degenerate triangles, zero boundary edges and zero
non-manifold edges. Open or multi-part geometry is not silently surface-filled or auto-joined.

## 6. Coordinate and normalization contract

### 6.1 Axis map

```text
(glTF X, glTF Y, glTF Z) -> (canonical X, canonical Z, canonical Y)
```

Equivalently, glTF right remains right, `+Z` forward becomes canonical `+Y`, and `+Y` up becomes canonical `+Z`.
Scene-node transforms are applied before this map.

### 6.2 Resolution and padding

- Caller supplies `TargetLongestDimension` in `8..128`; no hidden unit-class default exists in Application.
- Caller supplies padding in `1..4`; the accepted product default candidate is 1.
- Uniform scale preserves aspect ratio and maps the largest source extent to
  `TargetLongestDimension - (2 * padding)` voxel spans.
- Dimensions are derived with outward rounding, include padding, and must stay within `1..255`.
- The mapped solid is centred on canonical X and Y and grounded so the lowest occupied layer is Z=padding.
- Boundary decisions use double precision and a frozen epsilon derived from voxel scale; culture, thread count and input
  enumeration order cannot change the result.

### 6.3 Candidate origin and pivot

- Part `Origin` is canonical zero.
- Candidate `Pivot` is deterministic base-centre `((XSize-1)/2, (YSize-1)/2, 0)`.
- Local transform is identity and `VoxelUnitScale` records source metres per voxel.
- Result facts must mark pivot/mount as `ReviewRequired`; this value is not an HVA/game calibration claim.

## 7. Voxelization algorithm

1. Transform and axis-map all admitted vertices.
2. Compute normalized grid bounds and reject empty or numerically collapsed geometry.
3. For each triangle, enumerate only its clamped voxel AABB.
4. Mark a surface cell when the triangle intersects the closed voxel box using a deterministic separating-axis
   triangle/AABB test.
5. Flood-fill empty cells from the padded grid boundary with 6-neighbour connectivity.
6. Occupied cells are `surface OR not exterior`.
7. Reject empty output, occupancy overflow, disconnected output or a filled volume touching the outer grid boundary.
8. Construct the existing `Ra2VoxelSceneSnapshot`; do not duplicate its ordering, hashing, connectivity or symmetry code.

No GPU, random sampling, approximate point cloud, marching cubes, morphological repair, smoothing or AI decision is used.

## 8. Palette contract

- A complete existing `Ra2VoxelPaletteProfile` is mandatory.
- The caller selects either one explicit non-transparent palette index or one explicit RGBA target colour resolved by
  existing `FindNearestOpaqueIndex`.
- Geometry-only GLB does not imply white, green, remap or theatre palette selection.
- Remap indices are used only when explicitly requested by the caller.
- All occupied cells use the selected candidate index in 1D. Per-face/material colour transfer is deferred.
- Acceptance uses the supplied RA2 `unittem.pal` through the existing Westwood PAL decoder and an explicitly documented
  olive target colour; this is fixture policy, not automatic source-image colour extraction.

## 9. Failure and review contract

Required failure categories:

```text
None
InputTooLarge
MalformedContainer
UnsupportedFeature
ResourceLimitExceeded
InvalidTransform
InvalidAccessor
InvalidIndex
NonFiniteGeometry
DegenerateGeometry
DisconnectedGeometry
OpenSurface
NonManifoldSurface
InvalidOptions
EmptyVoxelResult
VoxelLimitExceeded
AnalysisFailed
Cancelled
```

Failures contain no snapshot or partial cell payload. Messages are bounded diagnostics and are not used for control flow.

Successful result facts include source/canonical hashes, source and output bounds, chosen axis map, scale, dimensions,
surface/interior/total cells, topology counts, palette hash/index, and these mandatory review flags:

```text
GeometryCandidate
UniformColourCandidate
PivotReviewRequired
NormalsNotGenerated
HvaNotGenerated
GameValidationNotRun
SemanticPartSplitNotAttempted
```

## 10. Separated-part policy

- One conversion request produces one caller-declared assembly part.
- The current P2 mesh is accepted only as a Body candidate.
- Node names, connected components, bounding boxes and geometric heuristics cannot automatically promote regions to
  Turret or Barrel.
- Reliable detached Turret/Barrel output requires one of: separate source GLBs, explicitly selected source nodes/primitives,
  or a user-reviewed spatial mask in a separately contracted stage.
- Future parts re-enter the existing Stage 1A rooted assembly graph; no second graph is permitted.

## 11. Test and verification matrix

### 11.1 Parser tests

- exact minimal GLB and the certified real GLB;
- U16/U32 indices and finite matrix/TRS transforms;
- bad magic/version/length/chunk order/alignment;
- out-of-range scene/node/mesh/accessor/buffer offsets;
- cyclic/deep node graph;
- unsupported modes/features/extensions;
- NaN/Infinity, bad indices and every resource limit boundary.

### 11.2 Geometry tests

- one-cell and asymmetric watertight boxes with analytically known occupancy;
- rotated/transformed box axis mapping;
- winding reversal invariance;
- triangle input-order and node-order determinism;
- open plane, two components, non-manifold edge and degenerate triangle rejection;
- cancellation during parser, topology, surface and flood-fill phases.

### 11.3 Canonical/output tests

- same bytes/options produce identical snapshot hash;
- changed resolution/palette/source changes the appropriate facts/hash;
- source hash is retained;
- output coordinates/dimensions/palette stay within 1B invariants;
- VOX write/read/write equality;
- SliceStack export/import exact cell equality;
- existing 1A/1B tests remain unchanged and pass.

### 11.4 Real artifact acceptance

For the P2 GLB, use an explicit Body descriptor, `TargetLongestDimension=64`, padding 1, supplied `unittem.pal` and an
explicit olive target colour. Acceptance requires:

- restricted reader success and topology facts matching the audited structure;
- bounded non-empty single-component voxel result;
- deterministic canonical hash across two independent runs;
- exact VOX and SliceStack round-trips;
- no external HTTP/model call and no project write;
- review flags remain present.

Visual quality, turret/barrel separation, colour accuracy, normals, HVA and in-game appearance are NotRun, not Passed.

## 12. Continuous execution stages

| Stage | Goal | Required gate |
|---|---|---|
| 1D-0 | audit, contract and R4 review | explicit user approval |
| 1D-1 | restricted GLB reader + immutable mesh/topology facts | parser/malformed/real-artifact focused tests |
| 1D-2 | deterministic transforms, axis and scale normalization | analytic transform/determinism tests |
| 1D-3 | surface SAT + watertight solid fill | analytic occupancy/topology/cancel tests |
| 1D-4 | palette policy + canonical snapshot/result facts | 1B invariant and hash tests |
| 1D-5 | real artifact, VOX/SliceStack regression and docs closeout | build, focused, Application full, IDE full, package |

Each stage stops on a required-gate failure. No later stage may be marked complete using an isolated rerun that hides a
repeatable failure.

## 13. Public API and compatibility

- Application exported allowlist remains exactly 77.
- Application friend assemblies remain unchanged.
- AssetHost exports remain zero.
- No new project, wire, persistence, capability or UI contract is introduced.
- 1D internal types are implementation details and cannot be exposed merely to connect assemblies.
- Product composition of an AssetHost lease with the 1D converter requires a later, independently reviewed seam.

## 14. Explicit non-goals

- additional Tencent calls or billing;
- image-to-3D generation quality improvement;
- mesh repair/decimation or automatic semantic segmentation;
- texture/material baking or source-image colour extraction;
- VXL normal generation, direct VXL writer, HVA animation or pivot calibration;
- automatic application/save, asset manifest closure or UI preview;
- claim of `GameReady`, `FinalVxl` or visually approved output.

## 15. Self-review result

The contract passes architecture, reuse, data ownership, resource-bound, public API and verification review. It avoids
the main rework risks by keeping GLB parsing transient, reusing the 1B snapshot, requiring explicit palette/part intent,
and refusing to guess semantic parts or pivots. Remaining uncertainty is visible and deferred rather than encoded as
truth.

The user explicitly approved this R4 contract. 1D-1 through 1D-5 were implemented without widening the contract;
completion evidence is recorded in `ASSET-VOX-1D_StageLedger.md`.
