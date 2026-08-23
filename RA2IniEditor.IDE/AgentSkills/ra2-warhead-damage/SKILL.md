---
name: ra2-warhead-damage
description: Explain and design RA2 Warhead damage, armor multipliers, area falloff, targeting consequences, and Ares/Phobos special effects. Use for Verses, armor interaction, CellSpread, PercentAtMax, InfDeath, immunities, or special warheads.
metadata:
  version: "1"
  ra2-domains: warhead-damage
  ra2-modes: chat,work
---

# Warhead and damage workflow

- Treat Weapon Damage as the base amount and Warhead as target response, area/falloff and special-effect logic.
- `Verses` ordering must match the active ArmorTypes catalog. Do not assume the vanilla 11-slot order when custom armor types or extensions are present.
- `CellSpread` controls radius and `PercentAtMax` controls edge falloff; evaluate them together. A zero or near-zero value may radically change behavior.
- Verses percentages also affect whether targets may be acquired, force-fired upon, or retaliated against; Ares special suffixes can decouple some of these decisions.
- Keep InfDeath and animation/effect choices separate from damage math. Do not claim a special effect works without its required owner, target, animation, weapon, or extension setting.
- In Work mode, only author fields confirmed for Warhead context and preserve all existing armor slots unless the request explicitly replaces them.

