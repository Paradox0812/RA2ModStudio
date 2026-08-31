# RA2IniEditor.IDE User Guide

This guide describes the IDE-only package. It does not cover the removed legacy table-style editor.

## 1. Open An INI Project

Start RA2IniEditor.IDE and open the folder or entry INI file for your RA2 / YR project. For split INI projects, open the project root or the main INI entry point used by your workflow.

After opening, confirm that the project files, sections, and diagnostics have loaded before making large edits.

If `.ini` is already associated with `RA2IniEditor.IDE.exe`, opening one existing INI
from Explorer now opens its direct parent folder as the project and selects that exact
file in an editable source session. Only top-level INI files in that folder belong to
this project-open operation. Each Explorer launch currently starts a separate IDE
process; reuse of an already-running window is not yet implemented.

## 2. Browse Project Files

Use the Project Explorer to inspect the files that belong to the current project. Select a file or section to bring the related text into the Source Editor.

The IDE-only package is source-first: navigation is organized around files and sections, not around the old object table workbench. Unsaved in-memory sessions are retained when you switch between files in the same project. Opening another project or closing the IDE is blocked while any project file remains dirty; save or revert each dirty file first.

### 2.1 Arrange Tool Windows

Drag a tool tab to float or re-dock it, and drag the pane splitter to change its width or height. The default right area contains Project Explorer and AI Assistant. Problems and Output are visible in the default bottom area; Find All References is on demand. Search opens as an independent floating tool and is hidden by default.

The floating Search title bar provides minimize and close controls. Closing it hides the same managed Search tool. Search always starts hidden, even if an older persisted layout recorded it as visible or bottom-docked; selecting Search opens it in the independent Floating Home zone. If it was minimized during the current session, selecting Search restores and activates it. Search is not materialized as a native floating host during normal startup. A dedicated maximize button is intentionally omitted, while normal caption dragging and edge resizing remain available.

To recover several windows at once, use the Window Layout button on the main toolbar or `View > Window Layout`:

- `Return Floating Tools Home` moves only currently floating managed tools back to their compiled areas.
- `Reset Default Layout` restores the default tool order, visibility, selection, and right/bottom dimensions.

The current presentation layout is stored in `shell-layout.v2.xml`. A normal restart restores valid user layout; Reset writes the compiled default arrangement immediately.

## 3. Edit With The Source Editor

Use the AvalonEdit-based Source Editor to edit INI text directly.

Recommended habits:

- Keep section headers and key/value lines in normal INI format.
- Make small edits and watch diagnostics after significant changes.
- Use undo / redo for local text edits.
- Save only after reviewing visible issues and save preflight prompts.

## 4. Use Completion

Completion provides field and value suggestions when the current source context is understood.

Completion quality depends on available metadata:

- Project field registry
- Global field registry
- BuiltIn v3.2 fallback field library
- Current section and field context

If a suggestion is missing, the field may still be valid for a specific mod or custom extension.

Fields known only as wrong-context, obsolete, non-existent, or pseudo-field diagnostics are not suggested as field names. If you type one manually, Hover, Quick Peek, or Diagnostics can still explain the risk. The BuiltIn library also quarantines low-evidence placeholder rows, so a valid custom extension can require Project or Global field metadata before it stops appearing as Unknown Key.

## 5. Read Hover And Reference Hover

Hover can show field descriptions and context near the current source location.

Reference Value Hover helps inspect recognized references such as weapons, projectiles, warheads, sounds, images, or other section-like values when the project context can resolve them.

Use hover information as a quick aid; verify unusual mod-specific references manually.

## 6. Use Quick Peek And Find References

Quick Peek / definition details let you inspect a referenced section or field detail without leaving the source-first editing flow.

Find References helps locate where a section or value is used across the loaded project context. This is useful before renaming, deleting, or changing important shared values.

### 6.1 Search The Project And Replace In The Current File

