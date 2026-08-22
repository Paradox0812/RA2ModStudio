# UI-MODERN-1 M0D WPF / DIP Dimension Specification

Status: visual contract; production XAML not started  
Baseline date: 2026-07-21  
Unit: WPF device-independent pixel (`1 DIP = 1/96 inch`)

## 1. Contract boundary

This document converts the frozen visual references into deterministic WPF layout values. It governs geometry and density only. Runtime text, counts, field values, search results, project state, and AI content continue to come from existing services.

The primary visual target uses a compact 30 DIP title/menu band followed by a 32 DIP toolbar, matching the user-supplied Visual Studio proportion reference. Achieving this density requires a project-owned WPF chrome/template in the later Shell implementation stage. That implementation must preserve Windows drag, double-click maximize/restore, minimize/maximize/close, system menu, Snap Layout, keyboard menu access, automation, and per-monitor DPI behavior.

## 2. Shared geometry tokens

| Token | Value | Purpose |
|---|---:|---|
| `UiTitleMenuHeight` | 30 | integrated title/menu command band |
| `UiControlHeightCompact` | 28 | compact toolbar and filter controls |
| `UiControlHeightDefault` | 32 | standard inputs and primary actions |
| `UiCommandRowHeight` | 32 | toolbar row |
| `UiDocumentTabHeight` | 30 | document tab strip |
| `UiToolHeaderHeight` | 30 | right/bottom tool headers |
| `UiStatusBarHeight` | 24 | Shell and Field Registry status row |
| `UiSplitterThickness` | 4 | resizable panel boundary |
| `UiSpace1 / 2 / 3 / 4` | 4 / 8 / 12 / 16 | spacing scale |
| `UiCornerSmall / Medium` | 3 / 6 | compact control and card radius |
| `UiBorderThickness` | 1 | normal divider/focus-neutral border |
| `UiFocusThickness` | 2 | keyboard focus cue |
| `UiIconSmall / Medium` | 16 / 20 | vector icon boxes |
| `UiTreeRowHeight` | 24 | project and hierarchy tree rows |
| `UiGridRowHeight` | 26 | dense Field Registry rows |

Minimum interactive hit target is 28 x 28 DIP for compact IDE commands. Text and vector geometry may be smaller inside that hit box.

## 3. Shell reference frame

The default design resolution is exactly 1920 x 1080 at 100% scaling, as requested by the user. This is a visual acceptance canvas, not a hard-coded runtime window size. At runtime WPF uses the active monitor work area and effective DIP size; taskbar, scaling, and per-monitor changes are handled responsively.

### 3.1 Fullscreen rows and nested workspace

| Root row | Height | Ownership |
|---|---:|---|
| Integrated title/menu band | 30 | spans complete window width |
| Main toolbar | 32 | spans complete window width |
| IDE workspace | 994 | editor column plus full-height right tool well |
| Status bar | 24 | spans complete window width |
| **Design canvas total** | **1080** | |

Within the 994 DIP IDE workspace, the editor column contains a 30 DIP document tab strip, a 700 DIP editor viewport, a 4 DIP horizontal splitter, and a 260 DIP opened bottom tool. If the bottom tool is collapsed, the editor viewport grows to 964 DIP.

### 3.2 Fullscreen workbench columns

| Column | Width |
|---|---:|
| Editor-side workspace | remaining, 1616 at 1920 width |
| Right splitter | 4 |
| Right tool well | 300 |

The editor-side workspace owns approximately 84.2% of the full width. The 300 DIP right tool well spans the complete 994 DIP workspace height, including the vertical range beside the bottom tool. The bottom tool is nested only inside the 1616 DIP editor-side column and never spans beneath the right tool.

### 3.3 Shell sizing rules

- The title/menu band, toolbar, document tabs, tool headers, and status bar are fixed-height rows.
- The editor uses star sizing and owns all remaining width and height.
- The right and bottom tool regions are user-resizable, with min/max clamps defined by the responsive specification.
- Maximizing or moving to a larger monitor must allocate added width and height to the editor first. Right and bottom tool panels retain their preferred DIP sizes instead of scaling with the window.
- Fullscreen preferred sizes are 300 DIP for the right tool well and 260 DIP for an opened bottom tool. Their soft maximums are 340 and 300 DIP unless the user explicitly resizes them.
- Root Shell content must use `UseLayoutRounding="True"` and `SnapsToDevicePixels="True"`; thin dividers must land on whole DIPs.
- Scrollbars, menus, tabs, buttons, text inputs, and tool headers use project-owned templates and dynamic theme resources.

