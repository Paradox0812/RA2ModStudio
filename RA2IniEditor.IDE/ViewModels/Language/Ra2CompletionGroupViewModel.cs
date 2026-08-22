namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2CompletionGroupViewModel
{
    public Ra2CompletionGroupViewModel(string name, IReadOnlyList<Ra2CompletionItemViewModel> items)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Completion group name cannot be empty.", nameof(name));

        Name = name;
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    public string Name { get; }

    public IReadOnlyList<Ra2CompletionItemViewModel> Items { get; }
}
