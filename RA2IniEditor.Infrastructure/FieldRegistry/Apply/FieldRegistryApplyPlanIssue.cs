using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply;

internal sealed class FieldRegistryApplyPlanIssue
{
    public FieldRegistryApplyPlanIssue(
        FieldRegistryApplyPlanSeverity severity,
        string key,
        Ra2SectionKind appliesTo,
        string message)
    {
        Severity = severity;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        AppliesTo = appliesTo;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Apply plan issue message cannot be empty.", nameof(message))
            : message;
    }

    public FieldRegistryApplyPlanSeverity Severity { get; }

    public string Key { get; }

    public Ra2SectionKind AppliesTo { get; }

    public string Message { get; }
}
