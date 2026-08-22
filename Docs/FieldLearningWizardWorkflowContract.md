# Field Learning Wizard Workflow Contract

## 1. Scope and Baseline

This document defines the A15-2E contract for the Field Learning Wizard.

Current baseline:

```text
RA2IniEditor.IDE-only
A15-2D Field Editor / Allowed Values Editor custom chrome completed
build/test/package passed after A15-2D
Shell main layout unchanged
Field Registry semantics unchanged
legacy table-style editor not restored
```

A15-2E target:

```text
Field Learning Wizard chrome / workflow layout
```

The Field Learning Wizard is a workflow dialog / tool window. It is not a small inspector, a simple editor form, Field Registry Manager, or the Field Import Preview window.

This document is contract-only. It does not implement UI changes.

## 2. Current Implementation Inventory

Current implementation files:

```text
XAML:
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml

code-behind:
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs

ViewModel / DataContext:
RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs
```

The constructor assigns the supplied `FieldRegistryHarvestPreviewViewModel` as DataContext:

```text
DataContext = _viewModel
```

Open/show paths:

```text
ShellWindow.OpenFieldLearningWizardWindow(...)
FieldRegistryCenterWindow.FieldLearningRequested -> Shell open path
FieldRegistryManagerWindow.RelearnCurrentIniRequested -> Shell open path
Shell current INI / current section learning commands -> Shell open path
```

Open behavior:

```text
non-modal Show()
Owner = ShellWindow
```

Reuse behavior:

```text
If _fieldLearningWizardWindow is visible, Shell reloads initial source when provided, activates the existing window, and does not create a duplicate.
Closed handler clears _fieldLearningWizardWindow.
```

Current role:

```text
Loads current INI or pasted text.
Parses and previews field drafts.
Allows row-level allowed-values editing through AllowedValuesEditorWindow.
Builds registry apply plans.
Applies only after existing confirmation flow.
```

Existing tests:

```text
RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
FieldRegistryHarvestPreviewViewModel tests cover shared parse/apply semantics.
```

## 3. Current Window Properties

Current XAML window properties:

```text
AutomationId: FieldLearningWizard.Window
Title: {Binding LearningWindowTitle}
Width: 1020
Height: 720
MinWidth: 820
MinHeight: 620
ResizeMode: CanResize
WindowStyle: default WPF system chrome
ShowInTaskbar: not set
SizeToContent: not set
WindowStartupLocation: not set
Background: ShellBackgroundBrush
```

The window currently keeps default WPF title icon, system title bar, and normal outer system border.

## 4. Current Workflow

### 4.1 Source

Current source controls:

```text
FieldLearningWizard.LearningSourceText
FieldLearningWizard.SourceSection
FieldLearningWizard.SourceNameTextBox
FieldLearningWizard.RawTextBox
```

Current source data:

```text
SourceName
RawText
LearningWindowTitle
LearningSourceSummaryText
CurrentIniHarvestStatusText
```

Source can come from:

```text
current INI
current section / selected source passed by Shell
pasted raw text
```

### 4.2 Parse

Current parse controls:

```text
FieldLearningWizard.UseCurrentIniButton
FieldLearningWizard.ParsePastedTextButton
```

Handlers:

```text
UseCurrentIni
ParsePastedText
```

Behavior:

```text
UseCurrentIni loads current source text through the existing source accessor.
ParsePastedText calls FieldRegistryHarvestPreviewViewModel.ParseAndPreview().
Neither action directly writes registry files.
```

### 4.3 Target / Mode

Current target controls:

```text
FieldLearningWizard.ApplyTargetSection
FieldLearningWizard.TargetScopeComboBox
FieldLearningWizard.ApplyModeComboBox
```

Current target display / status:

```text
TargetFilePreviewText
ApplySummaryText
GeneralizationApplySummaryText
GeneralizationWarningSummaryText
ApplyDisabledReason
ApplyStatusText
```

Behavior:

```text
SelectedTargetScope controls Project / Global target.
SelectedApplyMode controls apply mode.
Generalization summaries explain Unit / Techno generalization behavior.
Changing target/mode must keep existing ViewModel semantics.
```

### 4.4 Review

Current review tabs:

```text
FieldLearningWizard.MainTabs
FieldLearningWizard.CurrentIniDraftsTab
FieldLearningWizard.CurrentIniDraftsGrid
FieldLearningWizard.PreviewDiffTab
FieldLearningWizard.PreviewDiffGrid
FieldLearningWizard.ValidationIssuesTab
FieldLearningWizard.ValidationIssuesGrid
```

Row-level edit control:

```text
FieldLearningWizard.EditAllowedValuesButton
```

Behavior:

```text
CurrentIniDraftsGrid allows draft row review and enable/disable selection.
EditAllowedValues opens AllowedValuesEditorWindow for enum/list/boolean draft rows only.
AllowedValuesEditorWindow returns ResultText to the current draft row and does not write registry directly.
```

### 4.5 Apply Plan

Current apply plan controls:

