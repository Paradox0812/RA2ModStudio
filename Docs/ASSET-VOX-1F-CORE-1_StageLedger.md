# ASSET-VOX-1F-CORE-1 Stage Ledger

## CORE-1A Reuse and ownership audit

- Status: completed
- Result: selected visible-face extraction and normal palette/baking; rejected whole-project dependency, mutable VXL model,
  CLI, OBJ bridge and writer migration.

## CORE-1B Format-neutral surface projection

- Status: completed
- Result: immutable source-hash-bound faces, stable direction order, palette indices, surface-cell facts and typed limit/cancel.

## CORE-1C Derived normal field

- Status: completed
- Result: exact RA2/TS palettes, bounded estimation/smoothing/quantization and deterministic field hash without snapshot mutation.

## CORE-1D VOX compatibility and regression

- Status: completed
- Result: MagicaVoxel decode uses the same projection/bake path; existing VXL and colourizer paths retain the canonical snapshot boundary.

## CORE-1E Verification closeout

- Status: completed with one known unrelated WPF full-suite flake isolated and passed
- Evidence:
  - new focused tests: 6/6
  - all Application voxel tests: 48/48
  - Application: 255/255
  - AssetHost: 47/47
  - Release solution build: 0 errors / 1 pre-existing nullable warning
  - IDE full: 2798/2799 due to the documented WPF Popup/resource teardown flake
  - isolated failing WPF test rerun: 1/1
  - clean package: final gate command is recorded in the task delivery report

## Deferred governance queue

- Native 3D viewport consuming the surface projection.
- Optional normal-index visualization and before/after review.
- Explicit preservation/import of existing VXL normal indices.
- Hardened VXL writer and atomic materialization under a separate contract.
