# ASSET-VOX-3A Generation Orchestration — Final Contract

Date: 2026-08-28  
State: self-reviewed / awaiting explicit user approval  
Risk: R4 network/cost/artifact boundary + R3 lifecycle/UI composition + R2 exported façade  
Implementation: not started

## 1. Product outcome

The existing Voxel Style workspace gains one explicit **Generate Model Preview** path. With an active project, one
explicitly selected reference image, an optional design brief and a valid RA2 palette source, the user can start exactly one bounded Tencent Hunyuan
3D job, observe/cancel it, convert the verified GLB to the existing canonical voxel model, and inspect the resulting
session-only 3D/quality candidate.

The successful 3A flow is:

```text
reference image + design brief + explicit consent
  -> bundled-provider readiness probe
  -> one AssetHost generation run
  -> one verified GLB artifact copied while the lease is alive
  -> lease disposal/workspace cleanup
  -> existing restricted GLB reader and deterministic voxelizer
  -> existing optional local quality refinement
  -> generated in-memory source in the existing Voxel Style workspace
```

No output is written to the project. A 3A candidate is not VOX/VXL/HVA on disk and is not GameReady.

## 2. Capability truth and naming

The currently certified provider exposes only `ReferenceImageToMesh` and accepts exactly one image. Its professional
remote request does not send the design prompt with the image. Therefore:

- the reference image drives current-provider geometry;
- the natural-language design brief is bounded request provenance and future-provider intent;
- the UI must state that the current Tencent profile does not use the brief as a geometry input;
- text-only generation is a typed `CapabilityUnavailable` state, not an implicit fallback;
- 3A must not be described as pure natural-language-to-VOX.

A later provider with an audited `TextToMesh` capability, or a separately approved text-to-reference-image stage, may
consume the same product entry through a new contract. 3A does not add either capability.

## 3. Scope and allowed files

Implementation after approval may modify only:

```text
RA2IniEditor.AssetHost/
RA2IniEditor.AssetHost.Tests/
RA2IniEditor.IDE/RA2IniEditor.IDE.csproj
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelStylePreviewCoordinator.cs
RA2IniEditor.IDE/AssetAuthoring/                 new bounded generation adapter/session files
RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs
RA2IniEditor.Tests/IDE/                          focused coordinator/ViewModel/UI contract tests
Docs/ASSET-VOX-3A_*.md
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/CurrentCapabilities.md
Docs/DevelopmentRoadmap.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/DeveloperNotes.md                           only for provider setup/build facts
```

`RA2IniEditor.AssetProviders.TencentHy3D` production behavior and protocol are frozen. Its project file may be changed
only if automated bundle copying cannot be implemented from the IDE project without changing provider runtime behavior;
such a need must be reported before editing it.

No new solution project is authorized by 3A.

## 4. Forbidden scope

- `ShellWindow.xaml`, `ShellWindow.xaml.cs`, menus, docking, default layout and status bar.
- Tencent submit/query/download schema, API origin, secret variables, free-only gate or automatic retry.
- AssetHost protocol v1, internal workspace/lease semantics, process arbitration or failure meanings.
- GLB parser/voxelizer algorithms, 2A refinement algorithms or 2C structure-recognition semantics.
- INI, Field Registry, Completion, Hover, Diagnostics, Work mode, Apply/Undo/Redo/Save.
- project asset writes, overwrite, export, auto-save, VOX/VXL/HVA materialization or manifest closure.
- automatic DeepSeek style compilation or structure recognition after generation.
- pure text-to-mesh, text-to-image, semantic part recognition improvement, multi-part splitting or game validation.
- provider/plugin discovery, arbitrary executable picker, persistent job history, resume/recovery or cross-session cache.
- any real Tencent/DeepSeek call during implementation verification unless separately approved.

## 5. Architecture and authority

### 5.1 Dependency direction

```text
RA2IniEditor.IDE
  -> RA2IniEditor.AssetHost public experimental façade
  -> existing internal IRa2VoxelGenerationHost
  -> bundled Tencent child process

RA2IniEditor.IDE
  -> existing Application-internal GLB/voxel/refinement truth
```

AssetHost must not reference IDE, WPF, Infrastructure, project sessions or Application. Application must not acquire
process, filesystem, environment, HTTP or provider responsibilities.

### 5.2 Narrow AssetHost façade

3A adds one exported façade family in `RA2IniEditor.AssetHost`, marked `Experimental` in the public ledger:

