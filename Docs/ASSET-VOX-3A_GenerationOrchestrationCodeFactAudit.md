# ASSET-VOX-3A Generation Orchestration — Code Fact Audit

Date: 2026-08-28  
State: completed / contract input  
Implementation: not started  
Risk: R4 network/cost/artifact boundary + R3 IDE lifecycle composition + R2 exported façade

## 1. Audit question

Determine the smallest product composition that lets the existing Voxel Style workspace explicitly start one bounded
reference-image-to-mesh run, consume the verified GLB through the existing GLB-to-canonical-voxel bridge, and publish a
session-only 3D candidate without claiming text-only generation or adding project write authority.

## 2. Current code facts

### 2.1 The generation Host exists but the IDE cannot consume it

- `RA2IniEditor.AssetHost` owns the versioned local-process protocol, trusted executable configuration, workspace lease,
  progress, cancellation, timeout, process-tree termination, artifact validation and cleanup.
- `IRa2VoxelGenerationHost`, its request/result DTOs and the workspace lease are all assembly-internal.
- `RA2IniEditor.AssetHost` exports zero public types and grants friendship only to its test assembly.
- `RA2IniEditor.IDE` has no project reference to AssetHost and contains no Host configuration or generation adapter.

Consequently the current IDE cannot probe or run the completed Host. Adding direct HTTP/Tencent logic to the IDE would
create a second provider lifecycle and is rejected.

### 2.2 The real Tencent adapter is certified but image-driven

`RA2IniEditor.AssetProviders.TencentHy3D` implements the existing child-process protocol and the fixed profile:

```text
providerId: tencent-hy3d-openai-compatible
providerVersion: 1.0.0
modelId: hunyuan-3d-professional
modelRevision: 3.1-geometry
capability: ReferenceImageToMesh
references: exactly one
candidates: exactly one
```

The professional request sends `ImageBase64`. It does not send the Host design prompt together with the image because
the certified provider request shape makes those inputs mutually exclusive. The prompt is therefore provenance/intent
text in the current provider path, not geometry-driving text.

The adapter reads only `RA2INI_HY3D_API_KEY`, the official endpoint configuration and
`RA2INI_HY3D_FREE_ONLY_CONFIRMED=1`. Probe is offline; Generate is the only potentially billable operation and never
automatically retries Submit.

### 2.3 The GLB-to-voxel bridge already exists

Application already owns the restricted GLB reader, topology facts, deterministic voxelizer and canonical
`Ra2VoxelSceneSnapshot`. It requires:

- GLB no larger than 16 MiB and within the accepted restricted glTF profile;
- an explicit Body/Turret/Barrel role;
- target resolution, padding and palette policy;
- an explicit complete `Ra2VoxelPaletteProfile` and colour/index selection.

The resulting snapshot is review-required and not a final VXL/HVA asset. Reimplementing GLB parsing or voxelization in
AssetHost/IDE is rejected.

### 2.4 The current workspace is file-source-only and session-only

`Ra2VoxelStylePreviewCoordinator.LoadSource` accepts one project-contained `.vox` or one single-Section `.vxl` plus a
project-contained 768-byte PAL. Its source result requires a real file path. Style compilation also assumes a file-backed
source path when resolving directory inheritance.

Quality review may pair a project-contained GLB with a loaded voxel baseline, but it cannot consume a Host lease or
in-memory generated GLB. `UseCurrentQualityCandidateForSession` changes only `_workingGeometry`; `AcceptCurrentSession`
changes only an in-memory accepted flag. Neither writes a model file.

The existing workspace and native 3D viewport are the correct presentation surface. A second generator window or a new
Shell document is unnecessary.

### 2.5 Packaging is not product-ready

The Tencent provider project is in the IDE-only solution, but its executable/runtime files are not copied into the IDE
output as an integrity-described provider bundle. The IDE has no trusted runtime manifest containing executable identity,
hash, model revision or capability. Repository-relative `bin` probing and arbitrary user-selected executable paths are
not acceptable product behavior.

## 3. Reuse scan

| Need | Canonical implementation | Decision |
|---|---|---|
| Process/provider lifecycle | `Ra2VoxelGenerationHost` | Reuse unchanged behind one narrow façade |
| Tencent submit/poll/download | `RA2IniEditor.AssetProviders.TencentHy3D` | Reuse unchanged; no IDE HTTP client |
| GLB validation | `Ra2GlbMeshReader` | Reuse in Application through IDE friend boundary |
| GLB voxelization | `Ra2MeshVoxelizer` | Reuse; no second voxel model |
| Canonical truth | `Ra2VoxelSceneSnapshot` | Preserve as the sole in-memory part truth |
| Palette decoding | existing Westwood PAL path | Reuse exact 768-byte project-contained PAL policy |
| 3D review | existing Voxel Style document/viewport | Extend current surface; no new document |
| Quality refinement | existing 2A/R2 coordinator/core | Reuse from admitted in-memory mesh/snapshot |
| Structure analysis | existing 2C explicit action | Remains explicit; never auto-run after generation |
| Style compilation | existing 1E compiler | Remains explicit; never auto-run after generation |
| Project write/export | none in current workspace | Deliberately defer to ASSET-VOX-3C |

