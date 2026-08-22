# Codex Task: RA2IniEditor.IDE AI-4D-Fix Environment-only DeepSeek API Key Policy

## 0. Current Baseline

AI-4D has been completed.

Reported state:

```text
DeepSeekRa2AiClient implemented.
DeepSeekRa2AiClientOptions implemented.
DeepSeekRa2AiClientTests implemented with fake HttpMessageHandler.
AI panel still uses FakeRa2AiClient by default.
No UI provider switching.
No real API key in tests.
No Apply / Insert.
No file modification.
Tests: 1357 passed.
IdeOnly package: passed, packaged file count 719.
```

Current implementation summary:

```text
DeepSeekRa2AiClientOptions contains:
- BaseUrl
- ApiKey
- Model
- Timeout
- Temperature

API key is passed through options.
No environment-variable loading currently implemented.
No settings persistence currently implemented.
No API key UI currently implemented.
```

User decision:

```text
API Key should not be configured in Advanced UI or local project/user settings.
API Key should be supplied through environment variables.
```

This task updates the DeepSeek API key policy and related implementation/docs accordingly.

---

## 1. Goal

Make the project policy explicit:

```text
DeepSeek API Key is environment-variable only.
```

The adapter should remain testable through options, but production/live construction must come from environment variables through a small factory/helper.

Required environment variables:

```text
DEEPSEEK_API_KEY
```

Optional environment variables:

```text
DEEPSEEK_BASE_URL
DEEPSEEK_MODEL
DEEPSEEK_TIMEOUT_SECONDS
```

Default values:

```text
BaseUrl = https://api.deepseek.com
Model = deepseek-v4-pro, unless project/user later changes approved default
Timeout = 60 seconds
Temperature = 0.2
```

---

## 2. Important Design Clarification

`DeepSeekRa2AiClientOptions.ApiKey` may remain for testability and explicit object construction.

But the application must not store API keys in:

```text
source files
project files
repository docs with real values
local persisted settings
UI settings
package output
logs
exception messages
```

The app-facing production creation path should be:

```text
Environment variables -> DeepSeekRa2AiClientOptions -> DeepSeekRa2AiClient
```

Not:

```text
Advanced UI -> saved key
project config file -> saved key
repository file -> saved key
```

---

## 3. Files Allowed

Allowed source files:

```text
RA2IniEditor.IDE/AI/DeepSeekRa2AiClientOptions.cs
RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs
RA2IniEditor.IDE/AI/DeepSeekRa2AiClient.cs, only if needed for options compatibility
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientFactoryTests.cs
Docs/AiAssistantDeepSeekAdapterContract.md
Docs/AiAssistantSafetyContract.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project paths.

Do not modify Shell UI in this task.

---

## 4. Files / Areas Forbidden

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
AI panel provider selector
Advanced UI
model selector real behavior
provider settings persistence
project files
solution files
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn field registry JSON
legacy files
```

Do not implement:

```text
API key UI
local settings persistence
encrypted credential store
DeepSeek live send-flow wiring
Apply / Insert
file modification
Field Registry writes
whole-project context
auto-send context
diagnostic auto-fix
```

---

## 5. Required Implementation

### 5.1 Add / update DeepSeekRa2AiClientFactory

Implement a small factory/helper if not already present.

Suggested shape:

```csharp
internal static class DeepSeekRa2AiClientFactory
{
    public static DeepSeekRa2AiClientOptions CreateOptionsFromEnvironment();
}
```

or:

```csharp
internal static class DeepSeekRa2AiClientFactory
{
    public static DeepSeekRa2AiClient CreateFromEnvironment(HttpClient httpClient);
}
```

Prefer returning options if that keeps tests simpler.

The factory may read only:

```text
DEEPSEEK_API_KEY
DEEPSEEK_BASE_URL
DEEPSEEK_MODEL
DEEPSEEK_TIMEOUT_SECONDS
```

It must not read:

```text
files
project settings
user settings
registry
clipboard
other environment variables unrelated to DeepSeek
```

### 5.2 Missing API key

If `DEEPSEEK_API_KEY` is missing or whitespace:

```text
Options.ApiKey should be null/empty.
DeepSeekRa2AiClient.SendAsync should return MissingConfiguration as AI-4D already supports.
```

Do not throw from environment factory solely because API key is missing.

### 5.3 Base URL

If `DEEPSEEK_BASE_URL` is missing:

```text
use https://api.deepseek.com
```

If it is invalid:

```text
fall back to default or return invalid configuration based on current options validation style
```

Prefer deterministic testable behavior.

### 5.4 Model

If `DEEPSEEK_MODEL` is missing:

```text
use configured default model
```

Do not hard-code deprecated model names.

### 5.5 Timeout

If `DEEPSEEK_TIMEOUT_SECONDS` is missing:

```text
use 60 seconds
```

If invalid:

```text
fall back to default or invalid configuration based on current options validation style
```

Do not crash during factory construction.

---

## 6. Documentation Updates

Update documentation to remove or demote previous wording that allowed user-local settings files as an API key source.

Docs must state:

```text
First implementation policy:
- API key is environment-variable only.
- No Advanced UI API key input.
- No local settings persistence.
- No repository/project config key storage.
```

Recommended wording:

```text
DeepSeek API Key is read from DEEPSEEK_API_KEY.
DEEPSEEK_BASE_URL and DEEPSEEK_MODEL may optionally override provider endpoint/model.
The AI Assistant Advanced area may display provider/model/status later, but must not collect or save API keys.
```

---

## 7. Tests

Add/update tests for:

```text
1. Factory reads DEEPSEEK_API_KEY into options.
2. Factory uses default BaseUrl when DEEPSEEK_BASE_URL is missing.
3. Factory uses DEEPSEEK_BASE_URL when valid.
4. Factory uses default model when DEEPSEEK_MODEL is missing.
5. Factory uses DEEPSEEK_MODEL when present.
6. Factory handles missing API key without throwing.
7. Factory does not include API key in ToString / debug output.
8. DeepSeekRa2AiClient still maps missing API key to MissingConfiguration.
9. No test uses real API key.
```

Important testing note:

```text
Environment variable tests must isolate and restore previous environment values.
```

Do not make tests depend on user's real environment.

---

## 8. Validation Commands

Run full validation because source and tests may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Manual Smoke / User Setup Note

No UI smoke is required because AI panel is not switched to DeepSeek yet.

For future live use, user will configure:

```powershell
[Environment]::SetEnvironmentVariable("DEEPSEEK_API_KEY", "<your-key>", "User")
[Environment]::SetEnvironmentVariable("DEEPSEEK_BASE_URL", "https://api.deepseek.com", "User")
[Environment]::SetEnvironmentVariable("DEEPSEEK_MODEL", "deepseek-v4-pro", "User")
```

Do not include real key in docs or tests.

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-4D-Fix.
2. Files changed.
3. API key policy changes.
4. Environment variables supported.
5. Tests added/updated.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no API key UI/settings persistence added.
11. Confirmation no real provider switching added.
12. Confirmation no file modification behavior added.
13. Remaining risks.
14. Recommended next phase.
```