Open Search from the toolbar. Choose `整个项目` to search every `.ini` file currently listed in Project Explorer, or `当前文件` to search only the active editor buffer.

Search supports case-sensitive, whole-word, and .NET regular-expression matching. Double-click a result, press Enter, or use Previous/Next to navigate. Cross-file navigation retains dirty in-memory sessions within the current project; it does not save them automatically.

Replacement is intentionally available only for `当前文件`:

1. Enter the search and replacement text.
2. Click `预览`; this does not modify text.
3. Review the replacement count and click `应用`.
4. Use Ctrl+Z/Ctrl+Y to undo or redo the complete batch as one step.
5. Save normally when satisfied.

Application changes only the in-memory document and never saves automatically. If the document changes after preview, the IDE rejects the stale plan and requires a new preview.

## 7. Read Diagnostics And Issues

Issues / Diagnostics collect parse errors, validation warnings, unresolved references, and other project understanding results.

Common workflow:

1. Open or reload the project.
2. Review diagnostics before editing.
3. Make source edits.
4. Review diagnostics again.
5. Save only after confirming expected changes.

Not every warning is automatically a bug. RA2 / YR mods often use soft references, script-driven values, or extension-specific behavior.

For an expected custom field reported as Unknown Key, first verify its exact section context and extension version. If it is valid, add or import reviewed Project/Global registry metadata; do not copy a broad placeholder definition into unrelated contexts merely to silence the warning.

## 8. Save With Preflight

Before saving, review any save preflight confirmation shown by the IDE.

Preflight is intended to make risky writes visible, especially when the current document has parse issues, unresolved references, or pending editor state changes.

## 9. Use Backup / Rollback

When a workflow creates backups, keep the backup location until you have reopened and verified the edited project.

Rollback support is intended for explicit recovery paths, especially around field registry workflows. It should not replace external version control or manual project backups before large changes.

## 10. Manage Field Registries

Use Field Registry Manager to inspect and reload local field metadata.

The effective field priority is:

1. Project
2. Global
3. BuiltIn

Field learning / import preview helps inspect parsed fields before applying registry changes. Review preview, validation issues, and target scope before applying.

## 11. Use The AI Assistant

Open the AI tab and choose a mode before sending. `Chat` is the default and only answers or analyses; it never receives structured editing tools. Choose `Work` when you want a reviewable current-document modification. The model list contains DeepSeek V4 Flash and DeepSeek V4 Pro; V4 Flash is selected by default.

`Work` starts with one bounded intent call. If the request names existing project objects but their canonical Section IDs are not yet proven, the IDE may make up to two compact query-planning calls and search only the captured `current/rules/art` snapshots before the structured execution call. Normal Work therefore uses 2..4 provider calls. If execution then produces an allowlisted, model-correctable structured failure, one additional non-streaming correction is allowed, for an absolute maximum of five calls. Repeated/no-progress queries stop immediately; transport failures are not retried. After project retrieval actually runs, one muted line reports query batches, canonical entities, successful Host facts and the stop state; it contains no prompt, path or provider diagnostics. Only the final result is shown, and no path grants automatic Apply or Save authority.

Work mode already scopes supported authoring requests to the active current document, so the prompt does not need to repeat “当前文件”. It must still identify the target Section/object and describe a supported operation clearly.

Before sending, remember:

- A request can transmit bounded current-editor context and may incur provider charges.
- Ordinary assistant text is advisory and does not modify the editor.
- Prompts over 8000 characters are rejected before network activity and remain in the input box.
- Streaming output may end as cancelled, timed out, incomplete, or failed. Received text remains copyable, but failed turns are excluded from later conversation context.
- The IDE does not retry network/timeout/configuration failures or switch models. Only an allowlisted structured-plan failure can receive one bounded correction attempt.

The configuration status is intentionally safe: it reports readiness and official/custom endpoint use without displaying the API key or endpoint value.

