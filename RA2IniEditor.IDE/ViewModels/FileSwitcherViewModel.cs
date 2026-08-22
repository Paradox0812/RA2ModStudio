using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Provides file selection state for the IDE file switcher.
/// </summary>
public sealed class FileSwitcherViewModel : INotifyPropertyChanged
{
    private SourceFileItemViewModel? _selectedFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSwitcherViewModel"/> class.
    /// </summary>
    public FileSwitcherViewModel()
    {
        Files = new ObservableCollection<SourceFileItemViewModel>();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the displayed file list.
    /// </summary>
    public ObservableCollection<SourceFileItemViewModel> Files { get; }

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    public SourceFileItemViewModel? SelectedFile
    {
        get => _selectedFile;
        set => SetProperty(ref _selectedFile, value);
    }

    /// <summary>
    /// Replaces the displayed source files and selects the first item when available.
    /// </summary>
    public void ShowFiles(IEnumerable<SourceFileItemViewModel> files)
    {
        Files.Clear();

        foreach (SourceFileItemViewModel file in files)
            Files.Add(file);

        SelectedFile = Files.FirstOrDefault();
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
