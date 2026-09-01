# RA2IniEditor.IDE Release Checklist

Use this checklist before publishing or handing off an IDE-only source package.

## 1. Command Validation

- [ ] Run `dotnet restore .\RA2IniEditor.IDE.sln`.
- [ ] Run `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`.
- [ ] Run `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build`.
- [ ] Run `powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly`.
- [ ] Confirm the package is `artifacts\RA2IniEditor.IDE.SourceClean.zip`.
- [ ] Confirm the package does not contain legacy root `RA2IniEditor.sln` or `RA2IniEditor.csproj`.

## 2. IDE Launch Smoke Test

- [ ] Launch RA2IniEditor.IDE.
- [ ] Confirm the shell opens without startup exceptions.
- [ ] Confirm logs are created only in expected local log locations.
- [ ] Confirm theme and layout are usable enough for manual inspection.

## 3. Project Open Smoke Test

- [ ] Open a small sample INI folder or entry INI file.
- [ ] With `.ini` associated to the built IDE executable, open a top-level INI from
  Explorer and confirm its direct parent becomes the project and that exact file is
  selected, editable, and shown in the Source Editor.
- [ ] Confirm a missing/non-INI startup target reports an error without crashing; note
  that each Explorer launch currently starts a separate process.
- [ ] Confirm Project Explorer lists expected files or sections.
- [ ] Confirm Source Editor loads text.
- [ ] Confirm section navigation can move between loaded sections.
- [ ] Confirm no legacy table editor workflow is required for basic inspection.

## 4. Source Editor Smoke Test

- [ ] Edit a simple key/value line in the Source Editor.
- [ ] Use undo / redo for the edit.
- [ ] Confirm the editor does not lose text or caret context during basic navigation.
- [ ] Confirm dirty-state prompts appear when navigating away from unsaved edits.

### Search / Replace

- [ ] Search the whole project and confirm results show file, line, Section, and preview.
- [ ] Navigate to another result file and confirm dirty-navigation protection still applies.
- [ ] Select Current File, preview Replace All, and confirm preview does not alter the editor.
- [ ] Apply Replace All and confirm the document becomes dirty without changing the source file on disk.
- [ ] Confirm one Ctrl+Z/Ctrl+Y fully undoes/redoes the replacement batch.
- [ ] Confirm editing after preview makes the old replacement plan unusable.

## 5. Language Assistance Smoke Test

- [ ] Trigger completion in a known section.
- [ ] Confirm completion can commit a selected suggestion.
- [ ] Confirm Projectile completion offers canonical `AA` / `AG`, while Vehicle/Techno field-name completion does not offer their diagnostic guardrails.
- [ ] Hover a known field and confirm field details appear.
- [ ] Manually type Vehicle `AA=yes` in a disposable sample and confirm Hover / Quick Peek / Diagnostics still expose its wrong-context guardrail.
- [ ] Hover a recognized reference value and confirm reference context appears when available.
- [ ] Use Quick Peek / definition details for a recognized reference.
- [ ] Use Find References on a known section or value.

## 6. Diagnostics And Issues Smoke Test

- [ ] Confirm Issues / Diagnostics can display current project results.
- [ ] Introduce a harmless temporary issue in a disposable sample and confirm diagnostics update.
- [ ] Remove the temporary issue and confirm diagnostics clear or update.
- [ ] Confirm diagnostics are informational where appropriate and do not imply every mod-specific warning is fatal.

## 7. Save Preflight Smoke Test

- [ ] Save a disposable sample edit.
- [ ] Confirm save preflight prompts appear when expected.
- [ ] Confirm save completion does not corrupt INI formatting.
- [ ] Reopen the file and confirm the saved source text is readable.

## 8. Backup / Rollback Smoke Test

- [ ] Confirm save or registry workflows that create backups write them to expected locations.
- [ ] Confirm backup metadata is understandable.
- [ ] Confirm rollback paths are explicit and do not run silently.

## 9. Field Registry Smoke Test

