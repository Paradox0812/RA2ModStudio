using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationSectionQueryTests
{
    private readonly Ra2AutomationDocumentQueryService _service = new();

    [Fact]
    public void GetSection_UniqueNameUsesCaseInsensitiveMatchAndProjectsSpans()
    {
        const string text = "[InfantryTypes]\r\n0=E1\r\n[E1] ; infantry\r\n  Strength = 300 ; hp\r\n";

        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot(text, new BuiltInRa2FieldDefinitionProvider()),
            new Ra2AutomationSectionQuery(" e1 "));

        Ra2AutomationSectionFact section = AssertSuccess(result);
        Assert.Equal("E1", section.Name);
        Assert.Equal(Ra2SectionKind.Infantry, section.Kind);
        Assert.Equal(0, section.Occurrence);
        Assert.Equal(3, section.HeaderLineNumber);
        Assert.Equal("[E1]", AutomationTestSupport.Slice(text, section.HeaderSpan));
        Assert.Equal(
            " ; infantry\r\n  Strength = 300 ; hp\r\n",
            AutomationTestSupport.Slice(text, section.BodySpan));
        Assert.Equal(section.HeaderSpan.Start, section.FullSpan.Start);
        Assert.Equal(section.BodySpan.End, section.FullSpan.End);

        Ra2AutomationFieldFact field = Assert.Single(section.Fields);
        Assert.Equal("Strength", field.Key);
        Assert.Equal("300", field.EffectiveValue);
        Assert.Equal(4, field.LineNumber);
        Assert.Equal("Strength", AutomationTestSupport.Slice(text, field.KeySpan));
        Assert.Equal("300", AutomationTestSupport.Slice(text, field.ValueSpan!.Value));
        Assert.Equal("  Strength = 300 ; hp", AutomationTestSupport.Slice(text, field.LineSpan));
    }

    [Fact]
    public void GetSection_NullOccurrenceRejectsDuplicateNamesAsAmbiguous()
    {
        const string text = "[E1]\nStrength=100\n[E1]\nStrength=200\n";

        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationSectionQuery("E1"));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.AmbiguousSection, result.FailureKind);
        Assert.Null(result.Section);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public void GetSection_ExplicitOccurrencePreservesBodyIsolationDuplicateKeysAndOrder()
    {
        const string text =
            "[E1]\n" +
            "Duplicate=first\n" +
            "OnlyFirst=one\n" +
            "[E1]\n" +
            "Duplicate=second\n" +
            "Duplicate=third\n" +
            "OnlySecond=two\n";

        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationSectionQuery("E1", occurrence: 1));

        Ra2AutomationSectionFact section = AssertSuccess(result);
        Assert.Equal(1, section.Occurrence);
        Assert.Equal(
            new[] { "Duplicate", "Duplicate", "OnlySecond" },
            section.Fields.Select(field => field.Key));
        Assert.Equal(
            new[] { "second", "third", "two" },
            section.Fields.Select(field => field.EffectiveValue));
        Assert.DoesNotContain(section.Fields, field => field.EffectiveValue == "first");
        Assert.DoesNotContain(section.Fields, field => field.EffectiveValue == "one");
        Assert.All(section.Fields, field =>
            Assert.True(field.LineSpan.Start >= section.BodySpan.Start && field.LineSpan.End <= section.BodySpan.End));
    }

    [Fact]
    public void GetSection_ExplicitOutOfRangeIsNotFoundWithoutFallback()
    {
        const string text = "[E1]\nStrength=100\n[E1]\nStrength=200\n";

        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationSectionQuery("E1", occurrence: 2));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.NotFound, result.FailureKind);
        Assert.Null(result.Section);
    }

    [Fact]
    public void GetSection_MissingNameReturnsTypedFailureWithNoPartialPayload()
    {
        Ra2AutomationSectionQueryResult result = _service.GetSection(
            AutomationTestSupport.Snapshot("[E1]\nStrength=100\n"),
            new Ra2AutomationSectionQuery("Missing"));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.NotFound, result.FailureKind);
        Assert.Null(result.Section);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public void GetSection_RecordsMetadataFromTheHostSnapshotOnSuccessAndFailure()
    {
        const string text = "[E1]\nStrength=100\n";
        Ra2AutomationDocumentSnapshot snapshot = new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            9,
            "custom\rulesmd.ini",
            text,
            isEditable: true,
            new Ra2AutomationFieldRegistrySnapshot(
                new AutomationTestSupport.EmptyFieldDefinitionProvider(),
                42));

        Ra2AutomationSectionQueryResult success = _service.GetSection(snapshot, new Ra2AutomationSectionQuery("E1"));
        Ra2AutomationSectionQueryResult failure = _service.GetSection(snapshot, new Ra2AutomationSectionQuery("Nope"));

        Assert.Equal(snapshot.DocumentId, success.DocumentId);
        Assert.Equal(snapshot.Version, success.Version);
        Assert.Equal(snapshot.FilePath, success.FilePath);
        Assert.Equal(snapshot.FieldRegistry.Revision, success.FieldRegistryRevision);
        Assert.Equal(success.DocumentId, failure.DocumentId);
        Assert.Equal(success.FieldRegistryRevision, failure.FieldRegistryRevision);
    }

    private static Ra2AutomationSectionFact AssertSuccess(Ra2AutomationSectionQueryResult result)
    {
        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(Ra2AutomationSectionQueryFailureKind.None, result.FailureKind);
        Assert.NotNull(result.Section);
        Assert.NotEmpty(result.Message);
        return result.Section!;
    }
}
