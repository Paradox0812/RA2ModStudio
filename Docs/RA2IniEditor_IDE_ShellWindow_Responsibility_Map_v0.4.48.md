# RA2IniEditor IDE ShellWindow Responsibility Map v0.4.48

## Scope

v0.4.48 is a responsibility map and extraction plan for `ShellWindow.xaml.cs`.
This version intentionally does not change runtime behavior, WPF event routing,
completion commit, hover display, Add Property, edit session, diagnostics, or
project loading.

The goal is to make the next controller extraction work small, reviewable, and
guarded against accidental save-chain or UI lifecycle changes.

## Current ShellWindow Responsibilities

`ShellWindow` is currently the IDE shell composition root and owns these runtime
areas at the same time:

- Window startup, DataContext attachment, close cleanup.
- Project open and project explorer selection.
- Source editor text synchronization between `ShellViewModel.SourceEditor` and AvalonEdit.
- Read-only source highlighting reload.
- Edit mode, in-memory dirty state, and revert.
- Completion popup interaction and commit.
- Hover tooltip lifetime.
- Language navigation windows: Go To Definition, Peek Definition, Find All References.
- Issues navigation.
- Add Property / Field Browser launch, duplicate-key action handling, and in-memory text apply.
- Field Registry Manager, harvest preview, apply, rollback, local reload, and folder open commands.
- Search and Issues tool window lifetime.

This concentration is useful for early IDE shell velocity, but it is becoming
the main risk area for future Save Current File, Undo/Redo, diagnostics, and
larger editing features.

## Field Dependency Map

### Project and Explorer

- `_sectionNavigationResolver`
- `_searchToolWindow`
- `_issuesToolWindow`

### Field Registry and Annotation

- `_fieldRegistryRuntimeService`
- `_fieldRegistryManagerViewModel`
- `_fieldRegistryManagerWindow`
- `_fieldRegistryHarvestPreviewWindow`
- `_fieldAnnotationStore`
- `_fieldAnnotationPathService`
- `_fieldAnnotationEditingService`

### Language Service

- `_semanticModelBuilder`
- `_caretContextService`
- `_definitionProvider`
- `_referenceFinder`
- `_hoverProvider`
- `_completionProvider`
- `_completionDisplayEnhancer`
- `_peekDefinitionWindow`
- `_findReferencesWindow`

### Source Editor and Editable Session

- `_editableSessionService`
- `_textChangeApplier`
- `_editorStateViewModelFactory`
- `_boundSourceEditor`
- `_editableSession`
- `_isSynchronizingEditorText`

### Completion Interaction

- `_completionCommitCoordinator`
- `_completionDropdownViewModel`
- `_lastCompletionResult`

### Hover Interaction

- `SourceEditorHoverKeyHitPadding`
- `SourceEditorHoverDelayMilliseconds`
- `_sourceEditorHoverTimer`
- `_currentHoverPopup`
- `_pendingHoverOffset`

### Add Property

- `_addPropertyInsertPlanner`
- `_recentFieldUsageTracker`
- `_addPropertyTextDocumentParser`

## Event Handler Inventory

### Shell and Tools

- `OpenProjectFolder`
- `OpenProjectFolderForAutomationAsync`
- `OpenSearchToolWindow`
- `OpenFieldRegistryManagerWindow`
- `OpenIssuesToolWindow`
- `ToggleProjectExplorer`
- `ProjectExplorerTreeView_OnSelectedItemChanged`
- `IssuesToolWindow_OnIssueNavigateRequested`
- `OnClosed`

### Language Navigation

- `GoToDefinition_OnClick`
- `PeekDefinition_OnClick`
- `FindAllReferences_OnClick`
- `FindReferencesWindow_OnReferenceNavigateRequested`

### Completion

- `ShowCompletionPreview_OnClick`
- `SourceTextEditor_OnPreviewKeyDown`
- `SourceTextEditorTextArea_OnPreviewKeyDown`
- `CompletionDropdownView_OnCompletionItemDoubleClicked`
- `CompletionDropdownView_OnCompletionCommitRequested`
- `CompletionDropdownView_OnCompletionCloseRequested`

### Hover and Source Editor

