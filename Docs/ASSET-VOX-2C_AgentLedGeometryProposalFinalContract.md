# ASSET-VOX-2C Agent-Led Geometry Proposal — Final Contract

Status: final / user-authorized for continuous execution  
Risk: R3 internal orchestration boundary; R2 internal DTO additions  
Governance: deferred per stage, flush at 2C-5

## 1. Outcome

The explicit structure-recognition action produces an Agent-owned sparse geometry proposal. The Host deterministically
expands that proposal against the captured voxel evidence, applies only the requested operation, validates minimum safety,
and publishes a session-only 3D review candidate.

## 2. Ownership

- Application owns immutable evidence, evidence-detail slices, proposal DTO validation, target resolution, deterministic
  coordinate expansion, minimum safety checks and derived candidate snapshots.
- IDE owns prompts, provider calls, one optional evidence request, review, conditional arbitration, cancellation and
  stale-result protection.
- UI owns presentation only and reuses the existing structure/difference/candidate views.

## 3. Agent proposal contract

A proposal is bound to the exact evidence hash and selected symmetry plane. It contains 1..64 unique sparse operations.
Each operation references a Host-known aggregate region or a deterministic component target and chooses exactly one:

- `add_mirror`: preserve the selected occupied source cells and add their missing mirrored counterparts;
- `remove_source`: remove only selected occupied cells whose mirrored counterparts are absent.

Omitted targets are preserved. Unknown targets, duplicate/overlapping targets, stale hashes and invalid action values are
rejected. Presentation metadata and unknown additive JSON fields do not affect admission.

## 4. Evidence request

The primary analysis may request details once for at most eight known aggregate regions. The Host returns bounded,
coordinate-free component facts with stable target IDs, counts, bounds, mirror/coverage/contact/protection facts and a
slice hash. It does not return paths, raw coordinates, colours or a semantic conclusion. A repeated/no-progress query is
rejected; no second evidence-query round is allowed.

## 5. Review and arbitration

1. Primary analysis returns a proposal, optionally after the single evidence request.
2. Reviewer receives the same evidence plus the normalized primary proposal and returns its own proposal.
3. Agreement compares the sorted executable fingerprint `(target_id, action)` only. Reason text, order and confidence do
   not trigger disagreement.
4. On fingerprint disagreement, a third analysis receives both normalized proposals and returns the sole final proposal.
5. Direct agreement uses two analysis calls; disagreement uses three; one evidence request increases either path by one.
   Absolute ceiling: four provider calls. No hidden retry, model/provider switch or automatic weakening is allowed.

## 6. Minimum Host safety

The Host enforces only:

- exact current snapshot/evidence/coverage identity;
- known bounded targets and in-grid mirrored coordinates;
- no overlapping or conflicting operations;
- no removal of frozen/transition protected coordinates;
- no new connectivity break or enclosed cavity;
- existing maximum occupied-volume and six-view silhouette deltas;
- at least one real geometry change before publishing.

The Host must not require complete region classification, two-round label agreement, `SymmetricCore` status, coverage
threshold direction selection, roughness improvement or low-support improvement. It must never substitute its own action.

## 7. Review projection

The existing structure view projects final operations: mirror-add targets as planned symmetry repair, remove targets as
planned removal, protected geometry as protected, and omitted regions as preserved/unselected. The existing candidate and
Difference view show the actual added/removed cells. No new XAML, AutomationId or layout is required.

## 8. Failure behavior

Malformed tool output, transport failure, timeout, cancellation, clarification, unsupported geometry, stale evidence,
invalid query/proposal, failed arbitration or failed minimum safety produces no candidate and no write. The existing
Direct/Refined candidates remain available.

## 9. Frozen boundaries

No real provider call, Shell/layout, XAML, Apply/Save/export, VXL/HVA, public API, persistence, INI/Field Registry,
provider protocol or legacy change.

## 10. Stages

- 2C-0: code-fact audit and final contract.
- 2C-1: sparse proposal and bounded evidence-detail model.
- 2C-2: primary/review/conditional-arbitration compiler.
- 2C-3: deterministic executor and minimum safety.
- 2C-4: coordinator and existing review projection integration.
- 2C-5: focused/full verification, documentation and clean package.

## 11. Acceptance

- Direct proposal, one evidence request, agreement and arbitration paths are deterministic and bounded.
- Sparse omission preserves geometry.
- Agent-selected add/remove direction is not changed by Host heuristics.
- Stale/unknown/overlapping/protected-destructive proposals fail without candidate.
- A valid final proposal creates an actual 3D-reviewable candidate and difference.
- All frozen boundaries and full regressions remain intact.
