namespace RA2IniEditor.IDE.Diagnostics;

/// <summary>
/// Represents the current diagnosable state of the IDE readonly source editor.
/// </summary>
public enum SourceEditorState
{
    Empty,
    Loading,
    Loaded,
    DeferredLargeFile,
    ReadFailed
}
