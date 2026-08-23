---
name: ra2-sound-eva
description: Explain and plan RA2 sound, voice, EVA, theme, and object audio bindings across rules, soundmd, evamd, and extension fields. Use for sounds, voices, EVA announcements, themes, taunts, attack/select/death audio, or audio asset references.
metadata:
  version: "1"
  ra2-domains: sound-eva
  ra2-modes: chat,work
---

# Sound and EVA workflow

- Distinguish a rules field that references audio, a sound/voice/EVA registry entry, and the physical WAV/AUD/BAG asset.
- Close each reference across the appropriate INI file and report unresolved physical assets separately.
- Preserve existing list identities and country/side EVA mappings. Do not invent a voice set, language, taunt range or filename convention without context.
- Account for random variants, priorities, volume, range/visibility and extension-specific behavior only when supported by exact schema evidence.
- Work mode remains advisory until multi-file typed profiles and artifact validation are available; do not claim a sound is playable merely because an ID was added.

