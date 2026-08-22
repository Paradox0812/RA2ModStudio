# RA2IniEditor.IDE — Historical Accumulated Codex Context

> Superseded on 2026-08-22 by the compact
> `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`. This snapshot is preserved as
> historical evidence and is not the current project-state authority.

> Purpose: restore project context after resetting `.codex`, switching from Codex App to CLI, opening a new Codex session, or continuing after a crash.  
> Read the latest proposed/current entry first. Historical phase records remain below and are not current instructions unless explicitly referenced.

## AUTOMATION-HLI-0B minimum capability contract prepared; implementation awaiting confirmation

On 2026-08-22 `Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md` was generated
and self-reviewed from the completed HLI-0A audit and the current A1-A4 source facts.
This is a proposed R3 architecture contract; no implementation or public API has been
authorized or added.

The necessity decision is conditional but decisive for the current roadmap. If only
the in-process WPF AI needed structured editing, the existing internal A1-A4 path would
be sufficient and no physical split would be required. Because the planned system also
requires independent `net8.0` Agent/CLI/Job hosts, the current `net8.0-windows` assembly
placement is a real blocker. A UI-neutral assembly boundary is therefore necessary.

The contract rejects a bulk move. The dependency cone crosses IDE TextModel,
Classification, Language, Diagnostics/ViewModel adaptation, FieldTrust and Editing;
moving it as one package would be high-risk R3 and difficult to verify. The recommended
path is a new candidate `RA2IniEditor.Application` targeting `net8.0`, initially
referencing Core only, followed by minimal vertical slices. The IDE must consume each
new canonical implementation so that no second parser, diagnostic engine, reference
finder or edit planner survives.

The first four process-local capability IDs are `ini.document.section.get`,
`ini.document.references.find`, `ini.document.diagnostics.validate` and
`ini.document.edit.preview`. The reference capability is deliberately current-document
only; project-wide semantic references remain a separate contract. Inputs are explicit
immutable document and Field Registry snapshots. Expected failures and cancellation are
typed, results are immutable, and Application code may not read disk, global Registry
state, WPF, AvalonEdit, Shell or ViewModels.

Existing A3 owns active Preview, generation, single-use claim, live currency checks,
Apply and Undo. Existing Save/Backup/Writer/Rollback remains user/IDE-owned. Neither
boundary becomes an Agent capability. CLR contracts remain separate from any future
JSON/IPC/MCP protocol, which requires an R4 review.

The proposed migration order is HLI-1A0 dependency-cone characterization, HLI-1A1
section/reference query extraction, HLI-1A2 neutral diagnostics, HLI-1B semantic
Preview extraction, HLI-1C host-boundary confirmation, and only then HLI-2A Gateway.
Incremental extraction is assessed as medium controlled R3 risk; bulk extraction,
duplicated algorithms or a Gateway that directly references the WPF assembly are high
risk and rejected.

This stage changed only the HLI-0B contract and the two current-state documents. No
source, test, project reference, XAML, Shell, Dock, AutomationId, parser, diagnostics,
Field Registry, Search, Preview, Apply, Save, persistence or legacy behavior changed.
The next safe action is user confirmation of HLI-0B. After confirmation, write and
approve `AUTOMATION-HLI-1A0 Dependency Cone Characterization Contract`; do not create
the Application project or move code directly.

---

## AUTOMATION-HLI-0A existing capability audit completed

On 2026-08-20 the approved DocsOnly capability audit completed against
`Docs/AUTOMATION-HLI-0A_ExistingCapabilityAuditContract.md`. The authoritative
inventory is `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`, and execution
evidence is `Docs/AUTOMATION-HLI-0A_StageLedger.md`.

The audit confirms that the high-level Automation route should reuse, not replace,
the current implementation. Core already owns reusable field schema contracts. The
IDE already contains text parsing/semantic analysis, definitions and current-document
references, project text search, diagnostics, A2 structured Preview, A3 host Apply,
A4 Agent proposal adaptation, and reliable Save/Backup/Rollback. These paths are the
single implementation authorities for later capabilities.

The central boundary problem is assembly placement. Most Language/Search/Diagnostics
and A2 Preview code does not create WPF controls or perform direct UI work, so it is
algorithmically headless. It is nevertheless internal to the `net8.0-windows` WPF IDE
assembly and cannot currently be referenced by an independent `net8.0` CLI, Job or
external Agent host. HLI-0B must therefore define neutral application contracts before
any type is moved or exposed; existing IDE internal DTOs must not simply be made public.

The audit also locks three negative facts. Project Search is textual search and does
not provide a complete project semantic-reference query. Current diagnostics expose a
ViewModel-shaped result at one key boundary and need a neutral result contract before
extraction. Formal semantic Template, Capability Registry/Gateway, Job State, Event Bus
and Artifact identity infrastructure do not exist in the current repository and remain
separate later work.

The recommended—but not yet approved—assembly option is a new UI-neutral `net8.0`
Application layer between Core/Infrastructure adapters and the IDE host. The first
Agent-facing surface should be bounded query, document diagnostics and semantic edit
Preview. Existing A3 Apply/Undo, active editor ownership and Save/Writer remain host-only;
they must not be registered as Agent capabilities. In-process contracts and future wire
DTO/IPC protocol must be reviewed separately.

HLI-0A changed documentation only. No public API, source, tests, project references,
XAML, Shell, Dock, AutomationId, parser, diagnostics behavior, Field Registry, Search,
Save, Undo/Redo, persistence or legacy behavior changed. Build/test/package and UI
automation were NotRun under the DocsOnly profile. The next safe entry is a separately
confirmed `AUTOMATION-HLI-0B Minimum Capability Contract`; no Application project,
Gateway or external bridge may be implemented from this audit alone.

---

## AGENT-AUTHORING-1-R1 A4-R1 reliable structured editing implemented

On 2026-08-20 the user confirmed
`Docs/AGENT-AUTHORING-1-R1_A4_R1_ReliabilityContract.md`. The execution record is
`Docs/AGENT-AUTHORING-1-R1_A4_R1_StageLedger.md`.

A4-R1 closes the failure mode where explicitly configuring the official DeepSeek URL
was incorrectly treated as a custom endpoint and silently downgraded editing to a code
example. Endpoint identity is now classified only after final chat-completions URI
normalization. Shell resolves advisory, explicit edit, ambiguous edit and unavailable
edit locally before provider capacity is selected; ambiguous/unavailable edit requests
remain in the input box and do not acquire structured-edit authority.

Official, editable, explicit current-document requests use separated system/user
messages and require the single `preview_ini_edit_plan` tool. Its flat provider schema
is locally enforced as a strict `proposal` / `needs_clarification` outcome. Provider
plain text cannot substitute for a required tool call, and mixed prose never changes
the locally validated operation set. Tool arguments remain untrusted until the adapter
binds them to the captured A3 snapshot and produces a local Preview.

Preview preparation runs through a UI-independent runner. Before a card can attach,
Shell verifies cancellation, request generation, active streaming handle, current
document/Field Registry currency and coordinator ownership. The one active card is
pinned against chat-history trimming; later conversation context stores only its local
bounded summary. Apply remains explicit, single-use, current-document and in-memory
only, reusing A3 Undo semantics and never saving.

No parser, diagnostics, Completion, Field Registry, Search/Replace, save/backup,
Undo/Redo semantics, project dependency, `ShellWindow.xaml`, Dock/layout, menu, toolbar,
AutomationId or legacy behavior changed. No live provider, API key, UI automation or
computer-control testing was used. The next safe entry is a separate
`AGENT-AUTHORING-1-R1 HLI-0A` high-level Agent interface contract; multi-file writes,
automatic Apply/Save, retry and custom-endpoint tools remain out of scope.

---

## AGENT-AUTHORING-1-R1 A4 AI structured edit proposal completed

On 2026-07-28 A4 completed against
`Docs/AGENT-AUTHORING-1-R1_A4_AiStructuredEditProposalContract.md`; its execution
record is `Docs/AGENT-AUTHORING-1-R1_A4_StageLedger.md`.

A4 adds a provider-neutral tool-call DTO boundary and DeepSeek SSE assembly for one
bounded tool, `preview_ini_edit_plan`. The tool is exposed only when the request uses
the official endpoint and the Shell can capture an editable A3 authoring snapshot.
Ordinary chat, read-only/no-document states, invalid configuration, and custom
endpoints remain advisory-only. Existing Pipeline overloads also remain advisory-only.

Provider arguments remain untrusted strings until `Ra2AiAuthoringToolAdapter` performs
strict JSON validation. Unknown/duplicate properties, unsupported operations,
malformed JSON, more than one tool call, and request/current snapshot mismatch are
rejected before A3 Preview. The only operations are current-document `UpsertField`
and `ReplaceFieldValue`; there is no generic patch, command execution, file path,
Apply, Save, or project-level operation in the tool schema.

`Ra2AiAuthoringCoordinator` owns one active proposal and delegates preview/apply to
the existing A3 workspace. Added errors block Apply; added warnings, unknown fields,
or non-verified trust evidence require caution. The inline WPF proposal card shows
operation evidence and offers explicit Apply/Dismiss. Apply reuses the A3 live
currency gate and editor transaction, changes the in-memory session only, creates
one semantic Undo unit, and never saves. Document/session/registry/chat lifecycle
changes invalidate the proposal, and confirmed proposals are single-use.

Only private `ShellWindow.xaml.cs` activation/lifecycle glue was added to the Shell.
`ShellWindow.xaml`, AvalonDock structure, ContentIds, layout persistence, menu,
toolbar, global theme resources, parser/diagnostics/completion semantics, Field
Registry priority/data, Search, Save/Writer/Backup/Rollback, dependencies and legacy
were not changed.

Next safe entry is manual A4 acceptance using the official endpoint and a disposable
INI document. Broader edit operations, multi-document/project edits, automatic
apply/save, automatic retry, or custom-endpoint tool enablement require new contracts.

---

## AGENT-AUTHORING-1-R1 A3 editor transaction port completed

On 2026-07-28 A3 completed against
`Docs/AGENT-AUTHORING-1-R1_A3_EditorTransactionPortContract.md`. The execution
record is `Docs/AGENT-AUTHORING-1-R1_A3_StageLedger.md`, and the compact handoff is
`Docs/ContextCapsule_AGENT_AUTHORING_1_A3.md`.

A3 makes the A2 single-document Preview consumable through one internal transaction
path. `Ra2IniAuthoringWorkspace` owns one active Preview and a generation counter;
callers can only submit PreviewId plus explicit confirmation. An unconfirmed request
does not consume the Preview, while a confirmed matching request claims it before
the transaction and cannot be replayed. Older concurrent Preview generation cannot
overwrite a newer active slot.

The Shell-owned private transaction port reads the live editable Session, AvalonEdit
text, Field Registry provider revision and Caret at commit time, then reuses
`Ra2IniEditPreviewCurrencyEvaluator`. The Session Controller independently verifies
DocumentId, EditRevision, original text, editable state and no-op before performing
exactly one `UpdateText`. A successful transaction publishes one new Session revision,
one editor text sync and one existing semantic Undo unit. It does not save. Editor
sync failure attempts to restore text/Caret and preserves the old Session/semantic
Undo; restoration failure degrades to read-only to prevent inconsistent saving.

Only narrow `ShellWindow.xaml.cs` private transaction glue and Preview invalidation
points changed. `ShellWindow.xaml`, Dock/layout, AutomationIds and all other XAML are
unchanged. AI, Search behavior, Save/Writer/Backup/Rollback, parser, diagnostics,
Completion, Field Registry implementation/data, project/dependencies and legacy
remain unchanged.

Verification passed: A3 targeted and affected boundary tests 23/23; IDE-only Debug
solution build 0 warnings/0 errors; full non-UI tests 2436/2436. UIA was not required
because A3 introduces no user entry or control.

The next safe entry is a separate `AGENT-AUTHORING-1-R1 A4` user-visible
preview/confirmation contract. A3 must not be described as an AI file-editing feature,
and the current user instruction requires stopping after A3.

---

## AGENT-AUTHORING-1-R1 A2 single-document plan preview completed

On 2026-07-28 the A2 continuous package completed against
`Docs/AGENT-AUTHORING-1-R1_A2_SingleDocumentPlanPreviewContract.md`. The execution
record is `Docs/AGENT-AUTHORING-1-R1_A2_StageLedger.md`, and the compact handoff is
`Docs/ContextCapsule_AGENT_AUTHORING_1_A2.md`.

A2 introduces internal, UI-independent authoring contracts in the existing Editing
boundary. `Ra2AuthoringSnapshot` captures one editable document together with its
DocumentId, EditRevision, exact editor text and Field Registry provider revision.
`Ra2IniEditPlan` supports only `UpsertField` and `ReplaceFieldValue`; all operations
resolve against the same original Snapshot. `Ra2IniEditPreviewService` produces a
deterministic `Ra2TextChangeSet`, candidate text, per-operation field/trust evidence,
and current/candidate language analysis with diagnostic delta. It preserves existing
formatting/comments for replacements, rejects ambiguous sections/keys, conflicting
operations and no-op plans, and performs no I/O.

`Ra2IniEditPreviewCurrencyEvaluator` provides a pure stale check over document
identity, edit revision, session/editor text and Registry revision. It is not a
Preview Store and cannot Apply. A2 deliberately has no Shell, Dock, XAML, ViewModel,
AI, Undo/Redo, Save or disk-write wiring; therefore it has no new user-visible UI
surface. All new production types remain internal/Experimental.

Verification passed: IDE-only Debug solution build 0 warnings/0 errors; A2 and
related A0/A1/Registry/Session/ChangeSet/planner regression 104/104; full non-UI
tests 2419/2419. Record-only 1/4/7 MiB previews completed on this machine without
mutating source text. Existing `AGENT-AUTHORING-A1-TD-001` remains Open / Controlled
and was not repaid opportunistically.

Shell/Dock/UI, parser, diagnostics, Completion, Field Registry, Hover, Quick Peek,
AI, Save/backup/rollback, BuiltIn data and legacy remain unchanged. The next safe
entry is `AGENT-AUTHORING-1-R1 A3 EditorTransactionPortContract`, which must separately
contract a workspace-owned Preview Store, single-use consumption, complete currency
recheck and one existing semantic Undo transaction without automatic save.

---

## SEARCH-1-R1 project search and current-file replace completed

On 2026-07-23 the user authorized a self-reviewed continuous implementation before returning to `AGENT-AUTHORING-1-R1 A2`. The authoritative contract is `Docs/SEARCH-1-R1_ContinuousContract.md`, the execution record is `Docs/SEARCH-1-R1_StageLedger.md`, and the compact handoff is `Docs/ContextCapsule_SEARCH_1_R1.md`.

Search is no longer a placeholder. `Tool.Search` searches either the whole canonical Project Explorer file list or the current file. Matching supports case sensitivity, whole words, and .NET regular expressions with a 500 ms per-file timeout. Results are stable by project-file order and character position, capped at 10,000, include file/line/column/Section/preview, and navigate through the existing dirty-file guard. The current file always uses AvalonEdit's in-memory text; other files use `ReadonlyIniContentService`. Files above the existing 8 MB deferred-preview boundary or files that fail to load are skipped and reported rather than searched as error text.

Replace All is current-file only and preview-first. `Ra2EditableDocumentSession` now has internal `DocumentId` and `EditRevision` lifecycle facts. A replacement plan is applicable only while identity, revision, session text and editor text still match the preview. It does not save; application updates the in-memory dirty session and reuses the existing programmatic semantic Undo state, so one Ctrl+Z/Ctrl+Y undoes/redoes the batch. Project/multi-file replacement, recursive directory scanning, background indexing and automatic save remain excluded.

Verification passed: Debug solution build 0 warnings/0 errors; all non-UI tests 2380/2380; Search floating open/hide/reopen UIA 1/1. The final IdeOnly clean package is `artifacts/RA2IniEditor.IDE.SourceClean.zip`: 1003 entries, zero forbidden entries and zero required entry/contract omissions. The existing AvalonDock child-HWND automation boundary remains as `SEARCH-UIA-001`: external FlaUI sees floating host chrome but cannot traverse hosted Search controls. Functional behavior is therefore covered by Search/Replace unit, ViewModel, XAML and Shell boundary tests rather than a falsely green external UIA test.

`ShellWindow.xaml`, Dock ContentIds/Home/persistence, parser, diagnostics, Completion behavior, Field Registry, Hover, AI, Save/backup/rollback, BuiltIn data and legacy remain unchanged. The next safe entry is to rebaseline `AGENT-AUTHORING-1-R1 A2` against the now-existing identity/revision contract; Search's current-file transaction must not be generalized into Agent multi-file writes without a separate contract.

---

## UI-MODERN-PROGRAM-R1 VISUAL-FIX2 implementation completed; visual acceptance pending

On 2026-07-23 the user confirmed the bounded `VISUAL-FIX2` contract. The authoritative result is `Docs/UI-MODERN-PROGRAM-R1_VISUAL-FIX2_StageLedger.md`.

The Field Registry active-pack table now uses fixed 88/48 DIP `范围`/`字段` columns, removing the stretch-created gap without changing the surrounding responsive pane. Shell startup now asks the existing `ShellDockFloatingChromeController` to suppress rendering of intermediate floating hosts while compiled topology and persisted layout are applied. Registered hosts retain their prior opacity, and `ShellWindow_OnLoaded` restores it in `finally`.

The verified AvalonDock lifecycle remains intact: default topology may create the Search floating container, the dispatcher interval remains, and visibility is applied in its existing phase. The new suppression path does not call `Hide()`. No Store, Session, Coordinator, ContentId, Home profile, layout file, migration or Search behavior changed.

Verification passed: XAML parse; Debug solution build with 0 errors and one pre-existing `CS8602` warning; affected Shell/visual/layout tests 76/76; full non-UI tests 2335/2335. The first targeted run exposed an overbroad assertion in the new test; only that assertion was narrowed, and production code was unchanged.

The final IdeOnly clean package is `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-VISUAL-FIX2.Final.zip`. It contains 970 entries, has zero forbidden entries and zero required-file omissions, and differs from the VISUAL-FIX1 rollback anchor in exactly the ten expected implementation/test/governance files.

Real startup/no-flash and final active-pack spacing are not yet visually accepted. Next safe entry is `UI-MODERN-PROGRAM-R1 VISUAL-FIX2-ACCEPTANCE`.

---

## UI-MODERN-PROGRAM-R1 VISUAL-FIX1 implementation completed; visual acceptance pending

On 2026-07-23 the user approved a bounded screenshot-backed correction covering the Field Editor close glyph, Field Registry Center source-pane width and the AI right workspace. The authoritative result is `Docs/UI-MODERN-PROGRAM-R1_VISUAL-FIX1_StageLedger.md`.

The Field Editor local close button now uses a 30 x 28 DIP button, 6 DIP padding and a 14 DIP vector glyph. The Field Registry Center uses bounded `20* / 50* / 30*` columns and labels the source area `活跃字段包`. The AI Dock tab is the only `AI 助手` title authority; its visible context is a maximum 44-DIP two-line summary with no Expander. The existing verbose context element remains on its original update path and is available as a ToolTip. Welcome and draft guidance use the existing empty-state lifetime, and the concise safety footer wraps instead of overflowing.

No code-behind, public API, dependency, project file, Dock ContentId/topology/lifecycle, AI model/stream/cancel/failure behavior, Field Registry authority/write/apply/import/learning/rollback behavior, parser, completion, Hover, diagnostics, save, BuiltIn data or legacy behavior changed. Existing names and AutomationIds remain present.

Verification passed: four changed production XAML/resource files parsed; Debug solution build passed with 0 warnings/0 errors; Shell and visual-system boundary tests passed 48/48; full non-UI tests passed 2334/2334. Desktop screenshot capture was NotRun because computer control was intentionally avoided, so the correction is implementation/automated-gate complete but not yet visually accepted.

The final IdeOnly clean package is `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-VISUAL-FIX1.Final.zip`; it contains 969 entries and passed a zero-forbidden-entry scan.

