---
name: ra2-reference-registration
description: Analyze RA2 object references, type-list registration, list ordering, and dependency closure. Use for missing definitions, registration lists, object discoverability, reference repair, or deciding whether a newly created Section is actually reachable by the game.
metadata:
  version: "1"
  ra2-domains: reference-registration
  ra2-modes: chat,work
---

# Reference and registration workflow

- Build an explicit directed graph from source section/key to referenced object ID and expected target kind. Do not infer target kind from a familiar-looking ID when schema evidence is absent.
- Distinguish implicit construction from explicit enumerations. Some object families require numbered type-list registration; others are discovered through active references.
- Preserve numbered list order and existing indices. Adding an entry must not renumber unrelated rows unless the user explicitly approves a normalization.
- Check missing, ambiguous and wrong-kind targets separately. A text match in comments or another file is not a resolved semantic target.
- A complete object/profile must state every required registration and reference edge. If the current-document capability cannot close a project/multi-file edge, return it as an unresolved dependency rather than claiming completion.
- Work mode may only author registrations covered by a typed profile and current-document snapshot; otherwise remain advisory.

