namespace RA2IniEditor.IDE.Editing;

internal interface IRa2ActiveEditorProjection
{
    string CurrentText { get; }
    void SetText(string text);
    void EnterReadOnlyFailSafe();
}

internal sealed class Ra2ProjectEditorTransactionCoordinator
{
    private sealed record CompoundMember(
        string FilePath,
        Guid DocumentId,
        int AppliedRevision,
        string BeforeText,
        string AfterText);

    private sealed record CompoundEntry(
        long FieldRegistryRevision,
        IReadOnlyList<CompoundMember> Members,
        IReadOnlyList<Ra2EditableDocumentSession>? UndoneSessions);

    private readonly Ra2ProjectDocumentSessionStore _store;
    private readonly IRa2EditableDocumentSessionService _sessionService;
    private readonly IRa2ActiveEditorProjection _activeEditor;
    private readonly Func<long> _currentFieldRegistryRevision;
    private CompoundEntry? _compoundEntry;

    public Ra2ProjectEditorTransactionCoordinator(
        Ra2ProjectDocumentSessionStore store,
        IRa2EditableDocumentSessionService sessionService,
        IRa2ActiveEditorProjection activeEditor,
        Func<long> currentFieldRegistryRevision)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _activeEditor = activeEditor ?? throw new ArgumentNullException(nameof(activeEditor));
        _currentFieldRegistryRevision = currentFieldRegistryRevision ?? throw new ArgumentNullException(nameof(currentFieldRegistryRevision));
    }

    public Ra2ProjectEditApplyResult Apply(Ra2ProjectEditPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (!preview.Succeeded)
        {
            return Failure(preview, Ra2ProjectEditApplyOutcomeKind.PrepareFailed, "A failed project preview cannot be applied.");
        }
        if (_store.ProjectSessionId != preview.Snapshot.ProjectSessionId ||
            _store.ProjectRevision != preview.Snapshot.ProjectRevision)
        {
            return Failure(preview, Ra2ProjectEditApplyOutcomeKind.Stale, "The project changed after preview generation.");
        }

        long registryRevision;
        try
        {
            registryRevision = _currentFieldRegistryRevision();
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return Failure(preview, Ra2ProjectEditApplyOutcomeKind.PrepareFailed, "The current Field Registry revision could not be read.");
        }

        List<Ra2ProjectDocumentSessionReplacement> replacements = new(preview.DocumentPreviews.Count);
        foreach (Ra2AutomationEditPreviewResult documentPreview in preview.DocumentPreviews)
        {
            if (!_store.TryGetSession(documentPreview.FilePath, out Ra2EditableDocumentSession? current) ||
                current is null ||
                current.DocumentState.State == Ra2EditorDocumentState.ReadOnlyPreview ||
                current.DocumentId != documentPreview.DocumentId ||
                current.EditRevision != documentPreview.Version ||
                registryRevision != documentPreview.FieldRegistryRevision ||
                !string.Equals(current.DocumentState.CurrentText, preview.Snapshot.Documents
                    .Single(document => document.DocumentId == documentPreview.DocumentId).Text, StringComparison.Ordinal) ||
                string.IsNullOrEmpty(documentPreview.CandidateText) ||
                string.Equals(current.DocumentState.CurrentText, documentPreview.CandidateText, StringComparison.Ordinal))
            {
                return Failure(preview, Ra2ProjectEditApplyOutcomeKind.Stale, "A project document changed after preview generation.");
            }

            Ra2EditableDocumentSession prepared;
            try
            {
                prepared = _sessionService.UpdateText(current, documentPreview.CandidateText);
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
                return Failure(preview, Ra2ProjectEditApplyOutcomeKind.PrepareFailed, "A project document could not be prepared for commit.");
            }
            if (prepared.DocumentId != current.DocumentId ||
                prepared.EditRevision != checked(current.EditRevision + 1) ||
                !string.Equals(prepared.DocumentState.CurrentText, documentPreview.CandidateText, StringComparison.Ordinal))
            {
                return Failure(preview, Ra2ProjectEditApplyOutcomeKind.PrepareFailed, "A prepared project session is inconsistent.");
            }
            replacements.Add(new Ra2ProjectDocumentSessionReplacement(current, prepared));
        }

        Ra2ProjectDocumentSessionReplacement? activeReplacement = replacements.FirstOrDefault(
            replacement => string.Equals(
                replacement.Expected.DocumentState.FilePath,
                _store.ActiveFilePath,
                StringComparison.OrdinalIgnoreCase));
        if (activeReplacement is not null &&
            !string.Equals(_activeEditor.CurrentText, activeReplacement.Expected.DocumentState.CurrentText, StringComparison.Ordinal))
        {
            return Failure(preview, Ra2ProjectEditApplyOutcomeKind.Stale, "The active editor projection changed after preview generation.");
        }

        long projectRevisionBeforeCommit = _store.ProjectRevision;
        if (!_store.TryReplaceMany(replacements, out _))
            return Failure(preview, Ra2ProjectEditApplyOutcomeKind.CommitFailed, "The project sessions changed before atomic commit.");

        if (activeReplacement is not null)
        {
            try
            {
                _activeEditor.SetText(activeReplacement.Replacement.DocumentState.CurrentText);
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
                IReadOnlyList<Ra2ProjectDocumentSessionReplacement> rollback = replacements
                    .Select(replacement => new Ra2ProjectDocumentSessionReplacement(replacement.Replacement, replacement.Expected))
                    .ToArray();
                bool storeRestored = _store.TryRestoreMany(rollback, projectRevisionBeforeCommit, out _);
                try
                {
                    _activeEditor.SetText(activeReplacement.Expected.DocumentState.CurrentText);
                }
                catch (Exception restoreException) when (!IsFatal(restoreException))
                {
                    _activeEditor.EnterReadOnlyFailSafe();
                }
                if (!storeRestored)
                    _activeEditor.EnterReadOnlyFailSafe();
                return Failure(preview, Ra2ProjectEditApplyOutcomeKind.EditorSynchronizationFailed, "The active editor could not be synchronized; the project transaction was rolled back.");
            }
        }

        _compoundEntry = new CompoundEntry(
            registryRevision,
            replacements.Select(replacement => new CompoundMember(
                replacement.Expected.DocumentState.FilePath,
                replacement.Expected.DocumentId,
                replacement.Replacement.EditRevision,
                replacement.Expected.DocumentState.CurrentText,
                replacement.Replacement.DocumentState.CurrentText)).ToArray(),
            null);
        return Ra2ProjectEditApplyResult.Applied(preview, replacements, _store.DirtyDocumentCount);
    }

    public bool CanUndo => _compoundEntry is { UndoneSessions: null };
    public bool CanRedo => _compoundEntry is { UndoneSessions: not null };

    public Ra2ProjectCompoundUndoResult Undo()
        => Transition(isUndo: true);

    public Ra2ProjectCompoundUndoResult Redo()
        => Transition(isUndo: false);

    public void InvalidateCompoundEntry()
        => _compoundEntry = null;

    private Ra2ProjectCompoundUndoResult Transition(bool isUndo)
    {
        CompoundEntry? entry = _compoundEntry;
        if (entry is null || isUndo == (entry.UndoneSessions is not null))
            return Ra2ProjectCompoundUndoResult.Unavailable(isUndo);
        if (_currentFieldRegistryRevision() != entry.FieldRegistryRevision)
            return Ra2ProjectCompoundUndoResult.Stale(isUndo, "The Field Registry changed after the project transaction.");

        List<Ra2ProjectDocumentSessionReplacement> replacements = new(entry.Members.Count);
        foreach (CompoundMember member in entry.Members)
        {
            if (!_store.TryGetSession(member.FilePath, out Ra2EditableDocumentSession? current) || current is null)
                return Ra2ProjectCompoundUndoResult.Stale(isUndo, "A project transaction document is no longer available.");

            int expectedRevision;
            string expectedText;
            string targetText;
            if (isUndo)
            {
                expectedRevision = member.AppliedRevision;
                expectedText = member.AfterText;
                targetText = member.BeforeText;
            }
            else
            {
                Ra2EditableDocumentSession undone = entry.UndoneSessions!
                    .Single(session => session.DocumentId == member.DocumentId);
                expectedRevision = undone.EditRevision;
                expectedText = member.BeforeText;
                targetText = member.AfterText;
            }

            if (current.DocumentId != member.DocumentId ||
                current.EditRevision != expectedRevision ||
                !string.Equals(current.DocumentState.CurrentText, expectedText, StringComparison.Ordinal))
            {
                return Ra2ProjectCompoundUndoResult.Stale(isUndo, "A project transaction document changed after the last transition.");
            }

            Ra2EditableDocumentSession prepared = _sessionService.UpdateText(current, targetText);
            if (prepared.DocumentId != current.DocumentId || prepared.EditRevision != checked(current.EditRevision + 1))
                return Ra2ProjectCompoundUndoResult.Failed(isUndo, "A compound transition produced an inconsistent session.");
            replacements.Add(new(current, prepared));
        }

        Ra2ProjectDocumentSessionReplacement? active = replacements.FirstOrDefault(replacement =>
            string.Equals(replacement.Expected.DocumentState.FilePath, _store.ActiveFilePath, StringComparison.OrdinalIgnoreCase));
        if (active is not null && !string.Equals(_activeEditor.CurrentText, active.Expected.DocumentState.CurrentText, StringComparison.Ordinal))
            return Ra2ProjectCompoundUndoResult.Stale(isUndo, "The active editor changed after the project transaction.");

        long revisionBeforeCommit = _store.ProjectRevision;
        if (!_store.TryReplaceMany(replacements, out _))
            return Ra2ProjectCompoundUndoResult.Stale(isUndo, "The project transaction became stale before commit.");
        if (active is not null)
        {
            try
            {
                _activeEditor.SetText(active.Replacement.DocumentState.CurrentText);
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
                IReadOnlyList<Ra2ProjectDocumentSessionReplacement> rollback = replacements
                    .Select(replacement => new Ra2ProjectDocumentSessionReplacement(replacement.Replacement, replacement.Expected))
                    .ToArray();
                bool restored = _store.TryRestoreMany(rollback, revisionBeforeCommit, out _);
                try
                {
                    _activeEditor.SetText(active.Expected.DocumentState.CurrentText);
                }
                catch (Exception restoreException) when (!IsFatal(restoreException))
                {
                    _activeEditor.EnterReadOnlyFailSafe();
                }
                if (!restored)
                    _activeEditor.EnterReadOnlyFailSafe();
                return Ra2ProjectCompoundUndoResult.Failed(isUndo, "The active editor could not be synchronized; the compound transition was rolled back.");
            }
        }

        _compoundEntry = isUndo
            ? entry with { UndoneSessions = replacements.Select(replacement => replacement.Replacement).ToArray() }
            : entry with
            {
                Members = entry.Members.Zip(replacements, (member, replacement) =>
                    member with { AppliedRevision = replacement.Replacement.EditRevision }).ToArray(),
                UndoneSessions = null
            };
        return Ra2ProjectCompoundUndoResult.Completed(isUndo, replacements.Count, _store.DirtyDocumentCount);
    }

    private static Ra2ProjectEditApplyResult Failure(
        Ra2ProjectEditPreview preview,
        Ra2ProjectEditApplyOutcomeKind outcomeKind,
        string message)
        => Ra2ProjectEditApplyResult.Failed(outcomeKind, preview.ProjectPreviewId, message);

    private static bool IsFatal(Exception exception)
        => exception is OutOfMemoryException or AccessViolationException or AppDomainUnloadedException or BadImageFormatException or StackOverflowException;
}

