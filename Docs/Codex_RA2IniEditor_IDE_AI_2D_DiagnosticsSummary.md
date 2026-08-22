# Codex Task: RA2IniEditor.IDE AI-2D Diagnostics Summary Integration

## 0. Current Baseline

AI-2C has been completed.

Reported state:

```text
AI-2B: bounded current-document / caret context provider completed.
AI-2C: local Field Registry evidence retrieval completed.
Context Summary now displays current file, Section, Key/Value, caret line, nearby line count, field evidence count, and top evidence keys.
Generate still uses AI-1C deterministic mock response.
Tests: 1316 passed.
IdeOnly package: passed, packaged file count 690.
No DeepSeek / network / API key / PromptBuilder / Apply / Insert.
No file modification behavior.
No Field Registry write behavior.
Legacy not restored.
```

AI-2C reused:

```text
FieldRegistryRuntimeService.CurrentProvider
FieldRegistryRuntimeService.CurrentProvenanceProvider
IRa2FieldDefinitionProvider
IFieldRegistryProvenanceProvider
```

Next phase:

```text
AI-2D: Diagnostics Summary Integration
```

This is a limited implementation phase.

Do not connect DeepSeek.

Do not implement PromptBuilder.

Do not change diagnostics behavior.

---

## 1. Goal

Extend the AI context with a bounded diagnostics summary.

The AI Assistant should be able to show a small count and summary of diagnostics relevant to the current context:

```text
current caret line
current key/value
current Section
current file, bounded summary only
```

The diagnostics summary must be read-only and advisory.

It must not rerun diagnostics in a way that changes existing IDE behavior.

---

## 2. Hard Boundaries

Do not implement:

```text
DeepSeek client
network calls
API key configuration
PromptBuilder
AI apply / insert
file modification
Field Registry writes
whole-project context
auto-open AI
auto-send context
diagnostic auto-fix
```

Do not modify:

```text
diagnostics generation semantics
Field Registry services
Field Registry priority semantics
parser semantics
completion behavior
hover behavior
quick peek behavior
save preflight
BuiltIn field registry JSON
legacy files
solution / project files
```

This phase may read existing diagnostics state or call existing readonly diagnostic summary paths, but must not change diagnostic rules.

---

## 3. Files Allowed

Allowed implementation files:

```text
RA2IniEditor.IDE/AI/Ra2AiContext.cs
RA2IniEditor.IDE/AI/Ra2AiContextRequest.cs
RA2IniEditor.IDE/AI/Ra2AiDiagnosticSummary.cs
RA2IniEditor.IDE/AI/IRa2AiDiagnosticSummaryProvider.cs
RA2IniEditor.IDE/AI/Ra2CurrentFileAiDiagnosticSummaryProvider.cs
RA2IniEditor.IDE/AI/Ra2CurrentDocumentAiContextProvider.cs
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2AiDiagnosticSummaryProviderTests.cs
RA2IniEditor.Tests/IDE/Ra2AiContextProviderTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project conventions and paths.

If the existing project has a better location for AI helpers, use that and report it.

Keep `ShellWindow.xaml.cs` limited to wiring; do not put diagnostic summary logic there.

---

## 4. Existing Services to Reuse

Before editing, inspect existing diagnostics paths and reuse the smallest safe path.

Likely existing components:

```text
CurrentFileReadonlyDiagnosticService
Ra2FieldDiagnosticService
IssuesViewModel
IdeDiagnosticIssueViewModel
Ra2DocumentSnapshot
Ra2DocumentSemanticModel
Ra2CaretContextService
```

Preferred strategy:

```text
1. Use existing current diagnostics or readonly diagnostic service output if already available.
2. Summarize diagnostics rather than duplicating diagnostic rule logic.
3. Do not invent a parallel diagnostics engine.
4. Do not change existing diagnostics severity, messages, or triggers.
```

---

## 5. Diagnostic Summary Model

Add a small summary model if needed.

Suggested model:

```csharp
internal sealed class Ra2AiDiagnosticSummary
{
    public string Severity { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int? LineNumber { get; init; }
    public string? SectionName { get; init; }
    public string? KeyName { get; init; }
    public string? Source { get; init; }
}
```

Adjust names to project style.

Summaries must be display/reference data only.

---

## 6. Summary Inputs

Use bounded inputs:

```text
current caret line
current SectionName
current KeyName
current document diagnostics already available
```

Do not use:

```text
whole project diagnostics
all files
all historical issues
unbounded Issues panel dump
```

---

## 7. Summary Rules

### 7.1 Relevance priority

Return diagnostics in this priority order:

```text
1. diagnostics on current caret line
2. diagnostics for current key
3. diagnostics inside current Section
4. small top current-file summary
```

### 7.2 Result bound

Return at most a small number of diagnostic summaries.

Recommended:

```text
Top 5
```

Hard cap:

```text
Top 8
```

Do not include full Issues panel contents.

### 7.3 Advisory wording

Diagnostics summary is advisory.

Do not use diagnostics summary to block save, alter Save Preflight, alter Issues, or auto-fix files.

---

## 8. AI Context Integration

Extend `Ra2AiContext`:

```text
Diagnostics
DiagnosticCount
```

or equivalent.

Update context summary display:

```text
诊断：N
```

If compact UI allows, show short summary such as:

```text
诊断：2（当前行 1，当前 Section 1）
```

Do not add large diagnostics panels in AI-2D.

No prompt sending yet.

---

## 9. UI Behavior

When AI context is refreshed / Generate is clicked:

```text
1. Build caret context as AI-2B.
2. Run local field evidence retrieval as AI-2C.
3. Read/summarize current diagnostics.
4. Update Context Summary with diagnostics count.
5. Continue AI-1C mock response.
```

Do not call DeepSeek.

Do not send context anywhere.

Do not write files.

---

## 10. AutomationIds

Preserve existing:

```text
AiAssistant.ContextSummary
AiAssistant.ChatHistory
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.SafetyFooter
```

Optional additions:

```text
AiAssistant.DiagnosticsSummary
```

Do not add:

```text
AiAssistant.ApplyButton
```

---

## 11. Tests

### 11.1 Diagnostic summary provider tests

Required cases:

```text
1. Current line diagnostic is included first.
2. Current key diagnostic is included.
3. Current Section diagnostic is included.
4. Result count is bounded.
5. No diagnostics returns empty summary safely.
6. Summary does not mutate IssuesViewModel or diagnostics collection.
7. Summary does not trigger file writes.
8. Summary does not require whole-project scan.
```

### 11.2 Context provider integration tests

Required cases:

```text
1. AI context includes bounded diagnostic summaries.
2. DiagnosticCount reflects bounded summaries.
3. Field evidence behavior from AI-2C still works.
4. Nearby text remains bounded.
5. Selected text remains explicit-only.
```

### 11.3 UI boundary tests

Required checks:

```text
1. AiAssistant.ContextSummary still exists.
2. Context Summary can display diagnostics count.
3. Generate still does not modify source editor text.
4. Generate still does not mark dirty state, if observable.
5. No Apply button exists.
6. Section Tree remains default view.
```

Avoid pixel-perfect tests.

Do not require DeepSeek or network.

---

## 12. Validation Commands

Run full validation because source and Shell wiring may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 13. Manual Smoke Checklist

After implementation:

```text
1. Open a project and file with known diagnostics.
2. Put caret on or near a diagnostic line.
3. Open AI Assistant.
4. Generate mock response.
5. Confirm Context Summary shows diagnostic count > 0.
6. Move caret to a clean line.
7. Confirm diagnostics count becomes 0 or safe fallback on next explicit generate/context refresh.
8. Confirm field evidence from AI-2C still works.
9. Confirm no network/API key/DeepSeek is used.
10. Confirm no file modifications and no dirty state.
```

---

## 14. Final Report Format

Report:

```text
1. Phase completed: AI-2D.
2. Files changed.
3. Diagnostic summary provider implementation summary.
4. Existing diagnostics path reused.
5. Context Summary changes.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no DeepSeek/network/API key added.
11. Confirmation no PromptBuilder implemented yet.
12. Confirmation no file modification behavior added.
13. Confirmation diagnostics semantics unchanged.
14. Confirmation Field Registry semantics unchanged.
15. Remaining risks.
16. Recommended next phase.
```