When the official endpoint is configured and the current document is editable, the
assistant can return a bounded structured-edit proposal:

1. State explicitly that you want to modify the **current file**, and identify the target Section, key, and value where possible; for example: `把当前文件 [E1] 下的 Strength 修改为 150`.
2. If the request is ambiguous, the IDE asks you to clarify locally and preserves the prompt instead of treating generated INI text as an edit.
3. Review the automatically opened `修改预览：<文件名>` document in the main workspace. `结果` is the default and shows the exact complete candidate file in a read-only highlighted editor; use the left outline and Previous/Next commands to inspect complete changed Sections. `差异` preserves removed lines and old/new line numbers. `对象上下文` shows bounded directly referenced Sections as read-only evidence. Project proposals expose compact changed-file tabs. Closing the review tab does not discard the proposal, and `查看更改` on the inline card reopens it.
4. Review every operation, old/new line, field evidence, diagnostics count, and risk status. Diagnostics and registry trust are review evidence; malformed structure, unsafe identifiers or a stale snapshot fail before an applicable proposal is created.
5. Click `应用全部` only after review, or `放弃修改` to discard it. Partial hunk acceptance is not available.
6. Applying changes only the current in-memory document, returns to the source editor, creates one Undo unit, and does not save. Use Ctrl+Z to undo, then save normally if satisfied.

Capability routing distinguishes intent only to select relevant Skills and retrieval. In production Work, DeepSeek always returns a bounded current-document operation plan. If you explicitly ask for a skeleton/framework, it should return a skeleton; otherwise it is instructed to construct the complete usable object, including required Sections, references and registrations. The Host does not replace that decision with a fixed weapon/Projectile/Warhead Profile. Review all selected gameplay values in Diff because automated tests certify the transaction boundary, not game balance or runtime correctness.

Work mode can also construct project changes against a unique `rulesmd.ini` or `rules.ini`; a matching `artmd.ini`/`art.ini` is included when present. After opening the project folder, switch to Work and check the context summary. Describe the desired result naturally; for example: `给 HTNK 绑定美术：Art=HTNKART，Body=HTNKBODY，Cameo=HTNKICON。` DeepSeek is responsible for choosing all required Sections, fields, values, registrations and rules/art relationships, or for asking one concrete clarification when an indispensable target cannot be determined. It may propose new mod-specific keys and Sections even when they are absent from the Field Registry; registry trust and diagnostics are review hints, not a local rejection rule for this project proposal.

The Host still enforces minimum safety: the model can target only the captured rules/art pair, identifiers and payload sizes are bounded, the proposal must pass canonical INI Preview, and Apply remains explicit, snapshot-bound, single-use and atomic. The model cannot provide arbitrary paths, save files, create assets or bypass the Project Diff. `应用到项目` updates every changed in-memory session atomically, does not save, and one Ctrl+Z/Ctrl+Y undoes or redoes the project transaction together. The proposal does not require an Asset Manifest; missing SHP/Icon/VXL files do not block INI editing and no asset file is created.

Provider-generated proposal titles and explanatory messages are optional display text. Missing or malformed display text is replaced or ignored locally and does not discard otherwise valid document operations. A clarification response is different: it must contain a readable question and never creates an applicable proposal.

Project authoring requires one unique top-level rules target in the opened project. If the prerequisite is not met,
Work reports the concrete local reason (no project, missing/ambiguous rules target, unavailable snapshot, read-only
target, or resource limit) instead of presenting it as a DeepSeek parsing failure. It does not create a missing
rules or art file automatically. An empty matching art file is valid and optional; unrelated top-level INI files do not make a unique rules target ambiguous.

Work can also request Arcing/Homing/Phobos Projectile, YR/Ares Warhead and more complex current-file constructions through the same generic operation plan. Describe the intended behavior naturally and name known objects when possible. DeepSeek owns the semantic construction or asks for an indispensable missing fact; the Host only enforces captured-document authority, bounded safe INI identifiers, canonical Preview, explicit Apply and stale/undo/save boundaries.

