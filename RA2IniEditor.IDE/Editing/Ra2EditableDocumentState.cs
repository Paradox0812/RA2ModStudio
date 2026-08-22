namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditableDocumentState
{
    public Ra2EditableDocumentState(
        string filePath,
        string originalText,
        string currentText,
        Ra2EditorDocumentState state,
        Ra2EditorTextEncodingMetadata? encodingMetadata = null)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        OriginalText = originalText ?? throw new ArgumentNullException(nameof(originalText));
        CurrentText = currentText ?? throw new ArgumentNullException(nameof(currentText));
        State = state;
        EncodingMetadata = encodingMetadata ?? Ra2EditorTextEncodingMetadata.Unknown;
    }

    public string FilePath { get; }

    public string OriginalText { get; }

    public string CurrentText { get; }

    public Ra2EditorDocumentState State { get; }

    public Ra2EditorTextEncodingMetadata EncodingMetadata { get; }

    public bool IsDirty => State == Ra2EditorDocumentState.EditableDirty;
}
