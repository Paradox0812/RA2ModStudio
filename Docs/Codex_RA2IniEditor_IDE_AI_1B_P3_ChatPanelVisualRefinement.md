# Codex Task: RA2IniEditor.IDE AI-1B-P3 Chat Panel Visual Refinement

## 0. Context

AI-1B-P2 changed the AI Assistant right-side page toward a chat-style layout.

User screenshot review:

```text
1. 当前产出符合大方向，但 UI 仍然偏原生 WPF。
2. 顶部大号“关闭”按钮可以删除。
3. 下方用户输入框位置和结构需要优化。
4. 当前有太多“大框套小框”，不像 IDE 工具面板。
5. 参考 GitHub Copilot Chat / ChatGPT：上方聊天记录，下方固定 composer。
```

This task is a **layout / visual refinement only** task for the AI Assistant skeleton.

Do not implement real AI generation.

---

## 1. Goal

Refine the AI Assistant right-side page so it feels like an IDE chat panel instead of a WPF form.

Target result:

```text
Header:
  compact title + optional small close icon, no large Close button row

Main:
  scrollable chat history

Bottom:
  sticky composer with prompt input, advanced control, send button, safety hint
```

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

## 4. Required Visual Changes

### 4.1 Remove top large Close row

Remove the current large full-width “关闭” button row from the AI panel.

Preferred replacement:

```text
small icon/glyph button in the AI header
```

The close action must remain available and keep the existing AutomationId:

```text
AiAssistant.CloseButton
```

The close behavior remains:

```text
return to Section Tree / previous right tool view
```

Do not remove the close behavior.

---

### 4.2 Compact header

Header should be compact.

Suggested content:

```text
AI 助手
```

Optional small status text:

```text
草稿模式 / 不会自动修改文件
```

Avoid:

```text
large bordered header blocks
large button rows
full-width header buttons
```

Required AutomationId remains:

```text
AiAssistant.Header
```

---

### 4.3 Chat history as primary area

The main middle area should be a single scrollable chat history.

Required AutomationId:

```text
AiAssistant.ChatHistory
```

The chat history may contain static placeholder message cards.

Suggested placeholder structure:

```text
Assistant message:
  AI 助手已就绪。你可以直接描述需求，例如“帮我设计一个轻型防空车”。
  当前阶段不会发送请求，也不会修改文件。

User placeholder:
  用户消息将在后续阶段显示在这里。
```

Avoid:

```text
large independent ResponseArea rectangle
large independent DraftPreview rectangle
nested border boxes
form-like response/draft blocks
```

Preserve:

```text
AiAssistant.ResponseArea
AiAssistant.DraftPreview
```

But they may be compact elements inside chat history rather than giant boxes.

---

### 4.4 Composer at bottom

The bottom composer should be the strongest visual anchor.

Required AutomationId:

```text
AiAssistant.Composer
```

Composer should contain:

```text
left utility button placeholder, e.g. "+"
PromptBox
Advanced button / model options
Send button
small safety hint
```

Required preserved IDs:

```text
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.SafetyFooter
```

Required added/preserved IDs:

```text
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
```

Visual direction:

```text
prompt input and buttons should look like one composer bar
not separate large form controls
```

---

### 4.5 Advanced inside composer

The Advanced entry must be placed near the prompt box lower-right / right side, similar to ChatGPT.

Allowed placeholder content:

```text
进阶
模型：Mock / DeepSeek 后续接入
上下文：后续阶段配置
```

No real model switching is required.

No provider configuration is required.

No API key handling is required.

---

### 4.6 Context summary should be compact

Current context summary should not look like a large top panel.

Keep AutomationId:

```text
AiAssistant.ContextSummary
```

Suggested text:

```text
上下文：当前阶段仅占位，不会收集或发送上下文。
```

Preferred placement:

```text
small muted line above chat history
or compact line inside header/composer area
```

---

## 5. Reduce WPF Native Feel

Avoid:

```text
large Border inside Border inside Border
TextBox-like giant response areas
default WPF Button-heavy rows
large empty framed rectangles
form layout feel
```

Prefer:

```text
flat panel background
light separators
compact message cards
minimal borders
sticky composer
subtle disabled controls
Chinese-first labels
```

Do not introduce broad theme changes.

Keep the refinement local to the AI Assistant panel.

---

## 6. Section Tree Behavior

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

## 7. AutomationIds

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
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
```

If `AiAssistant.TaskKindSelector` exists from earlier skeleton, do not make it prominent. It may remain hidden/disabled/inside Advanced if tests require it.

Do not add:

```text
AiAssistant.ApplyButton
```

---

## 8. Button Behavior

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

## 9. Tests

Update/add boundary tests only.

Required checks:

```text
1. AiAssistant.ChatHistory exists.
2. AiAssistant.Composer exists.
3. AiAssistant.PromptBox remains.
4. AiAssistant.GenerateButton remains.
5. AiAssistant.AdvancedButton exists.
6. AiAssistant.ModelSelector or Advanced placeholder exists.
7. AiAssistant.SafetyFooter remains.
8. AiAssistant.DraftPreview remains.
9. AiAssistant.CloseButton remains.
10. No Apply button exists.
11. Section Tree AutomationId still exists.
12. RightToolWell Section tab still exists.
```

Avoid pixel-perfect tests.

---

## 10. Validation Commands

Run full validation because Shell XAML may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 11. Manual Smoke Checklist

After implementation:

```text
1. Launch RA2IniEditor.IDE.
2. Confirm Section tree is default right-side view.
3. Open AI Assistant.
4. Confirm there is no large top “关闭” row.
5. Confirm close is a small header button.
6. Confirm most of the panel is chat history.
7. Confirm prompt box / composer is fixed at the bottom.
8. Confirm “进阶” is near the composer / prompt box.
9. Confirm no prominent task kind selector is in the main path.
10. Confirm no Apply button exists.
11. Confirm Generate/Send does not call AI or modify files.
12. Close AI and confirm Section tree returns.
```

---

## 12. Final Report Format

Report:

```text
1. Phase completed: AI-1B-P3.
2. Files changed.
3. Layout changes.
4. Removed top close row? yes/no.
5. Composer and Advanced placement.
6. AutomationIds preserved/added.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation no AI generation logic added.
12. Confirmation no DeepSeek/network/API key added.
13. Confirmation no file modification behavior added.
14. Confirmation Section tree behavior preserved.
15. Manual smoke steps or result.
16. Remaining risks.
17. Recommended next phase.
```
