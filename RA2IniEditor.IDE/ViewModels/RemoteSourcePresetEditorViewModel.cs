using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

namespace RA2IniEditor.IDE.ViewModels;

internal sealed class RemoteSourcePresetEditorViewModel : INotifyPropertyChanged
{
    private readonly IFieldRegistryRawUrlResolver _urlResolver;
    private string _name;
    private string _url;
    private string _description;
    private string _tagsText;
    private bool _isEnabled;
    private string _validationMessage = "Name and URL are required. Saving a preset never fetches.";

    public RemoteSourcePresetEditorViewModel(FieldRegistryRemoteSourcePresetEditModel initial)
        : this(initial, new GitHubRawUrlResolver())
    {
    }

    internal RemoteSourcePresetEditorViewModel(
        FieldRegistryRemoteSourcePresetEditModel initial,
        IFieldRegistryRawUrlResolver urlResolver)
    {
        InitialId = initial.Id;
        _name = initial.Name;
        _url = initial.Url;
        _description = initial.Description ?? string.Empty;
        _tagsText = initial.TagsText;
        _isEnabled = initial.IsEnabled;
        _urlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? InitialId { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string TagsText
    {
        get => _tagsText;
        set => SetProperty(ref _tagsText, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "Preset name cannot be empty.";
            return false;
        }

        if (!_urlResolver.TryResolve(Url, out _, out string errorMessage))
        {
            ValidationMessage = errorMessage;
            return false;
        }

        ValidationMessage = "Preset is valid. OK will save locally without fetching.";
        return true;
    }

    public FieldRegistryRemoteSourcePresetEditModel ToEditModel()
        => new(InitialId, Name, Url, Description, TagsText, IsEnabled);

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
