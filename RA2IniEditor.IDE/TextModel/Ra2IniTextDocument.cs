namespace RA2IniEditor.IDE.TextModel;

internal sealed class Ra2IniTextDocument
{
    public Ra2IniTextDocument(
        string text,
        IReadOnlyList<Ra2IniDocumentLine> lines,
        Ra2IniNewLineKind newLineKind)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Lines = lines ?? throw new ArgumentNullException(nameof(lines));
        NewLineKind = newLineKind;
    }

    public string Text { get; }

    public IReadOnlyList<Ra2IniDocumentLine> Lines { get; }

    public Ra2IniNewLineKind NewLineKind { get; }

    public IEnumerable<Ra2IniDocumentLine> SectionHeaders
        => Lines.Where(line => line.Kind == Ra2IniDocumentLineKind.SectionHeader);

    public IEnumerable<Ra2IniDocumentLine> KeyValues
        => Lines.Where(line => line.Kind == Ra2IniDocumentLineKind.KeyValue);
}
