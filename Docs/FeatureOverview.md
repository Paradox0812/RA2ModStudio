# RA2IniEditor.IDE Feature Overview

This document describes features available in the current IDE. It does not describe
the entire long-term Agent roadmap. For the accepted natural-language INI + asset
production goal and an explicit implemented/planned split, see:

- `Docs/ProductVisionAndRequirements.md`
- `Docs/CurrentCapabilities.md`

## 1. Product Positioning

RA2IniEditor.IDE is the current IDE-only package for editing Red Alert 2 / Yuri's Revenge INI projects. It focuses on source-first editing, navigation, field intelligence, diagnostics, and safer save workflows.

The legacy table-style editor is intentionally not included in this package. The current build entry is `RA2IniEditor.IDE.sln`.

## 2. Source-First INI Editing

- Edit INI text directly in the Source Editor.
- Preserve a text-oriented workflow suitable for large `rules.ini`, `rulesmd.ini`, and split INI projects.
- Use project and file context to keep navigation, diagnostics, completion, and save behavior aligned with the current source buffer.
- A Windows file-association launch with one existing `.ini` path opens its direct
  parent as the project and activates that exact file through the normal editable
  session pipeline. Single-instance forwarding is not part of the current stage.

## 3. Project Explorer And Navigation

- Browse project files and sections from the IDE shell.
- Navigate between sections without relying on the old object table workflow.
- Keep the Source Editor as the primary editing surface.

### 3.1 Dockable Workspace

- Project Explorer and AI Assistant share the right tool area. Problems and Output occupy the default bottom tool area; Find All References is on demand. Search opens independently as a hidden-by-default floating tool whose native host is created only after an explicit Search command.
- Tool tabs can be resized, floated, re-docked, hidden, and reopened without recreating their content state.
- Closing Search hides rather than destroys its managed content; reopening preserves its valid dock/floating location, and a minimized floating host is restored by the Search command.
- The toolbar and `View > Window Layout` provide commands to return floating tools and restore the complete default layout.
- Valid presentation layout persists through `shell-layout.v2.xml`; Reset restores and immediately persists the compiled default.
- Shell, Dock and secondary windows inherit one Chinese-capable UI font authority, while editor/code surfaces retain an explicit monospace font.

### 3.2 Project Search And Current-File Replace

- Search scans the files already present in Project Explorer; it does not recursively discover extra files.
- Search supports the whole project or current file, case sensitivity, whole-word matching, and bounded .NET regular expressions.
- The current file is searched from the in-memory editor buffer, so unsaved edits are visible in results.
- Results include file, line, column, Section, and preview text, and navigate through the existing dirty-navigation guard.
- Replace All is deliberately limited to the current file. It requires a preview, rejects stale previews, changes only the in-memory session, and is one Undo/Redo transaction.
- Search/Replace never saves automatically; the normal Save Preflight, backup, encoding, and rollback path remains authoritative.

## 4. Field Intelligence

- Completion helps insert known RA2 / YR / Ares / Phobos field names and values where available.
- Hover surfaces field details near the source text.
- The field registry uses a priority model of Project, Global, then BuiltIn definitions.
- The BuiltIn v3.2 fallback field library keeps common field metadata available even when no local registry exists.
- BuiltIn v3.2 no longer exposes unreviewed uniform-template or auto-extracted rows at runtime. Diagnostic guardrails remain available to Hover, Quick Peek, and Diagnostics, but are intentionally omitted from field-name Completion.

## 5. Reference Understanding

- Reference Value Hover explains recognized references where the current context can resolve them.
- Quick Peek and definition details help inspect referenced sections without switching back to an old table editor.
- Find References supports source-oriented reference inspection.

## 6. Diagnostics And Save Preflight

- Issues / Diagnostics collect parse, validation, and project understanding results.
- Save Preflight is intended to make risky saves visible before writing changes.
- Diagnostics should be treated as assistance, not as a replacement for mod author review; RA2-family INI projects often contain soft references and mod-specific extensions.

## 7. Field Registry Workflow

- Field Registry Manager exposes local field registry status and reload workflows.
- Field learning / import preview supports reviewing parsed fields before applying changes.
- Registry behavior should stay conservative: import, apply, and rollback flows must remain explicit and reviewable.

