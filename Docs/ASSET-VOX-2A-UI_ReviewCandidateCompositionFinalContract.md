# ASSET-VOX-2A-UI Review Candidate Composition Final Contract

Date: 2026-08-27

Risk: R3

State: approved / implemented / automated verified / manual visual gate pending

## 0. Outcome

Extend the existing central Voxel Style workspace into a truthful review surface for the completed 2A candidates. The user
can pair the current VOX/VXL baseline with a project-contained GLB, generate local Direct/Refined/Symmetry candidates,
compare them in the existing interactive 3D viewport, select one for the current in-memory style session, and compare the
ordinary and contrast-optimized colour result.

This stage performs no real DeepSeek or Tencent call and writes no asset or project file.

## 1. Approved implementation scope

Production files:

```text
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelStylePreviewCoordinator.cs
RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs
```

Add one internal presentation-only file:

```text
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelQualityReviewProjection.cs
```

Tests and documents:

```text
RA2IniEditor.Tests/IDE/Ra2VoxelStylePreviewCoordinatorTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceUiContractTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceViewModelTests.cs
Docs/ASSET-VOX-2A-UI_*.md
Docs/DecisionLog.md
Docs/PublicApiLedger.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/FeatureOverview.md
Docs/UserGuide.md
Docs/DeveloperNotes.md
Docs/ReleaseChecklist.md
```

Changing the viewport/scene builder or adding another production/test file, package, serializer, cache or Shell entry is
outside this approval.

## 2. Frozen boundaries

- `ShellWindow.xaml`, `ShellWindow.xaml.cs`, menus, docking profiles and global layout;
- Application 2A algorithms, profiles, quality gates and canonical snapshot schema;
- `Ra2VoxelRefinementAiCoordinator` and every new/automatic AI/provider call; the existing user-explicit style compile
  remains unchanged, but automated verification performs no live call;
- AssetHost, Tencent adapter, provider workspace, billing/retry and generation protocol;
- INI Work/Chat, parser, Field Registry, completion, diagnostics and save/undo semantics;
- project Apply/Save, file replacement, export, VOX/VXL/HVA writer and game validation;
- public exported APIs, persistent settings and on-disk cache formats;
- legacy editor.

## 3. Architecture

```text
admitted VOX/VXL baseline
  + explicit project-contained GLB quality source
    -> existing bounded GLB reader
    -> existing 2A quality refiner
    -> immutable Direct / Refined / optional Symmetry snapshots
    -> IDE-internal review projection
    -> existing ViewModel generation guard
    -> existing interactive 3D viewport

selected in-memory geometry
  -> existing explicit style compile
  -> ordinary colour result
  -> existing 2A palette contrast optimizer
  -> contrast colour result
  -> review only
```

`Ra2VoxelSceneSnapshot` remains the sole geometry/palette truth. Presentation rows contain only strings, numbers, status
and references to existing immutable results. The View never executes geometry, palette or quality algorithms.

## 4. Source admission and provenance

### 4.1 Preconditions

- A valid VOX/VXL baseline must already be loaded.
- The user explicitly chooses one `.glb` file.
- The GLB must be inside the active project, must not traverse a reparse point and must pass existing GLB size/content
  limits.
- Selection alone makes zero provider calls.

### 4.2 Local option derivation

The coordinator derives the conversion options exactly as audited:

```text
identity/part/role/Section/stem = baseline snapshot
target longest dimension        = clamp(max(baseline X, Y, Z), existing converter min/max)
padding                         = 2
palette                         = baseline palette
palette index                   = most-frequent occupied baseline index, lowest-index tie break
refinement profile              = asset-vox-2a/refinement-v1
symmetry mode                   = Suggest
```

No user-facing advanced numeric settings are introduced in this stage.

### 4.3 Provenance state

The session exposes `Verified`, `UserPaired`, `Mismatch` or `Unavailable`. A conflicting available `mesh.glb` hash is a
typed failure and publishes no candidate set. The common no-hash case is visibly labelled `User paired; origin cannot be
cryptographically verified` and remains review-required.

## 5. Candidate model

