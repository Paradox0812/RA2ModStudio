# Field Registry Tertiary Surfaces UI Contract

## 1. Scope and Baseline

Baseline: RA2IniEditor.IDE-only after A15-2B-P / A15-2B-P2 Field Registry Center / Manager visual polish and custom chrome.

This is the A15-2T read-only audit and UI contract for Field Registry tertiary surfaces. It does not implement UI changes. It documents current windows/dialogs opened from Field Registry Center / Manager and defines staged future redesign boundaries.

Current accepted constraints:

- Active solution: `RA2IniEditor.IDE.sln`
- Active package profile: `IdeOnly`
- Legacy table-style editor is intentionally absent and must not be restored.
- Main Shell layout remains frozen unless a future Shell-specific task is approved.
- Field Registry semantics must remain unchanged: load order, Project > Global > BuiltIn priority, import diff semantics, apply writer behavior, rollback behavior, backup manifest behavior, field learning behavior, field editor validation behavior, allowed values behavior, remote preset behavior, diagnostics, completion, hover, quick peek, save preflight, and dirty/save behavior.

This contract covers tertiary Field Registry surfaces only. It excludes the already-polished Field Registry Center and Field Registry Manager primary management windows, and it excludes non-Field-Registry field surfaces such as Add Property, Quick Peek, and Field Annotation windows.

## 2. Inventory Summary

| Surface | Files | Type | Entry Point | Writes State | Current Chrome | Current UX Problem | Future Phase | Risk |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Field Import Preview | `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml`, `.xaml.cs`; `RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs` | Workflow Dialog | Manager `HarvestPreviewRequested` -> `ShellWindow.FieldRegistryManagerWindow_OnHarvestPreviewRequested` | Can fetch/cache remote text, manage presets/history, build/apply registry plans after MessageBox confirmation | Default WPF chrome | Dense mixed workflow; remote source, parse, diff, plan, apply, history, presets, and import/export are present in one large surface | A15-2C | High |
| Field Learning Wizard | `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml`, `.xaml.cs`; uses `FieldRegistryHarvestPreviewViewModel` | Workflow Dialog | Center learning, Manager relearn current INI, Shell current INI/current section commands | Can build/apply registry plans after MessageBox confirmation; opens Allowed Values Editor for row edits | Default WPF chrome | Workflow still feels like a large form/table; source, draft review, target, validation, and apply plan need clearer step rhythm | A15-2E | High |
| Field Editor / New Field Editor | `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml`, `.xaml.cs`; `RA2IniEditor.IDE/ViewModels/FieldRegistry/FieldEditorViewModel.cs` | Editor Dialog | Center new/edit/double-click -> `FieldRegistryCenterWindow.OpenFieldEditor` | Can save to Project or Global field registry through preview/apply service and backup manifest | Default WPF chrome | Long form with preview/result panels; save targets are separate buttons and write boundary is visually close to editing controls | A15-2D | High |
| Allowed Values Editor | `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml`, `.xaml.cs` | Editor Dialog | Field Learning Wizard row button `EditAllowedValues` | Does not write registry directly; returns edited allowed-values text to the parent preview row | Default WPF chrome | Dense editable DataGrid can resemble legacy table editing unless framed as a scoped value-list editor | A15-2D | Medium |
| Remote Preset Editor | `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml`, `.xaml.cs`; `RemoteSourcePresetEditorViewModel` | Preset / Remote Dialog | Field Import Preview add/edit preset | Returns a preset edit model to parent; parent persists preset changes | Default WPF chrome | Compact but still native form-like; remote/local persistence boundary is not visually strong | A15-2G | Medium |
| Apply / Rollback / Destructive Confirmations | MessageBox calls in `FieldRegistryManagerWindow.xaml.cs`, `FieldRegistryHarvestPreviewWindow.xaml.cs`, `FieldLearningWizardWindow.xaml.cs` | Confirmation Dialog | Cleanup apply, rollback selected, import apply, learning apply, clear remote history, remove remote preset | Writes state only after explicit confirmation for apply/rollback/cleanup; clear/remove affects local remote metadata | Native MessageBox | Risk details, target file, backup manifest, operation count, and local-only remote effects are compressed into plain MessageBox text | A15-2F | High |

