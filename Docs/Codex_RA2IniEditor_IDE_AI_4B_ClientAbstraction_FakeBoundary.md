# Codex Task: RA2IniEditor.IDE AI-4B AI Client Abstraction / Fake Boundary

## 0. Current Baseline

AI-4A has been completed.

Reported state:

```text
Docs/AiAssistantDeepSeekAdapterContract.md created.
Tests: 1336 passed.
IdeOnly package: passed, packaged file count 706.
No source code changed.
No DeepSeek / network / API key / Apply / Insert implemented.
Legacy not restored.
```

Next phase:

```text
AI-4B: AI client abstraction + fake/testable provider boundary
```

This is a limited source implementation phase.

Do not implement real DeepSeek network calls yet.

---

## 1. Goal

Introduce a small AI client abstraction and deterministic fake client so future DeepSeek integration can be tested safely.

Required result:

```text
1. Add IRa2AiClient or equivalent abstraction.
2. Add Ra2AiResponse / result model if needed.
3. Add deterministic FakeRa2AiClient.
4. Ensure clients consume Ra2AiRequest from AI-3B.
5. Fake client can represent success / cancellation / provider error / missing configuration.
6. No live network.
7. No API key configuration.
```

---

## 2. Hard Boundaries

Do not implement:

```text
DeepSeek HTTP client
real network calls
API key loading
API key UI
provider settings persistence
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

---

## 3. Files Allowed

Allowed implementation files:

```text
RA2IniEditor.IDE/AI/IRa2AiClient.cs
RA2IniEditor.IDE/AI/Ra2AiResponse.cs
RA2IniEditor.IDE/AI/Ra2AiResponseKind.cs, if needed
RA2IniEditor.IDE/AI/FakeRa2AiClient.cs
RA2IniEditor.Tests/IDE/Ra2AiClientTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project naming conventions.

Do not create a broad provider framework.

---

## 4. Required Interface Shape

Preferred minimal interface:

```csharp
internal interface IRa2AiClient
{
    Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken);
}
```

Requirements:

```text
1. Input is Ra2AiRequest from AI-3B.
2. Output is response text plus status/error metadata.
3. CancellationToken is required.
4. No UI types in interface.
5. No DeepSeek-specific fields in interface.
6. No file/project access.
```

---

## 5. Response Model

The response model must support at least:

```text
success text
cancelled
provider error
missing configuration
```

Suggested:

```csharp
internal enum Ra2AiResponseKind
{
    Success,
    Cancelled,
    ProviderError,
    MissingConfiguration
}

internal sealed class Ra2AiResponse
{
    public Ra2AiResponseKind Kind { get; init; }
    public string Text { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public bool IsSuccess => Kind == Ra2AiResponseKind.Success;
}
```

Adapt to project style.

Do not include raw request payload or API key fields in errors.

---

## 6. Fake Client Requirements

The fake client should be deterministic.

Allowed behavior:

```text
1. Return fixed success response for normal requests.
2. Echo a safe summary such as intent or prompt length if useful.
3. Support configured fake error mode for tests.
4. Respect pre-cancelled CancellationToken.
```

Forbidden:

```text
network
file IO
environment variable reads
API key reads
Field Registry reads
diagnostics reads
prompt building
```

---

## 7. UI Integration Boundary

AI-4B does not need to wire the fake client into the AI panel.

Preferred:

```text
Implement abstraction and tests only.
Keep UI behavior from AI-1C unchanged.
```

Full AI panel send flow belongs to a later phase.

---

## 8. Tests

Add focused tests:

```text
1. Fake client returns deterministic success response.
2. Fake client consumes Ra2AiRequest without reading files.
3. Fake client respects pre-cancelled CancellationToken.
4. Fake client can return configured provider error.
5. Fake client can return configured missing configuration error.
6. Response/error text does not expose API keys.
7. Interface has CancellationToken parameter.
8. No test requires network or real DeepSeek.
```

---

## 9. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-4B.
2. Files changed.
3. AI client abstraction added.
4. Fake client behavior.
5. Commands run.
6. Build result.
7. Test result.
8. Package result.
9. Confirmation no DeepSeek/network/API key added.
10. Confirmation no UI send-flow real provider integration.
11. Confirmation no file modification behavior added.
12. Recommended next phase.
```
