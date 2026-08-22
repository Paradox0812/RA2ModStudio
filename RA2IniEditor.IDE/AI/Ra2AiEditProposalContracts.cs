using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiEditProposalFailureKind
{
    None = 0,
    UnsupportedTool,
    MultipleToolCalls,
    MissingArguments,
    InvalidArgumentsJson,
    UnknownArgumentProperty,
    DuplicateArgumentProperty,
    InvalidOperation,
    RequestContextUnavailable,
    RequestContextStale,
    PreviewRejected,
    PreviewCancelled,
    ApplyBlocked,
    UnexpectedFailure
}

internal enum Ra2AiToolAdaptationOutcomeKind
{
    Proposal = 0,
    NeedsClarification,
    Failed
}

internal enum Ra2AiEditProposalApplyPolicy
{
    Normal = 0,
    Caution,
    Blocked
}

internal enum Ra2AiEditProposalState
{
    Preparing = 0,
    Ready,
    Applying,
    Applied,
    Blocked,
    Stale,
    Superseded,
    Dismissed,
    Failed
}

/// <summary>绑定一次 provider 请求开始时的本地创作快照。</summary>
internal sealed class Ra2AiAuthoringRequestContext
{
    public Ra2AiAuthoringRequestContext(Ra2AuthoringSnapshot snapshot)
        => Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

    public Ra2AuthoringSnapshot Snapshot { get; }
}

internal sealed class Ra2AiEditPlanCreationResult
{
    private Ra2AiEditPlanCreationResult(
        Ra2AiToolAdaptationOutcomeKind outcomeKind,
        Ra2IniEditPlan? plan,
        Ra2AiEditProposalFailureKind failureKind,
        string message)
    {
        bool isProposal = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;
        bool isClarification = outcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;
        bool isFailed = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Failed;
        if (!Enum.IsDefined(outcomeKind) ||
            isProposal != (plan is not null) ||
            isFailed != (failureKind != Ra2AiEditProposalFailureKind.None) ||
            isClarification && (plan is not null || failureKind != Ra2AiEditProposalFailureKind.None))
        {
            throw new ArgumentException("Plan creation result state is inconsistent.");
        }
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Plan creation result message is required.", nameof(message));

        OutcomeKind = outcomeKind;
        Plan = plan;
        FailureKind = failureKind;
        Message = message;
    }

    public bool Succeeded => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;

    public bool NeedsClarification => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;

    public Ra2AiToolAdaptationOutcomeKind OutcomeKind { get; }

    public Ra2IniEditPlan? Plan { get; }

    public Ra2AiEditProposalFailureKind FailureKind { get; }

    public string Message { get; }

    public static Ra2AiEditPlanCreationResult FromPlan(Ra2IniEditPlan plan)
        => new(
            Ra2AiToolAdaptationOutcomeKind.Proposal,
            plan ?? throw new ArgumentNullException(nameof(plan)),
            Ra2AiEditProposalFailureKind.None,
            "已解析结构化修改计划。");

    public static Ra2AiEditPlanCreationResult Clarification(string message)
        => new(
            Ra2AiToolAdaptationOutcomeKind.NeedsClarification,
            null,
            Ra2AiEditProposalFailureKind.None,
            message);

    public static Ra2AiEditPlanCreationResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
    {
        if (failureKind == Ra2AiEditProposalFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        return new Ra2AiEditPlanCreationResult(
            Ra2AiToolAdaptationOutcomeKind.Failed,
            null,
            failureKind,
            message);
    }
}

/// <summary>由协调器创建、可交给 UI 审阅但不能自行应用的不可变建议。</summary>
internal sealed class Ra2AiEditProposal
{
    internal Ra2AiEditProposal(
        Ra2IniEditPreview preview,
        Ra2AiEditProposalApplyPolicy applyPolicy,
        string riskSummary)
    {
        if (!preview.Succeeded || preview.PreviewId == Guid.Empty)
            throw new ArgumentException("A proposal requires a successful preview.", nameof(preview));
        if (!Enum.IsDefined(applyPolicy))
            throw new ArgumentOutOfRangeException(nameof(applyPolicy));
        if (string.IsNullOrWhiteSpace(riskSummary))
            throw new ArgumentException("Proposal risk summary is required.", nameof(riskSummary));

        ProposalId = Guid.NewGuid();
        Preview = preview;
        ApplyPolicy = applyPolicy;
        RiskSummary = riskSummary;
    }

