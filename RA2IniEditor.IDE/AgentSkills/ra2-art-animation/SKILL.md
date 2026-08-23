---
name: ra2-art-animation
description: Explain and plan RA2 artmd.ini object bindings, animations, SHP/VXL image references, palettes, sequences, cameos, debris, and animation damage. Use for art entries, animations, voxels, SHP, cameos, icons, or visual bindings.
metadata:
  version: "1"
  ra2-domains: art-animation
  ra2-modes: chat,work
---

# Art and animation workflow

- Separate rules object ID, `Image` reference, art section ID and physical asset filename. They may match by convention but are not the same authority.
- Classify output as voxel/HVA, SHP animation, infantry sequence, building art, projectile image, animation, cameo/icon or UI asset before choosing fields and palette.
- Close runtime references from rulesmd.ini/artmd.ini to assets and secondary animations. Report missing files, palette dependencies, facings/frames and theatre variants.
- Animation damage can use Warhead or Weapon logic with different ownership/registration caveats; verify the selected path and register referenced weapons where required.
- Do not claim a generated VOX/SHP/PNG has been converted or exists unless an asset tool produced a verified artifact.
- This skill currently supplies planning and INI binding knowledge only. Actual icon, VOX/VXL/HVA and SHP generation requires future host capability plugins plus artifact preview.

