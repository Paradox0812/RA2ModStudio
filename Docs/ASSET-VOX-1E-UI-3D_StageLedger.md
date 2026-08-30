# ASSET-VOX-1E-UI-3D Stage Ledger

## UI-3D-A Contract, risk and reuse gate

- Status: completed
- Result: R3/Immediate was explicitly approved; canonical snapshot, surface projector and geometry-region colours are reused.

## UI-3D-B Native scene adapter

- Status: completed
- Result: bounded visible faces are grouped into frozen WPF `MeshGeometry3D` material batches; no dependency or format path was added.

## UI-3D-C Interactive viewport and workspace routing

- Status: completed
- Result: original/result/region use orbit/pan/zoom/reset 3D; Palette remains 2D; SliceStack is an explicit/failure fallback.

## UI-3D-D Lifecycle and verification

- Status: completed / automated verified
- Evidence:
  - focused viewport/coordinator/UI/STA tests: 29/29
  - Application voxel tests: 48/48
  - Application full: 255/255
  - AssetHost full: 47/47
  - IDE full: 2801/2802; only the documented WPF Popup/resource teardown flake failed
  - isolated WPF flake rerun: 1/1
  - Release solution build: 0 errors / one pre-existing nullable test warning
- Manual 1920x1080 interaction/screenshot: pending user acceptance

## Deferred governance queue

- multi-part Body/Turret/Barrel composition and part visibility controls;
- normal-index visualization and game-lighting comparison;
- accepted-style handoff, VXL/HVA materialization, project Apply/Save and game validation.
