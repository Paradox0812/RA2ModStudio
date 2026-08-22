namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply;

internal sealed class FieldRegistryApplyPlan
{
    public FieldRegistryApplyPlan(
        FieldRegistryApplyTargetScope targetScope,
        FieldRegistryApplyMode mode,
        IReadOnlyList<FieldRegistryApplyPlanItem> items,
        IReadOnlyList<FieldRegistryApplyPlanIssue> issues)
    {
        TargetScope = targetScope;
        Mode = mode;
        Items = items ?? throw new ArgumentNullException(nameof(items));
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
        AddCount = items.Count(item => item.OperationKind == FieldRegistryApplyOperationKind.Add);
        UpdateCount = items.Count(item => item.OperationKind == FieldRegistryApplyOperationKind.Update);
        SkipCount = items.Count(item => item.OperationKind == FieldRegistryApplyOperationKind.Skip);
        RejectCount = items.Count(item => item.OperationKind == FieldRegistryApplyOperationKind.Reject);
        ErrorCount = issues.Count(issue => issue.Severity == FieldRegistryApplyPlanSeverity.Error);
        WarningCount = issues.Count(issue => issue.Severity == FieldRegistryApplyPlanSeverity.Warning);
        CanApplyInFuture = ErrorCount == 0;
    }

    public FieldRegistryApplyTargetScope TargetScope { get; }

    public FieldRegistryApplyMode Mode { get; }

    public IReadOnlyList<FieldRegistryApplyPlanItem> Items { get; }

    public IReadOnlyList<FieldRegistryApplyPlanIssue> Issues { get; }

    public int AddCount { get; }

    public int UpdateCount { get; }

    public int SkipCount { get; }

    public int RejectCount { get; }

    public int ErrorCount { get; }

    public int WarningCount { get; }

    public bool CanApplyInFuture { get; }
}
