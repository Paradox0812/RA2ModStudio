# UI-MODERN-PROGRAM-R1 M4-R2 Stage Result Ledger

Updated: 2026-07-23  
Contract: `Docs/UI-MODERN-PROGRAM-R1_M4R2_FieldRegistryVisualConvergenceContract.md`  
Exact inventory: `Docs/UI-MODERN-PROGRAM-R1_M4R2_ExactUiInventory.md`  
Execution status: **Implementation and automated gates completed; manual visual acceptance pending**

## 1. Scope and outcome

M4-R2 converged the Field Registry window family on the approved modern IDE presentation:

- Field Registry Center now uses a dominant virtualized field list, compact scope navigation and a real selected-field Inspector.
- Manager, Import Preview, Learning, Field Editor, Allowed Values, Remote Preset, Add Property and Annotation now share flat section, command, grid and footer hierarchy.
- Severity and workflow state are expressed graphically where the existing data already supports them.
- Existing handlers, bindings, ownership, dialog lifetime and Field Registry behavior were preserved.

This stage did not change Shell/Dock, dependencies, project files, public APIs, provider priority, matching, learning/import/apply/rollback semantics, parser, diagnostics, completion, Hover, Quick Peek, Save Preflight, BuiltIn data or legacy behavior.

## 2. Rollback anchor

| Item | Evidence |
|---|---|
| Package | `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.PreChange.Rollback.zip` |
| Entries | 967 |
| Bytes | 10,477,691 |
| SHA-256 | `24C7F80967F1B18C1C2554369DE859EEC5961E66E6C1CF1A434442ED92324D5A` |
| Forbidden entries | 0 |

## 3. Stage checkpoints

| Card | Result | Key implementation | Focused verification |
|---|---|---|---|
| M4-R2-P | Passed | Created and audited the pre-change IdeOnly rollback package. | 967 entries; forbidden-entry scan 0. |
| M4-R2-F | Passed | Added eight additive R2 Field Registry styles and thirteen vector Geometry resources. Existing accepted keys remained unchanged. | Debug build passed; focused tests 22/22. |
| M4-R2-A | Passed | Reworked Center geometry and hierarchy; added selected-field `Details` projection and effective-mapping wording. | Debug build passed; focused tests 65/65. |
| M4-R2-B | Passed | Flattened Manager status/rollback/cleanup workspaces; moved verbose rollback metadata to details. | Debug build passed; focused tests 28/28. |
| M4-R2-C | Passed | Added non-interactive source/review/plan/result workflow hierarchy to Import Preview. | Debug build passed; focused tests 30/30. |
| M4-R2-D | Passed | Added flat learning workflow and graphical validation severity presentation. | Debug build passed; focused tests 21/21. |
| M4-R2-E | Passed | Rebalanced Field Editor into metadata/documentation columns with a fixed action footer. | Debug build passed; focused tests 21/21. |
| M4-R2-F2 | Passed | Modernized Allowed Values and clarified Remote Preset as local-only configuration. | Debug build passed; focused tests 37/37. |
| M4-R2-G | Passed | Aligned Add Property and Annotation with the same Inspector/form/footer vocabulary. | Debug build passed; focused tests 19/19. |
| M4-R2-V | Automated gates passed | Completed static XAML/resource/UIA-boundary audit, full build and full non-UI suite. | Build 0 warnings/0 errors; tests 2334/2334. |
| M4-R2-CLOSE | Completed | Flushed governance documents and produced the final clean-source package. | Package evidence recorded in section 8. |

## 4. Implementation inventory

### Production presentation

- `RA2IniEditor.IDE/Themes/IconGeometryResources.xaml`
- `RA2IniEditor.IDE/Themes/IdeFieldRegistryStyles.xaml`
- `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldRegistryCenterWindow.xaml.cs`
- `RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldRegistryHarvestPreviewWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldLearningWizardWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldEditorWindow.xaml`
- `RA2IniEditor.IDE/Views/AllowedValuesEditorWindow.xaml`
- `RA2IniEditor.IDE/Views/RemoteSourcePresetEditorWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldBrowser/Ra2AddPropertyWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldAnnotations/Ra2FieldAnnotationEditorWindow.xaml`

### Boundary tests

- `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/IconResourceBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/FieldRegistryRollbackUiBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/FieldRegistryHarvestPreviewWindowApplyGuardrailTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldLearningWizardBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldEditorWindowBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2AddPropertyUiBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldAnnotationEditorUiBoundaryTests.cs`

## 5. Contract and API result

- Public API changes: none.
- New dependency or project-file changes: none.
- The only C# presentation projection is internal row property `Details`, initialized through the existing `Ra2FieldDetailsViewModel.FromDefinition` path.
- Nine existing selected-row detail bindings in Center were intentionally replaced by the approved `Details` projection.
- No existing AutomationId was removed. New AutomationIds are additive landmarks for empty states, inspectors, workflow areas and action footers.
- Existing Click handlers and bindings outside the authorized Center projection were preserved.