Named surfaces not found in the current IDE-only package:

- `FieldImportPreviewWindow`: not found by that exact name. The actual import preview window is `FieldRegistryHarvestPreviewWindow`.
- Standalone `Field Apply` window: not found. Apply currently uses buttons plus MessageBox confirmation in import preview / learning / field editor flows.
- Standalone `Rollback` window: not found. Rollback currently uses Manager grid plus MessageBox confirmation.
- Standalone cleanup preview child window: not found. Cleanup preview is inside Field Registry Manager.
- Remote import/preset selection window: not found. Preset import/export currently uses system `OpenFileDialog` / `SaveFileDialog`.

## 3. Detailed Surface Notes

### 3.1 Field Import Preview

- XAML: `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml`
- Code-behind: `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml.cs`
- ViewModel: `FieldRegistryHarvestPreviewViewModel`
- Entry point: `FieldRegistryManagerWindow.HarvestPreviewRequested` -> `ShellWindow.FieldRegistryManagerWindow_OnHarvestPreviewRequested` -> `new FieldRegistryHarvestPreviewWindow(...)`
- Owner assignment: Shell sets `Owner = this`.
- Modal/non-modal: non-modal, `Show()`.
- Window properties:
  - `WindowStyle`: default WPF chrome, not explicitly set.
  - `ShowInTaskbar`: default, not explicitly set.
  - `ResizeMode`: `CanResize`
  - `SizeToContent`: default/manual, not explicitly set.
  - `WindowStartupLocation`: default/manual, not explicitly set.
  - `Width`: 1040
  - `Height`: 720
  - `MinWidth`: 820
  - `MinHeight`: 650
- Existing AutomationIds:
  - `FieldImportPreview.Window`
  - `FieldImportPreview.InsertSampleButton`
  - `FieldImportPreview.ParsePreviewButton`
  - `FieldImportPreview.UseCurrentIniButton`
  - `FieldImportPreview.ClearButton`
  - `FieldImportPreview.SourceNameTextBox`
  - `FieldImportPreview.FetchUrlTextBox`
  - `FieldImportPreview.FetchRawTextButton`
  - `FieldImportPreview.CancelFetchButton`
  - `FieldImportPreview.RawTextBox`
  - `FieldImportPreview.FetchStatusText`
  - `FieldImportPreview.CurrentIniHarvestStatusText`
  - `FieldImportPreview.TargetScopeComboBox`
  - `FieldImportPreview.ApplyModeComboBox`
  - `FieldImportPreview.BuildApplyPlanButton`
  - `FieldImportPreview.ApplyButton`
  - `FieldImportPreview.TargetFilePreviewText`
  - `FieldImportPreview.ApplySummaryText`
  - `FieldImportPreview.MainFlowTabs`
  - `FieldImportPreview.PreviewDiffTab`
  - `FieldImportPreview.PreviewDiffGrid`
  - `FieldImportPreview.ApplyPlanTab`
  - `FieldImportPreview.ApplyDisabledReasonText`
  - `FieldImportPreview.ApplyStatusText`
  - `FieldImportPreview.GeneralizationSummaryText`
  - `FieldImportPreview.GeneralizationApplySummaryText`
  - `FieldImportPreview.GeneralizationWarningSummaryText`
  - `FieldImportPreview.LastApplySummaryText`
  - `FieldImportPreview.LastApplyTargetPathText`
  - `FieldImportPreview.LastApplyManifestPathText`
  - `FieldImportPreview.ApplyPlanGrid`
  - `FieldImportPreview.IssuesWarningsTab`
  - `FieldImportPreview.ValidationIssuesGrid`
  - `FieldImportPreview.ParseWarningsGrid`
  - `FieldImportPreview.AdvancedDetailsExpander`
  - `FieldImportPreview.AdvancedDetailsTabs`
  - `FieldImportPreview.RemoteHistoryTab`
  - `FieldImportPreview.RefreshRemoteHistoryButton`
  - `FieldImportPreview.UseCachedTextButton`
  - `FieldImportPreview.RefetchSelectedButton`
  - `FieldImportPreview.ClearRemoteHistoryButton`
  - `FieldImportPreview.RemoteHistoryStatusText`
  - `FieldImportPreview.RemoteHistoryGrid`
  - `FieldImportPreview.RemotePresetsTab`
  - `FieldImportPreview.RefreshRemotePresetsButton`
  - `FieldImportPreview.UsePresetUrlButton`
  - `FieldImportPreview.FetchSelectedPresetButton`
  - `FieldImportPreview.AddPresetButton`
  - `FieldImportPreview.EditPresetButton`
  - `FieldImportPreview.RemovePresetButton`
  - `FieldImportPreview.ImportPresetsButton`
  - `FieldImportPreview.ExportPresetsButton`
  - `FieldImportPreview.RemotePresetStatusText`
  - `FieldImportPreview.RemotePresetsGrid`
  - `FieldImportPreview.CurrentIniDraftsTab`
  - `FieldImportPreview.CurrentIniDraftsGrid`
  - `FieldImportPreview.GeneralizationTab`
  - `FieldImportPreview.GeneralizationGrid`
  - `FieldImportPreview.ParsedFieldsTab`
  - `FieldImportPreview.ParsedFieldsGrid`
  - `FieldImportPreview.FieldDraftsTab`
  - `FieldImportPreview.FieldDraftsGrid`
  - `FieldImportPreview.RawTextPreviewTab`
  - `FieldImportPreview.StatusText`