Next safe entry: `UI-MODERN-PROGRAM-R1 VISUAL-FIX1-ACCEPTANCE`, manually checking Field Editor, Field Registry Center and AI at the default right-dock width and compact width.

---

## UI-MODERN-PROGRAM-R1 M4-R2 implementation completed; visual acceptance pending

On 2026-07-23 the user confirmed `UI-MODERN-PROGRAM-R1 M4-R2` and authorized continuous execution without per-card approval waits. The authoritative contract is `Docs/UI-MODERN-PROGRAM-R1_M4R2_FieldRegistryVisualConvergenceContract.md`, the exact pre-change inventory is `Docs/UI-MODERN-PROGRAM-R1_M4R2_ExactUiInventory.md`, and the trusted result record is `Docs/UI-MODERN-PROGRAM-R1_M4R2_StageLedger.md`.

M4-R2 introduces eight additive Field Registry R2 style keys and thirteen vector Geometry keys, then adopts them across the nine approved surfaces. Center now uses a dominant virtualized list, compact scope pane and real selected-field Inspector; Manager separates compact status from detailed rollback/cleanup evidence; Import Preview and Learning expose workflow hierarchy; Field Editor, Allowed Values, Remote Preset, Add Property and Annotation share flat section, grid, Inspector and action-footer structure. Existing accepted style keys were not mutated.

The only C# change is the approved internal Center row `Details` projection and effective-mapping wording. No public API, dependency, project file, Shell/Dock, provider priority, matching, Field Registry write/import/learning/apply/rollback behavior, parser, diagnostics, completion, Hover, Quick Peek, Save Preflight, BuiltIn data or legacy behavior changed. No existing AutomationId or Click handler was removed; nine old Center detail bindings were replaced only by the approved projection. DataGrid recycling and virtualization remain reachable through the R2 `BasedOn` chain.

Verification passed: `dotnet restore .\RA2IniEditor.IDE.sln`; Debug solution build with 0 warnings/0 errors; full non-UI tests 2334/2334; nine changed XAML files parsed; missing and duplicate resource checks were zero; production `IdeSecondary*` references remained zero; all pre-existing AutomationIds were preserved. The opt-in `FieldImportApplySmokeTests` UIA filter is blocked before product launch because its existing `FindRepositoryRoot()` still searches for the removed/forbidden `RA2IniEditor.sln`. This is recorded as separate `UI-AUTO-IDEONLY-ROOT` infrastructure debt and was not “fixed” by restoring legacy.

The pre-change rollback package is `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.PreChange.Rollback.zip`: 967 entries, 10,477,691 bytes, SHA-256 `24C7F80967F1B18C1C2554369DE859EEC5961E66E6C1CF1A434442ED92324D5A`, zero forbidden entries. The final clean package is `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4R2.Final.zip`; final package evidence is recorded in the M4-R2 ledger.

All eight contract screenshots remain NotRun because general computer control was intentionally avoided and the existing automation cannot deterministically capture those states. M4-R2 must therefore be described as implementation/automated-gate complete, not visually accepted. The next safe entry is `UI-MODERN-PROGRAM-R1 M4-R2-VISUAL-ACCEPTANCE`: capture or manually inspect the named 1920 x 1080 / 100% and 820 x 620 states, then record acceptance or create one bounded correction contract.

---

## UI-MODERN-PROGRAM-R1 Revision A confirmed; M6-B completed

On 2026-07-22 the user confirmed continuous execution of the reviewed project-level visual modernization program without per-card approval waits. The authoritative contract is `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md`.

The complete program is an explicitly authorized R3 presentation-boundary direction, but every implementation card must remain R1: no public API, persistence, Field Registry authority/write, AI lifecycle, parser/editor/language semantic, dependency, project-file or legacy change. Revision A adds scoped workspace dictionaries, immutable-after-gate resource keys, exact UI inventories before each window family, quantitative visual tolerances, virtualization/performance constraints, a window-chrome matrix and package-level rollback anchors.

P0 established the original unique baseline. M3 is now completed and has its own accepted clean rollback package at `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M3.Rollback.zip`: 957 source entries, SHA-256 `1DB7A07AAE8D6770D0E40287EA1D97B1DC73F0E2ACAC28F02DD7F9B903E6E161`. Package hygiene excluded `.git`, `.vs`, `bin`, `obj`, `artifacts`, `TestResults`, caches, logs and archives.

M3 delivered the scoped workspace style dictionary, graphical Problems severity presentation, flat Project Explorer/Output, resource-driven AI/Hover visuals, compact menu/toolbar/Dock density and the modern floating Search composition. A narrow ContextMenu correction prevents explicit separators from receiving a `MenuItem` style. Seven ContentIds, Dock persistence/lifecycle, Search behavior and all protected semantic boundaries remain unchanged. The Debug solution build passed with zero errors; the full non-UI suite passed 2314/2314.

Real current-host Shell, Search and AI screenshots exist under `artifacts/M3-*.png`. The host was 2560 × 1440 at 125%; exact 1920 × 1080 / 100%, 1280 × 800 and 150% DPI checks remain NotRun. The populated Problems severity screenshot was not reliably captured and is not claimed. The pre-existing child-HWND UIA provider gap remains tracked as `UI-MODERN-M1-A11Y-001`.

M4 is completed. `Themes/IdeFieldRegistryStyles.xaml` now provides the scoped Field Registry vocabulary. Center uses the approved 156 / adaptive / 300 navigation-list-details workspace; Manager separates status, rollback and cleanup; Harvest and Learning express source/review/plan/apply hierarchy; Field Editor, Allowed Values, Add Property, Annotation and Remote Preset use modern inputs, virtualized grids, details and explicit write boundaries. Existing AutomationIds, handlers, lifetimes and business semantics remain intact.

M4 verification passed: Debug solution build 0 warnings/0 errors; full non-UI suite 2322/2322; four real WPF screenshots at configured 1040 × 700, 1120 × 880 and 1040 × 720 sizes; IdeOnly clean package. The visual smoke parsed only the built-in sample and performed no fetch, save, apply, rollback or registry mutation. The authoritative ledger is `Docs/UI-MODERN-PROGRAM-R1_M4_StageLedger.md`.

M4 final rollback package: `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M4.Accepted.Rollback.zip`; 960 entries; SHA-256 `0EE0E4F5F3A2EF966C9C6408393109F4E4F4ABEA2CCF6E2344643EB46BE075F5`.

M5 is completed. `Themes/IdeEditorAssistStyles.xaml` is the scoped visual authority for Completion, Peek, References and transactional dialogs. The affected surfaces adopt semantic resources, compact hierarchy, explicit warning bands and flat result presentation. `IdeSecondaryWindowStyles.xaml` now retains only four named compatibility aliases. Existing handlers, bindings, owner/lifetime behavior, AutomationIds and product semantics remain intact; four additive UIA landmarks were introduced. The dormant Completion Preview remains dormant.

M5 verification passed: Debug solution build 0 warnings/0 errors; full non-UI suite 2324/2324; five real WPF screenshots at the current 2560 x 1440 host; IdeOnly clean package. Dirty Navigation and Save Preflight were both cancelled through their real modal paths and retained the dirty editor state. The authoritative ledger is `Docs/UI-MODERN-PROGRAM-R1_M5_StageLedger.md`.

M5 final rollback package: `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M5.Accepted.Rollback.zip`; 963 entries; 10,451,406 bytes; SHA-256 `765FE2BC823CD578C827536125AF504A077842FBED29312F433BEF58ADC54C7C`; zero forbidden package entries.

M6-A is completed. `UI-MODERN-PROGRAM-R1-M6-A-Fix5 WindowLayoutGlyphClip` moves only the lower Window Layout chevron from the clipped Y=16 boundary to Y=15 inside its existing 16 x 16 slot; button dimensions, spacing, AutomationId and command behavior remain unchanged. The existing opt-in main-path UIA harness now contains `M6A_ShellResponsiveLandmarksAndKeyboardPaths_RemainReachable`, reuses current window/tab/file helpers, reads TextBox value text when UIA Name is empty, and falls back to scanning dynamic AvalonDock AutomationIds when a UIA property condition cannot resolve them.

M6-A runtime verification passed on the available 2560 x 1440 monitor with a 2560 x 1392 WorkArea at 96 DPI: 1920 x 1040 DIP and 1280 x 800 DIP window profiles kept the toolbar endpoint buttons, Output, Project Explorer and current-document status inside Shell bounds; Shift+Tab/Tab traversed Project Explorer <-> Window Layout; the AI model selector passed F4 expand and Escape collapse. Restore passed; Debug solution build passed with 0 errors and one pre-existing CS8602 warning; affected Shell/visual/UIA boundary tests passed 87/87; the real M6-A UIA smoke passed 1/1; full non-UI tests passed 2332/2332. The AvalonEdit runtime UIA provider gap remains `UI-MODERN-M1-A11Y-001`; M6-A does not synthesize peers and physical 150% DPI / mixed-monitor hardware remain NotRun. `UI-MODERN-M6A-UIA-001` separately records that the older edit/completion/add-property smoke still treats clean-file `RevertInMemoryChangesButton.IsEnabled` as a load-complete signal and currently fails there, so the complete opt-in UIA suite is not claimed green; repayment belongs to a narrow editor-accessibility/main-path task.

M6-B is completed. The exact audit is `Docs/UI-MODERN-PROGRAM-R1_M6B_ZeroReferenceAudit.md` and the current ledger is `Docs/UI-MODERN-PROGRAM-R1_M6_StageLedger.md`. Fourteen safe Shell history keys were removed. Fourteen `IdeSecondary*` compatibility definitions and their 56 production references were retired in favor of existing `IdeFieldRegistry*` / `IdeAssist*` authority; `Resources/Styles/IdeSecondaryWindowStyles.xaml` and its App merge entry were removed. Final production `IdeSecondary` occurrences are zero and the application resource inventory contains 379 explicit keys with zero duplicates. Five migrated windows retained identical AutomationId, binding and Click-handler sets.

M6-B verification passed: the pre-change IdeOnly rollback package has 963 entries and SHA-256 `8C3BC4C8BA43810B2734EF5D792A878D8E4735A47399A5AC5B6EFC67394057A6`; static resource/XAML audit passed; Debug build passed with 0 warnings/0 errors; affected boundary tests passed 64/64; full non-UI tests passed 2332/2332; hidden real startup produced a main-window handle and did not exit early. M6-B added no screenshot set because it is visual-neutral cleanup; M6-C owns the final screenshot index and final clean package.

Current next entry: separately gated `M6-C final closure`. Completion, Peek, Find References, Dirty Navigation and Save Preflight semantics remain frozen.

`UI-MODERN-PROGRAM-R1-M6-A-Fix3 ComboBoxFullSurfaceHitTarget` completed on 2026-07-23 after explicit user approval. The canonical `UiComboBoxStyle` now places its existing `DropDownToggle` across the complete control surface beneath the non-hit-test selection presenter; the editable text box remains above it in the text region. This fixes whole-surface opening for the AI model selector and consistently benefits Search, Issues and Field Registry consumers without Shell event handlers, duplicate templates, public API changes or AI/model lifecycle changes. XAML parse passed; Debug build passed with 0 errors and one pre-existing CS8602 test warning; visual tests passed 16/16; visual/Shell boundary tests passed 47/47; full non-UI tests passed 2324/2324. Physical visual confirmation remains part of M6-A rather than being inferred from automation.

`UI-MODERN-PROGRAM-R1-M6-A-Fix4-R1 WorkAreaBoundedMaximize` completed on 2026-07-23 after the user approved the reliability-amended native-window contract. The existing main-Shell `HwndSourceHook` now handles `WM_GETMINMAXINFO` in Win32 physical pixels and writes only `ptMaxPosition` / `ptMaxSize` from the current monitor WorkArea; invalid handles, monitor lookup failures and invalid geometry fail open to Windows. The path remains gated by the main maximize region, so AvalonDock floating windows with `maximizeRegion: null` are unchanged. Shell XAML, `WindowState="Maximized"`, 1280 x 800 RestoreBounds, Snap Layout hit testing, Dock persistence and protected product semantics remain unchanged. Debug build passed with 0 errors and one pre-existing CS8602 test warning; targeted Chrome/Shell/work-area tests passed 67/67; full non-UI tests passed 2332/2332. An isolated real Debug Shell started maximized at 0,0-2560,1392 on a 0,0-2560,1440 monitor whose WorkArea was 0,0-2560,1392; restore remained 1280 x 800 and re-maximize returned to the WorkArea. Hardware layouts with taskbars on other edges and mixed-DPI monitor transitions remain represented by pure native-coordinate tests but are not claimed as physical hardware evidence.

---

## UI-MODERN-M2-R2 cohesive Shell modernization completed

On 2026-07-22 the user authorized the reviewed cohesive M2 package and requested that computer control be avoided where possible. The authoritative contract and completion record is `Docs/UI-MODERN-M2-R2_CohesiveShellModernizationContract.md`.

The accepted earlier Row 1 floating-content placement remains the drag baseline. The Shell Search command now restores a registered minimized floating host by ContentId, then performs one `DispatcherPriority.Loaded` host activation/content focus dispatch. Floating hosts retain minimize, close-to-hide, native caption dragging, double-click and edge resize; the dedicated maximize button, `Shell.Dock.FloatingHost.MaximizeRestoreButton`, and project-owned HTMAXBUTTON hover region are removed. Main Shell maximize/restore remains unchanged.

Single-title composition is now model-local: `LayoutAnchorablePaneControl.Model.IsDirectlyHostedInFloatingWindow && Model.ChildrenCount == 1` collapses the inner header. Docked panes and multi-pane/tab navigation keep their headers. `UiFontFamily` is `Segoe UI Variable Text, Microsoft YaHei UI, Segoe UI`; an application Window style sets only FontFamily, floating explicit style repeats that authority, and code surfaces remain Consolas. Main menu adoption now has one ShellTheme authority and 4-DIP top-level horizontal padding. Screenshot review produced M2D-Fix1: the visible IDE-name TextBlock and its grid column were removed, leaving icon -> menu -> drag region -> caption buttons; the Window Title binding remains available to taskbar, Alt+Tab and system semantics.

Verification passed: restore; Debug solution build with 0 warnings/0 errors; combined UI/Shell boundary tests 55/55; full non-UI tests 2313/2313; IdeOnly clean package with 952 files. No computer-control smoke was run by user preference, so the next action is a short manual minimize/Search restore and 1920 x 1080 typography/menu review. Physical 1280 x 800 and 125%/150% DPI remain manual. The H1 child-HWND UIA provider gap and SEARCH-1 real Search implementation remain separate and open.

No public C# API, dependency, project file, ContentId, Dock Home/v2 persistence, parser/editor/AI/Field Registry/Completion/Hover/Quick Peek/Diagnostics/Save/backup/rollback, BuiltIn data, or legacy behavior changed.

---

## UI-MODERN-M1-R2 + UI-DOCK-5 continuous package accepted

On 2026-07-22 the user accepted continuous execution of the reviewed `UI-MODERN-M1-R2-Fix1 + UI-DOCK-5` direction and waived intermediate approval waits. The package still requires real screenshot evidence, targeted verification at every visual/runtime card, rollback anchors before custom chrome and persistence activation, and a hard stop on any Snap, docking, identity, migration, build, or protected-semantic regression.

The authoritative successor contracts are `Docs/UI-MODERN-M1-R2_PreviewParityContract.md` and `Docs/UI-DOCK-5_SearchFloatingTopologyContract.md`. They do not rewrite the completed UI-DOCK-4 history. M1G keeps its original verification/closure meaning; Search topology is owned by UI-DOCK-5. The frozen preview under `Docs/UiVisualBaselines/UI-MODERN-M1-R2-LayoutDirection-DarkReference.png` is layout-only evidence; the current package remains light-theme only.

The new product direction keeps the same seven ContentIds but makes `Tool.Search` a hidden-by-default Floating-home tool with preferred 560 x 620 geometry. Problems and Output remain the only default Bottom tabs; Find References remains on-demand. `shell-layout.v2.xml` becomes current presentation authority, with one-way safe v1 migration that normalizes only Search and leaves v1 untouched. Real Search/Replace remains separately gated; all visible mock Search data is removed while the old public mock members remain temporary compatibility debt until SEARCH-1.

Current next entry: M1D-R2A FoundationDefectCorrection. No C# public API, dependency, project-file, business semantic, dark-theme, or legacy change is authorized.

---

## UI-MODERN-M1-0R Visual System Foundation revised final contract proposed

The user accepted the M0 visual proportions and completed the UI-DOCK series before returning to modern UI work. A current-code regression confirmed that the old M0 Search-host audit is now partly historical: Search is embedded as the Shell-owned `Tool.Search` / `SearchToolView` and has stable Search AutomationIds, but remains read-only placeholder/mock behavior. `UI-MODERN-M1` may modernize that surface only; real Search execution and placeholder retirement remain a separate `SEARCH-1` behavior contract.

The revised final M1 contract is `Docs/UI-MODERN-M1_VisualSystemFoundationContract.md`. It preserves the 1920 x 1080 M0 geometry, AvalonDock 4.74.1, seven ContentIds, 300/260 compiled layout, Shell-owned content instances, deterministic Home recovery, and `shell-layout.v1.xml`. It adds no public API, dependency, project-file change, dark-theme selector, Search behavior, or protected business semantic.

Reliability review separated template definition from production adoption. M1A establishes tokens; M1B and M1C define/test explicitly keyed core and collection templates with no intended visual change; M1D explicitly adopts them only in Shell/Search and creates Visual Stop 1; M1E implements fail-closed integrated Shell chrome at Visual Stop 2; M1F implements fail-closed AvalonDock floating-host chrome at Visual Stop 3; M1G performs package verification and governance closure. `IdeSecondaryWindowStyles.xaml` and all secondary-window XAML are frozen throughout M1. M1B/M1C may not rebase existing compatibility styles or add application-wide implicit styles.

The revised contract adds an exact WPF template-part/behavior matrix and requires unique clean-source rollback archives plus recorded source-file hashes before M1E and M1F. Custom chrome is not accepted unless Windows Snap Layout/system commands/work-area/DPI and AvalonDock drag/re-dock/close-to-Home/persistence all pass.

Current stop: contract approval. Do not modify production XAML, C#, themes, tests, project files, or packages until the user explicitly confirms `确认 UI-MODERN-M1 最终契约`. After confirmation, enter M1A only.

---

## UI-DOCK-4 Layout Persistence completed

The user approved continuous execution of the final R4 UI-DOCK-4 contract on 2026-07-22. UI-DOCK-4A through UI-DOCK-4F are now complete. The authoritative implementation and verification record is `Docs/UI-DOCK-4_LayoutPersistenceContract.md`.

AvalonDock 4.74.1 deserialization replaces the layout model graph, so the final implementation resolves every live tool through the seven stable ContentIds. Shell continues to own and reuse the original content instances; compiled `ShellWindow.xaml` remains the sole default authority; the session captures that default in memory and restores through the same validated native-serializer path. Bottom/Right Home recovery still uses deterministic profiles and `AddToLayout` fallback when no suitable Pane remains.

The user-local presentation-only file is `%LOCALAPPDATA%\RA2IniEditor\IDE\Layout\shell-layout.v1.xml`. Reads are strict UTF-8 without BOM, bounded to 1 MiB, DTD-disabled, root/identity preflighted, post-restore validated, and quarantined to one bounded invalid path when necessary. Accepted closes save atomically; cancelled closes do not write; Reset immediately persists the compiled default. Titles/bindings, capabilities, AutomationIds, event subscriptions, Bottom state, and Project Explorer visibility are rebound from compiled invariants. Business/view-model/editor/Search/AI/Field Registry state is never serialized.

Floating geometry is validated against current monitor work areas. Same-DPI connected monitors preserve valid negative coordinates; unreachable, invalid, and oversized windows are safely recovered. Mixed-DPI cross-monitor conversion is intentionally treated as unreliable and falls back to the Shell monitor instead of guessing. Explicit secondary-monitor disconnect and 125%/150% scaling smoke remain hardware-only manual coverage, not an implementation gap.