The assistant selects from 18 bundled RA2 domain Skills. In Work mode, the first request sees only a compact Skill catalog and recommends the relevant entries; the IDE validates the recommendation, adds any capability-required guidance, and gives only those full Skill instructions to the execution request. Project and SuperWeapon routes therefore receive their source-backed required Skills. These Skills improve terminology, dependency order and version-aware semantics; they do not install code, call tools, write files, apply changes, or save the document.

Work includes two typed Ares SuperWeapon profiles. UnitDelivery creates the registration, provider binding or AlwaysGranted policy, common SuperWeapon fields, delivery list and ownership policy while referencing existing TechnoTypes. GenericWarhead creates the same common closure and references one existing Warhead without modifying it. You may describe existing buildings, units and Warheads by a natural/display name: the intent call must infer likely canonical Section IDs and verify those candidates against the captured rules snapshot before execution. The Host also accepts an exact, unique existing `Name`/`UIName` alias and normalizes it to the canonical Section ID; it never performs fuzzy matching. Missing or ambiguous identities are rejected for clarification instead of being guessed. Other explicit SuperWeapon types use the generic reviewed project-plan path. Every result is only a Project Diff until you explicitly apply it, and no path saves or creates assets automatically.

Work 的所有调用使用同一份最近对话、当前对象和本次发送开始时捕获的文档版本。第一轮可以要求 IDE
查询一个明确命名的 `current`、`rules` 或 `art` Section/引用，也可以按本地 `Name`/`UIName` 搜索对象。
如果首轮证据不足，最多再进行两轮紧凑补查；唯一的高置信结果会绑定到规范 Section ID 后交给执行轮。
因此“盟军发电厂”“GI”“IFV”这类已经存在于项目中的本地名称不再要求用户手工补写 Section ID。
多义名称不会被静默选择。这不会让模型读取任意路径、搜索整个磁盘或自动保存；若发送期间文档变化，
后续既有 stale 门禁仍会拒绝应用。

如果第二轮没有调用必需工具、参数 JSON 无效、选错既有 Section/文档或未通过可修正的 canonical Preview，
IDE 会在文档仍未变化时最多自动修正一次。成功时卡片显示“已自动修正 1 次”；仍失败时提示词会恢复，
不会生成 Proposal。网络、超时、取消、配置、资源、安全和过期上下文错误不会触发该修复。

Work 第一轮允许模型返回附加说明字段、缺省可选列表或省略查询占位参数；这些差异不会再阻止预览。
未知 domain/capability 也会在已有项目快照内转入通用 Project Plan。路径形式的查询目标仍不会执行，但只会
丢弃该查询，模型仍可利用捕获的项目摘要继续补查或生成可审阅预览。Ares UnitDelivery/GenericWarhead 在产品
Work 中同样由模型直接给出完整 rules 操作，不再由本地固定 Profile 决定字段内容。

如果请求明确指定项目中的 `rules` 或 `art` 文件，并要求先读取某个现有 Section，IDE 会让后续结构化
修改保持在查询命中的同一文件。例如检查 art `[HTNKART]` 后设置 `Remapable=yes`，应产生 art 项目预览，
而不是当前文档预览。若模型仍选错 rules/art 目标，错误会同时显示模型选择的文件和 Section 实际所在文件；
本次不会应用，也不会自动把操作移动到另一文件。

For Techno visual bindings, `Art`, `Body`, and `Cameo` in natural language are semantic roles, not literal rules keys. The normal graph is rules `[Owner].Image -> art [Image]`, with `Cameo` in the art section. Vanilla YR does not universally support a different art-section ID and body filename for Infantry/Vehicle/Aircraft; Phobos requires an already established `[General] ArtImageSwap=true`. If that distinction matters but the request context does not establish it, Work should ask a concrete clarification rather than write `Art=/Body=/Cameo=` into rules or silently enable a project-wide switch.