    public Guid ProposalId { get; }

    public Ra2IniEditPreview Preview { get; }

    public Ra2AiEditProposalApplyPolicy ApplyPolicy { get; }

    public string RiskSummary { get; }
}

internal sealed class Ra2AiEditProposalResult
{
    private Ra2AiEditProposalResult(
        Ra2AiToolAdaptationOutcomeKind outcomeKind,
        Ra2AiEditProposal? proposal,
        Ra2AiEditProposalFailureKind failureKind,
        string message)
    {
        bool isProposal = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;
        bool isClarification = outcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;
        bool isFailed = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Failed;
        if (!Enum.IsDefined(outcomeKind) ||
            isProposal != (proposal is not null) ||
            isFailed != (failureKind != Ra2AiEditProposalFailureKind.None) ||
            isClarification && (proposal is not null || failureKind != Ra2AiEditProposalFailureKind.None))
        {
            throw new ArgumentException("Proposal result state is inconsistent.");
        }
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Proposal result message is required.", nameof(message));

        OutcomeKind = outcomeKind;
        Proposal = proposal;
        FailureKind = failureKind;
        Message = message;
    }

    public bool Succeeded => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;

    public bool NeedsClarification => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;

    public Ra2AiToolAdaptationOutcomeKind OutcomeKind { get; }

    public Ra2AiEditProposal? Proposal { get; }

    public Ra2AiEditProposalFailureKind FailureKind { get; }

    public string Message { get; }

    public static Ra2AiEditProposalResult FromProposal(Ra2AiEditProposal proposal)
        => new(
            Ra2AiToolAdaptationOutcomeKind.Proposal,
            proposal ?? throw new ArgumentNullException(nameof(proposal)),
            Ra2AiEditProposalFailureKind.None,
            "已生成当前文件的结构化修改建议。");

    public static Ra2AiEditProposalResult Clarification(string message)
        => new(
            Ra2AiToolAdaptationOutcomeKind.NeedsClarification,
            null,
            Ra2AiEditProposalFailureKind.None,
            message);

    public static Ra2AiEditProposalResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
    {
        if (failureKind == Ra2AiEditProposalFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        return new Ra2AiEditProposalResult(
            Ra2AiToolAdaptationOutcomeKind.Failed,
            null,
            failureKind,
            message);
    }
}

internal sealed class Ra2AiEditProposalApplyResult
{
    private Ra2AiEditProposalApplyResult(
        Ra2IniEditApplyResult? authoringResult,
        Ra2AiEditProposalFailureKind failureKind,
        string message)
    {
        bool succeeded = failureKind == Ra2AiEditProposalFailureKind.None;
        if (succeeded != (authoringResult?.Succeeded == true))
            throw new ArgumentException("Proposal apply result state is inconsistent.");
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Proposal apply result message is required.", nameof(message));

        AuthoringResult = authoringResult;
        FailureKind = failureKind;
        Message = message;
    }

    public bool Succeeded => FailureKind == Ra2AiEditProposalFailureKind.None;

    public Ra2IniEditApplyResult? AuthoringResult { get; }

    public Ra2AiEditProposalFailureKind FailureKind { get; }

    public string Message { get; }

    public static Ra2AiEditProposalApplyResult Applied(Ra2IniEditApplyResult result)
    {
        if (result?.Succeeded != true)
            throw new ArgumentException("A successful authoring result is required.", nameof(result));

        return new Ra2AiEditProposalApplyResult(
            result,
            Ra2AiEditProposalFailureKind.None,
            result.Message);
    }

    public static Ra2AiEditProposalApplyResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        Ra2IniEditApplyResult? authoringResult = null)
    {
        if (failureKind == Ra2AiEditProposalFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        return new Ra2AiEditProposalApplyResult(
            authoringResult,
            failureKind,
            message);
    }
}
