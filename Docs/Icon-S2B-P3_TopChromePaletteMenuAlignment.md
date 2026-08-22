# Icon-S2B-P3 Top Chrome Palette and Menu Alignment Polish

## 0. Context

After Icon-S2B-P2, the main toolbar is more compact and the icon button chrome is improved.

Manual review found two remaining visual issues:

```text
1. The icon toolbar layer uses a color too close to the default border / divider color, causing the top chrome and content boundary to feel visually odd.
2. The middle menu row text alignment feels off; the menu text does not sit naturally in the row.
```

This phase is a small visual polish task focused only on the top chrome palette and menu row alignment.

---

## 1. Goal

Make the top chrome feel like one coherent IDE header area:

```text
Title/system edge
Menu row
Toolbar row
Content boundary
```

The menu row and toolbar row should have a natural color relationship, and the menu text should be vertically aligned and visually centered.

This phase must not change command behavior, icon geometry, AutomationIds, or toolbar command set.

---

## 2. Visual Problems to Fix

### 2.1 Toolbar color / boundary color conflict

Current problem:

```text
The toolbar background or toolbar separator is too close to default panel border color.
The boundary between toolbar and content feels visually mixed with the app default border.
```

Required improvement:

```text
1. Make toolbar background clearly a top chrome surface, not a border line.
2. Make toolbar bottom divider a distinct but subtle line.
3. Keep menu row and toolbar row within the same light neutral family.
4. Avoid strong contrast or heavy borders.
```

### 2.2 Menu row text alignment

Current problem:

```text
The middle menu row text appears vertically misaligned or visually cramped.
```

Required improvement:

```text
1. Menu text should be vertically centered within the menu row.
2. Menu row height should be stable and compact.
3. MenuItem padding should be consistent.
4. Text should not appear too high/low relative to row baseline.
```

---

## 3. Recommended Palette Adjustment

Use a clearer but still subtle top chrome hierarchy.

Suggested palette:

| Token | Suggested value | Purpose |
|---|---:|---|
| TopChrome.Background | `#F5F7FA` | Shared top chrome base |
| MenuBar.Background | `#F8FAFC` | Slightly lighter menu row |
| ToolBar.Background | `#EEF2F6` | Slightly more distinct toolbar row |
| ToolBar.BottomBorder | `#D5DCE5` | Clearer divider to content |
| TopChrome.InnerDivider | `#E3E8EF` | Soft divider between menu and toolbar |
| Content.Background | `#FFFFFF` | Main content |
| Panel.Border | `#D8DEE6` | Panel splitter / borders |
| Icon.Normal | `#2F3A45` | Existing normal icon |
| Icon.Disabled | `#9AA6B2` | Disabled icon |

Important:

```text
Do not make toolbar and bottom border the same color.
Do not make the menu row look like a separate unrelated strip.
Do not use strong blue/accent backgrounds for normal toolbar.
```

If exact colors need to be adapted to existing theme resources, keep the same visual relationship:

```text
Menu row slightly lighter
Toolbar row slightly darker
Divider line distinct but soft
Content clean white
```

---

## 4. Menu Alignment Requirements

Menu bar style should ensure:

```text
Height approximately 24-26 px.
MenuItem vertical content alignment = Center.
MenuItem padding balanced, e.g. horizontal 8-10, vertical 3-4.
Menu text baseline appears optically centered.
No extra top/bottom border squeezing text.
```

If the current MenuItem template uses default WPF styles, adjust only the scoped Shell/menu style, not global app behavior unless already scoped.

---

## 5. Toolbar Density Preservation

Keep the improvements from Icon-S2B-P2:

```text
Toolbar remains compact.
Toolbar icons remain 16x16.
Button hit target remains usable.
Default button border remains transparent/flat.
Hover/pressed/focus/disabled states remain visible.
```

Do not revert to heavy WPF default button chrome.

---

## 6. Files Allowed

Only modify:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Themes/ShellTheme.xaml
RA2IniEditor.IDE/Themes/IconResources.xaml, only if brush token mapping needs tiny adjustment
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Prefer implementing palette/alignment in `ShellTheme.xaml`.

---

## 7. Strictly Forbidden

Do not:

```text
Change toolbar command set
Change toolbar order
Change click handlers
Change AutomationIds
Change ToolTips
Change icon geometry semantics
Replace additional icons outside main toolbar
Modify AI Assistant behavior
Modify Field Registry behavior
Modify Section Tree / Project Explorer behavior
Add PNG / SVG runtime assets
Modify solution / project files
Restore legacy IDs
```

---

## 8. Test Requirements

Boundary tests should verify:

```text
1. Main toolbar AutomationIds still exist.
2. Menu entries still exist.
3. Legacy IDs are not restored.
4. Toolbar command handlers still appear wired.
5. Icon resource keys still resolve.
6. Save / Undo / Redo / Revert state rules still pass.
```

Avoid:

```text
pixel-perfect color assertions
exact height assertions unless existing tests already use stable structure-level values
```

If style tests are added, they should be structural and non-brittle.

---

## 9. Manual Smoke Checklist

After implementation, manually check:

```text
1. Menu row text appears vertically centered.
2. Menu row and toolbar row feel like one coherent top chrome.
3. Toolbar row no longer blends awkwardly with default border color.
4. Toolbar bottom divider is visible but subtle.
5. Icons remain readable.
6. Toolbar remains compact.
7. Hover / pressed / disabled states remain clear.
8. Content area still separates cleanly from top chrome.
9. No command behavior changes.
```

---

## 10. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: Icon-S2B-P3.
2. Files changed.
3. Palette/chrome changes.
4. Menu alignment changes.
5. Tests updated.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no command/AutomationId/icon semantic behavior changed.
11. Manual smoke result or remaining visual risks.
12. Recommended next phase.
```
