# DIFF-REVIEW-1 Canonical Result and Object Context Final Contract

Date: 2026-08-26

Status: Approved / implemented / automated verified; manual visual acceptance pending

Risk: R3 — temporary review-document presentation model and WPF view composition

Preconditions: `CONTENT-UI-1`, `CONTENT-PROJECT-UI-1`, `CONTENT-2D-2`,
`AGENT-WORK-ENTRY-1 W1-6` completed

## 1. User-visible outcome

The temporary AI review document is changed from one flat three-line-context unified
diff into one review surface with three explicit modes:

1. `结果` — default; displays the exact canonical candidate text that the existing
   Preview will apply, in a readonly AvalonEdit editor with line numbers and INI syntax
   colors;
2. `差异` — preserves the current unified diff including deleted lines and old/new line
   numbers;
3. `对象上下文` — displays a bounded, readonly index of directly related Sections and
   navigates to their full candidate Section text. Related items are always labeled
   `未修改，仅供审阅` unless they are also in the executable plan.

For project proposals, the review surface adds document tabs for the existing captured
documents. It never invents, opens, creates or reads a path outside the Preview snapshot.

The default 1920x1080 composition is:

```text
status / statistics / diagnostics                               Apply actions
document tabs: rulesmd.ini | artmd.ini
mode: 结果 | 差异 | 对象上下文       上一个更改 | 下一个更改
--------------------------------------------------------------------------
outline 220 DIP | readonly canonical candidate / unified diff / context
--------------------------------------------------------------------------
```

The first successful load selects the first document with changes, selects the first
changed Section or line anchor, enters `结果`, and scrolls to that candidate location.

## 2. Code-fact diagnosis

Current `Ra2AuthoringDiffProjectionBuilder`:

- validates source + text changes against candidate text;
- uses a fixed `ContextLineCount = 3`;
- emits only flat Context/Added/Removed/HunkHeader/FileHeader rows;
- has no Section identity, related reference identity or candidate editor payload;
- owns useful 8 MiB / 200,000 line / 20,000 visual row / 2,000 hunk limits;
- already supports project file headers and aggregate statistics.

Current `Ra2AuthoringDiffViewModel` owns only one row list and treats diff projection
success as its presentation readiness. Current XAML uses one virtualized ListBox. The
Shell already owns the correct temporary document lifecycle, cancellation, Apply,
Dismiss and Return-to-source events. No new Dock document or Shell layout is needed.

Existing reusable facilities:

- successful document/project Preview: source snapshot, exact candidate text, plan,
  changes, operation previews, Section-creation previews and diagnostics;
- `Ra2DocumentSemanticModelBuilder`: Section spans and core direct reference symbols;
- `Ra2AutomationDocumentQueryService.ResolveReference` + `GetSection`: canonical bounded
  resolution for both semantic-known and Field Registry-declared Reference/ReferenceList
  fields, including comma-separated fields such as `Deliver.Types`;
- `ReadonlyIniHighlightTokenizer` + `Ra2KnownFieldHighlightingTransformer`;
- AvalonEdit already used by the source editor;
- current unified diff projection and Apply/Dismiss lifecycle.

## 3. Architecture invariants

1. `CandidateText` from the successful canonical Preview is the sole text shown in
   Result mode and the sole text that existing Apply may commit.
2. Review Projection is presentation-only. It cannot alter Plan, Preview, CandidateText,
   Apply policy, snapshot identity, stale state, Undo, Save or diagnostics.
3. The current unified diff builder remains the sole deleted-line/old-new projection.
4. The existing semantic builder/query path is reused. No second INI parser, regex
   Section parser, local document cache or Field Registry copy is permitted.
5. Related Sections are evidence, never executable operations. Their absence or a
   relation-index failure cannot reject, block or rewrite a valid proposal.
6. Normal and project proposals continue using the same temporary AvalonDock document,
   coordinator and Workspace transaction path.
7. The surface supports only `Apply all` and `Dismiss`. Per-file, per-Section and per-hunk
   Apply are explicitly out of scope.
8. No review mode, selected tab, outline width or expansion state is persisted to layout,
   settings or conversation history.

