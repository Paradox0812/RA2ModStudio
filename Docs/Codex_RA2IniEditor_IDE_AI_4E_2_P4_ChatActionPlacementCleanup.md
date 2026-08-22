# Codex Task: RA2IniEditor.IDE AI-4E-2-P4 Chat Action Placement Cleanup

## 0. Context

Manual UI review after AI-4E-2-P3R found that the Advanced area is now much better because it only shows the model selector:

```text
进阶
模型
[ Mock ▼ ]
```

User and reviewer agree on the next correction:

```text
取消 / 复制 / 清空 不应该放在“进阶 / 模型选择”区域。
```

These buttons are valid AI chat actions, but their current placement is wrong.

This task is a **UI placement cleanup** for AI Assistant chat actions.

Do not change provider behavior.

Do not change DeepSeek / Mock send flow semantics.

---

## 1. Goal

Move or demote AI chat action buttons to their correct locations:

```text
Cancel:
  belongs to request sending state / composer send area.

Copy:
  belongs to assistant message card / latest response action.

Clear:
  belongs to chat history header / chat area action.
```

The Advanced area must remain minimal:

```text
Advanced:
  Model selector only
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
auto-send context
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

Shell changes are allowed only inside the existing AI Assistant UI.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs, only if button event placement/wiring needs adjustment
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if current AI panel state is already there
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not modify DeepSeekRa2AiClient / DeepSeekRa2AiClientFactory / PromptBuilder / ContextProvider in this task.

---

## 4. Required UI Changes

### 4.1 Advanced area

Advanced area should only contain:

```text
模型
[ Mock / DeepSeek ComboBox ]
```

Required:

```text
1. Remove visible Cancel / Copy / Clear buttons from Advanced.
2. Keep AiAssistant.ModelSelector.
3. Keep Mock default.
4. No Provider / Status / Intent / API key long rows.
5. No API key input.
6. No Save API key button.
7. No Apply button.
```

### 4.2 Cancel action

Cancel should not be permanently visible in Advanced.

Preferred behavior:

```text
1. Cancel appears in the composer area only while sending.
2. Or Cancel remains hidden/disabled until real request is in progress.
```

Minimum acceptable for this phase:

```text
Cancel button is no longer visible in Advanced.
Existing AiAssistant.CancelButton AutomationId may be preserved on a hidden/collapsed or composer-level button.
```

Do not remove cancellation behavior if it already exists.

### 4.3 Copy action

Copy should move to assistant message / latest response action.

Preferred:

```text
Assistant message card:
  [复制] small action near latest assistant response
```

Minimum acceptable:

```text
Copy button outside Advanced, near chat history or latest response area.
```

Preserve:

```text
AiAssistant.CopyButton
```

Copy behavior remains:

```text
copy latest assistant response
```

### 4.4 Clear action

Clear should move to chat history header/action area.

Preferred:

```text
Chat history header:
  [清空]
```

Minimum acceptable:

```text
Clear button outside Advanced and not inside model selector area.
```

Preserve:

```text
AiAssistant.ClearButton
```

Clear behavior remains:

```text
clear local chat messages / restore empty state
```

---

## 5. AutomationIds

Preserve existing where meaningful:

```text
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
AiAssistant.GenerateButton
AiAssistant.PromptBox
AiAssistant.SafetyFooter
AiAssistant.ChatHistory
AiAssistant.ContextSummary
AiAssistant.Composer
AiAssistant.CancelButton
AiAssistant.CopyButton
AiAssistant.ClearButton
```

Allowed additions:

```text
AiAssistant.ChatHistoryActions
AiAssistant.LatestMessageActions
AiAssistant.ComposerCancelButton
```

Forbidden:

```text
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
AiAssistant.ApplyButton
```

If the same action is moved, keep existing AutomationId rather than creating duplicate controls.

---

## 6. Behavior Must Remain

Existing behavior from AI-4E-2 must remain:

```text
Mock is default.
DeepSeek is explicit.
Mock uses FakeRa2AiClient.
DeepSeek uses environment-only configuration.
Missing DEEPSEEK_API_KEY shows MissingConfiguration when sending.
Copy copies latest assistant response.
Clear clears local chat state.
Cancel cancels/clears current sending state if applicable.
No API key UI.
No settings persistence.
No Apply / Insert.
No editor text mutation.
```

This task changes only action placement and layout.

---

## 7. Tests

Update boundary tests to match action placement.

Required checks:

```text
1. Advanced area contains ModelSelector.
2. Advanced area no longer contains visible Cancel / Copy / Clear action group if source-boundary testable.
3. CopyButton still exists outside Advanced.
4. ClearButton still exists outside Advanced.
5. CancelButton is hidden/collapsed or composer-level, not inside Advanced.
6. No ApiKeyTextBox exists.
7. No SaveApiKeyButton exists.
8. No ApplyButton exists.
9. Mock remains default.
10. Generate/send behavior remains AI-4E-2 behavior.
11. Copy still copies latest assistant response if behavior test exists.
12. Clear still clears local chat if behavior test exists.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 8. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Open Advanced.
3. Confirm Advanced only shows model selector.
4. Confirm Cancel / Copy / Clear are no longer inside Advanced.
5. Send a prompt in Mock mode.
6. Confirm Copy is available near assistant response / chat area.
7. Confirm Copy copies latest response.
8. Confirm Clear is available near chat history and clears messages.
9. Confirm no API key input exists.
10. Confirm no Save API key button exists.
11. Confirm no Apply button exists.
12. Confirm DeepSeek MissingConfiguration behavior still works when selected without env key.
13. Confirm no editor text changes and no dirty state.
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
1. Phase completed: AI-4E-2-P4.
2. Files changed.
3. Advanced area cleanup.
4. Cancel / Copy / Clear placement changes.
5. AutomationIds preserved/updated.
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
