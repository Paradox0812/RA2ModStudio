# RA2IniEditor IDE Handoff v0.4.33

## Scope

v0.4.33 adds a readonly current-document Completion contract preview for the IDE language layer.

This version only implements DTOs, provider contracts, provider logic, and tests. It does not connect AvalonEdit completion UI, does not edit text, does not save, does not create dirty state, and does not use project-wide indexing.

## Added Language Types

- `Ra2CompletionItemKind`
- `Ra2CompletionItem`
- `Ra2CompletionRequest`
- `Ra2CompletionResult`
- `IRa2CompletionProvider`
- `Ra2CompletionProvider`

All types are internal to `RA2IniEditor.IDE`.

## Behavior

Key completion:

- Uses `IRa2FieldDefinitionProvider.GetFields(sectionKind)`.
- Filters by the current key prefix.
- Returns conservative empty results for unknown section kinds.

Reference value completion:

- `Primary`, `Secondary`, `ElitePrimary`, `EliteSecondary`, `DeathWeapon`, `OpenToppedWeapon`, and `Weapon1` through `Weapon10` complete current-document Weapon sections.
- Weapon `Projectile` completes current-document Projectile sections.
- Weapon `Warhead` completes current-document Warhead sections.
- Duplicate target sections are deduplicated by label and kind.

No completion is returned in:

- comment lines;
- inline comments;
- section header comments;
- comma second token contexts;
- numeric literal contexts.

## Replacement Span

Completion results always include a replacement span.

- Key completion replaces the current key prefix.
- Reference value completion replaces the current reference prefix.
- Empty value completion uses a zero-length span at the caret.
- Empty/no-op results use a zero-length span at the caret.

## Boundaries

The provider does not reference WPF, AvalonEdit, file IO, network, save services, legacy project services, or dirty state. It only consumes the current snapshot, semantic model, caret context, caret offset, and field definition provider.

## Validation

Expected validation:

```powershell
dotnet test -c Release
dotnet build -c Release --no-incremental
```

UIA remains independent and is not required for this version because no completion UI was added.