## 4. Allowed and forbidden files

### Allowed implementation files

```text
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringDiffProjection.cs
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringReviewProjection.cs        (new, internal)
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringResultChangeRenderer.cs   (new, internal)
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringDiffViewModel.cs
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringDiffView.xaml
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringDiffView.xaml.cs
RA2IniEditor.Tests/IDE/Ra2AuthoringDiffProjectionTests.cs
RA2IniEditor.Tests/IDE/Ra2AuthoringDiffUiContractTests.cs
RA2IniEditor.Tests/IDE/Ra2AuthoringReviewProjectionTests.cs          (new)
Docs/DIFF-REVIEW-1_StageLedger.md                                   (new)
current/product documentation required at completion
```

`ShellWindow.xaml.cs` is not expected to change: its existing construction, event and
cancellation lifecycle is sufficient. If implementation proves otherwise, work stops
before editing Shell and the contract is revised.

### Forbidden

- `ShellWindow.xaml`, main layout, toolbar, menus, Project Explorer, Navigator and bottom
  tools;
- AI prompt/provider/Work routing and proposal adaptation;
- Application/Core public API, parser semantics and project transaction services;
- Field Registry, Diagnostics, Completion, Hover and Save Preflight;
- Apply/Undo/Redo/Save behavior;
- project files, dependencies and layout persistence;
- legacy solution/editor.

## 5. Presentation data model

All new types are IDE-internal, immutable after build, request/proposal-lifetime and
non-serialized.

```text
Ra2AuthoringReviewProjection
  Succeeded
  FailureKind / Message
  Documents : ordered ReviewDocument[]
  Diff       : existing Ra2AuthoringDiffProjection

Ra2AuthoringReviewDocument
  DocumentId
  FilePath / DisplayName / RelativePath
  SourceText
  CandidateText                      // exact Preview instance
  ChangedLocations[]                 // candidate offsets/line anchors
  OutlineItems[]
  RelationState                      // Available / Partial / Unavailable

Ra2AuthoringReviewOutlineItem
  Identity : (DocumentId, SectionName, Occurrence)
  Kind     : Created / Modified / Registration / Related / Unresolved
  Label / Reason
  CandidateFullSpan? / CandidateHeaderLine?
  IsExecutableChange

Ra2AuthoringReviewMode
  Result / Changes / ObjectContext
```

Identity never uses display text alone. Duplicate Section names retain the canonical
occurrence index. A missing Section relation is `Unresolved`, not guessed.

### Ownership

| Data | Primary owner | Lifetime | Serialized |
|---|---|---|---|
| Plan / candidate / apply identity | existing Preview/Workspace | proposal | no |
| Source and candidate editor documents | review ViewModel | open review document | no |
| changed-line/Section/relation index | Review Projection | proposal | no |
| selected mode/file/outline item | review ViewModel | view instance | no |
| editor renderers/transformers | review View | loaded view | no |

## 6. Projection rules

### 6.1 Document ordering

- Document proposal: one document.
- Project proposal: exactly the existing `DocumentPreviews` order; current rules-before-art
  behavior remains.
- A document with zero executable changes cannot be added merely as context.

### 6.2 Changed locations

Candidate offsets are calculated from the already validated ordered text changes using
the same cumulative delta used by the current diff builder. This mapping must be
extracted/reused, not implemented independently in the ViewModel.

- insertion/replacement: candidate span covers new text;
- deletion: zero-length candidate anchor at the deletion position plus removed-line count;
- navigation order: document order, then candidate offset, stable for equal offsets;
- Section ownership: the candidate semantic model Section containing the candidate
  anchor; if none exists, outline fallback is `文件级变更 · 行 N`.

### 6.3 Changed outline

- every Plan Section creation is `Created`;
- every target Section of an executable operation is `Modified`, unless already Created;
- known registry Sections may receive the visual subtype `Registration`; this subtype is
  presentation-only and must fall back to Modified when classification is unavailable;
- items are deduplicated by document + case-insensitive Section name + occurrence;
- created/modified items precede related/unresolved items.

