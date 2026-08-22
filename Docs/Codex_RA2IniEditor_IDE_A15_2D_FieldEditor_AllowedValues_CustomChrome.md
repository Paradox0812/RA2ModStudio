# Codex Task: RA2IniEditor.IDE A15-2D Field Editor / Allowed Values Custom Chrome Limited Implementation

## 0. Current Baseline

The Field Registry localization test synchronization has been completed.

Current expected baseline:

```text
RA2IniEditor.IDE-only
A15-2B-P/P2 Field Registry Center / Manager visual polish and custom chrome completed
A15-2B-P3 Manager scroll/localization completed or test-synced
Field Registry localization tests synchronized
tests should be green before starting this task
legacy table-style editor not restored
Shell main layout unchanged
```

This task starts:

```text
A15-2D: Field Editor / Allowed Values Editor chrome and layout polish
```

This is a **limited implementation** task.

It must not touch Field Learning Wizard. Field Learning Wizard is deferred to A15-2E.

---

## 1. Goal

Apply custom lightweight chrome and limited visual polish to:

```text
Field Editor / New Field Editor
Allowed Values Editor
```

The goal is to remove the default WPF window chrome and make these editor dialogs consistent with the current RA2IniEditor.IDE secondary-window style, while preserving all editing/save/apply semantics.

Required result:

```text
1. No default WPF title icon.
2. No default system title bar.
3. No normal outer system border.
4. Custom lightweight header.
5. Custom close button.
6. Preserve move ability.
7. Preserve resize behavior where it currently exists.
8. Preserve existing editor body layout and all write/apply behavior.
```

---

## 2. Target Surfaces

### 2.1 Field Editor

Expected paths:

```text
RA2IniEditor.IDE/Views/FieldEditorWindow.xaml
RA2IniEditor.IDE/Views/FieldEditorWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/FieldRegistry/FieldEditorViewModel.cs
```

Use actual discovered paths if different.

Current role:

```text
Structured field definition editor.
Non-modal Show().
Owner: FieldRegistryCenterWindow.
Writes state only through existing preview/apply flow.
```

### 2.2 Allowed Values Editor

Expected paths:

```text
RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml
RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml.cs
```

Use actual discovered paths if different.

Current role:

```text
Modal allowed values editor.
ShowDialog().
ResultText + DialogResult contract.
Currently opened from FieldLearningWizard.
```

Even though it is opened from the wizard today, this task may apply custom chrome to the Allowed Values Editor itself, but must not modify the Wizard open path.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/FieldEditorWindow.xaml
RA2IniEditor.IDE/Views/FieldEditorWindow.xaml.cs
RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml
RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2FieldEditorWindowBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if strictly necessary and display-only:

```text
local scoped style resource dictionary used only by these windows
```

Do not modify `FieldEditorViewModel.cs` unless the user explicitly approves display-only text changes. This task should not need it.

---

## 4. Files Forbidden

Do not modify:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml
RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs
RA2IniEditor.IDE/ViewModels/FieldRegistry/FieldEditorViewModel.cs, unless separately approved
Field Registry loader / writer / apply / rollback / import / learning services
FieldEditorSavePreviewBuilder
FieldEditorSaveApplyService
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

Do not change Field Editor behavior:

```text
BuildProjectPreview -> BuildPreview(FieldEditorSaveTarget.Project)
BuildGlobalPreview -> BuildPreview(FieldEditorSaveTarget.Global)
ApplyProjectSave -> ApplySave(FieldEditorSaveTarget.Project)
ApplyGlobalSave -> ApplySave(FieldEditorSaveTarget.Global)
FieldEditorViewModel.BuildSavePreview(...)
FieldEditorViewModel.ApplySave(...)
FieldRegistrySaveApplied event
FieldRegistryCenter reload request after successful save
CanSave gating
preview-before-save flow
Project / Global target behavior
backup manifest behavior
last apply path display
copy/open folder behavior
non-modal Show() behavior
```

Do not change Allowed Values Editor behavior:

```text
AddRow
RemoveSelectedRow
DedupeRows
SortRows
AppendBuiltInValues
RestoreScannedValues
Accept -> ResultText + DialogResult=true
Cancel -> DialogResult=false
ToAllowedValuesText format
ShowDialog result contract
row parsing / dedupe / sort semantics
BuiltIn value append semantics
local-only draft behavior
```

---

## 6. Custom Chrome Contract

Apply to both windows:

```text
WindowStyle=None
ResizeMode=CanResize
Use WindowChrome to preserve resize
Remove default WPF title icon
Remove system title bar
Remove normal outer system border
Add custom lightweight header
Add custom close button
Preserve Owner / Show / ShowDialog behavior
Preserve content body layout
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

    <!-- Custom root card/header/content -->
</Window>
```

Use the actual namespace/prefix style used elsewhere in the project. If `WindowChrome` is already used by Field Registry Center/Manager, mirror that approach.

