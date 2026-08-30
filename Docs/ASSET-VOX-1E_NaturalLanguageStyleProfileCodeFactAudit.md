# ASSET-VOX-1E Natural-Language Style Profile Code-Fact Audit

Date: 2026-08-26  
State: Audited / contract candidate prepared / implementation not authorized  
Audit risk: R0 (documents only)  
Proposed implementation risk: R4 (new user-authored format, model-compiled data, derived cache and canonical voxel colour changes)

## 1. Audit question

Can a user describe an RA2 voxel colour style in ordinary language, keep that description as a reusable project template,
and apply it to generated geometry without making model prose, a remote texture provider or a hidden heuristic the source
of canonical voxel truth?

The answer is **yes with a bounded two-authority design**:

```text
VOXEL_STYLE.md natural-language source
  -> dedicated DeepSeek structured compiler (proposal only)
  -> local schema/palette/region validator
  -> reviewed compiled style plan
  -> deterministic colourizer
  -> new Ra2VoxelSceneSnapshot + review artifacts
```

It is not reliable to treat the prose itself as executable painting instructions, to inject it into the general Work
prompt, or to assume that geometry-only GLB contains tyres, glass, weapons, remap masks or original colours.

## 2. Required context loaded

- `AGENTS.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `Docs/ASSET-VOX-1_SystemInvestigationAndArchitectureProposal.md`
- `Docs/ASSET-VOX-1B_CanonicalVoxelCoreFinalContract.md`
- `Docs/ASSET-VOX-1C_GenerationProviderHostFinalContract.md`
- `Docs/ASSET-VOX-1D_GlbToCanonicalVoxelCodeFactAudit.md`
- `Docs/ASSET-VOX-1D_GlbToCanonicalVoxelFinalContract.md`
- `Docs/ASSET-VOX-1D_StageLedger.md`
- `Docs/AGENT-SKILL-ROUTING-2_ModelSelectedSkillManifestContinuousFinalContract.md`
- directly related Prompt/Skill, AssetHost workspace, palette, snapshot and mesh-voxelizer implementation

No runtime source, project file, XAML or generated asset was changed during this audit.

## 3. Current implementation facts

### 3.1 Canonical geometry and palette truth already exist

`Ra2VoxelSceneSnapshot` is the existing immutable, internal, single-part authority. It owns:

- one bounded sparse occupied-cell set;
- one explicit 256-entry `Ra2VoxelPaletteProfile`;
- transparent and remap index sets that cannot overlap;
- copied part metadata and source hashes;
- stable ordering and a versioned canonical SHA-256.

An occupied cell cannot use a transparent palette index. The snapshot already has the correct granularity for colour:
every `Ra2VoxelCell` owns one palette index. No second voxel DTO is required.

### 3.2 Current 1D output is intentionally a white/uniform candidate

`Ra2MeshVoxelizer` accepts exactly one palette index or one target RGBA and assigns the resolved index to every surface
and interior cell. Its result explicitly carries `UniformColourCandidate`, `PivotReviewRequired`,
`NormalsNotGenerated`, `HvaNotGenerated`, `GameValidationNotRun` and `SemanticPartSplitNotAttempted`.

The accepted P2 sample is one connected, material-free GLB. The 1D acceptance result is a Body-only `29x64x31`,
20,261-cell candidate. It contains no trustworthy material, semantic-part or camera correspondence from which the
program could recover tyres, windows, turret, barrel, markings or remap regions.

### 3.3 Existing palette support is reusable but not a complete style engine

The current palette profile already provides:

- exact palette identity and hash;
- transparent/remap index identity;
- deterministic nearest opaque RGB selection with stable lowest-index tie-break;
- exact VOX/SliceStack round trips.

The existing nearest-RGB function is sufficient for an exact compatibility baseline, but it has no semantic region,
shade-ramp, remap-role, neighbour-stability, perceptual-error or coverage model. Those are additive colourizer concerns;
they must not be hidden inside the existing one-colour mesh voxelizer.

### 3.4 Built-in Agent Skills are not user style profiles

`Ra2AgentSkillCatalog` loads only application-bundled `AgentSkills/<id>/SKILL.md`, requires bounded frontmatter, forbids
`scripts`, caps the catalog at 64 Skills and caps selected bodies at 14 KiB. The first Work call sees a body-free manifest;
the Host validates the recommendation and injects selected bodies into the second call. Skills grant no path, network,
Apply, Save or shell authority.

This mechanism provides a useful **routing pattern**, but it is the wrong persistence owner for user styles:

- bundled Skills are application knowledge shipped with the IDE;
- a voxel style is user/project content and may change independently;
- copying style prose into `AgentSkills` would mix domain guidance with a mutable asset input;
- injecting style prose into the general Work executor would expose it to unrelated INI authoring and prompt budgets.

The 1E compiler should therefore reuse the existing AI transport/structured-response discipline, not register
`VOXEL_STYLE.md` as a built-in Skill and not create a second general Agent Skill catalog.

### 3.5 AssetHost workspace is transient, not a compiled-style repository

`Ra2GenerationWorkspace` owns one marker-protected, leased provider run. Its successful artifacts disappear when the
lease is disposed unless a later authority adopts them. It is intentionally not a persistent job, cache or project store.

A cross-run compiled-style cache therefore cannot be smuggled into the 1C workspace. If approved, 1E must introduce a
separate bounded, derived, disposable cache owned by IDE orchestration. It must never become the authoring source or a
project asset.

### 3.6 No existing natural-language style persistence or semantic mask engine was found

The repository has no current `VOXEL_STYLE.md` loader, style inheritance resolver, typed compiled style plan, semantic
region mask, deterministic multi-colour painter, colour error report or persistent style-plan cache. Adding these is a
new R4 data path, not a small extension to an existing template library.

## 4. Reuse scan

| Need | Existing authority to reuse | Gap that 1E may add | Forbidden parallel implementation |
|---|---|---|---|
| Natural-language compilation | Existing DeepSeek client and bounded structured tool-call parsing discipline | Dedicated style compiler request/result | Second AI transport/client or general Work prompt branch |
| Knowledge guidance | Existing bounded/versioned prompt-resource discipline | One fixed voxel-colour compiler instruction resource owned only by the dedicated compiler | Treat user style file as an Agent Skill or add it to the general Work manifest |
| Palette identity | `Ra2VoxelPaletteProfile` | Role/ramp resolution against the same profile | New 256-colour palette DTO |
| Voxel truth | `Ra2VoxelSceneSnapshot` | Produce a new snapshot with identical occupancy and changed palette indices | Mutable scene or provider-owned voxel DTO |
| Region storage | Canonical sorted occupied-cell order | Bounded mask aligned to that order | Model-generated per-cell JSON |
| Export/review | Existing VOX, SliceStack and PNG codecs | Swatch/mask/error reports and coloured exports | Direct VXL writer or UI bitmap path |
| Provider artifacts | Existing AssetHost lease | Caller-owned copied bytes/facts only | Returning raw provider paths |
| Cache | Existing hash/version conventions | Derived content-addressed style-plan cache | Reusing transient run workspace as persistent state |

## 5. Architecture check

### 5.1 Ownership

| Boundary | Owner | Lifetime | Authority |
|---|---|---|---|
| Natural-language source | Project/user | Persistent text file | Authoring intent only |
| Resolved source pack | IDE orchestration | Request | Exact ordered text/hash/scope facts |
| Model proposal | IDE AI adapter | Request | Untrusted structured suggestion |
| Compiled style plan | Application-internal model | Request/cache | Locally validated colour policy; review required until accepted |
| Semantic region mask | Application-internal model | Request/review artifact | Explicit evidence plus deterministic geometry classifications |
| Coloured snapshot | Existing Application voxel core | Request/artifact | Canonical candidate truth |
| Cache entry | IDE derived cache | Cross-run, disposable | Performance only; never source truth |
| Apply/project asset | Future product composition | Separate approval | Not part of 1E core |

### 5.2 Layer direction

- Application remains headless and owns only immutable data, validation and deterministic colour algorithms.
- IDE orchestration owns project-contained source discovery, DeepSeek calls, cache and user-facing composition.
- AssetHost remains provider process/workspace/artifact authority; it does not compile style prose or paint voxels.
- Provider output, user prose and cached JSON cannot construct project paths, choose executables or apply/save files.
- The existing Work INI pipeline is not extended by 1E core.

This preserves the accepted 1A/1B/1C/1D boundaries and does not require a public API.

## 6. Reliability boundary for text-only styling

Pure text can reliably state palette roles and visual policy, for example:

> 冷战盟军写实风格；车体以低饱和橄榄绿为主，顶部略亮、侧面中间调、底盘和凹陷更暗；少量红色重映射标识；
> 玻璃偏蓝灰，轮胎接近黑色；保持轮廓清晰，不使用高亮塑料感。

Text alone cannot prove which cells are glass, tyre or insignia. The first reliable implementation therefore distinguishes:

- deterministic geometry regions: top-facing exposure, side-facing exposure, underside, edge/ridge and interior;
- explicit semantic regions: user-reviewed mask, donor projection or trusted source material;
- inferred semantic regions: model suggestion without cell-level evidence.

Only the first two may paint cells as semantically named materials. An inferred-only request may still produce a coarse
geometry-banded candidate, but it must report unresolved semantic rules and keep `SemanticMaskReviewRequired`. It must
not randomly guess tyre/window cells to make the preview look more complete.

## 7. Data-model check

| Type candidate | Mutable? | Owner | Lifetime | Identity/version | Bounds |
|---|---|---|---|---|---|
| `Ra2VoxelStyleSource` | No | IDE | Request | relative path + scope + SHA-256 | UTF-8, 32 KiB each |
| `Ra2VoxelStyleSourcePack` | No | IDE | Request | ordered source hashes + schema | max 8 sources / 64 KiB total |
| `Ra2VoxelStylePlanProposal` | No | IDE adapter | Request | provider/model/schema evidence | bounded roles/rules/gaps |
| `Ra2CompiledVoxelStylePlan` | No | Application | Request/cache | canonical plan SHA-256 | fixed vocabularies and counts |
| `Ra2VoxelRegionMask` | No | Application | Request/artifact | source snapshot hash + mask hash | one byte per occupied cell, max 1,000,000 |
| `Ra2VoxelColourizationResult` | No | Application | Request/artifact | source/plan/result hashes | existing occupancy limit |
| `Ra2VoxelStyleCacheEntry` | No | IDE | Derived cache | key includes all compiler/palette inputs | bounded entry/root size |

All collections must be copied, deterministically ordered and exposed read-only. Unknown enum values, duplicate roles,
transparent occupied colours, mask/snapshot hash mismatch and coordinate-count mismatch are failures, not warnings.

## 8. Public API and persistence audit

- Proposed Application types remain internal.
- Application exported-type allowlist remains exactly 77.
- AssetHost exported public type count remains 0.
- No Automation Gateway method, Tool Catalog entry, project format or INI persistence field is added.
- `VOXEL_STYLE.md` is a new user-authored file convention and the compiled cache is a new derived persistence shape;
  this is the principal reason the implementation is R4 and requires approval.
- The source file is the only authoring truth. A cache may be deleted at any time without changing project semantics.

No public API ledger entry is required for the docs-only audit. The implementation closeout must record an explicit
zero-public-API result and the Experimental compiler wire shape.

## 9. Risk classification

### Current documentation task

- Risk: R0
- Verification: DocsOnly structure/path/diff audit
- Governance: StopForReview because the document freezes a future R4 design

### Proposed implementation

- Risk: R4
- Reasons: new source convention, model-compiled typed result, cache invalidation contract, canonical colour changes,
  and future product-visible preview artifacts.
- Mandatory stop conditions: public API drift, hidden project writes, occupancy/geometry change, unreviewed semantic
  painting, transparent/remap violation, cache becoming authoritative, Shell/UI expansion, or failed regression gates.

## 10. Audit conclusion

The design is feasible and does not require changing the accepted generation provider or voxel geometry pipeline.
The safe unit of work is a new `ASSET-VOX-1E Natural-Language Style Profile and Palette Review` package whose core is
headless and internal. UI selection/editing and project Apply remain separate approval surfaces.

The most important non-negotiable rule is: **DeepSeek may translate prose into a bounded plan, but only local validation
and deterministic painting may create a coloured canonical candidate.**
