# RA2IniEditor.IDE Developer Notes

These notes describe the current IDE-only package structure. Legacy table-style editor projects and root legacy files are intentionally absent.

## 1. Solution Entry

Use `RA2IniEditor.IDE.sln` for current IDE-only development.

Common commands:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Do not restore legacy root `RA2IniEditor.sln` or `RA2IniEditor.csproj` for IDE-only validation.

## 2. Project Structure

- `RA2IniEditor.Core`: core INI document model, parsing, schema, field definitions, and validation primitives.
- `RA2IniEditor.Infrastructure`: infrastructure services, field registry loading, BuiltIn v3.2 fallback registry assets, import / apply support, and IO helpers.
- `RA2IniEditor.Application`: Core-only `net8.0` document query, diagnostics, semantic edit-preview implementation, and Experimental high-level contracts.
- `RA2IniEditor.IDE`: WPF IDE shell, Source Editor integration, project explorer, navigation, completion, hover, diagnostics, save preflight, and field registry UI.
- `RA2IniEditor.Tests`: unit and boundary tests for Core, Infrastructure, and IDE behavior.
- `RA2IniEditor.UiAutomationTests`: opt-in UI automation tests for selected IDE smoke paths.
- `tools/package-source-clean.ps1`: clean source package generator. Use `-Profile IdeOnly` for the current package.

## 3. Source Editor Direction

The IDE package is source-first. The AvalonEdit-based Source Editor is the primary editing surface.

Development should preserve:

- text buffer and caret stability
- dirty-state tracking
- undo / redo expectations
- completion commit behavior
- hover and reference hover behavior
- diagnostics refresh boundaries
- save preflight behavior

Avoid reintroducing old table-editor assumptions into new IDE flows.

### File-association launch boundary

- `App` delegates startup arguments to the internal `Ra2LaunchRequestParser`.
- One raw existing `.ini` uses its direct parent as project root; the existing
  `--automation-open-folder` path remains compatible.
- `ShellWindow` waits for initial dock readiness, then reuses the same project-open,
  project-session, exact-file-load and editable-session path as normal navigation.
- Startup must not read INI text directly, search ancestor roots, save, invoke AI, or
  introduce a second project authority. Single-instance IPC is deferred.

## 4. Field Registry Direction

Field metadata is resolved conservatively through:

1. Project registry
2. Global registry
3. BuiltIn fallback

BuiltIn v3.2 fallback data is packaged through `RA2IniEditor.Infrastructure`. Project and Global metadata should remain distinct so provenance and priority stay understandable.

Field learning / import preview flows should remain reviewable before writing changes. Apply and rollback workflows must be explicit.

BuiltIn data-quality invariants are enforced by loader tests: no uniform inferred templates, `auto-extracted` rows, empty/unrecognized quality labels, or duplicate key + appliesTo identities. Evidence-insufficient rows are quarantined rather than relabeled as verified. `Ra2CompletionProvider` excludes VerifiedGuardrail, Obsolete, NonExistent, and PseudoField definitions only from field-name candidates; lookup, Hover, Quick Peek, Diagnostics, value completion, and commit behavior must not be coupled to that visibility filter.

## 5. Diagnostics And References

Diagnostics should help authors inspect parse issues, validation results, unresolved references, and project understanding gaps.

Reference features such as Reference Value Hover, Quick Peek, and Find References should use available project context without assuming every mod-specific value can be resolved.

Warnings should stay conservative where RA2-family mods commonly use soft references or extension-specific behavior.

### Search / Replace Boundary

- `Ra2ProjectSearchService` consumes the canonical Project Explorer descriptor list; it must not enumerate directories.
- The active file uses in-memory editor text; non-active files use `ReadonlyIniContentService`.
- Regex matching retains an explicit timeout and the 10,000-result safety bound.
- Current-file replacement is preview-first and binds to `DocumentId`, `EditRevision`, and original text.
- Replace All uses one existing programmatic semantic Undo transaction and must not call save or disk-write APIs.
- Project-level/multi-file replacement requires a separate contract.

## 6. Save And Safety

Save behavior should remain guarded by preflight checks where applicable.

When a workflow writes project or registry files, prefer explicit backup and rollback paths. Backup / rollback is a safety layer and should not replace version control.

## 7. IDE-Only Package Rules

The IdeOnly package should include only the current IDE solution, supporting projects, tests, tools, documentation, and required field registry assets.

It must not include or restore:

- legacy root `RA2IniEditor.sln`
- legacy root `RA2IniEditor.csproj`
- legacy MainWindow
- legacy table-style editor source
- legacy object workbench, country manager, side manager, or old object copy workflows