Do not use `AllowsTransparency=True` unless the existing Field Registry custom chrome uses it safely for large windows. These are large editor windows and should preserve resize performance.

---

## 7. Custom Header Requirements

For Field Editor:

```text
Title: 编辑字段
Subtitle: Field definition editor / project-global save preview if already shown, preferably Chinese-first.
Right side: close button.
```

For Allowed Values Editor:

```text
Title: 编辑可选值
Subtitle: one value per row / built-in scanned values if already explained, preferably Chinese-first.
Right side: close button.
```

Required close button AutomationIds:

```text
FieldEditor.CloseButton
AllowedValuesEditor.CloseButton
```

Required optional chrome AutomationIds:

```text
FieldEditor.CustomChrome
FieldEditor.ChromeTitle
AllowedValuesEditor.CustomChrome
AllowedValuesEditor.ChromeTitle
```

---

## 8. Close Behavior

### 8.1 Field Editor

Close button should be equivalent to closing the non-modal window.

Allowed:

```csharp
Close();
```

Do not trigger save/apply.

### 8.2 Allowed Values Editor

Close button must preserve cancel semantics.

Preferred:

```csharp
DialogResult = false;
Close();
```

If assigning `DialogResult=false` can throw when not shown as dialog, use the same existing Cancel handler or a safe equivalent.

Do not set `DialogResult=true`.

---

## 9. Drag Behavior

A custom chrome needs a drag region.

Preferred:

```text
Use WindowChrome CaptionHeight and mark interactive controls correctly.
```

If a local handler is needed:

```csharp
/// <summary>Moves the custom chrome window from the header drag area.</summary>
private void ChromeHeader_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.ButtonState == MouseButtonState.Pressed)
    {
        DragMove();
    }
}
```

Ensure:

```csharp
using System.Windows.Input;
```

Do not make TextBox/DataGrid areas draggable.

---

## 10. Existing AutomationIds Must Be Preserved

### 10.1 Field Editor

Preserve all existing `FieldEditor.*`, especially:

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

### 10.2 Allowed Values Editor

Preserve:

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

---

## 11. Visual / Text Cleanup

Allowed:

```text
1. Chinese-first labels inside the two target windows.
2. Local title/subtitle cleanup.
3. Lighter custom header.
4. Compact action button row if existing layout allows without moving behavior.
```

Do not:

```text
1. Redesign internal workflow.
2. Move save/apply commands into title bar.
3. Change input layout semantics.
4. Translate field keys, section names, paths, source file names, INI content, or enum values.
```

---

## 12. Tests

Update boundary tests only. Avoid pixel-perfect tests.

Required tests:

```text
FieldEditorWindow has WindowStyle=None.
FieldEditorWindow preserves ResizeMode=CanResize.
FieldEditorWindow has WindowChrome.
FieldEditor.CloseButton exists.
Existing FieldEditor.* AutomationIds remain.
FieldRegistryCenter still uses Show(), not ShowDialog(), for Field Editor.
BuildSavePreview / ApplySave / FieldRegistrySaveApplied remain present.

AllowedValuesEditorWindow has WindowStyle=None.
AllowedValuesEditorWindow preserves ResizeMode=CanResize.
AllowedValuesEditorWindow has WindowChrome.
AllowedValuesEditor.CloseButton exists.
Existing AllowedValuesEditor.* AutomationIds remain.
Accept still sets DialogResult=true.
Cancel still sets DialogResult=false.
ResultText remains produced by ToAllowedValuesText().
```

Suggested files:

```text
RA2IniEditor.Tests/IDE/Ra2FieldEditorWindowBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
```

---

## 13. Validation Commands

Run full validation because XAML/code-behind changes are expected:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 14. Manual Smoke Checklist

After implementation:

```text
1. Open Field Registry Center.
2. Click New Field and confirm Field Editor opens.
3. Confirm Field Editor has no default WPF title icon / system title bar / outer system border.
4. Confirm custom close button closes the editor without saving.
5. Confirm window can move and resize.
6. Confirm Project/Global preview and save buttons still behave as before.
7. Open Field Learning Wizard only if needed to reach Allowed Values Editor.
8. Open Allowed Values Editor.
9. Confirm it has no default WPF chrome.
10. Confirm OK still returns accepted values.
11. Confirm Cancel/close returns cancel semantics.
12. Confirm Field Learning Wizard itself was not redesigned.
```

---

## 15. Final Report Format

Report:

```text
1. Phase completed: A15-2D.
2. Files changed.
3. Chrome strategy used.
4. Field Editor changes.
5. Allowed Values Editor changes.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation Field Editor save/apply semantics unchanged.
11. Confirmation Allowed Values DialogResult/ResultText semantics unchanged.
12. Confirmation Field Learning Wizard not redesigned.
13. Confirmation Shell unchanged.
14. Confirmation legacy not restored.
15. Manual smoke steps or result.
16. Remaining risks.
17. Recommended next phase.
```