## 8. Backup / Rollback Safety

- Save and field registry workflows are expected to prefer explicit backup and rollback paths when writing project or registry files.
- Backup / rollback is a safety layer, not a replacement for version control or a full project copy before large edits.

## 9. AI Assistant

- The production AI path uses DeepSeek V4 Flash or DeepSeek V4 Pro; V4 Flash is the default.
- The panel has explicit Chat and Work modes. Chat is the safe default and exposes no editing tool; Work is required for structured current-document or rules/art project proposals.
- Sending is always explicit and can incur network usage and provider cost.
- Only bounded current-editor context, evidence, diagnostics summaries, and eligible recent conversation turns can enter a request; outbound text is sanitized before transmission.
- Prompts longer than 8000 characters are rejected before a request starts and remain available for editing.
- Streaming cancellation, timeout, provider failures, and incomplete output preserve received text while keeping failed turns out of future conversation context.
- On the official endpoint, an editable current document can expose one bounded preview-only structured-edit tool. A returned proposal is validated locally, previewed against the exact request snapshot, and shown as an inline review card.
- An explicit skeleton request can use `weapon-projectile-warhead-skeleton` v1. An ordinary request to build a usable direct-fire weapon chain uses the reviewed single-slot complete profile. A request for complete Primary and Secondary armaments can use a separate profile that creates two closed chains in one 30-operation proposal. Neither profile adds type-list registration or assets, and dual slots are not presented as cyclic or alternating fire.
- Work mode can also bind an existing Weapon to a new original-game Arcing or ROT-homing Projectile, or to a YR-core Warhead. These are separate source-gated profiles: trajectory families are never mixed, unsupported Phobos/Vertical/Airburst variants fail locally, and the YR Warhead profile refuses documents with `[ArmorTypes]` rather than pretending to cover Ares custom armor.
- Work mode can construct model-owned edits across one unique rules/art pair. The context summary displays the authoritative full project root and pair readiness; an empty art INI and unrelated top-level INI files are accepted. DeepSeek chooses the required Sections, fields, values, registration relationships and rules/art bindings, or returns a clarification when an indispensable target is unknown. Field Registry and Diagnostics remain visible advisory evidence but do not veto the project proposal. The proposal displays the actual changed files in Project Diff; explicit Apply is one atomic in-memory project transaction with compound Undo/Redo and no automatic Save or asset creation.
- Eighteen bundled RA2 domain Skills provide selectively loaded authoring guidance for RA2/YR/Ares/Phobos. In Work mode, the intent call sees a compact metadata-only Skill manifest and recommends relevant IDs; the Host validates that list, adds capability-required guidance, enforces the body budget, and gives the resolved bodies to the execution call. Project and SuperWeapon capabilities therefore cannot lose their source-backed required Skills even when model routing metadata drifts. Skills are read-only prompt knowledge: they cannot grant file, Apply, Save, network, or shell authority.
- Work 共享同一份受限最近对话、当前对象和已捕获 `current/rules/art` 文档投影。第一轮可请求命名 Section、引用或本地对象搜索；必要时 IDE 允许最多两次只包含查询摘要的补查，再进入结构化执行。正常 Work 为 2..4 次模型调用；若执行结果属于白名单内的可修正结构化失败，还可使用同一冻结事实追加一次修复，因此绝对上限为 5 次。重复查询会立即停止，且不能读取任意路径、扫描目录或改变项目成员。
- Work 实际运行项目检索后显示一行紧凑只读摘要，只包含 query batch、规范实体、成功 Host 事实和停止状态；Chat、无检索活动与由现有错误卡负责解释的 clarification/provider failure 不显示该行。
- 当第一轮明确查询 `rules` 或 `art` 时，字段编辑会继续使用同一项目作用域，第二轮必须保留查询命中的文档目标。若模型仍把既有 Section 放到另一文件，预览会拒绝并指出选错的文件和 Section 实际所在文件，而不会静默移动操作。
- Every successful structured proposal opens a temporary review document. Its default Result mode is the exact full `CandidateText` in a read-only highlighted editor, Changes preserves the bounded unified Diff, and Object Context shows depth-one exact related Sections from the captured snapshot. Changed-file tabs, Section outline and wrap navigation improve project review without adding partial Apply or Save authority. Closing the document preserves the proposal and the inline card can reopen it.
- A structured proposal cannot apply itself. The user must click Apply. Current-document and rules/art production Work both use model-owned bounded operation plans; Field Registry/diagnostic semantics are review evidence rather than a closed-world content veto. Stale proposals are always rejected.
- Applying a proposal changes only the current in-memory editor session, creates one Undo unit, and never saves. Custom endpoints and read-only/no-document states remain advisory-only.
- Generic transport retry and model fallback are intentionally not implemented. Timeout, network, cancellation, configuration, stale-context, resource, and safety failures never enter structured repair.

