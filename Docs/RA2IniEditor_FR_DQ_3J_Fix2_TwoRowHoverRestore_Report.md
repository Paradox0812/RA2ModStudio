# RA2IniEditor.IDE FR-DQ-3J-Fix2 Two-Row Hover Restore Report

## Summary

This patch restores the Source Editor hover popup from the 3J single-line `名称 | 类型 | 注释 | 来源` layout to the earlier compact two-row card style requested by the user.

The goal is to keep the hover small and readable while avoiding the horizontal overflow seen in 3J and 3J-Fix1.

## Modified Files

- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `Docs/RA2IniEditor_FR_DQ_3J_Fix2_TwoRowHoverRestore_Report.md`

## Hover Layout

The Source Editor hover now uses:

1. Header row: `类型 字段名 [显示名]`
2. Description row: compact green description, wrapped and height-limited
3. Metadata row: `示例 / 来源 / 适用` when present

This restores the visual direction shown by the earlier two-row hover screenshot, for example:

```text
Float VeteranCombat
一星兵种攻击力为原来的1.1倍
示例 1.1    来源 Global    适用 Global
```

## Overflow Protection

The popup still keeps the 3J-Fix1 placement and clamp behavior:

- Window-relative popup placement.
- Width clamped to available window width.
- Reduced hover width range: `280..440`.
- Description text wraps and is clipped to avoid oversized popups.
- Header uses an internal Grid so long names can trim instead of forcing the popup wider.

## Not Changed

This patch does not change:

- BuiltIn field registry data.
- Hover provider semantics.
- Diagnostics.
- Completion.
- Field Quick Peek.
- Save or dirty state flows.
- Project Explorer icon glyphs.

## Validation Performed

Static validation only in the current environment:

- Source package updated from `3J-Fix1` baseline.
- No build output added.
- No `bin/`, `obj/`, `.vs/`, `TestResults/`, or `artifacts/` folders included.

Please run locally:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```

