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
- [ ] Confirm a proposal that adds an error is blocked, and custom endpoints do not receive the structured-edit tool.
- [ ] Confirm no automatic retry or model fallback occurs.

## 12. Release Notes

- [ ] State that this is an IDE-only package.
- [ ] State that the legacy table-style editor is intentionally absent.
- [ ] List any known limitations around diagnostics, reference resolution, or opt-in UI automation.
