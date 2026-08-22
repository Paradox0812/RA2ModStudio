# Codex Task: RA2IniEditor.IDE AI-2B Caret-based Current Context Provider / Mock UI Summary

## 0. Current Baseline

AI-2A has been completed.

Reported state:

```text
Docs/AiAssistantContextProviderContract.md created.
No source/XAML/ViewModel/test/script/project files changed.
Tests: 1299 passed.
IdeOnly package: passed, packaged file count 679.
Shell unchanged.
Field Registry semantics unchanged.
Legacy not restored.
```

AI-2A inspected and documented existing paths:

```text
Ra2DocumentSnapshot
Ra2DocumentSemanticModel
Ra2DocumentSemanticModelBuilder
Ra2CaretContextService
Ra2SectionSymbol
Ra2KeyValueSymbol
CurrentFileReadonlyDiagnosticService
Ra2FieldDiagnosticService
IssuesViewModel
IdeDiagnosticIssueViewModel
FieldRegistryRuntimeService
IRa2FieldDefinitionProvider
CompositeRa2FieldDefinitionProvider
LocalRa2FieldDefinitionProvider
IFieldRegistryProvenanceProvider
FieldRegistryProvenanceSnapshot
Ra2CompletionProvider
Ra2HoverProvider
Ra2FieldQuickPeekService
```

Next phase:

```text
AI-2B: Caret-based Current Context Provider / Mock UI Summary
```

This is the first source implementation phase for AI context.

Do not connect DeepSeek.

Do not implement Field Registry retrieval yet.

Do not implement PromptBuilder yet.

---

## 1. Goal

Implement a bounded current-document / caret context provider for the AI Assistant.

The context provider should derive a small explainable context package from the current editor state:

```text
current document display name
caret offset / line
current Section
current Key / Value
explicit selected text, if available
nearby bounded text lines
```

Then show a mock Context Summary in the AI Assistant panel.

This phase must not collect whole-project context and must not send anything to a model.

---

## 2. Hard Boundaries

Do not implement:

```text
DeepSeek client
network calls
API key configuration
real prompt builder
Field Registry retrieval evidence
diagnostic summary integration
AI apply / insert
file modification
Field Registry writes
whole-project context
auto-open AI
auto-send context
```

Do not modify:

```text
Field Registry services
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

Shell changes are allowed only to wire the AI panel Context Summary to the bounded current-caret context.

---

## 3. Files Allowed

Allowed implementation files:

```text
RA2IniEditor.IDE/AI/Ra2AiContext.cs
RA2IniEditor.IDE/AI/Ra2AiContextRequest.cs
RA2IniEditor.IDE/AI/IRa2AiContextProvider.cs
RA2IniEditor.IDE/AI/Ra2CurrentDocumentAiContextProvider.cs
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs
RA2IniEditor.Tests/IDE/Ra2AiContextProviderTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project structure. If the project already has a better AI folder convention from AI-1C, follow it.

If adding new source files is unnecessary, keep implementation smaller, but do not keep complex context logic inside `ShellWindow.xaml.cs`.

---

## 4. Required Design

### 4.1 Context request

A request should describe what to collect.

Suggested model:

```csharp
internal sealed class Ra2AiContextRequest
{
    public int CaretOffset { get; init; }
    public string? SelectedText { get; init; }
    public int NearbyLineRadius { get; init; } = 5;
    public bool IncludeNearbyText { get; init; } = true;
}
```

Adjust to project style if needed.

### 4.2 Context result

Suggested model:

```csharp
internal sealed class Ra2AiContext
{
    public string? DocumentDisplayName { get; init; }
    public int CaretOffset { get; init; }
    public int LineNumber { get; init; }
    public string? SectionName { get; init; }
    public string? KeyName { get; init; }
    public string? ValueText { get; init; }
    public string? SelectedText { get; init; }
    public string NearbyText { get; init; } = string.Empty;
    public int NearbyLineCount { get; init; }
}
```

Keep it small.

Do not include field evidence or diagnostics in AI-2B. Those belong to AI-2C / AI-2D.

### 4.3 Provider behavior

The provider should:

```text
1. Use current document snapshot / semantic model / caret context services where available.
2. Derive current Section / Key / Value from caret offset.
3. Include selected text only if explicitly selected.
4. Include bounded nearby lines only.
5. Avoid whole file/project context.
6. Avoid absolute local paths.
7. Return safe fallback context if no document or no caret context is available.
```

### 4.4 Nearby text rule

Default:

```text
5 lines before caret line
current line
5 lines after caret line
```

Bound the output by line count and reasonable character limit.

Do not include entire file.

---

## 5. AI Panel Integration

Update the AI Assistant Context Summary area to display this context.

Required existing AutomationId:

```text
AiAssistant.ContextSummary
```

Suggested visible summary:

```text
上下文：当前文件 rulesmd.ini；Section [HTNK]；字段 Strength=400；附近行 11 行；字段依据 0；诊断 0。
```

For this phase:

```text
字段依据: 0
诊断: 0
```

because Field Registry retrieval and diagnostics summary are deferred.

If no document context exists:

```text
上下文：当前没有可用的编辑器上下文。
```

No model request is sent.

---

## 6. Generate Behavior

AI-1C mock generation can remain.

AI-2B may optionally include the context summary in the mock response, but only as local deterministic text.

Do not collect context automatically on caret movement.

Allowed:

```text
When user opens AI panel or clicks Generate, update context summary from current caret.
```

Preferred for safety:

```text
Update context summary on explicit Generate / Refresh context action only.
```

If no refresh button exists, Generate may update local summary and still produce mock response.

---

## 7. AutomationIds

Preserve existing:

```text
RightToolWell.Root
RightToolWell.SectionTab
RightToolWell.AiTab
RightToolWell.ActiveView

AiAssistant.Panel
AiAssistant.Header
AiAssistant.ContextSummary
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.CancelButton
AiAssistant.CopyButton
AiAssistant.ClearButton
AiAssistant.ResponseArea
AiAssistant.DraftPreview
AiAssistant.SafetyFooter
AiAssistant.ChatHistory
AiAssistant.Composer
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
```

Optional addition:

```text
AiAssistant.RefreshContextButton
AiAssistant.ContextSummaryDetails
```

Do not add:

```text
AiAssistant.ApplyButton
```

---

## 8. Tests

Add focused tests.

### 8.1 Context provider tests

Required cases:

```text
1. Builds context from caret inside key/value line.
2. Resolves current Section.
3. Resolves current Key and Value.
4. Includes bounded nearby lines.
5. Does not include entire file when file is larger than nearby radius.
6. Includes selected text only when provided.
7. Handles caret in comment / blank line safely.
8. Handles no semantic context safely.
```

### 8.2 UI boundary tests

Required checks:

```text
1. AiAssistant.ContextSummary still exists.
2. Context summary displays bounded current context text or placeholder.
3. Generate still does not modify source editor text.
4. Generate still does not mark document dirty, if observable.
5. No Apply button exists.
6. Section Tree remains default view.
```

Avoid pixel-perfect tests.

Do not require DeepSeek or network.

---

## 9. Validation Commands

Run full validation because source and Shell wiring may change:

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
1. Open a project and file.
2. Place caret on a key/value line.
3. Open AI Assistant.
4. Generate mock response.
5. Confirm context summary shows current file / Section / key / value / nearby line count.
6. Select text and confirm selected text is included only when explicit.
7. Confirm no whole file/project is shown.
8. Confirm no DeepSeek/network/API key is used.
9. Confirm no editor text changes.
10. Confirm no dirty state is created.
11. Confirm Section tree behavior remains unchanged.
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: AI-2B.
2. Files changed.
3. Context provider implementation summary.
4. Context summary UI changes.
5. Commands run.
6. Build result.
7. Test result.
8. Package result.
9. Confirmation no DeepSeek/network/API key added.
10. Confirmation no Field Registry retrieval implemented yet.
11. Confirmation no diagnostics summary implemented yet.
12. Confirmation no file modification behavior added.
13. Confirmation Section tree behavior preserved.
14. Manual smoke steps or result.
15. Remaining risks.
16. Recommended next phase.
```
