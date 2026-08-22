namespace RA2IniEditor.IDE.Editing;

internal interface IRa2SaveCurrentFileOrchestrator
{
    Ra2SaveCurrentFileOrchestrationResult PrepareToSave(
        Ra2SaveCurrentFilePlanRequest request,
        string? projectRoot,
        DateTime timestamp,
        bool executeBackup);
}