- Existing tests:
  - `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewBoundaryTests.cs`
  - `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewWindowApplyGuardrailTests.cs`
  - `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewViewModelTests.cs`
  - `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewViewModelApplyTests.cs`
  - `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewViewModelCurrentIniTests.cs`
  - `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewViewModelPresetTests.cs`
  - `WpfAutomationHarnessBoundaryTests` also checks key import preview AutomationIds.
- Reads/writes registry state:
  - Reads current provenance and active field registry state for diff/preview.
  - Can fetch/cache remote text and manage local remote history/presets.
  - Writes Project/Global active field registry only after building a plan and accepting MessageBox confirmation.
- Current UX problems:
  - Default WPF title bar remains.
  - Source input, URL fetch, parse, target, apply, tabs, remote history, remote presets, current INI drafts, generalization, parsed fields, raw preview, and status compete for attention.
  - Remote operations and registry apply operations live in the same large surface.
  - Some confirmation and file dialog text remains English.
  - Apply button is present in the same row as build/target controls, so future layout should make the write boundary stronger.
- Proposed category: Workflow Dialog.
- Future contract:
  - Use custom lightweight tool-window chrome with stable close AutomationId.
  - Convert to step-based layout: Source -> Parse/Preview -> Target -> Review -> Confirm Apply.
  - Keep remote history/presets in an advanced or separate remote-source band.
  - Show target scope, apply mode, target file, operation counts, warnings, and disabled reasons near the apply boundary.
  - Preserve existing parse/diff/apply semantics and no auto-apply behavior.
- Non-goals:
  - Do not change parser, normalizer, diff, apply plan builder, writer, remote cache, preset storage, or provenance lookup.
  - Do not add implicit network fetch or credentials.
  - Do not merge Learning Wizard responsibilities into import preview.
- Tests needed:
  - Boundary tests for new chrome and close button.
  - Boundary tests for step IDs, target/apply summary IDs, advanced remote IDs, and no automatic apply.
  - Existing ViewModel tests remain primary for semantics.

### 3.2 Field Learning Wizard

- XAML: `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml`
- Code-behind: `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs`
- ViewModel: `FieldRegistryHarvestPreviewViewModel`
- Entry point:
  - Field Registry Center `FieldLearningRequested` -> `ShellWindow.OpenFieldLearningWizardWindow`
  - Field Registry Manager `RelearnCurrentIniRequested` -> `ShellWindow.OpenFieldLearningWizardWindow`
  - Shell current INI/current section commands also use the same window.
- Owner assignment: Shell sets `Owner = this`.
- Modal/non-modal: non-modal, `Show()`.
- Window properties:
  - `WindowStyle`: default WPF chrome, not explicitly set.
  - `ShowInTaskbar`: default, not explicitly set.
  - `ResizeMode`: `CanResize`
  - `SizeToContent`: default/manual, not explicitly set.
  - `WindowStartupLocation`: default/manual, not explicitly set.
  - `Width`: 1020
  - `Height`: 720
  - `MinWidth`: 820
  - `MinHeight`: 620
