---
name: ra2-ares-phobos-extensions
description: Apply version-aware Ares and Phobos compatibility rules to RA2 INI designs. Use when a request explicitly mentions Ares, Phobos, extension-only fields, trajectories, shields, attach effects, laser trails, digital displays, radiation, or other extension systems.
metadata:
  version: "1"
  ra2-domains: ares-phobos
  ra2-modes: chat,work
---

# Ares and Phobos compatibility workflow

- Establish the target runtime and version before using extension fields. Vanilla, Ares and Phobos availability are not interchangeable.
- Prefer the active extension's documented replacement when it overrides or conflicts with older engine/Ares behavior; state the precedence instead of writing both forms.
- Keep extension systems in their actual owning section kinds: AttachEffect, Shield, LaserTrail, DigitalDisplay, Radiation and trajectory fields must not leak into unrelated contexts.
- Treat version labels, deprecations, renamed keys, incompatibilities and required companion fields as authoring constraints.
- Never infer support merely because a key exists in a broad or wrong-context row. Require exact captured Field Registry evidence and respect blocked/obsolete guardrails.
- Do not silently upgrade a vanilla request into Ares/Phobos-only content. Explain the dependency and obtain a clear target before proposing extension-only changes.

