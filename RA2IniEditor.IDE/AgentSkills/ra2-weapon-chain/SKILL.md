---
name: ra2-weapon-chain
description: Build or review RA2 Weapon to Projectile to Warhead reference chains and bind them to an existing TechnoType weapon slot. Use for weapons, coaxial guns, primary/secondary armaments, or explicit weapon-chain requests.
metadata:
  version: "1"
  ra2-domains: weapon-chain
  ra2-modes: chat,work
---

# Weapon-chain workflow

- The minimum closed direct-fire chain is host `Primary` or `Secondary` -> Weapon `Projectile` and `Warhead` -> corresponding Projectile and Warhead sections.
- A complete direct-fire profile also needs visible gameplay values: Weapon Damage/ROF/Range/Speed; Projectile Inviso/Image/AA/AG; Warhead Verses/InfDeath/CellSpread/PercentAtMax.
- `AA` and `AG` belong to Projectile, not Weapon or Unit. `Verses`, `InfDeath`, `CellSpread`, and `PercentAtMax` belong to Warhead.
- Select Primary versus Secondary from explicit intent and existing armament context. Do not overwrite an occupied slot silently; expose the replacement in the Diff.
- Use skeleton output only when the user explicitly asks for a skeleton, scaffold, placeholder, empty structure, or framework. “Build/create a usable weapon chain” means complete profile.
- Keep generated IDs unique, references closed and all operations atomic. Missing IDs or required gameplay values require clarification; failure must yield no partial plan.
- This profile does not imply sounds, muzzle animations, elite variants, Burst, custom trajectory, registration lists or assets unless separately requested and supported.

