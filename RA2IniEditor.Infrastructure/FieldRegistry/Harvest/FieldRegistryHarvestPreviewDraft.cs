using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestPreviewDraft
{
    public FieldRegistryHarvestPreviewDraft(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IReadOnlyList<FieldRegistryHarvestValidationIssue> issues)
    {
        Definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
        ErrorCount = issues.Count(issue => issue.Severity == FieldRegistryHarvestValidationSeverity.Error);
        WarningCount = issues.Count(issue => issue.Severity == FieldRegistryHarvestValidationSeverity.Warning);
        CanApplyInFuture = ErrorCount == 0;
    }

    public IReadOnlyList<Ra2FieldDefinition> Definitions { get; }

    public IReadOnlyList<FieldRegistryHarvestValidationIssue> Issues { get; }

    public int ErrorCount { get; }

    public int WarningCount { get; }

    public bool CanApplyInFuture { get; }
}
