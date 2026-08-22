namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveCurrentFileOrchestrator : IRa2SaveCurrentFileOrchestrator
{
    private readonly IRa2SaveCurrentFilePlanBuilder _savePlanBuilder;
    private readonly IRa2BackupPlanBuilder _backupPlanBuilder;
    private readonly IRa2BackupService _backupService;

    public Ra2SaveCurrentFileOrchestrator()
        : this(
            new Ra2SaveCurrentFilePlanBuilder(),
            new Ra2BackupPlanBuilder(),
            new Ra2BackupService())
    {
    }

    public Ra2SaveCurrentFileOrchestrator(
        IRa2SaveCurrentFilePlanBuilder savePlanBuilder,
        IRa2BackupPlanBuilder backupPlanBuilder,
        IRa2BackupService backupService)
    {
        _savePlanBuilder = savePlanBuilder ?? throw new ArgumentNullException(nameof(savePlanBuilder));
        _backupPlanBuilder = backupPlanBuilder ?? throw new ArgumentNullException(nameof(backupPlanBuilder));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
    }

    public Ra2SaveCurrentFileOrchestrationResult PrepareToSave(
        Ra2SaveCurrentFilePlanRequest request,
        string? projectRoot,
        DateTime timestamp,
        bool executeBackup)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2EditorSavePlan savePlan = _savePlanBuilder.BuildDryRun(request);
        if (!savePlan.CanSave)
            return Ra2SaveCurrentFileOrchestrationResult.SavePlanCannotSave(savePlan);

        Ra2BackupPlan backupPlan = _backupPlanBuilder.Build(savePlan, projectRoot, timestamp);
        if (!backupPlan.CanBackup)
            return Ra2SaveCurrentFileOrchestrationResult.BackupPlanCannotBackup(savePlan, backupPlan);

        if (!executeBackup)
            return Ra2SaveCurrentFileOrchestrationResult.StoppedBeforeWrite(savePlan, backupPlan);

        Ra2BackupResult backupResult = _backupService.CreateBackup(backupPlan);
        return backupResult.Success
            ? Ra2SaveCurrentFileOrchestrationResult.ReadyForWrite(savePlan, backupPlan, backupResult)
            : Ra2SaveCurrentFileOrchestrationResult.BackupFailed(savePlan, backupPlan, backupResult);
    }
}
