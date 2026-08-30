# ASSET-VOX-2B Semantic Symmetry Continuous Final Contract

Status: completed (`2B-0` through `2B-4`); automated verification passed; manual 1920x1080/provider acceptance remains separate

Package: `2B-0` through `2B-4`

## 1. Goal

Produce a review-first symmetry candidate in which every host- and model-confirmed structural-body region is strictly
left/right symmetric, while asymmetric attachments, protected thin features and uncertain regions retain their source
occupancy. DeepSeek classifies bounded host regions; deterministic local code alone chooses and changes voxel pairs.

## 2. Non-goals and frozen boundaries

This package does not:

- write, apply, save, export or replace VOX/VXL/HVA/assets;
- alter Shell, docking, menus or project lifecycle;
- change `Ra2AiRequest` into a multimodal/network protocol;
- call Tencent or change provider configuration;
- change INI, Field Registry, parser, completion, diagnostics or save preflight;
- perform production-quality material segmentation or colour optimization; or
- treat a DeepSeek classification as geometry authority.

No real DeepSeek call is permitted during automated implementation verification. A later manual provider probe requires
separate explicit approval.

## 3. Data contracts and invariants

Add internal, immutable derived contracts under the existing experimental voxel-authoring namespace:

### `Ra2VoxelSymmetryEvidencePackage`

- source snapshot hash and refinement profile hash;
- chosen and alternative lateral planes;
- six bounded silhouette summaries;
- ordered host-owned region evidence;
- package hash over canonical ordering.

### `Ra2VoxelSymmetryRegionEvidence`

- stable region ID created by the host;
- normalized bounding box and cell count;
- span, surface, support, contact, branch and thinness facts;
- mirrored-match and mismatch counts;
- GLB coverage summary;
- frozen/transition overlap; and
- no raw file path, arbitrary prompt text or per-voxel coordinate list.

### `Ra2VoxelSymmetryDisposition`

Exactly four values:

1. `SymmetricCore`
2. `AsymmetricAttachment`
3. `ProtectedThinFeature`
4. `Uncertain`

### `Ra2VoxelSemanticPartition`

- exact evidence-package hash;
- reviewed lateral plane;
- one decision for every host region ID;
- two-round provenance and confidence;
- immutable host-owned coordinate masks resolved from region IDs; and
- deterministic partition hash.

Invariants:

- every candidate cell belongs to exactly one disposition;
- every frozen cell is `ProtectedThinFeature` or `Uncertain` and is never editable;
- an unknown, missing, duplicate or invented region ID invalidates the model round;
- model disagreement or confidence below `0.80` resolves to `Uncertain`;
- partition masks are derived state and are never serialized;
- structural disposition and future material identity are separate concepts.

## 4. `2B-0` — bounded multiview evidence

Add a deterministic evidence builder after local Direct/Refined generation:

1. Evaluate half-cell lateral-plane candidates within two cells of the occupancy median.
2. Select the plane lexicographically by minimum protected mismatch, minimum total mismatch, minimum mesh-coverage residual,
   then minimum distance from the median.
3. Segment mirror mismatches and protected structures into at most 64 host regions using stable 26-neighbour grouping.
4. Merge only adjacent regions with equivalent bounded facts; never truncate cells silently.
5. Emit six 32x32-or-smaller run-length silhouette summaries and region facts.
6. Cap each AI prompt at 32,768 characters. If evidence cannot fit after deterministic compaction, return a typed local
   `EvidenceTooLarge` result and leave symmetry unavailable.

Required tests include deterministic hashes, stable ordering, plane selection, region coverage, cap handling and absence of
absolute paths/per-voxel lists.

## 5. `2B-1` — structured two-round DeepSeek classification

Add an IDE-internal compiler that reuses `IRa2AiClient`, separated messages and required tool calls.

Round one tool result:

- outcome: `proposal`, `clarification` or `unsupported`;
- evidence-package hash and reviewed plane;
- exactly one disposition/confidence/reason for each host region ID; and
- bounded unresolved assumptions.

Round two receives the same evidence plus the normalized round-one proposal and acts only as a critic. It returns the same
closed shape with final decisions. Local reconciliation rules are authoritative:

- exact agreement at confidence >= 0.80 keeps the decision;
- disagreement, omission or lower confidence becomes `Uncertain`;
- frozen overlap cannot become `SymmetricCore` or `AsymmetricAttachment`;
- DeepSeek may not invent coordinates, masks, paths, palette values or geometry changes; and
- malformed/provider/timeout/cancelled results are typed and do not remove Direct/Refined candidates.

The existing local `Generate candidates` action remains free of provider calls. AI classification is an explicit separate
user action.

## 6. `2B-2` — deterministic constrained symmetry

Add a new overload/path that accepts the reviewed partition. Keep the existing local quality conversion path intact.

For every mirrored pair inside `SymmetricCore`:

- both occupied remains occupied;
- both empty remains empty;
- a one-sided pair is filled on both sides only when bounded GLB evidence reaches the add threshold;
- it is cleared on both sides only when GLB evidence is below the keep threshold;
- an ambiguous pair is moved to `Uncertain` before execution and is not changed.

`AsymmetricAttachment`, `ProtectedThinFeature`, `Uncertain` and a one-cell transition band around their boundary preserve
source occupancy exactly. The resulting `SymmetricCore` must have zero unmatched pairs.

All existing hard gates remain mandatory: one connected body, no new cavities, frozen/transition preservation, bounded
volume/silhouette delta, no roughness/support regression beyond existing limits and deterministic hash. A failed gate returns
typed no-symmetry/diagnostic state; it never weakens a threshold or substitutes the old global support-count suggestion.

## 7. `2B-3` — workspace orchestration and stale-result safety

Product flow:

```text
Generate candidates (local, no provider)
  -> explicit AI structure recognition (two DeepSeek rounds)
  -> reviewed semantic partition
  -> deterministic constrained symmetry candidate
  -> 3D review
  -> optional Use for this session
```

Rules:

- changing source, GLB, local candidate generation, project root or model invalidates the partition and symmetry candidate;
- the existing operation generation/cancellation token owns both rounds;
- a late or cancelled result cannot publish into a newer session;
- Direct/Refined stay reviewable if DeepSeek is unavailable or classification fails;
- only an admitted constrained symmetry candidate can be selected; and
- selecting it clears compiled style preview exactly like the current geometry-candidate path.

## 8. `2B-4` — exact UI contract

Modify only the existing Voxel Style workspace. Do not change Shell or docking.

### Commands

- Preserve `VoxelStyle.Quality.Generate`, its label `生成候选`, and its local/no-cost behavior.
- Add a compact adjacent button labelled `AI 识别结构` with AutomationId
  `VoxelStyle.Quality.AnalyzeStructure`.
- Enable it only when local quality candidates are current, DeepSeek configuration is ready and no operation is active.
- Its tooltip must state that it performs two DeepSeek requests, may consume quota and never writes files.

### Preview modes

- Preserve all existing mode buttons and AutomationIds.
- Insert `结构区` between `差异` and `对称`, AutomationId `VoxelStyle.Preview.StructureRegions`.
- `结构区` displays the refined source candidate with this fixed overlay:
  - cyan: `SymmetricCore`;
  - amber: `AsymmetricAttachment`;
  - blue: `ProtectedThinFeature`;
  - violet: `Uncertain`;
  - neutral grey: unchanged/unclassified background.
- The existing `对称` mode continues to display final geometry, not the classification overlay.
- The existing `区域` mode continues to mean colour/style region mask and must not be repurposed.

### Review information

- Preserve `VoxelStyle.Quality.SemanticRegions`; extend each row to show disposition, provenance, confidence, cell count and
  bounded review reason.
- Add `VoxelStyle.Quality.SemanticLegend` for the fixed structure colours.
- Add `VoxelStyle.Quality.SemanticReview` for plane, round agreement, uncertain count and symmetry admission status.
- Keep the current viewport height, grid columns, scroll ownership, styles and responsive wrapping. No new modal window,
  sidebar, settings form or fixed-width panel is permitted.

### Interaction

