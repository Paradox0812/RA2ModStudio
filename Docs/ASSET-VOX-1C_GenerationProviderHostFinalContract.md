# ASSET-VOX-1C Generation Provider Host Final Contract

Date: 2026-08-26  
State: Implemented / automated verified  
Risk: R4  
Governance: Approved 2026-08-26 / 1C-1 through 1C-5 completed

## 1. Outcome

Stage 1C provides a provider-neutral, non-WPF Host for one bounded image-to-3D generation run. It launches an allowlisted
local provider process, supplies a versioned request workspace, observes bounded progress, supports timeout/cancellation,
and returns hash-verified geometry candidates plus provenance through a disposable workspace lease.

Stage 1C does not generate trusted VXL/HVA, does not write a mod project, does not install a real model and does not
claim that fixed seeds make every third-party model deterministic.

## 2. Deliverable split

| Slice | Deliverable | Required state |
|---|---|---|
| 1C-0 | Code fact audit, revised final contract, decision and ledger | Completed before implementation |
| 1C-1 | Headless AssetHost assembly, frozen internal API and readiness probe | Completed / verified |
| 1C-2 | Run workspace, lease, orphan janitor, artifact validation and provenance | Completed / verified |
| 1C-3 | Versioned local-process protocol, concurrent stream pumps and lifecycle arbitration | Completed / verified |
| 1C-4 | Deterministic managed fixture provider and probe/replay/failure matrix | Completed / verified |
| 1C-5 | Regression, package and documentation closeout | Completed / verified |
| 1C-P1 | Real TRELLIS/Hunyuan/provider adapter and environment setup | Deferred; separate authorization |

## 3. Allowed implementation files

- New `RA2IniEditor.AssetHost/` production project and files.
- New `RA2IniEditor.AssetHost.Tests/` tests and deterministic fixture provider.
- `RA2IniEditor.IDE.sln` only to add those two projects.
- Phase-specific docs, `Docs/DecisionLog.md`, `Docs/PublicApiLedger.md`, `Docs/DevelopmentRoadmap.md`,
  `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`, `Docs/Codex_CurrentPhase.md` and `Docs/README.md`.

No other file is implicitly allowed.

## 4. Forbidden implementation scope

- `ShellWindow.xaml`, `ShellWindow.xaml.cs`, all XAML, Dock layout and AutomationIds.
- Application public API, its exact 77-type allowlist or exact friend-assembly list.
- Stage 1A/1B voxel/assembly contracts and codecs except a separately approved defect fix.
- Existing final-asset Provider/Manifest semantics.
- INI parser, Field Registry, diagnostics, completion, Preview, Apply, Undo, Save or project session behavior.
- Project-directory writes, overwrite/delete behavior or automatic asset commit.
- Persistent job registry, event catalog, resume/recovery store, provider plugin discovery or arbitrary executable paths.
- HTTP/remote API adapter, secrets, paid calls, downloads, Python/CUDA installation or model weights.
- VXL/HVA writer, normals, pivot/mount inference, HVA animation or game readiness.

## 5. Assembly and authority boundary

`RA2IniEditor.AssetHost` targets `net8.0` and uses only BCL APIs. It must not reference WPF, IDE, Infrastructure,
Field Registry or Shell. Its types remain internal in 1C; only `RA2IniEditor.AssetHost.Tests` is a friend assembly.

The Host owns provider execution and its temporary workspace. It never owns project membership, Apply, Save, final asset
identity or canonical voxel truth. A future IDE/independent Host composition stage may consume it only through a separately
approved façade.

Process isolation in 1C means crash/timeout/resource and process-tree isolation. It is not an OS security sandbox. The
Host verifies and refuses outputs outside its workspace, but it cannot promise that an arbitrary executable could not
read or write elsewhere. Only trusted, explicitly configured executables may run. Untrusted-provider containment requires
a later AppContainer/container/sandbox stage.

## 5.1 Frozen internal Host surface

The 1C implementation must expose exactly one internal execution seam. It must not add a generic dispatcher, provider
plugin interface, public API, path-returning convenience API or synchronous wrapper:

```csharp
internal interface IRa2VoxelGenerationHost
{
    ValueTask<Ra2GenerationProbeResult> ProbeAsync(
        Ra2GenerationProviderConfiguration configuration,
        CancellationToken cancellationToken = default);

    ValueTask<Ra2GenerationRunResult> RunAsync(
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest request,
        IProgress<Ra2GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

internal interface IRa2GenerationWorkspaceLease : IAsyncDisposable
{
    IReadOnlyList<Ra2GenerationCandidate> Candidates { get; }

    ValueTask<Stream> OpenArtifactReadAsync(
        string candidateId,
        string artifactId,
        CancellationToken cancellationToken = default);
}
```

Contract rules:

- `Ra2GenerationProviderConfiguration` is immutable trusted Host input. It owns the executable path, expected executable
  hash, expected provider/model identity, accepted-license fact, workspace root, forbidden roots and resource limits.
  Neither request text nor provider output can construct or override it.
- `ProbeAsync` and `RunAsync` return immutable typed results for every expected operational failure, including cancellation;
  raw process/file/JSON exceptions do not cross the seam. Programmer-contract violations may still throw.
- A successful `Ra2GenerationRunResult` contains exactly one non-null workspace lease. A failure contains no lease and no
  candidates. Candidate collections are ordered, immutable and defensively owned.
- The lease exposes verified artifacts as read-only streams, never absolute or relative filesystem paths. A stream cannot
  escape the leased run root and is invalid after lease disposal.
- `DisposeAsync` is idempotent and closes all artifact streams before cleanup. A post-success cleanup failure quarantines
  the run and surfaces only a typed, sanitized workspace-cleanup exception; it never deletes outside the dedicated root.
- No second Host interface or direct provider-process helper may be consumed outside AssetHost tests in 1C.

## 5.2 Readiness probe

`ProbeAsync` is a side-effect-bounded compatibility/readiness check. It must:

1. validate the trusted configuration, executable existence and exact SHA-256;
2. launch the executable in protocol `probe` mode without a generation request or candidate workspace;
3. verify protocol/provider/model identity, declared capabilities, accepted-license requirement and model readiness;
4. finish within a trusted 5..60 second timeout, default 30 seconds;
5. return observed descriptor/hash/readiness and a typed safe failure without downloading models, accepting a license,
   writing the project or producing artifacts.

A successful probe is diagnostic evidence, not authorization and not a reusable security token. `RunAsync` always repeats
the executable hash, identity, capability and license checks immediately before generation so a stale probe cannot create
a time-of-check/time-of-use bypass. Probe results are not persisted or cached by 1C.

## 6. Provider descriptor

The immutable descriptor contains:

- `ProviderId`: 1..128 stable ASCII identifier.
- `ProtocolVersion`: exactly `1` for this stage.
- `ProviderVersion`, `ModelId`, `ModelRevision`: bounded non-secret identity text.
- `ExecutableSha256`: required uppercase SHA-256 for the configured executable.
- `Capabilities`: explicit supported input/output families.
- `SeedBehavior`: `Unsupported`, `BestEffort`, or `DeterministicDeclared`.
- `MaximumReferenceCount`, `MaximumCandidateCount`, `MaximumInputBytes`, `MaximumOutputBytes`.
- `LicenseId`, `LicenseUrl`, `Redistributable`, `RequiresUserAcceptance`.

Descriptor identity is configured by trusted Host code/configuration. Model output cannot choose the executable, protocol,
capabilities, limits or license state.

Supported 1C capability is `ReferenceImageToMesh`. `TextToReferenceImage`, `TextToMesh`, mesh-to-voxel and VXL generation
are not silently inferred. Reference-image generation remains a separate provider family.

## 7. Generation request

One immutable request contains:

- non-empty `RunId` GUID;
- bounded design prompt and negative constraints;
- 1..4 reference images supplied as caller-owned bytes, not source paths;
- reference name, media kind, length and computed SHA-256;
- explicit positive integer seed;
- candidate count `1..4` bounded by the descriptor;
- requested output `MeshGlb` and optional `PreviewPng`;
- expected provider identity/revision and request timeout;
- a versioned canonical request fingerprint.

Limits:

- Prompt: 16 KiB UTF-8; negative constraints: 8 KiB.
- Each reference: 32 MiB; aggregate input: 64 MiB.
- Each artifact: 256 MiB; aggregate successful output: 512 MiB.
- Timeout: 10 seconds to 30 minutes; trusted caller selects within this range.
- Maximum concurrent runs per Host instance: 1 in 1C.

