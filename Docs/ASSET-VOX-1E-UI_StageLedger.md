# ASSET-VOX-1E-UI Stage Ledger

## UI-0 Contract and architecture

- Status: completed
- Evidence: code-fact audit, final contract, R4 self-review
- Gate: passed

## UI-1 Headless preview session

- Status: completed
- Scope: bounded VOX source admission, style orchestration, immutable review projection, cancellation/stale-result ownership
- Evidence: `Ra2VoxelStylePreviewCoordinatorTests` 3/3 passed
- Gate: passed

## UI-2 Modern WPF document

- Status: completed
- Scope: responsive authoring/review surface, AutomationIds, image and plan projections
- Evidence: IDE build passed; XAML contract tests passed
- Gate: passed

## UI-3 Shell composition

- Status: completed
- Scope: one menu entry, single dynamic central document, close/shutdown cleanup
- Evidence: dynamic document lifecycle contract passed; document is absent from dock profiles
- Gate: passed

## UI-4 Verification and documentation closeout

- Status: completed
- Scope: targeted tests, final build/test/package, state docs, manual/real-provider status
- Evidence:
  - focused UI/session tests: 6/6
  - Application: 249/249
  - IDE: 2793/2793
  - AssetHost: 47/47
  - solution build: 0 warnings / 0 errors
  - IdeOnly clean package: passed, 1340 files
  - real DeepSeek: NotRun
  - physical screenshot/manual visual acceptance: NotRun
- Gate: automated gate passed; manual visual acceptance remains external

## Deferred governance queue

- downstream accepted-preview handoff/export
- explicit semantic-mask authoring/import
- project Apply/Save
- VXL/HVA materialization
- real DeepSeek and screenshot smoke test

## UI-FIX1 GridSplitter runtime style compatibility

- Status: completed
- Trigger: first physical open of the workspace raised `XamlParseException` at line 154 while assigning `Style`.
- Root cause: the WPF `GridSplitter` incorrectly referenced `IdeDockSplitterStyle`, whose target is AvalonDock
  `LayoutGridResizerControl`.
- Fix: reuse the existing WPF-compatible `UiGridSplitterStyle`; shared theme resources and Shell layout remain unchanged.
- Regression gate: the workspace contract pins the compatible resource key, the collection-style adoption allowlist includes
  this exact workspace, and the STA visual-resource test now constructs `Ra2VoxelStyleWorkspaceView` through
  `InitializeComponent()`.
- Evidence: focused 5/5; full IDE 2793/2793; solution build 0 warnings / 0 errors.
- Manual re-open after rebuilding: pending user confirmation.

## UI-R2 Unified voxel input and optional remap

- Status: completed / automated verified
- Scope:
  - ordinary VOX palettes with no remap range no longer require team-colour roles;
  - text-only, non-executable remap intent is deterministically retained as an unresolved assumption;
  - explicit/executable remap against an unavailable range still fails closed with a specific message;
  - the existing source picker accepts project-contained VOX or single-Section VXL plus an explicitly selected 768-byte PAL;
  - both inputs reuse the Stage 1B codecs and converge on the same immutable canonical snapshot.
- UI delta: existing button text/filter/status copy only; no layout, control, Shell, dock or AutomationId change.
- Evidence: IDE voxel focused 17/17; Application voxel focused 4/4; Application 249/249; IDE 2796/2796;
  Release solution build 0 warnings / 0 errors; IdeOnly clean package passed with 1341 files.
- Debug verification: blocked by the user-running IDE process; no process was terminated. Release verification is authoritative.
- Still deferred: multi-Section selection UI, semantic masks, VXL/HVA writing, normals/pivot, project Apply/Save,
  real DeepSeek smoke and game validation.

## UI-R2-FIX1 Role colour-source compatibility

- Status: completed / automated verified
- Trigger: a real DeepSeek style proposal passed JSON parsing but failed under the generic
  `A style colour role is invalid.` message.
- Root cause: the prompt/schema did not state the Application compiler's exclusive colour-source invariant; the compiler
  also collapsed invalid id, duplicate id and missing/conflicting colour sources into one message.
- Fix: state the index-or-RGB invariant in the provider prompt, normalize a redundant pair only when the canonical palette
  resolver proves both select the same entry, and return a bounded reason for every remaining role-shape failure.
- Boundaries: no PAL requirement for VOX, no colour guessing, no provider retry, no geometry, cache, public API, Shell,
  Apply/Save or VXL/HVA writer change.
- Evidence: focused voxel-style 20/20; Application plan-compiler focused 4/4; Application 249/249; IDE 2799/2799;
  Release solution build passed with 0 errors and one pre-existing nullable warning outside this scope.
- Manual real-provider recheck: pending user confirmation after rebuilding/restarting the IDE.
