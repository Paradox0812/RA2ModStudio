# RA2IniEditor.IDE FR-DQ-3J-Fix1 Hover Clamp / Icon Center Report

## 1. Phase completed

3J-Fix1: Compact hover overflow clamp and Project Explorer icon centering.

## 2. Goal

Fix two UI polish regressions reported after 3J:

1. Source Editor compact hover could overflow horizontally when the caret/mouse was near the right side of the window.
2. Project Explorer glyph icons were not visually centered inside their 18x18 badge containers.

## 3. Modified files

- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `Docs/RA2IniEditor_FR_DQ_3J_Fix1_HoverClampIconCenter_Report.md`

## 4. Hover changes

The Source Editor hover still uses the compact VS-style row:

```text
名称 | 类型 | 注释 | 来源
```

But the popup is now width-constrained and position-clamped inside the window:

- Added minimum / maximum hover width constants.
- Changed popup placement from `MousePoint` to window-relative placement.
- Clamped horizontal offset so the popup does not extend past the right edge of the window.
- Set fixed outer column widths for name / type / source and star width for comment.
- Replaced horizontal `StackPanel` cells with grid cells so text trimming is actually constrained.
- Kept `TextTrimming=CharacterEllipsis` on each value cell.

This avoids the long `VeteranSpeed` description stretching the popup outside the editor area.

## 5. Project Explorer icon changes

The glyph badge remains lightweight, but alignment is now stricter:

- Badge keeps a fixed `18x18` container.
- Text glyph uses fixed `16x16` size.
- Added `LineHeight=16` and `LineStackingStrategy=BlockLineHeight`.
- Added `TextAlignment=Center` and explicit horizontal / vertical alignment.
- Right tool well tab glyphs and AI panel header glyph are vertically centered.

## 6. Not changed

No changes were made to:

- BuiltIn field registry JSON
- Hover semantic provider
- Completion commit logic
- Diagnostics logic
- Save / Revert behavior
- AI provider / PromptBuilder
- Field Registry runtime

## 7. Static checks

- `ShellWindow.xaml` XML parse: passed.
- Source package excludes `bin/`, `obj/`, `.vs/`, `TestResults/`, `artifacts/`.

`dotnet build/test` were not run in this environment. Please validate locally with:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```

## 8. Manual smoke checklist

After publish, verify:

1. Hover `VeteranSpeed` or any long description near the right side of the editor.
2. Hover width stays within the app window.
3. Long comment text is ellipsized in the `注释` column.
4. Project Explorer glyphs are visually centered in their badges.
5. `Section` / `AI` tab glyphs are vertically aligned with text.
