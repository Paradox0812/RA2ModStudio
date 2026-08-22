# Field Editor and Learning Wizard Chrome Contract

## 1. Scope and Baseline

This document records the chrome and layout contract for Field Registry tertiary editor surfaces.

Current implementation baseline:

```text
RA2IniEditor.IDE-only
A15-2B-P3 completed
Field Registry Center / Manager chrome and visual polish completed
Field Editor and Field Learning Wizard still use default WPF system chrome
```

This contract is intentionally split by implementation phase:

```text
A15-2D: Field Editor / Allowed Values Editor
A15-2E: Field Learning Wizard
```

Only A15-2D is approved for the next implementation step. Field Learning Wizard is documented here for context and must remain deferred until A15-2E.

## 2. Inventory

### Field Editor

```text
Window XAML: RA2IniEditor.IDE/Views/FieldEditorWindow.xaml
code-behind: RA2IniEditor.IDE/Views/FieldEditorWindow.xaml.cs
ViewModel: RA2IniEditor.IDE/ViewModels/FieldRegistry/FieldEditorViewModel.cs
DataContext: FieldEditorWindow constructor assigns FieldEditorViewModel
Open path: FieldRegistryCenterWindow.OpenFieldEditor
Open mode: non-modal Show()
Owner: FieldRegistryCenterWindow
Writes state: yes, through existing preview/apply flow only
```

Open path summary:

```text
Create new field:
new FieldEditorWindow(_fieldEditorSaveContext)

Edit existing field:
new FieldEditorWindow(row.Definition, row.SectionKindValue, _fieldEditorSaveContext)

Show:
_fieldEditorWindow.Show()
```

### Allowed Values Editor

```text
Window XAML: RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml
code-behind: RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml.cs
ViewModel: private nested AllowedValuesEditorViewModel in AllowedValuesEditorWindow.xaml.cs
DataContext: AllowedValuesEditorWindow constructor assigns nested view model
Open path: FieldLearningWizardWindow.EditAllowedValues
Open mode: modal ShowDialog()
Owner: FieldLearningWizardWindow
Writes state: no registry write; returns local ResultText only
```

Open path summary:

```text
new AllowedValuesEditorWindow(...)
Owner = this
window.ShowDialog() == true
row.AllowedValuesText = window.ResultText
_viewModel.BuildApplyPlan()
```

### Field Learning Wizard

```text
Window XAML: RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml
code-behind: RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs
ViewModel: RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs
DataContext: FieldLearningWizardWindow constructor assigns FieldRegistryHarvestPreviewViewModel
Open path: ShellWindow.OpenFieldLearningWizardWindow
Open mode: non-modal Show()
Owner: ShellWindow
Writes state: yes, through existing build plan / confirmation / apply flow only
```

Field Learning Wizard is not part of A15-2D implementation and must not be modified during A15-2D.

## 3. Current Window Properties

### Field Editor

```text
AutomationId: FieldEditor.Window
Title: 编辑字段
Width: 900
Height: 780
MinWidth: 760
MinHeight: 660
ResizeMode: CanResize
WindowStyle: default WPF system chrome
ShowInTaskbar: not set
SizeToContent: not set
Background: ShellBackgroundBrush
```

### Allowed Values Editor

```text
AutomationId: AllowedValuesEditor.Window
Title: 编辑可选值
Width: 820
Height: 560
MinWidth: 660
MinHeight: 440
ResizeMode: CanResize
WindowStyle: default WPF system chrome
ShowInTaskbar: not set
SizeToContent: not set
Background: ShellBackgroundBrush
```