- [ ] Open Field Registry Manager.
- [ ] Confirm Project, Global, and BuiltIn status is understandable.
- [ ] Reload local field registry metadata.
- [ ] Open field learning / import preview on a disposable input.
- [ ] Review parsed fields, validation issues, and target scope before applying.
- [ ] Confirm apply / rollback workflows remain explicit and previewable.

## 10. Package Boundary Check

- [ ] Confirm the IDE-only package includes `RA2IniEditor.IDE.sln`.
- [ ] Confirm the IDE-only package includes `RA2IniEditor.Core`, `RA2IniEditor.Infrastructure`, `RA2IniEditor.IDE`, `RA2IniEditor.Tests`, and `RA2IniEditor.UiAutomationTests`.
- [ ] Confirm BuiltIn v3.2 field registry assets are present.
- [ ] Confirm BuiltIn v3.2 has no uniform inferred-template descriptions, `auto-extracted` rows, empty quality labels, unrecognized trust labels, or duplicate key + appliesTo identities.
- [ ] Confirm `Docs/` and `tools/` are present.
- [ ] Confirm generated folders such as `bin`, `obj`, `artifacts`, `.vs`, `TestResults`, and coverage output are excluded.

## 11. AI Assistant Smoke Test

- [ ] Confirm the AI panel defaults to DeepSeek V4 Flash.
- [ ] Confirm the model list contains only DeepSeek V4 Flash and DeepSeek V4 Pro.
- [ ] Confirm the mode selector defaults to Chat, both Chat and Work are keyboard accessible, and switching modes updates the compact summary.
- [ ] Start the IDE without opening Search and confirm neither `查找引用` nor `查找` appears transiently in the bottom tool well and no Search floating host flashes; then invoke Search and confirm the independent floating Dock appears and accepts focus.
- [ ] In Chat mode, send an edit-like request and confirm no structured editing tool or proposal is offered.
- [ ] In Work mode, request a complete object outside the legacy typed profiles and confirm it reaches the generic structured-plan tool or returns one concrete DeepSeek clarification; it must not be rejected by a local content whitelist or silently reduced to a skeleton.
- [ ] Confirm configuration status and the network/cost/no-file-mutation notice are visible without exposing endpoint or API key values.
- [ ] Confirm an over-8000-character prompt is rejected before a request starts and remains in the input box.
- [ ] Confirm cancellation/failure preserves received text and does not add the failed pair to later conversation context.
- [ ] Confirm an ambiguous edit-like prompt is preserved and clarified locally without starting an authoring request.
- [ ] With the official endpoint and a disposable editable INI, request one field change and confirm an inline proposal appears without changing editor text.
- [ ] Confirm the card shows operation evidence and old/new values; Dismiss must leave the document unchanged.
- [ ] Generate again, click Apply, and confirm only the in-memory current document changes, the document becomes dirty, Ctrl+Z undoes the full proposal, and no automatic save occurs.
- [ ] Edit the document after proposal generation and confirm the old proposal becomes unusable.
- [ ] Confirm provider prose without the required tool call produces a typed failure and no proposal card.
- [ ] Confirm mixed provider prose cannot alter the operations shown by the locally validated proposal card.
- [ ] Confirm a proposal that adds diagnostics is visibly marked for review but remains explicitly applicable; malformed tool JSON, unsafe identifiers, stale snapshots and custom endpoints still cannot cross the Work authority boundary.
- [ ] Confirm valid document and project proposals still open Preview when `summary`/`message` are missing, blank, null or provider-shaped metadata; confirm clarification without a readable message still fails and echoed operations remain inert.
- [ ] Confirm a successful proposal opens a read-only main-workspace Diff; closing it keeps the proposal and `查看更改` reopens it.
- [ ] Confirm a successful proposal opens in `结果`, shows the complete candidate file with line numbers/highlighting, selects the first changed Section, and does not permit editing or saving.
- [ ] Switch to `差异` and confirm removed lines and old/new line numbers remain visible; switch to `对象上下文` and confirm directly referenced Sections are labeled `未修改，仅供审阅`.
- [ ] For a rules/art proposal, confirm changed-file tabs preserve Preview order and cross-document related context never invents or opens another path.
- [ ] At 899 DIP and 639 DIP, confirm the local outline collapses/overlays while mode selection and Apply/Dismiss remain reachable; verify Ctrl+1/2/3 and F7/Shift+F7.
- [ ] Confirm `应用全部` returns focus to the source editor, creates one Undo unit, and does not save; `放弃修改` cannot be reopened.
- [ ] Request a current-file Weapon/Projectile/Warhead skeleton with disposable IDs and confirm the provider-visible tool is `preview_ini_edit_plan`, missing Sections appear in Diff, and no file is changed before Apply.
- [ ] Request a usable complete direct-fire weapon chain for an existing owner and confirm DeepSeek supplies the complete operation set rather than a local 15-field Profile or an implicit skeleton.
- [ ] Request Arcing, homing, Phobos Projectile and YR/Ares Warhead variants and confirm local Profile membership does not veto the proposal; review semantic compatibility and every selected value in Diff.
- [ ] With one unique rules/art pair, request `给 HTNK 绑定美术：Art=HTNKART，Body=HTNKBODY，Cameo=HTNKICON。` and confirm a structured Project Proposal appears even when those values are absent from learned Enum samples.
- [ ] Confirm the rules/art project proposal may create a missing Section and use a mod-specific field while showing Registry/Diagnostics as advisory review evidence rather than blocking explicit Apply.
- [ ] Confirm Project Diff orders rules before art; apply once, verify both documents become dirty without an automatic save, then verify one Ctrl+Z/Ctrl+Y undoes/redoes both together.
- [ ] Confirm missing SHP files do not block the INI proposal or Apply and no asset file is created.
- [ ] Confirm the clean output contains exactly 18 bundled `AgentSkills/*/SKILL.md` packages and no bundled Skill `scripts/` directory.
- [ ] With the official endpoint, run one successful Work request and confirm the intent tool accepts `selected_skill_ids` / `knowledge_gaps`, the execution request receives the capability-required Skill, and the normal path makes no third provider call.
- [ ] With one rules/art pair, ask a Work request that names existing objects by local `Name`/`UIName`; confirm the Host resolves unique canonical Sections, performs no more than two compact refinement rounds, and stops repeated queries without another round.
- [ ] After a Work request performs project retrieval, confirm one muted `项目检索` line appears before the proposal card; Chat, no-retrieval Work, clarification and provider failure must not show it.
- [ ] Confirm normal Work uses 2..4 provider calls depending on retrieval need, and an eligible structured repair never raises the absolute total above five.
- [ ] Open a project whose art file contains `[HTNKART]` with `Image=HTNKBODY` and `Cameo=HTNKICON`, then ask Work to verify those values and preview `Remapable=yes` without changing rules or saving. Confirm the proposal targets art; if a deliberately malformed plan targets rules, confirm the failure names both files and applies nothing.
- [ ] Induce one allowlisted structured-plan error and confirm Work performs at most one non-streaming correction, displays only the final proposal plus `已自动修正 1 次`, and still requires explicit Apply.
- [ ] Repeat with timeout/network cancellation and confirm no correction call occurs; repeat with an invalid correction and confirm the request stops after three total calls, restores the prompt, and creates no proposal.
- [ ] Confirm a context query can target only `current`, `rules`, or `art`; a path-like query is dropped, never reads another file, and does not destroy otherwise valid Work intent.
- [ ] Confirm the rules/art second-stage prompt contains `ra2-rules-art-binding` even when intent analysis reports `domain_intent_id=techno` or normalizes it to `art-animation`.
- [ ] Confirm `Art=... Body=... Cameo=...` is never expanded into literal rules `Art/Body/Cameo` keys; expect a correct rules/art graph or a concrete object-family/`ArtImageSwap` clarification.
- [ ] With only one unique rules file in the opened project, create an Ares UnitDelivery proposal using existing TechnoTypes. Confirm the second provider call uses `preview_ini_project_edit_plan`, registration/provider/common/effect fields appear in a rules-only Project Diff, Apply remains explicit and one Undo restores it.
- [ ] Create an Ares GenericWarhead proposal using one existing Warhead. Confirm the Warhead Section is not modified or duplicated, no art/asset prerequisite appears, and no automatic save occurs.
- [ ] Request another explicit SuperWeapon type and confirm it uses the generic reviewed Project Plan or asks one concrete clarification; a stale Field Registry Enum sample must not block it.
- [ ] At narrow width, confirm Diff actions remain available and the return-to-source action becomes a compact icon with a tooltip.
- [ ] Confirm no generic transport retry or model fallback occurs; only the documented single bounded structured-replan exception is permitted.

