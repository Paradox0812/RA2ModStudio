---
name: ra2-ini-document
description: Analyze and explain RA2/YR/Ares/Phobos INI document structure, sections, duplicate keys, references, and current-document changes. Use for general INI questions that do not belong to a more specific object family.
metadata:
  version: "1"
  ra2-domains: ini-document
  ra2-modes: chat,work
---

# RA2 INI document workflow

- Treat section IDs, field keys, values, comments, and registration lists as different concepts.
- Preserve comments, unrelated keys, ordering, casing, and duplicate-section facts unless the requested operation explicitly changes them.
- Resolve the current section and effective value from the captured document; do not infer that a draft from chat already exists in the file.
- Before proposing an edit, identify the exact target section, key, intended value, and referenced object IDs. If an identity is ambiguous, request clarification.
- Prefer a minimal structured change. Never return a whole replacement file when bounded field or section operations are sufficient.
- Creating an object may also require a registration entry or another referenced definition. Report that closure explicitly; never claim closure when the active capability cannot create it.
- Chat mode may explain or draft. Work mode must use only the declared preview tool and must not claim apply or save.

