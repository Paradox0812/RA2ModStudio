# RA2IniEditor.IDE — Historical Accumulated Current Phase

> Superseded on 2026-08-22 by the concise `Docs/Codex_CurrentPhase.md`.
> This snapshot is preserved as historical phase evidence and must not be used as
> the current task pointer.

## AUTOMATION-HLI-0B minimum capability contract prepared; implementation awaiting confirmation

```text
Contract: Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md
State: Proposed / self-reviewed / no implementation started.
Necessity decision: a UI-neutral assembly boundary is required for an independent
net8.0 Agent/CLI/Job host, but it is not required merely for the current in-process
WPF AI. A bulk Language/Diagnostics/Editing move is explicitly rejected.
Recommended architecture: add a net8.0 RA2IniEditor.Application candidate that
initially references Core only; migrate minimal vertical capability slices and make
the IDE consume the new canonical implementation.
First capabilities: ini.document.section.get, ini.document.references.find
(current-document only), ini.document.diagnostics.validate, and
ini.document.edit.preview. Apply/Undo, active Preview ownership and Save remain
IDE-host-only and are not Agent capabilities.
Risk: contract diff R0/DocsOnly; future assembly/API migration R3. Incremental
extraction is medium controlled risk; a bulk move or duplicated algorithm path is high.
Change: documentation only. No public API, source, project, test, XAML, Shell,
AutomationId, persistence or semantic behavior changed.
Next gate: user confirmation of HLI-0B, followed by a separate AUTOMATION-HLI-1A0
Dependency Cone Characterization Contract. Do not create the Application project yet.
```

---

## AUTOMATION-HLI-0A existing capability audit completed

```text
Contract: Docs/AUTOMATION-HLI-0A_ExistingCapabilityAuditContract.md
Matrix: Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md
Ledger: Docs/AUTOMATION-HLI-0A_StageLedger.md
Finding: Core field schema is directly headless-consumable, while most existing
Language/Search/Diagnostics/A2 Preview algorithms are UI-independent in behavior but
remain internal to the net8.0-windows IDE assembly. Algorithmically headless therefore
does not mean an independent host can reference them today.
Reuse: existing parser/semantic/diagnostic/Search/A2/A3/A4/Save paths must be adapted
or migrated, not duplicated. Current Search is text search; a general project semantic
reference query is not yet present. Formal Template, Capability Registry/Gateway,
Job/Event/Artifact infrastructure is also not present.
Boundary: Agent-facing first scope should be query + diagnostic + semantic Preview.
A3 Apply/Undo and Save/Writer remain IDE-host-only and are not Agent capabilities.
Change: documentation only; no public API, source, project, test, XAML, Shell,
AutomationId, persistence or runtime behavior changed.
Verification: DocsOnly static path/dependency/scope audit passed; build/test/package
were NotRun because no implementation or project metadata changed.
Next safe entry: AUTOMATION-HLI-0B Minimum Capability Contract. The recommended
net8.0 Application assembly is only a candidate and requires explicit approval.
```

---

## AGENT-AUTHORING-1-R1 A4-R1 reliable structured editing implemented

```text
Contract: Docs/AGENT-AUTHORING-1-R1_A4_R1_ReliabilityContract.md
Ledger: Docs/AGENT-AUTHORING-1-R1_A4_R1_StageLedger.md
Completed: canonical Official/Custom/Invalid endpoint identity; deterministic local
Advisory/EditExplicit/EditAmbiguous/EditUnavailable routing; required authoring tool;
proposal/needs_clarification outcome; system/user privilege split; typed rejection
when the tool is not invoked; background-safe Preview runner; final Shell generation,
cancellation, active-handle, currency and ownership gates; proposal-card history pinning.
Authority: provider prose and raw tool JSON never modify the document. Only a locally
validated A3 Preview can create a card, and only explicit Apply changes the current
in-memory session. Save, project/multi-file edits and automatic retry remain excluded.
Verification: staged targeted tests and loopback HTTP/SSE-to-local-Preview integration
passed. Final full verification/package evidence is in the A4-R1 ledger.
UI: no ShellWindow.xaml or layout/theme change; no computer-control testing was used.
Next safe entry: AGENT-AUTHORING-1-R1 HLI-0A high-level Agent interface contract.
```

---

## AGENT-AUTHORING-1-R1 A4 AI structured edit proposal completed

```text
Contract: Docs/AGENT-AUTHORING-1-R1_A4_AiStructuredEditProposalContract.md
Ledger: Docs/AGENT-AUTHORING-1-R1_A4_StageLedger.md
Completed: official DeepSeek tool-call transport, one preview-only
preview_ini_edit_plan tool, strict arguments adapter, request/current snapshot
currency check, A3 preview reuse, diagnostic/trust apply policy, and an inline
review card with explicit Apply/Dismiss actions.
Safety: ordinary chat remains advisory; custom endpoints remain advisory-only;
malformed/multiple/unsupported tool calls never become plans; AddedError blocks
Apply; Apply changes only the current in-memory editor session and never saves.
Lifecycle: document edits/switches, programmatic text changes, edit-mode changes,
Field Registry reload, chat clear, replacement proposal and Shell close invalidate
the one active proposal. A proposal is single-use and cannot be replayed.
Shell scope: ShellWindow.xaml.cs private wiring only. ShellWindow.xaml, Dock/layout,
menu/toolbar and global visual resources are unchanged.
Preserved: parser, diagnostics semantics, Completion, Field Registry priority/data,
Search, Save/Writer/Backup/Rollback, project/dependencies and legacy.
Next safe entry: manual A4 acceptance with an official endpoint and a disposable
INI file. Broader operations, project-wide edits, automatic apply/save, retry and
custom-endpoint tools require separate contracts.
```

---

## AGENT-AUTHORING-1-R1 A3 editor transaction port completed

```text
Contract: Docs/AGENT-AUTHORING-1-R1_A3_EditorTransactionPortContract.md
Ledger: Docs/AGENT-AUTHORING-1-R1_A3_StageLedger.md
Context capsule: Docs/ContextCapsule_AGENT_AUTHORING_1_A3.md
Completed: workspace-owned single active Preview, generation race protection,
PreviewId + explicit confirmation, single-use claim/consumption, Shell-owned live
currency recheck, one Session revision, one editor sync and one semantic Undo unit.
Apply changes in-memory editor/session state only; it never saves.
Shell scope: only ShellWindow.xaml.cs private transaction glue and invalidation
points changed. ShellWindow.xaml, Dock/layout and AutomationIds are unchanged.
Verification: targeted 23/23; IDE-only Debug build 0 warnings / 0 errors; full
non-UI tests 2436/2436.
Preserved: AI, Search behavior, Save/Writer/Backup/Rollback, parser, diagnostics,
Completion, Field Registry semantics/data, project/dependencies and legacy.
Next safe entry: separately contract AGENT-AUTHORING-1-R1 A4 user-visible
preview/confirmation wiring. A3 itself has no user entry and AI cannot apply edits.
User stop condition: stop after A3; do not enter A4 automatically.
```

---

## AGENT-AUTHORING-1-R1 A2 single-document plan preview completed

```text
Contract: Docs/AGENT-AUTHORING-1-R1_A2_SingleDocumentPlanPreviewContract.md
Ledger: Docs/AGENT-AUTHORING-1-R1_A2_StageLedger.md
Context capsule: Docs/ContextCapsule_AGENT_AUTHORING_1_A2.md
Completed: internal immutable authoring snapshot, structured UpsertField /
ReplaceFieldValue plan, deterministic original-coordinate ChangeSet planning,
candidate/current language analysis, operation evidence, diagnostic delta, and
pure preview currency evaluation.
Explicit exclusions: Preview Store, Apply, Undo/Redo integration, Save, disk write,
AI, Shell, XAML/ViewModel wiring, project/multi-document transactions.
Verification: IDE-only Debug build passed with 0 warnings / 0 errors; related
regression passed 104/104; full non-UI tests passed 2419/2419.
Preserved: Shell/Dock/UI, parser, diagnostics, Completion, Field Registry, Hover,
Quick Peek, AI, Save/backup/rollback, BuiltIn and legacy.
Next safe entry: AGENT-AUTHORING-1-R1 A3 EditorTransactionPortContract.
Do not describe A2 as a user-visible editing feature before A3/A4 wiring exists.
```

---

## SEARCH-1-R1 project search and current-file replace completed

```text
Contract: Docs/SEARCH-1-R1_ContinuousContract.md
Ledger: Docs/SEARCH-1-R1_StageLedger.md
Context capsule: Docs/ContextCapsule_SEARCH_1_R1.md
Completed: project-level read-only search over canonical Project Explorer files;
current-file in-memory search override; case/whole-word/regex matching; stable
results and cross-file navigation; current-file preview-first Replace All;
DocumentId/EditRevision stale protection; one semantic Undo/Redo transaction.
Explicit exclusions: recursive discovery, background index, project/multi-file
replace, automatic save, files outside the canonical top-level .ini list.
Verification: Debug IDE-only solution build passed with 0 warnings / 0 errors;
full non-UI tests passed 2380/2380; Search floating open/hide/reopen UIA passed 1/1.
IdeOnly clean package: artifacts/RA2IniEditor.IDE.SourceClean.zip; 1003 entries;
forbidden entries 0; required entry/contract omissions 0.
Known boundary: SEARCH-UIA-001 — AvalonDock floating child-HWND exposes host chrome
but not hosted Search controls to external FlaUI traversal.
Preserved: ShellWindow.xaml and Dock topology/persistence; parser, diagnostics,
Completion semantics, Field Registry, Hover, AI, Save/backup/rollback, BuiltIn and legacy.
Next safe entry: rebaseline AGENT-AUTHORING-1-R1 A2 against the now-existing
DocumentId/EditRevision contract. Do not infer Agent multi-file writing from Search Replace All.
```

---

## AGENT-AUTHORING-1-R1 A0 Semantic Characterization completed

```text
Contract: Docs/AGENT-AUTHORING-1-R1_A0_SemanticCharacterizationContract.md
Scope: tests and governance documentation only; no production behavior changes.
Locked facts: Core/TextModel differences for // comments, trailing section text,
semicolon/hash inline comments, trailing newline representation, mixed newline
metadata, covered-field comments and empty keys.
Verification: new characterization tests 8/8 passed; related parser/TextModel tests
26/26 passed; Debug IDE-only solution build passed with 0 warnings / 0 errors.
Preserved: public API, parser behavior, diagnostics, Field Registry, editor session,
Undo/Redo, Completion, save, AI, Shell/Dock/XAML, project files and legacy state.
Next safe entry: AGENT-AUTHORING-1-R1 A1-A ReadonlyLanguageAnalysisFacadeContract.
```

---

## UI-MODERN-PROGRAM-R1 VISUAL-FIX2 implementation and automated gates completed; visual acceptance pending

```text
Ledger: Docs/UI-MODERN-PROGRAM-R1_VISUAL-FIX2_StageLedger.md
Completed implementation: Field Registry active-pack columns are compacted to 88/48 DIP; startup-only floating-host opacity suppression prevents the hidden Search host from rendering during the compiled-default/persisted-layout transition.
Lifecycle boundary: the existing Float -> Dispatcher -> visibility order is preserved; the suppression path never calls Hide and always restores prior opacity in finally.
Verification: XAML parse passed; Debug solution build passed with 0 errors and one pre-existing CS8602 test warning; Shell/visual/layout boundary tests passed 76/76; full non-UI tests passed 2335/2335.
Final clean package: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-VISUAL-FIX2.Final.zip; 970 entries; forbidden-entry scan 0; required-file omissions 0; exactly 10 expected changes versus VISUAL-FIX1.
Preserved: public API, dependencies/project files, Dock ContentIds/Home profiles/persistence format/migration, Search behavior, Field Registry semantics, parser/editor/AI/Completion/Hover/Diagnostics/Save/BuiltIn/legacy.
Visual evidence: NotRun. Real startup no-flash and final active-pack spacing remain manual visual acceptance items.
Next safe entry: UI-MODERN-PROGRAM-R1 VISUAL-FIX2-ACCEPTANCE.
```

---

## UI-MODERN-PROGRAM-R1 VISUAL-FIX1 implementation and automated gates completed; visual acceptance pending

```text
Ledger: Docs/UI-MODERN-PROGRAM-R1_VISUAL-FIX1_StageLedger.md
Completed implementation: Field Editor close-glyph geometry, Field Registry active-pack width, AI compact context/welcome/footer composition.
AI contract correction: no Expander; only concise subject and conversation information is visible. The verbose existing context text is retained as a ToolTip.
Verification: four changed XAML/resource files parsed; Debug solution build passed with 0 warnings/0 errors; Shell/visual boundary tests passed 48/48; full non-UI tests passed 2334/2334.
Final clean package: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-VISUAL-FIX1.Final.zip; 969 entries; forbidden-entry scan 0.
Preserved: AI element names/AutomationIds/update paths; Dock ContentIds/topology/lifecycle; Field Registry semantics; public API; dependencies/project files; parser/editor/completion/Hover/diagnostics/save/BuiltIn/legacy.
Visual evidence: NotRun because computer control was intentionally avoided. Do not mark the correction visually accepted until Field Editor, Field Registry Center and AI are manually reviewed.
Next safe entry: UI-MODERN-PROGRAM-R1 VISUAL-FIX1-ACCEPTANCE.
```

---

## UI-MODERN-PROGRAM-R1 M4-R2 implementation and automated gates completed; visual acceptance pending

```text
Contract: Docs/UI-MODERN-PROGRAM-R1_M4R2_FieldRegistryVisualConvergenceContract.md
Exact inventory: Docs/UI-MODERN-PROGRAM-R1_M4R2_ExactUiInventory.md
Latest trusted ledger: Docs/UI-MODERN-PROGRAM-R1_M4R2_StageLedger.md
Status: implementation complete; automated gates complete; manual screenshot acceptance pending.
Scope delivered: Field Registry Center, Manager, Import Preview, Learning, Field Editor, Allowed Values, Remote Preset, Add Property and Annotation visual convergence.
Verification: restore passed; Debug build passed with 0 warnings/0 errors; full non-UI tests passed 2334/2334; static XAML/resource/UIA-boundary/virtualization gates passed.
UIA boundary: opt-in FieldImportApplySmokeTests are blocked before product launch because the existing harness searches for removed RA2IniEditor.sln. This remains separate debt UI-AUTO-IDEONLY-ROOT; legacy was not restored.
Visual evidence: all eight contract screenshots are NotRun because general computer control was avoided. Do not mark M4-R2 visually accepted until the named real-WPF states are reviewed.
Pre-change rollback: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.PreChange.Rollback.zip; 967 entries; 10,477,691 bytes; SHA-256 24C7F80967F1B18C1C2554369DE859EEC5961E66E6C1CF1A434442ED92324D5A.
Final clean package: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.Final.zip; package evidence is recorded in the M4-R2 ledger.
Public API/dependencies/project files/Shell/Dock/provider priority/parser/editor/AI/completion/Hover/Quick Peek/diagnostics/save/BuiltIn/legacy: unchanged.
Next safe entry: UI-MODERN-PROGRAM-R1 M4-R2-VISUAL-ACCEPTANCE. Any correction found there requires a bounded follow-up contract.
```

---

## UI-MODERN-PROGRAM-R1 active; M6-B zero-reference cleanup completed

