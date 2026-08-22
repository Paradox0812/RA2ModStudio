using RA2IniEditor.Application.Automation.Experimental;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationDeterminismTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(7)]
    public void QueryResultsRemainDeterministicForOneFourAndSevenMiDocuments(int approximateMegabytes)
    {
        string text = AutomationTestSupport.BuildLargeDocument(approximateMegabytes);
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(text);
        Ra2AutomationDocumentQueryService service = new();

        Ra2AutomationSectionQuery sectionQuery = new("E1");
        Ra2AutomationSectionQueryResult firstSection = service.GetSection(snapshot, sectionQuery);
        Ra2AutomationSectionQueryResult secondSection = service.GetSection(snapshot, sectionQuery);

        Assert.True(firstSection.Succeeded, firstSection.Message);
        Assert.True(secondSection.Succeeded, secondSection.Message);
        Assert.Equal(
            firstSection.Section!.Fields.Select(field => (field.Key, field.EffectiveValue, field.LineSpan)),
            secondSection.Section!.Fields.Select(field => (field.Key, field.EffectiveValue, field.LineSpan)));
        Assert.Equal(firstSection.Section.FullSpan, secondSection.Section.FullSpan);
        Assert.Equal(text, snapshot.Text);

        int headerOffset = text.IndexOf("[SharedWeapon]", StringComparison.Ordinal) + 1;
        Ra2AutomationReferenceQueryResult references = service.FindReferences(
            snapshot,
            new Ra2AutomationReferenceQuery(headerOffset));
        Ra2AutomationReferenceQueryResult secondReferences = service.FindReferences(
            snapshot,
            new Ra2AutomationReferenceQuery(headerOffset));

        Assert.True(references.Succeeded, references.Message);
        Assert.True(secondReferences.Succeeded, secondReferences.Message);
        Assert.Equal("SharedWeapon", references.Target!.Name);
        Assert.Equal(
            references.References.Select(reference => (reference.SourceSectionName, reference.SourceKey, reference.ValueSpan)),
            secondReferences.References.Select(reference => (reference.SourceSectionName, reference.SourceKey, reference.ValueSpan)));
        Assert.Single(references.References);
        Assert.Equal("E1", references.References[0].SourceSectionName);
    }
}
