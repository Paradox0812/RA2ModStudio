namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiStructuredFailureSource
{
    Response = 0,
    Adapter,
    Template,
    DocumentPreview,
    ProjectPreview,
    Host
}

/// <summary>保留结构化失败的类型化叶级事实；只在单次 AI 请求内流转，不序列化。</summary>
internal sealed record Ra2AiStructuredFailureEvidence
{
    private const int MaximumMessageLength = 1024;
    private const int MaximumFailedContentLength = 4096;

    private Ra2AiStructuredFailureEvidence(
        Ra2AiStructuredFailureSource source,
        Ra2AiEditProposalFailureKind proposalFailureKind,
        string message,
        Ra2AutomationTemplateExpansionFailureKind? templateFailureKind = null,
        Ra2IniEditPreviewFailureKind? documentPreviewFailureKind = null,
        Ra2AutomationProjectEditPreviewFailureKind? projectPreviewFailureKind = null,
        string? toolName = null,
        string? failedContent = null)
    {
        if (!Enum.IsDefined(source))
            throw new ArgumentOutOfRangeException(nameof(source));
        if (!Enum.IsDefined(proposalFailureKind) || proposalFailureKind == Ra2AiEditProposalFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(proposalFailureKind));

        Source = source;
        ProposalFailureKind = proposalFailureKind;
        Message = BoundRequired(message, MaximumMessageLength, nameof(message));
        TemplateFailureKind = templateFailureKind;
        DocumentPreviewFailureKind = documentPreviewFailureKind;
        ProjectPreviewFailureKind = projectPreviewFailureKind;
        ToolName = BoundOptional(toolName, Ra2AiToolCall.MaximumNameLength);
        FailedContent = BoundOptional(failedContent, MaximumFailedContentLength, trim: false);
    }

    public Ra2AiStructuredFailureSource Source { get; }

    public Ra2AiEditProposalFailureKind ProposalFailureKind { get; }

    public string Message { get; }

    public Ra2AutomationTemplateExpansionFailureKind? TemplateFailureKind { get; }

    public Ra2IniEditPreviewFailureKind? DocumentPreviewFailureKind { get; }

    public Ra2AutomationProjectEditPreviewFailureKind? ProjectPreviewFailureKind { get; }

    public string? ToolName { get; }

    public string? FailedContent { get; }

    public static Ra2AiStructuredFailureEvidence FromResponse(
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        string? failedContent = null)
        => new(Ra2AiStructuredFailureSource.Response, failureKind, message, failedContent: failedContent);