```text
Contract: Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md
Revision: A, confirmed by the user on 2026-07-22.
Execution mode: continuous StagePackage; no per-card approval waits, with safety stops and visual evidence still mandatory.
Risk: program R3 presentation boundary, each implementation card restricted to R1.
Completed: P0 authority/succession contract and unique no-Git rollback baseline.
Program baseline package: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-P0.Rollback.zip
M3 accepted rollback package: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M3.Rollback.zip
M4 accepted rollback package: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4.Accepted.Rollback.zip
M5 accepted rollback package: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M5.Accepted.Rollback.zip
M5 accepted package SHA-256: 765FE2BC823CD578C827536125AF504A077842FBED29312F433BEF58ADC54C7C; 963 entries.
Completed cards: M5-0, Foundation, M5-A through M5-D and M5-V.
Latest stage ledger: Docs/UI-MODERN-PROGRAM-R1_M6_StageLedger.md
Verification: Debug solution build passed with 0 warnings/0 errors; full non-UI tests passed 2324/2324; IdeOnly package hygiene passed.
Visual evidence: five real WPF artifacts prove Completion dropdown, Quick Peek, Find References, Dirty Navigation cancel and Save Preflight cancel. Current host was 2560 x 1440.
Completed stage: M6-A Responsive, Keyboard And Additive UIA Smoke.
Completed narrow correction: UI-MODERN-PROGRAM-R1-M6-A-Fix3 ComboBoxFullSurfaceHitTarget. The canonical UiComboBoxStyle now routes the complete non-editable control surface through its existing DropDownToggle while preserving editable text input and the right-side arrow. AI, Search, Issues and Field Registry consumers inherit the correction without local event handlers or duplicate templates.
Fix3 verification: XAML XML parse passed; Debug solution build passed with 0 errors and one pre-existing CS8602 test warning; visual-system tests passed 16/16; combined visual/Shell boundary tests passed 47/47; full non-UI tests passed 2324/2324.
Completed native-window correction: UI-MODERN-PROGRAM-R1-M6-A-Fix4-R1 WorkAreaBoundedMaximize. ShellWindowChromeController now handles WM_GETMINMAXINFO for the main Shell only, constraining native maximize coordinates to the current monitor WorkArea while preserving system track sizes, 1280 x 800 RestoreBounds, Snap/maximize routing and AvalonDock floating-host behavior. Invalid native geometry fails open to Windows.
Fix4 verification: Debug solution build passed with 0 errors and one pre-existing CS8602 test warning; Chrome/Shell/work-area tests passed 67/67; full non-UI tests passed 2332/2332. Isolated real Debug Shell startup was maximized at client rect 0,0-2560,1392 against monitor 0,0-2560,1440 and WorkArea 0,0-2560,1392; restore remained 1280 x 800 and re-maximize returned to the same WorkArea without editing Shell XAML.
Completed narrow correction: UI-MODERN-PROGRAM-R1-M6-A-Fix5 WindowLayoutGlyphClip. The Window Layout glyph keeps its existing 16 x 16 slot and 26 x 26 button, but its lower chevron now ends at Y=15 rather than the clipped Y=16 stroke boundary. Toolbar spacing, AutomationId, command routing and layout remain unchanged.
M6-A runtime evidence: the existing opt-in main-path UIA harness now includes `M6A_ShellResponsiveLandmarksAndKeyboardPaths_RemainReachable`. On the available 2560 x 1440 / 2560 x 1392 WorkArea / 96-DPI host it passed 1920 x 1040 DIP and 1280 x 800 DIP profiles; toolbar endpoints, Output, Project Explorer and current-document status stayed within Shell bounds. Shift+Tab/Tab traversed Project Explorer <-> Window Layout, and the AI model selector passed F4 expand / Escape collapse.
M6-A verification: restore passed; Debug solution build passed with 0 errors and one pre-existing CS8602 warning; affected Shell/visual/UIA boundary tests passed 87/87; M6-A real UIA smoke passed 1/1; full non-UI tests passed 2332/2332.
Completed stage: M6-B Zero-Reference Audit And Safe Legacy-Style Cleanup. Fourteen safe Shell history keys were removed; fourteen IdeSecondary compatibility definitions and 56 production references were retired in favor of existing IdeFieldRegistry/IdeAssist authority. The compatibility dictionary and App merge entry were removed. Final production IdeSecondary occurrences are zero; final application resource inventory is 379 explicit keys with zero duplicates.
M6-B verification: pre-change IdeOnly rollback package recorded at artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M6B.PreChange.Rollback.zip (963 entries, SHA-256 8C3BC4C8BA43810B2734EF5D792A878D8E4735A47399A5AC5B6EFC67394057A6); static XAML/resource audit passed; Debug solution build passed with 0 warnings/0 errors; affected boundary tests passed 64/64; full non-UI tests passed 2332/2332; hidden real startup produced a main-window handle without early exit.
Known accessibility boundary retained: the AvalonEdit document surface is still absent from the runtime UIA tree (`UI-MODERN-M1-A11Y-001`). M6-A proves document loading through the stable current-file/status landmarks and does not add synthetic peers. `UI-MODERN-M6A-UIA-001` records that the older edit/completion/add-property smoke still uses the clean-file `RevertInMemoryChangesButton` enabled state as a stale load-complete signal and fails there; the full opt-in UIA suite is therefore not claimed green. Physical 150% DPI and mixed-monitor hardware remain NotRun.
Next: separately gated M6-C final closure. Do not start it automatically.
Frozen: public API, dependencies/project files, seven ContentIds, Dock persistence/lifecycle, Search behavior, parser/editor/AI/Field Registry/Completion/Hover/Diagnostics/Save semantics, BuiltIn data and legacy.
```

---

## UI-MODERN-M2-R2 cohesive Shell modernization completed

```text
Contract: Docs/UI-MODERN-M2-R2_CohesiveShellModernizationContract.md
Completed: 2026-07-22 under the user's continuous-execution authorization.
Floating behavior: clicking Search now restores and activates an already minimized floating host through the registered ContentId path and one Loaded-priority focus dispatch.
Floating chrome: minimize/close remain; the dedicated floating maximize button, its AutomationId, and HTMAXBUTTON hover region are retired. Main Shell maximize/restore remains unchanged.
Title rule: the inner pane header collapses only for a directly hosted one-child floating pane; docked and multi-pane/tab navigation remains visible. The accepted Row 1 content-host drag baseline is preserved.
Typography: UiFontFamily is now Segoe UI Variable Text, Microsoft YaHei UI, Segoe UI; Window-level adoption changes only FontFamily. Consolas code surfaces and text rendering modes are unchanged.
Menu/title Fix1: one IdeMainMenuStyle/IdeMainMenuItemStyle authority replaces the competing Shell-local style; top-level padding is 4 DIP. Screenshot review then removed the visible IDE-name TextBlock and its column, leaving icon -> menu -> drag region -> caption buttons. The Window Title binding remains for taskbar/Alt+Tab/system semantics.
Verification: restore passed; Debug solution build passed with 0 warnings/0 errors; M2D-Fix1 targeted UI/Shell tests passed 55/55; prior full non-UI tests passed 2313/2313; IdeOnly package passed with 953 files.
Manual follow-up: no computer-control smoke was used by request. Verify minimize/Search restore three times and review 1920x1080 font/menu appearance. Physical 1280x800 and 125%/150% DPI remain manual.
Unchanged: seven ContentIds, Dock Home/v2 persistence, Search business behavior, dependencies/public API/project files, parser/editor/AI/Field Registry/Completion/Hover/Diagnostics/Save semantics, and legacy boundary.
Open separate debt: UI-MODERN-M1-A11Y-001 child-HWND UIA provider gap; SEARCH-1 real Search behavior.
Next recommended stage: perform the short manual M2 visual/lifecycle acceptance; after acceptance, contract SEARCH-1 instead of continuing ad-hoc UI patches.
```

---

## UI-MODERN-M1-R2 + UI-DOCK-5 completed

```text
Contracts: Docs/UI-MODERN-M1-R2_PreviewParityContract.md and Docs/UI-DOCK-5_SearchFloatingTopologyContract.md
Completed: 2026-07-22 under the user's continuous-execution authorization.
Current entry: package closed after M1G verification and governance flush.
Theme: light only. The frozen dark preview is authoritative only for layout, density, hierarchy, and single-title topology.
Implemented topology: Search is hidden Floating home at preferred 560 x 620; Bottom contains Problems/Output plus on-demand Find References; valid v2 preserves user layout and valid v1 migrates once without editing v1.
Stable authority: seven ContentIds, Shell-owned content instances, AvalonDock 4.74.1, strict UI-DOCK-4 validation/atomic-write/close-ordering safety, parser/editor/AI/Field Registry semantics.
Search surface: no sample query, rows, count, or Mock text; query/options/scope/file-pattern preview is present and execution buttons are explicitly unavailable pending SEARCH-1.
Verification: final restore/build passed with 0 warnings and 0 errors; full non-UI tests passed 2313/2313; two-process Search floating hide/reopen persistence UI smoke passed 1/1; IdeOnly package passed with 951 files.
Screenshot: artifacts/UI-DOCK-5-search-smoke.png.
No public C# API, dependency, project file, parser, editor, AI, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, or legacy behavior changed.
Open risk: AvalonDock floating host chrome is UIA-visible, but hosted Search content AutomationIds are not reachable from the top-level floating HWND; do not add synthetic peers without a narrow accessibility contract. Physical 1280 x 800 and non-100% DPI remain hardware NotRun.
Next recommended stage: first contract `UI-MODERN-M1-H1 FloatingContentAutomationAccessibility`; after that, `SEARCH-1` may own real project-index execution and results.
```

---

## UI-MODERN-M1-0R Visual System Foundation revised final contract proposed

```text
Contract: Docs/UI-MODERN-M1_VisualSystemFoundationContract.md
Contract date: 2026-07-22.
Status: revised final candidate generated after reliability self-review; awaiting explicit user approval. No production XAML/C#/test/project change has started.
Current-doc risk: R0 DocsOnly. Future implementation risk: R3 shared WPF presentation authority, Shell non-client behavior, and AvalonDock floating-host lifecycle.
Corrected current fact: Search is now the Shell-owned Tool.Search/SearchToolView with stable Search AutomationIds, but its data/commands remain read-only placeholder behavior. M1 may style it only; SEARCH-1 owns real behavior.
Package order: M1A semantic tokens; M1B core-control template definition (non-visual); M1C navigation/collection template definition (non-visual); M1D explicit Shell/Search adoption and Visual Stop 1; M1E integrated Shell chrome and Visual Stop 2; M1F floating-host chrome and Visual Stop 3; M1G verification/docs closure.
Reliability amendment: M1B/M1C cannot rebase existing compatibility styles or modify production surfaces; IdeSecondaryWindowStyles and secondary XAML are frozen; template parts/behavior receive an STA verification matrix; M1E/M1F require unique clean-source rollback archives and pre-edit file hashes.
Authority preserved: M0 1920x1080 DIP contract; seven ContentIds; 300/260 default geometry; Shell-owned content instances; UI-DOCK-4 v1 persistence and recovery.
Public API/dependency/project-file change: none authorized.
Approval phrase: 确认 UI-MODERN-M1 最终契约
Next safe entry after approval: M1A SemanticTokenAuthority only.
```

---

## UI-DOCK-4 Layout Persistence completed

```text
Contract: Docs/UI-DOCK-4_LayoutPersistenceContract.md
Contract date: 2026-07-22.
Risk: R4 presentation persistence, AvalonDock model replacement, Shell lifecycle, compatibility fallback, and monitor recovery.
Status: UI-DOCK-4A through UI-DOCK-4F completed; package closed.
Current implementation entry: none. Optional hardware-only multi-monitor/DPI smoke remains available but is not a code blocker.
Continuous cards: 4A serializer proof; 4B dynamic ContentId identity and compiled-default reset; 4C versioned atomic store; 4D restore/save lifecycle; 4E monitor recovery; 4F restart verification and closure.
Authority: compiled ShellWindow.xaml remains the sole default-layout authority; Shell continues to own all content instances; persisted XML owns presentation topology only.
Public API: no change authorized; all new types must remain internal.
Dependency: retain Dirkster.AvalonDock 4.74.1; no new package or project-file change.
Verification evidence: final Dock targeted tests passed 35/35; full non-UI suite passed 2305/2305; restore/build passed; IdeOnly package passed with 940 files. A pre-existing CS8602 warning may appear when the test project recompiles; the final solution build completed with zero errors.
4A evidence: layout/model replacement, same-content reuse, unknown ContentId cancellation, complete seven-content topology round-trip, and Bottom/Right AddToLayout fallback are proven.
4B evidence: dynamic current-model identity, content/invariant rebinding, hidden-tool snapshot coverage, and repeated compiled-default Reset are proven.
4C evidence: v1 LocalAppData path, strict UTF-8/no-BOM and 1 MiB bounds, safe XML rejection, bounded quarantine, atomic-write preservation, and bounded I/O failures are proven.
4D evidence: startup restore/default fallback, state/event rebinding, close-cancel ordering, accepted-close save, and immediate Reset persistence are implemented and tested.
4E evidence: finite geometry, connected negative coordinates, reachable title region, disconnected-monitor fallback, oversized clamp, and conservative mixed-DPI fallback are tested.
4F evidence: real Debug-process Reset/normal-close/restart smoke produced a safe 2573-byte v1 file with exactly seven identities and no business markers; packaged Codex launch caused expected Windows LocalAppData virtualization. Explicit disconnected-monitor and 125%/150% hardware smoke were not available.
Next recommended phase: return to the separately approved UI-MODERN visual-stage plan; UI-DOCK persistence requires no further implementation.
Forbidden scope: visual redesign/modernization, Search behavior, AI, Field Registry, parser, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, editor semantics, BuiltIn data, and legacy.
```

---

## UI-DOCK-3R completed; AI-REL-TD-001 repaid

```text
Contract: UI-DOCK-3R reliability amendment approved by the user on 2026-07-21.
Status: Completed.
Implementation: deterministic Home profiles/order, floating-close recovery, empty-Home reconstruction, compact preferred floating sizes, toolbar/View layout commands, and idempotent default reset are present.
Manual evidence: Codex runtime smoke passed Search 800x420 floating, close-to-Bottom, batch return, and default reset; the user explicitly confirmed the remaining manual test passed.
Stable layout: maximized Shell; Document.Source fixed; 300-DIP right group; 260-DIP bottom group; Output active; Find References hidden by default.
Automation: Shell.MainToolbar.WindowLayoutButton, Shell.Menu.WindowLayout, Shell.WindowLayout.ReturnFloatingToolsHome, Shell.WindowLayout.ResetDefaultLayout, and their View-menu mirrors are frozen.
Targeted verification: combined Shell boundary tests passed 39/39.
Restore: passed.
Solution build: passed with 0 errors and one pre-existing CS8602 test warning.
AI reliability closure: AI-REL-TD-001 resolved by recording the winning request termination cause before cancellation propagation; no retry, replay, fallback, API, or failure-taxonomy expansion.
AI verification: exact regression 20/20; DeepSeek client tests 62/62; two full-suite runs each 2278/2278.
Clean package: passed, 934 source files, artifacts/RA2IniEditor.IDE.SourceClean.zip.
Closed debt: AI-REL-TD-001.
Protected behavior unchanged except for the approved deterministic total-timeout versus late-user-cancellation attribution: parser, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, editor semantics, SSE protocol, BuiltIn data, and legacy.
Next safe entry: choose the next separately contracted product stage. Do not begin UI-DOCK-4 persistence automatically.
Authoritative document: Docs/UI-DOCK-1_AvalonDockShellContract.md.
```

