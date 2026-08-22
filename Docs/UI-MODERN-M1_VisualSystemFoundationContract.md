# UI-MODERN-M1 Visual System Foundation Final Contract

Status: Revised final contract candidate; awaiting explicit user approval  
Contract date: 2026-07-22  
Product: RA2IniEditor.IDE-only  
Current-document risk: `R0` DocsOnly  
Implementation risk after approval: `R3` — shared WPF presentation authority, Shell non-client chrome, AvalonDock floating-host chrome, keyboard/automation/DPI behavior

Authoritative predecessors:

- `Docs/UI-MODERN-1_CanonicalSurfaceVisualParityContract.md`
- `Docs/UI-MODERN-1_ControlDpiAutomationAudit.md`
- `Docs/UI-MODERN-1_WpfDimensionSpec.md`
- `Docs/UI-MODERN-1_ResponsiveLayoutSpec.md`
- `Docs/UI-DOCK-1_AvalonDockShellContract.md`
- `Docs/UI-DOCK-1_AvalonDockExactApiInventory.md`
- `Docs/UI-DOCK-4_LayoutPersistenceContract.md`

## 1. Contract correction against the current implementation

The M0 audit was accurate when captured, but UI-DOCK later changed the Search host and AvalonDock structure. M1 starts from the current code, not from the older screenshot implementation:

- Search is now the Shell-owned `SearchToolView` hosted by `Tool.Search`; the obsolete standalone Search window no longer exists.
- Search now exposes `Search.View`, query/options/results/status AutomationIds, but its query is read-only and its result model remains placeholder/mock behavior.
- Search behavior is therefore still unfinished. M1 may modernize its presentation only; real query execution, cancellation, result navigation, hierarchy, and placeholder retirement require a separate `SEARCH-1` behavior contract.
- AvalonDock 4.74.1 already owns the approved 300-DIP Right / 260-DIP Bottom structure, modern dock headers/tabs/splitters, deterministic Home recovery, and v1 user-local layout persistence.
- `ShellWindow.xaml` remains the sole compiled-default layout authority. M1 must not create a second topology or invalidate existing v1 layouts.

## 2. Functional goal

M1 establishes a project-owned modern WPF visual system and applies it to the Shell in reviewable increments. The delivered light Shell should express the approved IDE character through coherent semantic tokens, compact controls, explicit state visuals, restrained borders, crisp layout rounding, modern client chrome, and matching AvalonDock floating hosts.

The 1920 x 1080, 100% design canvas remains authoritative:

- integrated title/menu band: 30 DIP;
- main toolbar: 32 DIP;
- IDE workspace: 994 DIP;
- status bar: 24 DIP;
- editor-side workspace: 1616 DIP at 1920 width;
- right splitter/tool well: 4 + 300 DIP;
- opened bottom tool: 260 DIP under the editor column only;
- editor viewport with bottom open: 1616 x 700 DIP.

These values are design and acceptance dimensions, not a hard-coded monitor resolution. The existing responsive specification remains the later M2 authority.

## 3. Non-goals

M1 does not authorize:

- real Search implementation or replacement of its placeholder data;
- Shell panel topology, ContentId, Home, default visibility, tab order, persisted schema, or monitor recovery changes;
- automatic bottom-panel collapse, Compact/Standard/Wide behavior, overflow commands, or new responsive state management;
- Field Registry Center layout redesign;
- modernization of every secondary window;
- a runtime light/dark/high-contrast selector, theme persistence, or live palette switching;
- new menus, command search, activity rail, workspace switcher, or Visual Studio feature imitation;
- new third-party UI, theme, icon, MVVM, DI, or docking dependencies;
- raster-drawn controls, image generation, Image2, or API-key use for interactive UI;
- parser, editor, Field Registry, Completion, Hover, Quick Peek, Diagnostics, Save Preflight, backup/rollback, undo/redo, AI request/streaming/model behavior, BuiltIn data, or legacy changes.

M1 is the Shell visual foundation, not the end of the whole UI program. Responsive behavior, real Search, Field Registry restructuring, remaining secondary windows, and runtime theme switching stay separately gated.

## 4. Current implementation inventory

### 4.1 Resource composition

`App.xaml` currently merges, in order:

1. `Themes/ShellTheme.xaml`;
2. `Themes/IconResources.xaml`;
3. `Themes/IconGeometryResources.xaml`;
4. `Themes/IconImageResources.xaml`;
5. `Resources/Styles/IdeSecondaryWindowStyles.xaml`.

`ShellTheme.xaml` currently mixes palette brushes, ordinary control styles, and AvalonDock templates; separating its token ownership is the M1 refactor target. `IdeSecondaryWindowStyles.xaml` contains a second group of hard-coded presentation colors, but that debt is inventoried only and deliberately deferred from M1.

Current post-UI-DOCK targeted XAML scan (`bin/obj` excluded):

| Metric | Current value |
|---|---:|
| XAML files | 25 |
| AutomationId declarations | 416 |
| `StaticResource` references | 611 |
| `DynamicResource` references | 62 |
| Hard-coded hex-color occurrences | 86 |