## 4. Search workspace dimensions

Search becomes an embedded bottom tool surface. It is not a second primary Shell and must not duplicate the top-level title bar or global command rows.

Within the 260 DIP bottom panel:

| Search row | Height | Notes |
|---|---:|---|
| Query / filter toolbar | 40 | 8 DIP horizontal padding; 28 DIP controls |
| Result column header | 28 | file/location/match hierarchy header |
| Virtualized result viewport | 168 | receives extra height when panel grows |
| Search status row | 24 | result/progress/error state |
| **Panel total** | **260** | |

Toolbar width allocation at the fullscreen reference:

- query box: 420 DIP preferred, 240 DIP minimum, star-grow enabled;
- filter controls: 120 DIP preferred each;
- icon commands: 28 x 28 DIP;
- gaps: 8 DIP;
- right-side result/progress summary: auto width, 12 DIP trailing padding.

Result hierarchy uses 26 DIP match rows and 28 DIP file headers. Long paths and match previews trim with tooltip access; they do not force horizontal Shell growth.

## 5. Field Registry dimensions

The primary Field Registry Center reference frame is 1040 x 700 DIP, matching the current center window. Minimum is 820 x 620 DIP. Unlike the Shell, this surface already owns custom chrome, so the row totals below include its complete WPF layout.

### 5.1 Standard rows

| Row | Height |
|---|---:|
| Custom title / primary command header | 52 |
| Provider-priority strip | 44 |
| Main registry workspace | 580 |
| Status row | 24 |
| **Window total** | **700** |

### 5.2 Standard workspace columns

The workspace has 12 DIP left/right padding, leaving 1016 DIP usable width:

| Column | Width |
|---|---:|
| Internal navigation | 156 |
| Splitter | 4 |
| Field list / daily work surface | 552 |
| Splitter | 4 |
| Read-only details | 300 |
| **Usable total** | **1016** |

The internal navigation owns Overview, Fields, Sources, Import & Learn, and Backup & Rollback destinations. This is navigation within the existing Field Registry feature, not a new semantic layer.

### 5.3 Registry density

- navigation rows: 32 DIP;
- list filter/header area: 40 DIP;
- field grid header: 30 DIP;
- field rows: 26 DIP;
- detail section header: 32 DIP;
- detail property rows: 28 DIP minimum;
- title/header icon commands: 28 x 28 DIP;
- high-risk write actions remain visually separated from daily browsing actions.

At compact width the details column collapses first. The field list must never shrink below 480 DIP.

## 6. Popup and overlay limits

- Popup/dropdown maximum height: the lesser of 480 DIP or 60% of the current monitor work area.
- Popup width: at least owner width, at most the current monitor work area minus 16 DIP on each side.
- Completion, Quick Peek, menus, and tooltips must clamp to the monitor work area and may reposition above the anchor.
- No popup may derive placement from physical-pixel constants.

## 7. Accessibility and automation geometry

- Keyboard focus must remain visible at 100%, 125%, 150%, and 200% scaling.
- Icon-only commands require accessible name, tooltip, and stable `AutomationId`.
- Search query, search result tree/list, Field Registry navigation, field list, details pane, theme command, bottom panel tabs, and right tool tabs are mandatory automation anchors.
- Template changes must preserve logical focus order and existing command bindings.

## 8. Visual acceptance

M1 screenshots are compared primarily against the annotated M0D diagrams on a 1920 x 1080 design canvas at 100% scaling. A 1280 x 800 DIP startup-window capture remains mandatory as a compact fallback check, and Field Registry remains 1040 x 700 DIP. Acceptance is based on the reference proportions: compact top chrome, approximately 84% editor-side width, a full-height right tool well, a bottom tool restricted to the editor column, and a dominant editor viewport—not on illustrative text in the frozen images.