Editing or switching the document, reloading field metadata, clearing chat, or
receiving a newer proposal invalidates the old proposal. The tool cannot save,
edit multiple files, run commands, or operate through a custom endpoint.
If the provider returns only explanatory text for an explicit edit request, the IDE
rejects it as a missing structured-tool result; that text does not become an editable
proposal or accepted conversation state.

## 12. Previewing a natural-language voxel style

1. Open the folder that contains the voxel candidate and any optional project/directory `VOXEL_STYLE.md` files.
2. Choose `Tools -> Voxel Style Preview`.
3. Follow the five-stage navigator: `模型 / 几何 / 分划与标注 / 上色 / 审阅与导出`. The left panel contains only the
   current stage's actions; the lower tabs are read-only review facts.
4. On `模型`, click `载入模型…` and select either a single-model MagicaVoxel `.vox`, or a single-Section Westwood `.vxl`, inside
   the opened project. VXL input prompts for its corresponding project-contained 768-byte Westwood `.pal`; the IDE never
   guesses a palette or trusts the VXL reserved palette block.
5. Inspect the original model in the interactive 3D viewport. Left-drag rotates, middle-drag or Shift+left-drag pans,
   the wheel zooms, and `Reset View` fits the whole model. Switching review modes or temporarily switching documents keeps
   the current camera; choosing a genuinely different source fits that model once. This step is local and does not call DeepSeek.
6. Optional geometry review: on `几何`, click `载入 GLB…`, select a project-contained source mesh, then click `生成候选`.
   This local step makes no model/provider call. Compare Current, Direct, Refined and optional Symmetry; check the shown
   source provenance and quality facts, then click `Use for This Session` only on the candidate you want to colour.
7. On `分划与标注`, click `创建人工区域` for a provider-free manual workflow, or enter a short
   semantic hint and click `AI 建议`. DeepSeek receives only bounded
   text geometry facts. Review the two-pass result (a third pass appears only on disagreement), then click `接受建议` or
   `丢弃建议`. Use the same stage to load/save `.semantic.json`, choose browse/paint/erase, brush part/material/remap target,
   size and mirror behavior. AI never approves remap; enable it only for intended regions.
8. On `上色`, manually choose `地面载具 / 空中载具 / 大型水面单位 / 未知` and click `确认单位类型`. The IDE does not
   ask DeepSeek to classify the unit. The Host deterministically displays and loads exactly one matching colouring Skill.
9. Select one legal opaque/non-remap base colour from the active RA2 palette and one rule/technique template. Enter a style
   override or leave it empty, then click `编译上色预览`. This explicit compile may call the selected DeepSeek model and consume quota; the IDE
   does not retry automatically.
10. Use the single preview selector to switch between Original, candidates, Semantics, Styled, optional Contrast, Region and Palette. Geometry and colour results are 3D;
   Palette stays 2D. Use
   `Diagnostic Slices` when checking voxel axes/import layout or if the bounded 3D renderer reports a fallback. Review
   the lower `几何摘要 / 区域清单 / 上色计划 / 审阅问题` tabs; its divider can be dragged without changing the 3D camera.
11. Select a materializable view (`Original`, `Direct`, an available `Refined`/`Agent Geometry`, `Styled`, or `Contrast`) and
   click `固化最终候选`. Difference, Structure Regions, Region Mask and Palette are review-only and cannot be frozen.
12. On `审阅与导出`, click `导出 VOX…` and choose a new `.vox` path. The IDE writes a same-directory temporary file, reads it back through
    the canonical codec, verifies deterministic bytes and only then publishes it. The currently loaded source VOX cannot be
    overwritten. This action exports a copy only; it does not create VXL/HVA, apply/save the project or register an asset.

