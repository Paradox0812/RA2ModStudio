# ASSET-VOX-1A Golden Probe and Separated Assembly Final Contract

Status: Approved by user / implementation completed / VXLSE source contract verified / executable round trip deferred  
Date: 2026-08-26

## 1. Objective

Establish the first trustworthy, UI-neutral baseline for Agent-authored RA2/YR voxel assets. The baseline represents
detached body, turret and optional barrel files as one assembly, inspects bounded VXL/HVA metadata without writing
project files, and freezes the exact slice-addressing and palette rules exposed by the supplied VXLSE III source build.

This stage does not generate geometry, decode voxel spans, write VXL/HVA, calculate normals, call a model provider,
change INI files, or add UI.

## 2. Authority and reuse

- `Ra2VoxelAssetAssemblySpec` owns request-lifetime authoring topology only. It is not persisted and is not an Asset Manifest.
- `Ra2VoxelBinaryProbe` owns bounded read-only VXL/HVA metadata facts. It does not write stream content or retain binary data.
- `Ra2VoxelAssemblyProbe` verifies that every declared part has the expected VXL/HVA file and Section closure.
- Final `.vxl/.hva` artifacts continue to enter the product through the existing `Ra2AutomationAssetManifest` and
  `IRa2AutomationAssetProvider` boundary after later validation stages.
- The user explicitly authorized reuse of the local `VoxelNormalForge` source. Stage 1A uses it as a format/reference
  oracle; Stage 1B may migrate reviewed reader/writer and normal logic into the canonical voxel core. Its CLI/WPF layers
  are not imported.

All new .NET types remain `internal` until axis, slice and VXLSE import behavior is frozen. This stage adds no exported API.

## 3. Separated assembly contract

An assembly contains 1..16 parts and exactly one root `Body`. Every non-body part has a parent, all identities and file
stems are unique case-insensitively, and the graph must be acyclic and connected to the Body.

Supported roles at the baseline are:

- `Body`
- `Turret`
- `Barrel`
- `Other`

Each part declares:

- stable `PartId`;
- role and optional `ParentPartId`;
- extension-free output `FileStem`;
- expected VXL/HVA Section name;
- whether an HVA companion is required.

The contract supports `Body -> Turret -> Barrel`, `Body -> Turret`, multiple turrets and additional attached parts without
hard-coding a single tank layout. A real detached Barrel sample is therefore useful for later visual calibration but is
not a schema or implementation prerequisite. Mount coordinates, pivot and animation axes remain explicit later-stage data.

## 4. Binary probe limits and behavior

- Input must be readable and seekable and must not exceed the existing 16 MiB per-asset limit.
- VXL: signature `Voxel Animation`, 1..16 palettes, 1..256 Sections, bounded footer offsets, finite transforms/bounds,
  positive scale/dimensions, unique non-empty Section names.
- HVA: 1..4096 frames, 1..256 Sections, at most 65,536 transforms, finite matrix components and sufficient content.
- One unnamed HVA Section is accepted as a legacy compatibility fact only when its paired VXL has exactly one expected
  Section. Multiple unnamed or duplicate HVA Section identities remain ambiguous and are rejected.
- Any failed assembly result contains no partial successful part payload.
- Assembly artifacts form an exact case-insensitive closure: missing, unexpected or case-ambiguous names fail explicitly.
- Probe diagnostics distinguish invalid input, unsupported signature, truncation, resource limit and invalid structure.

The probe does not yet prove voxel span integrity, palette quality, normal correctness, HVA motion quality or game safety.

## 5. Confirmed local evidence

The implementation probe successfully read these external, non-repository samples:

| Sample | SHA-256 | Facts |
|---|---|---|
| `tnkd.vxl` | `DC75ED32CCEC37F8CF23A6E3D7218A02053A456B823990D690F025404A3021B9` | `Body`, 80x37x30, normal type 4 |
| `tnkdtur.vxl` | `B6302DC86CA45F6302BCE4844AB153037E4284BBC577BE2B8A258E94A944C39E` | `Body`, 80x37x30, normal type 4 |
| `ttank.vxl` | `B9FFCA493C8D431451735BB4CA991F730C8AFFCC28B124DA6C58CAE98D11B350` | `Body`, 89x54x19, normal type 4 |
| `ttanktur.vxl` | `9882E4FDF6519B7341B85B70FCAF241163849C1663ED8278DA704462BC4D29C5` | `Body`, 96x41x29, normal type 4 |

All four companion HVA files are one-frame/one-Section and contain finite transforms. `ttank.hva` has an unnamed single
Section; the other three identify `Body`. No external binary is copied into the repository.

## 6. Supplied VXLSE III compatibility facts

The supplied executable is:

