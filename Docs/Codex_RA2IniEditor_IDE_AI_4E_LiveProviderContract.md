# Codex Task: RA2IniEditor.IDE AI-4E Provider Selection / Live Send Flow Contract

## 0. Current Baseline

AI-4D-Fix has been completed.

Reported state:

```text
DeepSeekRa2AiClient implemented.
DeepSeekRa2AiClientOptions implemented.
DeepSeekRa2AiClientFactory implemented.
DeepSeek API key policy is environment-variable only.
DEEPSEEK_API_KEY is required for live DeepSeek.
DEEPSEEK_BASE_URL / DEEPSEEK_MODEL / DEEPSEEK_TIMEOUT_SECONDS are optional.
AI panel still uses FakeRa2AiClient by default.
No API key UI.
No settings persistence.
No Apply / Insert.
No file modification behavior.
Tests: 1368 passed.
IdeOnly package: passed, packaged file count 722.
Legacy not restored.
```

Next phase:

```text
AI-4E: Provider Selection / Live Send Flow Contract
```

This phase is **contract / planning first**.

Do not implement live provider wiring in this task.

---

## 1. Goal

Define how the AI Assistant panel will switch from fake provider to DeepSeek provider safely.

The goal is to introduce an explicit provider mode without storing API keys in UI or settings.

Provider modes:

```text
Mock
DeepSeek
```

The first live integration must:

```text
1. Keep Mock as safe fallback.
2. Use DeepSeek only when explicitly selected.
3. Read DeepSeek configuration only from environment variables through DeepSeekRa2AiClientFactory.
4. Show missing configuration clearly.
5. Preserve no-Apply / no-Insert behavior.
6. Preserve bounded context and PromptBuilder pipeline.
```

---

## 2. Hard Boundaries

Do not implement in this contract phase:

```text
source code changes
XAML changes
live DeepSeek wiring
provider selector behavior
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

---

## 3. Documents to Read First

Read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/AiAssistantArchitecture.md
Docs/AiAssistantSafetyContract.md
Docs/AiAssistantDeepSeekAdapterContract.md
Docs/AiAssistantPromptBuilderContract.md
```

Then inspect current AI source files:

```text
IRa2AiClient
FakeRa2AiClient
DeepSeekRa2AiClient
DeepSeekRa2AiClientOptions
DeepSeekRa2AiClientFactory
Ra2AiAssistantPipeline
Ra2AiPromptBuilder
ShellWindow AI send flow
```

---

## 4. Required Contract Output

Create or update:

```text
Docs/AiAssistantLiveProviderContract.md
```

Suggested structure:

```markdown
# AI Assistant Live Provider Contract

## 1. Scope and Baseline
## 2. Current AI Pipeline
## 3. Provider Modes
## 4. Environment-only DeepSeek Configuration
## 5. Advanced UI Requirements
## 6. Send Flow Rules
## 7. Error / Missing Configuration Behavior
## 8. Cancellation / Busy State
## 9. Safety Boundaries
## 10. Tests to Add / Update
## 11. Risks
## 12. Recommended Implementation Plan
## 13. Acceptance Criteria
```

---

## 5. Provider Mode Contract

### 5.1 Mock Mode

Mock mode remains available.

Rules:

```text
1. Does not require API key.
2. Does not require network.
3. Remains deterministic.
4. Useful for tests and offline use.
```

### 5.2 DeepSeek Mode

DeepSeek mode is explicit.

Rules:

```text
1. User selects DeepSeek from Advanced provider/model area.
2. API key is read only from DEEPSEEK_API_KEY.
3. Base URL/model/timeout are read from optional environment variables.
4. No API key input box exists.
5. No API key is saved to settings.
6. No API key is displayed.
```

### 5.3 Default Mode

Recommended default:

```text
Mock
```

until user explicitly switches to DeepSeek.

If user selects DeepSeek but DEEPSEEK_API_KEY is missing:

```text
show MissingConfiguration message in chat
do not crash
do not open key input UI
do not fall back silently unless explicitly designed
```

---

## 6. Advanced UI Requirements

The existing Advanced area near the composer may show:

```text
Provider: Mock / DeepSeek
Model: current configured model or placeholder
Status: Ready / Missing API Key / Error
```

It must not show:

```text
API key input
Save API key button
local settings path
secret value
```

Allowed UI text:

```text
API Key 通过环境变量 DEEPSEEK_API_KEY 配置。
```

No real settings persistence in first live phase.

---

## 7. Send Flow Contract

When provider = Mock:

```text
PromptBox
  -> ContextProvider
  -> PromptBuilder
  -> FakeRa2AiClient
  -> chat history
```

When provider = DeepSeek:

```text
PromptBox
  -> ContextProvider
  -> PromptBuilder
  -> DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()
  -> DeepSeekRa2AiClient
  -> chat history
```

Common rules:

```text
1. empty prompt no-op
2. user message appended once
3. busy state prevents duplicate sends
4. cancel cancels in-flight provider call if present
5. response is displayed as draft/advisory text
6. no editor text mutation
7. no dirty state mutation
8. no Field Registry writes
9. no Apply button
```

---

## 8. Error Behavior

Map provider results to chat messages:

```text
MissingConfiguration:
  DeepSeek 未配置。请设置环境变量 DEEPSEEK_API_KEY。

ProviderError:
  DeepSeek 请求失败。请检查网络、代理或稍后再试。

Cancelled:
  请求已取消。

Timeout:
  请求超时，请稍后再试。
```

If `Timeout` is not a separate enum, contract should decide whether to add it before implementation.

Errors must not include:

```text
API key
Authorization header
raw prompt
raw response body
full context
selected INI text
nearby text
absolute paths
environment variables
```

---

## 9. Cancellation / Busy State

Future implementation must support:

```text
1. Sending state.
2. Cancel button enabled only while sending.
3. CancellationTokenSource per request.
4. Clear busy state after success / error / cancellation.
5. No duplicate send while sending.
```

Do not fake long delays in normal use.

---

## 10. Tests to Plan

Plan tests for:

```text
1. Mock remains default provider.
2. Selecting DeepSeek uses DeepSeekRa2AiClientFactory.
3. Missing DEEPSEEK_API_KEY shows MissingConfiguration message.
4. DeepSeek success appends assistant response.
5. DeepSeek provider error appends safe error message.
6. DeepSeek cancellation clears busy state.
7. No API key appears in UI error message.
8. No Apply button exists.
9. Sending does not modify editor text.
10. Sending does not mark document dirty.
11. Tests use fake HttpMessageHandler / fake client; no real network.
```

Avoid pixel-perfect tests.

---

## 11. Recommended Implementation Split

After this contract, implement in small phases:

```text
AI-4E-1: Provider mode UI state in Advanced area, Mock default, no DeepSeek call.
AI-4E-2: Wire DeepSeek provider through factory with fake HttpClient/fake client seam for tests.
AI-4E-3: Busy/cancel/error chat states.
AI-4E-4: Manual smoke and provider status polish.
```

If Codex can keep the scope small, AI-4E-1 and AI-4E-2 may be combined only after user approval.

---

## 12. Validation Commands

For documentation-only contract:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If implementation is later approved:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 13. Final Report Format

Report:

```text
1. Phase completed: AI-4E contract.
2. Files changed.
3. Provider mode decisions.
4. Environment-only API key policy.
5. Error/cancellation contract.
6. Tests planned.
7. Commands run.
8. Test result.
9. Package result.
10. Confirmation no source code changed.
11. Recommended next phase.
```
