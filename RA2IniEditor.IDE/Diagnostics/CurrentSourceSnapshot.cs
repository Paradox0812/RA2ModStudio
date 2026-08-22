using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.Diagnostics;

/// <summary>
/// Represents one stable snapshot of the current readonly IDE source file.
/// </summary>
public sealed class CurrentSourceSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentSourceSnapshot"/> class.
    /// </summary>
    public CurrentSourceSnapshot(
        string projectRootPath,
        string filePath,
        string fileName,
        string text,
        int version,
        SourceEditorState state)
        : this(
            projectRootPath,
            filePath,
            fileName,
            text,
            version,
            state,
            Ra2EditorTextEncodingMetadata.Unknown)
    {
    }

    internal CurrentSourceSnapshot(
        string projectRootPath,
        string filePath,
        string fileName,
        string text,
        int version,
        SourceEditorState state,
        Ra2EditorTextEncodingMetadata encodingMetadata)
    {
        ProjectRootPath = projectRootPath;
        FilePath = filePath;
        FileName = fileName;
        Text = text;
        Version = version;
        State = state;
        EncodingMetadata = encodingMetadata ?? Ra2EditorTextEncodingMetadata.Unknown;
    }

    /// <summary>
    /// Gets the current project root path.
    /// </summary>
    public string ProjectRootPath { get; }

    /// <summary>
    /// Gets the current source file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the current source file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the current source editor text snapshot.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the source snapshot version.
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Gets the source editor state captured by this snapshot.
    /// </summary>
    public SourceEditorState State { get; }

    internal Ra2EditorTextEncodingMetadata EncodingMetadata { get; }

    /// <summary>
    /// Gets a value indicating whether diagnostics may run for this snapshot.
    /// </summary>
    public bool CanRunDiagnostics => State == SourceEditorState.Loaded;
}
