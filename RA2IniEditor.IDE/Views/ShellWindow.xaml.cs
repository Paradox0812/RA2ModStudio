using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AvalonDock;
using AvalonDock.Layout;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Controllers.Completion;
using RA2IniEditor.IDE.Controllers.EditorSession;
using RA2IniEditor.IDE.Controllers.FieldAnnotations;
using RA2IniEditor.IDE.Controllers.FieldBrowser;
using RA2IniEditor.IDE.Controllers.Hover;
using RA2IniEditor.IDE.Controllers.Language;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.Highlighting;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Language.FieldQuickPeek;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.Services.DirtyNavigation;
using RA2IniEditor.IDE.Services.SavePreflight;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.IDE.ViewModels.AI;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;
using RA2IniEditor.IDE.ViewModels.FieldAnnotations;
using RA2IniEditor.IDE.ViewModels.Editing;
using RA2IniEditor.IDE.ViewModels.Language;
using RA2IniEditor.IDE.Views.FieldAnnotations;
using RA2IniEditor.IDE.Views.Language;
using RA2IniEditor.IDE.Views.FieldQuickPeek;
using RA2IniEditor.IDE.Views.FieldBrowser;
using RA2IniEditor.IDE.Views.AI;
using RA2IniEditor.IDE.Search;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.Views;

public partial class ShellWindow : Window
{
    private sealed record ProgrammaticSemanticUndoState(
        string UndoText,
        string RedoText,
        int UndoCaretOffset,
        int RedoCaretOffset,
        string UndoMessage,
        string RedoMessage,
        bool IsUndone);

    private sealed class ShellEditorTransactionPort : IRa2EditorTransactionPort
    {
        private readonly ShellWindow _owner;

        public ShellEditorTransactionPort(ShellWindow owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
            => _owner.ApplyAuthoringPreviewTransaction(preview);
    }

    private sealed class AiAssistantStreamingMessageHandle
    {
        public AiAssistantStreamingMessageHandle(
            Ra2AiRequestSession requestSession,
            Border userMessageBorder,
            Border messageBorder,
            StackPanel responsePanel,
            StackPanel actionPanel,
            TextBlock streamingText,
            TextBlock statusText,
            Button copyButton,
            DispatcherTimer flushTimer)
        {
            RequestSession = requestSession;
            UserMessageBorder = userMessageBorder;
            MessageBorder = messageBorder;
            ResponsePanel = responsePanel;
            ActionPanel = actionPanel;
            StreamingText = streamingText;
            StatusText = statusText;
            CopyButton = copyButton;
            FlushTimer = flushTimer;
        }

        public Ra2AiRequestSession RequestSession { get; }

        public Border UserMessageBorder { get; }

        public Border MessageBorder { get; }

        public StackPanel ResponsePanel { get; }

        public StackPanel ActionPanel { get; }

        public TextBlock StreamingText { get; }

        public TextBlock StatusText { get; }

        public Button CopyButton { get; }

        public DispatcherTimer FlushTimer { get; }

        public Ra2AiIncrementalTextBuffer Buffer { get; } = new();

        public int ImmediateFlushScheduled;

        public int FinalizationState;
    }

    private readonly ReadonlySourceSectionNavigationResolver _sectionNavigationResolver = new();
    private readonly FieldRegistryRuntimeService _fieldRegistryRuntimeService = new();
    private readonly FieldRegistryManagerViewModel _fieldRegistryManagerViewModel = new();
    private readonly IRa2AiContextProvider _aiContextProvider = new Ra2CurrentDocumentAiContextProvider();
    private readonly IRa2AiConversationContextProvider _aiConversationContextProvider = new Ra2AiConversationContextProvider();
    private readonly IRa2AiCurrentSubjectExtractor _aiCurrentSubjectExtractor = new Ra2AiCurrentSubjectExtractor();
    private readonly IRa2AiPromptBuilder _aiPromptBuilder;
    private readonly IRa2DocumentSemanticModelBuilder _semanticModelBuilder = new Ra2DocumentSemanticModelBuilder();
    private readonly IRa2CaretContextService _caretContextService = new Ra2CaretContextService();
    private readonly IRa2LanguageNavigationController _languageNavigationController =
        new Ra2LanguageNavigationController(new Ra2DefinitionProvider(), new Ra2ReferenceFinder());
    private readonly Ra2FieldQuickPeekService _fieldQuickPeekService = new();
    private readonly Ra2ReferenceValueDetailService _referenceValueDetailService = new();
    private readonly IRa2SourceEditorHoverController _sourceEditorHoverController =
        new Ra2SourceEditorHoverController(new Ra2HoverProvider());
    private readonly IRa2CompletionInteractionController _completionInteractionController =
        new Ra2CompletionInteractionController(
            new Ra2CompletionProvider(),
            new Ra2CompletionDisplayEnhancer(),
            new Ra2CompletionCommitCoordinator(
                new Ra2CompletionCommitPlanner(),
                new Ra2TextChangeApplier(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService())));
    private readonly IRa2FieldBrowserController _fieldBrowserController =
        new Ra2FieldBrowserController(
            new Ra2AddPropertyInsertPlanner(),
            new Ra2TextChangeApplier(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService()));
    private readonly IRa2EditableDocumentSessionService _editableSessionService =
        new Ra2EditableDocumentSessionService(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService());
    private readonly Ra2ProjectSearchService _projectSearchService =
        new(new ReadonlyIniContentService(new IniFileStore()));
    private readonly Ra2CurrentFileReplacePlanner _currentFileReplacePlanner = new();
    private readonly IRa2EditorSessionController _editorSessionController;
    private readonly IRa2IniAuthoringWorkspace _authoringWorkspace;
    private readonly Ra2AiAuthoringCoordinator _aiAuthoringCoordinator;
    private readonly Ra2AiProposalPreparationRunner _aiProposalPreparationRunner;
    private readonly Ra2IniEditPreviewCurrencyEvaluator _authoringPreviewCurrencyEvaluator = new();
    private const int SourceEditorHoverDelayMilliseconds = 300;
    private const double SourceEditorHoverHorizontalOffset = 12.0;
    private const double SourceEditorHoverVerticalOffset = 18.0;
    private const double SourceEditorHoverMinimumWidth = 280.0;
    private const double SourceEditorHoverMaximumWidth = 440.0;
    private const double SourceEditorHoverWindowPadding = 12.0;
    private const int AiAssistantStreamFlushIntervalMilliseconds = 50;
    private const int AiAssistantStreamImmediateFlushThresholdCharacters = 512;
    private const int AiAssistantMaximumUserPromptCharacters = 8000;
    private const int AiAssistantMaximumTerminalMessageCards = 60;
    private const int AiAssistantMaximumMarkdownBlocks = 256;
    private const int AiAssistantMaximumMarkdownCodeBlocks = 64;
    private const int AiAssistantMaximumMarkdownTableRows = 200;
    private const int AiAssistantMaximumMarkdownTableCells = 1200;
    private const double AiAssistantAutoScrollTolerance = 24.0;
    private const int SourceEditorCompletionAutoTriggerDelayMilliseconds = 220;
    private readonly IRa2FieldAnnotationStore _fieldAnnotationStore = new Ra2FieldAnnotationJsonStore();
    private readonly Ra2FieldAnnotationPathService _fieldAnnotationPathService = new();
    private readonly IRa2FieldAnnotationCoordinator _fieldAnnotationCoordinator;
    private readonly IRa2FieldAnnotationEditingService _fieldAnnotationEditingService = new Ra2FieldAnnotationEditingService();
    private Ra2FieldAnnotationRefreshResult? _fieldAnnotationRefreshCache;
    private string? _fieldAnnotationRefreshCacheProjectRootPath;
    private readonly Ra2RecentFieldUsageTracker _recentFieldUsageTracker = new();
    private readonly Ra2IniTextDocumentParser _addPropertyTextDocumentParser = new();
    private readonly IRa2EditorStateViewModelFactory _editorStateViewModelFactory = new Ra2EditorStateViewModelFactory();
    private readonly IRa2SaveCurrentFileService _saveCurrentFileService = new Ra2SaveCurrentFileService();
    private readonly Ra2SavePreflightDiagnosticService _savePreflightDiagnosticService = new();
    private readonly IRa2SavePreflightConfirmationService _savePreflightConfirmationService =
        new Ra2SavePreflightConfirmationService();
    private readonly IRa2DirtyNavigationDialogService _dirtyNavigationDialogService = new Ra2DirtyNavigationDialogService();
    private readonly Ra2SaveCurrentFileUiMessageFormatter _saveCurrentFileUiMessageFormatter = new();
    private readonly Ra2CompletionDropdownViewModel _completionDropdownViewModel = new();
    private SourceEditorViewModel? _boundSourceEditor;
    private Ra2EditableDocumentSession? _editableSession;
    private SearchToolView? _searchToolView;
    private CancellationTokenSource? _searchCancellation;
    private Ra2CompletionResult? _lastCompletionResult;
    private ProgrammaticSemanticUndoState? _programmaticSemanticUndoState;
    private bool _isSynchronizingEditorText;
    private bool _isRestoringProjectExplorerSelection;
    private IssuesToolWindow? _issuesToolWindow;
    private FieldRegistryCenterWindow? _fieldRegistryCenterWindow;
    private FieldLearningWizardWindow? _fieldLearningWizardWindow;
    private FieldRegistryManagerWindow? _fieldRegistryManagerWindow;
    private FieldRegistryHarvestPreviewWindow? _fieldRegistryHarvestPreviewWindow;
    private Ra2PeekDefinitionWindow? _peekDefinitionWindow;
    private Ra2FieldQuickPeekWindow? _fieldQuickPeekWindow;
    private int? _sourceEditorContextMenuOffset;
    private readonly DispatcherTimer _sourceEditorHoverTimer;
    private readonly DispatcherTimer _sourceEditorCompletionAutoTriggerTimer;
    private Popup? _currentHoverPopup;
    private bool _isBottomToolPanelVisible = true;
    private bool _hasBottomToolVisibilitySnapshot;
    private bool _isApplyingProjectExplorerVisibility;
    private readonly HashSet<string> _bottomToolVisibilityBeforeCollapse = new(StringComparer.Ordinal);
    private string _lastActiveBottomToolContentId = "Tool.Output";
    private readonly ShellDockLayoutCoordinator _dockLayoutCoordinator;
    private readonly ShellDockLayoutSession _dockLayoutSession;
    private readonly ShellDockLayoutStore _dockLayoutStore;
    private readonly ShellMonitorWorkAreaProvider _dockMonitorWorkAreaProvider;
    private readonly ShellWindowChromeController _windowChromeController;
    private readonly ShellDockFloatingChromeController _floatingChromeController;
    private LayoutAnchorable? _sectionExplorerVisibilitySource;
    private readonly Ra2AiRequestLifecycle _aiAssistantRequestLifecycle = new();
    private AiAssistantStreamingMessageHandle? _activeAiAssistantStreamingMessage;
    private Ra2AiEditProposalViewModel? _activeAiEditProposalViewModel;
    private Ra2AiEditProposalView? _activeAiEditProposalView;
    private Border? _activeAiEditProposalMessageBorder;
    private long _aiAuthoringGeneration;
    private bool _isShellClosed;

