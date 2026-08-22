namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditorSavePlanBuilder : IRa2EditorSavePlanBuilder
{
    private readonly IRa2EditorSaveBoundary _saveBoundary;
    private readonly Ra2EditorNewLinePolicyProvider _newLinePolicyProvider;

    public Ra2EditorSavePlanBuilder()
        : this(new Ra2EditorSaveBoundary(), new Ra2EditorNewLinePolicyProvider())
    {
    }

    public Ra2EditorSavePlanBuilder(IRa2EditorSaveBoundary saveBoundary)
        : this(saveBoundary, new Ra2EditorNewLinePolicyProvider())
    {
    }

    public Ra2EditorSavePlanBuilder(
        IRa2EditorSaveBoundary saveBoundary,
        Ra2EditorNewLinePolicyProvider newLinePolicyProvider)
    {
        _saveBoundary = saveBoundary ?? throw new ArgumentNullException(nameof(saveBoundary));
        _newLinePolicyProvider = newLinePolicyProvider ?? throw new ArgumentNullException(nameof(newLinePolicyProvider));
    }

    public Ra2EditorSavePlan Build(Ra2EditableDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        bool hasFilePath = !string.IsNullOrWhiteSpace(session.DocumentState.FilePath);
        bool boundaryAllowsSave = _saveBoundary.CanSave(session.DocumentState);
        bool canSave = hasFilePath && boundaryAllowsSave;

        Ra2SaveCurrentFilePlanStatus status = ResolveStatus(session, hasFilePath, canSave);
        string reason = ResolveReason(session, hasFilePath, boundaryAllowsSave);

        return new Ra2EditorSavePlan(
            session.DocumentState.FilePath,
            session.DocumentState.CurrentText,
            session.TextDocument.NewLineKind,
            _newLinePolicyProvider.GetDefaultPolicy(session.TextDocument),
            canSave,
            reason,
            session.DocumentState.EncodingMetadata,
            status,
            reason,
            canSave ? "Dry-run only: a backup would be planned before a real save, but no backup is created now." : null);
    }

    private static Ra2SaveCurrentFilePlanStatus ResolveStatus(
        Ra2EditableDocumentSession session,
        bool hasFilePath,
        bool canSave)
    {
        if (canSave)
            return Ra2SaveCurrentFilePlanStatus.CanSave;

        if (!hasFilePath)
            return Ra2SaveCurrentFilePlanStatus.MissingFilePath;

        return session.DocumentState.State switch
        {
            Ra2EditorDocumentState.ReadOnlyPreview => Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview,
            Ra2EditorDocumentState.EditableClean => Ra2SaveCurrentFilePlanStatus.NotDirty,
            _ => Ra2SaveCurrentFilePlanStatus.UnknownFailure
        };
    }

    private static string ResolveReason(
        Ra2EditableDocumentSession session,
        bool hasFilePath,
        bool boundaryAllowsSave)
    {
        if (!hasFilePath)
            return "Cannot build a save preview because the current document has no file path.";

        return session.DocumentState.State switch
        {
            Ra2EditorDocumentState.EditableDirty when boundaryAllowsSave
                => "Text-first save preview is available. Save is not implemented yet.",
            Ra2EditorDocumentState.EditableClean
                => "No unsaved in-memory changes.",
            Ra2EditorDocumentState.ReadOnlyPreview
                => "Cannot save while the document is in read-only preview.",
            _ => "Cannot build a save preview for the current editor state."
        };
    }
}
