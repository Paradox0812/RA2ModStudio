using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiStructuredRepairPolicyTests
{
    [Fact]
    public void Evaluate_AuthoringToolNotInvoked_IsEligibleOnce()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateAuthoringToolNotInvoked("plain markdown");

        Ra2AiStructuredRepairDecision decision = Ra2AiStructuredRepairPolicy.Evaluate(
            response,
            proposalResult: null,
            repairAlreadyAttempted: false);

        Assert.True(decision.IsEligible);
        Assert.Equal(Ra2AiStructuredFailureSource.Response, decision.Evidence?.Source);
        Assert.False(Ra2AiStructuredRepairPolicy.Evaluate(response, null, true).IsEligible);
    }

    [Theory]
    [InlineData((int)Ra2AiEditProposalFailureKind.UnsupportedTool)]
    [InlineData((int)Ra2AiEditProposalFailureKind.MultipleToolCalls)]
    [InlineData((int)Ra2AiEditProposalFailureKind.MissingArguments)]
    [InlineData((int)Ra2AiEditProposalFailureKind.InvalidArgumentsJson)]
    [InlineData((int)Ra2AiEditProposalFailureKind.UnknownArgumentProperty)]
    [InlineData((int)Ra2AiEditProposalFailureKind.DuplicateArgumentProperty)]
    [InlineData((int)Ra2AiEditProposalFailureKind.InvalidOperation)]
    public void Evaluate_AllowlistedAdapterFailure_IsEligible(int failureKindValue)
    {
        Ra2AiEditProposalFailureKind failureKind = (Ra2AiEditProposalFailureKind)failureKindValue;
        Ra2AiStructuredFailureEvidence evidence = Ra2AiStructuredFailureEvidence.FromAdapter(
            failureKind,
            "invalid model plan");
        Ra2AiEditProposalResult result = Ra2AiEditProposalResult.Failed(
            failureKind,
            evidence.Message,
            evidence);

        Assert.True(Evaluate(result).IsEligible);
    }

    [Theory]
    [InlineData(Ra2IniEditPreviewFailureKind.SectionNotFound, true)]
    [InlineData(Ra2IniEditPreviewFailureKind.AmbiguousField, true)]
    [InlineData(Ra2IniEditPreviewFailureKind.SectionClassificationMismatch, true)]
    [InlineData(Ra2IniEditPreviewFailureKind.StalePlanTarget, false)]
    [InlineData(Ra2IniEditPreviewFailureKind.DocumentTooLarge, false)]
    [InlineData(Ra2IniEditPreviewFailureKind.BlockedFieldTrust, false)]
    public void Evaluate_DocumentPreviewUsesTypedAllowlist(
        Ra2IniEditPreviewFailureKind failureKind,
        bool expected)
    {
        Ra2AiStructuredFailureEvidence evidence =
            Ra2AiStructuredFailureEvidence.FromDocumentPreview(failureKind, "preview failed");
        Ra2AiEditProposalResult result = Ra2AiEditProposalResult.Failed(
            Ra2AiEditProposalFailureKind.PreviewRejected,
            evidence.Message,
            evidence);

        Assert.Equal(expected, Evaluate(result).IsEligible);
    }

    [Fact]
    public void Evaluate_ProjectDocumentFailureRequiresEligibleLeaf()
    {
        Ra2AiStructuredFailureEvidence eligible = Ra2AiStructuredFailureEvidence.FromProjectPreview(
            Ra2AutomationProjectEditPreviewFailureKind.DocumentPreviewFailed,
            Ra2IniEditPreviewFailureKind.SectionNotFound,
            "wrong document");
        Ra2AiStructuredFailureEvidence denied = Ra2AiStructuredFailureEvidence.FromProjectPreview(
            Ra2AutomationProjectEditPreviewFailureKind.DocumentPreviewFailed,
            Ra2IniEditPreviewFailureKind.ResultLimitExceeded,
            "resource limit");

        Assert.True(Evaluate(FailedPreview(eligible)).IsEligible);
        Assert.False(Evaluate(FailedPreview(denied)).IsEligible);
    }

    [Fact]
    public void Evaluate_MissingTypedEvidence_IsDenied()
    {
        Ra2AiEditProposalResult result = Ra2AiEditProposalResult.Failed(
            Ra2AiEditProposalFailureKind.PreviewRejected,
            "generic failure");

        Assert.False(Evaluate(result).IsEligible);
    }

    [Theory]
    [InlineData((int)Ra2AiResponseKind.Timeout)]
    [InlineData((int)Ra2AiResponseKind.ProviderError)]
    [InlineData((int)Ra2AiResponseKind.Cancelled)]
    [InlineData((int)Ra2AiResponseKind.MissingConfiguration)]
    [InlineData((int)Ra2AiResponseKind.LocalRejection)]
    public void Evaluate_InfrastructureOrLocalFailure_IsDenied(int responseKindValue)
    {
        Ra2AiResponseKind responseKind = (Ra2AiResponseKind)responseKindValue;
        Ra2AiResponse response = CreateFailureResponse(responseKind);

        Assert.False(Ra2AiStructuredRepairPolicy.Evaluate(response, null, false).IsEligible);
    }

    private static Ra2AiStructuredRepairDecision Evaluate(Ra2AiEditProposalResult result)
        => Ra2AiStructuredRepairPolicy.Evaluate(
            Ra2AiResponse.CreateToolCalls([new Ra2AiToolCall("call-1", "preview_ini_edit", "{}")]),
            result,
            repairAlreadyAttempted: false);

    private static Ra2AiEditProposalResult FailedPreview(Ra2AiStructuredFailureEvidence evidence)
        => Ra2AiEditProposalResult.Failed(
            Ra2AiEditProposalFailureKind.PreviewRejected,
            evidence.Message,
            evidence);

    private static Ra2AiResponse CreateFailureResponse(Ra2AiResponseKind kind)
        => kind switch
        {
            Ra2AiResponseKind.Timeout => Ra2AiResponse.CreateTimeout("timeout", Ra2AiFailureKind.TotalTimeout),
            Ra2AiResponseKind.ProviderError => Ra2AiResponse.CreateProviderFailure(
                Ra2AiFailureKind.NetworkOrProxy,
                "provider"),
            Ra2AiResponseKind.Cancelled => Ra2AiResponse.CreateCancelled("cancelled"),
            Ra2AiResponseKind.MissingConfiguration => Ra2AiResponse.CreateMissingConfiguration(),
            Ra2AiResponseKind.LocalRejection => Ra2AiResponse.CreateLocalRejection("rejected"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
}
