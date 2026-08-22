# UI-MODERN-PROGRAM-R1 M6-B Zero-Reference Audit

Status: Completed  
Date: 2026-07-23  
Authority: `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md`

## 1. Scope

M6-B audited application-level WPF resources and retired only:

- `IdeSecondary*` compatibility aliases whose consumers could adopt an existing canonical style without visual or behavioral change;
- Shell resource keys proven to have no production reference and no remaining positive contract assertion.

The audit did not redesign any surface. `ShellWindow.xaml`, Dock topology, ContentIds, AutomationIds, bindings, handlers, ViewModels, services and product semantics remained frozen.

## 2. Audit method

The audit covered:

- every production `.xaml` and `.cs` file under `RA2IniEditor.IDE`, excluding `bin` and `obj`;
- `StaticResource`, `DynamicResource`, `BasedOn`, implicit styles and direct C# `FindResource` / `TryFindResource` paths;
- dynamic `Icon.*` lookup through `IconKeyToDrawingImageConverter`;
- boundary-test assertions and accepted visual-contract documents;
- exact AutomationId, binding and Click-handler sets compared with the M6-B pre-change rollback archive.

Final application resource result:

```text
Merged dictionaries: 10
Explicit application dictionary keys: 379
Duplicate explicit keys: 0
Production IdeSecondary occurrences: 0
```

## 3. Removed Shell keys

The following 14 keys were removed from `Themes/ShellTheme.xaml`:

```text
ShellPanelStyle
ShellPanelTitleStyle
ShellToolbarButtonStyle
IdeBottomToolSurfaceStyle
IdeToolCommandButtonStyle
IdeToolSearchCommandBarStyle
IdeToolFilterStripStyle
IdeToolResultHeaderStyle
IdeToolResultHeaderTextStyle
IdeToolResultCellTextStyle
IdeToolStatusBarStyle
FileSwitcherListBoxStyle
SearchQueryTextBoxStyle
SearchResultsListViewStyle
```

`IdeToolResultListStyle` was initially a zero-reference candidate, but the first targeted verification proved that it owns the accepted `UiListViewStyle` inheritance contract. It was restored immediately and is not part of the cleanup.

## 4. Protected resources

The following dormant or indirect contract keys remain:

```text
IdeToolResultListStyle
IdeDocumentHeaderStyle
IdeToolWindowHeaderStyle
IdeToolWindowTitleStyle
IdeBottomToolCommandBarStyle
IdeBottomToolDataGridStyle
IdeSplitterStyle
IdeToolWindowTabControlStyle
IdeToolWindowDataGridStyle
IdeCompactTextBoxStyle
IdeCompactComboBoxStyle
IdeCompactButtonStyle
IdeEmptyStateTextStyle
```

Also protected:

- all `Ui*` design tokens and base control styles;
- all `Icon.*`, `IconGeometry.*` and `IconBrush.*` resources;
- all implicit styles;
- accepted AI, Field Registry, editor-assist and Dock resource vocabularies.

Zero explicit production references alone are not sufficient evidence to remove a frozen or dynamically addressed resource.

## 5. Compatibility retirement

Four application-level aliases and ten window-local alias definitions were retired.

| Compatibility key | Final canonical form |
|---|---|
| `IdeSecondaryCommandButtonStyle` | `IdeFieldRegistryCommandButtonStyle` |
| `IdeSecondaryDataGridStyle` | `IdeFieldRegistryDataGridStyle` |
| `IdeSecondaryWindowPanelStyle` | Removed; its two local definitions had no consumer |
| `IdeSecondaryHeaderStyle` | Existing `Margin="0,0,0,10"` applied directly |
| `IdeSecondaryTitleTextStyle` | `IdeAssistTitleTextStyle` plus existing `TextWrapping="Wrap"` |
| `IdeSecondaryDescriptionTextStyle` | `IdeAssistMutedTextStyle` |
| `IdeSecondaryToolbarStyle` | Existing `Margin="0,8,0,0"` applied directly |

The migration changed no AutomationId, binding, handler or collection virtualization setting. After the consumer migration, `Resources/Styles/IdeSecondaryWindowStyles.xaml` and its `App.xaml` merge entry were removed.

## 6. Verification evidence

```text
Static XAML/resource audit: passed
Modified XAML parse: 7/7
AutomationId/binding/Click sets: unchanged for all five migrated windows
Debug solution build: passed, 0 warnings, 0 errors
Affected boundary tests: 64/64 passed
Full non-UI tests: 2332/2332 passed
Hidden real startup smoke: main window handle obtained; no early process exit
```

The hidden startup process was force-terminated after the smoke because hidden `CloseMainWindow` did not request a graceful close. This is teardown behavior, not an application startup failure.

## 7. Next entry

`M6-C Final Closure`: full final verification, screenshot index, governance closure and final clean source package.

