# Codex Task: RA2IniEditor.IDE A15-2E Field Learning Wizard Chrome / Workflow Layout Contract

## 0. Current Baseline

A15-2D has been completed.

Reported state:

```text
Phase: A15-2D Field Editor / Allowed Values Editor custom chrome
Build: passed, 0 warnings / 0 errors
Tests: passed, 1298 passed
IdeOnly package: passed, packaged file count 657
Shell: unchanged
Field Editor save/apply semantics: unchanged
Allowed Values DialogResult / ResultText semantics: unchanged
Legacy table-style editor: not restored
```

A15-2D modified only:

```text
FieldEditorWindow.xaml / .xaml.cs
AllowedValuesEditorWindow.xaml / .xaml.cs
Ra2FieldEditorWindowBoundaryTests.cs
WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Next phase:

```text
A15-2E: Field Learning Wizard chrome / workflow layout
```

This task is **contract/planning first**.

Do not implement UI changes in this task.

---

## 1. Goal

Prepare a strict implementation contract for `FieldLearningWizardWindow`.

The Field Learning Wizard is a multi-step workflow surface, not a simple editor dialog.

The contract must clarify how to:

```text
1. Remove default WPF chrome.
2. Add custom lightweight tool-window chrome.
3. Preserve resize and move behavior.
4. Preserve existing learning workflow semantics.
5. Make the workflow structure clearer:
   source -> parse -> draft/review -> build apply plan -> apply
6. Preserve all existing commands, bindings, AutomationIds, and confirmation flows.
```

Do not implement until the user approves the final contract.

---

## 2. Hard Boundaries

Do not modify in this contract stage:

```text
XAML
code-behind
ViewModels
tests
scripts
field registry services
solution / project files
legacy files
```

Do not change behavior in any later implementation unless explicitly approved:

```text
UseCurrentIni behavior
ParsePastedText behavior
BuildApplyPlan behavior
ApplyCurrentPlan behavior
CreateApplyConfirmation behavior
ApplyConfirmed behavior
TargetScope behavior
ApplyMode behavior
Generalization behavior
Validation behavior
Preview diff behavior
Allowed Values editing behavior
Field Registry apply/write behavior
Backup manifest behavior
Diagnostics / Completion / Hover behavior
Save / Dirty behavior
```

Do not restore:

```text
RA2IniEditor.sln
RA2IniEditor.csproj
legacy MainWindow
legacy table-style editor
legacy object workbench
```

---

## 3. Documents to Read First

Before inspecting source, read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/FieldRegistrySurfacesUiContract.md
Docs/FieldRegistryTertiarySurfacesUiContract.md
Docs/FieldEditorAndLearningChromeContract.md
```

Then inspect the current Field Learning Wizard implementation.

---

## 4. Required Inspection

Inspect and report:

```text
1. FieldLearningWizardWindow.xaml path.
2. FieldLearningWizardWindow.xaml.cs path.
3. ViewModel / DataContext type.
4. Open/show path from Shell / Field Registry Center.
5. Modal / non-modal behavior.
6. Owner assignment.
7. Reuse/activation behavior if already open.
8. Current WindowStyle.
9. ShowInTaskbar.
10. ResizeMode.
11. SizeToContent.
12. WindowStartupLocation.
13. Width / Height / MinWidth / MinHeight.
14. Existing AutomationIds.
15. Existing tests.
16. Which commands write state and which are read-only.
17. Current workflow sections and layout problems.
```

Do not edit files during inspection.

---

## 5. Current Surface Classification

Classify the wizard as:

```text
Workflow Dialog / Tool Window
```

It is not:

```text
small inspector
simple editor form
Field Registry Manager
Import Preview window
```

Future implementation must preserve that it is a workflow with staged actions.

---

## 6. Expected Current Workflow Sections

Classify current UI into these sections:

```text
Step 1: Source
  - current INI source
  - pasted raw text
  - source name

Step 2: Parse
  - Use current INI
  - Parse pasted text
  - parse status

Step 3: Target / Mode
  - Project / Global target scope
  - apply mode
  - generalization summary / warnings

Step 4: Review
  - current INI drafts
  - preview diff
  - validation issues

Step 5: Apply Plan
  - build apply plan
  - apply plan table
  - status

Step 6: Apply
  - apply button
  - confirmation
  - apply result / registry write path
```

If a step does not exist in the current implementation, report it as missing.

---

## 7. Proposed Future UI Direction

The future implementation should make Field Learning Wizard feel like an IDE workflow window.

Preferred visual language:

```text
custom lightweight chrome
Chinese-first labels
compact header
workflow step strip or section headers
source / target / review / apply separation
status chips
warning summary
bounded scroll regions for large text/table areas
explicit apply boundary
clear disabled reasons
```

Avoid:

```text
default WPF icon/titlebar/system border
large unstructured form layout
huge blank editor area
ambiguous button clusters
mixing read-only parse actions with apply/write actions
hidden target scope
English-heavy labels
changing workflow semantics
```

---

## 8. Custom Chrome Contract

Future implementation should apply:

```text
WindowStyle=None
ResizeMode=CanResize
WindowChrome to preserve resize
custom lightweight header
custom close button
remove default WPF icon/titlebar/system border
preserve Owner / Show behavior
preserve non-modal behavior
```

Recommended AutomationIds to add:

```text
FieldLearningWizard.CustomChrome
FieldLearningWizard.ChromeTitle
FieldLearningWizard.CloseButton
```

Close behavior must be equivalent to existing window close/cancel semantics and must not apply changes.

---

## 9. AutomationIds to Preserve

Preserve all existing `FieldLearningWizard.*` AutomationIds, including:

```text
FieldLearningWizard.Window
FieldLearningWizard.HeaderArea
FieldLearningWizard.LearningSourceText
FieldLearningWizard.UseCurrentIniButton
FieldLearningWizard.ParsePastedTextButton
FieldLearningWizard.BuildApplyPlanButton
FieldLearningWizard.ApplyButton
FieldLearningWizard.SourceSection
FieldLearningWizard.SourceNameTextBox
FieldLearningWizard.RawTextBox
FieldLearningWizard.ApplyTargetSection
FieldLearningWizard.TargetScopeComboBox
FieldLearningWizard.ApplyModeComboBox
FieldLearningWizard.GeneralizationApplySummaryText
FieldLearningWizard.GeneralizationWarningSummaryText
FieldLearningWizard.MainTabs
FieldLearningWizard.CurrentIniDraftsTab
FieldLearningWizard.CurrentIniDraftsGrid
FieldLearningWizard.EditAllowedValuesButton
FieldLearningWizard.PreviewDiffTab
FieldLearningWizard.PreviewDiffGrid
FieldLearningWizard.ValidationIssuesTab
FieldLearningWizard.ValidationIssuesGrid
FieldLearningWizard.ApplyPlanTab
FieldLearningWizard.ApplyPlanGrid
FieldLearningWizard.StatusText
```

Do not rename existing AutomationIds.

---

## 10. Tests to Plan

Plan boundary tests for future implementation:

```text
1. FieldLearningWizardWindow has WindowStyle=None.
2. ResizeMode=CanResize remains.
3. WindowChrome exists.
4. FieldLearningWizard.CloseButton exists.
5. Existing FieldLearningWizard.* AutomationIds remain.
6. UseCurrentIni / ParsePastedText / BuildApplyPlan / Apply button handlers remain present.
7. CreateApplyConfirmation / ApplyConfirmed flow remains present.
8. Opening the wizard does not write registry files.
9. Apply remains gated by existing plan/confirmation behavior.
```

Avoid pixel-perfect tests.

---

## 11. Proposed Implementation Split

The contract should recommend whether A15-2E can be implemented in one limited pass or should be split.

Preferred split:

```text
A15-2E-1: Custom chrome + header only.
A15-2E-2: Workflow section layout / bounded scroll areas.
A15-2E-3: Localization and warning/disabled reason polish.
```

If inspection shows the existing XAML is small enough, propose a single limited implementation but keep behavior constraints strict.

---

## 12. Output Required

Create or update:

```text
Docs/FieldLearningWizardWorkflowContract.md
```

Suggested structure:

```markdown
# Field Learning Wizard Workflow Contract

## 1. Scope and Baseline

## 2. Current Implementation Inventory

## 3. Current Window Properties

## 4. Current Workflow

### 4.1 Source
### 4.2 Parse
### 4.3 Target / Mode
### 4.4 Review
### 4.5 Apply Plan
### 4.6 Apply

## 5. Current UX Problems

## 6. Proposed Custom Chrome Rules

## 7. Proposed Workflow Layout

## 8. AutomationIds to Preserve

## 9. Display-only Properties Needed

## 10. Commands / Handlers to Reuse

## 11. Semantic Boundaries

## 12. Tests to Add / Update

## 13. Risks

## 14. Recommended Implementation Plan

## 15. Acceptance Criteria
```

---

## 13. Validation Commands

For this contract/document-only phase:

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

## 14. Final Report Format

Report:

```text
1. Phase completed: A15-2E contract.
2. Files changed.
3. Files inspected.
4. Current workflow summary.
5. Current chrome/layout problems.
6. Proposed implementation split.
7. Commands run.
8. Test result.
9. Package result.
10. Confirmation no source/XAML/ViewModel behavior changed.
11. Confirmation Field Registry semantics unchanged.
12. Confirmation legacy not restored.
13. Recommended next phase.
```
