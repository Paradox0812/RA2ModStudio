# Field Registry Surfaces UI Contract

> Successor status (2026-07-23): UI-MODERN-PROGRAM-R1 M4 implemented the approved presentation-only modernization. The exact current inventory is `Docs/UI-MODERN-PROGRAM-R1_M4_FieldRegistryExactUiInventory.md` and the completion/evidence record is `Docs/UI-MODERN-PROGRAM-R1_M4_StageLedger.md`. This A15-era document remains the semantic/lifecycle background contract; obsolete statements about default native chrome or future-only layout are superseded by those two documents.

## 1. Scope and Baseline

Baseline: RA2IniEditor.IDE-only after A15-1R2 Floating Inspector Placement / Close Button.

This document is the A15-2A read-only inventory and UI contract for Field Registry-related surfaces. It does not implement UI changes. It exists to prevent freeform redesign and to define an explicit, user-approvable boundary before any future Field Registry UI work.

Current accepted constraints:

- Active solution: `RA2IniEditor.IDE.sln`
- Active package profile: `IdeOnly`
- Legacy table-style editor is intentionally absent and must not be restored.
- Main Shell layout remains frozen unless a future Shell-specific task is approved.
- Field Registry semantics must remain unchanged: Project > Global > BuiltIn priority, import diff semantics, apply / rollback behavior, backup manifest behavior, field learning behavior, diagnostics, completion, hover, quick peek, save preflight, undo / redo.

## 2. Inventory Summary

| Surface | Files | Type | Entry Point | Writes State | Future Classification | Risk | Proposed Phase |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Field Registry Center | `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml`, `.xaml.cs` | Non-modal window | Shell menu / toolbar `OpenFieldRegistryManagerWindow`; Manager advanced entry can return to related flows | Reload event only; opens Field Editor / learning / advanced tools | Management Tool Window | High | A15-2B |
| Field Registry Manager | `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml`, `.xaml.cs` | Non-modal window | Shell menu `OpenAdvancedFieldRegistryToolsWindow`; Center advanced tools | Can apply cleanup plan and rollback after confirmation; opens folders | Management Tool Window + Workflow Hub | High | A15-2B |
| Field Editor / New Field Editor | `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml`, `.xaml.cs`; `RA2IniEditor.IDE/ViewModels/FieldRegistry/FieldEditorViewModel.cs` | Non-modal child window | Field Registry Center new/edit/double-click | Can save to Project or Global registry after preview; creates backup manifest through apply service | Editor Dialog | High | A15-2D |
| Field Import Preview | `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml`, `.xaml.cs`; `RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs` | Non-modal window | Manager `OpenHarvestPreview`; Shell handler `FieldRegistryManagerWindow_OnHarvestPreviewRequested` | Can fetch/cache remote text, manage presets/history, build/apply plans after confirmation | Workflow Dialog | High | A15-2C |
| Field Learning Wizard | `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml`, `.xaml.cs`; uses `FieldRegistryHarvestPreviewViewModel` | Non-modal window | Center learning; Shell current INI/current section menu; Manager relearn current INI | Can build/apply plans after confirmation | Workflow Dialog | High | A15-2E |
| Allowed Values Editor | `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml`, `.xaml.cs` | Modal dialog | Field Learning Wizard row button `EditAllowedValues` | Only returns edited allowed-values text to current preview row; does not write registry by itself | Editor Dialog | Medium | A15-2D |
| Remote Preset Editor | `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml`, `.xaml.cs`; `RemoteSourcePresetEditorViewModel` | Modal dialog | Field Import Preview add/edit preset | Returns preset edit model; parent writes preset changes | Workflow Dialog | Medium | A15-2C |
| Apply / Rollback / destructive confirmations | MessageBox calls in `FieldRegistryManagerWindow.xaml.cs`, `FieldRegistryHarvestPreviewWindow.xaml.cs`, `FieldLearningWizardWindow.xaml.cs` | Modal confirmation | Cleanup apply, rollback selected, import apply, learning apply, clear/remove remote history/preset | Yes, when user confirms parent operation | Confirmation Dialog | High | A15-2F |
| Registry reload / open folder / status actions | Shell, Center, Manager code-behind handlers | Commands and status areas | Center / Manager toolbar buttons | Reload changes runtime provider state; open folder launches Explorer; status is display-only | Management Tool Window command group | Medium | A15-2B |

