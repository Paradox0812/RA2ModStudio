# UI-MODERN-PROGRAM-R1 M4-R2 — Field Registry Visual Convergence Contract

Status: proposed final contract; awaiting explicit user confirmation  
Date: 2026-07-23  
Risk direction: project-level R3 presentation boundary already authorized by UI-MODERN-PROGRAM-R1; every implementation card must remain R1  
Governance mode: Deferred during implementation; flush at visual gates, failure stop and package completion  
Exact current inventory: `Docs/UI-MODERN-PROGRAM-R1_M4R2_ExactUiInventory.md`

## 1. Authority and succession

After confirmation, this contract supersedes only the visual-acceptance and responsive-composition claims of:

- `Docs/UI-MODERN-PROGRAM-R1_M4_FieldRegistryExactUiInventory.md`
- `Docs/UI-MODERN-PROGRAM-R1_M4_StageLedger.md`
- `Docs/UiVisualBaselines/UI-FR-WpfDimensions.png`

It does not invalidate the trusted M4 behavior, handler, binding, automation, lifetime, virtualization, build, test or rollback evidence.

Historical M4 screenshots remain trusted evidence of the current implementation, but are no longer final visual targets.

Authority order:

1. Current user instruction and this confirmed M4-R2 contract.
2. `UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md`.
3. M4-R2 exact inventory.
4. M4 behavioral/lifecycle inventory and stage ledger.
5. `FieldRegistrySurfacesUiContract.md`.
6. Older illustrative dimensions and screenshots.

## 2. Goal

Converge the complete Field Registry family toward the accepted light IDE direction while preserving WPF performance and every registry semantic:

- make the Center a field-browsing workspace rather than a database form;
- give the virtualized field list a flat IDE hierarchy;
- turn the right pane into a real read-only Inspector with an explicit empty state;
- distinguish registry scope, active packs, definition provenance and effective mappings;
- modernize Manager, import, learning, field editing and value editing as task-oriented IDE workspaces;
- reduce nested borders, simultaneous tables and oversized text-command strips;
- provide reliable 1920 x 1080 primary geometry and bounded compact behavior;
- use project-owned XAML Geometry icons only.

## 3. Non-goals

M4-R2 must not:

- change Project > Global > BuiltIn priority;
- change provider enumeration, matching, fallback, enrichment or provenance;
- change Center filter matching, ordering or edit selection semantics;
- change import parse/diff/plan/apply behavior;
- change cleanup, rollback, backup manifest or confirmation behavior;
- change learning/generalization behavior;
- change Field Editor preview/save behavior;
- change Allowed Values result text or DialogResult behavior;
- change remote fetch/history/preset persistence;
- add automatic fetch, parse, plan, save, apply, cleanup or rollback;
- replace MessageBox confirmations with new dialogs;
- modify Completion, Hover, Quick Peek, Diagnostics, Save Preflight, parser/editor, AI or BuiltIn data;
- change Shell/Dock, ContentIds, layout persistence or floating-host behavior;
- add dark theme, a theme selector, a dependency or project-file change;
- restore any legacy editor.

## 4. Architecture and reuse contract

### 4.1 Canonical presentation reuse

M4-R2 reuses:

- `IdeVisualTokens.xaml` for typography, color, density and geometry;
- `UiButtonStyle`, `UiTextBoxStyle`, `UiComboBoxStyle` and `UiDataGridStyle`;
- `IdeFieldRegistryStyles.xaml` as the domain dictionary;
- `Ra2FieldDetailsViewModel.FromDefinition` for selected-field detail projection;
- `Ra2FieldTrustClassifier` through that ViewModel;
- existing Geometry resources and the M4-R2 additive Geometry set;
- all current DataContexts, bindings and handler names.

No parallel field-details model, registry adapter, service or provider is authorized.

### 4.2 Frozen-style rule

Existing accepted `IdeFieldRegistry*` keys are read-only during M4-R2. New styles must:

- use a surface-specific prefix;
- be `BasedOn` an existing accepted key where applicable;
- reference semantic resources instead of raw palette values;
- have no application-wide implicit target;
- be adopted explicitly by approved XAML files.

Authorized prefixes:

```text
IdeFieldRegistryR2*
IdeFieldRegistryCenterR2*
IdeFieldRegistryManagerR2*
IdeFieldRegistryWorkflowR2*
IdeFieldRegistryEditorR2*
IdeFieldRegistryCompactR2*
```

Maximum new style keys per implementation card: 8. The foundation card may additionally add only the 13 explicitly authorized Geometry keys below. A card requiring more styles must stop and split.

### 4.3 Icon contract

The foundation card may add project-owned Geometry keys under:

```text
IconGeometry.FieldRegistry.*
```

Authorized concepts:

```text
Search
Filter
Refresh
Add
Edit
Learn
Project
Global
BuiltIn
Import
History
Rollback
Copy
```

No raster assets, image-generation API, Emoji, Segoe MDL character glyphs or external icon package is authorized.

## 5. Data and wording contract

The Center must present:

- Project / Global / BuiltIn as `字段包范围` and `生效优先级`;
- loaded packs as `活跃字段包`;
- User / Yuri / Ares / Phobos / BuiltIn / External as `定义来源`;
- Center browse rows as `有效映射`.

The existing public binding property name `FieldCountText` remains unchanged. Only its generated Chinese unit text changes:

```text
n 条有效映射
显示 n / total 条有效映射
```

No count is recomputed differently.

The existing Manager presentation strings may be placed differently but are not reinterpreted. Any wording adjustment is limited to the terminology table above.

## 6. Exact geometry

All values are WPF device-independent pixels.

### 6.1 Field Registry Center

| Item | Contract |
|---|---:|
| Default window | 1280 x 720 |
| Minimum window | 820 x 620 |
| Custom chrome | 52 |
| Primary command/search band | 40 |
| Bottom status | 24 |
| Field row | 28 |
| Field header | 30 |
| Details section header | 30–32 |
| Details property row minimum | 26 |

Center columns use XAML-only clamped proportions:

| Column | Width | Min | Max |
|---|---:|---:|---:|
| scope/navigation | `18*` | 148 | 184 |
| field list | `52*` | 400 | unbounded |
| details Inspector | `30*` | 240 | 320 |

This replaces the unimplemented historical “collapse details below 960” statement. The Inspector remains reachable at every supported width, and no responsive controller, converter, timer or window-size event is introduced.

Expected approximate widths:

| Window width | Scope | List | Details |
|---:|---:|---:|---:|
| 1280 | 184 | 776 | 320 |
| 1040 | 184 | 544 | 312 |
| 820 | 148 | 426 | 246 |

Tolerance: ±6 DIP per workspace column after borders and layout rounding.

### 6.2 Other surfaces

| Surface | Default | Minimum | Composition rule |
|---|---:|---:|---|
| Manager | 1180 x 720 | 920 x 620 | status/rollback/cleanup tabs; rollback uses list + selected details |
| Import Preview | 1180 x 720 | 820 x 650 | source band + step strip + one main review surface + write boundary |
| Learning | 1180 x 720 | 820 x 620 | source + review + apply boundary; one review tab visible at a time |
| Field Editor | 960 x 720 | 760 x 620 | two-column editor above a sticky action footer; vertical scroll allowed |
| Allowed Values | 840 x 560 | 660 x 440 | one virtualized value list, compact command row, validation/footer |
| Remote Preset | 540 x 360 | 460 x 320 | retain compact native-modal structure |
| Add Property | 960 x 680 | 720 x 520 | virtualized results + details + focused insertion footer |
| Annotation | 640 x 520 | 520 x 420 | compact editor + explicit save boundary |

No default window may exceed 720 DIP height. WrapPanel or star sizing must absorb narrow-width command pressure; text buttons may not be given oversized uniform widths merely to align them.

## 7. Center exact composition

### 7.1 Header

- Keep the existing custom chrome, title, subtitle, header chips and close action.
- Chips use short status language and may not carry long paths.
- No second application title is added.

### 7.2 Command/search band

Order:

1. New Field — primary visual command.
2. Edit Selected.
3. Learn Fields.
4. Reload.
5. Advanced Tools — secondary command.
6. Search occupies remaining width.
7. Effective mapping count and warning summary remain visible without overlapping search.

