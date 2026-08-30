# ASSET-VOX-1C Generation Provider Host Code Fact Audit

Date: 2026-08-26  
State: Completed / revised-contract input  
Risk: R4 because the proposed child-process protocol and workspace handoff are compatibility boundaries

## 1. Audit question

Determine the smallest reliable Generation Provider Host that can accept bounded image-to-3D work, isolate provider
failures, report progress/cancellation and return reviewable geometry candidates without treating intermediate artifacts
as final RA2 assets or introducing a premature general Job/Event/Artifact runtime.

## 2. Current code facts

### 2.1 Final-asset provider is intentionally not reusable as a generation host

`IRa2AutomationAssetProvider.Resolve` is synchronous and manifest-closing. A success must return exactly one final
artifact for every `Ra2AutomationAssetRequirement`; `VxlModel` and `HvaAnimation` require `.vxl` and `.hva` identities.
Its source and artifact contracts defensively copy bounded bytes and only certify identity, extension and SHA-256.

Consequences:

- GLB, VOX, PNG, provider reports and provenance cannot be returned through that success shape.
- A generation failure cannot expose partial candidates through the final-asset provider.
- `Ra2AutomationExistingAssetProvider` remains the later re-entry point after real final assets exist.
- Extending this interface with asynchronous progress, temporary paths or partial candidates would break its accepted
  manifest-closure semantics.

### 2.2 Application is a deliberately pure/headless algorithm boundary

`RA2IniEditor.Application` targets `net8.0`, references Core only, and its boundary tests reject production use of
`File.Read`, `File.Write`, `Directory`, `Environment` and `Process`. Stage 1B's voxel snapshot/codecs are internal and
UI-neutral. Adding process/file orchestration there would violate an explicit tested architecture boundary.

Consequences:

- Process execution and temporary workspace ownership must live outside Application.
- The 77 exported Application types and exact friend-assembly list should remain unchanged in 1C.
- Stage 1B data remains canonical for voxel conversion, but 1C should stop at bounded geometry candidates; Stage 1D owns
  mesh normalization and voxel conversion.

### 2.3 There is no production process-provider host to extend

Production process launches are limited to narrow Shell/UI actions such as opening external locations. Test projects use
process launch for UI automation. There is no reusable production process-tree lifetime manager, JSON-lines provider
protocol, workspace lease, artifact quarantine, provider catalog or mesh-generation adapter.

The existing DeepSeek client and `Ra2AiRequestLifecycle` are provider-specific text-AI/IDE request components. Reusing
them would mix WPF conversation authority with binary geometry execution and would not provide process-tree or artifact
containment.

### 2.4 A general Job/Event/Artifact runtime is explicitly deferred

The current roadmap keeps `AUTOMATION-1 Job/Event/Artifact Runtime` deferred. No persisted job registry, event catalog,
resume token, recovery database or cross-session artifact repository exists.

1C can safely own one transient generation run and a disposable workspace lease. It must not introduce a generic job
framework under a voxel-specific name.

### 2.5 Stage 1A/1B boundaries are usable without modification

- Stage 1A owns Body/Turret/Barrel assembly identity.
- Stage 1B owns immutable single-part voxel truth, VOX exchange, VXL readback and VXLSE SliceStack conversion.
- The supplied VXLSE structural acceptance passed with a `3x4x5` asymmetric five-cell fixture.

1C provider outputs are not yet canonical voxel truth. Stage 1D will validate/normalize mesh data and derive Stage 1B
snapshots. This avoids making a model-specific mesh schema authoritative.

## 3. Reuse scan

| Search area | Existing candidate | Decision |
|---|---|---|
| Final asset resolution | `IRa2AutomationAssetProvider` | Preserve; re-enter only with final VXL/HVA |
| Canonical voxel data | `Ra2VoxelSceneSnapshot`, VOX/PNG codecs | Reuse in 1D, not in provider execution |
| Cancellation | `CancellationToken`, existing AI request lifecycle | Reuse .NET token semantics; do not reuse UI lifecycle object |
| HTTP | DeepSeek shared `HttpClient` | Do not reuse; remote geometry transport is a separate future adapter |
| Process execution | Shell links/UI automation | Not a provider host; create isolated Host implementation |
| Hashing/limits | SHA-256 and existing bounded-contract style | Reuse conventions and defensive validation |
| General job runtime | None; roadmap explicitly defers it | Do not create in 1C |

