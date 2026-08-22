namespace RA2IniEditor.IDE.Editing;

internal interface IRa2BackupPlanBuilder
{
    Ra2BackupPlan Build(Ra2EditorSavePlan savePlan, string? projectRoot, DateTime timestamp);
}