- Existing AutomationIds:
  - `FieldLearningWizard.Window`
  - `FieldLearningWizard.HeaderArea`
  - `FieldLearningWizard.LearningSourceText`
  - `FieldLearningWizard.UseCurrentIniButton`
  - `FieldLearningWizard.ParsePastedTextButton`
  - `FieldLearningWizard.BuildApplyPlanButton`
  - `FieldLearningWizard.ApplyButton`
  - `FieldLearningWizard.SourceSection`
  - `FieldLearningWizard.SourceNameTextBox`
  - `FieldLearningWizard.RawTextBox`
  - `FieldLearningWizard.ApplyTargetSection`
  - `FieldLearningWizard.TargetScopeComboBox`
  - `FieldLearningWizard.ApplyModeComboBox`
  - `FieldLearningWizard.GeneralizationApplySummaryText`
  - `FieldLearningWizard.GeneralizationWarningSummaryText`
  - `FieldLearningWizard.MainTabs`
  - `FieldLearningWizard.CurrentIniDraftsTab`
  - `FieldLearningWizard.CurrentIniDraftsGrid`
  - `FieldLearningWizard.EditAllowedValuesButton`
  - `FieldLearningWizard.PreviewDiffTab`
  - `FieldLearningWizard.PreviewDiffGrid`
  - `FieldLearningWizard.ValidationIssuesTab`
  - `FieldLearningWizard.ValidationIssuesGrid`
  - `FieldLearningWizard.ApplyPlanTab`
  - `FieldLearningWizard.ApplyPlanGrid`
  - `FieldLearningWizard.StatusText`
- Existing tests:
  - `RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs`
  - `WpfAutomationHarnessBoundaryTests` also checks key wizard layout/IDs.
- Reads/writes registry state:
  - Reads current INI or pasted text and builds field drafts.
  - Opens Allowed Values Editor for row-level draft edits.
  - Writes Project/Global field registry only after building an apply plan and accepting MessageBox confirmation.
- Current UX problems:
  - Default WPF title bar remains.
  - Workflow actions are in one header row, including both parse/build and apply.
  - Source area, target area, validation, draft grid, diff, and plan need stronger staged progression.
  - Editable allowed-values cell plus row edit button can feel table-heavy.
- Proposed category: Workflow Dialog.
- Future contract:
  - Use custom lightweight tool-window chrome with stable close AutomationId.
  - Create a clear workflow rhythm: Source -> Draft Review -> Target -> Plan -> Confirm Apply.
  - Keep row-level allowed-values editing available, but make it subordinate to draft review.
  - Separate write/apply action visually from read-only parse and preview actions.
  - Preserve existing current INI, current section, pasted text, build plan, and apply behavior.
- Non-goals:
  - Do not change harvest parser/normalizer/generalization behavior.
  - Do not change apply plan, target scope, backup, or reload semantics.
  - Do not make Allowed Values Editor write directly to registry.
- Tests needed:
  - Boundary tests for new chrome, source step, draft review step, target step, apply boundary, and close button.
  - ViewModel tests for disabled apply reason, source summary, and current INI source behavior remain primary.

### 3.3 Field Editor / New Field Editor

- XAML: `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml`
- Code-behind: `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml.cs`
- ViewModel: `RA2IniEditor.IDE/ViewModels/FieldRegistry/FieldEditorViewModel.cs`
- Entry point: Field Registry Center `CreateNewField`, `EditSelectedField`, or fields grid double-click -> `FieldRegistryCenterWindow.OpenFieldEditor`
- Owner assignment: Field Registry Center sets `_fieldEditorWindow.Owner = this`.
- Modal/non-modal: non-modal, `Show()`.
- Window properties:
  - `WindowStyle`: default WPF chrome, not explicitly set.
  - `ShowInTaskbar`: default, not explicitly set.
  - `ResizeMode`: `CanResize`
  - `SizeToContent`: default/manual, not explicitly set.
  - `WindowStartupLocation`: default/manual, not explicitly set.
  - `Width`: 900
  - `Height`: 780
  - `MinWidth`: 760
  - `MinHeight`: 660