## 12. Release Notes

- [ ] State that this is an IDE-only package.
- [ ] State that the legacy table-style editor is intentionally absent.
- [ ] List any known limitations around diagnostics, reference resolution, or opt-in UI automation.
## ASSET-VOX-1E-UI preview workspace

- [x] Opening the workspace and loading a VOX or VXL/PAL pair are provider-free.
- [x] Source path is project-contained, bounded and read-only.
- [x] VXL requires an explicit 768-byte PAL, reuses the canonical reader and rejects implicit multi-Section selection.
- [x] Empty-remap VOX palettes keep text-only team-colour intent unresolved without blocking ordinary shading.
- [x] Compile is explicit, cancellable, single-generation and has no automatic retry.
- [x] Original/result/region/palette projections and plan/review facts are present.
- [x] The former session-only acceptance boundary is superseded by ASSET-VOX-3B: only an explicitly frozen materializable
  candidate can use the separate verified VOX Save-As; project Apply/Save and VXL/HVA remain absent.
- [x] Dynamic document is absent from dock profiles and closed before layout persistence.
- [x] Automated build and unit/contract gates pass.
- [ ] Physical 1920x1080 screenshot/manual interaction acceptance.
- [ ] Explicitly authorized real DeepSeek style compile smoke.

## ASSET-VOX-1E-UI-3D interactive viewport