These values supersede the M0 snapshot only as inventory counts; they do not supersede its visual or dimensional authority.

### 4.2 Existing reusable paths

M1 must extend, not replace:

- `ShellTheme.xaml` named compatibility styles;
- project-owned vector icon resources;
- `IdeSecondaryWindowStyles.xaml` named secondary-surface styles;
- AvalonDock `AnchorableHeaderTemplate`, `AnchorableTitleTemplate`, `AnchorablePaneControlStyle`, and splitter style;
- Shell/AvalonDock boundary tests and WPF automation harness;
- `UseLayoutRounding`, `SnapsToDevicePixels`, WPF `WindowChrome`, `SystemCommands`, and native non-client hit testing already available in .NET/WPF.

### 4.3 Current native-style exposure relevant to M1

- Shell root does not enable layout rounding or device-pixel snapping.
- Shell still uses native OS non-client chrome, so title/menu cannot occupy the contracted combined 30 DIP band.
- many palette references are `StaticResource`, preventing a coherent future palette swap;
- TextBox, ComboBox, CheckBox, Menu/ContextMenu, TreeView, ListView, DataGrid, and ScrollBar coverage is incomplete;
- Shell still contains local hard-coded colors in Project Explorer and AI surfaces;
- project-owned AvalonDock docked chrome exists, but floating hosts retain native/inconsistent outer chrome.

### 4.4 Root, DataContext, and open/show paths

| Surface | Root / DataContext | Current open/show path |
|---|---|---|
| Main Shell | `ShellWindow` / inline `ShellViewModel` | `App.Application_Startup` constructs it, assigns `MainWindow`, then calls `Show()` |
| Embedded Search | `SearchToolView` / inline `SearchToolWindowViewModel` | Shell `OpenSearchToolWindow` calls `ShowAndActivateBottomTool("Tool.Search", BottomSearchResultsTab)` |
| Field Registry regression surface | `FieldRegistryCenterWindow` / constructor-provided existing manager/provider state | Shell `OpenFieldRegistryManagerWindow` reuses/activates the current instance or constructs it with `Owner=this`, then calls `Show()` |

M1 must not replace these DataContexts, constructors, ownership paths, show/activate behavior, or view-model lifetimes.

## 5. Architecture decisions

### 5.1 One semantic token authority

Create one `Themes/IdeVisualTokens.xaml` dictionary for the light runtime palette, geometry, typography, density, and interaction-state resources. Existing `Shell*Brush` resource keys remain available as compatibility keys and move to this dictionary; they must not be redefined later in `ShellTheme.xaml` or page-local resources.

New resources use `Ui*` names. Switchable colors are consumed through `DynamicResource`. Fixed geometry and typography may use `StaticResource` after the token dictionary has loaded.

### 5.2 Keyed adoption before global implicit adoption

M1 uses named project styles and explicit adoption. It must not introduce application-wide implicit styles for every WPF control in one step. Global implicit templates could silently alter 25 XAML surfaces and make regressions difficult to attribute.

M1B/M1C define new `Ui*` keyed styles but do not rebase existing `Shell*` / `Ide*` compatibility styles and do not alter any production surface. M1D explicitly applies the new keys only in `ShellWindow.xaml` and `SearchToolView.xaml`. Existing named styles used by secondary windows remain unchanged throughout M1.

This contract deliberately defers `IdeSecondaryWindowStyles.xaml` to a later secondary-window modernization package. Merely merging keyed styles at application scope does not authorize implicit or automatic adoption.

### 5.3 Preserve native control semantics

Modern templates change presentation only. They must retain WPF required parts and built-in behavior:

- TextBox: `PART_ContentHost`, selection, clipboard, undo, IME, read-only, wrapping, and scroll behavior;
- ComboBox: `PART_Popup`, `PART_EditableTextBox`, editable/non-editable mode, keyboard selection, and bounded dropdown height;
- CheckBox: checked/unchecked/indeterminate and access-key behavior;
- RadioButton: group semantics and keyboard navigation;
- TabControl: items host and `PART_SelectedContentHost`;
- ScrollBar: orientation, track, thumb, repeat actions, and AutomationPeer behavior;
- MenuItem/ContextMenu: access keys, submenu popup, keyboard navigation, commands, and checked state;
- DataGrid/TreeView/ListView/ListBox: selection, recycling virtualization, scrolling, headers, and double-click handlers.

DataGrid and TreeView must not be wrapped in an unbounded outer ScrollViewer.

### 5.4 Light-first runtime boundary

M1 delivers and visually accepts the light runtime palette. The approved dark image remains a palette-relationship reference for a later theme-switching contract. M1 must not ship unused dark dictionaries or a hidden theme switch.

High-contrast verification in M1 is a legibility/operability smoke, not a claim of a bespoke high-contrast theme. If project templates make Windows High Contrast unusable, the stage fails and stops; a later theme contract must define the automatic palette path.

### 5.5 Custom Shell chrome is fail-closed

The compact 30-DIP integrated title/menu band requires project-owned WPF chrome. M1 may implement it only if all Windows behaviors pass:

- drag and double-click maximize/restore;
- minimize, maximize/restore, close, Alt+F4, and Alt+Space;
- Win+Arrow and taskbar restore;
- Windows 11 Snap Layout hover on the maximize region;
- correct maximized work-area bounds without covering the taskbar;
- per-monitor DPI transition and keyboard/UI Automation access.

The implementation uses `WindowChrome` and native `WM_NCHITTEST`/`HTMAXBUTTON` handling through one internal controller. It must not synthesize pointer dragging. If Snap Layout or sizing cannot be preserved, the stage stops and restores native Shell chrome; it must not deliver a visually modern but behaviorally degraded title bar.

### 5.6 AvalonDock floating chrome reuses the same window boundary

AvalonDock 4.74.1 exposes `DockingManager.LayoutFloatingWindowControlCreated` and `.LayoutFloatingWindowControlClosed`; their event args expose `LayoutFloatingWindowControl`. The base control derives from `Window`.

M1 may style floating hosts and attach the shared non-client controller through those real events. It must not fork AvalonDock, replace its layout model, implement custom drag thresholds, or modify persistence. Floating content continues to be identified by existing ContentId-based tab/header AutomationIds.

### 5.7 No second default or persistence authority

Theme resources and chrome state are presentation-only and are not serialized into `shell-layout.v1.xml`. Reset, restore, invalid-file fallback, Home recovery, geometry clamping, and content-instance ownership continue through UI-DOCK-4 unchanged.

## 6. Frozen visual tokens

M1A must provide at least the following resources. Exact resource type is part of the contract.

### 6.1 Color/brush tokens

| Key | Light value | Purpose |
|---|---|---|
| `UiCanvasBrush` | `#F4F6F8` | application canvas |
| `UiSurfaceBrush` | `#FFFFFF` | primary editor/tool surface |
| `UiSurfaceSubtleBrush` | `#F5F7FA` | chrome and subtle headers |
| `UiSurfaceHoverBrush` | `#EAF2FB` | neutral hover |
| `UiSurfacePressedBrush` | `#DCE9F7` | pressed/check state |
| `UiBorderBrush` | `#D7DCE2` | normal border |
| `UiDividerBrush` | `#E3E8EF` | low-emphasis divider |
| `UiTextPrimaryBrush` | `#202733` | primary text |
| `UiTextSecondaryBrush` | `#697386` | secondary text |
| `UiTextDisabledBrush` | `#98A2B3` | disabled text |
| `UiAccentBrush` | `#0F6CBD` | selection/focus/accent |
| `UiAccentHoverBrush` | `#115EA3` | accent hover |
| `UiAccentPressedBrush` | `#0C3B5E` | accent pressed |
| `UiAccentSoftBrush` | `#EAF4FF` | subtle accent fill |
| `UiFocusBrush` | `#0F6CBD` | 2-DIP keyboard focus |
| `UiDangerBrush` | `#B42318` | error/destructive state |
| `UiWarningBrush` | `#B54708` | warning state |
| `UiSuccessBrush` | `#107C10` | success/connected state |
| `UiSelectionBrush` | `#DCEEFF` | active selection |
| `UiSelectionInactiveBrush` | `#EEF2F7` | inactive selection |

Existing `ShellBackgroundBrush`, `ShellTopBarBrush`, `ShellPanelBrush`, `ShellBorderBrush`, `ShellPrimaryTextBrush`, `ShellMutedTextBrush`, `ShellTopChromeBrush`, `ShellMenuBarBrush`, `ShellToolbarBrush`, `ShellTopChromeInnerDividerBrush`, `ShellToolbarSeparatorBrush`, `ShellToolbarBottomBorderBrush`, `ShellAccentBrush`, and `ShellDock*Brush` keys remain resolvable and map to the corresponding semantic role.

### 6.2 Geometry and density tokens

The exact M0 values remain authoritative:

- `UiTitleMenuHeight=30`;
- `UiControlHeightCompact=28`;
- `UiControlHeightDefault=32`;
- `UiCommandRowHeight=32`;
- `UiDocumentTabHeight=30`;
- `UiToolHeaderHeight=30`;
- `UiStatusBarHeight=24`;
- `UiSplitterThickness=4`;
- `UiSpace1/2/3/4=4/8/12/16`;
- `UiCornerSmall/Medium=3/6`;
- `UiBorderThickness=1`;
- `UiFocusThickness=2`;
- `UiIconSmall/Medium=16/20`;
- `UiTreeRowHeight=24`;
- `UiGridRowHeight=26`.

Compact command hit targets are 28 x 28 DIP. No later style may reduce the clickable target merely to preserve a smaller glyph.

### 6.3 Typography tokens

- UI font family: `Segoe UI Variable Text, Segoe UI` with installed-font fallback;
- code/editor font family remains `Consolas` in M1;
- default UI text: 12 DIP;
- metadata/status: 11 DIP;
- tool title: 12 DIP semibold;
- section title: 13 DIP semibold;
- keyboard focus is never communicated by color alone.

No font file or new package is bundled.

## 7. New and modified types/files

