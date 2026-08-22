# UI-MODERN-1 M0C Control / DPI / Automation Audit

Status: completed read-only audit  
Audit date: 2026-07-21  
Scope: `RA2IniEditor.IDE/**/*.xaml`

## 1. Executive finding

The current UI has a reusable IDE composition and a meaningful shared-style foundation, but it is not yet a complete design system. The strongest causes of native WPF appearance are incomplete template coverage, a nearly all-static theme graph, several native-chrome secondary windows, fixed geometry without explicit responsive modes, and page-local colors outside the theme authority.

The audit does not authorize production XAML edits. It defines the M1 migration order.

## 2. Repository facts

| Metric | Current value |
|---|---:|
| XAML files | 25 |
| Window roots | 18 |
| `StaticResource` references | 615 |
| `DynamicResource` references | 28 |
| Hard-coded hex color occurrences | 78 |
| AutomationId declarations | 393 |
| Button elements | 103 |
| TextBox elements | 32 |
| ComboBox elements | 13 |
| TabControl elements | 6 |
| TabItem elements | 25 |
| DataGrid elements | 29 |

`IconResources.xaml` accounts for the current dynamic-resource usage. General Shell and secondary-window styles still depend primarily on static brush resolution, so swapping a palette at runtime would not reliably refresh all existing controls.

## 3. Potential default-template exposure

The counts below identify controls without an explicit `Style` attribute. A local implicit style can reduce the actual exposure, so these are migration candidates rather than proof that every listed element currently uses the framework default.

| Control | Total | Explicit Style | Potential implicit/default |
|---|---:|---:|---:|
| Button | 103 | 94 | 9 |
| TextBox | 32 | 3 | 29 |
| ComboBox | 13 | 2 | 11 |
| CheckBox | 4 | 0 | 4 |
| TabControl | 6 | 1 | 5 |
| TabItem | 25 | 0 | 25 |
| DataGrid | 29 | 27 | 2 |
| TreeView | 1 | 0 | 1 |
| ListBox | 3 | 0 | 3 |
| MenuItem | 49 | 0 | 49 |
| ContextMenu | 1 | 0 | 1 |

Known implicit styles currently cover Shell menu items, Shell tree items, and completion list items. They do not form a complete application-wide control family.

## 4. Highest-priority visual gaps

### P0: shared theme authority

- `ShellTheme.xaml` contains both semantic brushes and control templates.
- `IdeSecondaryWindowStyles.xaml` contains a second set of hard-coded presentation colors.
- Shell, completion, Add Property, and Field Annotation surfaces add page-local colors.
- Light and dark palettes cannot currently be exchanged as one coherent resource unit.

Required M1 response: separate palette resources from style resources, convert switchable colors to dynamic resources, and add a safe light fallback.

### P0: input and navigation templates

TextBox, ComboBox, CheckBox, TabItem, TreeView, ContextMenu, and ScrollBar are the largest native-style risks. They must receive project-owned templates before Shell or Field Registry visual polishing.

### P0: standalone Search

The current Search window has no AutomationIds, uses native chrome, exposes three default CheckBoxes, and displays three hard-coded mock results. It is a baseline-only surface and must not become the implementation base for the final bottom Search workspace.

### P0: responsive ownership

Shell currently fixes the right tool well at 320 DIP and the bottom row at 190 DIP. It has a 960 x 640 minimum but no explicit Compact/Standard/Wide mode. The bottom panel is visible at startup even when it contains no useful results.

Required M2 response: introduce bounded pane state, default bottom collapse, and deterministic layout modes.

## 5. Chrome inventory

Custom chrome is already used by Field Registry Center, Field Registry Manager, Field Editor, Allowed Values, Field Learning, Quick Peek, and Peek Definition.

Native or inherited chrome remains on Search, Issues, Harvest Preview, Remote Preset, Add Property, Field Annotation, Completion Preview, Find References, Dirty Navigation, Save Preflight, and the main Shell.

Native OS file/folder pickers remain intentionally native. Project-owned workflow windows are M4/M5 modernization candidates.

## 6. DPI and layout-rounding risk

- Shell enables layout rounding only on a small icon subtree, not at the Window root.
- Secondary windows do not declare root `UseLayoutRounding` or `SnapsToDevicePixels`.
- Several dialogs use fixed width plus `SizeToContent=Height`; long localized text needs explicit wrapping and work-area clamping.
- Quick Peek and definition popups use content sizing and maximum heights, so multi-monitor placement must remain clamped after template changes.
- Field Registry Center is 1040 x 700 with an 820 x 620 minimum; its future three-column layout needs a collapsible details pane at compact widths.

Required response: root-level rounding, device-pixel snapping for borders and vector icons, DIP-based breakpoints, and real 100/125/150 percent DPI smoke tests.

## 7. Virtualization and scrolling

Shell Project Explorer explicitly enables recycling virtualization. Field Registry and diagnostics grids rely on DataGrid behavior and must preserve virtualization during retemplating. No design stage may wrap a virtualized list or grid in an outer unbounded ScrollViewer.

The AI chat has an intentional dedicated ScrollViewer. Its lifecycle and incremental rendering behavior are protected.

## 8. Automation findings

The application already has broad AutomationId coverage (393 declarations), especially on Shell and Field Registry workflows. This is a strong migration asset.

Immediate gaps:

- SearchToolWindow has zero AutomationIds.
- Modern right-tool Field tab does not exist.
- Bottom Search query, options, execute, cancel, results, and status IDs do not exist.
- Field Registry internal navigation IDs do not exist.
- icon-only controls need a consistent Name, HelpText, and ToolTip audit.

Existing IDs must be preserved unless an approved contract explicitly replaces the surface. New IDs must be stable and must not be generated from localized text.

## 9. M1 control-template order

1. palette and semantic brush authority;
2. focus visual and typography;
3. Button / ToggleButton;
4. TextBox / ComboBox / CheckBox / RadioButton;
5. TabControl / TabItem;
6. Menu / MenuItem / ContextMenu / ToolTip;
7. ScrollBar / GridSplitter / ProgressBar;
8. DataGrid / TreeView / ListView / ListBox;
9. custom and native project-owned Window chrome;
10. component gallery and light/dark/high-contrast proof.

## 10. Stop conditions carried forward

Stop M1 if a template breaks keyboard navigation, IME composition, AutomationPeer behavior, DataGrid or TreeView virtualization, popup placement, dark-theme resource resolution, or existing AutomationIds. Do not weaken tests or replace built-in control behavior with raster imagery.
