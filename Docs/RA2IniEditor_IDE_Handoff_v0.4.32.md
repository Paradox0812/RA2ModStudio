# RA2IniEditor IDE Handoff v0.4.32

## Scope

v0.4.32 adds readonly current-document language preview features for the IDE shell:

- Hover provider for section headers, known keys, and value references.
- Definition provider for field definitions and current-document section targets.
- Reference finder for value references that point to the same current-document section.
- Source editor context menu entries for Go To Definition, Peek Definition, and Find All References.
- Independent Peek Definition and Find References windows.

## Boundaries

All language lookups are based on the current AvalonEdit document text and the v0.4.31 `Ra2DocumentSemanticModel` / `Ra2CaretContext` services.

This version does not add Completion, editing, source persistence, project-wide indexing, legacy analysis, `ObjectAggregator`, `ProjectLoader`, or `ProjectSaveService` integration.

## Behavior

Go To Definition:

- On a value reference such as `Primary=120mm`, navigates to the `[120mm]` section in the current document.
- On a known key such as `Strength`, opens the definition preview because field definitions do not have source offsets inside the current INI file.
- On comments, whitespace, or unknown tokens, reports that no definition is available.

Peek Definition:

- Shows field definition type, source, and description for known keys.
- Shows section title, kind, and line number for section definitions.

Find All References:

- On a section header, lists current-document value references to that section.
- On a value reference, lists other current-document value references with the same target.
- Key reference search remains out of scope.

## UI Notes

The Source Editor remains AvalonEdit readonly. Text still flows from `SourceEditorViewModel.Text` into `SourceTextEditor.Document.Text` through the existing code-behind synchronization path.

The new windows are non-modal and independent from the main layout:

- `Ra2PeekDefinitionWindow`
- `Ra2FindReferencesWindow`

## Risk Notes

- Results are rebuilt from the current editor text at command time, so they do not read disk and do not use stale project-level indexes.
- Value reference extraction still follows the current v0.4.31 semantic model rules: weapon, projectile, and warhead references only.
- Hover UI is intentionally not wired to mouse hover in this version; provider tests cover hover data without introducing unstable pointer behavior.

## Verification

Expected commands:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

Manual smoke:

1. Open a project folder in the IDE shell.
2. Open an INI file with `[NEWINF]`, `Primary=120mm`, and `[120mm]`.
3. Place the caret on `120mm`, open the context menu, and select Go To Definition.
4. Confirm the editor navigates to `[120mm]`.
5. Select Find All References and confirm the references window lists current-document references.
6. Place the caret on `Strength`, select Peek Definition, and confirm field metadata is shown.
