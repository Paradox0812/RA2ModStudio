# ASSET-VOX-1B Canonical Voxel Core Final Contract

Date: 2026-08-26  
State: Completed / supplied VXLSE structural acceptance passed

## 1. Outcome

Stage 1B establishes a deterministic, UI-neutral voxel core. It does not generate final game-ready VXL/HVA assets.
The canonical authority is one immutable `Ra2VoxelSceneSnapshot` per asset part; detached Body, Turret and Barrel
relationships remain owned by the Stage 1A `Ra2VoxelAssetAssemblySpec` graph.

## 2. In scope

1. An immutable, bounded single-part voxel snapshot with a 256-entry RGBA palette, sparse cells, coordinate metadata,
   source hashes and a canonical SHA-256 hash.
2. A deterministic palette profile and nearest-colour quantizer.
3. A restricted MagicaVoxel `.vox` v150 reader/writer supporting one `SIZE`/`XYZI` model and an explicit `RGBA`
   palette. Unknown chunks are skipped within declared bounds; scene-graph/material semantics are not inferred.
4. A reviewed, bounded Westwood `.vxl` span reader derived from the user-authorized VoxelNormalForge source. It may
   decode multiple Sections into separate canonical snapshots but does not write VXL.
5. A VXLSE-compatible flattened RGBA SliceStack raster for the source-backed Downward/Rightward addressing contract,
   plus a restricted deterministic PNG encoder/decoder.
6. Property-style and malicious-input tests proving stable hashes, deterministic bytes, coordinate round trips,
   duplicate rejection, bounded allocation and truncation handling.

## 3. Explicitly out of scope

- WPF, Shell, AutomationIds, model/provider calls, AI prompts or project writes.
- Direct VXL compilation, HVA generation, normal baking, pivot/mount calibration or game-readiness claims.
- Arbitrary file-system paths. All codecs operate on caller-owned streams or byte arrays.
- MagicaVoxel scene graphs, transforms, materials, animation, multiple models or palette-less files.
- Lossy images, resizing, interpolation, antialiasing or implicit colour management.

## 4. Canonical rules

- Schema version is fixed at `1`.
- Dimensions are `1..255`; coordinates must be inside bounds; cells are unique and stored in `Z, Y, X` order.
- Palette length is exactly 256. Transparent and remap index sets are sorted, unique and bounded to `0..255`;
  a VXL-derived profile may have no transparent palette index because VXL occupancy is span-based.
- Occupied cells cannot use a transparent palette index.
- Source artifact hashes are normalized uppercase SHA-256 strings and sorted by name, case-insensitively.
- All strings are length-bounded and reject CR, LF and NUL.
- Scale, origin, pivot and transform values must be finite.
- Canonical hashing uses a versioned binary representation with explicit little-endian integers and UTF-8 length
  prefixes; it never depends on JSON property order, runtime hash codes or current culture.

## 5. Codec boundaries

### MagicaVoxel `.vox`

- Signature `VOX ` and version `150` or newer are accepted.
- `MAIN` must be the root and every chunk must remain within both stream and parent bounds.
- Exactly one `SIZE`, one following `XYZI`, and one `RGBA` are required.
- Dimensions and voxel count must fit the canonical limits; duplicate coordinates, colour index `0`, out-of-range
  coordinates and inconsistent chunk lengths are rejected.
- Writer emits version 150, `MAIN`, `SIZE`, `XYZI`, `RGBA` in stable order. Palette index `0` remains the canonical
  transparent slot; occupied cells use MagicaVoxel indices `1..255`.

### Westwood `.vxl`

- The Stage 1A signature/count/size limits remain authoritative.
- Every body/info/table/span offset is checked before seeking or allocating.
- The VXL header's 768-byte `PaletteData` block is reserved/unused by VXLSE and is not treated as authoritative colour
  data. The caller must supply the active external palette profile used by the asset.
- Span start/end pairs, skip/run lengths, duplicated run count, Z bounds and duplicate coordinates are validated.
- Each VXL Section becomes one canonical snapshot. Embedded normal indices and Section transforms are read for
  validation but normals are not canonical cell data in 1B.
- No VXL writer is exposed in 1B; direct compiler feasibility remains Stage 1F.

### SliceStack PNG