Named surface not found as a standalone window in the current IDE-only package:

- `Field Apply` standalone window: not found. Apply currently uses buttons plus MessageBox confirmation inside import preview / learning wizard / field editor workflows.
- `Rollback` standalone window: not found. Rollback currently uses Manager grid plus MessageBox confirmation.

## 3. Current UX Problems

- Field Registry Center and Manager both expose broad registry concepts, so their responsibility boundary is not obvious enough at first glance.
- Manager mixes persistent status, import launch, cleanup preview/apply, rollback manifests, folder openers, and warnings in one dense screen.
- Import Preview and Field Learning Wizard share the same heavy ViewModel and similar apply-plan concepts, but users may not clearly see where the workflow is: source input, validation, diff, apply plan, confirmation, result.
- DataGrid density is high. Tables are useful for diffs and rows, but status and risk summaries should not feel like raw form scaffolding.
- Project / Global / BuiltIn priority is explained in text in some places, but the future UI should make priority visual and persistent.
- Write-capable actions are available near read-only preview areas; future layout should make destructive/write boundaries more explicit.
- MessageBox confirmations are functional but too thin for complex apply/rollback risks.
- Field Editor includes preview, validation, JSON, target path, manifest path, and save actions in one long form; target scope and disabled reasons need stronger information hierarchy.
- Allowed Values Editor is scoped, but its DataGrid-heavy editor can feel like the removed legacy table editor unless framed as a focused value-list dialog.

## 4. Proposed Information Architecture

### 4.1 Management Surfaces

Management surfaces should be persistent IDE tool windows:

- Field Registry Center
- Field Registry Manager
- Registry reload / open folder / status actions

Future layout should prioritize:

- Compact tool header.
- Source priority display: Project > Global > BuiltIn.
- Active pack status summary.
- Warning/error list with clear disabled reasons.
- Small action toolbar for reload, learn, new field, edit field, advanced tools.
- Tables only where scanning/comparing many rows is useful.

### 4.2 Workflow Surfaces

Workflow surfaces should be step-based and review-before-write:

- Field Import Preview
- Field Learning Wizard
- Field Apply confirmation
- Rollback confirmation
- Remote preset import/apply workflows

Future layout should prioritize:

- Step labels: Source -> Parse -> Validate -> Diff -> Build Plan -> Confirm Apply.
- Explicit target scope: Project / Global.
- Apply mode and target file preview near apply action.
- Warnings/errors before the destructive action.
- Apply/cancel/rollback boundaries with no implicit write.

### 4.3 Editor Surfaces

Editor surfaces should be compact inspectors/editors:

- Field Editor / New Field Editor
- Allowed Values Editor

Future layout should prioritize:

- Compact field summary.
- Validation summary.
- Clear target selection.
- Preview-before-save.
- Value editing controls that are dense but not visually equivalent to the removed legacy object table editor.

### 4.4 Confirmation Surfaces

Confirmation surfaces should be small, risk-focused, and owner-bound:

- Cleanup apply confirmation.
- Apply field import / learning confirmation.
- Rollback selected confirmation.
- Remove remote preset / clear remote history confirmation.

Future layout should prioritize:

- Short action summary.
- Target scope / file.
- Risk list.
- Primary and secondary actions.
- No broad forms or unrelated controls.

## 5. Per-Surface Contracts

### 5.1 Field Registry Center

- Current files:
  - `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml`
  - `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml.cs`
  - Uses `FieldRegistryManagerViewModel` through `Manager` plus internal `FieldRegistryCenterFieldRow` rows.
