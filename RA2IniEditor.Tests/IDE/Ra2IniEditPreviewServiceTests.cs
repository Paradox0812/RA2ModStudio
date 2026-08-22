using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniEditPreviewServiceTests
{
    private readonly Ra2EditableDocumentSessionService _sessionService = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());
    private readonly Ra2IniEditPreviewService _service = new(
        new Ra2IniLanguageAnalysisService(),
        new Ra2AddPropertyInsertPlanner());

    [Fact]
    public void Preview_ReplacesOnlyValueAndPreservesFormattingAndComment()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]\nStrength = 100 ; keep");

        Ra2IniEditPreview preview = _service.Preview(
            snapshot,
            Plan(snapshot, Op(
                Ra2IniEditOperationKind.ReplaceFieldValue,
                "E1",
                "Strength",
                "125")));

        Assert.True(preview.Succeeded);
        Assert.Equal("[E1]\nStrength = 125 ; keep", preview.CandidateText);
        Assert.Equal(snapshot.Text, preview.Snapshot.Text);
        Assert.Equal(snapshot.FieldRegistry.Revision, preview.AutomationResult.FieldRegistryRevision);
        Assert.Equal(snapshot.EditRevision, preview.AutomationResult.Version);
        Assert.Equal(Ra2IniEditOperationOutcomeKind.Replaced, Assert.Single(preview.OperationPreviews).OutcomeKind);
    }

    [Fact]
    public void Preview_UpsertInsertsAfterLastFieldAndBeforeTrailingComment()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot(
            "[E1]\nStrength=100\n; trailing\n[NEXT]\nName=Next");

        Ra2IniEditPreview preview = _service.Preview(
            snapshot,
            Plan(snapshot, Op(
                Ra2IniEditOperationKind.UpsertField,
                "E1",
                "Armor",
                "steel")));

        Assert.True(preview.Succeeded);
        Assert.Equal(
            "[E1]\nStrength=100\nArmor=steel\n; trailing\n[NEXT]\nName=Next",
            preview.CandidateText);
    }

    [Fact]
    public void Preview_CoalescesSamePointInsertionsInPlanOrder()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]\r\nStrength=100\r\n[NEXT]");

        Ra2IniEditPreview preview = _service.Preview(
            snapshot,
            Plan(
                snapshot,
                Op(Ra2IniEditOperationKind.UpsertField, "E1", "Armor", "steel"),
                Op(Ra2IniEditOperationKind.UpsertField, "E1", "Primary", "Gun")));

        Assert.True(preview.Succeeded);
        Assert.Equal(
            "[E1]\r\nStrength=100\r\nArmor=steel\r\nPrimary=Gun\r\n[NEXT]",
            preview.CandidateText);
        Assert.Single(preview.ChangeSet!.Changes);
    }

    [Fact]
    public void Preview_InsertsIntoEmptySectionAtEndOfFileWithoutFinalNewline()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]");

        Ra2IniEditPreview preview = _service.Preview(
            snapshot,
            Plan(snapshot, Op(
                Ra2IniEditOperationKind.UpsertField,
                "E1",
                "Strength",
                "100")));

        Assert.Equal("[E1]\nStrength=100", preview.CandidateText);
    }

    [Theory]
    [InlineData("[E1]\rStrength=100\r[NEXT]", "[E1]\rStrength=100\rArmor=steel\r[NEXT]")]
    [InlineData("[E1]\r\nStrength=100\n[NEXT]", "[E1]\r\nStrength=100\nArmor=steel\n[NEXT]")]
    public void Preview_InsertionUsesTheAnchorLinesExistingLineBreak(
        string source,
        string expected)
    {
        Ra2AuthoringSnapshot snapshot = Snapshot(source);

        Ra2IniEditPreview preview = _service.Preview(
            snapshot,
            Plan(snapshot, Op(
                Ra2IniEditOperationKind.UpsertField,
                "E1",
                "Armor",
                "steel")));

        Assert.Equal(expected, preview.CandidateText);
    }

    [Fact]
    public void Preview_RepeatedPlanningKeepsSemanticPayloadStable()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]\nStrength=100");
        Ra2IniEditPlan plan = Plan(snapshot, Op(
            Ra2IniEditOperationKind.UpsertField,
            "E1",
            "Armor",
            "steel"));

        Ra2IniEditPreview first = _service.Preview(snapshot, plan);
        Ra2IniEditPreview second = _service.Preview(snapshot, plan);

        Assert.NotEqual(first.PreviewId, second.PreviewId);
        Assert.Equal(first.CandidateText, second.CandidateText);
        Assert.Equal(
            first.ChangeSet!.Changes.Select(change => (change.Span, change.NewText)),
            second.ChangeSet!.Changes.Select(change => (change.Span, change.NewText)));
        Assert.Equal(
            first.OperationPreviews.Select(item => (item.OutcomeKind, item.AffectedOriginalSpan)),
            second.OperationPreviews.Select(item => (item.OutcomeKind, item.AffectedOriginalSpan)));
    }

    [Fact]
    public void Preview_HandlesExistingEmptyValue()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]\nStrength=  ; keep");

        Ra2IniEditPreview preview = _service.Preview(
            snapshot,
            Plan(snapshot, Op(
                Ra2IniEditOperationKind.ReplaceFieldValue,
                "E1",
                "Strength",
                "100")));

        Assert.Equal("[E1]\nStrength=100  ; keep", preview.CandidateText);
    }

    [Fact]
    public void Preview_RejectsDuplicateSectionAndDuplicateKey()
    {
        Ra2AuthoringSnapshot duplicateSection = Snapshot("[E1]\nA=1\n[E1]\nB=2");
        Ra2IniEditPreview sectionResult = _service.Preview(
            duplicateSection,
            Plan(duplicateSection, Op(
                Ra2IniEditOperationKind.UpsertField,
                "E1",
                "Strength",
                "100")));

        Ra2AuthoringSnapshot duplicateKey = Snapshot("[E1]\nStrength=100\nStrength=200");
        Ra2IniEditPreview keyResult = _service.Preview(
            duplicateKey,
            Plan(duplicateKey, Op(
                Ra2IniEditOperationKind.UpsertField,
                "E1",
                "Strength",
                "300")));

        Assert.Equal(Ra2IniEditPreviewFailureKind.AmbiguousSection, sectionResult.FailureKind);
        Assert.Equal(Ra2IniEditPreviewFailureKind.AmbiguousField, keyResult.FailureKind);
    }

    [Fact]
    public void Preview_RejectsMissingReplaceAndConflictingTargets()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]");
        Ra2IniEditPreview missing = _service.Preview(
            snapshot,
            Plan(snapshot, Op(
                Ra2IniEditOperationKind.ReplaceFieldValue,
                "E1",
                "Strength",
                "100")));
        Ra2IniEditPreview conflict = _service.Preview(
            snapshot,
            Plan(
                snapshot,
                Op(Ra2IniEditOperationKind.UpsertField, "E1", "Strength", "100"),
                Op(Ra2IniEditOperationKind.UpsertField, "e1", "strength", "200")));

        Assert.Equal(Ra2IniEditPreviewFailureKind.FieldNotFound, missing.FailureKind);
        Assert.Equal(Ra2IniEditPreviewFailureKind.ConflictingOperations, conflict.FailureKind);
    }

    [Fact]
    public void Preview_RejectsStalePlanAndCancellation()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]");
        Ra2IniEditOperation operation = new(
            Ra2IniEditOperationKind.UpsertField,
            "E1",
            "Strength",
            "100");
        Ra2IniEditPlan stalePlan = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            snapshot.EditRevision,
            snapshot.FieldRegistry.Revision,
            [operation],
            "Stale",
            "Test");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Assert.Equal(
            Ra2IniEditPreviewFailureKind.StalePlanTarget,
            _service.Preview(snapshot, stalePlan).FailureKind);
        Assert.Equal(
            Ra2IniEditPreviewFailureKind.Canceled,
            _service.Preview(snapshot, Plan(snapshot, operation), cancellation.Token).FailureKind);
    }

    [Fact]
    public void Preview_UnknownCustomFieldRemainsAllowedAndVisibleAsEvidence()
    {
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]");

        Ra2IniEditPreview preview = _service.Preview(
            snapshot,
            Plan(snapshot, Op(
                Ra2IniEditOperationKind.UpsertField,
                "E1",
                "MyCustomField",
                "Value")));

        Ra2IniEditOperationPreview evidence = Assert.Single(preview.OperationPreviews);
        Assert.True(preview.Succeeded);
        Assert.False(evidence.IsKnownField);
        Assert.Equal(Ra2FieldTrustLevel.Unknown, evidence.FieldTrustLevel);
        Assert.True(preview.RequiresExplicitConfirmation);
    }

    private Ra2AuthoringSnapshot Snapshot(string text)
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing("rulesmd.ini", text);
        Ra2FieldRegistryProviderSnapshot registry = new(
            new BuiltInRa2FieldDefinitionProvider(),
            revision: 1);
        return Assert.IsType<Ra2AuthoringSnapshot>(
            Ra2AuthoringSnapshot.Capture(session, text, string.Empty, registry).Snapshot);
    }

    private static Ra2IniEditPlan Plan(
        Ra2AuthoringSnapshot snapshot,
        params Ra2IniEditOperation[] operations)
        => new(
            Guid.NewGuid(),
            snapshot.DocumentId,
            snapshot.EditRevision,
            snapshot.FieldRegistry.Revision,
            operations,
            "Test edit plan",
            "Tests");

    private static Ra2IniEditOperation Op(
        Ra2IniEditOperationKind kind,
        string section,
        string key,
        string value)
        => new(kind, section, key, value);
}
