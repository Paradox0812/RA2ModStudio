# Codex Task: RA2IniEditor.IDE AI-4E-2-P2 Composer Enter-to-Send

## 0. Context

This task extends the current AI-4E-2-P Provider Advanced Layout Fix.

User feedback:

```text
在此之上，增加按 Enter 发送消息的功能，而不是只能点“发送”。
```

The AI Assistant composer should behave like a chat input:

```text
Enter: send message
Shift+Enter: insert newline
```

This task is a **composer interaction refinement**.

Do not change provider logic.

Do not change DeepSeek / Mock behavior.

---

## 1. Goal

Allow the AI Assistant prompt box to send the current message by pressing `Enter`.

Required behavior:

```text
1. Enter sends the message.
2. Shift+Enter inserts a newline.
3. Empty / whitespace-only prompt remains no-op.
4. Existing Send button behavior remains unchanged.
5. Existing provider selection behavior remains unchanged.
6. Existing Mock / DeepSeek send flow remains unchanged.
```

---

## 2. Hard Boundaries

Do not implement or change:

```text
DeepSeek adapter behavior
DeepSeek request/response mapping
API key loading rules
PromptBuilder
ContextProvider
Field Registry evidence retrieval
Diagnostics summary
Apply / Insert
file modification
Field Registry writes
whole-project context
auto-send context outside explicit Enter/Send action
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
BuiltIn field registry JSON
legacy files
solution / project files
```

Shell changes are allowed only inside the existing AI Assistant composer / prompt input event wiring.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project paths.

Do not modify DeepSeekRa2AiClient / DeepSeekRa2AiClientFactory in this task.

---

## 4. Required Input Behavior

### 4.1 Enter

When focus is inside:

```text
AiAssistant.PromptBox
```

and user presses:

```text
Enter
```

with no Shift modifier:

```text
send current prompt
mark event handled
preserve same behavior as clicking AiAssistant.GenerateButton
```

### 4.2 Shift+Enter

When user presses:

```text
Shift+Enter
```

the PromptBox should insert a newline.

Do not send.

### 4.3 Empty Prompt

If PromptBox is empty or whitespace-only:

```text
Enter no-op
no user message added
no provider call
no mock call
no DeepSeek call
```

### 4.4 Busy State

If a request is already in progress:

```text
Enter must not start a duplicate send.
```

Use the same guard as Generate / Send button.

### 4.5 IME / Chinese Input Safety

Do not break Chinese input.

If the project can safely detect IME composition, Enter should not send while composition is active.

If IME composition detection is not already available, implement the smallest safe behavior and avoid broad input framework changes.

At minimum:

```text
Enter sends only when PromptBox has focus and not Shift+Enter.
No global Enter shortcut.
No Shell-wide Enter handler.
```

---

## 5. Implementation Guidance

Prefer reusing the existing send handler.

Suggested approach:

```text
PromptBox PreviewKeyDown / KeyDown
  if Enter and no Shift:
    e.Handled = true
    call existing AI send method / same handler used by Send button
  if Shift+Enter:
    allow normal TextBox newline behavior
```

Do not duplicate send logic.

Do not manually build prompt/context in the key handler.

The key handler should only route to the existing send method.

---

## 6. AutomationIds

Preserve existing:

```text
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.Composer
AiAssistant.ChatHistory
AiAssistant.ProviderSelector
AiAssistant.ProviderStatus
AiAssistant.SafetyFooter
```

Do not add:

```text
AiAssistant.ApplyButton
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
```

---

## 7. Tests

Update boundary / interaction tests only.

Required checks:

```text
1. PromptBox has an Enter-to-send handler or command binding.
2. Enter uses the same send path as Generate button.
3. Shift+Enter remains allowed for newline behavior if testable.
4. Empty prompt Enter does not add messages.
5. Enter does not bypass provider selection behavior.
6. Enter does not create Apply / Insert behavior.
7. Existing Send button still exists.
8. No Apply button exists.
```

If direct WPF key simulation is too costly in existing tests, use source/boundary tests to verify the event wiring and handler name, plus existing send behavior tests.

Avoid pixel-perfect tests.

---

## 8. Validation Commands

Run full validation because Shell XAML / code-behind may change:

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
2. Type a normal prompt.
3. Press Enter.
4. Confirm message sends.
5. Confirm user message appears in chat history.
6. Confirm assistant response appears according to selected provider behavior.
7. Type a multi-line prompt using Shift+Enter.
8. Confirm newline is inserted and message is not sent.
9. Press Send button and confirm it still works.
10. Try empty prompt + Enter and confirm no message is added.
11. Confirm no Apply button exists.
12. Confirm no editor text changes and no dirty state is created.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-4E-2-P2.
2. Files changed.
3. Enter-to-send implementation.
4. Shift+Enter behavior.
5. Tests added/updated.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation provider behavior unchanged.
11. Confirmation no API key UI/settings persistence added.
12. Confirmation no Apply/Insert/file modification behavior added.
13. Manual smoke steps or result.
14. Remaining risks.
15. Recommended next phase.
```
