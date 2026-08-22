# AI Assistant Right Tool Well Implementation Contract

## 1. Scope and Baseline

AI-1A is an inspection and implementation-contract phase for the future AI-1 Right Tool Well work.

This document is based on the current Shell inspection and the accepted AI-0 documents:

- `Docs/AiAgentPanelPlacementContract.md`
- `Docs/AiAssistantArchitecture.md`
- `Docs/AiAssistantSafetyContract.md`

The AI Assistant remains a DeepSeek-powered RA2 Modding Assistant, not a Codex-like file editing agent.

AI-1 must later add a right-side AI Assistant tab/view using a mock client only. AI-1 must not connect DeepSeek, use network, load API keys, modify files, update Field Registry data, or implement apply/insert behavior.

This contract does not implement UI or source code.

## 2. Current Shell / Section Region Inventory

Current files inspected:

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/ViewModels/ShellViewModel.cs`
- `RA2IniEditor.IDE/ViewModels/ProjectExplorerViewModel.cs`
- `RA2IniEditor.IDE/ViewModels/ProjectExplorerItemViewModel.cs`
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/ShellViewModelLayoutTests.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2LanguageUiBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2ShellWindowEditorSessionBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/ProjectExplorerViewModelTests.cs`

Current right-side layout:

- Root Shell grid has three columns:
  - editor column: `Width="*"`
  - splitter column: `x:Name="ProjectExplorerSplitterColumn"`, `Width="4"`
  - right panel column: `x:Name="ProjectExplorerColumn"`, `Width="320"`
- Right-side splitter is `x:Name="ProjectExplorerGridSplitter"`, `Grid.Column="1"`, `ResizeDirection="Columns"`.
- Right-side panel is `x:Name="ProjectExplorerPanel"`, `Grid.Column="2"`.
- The right panel currently contains a `Grid` with:
  - header row
  - `TreeView x:Name="ProjectExplorerTreeView"`
  - status text row

Current control type:

- The Section/Navigator area is a `TreeView`, not a `TabControl`, `ContentControl`, or standalone Navigator list.
- Historical `NavigatorListBox` / `Navigator.Items` are explicitly absent in boundary tests.
- Section nodes are children inside the Project Explorer tree, grouped under file/type/faction nodes.

Current data source:

- Shell `DataContext` is `ShellViewModel`.
- The right tree binds to `ProjectExplorer.Items`.
- The status line binds to `ProjectExplorer.StatusText`.
- The title binds to `ProjectExplorerTitle`.
- Item containers bind AutomationId from `ProjectExplorerItemViewModel.AutomationId`.

Current ViewModel ownership:

- `ShellViewModel.ProjectExplorer` is a `ProjectExplorerViewModel`.
- `ProjectExplorerViewModel.Items` is an `ObservableCollection<ProjectExplorerItemViewModel>`.
- `ProjectExplorerViewModel.SelectedItem` stores the selected node.
- `ProjectExplorerViewModel.ShowFiles` populates top-level file nodes.
- `ProjectExplorerViewModel.ShowGroupedSectionsForCurrentFile` adds current-file Section nodes under the file node.
- `ProjectExplorerViewModel.MarkCurrentFile` and `MarkCurrentSection` set visual current markers.
- `ProjectExplorerItemViewModel` owns `Kind`, `DisplayText`, `FilePath`, `LineNumber`, `SectionId`, `Children`, `IconText`, `DisplayTextWithCount`, `ToolTipText`, `IsCurrentFile`, `IsCurrentSection`, and `IsExpanded`.

Current code-behind behavior:

- `ToggleProjectExplorer` calls `ShellViewModel.ToggleProjectExplorer()` and then `ApplyProjectExplorerVisibility()`.
- `ProjectExplorerTreeView_OnSelectedItemChanged` handles file and Section selection.
- File selection runs dirty-navigation checks, closes hover/completion UI, loads the file, and starts the editable session.
- Section selection runs `TryNavigateToProjectExplorerSectionAsync`.
- Programmatic selection uses `SelectProjectExplorerItem`, `TryGetProjectExplorerTreeViewItem`, and `_isRestoringProjectExplorerSelection` to avoid reentry.
- Language/reference navigation can call `MarkCurrentSection`, `FindProjectExplorerSectionItem`, and `SelectProjectExplorerItem`.
- Visibility is controlled by `ApplyProjectExplorerVisibility`.

Current right-side visibility and sizing:

- `DefaultProjectExplorerWidth` is `320.0`.
- `_lastProjectExplorerColumnWidth` preserves the last visible right column width.
- When visible, `ProjectExplorerSplitterColumn.Width` is set to `5` and `ProjectExplorerColumn.Width` restores `_lastProjectExplorerColumnWidth`.
- When hidden, current width is captured, `ProjectExplorerPanel` and splitter are collapsed, and both right columns become width `0`.

Existing command patterns suitable for opening AI Assistant:

- Shell uses direct XAML click handlers for menu and toolbar commands.
- Current examples include `ToggleProjectExplorer`, `FocusIssuesToolTab`, `OpenFieldRegistryManagerWindow`, and search/diagnostics actions.
- AI-1 should follow this existing Shell pattern unless a separate command abstraction is approved.

## 3. Current AutomationIds to Preserve

Existing Shell / right-side IDs that must be preserved:

- `Shell.Window`
- `Shell.Menu.ToggleProjectExplorer`
- `Shell.MainToolbar.ProjectExplorerButton`
- `Shell.ProjectExplorerGridSplitter`
- `Shell.ProjectExplorer`
- `Shell.ProjectExplorerStatusText`
- Dynamic tree item IDs from `ProjectExplorerItemViewModel.AutomationId`, currently formatted as `Shell.ProjectExplorer.{Kind}.{DisplayText}`

Existing tested names / structure that should not be renamed without updating tests:

- `ProjectExplorerSplitterColumn`
- `ProjectExplorerColumn`
- `ProjectExplorerGridSplitter`
- `ProjectExplorerPanel`
- `ProjectExplorerTreeView`
- `ProjectExplorerTitle`
- `ProjectExplorer.Items`
- `ProjectExplorer.StatusText`
- `ProjectExplorerItemViewModel.AutomationId`
- `DisplayTextWithCount`
- `ToolTipText`
- `IsCurrentFile`
- `IsCurrentSection`
- `IsExpanded`

AI-1 may append new AutomationIds:

- `RightToolWell.Root`
- `RightToolWell.SectionTab`
- `RightToolWell.AiTab`
- `RightToolWell.ActiveView`
- `AiAssistant.Panel`
- `AiAssistant.Header`
- `AiAssistant.CloseButton`
- `AiAssistant.ContextSummary`
- `AiAssistant.TaskKindSelector`
- `AiAssistant.PromptBox`
- `AiAssistant.GenerateButton`
- `AiAssistant.CancelButton`
- `AiAssistant.CopyButton`
- `AiAssistant.ClearButton`
- `AiAssistant.ResponseArea`
- `AiAssistant.DraftPreview`
- `AiAssistant.SafetyFooter`

## 4. Proposed Right Tool Well Strategy

Recommended strategy: preserve the existing right column, splitter, panel name, tree name, bindings, and visibility behavior, then add Right Tool Well switching inside `ProjectExplorerPanel`.

Do not add a second right column.

Do not replace the right-side area with a non-modal AI window.

Do not remove or rename the existing `ProjectExplorerTreeView`.

Preferred shell structure for AI-1:

- Keep `ProjectExplorerSplitterColumn`, `ProjectExplorerColumn`, `ProjectExplorerGridSplitter`, and `ProjectExplorerPanel`.
- Add an inner root container with `AutomationProperties.AutomationId="RightToolWell.Root"`.
- Add two switch controls or tab headers:
  - Section Tree / Navigator, `RightToolWell.SectionTab`
  - AI Assistant, `RightToolWell.AiTab`
- Keep the existing Project Explorer tree as the default active view.
- Add the AI panel as a second view under the same right-side footprint.

TabControl vs ContentControl decision:

- A full `TabControl` is possible, but it has higher risk because the current `ProjectExplorerTreeView` is used directly by code-behind for layout update, container lookup, focus, and programmatic selection.
- A conservative local view switch is safer: keep a stable right-tool root, keep the existing tree element alive with the same `x:Name`, and toggle visibility between the Section view and AI view.
- If a `TabControl` is used later, AI-1 must still preserve `ProjectExplorerTreeView` identity and ensure programmatic navigation can switch back to the Section tab before calling tree container lookup.

Required active-view behavior:

- Section view is active by default.
- AI view opens only through explicit command.
- Closing AI returns to Section view.
- Project Explorer visibility toggle still controls the whole right column.
- Existing project tree selection and Section jump behavior remain unchanged.
- If programmatic navigation needs the tree while AI view is active, AI-1 should switch the Right Tool Well back to the Section view before selecting or focusing tree nodes.

## 5. AI Assistant Panel Layout

AI-1 panel layout is mock-only and preview-only.

Required areas:

- Header
- Context Summary
- Task Kind Selector
- Prompt Input
- Actions
- Response Area
- Draft Preview Area
- Safety Footer

Required controls:

- Close button: returns to Section view.
- Task kind selector: displays the initial task kinds only.
- Prompt box: accepts user instruction text.
- Generate button: invokes mock generation only.
- Cancel button: cancels mock busy state.
- Copy button: copies response text only.
- Clear button: clears AI panel state only.
- Response area: displays mock response.
- Draft preview: displays draft text, not an apply surface.
- Safety footer: states output is draft/suggestion text and does not modify files.

Initial task kinds:

- ExplainField
- FindFieldsByRequirement
- GenerateUnitPrototype
- GenerateWeaponChainDraft
- ReviewIniSnippet
- ExplainDiagnostics

No Apply button is allowed in AI-1.

## 6. Mock Client Boundary

AI-1 must use a mock client only.

Required mock constraints:

- `MockRa2AiClient` or equivalent deterministic mock.
- No DeepSeek.
- No network.
- No API key.
- No file writes.
- No project scan.
- No apply or insert behavior.
- No Field Registry writes.
- No shell command execution.

Mock response should be sufficient to test:

- busy state
- cancellation
- response display
- copy behavior
- clear behavior
- empty prompt behavior
- optional simulated error state

Mock output is text only and must be marked as draft/suggestion content.

## 7. Commands / Entry Points

Recommended AI-1 entry point:

- Add an explicit menu item under the existing `视图` menu or another approved Shell menu group.
- Optionally add a toolbar button only if the implementation contract for AI-1 explicitly approves toolbar changes.

Suggested new command IDs:

- `Shell.Menu.OpenAiAssistant`
- `Shell.MainToolbar.AiAssistantButton` if toolbar entry is approved.

Recommended code-behind handlers:

- `OpenAiAssistantInRightToolWell`
- `CloseAiAssistantInRightToolWell`
- `ShowSectionTreeInRightToolWell`

These handlers must only switch the right tool view. They must not collect context automatically, send AI requests, or modify files.

AI generation itself should be handled by the AI panel ViewModel and mock client, not by directly editing Shell document state.

## 8. Files Proposed for Implementation

Likely AI-1 implementation files:

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/ViewModels/ShellViewModel.cs`
- `RA2IniEditor.IDE/ViewModels/AiAssistant/AiAssistantViewModel.cs`
- `RA2IniEditor.IDE/Views/AiAssistant/AiAssistantPanelView.xaml`
- `RA2IniEditor.IDE/Views/AiAssistant/AiAssistantPanelView.xaml.cs`
- `RA2IniEditor.IDE/AI/Clients/IRa2AiClient.cs`
- `RA2IniEditor.IDE/AI/Clients/MockRa2AiClient.cs`
- `RA2IniEditor.IDE/AI/Models/Ra2AiRequest.cs`
- `RA2IniEditor.IDE/AI/Models/Ra2AiResponse.cs`
- `RA2IniEditor.IDE/AI/Models/Ra2AiTaskKind.cs`

Likely tests to update or add:

- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/ShellViewModelLayoutTests.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`
- new `RA2IniEditor.Tests/IDE/AiAssistantRightToolWellBoundaryTests.cs`
- new `RA2IniEditor.Tests/IDE/AiAssistantViewModelTests.cs`

AI-1 should keep file count small where possible. If implementation exceeds the normal change budget, split AI-1 into:

- AI-1B: Shell Right Tool Well frame only.
- AI-1C: AI panel ViewModel and mock client.
- AI-1D: tests and automation coverage.

## 9. Tests to Add / Update

Planned test coverage:

- Section Tree remains present.
- Section Tree remains the default right-side view.
- Existing `Shell.ProjectExplorer` AutomationId remains.
- Existing Project Explorer tree bindings remain.
- Existing Project Explorer column/splitter names remain.
- `RightToolWell.Root` exists.
- `RightToolWell.SectionTab` exists.
- `RightToolWell.AiTab` exists.
- `RightToolWell.ActiveView` exists if implemented as a named active view host.
- AI opens only by explicit command.
- AI does not auto-open on startup.
- AI close returns to Section Tree.
- AI-1 has no Apply button.
- Context Summary exists.
- Safety Footer exists.
- Mock generate displays a response.
- Mock generate does not modify source editor text.
- Mock generate does not mark document dirty.
- Mock generate does not write files.
- Mock generate does not update Field Registry.
- Copy affects clipboard/response only and not editor text.
- Clear resets AI panel state only.

Existing tests likely requiring careful updates:

- `IdeShellBoundaryTests.IdeShellWindow_DefinesExpectedPlaceholderRegions`
- `IdeShellBoundaryTests.IdeShellBoundary_EditorAndProjectExplorerUseDockedGridLayout`
- `IdeShellBoundaryTests.IdeShellBoundary_ProjectExplorerUsesCompactToolWindowLayout`
- `IdeShellBoundaryTests.IdeProjectExplorer_UsesDescriptorNodesWithoutInlineSourceTextOrLegacyDependencies`
- `ShellViewModelLayoutTests.ProjectExplorer_IsVisibleByDefault`
- `ShellViewModelLayoutTests.ToggleProjectExplorer_ChangesVisibilityWithoutClearingTreeState`
- `WpfAutomationHarnessBoundaryTests.ShellAndFieldRegistryWindows_ExposeStableAutomationIds`
- `Ra2LanguageUiBoundaryTests.DefinitionNavigation_SynchronizesProjectExplorerSelectionWithoutReentry`
- `Ra2ShellWindowEditorSessionBoundaryTests` project explorer navigation assertions

Tests should use AutomationIds and semantic assertions. Avoid pixel-perfect layout assertions.

## 10. Semantic and Safety Boundaries

AI-1 must not change:

- INI parser semantics.
- Completion candidate generation.
- Completion commit behavior.
- Hover data source.
- Diagnostics.
- Save preflight.
- Backup and rollback.
- Undo and redo.
- Field Registry load/apply/rollback/import/learning semantics.
- Project > Global > BuiltIn priority.
- BuiltIn field definitions.
- Project Explorer grouping semantics.
- Project Explorer file/Section navigation semantics.
- Dirty-navigation behavior.
- Legacy exclusion rules.

AI-1 must not implement:

- DeepSeek client.
- Real network client.
- API key storage or loading.
- Context provider that scans project files.
- Prompt builder that sends real requests.
- Apply button.
- Insert workflow.
- File modification.
- Field Registry modification.
- Shell command execution.

## 11. Risks

Primary risks:

- Existing tests assert exact Shell names and layout markers. Renaming `ProjectExplorer*` elements would cause avoidable regressions.
- `ProjectExplorerTreeView` is used directly in code-behind for `UpdateLayout`, container lookup, selection, focus, and `BringIntoView`.
- If the AI view hides or unloads the tree, programmatic Section navigation may fail unless AI-1 switches back to Section view before tree selection.
- A full `TabControl` can change focus behavior and selected content availability.
- Adding toolbar/menu entry points can touch Shell boundaries and must be explicitly approved in AI-1.
- AI panel state must not be coupled to document dirty state or editor text mutation.

Recommended mitigation:

- Preserve existing right-side element names and bindings.
- Use append-only AutomationIds.
- Keep Section view default.
- Route all AI request behavior through mock-only ViewModel code.
- Add tests that prove source editor text and dirty state are unchanged by mock generation.

## 12. Recommended AI-1 Implementation Plan

Recommended split:

1. AI-1B Right Tool Well frame:
   - Modify Shell only after approval.
   - Keep existing right column/splitter/panel names.
   - Add Right Tool Well root and Section/AI switch controls.
   - Keep Section view default.
   - Add explicit open/close switching only.
   - No AI generation yet.

2. AI-1C Mock AI panel:
   - Add AI panel view and ViewModel.
   - Add task selector, prompt box, response area, draft preview, safety footer.
   - Add deterministic mock client.
   - No DeepSeek, network, API key, apply, insert, file writes.

3. AI-1D Tests:
   - Update Shell boundary tests to include Right Tool Well while preserving Project Explorer assertions.
   - Add AI Assistant AutomationId tests.
   - Add mock generation state tests.
   - Add safety tests proving no editor mutation.

AI-1 should stop after mock-only preview/copy behavior.

## 13. Acceptance Criteria

AI-1 implementation will be acceptable only when:

- Existing Section Tree remains default.
- Existing Section Tree behavior remains unchanged.
- Existing Project Explorer element names and AutomationIds are preserved.
- The right-side footprint remains the existing right column.
- No second right sidebar is added.
- No non-modal AI window is added.
- AI opens only by explicit command.
- AI closes back to Section Tree.
- AI-1 uses mock client only.
- AI-1 has no DeepSeek, no network, no API key.
- AI-1 has no Apply button.
- AI-1 does not modify files.
- AI-1 does not mark the document dirty.
- AI-1 does not update Field Registry.
- AI-1 does not execute shell commands.
- New AutomationIds are append-only.
- Build, tests, and IdeOnly package pass for the eventual implementation phase.
