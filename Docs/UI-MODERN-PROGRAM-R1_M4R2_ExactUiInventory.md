# UI-MODERN-PROGRAM-R1 M4-R2-0 — Field Registry Exact UI Inventory

Status: completed read-only inventory; implementation not started  
Date: 2026-07-23  
Authority reviewed:

- `AGENTS.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md`
- `Docs/UI-MODERN-PROGRAM-R1_M4_FieldRegistryExactUiInventory.md`
- `Docs/UI-MODERN-PROGRAM-R1_M4_StageLedger.md`
- `Docs/FieldRegistrySurfacesUiContract.md`

Companion proposed contract:

- `Docs/UI-MODERN-PROGRAM-R1_M4R2_FieldRegistryVisualConvergenceContract.md`

## 1. Inventory conclusion

The M4 behavioral, lifecycle, automation, virtualization and build baseline remains trustworthy. Its visual acceptance is not sufficient for the current user requirement.

The current implementation has a coherent token and keyed-style foundation, but the Field Registry family still presents as a collection of WPF management forms:

- Center uses a fixed `156 / * / 300` workspace at a `1040 x 700` default.
- The Center list is a minimally rebased DataGrid and the empty details pane still renders property labels with empty values.
- “source” is used for pack scope, active pack count and definition provenance without enough distinction.
- Center shows an effective `SectionKind + Key` row count as “fields”, while the active pack card shows physical local definition count.
- Manager retains six DataGrids; Harvest retains ten; Learning retains four.
- Field Editor remains a long sequence of bordered form sections.
- Allowed Values remains one bordered DataGrid plus nine text commands.
- The prior WPF dimension map describes compact-detail collapse, but no current XAML trigger, visual state, converter or code-behind responsive path implements it.

M4-R2 can remain presentation-only if it:

- preserves the current provider, ViewModel and service paths;
- reuses `Ra2FieldDetailsViewModel` and `Ra2FieldTrustClassifier`;
- uses additive, surface-scoped styles rather than mutating frozen shared keys;
- preserves all existing handler names, bindings, selection state, Owner and Show/ShowDialog behavior;
- retains DataGrid virtualization and recycling.

## 2. Current visual evidence

| Artifact | Size | SHA-256 | Current interpretation |
|---|---:|---|---|
| `Docs/UiVisualBaselines/UI-FR-Light.png` | 1672 x 941 px | `10D9979917F4F4235C069334DB30B15771D39659FD4AC3777B5081F2B378E687` | Visual direction only: hierarchy, icon language, details Inspector and whitespace |
| `Docs/UiVisualBaselines/UI-FR-WpfDimensions.png` | 1600 x 1000 px | `294B50CC4E23EDA777BC5B9B3973288F2D6D50D0A4595514B2E83D87AEB8D643` | Historical dimension map; compact-collapse statement is not implemented and is superseded by the proposed R2 contract |
| `artifacts/M4-RegistryCenter-Default.png` | 1040 x 700 px | `2E31899438FD126805149DCE567ADA61ECBDBEB17366E5B97E2C5757FFB5E854` | Trusted current default-state evidence, not the final visual target |
| `artifacts/M4-RegistryCenter-FieldSelected.png` | 1040 x 700 px | `C1E50E7B4AEB020E20E55CB4AB679DAECFE6B391BF83C40349F5CE0501810B03` | Trusted current selected-state evidence, not the final visual target |
| `artifacts/M4-RegistryManager-Rollback.png` | 1120 x 880 px | `8E67B68D0D665E9814740482E538390E640495A9169A3D720715766498B79A36` | Confirms large blank rollback workspace and dense horizontal table |
| `artifacts/M4-Harvest-DiffReview.png` | 1040 x 720 px | `996B1AD7BEE9850CA44652E65F43AE43173143A298F64301E5ECAD74B407B93D` | Confirms dense form bands and tabular workflow |

The user-supplied current screenshot matches the M4 Center evidence closely. The issue is therefore the implemented composition, not a stale binary or isolated scaling defect.

## 3. Surface geometry and structural density

