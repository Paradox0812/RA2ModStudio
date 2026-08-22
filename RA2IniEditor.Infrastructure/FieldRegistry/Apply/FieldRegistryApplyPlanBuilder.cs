using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply;

internal sealed class FieldRegistryApplyPlanBuilder : IFieldRegistryApplyPlanBuilder
{
    private static readonly Ra2FieldDefinition RejectedPlaceholderDefinition = new(
        "__RejectedPreviewDefinition",
        [Ra2SectionKind.Unknown],
        FieldEditorKind.Text,
        Ra2FieldSourceKind.Unknown,
        "Rejected placeholder for preview-only apply plan items.");

    public FieldRegistryApplyPlan BuildPlan(FieldRegistryApplyPlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<FieldRegistryApplyPlanItem> items = new();
        List<FieldRegistryApplyPlanIssue> issues = new();

        if (request.PreviewDraft.ErrorCount > 0)
        {
            issues.Add(new FieldRegistryApplyPlanIssue(
                FieldRegistryApplyPlanSeverity.Error,
                string.Empty,
                Ra2SectionKind.Unknown,
                "Preview draft contains validation errors; apply is blocked."));

            foreach (FieldRegistryHarvestDiffRow row in request.DiffResult.Rows)
            {
                items.Add(CreateItem(
                    request,
                    row,
                    FieldRegistryApplyOperationKind.Reject,
                    TryFindPreviewDefinition(request.PreviewDraft, row) ?? RejectedPlaceholderDefinition,
                    "Rejected because the preview draft contains validation errors."));
            }

            return CreatePlan(request, items, issues);
        }

        foreach (FieldRegistryHarvestDiffRow row in request.DiffResult.Rows)
        {
            Ra2FieldDefinition? previewDefinition = TryFindPreviewDefinition(request.PreviewDraft, row);
            FieldRegistryApplyPlanItem item = BuildItem(request, row, previewDefinition, issues);
            items.Add(item);
        }

        return CreatePlan(request, items, issues);
    }

    private static FieldRegistryApplyPlanItem BuildItem(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        Ra2FieldDefinition? previewDefinition,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        switch (row.Kind)
        {
            case FieldRegistryHarvestDiffKind.Added:
                if (previewDefinition is null)
                    return RejectMissingPreviewDefinition(request, row, issues);

                return CreateItem(
                    request,
                    row,
                    FieldRegistryApplyOperationKind.Add,
                    previewDefinition,
                    "New field candidate will be added to the selected target scope.");

            case FieldRegistryHarvestDiffKind.Same:
                issues.Add(new FieldRegistryApplyPlanIssue(
                    FieldRegistryApplyPlanSeverity.Info,
                    row.Key,
                    row.AppliesTo,
                    "Already matches effective registry."));
                return CreateItem(
                    request,
                    row,
                    FieldRegistryApplyOperationKind.Skip,
                    previewDefinition ?? RejectedPlaceholderDefinition,
                    "Already matches effective registry.");

            case FieldRegistryHarvestDiffKind.Changed:
                if (previewDefinition is null)
                    return RejectMissingPreviewDefinition(request, row, issues);

                return BuildChangedItem(request, row, previewDefinition, issues);

            case FieldRegistryHarvestDiffKind.Invalid:
                issues.Add(new FieldRegistryApplyPlanIssue(
                    FieldRegistryApplyPlanSeverity.Error,
                    row.Key,
                    row.AppliesTo,
                    "Invalid diff row cannot be applied."));
                return CreateItem(
                    request,
                    row,
                    FieldRegistryApplyOperationKind.Reject,
                    previewDefinition ?? RejectedPlaceholderDefinition,
                    "Invalid diff row cannot be applied.");

            case FieldRegistryHarvestDiffKind.Conflict:
                issues.Add(new FieldRegistryApplyPlanIssue(
                    FieldRegistryApplyPlanSeverity.Error,
                    row.Key,
                    row.AppliesTo,
                    "Conflicting diff row cannot be applied."));
                return CreateItem(
                    request,
                    row,
                    FieldRegistryApplyOperationKind.Reject,
                    previewDefinition ?? RejectedPlaceholderDefinition,
                    "Conflicting diff row cannot be applied.");

            default:
                issues.Add(new FieldRegistryApplyPlanIssue(
                    FieldRegistryApplyPlanSeverity.Error,
                    row.Key,
                    row.AppliesTo,
                    "Unknown diff row kind cannot be applied."));
                return CreateItem(
                    request,
                    row,
                    FieldRegistryApplyOperationKind.Reject,
                    previewDefinition ?? RejectedPlaceholderDefinition,
                    "Unknown diff row kind cannot be applied.");
        }
    }