Final evidence: restore/build passed; Dock targeted tests passed 35/35; full non-UI suite passed 2305/2305; IdeOnly clean package passed with 940 source files. Real Debug-process Reset, normal close, persisted XML inspection, and second startup passed. The smoke file contained exactly one of every approved identity, no BOM, and no business markers. A Codex-packaged launch virtualized LocalAppData under its package cache, which is a host-environment fact rather than an application path change. No XAML/theme/project/dependency/public API or protected business semantic changed. The next UI work returns to the separately gated UI-MODERN visual-stage plan.

---

## UI-DOCK-3R completed; AI-REL-TD-001 narrow reliability closure

The user approved the UI-DOCK-3R reliability amendment and later confirmed the remaining manual dock test passed. The production snapshot now owns deterministic Home profiles and ordering through the internal `ShellDockLayoutCoordinator`; floating close returns the same managed tool to Bottom or Right, empty Home groups are reconstructed, Shell shutdown bypasses recovery, and repeated reset restores the compiled 300-DIP right / 260-DIP bottom layout without duplicate tools. The toolbar and View menu expose Return Floating Tools Home and Reset Default Layout through the frozen AutomationIds in `Docs/UI-DOCK-1_AvalonDockShellContract.md`.

Codex runtime smoke observed Search floating at 800 x 420, floating-close recovery to Bottom, toolbar batch return, and default reset. The user supplied the final manual visual acceptance. No layout persistence, Search behavior, parser, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, editor semantics, BuiltIn data, or legacy behavior changed.

The UI-DOCK-3R full-suite gate originally exposed `AI-REL-TD-001`: linked-token cancellation could reach the HTTP handler before the authoritative termination callback recorded `TotalTimeout`, allowing a handler-triggered late user cancellation to win. The user separately authorized a narrow AI reliability fix. `DeepSeekRa2AiClient` now lets the first request-local termination source atomically record its cause before it propagates cancellation to the HTTP/SSE request. Existing user-cancel, total-timeout, and streaming-idle classifications remain distinct; no retry, replay, model fallback, new failure kind, public API, or test-only production hook was added.

Final evidence: restore passed; the solution built with 0 errors and one pre-existing CS8602 warning; the exact regression passed 20/20; all `DeepSeekRa2AiClientTests` passed 62/62; two consecutive full-suite runs passed 2278/2278; the IdeOnly clean package passed with 934 source files at `artifacts/RA2IniEditor.IDE.SourceClean.zip`. `AI-REL-TD-001` and the UI-DOCK-3R verification gate are closed. UI-DOCK-4 persistence remains deferred and requires a separate approved contract.

---

## UI-MODERN-M0 Visual Baseline And WPF Dimension Contract completed; awaiting visual approval

The user authorized continuous execution of the modern UI plan with a mandatory stop after every visual stage. `UI-MODERN-M0A` through `M0D` are complete as a documentation/visual-baseline package; no production XAML or runtime behavior was changed.

M0A froze the four approved references under `Docs/UiVisualBaselines/` and recorded their exact 1672 x 941 pixel dimensions and SHA-256 hashes in `Docs/UI-MODERN-1_CanonicalSurfaceVisualParityContract.md`. These images are immutable design-intent evidence; illustrative file names, values, counts, search results, and AI text are not runtime fixtures.

M0B built the IDE-only solution successfully and captured the current real WPF baselines: `Current-Shell-Light.png`, `Current-Search-Light.png`, and `Current-FieldRegistry-Light.png`. The current Search surface is confirmed to be a 720 x 480 native standalone placeholder with mock results and no AutomationIds. The current Field Registry Center is 1040 x 700 with custom chrome but retains a dense form/DataGrid presentation.

M0C audited 25 XAML files: 615 StaticResource references, 28 DynamicResource references, 78 hard-coded hex-color occurrences, and 393 AutomationId declarations. The P0 gaps are incomplete shared templates, mostly static theme lookup, missing root layout rounding/DPI policy, Search automation absence, and inconsistent native/custom secondary-window chrome. Full evidence and the M1 migration order are in `Docs/UI-MODERN-1_ControlDpiAutomationAudit.md`.

M0D converted the references into deterministic WPF/DIP contracts. M0D-Fix2 supersedes the earlier small-window and 1920 x 1040 work-area diagrams. The user supplied `Docs/UiVisualBaselines/UI-SHELL-1920x1080-LayoutReference.png` (2559 x 1389, SHA-256 `B3C010577AB160DBFD9FD4DD5CA1A7C472D6A855461ABBA0AB9D25FA2DF0E186`) as the geometry reference and explicitly set the default design resolution to 1920 x 1080. The resulting Shell has a compact 30-DIP title/menu band and 32-DIP toolbar; a 1616-DIP editor-side workspace (84.2%); a 4-DIP splitter; and a 300-DIP right tool that spans the complete 994-DIP workspace height. The bottom tool is 260 DIP and is nested only under the editor column. With bottom tools open, the editor is 1616 x 700 DIP; collapsed it is 1616 x 964 DIP. Search uses the same bottom region and never spans beneath the right tool. The user reference is authoritative for proportions only, not Visual Studio content, extensions, pet overlay, icons, or dark colors. Field Registry remains 1040 x 700 DIP with 156/552/300 navigation/list/details columns. Responsive and DPI rules remain in `Docs/UI-MODERN-1_WpfDimensionSpec.md` and `Docs/UI-MODERN-1_ResponsiveLayoutSpec.md`.

Verification evidence: `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` passed with 0 warnings and 0 errors before the real-WPF capture. The three final dimension diagrams were rendered deterministically at 1600 x 1000 and visually inspected after correcting a first-pass annotation newline defect. No tests or clean package were required for this DocsOnly package. Legacy was not restored.

Current stop is the renewed user visual-review gate for M0D-Fix2. Do not begin `UI-MODERN-M1 Visual System Foundation` until the user approves or amends the 1920 x 1080 Visual Studio-ratio diagrams. The compact title/menu band implies project-owned WPF chrome in the later Shell stage; it must preserve Windows system commands, dragging, maximize/restore, Snap Layout, keyboard access, automation, and per-monitor DPI. M1 must otherwise use project-owned `DynamicResource` tokens, `Style`/`ControlTemplate`, and vector geometries; it must not use image generation or raster controls and must preserve parser, Field Registry semantics/priority, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, AI behavior, BuiltIn data, and legacy behavior.

---

## FR-DQ-4 PlaceholderRetirementAndTrustCleanup completed

The user confirmed the corrected continuous FR-DQ-4 contract. Rollback anchors are preserved in `artifacts/RA2IniEditor.IDE.SourceClean.FR-DQ-4-0.Rollback.zip` and `artifacts/builtin-yr-ares-phobos-fallback-v3.2.FR-DQ-4-0.Rollback.fields.json`. The stable candidate manifest is `Docs/FieldRegistryPlaceholderRetirementCandidates_2026-07-20.csv`; dispositions are evidence decisions, not JSON array indexes.

Completed checkpoints:

- 4A inventoried 2947 candidate rows without changing runtime JSON.
- 4B excludes diagnostic-only trust levels from field-name Completion while retaining lookup/Hover/Diagnostics. Real AA/AG tests passed.
- 4C removed 12 broad Techno inferred fallbacks only where a ModEnc `[General]`-backed Global replacement exists.
- 4D checkpoint 1 promoted and rewrote 30 official-source rows: Banner 7, Terrain/Tiberium 9, Country UI 11, and Phobos UI settings 3. Context corrections moved `ConditionYellow.Terrain` and the three `[Sidebar]/[Phobos]` settings to Global.
- 4D checkpoint 2 additionally promoted EVA Types, damaged Aircraft images, Ares prerequisites, sell feedback, engineer/infantry/building-upgrade/prone/slave-owner families; three unsupported fixed EVA rows were quarantined and `DefaultDisguise / Infantry` was superseded by its official Side row.
- 4D checkpoint 3 promoted 10 directly documented small-extension/Sound rows, quarantined three contradictory or unsupported small-context rows, then removed 298 uniform templates from ArtObject, Building, Warhead, Weapon, Global and Vehicle while retaining all non-template definitions in those contexts.
- 4D completed by splitting the remaining 1147 broad Techno templates into five stable key ranges. Every range matched runtime removals to manifest transitions exactly and passed the scoped test gate. Uniform inferred templates are now zero.
- 4E split all 810 remaining auto-extracted rows into five stable source/key ranges: two Phobos ranges of 175 rows and three Yuri ranges of 154/154/152 rows. Source metadata alone was insufficient because extraction had produced provable context errors, so these rows were quarantined without declaring the underlying keys nonexistent.
- 4F normalized 208 `community-reviewed-*` labels to the existing manual-curated trust family, retained 66 specific inferred rows without promotion, and repaired five required identity/garrison rows whose descriptions were mojibake and quality was empty.
- 4G verified Completion, Hover, Quick Peek, Diagnostics and highlighting surfaces against the real v3.2 provider. A fixed Vehicle sample containing three quarantined rows produces exactly three Unknown Key warnings; the 4-0 baseline contained all three identities and the current runtime contains none.
- 4H completed IDE-only restore/build/full test/clean package gates and synchronized product-facing and governance documentation.

Final facts: runtime BuiltIn rows `2604`; uniform inferred templates `0`; auto-extracted rows `0`; empty quality `0`; unrecognized runtime trust `0`; exact identity duplicates `0`; PendingManualReview `0`. Manifest counts are `DiagnosticOnlyKeep=349`, `SupersededRemove=13`, `PromoteAndRewrite=65`, `RetainReviewed=259`, and `Quarantine=2261`. Surface-focused tests passed `900/900`; full non-UI tests passed `2274/2274`; restore/build/IdeOnly package gates passed. No repository `.ini` corpus exists, so 4G records representative rather than real-project occurrence counts.

One 4D mechanical rewrite transported UTF-8 Chinese through the Windows PowerShell default console code page and corrupted the JSON. The task stopped, restored the exact 4-0 JSON rollback hash, then replayed reviewed identities with explicit UTF-8, cardinality assertions, round-trip validation and atomic replacement. Do not use a non-ASCII console pipeline for future Field Registry rewrites.

FR-DQ-4 is closed. A later phase should begin from the final clean package and preserve every semantic freeze outside the approved key Completion filter and BuiltIn data cleanup. Any recovery of quarantined rows must be evidence-driven against the 4-0 rollback anchors and stable manifest identities.

Authoritative documents: `Docs/Codex_RA2IniEditor_IDE_FR_DQ_4_PlaceholderRetirement.md`, `Docs/FieldRegistryPlaceholderRetirementAudit_2026-07-20.md`, and `Docs/ContextCapsule_FR_DQ_4.md`.

---

## AI-REL-3 ProviderTrustPrivacyAndResourceHardening completed

```text
Contract state: Confirmed by user on 2026-07-20; completed through AI-REL-3I.
Current trusted runtime baseline: AI-REL-3 completed.
User direction: remove Mock from the product, replace provider choice with DeepSeek V4 Flash / V4 Pro model choice, and default to V4 Flash.
Official facts verified on 2026-07-20: deepseek-v4-flash and deepseek-v4-pro are valid API ids; DeepSeek V4 defaults to thinking enabled when no explicit toggle is sent.
Confirmed correction: both models explicitly use non-thinking mode, max_tokens=8192, UI typed selection is request authority, and DEEPSEEK_MODEL no longer overrides it.
Mock scope: delete production FakeRa2AiClient and all Shell Mock branches; deterministic substitutes may exist only inside tests.
Continuous package: 3-0 clean-package rollback anchor; 3A typed model authority; 3C1 one-read configuration snapshot; 3C2 endpoint/numeric trust; 3B Mock retirement/UI selector and overlong-prompt rejection; 3D shared sanitizer; 3E prompt budgets; 3F output/diagnostics/response invariants; 3G Shell bounds/transparency; 3H loopback/failure verification; 3I full verification/docs closure.
Explicit non-goals: no automatic retry, Retry-After, provider/model fallback, thinking selector, persistence, new dependency, file mutation, Field Registry change, or broad Shell redesign.
Authoritative confirmed contract: Docs/Codex_RA2IniEditor_IDE_AI_REL_3_ProviderTrustPrivacyResource.md
```

AI-REL-3 is complete. The production AI path now has typed Flash/Pro model identity with Flash default, one-read configuration authority, strict endpoint/numeric trust, shared outbound sanitization, deterministic prompt/output/UI resource bounds, invariant response factories, and request-local safe diagnostics. Production Mock/Fake is removed and test substitutes remain test-only. 3G tests passed 51/51; 3H loopback/failure tests passed 48/48; final build passed with 0 warnings/0 errors; full tests passed 2171/2171. Runtime UI smoke confirmed Flash default, exactly two model options and safe status/footer. Minimal live Flash/Pro requests each passed once. The clean 3-0 rollback package is preserved separately; the final IdeOnly package is generated after the documentation flush. See `Docs/ContextCapsule_AI_REL_3.md` for the compact completed-state packet.

---

## AI-REL-1 TimeoutRecovery completed

```text
The internal Ra2AiConversationTurn now owns IsContextEligible. Conversation history requires Completed && IsContextEligible before truncation budgets are applied.
Shell associates each streamed assistant card with its initiating user card. Completed terminal rendering keeps both eligible; Incomplete/Error rendering excludes both and adds an explicit restore-only action.
“恢复提示词” restores the trimmed submitted prompt only when the input is empty, focuses the input, never auto-sends, and never overwrites existing content.
Dynamic AutomationIds: AiAssistant.RestorePromptButton and AiAssistant.RestorePromptStatus.
No automatic retry, request replay, backoff, frozen request snapshot, persistence, or API-key storage was added.
Verification: provider tests 15/15; AI/DeepSeek/Shell targeted tests 231/231; IDE-only build 0 warnings / 0 errors; full tests 2083/2083; IdeOnly clean package passed with 898 files.
Debug WPF launch plus AI panel/advanced-area visibility passed. On 2026-07-20 the user confirmed the manual restore, focus, non-overwrite, no-auto-send, and failed-context-isolation smoke passed.
Authoritative stage document: Docs/Codex_RA2IniEditor_IDE_AI_REL_1_TimeoutRecovery.md
```

AI-REL-1-UI-VERIFY is resolved. No AI-REL-1 code, compatibility, TODO, or verification debt remains.

Historical next entry at that time was `AI-REL-2A FailureTaxonomyContract`; AI-REL-2 is now completed and this line is superseded by the AI-REL-3 proposed entry above.

No external public API, serialization, DeepSeek transport, SSE parser, Pipeline, PromptBuilder, request lifecycle, `ShellWindow.xaml`, Field Registry, completion, Hover, diagnostics, save preflight, project file, or legacy behavior changed.

---

## AI-STREAM-3 ShellIncrementalRendering completed

```text
Phase completed: AI-STREAM-3 ShellIncrementalRendering
Shell uses Ra2AiAssistantPipeline.SendStreamingAsync and preserves the AI-STREAM-2 transport boundary.
Each request owns one InProgress assistant message card. Ordered deltas update that same card through a thread-safe Ra2AiIncrementalTextBuffer.
WPF updates are coalesced at 50 ms or 512 pending characters; at most one immediate Dispatcher flush can be queued.
Streaming presentation uses lightweight text Runs. The existing Markdown renderer and code-block copy controls are created once at terminal presentation.
Ra2AiResponse.Text remains terminal authority. A successful response whose callback accumulation differs from response.Text fails closed as Error and is excluded from conversation context.
Success maps to Completed. Incomplete, Cancelled, and Timeout map to Incomplete. ProviderError, MissingConfiguration, consumer failures, and consistency failures map to Error.
Partial non-success text remains visible and copyable; its terminal status is presented separately and is not mixed into copied/model text.
The chat follows output only when already within 24 px of the bottom, preserving deliberate upward scrolling.
AI-STREAM-0 request identity, cancel/busy behavior, stale flush rejection, window-close cancellation, timer cleanup, and request-session disposal remain unchanged.
Existing AI AutomationIds remain stable. ShellWindow.xaml changed only by naming the existing chat ScrollViewer.
Verification: 5/5 buffer tests; 68/68 AI/Shell targeted tests; IDE-only build 0 errors with one pre-existing CS8602 test warning; 2081/2081 full tests; Mock UI smoke passed; user-authorized live DeepSeek multi-delta plus cancel-after-partial smoke passed; IdeOnly clean package passed with 897 files.
Clean package: artifacts/RA2IniEditor.IDE.SourceClean.zip
```

Live DeepSeek verification observed the request-owned InProgress card, disabled Generate / enabled Cancel state, ordered partial text, successful cancellation, retained partial text, Incomplete terminal status, re-enabled copy and Generate controls, and exclusion of the incomplete assistant turn from conversation context. No project or editor file was open, and the synthetic prompt contained no sensitive data.

The AI-STREAM-0 through AI-STREAM-3 line is complete. Timeout retry policy and broader XAML polish remain separate contract-gated topics; do not combine them with streaming maintenance without a new user-approved contract.

No public API, DeepSeek transport, SSE parser, PromptBuilder, Field Registry, completion, Hover, Quick Peek, diagnostics, save preflight, project file, or legacy behavior changed.

---

## AI-STREAM-2 StreamingTransportAndPipelineIntegration completed

```text
Phase completed: AI-STREAM-2 StreamingTransportAndPipelineIntegration
AI-STREAM-2A added the internal ordered Ra2AiContentDeltaHandler / IRa2AiClient.SendStreamingAsync contract, Ra2AiResponseKind.Incomplete, and partial-text plus finish-kind response metadata.
AI-STREAM-2B made DeepSeek SSE the canonical HTTP transport: stream=true, text/event-stream, ResponseHeadersRead, the existing DeepSeekRa2AiSseParser, strict UTF-8 decoding, and deterministic finish mapping.
The existing SendAsync API remains compatible and reuses the SSE transport while mapping Incomplete back to ProviderError for the current non-incremental Shell surface.
The application total timeout remains 120 seconds by default. A 60-second inter-content idle timeout starts only after the first content delta; keepalive/metadata frames do not reset it.
Accumulated assistant content is limited to 1 MiB. Partial text is retained only in non-success responses for cancellation, timeout, incomplete finish, malformed/ended streams, and size failures.
The DeepSeek client owns/disposes HttpRequestMessage, HttpResponseMessage, response Stream, and StreamReader. The SSE parser still does not dispose caller-owned readers.
Delta callbacks are ordered, non-concurrent, and naturally backpressured. Consumer callback failures propagate after resource cleanup and are not mapped as provider failures.
AI-STREAM-2C added Ra2AiAssistantPipeline.SendStreamingAsync. PromptBuilder still builds exactly one canonical request, and Pipeline remains independent of WPF/Dispatcher.
Verification: combined AI streaming/lifecycle tests passed 74/74; IDE-only build passed with 0 warnings / 0 errors; full tests passed 2076/2076.
```

Next recommended package: `AI-STREAM-3 ShellIncrementalRendering`.

Mandatory AI-STREAM-3 contract:

```text
Use Ra2AiAssistantPipeline.SendStreamingAsync; do not call DeepSeek transport directly from Shell.
Create one request-owned assistant turn in InProgress state and update that same turn through bounded Dispatcher coalescing rather than creating one control per delta.
Preserve exact final text order and flush the pending buffer before terminal presentation.
Success -> Completed; Incomplete/Cancelled/Timeout -> Incomplete; ProviderError/MissingConfiguration/consumer failure -> Error.
Only Completed turns may be reused by conversation context.
Preserve AI-STREAM-0 single-request identity, cancellation, stale-completion rejection, close cancellation, and busy-state rules.
```

No Shell/XAML, AutomationId, external public API, Field Registry, parser semantics, completion, Hover, Quick Peek, diagnostics, save preflight, project file, or legacy behavior was changed in AI-STREAM-2.

---

## AI-STREAM-1 / AI-STREAM-1A StreamEventAndSseParser completed

