---
name: ra2-ai-programming
description: Design or review RA2 AI task groups and trigger chains across TaskForce, ScriptType, TeamType, and AITriggerType. Use for AI teams, task forces, scripts, triggers, weights, conditions, or production behavior.
metadata:
  version: "1"
  ra2-domains: ai-programming
  ra2-modes: chat,work
---

# AI programming workflow

- Treat a usable AI behavior as a four-object reference closure: TaskForce defines composition, ScriptType defines ordered actions, TeamType binds the two with ownership/behavior, and AITriggerType decides when and how often the team is created.
- Keep numbered tuple syntax and field ordering exact for each AI object family; these are schemas, not ordinary key/value bags.
- Verify every referenced unit, TaskForce, ScriptType, TeamType, country/house and target category. A Section that is not referenced by the next layer is not a usable AI chain.
- Explain trigger conditions, weights, difficulty masks, limits and autocreate/reinforcement behavior separately. Do not invent numeric tuple positions from memory when local evidence is absent.
- For an existing chain, preserve IDs and unrelated tuple entries. For a new chain, choose stable unique IDs and show the complete reference graph.
- Work mode must remain unavailable until a typed AI profile can validate tuple arity and reference closure; do not fake AI completion with empty sections.

