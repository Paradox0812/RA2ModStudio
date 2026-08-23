---
name: ra2-faction-authoring
description: Explain and design RA2/Ares Side, Country, and faction relationships, including lists, defaults, ownership, UI assets, loading screens, and AI selection. Use for countries, sides, houses, factions, or playable-side setup.
metadata:
  version: "1"
  ra2-domains: faction
  ra2-modes: chat,work
---

# Side and Country workflow

- Distinguish Side defaults, Country identity/playability, House instances, Owner lists and UI/art resources.
- A usable faction is a cross-file/reference closure: enumerations, Country/Side sections, ownership and prerequisite policy, starting objects, paradrop/default types, UI/loading assets, colors/EVA and AI selection weights may all participate.
- Preserve list order and indices where the engine assigns meaning. Never renumber an existing list casually.
- Do not infer an Allied/Soviet/Yuri base, country ID, color, EVA, flag, sidebar or starting units from a display name alone.
- Mark Ares-specific multi-side/country fields and asset size/palette constraints explicitly.
- Work mode remains unavailable until a typed faction profile can validate required lists and cross-file assets; do not produce a lone Country section as “complete faction”.

