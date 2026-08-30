using System.Reflection;
using RA2IniEditor.Application.Automation.Experimental;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationProjectEditPreviewTests
{
    private static readonly Guid ProjectSessionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void Contracts_AreImmutableAndDefensivelyCopyCollections()
    {
        Ra2AutomationDocumentSnapshot first = Snapshot("11111111-1111-1111-1111-111111111111", "rulesmd.ini", "[E1]\nStrength=100\n");
        Ra2AutomationDocumentSnapshot second = Snapshot("22222222-2222-2222-2222-222222222222", "artmd.ini", "[E1]\nCameo=E1ICON\n");
        List<Ra2AutomationDocumentSnapshot> documents = [first, second];
        Ra2AutomationProjectSnapshot snapshot = new(ProjectSessionId, 7, "C:\\Mod", documents);
        documents.Clear();

        List<Ra2AutomationEditPlan> plans = [Plan(first, "Strength", "150"), Plan(second, "Cameo", "NEWICON")];
        Ra2AutomationProjectEditPlan plan = new(Guid.NewGuid(), ProjectSessionId, 7, plans, "  update project  ", "  tests  ");
        plans.Clear();

        Assert.Equal(2, snapshot.Documents.Count);
        Assert.Equal(2, plan.DocumentPlans.Count);
        Assert.Equal("update project", plan.Summary);
        Assert.Equal("tests", plan.Origin);
        Assert.All(
            new[] { typeof(Ra2AutomationProjectSnapshot), typeof(Ra2AutomationProjectEditPlan), typeof(Ra2AutomationProjectEditPreviewResult) }
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void Contracts_RejectInvalidIdentityDuplicateTargetsRegistryMismatchAndLimits()
    {
        Ra2AutomationDocumentSnapshot first = Snapshot("11111111-1111-1111-1111-111111111111", "rulesmd.ini", "[E1]\nStrength=100\n");
        Ra2AutomationDocumentSnapshot duplicatePath = Snapshot("22222222-2222-2222-2222-222222222222", "RULESMD.INI", "[E2]\nStrength=100\n");
        Ra2AutomationDocumentSnapshot differentRegistry = Snapshot("33333333-3333-3333-3333-333333333333", "artmd.ini", "[E1]\nCameo=E1ICON\n", 8);

        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectSnapshot(Guid.Empty, 0, "C:\\Mod", [first]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationProjectSnapshot(ProjectSessionId, -1, "C:\\Mod", [first]));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectSnapshot(ProjectSessionId, 0, " ", [first]));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectSnapshot(ProjectSessionId, 0, "C:\\Mod", [first, first]));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectSnapshot(ProjectSessionId, 0, "C:\\Mod", [first, duplicatePath]));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectSnapshot(ProjectSessionId, 0, "C:\\Mod", [first, differentRegistry]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationProjectSnapshot(ProjectSessionId, 0, "C:\\Mod", []));

        Ra2AutomationEditPlan leaf = Plan(first, "Strength", "150");
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectEditPlan(Guid.Empty, ProjectSessionId, 0, [leaf], "summary", "tests"));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectEditPlan(Guid.NewGuid(), Guid.Empty, 0, [leaf], "summary", "tests"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationProjectEditPlan(Guid.NewGuid(), ProjectSessionId, -1, [leaf], "summary", "tests"));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectEditPlan(Guid.NewGuid(), ProjectSessionId, 0, [leaf, leaf], "summary", "tests"));
    }

    [Fact]
    public void PreviewProject_ReturnsOrderedSuccessThroughCanonicalLeafService()
    {
        Ra2AutomationDocumentSnapshot rules = Snapshot("11111111-1111-1111-1111-111111111111", "rulesmd.ini", "[E1]\nStrength=100\n");
        Ra2AutomationDocumentSnapshot art = Snapshot("22222222-2222-2222-2222-222222222222", "artmd.ini", "[E1]\nCameo=E1ICON\n");
        Ra2AutomationProjectSnapshot snapshot = new(ProjectSessionId, 9, "C:\\Mod", [rules, art]);
        Ra2AutomationProjectEditPlan plan = new(
            Guid.NewGuid(), ProjectSessionId, 9,
            [Plan(art, "Cameo", "NEWICON"), Plan(rules, "Strength", "150")],
            "update two files", "tests");

        Ra2AutomationProjectEditPreviewResult result = new Ra2AutomationCapabilityGateway().PreviewProject(snapshot, plan);

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { art.DocumentId, rules.DocumentId }, result.DocumentPreviews.Select(preview => preview.DocumentId));
        Assert.Equal(2, result.TotalOperationCount);
        Assert.Equal(0, result.TotalSectionCreationCount);
        Assert.True(result.RequiresExplicitConfirmation);
        Assert.NotEqual(Guid.Empty, result.ProjectPreviewId);
    }

    [Fact]
    public void PreviewProject_RejectsStaleMissingAndLeafFailureWithoutPartialPayload()
    {
        Ra2AutomationDocumentSnapshot rules = Snapshot("11111111-1111-1111-1111-111111111111", "rulesmd.ini", "[E1]\nStrength=100\n");
        Ra2AutomationDocumentSnapshot art = Snapshot("22222222-2222-2222-2222-222222222222", "artmd.ini", "[E1]\nCameo=E1ICON\n");
        Ra2AutomationProjectSnapshot snapshot = new(ProjectSessionId, 9, "C:\\Mod", [rules, art]);
        Ra2AutomationCapabilityGateway gateway = new();

        Ra2AutomationProjectEditPlan stale = new(Guid.NewGuid(), ProjectSessionId, 8, [Plan(rules, "Strength", "150")], "stale", "tests");
        Assert.Equal(Ra2AutomationProjectEditPreviewFailureKind.StaleProject, gateway.PreviewProject(snapshot, stale).FailureKind);

        Ra2AutomationDocumentSnapshot outsider = Snapshot("33333333-3333-3333-3333-333333333333", "other.ini", "[X]\nValue=1\n");
        Ra2AutomationProjectEditPlan missing = new(Guid.NewGuid(), ProjectSessionId, 9, [Plan(outsider, "Value", "2")], "missing", "tests");
        Assert.Equal(Ra2AutomationProjectEditPreviewFailureKind.DocumentNotFound, gateway.PreviewProject(snapshot, missing).FailureKind);

        Ra2AutomationEditPlan badLeaf = new(
            Guid.NewGuid(), art.DocumentId, art.Version + 1, art.FieldRegistry.Revision,
            [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Cameo", "NEWICON")],
            "bad", "tests");
        Ra2AutomationProjectEditPlan partial = new(
            Guid.NewGuid(), ProjectSessionId, 9,
            [Plan(rules, "Strength", "150"), badLeaf], "partial", "tests");
        Ra2AutomationProjectEditPreviewResult failed = gateway.PreviewProject(snapshot, partial);
        Assert.Equal(Ra2AutomationProjectEditPreviewFailureKind.DocumentPreviewFailed, failed.FailureKind);
        Assert.Equal(Ra2AutomationEditPreviewFailureKind.StalePlanTarget, failed.FailedDocumentFailureKind);
        Assert.Empty(failed.DocumentPreviews);
        Assert.Equal(Guid.Empty, failed.ProjectPreviewId);
        Assert.False(failed.RequiresExplicitConfirmation);
    }

    [Fact]
    public void PreviewProject_CancellationHasNoPartialPayload()
    {
        Ra2AutomationDocumentSnapshot rules = Snapshot("11111111-1111-1111-1111-111111111111", "rulesmd.ini", "[E1]\nStrength=100\n");
        Ra2AutomationProjectSnapshot snapshot = new(ProjectSessionId, 1, "C:\\Mod", [rules]);
        Ra2AutomationProjectEditPlan plan = new(Guid.NewGuid(), ProjectSessionId, 1, [Plan(rules, "Strength", "150")], "cancel", "tests");
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AutomationProjectEditPreviewResult result = new Ra2AutomationCapabilityGateway().PreviewProject(snapshot, plan, source.Token);

        Assert.Equal(Ra2AutomationProjectEditPreviewFailureKind.Canceled, result.FailureKind);
        Assert.Empty(result.DocumentPreviews);
    }

    private static Ra2AutomationDocumentSnapshot Snapshot(string id, string path, string text, long registryRevision = 7)
        => new(
            Guid.Parse(id), 1, path, text, true,
            new Ra2AutomationFieldRegistrySnapshot(new AutomationTestSupport.EmptyFieldDefinitionProvider(), registryRevision));

    private static Ra2AutomationEditPlan Plan(Ra2AutomationDocumentSnapshot snapshot, string key, string value)
        => new(
            Guid.NewGuid(), snapshot.DocumentId, snapshot.Version, snapshot.FieldRegistry.Revision,
            [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "E1", key, value)],
            $"Set {key}", "tests");
}
