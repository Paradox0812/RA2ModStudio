# ASSET-VOX-2B Stage Ledger

Status: Completed in code; automated gates passed; manual 1920x1080 and real-provider acceptance remain separate.

## Stage result ledger

| Stage | State | Delivered evidence |
|---|---|---|
| 2B-0 bounded multiview evidence | Completed | Stable plane selection, six bounded silhouettes, <=64 host regions, exact package hash, no paths or per-voxel prompt payload. |
| 2B-1 two-round structure compiler | Completed | Two sequential required-tool calls, compact critic input, exact host-region validation, typed timeout/cancel/malformed/unsupported failures, no retry or third call. |
| 2B-2 constrained symmetry | Completed | Only confirmed `SymmetricCore` pairs may change; attachment/thin/uncertain/transition occupancy is preserved; GLB coverage selects add/remove direction; topology, cavity, volume, silhouette, support and roughness gates remain mandatory. |
| 2B-3 workspace orchestration | Completed | Local candidate generation performs zero provider calls; explicit AI action owns both rounds; GLB/source/project/model changes and cancellation reject stale publication; Direct/Refined survive all AI failures. |
| 2B-4 review UI | Completed | Added explicit `AI 识别结构`, `结构区`, fixed semantic overlay, compact region facts and review summary in the existing workspace only. |

## Verification matrix

| Gate | Result |
|---|---|
| Semantic evidence/executor focused tests | Passed, 20/20 with the related quality-refinement matrix |
| Semantic compiler tests | Passed, 5/5 |
| Affected IDE coordinator/ViewModel/viewport/XAML tests | Passed, 30/30 |
| Application full suite | Passed, 278/278 |
| IDE full suite | Passed, 2825/2825 |
| Debug IDE-only solution build | Passed, 0 errors; one pre-existing nullable warning in `BuiltInFieldRegistryPackLoaderTests.cs` |
| Real DeepSeek/Tencent calls | Not run by contract |
| Manual 1920x1080 visual/physical review | Pending user acceptance |
| IdeOnly clean source package | Passed; 1375 files in `artifacts/RA2IniEditor.IDE.SourceClean.zip` |

## Diff intent

- Application owns immutable derived evidence, semantic partition reconciliation and deterministic candidate execution.
- IDE owns the text-only two-round DeepSeek adapter, session orchestration and review projection.
- The viewport consumes a derived partition only for display; it cannot mutate voxel state.
- All new runtime contracts remain internal and nonserialized. No public API or persistence schema changed.
- The old local global-symmetry suggestion is no longer published by `生成候选`; the explicit semantic path is the only
  product route to a symmetry candidate.

## Frozen-boundary confirmation

- No real DeepSeek or Tencent request was made.
- No Shell, docking, menu, project Apply/Save, export, VXL/HVA writer, INI, Field Registry or legacy behavior changed.
- No new dependency or project-file edit was required.
- Direct/Refined generation remains local, read-only and usable when AI structure recognition fails.

## Remaining risks and next phase

- Physical review is still required on the user's real body/turret/barrel model to confirm that host region evidence gives
  DeepSeek enough information and that cyan/amber/blue/violet overlays are legible at 1920x1080.
- Model-provider quality has not been verified because real calls were explicitly prohibited.
- Colouring is still the existing coarse geometry-role pipeline. It does not yet use the 2B structural partition for
  material identity, glass, tyre, bare-metal, remap or accent recognition. Structural semantics must remain separate from
  a future material-semantic colouring package.

## 2026-08-28 physical-sample correction

- State: Completed / automated and physical product-path probe passed; rebuilt UI screenshot acceptance remains manual.
- Root cause: the original evidence builder emitted one region for every disconnected mismatch/protected component. The
  user sample exceeded 64 regions, so quality generation succeeded but `SymmetryEvidence` was null and AI recognition was
  disabled.
- Correction: exact coordinates are retained while model-facing mismatch components are grouped into deterministic
  side/height/depth/morphology buckets; protected components retain one exact coordinate union plus component count.
- Physical evidence: the real `H:\RA2\YR_Test\body-candidate.vox + mesh.glb` product path now creates bounded evidence,
  covers all 18,286 Refined coordinates exactly once and leaves no absolute-path probe in source.
