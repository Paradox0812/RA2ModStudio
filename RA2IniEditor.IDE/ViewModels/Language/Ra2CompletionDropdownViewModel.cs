using System.ComponentModel;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2CompletionDropdownViewModel : INotifyPropertyChanged
{
    private int _selectedIndex = -1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<Ra2CompletionDropdownItemViewModel> Items { get; private set; } = [];

    public int ReplacementStart { get; private set; }

    public int ReplacementLength { get; private set; }

    public string ReplacementText { get; private set; } = "Replacement: start=0, length=0";

    public string CountText { get; private set; } = "Items: 0";

    public string StatusText { get; private set; } = "No completion items for current caret position.";

    public bool HasItems => Items.Count > 0;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value)
                return;

            _selectedIndex = value;
            OnPropertyChanged(nameof(SelectedIndex));
            OnPropertyChanged(nameof(SelectedItem));
        }
    }

    public Ra2CompletionDropdownItemViewModel? SelectedItem
        => SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;

    public void Update(Ra2CompletionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        _selectedIndex = -1;
        OnPropertyChanged(nameof(SelectedIndex));
        OnPropertyChanged(nameof(SelectedItem));

        Items = result.Items
            .Select(item => new Ra2CompletionDropdownItemViewModel(item))
            .ToArray();
        ReplacementStart = result.ReplacementSpan.Start;
        ReplacementLength = result.ReplacementSpan.Length;
        ReplacementText = $"Replacement: start={ReplacementStart}, length={ReplacementLength}";
        CountText = Items.Count == 1 ? "Items: 1" : $"Items: {Items.Count}";
        StatusText = Items.Count == 0
            ? "No completion items for current caret position."
            : Items.Count == 1
                ? "1 completion item."
                : $"{Items.Count} completion items.";
        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(ReplacementStart));
        OnPropertyChanged(nameof(ReplacementLength));
        OnPropertyChanged(nameof(ReplacementText));
        OnPropertyChanged(nameof(CountText));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(HasItems));
        SelectedIndex = Items.Count > 0 ? 0 : -1;
    }

    public void MoveSelection(int delta)
    {
        if (Items.Count == 0)
        {
            SelectedIndex = -1;
            return;
        }

        int nextIndex = SelectedIndex < 0 ? 0 : SelectedIndex + delta;
        SelectedIndex = Math.Clamp(nextIndex, 0, Items.Count - 1);
    }

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
