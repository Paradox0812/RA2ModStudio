# Codex Task: RA2IniEditor.IDE AI-1A Right Tool Well Inspection / Mock AI Panel Implementation Contract

## 0. Current Baseline

AI-0 has been completed.

Accepted state:

```text
Docs/AiAssistantArchitecture.md created.
Docs/AiAssistantSafetyContract.md created.
AI feature is defined as DeepSeek-powered RA2 Modding Assistant.
It is not a Codex-like file editing agent.
DeepSeek is text-generation backend only.
Initial workflow is draft/copy oriented.
AI placement target is existing right-side Section area -> Right Tool Well.
```

Validation from AI-0:

```text
dotnet test: 1298 passed
IdeOnly package: passed
legacy not restored
source code not changed
```

This task starts the next phase:

```text
AI-1A: Right Tool Well inspection and Mock AI Panel implementation contract
```

This is **planning / contract first**.

Do not implement UI in this task.

---

## 1. Goal

Inspect the current right-side Section / Navigator area and produce a precise implementation contract for AI-1.

AI-1 implementation should later add a right-side AI Assistant tab/view using a mock client only.

This phase must determine:

```text
1. Current Shell structure around the right-side Section area.
2. Whether TabControl or ContentControl view switching is safer.
3. Which files must be modified.
4. Which AutomationIds must be preserved.
5. How Section Tree remains default.
6. How AI Assistant opens by explicit command.
7. How AI Assistant closes and returns to Section Tree / previous right-side view.
8. How to keep AI-1 mock-only and preview-only.
```

---

## 2. Hard Boundaries

Do not modify any files in this planning task.

Do not implement:

```text
AI panel UI
Right Tool Well
MockRa2AiClient
context provider
prompt builder
DeepSeek client
network calls
apply / insert flow
file modification
```

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
ViewModels
tests
project files
scripts
Field Registry logic
parser / diagnostics / completion / hover
legacy files
```

---

## 3. Documents to Read First

Read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/AiAgentPanelPlacementContract.md
Docs/AiAssistantArchitecture.md
Docs/AiAssistantSafetyContract.md
```

Then inspect current Shell and right-side Section implementation.

---

## 4. Required Source Inspection

Inspect and report:

```text
1. ShellWindow.xaml right-side Section / Navigator region.
2. ShellWindow.xaml.cs methods that populate or react to Section tree / Navigator selection.
3. Current ViewModel or data source for Section tree.
4. Current AutomationIds for Section tree / Navigator.
5. Current right-side width / Grid column sizing.
6. Whether the right-side region is a Grid column, ContentControl, ListBox, TabControl, or custom control.
7. Whether adding a TabControl would break existing bindings or tests.
8. Whether a ContentControl + local mode enum is safer.
9. Existing tests covering the right-side Section region.
10. Existing toolbar/menu command patterns suitable for opening AI Assistant.
```

Do not edit files.

---

## 5. Required Contract Output

Create or update:

```text
Docs/AiAssistantRightToolWellImplementationContract.md
```

This document must contain:

```markdown
# AI Assistant Right Tool Well Implementation Contract

## 1. Scope and Baseline

## 2. Current Shell / Section Region Inventory

## 3. Current AutomationIds to Preserve

## 4. Proposed Right Tool Well Strategy

## 5. AI Assistant Panel Layout

## 6. Mock Client Boundary

## 7. Commands / Entry Points

## 8. Files Proposed for Implementation

## 9. Tests to Add / Update

## 10. Semantic and Safety Boundaries

## 11. Risks

## 12. Recommended AI-1 Implementation Plan

## 13. Acceptance Criteria
```

---

## 6. Required AI-1 Design Constraints

The contract must enforce:

```text
1. Existing Section Tree remains default.
2. Existing Section Tree behavior remains unchanged.
3. AI Assistant is an additional right-side tab/view.
4. AI opens only by explicit command.
5. AI does not auto-open on startup, caret movement, diagnostics, file open, or project load.
6. AI-1 uses MockRa2AiClient only.
7. AI-1 has no DeepSeek, no network, no real API key.
8. AI-1 has no Apply button.
9. AI-1 cannot modify files.
10. AI-1 cannot update Field Registry.
11. AI-1 cannot execute shell commands.
```

---

## 7. Initial AI Panel Layout Contract

The AI tab/view should include:

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

Required proposed AutomationIds:

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

Final names may be adjusted after actual Shell inspection, but the contract must list a stable plan.

---

## 8. Initial Task Kinds

AI-1 may only display task kinds. Mock response may vary by selection.

Initial task kinds:

```text
解释字段
查找相关字段
生成单位原型
生成武器链草案
审查 INI 片段
解释诊断
```

No task kind may write files.

---

## 9. Mock Client Boundary

AI-1 implementation contract must require:

```text
MockRa2AiClient only
deterministic responses
no network
no API key
no DeepSeek
no file writes
no project scan
no apply / insert
```

Mock response should be enough to verify UI state:

```text
busy
cancel
response display
copy
clear
empty prompt
error state if simulated
```

---

## 10. Tests to Plan

Plan tests for:

```text
1. Section Tree remains present.
2. Section Tree default view remains default.
3. AI tab/view exists.
4. AI opens only by explicit command.
5. AI close returns to Section Tree or previous view.
6. No Apply button exists in AI-1.
7. Context Summary exists.
8. Safety Footer exists.
9. Mock generate does not modify source editor text.
10. Mock generate does not mark document dirty.
11. Mock generate does not write files.
```

Avoid pixel-perfect tests.

---

## 11. Validation Commands

For this contract/document-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 12. Final Report Format

Report:

```text
1. Phase completed: AI-1A Right Tool Well inspection / implementation contract.
2. Files changed.
3. Files inspected.
4. Current Shell / right-side Section structure.
5. Proposed Right Tool Well strategy.
6. Proposed files for AI-1 implementation.
7. Tests proposed.
8. Commands run.
9. Test result.
10. Package result.
11. Confirmation no source code changed.
12. Recommended next phase.
```
