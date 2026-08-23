---
name: ra2-particle-radiation
description: Explain and design RA2/Ares/Phobos Particle, ParticleSystem, radiation, smoke, gas, and damage-particle relationships. Use for particle systems, radiation, smoke trails, gas clouds, fire particles, or related visual/damage logic.
metadata:
  version: "1"
  ra2-domains: particle-radiation
  ra2-modes: chat,work
---

# Particle and radiation workflow

- Separate Particle visual/damage behavior, ParticleSystem spawning/holding behavior, Weapon/Warhead emission, and extension-specific radiation systems.
- Close every ParticleSystem -> Particle and source Weapon/Techno/Animation -> ParticleSystem reference before calling the effect usable.
- Evaluate lifetime, spawn cadence, velocity, color/image, damage, warhead and ownership together; visual output and damage delivery are not the same path.
- Mark Ares/Phobos fields and version requirements. Do not mix legacy radiation, gas/smoke particles and extension radiation fields as if they were aliases.
- Work mode remains unavailable for complete particle/radiation objects until typed profiles validate references and timing/value domains.

