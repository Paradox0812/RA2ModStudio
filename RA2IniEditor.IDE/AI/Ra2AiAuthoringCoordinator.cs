using RA2IniEditor.IDE.Editing;
using System.IO;

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
        => PrepareProposal(
            requestContext,
            new Ra2AiAuthoringRequestContext(currentSnapshot),
            response,
            cancellationToken);

    public Ra2AiEditProposalResult PrepareProposal(
        Ra2AiAuthoringRequestContext requestContext,
        Ra2AiAuthoringRequestContext currentContext,
        Ra2AiResponse response,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestContext);
        ArgumentNullException.ThrowIfNull(currentContext);
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
            const string message = "AI 响应没有包含可预览的结构化修改。";
            return Failed(
                Ra2AiEditProposalFailureKind.MissingArguments,
                message,
                Ra2AiStructuredFailureEvidence.FromResponse(
                    Ra2AiEditProposalFailureKind.MissingArguments,
                    message,
                    response.Text));
        }

        if (response.ToolCalls.Count != 1)
        {
            const string message = "当前版本一次只接受一项结构化修改建议。";
            return Failed(
                Ra2AiEditProposalFailureKind.MultipleToolCalls,
                message,
                Ra2AiStructuredFailureEvidence.FromAdapter(
                    Ra2AiEditProposalFailureKind.MultipleToolCalls,
                    message));
        }

        if (!Ra2AiAuthoringContextCurrency.Matches(requestContext, currentContext))
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
        {
            Ra2AiStructuredFailureEvidence? evidence = planResult.FailureEvidence?.WithTool(response.ToolCalls[0]);
            return Failed(planResult.FailureKind, planResult.Message, evidence);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (requestContext.Scope == Ra2AiAuthoringScope.Project)
        {
            Ra2ProjectEditPreview projectPreview = _workspace.PreviewProject(
                requestContext.ProjectSnapshot!,
                planResult.ProjectPlan!,
                cancellationToken);
            if (!projectPreview.Succeeded)
            {
                string message = FormatProjectPreviewFailure(projectPreview);
                Ra2AutomationProjectEditPreviewResult result = projectPreview.AutomationResult;
                return projectPreview.AutomationResult.FailureKind == Ra2AutomationProjectEditPreviewFailureKind.Canceled
                    ? Cancelled()
                    : Failed(
                        Ra2AiEditProposalFailureKind.PreviewRejected,
                        message,
                        Ra2AiStructuredFailureEvidence.FromProjectPreview(
                            result.FailureKind,
                            result.FailureKind == Ra2AutomationProjectEditPreviewFailureKind.DocumentPreviewFailed
                                ? result.FailedDocumentFailureKind
                                : null,
                            message).WithTool(response.ToolCalls[0]));
            }

            Ra2AiEditProposalApplyPolicy projectPolicy = DetermineApplyPolicy(projectPreview);
            Ra2AiEditProposal projectProposal = Ra2AiEditProposal.FromProject(
                projectPreview,
                planResult.AssetManifest,
                projectPolicy,
                BuildRiskSummary(projectPreview, projectPolicy));
            lock (_proposalGate)
            {
                if (generation != _proposalGeneration)
                {
                    _workspace.TryDiscardActiveProjectPreview(projectPreview.ProjectPreviewId);
                    return Failed(Ra2AiEditProposalFailureKind.RequestContextStale, "该项目修改建议已被更新的请求取代。");
                }
                _activeProposal = projectProposal;
            }
            return Ra2AiEditProposalResult.FromProposal(projectProposal);
        }

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
                    preview.Message,
                    Ra2AiStructuredFailureEvidence.FromDocumentPreview(
                        preview.FailureKind,
                        preview.Message).WithTool(response.ToolCalls[0]));
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
                !ProposalPreviewMatches(_activeProposal, proposal))
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

        if (proposal.Scope == Ra2AiAuthoringScope.Project)
        {
            Ra2ProjectEditApplyResult projectResult = _workspace.ApplyProject(new Ra2ProjectEditApplyRequest(
                proposal.ProjectPreview.ProjectPreviewId,
                ExplicitConfirmationGranted: true));
            return projectResult.Succeeded
                ? Ra2AiEditProposalApplyResult.Applied(projectResult)
                : Ra2AiEditProposalApplyResult.Failed(
                    projectResult.OutcomeKind == Ra2ProjectEditApplyOutcomeKind.Stale
                        ? Ra2AiEditProposalFailureKind.RequestContextStale
                        : Ra2AiEditProposalFailureKind.PreviewRejected,
                    projectResult.Message,
                    projectAuthoringResult: projectResult);
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

        return proposal.Scope == Ra2AiAuthoringScope.Project
            ? _workspace.TryDiscardActiveProjectPreview(proposal.ProjectPreview.ProjectPreviewId)
            : _workspace.TryDiscardActivePreview(proposal.Preview.PreviewId);
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

    private static bool ProposalPreviewMatches(Ra2AiEditProposal active, Ra2AiEditProposal candidate)
        => active.Scope == candidate.Scope &&
           (active.Scope == Ra2AiAuthoringScope.Project
               ? active.ProjectPreview.ProjectPreviewId == candidate.ProjectPreview.ProjectPreviewId
               : active.Preview.PreviewId == candidate.Preview.PreviewId);

    private static Ra2AiEditProposalApplyPolicy DetermineApplyPolicy(
        Ra2IniEditPreview preview)
    {
        if (preview.AddedErrorCount > 0 ||
            preview.AddedWarningCount > 0 ||
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

    private static Ra2AiEditProposalApplyPolicy DetermineApplyPolicy(Ra2ProjectEditPreview preview)
    {
        if (preview.DocumentPreviews.Any(document =>
                document.AddedErrorCount > 0 ||
                document.AddedWarningCount > 0 ||
                document.SectionCreationPreviews.Any(section =>
                    section.AuthoringDisposition != Ra2AutomationFieldAuthoringDisposition.Normal) ||
                document.OperationPreviews.Any(operation =>
                    !operation.IsKnownField ||
                    operation.FieldTrustLevel is not (
                        Ra2AutomationFieldTrustLevel.Verified or
                        Ra2AutomationFieldTrustLevel.ManualCurated))))
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
            Ra2AiEditProposalApplyPolicy.Caution =>
                $"需要复核：新增错误 {preview.AddedErrorCount}、警告 {preview.AddedWarningCount}，或包含未完全核验的字段/Section 分类；这些诊断不阻止显式应用。",
            _ => "未发现新增错误、警告或字段可信度风险。"
        };

    private static string BuildRiskSummary(
        Ra2ProjectEditPreview preview,
        Ra2AiEditProposalApplyPolicy policy)
    {
        int errors = preview.DocumentPreviews.Sum(document => document.AddedErrorCount);
        int warnings = preview.DocumentPreviews.Sum(document => document.AddedWarningCount);
        return policy switch
        {
            Ra2AiEditProposalApplyPolicy.Blocked => $"阻止应用：项目候选内容违反最低结构安全界限（错误 {errors}）。",
            Ra2AiEditProposalApplyPolicy.Caution => $"建议复核：项目候选内容新增错误 {errors}、警告 {warnings}，或包含未核验字段/Section；这些诊断不阻止显式应用。",
            _ => "两个 INI 候选均未发现新增错误、警告或字段可信度风险。"
        };
    }

    private static string FormatProjectPreviewFailure(Ra2ProjectEditPreview preview)
    {
        Ra2AutomationProjectEditPreviewResult result = preview.AutomationResult;
        if (result.FailureKind != Ra2AutomationProjectEditPreviewFailureKind.DocumentPreviewFailed ||
            result.FailedDocumentFailureKind != Ra2AutomationEditPreviewFailureKind.SectionNotFound ||
            result.FailedDocumentId is not Guid failedDocumentId)
        {
            return result.Message;
        }

        Ra2AutomationDocumentSnapshot? failedDocument = preview.Snapshot.Documents
            .SingleOrDefault(document => document.DocumentId == failedDocumentId);
        Ra2AutomationEditPlan? failedPlan = preview.Plan.DocumentPlans
            .SingleOrDefault(plan => plan.ExpectedDocumentId == failedDocumentId);
        if (failedDocument is null || failedPlan is null)
            return result.Message;

        HashSet<string> failedDocumentSections = GetSectionNames(failedDocument.Text);
        string[] missingSections = failedPlan.Operations
            .Select(operation => operation.SectionName)
            .Where(section => !failedDocumentSections.Contains(section))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        string failedFileName = Path.GetFileName(failedDocument.FilePath);
        if (missingSections.Length != 1)
            return $"在 {failedFileName} 中未找到模型计划指定的目标 Section；本次未应用。";

        string missingSection = missingSections[0];
        Ra2AutomationDocumentSnapshot[] otherMatches = preview.Snapshot.Documents
            .Where(document => document.DocumentId != failedDocumentId)
            .Where(document => GetSectionNames(document.Text).Contains(missingSection))
            .ToArray();
        return otherMatches.Length == 1
            ? $"模型计划选择了错误的文档目标：在 {failedFileName} 中未找到 [{missingSection}]，但该 Section 存在于 {Path.GetFileName(otherMatches[0].FilePath)}；本次未应用。"
            : $"在 {failedFileName} 中未找到目标 Section [{missingSection}]；本次未应用。";
    }

    private static HashSet<string> GetSectionNames(string text)
        => new Ra2IniTextDocumentParser()
            .Parse(text)
            .SectionHeaders
            .Select(header => header.SectionName)
            .Where(section => !string.IsNullOrWhiteSpace(section))
            .Select(section => section!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private long AdvanceGeneration()
        => _proposalGeneration = unchecked(_proposalGeneration + 1);

    private static Ra2AiEditProposalResult Cancelled()
        => Failed(
            Ra2AiEditProposalFailureKind.PreviewCancelled,
            "已取消生成结构化修改预览。");

    private static Ra2AiEditProposalResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        Ra2AiStructuredFailureEvidence? failureEvidence = null)
        => Ra2AiEditProposalResult.Failed(failureKind, message, failureEvidence);
}