```text
Phase completed: AI-STREAM-1 StreamEventAndSseParser
Hardening addendum completed: AI-STREAM-1A TerminalSemanticsAndProtocolHardening
Ra2AiStreamEvent represents ordered ContentDelta or protocol Completed events.
Completed is emitted only after data: [DONE] and carries the last mapped finish reason: Stop / Length / ContentFilter / ToolCalls / InsufficientSystemResource / Unknown.
DeepSeekRa2AiSseParser supports blank-line SSE framing, CRLF, multi-line data fields, keepalive comments, role/reasoning-only frames, and empty-choices usage frames.
When present, object must be chat.completion.chunk, stream id must be non-empty and stable, and choice index 0 is selected independently of array order.
A single unindexed choice remains supported for compatibility.
Malformed JSON/shape, mismatched stream identity, oversized events, cancellation, and EOF before [DONE] fail explicitly.
One accumulated SSE data event is limited to 1 MiB; the caller retains TextReader disposal responsibility.
Verification: parser tests passed 24/24; IDE-only build passed with 0 warnings / 0 errors; full tests passed 2058/2058.
```

AI-STREAM-1/1A was followed by the completed AI-STREAM-2 package recorded above.

Mandatory AI-STREAM-2 contract:

```text
Only Stop with non-empty accumulated content maps to Success.
Length / ContentFilter / ToolCalls / InsufficientSystemResource / Unknown and every interrupted or malformed stream remain non-success terminal states.
Partial text may remain visible but is Incomplete/Error and is excluded from conversation context.
The existing application-level total timeout remains authoritative; first-meaningful-content and inter-content idle timeouts must be explicit, and keepalive does not count as model content.
The transport adds an accumulated-response limit, owns/disposes all HTTP stream resources, and is tested with fragmented UTF-8 through a real StreamReader.
```

Mandatory AI-STREAM-3 contract:

```text
Do not dispatch every token to WPF.
Coalesce updates by a bounded time/character threshold (recommended approximately 30-60 ms) without changing final text order.
Preserve AI-STREAM-0 request identity, cancellation, busy-state, and completed-context rules.
```

No streaming transport, Pipeline integration, Shell incremental rendering, retry, file mutation, Field Registry behavior, XAML, project file, or legacy behavior was added in AI-STREAM-1/1A.

---

## AI-STREAM-0 RequestLifecycleHardening completed

```text
Phase completed: AI-STREAM-0 RequestLifecycleHardening
DeepSeek timeout has one application-level authority: 120 seconds by default, with DEEPSEEK_TIMEOUT_SECONDS override.
The factory-owned shared HttpClient uses InfiniteTimeSpan and cannot terminate the request ahead of the application timeout.
Ra2AiRequestLifecycle / Ra2AiRequestSession own the single-active-request identity and CancellationTokenSource lifetime.
Cancel does not clear busy state early; only completion of the matching request restores Generate, Clear, Cancel, and model-selector state.
Shell close requests cancellation of the current AI operation.
Ra2AiConversationTurnState distinguishes Completed / InProgress / Incomplete / Error.
Ra2AiConversationContextProvider reuses only Completed turns, so cancellation and failure messages do not contaminate later prompts.
No SSE parsing, streaming transport, incremental rendering, retry, file mutation, or Field Registry behavior was added.
Verification: dotnet restore passed; IDE-only build passed with 0 warnings / 0 errors; full tests passed 2034/2034.
```

AI-STREAM-0 was followed by the completed AI-STREAM-1 / AI-STREAM-1A parser package above.

---

## AI-TimeoutAndContextDisclosure completed

```text
Phase completed: AI-TimeoutAndContextDisclosure
DeepSeek timeout is represented by the internal Ra2AiResponseKind.Timeout state instead of ProviderError text inspection.
The default DeepSeek timeout is 120 seconds and remains overridable through DEEPSEEK_TIMEOUT_SECONDS.
User cancellation remains Cancelled and all terminal states clear the existing sending state.
The AI panel now accurately discloses that explicit DeepSeek sends include bounded current-document context, field evidence, diagnostics summary, and recent conversation context.
No automatic retry, streaming, whole-project context, Apply/Insert, editor mutation, or Field Registry write was added.
```

This historical phase was superseded on the AI reliability line by `AI-STREAM-0 RequestLifecycleHardening`.

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

## AI-REL-2 Continuous Failure Taxonomy completed

```text
AI-REL-2A through AI-REL-2E completed on 2026-07-20.
Ra2AiResponse now carries an internal orthogonal FailureKind while ResponseKind remains terminal state and StreamFinishKind remains model completion reason.
DeepSeekRa2AiClient maps configuration, HTTP status, status-less network exceptions, local total/idle timeouts, protocol failures, and response-size failures without reading provider error bodies or exposing raw exception text.
User cancellation, total timeout, and streaming idle timeout use request-local first-signal-wins attribution; no static request state was introduced.
Shell selects fixed safe Chinese guidance through DeepSeekRa2AiFailureUiMessageFormatter. It does not parse or display response ErrorMessage.
Incomplete text remains visible and copyable, is marked incomplete, and remains excluded from later conversation context under the AI-REL-1 rules.
No automatic retry, Retry-After, request snapshot, persistence, API-key storage, new dependency, XAML/layout/AutomationId change, or Field Registry behavior was added.
Verification: stage tests 10/10, 63/63, 60/60; cross-stage tests 263/263; full tests 2115/2115; Debug WPF plus explicit DeepSeek live UI smoke passed; IdeOnly clean package passed with 903 files.
Authoritative document: Docs/Codex_RA2IniEditor_IDE_AI_REL_2_FailureTaxonomy.md
```

The AI reliability baseline is now AI-REL-2. Do not add retry behavior as a maintenance detail; retry requires a new contract covering duplicate requests, cost, privacy, idempotency, backoff, and observability.

---

## 1. Project Identity

RA2IniEditor.IDE is an INI-focused IDE for Red Alert 2 / Yuri's Revenge / Ares / Phobos mod files.

It is source-first:

```text
preserve original INI text
preserve comments
preserve ordering
preserve custom fields
add IDE assistance: completion, hover, quick peek, references, diagnostics, save preflight, backups, rollback
```

It is not the removed legacy table-style editor.

---

## 2. Active Package Boundary

Current package is **IDE-only**.

Active solution:

```text
RA2IniEditor.IDE.sln
```

Active clean package profile:

```text
IdeOnly
```

Do not restore:

```text
RA2IniEditor.sln
RA2IniEditor.csproj
legacy MainWindow
legacy table-style editor
legacy object workbench
old Key-Value DataGrid editor
old Country / Side manager
old object copy / weapon-chain copy workflows
```

---

## 3. Stable Product Decisions

### 3.1 Save preflight

Save preflight must remain:

```text
prompt-style / user may continue saving
```

It must not become:

```text
hard blocking gate
```

### 3.2 Field library purpose

BuiltIn / YR / Ares / Phobos field libraries are fallback/reference sources for:

```text
Completion
Hover
Quick Peek
Diagnostics reference
lowering false UnknownKey noise
```

They are not:

```text
hard authority
save blocker
only legal field source
```

### 3.3 UI direction

UI direction is IDE-like, not legacy form/table editor.

Main Shell should remain stable. Secondary windows may be improved, but by explicit contract only.

---

## 4. Current Accepted Baseline

Current accepted baseline:

```text
v0.4.96-pre.2 IDE-only Source Package Stabilization
Phase 0: HandoffArchiveIndex completed
Phase 1: SecondaryWindowsInventory completed
A15-1R2: borderless inspectors open near AvalonEdit caret and have close buttons
A15-2A: Field Registry surfaces inventory / UI contract completed
A15-2B: Field Registry Center / Manager read-only management layout completed
A15-2B-P: Field Registry Center / Manager visual polish completed
A15-2B-P2: Field Registry Center / Manager custom chrome completed
A15-2B-P3: Field Registry Manager scroll/localization refinement completed
A15-2B-P4: Field Registry Manager cleanup preview demotion completed
A15-2D: Field Editor / Allowed Values Editor custom chrome completed
A15-2E-1: Field Learning Wizard custom chrome completed
A15-2E-2: Field Learning Wizard workflow layout / bounded scroll areas completed
AI-0: DeepSeek-powered RA2 Modding Assistant architecture / safety contract completed
AI-1A: Right Tool Well inspection / implementation contract completed
AI-1B: Right Tool Well frame / empty AI page completed
AI-1B-P2: Chat-style composer / Advanced placement completed
AI-1B-P3: Chat panel visual refinement completed
AI-1B-P4: Composer minimalism / wrapping fix completed
AI-1C: Mock AI response display completed
AI-2A: Context provider / Field Registry retrieval contract completed
AI-2B: Caret-based current context provider / mock UI summary completed
AI-2C: Local Field Registry evidence retrieval completed
AI-2D: Bounded diagnostics summary integration completed
AI-3A: Prompt Builder contract completed
AI-3B: Deterministic Prompt Builder implementation completed
AI-4A: DeepSeek adapter contract completed
AI-4B: AI client abstraction / deterministic fake boundary completed
AI-4C: Fake client send flow wiring completed
AI-4D: DeepSeek-compatible adapter implementation completed
AI-4D-Fix: Environment-only DeepSeek API key policy completed
AI-4E-1: Provider mode UI state / Mock default completed
AI-4E-2: Minimal provider selection / DeepSeek live send flow completed
AI-4E-2-P: Provider Advanced layout fix completed
AI-4E-2-P2: Composer Enter-to-send completed
AI-4E-2-P3R: Minimal model ComboBox restore completed
AI-4E-2-P4: Chat action placement cleanup completed
AI-4E-2 DeepSeek endpoint hotfix completed
AI-5A: Stable draft output contract completed
AI-5B: Stable draft PromptBuilder update completed
AI-5C: Markdown response rendering / code block copy completed
AI-5C-P: Markdown rich rendering completed
AI-5C-P2: Markdown table / inline bold rendering fix completed
AI-5C-P3: Inline code rendering fix completed
AI-5B-Fix2: Draft evidence profiles completed
AI-6A: Conversation Context / Current Subject contract completed
AI-6B: Bounded conversation context extraction completed
AI-6C: Current subject / draft subject extraction completed
AI-6D: PromptBuilder conversation context / current subject integration completed
AI-6E: Context Summary UI polish / Current Subject display completed
AI-6F: Subject-aware Field Evidence Expansion completed
Icon-0A: Main toolbar icon inventory completed
Icon-0B: Main toolbar command contract completed
Icon-0C: Main toolbar state cleanup completed
Icon-S1: WPF vector icon resource contract completed
Icon-S2A: Resource dictionary scaffold / brush token mapping completed
Icon-S2B: Main toolbar P0 vector replacement completed
Icon-S2B-P: Main toolbar flat button chrome cleanup completed
Icon-S2B-P2: Main toolbar density / top chrome polish completed
Icon-S2B-P3: Top chrome palette / menu alignment polish completed
FR-DQ-2B-Apply: Batch A canonical Field Registry descriptions completed
FR-DQ-2C-Prep-Fix: Batch B verification input granularity completed
FR-DQ-2C-Verify-ManualApply: Batch B source verification and limited BuiltIn Hover hygiene patch completed
```

Recent reported verification:

```text
restore: not run in this environment
build: not run in this environment
test: not run in this environment because dotnet CLI is unavailable
static JSON validation: passed
manual clean package: passed, packaged file count 791
legacy: not restored
Shell main layout: unchanged
Field Registry semantics: Batch B descriptions updated only; provider priority / runtime lookup unchanged
```

Current next work:

```text
FR-DQ-2C-Verify-ManualApply completed. Batch B fields BuildCat / Crewed / Turret / ThreatPosed were source-verified against ModEnc. BuiltIn v3.2 was updated only for Batch B Hover hygiene: BuildCat / Crewed / Turret / ThreatPosed no longer expose direct placeholder or low-quality Batch B descriptions. Added exact source-backed rows for Crewed / Vehicle, Crewed / Aircraft, Turret / Vehicle, and Turret / Building; improved BuildCat / Building, Crewed / Building, ThreatPosed / Techno; changed BuildCat / Techno, Crewed / Techno, Turret / Techno, and ThreatPosed / AI to explicit non-canonical / broad fallback guardrail text. Provider priority, lookup / fallback / enrichment logic, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, user Global active pack, project files, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.
```

---

## 5. Important Existing Documentation

Current product docs:

```text
Docs/FeatureOverview.md
Docs/UserGuide.md
Docs/ReleaseChecklist.md
Docs/DeveloperNotes.md
```

Current architecture / phase docs:

```text
Docs/HandoffArchiveIndex.md
Docs/SecondaryWindowsInventory.md
Docs/FieldRegistrySurfacesUiContract.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/DocumentationMaintenance.md
```

Current task docs:

```text
Docs/Codex_RA2IniEditor_IDE_A15_2B_P_VisualPolish_Implementation.md
Docs/Codex_RA2IniEditor_IDE_A15_2B_P2_FieldRegistry_CustomChrome.md
Docs/Codex_RA2IniEditor_IDE_A15_2B_P3_ManagerScroll_Localization.md
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
Docs/Codex_RA2IniEditor_IDE_AI_3A_PromptBuilderContract.md
Docs/AiAssistantPromptBuilderContract.md
Docs/Codex_RA2IniEditor_IDE_AI_3B_PromptBuilderImplementation.md
Docs/AiAssistantDeepSeekAdapterContract.md
Docs/Codex_RA2IniEditor_IDE_AI_4B_ClientAbstraction_FakeBoundary.md
Docs/Codex_RA2IniEditor_IDE_AI_4C_FakeClientSendFlow.md
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
Docs/IconToolbarInventory.md
Docs/IconToolbarCommandContract.md
Docs/IconStyleGuide.md
Docs/IconConceptReview.md
Docs/IconSystemCoveragePlan.md
Docs/IconVectorResourceContract.md
Docs/Codex_RA2IniEditor_IDE_Icon_S2A_ResourceScaffold.md
Docs/Codex_RA2IniEditor_IDE_Icon_S2B_MainToolbarVectorReplacement.md
Docs/FieldRegistryEffectiveDescriptionAudit.md
Docs/FieldRegistryDescriptionSourcePolicy.md
Docs/FieldRegistryDescriptionBackfill_P0A_Candidates.md
Docs/FieldRegistryDescriptionVerification_BatchA.md
Docs/FieldRegistryDescriptionPatchPlan_BatchA.md
Docs/FieldRegistryDescriptionVerification_BatchB_Input.md
Docs/FieldRegistryDescriptionVerification_BatchB.md
Docs/FieldRegistryHoverQualityScan_2026-06-03.md
```

Codex usage docs:

```text
Docs/Codex_CLI_RA2IniEditor_Usage_Guide.md
Docs/Codex_CLI_Prompt_Templates_RA2IniEditor.md
Docs/Codex_CLI_A15_2B_P_Runbook.md
```

---

## 6. Strict UI Workflow

Codex must not freely design UI.

UI tasks must follow:

```text
Inventory -> UI Contract -> User Approval -> Limited Implementation -> Screenshot/Smoke Verification -> Build/Test/Package
```

When a UI task asks for visual changes, Codex must first report:

```text
files involved
root Window/control properties
DataContext/ViewModel
open/show path
existing AutomationIds
allowed files
forbidden files
semantic boundaries
```

Do not skip approval gates.

---

## 7. Shell Boundary

Do not modify unless explicitly approved:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
main Shell layout
toolbar
menu
Project Explorer
Navigator
bottom tabs
status bar
global docking structure
```

Known exception already completed:

```text
A15-1R2 used a minimal ShellWindow.xaml.cs wiring change to pass AvalonEdit caret screen point to borderless inspectors.
```

Do not extend that exception without new approval.

---

## 8. A15-1R2 Inspector State

Target windows:

```text
RA2IniEditor.IDE/Views/Language/Ra2PeekDefinitionWindow.xaml
RA2IniEditor.IDE/Views/FieldQuickPeek/Ra2FieldQuickPeekWindow.xaml
```

Current expected behavior:

```text
borderless inspector host
opens near AvalonEdit caret
clamps to screen edge
has visible close button
Esc closes
opening/closing does not dirty document
```

Do not keep reworking these unless user reports a concrete issue.

---

## 9. Field Registry UI State

### 9.1 A15-2A completed

`Docs/FieldRegistrySurfacesUiContract.md` inventories Field Registry-related surfaces and separates phases:

```text
A15-2B: Center / Manager read-only management layout
A15-2C: Field Import Preview workflow layout
A15-2D: Field Editor / Allowed Values Editor layout
A15-2E: Field Learning Wizard workflow layout
A15-2F: Apply / Rollback confirmation consistency
```

### 9.2 A15-2B completed

A15-2B changed:

```text
FieldRegistryCenterWindow.xaml
FieldRegistryManagerWindow.xaml
FieldRegistryManagerViewModel.cs
FieldRegistryManagerViewModelTests.cs
WpfAutomationHarnessBoundaryTests.cs
```

Reported display-only ViewModel properties:

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

### 9.3 A15-2B-P / P2 completed state

Field Registry Center / Manager now have the approved visual polish and custom chrome direction:

```text
default WPF title icon / system title bar / outer chrome removed
custom lightweight tool-window header with close buttons
WindowChrome preserves move and resize behavior
Project > Global > BuiltIn is visible as a source priority strip
status chips/cards and short path display are available
read-only entry actions and write/risk actions are separated
compact empty-state text is available for warnings, cleanup preview, and rollback
Manager active packs and warnings areas use bounded internal vertical scroll hosts
Manager cleanup preview is demoted to a compact advanced cleanup section with bounded preview details
```

Field Learning Wizard screenshot also shows issues, but it is deferred to A15-2E.
Field Learning Wizard now has custom lightweight chrome from A15-2E-1 and workflow layout / bounded scroll areas from A15-2E-2. Localization and warning/disabled reason polish remains deferred to A15-2E-3.

### 9.4 AI Assistant / Right Tool Well state

AI direction is documented as a DeepSeek-powered RA2 Modding Assistant, not a Codex-like file editing agent.

AI-1B completed the first Shell-limited UI frame:

```text
existing right-side ProjectExplorerPanel is now the Right Tool Well host
ProjectExplorerColumn / ProjectExplorerGridSplitter / ProjectExplorerPanel remain in place
Section Tree / Navigator remains the default visible view
ProjectExplorerTreeView keeps its x:Name, Shell.ProjectExplorer AutomationId, ProjectExplorer.Items binding, selection handler, focus, BringIntoView, and container lookup accessibility
RightToolWell.Root / SectionTab / AiTab / ActiveView were added
AI Assistant exists only as a second skeleton view inside the same right column
AI opens only by explicit command and closes back to Section Tree
Generate / Cancel / Copy / Clear are disabled placeholders
no MockRa2AiClient generation, DeepSeek, network, API key, context provider, prompt builder, Apply/Insert, file write, Field Registry write, or whole-project context exists
AI-1B-P2 refines the skeleton into a ChatGPT-style layout:
  chat history occupies the main area
  composer is fixed at the bottom
  Advanced/model selector placeholders live inside the composer side area
  task kind selector is demoted to a disabled Advanced placeholder for AutomationId continuity
  no AI generation or provider integration exists
AI-1B-P3 further reduces WPF form feel:
  large top Close button removed
  compact header x close button preserves AiAssistant.CloseButton
  context summary is a compact muted header line
  chat history remains the main area
  composer remains the bottom anchor with prompt, Advanced/model placeholder, and disabled send nearby
AI-1B-P4 further simplifies header/composer:
  visible AI page x close button removed; return via RightToolWell.SectionTab
  composer + placeholder removed
  PromptBox wraps by available width and supports multiline input
  safety footer is outside the input border, smaller and muted
  no AI generation or provider integration exists
AI-1C adds deterministic local mock chat behavior:
  non-empty PromptBox send appends one user message and one local mock assistant response
  blank PromptBox send is a no-op
  Copy copies only the latest mock assistant response
  Clear removes local mock chat messages and restores the empty state
  Cancel remains disabled because no provider/network call exists
  AiAssistant.UserMessageList / AssistantMessageList / LatestAssistantMessage / EmptyStateMessage were added
  no DeepSeek client, network, API key, context provider, prompt builder, Apply/Insert, editor text mutation, dirty-state mutation, Field Registry write, or whole-project context exists
