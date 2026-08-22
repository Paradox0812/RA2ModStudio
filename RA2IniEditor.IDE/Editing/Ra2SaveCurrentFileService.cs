namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveCurrentFileService : IRa2SaveCurrentFileService
{
    private readonly IRa2SaveCurrentFileOrchestrator _orchestrator;
    private readonly IRa2TextFirstFileWriter _writer;
    private readonly IRa2EditableDocumentSessionService _sessionService;
    private readonly IRa2SaveRollbackService _rollbackService;

    public Ra2SaveCurrentFileService()
        : this(
            new Ra2SaveCurrentFileOrchestrator(),
            new Ra2TextFirstFileWriter(),
            new Ra2EditableDocumentSessionService(
                new Ra2IniTextDocumentParser(),
                new Ra2DirtyStateService()),
            new Ra2SaveRollbackService())
    {
    }

    public Ra2SaveCurrentFileService(
        IRa2SaveCurrentFileOrchestrator orchestrator,
        IRa2TextFirstFileWriter writer,
        IRa2EditableDocumentSessionService sessionService)
        : this(orchestrator, writer, sessionService, new Ra2SaveRollbackService())
    {
    }

    public Ra2SaveCurrentFileService(
        IRa2SaveCurrentFileOrchestrator orchestrator,
        IRa2TextFirstFileWriter writer,
        IRa2EditableDocumentSessionService sessionService,
        IRa2SaveRollbackService rollbackService)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _rollbackService = rollbackService ?? throw new ArgumentNullException(nameof(rollbackService));
    }

    public Ra2SaveCurrentFileResult Save(
        Ra2SaveCurrentFilePlanRequest request,
        string? projectRoot,
        DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2SaveCurrentFileOrchestrationResult orchestration = _orchestrator.PrepareToSave(
            request,
            projectRoot,
            timestamp,
            executeBackup: true);
        if (!orchestration.ReadyToWrite || orchestration.SavePlan is null)
            return Ra2SaveCurrentFileResult.NotReady(orchestration, request.Session);

        Ra2TextFileWriteResult writeResult = _writer.Write(orchestration.SavePlan);
        if (!writeResult.Success || request.Session is null)
        {
            if (orchestration.BackupPlan is null)
                return Ra2SaveCurrentFileResult.WriteFailedWithoutRollback(orchestration, writeResult, request.Session);

            Ra2RollbackResult rollbackResult = _rollbackService.RestoreFromBackup(orchestration.BackupPlan);
            return Ra2SaveCurrentFileResult.WriteFailed(orchestration, writeResult, rollbackResult, request.Session);
        }

        Ra2EditableDocumentSession updatedSession = _sessionService.MarkSaved(
            request.Session,
            orchestration.SavePlan.Text);
        return Ra2SaveCurrentFileResult.Succeeded(orchestration, writeResult, updatedSession);
    }
}
