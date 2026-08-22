# Codex Task: RA2IniEditor.IDE AI-6D PromptBuilder Conversation Context / Current Subject Integration

## 0. Current Baseline

AI-6C has been completed.

Reported state:

```text
AI-6B: bounded conversation context model / extraction completed.
AI-6C: current subject / draft subject extraction completed.
Ra2AiCurrentSubjectExtractor can infer candidate subject metadata from current-session assistant draft text.
Main unit draft is prioritized over weapon / warhead / projectile follow-up definitions.
Extracted subject is marked draft/advisory and not applied file state.
Tests: 1422 passed.
IdeOnly package: passed, packaged file count 760.
No PromptBuilder integration yet.
No send-flow/UI change.
No Apply / Insert / file modification behavior.
Legacy not restored.
```

Next phase:

```text
AI-6D: PromptBuilder integration for Conversation Context and Current Subject
```

This is a limited source implementation phase.

---

## 1. Goal

Integrate AI-6B conversation context and AI-6C current subject into the prompt-building pipeline.

The PromptBuilder should include two new clearly separated prompt sections:

```text
Conversation Context
Current Subject
```

These must be separate from:

```text
Current IDE Context
Field Registry Evidence
Diagnostics Summary
```

This phase should let the model understand references such as:

```text
这个单位
刚才那个武器
在这个基础上
继续修改
把它改成苏军单位
```

without treating prior assistant drafts as applied project files.

---

## 2. Hard Boundaries

Do not implement:

```text
Apply / Insert
file modification
Field Registry writes
whole-project context
unbounded chat history
cross-session memory
hidden memory
settings persistence
DeepSeek adapter changes
provider selection changes
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
BuiltIn Field Registry JSON
legacy files
solution / project files
```

Do not change:

```text
Field Registry evidence advisory semantics
Diagnostics advisory semantics
AI output draft/advisory semantics
PromptBuilder bounded-context-only rule
```

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/AI/Ra2AiPromptBuildRequest.cs
RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs
RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs, if current pipeline needs to pass extra context
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs, only for minimal wiring from current chat messages to providers
RA2IniEditor.Tests/IDE/Ra2AiPromptBuilderTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs, if pipeline is changed
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs, only if source-boundary wiring tests need updating
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if a small request/composition model keeps code cleaner:

```text
RA2IniEditor.IDE/AI/Ra2AiAssistantPipelineRequest.cs
```

Do not modify XAML in this phase unless a compile break requires a trivial fix.

---

## 4. Required Design

### 4.1 Prompt build request

Extend `Ra2AiPromptBuildRequest` with optional context inputs:

```text
ConversationContext
CurrentSubject
```

Suggested conceptual shape:

```csharp
internal sealed class Ra2AiPromptBuildRequest
{
    public Ra2AiIntent Intent { get; init; } = Ra2AiIntent.Auto;
    public string UserPrompt { get; init; } = string.Empty;
    public Ra2AiContext Context { get; init; }
    public Ra2AiConversationContext? ConversationContext { get; init; }
    public Ra2AiCurrentSubject? CurrentSubject { get; init; }
}
```

Adapt to current project style.

### 4.2 Prompt sections

`Ra2AiPromptBuilder` must add sections:

```text
Conversation Context
Current Subject
```

Conversation Context section must state:

```text
This is recent visible chat context from the current AI Assistant session.
It is bounded and may be truncated.
It is not hidden memory.
Assistant messages are draft/advisory text, not applied file state.
```

Current Subject section must state:

```text
This is the current discussed subject inferred from conversation or current IDE context.
If Source=LastAssistantDraft, treat it as a prior draft, not project file state.
Do not assume it exists in rulesmd.ini / artmd.ini unless the user explicitly says it was applied.
```

### 4.3 Ordering

Recommended prompt order:

```text
Application Rules
User Request
Current Subject
Conversation Context
Current IDE Context
Field Registry Evidence
Diagnostics Summary
Output Requirements
```

Reason:

```text
Current Subject helps resolve "这个单位" before the model reads IDE/evidence details.
```

### 4.4 Bounded context only

PromptBuilder must only consume:

```text
Ra2AiPromptBuildRequest.UserPrompt
Ra2AiPromptBuildRequest.Intent
Ra2AiPromptBuildRequest.Context
Ra2AiPromptBuildRequest.ConversationContext
Ra2AiPromptBuildRequest.CurrentSubject
```

PromptBuilder must not:

```text
read files
query providers
rerun diagnostics
read environment variables
inspect editor controls
read chat UI directly
```

---

## 5. Send Flow Wiring

If current AI panel send flow calls `Ra2AiAssistantPipeline`, wire it so that:

```text
1. Shell gathers current visible AI chat messages.
2. Ra2AiConversationContextProvider extracts bounded conversation context.
3. Ra2AiCurrentSubjectExtractor extracts current subject.
4. Pipeline passes both into PromptBuilder.
5. Provider call still uses selected Mock / DeepSeek provider as already implemented.
```

ShellWindow.xaml.cs must not:

```text
manually build Conversation Context prompt text
manually infer subject in UI code
parse draft section IDs directly
```

Shell may only pass chat message text/roles to the providers.

If full send-flow wiring becomes too large, stop after PromptBuilder support and tests, then report that a separate AI-6D-2 wiring phase is needed.

---

## 6. Draft State Rules

Prompt must explicitly preserve these rules:

```text
1. Prior assistant draft is conversation draft only.
2. Prior assistant draft is not applied file state.
3. Do not assume extracted subject exists in project files.
4. Do not claim modifications have been applied.
5. If user asks to modify "this unit", use Current Subject to resolve reference but still output draft.
```

---

## 7. Tests

### 7.1 PromptBuilder tests

Required tests:

```text
1. Prompt includes Conversation Context section when provided.
2. Prompt includes Current Subject section when provided.
3. Prompt states assistant draft is not applied file state.
4. Prompt includes SubjectId and SubjectKind when available.
5. Prompt can resolve "这个单位" context via Current Subject wording.
6. Prompt states conversation context is bounded and current-session only.
7. Prompt does not include hidden/provider metadata.
8. Existing Field Registry evidence / diagnostics / draft-output tests still pass.
9. PromptBuilder still does not require DeepSeek / network / API key.
```

### 7.2 Pipeline tests, if wiring is changed

Required tests:

```text
1. Send flow passes bounded conversation context to PromptBuilder.
2. Send flow passes extracted current subject to PromptBuilder.
3. Prior assistant draft remains marked as draft/advisory.
4. Generate does not modify editor text.
5. Generate does not mark dirty state, if observable.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 8. Validation Commands

Run full validation because source/tests may change:

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
1. Open AI Assistant.
2. Generate a unit draft such as [LAAV].
3. Ask: 在这个单位基础上，把它改成苏军单位。
4. Confirm model understands "这个单位" as the prior draft subject.
5. Confirm it does not claim the draft already exists in project files.
6. Confirm output remains draft/advisory.
7. Confirm no editor text changes and no dirty state.
8. Confirm no Apply / Insert button exists.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-6D.
2. Files changed.
3. PromptBuilder integration summary.
4. Conversation Context section behavior.
5. Current Subject section behavior.
6. Send-flow wiring summary, if implemented.
7. Tests added/updated.
8. Commands run.
9. Build result.
10. Test result.
11. Package result.
12. Confirmation no Apply/Insert/file modification behavior added.
13. Confirmation no provider/DeepSeek behavior changed.
14. Manual smoke steps or result.
15. Remaining risks.
16. Recommended next phase.
```
