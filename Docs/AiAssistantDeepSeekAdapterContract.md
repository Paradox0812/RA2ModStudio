# AI Assistant DeepSeek Adapter Contract

## 1. Scope and Baseline

AI-4A defines the contract for connecting the RA2IniEditor.IDE AI Assistant pipeline to a future DeepSeek text generation backend.

Baseline:

- AI-2B added bounded current-document / caret context.
- AI-2C added local Field Registry evidence retrieval.
- AI-2D added bounded diagnostics summary integration.
- AI-3A defined the Prompt Builder contract.
- AI-3B implemented deterministic Prompt Builder types and tests.
- The current AI Assistant still has no DeepSeek client, network access, API key configuration, Apply, Insert, file mutation, Field Registry write, whole-project context, auto-open, or auto-send behavior.
- The AI Assistant is a DeepSeek-powered RA2 Modding Assistant, not a Codex-like autonomous file editing agent.

AI-4A is documentation and planning only. It does not authorize source code changes.

The future DeepSeek adapter must consume only a `Ra2AiRequest` produced by the Prompt Builder. It must treat DeepSeek as a text generation provider and must not give DeepSeek file-system, tool, registry, save, apply, or shell authority.

## 2. Adapter Placement

The future adapter must sit behind a small AI client abstraction.

Preferred future shape:

```csharp
internal interface IRa2AiClient
{
    Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken);
}
```

If this interface already exists by the implementation phase, reuse it. If it does not exist, AI-4B may introduce it under `RA2IniEditor.IDE/AI` with deterministic tests.

Allowed dependency direction:

```text
AI Panel / ViewModel
  -> Context Provider
  -> Prompt Builder
  -> IRa2AiClient
  -> DeepSeek Adapter
```

Forbidden dependency direction:

```text
DeepSeek Adapter -> XAML controls
DeepSeek Adapter -> editor controls
DeepSeek Adapter -> Field Registry providers
DeepSeek Adapter -> diagnostics services
DeepSeek Adapter -> parser services
DeepSeek Adapter -> filesystem
DeepSeek Adapter -> Shell commands
```

The adapter must not be called directly from XAML. UI code should only interact with a client abstraction or ViewModel state. The adapter must not build context or build prompts; it receives a ready `Ra2AiRequest`.

## 3. Configuration and API Key Policy

No API key may be stored in the repository.

Allowed future configuration sources:

- Environment variable only for the first implementation.
- User-local settings files are not allowed to store API keys in the current implementation.
- Secure OS credential store remains a possible future option only after a separate approved contract.

Initial environment variable names:

```text
DEEPSEEK_API_KEY
DEEPSEEK_BASE_URL
DEEPSEEK_MODEL
DEEPSEEK_TIMEOUT_SECONDS
```

Implementation must verify current DeepSeek provider documentation before hard-coding endpoint paths, model names, request fields, or response fields.

Required API key rules:

- DeepSeek API key is read from `DEEPSEEK_API_KEY`.
- No API key in source code.
- No API key in documentation examples except placeholder names.
- No API key in package output.
- No API key in logs, diagnostics, exception text, or UI error details.
- No API key in prompts.
- No API key input in the AI Assistant Advanced UI.
- No local settings persistence for API keys.
- Missing API key must disable or error the live provider gracefully.
- Invalid configuration must produce a user-visible non-crashing error state.
- Normal unit tests and CI must not require real API keys.

Model configuration must remain explicit and bounded. The model selector placeholder in the UI must not imply that a live provider is configured until a later implementation phase actually wires it.

## 4. Request Mapping

The adapter request body must be based on:

```text
Ra2AiRequest.PromptText
```

The adapter may also use:

```text
Ra2AiRequest.Intent
Ra2AiRequest.UserPrompt
```

only for local metadata, telemetry-free UI state, or future non-sensitive routing decisions. It must not reconstruct context from files or IDE state.

The adapter must not send:

- Whole project content.
- Whole current document.
- Entire Field Registry.
- Diagnostics not already present in the PromptBuilder output.
- Absolute local paths by default.
- Environment variables.
- API keys or credentials.
- Clipboard content unless explicitly included in the prompt by the user workflow.
- Build/package/test output directories.

Provider-specific request shape belongs to AI-4B/AI-4C implementation and must be verified against current provider documentation. AI-4A does not lock JSON fields, endpoint URLs, or model names.

Future request creation rules:

- Requests are sent only after explicit user action.
- No auto-send on caret movement, diagnostics update, file open, project load, hover, completion, or Section selection.
- Use `CancellationToken`.
- Use a bounded timeout.
- Avoid retry loops by default.
- Do not block the UI thread.
- Do not write request payloads to logs by default.

## 5. Response Handling

DeepSeek response is text only.

The IDE must treat response text as:

```text
explanation
suggestion
draft INI
unit prototype draft
diagnostic explanation
field recommendation
```

It must not be treated as:

```text
trusted configuration
validated INI
applied edit
save authorization
Field Registry update
diagnostic suppression
automatic fix
```

Future response model should separate:

- Response text.
- Provider status.
- Optional provider metadata that is safe to show.
- Error state, if any.

No response path may modify editor text, mark the document dirty, write Field Registry files, change diagnostics, alter Save Preflight, or trigger Apply / Insert. Any future insertion workflow requires a separate preview/confirm contract.

Response text should be displayed in AI chat history as advisory output. Generated INI must remain visibly marked as draft when the PromptBuilder requested draft output or the response contains an INI block.

## 6. Timeout and Cancellation

Future live requests must support cancellation.

Required behavior:

- User can cancel an in-flight provider request.
- Cancellation clears busy state.
- Cancellation does not append misleading success responses.
- Cancellation does not mutate editor text or Field Registry state.
- Cancellation does not leave the AI panel stuck in sending state.

Timeout behavior:

- Use a bounded default timeout.
- Timeout maps to a visible timeout state.
- Timeout clears busy state.
- Timeout does not retry indefinitely.
- Timeout does not expose raw prompt or API key.

Recommended future state values:

```text
idle
sending
response received
cancelled
timeout
network error
provider error
missing API key
invalid configuration
```

## 7. Error Handling

Provider errors must not crash the IDE.

Required error handling:

- Missing API key maps to disabled provider or missing-key error state.
- Invalid base URL / model / configuration maps to invalid-configuration state.
- Network failure maps to network-error state.
- Timeout maps to timeout state.
- Provider HTTP errors map to provider-error state.
- Malformed provider response maps to provider-error state.
- Cancellation maps to cancelled state.

Errors must be visible to the user but must be sanitized. They must not expose:

- API key.
- Raw request body.
- Raw prompt.
- Raw response.
- Selected INI text.
- Nearby text.
- Full context payload.
- Absolute local paths.
- Environment variables.

The AI chat history must distinguish errors from successful assistant responses. A provider error must not appear as a normal model answer.

## 8. Logging and Redaction

Default behavior: do not log AI prompts or responses.

Do not log by default:

- Raw prompt.
- Raw response.
- API key.
- Full context payload.
- Selected INI text.
- Nearby text.
- Field Registry evidence payload.
- Diagnostics payload.
- Absolute local paths.
- Environment variables.
- Provider headers.

If future debug logging is added, it must be:

- Opt-in.
- Redacted.
- Disabled in normal CI.
- Excluded from source packages.
- Covered by a separate logging contract or implementation task.

Exception messages should be sanitized before reaching UI or logs. Tests must verify that API keys are not included in visible error text.

## 9. UI State Integration

The future UI integration must preserve the Right Tool Well behavior:

- Section Tree remains the default page.
- AI opens only through explicit user command.
- No auto-open on diagnostics, caret movement, project load, file open, hover, completion, or Section selection.
- No Apply button is introduced by DeepSeek adapter work.
- Copy remains explicit and does not mutate the document.

Provider state should map to AI panel state:

- `idle`: ready for user input.
- `sending`: disable duplicate send, enable cancel if supported.
- `response received`: append advisory assistant response.
- `cancelled`: show compact cancellation status or return to idle.
- `timeout`: show sanitized timeout error.
- `network error`: show sanitized network error.
- `provider error`: show sanitized provider error.
- `missing API key`: show provider unavailable state.
- `invalid configuration`: show configuration error state.

The UI must not display raw prompt payloads by default. If a future prompt preview is exposed, it must be a separate explicit debug/development feature and must not leak secrets or absolute paths.

## 10. Tests to Add / Update

AI-4B / AI-4C implementation must be testable without live DeepSeek.

Required future tests:

1. Missing API key produces disabled or missing-key state without crashing.
2. Invalid configuration produces sanitized error state.
3. Adapter uses supplied `Ra2AiRequest.PromptText`.
4. Adapter does not read files, build context, query Field Registry providers, or rerun diagnostics.
5. Adapter supports cancellation.
6. Timeout maps to timeout error state.
7. Provider HTTP error maps to provider error state.
8. Malformed response maps to provider error state.
9. API key does not appear in exception messages, UI error text, logs, or response models.
10. Raw prompt and raw response are not logged by default.
11. Fake HTTP handler can simulate success, timeout, cancellation, and provider errors.
12. Response is displayed as advisory/draft text.
13. No editor text mutation occurs.
14. Dirty state is not changed by provider response.
15. Field Registry is not written or reloaded.
16. Normal unit tests do not require network or credentials.

Potential future test files:

```text
RA2IniEditor.Tests/IDE/Ra2AiClientBoundaryTests.cs
RA2IniEditor.Tests/IDE/DeepSeekAiClientTests.cs
RA2IniEditor.Tests/IDE/AiAssistantProviderStateTests.cs
```

Only update UI boundary tests if a later implementation phase changes UI state or AutomationIds.

## 11. Non-goals

AI-4A does not implement:

- DeepSeek client source code.
- `IRa2AiClient`.
- `Ra2AiResponse`.
- HTTP request code.
- Response parsing code.
- API key UI.
- Settings persistence.
- Model selector behavior.
- PromptBuilder changes.
- Context provider changes.
- AI panel live send flow.
- Apply / Insert.
- File modification.
- Field Registry writes.
- Whole-project context.
- Diagnostic auto-fix.
- Shell command execution.

Future implementation phases must not use DeepSeek output as authority to save, apply, insert, rewrite diagnostics, or rewrite Field Registry data.

## 12. Risks

- Provider API details may change; implementation must verify current DeepSeek documentation before coding request/response fields.
- API keys can leak through exception messages if errors are not sanitized.
- Raw prompt logging could leak selected INI text or nearby context.
- Long prompts may increase latency and cost; context remains bounded by earlier phases.
- Network calls can hang the UI if not awaited asynchronously with timeout and cancellation.
- Provider output may include unsafe instructions; the IDE must keep it as advisory text only.
- UI wording can accidentally imply changes were applied; response display must avoid that.
- Retry loops can spam the provider; default implementation should avoid automatic retries unless later contracted.

## 13. Recommended Implementation Plan

Recommended split after AI-4A:

1. AI-4B: AI client abstraction and fake provider boundary
   - Add `IRa2AiClient` and `Ra2AiResponse` / state model if needed.
   - Add fake or deterministic client tests.
   - No live network.
   - No UI live provider call.

2. AI-4C: DeepSeek adapter implementation with testable HTTP boundary
   - Read API key from `DEEPSEEK_API_KEY` through an environment-only factory path.
   - Optionally read `DEEPSEEK_BASE_URL`, `DEEPSEEK_MODEL`, and `DEEPSEEK_TIMEOUT_SECONDS`.
   - Use fake HTTP handler for tests.
   - Add timeout, cancellation, sanitized error mapping.
   - No UI live call unless explicitly approved.

3. AI-4D: AI panel send flow integration
   - Use Context Provider -> Prompt Builder -> `IRa2AiClient`.
   - Add busy/cancel/error UI state.
   - Preserve mock/local testability.
   - No Apply / Insert.

4. AI-4E: Provider settings and model selector polish
   - Separate contract required.
   - Keep API key out of repository, package output, UI input, and local persisted settings.
   - Keep tests credential-free.

AI-4B and AI-4C may be combined only with explicit user approval and only if the implementation remains small, testable, and credential-free in CI.

## 14. Acceptance Criteria

The DeepSeek adapter contract is accepted when:

- Adapter placement is behind `IRa2AiClient` or an equivalent future AI client abstraction.
- Adapter consumes `Ra2AiRequest.PromptText` and does not build context or prompts.
- API key policy excludes repository, package output, logs, diagnostics, UI errors, and tests.
- Request mapping forbids whole-project, whole-document, entire-registry, secrets, generated folders, and unbounded context.
- Response handling treats DeepSeek output as advisory/draft text only.
- Timeout and cancellation behavior are defined.
- Error UI states are defined and sanitized.
- Logging and redaction rules forbid raw prompt/response logging by default.
- Tests are planned with fake HTTP and no live credentials.
- Non-goals explicitly exclude Apply / Insert, file modification, Field Registry writes, diagnostics changes, parser changes, and legacy restoration.
- Recommended implementation split is documented for AI-4B through AI-4E.
- No source code, XAML, code-behind, ViewModels, tests, scripts, Field Registry services, diagnostics behavior, parser behavior, completion, hover, quick peek, save preflight, BuiltIn JSON, solution/project files, or legacy files are modified by AI-4A.
