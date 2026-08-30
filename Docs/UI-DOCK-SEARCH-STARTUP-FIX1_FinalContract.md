# UI-DOCK-SEARCH-STARTUP-FIX1 Final Contract

Status: approved by the user's explicit startup-flash repair request; Fix3 supersedes the incomplete
floating-host-only suppression described below.

## Fix3 root-cause amendment (2026-08-25)

Physical startup verification showed that suppressing native floating hosts was insufficient. AvalonDock can
render `Tool.FindReferences` and `Tool.Search` temporarily inside the main `DockingManager` while a persisted
layout is being deserialized. The startup flow also yields at `DispatcherPriority.ContextIdle`, so that
intermediate model has a real opportunity to render.

The final startup transaction therefore has two presentation gates:

- the main `ShellDockManager` starts at opacity zero and is not hit-testable;
- native floating hosts retain the existing floating-chrome suppression;
- after persisted-layout restore, both default-hidden tools (`Tool.FindReferences` and `Tool.Search`) are
  normalized to hidden;
- only after normalization and host refresh does the Shell reveal the main DockingManager and enable input.

This gate changes presentation timing only. It does not change the serialized layout schema, project/editor
state, Search/Find References business behavior, or the persisted placement of other tools.

## Problem fact

`Tool.Search` is compiled as hidden but has a floating Home zone. The previous startup topology called
`Float()` before the later visibility pass called `Hide()`. AvalonDock therefore created a native floating
host during Shell initialization, and the host could become visible briefly while its asynchronous close
completed.

## Required behavior

- A default-hidden floating tool receives its preferred floating geometry during startup but does not call
  `Float()` and does not create a floating host.
- The existing `ShowAndActivate("Tool.Search")` path materializes and activates the floating host on the
  first explicit user command.
- Search visibility and bottom placement are not restored as authoritative startup state. After any v2 or legacy
  layout restoration, Search is normalized to hidden before floating hosts are refreshed.
- Other tools and Search floating bounds continue to use the existing layout persistence system.
- Search content, query/replace behavior, close-to-hide behavior, dragging, geometry bounds and layout file
  format remain unchanged.

## Allowed files

- `RA2IniEditor.IDE/Views/ShellDockLayoutCoordinator.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.Tests/IDE/Ra2ShellIdeLayoutBoundaryTests.cs`
- current phase and product documentation

## Forbidden changes

- themes, floating chrome template, Dock serialization schema and layout store version
- Search view/view-model/services and all INI semantic or save behavior
- delay/sleep-based masking or a second Search host path

## Acceptance

- Automated STA layout test proves startup leaves hidden Search non-floating.
- The same test proves the first explicit show makes it visible, floating, selected and active.
- A restored visible/bottom Search state is hidden during startup, and a later explicit Search command overrides
  bottom placement with the Floating Home zone.
- IDE build/tests pass; physical no-flash observation remains a manual startup smoke item.
- A startup-order regression proves the main Dock is not revealed until restore and both hidden-tool
  normalizations have completed.
