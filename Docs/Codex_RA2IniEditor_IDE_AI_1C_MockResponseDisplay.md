# Codex Task: RA2IniEditor.IDE AI-1C Mock AI Response Display

## 0. Current Baseline

AI-1B / AI-1B-P / AI-1B-P4 have been accepted by the user.

Current accepted state:

```text
Right-side ProjectExplorer / Section area has been evolved into Right Tool Well.
Section Tree remains default.
AI Assistant is the second right-side view.
AI page uses chat-style layout.
Prompt composer is at the bottom.
Advanced/model options are placed near the composer.
The extra close button and meaningless plus button were removed.
PromptBox supports wrapping / multi-line input.
No DeepSeek / network / context provider / prompt builder / file modification exists yet.
```

This task starts:

```text
AI-1C: Mock AI response display
```

This is a limited implementation task.

---

## 1. Goal

Add deterministic mock AI response behavior to the existing AI Assistant chat UI.

AI-1C should make the skeleton interactive enough to validate chat flow:

```text
1. User enters text in PromptBox.
2. User clicks Generate / Send.
3. The UI appends a user message.
4. The UI appends a deterministic mock assistant response.
5. Copy / Clear work on local displayed content.
6. Cancel is present but does not need to cancel real network because there is no network yet.
```

This phase must not call DeepSeek.

---

## 2. Hard Boundaries

Do not implement:

```text
DeepSeek client
network calls
API key configuration
real context provider
real prompt builder
intent classifier
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
parser
diagnostics
completion
hover
quick peek
save preflight
BuiltIn field registry JSON
legacy files
solution / project files
```

Shell changes are allowed only inside the existing Right Tool Well / AI Assistant area and minimal UI state wiring.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed if the project structure strongly favors a small internal model class:

```text
RA2IniEditor.IDE/AI/Ra2AiChatMessage.cs
RA2IniEditor.IDE/AI/MockRa2AiClient.cs
```

However, prefer the smallest implementation that fits current Shell architecture. Do not introduce broad AI framework abstractions in AI-1C.

---

## 4. Required Behavior

### 4.1 Generate / Send

When user clicks Generate/Send:

```text
1. If PromptBox is empty or whitespace, do not add a message.
2. Otherwise, append a user message with the prompt text.
3. Append a deterministic mock assistant message.
4. Clear PromptBox after sending if that matches existing UI convention.
5. Do not collect real context.
6. Do not call network.
7. Do not modify editor text.
8. Do not mark document dirty.
```

Suggested mock response:

```text
这是 AI-1C 的本地 Mock 回复。后续阶段会接入字段库上下文和 DeepSeek。当前不会修改任何文件。
```

If the user prompt contains common patterns, optional deterministic variations are allowed, but no real model logic is required.

### 4.2 Chat history

Use the existing chat history area.

Required:

```text
1. User messages and assistant messages are visually distinct.
2. Messages are scrollable.
3. Existing placeholders are replaced or supplemented by real local mock messages.
```

### 4.3 Copy

Copy should copy the latest assistant response or selected response if such selection exists.

Acceptable in AI-1C:

```text
Copy latest assistant mock response.
```

Do not copy hidden context or prompts.

### 4.4 Clear

Clear should clear local chat messages / return to placeholder state.

Do not clear documents or project state.

### 4.5 Cancel

Because AI-1C has no network call, Cancel may be disabled or may only clear a local busy state.

Do not add fake delays unless tests require busy state validation.

---

## 5. UI / AutomationIds

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

Allowed additions:

```text
AiAssistant.UserMessageList
AiAssistant.AssistantMessageList
AiAssistant.LatestAssistantMessage
AiAssistant.EmptyStateMessage
```

Do not add:

```text
AiAssistant.ApplyButton
```

---

## 6. Safety Text

Safety footer must remain visible:

```text
AI 输出仅作为草稿或建议；当前阶段不会发送真实请求或修改文件。
```

Because AI-1C uses mock response only, text may state:

```text
当前为 Mock 回复阶段，不会连接 DeepSeek。
```

---

## 7. Tests

Update/add boundary and behavior tests.

Required tests:

```text
1. AI panel AutomationIds still exist.
2. No Apply button exists.
3. Generate with empty prompt does not add a message.
4. Generate with non-empty prompt adds user + assistant mock messages.
5. Generate does not modify source editor text.
6. Generate does not mark document dirty, if dirty state is testable.
7. Clear removes local messages / restores empty state.
8. Copy command is present and does not require network.
9. Section Tree remains default and its AutomationId remains.
```

Avoid pixel-perfect tests.

Do not require DeepSeek or network in tests.

---

## 8. Validation Commands

Run full validation because Shell XAML / code-behind / ViewModel may change:

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
1. Launch RA2IniEditor.IDE.
2. Confirm Section tree is default right-side view.
3. Open AI Assistant.
4. Enter a prompt.
5. Click Generate/Send.
6. Confirm user message appears in chat history.
7. Confirm mock assistant response appears.
8. Confirm PromptBox wraps and clears or remains according to chosen behavior.
9. Confirm Copy works for latest response.
10. Confirm Clear resets local chat.
11. Confirm no Apply button exists.
12. Confirm no network/API key/DeepSeek is used.
13. Confirm no editor text changes and no dirty state is created.
14. Switch back to Section tree and confirm navigation still works.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-1C.
2. Files changed.
3. Mock response implementation strategy.
4. Chat behavior implemented.
5. Commands run.
6. Build result.
7. Test result.
8. Package result.
9. Confirmation no DeepSeek/network/API key added.
10. Confirmation no file modification behavior added.
11. Confirmation Section tree behavior preserved.
12. Manual smoke steps or result.
13. Remaining risks.
14. Recommended next phase.
```