---

## UI-DOCK-3 modern visual gate implemented; superseded by UI-DOCK-3R status above

```text
Contract: approved by the user on 2026-07-21.
Risk: R3 Shell composition and dependency change, contained by stable ContentIds and unchanged business semantics.
Completed cards: UI-DOCK-0A dependency/license, UI-DOCK-0B exact API inventory, UI-DOCK-1A Find References content extraction, UI-DOCK-1B Search content extraction, UI-DOCK-2 AvalonDock Shell composition, UI-DOCK-3 project-owned modern dock theme and automation anchors.
Dependency: Dirkster.AvalonDock 4.74.1 only; no 5.x preview, theme, MVVM, or DI add-on.
Default layout: maximized Shell; non-floating Document.Source; 300-DIP full-height right group; 260-DIP editor-side bottom group; Output active; Find All References hidden until invoked.
Stable tool identities: Tool.SectionExplorer, Tool.AiAssistant, Tool.Problems, Tool.Output, Tool.Search, Tool.FindReferences.
Modern visual layer: project-owned AvalonDock tabs, pane titles/actions, active accent, hover/focus states, and splitters are applied through ShellTheme; rendered tabs/headers derive stable AutomationIds from ContentId.
Runtime smoke: startup/default layout, Search activation, right tool switching, both splitter directions, Search floating, and floating maximize/restore passed on the available 2560x1440 display. Application restart restored the compiled dock layout.
Verification: IDE project build passed 0 warnings / 0 errors; Shell boundary suite passed 6/6; IDE-only solution build passed 0 warnings / 0 errors after smoke processes were closed; full non-UI suite passed 2276/2276. Clean package is deferred until user accepts the second visual gate.
Remaining visual check: scripted dragging did not reliably trigger AvalonDock docking guides, so Search / Find All References re-docking requires user manual confirmation before UI-DOCK-3 is closed.
Protected behavior unchanged: parser, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, undo/redo, AI request/streaming, BuiltIn data, and legacy.
Current stop: user review of the second modern AvalonDock visual layer and manual re-dock check. Do not begin UI-DOCK-4 persistence until approved.
Authoritative documents: Docs/UI-DOCK-1_AvalonDockShellContract.md and Docs/UI-DOCK-1_AvalonDockExactApiInventory.md.
```

---

## UI-MODERN-M0 Visual Baseline And WPF Dimension Contract completed; awaiting visual approval

```text
Package: UI-MODERN-M0A through M0D.
Status: visual baseline package completed on 2026-07-21; production XAML not started.
Risk: package R0 DocsOnly; the later Shell/UI implementation route remains R3 and stays behind the user visual-review gate.
M0A: four approved visual references were frozen with dimensions and SHA-256 hashes.
M0B: current real WPF Shell, standalone Search placeholder, and Field Registry Center screenshots were captured.
M0C: 25 XAML files were audited for template coverage, hard-coded colors, DPI/layout rounding, virtualization, native/custom chrome, and AutomationIds.
M0D: deterministic Shell/Search/Field Registry DIP specifications, responsive rules, and three 1600x1000 annotated dimension diagrams were generated and visually inspected.
M0D-Fix2 supersedes Fix1: the user supplied a Visual Studio layout reference and set the default design resolution to 1920x1080. The editor-side workspace is 1616 DIP (84.2%), the 300-DIP right tool spans the full 994-DIP workspace height, and the 260-DIP bottom tool is nested only beneath the editor. With bottom tools open the editor remains 1616x700; collapsed it becomes 1616x964. The compact top title/menu plus toolbar occupies 62 DIP.
Verification: dotnet build RA2IniEditor.IDE.sln -c Debug --no-restore passed with 0 warnings and 0 errors; documents and PNG dimensions/hashes were checked.
Current stop: renewed user visual review of M0D-Fix2 1920x1080 Visual Studio-ratio diagrams. Do not begin M1 production XAML until the user accepts or corrects them.
Next proposed stage after approval: UI-MODERN-M1 Visual System Foundation, using project-owned DynamicResource tokens and WPF ControlTemplates.
Protected behavior: parser, Field Registry semantics/priority, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, AI behavior, BuiltIn data, and legacy remain unchanged.
Authoritative M0 documents: Docs/UI-MODERN-1_CanonicalSurfaceVisualParityContract.md, Docs/UI-MODERN-1_ControlDpiAutomationAudit.md, Docs/UI-MODERN-1_WpfDimensionSpec.md, Docs/UI-MODERN-1_ResponsiveLayoutSpec.md.
```

---

## FR-DQ-4 PlaceholderRetirementAndTrustCleanup completed

```text
Contract state: confirmed by user; continuous package execution authorized without per-stage approval.
Risk: R4 Field Registry migration and Completion visibility; governed by the confirmed FR-DQ-4 contract and rollback anchors.
Completed: 4-0 rollback anchors, 4A audit manifest, 4B diagnostic-only key Completion exclusion, 4C confirmed superseded retirement.
Completed: FR-DQ-4-0 through FR-DQ-4H.
Current: package closed; next phase requires a new confirmed contract.
Runtime BuiltIn rows: 2604.
Uniform inferred-template descriptions: 0.
Auto-extracted runtime rows: 0.
Empty quality rows: 0; unrecognized runtime trust rows: 0; PendingManualReview rows: 0.
Manifest dispositions: DiagnosticOnlyKeep 349; SupersededRemove 13; PromoteAndRewrite 65; RetainReviewed 259; Quarantine 2261.
Exact key + appliesTo duplicates: 0.
Targeted verification: surface-focused BuiltIn/Completion/Hover/Quick Peek/Diagnostics/trust/highlighting tests passed 900/900.
Package verification: restore passed; Debug build passed with 0 warnings / 0 errors; full non-UI suite passed 2274/2274; IdeOnly clean package hygiene passed.
Authoritative contract: Docs/Codex_RA2IniEditor_IDE_FR_DQ_4_PlaceholderRetirement.md
Audit/manifest: Docs/FieldRegistryPlaceholderRetirementAudit_2026-07-20.md and Docs/FieldRegistryPlaceholderRetirementCandidates_2026-07-20.csv
Context capsule: Docs/ContextCapsule_FR_DQ_4.md
```

The narrow Completion change excludes only VerifiedGuardrail, Obsolete, NonExistent and PseudoField trust levels from field-name candidates. Lookup, Hover and Diagnostics still retain those facts. Real BuiltIn AA/AG regression confirms Unit/Vehicle completion hides diagnostic guardrails while Projectile completion retains canonical AA/AG.

FR-DQ-4H is complete. Real v3.2 tests confirm Projectile AA/AG remain available in Completion, Vehicle AA/AG guardrails remain hidden from key Completion but available to Hover/Quick Peek/Diagnostics, and a fixed Vehicle sample reports exactly three quarantined legacy rows as Unknown Key. No repository `.ini` corpus exists, so real-project occurrence counts could not be measured; the representative delta is explicitly limited to `+3` warnings for the fixed three-row sample. Final runtime JSON SHA-256 is `0A4024853D78CA57A13796E5C405ECDF86D4BED7124758891A0E491524B81A3C`; final manifest SHA-256 is `696384F56045B18047B24AC25C28FB54433400C36B90B9A5F8EF682D13404EB1`.

No public API, provider priority, lookup hierarchy, Completion commit/value candidates, Hover source, Quick Peek, Diagnostics core, parser, save behavior, Shell/XAML, project file or legacy behavior has changed.

---

## AI-REL-3 ProviderTrustPrivacyAndResourceHardening completed

```text
Contract state: Confirmed by user on 2026-07-20 and completed through AI-REL-3I.
Risk: R4 network/model/privacy/cost/observability boundary; authorized only within the confirmed contract and per-card stop rules.
Current runtime baseline: AI-REL-3 completed; AI-REL-2 failure taxonomy and AI-REL-1 context isolation remain authoritative within it.
Confirmed product direction: remove Mock, expose DeepSeek V4 Flash / V4 Pro, default V4 Flash.
Official API facts verified on 2026-07-20: model ids are deepseek-v4-flash and deepseek-v4-pro; V4 thinking defaults enabled unless explicitly disabled.
Confirmed behavior: product/production Fake removed; test-only stubs retained in tests; DEEPSEEK_MODEL ceases to override UI selection; both models explicitly use non-thinking mode; no automatic retry.
Additional package scope: endpoint/config trust, outbound secret scrub and prompt budgets, max output tokens, response invariants, request-local safe diagnostics, Shell history/Markdown bounds, loopback and dynamic failure verification.
Authoritative confirmed contract: Docs/Codex_RA2IniEditor_IDE_AI_REL_3_ProviderTrustPrivacyResource.md
```

AI-REL-3 is complete. Production Mock/Fake and environment model authority are removed; the typed Flash/Pro catalog defaults to Flash; one immutable configuration snapshot governs both UI status and client construction; endpoint, numeric, privacy and prompt/output resource boundaries are enforced; response factories and request-local safe diagnostics are frozen. 3G targeted tests passed 51/51, 3H loopback/failure tests passed 48/48, final restore/build passed with 0 warnings and 0 errors, and the full suite passed 2171/2171. Runtime AI-panel smoke confirmed the two-item selector, Flash default and safe status/footer; user-authorized minimal live requests for Flash and Pro each passed once. The 3-0 rollback archive is preserved under a distinct filename. No AI-REL-4 behavior is authorized without a new contract.

AI-REL-3 changed only the approved AI runtime, AI-panel XAML/wiring, tests and documentation. Project files, Field Registry, parser, completion, Hover, diagnostics semantics, save behavior, Shell main layout and legacy remained unchanged.

---

## AI-REL-2 Continuous Failure Taxonomy completed

```text
Package completed: AI-REL-2A through AI-REL-2E.
Failure model: internal Ra2AiFailureKind distinguishes missing configuration, authentication/authorization, rate limiting, request rejection, provider timeout, service unavailable, network/proxy, total timeout, streaming idle timeout, protocol error, response too large, and unknown failures.
Transport: DeepSeekRa2AiClient classifies failures without reading provider error bodies or exposing raw exception messages; user cancellation, total timeout, and streaming idle timeout use request-local first-signal-wins attribution.
Presentation: Shell uses fixed safe Chinese guidance selected by DeepSeekRa2AiFailureUiMessageFormatter; partial responses retain visible text and add an incomplete warning.
Compatibility: response kind, stream finish kind, Pipeline, request lifecycle, context eligibility, restore-only recovery, default timeouts, SSE payload/parser, and existing AutomationIds remain authoritative.
Verification: build passed with 0 errors and one pre-existing test CS8602 warning; stage tests passed 10/10, 63/63, and 60/60; cross-stage tests passed 263/263; full tests passed 2115/2115; IdeOnly clean package passed with 903 files.
UI evidence: Debug WPF shell, AI panel, advanced model selector, explicit DeepSeek selection, one synthetic non-sensitive request, complete response, and terminal control reset passed.
Authoritative result: Docs/Codex_RA2IniEditor_IDE_AI_REL_2_FailureTaxonomy.md
```

AI-REL-2 introduces no external public API, serialization, persistence, automatic retry, `Retry-After`, request snapshots, new dependencies, XAML/layout/AutomationId changes, Field Registry changes, project-file changes, or legacy restoration.

Next entry is not automatic. Any retry policy must begin with a separate contract that explicitly revisits duplicate-request, cost, privacy, and idempotency risks.

---

## AI-REL-1 TimeoutRecovery completed

```text
AI-REL-1A completed: internal IsContextEligible added; conversation context now requires Completed && IsContextEligible; provider tests passed 15/15.
AI-REL-1B completed: each streaming response owns its initiating user card; non-Completed terminal rendering excludes both turns and exposes a restore-only action.
Recovery safety: “恢复提示词” only fills an empty prompt box, never auto-sends, never overwrites existing text, and warns that a later send may cost money.
AutomationIds: AiAssistant.RestorePromptButton; AiAssistant.RestorePromptStatus.
Verification: IDE-only build 0 warnings / 0 errors; AI/DeepSeek/Shell targeted tests 231/231; full tests 2083/2083; IdeOnly clean package passed with 898 files.
UI evidence: Debug WPF IDE, AI panel, and advanced model area opened successfully; on 2026-07-20 the user confirmed the manual restore, focus, non-overwrite, no-auto-send, and failed-context-isolation smoke passed.
Latest contract/result: Docs/Codex_RA2IniEditor_IDE_AI_REL_1_TimeoutRecovery.md
```

AI-REL-1-UI-VERIFY is resolved. No AI-REL-1 code, compatibility, TODO, or verification debt remains.

Next recommended phase: `AI-REL-2A FailureTaxonomyContract`. It is contract-only and must stop for user confirmation before any transport, response-contract, or Shell implementation.

No external public API, serialization, DeepSeek transport, SSE parser, Pipeline, PromptBuilder, request lifecycle, `ShellWindow.xaml`, Field Registry, completion, Hover, diagnostics, save preflight, project file, or legacy behavior changed.

---

## AI-STREAM-3 ShellIncrementalRendering completed

```text
Phase completed: AI-STREAM-3 ShellIncrementalRendering
Task Cards completed: AI-STREAM-3A thread-safe incremental buffer; AI-STREAM-3B request-owned Shell incremental rendering
Canonical path: Shell now calls Ra2AiAssistantPipeline.SendStreamingAsync and does not access DeepSeek transport directly
Message ownership: one InProgress assistant card is created per request and finalized in place; no control is created per delta
Dispatcher policy: pending deltas are coalesced every 50 ms or at a 512-character threshold, with at most one immediate flush queued
Rendering policy: streaming text uses lightweight Runs; the existing Markdown renderer runs once at terminal presentation
Terminal authority: Ra2AiResponse.Text remains canonical; successful buffer/response mismatches fail closed as Error and cannot enter conversation context
State mapping: Success -> Completed; Incomplete/Cancelled/Timeout -> Incomplete; ProviderError/MissingConfiguration/consumer failure -> Error
Scrolling: the chat follows updates only while already within 24 px of the bottom, so user-initiated upward scrolling is preserved
Lifecycle: AI-STREAM-0 single-request identity, cancel/busy rules, stale flush rejection, close cancellation, timer detachment, and session disposal remain authoritative
Automation: existing AI AutomationIds are preserved; Shell layout is unchanged apart from naming the existing chat ScrollViewer
Verification: buffer tests passed 5/5; AI/Shell targeted tests passed 68/68; IDE-only build passed with 0 errors and one pre-existing CS8602 test warning; full tests passed 2081/2081; Mock UI smoke passed; user-authorized live DeepSeek multi-delta plus cancel-after-partial smoke passed; IdeOnly clean package passed with 897 files
Clean package: artifacts/RA2IniEditor.IDE.SourceClean.zip
```

Live provider evidence: the Shell exposed InProgress with Generate disabled / Cancel enabled, showed ordered partial text in the same card, then preserved the partial response after cancellation as Incomplete with copy enabled. Conversation context retained only the completed user turn. No editor file was open or modified during the smoke.

The AI-STREAM-0 through AI-STREAM-3 line is complete. Next product phase requires user selection; timeout retry policy and broader XAML polish remain separate contract-gated topics.

No public API, DeepSeek transport, SSE parser, PromptBuilder, Field Registry, completion, Hover, Quick Peek, diagnostics, save preflight, project file, or legacy behavior changed.

---

