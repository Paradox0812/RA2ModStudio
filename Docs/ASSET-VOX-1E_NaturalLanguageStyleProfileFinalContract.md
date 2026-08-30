# ASSET-VOX-1E Natural-Language Style Profile and Palette Review Final Contract

Date: 2026-08-26  
State: Approved / implemented through 1E-5 / automated verified  
Implementation risk: R4  
Governance: StopForReview  
Package: `ASSET-VOX-1E-0` through `ASSET-VOX-1E-5`

## 1. Outcome

After approval, a user can keep a reusable natural-language voxel style in `VOXEL_STYLE.md`, optionally refine it for
one request, compile it through one bounded DeepSeek structured call, review the resulting typed palette/region plan,
and deterministically recolour an existing canonical voxel snapshot.

The package produces a new coloured canonical candidate plus VOX/SliceStack/review reports. It does not claim that text
alone can identify tyres, glass, weapons or insignia; it does not generate VXL/HVA, edit a mod project, auto-apply/save,
or add a visual editor. A separately approved `ASSET-VOX-1E-UI` contract is required before any XAML/Shell work.

## 2. Contract-loading summary

### 2.1 Current goal

Add a reliable, AGENTS-like natural-language authoring source for voxel colour style while preserving local deterministic
voxel truth and making uncertainty visible.

### 2.2 Allowed implementation files

- new `RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/Ra2VoxelStyle*.cs` files;
- directly related `RA2IniEditor.Application.Tests/Ra2VoxelStyle*Tests.cs` files;
- new IDE-internal style source/compiler/cache files under `RA2IniEditor.IDE/AI/` or a narrowly named
  `RA2IniEditor.IDE/AssetAuthoring/` directory;
- one bundled default/example source under `RA2IniEditor.IDE/VoxelStyles/` and the minimum project content declaration;
- directly related IDE tests;
- one headless acceptance tool or an extension to the existing 1D acceptance path, only if it reuses the canonical
  Application APIs and writes solely under excluded `artifacts/`;
- this contract, code-fact audit, stage ledger and required decision/API/debt/status/context documentation.

Every stage must name its exact touched files before editing. The list above is an allowlist ceiling, not blanket
permission to modify every file.

### 2.3 Forbidden files and behavior

- `ShellWindow.xaml`, `ShellWindow.xaml.cs`, all XAML, Dock layout, menu/toolbar and AutomationIds;
- INI parser, Field Registry, Completion, Hover, Diagnostics, Save Preflight, project Preview/Apply/Undo/Redo/Save;
- existing Agent Chat/Work routing, INI Skill selection and current provider tool schemas;
- AssetHost protocol/workspace/lease, Tencent provider request or any paid/remote 3D generation call;
- 1A assembly semantics, 1B snapshot/VOX/VXL/SliceStack semantics and 1D geometry/topology/voxelization semantics,
  except additive internal consumers approved here;
- direct VXL writer, normal generation, pivot/mount inference, HVA, detached-part inference and game readiness claims;
- arbitrary filesystem discovery, project-external includes, scripts, plugins, URLs, shell commands or executable content;
- dependencies, public .NET APIs, a second voxel DTO, a second AI transport or a second general Agent Skill catalog.

### 2.4 Semantic boundary

1E may change only the palette index of an already occupied canonical cell. For a successful result:

- scene dimensions, part descriptor, coordinates, occupancy count, connectivity and source geometry hash are unchanged;
- no cell is added, deleted, moved or assigned a transparent index;
- remap indices are used only by an explicit reviewed remap rule/mask;
- the input snapshot remains immutable and available;
- the output receives style-plan provenance and a different canonical hash when colours differ.

### 2.5 AutomationIds

None. The core package has no UI. Candidate UI IDs must not be reserved or added until the separate UI inventory and
exact UI contract are approved.

### 2.6 Validation commands

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build --filter FullyQualifiedName~Ra2VoxelStyle
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter FullyQualifiedName~VoxelStyle
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