## 6. Static verification

| Gate | Result |
|---|---|
| Changed XAML parse | Passed, 9/9 |
| Missing referenced resource keys | 0 |
| Duplicate Field Registry style keys | 0 |
| Duplicate icon Geometry keys | 0 |
| Additive R2 Field Registry style keys | Exactly 8 |
| Additive Field Registry Geometry keys | Exactly 13 |
| Production `IdeSecondary*` references | 0 |
| Removed AutomationIds | 0 across all nine surfaces |
| Removed handlers | 0 across all nine surfaces |
| Unauthorized removed bindings | 0 |
| DataGrid virtualization | Preserved through the old and R2 `BasedOn` chains |
| Legacy solution/project restored | No |

AutomationId counts before/after:

| Surface | Before | After |
|---|---:|---:|
| Center | 33 | 42 |
| Manager | 57 | 59 |
| Import Preview | 64 | 67 |
| Learning | 36 | 36 |
| Field Editor | 36 | 37 |
| Allowed Values | 13 | 18 |
| Remote Preset | 9 | 11 |
| Add Property | 19 | 22 |
| Annotation | 7 | 10 |

## 7. Verification matrix

| Command / check | Result |
|---|---|
| `dotnet restore .\RA2IniEditor.IDE.sln` | Passed; projects up to date. |
| `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed; 0 warnings, 0 errors. |
| `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | Passed; 2334/2334. |
| Opt-in `FieldImportApplySmokeTests` UIA filter | Blocked before product launch: the existing harness searches for removed/forbidden `RA2IniEditor.sln`. Three tests failed in 7 ms at `FindRepositoryRoot()`. |
| General computer-control visual capture | NotRun by user preference and contract guidance. |

The UIA result is an existing IDE-only test-infrastructure root-discovery defect, not evidence of a product failure. M4-R2 did not restore the legacy solution or widen scope to repair the harness.

## 8. Final clean package

Final evidence is populated after the clean-source packaging gate:

| Item | Evidence |
|---|---|
| Package | `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.Final.zip` |
| Entries | 968 |
| Bytes | Recorded in the external delivery report after the final archive is sealed. |
| SHA-256 | Recorded in the external delivery report after the final archive is sealed; embedding the archive's own final hash would make the package self-referential. |
| Forbidden entries | 0 |

## 9. Screenshot index

Screenshots are independent acceptance evidence. They were not inferred from build or tests.

| Required artifact | Status |
|---|---|
| `M4R2-RegistryCenter-1280x720-Default.png` | NotRun |
| `M4R2-RegistryCenter-1280x720-Selected.png` | NotRun |
| `M4R2-RegistryCenter-820x620-Compact.png` | NotRun |
| `M4R2-RegistryManager-RollbackSelected.png` | NotRun |
| `M4R2-Harvest-DiffReview.png` | NotRun |
| `M4R2-Learning-DraftsAndIssues.png` | NotRun |
| `M4R2-FieldEditor-ValidPreview.png` | NotRun |
| `M4R2-AllowedValues-Populated.png` | NotRun |

Consequently, M4-R2 is not marked as visually accepted. The implementation and automated verification are complete; physical visual review remains the next acceptance gate.

## 10. Deferred Governance Queue

### PublicApiLedger pending entries

None. M4-R2 adds no public API.

### Technical debt pending entries

| Debt | Reason | Impact | Suggested resolution | Status |
|---|---|---|---|---|
| `UI-AUTO-IDEONLY-ROOT` | `RA2IniEditor.UiAutomationTests` root discovery still hardcodes removed `RA2IniEditor.sln`. | Opt-in UIA tests fail before app launch in the IDE-only repository. | Separate narrow infrastructure contract: discover `RA2IniEditor.IDE.sln` without restoring legacy. | Deferred |
| M4-R2 physical screenshot set | Existing automation cannot deterministically capture the required nine-window states; general computer control was intentionally avoided. | Final visual parity at 1920 x 1080 / 100% and compact 820 x 620 is unverified. | Run the named manual visual acceptance set before calling M4-R2 visually accepted. | Pending |

### DecisionLog candidates

None. Implementation followed the already confirmed contract and introduced no new architecture decision.

## 11. Next safe entry

Stage: `UI-MODERN-PROGRAM-R1 M4-R2-VISUAL-ACCEPTANCE`

Goal:

- capture or manually inspect the eight named real WPF states;
- verify the 1920 x 1080 / 100% and 820 x 620 acceptance rules;
- record accepted findings or return one bounded defect list.

Allowed scope:

- read-only launch/capture/manual review;
- update this ledger with evidence.

Forbidden scope:

- ad-hoc XAML polishing;
- Shell/Dock changes;
- Field Registry semantic changes;
- legacy restoration.

Stop condition:

- any visual defect requiring implementation must become a separately bounded correction contract;
- UIA infrastructure repair must remain a separate narrow task.