All existing button IDs and handlers remain. Icon + text is allowed; icon-only use is allowed only with Automation Name and ToolTip.

### 7.3 Scope pane

- Replace stacked bordered cards with flat Project, Global and BuiltIn scope rows.
- Keep the visible priority order 1 / 2 / 3.
- Show short status/path text with ellipsis and full path ToolTip.
- Keep the active-packs grid virtualized, but use the compact R2 grid style and the label `活跃字段包`.
- No fake Overview/Fields/Sources navigation items are introduced.

### 7.4 Field list

- Retain `FieldsGrid`, its ItemsSource, selected-item authority and double-click handler.
- Retain DataGrid virtualization/recycling.
- Remove vertical cell borders and heavy header framing through an R2-specific grid/cell/header style.
- Preserve column resize hit targets.
- Use weak horizontal row separators, explicit hover, selected background and keyboard focus.
- Do not implement card rows or row shadows.
- Column semantics:
  - `Key`
  - `Section`
  - `编辑方式`
  - `定义来源`
  - `说明`
- `ValueKind` remains available in the Inspector; adding it as another fixed list column is not required.

### 7.5 Details Inspector

The selected row may add exactly one internal property:

```csharp
public Ra2FieldDetailsViewModel Details { get; }
```

The class remains internal. The property is populated once from the existing definition using `FromDefinition`.

No selection:

- show `FieldRegistryCenter.Details.EmptyState`;
- hide empty property labels;
- show a short instruction and no fake values.

Selected:

- show key/display name;
- show trust label/detail;
- show definition source;
- show Section, editor kind and value kind;
- show description;
- show examples and allowed values only when present;
- keep existing current-status summary visually subordinate.

No edit, save or registry command is added inside the Inspector in M4-R2.

## 8. Manager exact composition

### Status tab

- Preserve the three scope summaries.
- Replace framed status cards with flat scope bands.
- Active packs and warnings remain separate, virtualized/read-only surfaces.
- Empty warnings use an explicit empty state rather than an empty framed list.

### Rollback tab

- Keep the current manifest DataGrid and `SelectedRollbackManifest`.
- Reduce the primary list to scan columns: scope, state, timestamp, target and add/update/skip counts.
- Move mode, manifest path, backup state and status message to `FieldRegistryManager.RollbackDetails`.
- Keep refresh/open/rollback handlers and enabled states.
- Keep MessageBox confirmation.
- No manifest property or ViewModel change is authorized.

### Cleanup tab

- Keep existing plan, repair preview and write warning bindings.
- Make plan summary/read-only preview the primary hierarchy.
- Keep repair sub-tabs only for detailed tables.
- `FieldRegistryManager.CleanupDetails` hosts selected/summary information already present in bindings.
- Keep build/apply handlers and MessageBox confirmation.

## 9. Workflow surfaces

### 9.1 Import Preview

Hierarchy:

```text
Source -> Review -> Plan -> Confirm/Result
```

- Add a non-interactive `WorkflowStepStrip`; it reflects hierarchy only and does not become a navigation state owner.
- Source name, source text/URL and fetch controls stay in `SourceArea`.
- MainFlowTabs remain the state owner for diff, plan and issues.
- Target scope, apply mode, target path and plan summary remain in `PlanArea`.
- Advanced history/preset/draft/raw tabs remain inside the existing advanced Expander.
- Fetch/cancel, parse, build and apply state remain unchanged.
- Long URLs and paths use ellipsis or wrapping without expanding command rows.

### 9.2 Learning

- Reuse the same visual vocabulary but keep its simpler source set.
- Keep `WorkflowStepStrip`, source, review tabs and apply boundary.
- Preserve editable draft rows and modal Allowed Values path.
- Make validation severity graphical using existing Issue Geometry while preserving text severity.
- Do not add remote/history controls.

## 10. Editor surfaces

### 10.1 Field Editor

- Reflow current controls into `MetadataColumn` and `DocumentationColumn`.
- Metadata: Key, Section, editor kind, value kind and conditional value-format fields.
- Documentation: display name, aliases, allowed values and description.
- Preview issues and JSON preview remain below/alongside the edit area but are visually secondary until generated.
- Keep separate Project/Global preview and save commands; no target selector is introduced because it would change the existing command contract.
- Keep target and manifest result paths.
- `ActionFooter` remains visible after scrolling and preserves current CanSave/IsCancel behavior.