Search covered `Ra2VoxelGenerationHost`, Tencent provider constants/client/protocol, IDE project references, workspace
source/quality coordinators, GLB reader/voxelizer, PAL admission, review artifacts, AutomationIds and related tests.

## 4. Architecture check

### Touched boundaries proposed for implementation

```text
IDE Voxel Style UI
  -> IDE-internal generation session coordinator
  -> narrow AssetHost exported façade
  -> existing internal Host + Tencent child process
  -> verified bounded GLB bytes
  -> existing Application reader/voxelizer/refiner
  -> generated in-memory source session
  -> existing 3D/style/structure review paths
```

AssetHost remains provider/process/workspace authority. Application remains geometry/canonical-data authority. IDE owns
product configuration, explicit user consent, current-project context and transient presentation/session state.

### Rejected alternatives

1. **Add `InternalsVisibleTo RA2IniEditor.IDE` to AssetHost.** Rejected because it exposes the entire Host implementation
   boundary rather than the separately anticipated façade.
2. **Call Tencent directly from the IDE.** Rejected because it duplicates timeout, protocol, cost, secret and artifact
   ownership already certified in AssetHost.
3. **Copy GLB into the project before review.** Rejected because it introduces premature persistence and cleanup/undo
   obligations.
4. **Pretend the existing provider supports text-only generation.** Rejected because the certified capability is
   `ReferenceImageToMesh` and its remote request does not include the design prompt.
5. **Create a second generator document.** Rejected because the existing Voxel Style workspace already owns canonical
   review, quality candidates, style and structure views.
6. **Use a synthetic/non-existent source path.** Rejected because it would disguise session data as a project asset and
   break source identity/lifecycle semantics.

## 5. Data model check

| Concept | Primary owner | Lifetime | Serialization |
|---|---|---|---|
| Bundled provider manifest | IDE product bundle | build/output lifetime | generated output manifest; no secret |
| Trusted provider launch profile | AssetHost façade caller | one probe/run | not persisted |
| Reference image bytes/hash | generation request | one run | Host staging only; deleted with lease |
| Design brief/negative constraints | generation request | one run/session provenance | not written to project |
| Verified GLB bytes/hash/provenance | façade result then IDE session | until conversion/session replacement | memory only in 3A |
| Generated voxel snapshot | IDE generation session using Application truth | until source replacement/close | memory only in 3A |
| Optional quality candidates | existing quality transaction | current generation/session | derived, memory only |
| Provider progress/failure | current request | terminal UI state | not persisted |
| Cost consent | UI action + existing environment gate | each submit/session | not persisted by IDE |

The generated source must explicitly carry `GeneratedSession` provenance and a project-root style anchor. It must not use
a fake file path. Existing file-loaded provenance remains unchanged.

## 6. Main contract gaps to close

1. Narrow exported AssetHost façade without exposing the internal lease or provider protocol.
2. Integrity-described bundled provider output and deterministic runtime discovery.
3. Exact reference-image, PAL, resolution, consent and project-context admission.
4. Lease-to-memory artifact adoption and disposal before UI publication.
5. A generated-source identity that works with existing review/style paths without pretending to be a file.
6. Cancellation/stale-generation protection across Probe, Generate, artifact copy, conversion and local refinement.
7. Typed distinction among provider, bundle, conversion, palette, resource, cancellation and cost failures.
8. No hidden remote call, automatic retry, automatic style compile, automatic structure recognition or project write.
9. Honest capability presentation: current 3A is reference-guided; text-only geometry remains unavailable.

## 7. Risk classification

```text
Level: R4 overall
Reason: external network/cost action, executable integrity bundle, artifact handoff and public façade compatibility boundary
Changed areas: AssetHost façade; IDE build composition; IDE workspace coordinator/ViewModel/XAML; tests/docs
High-risk files: AssetHost contracts/host adapter, IDE csproj and workspace lifecycle
Public API impact: additive AssetHost experimental façade/result/request/failure contracts
Architecture impact: first production IDE -> AssetHost -> Application composition
Persistence impact: no project persistence; generated output manifest only in build output
Test impact: new façade, bundle, orchestration, stale/cancel, UI contract and full regression tests
Governance mode: StopForReview
Allowed action now: audit and final-contract documentation only
Stop condition: explicit user approval required before any runtime/project/XAML change
```

## 8. Audit conclusion

The path is feasible without replacing the completed Host, Tencent adapter, converter or workspace. The reliable minimum
is a reference-guided, explicit-submit, session-only product loop. Pure natural-language geometry and model-file commit
remain separate capabilities and must not be reported as completed by 3A.

