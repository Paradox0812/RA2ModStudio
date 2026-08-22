# Codex Task: RA2IniEditor.IDE AI-4E-1 Provider Mode UI State / Mock Default

## 0. Current Baseline

AI-4E live provider contract has been completed.

Reported state:

```text
Docs/AiAssistantLiveProviderContract.md created.
Default provider: Mock.
DeepSeek must be explicitly selected.
DeepSeek configuration is environment-variable only.
DEEPSEEK_API_KEY is required for live DeepSeek.
DEEPSEEK_BASE_URL / DEEPSEEK_MODEL / DEEPSEEK_TIMEOUT_SECONDS are optional.
No API key UI.
No local settings persistence.
No Apply / Insert.
No file modification behavior.
No Field Registry writes.
```

Next phase:

```text
AI-4E-1: Provider mode UI state in Advanced area, Mock default, no live DeepSeek call.
```

This is a limited implementation phase.

Do not call DeepSeek in this phase.

---

## 1. Goal

Add provider mode state to the AI Assistant Advanced area.

The AI panel should expose the future provider selection shape without making any real DeepSeek request.

Required result:

```text
1. Provider mode is visible in Advanced area.
2. Mock is the default provider.
3. DeepSeek can appear as an option or disabled/placeholder option.
4. Current send flow still uses FakeRa2AiClient only.
5. No real DeepSeek call is made.
6. No API key is read.
7. No API key UI or persistence is added.
```

This phase is UI/state preparation only.

---

## 2. Hard Boundaries

Do not implement:

```text
live DeepSeek send flow
DeepSeek client switching in actual send path
API key loading
API key UI
settings persistence
model selector real behavior
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

Shell changes are allowed only inside the existing AI Assistant Advanced / provider mode UI state.

Do not redesign the AI panel.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if current AI panel state is there
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project paths.

Do not add broad provider framework classes.

Do not modify DeepSeekRa2AiClient in this phase unless a test reveals an already-existing compile issue.

---

## 4. Provider Mode Contract

### 4.1 Provider modes

Supported UI modes:

```text
Mock
DeepSeek
```

Default:

```text
Mock
```

### 4.2 Mock mode

Mock mode behavior:

```text
1. Current AI-4C send flow remains unchanged.
2. Uses FakeRa2AiClient.
3. Does not require API key.
4. Does not require network.
5. Remains deterministic.
```

### 4.3 DeepSeek mode

AI-4E-1 may show DeepSeek as a selectable/visible option, but must not perform real send.

Acceptable behavior in this phase:

```text
Option A:
  DeepSeek option is visible but disabled, with text:
  "DeepSeek 后续接入；API Key 将通过 DEEPSEEK_API_KEY 环境变量读取。"

Option B:
  DeepSeek option can be selected for UI preview only, but Generate still uses Mock and shows "当前阶段仍使用 Mock。"
```

Preferred for safety:

```text
Option A: visible but disabled placeholder
```

Do not silently pretend to use DeepSeek.

---

## 5. Advanced UI Requirements

The Advanced area near the composer should show:

```text
Provider: Mock / DeepSeek
Model: Mock / DeepSeek 后续接入
Status: Mock 模式 / DeepSeek 未启用
```

It must not show:

```text
API key input
Save API key button
local settings file path
secret value
```

Required text:

```text
API Key 通过环境变量 DEEPSEEK_API_KEY 配置；本阶段不会读取或发送真实请求。
```

---

## 6. Required AutomationIds

Preserve existing:

```text
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
AiAssistant.GenerateButton
AiAssistant.PromptBox
AiAssistant.SafetyFooter
AiAssistant.ChatHistory
AiAssistant.ContextSummary
```

Add if needed:

```text
AiAssistant.ProviderSelector
AiAssistant.ProviderStatus
AiAssistant.DeepSeekEnvironmentHint
```

Do not add:

```text
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
AiAssistant.ApplyButton
```

---

## 7. Send Flow Boundary

Generate / Send must continue to use:

```text
FakeRa2AiClient
```

No matter what provider UI placeholder says in AI-4E-1.

Generate must not:

```text
read DEEPSEEK_API_KEY
create DeepSeekRa2AiClient
call network
switch to real provider
modify editor text
mark dirty
write files
```

---

## 8. Tests

Update/add boundary tests.

Required tests:

```text
1. Provider selector/status exists in Advanced area.
2. Mock is default provider/status.
3. No API key input AutomationId exists.
4. No save API key button exists.
5. Generate still uses fake send flow.
6. Generate does not read environment variables, if testable.
7. Generate does not call DeepSeek/network.
8. No Apply button exists.
9. Existing AI chat and Section tree AutomationIds remain.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 9. Validation Commands

Run full validation because Shell XAML / code-behind may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 10. Manual Smoke Checklist

After implementation:

```text
1. Launch IDE.
2. Open AI Assistant.
3. Open Advanced.
4. Confirm Provider/Model/Status are shown.
5. Confirm Mock is default.
6. Confirm no API key input exists.
7. Confirm no save API key button exists.
8. Send a prompt.
9. Confirm response still comes from fake pipeline.
10. Confirm no network/API key is used.
11. Confirm no document text changes and no dirty state.
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: AI-4E-1.
2. Files changed.
3. Provider mode UI state implemented.
4. Default provider behavior.
5. API key UI absence confirmation.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no live DeepSeek call added.
11. Confirmation no API key read/persistence added.
12. Confirmation no file modification behavior added.
13. Remaining risks.
14. Recommended next phase.
```
