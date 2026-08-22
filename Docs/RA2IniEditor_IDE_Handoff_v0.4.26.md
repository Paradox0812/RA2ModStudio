# RA2IniEditor IDE Handoff v0.4.26

## Version

v0.4.26 Field Registry Import UX Polish

## Scope

This version polishes the existing Field Registry Import / Apply / Rollback experience. It does not add GitHub fetch, Completion, INI save, dirty state, editable source text, batch rollback, or multi-target pack selection.

## Field Import Preview UX

The initial preview status now explains supported input formats and the explicit apply boundary:

- INI-like lines
- bullet list entries
- markdown table rows

The UI now shows a three-step flow:

1. Parse raw text
2. Review diff and validation issues
3. Build an apply plan and confirm

Apply also exposes a dedicated disabled reason:

- no preview generated;
- no apply plan built;
- no add/update operations;
- plan contains errors;
- plan contains rejected items;
- project target requires an open project folder.

Apply success is displayed as a readable multi-line summary with target, manifest, added, updated, and skipped counts.

## Field Registry Manager UX

The manager now explains active pack priority:

```text
Project active fields > Global active fields > BuiltIn fields
```

It also clarifies that `UnknownKey` means the key is absent from the effective registry and is not automatically an INI validity error.

Warnings now have an empty-state text:

```text
No warnings.
```

Recent Import Backups explains that rollback restores the target active pack from a manifest, does not modify INI files, and does not contact the network.

## UIA Diagnostics

UIA smoke diagnostics now include:

- top-level window list on window lookup failure;
- a compact automation tree when a control AutomationId cannot be found;
- ApplyStatusText, ApplyDisabledReason, and ApplySummaryText when Apply stays disabled.

The apply -> rollback UIA smoke still runs only when:

```powershell
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test RA2IniEditor.UiAutomationTests -c Release
```

## Guardrails

Still not implemented:

- GitHub fetch
- network access
- Completion
- field registry editor
- INI save / dirty / edit chain
- ProjectSaveService integration
- ObjectAggregator / ProjectLoader integration
- batch rollback
- multi-target pack selection
