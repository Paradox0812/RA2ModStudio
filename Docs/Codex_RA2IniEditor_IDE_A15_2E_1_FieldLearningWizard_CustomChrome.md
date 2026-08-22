# Codex Task: RA2IniEditor.IDE A15-2E-1 Field Learning Wizard Custom Chrome / Header Only

## 0. Current Baseline

A15-2E contract has been completed.

Reported state:

```text
Docs/FieldLearningWizardWorkflowContract.md created.
Workflow covered: Source / Parse / Target-Mode / Review / Apply Plan / Apply.
Tests: 1298 passed.
IdeOnly package: passed, packaged file count 659.
No source / XAML / code-behind / ViewModel / tests / services changed.
Field Registry semantics unchanged.
Legacy table-style editor not restored.
```

A15-2E is split into:

```text
A15-2E-1: Custom chrome + header only.
A15-2E-2: Workflow section layout / bounded scroll areas.
A15-2E-3: Localization and warning / disabled reason polish.
```

This task is **A15-2E-1 only**.

Do not implement A15-2E-2 or A15-2E-3 in this task.

---

## 1. Goal

Apply custom lightweight chrome and header to:

```text
FieldLearningWizardWindow
```

The result should remove default WPF chrome while preserving the existing workflow layout and behavior.

Required result:

```text
1. No default WPF title icon.
2. No default system title bar.
3. No normal outer system border.
4. Custom lightweight header.
5. Custom close button.
6. Preserve move ability.
7. Preserve resize behavior.
8. Preserve non-modal Show() behavior.
9. Preserve existing workflow layout and all commands.
```

---

## 2. Target Surface

Expected paths:

```text
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs
```

Expected ViewModel:

```text
RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs
```

Do not modify the ViewModel in this phase.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if needed for local scoped style reuse:

```text
RA2IniEditor.IDE/Views/IdeSecondaryWindowStyles.xaml
```

---

## 4. Files Forbidden

Do not modify:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs
RA2IniEditor.IDE/Views/FieldEditorWindow.xaml
RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml
RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml
RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml
Field Registry services
parser / normalization / validation
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

Close button must not apply changes.

---

## 6. Custom Chrome Contract

Apply to `FieldLearningWizardWindow`:

```text
WindowStyle=None
ResizeMode=CanResize
WindowChrome to preserve resize
Remove default WPF title icon
Remove system title bar
Remove normal outer system border
Add custom lightweight header
Add custom close button
Preserve Owner / Show behavior
Preserve non-modal behavior
```

Recommended XAML pattern:

```xml
<Window
    ...
    WindowStyle="None"
    ResizeMode="CanResize"
    Background="{DynamicResource ShellBackgroundBrush}">

    <shell:WindowChrome.WindowChrome>
        <shell:WindowChrome
            CaptionHeight="40"
            ResizeBorderThickness="6"
            GlassFrameThickness="0"
            UseAeroCaptionButtons="False" />
    </shell:WindowChrome.WindowChrome>

    <!-- Custom root/header/content -->
</Window>
```

Use the actual namespace/prefix style already used in the project.

Do not use tiny inspector behavior.

Do not use `SizeToContent=WidthAndHeight`.

This is a large workflow window and must remain resizable.

---

## 7. Header Requirements

Custom header content:

```text
Title:
  bound LearningWindowTitle if already available, or equivalent existing title binding.

Subtitle:
  short Chinese-first workflow description, if already safe to add as static UI text.

Right side:
  close button.
```

Required new AutomationIds:

```text
FieldLearningWizard.CustomChrome
FieldLearningWizard.ChromeTitle
FieldLearningWizard.CloseButton
```

Close button behavior:

```text
Close only this wizard window.
Do not apply current plan.
Do not parse/build/apply.
Equivalent to normal window close.
```

Esc behavior may remain unchanged. Do not introduce global keyboard handling unless already local and safe.

---

## 8. Drag / Resize Behavior

Preferred:

```text
Use WindowChrome CaptionHeight and ensure header drag works.
```

If a local drag handler is needed:

```csharp
/// <summary>Moves the custom chrome workflow window from the header drag region.</summary>
private void ChromeHeader_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.ButtonState == MouseButtonState.Pressed)
    {
        DragMove();
    }
}
```

Required namespace if used:

```csharp
using System.Windows.Input;
```

Do not make TextBox, DataGrid, TabControl, or buttons draggable.

Resize must remain available.

---

## 9. Existing AutomationIds Must Be Preserved

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

Do not rename existing AutomationIds.

---

## 10. Implementation Non-goals

Do not in this phase:

```text
1. Re-layout the workflow steps.
2. Add bounded scroll regions.
3. Change tabs / grids / source text area.
4. Localize large sections of wizard text.
5. Reorder buttons.
6. Change apply button placement.
7. Change target scope or apply mode UI behavior.
8. Change Allowed Values Editor.
```

Those belong to:

```text
A15-2E-2: Workflow section layout / bounded scroll areas.
A15-2E-3: Localization and warning / disabled reason polish.
```

---

## 11. Tests

Update boundary tests only. Avoid pixel-perfect tests.

Required tests:

```text
1. FieldLearningWizardWindow has WindowStyle=None.
2. ResizeMode=CanResize remains.
3. WindowChrome exists.
4. FieldLearningWizard.CloseButton exists.
5. FieldLearningWizard.CustomChrome exists.
6. FieldLearningWizard.ChromeTitle exists.
7. Existing FieldLearningWizard.* AutomationIds remain.
8. UseCurrentIni / ParsePastedText / BuildApplyPlan / Apply button handlers remain present.
9. CreateApplyConfirmation / ApplyConfirmed flow remains present.
```

Suggested test files:

```text
RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
```

---

## 12. Validation Commands

Run full validation because XAML/code-behind changes are expected:

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
2. Confirm no default WPF title icon / system title bar / outer system border.
3. Confirm custom header is visible.
4. Confirm custom close button closes the wizard without applying changes.
5. Confirm window can move and resize.
6. Confirm Use Current INI still works.
7. Confirm Parse Pasted Text still works.
8. Confirm Build Apply Plan still works.
9. Confirm Apply still uses existing confirmation flow.
10. Confirm tabs/grids/text inputs still behave as before.
11. Confirm Shell main layout is unchanged.
```

---

## 14. Final Report Format

Report:

```text
1. Phase completed: A15-2E-1.
2. Files changed.
3. Chrome strategy used.
4. Header / close button implementation.
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
