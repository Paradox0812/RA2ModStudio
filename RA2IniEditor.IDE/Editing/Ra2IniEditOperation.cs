namespace RA2IniEditor.IDE.Editing;

internal enum Ra2IniEditOperationKind
{
    UpsertField = 0,
    ReplaceFieldValue
}

/// <summary>
/// 表示一个受限的单字段结构化编辑意图。
/// </summary>
internal sealed class Ra2IniEditOperation
{
    public Ra2IniEditOperation(
        Ra2IniEditOperationKind kind,
        string sectionName,
        string key,
        string value)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        SectionName = NormalizeIdentifier(sectionName, nameof(sectionName), allowEquals: false, allowBrackets: false);
        Key = NormalizeIdentifier(key, nameof(key), allowEquals: false, allowBrackets: true);
        Value = ValidateValue(value);
    }

    public Ra2IniEditOperationKind Kind { get; }

    public string SectionName { get; }

    public string Key { get; }

    public string Value { get; }

    private static string NormalizeIdentifier(
        string value,
        string parameterName,
        bool allowEquals,
        bool allowBrackets)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Structured edit identifier cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (ContainsLineBreakOrNull(normalized) ||
            (!allowEquals && normalized.Contains('=')) ||
            (!allowBrackets && normalized.IndexOfAny(['[', ']']) >= 0))
        {
            throw new ArgumentException("Structured edit identifier contains unsupported characters.", parameterName);
        }

        return normalized;
    }

    private static string ValidateValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (ContainsLineBreakOrNull(value))
            throw new ArgumentException("Structured field values cannot contain line breaks or NUL.", nameof(value));

        return value;
    }

    private static bool ContainsLineBreakOrNull(string value)
        => value.IndexOfAny(['\r', '\n', '\0']) >= 0;
}