```text
FieldLearningWizard.BuildApplyPlanButton
FieldLearningWizard.ApplyPlanTab
FieldLearningWizard.ApplyPlanGrid
```

Handler:

```text
BuildApplyPlan
```

Behavior:

```text
BuildApplyPlan calls FieldRegistryHarvestPreviewViewModel.BuildApplyPlan().
It builds a plan from current preview/draft state.
It does not apply registry writes by itself.
```

### 4.6 Apply

Current apply controls:

```text
FieldLearningWizard.ApplyButton
FieldLearningWizard.StatusText
```

Handler:

```text
ApplyCurrentPlan
```

Confirmation and apply flow:

```text
CreateApplyConfirmation()
MessageBox confirmation
ApplyConfirmed() only when user confirms Yes
```

Behavior:

```text
Apply writes registry only after an apply plan exists, CanApply is true, and the user accepts confirmation.
Backup/apply writer semantics remain owned by existing Field Registry apply services and ViewModel flow.
```

## 5. Current UX Problems

Current chrome problems:

```text
Default WPF title icon remains.
Default system title bar remains.
Normal outer system border remains.
The window does not visually match Field Registry Center / Manager / Field Editor chrome direction.
```

Current workflow layout problems:

```text
Source, parse, target, review, build plan, and apply controls are visible at once without a strong staged rhythm.
Header mixes read-only parse actions with write-capable apply action.
Target scope and apply mode are not visually strong enough as a workflow gate.
Draft review, preview diff, validation, and apply plan are all tabbed but not clearly connected as workflow steps.
Raw text and large grids can dominate the window.
Apply boundary relies on button state and MessageBox confirmation, but the visual risk boundary is weak.
Some user-facing text may need Chinese-first cleanup when the implementation phase reaches localization.
```

## 6. Proposed Custom Chrome Rules

Future A15-2E implementation should apply:

```text
WindowStyle=None
ResizeMode=CanResize
WindowChrome to preserve resize behavior
custom lightweight header
custom close button
remove default WPF icon
remove default system title bar
remove normal outer system border
preserve Owner / Show behavior
preserve non-modal behavior
```

Recommended new AutomationIds:

```text
FieldLearningWizard.CustomChrome
FieldLearningWizard.ChromeTitle
FieldLearningWizard.CloseButton
```

Close behavior:

```text
Close button must be equivalent to closing the non-modal window.
It must not parse, build a plan, apply, or mutate registry state.
```

Chrome implementation should mirror the large-window pattern already used by Field Registry Center / Manager and A15-2D editor windows:

```text
WindowChrome
ResizeBorderThickness="6"
GlassFrameThickness="0"
UseAeroCaptionButtons="False"
no AllowsTransparency=True for this large workflow window
```

## 7. Proposed Workflow Layout

The future layout should make the wizard read as a staged IDE workflow:

```text
Source -> Parse -> Draft Review -> Target / Mode -> Apply Plan -> Apply
```

Recommended structure:

```text
Header:
  title, source summary, close button

Step / status strip:
  source loaded, drafts, issues, target, plan, apply readiness

Source section:
  source name
  current INI / parse pasted controls
  raw text box in a bounded scroll region

Target / mode section:
  target scope
  apply mode
  target file preview
  generalization summary and warnings

Review section:
  draft rows
  diff
  validation issues

Apply plan section:
  build plan button
  apply plan grid
  disabled reason / apply status

Apply boundary:
  apply button visually separated from read-only parse/review actions
  confirmation remains required
```

Do not move write-capable behavior into hidden or automatic flows.

## 8. AutomationIds to Preserve

Preserve all existing `FieldLearningWizard.*` AutomationIds:

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

Allowed additions for future A15-2E implementation:

```text
FieldLearningWizard.CustomChrome
FieldLearningWizard.ChromeTitle
FieldLearningWizard.CloseButton
FieldLearningWizard.WorkflowSteps
FieldLearningWizard.SourceStep
FieldLearningWizard.ParseStep
FieldLearningWizard.ReviewStep
FieldLearningWizard.TargetStep
FieldLearningWizard.ApplyPlanStep
FieldLearningWizard.ApplyBoundary
```

Do not rename or remove existing AutomationIds.

## 9. Display-only Properties Needed

A15-2E can likely be implemented without new ViewModel properties if the XAML uses existing bindings:

```text
LearningWindowTitle
LearningSourceSummaryText
CurrentIniHarvestStatusText
TargetFilePreviewText
ApplySummaryText
GeneralizationApplySummaryText
GeneralizationWarningSummaryText
ApplyDisabledReason
ApplyStatusText
SummaryText
StatusText
CanBuildApplyPlan
CanApply
```

If additional ViewModel properties are needed, they must be display-only only:

```text
no IO
no reload
no parser invocation
no apply plan mutation
no registry write
no source text mutation
```

Potential display-only additions, if future implementation requires them:

```text
LearningWorkflowStatusText
LearningDraftSummaryText
LearningApplyReadinessText
LearningWarningSummaryText
```

