using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AuthoringDiff;
using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AuthoringReviewProjectionTests
{
    [Fact]
    public void ProjectProjection_PreservesPreviewOrderAndExactCandidateText()
    {
        Fixture fixture = Fixture.CreateTwoDocument();
        Ra2AiEditProposal proposal = fixture.CreateProposal();

        Ra2AuthoringReviewProjection result = new Ra2AuthoringReviewProjectionBuilder().Build(proposal);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(["rulesmd.ini", "artmd.ini"], result.Documents.Select(document => document.DisplayName));
        Assert.Equal(
            proposal.ProjectPreview.DocumentPreviews.Select(preview => preview.CandidateText),
            result.Documents.Select(document => document.CandidateText));
        Assert.All(result.Documents, document => Assert.NotEmpty(document.ChangedLocations));
        Assert.Equal(Ra2AuthoringReviewOutlineKind.Modified, result.Documents[0].OutlineItems.First().Kind);
    }

    [Fact]
    public void ChangeMapping_MapsInsertReplacementAndDeletionToCandidateAnchors()
    {
        const string source = "[A]\nOne=1\nTwo=2\n";
        int one = source.IndexOf('1');
        int twoLine = source.IndexOf("Two=2", StringComparison.Ordinal);
        const string candidate = "[A]\nOne=10\nAdded=yes\n";
        Ra2AutomationTextChange[] changes =
        [
            Change(one, 1, "10"),
            Change(twoLine, "Two=2\n".Length, "Added=yes\n")
        ];

        bool mapped = Ra2AuthoringDiffProjectionBuilder.TryMapChanges(
            source, candidate, changes, CancellationToken.None, out IReadOnlyList<Ra2AuthoringMappedChange> locations, out Ra2AuthoringDiffProjection? failure);

        Assert.True(mapped, failure?.Message);
        Assert.Equal(2, locations.Count);
        Assert.Equal("10", candidate.Substring(locations[0].CandidateSpan.Start, locations[0].CandidateSpan.Length));
        Assert.Equal("Added=yes\n", candidate.Substring(locations[1].CandidateSpan.Start, locations[1].CandidateSpan.Length));
        Assert.True(locations[1].RemovedLineCount > 0);
    }

    [Fact]
    public void DirectReference_AddsReadonlyRelatedSectionWithoutChangingExecutableOutline()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ReviewRelation"));
        SchemaProvider provider = new();
        Ra2AutomationDocumentSnapshot rules = Snapshot(
            Guid.NewGuid(), Path.Combine(root, "rulesmd.ini"),
            "[E1]\nStrength=100\nPrimary=E1Gun\n\n[E1Gun]\nDamage=25\nProjectile=Invisible\n",
            provider);
        Guid sessionId = Guid.NewGuid();
        Ra2AutomationProjectSnapshot snapshot = new(sessionId, 1, root, [rules]);
        Ra2AutomationProjectEditPlan plan = new(
            Guid.NewGuid(), sessionId, 1, [Plan(rules, "E1", "Strength", "150")], "one file", "tests");
        Ra2ProjectEditPreview preview = new Ra2ProjectEditPreviewService().Preview(snapshot, plan);
        Ra2AiEditProposal proposal = Ra2AiEditProposal.FromProject(preview, null, Ra2AiEditProposalApplyPolicy.Normal, "risk");

        Ra2AuthoringReviewProjection result = new Ra2AuthoringReviewProjectionBuilder().Build(proposal);

        Assert.True(result.Succeeded, result.Message);
        Ra2AuthoringReviewDocument document = Assert.Single(result.Documents);
        Ra2AuthoringReviewOutlineItem related = Assert.Single(document.OutlineItems, item => item.Kind == Ra2AuthoringReviewOutlineKind.Related);
        Assert.Equal("E1Gun", related.SectionName);
        Assert.False(related.IsExecutableChange);
        Assert.Contains("[E1Gun]", related.ContextText, StringComparison.Ordinal);
        Assert.True(result.Diff.Succeeded);
    }

    [Fact]
    public void SchemaDeclaredReference_ResolvesAcrossCapturedProjectDocumentsOnly()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ReviewCrossDocument"));
        SchemaProvider provider = new();
        Ra2AutomationDocumentSnapshot rules = Snapshot(Guid.NewGuid(), Path.Combine(root, "rulesmd.ini"), "[HTNK]\nStrength=100\nImage=HTNKART\n", provider);
        Ra2AutomationDocumentSnapshot art = Snapshot(Guid.NewGuid(), Path.Combine(root, "artmd.ini"), "[HTNKART]\nImage=HTNKBODY\nCameo=HTNKICON\n", provider);
        Guid sessionId = Guid.NewGuid();
        Ra2AutomationProjectSnapshot snapshot = new(sessionId, 1, root, [rules, art]);
        Ra2AutomationProjectEditPlan plan = new(Guid.NewGuid(), sessionId, 1, [Plan(rules, "HTNK", "Strength", "150")], "rules only", "tests");
        Ra2ProjectEditPreview preview = new Ra2ProjectEditPreviewService().Preview(snapshot, plan);

        Ra2AuthoringReviewProjection result = new Ra2AuthoringReviewProjectionBuilder().Build(
            Ra2AiEditProposal.FromProject(preview, null, Ra2AiEditProposalApplyPolicy.Normal, "risk"));

        Assert.True(result.Succeeded, result.Message);
        Ra2AuthoringReviewOutlineItem related = Assert.Single(result.Documents[0].OutlineItems, item => item.Kind == Ra2AuthoringReviewOutlineKind.Related);
        Assert.Equal("HTNKART", related.SectionName);
        Assert.Equal("artmd.ini", related.ContextFileName);
        Assert.Contains("Cameo=HTNKICON", related.ContextText, StringComparison.Ordinal);
        Assert.Single(result.Documents);
    }

    [Fact]
    public void AmbiguousReference_IsUnresolvedAndDoesNotInvalidateResultOrDiff()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ReviewAmbiguous"));
        SchemaProvider provider = new();
        Ra2AutomationDocumentSnapshot rules = Snapshot(Guid.NewGuid(), Path.Combine(root, "rulesmd.ini"), "[E1]\nStrength=100\nPrimary=Gun\n\n[Gun]\nDamage=1\n\n[Gun]\nDamage=2\n", provider);
        Guid sessionId = Guid.NewGuid();
        Ra2AutomationProjectSnapshot snapshot = new(sessionId, 1, root, [rules]);
        Ra2AutomationProjectEditPlan plan = new(Guid.NewGuid(), sessionId, 1, [Plan(rules, "E1", "Strength", "150")], "one", "tests");
        Ra2ProjectEditPreview preview = new Ra2ProjectEditPreviewService().Preview(snapshot, plan);

        Ra2AuthoringReviewProjection result = new Ra2AuthoringReviewProjectionBuilder().Build(
            Ra2AiEditProposal.FromProject(preview, null, Ra2AiEditProposalApplyPolicy.Normal, "risk"));

        Assert.True(result.Succeeded, result.Message);
        Assert.True(result.Diff.Succeeded);
        Assert.Contains(result.Documents[0].OutlineItems, item => item.Kind == Ra2AuthoringReviewOutlineKind.Unresolved && item.SectionName == "Gun");
        Assert.NotEqual(Ra2AuthoringRelationState.Available, result.Documents[0].RelationState);
    }

    [Fact]
    public void MissingReference_IsReportedWithoutGuessing()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ReviewMissing"));
        SchemaProvider provider = new();
        Ra2AutomationDocumentSnapshot rules = Snapshot(Guid.NewGuid(), Path.Combine(root, "rulesmd.ini"), "[E1]\nStrength=100\nPrimary=MissingGun\n", provider);
        Guid sessionId = Guid.NewGuid();
        Ra2AutomationProjectSnapshot snapshot = new(sessionId, 1, root, [rules]);
        Ra2AutomationProjectEditPlan plan = new(Guid.NewGuid(), sessionId, 1, [Plan(rules, "E1", "Strength", "150")], "one", "tests");
        Ra2ProjectEditPreview preview = new Ra2ProjectEditPreviewService().Preview(snapshot, plan);

        Ra2AuthoringReviewProjection result = new Ra2AuthoringReviewProjectionBuilder().Build(
            Ra2AiEditProposal.FromProject(preview, null, Ra2AiEditProposalApplyPolicy.Normal, "risk"));

        Assert.True(result.Succeeded, result.Message);
        Ra2AuthoringReviewOutlineItem unresolved = Assert.Single(result.Documents[0].OutlineItems, item => item.Kind == Ra2AuthoringReviewOutlineKind.Unresolved);
        Assert.Equal("MissingGun", unresolved.SectionName);
        Assert.Null(unresolved.ContextText);
    }

    [Fact]
    public void RelationBudget_IsBoundedAndMarksProjectionPartial()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ReviewLimit"));
        SchemaProvider provider = new();
        string[] names = Enumerable.Range(0, Ra2AuthoringReviewProjectionBuilder.MaximumRelatedItems + 2).Select(index => $"Unit{index}").ToArray();
        string text = $"[Spawner]\nStrength=100\nDeliver.Types={string.Join(',', names)}\n\n" +
            string.Join("\n", names.Select(name => $"[{name}]\nStrength=1\n"));
        Ra2AutomationDocumentSnapshot rules = Snapshot(Guid.NewGuid(), Path.Combine(root, "rulesmd.ini"), text, provider);
        Guid sessionId = Guid.NewGuid();
        Ra2AutomationProjectSnapshot snapshot = new(sessionId, 1, root, [rules]);
        Ra2AutomationProjectEditPlan plan = new(Guid.NewGuid(), sessionId, 1, [Plan(rules, "Spawner", "Strength", "150")], "many", "tests");
        Ra2ProjectEditPreview preview = new Ra2ProjectEditPreviewService().Preview(snapshot, plan);

        Ra2AuthoringReviewProjection result = new Ra2AuthoringReviewProjectionBuilder().Build(
            Ra2AiEditProposal.FromProject(preview, null, Ra2AiEditProposalApplyPolicy.Normal, "risk"));

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(Ra2AuthoringReviewProjectionBuilder.MaximumRelatedItems,
            result.Documents[0].OutlineItems.Count(item => item.Kind == Ra2AuthoringReviewOutlineKind.Related));
        Assert.Equal(Ra2AuthoringRelationState.Partial, result.Documents[0].RelationState);
    }

    [Fact]
    public void Cancellation_ReturnsTypedFailureAndNoPartialDocuments()
    {
        Fixture fixture = Fixture.CreateTwoDocument();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2AuthoringReviewProjection result = new Ra2AuthoringReviewProjectionBuilder().Build(fixture.CreateProposal(), cancellation.Token);

        Assert.Equal(Ra2AuthoringReviewFailureKind.Canceled, result.FailureKind);
        Assert.Empty(result.Documents);
    }

    private sealed class Fixture
    {
        private Fixture(Ra2ProjectEditPreview preview) => Preview = preview;
        public Ra2ProjectEditPreview Preview { get; }
        public Ra2AiEditProposal CreateProposal() => Ra2AiEditProposal.FromProject(Preview, null, Ra2AiEditProposalApplyPolicy.Normal, "risk");

        public static Fixture CreateTwoDocument()
        {
            string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ReviewProjection"));
            SchemaProvider provider = new();
            Ra2AutomationDocumentSnapshot rules = Snapshot(Guid.NewGuid(), Path.Combine(root, "rulesmd.ini"), "[E1]\nStrength=100\n", provider);
            Ra2AutomationDocumentSnapshot art = Snapshot(Guid.NewGuid(), Path.Combine(root, "artmd.ini"), "[E1ART]\nImage=OLD\n", provider);
            Guid sessionId = Guid.NewGuid();
            Ra2AutomationProjectSnapshot snapshot = new(sessionId, 2, root, [rules, art]);
            Ra2AutomationProjectEditPlan plan = new(
                Guid.NewGuid(), sessionId, 2,
                [Plan(rules, "E1", "Strength", "150"), Plan(art, "E1ART", "Image", "NEW")],
                "two files", "tests");
            return new Fixture(new Ra2ProjectEditPreviewService().Preview(snapshot, plan));
        }
    }

    private sealed class SchemaProvider : IRa2FieldDefinitionProvider
    {
        private static readonly IReadOnlyList<Ra2FieldDefinition> Fields =
        [
            Definition("Primary", Ra2FieldValueKind.Reference),
            Definition("Projectile", Ra2FieldValueKind.Reference),
            Definition("Image", Ra2FieldValueKind.Reference),
            Definition("Cameo", Ra2FieldValueKind.Reference),
            Definition("Deliver.Types", Ra2FieldValueKind.ReferenceList),
            Definition("Strength", Ra2FieldValueKind.Integer)
        ];

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = Fields.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind) => Fields;
        public bool IsKnownField(Ra2SectionKind sectionKind, string key) => TryGetField(sectionKind, key, out _);

        private static Ra2FieldDefinition Definition(string key, Ra2FieldValueKind kind)
            => new(key, [], FieldEditorKind.Text, Ra2FieldSourceKind.User, valueMetadata: new Ra2FieldValueMetadata(kind));
    }

    private static Ra2AutomationDocumentSnapshot Snapshot(Guid id, string path, string text, IRa2FieldDefinitionProvider provider)
        => new(id, 1, path, text, true, new Ra2AutomationFieldRegistrySnapshot(provider, 7));

    private static Ra2AutomationEditPlan Plan(Ra2AutomationDocumentSnapshot snapshot, string section, string key, string value)
        => new(Guid.NewGuid(), snapshot.DocumentId, snapshot.Version, snapshot.FieldRegistry.Revision,
            [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, section, key, value)], "change", "tests");

    private static Ra2AutomationTextChange Change(int start, int length, string newText)
        => new(new Ra2AutomationTextSpan(start, length), newText, "test");
}
