# Codex Task: RA2IniEditor.IDE AI-0P-R2 Right Tool Well Direct Integration Contract

## 0. Context

User decision:

```text
不需要考虑非模态 AI 窗口，直接改造右侧 Section 区域即可。
```

This supersedes the previous fallback strategy.

The AI Assistant must be integrated into the existing right-side Section / Navigator area as a shared right tool area.

Do not propose a non-modal AI window fallback.

This is still a **contract / planning task** unless the user explicitly approves implementation.

Do not implement UI in this task.

---

## 1. Final Placement Decision

The future AI Assistant placement is:

```text
Existing right-side Section area -> shared Right Tool Well
```

The right-side area should host:

```text
Tab / view 1: Section Tree / Navigator
Tab / view 2: AI Assistant
```

Section Tree remains the default view.

AI Assistant opens only by explicit user action.

---

## 2. Rejected Alternatives

Do not use:

```text
non-modal AI tool window
second independent right sidebar
bottom AI tab
modal AI dialog
caret-near AI popup
AI overlay over Source Editor
```

These are no longer acceptable for the primary AI surface.

---

## 3. Design Goal

Transform the current right-side Section area into a shared right-side tool well without disrupting the main Shell layout.

The design should preserve:

```text
Source Editor width as much as possible
Project Explorer / Navigator behavior
Issues / Search bottom panels
main toolbar/menu/status bar
existing Section tree behavior
```

The AI Assistant should share the existing right-side footprint instead of adding new columns.

---

## 4. Required Contract Output

Create or update:

```text
Docs/AiAgentPanelPlacementContract.md
```

This document must state:

```text
1. AI Assistant will be integrated into the existing right-side Section area.
2. The right area becomes a Right Tool Well.
3. Section Tree remains the default tab/view.
4. AI Assistant is an additional tab/view.
5. AI does not auto-open on startup.
6. AI opens only by explicit command.
7. Closing AI returns to Section Tree or previous right tool view.
8. No non-modal AI tool window fallback.
9. No second right sidebar.
10. Shell implementation requires a separate approved implementation phase.
```

---

## 5. Required Inspection Before Future Implementation

Before any implementation, Codex must inspect and report:

```text
1. Current right-side Section tree XAML region.
2. Current control type:
   - TabControl
   - Grid column
   - ContentControl
   - ListBox
   - custom UserControl
   - other
3. Current ViewModel/DataContext for Section tree.
4. Current AutomationIds.
5. Current width / Grid column sizing.
6. Current entry points that update Section tree.
7. Whether adding a TabControl would break existing bindings.
8. Whether a ContentControl with view switching is safer than TabControl.
9. Exact files that would need changes.
10. Risk level.
```

Do not implement until this inspection is complete and user approves the implementation plan.

---

## 6. Right Tool Well Behavioral Rules

### 6.1 Section Tree

```text
1. Section Tree remains default.
2. Existing Section tree selection/jump behavior must remain unchanged.
3. Existing AutomationIds must be preserved.
4. Existing width behavior should remain as close as possible.
```

### 6.2 AI Assistant

```text
1. AI tab/view is inactive by default.
2. AI tab/view opens from toolbar/menu command.
3. AI tab/view can be closed/collapsed.
4. Closing AI should restore Section Tree or previous right-side view.
5. AI response content must not resize the entire Shell unpredictably.
```

### 6.3 No Auto Behavior

AI Assistant must not:

```text
auto-open on caret movement
auto-open on diagnostics
auto-open on project load
auto-send context
auto-apply edits
```

---

## 7. AI Assistant Panel Layout

The AI tab/view should contain:

```text
Header
Context Summary
Task Kind Selector
Prompt Input
Actions
Response Area
Draft Preview Area
Safety Footer
```

### Header

```text
AI 助手
关闭 / 返回 Section
```

### Context Summary

Must show what will be sent before any request:

```text
当前文件
当前 Section
当前 Key / Value
附近行数
字段库提示数量
诊断数量
```

### Task Kind Selector

Initial task kinds:

```text
解释字段
解释 Section
解释引用
解释诊断
建议 Value
生成草稿
```

### Actions

```text
生成回复
取消
复制结果
清空
```

No Apply button in AI-1.

### Safety Footer

Always show:

```text
AI 输出仅作为草稿，不会自动修改文件。应用修改前需要预览和确认。
```

---

## 8. AI Phase Impact

### AI-0P-R2

This task only updates placement contract.

### AI-0

Architecture / Safety Contract must reflect:

```text
AI Assistant placement target = right-side tool well tab/view.
No non-modal tool window fallback.
Shell change requires separate approval.
```

### AI-1

AI-1 may implement the right-side AI tab/view only after explicit approval.

AI-1 still uses:

```text
MockRa2AiClient
```

AI-1 must not implement:

```text
DeepSeek
real network client
apply
file modification
whole-project context
```

---

## 9. Shell Boundaries

Future implementation may touch Shell only in an approved AI-1 implementation phase.

Before that, this contract task must not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
Navigator / Section tree logic
Project Explorer
Source Editor layout
Issues / Search panels
```

---

## 10. AutomationId Planning

Future implementation should preserve current Section tree AutomationIds.

Suggested new AI / Right Tool Well IDs:

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
```

Final IDs must be adjusted after inspecting actual Shell structure.

---

## 11. Tests to Plan

Future implementation tests should verify:

```text
1. Section tree default view remains available.
2. Section tree AutomationIds remain.
3. AI tab/view exists.
4. AI tab/view is not active by default if default state is testable.
5. AI opens only through explicit command.
6. AI close returns to Section Tree or previous view.
7. No apply button exists in AI-1.
8. Context summary exists.
9. Safety footer exists.
```

Avoid pixel-perfect tests.

---

## 12. Non-goals

Do not implement:

```text
AI Agent logic
DeepSeek client
prompt builder
context provider
apply / insert
draft diff
project-wide indexing
floating AI window
bottom AI panel
new docking framework
```

---

## 13. Suggested Codex Prompt

```text
请读取 AGENTS.md、Docs/RA2IniEditor_IDE_Full_Codex_Context.md、Docs/Codex_CurrentPhase.md，以及 Docs/Codex_RA2IniEditor_IDE_AI_0P_R2_RightToolWellDirectIntegration.md。

不要修改源码。请只创建/更新 Docs/AiAgentPanelPlacementContract.md，明确 AI Assistant 将直接集成到现有右侧 Section 区域，将其改造为 Right Tool Well；Section 树为默认页，AI 为第二页；不考虑非模态 AI 窗口，不新增第二右栏，不实现 UI。
```

---

## 14. Acceptance Criteria

This contract is accepted when it states:

```text
1. AI Assistant integrates into existing right-side Section area.
2. Section Tree remains default.
3. AI is an additional right-side tab/view.
4. No non-modal AI window fallback.
5. No second right sidebar.
6. AI opens only by explicit command.
7. Closing AI returns to Section Tree / previous view.
8. AI-1 remains mock-client and preview-only.
9. Shell implementation requires separate approval.
```
