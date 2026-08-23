---
name: ra2-field-schema-trust
description: Apply the IDE Field Registry as typed, source-aware evidence when explaining or authoring RA2 INI fields. Use across RA2 domains whenever field validity, value shape, context, source, or trust matters.
metadata:
  version: "1"
  ra2-domains: field-schema
  ra2-modes: chat,work
---

# Field schema and trust workflow

- Field Registry is the authority for field applicability, value shape, source and trust; it is not an object-completeness template or a gameplay balance table.
- Match both key and section kind. A same-named field in another section kind is not evidence that the field is valid here.
- Treat source-verified/manual-curated facts as strong; treat inferred facts as caution; treat guardrail, wrong-context, obsolete, pseudo-field and non-existent rows as blocked for authoring.
- Preserve provider priority Project > Global > BuiltIn as captured in the request snapshot. Never mutate or relearn the registry from a generation request.
- Validate booleans, numbers, enums, references and lists using the effective schema. Keep model-chosen balance values visible in the Diff and never describe them as official defaults without evidence.
- If required evidence is missing or blocked, fail closed or request clarification. Do not replace an unknown field with a plausible-looking synonym.