- Current window properties:
  - `WindowStyle`: default WPF window chrome.
  - `ShowInTaskbar`: default.
  - `ResizeMode`: `CanResize`.
  - `SizeToContent`: default manual.
  - `WindowStartupLocation`: default manual.
  - `Width`: 1040.
  - `Height`: 700.
  - `MinWidth`: 820.
  - `MinHeight`: 620.
  - Owner behavior: Shell sets `Owner = this`.
  - Show path: `ShellWindow.OpenFieldRegistryManagerWindow` creates `FieldRegistryCenterWindow` and calls `Show()`.
- Current behavior:
  - Shows active packs, field list, search, reload, learning, new/edit field, advanced tools.
  - Opens `FieldEditorWindow` as a non-modal child with owner set to Center.
- Problems:
  - Name says Center, but Shell method name still says `OpenFieldRegistryManagerWindow`.
  - Dense grids dominate; priority and active pack status should be clearer.
  - New/edit actions are close to read-only browse without a stronger write-boundary cue.
- Proposed layout:
  - Management pane with status cards for Project / Global / BuiltIn.
  - Compact action toolbar.
  - Field browser table below status, with search/filter row.
  - Clear empty states for no project registry and no global registry.
- Allowed future changes:
  - Reorganize layout and labels.
  - Add visual priority chips.
  - Improve disabled reasons and status summaries.
- Forbidden future changes:
  - Change provider priority or reload semantics.
  - Change field save/apply behavior.
  - Move responsibilities into legacy table workflow.
- Tests needed:
  - XAML boundary IDs for status cards and toolbar.
  - ViewModel display tests for Project / Global / BuiltIn status text.
  - No semantic mutation from layout-only changes.

### 5.2 Field Registry Manager

- Current files:
  - `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml`
  - `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml.cs`
  - DataContext: `FieldRegistryManagerViewModel`.
- Current window properties:
  - `WindowStyle`: default WPF window chrome.
  - `ShowInTaskbar`: default.
  - `ResizeMode`: `CanResize`.
  - `SizeToContent`: default manual.
  - `WindowStartupLocation`: default manual.
  - `Width`: 1120.
  - `Height`: 880.
  - `MinWidth`: 920.
  - `MinHeight`: 720.
  - Owner behavior: Shell sets `Owner = this`.
  - Show path: `ShellWindow.OpenAdvancedFieldRegistryToolsWindow` calls `Show()`.
- Current behavior:
  - Shows active packs, rollback manifests, cleanup preview, warnings, status.
  - Opens Field Import Preview.
  - Applies cleanup after MessageBox confirmation.
  - Rolls back selected manifest after MessageBox confirmation.
  - Opens global/project/rollback folders.
- Problems:
  - Too many operation categories in one screen.
  - Cleanup and rollback are high-risk but visually share space with status browsing.
  - Confirmation surface is MessageBox-only.
- Proposed layout:
  - Management hub with separate bands/tabs for Status, Import, Cleanup, Rollback.
  - Rollback area should expose target, manifest, backup, action summary before confirmation.
  - Cleanup should show read-only preview before apply and a clear apply boundary.
- Allowed future changes:
  - Reorder panels.
  - Introduce status cards and warning summaries.
  - Replace complex MessageBox confirmations with dedicated small confirmations.
- Forbidden future changes:
  - Change cleanup planner/apply semantics.
  - Change rollback manifest reader/service behavior.
  - Auto-run import, cleanup, or rollback.
- Tests needed:
  - XAML boundary tests for distinct Status / Cleanup / Rollback regions.
  - Command availability tests for rollback and project folder disabled reasons.

### 5.3 Field Editor / New Field Editor

- Current files:
  - `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml`
  - `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml.cs`
  - `RA2IniEditor.IDE/ViewModels/FieldRegistry/FieldEditorViewModel.cs`
- Current window properties:
  - `WindowStyle`: default WPF window chrome.
  - `ShowInTaskbar`: default.
  - `ResizeMode`: `CanResize`.
  - `SizeToContent`: default manual.
  - `WindowStartupLocation`: default manual.
  - `Width`: 900.
  - `Height`: 780.
  - `MinWidth`: 760.
  - `MinHeight`: 660.
  - Owner behavior: Field Registry Center sets `Owner = this`.
  - Show path: `FieldRegistryCenterWindow.OpenFieldEditor` calls `Show()`.
