# ASSET-VOX-1C-P1 Hunyuan3D-2mini Shape Provider Final Contract

Date: 2026-08-26  
State: Final / self-reviewed / blocked on explicit external authorization  
Risk: R4  
Prerequisite: completed `ASSET-VOX-1C` and this phase's environment/code-fact audit

## 1. Outcome

P1 certifies one real, local, shape-only reference-image-to-GLB provider against the existing
`ra2-voxel-generation/1` Host contract. The target is a pinned Hunyuan3D-2mini revision running in an isolated Python
3.11 bundle and launched through one self-contained single-file adapter executable.

Success means the current machine can probe, generate, cancel, time out and reproduce complete provenance through the
existing Host. It does not mean the mesh is visually approved, voxelized, VXL/HVA-compatible or game-ready.

## 2. Stage sequence and mandatory self-review

| Stage | Deliverable | Gate before proceeding |
|---|---|---|
| P1-0 | Environment, upstream/license and reuse audit; final contract | Docs self-review; explicit user authorization |
| P1-1 | Fixed provider bundle manifest, self-contained adapter shell and protocol conformance tests | No Host/public API change; no arbitrary command/path input |
| P1-2 | Python shape-only runner plus fake/offline runner integration | Probe/run/cancel/timeout/process-tree tests pass without model download |
| P1-3 | User-authorized isolated environment and pinned source/dependency/weight provisioning | Manifest/hash/license inventory complete; no project contamination |
| P1-4 | Real probe and bounded generation certification | GLB/JSON hashes, identity, provenance, cancellation, timeout and replay evidence pass |
| P1-5 | Full regression, clean package and documentation closeout | Diff audit, package exclusion and R4 final review pass |

A failed mandatory gate stops the sequence. It may not be downgraded to a warning to enter the next stage.

## 3. Frozen provider identity

The exact source commit, model artifact revision and weight hashes are recorded only after P1-3 obtains them. They must
then be copied into the provider deployment manifest and the trusted Host configuration. Floating branches, `latest`,
unpinned package versions and mutable model aliases are not certifiable.

The initial capability is exactly:

```text
Provider: Hunyuan3D-2mini family, exact revision pinned at provisioning
Mode: reference image -> shape-only mesh
Output: one GLB candidate plus provider JSON; preview PNG optional
References: exactly one for the first certification slice
Candidates: exactly one for the first certification slice
Seed behavior: BestEffort until repeated real runs prove otherwise
Texture/PBR: unsupported in P1
```

## 4. Provider bundle and executable boundary

The external bundle is outside the repository, build outputs and project roots. It contains:

- an isolated Python 3.11 environment;
- pinned provider source and Python dependencies;
- pinned model artifacts/weights;
- a read-only deployment manifest with file identities and hashes;
- the fixed Python runner used only by the adapter.

The repository contains only the adapter source/tests and sample manifest schema. The production adapter is published as
a self-contained single-file executable so the existing Host executable hash covers adapter code. It resolves only
bundle-relative fixed locations from trusted deployment configuration. Prompt, image metadata, provider output and
environment variables cannot replace the interpreter, entrypoint, model root or output root.

The adapter must never install, update, clone, download, accept a license or start a persistent server during `probe` or
`run`.

## 5. Protocol mapping

The adapter implements the existing JSON-lines protocol without additive Host behavior:

- `probe` validates the deployment manifest, required files, importability, CUDA device/VRAM and model readiness, then
  reports the configured provider/model/license descriptor.
- `run` loads the same pinned model, reads the Host-owned reference image and writes artifacts only under the supplied
  run workspace.
- progress remains monotonic and bounded; stdout is protocol-only and diagnostic logs use bounded stderr.
- one terminal event is emitted. Process exit, terminal event, cancellation and timeout are still arbitrated by AssetHost.
- provider JSON records adapter version/hash, bundle-manifest hash, provider source revision, model revision/hash set,
  Python/Torch/CUDA/GPU identities, request fingerprint, input hash, seed and effective generation settings.