The workspace exposes these immutable geometry modes:

1. `Current model`: the admitted VOX/VXL baseline;
2. `Direct`: the direct candidate returned by 2A from the chosen GLB;
3. `Refined`: the bounded supersampled candidate;
4. `Symmetry`: the optional suggestion; disabled with an explanation when absent.

No mode silently replaces another. Candidate generation is atomic; a failed/cancelled/stale generation leaves the last
valid candidate set visible and does not change the working geometry.

## 6. Session geometry selection

- Add an explicit `Use for this session` command enabled only for Direct/Refined/Symmetry.
- Selection records the chosen immutable snapshot as working geometry in memory.
- It clears compiled style preview, contrast preview, review rows and prior style acceptance.
- The admitted source path and baseline remain unchanged and always remain available under `Current model`.
- A new GLB generation, baseline load, project change or document disposal clears working-geometry selection.
- There is no implicit selection after generation.

The existing style compiler consumes working geometry when present; source-pack resolution continues to use the admitted
VOX/VXL path. The status and acceptance text must name the active geometry candidate.

## 7. Colour candidate composition

On the existing explicit `Compile Preview` action:

1. compile the style plan as today against the active palette and working snapshot;
2. create the ordinary deterministic colour result;
3. run `Ra2VoxelPaletteContrastOptimizer.Optimize` locally;
4. if the plan changed, colourize a separate contrast result and publish before/after contrast facts;
5. never reject the ordinary valid result merely because no useful contrast candidate exists.

Modes:

- `Styled`: existing ordinary result;
- `Contrast`: optimized result, disabled when identical or unavailable;
- `Regions`: geometry region mask for the active working geometry;
- `Palette`: existing 2D palette swatch.

`Accept Preview` accepts the currently visible Styled or Contrast result in the current session only. It does not accept a
geometry candidate, write a file or generate VXL/HVA.

## 8. Visual composition

The current two-column IDE workspace is retained.

### 8.1 Left authoring column

- keep Source, inherited style sources, natural-language override and style acceptance;
- add one compact `Quality source` card below Source;
- show GLB display name, provenance state and one-line candidate status;
- commands: `Choose GLB…`, `Generate candidates`, `Cancel` through existing operation state;
- do not add an advanced options form.

### 8.2 Review header

Use two visibly separated compact mode groups:

```text
Geometry: Current | Direct | Refined | Symmetry | Use for this session
Style:    Styled | Contrast | Regions | Palette | Reset view | Diagnostic slices
```

At normal 1920x1080 allocation the controls remain on one compact header where possible. At narrower widths each group
uses wrapping layout; controls must not be clipped or force the viewport below 280 DIP height.

### 8.3 Review facts

Replace no existing role/rule/review information. Add compact, non-DataGrid review surfaces:

- a quality comparison matrix with Current/Direct/Refined/Symmetry columns and occupied cells, roughness, low-support
  cells, symmetry and unmatched cells;
- normal comparison summary;
- semantic-region proposals with provenance labels; and
- palette contrast before/after separation and changed-role count.

Use existing IDE severity/status visuals. `Model inferred` must never use the same visual state as `Geometry verified`.

## 9. Automation surface

Preserve every existing `VoxelStyle.*` AutomationId. Add exactly:

```text
VoxelStyle.Quality.SourcePicker
VoxelStyle.Quality.Generate
VoxelStyle.Quality.Status
VoxelStyle.Quality.Provenance
VoxelStyle.Quality.UseCandidate
VoxelStyle.Preview.Direct
VoxelStyle.Preview.Refined
VoxelStyle.Preview.Symmetry
VoxelStyle.Preview.Contrast
VoxelStyle.Quality.Metrics
VoxelStyle.Quality.NormalComparison
VoxelStyle.Quality.SemanticRegions
VoxelStyle.Quality.PaletteContrast
```

No Shell AutomationId is added or changed.

## 10. Concurrency and failure behavior

