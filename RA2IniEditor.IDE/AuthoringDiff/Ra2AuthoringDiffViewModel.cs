using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.ViewModels.AI;

namespace RA2IniEditor.IDE.AuthoringDiff;

internal sealed class Ra2AuthoringDiffViewModel : INotifyPropertyChanged
{
    private readonly Ra2AuthoringReviewProjectionBuilder _builder = new();
    private IReadOnlyList<Ra2AuthoringDiffRow> _rows = [];
    private IReadOnlyList<Ra2AuthoringReviewDocument> _documents = [];
    private Ra2AuthoringReviewDocument? _selectedDocument;
    private Ra2AuthoringReviewOutlineItem? _selectedOutlineItem;
    private Ra2AuthoringReviewMode _mode = Ra2AuthoringReviewMode.Result;
    private string _statusText = "正在生成差异预览…";
    private string _statsText = "正在计算";
    private bool _isLoading = true;
    private bool _reviewSucceeded;
    private bool _showRelativeFilePaths = true;

    public Ra2AuthoringDiffViewModel(Ra2AiEditProposalViewModel proposalViewModel)
    {
        ProposalViewModel = proposalViewModel ?? throw new ArgumentNullException(nameof(proposalViewModel));
        ProposalViewModel.PropertyChanged += ProposalViewModel_OnPropertyChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public Ra2AiEditProposalViewModel ProposalViewModel { get; }
    public Ra2AiEditProposal Proposal => ProposalViewModel.Proposal;
    public string Title => Proposal.Scope == Ra2AiAuthoringScope.Project
        ? $"项目修改预览：{string.Join(" + ", Proposal.ProjectPreview.DocumentPreviews.Select(item => Path.GetFileName(item.FilePath)))}"
        : $"修改预览：{Path.GetFileName(Proposal.Preview.Snapshot.FilePath)}";
    public string StatusText => _statusText;
    public string StatsText => _statsText;
    public string DiagnosticSummary => Proposal.Scope == Ra2AiAuthoringScope.Project
        ? $"错误 {Proposal.ProjectPreview.DocumentPreviews.Sum(item => item.AddedErrorCount)} · 警告 {Proposal.ProjectPreview.DocumentPreviews.Sum(item => item.AddedWarningCount)}"
        : $"错误 {Proposal.Preview.AddedErrorCount} · 警告 {Proposal.Preview.AddedWarningCount}";
    public IReadOnlyList<Ra2AuthoringDiffRow> Rows => _rows;
    public IReadOnlyList<Ra2AuthoringReviewDocument> Documents => _documents;
    public Ra2AuthoringReviewDocument? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (ReferenceEquals(_selectedDocument, value))
                return;
            _selectedDocument = value;
            _selectedOutlineItem = value?.OutlineItems.FirstOrDefault(item => item.IsExecutableChange) ?? value?.OutlineItems.FirstOrDefault();
            OnPropertyChanged();
            RaiseDocumentState();
        }
    }
    public IReadOnlyList<Ra2AuthoringReviewOutlineItem> OutlineItems => SelectedDocument?.OutlineItems ?? [];
    public Ra2AuthoringReviewOutlineItem? SelectedOutlineItem
    {
        get => _selectedOutlineItem;
        set
        {
            if (ReferenceEquals(_selectedOutlineItem, value))
                return;
            _selectedOutlineItem = value;
            if (value?.Kind is Ra2AuthoringReviewOutlineKind.Related or Ra2AuthoringReviewOutlineKind.Unresolved)
                _mode = Ra2AuthoringReviewMode.ObjectContext;
            else if (value is not null)
                _mode = Ra2AuthoringReviewMode.Result;
            OnPropertyChanged();
            RaiseModeState();
        }
    }
    public Ra2AuthoringReviewMode Mode => _mode;
    public string ResultText => SelectedDocument?.CandidateText ?? string.Empty;
    public string ContextText => SelectedOutlineItem?.ContextText ?? string.Empty;
    public string ContextTitle => SelectedOutlineItem is null
        ? "请选择关联 Section"
        : $"{SelectedOutlineItem.ContextFileName ?? SelectedOutlineItem.FileName} · [{SelectedOutlineItem.SectionName}]";
    public string RelationMessage => SelectedDocument?.RelationMessage ?? "未能建立可靠的直接引用上下文。";
    public bool IsResultMode => _mode == Ra2AuthoringReviewMode.Result;
    public bool IsChangesMode => _mode == Ra2AuthoringReviewMode.Changes;
    public bool IsObjectContextMode => _mode == Ra2AuthoringReviewMode.ObjectContext;
    public bool HasMultipleDocuments => _documents.Count > 1;
    public bool HasChangedLocations => SelectedDocument?.ChangedLocations.Count > 0;
    public bool IsLoading => _isLoading;
    public bool IsApplyEnabled => _reviewSucceeded && ProposalViewModel.IsApplyEnabled;
    public bool IsDismissEnabled => ProposalViewModel.IsDismissEnabled;
    public bool IsBlocked => ProposalViewModel.State == Ra2AiEditProposalState.Blocked;
    public bool IsStale => ProposalViewModel.State is Ra2AiEditProposalState.Stale or
        Ra2AiEditProposalState.Superseded or Ra2AiEditProposalState.Dismissed or
        Ra2AiEditProposalState.Applied or Ra2AiEditProposalState.Failed;
    public bool ShowRelativeFilePaths => _showRelativeFilePaths;

    internal void SetCompactLayout(bool isCompact)
    {
        bool show = !isCompact;
        if (_showRelativeFilePaths == show)
            return;
        _showRelativeFilePaths = show;
        OnPropertyChanged(nameof(ShowRelativeFilePaths));
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        Ra2AuthoringReviewProjection projection = await Task.Run(
            () => _builder.Build(Proposal, cancellationToken), cancellationToken);
        _rows = projection.Diff.Rows;
        _documents = projection.Documents;
        _selectedDocument = _documents.FirstOrDefault(document => document.ChangedLocations.Count > 0) ?? _documents.FirstOrDefault();
        _selectedOutlineItem = _selectedDocument?.OutlineItems.FirstOrDefault(item => item.IsExecutableChange) ?? _selectedDocument?.OutlineItems.FirstOrDefault();
        _reviewSucceeded = projection.Succeeded;
        _isLoading = false;
        _statusText = projection.Succeeded
            ? Proposal.Scope == Ra2AiAuthoringScope.Project ? "AI 项目修改预览" : "AI 修改预览"
            : projection.Message;
        _statsText = projection.Succeeded && projection.Diff.Succeeded
            ? Proposal.Scope == Ra2AiAuthoringScope.Project
                ? $"{Proposal.ProjectPreview.DocumentPreviews.Count} 个文件 · {Proposal.ProjectPreview.AutomationResult.TotalOperationCount + Proposal.ProjectPreview.AutomationResult.TotalSectionCreationCount} 项更改  +{projection.Diff.AddedLineCount} / -{projection.Diff.RemovedLineCount}  {projection.Diff.HunkCount} 个差异块"
                : $"{Proposal.Preview.Plan.SectionCreations.Count + Proposal.Preview.Plan.Operations.Count} 项更改  +{projection.Diff.AddedLineCount} / -{projection.Diff.RemovedLineCount}  {projection.Diff.HunkCount} 个差异块"
            : projection.Succeeded ? "结果可用；差异视图受资源上限限制" : "审阅不可用；仍可在 AI 卡片处理提案";
        RaiseAll();
    }

    internal void SetMode(Ra2AuthoringReviewMode mode)
    {
        if (!Enum.IsDefined(mode) || _mode == mode)
            return;
        _mode = mode;
        RaiseModeState();
    }

    internal Ra2AuthoringReviewChangeLocation? MoveChange(int delta)
    {
        if (SelectedDocument?.ChangedLocations.Count is not > 0)
            return null;
        IReadOnlyList<Ra2AuthoringReviewChangeLocation> locations = SelectedDocument.ChangedLocations;
        int current = SelectedOutlineItem is null
            ? -1
            : FindNearestLocationIndex(locations, SelectedOutlineItem.CandidateOffset);
        int next = (current + delta) % locations.Count;
        if (next < 0)
            next += locations.Count;
        Ra2AuthoringReviewChangeLocation location = locations[next];
        _selectedOutlineItem = SelectedDocument.OutlineItems
            .Where(item => item.IsExecutableChange)
            .OrderBy(item => Math.Abs((long)item.CandidateOffset - location.CandidateOffset))
            .FirstOrDefault() ?? _selectedOutlineItem;
        _mode = Ra2AuthoringReviewMode.Result;
        OnPropertyChanged(nameof(SelectedOutlineItem));
        RaiseModeState();
        return location;
    }

    public void Dispose() => ProposalViewModel.PropertyChanged -= ProposalViewModel_OnPropertyChanged;

    private void ProposalViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Ra2AiEditProposalViewModel.State) or nameof(Ra2AiEditProposalViewModel.IsApplyEnabled) or nameof(Ra2AiEditProposalViewModel.IsDismissEnabled))
        {
            OnPropertyChanged(nameof(IsApplyEnabled));
            OnPropertyChanged(nameof(IsDismissEnabled));
            OnPropertyChanged(nameof(IsBlocked));
            OnPropertyChanged(nameof(IsStale));
        }
    }

    private void RaiseAll()
    {
        OnPropertyChanged(nameof(Rows)); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(StatsText));
        OnPropertyChanged(nameof(IsLoading)); OnPropertyChanged(nameof(IsApplyEnabled));
        OnPropertyChanged(nameof(Documents)); OnPropertyChanged(nameof(SelectedDocument));
        OnPropertyChanged(nameof(HasMultipleDocuments));
        RaiseDocumentState();
    }

    private void RaiseDocumentState()
    {
        OnPropertyChanged(nameof(OutlineItems)); OnPropertyChanged(nameof(SelectedOutlineItem));
        OnPropertyChanged(nameof(ResultText)); OnPropertyChanged(nameof(ContextText));
        OnPropertyChanged(nameof(ContextTitle)); OnPropertyChanged(nameof(RelationMessage));
        OnPropertyChanged(nameof(HasChangedLocations));
    }

    private void RaiseModeState()
    {
        OnPropertyChanged(nameof(Mode)); OnPropertyChanged(nameof(IsResultMode));
        OnPropertyChanged(nameof(IsChangesMode)); OnPropertyChanged(nameof(IsObjectContextMode));
        OnPropertyChanged(nameof(ContextText)); OnPropertyChanged(nameof(ContextTitle));
    }

    private static int FindNearestLocationIndex(IReadOnlyList<Ra2AuthoringReviewChangeLocation> locations, int offset)
    {
        int best = 0;
        long bestDistance = long.MaxValue;
        for (int index = 0; index < locations.Count; index++)
        {
            long distance = Math.Abs((long)locations[index].CandidateOffset - offset);
            if (distance >= bestDistance)
                continue;
            best = index;
            bestDistance = distance;
        }
        return best;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
