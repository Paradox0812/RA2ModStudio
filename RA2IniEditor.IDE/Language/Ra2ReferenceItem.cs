namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2ReferenceItem
{
    public Ra2ReferenceItem(
        string sourceSectionName,
        string sourceKey,
        string value,
        int lineNumber,
        Ra2TextSpan lineSpan,
        Ra2TextSpan valueSpan)
    {
        SourceSectionName = sourceSectionName;
        SourceKey = sourceKey;
        Value = value;
        LineNumber = lineNumber;
        LineSpan = lineSpan;
        ValueSpan = valueSpan;
    }

    public string SourceSectionName { get; }

    public string SourceKey { get; }

    public string Value { get; }

    public int LineNumber { get; }

    public Ra2TextSpan LineSpan { get; }

    public Ra2TextSpan ValueSpan { get; }
}
