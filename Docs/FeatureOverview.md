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

## 3. Project Explorer And Navigation

- Browse project files and sections from the IDE shell.
- Navigate between sections without relying on the old object table workflow.
- Keep the Source Editor as the primary editing surface.

### 3.1 Dockable Workspace

- Project Explorer and AI Assistant share the right tool area. Problems and Output occupy the default bottom tool area; Find All References is on demand. Search opens independently as a hidden-by-default floating tool.
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
- Sending is always explicit and can incur network usage and provider cost.
- Only bounded current-editor context, evidence, diagnostics summaries, and eligible recent conversation turns can enter a request; outbound text is sanitized before transmission.
- Prompts longer than 8000 characters are rejected before a request starts and remain available for editing.
- Streaming cancellation, timeout, provider failures, and incomplete output preserve received text while keeping failed turns out of future conversation context.
- On the official endpoint, an editable current document can expose one bounded preview-only structured-edit tool. A returned proposal is validated locally, previewed against the exact request snapshot, and shown as an inline review card.
- A structured proposal cannot apply itself. The user must click Apply; added errors block Apply, riskier evidence is marked for review, and stale proposals are rejected.
- Applying a proposal changes only the current in-memory editor session, creates one Undo unit, and never saves. Custom endpoints and read-only/no-document states remain advisory-only.
- Automatic retry and model fallback are intentionally not implemented.

## 10. IDE-Only Package Boundary

The IDE-only clean source package includes the current IDE projects, tests, tools, documentation, and BuiltIn field registry assets needed for IDE development and validation.

It must not restore or ship:

- legacy root `RA2IniEditor.sln`
- legacy root `RA2IniEditor.csproj`
- legacy table-style editor source
- old object workbench, country manager, side manager, or legacy MainWindow workflows

## 11. Known Limitations

- Some reference and diagnostic results depend on available project context and field metadata.
- A valid mod-specific field may appear as Unknown Key after low-evidence fallback rows are quarantined; add verified Project/Global metadata instead of treating every warning as proof that the field is invalid.
- UI automation tests are opt-in and are not part of the ordinary unit test command.
- Project search and current-file Replace All are implemented. Project-level/multi-file replace and recursive disk search remain intentionally unavailable.
- AI structured edits support only bounded field upsert/value replacement in the current document. Generic patches, project-wide edits, automatic Apply/Save, and custom-endpoint tools are intentionally unavailable.
- AvalonDock's floating child-HWND currently prevents the automation harness from traversing into hosted Search controls; this is tracked separately from normal visual/interaction behavior.
- Historical handoff documents may still mention older behavior; the current product-facing overview is this IDE-only direction.

## 12. Long-Term Product Direction

The accepted destination is a high-level Agent that can turn natural-language mod
requirements into reviewable INI, Cameo/Icon, VOX/VXL and SHP artifacts and bind them
together. Those asset pipelines, an independent Capability Gateway, CLI/Agent host,
multi-file transactions and runtime test host are **not current features**. Their
staged implementation path is maintained in `Docs/DevelopmentRoadmap.md`.