- [x] Original/result/region use the canonical visible-face projection; Palette remains 2D.
- [x] Scene generation is bounded, cancellable, generation guarded and falls back to the existing SliceStack.
- [x] No HelixToolkit/new dependency, Shell layout change, writer or project-write authority was added.
- [x] Orbit/pan/zoom/reset controls and new AutomationIds are present; STA XAML construction passes.
- [ ] At 1920x1080, manually verify initial fit, left orbit, middle/Shift pan, wheel zoom, reset and SliceStack return.
- [ ] Confirm region colours are legible and the geometry-only lighting notice is not clipped at narrow width.

## ASSET-VOX-2A-UI candidate composition

- [x] Project-contained GLB admission is bounded, reparse-safe and provider-free.
- [x] Verified/UserPaired/Mismatch provenance is visible and mismatch publishes no candidates.
- [x] Direct/Refined/optional Symmetry are immutable and require explicit session selection.
- [x] Selected geometry is consumed by the existing explicit style compile.
- [x] Ordinary Styled remains valid when optional Contrast is identical or unavailable.
- [x] Quality metrics, normal comparison, semantic provenance and palette contrast use non-DataGrid IDE surfaces.
- [x] Application 264/264, IDE 2814/2814 and AssetHost 47/47 pass; solution build has 0 errors.
- [x] No real DeepSeek/Tencent call, Shell/layout, Apply/Save, VXL/HVA or public API change.
- [ ] At 1920x1080, manually verify wrapped commands, all candidate switches and `用于本会话` then Styled/Contrast review.
- [x] Automatic Refined selection requires measurable roughness improvement; cleanup-only candidates remain review-only.
- [x] `组合审阅` exposes all candidate delta/quality facts and Difference works without a protection-mask dependency.
- [x] Malformed semantic responses and evidence mismatches retain concrete localized diagnostics.
- [ ] Repeat the certified Body sample in the rebuilt UI and confirm Conservative is selected while SurfacePolish is review-only.
- [ ] Run one explicitly authorized live DeepSeek structure-recognition smoke and record the exact diagnostic or success.

## ASSET-VOX-3A manual release checks

- [ ] Provider bundle contains exe/dll/deps/runtimeconfig plus a matching SHA-256 manifest.
- [ ] Preparing generation does not send the reference image.
- [ ] Consent dialog clearly states one image, one job, no retry and unknown free balance.
- [ ] Cancel/timeout leaves no project or asset file changes.
- [ ] Successful output appears as a session-only generated source and can enter local quality review.

