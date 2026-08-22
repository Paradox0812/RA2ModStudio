# RA2IniEditor IDE Handoff v0.4.42.1

## Scope

v0.4.42.1 hardens the completion preview commit interaction introduced in v0.4.42. The change is limited to the IDE shell completion popup, input routing, and guardrail tests.

## Fixed Behavior

- The completion popup now keeps itself open while interacting with the candidate list.
- Moving keyboard focus from the editor into the completion dropdown no longer closes the popup.
- Enter and Tab commit requests are routed from the editor, AvalonEdit text area, and dropdown list.
- Double-clicking a completion item routes through the same commit path as keyboard commit.
- Read-only or unavailable commit paths now show a visible status message instead of silently closing.

## Guardrails

- No INI save pipeline changes.
- No dirty state changes.
- No disk write changes.
- No Completion UI beyond the existing preview dropdown.
- No legacy analysis, ObjectAggregator, ProjectLoader, or ProjectSaveService integration.
- No UI automation was required for this hotfix.

## Important Files

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/Views/Language/Ra2CompletionDropdownView.xaml`
- `RA2IniEditor.IDE/Views/Language/Ra2CompletionDropdownView.xaml.cs`
- `RA2IniEditor.Tests/IDE/Ra2CompletionDropdownFocusTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2CompletionCommitInputRoutingTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2CompletionCommitFailureVisibilityTests.cs`

## Manual Smoke Checklist

1. Open the IDE shell and load a small INI file.
2. Enable edit mode.
3. Trigger completion preview.
4. Click inside the completion list and confirm the popup remains open.
5. Double-click a candidate and confirm the edit-mode commit preview applies.
6. Trigger completion in read-only mode and press Enter or Tab.
7. Confirm the output/status text reports that commit was skipped because edit mode is not active.
