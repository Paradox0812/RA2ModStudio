namespace RA2IniEditor.Application.Editing;

internal sealed class Ra2TextChangeSet
{
    public Ra2TextChangeSet(IEnumerable<Ra2TextChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        Ra2TextChange[] ordered = changes.OrderBy(change => change.Span.Start).ToArray();
        for (int index = 1; index < ordered.Length; index++)
        {
            Ra2TextChange previous = ordered[index - 1];
            Ra2TextChange current = ordered[index];
            if (previous.Span.Start + previous.Span.Length > current.Span.Start)
                throw new ArgumentException("Text changes cannot overlap.", nameof(changes));
        }

        Changes = ordered;
    }

    public IReadOnlyList<Ra2TextChange> Changes { get; }

    public string Apply(string sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        string result = sourceText;
        for (int index = Changes.Count - 1; index >= 0; index--)
        {
            Ra2TextChange change = Changes[index];
            if (change.Span.Start < 0 ||
                change.Span.Length < 0 ||
                change.Span.Start > sourceText.Length ||
                change.Span.Start + change.Span.Length > sourceText.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceText), "A text change span is outside the source text.");
            }

            result = result
                .Remove(change.Span.Start, change.Span.Length)
                .Insert(change.Span.Start, change.NewText);
        }

        return result;
    }
}