- path: `H:\RA2\vxlse3 MagicalVoxel导入版本\vxlse3 MagicalVoxel导入版本\vxlse_III.exe`;
- file version: `1.3.9.3281`;
- product version: `1.4.0.0`;
- SHA-256: `DB9A882A74E16ECB1D938C6D07EC4C97B28D51EF23975730DF2211E354916458`.

The adjacent source freezes these importer rules:

- VXL dimensions and the dialog offset are 1..255.
- Default `Downward` layout uses `offset=Z`, PNG `width=X`, `height=Y*Z`.
- `Rightward` layout uses `offset=X`, PNG `width=Y*X`, `height=Z`.
- Both directions place the highest VXL `Y` slice first. Within a slice, PNG X maps to VXL X and PNG row maps to VXL Z.
- The importer uses integer division and does not reject remainder pixels. IDE-generated packages must require the exact
  dimensions above instead of relying on VXLSE truncation.
- Occupancy is `alpha == 0` empty and any non-zero alpha occupied.
- The PNG must have a direct RGB-alpha or grayscale-alpha channel. Palette PNG plus `tRNS`, RGB-only PNG and inferred
  transparency are incompatible with this importer path.
- RGB is always mapped to the active VXLSE palette. The supplied binary `.pal` format is 256 RGB triples in 0..63 and
  VXLSE expands channels with `value*4`; nearest-colour selection uses RGB Euclidean distance with normalized colour
  structure as the equal-distance tie breaker. Exact palette RGB values are therefore required for lossless indices.
- Transparent pixels are not written back as empty voxels. Import must target a new or explicitly cleared Section.
- The importer writes occupancy and colour but not normals. Normal regeneration is mandatory after every import.
- `Resize` calls `DefaultTransforms`, which overwrites bounds according to VXLSE's session-global land/air choice. Slice
  import cannot establish a trustworthy part pivot or mount position; those remain explicit authored facts.
- The executable accepts a VXL path on startup but exposes no command-line slice-import protocol. Production automation
  must generate an import package and must not depend on GUI click automation.

`Ra2VxlseSliceImportContract` implements only these deterministic, source-backed facts. It performs no PNG I/O and no
VXL mutation.

## 7. Still-unfrozen facts

The following remain `Unknown / Pending VXLSE probe` and must not be hard-coded:

- world/local axis mapping between source mesh, canonical voxel scene, VXL and HVA;
- detached turret/barrel mount offsets and rotation origins;
- normal generation parity with VXLSE and the game;
- multi-frame HVA interpolation/loop behavior;
- direct writer game compatibility.

The supplied source and executable are adjacent in one distribution, but binary/source identity has not been proven by a
reproducible build. A later generated PNG -> VXLSE -> decoded VXL round trip remains an independent acceptance check.

## 8. User-supplied material status

Received:

1. VXLSE III executable, adjacent source and RA2 `unitsno/unittem/uniturb` palettes.
2. Real detached Body+Turret samples.
3. Authorization to use `VoxelNormalForge` as an independent reference implementation.

Not blocking Stage 1A:

- no real detached Barrel sample; a 3x4x5 asymmetric synthetic Barrel closes coordinate identity, while a real sample is
  deferred to later pivot/visual/game calibration;
- no selected production theatre palette; the package can carry an explicit palette profile and must not infer one.

Needed later, not required for the present code baseline:

- one visual reference and desired unit style for candidate generation;
- the intended image-to-3D provider or local GPU constraints;
- a game smoke-test project/map for final VXL/HVA acceptance.

## 9. Verification contract

- Targeted assembly/probe tests must cover valid Body/Turret/Barrel closure, graph cycles, truncation, non-finite HVA,
  missing companion files, Section mismatch and the single unnamed legacy HVA case.
- At least two existing real detached assets must pass the binary probe without being committed to source control.
- IDE-only restore/build, Application tests and clean-package checks remain required at the stage stop point.
- Both VXLSE slice directions must round-trip every coordinate of a 3x4x5 asymmetric synthetic part.
- Import preflight must reject wrong dimensions, absent direct alpha and a non-empty target Section.
- Westwood PAL decoding, occupancy and nearest-colour behavior must have deterministic tests.
- Actual executable import, visual output and game smoke remain `NotRun` until Stage 1B owns a deterministic PNG package
  exporter; they are acceptance evidence, not a reason to encode GUI automation into the product.

## 10. Stop rule and next stage

Stage 1A is complete when assembly/binary probes and the source-backed VXLSE slice contract pass automated verification.
It does not claim an executable GUI import, visual quality, normal parity or game readiness. The next implementation stage
is `ASSET-VOX-1B Canonical Voxel Core`, starting with reviewed migration of the user-authorized VoxelNormalForge
reader/writer, a deterministic in-memory voxel snapshot and a PNG exporter whose output can perform the deferred
executable round trip.
