namespace RA2IniEditor.IDE.Editing;

internal interface IRa2SaveCurrentFilePlanBuilder
{
    Ra2EditorSavePlan BuildDryRun(Ra2SaveCurrentFilePlanRequest request);
}
