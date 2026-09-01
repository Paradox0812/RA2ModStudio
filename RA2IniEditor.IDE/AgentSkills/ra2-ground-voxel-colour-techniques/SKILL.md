---
name: ra2-ground-voxel-colour-techniques
description: Propose evidence-bound colour roles and semantic bindings for a human-confirmed RA2/YR ground vehicle without writing voxel cells or overriding the selected base colour.
metadata:
  version: "3"
  ra2-domains: voxel-colour-ground
  ra2-modes: chat
---

# RA2 ground-vehicle colouring techniques

## Preconditions and authority

- Use only after the Host supplies a human-confirmed Ground class, active palette facts and semantic requirements. The manual opaque
  non-remap BodyBase and selected technique remain Host-local; they are applied after the model proposal and may not appear
  in the provider request.
- Propose bounded colour roles and bindings only. Local validators own palette legality, numeric technique policy, mask
  membership, rule ordering, quality admission, and actual palette-index writes.
- Never replace BodyBase, expand a semantic mask, approve remap, move geometry, or emit cell coordinates.

## Form hierarchy

- Preserve a readable body mass: exposed upper armour, vertical/oblique sides, lower hull, undercarriage, and recesses should
  form coherent steps within the same painted family.
- Treat the underside/lower structure as a strong ground contact cue. Tracks, tyres, suspension gaps, wheel wells and deep
  lower openings require explicit dark or neutral material roles, not a black outline around the whole silhouette.
- Keep hull, turret and painted equipment in compatible families unless semantic evidence declares a different material.
  Use value separation to reveal turret rings, mantlets, barrels, launchers, radar dishes and attachments.
- Large armour planes should stay quiet. Reserve ridge light for silhouette breaks and major plane changes; avoid one-voxel
  glitter, random wear, checkerboard shading and uniform edge outlining.
- Distinguish front/rear only from the human ForwardDirection. Keep long lateral side fields coherent, use upper bevel and
  shoulder bands to explain armour volume, and reserve confirmed end cues for front-quarter recognition. Unknown direction
  keeps both ends review-bound.
- Never darken a visible lateral hull cell merely because its bottom face is exposed. True undercarriage may use Underside;
  side-plus-under cells remain body-family surfaces.
- Treat wheel wells, true openings and ground-contact shadows as intentional dark boundaries; smooth side armour and panel
  partitions are not dark-opening evidence. Compress unsupported micro components before colour assignment.

## Material and remap discipline

- Bind PaintedSurface to the body geometry family, never to a late flat BodyBase mask.
- Bind glass, rubber, bare metal, lights, dark openings and accents only when the corresponding semantic requirement exists.
- Keep Light and Accent as different role IDs when both are required.
- Remap is an explicit-mask-only identification material. Do not use it for body shadow, tracks, underside fill or whole-body paint.
- Let the Host derive one-cell emphasis along effective hull/turret/barrel/attachment and eligible material interfaces.
  RegionId-only partition seams are not boundaries. Paint the emphasis on the PaintedSurface owner only; never overwrite
  rubber, glass, metal, lights, openings, accents or remap.

## Review priorities

- Check top/side/lower readability at game scale, track/wheel separation, turret/barrel direction, material boundaries,
  sparse accent survival and unexplained left/right asymmetry.
- Prefer a bounded contrast candidate when palette quantization collapses important ground-contact or silhouette steps.
- Report blockers and warnings; do not claim VXL/HVA correctness or GameReady quality.
- Review eight fixed views for confirmed-front recognition, uninterrupted side-field bands, sparse structural seams,
  accent budgets and subpixel-risk loss. Missing normals or unevaluated VPL remain explicit review findings.
