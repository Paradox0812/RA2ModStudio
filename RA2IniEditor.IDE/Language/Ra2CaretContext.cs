namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2CaretContext
{
    public Ra2CaretContext(
        int offset,
        Ra2CaretRegion region,
        Ra2SectionSymbol? section,
        Ra2KeyValueSymbol? keyValue,
        string? tokenText,
        Ra2TextSpan? tokenSpan)
    {
        Offset = offset;
        Region = region;
        Section = section;
        KeyValue = keyValue;
        TokenText = tokenText;
        TokenSpan = tokenSpan;
    }

    public int Offset { get; }

    public Ra2CaretRegion Region { get; }

    public Ra2SectionSymbol? Section { get; }

    public Ra2KeyValueSymbol? KeyValue { get; }

    public string? TokenText { get; }

    public Ra2TextSpan? TokenSpan { get; }
}