### 6.4 Related Sections

Related context is deliberately bounded and non-recursive:

- only direct outgoing references from changed Sections are considered;
- maximum depth: 1;
- maximum related items: 64 for the whole proposal;
- maximum unresolved items: 32;
- construct a request-local automation snapshot over the exact CandidateText with the
  same document identity, version and captured Field Registry snapshot; never register
  it in Project Store or Workspace;
- enumerate candidate fields from the existing semantic model, then call the existing
  `ResolveReference` for semantic-known or Field Registry-declared Reference/
  ReferenceList fields; advance `referenceIndex` only within the global related-item
  budget and stop on the canonical out-of-range result;
- call existing `GetSection` for the exact resolved target identity and occurrence;
- resolve in the source candidate document first when the canonical fact says the target
  exists there;
- when it is absent there in a project proposal, query only the other documents already
  present in the captured Project Snapshot (CandidateText for changed documents, captured
  source text for unchanged documents); accept only one exact case-insensitive Section
  match across that bounded set;
- multiple exact cross-document matches are `Unresolved`; no file priority or filename
  heuristic chooses between them;
- no fuzzy match, display-name match, web lookup, DeepSeek call or filesystem read;
- a Section already changed is not duplicated as Related;
- ambiguous/missing targets become `Unresolved` with a safe local reason;
- relation-index cancellation or failure sets `Partial/Unavailable` and leaves Result,
  Diff and Apply authority untouched.

The first implementation does not claim a complete game dependency graph. It provides
deterministic direct context only.

### 6.5 Resource limits

- retain current 8 MiB and 200,000-line limit per source/candidate document;
- retain current 20,000 unified-diff rows and 2,000 hunks;
- project document count remains the existing Preview limit;
- tokenize and build semantic indexes only in the existing cancellable background load;
- only the selected candidate document is assigned to the visible AvalonEdit document;
- Result mode never copies or concatenates multiple project documents;
- context Section extraction is bounded to 1 MiB total display text; items beyond the
  cap are listed without body text and marked `内容已省略`.

## 7. Exact UI contract

### 7.1 Existing header and command row

Preserve:

- status text, aggregate statistics and diagnostic counts;
- Return-to-source, Dismiss and Apply-all commands;
- current danger/warning/stale presentation;
- all existing AutomationIds.

The review command row is inserted below existing actions; no global toolbar changes.

### 7.2 Document selector

- hidden for one document;
- horizontal compact tabs for two project documents;
- label is file name; tooltip is existing relative path;
- selecting a document preserves current mode and selects that document's first changed
  outline item;
- no close button, reordering or drag behavior.

### 7.3 Mode selector

Three mutually exclusive compact IDE tabs:

```text
结果 | 差异 | 对象上下文
```

- Result is initial/default on every newly opened review document;
- reopening a closed review resets to Result and first change;
- keyboard: Ctrl+1 Result, Ctrl+2 Changes, Ctrl+3 Object Context;
- mode switching never rebuilds or recaptures Preview.

### 7.4 Outline

- default width 220 DIP, minimum 180, maximum 360;
- one local GridSplitter; its width lives only for the view instance;
- document/Section hierarchy with badges `新增`, `修改`, `注册`, `关联`, `未解析`;
- changed items use primary text; related/unresolved use secondary text;
- selecting Changed/Created/Registration switches to Result and scrolls to the full
  candidate Section/anchor;
- selecting Related switches to Object Context and displays its complete Section;
- selecting Unresolved shows its safe local reason and no fabricated text.

### 7.5 Result mode

- readonly AvalonEdit; ShowLineNumbers true; WordWrap false; horizontal/vertical scroll;
- exact candidate text, including original comments, blank lines, order and line endings;
- reuse `ReadonlyIniHighlightTokenizer` and `Ra2KnownFieldHighlightingTransformer` with
  the captured Field Registry provider;
- selected document's changed lines receive soft added/modified background and a 3 DIP
  accent stripe through a new internal AvalonEdit background renderer;
- deletion anchors receive a non-editable `−N 行` marker; removed content itself remains
  exclusively authoritative in Changes mode;
