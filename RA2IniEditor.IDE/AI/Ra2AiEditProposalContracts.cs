using System.IO;
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
    TemplateExpansionRejected,
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

internal enum Ra2AiAuthoringScope
{
    Document = 0,
    Project
}

internal sealed record Ra2AiProjectTargetResolution(
    Ra2AiProjectEditAvailabilityKind Availability,
    IReadOnlyList<string> TargetFilePaths)
{
    public bool Succeeded => Availability == Ra2AiProjectEditAvailabilityKind.Available;
}

/// <summary>仅做 IDE 发送前的精确文件名 admission；Application compiler 仍是最终配对权威。</summary>
internal static class Ra2AiProjectAuthoringAdmission
{
    internal static Ra2AiProjectTargetResolution ResolveRulesWithOptionalArtTargets(IEnumerable<string> projectFiles)
    {
        ArgumentNullException.ThrowIfNull(projectFiles);
        string[] files = projectFiles.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray();
        string[] rulesMd = Find(files, "rulesmd.ini");
        string[] artMd = Find(files, "artmd.ini");
        string[] rulesClassic = Find(files, "rules.ini");
        string[] artClassic = Find(files, "art.ini");
        if (rulesMd.Length > 1 || artMd.Length > 1 || rulesClassic.Length > 1 || artClassic.Length > 1 ||
            rulesMd.Length + rulesClassic.Length > 1)
        {
            return new(Ra2AiProjectEditAvailabilityKind.PairAmbiguous, []);
        }

        if (rulesMd.Length == 1)
        {
            return new(
                Ra2AiProjectEditAvailabilityKind.Available,
                Array.AsReadOnly(artMd.Length == 1 ? new[] { rulesMd[0], artMd[0] } : new[] { rulesMd[0] }));
        }
        if (rulesClassic.Length == 1)
        {
            return new(
                Ra2AiProjectEditAvailabilityKind.Available,
                Array.AsReadOnly(artClassic.Length == 1 ? new[] { rulesClassic[0], artClassic[0] } : new[] { rulesClassic[0] }));
        }

        return new(Ra2AiProjectEditAvailabilityKind.PairMissing, []);
    }

    internal static Ra2AiProjectTargetResolution ResolveRulesArtTargets(IEnumerable<string> projectFiles)
    {
        ArgumentNullException.ThrowIfNull(projectFiles);
        string[] files = projectFiles.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray();
        string[] rulesMd = Find(files, "rulesmd.ini");
        string[] artMd = Find(files, "artmd.ini");
        string[] rulesClassic = Find(files, "rules.ini");
        string[] artClassic = Find(files, "art.ini");
        if (rulesMd.Length > 1 || artMd.Length > 1 || rulesClassic.Length > 1 || artClassic.Length > 1)
            return new(Ra2AiProjectEditAvailabilityKind.PairAmbiguous, []);

        bool hasMdPair = rulesMd.Length == 1 && artMd.Length == 1;
        bool hasClassicPair = rulesClassic.Length == 1 && artClassic.Length == 1;
        if (hasMdPair == hasClassicPair)
        {
            return new(
                hasMdPair
                    ? Ra2AiProjectEditAvailabilityKind.PairAmbiguous
                    : Ra2AiProjectEditAvailabilityKind.PairMissing,
                []);
        }

        return new(
            Ra2AiProjectEditAvailabilityKind.Available,
            Array.AsReadOnly(hasMdPair
                ? new[] { rulesMd[0], artMd[0] }
                : new[] { rulesClassic[0], artClassic[0] }));
    }

    private static string[] Find(IEnumerable<string> files, string fileName)
        => files.Where(path => string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase)).ToArray();
}

/// <summary>绑定一次 provider 请求开始时的本地创作快照。</summary>
internal sealed class Ra2AiAuthoringRequestContext
{
    public Ra2AiAuthoringRequestContext(Ra2AuthoringSnapshot snapshot)
    {
        Scope = Ra2AiAuthoringScope.Document;
        DocumentSnapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        TargetFilePaths = [];
    }

