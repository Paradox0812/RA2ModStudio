using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace RA2IniEditor.IDE.Highlighting;

internal readonly record struct Ra2HighlightingStyle(Brush Brush);

/// <summary>
/// Provides brushes for the readonly RA2 INI source highlighter.
/// </summary>
public static class Ra2HighlightingBrushes
{
    public static readonly Brush SectionHeader = Freeze(new SolidColorBrush(Color.FromRgb(30, 90, 168)));

    public static readonly Brush KnownKey = Freeze(new SolidColorBrush(Color.FromRgb(17, 24, 39)));

    public static readonly Brush UnknownKey = Freeze(new SolidColorBrush(Color.FromRgb(194, 65, 12)));

    public static readonly Brush EqualsOperator = Freeze(new SolidColorBrush(Color.FromRgb(107, 114, 128)));

    public static readonly Brush Value = Freeze(new SolidColorBrush(Color.FromRgb(0, 152, 229)));

    public static readonly Brush NumberValue = Freeze(new SolidColorBrush(Color.FromRgb(0, 136, 214)));

    public static readonly Brush BooleanValue = Freeze(new SolidColorBrush(Color.FromRgb(0, 152, 229)));

    public static readonly Brush ReferenceValue = Freeze(new SolidColorBrush(Color.FromRgb(0, 152, 229)));

    public static readonly Brush EnumValue = Freeze(new SolidColorBrush(Color.FromRgb(0, 152, 229)));

    public static readonly Brush NeutralValue = Freeze(new SolidColorBrush(Color.FromRgb(107, 114, 128)));

    public static readonly Brush Comment = Freeze(new SolidColorBrush(Color.FromRgb(0, 160, 0)));

    private static Brush Freeze(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }
}

/// <summary>
/// Applies readonly RA2 INI token colors to an AvalonEdit document.
/// </summary>
public sealed class Ra2KnownFieldHighlightingTransformer : DocumentColorizingTransformer
{
    private readonly ReadonlyIniHighlightTokenizer _tokenizer;
    private TextDocument? _cachedDocument;
    private ITextSourceVersion? _cachedVersion;
    private IReadOnlyList<IniHighlightToken> _cachedTokens = Array.Empty<IniHighlightToken>();

    public Ra2KnownFieldHighlightingTransformer(ReadonlyIniHighlightTokenizer tokenizer)
    {
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        TextDocument? document = CurrentContext?.Document;
        if (document is null || line.Length <= 0)
            return;

        IReadOnlyList<IniHighlightToken> tokens = GetTokens(document);
        int lineStart = line.Offset;
        int lineEnd = line.EndOffset;

        foreach (IniHighlightToken token in tokens)
        {
            int tokenStart = token.StartOffset;
            int tokenEnd = token.StartOffset + token.Length;
            if (tokenEnd <= lineStart)
                continue;

            if (tokenStart >= lineEnd)
                break;

            int highlightStart = Math.Max(tokenStart, lineStart);
            int highlightEnd = Math.Min(tokenEnd, lineEnd);
            if (highlightStart >= highlightEnd)
                continue;

            Ra2HighlightingStyle style = GetStyle(token.Kind);
            ChangeLinePart(
                highlightStart,
                highlightEnd,
                element => ApplyStyle(element.TextRunProperties, style));
        }
    }

    private IReadOnlyList<IniHighlightToken> GetTokens(TextDocument document)
    {
        ITextSourceVersion version = document.Version;
        if (ReferenceEquals(_cachedDocument, document) && Equals(_cachedVersion, version))
            return _cachedTokens;

        _cachedDocument = document;
        _cachedVersion = version;
        _cachedTokens = _tokenizer.Tokenize(document.Text);
        return _cachedTokens;
    }

    internal static Ra2HighlightingStyle GetStyle(IniHighlightTokenKind kind) => kind switch
    {
        IniHighlightTokenKind.SectionHeader => new(Ra2HighlightingBrushes.SectionHeader),
        IniHighlightTokenKind.KnownKey => new(Ra2HighlightingBrushes.KnownKey),
        IniHighlightTokenKind.UnknownKey => new(Ra2HighlightingBrushes.UnknownKey),
        IniHighlightTokenKind.Equals => new(Ra2HighlightingBrushes.EqualsOperator),
        IniHighlightTokenKind.Value => new(Ra2HighlightingBrushes.Value),
        IniHighlightTokenKind.NumberValue => new(Ra2HighlightingBrushes.NumberValue),
        IniHighlightTokenKind.BooleanValue => new(Ra2HighlightingBrushes.BooleanValue),
        IniHighlightTokenKind.ReferenceValue => new(Ra2HighlightingBrushes.ReferenceValue),
        IniHighlightTokenKind.EnumValue => new(Ra2HighlightingBrushes.EnumValue),
        IniHighlightTokenKind.NeutralValue => new(Ra2HighlightingBrushes.NeutralValue),
        IniHighlightTokenKind.Comment => new(Ra2HighlightingBrushes.Comment),
        _ => new(Ra2HighlightingBrushes.Value)
    };

    private static Brush GetBrush(IniHighlightTokenKind kind) => GetStyle(kind).Brush;

    private static void ApplyStyle(
        VisualLineElementTextRunProperties properties,
        Ra2HighlightingStyle style)
    {
        properties.SetForegroundBrush(style.Brush);
    }
}
