using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AI;

/// <summary>协调 provider 工具结果、A3 预览和显式用户确认，不直接访问 WPF 或文件系统。</summary>
internal sealed class Ra2AiAuthoringCoordinator
{
    private readonly Ra2AiAuthoringToolAdapter _adapter;
    private readonly IRa2IniAuthoringWorkspace _workspace;
    private readonly object _proposalGate = new();
    private long _proposalGeneration;
    private Ra2AiEditProposal? _activeProposal;

    public Ra2AiAuthoringCoordinator(
        Ra2AiAuthoringToolAdapter adapter,
        IRa2IniAuthoringWorkspace workspace)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
    }

    internal Ra2AiEditProposal? ActiveProposal
    {
        get
        {
            lock (_proposalGate)
                return _activeProposal;
        }
    }

    public Ra2AiEditProposalResult PrepareProposal(
        Ra2AiAuthoringRequestContext requestContext,
        Ra2AuthoringSnapshot currentSnapshot,
        Ra2AiResponse response,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestContext);
        ArgumentNullException.ThrowIfNull(currentSnapshot);
        ArgumentNullException.ThrowIfNull(response);

        long generation;
        lock (_proposalGate)
        {
            generation = AdvanceGeneration();
            _activeProposal = null;
        }
        _workspace.InvalidateActivePreview();

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (response.Kind != Ra2AiResponseKind.ToolCalls)
        {
            return Failed(
                Ra2AiEditProposalFailureKind.MissingArguments,
                "AI 响应没有包含可预览的结构化修改。");
        }

        if (response.ToolCalls.Count != 1)
        {
            return Failed(
                Ra2AiEditProposalFailureKind.MultipleToolCalls,
                "当前版本一次只接受一项结构化修改建议。");
        }

        if (!SnapshotsMatch(requestContext.Snapshot, currentSnapshot))
        {
            return Failed(
                Ra2AiEditProposalFailureKind.RequestContextStale,
                "请求期间当前文档或字段库已经变化，请重新发送修改请求。");
        }

        Ra2AiEditPlanCreationResult planResult = _adapter.TryCreatePlan(
            response.ToolCalls[0],
            requestContext);
        if (planResult.NeedsClarification)
            return Ra2AiEditProposalResult.Clarification(planResult.Message);
        if (!planResult.Succeeded)
            return Failed(planResult.FailureKind, planResult.Message);

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        Ra2IniEditPreview preview = _workspace.Preview(
            requestContext.Snapshot,
            planResult.Plan!,
            cancellationToken);
        if (!preview.Succeeded)
        {
            return preview.FailureKind == Ra2IniEditPreviewFailureKind.Canceled
                ? Cancelled()
                : Failed(
                    Ra2AiEditProposalFailureKind.PreviewRejected,
                    preview.Message);
        }

        Ra2AiEditProposalApplyPolicy policy = DetermineApplyPolicy(preview);
        Ra2AiEditProposal proposal = new(preview, policy, BuildRiskSummary(preview, policy));
        lock (_proposalGate)
        {
            if (generation != _proposalGeneration)
            {
                _workspace.TryDiscardActivePreview(preview.PreviewId);
                return Failed(
                    Ra2AiEditProposalFailureKind.RequestContextStale,
                    "该修改建议已被更新的请求取代。");
            }

            _activeProposal = proposal;
        }

        return Ra2AiEditProposalResult.FromProposal(proposal);
    }

    public Ra2AiEditProposalApplyResult ApplyConfirmed(Ra2AiEditProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);

        lock (_proposalGate)
        {
            if (_activeProposal is null ||
                _activeProposal.ProposalId != proposal.ProposalId ||
                _activeProposal.Preview.PreviewId != proposal.Preview.PreviewId)
            {
                return Ra2AiEditProposalApplyResult.Failed(
                    Ra2AiEditProposalFailureKind.RequestContextStale,
                    "该修改建议已失效、已处理或已被更新建议取代。");
            }

            if (proposal.ApplyPolicy == Ra2AiEditProposalApplyPolicy.Blocked)
            {
                return Ra2AiEditProposalApplyResult.Failed(
                    Ra2AiEditProposalFailureKind.ApplyBlocked,
                    "该建议会新增错误，当前版本禁止应用。");
            }

            _activeProposal = null;
            AdvanceGeneration();
        }

        Ra2IniEditApplyResult result = _workspace.Apply(new Ra2IniEditApplyRequest(
            proposal.Preview.PreviewId,
            explicitConfirmationGranted: true));
        return result.Succeeded
            ? Ra2AiEditProposalApplyResult.Applied(result)
            : Ra2AiEditProposalApplyResult.Failed(
                result.OutcomeKind == Ra2IniEditApplyOutcomeKind.StalePreview
                    ? Ra2AiEditProposalFailureKind.RequestContextStale
                    : Ra2AiEditProposalFailureKind.PreviewRejected,
                result.Message,
                result);
    }

    public bool Dismiss(Ra2AiEditProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);

        lock (_proposalGate)
        {
            if (_activeProposal is null ||
                _activeProposal.ProposalId != proposal.ProposalId)
            {
                return false;
            }

            _activeProposal = null;
            AdvanceGeneration();
        }

        return _workspace.TryDiscardActivePreview(proposal.Preview.PreviewId);
    }

    public Ra2AiEditProposal? InvalidateActiveProposal()
    {
        Ra2AiEditProposal? invalidated;
        lock (_proposalGate)
        {
            invalidated = _activeProposal;
            _activeProposal = null;
            AdvanceGeneration();
        }

        _workspace.InvalidateActivePreview();
        return invalidated;
    }

    private static bool SnapshotsMatch(
        Ra2AuthoringSnapshot request,
        Ra2AuthoringSnapshot current)
        => request.DocumentId == current.DocumentId &&
           request.EditRevision == current.EditRevision &&
           request.FieldRegistry.Revision == current.FieldRegistry.Revision &&
           string.Equals(request.Text, current.Text, StringComparison.Ordinal);

    private static Ra2AiEditProposalApplyPolicy DetermineApplyPolicy(
        Ra2IniEditPreview preview)
    {
        if (preview.AddedErrorCount > 0)
            return Ra2AiEditProposalApplyPolicy.Blocked;
        if (preview.AddedWarningCount > 0 ||
            preview.SectionCreationPreviews.Any(section =>
                section.AuthoringDisposition != Ra2AutomationFieldAuthoringDisposition.Normal) ||
            preview.OperationPreviews.Any(operation =>
                !operation.IsKnownField ||
                operation.FieldTrustLevel is not (
                    Ra2FieldTrustLevel.Verified or
                    Ra2FieldTrustLevel.ManualCurated)))
        {
            return Ra2AiEditProposalApplyPolicy.Caution;
        }

        return Ra2AiEditProposalApplyPolicy.Normal;
    }

    private static string BuildRiskSummary(
        Ra2IniEditPreview preview,
        Ra2AiEditProposalApplyPolicy policy)
        => policy switch
        {
            Ra2AiEditProposalApplyPolicy.Blocked =>
                $"阻止应用：候选内容新增 {preview.AddedErrorCount} 个错误。",
            Ra2AiEditProposalApplyPolicy.Caution =>
                $"需要复核：新增 {preview.AddedWarningCount} 个警告，或包含未完全核验的字段/Section 分类。",
            _ => "未发现新增错误、警告或字段可信度风险。"
        };

    private long AdvanceGeneration()
        => _proposalGeneration = unchecked(_proposalGeneration + 1);

    private static Ra2AiEditProposalResult Cancelled()
        => Failed(
            Ra2AiEditProposalFailureKind.PreviewCancelled,
            "已取消生成结构化修改预览。");

    private static Ra2AiEditProposalResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
        => Ra2AiEditProposalResult.Failed(failureKind, message);
}
