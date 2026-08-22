using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Models;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Provides readonly current-file navigator display state.
/// </summary>
public sealed class NavigatorViewModel : INotifyPropertyChanged
{
    private SectionIndexItemViewModel? _selectedItem;
    private string _statusText = "Open a file to build the current-file navigator.";

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the current section index items.
    /// </summary>
    public ObservableCollection<SectionIndexItemViewModel> Items { get; } = [];

    /// <summary>
    /// Gets or sets the selected navigator item.
    /// </summary>
    public SectionIndexItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    /// <summary>
    /// Gets the status text displayed below the navigator list.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    /// <summary>
    /// Shows a loading state while the current file or index is being loaded.
    /// </summary>
    public void ShowLoading(string message)
    {
        Items.Clear();
        SelectedItem = null;
        StatusText = message;
    }

    /// <summary>
    /// Shows section index items for the current file.
    /// </summary>
    public void ShowSections(IEnumerable<ReadonlySectionIndexItem> items)
    {
        Items.Clear();
        foreach (ReadonlySectionIndexItem item in items)
            Items.Add(new SectionIndexItemViewModel(item));

        SelectedItem = null;
        StatusText = Items.Count == 1 ? "1 section" : $"{Items.Count} sections";
    }

    /// <summary>
    /// Shows an empty navigator state.
    /// </summary>
    public void ShowEmptyState(string message)
    {
        Items.Clear();
        SelectedItem = null;
        StatusText = message;
    }

    /// <summary>
    /// Shows a disabled navigator state.
    /// </summary>
    public void ShowDisabled(string message)
    {
        Items.Clear();
        SelectedItem = null;
        StatusText = message;
    }

    /// <summary>
    /// Shows an error navigator state.
    /// </summary>
    public void ShowError(string message)
    {
        Items.Clear();
        SelectedItem = null;
        StatusText = message;
    }

    /// <summary>
    /// Clears the current navigator state.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        SelectedItem = null;
        StatusText = "Open a file to build the current-file navigator.";
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
