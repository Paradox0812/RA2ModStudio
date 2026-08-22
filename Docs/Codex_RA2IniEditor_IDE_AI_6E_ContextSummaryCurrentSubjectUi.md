# Codex Task: RA2IniEditor.IDE AI-6E Context Summary UI Polish / Current Subject Display

## 0. Current Baseline

AI-6D has been completed.

Reported state:

```text
AI-6B: bounded ConversationContext extraction completed.
AI-6C: CurrentSubject / draft subject extraction completed.
AI-6D: PromptBuilder and send flow now receive ConversationContext and CurrentSubject.
Prompt now contains independent Current Subject and Conversation Context sections.
Tests: 1427 passed.
IdeOnly package: passed, packaged file count 761.
No XAML changes in AI-6D.
No Apply / Insert / editor mutation / dirty-state mutation.
No Field Registry writes.
Legacy not restored.
```

Next phase:

```text
AI-6E: Context Summary UI polish / Current Subject display
```

This is a limited UI display phase.

Do not change PromptBuilder semantics.

Do not change DeepSeek adapter behavior.

Do not change provider selection behavior.

---

## 1. Goal

Expose the current AI context state in the AI Assistant panel so the user can understand what the assistant will use for follow-up prompts.

The UI should show a compact read-only context summary, including:

```text
current IDE context
field evidence count
diagnostics count
conversation context state
current subject
```

Most importantly, when the current subject is inferred from a prior assistant draft, the panel should show something like:

```text
当前主题：LAAV（上一轮 AI 草稿）
```

This improves user trust and explains why "这个单位" can be resolved.

---

## 2. Hard Boundaries

Do not implement or modify:

```text
PromptBuilder rules
DeepSeek adapter behavior
provider selection behavior
API key loading
Apply / Insert
file modification
Field Registry writes
whole-project context
unbounded chat history
cross-session memory
hidden memory
settings persistence
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

This phase only displays existing bounded context information.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if current AI panel state is already there
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if a tiny display helper is useful:

```text
RA2IniEditor.IDE/AI/Ra2AiContextSummaryFormatter.cs
RA2IniEditor.Tests/IDE/Ra2AiContextSummaryFormatterTests.cs
```

Do not modify AI providers / PromptBuilder / DeepSeek adapter unless a compile error requires a trivial fix.

---

## 4. Required UI Behavior

### 4.1 Context Summary area

Use existing:

```text
AiAssistant.ContextSummary
```

The summary should remain compact and read-only.

It may show a single compact line or 2-3 short wrapped lines.

Suggested format:

```text
上下文：当前文件 rulesmd.ini；Section [LAAV]；字段 Strength=200；字段依据 8；诊断 0。
当前主题：LAAV（上一轮 AI 草稿）
对话上下文：最近 6 轮，已截断/未截断
```

If no subject exists:

```text
当前主题：无
```

If no editor context exists:

```text
上下文：当前没有可用编辑器上下文。
```

### 4.2 Current Subject display

Display current subject if available:

```text
当前主题：<SubjectId>（上一轮 AI 草稿）
```

Include kind when compact:

```text
当前主题：LAAV / Unit（上一轮 AI 草稿）
```

Do not imply applied file state.

Do not say:

```text
当前项目对象：LAAV
已写入 rulesmd.ini
```

unless future explicit applied-state support exists.

### 4.3 Conversation Context display

Show only summary metadata:

```text
对话上下文：最近 N 轮
```

If truncated:

```text
对话上下文：最近 N 轮，已截断
```

Do not display full conversation text in the summary.

### 4.4 Refresh timing

The UI summary should update when:

```text
1. AI Assistant is opened.
2. Generate / Send is clicked.
3. Chat history changes after assistant response, if current architecture makes this simple.
```

Do not auto-send anything.

Do not auto-open AI.

---

## 5. Visual Requirements

The summary must not become a large form panel.

Prefer:

```text
compact muted text
small chips
wrapped short lines
```

Avoid:

```text
large bordered form rows
full conversation dump
long prompt dump
raw hidden context
API key / provider metadata
```

---

## 6. AutomationIds

Preserve:

```text
AiAssistant.ContextSummary
AiAssistant.ChatHistory
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.ModelSelector
```

Allowed additions:

```text
AiAssistant.CurrentSubjectSummary
AiAssistant.ConversationContextSummary
AiAssistant.ContextSummaryLine
```

Forbidden:

```text
AiAssistant.ApplyButton
AiAssistant.InsertButton
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
```

---

## 7. Tests

Add/update boundary tests.

Required checks:

```text
1. AiAssistant.ContextSummary still exists.
2. Current subject summary exists when subject is available.
3. Current subject display includes draft/advisory wording for LastAssistantDraft.
4. Current subject display does not claim project file state.
5. Conversation context summary shows bounded turn count / truncated state if testable.
6. Context summary does not expose hidden provider metadata.
7. No Apply / Insert button exists.
8. Generate/send behavior remains unchanged.
9. No editor text mutation / dirty-state mutation, if observable.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 8. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Generate a unit draft such as [LAAV].
3. Confirm the AI panel eventually shows:
   当前主题：LAAV（上一轮 AI 草稿）
4. Ask: 在这个单位基础上，把它改成苏军单位。
5. Confirm the response understands the subject.
6. Confirm summary does not claim LAAV exists in project files.
7. Confirm no Apply / Insert button exists.
8. Confirm no editor text changes and no dirty state.
```

---

## 9. Validation Commands

Run full validation because Shell UI may change:

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
1. Phase completed: AI-6E.
2. Files changed.
3. Context Summary UI changes.
4. Current Subject display behavior.
5. Conversation Context display behavior.
6. AutomationIds preserved/added.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation PromptBuilder/provider behavior unchanged.
12. Confirmation no Apply/Insert/file modification behavior added.
13. Manual smoke steps or result.
14. Remaining risks.
15. Recommended next phase.
```
