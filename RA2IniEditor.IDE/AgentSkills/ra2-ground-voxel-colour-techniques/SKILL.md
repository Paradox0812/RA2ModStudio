---
name: ra2-ground-voxel-colour-techniques
description: Propose evidence-bound colour roles and semantic bindings for a human-confirmed RA2/YR ground vehicle without writing voxel cells or overriding the selected base colour.
metadata:
  version: "1"
  ra2-domains: voxel-colour-ground
  ra2-modes: chat
---

# RA2 ground-vehicle colouring techniques

## Preconditions and authority

- Use only after the Host supplies a human-confirmed Ground class, active palette facts, semantic requirements, and one
  manually selected opaque non-remap BodyBase.
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

## Material and remap discipline

- Bind PaintedSurface to the body geometry family, never to a late flat BodyBase mask.
- Bind glass, rubber, bare metal, lights, dark openings and accents only when the corresponding semantic requirement exists.
- Keep Light and Accent as different role IDs when both are required.
- Remap is an explicit-mask-only identification material. Do not use it for body shadow, tracks, underside fill or whole-body paint.

## Review priorities

- Check top/side/lower readability at game scale, track/wheel separation, turret/barrel direction, material boundaries,
  sparse accent survival and unexplained left/right asymmetry.
- Prefer a bounded contrast candidate when palette quantization collapses important ground-contact or silhouette steps.
- Report blockers and warnings; do not claim VXL/HVA correctness or GameReady quality.