Provider provenance must not contain absolute user paths, environment secrets, access tokens or image bytes.

## 6. Failure semantics

Expected failures map to existing `Ra2GenerationFailureKind`; P1 adds no new public or internal Host enum:

| Condition | Existing result |
|---|---|
| Bundle/model missing or import/GPU readiness failure | `ProviderNotReady` or provider-reported failure during run |
| Provider/model/revision/license mismatch | existing identity/license failure |
| Manifest/hash mismatch | probe/run fails closed before generation |
| Out of memory/model exception | sanitized provider-reported/process failure; no lease |
| Cancellation | `Canceled`; adapter and Python process tree terminated |
| Timeout | `TimedOut`; adapter and Python process tree terminated |
| Missing/invalid/oversized GLB or provenance | existing output/protocol/resource failure; no lease |
| Cleanup failure | existing quarantine/cleanup behavior |

Raw Python tracebacks, local absolute paths and provider internals do not cross the Host result seam. Full local logs, if
retained for certification, live only in the explicitly authorized external bundle/test evidence area and are not packed.

## 7. Verification matrix

### P1-1/P1-2 automated, model-free

- exact protocol identity/version and provider/model/license fields;
- deployment manifest traversal, duplicate/hash/size and symlink/reparse rejection;
- no arbitrary command, interpreter, entrypoint, model or output-root injection;
- fake runner success, malformed protocol, crash, no terminal, cancel and timeout;
- adapter plus Python child process-tree termination;
- artifact paths remain in Host workspace and no project files change;
- repository package excludes environment, source cache, weights and generated artifacts.

### P1-3 environment certification

- exact Python, Torch, CUDA runtime, GPU, provider commit, model revision and every declared weight hash recorded;
- license ID/URL and explicit acceptance fact recorded; no acceptance is inferred from configuration defaults;
- clean probe works without any network access or download after provisioning;
- environment and model bundle are outside the repository and project roots.

### P1-4 real runs

- one supplied non-sensitive reference image produces one non-empty Host-validated GLB and provider JSON;
- a second same-input/same-seed run records whether output hashes match, but mismatch is not failure while seed behavior
  remains `BestEffort`;
- changed image or seed changes request fingerprint and is represented in provenance;
- cancel during model load/inference and a forced timeout both terminate the complete process tree and leave no successful
  lease;
- replay evidence binds executable, bundle manifest, source/model revision, input, request and artifacts;
- quality remains `CandidateOnly`; no visual, voxel, VXL/HVA or game-readiness certification is inferred.

### P1-5 repository gates

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

The final diff audit must confirm zero Shell/XAML, INI/parser, Field Registry, diagnostics, completion, project mutation,
Apply/Save, Stage 1B authority or exported public API changes.

## 8. Stop rules

Stop P1 and do not enter 1D when any of these occurs:

- license acceptance or installation/download permission is absent or ambiguous;
- pinned source/model artifacts cannot be obtained or their hashes cannot be frozen;
- the real provider needs an unapproved driver/system CUDA replacement, service, account, API key or paid call;
- probe/run requires arbitrary command execution or writes outside the external bundle and Host workspace;
- cancellation/timeout cannot terminate the process tree;
- output cannot be bound to complete provenance;
- model-free or repository gates fail for a touched boundary.

## 9. API, persistence, UI and packaging effects

- Public API: none.
- Existing Host protocol/API: unchanged.
- Persistence/project format: none.
- UI/AutomationIds: none.
- INI/Field Registry/editor semantics: none.
- Clean source package: adapter source/tests/sample schema only; no provider environment, upstream checkout, cache, weights,
  license acceptance record tied to a person, generated mesh or transient workspace.

## 10. Approval boundary

General permission to continue the ASSET-VOX plan is not permission to accept the Tencent license or install/download a
large third-party model stack. P1-1 through P1-5 may begin only after the user explicitly confirms both.

