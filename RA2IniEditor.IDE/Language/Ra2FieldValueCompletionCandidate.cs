namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2FieldValueCompletionCandidate
{
    public Ra2FieldValueCompletionCandidate(
        string value,
        string? displayName,
        string? description,
        Ra2CompletionItemKind kind,
        int priority,
        Ra2FieldValueCompletionSourceKind sourceKind)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value completion candidate cannot be empty.", nameof(value))
            : value;
        DisplayName = displayName;
        Description = description;
        Kind = kind;
        Priority = priority;
        SourceKind = sourceKind;
    }

    public string Value { get; }

    public string? DisplayName { get; }

    public string? Description { get; }

    public Ra2CompletionItemKind Kind { get; }

    public int Priority { get; }

    public Ra2FieldValueCompletionSourceKind SourceKind { get; }
}