internal enum Ra2ProjectCompoundUndoOutcomeKind
{
    Completed = 0,
    Unavailable,
    Stale,
    Failed
}

internal sealed record Ra2ProjectCompoundUndoResult(
    Ra2ProjectCompoundUndoOutcomeKind OutcomeKind,
    bool IsUndo,
    string Message,
    int AffectedDocumentCount,
    int DirtyDocumentCount)
{
    public bool Succeeded => OutcomeKind == Ra2ProjectCompoundUndoOutcomeKind.Completed;

    public static Ra2ProjectCompoundUndoResult Completed(bool isUndo, int affectedDocumentCount, int dirtyDocumentCount)
        => new(Ra2ProjectCompoundUndoOutcomeKind.Completed, isUndo, isUndo ? "Undid the project transaction." : "Redid the project transaction.", affectedDocumentCount, dirtyDocumentCount);

    public static Ra2ProjectCompoundUndoResult Unavailable(bool isUndo)
        => new(Ra2ProjectCompoundUndoOutcomeKind.Unavailable, isUndo, "No project transaction is available for this operation.", 0, 0);

    public static Ra2ProjectCompoundUndoResult Stale(bool isUndo, string message)
        => new(Ra2ProjectCompoundUndoOutcomeKind.Stale, isUndo, message, 0, 0);

    public static Ra2ProjectCompoundUndoResult Failed(bool isUndo, string message)
        => new(Ra2ProjectCompoundUndoOutcomeKind.Failed, isUndo, message, 0, 0);
}