- Current behavior:
  - Edits a field definition.
  - Builds Project / Global save preview.
  - Saves to Project / Global registry through apply service.
  - Shows last target and backup manifest paths.
- Problems:
  - Target selection happens through separate buttons, not an obvious target scope selector.
  - Validation, JSON preview, save action, path results compete for attention.
  - Cancel is present but save success does not clearly frame what changed in the registry.
- Proposed layout:
  - Compact editor dialog with field identity summary.
  - Basic metadata, value metadata, documentation/allowed values sections.
  - Save preview section with validation summary.
  - Target scope selector plus explicit save action.
- Allowed future changes:
  - Reflow form sections.
  - Improve validation summary and disabled reasons.
  - Improve target/manifest result display.
- Forbidden future changes:
  - Change draft factory, preview builder, apply service, or save target semantics.
  - Save without preview where preview is currently required by workflow.
- Tests needed:
  - ViewModel validation tests remain primary.
  - XAML boundary tests for target selector / preview / apply controls in future design.

### 5.4 Field Import Preview

- Current files:
  - `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml`
  - `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml.cs`
  - `RA2IniEditor.IDE/ViewModels/FieldRegistryHarvestPreviewViewModel.cs`
- Current window properties:
  - `WindowStyle`: default WPF window chrome.
  - `ShowInTaskbar`: default.
  - `ResizeMode`: `CanResize`.
  - `SizeToContent`: default manual.
  - `WindowStartupLocation`: default manual.
  - `Width`: 1040.
  - `Height`: 720.
  - `MinWidth`: 820.
  - `MinHeight`: 650.
  - Owner behavior: Shell sets `Owner = this` when opened from Manager.
  - Show path: `ShellWindow.FieldRegistryManagerWindow_OnHarvestPreviewRequested` calls `Show()`.
- Current behavior:
  - Accepts pasted raw text, current INI, or fetched remote text.
  - Manages remote source history and presets.
  - Builds diff and apply plan.
  - Applies after MessageBox confirmation.
- Problems:
  - Main flow and advanced details are present but still visually dense.
  - Remote history/presets are nested in advanced details but still add workflow complexity.
  - Apply action needs stronger risk framing.
- Proposed layout:
  - Step-based workflow: Source, Preview, Plan, Confirm.
  - Keep remote history/presets in a secondary advanced area.
  - Keep target scope and apply mode near apply summary.
  - Promote validation and disabled reasons before apply.
- Allowed future changes:
  - Improve step navigation and visual hierarchy.
  - Reframe remote sources as advanced import sources.
  - Replace apply MessageBox with a richer confirmation.
- Forbidden future changes:
  - Auto-fetch or auto-apply.
  - Change diff semantics or apply plan builder behavior.
  - Add credentials or implicit network behavior.
- Tests needed:
  - Existing `FieldRegistryHarvestPreviewBoundaryTests` should remain.
  - Future tests for step labels, target scope visibility, disabled reasons, no automatic apply.

### 5.5 Field Learning Wizard

- Current files:
  - `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml`
  - `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml.cs`
  - Uses `FieldRegistryHarvestPreviewViewModel`.
- Current window properties:
  - `WindowStyle`: default WPF window chrome.
  - `ShowInTaskbar`: default.
  - `ResizeMode`: `CanResize`.
  - `SizeToContent`: default manual.
  - `WindowStartupLocation`: default manual.
  - `Width`: 1020.
  - `Height`: 720.
  - `MinWidth`: 820.
  - `MinHeight`: 620.
  - Owner behavior: Shell sets `Owner = this`.
  - Show path: `ShellWindow.OpenFieldLearningWizardWindow` calls `Show()`.
- Current behavior:
  - Learns fields from current INI/current section/pasted text.
  - Allows row-level allowed-values editing.
  - Builds and applies registry plan after MessageBox confirmation.
- Problems:
  - It is a workflow but currently appears as a large form with many tables.
  - Current source, parse result, target scope, validation, and plan are all visible without a clear step rhythm.
