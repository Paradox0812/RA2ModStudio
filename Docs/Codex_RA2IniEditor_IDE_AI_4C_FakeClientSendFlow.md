# Codex Task: RA2IniEditor.IDE AI-4C Fake Client Send Flow Wiring

## 0. Current Baseline

AI-4B has been completed.

Reported state:

```text
IRa2AiClient added.
Ra2AiResponseKind added.
Ra2AiResponse added.
FakeRa2AiClient added.
Ra2AiClientTests added.
Tests: 1343 passed.
IdeOnly package: passed, packaged file count 712.
No DeepSeek / network / API key.
No UI send-flow real provider integration.
No Apply / Insert.
No file modification behavior.
Shell UI unchanged.
Legacy not restored.
```

AI-1C currently still uses local mock chat behavior directly from the AI panel.

Next recommended phase:

```text
AI-4C: Wire AI panel send flow to PromptBuilder + FakeRa2AiClient
```

This is a limited implementation phase.

Do not implement DeepSeek.

---

## 1. Goal

Replace the AI panel's ad-hoc local mock response path with the real internal AI pipeline using the fake client:

```text
PromptBox
  -> current bounded AI context
  -> Ra2AiPromptBuilder
  -> FakeRa2AiClient
  -> chat history response
```

The visible behavior may remain similar to AI-1C, but the internal flow should now validate the real architecture.

Required result:

```text
1. Generate / Send builds bounded context using AI-2B / AI-2C / AI-2D.
2. Generate / Send builds prompt using AI-3B PromptBuilder.
3. Generate / Send calls FakeRa2AiClient through IRa2AiClient.
4. Response is appended to chat history.
5. Error/cancel states are locally handled, even if fake-only.
6. No DeepSeek / network / API key is added.
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
real model selector behavior
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

Shell changes are allowed only to wire the AI panel to already existing AI services.

Do not redesign AI panel layout in this phase.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if existing AI panel state is there
RA2IniEditor.IDE/AI/FakeRa2AiClient.cs, only if minimal behavior adjustment is needed
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2AiClientTests.cs, only if fake behavior needs additional tests
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed if current architecture requires simple composition helper:

```text
RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs
RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs
```

Only add this if it keeps ShellWindow.xaml.cs from growing too much.

Do not create a broad framework.

---

## 4. Required Implementation Strategy

### 4.1 Use existing services

The send flow must reuse:

```text
Ra2CurrentDocumentAiContextProvider
Ra2FieldRegistryAiEvidenceProvider
Ra2CurrentFileAiDiagnosticSummaryProvider
Ra2AiPromptBuilder
IRa2AiClient
FakeRa2AiClient
```

Do not duplicate context building, evidence retrieval, diagnostics summary, or prompt building inside ShellWindow.xaml.cs.

### 4.2 Shell wiring

ShellWindow.xaml.cs may:

```text
1. Gather current editor/caret/selection state.
2. Call existing context provider.
3. Call PromptBuilder.
4. Call IRa2AiClient.
5. Append response/error message to local AI chat UI.
```

ShellWindow.xaml.cs must not:

```text
1. Build prompt text manually.
2. Query Field Registry directly outside existing AI provider wiring.
3. Re-run diagnostics directly.
4. Perform file IO.
5. Access environment variables.
6. Contain DeepSeek-specific code.
```

### 4.3 Fake client only

The AI client instance must be FakeRa2AiClient.

No real provider selection yet.

The Advanced model selector remains placeholder only.

---

## 5. UI Behavior

### 5.1 Success

On non-empty prompt:

```text
1. append user message
2. build context
3. build prompt
4. call fake client
5. append fake assistant response
6. clear PromptBox if current behavior already does so
```

The fake assistant response may mention:

```text
本地 Fake AI 回复
已构建上下文和 prompt
不会连接 DeepSeek 或修改文件
```

### 5.2 Empty prompt

No message should be added.

### 5.3 Error states

If FakeRa2AiClient is configured or composed to return error state:

```text
ProviderError -> append visible error-style assistant message
MissingConfiguration -> append visible configuration error message
Cancelled -> append cancellation state or no-op, but must clear busy state
```

AI-4C does not need UI controls to trigger fake errors unless tests use internal pipeline tests.

### 5.4 Busy / Cancel

If async flow is added:

```text
1. busy state should prevent duplicate sends
2. cancel should cancel the in-flight fake call if cancellable
```

But do not over-engineer fake delays.

---

## 6. Safety Requirements

Generate must not:

```text
modify editor text
mark document dirty
write files
write Field Registry
call network
read API key
upload project
auto-apply generated content
```

The AI response remains draft/advisory.

No Apply button.

---

## 7. Tests

### 7.1 Pipeline / Shell tests

Required tests:

```text
1. non-empty prompt builds prompt and returns fake response
2. empty prompt does not add messages
3. fake client success is displayed in chat
4. fake provider error is displayed as error state, if pipeline testable
5. missing configuration state is displayed as error state, if pipeline testable
6. Generate does not modify source editor text
7. Generate does not mark document dirty, if observable
8. No DeepSeek/network/API key is required
9. No Apply button exists
10. Section Tree remains default view
```

### 7.2 PromptBuilder integration

If testable:

```text
1. Fake client receives Ra2AiRequest built by PromptBuilder.
2. Request PromptText includes Application Rules and Current IDE Context.
3. The send flow does not manually create a prompt bypassing PromptBuilder.
```

Avoid pixel-perfect tests.

---

## 8. Validation Commands

Run full validation because Shell wiring may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Launch IDE.
2. Open a project/file.
3. Put caret on a known field.
4. Open AI Assistant.
5. Enter prompt and send.
6. Confirm user message appears.
7. Confirm fake assistant response appears.
8. Confirm Context Summary still shows current context / evidence / diagnostics.
9. Confirm no DeepSeek/network/API key is used.
10. Confirm no document text changes.
11. Confirm no dirty state is created.
12. Confirm Section tree remains usable.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-4C.
2. Files changed.
3. Send flow wiring summary.
4. How PromptBuilder and FakeRa2AiClient are used.
5. Commands run.
6. Build result.
7. Test result.
8. Package result.
9. Confirmation no DeepSeek/network/API key added.
10. Confirmation no file modification behavior added.
11. Confirmation no Apply/Insert added.
12. Confirmation Section tree behavior preserved.
13. Manual smoke steps or result.
14. Remaining risks.
15. Recommended next phase.
```