The focused IDE command becomes mandatory only after IDE orchestration exists. A real DeepSeek call is not a default
automated gate; one explicitly authorized free/test call may be used in 1E-5, otherwise a recorded response fixture is
the compiler integration evidence.

### 2.7 Approval

The user approved continuous implementation of `1E-1` through `1E-5` on 2026-08-27. That approval did not include UI,
real DeepSeek calls, project Apply/Save, VXL/HVA or game validation; those boundaries remain frozen.

## 3. Source convention: `VOXEL_STYLE.md`

### 3.1 Authoring format

- UTF-8 text, optional UTF-8 BOM, normalized internally to LF for hashing.
- Natural-language Markdown; no mandatory YAML/JSON/frontmatter.
- 1..32,768 characters per file; maximum 8 resolved files and 65,536 characters for the complete source pack.
- NUL and invalid Unicode are rejected. Unknown Markdown is retained as prose, never executed.
- No `include`, macro, script, URL fetch, environment variable or arbitrary path expansion exists in v1.
- A file may mention a desired donor/style reference, but that mention never opens a file. Donor data must be supplied
  separately as an already contained, hash-identified Host input.

Recommended headings are advisory only:

```markdown
# 风格名称

## 整体观感
## 主色和明暗
## 材质与语义区域
## 阵营重映射
## 禁止项
## 不确定时的处理
```

Example valid source:

```markdown
# 冷战盟军装甲车

整体使用低饱和橄榄绿，避免塑料高光。顶部使用略亮的同色，侧面保持中间调，底盘、凹陷和朝下表面
使用更暗的绿色。轮胎接近黑色，玻璃使用低饱和蓝灰，裸露金属只用于很小的边缘。

只在明确提供重映射蒙版时使用阵营色；没有蒙版就保留为待确认，不要猜测标识位置。不能可靠识别轮胎或
玻璃时，只做几何明暗分层并在报告里提示，不要随机涂色。
```

### 3.2 Resolution and precedence

The trusted request supplies a project root and optional target asset directory. Both must be normalized, contained and
free of escaping reparse-point traversal.

Resolution order is broad to narrow:

1. one bundled default style;
2. project-root `VOXEL_STYLE.md`, when present;
3. each ancestor `VOXEL_STYLE.md` from project root to the explicit target asset directory;
4. one explicit per-request natural-language override, maximum 8,192 characters.

The resolver checks only the exact ancestor chain; it never recursively scans a project. If no target directory exists,
only the project root participates. A narrower source has semantic precedence, but prose is not locally merged. The
compiler receives the ordered, scope-labelled pack and must attach one or more valid source-scope IDs to every resulting
role/rule. An unresolved material conflict returns `ClarificationRequired`; it does not silently choose a value.

The bundled default ensures a compilable coarse style but cannot authorize semantic masks or remap.

## 4. AI compilation contract

### 4.1 Dedicated compiler, not general Work

The style compiler is a dedicated IDE-internal operation. It may reuse the current DeepSeek client, request/diagnostic
types and strict structured tool parsing, but it does not enter the INI Chat/Work pipeline and does not change existing
Skill routing.

For a cache miss it performs exactly one non-streaming structured call. There is no automatic retry in 1E. A provider,
timeout, malformed-tool or validation failure returns a typed failure and leaves the source snapshot untouched.

The call receives:

- the ordered style source pack;
- the target part role and bounded geometry facts, never raw project paths;
- palette identity, transparent/remap facts and bounded RGB swatches;
- the fixed region vocabulary and output schema;
- optional hash-identified donor statistics/mask facts;
- one fixed, versioned voxel-colour compiler instruction resource owned by this dedicated operation. It is not registered
  in `Ra2AgentSkillCatalog` and is not exposed in the INI Work manifest.

