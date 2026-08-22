# RA2IniEditor IDE Handoff v0.4.31

## Scope

v0.4.31 adds the RA2 Language Service Foundation for current-document readonly INI text.

Implemented:

- `Ra2DocumentSnapshot`
- `Ra2TextSpan`
- `Ra2DocumentSemanticModel`
- `Ra2SectionSymbol`
- `Ra2KeyValueSymbol`
- `Ra2ValueReferenceSymbol`
- `Ra2DocumentSemanticModelBuilder`
- `Ra2CaretContext`
- `Ra2CaretContextService`

## Model

The semantic model is built from one text snapshot only. It does not hold AvalonEdit objects and does not read from disk.

The model includes:

- section symbols with inferred `Ra2SectionKind`;
- key/value symbols with line, key, value, and span data;
- known-key flags from `IRa2FieldDefinitionProvider`;
- value reference symbols for Weapon, Projectile, and Warhead references;
- lookup helpers for section by offset, key/value by offset, and section by name.

## Classifier Relationship

`Ra2DocumentSemanticModelBuilder` reuses the v0.4.30 `IRa2SectionClassifier`, so Source highlighter, Project Explorer, and the language model can share the same current-document section kind inference.

## Caret Context

`Ra2CaretContextService` maps an offset to:

- section header;
- key;
- value;
- comment line;
- whitespace;
- unknown.

It returns the nearest section/key-value symbol and token text/span when applicable.

## Boundaries

This version does not implement:

- Completion UI or insertion;
- Hover UI;
- right-click menus;
- Go To Definition UI;
- Find References UI;
- Diagnostics integration;
- INI save, dirty, or edit flow;
- full-project indexing;
- ObjectAggregator / ProjectLoader / ProjectSaveService integration.

Future Hover, Completion, Definition, References, and current-section diagnostics should build on `Ra2DocumentSemanticModel` and `Ra2CaretContext`.
