using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.TextModel;

internal sealed class Ra2IniDocumentLine
{
    public Ra2IniDocumentLine(
        int lineNumber,
        Ra2TextSpan span,
        string text,
        string lineBreak,
        Ra2IniDocumentLineKind kind)
    {
        if (lineNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number must be positive.");

        LineNumber = lineNumber;
        Span = span;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        LineBreak = lineBreak ?? throw new ArgumentNullException(nameof(lineBreak));
        Kind = kind;
    }

    public int LineNumber { get; }

    public Ra2TextSpan Span { get; }

    public string Text { get; }

    public string LineBreak { get; }

    public Ra2IniDocumentLineKind Kind { get; }

    public string? SectionName { get; init; }

    public Ra2TextSpan? SectionNameSpan { get; init; }

    public string? Key { get; init; }

    public Ra2TextSpan? KeySpan { get; init; }

    public string? Value { get; init; }

    public Ra2TextSpan? ValueSpan { get; init; }

    public string? InlineComment { get; init; }

    public Ra2TextSpan? InlineCommentSpan { get; init; }
}
