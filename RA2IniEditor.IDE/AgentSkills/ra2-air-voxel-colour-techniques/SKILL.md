---
name: ra2-air-voxel-colour-techniques
description: Propose evidence-bound colour roles and semantic bindings for a human-confirmed RA2/YR air unit while preserving shallow planform readability and the manual base colour.
metadata:
  version: "2"
  ra2-domains: voxel-colour-air
  ra2-modes: chat
---

# RA2 air-unit colouring techniques

## Preconditions and authority

- Use only after the Host confirms Air and supplies active palette facts plus semantic requirements. The manual opaque
  non-remap BodyBase and selected technique remain Host-local and are applied after the model proposal.
- Propose bounded colour roles and bindings only. The local policy catalog decides numeric offsets and thin-surface
  conflicts; the local colourizer alone writes palette indices.
- Never replace BodyBase, infer a canopy from brightness alone, expand masks, emit coordinates, or approve remap.

## Planform hierarchy

- Aircraft are shallow, surface-heavy forms. Prioritize nose/tail direction, wing roots, leading/trailing edges, paired
  wings or nacelles, and separation between fuselage, wings, engines and control surfaces.
- Use broad, quiet bands across wings and fuselage. Do not apply a tank-like vertical gradient across an entire wing and do
  not turn every leading edge into a bright one-voxel stripe.
- Treat underside as a distinct region whose value may be lighter or darker than the upper body, but must remain locally
  distinguishable. Never assume a universal darker underside.
- On cells representing both top and underside faces, accept the Host's BodyBase dual-surface decision; do not rely on rule order.
- Keep fuselage nose/tail end cues distinct from broad side surfaces without drawing a continuous dark band around the
  airframe. Thin side-plus-under cells remain in the body family unless they are semantically underside structure.

## Materials, symmetry and remap

- Canopy/glass, intakes, exhausts, engine openings, landing gear, weapon pylons and lights require explicit semantic roles.
- Preserve paired markings and material cues when trusted symmetry evidence exists; report rather than silently repair an
  unexplained asymmetry.
- Keep remap sparse on deliberate wing, tail, fuselage or insignia masks. Large remap fields must not erase airframe form.
- Let the Host emphasize only effective fuselage/wing/nacelle/canopy/pylon/tail or eligible material interfaces. Ignore
  RegionId-only seams. Boundary accent belongs on PaintedSurface and must not overwrite canopy, opening, metal, light,
  accent or remap cells.

## Review priorities

- Inspect multiple orthographic/isometric views and intended game scale for planform silhouette, wing-root continuity,
  canopy/opening recognition, underside separation and paired-feature balance.
- Prefer low-frequency form cues over decorative palette noise. Report palette collapse or lost small accents as warnings.
- Do not claim flight animation, normals, HVA, shadow, pivot or GameReady correctness.
