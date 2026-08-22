namespace RA2IniEditor.IDE.Editing;

internal interface IRa2SaveCurrentFileService
{
    Ra2SaveCurrentFileResult Save(
        Ra2SaveCurrentFilePlanRequest request,
        string? projectRoot,
        DateTime timestamp);
}
