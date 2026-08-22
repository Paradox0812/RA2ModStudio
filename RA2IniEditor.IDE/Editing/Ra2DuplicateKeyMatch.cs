using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2DuplicateKeyMatch
{
    public Ra2DuplicateKeyMatch(
        string key,
        int lineNumber,
        Ra2TextSpan lineSpan,
        Ra2TextSpan valueSpan,
        string existingValue)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Duplicate key cannot be empty.", nameof(key));

        Key = key.Trim();
        LineNumber = lineNumber;
        LineSpan = lineSpan;
        ValueSpan = valueSpan;
        ExistingValue = existingValue ?? string.Empty;
    }

    public string Key { get; }

    public int LineNumber { get; }

    public Ra2TextSpan LineSpan { get; }

    public Ra2TextSpan ValueSpan { get; }

    public string ExistingValue { get; }
}
