# ASSET-VOX-1D GLB-to-Canonical-Voxel Code Fact Audit

Date: 2026-08-26  
State: Completed / contract input  
Implementation state: Not started

## 1. Objective

Determine the smallest reliable bridge from a Host-validated glTF 2.0 binary mesh to the existing
`Ra2VoxelSceneSnapshot` authority without changing Provider, Host, final asset, project-write or UI ownership.

## 2. Existing authorities

| Authority | Current owner | Reuse in 1D |
|---|---|---|
| Provider execution, API key, billing confirmation and download | `RA2IniEditor.AssetProviders.TencentHy3D` | Unchanged; no call in 1D |
| Process lifecycle, bounded workspace and artifact lease | `RA2IniEditor.AssetHost` | Upstream only; no protocol change |
| Single-part canonical cells, palette, hashes and quality facts | Application `VoxelAuthoring` | Required output authority |
| Body/Turret/Barrel identity and parent graph | `Ra2VoxelAssetAssemblySpec` | Retained; one 1D conversion produces one part |
| VOX v150 and VXLSE SliceStack exchange | `Ra2MagicaVoxelCodec` / `Ra2VoxelSliceStackCodec` | Required downstream proof |
| Final VXL normals, HVA and game acceptance | VXLSE/manual future stages | Explicitly outside 1D |

Application is a headless, deterministic algorithm assembly. Its boundary tests forbid filesystem, process,
environment, WPF and Infrastructure access. The new bridge therefore has to consume caller-owned bytes and return
in-memory facts; it cannot open an AssetHost lease or a project path itself.

## 3. Canonical-core facts

- `Ra2VoxelSceneSnapshot` is internal, immutable and describes exactly one part.
- Dimensions are `1..255`; occupancy is capped at 1,000,000 unique non-transparent cells.
- Cells are canonicalized in `Z,Y,X` order and the snapshot owns connectivity, X-symmetry and SHA-256 facts.
- A palette contains exactly 256 RGBA entries with explicit transparent/remap indices.
- Existing Westwood PAL decode and nearest-opaque-colour selection are reusable; no second palette implementation is
  justified.
- Stage 1A already owns the detached assembly model. A mesh converter must not invent a second assembly graph.

## 4. Real P2 artifact facts

Authority artifact:

```text
artifacts/asset-vox-1c-p2-live/call-04-postfix-20260826/mesh.glb
```

| Fact | Value |
|---|---|
| File length | 8,991,920 bytes |
| SHA-256 | `22FD5BE5BEB833C8ECAF05E16A8A070B699FF1C9339F24E51054A330CB57F709` |
| glTF version | 2.0 binary GLB |
| Scene/node/mesh/primitive | 1 / 1 / 1 / 1 |
| Vertices | 249,567 |
| Indices / triangles | 1,499,094 / 499,698 |
| Attributes | `POSITION` only |
| Index type | unsigned 32-bit |
| Bounds | min `[-0.2395618111, 0, -0.5721657872]`; max `[0.2426466793, 0.5098826885, 0.5721297860]` |
| Duplicate/unused vertices | 0 / 0 |
| Degenerate/zero-area triangles | 0 / 0 |
| Boundary/non-manifold edges | 0 / 0 |
| Vertex-connected components | 1 |
| Material colour / texture | absent / absent |

The artifact is a credible watertight single-component geometry candidate. It does not retain the source image colour,
semantic part labels or a detached turret/barrel hierarchy.

## 5. Format and orientation facts

The official glTF 2.0 specification establishes a right-handed coordinate system, `+Y` up, `+Z` forward, metres as
linear units, and node transform composition as `T * R * S`. `POSITION` is finite `VEC3/FLOAT`; indexed triangle
primitives reference it through accessors and buffer views.

The existing RA2 voxel core uses `X,Y,Z` local voxel coordinates and the VXLSE bridge already freezes its own raster
permutation. 1D therefore needs one explicit source-to-canonical map before voxelization:

```text
canonical X <- glTF X       (right)
canonical Y <- glTF Z       (forward)
canonical Z <- glTF Y       (up)
```

The map is format-derived, not inferred from the picture. Mount/pivot orientation in the game is still unverified and
must remain a review fact rather than an acceptance claim.

## 6. Missing capability

No repository implementation currently:

- parses GLB mesh geometry in Application;
- applies bounded glTF scene/node transforms;
- audits triangle topology as typed facts;
- converts triangles to surface cells;
- fills the interior of a watertight mesh;
- normalizes scale/origin into bounded canonical voxel dimensions; or
- records voxelization-specific review/failure facts.

There is no SharpGLTF, Assimp, OpenTK or Silk.NET dependency. Adding one for the restricted current format would expand
runtime/package/licensing surface and is not necessary for the certified sample.

## 7. Consequences for the contract

1. Add an internal, BCL-only, restricted GLB reader and transient mesh snapshot in Application.
2. Flatten supported transformed triangle primitives into one caller-declared part; do not infer semantic parts.
3. Use deterministic triangle/AABB surface intersection followed by bounded exterior flood fill for watertight solid fill.
4. Require an explicit palette profile and explicit target colour/index policy. Missing GLB material is not an error.
5. Produce a canonical snapshot plus typed topology/normalization/voxelization/review facts.
6. Fail before snapshot creation on malformed buffers, unsupported primitive shape, non-finite values, out-of-range
   indices, open/non-manifold geometry, resource overflow or empty output.
7. Keep all new types internal. Application public allowlist remains 77; AssetHost public exports remain zero.
8. Defer AssetHost lease-to-Application composition, semantic part segmentation, colour recovery, normals, HVA and final
   VXL writing to later stages.

## 8. Risk classification

`R4 / StopForReview`.

The bridge creates a new path into canonical voxel truth and handles untrusted binary structure. Implementation is not
authorized by completion of this audit alone. The final contract requires explicit approval.

## 9. References

- Khronos glTF 2.0 specification: <https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html>
- Tomas Akenine-Möller, *Fast 3D Triangle-Box Overlap Testing*:
  <https://doi.org/10.1145/1198555.1198747>
- `Docs/ASSET-VOX-1B_CanonicalVoxelCoreFinalContract.md`
- `Docs/ASSET-VOX-1C_GenerationProviderHostFinalContract.md`
- `Docs/ASSET-VOX-1C-P2_TencentHy3DRemoteProviderFinalContract.md`
