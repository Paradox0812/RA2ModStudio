# Codex Task: RA2IniEditor.IDE AI-1B-P2 Chat-style Composer / Advanced Options Placement

## 0. Context

AI-1B / AI-1B-P has established the right-side AI Assistant skeleton inside the existing Right Tool Well.

User feedback:

```text
AI 面板应该像 ChatGPT 网页：
上方大部分是聊天记录；
下方是固定输入框；
附带上下文 / 选择模型等选项放在输入框右下角的“进阶”里。
任务类别不应作为主界面下拉框，默认由 DeepSeek 自行判断。
```

This task is a **layout refinement contract / limited implementation** for the AI Assistant skeleton.

Do not implement real AI logic in this task.

---

## 1. Goal

Refine the AI Assistant page into a ChatGPT-like chat layout:

```text
Main area:
  Scrollable chat history

Bottom:
  Sticky composer
  Left utility button
  Multi-line prompt box
  Advanced menu at the lower/right side of composer
  Send button
  Safety hint
```

The UI should feel like a chat assistant, not a form with task-kind dropdowns.

---

## 2. Product Decisions

### 2.1 Task kind selector is removed from the main path

The main UI should not force the user to choose:

```text
解释字段
查找相关字段
生成单位原型
生成武器链草案
审查 INI 片段
解释诊断
```

Instead:

```text
default mode = Auto
DeepSeek / prompt builder decides intent from user input and context
```

Future optional override may live inside Advanced, but not as a prominent main control.

### 2.2 Advanced options live inside the composer

The Advanced entry should be near the prompt box, like the lower-right "进阶" control in ChatGPT.

Advanced may contain:

```text
模型选择
上下文附带选项
回答详细度
是否包含字段依据
```

For this phase, Advanced can be a placeholder or simple dropdown/flyout.

### 2.3 Send button is the main action

The composer should visually prioritize:

```text
input -> send
```

Not:

```text
task selection -> form submit
```

---

## 3. Hard Boundaries

Do not implement:

```text
MockRa2AiClient generation
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

Shell changes are allowed only inside the existing Right Tool Well / AI skeleton area.

Do not redesign the whole Shell.

---

## 4. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs, only if current skeleton wiring requires local no-op layout state
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if current skeleton uses UI-only state
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not create broad AI framework classes yet.

---

## 5. Required Layout

### 5.1 Chat history

The main content area should be a scrollable chat history.

Required AutomationId:

```text
AiAssistant.ChatHistory
```

It should contain placeholder message cards, for example:

```text
助手：
AI 助手已就绪。你可以直接描述需求，例如“帮我设计一个轻型防空车”。
当前阶段不会发送请求，也不会修改文件。
```

Optional AutomationIds:

```text
AiAssistant.EmptyMessage
AiAssistant.AssistantMessage
AiAssistant.UserMessage
```

### 5.2 Composer

The composer must be fixed at the bottom.

Required AutomationId:

```text
AiAssistant.Composer
```

Composer structure:

```text
+ / context utility placeholder
PromptBox
Advanced button/dropdown
Send button
Safety hint
```

Required preserved AutomationIds:

```text
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.SafetyFooter
```

The existing `GenerateButton` may visually become the send button, but its AutomationId should stay `AiAssistant.GenerateButton`.

### 5.3 Advanced control

Add:

```text
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
```

For this phase:

```text
Advanced options may be disabled placeholder or simple static menu.
No real model switching is required.
No provider configuration is required.
No API key handling is required.
```

Suggested visible text:

```text
进阶
模型：Mock / DeepSeek 后续接入
上下文：后续阶段配置
```

### 5.4 Context summary

Context summary should not occupy a large top block.

Required preserved AutomationId:

```text
AiAssistant.ContextSummary
```

Preferred placement:

```text
compact row above chat history
or small collapsible line inside the composer/header
```

Suggested text:

```text
上下文：当前阶段仅占位，不会收集或发送上下文。
```

### 5.5 Response / Draft Preview

Preserve existing AutomationIds:

```text
AiAssistant.ResponseArea
AiAssistant.DraftPreview
```

But they may be represented as compact placeholders inside the chat history.

Do not keep large separate empty rectangles for response and draft.

---

## 6. AutomationIds

Preserve existing:

```text
RightToolWell.Root
RightToolWell.SectionTab
RightToolWell.AiTab
RightToolWell.ActiveView

AiAssistant.Panel
AiAssistant.Header
AiAssistant.CloseButton
AiAssistant.ContextSummary
AiAssistant.TaskKindSelector
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
```

If `AiAssistant.TaskKindSelector` already exists from AI-1B, do not remove it abruptly if tests depend on it. Instead:

```text
hide / demote it into Advanced options
or keep a disabled/internal placeholder with the same AutomationId
```

Add:

```text
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
```

Do not add an Apply button.

---

## 7. Button Behavior

This phase remains skeleton-only.

Allowed:

```text
Send/Generate button exists but does not call AI.
Advanced opens static placeholder options if simple.
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

## 8. Section Tree Behavior

Must remain unchanged:

```text
Section Tree remains default view.
ProjectExplorerTreeView x:Name remains.
ProjectExplorerTreeView AutomationId remains.
ProjectExplorerTreeView binding remains.
Section selection/jump remains unchanged.
AI close returns to Section Tree.
```

---

## 9. Visual Rules

Prefer:

```text
ChatGPT-like vertical flow
scrollable history
sticky bottom input
compact context summary
Advanced options inside composer
single primary send button
Chinese-first labels
```

Avoid:

```text
form-heavy layout
large response/draft rectangles
task-type dropdown as main interaction
Apply button
auto-send behavior
hidden safety text
```

---

## 10. Tests

Update/add boundary tests only.

Required checks:

```text
1. AiAssistant.ChatHistory exists.
2. AiAssistant.Composer exists.
3. AiAssistant.PromptBox remains.
4. AiAssistant.GenerateButton remains.
5. AiAssistant.AdvancedButton exists.
6. AiAssistant.ModelSelector exists or an Advanced placeholder exists.
7. AiAssistant.SafetyFooter remains.
8. AiAssistant.DraftPreview still exists.
9. No Apply button exists.
10. Section Tree AutomationId still exists.
11. RightToolWell Section tab still exists.
```

Avoid pixel-perfect tests.

---

## 11. Validation Commands

Run full validation because Shell XAML may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 12. Manual Smoke Checklist

After implementation:

```text
1. Launch RA2IniEditor.IDE.
2. Confirm Section tree is default right-side view.
3. Open AI Assistant.
4. Confirm most of the panel is chat history.
5. Confirm bottom composer contains prompt box.
6. Confirm Advanced is near the prompt box lower/right area.
7. Confirm model selector placeholder appears in Advanced.
8. Confirm no prominent task kind selector appears in the main panel.
9. Confirm no Apply button exists.
10. Confirm Generate/Send does not call AI or modify files.
11. Close AI and confirm Section tree returns.
```

---

## 13. Final Report Format

Report:

```text
1. Phase completed: AI-1B-P2.
2. Files changed.
3. Layout changes.
4. Advanced/model selector placement.
5. AutomationIds preserved/added.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no AI generation logic added.
11. Confirmation no DeepSeek/network/API key added.
12. Confirmation no file modification behavior added.
13. Confirmation Section tree behavior preserved.
14. Manual smoke steps or result.
15. Remaining risks.
16. Recommended next phase.
```
