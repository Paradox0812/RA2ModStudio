namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestValidationIssue
{
    public FieldRegistryHarvestValidationIssue(
        string sourceName,
        int lineNumber,
        string? key,
        FieldRegistryHarvestValidationSeverity severity,
        string message)
    {
        SourceName = sourceName;
        LineNumber = lineNumber;
        Key = string.IsNullOrWhiteSpace(key) ? null : key.Trim();
        Severity = severity;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Validation issue message cannot be empty.", nameof(message))
            : message;
    }

    public string SourceName { get; }

    public int LineNumber { get; }

    public string? Key { get; }

    public FieldRegistryHarvestValidationSeverity Severity { get; }

    public string Message { get; }
}