- Proposed layout:
  - Workflow dialog with source input, extracted drafts, validation/diff, apply plan.
  - Keep row edit affordance, but visually subordinate it to draft review.
  - Explicit target scope and apply mode.
- Allowed future changes:
  - Add clearer step headers.
  - Add empty states and disabled reasons.
  - Improve validation/plan summary.
- Forbidden future changes:
  - Change field learning parser/normalizer/generalization behavior.
  - Change apply plan or backup behavior.
- Tests needed:
  - XAML boundary tests for source, draft grid, target scope, apply plan.
  - ViewModel tests for disabled apply reason and current INI source behavior.

### 5.6 Allowed Values Editor

- Current files:
  - `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml`
  - `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml.cs`
  - DataContext: internal nested `AllowedValuesEditorViewModel`.
- Current window properties:
  - `WindowStyle`: default WPF window chrome.
  - `ShowInTaskbar`: default.
  - `ResizeMode`: `CanResize`.
  - `SizeToContent`: default manual.
  - `WindowStartupLocation`: default manual.
  - `Width`: 820.
  - `Height`: 560.
  - `MinWidth`: 660.
  - `MinHeight`: 440.
  - Owner behavior: Field Learning Wizard sets `Owner = this`.
  - Show path: `FieldLearningWizardWindow.EditAllowedValues` calls `ShowDialog()`.
- Current behavior:
  - Edits allowed values in a DataGrid.
  - Can add/remove/dedupe/sort/append BuiltIn/restore scanned values.
  - OK returns text to the parent row; Cancel discards.
- Problems:
  - Dense table can visually resemble the removed legacy editor if not framed as a focused value-list editor.
  - No separate validation summary.
- Proposed layout:
  - Compact editor dialog with key/type summary.
  - Value rows table plus concise toolbar.
  - Validation/duplicate summary before OK.
- Allowed future changes:
  - Improve summary and validation display.
  - Reflow toolbar.
- Forbidden future changes:
  - Write registry files directly from this dialog.
  - Reintroduce legacy table editor semantics.
- Tests needed:
  - Boundary tests for OK/Cancel and no direct registry apply.
  - ViewModel-style tests if nested model is extracted in a future approved phase.

### 5.7 Remote Preset Editor

- Current files:
  - `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml`
  - `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml.cs`
  - DataContext: `RemoteSourcePresetEditorViewModel`.
- Current window properties:
  - `WindowStyle`: default WPF window chrome.
  - `ShowInTaskbar`: default.
  - `ResizeMode`: `CanResize`.
  - `SizeToContent`: default manual.
  - `WindowStartupLocation`: `CenterOwner`.
  - `Width`: 540.
  - `Height`: 360.
  - `MinWidth`: 460.
  - `MinHeight`: 320.
  - Owner behavior: Field Import Preview sets `Owner = this`.
  - Show path: `FieldRegistryHarvestPreviewWindow.AddPreset` / `EditPreset` calls `ShowDialog()`.
- Current behavior:
  - Edits preset name, URL, description, tags, enabled state.
  - Validates before returning edit model to parent.
- Problems:
  - Works as a compact dialog, but future import workflow should clarify whether this affects local preset storage only.
- Proposed layout:
  - Keep as compact modal editor.
  - Add clearer validation and local-only persistence wording if needed.
- Allowed future changes:
  - Improve validation text and field grouping.
- Forbidden future changes:
  - Trigger network fetch or apply registry changes from this dialog.
- Tests needed:
  - Existing `RemoteSourcePresetEditorViewModelTests` remain primary.
  - XAML boundary for required fields and OK/Cancel.

### 5.8 Apply / Rollback / Destructive Confirmations

- Current files:
  - `FieldRegistryManagerWindow.xaml.cs`
  - `FieldRegistryHarvestPreviewWindow.xaml.cs`
  - `FieldLearningWizardWindow.xaml.cs`
  - Confirmation view models are in `FieldRegistryHarvestPreviewViewModel.cs` and `FieldRegistryManagerViewModel`.
- Current behavior:
  - MessageBox confirmation for cleanup apply, rollback selected, import apply, learning apply, clear remote history, remove remote preset.
  - Owner is the current window.
