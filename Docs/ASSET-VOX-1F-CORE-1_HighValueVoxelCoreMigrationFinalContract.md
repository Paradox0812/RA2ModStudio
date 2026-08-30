# ASSET-VOX-1F-CORE-1 High-Value Voxel Core Migration Final Contract

Date: 2026-08-27  
State: implemented after self-review under the user's instruction to migrate the highest-value VoxelNormalForge parts and support VOX  
Risk: R2 / Immediate governance

## Outcome

The Application owns one bounded, immutable and format-neutral surface projection plus one derived VXL-normal field pipeline.
Both operate only on `Ra2VoxelSceneSnapshot`, so MagicaVoxel VOX and Westwood VXL inputs share the same algorithms after
their existing canonical decoders have completed.

## Migrated capabilities

- Deterministic six-direction visible-face extraction derived from the reviewed VoxelNormalForge OBJ surface algorithm.
- Shared canonical occupancy/neighbourhood checks reused by style geometry classification, surface projection and normal baking.
- Exact 244-direction RA2/YR and 36-direction TS normal palettes with maximum-dot-product quantization.
- Bounded radius-based surface-normal estimation, optional bounded smoothing, immutable quantized normal field and provenance hash.
- Typed cancellation and resource-limit failures; no partial projection or normal field is published.

## Data ownership

- `Ra2VoxelSceneSnapshot` remains the only canonical voxel truth and its schema is unchanged.
- Surface faces and normal samples are read-only derived data bound to the source `CanonicalHash`.
- A normal field is not serialized into VOX or VXL and does not mutate source cells.
- VOX has no VXL `normalIndex`; it therefore receives a generated review field through the same geometry path.
- Existing VXL `normalIndex` values remain outside the canonical snapshot and are not silently treated as preserved input.

## Explicit exclusions

- No dependency on the VoxelNormalForge project, mutable `VxlModel`, CLI or OBJ file bridge.
- No VXL/HVA writer, original-normal blending, project Apply/Save or game-ready claim.
- No Shell, XAML, 3D viewport, DeepSeek, INI, Field Registry, parser, diagnostics or completion change.
- No public API, serialized schema, package dependency or project-file change.

## Next use

The surface projection is the direct geometry input for `ASSET-VOX-1E-UI-3D`. The normal field is the reviewed input for a
later normal visualization/bake review stage; VXL persistence requires a separate writer contract that explicitly owns
normal-index preservation and atomic file materialization.

## Verification contract

- Single cell, solid cube, shared-face culling, deterministic face order, budget and cancellation.
- VOX codec round-trip followed by identical surface and normal output.
- Exact RA2/TS palette counts, unit vectors and nearest-direction quantization.
- Deterministic normal field, TS output bounds, sample budget and cancellation.
- Existing voxel/style tests, full Application tests, solution build, IDE tests and clean package.

