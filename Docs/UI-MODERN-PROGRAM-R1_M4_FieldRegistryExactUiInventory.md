# UI-MODERN-PROGRAM-R1 — M4 Field Registry Exact UI Inventory

Status: Completed read-only gate  
Date: 2026-07-23  
Authority: `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md` Revision A  
Prior surface contract: `Docs/FieldRegistrySurfacesUiContract.md`

## 1. Gate result

M4 can proceed as presentation-only work. The existing windows already expose stable bindings, handlers and AutomationIds; no ViewModel, service, public API, registry authority or write-path change is required to reach the approved visual hierarchy.

Current implementation goal: introduce `Themes/IdeFieldRegistryStyles.xaml`, then modernize the Field Registry family in cards M4-A through M4-G. Each card is limited to five files and retains the existing lifetime and behavioral wiring.

Approval state: the user confirmed continuous execution of Revision A and explicitly waived per-card approval waits. A new approval is required only if a stop condition is reached.

## 2. Frozen semantic and lifecycle boundaries

- Provider priority remains `Project > Global > BuiltIn`.
- Reload, import diff, apply plan, backup manifest, rollback, cleanup and learning behavior remain unchanged.
- Completion, Hover, Quick Peek, Diagnostics, Save Preflight, parser/editor behavior and BuiltIn data remain unchanged.
- Existing `Click`, `TextChanged`, `PreviewKeyDown`, double-click and selection bindings remain attached to the same handler names.
- Existing Owner, `Show`/`ShowDialog`, `DialogResult`, dirty-close and child-window relationships remain unchanged.
- WindowStyle, ResizeMode, minimum dimensions and custom-chrome/system-command behavior remain unchanged per the program chrome matrix.
- No automatic fetch, parse, plan, save, apply, rollback or cleanup is introduced.

## 3. Resource and performance boundary

- New dictionary: `Themes/IdeFieldRegistryStyles.xaml`, merged after collection/workspace styles and before Shell-specific compatibility resources.
- It may reference semantic tokens and keyed base styles only. It may not define application-wide implicit control styles.
- All field, pack, rollback, diff, issue, plan and value grids continue through a DataGrid style based on `UiDataGridStyle`/the accepted collection layer.
- Required inherited settings: `ScrollViewer.CanContentScroll=True`, row and column virtualization enabled, `VirtualizingPanel.IsVirtualizing=True`, recycling mode.
- Large lists may not be replaced by non-virtualized `ItemsControl`/`StackPanel` collections. No per-row shadow or converter-heavy card template is allowed.

## 4. Exact surface inventory

| Card | Surface and DataContext owner | Lifetime / code-behind anchors | State and selection that must survive | Presentation-only path |
|---|---|---|---|---|
| M4-A | `FieldRegistryCenterWindow`; Window owns `Manager`, `FieldRows`, `FieldCountText` | Shell-owned non-modal Window; `FieldsGrid` is read by edit/double-click handlers; child `FieldEditorWindow` is reused until closed | Search filter text, selected field, field list scroll, active pack/status text | Recompose into 156/552/300 navigation/list/details workspace; keep `FieldsGrid` and all existing bindings/handlers |
| M4-B | `FieldRegistryManagerWindow`; external `FieldRegistryManagerViewModel` | Shell-owned non-modal Window; apply/rollback remain MessageBox-confirmed; folder/reload/relearn events remain unchanged | selected rollback manifest, cleanup Expander state, repair tabs, grid scroll | Flat maintenance hub with distinct status/read-only/write zones; no command or confirmation changes |
| M4-C | `FieldRegistryHarvestPreviewWindow`; owns `FieldRegistryHarvestPreviewViewModel` | Shell-owned non-modal Window; async fetch/cancel, preset dialogs, import/export dialogs and apply confirmation remain unchanged | main/advanced tab selection, advanced expansion, selected remote history/preset, current draft edits, grid scroll | Source -> review -> plan/result hierarchy with modern bands and scoped styles |
| M4-D | `FieldLearningWizardWindow`; owns injected `FieldRegistryHarvestPreviewViewModel` | Shell-owned non-modal custom-chrome Window; `AllowedValuesEditorWindow.ShowDialog()`; apply confirmation remains | source text, current draft edits, selected review tab, allowed-value edits | Clarify source/review/apply boundaries without changing shared ViewModel |
| M4-E | `FieldEditorWindow` and `AllowedValuesEditorWindow` | Center-owned non-modal editor; learning-owned modal allowed-values editor; existing DialogResult rules retained | current draft inputs, preview/result state, selected value row | Modern form sections, preview boundary and focused value editor |
| M4-F | `Ra2AddPropertyWindow` and `Ra2FieldAnnotationEditorWindow` | Shell-owned modal add dialog with Loaded focus/keyboard path; annotation dialog preserves dirty-close prompt | selected field, search/filter, duplicate action, value/option text, annotation dirty state | Adopt Field Registry visual vocabulary without changing insert/annotation behavior |
| M4-G | `RemoteSourcePresetEditorWindow`; owns `RemoteSourcePresetEditorViewModel` | Harvest-owned modal Window; OK validates and returns EditModel, Cancel returns false | entered name/URL/description/tags/enabled state | Compact local-preset editor using the shared field form/footer styles |

## 5. Frozen AutomationId groups

Every currently present ID is preserved. Key identity groups are listed below; the implementation must run a source-level before/after set comparison for each changed window.

### M4-A Center