AI-2A completed the Context Provider / Field Registry Retrieval contract:
  Docs/AiAssistantContextProviderContract.md defines bounded context, forbidden context, local advisory retrieval strategy, diagnostics summary strategy, future model/interface anchors, tests, risks, and staged plan
  no source code was changed in AI-2A
AI-2B adds bounded current-document / caret context:
  RA2IniEditor.IDE/AI contains Ra2AiContext, Ra2AiContextRequest, IRa2AiContextProvider, and Ra2CurrentDocumentAiContextProvider
  provider reuses Ra2DocumentSnapshot, Ra2DocumentSemanticModel, Ra2CaretContextService, and Ra2CaretContext
  context resolves current file display name, caret offset, caret line, current Section, current Key / Value, explicit selected text, and bounded nearby text
  nearby text defaults to 5 lines before and after caret line and is capped by character count
  comment lines, blank lines, no document, and missing semantic context are safe fallback paths
  AiAssistant.ContextSummary updates when AI opens or Generate is clicked
  Field evidence count remains 0 and diagnostic count remains 0 because Field Registry retrieval and diagnostics summary are deferred to AI-2C / AI-2D
  Generate still uses deterministic local mock response
  no DeepSeek client, network, API key, PromptBuilder, Field Registry retrieval evidence, diagnostics summary, Apply/Insert, editor text mutation, dirty-state mutation, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
AI-2C adds local Field Registry evidence retrieval:
  RA2IniEditor.IDE/AI contains Ra2AiFieldEvidence, IRa2AiFieldEvidenceProvider, and Ra2FieldRegistryAiEvidenceProvider
  retrieval reuses FieldRegistryRuntimeService.CurrentProvider and CurrentProvenanceProvider through IRa2FieldDefinitionProvider and IFieldRegistryProvenanceProvider
  exact current key lookup uses TryGetField; prompt and explicit selected text use simple local key/display/alias/description matching
  retrieval is bounded to Top 8 by default and hard-capped at Top 12
  evidence is advisory display/reference data and does not change diagnostics, save behavior, Project > Global > BuiltIn priority, or Field Registry semantics
  AiAssistant.ContextSummary shows field evidence count and top keys when available
  diagnostics count remains 0 because diagnostics summary is deferred to AI-2D
  no DeepSeek client, network, API key, PromptBuilder, diagnostics summary, Apply/Insert, editor text mutation, dirty-state mutation, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
AI-2D adds bounded diagnostics summary integration:
  RA2IniEditor.IDE/AI contains Ra2AiDiagnosticSummary, IRa2AiDiagnosticSummaryProvider, and Ra2CurrentFileAiDiagnosticSummaryProvider
  diagnostic summaries read the current visible diagnostics snapshot passed from IssuesViewModel.Items
  diagnostics are filtered to the current file and current snapshot version where available
  priority is current caret line, current key, current Section, then a small current-file top summary
  summary results default to Top 5 and are hard-capped at Top 8
  AiAssistant.ContextSummary now shows dynamic diagnostic count while preserving field evidence count/top keys
  summaries are advisory display/reference data only
  no diagnostics rerun, diagnostics rule change, Issues mutation, Save Preflight change, diagnostic auto-fix, editor text mutation, dirty-state mutation, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
  no DeepSeek client, network, API key, PromptBuilder, Apply, or Insert behavior exists
AI-3A defines the Prompt Builder contract:
  supported internal intents are Auto, ExplainField, FindFieldsByRequirement, GenerateUnitPrototype, GenerateWeaponChainDraft, ReviewIniSnippet, and ExplainDiagnostics
  prompt sections are Application Rules, User Request, Current IDE Context, Field Registry Evidence, Diagnostics Summary, and Output Requirements
  Field Registry evidence and diagnostics are advisory only
  INI/project text is untrusted data, not instructions
AI-3B adds deterministic PromptBuilder implementation:
  RA2IniEditor.IDE/AI contains Ra2AiIntent, Ra2AiPromptBuildRequest, Ra2AiRequest, IRa2AiPromptBuilder, and Ra2AiPromptBuilder
  PromptBuilder consumes only UserPrompt, Intent, and the already-built Ra2AiContext
  PromptBuilder includes bounded context, field evidence, and diagnostics already present on Ra2AiContext
  generated prompt marks INI output as draft and forbids claiming files were modified, saved, applied, inserted, or fixed
  PromptBuilder does not read files, inspect editor controls, query providers, rerun diagnostics, reload Field Registry, call network, or collect additional context
  no Shell wiring was needed and UI behavior remains unchanged
  no DeepSeek client, network, API key, Apply/Insert, file modification, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
AI-4B adds AI client abstraction / deterministic fake boundary:
  RA2IniEditor.IDE/AI contains IRa2AiClient, Ra2AiResponseKind, Ra2AiResponse, and FakeRa2AiClient
  IRa2AiClient.SendAsync consumes the AI-3B Ra2AiRequest and requires CancellationToken
  Ra2AiResponse supports Success, Cancelled, ProviderError, and MissingConfiguration states
  FakeRa2AiClient is deterministic and returns success, cancelled, provider error, or missing configuration responses for tests
  FakeRa2AiClient does not read files, editor controls, Field Registry providers, diagnostics services, environment variables, API keys, or network
  no Shell wiring was needed and AI panel behavior remains the AI-1C local mock chat flow
  no DeepSeek client, network, API key, Apply/Insert, file modification, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
AI-4C wires the AI panel send flow to the internal fake AI pipeline:
  RA2IniEditor.IDE/AI contains Ra2AiAssistantPipeline as a small composition helper
  PromptBox non-empty send appends a user message, builds bounded context, builds a Ra2AiRequest with Ra2AiPromptBuilder, calls IRa2AiClient/FakeRa2AiClient, and appends the fake assistant response
  empty PromptBox remains a no-op
  ProviderError and MissingConfiguration responses are displayed as error-style assistant messages
  Cancelled responses clear sending state and can display a compact cancellation message
  ShellWindow.xaml.cs performs only minimal AI panel wiring and does not manually build prompts, directly query Field Registry, rerun diagnostics, perform file IO, read environment variables, or contain DeepSeek-specific code
  AI panel layout, Section Tree default behavior, Apply/Insert absence, and model selector placeholder behavior remain unchanged
  no DeepSeek client, network, API key, file modification, editor text mutation, dirty-state mutation, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
AI-4D adds a DeepSeek-compatible AI client adapter behind IRa2AiClient:
  RA2IniEditor.IDE/AI contains DeepSeekRa2AiClient and DeepSeekRa2AiClientOptions
  DeepSeekRa2AiClient consumes only Ra2AiRequest and sends only Ra2AiRequest.PromptText as user content in a minimal OpenAI-compatible chat completion request shape
  adapter uses configured BaseUrl, ApiKey, Model, Timeout, and conservative Temperature from options
  adapter is tested with injected HttpClient / fake HttpMessageHandler and normal tests do not require live DeepSeek, real network, or real credentials
  Missing API key / invalid options map to MissingConfiguration
  HTTP non-success, malformed JSON, missing assistant content, HttpRequestException, and timeout map to ProviderError
  pre-cancelled and operation-cancelled requests map to Cancelled
  errors do not expose API key, raw prompt, raw response body, Authorization header, full context, selected text, nearby text, absolute paths, or environment variables
  AI panel still uses FakeRa2AiClient by default; no provider selector, live send-flow switch, API key UI, settings persistence, Apply/Insert, file mutation, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
AI-4D-Fix makes the DeepSeek API key policy environment-variable only:
  RA2IniEditor.IDE/AI contains DeepSeekRa2AiClientFactory
  production/live options creation reads only DEEPSEEK_API_KEY, DEEPSEEK_BASE_URL, DEEPSEEK_MODEL, and DEEPSEEK_TIMEOUT_SECONDS
  DEEPSEEK_API_KEY is the only allowed API key source for the first implementation
  DEEPSEEK_BASE_URL, DEEPSEEK_MODEL, and DEEPSEEK_TIMEOUT_SECONDS are optional overrides
  default BaseUrl is https://api.deepseek.com, default Model is deepseek-v4-pro, default Timeout is 60 seconds, and default Temperature remains 0.2
  DeepSeekRa2AiClientOptions.ApiKey remains available for direct tests and explicit construction, but no API key is collected from Advanced UI or persisted settings
  environment-variable tests isolate and restore process environment values and do not depend on the user's real environment
  no Shell UI, provider selector, live send-flow switch, API key UI, local settings persistence, Apply/Insert, file mutation, Field Registry write, whole-project context, auto-open, or auto-send behavior exists
AI-4E-1 adds provider mode UI state in the AI Assistant Advanced area:
  AiAssistant.ProviderSelector shows Mock as the default provider
  DeepSeek is visible as a disabled future option
  AiAssistant.ProviderStatus states that this phase does not read API keys or send real requests
  AiAssistant.DeepSeekEnvironmentHint states that future API key configuration uses DEEPSEEK_API_KEY
  AiAssistant.AdvancedButton, AdvancedOptions, ModelSelector, GenerateButton, PromptBox, SafetyFooter, ChatHistory, and ContextSummary are preserved
  Generate / Send still uses the AI-4C FakeRa2AiClient pipeline
  no live DeepSeek send flow, network call, API key loading, API key input, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-open, auto-send context, diagnostic auto-fix, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1368 and package count 725
AI-4E-2 adds minimal provider selection / DeepSeek live send flow:
  AiAssistant.ProviderSelector can choose Mock or DeepSeek
  Mock remains the default provider and continues to use FakeRa2AiClient
  DeepSeek is used only after explicit user selection and explicit Generate / Send
  DeepSeek client construction goes through DeepSeekRa2AiClientFactory.CreateClientFromEnvironment()
  DeepSeek options are still created from environment variables only through DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment()
  ShellWindow.xaml.cs does not directly read environment variables and does not construct HttpClient directly
  MissingConfiguration, ProviderError, Cancelled, timeout-like provider errors, and unhandled errors are mapped to sanitized chat messages
  DeepSeek success output is appended as advisory assistant text only
  busy state still blocks duplicate sends and Cancel cancels the active request / clears busy state
  no API key input UI, save API key button, settings persistence, project/local API key storage, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-open, auto-send context, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1370 and package count 726
AI-4E-2-P fixes the AI Assistant Advanced provider layout:
  provider selection no longer uses a ComboBox popup
  AiAssistant.ProviderSelector is now a compact Mock / DeepSeek RadioButton group
  AiAssistant.ProviderMockOption and AiAssistant.ProviderDeepSeekOption were added
  Mock remains the default provider and DeepSeek still requires explicit user selection
  AiAssistant.ModelSelector is preserved as compact read-only model text
  AiAssistant.TaskKindSelector is preserved as compact read-only intent text
  AiAssistant.ProviderStatus and AiAssistant.DeepSeekEnvironmentHint keep wrapping text
  provider behavior from AI-4E-2 is unchanged
  no API key input UI, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-open, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1370 and package count 728
AI-4E-2-P2 adds composer Enter-to-send:
  AiAssistant.PromptBox handles PreviewKeyDown through AiAssistantPromptBox_OnPreviewKeyDown
  Enter without Shift calls the existing GenerateAiAssistantResponse send path
  Shift+Enter remains normal multiline input behavior
  empty prompts and busy duplicate sends remain guarded by the existing send method
  the key handler does not build context, build prompts, select providers, or call clients directly
  Mock / DeepSeek provider behavior from AI-4E-2 is unchanged
  no Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context beyond explicit Enter/Send action, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1370 and package count 728
AI-4E-2-P3R restores a minimal model ComboBox in Advanced:
  Advanced now permanently shows only one compact AiAssistant.ModelSelector ComboBox
  the model options are Mock and DeepSeek
  Mock is selected by default
  the selected model maps to the existing provider mode used by AI-4E-2
  permanent Provider, Status, Intent, API Key, BaseUrl, and Timeout rows are removed from Advanced
  DeepSeek missing configuration remains a chat-history error after the user selects DeepSeek and sends
  no API key input UI, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context beyond explicit Enter/Send action, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1370 and package count 729
AI-4E-2-P4 cleans up AI chat action placement:
  Advanced still permanently shows only the compact AiAssistant.ModelSelector ComboBox
  Cancel / Copy / Clear are no longer visible inside Advanced
  AiAssistant.ClearButton moved to AiAssistant.ChatHistoryActions above the chat history
  AiAssistant.CancelButton moved to the composer send area
  global latest-response copy UI and CopyLatestAiAssistantResponse were removed
  every assistant message card generated by AddAiAssistantMessage includes an AiAssistant.AssistantMessageCopyButton
  each assistant-message copy button copies only that specific assistant message text
  user messages do not get copy buttons
  Mock / DeepSeek provider behavior and Enter-to-send behavior are unchanged
  no API key input UI, save API key button, settings persistence, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context beyond explicit Enter/Send action, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1370 and package count 731
AI-4E-2 DeepSeek endpoint hotfix:
  DeepSeekRa2AiClientOptions normalizes base URL values to the /chat/completions endpoint before DeepSeekRa2AiClient sends requests
  missing DEEPSEEK_BASE_URL still uses the environment-only default base URL, but the actual request URI is https://api.deepseek.com/chat/completions
  explicit full chat completions endpoints remain unchanged and are not appended twice
  DeepSeekRa2AiClient continues to parse assistant text from choices[0].message.content
  tests cover default base URL endpoint normalization, full endpoint preservation, and choices[0].message.content parsing
  malformed JSON and missing assistant content still map to ProviderError
  API key policy remains DEEPSEEK_API_KEY environment-variable only
  no API key UI, settings persistence, provider selector semantic change, send-flow semantic change, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming output, retry loop, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  Release full test passed; latest test count 1373. Debug build/test was blocked by a running RA2IniEditor.IDE process locking Debug output DLLs
AI-5A defines the stable draft output contract:
  generated INI drafts must not randomly choose faction / Owner when unspecified
  clean INI blocks do not contain explanatory comments by default
  explanations, field rationale, assumptions, warnings, and uncertainty stay outside code blocks
  rulesmd.ini and artmd.ini draft sections are separated
  new referenced IDs are listed as follow-up TODO definitions
  fields without Field Registry evidence are omitted from clean draft or placed under optional / verify-before-use
  no source code, UI, provider, send-flow, API key, Apply/Insert, file mutation, or Field Registry behavior changed in AI-5A
AI-5B updates Ra2AiPromptBuilder stable draft rules:
  Ra2AiPromptBuilder now appends Stable INI Draft Rules to the deterministic prompt
  draft/prototype output is explicitly draft/advisory only and must not claim Apply / Insert / Save / file write happened
  missing faction / side / country / Owner requires TODO placeholders such as Owner=<TODO_OWNER>, not random Allied / Soviet / Yuri selection
  clean copyable INI blocks are instructed to omit explanatory comments by default
  explanations, field rationale, assumptions, risks, warnings, and uncertainty are instructed to stay outside code blocks
  rulesmd.ini and artmd.ini draft blocks are separated when both are relevant
  newly referenced weapon / warhead / projectile / art / voxel / SHP / cameo / sound / animation / prerequisite IDs must be listed under "需要补充的定义"
  no-hallucinated-field guidance requires clean drafts to use only field keys confirmed by Field Registry Evidence
  unconfirmed field keys stay out of clean drafts by default and belong under "可选 / 使用前需验证" if useful
  the prompt distinguishes field keys from object IDs / values and allows new object IDs as values only when listed as follow-up definitions
  PromptBuilder still consumes only UserPrompt, Intent, and the already-built Ra2AiContext
  no DeepSeek adapter, provider selection, API key loading, AI panel UI, Apply/Insert, editor text mutation, dirty-state mutation, file write, Field Registry write, whole-project context, parser change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1377 and package count 735
AI-5C adds Markdown response rendering / code block copy:
  assistant response text remains the source of truth for full-message copy
  RA2IniEditor.IDE/AI contains Ra2AiMarkdownBlock and Ra2AiMarkdownResponseParser
  the parser performs deterministic fenced-code-block splitting only and does not execute or interpret code
  supported fences include language labels such as ini, rules, art, and unlabeled fences
  multiple code blocks are detected and unterminated fences fall back safely to plain text
  assistant messages keep AiAssistant.AssistantMessageCopyButton for copying the full assistant response text
  detected code blocks render as compact code cards with AiAssistant.CodeBlock, AiAssistant.CodeBlockLanguage, and AiAssistant.CodeBlockCopyButton
  copy-code actions copy only the code content inside the fence and exclude Markdown fence markers
  user messages remain plain chat messages and do not get code-block copy actions
  copy actions do not modify editor text, mark dirty, apply, insert, save, write files, or write Field Registry data
  no DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Markdown-to-file conversion, automatic code insertion, draft validation, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics, diagnostics behavior, completion/hover/quick peek, save preflight, BuiltIn JSON, legacy restore, or solution/project file change was added
  no AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added
  Debug restore passed; Debug build was blocked by a running RA2IniEditor.IDE process locking Debug output DLLs. Release build passed with 0 warnings / 0 errors, Release tests passed with 1383 tests, and IdeOnly package passed with package count 739
AI-5C-P adds lightweight Markdown rich rendering:
  assistant response text remains the source of truth for full-message copy
  RA2IniEditor.IDE/AI now also contains Ra2AiMarkdownBlockKind
  Ra2AiMarkdownBlock carries block kind and heading level in addition to language/code text
  Ra2AiMarkdownResponseParser parses # / ## / ### headings, paragraphs, unordered list items, ordered list items, and fenced code blocks
  unsupported Markdown falls back to paragraph text
  Markdown inside fenced code blocks is not parsed as headings or lists
  existing code card rendering and copy-code behavior are preserved
  assistant-message copy still copies the original full Markdown response
  rendered heading / paragraph / list elements expose AiAssistant.MarkdownHeading, AiAssistant.MarkdownParagraph, and AiAssistant.MarkdownListItem
  no WebView, NuGet package, project file dependency, Apply/Insert, automatic file modification, Field Registry write, Markdown-to-file conversion, automatic code insertion, draft validation, DeepSeek adapter behavior change, provider switching behavior change, API key UI/loading, settings persistence, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, PromptBuilder behavior change, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  no AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added
  restore/build/test/package passed; latest test count 1389 and package count 741
AI-5C-P2 fixes Markdown table and inline bold rendering:
  common GitHub-style pipe tables are detected as header row + separator row + optional body rows
  table cells are split by pipe delimiters, trimmed, and rendered as lightweight WPF Grid table cards
  malformed tables fall back safely to paragraph text
  tables inside fenced code blocks remain code content and are not parsed as tables
  rendered tables expose AiAssistant.MarkdownTable, AiAssistant.MarkdownTableHeader, AiAssistant.MarkdownTableRow, and AiAssistant.MarkdownTableCell
  simple non-nested **bold** spans render as WPF inline Runs with FontWeights.Bold in headings, paragraphs, lists, and table cells
  malformed or unsupported inline Markdown remains safe plain text
  existing heading / paragraph / bullet / numbered / fenced code block rendering and copy behavior is preserved
  assistant-message copy still copies the original full Markdown response
  code-block copy still copies only fence contents without Markdown fence markers
  no WebView, Markdig, NuGet package, project file dependency, Apply/Insert, automatic file modification, Field Registry write, Markdown-to-file conversion, automatic code insertion, draft validation, DeepSeek adapter behavior change, provider switching behavior change, PromptBuilder behavior change, API key UI/loading, settings persistence, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  no AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added
  Debug restore passed; Debug build was blocked by a running RA2IniEditor.IDE process locking Debug output DLLs. Release build passed with 0 warnings / 0 errors, Release tests passed with 1393 tests, and IdeOnly package passed with package count 742
AI-5C-P3 fixes single-backtick inline code rendering:
  simple inline code spans such as `TODO_OWNER`, `Owner=<TODO_OWNER>`, `Image=LAAV`, and `Voxel` render without visible backticks
  inline code uses a lightweight monospace WPF inline style through the existing assistant inline text helper
  the inline helper is used by headings, paragraphs, list items, and table cells
  unterminated inline backticks fall back safely to raw text
  fenced code blocks remain unaffected because they render through the existing code-card path
  assistant-message copy still copies the original full Markdown response, including backticks
  code-block copy still copies only fence contents without Markdown fence markers
  no provider behavior, DeepSeek adapter behavior, PromptBuilder behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, Markdown-to-file conversion, automatic code insertion, draft validation, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  no AiAssistant.ApplyButton, AiAssistant.InsertButton, AiAssistant.ApiKeyTextBox, or AiAssistant.SaveApiKeyButton was added
  restore/build/test/package passed; latest test count 1393 and package count 743