## 10. Voxel Style Review Workspace

The IDE now exposes the Stage 1E natural-language voxel colour pipeline through `Tools -> Voxel Style Preview`.

The current workspace is organized as five task stages: Model, Geometry, Partition & Label, Colour, and Review & Export.
Unit class is selected and confirmed by the user; the active colouring workflow does not call DeepSeek for classification.
The Host binds that current human selection to exactly one Ground/Air/LargeSurface/Unknown colouring Skill. Base colour
remains an exact human-selected index from the active RA2 palette.

- Select a bounded single-model `.vox`, or a single-Section `.vxl` with its explicitly selected Westwood `.pal`, inside
  the active project. Both become the same immutable canonical voxel snapshot.
- Review the original SliceStack locally before any network request.
- Add a natural-language per-request style override on top of inherited `VOXEL_STYLE.md` sources.
- Explicitly compile a structured style plan through the currently selected DeepSeek model.
- Compare original, coloured result, geometry-region mask and palette swatch, then review roles, rules and unresolved risk.
- Ordinary shading does not require team-colour/remap metadata. Without a remap range, text-only team-colour intent stays
  unresolved and cannot block or silently alter the normal body-colour preview.
- Optionally pair a project-contained `.glb` with the loaded baseline and generate local Direct, Refined and optional
  Symmetry geometry candidates. Provenance is shown as verified, user-paired or mismatched rather than guessed.
- Compare quality metrics, normals and semantic-region evidence, explicitly choose one geometry for the current session,
  then compare ordinary and optional contrast-enhanced colour candidates.

The workspace remains project-write-free. The user can explicitly freeze one materializable final candidate and export a
verified MagicaVoxel `.vox` copy; this does not apply/save the project, register an asset, or generate VXL/HVA. Opening the
workspace or loading a source never calls DeepSeek. Real provider usage begins only when the user clicks an explicit
compile, recognition or generation action and may consume provider quota.

## 11. IDE-Only Package Boundary

The IDE-only clean source package includes the current IDE projects, tests, tools, documentation, and BuiltIn field registry assets needed for IDE development and validation.

It must not restore or ship:

- legacy root `RA2IniEditor.sln`
- legacy root `RA2IniEditor.csproj`
- legacy table-style editor source
- old object workbench, country manager, side manager, or legacy MainWindow workflows

## 12. Interactive Voxel Review

The Voxel Style workspace displays original, coloured and geometry-region models in an interactive 3D viewport. Drag to
orbit, use middle-drag or Shift-drag to pan, use the wheel to zoom, and reset to fit the full model. Palette remains a 2D
swatch; the existing SliceStack can be selected as a diagnostic view and is used automatically if the bounded 3D scene
cannot be built. The same viewport now compares Current/Direct/Refined/optional Symmetry geometry and Styled/Contrast
results. This viewport is a review surface, not a VXL/HVA writer or game-lighting simulator.

## 13. Known Limitations

- Some reference and diagnostic results depend on available project context and field metadata.
- A valid mod-specific field may appear as Unknown Key after low-evidence fallback rows are quarantined; add verified Project/Global metadata instead of treating every warning as proof that the field is invalid.
- UI automation tests are opt-in and are not part of the ordinary unit test command.
- Project search and current-file Replace All are implemented. Project-level/multi-file replace and recursive disk search remain intentionally unavailable.
- AI structured edits support generic model-owned bounded operations against the current document or unique opened rules/art pair. Legacy typed templates remain headless compatibility helpers only. Other project files, arbitrary paths, automatic Apply/Save, external Skills, and custom-endpoint tools are intentionally unavailable.
- Work can create complete Ares UnitDelivery, GenericWarhead and other SuperWeapon proposals in the unique opened rules project. Capability-specific Skills and Host retrieval provide evidence, while DeepSeek returns the complete bounded rules/art operation plan; local typed Profiles no longer veto production content. Natural/display names are searched in the captured rules snapshot by canonical Section ID and exact local `Name`/`UIName`; unique matches become canonical bindings, while fuzzy or ambiguous guesses are refined or returned for review. Art and asset files are not required, generated or written.
- AvalonDock's floating child-HWND currently prevents the automation harness from traversing into hosted Search controls; this is tracked separately from normal visual/interaction behavior.
- Historical handoff documents may still mention older behavior; the current product-facing overview is this IDE-only direction.

