# AGENT-QUERY-2 Stage Ledger

## QUERY-2A — Semantic object search

- Status: Completed / automated verified.
- Added IDE-internal `search_objects` query with symbolic target, entity role, Section-kind filter and bounded results.
- Reused `Ra2DocumentSemanticModelBuilder` over the immutable captured snapshot.
- Exact canonical ID, `Name`, `UIName`, and short Section comments are searchable; matching is deterministic and does not use hard-coded game object IDs.
- Old `get_section` / `resolve_reference` payloads remain accepted.
- Gate: related Release tests passed.

## QUERY-2B — Bounded retrieval refinement

- Status: Completed / automated verified.
- Added at most two compact non-streaming refinement calls between intent analysis and structured execution.
- Repeated fingerprints stop as `NoProgress`; two rounds stop as `RoundLimit`; transport/provider failures are not retried.
- Refinement receives Skill summaries, not full Skill bodies, and cannot emit edits.
- Gate: AI Release regression passed.

## ENTITY-1 — Canonical bindings and evidence packs

- Status: Completed / automated verified.
- Unique exact search results and role-tagged exact Section queries produce request-lifetime canonical bindings.
- Equal-score ambiguous aliases never bind silently.
- SuperWeapon project capabilities add captured `[SuperWeaponTypes]` and exact resolved entity Sections before execution.
- No public HLI, persistence, Field Registry or Apply/Save authority changed.

## CONTEXT-4 — Project prompt compaction

- Status: Completed / automated verified.
- Project Work execution retains user request, validated intent, active Skills, project projection, canonical bindings and Host facts.
- Unrelated caret-local selected text, nearby text, Field Registry evidence, diagnostics and current-IDE metadata are omitted for project routes.
- Query facts are truncated after lower-priority conversation/local context.

## EVAL-1 — Natural-language and cost gates

- Status: Completed / automated verified.
- Representative Chinese UnitDelivery request resolves provider building and delivered Infantry/Vehicle from local aliases before producing the existing structured Project Proposal.
- Regression covers legacy query compatibility, deterministic search, ambiguity, no-progress, two-round cap, capability evidence and project prompt compaction.
- Latest AI-scope Release gate before final full verification: 466/466 passed.

## Final verification

- `dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore`: passed, 1 existing nullable test warning / 0 errors.
- `RA2IniEditor.Application.Tests` Release `--no-build`: 198/198 passed.
- `RA2IniEditor.Tests` Release `--no-build`: 2740/2740 passed.
- `package-source-clean.ps1 -Profile IdeOnly`: passed; 1245 files; package excludes `.git/.vs/bin/obj/artifacts/TestResults` and archive patterns.
- `git diff --check`: passed; only existing line-ending normalization warnings were reported.
- Real DeepSeek, physical WPF and game-runtime smoke: NotRun; manual acceptance required.

## TRACE-1 — Compact UI summary

- Status: Contracted / not implemented.
- Backend exposes stop reason, refinement count, canonical binding count, query result count and bounded prompt-character counts without retaining raw refinement prompts/responses.
- XAML/Shell presentation awaits explicit approval under the strict UI workflow.

## Deferred risks

- Localized CSF display text is not part of the current captured semantic model; identities absent from Section ID/`Name`/`UIName` still depend on model inference or clarification.
- Real DeepSeek adherence to the additive query schema and physical WPF behavior remain manual acceptance items.
