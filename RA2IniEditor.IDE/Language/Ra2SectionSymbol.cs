using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2SectionSymbol
{
    public Ra2SectionSymbol(
        string name,
        Ra2SectionKind kind,
        int headerLineNumber,
        Ra2TextSpan headerSpan,
        Ra2TextSpan bodySpan,
        string? inlineComment = null,
        string? precedingComment = null)
    {
        Name = name;
        Kind = kind;
        HeaderLineNumber = headerLineNumber;
        HeaderSpan = headerSpan;
        BodySpan = bodySpan;
        InlineComment = NormalizeOptional(inlineComment);
        PrecedingComment = NormalizeOptional(precedingComment);
        DisplayNote = InlineComment ?? PrecedingComment;
    }

    public string Name { get; }

    public Ra2SectionKind Kind { get; }

    public int HeaderLineNumber { get; }

    public Ra2TextSpan HeaderSpan { get; }

    public Ra2TextSpan BodySpan { get; }

    public string? InlineComment { get; }

    public string? PrecedingComment { get; }

    public string? DisplayNote { get; }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