## 8. Testing Notes

Ordinary validation should use the non-UI test project:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
```

UI automation tests live in `RA2IniEditor.UiAutomationTests` and should remain opt-in so normal validation does not unexpectedly launch WPF windows.

## 9. AI Assistant Boundaries

- `DeepSeekRa2AiModelCatalog` owns the typed Flash/Pro display-name and API-id mapping; V4 Flash is the default.
- One immutable configuration snapshot must be shared by Shell status presentation and client construction for a request.
- Production code has no Mock/Fake provider path. Deterministic substitutes belong only in tests.
- Remote endpoints require HTTPS; HTTP is allowed only for loopback verification. Sensitive configuration and provider error bodies must not enter UI diagnostics.
- Prompt preparation uses shared outbound sanitization and deterministic per-section/total budgets. Prompts over 8000 characters are rejected before request-session creation.
- Response construction is factory-controlled, request diagnostics are request-local, and callback consumer failures must propagate unchanged.
- Keep provider/transport failures separate from local Work admission. `LocalRejection` is internal and transient,
  carries only a locally generated safe message, has no provider error body or tool call, and must use
  `FailureKind.None`; do not map intent validation or project-availability failures to `ProtocolError`.
- Existing Pipeline overloads are advisory-only. Shell may select `CurrentDocumentEditPreview` only for a ready official endpoint and a successfully captured editable `Ra2AuthoringSnapshot`.
- Production Work exposes only `preview_ini_edit_plan` for current documents and `preview_ini_project_edit_plan` for captured rules/art pairs. `expand_ini_content_template` and `expand_ini_project_content_template` remain headless compatibility entry points and must not be exposed by the production catalog. Provider arguments remain untrusted: duplicate identity, malformed JSON, unsafe identifiers, resource overflow, unsupported operations and snapshot mismatch reject; additive descriptive metadata is ignored.
- `Ra2AiAuthoringCoordinator` owns one active proposal and reuses the A3 Preview/apply transaction. Added errors block Apply; Apply always requires explicit confirmation and never calls Save or disk IO.
- Ordinary DeepSeek Tool Calls are not strict-schema guaranteed. The adapter may normalize only unambiguous presentation drift (trailing comma, inferable outcome, missing display summary, one operation object, numeric scalar value). Template arguments additionally accept the declared named object, numeric scalars, boolean-to-Yes/No conversion, and a numeric version string. Unknown/duplicate properties, null/composite template values, and boolean/null/composite field-operation values remain rejected; never add a general JSON repair path.
- In generic production document/project proposals, `summary` and `message` are presentation metadata, not execution authority. Ignore proposal `message` regardless of JSON shape; use a fixed local summary when provider `summary` is missing, blank, null or non-string. Only `operations`/`documents` enter canonical Preview. Explicit headless template contracts retain their own typed argument validation. Clarification still requires a non-empty bounded string message and never executes echoed proposal payload.
- An explicit `needs_clarification` outcome remains non-authoring even if a non-strict provider echoes proposal-shaped fields. Return only the validated bounded message and keep every echoed template field inert. For explicit complete-object requests, prompt the provider to choose conservative visible draft tuning values when only gameplay tuning is omitted; clarification is reserved for unresolved owner, slot, or object identity.
- `Ra2AutomationTemplateService` exposes one source-audited Weapon/Projectile/Warhead relationship skeleton. Internal template definitions compile only into the canonical EditPlan; do not add raw section bodies, gameplay defaults, registration edits, persistence, or a parallel Apply service.
- `Ra2AutomationTemplateService` also exposes one source-audited direct-fire complete profile. It must bind one unique existing compatible owner, validate its complete argument set, and compile into the same canonical EditPlan; only explicit skeleton language may route to the sparse profile.
- `Ra2AgentSkillCatalog` loads only bundled `AgentSkills/*/SKILL.md` packages, validates bounded metadata/content, rejects scripts, and owns both the compact manifest and full bodies. Work call one receives metadata only and recommends ordered IDs; the Host resolves them against the same catalog snapshot, forces capability-critical and field-trust Skills, validates mode and the 14 KiB body budget, then passes the explicit resolution to call two. Chat retains exact-domain local selection. Skills are prompt knowledge, never capabilities or authorization. External roots, hot reload and executable Skills require a later security/versioning contract.
- `AGENT-QUERY-2` owns the bounded Work retrieval loop. Intent analysis may request `get_section`, `resolve_reference`, or `search_objects` against symbolic `current/rules/art`; at most two compact refinement calls may add new fingerprints before execution. Search reuses `Ra2DocumentSemanticModelBuilder` over the original captured snapshot and binds only unique exact Section ID/`Name`/`UIName` results. Never replace symbolic targets with paths, refresh snapshots, add a parallel parser, persist bindings, or silently choose an ambiguous alias.
- `AGENT-REPAIR-1` adds one conditional non-streaming execution repair, not a transport retry. `Ra2AiBoundedStructuredReplanCoordinator` owns the deny-by-default typed policy, reuses an immutable execution seed, checks context currency before cost, and runs the same proposal/Preview path again. Chat remains one call; normal Work is 2..4 calls, and eligible repair makes the absolute limit 5. Shell may recapture UI-owned context and render the final result only; it must not contain prompt text, auto-Apply, or auto-Save.
- `AGENT-CONTEXT-3-FIX1` preserves query/execution target continuity. An authoring package labeled as a current-document field edit is normalized to the existing project route only when that same package explicitly requests a symbolic `rules` or `art` query. Successful Section facts tell execution to retain the resolved target. The Host never silently retargets a model plan; if project Preview reports `SectionNotFound`, the IDE may use the same captured pair to name the selected file and the unique counterpart containing that Section, then rejects the proposal without Apply/Save.
- `Ra2AiUserMode.Chat` is the safe default and exposes zero authoring tools. `Work` enables admitted current-document routes plus the rules/art project route. Project membership comes from the immutable successful project-open result and `Ra2ProjectDocumentSessionStore.MemberFilePaths`, never from `ProjectExplorer.Items`. Project requests capture the same target paths before and after the model call, then reuse Workspace `PreviewProject/ApplyProject`, the project session owner and compound Undo; mode state remains window/process-local and does not change provider, endpoint or Save authority.
- The production rules/art route exposes `preview_ini_project_edit_plan`, not the fixed project template. DeepSeek owns content semantics and returns bounded operations against symbolic `rules`/`art` targets. The adapter maps those symbols only to captured session documents and creates missing Upsert sections as `SectionKind.Unknown`. It must never accept model paths, snapshot/revision identifiers, raw candidate text, Apply, Save, or asset-write instructions.
- Production current-document capability routes likewise expose `preview_ini_edit_plan` regardless of their historical template/profile ID. DeepSeek owns the complete operation set; the adapter infers Preview-only Section creation for absent Sections referenced by upserts. Legacy typed compilers remain test/headless compatibility code and have no production veto authority.
- `ra2-rules-art-binding` is the source-backed knowledge owner for Techno project bindings. It must distinguish semantic role labels from literal keys and preserve the canonical `rules Owner.Image -> art Section -> body/cameo` graph. Vanilla YR does not treat art-side `Image` as a universal Infantry/Vehicle/Aircraft body rename; the Phobos exception requires established `ArtImageSwap=true`. Do not silently enable that project-wide switch.
- For generic model-owned project plans, Field Registry trust, unknown fields and Diagnostics are advisory review evidence; they cannot block explicit project Apply. Minimum Host safety remains strict: tool/JSON identity, unique properties, safe INI identifiers, document/operation/string limits, canonical parser/Preview, captured snapshot equality, explicit single-use Apply, stale rejection and atomic rollback. The fixed project template and its `OpenReference` policy remain available only for headless compatibility and must not be reintroduced as the production AI content authority.
- `Ra2AuthoringDiffProjectionBuilder` is an internal presentation projection over the successful Preview change set. Keep it cancellable and bounded (8 MiB / 200k input lines / 20k rows / 2k hunks); it must validate the candidate and never own editor or transaction authority.
- `Ra2AuthoringReviewProjectionBuilder` owns only proposal-lifetime review data. Result must remain the exact Preview `CandidateText`; direct context must reuse `Ra2DocumentSemanticModelBuilder` plus `ResolveReference`/`GetSection`, remain depth-one and bounded, and never become Apply or persistence authority.
- Document/session/Field Registry/chat lifecycle changes invalidate both coordinator authority and the visible proposal card. Custom endpoints remain advisory-only.
- Generic transport retry, model fallback, thinking-mode selection, and AI persistence remain out of scope. The single bounded structured-replan exception is governed by `AGENT-REPAIR-1`.
- `CONTENT-2E` adds internal immutable UnitDelivery and GenericWarhead profiles. `Action` is always an explicit argument;
  provider Building and AlwaysGranted are mutually exclusive; effect references resolve against the captured rules snapshot.
  Source-bounded profile fields bypass stale Field Registry Enum membership but still pass bounded identifier/value and canonical
  Preview checks. Other SuperWeapon types remain model-owned generic project plans with Registry/Diagnostics advisory only.
- `CONTENT-2E-FIX2` canonicalizes all admitted SuperWeapon intent metadata to the `superweapon` domain and complete-object
  completion level, including a narrowly recognized mislabeled current-document package that already contains rules queries.
  The intent prompt must turn natural/display object names into candidate canonical IDs and query them. As a defensive fallback,
  `Ra2SuperWeaponProfileCatalog` may normalize only exact, unique aliases found in the captured rules Section/`Name`/`UIName`,
  restricted to the required Building/Techno/Warhead kind. Never add fuzzy matching, hard-coded game object aliases, a second
  parser/index, or persistence/cache authority; ambiguous and missing identities must continue through existing rejection paths.
- SuperWeapon project admission requires one unique rules/rulesmd member; matching art is optional. The only approved Shell change
  is capture wiring through `ResolveRulesWithOptionalArtTargets`. Do not add a second snapshot owner, accept model paths, fabricate
  an Asset Manifest, or make art/assets a prerequisite.

## 10. Maintenance Principles

- Keep Core free of WPF and IDE shell dependencies.
- Keep Infrastructure services reusable by IDE code without depending on UI state.
- Keep IDE controllers and views responsible for UI glue, not schema rules.
- Avoid broad refactors during release stabilization.
- Do not modify BuiltIn field definitions unless the task explicitly targets field metadata.

## 11. Automation Architecture Direction

The HLI-1 Query, Diagnostics, and semantic Edit Preview algorithms are real, tested,
and live in the Core-only `net8.0` Application assembly. They are Experimental
in-process APIs, not a stable Agent SDK or wire protocol.

The governing boundary is documented in
`Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md` and the completed HLI-1 ledgers:

- a candidate `RA2IniEditor.Application` (`net8.0`) assembly;
- UI-neutral document query, diagnostics and semantic Preview capabilities;
- IDE-host ownership of active editor capture, Apply, Undo and Save;
- later Gateway/CLI/Job/Asset consumers using the same canonical implementation.

HLI-0B, HLI-1A/1B/1C, HLI-2A/2B/2C, CONTENT-1, AGENT-MODE-1, AGENT-KNOWLEDGE-1 and the first
CONTENT-2A Techno dual-armament and CONTENT-2B Projectile/Warhead slices are implemented through the documented vertical slices. The dual profile
creates two closed direct-fire chains for one existing owner; it does not claim cyclic fire. CONTENT-2B adds separate
Arcing/Homing Projectile profiles and one YR-core Warhead profile, without claiming Phobos trajectories or Ares custom armor.
CONTENT-2E retains typed Ares UnitDelivery/GenericWarhead profiles for deterministic headless compatibility and source-backed
test fixtures, but production Work now uses the generic model-owned rules/art Project Plan for those capabilities as well.
Capability IDs select Skills and retrieval policy; local Profiles do not veto production model content. This does not certify
game-runtime correctness for every SuperWeapon type. CONTENT-2C AI programming tuples remain frozen by user direction. Do not add
public Apply/Save, CLI/wire DTO, session/permission authority, external/executable Skills, or a second Preview path
without the corresponding contract.

## 12. Documentation Authority

Start at `Docs/README.md`. Product goal, current capabilities, current phase and
roadmap have separate owners. Do not append historical phase narratives to
`Codex_CurrentPhase.md` or `RA2IniEditor_IDE_Full_Codex_Context.md`; preserve detail in
the phase Contract/Stage Ledger and keep the two current-state files concise.
## 13. Retrieval Summary And Lazy Floating Tool Lifecycle

- Retrieval summary rendering stays inside the existing dynamic AI message builder. It consumes
  `Ra2AiAssistantPipelineResult.SemanticRetrieval`, never raw request/response text, and is not conversation context.
- Default-hidden Floating tools must not be passed to `LayoutAnchorable.Float()` during compiled startup topology.
  `ShellDockLayoutCoordinator.ShowAndActivate` is the only materialization path for the first explicit open.
- Do not reintroduce a Dispatcher delay or opacity-only workaround for Search startup visibility.

## 14. Tencent Hunyuan 3D provider adapter

- `RA2IniEditor.AssetProviders.TencentHy3D` is a child-process implementation of the existing internal AssetHost protocol;
  it is not a public SDK and is not wired to UI/Work yet.
- Configure only Windows User variables `RA2INI_HY3D_API_KEY` and
  `RA2INI_HY3D_FREE_ONLY_CONFIRMED=1`; the optional base URL must be the exact official origin.
- Do not add generic API-key fallback, provider HTTP to AssetHost, prompt+image submission, automatic submit retry,
  non-HTTPS artifact access or source-package test artifacts.
- `Ra2ProviderProcessRunner` must continue clearing inherited environment and copy only `SystemRoot`, `WINDIR`, `TEMP`
  and `TMP` before adding its two `DOTNET_*` settings. Do not pass API keys, proxy variables or arbitrary user state to
  provider children; the Tencent adapter reads its dedicated settings from Windows User scope.
- A live run is opt-in and must be preceded by manual Tencent console verification of free balance and disabled postpaid.
- P2 transport certification succeeded on 2026-08-26 after the minimum Windows child-runtime environment fix. Real
  evidence belongs only under excluded `artifacts/`; the provider response omitted credit-consumption fields, so future
  calls still require console verification and explicit authorization.
# ASSET-VOX-1D developer note (2026-08-26)

- GLB-to-voxel conversion is internal to Application and consumes caller-owned bytes; do not add file/process/Host access
  to Application to connect it.
- One conversion produces one declared part. Never infer Body/Turret/Barrel identity from connected components or names.
- `Ra2VoxelSceneSnapshot` remains canonical truth. The mesh snapshot and voxelization facts are transient input evidence.
- Real 499,698-triangle acceptance is sub-second after deterministic packed-edge sorting. Do not restore the dictionary
  edge-incidence implementation; it regressed topology analysis to roughly 214 seconds on the certified mesh.
- Product/UI composition must run through a background cancellable workflow and retain all review flags; 1D does not
  authorize final VXL/HVA or automatic project writes.

# ASSET-VOX-1E developer note (2026-08-27)

- `VOXEL_STYLE.md` is untrusted natural-language authoring input. Only the local compiled plan may drive recolouring;
  never parse prose into per-cell coordinates or let provider output bypass palette/remap validation.
- The IDE owns source resolution, the one-call structured compiler and derived cache. Application remains file/process/AI
  neutral and owns the deterministic plan, mask, colourizer and in-memory review package.
- Keep the exact paint order `WholePart -> Interior -> Side -> Top -> Under -> Edge -> ExplicitMask`. Changing it changes
  golden hashes and requires a versioned contract plus new acceptance artifacts.
- Text-only geometry shading is intentionally coarse. Glass, rubber, accent and remap require explicit mask evidence;
  do not convert an unresolved semantic rule into a guessed geometry rule.
- Review artifacts do not imply project adoption, VXL/HVA, normals or game readiness. UI/Work integration and every
  file-write/adoption path require their own approved contract.
## 2026-08-27 ASSET-VOX-1E-UI composition notes

- `Ra2VoxelStylePreviewCoordinator` is the only UI transaction owner. It reuses the 1E resolver/compiler/cache,
  Application colourizer and path-free review package; do not move colouring logic into WPF.
- `Ra2VoxelStyleWorkspaceViewModel` owns one generation and cancellation source. Source/style changes invalidate late
  results before they can replace the visible preview.
- `Document.VoxelStyle` is a dynamic, non-floating central document. It is intentionally absent from
  `ShellDockToolProfile[]` and is closed before dock-layout serialization.
- The view uses existing IDE workspace/control resources. Direct adoption of core `UiTextBoxStyle` outside its governed
  Shell/Search allowlist is forbidden; use the public IDE-level style resources.
- Session acceptance is not persistence. Adding export, Apply/Save, VXL/HVA handoff or semantic-mask editing requires a
  separate contract and data-model/public-API review.

## 2026-08-27 ASSET-VOX-1E-UI-R2 input and remap notes

### Role colour-source compatibility fix

- The provider prompt now states the Application compiler's exclusive role-colour source invariant explicitly:
  palette index or RGB target, never neither.
- The IDE adapter only removes a redundant RGB value when `Ra2VoxelPaletteProfile` resolves it to the same eligible
  palette entry already named by the proposal. It does not invent a missing colour or choose between conflicting values.
- `Ra2VoxelStylePlanCompiler` retains authority and now reports invalid id, duplicate id, missing source and conflicting
  sources separately. A MagicaVoxel VOX continues to use its embedded RGBA palette; an external PAL is required only for
  Westwood VXL input.

- Both VOX and VXL must converge through `Ra2VoxelSceneSnapshot`; do not add a WPF-local or IDE-local voxel DTO.
- VXL colour truth is the explicitly selected external Westwood PAL. The reserved VXL palette bytes remain non-authoritative.
- The UI accepts one decoded VXL Section only. A future multi-Section selector must operate on the existing reader result;
  it must not change Section order or silently choose `Body`.
- When a palette has no remap indices, only `remap_policy=none` plus entirely `InferredTextOnly` remap rules may be demoted
  into an unresolved assumption. Explicit masks, paintable remap rules and remap interior roles still fail closed.
- Precise local compiler messages are safe fixed strings. Do not expose raw provider payloads, paths or prompt text.

## 2026-08-27 ASSET-VOX-1E-UI-3D notes

- `Ra2VoxelViewportSceneBuilder` is the only WPF mesh adapter. It consumes `Ra2VoxelSurfaceProjector`; never add a second
  neighbour/exposed-face implementation in the IDE.
- `Ra2VoxelViewport3D` owns only camera state and cancellable frozen scene presentation. Snapshot, mask and palette remain
  Application-owned immutable facts.
- Scene replacement is generation guarded. Resource-limit failure routes to the existing SliceStack rather than weakening
  the face budget or blocking the authoring session.
- The two directional lights are readability aids only. Normal palette visualization and game-lighting parity need a new contract.

## 2026-08-27 ASSET-VOX-2A notes

- `Ra2VoxelQualityRefiner` must keep source mesh and direct snapshot immutable. Refinement is a source-hash-bound derived
  candidate and must continue using the existing voxelizer/snapshot/surface/normal authorities.
- Feature survival is an exact coordinate gate. Do not replace it with a comparison of aggregate thin-feature counts.
- Symmetry remains `Off` or review-only `Suggest`; do not add global side copy or silent enforcement.
- `Ra2VoxelRefinementAiCoordinator` owns a hard maximum of three distinct required-tool rounds. Provider retry, parse
  repair or recursive replanning is not implicit and must not be added without a new cost/reliability contract.
- Palette contrast may change only non-exact body shading roles. Semantic materials, remap, rules, scopes, occupancy and
  the input compiled plan remain immutable.
- The headless seam is not product composition. UI, real calls, project adoption and VXL/HVA remain separate gates.

## 2026-08-27 ASSET-VOX-2A-UI notes

- `Ra2VoxelStylePreviewCoordinator` owns GLB admission, source-hash provenance, deterministic option derivation and atomic
  candidate publication. Candidate generation must remain provider-free and file-write-free.
- `Ra2VoxelSceneSnapshot` remains canonical truth. IDE quality/result records and rows are session-only projections.
- Geometry use and styled-result acceptance are separate actions. A new geometry selection invalidates prior style rows and
  acceptance; it never mutates the admitted VOX/VXL baseline.
- Style compilation receives an immutable source-result copy whose snapshot is the selected working geometry; source-pack
  lookup continues to use the admitted source path.
- Palette contrast is optional. Failure or a no-op contrast plan cannot invalidate an otherwise valid ordinary style result.
- Do not promote `VoxelStyle.Quality.*` identities into a persistence/plugin contract without a public API review.

## ASSET-VOX-2B implementation notes

- `Ra2VoxelSymmetryEvidencePackage` and `Ra2VoxelSemanticPartition` are immutable derived state bound to canonical hashes;
  they must not be serialized as project authority.
- Model-facing `core-*` regions are already mirror-matched context. Unmatched occupied-side groups must remain neutral
  `repair-*` questions and expose mirror-target coverage/contact facts; do not encode `attachment` or another semantic
  conclusion into a Host-generated region ID.
- The semantic compiler is intentionally text-only and performs exactly two required-tool calls. The critic receives a
  compact normalized first round; unknown or invented region IDs fail before geometry execution.
- Round two is an independent verifier, not an instruction to disagree. Supported first-round decisions remain valid;
  actual disagreement still reconciles to `Uncertain` and must not be bypassed by a Host fallback.
- `Ra2VoxelSemanticSymmetryExecutor` alone owns voxel changes. It preserves every non-core coordinate and a transition seam,
  uses supersampled coverage for pair direction and retains connectivity/cavity/volume/silhouette/support/roughness gates.
- Structure classification and material classification are different data domains. A future colouring stage may consume the
  partition as supporting evidence but must not equate structural core with one palette/material role.
- Do not restore one-region-per-connected-component evidence. Real rasterized vehicle surfaces can contain more than 64
  disconnected mismatch islands. Keep exact coordinates in Host memory and use the canonical side/height/depth/morphology
  grouping; `ConnectedComponentCount` makes the aggregation explicit to the compiler and package hash.
- `Ra2VoxelQualityPreviewResult.SymmetryEvidenceResult` retains typed local evidence failure diagnostics. Presentation may
  localize them, but must not collapse them back to an unexplained disabled button.
- The semantic compiler is strict about evidence identity, not incidental JSON spelling. Keep hash, plane, known region ID
  and complete coverage checks; do not restore exact-property-set rejection for optional provider metadata or equivalent
  camelCase/string-number representations.
- Difference blue must not be used for local frozen coordinates. It falsely looks like AI symmetry evidence. Pre-AI
  difference is green/red/translucent-grey; semantic blue is reserved for protected thin features and may be asymmetric.
- Candidate safety and smoothing value are separate facts. `ValidateCandidate` retains structural authority, while
  automatic Refined selection additionally requires material roughness improvement and ranks roughness before low-support
  count. Do not reintroduce cleanup-first selection.
- Candidate derivation hashes must include behavior kind and every threshold that can change occupancy. Candidate reviews
  are derived, nonserialized session facts.
- Difference scene construction consumes candidate + same-grid comparison only. A protection mask is not read by the
  Difference renderer and must not be introduced as an admission dependency.
- Provider representation normalization may accept equivalent wrappers and aliases, but duplicate aliases, missing regions,
  unknown IDs, stale hash/plane and incomplete coverage must retain distinct fail-closed diagnostics.

## ASSET-VOX-2C implementation notes

- `Ra2VoxelGeometryProposal` is internal, immutable, derived session state. Do not serialize it or promote target IDs into
  a public plugin/persistence contract.
- The primary pass may request one coordinate-free component slice. Review cannot query; arbitration is conditional on the
  normalized executable fingerprint and cannot query. Keep the absolute provider-call ceiling at four.
- Fingerprint equality ignores order, reason and confidence but never ignores target ID or action. Third-round transport,
  timeout and cancellation remain their original typed failures rather than being mislabeled as a semantic arbitration.
- `Ra2VoxelAgentGeometryProposalExecutor` expands against the immutable source occupancy. Do not let an earlier operation
  alter whether a later target originally had a mirror; that would create order-dependent cascading edits.
- `Ra2VoxelSymmetryEvidencePackage.CenterSeamGaps` owns bounded missing-coordinate targets for exact one/two-cell X-center
  seams. `bridge_center_gap` is the only compatible action; ordinary occupied targets cannot use it, and seam targets cannot
  use add/remove. Keep exact coordinates Host-owned and nonserialized.
- Center-seam execution must reuse `Ra2VoxelQualityRefiner.ResolveAddedPaletteIndex` and the existing minimum safety gates.
  Do not convert the evidence builder into automatic fill or widen it to arbitrary/off-axis/three-cell holes.
- Host gates are minimum physical safety, not semantic authority. Do not restore coverage-direction, roughness,
  low-support or complete-classification vetoes to this executor.
- Structure projection must colour only exact selected target coordinates. Omitted coordinates remain protected/uncertain,
  and the candidate view compares the final Agent candidate with the Refined baseline using real geometry delta colours.

## ASSET-VOX-3A generation boundary

The IDE consumes `RA2IniEditor.AssetHost` only through `Ra2MeshGenerationFacade`. The provider bundle is fixed at build
time and revalidated at runtime. Generated GLB/preview bytes are copied before the Host lease is disposed. The IDE then
converts the GLB in memory and represents it as a generated-session source; do not replace this with a fake project path
or add project writes to the façade. Live provider probes require a separate approval and manual acceptance record.

## ASSET-VOX-3B accepted-candidate/export boundary

- `Ra2VoxelAcceptedCandidate` is immutable IDE-internal session authority. Never reconstruct it from the currently visible
  review mode at export time and never serialize it as project state.
- Only canonical snapshot-bearing modes may be frozen. Difference, semantic overlays, masks and palette swatches are
  projections and must never cross the materialization boundary.
- `Ra2VoxelVoxExportService` is the only IDE export transaction. It must reuse `Ra2MagicaVoxelCodec`, perform a same-folder
  physical flush and byte-exact decode/re-encode gate, and publish only through an atomic move/replace.
- Do not relax source-overwrite rejection or merge this Save-As path into INI/project Apply/Save. Multi-part assembly,
  manifests and VXL/HVA need separate authorities and contracts.
## ASSET-VOX-4A semantic masking boundary

- `Ra2VoxelSemanticEvidencePackage` is internal derived state bound to the current canonical working hash. Region IDs and
  masks are session evidence, not a public plugin or persistence contract.
- DeepSeek receives `ToPromptText` only. Do not add render/image/file bytes or palette colours to this compiler unless a
  separately approved multimodal provider contract exists.
- Keep two analysis passes and conditional third arbitration. Equality is based on region/part/material/remap suggestion,
  not wording or confidence.
- `Ra2VoxelSemanticLayerResolver` owns `HumanOverride > AgentSuggestion > Unknown`. Never let re-analysis overwrite same-hash
  human state or let AI return `ExplicitlyApproved` remap.
- Material execution must continue through `Ra2VoxelSemanticStyleIntegrator` -> `Ra2VoxelExplicitMask` -> existing
  `Ra2VoxelColourizer`. Missing style roles remain unresolved; do not guess palette indices.
- Semantic 3D colours and click hit-testing are presentation aids. They never make a region classification authoritative or materializable by themselves.

## ASSET-VOX-4B manual semantic mask boundary

- `Ra2VoxelSemanticManualMaskLayer` is immutable, sparse, session-only and bound to the current canonical cell ordering.
  Do not serialize it or key it by screen coordinates.
- `Ra2VoxelSemanticMaskComposer` owns the final per-cell precedence. Keep cell human overrides above region human/Agent
  seeds, and keep Unknown executable as a no-mask result.
- The surface brush uses existing occupied coordinates and six-neighbour surface traversal. A mirrored counterpart is added
  only when it exists; the entire click is one undo item.
- Do not write palette indices from the editor. Final composition must continue through `Ra2VoxelSemanticStyleIntegrator`,
  `Ra2VoxelExplicitMask` and `Ra2VoxelColourizer`.
- The 3D control emits canonical cell coordinates directly on left-button down. Left input never owns camera movement;
  right drag owns orbit, Shift+right/middle own pan and the wheel owns zoom.
- `Ra2VoxelViewportSceneBuilder` must keep each colour-batched `GeometryModel3D` paired with one coordinate per emitted
  quad. Resolve WPF hit triangles by model identity and the quad's four-vertex ordinal; never restore nearest-centre guessing.
- Hit metadata is derived, scene-lifetime and nonserialized. Replace model, snapshot, semantic evidence, coordinate index and
  hit map as one scene generation, and reject any missing/cross-face/stale mapping with readable feedback.
- Continuous stroke input must remain split by authority: the viewport owns capture, <=4-DIP exact-hit sampling and the
  temporary seed overlay; the ViewModel owns one frozen base-layer transaction; `Ra2VoxelSemanticMaskEditor` remains the
  only footprint/mirror/Paint/Erase executor. Never invoke the click transaction repeatedly from `MouseMove`.
- A stroke commits once and creates at most one undo record. Scene/hash/mode/capture/camera transitions cancel without
  mutation. Keep the 4096 samples-per-move and 8192 unique-seed limits fail-closed and non-truncating.
- Part/Material review palettes and legends are IDE presentation state only. Do not serialize them, feed them into semantic
  precedence, or translate their RGB values into VOX palette indices.
## ASSET-VOX-4D persistence boundary

`Ra2VoxelSemanticSidecarStore` is IDE-internal and serializes `ra2-voxel-semantic-sidecar` version 1. It reuses
`AtomicTextFileWriter`, rejects paths outside the active project or through reparse points, and performs complete temporary
validation before ViewModel publication. The three authoring layers remain separate. Do not fold this sidecar into project
Save/Apply, model writers, provider DTOs or public C# APIs without a separately approved compatibility stage.

## ASSET-VOX-4E Rev.6 directional/boundary boundary

- `Ra2VoxelColourizer.BuildGeometryMask` is the sole owner of longitudinal/lateral surface derivation. Keep it bound to the
  canonical occupied ordering; do not persist orientation bits or create an IDE-side geometry classifier.
- Exclusive primary surfaces use Top → LongitudinalEnd → Side → Under. Side-plus-under must remain the side/body family;
  changing this precedence requires a new colour-quality contract and real-model regression.
- `Ra2VoxelSemanticBoundaryProjector` owns runtime-only effective PartRole/MaterialRole interfaces. It ignores RegionId,
  selects only PaintedSurface ownership and relies on later direct/remap rules for exact protection.
- Technique differentiation belongs to typed revision-2 policy and local materialization. Manual base colour and technique
  remain outside the Provider request; never widen its JSON or ask for coordinates to reproduce local masks.
- Part/Material review buttons are global presentation controls. They may select Semantics 3D but must not change workflow
  stage, authoring state, persistence or materializable candidate identity.
