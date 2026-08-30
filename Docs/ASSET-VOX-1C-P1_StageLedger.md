# ASSET-VOX-1C-P1 Stage Ledger

Date: 2026-08-26  
Package state: Stopped at mandatory external authorization gate

| Stage | Goal | Evidence | Self-review | State | Next entry |
|---|---|---|---|---|---|
| P1-0 | Environment/provider/license/reuse audit and final contract | Machine facts, official upstream sources, code fact audit, contract | Passed: no architecture duplication; R4 correctly retained | Completed | Explicit user authorization |
| P1-1 | Fixed bundle manifest and self-contained adapter shell | Model-free protocol/security tests | Not run | Blocked | License/install approval |
| P1-2 | Python runner and fake/offline integration | Success/failure/cancel/timeout/process-tree matrix | Not run | Blocked | P1-1 pass |
| P1-3 | Provision pinned Hunyuan3D-2mini shape-only bundle | Exact revisions/hashes/license/environment record | Not run | Blocked | Explicit external authorization |
| P1-4 | Real probe and generation certification | GLB/provenance/replay/cancel/timeout evidence | Not run | Blocked | P1-3 pass |
| P1-5 | Regression/package/docs closeout | Full tests, diff audit, clean package | Not run | Blocked | P1-4 pass |
| 1D | Mesh-to-canonical-voxel bridge | Separate R3 contract and implementation | Not started | Pending | P1 completed |

## P1-0 self-review

### Passed

- Reused the completed 1C Host and 1B canonical voxel authority; no parallel job, provider, workspace or voxel model was
  proposed.
- Selected a provider family that is credible for the observed Windows/16 GB machine and explicitly rejected a baseline
  whose official minimum is not met.
- Separated license acceptance, provisioning, real certification and later visual/voxel/game-quality claims.
- Froze process-tree, provenance, package and no-project-write gates before implementation.
- Preserved zero public API, persistence, UI, INI and editor-semantic effects.

### Not verified

- The Hunyuan provider has not been installed, imported, probed or run.
- The exact upstream commit, model revision and weight hashes are not yet frozen.
- GPU/CUDA/PyTorch compatibility, peak VRAM, latency and output quality are unknown until real certification.
- Same-seed repeatability is unknown and remains `BestEffort`.

### Mandatory blocker

The next stage requires the user to accept the Tencent Hunyuan 3D 2.0 Community License and authorize creation of an
isolated Python 3.11 environment plus download of source, dependencies and model weights. The current agent cannot accept
that license or infer authorization from the general request to continue.

## P1-0 verification evidence

- Three phase documents exist and are non-empty: 7,351 / 9,438 / 3,114 bytes at verification time.
- README, current phase and full context references resolve to the three phase documents.
- Scoped change audit contains documentation files only; no production, test, project, Shell or XAML file was added to
  the P1-0 change set.
- State scan confirms P1-0 is `Completed`, P1-1 through P1-5 are `Blocked`/`Not run`, and 1D is `Pending`.
- Build/test/package: NotRun because P1-0 is docs-only and no executable/project input changed. The accepted 1C automated
  baseline remains the latest executable evidence; it is not reused as evidence that a real provider works.

## Deferred governance queue

- Transitive Python/model bundle integrity is self-validated provenance, not signed supply-chain trust.
- OS sandbox/AppContainer/container enforcement remains deferred; only the fixed trusted adapter may execute.
- Texture/PBR generation, multi-view input, multiple candidates and provider comparison remain outside P1.
- 1D owns GLB parsing, normalization and deterministic voxelization; P1 must not duplicate it.
- VXL/HVA, normals, pivot/mount, detached assembly animation and game smoke testing remain later stages.
