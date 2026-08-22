using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveCurrentFilePlanBuilder : IRa2SaveCurrentFilePlanBuilder
{
    private const string BackupDryRunPreview = "Dry-run only: a backup would be planned before a real save, but no backup is created now.";

    private readonly IRa2EditorSavePlanBuilder _sessionPlanBuilder;

    public Ra2SaveCurrentFilePlanBuilder()
        : this(new Ra2EditorSavePlanBuilder())
    {
    }

    public Ra2SaveCurrentFilePlanBuilder(IRa2EditorSavePlanBuilder sessionPlanBuilder)
    {
        _sessionPlanBuilder = sessionPlanBuilder ?? throw new ArgumentNullException(nameof(sessionPlanBuilder));
    }

    public Ra2EditorSavePlan BuildDryRun(Ra2SaveCurrentFilePlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Session is null)
        {
            return CreateCannotSavePlan(
                Ra2SaveCurrentFilePlanStatus.NoEditableSession,
                "There is no editable session to save.");
        }

        if (request.IsReadOnlyPreview || request.Session.DocumentState.State == Ra2EditorDocumentState.ReadOnlyPreview)
        {
            return CreateCannotSavePlan(
                Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview,
                "The current document is read-only preview. Enter edit mode before saving.",
                request.Session);
        }

        Ra2EditorSavePlan plan = _sessionPlanBuilder.Build(request.Session);
        if (plan.CanSave)
        {
            return plan.WithStatus(
                Ra2SaveCurrentFilePlanStatus.CanSave,
                "Save current file dry-run is available.",
                BackupDryRunPreview);
        }

        Ra2SaveCurrentFilePlanStatus status = ResolveCannotSaveStatus(request.Session);
        return plan.WithStatus(status, ResolveCannotSaveMessage(status), backupPlanPreview: null);
    }

    private static Ra2SaveCurrentFilePlanStatus ResolveCannotSaveStatus(Ra2EditableDocumentSession session)
    {
        if (string.IsNullOrWhiteSpace(session.DocumentState.FilePath))
            return Ra2SaveCurrentFilePlanStatus.MissingFilePath;

        return session.DocumentState.State switch
        {
            Ra2EditorDocumentState.EditableClean => Ra2SaveCurrentFilePlanStatus.NotDirty,
            Ra2EditorDocumentState.ReadOnlyPreview => Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview,
            _ => Ra2SaveCurrentFilePlanStatus.UnknownFailure
        };
    }

    private static string ResolveCannotSaveMessage(Ra2SaveCurrentFilePlanStatus status)
        => status switch
        {
            Ra2SaveCurrentFilePlanStatus.NotDirty => "The current file has no unsaved in-memory changes.",
            Ra2SaveCurrentFilePlanStatus.MissingFilePath => "The current file path is missing, so it cannot be saved.",
            Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview => "The current document is read-only preview. Enter edit mode before saving.",
            Ra2SaveCurrentFilePlanStatus.NoEditableSession => "There is no editable session to save.",
            _ => "Cannot build a save current file dry-run plan for the current editor state."
        };

    private static Ra2EditorSavePlan CreateCannotSavePlan(
        Ra2SaveCurrentFilePlanStatus status,
        string message,
        Ra2EditableDocumentSession? session = null)
    {
        return new Ra2EditorSavePlan(
            session?.DocumentState.FilePath ?? string.Empty,
            session?.DocumentState.CurrentText ?? string.Empty,
            session?.TextDocument.NewLineKind ?? Ra2IniNewLineKind.Unknown,
            Ra2EditorNewLineSavePolicy.PreserveCurrentText,
            canSave: false,
            reason: message,
            session?.DocumentState.EncodingMetadata,
            status,
            message,
            backupPlanPreview: null);
    }
}
