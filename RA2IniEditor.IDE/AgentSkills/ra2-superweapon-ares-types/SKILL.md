---
name: ra2-superweapon-ares-types
description: Apply source-frozen Ares SuperWeapon type rules, especially UnitDelivery and GenericWarhead complete profiles.
metadata:
  version: "1"
  ra2-domains: superweapon
  ra2-modes: chat,work
---

# Ares SuperWeapon type profiles

## UnitDelivery

- Use `Type=UnitDelivery`.
- `Deliver.Types` is a comma-separated list of 1..16 unique existing TechnoType IDs.
- `Deliver.Owner` is one of `invoker`, `neutral`, `special`, or `civilian`; Ares documents `invoker` as the default, but typed Work proposals state it explicitly.
- Use an explicitly compatible `SW.AITargeting`; `ParaDrop` is the documented default. Use `None` only for an intentionally non-AI/manual design.
- Do not generate Techno definitions, art sections, or asset files merely because they are referenced.

## GenericWarhead

- Use `Type=GenericWarhead`, `SW.Damage`, and `SW.Warhead`.
- `SW.Warhead` must uniquely resolve to an existing Warhead for the v1 typed profile.
- `Offensive` is the documented AI targeting default. Use `None` only for an intentionally non-AI/manual design.
- Do not create an empty Warhead. Do not modify an existing Warhead's `CellSpread` or provider `DamageSelf` unless the request explicitly enters a broader reviewed plan.

## Shared closure

- Register the new ID exactly once in `[SuperWeaponTypes]`.
- Choose exactly one of an existing provider Building slot (`SuperWeapon`/`SuperWeapon2`) or `SW.AlwaysGranted=yes`.
- Provide explicit `UIName`, `Name`, `IsPowered`, `RechargeTime`, `Action`, `SidebarImage`, `ShowTimer`, and `DisableableFromShell` values.
- If type/provider/effect identity cannot be established from the captured project, return clarification rather than a partial Section.

Sources:

- https://ares-developers.github.io/Ares-docs/new/superweapons/types/unitdelivery.html
- https://ares-developers.github.io/Ares-docs/new/superweapons/types/genericwarhead.html
- https://ares-developers.github.io/Ares-docs/new/superweapons/targeting.html
- https://ares-developers.github.io/Ares-docs/new/superweapons/availability.html
