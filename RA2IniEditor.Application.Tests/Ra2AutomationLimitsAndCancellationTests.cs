using RA2IniEditor.Application.Automation.Experimental;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationLimitsAndCancellationTests
{
    private readonly Ra2AutomationDocumentQueryService _service = new();

    [Fact]
    public void Queries_RejectDocumentAboveEightMiUtf16Characters()
    {
        string text = new('x', Ra2AutomationDocumentQueryService.MaximumDocumentCharacters + 1);
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(text);

        Ra2AutomationSectionQueryResult section = _service.GetSection(
            snapshot,
            new Ra2AutomationSectionQuery("E1"));
        Ra2AutomationReferenceQueryResult references = _service.FindReferences(
            snapshot,
            new Ra2AutomationReferenceQuery(0));

        Assert.Equal(Ra2AutomationSectionQueryFailureKind.DocumentTooLarge, section.FailureKind);
        Assert.Null(section.Section);
        Assert.Equal(Ra2AutomationReferenceQueryFailureKind.DocumentTooLarge, references.FailureKind);
        Assert.Null(references.Target);
        Assert.Empty(references.References);
    }

    [Fact]
    public void GetSection_RejectsMoreThanTenThousandFieldsWithoutPartialFacts()
    {
        string text = AutomationTestSupport.BuildManyFields(10_001);

        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationSectionQuery("ManyFields"));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.ResultLimitExceeded, result.FailureKind);
        Assert.Null(result.Section);
    }

    [Fact]
    public void FindReferences_RejectsMoreThanTenThousandReferencesWithoutPartialFacts()
    {
        string text = AutomationTestSupport.BuildManyReferences(10_001);

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(text.IndexOf("SharedWeapon", StringComparison.Ordinal) + 1));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationReferenceQueryFailureKind.ResultLimitExceeded, result.FailureKind);
        Assert.Null(result.Target);
        Assert.Empty(result.References);
        Assert.False(result.HasReferences);
    }

    [Fact]
    public void GetSection_PreCanceledRequestReturnsCanceledWithoutPayload()
    {
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot("[E1]\nStrength=100\n"),
            new Ra2AutomationSectionQuery("E1"),
            source.Token);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.Canceled, result.FailureKind);
        Assert.Null(result.Section);
    }

    [Fact]
    public void FindReferences_PreCanceledRequestReturnsCanceledWithoutPayload()
    {
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot("[E1]\nPrimary=Weapon\n"),
            new Ra2AutomationReferenceQuery(10),
            source.Token);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationReferenceQueryFailureKind.Canceled, result.FailureKind);
        Assert.Null(result.Target);
        Assert.Empty(result.References);
    }

    [Fact]
    public void GetSection_CancellationRaisedDuringBuildIsObservedAfterBuild()
    {
        using CancellationTokenSource source = new();
        Ra2AutomationReferenceQueryResult referenceResult = _service.FindReferences(
            AutomationTestSupport.Snapshot("[E1]\nStrength=100\n", new AutomationTestSupport.CancelingFieldDefinitionProvider(source)),
            new Ra2AutomationReferenceQuery(0),
            source.Token);

        Assert.Equal(Ra2AutomationReferenceQueryFailureKind.Canceled, referenceResult.FailureKind);

        using CancellationTokenSource sectionSource = new();
        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot("[E1]\nStrength=100\n", new AutomationTestSupport.CancelingFieldDefinitionProvider(sectionSource)),
            new Ra2AutomationSectionQuery("E1"),
            sectionSource.Token);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.Canceled, result.FailureKind);
        Assert.Null(result.Section);
    }

    [Fact]
    public void GetSection_MapsNonFatalAnalysisExceptionsToSafeFailure()
    {
        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot(
                "[E1]\nStrength=100\n",
                new AutomationTestSupport.ThrowingFieldDefinitionProvider(new InvalidOperationException("secret detail"))),
            new Ra2AutomationSectionQuery("E1"));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.AnalysisFailed, result.FailureKind);
        Assert.Null(result.Section);
        Assert.DoesNotContain("secret detail", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetSection_DoesNotSwallowFatalAnalysisExceptions()
    {
        Assert.Throws<OutOfMemoryException>(() => _service.GetSection(
            AutomationTestSupport.Snapshot(
                "[E1]\nStrength=100\n",
                new AutomationTestSupport.ThrowingFieldDefinitionProvider(new OutOfMemoryException())),
            new Ra2AutomationSectionQuery("E1")));
    }

    [Fact]
    public void FindReferences_MapsNonFatalAnalysisExceptionsToSafeFailure()
    {
        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(
                "[E1]\nPrimary=Weapon\n",
                new AutomationTestSupport.ThrowingFieldDefinitionProvider(new InvalidOperationException("secret detail"))),
            new Ra2AutomationReferenceQuery(14));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationReferenceQueryFailureKind.AnalysisFailed, result.FailureKind);
        Assert.Null(result.Target);
        Assert.Empty(result.References);
        Assert.DoesNotContain("secret detail", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FindReferences_DoesNotSwallowFatalAnalysisExceptions()
    {
        Assert.Throws<OutOfMemoryException>(() => _service.FindReferences(
            AutomationTestSupport.Snapshot(
                "[E1]\nPrimary=Weapon\n",
                new AutomationTestSupport.ThrowingFieldDefinitionProvider(new OutOfMemoryException())),
            new Ra2AutomationReferenceQuery(14)));
    }
}
