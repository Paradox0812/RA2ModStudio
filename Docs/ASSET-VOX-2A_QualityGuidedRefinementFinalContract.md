# ASSET-VOX-2A Quality-Guided Refinement Final Contract

Date: 2026-08-27

Risk: R4

State: Accepted / implemented / automated verified / connectivity correction applied

## 0. Final delivery statement

ASSET-VOX-2A adds a review-first quality layer between immutable admitted mesh evidence and an accepted styled voxel
candidate. It reduces conversion aliasing without editing or regenerating the source model, measures rather than assumes
symmetry, improves weak armour-colour contrast, and exposes at most three bounded DeepSeek reasoning rounds over local
facts. A language model never writes cells, vertices, files or palette bytes and never silently replaces the source.

The user explicitly excluded original-model adjustment. Source mesh positions, triangles, transforms, provider artifacts
and hashes are therefore frozen throughout this stage.

## 1. Delivered outcome

The repository can now:

1. derive deterministic surface, silhouette, roughness, symmetry and thin-feature facts;
2. produce direct and bounded 2x-supersampled conversion candidates;
3. preserve every protected source coordinate through cleanup;
4. produce an optional local X-symmetry suggestion without an enforcement mode;
5. compare deterministic normal fields for direct and refined candidates;
6. expose provenance-tagged semantic review regions without promoting guesses to masks;
7. improve weak body-role palette contrast while preserving explicit/semantic/remap choices; and
8. run a one-to-three-round, early-stoppable, fully keyed DeepSeek coordinator through fake clients.

These are headless candidate capabilities. Existing UI composition, real provider calls, project Apply/Save and VXL/HVA
materialization are not part of 2A.

## 2. Scope

### 2.1 Modified production areas

```text
RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/
RA2IniEditor.IDE/AssetAuthoring/
```

### 2.2 Tests and governance

```text
RA2IniEditor.Application.Tests/
RA2IniEditor.Tests/IDE/
Docs/ASSET-VOX-2A_*.md
Docs/DecisionLog.md
Docs/PublicApiLedger.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

### 2.3 Frozen boundaries

- Shell, docking, XAML, menus, themes, AutomationIds and 3D viewport composition;
- INI parser, Field Registry, diagnostics, completion, Work mode and Preview/Apply/Undo/Save;
- AssetHost protocol, Tencent request/billing/retry and every provider executable;
- canonical snapshot serialization, VOX/VXL reader semantics and assembly graph;
- VXL/HVA writing, project asset persistence and game launch/validation;
- third-party packages and public exported APIs.

## 3. Architecture and authority

```text
immutable admitted mesh
  -> existing direct voxelizer
  -> deterministic source facts + protected-coordinate mask
  -> bounded 2x voxelization
  -> coverage downsample + one bounded cleanup pass
  -> protected-coordinate survival + local quality gates
  -> optional symmetry suggestion candidate
  -> normal/semantic/palette review facts
  -> immutable headless review candidates