## AI-STREAM-2 StreamingTransportAndPipelineIntegration completed

```text
Phase completed: AI-STREAM-2 StreamingTransportAndPipelineIntegration
Task Cards completed: AI-STREAM-2A internal streaming client/response contract; AI-STREAM-2B DeepSeek SSE transport; AI-STREAM-2C Pipeline delta pass-through
IRa2AiClient: existing SendAsync preserved; SendStreamingAsync and ordered Ra2AiContentDeltaHandler added as internal experimental contracts
Response contract: Ra2AiResponseKind.Incomplete added; Ra2AiResponse can retain partial text and Ra2AiStreamFinishKind without treating it as success
DeepSeek request: stream=true, Accept: text/event-stream, ResponseHeadersRead
Canonical transport: legacy SendAsync reuses the SSE transport and maps Incomplete to the existing ProviderError-compatible surface
Timeouts: existing 120-second application total timeout remains authoritative; after the first content delta, a 60-second inter-content idle timeout starts and resets only after each delta callback completes
Resource bounds: accumulated assistant content is limited to 1 MiB; malformed, interrupted, oversized, cancelled, and timed-out streams retain partial text only in non-success responses
Resource ownership: DeepSeek client owns/disposes request, response, response stream, and StreamReader; parser remains caller-reader-neutral
Consumer boundary: delta callbacks are ordered and non-concurrent; callback failures are not misreported as provider failures
Pipeline: SendStreamingAsync builds one canonical Ra2AiRequest and passes deltas through without WPF/Dispatcher dependencies
Verification: combined AI streaming/lifecycle tests passed 74/74; IDE-only build passed with 0 warnings / 0 errors; full tests passed 2076/2076
Next recommended phase: AI-STREAM-3 ShellIncrementalRendering
```

AI-STREAM-3 must use `Ra2AiAssistantPipeline.SendStreamingAsync`, create one `InProgress` assistant turn per active request, coalesce Dispatcher updates by a bounded time/character threshold, and finalize the same turn as `Completed`, `Incomplete`, or `Error`. Only `Ra2AiResponseKind.Success` may enter completed conversation context. Request identity, cancellation, stale-completion rejection, and busy-state rules from AI-STREAM-0 remain authoritative.

No Shell/XAML, AutomationId, external public API, Field Registry, completion, Hover, Quick Peek, diagnostics, save preflight, project file, or legacy behavior was changed.

---

## AI-STREAM-1 / AI-STREAM-1A StreamEventAndSseParser completed

```text
Phase completed: AI-STREAM-1 StreamEventAndSseParser
Hardening addendum completed: AI-STREAM-1A TerminalSemanticsAndProtocolHardening
Internal event contract: ordered ContentDelta events followed by one Completed event emitted only after data: [DONE]
Terminal metadata: Completed carries Stop / Length / ContentFilter / ToolCalls / InsufficientSystemResource / Unknown
Protocol validation: optional chunk object and stream id are validated when present; stream ids must remain stable; index 0 is selected independently of array order
Compatibility: a single choice without index remains accepted; keepalive comments, role-only, reasoning-only, and empty-choices usage frames remain non-content
Resource boundary: one accumulated SSE data event is limited to 1 MiB
Failure boundary: malformed JSON/shape, identity mismatch, oversized event, cancellation, and EOF before [DONE] remain explicit failures
Ownership: parser does not dispose the caller-owned TextReader
Verification: parser tests passed 24/24; IDE-only build passed with 0 warnings / 0 errors; full tests passed 2058/2058
AI-STREAM-1/1A was followed by the completed AI-STREAM-2 package recorded above.
```

AI-STREAM-2 must preserve these additional contract rules:

- Only `Stop` plus non-empty accumulated assistant content may become `Ra2AiResponseKind.Success`.
- `Length`, `ContentFilter`, `ToolCalls`, `InsufficientSystemResource`, `Unknown`, missing content, timeout, cancellation, malformed data, and connection loss must not enter completed conversation context.
- Partial visible content may be retained for diagnosis/display, but must be marked incomplete/error rather than success.
- Keep the current application-level total timeout authority and define first-meaningful-content plus inter-content idle timeout explicitly; SSE keepalive proves connection activity but is not assistant content.
- Add a bounded accumulated-response limit and fragmented UTF-8 / real `StreamReader` integration tests.
- The transport owns and disposes `HttpResponseMessage`, response stream, and reader.

AI-STREAM-3 must batch Dispatcher updates instead of dispatching every token. The recommended contract is time/size coalescing (approximately 30–60 ms or a bounded character threshold) while preserving exact final text and the request identity rules established by AI-STREAM-0.

No Shell/XAML, public API, Field Registry, completion, Hover, Quick Peek, diagnostics, save preflight, project file, or legacy behavior was changed.

---

## AI-STREAM-0 RequestLifecycleHardening completed

```text
Phase completed: AI-STREAM-0 RequestLifecycleHardening
Timeout authority: application-level 120 seconds by default; DEEPSEEK_TIMEOUT_SECONDS remains the override
Shared HttpClient timeout: disabled (InfiniteTimeSpan), so it no longer races the application timeout
Request lifecycle: one active request; cancel keeps the UI busy until the matching request finishes
Window close: requests cancellation for the active AI request
Conversation context: only Completed turns are reused; cancelled/incomplete/error messages remain visible but are excluded
Streaming transport/UI: not implemented in this phase
Verification: IDE-only restore passed; build passed with 0 warnings / 0 errors; full tests passed 2034/2034
Next recommended phase: AI-STREAM-1 StreamEventAndSseParser
```

No XAML, AutomationId, public API, Field Registry data/provider priority, completion, Hover, Quick Peek, diagnostics, parser, save preflight, project file, or legacy behavior was changed.

---

## AI-TimeoutAndContextDisclosure completed

```text
Phase completed: AI-TimeoutAndContextDisclosure
DeepSeek timeout result: explicit Ra2AiResponseKind.Timeout
Default timeout: 120 seconds; DEEPSEEK_TIMEOUT_SECONDS remains the override
Cancellation: remains distinct from timeout
Automatic retry: not implemented, to avoid duplicate requests and cost
Context disclosure: now describes the bounded current-document, evidence, diagnostics, and recent-conversation context actually used
Whole-project context / file mutation: not added
```

Targeted adapter, factory, and Shell boundary tests passed. BuiltIn Field Registry, provider priority, completion, hover, diagnostics, parser, save preflight, project files, and legacy behavior were not changed.

---

## FR-DQ-3H-LightweightHoverTrustAndDiagnosticPolish completed

```text
Phase completed: FR-DQ-3H-LightweightHoverTrustAndDiagnosticPolish
Baseline: FR-DQ-3G-P0P1P2-UnifiedBuiltInMerge
Goal: preserve field quality metadata and expose trust information without making Hover noisy
Hover behavior: verified fields have no extra trust badge; inferred/guardrail/obsolete/non-existent fields get at most one short footnote
Quick Peek: shows trust badge and detail section
Diagnostics: FIELD_WRONG_CONTEXT / FIELD_OBSOLETE_KEY / FIELD_NON_EXISTENT_KEY / FIELD_PSEUDO_FIELD added for actionable risk cases
BuiltIn JSON: unchanged in this phase
Runtime BuiltIn field count: 4878
needs-more-evidence rows: 0
schema.type=Text rows: 0
dotnet restore/build/test: not run because dotnet CLI is unavailable in patch environment
static JSON validation: passed
```

Files changed in this phase:

- `RA2IniEditor.Core/Schema/Ra2FieldSchema.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/FieldRegistryFieldDto.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/LocalFieldRegistryLoader.cs`
- `RA2IniEditor.IDE/FieldTrust/*`
- `RA2IniEditor.IDE/Language/Ra2HoverProvider.cs`
- `RA2IniEditor.IDE/ViewModels/FieldDetails/Ra2FieldDetailsViewModel.cs`
- `RA2IniEditor.IDE/Views/FieldQuickPeek/Ra2FieldQuickPeekWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldBrowser/Ra2AddPropertyWindow.xaml`
- `RA2IniEditor.IDE/Diagnostics/Ra2FieldDiagnosticService.cs`
- related tests and handoff docs

Hover remains lightweight by design. No save, completion commit, provider priority, parser, AI provider, or legacy behavior was changed.

---


## FR-DQ-3G-P0P1P2-UnifiedBuiltInMerge completed

```text
Phase completed: FR-DQ-3G-P0P1P2-UnifiedBuiltInMerge-ManualApply
Baseline: FR-DQ-3F-InferredBacklogRecovery
Runtime BuiltIn field count: 4878
DirectFix applied: 308
Guardrail applied: 129
KeepInferred applied: 103
RemoveOrBacklog removed: 231
RemoveOrBacklog safety-kept: 8
needs-more-evidence rows: 0
schema.type=Text rows: 0
Direct Hover-risk rows: 0
dotnet restore/build/test: not run in patch environment because dotnet CLI is unavailable
static JSON validation: passed after each merge bucket
```

Files changed in this phase:

- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json`
- `Docs/FieldRegistryDescriptionVerification_P0P1P2UnifiedMerge_2026-06-03.md`
- `Docs/FieldRegistryP0P1P2UnifiedMerge_RemovedOrBacklog_2026-06-03.csv`
- `Docs/Codex_CurrentPhase.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `AGENTS.md`

No provider priority, lookup/fallback/enrichment, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, user Global active pack, or legacy behavior was changed.

---

## FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply completed

```text
Phase completed: FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply
Baseline: FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply
Rows affected: 273
Source-backed rows: 0
NeedsMoreEvidence guardrail rows: 273
Direct Hover-risk rows: 273 -> 0
Direct placeholder rows: 251 -> 0
Exact integer generic rows: 22 -> 0
Exact numeric generic rows: 0 -> 0
NeedsMoreEvidence rows: 1594 -> 1867
Clean package target: IdeOnly
Next recommended phase: FR-DQ-3B-FinalHoverQualityAudit
```

Files changed in this phase:

- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json`
- `RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs`
- `Docs/FieldRegistryDescriptionVerification_ResidualHoverRiskBurnDown_2026-06-03.md`
- `Docs/FieldRegistryHoverQualityScan_2026-06-03.md`
- `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `AGENTS.md`

No provider priority, lookup/fallback/enrichment, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project-file, user Global active pack, or legacy behavior was changed.

---

## FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply completed

```text
Phase completed: FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply
Baseline: FR-DQ-2Y-ArtVoxelTerrainSound-MegaBatch-ManualApply
Rows affected: 200
Source-backed rows: 192
Non-canonical guardrail rows: 4
NeedsMoreEvidence guardrail rows: 4
Direct Hover-risk rows: 473 -> 273
Direct placeholder rows: 430 -> 251
Exact integer generic rows: 43 -> 22
Exact numeric generic rows: 0 -> 0
Clean package target: IdeOnly
Next recommended phase: FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply
```

Files changed in this phase:

- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json`
- `RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs`
- `Docs/FieldRegistryDescriptionVerification_AresPhobosExtensions_2026-06-03.md`
- `Docs/FieldRegistryHoverQualityScan_2026-06-03.md`
- `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `AGENTS.md`

No provider priority, lookup/fallback/enrichment, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project-file, user Global active pack, or legacy behavior was changed.

---

## FR-DQ-2Y-ArtVoxelTerrainSound-MegaBatch-ManualApply completed

```text
Phase completed: FR-DQ-2Y-ArtVoxelTerrainSound-MegaBatch-ManualApply
Baseline: FR-DQ-2X-SuperWeaponSideCountryUi-MegaBatch-ManualApply
Rows affected: 154
Source-backed rows: 14
Non-canonical guardrail rows: 8
NeedsMoreEvidence guardrail rows: 132
Direct Hover-risk rows: 627 -> 473
Direct placeholder rows: 583 -> 430
Exact integer generic rows: 44 -> 43
Clean package target: IdeOnly
Next recommended phase: FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply
```

Files changed in this phase:

- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json`
- `RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs`
- `Docs/FieldRegistryDescriptionVerification_ArtVoxelTerrainSound_2026-06-03.md`
- `Docs/FieldRegistryHoverQualityScan_2026-06-03.md`
- `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `AGENTS.md`

No provider priority, lookup/fallback/enrichment, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy behavior changed.


## Active Baseline

```text
RA2IniEditor.IDE-only
v0.4.96-pre.2 IDE-only Source Package Stabilization
A15-1R2 accepted
A15-2A completed
A15-2B completed
A15-2B-P completed
A15-2B-P2 completed
A15-2B-P3 completed
A15-2B-P4 completed
A15-2D completed
A15-2E-1 completed
A15-2E-2 completed
AI-0 completed
AI-1A completed
AI-1B completed
AI-1B-P2 completed
AI-1B-P3 completed
AI-1B-P4 completed
AI-1C completed
AI-2A completed
AI-2B completed
AI-2C completed
AI-2D completed
AI-3A completed
AI-3B completed
AI-4A completed
AI-4B completed
AI-4C completed
AI-4D completed
AI-4D-Fix completed
AI-4E-1 completed
AI-4E-2 completed
AI-4E-2-P completed
AI-4E-2-P2 completed
AI-4E-2-P3R completed
AI-4E-2-P4 completed
AI-4E-2-DeepSeekEndpointFix completed
AI-5B completed
AI-5C completed
AI-5C-P completed
AI-5C-P2 completed
AI-5C-P3 completed
AI-5B-Fix2 completed
AI-6A completed
AI-6B completed
AI-6C completed
AI-6D completed
AI-6E completed
AI-6F completed
Icon-0A completed
Icon-0B completed
Icon-0C completed
Icon-S1 completed
Icon-S2A completed
Icon-S2B completed
Icon-S2B-P completed
Icon-S2B-P2 completed
Icon-S2B-P3 completed
FR-DQ-2B-Apply completed
FR-DQ-2C-Prep-Fix completed
FR-DQ-2C-Verify-ManualApply completed
FR-DQ-2F-AI-LowQuality-ManualApply completed
FR-DQ-2F-AI-CrossContext-ManualApply completed
FR-DQ-2G-AI-Page-Batch-ManualApply completed
FR-DQ-2H-TechnoTypes-Common-ManualApply completed
FR-DQ-2I-TechnoTypes-CombatMobility-ManualApply completed
FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply completed
FR-DQ-2K-TechnoTypes-ProductionVeterancy-ManualApply completed
FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply completed
FR-DQ-2M-TechnoTypes-WeaponTargeting-ManualApply completed
FR-DQ-2N-TechnoTypes-AircraftAndSpawn-ManualApply completed
FR-DQ-2O-TechnoTypes-JumpjetAndFlightTuning-ManualApply completed
FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply completed
FR-DQ-2Q-TechnoTypes-RepairPowerCaptureFactoryRadar-BigBatch-ManualApply completed
FR-DQ-2R-WeaponCore-BigBatch-ManualApply completed
FR-DQ-2S-WarheadCore-BigBatch-ManualApply completed
FR-DQ-2T-ProjectileCore-BigBatch-ManualApply completed
FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply completed
FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply completed
FR-DQ-2W-TechnoTypesRemaining-UnresolvedGuardrail-MegaBatch-ManualApply completed
```

Recent reported verification:

```text
restore: not run in this environment
build: not run in this environment
test: not run in this environment because dotnet CLI is unavailable
static JSON validation: passed
manual clean package: passed for current phase
legacy: not restored
Shell main layout: unchanged
Field Registry semantics: FR-DQ-2R Weapon core rows and same-domain Phobos WeaponType extensions updated only; provider priority / runtime lookup unchanged
```