### 7.1 New resource dictionaries

- `RA2IniEditor.IDE/Themes/IdeVisualTokens.xaml`
- `RA2IniEditor.IDE/Themes/IdeControlStyles.xaml`
- `RA2IniEditor.IDE/Themes/IdeCollectionStyles.xaml`

They are automatically compiled by the WPF SDK; no project-file edit is expected or authorized.

### 7.2 New internal controllers

#### `ShellWindowChromeController`

Internal, Shell-owned, one instance per styled Window.

Responsibilities:

- attach/detach `HwndSource` hooks;
- expose native non-client hit-test behavior for maximize/Snap Layout;
- keep maximize/restore visual state synchronized;
- invoke system menu/native window commands without replacing existing Shell closing logic;
- remove hooks deterministically when the Window closes.

Expected internal shape:

```csharp
internal sealed class ShellWindowChromeController : IDisposable
{
    internal ShellWindowChromeController(Window window, FrameworkElement dragRegion, FrameworkElement maximizeRegion);
    internal void Attach();
    internal void ShowSystemMenu(Point screenPoint);
    public void Dispose();
}
```

The class name and internal signatures may be narrowed during implementation if the exact WPF hook lifecycle requires it; any public API, extra persistent state, or alternate ownership requires a contract amendment.

#### `ShellDockFloatingChromeController`

Internal, Shell-owned, one instance per `DockingManager`.

Responsibilities:

- subscribe to floating-window-created/closed events;
- apply the approved floating host style;
- attach/dispose one `ShellWindowChromeController` per live floating host;
- avoid retaining closed hosts;
- never inspect or mutate business content.

### 7.3 Modified production files across the package

- `RA2IniEditor.IDE/App.xaml`
- `RA2IniEditor.IDE/Themes/ShellTheme.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/Views/SearchToolView.xaml`

No single Task Card may modify more than five files. The package-level list does not authorize touching all files in one card.

## 8. Public API, fields, methods, and serialized contract

- External public API change: none.
- Existing public/internal business signatures: unchanged.
- New controller types: `internal` only.
- New Shell fields: private controller references only; no static/global UI state.
- New XAML handlers: private, chrome-only, and must delegate to `SystemCommands` or the internal controllers.
- New serialized fields/files: none.
- `shell-layout.v1.xml` shape and path: unchanged.
- New package references: none.
- A PublicApiLedger entry is not required unless implementation unexpectedly introduces a public member or external serialized contract; that event is an immediate stop.

## 9. Automation contract

All current AutomationIds, including the UI-DOCK-4 frozen list and the current Search IDs, remain unchanged.

M1E adds only these Shell chrome anchors:

- `Shell.TitleBar`
- `Shell.TitleBar.SystemMenuButton`
- `Shell.TitleBar.DragRegion`
- `Shell.TitleBar.MinimizeButton`
- `Shell.TitleBar.MaximizeRestoreButton`
- `Shell.TitleBar.CloseButton`

M1F floating hosts use window-local stable anchors:

- `Shell.Dock.FloatingHost`
- `Shell.Dock.FloatingHost.MinimizeButton`
- `Shell.Dock.FloatingHost.MaximizeRestoreButton`
- `Shell.Dock.FloatingHost.CloseButton`

Content identity remains available through `Shell.Dock.Tab.{ContentId}` and `Shell.Dock.Header.{ContentId}`. Localized titles must not become AutomationIds or persistence keys.

Icon-only controls require a stable AutomationId, `AutomationProperties.Name`, and ToolTip. Existing automation peers must not be replaced with decorative-only elements.

## 10. Resource and runtime call order

### 10.1 Application startup resources

Final M1 merge order:

1. `Themes/IdeVisualTokens.xaml`;
2. `Themes/IdeControlStyles.xaml`;
3. `Themes/IdeCollectionStyles.xaml`;
4. `Themes/ShellTheme.xaml`;
5. existing icon dictionaries;
6. `Resources/Styles/IdeSecondaryWindowStyles.xaml`.

Token keys are defined only by item 1. Foundation styles are defined only by items 2/3. Shell/AvalonDock compatibility styles are defined by item 4. Secondary aliases are defined by item 6.

`IdeSecondaryWindowStyles.xaml` remains in the existing merge order but is read-only in M1. New keyed dictionaries must not override its named keys or introduce implicit styles that change its descendants.

### 10.2 Shell lifecycle

1. `InitializeComponent` loads tokens/templates and the compiled AvalonDock default.
2. Existing UI-DOCK-4 session captures/restores layout in its current order.
3. Shell creates and attaches its chrome controller after visual elements exist and before user interaction.
4. Shell creates the floating-host controller after `DockingManager` exists.
5. Floating created: apply style, attach controller, retain only a live host/controller pair.
6. Floating closed: detach and remove the pair.
7. Shell closing: existing accepted/cancelled layout-save ordering remains authoritative.
8. Shell closed/disposed: detach all Hwnd hooks and AvalonDock event handlers.

Custom close buttons call the normal Window close path; they must not bypass `ShellWindow_OnClosing`, layout save, cancellation, or floating-close Home recovery.

