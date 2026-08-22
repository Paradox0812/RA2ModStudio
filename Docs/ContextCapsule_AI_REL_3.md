# AI-REL-3 Context Capsule

## Scope

- Project: RA2IniEditor.IDE-only
- Package: AI-REL-3 ProviderTrustPrivacyAndResourceHardening
- Updated: 2026-07-20
- Contract: `Docs/Codex_RA2IniEditor_IDE_AI_REL_3_ProviderTrustPrivacyResource.md`

## Current state

AI-REL-3 is completed through 3I. This capsule is now a stable handoff record; any new AI behavior requires a new contract.

## Completed checkpoints

| Stage | Result | Verification |
|---|---|---|
| 3-0 | Clean rollback anchor created and exclusion-scanned | 904 entries; SHA-256 recorded in contract |
| 3A | Typed Flash/Pro catalog; Flash default; env model retired | Factory tests 15/15 |
| 3C1 | One-read immutable configuration snapshot | Factory tests 17/17 |
| 3C2 | Endpoint/model/numeric trust; non-thinking/max_tokens payload | Combined targeted tests 85/85 |
| 3B | Production Fake removed; typed Shell selector; prompt hard rejection | Shell/client boundary tests 102/102 |
| 3D | Shared outbound text sanitizer | Sanitizer/conversation tests 27/27 |
| 3E | Sanitized/bounded prompt plus preparation facts | PromptBuilder/pipeline tests 30/30 |
| 3F1 | Request-local safe transport diagnostics | DeepSeek client tests 62/62 |
| 3F2 | Invariant-enforcing response factories; constructor private | Response/client/pipeline tests 90/90 |
| 3G | Safe Shell facts plus history/Markdown resource bounds | Shell/resource tests 51/51 |
| 3H | Deterministic loopback transport and failure verification | Loopback/failure/UI-boundary tests 48/48 |
| 3I | Full verification, live provider smoke, UI smoke, docs and package closure | Build 0/0; full tests 2171/2171; Flash/Pro each passed once |

## Current contract facts

- `DeepSeekRa2AiModelCatalog` is the only model display/API-id mapping source.
- Shell captures a typed model, creates one `DeepSeekRa2AiConfigurationSnapshot`, and constructs the client from that same snapshot.
- Only remote HTTPS and loopback HTTP endpoints are accepted; endpoint and API key are never displayed by `ToString()`.
- Product/production code has no Fake/Mock provider path; deterministic substitutes live only in tests.
- User prompt over 8000 characters is rejected before request-session creation and remains in the input box.
- No automatic retry, fallback, thinking selector, persistence, new dependency, file mutation or Field Registry behavior change is authorized.

## Next entry

No AI-REL-4 implementation is authorized. The next AI change must begin with code-fact regression and a separate contract. Automatic retry, provider/model fallback, thinking-mode selection, persistence, new dependencies, file mutation and broad Shell redesign remain outside the accepted baseline.

## Verification baseline

- All targeted gates are green as listed above; no unresolved F2 or diagnostics-stability debt remains.
- Final restore/build/test passed; build reported 0 warnings/0 errors and tests passed 2171/2171.
- Runtime AI-panel smoke passed. Minimal live DeepSeek V4 Flash and V4 Pro requests each succeeded once; they must not be repeated merely for this package.
- Rollback anchor: `artifacts/RA2IniEditor.IDE.SourceClean.AI-REL-3-0.Rollback.zip`, SHA-256 `C7D6B446E8BDB42147BCCC7D8E93D06432876C08DC2E14C9EC07F837F6614759`.

## Required reading

1. `AGENTS.md`
2. `Docs/Codex_RA2IniEditor_IDE_AI_REL_3_ProviderTrustPrivacyResource.md`
3. `Docs/Codex_CurrentPhase.md`
4. This capsule