### 10.2 Allowed Values

- Keep one editable virtualized DataGrid.
- Group Add/Remove as row commands and Dedupe/Sort/Append/Restore as normalization commands.
- Use `Toolbar` and `ValidationSummary`.
- No overflow menu or new command routing is added.
- OK/Cancel and `ResultText` behavior remain unchanged.

### 10.3 Remote Preset

- Retain native modal chrome and current 540 x 360 geometry.
- Use compact R2 labels, inputs, validation and footer.
- Explicitly label the result as a local preset edit.
- No fetch or apply action is added.

### 10.4 Add Property and Annotation

These surfaces receive a consistency pass only:

- use the same flat list, Inspector, input and write-boundary vocabulary;
- preserve all existing bindings, handlers, keyboard behavior and dirty-close behavior;
- do not merge them into Center or Field Editor;
- do not alter add/duplicate/annotation semantics.

## 11. State acceptance matrix

| Surface | Required states |
|---|---|
| Center | no selection; selected verified field; selected inferred/guardrail field; no Project pack; warning present; filter no-result; long path; long description |
| Manager | status with/without project; warnings empty/populated; rollback empty; Ready selected; non-Ready selected; cleanup unbuilt; cleanup preview with warnings |
| Import | empty source; parsed sample diff; validation errors/warnings; plan disabled; plan built; fetch busy/cancellable; advanced collapsed/expanded; long URL |
| Learning | current INI source; pasted source; empty drafts; populated drafts; issues; plan disabled/built; allowed-values round trip |
| Field Editor | new field; existing field; conditional Boolean/list inputs; invalid preview; valid preview; saved target/manifest paths; long text |
| Allowed Values | empty; populated; selected row; duplicate normalization; restored scanned values; OK/Cancel |
| Remote Preset | invalid; valid; enabled/disabled; long URL/description |
| Add/Annotation | no selection; selected details; duplicate warning; dirty annotation; disabled/valid save |

Visual acceptance of only the default state is insufficient.

## 12. Accessibility, keyboard and UI Automation

- Preserve every existing AutomationId and handler.
- Run before/after AutomationId, Binding and Click-handler set comparisons for every changed XAML file.
- All icon-only commands require Automation Name and ToolTip.
- Tab order follows visual order.
- DataGrid arrow, PageUp/PageDown, Home/End and Enter/double-click paths remain.
- Escape/IsCancel behavior remains for modal editors.
- Visible focus must not be removed.
- Severity and trust remain textual as well as graphical.
- No new AutomationPeer or child-HWND accessibility workaround is part of M4-R2.

## 13. StagePackage execution plan

### M4-R2-0 — Exact inventory and final contract

Allowed files:

- `Docs/UI-MODERN-PROGRAM-R1_M4R2_ExactUiInventory.md`
- `Docs/UI-MODERN-PROGRAM-R1_M4R2_FieldRegistryVisualConvergenceContract.md`

Result: documentation gate only. Stop for explicit confirmation.

### M4-R2-P — Pre-change rollback anchor

Actions:

- generate `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.PreChange.Rollback.zip`;
- record entry count, bytes and SHA-256;
- verify no forbidden package entries.

No production file changes.

### M4-R2-F — Additive visual foundation

Allowed files:

- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Themes/IconGeometryResources.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/IconResourceBoundaryTests.cs`

No production surface adopts an R2 key in this card.

### M4-R2-A — Center

Allowed files:

- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml.cs`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`

Code-behind allowance is limited to:

- the internal `Details` projection;
- effective-mapping wording;
- required using directives.

### M4-R2-B — Manager

Allowed files:

- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/FieldRegistryRollbackUiBoundaryTests.cs`

Code-behind and ViewModel are read-only.

### M4-R2-C — Import Preview

Allowed files:

- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewWindowApplyGuardrailTests.cs`

Code-behind and ViewModel are read-only.

### M4-R2-D — Learning

Allowed files:

- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs`

Code-behind and ViewModel are read-only.

### M4-R2-E — Field Editor

Allowed files:

- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldEditorWindowBoundaryTests.cs`

Code-behind and ViewModel are read-only.

### M4-R2-F2 — Allowed Values and Remote Preset

Allowed files:

- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml`
- `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs`

Code-behind and ViewModels are read-only.

### M4-R2-G — Add Property and Annotation consistency

Allowed files:

- `RA2IniEditor.IDE/Views/FieldBrowser/Ra2AddPropertyWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldAnnotations/Ra2FieldAnnotationEditorWindow.xaml`
- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2AddPropertyUiBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldAnnotationEditorUiBoundaryTests.cs`

Code-behind and ViewModels are read-only.

### M4-R2-V — Verification and visual gate

Required evidence:

```text
M4R2-RegistryCenter-1280x720-Default.png
M4R2-RegistryCenter-1280x720-Selected.png
M4R2-RegistryCenter-820x620-Compact.png
M4R2-RegistryManager-RollbackSelected.png
M4R2-Harvest-DiffReview.png
M4R2-Learning-DraftsAndIssues.png
M4R2-FieldEditor-ValidPreview.png
M4R2-AllowedValues-Populated.png
```

Prefer existing process/UIA automation and user-supplied screenshots. General computer control is not required. If a real state cannot be captured, mark it NotRun rather than inferring visual success.

### M4-R2-CLOSE — Governance and clean package

Update:

- `Docs/UI-MODERN-PROGRAM-R1_M4R2_StageLedger.md`;
- `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md`;
- `Docs/Codex_CurrentPhase.md`;
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`;

Create `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.Final.zip` and a screenshot index in the M4-R2 stage ledger.

No product-facing document is expected to require change because behavior remains unchanged. If implementation reveals a user-guide wording obligation, stop and schedule a separate documentation card instead of exceeding the five-file close budget.

## 14. Verification

Per card:

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "<focused classes>"
```

Package gate:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Additional static gates:

- all changed XAML parses;
- every referenced resource key exists;
- no duplicate resource key;
- frozen AutomationId/Binding/handler sets are preserved;
- no production `IdeSecondary*` reference returns;
- no forbidden file appears in the diff;
- DataGrid virtualization properties remain reachable through BasedOn chains;
- no legacy project or file returns.

Performance smoke:

- populate the Center with the current effective mapping count;
- open and scroll without non-virtualized row expansion;
- type a representative filter and clear it;
- record behavior honestly; do not change the collection contract inside a visual card if it fails.

## 15. Visual acceptance

At 1920 x 1080 / 100%:

- the Center defaults to 1280 x 720 DIP;
- the field list is the dominant surface;
- the scope pane and Inspector are subordinate;
- no primary region has a second complete border;
- command hierarchy is readable without oversized equal-width buttons;
- empty Inspector state contains no blank property form;
- selected trust/source/type information is visible;
- list vertical cell lines are absent;
- focus, hover and selected states are distinct;
- text and controls do not clip.

At 820 x 620:

- all three Center panes remain reachable;
- field list remains at least 400 DIP before borders/rounding tolerance;
- long metadata trims or wraps;
- no horizontal overlap occurs in the header/command band;
- details are not silently removed.

Screenshots, build and tests are independent gates. None substitutes for another.

## 16. Stop conditions

Stop and request review if any card requires:

- a public API, DTO, schema or serialization change;
- a Field Registry ViewModel/service/provider change;
- different authority, matching, priority or write semantics;
- Shell/Dock or project-file modification;
- new dependency or raster asset;
- different Owner, Show/ShowDialog, DialogResult or confirmation behavior;
- a responsive converter/controller/timer or persisted UI state;
- mutation of an accepted shared style instead of additive R2 adoption;
- loss of virtualization;
- weakening a behavioral assertion;
- more than five changed files;
- more than eight new style keys in one card, excluding only the foundation card’s explicitly authorized Geometry set;
- recovery from a failing build/test outside the card’s allowed files.

## 17. Approval gate

No production XAML, C#, theme, test or package change may begin until the user explicitly confirms:

```text
确认 UI-MODERN-PROGRAM-R1 M4-R2 最终契约
```

After confirmation, M4-R2-P through M4-R2-CLOSE may execute continuously without per-card approval waits, subject to the stop conditions and required visual evidence above.
