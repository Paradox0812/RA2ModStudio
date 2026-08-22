# UI-MODERN-1 M0D Responsive Layout Specification

Status: visual contract; production XAML not started  
Baseline date: 2026-07-21

## 1. WPF responsiveness model

WPF does not provide web-style media queries. UI-MODERN-1 therefore uses explicit layout modes selected from the actual WPF client size. Mode changes affect visibility, labels, and panel defaults; they must not recreate view models, reload project data, reset selection, or change command semantics.

All thresholds are DIPs. Resizing is debounced only if measurement proves it necessary; no animation is required for correctness.

The Shell is fullscreen-first. The default visual acceptance canvas is 1920 x 1080 at 100% scaling. Runtime layout still uses the maximized active-monitor work area (`SystemParameters.WorkArea` or the per-monitor equivalent), while physical resolution is used only to derive effective DIPs. Added monitor space belongs to the editor; tool wells do not grow automatically with the window.

The workspace uses two top-level columns: editor-side workspace, 4 DIP splitter, and right tool well. The right tool spans the complete workspace height. Document tabs, editor, horizontal splitter, and bottom tools are nested inside the editor-side column, so bottom tools never consume width beneath the right tool.

## 2. Shell width modes

| Mode | Window width | Right tool well | Bottom panel default | Command presentation |
|---|---:|---:|---:|---|
| Compact | 960–1099 | 280 | 180 when opened | secondary labels may become icon-only |
| Standard | 1100–1439 | 320 | 220 when opened | normal labels and compact commands |
| Wide | 1440+ | 300 preferred, 340 soft max | 260 preferred, 300 soft max | added width goes to the editor |

Rules:

- The right tool well may be collapsed by the user in every mode.
- The editor minimum width is 560 DIP. If a resize would violate it, the right tool well collapses before the editor is compressed further.
- The bottom tool panel has a 160 DIP minimum and is capped at 45% of available client height.
- Entering Compact mode hides low-priority command text before hiding commands; hidden commands remain reachable from menus/overflow.
- Wide mode allocates added width to the editor. Tool wells keep their preferred DIP sizes unless the user explicitly resizes them.

## 3. Shell height modes

| Mode | Window height | Expanded bottom target | Behavior |
|---|---:|---:|---|
| Short | 640–719 | 180 | compact status content; bottom capped at 45% |
| Standard | 720–899 | 220 | full bottom header/status |
| Tall | 900+ | 260 preferred, 300 soft max | added height goes to the editor first |

The menu, toolbar, document tabs, and Shell status rows retain their fixed heights. Extra vertical space goes first to the editor, then to an already-open bottom result viewport.

## 4. Search responsive behavior

Search is hosted in the bottom tool panel.

### Compact width

- query box remains first and receives all star width;
- optional filter labels collapse to icons or short labels;
- match case / whole word / regex remain reachable and preserve state;
- result metadata trims before match text;
- the file hierarchy remains virtualized.

### Standard and Wide width

- query and common filters share one 40 DIP toolbar row;
- status/progress remains right-aligned when space permits;
- Wide adds query width and result preview width; it does not introduce a second result model.

### Short height

- result header and status remain visible;
- the result viewport alone shrinks;
- opening Search may collapse a competing bottom tool, but must not alter its state.

## 5. Field Registry width modes

| Mode | Window width | Navigation | Field list | Details |
|---|---:|---:|---:|---:|
| Compact | 820–959 | 148 | remaining, min 644 at 820 frame | collapsed; open on demand |
| Standard | 960–1199 | 156 | remaining, min 480 | 280–300 |
| Wide | 1200+ | 168 | remaining | 320 |

Rules:

- Details collapse before navigation or field list.
- Compact mode exposes details through the same selection and command bindings, using a reversible overlay/drawer or explicit details toggle.
- Provider priority stays visible in every width mode; content may abbreviate but its ordering must not change.
- Import/Learn and Backup/Rollback remain internal destinations and may use full-workspace pages; they do not squeeze into the daily three-column view.
- Dialogs opened by the Center clamp to the owner monitor and never exceed the monitor work area.

## 6. Field Registry height modes

| Mode | Window height | Rule |
|---|---:|---|
| Compact | 620–699 | title, priority strip, and status stay fixed; lists scroll |
| Standard | 700–899 | 26 DIP list rows and complete details headers |
| Tall | 900+ | additional height goes to field list/details scrolling regions |

No layout mode may hide validation, dirty-state, failure, rollback, or trust/risk indicators that are required by the existing workflow.

## 7. DPI matrix

The same DIP geometry is validated at these Windows scaling factors:

| Scale | Physical pixels for 1280 x 800 DIP | Required check |
|---|---:|---|
| 100% | 1280 x 800 | baseline alignment |
| 125% | 1600 x 1000 | one-pixel dividers and text clipping |
| 150% | 1920 x 1200 | popup clamp and toolbar wrapping |
| 200% | 2560 x 1600 | keyboard focus, minimum hit targets, dialog fit |

### 7.1 Fullscreen work-area acceptance cases

Taskbar and OS chrome vary, so the effective work-area values below are approximate acceptance anchors rather than hard-coded sizes:

| Physical display | Scale | Approx. effective work area | Editor with right tool, bottom closed | Editor with right tool and 220-DIP bottom tool |
|---|---:|---:|---:|---:|
| 1920 x 1080 | 100% | 1920 x 1080 design canvas | 1616 x 964 | 1616 x 700 |
| 2560 x 1440 | 125% | approximately 2048 x 1120 work area | 1744 x 1004 | 1744 x 740 |
| 3840 x 2160 | 150% | approximately 2560 x 1413 work area | 2256 x 1297 | 2256 x 1033 |
| 3840 x 2160 | 200% | approximately 1920 x 1060 work area | 1616 x 944 | 1616 x 680 |

At the 1920 x 1080 reference, the editor-side workspace must occupy approximately 84% of width. With the bottom tool open, the editor viewport must retain at least 65% of the 994 DIP workspace height; with it closed, it receives the complete height below document tabs. If these ratios fail, tool panels clamp or collapse before the editor is reduced.

Per-monitor DPI transitions must retain the selected document, search query/results, Field Registry selection, panel visibility, and splitter proportions. A DPI change must not reload services or issue commands.

## 8. State preservation contract

The following state survives all responsive mode transitions:

- active document and caret/selection;
- Project Explorer expansion/selection;
- right tool selection and visibility;
- bottom tool selection, Search query/options/results, and expanded/collapsed state;
- Field Registry destination, provider choice, filter, selected row, and read-only details context;
- theme selection and keyboard focus whenever the focused element remains visible.

If an element becomes hidden in Compact mode, focus moves to the nearest visible owner control and returns predictably when the element is restored.

## 9. Visual-stage stop conditions

Implementation must stop for diagnosis if any screenshot shows editor width below its minimum, clipped required commands, overlapping text, inaccessible hidden filters, lost state after a mode change, blurry 1-DIP dividers, or a popup outside the monitor work area. Ad-hoc XAML polishing is not permitted past a failed visual gate.
