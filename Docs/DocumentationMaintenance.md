# Codex Documentation Maintenance Policy

## 1. Purpose

The documentation set separates durable requirements, current facts, future plans
and historical evidence. `Docs/README.md` is the only documentation entry point.

## 2. Document ownership

| Document | Role | Update rule |
|---|---|---|
| `AGENTS.md` | Stable project authorization and workflow rules | Only when stable rules explicitly change |
| `ProductVisionAndRequirements.md` | Accepted product goal and durable requirements | Only after explicit user requirement/decision changes |
| `CurrentCapabilities.md` | Implemented/partial/not-implemented capability matrix | Update at major verified capability or audit closure |
| `DevelopmentRoadmap.md` | Ordered route from current state to accepted goal | Update when stage direction or dependency order changes |
| `DecisionLog.md` | Why major boundaries were accepted/rejected/deferred | Append accepted/proposed decisions; never rewrite history |
| `Codex_CurrentPhase.md` | Concise latest status and next safe entry | Update when active stage or latest trusted evidence changes |
| `RA2IniEditor_IDE_Full_Codex_Context.md` | Compact continuation capsule | Update at governance flush points; keep concise |
| Contract / Stage Ledger | Exact scope and historical verification | Preserve; update only inside its active package |
| `FeatureOverview.md` / `UserGuide.md` | Current user-facing product behavior | Update only when behavior changes |
| `DeveloperNotes.md` | Current development structure and constraints | Update on workflow/architecture changes |
| `HandoffArchiveIndex.md` / `Archive/` | Historical discovery | Update when archival structure changes |

## 3. Authority and status rules

Use the reading order in `Docs/README.md`. Every status claim must be backed by source
facts, a Stage Ledger, a Verification Matrix or explicit user instruction.

Allowed status words:

- Completed / Verified
- Implemented / Acceptance Pending
- Partial
- Proposed
- Not Implemented
- Unknown / Pending Verification
- Failed / Blocked

Do not convert `NotRun` into Passed, a confirmed contract into implementation, or an
algorithm inside the WPF assembly into an external/headless capability.

## 4. History policy

- Historical handoffs, contracts and ledgers remain immutable evidence.
- Superseded accumulated CurrentPhase/Context snapshots live under `Docs/Archive/`.
- Do not copy full stage logs into current-state documents; link them.
- Do not treat an old “next phase” line as current authority.
- If code and documents conflict, record the conflict or `Unknown / Pending
  Verification`; do not silently rewrite history.

## 5. Phase-end documentation gate

At a package completion, failure stop, handoff or explicit documentation task:

1. Close or update the Stage Ledger/TaskReview.
2. Flush public API, debt and decision records if triggered.
3. Update `Codex_CurrentPhase.md`.
4. Update the compact Full Context only when continuation facts changed.
5. Update CurrentCapabilities only for a materially changed verified capability.
6. Update product docs only when user-visible behavior changed.
7. Report changed docs, unknowns, skipped verification and next safe entry.

## 6. Safety rules

Documentation maintenance must not modify runtime code, tests, project files, XAML,
build configuration, persistence formats or generated assets. A docs-only task uses a
DocsOnly verification profile: link/path/status consistency and final diff audit;
build/test/package remain NotRun unless the task explicitly requires them.
