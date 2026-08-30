# ASSET-VOX-3D Center-Seam Bridge Proposal — Final Contract

Date: 2026-08-29  
Status: implemented / automated verified  
Risk: R3 internal Agent tool and geometry-execution contract  
Governance: immediate single-stage closeout

## 1. Outcome

After a symmetric working candidate already contains occupied geometry on both sides of the selected X symmetry plane,
the Agent can explicitly bridge a one- or two-cell empty center seam. The result remains a session-only review candidate
and advances working geometry only through the existing explicit `用于本会话` action.

## 2. Evidence and ownership

- Application owns immutable `seam-gap-*` targets bound to the canonical source, coverage and selected-plane hashes.
- A target contains only exact Host-owned empty coordinates, occupied anchors and bounded aggregate evidence.
- One-cell targets exist only on an integer X plane; two-cell targets exist only on a half-cell X plane.
- Every target requires the immediate occupied anchor on both sides of the same Y/Z line.
- Connected gap patches remain separate while bounded; deterministic evidence buckets are used only when fragmentation
  would exceed the 24-target prompt limit. Coordinates are never truncated.

## 3. Agent operation

`bridge_center_gap` is valid only for `seam-gap-*` targets. `add_mirror` and `remove_source` remain valid only for occupied
region/component targets. Unknown, stale, overlapping or action/target-incompatible proposals fail before mutation.

The Host does not automatically fill a seam and does not infer arbitrary holes as seam targets. The primary/reviewer/
conditional-arbitrator flow remains unchanged; executable agreement still includes both target ID and action.

## 4. Execution and safety

- The executor adds exactly the selected missing seam coordinates and never removes occupied cells for this action.
- Added palette indices reuse the existing deterministic six-neighbour, then 26-neighbour, then dominant opaque fallback.
- Existing protection, connectivity, cavity, occupied-volume and six-view silhouette gates remain authoritative.
- A three-cell gap, off-axis hole, window, cavity or arbitrary internal void is not eligible merely because it is empty.

## 5. Frozen boundaries

No Shell/XAML/AutomationId, real provider call, Apply/Save, VOX writer transaction, VXL/HVA, persistence, public API,
snapshot schema, INI, Field Registry, provider transport or legacy change is authorized or implemented.

## 6. Acceptance

- Integer-plane one-cell and half-cell-plane two-cell seams are exposed and bridgeable.
- Three-cell and arbitrary interior holes are not promoted.
- Wrong action/target combinations fail closed.
- Tool parsing, two-pass agreement and deterministic execution preserve evidence identity and minimum safety.