- `SourceTextEditor_OnLostKeyboardFocus`
- `SourceTextEditor_OnMouseMove`
- `SourceEditorHoverTimer_OnTick`
- `SourceTextEditor_OnMouseLeave`
- `SourceTextEditor_OnTextChanged`
- `SourceTextEditorCaret_OnPositionChanged`
- `SourceTextEditorTextView_OnScrollOffsetChanged`

### Editing

- `EnterEditMode_OnClick`
- `RevertInMemoryChanges_OnClick`

### Add Property

- `AddProperty_OnClick`
- `OpenFieldAnnotationEditor`
- `ApplyAddPropertyInsertDuplicate`
- `ApplyAddPropertyReplaceExisting`
- `ApplyAddPropertyPlan`

### Field Registry Manager

- `FieldRegistryManagerWindow_OnReloadLocalFieldRegistryRequested`
- `FieldRegistryManagerWindow_OnHarvestPreviewRequested`
- `FieldRegistryManagerWindow_OnOpenGlobalRegistryFolderRequested`
- `FieldRegistryManagerWindow_OnOpenProjectRegistryFolderRequested`
- `FieldRegistryManagerWindow_OnRefreshRollbackManifestsRequested`
- `FieldRegistryManagerWindow_OnOpenRollbackTargetFolderRequested`
- `FieldRegistryManagerWindow_OnOpenRollbackManifestFolderRequested`
- `FieldRegistryManagerWindow_OnOpenRollbackBackupFolderRequested`
- `FieldRegistryManagerWindow_OnRollbackCompleted`
- `FieldRegistryManagerWindow_OnClosed`

## Method Grouping

### Source Editor / Text Sync

Current methods:

- `ShellWindow_OnDataContextChanged`
- `AttachSourceEditorTextBinding`
- `SourceEditor_OnPropertyChanged`
- `SetReadonlySourceText`
- `SetEditorTextFromProgram`
- `InstallReadonlySourceHighlighting`
- `ReloadReadonlySourceHighlighting`
- `ReloadLocalFieldRegistryForReadonlyHighlighting`
- `ReplaceReadonlySourceHighlightingTransformer`
- `RestoreSourceEditorFocusAtCaret`
- `TryScrollSourceEditorToLine`
- `TryScrollSourceEditorToLanguageTarget`
- `TryScrollSourceEditorToCharacterIndex`

Candidate controller: `Ra2SourceEditorController`

Responsibilities:

- AvalonEdit text load and guarded programmatic synchronization.
- Caret offset get/set and focus restore.
- Read-only source highlighting wiring.
- Scroll-to-line and scroll-to-character helper orchestration.

Forbidden responsibilities:

- Completion candidate generation.
- Add Property.
- Save or disk writes.
- Hover semantic data construction.
- Project Explorer model updates.

### Editable Session

Current methods:

- `EnterEditMode_OnClick`
- `RevertInMemoryChanges_OnClick`
- `SourceTextEditor_OnTextChanged`
- `ResetEditableSessionToReadOnly`
- `UpdateEditorStateControls`

Candidate controller: `Ra2EditorSessionController`

Responsibilities:

- Enter edit mode.
- Revert in-memory changes.
- Track and refresh editable session state.
- Rebuild in-memory text model after valid editor text changes.

Forbidden responsibilities:

- Save to disk.
- ProjectSaveService integration.
- Completion candidate generation.
- Add Property searching.
- WPF-specific popup handling.

### Completion Interaction

Current methods:

- `HandleCompletionPreviewKeyDown`
- `ShowCompletionDropdownAtCaret`
- `ShowCompletionDropdown`
- `TryGetCompletionPopupPosition`
- `CloseCompletionDropdown`
- `TryCommitSelectedCompletionOrClose`
- `TryCommitCompletionItemOrClose`
- `ShowCompletionCommitStatus`
- `IsFocusMovingInsideCompletionDropdown`

Candidate controller: `Ra2CompletionInteractionController`

Responsibilities:

- Ctrl+Space activation.
- Popup open, positioning, close, selection movement.
- Enter / Tab / double-click commit routing.
- Commit coordinator invocation.
- Post-commit caret restoration and status message.

Forbidden responsibilities:

- Completion provider semantic rules.
- Field Registry loading.
- Add Property insert planning.
- Save or dirty persistence.