- The workspace retains one generation counter and one active cancellation source.
- Source load, GLB candidate generation and style compile are mutually exclusive operations.
- Baseline change, project change, close and explicit cancel invalidate the active operation.
- Late candidate/style/viewport results cannot replace newer state.
- Typed local failures are translated to actionable messages without exposing absolute paths outside the Source card.
- Candidate generation makes no provider request and has no retry.
- Resource or quality-gate rejection publishes no partial candidate set.
- Existing 3D face-budget failure continues to fall back to diagnostic slices.

## 11. Data and API impact

- New data is IDE-internal and session-lifetime only.
- No public/exported type or method changes.
- No serializer, project schema, preference, cache or layout record changes.
- No public API ledger candidate is promoted.
- No source file, GLB, VOX, VXL, PAL or HVA is mutated.

## 12. Continuous implementation stages

### UI-1 — Candidate transaction and provenance

- add bounded GLB admission and deterministic option derivation to the existing coordinator;
- call existing reader/refiner only;
- add typed result/provenance projection;
- focused path/hash/cancel/determinism/no-write tests;
- self-review before UI-2.

### UI-2 — ViewModel candidate lifecycle

- add candidate modes, working-geometry selection and generation/stale guards;
- clear style/acceptance state on geometry changes;
- focused lifecycle/mode/selection tests;
- self-review before UI-3.

### UI-3 — Style and contrast composition

- compile against working geometry;
- publish ordinary and optional contrast results;
- preserve existing explicit provider boundary and no-write semantics;
- focused palette/acceptance/cache/cancel tests;
- self-review before UI-4.

### UI-4 — XAML review surface

- add compact quality-source card, wrapped mode groups and quality review surfaces;
- preserve existing visual tokens, viewport and every AutomationId;
- add STA construction and exact UI-contract tests;
- self-review before UI-5.

### UI-5 — Closeout

- run focused tests, Application/IDE/AssetHost full tests, solution build and clean package;
- audit diff and public exports;
- update decision/API/product/context documentation;
- request manual 1920x1080 screenshot and interaction acceptance.

Implementation proceeds continuously only after explicit approval of this final contract. Each stage is self-reviewed; no
intermediate user approval is required unless a frozen boundary is encountered.

## 13. Verification gates

Required automated evidence:

```text
candidate path/GLB/provenance/option derivation
direct/refined/optional symmetry publication
no partial publication after failure/cancel/stale response
working geometry selection and style-state invalidation
style compile consumes selected geometry
ordinary/contrast result distinction and identical-plan disable state
project change/dispose clears candidate state
all old and new AutomationIds
no DataGrid, no new dependency, real STA InitializeComponent
no File.Write/project Apply/Save/provider invocation
full Application tests
full IDE tests
full AssetHost tests
IDE-only solution build
IdeOnly clean source package
```

Manual gate after automated closeout:

- 1920x1080 current/direct/refined/symmetry switching;
- orbit/pan/zoom/reset and slice fallback;
- narrow-width wrapping without clipped actions;
- candidate selection followed by style/contrast compile;
- visual confirmation that provenance and inferred-region states are not misleading.

## 14. Explicit non-goals

- live DeepSeek quality analysis or a Tencent regeneration;
- source-mesh editing, denoising or topology repair;
- authoritative tyre/glass/weapon masks;
- Body/Turret/Barrel split or multi-part assembly;
- VXL/HVA writing, Apply/Save/export or game smoke;
- normal-index editing/game-lighting simulation;
- advanced refinement parameter UI.

## 15. Self-review

Result: approved by the user and implemented without crossing a frozen boundary.

The contract closes the principal rework risks:

1. it does not pretend an existing VOX can recreate a mesh-derived refined candidate;
2. it reuses canonical snapshots, 2A algorithms and the existing viewport;
3. it separates geometry selection from style acceptance;
4. it makes unprovable GLB/VOX pairing explicit;
5. it adds no writer, persistence, Shell or paid-call authority; and
6. it provides a continuous testable path from candidate generation to visual comparison without a second voxel model.

Approval phrase:

```text
批准 ASSET-VOX-2A-UI 最终契约，连续执行 UI-1 → UI-5；不进行真实 DeepSeek/Tencent 调用，不修改 Shell、Apply/Save、VXL/HVA。
```
