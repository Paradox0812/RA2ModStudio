# RA2IniEditor IDE Handoff v0.4.22B

## Version

- Target: v0.4.22B Field Import Preview UX Polish + Effective Registry Diff
- Baseline: v0.4.22A Field Registry Harvest Preview UI

## Completed

v0.4.22B renames the user-facing harvest preview experience to Field Import Preview and adds a readonly diff against the current effective field registry provider.

The user flow is:

```text
Field Registry Manager
  -> Open Field Import Preview
  -> Insert Sample or paste field documentation
  -> Parse & Preview
  -> inspect Preview Diff, Parsed Fields, Validation Issues, Field Drafts, Parse Warnings
```

## UX Changes

User-facing wording changed from Harvest Preview to Field Import Preview:

- Window title: `Field Registry Import Preview - Preview Only`
- Entry button: `Open Field Import Preview`
- Input header: `Paste Field Documentation / INI-like Text`
- Tabs: `Preview Diff`, `Parsed Fields`, `Validation Issues`, `Field Drafts`, `Parse Warnings`

The preview window also adds an `Insert Sample` button. The sample covers:

- INI-like lines
- Bullet list entries
- Markdown table rows

`Insert Sample` does not automatically parse. Users still explicitly click `Parse & Preview`.

## Effective Registry Diff

New internal Infrastructure diff contracts:

- `FieldRegistryHarvestDiffKind`
- `FieldRegistryHarvestDiffRow`
- `FieldRegistryHarvestDiffResult`
- `IFieldRegistryHarvestDiffService`
- `FieldRegistryHarvestDiffService`

Diff compares:

```text
FieldRegistryHarvestPreviewDraft.Definitions
  vs
current effective IRa2FieldDefinitionProvider
```

The current effective provider remains:

```text
Project Local > Global Local > BuiltIn
```

The preview window receives a provider accessor from `ShellWindow`, so `Parse & Preview` uses the current provider without triggering reload.

## Diff Kinds

- `Added`: no effective field exists for key + appliesTo.
- `Same`: effective field exists and key fields match.
- `Changed`: effective field exists but editor kind, source kind, or description differs.
- `Conflict`: reserved for future use.
- `Invalid`: defensive row for preview definitions that cannot be compared.

Description comparison treats null, empty, and whitespace-only descriptions as equivalent after trim.

## Explicitly Not Implemented

- GitHub fetch
- Network access
- Active pack enumeration for provenance
- `active/*.fields.json` writes
- Apply / rollback
- Active pack backup
- Field registry editor
- Completion
- Save / dirty / INI editing
- Auto reload highlighter provider
- Auto reload active field registry
- ProjectSaveService integration

## Tests

Added and updated tests cover:

- Diff Added / Same / Changed / Invalid.
- Description null / empty equivalence.
- Insert Sample behavior.
- ViewModel diff row generation against an empty effective provider.
- Clear resetting diff rows and counts.
- User-facing Field Import Preview wording.
- Guardrails against network, file writes, active writes, apply/rollback commands, backup, ProjectSaveService, and Completion.

## Next Suggested Step

Prefer v0.4.23A: Active Pack Provenance Read Model.

This would keep behavior readonly while allowing diff rows to show whether an effective field comes from Project, Global, or BuiltIn before any apply/rollback design is attempted.