### Field Learning Wizard

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
Background: ShellBackgroundBrush
```

## 4. Current UX Problems

Common issue:

```text
Field Editor, Allowed Values Editor, and Field Learning Wizard still show the native WPF title bar, default icon area, and system border.
```

Field Editor-specific issues:

```text
The internal editor already uses IDE secondary styles, but the outer frame still looks like a default WPF dialog.
The window is a structured editor with preview/apply sections, so chrome work must not disrupt the command placement.
The existing body scroll region and bottom save actions must remain stable.
```

Allowed Values Editor-specific issues:

```text
The window is a small local draft editor but still uses default WPF chrome.
It is opened modally from the learning workflow and must preserve the ShowDialog / ResultText contract.
Its internal grid and command rows are already focused; the main polish target is chrome consistency.
```

Field Learning Wizard-specific issues:

```text
The workflow surface also still uses default WPF chrome.
It has a different workflow shape from Field Editor and must be handled separately in A15-2E.
```

## 5. Proposed Custom Chrome Rules

Shared future chrome direction:

```text
WindowStyle=None
ResizeMode=CanResize preserved
WindowChrome used to preserve resize behavior
default WPF icon removed
default system title bar removed
normal outer system border removed
custom lightweight header added
custom close button added
header supports moving the window
existing Owner / Show / ShowDialog behavior preserved
```

Do not use inspector popup sizing. These are editor/workflow windows and must remain resizable management surfaces.

Recommended implementation details:

```text
Use a root Border/Grid consistent with existing Field Registry custom chrome direction.
Keep the content layout inside the current root workflow order.
Keep close behavior local to each window.
Do not move save/apply commands into the custom header.
Do not change any ViewModel mutation, IO, preview, apply, or confirmation behavior.
```

## 6. Editor Window Layout Direction

A15-2D Field Editor should remain a structured editor dialog.

Preserve these sections:

```text
FieldEditor.HeaderArea
FieldEditor.BasicSection
FieldEditor.DescriptionSection
FieldEditor.SavePreviewSection
FieldEditor.ApplyResultPanel
FieldEditor.StatusText
bottom Project / Global save and Cancel command area
```

Preserve these workflow rules:

```text
ProjectPreviewButton and GlobalPreviewButton only build previews.
ProjectSaveButton and GlobalSaveButton call existing apply paths.
CanSave remains based on SavePreview?.CanSave.
PersistedPreviewTextBox remains read-only and one-way.
ApplyResultPanel remains a post-apply result display.
FieldRegistrySaveApplied remains the event used to notify the Center.
```

Allowed A15-2D visual changes:

```text
custom chrome wrapper/header
close button
title/header alignment
minor spacing required by chrome wrapper
Chinese-first title/header text cleanup where it does not affect bindings or commands
```

Forbidden A15-2D Field Editor changes:

```text
do not modify FieldEditorViewModel save/apply logic
do not change FieldEditorSavePreviewBuilder or FieldEditorSaveApplyService
do not change Project / Global target semantics
do not change non-modal Show() behavior
do not change FieldRegistryCenter open/reload semantics
do not change validation semantics
```

## 7. Allowed Values Editor Layout Direction

A15-2D Allowed Values Editor should remain a local draft editor.

Preserve these elements:

```text
AllowedValuesEditor.Grid
AllowedValuesEditor.AddButton
AllowedValuesEditor.RemoveButton
AllowedValuesEditor.DedupeButton
AllowedValuesEditor.SortButton
AllowedValuesEditor.AppendBuiltInButton
AllowedValuesEditor.RestoreScannedButton
AllowedValuesEditor.OkButton
AllowedValuesEditor.CancelButton
```

Preserve these workflow rules:

```text
Accept sets ResultText from ToAllowedValuesText() and DialogResult=true.
Cancel sets DialogResult=false.
ShowDialog result controls whether the caller writes row.AllowedValuesText.
No registry writer or save service is introduced here.
No dirty document or project save coupling is introduced here.
```

Allowed A15-2D visual changes:

```text
custom chrome wrapper/header
close button with cancel-equivalent behavior
title/header alignment
minor spacing required by chrome wrapper
Chinese-first title/header text cleanup where it does not affect parsing or ResultText
```

Forbidden A15-2D Allowed Values changes:

```text
do not change row parsing
do not change dedupe / sort behavior
do not change built-in append behavior
do not change ResultText formatting
do not change modal ShowDialog contract
do not modify FieldLearningWizardWindow during A15-2D
```

## 8. AutomationIds to Preserve

### Field Editor

Preserve all existing FieldEditor AutomationIds:

```text
FieldEditor.Window
FieldEditor.HeaderArea
FieldEditor.BasicSection
FieldEditor.KeyTextBox
FieldEditor.SectionKindComboBox
FieldEditor.EditorKindComboBox
FieldEditor.ValueKindComboBox
FieldEditor.BooleanStyleComboBox
FieldEditor.SeparatorTextBox
FieldEditor.EnumNameTextBox
FieldEditor.DescriptionSection
FieldEditor.DisplayNameTextBox
FieldEditor.AliasesTextBox
FieldEditor.AllowedValuesTextBox
FieldEditor.DescriptionTextBox
FieldEditor.SavePreviewSection
FieldEditor.CopyPersistedPreviewButton
FieldEditor.ProjectPreviewButton
FieldEditor.GlobalPreviewButton
FieldEditor.PreviewSummaryText
FieldEditor.PreviewIssuesGrid
FieldEditor.PersistedPreviewTextBox
FieldEditor.ApplyResultPanel
FieldEditor.TargetPathTextBox
FieldEditor.ManifestPathTextBox
FieldEditor.CopyTargetPathButton
FieldEditor.OpenTargetFolderButton
FieldEditor.CopyManifestPathButton
FieldEditor.OpenManifestFolderButton
FieldEditor.StatusText
FieldEditor.ProjectSaveButton
FieldEditor.GlobalSaveButton
FieldEditor.CancelButton
```

Allowed additions for A15-2D:

```text
FieldEditor.CustomChrome
FieldEditor.ChromeTitle
FieldEditor.CloseButton
```

### Allowed Values Editor

Preserve all existing AllowedValuesEditor AutomationIds:

```text
AllowedValuesEditor.Window
AllowedValuesEditor.Grid
AllowedValuesEditor.AddButton
AllowedValuesEditor.RemoveButton
AllowedValuesEditor.DedupeButton
AllowedValuesEditor.SortButton
AllowedValuesEditor.AppendBuiltInButton
AllowedValuesEditor.RestoreScannedButton
AllowedValuesEditor.OkButton
AllowedValuesEditor.CancelButton
```

Allowed additions for A15-2D:

```text
AllowedValuesEditor.CustomChrome
AllowedValuesEditor.ChromeTitle
AllowedValuesEditor.CloseButton
```

### Field Learning Wizard

Preserve all existing FieldLearningWizard AutomationIds in future A15-2E. Do not modify them during A15-2D.

## 9. Tests to Add / Update

A15-2D should update boundary-style tests only.

Recommended test files:

```text
RA2IniEditor.Tests/IDE/Ra2FieldEditorWindowBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
```

Field Editor test assertions:

```text
FieldEditorWindow has WindowStyle="None".
FieldEditorWindow preserves ResizeMode="CanResize".
FieldEditorWindow contains WindowChrome.
FieldEditor.CloseButton exists.
FieldEditor.CustomChrome or FieldEditor.ChromeTitle exists if added.
All existing FieldEditor.* AutomationIds still exist.
FieldRegistryCenter still opens FieldEditorWindow with Show(), not ShowDialog().
BuildSavePreview, ApplySave, and FieldRegistrySaveApplied remain present.
PersistedPreviewTextBox remains read-only and one-way.
ProjectSaveButton and GlobalSaveButton still bind to CanSave.
```

Allowed Values Editor test assertions:

```text
AllowedValuesEditorWindow has WindowStyle="None".
AllowedValuesEditorWindow preserves ResizeMode="CanResize".
AllowedValuesEditorWindow contains WindowChrome.
AllowedValuesEditor.CloseButton exists.
AllowedValuesEditor.CustomChrome or AllowedValuesEditor.ChromeTitle exists if added.
All existing AllowedValuesEditor.* AutomationIds still exist.
Accept still sets DialogResult=true.
Cancel still sets DialogResult=false.
ResultText remains produced by ToAllowedValuesText().
No FieldRegistryApplyWriter, ProjectSaveService, Dirty, or registry apply coupling is introduced.
```

Do not add pixel-perfect tests.

## 10. Semantic Boundaries

Do not change:

```text
field editor validation
save preview behavior
project/global apply behavior
FieldRegistrySaveApplied reload request
Allowed Values parsing
Allowed Values ResultText formatting
Allowed Values modal return contract
field learning parse behavior
build apply plan behavior
apply behavior
target scope behavior
diagnostics/completion/hover behavior
```

Do not modify:

```text
FieldEditorSavePreviewBuilder
FieldEditorSaveApplyService
Field Registry loader / writer / apply / rollback / import / learning services
parser / normalization / validation
BuiltIn field registry JSON
ShellWindow.xaml
ShellWindow.xaml.cs
FieldLearningWizardWindow.xaml during A15-2D
FieldLearningWizardWindow.xaml.cs during A15-2D
```

## 11. Risks

```text
Changing FieldEditorWindow from non-modal to modal would block Center workflow and is forbidden.
Changing AllowedValuesEditorWindow from modal to non-modal would break ResultText handoff and is forbidden.
A close button in AllowedValuesEditor must preserve cancel-equivalent behavior.
A custom header drag handler must not interfere with DataGrid/TextBox interactions.
WindowChrome must preserve resize because both windows contain editable grids or large text regions.
Source text currently contains mojibake in some checked-out views; localization cleanup should be done only where tests and user approval permit it.
```

## 12. Recommended Implementation Split

Recommended sequence:

```text
1. A15-2D: implement FieldEditorWindow custom chrome.
2. A15-2D: implement AllowedValuesEditorWindow custom chrome.
3. A15-2D: update boundary tests for chrome and preserved AutomationIds.
4. A15-2D: run restore/build/test/package.
5. A15-2E: separately contract and implement FieldLearningWizard chrome/layout.
```

A15-2D must not include Field Learning Wizard implementation.

## 13. A15-2D Validation Commands

After A15-2D implementation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

For this contract-only document update:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```
