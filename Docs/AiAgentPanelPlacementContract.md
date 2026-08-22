# AI Agent Panel Placement Contract

## 1. Scope

This document defines the placement contract for the future RA2IniEditor.IDE AI Assistant panel.

This is a contract-only document. It does not implement UI, does not modify Shell code, and does not introduce AI runtime behavior.

## 2. Final Placement Decision

The AI Assistant will be integrated directly into the existing right-side Section area.

The existing right-side Section / Navigator region will become a shared:

```text
Right Tool Well
```

The Right Tool Well will host:

```text
Default page: Section Tree / Navigator
Second page: AI Assistant
```

Section Tree remains the default page. AI Assistant is available only as an additional right-side page/view.

## 3. Rejected Placement Options

The following options are explicitly rejected for the primary AI Assistant surface:

```text
non-modal AI tool window
second independent right sidebar
bottom AI tab
modal AI dialog
caret-near AI popup
AI overlay over Source Editor
new docking framework
```

Do not implement or propose a non-modal AI window fallback for this phase.

Do not add a second right column.

## 4. Right Tool Well Behavior

The existing right-side area must be converted conservatively.

Required behavior:

```text
Section Tree remains the default right-side view.
AI Assistant is inactive by default.
AI Assistant opens only by explicit user command.
AI Assistant does not auto-open on startup.
Closing AI returns to Section Tree or the previously active right-side view.
The right-side footprint should remain close to the existing Section area width.
AI content must not resize the whole Shell unpredictably.
```

The existing Section tree selection and jump behavior must remain unchanged.

## 5. AI Assistant Initial Surface

The future AI page should contain:

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

The Context Summary must show what will be sent before any request:

```text
current file
current Section
current Key / Value
nearby line count
field library hint count
diagnostic count
```

Initial task kinds:

```text
explain field
explain Section
explain reference
explain diagnostic
suggest Value
generate draft
```

Initial actions:

```text
generate response
cancel
copy result
clear
```

AI-1 must not include an Apply button.

The Safety Footer must always communicate:

```text
AI output is a draft only and will not modify files automatically. Preview and confirmation are required before applying any change.
```

## 6. Non-Automatic Behavior

The AI Assistant must not:

```text
auto-open on caret movement
auto-open on diagnostics
auto-open on project load
auto-send context
auto-apply edits
modify files directly
```

AI opens only through an explicit command.

## 7. Implementation Gate

This contract does not authorize implementation.

Future implementation requires a separate approved implementation phase.

Before implementation, Codex must inspect and report:

```text
current right-side Section tree XAML region
current control type
current ViewModel / DataContext for Section tree
current AutomationIds
current width / Grid column sizing
current entry points that update Section tree
whether adding a TabControl would break existing bindings
whether a ContentControl with view switching is safer than TabControl
exact files that would need changes
risk level
```

Implementation must not begin until that inspection is complete and approved.

## 8. Shell Boundaries

Future AI-1 implementation may touch Shell only after explicit approval.

This contract task must not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
Navigator / Section tree logic
Project Explorer
Source Editor layout
Issues / Search panels
Field Registry behavior
INI parser behavior
legacy files
solution / project files
```

## 9. AutomationId Planning

Existing Section tree AutomationIds must be preserved.

Suggested future Right Tool Well AutomationIds:

```text
RightToolWell.Root
RightToolWell.SectionTab
RightToolWell.AiTab
RightToolWell.ActiveView
```

Suggested future AI Assistant AutomationIds:

```text
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

Final AutomationIds must be adjusted after inspecting the actual Shell structure.

## 10. AI Phase Boundaries

AI-0P-R2:

```text
placement contract only
no source code changes
no UI implementation
```

AI-0:

```text
architecture / safety contract must reflect right-side tool well placement
no non-modal AI window fallback
no second right sidebar
```

AI-1:

```text
may implement right-side AI tab/view only after explicit approval
uses MockRa2AiClient
preview-only
no DeepSeek
no real network client
no apply
no file modification
no whole-project context
```

## 11. Future Test Plan

Future implementation tests should verify:

```text
Section Tree remains default view
Section Tree AutomationIds remain
AI tab/view exists
AI tab/view is not active by default if testable
AI opens only through explicit command
AI close returns to Section Tree or previous view
AI-1 has no Apply button
Context Summary exists
Safety Footer exists
```

Tests should avoid pixel-perfect assertions.

## 12. Acceptance Criteria

This contract is accepted when:

```text
AI Assistant integrates into the existing right-side Section area.
The existing right-side area becomes a Right Tool Well.
Section Tree remains the default tab/view.
AI Assistant is the second tab/view.
AI does not auto-open on startup.
AI opens only by explicit command.
Closing AI returns to Section Tree or previous right-side view.
There is no non-modal AI tool window fallback.
There is no second right sidebar.
AI-1 remains mock-client and preview-only.
Shell implementation requires a separate approved phase.
No source files are modified by this contract task.
```
