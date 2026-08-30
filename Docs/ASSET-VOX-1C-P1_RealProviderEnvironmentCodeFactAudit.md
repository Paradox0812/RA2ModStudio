# ASSET-VOX-1C-P1 Real Provider Environment Code-Fact Audit

Date: 2026-08-26  
State: Completed / docs-only / implementation not authorized  
Risk: R4

## 1. Audit conclusion

The existing `RA2IniEditor.AssetHost` already owns the provider-neutral process, timeout, cancellation, workspace,
artifact and provenance boundary required by a real image-to-3D provider. Stage P1 must add one fixed provider bundle
that speaks the frozen `ra2-voxel-generation/1` protocol; it must not add another Host, job system, public plugin API,
project writer or voxel DTO.

The recommended local baseline is **Hunyuan3D-2mini shape-only**. It is the smallest credible match for the current
Windows workstation and for the next deterministic mesh-to-voxel stage. TRELLIS.2 remains a later Linux/24 GB+ option,
not the current certified provider.

This conclusion is an environment and architecture decision only. No third-party license was accepted, no dependency or
weight was downloaded, and no real generation was run.

## 2. Current machine facts

| Item | Observed fact | Consequence |
|---|---|---|
| GPU | NVIDIA GeForce RTX 4080 SUPER, 16,376 MiB VRAM | Credible for Hunyuan shape-only/mini; insufficient for the current official TRELLIS.2 minimum |
| Driver | NVIDIA 596.21 | Driver is modern, but provider compatibility still requires a real probe |
| CUDA toolkit | 11.8 installed | Must not be treated as proof of PyTorch/provider compatibility; use an isolated pinned runtime |
| Python | 3.11.9 and 3.14 installed | Provider must pin Python 3.11; 3.14 is not the certification target |
| Python ML packages | `torch`, `trimesh`, `PIL`, `huggingface_hub`, `diffusers` absent in Python 3.11 | Environment is not ready and cannot pass a real probe |
| Environment tools | no conda or uv; git and git-lfs present; cmake/ninja absent | Provisioning must be explicit and isolated; native build requirements remain a gate |
| Free space | C: about 388 GB; H: about 99.3 GB | Capacity is plausible, but model/dependency downloads remain user-authorized external changes |
| Existing model cache | no relevant Hugging Face/model cache found in the inspected standard locations | Real certification requires downloads or a user-supplied offline bundle |

## 3. Official upstream facts used for selection

- TRELLIS.2's current official repository describes a Linux-tested environment, NVIDIA GPU with at least 24 GB VRAM,
  CUDA 12.4 recommendation and conda-based setup. This machine does not meet that local baseline.
- Hunyuan3D-2's official repository advertises Windows support, approximately 6 GB VRAM for shape-only and 16 GB for
  shape plus texture, and provides a smaller Hunyuan3D-2mini family. The current machine is therefore a plausible
  shape-only target, subject to a real install/probe/generation test.
- Hunyuan3D-2 is governed by the Tencent Hunyuan 3D 2.0 Community License. Using the software is license acceptance;
  distribution and territory terms exist. The project cannot accept that license on the user's behalf.

Authoritative sources:

- <https://github.com/microsoft/TRELLIS.2>
- <https://github.com/Tencent-Hunyuan/Hunyuan3D-2>
- <https://github.com/Tencent-Hunyuan/Hunyuan3D-2/blob/main/LICENSE>

## 4. Reuse and ownership map

| Concern | Existing authority | P1 action |
|---|---|---|
| Probe/run lifecycle | `IRa2VoxelGenerationHost` | Reuse unchanged |
| Process tree, cancellation and timeout | `Ra2ProviderProcessRunner` | Reuse; prove with the real provider |
| Workspace/artifact lease | `IRa2GenerationWorkspaceLease` | Reuse unchanged |
| GLB/PNG/JSON validation | AssetHost artifact validation | Reuse; do not call GLB a valid voxel scene yet |
| Canonical voxel truth | Application-internal `Ra2VoxelSceneSnapshot` | Do not access in P1; 1D owns the bridge |
| Provider implementation | Missing | Add one fixed, separately deployable adapter bundle after authorization |
| Model weights/runtime | Missing and externally licensed | User-owned external bundle; never package in source |
| Project/INI/asset writes | Existing editor transaction boundaries | Forbidden in P1 |

Data lifetime:

1. The provider bundle owns its isolated runtime, pinned code and weights outside the project/workspace.
2. AssetHost owns each transient run directory and the successful read-only lease.
3. The caller supplies reference-image bytes and consumes verified artifact streams while the lease is alive.
4. Stage 1D, not P1, will parse caller-owned GLB bytes into a new immutable canonical voxel snapshot.

## 5. Architecture findings that prevent hidden rework

### 5.1 Fixed adapter executable

The Host trusts one configured executable hash. A framework-dependent `.exe` that loads mutable adjacent application DLLs
would make that hash incomplete. Production certification therefore requires a self-contained, single-file .NET adapter
launcher. It may start only its fixed bundle-relative Python entrypoint; user prompt or provider output cannot choose a
command, interpreter, working root or arbitrary arguments.

### 5.2 No persistent local HTTP server

A separately started HTTP service would outlive the Host process tree and weaken the frozen cancel/timeout semantics.
P1 therefore uses one adapter process per probe/run and communicates through the existing JSON-lines protocol. A remote
or persistent service remains a distinct future contract.

### 5.3 Transitive bundle provenance

Host verification of the adapter executable does not by itself authenticate Python packages, scripts or weights. The
adapter must validate a bounded bundle manifest and emit its hash, Python/Torch/CUDA versions, model artifact revision,
provider source revision, weight hashes and seed behavior in provider provenance. This is compatibility evidence, not an
OS sandbox or a supply-chain signature.

### 5.4 Shape-only first

P1 certifies reference-image-to-geometry only. Texture generation is deferred because it consumes the full 16 GB class
budget and its PBR output is not the final RA2 palette representation. Palette quantization, voxelization, normals and
VXL/HVA are separate deterministic stages.

## 6. Allowed and forbidden scope

Allowed after explicit license/install authorization:

- one new provider-adapter project and its focused tests;
- one external provider bundle root outside `RA2IniEditor_IDE`;
- pinned Python 3.11 environment, provider source/dependencies and Hunyuan3D-2mini shape-only weights;
- phase docs, solution wiring and clean-source packaging rules that exclude runtime/weights.

Forbidden:

- Shell/XAML/UI/AutomationId changes;
- INI, Field Registry, diagnostics, completion, Preview/Apply/Undo/Save changes;
- public API or changes to the existing 1C protocol/Host seam;
- automatic product-time installation/download/license acceptance;
- arbitrary executable/provider discovery, API keys, paid calls or network service;
- project-directory output, direct VXL/HVA, normals, pivot/mount, animation or `GameReady` claims.

## 7. Risk and gate result

P1 remains R4 because it crosses an external license, large third-party runtime, model weights, GPU compatibility and
child-process protocol boundary. The docs-only audit passes. Implementation is blocked until the user explicitly accepts
the applicable license and authorizes dependency/source/weight installation plus real local generation tests.