## ASSET-VOX-3B accepted candidate and VOX export

- [x] Original/Direct/Refined/Agent Geometry/Styled/Contrast are the only materializable candidate kinds.
- [x] Difference/Structure/Mask/Palette cannot be frozen or exported.
- [x] Review navigation preserves the frozen snapshot; source/geometry/style changes invalidate it.
- [x] Export reuses the canonical codec and requires byte-exact temporary-file round-trip before atomic publish.
- [x] The current source VOX cannot be overwritten; another target requires native overwrite confirmation.
- [x] Cancellation/failure leaves no new target or temporary file in automated tests.
- [ ] Manually freeze an Original candidate, export to a new filename and reopen it in the workspace.
- [ ] Manually confirm choosing the current source filename is rejected and its bytes remain unchanged.
- [ ] Manually confirm no project Apply/Save, asset registration or VXL/HVA action occurs.

## ASSET-VOX-4B-STROKE-1 continuous semantic painting

- [x] Multi-seed stroke execution reuses the single Application mask editor and is atomic/order-independent.
- [x] One successful stroke creates one undo item; cancel/failure/no-change leaves layer and history unchanged.
- [x] Pointer sampling is exact visible-surface hit-map based, <=4 DIP, deduplicated and resource bounded.
- [x] Temporary Paint/Erase overlays do not rebuild the formal scene or write semantic/palette state during drag.
- [x] Part/Material review colours and all 16 fixed mappings have automated coverage.
- [x] Application 302/302, IDE 2885/2885 and AssetHost 50/50 pass; Debug build has 0 errors.
- [ ] Manually verify click, slow/fast drag, background gap, size 3, mirror, erase and one-step undo in a fresh process.
- [ ] Manually verify right-button cancellation/orbit and Part/Material legend consistency at 100% and 125% DPI.

## ASSET-VOX-4E Rev.6 colouring and classification preview

- [x] Direction masks distinguish longitudinal ends from lateral sides and keep side-plus-under cells out of Underside.
- [x] Effective semantic boundaries ignore RegionId-only seams and never overwrite direct materials/remap.
- [x] Five revision-2 techniques have distinct typed policies and produce five distinct RA2 indexed-ramp fixture candidate hashes.
- [x] The global preview toolbar preserves Part/Material review AutomationIds and switches either entry to Semantics 3D
  without changing workflow stage or semantic composition.
- [x] Debug build and focused VoxelColour 55/55 plus VoxelStyle workspace 28/28 pass.
- [ ] On a real ground vehicle, confirm long side black strips are gone and front/rear direction remains recognizable.
- [ ] Compare at least three techniques on the same base colour and confirm visibly different, coherent results.
- [ ] Verify the global Part/Material classification preview, legend, 3D return and 100%/125% DPI layout manually.

## ASSET-VOX-4E Rev.7 form-zone and game-scale preview

- [x] Human ForwardDirection is snapshot/composition-bound; Unknown does not guess front/rear.
- [x] FormZone, BoundaryIntent, material-local families and Macro/Meso/Micro/SubPixelRisk are derived and hash-bound.
- [x] Five revision-3 techniques have distinct typed spatial/boundary/detail policies.
- [x] Fixed eight-view game-scale facts and explicit NormalContext/VplNotEvaluated quality facts are present.
- [x] Section-16 AutomationIds are unique; no Shell, persistence, Provider protocol or writer changes.
- [x] Focused Application 65/65 and IDE/Skill/UI 112/112 pass.
- [x] Rev.7-G gate: WPF test-owned Application lifecycle fixed without product theme/Shell changes；restore passed；
  build 0 warning/error；Application 368/368；AssetHost 50/50；IDE 2922/2922.
- [x] IdeOnly clean package created with 1470 entries；forbidden directory/archive scan found 0 violations.
- [ ] Manually verify one ground, one air and one large-surface model, game-scale readability, five-technique distinction,
  diagnostic entries, no Slice switch, and 100%/125% DPI. User owns this acceptance; do not use computer control.
