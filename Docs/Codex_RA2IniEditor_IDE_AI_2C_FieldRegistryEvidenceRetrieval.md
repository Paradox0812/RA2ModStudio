# Codex Task: RA2IniEditor.IDE AI-2C Field Registry Evidence Retrieval

## 0. Current Baseline

AI-2B has been completed.

Reported state:

```text
AI-2B: bounded current-document / caret context provider completed.
Context Summary now displays current file, Section, Key/Value, caret line, nearby line count.
Generate still uses AI-1C deterministic mock response.
Field evidence count is fixed at 0.
Diagnostics count is fixed at 0.
Tests: 1306 passed.
IdeOnly package: passed, packaged file count 685.
No DeepSeek / network / API key / PromptBuilder / Apply / Insert.
No file modification behavior.
No Field Registry write behavior.
Legacy not restored.
```

AI-2B added / modified:

```text
RA2IniEditor.IDE/AI/Ra2AiContext.cs
RA2IniEditor.IDE/AI/Ra2AiContextRequest.cs
RA2IniEditor.IDE/AI/IRa2AiContextProvider.cs
RA2IniEditor.IDE/AI/Ra2CurrentDocumentAiContextProvider.cs
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2AiContextProviderTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Next phase:

```text
AI-2C: local Field Registry evidence retrieval
```

This is a limited implementation phase.

Do not connect DeepSeek.

Do not implement PromptBuilder.

Do not implement diagnostics summary yet.

---

## 1. Goal

Extend the AI context with bounded local Field Registry evidence.

The AI Assistant should be able to show a small list/count of field evidence derived from:

```text
current Key / Section from AI-2B caret context
selected INI snippet, if explicit
user prompt text, if available from the current AI input
```

The result must remain local, bounded, and advisory.

---

## 2. Hard Boundaries

Do not implement:

```text
DeepSeek client
network calls
API key configuration
PromptBuilder
diagnostics summary integration
AI apply / insert
file modification
Field Registry writes
whole-project context
auto-open AI
auto-send context
```

Do not modify:

```text
Field Registry loader / writer / apply / rollback / import / learning services
Field Registry priority semantics
parser semantics
diagnostics behavior
completion behavior
hover behavior
quick peek behavior
save preflight
BuiltIn field registry JSON
legacy files
solution / project files
```

This phase may read existing Field Registry provider state, but must not reload, write, import, apply, or mutate it.

---

## 3. Files Allowed

Allowed implementation files:

```text
RA2IniEditor.IDE/AI/Ra2AiContext.cs
RA2IniEditor.IDE/AI/Ra2AiContextRequest.cs
RA2IniEditor.IDE/AI/Ra2AiFieldEvidence.cs
RA2IniEditor.IDE/AI/IRa2AiFieldEvidenceProvider.cs
RA2IniEditor.IDE/AI/Ra2FieldRegistryAiEvidenceProvider.cs
RA2IniEditor.IDE/AI/Ra2CurrentDocumentAiContextProvider.cs
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2AiFieldEvidenceProviderTests.cs
RA2IniEditor.Tests/IDE/Ra2AiContextProviderTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project conventions and paths.

If the existing project has a better location for AI helpers, use that and report it.

Keep `ShellWindow.xaml.cs` limited to wiring; do not put retrieval logic there.

---

## 4. Existing Services to Reuse

Before editing, inspect existing field lookup paths and reuse the smallest safe path.

Likely existing components:

```text
IRa2FieldDefinitionProvider
CompositeRa2FieldDefinitionProvider
LocalRa2FieldDefinitionProvider
FieldRegistryRuntimeService
IFieldRegistryProvenanceProvider
FieldRegistryProvenanceSnapshot
Ra2CompletionProvider
Ra2HoverProvider
Ra2FieldQuickPeekService
```

The evidence provider should reuse existing field definition models and provider abstractions where possible.

Do not duplicate Field Registry loading logic.

Do not re-parse registry files.

---

## 5. Evidence Model

Add a small evidence model if needed.

Suggested model:

```csharp
internal sealed class Ra2AiFieldEvidence
{
    public string Key { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? SectionKind { get; init; }
    public string? ValueKind { get; init; }
    public string? Description { get; init; }
    public string? Example { get; init; }
    public string? SourceName { get; init; }
    public string? Provenance { get; init; }
    public double Score { get; init; }
}
```

