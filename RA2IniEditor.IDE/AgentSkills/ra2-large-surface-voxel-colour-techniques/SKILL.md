---
name: ra2-large-surface-voxel-colour-techniques
description: Propose evidence-bound colour roles and semantic bindings for a human-confirmed large RA2/YR naval or surface unit with quiet long-plane shading.
metadata:
  version: "2"
  ra2-domains: voxel-colour-large-surface
  ra2-modes: chat
---

# RA2 large-surface colouring techniques

## Preconditions and authority

- Use only after the Host confirms LargeSurface and supplies active palette facts plus semantic requirements. The manual
  opaque non-remap BodyBase and selected technique remain Host-local and are applied after the model proposal.
- Propose bounded roles and bindings only. Typed local policy owns numeric contrast, dual-surface resolution, legality and
  palette-index writes.
- Do not pretend deck/hull semantics exist when they were not supplied; v1 has no separate Deck/Hull MaterialRole.

## Long-plane hierarchy

- Preserve low-frequency grouping across long hulls and deck-like surfaces. Per-voxel speckle, repeated edge glitter and
  frequent alternating bands read as noise or damage.
- Separate the broad painted family from superstructure, weapon mounts, openings and underside/below-water structure using
  supplied semantics. Top-facing geometry is not automatically the brightest material.
- Keep large planes coherent while using sparse structural breaks to reveal bow/stern direction, deck boundaries and major
  height changes. A darker underside is a preference, not a hard substitute for local evidence.
- Accept the Host's TopPreferred decision for top/underside dual-surface cells; do not create order-dependent rules.
- Preserve bow/stern directional end cues separately from long hull sides. A visible hull-side cell with an exposed bottom
  face remains body-family colour; do not create a continuous underside stripe along the waterline or lower silhouette.

## Materials and remap

- Apply glass, bare metal, lights, openings and accents only through matching requirements. Keep Light and Accent distinct.
- Use remap sparingly on deliberate identification zones. Do not cover a full hull side merely because the palette permits it.
- Do not invent aircraft, deck equipment or waterline masks from geometry aggregates.
- Let the Host derive sparse one-cell emphasis along effective hull/superstructure/weapon/attachment and eligible material
  interfaces. Ignore RegionId-only segmentation, and never overwrite direct glass, metal, light, opening, accent or remap
  materials with a boundary colour.

## Review priorities

- Review long-plane rhythm, deck/superstructure readability where semantics exist, sparse detail survival, material bleed,
  low-frequency contrast and visual noise at game scale.
- Report missing deck/hull semantics as a limitation and keep the candidate review-bound rather than claiming completion.
- Do not claim naval locomotion, waterline behavior, normals, HVA, shadow or GameReady correctness.