- `AI 识别结构` publishes no UI state until both rounds and local reconciliation finish.
- Cancellation retains the last fully completed local quality preview and publishes no partial partition.
- Failure is shown in the existing status surface; no message box is introduced.
- `用于本会话` remains the only geometry-selection action and is enabled for symmetry only after all gates pass.

## 9. Allowed files

Expected runtime scope:

- `RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/Ra2VoxelQualityRefinement.cs`
- one new Application semantic-symmetry contract/algorithm file if separation is clearer
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelStylePreviewCoordinator.cs`
- one new IDE DeepSeek semantic compiler file
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelQualityReviewProjection.cs`
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelViewportSceneBuilder.cs`
- `RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml`
- its existing code-behind only for the approved button/mode wiring
- directly corresponding Application and IDE tests
- this contract, stage ledger, DecisionLog candidate and current context/status docs

No project file change is expected because SDK default compile inclusion already applies.

## 10. Verification gates

Per-stage targeted gates:

- 2B-0: evidence/plane/region determinism and bounds;
- 2B-1: two-round agreement, disagreement, malformed, timeout, cancellation and stale hash;
- 2B-2: exact core symmetry, attachment/thin/uncertain preservation, transition seam, connectivity/cavity/silhouette gates;
- 2B-3: coordinator/ViewModel cancellation, invalidation, no-provider fallback and explicit session selection;
- 2B-4: XAML AutomationIds, button enablement, semantic overlay colours and existing mode preservation.

Package gates:

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Manual 1920x1080 review remains required after implementation. It must confirm that attachments retain intentional
asymmetry, the body core has no unmatched pair, thin structures survive and structure-region colours are understandable.

## 11. Continuous-package stop rules

Stop and flush partial state if:

- any stage's required targeted test fails;
- a new public or multimodal network contract becomes necessary;
- a frozen/attachment/uncertain cell changes;
- admitted `SymmetricCore` retains any unmatched pair;
- provider failure blocks Direct/Refined review;
- implementation requires Shell, persistence, Apply/Save, VXL/HVA or real paid calls; or
- the approved UI dimensions/AutomationIds cannot be preserved.

After 2B-4 verification and documentation flush, stop. Material-semantic colouring optimization is the recommended next
package and is not silently included in 2B.

## 12. 2026-08-28 physical-sample correction

The first implementation interpreted step 4.3 as one model region per 26-neighbour mismatch/protection component. The
real `body-candidate.vox + mesh.glb` pair contains more than 64 disconnected surface-difference components, so the local
candidate succeeded while the evidence package failed and the `AI 识别结构` action remained disabled.

The corrected bounded evidence contract is:

1. The complete matched core remains one host region.
2. Locally protected coordinates remain exact and are summarized as one protected region with an explicit connected-
   component count; local protection authority is unchanged.
3. Mismatch components remain exact internally, but the model-facing region list deterministically groups them by lateral
   side, lower/middle/upper height, front/rear depth and morphology (`detached`, `thin-attached`, `detail-attached` or
   `bodylike-attached`). This produces at most 48 mismatch regions and at most 50 total regions.
4. Every occupied coordinate must still occur in exactly one region. Grouping may summarize disconnected components but
   may not truncate, sample or discard coordinates.
5. Each region reports its exact connected-component count. Package hashing includes that fact.
6. Evidence/prompt limits remain unchanged. A residual failure reports its measured region or character count rather than
   a generic boundary message.

Raising the 64-region/tool-output limit and silently dropping small components remain rejected. This correction changes
only derived, nonserialized evidence and status diagnostics; it does not change provider calls, geometry authority,
topology gates, Shell, Apply/Save, VXL/HVA or project files.

## 13. 2026-08-28 visual-delta and provider-shape correction

Manual product review exposed three acceptance failures after the evidence-boundary correction:

1. The admitted Refined candidate changed too few cells to be legible at whole-model scale. The candidate set now includes
   Conservative, fill-biased Balanced and bounded SurfacePolish thresholds. Existing connectivity, frozen-coordinate,
   cavity, volume, six-view silhouette and measurable-improvement gates remain authoritative.
2. Difference blue previously meant local frozen geometry, not an AI symmetric region. Difference now renders only added
   green, removed red and unchanged translucent grey. Blue is reserved for `ProtectedThinFeature` in the post-AI structure
   view and may intentionally be one-sided; confirmed symmetric body is cyan.
3. Model content invariants remain strict, but equivalent provider JSON representation is normalized. Optional explanatory
   fields, additional metadata, camelCase aliases, string-form bounded numbers and common tool-argument wrappers are
   accepted. Evidence hash, selected plane, exact known region IDs, complete region coverage and bounded dispositions are
   still mandatory. Invented or missing regions still fail before geometry execution.

When evidence exists, `AI 识别结构` remains clickable even if configuration readiness changed after workspace creation;
the click path reports missing configuration without sending a request. AutomationIds and the original quota/read-only
HelpText remain unchanged.

## 14. 2026-08-28 candidate-selection and recognition-diagnostic hardening

Post-implementation self-review found that the stronger candidate was still ranked primarily by low-support count. That
could automatically prefer an erosive cleanup even when it did not materially improve roughness. The final correction is:

1. A candidate is eligible for automatic smoothing selection only when its roughness improves by more than `0.005` and
   every existing topology, frozen-coordinate, cavity, volume and silhouette gate passes.
2. Eligible candidates are ranked by roughness first, then low-support/unmatched cells and total delta. A safe cleanup-only
   candidate remains visible for comparison but cannot become Refined solely because it removes more cells.
3. Candidate derivation identity includes candidate kind, cluster threshold and occupancy threshold. The three candidates
   can no longer share incomplete provenance.
4. `组合审阅` reports all candidates, admission state, added/removed counts, roughness and low-support facts. This is
   derived session state and is not persisted.
5. Semantic tool parsing retains exact evidence authority but reports canonical malformed/mismatch causes. Duplicate aliases
   fail closed; common equivalent wrappers/camelCase/string-number forms remain accepted.
6. Difference rendering requires only a same-grid comparison snapshot. The protection mask is not a Difference input and
   may not block that review mode.

The local certified sample selects `Conservative` instead of `SurfacePolish`: roughness `1.552632 -> 1.542869`, low-support
`76 -> 70`, with `+24/-29` cells. SurfacePolish remains review-only (`1.549592`, low-support 58, `+14/-153`). This probe is
provider-free and writes no files.

## 15. 2026-08-28 neutral repair-evidence correction

Live review proved that the bounded evidence contract still encoded a semantic conclusion before DeepSeek ran. The
matched `core` region was already fully mirrored, while every unmatched region ID contained `mismatch` plus morphology
such as `thin-attached`, `detail-attached` or `bodylike-attached`. Both model rounds therefore tended to protect every
actual repair opportunity, leaving the executor with an already-symmetric core and no possible change.

This section supersedes the model-facing naming and prompt semantics in section 12 without changing coordinate authority:

1. Unmatched occupied-side groups use neutral `repair-*` IDs. Morphology is limited to `detached`, `slender`, `compact`
   or `broad`; no region ID asserts that geometry is an attachment.
2. Every region adds deterministic `mirror_coverage` and `mirror_contact` facts. They describe GLB support and existing-
   body contact at the empty mirrored target; coordinates, paths and material claims remain absent from the prompt.
3. `core-*` is explicitly documented as already mirror-matched context. `repair-*` is a question for classification, not
   a preclassified accessory. Missing occupancy on the opposite side is the repair condition and is not ambiguity by
   itself.
4. A repair region may become `SymmetricCore` only when both model rounds independently agree at the existing confidence
   threshold. Positive bounded evidence is required before calling it an intentional attachment or protected thin feature.
5. The second round verifies the first against the same facts. It must not manufacture disagreement merely to appear
   critical. Real disagreement still becomes `Uncertain`; the reconciler and executor thresholds are unchanged.
6. If no repair region is jointly admitted, the UI reports that exact reason in Chinese and retains Direct/Refined. It no
   longer presents the executor's internal English sentence as a generic geometry-gate failure.

Rejected alternatives remain: locally force every mismatch into the core, relax two-round reconciliation, infer material
from structure, add a third provider call, or let DeepSeek return coordinates/edits. This correction changes only internal,
nonserialized derived evidence, prompt semantics and diagnostics.
