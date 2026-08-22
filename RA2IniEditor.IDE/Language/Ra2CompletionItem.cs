namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2CompletionItem
{
    public Ra2CompletionItem(
        string label,
        Ra2CompletionItemKind kind,
        string? detail = null,
        string? documentation = null,
        string? insertText = null,
        int priority = 0,
        Ra2CompletionItemSourceKind sourceKind = Ra2CompletionItemSourceKind.CurrentDocumentSection)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Completion label cannot be empty.", nameof(label));

        Label = label;
        Kind = kind;
        Detail = detail;
        Documentation = documentation;
        InsertText = string.IsNullOrEmpty(insertText) ? label : insertText;
        Priority = priority;
        SourceKind = sourceKind;
    }

    public string Label { get; }

    public Ra2CompletionItemKind Kind { get; }

    public string? Detail { get; }

    public string? Documentation { get; }

    public string InsertText { get; }

    public int Priority { get; }

    public Ra2CompletionItemSourceKind SourceKind { get; }
}