### Hover

Current methods:

- `SourceTextEditor_OnMouseMove`
- `SourceEditorHoverTimer_OnTick`
- `TryShowSourceEditorHoverAtOffset`
- `TryCreateKeyHoverContext`
- `IsKeyHoverHitCandidate`
- `TryGetDocumentOffsetFromMouse`
- `CloseSourceEditorHoverToolTip`
- `ShowSourceEditorHoverToolTip`

Candidate controller: `Ra2SourceEditorHoverController`

Responsibilities:

- Mouse hover debounce.
- Tooltip popup lifetime.
- Key-token hit filtering.
- Close on mouse leave, scroll, caret move, text change, or completion popup.

Forbidden responsibilities:

- Hover provider semantic content rules.
- Completion popup behavior.
- Add Property.
- Save or dirty state.

### Language Navigation

Current methods:

- `GoToDefinition_OnClick`
- `PeekDefinition_OnClick`
- `FindAllReferences_OnClick`
- `TryGetDefinitionAtCaret`
- `TryBuildLanguageContext`
- `TryBuildLanguageContextAtOffset`
- `ShowPeekDefinitionWindow`
- `ShowFindReferencesWindow`
- `FindReferencesWindow_OnReferenceNavigateRequested`
- `FormatTargetName`
- `TrimSectionTitle`

Candidate controller: `Ra2LanguageNavigationController`

Responsibilities:

- Build current-document language context.
- Invoke definition and reference services.
- Open and update preview windows.
- Navigate source editor to language targets.

Forbidden responsibilities:

- Completion interaction.
- Hover tooltip lifetime.
- Save.
- Editable session mutation.

### Add Property / Field Browser

Current methods:

- `AddProperty_OnClick`
- `OpenFieldAnnotationEditor`
- `ApplyAddPropertyInsertDuplicate`
- `ApplyAddPropertyReplaceExisting`
- `ApplyAddPropertyPlan`
- `CreateFieldDisplayResolver`
- `GetProjectFieldAnnotationPath`

Candidate controller: `Ra2FieldBrowserController`

Responsibilities:

- Open Add Property with current section kind and annotation status.
- Refresh annotation display after annotation editor save.
- Route duplicate-key actions.
- Apply Add Property plans through in-memory text change applier.
- Record recent field usage after successful in-memory apply.

Forbidden responsibilities:

- Field Registry runtime loading.
- Source editor core text synchronization.
- Save to disk.
- Completion commit.
- ObjectAggregator or full-project indexing.

### Project / Explorer

Current methods:

- `OpenProjectFolder`
- `OpenProjectFolderForAutomationAsync`
- `ProjectExplorerTreeView_OnSelectedItemChanged`
- `TryNavigateToSection`
- `ApplyProjectExplorerVisibility`
- `TryNavigateToIssue`

Candidate controller: `Ra2ProjectShellController`

Responsibilities:

- Open folder and automation open folder.
- Switch current file from Project Explorer.
- Section and issue navigation orchestration.
- Clear transient hover/completion state on file switch.

Forbidden responsibilities:

- Text editing.
- Add Property insert.
- Completion commit.
- Save to disk.

### Field Registry Manager

Current methods:

- `OpenFieldRegistryManagerWindow`
- `FieldRegistryManagerWindow_OnReloadLocalFieldRegistryRequested`
- `FieldRegistryManagerWindow_OnHarvestPreviewRequested`
- `FieldRegistryManagerWindow_OnOpenGlobalRegistryFolderRequested`
- `FieldRegistryManagerWindow_OnOpenProjectRegistryFolderRequested`
- `FieldRegistryManagerWindow_OnRefreshRollbackManifestsRequested`
- `FieldRegistryManagerWindow_OnOpenRollbackTargetFolderRequested`
- `FieldRegistryManagerWindow_OnOpenRollbackManifestFolderRequested`
- `FieldRegistryManagerWindow_OnOpenRollbackBackupFolderRequested`
- `FieldRegistryManagerWindow_OnRollbackCompleted`
- `FieldRegistryManagerWindow_OnClosed`
- `RefreshFieldRegistryRollbackManifests`
- `OpenRegistryFolder`
- `OpenRollbackFolder`