    public static Ra2AiStructuredFailureEvidence FromAdapter(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
        => new(Ra2AiStructuredFailureSource.Adapter, failureKind, message);

    public static Ra2AiStructuredFailureEvidence FromTemplate(
        Ra2AiEditProposalFailureKind failureKind,
        Ra2AutomationTemplateExpansionFailureKind templateFailureKind,
        string message)
        => new(
            Ra2AiStructuredFailureSource.Template,
            failureKind,
            message,
            templateFailureKind: templateFailureKind);

    public static Ra2AiStructuredFailureEvidence FromDocumentPreview(
        Ra2IniEditPreviewFailureKind previewFailureKind,
        string message)
        => new(
            Ra2AiStructuredFailureSource.DocumentPreview,
            Ra2AiEditProposalFailureKind.PreviewRejected,
            message,
            documentPreviewFailureKind: previewFailureKind);

    public static Ra2AiStructuredFailureEvidence FromProjectPreview(
        Ra2AutomationProjectEditPreviewFailureKind projectFailureKind,
        Ra2IniEditPreviewFailureKind? documentFailureKind,
        string message)
        => new(
            Ra2AiStructuredFailureSource.ProjectPreview,
            Ra2AiEditProposalFailureKind.PreviewRejected,
            message,
            documentPreviewFailureKind: documentFailureKind,
            projectPreviewFailureKind: projectFailureKind);

    public static Ra2AiStructuredFailureEvidence FromHost(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
        => new(Ra2AiStructuredFailureSource.Host, failureKind, message);

    public Ra2AiStructuredFailureEvidence WithTool(Ra2AiToolCall toolCall)
    {
        ArgumentNullException.ThrowIfNull(toolCall);
        return new Ra2AiStructuredFailureEvidence(
            Source,
            ProposalFailureKind,
            Message,
            TemplateFailureKind,
            DocumentPreviewFailureKind,
            ProjectPreviewFailureKind,
            toolCall.Name,
            toolCall.ArgumentsJson);
    }

    private static string BoundRequired(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Failure evidence text is required.", parameterName);
        return value.Trim().Length <= maximumLength
            ? value.Trim()
            : value.Trim()[..maximumLength];
    }

    private static string? BoundOptional(string? value, int maximumLength, bool trim = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string normalized = trim ? value.Trim() : value;
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }
}

internal sealed record Ra2AiStructuredRepairDecision(
    bool IsEligible,
    Ra2AiStructuredFailureEvidence? Evidence,
    string Reason)
{
    public static Ra2AiStructuredRepairDecision Eligible(Ra2AiStructuredFailureEvidence evidence)
        => new(true, evidence ?? throw new ArgumentNullException(nameof(evidence)), "eligible-structured-failure");

    public static Ra2AiStructuredRepairDecision NotEligible(string reason)
        => new(false, null, string.IsNullOrWhiteSpace(reason) ? "not-eligible" : reason.Trim());
}

/// <summary>第三次调用使用的有界修复上下文；只描述一次失败，不授予新能力。</summary>
internal sealed record Ra2AiStructuredRepairContext
{
    public Ra2AiStructuredRepairContext(Ra2AiStructuredFailureEvidence evidence, int attempt = 1)
    {
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        if (attempt != 1)
            throw new ArgumentOutOfRangeException(nameof(attempt), "Only repair attempt 1/1 is supported.");
        Attempt = attempt;
    }

    public Ra2AiStructuredFailureEvidence Evidence { get; }

    public int Attempt { get; }
}

/// <summary>结构化修复的唯一资格白名单。未知和基础设施失败一律拒绝。</summary>
internal static class Ra2AiStructuredRepairPolicy
{
    public static Ra2AiStructuredRepairDecision Evaluate(
        Ra2AiResponse response,
        Ra2AiEditProposalResult? proposalResult,
        bool repairAlreadyAttempted)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (repairAlreadyAttempted)
            return Ra2AiStructuredRepairDecision.NotEligible("repair-limit-reached");

        if (response.Kind == Ra2AiResponseKind.AuthoringToolNotInvoked)
        {
            return Ra2AiStructuredRepairDecision.Eligible(
                Ra2AiStructuredFailureEvidence.FromResponse(
                    Ra2AiEditProposalFailureKind.MissingArguments,
                    "Work 模式要求结构化工具调用，但模型只返回了文本。",
                    response.Text));
        }

        if (response.Kind != Ra2AiResponseKind.ToolCalls || proposalResult is null)
            return Ra2AiStructuredRepairDecision.NotEligible("response-kind-not-repairable");
        if (proposalResult.Succeeded || proposalResult.NeedsClarification)
            return Ra2AiStructuredRepairDecision.NotEligible("proposal-terminal-not-failed");
        if (proposalResult.FailureEvidence is not { } evidence)
            return Ra2AiStructuredRepairDecision.NotEligible("typed-failure-evidence-missing");

        return IsEligible(evidence)
            ? Ra2AiStructuredRepairDecision.Eligible(evidence)
            : Ra2AiStructuredRepairDecision.NotEligible("failure-kind-not-allowlisted");
    }

    private static bool IsEligible(Ra2AiStructuredFailureEvidence evidence)
        => evidence.Source switch
        {
            Ra2AiStructuredFailureSource.Adapter => IsEligibleAdapterFailure(evidence.ProposalFailureKind),
            Ra2AiStructuredFailureSource.Template =>
                evidence.TemplateFailureKind is { } template && IsEligibleTemplateFailure(template),
            Ra2AiStructuredFailureSource.DocumentPreview =>
                evidence.DocumentPreviewFailureKind is { } document && IsEligibleDocumentFailure(document),
            Ra2AiStructuredFailureSource.ProjectPreview => IsEligibleProjectFailure(evidence),
            _ => false
        };

    private static bool IsEligibleAdapterFailure(Ra2AiEditProposalFailureKind failureKind)
        => failureKind is
            Ra2AiEditProposalFailureKind.UnsupportedTool or
            Ra2AiEditProposalFailureKind.MultipleToolCalls or
            Ra2AiEditProposalFailureKind.MissingArguments or
            Ra2AiEditProposalFailureKind.InvalidArgumentsJson or
            Ra2AiEditProposalFailureKind.UnknownArgumentProperty or
            Ra2AiEditProposalFailureKind.DuplicateArgumentProperty or
            Ra2AiEditProposalFailureKind.InvalidOperation;

    private static bool IsEligibleTemplateFailure(Ra2AutomationTemplateExpansionFailureKind failureKind)
        => failureKind is
            Ra2AutomationTemplateExpansionFailureKind.TemplateNotFound or
            Ra2AutomationTemplateExpansionFailureKind.TemplateVersionMismatch or
            Ra2AutomationTemplateExpansionFailureKind.InvalidArguments or
            Ra2AutomationTemplateExpansionFailureKind.MissingRequiredArgument or
            Ra2AutomationTemplateExpansionFailureKind.UnknownArgument or
            Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument or
            Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound or
            Ra2AutomationTemplateExpansionFailureKind.RequiredSectionKindMismatch or
            Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentNotFound or
            Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentAmbiguous;

    private static bool IsEligibleDocumentFailure(Ra2IniEditPreviewFailureKind failureKind)
        => failureKind is
            Ra2IniEditPreviewFailureKind.InvalidPlan or
            Ra2IniEditPreviewFailureKind.UnsupportedOperation or
            Ra2IniEditPreviewFailureKind.InvalidSection or
            Ra2IniEditPreviewFailureKind.SectionNotFound or
            Ra2IniEditPreviewFailureKind.AmbiguousSection or
            Ra2IniEditPreviewFailureKind.FieldNotFound or
            Ra2IniEditPreviewFailureKind.AmbiguousField or
            Ra2IniEditPreviewFailureKind.ConflictingOperations or
            Ra2IniEditPreviewFailureKind.OverlappingChanges or
            Ra2IniEditPreviewFailureKind.NoChanges or
            Ra2IniEditPreviewFailureKind.SectionAlreadyExists or
            Ra2IniEditPreviewFailureKind.ConflictingSectionCreations or
            Ra2IniEditPreviewFailureKind.SectionClassificationMismatch;

    private static bool IsEligibleProjectFailure(Ra2AiStructuredFailureEvidence evidence)
        => evidence.ProjectPreviewFailureKind switch
        {
            Ra2AutomationProjectEditPreviewFailureKind.InvalidProjectPlan => true,
            Ra2AutomationProjectEditPreviewFailureKind.DocumentNotFound => true,
            Ra2AutomationProjectEditPreviewFailureKind.DuplicateDocumentTarget => true,
            Ra2AutomationProjectEditPreviewFailureKind.DocumentPreviewFailed =>
                evidence.DocumentPreviewFailureKind is { } document && IsEligibleDocumentFailure(document),
            _ => false
        };
}