## Active Task

```text
FR-DQ-2W-TechnoTypesRemaining-UnresolvedGuardrail-MegaBatch-ManualApply completed. This phase converted 1413 remaining exact Techno placeholder/generic rows to explicit NeedsMoreEvidence guardrails and wrote the complete unresolved list to Docs/FieldRegistryUnresolvedRows_2026-06-03.md. No Field Registry provider priority, runtime lookup, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy files were changed. BuiltIn v3.2 field count remains 5069. Direct Hover-risk rows are now 864. Direct placeholder rows are now 802; exact integer generic rows are now 62; exact numeric generic rows are now 0.

Next recommended phase: FR-DQ-2X-SuperWeaponSideCountryUIMegaBatch-ManualApply.
```

## Recently Completed Task Documents

```text
Docs/Codex_RA2IniEditor_IDE_A15_2B_P_VisualPolish_Implementation.md
Docs/Codex_RA2IniEditor_IDE_A15_2B_P2_FieldRegistry_CustomChrome.md
Docs/Codex_RA2IniEditor_IDE_A15_2B_P3_ManagerScroll_Localization.md
Docs/Codex_RA2IniEditor_IDE_A15_2B_P4_CleanupPreview_Demotion.md
Docs/FieldEditorAndLearningChromeContract.md
Docs/Codex_RA2IniEditor_IDE_A15_2D_FieldEditor_AllowedValues_CustomChrome.md
Docs/FieldLearningWizardWorkflowContract.md
Docs/Codex_RA2IniEditor_IDE_A15_2E_1_FieldLearningWizard_CustomChrome.md
Docs/Codex_RA2IniEditor_IDE_A15_2E_2_FieldLearningWizard_WorkflowLayout.md
Docs/AiAgentPanelPlacementContract.md
Docs/AiAssistantArchitecture.md
Docs/AiAssistantSafetyContract.md
Docs/AiAssistantRightToolWellImplementationContract.md
Docs/Codex_RA2IniEditor_IDE_AI_1B_RightToolWell_EmptyAiPage.md
Docs/Codex_RA2IniEditor_IDE_AI_1B_P2_ChatComposerAdvanced.md
Docs/Codex_RA2IniEditor_IDE_AI_1B_P3_ChatPanelVisualRefinement.md
Docs/Codex_RA2IniEditor_IDE_AI_1B_P4_ComposerMinimalism_WrappingFix.md
Docs/Codex_RA2IniEditor_IDE_AI_1C_MockResponseDisplay.md
Docs/AiAssistantContextProviderContract.md
Docs/Codex_RA2IniEditor_IDE_AI_2B_CaretContextProvider.md
Docs/Codex_RA2IniEditor_IDE_AI_2C_FieldRegistryEvidenceRetrieval.md
Docs/Codex_RA2IniEditor_IDE_AI_2D_DiagnosticsSummary.md
Docs/Codex_RA2IniEditor_IDE_AI_3A_PromptBuilderContract.md
Docs/AiAssistantPromptBuilderContract.md
Docs/Codex_RA2IniEditor_IDE_AI_3B_PromptBuilderImplementation.md
Docs/AiAssistantDeepSeekAdapterContract.md
Docs/Codex_RA2IniEditor_IDE_AI_4B_ClientAbstraction_FakeBoundary.md
Docs/Codex_RA2IniEditor_IDE_AI_4C_FakeClientSendFlow.md
Docs/Codex_RA2IniEditor_IDE_AI_4D_DeepSeekAdapterImplementation.md
Docs/Codex_RA2IniEditor_IDE_AI_4D_Fix_EnvironmentOnlyApiKey.md
Docs/AiAssistantLiveProviderContract.md
Docs/Codex_RA2IniEditor_IDE_AI_4E_1_ProviderModeUiState.md
Docs/Codex_RA2IniEditor_IDE_AI_4E_2_LiveProviderSelection.md
Docs/Codex_RA2IniEditor_IDE_AI_4E_2_P_ProviderAdvancedLayoutFix.md
Docs/Codex_RA2IniEditor_IDE_AI_4E_2_P2_ComposerEnterToSend.md
Docs/Codex_RA2IniEditor_IDE_AI_4E_2_P3R_MinimalModelComboBox.md
Docs/Codex_RA2IniEditor_IDE_AI_4E_2_P4_ChatActionPlacementCleanup.md
Docs/Codex_RA2IniEditor_IDE_AI_4E_2_P4_CopyPerAssistantMessage_Addendum.md
Docs/AiAssistantStableDraftOutputContract.md
Docs/Codex_RA2IniEditor_IDE_AI_5B_StableDraftPromptBuilderUpdate.md
Docs/Codex_RA2IniEditor_IDE_AI_5B_NoHallucinatedFields_Addendum.md
Docs/Codex_RA2IniEditor_IDE_AI_5C_MarkdownRendering_CodeBlockCopy_Implementation.md
Docs/Codex_RA2IniEditor_IDE_AI_5C_P_MarkdownRichRendering.md
Docs/Codex_RA2IniEditor_IDE_AI_5C_P2_MarkdownTableInlineRenderingFix.md
Docs/Codex_RA2IniEditor_IDE_AI_5C_P3_InlineCodeRenderingFix.md
Docs/AiAssistantConversationContextContract.md
Docs/Codex_RA2IniEditor_IDE_AI_6B_ConversationContextBoundedExtraction.md
Docs/Codex_RA2IniEditor_IDE_AI_6C_CurrentSubjectExtraction.md
Docs/Codex_RA2IniEditor_IDE_AI_6D_PromptBuilderConversationSubjectIntegration.md
Docs/Codex_RA2IniEditor_IDE_AI_6E_ContextSummaryCurrentSubjectUi.md
```

## Required Context Documents

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/FieldRegistrySurfacesUiContract.md
```

## Next Recommended Codex Prompt

```text
Please read AGENTS.md, Docs/RA2IniEditor_IDE_Full_Codex_Context.md, Docs/FieldRegistryDescriptionVerification_BatchB_Input.md, Docs/FieldRegistryDescriptionSourcePolicy.md, and the specific FR-DQ-2C-Verify task document when it is created or named.

