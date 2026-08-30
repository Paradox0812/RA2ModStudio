# ASSET-VOX-2A Quality Refinement Code Fact Audit

Date: 2026-08-27

State: Completed / reconciled with implementation

Implementation state: 2A-1 through 2A-5 implemented and automated verified

## 1. User-visible problem

The current certified Body candidate is recognizable as an armoured vehicle, but it is not yet a production-quality
voxel asset. Physical 3D review shows:

- dense local bumps and stair-step noise on nominally planar armour;
- visibly inconsistent left/right body and gun-shield silhouettes;
- weak separation between armour, tyres, glass, metal and accent regions; and
- an almost uniform low-contrast green result after style compilation.

These are asset-generation and conversion-quality defects. The native WPF 3D viewport is displaying the canonical
snapshot faithfully; replacing the viewport will not repair them.

## 2. Current geometry path

```text
reference image
  -> Tencent Hunyuan 3D 3.1 Geometry GLB
  -> restricted GLB reader
  -> exact transformed triangle mesh
  -> direct triangle/AABB surface rasterization at the target grid
  -> exterior flood fill
  -> Ra2VoxelSceneSnapshot
```

Current audited facts:

- The provider request sends one image with `GenerateType = Geometry` and `EnablePBR = false`.
- The provider adapter does not send the user's natural-language prompt.
- The certified GLB contains POSITION geometry only; it has no colour, texture, material or semantic part labels.
- The current voxelizer performs no mesh denoising, anti-aliasing, supersampling, signed-distance filtering,
  morphological repair, feature protection or symmetry correction.
- The accepted real candidate is `29 x 64 x 31`, contains 20,261 occupied cells and is one connected Body candidate.
- The current canonical snapshot already owns ordering, hashing, connectivity and global X-symmetry facts.
- A conversion request still produces exactly one caller-declared part; no current algorithm can reliably split Body,
  Turret or Barrel from the single connected GLB.

Direct target-resolution rasterization explains why high-frequency mesh detail aliases into isolated voxel bumps. It
also means that repairing only the final rendered image would hide, rather than fix, the source snapshot.

## 3. Current colour path

```text
natural-language style sources
  -> one structured DeepSeek style-plan call
  -> local plan validation
  -> deterministic geometry regions
       top / side / underside / edge / interior
  -> nearest valid palette index
  -> recoloured snapshot with unchanged occupancy
```

Current audited facts:

- Without a semantic mask, tyre, glass, bare-metal, weapon, light and remap roles remain unresolved.
- The model is not allowed to return per-cell colours or occupancy.
- The colourizer correctly preserves geometry, but geometry-only regions cannot distinguish a wheel from an armour
  panel merely because both occupy a side-facing surface.
- The current screenshot's body roles map to a narrow group of neighbouring green palette indices. Local validation
  checks validity but does not yet require useful perceptual separation between light, base, dark and material roles.

The colour problem is therefore not primarily a PAL requirement. A PAL is needed to map approved roles to executable
RA2 indices, but it cannot supply missing semantic region evidence.

## 4. Current DeepSeek boundary

`Ra2AiRequest` and `DeepSeekRa2AiClient` currently serialize message `content` as strings plus optional function tools.
There is no image-content payload, 3D file attachment or multimodal evidence contract in the current client.

Consequences:

1. DeepSeek may reason over bounded textual geometry facts and select a local deterministic operation plan.
2. It cannot currently inspect the reference image, GLB or rendered views directly.
3. Multiple calls do not create missing visual evidence. A repeated text-only guess must never be promoted to an
   authoritative tyre/glass/gun-shield mask.
4. Real visual semantic recognition requires a separately approved vision-capable adapter or explicit user masks.

## 5. Existing reuse candidates

| Need | Existing authority to reuse |
|---|---|
| canonical occupied cells and hashes | `Ra2VoxelSceneSnapshot` |
| neighbourhood and visible faces | `Ra2VoxelSurfaceProjection` / shared voxel neighbourhood checks |
| normals and orientation evidence | `Ra2VoxelNormalField` |
| palette decoding and nearest executable colour | `Ra2VoxelPaletteProfile` |
| style-plan compilation/cache | `Ra2VoxelStyleCompiler` / `Ra2VoxelStylePlanCache` |
| style recolouring and review package | `Ra2VoxelColourizer` / `Ra2VoxelColourReviewPackage` |
| interactive visual review | existing `Ra2VoxelViewport3D` |
| Body/Turret/Barrel ownership | `Ra2VoxelAssetAssemblySpec` |

No second snapshot, palette, part graph, DeepSeek client or viewport authority is justified.

## 6. Root-cause conclusion

The current result has three independent limitations:

1. **provider geometry limitation**: single-image shape generation contains uncertain/asymmetric detail;
2. **conversion limitation**: direct target-grid rasterization preserves and aliases that uncertainty;
3. **semantic limitation**: geometry-only style regions cannot identify functional materials.

ASSET-VOX-2A can reliably reduce conversion noise, measure and optionally correct high-confidence symmetry, and improve
palette contrast. It cannot guarantee recovery of geometry absent from the GLB or authoritative visual part/material
recognition from the current text-only DeepSeek transport.

The user explicitly excluded original-model adjustment during execution. The final implementation therefore leaves mesh
vertices/triangles untouched and performs supersampling, bounded occupancy cleanup and symmetry only as derived voxel
candidates. This narrows risk and removes the proposed mesh-preconditioner dependency without changing the accepted
canonical snapshot authority.

## 7. Risk and governance

- Contract writing risk: `R0 / Immediate` (documents only).
- Implemented change risk: `R4 / approved bounded execution` because a refinement candidate can change canonical occupancy and
  because AI evidence must not become geometry authority.
- Real multi-round DeepSeek execution is a separate cost gate. This audit grants no paid call, retry or provider change.
- No public API, persistence, project-write, VXL/HVA writer, Shell or UI change is authorized by this audit.
