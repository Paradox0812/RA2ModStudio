using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.ViewModels;

public sealed class ShellViewModel : INotifyPropertyChanged
{
    private readonly ProjectOpenService _projectOpenService;
    private readonly ReadonlyIniContentService _contentService;
    private readonly ReadonlyProjectExplorerGroupingService _projectExplorerGroupingService;
    private readonly CurrentFileReadonlyDiagnosticService _diagnosticService;
    private readonly ManualFullDiagnosticsService _manualFullDiagnosticsService = new();
    private CurrentSourceSnapshot? _currentSnapshot;
    private string _currentProjectRootPath = string.Empty;
    private IReadOnlyList<ReadonlyIniFileDescriptor> _currentProjectFiles = [];
    private int _selectedFileLoadVersion;
    private string _outputText = "就绪";
    private string _projectTitle = "未打开项目";
    private bool _isProjectExplorerVisible = true;
    private string _statusCurrentFileText = "未选择文件";
    private string _statusDirtyStateText = "无文件";
    private string _statusEncodingText = "编码：-";
    private string _statusNewlineText = "换行：-";
    private string _statusCaretPositionText = "行 1，列 1";
    private string _statusSelectionText = "未选择";
    private string _statusOperationText = "就绪";
    private string _statusOperationKindText = "Info";

    public ShellViewModel()
        : this(
            new ProjectOpenService(),
            new ReadonlyIniContentService(new IniFileStore()),
            new ReadonlyNavigatorIndexService(),
            new CurrentFileReadonlyDiagnosticService())
    {
    }

    public ShellViewModel(ProjectOpenService projectOpenService, ReadonlyIniContentService contentService)
        : this(
            projectOpenService,
            contentService,
            new ReadonlyNavigatorIndexService(),
            new CurrentFileReadonlyDiagnosticService())
    {
    }

    public ShellViewModel(
        ProjectOpenService projectOpenService,
        ReadonlyIniContentService contentService,
        ReadonlyNavigatorIndexService navigatorIndexService,
        CurrentFileReadonlyDiagnosticService diagnosticService)
    {
        _projectOpenService = projectOpenService;
        _contentService = contentService;
        _projectExplorerGroupingService = new ReadonlyProjectExplorerGroupingService();
        _diagnosticService = diagnosticService;
        SourceEditor.ShowEmptyState("打开项目文件夹后可预览 INI 源文本。");
        Navigator.Clear();
        ProjectExplorer.Clear();
        Issues.Clear(IssuesStatusMessages.NoFileSelected);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; } = "RA2 INI Editor IDE";

    public string NavigatorTitle { get; } = "对象 / Section 导航";

    public NavigatorViewModel Navigator { get; } = new();

    public SourceEditorViewModel SourceEditor { get; } = new();

    public ProjectExplorerViewModel ProjectExplorer { get; } = new();

    public IssuesViewModel Issues { get; } = new();

    public string ProjectExplorerTitle { get; } = "项目浏览器";

    public string IssuesTitle { get; } = "问题 / 输出";

    public string ProjectTitle
    {
        get => _projectTitle;
        private set => SetProperty(ref _projectTitle, value);
    }

    public string OutputText
    {
        get => _outputText;
        private set => SetProperty(ref _outputText, value);
    }

    /// <summary>
    /// Gets whether the Project Explorer panel is visible in the workspace.
    /// </summary>
    public bool IsProjectExplorerVisible
    {
        get => _isProjectExplorerVisible;
        private set => SetProperty(ref _isProjectExplorerVisible, value);
    }

