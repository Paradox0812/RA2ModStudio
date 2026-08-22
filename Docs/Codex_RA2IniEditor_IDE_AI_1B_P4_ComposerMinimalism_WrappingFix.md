# Codex Task: RA2IniEditor.IDE AI-1B-P4 Composer Minimalism / Wrapping Fix

## 0. Context

AI-1B-P3 refined the AI Assistant right-side panel toward a chat-style layout.

User screenshot review shows the current AI panel is closer to the desired direction, but the composer still needs refinement.

User feedback:

```text
1. 不要在 AI 面板右上角增加 ×，界面应尽可能简洁。
2. 左下角 + 目前意义不明，可以去掉。
3. “AI 输出仅作为草稿或建议；当前阶段不会发送请求或修改文件。” 应该挪到输入框下方，脱离输入框。
4. 这句安全提示字体太大，需要更轻、更小。
5. 输入框不会按照实际宽度智能换行，目前只是一行。
```

This task is a **layout-only refinement** for the existing AI Assistant skeleton.

Do not implement AI generation logic.

---

## 1. Goal

Refine the AI Assistant composer and header into a simpler chat-style layout.

Target result:

```text
Header:
  simple "AI 助手" title
  no extra × close button in the top-right of the AI page

Main:
  chat history remains

Bottom Composer:
  prompt box with proper text wrapping
  send button
  advanced button
  small safety hint below the input area
```

The AI panel should feel cleaner and less WPF-form-like.

---

## 2. Hard Boundaries

Do not implement:

```text
MockRa2AiClient response generation
DeepSeek client
network calls
API key configuration
real context provider
prompt builder
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

Shell changes are allowed only inside the existing Right Tool Well / AI Assistant skeleton.

Do not redesign the whole Shell.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs, only if existing skeleton wiring requires local no-op layout state
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if existing skeleton uses UI-only state
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not create new AI framework classes in this phase.

---

## 4. Required UI Changes

### 4.1 Remove AI page top-right close button

Remove the visible `×` close button from the AI page header.

Rationale:

```text
The right tool well already has Section / AI tabs. Returning to Section should be done through the Section tab, not an extra close button.
```

AutomationId handling:

```text
If tests currently require AiAssistant.CloseButton, update tests according to the new contract.
```

Preferred behavior:

```text
AI is closed/left by switching back to Section tab.
```

Do not remove the Section tab.

Do not remove the AI tab.

### 4.2 Remove the left-bottom "+" utility button

Remove the visible `+` utility placeholder in the composer.

Rationale:

```text
It currently has no implemented meaning and adds visual noise.
```

Do not add replacement behavior.

Do not implement context attach or file attach in this phase.

### 4.3 Move safety hint below composer input

The safety hint:

```text
AI 输出仅作为草稿或建议；当前阶段不会发送请求或修改文件。
```

should be outside the input border/container and below the main input row.

Required AutomationId remains:

```text
AiAssistant.SafetyFooter
```

Visual requirements:

```text
1. Small font.
2. Muted color.
3. Not inside the TextBox-like input area.
4. Does not compete with the input text.
```

### 4.4 Make prompt input wrap by available width

The prompt input must support multi-line wrapping.

Required for `AiAssistant.PromptBox`:

```text
AcceptsReturn="True"
TextWrapping="Wrap"
VerticalScrollBarVisibility="Auto"
HorizontalScrollBarVisibility="Disabled"
MinLines or MinHeight suitable for 2 lines
MaxLines or MaxHeight suitable for compact composer
```

Expected behavior:

```text
Long prompt text wraps within the available composer width.
Composer height can grow modestly or scroll internally.
It must not remain a single horizontal line.
```

### 4.5 Composer visual simplification

Composer should be visually simpler:

```text
PromptBox + Send + Advanced
Safety hint below
```

Do not use multiple nested borders.

Avoid:

```text
large framed box inside another framed box
separate meaningless utility button
large safety text
top-heavy form controls
```

Prefer:

```text
one compact composer container
prompt area with wrap
advanced near send
send button right side
small safety hint below
```

---

## 5. AutomationIds

Preserve existing where still meaningful:

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

`AiAssistant.CloseButton` is no longer required for this design if the AI tab can be left through `RightToolWell.SectionTab`.

Do not add:

```text
AiAssistant.ApplyButton
```

---

## 6. Button Behavior

This phase remains skeleton-only.

Allowed:

```text
Generate/Send button exists but does not call AI.
Advanced opens static placeholder options if already simple.
Cancel/Copy/Clear may remain disabled/no-op placeholders.
```

Forbidden:

```text
real mock response generation
real provider call
context collection
file modification
document insertion
```

---

## 7. Section Tree Behavior

Must remain unchanged:

```text
Section Tree remains default view.
ProjectExplorerTreeView x:Name remains.
ProjectExplorerTreeView AutomationId remains.
ProjectExplorerTreeView binding remains.
Section selection/jump remains unchanged.
User can return to Section via Section tab.
```

---

## 8. Tests

Update/add boundary tests only.

Required checks:

```text
1. AiAssistant.ChatHistory exists.
2. AiAssistant.Composer exists.
3. AiAssistant.PromptBox remains.
4. PromptBox supports wrapping / multi-line input in XAML.
5. AiAssistant.GenerateButton remains.
6. AiAssistant.AdvancedButton remains.
7. AiAssistant.SafetyFooter remains.
8. Safety footer is outside the prompt box region if testable by source structure.
9. No Apply button exists.
10. Section Tree AutomationId still exists.
11. RightToolWell Section tab still exists.
12. Visible top-right AI close button is not required by tests.
```

Avoid pixel-perfect tests.

---

## 9. Validation Commands

Run full validation because Shell XAML may change:

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
1. Launch RA2IniEditor.IDE.
2. Confirm Section tree is default right-side view.
3. Open AI Assistant.
4. Confirm no top-right AI page × button is visible.
5. Confirm the left-bottom + button is gone.
6. Confirm prompt input wraps long text by available width.
7. Confirm safety hint is below input area, smaller and muted.
8. Confirm Advanced remains near the composer.
9. Confirm Send/Generate does not call AI or modify files.
10. Switch back to Section tab and confirm Section tree returns.
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: AI-1B-P4.
2. Files changed.
3. Header/close changes.
4. Composer changes.
5. Prompt wrapping changes.
6. Safety footer changes.
7. AutomationIds preserved/removed/updated.
8. Commands run.
9. Build result.
10. Test result.
11. Package result.
12. Confirmation no AI generation logic added.
13. Confirmation no DeepSeek/network/API key added.
14. Confirmation no file modification behavior added.
15. Confirmation Section tree behavior preserved.
16. Manual smoke steps or result.
17. Remaining risks.
18. Recommended next phase.
```