Do not modify files yet. First summarize the task goal, allowed files, forbidden files, semantic boundaries, verification source trust rules, validation commands, and whether online/source verification is required before any patch plan. Stop and wait for approval.
```

## Do Not Start Yet

```text
A15-2E-3 Field Learning Wizard localization and warning/disabled reason polish
A15-2F Apply / Rollback confirmation redesign
AI panel live provider switching / API key UI / settings persistence unless explicitly approved by a dedicated AI adapter phase
Apply / Insert
FR-DQ-2D Batch B Patch Plan or FR-DQ-2E Batch B Apply before FR-DQ-2C-Verify source verification is completed
```

## Completed Criteria For AI-1B

```text
The existing right-side ProjectExplorerPanel now contains a conservative Right Tool Well frame.
Section Tree / Navigator remains the default visible view.
ProjectExplorerTreeView keeps its x:Name, Shell.ProjectExplorer AutomationId, ProjectExplorer.Items binding, selection handler, and code-behind accessibility.
RightToolWell.Root, RightToolWell.SectionTab, RightToolWell.AiTab, and RightToolWell.ActiveView were added.
AI Assistant exists only as a second right-side skeleton view with placeholder regions and disabled Generate/Cancel/Copy/Clear controls.
AI opens only through explicit right-tool/menu commands and closes back to Section Tree.
No MockRa2AiClient generation, DeepSeek, network, API key, context provider, prompt builder, Apply/Insert, file write, Field Registry write, or whole-project context was implemented.
restore/build/test/package passed.
```

## Completed Criteria For AI-1B-P2

```text
AI Assistant skeleton was refined from a form-like layout to a ChatGPT-style layout.
Most of the AI panel is now AiAssistant.ChatHistory with compact placeholder messages.
The bottom fixed composer is exposed as AiAssistant.Composer.
AiAssistant.PromptBox and AiAssistant.GenerateButton remain, with Generate visually acting as the disabled send placeholder.
AiAssistant.AdvancedButton, AiAssistant.AdvancedOptions, and AiAssistant.ModelSelector were added inside the composer side area.
AiAssistant.TaskKindSelector remains only as a disabled Advanced placeholder for AutomationId continuity, not as a prominent main control.
AiAssistant.ResponseArea and AiAssistant.DraftPreview remain as compact chat/history placeholders.
No MockRa2AiClient generation, DeepSeek client, network, API key, context provider, prompt builder, Apply/Insert, file write, Field Registry write, or whole-project context was implemented.
restore/build/test/package passed.
```

## Completed Criteria For AI-1B-P3

```text
AI Assistant skeleton was further refined to reduce WPF form feel.
The large top Close button was replaced by a compact header x button that preserves AiAssistant.CloseButton.
AiAssistant.ContextSummary is now a compact muted header line instead of a separate top block.
The primary middle area remains AiAssistant.ChatHistory.
AiAssistant.ResponseArea and AiAssistant.DraftPreview remain as compact elements inside chat history.
AiAssistant.Composer remains fixed at the bottom with PromptBox, Advanced, model placeholder, and disabled send button nearby.
AiAssistant.TaskKindSelector remains only as a disabled Advanced placeholder for AutomationId continuity.
No MockRa2AiClient generation, DeepSeek client, network, API key, context provider, prompt builder, Apply/Insert, file write, Field Registry write, or whole-project context was implemented.
restore/build/test/package passed.
```

## Completed Criteria For AI-1B-P4

```text
AI Assistant skeleton header/composer was simplified further.
The visible AI page top-right x close button was removed; users return to Section through RightToolWell.SectionTab.
The meaningless composer + utility placeholder was removed.
AiAssistant.PromptBox now supports wrapping and multiline input with TextWrapping=Wrap, AcceptsReturn=True, disabled horizontal scrolling, auto vertical scrolling, and compact height bounds.
AiAssistant.SafetyFooter remains but is moved below the composer input border and uses smaller muted text.
PromptBox, Advanced, model placeholder, and disabled send remain grouped near the composer input.
No MockRa2AiClient generation, DeepSeek client, network, API key, context provider, prompt builder, Apply/Insert, file write, Field Registry write, or whole-project context was implemented.
restore/build/test/package passed.
```

## Completed Criteria For AI-1C

```text
AI Assistant chat UI now supports deterministic local mock response display.
PromptBox non-empty send appends a local user message and a local mock assistant response, then clears the PromptBox.
Blank PromptBox send is a no-op and does not add chat messages.
Copy copies only the latest mock assistant response.
Clear removes local mock chat messages and restores the empty state.
Cancel remains disabled because no network or async provider call exists.
AiAssistant.UserMessageList, AiAssistant.AssistantMessageList, AiAssistant.LatestAssistantMessage, and AiAssistant.EmptyStateMessage were added.
No DeepSeek client, network, API key, context provider, prompt builder, Apply/Insert, editor text mutation, dirty-state mutation, Field Registry write, or whole-project context was implemented.
restore/build/test/package passed; latest package count 677.
```

## Completed Criteria For AI-2B

```text
Bounded current-document / caret context provider was added under RA2IniEditor.IDE/AI.
Provider reuses Ra2DocumentSnapshot, Ra2DocumentSemanticModel, Ra2CaretContextService, and Ra2CaretContext.
Context resolves current file display name, caret offset, caret line, current Section, current Key / Value, explicit selected text, and bounded nearby text.
Nearby text defaults to 5 lines before and after the caret line and is additionally capped by character count; it does not include the whole file for large files.
Comment lines, blank lines, no document, and missing semantic context return safe fallback context.
AiAssistant.ContextSummary now displays current bounded context when AI opens or Generate is clicked.
Field evidence count remains 0 and diagnostic count remains 0 because Field Registry retrieval and diagnostics summary are deferred to AI-2C / AI-2D.
Generate still uses the AI-1C deterministic local mock response and does not modify editor text or dirty state.
No DeepSeek, network, API key, PromptBuilder, Field Registry retrieval evidence, diagnostics summary, Apply/Insert, file write, Field Registry write, whole-project context, auto-open, or auto-send behavior was added.
restore/build/test/package passed; latest package count 690.
```

## Completed Criteria For AI-2C

```text
Local Field Registry evidence retrieval was added under RA2IniEditor.IDE/AI.
Ra2FieldRegistryAiEvidenceProvider reuses the active IRa2FieldDefinitionProvider and IFieldRegistryProvenanceProvider passed from FieldRegistryRuntimeService.CurrentProvider / CurrentProvenanceProvider.
Current key exact lookup uses the existing provider TryGetField path; prompt and explicit selected text use bounded local key/display/alias/description matching.
Evidence is deduplicated, sorted by local match score, defaults to Top 8, and is hard-capped at Top 12.
Ra2AiContext now exposes FieldEvidence, FieldEvidenceCount, and FieldEvidenceTopKeysText as display/reference data.
AiAssistant.ContextSummary now shows field evidence count and top keys when available.
Diagnostics count remains 0 because diagnostics summary integration is deferred to AI-2D.
Generate still uses the AI-1C deterministic local mock response and does not modify editor text or dirty state.
No DeepSeek, network, API key, PromptBuilder, diagnostics summary, Apply/Insert, file write, Field Registry write, whole-project context, auto-open, or auto-send behavior was added.
restore/build/test/package passed; latest package count 690.
```

## Completed Criteria For AI-2D

```text
Bounded diagnostics summary integration was added under RA2IniEditor.IDE/AI.
Ra2CurrentFileAiDiagnosticSummaryProvider reads the current visible diagnostics snapshot passed from IssuesViewModel.Items and does not rerun diagnostics.
Diagnostic summaries filter to the current file and current snapshot version where available.
Summary priority is current caret line, then current key, then current Section, then a small current-file top summary.
Diagnostic summaries default to Top 5 and are hard-capped at Top 8.
Ra2AiContext now exposes Diagnostics and DiagnosticCount as display/reference data.
AiAssistant.ContextSummary now shows dynamic diagnostics count while preserving AI-2C field evidence count/top keys.
Diagnostics summary is advisory and does not alter diagnostics generation, Issues panel behavior, Save Preflight, editor text, dirty state, Field Registry, or parser behavior.
Generate still uses the AI-1C deterministic local mock response.
No DeepSeek, network, API key, PromptBuilder, Apply/Insert, diagnostic auto-fix, file write, Field Registry write, whole-project context, auto-open, or auto-send behavior was added.
restore/build/test/package passed; latest package count 695.
```

## Completed Criteria For AI-3B

```text
Deterministic PromptBuilder was added under RA2IniEditor.IDE/AI.
Ra2AiIntent defaults to Auto and supports the planned internal intents.
Ra2AiPromptBuildRequest accepts only UserPrompt, Intent, and the already-built Ra2AiContext.
Ra2AiRequest contains Intent, UserPrompt, and PromptText without provider-specific fields.
Ra2AiPromptBuilder builds fixed Application Rules, User Request, Current IDE Context, Field Registry Evidence, Diagnostics Summary, and Output Requirements sections.
Prompt text marks Field Registry evidence as advisory reference data, diagnostics as advisory summaries rather than auto-fix commands, generated INI as draft, and INI/project content as untrusted data rather than instructions.
PromptBuilder does not read files, inspect editor controls, query providers, rerun diagnostics, reload Field Registry, call network, or collect additional context.
No Shell wiring was needed and UI behavior remains unchanged.
No DeepSeek, network, API key, Apply/Insert, file modification behavior, Field Registry write, whole-project context, auto-open, or auto-send behavior was added.
restore/build/test/package passed; latest test count 1336 and package count 704.
```

## Completed Criteria For AI-4B

```text
AI client abstraction was added under RA2IniEditor.IDE/AI.
IRa2AiClient.SendAsync consumes the AI-3B Ra2AiRequest and requires a CancellationToken.
Ra2AiResponseKind supports Success, Cancelled, ProviderError, and MissingConfiguration.
Ra2AiResponse carries response text, optional error metadata, and IsSuccess.
FakeRa2AiClient returns deterministic success text based on intent and prompt lengths only.
FakeRa2AiClient supports configured cancelled, provider error, and missing configuration responses.
FakeRa2AiClient respects pre-cancelled CancellationToken before configured behavior.
FakeRa2AiClient does not read files, project state, editor controls, Field Registry providers, diagnostics services, environment variables, API keys, or network.
No XAML, Shell UI, ViewModel, AI panel send-flow wiring, DeepSeek, network, API key, Apply/Insert, file modification behavior, Field Registry write, whole-project context, auto-open, or auto-send behavior was added.
restore/build/test/package passed; latest test count 1343 and package count 712.
```

## Completed Criteria For AI-4C

```text
AI panel Generate / Send now uses the internal fake AI pipeline.
Non-empty PromptBox send appends a user message, builds bounded AI context, builds a Ra2AiRequest with Ra2AiPromptBuilder, calls IRa2AiClient through FakeRa2AiClient, appends the fake assistant response, and clears PromptBox.
Empty PromptBox remains a no-op.
Ra2AiAssistantPipeline was added as a small composition helper so ShellWindow.xaml.cs does not manually build prompt text.
ProviderError and MissingConfiguration responses are represented as error-style assistant messages.
Cancelled responses clear sending state and can display a compact cancellation message.
Cancel, Copy, and Clear remain local AI panel actions only.
AI panel layout, Section Tree default view, Project Explorer behavior, Advanced/model selector placeholder behavior, and absence of Apply/Insert are preserved.
No DeepSeek client, network, API key loading/UI, provider settings persistence, real model selector behavior, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1346 and package count 715.
```

## Completed Criteria For AI-4D

```text
DeepSeek-compatible AI client adapter was added under RA2IniEditor.IDE/AI.
DeepSeekRa2AiClient sits behind IRa2AiClient and consumes only the AI-3B Ra2AiRequest.
DeepSeekRa2AiClient sends only Ra2AiRequest.PromptText as user content in a minimal OpenAI-compatible chat completion request shape.
DeepSeekRa2AiClientOptions carries BaseUrl, ApiKey, Model, Timeout, and Temperature, with ToString redacting the API key.
The adapter uses injected HttpClient, so tests use fake HttpMessageHandler and do not require live DeepSeek, real network, or real credentials.
Missing API key / invalid options map to MissingConfiguration.
HTTP non-success, malformed JSON, missing assistant content, HttpRequestException, and timeout map to ProviderError.
Pre-cancelled and operation-cancelled requests map to Cancelled.
Errors do not include API key, raw prompt, raw response body, Authorization header, full context, selected INI text, nearby text, absolute paths, or environment variables.
AI panel still uses FakeRa2AiClient by default; no Shell UI / send-flow wiring provider switch was added.
No API key UI, settings persistence, real model selector behavior, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1357 and package count 719.
```

## Completed Criteria For AI-4D-Fix

```text
DeepSeek API key policy is environment-variable only for the first implementation.
DeepSeekRa2AiClientFactory was added under RA2IniEditor.IDE/AI.
DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment reads only DEEPSEEK_API_KEY, DEEPSEEK_BASE_URL, DEEPSEEK_MODEL, and DEEPSEEK_TIMEOUT_SECONDS.
DEEPSEEK_API_KEY is the only allowed API key source.
DEEPSEEK_BASE_URL, DEEPSEEK_MODEL, and DEEPSEEK_TIMEOUT_SECONDS are optional overrides.
Default BaseUrl is https://api.deepseek.com.
Default Model is deepseek-v4-pro.
Default Timeout is 60 seconds.
DeepSeekRa2AiClientOptions.ApiKey remains for tests and explicit construction.
No API key input in AI Advanced UI was added.
No local settings persistence or repository/project config key storage was added.
Environment-variable tests isolate and restore previous process environment values and do not depend on the user's real environment.
No Shell UI / send-flow wiring provider switch, API key UI, settings persistence, real model selector behavior, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1368 and package count 722.
```

## Completed Criteria For AI-4E-1

```text
AI Assistant Advanced area exposes provider mode UI state.
Provider selector shows Mock as the default option.
DeepSeek is visible as a disabled future option.
Provider status states that AI-4E-1 does not read API keys and does not send real requests.
DeepSeek environment hint states that future API key configuration uses DEEPSEEK_API_KEY.
AiAssistant.ProviderSelector, AiAssistant.ProviderStatus, and AiAssistant.DeepSeekEnvironmentHint AutomationIds were added.
Existing AiAssistant.AdvancedButton, AiAssistant.AdvancedOptions, AiAssistant.ModelSelector, AiAssistant.GenerateButton, AiAssistant.PromptBox, AiAssistant.SafetyFooter, AiAssistant.ChatHistory, and AiAssistant.ContextSummary AutomationIds are preserved.
Generate / Send still uses the AI-4C FakeRa2AiClient pipeline.
No live DeepSeek send flow, network call, API key loading, API key input, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1368 and package count 725.
```

## Completed Criteria For AI-4E-2

```text
AI Assistant Advanced provider selector can choose Mock or DeepSeek.
Mock remains the default provider.
Mock provider continues to use the AI-4C FakeRa2AiClient pipeline.
DeepSeek is used only after explicit user selection and explicit Generate / Send.
DeepSeek provider is constructed through DeepSeekRa2AiClientFactory.CreateClientFromEnvironment(), which creates options from DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment().
DeepSeek API key configuration remains environment-variable only through DEEPSEEK_API_KEY, with optional DEEPSEEK_BASE_URL, DEEPSEEK_MODEL, and DEEPSEEK_TIMEOUT_SECONDS handled by the factory.
ShellWindow.xaml.cs does not directly read environment variables and does not contain HttpClient construction.
MissingConfiguration, ProviderError, Cancelled, timeout-like provider errors, and unhandled exceptions are mapped to sanitized chat messages.
DeepSeek success output is appended as advisory assistant text only.
Busy state still blocks duplicate sends, and Cancel cancels the current request and clears busy state.
No API key input UI, save API key button, settings persistence, project/local API key storage, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1370 and package count 726.
```

## Completed Criteria For AI-4E-2-P

```text
AI Assistant Advanced provider selector no longer uses a ComboBox popup.
Provider selection is a compact Mock / DeepSeek RadioButton group inside AiAssistant.ProviderSelector.
AiAssistant.ProviderMockOption and AiAssistant.ProviderDeepSeekOption were added.
Mock remains the default provider and DeepSeek still requires explicit user selection.
AiAssistant.ModelSelector is preserved as compact read-only model text, not a disabled ComboBox.
AiAssistant.TaskKindSelector is preserved as compact read-only intent text, not a disabled ComboBox.
AiAssistant.ProviderStatus and AiAssistant.DeepSeekEnvironmentHint keep TextWrapping=Wrap.
Provider behavior from AI-4E-2 is unchanged.
No API key input UI, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1370 and package count 728.
```

## Completed Criteria For AI-4E-2-P2

```text
AiAssistant.PromptBox now has Enter-to-send behavior through AiAssistantPromptBox_OnPreviewKeyDown.
Enter without Shift routes to the existing GenerateAiAssistantResponse send method.
Shift+Enter remains normal TextBox newline behavior.
Empty / whitespace-only prompts remain no-op through the existing send guard.
Busy state still blocks duplicate sends through the existing send guard.
The key handler does not build context, build prompts, select providers, or call clients directly.
Mock / DeepSeek provider behavior from AI-4E-2 is unchanged.
No Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context beyond explicit Enter/Send action, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1370 and package count 728.
```

## Completed Criteria For AI-4E-2-P3R

```text
AI Assistant Advanced area now exposes only one compact model selector.
AiAssistant.ModelSelector is restored as a ComboBox with Mock and DeepSeek options.
Mock remains selected by default.
DeepSeek remains an explicit user selection before the live DeepSeek send flow is used.
Permanent Provider, Status, Intent, API Key, BaseUrl, and Timeout rows were removed from Advanced.
MissingConfiguration for unconfigured DeepSeek remains shown only as a sanitized chat message after selecting DeepSeek and sending.
Provider behavior from AI-4E-2 is unchanged and AI-4E-2-P2 Enter-to-send remains unchanged.
No API key input UI, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context beyond explicit Enter/Send action, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1370 and package count 729.
```

## Completed Criteria For AI-4E-2-P4

```text
AI Assistant Advanced area still exposes only one compact AiAssistant.ModelSelector ComboBox.
Cancel / Copy / Clear are no longer visible inside Advanced.
AiAssistant.ClearButton is placed in AiAssistant.ChatHistoryActions above the chat history and preserves clear-chat behavior.
AiAssistant.CancelButton is placed in the composer send area and preserves cancellation behavior.
Global latest-response copy UI and CopyLatestAiAssistantResponse were removed.
Every assistant message card created by AddAiAssistantMessage now includes an AiAssistant.AssistantMessageCopyButton.
Each assistant-message copy button copies only that specific assistant message text.
User messages do not get copy buttons.
Provider behavior from AI-4E-2 is unchanged and AI-4E-2-P2 Enter-to-send remains unchanged.
No API key input UI, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context beyond explicit Enter/Send action, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1370 and package count 731.
```

## Completed Criteria For AI-4E-2 DeepSeek Endpoint Hotfix

```text
DeepSeekRa2AiClientOptions now normalizes base URL values to the /chat/completions endpoint used by DeepSeek's OpenAI-compatible API.
Default DEEPSEEK_BASE_URL missing path behavior now sends to https://api.deepseek.com/chat/completions.
Explicit full chat completions endpoints remain unchanged and are not appended twice.
DeepSeekRa2AiClient still parses assistant text from choices[0].message.content.
Tests cover default base URL endpoint normalization, full endpoint preservation, and choices[0].message.content parsing.
Malformed JSON and missing assistant content still map to ProviderError.
API key policy remains environment-variable only through DEEPSEEK_API_KEY.
No API key UI, settings persistence, provider selector semantic change, send-flow semantic change, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
Release full test passed; latest test count 1373. Debug build/test was blocked by running RA2IniEditor.IDE process locking Debug output DLLs.
```

## Completed Criteria For AI-5B

```text
Ra2AiPromptBuilder now appends Stable INI Draft Rules to the deterministic prompt.
The prompt tells draft/prototype generation to keep generated INI as draft/advisory text only and never claim Apply / Insert / Save / file write happened.
If faction / side / country / Owner is unspecified, the prompt forbids randomly choosing Allied / Soviet / Yuri / mod factions and requires TODO placeholders such as Owner=<TODO_OWNER>.
Clean copyable INI blocks are instructed to omit explanatory comments by default, with explanations, field rationale, assumptions, risks, warnings, and uncertainty outside code blocks.
Draft output is instructed to separate rulesmd.ini and artmd.ini blocks when both are relevant.
Newly referenced weapon / warhead / projectile / art / voxel / SHP / cameo / sound / animation / prerequisite IDs must be listed under "需要补充的定义".
No-hallucinated-field guidance was added: clean INI blocks should use only field keys confirmed by Field Registry Evidence, unconfirmed field keys stay out of clean drafts by default, and useful unconfirmed keys belong under "可选 / 使用前需验证".
The prompt explicitly distinguishes field keys from object IDs / values and allows new object IDs as values only when listed as follow-up definitions.
Existing prompt-injection boundaries, advisory Field Registry evidence, and advisory diagnostics rules are preserved.
PromptBuilder still consumes only UserPrompt, Intent, and the already-built Ra2AiContext; it does not read files, providers, diagnostics services, environment variables, UI controls, network, or API keys.
No DeepSeek adapter, provider selection, API key loading, AI panel UI, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1377 and package count 735.
```

## Completed Criteria For AI-5C

```text
AI Assistant assistant-message responses now parse Markdown fences for display while keeping the original assistant response text as the source of truth.
Ra2AiMarkdownBlock and Ra2AiMarkdownResponseParser were added under RA2IniEditor.IDE/AI.
The deterministic parser splits surrounding text and fenced code blocks, preserves language labels such as ini / rules / art, removes fence markers from code-block text, handles multiple code blocks, and falls back safely to plain text for unterminated fences.
Every assistant message still exposes AiAssistant.AssistantMessageCopyButton and copies the full assistant response text only.
Assistant messages with fenced code blocks render compact code cards with AiAssistant.CodeBlock, AiAssistant.CodeBlockLanguage, and AiAssistant.CodeBlockCopyButton.
Copy-code actions copy only the code content inside the fence and do not include Markdown fence markers.
User messages remain plain chat messages and do not get code-block copy actions.
Copy actions do not modify editor text, mark dirty, apply, insert, save, write files, or write Field Registry data.
Provider behavior, DeepSeek adapter behavior, API key loading, settings persistence, model/provider switching semantics, send flow, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics, diagnostics behavior, completion/hover/quick peek, save preflight, BuiltIn JSON, legacy files, and solution/project files are unchanged.
No AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added.
Debug restore passed; Debug build was blocked by running RA2IniEditor.IDE process locking Debug output DLLs. Release build passed with 0 warnings / 0 errors, Release tests passed with 1383 tests, and IdeOnly package passed with package count 739.
```

## Completed Criteria For AI-5C-P

```text
AI Assistant assistant-message responses now render a lightweight Markdown subset instead of showing all non-code Markdown as one raw text block.
Supported rendering includes # / ## / ### headings, normal paragraphs, unordered list items, ordered list items, and fenced code blocks.
Ra2AiMarkdownBlockKind was added under RA2IniEditor.IDE/AI, and Ra2AiMarkdownBlock now carries block kind plus heading level.
Ra2AiMarkdownResponseParser still performs deterministic parsing only; it does not execute code, interpret INI, validate drafts, or access providers/files.
Existing fenced code block behavior is preserved: code blocks keep language labels, code content excludes fence markers, multiple code blocks are supported, and unterminated fences safely fall back to plain text.
Code blocks continue to render as existing code cards with AiAssistant.CodeBlock, AiAssistant.CodeBlockLanguage, and AiAssistant.CodeBlockCopyButton.
Headings, paragraphs, and list rows use AiAssistant.MarkdownHeading, AiAssistant.MarkdownParagraph, and AiAssistant.MarkdownListItem.
Full assistant-message copy still copies the original full Markdown response through AiAssistant.AssistantMessageCopyButton.
Copy-code still copies only the code content inside the fence.
Unsupported Markdown falls back to paragraph text.
No WebView, NuGet package, project file dependency, Apply/Insert, automatic file modification, Field Registry write, Markdown-to-file conversion, automatic code insertion, draft validation, DeepSeek adapter behavior change, provider switching behavior change, API key UI, settings persistence, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, PromptBuilder behavior change, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
No AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added.
restore/build/test/package passed; latest test count 1389 and package count 741.
```

## Completed Criteria For AI-5C-P2

```text
AI Assistant assistant-message Markdown rendering now supports common GitHub-style pipe tables.
Pipe tables are detected as header row + separator row + optional body rows, cells are split by pipe delimiters and trimmed, malformed tables fall back safely to paragraph text, and tables inside fenced code blocks remain code content.
Rendered tables use a lightweight WPF Grid card with AiAssistant.MarkdownTable, AiAssistant.MarkdownTableHeader, AiAssistant.MarkdownTableRow, and AiAssistant.MarkdownTableCell.
Simple non-nested **bold** spans render as WPF inline Runs with FontWeights.Bold in headings, paragraphs, lists, and table cells.
Malformed or unsupported inline Markdown remains safe plain text.
Existing heading, paragraph, bullet, numbered, fenced code block, code block language, and copy-code behavior is preserved.
Full assistant-message copy still copies the original full Markdown response.
Copy-code still copies only the code content inside the fence and excludes Markdown fence markers.
No WebView, Markdig, NuGet package, project file dependency, Apply/Insert, automatic file modification, Field Registry write, Markdown-to-file conversion, automatic code insertion, draft validation, DeepSeek adapter behavior change, provider switching behavior change, PromptBuilder behavior change, API key UI/loading, settings persistence, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
No AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added.
Debug restore passed; Debug build was blocked by a running RA2IniEditor.IDE process locking Debug output DLLs. Release build passed with 0 warnings / 0 errors, Release tests passed with 1393 tests, and IdeOnly package passed with package count 742.
```

## Completed Criteria For AI-5C-P3

```text
AI Assistant rendered Markdown text now supports simple single-backtick inline code spans.
Inline code is rendered without visible backtick markers and uses a lightweight monospace inline style.
Inline code rendering is applied through the existing WPF inline text helper used by headings, paragraphs, list items, and table cells.
Fenced code blocks are unaffected because they render through the existing code-card path and do not call the inline text helper.
Unterminated inline backticks fall back safely to raw text.
Full assistant-message copy still copies the original full Markdown response, including inline backticks.
Code-block copy still copies only the fenced code content and excludes Markdown fence markers.
No separate inline-code copy action was added.
No provider behavior, DeepSeek adapter behavior, PromptBuilder behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, Markdown-to-file conversion, automatic code insertion, draft validation, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
No AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added.
restore/build/test/package passed; latest test count 1393 and package count 743.
```

## Completed Criteria For AI-5B-Fix2

```text
AI evidence retrieval now supports bounded draft evidence profiles.
Core profiles include UnitCore, VehicleCore, InfantryCore, BuildingCore, WeaponCore, ProjectileCore, and WarheadCore.
Mechanism profiles include AntiAirWeapon, GroundAttackWeapon, DeployTransform, Transport, StealthScout, Sensor, SelfRepair, GarrisonPassenger, BuildLimitTechPrerequisite, Veterancy, ArtVoxel, and ArtShp.
Profiles are selected deterministically from user prompt keywords and draft-like wording.
Profile seed keys are only returned as evidence when the active Field Registry provider confirms the key for the active section kind.
Unavailable profile seed keys are skipped and are not fabricated.
Existing exact-current-key and prompt-key matches keep priority over draft profile hints.
Existing Top N bounding remains in effect.
No-hallucinated-fields PromptBuilder rules were not weakened; PromptBuilder itself was not changed.
No DeepSeek adapter behavior, provider switching behavior, UI behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1402 and package count 744.
```

## Completed Criteria For AI-6A

```text
Docs/AiAssistantConversationContextContract.md defines Current IDE Context, Conversation Context, and Current Subject as separate future context types.
The contract states prior assistant INI drafts are conversation draft text only, not applied project file state.
The contract defines future SubjectKind / SubjectId / Source / Summary / Confidence anchors.
The contract defines bounded chat history, draft memory, PromptBuilder integration, UI summary, privacy/safety, tests, and AI-6B through AI-6E implementation split.
No source code, XAML, code-behind, ViewModel, tests, scripts, project files, Field Registry JSON, legacy, PromptBuilder source, send-flow, DeepSeek adapter, provider selection, Apply/Insert, editor text mutation, dirty-state mutation, file write, or Field Registry write behavior was changed.
Doc-phase test/package passed; latest test count 1402 and package count 746.
```

## Completed Criteria For AI-6B

```text
AI conversation context extraction now has pure current-session model/provider types under RA2IniEditor.IDE/AI.
Ra2AiConversationTurn stores User / Assistant role, visible text, and IsDraftResponse.
Ra2AiConversationContext stores bounded turns, TotalCharacterCount, and WasTruncated.
Ra2AiConversationContextRequest provides LastTurns, MaxCharacters, and MaxSingleTurnCharacters bounds with defaults 6 / 6000 / 2000.
Ra2AiConversationContextProvider extracts only caller-supplied current-session visible conversation turns.
Extraction keeps newest turns, enforces total and per-turn character bounds, safely truncates long messages, and sets WasTruncated when any bound cuts content.
Assistant turns are marked as draft/advisory responses and not applied file state.
Sensitive guardrails redact API-key-like tokens, Authorization headers, DeepSeek environment markers, provider metadata, raw request payload markers, and raw response payload markers from extracted context.
Empty chat returns an empty context.
Tests cover recent user/assistant turns, last N turns, total bounds, oversized assistant truncation, assistant draft marking, hidden provider metadata redaction, API-key-like redaction, Authorization/raw payload redaction, empty chat, source message immutability, and no editor text/dirty-state mutation.
No PromptBuilder integration, current subject extraction, draft section ID extraction, Shell UI change, DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1413 and package count 753.
```

## Completed Criteria For AI-6C

```text
AI current subject / draft subject extraction now has pure model/extractor types under RA2IniEditor.IDE/AI.
Ra2AiSubjectKind supports Unknown, Unit, Weapon, Warhead, Projectile, Art, and Section.
Ra2AiSubjectSource supports Unknown, CurrentCaretSection, LastAssistantDraft, and UserMention.
Ra2AiCurrentSubject stores Kind, SubjectId, Source, Summary, Confidence, and IsDraft.
Ra2AiCurrentSubjectExtractor consumes only Ra2AiConversationContext and reads only current-session visible assistant draft/user turn text supplied in that context.
The extractor parses visible INI-style section blocks such as [LAAV], [LAAVMissile], [LAAVMissileWH], and [LAAVMissileP].
Subject kind inference is conservative and uses local field-key heuristics only: unit keys such as Strength / Armor / Primary, weapon keys such as Damage / ROF / Warhead, warhead keys such as Verses / CellSpread, projectile keys such as AA / AG, and art keys/context such as artmd.ini / Voxel / Cameo.
Main unit sections from the last assistant draft are prioritized over weapon / warhead / projectile follow-up definitions for unit prototype drafts.
Recent user-mentioned section IDs can influence selection only when no main unit candidate is present.
Malformed or unknown drafts return Unknown safely.
Extracted subjects from assistant drafts are marked LastAssistantDraft and draft/advisory, and summaries state they come from conversation draft text rather than confirmed project file state.
Tests cover Unit, Weapon, Warhead, Projectile, main-unit priority, user-mentioned section selection without a main unit, malformed fallback, draft/advisory marking, no project-file-state claim, and pure extraction without files/providers/diagnostics/environment access.
No PromptBuilder integration, Conversation Context prompt section, Current Subject UI display, Shell UI change, send-flow change, DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1422 and package count 760.
```

## Completed Criteria For AI-6D

```text
AI PromptBuilder now integrates bounded conversation context and current subject.
Ra2AiPromptBuildRequest accepts optional ConversationContext and CurrentSubject.
Ra2AiPromptBuilder emits Current Subject and Conversation Context sections before Current IDE Context, Field Registry Evidence, and Diagnostics Summary.
The Current Subject section includes SubjectKind, SubjectId, Source, IsDraft, Confidence, and Summary when available, and states LastAssistantDraft is prior draft text only.
The Conversation Context section states the source is recent visible current-session AI Assistant chat, bounded and possibly truncated, not hidden memory, and assistant messages are draft/advisory text rather than applied file state.
Ra2AiAssistantPipeline passes optional ConversationContext and CurrentSubject into PromptBuilder while preserving the existing provider client call.
ShellWindow.xaml.cs only stores visible chat message role/text metadata and passes bounded conversation context/current subject through the AI-6B/AI-6C providers; it does not build conversation prompt text or parse draft section IDs in UI code.
Tests cover PromptBuilder section presence, ordering, prior-draft-not-applied wording, SubjectId/SubjectKind, follow-up reference wording, bounded/current-session conversation wording, sensitive metadata redaction through the conversation context provider, and pipeline context/subject forwarding.
No XAML, DeepSeek adapter behavior, provider selection behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, editor dirty-state mutation, Field Registry write, whole-project context, unbounded chat history, hidden/cross-session memory, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1427 and package count 761.
```

## Completed Criteria For AI-6E

```text
AI Assistant ContextSummary UI now displays compact read-only current IDE context, current subject, and conversation context lines.
AiAssistant.ContextSummary remains present and wraps instead of truncating.
AiAssistant.CurrentSubjectSummary and AiAssistant.ConversationContextSummary were added.
Current subject display shows "当前主题：无" when unknown.
LastAssistantDraft subjects display as "上一轮 AI 草稿，仅作草稿/建议，未写入项目文件" and do not imply applied project file state.
Conversation context display shows only bounded metadata: recent visible turn count and truncated / not-truncated state.
Context summary refreshes when AI Assistant opens, Generate/Send is clicked, assistant/cancel/error response is appended, and chat is cleared.
Tests cover stable summary AutomationIds, draft/advisory wording, no project-file-state claim, bounded turn/truncation summary, no hidden provider metadata exposure in the panel, no Apply/Insert/API-key controls, and unchanged send wiring boundaries.
No PromptBuilder rule change, DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, editor dirty-state mutation, Field Registry write, whole-project context, unbounded chat history, hidden/cross-session memory, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1427 and package count 762.
```
## Completed Criteria For AI-6F

```text
AI Field Registry evidence retrieval now supports subject-aware follow-up expansion.
Ra2AiContextRequest carries optional ConversationContext and CurrentSubject into Ra2CurrentDocumentAiContextProvider.
IRa2AiFieldEvidenceProvider / Ra2FieldRegistryAiEvidenceProvider accept the bounded conversation/current-subject inputs directly.
Previous assistant draft field keys are extracted from current-session assistant draft text with the existing deterministic INI key parser, then returned only when the active provider confirms them.
CurrentSubject.Kind=Unit adds UnitCore and VehicleCore profile candidates; Weapon / Projectile / Warhead / Art subject kinds add their conservative core profiles.
Follow-up intent profiles cover faction/owner, anti-air weapon, deploy/transform, transport, and stealth/scout seed keys.
Faction/owner prompts can retrieve Owner, RequiredHouses, ForbiddenHouses, Prerequisite, UIName, Name, and Image when provider-confirmed.
Anti-air prompts can retrieve Primary, Secondary, Damage, ROF, Range, Projectile, Warhead, AA, AG, and Verses when provider-confirmed.
Seed keys that the active Field Registry provider does not confirm are skipped and are not fabricated.
Evidence defaults to Top 16 and remains hard-capped at 24; exact current key remains highest priority.
ShellWindow.xaml.cs only passes already-built bounded conversation context/current subject into context retrieval; it does not implement evidence logic or parse draft section IDs.
No PromptBuilder no-hallucinated-fields rule change, DeepSeek adapter behavior, provider selection behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, editor dirty-state mutation, Field Registry write, whole-project context, unbounded chat history, hidden/cross-session memory, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added.
restore/build/test/package passed; latest test count 1432 and package count 763.
```

## Completed Criteria For A15-2B-P / A15-2B-P2

```text
Field Registry Center / Manager no longer feel like default WPF form windows.
Project > Global > BuiltIn is clear.
Visible English UI is reduced where safe.
Default system chrome/icon is removed.
Existing commands and AutomationIds are preserved.
Field Registry semantics remain unchanged.
restore/build/test/package pass.
```

## Completed Criteria For A15-2D

```text
Field Editor uses custom lightweight chrome with no default WPF title icon/system title bar.
Allowed Values Editor uses custom lightweight chrome with no default WPF title icon/system title bar.
ResizeMode=CanResize and WindowChrome resize behavior are preserved.
Existing FieldEditor and AllowedValuesEditor AutomationIds are preserved.
Field Editor save preview/apply semantics remain unchanged.
Allowed Values ShowDialog / DialogResult / ResultText semantics remain unchanged.
Field Learning Wizard and Shell remain unchanged.
restore/build/test/package pass.
```

## Completed Criteria For A15-2E-1

```text
Field Learning Wizard uses custom lightweight chrome with no default WPF title icon/system title bar.
FieldLearningWizard.CustomChrome, FieldLearningWizard.ChromeTitle, and FieldLearningWizard.CloseButton are present.
ResizeMode=CanResize and WindowChrome resize behavior are preserved.
Existing FieldLearningWizard AutomationIds, tabs, grids, source text area, commands, and bindings are preserved.
UseCurrentIni / ParsePastedText / BuildApplyPlan / Apply confirmation semantics remain unchanged.
Shell, Field Editor, Allowed Values Editor, Center, Manager, services, parser, validation, and BuiltIn JSON remain unchanged.
restore/build/test/package pass.
```

## Completed Criteria For A15-2B-P4

```text
Field Registry Manager cleanup preview remains available but is demoted from a large default area to a compact advanced cleanup section.
BuildCleanupPlan and ApplyCleanupPlan commands, click handlers, AutomationIds, and cleanup semantics are preserved.
CleanupPreviewExpander, CleanupPreviewSummaryCard, CleanupPreviewEmptyState, and CleanupPreviewScrollHost were added.
Empty cleanup preview no longer reserves a large MinHeight area.
Cleanup preview details use a bounded MaxHeight scroll host after a plan is built.
Apply cleanup remains grouped under the write/risk action area.
restore/build/test/package pass.
```

## Completed Criteria For A15-2E-2

```text
Field Learning Wizard has a display-only workflow step strip: source, parse, target/mode, preview, apply plan, apply.
Source raw text is contained in FieldLearningWizard.SourceScrollHost.
Target / mode summary is explicitly grouped under FieldLearningWizard.TargetModeSummary.
Review tabs and existing grids are preserved inside FieldLearningWizard.ReviewScrollHost.
Build Apply Plan and Apply are moved out of the header into FieldLearningWizard.ApplyBoundaryPanel.
Existing FieldLearningWizard AutomationIds, handlers, commands, bindings, WindowStyle=None, and WindowChrome are preserved.
Field Learning parse/build/apply/write semantics remain unchanged.
restore/build/test/package pass.
```

## Completed Criteria For A15-2B-P3

```text
Field Registry Manager active packs area has a bounded vertical scroll host.
Field Registry Manager warnings area has a bounded vertical scroll host.
Large fixed blank row sizing was reduced.
Manager-visible mixed English/Chinese labels were reduced where safe.
Existing FieldRegistryManager AutomationIds and commands are preserved.
Field Registry semantics remain unchanged.
restore/build/test/package pass.
```


FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply completed. This source-family batch verified TechnoTypes targeting / transport / deploy / hover fields against ModEnc pages: SizeLimit, OpenTopped, DeploysInto, UndeploysInto, DeployFire, DeployFireWeapon, DeployTime, DeployToLand, Naval, Underwater, JumpJet, BalloonHover, and HoverAttack. BuiltIn v3.2 now has 29 new exact object-context rows, updated 15 existing rows, source-backed descriptions for transport/deploy/hover/naval fields, and preserved conservative broad Techno fallback rows where exact context is narrower. BuiltIn v3.2 field count is now 4769. Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_TechnoTypesTargetingTransport_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.


FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply completed. This source-family batch verified TechnoTypes cloak, radar, sensor, disguise, and immunity fields against ModEnc pages: Cloakable, CloakingSpeed, RadarInvisible, Sensors, SensorsSight, DetectDisguise, CanDisguise, DisguiseWhenStill, PermaDisguise, ImmuneToVeins, ImmuneToRadiation, ImmuneToPsionics, ImmuneToPsionicWeapons, ImmuneToPoison, and TypeImmune. BuiltIn v3.2 now has 50 new exact/context rows, 14 updated existing rows, and target rows no longer expose placeholder / generic descriptions. BuiltIn v3.2 field count is now 4862. Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_TechnoTypesCombatBehavior_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.



FR-DQ-2M-TechnoTypes-WeaponTargeting-ManualApply completed. This source-family batch verified TechnoTypes weapon targeting and Weapon-only firing behavior fields against ModEnc pages: OmniFire, DistributedFire, FireAngle, CanPassiveAquire, CanRetaliate, PreventAttackMove, NoAutoFire, LandTargeting, NavalTargeting, FireOnce, Burst, DecloakToFire, UseSparkParticles, and AttachedParticleSystem. BuiltIn v3.2 now has 25 new exact/context rows, 17 updated existing rows, source-backed descriptions for target acquisition / retaliation / land-naval targeting, and guardrail rows for Weapon-only flags previously exposed through Techno or Global fallback contexts. BuiltIn v3.2 field count is now 4887. Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_TechnoTypesWeaponTargeting_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.



FR-DQ-2N-TechnoTypes-AircraftAndSpawn-ManualApply completed. This source-family batch verified TechnoTypes aircraft / spawner / docking / landing / missile-spawn / flight-pitch fields against ModEnc pages: Spawns, SpawnsNumber, SpawnRegenRate, SpawnReloadRate, MissileSpawn, Spawned, Dock, AirportBound, Landable, MoveToShroud, Fighter, FlyBy, FlyBack, AircraftTypes / JumpjetCrash, PitchSpeed, and PitchAngle. BuiltIn v3.2 now has 41 new exact/context rows, 17 updated or guarded existing rows, source-backed descriptions for spawner and aircraft behavior fields, and guardrail rows for aircraft-only flags previously exposed through broad Techno or AI fallback contexts. BuiltIn v3.2 field count is now 4928. Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_TechnoTypesAircraftSpawn_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.



## FR-DQ-2O-TechnoTypes-JumpjetAndFlightTuning-ManualApply Completed

Completed the jumpjet / flight tuning / acceleration field family. BuiltIn v3.2 now has source-backed exact Aircraft / Infantry / Vehicle rows for JumpjetTurnRate, JumpjetSpeed, JumpjetClimb, JumpjetCrash, JumpjetHeight, JumpjetAccel, JumpjetWobbles, JumpjetNoWobbles, JumpjetDeviation, SlowdownDistance, AccelerationFactor, and DeaccelerationFactor. Phobos Warhead jumpjet override rows were updated for Warheads with IsLocomotor=yes and Locomotor=Jumpjet. Weight is exact for Vehicle; PhysicalSize is exact for Infantry. Broad Techno rows are conservative fallbacks or guardrails.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_TechnoTypesJumpjetFlightTuning_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

Next recommended phase: FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply.

## FR-DQ-2Q-TechnoTypes-RepairPowerCaptureFactoryRadar-BigBatch-ManualApply Completed

Completed the repair / power / capture / occupant / factory / radar / superweapon source-family batch.

This batch verified BuildingTypes-centered repair, power, capture, garrison, rubble/bib, factory, radar, superweapon, refinery, absorb, hospital, armory, cloning, and construction-yard rows; VehicleTypes `Harvester` and `PoweredUnit`; Ares `PoweredBy`, `EngineerRepairable`, `Unsellable`, and `SuperWeapons`; and Phobos Shield `SelfHealing` / `Powered` rows.

Result summary:

```text
BuiltIn v3.2 field count: 4988 -> 5030
Rows affected: 90
New exact/context rows: 42
Updated / guarded existing rows: 48
Target rows with direct placeholder / generic labels: 0
Exact “数值型字段” rows: 0
Exact “整数型字段” rows: 99
Placeholder rows: 2268
Source-verified rows: 826
Strict non-source-verified rows: 4204
Hover-risk placeholder/generic rows: 2367
```

Unresolved / intentionally not applied:

```text
CapturableBy
CanOccupyFireWeapon
Disableable / Techno remains NeedsMoreEvidence guardrail
```

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_TechnoTypesRepairPowerCaptureFactoryRadar_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

Next recommended phase: FR-DQ-2R-WeaponCore-BigBatch-ManualApply.


## FR-DQ-2S-WarheadCore-BigBatch-ManualApply Completed

Completed the Warhead core big batch. Verified vanilla Warhead flags, Ares Warhead extensions, and same-domain Phobos Warhead extensions. Added 8 exact/context rows, updated or guarded 97 existing rows, total affected rows 105. BuiltIn v3.2 field count is now 5044. Source-verified rows are 1023; strict non-source-verified rows are 4021; direct Hover-risk placeholder/generic rows are 2275.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_WarheadCore_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

## FR-DQ-2T Projectile Core Big Batch Manual Apply

FR-DQ-2T-ProjectileCore-BigBatch-ManualApply completed. This batch source-verified vanilla Projectile core flags, Ares projectile collision/trench extensions, and same-domain Phobos projectile interception / collision / trajectory rows.

Processed scope: AA, AG, ROT, Image, Shadow, Proximity, Ranged, Arcing, Inaccurate, FlakScatter, SubjectToCliffs, SubjectToElevation, SubjectToWalls, SubjectToBuildings, SubjectToTrenches, Acceleration, Vertical, Dropping, Arm, CourseLockDuration, Scalable, Interceptable, SubjectToGround / SubjectToLand / SubjectToWater, and Trajectory.Bombard / Parabola / Straight rows.

Added 7 exact/context rows, updated or guarded 129 existing rows, total affected rows 136. BuiltIn v3.2 field count is now 5051. Source-verified rows are 1158; strict non-source-verified rows are 3893; direct Hover-risk placeholder/generic rows are 2219.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_ProjectileCore_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

Next recommended phase: FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply.


## FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply Completed

Completed the Projectile advanced Phobos/Ares/ModEnc batch. Verified Airburst / Splits, AirburstWeapon additions, Retarget*, scatter customization, per-projectile Gravity, Parachuted / BombParachute, ReturnWeapon, and Shrapnel advanced rows. Added 4 exact Projectile rows, updated or guarded 52 existing rows, total affected rows 56. BuiltIn v3.2 field count is now 5055. Source-verified rows are 1212; strict non-source-verified rows are 3843; direct Hover-risk placeholder/generic rows are 2195.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_ProjectilePhobosAdvanced_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

Next recommended phase: FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply.

## FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply Completed

Completed the Art / Animation core big batch. Verified ModEnc art(md).ini Animation playback / looping / trailer / spawn / palette / lighting rows and same-domain Phobos Animation extensions for visibility, Anim-to-Unit, fire animation spawning, animation damage, attached particle system, and debris / splash behavior.

Result summary:

```text
BuiltIn v3.2 field count: 5055 -> 5069
Rows affected: 107
New exact/context rows: 14
Updated / guarded existing rows: 93
Source-verified rows: 1212 -> 1312
Strict non-source-verified rows: 3843 -> 3757
Direct placeholder rows: 2115 -> 2073
Exact integer generic rows: 80 -> 78
Exact numeric generic rows: 0 -> 0
Direct Hover-risk placeholder/generic rows: 2195 -> 2151
```

ModEnc-sourced rows include Image / Theater, Animation Looping flags, Normalized, TrailerAnim / TrailerSeperation, SpawnCount, Translucent, UseNormalLight, AltPalette, AnimPalette, and Next. Phobos-sourced rows include VisibleTo / RestrictVisibilityIfCloaked / DetachOnCloak, Anim-to-Unit CreateUnit.* rows, Fire animation spawning rows, Animation Damage.* rows, AttachedSystem, AttachedAnimPosition, SplashAnims, WakeAnim, and Warhead.Detonate. AI base construction rows that were misclassified as Animation are guarded as non-canonical.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_ArtAnimationCore_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

Next recommended phase: FR-DQ-2W-ArtAnimationPhobosCreateAndVisuals-BigBatch-ManualApply.


## FR-DQ-2X-SuperWeaponSideCountryUi-MegaBatch-ManualApply Completed

Completed the SuperWeapon / Side / Country / Banner / Eva / UI-related Global mega batch. The phase cleared direct Hover-risk rows in the SuperWeapon, Side, Country, Banner, Eva, and selected Global UI families. Source-backed rows use Ares / Phobos superweapon, side defaults, paradrop, and UI documentation. Source-insufficient Banner / Eva / load-screen / color rows were converted to explicit `NeedsMoreEvidence` guardrails and appended to `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.