It receives no files, shell, network tool, Apply/Save tool, project write authority or provider executable controls.
Style prose is marked untrusted data and cannot change compiler/system rules.

### 4.2 Proposal result

The tool result has exactly one outcome:

- `proposal`: contains a bounded complete plan proposal;
- `clarification`: contains a non-empty user-facing question and no paint rules;
- `unsupported`: explains which requested style requirement cannot be represented locally.

A proposal contains:

- title and short summary;
- 1..32 colour roles;
- 1..64 ordered region rules;
- interior/hidden-surface policy;
- remap policy;
- unresolved assumptions and required review items;
- source-scope provenance for every role/rule;
- no per-voxel coordinates, paths, commands or output files.

Unknown properties, duplicate IDs, over-limit arrays, non-finite values, invalid enum strings or missing provenance make
the proposal malformed. Free-form assistant text is not parsed as a fallback.

### 4.3 Colour-role vocabulary

Each role has a stable local ID plus one category:

```text
BodyBase, BodyLight, BodyMid, BodyDark, Underside,
Glass, Rubber, BareMetal, Accent, Remap
```

It specifies exactly one target:

- an sRGB triplet; or
- an exact palette index requested by the user/profile.

Local compilation resolves sRGB to an opaque palette index. Exact index targets are checked against the active palette.
`Remap` must resolve to `Ra2VoxelPaletteProfile.RemapIndices`; all other categories exclude remap by default. An occupied
cell can never resolve to a transparent index.

### 4.4 Region vocabulary and evidence

The model can only select from:

```text
WholePart, TopExposed, SideExposed, UnderExposed, EdgeOrRidge, Interior,
ExplicitMask, DonorMask, SourceMaterialMask
```

Evidence is one of:

```text
DeterministicGeometry, ExplicitUserMask, DonorProjection, SourceMaterial, InferredTextOnly
```

The model cannot return cell coordinates or invent an explicit mask. `InferredTextOnly` is valid as an unresolved intent
fact, but cannot drive `Glass`, `Rubber`, `BareMetal`, `Accent` or `Remap` painting. Such a rule is retained in the report
and sets `SemanticMaskReviewRequired`.

## 5. Local compiled-plan contract

### 5.1 Authority transition

`Ra2VoxelStylePlanProposal` is untrusted. The local compiler produces `Ra2CompiledVoxelStylePlan` only after validating:

- exact source-pack, palette and input snapshot hashes;
- schema/compiler/model identity;
- all IDs, categories, bounds and provenance;
- role-to-palette resolution and deterministic tie-break;
- remap/transparent restrictions;
- region/evidence compatibility;
- conflict and coverage policy.

The compiled plan is immutable, canonically serialized and SHA-256 identified. Local validation does not attempt to
decide whether prose aesthetically matches the proposal; the first use of a new plan remains review-required.

### 5.2 Palette distance

1E-2 must keep the current squared-RGB nearest-index method as an explicit compatibility metric. It may add one fixed,
integer-defined perceptual metric only if its formula, coefficient scale, overflow bounds and lowest-index tie-break are
frozen by golden tests. It may not use platform-dependent image libraries or floating-point colour-management profiles.

The compiled plan records the metric ID. Changing the metric ID invalidates cache entries and requires new golden output;
it must never silently recolour a previously accepted plan.

### 5.3 Cache

The source file remains the authoring truth. The optional cache is derived and disposable:

```text
%LocalAppData%\RA2IniEditor\AssetStyleCache\v1\<cache-key>.json
```

The cache key is SHA-256 over a versioned canonical sequence containing:

- ordered source scope IDs and hashes;
- per-request override hash;
- compiler schema/prompt revision;
- provider/model/revision identity;
- active palette profile hash;
- target part role;
- geometry-facts hash;
- donor/mask fact hashes;
- colour-distance metric ID.