    private Ra2AiAuthoringRequestContext(
        Ra2AutomationProjectSnapshot snapshot,
        IReadOnlyList<string> targetFilePaths)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(targetFilePaths);
        string[] paths = targetFilePaths.ToArray();
        if (paths.Length != snapshot.Documents.Count ||
            paths.Any(string.IsNullOrWhiteSpace) ||
            paths.Distinct(StringComparer.OrdinalIgnoreCase).Count() != paths.Length ||
            paths.Where((path, index) => !string.Equals(
                path,
                snapshot.Documents[index].FilePath,
                StringComparison.OrdinalIgnoreCase)).Any())
        {
            throw new ArgumentException("Project authoring targets must match the project snapshot.", nameof(targetFilePaths));
        }

        Scope = Ra2AiAuthoringScope.Project;
        ProjectSnapshot = snapshot;
        TargetFilePaths = Array.AsReadOnly(paths);
    }

    public Ra2AiAuthoringScope Scope { get; }

    public Ra2AuthoringSnapshot? DocumentSnapshot { get; }

    public Ra2AutomationProjectSnapshot? ProjectSnapshot { get; }

    public IReadOnlyList<string> TargetFilePaths { get; }

    public Ra2AuthoringSnapshot Snapshot
        => DocumentSnapshot ?? throw new InvalidOperationException("The authoring request is project-scoped.");

    public static Ra2AiAuthoringRequestContext ForProject(
        Ra2AutomationProjectSnapshot snapshot,
        IReadOnlyList<string> targetFilePaths)
        => new(snapshot, targetFilePaths);
}

internal sealed class Ra2AiEditPlanCreationResult
{
    private Ra2AiEditPlanCreationResult(
        Ra2AiToolAdaptationOutcomeKind outcomeKind,
        Ra2IniEditPlan? plan,
        Ra2AutomationProjectEditPlan? projectPlan,
        Ra2AutomationAssetManifest? assetManifest,
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        Ra2AiStructuredFailureEvidence? failureEvidence = null)
    {
        bool isProposal = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;
        bool isClarification = outcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;
        bool isFailed = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Failed;
        bool hasDocumentPlan = plan is not null;
        bool hasProjectPlan = projectPlan is not null;
        if (!Enum.IsDefined(outcomeKind) ||
            isProposal != (hasDocumentPlan ^ hasProjectPlan) ||
            !isProposal && (plan is not null || projectPlan is not null || assetManifest is not null) ||
            projectPlan is null && assetManifest is not null ||
            isFailed != (failureKind != Ra2AiEditProposalFailureKind.None) ||
            isFailed != (failureEvidence is not null) ||
            isClarification && (plan is not null || projectPlan is not null || failureKind != Ra2AiEditProposalFailureKind.None))
        {
            throw new ArgumentException("Plan creation result state is inconsistent.");
        }
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Plan creation result message is required.", nameof(message));

        OutcomeKind = outcomeKind;
        Plan = plan;
        ProjectPlan = projectPlan;
        AssetManifest = assetManifest;
        FailureKind = failureKind;
        Message = message;
        FailureEvidence = failureEvidence;
    }

    public bool Succeeded => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;

    public bool NeedsClarification => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;

    public Ra2AiToolAdaptationOutcomeKind OutcomeKind { get; }

    public Ra2IniEditPlan? Plan { get; }

    public Ra2AutomationProjectEditPlan? ProjectPlan { get; }

    public Ra2AutomationAssetManifest? AssetManifest { get; }

    public Ra2AiEditProposalFailureKind FailureKind { get; }

    public string Message { get; }

    public Ra2AiStructuredFailureEvidence? FailureEvidence { get; }

    public static Ra2AiEditPlanCreationResult FromPlan(Ra2IniEditPlan plan)
        => new(
            Ra2AiToolAdaptationOutcomeKind.Proposal,
            plan ?? throw new ArgumentNullException(nameof(plan)),
            null,
            null,
            Ra2AiEditProposalFailureKind.None,
            "已解析结构化修改计划。");

    public static Ra2AiEditPlanCreationResult FromProjectPlan(
        Ra2AutomationProjectEditPlan plan,
        Ra2AutomationAssetManifest? assetManifest)
        => new(
            Ra2AiToolAdaptationOutcomeKind.Proposal,
            null,
            plan ?? throw new ArgumentNullException(nameof(plan)),
            assetManifest,
            Ra2AiEditProposalFailureKind.None,
            "已解析项目结构化修改计划。");

