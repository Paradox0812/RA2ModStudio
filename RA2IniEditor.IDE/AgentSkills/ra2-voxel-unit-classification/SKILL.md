---
name: ra2-voxel-unit-classification
description: Classify an evidence-bound RA2/YR voxel unit as ground, air, large surface, or unknown before any class-specific colouring plan is compiled.
metadata:
  version: "1"
  ra2-domains: voxel-unit-classification
  ra2-modes: chat
---

# RA2 voxel unit classification

## Authority boundary

- Return a proposal only. A human-confirmed or human-corrected class is required before a colouring Skill may be routed.
- Use only Host-provided bounded facts and cite their exact fact IDs. Do not invent coordinates, masks, palette entries, filenames, images, or hidden geometry.
- A filename or archive name is never sufficient evidence. Palette colour, faction colour, and current paint are not unit-class evidence.
- When evidence is conflicting or insufficient, return `Unknown` with Low confidence instead of forcing a ground/air answer.

## Class criteria

### Ground

Prefer Ground when several facts support a land-oriented body mass: hull-like volume, undercarriage or lower-body structure,
wheel/track semantics, turret/barrel relationships, or strong top/side/lower separation. A turret alone is not sufficient.

### Air

Prefer Air when several facts support a shallow planform: broad lateral span, wing/nacelle/control-surface semantics,
fuselage direction, canopy/intake/exhaust roles, or low vertical depth relative to length/span. Thin geometry alone is not sufficient.

### LargeSurface

Prefer LargeSurface for carrier/ship-like long planar bodies with deck/hull/superstructure evidence, sparse upper structures,
or naval semantic intent. Large size alone is not sufficient; a large tank remains Ground.

### Unknown

Use Unknown when facts support multiple classes, orientation is unresolved, semantic evidence is too sparse, or the model is
outside these three unit families.

## Confidence and response discipline

- High: multiple independent geometry and semantic facts agree, with no material contradiction.
- Medium: the class is better supported than alternatives but one important evidence family is absent.
- Low: the proposal depends on weak aggregates, has contradictions, or should remain Unknown.
- Cite only provided `FactId` values. Explain the strongest supporting and contradicting facts in one bounded reason.
- Do not choose a colouring Skill, base colour, technique template, palette index, or remap policy.

