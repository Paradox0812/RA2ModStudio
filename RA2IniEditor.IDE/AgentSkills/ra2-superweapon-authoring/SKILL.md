---
name: ra2-superweapon-authoring
description: Design or review RA2/Ares/Phobos SuperWeapon definitions, provider buildings, targeting, charge, AI use, effects, and sidebar presentation. Use for super weapons, support powers, charge/drain logic, delivery, targeting, or superweapon cameos.
metadata:
  version: "1"
  ra2-domains: superweapon
  ra2-modes: chat,work
---

# SuperWeapon workflow

- Start with the exact superweapon type and active engine extension. Type-specific fields and defaults must not be mixed across unrelated types.
- A usable superweapon may require the SuperWeapon section, a provider BuildingType binding, availability/charge rules, valid target rules, AI targeting, effect objects and sidebar/cameo assets.
- Evaluate range, designators/inhibitors, affected houses/targets, shroud behavior, deferment, costs, one-time limits and charge/drain as a coherent targeting and lifecycle policy.
- For damage delivery, close references to Weapon/Warhead/Animation/UnitDelivery content rather than assuming the superweapon section alone creates an effect.
- Do not select a country, side, building, effect asset or AI targeting mode without request/context evidence.
- Work mode must refuse complete authoring until a type-specific profile exists; Chat mode may provide a version-labeled design and dependency checklist.