Do not add display-only properties unless the layout cannot reasonably use existing ones.

## 10. Commands / Handlers to Reuse

Future implementation must reuse existing handlers:

```text
UseCurrentIni
ParsePastedText
BuildApplyPlan
EditAllowedValues
ApplyCurrentPlan
```

Future implementation must preserve these ViewModel calls:

```text
LoadCurrentIniHarvestPreview(...)
ParseAndPreview()
BuildApplyPlan()
CreateApplyConfirmation()
ApplyConfirmed()
```

Future implementation must preserve:

```text
AllowedValuesEditorWindow ShowDialog() row-edit flow
MessageBox confirmation before ApplyConfirmed()
CanBuildApplyPlan binding
CanApply binding
TargetScopeComboBox selected value binding
ApplyModeComboBox selected value binding
```

## 11. Semantic Boundaries

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

Do not modify:

```text
Field Registry loader / writer / apply / rollback / import / learning services
FieldRegistryHarvestPreviewViewModel semantics
parser / normalization / validation
BuiltIn field registry JSON
ShellWindow.xaml
ShellWindow.xaml.cs
Field Editor / Allowed Values Editor A15-2D behavior
solution / project files
legacy files
```

Write boundaries:

```text
Opening the wizard must not write registry files.
Use current INI must not write registry files.
Parse pasted text must not write registry files.
Build apply plan must not write registry files.
Apply must remain gated by CanApply and confirmation.
Allowed Values editing must remain local to draft row text.
```

## 12. Tests to Add / Update

Future implementation should update boundary tests only and avoid pixel-perfect checks.

Recommended files:

```text
RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
```

Planned assertions:

```text
FieldLearningWizardWindow has WindowStyle=None.
ResizeMode=CanResize remains.
WindowChrome exists.
FieldLearningWizard.CloseButton exists.
FieldLearningWizard.CustomChrome and/or ChromeTitle exists.
Existing FieldLearningWizard.* AutomationIds remain.
UseCurrentIni / ParsePastedText / BuildApplyPlan / Apply button handlers remain present.
CreateApplyConfirmation / ApplyConfirmed flow remains present.
ApplyCurrentPlan still uses MessageBox confirmation.
Opening the wizard remains non-modal Show().
Shell reuse / Activate behavior remains.
No registry apply service is invoked from source/parse/build handlers directly.
```

Existing ViewModel tests should continue to cover parse/build/apply semantics.

## 13. Risks

Primary risks:

```text
Accidentally changing shared FieldRegistryHarvestPreviewViewModel behavior affects both Field Learning Wizard and Field Import Preview.
Moving Apply near parse controls can blur read-only vs write-capable actions.
Changing Show() / reuse behavior can create duplicate wizard windows or break source reload.
Changing target/mode bindings can alter apply destination.
Changing Allowed Values row editing can break ResultText handoff.
WindowChrome hit testing can interfere with TextBox/DataGrid input if applied too broadly.
```

Mitigations:

```text
Keep implementation staged.
Prefer chrome/header-only first.
Use AutomationId-based tests.
Preserve all existing handlers and bindings.
Do not touch shared services or parser/apply semantics.
Request screenshot/manual smoke after visual implementation.
```

## 14. Recommended Implementation Plan

Recommended split:

```text
A15-2E-1: Custom chrome + header only
A15-2E-2: Workflow section layout / bounded scroll areas
A15-2E-3: Localization and warning/disabled reason polish
```

### A15-2E-1: Custom chrome + header only

Allowed changes:

```text
FieldLearningWizardWindow.xaml
FieldLearningWizardWindow.xaml.cs only for close button if needed
boundary tests
phase docs
```

Goals:

```text
WindowStyle=None
WindowChrome
custom header
custom close button
preserve resize
preserve existing layout body
```

### A15-2E-2: Workflow section layout / bounded scroll areas

Allowed after separate confirmation:

```text
reorganize visible sections into Source / Target / Review / Apply Plan / Apply boundary
add bounded scroll regions for raw text and large grids if needed
add workflow section AutomationIds
```

Must preserve all bindings and handlers.

### A15-2E-3: Localization and warning/disabled reason polish

Allowed after separate confirmation:

```text
Chinese-first visible labels
warning / disabled reason presentation
status chip text
empty states
```

Any ViewModel addition must be display-only.

## 15. Acceptance Criteria

A15-2E contract is accepted when:

```text
current implementation inventory is documented
current workflow sections are documented
existing AutomationIds are listed and preserved
field learning semantic boundaries are explicit
future implementation split is explicit
tests to add/update are planned
no source/XAML/ViewModel/test files were modified during this contract phase
dotnet test --no-build passes
IdeOnly package passes
legacy is not restored
```

Future implementation acceptance should require:

```text
custom chrome removes default WPF title icon/system title bar
resize behavior remains
close button does not apply changes
existing workflow commands and bindings remain
Apply remains gated by plan and confirmation
Field Registry semantics remain unchanged
Shell layout remains unchanged
tests pass
package passes
manual smoke confirms no unintended writes
```
