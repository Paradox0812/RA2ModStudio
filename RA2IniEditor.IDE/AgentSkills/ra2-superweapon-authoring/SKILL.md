---
name: ra2-superweapon-authoring
description: Design or review RA2/Ares/Phobos SuperWeapon definitions, provider buildings, targeting, charge, AI use, effects, and sidebar presentation. Use for super weapons, support powers, charge/drain logic, delivery, targeting, or superweapon cameos.
metadata:
  version: "2"
  ra2-domains: superweapon
  ra2-modes: chat,work
---

# SuperWeapon authoring core v2

- Start with the exact superweapon type and active engine extension. Type-specific fields and defaults must not be mixed across unrelated types.
- A usable superweapon may require the SuperWeapon section, a provider BuildingType binding, availability/charge rules, valid target rules, AI targeting, effect objects and sidebar/cameo assets.
- Evaluate range, designators/inhibitors, affected houses/targets, shroud behavior, deferment, costs, one-time limits and charge/drain as a coherent targeting and lifecycle policy.
- For damage delivery, close references to Weapon/Warhead/Animation/UnitDelivery content rather than assuming the superweapon section alone creates an effect.
- Do not select a country, side, building, effect asset or AI targeting mode without request/context evidence.
- Treat “超级武器”, “超武”, “支援技能”, “支援能力”, `SuperWeapon`, and `support power` as this domain.
- A complete object must close `[SuperWeaponTypes]` registration, one SuperWeapon Section, exactly one availability strategy (provider Building slot or explicit `SW.AlwaysGranted`), type-specific effect references, charge/display fields, `Action`, and compatible AI targeting.
- `Action` has no one-size-fits-all official value across every SuperWeapon type and UI behavior. Select it explicitly from the requested behavior and active engine documentation; never let an old Field Registry enum invent or veto a new object ID/value.
- Provider mode may modify only one uniquely resolved Building and only `SuperWeapon` or `SuperWeapon2`. AlwaysGranted mode must not fabricate a provider.
- Natural-language/display object names are not INI identities. Resolve them to canonical Section IDs from the captured rules document before proposing the profile; request bounded Section evidence for candidate IDs, and never invent an unverified provider/effect ID.
- UnitDelivery reuses existing TechnoTypes unless the user explicitly asks for a larger compound object. GenericWarhead v1 reuses an existing Warhead and must not create an empty Warhead or silently modify its `CellSpread`/`DamageSelf`.
- Unknown engine, type, provider, effect identity, or incompatible targeting is a clarification. Do not silently emit a skeleton when the user asked for a usable object.
- Typed profiles produce deterministic bounded Project Preview. Other explicit source-backed types may use the model-owned project plan and must be marked for review. Neither path applies or saves automatically.

## Sources

- Ares SuperWeapon overview/types: https://ares-developers.github.io/Ares-docs/new/superweapons/types/index.html
- Ares targeting: https://ares-developers.github.io/Ares-docs/new/superweapons/targeting.html
- Ares availability: https://ares-developers.github.io/Ares-docs/new/superweapons/availability.html
- ModEnc Action: https://modenc.renegadeprojects.com/Action:
