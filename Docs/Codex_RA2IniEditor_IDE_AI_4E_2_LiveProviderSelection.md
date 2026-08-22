# Codex Task: RA2IniEditor.IDE AI-4E-2 Minimal Provider Selection / DeepSeek Live Send Flow

## 0. Current Baseline

AI-4E-1 has been completed.

Reported state:

```text
AI Assistant Advanced area now shows provider mode UI/state.
Provider default is Mock.
DeepSeek is visible but disabled / placeholder.
AI panel Generate / Send still uses AI-4C FakeRa2AiClient pipeline.
No API key UI.
No API key loading in send flow.
No settings persistence.
No Apply / Insert.
No file modification behavior.
Tests: 1368 passed.
IdeOnly package: passed, packaged file count 725.
Legacy not restored.
```

Next phase:

```text
AI-4E-2: Minimal provider selection wiring / DeepSeek live send flow
```

This is a limited implementation phase.

It is the first phase allowed to call `DeepSeekRa2AiClient`, but only after explicit user selection of DeepSeek.

---

## 1. Goal

Enable the AI Assistant to switch between:

```text
Mock
DeepSeek
```

The send flow should become:

```text
Provider = Mock:
  PromptBox
    -> bounded AI context
    -> PromptBuilder
    -> FakeRa2AiClient
    -> chat history

Provider = DeepSeek:
  PromptBox
    -> bounded AI context
    -> PromptBuilder
    -> DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()
    -> DeepSeekRa2AiClient
    -> chat history
```

DeepSeek must only be used when the user explicitly selects DeepSeek in Advanced.

Mock remains the default provider.

---

## 2. Hard Boundaries

Do not implement:

```text
API key input UI
API key save button
settings persistence
project config for API key
local user settings file for API key
Apply / Insert
file modification
Field Registry writes
whole-project context
auto-send context
diagnostic auto-fix
streaming output
retry loops
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

Do not change:

```text
PromptBuilder context boundaries
Field Registry evidence advisory semantics
Diagnostics advisory semantics
AI output draft/advisory semantics
```

---

## 3. Files Allowed

Allowed implementation files:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if existing AI panel state is there
RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs, only if provider injection needs minimal adjustment
RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs, only if needed for safe construction seam
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs, if pipeline is adjusted
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientFactoryTests.cs, only if factory behavior is touched
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not modify `DeepSeekRa2AiClient.cs` unless an existing adapter bug blocks integration and the fix is minimal.

---

## 4. Provider Selection UI

### 4.1 Provider selector

The Advanced area should allow choosing:

```text
Mock
DeepSeek
```

Required AutomationIds remain:

```text
AiAssistant.ProviderSelector
AiAssistant.ProviderStatus
AiAssistant.DeepSeekEnvironmentHint
```

DeepSeek is no longer merely disabled. It may be enabled as a selectable option.

### 4.2 Default

Default provider must remain:

```text
Mock
```

No live request should occur until:

```text
user opens AI Assistant
user selects DeepSeek
user explicitly clicks Generate / Send
```

### 4.3 Environment hint

The UI must continue to show:

```text
API Key 通过环境变量 DEEPSEEK_API_KEY 配置。
```

Do not add:

```text
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
AiAssistant.ApplyButton
```

---

## 5. DeepSeek Configuration Rules

DeepSeek live send must use:

```text
DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()
```

Supported environment variables:

```text
DEEPSEEK_API_KEY
DEEPSEEK_BASE_URL
DEEPSEEK_MODEL
DEEPSEEK_TIMEOUT_SECONDS
```

Rules:

```text
1. Do not read API key from UI.
2. Do not save API key.
3. Do not show API key.
4. Do not log API key.
5. If DEEPSEEK_API_KEY is missing, return/show MissingConfiguration.
6. Missing configuration should not crash the IDE.
```

---

## 6. Send Flow Rules

### 6.1 Common flow

For non-empty prompt:

```text
1. append user message
2. build bounded AI context
3. build prompt via Ra2AiPromptBuilder
4. select provider
5. call IRa2AiClient
6. append assistant success/error/cancel message
7. clear prompt according to existing behavior
```

For empty prompt:

```text
no-op
```

### 6.2 Mock provider

Must continue to use:

```text
FakeRa2AiClient
```

### 6.3 DeepSeek provider

Must use:

```text
DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()
DeepSeekRa2AiClient
```

DeepSeek response must be shown as draft/advisory text in chat history.

No Apply button.

No automatic insert.

No file modification.

---

## 7. Busy / Cancel Behavior

If request is in progress:

```text
1. Generate button should not start duplicate request.
2. Cancel should cancel current request through CancellationTokenSource.
3. Busy state must clear after success/error/cancel.
4. Cancelled response should be visible or safely no-op, but no stuck busy state.
```

If implementing full busy state is too large, stop and report before over-implementing.

---

## 8. Error Mapping

Map `Ra2AiResponseKind` to visible chat messages:

```text
Success:
  show response text

MissingConfiguration:
  DeepSeek 未配置。请设置环境变量 DEEPSEEK_API_KEY。

ProviderError:
  DeepSeek 请求失败。请检查网络、代理或稍后再试。

Cancelled:
  请求已取消。
```

If Timeout kind exists:

```text
Timeout:
  请求超时，请稍后再试。
```

Do not display:

```text
API key
Authorization header
raw prompt
raw response body
full context payload
selected INI text
nearby text
absolute paths
environment variables
```

---

## 9. Testing Strategy

Normal tests must not call real DeepSeek.

Required tests:

```text
1. Mock is default provider.
2. Provider selector/status exists.
3. Selecting DeepSeek changes provider state.
4. Missing DEEPSEEK_API_KEY yields MissingConfiguration message.
5. DeepSeek success appends assistant response using fake HTTP seam or injected test seam.
6. ProviderError appends safe error message.
7. Cancel clears busy state, if cancellation state is implemented.
8. No API key appears in UI error messages.
9. No ApiKeyTextBox exists.
10. No SaveApiKeyButton exists.
11. No Apply button exists.
12. Sending does not modify editor text.
13. Sending does not mark document dirty, if observable.
14. Tests do not require live network or real API key.
```

Preferred seam for tests:

```text
factory/client injection through small local provider factory method
or testable pipeline helper
```

Do not introduce a broad dependency injection framework.

---

## 10. Manual Smoke Checklist

After implementation:

```text
1. Ensure DEEPSEEK_API_KEY is not set.
2. Launch IDE.
3. Open AI Assistant.
4. Confirm provider default is Mock.
5. Send prompt in Mock mode; confirm Fake response.
6. Select DeepSeek.
7. Send prompt with no DEEPSEEK_API_KEY; confirm MissingConfiguration message.
8. Set DEEPSEEK_API_KEY in environment and restart IDE.
9. Select DeepSeek.
10. Send a small prompt.
11. Confirm response appears in chat.
12. Confirm no file content changes and no dirty state.
13. Confirm no Apply / Insert UI exists.
```

Do not put a real key in docs, screenshots, logs, or reports.

---

## 11. Validation Commands

Run full validation because Shell send flow may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 12. Final Report Format

Report:

```text
1. Phase completed: AI-4E-2.
2. Files changed.
3. Provider selection implementation.
4. DeepSeek configuration path.
5. Send flow changes.
6. Busy/cancel/error behavior.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation no API key UI/settings persistence added.
12. Confirmation no Apply/Insert/file modification behavior added.
13. Confirmation no Field Registry writes.
14. Manual smoke steps or result.
15. Remaining risks.
16. Recommended next phase.
```