The request contains no project path, command line, environment block, API key, shell fragment, output path or executable
identity supplied by the model.

## 8. Workspace and artifact ownership

The caller supplies one trusted workspace root plus an optional bounded list of forbidden roots; the workspace must not
equal or sit inside any project/forbidden root. The Host creates an unpredictable, unique run directory below the
workspace root and stages all inputs itself. The configured provider executable must also live outside the workspace.

Required layout:

```text
<workspace-root>/.ra2-asset-host-root
<workspace-root>/<run-id>/
  .ra2-run.json
  .active.lock
  staging/
    request.json
    inputs/<sha256>.<ext>
    provider-output/
  completed/
    result.json
    artifacts/<sha256>.<ext>
```

Rules:

- Every resolved path must remain under the run root after normalization.
- Existing paths, `..`, rooted response paths, alternate data streams and reparse points/symlinks are rejected.
- Provider outputs are admitted only from `staging/provider-output` and copied/moved by the Host after validation.
- Allowed output formats in 1C are GLB (`glTF` magic), PNG (standard signature) and bounded provider JSON reports.
- Executables, scripts, DLLs, archives and unknown formats are rejected.
- Host recomputes length and SHA-256; provider-declared hashes are evidence to compare, not authority.
- Success is promoted atomically to `completed`. Failure/cancel returns zero candidates and attempts to delete staging.
- Cleanup failure yields a typed `CleanupFailed` diagnostic and quarantines the run; it never turns the run into success.
- Successful result owns a disposable workspace lease. Disposing the lease removes the run unless a later, separately
  authorized artifact repository explicitly adopts it.

The workspace root is dedicated to AssetHost and must carry the exact root marker before any cleanup is allowed. The Host
holds an exclusive `.active.lock` handle from run-directory creation through workspace-lease disposal. Before each
`RunAsync`, an internal janitor may inspect direct child directories only and may delete a directory only when all of the
following are true:

- the root marker is valid, the direct child name is a canonical run GUID and `.ra2-run.json` matches that GUID/protocol;
- no path in the candidate tree is a reparse point or alternate stream and every normalized path remains below the root;
- the exclusive active lock can be acquired, proving that no live process or workspace lease owns the run;
- last Host activity is older than the trusted TTL: default 24 hours, configurable only within 1 hour..30 days.

The Host may create a missing root and marker, or add the marker to an existing empty directory. An existing non-empty root
without the exact marker is `WorkspaceRejected`; it is never adopted. Unknown folders, malformed run markers, active locks
and young runs are skipped for deletion, never repaired or recursively guessed; their presence makes a new `RunAsync`
reject the contaminated root rather than executing beside unowned data.

Janitor failure is bounded diagnostic evidence; it cannot make `ProbeAsync` fail. `RunAsync` performs the sweep before
creating a run and then enforces a trusted aggregate workspace-root budget (default 4 GiB, configurable 512 MiB..64 GiB).
If the budget remains exceeded, generation fails `ResourceLimitExceeded` without starting the provider. A per-run size
watchdog terminates a provider that crosses the 512 MiB run limit; this limits damage but is not an OS disk quota.

No path in this workspace is a mod-project asset and no automatic copy/commit exists in 1C.

## 9. Process protocol v1

Protocol identity is `ra2-voxel-generation/1`.

The Host launches the configured executable directly with `UseShellExecute=false`, redirected standard streams, a trusted
working directory and a minimal environment. No shell, command interpreter or model-generated argument is used.

Fixed launch arguments identify the protocol plus `probe` or `generate` operation. Generate mode may receive the trusted
run directory; probe mode receives no candidate/output directory. `request.json` contains the versioned generation request;
input and output paths inside JSON are relative to the run directory.

Standard output is UTF-8 JSON Lines. Each line is exactly one of:

- `started`: operation, provider identity and, for generate, request fingerprint acknowledgement;
- `probe_completed`: exactly one terminal probe success with descriptor, capability, license and model-readiness facts;
- `progress`: monotonic sequence, phase, optional 0..100 percent and bounded safe message;
- `candidate`: candidate identity and relative artifact declarations;
- `completed`: exactly one terminal success declaration;
- `failed`: exactly one terminal typed provider failure.

Protocol limits:

- Maximum line: 1 MiB; maximum lines: 4096.
- Maximum progress events accepted: 1024; delivery to consumer is coalesced to at most 10 per second.
- Standard error capture: last 64 KiB, sanitized and never treated as protocol.
- Exactly one `started`; exactly one operation-appropriate terminal line; nothing after terminal. Probe mode rejects
  `progress`, `candidate` and `completed`; generate mode rejects `probe_completed` and requires unique candidate IDs.
- Duplicate JSON root properties, unknown terminal kinds, invalid UTF-8, malformed JSON and mismatched request/provider
  identity are protocol failures.
- Unknown additive non-authoritative fields are ignored within depth/size limits; executable artifact declarations remain
  strictly validated.

The provider process receives no project credentials or project path. Secrets are out of scope because remote/API
transport is out of scope.

### 9.1 Concurrent stream and exit discipline

- The Host starts independent stdout and stderr drain tasks immediately after process start and before awaiting exit.
  It must never read one redirected pipe to completion before draining the other.
- Stdout parsing, stderr ring-buffer capture and progress delivery use separate bounded paths. A slow/throwing progress
  observer cannot block either process pipe; intermediate progress may be coalesced, but terminal protocol evidence cannot.
- Exceeding line/event/stderr bounds atomically latches a protocol/resource failure, terminates the process tree and keeps
  draining both pipes to bounded EOF/grace completion. Output is never accumulated without a fixed cap.
- A terminal line is provisional until both redirected streams reach EOF and the process exits within the grace period.
  Post-terminal output, non-zero success exit and terminal/exit disagreement remain failures.
- The success commit point is atomically latched only after terminal agreement, zero exit, artifact validation and atomic
  promotion. Cancellation observed before that point wins over timeout; timeout wins over later protocol/process evidence.
  Cancellation after the commit point does not revoke a returned lease.

## 10. Run state and progress

Transient states are:

```text
Created -> Starting -> Running -> Validating -> CandidateReady
                         |            |
                         +-> Failed <-+
                         +-> Canceled
                         +-> TimedOut
```

Probe uses a separate non-generation path:

```text
Created -> Probing -> Ready
               +-> Failed / Canceled / TimedOut
```

States are monotonic and request-local. There is no pause, resume, retry, fallback, persisted event log or cross-session
recovery. Progress is presentation evidence only and cannot promote state or admit an artifact.

The immutable terminal result contains:

- state and typed failure kind;
- safe diagnostic message;
- provider/request identities and timestamps/duration;
- bounded progress summary;
- on success only, one workspace lease that owns the ordered candidate descriptors;
- on failure only, zero candidates and cleanup/quarantine evidence.

## 11. Failure taxonomy

Required failure kinds:

- `InvalidRequest`
- `ProviderNotConfigured`
- `ProviderNotReady`
- `ProviderIdentityMismatch`
- `ExecutableHashMismatch`
- `CapabilityUnsupported`
- `LicenseNotAccepted`
- `WorkspaceRejected`
- `ProcessStartFailed`
- `ProtocolViolation`
- `ProviderReportedFailure`
- `OutputMissing`
- `OutputRejected`
- `ResourceLimitExceeded`
- `TimedOut`
- `Canceled`
- `TerminationFailed`
- `ProcessCrashed`
- `ReplayMismatch`
- `CleanupFailed`
- `UnexpectedFailure`

No exception text, absolute path, environment variable, command line, raw stderr or secret is returned as the safe user
message. Full local diagnostics remain bounded and separate from model-visible content.

## 12. Cancellation, timeout and crash

- User cancellation and timeout are distinct terminal states.
- On either, the Host atomically latches the terminal request, terminates the entire child process tree, keeps both stream
  pumps draining through EOF or the bounded grace period, and validates that the process is gone before cleanup. It must
  not cancel a pipe reader first and leave the child blocked on a full redirected buffer.
- Exit without a valid terminal protocol message is `ProcessCrashed`, even when exit code is zero.
- A valid `completed` message followed by non-zero exit is failure; candidate promotion occurs only after terminal, exit
  and artifact validation all agree.
- There is no automatic retry or provider/model fallback in 1C.
- Cancellation after candidate declaration but before atomic promotion returns zero candidates.

## 13. Determinism and replay language

