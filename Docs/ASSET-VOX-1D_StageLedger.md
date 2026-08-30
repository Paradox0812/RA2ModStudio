# ASSET-VOX-1D Stage Result Ledger

Date: 2026-08-26  
Package: GLB-to-Canonical-Voxel Bridge

## Stage status

| Stage | Goal | Files touched | Verification | State after stage | Next entry satisfied |
|---|---|---|---|---|---|
| 1D-0A | Audit 1B/1C/real GLB and authoritative format facts | code-fact audit | Docs/evidence review | Completed | 1D-0B |
| 1D-0B | Freeze R4 data/algorithm/failure/verification contract | final contract, ledger and governance candidates | DocsOnly structural validation passed; user approved | Completed | 1D-1 |
| 1D-1 | Restricted GLB reader and mesh/topology snapshot | `Ra2GlbMeshReader.cs`, focused tests | parser/topology/failure matrix | Completed | 1D-2 |
| 1D-2 | Deterministic transform, axis and scale normalization | `Ra2GlbMeshReader.cs`, `Ra2MeshVoxelizer.cs` | TRS/axis/dimension/determinism tests | Completed | 1D-3 |
| 1D-3 | Triangle/AABB surface rasterization and watertight fill | `Ra2MeshVoxelizer.cs`, focused tests | analytic solid/open/non-manifold/degenerate/cancel tests | Completed | 1D-4 |
| 1D-4 | Palette policy and canonical snapshot/result facts | `Ra2MeshVoxelizer.cs`, focused tests | review flags/hash/VOX/SliceStack 10/10 | Completed | 1D-5 |
| 1D-5 | Real GLB acceptance, VOX/SliceStack regression and closeout | real acceptance artifacts, tests and docs | real 1/1; Application 238/238; IDE 2779/2779; Host 47/47; build 0 errors; package 1315 | Completed | Later product composition audit |

## Contract self-review

| Gate | Result | Evidence |
|---|---|---|
| Architecture ownership | Passed | Host remains orchestration; Application remains deterministic algorithms |
| Reuse | Passed | 1A assembly, 1B snapshot/palette/VOX/SliceStack remain authorities |
| Data lifetime | Passed | GLB/mesh/result are in-memory request-lifetime values |
| Public API | Passed | internal-only plan; exported allowlist remains 77 |
| Resource bounds | Passed | bytes, graph, vertices, triangles and cells are explicitly capped |
| Semantic honesty | Passed | no semantic split, colour recovery, final pivot, HVA or GameReady claim |
| Verification design | Passed | malformed, analytic geometry, determinism, real artifact and regression gates |
| R4 approval | Passed | user explicitly approved continuous 1D-1 through 1D-5 execution |

## Deferred governance queue

### Public API

| Stage | Entry | State |
|---|---|---|
| 1D | No public API candidate; all bridge types remain internal | Implemented / verified zero-change |

### Technical debt

| Debt ID | Area | Reason accepted | Risk | Repayment trigger | Suggested next task | State |
|---|---|---|---|---|---|---|
| VOX-1D-D01 | Semantic parts and colour | Certified GLB is one connected, geometry-only mesh | Cannot produce truthful detached turret/barrel or original palette colours | Separate GLBs/nodes, colour material, or reviewed masks become available | ASSET-VOX-1E Part/Palette Review | Open / intentional |

### Decision candidate

| Stage | Decision | Status | Needs human review |
|---|---|---|---|
| 1D | Keep mesh conversion internal and one-part; use explicit axis/resolution/palette; reject topology that cannot support deterministic solid fill | Accepted / implemented | Review completed |

## Current stop point

1D-1 through 1D-5 are complete. The certified P2 GLB produced a deterministic `29x64x31`, 20,261-cell Body candidate
with canonical hash `3FC301CC7B1336635EBD137E8312D85179A32E501CC60E1FB983E2DB4D986D90`. Parser/topology took
187 ms and the two voxelization passes took 99 ms / 81 ms in the final acceptance run. The initial edge dictionary
implementation exposed a 214-second topology regression; replacing it with deterministic sorted packed edges preserved
the exact output hashes and removed the regression.

Acceptance artifacts:

```text
artifacts/asset-vox-1d-acceptance/p2-body-64/acceptance-report.json
artifacts/asset-vox-1d-acceptance/p2-body-64/body-candidate.vox
artifacts/asset-vox-1d-acceptance/p2-body-64/body-slicestack.png
```

No external provider call, UI/project write, final VXL/HVA or GameReady claim occurred. The next safe entry is a separate
product-composition/background-preview audit or detached-part/palette review stage.
