using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2CompletionPreviewViewModel
{
    public Ra2CompletionPreviewViewModel(Ra2CompletionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Items = result.Items
            .Select(item => new Ra2CompletionItemViewModel(item))
            .ToArray();
        Groups = Items
            .GroupBy(item => item.Kind)
            .OrderBy(group => GetKindOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new Ra2CompletionGroupViewModel(group.Key, group.ToArray()))
            .ToArray();
        ReplacementStart = result.ReplacementSpan.Start;
        ReplacementLength = result.ReplacementSpan.Length;
        ReplacementText = $"Replacement: start={ReplacementStart}, length={ReplacementLength}";
        CountText = Items.Count == 1
            ? "Items: 1"
            : $"Items: {Items.Count}";
        EmptyText = "No completion items for current caret position.";
        StatusText = Items.Count == 0
            ? EmptyText
            : Items.Count == 1
                ? "1 completion item."
                : $"{Items.Count} completion items.";
    }

    public IReadOnlyList<Ra2CompletionItemViewModel> Items { get; }

    public IReadOnlyList<Ra2CompletionGroupViewModel> Groups { get; }

    public int ReplacementStart { get; }

    public int ReplacementLength { get; }

    public string ReplacementText { get; }

    public string CountText { get; }

    public string EmptyText { get; }

    public string StatusText { get; }

    public bool HasItems => Items.Count > 0;

    private static int GetKindOrder(string kind)
    {
        return kind switch
        {
            "Key" => 0,
            "Reference" => 1,
            "Value" => 2,
            "Section" => 3,
            "Keyword" => 4,
            _ => 99
        };
    }
}