AI-5B-Fix2 adds bounded draft evidence profiles:
  RA2IniEditor.IDE/AI/Ra2FieldRegistryAiEvidenceProvider.cs now selects deterministic draft evidence profiles from prompt keywords
  core profiles include UnitCore, VehicleCore, InfantryCore, BuildingCore, WeaponCore, ProjectileCore, and WarheadCore
  mechanism profiles include AntiAirWeapon, GroundAttackWeapon, DeployTransform, Transport, StealthScout, Sensor, SelfRepair, GarrisonPassenger, BuildLimitTechPrerequisite, Veterancy, ArtVoxel, and ArtShp
  profile seed keys are only returned when the active Field Registry provider confirms the key for the active section kind
  unavailable profile seed keys are skipped and are not fabricated
  exact current-key evidence and direct prompt key matches keep priority over profile hints
  existing Top N evidence bounding remains in effect
  Ra2AiPromptBuilder was not changed, so no-hallucinated-fields stable draft rules remain unchanged
  tests cover vehicle/core draft selection, anti-air, deploy transform, transport, stealth scout, provider-confirmed filtering, bounding, priority, and non-draft prompts
  no DeepSeek adapter behavior, provider switching behavior, UI behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1402 and package count 744
AI-6A defines Conversation Context / Current Subject contract:
  Docs/AiAssistantConversationContextContract.md separates Current IDE Context, Conversation Context, and Current Subject
  prior assistant INI drafts are defined as conversation draft text only, not applied project file state
  future SubjectKind / SubjectId / Source / Summary / Confidence anchors are defined
  bounded chat history, draft memory, PromptBuilder integration, UI summary, privacy/safety, tests, and AI-6B through AI-6E implementation split are defined
  no source code, XAML, code-behind, ViewModel, tests, scripts, project files, Field Registry JSON, legacy, PromptBuilder source, send-flow, DeepSeek adapter, provider selection, Apply/Insert, editor text mutation, dirty-state mutation, file write, or Field Registry write behavior was changed
  doc-phase test/package passed; latest test count 1402 and package count 746
AI-6B adds bounded conversation context extraction:
  RA2IniEditor.IDE/AI now contains Ra2AiConversationTurn, Ra2AiConversationContext, Ra2AiConversationContextRequest, IRa2AiConversationContextProvider, and Ra2AiConversationContextProvider
  Ra2AiConversationTurn stores User / Assistant role, visible text, and IsDraftResponse
  Ra2AiConversationContext stores bounded turns, TotalCharacterCount, and WasTruncated
  Ra2AiConversationContextRequest provides LastTurns, MaxCharacters, and MaxSingleTurnCharacters bounds with defaults 6 / 6000 / 2000
  Ra2AiConversationContextProvider extracts only caller-supplied current-session visible conversation turns
  extraction keeps newest turns, enforces total and per-turn character bounds, safely truncates long messages, and sets WasTruncated when any bound cuts content
  assistant turns are marked as draft/advisory responses and not applied file state
  sensitive guardrails redact API-key-like tokens, Authorization headers, DeepSeek environment markers, provider metadata, raw request payload markers, and raw response payload markers from extracted context
  empty chat returns an empty context
  tests cover recent user/assistant turns, last N turns, total bounds, oversized assistant truncation, assistant draft marking, hidden provider metadata redaction, API-key-like redaction, Authorization/raw payload redaction, empty chat, source message immutability, and no editor text/dirty-state mutation
  no PromptBuilder integration, current subject extraction, draft section ID extraction, Shell UI change, DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1413 and package count 753
AI-6C adds current subject / draft subject extraction:
  RA2IniEditor.IDE/AI now contains Ra2AiCurrentSubject, Ra2AiSubjectKind, Ra2AiSubjectSource, IRa2AiCurrentSubjectExtractor, and Ra2AiCurrentSubjectExtractor
  Ra2AiSubjectKind supports Unknown, Unit, Weapon, Warhead, Projectile, Art, and Section
  Ra2AiSubjectSource supports Unknown, CurrentCaretSection, LastAssistantDraft, and UserMention
  Ra2AiCurrentSubject stores Kind, SubjectId, Source, Summary, Confidence, and IsDraft
  Ra2AiCurrentSubjectExtractor consumes only Ra2AiConversationContext and reads only current-session visible assistant draft/user turn text supplied in that context
  the extractor parses visible INI-style section blocks such as [LAAV], [LAAVMissile], [LAAVMissileWH], and [LAAVMissileP]
  subject kind inference is conservative and uses local field-key heuristics only: unit keys such as Strength / Armor / Primary, weapon keys such as Damage / ROF / Warhead, warhead keys such as Verses / CellSpread, projectile keys such as AA / AG, and art keys/context such as artmd.ini / Voxel / Cameo
  main unit sections from the last assistant draft are prioritized over weapon / warhead / projectile follow-up definitions for unit prototype drafts
  recent user-mentioned section IDs can influence selection only when no main unit candidate is present
  malformed or unknown drafts return Unknown safely
  extracted subjects from assistant drafts are marked LastAssistantDraft and draft/advisory, and summaries state they come from conversation draft text rather than confirmed project file state
  tests cover Unit, Weapon, Warhead, Projectile, main-unit priority, user-mentioned section selection without a main unit, malformed fallback, draft/advisory marking, no project-file-state claim, and pure extraction without files/providers/diagnostics/environment access
  no PromptBuilder integration, Conversation Context prompt section, Current Subject UI display, Shell UI change, send-flow change, DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, Field Registry write, whole-project context, auto-send context, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1422 and package count 760
AI-6D integrates conversation context and current subject into PromptBuilder:
  Ra2AiPromptBuildRequest now accepts optional ConversationContext and CurrentSubject
  Ra2AiPromptBuilder emits separate Current Subject and Conversation Context sections before Current IDE Context, Field Registry Evidence, and Diagnostics Summary
  Current Subject includes SubjectKind, SubjectId, Source, IsDraft, Confidence, and Summary when available
  Current Subject wording states LastAssistantDraft is prior assistant draft text only and must not be assumed to exist in rulesmd.ini / artmd.ini unless the user explicitly says it was applied/pasted/saved and Current IDE Context supports that
  Conversation Context wording states it is recent visible current-session AI Assistant chat, bounded and possibly truncated, not hidden memory / provider metadata / raw payload, and assistant messages are draft/advisory rather than applied file state
  Ra2AiAssistantPipeline forwards optional ConversationContext and CurrentSubject to PromptBuilder while preserving the existing provider client call
  ShellWindow.xaml.cs stores only visible chat message role/text metadata and sends it through Ra2AiConversationContextProvider and Ra2AiCurrentSubjectExtractor; it does not manually build conversation prompt text or parse draft section IDs in UI code
  tests cover PromptBuilder section presence, ordering, prior-draft-not-applied wording, SubjectId/SubjectKind, follow-up reference wording, bounded/current-session conversation wording, sensitive metadata redaction through the conversation context provider, and pipeline context/subject forwarding
  no XAML, Current Subject UI display, DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, editor dirty-state mutation, Field Registry write, whole-project context, unbounded chat history, hidden/cross-session memory, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1427 and package count 761
AI-6E polishes AI Assistant Context Summary UI:
  AiAssistant.ContextSummary remains in the AI Assistant header and now wraps instead of truncating
  AiAssistant.CurrentSubjectSummary and AiAssistant.ConversationContextSummary were added as compact read-only lines below the current IDE context summary
  current IDE context still shows current file, Section, key/value, line, nearby-line count, Field Registry evidence count/top keys, diagnostics count, and selected-text state
  current subject display shows "当前主题：无" when unknown
  current subject display shows IDs such as LAAV / Unit and marks LastAssistantDraft as "上一轮 AI 草稿，仅作草稿/建议，未写入项目文件"
  conversation context display shows only bounded metadata: recent visible turn count and truncated / not-truncated state
  summary refreshes when AI Assistant opens, Generate/Send is clicked, assistant/cancel/error response is appended, and chat is cleared
  tests cover stable ContextSummary automation ids, CurrentSubject/ConversationContext summary ids, draft/advisory wording, no project-file-state claim, bounded turn/truncation summary, no hidden provider metadata exposure in the panel, no Apply/Insert/API-key controls, and unchanged send wiring boundaries
  no PromptBuilder rule change, DeepSeek adapter behavior, provider switching behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, editor dirty-state mutation, Field Registry write, whole-project context, unbounded chat history, hidden/cross-session memory, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1427 and package count 762
AI-6F expands subject-aware Field Registry evidence retrieval:
  Ra2AiContextRequest carries optional ConversationContext and CurrentSubject into Ra2CurrentDocumentAiContextProvider
  IRa2AiFieldEvidenceProvider and Ra2FieldRegistryAiEvidenceProvider accept bounded conversation/current-subject inputs
  previous assistant draft field keys are extracted from current-session assistant draft text with the deterministic INI key parser and returned only when the active provider confirms them
  CurrentSubject.Kind=Unit adds UnitCore and VehicleCore profile candidates; Weapon / Projectile / Warhead / Art subject kinds add conservative core profiles
  follow-up intent profiles cover faction/owner, anti-air weapon, deploy/transform, transport, and stealth/scout seed keys
  faction/owner prompts can retrieve Owner, RequiredHouses, ForbiddenHouses, Prerequisite, UIName, Name, and Image when provider-confirmed
  anti-air prompts can retrieve Primary, Secondary, Damage, ROF, Range, Projectile, Warhead, AA, AG, and Verses when provider-confirmed
  seed keys that the active Field Registry provider does not confirm are skipped and are not fabricated
  evidence defaults to Top 16 and remains hard-capped at 24; exact current key remains highest priority
  ShellWindow.xaml.cs only passes already-built bounded conversation context/current subject into context retrieval; it does not implement evidence logic or parse draft section IDs
  no PromptBuilder no-hallucinated-fields rule change, DeepSeek adapter behavior, provider selection behavior, API key UI/loading, settings persistence, Apply/Insert, automatic file modification, editor dirty-state mutation, Field Registry write, whole-project context, unbounded chat history, hidden/cross-session memory, diagnostic auto-fix, streaming, retry loops, parser semantics change, diagnostics behavior change, completion/hover/quick peek change, save preflight change, BuiltIn JSON change, legacy restore, or solution/project file change was added
  restore/build/test/package passed; latest test count 1432 and package count 763
```

---

## 10. Current Task Boundaries

### 10.1 A15-2B-P visual polish completed

Completed target surfaces:

```text
Field Registry Center
Field Registry Manager / Advanced Tools
```

Modified / allowed files:

```text
FieldRegistryCenterWindow.xaml
FieldRegistryCenterWindow.xaml.cs
FieldRegistryManagerWindow.xaml
FieldRegistryManagerWindow.xaml.cs
FieldRegistryManagerViewModel.cs
FieldRegistryPackStatusViewModel.cs
WpfAutomationHarnessBoundaryTests.cs
FieldRegistryManagerViewModelTests.cs
```

Completed goals:

```text
improve visual hierarchy
reduce WPF form feeling
use status chips/cards
shorten path display with tooltip
group search + field count
separate read-only actions from write/risk actions
compact empty states
localize visible English UI text in these two windows where safe
preserve existing handlers, AutomationIds, and Field Registry semantics
```

### 10.2 A15-2B-P2 custom chrome completed

Field Registry windows now align with the borderless inspector direction while remaining large management windows:

```text
remove default WPF icon
remove default system title bar
remove normal outer system border
use custom lightweight tool-window chrome
preserve move and preferably resize behavior
```

These are large management windows, not small inspector popups. Do not use tiny `SizeToContent` behavior.

Implemented strategy:

```text
WindowStyle=None
custom header
custom close button
preserve resize with ResizeMode=CanResize and WindowChrome
```

### 10.3 A15-2B-P3 Manager scroll / localization completed

Completed target surface:

```text
Field Registry Manager / Advanced Tools
```

Modified / allowed files:

```text
FieldRegistryManagerWindow.xaml
FieldRegistryManagerViewModel.cs
WpfAutomationHarnessBoundaryTests.cs
FieldRegistryManagerViewModelTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Completed goals:

```text
current active registry area has a bounded vertical scroll host
warnings area has a bounded vertical scroll host
large fixed blank row sizing was reduced
Manager-only mixed English/Chinese visible text was reduced where safe
existing buttons, commands, bindings, AutomationIds, and Field Registry semantics were preserved
```

### 10.3.1 A15-2B-P4 Cleanup preview demotion completed

Completed target surface:

```text
Field Registry Manager / Advanced Tools
```

Modified / allowed files:

```text
FieldRegistryManagerWindow.xaml
FieldRegistryManagerViewModel.cs
WpfAutomationHarnessBoundaryTests.cs
FieldRegistryRollbackUiBoundaryTests.cs
FieldRegistryManagerViewModelTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Completed goals:

```text
Cleanup preview was not deleted.
BuildCleanupPlan and ApplyCleanupPlan commands remain wired to their existing handlers.
Cleanup preview no longer reserves a large empty MinHeight area by default.
Cleanup preview is presented as a compact advanced cleanup section with an Expander.
Cleanup details are shown inside a bounded MaxHeight scroll host.
Apply cleanup remains visually grouped as a write/risk action.
Cleanup plan/apply semantics, backup/confirmation behavior, rollback behavior, and Field Registry priority remain unchanged.
```

### 10.4 A15-2D Field Editor / Allowed Values custom chrome completed

Completed target surfaces:

```text
Field Editor / New Field Editor
Allowed Values Editor
```

Modified / allowed files:

```text
FieldEditorWindow.xaml
FieldEditorWindow.xaml.cs
AllowedValuesEditorWindow.xaml
AllowedValuesEditorWindow.xaml.cs
Ra2FieldEditorWindowBoundaryTests.cs
WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Completed goals:

```text
Field Editor no longer uses the default WPF title icon/system title bar
Allowed Values Editor no longer uses the default WPF title icon/system title bar
both windows use lightweight custom chrome with close buttons
WindowChrome preserves resize behavior
existing FieldEditor and AllowedValuesEditor AutomationIds are preserved
Field Editor save preview/apply behavior is unchanged
Allowed Values ShowDialog / DialogResult / ResultText behavior is unchanged
Field Learning Wizard and Shell are unchanged
```

### 10.5 A15-2E-1 Field Learning Wizard custom chrome completed

Completed target surface:

```text
Field Learning Wizard
```

Modified / allowed files:

```text
FieldLearningWizardWindow.xaml
FieldLearningWizardWindow.xaml.cs
Ra2FieldLearningWizardBoundaryTests.cs
WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Completed goals:

```text
Field Learning Wizard no longer uses the default WPF title icon/system title bar.
The window uses lightweight custom chrome with FieldLearningWizard.CustomChrome, FieldLearningWizard.ChromeTitle, and FieldLearningWizard.CloseButton.
WindowChrome preserves resize behavior with ResizeMode=CanResize.
Existing workflow layout, tabs, grids, source text area, commands, bindings, and FieldLearningWizard AutomationIds are preserved.
UseCurrentIni / ParsePastedText / BuildApplyPlan / Apply confirmation behavior is unchanged.
Field Registry services, parser, validation, BuiltIn JSON, Shell, Field Editor, Allowed Values Editor, Center, and Manager are unchanged.
```

### 10.6 A15-2E-2 Field Learning Wizard workflow layout completed

Completed target surface:

```text
Field Learning Wizard
```

Modified / allowed files:

```text
FieldLearningWizardWindow.xaml
Ra2FieldLearningWizardBoundaryTests.cs
WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Completed goals:

```text
WorkflowStepStrip shows the display-only flow: source, parse, target/mode, preview, apply plan, apply.
Source raw text is bounded by SourceScrollHost.
Target / mode summary is grouped under TargetModeSummary.
MainTabs and all existing review grids remain but are bounded by ReviewScrollHost.
BuildApplyPlan and Apply are visually separated into ApplyBoundaryPanel instead of the header action cluster.
Existing handlers, bindings, commands, AutomationIds, WindowStyle=None, and WindowChrome are preserved.
UseCurrentIni / ParsePastedText / BuildApplyPlan / Apply confirmation behavior is unchanged.
Shell, Field Editor, Allowed Values Editor, Center, Manager, services, parser, validation, and BuiltIn JSON are unchanged.
```

---

## 11. Forbidden Changes For Current Field Registry Work

Do not modify:

```text
Field Registry loader/writer/apply/rollback services
Harvest parser / normalize / validate / preview semantics
Project > Global > BuiltIn priority
cleanup plan semantics
apply cleanup behavior
rollback behavior
backup manifest behavior
field learning behavior
import preview behavior
completion / hover / diagnostics / quick peek / save preflight
BuiltIn field registry JSON
Shell main layout
legacy files
```

---

## 11.1 Icon-0C Main Toolbar State Cleanup completed

Completed target surface:

```text
Main Shell toolbar state / AutomationId hygiene
```

Modified / allowed files:

```text
ShellWindow.xaml.cs
IdeShellBoundaryTests.cs
FieldImportApplySmokeTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Completed goals:

```text
Open Project Folder, Search, Issues, and Field Registry remain accessible from the main toolbar with their approved Shell.MainToolbar.* AutomationIds.
Save keeps the current actual Shell.SourceEditor.SaveCurrentFileButton AutomationId and is enabled only when the current editable session is dirty.
Undo / Redo keep their current Shell.SourceEditor.* AutomationIds and are enabled only when an editable session exists and AvalonEdit reports available undo/redo state.
Revert keeps Shell.SourceEditor.RevertInMemoryChangesButton and is enabled only when the current editable session is dirty.
Enter Edit Mode remains collapsed.
Project Explorer remains present and functional.
The old legacy Shell.FieldRegistryButton AutomationId was not restored; FieldImportApplySmokeTests now target Shell.MainToolbar.FieldRegistryButton.
No placeholder Icon* resources were changed and no SVG / PNG / DrawingImage resources were added.
No command handlers, menu entries, command semantics, Field Registry services, diagnostics, parser, completion, hover, quick peek, save preflight, BuiltIn JSON, AI behavior, legacy files, solution files, or project files were changed.
```

---

## 11.2 FR-DQ-2F-AI-LowQuality-ManualApply completed

Completed target surface:

```text
BuiltIn v3.2 Field Registry Hover quality for direct AI low-quality rows
```

Modified files:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs
Docs/FieldRegistryDescriptionVerification_AiLowQuality_2026-06-03.md
Docs/FieldRegistryHoverQualityScan_2026-06-03.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
AGENTS.md
```

Completed goals:

```text
The remaining direct BuiltIn rows whose description was exactly `数值型字段` were removed.
14 [AI] base-composition Ratio / Limit rows were source-verified against ModEnc and changed to source-backed descriptions with Float / Integer editor kinds.
14 existing Global rough rows for those AI Ratio / Limit keys were changed to explicit non-canonical guardrails because sources place those flags in [AI].
14 Techno placeholder rows for those AI Ratio / Limit keys were changed to explicit non-canonical guardrails.
5 Dumb*Coefficient threat-system fields were added as [General] / Global Float rows.
Existing [AI] and Techno Dumb*Coefficient rows were changed to explicit non-canonical guardrails.
Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed.
```

Validation result in patch environment:

```text
JSON parse: passed
BuiltIn v3.2 field count: 4643
Exact `数值型字段` rows: 0
Direct target row validation: passed
dotnet test: not run because dotnet CLI is unavailable in the patch environment
```

---

## 11.3 FR-DQ-2F-AI-CrossContext-ManualApply completed

Completed target surface:

```text
BuiltIn v3.2 Field Registry Hover quality for the next AI cross-context placeholder batch
```