- The raster layout is exactly the Stage 1A VXLSE contract; highest Y block is first.
- The user-supplied MagicalVoxel import executable adds a confirmed axis bridge after reading the raster:
  input `(x,y,z)` becomes VXL `(z, inputXSize-1-x, y)`. Canonical export therefore applies the inverse mapping and
  uses an import volume of canonical `Y,Z,X`. This executable-specific bridge is additive; it does not change the raw
  generic SliceStack codec or the standard VXL X/Y/Z reader.
- Empty pixels are RGBA `0,0,0,0`; occupied pixels use palette RGB and alpha `255`.
- PNG is limited to one non-interlaced 8-bit RGBA image. Encoder uses filter 0 and deterministic zlib output.
- Decoder validates signature, CRC, chunk ordering, dimensions, colour type, decompressed size and PNG filters 0..4.
- Import quantizes each non-zero-alpha pixel against non-transparent palette entries and reconstructs a snapshot.

## 6. Limits

- Maximum part dimension: 255.
- Maximum cells per part: 1,000,000, additionally bounded by the volume implied by dimensions.
- Maximum VXL Sections: 16; maximum VXL palette count: 1.
- Maximum `.vox` chunk count: 4096; maximum encoded input: 256 MiB.
- Maximum PNG encoded input: 256 MiB; maximum decoded raster bytes: 128 MiB.
- Maximum identity: 64 UTF-16 code units; VXL Section name: 16 ASCII bytes.

## 7. Failure behavior

Malformed binary input raises `InvalidDataException`; invalid caller construction raises argument exceptions. A failure
returns no partial snapshot, does not write a path and does not mutate an existing snapshot.

## 8. Verification matrix

| Area | Required evidence |
|---|---|
| Snapshot | order-independent construction, immutable copies, stable and mutation-sensitive canonical hash |
| Palette | deterministic exact/nearest selection, transparent-index exclusion, stable profile hash |
| VOX | deterministic write/read round trip, unknown bounded chunk skip, truncation/duplicate/oversize rejection |
| VXL | VoxelNormalForge-compatible synthetic span decode, multi-Section identity, malformed offset/run rejection |
| Slice | every asymmetric coordinate round trip in both directions, exact RGBA occupancy and palette preservation |
| PNG | deterministic bytes, CRC/filter decode, wrong format/truncation/decompression-limit rejection |
| Regression | focused tests, all Application tests, IDE-only build/test, clean source package |

## 9. Stop rule

Stage 1B is complete only when all pure-core tests pass and the explicit supplied-VXLSE readback acceptance proves the
asymmetric fixture's dimensions, occupancy, coordinates and palette indices. Visual quality, pivot/mount correctness,
normal quality, HVA and in-game smoke remain unverified. Any need for WPF, a new dependency, public API or persistence
format stops the stage instead of silently expanding it.

## 10. Executable acceptance result

The deterministic acceptance fixture is generated at:

```text
artifacts/VxlseAcceptance/vxlse-acceptance-downward.png
artifacts/VxlseAcceptance/unittem.pal
artifacts/VxlseAcceptance/expected.txt
```

The supplied VXLSE executable is a classic Delphi application that could not be targeted by the available UI provider.
The user therefore completed the bounded GUI import/save step manually:

1. In VXLSE choose `File > New > Red Alert 2`; the initial size is irrelevant because import resizes the Section.
2. Choose `Edit > Import Slices`, browse to `vxlse-acceptance-downward.png`, select `Downward`, and set offset `3`.
3. Accept the import confirmation and save the fresh result. The accepted run replaced
   `artifacts/VxlseAcceptance/vxlse-acceptance.vxl`.

Re-importing into the first acceptance file is invalid: the supplied importer only calls `SetVoxel` for non-transparent
pixels and does not clear an old occupied cell when the replacement pixel has alpha zero. The target Section must be
new and empty, as required by the preflight contract.

The explicit `verify` acceptance decoded that file through `Ra2WestwoodVxlReader` and compared all five expected
coordinates and palette indices. It passed with Section `Body`, dimensions `3x4x5`, occupancy `5` and canonical hash
`29A4A1150EEFB6305021B29CA37B7C3F58B0B845FEB779C63F93EA0DCF0161C2`.

This closes executable structural compatibility only. Normals, pivot/mount placement, HVA animation, visual quality and
in-game behavior remain outside Stage 1B and are not implied by the passing readback.