    public ShellWindow()
    {
        _aiPromptBuilder = new Ra2AiPromptBuilder();
        _fieldAnnotationCoordinator = new Ra2FieldAnnotationCoordinator(
            _fieldAnnotationStore,
            _fieldAnnotationPathService);
        _editorSessionController = new Ra2EditorSessionController(_editableSessionService);
        InitializeComponent();
        _authoringWorkspace = new Ra2IniAuthoringWorkspace(
            new Ra2IniEditPreviewService(
                new Ra2IniLanguageAnalysisService(),
                new Ra2AddPropertyInsertPlanner()),
            new ShellEditorTransactionPort(this));
        _aiAuthoringCoordinator = new Ra2AiAuthoringCoordinator(
            new Ra2AiAuthoringToolAdapter(),
            _authoringWorkspace);
        _aiProposalPreparationRunner = new Ra2AiProposalPreparationRunner(
            _aiAuthoringCoordinator);
        _windowChromeController = new ShellWindowChromeController(
            this,
            ShellTitleBarDragRegion,
            ShellTitleBarMaximizeRestoreButton);
        _windowChromeController.Attach();
        _floatingChromeController = new ShellDockFloatingChromeController(ShellDockManager);
        _floatingChromeController.Attach();
        ShellDockToolProfile[] dockProfiles =
        [
            new("Tool.Problems", ShellDockHomeZone.Bottom, 0, true, 880, 460),
            new("Tool.Output", ShellDockHomeZone.Bottom, 1, true, 800, 420),
            new("Tool.FindReferences", ShellDockHomeZone.Bottom, 2, false, 700, 460),
            new("Tool.Search", ShellDockHomeZone.Floating, 0, false, 560, 620),
            new("Tool.SectionExplorer", ShellDockHomeZone.Right, 0, true, 320, 720),
            new("Tool.AiAssistant", ShellDockHomeZone.Right, 1, true, 360, 760)
        ];
        _dockLayoutCoordinator = new ShellDockLayoutCoordinator(
            ShellDockManager,
            GetDockViewportScreenBounds,
            dockProfiles);
        _dockLayoutSession = new ShellDockLayoutSession(
            ShellDockManager,
            [
                SourceDocumentAnchorable,
                BottomProblemsAnchorable,
                BottomOutputAnchorable,
                SearchAnchorable,
                FindReferencesAnchorable,
                SectionExplorerAnchorable,
                AiAssistantAnchorable
            ],
            dockProfiles);
        _dockLayoutStore = new ShellDockLayoutStore();
        _dockMonitorWorkAreaProvider = new ShellMonitorWorkAreaProvider(this);
        Loaded += ShellWindow_OnLoaded;
        FindReferencesView.ReferenceNavigateRequested += FindReferencesWindow_OnReferenceNavigateRequested;
        _searchToolView = SearchToolContentHost.Children.OfType<SearchToolView>().Single();
        _searchToolView.SearchRequested += SearchToolView_OnSearchRequested;
        _searchToolView.ResultNavigateRequested += SearchToolView_OnResultNavigateRequested;
        _searchToolView.ReplacePreviewRequested += SearchToolView_OnReplacePreviewRequested;
        _searchToolView.ReplaceApplyRequested += SearchToolView_OnReplaceApplyRequested;
        RebindSectionExplorerVisibilitySource();
        AiAssistantModelSelector.ItemsSource = DeepSeekRa2AiModelCatalog.Options;
        AiAssistantModelSelector.SelectedValue = DeepSeekRa2AiModelCatalog.Default;
        AutomationProperties.SetAutomationId(SourceTextEditor.TextArea, "Shell.SourceEditor.TextArea");
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveCurrentFileCommand_Executed));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, UndoCurrentFileCommand_Executed, UndoRedoCommand_CanExecute));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, RedoCurrentFileCommand_Executed, UndoRedoCommand_CanExecute));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Save, new KeyGesture(Key.S, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Undo, new KeyGesture(Key.Z, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Redo, new KeyGesture(Key.Y, ModifierKeys.Control)));
        _sourceEditorHoverTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(SourceEditorHoverDelayMilliseconds)
        };
        _sourceEditorHoverTimer.Tick += SourceEditorHoverTimer_OnTick;
        _sourceEditorCompletionAutoTriggerTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(SourceEditorCompletionAutoTriggerDelayMilliseconds)
        };
        _sourceEditorCompletionAutoTriggerTimer.Tick += SourceEditorCompletionAutoTriggerTimer_OnTick;
        CompletionDropdownView.DataContext = _completionDropdownViewModel;
        CompletionDropdownView.CompletionItemDoubleClicked += CompletionDropdownView_OnCompletionItemDoubleClicked;
        CompletionDropdownView.CompletionCommitRequested += CompletionDropdownView_OnCompletionCommitRequested;
        CompletionDropdownView.CompletionCloseRequested += CompletionDropdownView_OnCompletionCloseRequested;
        SourceTextEditor.TextArea.Caret.PositionChanged += SourceTextEditorCaret_OnPositionChanged;
        SourceTextEditor.TextArea.SelectionChanged += SourceTextEditorSelection_OnChanged;
        SourceTextEditor.TextArea.TextView.ScrollOffsetChanged += SourceTextEditorTextView_OnScrollOffsetChanged;
        SourceTextEditor.TextArea.PreviewKeyDown += SourceTextEditorTextArea_OnPreviewKeyDown;
        SourceTextEditor.MouseMove += SourceTextEditor_OnMouseMove;
        SourceTextEditor.MouseLeave += SourceTextEditor_OnMouseLeave;
        InstallReadonlySourceHighlighting();
        DataContextChanged += ShellWindow_OnDataContextChanged;
        AttachSourceEditorTextBinding(DataContext as ShellViewModel);
        ApplyProjectExplorerVisibility();
        ApplyBottomToolPanelVisibility();
        ResetEditableSessionToReadOnly();
        UpdateShellStatusBar();
    }

    private async void ShellWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ShellWindow_OnLoaded;
        _floatingChromeController.BeginInitialLayoutVisibilitySuppression();
        try
        {
            _dockLayoutCoordinator.ApplyInitialFloatingGeometry();
            _dockLayoutCoordinator.ApplyCompiledDefaultTopology();
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
            _dockLayoutCoordinator.ApplyCompiledDefaultVisibility();
            if (_dockLayoutCoordinator.FindTool("Tool.Output") is { } output)
            {
                output.IsSelected = true;
                output.IsActive = true;
            }
            ShellDockLayoutOperationResult captureResult = _dockLayoutSession.CaptureCompiledDefault();
            if (!captureResult.Succeeded)
            {
                ReportDockLayoutFailure("无法建立默认窗口布局快照，已继续使用当前布局。", captureResult);
                return;
            }

            await TryRestorePersistedDockLayoutAsync();
            _floatingChromeController.RefreshExistingHosts();
        }
        finally
        {
            _floatingChromeController.CompleteInitialLayoutVisibilitySuppression();
        }

        _ = Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(_floatingChromeController.RefreshExistingHosts));
    }

    private Rect GetDockViewportScreenBounds()
    {
        Point origin = ShellDockManager.PointToScreen(new Point());
        if (PresentationSource.FromVisual(ShellDockManager)?.CompositionTarget is { } target)
            origin = target.TransformFromDevice.Transform(origin);
        return new Rect(origin, new Size(ShellDockManager.ActualWidth, ShellDockManager.ActualHeight));
    }

    private async void OpenProjectFolder(object sender, RoutedEventArgs e)
    {
        CloseSourceEditorHoverToolTip();
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();
        if (DataContext is ShellViewModel guardViewModel && !TryResolveDirtyNavigationBeforeLeavingCurrentFile(guardViewModel))
            return;

        OpenFolderDialog dialog = new()
        {
            Title = "Open RA2 INI Project Folder"
        };

        if (dialog.ShowDialog(this) != true)
            return;

        if (DataContext is ShellViewModel viewModel)
        {
            ResetEditableSessionToReadOnly();
            await viewModel.OpenProjectFolderAsync(dialog.FolderName);
            ReloadReadonlySourceHighlighting(viewModel);
            ResetEditableSessionToReadOnly();
        }
    }

    internal async Task OpenProjectFolderForAutomationAsync(string folderPath)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        CloseSourceEditorHoverToolTip();
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            viewModel.ShowOutputMessage($"Automation open folder failed: '{folderPath}' does not exist.");
            return;
        }

        if (!TryResolveDirtyNavigationBeforeLeavingCurrentFile(viewModel))
            return;

        ResetEditableSessionToReadOnly();
        await viewModel.OpenProjectFolderAsync(folderPath);
        ReloadReadonlySourceHighlighting(viewModel);
        ResetEditableSessionToReadOnly();
    }

    private void OpenSearchToolWindow(object sender, RoutedEventArgs e)
    {
        ShowAndActivateSearchTool();
    }

    private void OpenFieldRegistryManagerWindow(object sender, RoutedEventArgs e)
    {
        if (_fieldRegistryCenterWindow is { IsVisible: true })
        {
            _fieldRegistryCenterWindow.Activate();
            return;
        }

        _fieldRegistryManagerViewModel.RefreshFromState(_fieldRegistryRuntimeService.CurrentState);
        string? projectRootPath = DataContext is ShellViewModel shellViewModel
            ? shellViewModel.CurrentProjectRootPath
            : null;
        _fieldRegistryCenterWindow = new FieldRegistryCenterWindow(
            _fieldRegistryManagerViewModel,
            _fieldRegistryRuntimeService.CurrentProvider,
            _fieldRegistryRuntimeService.CurrentProvenanceProvider,
            projectRootPath,
            _fieldRegistryRuntimeService.GetGlobalRootDirectoryPath())
        {
            Owner = this
        };

        _fieldRegistryCenterWindow.ReloadLocalFieldRegistryRequested += FieldRegistryCenterWindow_OnReloadLocalFieldRegistryRequested;
        _fieldRegistryCenterWindow.FieldLearningRequested += FieldRegistryCenterWindow_OnFieldLearningRequested;
        _fieldRegistryCenterWindow.AdvancedToolsRequested += FieldRegistryCenterWindow_OnAdvancedToolsRequested;
        _fieldRegistryCenterWindow.Closed += FieldRegistryCenterWindow_OnClosed;
        _fieldRegistryCenterWindow.Show();
    }

    private void OpenAdvancedFieldRegistryToolsWindow(object sender, RoutedEventArgs e)
    {
        if (_fieldRegistryManagerWindow is { IsVisible: true })
        {
            _fieldRegistryManagerWindow.Activate();
            return;
        }

        _fieldRegistryManagerViewModel.RefreshFromState(_fieldRegistryRuntimeService.CurrentState);
        _fieldRegistryManagerWindow = new FieldRegistryManagerWindow
        {
            Owner = this,
            DataContext = _fieldRegistryManagerViewModel
        };

        _fieldRegistryManagerWindow.ReloadLocalFieldRegistryRequested += FieldRegistryManagerWindow_OnReloadLocalFieldRegistryRequested;
        _fieldRegistryManagerWindow.HarvestPreviewRequested += FieldRegistryManagerWindow_OnHarvestPreviewRequested;
        _fieldRegistryManagerWindow.RelearnCurrentIniRequested += FieldRegistryManagerWindow_OnRelearnCurrentIniRequested;
        _fieldRegistryManagerWindow.CleanupApplied += FieldRegistryManagerWindow_OnCleanupApplied;
        _fieldRegistryManagerWindow.OpenGlobalRegistryFolderRequested += FieldRegistryManagerWindow_OnOpenGlobalRegistryFolderRequested;
        _fieldRegistryManagerWindow.OpenProjectRegistryFolderRequested += FieldRegistryManagerWindow_OnOpenProjectRegistryFolderRequested;
        _fieldRegistryManagerWindow.RefreshRollbackManifestsRequested += FieldRegistryManagerWindow_OnRefreshRollbackManifestsRequested;
        _fieldRegistryManagerWindow.OpenRollbackTargetFolderRequested += FieldRegistryManagerWindow_OnOpenRollbackTargetFolderRequested;
        _fieldRegistryManagerWindow.OpenRollbackManifestFolderRequested += FieldRegistryManagerWindow_OnOpenRollbackManifestFolderRequested;
        _fieldRegistryManagerWindow.OpenRollbackBackupFolderRequested += FieldRegistryManagerWindow_OnOpenRollbackBackupFolderRequested;
        _fieldRegistryManagerWindow.RollbackCompleted += FieldRegistryManagerWindow_OnRollbackCompleted;
        _fieldRegistryManagerWindow.Closed += FieldRegistryManagerWindow_OnClosed;
        RefreshFieldRegistryRollbackManifests();
        _fieldRegistryManagerWindow.Show();
    }

    private void FieldRegistryCenterWindow_OnReloadLocalFieldRegistryRequested(object? sender, EventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);
    }

    private void FieldRegistryCenterWindow_OnFieldLearningRequested(object? sender, EventArgs e)
        => OpenFieldLearningWizardWindow();

    private void FieldRegistryCenterWindow_OnAdvancedToolsRequested(object? sender, EventArgs e)
        => OpenAdvancedFieldRegistryToolsWindow(this, new RoutedEventArgs());

    private void FieldRegistryCenterWindow_OnClosed(object? sender, EventArgs e)
    {
        if (_fieldRegistryCenterWindow is not null)
        {
            _fieldRegistryCenterWindow.ReloadLocalFieldRegistryRequested -= FieldRegistryCenterWindow_OnReloadLocalFieldRegistryRequested;
            _fieldRegistryCenterWindow.FieldLearningRequested -= FieldRegistryCenterWindow_OnFieldLearningRequested;
            _fieldRegistryCenterWindow.AdvancedToolsRequested -= FieldRegistryCenterWindow_OnAdvancedToolsRequested;
            _fieldRegistryCenterWindow.Closed -= FieldRegistryCenterWindow_OnClosed;
        }

        _fieldRegistryCenterWindow = null;
    }

    private void OpenFieldLearningFromCurrentIni(object sender, RoutedEventArgs e)
        => OpenFieldLearningWizardWindow(GetCurrentIniSourceForFieldRegistryHarvest());

    private void OpenFieldLearningFromCurrentSection(object sender, RoutedEventArgs e)
    {
        if (!TryGetCurrentSectionSourceForFieldRegistryHarvest(out FieldRegistryCurrentIniSource? source, out string message))
        {
            if (DataContext is ShellViewModel viewModel)
                viewModel.ShowOutputMessage(message);

            return;
        }

        OpenFieldLearningWizardWindow(source);
    }

    private void OpenFieldLearningWizardWindow(FieldRegistryCurrentIniSource? initialSource = null)
    {
        if (_fieldLearningWizardWindow is { IsVisible: true })
        {
            if (initialSource is not null)
                LoadFieldLearningSource(_fieldLearningWizardWindow, initialSource);

            _fieldLearningWizardWindow.Activate();
            return;
        }

        FieldRegistryHarvestPreviewViewModel viewModel = CreateFieldRegistryHarvestPreviewViewModel();
        _fieldLearningWizardWindow = new FieldLearningWizardWindow(viewModel, GetCurrentIniSourceForFieldRegistryHarvest)
        {
            Owner = this
        };
        _fieldLearningWizardWindow.Closed += (_, _) => _fieldLearningWizardWindow = null;
        if (initialSource is not null)
            LoadFieldLearningSource(_fieldLearningWizardWindow, initialSource);

        _fieldLearningWizardWindow.Show();
    }

    private static void LoadFieldLearningSource(FieldLearningWizardWindow window, FieldRegistryCurrentIniSource source)
    {
        if (window.DataContext is not FieldRegistryHarvestPreviewViewModel viewModel)
            return;

        viewModel.RawText = source.Text;
        viewModel.LoadCurrentIniHarvestPreview(source.SourceName, source.Text);
    }

    private FieldRegistryHarvestPreviewViewModel CreateFieldRegistryHarvestPreviewViewModel()
        => new(
            new MarkdownFieldRegistryHarvestParser(),
            new FieldRegistryHarvestNormalizer(),
            new FieldRegistryHarvestPreviewBuilder(),
            new FieldRegistryHarvestDiffService(),
            () => _fieldRegistryRuntimeService.CurrentProvenanceProvider,
            new FieldRegistryApplyPlanBuilder(),
            new FieldRegistryApplyWriter(),
            () => DataContext is ShellViewModel viewModel ? viewModel.CurrentProjectRootPath : null,
            _fieldRegistryRuntimeService.GetGlobalRootDirectoryPath,
            () =>
            {
                if (DataContext is ShellViewModel viewModel)
                    ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);
            });

    private void OpenIssuesToolWindow(object sender, RoutedEventArgs e)
    {
        if (_issuesToolWindow is { IsVisible: true })
        {
            _issuesToolWindow.Activate();
            return;
        }

        if (DataContext is not ShellViewModel viewModel)
            return;

        _issuesToolWindow = new IssuesToolWindow
        {
            Owner = this,
            DataContext = viewModel
        };

        _issuesToolWindow.IssueNavigateRequested += IssuesToolWindow_OnIssueNavigateRequested;
        _issuesToolWindow.ClearIssuesRequested += IssuesToolWindow_OnClearIssuesRequested;
        _issuesToolWindow.ClearIssueFiltersRequested += IssuesToolWindow_OnClearIssueFiltersRequested;
        _issuesToolWindow.RefreshCurrentFileDiagnosticsRequested += IssuesToolWindow_OnRefreshCurrentFileDiagnosticsRequested;
        _issuesToolWindow.RunManualFullDiagnosticsRequested += IssuesToolWindow_OnRunManualFullDiagnosticsRequested;
        _issuesToolWindow.Closed += (_, _) => _issuesToolWindow = null;
        _issuesToolWindow.Show();
    }

    private void CloseShell_OnClick(object sender, RoutedEventArgs e)
        => Close();

    private void ShellTitleBarSystemMenuButton_OnClick(object sender, RoutedEventArgs e)
    {
        Point menuOrigin = ShellTitleBarSystemMenuButton.PointToScreen(
            new Point(0, ShellTitleBarSystemMenuButton.ActualHeight));
        _windowChromeController.ShowSystemMenu(menuOrigin);
    }

    private void ShellTitleBarMinimizeButton_OnClick(object sender, RoutedEventArgs e)
        => SystemCommands.MinimizeWindow(this);

    private void ShellTitleBarMaximizeRestoreButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            SystemCommands.RestoreWindow(this);
        else
            SystemCommands.MaximizeWindow(this);
    }

    private void ShellTitleBarCloseButton_OnClick(object sender, RoutedEventArgs e)
        => Close();

    private void OpenWindowLayoutMenu_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { ContextMenu: { } menu } button)
            return;

        menu.PlacementTarget = button;
        menu.IsOpen = true;
    }

    private void ReturnFloatingToolsHome_OnClick(object sender, RoutedEventArgs e)
        => _dockLayoutCoordinator.ReturnFloatingToolsHome();

    private void ResetDefaultDockLayout_OnClick(object sender, RoutedEventArgs e)
    {
        _isApplyingProjectExplorerVisibility = true;
        try
        {
            ShellDockLayoutOperationResult resetResult = _dockLayoutSession.ResetToCompiledDefault();
            if (!resetResult.Succeeded)
            {
                ReportDockLayoutFailure("无法恢复默认窗口布局。", resetResult);
                return;
            }
            RebindSectionExplorerVisibilitySource();
            SynchronizeShellStateFromDockLayout();
            PersistCurrentDockLayout("默认窗口布局已恢复，但无法保存到本机。", reportSuccess: false);
        }
        finally
        {
            _isApplyingProjectExplorerVisibility = false;
        }
    }

    private void ShellDockManager_OnAnchorableHiding(object? sender, AnchorableHidingEventArgs e)
    {
        if (!_dockLayoutCoordinator.TryBeginFloatingHideRecovery(e.Anchorable))
            return;

        e.Cancel = true;
        Dispatcher.BeginInvoke(
            () => _dockLayoutCoordinator.ReturnToolHome(e.Anchorable, activate: true),
            DispatcherPriority.ContextIdle);
    }

    private void FocusIssuesToolTab(object sender, RoutedEventArgs e)
    {
        ShowAndActivateBottomTool("Tool.Problems", BottomIssuesGrid);
    }

    private void FocusOutputToolTab(object sender, RoutedEventArgs e)
    {
        ShowAndActivateBottomTool("Tool.Output", BottomOutputTab);
    }

    private void FocusSearchResultsToolTab(object sender, RoutedEventArgs e)
    {
        ShowAndActivateSearchTool();
    }

    private void ToggleBottomToolPanel(object sender, RoutedEventArgs e)
    {
        _isBottomToolPanelVisible = !_isBottomToolPanelVisible;
        ApplyBottomToolPanelVisibility();
    }

    private async void BottomIssuesGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        await TryNavigateToIssueAsync(viewModel, viewModel.Issues.SelectedIssue);
    }

    private void ClearIssuesFromShell(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            viewModel.ClearIssues();
    }

    private void ClearIssueFiltersFromShell(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            viewModel.ClearIssueFilters();
    }

    private void RefreshCurrentFileDiagnosticsFromShell(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            viewModel.RefreshCurrentFileDiagnostics(
                SourceTextEditor.Document.Text,
                _fieldRegistryRuntimeService.CurrentProvider);
    }

    private async void RunManualFullDiagnosticsFromShell(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            await RunManualFullDiagnosticsWithFeedbackAsync(viewModel);
    }

    private void ReloadFieldRegistryFromShell(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);
    }

    private void IssuesToolWindow_OnClearIssuesRequested(object? sender, EventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            viewModel.ClearIssues();
    }

    private void IssuesToolWindow_OnClearIssueFiltersRequested(object? sender, EventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            viewModel.ClearIssueFilters();
    }

    private void IssuesToolWindow_OnRefreshCurrentFileDiagnosticsRequested(object? sender, EventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            viewModel.RefreshCurrentFileDiagnostics(
                SourceTextEditor.Document.Text,
                _fieldRegistryRuntimeService.CurrentProvider);
    }

    private async void IssuesToolWindow_OnRunManualFullDiagnosticsRequested(object? sender, EventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            await RunManualFullDiagnosticsWithFeedbackAsync(viewModel);
    }

    private async Task RunManualFullDiagnosticsWithFeedbackAsync(ShellViewModel viewModel)
    {
        bool hasProjectFiles = viewModel.ProjectExplorer.Items.Any(item => item.Kind == ProjectExplorerItemKind.File);
        if (!hasProjectFiles)
        {
            viewModel.SetOperationStatus("全量诊断未运行：没有可诊断的 INI 文件", "Warning");
            viewModel.ShowOutputMessage("当前没有可诊断的 INI 文件。");
            return;
        }

        viewModel.SetOperationStatus("正在运行全量诊断...", "Busy");
        try
        {
            await viewModel.RunManualFullDiagnosticsAsync(
                SourceTextEditor.Document.Text,
                _fieldRegistryRuntimeService.CurrentProvider);
            int issueCount = viewModel.Issues.TotalCount;
            string message = issueCount == 0
                ? "全量诊断完成：未发现问题"
                : $"全量诊断完成：发现 {issueCount} 个问题";
            viewModel.SetOperationStatus(message, issueCount == 0 ? "Success" : "Warning");
        }
        catch (Exception ex)
        {
            string message = $"全量诊断失败：{ShortenStatusReason(ex.Message)}";
            viewModel.SetOperationStatus(message, "Error");
            viewModel.ShowOutputMessage(message);
        }
    }

    private void ToggleProjectExplorer(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        viewModel.ToggleProjectExplorer();
        ApplyProjectExplorerVisibility();
    }

    private void OpenAiAssistantInRightToolWell(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel { IsProjectExplorerVisible: false } viewModel)
        {
            viewModel.ToggleProjectExplorer();
            ApplyProjectExplorerVisibility();
        }

        RefreshAiAssistantContextSummary();
        UpdateAiAssistantConfigurationStatus(
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot(GetSelectedAiAssistantModel()));
        SetRightToolWellAiViewVisible(true);
    }

    private void CloseAiAssistantInRightToolWell(object sender, RoutedEventArgs e)
    {
        if (_dockLayoutCoordinator.FindTool("Tool.AiAssistant") is { IsVisible: true } aiAssistant)
            aiAssistant.Hide();

        SetRightToolWellAiViewVisible(false);
    }

    private void ShowSectionTreeInRightToolWell(object sender, RoutedEventArgs e)
        => SetRightToolWellAiViewVisible(false);

    private void SetRightToolWellAiViewVisible(bool isAiVisible)
    {
        _dockLayoutCoordinator.ShowAndActivate(isAiVisible ? "Tool.AiAssistant" : "Tool.SectionExplorer");
    }

    private async void GenerateAiAssistantResponse(object sender, RoutedEventArgs e)
    {
        string rawPrompt = AiAssistantPromptBox.Text;
        if (rawPrompt.Length > AiAssistantMaximumUserPromptCharacters)
        {
            AiAssistantRequestPreparationNotice.Text =
                $"提示词不能超过 {AiAssistantMaximumUserPromptCharacters} 个字符；输入内容已保留，尚未发送。";
            AiAssistantRequestPreparationNotice.Visibility = Visibility.Visible;
            return;
        }

        string prompt = rawPrompt.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            RefreshAiAssistantContextSummary();
            return;
        }

        DeepSeekRa2AiModel selectedModel = GetSelectedAiAssistantModel();
        DeepSeekRa2AiConfigurationSnapshot configurationSnapshot =
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot(selectedModel);
        UpdateAiAssistantConfigurationStatus(configurationSnapshot);
        Ra2AiAuthoringRequestContext? authoringRequestContext = null;
        Ra2AiEditAvailabilityKind editAvailability = configurationSnapshot.State !=
            DeepSeekRa2AiConfigurationState.Ready
                ? Ra2AiEditAvailabilityKind.MissingConfiguration
                : configurationSnapshot.EndpointKind != DeepSeekRa2AiEndpointKind.Official
                    ? Ra2AiEditAvailabilityKind.UnsupportedEndpoint
                    : Ra2AiEditAvailabilityKind.SnapshotUnavailable;
        if (configurationSnapshot.State == DeepSeekRa2AiConfigurationState.Ready &&
            configurationSnapshot.EndpointKind == DeepSeekRa2AiEndpointKind.Official)
        {
            Ra2AuthoringSnapshotCaptureResult capture = CaptureCurrentAuthoringSnapshot();
            if (capture.Succeeded && capture.Snapshot is not null)
            {
                authoringRequestContext = new Ra2AiAuthoringRequestContext(capture.Snapshot);
                editAvailability = Ra2AiEditAvailabilityKind.Available;
            }
            else
            {
                editAvailability = capture.FailureKind is
                    Ra2AuthoringSnapshotCaptureFailureKind.NoEditableSession or
                    Ra2AuthoringSnapshotCaptureFailureKind.ReadOnly
                        ? Ra2AiEditAvailabilityKind.NoEditableDocument
                        : Ra2AiEditAvailabilityKind.SnapshotUnavailable;
            }
        }

        Ra2AiInteractionRoute interactionRoute = Ra2AiInteractionRouter.Resolve(
            prompt,
            editAvailability);
        if (interactionRoute.Kind == Ra2AiInteractionRouteKind.EditAmbiguous)
        {
            ShowLocalAiAuthoringRouteNotice(
                "请明确当前文件、Section、Key 和目标值；输入内容已保留，尚未发送。");
            return;
        }
        if (interactionRoute.Kind == Ra2AiInteractionRouteKind.EditUnavailable)
        {
            ShowLocalAiAuthoringRouteNotice(FormatAiEditUnavailableMessage(editAvailability));
            return;
        }

        Ra2AiAssistantPipeline pipeline = CreateAiAssistantPipeline(configurationSnapshot);

        if (!_aiAssistantRequestLifecycle.TryStart(out Ra2AiRequestSession? requestSession) || requestSession is null)
            return;

        InvalidateActiveAiEditProposal(markSuperseded: true);
        long requestGeneration = Volatile.Read(ref _aiAuthoringGeneration);

        AiAssistantRequestPreparationNotice.Visibility = Visibility.Collapsed;
        SetAiAssistantSendingState(true);
        Ra2AiContext? context = null;
        Border? userMessageBorder = null;
        AiAssistantStreamingMessageHandle? streamingMessage = null;
        try
        {
            Ra2AiConversationContext conversationContext = BuildAiAssistantConversationContext();
            Ra2AiCurrentSubject currentSubject = _aiCurrentSubjectExtractor.Extract(conversationContext);
            context = BuildCurrentAiContext(prompt, conversationContext, currentSubject);
            UpdateAiAssistantContextSummary(context, conversationContext, currentSubject);

            AiAssistantEmptyStateMessage.Visibility = Visibility.Collapsed;
            userMessageBorder = AddAiAssistantMessage(prompt, isUserMessage: true);
            AiAssistantPromptBox.Clear();
            AiAssistantChatScrollViewer.ScrollToEnd();

            streamingMessage = AddAiAssistantStreamingMessage(requestSession, userMessageBorder);
            AiAssistantStreamingMessageHandle requestStreamingMessage = streamingMessage;
            Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
                prompt,
                context,
                conversationContext,
                currentSubject,
                interactionRoute,
                (delta, callbackToken) => QueueAiAssistantContentDeltaAsync(
                    requestStreamingMessage,
                    delta,
                    callbackToken),
                requestSession.Token);
            UpdateAiAssistantRequestPreparationNotice(result.Request);
            if (result.Response.Kind == Ra2AiResponseKind.ToolCalls)
            {
                await PrepareAndAttachAiEditProposalAsync(
                    streamingMessage,
                    authoringRequestContext,
                    result.Response,
                    requestGeneration,
                    requestSession.Token);
            }
            else
            {
                FinalizeAiAssistantStreamingMessage(streamingMessage, result.Response);
            }

            RefreshAiAssistantContextSummary(context);
        }
        catch (OperationCanceledException)
        {
            const string responseText = "请求已取消。";
            if (streamingMessage is not null)
            {
                FinalizeFailedAiAssistantStreamingMessage(
                    streamingMessage,
                    responseText,
                    "请求已取消，以上内容不完整。",
                    Ra2AiConversationTurnState.Incomplete,
                    isErrorMessage: false);
            }
            else
            {
                AddAiAssistantMessage(
                    responseText,
                    isUserMessage: false,
                    turnState: Ra2AiConversationTurnState.Incomplete,
                    recoveryUserMessageBorder: userMessageBorder,
                    restorePrompt: prompt);
            }

            if (context is not null)
                RefreshAiAssistantContextSummary(context);
        }
        catch (Exception)
        {
            const string responseText = "DeepSeek 请求失败。请检查网络、代理或稍后重试。";
            if (streamingMessage is not null)
            {
                FinalizeFailedAiAssistantStreamingMessage(
                    streamingMessage,
                    responseText,
                    "响应处理失败，以上内容可能不完整。",
                    Ra2AiConversationTurnState.Error,
                    isErrorMessage: true);
            }
            else
            {
                AddAiAssistantMessage(
                    responseText,
                    isUserMessage: false,
                    isErrorMessage: true,
                    turnState: Ra2AiConversationTurnState.Error,
                    recoveryUserMessageBorder: userMessageBorder,
                    restorePrompt: prompt);
            }

            if (context is not null)
                RefreshAiAssistantContextSummary(context);
        }
        finally
        {
            if (streamingMessage is not null)
                ReleaseAiAssistantStreamingMessage(streamingMessage);

            bool completedCurrentRequest = _aiAssistantRequestLifecycle.TryComplete(requestSession);
            requestSession.Dispose();
            if (completedCurrentRequest)
            {
                SetAiAssistantSendingState(false);
                TrimAiAssistantTerminalMessageHistory();
            }
        }
    }

    private void CancelAiAssistantResponse(object sender, RoutedEventArgs e)
    {
        if (_aiAssistantRequestLifecycle.TryCancelCurrent())
            AiAssistantCancelButton.IsEnabled = false;
    }

    private void AiAssistantPromptBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            return;

        e.Handled = true;
        GenerateAiAssistantResponse(AiAssistantGenerateButton, new RoutedEventArgs());
    }

    private void ClearAiAssistantMessages(object sender, RoutedEventArgs e)
    {
        _aiAssistantRequestLifecycle.TryCancelCurrent();
        InvalidateActiveAiEditProposal(markSuperseded: false);
        for (int i = AiAssistantChatMessages.Children.Count - 1; i >= 0; i--)
        {
            if (AiAssistantChatMessages.Children[i] is FrameworkElement { Tag: "AiAssistantMessage" })
                AiAssistantChatMessages.Children.RemoveAt(i);
        }

        AiAssistantEmptyStateMessage.Visibility = Visibility.Visible;
        AiAssistantClearButton.IsEnabled = false;
        RefreshAiAssistantContextSummary();
    }

    private Border AddAiAssistantMessage(
        string text,
        bool isUserMessage,
        bool isErrorMessage = false,
        Ra2AiConversationTurnState turnState = Ra2AiConversationTurnState.Completed,
        Border? recoveryUserMessageBorder = null,
        string? restorePrompt = null)
    {
        Border messageBorder = CreateAiAssistantMessageBorder(
            text,
            isUserMessage,
            isErrorMessage,
            turnState);

        TextBlock messageText = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiMessageTextStyle"),
            Text = $"你：{text}",
            TextWrapping = TextWrapping.Wrap
        };

        if (isUserMessage)
        {
            messageBorder.Child = messageText;
        }
        else
        {
            (StackPanel responsePanel, StackPanel actionPanel, _) = AttachAiAssistantResponseLayout(messageBorder);
            AppendAiAssistantMarkdownBlocks(responsePanel, text);
            if (recoveryUserMessageBorder is not null && !string.IsNullOrWhiteSpace(restorePrompt))
            {
                SetAiAssistantMessageContextEligibility(recoveryUserMessageBorder, isContextEligible: false);
                AddAiAssistantRestorePromptAction(
                    responsePanel,
                    actionPanel,
                    restorePrompt,
                    recoveryUserMessageBorder);
            }
        }

        InsertAiAssistantMessage(messageBorder);
        return messageBorder;
    }

    private Border CreateAiAssistantMessageBorder(
        string text,
        bool isUserMessage,
        bool isErrorMessage,
        Ra2AiConversationTurnState turnState)
    {
        Border messageBorder = new()
        {
            Style = FindRequiredVisualResource<Style>(isUserMessage
                ? "IdeAiUserMessageStyle"
                : isErrorMessage
                    ? "IdeAiErrorMessageStyle"
                    : "IdeAiAssistantMessageStyle"),
            DataContext = new Ra2AiConversationTurn
            {
                Role = isUserMessage ? Ra2AiConversationRole.User : Ra2AiConversationRole.Assistant,
                Text = text,
                IsDraftResponse = !isUserMessage,
                State = turnState,
                IsContextEligible = turnState == Ra2AiConversationTurnState.Completed
            },
            Tag = "AiAssistantMessage"
        };

        AutomationProperties.SetAutomationId(
            messageBorder,
            isUserMessage ? "AiAssistant.UserMessageList" : "AiAssistant.AssistantMessageList");
        return messageBorder;
    }

    private (StackPanel ResponsePanel, StackPanel ActionPanel, Button CopyButton) AttachAiAssistantResponseLayout(
        Border messageBorder)
    {
        Grid messageGrid = new();
        messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        StackPanel responsePanel = new()
        {
            Margin = new Thickness(0, 0, 8, 0)
        };
        Grid.SetColumn(responsePanel, 0);
        messageGrid.Children.Add(responsePanel);

        StackPanel actionPanel = new()
        {
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(actionPanel, 1);
        messageGrid.Children.Add(actionPanel);

        Button copyButton = new()
        {
            Content = "复制",
            Padding = new Thickness(6, 2, 6, 2),
            VerticalAlignment = VerticalAlignment.Top,
            Style = (Style)FindResource("IdeBlueCompactButtonStyle")
        };
        AutomationProperties.SetAutomationId(copyButton, "AiAssistant.AssistantMessageCopyButton");
        copyButton.Click += (_, _) =>
        {
            if (messageBorder.DataContext is Ra2AiConversationTurn turn)
                CopyAiAssistantMessage(turn.Text);
        };

        actionPanel.Children.Add(copyButton);
        messageBorder.Child = messageGrid;
        return (responsePanel, actionPanel, copyButton);
    }

    private void InsertAiAssistantMessage(Border messageBorder)
    {
        int insertIndex = Math.Max(0, AiAssistantChatMessages.Children.Count - 1);
        AiAssistantChatMessages.Children.Insert(insertIndex, messageBorder);
    }

    private AiAssistantStreamingMessageHandle AddAiAssistantStreamingMessage(
        Ra2AiRequestSession requestSession,
        Border userMessageBorder)
    {
        if (_activeAiAssistantStreamingMessage is not null)
            throw new InvalidOperationException("An AI streaming message is already active.");

        Border messageBorder = CreateAiAssistantMessageBorder(
            string.Empty,
            isUserMessage: false,
            isErrorMessage: false,
            Ra2AiConversationTurnState.InProgress);
        (StackPanel responsePanel, StackPanel actionPanel, Button copyButton) =
            AttachAiAssistantResponseLayout(messageBorder);
        copyButton.IsEnabled = false;

        TextBlock prefixBlock = CreateAiAssistantTextBlock("助手：");
        AutomationProperties.SetAutomationId(prefixBlock, "AiAssistant.LatestAssistantMessage");
        responsePanel.Children.Add(prefixBlock);

        TextBlock streamingText = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiMessageTextStyle"),
            TextWrapping = TextWrapping.Wrap
        };
        responsePanel.Children.Add(streamingText);

        TextBlock statusText = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiMetadataTextStyle"),
            Margin = new Thickness(0, 4, 0, 0),
            Text = "正在生成…",
            TextWrapping = TextWrapping.Wrap
        };
        responsePanel.Children.Add(statusText);

        DispatcherTimer flushTimer = new(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(AiAssistantStreamFlushIntervalMilliseconds)
        };
        AiAssistantStreamingMessageHandle handle = new(
            requestSession,
            userMessageBorder,
            messageBorder,
            responsePanel,
            actionPanel,
            streamingText,
            statusText,
            copyButton,
            flushTimer);

        _activeAiAssistantStreamingMessage = handle;
        flushTimer.Tick += AiAssistantStreamFlushTimer_OnTick;
        InsertAiAssistantMessage(messageBorder);
        flushTimer.Start();
        AiAssistantChatScrollViewer.ScrollToEnd();
        return handle;
    }

    private ValueTask QueueAiAssistantContentDeltaAsync(
        AiAssistantStreamingMessageHandle handle,
        string delta,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        if (Volatile.Read(ref handle.FinalizationState) != 0)
            return ValueTask.CompletedTask;

        handle.Buffer.Append(delta);
        if (handle.Buffer.PendingCharacterCount >= AiAssistantStreamImmediateFlushThresholdCharacters)
            ScheduleImmediateAiAssistantStreamFlush(handle);

        return ValueTask.CompletedTask;
    }

    private void ScheduleImmediateAiAssistantStreamFlush(AiAssistantStreamingMessageHandle handle)
    {
        if (Volatile.Read(ref handle.FinalizationState) != 0
            || Dispatcher.HasShutdownStarted
            || Dispatcher.HasShutdownFinished
            || Interlocked.CompareExchange(ref handle.ImmediateFlushScheduled, 1, 0) != 0)
        {
            return;
        }

        try
        {
            _ = Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    FlushAiAssistantStreamingMessage(handle);
                }
                finally
                {
                    Interlocked.Exchange(ref handle.ImmediateFlushScheduled, 0);
                    if (Volatile.Read(ref handle.FinalizationState) == 0
                        && handle.Buffer.PendingCharacterCount >= AiAssistantStreamImmediateFlushThresholdCharacters)
                    {
                        ScheduleImmediateAiAssistantStreamFlush(handle);
                    }
                }
            }));
        }
        catch (InvalidOperationException) when (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            Interlocked.Exchange(ref handle.ImmediateFlushScheduled, 0);
        }
    }

    private void AiAssistantStreamFlushTimer_OnTick(object? sender, EventArgs e)
    {
        if (_activeAiAssistantStreamingMessage is { } handle
            && ReferenceEquals(sender, handle.FlushTimer))
        {
            FlushAiAssistantStreamingMessage(handle);
        }
    }

    private void FlushAiAssistantStreamingMessage(AiAssistantStreamingMessageHandle handle)
    {
        if (!ReferenceEquals(_activeAiAssistantStreamingMessage, handle)
            || Volatile.Read(ref handle.FinalizationState) != 0)
        {
            return;
        }

        bool shouldFollow = IsAiAssistantChatNearBottom();
        if (!AppendPendingAiAssistantStreamText(handle))
            return;

        handle.StatusText.Visibility = Visibility.Collapsed;
        handle.MessageBorder.DataContext = CreateAiAssistantConversationTurn(
            handle.Buffer.GetAccumulatedText(),
            Ra2AiConversationTurnState.InProgress);
        if (shouldFollow)
            AiAssistantChatScrollViewer.ScrollToEnd();
    }

    private static bool AppendPendingAiAssistantStreamText(AiAssistantStreamingMessageHandle handle)
    {
        string pendingText = handle.Buffer.DrainPending();
        if (pendingText.Length == 0)
            return false;

        handle.StreamingText.Inlines.Add(new Run(pendingText));
        return true;
    }

    private void FinalizeAiAssistantStreamingMessage(
        AiAssistantStreamingMessageHandle handle,
        Ra2AiResponse response)
    {
        if (!TryBeginAiAssistantStreamingFinalization(handle, out bool shouldFollow))
            return;

        AppendPendingAiAssistantStreamText(handle);
        string accumulatedText = handle.Buffer.GetAccumulatedText();
        bool isSuccessfulResponseConsistent = response.Kind != Ra2AiResponseKind.Success
            || (!string.IsNullOrWhiteSpace(response.Text)
                && handle.Buffer.AccumulatedTextEquals(response.Text));
        if (!isSuccessfulResponseConsistent)
        {
            string inconsistentText = response.Text.Length > 0 ? response.Text : accumulatedText;
            RenderFinalAiAssistantStreamingMessage(
                handle,
                string.IsNullOrWhiteSpace(inconsistentText) ? "AI 未返回可显示内容。" : inconsistentText,
                "流式响应一致性校验失败，本次回答不会进入后续对话上下文。",
                Ra2AiConversationTurnState.Error,
                isErrorMessage: true,
                diagnostics: null,
                shouldFollow);
            return;
        }

        bool hasPartialText = response.Text.Length > 0;
        string displayText = hasPartialText
            ? response.Text
            : FormatDeepSeekAiAssistantResponse(response);
        string? terminalStatus = hasPartialText
            ? GetAiAssistantTerminalStatus(response)
            : null;
        RenderFinalAiAssistantStreamingMessage(
            handle,
            displayText,
            terminalStatus,
            GetAiAssistantConversationTurnState(response.Kind),
            IsAiAssistantErrorMessage(response.Kind),
            response.Kind == Ra2AiResponseKind.Success ? null : response.Diagnostics,
            shouldFollow);
    }

    private async Task PrepareAndAttachAiEditProposalAsync(
        AiAssistantStreamingMessageHandle handle,
        Ra2AiAuthoringRequestContext? requestContext,
        Ra2AiResponse response,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        if (requestContext is null)
        {
            FinalizeFailedAiAssistantStreamingMessage(
                handle,
                "当前请求没有绑定可编辑文档快照，结构化修改建议已拒绝。",
                "结构化修改建议未完成。",
                Ra2AiConversationTurnState.Error,
                isErrorMessage: true);
            return;
        }

        Ra2AuthoringSnapshotCaptureResult currentCapture = CaptureCurrentAuthoringSnapshot();
        if (!currentCapture.Succeeded || currentCapture.Snapshot is null)
        {
            FinalizeFailedAiAssistantStreamingMessage(
                handle,
                currentCapture.FailureMessage ?? "当前文档状态无法用于生成修改预览。",
                "结构化修改建议未完成。",
                Ra2AiConversationTurnState.Error,
                isErrorMessage: true);
            return;
        }

        Ra2AiEditProposalResult proposalResult = await _aiProposalPreparationRunner.PrepareAsync(
            requestContext,
            currentCapture.Snapshot,
            response,
            cancellationToken);
        if (_isShellClosed ||
            cancellationToken.IsCancellationRequested ||
            requestGeneration != Volatile.Read(ref _aiAuthoringGeneration) ||
            !ReferenceEquals(_activeAiAssistantStreamingMessage, handle))
        {
            _aiAuthoringCoordinator.InvalidateActiveProposal();
            FinalizeFailedAiAssistantStreamingMessage(
                handle,
                "当前请求已失效，请重新发送修改请求。",
                "当前请求已失效。",
                Ra2AiConversationTurnState.Incomplete,
                isErrorMessage: false);
            return;
        }

        if (proposalResult.NeedsClarification)
        {
            RestoreAiAuthoringPromptIfEmpty(handle);
            RenderLocalAiAuthoringTerminalMessage(
                handle,
                proposalResult.Message,
                Ra2AiConversationTurnState.Completed,
                isErrorMessage: false);
            return;
        }

        if (!proposalResult.Succeeded || proposalResult.Proposal is null)
        {
            FinalizeFailedAiAssistantStreamingMessage(
                handle,
                proposalResult.Message,
                "结构化修改建议未完成。",
                proposalResult.FailureKind == Ra2AiEditProposalFailureKind.PreviewCancelled
                    ? Ra2AiConversationTurnState.Incomplete
                    : Ra2AiConversationTurnState.Error,
                isErrorMessage: proposalResult.FailureKind != Ra2AiEditProposalFailureKind.PreviewCancelled);
            return;
        }

        Ra2AiEditProposal proposal = proposalResult.Proposal;
        long fieldRegistryRevision = _fieldRegistryRuntimeService.CaptureProviderSnapshot().Revision;
        Ra2IniEditPreviewCurrencyResult currency = _authoringPreviewCurrencyEvaluator.Evaluate(
            proposal.Preview,
            _editableSession,
            SourceTextEditor.Document.Text,
            fieldRegistryRevision);
        if (!currency.IsCurrent ||
            !ReferenceEquals(_aiAuthoringCoordinator.ActiveProposal, proposal))
        {
            _aiAuthoringCoordinator.InvalidateActiveProposal();
            FinalizeFailedAiAssistantStreamingMessage(
                handle,
                "请求期间当前文档或字段库已经变化，请重新发送修改请求。",
                "结构化修改建议已经失效。",
                Ra2AiConversationTurnState.Error,
                isErrorMessage: true);
            return;
        }

        FinalizeAiAssistantStreamingMessage(handle, response);

        Ra2AiEditProposalViewModel viewModel = new(proposal);
        Ra2AiEditProposalView view = new()
        {
            DataContext = viewModel
        };
        view.ApplyRequested += AiEditProposalView_OnApplyRequested;
        view.DismissRequested += AiEditProposalView_OnDismissRequested;
        handle.ResponsePanel.Children.Add(view);
        handle.MessageBorder.DataContext = CreateAiAssistantConversationTurn(
            $"结构化修改建议：{proposal.Preview.Plan.Summary}",
            Ra2AiConversationTurnState.Completed);
        _activeAiEditProposalViewModel = viewModel;
        _activeAiEditProposalView = view;
        _activeAiEditProposalMessageBorder = handle.MessageBorder;
        AiAssistantChatScrollViewer.ScrollToEnd();
    }

    private void RenderLocalAiAuthoringTerminalMessage(
        AiAssistantStreamingMessageHandle handle,
        string message,
        Ra2AiConversationTurnState turnState,
        bool isErrorMessage)
    {
        if (!TryBeginAiAssistantStreamingFinalization(handle, out bool shouldFollow))
            return;

        RenderFinalAiAssistantStreamingMessage(
            handle,
            message,
            terminalStatus: null,
            turnState,
            isErrorMessage,
            diagnostics: null,
            shouldFollow);
    }

    private void RestoreAiAuthoringPromptIfEmpty(AiAssistantStreamingMessageHandle handle)
    {
        if (!string.IsNullOrEmpty(AiAssistantPromptBox.Text))
            return;

        AiAssistantPromptBox.Text = GetAiAssistantMessageText(handle.UserMessageBorder);
        AiAssistantPromptBox.CaretIndex = AiAssistantPromptBox.Text.Length;
    }

    private void AddAiEditProposalFailure(
        AiAssistantStreamingMessageHandle handle,
        string message)
    {
        TextBlock failureText = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiMetadataTextStyle"),
            Margin = new Thickness(0, 6, 0, 0),
            Text = string.IsNullOrWhiteSpace(message)
                ? "无法生成结构化修改预览。"
                : message.Trim(),
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(
            failureText,
            "AiAssistant.EditProposalFailure");
        handle.ResponsePanel.Children.Add(failureText);
        handle.MessageBorder.DataContext = CreateAiAssistantConversationTurn(
            failureText.Text,
            Ra2AiConversationTurnState.Error);
        SetAiAssistantMessageContextEligibility(
            handle.UserMessageBorder,
            isContextEligible: false);
        AiAssistantChatScrollViewer.ScrollToEnd();
    }

    private void AiEditProposalView_OnApplyRequested(object? sender, EventArgs e)
    {
        if (sender is not Ra2AiEditProposalView view ||
            !ReferenceEquals(view, _activeAiEditProposalView) ||
            _activeAiEditProposalViewModel is not { } viewModel)
        {
            return;
        }

        viewModel.BeginApply();
        Ra2AiEditProposalApplyResult result =
            _aiAuthoringCoordinator.ApplyConfirmed(viewModel.Proposal);
        if (result.Succeeded)
        {
            viewModel.MarkApplied(result.Message);
        }
        else if (result.FailureKind == Ra2AiEditProposalFailureKind.RequestContextStale)
        {
            viewModel.MarkStale(result.Message);
        }
        else
        {
            viewModel.MarkFailed(result.Message);
        }

        DetachActiveAiEditProposalView();
        RefreshAiAssistantContextSummary();
    }

    private void AiEditProposalView_OnDismissRequested(object? sender, EventArgs e)
    {
        if (sender is not Ra2AiEditProposalView view ||
            !ReferenceEquals(view, _activeAiEditProposalView) ||
            _activeAiEditProposalViewModel is not { } viewModel)
        {
            return;
        }

        if (_aiAuthoringCoordinator.Dismiss(viewModel.Proposal))
            viewModel.MarkDismissed();
        else
            viewModel.MarkStale("该修改建议已经失效。");

        DetachActiveAiEditProposalView();
    }

    private void InvalidateActiveAiEditProposal(bool markSuperseded)
    {
        Interlocked.Increment(ref _aiAuthoringGeneration);
        Ra2AiEditProposal? invalidated = _aiAuthoringCoordinator.InvalidateActiveProposal();
        if (invalidated is not null &&
            _activeAiEditProposalViewModel is { } viewModel &&
            viewModel.Proposal.ProposalId == invalidated.ProposalId)
        {
            if (markSuperseded)
                viewModel.MarkSuperseded();
            else
                viewModel.MarkStale("当前文档、字段库或会话状态已经变化，请重新生成修改建议。");
        }

        if (invalidated is not null)
            DetachActiveAiEditProposalView();
    }

    private void DetachActiveAiEditProposalView()
    {
        if (_activeAiEditProposalView is { } view)
        {
            view.ApplyRequested -= AiEditProposalView_OnApplyRequested;
            view.DismissRequested -= AiEditProposalView_OnDismissRequested;
        }

        _activeAiEditProposalView = null;
        _activeAiEditProposalViewModel = null;
        _activeAiEditProposalMessageBorder = null;
    }

    private void FinalizeFailedAiAssistantStreamingMessage(
        AiAssistantStreamingMessageHandle handle,
        string fallbackText,
        string partialTextStatus,
        Ra2AiConversationTurnState turnState,
        bool isErrorMessage)
    {
        if (!TryBeginAiAssistantStreamingFinalization(handle, out bool shouldFollow))
            return;

        AppendPendingAiAssistantStreamText(handle);
        string accumulatedText = handle.Buffer.GetAccumulatedText();
        bool hasPartialText = accumulatedText.Length > 0;
        RenderFinalAiAssistantStreamingMessage(
            handle,
            hasPartialText ? accumulatedText : fallbackText,
            hasPartialText ? partialTextStatus : null,
            turnState,
            isErrorMessage,
            diagnostics: null,
            shouldFollow);
    }

    private bool TryBeginAiAssistantStreamingFinalization(
        AiAssistantStreamingMessageHandle handle,
        out bool shouldFollow)
    {
        shouldFollow = false;
        if (!ReferenceEquals(_activeAiAssistantStreamingMessage, handle)
            || Interlocked.CompareExchange(ref handle.FinalizationState, 1, 0) != 0)
        {
            return false;
        }

        shouldFollow = IsAiAssistantChatNearBottom();
        StopAiAssistantStreamingTimer(handle);
        return true;
    }

    private void RenderFinalAiAssistantStreamingMessage(
        AiAssistantStreamingMessageHandle handle,
        string text,
        string? terminalStatus,
        Ra2AiConversationTurnState turnState,
        bool isErrorMessage,
        Ra2AiRequestDiagnostics? diagnostics,
        bool shouldFollow)
    {
        handle.ResponsePanel.Children.Clear();
        AppendAiAssistantMarkdownBlocks(handle.ResponsePanel, text);
        if (!string.IsNullOrWhiteSpace(terminalStatus))
        {
            handle.StatusText.Text = terminalStatus;
            handle.StatusText.Visibility = Visibility.Visible;
            handle.ResponsePanel.Children.Add(handle.StatusText);
        }

        if (diagnostics is not null)
            AddAiAssistantRequestDiagnostics(handle.ResponsePanel, diagnostics);

        handle.MessageBorder.Style = FindRequiredVisualResource<Style>(isErrorMessage
            ? "IdeAiErrorMessageStyle"
            : "IdeAiAssistantMessageStyle");
        handle.MessageBorder.DataContext = CreateAiAssistantConversationTurn(text, turnState);
        bool isContextEligible = turnState == Ra2AiConversationTurnState.Completed;
        SetAiAssistantMessageContextEligibility(handle.UserMessageBorder, isContextEligible);
        if (!isContextEligible)
        {
            AddAiAssistantRestorePromptAction(
                handle.ResponsePanel,
                handle.ActionPanel,
                GetAiAssistantMessageText(handle.UserMessageBorder),
                handle.UserMessageBorder);
        }

        handle.CopyButton.IsEnabled = !string.IsNullOrWhiteSpace(text);
        Volatile.Write(ref handle.FinalizationState, 2);

        if (shouldFollow)
            AiAssistantChatScrollViewer.ScrollToEnd();
    }

    private static Ra2AiConversationTurn CreateAiAssistantConversationTurn(
        string text,
        Ra2AiConversationTurnState turnState)
        => new()
        {
            Role = Ra2AiConversationRole.Assistant,
            Text = text,
            IsDraftResponse = true,
            State = turnState,
            IsContextEligible = turnState == Ra2AiConversationTurnState.Completed
        };

    private void AddAiAssistantRestorePromptAction(
        StackPanel responsePanel,
        StackPanel actionPanel,
        string submittedPrompt,
        Border userMessageBorder)
    {
        string prompt = submittedPrompt.Trim();
        if (prompt.Length == 0)
            return;

        Button restoreButton = new()
        {
            Content = "恢复提示词",
            Margin = new Thickness(0, 4, 0, 0),
            Padding = new Thickness(6, 2, 6, 2),
            Style = (Style)FindResource("IdeBlueCompactButtonStyle")
        };
        AutomationProperties.SetAutomationId(restoreButton, "AiAssistant.RestorePromptButton");
        AutomationProperties.SetName(restoreButton, "恢复提示词");
        AutomationProperties.SetHelpText(
            restoreButton,
            "仅恢复文本，不会自动发送；再次发送可能产生服务费用。");

        TextBlock restoreStatus = new()
        {
            Foreground = (Brush)FindResource("ShellMutedTextBrush"),
            Margin = new Thickness(0, 4, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed
        };
        AutomationProperties.SetAutomationId(restoreStatus, "AiAssistant.RestorePromptStatus");

        restoreButton.Click += (_, _) =>
        {
            SetAiAssistantMessageContextEligibility(userMessageBorder, isContextEligible: false);
            if (!string.IsNullOrWhiteSpace(AiAssistantPromptBox.Text))
            {
                restoreStatus.Text = "输入框已有内容，未覆盖。";
                restoreStatus.Visibility = Visibility.Visible;
                return;
            }

            AiAssistantPromptBox.Text = prompt;
            AiAssistantPromptBox.Focus();
            AiAssistantPromptBox.CaretIndex = AiAssistantPromptBox.Text.Length;
            restoreStatus.Text = "提示词已恢复到输入框，尚未发送。";
            restoreStatus.Visibility = Visibility.Visible;
        };

        actionPanel.Children.Add(restoreButton);
        responsePanel.Children.Add(restoreStatus);
    }

    private static string GetAiAssistantMessageText(Border messageBorder)
        => messageBorder.DataContext is Ra2AiConversationTurn turn ? turn.Text : string.Empty;

    private static void SetAiAssistantMessageContextEligibility(
        Border messageBorder,
        bool isContextEligible)
    {
        if (messageBorder.DataContext is not Ra2AiConversationTurn turn
            || turn.IsContextEligible == isContextEligible)
        {
            return;
        }

        messageBorder.DataContext = new Ra2AiConversationTurn
        {
            Role = turn.Role,
            Text = turn.Text,
            IsDraftResponse = turn.IsDraftResponse,
            State = turn.State,
            IsContextEligible = isContextEligible
        };
    }

    private static bool IsAiAssistantErrorMessage(Ra2AiResponseKind responseKind)
        => responseKind is Ra2AiResponseKind.Timeout
            or Ra2AiResponseKind.ProviderError
            or Ra2AiResponseKind.MissingConfiguration
            or Ra2AiResponseKind.AuthoringToolNotInvoked;

    private static string? GetAiAssistantTerminalStatus(
        Ra2AiResponse response)
    {
        if (response.Kind is not Ra2AiResponseKind.Success
            && response.Kind is not Ra2AiResponseKind.Cancelled
            && response.FailureKind != Ra2AiFailureKind.None)
        {
            return DeepSeekRa2AiFailureUiMessageFormatter.FormatPartialTerminalStatus(
                response.FailureKind);
        }

        return response.Kind switch
        {
            Ra2AiResponseKind.Incomplete => response.FinishKind switch
            {
                Ra2AiStreamFinishKind.Length => "回答因长度限制提前结束，以上内容不完整。",
                Ra2AiStreamFinishKind.ContentFilter => "回答因内容过滤提前结束，以上内容不完整。",
                Ra2AiStreamFinishKind.ToolCalls => "模型请求了当前面板不支持的工具调用，以上内容不完整。",
                Ra2AiStreamFinishKind.InsufficientSystemResource => "服务资源不足，回答提前结束，以上内容不完整。",
                _ => "回答未正常结束，以上内容不完整。"
            },
            Ra2AiResponseKind.Cancelled => "请求已取消，以上内容不完整。",
            Ra2AiResponseKind.Timeout => "请求超时，以上内容可能不完整。",
            Ra2AiResponseKind.ProviderError => "DeepSeek 请求失败，以上内容可能不完整。",
            Ra2AiResponseKind.MissingConfiguration => "提供方配置缺失，以上内容可能不完整。",
            Ra2AiResponseKind.ToolCalls =>
                "编辑权限仅来自本地校验后的结构化操作；以上文字不改变建议内容。",
            Ra2AiResponseKind.AuthoringToolNotInvoked =>
                "未调用所需编辑工具，本次内容不会进入后续对话上下文。",
            _ => null
        };
    }

    private bool IsAiAssistantChatNearBottom()
        => AiAssistantChatScrollViewer.ScrollableHeight - AiAssistantChatScrollViewer.VerticalOffset
            <= AiAssistantAutoScrollTolerance;

    private void StopAiAssistantStreamingTimer(AiAssistantStreamingMessageHandle handle)
    {
        handle.FlushTimer.Stop();
        handle.FlushTimer.Tick -= AiAssistantStreamFlushTimer_OnTick;
    }

    private void ReleaseAiAssistantStreamingMessage(AiAssistantStreamingMessageHandle handle)
    {
        StopAiAssistantStreamingTimer(handle);
        if (Volatile.Read(ref handle.FinalizationState) == 0)
            Volatile.Write(ref handle.FinalizationState, 2);

        if (ReferenceEquals(_activeAiAssistantStreamingMessage, handle))
            _activeAiAssistantStreamingMessage = null;
    }

    private static void CopyAiAssistantMessage(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
            Clipboard.SetText(text);
    }

    private void AppendAiAssistantMarkdownBlocks(Panel responsePanel, string text)
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(text);
        TextBlock prefixBlock = CreateAiAssistantTextBlock("助手：");
        AutomationProperties.SetAutomationId(prefixBlock, "AiAssistant.LatestAssistantMessage");
        responsePanel.Children.Add(prefixBlock);

        if (RequiresAiAssistantPlainTextFallback(blocks))
        {
            responsePanel.Children.Add(CreateAiAssistantPlainTextFallback(text));
            return;
        }

        foreach (Ra2AiMarkdownBlock block in blocks)
        {
            if (block.IsCodeBlock)
            {
                responsePanel.Children.Add(CreateAiAssistantCodeBlock(block));
                continue;
            }

            responsePanel.Children.Add(CreateAiAssistantMarkdownBlock(block));
        }
    }

    private static bool RequiresAiAssistantPlainTextFallback(
        IReadOnlyList<Ra2AiMarkdownBlock> blocks)
    {
        if (blocks.Count > AiAssistantMaximumMarkdownBlocks
            || blocks.Count(block => block.IsCodeBlock) > AiAssistantMaximumMarkdownCodeBlocks)
        {
            return true;
        }

        long totalTableCells = 0;
        foreach (Ra2AiMarkdownBlock table in blocks.Where(block =>
            block.Kind == Ra2AiMarkdownBlockKind.Table))
        {
            if (table.TableRows.Count > AiAssistantMaximumMarkdownTableRows)
                return true;

            totalTableCells += (long)table.TableHeaders.Count * (table.TableRows.Count + 1L);
            if (totalTableCells > AiAssistantMaximumMarkdownTableCells)
                return true;
        }

        return false;
    }

    private FrameworkElement CreateAiAssistantPlainTextFallback(string text)
    {
        TextBox fallback = new()
        {
            AcceptsReturn = true,
            IsReadOnly = true,
            Style = FindRequiredVisualResource<Style>("IdeAiPlainTextFallbackStyle"),
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        AutomationProperties.SetAutomationId(fallback, "AiAssistant.MarkdownFallbackText");
        return fallback;
    }

    private TextBlock CreateAiAssistantTextBlock(string text)
    {
        TextBlock textBlock = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiMessageTextStyle"),
            TextWrapping = TextWrapping.Wrap
        };
        AppendAiAssistantInlineText(textBlock, text);
        return textBlock;
    }

    private FrameworkElement CreateAiAssistantMarkdownBlock(Ra2AiMarkdownBlock block)
        => block.Kind switch
        {
            Ra2AiMarkdownBlockKind.Heading => CreateAiAssistantHeadingBlock(block),
            Ra2AiMarkdownBlockKind.Bullet => CreateAiAssistantListBlock("•", block.Text),
            Ra2AiMarkdownBlockKind.Numbered => CreateAiAssistantListBlock("1.", block.Text),
            Ra2AiMarkdownBlockKind.Table => CreateAiAssistantTableBlock(block),
            _ => CreateAiAssistantParagraphBlock(block.Text)
        };

    private TextBlock CreateAiAssistantHeadingBlock(Ra2AiMarkdownBlock block)
    {
        TextBlock heading = CreateAiAssistantTextBlock(block.Text);
        heading.Style = FindRequiredVisualResource<Style>("IdeAiMarkdownHeadingStyle");
        heading.FontSize = block.HeadingLevel switch
        {
            1 => 15,
            2 => 13,
            _ => 12
        };
        AutomationProperties.SetAutomationId(heading, "AiAssistant.MarkdownHeading");
        return heading;
    }

    private TextBlock CreateAiAssistantParagraphBlock(string text)
    {
        TextBlock paragraph = CreateAiAssistantTextBlock(text);
        paragraph.Style = FindRequiredVisualResource<Style>("IdeAiMarkdownParagraphStyle");
        AutomationProperties.SetAutomationId(paragraph, "AiAssistant.MarkdownParagraph");
        return paragraph;
    }

    private FrameworkElement CreateAiAssistantListBlock(string marker, string text)
    {
        Grid listGrid = new()
        {
            Margin = new Thickness(0, 2, 0, 2)
        };
        AutomationProperties.SetAutomationId(listGrid, "AiAssistant.MarkdownListItem");
        listGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        listGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        TextBlock markerText = CreateAiAssistantTextBlock(marker);
        markerText.Margin = new Thickness(0, 0, 6, 0);
        Grid.SetColumn(markerText, 0);
        listGrid.Children.Add(markerText);

        TextBlock itemText = CreateAiAssistantTextBlock(text);
        Grid.SetColumn(itemText, 1);
        listGrid.Children.Add(itemText);
        return listGrid;
    }

    private FrameworkElement CreateAiAssistantTableBlock(Ra2AiMarkdownBlock block)
    {
        Border tableBorder = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiMarkdownTableStyle")
        };
        AutomationProperties.SetAutomationId(tableBorder, "AiAssistant.MarkdownTable");

        Grid tableGrid = new();
        int columnCount = block.TableHeaders.Count;
        for (int index = 0; index < columnCount; index++)
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddAiAssistantTableRow(tableGrid, block.TableHeaders, rowIndex: 0, isHeader: true);

        for (int index = 0; index < block.TableRows.Count; index++)
        {
            tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddAiAssistantTableRow(tableGrid, block.TableRows[index], index + 1, isHeader: false);
        }

        tableBorder.Child = tableGrid;
        return tableBorder;
    }

    private void AddAiAssistantTableRow(Grid tableGrid, IReadOnlyList<string> cells, int rowIndex, bool isHeader)
    {
        for (int columnIndex = 0; columnIndex < tableGrid.ColumnDefinitions.Count; columnIndex++)
        {
            Border cellBorder = new()
            {
                Style = FindRequiredVisualResource<Style>("IdeAiMarkdownTableCellStyle"),
                BorderThickness = new Thickness(
                    left: columnIndex == 0 ? 0 : 1,
                    top: rowIndex == 0 ? 0 : 1,
                    right: 0,
                    bottom: 0)
            };
            AutomationProperties.SetAutomationId(
                cellBorder,
                isHeader
                    ? "AiAssistant.MarkdownTableHeader"
                    : columnIndex == 0
                        ? "AiAssistant.MarkdownTableRow"
                        : "AiAssistant.MarkdownTableCell");

            TextBlock cellText = CreateAiAssistantTextBlock(columnIndex < cells.Count ? cells[columnIndex] : string.Empty);
            if (isHeader)
                cellText.FontWeight = FontWeights.SemiBold;

            cellBorder.Child = cellText;
            Grid.SetRow(cellBorder, rowIndex);
            Grid.SetColumn(cellBorder, columnIndex);
            tableGrid.Children.Add(cellBorder);
        }
    }

    private void AppendAiAssistantInlineText(TextBlock textBlock, string text)
    {
        int position = 0;
        while (position < text.Length)
        {
            int boldOpen = text.IndexOf("**", position, StringComparison.Ordinal);
            int codeOpen = text.IndexOf('`', position);
            if (boldOpen < 0 && codeOpen < 0)
            {
                textBlock.Inlines.Add(new Run(text[position..]));
                return;
            }

            if (codeOpen >= 0 && (boldOpen < 0 || codeOpen < boldOpen))
            {
                if (codeOpen > position)
                    textBlock.Inlines.Add(new Run(text[position..codeOpen]));

                int codeClose = text.IndexOf('`', codeOpen + 1);
                if (codeClose < 0)
                {
                    textBlock.Inlines.Add(new Run(text[codeOpen..]));
                    return;
                }

                string codeText = text[(codeOpen + 1)..codeClose];
                if (codeText.Length == 0)
                {
                    textBlock.Inlines.Add(new Run(text[codeOpen..(codeClose + 1)]));
                }
                else
                {
                    Run codeRun = new(codeText)
                    {
                        Style = FindRequiredVisualResource<Style>("IdeAiInlineCodeStyle")
                    };
                    AutomationProperties.SetAutomationId(codeRun, "AiAssistant.MarkdownInlineCode");
                    textBlock.Inlines.Add(codeRun);
                }

                position = codeClose + 1;
                continue;
            }

            if (boldOpen > position)
                textBlock.Inlines.Add(new Run(text[position..boldOpen]));

            int boldClose = text.IndexOf("**", boldOpen + 2, StringComparison.Ordinal);
            if (boldClose < 0)
            {
                textBlock.Inlines.Add(new Run(text[boldOpen..]));
                return;
            }

            string boldText = text[(boldOpen + 2)..boldClose];
            if (boldText.Length == 0 || boldText.Contains("**", StringComparison.Ordinal))
            {
                textBlock.Inlines.Add(new Run(text[boldOpen..(boldClose + 2)]));
            }
            else
            {
                textBlock.Inlines.Add(new Run(boldText) { FontWeight = FontWeights.Bold });
            }

            position = boldClose + 2;
        }
    }

    private FrameworkElement CreateAiAssistantCodeBlock(Ra2AiMarkdownBlock block)
    {
        Border codeBorder = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiCodeBlockStyle")
        };
        AutomationProperties.SetAutomationId(codeBorder, "AiAssistant.CodeBlock");

        StackPanel codePanel = new();
        Grid headerGrid = new()
        {
            Margin = new Thickness(0, 0, 0, 4)
        };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        TextBlock languageText = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeAiMetadataTextStyle"),
            Text = string.IsNullOrWhiteSpace(block.Language) ? "code" : block.Language,
            TextWrapping = TextWrapping.NoWrap
        };
        AutomationProperties.SetAutomationId(languageText, "AiAssistant.CodeBlockLanguage");
        Grid.SetColumn(languageText, 0);
        headerGrid.Children.Add(languageText);

        string codeText = block.Text;
        Button copyCodeButton = new()
        {
            Content = "复制代码",
            Padding = new Thickness(6, 2, 6, 2),
            Style = (Style)FindResource("IdeBlueCompactButtonStyle")
        };
        AutomationProperties.SetAutomationId(copyCodeButton, "AiAssistant.CodeBlockCopyButton");
        copyCodeButton.Click += (_, _) => CopyAiAssistantCodeBlock(codeText);
        Grid.SetColumn(copyCodeButton, 1);
        headerGrid.Children.Add(copyCodeButton);
        codePanel.Children.Add(headerGrid);

        TextBox codeTextBox = new()
        {
            AcceptsReturn = true,
            IsReadOnly = true,
            Style = FindRequiredVisualResource<Style>("IdeAiReadOnlyCodeTextStyle"),
            Text = codeText,
            TextWrapping = TextWrapping.NoWrap,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        codePanel.Children.Add(codeTextBox);

        codeBorder.Child = codePanel;
        return codeBorder;
    }

    private static void CopyAiAssistantCodeBlock(string codeText)
    {
        if (!string.IsNullOrWhiteSpace(codeText))
            Clipboard.SetText(codeText);
    }

    private Ra2AiConversationContext BuildAiAssistantConversationContext()
    {
        List<Ra2AiConversationTurn> turns = [];
        foreach (UIElement child in AiAssistantChatMessages.Children)
        {
            if (child is FrameworkElement { Tag: "AiAssistantMessage", DataContext: Ra2AiConversationTurn turn })
                turns.Add(turn);
        }

        return _aiConversationContextProvider.BuildContext(new Ra2AiConversationContextRequest
        {
            Turns = turns
        });
    }

    private Ra2AiAssistantPipeline CreateAiAssistantPipeline(
        DeepSeekRa2AiConfigurationSnapshot configurationSnapshot)
        => new(
            _aiPromptBuilder,
            DeepSeekRa2AiClientFactory.CreateClient(configurationSnapshot));

    private DeepSeekRa2AiModel GetSelectedAiAssistantModel()
        => AiAssistantModelSelector.SelectedValue is DeepSeekRa2AiModel selectedModel
            ? selectedModel
            : DeepSeekRa2AiModelCatalog.Default;

    private void UpdateAiAssistantConfigurationStatus(
        DeepSeekRa2AiConfigurationSnapshot snapshot)
    {
        string endpointKind = snapshot.UsesCustomEndpoint ? "自定义端点" : "官方端点";
        AiAssistantConfigurationStatusText.Text = snapshot.State switch
        {
            DeepSeekRa2AiConfigurationState.Ready => $"配置可用（{endpointKind}）",
            DeepSeekRa2AiConfigurationState.MissingApiKey => $"配置缺失：未设置 API Key（{endpointKind}）",
            DeepSeekRa2AiConfigurationState.InvalidBaseUrl => "配置无效：自定义端点不受信任",
            DeepSeekRa2AiConfigurationState.InvalidTimeout => "配置无效：超时必须为 10–600 秒整数",
            DeepSeekRa2AiConfigurationState.UnsupportedModel => "配置无效：模型不受支持",
            _ => "配置状态未知"
        };
    }

    private void UpdateAiAssistantRequestPreparationNotice(Ra2AiRequest request)
    {
        List<string> notices = [$"出站 prompt：{request.PromptCharacterCount} 个字符"];
        if (request.PreparationFlags.HasFlag(Ra2AiRequestPreparationFlags.SensitiveContentRedacted))
            notices.Add("已清理可能的敏感内容");
        if (request.PreparationFlags.HasFlag(Ra2AiRequestPreparationFlags.SelectedTextTruncated))
            notices.Add("显式选区已按预算截断");
        if (request.PreparationFlags.HasFlag(Ra2AiRequestPreparationFlags.ContextTruncated))
            notices.Add("上下文已按预算截断");
        if (request.PreparationFlags.HasFlag(Ra2AiRequestPreparationFlags.TotalPromptTruncated))
            notices.Add("最终 prompt 已按总预算截断");

        AiAssistantRequestPreparationNotice.Text = string.Join("；", notices) + "。";
        AiAssistantRequestPreparationNotice.Visibility = Visibility.Visible;
    }

    private static void AddAiAssistantRequestDiagnostics(
        Panel responsePanel,
        Ra2AiRequestDiagnostics diagnostics)
    {
        static string FormatDuration(TimeSpan? value)
            => value is null ? "未发生" : $"{value.Value.TotalMilliseconds:0} ms";

        string statusCode = diagnostics.HttpStatusCode?.ToString() ?? "无";
        TextBlock diagnosticsText = new()
        {
            Margin = new Thickness(0, 5, 0, 0),
            Foreground = Brushes.DimGray,
            FontSize = 10,
            Text = $"诊断详情：RequestId={diagnostics.RequestId}；Model={diagnostics.ModelId}；Headers={FormatDuration(diagnostics.TimeToHeaders)}；FirstContent={FormatDuration(diagnostics.TimeToFirstContent)}；Total={FormatDuration(diagnostics.TotalDuration)}；Deltas={diagnostics.ContentDeltaCount}；Characters={diagnostics.ContentCharacterCount}；HTTP={statusCode}",
            TextWrapping = TextWrapping.Wrap
        };
        AutomationProperties.SetAutomationId(diagnosticsText, "AiAssistant.RequestDiagnostics");
        responsePanel.Children.Add(diagnosticsText);
    }

    private static Ra2AiConversationTurnState GetAiAssistantConversationTurnState(Ra2AiResponseKind responseKind)
        => responseKind switch
        {
            Ra2AiResponseKind.Success or Ra2AiResponseKind.ToolCalls
                => Ra2AiConversationTurnState.Completed,
            Ra2AiResponseKind.Incomplete or Ra2AiResponseKind.Cancelled or Ra2AiResponseKind.Timeout
                => Ra2AiConversationTurnState.Incomplete,
            _ => Ra2AiConversationTurnState.Error
        };

    private static string FormatDeepSeekAiAssistantResponse(Ra2AiResponse response)
    {
        if (response.Kind == Ra2AiResponseKind.Success)
            return response.Text;

        if (response.Kind == Ra2AiResponseKind.ToolCalls)
            return "已收到结构化修改建议，正在生成本地预览。";

        if (response.Kind == Ra2AiResponseKind.Cancelled)
            return "请求已取消。";

        if (response.FailureKind != Ra2AiFailureKind.None)
        {
            return DeepSeekRa2AiFailureUiMessageFormatter.FormatStandaloneMessage(
                response.FailureKind);
        }

        return response.Kind switch
        {
            Ra2AiResponseKind.AuthoringToolNotInvoked =>
                "DeepSeek 未返回所需的结构化修改调用；本次内容未形成可应用建议。",
            Ra2AiResponseKind.Incomplete => "DeepSeek 响应未完整结束，请重试。",
            Ra2AiResponseKind.Timeout => "DeepSeek 请求超时，请稍后重试。",
            Ra2AiResponseKind.MissingConfiguration => "DeepSeek 未配置。请设置环境变量 DEEPSEEK_API_KEY 后重试。",
            Ra2AiResponseKind.ProviderError => "DeepSeek 请求失败。请检查网络、代理或稍后重试。",
            _ => "AI 请求返回了未知状态。"
        };
    }

    private void ShowLocalAiAuthoringRouteNotice(string message)
    {
        AiAssistantRequestPreparationNotice.Text = message;
        AiAssistantRequestPreparationNotice.Visibility = Visibility.Visible;
        RefreshAiAssistantContextSummary(AiAssistantPromptBox.Text);
    }

    private static string FormatAiEditUnavailableMessage(
        Ra2AiEditAvailabilityKind availability)
        => availability switch
        {
            Ra2AiEditAvailabilityKind.MissingConfiguration =>
                "DeepSeek 配置尚未就绪，无法生成编辑预览；输入内容已保留，尚未发送。",
            Ra2AiEditAvailabilityKind.UnsupportedEndpoint =>
                "自定义端点仅支持普通问答，无法生成编辑预览；输入内容已保留，尚未发送。",
            Ra2AiEditAvailabilityKind.NoEditableDocument =>
                "当前没有可编辑文档，无法生成编辑预览；输入内容已保留，尚未发送。",
            _ => "当前文档快照不可用，无法生成编辑预览；输入内容已保留，尚未发送。"
        };

    private void SetAiAssistantSendingState(bool isSending)
    {
        AiAssistantGenerateButton.IsEnabled = !isSending;
        AiAssistantCancelButton.IsEnabled = isSending;
        AiAssistantClearButton.IsEnabled = !isSending && HasAiAssistantMessages();
        AiAssistantModelSelector.IsEnabled = !isSending;
    }

    private bool HasAiAssistantMessages()
    {
        foreach (UIElement child in AiAssistantChatMessages.Children)
        {
            if (child is FrameworkElement { Tag: "AiAssistantMessage" })
                return true;
        }

        return false;
    }

    private void TrimAiAssistantTerminalMessageHistory()
    {
        while (GetAiAssistantTerminalMessageCardCount() > AiAssistantMaximumTerminalMessageCards)
        {
            int userIndex = -1;
            int assistantIndex = -1;
            for (int index = 0; index < AiAssistantChatMessages.Children.Count; index++)
            {
                if (AiAssistantChatMessages.Children[index] is not FrameworkElement
                    {
                        Tag: "AiAssistantMessage",
                        DataContext: Ra2AiConversationTurn turn
                    })
                {
                    continue;
                }

                if (userIndex < 0 && turn.Role == Ra2AiConversationRole.User)
                {
                    userIndex = index;
                    continue;
                }

                if (userIndex >= 0 && turn.Role == Ra2AiConversationRole.Assistant)
                {
                    assistantIndex = index;
                    break;
                }
            }

            if (userIndex < 0 || assistantIndex < 0)
                return;

            if (ReferenceEquals(
                    AiAssistantChatMessages.Children[assistantIndex],
                    _activeAiEditProposalMessageBorder))
            {
                return;
            }

            AiAssistantChatMessages.Children.RemoveAt(assistantIndex);
            AiAssistantChatMessages.Children.RemoveAt(userIndex);
        }
    }

    private int GetAiAssistantTerminalMessageCardCount()
    {
        int count = 0;
        foreach (UIElement child in AiAssistantChatMessages.Children)
        {
            if (child is FrameworkElement
                {
                    Tag: "AiAssistantMessage",
                    DataContext: Ra2AiConversationTurn
                    {
                        State: not Ra2AiConversationTurnState.InProgress
                    }
                })
            {
                count++;
            }
        }

        return count;
    }

    private void RefreshAiAssistantContextSummary(string? promptText = null)
        => RefreshAiAssistantContextSummary(BuildCurrentAiContext(promptText));

    private void RefreshAiAssistantContextSummary(Ra2AiContext context)
    {
        Ra2AiConversationContext conversationContext = BuildAiAssistantConversationContext();
        Ra2AiCurrentSubject currentSubject = _aiCurrentSubjectExtractor.Extract(conversationContext);
        UpdateAiAssistantContextSummary(context, conversationContext, currentSubject);
    }

    private void UpdateAiAssistantContextSummary(
        Ra2AiContext context,
        Ra2AiConversationContext conversationContext,
        Ra2AiCurrentSubject currentSubject)
    {
        AiAssistantContextSummaryText.Text = FormatAiContextSummary(context);
        AiAssistantCurrentSubjectSummaryText.Text = FormatAiCurrentSubjectSummary(currentSubject);
        AiAssistantConversationContextSummaryText.Text = FormatAiConversationContextSummary(conversationContext);
    }

    private Ra2AiContext BuildCurrentAiContext(
        string? promptText = null,
        Ra2AiConversationContext? conversationContext = null,
        Ra2AiCurrentSubject? currentSubject = null)
    {
        if (DataContext is not ShellViewModel viewModel ||
            viewModel.CurrentSnapshot is null ||
            !viewModel.CurrentSnapshot.CanRunDiagnostics)
        {
            return _aiContextProvider.BuildContext(new Ra2AiContextRequest(
                documentDisplayName: null,
                semanticModel: null,
                caretOffset: 0,
                selectedText: GetExplicitAiSelectedText(),
                promptText: promptText,
                fieldDefinitionProvider: _fieldRegistryRuntimeService.CurrentProvider,
                fieldProvenanceProvider: _fieldRegistryRuntimeService.CurrentProvenanceProvider,
                diagnosticIssues: [],
                conversationContext: conversationContext,
                currentSubject: currentSubject));
        }

        Ra2DocumentSnapshot snapshot = new(
            viewModel.CurrentSnapshot.FileName,
            SourceTextEditor.Document.Text,
            viewModel.CurrentSnapshot.Version);
        Ra2DocumentSemanticModel model = _semanticModelBuilder.Build(
            snapshot,
            _fieldRegistryRuntimeService.CurrentProvider);
        int caretOffset = Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        return _aiContextProvider.BuildContext(new Ra2AiContextRequest(
            viewModel.CurrentSnapshot.FileName,
            model,
            caretOffset,
            GetExplicitAiSelectedText(),
            promptText: promptText,
            fieldDefinitionProvider: _fieldRegistryRuntimeService.CurrentProvider,
            fieldProvenanceProvider: _fieldRegistryRuntimeService.CurrentProvenanceProvider,
            diagnosticIssues: viewModel.Issues.Items.ToArray(),
            documentFilePath: viewModel.CurrentSnapshot.FilePath,
            documentVersion: viewModel.CurrentSnapshot.Version,
            conversationContext: conversationContext,
            currentSubject: currentSubject));
    }

    private Ra2AuthoringSnapshotCaptureResult CaptureCurrentAuthoringSnapshot()
        => Ra2AuthoringSnapshot.Capture(
            _editableSession,
            SourceTextEditor.Document.Text,
            (DataContext as ShellViewModel)?.CurrentProjectRootPath,
            _fieldRegistryRuntimeService.CaptureProviderSnapshot());

    private string? GetExplicitAiSelectedText()
    {
        string selectedText = SourceTextEditor.SelectedText;
        return string.IsNullOrWhiteSpace(selectedText) ? null : selectedText;
    }

    private static string FormatAiContextSummary(Ra2AiContext context)
    {
        string fieldEvidence = FormatFieldEvidenceSummary(context);
        string diagnostics = FormatDiagnosticsSummary(context);
        if (!context.HasSemanticContext)
            return $"上下文：当前没有可用的编辑器上下文。{fieldEvidence}；{diagnostics}。";

        string document = string.IsNullOrWhiteSpace(context.DocumentDisplayName)
            ? "未命名文件"
            : context.DocumentDisplayName;
        string section = string.IsNullOrWhiteSpace(context.SectionName)
            ? "无 Section"
            : $"[{context.SectionName}]";
        string keyValue = string.IsNullOrWhiteSpace(context.KeyName)
            ? "无字段"
            : string.IsNullOrWhiteSpace(context.ValueText)
                ? context.KeyName
                : $"{context.KeyName}={context.ValueText}";
        string selected = context.HasExplicitSelection ? "；包含选中文本" : string.Empty;
        return $"上下文：当前文件 {document}；Section {section}；字段 {keyValue}；光标第 {context.LineNumber} 行；附近行 {context.NearbyLineCount}；{fieldEvidence}；{diagnostics}{selected}。";
    }

    private static string FormatFieldEvidenceSummary(Ra2AiContext context)
    {
        if (context.FieldEvidenceCount <= 0)
            return "字段依据 0";

        string topKeys = context.FieldEvidenceTopKeysText;
        return string.IsNullOrWhiteSpace(topKeys)
            ? $"字段依据 {context.FieldEvidenceCount}"
            : $"字段依据 {context.FieldEvidenceCount}（{topKeys}）";
    }

    private static string FormatDiagnosticsSummary(Ra2AiContext context)
        => $"诊断 {context.DiagnosticCount}";

    private static string FormatAiCurrentSubjectSummary(Ra2AiCurrentSubject subject)
    {
        if (subject.Kind == Ra2AiSubjectKind.Unknown || string.IsNullOrWhiteSpace(subject.SubjectId))
            return "当前主题：无";

        string kind = subject.Kind.ToString();
        string source = subject.Source switch
        {
            Ra2AiSubjectSource.LastAssistantDraft => "上一轮 AI 草稿，仅作草稿/建议，未写入项目文件",
            Ra2AiSubjectSource.UserMention => "用户提及，仅作对话草稿/建议，未确认写入项目文件",
            Ra2AiSubjectSource.CurrentCaretSection => "当前光标 Section",
            _ => "来源未确定"
        };
        return $"当前主题：{subject.SubjectId} / {kind}（{source}）";
    }

    private static string FormatAiConversationContextSummary(Ra2AiConversationContext conversationContext)
    {
        string truncated = conversationContext.WasTruncated ? "已截断" : "未截断";
        return $"对话上下文：最近 {conversationContext.Turns.Count} 轮，{truncated}";
    }

    private async void ProjectExplorerTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (_isRestoringProjectExplorerSelection)
            return;

        if (DataContext is not ShellViewModel viewModel)
            return;

        if (e.NewValue is not ProjectExplorerItemViewModel selectedItem)
            return;

        if (selectedItem.Kind == ProjectExplorerItemKind.File)
        {
            if (!TryResolveDirtyNavigationBeforeLeavingCurrentFile(viewModel))
            {
                RestoreProjectExplorerSelectionToCurrentFile(viewModel);
                return;
            }

            CloseSourceEditorHoverToolTip();
            CloseCompletionDropdown();
            ResetEditableSessionToReadOnly();
            await viewModel.LoadProjectExplorerFileAsync(
                selectedItem,
                _fieldRegistryRuntimeService.CurrentProvider);
            StartEditableSessionForCurrentSnapshot(viewModel);
            return;
        }

        viewModel.ProjectExplorer.SelectedItem = selectedItem;
        if (selectedItem.Kind == ProjectExplorerItemKind.Section)
        {
            await TryNavigateToProjectExplorerSectionAsync(viewModel, selectedItem);
            return;
        }

        viewModel.ShowOutputMessage("Explorer navigation skipped: selected node is not a section.");
    }

    private void RestoreProjectExplorerSelectionToCurrentFile(ShellViewModel viewModel)
    {
        if (viewModel.CurrentSnapshot is null)
            return;

        ProjectExplorerItemViewModel? currentFileItem = FindProjectExplorerFileItem(viewModel, viewModel.CurrentSnapshot.FilePath);
        if (currentFileItem is null)
            return;

        viewModel.ProjectExplorer.SelectedItem = currentFileItem;
        viewModel.ProjectExplorer.MarkCurrentFile(currentFileItem.FilePath!);
        SelectProjectExplorerItem(currentFileItem);
    }

    private void SelectProjectExplorerItem(ProjectExplorerItemViewModel item)
    {
        SetRightToolWellAiViewVisible(false);
        _isRestoringProjectExplorerSelection = true;
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                ProjectExplorerTreeView.UpdateLayout();
                if (TryGetProjectExplorerTreeViewItem(item) is TreeViewItem container)
                {
                    container.IsSelected = true;
                    container.BringIntoView();
                    container.Focus();
                }
            }
            finally
            {
                _isRestoringProjectExplorerSelection = false;
            }
        }, DispatcherPriority.Background);
    }

    private TreeViewItem? TryGetProjectExplorerTreeViewItem(ProjectExplorerItemViewModel item)
    {
        ProjectExplorerTreeView.UpdateLayout();
        if (ProjectExplorerTreeView.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem rootContainer)
            return rootContainer;

        foreach (object rootItem in ProjectExplorerTreeView.Items)
        {
            if (ProjectExplorerTreeView.ItemContainerGenerator.ContainerFromItem(rootItem) is not TreeViewItem fileContainer)
                continue;

            TreeViewItem? matchingContainer = TryGetProjectExplorerTreeViewItem(fileContainer, item);
            if (matchingContainer is not null)
                return matchingContainer;
        }

        return null;
    }

    private static TreeViewItem? TryGetProjectExplorerTreeViewItem(ItemsControl parent, ProjectExplorerItemViewModel item)
    {
        parent.UpdateLayout();
        if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem directContainer)
            return directContainer;

        for (int index = 0; index < parent.Items.Count; index++)
        {
            if (parent.ItemContainerGenerator.ContainerFromIndex(index) is not TreeViewItem childContainer)
                continue;

            TreeViewItem? matchingContainer = TryGetProjectExplorerTreeViewItem(childContainer, item);
            if (matchingContainer is not null)
                return matchingContainer;
        }

        return null;
    }

    private async Task TryNavigateToProjectExplorerSectionAsync(ShellViewModel viewModel, ProjectExplorerItemViewModel section)
    {
        if (section.Kind != ProjectExplorerItemKind.Section)
        {
            viewModel.ShowOutputMessage("Explorer navigation skipped: selected node is not a section.");
            return;
        }

        if (viewModel.CurrentSnapshot is not null &&
            string.Equals(section.FilePath, viewModel.CurrentSnapshot.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            TryNavigateToSection(viewModel, section);
            return;
        }

        if (!TryResolveDirtyNavigationBeforeLeavingCurrentFile(viewModel))
        {
            RestoreProjectExplorerSelectionToCurrentFile(viewModel);
            return;
        }

        ProjectExplorerItemViewModel? fileItem = FindProjectExplorerFileItem(viewModel, section.FilePath);
        if (fileItem is null)
        {
            viewModel.ShowOutputMessage("Explorer navigation skipped: section file was not found.");
            return;
        }

        CloseSourceEditorHoverToolTip();
        CloseCompletionDropdown();
        ResetEditableSessionToReadOnly();
        await viewModel.LoadProjectExplorerFileAsync(
            fileItem,
            _fieldRegistryRuntimeService.CurrentProvider);
        StartEditableSessionForCurrentSnapshot(viewModel);
        TryNavigateToSection(viewModel, section);
    }

    private static ProjectExplorerItemViewModel? FindProjectExplorerFileItem(ShellViewModel viewModel, string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        return viewModel.ProjectExplorer.Items.FirstOrDefault(item =>
            item.Kind == ProjectExplorerItemKind.File &&
            string.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    }

    private static ProjectExplorerItemViewModel? FindProjectExplorerSectionItem(ShellViewModel viewModel, string? filePath, string? sectionId)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(sectionId))
            return null;

        foreach (ProjectExplorerItemViewModel fileItem in viewModel.ProjectExplorer.Items)
        {
            foreach (ProjectExplorerItemViewModel descendant in EnumerateProjectExplorerDescendants(fileItem))
            {
                if (descendant.Kind != ProjectExplorerItemKind.Section)
                    continue;

                if (string.Equals(descendant.FilePath, filePath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(descendant.SectionId, sectionId, StringComparison.OrdinalIgnoreCase))
                    return descendant;
            }
        }

        return null;
    }

    private static IEnumerable<ProjectExplorerItemViewModel> EnumerateProjectExplorerDescendants(ProjectExplorerItemViewModel item)
    {
        foreach (ProjectExplorerItemViewModel child in item.Children)
        {
            yield return child;
            foreach (ProjectExplorerItemViewModel descendant in EnumerateProjectExplorerDescendants(child))
                yield return descendant;
        }
    }

    private async void IssuesToolWindow_OnIssueNavigateRequested(object? sender, IdeDiagnosticIssueViewModel? issue)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        await TryNavigateToIssueAsync(viewModel, issue);
    }

    private void GoToDefinition_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryBuildLanguageNavigationRequest(
                out ShellViewModel? viewModel,
                out Ra2LanguageNavigationRequest? request))
            return;

        Ra2GoToDefinitionResult result = _languageNavigationController.GoToDefinition(request);
        if (!result.Success || result.Target is null)
        {
            viewModel.ShowOutputMessage(result.Message);
            return;
        }

        if (result.Action == Ra2GoToDefinitionAction.JumpToDefinition && result.TargetOffset is int targetOffset)
        {
            TryScrollSourceEditorToLanguageTarget(
                viewModel,
                targetOffset,
                result.Message,
                result.SectionName);
            return;
        }

        ShowPeekDefinitionWindow(result.Target);
        viewModel.ShowOutputMessage(result.Message);
    }

    private void SourceTextEditor_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _sourceEditorContextMenuOffset = TryGetDocumentOffsetFromMouse(e, out int offset)
            ? offset
            : null;
    }

    private void SourceEditorContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        int offset = _sourceEditorContextMenuOffset ??
                     Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        PeekFieldDetailsMenuItem.Header = "\u901f\u89c8\u5b57\u6bb5\u8be6\u60c5";
        PeekFieldDetailsMenuItem.IsEnabled = false;
        FindReferencesMenuItem.Header = "\u67e5\u627e\u5f53\u524d\u6587\u4ef6\u5f15\u7528";
        FindReferencesMenuItem.IsEnabled = false;
        if (!TryBuildLanguageContextAtOffset(
                offset,
                out _,
                out Ra2DocumentSemanticModel? model,
                out Ra2CaretContext? context))
        {
            return;
        }

        Ra2TextSpan? selectionSpan = GetContextMenuSelectionSpan(offset);
        FindReferencesMenuItem.IsEnabled = CanFindCurrentFileReferences(model, context, selectionSpan);
        Ra2ReferenceValueDetailResult referenceResult = _referenceValueDetailService.Resolve(
            new Ra2ReferenceValueDetailRequest(
                model,
                offset,
                selectionSpan));
        if (referenceResult.Success)
        {
            PeekFieldDetailsMenuItem.Header = "\u67e5\u770b\u5f15\u7528\u76ee\u6807\u8be6\u60c5";
            PeekFieldDetailsMenuItem.IsEnabled = true;
            return;
        }

        PeekFieldDetailsMenuItem.IsEnabled = _fieldQuickPeekService.CanResolveKeyValueLine(model, offset);
    }

    private void PeekFieldDetails_OnClick(object sender, RoutedEventArgs e)
    {
        int offset = _sourceEditorContextMenuOffset ??
                     Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        if (!TryBuildLanguageContextAtOffset(
                offset,
                out ShellViewModel? viewModel,
                out Ra2DocumentSemanticModel? model,
                out _))
        {
            viewModel?.ShowOutputMessage("无法速览字段详情：当前没有可用的 INI 文档。");
            return;
        }

        Ra2ReferenceValueDetailResult referenceResult = _referenceValueDetailService.Resolve(
            new Ra2ReferenceValueDetailRequest(
                model,
                offset,
                GetContextMenuSelectionSpan(offset)));
        if (referenceResult.Success && referenceResult.Target is not null)
        {
            ShowPeekDefinitionWindow(referenceResult.Target);
            viewModel.ShowOutputMessage(referenceResult.Status == Ra2ReferenceValueDetailStatus.MissingTarget
                ? "\u5f15\u7528\u76ee\u6807\u672a\u5728\u5f53\u524d\u6587\u4ef6\u4e2d\u627e\u5230\u3002"
                : $"\u5df2\u6253\u5f00\u5f15\u7528\u76ee\u6807 {referenceResult.Reference?.TargetSectionName} \u7684\u8be6\u60c5\u3002");
            return;
        }

        Ra2FieldQuickPeekResult result = _fieldQuickPeekService.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            _fieldRegistryRuntimeService.CurrentProvider,
            _fieldRegistryRuntimeService.CurrentProvenanceProvider));
        if (result.Status == Ra2FieldQuickPeekStatus.NotKeyValueLine)
        {
            viewModel.ShowOutputMessage("无法速览字段详情：当前位置不是 key-value 行。");
            return;
        }

        ShowFieldQuickPeekWindow(result.Details);
    }

    private Ra2TextSpan? GetContextMenuSelectionSpan(int offset)
    {
        if (SourceTextEditor.TextArea.Selection.IsEmpty)
            return null;

        ISegment? matchingSegment = null;
        foreach (ISegment segment in SourceTextEditor.TextArea.Selection.Segments)
        {
            if (offset < segment.Offset || offset > segment.EndOffset)
                continue;

            if (matchingSegment is not null)
                return null;

            matchingSegment = segment;
        }

        return matchingSegment is null || matchingSegment.Length <= 0
            ? null
            : new Ra2TextSpan(matchingSegment.Offset, matchingSegment.Length);
    }

    private void PeekDefinition_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryBuildLanguageNavigationRequest(
                out ShellViewModel? viewModel,
                out Ra2LanguageNavigationRequest? request))
            return;

        Ra2PeekDefinitionResult result = _languageNavigationController.PeekDefinition(request);
        if (!result.Success || result.Target is null)
        {
            viewModel.ShowOutputMessage(result.Message);
            return;
        }

        ShowPeekDefinitionWindow(result.Target);
        viewModel.ShowOutputMessage(result.Message);
    }

    private void FindAllReferences_OnClick(object sender, RoutedEventArgs e)
    {
        bool useContextMenuPosition = ReferenceEquals(sender, FindReferencesMenuItem);
        if (!TryBuildFindReferencesNavigationRequest(
                useContextMenuPosition,
                out ShellViewModel? viewModel,
                out Ra2LanguageNavigationRequest? request))
            return;

        Ra2FindReferencesNavigationResult result = _languageNavigationController.FindReferences(request);
        if (!result.Success || result.References is null)
        {
            viewModel.ShowOutputMessage(result.Message);
            return;
        }

        ShowFindReferencesWindow(result.References);
        viewModel.ShowOutputMessage(result.Message);
    }

    private bool CanFindCurrentFileReferences(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        Ra2TextSpan? selectionSpan)
    {
        Ra2ReferenceResult result = _languageNavigationController.FindReferences(
            new Ra2LanguageNavigationRequest(
                model,
                context,
                _fieldRegistryRuntimeService.CurrentProvider,
                _fieldRegistryRuntimeService.CurrentProvenanceProvider,
                selectionSpan)).References!;
        return !string.IsNullOrWhiteSpace(result.TargetName);
    }

    private void ShowCompletionPreview_OnClick(object sender, RoutedEventArgs e)
        => ShowCompletionDropdownAtCaret();

    private void AddProperty_OnClick(object sender, RoutedEventArgs e)
    {
        CloseSourceEditorHoverToolTip();
        if (!TryBuildLanguageContext(out ShellViewModel? viewModel, out _, out Ra2CaretContext? context))
            return;

        Ra2EditorDocumentState editorState = _editableSession?.DocumentState.State ?? Ra2EditorDocumentState.ReadOnlyPreview;
        int caretOffset = Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        Ra2IniTextDocument currentTextDocument = _editableSession?.TextDocument ??
                                                _addPropertyTextDocumentParser.Parse(SourceTextEditor.Document.Text);
        Ra2AddPropertyOpenResult openResult = _fieldBrowserController.CreateAddPropertyViewModel(new Ra2AddPropertyOpenRequest(
            _fieldRegistryRuntimeService.CurrentProvider,
            _fieldAnnotationStore,
            GetProjectFieldAnnotationPath(viewModel.CurrentProjectRootPath),
            context.Section?.Kind,
            editorState,
            _recentFieldUsageTracker,
            currentTextDocument,
            caretOffset));
        Ra2AddPropertyWindow window = new(openResult.ViewModel)
        {
            Owner = this
        };
        window.EditAnnotationRequested += (_, _) =>
            OpenFieldAnnotationEditor(window, viewModel.CurrentProjectRootPath);

        if (window.ShowDialog() != true)
            return;

        Ra2AddPropertyConfirmationResult confirmation = _fieldBrowserController.ConfirmAddProperty(
            new Ra2AddPropertyConfirmationRequest(window.ViewModel, _editableSession is not null));

        if (confirmation.Action == Ra2AddPropertyConfirmationAction.Cancelled)
        {
            viewModel.ShowOutputMessage("Add property cancelled.");
            return;
        }

        if (confirmation.Action == Ra2AddPropertyConfirmationAction.JumpExisting &&
            confirmation.Match is { } jumpMatch)
        {
            RestoreSourceEditorFocusAtCaret(jumpMatch.LineSpan.Start);
            viewModel.ShowOutputMessage($"Jumped to existing field '{jumpMatch.Key}' at line {jumpMatch.LineNumber}.");
            return;
        }

        if (confirmation.Action == Ra2AddPropertyConfirmationAction.RequiresEditMode)
        {
            viewModel.ShowOutputMessage("Add property skipped: no editable file is currently open.");
            return;
        }

        if (confirmation.Action == Ra2AddPropertyConfirmationAction.ReplaceExisting &&
            confirmation.Match is { } replaceMatch)
        {
            ApplyAddPropertyReplaceExisting(viewModel, window.ViewModel, replaceMatch);
            return;
        }

        ApplyAddPropertyInsertDuplicate(viewModel, window.ViewModel);
    }

    private void OpenFieldAnnotationEditor(Ra2AddPropertyWindow owner, string? projectRootPath)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        Ra2AddPropertyItemViewModel? selectedItem = owner.ViewModel.SelectedItem;
        if (selectedItem is null)
        {
            viewModel.ShowOutputMessage("Select a field before editing annotations.");
            return;
        }

        Ra2FieldAnnotationRefreshResult annotationRefresh = RefreshFieldAnnotations(projectRootPath);
        Ra2FieldAnnotationEditorViewModel editorViewModel = new(
            selectedItem.SectionKind,
            selectedItem.DisplayInfo,
            annotationRefresh.Pack,
            annotationRefresh.AnnotationPath,
            _fieldAnnotationStore,
            _fieldAnnotationEditingService);
        Ra2FieldAnnotationEditorWindow editorWindow = new(editorViewModel)
        {
            Owner = owner
        };
        editorWindow.AnnotationSaved += (_, _) =>
        {
            Ra2FieldAnnotationRefreshResult refreshed = RefreshFieldAnnotations(projectRootPath);
            owner.ViewModel.RefreshDisplay(
                refreshed.DisplayResolver,
                refreshed.Status);
            viewModel.ShowOutputMessage("Field annotation library saved and refreshed.");
        };
        editorWindow.ShowDialog();
    }

    private void SourceTextEditor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        => HandleSourceEditorPreviewKeyDown(e);

    private void SourceTextEditorTextArea_OnPreviewKeyDown(object sender, KeyEventArgs e)
        => HandleSourceEditorPreviewKeyDown(e);

    private void HandleSourceEditorPreviewKeyDown(KeyEventArgs e)
    {
        if (IsSaveShortcut(e))
        {
            StopCompletionAutoTrigger();
            SaveCurrentFileFromShell();
            e.Handled = true;
            return;
        }

        if (IsUndoShortcut(e))
        {
            UndoCurrentFileFromShell();
            e.Handled = true;
            return;
        }

        if (IsRedoShortcut(e))
        {
            RedoCurrentFileFromShell();
            e.Handled = true;
            return;
        }

        HandleCompletionPreviewKeyDown(e);
    }

    private static bool IsSaveShortcut(KeyEventArgs e)
    {
        Key key = GetActualKey(e);
        return key == Key.S && Keyboard.Modifiers == ModifierKeys.Control;
    }

    private static bool IsUndoShortcut(KeyEventArgs e)
    {
        Key key = GetActualKey(e);
        return key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control;
    }

    private static bool IsRedoShortcut(KeyEventArgs e)
    {
        Key key = GetActualKey(e);
        return key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control;
    }

    private static Key GetActualKey(KeyEventArgs e)
    {
        return e.Key switch
        {
            Key.System => e.SystemKey,
            Key.ImeProcessed => e.ImeProcessedKey,
            Key.DeadCharProcessed => e.DeadCharProcessedKey,
            _ => e.Key
        };
    }

    private void HandleCompletionPreviewKeyDown(KeyEventArgs e)
    {
        Key key = GetActualKey(e);
        if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.Space)
        {
            ShowCompletionDropdownAtCaret();
            e.Handled = true;
            return;
        }

        if (!CompletionDropdownPopup.IsOpen)
            return;

        if (key == Key.Escape)
        {
            CloseCompletionDropdown();
            e.Handled = true;
            return;
        }

        if (key == Key.Down)
        {
            _completionDropdownViewModel.MoveSelection(1);
            e.Handled = true;
            return;
        }

        if (key == Key.Up)
        {
            _completionDropdownViewModel.MoveSelection(-1);
            e.Handled = true;
            return;
        }

        if (key is Key.Enter or Key.Tab)
        {
            TryCommitSelectedCompletionOrClose();
            e.Handled = true;
        }
    }

    private void SourceTextEditor_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (IsFocusMovingInsideCompletionDropdown(e.NewFocus))
            return;

        StopCompletionAutoTrigger();
        CloseSourceEditorHoverToolTip();
        CloseCompletionDropdown();
    }

    private void SourceTextEditor_OnMouseMove(object sender, MouseEventArgs e)
    {
        int? offset = TryGetDocumentOffsetFromMouse(e, out int documentOffset)
            ? documentOffset
            : null;
        Ra2SourceEditorHoverPointerMoveResult result = _sourceEditorHoverController.OnPointerMoved(
            CompletionDropdownPopup.IsOpen,
            offset,
            _sourceEditorHoverTimer.IsEnabled);
        if (result.Action == Ra2SourceEditorHoverPointerMoveAction.Ignore)
            return;

        if (result.Action == Ra2SourceEditorHoverPointerMoveAction.Close)
        {
            CloseSourceEditorHoverToolTip();
            return;
        }

        _sourceEditorHoverTimer.Stop();
        CloseSourceEditorHoverPopupOnly();
        _sourceEditorHoverTimer.Start();
    }

    private void SourceEditorHoverTimer_OnTick(object? sender, EventArgs e)
    {
        _sourceEditorHoverTimer.Stop();
        if (_sourceEditorHoverController.ConsumePendingOffset() is not int offset)
            return;

        TryShowSourceEditorHoverAtOffset(offset);
    }

    private void TryShowSourceEditorHoverAtOffset(int offset)
    {
        if (!TryBuildLanguageContextAtOffset(offset, out _, out Ra2DocumentSemanticModel? model, out Ra2CaretContext? context))
        {
            CloseSourceEditorHoverToolTip();
            return;
        }

        Ra2SourceEditorHoverResolveResult result = _sourceEditorHoverController.ResolveHover(new Ra2SourceEditorHoverRequest(
            model,
            context,
            offset,
            CreateFieldDisplayResolver((DataContext as ShellViewModel)?.CurrentProjectRootPath),
            _fieldRegistryRuntimeService.CurrentProvenanceProvider));
        if (!result.Success || result.Display is null)
        {
            CloseSourceEditorHoverToolTip();
            return;
        }

        ShowSourceEditorHoverToolTip(result.Display);
        _sourceEditorHoverController.MarkHoverShown(offset);
    }

    private void SourceTextEditor_OnMouseLeave(object? sender, MouseEventArgs e)
        => CloseSourceEditorHoverToolTip();

    private void SourceTextEditor_OnTextChanged(object? sender, EventArgs e)
    {
        CloseSourceEditorHoverToolTip();
        UpdateShellStatusBar();
        if (_isSynchronizingEditorText || _editableSession is null)
        {
            StopCompletionAutoTrigger();
            return;
        }

        InvalidateActiveAiEditProposal(markSuperseded: false);
        CloseCompletionDropdown();
        Ra2EditorSessionOperationResult result = _editorSessionController.UpdateTextFromUser(
            new Ra2EditorSessionUpdateTextRequest(
                _editableSession,
                SourceTextEditor.Document.Text));
        if (!result.Success || result.Session is null)
        {
            StopCompletionAutoTrigger();
            return;
        }

        _editableSession = result.Session;
        InvalidateProgrammaticSemanticUndoIfTextChanged(result.Session.DocumentState.CurrentText);
        UpdateEditorStateControls();
        ScheduleCompletionAutoTrigger();
    }

    private void SaveCurrentFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SaveCurrentFileFromShell();
        e.Handled = true;
    }

    private void UndoRedoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = e.Command == ApplicationCommands.Undo
            ? CanUndoSourceEditor()
            : CanRedoSourceEditor();
        e.Handled = true;
    }

    private void UndoCurrentFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        UndoCurrentFileFromShell();
        e.Handled = true;
    }

    private void RedoCurrentFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        RedoCurrentFileFromShell();
        e.Handled = true;
    }

    private void SaveCurrentFile_OnClick(object sender, RoutedEventArgs e)
        => SaveCurrentFileFromShell();

    private void UndoCurrentFile_OnClick(object sender, RoutedEventArgs e)
        => UndoCurrentFileFromShell();

    private void RedoCurrentFile_OnClick(object sender, RoutedEventArgs e)
        => RedoCurrentFileFromShell();

    private void EnterEditMode_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            StartEditableSessionForCurrentSnapshot(viewModel);
    }

    private void UndoCurrentFileFromShell()
    {
        if (TryUndoProgrammaticSemanticChange())
            return;

        if (!CanUndoSourceEditor())
        {
            UpdateEditorStateControls();
            if (DataContext is ShellViewModel viewModel)
                viewModel.ShowOutputMessage("没有可撤销的编辑。");
            return;
        }

        CloseSourceEditorHoverToolTip();
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();
        int? topLineNumber = CaptureSourceEditorTopLineNumber();
        SourceTextEditor.Undo();
        RestoreSourceEditorTopLineIfDrifted(topLineNumber);
        StopCompletionAutoTrigger();
        UpdateEditorStateControls();
        if (DataContext is ShellViewModel currentViewModel)
            currentViewModel.ShowOutputMessage("已撤销上一步编辑。");
    }

    private void RedoCurrentFileFromShell()
    {
        if (TryRedoProgrammaticSemanticChange())
            return;

        if (!CanRedoSourceEditor())
        {
            UpdateEditorStateControls();
            if (DataContext is ShellViewModel viewModel)
                viewModel.ShowOutputMessage("没有可重做的编辑。");
            return;
        }

        CloseSourceEditorHoverToolTip();
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();
        int? topLineNumber = CaptureSourceEditorTopLineNumber();
        SourceTextEditor.Redo();
        RestoreSourceEditorTopLineIfDrifted(topLineNumber);
        StopCompletionAutoTrigger();
        UpdateEditorStateControls();
        if (DataContext is ShellViewModel currentViewModel)
            currentViewModel.ShowOutputMessage("已重做上一步编辑。");
    }

    private bool CanUndoSourceEditor()
        => _editableSession is not null &&
           !SourceTextEditor.IsReadOnly &&
           (SourceTextEditor.CanUndo || CanUndoProgrammaticSemanticChange());

    private bool CanRedoSourceEditor()
        => _editableSession is not null &&
           !SourceTextEditor.IsReadOnly &&
           (SourceTextEditor.CanRedo || CanRedoProgrammaticSemanticChange());

    private bool CanUndoProgrammaticSemanticChange()
        => _programmaticSemanticUndoState is { IsUndone: false } state &&
           string.Equals(SourceTextEditor.Document.Text, state.RedoText, StringComparison.Ordinal);

    private bool CanRedoProgrammaticSemanticChange()
        => _programmaticSemanticUndoState is { IsUndone: true } state &&
           string.Equals(SourceTextEditor.Document.Text, state.UndoText, StringComparison.Ordinal);

    private bool TryUndoProgrammaticSemanticChange()
    {
        if (!CanUndoProgrammaticSemanticChange() || _programmaticSemanticUndoState is null)
            return false;

        ProgrammaticSemanticUndoState state = _programmaticSemanticUndoState;
        ApplyProgrammaticSemanticText(state.UndoText, state.UndoCaretOffset);
        _programmaticSemanticUndoState = state with { IsUndone = true };
        UpdateEditorStateControls();
        CommandManager.InvalidateRequerySuggested();
        if (DataContext is ShellViewModel viewModel)
            viewModel.ShowOutputMessage(state.UndoMessage);

        return true;
    }

    private bool TryRedoProgrammaticSemanticChange()
    {
        if (!CanRedoProgrammaticSemanticChange() || _programmaticSemanticUndoState is null)
            return false;

        ProgrammaticSemanticUndoState state = _programmaticSemanticUndoState;
        ApplyProgrammaticSemanticText(state.RedoText, state.RedoCaretOffset);
        _programmaticSemanticUndoState = state with { IsUndone = false };
        UpdateEditorStateControls();
        CommandManager.InvalidateRequerySuggested();
        if (DataContext is ShellViewModel viewModel)
            viewModel.ShowOutputMessage(state.RedoMessage);

        return true;
    }

    private void ApplyProgrammaticSemanticText(string text, int caretOffset)
    {
        CloseSourceEditorHoverToolTip();
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();
        SetEditorTextFromProgram(text, caretOffset);
        ClearAvalonEditUndoStackOnly();
        SyncEditableSessionFromProgrammaticText(text);
        RestoreSourceEditorFocusAtCaret(caretOffset);
    }

    private void SyncEditableSessionFromProgrammaticText(string text)
    {
        if (_editableSession is null)
            return;

        Ra2EditorSessionOperationResult result = _editorSessionController.UpdateTextFromUser(
            new Ra2EditorSessionUpdateTextRequest(_editableSession, text));
        if (result.Success && result.Session is not null)
            _editableSession = result.Session;
    }

    private Ra2IniEditApplyResult ApplyAuthoringPreviewTransaction(Ra2IniEditPreview preview)
    {
        Ra2EditableDocumentSession? sessionBeforeApply = _editableSession;
        string editorTextBeforeApply = SourceTextEditor.Document.Text;
        int caretBeforeApply = Math.Clamp(
            SourceTextEditor.TextArea.Caret.Offset,
            0,
            editorTextBeforeApply.Length);
        ProgrammaticSemanticUndoState? semanticUndoBeforeApply = _programmaticSemanticUndoState;
        long fieldRegistryRevision = _fieldRegistryRuntimeService.CaptureProviderSnapshot().Revision;
        Ra2IniEditPreviewCurrencyResult currency = _authoringPreviewCurrencyEvaluator.Evaluate(
            preview,
            sessionBeforeApply,
            editorTextBeforeApply,
            fieldRegistryRevision);
        if (!currency.IsCurrent)
            return Ra2IniEditApplyResult.Stale(preview.PreviewId, currency);

        if (sessionBeforeApply is null || preview.CandidateText is null)
        {
            return Ra2IniEditApplyResult.TransactionRejected(
                preview.PreviewId,
                "The current editor transaction has no applicable session or candidate text.");
        }

        int redoCaretOffset = Math.Clamp(caretBeforeApply, 0, preview.CandidateText.Length);
        ProgrammaticSemanticUndoState? semanticUndoAfterApply = CreateProgrammaticSemanticUndoState(
            editorTextBeforeApply,
            preview.CandidateText,
            caretBeforeApply,
            redoCaretOffset,
            $"已撤销 {preview.OperationPreviews.Count} 项结构化编辑。",
            $"已重做 {preview.OperationPreviews.Count} 项结构化编辑。");
        if (semanticUndoAfterApply is null)
        {
            return Ra2IniEditApplyResult.TransactionRejected(
                preview.PreviewId,
                "The structured edit did not produce an effective text change.");
        }

        Ra2EditorSessionOperationResult sessionResult = _editorSessionController.ApplyProgrammaticText(
            new Ra2EditorSessionApplyProgrammaticTextRequest(
                sessionBeforeApply,
                preview.Snapshot.DocumentId,
                preview.Snapshot.EditRevision,
                preview.Snapshot.Text,
                preview.CandidateText,
                redoCaretOffset));
        if (!sessionResult.Success ||
            sessionResult.Session is null ||
            sessionResult.TextToSyncToEditor is null)
        {
            return Ra2IniEditApplyResult.TransactionRejected(
                preview.PreviewId,
                sessionResult.Message);
        }

        Ra2IniEditApplyResult appliedResult = Ra2IniEditApplyResult.Applied(
            preview,
            sessionResult.Session,
            caretBeforeApply,
            sessionResult.CaretOffset ?? redoCaretOffset);
        try
        {
            SetEditorTextFromProgram(
                sessionResult.TextToSyncToEditor,
                sessionResult.CaretOffset ?? redoCaretOffset);
            _editableSession = sessionResult.Session;
            _programmaticSemanticUndoState = semanticUndoAfterApply;
            ClearAvalonEditUndoStackOnly();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and
                                          not StackOverflowException and
                                          not AccessViolationException)
        {
            bool editorRestored = TryRestoreEditorAfterAuthoringFailure(
                editorTextBeforeApply,
                caretBeforeApply);
            _editableSession = sessionBeforeApply;
            _programmaticSemanticUndoState = semanticUndoBeforeApply;
            if (!editorRestored)
                ResetEditableSessionToReadOnly();

            return Ra2IniEditApplyResult.UnexpectedFailure(preview.PreviewId);
        }

        try
        {
            UpdateEditorStateControls();
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and
                                          not StackOverflowException and
                                          not AccessViolationException)
        {
            // The semantic transaction is already committed. A later Shell refresh
            // must not convert that successful commit into a failed apply result.
        }

        return appliedResult;
    }

    private bool TryRestoreEditorAfterAuthoringFailure(string text, int caretOffset)
    {
        try
        {
            SetEditorTextFromProgram(text, caretOffset);
            ClearAvalonEditUndoStackOnly();
            return true;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and
                                          not StackOverflowException and
                                          not AccessViolationException)
        {
            return false;
        }
    }

    private void InvalidateProgrammaticSemanticUndoIfTextChanged(string currentText)
    {
        if (_programmaticSemanticUndoState is null)
            return;

        if (!string.Equals(currentText, _programmaticSemanticUndoState.RedoText, StringComparison.Ordinal) &&
            !string.Equals(currentText, _programmaticSemanticUndoState.UndoText, StringComparison.Ordinal))
            _programmaticSemanticUndoState = null;
    }

    private int? CaptureSourceEditorTopLineNumber()
    {
        SourceTextEditor.TextArea.TextView.EnsureVisualLines();
        return SourceTextEditor.TextArea.TextView.VisualLines.FirstOrDefault()?.FirstDocumentLine.LineNumber;
    }

    private void RestoreSourceEditorTopLineIfDrifted(int? topLineNumber)
    {
        if (topLineNumber is not > 0)
            return;

        Dispatcher.BeginInvoke(
            () =>
            {
                if (topLineNumber > SourceTextEditor.Document.LineCount)
                    return;

                SourceTextEditor.TextArea.TextView.EnsureVisualLines();
                int? currentTopLineNumber = CaptureSourceEditorTopLineNumber();
                if (currentTopLineNumber is null ||
                    Math.Abs(currentTopLineNumber.Value - topLineNumber.Value) > 2)
                {
                    SourceTextEditor.ScrollTo(topLineNumber.Value, 1);
                }
            },
            DispatcherPriority.ContextIdle);
    }

    private void SaveCurrentFileFromShell()
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        CloseSourceEditorHoverToolTip();
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();

        TrySaveCurrentFileWithPreflight(viewModel);
    }

    private bool TrySaveCurrentFileWithPreflight(ShellViewModel viewModel)
    {
        if (!TryRunSavePreflight(viewModel))
            return false;

        bool hasLoadedFile = viewModel.CurrentSnapshot is { CanRunDiagnostics: true };
        Ra2SaveCurrentFileResult result = _saveCurrentFileService.Save(
            new Ra2SaveCurrentFilePlanRequest(_editableSession, SourceTextEditor.IsReadOnly),
            viewModel.CurrentProjectRootPath,
            DateTime.Now);

        if (result.UpdatedSession is not null)
        {
            _editableSession = result.UpdatedSession;
            SourceTextEditor.IsReadOnly = false;
        }

        UpdateEditorStateControls();
        viewModel.ShowOutputMessage(_saveCurrentFileUiMessageFormatter.Format(result, hasLoadedFile));
        UpdateSaveOperationStatus(viewModel, result);
        return result.Success;
    }

    private bool TryRunSavePreflight(ShellViewModel viewModel)
    {
        if (_editableSession?.DocumentState.IsDirty != true)
            return true;

        Ra2SavePreflightResult result = _savePreflightDiagnosticService.Analyze(
            viewModel.CurrentSnapshot,
            SourceTextEditor.Document.Text,
            _fieldRegistryRuntimeService.CurrentProvider);

        if (!result.WasRun || !result.HasIssues)
            return true;

        viewModel.Issues.ReplaceIssues(result.Issues, result.SummaryText);
        viewModel.SetOperationStatus(result.SummaryText, result.ErrorCount > 0 ? "Error" : "Warning");

        if (_savePreflightConfirmationService.ConfirmContinue(this, result))
        {
            viewModel.SetOperationStatus("继续保存…", "Info");
            return true;
        }

        viewModel.ShowOutputMessage("已取消保存：保存前检查发现可能问题，当前修改仍保留。");
        viewModel.SetOperationStatus("已取消保存，修改仍保留", "Warning");
        return false;
    }

    private void RevertInMemoryChanges_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        Ra2EditorSessionOperationResult result = _editorSessionController.Revert(
            new Ra2EditorSessionRevertRequest(_editableSession));
        if (!result.Success || result.TextToSyncToEditor is null)
        {
            viewModel.ShowOutputMessage(result.Message ?? "There are no in-memory changes to revert.");
            viewModel.SetOperationStatus(
                $"恢复失败：{ShortenStatusReason(result.Message ?? "没有可恢复的内存修改。")}",
                "Error");
            return;
        }

        SetEditorTextFromProgram(result.TextToSyncToEditor);
        ClearSourceEditorUndoStack();
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();
        if (result.Session is not null)
            _editableSession = result.Session;

        if (result.ShouldSetEditable)
            SourceTextEditor.IsReadOnly = false;

        if (result.ShouldSetReadOnly)
            ResetEditableSessionToReadOnly();
        UpdateEditorStateControls();
        viewModel.ShowOutputMessage(result.Message ?? "Reverted in-memory changes.");
        viewModel.SetOperationStatus("已恢复到上次保存内容", "Success");
    }

    private void CompletionDropdownView_OnCompletionItemDoubleClicked(
        object? sender,
        Ra2CompletionDropdownItemViewModel item)
        => TryCommitCompletionItemOrClose(item);

    private void CompletionDropdownView_OnCompletionCommitRequested(object? sender, EventArgs e)
        => TryCommitSelectedCompletionOrClose();

    private void CompletionDropdownView_OnCompletionCloseRequested(object? sender, EventArgs e)
        => CloseCompletionDropdown();

    private void ApplyAddPropertyInsertDuplicate(ShellViewModel viewModel, Ra2AddPropertyViewModel addPropertyViewModel)
    {
        if (_editableSession is null)
            return;

        string textBeforeApply = SourceTextEditor.Document.Text;
        int caretOffset = Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        Ra2FieldBrowserActionResult result = _fieldBrowserController.ApplyInsertDuplicate(new Ra2AddPropertyApplyRequest(
            addPropertyViewModel,
            _editableSession,
            caretOffset));
        ApplyFieldBrowserActionResult(
            viewModel,
            addPropertyViewModel,
            result,
            textBeforeApply,
            caretOffset,
            "已撤销添加字段。",
            "已重做添加字段。");
    }

    private void ApplyAddPropertyReplaceExisting(
        ShellViewModel viewModel,
        Ra2AddPropertyViewModel addPropertyViewModel,
        Ra2DuplicateKeyMatch match)
    {
        if (_editableSession is null)
            return;

        string textBeforeApply = SourceTextEditor.Document.Text;
        int undoCaretOffset = Math.Clamp(match.ValueSpan.Start, 0, SourceTextEditor.Document.TextLength);
        Ra2FieldBrowserActionResult result = _fieldBrowserController.ApplyReplaceExisting(
            new Ra2AddPropertyReplaceApplyRequest(addPropertyViewModel, _editableSession, match));
        ApplyFieldBrowserActionResult(
            viewModel,
            addPropertyViewModel,
            result,
            textBeforeApply,
            undoCaretOffset,
            "已撤销替换字段。",
            "已重做替换字段。");
    }

    private void ApplyFieldBrowserActionResult(
        ShellViewModel viewModel,
        Ra2AddPropertyViewModel addPropertyViewModel,
        Ra2FieldBrowserActionResult result,
        string textBeforeApply,
        int undoCaretOffset,
        string undoMessage,
        string redoMessage)
    {
        if (!result.Success ||
            result.UpdatedSession is null ||
            result.UpdatedText is null ||
            result.CaretOffset is null)
        {
            viewModel.ShowOutputMessage(result.Message);
            return;
        }

        _editableSession = result.UpdatedSession;
        _programmaticSemanticUndoState = CreateProgrammaticSemanticUndoState(
            textBeforeApply,
            result.UpdatedText,
            undoCaretOffset,
            result.CaretOffset.Value,
            undoMessage,
            redoMessage);
        if (addPropertyViewModel.SelectedItem is { } selectedItem)
            _recentFieldUsageTracker.Record(selectedItem.SectionKind, addPropertyViewModel.OptionText);

        SetEditorTextFromProgram(result.UpdatedText, result.CaretOffset.Value);
        UpdateEditorStateControls();
        RestoreSourceEditorFocusAtCaret(result.CaretOffset.Value);
        viewModel.ShowOutputMessage(result.Message);
    }

    private void SourceTextEditorCaret_OnPositionChanged(object? sender, EventArgs e)
    {
        CloseSourceEditorHoverToolTip();
        CloseCompletionDropdown();
        UpdateShellCaretStatus();
    }

    private void SourceTextEditorSelection_OnChanged(object? sender, EventArgs e)
    {
        UpdateShellCaretStatus();
    }

    private void SourceTextEditorTextView_OnScrollOffsetChanged(object? sender, EventArgs e)
    {
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();
        CloseSourceEditorHoverToolTip();
    }

    private void ShowCompletionDropdownAtCaret()
    {
        CloseSourceEditorHoverToolTip();
        TryShowCompletionDropdownAtCaret(showOutputMessage: true);
    }

    private bool TryShowCompletionDropdownAtCaret(bool showOutputMessage)
    {
        StopCompletionAutoTrigger();
        CloseSourceEditorHoverToolTip();
        if (!TryBuildLanguageContext(out ShellViewModel? viewModel, out Ra2DocumentSemanticModel? model, out Ra2CaretContext? context))
            return false;

        int caretOffset = Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        Ra2CompletionOpenResult result = _completionInteractionController.OpenCompletions(new Ra2CompletionOpenRequest(
            model,
            context,
            caretOffset,
            _fieldRegistryRuntimeService.CurrentProvider,
            context.Section?.Kind ?? Ra2SectionKind.Unknown,
            CreateFieldDisplayResolver(viewModel.CurrentProjectRootPath)));
        if (!showOutputMessage && result.CompletionResult.Items.Count == 0)
            return false;

        ShowCompletionDropdown(result.CompletionResult);
        if (showOutputMessage)
            viewModel.ShowOutputMessage(result.Message);

        return true;
    }

    private void SourceEditorCompletionAutoTriggerTimer_OnTick(object? sender, EventArgs e)
    {
        StopCompletionAutoTrigger();
        if (!CanAutoTriggerCompletion())
            return;

        TryShowCompletionDropdownAtCaret(showOutputMessage: false);
    }

    private void ScheduleCompletionAutoTrigger()
    {
        StopCompletionAutoTrigger();
        if (!CanAutoTriggerCompletion())
            return;

        _sourceEditorCompletionAutoTriggerTimer.Start();
    }

    private void StopCompletionAutoTrigger()
        => _sourceEditorCompletionAutoTriggerTimer.Stop();

    private bool CanAutoTriggerCompletion()
        => _editableSession is not null &&
           !SourceTextEditor.IsReadOnly &&
           !CompletionDropdownPopup.IsOpen &&
           (SourceTextEditor.IsKeyboardFocusWithin || SourceTextEditor.TextArea.IsKeyboardFocusWithin);

    private void FieldRegistryManagerWindow_OnReloadLocalFieldRegistryRequested(object? sender, EventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);
    }

    private void FieldRegistryManagerWindow_OnHarvestPreviewRequested(object? sender, EventArgs e)
    {
        if (_fieldRegistryHarvestPreviewWindow is { IsVisible: true })
        {
            _fieldRegistryHarvestPreviewWindow.Activate();
            return;
        }

        _fieldRegistryHarvestPreviewWindow = new FieldRegistryHarvestPreviewWindow(
            () => _fieldRegistryRuntimeService.CurrentProvenanceProvider,
            () => DataContext is ShellViewModel viewModel ? viewModel.CurrentProjectRootPath : null,
            _fieldRegistryRuntimeService.GetGlobalRootDirectoryPath,
            () =>
            {
                if (DataContext is ShellViewModel viewModel)
                    ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);
            },
            GetCurrentIniSourceForFieldRegistryHarvest)
        {
            Owner = this
        };
        _fieldRegistryHarvestPreviewWindow.Closed += (_, _) => _fieldRegistryHarvestPreviewWindow = null;
        _fieldRegistryHarvestPreviewWindow.Show();
    }

    private void FieldRegistryManagerWindow_OnRelearnCurrentIniRequested(object? sender, EventArgs e)
        => OpenFieldLearningWizardWindow(GetCurrentIniSourceForFieldRegistryHarvest());

    private void FieldRegistryManagerWindow_OnCleanupApplied(object? sender, EventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);
        RefreshFieldRegistryRollbackManifests();
        viewModel.ShowOutputMessage("字段库清理已应用，并已重新加载本地字段库。");
    }

    private FieldRegistryCurrentIniSource? GetCurrentIniSourceForFieldRegistryHarvest()
    {
        if (DataContext is not ShellViewModel { CurrentSnapshot: not null } viewModel)
            return null;

        string sourceName = string.IsNullOrWhiteSpace(viewModel.CurrentSnapshot.FileName)
            ? "current.ini"
            : viewModel.CurrentSnapshot.FileName;
        return new FieldRegistryCurrentIniSource(sourceName, SourceTextEditor.Document.Text);
    }

    private bool TryGetCurrentSectionSourceForFieldRegistryHarvest(
        out FieldRegistryCurrentIniSource? source,
        out string message)
    {
        source = null;
        message = string.Empty;
        if (DataContext is not ShellViewModel { CurrentSnapshot: not null } viewModel)
        {
            message = "请先从项目浏览器中选择一个 INI 文件。";
            return false;
        }

        TextDocument document = SourceTextEditor.Document;
        if (document.TextLength == 0)
        {
            message = "当前文件没有可学习的文本。";
            return false;
        }

        int caretOffset = Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, document.TextLength);
        DocumentLine caretLine = caretOffset == document.TextLength && document.LineCount > 0
            ? document.GetLineByNumber(document.LineCount)
            : document.GetLineByOffset(caretOffset);

        DocumentLine? headerLine = FindSectionHeaderAtOrBefore(document, caretLine);
        if (headerLine is null ||
            !TryReadSectionId(document.GetText(headerLine).Trim(), out string sectionId))
        {
            message = "当前光标不在可学习的 Section 内。";
            return false;
        }

        int endOffset = document.TextLength;
        for (int lineNumber = headerLine.LineNumber + 1; lineNumber <= document.LineCount; lineNumber++)
        {
            DocumentLine candidate = document.GetLineByNumber(lineNumber);
            if (TryReadSectionId(document.GetText(candidate).Trim(), out _))
            {
                endOffset = candidate.Offset;
                break;
            }
        }

        string sectionText = document.GetText(headerLine.Offset, Math.Max(0, endOffset - headerLine.Offset));
        if (string.IsNullOrWhiteSpace(sectionText))
        {
            message = $"当前 Section [{sectionId}] 没有可学习的文本。";
            return false;
        }

        string sourceName = $"{viewModel.CurrentSnapshot.FileName} [{sectionId}]";
        source = new FieldRegistryCurrentIniSource(sourceName, sectionText);
        return true;
    }

    private static DocumentLine? FindSectionHeaderAtOrBefore(TextDocument document, DocumentLine startLine)
    {
        for (int lineNumber = startLine.LineNumber; lineNumber >= 1; lineNumber--)
        {
            DocumentLine line = document.GetLineByNumber(lineNumber);
            if (TryReadSectionId(document.GetText(line).Trim(), out _))
                return line;
        }

        return null;
    }

    private static bool TryReadSectionId(string text, out string sectionId)
    {
        sectionId = string.Empty;
        if (text.Length < 3 || text[0] != '[')
            return false;

        int closingIndex = text.IndexOf(']');
        if (closingIndex <= 1)
            return false;

        string trailingText = text[(closingIndex + 1)..].TrimStart();
        if (trailingText.Length > 0 && !trailingText.StartsWith(';'))
            return false;

        sectionId = text[1..closingIndex].Trim();
        return sectionId.Length > 0;
    }

    private void FieldRegistryManagerWindow_OnOpenGlobalRegistryFolderRequested(object? sender, EventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
            OpenRegistryFolder(_fieldRegistryRuntimeService.GetGlobalActiveDirectoryPath(), viewModel);
    }

    private void FieldRegistryManagerWindow_OnOpenProjectRegistryFolderRequested(object? sender, EventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        string? projectDirectory = _fieldRegistryRuntimeService.GetProjectActiveDirectoryPath(viewModel.CurrentProjectRootPath);
        if (projectDirectory is null)
        {
            viewModel.ShowOutputMessage("Open Project Registry Folder skipped: no project is open.");
            return;
        }

        OpenRegistryFolder(projectDirectory, viewModel);
    }

    private void FieldRegistryManagerWindow_OnRefreshRollbackManifestsRequested(object? sender, EventArgs e)
        => RefreshFieldRegistryRollbackManifests();

    private void FieldRegistryManagerWindow_OnOpenRollbackTargetFolderRequested(object? sender, string directoryPath)
        => OpenRollbackFolder("目标", directoryPath);

    private void FieldRegistryManagerWindow_OnOpenRollbackManifestFolderRequested(object? sender, string directoryPath)
        => OpenRollbackFolder("清单", directoryPath);

    private void FieldRegistryManagerWindow_OnOpenRollbackBackupFolderRequested(object? sender, string directoryPath)
        => OpenRollbackFolder("备份", directoryPath);

    private void FieldRegistryManagerWindow_OnRollbackCompleted(object? sender, FieldRegistryRollbackResult e)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);
        RefreshFieldRegistryRollbackManifests();
        _fieldRegistryManagerViewModel.ShowRollbackCompleted(e);
        viewModel.ShowOutputMessage($"回滚已完成：{e.Message}");
    }

    private void FieldRegistryManagerWindow_OnClosed(object? sender, EventArgs e)
    {
        if (_fieldRegistryManagerWindow is not null)
        {
            _fieldRegistryManagerWindow.ReloadLocalFieldRegistryRequested -= FieldRegistryManagerWindow_OnReloadLocalFieldRegistryRequested;
            _fieldRegistryManagerWindow.HarvestPreviewRequested -= FieldRegistryManagerWindow_OnHarvestPreviewRequested;
            _fieldRegistryManagerWindow.RelearnCurrentIniRequested -= FieldRegistryManagerWindow_OnRelearnCurrentIniRequested;
            _fieldRegistryManagerWindow.CleanupApplied -= FieldRegistryManagerWindow_OnCleanupApplied;
            _fieldRegistryManagerWindow.OpenGlobalRegistryFolderRequested -= FieldRegistryManagerWindow_OnOpenGlobalRegistryFolderRequested;
            _fieldRegistryManagerWindow.OpenProjectRegistryFolderRequested -= FieldRegistryManagerWindow_OnOpenProjectRegistryFolderRequested;
            _fieldRegistryManagerWindow.RefreshRollbackManifestsRequested -= FieldRegistryManagerWindow_OnRefreshRollbackManifestsRequested;
            _fieldRegistryManagerWindow.OpenRollbackTargetFolderRequested -= FieldRegistryManagerWindow_OnOpenRollbackTargetFolderRequested;
            _fieldRegistryManagerWindow.OpenRollbackManifestFolderRequested -= FieldRegistryManagerWindow_OnOpenRollbackManifestFolderRequested;
            _fieldRegistryManagerWindow.OpenRollbackBackupFolderRequested -= FieldRegistryManagerWindow_OnOpenRollbackBackupFolderRequested;
            _fieldRegistryManagerWindow.RollbackCompleted -= FieldRegistryManagerWindow_OnRollbackCompleted;
            _fieldRegistryManagerWindow.Closed -= FieldRegistryManagerWindow_OnClosed;
        }

        _fieldRegistryManagerWindow = null;
    }

    private void RefreshFieldRegistryRollbackManifests()
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        _fieldRegistryManagerViewModel.RefreshRollbackManifests(
            viewModel.CurrentProjectRootPath,
            _fieldRegistryRuntimeService.GetGlobalRootDirectoryPath());
    }

    private void ShellWindow_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        AttachSourceEditorTextBinding(e.NewValue as ShellViewModel);
    }

    private void AttachSourceEditorTextBinding(ShellViewModel? viewModel)
    {
        if (_boundSourceEditor is not null)
            _boundSourceEditor.PropertyChanged -= SourceEditor_OnPropertyChanged;

        _boundSourceEditor = viewModel?.SourceEditor;
        if (_boundSourceEditor is null)
        {
            SetReadonlySourceText(string.Empty);
            return;
        }

        _boundSourceEditor.PropertyChanged += SourceEditor_OnPropertyChanged;
        SetReadonlySourceText(_boundSourceEditor.Text);
    }

    private void SourceEditor_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SourceEditorViewModel.Text))
            return;

        if (sender is SourceEditorViewModel sourceEditor)
            SetReadonlySourceText(sourceEditor.Text);
    }

    private void SetReadonlySourceText(string text)
    {
        CloseSourceEditorHoverToolTip();
        StopCompletionAutoTrigger();
        ResetEditableSessionToReadOnly();
        if (SourceTextEditor.Document.Text == text)
            return;

        CloseCompletionDropdown();
        SetEditorTextFromProgram(text);
    }

    private void SetEditorTextFromProgram(string text, int? caretOffset = null)
    {
        CloseSourceEditorHoverToolTip();
        StopCompletionAutoTrigger();
        InvalidateActiveAiEditProposal(markSuperseded: false);
        _isSynchronizingEditorText = true;
        try
        {
            SourceTextEditor.Document.Text = text;
            if (caretOffset is int offset)
            {
                int normalizedOffset = Math.Clamp(offset, 0, SourceTextEditor.Document.TextLength);
                SourceTextEditor.TextArea.Caret.Offset = normalizedOffset;
                SourceTextEditor.ScrollTo(SourceTextEditor.Document.GetLocation(normalizedOffset).Line, 1);
            }
        }
        finally
        {
            _isSynchronizingEditorText = false;
        }
    }

    private void ClearSourceEditorUndoStack()
    {
        _programmaticSemanticUndoState = null;
        ClearAvalonEditUndoStackOnly();
        UpdateEditorStateControls();
    }

    private void ClearAvalonEditUndoStackOnly()
        => SourceTextEditor.Document.UndoStack.ClearAll();

    private void ResetEditableSessionToReadOnly()
    {
        StopCompletionAutoTrigger();
        InvalidateActiveAiEditProposal(markSuperseded: false);
        _programmaticSemanticUndoState = null;
        _editableSession = null;
        SourceTextEditor.IsReadOnly = true;
        UpdateEditorStateControls();
    }

    private void StartEditableSessionForCurrentSnapshot(ShellViewModel viewModel)
    {
        StopCompletionAutoTrigger();
        InvalidateActiveAiEditProposal(markSuperseded: false);
        if (viewModel.CurrentSnapshot is not { CanRunDiagnostics: true } snapshot)
        {
            ResetEditableSessionToReadOnly();
            return;
        }

        Ra2EditorSessionOperationResult result = _editorSessionController.EnterEditMode(
            new Ra2EditorSessionEnterRequest(
                snapshot.FilePath,
                SourceTextEditor.Document.Text,
                snapshot.EncodingMetadata));
        if (!result.Success || result.Session is null)
        {
            ResetEditableSessionToReadOnly();
            viewModel.ShowOutputMessage(result.Message ?? "Cannot open editable in-memory session.");
            return;
        }

        _editableSession = result.Session;
        SourceTextEditor.IsReadOnly = !result.ShouldSetEditable;
        UpdateEditorStateControls();
    }

    private bool TryResolveDirtyNavigationBeforeLeavingCurrentFile(ShellViewModel viewModel)
    {
        if (_editableSession?.DocumentState.IsDirty != true)
            return true;

        Ra2DirtyNavigationDecision decision = _dirtyNavigationDialogService.ShowDirtyNavigationDialog(
            this,
            _editableSession.DocumentState.FilePath);

        return decision switch
        {
            Ra2DirtyNavigationDecision.Save => TrySaveDirtyFileBeforeNavigation(viewModel),
            Ra2DirtyNavigationDecision.Discard => TryDiscardDirtyFileBeforeNavigation(viewModel),
            _ => CancelDirtyNavigation(viewModel)
        };
    }

    private bool TrySaveDirtyFileBeforeNavigation(ShellViewModel viewModel)
        => TrySaveCurrentFileWithPreflight(viewModel);

    private bool TryDiscardDirtyFileBeforeNavigation(ShellViewModel viewModel)
    {
        Ra2EditorSessionOperationResult result = _editorSessionController.Revert(
            new Ra2EditorSessionRevertRequest(_editableSession));
        if (!result.Success || result.TextToSyncToEditor is null)
        {
            viewModel.ShowOutputMessage(result.Message ?? "无法放弃当前内存修改。");
            viewModel.SetOperationStatus(
                $"恢复失败：{ShortenStatusReason(result.Message ?? "无法放弃当前内存修改。")}",
                "Error");
            return false;
        }

        SetEditorTextFromProgram(result.TextToSyncToEditor);
        ClearSourceEditorUndoStack();
        StopCompletionAutoTrigger();
        CloseCompletionDropdown();
        if (result.Session is not null)
            _editableSession = result.Session;

        if (result.ShouldSetEditable)
            SourceTextEditor.IsReadOnly = false;

        if (result.ShouldSetReadOnly)
            ResetEditableSessionToReadOnly();

        UpdateEditorStateControls();
        viewModel.ShowOutputMessage("已放弃当前文件的内存修改。");
        viewModel.SetOperationStatus("已恢复到上次保存内容", "Success");
        return true;
    }

    private static bool CancelDirtyNavigation(ShellViewModel viewModel)
    {
        viewModel.ShowOutputMessage("已取消导航，当前未保存修改仍保留。");
        viewModel.SetOperationStatus("导航已取消，未保存修改仍保留", "Warning");
        return false;
    }

    private void UpdateEditorStateControls()
    {
        Ra2EditorStateViewModel editorState = _editorStateViewModelFactory.Create(_editableSession);

        EditorStateTextBlock.Text = $"编辑状态：{editorState.StateText}";
        EditorSaveHintTextBlock.Text = editorState.SaveHintText;
        bool hasEditableDirtySession = editorState.HasSession && editorState.IsDirty;
        bool hasEditableSession = editorState.HasSession && editorState.IsEditing;
        bool canRevertDirtySession = editorState.CanRevertInMemoryChanges && editorState.IsDirty;
        SaveCurrentFileButton.IsEnabled = hasEditableDirtySession;
        RevertInMemoryChangesButton.IsEnabled = canRevertDirtySession;
        UndoCurrentFileButton.IsEnabled = hasEditableSession && CanUndoSourceEditor();
        RedoCurrentFileButton.IsEnabled = hasEditableSession && CanRedoSourceEditor();

        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.UpdateDirtyStatus(BuildStatusDirtyStateText(editorState, viewModel));
            UpdateShellStatusBar();
        }
    }

    private static string BuildStatusDirtyStateText(Ra2EditorStateViewModel editorState, ShellViewModel viewModel)
    {
        if (viewModel.CurrentSnapshot is null)
            return "无文件";

        if (editorState.IsDirty)
            return "未保存";

        if (editorState.IsEditing)
            return "已保存";

        return "未修改";
    }

    private static void UpdateSaveOperationStatus(ShellViewModel viewModel, Ra2SaveCurrentFileResult result)
    {
        if (result.Success)
        {
            viewModel.SetOperationStatus("保存成功", "Success");
            return;
        }

        if (result.FailureKind == Ra2SaveCurrentFileFailureKind.SavePlanCannotSave)
        {
            viewModel.SetOperationStatus($"保存未执行：{ShortenStatusReason(result.Message)}", "Warning");
            return;
        }

        viewModel.SetOperationStatus($"保存失败：{ShortenStatusReason(result.Message)}", "Error");
    }

    private static string ShortenStatusReason(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "未知原因";

        string firstLine = message
            .Split([Environment.NewLine, "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?
            .Trim() ?? "未知原因";

        const int maxLength = 72;
        return firstLine.Length <= maxLength
            ? firstLine
            : string.Concat(firstLine.AsSpan(0, maxLength), "...");
    }

    private void UpdateShellStatusBar()
    {
        UpdateShellCaretStatus();
        UpdateShellTextStatus();
    }

    private void UpdateShellCaretStatus()
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        int caretLine = Math.Max(1, SourceTextEditor.TextArea.Caret.Line);
        int caretColumn = Math.Max(1, SourceTextEditor.TextArea.Caret.Column);
        int selectedCharacterCount = GetSelectionLength();
        viewModel.UpdateEditorCaretStatus(caretLine, caretColumn, selectedCharacterCount);
    }

    private void UpdateShellTextStatus()
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        viewModel.UpdateEditorTextStatus(SourceTextEditor.Document.Text);
    }

    private int GetSelectionLength()
    {
        if (SourceTextEditor.TextArea.Selection.IsEmpty)
            return 0;

        int length = 0;
        foreach (ISegment segment in SourceTextEditor.TextArea.Selection.Segments)
            length += Math.Max(0, segment.Length);

        return length;
    }

    private void InstallReadonlySourceHighlighting()
    {
        InvalidateActiveAiEditProposal(markSuperseded: false);
        IRa2FieldDefinitionProvider fieldProvider = _fieldRegistryRuntimeService.Reload(null);
        _fieldRegistryManagerViewModel.RefreshFromState(_fieldRegistryRuntimeService.CurrentState);
        ReplaceReadonlySourceHighlightingTransformer(fieldProvider);
        RefreshFieldAnnotations(null);
    }

    private void ReloadReadonlySourceHighlighting(ShellViewModel viewModel)
    {
        InvalidateActiveAiEditProposal(markSuperseded: false);
        IRa2FieldDefinitionProvider fieldProvider = _fieldRegistryRuntimeService.Reload(viewModel.CurrentProjectRootPath);
        _fieldRegistryManagerViewModel.RefreshFromState(_fieldRegistryRuntimeService.CurrentState);
        ReplaceReadonlySourceHighlightingTransformer(fieldProvider);
        RefreshFieldAnnotations(viewModel.CurrentProjectRootPath);

        if (_fieldRegistryRuntimeService.CurrentState.Warnings.Count > 0)
            viewModel.ShowOutputMessage($"本地字段库已加载，发现 {_fieldRegistryRuntimeService.CurrentState.Warnings.Count} 条警告，内置字段库仍作为兜底。");
    }

    private void ReloadLocalFieldRegistryForReadonlyHighlighting(ShellViewModel viewModel)
    {
        try
        {
            ReloadReadonlySourceHighlighting(viewModel);
            _fieldRegistryCenterWindow?.RefreshFieldRegistryContext(
                _fieldRegistryRuntimeService.CurrentProvider,
                _fieldRegistryRuntimeService.CurrentProvenanceProvider,
                viewModel.CurrentProjectRootPath,
                _fieldRegistryRuntimeService.GetGlobalRootDirectoryPath());
            SourceTextEditor.TextArea.TextView.Redraw();

            int warningCount = _fieldRegistryRuntimeService.CurrentState.Warnings.Count;
            viewModel.ShowOutputMessage($"已重新加载本地字段库：{_fieldRegistryRuntimeService.CurrentState.TotalLocalFieldCount} 个本地字段，{warningCount} 条警告。");
            viewModel.SetOperationStatus(
                warningCount == 0 ? "字段库已重新加载" : $"字段库已重新加载，包含 {warningCount} 条警告",
                warningCount == 0 ? "Success" : "Warning");
        }
        catch (Exception ex)
        {
            string message = $"字段库重新加载失败：{ShortenStatusReason(ex.Message)}";
            viewModel.ShowOutputMessage(message);
            viewModel.SetOperationStatus(message, "Error");
        }
    }

    private void ReplaceReadonlySourceHighlightingTransformer(IRa2FieldDefinitionProvider fieldProvider)
    {
        for (int index = SourceTextEditor.TextArea.TextView.LineTransformers.Count - 1; index >= 0; index--)
        {
            if (SourceTextEditor.TextArea.TextView.LineTransformers[index] is Ra2KnownFieldHighlightingTransformer)
                SourceTextEditor.TextArea.TextView.LineTransformers.RemoveAt(index);
        }

        ReadonlyIniHighlightTokenizer tokenizer = new(fieldProvider);

        SourceTextEditor.TextArea.TextView.LineTransformers.Add(
            new Ra2KnownFieldHighlightingTransformer(tokenizer));
    }

    private IRa2FieldDisplayResolver CreateFieldDisplayResolver(string? projectRootPath)
        => GetCachedFieldAnnotations(projectRootPath).DisplayResolver;

    private string GetProjectFieldAnnotationPath(string? projectRootPath)
        => _fieldAnnotationCoordinator.GetProjectAnnotationPath(projectRootPath);

    private Ra2FieldAnnotationRefreshResult RefreshFieldAnnotations(string? projectRootPath)
    {
        Ra2FieldAnnotationRefreshResult result = _fieldAnnotationCoordinator.Refresh(new Ra2FieldAnnotationRefreshRequest(
            _fieldRegistryRuntimeService.CurrentProvider,
            projectRootPath));
        _fieldAnnotationRefreshCache = result;
        _fieldAnnotationRefreshCacheProjectRootPath = NormalizeProjectRootPath(projectRootPath);
        return result;
    }

    private Ra2FieldAnnotationRefreshResult GetCachedFieldAnnotations(string? projectRootPath)
    {
        string? normalizedProjectRootPath = NormalizeProjectRootPath(projectRootPath);
        if (_fieldAnnotationRefreshCache is not null &&
            string.Equals(_fieldAnnotationRefreshCacheProjectRootPath, normalizedProjectRootPath, StringComparison.OrdinalIgnoreCase))
        {
            return _fieldAnnotationRefreshCache;
        }

        return RefreshFieldAnnotations(projectRootPath);
    }

    private static string? NormalizeProjectRootPath(string? projectRootPath)
        => string.IsNullOrWhiteSpace(projectRootPath)
            ? null
            : Path.GetFullPath(projectRootPath);

    private static void OpenRegistryFolder(string directoryPath, ShellViewModel viewModel)
    {
        try
        {
            Directory.CreateDirectory(directoryPath);
            Process.Start(new ProcessStartInfo
            {
                FileName = directoryPath,
                UseShellExecute = true
            });
            viewModel.ShowOutputMessage($"Opened field registry folder: {directoryPath}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception)
        {
            viewModel.ShowOutputMessage($"Failed to open field registry folder: {ex.Message}");
        }
    }

    private void OpenRollbackFolder(string label, string directoryPath)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        try
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"回滚{label}目录不存在：{directoryPath}");

            Process.Start(new ProcessStartInfo
            {
                FileName = directoryPath,
                UseShellExecute = true
            });
            _fieldRegistryManagerViewModel.ShowRollbackFolderOpened(label, directoryPath);
            viewModel.ShowOutputMessage($"Opened rollback {label} folder: {directoryPath}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or DirectoryNotFoundException)
        {
            _fieldRegistryManagerViewModel.ShowRollbackFolderOpenFailed(label, ex);
            viewModel.ShowOutputMessage($"Failed to open rollback {label} folder: {ex.Message}");
        }
    }

    private async Task TryNavigateToIssueAsync(ShellViewModel viewModel, IdeDiagnosticIssueViewModel? issue)
    {
        if (issue is null)
        {
            viewModel.ShowOutputMessage("No issue selected.");
            return;
        }

        if (string.IsNullOrWhiteSpace(issue.FilePath))
        {
            viewModel.ShowOutputMessage("Cannot jump because this issue has no file path.");
            return;
        }

        if (viewModel.CurrentSnapshot is { } currentSnapshot &&
            string.Equals(issue.FilePath, currentSnapshot.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            TryNavigateToCurrentFileIssue(viewModel, currentSnapshot, issue);
            return;
        }

        ProjectExplorerItemViewModel? fileItem = FindProjectExplorerFileItem(viewModel, issue.FilePath);
        if (fileItem is null)
        {
            viewModel.ShowOutputMessage("Cannot jump because the issue file is not in Project Explorer.");
            return;
        }

        if (!TryResolveDirtyNavigationBeforeLeavingCurrentFile(viewModel))
            return;

        CloseSourceEditorHoverToolTip();
        CloseCompletionDropdown();
        ResetEditableSessionToReadOnly();
        await viewModel.LoadProjectExplorerFileAsync(
            fileItem,
            _fieldRegistryRuntimeService.CurrentProvider);
        StartEditableSessionForCurrentSnapshot(viewModel);
        SelectProjectExplorerItem(fileItem);

        if (viewModel.CurrentSnapshot is null ||
            !string.Equals(issue.FilePath, viewModel.CurrentSnapshot.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            viewModel.ShowOutputMessage("Cannot jump because the issue file failed to load.");
            return;
        }

        if (!viewModel.CurrentSnapshot.CanRunDiagnostics)
        {
            viewModel.ShowOutputMessage("Cannot jump because the target issue file is not loaded as source text.");
            return;
        }

        TryNavigateToIssueLocation(viewModel, issue);
    }

    private void TryNavigateToCurrentFileIssue(
        ShellViewModel viewModel,
        CurrentSourceSnapshot currentSnapshot,
        IdeDiagnosticIssueViewModel issue)
    {
        if (issue.Version != currentSnapshot.Version)
        {
            viewModel.ShowOutputMessage("Cannot jump because the issue result is stale.");
            return;
        }

        TryNavigateToIssueLocation(viewModel, issue);
    }

    private void TryNavigateToIssueLocation(ShellViewModel viewModel, IdeDiagnosticIssueViewModel issue)
    {
        if (issue.LineNumber is null)
        {
            viewModel.ShowOutputMessage("Cannot jump because this issue has no line number.");
            return;
        }

        string message = issue.ColumnNumber is > 0
            ? $"Jumped to issue at Line {issue.LineNumber.Value}, Col {issue.ColumnNumber.Value}."
            : $"Jumped to issue at Line {issue.LineNumber.Value}.";
        TryScrollSourceEditorToLine(viewModel, issue.LineNumber.Value, issue.ColumnNumber, message);
    }

    private bool TryBuildLanguageContext(
        out ShellViewModel viewModel,
        out Ra2DocumentSemanticModel model,
        out Ra2CaretContext context)
    {
        model = null!;
        context = null!;

        if (DataContext is not ShellViewModel currentViewModel)
        {
            viewModel = null!;
            return false;
        }

        viewModel = currentViewModel;
        if (viewModel.CurrentSnapshot is null)
        {
            viewModel.ShowOutputMessage("Language preview skipped: no source file is loaded.");
            return false;
        }

        if (!viewModel.CurrentSnapshot.CanRunDiagnostics)
        {
            viewModel.ShowOutputMessage("Language preview skipped: current source text is not a loaded INI document.");
            return false;
        }

        Ra2DocumentSnapshot snapshot = new(
            viewModel.CurrentSnapshot.FilePath,
            SourceTextEditor.Document.Text,
            viewModel.CurrentSnapshot.Version);
        model = _semanticModelBuilder.Build(snapshot, _fieldRegistryRuntimeService.CurrentProvider);
        int caretOffset = Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        context = _caretContextService.GetContext(model, caretOffset);
        return true;
    }

    private bool TryBuildLanguageNavigationRequest(
        out ShellViewModel viewModel,
        out Ra2LanguageNavigationRequest request)
    {
        request = null!;
        if (!TryBuildLanguageContext(
                out viewModel,
                out Ra2DocumentSemanticModel? model,
                out Ra2CaretContext? context))
        {
            return false;
        }

        request = new Ra2LanguageNavigationRequest(
            model,
            context,
            _fieldRegistryRuntimeService.CurrentProvider,
            _fieldRegistryRuntimeService.CurrentProvenanceProvider);
        return true;
    }

    private bool TryBuildFindReferencesNavigationRequest(
        bool useContextMenuPosition,
        out ShellViewModel viewModel,
        out Ra2LanguageNavigationRequest request)
    {
        request = null!;
        if (!useContextMenuPosition)
            return TryBuildLanguageNavigationRequest(out viewModel, out request);

        int offset = _sourceEditorContextMenuOffset ??
                     Math.Clamp(SourceTextEditor.TextArea.Caret.Offset, 0, SourceTextEditor.Document.TextLength);
        if (!TryBuildLanguageContextAtOffset(
                offset,
                out viewModel,
                out Ra2DocumentSemanticModel? model,
                out Ra2CaretContext? context))
        {
            return false;
        }

        request = new Ra2LanguageNavigationRequest(
            model,
            context,
            _fieldRegistryRuntimeService.CurrentProvider,
            _fieldRegistryRuntimeService.CurrentProvenanceProvider,
            GetContextMenuSelectionSpan(offset));
        return true;
    }

    private bool TryBuildLanguageContextAtOffset(
        int offset,
        out ShellViewModel viewModel,
        out Ra2DocumentSemanticModel model,
        out Ra2CaretContext context)
    {
        model = null!;
        context = null!;

        if (DataContext is not ShellViewModel currentViewModel)
        {
            viewModel = null!;
            return false;
        }

        viewModel = currentViewModel;
        if (viewModel.CurrentSnapshot is null || !viewModel.CurrentSnapshot.CanRunDiagnostics)
            return false;

        Ra2DocumentSnapshot snapshot = new(
            viewModel.CurrentSnapshot.FilePath,
            SourceTextEditor.Document.Text,
            viewModel.CurrentSnapshot.Version);
        model = _semanticModelBuilder.Build(snapshot, _fieldRegistryRuntimeService.CurrentProvider);
        context = _caretContextService.GetContext(model, Math.Clamp(offset, 0, SourceTextEditor.Document.TextLength));
        return true;
    }

    private bool TryGetDocumentOffsetFromMouse(MouseEventArgs e, out int offset)
    {
        offset = 0;
        try
        {
            Point point = e.GetPosition(SourceTextEditor);
            var position = SourceTextEditor.GetPositionFromPoint(point);
            if (position is null)
                return false;

            offset = SourceTextEditor.Document.GetOffset(position.Value.Line, position.Value.Column);
            return offset >= 0 && offset <= SourceTextEditor.Document.TextLength;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private void CloseSourceEditorHoverToolTip()
    {
        _sourceEditorHoverController.Reset();
        _sourceEditorHoverTimer.Stop();
        CloseSourceEditorHoverPopupOnly();
    }

    private void CloseSourceEditorHoverPopupOnly()
    {
        if (_currentHoverPopup is not null)
        {
            _currentHoverPopup.IsOpen = false;
            _currentHoverPopup.Child = null;
            _currentHoverPopup = null;
        }

        if (SourceTextEditor.ToolTip is ToolTip toolTip)
            toolTip.IsOpen = false;

        SourceTextEditor.ToolTip = null;
    }

    private T FindRequiredVisualResource<T>(string key)
        where T : class
    {
        return TryFindResource(key) as T
            ?? throw new InvalidOperationException($"Required visual resource '{key}' was not found or has an unexpected type.");
    }

    private void ShowSourceEditorHoverToolTip(Ra2HoverDisplayViewModel display)
    {
        CloseSourceEditorHoverToolTip();
        double popupWidth = GetSourceEditorHoverWidth();
        Point popupPoint = GetSourceEditorHoverPlacementPoint(popupWidth);
        Border border = new()
        {
            Style = FindRequiredVisualResource<Style>("IdeHoverCardStyle"),
            ClipToBounds = true,
            Focusable = false,
            IsHitTestVisible = false,
            Width = popupWidth,
            Child = CreateSourceEditorHoverCard(display)
        };

        _currentHoverPopup = new Popup
        {
            AllowsTransparency = true,
            Child = border,
            Focusable = false,
            HorizontalOffset = popupPoint.X,
            IsHitTestVisible = false,
            Placement = PlacementMode.Relative,
            PlacementTarget = this,
            StaysOpen = true,
            VerticalOffset = popupPoint.Y
        };
        _currentHoverPopup.IsOpen = true;
    }

    private double GetSourceEditorHoverWidth()
    {
        double availableWidth = ActualWidth > 0 ? ActualWidth : SourceTextEditor.ActualWidth;
        if (availableWidth <= 0)
            return SourceEditorHoverMaximumWidth;

        double paddedWidth = Math.Max(SourceEditorHoverMinimumWidth, availableWidth - (SourceEditorHoverWindowPadding * 2));
        return Math.Min(SourceEditorHoverMaximumWidth, paddedWidth);
    }

    private Point GetSourceEditorHoverPlacementPoint(double popupWidth)
    {
        Point mousePoint = Mouse.GetPosition(this);
        double windowWidth = ActualWidth > 0 ? ActualWidth : popupWidth + (SourceEditorHoverWindowPadding * 2);
        double x = mousePoint.X + SourceEditorHoverHorizontalOffset;
        double maxX = Math.Max(SourceEditorHoverWindowPadding, windowWidth - popupWidth - SourceEditorHoverWindowPadding);
        if (x > maxX)
            x = maxX;

        return new Point(Math.Max(SourceEditorHoverWindowPadding, x), mousePoint.Y + SourceEditorHoverVerticalOffset);
    }

    private StackPanel CreateSourceEditorHoverCard(Ra2HoverDisplayViewModel display)
    {
        StackPanel panel = new()
        {
            ClipToBounds = true,
            Focusable = false,
            IsHitTestVisible = false,
            Orientation = Orientation.Vertical
        };

        Grid header = new()
        {
            ClipToBounds = true,
            Focusable = false,
            IsHitTestVisible = false
        };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        TextBlock? typeBlock = null;
        if (!string.IsNullOrWhiteSpace(display.FieldTypeText))
        {
            typeBlock = CreateHoverInlineText(
                display.FieldTypeText,
                FindRequiredVisualResource<Brush>("UiAccentBrush"),
                FontWeights.SemiBold);
            Grid.SetColumn(typeBlock, 0);
            header.Children.Add(typeBlock);
        }

        string nameText = string.IsNullOrWhiteSpace(display.DisplayNameText)
            ? display.FieldNameText
            : $"{display.FieldNameText} {display.DisplayNameText}";
        TextBlock nameBlock = CreateHoverInlineText(
            nameText,
            FindRequiredVisualResource<Brush>("UiTextPrimaryBrush"),
            FontWeights.SemiBold,
            typeBlock is null ? new Thickness(0) : new Thickness(4, 0, 0, 0));
        Grid.SetColumn(nameBlock, 1);
        header.Children.Add(nameBlock);

        panel.Children.Add(header);

        if (!string.IsNullOrWhiteSpace(display.DescriptionText))
        {
            TextBlock description = new()
            {
                Text = display.DescriptionText,
                Foreground = FindRequiredVisualResource<Brush>("UiSuccessBrush"),
                FontFamily = SourceTextEditor.FontFamily,
                FontSize = SourceTextEditor.FontSize,
                Focusable = false,
                IsHitTestVisible = false,
                Margin = new Thickness(0, 3, 0, 0),
                MaxHeight = Math.Max(32.0, SourceTextEditor.FontSize * 2.8),
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.Wrap
            };
            panel.Children.Add(description);
        }

        if (display.HasExample || display.HasMetadata)
        {
            WrapPanel metadata = new()
            {
                ClipToBounds = true,
                Focusable = false,
                IsHitTestVisible = false,
                Margin = new Thickness(0, 5, 0, 0),
                Orientation = Orientation.Horizontal
            };

            if (!string.IsNullOrWhiteSpace(display.ExampleValueText))
                AddHoverMetadataPair(metadata, "示例", display.ExampleValueText, useCodePill: true);

            if (!string.IsNullOrWhiteSpace(display.SourceText))
                AddHoverMetadataPair(metadata, "来源", display.SourceText, useCodePill: false);

            if (!string.IsNullOrWhiteSpace(display.AppliesToText))
                AddHoverMetadataPair(metadata, "适用", display.AppliesToText, useCodePill: false);

            panel.Children.Add(metadata);
        }

        return panel;
    }

    private TextBlock CreateHoverInlineText(
        string text,
        Brush foreground,
        FontWeight fontWeight,
        Thickness? margin = null)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = foreground,
            FontFamily = SourceTextEditor.FontFamily,
            FontSize = SourceTextEditor.FontSize,
            FontWeight = fontWeight,
            Focusable = false,
            IsHitTestVisible = false,
            Margin = margin ?? new Thickness(0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void AddHoverMetadataPair(WrapPanel metadata, string label, string value, bool useCodePill)
    {
        StackPanel pair = new()
        {
            Focusable = false,
            IsHitTestVisible = false,
            Margin = metadata.Children.Count == 0 ? new Thickness(0) : new Thickness(12, 0, 0, 0),
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        pair.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = FindRequiredVisualResource<Brush>("UiTextSecondaryBrush"),
            FontFamily = SourceTextEditor.FontFamily,
            FontSize = SourceTextEditor.FontSize - 1,
            Focusable = false,
            IsHitTestVisible = false,
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center
        });

        if (useCodePill)
        {
            pair.Children.Add(new Border
            {
                Style = FindRequiredVisualResource<Style>("IdeHoverCodePillStyle"),
                Child = new TextBlock
                {
                    Text = value,
                    Foreground = FindRequiredVisualResource<Brush>("UiAccentBrush"),
                    FontFamily = SourceTextEditor.FontFamily,
                    FontSize = SourceTextEditor.FontSize - 1,
                    Focusable = false,
                    IsHitTestVisible = false,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }
        else
        {
            pair.Children.Add(new TextBlock
            {
                Text = value,
                Foreground = FindRequiredVisualResource<Brush>("UiTextPrimaryBrush"),
                FontFamily = SourceTextEditor.FontFamily,
                FontSize = SourceTextEditor.FontSize - 1,
                Focusable = false,
                IsHitTestVisible = false,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        metadata.Children.Add(pair);
    }

    private void ShowPeekDefinitionWindow(Ra2DefinitionTarget target)
    {
        Ra2PeekDefinitionViewModel viewModel = new(target);
        if (_peekDefinitionWindow is { IsVisible: true })
        {
            _peekDefinitionWindow.Update(viewModel);
            if (TryGetFloatingInspectorCaretScreenPoint(out Point caretBottomScreenDip))
                _peekDefinitionWindow.PlaceNearCaret(caretBottomScreenDip);

            _peekDefinitionWindow.Activate();
            return;
        }

        _peekDefinitionWindow = new Ra2PeekDefinitionWindow(viewModel)
        {
            Owner = this
        };
        if (TryGetFloatingInspectorCaretScreenPoint(out Point newCaretBottomScreenDip))
            _peekDefinitionWindow.PlaceNearCaret(newCaretBottomScreenDip);

        _peekDefinitionWindow.Closed += (_, _) => _peekDefinitionWindow = null;
        _peekDefinitionWindow.Show();
    }

    private void ShowFieldQuickPeekWindow(ViewModels.FieldDetails.Ra2FieldDetailsViewModel viewModel)
    {
        if (_fieldQuickPeekWindow is { IsVisible: true })
        {
            _fieldQuickPeekWindow.Update(viewModel);
            if (TryGetFloatingInspectorCaretScreenPoint(out Point caretBottomScreenDip))
                _fieldQuickPeekWindow.PlaceNearCaret(caretBottomScreenDip);

            _fieldQuickPeekWindow.Activate();
            return;
        }

        _fieldQuickPeekWindow = new Ra2FieldQuickPeekWindow(viewModel)
        {
            Owner = this
        };
        if (TryGetFloatingInspectorCaretScreenPoint(out Point newCaretBottomScreenDip))
            _fieldQuickPeekWindow.PlaceNearCaret(newCaretBottomScreenDip);

        _fieldQuickPeekWindow.Closed += (_, _) => _fieldQuickPeekWindow = null;
        _fieldQuickPeekWindow.Show();
    }

    private void ShowFindReferencesWindow(Ra2ReferenceResult result)
    {
        Ra2FindReferencesViewModel viewModel = new(result);
        FindReferencesView.DataContext = viewModel;
        ShowAndActivateBottomTool("Tool.FindReferences", FindReferencesView);
    }

    private void ShowCompletionDropdown(Ra2CompletionResult result)
    {
        CloseSourceEditorHoverToolTip();
        _completionDropdownViewModel.Update(result);
        if (TryGetCompletionPopupPosition(out Point position))
        {
            CompletionDropdownPopup.HorizontalOffset = Math.Max(0, position.X);
            CompletionDropdownPopup.VerticalOffset = Math.Max(0, position.Y);
        }
        else
        {
            CompletionDropdownPopup.HorizontalOffset = 18;
            CompletionDropdownPopup.VerticalOffset = 48;
        }

        _lastCompletionResult = result;
        CompletionDropdownPopup.IsOpen = true;
    }

    private bool TryGetCompletionPopupPosition(out Point position)
    {
        position = default;
        string text = SourceTextEditor.Document.Text;
        int caretOffset = SourceTextEditor.TextArea.Caret.Offset;
        if (!Ra2CompletionDropdownPositioning.CanShowNearCaret(text, caretOffset))
            return false;

        try
        {
            var textView = SourceTextEditor.TextArea.TextView;
            textView.EnsureVisualLines();
            Rect caretRectangle = SourceTextEditor.TextArea.Caret.CalculateCaretRectangle();
            Point caretBottom = new(
                caretRectangle.Left - textView.ScrollOffset.X,
                caretRectangle.Bottom - textView.ScrollOffset.Y);
            position = textView.TransformToAncestor(SourceTextEditor).Transform(caretBottom);
            return !double.IsNaN(position.X) &&
                   !double.IsNaN(position.Y) &&
                   position.X >= 0 &&
                   position.Y >= 0 &&
                   position.Y <= SourceTextEditor.ActualHeight;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private bool TryGetFloatingInspectorCaretScreenPoint(out Point caretBottomScreenDip)
    {
        caretBottomScreenDip = default;
        string text = SourceTextEditor.Document.Text;
        int caretOffset = SourceTextEditor.TextArea.Caret.Offset;
        if (!Ra2CompletionDropdownPositioning.CanShowNearCaret(text, caretOffset))
            return false;

        try
        {
            var textView = SourceTextEditor.TextArea.TextView;
            textView.EnsureVisualLines();
            Rect caretRectangle = SourceTextEditor.TextArea.Caret.CalculateCaretRectangle();
            Point caretBottom = new(
                caretRectangle.Left - textView.ScrollOffset.X,
                caretRectangle.Bottom - textView.ScrollOffset.Y);
            Point editorPoint = textView.TransformToAncestor(SourceTextEditor).Transform(caretBottom);
            Point screenPixelPoint = SourceTextEditor.PointToScreen(editorPoint);
            caretBottomScreenDip = ConvertScreenPixelsToDip(screenPixelPoint);
            return !double.IsNaN(caretBottomScreenDip.X) &&
                   !double.IsNaN(caretBottomScreenDip.Y) &&
                   !double.IsInfinity(caretBottomScreenDip.X) &&
                   !double.IsInfinity(caretBottomScreenDip.Y);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private Point ConvertScreenPixelsToDip(Point screenPixelPoint)
    {
        PresentationSource? source = PresentationSource.FromVisual(SourceTextEditor) ??
                                     PresentationSource.FromVisual(this);
        return source?.CompositionTarget?.TransformFromDevice.Transform(screenPixelPoint) ??
               screenPixelPoint;
    }

    private void CloseCompletionDropdown(bool clearCompletionResult = true)
    {
        CloseSourceEditorHoverToolTip();
        if (CompletionDropdownPopup.IsOpen)
            CompletionDropdownPopup.IsOpen = false;

        if (clearCompletionResult)
            _lastCompletionResult = null;
    }

    private void TryCommitSelectedCompletionOrClose()
        => TryCommitCompletionItemOrClose(GetSelectedCompletionItemOrFirst());

    private Ra2CompletionDropdownItemViewModel? GetSelectedCompletionItemOrFirst()
        => _completionDropdownViewModel.SelectedItem ??
           (_completionDropdownViewModel.Items.Count > 0 ? _completionDropdownViewModel.Items[0] : null);

    private void TryCommitCompletionItemOrClose(Ra2CompletionDropdownItemViewModel? selectedItem)
    {
        string textBeforeCommit = SourceTextEditor.Document.Text;
        Ra2CompletionResult? completionResultBeforeCommit = _lastCompletionResult;
        Ra2CompletionCommitInteractionResult result = _completionInteractionController.TryCommit(
            new Ra2CompletionCommitInteractionRequest(
                _editableSession,
                _lastCompletionResult,
                selectedItem?.Item,
                selectedItem?.Label));
        if (!result.Success || result.Session is null)
        {
            ShowCompletionCommitStatus(result.Message);
            if (result.ShouldCloseDropdown)
                CloseCompletionDropdown();
            return;
        }

        _editableSession = result.Session;
        _programmaticSemanticUndoState = CreateCompletionSemanticUndoState(
            textBeforeCommit,
            completionResultBeforeCommit,
            result.Session.DocumentState.CurrentText,
            result.CaretOffset);
        SetEditorTextFromProgram(
            result.Session.DocumentState.CurrentText,
            result.CaretOffset);
        UpdateEditorStateControls();
        CloseCompletionDropdown();
        RestoreSourceEditorFocusAtCaret(result.CaretOffset);

        if (DataContext is ShellViewModel currentViewModel)
            currentViewModel.ShowOutputMessage(result.Message);
    }

    private static ProgrammaticSemanticUndoState? CreateCompletionSemanticUndoState(
        string textBeforeCommit,
        Ra2CompletionResult? completionResult,
        string textAfterCommit,
        int redoCaretOffset)
    {
        if (completionResult is null)
            return null;

        Ra2TextSpan span = completionResult.ReplacementSpan;
        if (span.Start < 0 || span.Length < 0 || span.Start + span.Length > textBeforeCommit.Length)
            return null;

        string undoText = textBeforeCommit.Remove(span.Start, span.Length);
        if (string.Equals(undoText, textAfterCommit, StringComparison.Ordinal))
            return null;

        return CreateProgrammaticSemanticUndoState(
            undoText,
            textAfterCommit,
            span.Start,
            redoCaretOffset,
            "已撤销补全字段。",
            "已重做补全字段。");
    }

    private static ProgrammaticSemanticUndoState? CreateProgrammaticSemanticUndoState(
        string undoText,
        string redoText,
        int undoCaretOffset,
        int redoCaretOffset,
        string undoMessage,
        string redoMessage)
    {
        if (string.Equals(undoText, redoText, StringComparison.Ordinal))
            return null;

        return new ProgrammaticSemanticUndoState(
            undoText,
            redoText,
            undoCaretOffset,
            redoCaretOffset,
            undoMessage,
            redoMessage,
            IsUndone: false);
    }

    private void ShowCompletionCommitStatus(string message)
    {
        if (DataContext is ShellViewModel viewModel)
            viewModel.ShowOutputMessage(message);

        EditorSaveHintTextBlock.Text = message;
    }

    private bool IsFocusMovingInsideCompletionDropdown(object? newFocus)
    {
        if (!CompletionDropdownPopup.IsOpen)
            return false;

        if (CompletionDropdownView.IsKeyboardFocusWithin || CompletionDropdownView.IsMouseOver)
            return true;

        return newFocus is DependencyObject dependencyObject &&
            (ReferenceEquals(dependencyObject, CompletionDropdownView) || CompletionDropdownView.IsAncestorOf(dependencyObject));
    }

    private void RestoreSourceEditorFocusAtCaret(int caretOffset)
    {
        Dispatcher.BeginInvoke(
            () =>
            {
                int normalizedOffset = Math.Clamp(caretOffset, 0, SourceTextEditor.Document.TextLength);
                SourceTextEditor.Focus();
                SourceTextEditor.TextArea.Focus();
                Keyboard.Focus(SourceTextEditor.TextArea);
                SourceTextEditor.TextArea.Caret.Offset = normalizedOffset;
            },
            DispatcherPriority.ContextIdle);
    }

    private void FindReferencesWindow_OnReferenceNavigateRequested(object? sender, Ra2ReferenceItemViewModel item)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        TryScrollSourceEditorToLanguageTarget(
            viewModel,
            item.ValueSpanStart,
            $"Jumped to reference [{item.Value}] at Line {item.Line}.",
            null);
    }

    private void TryNavigateToSection(ShellViewModel viewModel, ProjectExplorerItemViewModel section)
    {
        if (section.Kind != ProjectExplorerItemKind.Section)
        {
            viewModel.ShowOutputMessage("Explorer navigation skipped: selected node is not a section.");
            return;
        }

        if (string.IsNullOrWhiteSpace(section.SectionId))
        {
            viewModel.ShowOutputMessage("Explorer navigation skipped: section id is missing.");
            return;
        }

        if (viewModel.CurrentSnapshot is null)
        {
            viewModel.ShowOutputMessage("Cannot jump because no source file is loaded.");
            return;
        }

        if (!string.Equals(section.FilePath, viewModel.CurrentSnapshot.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            viewModel.ShowOutputMessage("Cannot jump because this section belongs to another file.");
            return;
        }

        ReadonlySectionNavigationTarget? target = _sectionNavigationResolver.Resolve(
            SourceTextEditor.Text,
            section.SectionId,
            section.LineNumber);

        if (target is null)
        {
            viewModel.ShowOutputMessage($"Explorer navigation skipped: [{section.SectionId}] header was not found in the current text.");
            return;
        }

        TryScrollSourceEditorToCharacterIndex(viewModel, target);
    }

    private void ApplyProjectExplorerVisibility()
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        _isApplyingProjectExplorerVisibility = true;
        try
        {
            if (viewModel.IsProjectExplorerVisible)
            {
                _dockLayoutCoordinator.ShowAndActivate("Tool.SectionExplorer");
                return;
            }

            if (_dockLayoutCoordinator.FindTool("Tool.SectionExplorer") is { IsVisible: true } sectionExplorer)
                sectionExplorer.Hide();
        }
        finally
        {
            _isApplyingProjectExplorerVisibility = false;
        }
    }

    private void ApplyBottomToolPanelVisibility()
    {
        if (_isBottomToolPanelVisible)
        {
            RestoreBottomToolVisibilitySnapshot();
            return;
        }

        CaptureAndHideBottomTools();
    }

    private void ShowAndActivateBottomTool(string contentId, UIElement? focusTarget = null)
    {
        _isBottomToolPanelVisible = true;
        _hasBottomToolVisibilitySnapshot = false;
        _bottomToolVisibilityBeforeCollapse.Clear();
        _dockLayoutCoordinator.ShowAndActivate(contentId);
        _lastActiveBottomToolContentId = contentId;
        focusTarget?.Focus();
    }

    private void ShowAndActivateSearchTool()
    {
        _dockLayoutCoordinator.ShowAndActivate("Tool.Search");
        if (!_floatingChromeController.RestoreAndActivateMinimizedHost(
                "Tool.Search",
                () => SearchToolContentHost.Focus()))
        {
            SearchToolContentHost.Focus();
        }
    }

    private async void SearchToolView_OnSearchRequested(object? sender, EventArgs e)
    {
        if (_searchToolView is null || DataContext is not ShellViewModel viewModel)
            return;

        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = new CancellationTokenSource();
        CancellationToken cancellationToken = _searchCancellation.Token;

        SearchToolWindowViewModel searchViewModel = _searchToolView.ViewModel;
        Ra2SearchOptions options = searchViewModel.CreateOptions();
        IReadOnlyList<ReadonlyIniFileDescriptor> files = viewModel.ProjectExplorer.Items
            .Where(item => item.Kind == ProjectExplorerItemKind.File)
            .Select(item => item.ToDescriptor())
            .ToArray();
        string? currentFilePath = viewModel.CurrentSnapshot?.FilePath;
        string? currentEditorText = viewModel.CurrentSnapshot is null
            ? null
            : SourceTextEditor.Document.Text;

        searchViewModel.BeginSearch();
        try
        {
            Ra2SearchExecutionResult result = await Task.Run(
                () => _projectSearchService.Search(
                    options,
                    files,
                    currentFilePath,
                    currentEditorText,
                    cancellationToken),
                cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
                searchViewModel.ApplySearchResult(result);
        }
        catch (OperationCanceledException)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                searchViewModel.ApplySearchResult(Ra2SearchExecutionResult.Failed(
                    Ra2SearchFailureKind.Canceled,
                    "查找已取消。"));
            }
        }
        catch (Exception ex)
        {
            searchViewModel.ApplySearchResult(Ra2SearchExecutionResult.Failed(
                Ra2SearchFailureKind.Unexpected,
                $"查找失败：{ex.Message}"));
        }
    }

    private async void SearchToolView_OnResultNavigateRequested(SearchResultItemViewModel result)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;

        if (viewModel.CurrentSnapshot is null ||
            !string.Equals(result.FilePath, viewModel.CurrentSnapshot.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            ProjectExplorerItemViewModel? fileItem = FindProjectExplorerFileItem(viewModel, result.FilePath);
            if (fileItem is null)
            {
                viewModel.ShowOutputMessage("无法导航：查找结果文件已不在项目浏览器中。");
                return;
            }

            if (!TryResolveDirtyNavigationBeforeLeavingCurrentFile(viewModel))
                return;

            CloseSourceEditorHoverToolTip();
            CloseCompletionDropdown();
            ResetEditableSessionToReadOnly();
            await viewModel.LoadProjectExplorerFileAsync(
                fileItem,
                _fieldRegistryRuntimeService.CurrentProvider);
            StartEditableSessionForCurrentSnapshot(viewModel);
            SelectProjectExplorerItem(fileItem);
        }

        if (viewModel.CurrentSnapshot is null ||
            !string.Equals(result.FilePath, viewModel.CurrentSnapshot.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            viewModel.ShowOutputMessage("无法导航：查找结果文件加载失败。");
            return;
        }

        string currentText = SourceTextEditor.Document.Text;
        if (result.CharacterIndex < 0 ||
            result.CharacterIndex + result.Length > currentText.Length ||
            !string.Equals(
                currentText.Substring(result.CharacterIndex, result.Length),
                result.MatchedText,
                StringComparison.Ordinal))
        {
            viewModel.ShowOutputMessage("无法导航：查找结果已过期，请重新执行查找。");
            return;
        }

        TryScrollSourceEditorToLanguageTarget(
            viewModel,
            result.CharacterIndex,
            $"已导航到 {result.FileName} 第 {result.LineNumber} 行，第 {result.ColumnNumber} 列。",
            result.SectionName);
    }

    private void SearchToolView_OnReplacePreviewRequested(object? sender, EventArgs e)
    {
        if (_searchToolView is null || DataContext is not ShellViewModel viewModel)
            return;

        SearchToolWindowViewModel searchViewModel = _searchToolView.ViewModel;
        if (_editableSession is not null &&
            !string.Equals(
                _editableSession.DocumentState.CurrentText,
                SourceTextEditor.Document.Text,
                StringComparison.Ordinal))
        {
            searchViewModel.ApplyReplacePlan(Ra2CurrentFileReplacePlan.Failed(
                Ra2ReplaceFailureKind.Unexpected,
                "编辑器文本仍在同步，请稍后重新预览替换。"));
            return;
        }

        Ra2CurrentFileReplacePlan plan = _currentFileReplacePlanner.Plan(
            _editableSession,
            searchViewModel.CreateOptions(),
            searchViewModel.ReplacementText);
        searchViewModel.ApplyReplacePlan(plan);
        viewModel.ShowOutputMessage(plan.Message);
    }

    private void SearchToolView_OnReplaceApplyRequested(object? sender, EventArgs e)
    {
        if (_searchToolView is null || DataContext is not ShellViewModel viewModel)
            return;

        SearchToolWindowViewModel searchViewModel = _searchToolView.ViewModel;
        Ra2CurrentFileReplacePlan? plan = searchViewModel.CurrentReplacePlan;
        if (plan is null ||
            !plan.IsCurrentFor(_editableSession) ||
            !string.Equals(SourceTextEditor.Document.Text, plan.OriginalText, StringComparison.Ordinal))
        {
            const string staleMessage = "替换预览已过期，请重新预览后再应用。";
            searchViewModel.CompleteReplace(staleMessage);
            viewModel.ShowOutputMessage(staleMessage);
            return;
        }

        int undoCaretOffset = Math.Clamp(
            SourceTextEditor.TextArea.Caret.Offset,
            0,
            plan.OriginalText.Length);
        int redoCaretOffset = Math.Clamp(undoCaretOffset, 0, plan.UpdatedText.Length);
        ProgrammaticSemanticUndoState? undoState = CreateProgrammaticSemanticUndoState(
            plan.OriginalText,
            plan.UpdatedText,
            undoCaretOffset,
            redoCaretOffset,
            $"已撤销当前文件的 {plan.MatchCount} 处替换。",
            $"已重做当前文件的 {plan.MatchCount} 处替换。");
        if (undoState is null)
        {
            const string noChangeMessage = "替换未产生文本变化。";
            searchViewModel.CompleteReplace(noChangeMessage);
            viewModel.ShowOutputMessage(noChangeMessage);
            return;
        }

        ApplyProgrammaticSemanticText(plan.UpdatedText, redoCaretOffset);
        _programmaticSemanticUndoState = undoState;
        UpdateEditorStateControls();
        CommandManager.InvalidateRequerySuggested();

        string successMessage = $"已在当前文件内存中替换 {plan.MatchCount} 处；尚未保存。";
        searchViewModel.CompleteReplace(successMessage);
        viewModel.ShowOutputMessage(successMessage);
        viewModel.SetOperationStatus($"当前文件已替换 {plan.MatchCount} 处，等待保存", "Warning");
    }

    private LayoutAnchorable[] GetBottomTools()
        => _dockLayoutCoordinator.GetTools(ShellDockHomeZone.Bottom);

    private void CaptureAndHideBottomTools()
    {
        _bottomToolVisibilityBeforeCollapse.Clear();
        foreach (LayoutAnchorable tool in GetBottomTools())
        {
            if (!tool.IsVisible)
                continue;

            _bottomToolVisibilityBeforeCollapse.Add(tool.ContentId);
            if (tool.IsActive || tool.IsSelected)
                _lastActiveBottomToolContentId = tool.ContentId;
        }

        _hasBottomToolVisibilitySnapshot = true;
        foreach (LayoutAnchorable tool in GetBottomTools())
        {
            if (tool.IsVisible)
                tool.Hide();
        }
    }

    private void RestoreBottomToolVisibilitySnapshot()
    {
        if (!_hasBottomToolVisibilitySnapshot)
            return;

        LayoutAnchorable? toolToActivate = null;
        foreach (LayoutAnchorable tool in GetBottomTools())
        {
            if (!_bottomToolVisibilityBeforeCollapse.Contains(tool.ContentId))
                continue;

            if (!tool.IsVisible)
                _dockLayoutCoordinator.ShowAndActivate(tool.ContentId);

            if (string.Equals(tool.ContentId, _lastActiveBottomToolContentId, StringComparison.Ordinal))
                toolToActivate = tool;
        }

        if (toolToActivate is not null)
        {
            toolToActivate.IsSelected = true;
            toolToActivate.IsActive = true;
        }

        _hasBottomToolVisibilitySnapshot = false;
        _bottomToolVisibilityBeforeCollapse.Clear();
    }

    private void SectionExplorerAnchorable_OnIsVisibleChanged(object? sender, EventArgs e)
    {
        if (_isApplyingProjectExplorerVisibility || DataContext is not ShellViewModel viewModel)
            return;

        if (sender is LayoutAnchorable sectionExplorer &&
            viewModel.IsProjectExplorerVisible != sectionExplorer.IsVisible)
            viewModel.ToggleProjectExplorer();
    }

    private void RebindSectionExplorerVisibilitySource()
    {
        if (_sectionExplorerVisibilitySource is not null)
            _sectionExplorerVisibilitySource.IsVisibleChanged -= SectionExplorerAnchorable_OnIsVisibleChanged;
        _sectionExplorerVisibilitySource = _dockLayoutCoordinator.FindTool("Tool.SectionExplorer");
        if (_sectionExplorerVisibilitySource is not null)
            _sectionExplorerVisibilitySource.IsVisibleChanged += SectionExplorerAnchorable_OnIsVisibleChanged;
    }

    private async Task TryRestorePersistedDockLayoutAsync()
    {
        ShellDockLayoutOperationResult readResult = _dockLayoutStore.TryRead(out string? serialized);
        if (!readResult.Succeeded)
        {
            if (readResult.FailureKind != ShellDockLayoutFailureKind.NotFound)
            {
                _dockLayoutStore.TryQuarantine();
                ReportDockLayoutFailure("本机窗口布局无效，已使用默认布局。", readResult);
                FinalizeRestoredDockLayout();
                return;
            }

            await TryMigrateLegacyDockLayoutAsync();
            FinalizeRestoredDockLayout();
            return;
        }

        ShellDockLayoutOperationResult restoreResult = _dockLayoutSession.TryRestore(serialized!);
        if (!restoreResult.Succeeded)
        {
            _dockLayoutStore.TryQuarantine();
            ShellDockLayoutOperationResult fallbackResult = _dockLayoutSession.ResetToCompiledDefault();
            if (!fallbackResult.Succeeded)
                ReportDockLayoutFailure("窗口布局恢复失败，当前布局可能不完整。", fallbackResult);
            else
                ReportDockLayoutFailure("本机窗口布局不兼容，已恢复默认布局。", restoreResult);
        }

        FinalizeRestoredDockLayout();
    }

    private async Task TryMigrateLegacyDockLayoutAsync()
    {
        ShellDockLayoutOperationResult legacyReadResult = _dockLayoutStore.TryReadLegacy(out string? legacySerialized);
        if (!legacyReadResult.Succeeded)
        {
            if (legacyReadResult.FailureKind != ShellDockLayoutFailureKind.NotFound)
            {
                _dockLayoutStore.TryQuarantineLegacy();
                ReportDockLayoutFailure("旧版窗口布局无效，已使用默认布局。", legacyReadResult);
            }
            return;
        }

        ShellDockLayoutOperationResult restoreResult = _dockLayoutSession.TryRestore(legacySerialized!);
        if (!restoreResult.Succeeded)
        {
            _dockLayoutStore.TryQuarantineLegacy();
            ShellDockLayoutOperationResult fallbackResult = _dockLayoutSession.ResetToCompiledDefault();
            if (!fallbackResult.Succeeded)
                ReportDockLayoutFailure("旧版窗口布局迁移失败，当前布局可能不完整。", fallbackResult);
            else
                ReportDockLayoutFailure("旧版窗口布局不兼容，已恢复默认布局。", restoreResult);
            return;
        }

        _dockLayoutCoordinator.PlaceToolAtCompiledDefaultHome("Tool.Search");
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
        _dockLayoutCoordinator.ApplyToolCompiledDefaultVisibility("Tool.Search");
        PersistCurrentDockLayout("旧版窗口布局已迁移，但无法保存 v2 布局。", reportSuccess: false);
    }

    private void FinalizeRestoredDockLayout()
    {
        bool usedMonitorFallback = _dockLayoutCoordinator.RecoverFloatingGeometry(_dockMonitorWorkAreaProvider);
        RebindSectionExplorerVisibilitySource();
        SynchronizeShellStateFromDockLayout();
        if (usedMonitorFallback && DataContext is ShellViewModel viewModel)
            viewModel.SetOperationStatus("部分浮动窗口已移回当前显示器。", "Info");
    }

    private ShellDockLayoutOperationResult PersistCurrentDockLayout(string failureMessage, bool reportSuccess)
    {
        ShellDockLayoutOperationResult serializeResult = _dockLayoutSession.TrySerializeCurrent(out string? serialized);
        if (!serializeResult.Succeeded)
        {
            ReportDockLayoutFailure(failureMessage, serializeResult);
            return serializeResult;
        }

        ShellDockLayoutOperationResult writeResult = _dockLayoutStore.TryWrite(serialized!);
        if (!writeResult.Succeeded)
            ReportDockLayoutFailure(failureMessage, writeResult);
        else if (reportSuccess && DataContext is ShellViewModel viewModel)
            viewModel.SetOperationStatus("窗口布局已保存。", "Info");
        return writeResult;
    }

    private void SynchronizeShellStateFromDockLayout()
    {
        _hasBottomToolVisibilitySnapshot = false;
        _bottomToolVisibilityBeforeCollapse.Clear();
        LayoutAnchorable[] bottomTools = _dockLayoutCoordinator.GetTools(ShellDockHomeZone.Bottom);
        _isBottomToolPanelVisible = bottomTools.Any(tool => tool.IsVisible);
        _lastActiveBottomToolContentId = bottomTools
            .FirstOrDefault(tool => tool.IsActive || tool.IsSelected)?.ContentId
            ?? bottomTools.FirstOrDefault(tool => tool.IsVisible)?.ContentId
            ?? "Tool.Output";

        if (DataContext is not ShellViewModel viewModel ||
            _dockLayoutCoordinator.FindTool("Tool.SectionExplorer") is not { } sectionExplorer ||
            viewModel.IsProjectExplorerVisible == sectionExplorer.IsVisible)
            return;

        _isApplyingProjectExplorerVisibility = true;
        try
        {
            viewModel.ToggleProjectExplorer();
        }
        finally
        {
            _isApplyingProjectExplorerVisibility = false;
        }
    }

    private void ReportDockLayoutFailure(string message, ShellDockLayoutOperationResult result)
    {
        if (DataContext is not ShellViewModel viewModel)
            return;
        viewModel.SetOperationStatus(message, "Warning");
        viewModel.ShowOutputMessage($"{message}（{result.FailureKind}）");
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _dockLayoutCoordinator.BeginShellClose();
        base.OnClosing(e);
        if (e.Cancel)
        {
            _dockLayoutCoordinator.CancelShellClose();
            return;
        }

        PersistCurrentDockLayout("关闭时无法保存窗口布局，已保留上一次有效布局。", reportSuccess: false);
    }

    protected override void OnClosed(EventArgs e)
    {
        _isShellClosed = true;
        _floatingChromeController.Dispose();
        _windowChromeController.Dispose();
        if (_activeAiAssistantStreamingMessage is { } streamingMessage)
            ReleaseAiAssistantStreamingMessage(streamingMessage);

        InvalidateActiveAiEditProposal(markSuperseded: false);
        _aiAssistantRequestLifecycle.TryCancelCurrent();

        if (_boundSourceEditor is not null)
        {
            _boundSourceEditor.PropertyChanged -= SourceEditor_OnPropertyChanged;
            _boundSourceEditor = null;
        }

        DataContextChanged -= ShellWindow_OnDataContextChanged;
        _sourceEditorHoverTimer.Tick -= SourceEditorHoverTimer_OnTick;
        _sourceEditorCompletionAutoTriggerTimer.Tick -= SourceEditorCompletionAutoTriggerTimer_OnTick;
        StopCompletionAutoTrigger();
        SourceTextEditor.TextArea.Caret.PositionChanged -= SourceTextEditorCaret_OnPositionChanged;
        SourceTextEditor.TextArea.SelectionChanged -= SourceTextEditorSelection_OnChanged;
        SourceTextEditor.TextArea.TextView.ScrollOffsetChanged -= SourceTextEditorTextView_OnScrollOffsetChanged;
        SourceTextEditor.TextArea.PreviewKeyDown -= SourceTextEditorTextArea_OnPreviewKeyDown;
        SourceTextEditor.MouseMove -= SourceTextEditor_OnMouseMove;
        SourceTextEditor.MouseLeave -= SourceTextEditor_OnMouseLeave;
        CompletionDropdownView.CompletionItemDoubleClicked -= CompletionDropdownView_OnCompletionItemDoubleClicked;
        CompletionDropdownView.CompletionCommitRequested -= CompletionDropdownView_OnCompletionCommitRequested;
        CompletionDropdownView.CompletionCloseRequested -= CompletionDropdownView_OnCompletionCloseRequested;
        FindReferencesView.ReferenceNavigateRequested -= FindReferencesWindow_OnReferenceNavigateRequested;
        if (_searchToolView is not null)
        {
            _searchToolView.SearchRequested -= SearchToolView_OnSearchRequested;
            _searchToolView.ResultNavigateRequested -= SearchToolView_OnResultNavigateRequested;
            _searchToolView.ReplacePreviewRequested -= SearchToolView_OnReplacePreviewRequested;
            _searchToolView.ReplaceApplyRequested -= SearchToolView_OnReplaceApplyRequested;
            _searchToolView = null;
        }
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = null;
        if (_sectionExplorerVisibilitySource is not null)
            _sectionExplorerVisibilitySource.IsVisibleChanged -= SectionExplorerAnchorable_OnIsVisibleChanged;
        CloseSourceEditorHoverToolTip();
        CloseCompletionDropdown();
        _issuesToolWindow?.Close();
        _fieldRegistryCenterWindow?.Close();
        _fieldLearningWizardWindow?.Close();
        _fieldRegistryManagerWindow?.Close();
        _fieldRegistryHarvestPreviewWindow?.Close();
        _peekDefinitionWindow?.Close();
        _fieldQuickPeekWindow?.Close();
        base.OnClosed(e);
    }

    private void TryScrollSourceEditorToLine(
        ShellViewModel viewModel,
        int oneBasedLineNumber,
        int? oneBasedColumnNumber,
        string successMessage)
    {
        if (oneBasedLineNumber <= 0)
        {
            viewModel.ShowOutputMessage("Navigation skipped: target has no valid line number.");
            return;
        }

        if (SourceTextEditor.Document.LineCount <= 0)
        {
            viewModel.ShowOutputMessage("Navigation skipped: source text has no navigable lines.");
            return;
        }

        int targetLineNumber = Math.Min(oneBasedLineNumber, SourceTextEditor.Document.LineCount);

        Dispatcher.BeginInvoke(
            () =>
            {
                if (SourceTextEditor.Document.LineCount <= 0)
                {
                    viewModel.ShowOutputMessage("Navigation skipped: source text has no navigable lines.");
                    return;
                }

                int currentTargetLineNumber = Math.Min(targetLineNumber, SourceTextEditor.Document.LineCount);
                DocumentLine line = SourceTextEditor.Document.GetLineByNumber(currentTargetLineNumber);
                int columnOffset = oneBasedColumnNumber is > 1
                    ? Math.Min(oneBasedColumnNumber.Value - 1, line.Length)
                    : 0;
                int targetOffset = line.Offset + columnOffset;
                SourceTextEditor.Focus();
                SourceTextEditor.TextArea.Caret.Offset = targetOffset;
                SourceTextEditor.ScrollTo(currentTargetLineNumber, Math.Max(1, columnOffset + 1));
                viewModel.ShowOutputMessage(successMessage);
            },
            DispatcherPriority.Background);
    }

    private void TryScrollSourceEditorToLanguageTarget(
        ShellViewModel viewModel,
        int characterIndex,
        string successMessage,
        string? sectionName)
    {
        if (characterIndex < 0 || characterIndex > SourceTextEditor.Document.TextLength)
        {
            viewModel.ShowOutputMessage("Language navigation skipped: target character position is out of range.");
            return;
        }

        Dispatcher.BeginInvoke(
            () =>
            {
                if (characterIndex < 0 || characterIndex > SourceTextEditor.Document.TextLength)
                {
                    viewModel.ShowOutputMessage("Language navigation skipped: target character position is out of range.");
                    return;
                }

                TextLocation location = SourceTextEditor.Document.GetLocation(characterIndex);
                SourceTextEditor.Focus();
                SourceTextEditor.TextArea.Caret.Offset = characterIndex;
                SourceTextEditor.ScrollTo(location.Line, location.Column);
                string outputMessage = successMessage;
                if (!string.IsNullOrWhiteSpace(sectionName) && viewModel.CurrentSnapshot is not null)
                {
                    viewModel.ProjectExplorer.MarkCurrentSection(viewModel.CurrentSnapshot.FilePath, sectionName);
                    ProjectExplorerItemViewModel? matchingSection = FindProjectExplorerSectionItem(
                        viewModel,
                        viewModel.CurrentSnapshot.FilePath,
                        sectionName);
                    if (matchingSection is not null)
                    {
                        SelectProjectExplorerItem(matchingSection);
                    }
                    else
                    {
                        outputMessage = $"{successMessage} Navigation tree did not contain [{sectionName}].";
                    }
                }

                viewModel.ShowOutputMessage(outputMessage);
            },
            DispatcherPriority.Background);
    }

    private void TryScrollSourceEditorToCharacterIndex(ShellViewModel viewModel, ReadonlySectionNavigationTarget target)
    {
        if (target.CharacterIndex < 0 || target.CharacterIndex > SourceTextEditor.Document.TextLength)
        {
            viewModel.ShowOutputMessage("Explorer navigation skipped: section character position is out of range.");
            return;
        }

        Dispatcher.BeginInvoke(
            () =>
            {
                if (target.CharacterIndex < 0 || target.CharacterIndex > SourceTextEditor.Document.TextLength)
                {
                    viewModel.ShowOutputMessage("Explorer navigation skipped: section character position is out of range.");
                    return;
                }

                TextLocation location = SourceTextEditor.Document.GetLocation(target.CharacterIndex);
                SourceTextEditor.Focus();
                SourceTextEditor.TextArea.Caret.Offset = target.CharacterIndex;
                SourceTextEditor.ScrollTo(location.Line, location.Column);
                if (viewModel.CurrentSnapshot is not null)
                {
                    viewModel.ProjectExplorer.MarkCurrentSection(viewModel.CurrentSnapshot.FilePath, target.SectionId);
                    ProjectExplorerItemViewModel? matchingSection = FindProjectExplorerSectionItem(
                        viewModel,
                        viewModel.CurrentSnapshot.FilePath,
                        target.SectionId);
                    if (matchingSection is not null)
                        SelectProjectExplorerItem(matchingSection);
                }

                viewModel.ShowOutputMessage($"Jumped to section [{target.SectionId}] at Line {target.OneBasedLineNumber}.");
            },
            DispatcherPriority.Background);
    }
}
