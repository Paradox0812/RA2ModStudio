using RA2IniEditor.Application.Automation.Experimental;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationEditContractTests
{
    [Fact]
    public void Operation_NormalizesIdentifiersAndPreservesValue()
    {
        Ra2AutomationEditOperation operation = new(
            Ra2AutomationEditOperationKind.UpsertField,
            "  E1  ",
            "  Strength  ",
            " 150 ");

        Assert.Equal("E1", operation.SectionName);
        Assert.Equal("Strength", operation.Key);
        Assert.Equal(" 150 ", operation.Value);
    }

    [Theory]
    [InlineData("", "Key")]
    [InlineData("[E1]", "Key")]
    [InlineData("E1\nE2", "Key")]
    [InlineData("E1", "Bad=Key")]
    public void Operation_RejectsInvalidIdentifiers(string section, string key)
    {
        Assert.Throws<ArgumentException>(() => new Ra2AutomationEditOperation(
            Ra2AutomationEditOperationKind.UpsertField,
            section,
            key,
            "1"));
    }

    [Fact]
    public void Operation_EnforcesExactLengthAndValueRules()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationEditOperation(
            (Ra2AutomationEditOperationKind)99,
            "E1",
            "Strength",
            "1"));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationEditOperation(
            Ra2AutomationEditOperationKind.UpsertField,
            new string('S', Ra2AutomationEditOperation.MaximumSectionNameLength + 1),
            "Key",
            "1"));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationEditOperation(
            Ra2AutomationEditOperationKind.UpsertField,
            "E1",
            "Key",
            "1\r\n2"));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationEditOperation(
            Ra2AutomationEditOperationKind.UpsertField,
            "E1",
            "Key",
            new string('V', Ra2AutomationEditOperation.MaximumValueLength + 1)));
    }

    [Fact]
    public void Plan_DefensivelyCopiesOperationsAndValidatesIdentity()
    {
        Guid documentId = Guid.NewGuid();
        List<Ra2AutomationEditOperation> source =
        [
            new(Ra2AutomationEditOperationKind.UpsertField, "E1", "Strength", "150")
        ];
        Ra2AutomationEditPlan plan = new(
            Guid.NewGuid(),
            documentId,
            2,
            3,
            source,
            "  Set Strength  ",
            "  test  ");

        source.Clear();

        Assert.Single(plan.Operations);
        Assert.Equal("Set Strength", plan.Summary);
        Assert.Equal("test", plan.Origin);
        Assert.Throws<ArgumentException>(() => new Ra2AutomationEditPlan(
            Guid.Empty,
            documentId,
            2,
            3,
            plan.Operations,
            "Summary",
            "test"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationEditPlan(
            Guid.NewGuid(),
            documentId,
            2,
            3,
            Array.Empty<Ra2AutomationEditOperation>(),
            "Summary",
            "test"));
    }

    [Fact]
    public void FailureEnum_PreservesApprovedNumericValues()
    {
        Assert.Equal(0, (int)Ra2AutomationEditPreviewFailureKind.None);
        Assert.Equal(16, (int)Ra2AutomationEditPreviewFailureKind.UnexpectedFailure);
        Assert.Equal(17, (int)Ra2AutomationEditPreviewFailureKind.DocumentTooLarge);
        Assert.Equal(18, (int)Ra2AutomationEditPreviewFailureKind.ResultLimitExceeded);
        Assert.Equal(8, (int)Ra2AutomationFieldTrustLevel.Unknown);
    }

    [Fact]
    public void ServiceSkeleton_FailureCarriesNoApplicablePayload()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot("[E1]\nStrength=100\n");
        Ra2AutomationEditPlan plan = new(
            Guid.NewGuid(),
            snapshot.DocumentId,
            snapshot.Version,
            snapshot.FieldRegistry.Revision,
            [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Strength", "150")],
            "Set Strength",
            "test");

        Ra2AutomationEditPreviewResult result = new Ra2AutomationEditPreviewService().Preview(snapshot, plan);

        Assert.False(result.Succeeded);
        Assert.Equal(Guid.Empty, result.PreviewId);
        Assert.Null(result.CandidateText);
        Assert.Empty(result.Changes);
        Assert.Empty(result.OperationPreviews);
        Assert.Empty(result.AddedDiagnostics);
        Assert.Empty(result.RemovedDiagnostics);
        Assert.False(result.RequiresExplicitConfirmation);
    }
}
