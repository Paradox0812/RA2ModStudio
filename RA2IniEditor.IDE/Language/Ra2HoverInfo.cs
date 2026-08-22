namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2HoverInfo
{
    public Ra2HoverInfo(
        string title,
        string kind,
        string detail,
        string? description,
        string? source,
        Ra2TextSpan span,
        string? rawKey = null,
        string? displayName = null,
        string? typeDisplay = null,
        IReadOnlyList<string>? aliases = null)
    {
        Title = title;
        Kind = kind;
        Detail = detail;
        Description = description;
        Source = source;
        Span = span;
        RawKey = rawKey;
        DisplayName = displayName;
        TypeDisplay = typeDisplay;
        Aliases = aliases ?? [];
    }

    public string Title { get; }

    public string Kind { get; }

    public string Detail { get; }

    public string? Description { get; }

    public string? Source { get; }

    public Ra2TextSpan Span { get; }

    public string? RawKey { get; }

    public string? DisplayName { get; }

    public string? TypeDisplay { get; }

    public IReadOnlyList<string> Aliases { get; }
}