```csharp
public sealed class Ra2MeshGenerationFacade
{
    public static Ra2MeshGenerationFacade CreateFromBundle(
        string bundleManifestPath,
        string workspaceRoot,
        IReadOnlyList<string> forbiddenRoots,
        bool licenseAccepted);

    public ValueTask<Ra2MeshGenerationResult> ProbeAsync(
        CancellationToken cancellationToken = default);

    public ValueTask<Ra2MeshGenerationResult> GenerateAsync(
        Ra2MeshGenerationRequest request,
        IProgress<Ra2MeshGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
```

The supporting exported contract family is limited to:

- `Ra2MeshGenerationRequest`;
- `Ra2MeshGenerationResult`;
- `Ra2MeshGenerationProgress`;
- `Ra2MeshGenerationFailureKind`;
- `Ra2ReferenceImageFormat`.

No internal Host DTO, protocol writer/parser, provider configuration, candidate, workspace path or lease becomes public.
The façade consumes a successful internal lease, copies exactly one verified GLB no larger than 16 MiB into defensively
owned memory, optionally copies one bounded PNG preview, disposes the lease, and only then returns success. Result byte
access returns read-only streams/copies; callers cannot mutate façade-owned buffers.

`Ra2MeshGenerationRequest` carries exactly one defensively copied reference image, optional bounded design brief,
optional negative constraints, one positive provenance seed, candidate count 1, optional preview request and bounded
timeout. It contains no project/output/executable path, environment block, API key or command text.

`Ra2MeshGenerationResult` exposes operation state, typed failure, safe message, provider/model/revision, request
fingerprint, duration and bounded progress summary. A successful Probe is `Ready` and contains no artifact. A successful
Generate is `CandidateReady` and contains exactly one GLB length/hash plus a read-only stream factory; all other states
contain no artifact. Expected operational failures do not escape as raw process/file/JSON exceptions.

The public façade does not expose a generic executable argument list, environment block, output path, shell command or
provider plugin interface.

`CreateFromBundle` performs only programmer-argument validation and captures normalized trusted roots. Missing/malformed
bundle files, hash/identity mismatches and readiness faults are operational outcomes returned by Probe/Generate through
the typed result; they do not escape as raw I/O exceptions. Generate repeats bundle path, manifest, executable hash,
provider identity and capability validation immediately before launching, so Probe is diagnostic evidence rather than a
time-of-check authorization token.

### 5.3 Product bundle

The IDE build produces this non-source output beneath its application directory:

```text
Providers/TencentHy3D/
  provider.bundle.json
  RA2IniEditor.AssetProviders.TencentHy3D.exe
  RA2IniEditor.AssetProviders.TencentHy3D.dll
  RA2IniEditor.AssetProviders.TencentHy3D.deps.json
  RA2IniEditor.AssetProviders.TencentHy3D.runtimeconfig.json
```

`provider.bundle.json` uses schema `ra2-asset-provider-bundle/1` and contains only relative executable identity, exact
SHA-256, protocol/provider/model/revision and `ReferenceImageToMesh` capability. It contains no API key, endpoint override,
user path, project path or accepted-credit fact.

The build must establish provider build order, copy only the fixed output allowlist, compute the executable hash after the
copy and write the manifest atomically. Runtime rejects missing, extra-rooted, reparse, malformed, mismatched or stale
bundle evidence. Runtime never probes repository `bin/obj` folders and never accepts a user-selected executable.

## 6. IDE generation session model

One `Ra2VoxelGenerationSession` is owned by the existing workspace ViewModel/coordinator and contains immutable facts:

- session generation number and run ID;
- active project-root identity captured at start;
- reference display name, format, length and SHA-256, but not model-visible/local absolute path;
- bounded design brief and negative constraints;
- provider/model/revision/request fingerprint and run duration;
- GLB length/SHA-256;
- palette identity/hash and selected neutral preview index;
- target resolution, conversion facts and canonical snapshot hash;
- generated canonical Direct snapshot;
- optional existing 2A quality review/candidates;
- terminal state/failure kind/safe message.

Session state is monotonic:

```text
Empty -> Ready -> Probing -> AwaitingConsent -> Generating -> AdoptingArtifact
      -> Converting -> CandidateReady
      -> Failed / Canceled / TimedOut
```