An exact valid hit performs zero model calls. Unknown schema, hash mismatch, malformed JSON or interrupted write is a
cache miss, not task failure. Writes are atomic replace-from-temp. The root is capped at 64 MiB and 256 entries; cleanup
uses deterministic oldest-access/then-key order. Cache data contains canonical compiled JSON and safe provenance only:
no API key, absolute project path, raw image, raw donor binary, conversation or provider response body.

Cache entries are immutable. Concurrent writers use unique temporary names and atomic create/move; the loser revalidates
the winning exact-key entry and discards its temporary file. Readers open with deletion sharing and treat a concurrent
eviction as a miss. Cleanup inspects direct regular files only, rejects reparse points, and evicts by entry creation time
then cache key until both limits hold; it never follows directories or repairs unknown files.

The cache is not stored under the AssetHost workspace, project tree or source package.

## 6. Deterministic colourization contract

### 6.1 Geometry-derived mask

The colourizer analyzes only existing occupancy using the canonical axes and fixed neighbour order
`+X, -X, +Y, -Y, +Z, -Z`. A face is exposed when its adjacent coordinate is outside the grid or unoccupied. It derives:

- top exposure;
- side exposure;
- underside exposure;
- edge/ridge exposure, defined as exposure in at least two distinct axis families (`X`, `Y`, `Z`);
- interior cells.

`TopExposed` is `+Z`, `UnderExposed` is `-Z`, and `SideExposed` is any `X/Y` exposure. These regions may overlap;
the compiled fixed painting order in section 6.3 resolves them and an explicit semantic mask remains narrower than every
geometry region. The classification is an immutable byte-per-occupied-cell mask aligned to the canonical sorted cell order. Its header
contains source snapshot hash, cell count, region table and mask hash. It cannot outlive or be applied to a different
snapshot hash.

### 6.2 Explicit semantic mask

1E may consume an already supplied, project-contained or review-artifact mask only after exact dimensions, source hash,
cell count, region vocabulary and hash validation. Mask import is read-only. The style prose cannot name a path that the
compiler opens. Donor-image projection and automatic segmentation are not implemented in the first package; their data
contracts may be reserved, but they remain `Unsupported` until a later approved stage proves them.

### 6.3 Painting order

1. establish `WholePart` base role;
2. assign `Interior` using the compiled interior policy, never an implicit transparent/default index;
3. apply exposed geometry in exact order `SideExposed`, `TopExposed`, `UnderExposed`, `EdgeOrRidge`;
4. apply validated explicit semantic masks;
5. apply remap only where an explicit reviewed remap mask exists;
6. verify every occupied input coordinate appears exactly once in output.

Rule ordering is stored in the compiled plan. Equal-priority overlap is a validation failure. There is no random seed,
dithering or model callback inside painting.

### 6.4 Result and review flags

`Ra2VoxelColourizationResult` contains one success snapshot or one typed failure, never both. Success reports:

- source snapshot/style plan/palette/result hashes;
- per-role and per-region cell counts;
- distinct palette indices and remap coverage;
- unresolved semantic rules;
- palette distance/error statistics;
- geometry and occupancy equality facts;
- deterministic replay facts;
- review flags.

Required review flags include:

```text
StylePlanReviewRequired, TextOnlyCoarseStyle, SemanticMaskReviewRequired,
RemapReviewRequired, PaletteErrorReviewRequired, PivotReviewRequired,
NormalsNotGenerated, HvaNotGenerated, GameValidationNotRun
```

`UniformColourCandidate` from the source is not mutated. The new result independently reports whether output is still
uniform. It cannot clear pivot/normal/HVA/game flags.

## 7. Failure taxonomy

Expected operational failures are typed and sanitized:

```text
None,
NoStyleSource,
InvalidEncoding,
SourceTooLarge,
TooManySources,
SourcePathOutsideProject,
SourcePathRejected,
CompilerUnavailable,
CompilerTimeout,
CompilerProviderFailure,
MalformedProposal,
ClarificationRequired,
UnsupportedStyleRequirement,
SourceScopeMismatch,
UnknownColourRole,
UnknownRegion,
PaletteMismatch,
PaletteResolutionFailed,
TransparentIndexSelected,
RemapPolicyViolation,
MaskSnapshotMismatch,
MaskShapeMismatch,
RuleConflict,
CoverageViolation,
ResourceLimitExceeded,
AnalysisFailed,
Cancelled
```

Malformed cache entries are cache misses and are recorded as diagnostics, not exposed as a separate authoring failure.
Programmer-contract violations may throw; user/model/file/provider failures must not cross the seam as raw exceptions.

## 8. Review artifacts

The headless acceptance path may write only below excluded `artifacts/asset-vox-1e-acceptance/<case>/`:

```text
style-source-pack.json        safe hashes/scopes only
compiled-style-plan.json      canonical typed plan
colour-review-report.json     coverage/error/review facts
palette-swatch.png            exact active palette and selected roles
region-mask.png               deterministic/existing mask visualization
remap-mask.png                explicit remap coverage, if any
body-coloured.vox             existing restricted VOX v150 output
body-coloured-slicestack.png  existing exact RGBA SliceStack output
```

No artifact is adopted into the mod project. Raw absolute paths, secrets, prompt bodies and API responses are excluded.

## 9. Continuous implementation stages

### 1E-0 — Audit and contract

Deliver this code-fact audit and final contract, perform R4 self-review and stop for approval.

Acceptance: docs-only diff; no runtime/UI/project changes.

### 1E-1 — Source pack and deterministic resolution

Implement bounded `VOXEL_STYLE.md` loading, exact ancestor-chain resolution, scope/hash facts, per-request override and
path/reparse-point containment. Add one bundled default/example profile.

Acceptance: encoding, bounds, ordering, precedence labels, no recursive scan, escape/reparse rejection and stable hashes.

### 1E-2 — Structured compiler and compiled-plan cache

Implement the dedicated one-call DeepSeek compiler seam, strict proposal parser, local compiled-plan validation, canonical
serialization and bounded content-addressed cache. Tests use deterministic response fixtures; no real call by default.

Acceptance: exact hit uses zero calls; miss uses one call; no retry/Work pipeline mutation; malformed/unknown output,
model/version/palette/hash invalidation and atomic-cache corruption cases pass.

### 1E-3 — Deterministic regions and palette colourizer

Implement geometry-derived mask, role resolution, fixed paint ordering, interior policy, remap prohibition without mask
and immutable output snapshot. Reuse existing palette/snapshot/codecs.

Acceptance: occupancy/geometry equality, transparent/remap safety, overlap/conflict failure, deterministic replay,
snapshot immutability and VOX/SliceStack exact round trip.

### 1E-4 — Headless review package

Add safe review facts and the artifact set in section 8. Exercise coarse text-only, explicit mask, remap-prohibited,
palette-error and invalid-mask matrices.

Acceptance: all claims trace to hashes/counts; no project adoption; no VXL/HVA/GameReady claim.

### 1E-5 — Real candidate acceptance and closeout

Apply an approved natural-language profile to the existing P2/1D Body candidate. Verify repeated local colourization,
golden artifacts, full Application/IDE/AssetHost regression, build and clean package. A real compiler call requires a
separate explicit free/test-call authorization; otherwise use a reviewed captured fixture and report `NotRun` for live AI.

Acceptance requires manual visual review to be reported separately as `Passed`, `Failed` or `NotRun`; automated hashes
cannot certify aesthetics.

## 10. Verification matrix

