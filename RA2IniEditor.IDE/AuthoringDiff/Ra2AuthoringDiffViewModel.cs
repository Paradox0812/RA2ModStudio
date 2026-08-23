using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.ViewModels.AI;

namespace RA2IniEditor.IDE.AuthoringDiff;

internal sealed class Ra2AuthoringDiffViewModel : INotifyPropertyChanged
{
    private readonly Ra2AuthoringDiffProjectionBuilder _builder = new();
    private IReadOnlyList<Ra2AuthoringDiffRow> _rows = [];
    private string _statusText = "正在生成差异预览…";
    private string _statsText = "正在计算";
    private bool _isLoading = true;
    private bool _projectionSucceeded;

    public Ra2AuthoringDiffViewModel(Ra2AiEditProposalViewModel proposalViewModel)
    {
        ProposalViewModel = proposalViewModel ?? throw new ArgumentNullException(nameof(proposalViewModel));
        ProposalViewModel.PropertyChanged += ProposalViewModel_OnPropertyChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public Ra2AiEditProposalViewModel ProposalViewModel { get; }
    public Ra2AiEditProposal Proposal => ProposalViewModel.Proposal;
    public string Title => $"修改预览：{Path.GetFileName(Proposal.Preview.Snapshot.FilePath)}";
    public string StatusText => _statusText;
    public string StatsText => _statsText;
    public string DiagnosticSummary => $"错误 {Proposal.Preview.AddedErrorCount} · 警告 {Proposal.Preview.AddedWarningCount}";
    public IReadOnlyList<Ra2AuthoringDiffRow> Rows => _rows;
    public bool IsLoading => _isLoading;
    public bool IsApplyEnabled => _projectionSucceeded && ProposalViewModel.IsApplyEnabled;
    public bool IsDismissEnabled => ProposalViewModel.IsDismissEnabled;
    public bool IsBlocked => ProposalViewModel.State == Ra2AiEditProposalState.Blocked;
    public bool IsStale => ProposalViewModel.State is Ra2AiEditProposalState.Stale or
        Ra2AiEditProposalState.Superseded or Ra2AiEditProposalState.Dismissed or
        Ra2AiEditProposalState.Applied or Ra2AiEditProposalState.Failed;

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        Ra2AuthoringDiffProjection projection = await Task.Run(
            () => _builder.Build(Proposal.Preview, cancellationToken), cancellationToken);
        _rows = projection.Rows;
        _projectionSucceeded = projection.Succeeded;
        _isLoading = false;
        _statusText = projection.Succeeded ? "AI 修改预览" : projection.Message;
        _statsText = projection.Succeeded
            ? $"{Proposal.Preview.Plan.SectionCreations.Count + Proposal.Preview.Plan.Operations.Count} 项更改  +{projection.AddedLineCount} / -{projection.RemovedLineCount}  {projection.HunkCount} 个差异块"
            : "差异不可用；仍可在 AI 卡片应用整份提案";
        RaiseAll();
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
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
