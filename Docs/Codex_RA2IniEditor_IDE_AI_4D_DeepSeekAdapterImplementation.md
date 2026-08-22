# Codex Task: RA2IniEditor.IDE AI-4D DeepSeek Adapter Implementation

## 0. Current Baseline

AI-4C has been completed.

Reported state:

```text
AI-4B: IRa2AiClient / Ra2AiResponse / FakeRa2AiClient completed.
AI-4C: AI panel send flow now uses:
  PromptBox
  -> current bounded AI context
  -> Ra2AiPromptBuilder
  -> IRa2AiClient
  -> FakeRa2AiClient
  -> chat history response

Tests: 1346 passed.
IdeOnly package: passed, packaged file count 715.
No DeepSeek / real network / API key loading.
No Apply / Insert.
No file modification behavior.
No Field Registry write behavior.
Legacy not restored.
```

Next phase:

```text
AI-4D: DeepSeek adapter implementation
```

This phase implements the adapter class and testable HTTP boundary, but must not yet switch the AI panel to real DeepSeek by default.

---

## 1. Goal

Implement a DeepSeek-compatible AI client adapter behind `IRa2AiClient`.

The adapter must:

```text
1. Consume Ra2AiRequest from AI-3B.
2. Send Ra2AiRequest.PromptText to DeepSeek-compatible chat completion endpoint.
3. Return Ra2AiResponse.
4. Support CancellationToken.
5. Handle timeout / provider error / missing configuration safely.
6. Avoid leaking API keys or raw prompt payloads in errors/logs.
7. Be testable without live network or real API key.
```

This task should prepare the real provider implementation, but not enable it in the visible AI panel unless explicitly approved.

---

## 2. Hard Boundaries

Do not implement:

```text
AI panel provider selector real behavior
model selector persistence
API key UI
settings persistence
Apply / Insert
file modification
Field Registry writes
whole-project context
auto-send context
diagnostic auto-fix
```

Do not modify:

```text
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn field registry JSON
legacy files
solution / project files
```

Do not make normal tests depend on live DeepSeek or real credentials.

---

## 3. Files Allowed

Allowed source files:

```text
RA2IniEditor.IDE/AI/DeepSeekRa2AiClient.cs
RA2IniEditor.IDE/AI/DeepSeekRa2AiClientOptions.cs
RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs, only if useful and small
RA2IniEditor.IDE/AI/IRa2AiClient.cs, only if a minimal extension is absolutely necessary
RA2IniEditor.IDE/AI/Ra2AiResponse.cs, only if a missing error/status field is needed
RA2IniEditor.IDE/AI/Ra2AiResponseKind.cs, only if a timeout / invalid configuration kind is needed
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs
RA2IniEditor.Tests/IDE/Ra2AiClientTests.cs, only if response enum changes require update
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project naming conventions.

Do not modify Shell UI / send-flow wiring in this phase unless user explicitly approves.

---

## 4. Configuration Policy

The adapter must not hard-code secrets.

Allowed configuration sources for this implementation:

```text
DeepSeekRa2AiClientOptions
```

The options may include:

```text
BaseUrl
ApiKey
Model
Timeout
```

But normal construction in tests must use fake values.

API key rules:

```text
1. No API key in source code.
2. No real API key in tests.
3. No real API key in docs examples.
4. No API key in error messages.
5. No API key in ToString/debug output.
6. Missing API key returns MissingConfiguration response or controlled error response.
```

Do not implement repository-stored config files.

Do not implement user settings UI.

Environment variable loading may be deferred to a later phase, or implemented only in a factory if small and testable.

If environment variables are used, names must be:

```text
DEEPSEEK_API_KEY
DEEPSEEK_BASE_URL
```

but the adapter itself should still be directly testable via options.

---

## 5. Request Mapping

The adapter should send a DeepSeek-compatible request using:

```text
Ra2AiRequest.PromptText
```

Do not send raw project files directly.

Do not rebuild prompt text inside adapter.

Do not add extra context inside adapter.

Expected conceptual mapping:

```text
model: configured model
messages:
  system/developer role if API requires it, but PromptBuilder already contains application rules
  user content: Ra2AiRequest.PromptText
temperature: conservative default
```

Use current DeepSeek-compatible API shape only if known from local docs / existing project config. If uncertain, implement a minimal OpenAI-compatible chat completion shape behind tests and document the assumption.

---

## 6. Response Mapping

Map provider response to `Ra2AiResponse`.

Required mappings:

```text
success -> Success with response text
missing API key / invalid options -> MissingConfiguration
HTTP non-success -> ProviderError
malformed JSON / missing content -> ProviderError
pre-cancelled token -> Cancelled
operation cancelled -> Cancelled
timeout -> ProviderError or Timeout kind if enum supports it
```

If adding `Timeout` or `InvalidConfiguration` enum values improves clarity, it is allowed, but tests must be updated.

Do not include raw request body in errors.

Do not include API key.

---

## 7. HTTP / Testability Requirements

The adapter must be testable without real network.

Preferred:

```text
Inject HttpClient or HttpMessageHandler-backed HttpClient.
```

Tests should use fake HttpMessageHandler.

Required tests:

```text
1. Sends request using configured base URL and model.
2. Sends Authorization header without exposing key in errors.
3. Uses Ra2AiRequest.PromptText.
4. Maps success response text.
5. Missing API key returns MissingConfiguration.
6. HTTP 500 maps ProviderError.
7. Malformed JSON maps ProviderError.
8. Pre-cancelled CancellationToken maps Cancelled.
9. Timeout/cancellation does not crash.
10. No real network is required.
```

---

## 8. Timeout / Cancellation

The adapter must support:

```text
CancellationToken
timeout option
non-blocking async call
```

Do not block UI thread.

Do not implement retry loops in AI-4D.

---

## 9. Logging / Redaction

Do not add logging of:

```text
raw prompt
raw response
API key
Authorization header
full context
selected INI text
nearby text
absolute paths
environment variables
```

If errors include provider messages, sanitize them.

---

## 10. UI Integration Boundary

AI-4D should not switch the AI panel to DeepSeek.

Preferred result:

```text
DeepSeekRa2AiClient exists and is tested.
AI panel still uses FakeRa2AiClient until a separate AI-4E provider selection / live send-flow phase.
```

If user explicitly approves live wiring later, that is AI-4E.

---

## 11. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 12. Manual Smoke Checklist

AI-4D should have no required UI change.

Optional smoke:

```text
1. Launch IDE.
2. Open AI Assistant.
3. Confirm current fake send flow still works.
4. Confirm no real DeepSeek/network request is made.
5. Confirm no API key is required.
```

---

## 13. Final Report Format

Report:

```text
1. Phase completed: AI-4D.
2. Files changed.
3. DeepSeek adapter implementation summary.
4. Configuration policy.
5. Request / response mapping.
6. Tests added.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation no real UI provider switching added.
12. Confirmation no API key stored in repository.
13. Confirmation no file modification behavior added.
14. Remaining risks.
15. Recommended next phase.
```