- Existing AutomationIds:
  - `FieldEditor.Window`
  - `FieldEditor.HeaderArea`
  - `FieldEditor.BasicSection`
  - `FieldEditor.KeyTextBox`
  - `FieldEditor.SectionKindComboBox`
  - `FieldEditor.EditorKindComboBox`
  - `FieldEditor.ValueKindComboBox`
  - `FieldEditor.BooleanStyleComboBox`
  - `FieldEditor.SeparatorTextBox`
  - `FieldEditor.EnumNameTextBox`
  - `FieldEditor.DescriptionSection`
  - `FieldEditor.DisplayNameTextBox`
  - `FieldEditor.AliasesTextBox`
  - `FieldEditor.AllowedValuesTextBox`
  - `FieldEditor.DescriptionTextBox`
  - `FieldEditor.SavePreviewSection`
  - `FieldEditor.CopyPersistedPreviewButton`
  - `FieldEditor.ProjectPreviewButton`
  - `FieldEditor.GlobalPreviewButton`
  - `FieldEditor.PreviewSummaryText`
  - `FieldEditor.PreviewIssuesGrid`
  - `FieldEditor.PersistedPreviewTextBox`
  - `FieldEditor.ApplyResultPanel`
  - `FieldEditor.TargetPathTextBox`
  - `FieldEditor.CopyTargetPathButton`
  - `FieldEditor.OpenTargetFolderButton`
  - `FieldEditor.ManifestPathTextBox`
  - `FieldEditor.CopyManifestPathButton`
  - `FieldEditor.OpenManifestFolderButton`
  - `FieldEditor.StatusText`
  - `FieldEditor.ProjectSaveButton`
  - `FieldEditor.GlobalSaveButton`
  - `FieldEditor.CancelButton`
- Existing tests:
  - `RA2IniEditor.Tests/IDE/Ra2FieldEditorWindowBoundaryTests.cs`
  - `RA2IniEditor.Tests/IDE/FieldEditorSavePreviewTests.cs`
  - `WpfAutomationHarnessBoundaryTests` checks key editor IDs.
- Reads/writes registry state:
  - Reads effective provider/provenance via `FieldEditorSaveContext`.
  - Builds Project/Global save previews.
  - Writes Project/Global field registry through `FieldEditorViewModel.ApplySave` and raises `FieldRegistrySaveApplied` on success.
  - Can copy paths and open target/manifest folders; those are OS shell actions, not registry semantics.
- Current UX problems:
  - Default WPF title bar remains.
  - Basic fields, value metadata, description, allowed values, preview, JSON, apply result paths, and save buttons form one long surface.
  - Target scope is represented by separate preview/save buttons instead of a stronger target selector and write boundary.
  - Full target/manifest paths are primary text boxes and can dominate the layout after save.
- Proposed category: Editor Dialog.
- Future contract:
  - Use custom lightweight editor dialog chrome with stable close AutomationId.
  - Reframe layout into compact identity card, metadata editor, documentation/allowed values editor, validation/preview, and explicit save boundary.
  - Preserve preview-before-save behavior and backup manifest visibility.
  - Add stable IDs for target selector/summary if a selector is introduced.
- Non-goals:
  - Do not change draft factory, preview builder, apply service, validation rules, or target semantics.
  - Do not save without an explicit user action.
  - Do not convert this into a legacy table editor.
- Tests needed:
  - Boundary tests for custom chrome, validation summary, target section, preview section, apply result section, and close button.
  - Existing ViewModel and save preview tests remain primary for semantics.

### 3.4 Allowed Values Editor

- XAML: `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml`
- Code-behind: `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml.cs`
- ViewModel: internal nested `AllowedValuesEditorViewModel`
- Entry point: Field Learning Wizard `EditAllowedValues`
- Owner assignment: Field Learning Wizard sets `Owner = this`.
- Modal/non-modal: modal, `ShowDialog()`.
- Window properties:
  - `WindowStyle`: default WPF chrome, not explicitly set.
  - `ShowInTaskbar`: default, not explicitly set.
  - `ResizeMode`: `CanResize`
  - `SizeToContent`: default/manual, not explicitly set.
  - `WindowStartupLocation`: default/manual, not explicitly set.
  - `Width`: 820
  - `Height`: 560
  - `MinWidth`: 660
  - `MinHeight`: 440