Modified files:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs
Docs/FieldRegistryDescriptionVerification_AiCrossContext_2026-06-03.md
Docs/FieldRegistryHoverQualityScan_2026-06-03.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
AGENTS.md
```

Completed goals:

```text
Owner / AI, Prerequisite / AI, and Sight / AI were changed from inherited placeholder rows into explicit non-canonical guardrails because sources place those fields on TechnoTypes, not the [AI] section.
AirstripRatio / AI and AirstripLimit / AI were source-verified as [AI] old base-composition controls and updated to Float / Integer source-backed descriptions with obsolete / parsed no-op caveats.
AirstripRatio / Global, AirstripLimit / Global, AirstripRatio / Techno, and AirstripLimit / Techno were changed from placeholder rows into explicit non-canonical guardrails because sources place those flags in [AI].
Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed.
```

Validation result in patch environment:

```text
JSON parse: passed
BuiltIn v3.2 field count: 4643
Placeholder rows after patch: 2452
Exact `数值型字段` rows: 0
Target row validation: passed
dotnet test: not run because dotnet CLI is unavailable in the patch environment
```

---

## 11.4 FR-DQ-2G-AI-Page-Batch-ManualApply completed
FR-DQ-2H-TechnoTypes-Common-ManualApply completed

Completed target surface:

```text
BuiltIn v3.2 Field Registry Hover quality for ModEnc [AI] page source-batch rows
```

Modified files:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs
Docs/FieldRegistryDescriptionVerification_AiPageBatch_2026-06-03.md
Docs/FieldRegistryHoverQualityScan_2026-06-03.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
AGENTS.md
```

Completed goals:

```text
Switched from micro-batches to source-page batching for ModEnc [AI].
Added 19 new direct [AI] rows for source-confirmed keys that previously only had Global/Techno fallback rows.
Updated 62 existing direct [AI] rows with source-backed Chinese Hover descriptions.
Converted 149 Global/Techno rows for source-confirmed [AI] keys into explicit non-canonical guardrails.
BuiltIn v3.2 field count is now 4662.
Exact `数值型字段` rows remain 0.
Placeholder rows decreased from 2452 to 2393.
Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed.
```

Validation result in patch environment:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target AI row validation: passed
dotnet test: not run because dotnet CLI is unavailable in the patch environment
```

---



## 11.5 FR-DQ-2H-TechnoTypes-Common-ManualApply completed

```text
TechnoTypes common-field source-family batch completed.
Verified ModEnc pages: Primary, Secondary, Strength, Speed, TechLevel, Cost, Armor, Sight, Owner, and Prerequisite.
BuiltIn v3.2 updated common object Hover descriptions with source-backed Chinese wording.
Added 39 exact object-context rows for source-confirmed Aircraft / Building / Infantry / Vehicle contexts.
Updated existing Techno rows for Primary, Secondary, Strength, Speed, TechLevel, Cost, Armor, Sight, Owner, and Prerequisite.
Updated Speed / Weapon and Strength / Projectile as source-confirmed non-Techno contexts.
Converted wrong-context or broad rows Speed / Global, TechLevel / AI, TechLevel / Global, Cost / Global, Armor / Global, Armor / Projectile, Sight / AI, Owner / AI, and Prerequisite / AI into explicit guardrails.
Deferred Strength / Shield because it belongs to a Shield / extension context and should be handled in an Ares/Phobos-specific batch.
Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, FieldRegistryDescriptionVerification_TechnoTypesCommon_2026-06-03.md, FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md.
No provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project file, user Global active pack, or legacy file was changed.
Static JSON validation passed; exact key/appliesTo duplicate check passed; dotnet restore/build/test were not run because dotnet CLI is unavailable in the patch environment.
Next recommended phase: FR-DQ-2I-TechnoTypes-CombatMobility-ManualApply.
```



## 11.6 FR-DQ-2I-TechnoTypes-CombatMobility-ManualApply completed

`FR-DQ-2I-TechnoTypes-CombatMobility-ManualApply` completed as a source-family Field Registry Hover quality batch.

Verified and applied source-backed descriptions / guardrails for:

```text
GuardRange
ROT
Locomotor
MovementZone
SpeedType
MovementRestrictedTo
Reload
Ammo
PipWrap
Passengers
Size
Category
```

BuiltIn v3.2 changes:

```text
Field count: 4701 -> 4740
New exact object-context rows: 39
Updated / guarded existing rows: 31
```

Important semantic boundaries:

- `ROT / Techno` and `ROT / Projectile` are both valid but have different meanings. `ROT / Weapon` is a guardrail.
- `Locomotor / Techno` and `Locomotor / Warhead` are both valid but have different meanings. `Locomotor / Weapon` is a guardrail.
- `MovementRestrictedTo` is confirmed for VehicleTypes only.
- `Passengers` is confirmed for VehicleTypes, AircraftTypes and BuildingTypes, not Infantry.
- `Size` is confirmed for InfantryTypes, VehicleTypes and AircraftTypes, not Building.
- `Category` is source-backed with caveat because the source documents valid categories but has an incomplete flag template.

Modified files:

```text
AGENTS.md
Docs/Codex_CurrentPhase.md
Docs/FieldRegistryDescriptionVerification_TechnoTypesCombatMobility_2026-06-03.md
Docs/FieldRegistryHoverQualityScan_2026-06-03.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs
```

No provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project file, user Global active pack, or legacy file was changed.

Static JSON validation passed; exact key/appliesTo duplicate check passed; dotnet restore/build/test were not run because dotnet CLI is unavailable in the patch environment.

Next recommended phase: `FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply`.

## 12. Verification Commands

Full validation after XAML/ViewModel changes:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

For doc-only tasks:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing, run full validation.

---

## 13. Manual Smoke For Current Task

After A15-2E-2 implementation:

```text
Open Field Learning Wizard.
Confirm custom chrome from A15-2E-1 still exists.
Confirm workflow step strip is visible.
Confirm Source / Target / Review / Apply areas are easier to understand.
Confirm source raw text area does not dominate the whole window.
Confirm tabs/grids still work.
Confirm Use Current INI works.
Confirm Parse Pasted Text works.
Confirm Build Apply Plan works.
Confirm Apply still uses existing confirmation flow.
Confirm opening/closing does not write registry files.
Confirm Shell layout is unchanged.
Confirm Field Registry behavior did not change.
```

---

## 14. Next Phase Queue

Recommended order:

```text
1. FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply
2. FR-DQ-2D Batch B Patch Plan, only after source verification completes
3. FR-DQ-2E Batch B Apply, only after patch plan approval
4. AI-4E-3 provider status / cancellation polish or AI-5 draft/copy workflow contract, only after explicit approval
5. A15-2E-3 Field Learning Wizard localization and warning/disabled reason polish
6. A15-2F Apply / Rollback confirmation consistency
7. A15-2C Field Import Preview workflow contract / implementation, if still required
8. A13/A14 Find References UX polish
9. Beta readiness smoke checklist
```

Do not start Batch B patch planning or JSON apply before FR-DQ-2C-Verify source verification. Do not start AI panel live provider switching, API key UI, settings persistence, Apply, or Insert yet.

---

## 15. Documentation Maintenance Rule

After each completed phase, Codex should update or propose updates to:

```text
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
phase-specific contract docs
```

Codex should also mention documentation updates in final report.


### FR-DQ-2J-TechnoTypes-TargetingAndTransport-ManualApply completed
FR-DQ-2K-TechnoTypes-ProductionVeterancy-ManualApply completed

This source-family batch verified and applied TechnoTypes targeting / transport / deploy / hover fields:

```text
SizeLimit, OpenTopped, DeploysInto, UndeploysInto, DeployFire, DeployFireWeapon, DeployTime, DeployToLand, Naval, Underwater, JumpJet, BalloonHover, HoverAttack
```

Modified BuiltIn v3.2 field registry metadata only. Added 29 exact object-context rows, updated 15 existing rows, and raised field count from 4740 to 4769. `DeploysInto` and `UndeploysInto` keep conservative broad Techno fallback rows while exact Vehicle / Building rows hold the stronger source-backed semantics. `Naval` separates Building shipyard behavior from Vehicle naval-unit behavior. No provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML / UI, project files, or legacy code changed.

Verification doc:

```text
Docs/FieldRegistryDescriptionVerification_TechnoTypesTargetingTransport_2026-06-03.md
```


### FR-DQ-2K-TechnoTypes-ProductionVeterancy-ManualApply

Completed manual source-family patch for TechnoTypes production, crate, veterancy, bounty, protection, and behavior fields.

Verified sources:

```text
ModEnc AllowedToStartInMultiplayer
ModEnc CrateGoodie
ModEnc Trainable
ModEnc Insignificant
ModEnc NoMovingFire
ModEnc OpportunityFire
ModEnc ToProtect
ModEnc ThreatAvoidanceCoefficient
ModEnc Soylent
Ares Bounty documentation
ModEnc VeteranAbilities
ModEnc EliteAbilities
```

Modified BuiltIn v3.2 rows for:

```text
AllowedToStartInMultiplayer
CrateGoodie
Trainable
Insignificant
NoMovingFire
OpportunityFire
ToProtect
ThreatAvoidanceCoefficient
Soylent
Bounty
VeteranAbilities
EliteAbilities
```

Result:

```text
BuiltIn v3.2 field count: 4769 -> 4808
New exact/context rows: 39
Updated existing rows: 11
Target rows with direct placeholder / generic labels: 0
Exact `数值型字段` rows: 0
Exact `整数型字段` rows: 99
```

Important semantic boundaries:

- `ToProtect / AI` is a guardrail row; the verified source makes it a TechnoTypes field, not an `[AI]` field.
- `CrateGoodie / Techno` is a broad fallback guardrail; the verified canonical target is VehicleTypes.
- `Bounty` uses the Ares boolean interpretation. The older RockPatch integer interpretation was not used as the BuiltIn v3.2 canonical form.
- No provider priority, lookup/fallback/enrichment, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project file, or legacy behavior changed.

Documentation added:

```text
Docs/FieldRegistryDescriptionVerification_TechnoTypesProductionVeterancy_2026-06-03.md
```

Next recommended phase:

```text
FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply
```


### FR-DQ-2L-TechnoTypes-CombatBehavior-ManualApply

Completed manual source-family patch for TechnoTypes cloak, radar, sensor, disguise, and immunity behavior fields.

Verified sources:

```text
ModEnc Cloakable
ModEnc CloakingSpeed
ModEnc RadarInvisible
ModEnc Sensors
ModEnc SensorsSight
ModEnc DetectDisguise
ModEnc CanDisguise
ModEnc DisguiseWhenStill
ModEnc PermaDisguise
ModEnc ImmuneToVeins
ModEnc ImmuneToRadiation
ModEnc ImmuneToPsionics
ModEnc ImmuneToPsionicWeapons
ModEnc ImmuneToPoison
ModEnc TypeImmune
```

Result:

```text
BuiltIn v3.2 field count: 4808 -> 4862
New exact/context rows: 54
Updated existing rows: 14
Target rows with direct placeholder / generic labels: 0
Exact `数值型字段` rows: 0
Exact `整数型字段` rows: 99
```

Important semantic boundaries:

- `Cloakable / Techno` is a guardrail-style broad row; verified exact contexts are InfantryTypes, VehicleTypes, and BuildingTypes.
- `DisguiseWhenStill / Techno` is a guardrail row; verified exact context is VehicleTypes.
- `TypeImmune / Techno` is a guardrail row; verified exact contexts are InfantryTypes, VehicleTypes, and BuildingTypes.
- `RadarInvisible` has broader ObjectTypes semantics, but this batch only updated TechnoTypes/object-context rows.
- `Cloakable / AttachEffect` remains unresolved for a later extension-field batch.

Documentation added:

```text
Docs/FieldRegistryDescriptionVerification_TechnoTypesCombatBehavior_2026-06-03.md
```

Next recommended phase:

```text
FR-DQ-2M-TechnoTypes-WeaponTargeting-ManualApply
```


### FR-DQ-2M-TechnoTypes-WeaponTargeting-ManualApply

Completed manual source-family patch for TechnoTypes weapon targeting, automatic acquisition, retaliation, land/naval targeting, and Weapon-only firing behavior fields.

Verified sources:

```text
ModEnc OmniFire
ModEnc DistributedFire
ModEnc FireAngle
ModEnc CanPassiveAquire
ModEnc CanRetaliate
ModEnc PreventAttackMove
ModEnc NoAutoFire
ModEnc LandTargeting
ModEnc NavalTargeting
ModEnc FireOnce
ModEnc Burst
ModEnc DecloakToFire
ModEnc UseSparkParticles
ModEnc AttachedParticleSystem
```

Result:

```text
BuiltIn v3.2 field count: 4862 -> 4887
New exact/context rows: 25
Updated existing rows: 17
Target rows with direct placeholder / generic labels: 0
Exact `数值型字段` rows: 0
Exact `整数型字段` rows: 99
```

Important semantic boundaries:

- `OmniFire`, `FireOnce`, `Burst`, `DecloakToFire`, and `UseFireParticles` are Weapon fields; existing Techno / Global fallback rows are guardrails.
- `FireAngle` is exact for VehicleTypes and BuildingTypes only.
- `PreventAttackMove` is exact for InfantryTypes and VehicleTypes only.
- `Passive / Techno` remains unresolved because no reliable ModEnc field page was found.
- `LandTargeting` and `NavalTargeting` have exact Aircraft / Building / Infantry / Vehicle rows.

Documentation added:

```text
Docs/FieldRegistryDescriptionVerification_TechnoTypesWeaponTargeting_2026-06-03.md
```

Next recommended phase:

```text
FR-DQ-2N-TechnoTypes-AircraftAndSpawn-ManualApply
```


### FR-DQ-2N TechnoTypes Aircraft / Spawn ManualApply

Completed `FR-DQ-2N-TechnoTypes-AircraftAndSpawn-ManualApply`.

This batch verified aircraft / spawner / docking / landing / missile-spawn / flight-pitch rows against ModEnc source pages and updated BuiltIn v3.2 Field Registry data.

Processed keys:

```text
Spawns
SpawnsNumber
SpawnRegenRate
SpawnReloadRate
MissileSpawn
Spawned
Dock
AirportBound
Landable
MoveToShroud
Fighter
FlyBy
FlyBack
Crashable
PitchSpeed
PitchAngle
```

Result summary:

```text
BuiltIn v3.2 field count: 4887 -> 4928
New exact/context rows: 41
Updated / guarded existing rows: 17
Target rows with direct placeholder / generic labels: 0
Exact “数值型字段” rows: 0
Exact “整数型字段” rows: 99
```

Important boundaries:

- Provider priority, lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML / UI, project files, and legacy files were not changed.
- `Spawns / Global`, `Spawned / AI`, and aircraft-only broad Techno rows are guardrails, not canonical rows.
- `Crashable` is source-backed with AircraftTypes / JumpjetCrash caveats.
- `PitchSpeed` and `PitchAngle` are exact for Aircraft and Vehicle jumpjet-style contexts; the broad Techno rows are conservative fallbacks.


### FR-DQ-2O TechnoTypes Jumpjet / Flight Tuning ManualApply

Completed `FR-DQ-2O-TechnoTypes-JumpjetAndFlightTuning-ManualApply`.

This source-family batch verified jumpjet / flight tuning / movement acceleration rows against ModEnc Jumpjet flags, individual Jumpjet parameter pages, SlowdownDistance, AccelerationFactor, DeaccelerationFactor, Weight, PhysicalSize, and Phobos Fixed / Improved Logics for Warhead jumpjet locomotor overrides.

Result summary:

```text
BuiltIn v3.2 field count: 4928 -> 4965
New exact/context rows: 37
Updated / guarded existing rows: 27
Target rows with direct placeholder / generic labels: 0
Exact “数值型字段” rows: 0
Exact “整数型字段” rows: 99
```

Important semantic boundaries:

- Jumpjet unit parameters are exact for Aircraft / Infantry / Vehicle and broad-only on Techno.
- Phobos Warhead Jumpjet* rows are extension overrides for Warheads using `IsLocomotor=yes` and `Locomotor=Jumpjet`.
- Weight is VehicleTypes-only; PhysicalSize is InfantryType display Z-fudge only.
- No provider priority, lookup/fallback/enrichment, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, or legacy behavior changed.

Documentation added:

```text
Docs/FieldRegistryDescriptionVerification_TechnoTypesJumpjetFlightTuning_2026-06-03.md
```

Next recommended phase:

```text
FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply
```


### FR-DQ-2P TechnoTypes Economy / Resource / Crush ManualApply

Completed `FR-DQ-2P-TechnoTypes-EconomyAndResource-ManualApply`.

This source-family batch verified economy, resource storage, pip display, IFV mode, bunker entry, and crush interaction rows against ModEnc source pages / field tables.

Processed keys:

```text
Storage
PipScale
Pip
Points
Bunkerable
IFVMode
Crushable
Crusher
OmniCrusher
OmniCrushResistant
CrushSound
CrushableLevel
CrusherLevel
```

Result summary:

```text
BuiltIn v3.2 field count: 4965 -> 4988
New exact/context rows: 23
Updated / guarded existing rows: 13
Target rows with direct placeholder / generic labels: 0
Exact “数值型字段” rows: 0
Exact “整数型字段” rows: 99
Placeholder rows: 2304
Source-verified rows: 737
Strict non-source-verified rows: 4251
Hover-risk placeholder/generic rows: 2403
```

Important semantic boundaries:

- `Storage` is exact for Building / Vehicle / Infantry and broad-only on Techno.
- `PipScale` is a TechnoTypes pip display selector with specific object-context rows.
- `Pip` is InfantryTypes-only; the Techno row is a guardrail.
- `Bunkerable` and `OmniCrusher` are VehicleTypes-only; Techno rows are guardrails.
- `IFVMode` is exact for InfantryTypes and VehicleTypes.
- `Crushable` also applies to broader ObjectTypes, but this batch only handled Techno/object-context rows.
- `Crusher` is exact for Vehicle and broad-only on Techno.
- `OmniCrushResistant` is exact for Infantry / Vehicle and broad-only on Techno.
- `CrushableLevel` and `CrusherLevel` remain unresolved.

Documentation added:

```text
Docs/FieldRegistryDescriptionVerification_TechnoTypesEconomyResource_2026-06-03.md
```

Next recommended phase:

```text
FR-DQ-2Q-TechnoTypes-RepairAndPower-ManualApply
```


### FR-DQ-2Q TechnoTypes Repair / Power / Capture / Factory / Radar BigBatch

Completed `FR-DQ-2Q-TechnoTypes-RepairPowerCaptureFactoryRadar-BigBatch-ManualApply`.

This source-family batch verified repair, power, capture, garrison, building visual/foundation, factory, radar, superweapon, refinery/harvester, absorb, hospital, armory, cloning, and construction yard rows against ModEnc / Ares / Phobos sources.

Processed keys:

```text
Repairable
SelfHealing
TiberiumHeal
Powered
PoweredUnit
PowersUnit
Drainable
Disableable
PoweredBy
Overpowerable
Unsellable
Capturable
NeedsEngineer
EngineerRepairable
CanBeOccupied
MaxNumberOccupants
CanOccupyFire
LeaveRubble
Bib
FreeUnit
Factory
WeaponsFactory
UnitRepair
Radar
SpySat
SuperWeapon
SuperWeapon2
SuperWeapons
NukeSilo
Refinery
Harvester
DockUnload
UnitAbsorb
InfantryAbsorb
Hospital
Armory
Cloning
ConstructionYard
```

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

Important semantic boundaries:

- Most rows in this batch are BuildingTypes-centered and broad Techno rows are guardrails.
- `Harvester` is exact for VehicleTypes.
- `PoweredUnit` is exact for VehicleTypes and `PowersUnit` is exact for BuildingTypes.
- `TiberiumHeal` carries a TS / RA2-YR caveat and is not promoted to a fully active RA2/YR healing mechanic.
- `Unsellable` is vanilla BuildingTypes and Ares-expanded TechnoTypes.
- `SelfHealing / Shield` and `Powered / Shield` are Phobos Shield rows.
- `CapturableBy` and `CanOccupyFireWeapon` remain unresolved.
- `Disableable / Techno` remains a NeedsMoreEvidence guardrail.

Documentation added:

```text
Docs/FieldRegistryDescriptionVerification_TechnoTypesRepairPowerCaptureFactoryRadar_2026-06-03.md
```

Next recommended phase:

```text
FR-DQ-2R-WeaponCore-BigBatch-ManualApply
```



### FR-DQ-2R Weapon Core Big Batch

Completed `FR-DQ-2R-WeaponCore-BigBatch-ManualApply`. Verified core WeaponType rows and closely related Phobos WeaponType extensions. Added 6 exact/context rows, updated or guarded 98 existing rows, total affected rows 104. BuiltIn v3.2 field count is now 5036. Source-verified rows are 919; strict non-source-verified rows are 4117; direct Hover-risk placeholder/generic rows are 2321. No provider priority, runtime lookup, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project, or legacy code changed. Next recommended phase: `FR-DQ-2S-WarheadCore-BigBatch-ManualApply`.


## FR-DQ-2S-WarheadCore-BigBatch-ManualApply Completed

Completed the Warhead core big batch. Verified vanilla Warhead flags, Ares Warhead extensions, and same-domain Phobos Warhead extensions. Added 8 exact/context rows, updated or guarded 97 existing rows, total affected rows 105. BuiltIn v3.2 field count is now 5044. Source-verified rows are 1023; strict non-source-verified rows are 4021; direct Hover-risk placeholder/generic rows are 2275.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_WarheadCore_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

## FR-DQ-2T Projectile Core Big Batch Manual Apply

FR-DQ-2T-ProjectileCore-BigBatch-ManualApply completed. This batch source-verified vanilla Projectile core flags, Ares projectile collision/trench extensions, and same-domain Phobos projectile interception / collision / trajectory rows.

Processed scope: AA, AG, ROT, Image, Shadow, Proximity, Ranged, Arcing, Inaccurate, FlakScatter, SubjectToCliffs, SubjectToElevation, SubjectToWalls, SubjectToBuildings, SubjectToTrenches, Acceleration, Vertical, Dropping, Arm, CourseLockDuration, Scalable, Interceptable, SubjectToGround / SubjectToLand / SubjectToWater, and Trajectory.Bombard / Parabola / Straight rows.

Added 7 exact/context rows, updated or guarded 129 existing rows, total affected rows 136. BuiltIn v3.2 field count is now 5051. Source-verified rows are 1158; strict non-source-verified rows are 3893; direct Hover-risk placeholder/generic rows are 2219.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_ProjectileCore_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md. Field Registry provider priority, provider lookup / fallback / enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider / PromptBuilder, XAML / UI, project files, user Global active pack, and legacy files were not changed. dotnet test was not run in the patch environment because dotnet CLI is unavailable; static JSON validation passed.

Next recommended phase: FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply.


### FR-DQ-2U Projectile Phobos Advanced Big Batch

Completed `FR-DQ-2U-ProjectilePhobosAdvanced-BigBatch-ManualApply`. Verified advanced Projectile rows for Airburst / Splits, AirburstWeapon additions, Retarget*, scatter customization, projectile Gravity, Parachuted / BombParachute, ReturnWeapon, and Shrapnel enhancements. Added 4 exact Projectile rows, updated or guarded 52 existing rows, total affected rows 56. BuiltIn v3.2 field count is now 5055. Source-verified rows are 1212; strict non-source-verified rows are 3843; direct Hover-risk placeholder/generic rows are 2195. No provider priority, runtime lookup, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project, or legacy code changed. Next recommended phase: `FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply`.

### FR-DQ-2V Art Animation Core Big Batch

Completed `FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply`. Verified ModEnc art(md).ini Animation playback / looping / trailer / spawn / palette / lighting rows and same-domain Phobos Animation extensions for visibility, Anim-to-Unit, fire animation spawning, animation damage, attached particle system, and debris / splash behavior. Added 14 exact/context rows, updated or guarded 93 existing rows, total affected rows 107. BuiltIn v3.2 field count is now 5069. Source-verified rows are 1312; strict non-source-verified rows are 3757; direct Hover-risk placeholder/generic rows are 2151. No provider priority, runtime lookup, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project, or legacy code changed. Next recommended phase: `FR-DQ-2W-ArtAnimationPhobosCreateAndVisuals-BigBatch-ManualApply`.


### FR-DQ-2W TechnoTypes Remaining Unresolved Guardrail MegaBatch

Completed `FR-DQ-2W-TechnoTypesRemaining-UnresolvedGuardrail-MegaBatch-ManualApply`. Converted 1413 remaining exact `Techno` direct Hover-risk rows from placeholder/generic text into explicit `NeedsMoreEvidence` guardrails. This phase intentionally did not invent canonical semantics for rows without reliable ModEnc / Ares / Phobos field-page support. The complete unresolved row list is now tracked in `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`. BuiltIn v3.2 field count remains 5069. Source-verified rows remain 1312; direct Hover-risk rows are now 1140. Field Registry provider priority, runtime lookup/fallback/enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, and legacy code were not changed. Next recommended phase: `FR-DQ-2X-SuperWeaponSideCountryUIMegaBatch-ManualApply`.


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


## FR-DQ-2Y-ArtVoxelTerrainSound-MegaBatch-ManualApply Update

- Completed on 2026-06-03.
- Baseline package: FR-DQ-2X SuperWeapon/Side/Country/UI mega batch.
- Targeted ArtObject, VoxelAnim, Terrain, Sound, ParticleSystem, and selected Global visual direct Hover-risk rows.
- Rows affected: 154.
- Source-backed rows: 14.
- Non-canonical guardrail rows: 8.
- NeedsMoreEvidence rows: 132.
- Direct Hover-risk rows reduced from 627 to 473.
- Remaining direct Hover-risk rows are tracked in `Docs/FieldRegistryHoverQualityScan_2026-06-03.md`.
- Unresolved rows are tracked in `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.

