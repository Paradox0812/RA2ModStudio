# UI-MODERN-PROGRAM-R1 — M5 Stage Result Ledger

Status: Completed  
Package: M5 Assistive And Transactional Surface Modernization  
Authority: `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md` Revision A  
Last updated: 2026-07-23

## Stage checkpoints

| Card | Result | Scope | Verification evidence | Next safe entry |
|---|---|---|---|---|
| M5-0 | Completed | Exact Completion, Peek, References and transactional-dialog UI inventory | `Docs/UI-MODERN-PROGRAM-R1_M5_AssistiveSurfaceExactUiInventory.md` | M5 Foundation |
| M5 Foundation | Completed | Added scoped `IdeEditorAssistStyles.xaml` and merged it after semantic/base resources | Foundation visual-system tests 15/15 | M5-A |
| M5-A | Completed | Completion dropdown and dormant preview presentation | Completion/UI boundary tests 50/50; real dropdown screenshot | M5-B |
| M5-B | Completed | Quick Peek, Peek Definition and Find References presentation | Language/UI boundary tests 26/26; real Quick Peek and Find References screenshots | M5-C |
| M5-C | Completed | Dirty Navigation and Save Preflight presentation | Dialog/UI boundary tests 26/26; both real modal cancellation paths passed | M5-D |
| M5-D | Completed | Reduced secondary-window compatibility authority to four explicit aliases | Compatibility/automation tests 33/33 | M5-V |
| M5-V | Completed | Full build, regression, real WPF evidence and clean-package closure | Debug build 0 warnings/0 errors; full non-UI 2324/2324; five real screenshots; IdeOnly package passed | M6-A |

## Protected boundary result

- No public C# API, dependency, project-file, Shell/Dock topology, ContentId, persistence or business-service change.
- Completion candidate/commit behavior, Peek resolution/navigation, Find References data/navigation, Dirty Navigation outcomes and Save Preflight analysis/save behavior remain unchanged.
- Existing AutomationIds remain present and unique. Additive landmarks are `Ra2CompletionDropdown.Header`, `Ra2FindReferences.Header`, `DirtyNavigation.WarningBand` and `SavePreflight.WarningBand`.
- `Ra2CompletionPreviewWindow` remains dormant: no production constructor path was introduced.
- All affected XAML uses semantic resources; the final hard-coded six/eight-digit hexadecimal color count is zero.
- Parser/editor/AI/Field Registry/Hover/Diagnostics core semantics, BuiltIn data and legacy remain unchanged.

## Visual evidence

| Artifact | Size | SHA-256 | Result |
|---|---:|---|---|
| `artifacts/UI-MODERN-PROGRAM-R1-M5/M5-CompletionDropdown.png` | 520 × 71 | `7AD8D138F5173A79BC48CC9EED80D59865A7089D5B1DDB419ECDACCADE6F0807` | Accepted: compact header, one selected proposal and metadata hierarchy |
| `artifacts/UI-MODERN-PROGRAM-R1-M5/M5-ReferenceTargetQuickPeek.png` | 502 × 239 | `1B3C6745B69A4DDEF8B7B42AD805D44B0A393C6F06627D4AF4DF83CFE485B721` | Accepted: single-title inspector, chips, explanation and source hierarchy |
| `artifacts/UI-MODERN-PROGRAM-R1-M5/M5-FindReferences.png` | 2560 × 1440 | `D90B34CBDB961D8D2F295459BC1A35A7842E4B83426C70CBAB5A108F4DCE1F8D` | Accepted: real bottom Dock content with one current-file reference |
| `artifacts/UI-MODERN-PROGRAM-R1-M5/M5-DirtyNavigation.png` | 406 × 251 | `4E13043B89B8AEB5872DFC3633F1454C2962110978AC9D87CB297BAA90016156` | Accepted: warning band, file identity and three explicit outcomes; Cancel retained dirty state |
| `artifacts/UI-MODERN-PROGRAM-R1-M5/M5-SavePreflight.png` | 486 × 334 | `4589076209FE11268FC87212A855464B2247806D135500BCA8685415AC9CB46C` | Accepted: issue/severity summary and explicit continue/cancel boundary; Cancel retained dirty state |

Capture host was the current 2560 × 1440 Windows session. A disposable project under `artifacts` was used. The first clean Ctrl+S probe wrote only a trailing space and created a backup inside that disposable fixture; both the fixture and its backup were subsequently deleted. The actual warning-path probe used an in-memory duplicate key, opened Save Preflight, selected Cancel and did not write that change. No product source, user project, registry pack or semantic data was mutated by the smoke.

`Ra2CompletionPreviewWindow` and `Ra2PeekDefinitionWindow` did not receive independent runtime screenshots in this gate; their resource/template/AutomationId contracts are covered by compilation and targeted tests. No new production activation path was added merely to capture them.

## Verification Matrix

Selected profile: full package gate for presentation-only WPF changes.