| Surface | Current geometry | AutomationIds | DataGrids | Buttons | Borders | Lifecycle |
|---|---|---:|---:|---:|---:|---|
| Field Registry Center | 1040 x 700; min 820 x 620; custom chrome | 33 | 2 | 6 | 20 | Shell-owned non-modal; child editor reused until closed |
| Field Registry Manager | 1120 x 880; min 920 x 720; custom chrome | 57 | 6 | 13 | 17 | Shell-owned non-modal; destructive operations remain confirmed |
| Field Import Preview | 1040 x 720; min 820 x 650; native chrome | 64 | 10 | 20 | 2 | Shell-owned non-modal; async fetch and apply confirmation |
| Field Learning Wizard | 1020 x 720; min 820 x 620; custom chrome | 36 | 4 | 6 | 12 | Shell-owned non-modal; Allowed Values is modal child |
| Field Editor | 900 x 780; min 760 x 660; custom chrome | 36 | 1 | 11 | 6 | Center-owned non-modal; preview-before-save path |
| Allowed Values Editor | 820 x 560; min 660 x 440; custom chrome | 13 | 1 | 9 | 2 | Learning-owned modal; returns draft text only |
| Remote Preset Editor | 540 x 360; min 460 x 320; native chrome | 9 | 0 | 2 | 2 | Harvest-owned modal; returns local edit model |
| Add Property | 880 x 640; min 720 x 520; native chrome | 19 | 1 | existing focused commands | scoped | Shell/editor-owned modal |
| Field Annotation Editor | 620 x 500; min 520 x 420; native chrome | 7 | 0 | existing focused commands | scoped | Add Property-owned modal; dirty-close path |

Structural counts are diagnostic rather than targets. M4-R2 must reduce competing frames and simultaneous tables without removing required data or changing workflow state.

## 4. Center data facts

### 4.1 Effective row count

`FieldRegistryCenterWindow.RefreshFieldRows` iterates every `Ra2SectionKind`, calls the existing provider for that section, and de-duplicates on:

```text
SectionKind + U+001F + Definition.Key
```

Therefore `FieldCountText` reports effective browse rows/mappings, not unique physical registry definitions.

The active pack count (for example Global `1906`) describes definitions in the loaded local pack. A value such as `44967` describes effective Section/Key browse rows after provider composition. No provider defect is inferred.

M4-R2 may change only the presentation units:

- `n 个字段` -> `n 条有效映射`
- `显示 n / total 个字段` -> `显示 n / total 条有效映射`

It may not change enumeration, de-duplication, ordering, filtering predicate or provider priority.

### 4.2 Scope and provenance vocabulary

These concepts are distinct:

| Concept | Existing facts | Required label family |
|---|---|---|
| Registry scope / authority | Project, Global, BuiltIn | 字段包范围 / 生效优先级 |
| Active local packs | Manager `Packs` | 活跃字段包 |
| Definition provenance | User, Yuri, Ares, Phobos, BuiltIn, External | 定义来源 |
| Effective browse mapping | SectionKind + Key row | 有效映射 |

M4-R2 may relabel these concepts but may not merge or reinterpret them.

### 4.3 Selected-field details reuse

The current Center row exposes only:

- `Key`
- `SectionKind`
- `EditorKind`
- `ValueKind`
- `SourceKind`
- `Description`

The existing internal `Ra2FieldDetailsViewModel.FromDefinition` already provides:

- display name and key;
- Section display;
- formatted source;
- editor/value kinds;
- trust label and trust detail through `Ra2FieldTrustClassifier`;
- description;
- examples;
- allowed values.

The safe reuse path is one internal `Details` property on `FieldRegistryCenterFieldRow`, created from the already-owned definition. No new provider query, service, DTO or public API is required.

## 5. Frozen lifecycle and handler boundaries

| Surface | Frozen behavior |
|---|---|
| Center | Reload, learning, new/edit, advanced tools, filter TextChanged and double-click handlers remain; `FieldsGrid` selection remains edit authority |
| Manager | Reload/import/relearn/folder actions, cleanup plan/apply, rollback refresh/open/execute and MessageBox confirmations remain |
| Harvest | Insert/parse/current INI/clear, async fetch/cancel, history/preset actions, plan/apply and confirmations remain |
| Learning | Current INI, pasted parse, allowed-values dialog, plan/apply and confirmation remain |
| Field Editor | Project/Global preview, Project/Global save, copy/open path and close handlers remain |
| Allowed Values | Add/remove/dedupe/sort/append/restore, OK/Cancel and DialogResult rules remain |
| Remote Preset | Validation, EditModel return and DialogResult rules remain |
| Add Property / Annotation | Insert, duplicate handling, annotation dirty/save/close and keyboard behavior remain |

Shell remains the owner of the existing non-modal windows and event subscriptions. `ShellWindow.xaml` and `ShellWindow.xaml.cs` are read-only for M4-R2.

## 6. Automation contract

All currently present AutomationIds are frozen. Current counts are:

- Center: 33
- Manager: 57
- Harvest: 64
- Learning: 36
- Field Editor: 36
- Allowed Values: 13
- Remote Preset: 9
- Add Property: 19
- Annotation: 7

Every changed surface must pass a before/after source-set comparison. Existing IDs may move in the visual tree but may not be renamed, duplicated or removed.

Authorized additive landmarks are:

```text
FieldRegistryCenter.Details.EmptyState
FieldRegistryCenter.Details.Content
FieldRegistryCenter.ScopeSummary
FieldRegistryManager.RollbackDetails
FieldRegistryManager.CleanupDetails
FieldImportPreview.WorkflowStepStrip
FieldImportPreview.SourceArea
FieldImportPreview.ReviewArea
FieldImportPreview.PlanArea
FieldLearningWizard.ReviewArea
FieldEditor.MetadataColumn
FieldEditor.DocumentationColumn
FieldEditor.ActionFooter
AllowedValuesEditor.Toolbar
AllowedValuesEditor.ValidationSummary
```

No AutomationId is placed inside a repeated DataTemplate unless it is already present and covered by the existing boundary contract.

## 7. Shared style reference graph

The following accepted keys have broad consumers and must not be mutated in place during M4-R2:

| Key | Production XAML consumers |
|---|---:|
| `IdeFieldRegistryRootStyle` | 8 |
| `IdeFieldRegistryCommandButtonStyle` | 9 |
| `IdeFieldRegistryAccentButtonStyle` | 7 |
| `IdeFieldRegistryDataGridStyle` | 7 |
| `IdeFieldRegistryFormInputStyle` | 6 |
| `IdeFieldRegistryFormSectionStyle` | 6 |
| `IdeFieldRegistryWriteBoundaryStyle` | 6 |
| `IdeFieldRegistryFormComboBoxStyle` | 4 |
| `IdeFieldRegistryStatusCardStyle` | 4 |
| `IdeFieldRegistryTabControlStyle` | 3 |
| `IdeFieldRegistryTabItemStyle` | 3 |

Narrow keys such as `IdeFieldRegistryNavigationPaneStyle` and `IdeFieldRegistrySearchBoxStyle` currently have only the Center as a consumer, but they are still accepted M4 keys. M4-R2 uses new surface-scoped derived keys instead of rewriting their definitions.

`IdeFieldRegistryWorkflowBandStyle` currently has zero production references. It remains protected until the M4-R2 foundation card explicitly decides whether to adopt it unchanged or leave it dormant; it is not deleted as incidental cleanup.

## 8. Performance and collection boundary

All DataGrid-based large collections must retain:

```text
ScrollViewer.CanContentScroll=True
EnableRowVirtualization=True
EnableColumnVirtualization=True
VirtualizingPanel.IsVirtualizing=True
VirtualizingPanel.VirtualizationMode=Recycling
```

Forbidden:

- replacing large grids with StackPanel/ItemsControl card lists;
- per-row shadows;
- converter-heavy icon lookup for every effective mapping;
- rebuilding provider definitions when selection changes;
- changing filter semantics as part of visual work.

The Center filter currently clears and repopulates the bound collection on every text change. M4-R2 records a performance smoke for this path but does not silently replace its collection contract. If the smoke fails, a separately bounded presentation-performance card is required.

## 9. Existing focused tests

Primary source/boundary tests:

- `IdeVisualSystemBoundaryTests`
- `Ra2FieldLearningWizardBoundaryTests`
- `Ra2FieldEditorWindowBoundaryTests`
- `FieldRegistryHarvestPreviewBoundaryTests`
- `FieldRegistryHarvestPreviewWindowApplyGuardrailTests`
- `FieldRegistryRollbackUiBoundaryTests`
- `Ra2AddPropertyUiBoundaryTests`
- `Ra2FieldAnnotationEditorUiBoundaryTests`
- `WpfAutomationHarnessBoundaryTests`
- `IconResourceBoundaryTests`

Behavioral tests for Manager, Harvest, rollback, editor save and preset ViewModels remain regression coverage and must not be weakened to accommodate presentation changes.

## 10. Read-only gate result

M4-R2 is feasible without:

- a new dependency;
- a project-file change;
- a Shell/Dock change;
- a new public API;
- a new responsive controller or converter;
- a provider, ViewModel or service semantic change;
- a registry write, migration or data rewrite.

The exact implementation boundary is defined in the companion proposed contract. Production implementation remains blocked until that contract is explicitly confirmed.