Search terms included `AssetProvider`, `AssetManifest`, `ProcessStartInfo`, `HttpClient`, `CancellationToken`, `Job`,
`Artifact`, `Provenance`, `VoxelSceneSnapshot` and relevant tests/roadmap decisions.

## 4. Recommended assembly boundary

Create a separate `RA2IniEditor.AssetHost` `net8.0` assembly. It may use BCL process/file/JSON primitives, but it must
not reference WPF, IDE, Infrastructure, Field Registry or Shell. Initial 1C contracts remain internal and are exposed
only to their test assembly. No Application public API or friend list changes are required.

Expected dependency direction:

```text
RA2IniEditor.AssetHost              (process, workspace, protocol, provenance)
RA2IniEditor.AssetHost.Tests        -> AssetHost
RA2IniEditor.IDE                    (future 1E composition, not changed in 1C)
RA2IniEditor.Application            (1B/1D deterministic content, unchanged in 1C)
```

The first implementation proves the protocol with a deterministic managed fixture provider. It does not install
TRELLIS, Python, CUDA or model weights and does not perform a paid/network call.

## 5. Data ownership conclusion

| Concept | Owner | Lifetime | Serialized in 1C |
|---|---|---|---|
| Provider descriptor | AssetHost catalog | process lifetime | No persistent catalog |
| Generation request | caller + one run | request lifetime | Versioned request JSON inside run workspace |
| Progress | running Host invocation | transient | Bounded JSON-lines only |
| Candidate artifact | successful workspace lease | until explicit disposal | File-backed, hash-addressed within workspace |
| Provenance | immutable run result | result/lease lifetime | Versioned result manifest inside workspace |
| Failure evidence | immutable run result | result lifetime | No partial candidate payload |
| Job history/recovery | Not owned in 1C | Deferred | Not implemented |

## 6. Main gaps that contract must close

1. Exact provider descriptor/capability/license semantics.
2. Versioned request/progress/result protocol and duplicate/final-message rules.
3. Trusted executable configuration versus untrusted model input.
4. Workspace containment, reparse-point rejection, size/magic/hash checks and cleanup behavior.
5. Timeout, cancellation and entire process-tree termination.
6. Bounded stderr/stdout and diagnostic redaction.
7. Determinism claims: fixture replay is required; real providers may declare best-effort seed behavior only.
8. Explicit distinction between process fault isolation and an OS security sandbox.
9. No project writes, no final VXL claim and no general persistent Job Runtime.

## 6.1 Revised-contract closure review

The final-contract self-review found four omissions that are now closed without widening product scope:

1. A bounded `ProbeAsync` path distinguishes executable/protocol/model/capability/license readiness from generation.
   Run still repeats security-relevant checks, so probe does not become an authorization cache.
2. The internal surface is frozen to one Host with `ProbeAsync`/`RunAsync` and one `IAsyncDisposable` read-only workspace
   lease. No filesystem path, generic provider plugin or public Application API is introduced.
3. A marker- and lock-based TTL janitor can remove only expired orphan runs below the dedicated workspace root; it is not
   persisted job recovery and cannot infer/delete unknown directories.
4. Stdout/stderr are drained concurrently through bounded paths. Backpressure and cancel/timeout/terminal/exit races are
   explicit verification requirements rather than implementation details left to chance.

These amendments preserve the R4 classification because the protocol and lease remain compatibility boundaries. They
reduce implementation ambiguity and do not justify starting runtime work without explicit approval.

## 7. Risk conclusion

Initial triage was R3. The audit raises the implementation package to R4 because a versioned child-process protocol and
file-backed artifact handoff are compatibility boundaries. Implementation requires explicit approval of the final
contract. Documentation-only audit/contract work is safe to complete before that approval.