Candidate controller: `Ra2FieldRegistryManagerController`

Responsibilities:

- Own Field Registry Manager window lifetime.
- Reload local field registry on user command.
- Open harvest preview.
- Show rollback manifests and route rollback results.
- Refresh highlighter after local registry changes.

Forbidden responsibilities:

- Completion UI lifecycle.
- Add Property insert.
- Save current INI file.
- Core or Infrastructure public API changes.

## Controller Candidate Summary

| Candidate | First extraction risk | Main dependency | Why it helps |
| --- | --- | --- | --- |
| `Ra2LanguageNavigationController` | Low | Language services and preview windows | Mostly read-only current-document routing. |
| `Ra2SourceEditorHoverController` | Medium | AvalonEdit mouse events and hover provider | Isolates fragile tooltip lifecycle. |
| `Ra2CompletionInteractionController` | Medium-high | Completion popup and commit coordinator | Reduces event routing complexity. |
| `Ra2FieldBrowserController` | Medium-high | Add Property, annotations, edit session | Separates field browser from shell wiring. |
| `Ra2FieldRegistryManagerController` | Medium-high | Field registry service and windows | Keeps import/apply/rollback away from editor events. |
| `Ra2ProjectShellController` | Medium-high | ShellViewModel and explorer | Separates project/file switching from editor features. |
| `Ra2EditorSessionController` | High | Editable session and dirty state | Touches future Save/Undo boundary. |
| `Ra2SourceEditorController` | High | AvalonEdit text sync | Most central editor state boundary. |

## Recommended Extraction Order

1. `Ra2LanguageNavigationController`
   - Lowest risk because it mostly builds read-only current-document context and opens preview windows.
2. `Ra2SourceEditorHoverController`
   - Good next step because hover bugs are lifecycle-heavy but isolated from text mutation.
3. `Ra2CompletionInteractionController`
   - High value, but must preserve `_lastCompletionResult` lifecycle and read-only commit guard.
4. `Ra2FieldBrowserController`
   - Depends on Add Property, duplicate actions, annotation refresh, and in-memory text apply.
5. `Ra2FieldRegistryManagerController`
   - Keeps registry import/apply/rollback UI away from editor event code.
6. `Ra2ProjectShellController`
   - Needs careful file switching cleanup and source editor reset handling.
7. `Ra2EditorSessionController`
   - Save/Undo-related risk; should wait until editor state boundaries are stable.
8. `Ra2SourceEditorController`
   - Most central; extract after other feature controllers stop reaching directly into editor internals.

Do not extract more than one controller in a single version unless the user explicitly approves a larger refactor.

## Guardrails For Future Extraction

- Do not add Save Current File, Save All, `ProjectSaveService`, `IniFileService`, or legacy save dependencies while extracting controllers.
- Do not change `ShellWindow.xaml` event names and command routing unless the extraction version explicitly scopes that change.
- Do not change Completion commit semantics, especially read-only behavior and `_lastCompletionResult` lifetime.
- Do not change Add Property insertion semantics: it remains in-memory and uses raw keys.
- Do not change Hover content rules while extracting hover lifetime code.
- Do not introduce ObjectAggregator or full-project indexing.
- Do not move Core or Infrastructure public API for controller extraction.
- Keep controller constructors narrow and pass only the dependencies needed for that controller.
- Prefer extracting read-only or window-lifetime controllers before dirty/edit-session controllers.

## Regression Test Checklist

After any controller extraction, run:

- `dotnet test -c Release`
- `dotnet build -c Release --no-incremental`

Manual smoke for any extraction:

1. Start `RA2IniEditor.IDE`.
2. Open a folder.
3. Open an INI file.
4. Enter edit mode.
5. Use Ctrl+Space completion.
6. Commit a completion.
7. Open Add Property and insert a raw key in memory.
8. Trigger hover on a known field.
9. Use Go To Definition, Peek Definition, and Find All References.
10. Revert in-memory changes.
11. Confirm no disk save occurs.

## v0.4.48 Verification Expectation

This version should only add documentation and guardrail tests. It should not
modify `ShellWindow.xaml`, `ShellWindow.xaml.cs`, completion behavior, Add
Property behavior, hover behavior, edit mode, dirty state, or diagnostics.
