# Codex Task: RA2IniEditor.IDE A15-2E-2 Field Learning Wizard Workflow Layout / Bounded Scroll Areas

## 0. Current Baseline

A15-2E-1 has been manually accepted by the user.

Current accepted state:

```text
Field Learning Wizard custom chrome + header completed.
Default WPF title icon / system title bar / outer system border removed.
Custom close button exists.
Window remains a large workflow window.
Resize / move behavior should remain.
Field Learning workflow semantics unchanged.
Shell unchanged.
Legacy not restored.
```

This task starts:

```text
A15-2E-2: Field Learning Wizard workflow section layout / bounded scroll areas
```

This is a **limited implementation** task.

Do not perform A15-2E-3 localization/warning polish in this task except for tiny section labels required by layout clarity.

---

## 1. Goal

Improve the internal layout of `FieldLearningWizardWindow` so the workflow is easier to understand and large content regions do not dominate the window.

The workflow should become clearer as:

```text
Source -> Parse -> Target / Mode -> Review -> Apply Plan -> Apply
```

Primary goals:

```text
1. Make workflow sections visually clearer.
2. Give large text/table regions bounded scroll behavior.
3. Reduce oversized blank regions.
4. Keep existing tabs, commands, bindings, and workflow behavior.
5. Preserve A15-2E-1 custom chrome.
```

---

## 2. Target Surface

Only:

```text
FieldLearningWizardWindow
```

Expected files:

```text
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project paths.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs, only for local UI wiring if strictly required
RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if strictly necessary and display-only:

```text
RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs
```

Prefer not to modify the ViewModel. If a display-only property is needed, it must be derived from existing state only.

---

## 4. Files Forbidden

Do not modify:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/Views/FieldEditorWindow.xaml
RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml
RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml
RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml
Field Registry services
Field Registry apply/write services
parser / normalization / validation services
BuiltIn field registry JSON
solution / project files
legacy files
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

## 5. Semantic Boundaries

Do not change:

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

Opening the wizard must not write registry files.

Parsing must remain explicit.

Build Apply Plan must remain explicit.

Apply must remain confirmation-gated.

---

## 6. Required Pre-Implementation Inspection

Before editing, inspect and report:

```text
1. Current FieldLearningWizardWindow.xaml layout root.
2. Current row/column sizing.
3. Current SourceSection structure.
4. Current ApplyTargetSection structure.
5. Current MainTabs layout and tab content.
6. Existing scroll hosts, if any.
7. Existing large fixed height / star-sized areas.
8. Existing AutomationIds.
9. Existing boundary tests.
```

Then implement only the bounded layout changes described below.

---

## 7. Required Layout Changes

### 7.1 Workflow Header / Step Strip

Add or improve a compact workflow indicator near the top of the content area.

Suggested text:

```text
来源 -> 解析 -> 目标 -> 预览 -> 应用计划 -> 应用
```

or chip-style sections:

```text
来源
解析
目标
预览
应用计划
应用
```

Required AutomationId:

```text
FieldLearningWizard.WorkflowStepStrip
```

This is display-only. It must not drive workflow state.

### 7.2 Source Section

The source section currently contains source name and raw text.

Requirements:

```text
1. Keep SourceNameTextBox.
2. Keep RawTextBox.
3. Put RawTextBox inside a bounded area if it can grow too large.
4. Source helper/status text should remain visible.
5. Do not change parsing behavior.
```

Allowed new AutomationIds:

```text
FieldLearningWizard.SourceScrollHost
FieldLearningWizard.SourceSummary
```

### 7.3 Target / Mode Section

The target/mode area should remain visible and compact.

Requirements:

```text
1. Keep TargetScopeComboBox.
2. Keep ApplyModeComboBox.
3. Keep GeneralizationApplySummaryText.
4. Keep GeneralizationWarningSummaryText.
5. Reduce excessive vertical whitespace if present.
6. Do not change target/mode behavior.
```

Allowed new AutomationId:

```text
FieldLearningWizard.TargetModeSummary
```

### 7.4 Main Tabs / Review Area

The existing tabs must remain:

```text
FieldLearningWizard.MainTabs
FieldLearningWizard.CurrentIniDraftsTab
FieldLearningWizard.PreviewDiffTab
FieldLearningWizard.ValidationIssuesTab
FieldLearningWizard.ApplyPlanTab
```

Requirements:

```text
1. Keep all existing tabs.
2. Keep all existing grids.
3. Give tab content a bounded height or internal scroll where needed.
4. Do not let empty grids consume excessive vertical space.
5. Do not change tab content semantics.
```

Allowed new AutomationIds:

```text
FieldLearningWizard.ReviewScrollHost
FieldLearningWizard.EmptyReviewState
```

### 7.5 Apply Boundary

The Apply area must remain clearly separated as a write action.

Requirements:

```text
1. Keep ApplyButton.
2. Keep BuildApplyPlanButton.
3. BuildApplyPlan remains read-only/planning action.
4. Apply remains write/confirmation-gated action.
5. Do not move Apply into header chrome.
6. Do not change confirmation flow.
```

Allowed new AutomationId:

```text
FieldLearningWizard.ApplyBoundaryPanel
```

---

## 8. AutomationIds to Preserve

Preserve all existing `FieldLearningWizard.*` AutomationIds:

```text
FieldLearningWizard.Window
FieldLearningWizard.CustomChrome
FieldLearningWizard.ChromeTitle
FieldLearningWizard.CloseButton
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