`Use for This Session` selects geometry; `固化最终候选` freezes the currently visible materializable snapshot as the sole
export authority. They are intentionally separate. Pure view switching does not change the frozen candidate; changing the
source, adopted geometry or compiled style invalidates it. A `User paired` GLB is not cryptographically proven to be the
source of the baseline and should be visually checked before use.

Semantic glass, tyre, accent and remap painting remains non-authoritative until AI suggestions are accepted or a human
override creates an explicit mask. The text model does not see the render; visually ambiguous regions should remain Unknown
and be corrected manually. Ordinary VOX shading does not require remap indices. Multi-Section VXL files currently require a later Section-selection UI and are rejected
instead of silently choosing one Section.

The viewport label `geometry review lighting` is intentional: it shades exposed faces for readability but does not yet
visualize VXL normal indices or reproduce the game engine's lighting.

## 13. Known Limitations

- The legacy table editor, object workbench, country manager, side manager, and old object copy workflows are not part of this IDE-only package.
- Diagnostics and reference resolution depend on loaded files and available metadata.
- Some features may be conservative or preview-first by design to avoid unsafe writes.
- Search is on-demand rather than a persistent background index. Files above the current 8 MB preview boundary are skipped and reported in Search status.
- Replace All is current-file only; project-level or multi-file replace is not available.
- Current-file and unique rules/art Work accept generic model-owned structured operations rather than fixed production Profiles. Project asset persistence, codec validation, actual asset generation and edits to other project files remain unavailable.
- Diff projection fails closed above 8 MiB, 200,000 input lines, 20,000 visible rows, or 2,000 hunks; the inline proposal remains available for whole-plan review/apply.
- Floating Search content has a known AvalonDock child-window UI Automation provider limitation; the visible controls remain usable normally, but the current automation probe cannot traverse that hosted subtree.
- Physical compact-resolution and non-100% DPI visual checks may still require manual verification on matching hardware.
- Historical handoff documents may describe older implementation phases; use this guide for current product-facing IDE usage.

## Voxel structure review workflow

1. Open the Voxel Style workspace, load a project-contained VOX or VXL (VXL also requires its PAL), and pair its GLB.
2. Click `生成候选`. This step is local, creates Direct/Refined review data and does not consume AI quota.
   A successful local pass reports `结构证据已就绪（N 个区域）`. The AI action then remains clickable; if DeepSeek is
   not configured, clicking it reports that fact without sending a request. A local evidence failure still keeps the
   action disabled and consumes no quota. Read `组合审阅` before choosing a result: it lists Conservative, Balanced and
   SurfacePolish deltas and quality facts. Only a candidate with measurable roughness improvement can become Refined;
   a more aggressive cleanup may remain `仅审阅` even when it lowers the low-support count.
3. Click `AI 识别结构` only when you want provider calls. It normally performs a primary analysis and independent review;
   a third arbitration runs only when their executable target/action sets differ. The primary may additionally request one
   bounded detail slice, so the absolute maximum is four calls. There is no hidden retry. Review `结构区`: cyan marks a
   selected mirror-add target, amber a selected removal target, blue protected geometry, and violet omitted/preserved
   geometry. When the evidence contains a one/two-cell X-center seam with occupied anchors on both sides, the Agent can
   additionally select `bridge_center_gap`; it is shown as a real green addition in the candidate difference. The Host does
   not fill it automatically, and three-cell/off-axis/arbitrary holes are excluded. These colours describe the final
   proposal rather than a Host-owned part classification.
4. Review `对称`. It is available only when the final sparse proposal passes the minimum local safety line and shows the
   actual green additions/red removals relative to the Refined baseline, with unchanged geometry translucent grey.
5. Click `用于本会话` only if the chosen geometry is acceptable. This changes only the working geometry; use the separate
   `固化最终候选` and `导出 VOX…` actions if you later want a verified VOX copy.

Changing the source, GLB, project or selected model invalidates the previous structure result. AI failure never removes the
local Direct/Refined candidates. Current colouring remains coarse geometry-based and should not be treated as authoritative
glass, tyre, metal, accent or team-colour recognition.