    public CurrentSourceSnapshot? CurrentSnapshot
    {
        get => _currentSnapshot;
        private set
        {
            if (EqualityComparer<CurrentSourceSnapshot?>.Default.Equals(_currentSnapshot, value))
                return;

            _currentSnapshot = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSnapshot)));
            RefreshStatusFromCurrentSnapshot();
        }
    }

    public string StatusCurrentFileText
    {
        get => _statusCurrentFileText;
        private set => SetProperty(ref _statusCurrentFileText, value);
    }

    public string StatusDirtyStateText
    {
        get => _statusDirtyStateText;
        private set => SetProperty(ref _statusDirtyStateText, value);
    }

    public string StatusEncodingText
    {
        get => _statusEncodingText;
        private set => SetProperty(ref _statusEncodingText, value);
    }

    public string StatusNewlineText
    {
        get => _statusNewlineText;
        private set => SetProperty(ref _statusNewlineText, value);
    }

    public string StatusCaretPositionText
    {
        get => _statusCaretPositionText;
        private set => SetProperty(ref _statusCaretPositionText, value);
    }

    public string StatusSelectionText
    {
        get => _statusSelectionText;
        private set => SetProperty(ref _statusSelectionText, value);
    }

    public string StatusOperationText
    {
        get => _statusOperationText;
        private set => SetProperty(ref _statusOperationText, value);
    }

    public string StatusOperationKindText
    {
        get => _statusOperationKindText;
        private set => SetProperty(ref _statusOperationKindText, value);
    }

    public string? CurrentProjectRootPath => string.IsNullOrWhiteSpace(_currentProjectRootPath)
        ? null
        : _currentProjectRootPath;

    internal IReadOnlyList<ReadonlyIniFileDescriptor> CurrentProjectFiles => _currentProjectFiles;

    public async Task OpenProjectFolderAsync(string folderPath)
    {
        _selectedFileLoadVersion++;
        _currentProjectRootPath = string.Empty;
        _currentProjectFiles = [];
        CurrentSnapshot = null;
        Issues.Clear(IssuesStatusMessages.NoFileSelected);
        ProjectExplorer.Clear();
        SourceEditor.ShowLoading("正在打开项目...");
        Navigator.ShowLoading("正在打开项目...");

        try
        {
            var result = await Task.Run(() => _projectOpenService.OpenFolderReadonly(folderPath));
            _currentProjectRootPath = result.ProjectFolderPath;
            _currentProjectFiles = Array.AsReadOnly(result.Files.ToArray());
            ProjectTitle = BuildProjectTitle(folderPath);
            ProjectExplorer.ShowFiles(result.Files);

            if (result.IsEmpty)
            {
                SourceEditor.ShowEmptyState("所选文件夹中没有找到 INI 文件。");
                Navigator.ShowEmptyState("所选文件夹中没有找到 INI 文件。");
                CurrentSnapshot = null;
                Issues.Clear(IssuesStatusMessages.NoFileSelected);
                OutputText = $"已打开 {folderPath}，未找到 INI 文件。";
                return;
            }

            SourceEditor.ShowEmptyState("请从项目浏览器中选择一个 INI 文件。");
            Navigator.Clear();
            CurrentSnapshot = null;
            Issues.Clear(IssuesStatusMessages.NoFileSelected);
            OutputText = $"在 {folderPath} 中找到 {result.TotalIniFileCount} 个 INI 文件。";
        }
        catch (Exception ex)
        {
            _currentProjectFiles = [];
            ProjectExplorer.ShowFiles([]);
            SourceEditor.ShowError("打开失败", $"无法打开所选文件夹：{Environment.NewLine}{Environment.NewLine}{ex.Message}");
            Navigator.ShowError("项目文件夹打开失败，导航不可用。");
            CurrentSnapshot = null;
            Issues.Clear(IssuesStatusMessages.SkippedProjectFolderOpenFailed);
            OutputText = $"打开 {folderPath} 失败。";
        }
    }

    /// <summary>
    /// Updates the output pane with a short user-facing status message.
    /// </summary>
    public void ShowOutputMessage(string message)
    {
        OutputText = message;
    }

    public void UpdateEditorCaretStatus(int line, int column, int selectedCharacterCount)
    {
        StatusCaretPositionText = $"行 {Math.Max(1, line)}，列 {Math.Max(1, column)}";
        StatusSelectionText = selectedCharacterCount <= 0
            ? "未选择"
            : $"已选择 {selectedCharacterCount} 个字符";
    }

    public void UpdateEditorTextStatus(string currentText)
    {
        StatusNewlineText = $"换行：{FormatNewLine(DetectNewLine(currentText))}";
    }

    public void UpdateDirtyStatus(string stateText)
    {
        StatusDirtyStateText = string.IsNullOrWhiteSpace(stateText)
            ? "无文件"
            : stateText;
    }

    public void SetOperationStatus(string text, string kind = "Info")
    {
        StatusOperationText = string.IsNullOrWhiteSpace(text) ? "就绪" : text.Trim();
        StatusOperationKindText = string.IsNullOrWhiteSpace(kind) ? "Info" : kind.Trim();
    }

    public void ClearIssues()
    {
        Issues.Clear(IssuesStatusMessages.IssuesCleared);
        OutputText = IssuesStatusMessages.IssuesCleared;
    }

    public void ClearIssueFilters()
    {
        Issues.ClearFilters();
        OutputText = "Issue filters cleared.";
    }

    public void RefreshCurrentFileDiagnostics(
        string currentEditorText,
        IRa2FieldDefinitionProvider? fieldProvider = null)
    {
        if (CurrentSnapshot is null)
        {
            Issues.Clear(IssuesStatusMessages.NoFileSelected);
            OutputText = IssuesStatusMessages.NoFileSelected;
            return;
        }

        if (!CurrentSnapshot.CanRunDiagnostics)
        {
            Issues.Clear(IssuesStatusMessages.SkippedSourceNotLoaded);
            OutputText = IssuesStatusMessages.SkippedSourceNotLoaded;
            return;
        }

        CurrentSourceSnapshot diagnosticSnapshot = new(
            _currentProjectRootPath,
            CurrentSnapshot.FilePath,
            CurrentSnapshot.FileName,
            currentEditorText,
            CurrentSnapshot.Version,
            CurrentSnapshot.State,
            CurrentSnapshot.EncodingMetadata);
        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = _diagnosticService.Analyze(diagnosticSnapshot, fieldProvider);
        Issues.ReplaceIssues(issues);
        OutputText = Issues.StatusText;
    }

    public Task RunManualFullDiagnosticsAsync(
        string currentEditorText,
        IRa2FieldDefinitionProvider? fieldProvider = null)
        => RunManualFullDiagnosticsAsync(currentEditorText, fieldProvider, documentOverrides: null);

    internal async Task RunManualFullDiagnosticsAsync(
        string currentEditorText,
        IRa2FieldDefinitionProvider? fieldProvider,
        IReadOnlyDictionary<string, ManualFullDiagnosticsDocumentOverride>? documentOverrides)
    {
        IReadOnlyList<ReadonlyIniFileDescriptor> files = ProjectExplorer.Items
            .Where(item => item.Kind == ProjectExplorerItemKind.File)
            .Select(item => item.ToDescriptor())
            .ToArray();
        if (files.Count == 0)
        {
            Issues.Clear(IssuesStatusMessages.NoFileSelected);
            OutputText = IssuesStatusMessages.NoFileSelected;
            return;
        }

        Issues.Clear(IssuesStatusMessages.ManualFullDiagnosticsPending);
        OutputText = IssuesStatusMessages.ManualFullDiagnosticsPending;
        ManualFullDiagnosticsRequest request = new(
            _currentProjectRootPath,
            files,
            CurrentSnapshot,
            currentEditorText,
            fieldProvider,
            documentOverrides);
        ManualFullDiagnosticsResult result = await Task.Run(() => _manualFullDiagnosticsService.Analyze(request));
        Issues.ReplaceIssues(result.Issues, result.StatusText);
        OutputText = result.StatusText;
    }

    /// <summary>
    /// Toggles the Project Explorer panel visibility without changing tree state.
    /// </summary>
    public void ToggleProjectExplorer()
    {
        IsProjectExplorerVisible = !IsProjectExplorerVisible;
    }

    public async Task LoadProjectExplorerFileAsync(
        ProjectExplorerItemViewModel? selectedItem,
        IRa2FieldDefinitionProvider? fieldProvider = null)
    {
        ProjectExplorer.SelectedItem = selectedItem;
        if (selectedItem is null || selectedItem.Kind != ProjectExplorerItemKind.File)
            return;

        if (CurrentSnapshot is not null &&
            string.Equals(CurrentSnapshot.FilePath, selectedItem.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            ShowOutputMessage($"{selectedItem.DisplayText} 已加载。");
            return;
        }

        await LoadSelectedFileAsync(selectedItem, fieldProvider);
    }

    private async Task LoadSelectedFileAsync(
        ProjectExplorerItemViewModel? selectedFile,
        IRa2FieldDefinitionProvider? fieldProvider)
    {
        int loadVersion = ++_selectedFileLoadVersion;

        if (selectedFile is null)
        {
            SourceEditor.ShowEmptyState("所选文件夹中没有找到 INI 文件。");
            Navigator.ShowEmptyState("所选文件夹中没有找到 INI 文件。");
            CurrentSnapshot = null;
            Issues.Clear(IssuesStatusMessages.NoFileSelected);
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedFile.FilePath))
        {
            ShowOutputMessage("所选项目浏览器节点没有文件路径，无法加载。");
            return;
        }

        string selectedFilePath = selectedFile.FilePath;
        ProjectExplorer.MarkCurrentFile(selectedFilePath);
        SourceEditor.ShowLoading(selectedFile.FileName);
        Navigator.ShowLoading("正在加载当前文件导航...");
        CurrentSnapshot = new CurrentSourceSnapshot(
            _currentProjectRootPath,
            selectedFilePath,
            selectedFile.FileName,
            SourceEditor.Text,
            loadVersion,
            SourceEditor.State);
        Issues.Clear(IssuesStatusMessages.Pending);
        OutputText = $"正在加载 {selectedFile.FileName}...";

        var result = await Task.Run(() => _contentService.ReadFileReadonly(selectedFile.ToDescriptor()));
        if (loadVersion != _selectedFileLoadVersion)
            return;

        if (result.ErrorMessage is not null)
        {
            SourceEditor.ShowError(result.FileName, result.Text);
            Navigator.ShowError("文件读取失败，导航不可用。");
            CurrentSnapshot = new CurrentSourceSnapshot(
                _currentProjectRootPath,
                result.FilePath,
                result.FileName,
                SourceEditor.Text,
                loadVersion,
                SourceEditor.State,
                result.EncodingMetadata);
            ProjectExplorer.ShowPlaceholderForCurrentFile(result.FilePath, "已跳过 Section：文件读取失败。");
            Issues.Clear(IssuesStatusMessages.SkippedReadFailed);
            OutputText = $"读取 {result.FileName} 失败。";
            return;
        }

        if (result.IsLargeFileDeferred)
        {
            SourceEditor.ShowLargeFileDeferred(result.FileName, result.Text, result.MetadataText);
            Navigator.ShowDisabled("大文件已延迟预览，导航暂不可用。");
            CurrentSnapshot = new CurrentSourceSnapshot(
                _currentProjectRootPath,
                result.FilePath,
                result.FileName,
                SourceEditor.Text,
                loadVersion,
                SourceEditor.State,
                result.EncodingMetadata);
            ProjectExplorer.ShowPlaceholderForCurrentFile(result.FilePath, "已跳过 Section：大文件预览已延迟加载。");
            Issues.Clear(IssuesStatusMessages.SkippedDeferredLargeFile);
            OutputText = $"{result.FileName} 过大，已跳过自动预览。";
            return;
        }

        SourceEditor.ShowDocument(result.FileName, result.Text, result.MetadataText);
        CurrentSnapshot = new CurrentSourceSnapshot(
            _currentProjectRootPath,
            result.FilePath,
            result.FileName,
            SourceEditor.Text,
            loadVersion,
            SourceEditor.State,
            result.EncodingMetadata);
        Issues.Clear(IssuesStatusMessages.Pending);
        OutputText = $"已加载 {result.FileName}，正在刷新诊断...";

        await Task.Yield();

        CurrentSourceSnapshot diagnosticSnapshot = CurrentSnapshot;
        try
        {
            IReadOnlyList<IdeDiagnosticIssueViewModel> issues = await Task.Run(() =>
                _diagnosticService.Analyze(diagnosticSnapshot, fieldProvider));
            if (!TryReplaceIssuesForCurrentSnapshot(diagnosticSnapshot, issues))
                OutputText = IssuesStatusMessages.SkippedStaleResult;
        }
        catch (Exception ex)
        {
            if (IsCurrentSnapshot(diagnosticSnapshot))
            {
                Issues.Clear($"当前文件诊断失败：{ex.Message}");
                OutputText = $"当前文件诊断失败：{ex.Message}";
            }
        }

        try
        {
            var groupedSections = await Task.Run(() => _projectExplorerGroupingService.BuildGroups(result.Text));
            if (loadVersion != _selectedFileLoadVersion)
                return;

            if (groupedSections.Count == 0)
            {
                Navigator.ShowEmptyState("当前文件中没有找到 Section。");
                ProjectExplorer.ShowPlaceholderForCurrentFile(result.FilePath, "当前文件中没有找到 Section。");
            }
            else
            {
                ProjectExplorer.ShowGroupedSectionsForCurrentFile(result.FilePath, groupedSections);
            }
        }
        catch (Exception ex)
        {
            if (loadVersion == _selectedFileLoadVersion)
            {
                Navigator.ShowError($"Section 索引构建失败，导航不可用：{ex.Message}");
                ProjectExplorer.ShowPlaceholderForCurrentFile(result.FilePath, "已跳过 Section：分组构建失败。");
            }
        }

        OutputText = $"已加载 {result.FileName}。";
    }

    private static string BuildProjectTitle(string folderPath)
    {
        string title = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return string.IsNullOrWhiteSpace(title) ? folderPath : title;
    }

    private void RefreshStatusFromCurrentSnapshot()
    {
        if (CurrentSnapshot is null)
        {
            StatusCurrentFileText = "未选择文件";
            StatusEncodingText = "编码：-";
            StatusNewlineText = "换行：-";
            return;
        }

        StatusCurrentFileText = CurrentSnapshot.FileName;
        StatusEncodingText = $"编码：{CurrentSnapshot.EncodingMetadata.DisplayName}";
        UpdateEditorTextStatus(CurrentSnapshot.Text);
    }

    private static string DetectNewLine(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        int crlfIndex = text.IndexOf("\r\n", StringComparison.Ordinal);
        int lfIndex = text.IndexOf('\n');
        int crIndex = text.IndexOf('\r');

        if (crlfIndex >= 0 && (lfIndex < 0 || crlfIndex <= lfIndex) && (crIndex < 0 || crlfIndex <= crIndex))
            return "\r\n";

        if (lfIndex >= 0 && (crIndex < 0 || lfIndex < crIndex))
            return "\n";

        return crIndex >= 0 ? "\r" : string.Empty;
    }

    private static string FormatNewLine(string newLine) => newLine switch
    {
        "\r\n" => "CRLF",
        "\n" => "LF",
        "\r" => "CR",
        _ => "-"
    };

    private bool TryReplaceIssuesForCurrentSnapshot(CurrentSourceSnapshot snapshot, IReadOnlyList<IdeDiagnosticIssueViewModel> issues)
    {
        if (!IsCurrentSnapshot(snapshot))
            return false;

        Issues.ReplaceIssues(issues);
        return true;
    }

    private bool IsCurrentSnapshot(CurrentSourceSnapshot snapshot)
    {
        return CurrentSnapshot is not null &&
               CurrentSnapshot.Version == snapshot.Version &&
               string.Equals(CurrentSnapshot.FilePath, snapshot.FilePath, StringComparison.OrdinalIgnoreCase);
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
