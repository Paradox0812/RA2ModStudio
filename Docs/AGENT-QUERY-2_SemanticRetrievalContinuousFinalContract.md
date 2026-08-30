# AGENT-QUERY-2 Semantic Retrieval Continuous Final Contract

Status: approved for continuous implementation by the user on 2026-08-25.

## 1. Goal

Replace the embedded Work Agent's single exact-query hop with a bounded semantic retrieval loop that can:

1. search captured INI snapshots by canonical Section ID and common local aliases such as `Name` / `UIName`;
2. bind natural-language entities to canonical Section identities with explicit evidence;
3. collect capability-specific facts before structured execution;
4. stop deterministically without unbounded model calls or repeated evidence;
5. keep provider output advisory until the existing local preview/apply gates accept it.

This package fixes the usability gap where a user must manually provide IDs such as provider building, delivered unit, or existing Warhead even though those objects are already present in the captured project.

## 2. Scope

### Allowed

- IDE-internal AI query contracts, request-lifetime retrieval models, pipeline orchestration and prompt formatting.
- Reuse of the existing semantic model over immutable captured snapshots.
- IDE tests and project documentation.

### Forbidden

- Public Automation/HLI API changes.
- Parser, Field Registry data/provider priority, diagnostics, completion, save-preflight, undo/redo or legacy behavior changes.
- New file-system, shell, network, apply or save authority.
- Hard-coded project entity IDs or case-specific production shortcuts.
- XAML changes without a separately reviewed exact UI contract.

## 3. Architecture and data ownership

- `Ra2AiContextQueryExecutor` remains the only Host entry for model-requested read-only facts.
- A request-lifetime semantic index is built lazily from the already captured `current`, `rules`, or `art` snapshot using `Ra2DocumentSemanticModelBuilder`.
- Search identity is `(symbolic target, canonical Section ID)`.
- Search aliases are bounded to canonical ID, `Name`, `UIName`, and the semantic model's short Section display note. INI values remain untrusted data.
- A resolved entity binding contains role, target, canonical Section ID, Section kind, matched alias and match basis.
- Retrieval attempts, query fingerprints, stop reason and model-call count are immutable request results. They are not persisted.

## 4. Query contract

Existing query kinds remain compatible:

- `get_section`
- `resolve_reference`

New bounded query kind:

- `search_objects`
  - `target`: `current`, `rules`, or `art`
  - `search_text`: non-empty local alias/ID candidate
  - `entity_role`: optional semantic role such as `provider-building` or `delivery-type`
  - `accepted_kinds`: optional Section-kind allow-list
  - `maximum_results`: 1..8

The initial analysis schema accepts the extended shape. The parser continues to accept the old exact shape so existing fake clients and recorded tests remain valid.

## 5. Bounded retrieval loop

The Work request is limited to:

1. one intent-analysis call;
2. zero to two compact retrieval-refinement calls;
3. one structured execution call;
4. the existing optional single structured-repair call.

Maximum provider calls for one request: five.

The loop stops when any condition is true:

- all requested entity roles have exactly one high-confidence binding and no query failed;
- the refinement stage reports ready or needs clarification;
- no new query fingerprint or evidence is produced;
- two refinement rounds are exhausted;
- cancellation or provider failure occurs.

No retry is performed for transport errors. No provider or model fallback is allowed.

## 6. Capability evidence packs

For project SuperWeapon authoring, the Host augments model queries with bounded, deterministic evidence:

- `[SuperWeaponTypes]` registration Section when present;
- exact Sections for uniquely resolved provider, delivered object, Warhead, or existing SuperWeapon bindings;
- only the fields already present in the captured snapshot, under the existing evidence character budget.

Evidence packs do not invent defaults, approve values, or block provider proposals. Local preview validation remains the final structural gate.

## 7. Prompt and token policy

- Refinement calls receive a compact prompt: original request summary, intent facts, Skill manifest summaries, project target metadata and accumulated query facts.
- Full Skill bodies are reserved for the execution call.
- Project execution omits unrelated caret selection, nearby text, diagnostics and Field Registry evidence unless the route is current-document scoped.
- Canonical entity bindings and Host facts are protected ahead of low-priority conversation/context sections during prompt truncation.
- Stable ordering and deduplication are mandatory.

## 8. Verification gates

Required tests:

- old `get_section` / `resolve_reference` payloads still parse and execute;
- `search_objects` resolves ID, `Name` and `UIName` aliases with deterministic ordering;
- Section-kind filters reject mismatched objects;
- ambiguous search remains ambiguous and never silently binds;
- the loop performs at most two refinement calls and stops on repeated/no-progress queries;
- typed SuperWeapon flow receives registration and canonical entity facts;
- project execution prompt excludes unrelated editor context and retains Host facts;
- no apply/save/path authority is added;
- representative Chinese natural-language Work requests reach structured execution without manually supplying already discoverable local IDs.

Final verification is one Release build, full Application tests, full IDE tests and one clean IDE-only package check. Real DeepSeek and UI smoke are manual follow-up checks.

## 9. Continuous stages

| Stage | Deliverable | Stop rule |
|---|---|---|
| QUERY-2A | extended query/result contracts and semantic object search | compatibility or deterministic-search tests fail |
| QUERY-2B | bounded refinement protocol and pipeline loop | call cap/no-progress/cancellation tests fail |
| ENTITY-1 | canonical entity bindings and capability evidence packs | ambiguous entity is silently selected |
| CONTEXT-4 | project prompt compaction and fact-priority truncation | required execution facts are lost |
| EVAL-1 | natural-language regression and cost/call-count matrix | representative scenario cannot reach execution |
| TRACE-1 | backend trace DTO and exact UI contract only | XAML approval is absent |

## 10. Deferred UI contract boundary

This package may expose backend trace facts but must not edit XAML. A later exact UI contract will specify a compact, collapsed-by-default execution trace with no new model authority and no raw hidden prompt/response exposure.