If AI structure recognition fails, the status now distinguishes a missing/duplicate tool field, invalid JSON/value, an
incomplete region list, or stale evidence. These messages diagnose the provider result; they do not discard the local
Direct/Refined candidates and do not mean the model file was modified.

If both rounds complete but no repair region is jointly confirmed as body core, the IDE now says so directly and keeps the
local candidates unchanged. This is a safe no-op, not a hidden edit. Rebuild/restart after an update before repeating the
same recognition case; a running old executable will continue to use its previously loaded evidence and prompt contract.

## 人工修正体素材质区域

1. 可以在“分划与标注”阶段点击 `获取 AI 建议` 并接受，也可以完全跳过模型调用；点击
   `浏览`、`画笔` 或 `擦除` 时，IDE 会自动准备本地人工区域。
2. 区域下拉框用于大块赋值；画笔部件/材质是独立目标，不需要预先选择区域列表行。短点击会自动采用命中的
   3D 区域，并在列表中同步选中它。
3. 切换为 `画笔`，选择大小 1–3 和是否镜像。左键单击可修改一个位置；按住左键拖动可连续绘制，释放时
   整条笔划一次提交、一次撤销即可完全恢复。左键不控制相机；在模型或主视图空白处按住右键拖动可旋转，
   Shift+右键或中键拖动可平移，滚轮可缩放。
4. 切换为 `擦除` 可移除局部人工覆盖，恢复区域/AI 底稿；`撤销/重做` 只作用于体素画笔，不删除 AI 建议。
5. 完成后进入“上色”阶段，人工确认单位类型、基准色与技法，再编译着色预览。只有明确的材质角色会进入色板安全映射；阵营色仍必须人工批准。

画笔只处理当前可见的外露表面；每个可见面会精确映射回所属体素，不再用屏幕附近的体素猜测。空白处左键
会显示“未命中模型表面”且不修改蒙版。旋转模型即可接触另一侧。它不修改几何、不会自动保存，也不会写 VXL/HVA。
拖动过程中黄色表示待提交绘制路径、红色表示待提交擦除路径；它们只显示命中采样点。`部件 / 材质` 可切换
当前审阅配色和图例，这些标注色不等于游戏内色板颜色。

## 从参考图生成会话内体素预览

1. 打开项目和“体素风格预览”。
2. 在“生成模型预览”中选择一张 PNG/JPEG；没有已载入模型色板时，再选择项目内 768 字节 PAL。
3. 可填写设计说明，但当前几何 API 只使用参考图，该说明只作为本次来源记录。
4. 点击“生成预览”，先等待离线 Provider 探测，再审阅一次任务的发送确认。
5. 确认后会创建一个远程任务；成功结果只进入当前会话，不会自动保存或写入项目。

免费余额无法由 IDE 判断。只有确认账户仍使用免费包并已设置所需环境变量时才继续。取消、失败或超时都
不会自动重试。生成结果可在同一工作区继续审阅、固化并导出为 VOX 副本；VXL/HVA、项目自动注册和游戏内
验证仍需等待后续独立功能。
## 保存和恢复体素语义分划

1. 在体素风格工作区的“分划与标注”阶段完成 AI 建议接受、区域人工覆盖或 3D 画笔标注。
2. 点击“保存分划”，在当前项目目录内保存推荐的 `<模型文件名>.semantic.json`。
3. 以后先载入完全相同的 VOX/VXL 工作几何，再点击“载入分划”选择该文件。
4. 如果模型、确定性区域证据或人工层哈希不一致，IDE 会拒绝载入且保留当前会话不变。

sidecar 不参与项目普通 Save/Apply，也不会自动发现或自动载入。替换模型、采用其他几何候选或载入另一份
sidecar 前，IDE 会在当前语义分划存在未保存修改时提示确认。