    public static Ra2AiEditPlanCreationResult Clarification(string message)
        => new(
            Ra2AiToolAdaptationOutcomeKind.NeedsClarification,
            null,
            null,
            null,
            Ra2AiEditProposalFailureKind.None,
            message);

    public static Ra2AiEditPlanCreationResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        Ra2AiStructuredFailureEvidence? failureEvidence = null)
    {
        if (failureKind == Ra2AiEditProposalFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        return new Ra2AiEditPlanCreationResult(
            Ra2AiToolAdaptationOutcomeKind.Failed,
            null,
            null,
            null,
            failureKind,
            message,
            failureEvidence ?? Ra2AiStructuredFailureEvidence.FromAdapter(failureKind, message));
    }
}

/// <summary>由协调器创建、可交给 UI 审阅但不能自行应用的不可变建议。</summary>
internal sealed class Ra2AiEditProposal
{
    private readonly Ra2IniEditPreview? _documentPreview;
    private readonly Ra2ProjectEditPreview? _projectPreview;

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
        Scope = Ra2AiAuthoringScope.Document;
        _documentPreview = preview;
        ApplyPolicy = applyPolicy;
        RiskSummary = riskSummary;
    }

    private Ra2AiEditProposal(
        Ra2ProjectEditPreview preview,
        Ra2AutomationAssetManifest? assetManifest,
        Ra2AiEditProposalApplyPolicy applyPolicy,
        string riskSummary)
    {
        if (!preview.Succeeded || preview.ProjectPreviewId == Guid.Empty)
            throw new ArgumentException("A project proposal requires a successful preview.", nameof(preview));
        if (assetManifest is not null && assetManifest.ProjectSessionId != preview.Snapshot.ProjectSessionId)
            throw new ArgumentException("The asset manifest does not match the project preview.", nameof(assetManifest));
        if (!Enum.IsDefined(applyPolicy))
            throw new ArgumentOutOfRangeException(nameof(applyPolicy));
        if (string.IsNullOrWhiteSpace(riskSummary))
            throw new ArgumentException("Proposal risk summary is required.", nameof(riskSummary));

        ProposalId = Guid.NewGuid();
        Scope = Ra2AiAuthoringScope.Project;
        _projectPreview = preview;
        AssetManifest = assetManifest;
        ApplyPolicy = applyPolicy;
        RiskSummary = riskSummary;
    }

    public Guid ProposalId { get; }

    public Ra2AiAuthoringScope Scope { get; }

    public Ra2IniEditPreview Preview
        => _documentPreview ?? throw new InvalidOperationException("The proposal is project-scoped.");

    public Ra2ProjectEditPreview ProjectPreview
        => _projectPreview ?? throw new InvalidOperationException("The proposal is document-scoped.");

    public Ra2AutomationAssetManifest? AssetManifest { get; }

    public Ra2AiEditProposalApplyPolicy ApplyPolicy { get; }

    public string RiskSummary { get; }

    public static Ra2AiEditProposal FromProject(
        Ra2ProjectEditPreview preview,
        Ra2AutomationAssetManifest? assetManifest,
        Ra2AiEditProposalApplyPolicy applyPolicy,
        string riskSummary)
        => new(preview, assetManifest, applyPolicy, riskSummary);
}