## 11. Continuous Task Cards and visual stops

Execution is continuous only between non-visual checks. Every card marked **Visual Stop** requires a real WPF screenshot/manual smoke and explicit user acceptance before the next card starts.

### M1A — SemanticTokenAuthority

Goal: separate semantic tokens from styles with no intended pixel change.

Allowed files, maximum four:

- new `Themes/IdeVisualTokens.xaml`;
- `App.xaml`;
- `Themes/ShellTheme.xaml`;
- new `RA2IniEditor.Tests/IDE/IdeVisualSystemBoundaryTests.cs`.

Required checks:

- all frozen resource keys resolve through an STA `Application.LoadComponent`/resource-dictionary load;
- no key is duplicated across token/style dictionaries;
- existing Shell compatibility keys remain present;
- the IDE builds its BAML without `XamlParseException`, then a real Debug smoke opens Shell, Search, Field Registry Center, and Quick Peek without resource-resolution failure;
- build and targeted visual-system boundary tests pass.

Expected visual delta: none. If a visible change occurs, stop and treat it as an unintended visual gate.

### M1B — CoreControlTemplateDefinition

Goal: define and test project-owned keyed templates for Button/ToggleButton, TextBox, ComboBox, CheckBox, RadioButton, Expander, ToolTip, Menu/MenuItem, and ContextMenu without applying them to production surfaces.

Frozen style keys:

```text
UiButtonStyle
UiAccentButtonStyle
UiIconButtonStyle
UiTextBoxStyle
UiComboBoxStyle
UiCheckBoxStyle
UiRadioButtonStyle
UiExpanderStyle
UiToolTipStyle
UiMenuStyle
UiMenuItemStyle
UiContextMenuStyle
```

Allowed files, maximum three:

- new `Themes/IdeControlStyles.xaml`;
- `App.xaml`;
- `IdeVisualSystemBoundaryTests.cs`.

Required checks:

- every style is explicitly keyed and resolves through the application merge chain;
- template-part and interaction checks in section 14 pass in an STA test host;
- no `Shell*`/`Ide*` compatibility style is rebased;
- no implicit `Style TargetType` is introduced at application scope;
- no production XAML outside `App.xaml` changes.

Expected visual delta: none. A visible application change means a key collision or implicit-style leak and is a hard stop.

### M1C — NavigationCollectionTemplateDefinition

Goal: define and test keyed templates/styles for TabControl/TabItem, TreeView/TreeViewItem, ListView/ListBox, DataGrid rows/cells/headers, ScrollBar, GridSplitter, and ProgressBar without applying them to production surfaces.

Frozen style keys:

```text
UiTabControlStyle
UiTabItemStyle
UiTreeViewStyle
UiTreeViewItemStyle
UiListViewStyle
UiListBoxStyle
UiDataGridStyle
UiDataGridRowStyle
UiDataGridCellStyle
UiDataGridColumnHeaderStyle
UiScrollBarStyle
UiGridSplitterStyle
UiProgressBarStyle
```

Allowed files, maximum three:

- new `Themes/IdeCollectionStyles.xaml`;
- `App.xaml`;
- `IdeVisualSystemBoundaryTests.cs`.

Required checks:

- every style is explicitly keyed and resolves without overriding existing named styles;
- template-part, scrolling, selection, and virtualization checks in section 14 pass;
- no existing AvalonDock style, Shell XAML, Search XAML, or secondary-window XAML changes;
- no dock geometry, model capability, AutomationId, or persistence assertion changes.

Expected visual delta: none. A visible application change is a hard stop.

### M1D — ShellAndEmbeddedSearchAdoption — Visual Stop 1

Goal: explicitly apply the new keyed styles to Shell and embedded Search, update Shell/AvalonDock compatibility presentation only where the Shell consumes it, remove Shell-local hard-coded colors, and enable root-level layout rounding.

Allowed files, maximum five:

- `Views/ShellWindow.xaml`;
- `Views/SearchToolView.xaml`;
- `Themes/ShellTheme.xaml`;
- `IdeShellBoundaryTests.cs`;
- `IdeVisualSystemBoundaryTests.cs`.

Required behavior:

- root `UseLayoutRounding=True` and `SnapsToDevicePixels=True`;
- all new control/collection styles are referenced explicitly; no global implicit adoption;
- `IdeSecondaryWindowStyles.xaml` and every secondary-window XAML remain unchanged;
- Search remains read-only placeholder behavior and its current IDs/bindings remain unchanged;
- AvalonEdit editor font, bindings, handlers, context-menu commands, caret/selection, dirty/undo behavior remain unchanged;
- Project Explorer recycling virtualization and Problems DataGrid behavior remain intact;
- dock ContentId/header/tab automation and 300/260 geometry remain unchanged;
- AI streaming/message lifecycle remains unchanged;
- no illustrative reference-image data is hard-coded.

Visual evidence:

- full maximized 1920 x 1080 Shell with editor, Right tools, and Bottom tools;
- 1280 x 800 fallback;
- Search docked and floating at the current preferred size;
- Project Explorer, Problems DataGrid, AI prompt/advanced area, menus, context menus, and focus states;
- 100% plus one available non-100% DPI scale.

