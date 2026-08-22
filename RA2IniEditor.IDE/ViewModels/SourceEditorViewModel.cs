using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Diagnostics;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Provides readonly display state for the source editor area.
/// </summary>
public sealed class SourceEditorViewModel : INotifyPropertyChanged
{
    private string _documentTitle = string.Empty;
    private string _metadataText = string.Empty;
    private SourceEditorState _state = SourceEditorState.Empty;
    private string _text = string.Empty;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the current document title displayed above the source editor.
    /// </summary>
    public string DocumentTitle
    {
        get => _documentTitle;
        private set => SetProperty(ref _documentTitle, value);
    }

    /// <summary>
    /// Gets the current readonly source text.
    /// </summary>
    public string Text
    {
        get => _text;
        private set => SetProperty(ref _text, value);
    }

    /// <summary>
    /// Gets metadata shown for the current readonly source text.
    /// </summary>
    public string MetadataText
    {
        get => _metadataText;
        private set => SetProperty(ref _metadataText, value);
    }

    /// <summary>
    /// Gets the current readonly source editor state.
    /// </summary>
    public SourceEditorState State
    {
        get => _state;
        private set => SetProperty(ref _state, value);
    }

    /// <summary>
    /// Shows a loading state for a pending readonly document.
    /// </summary>
    public void ShowLoading(string documentTitle)
    {
        DocumentTitle = documentTitle;
        Text = "正在加载源文本预览...";
        MetadataText = string.Empty;
        State = SourceEditorState.Loading;
    }

    /// <summary>
    /// Shows an empty source editor state.
    /// </summary>
    public void ShowEmptyState(string message)
    {
        DocumentTitle = "未选择文件";
        Text = message;
        MetadataText = string.Empty;
        State = SourceEditorState.Empty;
    }

    /// <summary>
    /// Shows an error state for a readonly document.
    /// </summary>
    public void ShowError(string documentTitle, string message)
    {
        DocumentTitle = documentTitle;
        Text = message;
        MetadataText = "读取失败";
        State = SourceEditorState.ReadFailed;
    }

    /// <summary>
    /// Shows a deferred preview state for a very large file.
    /// </summary>
    public void ShowLargeFileDeferred(string documentTitle, string message, string metadataText)
    {
        DocumentTitle = documentTitle;
        Text = message;
        MetadataText = metadataText;
        State = SourceEditorState.DeferredLargeFile;
    }

    /// <summary>
    /// Shows a readonly document in the source editor.
    /// </summary>
    public void ShowDocument(string documentTitle, string text, string? metadataText = null)
    {
        DocumentTitle = documentTitle;
        Text = text;
        MetadataText = metadataText ?? string.Empty;
        State = SourceEditorState.Loaded;
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
