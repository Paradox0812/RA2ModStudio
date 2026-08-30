# ASSET-VOX-2A-R2 Topology-Protected Refinement Final Contract

Status: approved for continuous execution (`R2-0` through `R2-4`).

## Goal

Replace destructive support-count cleanup with a deterministic, topology-aware refinement pipeline. Long barrels, antennae, thin plates and their attachment paths are safety-critical geometry. A candidate may improve large body surfaces, but it must not shorten, disconnect or silently reshape those structures.

## Authority and boundaries

- The loaded canonical VOX/VXL snapshot remains immutable and authoritative.
- GLB conversion and every refinement are derived, session-only review candidates.
- DeepSeek/Tencent may not choose cells, override gates or write assets in this phase.
- No project Apply/Save, VXL/HVA export, Shell change, persistence schema or third-party dependency is permitted.
- Existing preview and quality AutomationIds remain stable.

## R2-0 — baseline and fixtures

- Freeze fixtures for a body with a barrel, tapered rod, thin plate, antenna, isolated noise and asymmetric body detail.
- Preserve the rule that one isolated attached bump is not automatically protected.
- Record deterministic hashes and typed outcomes; a clean object may legitimately produce `NoSafeImprovement`.

## R2-1 — structure graph and zones

- Classify canonical cells as `Smoothable`, `Transition`, or `Frozen`.
- A sustained rod or plate component is frozen in full, including degree-one endpoints and its occupied attachment path.
- The occupied one-cell neighbourhood around frozen structures is a transition zone.
- Protection facts include frozen/transition cells, protected components, endpoints and branch cells.

## R2-2 — masked discrete distance refinement

- Remove the unconditional `face-neighbour <= 1` deletion rule.
- Build conservative and balanced candidates from the supersampled conversion.
- Apply deterministic discrete signed-distance filtering only outside frozen cells; transition cells are anchors in the first production profile.
- Reinsert frozen coordinates exactly before admission.

## R2-3 — hard admission gates

- Frozen coordinates, endpoints and protected paths must survive exactly.
- Component count, enclosed cavities, volume and six silhouettes may not exceed frozen limits.
- A candidate must show a measurable quality improvement; merely changing the hash is insufficient.
- Candidate selection is hard-gate plus Pareto/lexicographic selection, never a weighted score.
- If no candidate is safe and better, return typed `NoSafeImprovement` and retain the direct candidate.

## R2-4 — review projection

- Add an interactive 3D difference view: added green, removed red, protected unchanged blue, ordinary unchanged neutral.
- Show the admission outcome and structure-protection summary in the existing quality review surface.
- An unadmitted candidate cannot be selected for the working session.
- Add AutomationIds `VoxelStyle.Preview.Difference`, `VoxelStyle.Quality.Admission`, `VoxelStyle.Quality.StructureFacts`, and `VoxelStyle.Quality.DifferenceLegend`.

## Verification

1. Focused Application tests for structure protection, deterministic refinement, gates and no-safe-improvement.
2. Focused IDE tests for projection, scene difference colours, ViewModel selection rules and XAML contract.
3. IDE-only restore/build/test. Packaging is run only after the focused and full suites pass.

## Stop rules

- Stop success classification if a protected endpoint/path is missing, a new disconnected component/cavity appears, or the candidate has no measurable improvement.
- Do not weaken a gate to make a fixture pass. Return `NoSafeImprovement` instead.
