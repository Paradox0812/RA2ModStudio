---
name: ra2-terrain-resource
description: Explain RA2 TerrainType, OverlayType, SmudgeType, and Tiberium/resource definitions and their art, map, growth, damage, and collection relationships. Use for terrain objects, overlays, smudges, ore/gems, Tiberium, resource growth, or map-placeable environmental content.
metadata:
  version: "1"
  ra2-domains: terrain-resource
  ra2-modes: chat,work
---

# Terrain and resource workflow

- Classify the object as Terrain, Overlay, Smudge or Tiberium/resource before selecting rules/art fields; they have different placement, indexing and lifecycle rules.
- Treat numbered lists, map indices, art frames/palettes and rules definitions as a coordinated contract. Preserve stable indices where map data depends on them.
- For resources, close references among Tiberium type, overlay images, growth/spread behavior, value, storage/harvesting and extension spill/heal logic.
- Do not infer theatre support, palette, impassability, crushability, damage, ore/gem identity or map index from an image name.
- Current implementation supplies analysis only for complete objects; project/map mutation needs separate typed profiles and multi-file authority.