Adjust names to the project style.

Evidence must be display/reference data only.

---

## 6. Retrieval Inputs

Use bounded inputs:

```text
current KeyName from Ra2AiContext
current SectionName / SectionKind if available
current ValueText, only as weak signal
explicit SelectedText, only if present
current AI prompt text from PromptBox
```

Do not use:

```text
whole document
whole project
entire registry
absolute file paths
```

---

## 7. Retrieval Rules

### 7.1 Current key exact match

If current caret context has `KeyName`, prioritize exact field lookup.

Expected:

```text
KeyName=Strength -> field evidence for Strength
```

### 7.2 Section-aware filtering

If existing field model supports section kinds, prefer matches compatible with current Section/section kind.

If section kind cannot be determined, return key evidence without pretending certainty.

### 7.3 Prompt keyword matching

For natural language prompt text, use simple local matching only.

Allowed:

```text
case-insensitive key match
display name match
alias match if model supports aliases
description keyword match
```

Avoid over-engineering semantic search in AI-2C.

No embeddings.

No network.

No DeepSeek.

### 7.4 Result bound

Return at most a small number of evidence items.

Recommended:

```text
Top 8
```

Hard cap:

```text
Top 12
```

Do not include entire registry.

### 7.5 Advisory wording

Evidence is advisory.

Do not use evidence as a hard legality gate.

Do not change diagnostics.

Do not change save behavior.

---

## 8. AI Context Integration

Extend `Ra2AiContext`:

```text
FieldEvidence
FieldEvidenceCount
```

or equivalent.

Update context summary display:

```text
字段依据：N
```

If compact UI allows, show short evidence list:

```text
字段依据：Strength, Armor, Primary
```

Do not add large evidence panels in AI-2C unless already simple.

No prompt sending yet.

---

## 9. UI Behavior

When AI context is refreshed / Generate is clicked:

```text
1. Build caret context as AI-2B.
2. Run local field evidence retrieval.
3. Update Context Summary with evidence count and possibly top keys.
4. Continue AI-1C mock response.
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
AiAssistant.FieldEvidenceSummary
AiAssistant.FieldEvidenceList
```

Do not add:

```text
AiAssistant.ApplyButton
```

---

## 11. Tests

### 11.1 Evidence provider tests

Required cases:

```text
1. Exact current key returns matching evidence.
2. Unknown key returns empty evidence safely.
3. Prompt keyword can match a field key.
4. Prompt keyword can match description/display text if available.
5. Result count is bounded.
6. Evidence retrieval does not mutate provider/registry state.
7. Evidence retrieval does not require file IO or registry reload.
8. BuiltIn/project/global evidence remains advisory.
```

### 11.2 Context provider integration tests

Required cases:

```text
1. AI context includes evidence for current key.
2. FieldEvidenceCount reflects bounded evidence.
3. No evidence when no key/prompt match.
4. Nearby text remains bounded.
5. Selected text remains explicit-only.
```

### 11.3 UI boundary tests

Required checks:

```text
1. AiAssistant.ContextSummary still exists.
2. Context Summary can display field evidence count.
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
1. Open a project and file.
2. Put caret on a known field such as Strength.
3. Open AI Assistant.
4. Generate mock response.
5. Confirm Context Summary shows field evidence count > 0.
6. Confirm evidence list/summary includes current key if UI shows it.
7. Put caret on an unknown field.
8. Confirm evidence count is 0 or safe fallback.
9. Type a prompt that mentions a known field keyword.
10. Confirm local evidence count updates if prompt matching is wired.
11. Confirm no network/API key/DeepSeek is used.
12. Confirm no file modifications and no dirty state.
```

---

## 14. Final Report Format

Report:

```text
1. Phase completed: AI-2C.
2. Files changed.
3. Evidence provider implementation summary.
4. Existing Field Registry provider path reused.
5. Context Summary changes.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no DeepSeek/network/API key added.
11. Confirmation no diagnostics summary implemented yet.
12. Confirmation no PromptBuilder implemented yet.
13. Confirmation no file modification behavior added.
14. Confirmation Field Registry semantics unchanged.
15. Remaining risks.
16. Recommended next phase.
```
