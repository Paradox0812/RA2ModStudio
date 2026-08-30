---
name: ra2-field-schema-trust
description: Apply the IDE Field Registry as typed, source-aware evidence when explaining or authoring RA2 INI fields. Use across RA2 domains whenever field validity, value shape, context, source, or trust matters.
metadata:
  version: "2"
  ra2-domains: field-schema
  ra2-modes: chat,work
---

# Field schema and trust workflow

- Field Registry is source-aware evidence for field applicability, value shape, source and trust; it is not an exhaustive RA2 namespace, object-completeness template, gameplay balance table, or universal veto over model-owned project plans.
- Match both key and section kind. A same-named field in another section kind is not evidence that the field is valid here.
- Treat source-verified/manual-curated facts as strong and inferred facts as caution. Guardrail, wrong-context, obsolete, pseudo-field and non-existent rows block only typed current-document Profiles; in a generic rules/art project plan they are caution evidence that the model must review against source-backed domain knowledge.
- Preserve provider priority Project > Global > BuiltIn as captured in the request snapshot. Never mutate or relearn the registry from a generation request.
- Validate booleans, numbers, enums, references and lists using the effective schema. Keep model-chosen balance values visible in the Diff and never describe them as official defaults without evidence.
- For typed current-document Profiles, missing or blocked required evidence remains a local fail-closed condition. For the generic rules/art project tool, unknown, inferred, blocked, or obsolete Registry rows are advisory review evidence only: do not reject a source-backed model operation or replace an unknown key with a plausible synonym.