The user must explicitly accept Visual Stop 1 before M1E.

### M1E — IntegratedShellChrome — Visual Stop 2

Goal: implement the 30-DIP integrated title/menu band and exact 32/24-DIP toolbar/status rows while preserving native Windows behavior.

Pre-change rollback gate:

1. run the IdeOnly clean-package script;
2. copy the clean package to `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-M1E-PreChrome.zip`;
3. record SHA-256 for that archive plus `ShellWindow.xaml`, `ShellWindow.xaml.cs`, and `ShellTheme.xaml` in the Stage Result Ledger;
4. do not edit until all hashes are recorded.

Allowed files, maximum five:

- `Views/ShellWindow.xaml`;
- `Views/ShellWindow.xaml.cs`;
- new `Views/ShellWindowChromeController.cs`;
- `Themes/ShellTheme.xaml`;
- `IdeVisualSystemBoundaryTests.cs`.

Required visual/behavior evidence:

- exact 30-DIP title/menu, 32-DIP toolbar, and 24-DIP status rows;
- 1920 x 1080 editor/right/bottom proportions still match M0;
- drag, double-click, minimize, maximize/restore, close, Alt+F4, Alt+Space, Win+Arrow, taskbar restore;
- Windows 11 Snap Layout appears from the maximize region;
- maximized content respects the active monitor work area;
- all new `Shell.TitleBar.*` AutomationIds expose stable names;
- 100%, 125%, and 150% where available; unavailable scales are recorded as manual coverage, never silently passed.

Failure rule: any missing native behavior stops at M1E and restores the pre-chrome source anchor. Native Shell chrome remains authoritative until the defect is separately resolved. Do not proceed to M1F.

### M1F — AvalonDockFloatingHostChrome — Visual Stop 3

Goal: apply project-owned outer chrome to AvalonDock floating hosts through the real 4.74.1 lifecycle without changing dock behavior.

Pre-change rollback gate:

1. run the IdeOnly clean-package script from the user-accepted M1E state;
2. copy it to `artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-M1F-PreFloatingChrome.zip`;
3. record SHA-256 for that archive plus `ShellWindow.xaml.cs`, `ShellTheme.xaml`, and `ShellWindowChromeController.cs`;
4. do not edit until all hashes are recorded.

Allowed files, maximum five:

- new `Views/ShellDockFloatingChromeController.cs`;
- `Views/ShellWindow.xaml.cs`;
- `Themes/ShellTheme.xaml`;
- `Ra2ShellIdeLayoutBoundaryTests.cs`;
- `IdeVisualSystemBoundaryTests.cs`.

Required behavior/evidence:

- float Search and one Right tool; both use approved compact outer chrome and preferred dimensions;
- move, resize, maximize/restore, drag-to-dock guide, re-dock, close-to-Home, Return Floating Tools Home, Reset Default Layout, normal close, and second startup all work;
- hidden state, content instances, AI streaming, Search results, and current layout sizes are preserved;
- no controller/event leak remains after repeated float/close cycles;
- persisted XML contains exactly the seven approved identities and no theme/business state;
- floating-host controls expose the frozen AutomationIds/names.

Failure rule: loss of drag guides, close recovery, persistence, or Window behavior stops at M1F and restores the pre-floating-chrome source anchor.

### M1G — PackageVerificationAndGovernanceClosure

Goal: run final verification after the user accepts Visual Stop 3 and flush documentation once.

Allowed documentation files:

- this contract/result ledger;
- `Docs/Codex_CurrentPhase.md`;
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`;
- product-facing user/release/smoke docs only if verified user-visible behavior requires them.

No new runtime feature is allowed in M1G.

## 12. Allowed and forbidden implementation files

Only the per-card files above are approved. The package never authorizes edits to:

- `RA2IniEditor.IDE/RA2IniEditor.IDE.csproj`, solution files, package references, or AvalonDock version;
- `RA2IniEditor.IDE/Resources/Styles/IdeSecondaryWindowStyles.xaml` and all secondary-window XAML;
- `ShellDockLayoutSession`, `ShellDockLayoutStore`, `ShellDockLayoutCoordinator`, monitor recovery, or atomic writer behavior;
- Search view model/business code;
- Field Registry view models/services/data or BuiltIn JSON;
- parser, completion, Hover, Quick Peek, diagnostics, save, editor-core, AI transport/pipeline, or model-selection code;
- legacy solution/projects;
- frozen M0 source images or their hashes;
- generated `bin`, `obj`, `.vs`, artifacts, or test output.

No file move, directory restructure, broad formatting, or unrelated cleanup is allowed.

## 13. Exact API inventory for implementation

The following current APIs/identities are the permitted integration anchors. Implementation must re-read their exact source before each card and may not guess additional AvalonDock convenience APIs.

### WPF/.NET

```text
System.Windows.Window
System.Windows.Shell.WindowChrome
System.Windows.SystemCommands
System.Windows.Interop.HwndSource
System.Windows.Interop.HwndSourceHook
SystemParameters.WorkArea / current per-monitor work-area path already used by the project
```

### AvalonDock 4.74.1

```text
AvalonDock.DockingManager.LayoutFloatingWindowControlCreated
AvalonDock.DockingManager.LayoutFloatingWindowControlClosed
AvalonDock.LayoutFloatingWindowControlCreatedEventArgs.LayoutFloatingWindowControl
AvalonDock.LayoutFloatingWindowControlClosedEventArgs.LayoutFloatingWindowControl
AvalonDock.Controls.LayoutFloatingWindowControl : System.Windows.Window
AvalonDock.Controls.LayoutAnchorableFloatingWindowControl
AvalonDock.Controls.LayoutDocumentFloatingWindowControl
```

Existing project-owned styling anchors:

```text
DockingManager.AnchorableHeaderTemplate
DockingManager.AnchorableTitleTemplate
DockingManager.AnchorablePaneControlStyle
DockingManager.DocumentHeaderTemplate
DockingManager.GridSplitterHorizontalStyle
DockingManager.GridSplitterVerticalStyle
```

Forbidden guesses:

- no `Xceed.Wpf.AvalonDock.*` API;
- no AvalonDock 5.x theme/MVVM/DI API;
- no assumed `FloatingWindowStyle` property on `DockingManager`;
- no writable `IsFloating` assumption;
- no custom layout serializer or alternate ContentId registry;
- no custom pointer capture/drag threshold;
- no localized-title identity.

## 14. Test and verification contract

### 14.1 Template-part and behavior matrix

M1B/M1C do not need a production component gallery. `IdeVisualSystemBoundaryTests` must load each keyed style in an STA WPF host, create a real control, call `ApplyTemplate`, and verify the applicable minimum contract below.

| Control family | Required template/structure check | Required behavior check |
|---|---|---|
| Button / ToggleButton | content presenter remains available; focus chrome does not replace content | command/click, Enter/Space, disabled, checked/indeterminate where applicable |
| TextBox | `PART_ContentHost` exists | input, selection, copy/paste, undo, read-only, multiline, wrapping, Chinese IME manual smoke |
| ComboBox | `PART_Popup`; `PART_EditableTextBox` when editable | keyboard open/select/close, editable/non-editable, bounded dropdown |
| CheckBox | content plus three-state glyph path | Space/access key, checked/unchecked/indeterminate |
| RadioButton | content plus selected glyph path | group exclusivity and arrow/Space behavior |
| Expander | header and expandable content presenters remain connected | keyboard toggle and collapsed-content exclusion |
| Menu / MenuItem / ContextMenu | submenu `PART_Popup` where applicable; header presenter recognizes access keys | Alt/access keys, arrows, Enter, Esc, commands, checked state |
| ToolTip | content presenter and bounded padding | placement remains WPF-owned; no focus capture |
| TabControl / TabItem | `PART_SelectedContentHost` and items host | selection, Ctrl+Tab/arrow behavior where currently supported, focus visibility |
| TreeView / TreeViewItem | hierarchical items presenter and ScrollViewer remain bounded | expand/collapse, selection, recycling virtualization on Project Explorer after M1D |
| ListView / ListBox | items presenter/ScrollViewer remain bounded | selection, keyboard navigation, virtualization where enabled |
| DataGrid | column-header presenter and internal ScrollViewer remain present | headers, sorting where enabled, full-row selection, read-only, double-click handler, virtualization |
| ScrollBar | `PART_Track` with Thumb and repeat actions | both orientations, pointer drag, keyboard/page movement |
| GridSplitter | resize cursor/hit target without replacing resize behavior | horizontal/vertical resizing and preview behavior |
| ProgressBar | `PART_Track` and `PART_Indicator` | minimum/maximum/value and determinate clipping |

Additional rules:

- a template-part test is not sufficient when a behavior test is listed;
- `ApplyTemplate` success is not equivalent to keyboard/IME/virtualization success;
- no production-only test hook, fake control, or raster comparison is allowed;
- M1D reruns the relevant tests against the actual Shell/Search adoption and performs the listed real-UI smoke.

### 14.2 Per-card automated gate

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~IdeVisualSystemBoundaryTests|FullyQualifiedName~Ra2ShellIdeLayoutBoundaryTests|FullyQualifiedName~IdeShellBoundaryTests|FullyQualifiedName~WpfAutomationHarnessBoundaryTests"
```

The filter may be narrowed to the files actually affected by a card, but may not omit an affected existing boundary suite merely because a new test passes.

### 14.3 Package gate

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

### 14.4 Required manual matrix

- 1920 x 1080 at 100%: exact M0 proportions and all tool regions;
- 1280 x 800: no clipping or editor collapse below 560 DIP;
- 125% and 150% where available: crisp 1-DIP dividers, popup clamp, title hit testing, and no text clipping;
- keyboard: Alt menu access, Tab/Shift+Tab, arrow navigation, Space/Enter, Esc, ContextMenu key;
- Chinese IME in AI prompt and any editable Shell input;
- Project Explorer/DataGrid virtualization and scrolling;
- dock split, float, re-dock, close-to-Home, Reset, normal close/restart persistence;
- Windows chrome and Snap behavior listed in M1E;
- AutomationId/Name inspection for new icon-only chrome controls;
- Windows High Contrast smoke for visibility and operability.