## 9. Allowed New AutomationIds

Add where appropriate:

```text
FieldLearningWizard.WorkflowStepStrip
FieldLearningWizard.SourceScrollHost
FieldLearningWizard.SourceSummary
FieldLearningWizard.TargetModeSummary
FieldLearningWizard.ReviewScrollHost
FieldLearningWizard.EmptyReviewState
FieldLearningWizard.ApplyBoundaryPanel
```

---

## 10. Visual Rules

Prefer:

```text
compact section headers
bounded scroll areas
workflow chips
clear source / target / review / apply separation
less blank space
Chinese-first section labels
existing button text preserved where tests depend on it
```

Avoid:

```text
full workflow redesign
moving apply button to chrome
changing tab model
large empty TextBox/Grid areas
pixel-perfect layout assumptions
English-heavy new labels
```

---

## 11. Tests

Update boundary tests only.

Required checks:

```text
1. WorkflowStepStrip exists.
2. SourceScrollHost or equivalent bounded source area exists.
3. ApplyBoundaryPanel exists.
4. Existing FieldLearningWizard.* AutomationIds remain.
5. UseCurrentIniButton remains.
6. ParsePastedTextButton remains.
7. BuildApplyPlanButton remains.
8. ApplyButton remains.
9. WindowStyle=None remains from A15-2E-1.
10. WindowChrome remains from A15-2E-1.
```

Do not write pixel-perfect tests.

Do not weaken existing workflow behavior tests.

---

## 12. Validation Commands

Run full validation because XAML changes are expected:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 13. Manual Smoke Checklist

After implementation:

```text
1. Open Field Learning Wizard.
2. Confirm custom chrome from A15-2E-1 still exists.
3. Confirm workflow step strip is visible.
4. Confirm Source / Target / Review / Apply areas are easier to understand.
5. Confirm source raw text area does not dominate the whole window.
6. Confirm tabs/grids still work.
7. Confirm Use Current INI works.
8. Confirm Parse Pasted Text works.
9. Confirm Build Apply Plan works.
10. Confirm Apply still uses existing confirmation flow.
11. Confirm opening/closing does not write registry files.
12. Confirm Shell layout is unchanged.
```

---

## 14. Final Report Format

Report:

```text
1. Phase completed: A15-2E-2.
2. Files changed.
3. Layout sections changed.
4. New AutomationIds added.
5. Commands run.
6. Build result.
7. Test result.
8. Package result.
9. Confirmation Field Learning workflow semantics unchanged.
10. Confirmation Shell unchanged.
11. Confirmation legacy not restored.
12. Manual smoke steps or result.
13. Remaining risks.
14. Recommended next phase.
```