`FieldRegistryCenter.Window`, `HeaderArea`, `HeaderChips`, `Toolbar`, `ActionGroup`, `PriorityStrip`, `PriorityChipProject`, `PriorityChipGlobal`, `PriorityChipBuiltIn`, `StatusSummaryPanel`, `ProjectStatusCard`, `GlobalStatusCard`, `BuiltInStatusCard`, `SearchArea`, `SearchTextBox`, `FieldCountChip`, `WarningSummary`, `ActivePacksPanel`, `ActivePacksCompactList`, `PacksGrid`, `MainFieldsPanel`, `FieldsGrid`, `StatusArea`, and all existing action-button IDs.

Additive anchors authorized by Revision A: `FieldRegistryCenter.Navigation`, `FieldRegistryCenter.FieldList`, `FieldRegistryCenter.Details`. They are presentation landmarks only.

### M4-B Manager

All `FieldRegistryManager.*` IDs currently present, including status chips/panels, packs, read/write action groups, rollback grid/actions/status, cleanup preview/repair tabs/grids/warnings and folder actions.

### M4-C Harvest

All `FieldImportPreview.*` IDs currently present, including source/fetch controls, `MainFlowTabs`, diff/issues/plan grids, target/apply controls, status/result paths, `AdvancedDetailsExpander`, remote history/preset controls and advanced grids.

### M4-D through M4-G

All existing `FieldLearningWizard.*`, `FieldEditor.*`, `AllowedValuesEditor.*`, `AddProperty.*`, `FieldAnnotationEditor.*` and `RemoteSourcePresetEditor.*` IDs remain frozen. No ID may be renamed to fit the new visual grouping.

## 6. Exact binding/handler boundaries by card

### M4-A

- Fields: `FieldRows`, `Key`, `SectionKind`, `EditorKind`, `ValueKind`, `SourceKind`, `Description`.
- Manager status: `Packs`, path/display/status/warning/count properties and source chips.
- Handlers: reload, learning, new/edit, advanced tools, search text change and field double-click.

### M4-B

- Packs/status, `SelectedRollbackManifest`, rollback capability/reason/path properties, `CleanupPlanRows`, `RepairPreview.*`, cleanup and warning summaries.
- Handlers: reload/import/relearn/folder actions, build/apply cleanup, refresh/open/execute rollback, close.

### M4-C / M4-D

- Source/fetch state; candidates/drafts/diff/issues; target scope/apply mode; plan/status/result paths; remote history/preset state.
- Learning reuses the same ViewModel contract and adds no adapter.
- All fetch, parse, clear, plan, apply, preset/history and allowed-values handlers are retained verbatim.

### M4-E through M4-G

- Field Editor retains all draft, option, preview, `CanSave`, result-path and issue bindings.
- Allowed Values retains editable `Rows`, `SelectedItem`, add/remove/dedupe/sort/append/restore and DialogResult paths.
- Add Property retains `FilteredItems`, `SelectedItem`, details, duplicate action, value/option/preview, confirmation and annotation bindings plus Loaded keyboard behavior.
- Annotation retains the ViewModel dirty/save contract and close prompt.
- Remote Preset retains validation and EditModel return semantics.

## 7. Current structural facts and correction targets

- Center and Manager already have custom chrome and many M1-era status cards, but both still wrap the complete client area in a second full border and arrange most content as vertically stacked bordered panels. M4 removes duplicate region framing and establishes a primary workspace hierarchy.
- Center currently uses a two-column packs/fields region rather than the approved 156/552/300 navigation/list/details architecture. The selected-field details column can be presentation-only by binding directly to `FieldsGrid.SelectedItem`; no new state type is required unless a later implementation fact proves otherwise.
- Harvest is still native-window chrome and its entire 1040 × 720 composition is compressed into dense form bands and nested tabs. M4-C may modernize the client only; WindowStyle/lifecycle remain frozen.
- Field Editor uses long stacked bordered forms; Allowed Values is focused but still framed as a bordered DataGrid form.
- Add Property and Annotation have not yet adopted the shared modern Field Registry vocabulary.
- `IdeSecondaryDataGridStyle` currently rebases to the accepted tool-window DataGrid and retains virtualization. M4 introduces a new Field Registry grid style and adopts it explicitly; compatibility aliases are not deleted until M6 zero-reference audit.

## 8. Allowed files and card budget

- M4 foundation: `App.xaml`, new `Themes/IdeFieldRegistryStyles.xaml`, one focused visual-boundary test file.
- Each surface card: only its listed XAML surface(s), the shared Field Registry dictionary when additive keys are necessary, and focused test files; no more than five files total.
- Code-behind and ViewModels are read-only unless a build-only reference correction is required. Any behavioral code requirement is a stop condition.

Forbidden: Core/Infrastructure providers and services, Field Registry ViewModels, BuiltIn JSON, project/solution files, Shell/Dock files, parser/editor/AI code, package scripts and legacy files.

## 9. Validation matrix

Per card:

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "<focused boundary classes>"
```

M4 package gate:

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly -PackageName RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4.Rollback.zip
```

Required visual evidence remains `M4-RegistryCenter-Default.png`, `M4-RegistryCenter-FieldSelected.png`, `M4-RegistryManager-Rollback.png`, and `M4-Harvest-DiffReview.png`. Evidence not captured from a real WPF state is recorded as NotRun, not inferred from build success.

## 10. Stop conditions

Stop the continuous package if implementation requires a public API/ViewModel/service change, registry state write or authority change, different confirmation/lifecycle behavior, new persistence/dependency/project-file change, loss of virtualization, removal/weakening of behavior assertions, or more than five files in one card.