Changing project, selecting/loading another model, starting a newer generation or closing the document cancels the active
request and invalidates its generation number. Late progress/results cannot replace newer state. Source image bytes, GLB
bytes, generated snapshots and optional preview bytes are released when the session is replaced/closed.

No generated session is serialized into AvalonDock layout, app settings, recent projects or project files.

## 7. Input and admission contract

### 7.1 Required context

- An active, normal, non-reparse project directory is required.
- Provider workspace is fixed to a dedicated `%LocalAppData%/RA2IniEditor/AssetHost/Runs` root and must remain outside
  the active project; the project root is passed as a forbidden root.
- Bundle/profile readiness must pass before remote submission.

### 7.2 Reference image

- The user explicitly selects exactly one PNG or JPEG through a file picker.
- The image may be outside the project because the user chose it explicitly; no directory scanning occurs.
- It must be a regular non-reparse file with valid signature and size `1..6 MiB`.
- Bytes are read once into a defensively owned request and hashed locally.
- The confirmation surface states that this image will be sent to Tencent Hunyuan 3D.
- Absolute paths never enter Host prompt/provenance, model-visible text or user-facing failure diagnostics.

WebP remains unavailable for the current Tencent product profile even if the generic Host enum can represent it.

### 7.3 Palette and conversion

The generated mesh has no authoritative RA2 palette. Before generation the user must provide one of:

1. the complete palette already carried by a currently loaded canonical model; or
2. an explicitly selected, project-contained, regular, exactly 768-byte Westwood `.pal` file.

No theatre/team/remap palette is guessed. 3A uses an explicitly displayed neutral olive preview target resolved through
the existing nearest-opaque-index function. This is a temporary review colour, not final style evidence.

Target longest dimension is explicit, default `64`, allowed `32, 48, 64, 96, 128`; padding remains `1`. Role is Body.
Turret/Barrel promotion and multi-part splitting are forbidden.

### 7.4 Prompt and limits

- Design brief: optional, up to 8 KiB UTF-8 in the product UI; Host's lower-level 16 KiB ceiling remains unchanged. It is
  labelled as session intent/provenance because the current Tencent image request does not consume it as geometry input.
- Negative constraints: optional, up to 4 KiB UTF-8.
- Current provider candidate count: exactly 1.
- Current provider reference count: exactly 1.
- Timeout: product default 10 minutes, allowed 1..20 minutes.
- Seed: hidden fixed positive provenance value because the provider declares seed unsupported; UI must not promise replay.

## 8. Explicit cost/privacy gate

Probe is offline and may run after local validation. Generate may start only after a user confirmation showing:

- provider and model identity;
- reference image display name and size;
- that the image is sent to the official Tencent endpoint;
- exactly one remote job and no automatic retry;
- that the IDE cannot independently prove remaining free credit;
- that `RA2INI_HY3D_FREE_ONLY_CONFIRMED=1` is still required;
- that the result is a temporary review candidate and is not saved.

Canceling confirmation submits zero jobs. Missing API key/free-only configuration yields a typed readiness failure before
Submit. 3A does not persist a “never ask again” consent.

## 9. Composite execution transaction

1. Capture immutable project/input/palette/resolution state and increment the session generation.
2. Validate local paths, signatures, sizes, bundle manifest and workspace separation.
3. Run the façade Probe; publish safe readiness facts only if the generation is still current.
4. Obtain explicit cost/privacy consent.
5. Run exactly one Generate through AssetHost; forward bounded progress and cancellation.
6. On Host success, façade adopts exactly one GLB into bounded memory and disposes the workspace lease.
7. Verify the returned hash/length again at the IDE composition seam.
8. Parse and voxelize through the existing Application 1D path using the captured Body/palette/resolution policy.
9. Publish the generated Direct snapshot if and only if conversion succeeds and the session/project generation is current.
10. Attempt existing local 2A quality analysis against the admitted in-memory mesh. `NoSafeImprovement` keeps Direct and
    remains a successful generation; an unexpected quality-analysis failure is visible but cannot erase a valid Direct.
11. Switch the existing viewport to Direct. Do not compile style, call DeepSeek structure analysis, accept, export or save.

At every failure before step 9, no generated source replaces the last valid workspace source. There is no automatic retry,
fallback provider or silent re-submit.

## 10. Generated-source compatibility

The existing source model must be extended additively to distinguish:

```text
FileSource      -> real project-contained VOX/VXL file path
GeneratedSession -> no file path; project-root style anchor + generation provenance
```

