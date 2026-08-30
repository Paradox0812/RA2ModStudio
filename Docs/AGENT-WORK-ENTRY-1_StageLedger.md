# AGENT-WORK-ENTRY-1 Stage Ledger

Date: 2026-08-26

Status: Implemented / automated verified / real provider and GUI manual test not run

| Stage | Goal | Files touched | Verification | State after stage | Next entry satisfied |
|---|---|---|---|---|---|
| W1-1 | Audit the actual Work gates and prior evidence | Contract, pipeline, intent/retrieval sources and tests | Code-fact trace completed | Strict descriptive metadata and swallowed diagnostics confirmed | Yes |
| W1-2 | Separate fatal authority checks from recoverable model shape | Intent analysis stage and pipeline | Focused parser/pipeline tests | Typed fatal result; additive/missing metadata normalized | Yes |
| W1-3 | Make Host queries item-tolerant and path-safe | Intent/retrieval parsers | Query and realistic-shape tests | Invalid query dropped; only symbolic targets execute | Yes |
| W1-4 | Remove all typed Profile vetoes from production Work | Tool catalog, prompt builder, current/project adapters | AI focused 508/508 | Capability selects Skill/retrieval; generic current/project plans own content; missing current Sections are inferred for Preview | Yes |
| W1-5 | Full regression and documentation closure | Tests and current docs | AI 509/509; Application 198/198; IDE 2756/2756; Debug build 0 warnings/0 errors; diff check clean; IdeOnly package 1250 files | Automated baseline green | Manual provider/UI test remains |
| W1-6 | Remove presentation metadata from proposal admission | Generic document/project adapter, provider schemas, shared repair fixtures and docs | Focused adapter/project/repair/catalog 74/74; AI 493/493; Application 198/198; IDE 2771/2771; Debug build 0/0; binary rejection-text audit clean | Proposal `message` is ignored; invalid/blank `summary` defaults locally; clarification still requires a readable message | Yes |

## Corrected evidence statement

Previous delivery reports established deterministic compatibility with hand-authored
tool arguments, but did not establish real DeepSeek shape tolerance. In particular, the
runtime discarded the intent parser's failure detail and rejected non-safety metadata
differences. Those claims are superseded by this ledger.

The new automated evidence includes non-ideal tool arguments: additive properties,
missing optional arrays/placeholders, enum casing/separator variants, unknown capability
and domain strings, one unsafe query beside a valid query, single-object project document
shape and additive project/operation metadata. A natural-language UnitDelivery flow uses the generic project operation tool
rather than a local typed Profile. The same production rule covers current-document weapon-chain, dual-armament, Projectile
and Warhead capabilities; their typed compilers remain headless compatibility fixtures, not provider-visible production tools.

W1-6 closes a separate provider-shape regression exposed by real UI use. The generic
proposal adapter no longer validates `message` or rejects a missing/blank/null/object
`summary`; those values cannot change target, operation or authority. The same adapter
is used by normal execution and bounded repair. Clarification remains non-authoring and
continues to require a non-empty bounded string message.

## Remaining manual evidence

- Real DeepSeek V4 Flash response against the user's exact prompt: NotRun (no paid call
  made in this stage).
- WPF Project Diff visual and explicit Apply/Undo behavior: NotRun; existing automated
  preview/apply/undo regressions passed.
- Game-runtime correctness of model-selected SuperWeapon values: outside IDE automated
  verification; must be reviewed in Diff and tested in the target mod.
