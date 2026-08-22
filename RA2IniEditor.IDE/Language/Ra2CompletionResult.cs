namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2CompletionResult
{
    public Ra2CompletionResult(
        IReadOnlyList<Ra2CompletionItem> items,
        Ra2TextSpan replacementSpan)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        ReplacementSpan = replacementSpan;
    }

    public IReadOnlyList<Ra2CompletionItem> Items { get; }

    public Ra2TextSpan ReplacementSpan { get; }

    public static Ra2CompletionResult EmptyAt(int caretOffset)
        => new([], new Ra2TextSpan(Math.Max(0, caretOffset), 0));
}