- Existing AutomationIds:
  - `AllowedValuesEditor.Window`
  - `AllowedValuesEditor.Grid`
  - `AllowedValuesEditor.AddButton`
  - `AllowedValuesEditor.RemoveButton`
  - `AllowedValuesEditor.DedupeButton`
  - `AllowedValuesEditor.SortButton`
  - `AllowedValuesEditor.AppendBuiltInButton`
  - `AllowedValuesEditor.RestoreScannedButton`
  - `AllowedValuesEditor.OkButton`
  - `AllowedValuesEditor.CancelButton`
- Existing tests:
  - `Ra2FieldLearningWizardBoundaryTests.AllowedValuesEditorWindow_IsLocalDraftEditorWithoutRegistryApplyOrSaveCoupling`
  - `Ra2FieldEditorWindowBoundaryTests.AllowedValuesEditor_UsesPlainChineseColumnsAndDisabledRemoveButton`
  - `WpfAutomationHarnessBoundaryTests` reads this XAML for shared secondary-window checks.
- Reads/writes registry state:
  - Does not write registry directly.
  - Returns `ResultText` to the parent row when OK is accepted.
  - Can append BuiltIn completion values through local catalog lookup.
- Current UX problems:
  - Default WPF title bar remains.
  - Editable DataGrid dominates the dialog.
  - No explicit validation/duplicate summary before OK.
  - OK/Cancel are visually correct but not framed as "return to draft row only".
- Proposed category: Editor Dialog.
- Future contract:
  - Use compact custom dialog chrome or consistent editor dialog chrome.
  - Keep modal scope narrow: edit value list only, no registry write.
  - Add field key/type summary and validation/duplicate summary.
  - Keep table only if it remains the clearest value-list editor; avoid legacy-editor visual language.
- Non-goals:
  - Do not write registry files directly.
  - Do not change allowed-values serialization format.
  - Do not change BuiltIn candidate append semantics.
- Tests needed:
  - Boundary tests for chrome, summary, OK/Cancel, and no direct registry save/apply references.
  - Optional unit-style tests if the nested ViewModel is later extracted by an approved task.

### 3.5 Remote Preset Editor

- XAML: `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml`
- Code-behind: `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml.cs`
- ViewModel: `RemoteSourcePresetEditorViewModel`
- Entry point: Field Import Preview `AddPreset` / `EditPreset`
- Owner assignment: Field Import Preview sets `Owner = this`.
- Modal/non-modal: modal, `ShowDialog()`.
- Window properties:
  - `WindowStyle`: default WPF chrome, not explicitly set.
  - `ShowInTaskbar`: default, not explicitly set.
  - `ResizeMode`: `CanResize`
  - `SizeToContent`: default/manual, not explicitly set.
  - `WindowStartupLocation`: `CenterOwner`
  - `Width`: 540
  - `Height`: 360
  - `MinWidth`: 460
  - `MinHeight`: 320
- Existing AutomationIds:
  - `RemoteSourcePresetEditor.Window`
  - `RemoteSourcePresetEditor.NameTextBox`
  - `RemoteSourcePresetEditor.UrlTextBox`
  - `RemoteSourcePresetEditor.DescriptionTextBox`
  - `RemoteSourcePresetEditor.TagsTextBox`
  - `RemoteSourcePresetEditor.EnabledCheckBox`
  - `RemoteSourcePresetEditor.ValidationText`
  - `RemoteSourcePresetEditor.OkButton`
  - `RemoteSourcePresetEditor.CancelButton`
- Existing tests:
  - `RA2IniEditor.Tests/IDE/RemoteSourcePresetEditorViewModelTests.cs`
  - `WpfAutomationHarnessBoundaryTests.RemoteSourcePresetEditor_ExposesStableAutomationIds`
- Reads/writes registry state:
  - Does not write registry or fetch remote content directly.
  - Returns an edit model to Field Import Preview.
  - Parent ViewModel handles preset persistence.
- Current UX problems:
  - Default WPF title bar remains.
  - Compact but still a plain label/textbox form.
  - It does not clearly distinguish editing local preset metadata from fetching remote content.
