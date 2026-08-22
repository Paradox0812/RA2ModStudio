# Codex Task: RA2IniEditor.IDE AI-1B Right Tool Well Frame / Empty AI Page

## 0. Current Baseline

AI-1A has been completed.

Reported state:

```text
Docs/AiAssistantRightToolWellImplementationContract.md created.
Current right-side area is ProjectExplorerPanel inside ProjectExplorerColumn.
Default width is 320.
Current Section / Navigator is ProjectExplorerTreeView bound to ProjectExplorer.Items.
ProjectExplorerTreeView is referenced directly by code-behind for selection, focus, BringIntoView, and container lookup.
Right-side visibility is controlled by:
  IsProjectExplorerVisible
  ApplyProjectExplorerVisibility()
  ProjectExplorerSplitterColumn
  ProjectExplorerColumn
Tests: 1298 passed.
IdeOnly package: passed, packaged file count 672.
No source code changed.
Legacy not restored.
```

Next phase:

```text
AI-1B: Right Tool Well frame + Section default view + explicit AI empty page open/close
```

This is a limited implementation task.

Do not implement MockRa2AiClient generation logic in this phase.

---

## 1. Goal

Convert the existing right-side Project Explorer / Section area into a conservative Right Tool Well frame.

AI-1B must add only:

```text
1. A right-side tool well container around the existing Section Tree.
2. Section Tree remains the default visible view.
3. An AI Assistant empty/skeleton page exists as a second view.
4. A user command can explicitly open the AI view.
5. The AI view can close/return to Section Tree.
6. Existing ProjectExplorerTreeView x:Name, binding, selection, focus, BringIntoView, and container lookup behavior remain unchanged.
```

AI-1B must not implement real AI behavior.

---

## 2. Hard Boundaries

Do not implement:

```text
MockRa2AiClient response generation
DeepSeek client
network calls
API key configuration
context provider
prompt builder
AI apply / insert
file modification
field registry writes
whole-project context
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

Shell changes are allowed only inside the existing right-side ProjectExplorerPanel / right tool well region and minimal command wiring.

Do not redesign the main Shell.

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

Allowed only if the project already has a suitable style/resource location:

```text
RA2IniEditor.IDE/Views/IdeSecondaryWindowStyles.xaml
```

Do not create broad new framework abstractions in AI-1B.

---

## 4. Required Implementation Strategy

### 4.1 Preserve existing ProjectExplorerTreeView

The existing TreeView must remain:

```text
same x:Name
same ItemsSource binding
same SelectionChanged / MouseDoubleClick / keyboard behavior if present
same AutomationId
same code-behind accessibility
```

Do not wrap it in a way that prevents current code-behind container lookup or BringIntoView from working.

### 4.2 Add local view switching

Use a conservative local view switch inside the existing right-side region.

Preferred options:

```text
1. Small tab/header row with two toggle buttons:
   Sections / AI

2. Content host with both views and Visibility switching:
   Section view visible by default
   AI view hidden by default
```

Avoid if risky:

```text
Replacing ProjectExplorerTreeView with an entirely new TabControl that changes its visual tree too much.
```

If TabControl is used, preserve direct access to ProjectExplorerTreeView.

### 4.3 Default state

At startup:

```text
Section Tree / Navigator is visible.
AI Assistant is hidden/inactive.
```

AI must not auto-open on:

```text
startup
project load
file open
caret movement
diagnostic update
section selection
```

### 4.4 Explicit AI open command

Add a user-visible command/entry point only if safe.

Acceptable:

```text
Toolbar/menu button: AI 助手
```

This command switches the right tool well to AI view and ensures right panel is visible.

Do not add keyboard shortcut in AI-1B unless already trivial and approved.

### 4.5 AI close behavior

AI skeleton page should have:

```text
AI close button
```

Behavior:

```text
Close AI -> return to Section Tree
```

No state persistence required in AI-1B.

---

## 5. AI Skeleton Page Layout

AI-1B should show a non-functional skeleton only.

Required regions:

```text
Header
Context Summary placeholder
Task Kind Selector placeholder
Prompt Input placeholder
Action row placeholder
Response Area placeholder
Draft Preview placeholder
Safety Footer
```

Suggested text:

```text
AI 助手
当前阶段：Mock / 预览占位
上下文摘要将在后续阶段显示
AI 输出仅作为草稿，不会自动修改文件
```

Required AutomationIds:

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

Buttons may be disabled placeholders in AI-1B.

Do not implement generation logic.

---

## 6. ShellViewModel / State

If a ViewModel property is needed, keep it small and UI-only.

Allowed display/navigation state:

```text
RightToolWellActiveView
IsAiAssistantVisible
```

or equivalent.

This state must not trigger AI generation or context collection.

Do not store AI prompts/responses in AI-1B unless needed as placeholder static text.

---

## 7. Tests

Add/update boundary tests only.

Required tests:

```text
1. Existing ProjectExplorerTreeView AutomationId remains.
2. Existing ProjectExplorerPanel / right-side region remains.
3. RightToolWell.Root exists.
4. RightToolWell.SectionTab exists.
5. RightToolWell.AiTab exists.
6. AiAssistant.Panel exists.
7. AiAssistant.SafetyFooter exists.
8. No Apply button exists in AI-1B.
9. ProjectExplorerTreeView x:Name is preserved if tested by source boundary.
10. ShellWindow.xaml.cs still references ProjectExplorerTreeView successfully.
```

If feasible:

```text
1. default view is Section.
2. AI open command exists but does not generate output.
3. AI close returns to Section view.
```

Avoid pixel-perfect tests.

---

## 8. Manual Smoke Checklist

After implementation:

```text
1. Launch RA2IniEditor.IDE.
2. Confirm right-side Section tree is visible by default.
3. Confirm Section selection/jump still works.
4. Open AI Assistant explicitly from the new command.
5. Confirm right-side region switches to AI skeleton page.
6. Confirm AI skeleton has context summary placeholder and safety footer.
7. Confirm Generate/Cancel/Copy/Clear do not perform real AI work.
8. Close AI and confirm Section tree returns.
9. Confirm Source Editor width is not unexpectedly compressed beyond existing right panel width.
10. Confirm no file modification occurs.
```

---

## 9. Validation Commands

Run full validation because Shell XAML/code-behind changes are expected:

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
1. Phase completed: AI-1B.
2. Files changed.
3. Right Tool Well implementation strategy.
4. How ProjectExplorerTreeView was preserved.
5. AI skeleton UI added.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no AI generation logic was implemented.
11. Confirmation no DeepSeek/network/API key was added.
12. Confirmation no file modification behavior was added.
13. Confirmation legacy not restored.
14. Manual smoke steps or result.
15. Remaining risks.
16. Recommended next phase.
```
