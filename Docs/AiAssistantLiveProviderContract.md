# AI Assistant Live Provider Contract

## Implementation Update: AI-STREAM-0 RequestLifecycleHardening

- The application timeout is the single timeout authority: 120 seconds by default, overridable through `DEEPSEEK_TIMEOUT_SECONDS`.
- The factory-owned shared `HttpClient.Timeout` is `InfiniteTimeSpan`, preventing a hidden 100-second transport timeout from racing the application timeout.
- `Ra2AiRequestLifecycle` permits one active request and preserves request identity through cancellation and completion.
- Cancel requests cancellation but does not restore idle UI state early; the matching request completion owns cleanup.
- Closing the Shell requests cancellation of the active AI operation.
- Conversation turns have `Completed`, `InProgress`, `Incomplete`, or `Error` state; only `Completed` turns can enter a later prompt.
- Cancellation/error messages remain visible and advisory but are not reused as model context.
- This update does not add SSE, streaming transport, incremental UI rendering, retry, Apply/Insert, or file mutation.
- Verified with IDE-only build (0 warnings / 0 errors) and full tests (2034/2034).

## Implementation Update: AI-TimeoutAndContextDisclosure

- DeepSeek timeout is represented by `Ra2AiResponseKind.Timeout`.
- User cancellation remains `Ra2AiResponseKind.Cancelled`.
- The default timeout is 120 seconds; `DEEPSEEK_TIMEOUT_SECONDS` remains the environment override.
- The AI panel discloses the bounded context sent after explicit DeepSeek selection and send.
- Automatic retry remains out of scope to avoid duplicate requests and cost.

## 1. Scope and Baseline

This contract defines the future AI-4E provider selection and live send-flow boundary for the RA2IniEditor.IDE AI Assistant.

Baseline:

- `IRa2AiClient` exists and accepts `Ra2AiRequest` plus `CancellationToken`.
- `FakeRa2AiClient` exists and remains deterministic.
- `DeepSeekRa2AiClient` exists behind `IRa2AiClient`.
- `DeepSeekRa2AiClientFactory` creates options from environment variables.
- `DEEPSEEK_API_KEY` is the only approved live API key source.
- `DEEPSEEK_BASE_URL`, `DEEPSEEK_MODEL`, and `DEEPSEEK_TIMEOUT_SECONDS` are optional environment overrides.
- The current AI panel send-flow is wired to the internal fake pipeline by default.
- No live provider switching is implemented yet.
- No API key UI, settings persistence, Apply, Insert, file mutation, Field Registry write, whole-project context, auto-send, or diagnostic auto-fix is implemented.

AI-4E live provider work must preserve the existing safety position: the AI Assistant is a DeepSeek-powered RA2 modding assistant, not an autonomous file-editing agent. Provider output is advisory text only.

## 2. Current AI Pipeline

The current local send path is:

```text
PromptBox
  -> bounded current AI context
  -> Ra2AiPromptBuilder
  -> Ra2AiAssistantPipeline
  -> IRa2AiClient
  -> FakeRa2AiClient
  -> chat history
```

The pipeline must remain the shared boundary for Mock and DeepSeek modes. Future live integration must not duplicate context building, Field Registry evidence retrieval, diagnostics summary creation, or prompt building inside `ShellWindow.xaml.cs` or inside the DeepSeek adapter.

The DeepSeek adapter must consume only the `Ra2AiRequest` produced by `Ra2AiPromptBuilder`. It must not rebuild prompts, add hidden context, read files, inspect editor controls, query Field Registry providers, run diagnostics, or access project state.

## 3. Provider Modes

Future provider selection has exactly these first-phase modes:

```text
Mock
DeepSeek
```

### 3.1 Mock Mode

Mock mode is the default and remains the safe fallback.

Rules:

- Does not require API key.
- Does not require network.
- Uses `FakeRa2AiClient`.
- Remains deterministic for tests and offline use.
- Uses the same bounded context and PromptBuilder pipeline as live mode.
- Must not mutate editor text, dirty state, files, Field Registry data, diagnostics, parser behavior, completion, hover, quick peek, or save preflight.

### 3.2 DeepSeek Mode

DeepSeek mode is explicit. It must not be enabled silently by the presence of an environment variable.

Rules:

- User must explicitly select DeepSeek from the AI Assistant provider/model area.
- Options are created through `DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()`.
- API key is read only from `DEEPSEEK_API_KEY`.
- Optional base URL, model, and timeout are read from `DEEPSEEK_BASE_URL`, `DEEPSEEK_MODEL`, and `DEEPSEEK_TIMEOUT_SECONDS`.
- No API key input box exists.
- No API key is saved to settings.
- No API key is displayed, logged, copied into prompts, or included in chat messages.
- Normal tests and CI must not require real network or real credentials.

### 3.3 Default and Fallback

Recommended default:

```text
Mock
```

If the user selects DeepSeek and required configuration is missing, the UI shows a `MissingConfiguration` message. It must not crash, must not open an API key input UI, and must not silently fall back to Mock unless a later approved contract defines an explicit fallback UX.

## 4. Environment-only DeepSeek Configuration

The first live provider implementation uses environment variables only:

```text
Required:
DEEPSEEK_API_KEY

Optional:
DEEPSEEK_BASE_URL
DEEPSEEK_MODEL
DEEPSEEK_TIMEOUT_SECONDS
```

Configuration rules:

- `DeepSeekRa2AiClientOptions.ApiKey` may remain available for tests and explicit construction.
- UI must not collect or persist API keys.
- Local settings persistence is out of scope.
- Project files, repository files, package output, documentation examples, logs, diagnostics, exceptions, and chat text must not contain real API keys.
- Error text must be sanitized and must not include the API key, Authorization header, raw prompt, raw response, selected INI text, nearby text, full context, absolute local paths, or environment variable values.

The Advanced UI may display provider status text such as:

```text
Provider: Mock / DeepSeek
Model: Mock / configured DeepSeek model / unavailable
Status: Ready / Missing API Key / Provider Error / Timeout / Cancelled
```

It must not display:

```text
API key input
Save API key button
local settings path
secret value
Authorization header
raw request payload
raw response payload
```

Allowed explanatory UI copy:

```text
DeepSeek API key is configured through the DEEPSEEK_API_KEY environment variable.
```

## 5. Send Flow Rules

Common rules for both provider modes:

- Empty prompt is a no-op and appends no message.
- Non-empty prompt appends exactly one user message.
- Context is built only from the approved bounded providers.
- Prompt text is built only by `Ra2AiPromptBuilder`.
- Provider call goes through `IRa2AiClient.SendAsync`.
- `CancellationToken` is passed through the full send path.
- Successful response is appended as advisory assistant text.
- Generated INI remains draft text for manual review.
- Copy remains explicit and does not mutate the active document.
- No Apply button or Insert behavior is introduced.
- No editor text mutation occurs.
- Dirty state is not changed by provider response.
- Field Registry is not written, reloaded, imported, learned, applied, or rolled back by AI send.
- No whole-project context, auto-send, background provider request, or diagnostic auto-fix is introduced.

Mock send path:

```text
PromptBox
  -> ContextProvider
  -> Ra2AiPromptBuilder
  -> Ra2AiAssistantPipeline
  -> FakeRa2AiClient
  -> chat history
```

DeepSeek send path:

```text
PromptBox
  -> ContextProvider
  -> Ra2AiPromptBuilder
  -> Ra2AiAssistantPipeline
  -> DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()
  -> DeepSeekRa2AiClient
  -> chat history
```

Future wiring should keep provider creation small and testable. If `ShellWindow.xaml.cs` remains the integration point, it may only perform minimal provider-mode selection and dependency wiring; it must not inline provider request JSON, context building, evidence retrieval, diagnostics summary, or prompt construction.

## 6. Response and UI State Mapping

Current response kinds:

```text
Success
Cancelled
ProviderError
MissingConfiguration
```

Timeout is represented as a distinct response kind:

```text
Ra2AiResponseKind.Timeout
```

The DeepSeek adapter maps its timeout cancellation to `Timeout`, while an externally cancelled request remains `Cancelled`. UI mapping must use the response kind and must not infer timeout by parsing error text.

Required UI behavior:

```text
Success:
  append normal assistant message with advisory/draft wording.

MissingConfiguration:
  append error-style assistant message.
  recommended text: DeepSeek is not configured. Set DEEPSEEK_API_KEY and try again.

ProviderError:
  append error-style assistant message.
  recommended text: DeepSeek request failed. Check network/proxy settings or try again later.

Cancelled:
  clear busy state.
  either append compact cancellation status or return to idle without success text.
  recommended text if shown: Request cancelled.

Timeout:
  append error-style assistant message if modeled separately.
  recommended text: DeepSeek request timed out. Try again later.
```

Error-style assistant messages must not look like successful model output. They must not include raw provider text if that text can contain secrets, prompt payload, selected INI text, nearby context, full context, absolute paths, environment variables, or Authorization headers.

## 7. Busy and Cancellation State

Future live provider UI must support:

- `Idle`
- `Sending`
- `Cancelled`
- `ProviderError`
- `MissingConfiguration`
- `Timeout` if a distinct timeout kind is added

Required behavior:

- Sending state disables duplicate send.
- Cancel is enabled only while a request is in flight.
- Each request owns its own `CancellationTokenSource`.
- Cancellation token is passed to `Ra2AiAssistantPipeline.SendAsync` and then to `IRa2AiClient.SendAsync`.
- Success, error, missing configuration, cancellation, and timeout all clear busy state.
- Cancel must not append misleading success output.
- Cancel must not mutate editor text, mark dirty, write Field Registry data, or trigger Save Preflight.
- No fake long delay is introduced in normal use just to make cancellation visible.

## 8. Safety Boundaries

AI-4E live provider implementation must not change:

- XAML / Shell UI layout except explicitly approved provider controls.
- Main Shell layout, Project Explorer, Navigator, bottom tabs, status bar, global docking structure.
- INI parser semantics.
- Field Registry load/apply/rollback/import/learning semantics.
- Project > Global > BuiltIn priority.
- Completion candidate generation.
- Completion commit behavior.
- Hover data source.
- Diagnostics behavior.
- Save Preflight.
- Backup / rollback.
- Undo / redo.
- BuiltIn field definitions.
- Legacy exclusion rules.

AI provider output must never directly perform:

- file writes
- saves
- Apply
- Insert
- Field Registry writes
- whole-project reads
- shell commands
- diagnostic auto-fix
- editor text mutation
- dirty-state mutation

## 9. Tests to Add / Update

Future implementation tests should stay credential-free and network-free.

Planned tests:

1. Mock remains the default provider.
2. Mock mode uses `FakeRa2AiClient` and does not require environment variables.
3. Selecting DeepSeek uses `DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()`.
4. Missing `DEEPSEEK_API_KEY` maps to `MissingConfiguration`.
5. Missing configuration appends an error-style assistant message.
6. DeepSeek success appends one advisory assistant response.
7. DeepSeek provider HTTP error appends sanitized provider error message.
8. Malformed or missing provider content maps to provider error.
9. Timeout maps to either `ProviderError` with timeout wording or a distinct `Timeout` kind, depending on the implementation decision.
10. Cancellation clears busy state and does not append success output.
11. Duplicate send is blocked while sending.
12. Empty prompt appends no user or assistant message.
13. API key does not appear in response model, UI error text, exception text, or test output.
14. Authorization header is sent in adapter tests but not exposed in errors.
15. Sending does not modify editor text.
16. Sending does not mark the document dirty.
17. Sending does not write or reload Field Registry data.
18. No Apply / Insert control or behavior is introduced.
19. Tests use fake client or fake `HttpMessageHandler`; no real DeepSeek, network, or API key is required.
20. Environment-variable tests isolate and restore process environment values.

Suggested test scope:

```text
RA2IniEditor.Tests/IDE/Ra2AiAssistantProviderModeTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAssistantSendFlowTests.cs
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientFactoryTests.cs
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs
```

Avoid pixel-perfect UI tests. Prefer boundary tests around provider selection, response mapping, cancellation, and mutation guarantees.

## 10. Risks

- UI wording may imply generated text was applied; all provider output must remain advisory.
- Missing API key handling can accidentally encourage an API key UI; this phase must keep environment-only configuration.
- Provider errors can leak secrets if raw exception or HTTP content is shown.
- Live network calls can block the UI if not awaited asynchronously with timeout and cancellation.
- Silent fallback from DeepSeek to Mock can confuse users; fallback behavior requires an explicit UX decision.
- Adding a distinct `Timeout` enum is clearer but touches response mapping and tests.
- Provider selection inside `ShellWindow.xaml.cs` can grow quickly; future implementation should keep it as minimal wiring or extract a small provider-mode helper.

## 11. Recommended Implementation Plan

Recommended split after this contract:

```text
AI-4E-1: Provider mode state contract in code
  - Add provider mode enum/model if needed.
  - Keep Mock default.
  - No live DeepSeek call yet unless separately approved.

AI-4E-2: Minimal provider selection wiring
  - Select Mock or DeepSeek explicitly.
  - Create DeepSeek options only through DeepSeekRa2AiClientFactory.
  - Keep API key environment-only.
  - Preserve current bounded context and PromptBuilder pipeline.

AI-4E-3: Busy/cancel/error UI state
  - Ensure duplicate sends are blocked.
  - Ensure cancellation clears busy state.
  - Map MissingConfiguration, ProviderError, Cancelled, and timeout behavior to safe chat messages.

AI-4E-4: Tests and manual smoke
  - Add provider mode and send-flow tests with fake client/fake HTTP.
  - Verify no editor text mutation, no dirty state mutation, no Apply/Insert, no Field Registry writes.
```

AI-4E-1 and AI-4E-2 may be combined only with explicit user approval and only if the source changes remain small, testable, and credential-free.

## 12. Validation Commands

For this documentation-only contract, source build is not required unless the user requests it. Optional validation:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

For later implementation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 13. Acceptance Criteria

This contract is accepted when:

- Mock and DeepSeek provider modes are defined.
- Mock remains the default safe provider.
- DeepSeek is explicit and configured only through environment variables.
- No API key UI or settings persistence is allowed.
- MissingConfiguration, ProviderError, Cancelled, and timeout behavior are defined.
- Busy and cancellation state rules are defined.
- Send flow preserves bounded context, PromptBuilder, `IRa2AiClient`, and advisory chat output.
- Safety boundaries explicitly forbid Apply, Insert, file mutation, Field Registry writes, whole-project context, auto-send, and diagnostic auto-fix.
- Test plan covers provider selection, environment isolation, fake HTTP, sanitized errors, cancellation, no mutation, and no credentials.
- Recommended implementation split is documented.
- No source code, XAML, ViewModel, tests, scripts, Field Registry JSON, solution/project files, parser behavior, diagnostics behavior, completion, hover, quick peek, save preflight, BuiltIn JSON, or legacy files are modified by this contract task.
