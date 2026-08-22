using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2KeyValueSymbol
{
    public Ra2KeyValueSymbol(
        string sectionName,
        Ra2SectionKind sectionKind,
        string key,
        string? value,
        string? rawValue,
        string? inlineComment,
        int lineNumber,
        Ra2TextSpan lineSpan,
        Ra2TextSpan keySpan,
        Ra2TextSpan? valueSpan,
        bool isKnownKey)
    {
        SectionName = sectionName;
        SectionKind = sectionKind;
        Key = key;
        Value = value;
        RawValue = rawValue;
        InlineComment = inlineComment;
        LineNumber = lineNumber;
        LineSpan = lineSpan;
        KeySpan = keySpan;
        ValueSpan = valueSpan;
        IsKnownKey = isKnownKey;
    }

    public string SectionName { get; }

    public Ra2SectionKind SectionKind { get; }

    public string Key { get; }

    public string? Value { get; }

    public string? RawValue { get; }

    public string? InlineComment { get; }

    public int LineNumber { get; }

    public Ra2TextSpan LineSpan { get; }

    public Ra2TextSpan KeySpan { get; }

    public Ra2TextSpan? ValueSpan { get; }

    public bool IsKnownKey { get; }
}