Both variants carry one canonical snapshot and palette. GeneratedSession must not construct a fake `.vox` path. Existing
file admission and path checks remain unchanged.

Style source resolution for GeneratedSession anchors directory inheritance at the captured project root. Existing style
Compile remains explicit and may consume the generated snapshot after the user enters a style request. Existing 2C
structure recognition remains explicit. Neither is invoked automatically or treated as part of 3A success.

## 11. UI contract

3A modifies only the existing Voxel Style document. It adds one compact **生成模型** card above the existing 来源/几何
候选 cards. It does not change the Shell command bar, menus or Dock layout.

The card contains:

- reference image picker and one-line image facts;
- optional design brief multi-line editor labelled `设计说明（当前腾讯生成只使用参考图）`, default height 84 DIP and
  bounded growth to 150 DIP;
- collapsed-by-default `高级` row for negative constraints, resolution and timeout;
- palette status with a project PAL picker only when no current palette is reusable;
- primary `生成预览` button and existing shared Cancel behavior;
- compact progress text/percent and the current provider capability warning;
- no API-key textbox, executable picker, output-path field, seed promise or auto-save option.

The remote confirmation uses the existing window/dialog visual tokens, has no maximize button, and presents `取消` and
`提交 1 次生成` actions. It must not use a native form/DataGrid layout.

At 1920x1080 the existing 34/66 authoring/review ratio remains unchanged. At narrower widths the current outer scrolling
contract remains authoritative; controls wrap rather than force a wider left column.

### AutomationIds to preserve

All existing `VoxelStyle.*` AutomationIds remain unchanged, including source, quality, style, viewport and acceptance IDs.

### AutomationIds to add

```text
VoxelStyle.Generation.Card
VoxelStyle.Generation.ReferencePicker
VoxelStyle.Generation.ReferenceFacts
VoxelStyle.Generation.Brief
VoxelStyle.Generation.Advanced
VoxelStyle.Generation.NegativeConstraints
VoxelStyle.Generation.Resolution
VoxelStyle.Generation.Timeout
VoxelStyle.Generation.PalettePicker
VoxelStyle.Generation.PaletteFacts
VoxelStyle.Generation.Submit
VoxelStyle.Generation.Progress
VoxelStyle.Generation.CapabilityNotice
VoxelStyle.Generation.ConfirmDialog
VoxelStyle.Generation.ConfirmSubmit
VoxelStyle.Generation.ConfirmCancel
```

## 12. Failure taxonomy and messages

The IDE composite result must distinguish at least:

```text
None
NoActiveProject
InvalidProject
ReferenceMissing
ReferenceRejected
PaletteMissing
PaletteRejected
BundleMissing
BundleRejected
ProviderNotConfigured
ProviderNotReady
CapabilityUnavailable
ConsentDeclined
RemoteGenerationFailed
ArtifactMissing
ArtifactRejected
ArtifactTooLarge
ConversionRejected
NoSafeRefinement
ResourceLimitExceeded
TimedOut
Canceled
StaleResult
UnexpectedFailure
```

Provider failures map without exposing raw HTTP bodies, signed URLs, API keys, child command lines, stderr or absolute
paths. `NoSafeRefinement` is a warning-success when Direct exists. `StaleResult` is silently discarded from visible state
unless diagnostic tests request it.

## 13. Test and verification contract

### 13.1 AssetHost façade

- bundle manifest/path/hash/identity/capability success and every rejection boundary;
- façade maps every internal Host terminal state without leaking internal lease/path types;
- success copies exactly one GLB, verifies hash/size and disposes the lease;
- cancellation/timeout/cleanup and progress observer failures remain bounded;
- exported-type allowlist is exact and ledger-backed;
- deterministic fixture provider only; no network.

### 13.2 IDE orchestration

- no project, invalid reference, invalid PAL, missing bundle/config and capability unavailable fail before Generate;
- consent cancel causes zero Generate calls;
- exactly one Probe and one Generate on success; no retry;
- project/source change, newer run, close and cancel reject late progress/results;
- Host success + converter failure preserves previous valid source;
- valid GLB produces a GeneratedSession Direct snapshot and existing 3D scene;
- local quality success/NoSafeImprovement/failure preserve correct Direct semantics;
- generated source has no fake path and resolves style inheritance from project root;
- explicit style/structure actions remain separate and are never called automatically;
- no project/output file is created, modified or deleted.

### 13.3 UI/build/package

