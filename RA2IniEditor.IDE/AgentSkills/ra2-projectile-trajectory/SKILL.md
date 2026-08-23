---
name: ra2-projectile-trajectory
description: Explain and design RA2, Ares, or Phobos projectile behavior and trajectory configuration. Use for projectile movement, targeting, air/ground eligibility, missiles, arcing shots, straight/bombard/parabola trajectories, airburst, or splits.
metadata:
  version: "1"
  ra2-domains: projectile-trajectory
  ra2-modes: chat,work
---

# Projectile trajectory workflow

- Separate the Weapon's speed/range/damage from the Projectile's movement, image, targeting flags and extension trajectory fields.
- In vanilla logic, combinations such as Inviso, Arcing, Vertical and ROT-based movement have distinct semantics; do not combine them casually.
- For Phobos custom trajectories, select exactly one trajectory family and follow its documented required/optional fields. Do not combine `Trajectory` with original Arcing, ROT, Vertical or Inviso logic unless the active extension documentation explicitly permits it.
- Evaluate AA, AG, land/naval targeting, subject-to-ground, detonation distance, pass-through, proximity, airburst/splits and shrapnel as interacting constraints rather than isolated switches.
- Never copy every trajectory option into one object. Start from intended visible behavior, choose one coherent family, then add only supported modifiers.
- Verify all extension fields against the captured Field Registry and identify the required Phobos/Ares version in explanations.

