namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiMarkdownBlock
{
    public Ra2AiMarkdownBlockKind Kind { get; init; } = Ra2AiMarkdownBlockKind.Paragraph;

    public bool IsCodeBlock => Kind == Ra2AiMarkdownBlockKind.Code;

    public int HeadingLevel { get; init; }

    public string? Language { get; init; }

    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<string> TableHeaders { get; init; } = [];

    public IReadOnlyList<IReadOnlyList<string>> TableRows { get; init; } = [];
}
