using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationSectionCreationPreviewTests
{
    [Theory]
    [InlineData("", "[New]\nKey=Value\n")]
    [InlineData("[E]\nA=1", "[E]\nA=1\n\n[New]\nKey=Value\n")]
    [InlineData("[E]\nA=1\n", "[E]\nA=1\n\n[New]\nKey=Value\n")]
    [InlineData("[E]\nA=1\n\n", "[E]\nA=1\n\n[New]\nKey=Value\n")]
    [InlineData("[E]\r\nA=1", "[E]\r\nA=1\r\n\r\n[New]\r\nKey=Value\r\n")]
    public void CreateSection_AppendsDeterministicGoldenText(string source, string expected)
    {
        Ra2AutomationDocumentSnapshot snapshot = Editable(source);

        Ra2AutomationEditPreviewResult result = Preview(
            snapshot,
            [new Ra2AutomationSectionCreateOperation("New", Ra2SectionKind.Unknown)],
            [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "New", "Key", "Value")]);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(expected, result.CandidateText);
        Ra2AutomationSectionCreatePreview creation = Assert.Single(result.SectionCreationPreviews);
        Assert.False(creation.IsClassificationResolved);
        Assert.Equal(Ra2AutomationFieldAuthoringDisposition.Caution, creation.AuthoringDisposition);
        Assert.Equal(source.Length, creation.AffectedOriginalSpan.Start);
        Assert.Equal(expected, ApplyChanges(source, result.Changes));
    }

    [Fact]
    public void CreateSections_PreservesPlanOrderAndOneBlankLineBetweenCreatedSections()
    {
        Ra2AutomationDocumentSnapshot snapshot = Editable(string.Empty);
        Ra2AutomationEditPreviewResult result = Preview(
            snapshot,
            [
                new Ra2AutomationSectionCreateOperation("A", Ra2SectionKind.Unknown),
                new Ra2AutomationSectionCreateOperation("B", Ra2SectionKind.Unknown)
            ],
            [
                new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "B", "Second", "2"),
                new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "A", "First", "1")
            ]);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal("[A]\nFirst=1\n\n[B]\nSecond=2\n", result.CandidateText);
        Assert.Equal(["A", "B"], result.SectionCreationPreviews.Select(item => item.Operation.SectionName));
        Assert.Equal(["Second", "First"], result.OperationPreviews.Select(item => item.Operation.Key));
    }

    [Fact]
    public void CreateSection_CanMixExistingAndNewSectionOperations()
    {
        Ra2AutomationDocumentSnapshot snapshot = Editable("[Existing]\nA=1");
        Ra2AutomationEditPreviewResult result = Preview(
            snapshot,
            [new Ra2AutomationSectionCreateOperation("New", Ra2SectionKind.Unknown)],
            [
                new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "Existing", "B", "2"),
                new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "New", "C", "3")
            ]);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal("[Existing]\nA=1\nB=2\n\n[New]\nC=3\n", result.CandidateText);
    }

    [Fact]
    public void CreateSection_RejectsExistingDuplicateAndClassificationMismatchWithoutPayload()
    {
        Ra2AutomationDocumentSnapshot existing = Editable("[New]\nA=1\n");
        AssertFailure(
            Preview(existing, [new Ra2AutomationSectionCreateOperation("new", Ra2SectionKind.Unknown)], []),
            Ra2AutomationEditPreviewFailureKind.SectionAlreadyExists);

        Ra2AutomationDocumentSnapshot empty = Editable(string.Empty);
        AssertFailure(
            Preview(
                empty,
                [
                    new Ra2AutomationSectionCreateOperation("New", Ra2SectionKind.Unknown),
                    new Ra2AutomationSectionCreateOperation("NEW", Ra2SectionKind.Unknown)
                ],
                []),
            Ra2AutomationEditPreviewFailureKind.ConflictingSectionCreations);

        Ra2AutomationDocumentSnapshot classified = Editable("[E1]\nPrimary=NewThing\n");
        AssertFailure(
            Preview(
                classified,
                [new Ra2AutomationSectionCreateOperation("NewThing", Ra2SectionKind.Warhead)],
                [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "NewThing", "Damage", "10")]),
            Ra2AutomationEditPreviewFailureKind.SectionClassificationMismatch);
    }

    [Fact]
    public void CreateSection_BlockedTrustFailsButInferredTrustProducesCaution()
    {
        Ra2AutomationDocumentSnapshot blocked = Editable(
            string.Empty,
            new SingleFieldProvider("Danger", "non-existent"));
        AssertFailure(
            Preview(
                blocked,
                [new Ra2AutomationSectionCreateOperation("New", Ra2SectionKind.Unknown)],
                [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "New", "Danger", "1")]),
            Ra2AutomationEditPreviewFailureKind.BlockedFieldTrust);

        Ra2AutomationDocumentSnapshot caution = Editable(
            string.Empty,
            new SingleFieldProvider("Maybe", "inferred"));
        Ra2AutomationEditPreviewResult result = Preview(
            caution,
            [new Ra2AutomationSectionCreateOperation("New", Ra2SectionKind.Unknown)],
            [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "New", "Maybe", "1")]);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(Ra2AutomationFieldTrustLevel.Inferred, Assert.Single(result.OperationPreviews).FieldTrustLevel);
        Assert.Equal(
            Ra2AutomationFieldAuthoringDisposition.Caution,
            Assert.Single(result.SectionCreationPreviews).AuthoringDisposition);
    }

    [Fact]
    public void Plan_OldConstructorKeepsEmptyRejectionAndNewOverloadUsesSharedWorkBudget()
    {
        Ra2AutomationDocumentSnapshot snapshot = Editable(string.Empty);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationEditPlan(
            Guid.NewGuid(), snapshot.DocumentId, snapshot.Version, snapshot.FieldRegistry.Revision,
            [], "old", "tests"));

        Ra2AutomationEditPlan sectionOnly = new(
            Guid.NewGuid(), snapshot.DocumentId, snapshot.Version, snapshot.FieldRegistry.Revision,
            [new Ra2AutomationSectionCreateOperation("New", Ra2SectionKind.Unknown)],
            [], "new", "tests");
        Assert.Single(sectionOnly.SectionCreations);
        Assert.Empty(sectionOnly.Operations);

        Ra2AutomationSectionCreateOperation creation = new("S", Ra2SectionKind.Unknown);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationEditPlan(
            Guid.NewGuid(), snapshot.DocumentId, snapshot.Version, snapshot.FieldRegistry.Revision,
            Enumerable.Repeat(creation, Ra2AutomationEditPlan.MaximumOperationCount + 1),
            [], "too many", "tests"));
    }

    private static Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        IEnumerable<Ra2AutomationSectionCreateOperation> creations,
        IEnumerable<Ra2AutomationEditOperation> operations)
        => new Ra2AutomationEditPreviewService().Preview(
            snapshot,
            new Ra2AutomationEditPlan(
                Guid.NewGuid(),
                snapshot.DocumentId,
                snapshot.Version,
                snapshot.FieldRegistry.Revision,
                creations,
                operations,
                "section creation",
                "tests"));

    private static Ra2AutomationDocumentSnapshot Editable(
        string text,
        IRa2FieldDefinitionProvider? provider = null)
        => new(
            Guid.NewGuid(),
            1,
            "rulesmd.ini",
            text,
            true,
            new Ra2AutomationFieldRegistrySnapshot(
                provider ?? new AutomationTestSupport.EmptyFieldDefinitionProvider(),
                7));

    private static string ApplyChanges(string source, IReadOnlyList<Ra2AutomationTextChange> changes)
    {
        string result = source;
        foreach (Ra2AutomationTextChange change in changes.OrderByDescending(change => change.Span.Start))
            result = result.Remove(change.Span.Start, change.Span.Length).Insert(change.Span.Start, change.NewText);
        return result;
    }

    private static void AssertFailure(
        Ra2AutomationEditPreviewResult result,
        Ra2AutomationEditPreviewFailureKind expected)
    {
        Assert.False(result.Succeeded);
        Assert.Equal(expected, result.FailureKind);
        Assert.Null(result.CandidateText);
        Assert.Empty(result.Changes);
        Assert.Empty(result.OperationPreviews);
        Assert.Empty(result.SectionCreationPreviews);
        Assert.Empty(result.AddedDiagnostics);
        Assert.Empty(result.RemovedDiagnostics);
    }

    private sealed class SingleFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition _definition;

        public SingleFieldProvider(string key, string quality)
        {
            _definition = new Ra2FieldDefinition(
                key,
                [Ra2SectionKind.Unknown],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.User,
                registryQuality: quality);
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            if (string.Equals(key, _definition.Key, StringComparison.OrdinalIgnoreCase))
            {
                definition = _definition;
                return true;
            }
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind) => [_definition];
        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => string.Equals(key, _definition.Key, StringComparison.OrdinalIgnoreCase);
    }
}
