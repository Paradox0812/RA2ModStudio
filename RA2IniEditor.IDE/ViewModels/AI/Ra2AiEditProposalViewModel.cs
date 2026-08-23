using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.ViewModels.AI;

internal sealed class Ra2AiEditProposalOperationViewModel
{
    public Ra2AiEditProposalOperationViewModel(
        string actionText,
        string targetText,
        string changeText,
        string evidenceText)
    {
        ActionText = actionText;
        TargetText = targetText;
        ChangeText = changeText;
        EvidenceText = evidenceText;
    }

    public string ActionText { get; }

    public string TargetText { get; }

    public string ChangeText { get; }

    public string EvidenceText { get; }
}

/// <summary>承载建议卡展示状态；编辑权威仍属于 Ra2AiAuthoringCoordinator。</summary>
internal sealed class Ra2AiEditProposalViewModel : INotifyPropertyChanged
{
    private Ra2AiEditProposalState _state;
    private string _resultMessage;

    public Ra2AiEditProposalViewModel(Ra2AiEditProposal proposal)
    {
        Proposal = proposal ?? throw new ArgumentNullException(nameof(proposal));
        _state = proposal.ApplyPolicy == Ra2AiEditProposalApplyPolicy.Blocked
            ? Ra2AiEditProposalState.Blocked
            : Ra2AiEditProposalState.Ready;
        _resultMessage = proposal.RiskSummary;
        Operations = Array.AsReadOnly(
            proposal.Preview.SectionCreationPreviews
                .Select(CreateSectionCreation)
                .Concat(proposal.Preview.OperationPreviews.Select(operation =>
                    CreateOperation(operation, proposal.Preview.Snapshot.Text)))
                .ToArray());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Ra2AiEditProposal Proposal { get; }

    public string Title => "建议修改当前文件";

    public string Summary => Proposal.Preview.Plan.Summary;

    public string RiskSummary => Proposal.RiskSummary;

    public IReadOnlyList<Ra2AiEditProposalOperationViewModel> Operations { get; }

    public Ra2AiEditProposalState State => _state;

    public string StatusText => _state switch
    {
        Ra2AiEditProposalState.Preparing => "正在生成预览",
        Ra2AiEditProposalState.Ready => Proposal.ApplyPolicy ==
                                        Ra2AiEditProposalApplyPolicy.Caution
            ? "需要复核"
            : "可应用",
        Ra2AiEditProposalState.Applying => "正在应用",
        Ra2AiEditProposalState.Applied => "已应用过",
        Ra2AiEditProposalState.Blocked => "已阻止",
        Ra2AiEditProposalState.Stale => "已失效",
        Ra2AiEditProposalState.Superseded => "已被取代",
        Ra2AiEditProposalState.Dismissed => "已忽略",
        _ => "失败"
    };

    public string ApplyButtonText
        => Proposal.ApplyPolicy == Ra2AiEditProposalApplyPolicy.Caution
            ? "仍要应用"
            : "应用到当前文件";

    public bool IsApplyEnabled
        => _state == Ra2AiEditProposalState.Ready &&
           Proposal.ApplyPolicy != Ra2AiEditProposalApplyPolicy.Blocked;

    public bool IsDismissEnabled
        => _state is Ra2AiEditProposalState.Ready or
            Ra2AiEditProposalState.Blocked;

    public string ResultMessage => _resultMessage;

    public void BeginApply()
        => SetState(Ra2AiEditProposalState.Applying, "正在通过编辑器事务应用建议……");

    public void MarkApplied(string message)
        => SetState(
            Ra2AiEditProposalState.Applied,
            string.IsNullOrWhiteSpace(message)
                ? "已应用到内存，尚未保存；可使用 Ctrl+Z 撤销。"
                : $"{message} 可使用 Ctrl+Z 撤销。");

    public void MarkFailed(string message)
        => SetState(
            Ra2AiEditProposalState.Failed,
            NormalizeMessage(message, "无法应用该修改建议。"));

    public void MarkStale(string message)
        => SetState(
            Ra2AiEditProposalState.Stale,
            NormalizeMessage(message, "文档已经变化，请重新发送修改请求。"));

    public void MarkSuperseded()
        => SetState(
            Ra2AiEditProposalState.Superseded,
            "该建议已被更新的结构化修改建议取代。");

    public void MarkDismissed()
        => SetState(Ra2AiEditProposalState.Dismissed, "已忽略该修改建议。");

    private void SetState(Ra2AiEditProposalState state, string message)
    {
        if (_state == state && string.Equals(_resultMessage, message, StringComparison.Ordinal))
            return;

        _state = state;
        _resultMessage = message;
        OnPropertyChanged(nameof(State));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsApplyEnabled));
        OnPropertyChanged(nameof(IsDismissEnabled));
        OnPropertyChanged(nameof(ResultMessage));
    }

    private static Ra2AiEditProposalOperationViewModel CreateOperation(
        Ra2IniEditOperationPreview preview,
        string sourceText)
    {
        Ra2IniEditOperation operation = preview.Operation;
        string action = preview.OutcomeKind == Ra2IniEditOperationOutcomeKind.Inserted
            ? "新增"
            : "替换";
        string oldValue = preview.OutcomeKind == Ra2IniEditOperationOutcomeKind.Inserted
            ? "（不存在）"
            : ReadOriginalValue(preview, sourceText);
        string change = preview.OutcomeKind == Ra2IniEditOperationOutcomeKind.Inserted
            ? operation.Value
            : $"{oldValue}  →  {operation.Value}";
        string evidence = preview.IsKnownField
            ? $"{preview.ResolvedSectionKind} · {preview.FieldTrustLevel}"
            : $"{preview.ResolvedSectionKind} · 未知字段";
        return new Ra2AiEditProposalOperationViewModel(
            action,
            $"[{operation.SectionName}] {operation.Key}",
            change,
            evidence);
    }

    private static Ra2AiEditProposalOperationViewModel CreateSectionCreation(
        Ra2AutomationSectionCreatePreview preview)
    {
        string evidence = preview.IsClassificationResolved
            ? $"实际分类：{preview.ActualSectionKind}"
            : $"预期分类：{preview.Operation.ExpectedSectionKind} · 当前尚未由文档引用解析";
        return new Ra2AiEditProposalOperationViewModel(
            "创建 Section",
            $"[{preview.Operation.SectionName}]",
            $"新增空 Section（{preview.Operation.ExpectedSectionKind}）",
            evidence);
    }

    internal static string ReadOriginalValue(
        Ra2IniEditOperationPreview preview,
        string sourceText)
    {
        int start = preview.AffectedOriginalSpan.Start;
        int length = preview.AffectedOriginalSpan.Length;
        if (length == 0)
            return "（空值）";
        if (start < 0 || length < 0 || start > sourceText.Length - length)
            return "（无法读取原值）";

        return sourceText.Substring(start, length);
    }

    private static string NormalizeMessage(string? message, string fallback)
        => string.IsNullOrWhiteSpace(message) ? fallback : message.Trim();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
