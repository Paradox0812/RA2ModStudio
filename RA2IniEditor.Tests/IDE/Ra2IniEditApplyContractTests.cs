using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniEditApplyContractTests
{
    [Fact]
    public void ApplyRequest_CarriesOnlyPreviewIdentityAndExplicitConfirmation()
    {
        string[] propertyNames = typeof(Ra2IniEditApplyRequest)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                nameof(Ra2IniEditApplyRequest.ExplicitConfirmationGranted),
                nameof(Ra2IniEditApplyRequest.PreviewId)
            ],
            propertyNames);
    }

    [Theory]
    [InlineData((int)Ra2IniEditApplyOutcomeKind.PreviewUnavailable)]
    [InlineData((int)Ra2IniEditApplyOutcomeKind.ConfirmationRequired)]
    [InlineData((int)Ra2IniEditApplyOutcomeKind.TransactionRejected)]
    [InlineData((int)Ra2IniEditApplyOutcomeKind.UnexpectedFailure)]
    public void FailedResult_NeverCarriesCommitEvidence(int outcomeValue)
    {
        Ra2IniEditApplyOutcomeKind outcome = (Ra2IniEditApplyOutcomeKind)outcomeValue;
        Guid previewId = Guid.NewGuid();
        Ra2IniEditApplyResult result = outcome switch
        {
            Ra2IniEditApplyOutcomeKind.PreviewUnavailable =>
                Ra2IniEditApplyResult.PreviewUnavailable(previewId),
            Ra2IniEditApplyOutcomeKind.ConfirmationRequired =>
                Ra2IniEditApplyResult.ConfirmationRequired(previewId),
            Ra2IniEditApplyOutcomeKind.TransactionRejected =>
                Ra2IniEditApplyResult.TransactionRejected(previewId),
            Ra2IniEditApplyOutcomeKind.UnexpectedFailure =>
                Ra2IniEditApplyResult.UnexpectedFailure(previewId),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };

        Assert.False(result.Succeeded);
        Assert.Equal(outcome, result.OutcomeKind);
        Assert.Equal(previewId, result.PreviewId);
        Assert.Null(result.UpdatedSession);
        Assert.Null(result.TextToSyncToEditor);
        Assert.Null(result.UndoText);
        Assert.Null(result.RedoText);
        Assert.Null(result.UndoCaretOffset);
        Assert.Null(result.RedoCaretOffset);
        Assert.Equal(0, result.OperationCount);
        Assert.False(result.IsDirtyAfterApply);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }
}
