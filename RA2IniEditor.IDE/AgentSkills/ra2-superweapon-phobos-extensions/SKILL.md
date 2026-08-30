---
name: ra2-superweapon-phobos-extensions
description: Review Phobos additive SuperWeapon extensions without replacing the underlying YR/Ares type contract.
metadata:
  version: "1"
  ra2-domains: superweapon
  ra2-modes: chat,work
---

# Phobos SuperWeapon extensions

- Treat Phobos SuperWeapon features as additive extensions over the selected YR/Ares base type; never infer a base `Type` from one extension tag.
- Resolve the active Phobos version and exact requested extension before proposing fields. If version/type applicability is uncertain, return clarification.
- `LimboDelivery`, detonation/weapon launch, chained SuperWeapon launch, targeting filters, designators/inhibitors, costs, and one-time limits are separate behavior families. Do not combine them merely because they are all available.
- Close every new Section/reference and registration required by the chosen logic. Do not create asset files, game hooks, or runtime test claims.
- Phobos requests currently use the model-owned bounded project plan and require review; they are not covered by the two Ares typed-complete profiles.

Source:

- https://phobos.readthedocs.io/en/latest/New-or-Enhanced-Logics.html