- exact AutomationId contract and existing IDs preserved;
- existing source-load, quality, style, viewport and layout tests remain green;
- real STA `InitializeComponent()` test passes;
- provider bundle contains only the fixed allowlist and manifest hash matches copied executable;
- source package excludes all provider build outputs, run workspaces, generated GLB/VOX and secrets.

Required commands after implementation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Manual acceptance after automated gates:

- 1920x1080 visual/layout review;
- invalid/missing environment explanation review;
- cancellation without remote job where possible;
- real Tencent smoke remains `NotRun` until separately authorized, limited to one confirmed free-only job.

## 14. Continuous implementation stages

| Stage | Goal | Required gate |
|---|---|---|
| 3A-0 | code-fact audit, contract, API/decision candidates | user approval of final contract |
| 3A-1 | narrow AssetHost façade and trusted bundle manifest | façade/bundle/lease tests; zero network |
| 3A-2 | generated-session model and IDE headless orchestration | fixture end-to-end + cancellation/stale/resource tests |
| 3A-3 | existing workspace UI and explicit consent composition | XAML/AutomationId/STA tests |
| 3A-4 | generated-source reuse of 1D/2A/1E/2C explicit paths | canonical snapshot and no-hidden-call/no-write tests |
| 3A-5 | focused/full regression, docs and clean package | build + all required suites + package |

Every stage receives its own pre-change risk recheck and post-stage review. Required-gate failure stops the package; tests
must not be weakened to continue. 3A-P1 real-provider acceptance is a separate explicitly authorized stage after 3A-5.

## 15. Public API ledger proposal

| Task | API | Kind | Reason | Expected next use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| ASSET-VOX-3A | `Ra2MeshGenerationFacade` family | exported façade/DTO/result/failure | allow product composition without exposing Host internals | IDE generation and future independent Agent Host | Experimental | exact export allowlist, façade lifecycle, bundle and mapping tests | no protocol/plugin/project-write authority |

Compatibility is additive. Existing internal Host contracts and Application exported allowlist remain unchanged. The
experimental façade may evolve only through a later reviewed contract.

## 16. Decision-log proposal

### Decision: Product generation uses a narrow Host façade and honest capability negotiation

- Status: Proposed; becomes Accepted only when this contract is approved.
- Decision: IDE consumes AssetHost through one bounded façade; current product generation is explicitly reference-guided.
- Rejected: IDE friendship to all Host internals, direct Tencent HTTP, repository-bin discovery, fake file paths and
  presenting provenance text as provider-consumed geometry input.
- Consequence: first useful product loop is safe and testable, but pure text-to-geometry and disk commit remain later work.
- Follow-up: semantic parts in 3B; explicit VOX commit/export in 3C; separately audited TextToMesh/TextToReferenceImage.

## 17. Self-review

### Risk review

Passed conditionally. R4 is correctly exposed and implementation is blocked on explicit approval. Real provider use is
not bundled into automated verification.

### Architecture/reuse review

Passed. The contract reuses one Host, one Tencent adapter, one GLB reader/voxelizer, one canonical snapshot and the existing
workspace. It rejects direct HTTP and a second generator UI.

### Data ownership/lifecycle review

Passed. Provider workspace ownership ends at façade adoption; generated GLB/snapshot state is bounded to the workspace
session; no fake file or project asset identity is introduced.

### Public API review

Passed conditionally. A public façade is necessary for the assembly boundary and future independent Host use. Its surface
is deliberately small, experimental, additive and exact-allowlist tested. Internal protocol/lease types remain hidden.

### UI review

Passed. The change is confined to the existing document, defines exact controls/AutomationIds and preserves the current
layout. Physical 1920x1080 acceptance remains mandatory after implementation.

### Cost/privacy review

Passed. One explicit submit, no retry, offline probe, existing free-only environment gate and per-run confirmation are
mandatory. The IDE does not claim it can verify free balance.

### Rework review

Passed with one intentional limitation: current Tencent geometry remains reference-image-driven. The contract does not
pretend to close pure text generation; instead it leaves an explicit capability seam. This prevents a later provider from
forcing replacement of Host, generated-session or review architecture.

## 18. Approval gate

No runtime, project, XAML, public API, build or provider change may start until the user explicitly approves this final
contract. Approval authorizes 3A-1 through 3A-5 only; it does not authorize a real Tencent/DeepSeek call, project Apply/Save,
VOX/VXL/HVA write, semantic-part work or Shell changes.