| Area | Minimum evidence |
|---|---|
| Source discovery | root/ancestor order, missing file, max count/size, UTF-8, escape, reparse, stable normalization |
| Compiler | proposal/clarification/unsupported, exact shape, unknown field, duplicate IDs, limits, provenance |
| Model calls | cache hit 0, cache miss 1, failure 1, cancellation bounded, no implicit retry |
| Cache | full-key invalidation, atomic write, corrupt miss, size/count eviction, no secrets/absolute paths |
| Palette | exact index, RGB tie-break, transparent exclusion, remap inclusion/exclusion, palette-hash mismatch |
| Regions | six deterministic geometry regions, source-hash binding, explicit mask validation, overlap failure |
| Painting | source immutable, geometry/occupancy unchanged, stable output hash, all cells assigned once |
| Honesty | text-only tyre/glass/remap remains unresolved; normal/pivot/HVA/game flags preserved |
| Codecs | coloured VOX and SliceStack exact palette-index/coordinate round trip |
| Boundaries | no public API, Gateway, INI, Field Registry, Shell/XAML, provider protocol or project write change |
| Regression | focused + full Application/IDE/AssetHost + build + clean package |

## 11. Stop rules

Stop implementation and do not claim stage success if any of these occurs:

- style prose or model proposal becomes canonical without local typed validation;
- the model returns or paints per-cell coordinates;
- output geometry, occupancy, part identity or source snapshot is mutated;
- semantic material or remap is painted from text-only inference;
- a transparent index reaches an occupied cell;
- cache changes semantics, survives a mismatched key or is placed in project/AssetHost run storage;
- the general INI Work pipeline, public API, Shell/XAML or project Apply/Save is changed;
- a real remote call is made without explicit authorization;
- focused/full/package gates fail and cannot be corrected inside the approved boundary.

## 12. Deferred scope

- `ASSET-VOX-1E-UI`: style selection/editor, plan diff, swatches, mask/error overlays and explicit accept/recompile controls;
- automatic image/material semantic segmentation and donor projection;
- generated texture/PBR provider integration;
- detached Body/Turret/Barrel generation and per-part style inheritance;
- pivot/mount calibration, normals, direct VXL writer, HVA and game smoke;
- project asset adoption, Undo/Redo/Save and Work-Agent end-to-end asset creation.

These are not silently implemented under this approval.

## 13. Self-review

| Gate | Result | Reason |
|---|---|---|
| Risk classification | Passed | docs are R0; implementation correctly classified R4/StopForReview |
| Architecture | Passed | IDE orchestrates AI/files/cache; Application owns deterministic colour; Host remains provider-only |
| Reuse | Passed | reuses existing client discipline, palette, snapshot and codecs; no parallel core DTO/transport |
| Data model | Passed | source/proposal/compiled plan/mask/result/cache have explicit owner, lifetime, bounds and hashes |
| Public API | Passed | all planned runtime types internal; allowlist 77 and AssetHost exports 0 stay frozen |
| Persistence | Passed with approval gate | source convention and derived cache are versioned; cache is non-authoritative/disposable |
| Token economy | Passed | exact cache hit 0 calls; miss exactly 1; no model-generated cell arrays or automatic retry |
| Semantic honesty | Passed | text-only cannot assert tyre/glass/remap regions; unresolved rules remain visible |
| UI boundary | Passed | no UI/AutomationIds; separate exact contract required |
| Verification | Passed | focused, malformed, deterministic, real-candidate, regression and package gates are defined |
| Rework risk | Acceptable | model/compiler and local painter are separated; future masks/providers/UI can extend inputs without replacing voxel truth |

Self-review found no unresolved contract blocker. The main residual risk is visual quality: coarse geometry bands are
deterministic but cannot replace semantic masks. The contract makes that limitation a visible review state instead of
encoding a guess.

## 14. Approval record

Approved text:

```text
批准 ASSET-VOX-1E Natural-Language Style Profile 最终契约，连续执行 1E-1 → 1E-5；
UI、真实 DeepSeek 调用、项目 Apply/Save、VXL/HVA 仍需单独批准。
```

Implementation and verification evidence is authoritative in `ASSET-VOX-1E_StageLedger.md`. Core implementation is
complete; the separately named boundaries above remain unapproved.