- Proposed category: Preset / Remote Dialog.
- Future contract:
  - Use compact custom dialog chrome or consistent preset dialog chrome.
  - Add source/status card explaining local-only preset edit.
  - Keep validation visible and Chinese-first.
  - Preserve no automatic fetch/apply behavior.
- Non-goals:
  - Do not trigger network fetch from this dialog.
  - Do not write active field registry.
  - Do not change preset validation or storage semantics.
- Tests needed:
  - Boundary tests for chrome, local-only status text, required fields, OK/Cancel, and stable AutomationIds.
  - Existing ViewModel validation tests remain primary.

### 3.6 Apply / Rollback / Destructive Confirmations

- XAML: none; native `MessageBox` currently.
- Code-behind:
  - `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml.cs`
  - `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml.cs`
  - `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs`
- ViewModel:
  - `FieldRegistryManagerViewModel` / `FieldRegistryRollbackConfirmationViewModel`
  - `FieldRegistryHarvestPreviewViewModel` / `FieldRegistryApplyConfirmationViewModel`
- Entry point:
  - Manager `ApplyCleanupPlan`
  - Manager `RollbackSelected`
  - Import Preview `ApplyCurrentPlan`
  - Import Preview `ClearRemoteHistory`
  - Import Preview `RemovePreset`
  - Learning Wizard `ApplyCurrentPlan`
  - Learning Wizard `EditAllowedValues` information MessageBox for non-editable row types
  - Field Editor path copy/open failures use warning MessageBox
- Owner assignment: MessageBox calls pass the current window as owner where relevant.
- Modal/non-modal: modal native MessageBox.
- Window properties: not applicable.
- Existing AutomationIds: none for MessageBox confirmations.
- Existing tests:
  - `FieldRegistryHarvestPreviewWindowApplyGuardrailTests`
  - `FieldRegistryRollbackUiBoundaryTests`
  - `FieldRegistryManagerRollbackViewModelTests`
  - ViewModel apply/rollback tests in IDE and Infrastructure test folders.
- Reads/writes registry state:
  - Apply cleanup, rollback selected, import apply, and learning apply write state only after confirmation.
  - Clear remote history and remove preset affect local remote metadata after confirmation.
  - Field Editor path warning MessageBoxes do not write registry.
- Current UX problems:
  - Native MessageBox cannot expose rich risk summaries, target scope, target file, backup manifest, operation counts, or warning chips.
  - Some remote confirmation strings are still English.
  - No stable AutomationIds for UI automation if these become custom dialogs later.
- Proposed category: Confirmation Dialog.
- Future contract:
  - Introduce small owner-bound confirmation dialogs only after a dedicated A15-2F contract is approved.
  - High-risk apply/rollback confirmations should show action summary, target scope/file, backup/manifest, operation count, warnings, and primary/secondary actions.
  - Low-risk clear/remove confirmations may remain MessageBox unless A15-2F explicitly expands scope.
  - All confirmation text should be Chinese-first.
- Non-goals:
  - Do not remove explicit confirmation.
  - Do not auto-apply after plan build.
  - Do not change backup manifest, rollback, apply writer, or remote metadata behavior.
- Tests needed:
  - Boundary tests for custom confirmation AutomationIds if introduced.
  - ViewModel tests for confirmation title/message/summary data.
  - Guardrail tests that write actions still require confirmation.

## 4. Cross-cutting Problems

- Default WPF title icon/titlebar/chrome remains on all discovered tertiary windows.
- Workflow dialogs mix source input, review, target selection, advanced details, and write actions in a single large form.
- Apply/write actions are sometimes visually adjacent to read-only parse/preview/build actions.
- Long paths and raw JSON can dominate primary content.
- DataGrid-heavy regions are necessary for diffs and rows, but they can resemble the removed legacy table editor if not framed carefully.
- Remote source operations, local preset metadata, and active registry writes need clearer boundaries.
- Confirmation surfaces are native MessageBox only and have no stable AutomationIds.
- Some user-facing strings remain English in remote history/preset confirmations and system file dialogs.

## 5. Common Chrome / Style Rules

Future tertiary implementation phases should follow these rules unless a task-specific contract says otherwise:

- Use custom lightweight chrome for top-level tertiary Field Registry windows where appropriate.
- Preserve resize behavior for large workflow/editor windows.
- Preserve owner relationships and modal/non-modal behavior unless explicitly approved.
- Add a close button AutomationId for each custom chrome window.
- Keep existing AutomationIds and only append new stable IDs.
- Do not use tiny `SizeToContent` popup behavior for large workflow windows.
- Keep visual hierarchy consistent with Field Registry Center / Manager: compact header, status chips, restrained cards, and explicit write boundaries.
- Use DataGrid only where scanning/comparing rows is the core task.
- Do not add decorative-only UI or marketing-style layouts.

## 6. Localization Rules

- Visible user-facing text in these tertiary Field Registry surfaces should be Chinese-first in future implementation phases.
- Do not translate field keys, section names, source file names, paths, INI content, enum/data values from packs, or AutomationId values.
- English protocol/domain terms such as URL, JSON, INI, Key, Project, Global, BuiltIn may remain where they are product vocabulary.
- Remote confirmation strings currently in English should be localized when that surface is in scope.
- System `OpenFileDialog` / `SaveFileDialog` titles and filters may be localized only in the remote/preset phase if preserving behavior is straightforward.

## 7. Recommended Redesign Order

Recommended staged order:

1. A15-2C: Field Import Preview workflow contract / implementation
2. A15-2D: Field Editor + Allowed Values Editor contract / implementation
3. A15-2E: Field Learning Wizard workflow contract / implementation
4. A15-2F: Apply / Rollback / destructive confirmation consistency
5. A15-2G: Remote Preset / optional tertiary surfaces

Rationale:

- Import Preview is the densest write-capable workflow and also owns remote history/presets, so it should establish the workflow pattern first.
- Field Editor and Allowed Values Editor are focused editor dialogs and can then reuse target/validation/write-boundary language.
- Learning Wizard shares import/apply concepts but has a different source/draft review rhythm; it should follow once import vocabulary stabilizes.
- Confirmation consistency should come after the exact operation summaries and warning data are agreed.
- Remote Preset Editor can remain compact until the import/remote source model is settled.

## 8. Risk Matrix

| Area | Risk | Why | Mitigation |
| --- | --- | --- | --- |
| Field Import Preview | High | Fetch, parse, diff, plan, apply, remote cache, preset persistence, import/export converge in one window | Use step-based contract; preserve ViewModel semantics; test no auto-fetch/auto-apply |
| Field Learning Wizard | High | Generates drafts from current INI/text and writes field registry after confirmation | Keep source/draft/target/apply boundaries explicit; preserve shared harvest ViewModel behavior |
| Field Editor | High | Writes Project/Global registry files and creates backup manifests | Keep preview-before-save and explicit target boundary; preserve apply service |
| Allowed Values Editor | Medium | Edits parent draft row, not registry directly, but table UI can imply broader editing | Keep modal scope narrow; assert no direct registry save/apply coupling |
| Remote Preset Editor | Medium | Persists local remote source metadata and may be confused with network fetch | Clarify local-only edit; keep fetch outside dialog |
| Apply / Rollback confirmations | High | Destructive or state-restoring operations are currently thin MessageBoxes | Introduce richer confirmations only under A15-2F; keep explicit confirmation guardrails |
| System file dialogs for preset import/export | Medium | Native dialogs are outside WPF styling and touch filesystem | Treat as remote/preset phase only; do not replace without explicit contract |

## 9. Acceptance Criteria for Future Phases

For future tertiary implementation phases:

- Scope is approved before code changes.
- Modified files are limited to approved surface XAML/code-behind/ViewModel/tests/docs.
- Existing AutomationIds are preserved; new AutomationIds are stable and documented.
- Main Shell layout is not changed unless explicitly approved.
- Field Registry load order, Project > Global > BuiltIn priority, parser/diff/apply/rollback/backup/learning/editor/allowed-values/remote semantics remain unchanged.
- Write-capable actions still require explicit preview and/or confirmation where they do today.
- Remote fetch and remote preset actions do not become automatic.
- No legacy project, legacy table editor, or removed object workbench is restored.
- Future implementation includes focused boundary tests and relevant ViewModel tests.
- `dotnet build`, `dotnet test`, and `IdeOnly` package pass unless a narrower validation set is explicitly approved.
- Manual smoke opens each affected surface, verifies chrome/close/resize behavior, checks disabled states, and confirms no unintended writes.
