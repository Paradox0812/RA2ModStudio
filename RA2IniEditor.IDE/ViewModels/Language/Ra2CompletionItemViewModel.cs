using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2CompletionItemViewModel
{
    public Ra2CompletionItemViewModel(Ra2CompletionItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Label = item.Label;
        Kind = item.Kind.ToString();
        Detail = item.Detail ?? string.Empty;
        Documentation = item.Documentation ?? string.Empty;
        InsertText = item.InsertText;
        Priority = item.Priority;
        SourceKind = item.SourceKind.ToString();
        IsFallback = item.SourceKind == Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback;
        SourceDisplayText = GetSourceDisplayText(item.SourceKind);
    }

    public string Label { get; }

    public string Kind { get; }

    public string Detail { get; }

    public string Documentation { get; }

    public string InsertText { get; }

    public int Priority { get; }

    public string SourceKind { get; }

    public bool IsFallback { get; }

    public string SourceDisplayText { get; }

    private static string GetSourceDisplayText(Ra2CompletionItemSourceKind sourceKind)
    {
        return sourceKind switch
        {
            Ra2CompletionItemSourceKind.FieldRegistry => "Field Registry",
            Ra2CompletionItemSourceKind.CurrentDocumentSection => "Current Document",
            Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback => "Current Document - Unclassified",
            _ => "Unknown"
        };
    }
}