The Host guarantees deterministic request fingerprints, stable candidate ordering, hash verification and deterministic
fixture behavior. It does not guarantee third-party model determinism.

For `DeterministicDeclared`, repeating provider/revision/request/seed may be compared and drift reported. A mismatch is
`ReplayMismatch` evidence, not silently accepted equivalence. For `BestEffort`, the seed is provenance only. 1C has no
persistent cache and therefore cannot promise cross-session replay without rerunning the provider.

The managed fixture provider must return identical artifacts for the same request/seed and distinct fingerprints for
mutated input/seed. Real provider replay certification belongs to 1C-P1.

## 14. Provenance

`result.json` records only bounded, non-secret facts:

- protocol/provider/model/revision/executable hash;
- request fingerprint, seed and input hashes;
- start/end UTC timestamps and duration;
- candidate IDs, relative artifact names, lengths and SHA-256;
- declared seed behavior, license ID and accepted-license fact;
- terminal state and sanitized failure kind.

Raw prompts may remain in the private request workspace for the active lease but are not copied into UI diagnostics,
logs or future project assets by 1C.

## 15. Verification matrix

| Area | Required evidence |
|---|---|
| Contracts | bounds, immutability, exact enums/states, defensive copies, canonical fingerprint |
| Probe | ready, model missing, identity/hash/capability/license mismatch, timeout/cancel, no artifact/workspace side effects |
| Workspace | containment, collisions, reparse/path traversal/ADS rejection, atomic promotion, lease disposal, cleanup/quarantine |
| Janitor | root/run markers, active-lock skip, young/unknown skip, expired orphan cleanup, root-budget rejection |
| Protocol | probe/generate separation, ordered valid flow, additive metadata, duplicate/malformed/oversized/post-terminal rejection |
| Process | real managed child process, exit agreement, concurrent bounded stdout/stderr, entire-tree cancellation |
| Race/backpressure | stderr beyond pipe buffer, progress flood/slow observer, cancel-vs-timeout, terminal-vs-exit, candidate-vs-cancel |
| Resource | input/output/candidate/progress/line/aggregate limits |
| Failure | every failure kind returns zero candidate leases except successful terminal result |
| Replay | deterministic fixture same seed equal; changed seed/input different; provider drift evidence |
| Regression | AssetHost tests, Application tests, IDE-only build/test, clean source package |

No test may invoke a paid service, download a model, require Python/CUDA, write the user's project or rely on VXLSE GUI.

## 16. Acceptance criteria

1. New Host and tests are headless and build through the IDE-only solution.
2. Application exported allowlist remains exactly 77 and Application production-source forbidden-token test remains green.
3. A real managed fixture child process passes readiness probe and completes a deterministic candidate run.
4. Timeout, cancellation, crash, malformed protocol, output escape, aggregate overflow and orphan/root-budget cases return typed zero-candidate
   failures and do not leave an adoptable artifact.
5. Successful output is a verified GLB/optional PNG candidate plus provenance, never `VxlModel`, `HvaAnimation` or
   `GameReady`.
6. No project, Shell, XAML, INI or final Asset Provider behavior changes.
7. Concurrent stdout/stderr backpressure and cancel/timeout/exit races complete without deadlock and select the frozen
   terminal-state precedence.
8. Full required regression and clean-source packaging complete or any failure is reported without success wording.

## 17. Stop rules

Stop implementation if any of the following becomes necessary:

- changing Application public types/friends or moving Stage 1B authority;
- adding persistence, generic job/event registry, plugin discovery or cross-session resume;
- executing an untrusted/non-allowlisted path or claiming OS sandboxing;
- adding HTTP/API keys, downloading/accepting licenses, installing dependencies or making a paid call;
- writing a project, modifying INI, generating final VXL/HVA or adding UI;
- weakening path, artifact, process or zero-partial-result tests.

## 18. Approval statement

Implementation may begin only after the user explicitly says:

```text
批准修订版 ASSET-VOX-1C 最终契约，连续执行 1C-1 → 1C-5
```

Approval was received on 2026-08-26. The implementation stayed within the approved file boundary. Automated closeout
passed with AssetHost `38/38`, Application `228/228`, IDE `2779/2779`, an IDE-only build with zero warnings/errors and
an IdeOnly clean source package containing 1295 files. Real provider, visual quality, VXL/HVA and project/UI integration
remain explicitly deferred.