## 14. Long-Term Product Direction

The accepted destination is a high-level Agent that can turn natural-language mod
requirements into reviewable INI, Cameo/Icon, VOX/VXL and SHP artifacts and bind them
together. Those asset pipelines, an independent CLI/Agent host,
multi-file transactions and runtime test host are **not current features**. Their
staged implementation path is maintained in `Docs/DevelopmentRoadmap.md`.

## 15. Review-First Voxel Structure Recognition

The Voxel Style workspace can pair the current VOX/VXL baseline with a project-contained GLB, generate local geometry
candidates, and then explicitly ask DeepSeek for a sparse Agent geometry proposal. The primary pass may request one
bounded detail slice; an independent review normally completes the workflow in two analysis calls. A third arbitration
call is made only when the two executable `(target, action)` sets differ; the absolute ceiling is four calls when the one
detail query is also used. Omitted targets remain unchanged. Host code expands only known add-mirror/remove-source targets
or Agent-selected one/two-cell `bridge_center_gap` targets on the X center seam, and enforces minimum geometry safety
without substituting its own semantic classification or edit direction. Arbitrary/off-axis/three-cell holes are never
promoted by this seam rule. The existing
3D structure and candidate-difference views remain review-only. Only an explicitly frozen materializable candidate may be
exported through the separate verified VOX Save-As action.

Local candidate review now shows Conservative, Balanced and SurfacePolish facts together. Refined is selected only from
candidates that pass structural gates and materially reduce roughness; aggressive cleanup remains review-only when it
does not satisfy that goal. Difference view uses only geometry delta, and structure-response errors report whether the
provider shape or the current evidence partition failed. Real-provider recognition still requires manual acceptance.
## Voxel semantic masking (ASSET-VOX-4A)

The Voxel Style workspace can derive bounded semantic regions from the current working geometry and ask DeepSeek for
text-only part/material suggestions. Suggestions are never automatically authoritative: the user accepts or discards them,
can click a 3D region and override part/material labels, mirror the correction to the opposite side, and explicitly approve
team colour only where intended. Accepted material masks reuse the normal palette-safe style preview; they do not modify
geometry or write an asset until the user separately freezes and exports a candidate.

ASSET-VOX-4B adds a compact manual correction layer on top of that seed. The user can select an independent brush target,
rotate the existing 3D model, and paint or erase bounded exposed-surface cells with optional mirror linkage. Brush changes
are locally undoable/redoable and do not change geometry. The effective per-cell mask continues through the same
palette-safe style preview and explicit VOX freeze/export workflow.

ASSET-VOX-4B-FIX2 gives pointer input a single owner: left click performs semantic selection/paint/erase, while right drag
orbits from either model or empty viewport space (Shift+right/middle pans, wheel zooms). Each rendered surface quad carries
session-only exact canonical-coordinate hit metadata, so painting never falls back to a nearest-cell guess.

ASSET-VOX-4B-STROKE-1 extends that exact hit path into a cancellable continuous stroke. Holding the left button samples
only currently visible surfaces, shows a lightweight temporary path, and commits the complete stroke once on release as
one undoable edit. The semantic view can switch between fixed Part and Material review colours; those colours and the
legend are presentation-only and never write the asset palette.
## 可保存的体素语义分划

“体素风格预览 → 语义”现在支持把部件/材质分划显式保存为项目内 `.semantic.json`，并在以后载入同一
canonical 模型时恢复。文件保留 AI 建议与人工覆盖的来源层级；它不是模型文件，也不会修改 VOX/VXL/HVA。
