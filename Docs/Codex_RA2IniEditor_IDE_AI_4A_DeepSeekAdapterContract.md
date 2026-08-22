# Codex Task: RA2IniEditor.IDE AI-4A DeepSeek Adapter Contract

## 0. Current Baseline

AI-3B has been completed.

Reported state:

```text
AI-2B: bounded current-document / caret context provider completed.
AI-2C: local Field Registry evidence retrieval completed.
AI-2D: bounded diagnostics summary integration completed.
AI-3A: Prompt Builder contract completed.
AI-3B: deterministic Prompt Builder implementation completed.
Tests: 1336 passed.
IdeOnly package: passed, packaged file count 704.
No DeepSeek / network / API key.
No Apply / Insert.
No file modification behavior.
No Field Registry write behavior.
Legacy not restored.
```

AI-3B added:

```text
Ra2AiIntent
Ra2AiPromptBuildRequest
Ra2AiRequest
IRa2AiPromptBuilder
Ra2AiPromptBuilder
Ra2AiPromptBuilderTests
```

Next phase:

```text
AI-4A: DeepSeek Adapter Contract
```

This phase is **contract / planning only**.

Do not implement DeepSeek source code in this task.

---

## 1. Goal

Define how RA2IniEditor.IDE will connect the existing AI Assistant pipeline to DeepSeek safely.

The adapter must sit behind the existing AI client abstraction and consume the `Ra2AiRequest` produced by the Prompt Builder.

This contract must define:

```text
1. DeepSeek adapter boundaries.
2. API key and configuration policy.
3. Request / response mapping.
4. Timeout and cancellation behavior.
5. Error handling and UI state.
6. Logging and redaction rules.
7. Test strategy without live network.
8. Explicit non-goals.
```

Do not implement the adapter yet.

---

## 2. Required Documents to Read

Before writing the contract, read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/AiAssistantArchitecture.md
Docs/AiAssistantSafetyContract.md
Docs/AiAssistantContextProviderContract.md
Docs/AiAssistantPromptBuilderContract.md
```

Then inspect AI source files added in AI-1C / AI-2B / AI-2C / AI-2D / AI-3B.

---

## 3. Hard Boundaries

Do not modify source code in AI-4A.

Do not implement:

```text
DeepSeek client source code
network calls
API key UI
settings persistence
HTTP request code
response parsing code
Apply / Insert
file modification
Field Registry writes
whole-project context
auto-open AI
auto-send context
diagnostic auto-fix
```

Do not modify:

```text
XAML
code-behind
ViewModels
tests
scripts
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn field registry JSON
solution / project files
legacy files
```

---

## 4. Adapter Placement

The future DeepSeek adapter must be behind an AI client abstraction.

Preferred future shape:

```csharp
internal interface IRa2AiClient
{
    Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken);
}
```

If `IRa2AiClient` already exists by implementation time, reuse it. If not, AI-4B may introduce it.

The adapter must not be called directly from XAML.

The adapter must not read files, build context, or build prompts.

Allowed future dependency direction:

```text
AI Panel / ViewModel
  -> Context Provider
  -> Prompt Builder
  -> IRa2AiClient
  -> DeepSeek Adapter
```

Forbidden dependency direction:

```text
DeepSeek Adapter -> editor controls
DeepSeek Adapter -> Field Registry provider
DeepSeek Adapter -> diagnostics service
DeepSeek Adapter -> filesystem
```

---

## 5. API Key / Configuration Policy

API key must not be stored in repository.

Allowed future sources:

```text
environment variable
user-local settings file excluded from source/package
secure OS credential store, if later approved
```

Recommended initial environment variables:

```text
DEEPSEEK_API_KEY
DEEPSEEK_BASE_URL
```

But implementation must verify current provider documentation before hard-coding request details.

The contract must state:

```text
1. No API key in source code.
2. No API key in docs examples except placeholder names.
3. No API key in package output.
4. No API key in logs.
5. Missing API key disables live provider gracefully.
6. Tests must not require real API key.
```

---

## 6. Network / Request Policy

Future adapter must:

```text
1. Use explicit user-triggered request only.
2. Support CancellationToken.
3. Use timeout.
4. Avoid retry loops by default.
5. Never block UI thread.
6. Never auto-send on caret movement, diagnostics update, file open, or project load.
7. Never upload whole project by default.
```

The request body must be based on:

```text
Ra2AiRequest.PromptText
```

not raw project files.

---

## 7. Response Policy

DeepSeek response is text.

The IDE must treat it as:

```text
explanation / suggestion / draft
```

not as:

```text
trusted configuration
applied edit
validated INI
```

Response must be displayed in chat history.

No file mutation.

No auto-insert.

No Field Registry update.

---

## 8. Error / Cancellation UI Policy

Future adapter must map provider state to UI states:

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

Errors must:

```text
1. not crash IDE.
2. clear busy state.
3. not mutate chat history into misleading success.
4. be visible to the user.
5. avoid exposing API key or raw sensitive request.
```

Cancel must not leave the AI panel in a stuck busy state.

---

## 9. Logging / Redaction Policy

Do not log by default:

```text
raw prompt
raw response
API key
full context payload
selected INI text
nearby text
absolute paths
environment variables
```

If future debug logging is added, it must be opt-in and redacted.

---

## 10. Test Strategy

AI-4B implementation must be testable without live DeepSeek.

Required future tests:

```text
1. Missing API key produces disabled/error state.
2. Adapter uses supplied Ra2AiRequest.PromptText.
3. Adapter supports cancellation.
4. Timeout maps to error state.
5. Provider error maps to error state.
6. No API key appears in exception messages.
7. No live network is required in normal unit tests.
8. Mock/fake HTTP handler can simulate response.
9. Response is displayed as draft/advisory text.
10. No file modification or dirty state occurs.
```

Do not add integration tests requiring real credentials in normal CI.

---

## 11. Output Required

Create or update:

```text
Docs/AiAssistantDeepSeekAdapterContract.md
```

Suggested structure:

```markdown
# AI Assistant DeepSeek Adapter Contract

## 1. Scope and Baseline
## 2. Adapter Placement
## 3. Configuration and API Key Policy
## 4. Request Mapping
## 5. Response Handling
## 6. Timeout and Cancellation
## 7. Error Handling
## 8. Logging and Redaction
## 9. UI State Integration
## 10. Tests to Add / Update
## 11. Non-goals
## 12. Risks
## 13. Recommended Implementation Plan
## 14. Acceptance Criteria
```

---

## 12. Recommended Implementation Split After Contract

After AI-4A, do not implement everything at once if risky.

Recommended split:

```text
AI-4B: IRa2AiClient + fake/testable HTTP boundary, no UI live call yet.
AI-4C: DeepSeek adapter implementation with environment-variable configuration.
AI-4D: AI panel send flow uses PromptBuilder + DeepSeek adapter with cancel/error state.
AI-4E: Provider settings / model selector polish.
```

If implementation is simple, AI-4B/4C may be combined, but only with user approval.

---

## 13. Validation Commands

For documentation-only AI-4A:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 14. Final Report Format

Report:

```text
1. Phase completed: AI-4A.
2. Files changed.
3. Adapter placement decisions.
4. API key / configuration policy.
5. Error / cancellation policy.
6. Logging / redaction policy.
7. Recommended implementation split.
8. Commands run.
9. Test result.
10. Package result.
11. Confirmation no source code changed.
12. Recommended next phase.
```
