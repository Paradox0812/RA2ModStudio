# RA2IniEditor.IDE User Guide

This guide describes the IDE-only package. It does not cover the removed legacy table-style editor.

## 1. Open An INI Project

Start RA2IniEditor.IDE and open the folder or entry INI file for your RA2 / YR project. For split INI projects, open the project root or the main INI entry point used by your workflow.

After opening, confirm that the project files, sections, and diagnostics have loaded before making large edits.

## 2. Browse Project Files

Use the Project Explorer to inspect the files that belong to the current project. Select a file or section to bring the related text into the Source Editor.

The IDE-only package is source-first: navigation is organized around files and sections, not around the old object table workbench.

### 2.1 Arrange Tool Windows

Drag a tool tab to float or re-dock it, and drag the pane splitter to change its width or height. The default right area contains Project Explorer and AI Assistant. Problems and Output are visible in the default bottom area; Find All References is on demand. Search opens as an independent floating tool and is hidden by default.

The floating Search title bar provides minimize and close controls. Closing it hides the same managed Search tool; selecting Search again reopens it in its last valid dock/floating location. If it was minimized, selecting Search restores and activates it. A dedicated maximize button is intentionally omitted, while normal caption dragging and edge resizing remain available.

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

Search supports case-sensitive, whole-word, and .NET regular-expression matching. Double-click a result, press Enter, or use Previous/Next to navigate. Cross-file navigation still uses the normal Save/Discard/Cancel dirty-file decision.

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

`Work` uses two provider calls for each send. The first call returns a bounded intent package that the IDE validates locally; the second call performs the selected advisory or structured-preview task. Only the second response is shown. If intent analysis fails, the execution call is not sent. This increases Work-mode latency and provider usage, but it does not grant automatic Apply or Save authority.

Work mode already scopes supported authoring requests to the active current document, so the prompt does not need to repeat “当前文件”. It must still identify the target Section/object and describe a supported operation clearly.

Before sending, remember:

- A request can transmit bounded current-editor context and may incur provider charges.
- Ordinary assistant text is advisory and does not modify the editor.
- Prompts over 8000 characters are rejected before network activity and remain in the input box.
- Streaming output may end as cancelled, timed out, incomplete, or failed. Received text remains copyable, but failed turns are excluded from later conversation context.
- The IDE does not automatically retry or switch models after a failure.

The configuration status is intentionally safe: it reports readiness and official/custom endpoint use without displaying the API key or endpoint value.

When the official endpoint is configured and the current document is editable, the
assistant can return a bounded structured-edit proposal:

1. State explicitly that you want to modify the **current file**, and identify the target Section, key, and value where possible; for example: `把当前文件 [E1] 下的 Strength 修改为 150`.
2. If the request is ambiguous, the IDE asks you to clarify locally and preserves the prompt instead of treating generated INI text as an edit.
3. Review the automatically opened `修改预览：<文件名>` document in the main workspace. It shows a read-only unified Diff; closing the tab does not discard the proposal, and `查看更改` on the inline card reopens it.
4. Review every operation, old/new line, field evidence, diagnostics count, and risk status. If the proposal is blocked, fix the request or document; it cannot be applied.
5. Click `应用全部` only after review, or `放弃修改` to discard it. Partial hunk acceptance is not available.
6. Applying changes only the current in-memory document, returns to the source editor, creates one Undo unit, and does not save. Use Ctrl+Z to undo, then save normally if satisfied.

Template routing distinguishes intent. If you explicitly ask for a skeleton/framework, the request must supply all three IDs and the proposal creates only the three Sections plus the Weapon `Projectile=` and `Warhead=` relationships. For an ordinary request to build a usable direct-fire weapon chain, Work mode uses the single-slot complete profile. If you explicitly request complete Primary and Secondary armaments, it can instead create two closed direct-fire chains in one atomic 30-operation proposal. `Primary`/`Secondary` do not guarantee alternating fire: requests that explicitly demand cyclic/alternating fire are currently rejected before sending until the Gattling field/profile source gate is implemented. These profiles still do not add type-list registration, indexes, art, icons, voxels, or SHP assets.

Work mode also supports three focused requests against one existing Weapon: create an original-game curved/Arcing Projectile, create a positive-ROT homing Projectile, or create a YR-core Warhead. State the existing Weapon ID, the new Section ID, and the intended targeting/damage behavior. Arcing and homing are intentionally separate; requests that mix them or ask for unsupported Phobos/Vertical/Airburst trajectories are rejected locally. The YR-core Warhead profile uses the original 11-slot `Verses` layout and refuses a document containing `[ArmorTypes]`; Ares custom armor overrides require a later dedicated profile.

The assistant also selects from 15 bundled RA2 domain Skills according to the request. These Skills improve terminology, dependency order, validation and fail-closed behavior; they do not install code, call tools, write files, apply changes, or save the document.

Editing or switching the document, reloading field metadata, clearing chat, or
receiving a newer proposal invalidates the old proposal. The tool cannot save,
edit multiple files, run commands, or operate through a custom endpoint.
If the provider returns only explanatory text for an explicit edit request, the IDE
rejects it as a missing structured-tool result; that text does not become an editable
proposal or accepted conversation state.

## 12. Known Limitations

- The legacy table editor, object workbench, country manager, side manager, and old object copy workflows are not part of this IDE-only package.
- Diagnostics and reference resolution depend on loaded files and available metadata.
- Some features may be conservative or preview-first by design to avoid unsafe writes.
- Search is on-demand rather than a persistent background index. Files above the current 8 MB preview boundary are skipped and reported in Search status.
- Replace All is current-file only; project-level or multi-file replace is not available.
- Production complete-object coverage is currently limited to one direct-fire Weapon/Projectile/Warhead profile. Techno, AI, SuperWeapon, faction, registration-list, multi-file and asset profiles remain unavailable.
- Diff projection fails closed above 8 MiB, 200,000 input lines, 20,000 visible rows, or 2,000 hunks; the inline proposal remains available for whole-plan review/apply.
- Floating Search content has a known AvalonDock child-window UI Automation provider limitation; the visible controls remain usable normally, but the current automation probe cannot traverse that hosted subtree.
- Physical compact-resolution and non-100% DPI visual checks may still require manual verification on matching hardware.
- Historical handoff documents may describe older implementation phases; use this guide for current product-facing IDE usage.
