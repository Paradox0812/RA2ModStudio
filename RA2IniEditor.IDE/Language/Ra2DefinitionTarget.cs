namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2DefinitionTarget
{
    public Ra2DefinitionTarget(
        Ra2DefinitionTargetKind kind,
        string title,
        string detail,
        string? sourceName,
        string? sourcePath,
        Ra2TextSpan? targetSpan,
        int? targetLineNumber,
        string? description = null)
    {
        Kind = kind;
        Title = title;
        Detail = detail;
        SourceName = sourceName;
        SourcePath = sourcePath;
        TargetSpan = targetSpan;
        TargetLineNumber = targetLineNumber;
        Description = description;
    }

    public Ra2DefinitionTargetKind Kind { get; }

    public string Title { get; }

    public string Detail { get; }

    public string? SourceName { get; }

    public string? SourcePath { get; }

    public Ra2TextSpan? TargetSpan { get; }

    public int? TargetLineNumber { get; }

    public string? Description { get; }
}