internal sealed class Ra2AiEditProposalResult
{
    private Ra2AiEditProposalResult(
        Ra2AiToolAdaptationOutcomeKind outcomeKind,
        Ra2AiEditProposal? proposal,
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        Ra2AiStructuredFailureEvidence? failureEvidence = null)
    {
        bool isProposal = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;
        bool isClarification = outcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;
        bool isFailed = outcomeKind == Ra2AiToolAdaptationOutcomeKind.Failed;
        if (!Enum.IsDefined(outcomeKind) ||
            isProposal != (proposal is not null) ||
            isFailed != (failureKind != Ra2AiEditProposalFailureKind.None) ||
            isFailed != (failureEvidence is not null) ||
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
        FailureEvidence = failureEvidence;
    }

    public bool Succeeded => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.Proposal;

    public bool NeedsClarification => OutcomeKind == Ra2AiToolAdaptationOutcomeKind.NeedsClarification;

    public Ra2AiToolAdaptationOutcomeKind OutcomeKind { get; }

    public Ra2AiEditProposal? Proposal { get; }

    public Ra2AiEditProposalFailureKind FailureKind { get; }

    public string Message { get; }

    public Ra2AiStructuredFailureEvidence? FailureEvidence { get; }

    public static Ra2AiEditProposalResult FromProposal(Ra2AiEditProposal proposal)
        => new(
            Ra2AiToolAdaptationOutcomeKind.Proposal,
            proposal ?? throw new ArgumentNullException(nameof(proposal)),
            Ra2AiEditProposalFailureKind.None,
            proposal.Scope == Ra2AiAuthoringScope.Project
                ? "已生成当前项目的结构化修改建议。"
                : "已生成当前文件的结构化修改建议。");

    public static Ra2AiEditProposalResult Clarification(string message)
        => new(
            Ra2AiToolAdaptationOutcomeKind.NeedsClarification,
            null,
            Ra2AiEditProposalFailureKind.None,
            message);

    public static Ra2AiEditProposalResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        Ra2AiStructuredFailureEvidence? failureEvidence = null)
    {
        if (failureKind == Ra2AiEditProposalFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        return new Ra2AiEditProposalResult(
            Ra2AiToolAdaptationOutcomeKind.Failed,
            null,
            failureKind,
            message,
            failureEvidence ?? Ra2AiStructuredFailureEvidence.FromHost(failureKind, message));
    }
}

internal sealed class Ra2AiEditProposalApplyResult
{
    private Ra2AiEditProposalApplyResult(
        Ra2IniEditApplyResult? authoringResult,
        Ra2ProjectEditApplyResult? projectAuthoringResult,
        Ra2AiEditProposalFailureKind failureKind,
        string message)
    {
        bool succeeded = failureKind == Ra2AiEditProposalFailureKind.None;
        if (succeeded != ((authoringResult?.Succeeded == true) ^ (projectAuthoringResult?.Succeeded == true)) ||
            !succeeded && (authoringResult?.Succeeded == true || projectAuthoringResult?.Succeeded == true) ||
            authoringResult is not null && projectAuthoringResult is not null)
            throw new ArgumentException("Proposal apply result state is inconsistent.");
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Proposal apply result message is required.", nameof(message));

        AuthoringResult = authoringResult;
        ProjectAuthoringResult = projectAuthoringResult;
        FailureKind = failureKind;
        Message = message;
    }

    public bool Succeeded => FailureKind == Ra2AiEditProposalFailureKind.None;

    public Ra2IniEditApplyResult? AuthoringResult { get; }

    public Ra2ProjectEditApplyResult? ProjectAuthoringResult { get; }

    public Ra2AiEditProposalFailureKind FailureKind { get; }

    public string Message { get; }

    public static Ra2AiEditProposalApplyResult Applied(Ra2IniEditApplyResult result)
    {
        if (result?.Succeeded != true)
            throw new ArgumentException("A successful authoring result is required.", nameof(result));

        return new Ra2AiEditProposalApplyResult(
            result,
            null,
            Ra2AiEditProposalFailureKind.None,
            result.Message);
    }

    public static Ra2AiEditProposalApplyResult Applied(Ra2ProjectEditApplyResult result)
    {
        if (result?.Succeeded != true)
            throw new ArgumentException("A successful project authoring result is required.", nameof(result));

        return new Ra2AiEditProposalApplyResult(
            null,
            result,
            Ra2AiEditProposalFailureKind.None,
            result.Message);
    }

    public static Ra2AiEditProposalApplyResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message,
        Ra2IniEditApplyResult? authoringResult = null,
        Ra2ProjectEditApplyResult? projectAuthoringResult = null)
    {
        if (failureKind == Ra2AiEditProposalFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        return new Ra2AiEditProposalApplyResult(
            authoringResult,
            projectAuthoringResult,
            failureKind,
            message);
    }
}