    private static FieldRegistryApplyPlanItem BuildChangedItem(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        Ra2FieldDefinition previewDefinition,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        if (request.Mode == FieldRegistryApplyMode.AppendOnly)
        {
            issues.Add(new FieldRegistryApplyPlanIssue(
                FieldRegistryApplyPlanSeverity.Warning,
                row.Key,
                row.AppliesTo,
                "Existing field differs and mode is AppendOnly."));
            return CreateItem(
                request,
                row,
                FieldRegistryApplyOperationKind.Skip,
                previewDefinition,
                "Existing field differs and mode is AppendOnly.");
        }

        if (request.Mode == FieldRegistryApplyMode.SkipExisting)
        {
            issues.Add(new FieldRegistryApplyPlanIssue(
                FieldRegistryApplyPlanSeverity.Info,
                row.Key,
                row.AppliesTo,
                "Existing field differs and mode is SkipExisting."));
            return CreateItem(
                request,
                row,
                FieldRegistryApplyOperationKind.Skip,
                previewDefinition,
                "Existing field differs and mode is SkipExisting.");
        }

        return row.ExistingScope switch
        {
            FieldRegistryProvenanceScope.None => CreateItem(
                request,
                row,
                FieldRegistryApplyOperationKind.Add,
                previewDefinition,
                "Changed field has no existing definition in the effective registry and will be added."),

            FieldRegistryProvenanceScope.BuiltIn => BuildBuiltInOverrideItem(request, row, previewDefinition, issues),
            FieldRegistryProvenanceScope.Global => BuildGlobalChangedItem(request, row, previewDefinition, issues),
            FieldRegistryProvenanceScope.Project => BuildProjectChangedItem(request, row, previewDefinition, issues),
            _ => BuildUnknownChangedItem(request, row, previewDefinition, issues)
        };
    }

    private static FieldRegistryApplyPlanItem BuildBuiltInOverrideItem(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        Ra2FieldDefinition previewDefinition,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        issues.Add(new FieldRegistryApplyPlanIssue(
            FieldRegistryApplyPlanSeverity.Warning,
            row.Key,
            row.AppliesTo,
            "BuiltIn definitions are not modified directly; this would create an override in the selected target scope."));
        return CreateItem(
            request,
            row,
            FieldRegistryApplyOperationKind.Add,
            previewDefinition,
            "Create an override in the selected target scope; BuiltIn is not modified.");
    }

    private static FieldRegistryApplyPlanItem BuildGlobalChangedItem(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        Ra2FieldDefinition previewDefinition,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        if (request.TargetScope == FieldRegistryApplyTargetScope.Global)
        {
            return CreateItem(
                request,
                row,
                FieldRegistryApplyOperationKind.Update,
                previewDefinition,
                "Update the existing Global field definition.");
        }

        issues.Add(new FieldRegistryApplyPlanIssue(
            FieldRegistryApplyPlanSeverity.Warning,
            row.Key,
            row.AppliesTo,
            "Applying to Project will create a Project override that takes precedence over the existing Global definition."));
        return CreateItem(
            request,
            row,
            FieldRegistryApplyOperationKind.Add,
            previewDefinition,
            "Create a Project override that takes precedence over Global.");
    }

    private static FieldRegistryApplyPlanItem BuildProjectChangedItem(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        Ra2FieldDefinition previewDefinition,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        if (request.TargetScope == FieldRegistryApplyTargetScope.Project)
        {
            return CreateItem(
                request,
                row,
                FieldRegistryApplyOperationKind.Update,
                previewDefinition,
                "Update the existing Project field definition.");
        }

        issues.Add(new FieldRegistryApplyPlanIssue(
            FieldRegistryApplyPlanSeverity.Error,
            row.Key,
            row.AppliesTo,
            "A Project definition currently has higher priority. Applying to Global will not change the effective result."));
        return CreateItem(
            request,
            row,
            FieldRegistryApplyOperationKind.Reject,
            previewDefinition,
            "Rejected because Global target cannot override the current Project effective definition.");
    }

    private static FieldRegistryApplyPlanItem BuildUnknownChangedItem(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        Ra2FieldDefinition previewDefinition,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        issues.Add(new FieldRegistryApplyPlanIssue(
            FieldRegistryApplyPlanSeverity.Error,
            row.Key,
            row.AppliesTo,
            "Existing field provenance is unknown; apply planning rejects this row."));
        return CreateItem(
            request,
            row,
            FieldRegistryApplyOperationKind.Reject,
            previewDefinition,
            "Rejected because existing field provenance is unknown.");
    }

    private static FieldRegistryApplyPlanItem RejectMissingPreviewDefinition(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        issues.Add(new FieldRegistryApplyPlanIssue(
            FieldRegistryApplyPlanSeverity.Error,
            row.Key,
            row.AppliesTo,
            "No matching preview definition was found for this diff row."));
        return CreateItem(
            request,
            row,
            FieldRegistryApplyOperationKind.Reject,
            RejectedPlaceholderDefinition,
            "Rejected because no matching preview definition was found.");
    }

    private static FieldRegistryApplyPlanItem CreateItem(
        FieldRegistryApplyPlanRequest request,
        FieldRegistryHarvestDiffRow row,
        FieldRegistryApplyOperationKind operationKind,
        Ra2FieldDefinition previewDefinition,
        string message)
    {
        return new FieldRegistryApplyPlanItem(
            row.Key,
            row.AppliesTo,
            operationKind,
            request.TargetScope,
            row.ExistingScope,
            row.ExistingSourceName,
            previewDefinition,
            message);
    }

    private static FieldRegistryApplyPlan CreatePlan(
        FieldRegistryApplyPlanRequest request,
        List<FieldRegistryApplyPlanItem> items,
        List<FieldRegistryApplyPlanIssue> issues)
    {
        return new FieldRegistryApplyPlan(
            request.TargetScope,
            request.Mode,
            Array.AsReadOnly(items.ToArray()),
            Array.AsReadOnly(issues.ToArray()));
    }

    private static Ra2FieldDefinition? TryFindPreviewDefinition(
        FieldRegistryHarvestPreviewDraft previewDraft,
        FieldRegistryHarvestDiffRow row)
    {
        return previewDraft.Definitions.FirstOrDefault(definition =>
            string.Equals(definition.Key, row.Key, StringComparison.OrdinalIgnoreCase) &&
            definition.AppliesTo.Contains(row.AppliesTo));
    }
}
