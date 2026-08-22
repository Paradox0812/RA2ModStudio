using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryApplyPlanBuilderTests
{
    [Fact]
    public void AddedProducesAddForAppendOrUpdate()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("MyNewKey", Ra2SectionKind.Infantry)],
            [Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Add, item.OperationKind);
        Assert.Equal(FieldRegistryApplyTargetScope.Project, item.TargetScope);
        Assert.Equal(1, plan.AddCount);
        Assert.Equal(0, plan.ErrorCount);
        Assert.True(plan.CanApplyInFuture);
    }

    [Fact]
    public void AddedMissingPreviewDefinitionRejectsWithError()
    {
        FieldRegistryApplyPlan plan = Build(
            [],
            [Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Reject, item.OperationKind);
        Assert.Equal(1, plan.RejectCount);
        Assert.Equal(1, plan.ErrorCount);
        Assert.Contains(plan.Issues, issue =>
            issue.Severity == FieldRegistryApplyPlanSeverity.Error &&
            issue.Key == "MyNewKey" &&
            issue.Message.Contains("preview definition", StringComparison.OrdinalIgnoreCase));
        Assert.False(plan.CanApplyInFuture);
    }

    [Fact]
    public void SameProducesSkipWithInfo()
    {
        FieldRegistryApplyPlan plan = Build(
            [],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Same, FieldRegistryProvenanceScope.BuiltIn, "BuiltIn")],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Skip, item.OperationKind);
        Assert.Equal(1, plan.SkipCount);
        FieldRegistryApplyPlanIssue issue = Assert.Single(plan.Issues);
        Assert.Equal(FieldRegistryApplyPlanSeverity.Info, issue.Severity);
        Assert.True(plan.CanApplyInFuture);
    }

    [Fact]
    public void ChangedBuiltInAppendOrUpdateProjectAddsOverrideWithWarning()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.BuiltIn, "BuiltIn")],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Add, item.OperationKind);
        Assert.Equal(1, plan.AddCount);
        Assert.Equal(1, plan.WarningCount);
        Assert.Contains(plan.Issues, issue =>
            issue.Severity == FieldRegistryApplyPlanSeverity.Warning &&
            issue.Message.Contains("BuiltIn", StringComparison.Ordinal) &&
            issue.Message.Contains("override", StringComparison.OrdinalIgnoreCase));
        Assert.True(plan.CanApplyInFuture);
    }

    [Fact]
    public void ChangedGlobalAppendOrUpdateProjectAddsOverrideWithWarning()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Global, "global.fields.json")],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Add, item.OperationKind);
        Assert.Equal(1, plan.AddCount);
        Assert.Equal(1, plan.WarningCount);
        Assert.Contains(plan.Issues, issue =>
            issue.Severity == FieldRegistryApplyPlanSeverity.Warning &&
            issue.Message.Contains("Project", StringComparison.Ordinal) &&
            issue.Message.Contains("Global", StringComparison.Ordinal));
    }

    [Fact]
    public void ChangedGlobalAppendOrUpdateGlobalUpdates()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Global, "global.fields.json")],
            FieldRegistryApplyTargetScope.Global,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Update, item.OperationKind);
        Assert.Equal(1, plan.UpdateCount);
        Assert.Equal(0, plan.ErrorCount);
        Assert.True(plan.CanApplyInFuture);
    }

    [Fact]
    public void ChangedProjectAppendOrUpdateProjectUpdates()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Project, "project.fields.json")],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Update, item.OperationKind);
        Assert.Equal(1, plan.UpdateCount);
        Assert.Equal(0, plan.ErrorCount);
        Assert.True(plan.CanApplyInFuture);
    }

    [Fact]
    public void ChangedProjectAppendOrUpdateGlobalRejectsWithError()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Project, "project.fields.json")],
            FieldRegistryApplyTargetScope.Global,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Reject, item.OperationKind);
        Assert.Equal(1, plan.RejectCount);
        Assert.Equal(1, plan.ErrorCount);
        Assert.Contains(plan.Issues, issue =>
            issue.Severity == FieldRegistryApplyPlanSeverity.Error &&
            issue.Message.Contains("Project", StringComparison.Ordinal) &&
            issue.Message.Contains("Global", StringComparison.Ordinal) &&
            issue.Message.Contains("effective", StringComparison.OrdinalIgnoreCase));
        Assert.False(plan.CanApplyInFuture);
    }

    [Fact]
    public void ChangedAppendOnlySkipsWithWarning()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Global, "global.fields.json")],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOnly);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Skip, item.OperationKind);
        Assert.Equal(1, plan.SkipCount);
        Assert.Equal(1, plan.WarningCount);
        Assert.Contains(plan.Issues, issue =>
            issue.Severity == FieldRegistryApplyPlanSeverity.Warning &&
            issue.Message.Contains("AppendOnly", StringComparison.Ordinal));
        Assert.True(plan.CanApplyInFuture);
    }

    [Fact]
    public void ChangedSkipExistingSkipsWithInfo()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Global, "global.fields.json")],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.SkipExisting);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Skip, item.OperationKind);
        Assert.Equal(1, plan.SkipCount);
        FieldRegistryApplyPlanIssue issue = Assert.Single(plan.Issues);
        Assert.Equal(FieldRegistryApplyPlanSeverity.Info, issue.Severity);
        Assert.True(plan.CanApplyInFuture);
    }

    [Fact]
    public void InvalidRejectsWithError()
    {
        FieldRegistryApplyPlan plan = Build(
            [],
            [Row("BadKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Invalid)],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Reject, item.OperationKind);
        Assert.Equal(1, plan.RejectCount);
        Assert.Equal(1, plan.ErrorCount);
        Assert.False(plan.CanApplyInFuture);
    }

    [Fact]
    public void ConflictRejectsWithError()
    {
        FieldRegistryApplyPlan plan = Build(
            [],
            [Row("ConflictKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Conflict)],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Reject, item.OperationKind);
        Assert.Equal(1, plan.RejectCount);
        Assert.Equal(1, plan.ErrorCount);
        Assert.False(plan.CanApplyInFuture);
    }

    [Fact]
    public void PreviewDraftErrorBlocksFutureApplyAndRejectsRows()
    {
        FieldRegistryApplyPlan plan = Build(
            [Definition("Owner", Ra2SectionKind.Infantry)],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate,
            previewIssues:
            [
                new FieldRegistryHarvestValidationIssue(
                    "source",
                    1,
                    "Owner",
                    FieldRegistryHarvestValidationSeverity.Error,
                    "Invalid owner field.")
            ]);

        FieldRegistryApplyPlanItem item = Assert.Single(plan.Items);
        Assert.Equal(FieldRegistryApplyOperationKind.Reject, item.OperationKind);
        Assert.Equal(1, plan.RejectCount);
        Assert.Equal(1, plan.ErrorCount);
        Assert.False(plan.CanApplyInFuture);
    }

    [Fact]
    public void PreviewDraftErrorWithoutRowsProducesPlanLevelError()
    {
        FieldRegistryApplyPlan plan = Build(
            [],
            [],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate,
            previewIssues:
            [
                new FieldRegistryHarvestValidationIssue(
                    "source",
                    1,
                    null,
                    FieldRegistryHarvestValidationSeverity.Error,
                    "Invalid input.")
            ]);

        Assert.Empty(plan.Items);
        Assert.Equal(1, plan.ErrorCount);
        Assert.False(plan.CanApplyInFuture);
    }

    private static FieldRegistryApplyPlan Build(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IReadOnlyList<FieldRegistryHarvestDiffRow> rows,
        FieldRegistryApplyTargetScope targetScope,
        FieldRegistryApplyMode mode,
        IReadOnlyList<FieldRegistryHarvestValidationIssue>? previewIssues = null)
    {
        FieldRegistryHarvestPreviewDraft draft = new(definitions, previewIssues ?? []);
        FieldRegistryHarvestDiffResult diff = new(rows);
        FieldRegistryApplyPlanRequest request = new(draft, diff, targetScope, mode);
        return new FieldRegistryApplyPlanBuilder().BuildPlan(request);
    }

    private static Ra2FieldDefinition Definition(string key, Ra2SectionKind appliesTo)
    {
        return new Ra2FieldDefinition(
            key,
            [appliesTo],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            "Test definition.");
    }

    private static FieldRegistryHarvestDiffRow Row(
        string key,
        Ra2SectionKind appliesTo,
        FieldRegistryHarvestDiffKind kind,
        FieldRegistryProvenanceScope existingScope = FieldRegistryProvenanceScope.None,
        string existingSourceName = "None")
    {
        return new FieldRegistryHarvestDiffRow(
            key,
            appliesTo,
            kind,
            FieldEditorKind.Text,
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            Ra2FieldSourceKind.User,
            existingScope,
            existingSourceName,
            null,
            "Preview description.",
            "Existing description.",
            "Diff message.");
    }
}
