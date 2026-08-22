# UI-MODERN-PROGRAM-R1 M3-0 — Shell Exact UI Inventory

Status: completed read-only inventory on 2026-07-22.  
Scope: Shell, Problems, Output, Project Explorer, AI Assistant, floating Search and shared visual resources.  
Purpose: freeze behavior-visible identities and code-behind dependencies before M3-A through M3-E.

## 1. Resource load order

Current `App.xaml` order:

1. `IdeVisualTokens.xaml`
2. `IdeControlStyles.xaml`
3. `IdeCollectionStyles.xaml`
4. `ShellTheme.xaml`
5. `IconResources.xaml`
6. `IconGeometryResources.xaml`
7. `IconImageResources.xaml`
8. `IdeSecondaryWindowStyles.xaml`

M3-A inserts `IdeWorkspaceStyles.xaml` after collection styles and before `ShellTheme.xaml`. This lets ShellTheme consume common workspace patterns while keeping later compatibility aliases valid. The application-wide Window style remains font-family only.

## 2. Dock identities and lifecycle

Frozen ContentIds:

```text
Document.Source
Tool.Problems
Tool.Output
Tool.Search
Tool.FindReferences
Tool.SectionExplorer
Tool.AiAssistant
```

Frozen named layout elements include `BottomToolPaneGroup`, `BottomToolTabs`, `BottomProblemsAnchorable`, `BottomOutputAnchorable`, `SearchAnchorable`, `FindReferencesAnchorable`, `RightToolPaneGroup`, `RightToolWellRoot`, `SectionExplorerAnchorable` and the AI anchorable.

M3 must not change `CanAutoHide`, `CanClose`, `CanDockAsTabbedDocument`, `CanFloat`, `CanHide`, ContentId, initial visibility, selected indices, preferred floating geometry or Home pane placement.

Search remains `Tool.Search`, hidden by default, preferred FloatingWidth 560 / FloatingHeight 620. `SearchToolContentHost` remains focusable and keeps `Shell.Tool.Search.Content`. Search activation continues through `ShowAndActivateSearchTool` and `ShowAndActivateFloatingTool`; no M3 code change is allowed in that lifecycle.

## 3. Problems surfaces

### Shell bottom Problems

Frozen:

- `BottomErrorListTab` / `Shell.BottomToolTabs.ErrorList`;
- `BottomIssuesGrid` / `Shell.BottomIssues.Grid`;
- `ItemsSource={Binding Issues.Items}`;
- `SelectedItem={Binding Issues.SelectedIssue, Mode=TwoWay}`;
- `MouseDoubleClick=BottomIssuesGrid_OnMouseDoubleClick`;
- refresh/full/clear AutomationIds and click handlers.

### Standalone Issues

Frozen:

- Window AutomationId `Issues.Window`;
- `IssuesGrid`, `Issues.Grid`, Items/SelectedIssue bindings and double-click handler;
- severity/source/search bindings;
- clear-filter, refresh, full-diagnostics and clear events;
- `IssueNavigateRequested`, `ClearIssuesRequested`, `ClearIssueFiltersRequested`, `RefreshCurrentFileDiagnosticsRequested`, `RunManualFullDiagnosticsRequested` lifecycle.

Existing canonical data path is `IssuesViewModel`. It already owns All/Error/Warning/Info labels, selected severity, total/filtered/error/warning/info counts and filter application. M3-B must not add or modify issue data state.

Graphical filters may bind a horizontal selector directly to `Issues.SeverityFilterOptions` and `Issues.SelectedSeverityFilter`. Existing ComboBox AutomationId remains on the standalone surface unless explicitly retained as a hidden compatibility peer; Shell adds only the approved `Shell.BottomIssues.Filter.*` and Count anchors.

Current issue rows expose `Severity`, `SeverityText`, `SeverityMarker`, `LocationText`, `Code`, `Message` and `SourceText`. Geometry is selected by XAML DataTriggers on `SeverityText`; no converter or ViewModel API is required.

## 4. Output

Frozen:

- `BottomOutputTab` / `Shell.BottomToolTabs.Output`;
- `Shell.OutputTextBox`;
- `Text={Binding OutputText, Mode=OneWay}`;
- `IsReadOnly=True`, AcceptsReturn and both scrollbars;
- Consolas/code-surface typography authority where applied.

M3-C may change surface style, padding and wrapping hierarchy, but not replace the binding or introduce editable/log state.

## 5. Project Explorer

Frozen:

- `ProjectExplorerTreeView` / `Shell.ProjectExplorer`;
- `ItemsSource={Binding ProjectExplorer.Items}`;
- `SelectedItemChanged=ProjectExplorerTreeView_OnSelectedItemChanged`;
- `ShellProjectExplorerTreeItemStyle` adoption;
- HierarchicalDataTemplate `ItemsSource={Binding Children}`;
- item icon and `DisplayTextWithCount` bindings;
- status binding and navigation handler path.

Virtualization is mandatory:

```text
ScrollViewer.CanContentScroll=True
VirtualizingStackPanel.IsVirtualizing=True
VirtualizingStackPanel.VirtualizationMode=Recycling
```

Code-behind calls `ProjectExplorerTreeView.UpdateLayout()` and uses its `ItemContainerGenerator`; M3-C may not replace the TreeView or rename it.

## 6. AI Assistant

Frozen XAML identities include:

```text
AiAssistant.Panel
AiAssistant.ChatHistory
AiAssistant.ChatHistoryActions
AiAssistant.EmptyStateMessage
AiAssistant.Composer
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.CancelButton
AiAssistant.ClearButton
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
AiAssistant.RequestPreparationNotice
AiAssistant.ConfigurationStatus
AiAssistant.DraftPreview
AiAssistant.ContextSummary
AiAssistant.CurrentSubjectSummary
AiAssistant.ConversationContextSummary
AiAssistant.SafetyFooter
```

M3-D preserves all existing click handlers, selected-model path, send/cancel enabled-state updates, conversation turn Tag/DataContext conventions and message trimming.

Allowed code-behind method family:

- user/assistant message visual creation and streaming-message visual update methods;
- `AppendAiAssistantMarkdownBlocks`;
- `CreateAiAssistantPlainTextFallback`;
- `CreateAiAssistantTextBlock`;
- heading/paragraph/list/table/table-row/code-block visual factories;
- inline-text visual formatting only;
- `ShowSourceEditorHoverToolTip`, `CreateSourceEditorHoverCard`, `CreateHoverInlineText`, `AddHoverMetadataPair`;
- at most one private required-resource helper.

Forbidden method families:

- AI send/cancel/clear orchestration and pipeline construction;
- provider/model selection and configuration resolution;
- request preparation, failure attribution, SSE/stream subscription and timers;
- conversation-context construction and trimming semantics;
- Dock/Search/Project Explorer/editor/session/shutdown methods.

Runtime visual AutomationIds for messages, Markdown, tables, code blocks, copy/restore actions and request diagnostics are frozen by `IdeShellBoundaryTests`.

## 7. Hover

Hover popup ownership and placement remain in Shell. Frozen behavior includes close/reset/timer sequencing, editor-relative clamping, popup StaysOpen behavior and data from `Ra2HoverDisplayViewModel`. M3-D may only replace hard-coded presentation brushes/thickness/radius with required shared resources and may not modify hover resolution or placement calculations.

## 8. Search content

Frozen Search anchors:

```text
Search.View
Search.QueryTextBox
Search.CaseSensitiveCheckBox
Search.WholeWordCheckBox
Search.RegexCheckBox
Search.ScopeComboBox
Search.FilePatternComboBox
Search.FindPreviousButton
Search.FindNextButton
Search.FindAllButton
Search.UnavailableHint
```

The current unavailable/read-only behavior is deliberate pending SEARCH-1. M3-E may change hierarchy, density and styles only. It may not restore mock results, add Replace, make execution buttons active or alter `SearchToolWindowViewModel`.

## 9. Current test authorities

- `IdeShellBoundaryTests.cs`: behavior-visible names, bindings, AI identities, navigation and prohibited controls.
- `IdeVisualSystemBoundaryTests.cs`: resource/template loading, TreeView/Issues handlers and visual adoption.
- `Ra2ShellIdeLayoutBoundaryTests.cs`: Dock topology, ContentIds, sizes, bottom/right placement and Problems/Output/Search composition.
- `IssuesViewModelTests.cs`: filtering/counting semantics; read-only for M3.

Tests may add assertions for new styles and anchors. Existing behavioral assertions may not be removed or weakened. Assertions that explicitly require textual E/W/I markers may be replaced only with geometry/severity-accessibility assertions while preserving SeverityText binding.

## 10. M3 stop conditions

Stop if implementation requires ViewModel changes, new public API, Dock controller/store/session changes, different ContentIds/Home behavior, AI pipeline changes, Search behavior, TreeView replacement, loss of virtualization, more than one private visual-resource helper, or modification outside the approved card files.