- Regression evidence: fragmented synthetic evidence, determinism/path-free checks and constrained-symmetry tests pass
  6/6; affected IDE tests pass 27/27; Application full passes 279/279; IDE full passes 2825/2825; Debug build passes with
  0 errors and one pre-existing nullable test warning.
- Frozen boundaries: no real DeepSeek/Tencent call, Shell/XAML, Apply/Save, VXL/HVA, persistence, public API, INI, Field
  Registry or legacy change.

## 2026-08-28 visual-delta / structure-response correction

- State: Completed in code; automated gates and local real-sample geometry probe passed; another real-provider/manual
  screenshot acceptance remains required.
- Real local sample: SurfacePolish admitted on `body-candidate.vox + mesh.glb`; Direct 18,301, Refined 18,162,
  +14/-153 (167 real changes), low-support surface 76 -> 58. All existing hard safety gates remained active.
- Difference presentation: unchanged geometry is translucent grey; green/red are the only pre-AI geometry delta colours.
  Blue now appears only in the AI structure view as protected thin geometry and is not claimed to be symmetric.
- Provider compatibility: equivalent bounded tool JSON is normalized while evidence hash, plane, region identity and full
  coverage remain strict. Invented/missing regions continue to fail closed.
- Availability: evidence readiness enables the explicit AI action; missing configuration is reported on click instead of
  silently disabling the control.
- No real provider call was made by Codex during this correction. The user-observed malformed provider result motivated
  the compatibility matrix; final live acceptance remains manual.

## 2026-08-28 selection/diagnostic self-audit correction

- State: implemented; focused automation and local certified-sample probe passed; live provider/manual screenshot pending.
- Automatic selection now requires material roughness improvement and ranks roughness before cleanup counts. Safe but
  cleanup-only candidates remain visible and cannot replace Refined automatically.
- Candidate provenance now includes behavior kind and occupancy threshold. `组合审阅` exposes the three candidate facts.
- Difference no longer depends on an unused feature-protection mask.
- Provider-shape failures now preserve canonical reason codes through localized UI messages; duplicate aliases and
  incomplete region coverage have explicit tests and remain fail-closed.
- Certified local pair result: Conservative selected; roughness `1.552632 -> 1.542869`, low-support `76 -> 70`,
  `+24/-29`. SurfacePolish is review-only at `+14/-153`.
- Focused verification: Application 22/22; affected IDE 35/35; Debug build passed with the existing nullable warning.
- Full verification: Application 280/280; IDE 2830/2830; AssetHost 47/47.
- Clean package: `artifacts/RA2IniEditor.IDE.SourceClean.zip`, IdeOnly profile, 1375 files.
- No real DeepSeek/Tencent call, Shell, Apply/Save, VXL/HVA, persistence, public API, INI, Field Registry or legacy change.

## 2026-08-28 neutral repair-evidence correction

- State: implemented and automatically verified; live two-round provider/manual visual acceptance remains pending.
- Root cause: model-facing evidence called every unmatched region an attached/detail/bodylike mismatch while `core` already
  contained only mirror-matched cells. The two rounds protected the regions that actually required a symmetry decision,
  so the deterministic executor correctly found zero changeable core pairs.
- Correction: unmatched groups now use neutral `repair-*` identities and expose mirror-target GLB coverage/contact facts.
  The first round treats a supported hull continuation as core unless positive bounded evidence proves an accessory/thin
  feature; the second round verifies rather than manufacturing disagreement.
- Safety retained: exact evidence hash/plane/region coverage, two calls, confidence threshold, disagreement-to-uncertain,
  non-core preservation, transition seam and all topology/quality gates remain unchanged.
- Focused evidence: Application semantic tests 6/6; compiler/coordinator tests 20/20.
- Full evidence: Application 280/280; IDE 2830/2830; AssetHost 47/47; isolated solution build 0 errors and one pre-existing
  nullable warning. The running IDE locked standard Debug output, so changed IDE code was built/tested in an isolated
  repository output; standard-bin `--no-build` supplied the complete source-boundary regression.
- No real provider call, Shell/XAML, Apply/Save, VXL/HVA, persistence, public API, INI, Field Registry or legacy change.