| Step | Status | Evidence |
|---|---|---|
| Targeted card tests | Passed | Foundation 15/15; M5-A 50/50; M5-B 26/26; M5-C 26/26; M5-D 33/33 |
| Build / Compile | Passed | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`; 0 warnings, 0 errors |
| Full Suite | Passed | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build`; 2324/2324 |
| Real WPF visual smoke | Passed | Five artifacts above; real completion, inspector, references and both transactional cancellation paths |
| Physical 1920 × 1080 / 1280 × 800 / 150% DPI | NotRun | Current host evidence is 2560 × 1440; responsive/hardware matrix remains M6-A scope |
| IdeOnly clean package | Passed | Final accepted package facts recorded below |

Final rollback package: `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M5.Accepted.Rollback.zip` (10,451,406 bytes, 963 entries, SHA-256 `765FE2BC823CD578C827536125AF504A077842FBED29312F433BEF58ADC54C7C`). Hygiene inspection found zero forbidden entries.

## Diff intent table

| File | Change type | Reason | In allowed scope |
|---|---|---|---|
| `Docs/UI-MODERN-PROGRAM-R1_M5_AssistiveSurfaceExactUiInventory.md` | Added | Freeze exact M5 UI/binding/handler/UIA/lifetime boundaries | Yes |
| `RA2IniEditor.IDE/Themes/IdeEditorAssistStyles.xaml` | Added | Scoped assistive/transactional visual vocabulary | Yes |
| `RA2IniEditor.IDE/App.xaml` | Modified | Merge the scoped M5 dictionary | Yes |
| `RA2IniEditor.IDE/Resources/Styles/IdeSecondaryWindowStyles.xaml` | Modified | Retain only four explicit compatibility aliases | Yes |
| `RA2IniEditor.IDE/Views/Language/Ra2CompletionDropdownView.xaml` | Modified | Adopt M5 completion presentation | Yes |
| `RA2IniEditor.IDE/Views/Language/Ra2CompletionPreviewWindow.xaml` | Modified | Adopt dormant preview presentation without activation | Yes |
| `RA2IniEditor.IDE/Views/FieldQuickPeek/Ra2FieldQuickPeekWindow.xaml` | Modified | Adopt inspector presentation | Yes |
| `RA2IniEditor.IDE/Views/Language/Ra2PeekDefinitionWindow.xaml` | Modified | Adopt definition-peek presentation | Yes |
| `RA2IniEditor.IDE/Views/Language/Ra2FindReferencesView.xaml` | Modified | Adopt flat reference-results presentation | Yes |
| `RA2IniEditor.IDE/Views/DirtyNavigation/Ra2DirtyNavigationDialog.xaml` | Modified | Adopt transactional warning presentation | Yes |
| `RA2IniEditor.IDE/Views/SavePreflight/SavePreflightConfirmationDialog.xaml` | Modified | Adopt transactional issue-summary presentation | Yes |
| `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs` | Modified | Assert resource ownership and M5 surface adoption | Yes |
| `RA2IniEditor.Tests/IDE/Ra2CompletionPreviewUiBoundaryTests.cs` | Modified | Preserve completion preview contracts | Yes |
| `RA2IniEditor.Tests/IDE/Ra2LanguageUiBoundaryTests.cs` | Modified | Preserve language-surface contracts | Yes |
| `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs` | Modified | Preserve/add UIA anchors without duplicates | Yes |
| `Docs/UI-MODERN-PROGRAM-R1_M5_StageLedger.md` | Added | Record package result and evidence | Yes |
| `Docs/Codex_CurrentPhase.md` | Modified | Advance latest trusted phase to M6-A | Yes |
| `Docs/RA2IniEditor_IDE_Full_Codex_Context.md` | Modified | Refresh the project context capsule | Yes |
| `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md` | Modified | Add evidence-backed M5 completion note | Yes |

The repository has no Git metadata. Final diff review used the accepted M4 clean-source archive as the baseline and hash-compared the current IDE, test and Docs trees while excluding `bin`, `obj` and `artifacts`. Pre-existing post-M4 evidence updates in the M4 ledger were not modified by M5.

## Deferred Governance Queue

### Public API ledger

None.

### Technical debt

- Physical 1920 × 1080, 1280 × 800 and 150% DPI review remains an M6-A responsive/hardware matrix item.
- Dormant Completion Preview and Peek Definition lack independent M5 runtime screenshots; preserve their current activation semantics and cover them in M6 only if a production path already exists.
- `UI-MODERN-M1-A11Y-001` child-HWND UIA provider gap remains separate and unchanged.

### Decision log candidate

No new architecture decision. M5 followed Revision A: scoped resource authority, explicit adoption and compatibility retirement only after reference verification.

## Next safe entry

Stage: M6-A Responsive, Keyboard And Additive UIA Smoke.  
Allowed: responsive/DPI review, keyboard traversal, focus visibility, additive UIA smoke and evidence-only corrections within the approved visual system.  
Forbidden: new product behavior, public API, dependencies/project files, Shell/Dock persistence/topology changes, parser/editor/language/AI/Field Registry/Save semantics and legacy restoration.  
Stop condition: complete the contracted M6 card only; do not begin M6-B or M6-C automatically without its preceding gate.
