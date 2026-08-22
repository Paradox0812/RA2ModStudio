# Codex Task: RA2IniEditor.IDE A15-2B-P Field Registry Center / Manager Visual Polish Limited Implementation

## 0. Context

A15-2B-R visual contract has been completed and reviewed.

This task approves limited implementation for:

```text
A15-2B-P: Field Registry Center / Manager visual polish
```

This is not a free frontend redesign.

The goal is to improve the visual clarity of the already implemented A15-2B read-only management layout, while preserving all Field Registry behavior semantics.

---

## 1. Current Baseline

```text
RA2IniEditor.IDE-only
A15-2B Field Registry Center / Manager read-only management layout completed
Tests: 1296 passed
IdeOnly package: passed
Legacy table-style editor: not restored
Shell main layout: unchanged
```

---

## 2. Goal

Improve Field Registry Center / Manager visual hierarchy and reduce default WPF form feeling.

Main goals:

```text
1. Make Project > Global > BuiltIn look like a status/source priority strip.
2. Make status summaries scannable with compact cards/chips.
3. Reduce long path visual noise with short display text and tooltip.
4. Keep search/filter/field count visually grouped.
5. Separate read-only entry actions from write/risk actions.
6. Reduce large blank table/list regions with compact empty states.
7. Localize user-facing English UI labels in these two windows where safe.
8. Preserve all current behavior, commands, bindings, AutomationIds, and semantics.
```

---

## 3. Target Surfaces

Allowed:

```text
Field Registry Center
Field Registry Manager / Advanced Tools
```

Deferred:

```text
Field Learning Wizard -> A15-2E
Field Import Preview -> A15-2C
Field Editor / Allowed Values Editor -> A15-2D
Apply / Rollback confirmation redesign -> A15-2F
```

Do not modify deferred surfaces.

---

## 4. Files Allowed

Allowed:

```text
FieldRegistryCenterWindow.xaml
FieldRegistryManagerWindow.xaml
FieldRegistryManagerViewModel.cs
FieldRegistryPackStatusViewModel.cs
WpfAutomationHarnessBoundaryTests.cs
FieldRegistryManagerViewModelTests.cs
```

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
Field Registry loader/writer/apply/rollback services
parser
diagnostics
completion
hover
quick peek
save preflight
backup/rollback infrastructure
BuiltIn field registry JSON
solution/project files
legacy files
```

---

## 5. Hard Semantic Boundaries

Do not change:

```text
Project > Global > BuiltIn priority
load order
active pack load/reload semantics
import preview semantics
cleanup plan semantics
apply cleanup behavior
rollback behavior
field learning behavior
backup manifest behavior
diagnostics/completion/hover behavior
save/dirty behavior
```

Only display-only properties are allowed.

---

## 6. UI Text Localization Boundary

This task may localize visible user-facing UI text only in Field Registry Center / Manager.

Do not translate:

```text
field keys
section names
source file names
paths
INI content
enum/data values from packs
AutomationId values
```

---

## 7. Center Contract

Preserve existing AutomationIds, including:

```text
FieldRegistryCenter.Window
FieldRegistryCenter.HeaderArea
FieldRegistryCenter.Toolbar
FieldRegistryCenter.ActionGroup
FieldRegistryCenter.ReloadButton
FieldRegistryCenter.LearnFieldsButton
FieldRegistryCenter.NewFieldButton
FieldRegistryCenter.EditFieldButton
FieldRegistryCenter.AdvancedToolsButton
FieldRegistryCenter.PriorityStrip
FieldRegistryCenter.StatusSummaryPanel
FieldRegistryCenter.SearchArea
FieldRegistryCenter.SearchTextBox
FieldRegistryCenter.WarningSummary
FieldRegistryCenter.ActivePacksPanel
FieldRegistryCenter.PacksGrid
FieldRegistryCenter.MainFieldsPanel
FieldRegistryCenter.FieldsGrid
FieldRegistryCenter.StatusArea
```

Add stable IDs as applicable:

```text
FieldRegistryCenter.HeaderChips
FieldRegistryCenter.ProjectStatusCard
FieldRegistryCenter.GlobalStatusCard
FieldRegistryCenter.BuiltInStatusCard
FieldRegistryCenter.FieldCountChip
FieldRegistryCenter.SearchSummaryRow
FieldRegistryCenter.ActivePacksCompactList
FieldRegistryCenter.PriorityChipProject
FieldRegistryCenter.PriorityChipGlobal
FieldRegistryCenter.PriorityChipBuiltIn
```

Required visual changes:

```text
HeaderArea: Chinese-first title, compact subtitle.
ActionGroup / Toolbar: compact IDE action row, preserve buttons.
PriorityStrip: flat status strip, not bordered paragraph.
StatusSummaryPanel: three readable cards with short/muted text.
SearchArea: combine search and field count.
WarningSummary: compact warning chip/block.
ActivePacksPanel: reduce visual weight and long path noise.
MainFieldsPanel: preserve grid behavior, reduce distracting explanatory text.
```

---

## 8. Manager Contract

Preserve existing AutomationIds, including:

```text
FieldRegistryManager.Window
FieldRegistryManager.HeaderArea
FieldRegistryManager.StatusHubPanel
FieldRegistryManager.EntryActionsPanel
FieldRegistryManager.Toolbar
FieldRegistryManager.ActivePacksPanel
FieldRegistryManager.ActivePacksHelpText
FieldRegistryManager.PacksGrid
FieldRegistryManager.ReloadButton
FieldRegistryManager.OpenFieldImportPreviewButton
FieldRegistryManager.RelearnCurrentIniButton
FieldRegistryManager.OpenGlobalFolderButton
FieldRegistryManager.OpenProjectFolderButton
FieldRegistryManager.RollbackPanel
FieldRegistryManager.RefreshRollbackManifestsButton
FieldRegistryManager.RollbackSelectedButton
FieldRegistryManager.OpenRollbackTargetFolderButton
FieldRegistryManager.OpenRollbackManifestFolderButton
FieldRegistryManager.OpenRollbackBackupFolderButton
FieldRegistryManager.RollbackManifestsGrid
FieldRegistryManager.RollbackDisabledReason
FieldRegistryManager.RollbackStatusText
FieldRegistryManager.CleanupPreviewPanel
FieldRegistryManager.BuildCleanupPlanButton
FieldRegistryManager.ApplyCleanupPlanButton
FieldRegistryManager.RepairPreviewTabs
FieldRegistryManager.CleanupPlanGrid
FieldRegistryManager.RepairPreviewSummary
FieldRegistryManager.RepairPreviewAbstractFieldsGrid
FieldRegistryManager.RepairPreviewRemovedConcreteGrid
FieldRegistryManager.RepairPreviewSkippedGrid
FieldRegistryManager.RepairPreviewWarningsList
FieldRegistryManager.CleanupStatusText
FieldRegistryManager.WarningsPanel
FieldRegistryManager.WarningsBox
FieldRegistryManager.StatusText
```

Add stable IDs as applicable:

```text
FieldRegistryManager.StatusChips
FieldRegistryManager.ActivePackChip
FieldRegistryManager.WarningChip
FieldRegistryManager.ProjectChip
FieldRegistryManager.GlobalChip
FieldRegistryManager.BuiltInChip
FieldRegistryManager.ReadOnlyActionsGroup
FieldRegistryManager.WriteActionsGroup
FieldRegistryManager.RollbackRiskSummary
FieldRegistryManager.CleanupReadOnlySummary
FieldRegistryManager.CleanupWriteWarning
FieldRegistryManager.WarningsEmptyState
```

Required visual changes:

```text
HeaderArea: compact Chinese-first title.
StatusHubPanel: use chips/status tiles.
EntryActionsPanel: only read-only/entry actions.
RollbackPanel: keep backup list and commands; rollback is write/risk action.
CleanupPreviewPanel: separate build preview from apply cleanup.
WarningsPanel: compact empty state.
```

---

## 9. Display-only ViewModel Properties

Existing display properties may be reused:

```text
SourcePriorityText
LoadedPackSummaryText
ProjectRegistryDisplayText
GlobalRegistryDisplayText
BuiltInFallbackDisplayText
WarningSummaryText
ProjectFolderDisabledReason
RollbackDisabledReason
```

Allowed new display-only properties:

```text
ProjectRegistryShortDisplayText
ProjectRegistryFullPathToolTip
GlobalRegistryShortDisplayText
GlobalRegistryFullPathToolTip
ActiveSourceChipText
WarningChipText
BuiltInChipText
CenterFieldListSummaryText
WarningsEmptyStateText
CleanupPreviewEmptyStateText
RollbackEmptyStateText
CleanupWriteWarningText
```

Optional for `FieldRegistryPackStatusViewModel`:

```text
ShortDirectoryPath
DirectoryPathToolTip
StatusChipText
```

Rules:

```text
Derived only from existing state.
No file IO.
No reload.
No mutation.
No semantic change.
```

---

## 10. Existing Handlers Must Be Reused

Do not change signatures or call chains.

Center:

```text
ReloadLocalFieldRegistry
OpenFieldLearning
CreateNewField
EditSelectedField
OpenAdvancedTools
```

Manager:

```text
ReloadLocalFieldRegistry
OpenHarvestPreview
RelearnCurrentIni
OpenGlobalRegistryFolder
OpenProjectRegistryFolder
RefreshRollbackManifests
RollbackSelected
OpenRollbackTargetFolder
OpenRollbackManifestFolder
OpenRollbackBackupFolder
BuildCleanupPlan
ApplyCleanupPlan
```

`ApplyCleanupPlan` and `RollbackSelected` are write/risk actions. This phase may move their visual grouping and labels, but must not change behavior.

---

## 11. Tests

Update `WpfAutomationHarnessBoundaryTests.cs`:

```text
Center new IDs exist.
Manager new IDs exist.
Existing key buttons still exist.
ApplyCleanupPlanButton appears in/near write/risk section.
RollbackSelectedButton appears in/near rollback risk section.
```

Update `FieldRegistryManagerViewModelTests.cs`:

```text
Short path display does not replace full path tooltip.
BuiltIn chip does not fake local pack count.
Warning zero/non-zero text.
Rollback empty/disabled reason.
Cleanup empty state text.
Display-only properties do not mutate Packs, Warnings, RollbackManifests, CleanupPlanRows.
```

Avoid pixel-perfect tests.

---

## 12. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 13. Final Report

Report:

```text
Phase completed: A15-2B-P.
Files changed.
UI sections changed.
Localized strings changed.
Display-only ViewModel properties added/changed.
Existing handlers preserved.
Commands run.
Build/test/package result.
Confirmation Field Registry semantics unchanged.
Confirmation Shell layout unchanged.
Confirmation legacy not restored.
Documentation updates made.
Remaining risks.
Recommended next phase.
```