- selection/copy are enabled; editing, completion, context mutation commands and Save are
  absent;
- initial and outline navigation scrolls to target line and briefly selects/focuses the
  line without changing source-editor caret.

### 7.6 Changes mode

- reuse the current virtualized ListBox, row kinds, line numbers, colors and file headers;
- no algorithm rewrite and no partial-accept controls;
- Previous/Next change navigation selects and scrolls to the next hunk/change row;
- a projection-limit failure displays the existing bounded message while Result mode
  remains available.

### 7.7 Object Context mode

- top note: `关联 Section 未被本次提案修改，仅供审阅。`;
- left outline remains visible;
- selected related Section is shown in a readonly highlighted code surface with file,
  Section and relation reason header;
- changed Sections are not repeated here;
- no Apply checkbox, generation command, navigation to arbitrary paths or editing;
- unavailable state explicitly says `未能建立可靠的直接引用上下文` and points users
  back to Result/Changes.

### 7.8 Navigation commands

- `上一个更改` / `下一个更改` appear beside mode tabs;
- disabled while loading or when no changed location exists;
- wrap from last to first and first to last;
- tooltip and accessible name include the wrap behavior;
- F7 / Shift+F7 provide next/previous change shortcuts inside the review surface only.

### 7.9 Responsive behavior

- width >= 900 DIP: outline visible at 220 DIP;
- 640..899 DIP: outline becomes a 180 DIP collapsible column, initially collapsed with an
  `大纲` toggle;
- width < 640 DIP: outline is overlay-style within the control, document tabs and mode
  selector remain horizontally scrollable, existing Return button stays compact;
- Apply/Dismiss remain visible at every supported width;
- no fixed-height context card and no nested vertical scrollbar around AvalonEdit.

## 8. Automation contract

Preserve all current IDs and add:

```text
Shell.AuthoringDiff.DocumentSelector
Shell.AuthoringDiff.DocumentTab
Shell.AuthoringDiff.Mode.Result
Shell.AuthoringDiff.Mode.Changes
Shell.AuthoringDiff.Mode.ObjectContext
Shell.AuthoringDiff.Outline
Shell.AuthoringDiff.OutlineItem
Shell.AuthoringDiff.OutlineToggle
Shell.AuthoringDiff.ResultEditor
Shell.AuthoringDiff.ResultEditor.TextArea
Shell.AuthoringDiff.ContextEditor
Shell.AuthoringDiff.PreviousChangeButton
Shell.AuthoringDiff.NextChangeButton
Shell.AuthoringDiff.RelationNotice
Shell.AuthoringDiff.RelationUnavailable
```

Mode tabs expose selected state through SelectionItem/IsSelected semantics. Outline items
expose Section name, state badge and file name in the accessible name. Color is never the
only state indicator.

## 9. Failure and fallback matrix

| Failure | Result | Changes | Context | Apply authority |
|---|---|---|---|---|
| valid Preview, semantic index fails | full candidate available; line-anchor outline | available | unavailable | unchanged |
| unified diff limit exceeded | full candidate available | bounded error | best effort | unchanged |
| relation limit exceeded | full candidate available | available | first bounded items + omission note | unchanged |
| cancellation/close | no partial UI retained | no partial rows | no partial context | proposal lifecycle unchanged |
| proposal stale/dismissed/applied | readonly historical view if still open | readonly | readonly | disabled by existing ViewModel |
| invalid/failed Preview | review document does not claim usable content | no rows | unavailable | existing proposal failure |

The review surface's Apply button delegates to the existing proposal ViewModel. Review
projection results never create or revoke authority. A load failure must be visible and
must not be translated into a provider/DeepSeek failure.

## 10. Stages

### DR1-A — Review projection core

- immutable review models;
- exact document ordering and candidate ownership;
- shared source-to-candidate location mapping;
- changed Section outline and typed fallback;
- no XAML change.

Gate: projection unit tests and existing diff tests.

### DR1-B — Canonical Result surface

- readonly AvalonEdit Result view;
- existing INI highlighting;
- changed-line renderer;
- document selection and Previous/Next navigation.