No runtime, UI, provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, project-file, or legacy behavior was changed.

Next recommended phase: `FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply`.


## FR-DQ-2Z-AresPhobosExtensions-MegaBatch-ManualApply Update

- Completed on 2026-06-03.
- Baseline package: FR-DQ-2Y Art/Voxel/Terrain/Sound mega batch.
- Targeted AttachEffect, Shield, LaserTrail, DigitalDisplay, Insignia, Radiation and related Ares / Phobos extension rows.
- Rows affected: 200.
- Source-backed rows: 192.
- Non-canonical guardrail rows: 4.
- NeedsMoreEvidence rows: 4.
- Direct Hover-risk rows reduced from 473 to 273.
- Remaining direct Hover-risk rows are tracked in `Docs/FieldRegistryHoverQualityScan_2026-06-03.md`.
- Unresolved rows are tracked in `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.

No runtime, UI, provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, project-file, or legacy behavior was changed.

Next recommended phase: `FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply`.


## FR-DQ-3A-ResidualHoverRiskBurnDown-MegaBatch-ManualApply Update

- Completed on 2026-06-03.
- Baseline package: FR-DQ-2Z Ares/Phobos Extensions mega batch.
- Goal: clear all remaining direct Hover-risk rows without inventing unsupported field semantics.
- Rows affected: 273.
- Source-backed rows added: 0.
- NeedsMoreEvidence guardrail rows added: 273.
- Direct Hover-risk rows reduced from 273 to 0.
- Direct placeholder rows reduced from 251 to 0.
- Exact integer generic rows reduced from 22 to 0.
- Exact numeric generic rows remained 0.
- NeedsMoreEvidence / unresolved guardrail rows increased from 1594 to 1867.
- Direct Hover-risk cleanup is now complete for BuiltIn v3.2.

Modified files: builtin-yr-ares-phobos-fallback-v3.2.fields.json, BuiltInFieldRegistryPackLoaderTests.cs, Docs/FieldRegistryDescriptionVerification_ResidualHoverRiskBurnDown_2026-06-03.md, Docs/FieldRegistryHoverQualityScan_2026-06-03.md, Docs/FieldRegistryUnresolvedRows_2026-06-03.md, Codex_CurrentPhase.md, RA2IniEditor_IDE_Full_Codex_Context.md, AGENTS.md.

No Field Registry provider priority, provider lookup / fallback / enrichment, Hover, Quick Peek, AI Evidence, PromptBuilder, parser, diagnostics, completion, save preflight, XAML/UI, project-file, user Global active pack, or legacy behavior was changed.

Next recommended phase: `FR-DQ-3B-FinalHoverQualityAudit`.



## FR-DQ-3C-UnresolvedRecheck-A

A targeted unresolved recheck promoted Aircraft/Weapon/Vehicle/TeamTypes rows from NeedsMoreEvidence guardrails to source-backed descriptions where ModEnc/Ares/Phobos sources explicitly supported the context. `Docs/FieldRegistryUnresolvedRows_2026-06-03.md` now tracks 1815 unresolved rows; direct Hover-risk rows remain 0.


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

## UI-DOCK-1 AvalonDock structural visual gate — 2026-07-21

The user approved the final `UI-DOCK-1` contract and authorized Shell-specific implementation. The first structural visual gate is now implemented and paused for user review.

Current production facts:

- `RA2IniEditor.IDE` references stable `Dirkster.AvalonDock` `4.74.1`; the repository root contains the required MS-PL notice.
- Shell is the sole dock composition root and starts maximized.
- `Document.Source` is non-closeable, non-floating, and non-movable.
- The 300-DIP right group contains `Tool.SectionExplorer` and `Tool.AiAssistant` and spans the full workspace height.
- The 260-DIP bottom group is nested under the editor side and contains `Tool.Problems`, `Tool.Output`, `Tool.Search`, and hidden-on-start `Tool.FindReferences`.
- Search and Find All References are internal reusable UserControls. Their old standalone Window wrappers were removed.
- Startup explicitly hides Find All References and activates Output after AvalonDock finishes its initial layout selection.
- Existing Shell commands reopen and activate the same content instances. No layout persistence is implemented in this gate.

Verification evidence:

```text
dotnet build .\RA2IniEditor.IDE\RA2IniEditor.IDE.csproj -c Debug --no-restore
Passed: 0 warnings / 0 errors.

dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
Passed: 0 warnings / 0 errors.

UI contract/boundary test filter
Passed: 58/58.
The full test project compilation still emits the unrelated pre-existing CS8602 warning in BuiltInFieldRegistryPackLoaderTests.cs:1961.

dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
Final run passed: 2275/2275.
The first full run exposed one already-existing timing race in SendAsync_LateUserCancellationDoesNotOverrideEarlierTotalTimeout; it then passed 5/5 focused reruns and the complete rerun. No AI production source was modified.

Computer-use runtime smoke
Passed: maximized startup, 300/260 structural proportions, Output default activation, Search activation/content, and Section Explorer / AI Assistant tab switching.
Available physical display: 2560x1440; restored fallback: 1280x800. The approved 1920x1080 design contract remains encoded in DIP geometry.
Native floating automation was inconclusive and is not recorded as passed; recheck manually during UI-DOCK-3.
```

No parser, Field Registry priority/lookup/data, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, undo/redo, AI request/streaming, or legacy behavior changed. The current native AvalonDock light appearance is temporary structural chrome; project-owned templates/theme belong exclusively to `UI-DOCK-3`. Versioned presentation-only persistence remains deferred to `UI-DOCK-4`. Clean packaging is deferred until the current visual gate is accepted.

## UI-MODERN-M1-R2 and UI-DOCK-5 completed — 2026-07-22

The light modern Shell package and Search floating-topology successor are complete. M1D-R2A/B/C corrected shared menu/DataGrid foundations and adopted compact Shell bottom/right/editor surfaces. M1E added project-owned non-transparent Shell chrome. M1F-R1 added project-owned AvalonDock floating chrome with a single visible title and hide-not-destroy behavior.

Search is now a hidden Floating-home `Tool.Search` with preferred 560 x 620 DIP bounds, centered relative to the editor viewport with bounded geometry. Problems/Output remain Bottom defaults, Find References remains on-demand Bottom, and Search is excluded from Bottom visibility state. `shell-layout.v2.xml` is authoritative. With no v2, a valid v1 restores the other six identities and normalizes only Search before atomically writing v2; invalid v2 never falls back to v1.

The old visible Search mock query, three sample rows and mock count were removed. The current light vertical preview exposes query, case/whole-word/regex, scope and editable file-pattern controls plus disabled previous/next/all buttons and a clear unavailable hint. Real Search/Replace/results remain out of scope for M1 and belong to future SEARCH contracts.

An AvalonDock lifecycle defect was found and fixed during real smoke: immediate Hide after `LayoutContent.Float()` raced the library's deferred floating-host Show and crashed the process. Default/migration placement and hidden visibility now execute in separate Dispatcher phases. Hidden Search persists an empty floating pane plus `PreviousContainerId`; a second process keeps Search hidden and reopening returns it to floating.

Verification evidence:

```text
dotnet restore .\RA2IniEditor.IDE.sln: passed
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore: passed, 0 warnings / 0 errors
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build: 2313/2313 passed
Search two-process UI automation smoke: 1/1 passed
IdeOnly clean package: passed, 951 files
Visual screenshot: artifacts/UI-DOCK-5-search-smoke.png
```

No public C# API, dependency, project file, parser, editor, AI, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, or legacy behavior changed.

Open verification/debt: physical 1280 x 800 and 125%/150% DPI switching were not available. AvalonDock 4.74.1 exposes the custom floating chrome controls to UIA but currently truncates the hosted Search content subtree from the floating HWND; two attempted peer-bridge approaches were rejected and fully reverted. Address this only through a narrow accessibility contract before relying on floating Search field AutomationIds in UI tests.

Next recommended stage: `UI-MODERN-M1-H1 FloatingContentAutomationAccessibility`, followed by a separately confirmed `SEARCH-1` contract for real project-index search.

## UI-MODERN-PROGRAM-R1 VISUAL-FIX3 and AGENT-AUTHORING-1 — 2026-07-23

VISUAL-FIX3 corrected two screenshot-backed UI defects and one additional project-scan
finding. The AI assistant's Clear, Advanced, Send, and Cancel controls now render the
existing `IconGeometry.Action.*` vector resources and inherit each control's Foreground;
the Shell no longer consumes the corresponding PNG action resources. The Field Registry
active-pack columns are now 80/56 DIP (the total remains 136 DIP), and the manager's
`TargetSectionKind` column is 112 DIP.

Project UI static audit facts:

```text
Production XAML parsed: 30/30
Numeric fixed-width DataGrid columns audited: 129
Likely clipped headers after fixes: 0
Raster action consumers in production XAML: 0
Targeted UI/resource tests: 53/53 passed
Full test suite: 2335/2335 passed
IdeOnly clean package: passed, 973 files
```

One theme-level candidate remains intentionally deferred: the frozen shared
`IdeSplitterStyle` in `ShellTheme.xaml` contains a hard-coded light background. There is
no current screenshot defect tied to it, and changing it would be a global shared-style
change, so it was not modified.

`AGENT-AUTHORING-1_HighLevelIniAuthoringArchitectureContract.md` records the current
authoring architecture. There is no single compiler service today. The effective language
rules are distributed across Core parse/validate, the IDE span-aware TextModel, semantic
model building, diagnostics, `IRa2FieldDefinitionProvider`, and save preflight. Existing
programmatic editing supports a single `Ra2TextChange`; it has no independent edit version,
multi-change transaction, preview result, field-registry revision, or UI-independent apply
port.

The approved future direction is:

```text
Agent adapter
  -> internal Authoring Workspace
  -> immutable Snapshot + structured Edit Plan
  -> Preview with before/after diagnostics
  -> version-checked editor transaction
  -> existing editable session and AvalonEdit Undo
  -> existing user-controlled save preflight/backup/writer
```

Built-in AI and external Codex/Agent adapters must share this path. Agents must not access
WPF controls or file writers directly, must not modify source token-by-token, and must not
auto-save. Runtime implementation is deliberately deferred to staged A1-A5 work; no public
API was added in VISUAL-FIX3.

No parser, diagnostics, completion, Field Registry provider priority/data, save,
AI provider/streaming, Dock layout, project file, dependency, or legacy behavior changed.

Next recommended stage: `AGENT-AUTHORING-1 A1 LanguageServicesFacade`, restricted to an
internal readonly facade and equivalence tests.

## AGENT-AUTHORING-1-R1 A0 Semantic Characterization — 2026-07-23

The optimized authoring plan now begins with an explicit semantic-characterization
stage. `Docs/AGENT-AUTHORING-1-R1_A0_SemanticCharacterizationContract.md` supplements
the original architecture contract and is authoritative for the A0-to-A1 transition.

Eight automated tests now lock the current observable differences between
`RA2IniEditor.Core.IniParser` and the IDE span-aware `Ra2IniTextDocumentParser`:

```text
// full-line comment
section header with unsupported trailing text
immediate semicolon inline comment
whitespace-prefixed hash inline comment
trailing-newline line representation
mixed-newline metadata
RA2IniEditor covered-field comment
empty key before '='
```

Verification:

```text
Ra2IniParserConsistencyCharacterizationTests: 8/8 passed
Related Core/TextModel parser tests: 26/26 passed
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore:
passed, 0 warnings / 0 errors
```

No production C#, public API, parser/validator/serializer behavior, diagnostics,
Field Registry, editor session, Undo/Redo, Completion, save, AI, Shell/Dock/XAML,
project file, dependency, package or legacy behavior changed.

Next safe entry:
`AGENT-AUTHORING-1-R1 A1-A ReadonlyLanguageAnalysisFacadeContract`.

## AGENT-AUTHORING-1-R1 A1 Language Services Facade — 2026-07-23

A1 continuous package is complete. The authoritative contract and result ledger are:

```text
Docs/AGENT-AUTHORING-1-R1_A1_ContinuousContract.md
Docs/AGENT-AUTHORING-1-R1_A1_StageLedger.md
Docs/ContextCapsule_AGENT_AUTHORING_1_A1.md
```

The current readonly analysis path is now:

```text
FieldRegistryRuntimeService
  -> captured Ra2FieldRegistryProviderSnapshot (Provider + Revision)
  -> Ra2LanguageAnalysisRequest
  -> Ra2IniLanguageAnalysisService
       -> existing Ra2IniTextDocumentParser
       -> existing Ra2DocumentSemanticModelBuilder
       -> existing CurrentFileReadonlyDiagnosticService
  -> Ra2IniLanguageAnalysisResult
       -> TextDocument + SemanticModel + ordered DiagnosticFact list
```

Field Registry ownership and lifecycle facts:

- initial Provider Revision is 1;
- each successful Reload publishes exactly one new Snapshot and increments once;
- loader work remains outside the publication gate;
- an old Snapshot, Request, or Result retains the old Provider and Revision after Reload;
- provider priority remains Project > Global > BuiltIn.

Language contract facts:

- all new contracts are internal; there is no public API change;
- the request is neutral and does not expose `CurrentSourceSnapshot` or `SourceEditorState`;
- one analysis uses only the captured Provider;
- diagnostics are adapted property-by-property in original order and no diagnostic algorithm is copied;
- non-fatal facade failures return a safe explicit failure result without raw exception text;
- A0 Core/TextModel characterization remains authoritative.

Open controlled debt:

```text
AGENT-AUTHORING-A1-TD-001
The facade builds a SemanticModel and the unchanged current-file diagnostic service
builds it again. Repay only if A2 Preview performance evidence identifies this as
material, or diagnostics gain a stable domain-fact input.
```

Verification:

```text
combined A1/A0/diagnostic targeted tests: 45/45 passed
IDE-only solution build: passed, 0 warnings / 0 errors
full non-UI tests: 2355/2355 passed
IdeOnly clean source package: passed, 989 files
```

No Shell/Dock/XAML/UI, parser/validator behavior, diagnostic algorithm, Completion,
Save Preflight, BuiltIn field data, project dependency, package profile, or legacy
behavior changed.

Next safe entry:
`AGENT-AUTHORING-1-R1 A2-A EditableSessionIdentityAndRevisionContract`.
This next step must contract document identity, independent Edit Revision, Registry
Revision, stale-preview rejection and current/candidate analysis ownership before
any Apply port is implemented.