Screenshots must be from the real WPF application. Generated reference art is not acceptance evidence.

## 15. Test-update rule

Existing tests contain exact presentation strings such as old color literals, 24-DIP icon buttons, and `StaticResource` references. A card may update an assertion only when the assertion encodes presentation intentionally superseded by this contract.

It is forbidden to weaken or delete assertions for:

- seven ContentIds and exact count;
- 300/260 compiled geometry;
- document non-close/non-float/non-move capabilities;
- Shell command handlers and editor event bindings;
- stable AutomationIds;
- Search/AI/diagnostics bindings and lifecycle;
- UI-DOCK-4 restore/save/reset/fallback behavior;
- no legacy/project/dependency restoration.

## 16. Stop conditions

Stop immediately and flush a partial Stage Result Ledger if any occurs:

- missing/duplicate resource key, `XamlParseException`, or dictionary-order dependency;
- keyboard, IME, clipboard, focus, AutomationPeer, or access-key regression;
- DataGrid/TreeView/ListView virtualization or scrolling regression;
- editor text/caret/selection/dirty/undo behavior changes;
- dock ContentId, topology, Home, capability, geometry, restore/save, or monitor behavior changes;
- floating drag guide, re-dock, close-to-Home, Reset, or second-startup failure;
- Snap Layout, maximize/work-area, system menu, or per-monitor DPI failure;
- screenshot mismatch in hierarchy, density, clipping, focus, or editor dominance;
- an implementation card exceeds five files or requires a new dependency/public API;
- fixing a failed gate would require Search, AI, parser, Field Registry, editor-core, or persistence changes.

No failed visual stage may be followed by ad-hoc polishing or the next Task Card.

## 17. DeepSeek boundary

No DeepSeek call is planned for M1. Shared WPF templates, non-client Win32 behavior, AvalonDock floating lifecycle, and screenshot parity are high-context R3 integration work and are not suitable for low-context delegated generation.

If the user later explicitly requests DeepSeek for a bounded test or documentation subtask, Codex must first provide an Exact API Inventory and a separate task package. DeepSeek may not design templates, public API, lifecycle, or chrome architecture.

## 18. Decision record

Proposed decisions embedded in this contract:

- one light semantic token authority;
- template definition and template adoption are separate cards;
- keyed Shell/Search-only adoption instead of global implicit restyling or compatibility-style rebasing;
- secondary-window styles remain frozen during M1;
- modern control templates preserve native WPF semantics;
- custom Shell/floating chrome fails closed to native chrome when Windows or docking behavior cannot be preserved;
- each Chrome card requires a clean-source rollback archive and recorded file hashes before editing;
- current v1 layout remains presentation topology only and is not version-bumped;
- Search mock/placeholder retirement is a separate behavior stage.

Rejected alternatives:

- MahApps, ModernWpf, FluentWPF, AvalonDock theme add-ons, or AvalonDock upgrade;
- Image2/raster controls;
- copying Visual Studio controls/features verbatim;
- a one-pass rewrite of all 25 XAML files;
- visually reviewing templates before a production surface actually consumes them;
- rebasing shared secondary-window styles inside the Shell foundation package;
- hidden unused dark-theme resources;
- parallel hand-built dock topology or persistence schema.

No separate DecisionLog exists in the current project. This contract is the proposed decision record; it becomes Accepted only after the user confirms it.

## 19. Self-review against rework risk

The plan is considered implementation-ready because:

- it reconciles the obsolete M0 Search-host facts with the current UI-DOCK implementation;
- token ownership precedes template ownership, templates are verified before adoption, and the first visual stop occurs only after Shell/Search consume them;
- every card stays within the five-file budget;
- M1B/M1C are explicitly pixel-neutral and cannot affect secondary windows through implicit styles or compatibility rebasing;
- every intended visible change has a mandatory user stop and real-WPF evidence;
- uncertain native/floating chrome behavior is fail-closed rather than assumed;
- both Chrome stages have clean-source rollback anchors and pre-edit hashes;
- UI-DOCK-4 identities, content instances, persistence, and recovery remain explicit regression gates;
- native input, accessibility, virtualization, DPI, and High Contrast are acceptance criteria, not afterthoughts;
- Search behavior and later full-UI stages are explicitly deferred instead of being hidden inside styling work;
- no public API, new dependency, image-generation path, or project-file change is needed.

Residual uncertainty cannot be removed by document design alone: Windows 11 Snap Layout and AvalonDock drag guides require real interactive verification. The contract treats either failure as a hard stop, preventing downstream rework.

## 20. Approval gate

No production XAML, C#, resource dictionary, test, project, or package change may begin until the user explicitly confirms:

```text
确认 UI-MODERN-M1 最终契约
```

After confirmation, execution starts at M1A only. It must not skip directly to Shell chrome or floating-host chrome.