Result summary:

```text
BuiltIn v3.2 field count: 5069 -> 5069
Rows affected: 237
Source-backed rows: 196
NeedsMoreEvidence guardrail rows: 41
Source-verified rows: 1312 -> 1508
Strict non-source-verified rows: 3757 -> 3561
Direct placeholder rows: 802 -> 583
Exact integer generic rows: 62 -> 44
Exact numeric generic rows: 0 -> 0
Direct Hover-risk placeholder/generic rows: 864 -> 627
```

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_SuperWeaponSideCountryUi_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Docs/FieldRegistryUnresolvedRows_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

Next recommended phase: FR-DQ-2Y-RemainingArtVoxelTerrainSound-MegaBatch-ManualApply.


## FR-DQ-3C-UnresolvedRecheck-A completed

- Promoted targeted unresolved rows using ModEnc / Ares / Phobos sources.
- Added `IvanBomb / Warhead` canonical row.
- Updated unresolved list and hover quality scan.
- Direct Hover-risk rows remain 0.
- Remaining unresolved rows: 1815.
- No provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project file, or legacy changes.


## FR-DQ-3D TeamTypes / AITriggerTypes Schema Recheck completed

- Completed: `FR-DQ-3D-TeamTypes-AITriggerTypes-SchemaRecheck-ManualApply`.
- Added precise `TeamType` / `TaskForce` rows and promoted AI programming legacy rows to source-backed guardrails where reliable ModEnc sources existed.
- Added verification doc: `Docs/FieldRegistryDescriptionVerification_AiSchemaRecheck_2026-06-03.md`.
- Updated unresolved list: `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.
- No provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project file, or legacy behavior changes.
- Current metrics: field count 3519, source-verified rows 2051, unresolved rows 0, direct Hover-risk rows 0.
- Next recommended phase: `FR-DQ-3E-TechnoResidualSourceFamilyRecheck`.


## FR-DQ-3E-LowConfidenceBurnDown

- Manual GPT-side source verification pass promoted/guardrailed 181 Phobos-supported Techno rows.
- Fixed 103 unsupported `schema.type=Text` values to `schema.type=String` while preserving `editorKind=Text`.
- Current metrics: field count 3519, source-verified rows 2051, unresolved rows 0, direct Hover-risk rows 0.


## FR-DQ-3F-InferredBacklogRecovery

- Manual GPT-side relaxed-evidence pass restored the 3E runtime backlog as inferred fallback rows.
- Recovered rows: 1590.
- Current metrics: field count 5109, source-verified rows 2051, inferred fallback rows 1590, unresolved rows 0, unsupported schema.type=Text rows 0, direct Hover-risk rows 0.
- New verification doc: `Docs/FieldRegistryDescriptionVerification_InferredBacklogRecovery_2026-06-03.md`.
- No provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project file, user Global active pack, or legacy behavior changes.

FR-DQ-3F metric correction: field count 5109, source-verified rows 2051, inferred fallback rows 1591, unresolved rows 0, unsupported schema.type=Text rows 0, direct Hover-risk rows 0.

## FR-DQ-3I-ReleaseIconAndUserDocs

- 基线：FR-DQ-3H Fix2 测试全绿版。
- 本阶段只补齐发布资产：应用图标、窗口 / 任务栏 / exe 图标、v0.5.0-preview 用户说明、Release Notes、Known Issues、字段可信度说明、打包说明和烟测清单。
- 不修改 BuiltIn 字段库、Hover 核心逻辑、Diagnostics 核心逻辑、保存链路或 Completion 行为。
- 发布定位：v0.5.0-preview 技术预览版。

## UI-MODERN-PROGRAM-R1 VISUAL-FIX3 — 2026-07-23

- AI 助手清空、进阶、发送和取消按钮已从旧 PNG 消费切换为现有
  `IconGeometry.Action.*` 矢量 Geometry。
- 字段库活跃来源包列宽调整为 80/56 DIP，总宽仍为 136 DIP；
  管理器 `目标 Section` 列宽调整为 112 DIP。
- 全项目 30 个生产 XAML 均可解析；129 个固定数值 DataGrid 列的标题宽度
  静态审计未再发现高风险裁切；生产 XAML 中无动作 PNG 消费。
- Debug build 通过；定向 53/53、全量 2335/2335 测试通过。
- IdeOnly 清洁源码包通过，973 个文件。
- `AGENT-AUTHORING-1` 已完成底层事实回归和架构契约。当前不存在统一编译器；
  后续应先建立 internal 语言服务门面，再按 Plan/Preview/Transaction 接入内置 AI
  与外部 Agent。
- 无 public API、解析、诊断、补全、保存、字段优先级、AI 请求/流式、Dock 或
  legacy 行为变更。

下一推荐阶段：`AGENT-AUTHORING-1 A1 LanguageServicesFacade`。该阶段只做
internal 只读门面和等价性测试，不开放 Agent Apply。

## AGENT-AUTHORING-1-R1 A1 Language Services Facade — 2026-07-23

- A1-B1：`FieldRegistryRuntimeService` 现可发布 internal Provider Snapshot +
  Revision；初始 Revision 为 1，每次成功 Reload 只递增一次，旧 Snapshot 不漂移。
- A1-A1/A2：新增 UI 无关的分析 Request、DiagnosticFact、显式失败分类和只读 Result。
- A1-A3：新增 internal `IRa2IniLanguageAnalysisService`，复用现有 TextModel、
  Semantic Builder 和完整 CurrentFile 诊断服务。
- A1-C：旧 Request 在 Registry Reload 后仍使用捕获 Provider；诊断事实与现有服务
  逐属性、逐顺序等价；契约源文件不引用 WPF、Shell、writer 或 Runtime Service。
- 受控技术债 `AGENT-AUTHORING-A1-TD-001`：SemanticModel 当前会在门面和现有
  诊断服务中各构建一次；只在 A2 性能证据证明必要时偿还。
- 定向 45/45、完整测试 2355/2355；IDE-only build 0 warnings/0 errors；
  IdeOnly clean package 通过，989 个文件。
- 无 public API、Provider priority、parser/diagnostics/completion/save、Shell/UI、
  项目依赖或 legacy 行为变化。

权威结果：`Docs/AGENT-AUTHORING-1-R1_A1_StageLedger.md`。  
下一推荐阶段：`AGENT-AUTHORING-1-R1 A2-A EditableSessionIdentityAndRevisionContract`。
