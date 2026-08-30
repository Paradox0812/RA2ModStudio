# ASSET-VOX-2A Stage Result Ledger

Date: 2026-08-27

State: Completed / automated verified / live-provider and physical visual acceptance not run

## Scope correction

The user excluded original-model adjustment. All stages therefore treat the admitted mesh and existing canonical
snapshots as immutable evidence. No source vertex, triangle, provider request or generation result is changed.

## 2A-1 — Quality facts and protection

- Added deterministic surface, exposed-face roughness, low-support, fixed six-view silhouette, global X-symmetry and
  locally supported thin-feature facts.
- Added a source-hash-bound binary protection mask.
- Gate: deterministic analysis, complete silhouettes, isolated-bump rejection and cancellation fixtures pass.
- Review: no new canonical model, persistence or public API authority.

## 2A-2 — Supersampled conversion

- Reused `Ra2MeshVoxelizer` at direct and bounded 2x resolutions; longest transient dimension is capped at 128.
- Downsampling uses frozen coverage voting, one bounded cleanup pass and exact protected-coordinate union.
- Candidate retains source dimensions and must retain one dominant body; small detached attachments are review facts,
  while output without a component containing at least 95% of occupied cells is rejected. Source mesh/snapshot remain immutable.
- Gate: conversion, determinism, dimensions, connectivity and cancellation fixtures pass.

## 2A-3 — Symmetry suggestion

- Added `Off` and review-only `Suggest`; there is no silent enforcement mode.
- Local mirrored support may add a missing mirror or remove a one-cell unsupported bump while protected coordinates remain
  untouched.
- Volume/silhouette gates and non-regression of unmatched cells reject unsafe suggestions.
- Gate: symmetric/asymmetric/protected-source fixtures pass.

## 2A-4 — Bounded DeepSeek coordinator

- Added three distinct structured rounds: diagnosis, bounded plan and review.
- The diagnosis may stop the sequence after one call; an exact fully keyed cache hit makes zero calls.
- Each round requires exactly one named tool call. Malformed/provider/cancel outcomes are typed; a fourth call is
  structurally impossible.
- This stage is an internal headless seam only and is not wired to UI or live provider execution.
- Gate: 5 fake-client tests pass; zero network calls.

## 2A-5 — Semantic, palette and review facts

- Review package includes source/refined/symmetry facts, deterministic normal-field comparison and four provenance-tagged
  semantic-region candidates.
- Added palette contrast candidate for weak body-role separation. It preserves exact palette selections, semantic roles,
  remap roles, rules and the original plan; it never rejects ordinary colour solely because ideal separation is absent.
- Gate: palette improvement/preservation/determinism fixtures pass.

## Verification

```text
Application.Tests: 264/264 passed
IDE.Tests: 2807/2807 passed (isolated output used because the running IDE locked default output DLLs)
AssetHost.Tests: 47/47 passed
IDE-only solution build: 0 errors, 1 pre-existing nullable warning
IdeOnly clean source package: 1361 files; package hygiene exclusions passed
Focused new tests: geometry/palette 9/9; coordinator 5/5
Real Tencent calls: 0
Real DeepSeek calls: 0
```

## 2026-08-27 connectivity correction verification

```text
Real product-path probe: body-candidate.vox + mesh.glb passed; refined 17,397 cells; 1 component; 100% dominant share
Default coverage: 40%; occupied-volume gate remains 5%; silhouette gate remains 3%
Focused refinement: 9/9 passed
Focused IDE candidate/UI lifecycle: 16/16 passed
Application.Tests: 267/267 passed
IDE.Tests: 2814/2814 passed
AssetHost.Tests: 47/47 passed
IDE-only solution build: 0 errors, 1 pre-existing nullable warning
IdeOnly clean source package: 1366 files
Real Tencent/DeepSeek calls: 0
```

## Boundaries preserved

- Application public API: no exported change.
- Persistent/cache schema: no new on-disk format; coordinator cache is process-memory only.
- Shell/XAML/docking/AutomationIds: unchanged.
- INI, Field Registry, diagnostics, completion, Work mode, Apply/Save/Undo: unchanged.
- AssetHost protocol and Tencent provider: unchanged.
- VXL/HVA writer, project asset materialization and game validation: not added.

## Remaining risks

1. This stage cannot repair geometry missing from the provider GLB; the user deliberately excluded that work.
2. Text-only DeepSeek cannot authoritatively identify tyre, glass, gun shield or weapons from images.
3. The new headless candidates are not yet composed into the style workspace UI.
4. Physical visual comparison of direct/refined/symmetry candidates remains a separate UI/acceptance stage.

## Recommended next stage

`ASSET-VOX-2A-UI Review Candidate Composition`: expose direct/refined/symmetry and before/after quality facts in the
existing 3D workspace without Apply/Save or source replacement. A live DeepSeek trial remains independently gated.