- Problems:
  - MessageBox gives little room for risk details, target file, backup manifest, and operation counts.
- Proposed layout:
  - Future dedicated confirmation dialog for high-risk apply/rollback only.
  - Keep low-risk clear/remove confirmations as MessageBox unless user approves broader consistency work.
- Allowed future changes:
  - Add small confirmation dialogs with summary and risk list.
- Forbidden future changes:
  - Remove explicit confirmation.
  - Auto-apply after build plan.
  - Change backup manifest behavior.
- Tests needed:
  - Boundary tests that high-risk apply/rollback still require confirmation.
  - Tests for target scope and operation count in confirmation view models.

## 6. Redesign Order

Recommended order:

1. A15-2B: Field Registry Center / Manager read-only management layout.
2. A15-2C: Field Import Preview workflow layout.
3. A15-2D: Field Editor / Allowed Values Editor layout.
4. A15-2E: Field Learning Wizard workflow layout.
5. A15-2F: Apply / Rollback confirmation consistency.

Rationale:

- Center / Manager define the mental model for Project / Global / BuiltIn and must be clarified first.
- Import Preview is the densest write-capable workflow and benefits from that management vocabulary.
- Field Editor and Allowed Values Editor can then inherit target/validation language.
- Learning Wizard shares the import/apply plan model and should follow after the import workflow is stable.
- Confirmation consistency should come last so the exact risks and summaries are known.

## 7. Non-goals

A15-2A and future layout phases must not:

- Modify parser semantics.
- Modify Field Registry resolution or Project > Global > BuiltIn priority.
- Modify import preview diff semantics.
- Modify apply / rollback behavior.
- Modify backup manifest behavior.
- Modify field learning behavior.
- Modify diagnostics, completion, hover, quick peek, save preflight, undo / redo.
- Restore `RA2IniEditor.sln`, `RA2IniEditor.csproj`, legacy `MainWindow`, legacy table-style editor, or legacy object workbench.
- Convert focused DataGrid editors into the removed legacy object/table editor.
- Add automatic network fetch or automatic registry apply.

## 8. Risk Matrix

| Area | Risk | Why | Mitigation |
| --- | --- | --- | --- |
| Field Registry Center / Manager | High | Central management surface; touches reload, learning, advanced tools, cleanup, rollback | Start with layout-only contract; preserve command handlers and ViewModel semantics |
| Import Preview | High | Fetch, diff, plan, apply, preset/history all converge here | Step-based redesign; test no auto-apply and explicit target scope |
| Field Editor | High | Writes Project/Global registry files | Keep preview-before-save; preserve apply service and backup behavior |
| Field Learning Wizard | High | Generates and applies learned fields | Preserve parser/generalization/apply plan; make source and target explicit |
| Apply / Rollback confirmations | High | Destructive or state-restoring actions | Dedicated confirmation only after summary fields are agreed |
| Allowed Values Editor | Medium | Edits parent preview row, not registry directly | Keep modal scope narrow; no direct writes |
| Remote Preset Editor | Medium | Persists preset edits through parent workflow | Keep local-only wording and validation |
| Open folder / reload actions | Medium | Reload affects runtime provider; folder launch touches OS shell | Keep explicit buttons; show disabled reasons |

## 9. Acceptance Criteria

For any future implementation phase:

- Scope is approved before code changes.
- Modified files are limited to approved UI surface files and focused tests.
- Main Shell layout is not changed unless explicitly approved.
- Existing command handlers and ViewModel semantics remain intact.
- Project / Global / BuiltIn priority remains unchanged and visible.
- Write-capable actions still require explicit preview/confirmation where they do today.
- No legacy project or table-style editor is restored.
- `dotnet build`, `dotnet test`, and `IdeOnly` package pass unless the user approved a narrower validation set.
- Manual smoke includes opening the affected surface, using disabled states, and confirming no unintended writes.

## 10. Validation

A15-2A validation scope:

- Documentation-only change: this file.
- Do not modify XAML, code-behind, ViewModels, tests, package scripts, BuiltIn definitions, solution/project files, or behavior.
- Required commands:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```