```

- `Ra2VoxelSceneSnapshot` remains the sole canonical voxel truth.
- Every output is derived and source-hash-bound; no source object is mutated.
- DeepSeek may only classify facts and select closed values already present in a tool schema.
- Local deterministic code remains the sole authority for candidate creation and admission.
- `Ra2VoxelAssetAssemblySpec` remains the only Body/Turret/Barrel graph.

## 4. Frozen refinement profile

`asset-vox-2a/refinement-v1` is deterministic and hashable:

- transient longest dimension: `min(target * 2, 128)`;
- coverage threshold: 25–75%, default 40%; the default is backed by the certified `body-candidate.vox + mesh.glb`
  product-path probe and keeps the existing 5% volume and 3% silhouette gates intact;
- thin span threshold: 1–4, default 2;
- occupied-volume delta gate: default 5%;
- fixed-view silhouette-area delta gate: default 3%;
- cleanup passes: 0–1, default 1;
- CPU-only, no randomness and no order-dependent accumulation.

No mesh smoothing, vertex displacement, welding, topology repair or source-model regeneration exists in this profile.

## 5. Quality facts and feature protection

For direct/refined candidates, local analysis records:

- occupied/surface/exposed-face/low-support counts;
- fixed front/rear/left/right/top/bottom silhouette area and hash;
- mirrored-pair, unmatched-cell and normalized X-symmetry facts;
- supported multi-axis thin-feature count and source-bound protection-mask hash;
- canonical source/facts hashes; and
- source/candidate normal-field hashes plus common/changed sample counts.

A protected cell must be locally thin on at least two axes and have stable face-neighbour support. An isolated attached bump
is deliberately not protected. Every protected coordinate must exist in the refined candidate; aggregate-count comparison
is not accepted as a substitute.

## 6. Conversion and cleanup

1. Convert once at target resolution using the existing voxelizer.
2. Analyse and freeze the source protection mask.
3. Convert the unchanged mesh again at bounded 2x resolution.
4. Downsample by deterministic cell-coverage voting.
5. Remove only non-protected one-neighbour cells and fill only five-neighbour interior holes, at most once.
6. Union protected source coordinates back into the candidate.
7. Reject loss of protected coordinates, unsafe volume or unsafe silhouette changes. Connectivity is evaluated relatively:
   one component passes; multiple components pass only when at least 95% of occupied cells remain in one dominant body;
   evenly fragmented output is rejected. Component count and dominant-body share remain visible review facts.
8. Construct a new snapshot using the existing constructor and derivation hash.

Failure to improve safely returns a typed non-success; it does not relax a gate.

## 7. Symmetry suggestion

Only two modes exist:

```text
Off       measure only
Suggest   produce a separate review candidate
```

The canonical plane is X-centre. Local mirrored support may add one missing mirror or remove an unsupported one-cell bump.
Protected cells are excluded. No side is copied wholesale, Y/Z bounds are unchanged, results without a dominant body are rejected,
and a suggestion is retained only when unmatched-cell count does not regress and local quality gates pass.

There is intentionally no silent `Enforce` mode.

## 8. Bounded DeepSeek coordinator

At most three distinct semantic requests are possible for one fully keyed facts pair:

1. `diagnose_voxel_quality`: `continue | no_action`, bounded summary and risks;
2. `plan_voxel_refinement`: coverage 25–75, `off | suggest`, bounded labels and rationale;
3. `review_voxel_refinement`: `accept | reject`, `preserve | contrast`, bounded notes.

Each request requires exactly one matching tool call and no prose. A `no_action` diagnosis stops after one call. An exact
cache hit makes zero calls. Malformed/provider/cancel outcomes are typed and never retried. A fourth call is structurally
impossible. Automated tests use a fake client and make zero network calls.

This coordinator is a headless seam; it is not wired to the current product UI and was not executed against real
DeepSeek in this stage.

## 9. Semantic-region boundary

The review package emits four bounded candidates:

- `body-shell`: complete occupied candidate, geometry-derived;
- `lower-contact-candidate`: possible wheels/tracks, model-inferred and non-executable;
- `upper-aperture-candidate`: possible glass/hatches/turret detail, model-inferred and non-executable;
- `protected-thin-structures`: geometry protection evidence, not a functional-material label.

Every region includes region id, provenance, derivation id, cell count, confidence and rationale. Text-only DeepSeek
cannot promote tyre, glass, gun shield, weapon or remap semantics to verified masks.

## 10. Palette-contrast boundary

- Reuse the existing palette profile, compiled style plan and colourizer.
- Optimisation runs only when distinct body-role luminance separation is below 10.
- Bounded target offsets are Light +30, Base 0, Mid -20, Dark -42 and Underside -58 relative to the base role.
- Selection is limited to opaque non-remap entries and balances luminance against colour-family distance.
- Exact palette selections, semantic material roles, remap roles, rules, scopes and the input plan are immutable.
- The result is a separate review plan with before/after separation and changed-role facts.
- An imperfect palette produces the closest safe alternative, not rejection of an otherwise valid body style.

Colour never changes occupancy, coordinates, part identity, pivot, normals or HVA.

## 11. Typed outcomes

Geometry/refinement:

```text
Success
NoSafeImprovement
InvalidOptions
SourceConversionFailed
SupersampleConversionFailed
ProtectedFeatureConflict
QualityGateRejected
AnalysisFailed
Cancelled
```

AI coordination:

```text
Success
ProviderFailure
MalformedDiagnosis
MalformedPlan
MalformedReview
Cancelled
```

Failures publish no admitted refined candidate and carry bounded local messages.

## 12. Continuous stage results

| Stage | Result | Gate |
|---|---|---|
| `2A-1` | facts, silhouettes, protection mask | deterministic/cancel/isolated-bump fixtures |
| `2A-2` | supersampled conversion and cleanup | dimensions/connectivity/protected survival/determinism |
| `2A-3` | bounded symmetry suggestion | asymmetric/symmetric/source-immutability fixtures |
| `2A-4` | three-round maximum coordinator | 5 fake-client round/cache/early-stop/malformed/failure tests |
| `2A-5` | semantic/normal/palette review facts | palette preservation/improvement/determinism + full regression |

## 13. Verification result

```text
Application.Tests: 267/267 passed
IDE.Tests: 2814/2814 passed
AssetHost.Tests: 47/47 passed
Focused new geometry/palette tests: 9/9 passed
Focused new coordinator tests: 5/5 passed
IDE-only solution build: 0 errors, 1 pre-existing nullable test warning
IdeOnly clean source package: 1366 files
Certified product-path probe: refined 17,397 cells; 1 component; 100% dominant share; unchanged 5%/3% gates passed
Real Tencent calls: 0
Real DeepSeek calls: 0
```

Because the user was running the IDE, default output DLLs were locked. Verification used isolated output directories;
the first isolated directory was corrected so repository-path-dependent tests retained their expected depth. Final full
suites passed.

## 14. API, persistence and compatibility

- All new production types are internal.
- Application exported API and AssetHost protocol are unchanged.
- No serialized/cache/project format is added; AI cache lifetime is process memory.
- No user setting or AutomationId is added.
- Legacy editor is not restored.

## 15. Explicit non-goals and remaining gates

- source-model repair/regeneration or another Tencent call;
- authoritative visual semantic recognition;
- Body/Turret/Barrel automatic split;
- UI composition of direct/refined/symmetry candidates;
- real DeepSeek acceptance;
- VXL/HVA writing, Apply/Save, project materialization or GameReady certification.

Recommended next stage: `ASSET-VOX-2A-UI Review Candidate Composition`, followed separately by a user-authorized live
DeepSeek trial only if its added value justifies the semantic-call cost.