Gate: UI contract tests, renderer/location tests, build.

### DR1-C — Unified Changes integration

- mode selector;
- move current ListBox under Changes without changing its semantics;
- keep Result available when unified projection reaches its own limits.

Gate: existing diff row/statistics/limits tests and view-state tests.

### DR1-D — Bounded Object Context

- depth-one exact relations;
- related/unresolved outline items;
- readonly related Section surface and partial/unavailable states.

Gate: exact/ambiguous/missing/cross-document/limit/cancellation tests.

### DR1-E — Responsive/accessibility/documentation closure

- 900/640 DIP behavior;
- AutomationIds and keyboard contract;
- current/product docs and Stage Ledger;
- full IDE-only build/test/package;
- request 1920x1080 and narrow-width manual screenshots.

## 11. Test contract

Automated tests must prove:

1. Result document text is reference-equal or exact ordinal-equal to successful
   `CandidateText`; no reconstruction or normalization;
2. project document ordering and file identities match Preview;
3. insert/replace/delete map to stable candidate anchors;
4. created, modified and registry items are deduplicated and ordered;
5. duplicate Section names preserve occurrence identity;
6. exact depth-one references resolve, ambiguous/missing references do not guess;
7. relation failure does not fail Result, Diff or proposal Apply state;
8. unified diff output and resource-limit behavior remain unchanged;
9. stale/dismissed/applied states disable existing Apply/Dismiss behavior correctly;
10. no `TextChanged` mutation path, Save command, partial Apply or layout serialization is
    introduced in the review view;
11. all old and new AutomationIds exist;
12. responsive thresholds preserve Apply/Dismiss and mode access;
13. open/close/reopen cancels and disposes editor renderers without retaining proposal
    text or event handlers.

Verification commands after implementation:

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Ra2AuthoringDiff"
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Manual acceptance:

- 1920x1080 project proposal with registry + provider + newly created SuperWeapon Section;
- Result opens on the complete candidate Section and full file remains scrollable;
- Changes still shows old/deleted lines;
- E1/FV or other exact direct references appear only under readonly context;
- 899 DIP and 639 DIP width checks;
- Apply all, Undo and no-auto-Save behavior rechecked by the user.

## 12. Explicit non-goals

- side-by-side two-editor diff and synchronized scrolling;
- arbitrary recursive dependency graph;
- fuzzy Section/display-name matching;
- Section folding persistence;
- per-hunk/per-file Apply;
- editing candidate text inside Review;
- saving/exporting a patch;
- new global Dock, toolbar/menu command or layout schema;
- DeepSeek calls for explaining the Diff.

These can be considered only after this exact-candidate review surface is manually
accepted.

## 13. Self-review

### Risk classification

- Level: R3.
- Governance mode: StopForReview.
- Public API impact: none expected.
- Persistence/snapshot/compatibility impact: none; Preview identity and CandidateText are
  consumed, never changed.
- High-risk boundary: presentation must not become Apply authority.
- Stop condition: any required Shell layout, Application public API, parser, transaction,
  persistence or dependency change.

### Architecture decision

Decision: extend the existing temporary AuthoringDiff document with one internal Review
Projection. Do not replace Preview, Diff algorithm, Workspace or Shell lifecycle.

### Reuse decision

- reuse current candidate/source/change payloads;
- reuse current unified diff projection;
- reuse existing semantic model plus canonical `ResolveReference`/`GetSection` query
  services and readonly highlighter;
- reuse current AvalonDock document and proposal event wiring;
- add only the missing presentation projection and AvalonEdit change renderer.

### Data-model decision

Review state belongs solely to the AuthoringDiff presentation layer and dies with the
temporary review document. It is never serialized, cached in Project Store or fed back
to the model. This prevents UI convenience state from becoming edit authority.

### Reliability conclusion

The user explicitly approved this contract on 2026-08-26. DR1-A through DR1-E were
implemented continuously without changing Shell or edit authority.
The design is robust because complete review never depends on semantic reconstruction:
Result always displays the exact candidate. Diff and Object Context are independently
degradable evidence layers.
