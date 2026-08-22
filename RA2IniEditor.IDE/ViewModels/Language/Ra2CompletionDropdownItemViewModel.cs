using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2CompletionDropdownItemViewModel
{
    public Ra2CompletionDropdownItemViewModel(Ra2CompletionItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Item = item;
        Label = item.Label;
        Kind = item.Kind.ToString();
        TypeDisplay = GetTypeDisplay(item);
        SourceDisplayText = GetSourceDisplayText(item.SourceKind);
        Detail = item.Detail ?? string.Empty;
        AnnotationText = GetAnnotationText(item);
        IsFallback = item.SourceKind == Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback;
    }

    public string Label { get; }

    public Ra2CompletionItem Item { get; }

    public string Kind { get; }

    public string TypeDisplay { get; }

    public string SourceDisplayText { get; }

    public string Detail { get; }

    public string AnnotationText { get; }

    public bool IsFallback { get; }

    private static string GetTypeDisplay(Ra2CompletionItem item)
    {
        if (TryExtractType(item.Detail, out string typeDisplay))
            return typeDisplay;

        return item.Kind.ToString();
    }

    private static string GetAnnotationText(Ra2CompletionItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Documentation))
            return item.Documentation;

        if (!TryExtractType(item.Detail, out _) && !string.IsNullOrWhiteSpace(item.Detail))
            return item.Detail;

        return string.Empty;
    }

    private static bool TryExtractType(string? detail, out string typeDisplay)
    {
        typeDisplay = string.Empty;
        if (string.IsNullOrWhiteSpace(detail))
            return false;

        const string prefix = "Type: ";
        int prefixIndex = detail.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (prefixIndex < 0)
            return false;

        string tail = detail[(prefixIndex + prefix.Length)..].Trim();
        int endIndex = tail.Length;
        foreach (string separator in new[] { " | ", ";", "Aliases:" })
        {
            int separatorIndex = tail.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (separatorIndex >= 0)
                endIndex = Math.Min(endIndex, separatorIndex);
        }

        typeDisplay = tail[..endIndex].Trim();
        return typeDisplay.Length > 0;
    }

    private static string GetSourceDisplayText(Ra2CompletionItemSourceKind sourceKind)
    {
        return sourceKind switch
        {
            Ra2CompletionItemSourceKind.FieldRegistry => "Field Registry",
            Ra2CompletionItemSourceKind.CurrentDocumentSection => "Current Document",
            Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback => "Current Document - Unclassified",
            Ra2CompletionItemSourceKind.BuiltInValueCatalog => "BuiltIn",
            Ra2CompletionItemSourceKind.UserValueCatalog => "User",
            Ra2CompletionItemSourceKind.ProjectValueCatalog => "Project",
            Ra2CompletionItemSourceKind.CurrentDocumentInference => "Current Document",
            _ => "Unknown"
        };
    }
}
