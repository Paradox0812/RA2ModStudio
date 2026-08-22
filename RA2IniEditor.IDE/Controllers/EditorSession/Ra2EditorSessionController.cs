using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.Controllers.EditorSession;

internal interface IRa2EditorSessionController
{
    Ra2EditorSessionOperationResult EnterEditMode(Ra2EditorSessionEnterRequest request);

    Ra2EditorSessionOperationResult Revert(Ra2EditorSessionRevertRequest request);

    Ra2EditorSessionOperationResult UpdateTextFromUser(Ra2EditorSessionUpdateTextRequest request);

    Ra2EditorSessionOperationResult ApplyProgrammaticText(
        Ra2EditorSessionApplyProgrammaticTextRequest request);
}

internal sealed class Ra2EditorSessionController : IRa2EditorSessionController
{
    private readonly IRa2EditableDocumentSessionService _sessionService;

    public Ra2EditorSessionController(IRa2EditableDocumentSessionService sessionService)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public Ra2EditorSessionOperationResult EnterEditMode(Ra2EditorSessionEnterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2EditableDocumentSession session = _sessionService.StartEditing(
            request.FilePath,
            request.CurrentText,
            request.EncodingMetadata);
        return Ra2EditorSessionOperationResult.EnteredEditMode(
            session,
            "Opened editable in-memory session.");
    }

    public Ra2EditorSessionOperationResult Revert(Ra2EditorSessionRevertRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Session is null)
        {
            return Ra2EditorSessionOperationResult.Failed(
                Ra2EditorSessionOperationKind.Revert,
                "There are no in-memory changes to revert.");
        }

        Ra2EditableDocumentSession revertedSession = _sessionService.Revert(request.Session);
        return Ra2EditorSessionOperationResult.Reverted(
            revertedSession,
            revertedSession.DocumentState.CurrentText,
            "Reverted in-memory changes.");
    }

    public Ra2EditorSessionOperationResult UpdateTextFromUser(Ra2EditorSessionUpdateTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Session is null)
        {
            return Ra2EditorSessionOperationResult.Failed(
                Ra2EditorSessionOperationKind.UpdateTextFromUser,
                "There is no editable session to update.");
        }

        Ra2EditableDocumentSession updatedSession = _sessionService.UpdateText(
            request.Session,
            request.CurrentText);
        return Ra2EditorSessionOperationResult.UpdatedFromUserText(updatedSession);
    }

    public Ra2EditorSessionOperationResult ApplyProgrammaticText(
        Ra2EditorSessionApplyProgrammaticTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Ra2EditableDocumentSession? session = request.Session;
        if (session is null)
        {
            return ProgrammaticTextFailure(
                "There is no editable session to update.");
        }

        if (session.DocumentState.State == Ra2EditorDocumentState.ReadOnlyPreview)
        {
            return ProgrammaticTextFailure(
                "The current document is read-only.");
        }

        if (request.ExpectedDocumentId == Guid.Empty ||
            session.DocumentId != request.ExpectedDocumentId ||
            session.EditRevision != request.ExpectedEditRevision ||
            !string.Equals(
                session.DocumentState.CurrentText,
                request.ExpectedCurrentText,
                StringComparison.Ordinal))
        {
            return ProgrammaticTextFailure(
                "The editable session no longer matches the preview snapshot.");
        }

        if (string.Equals(
                session.DocumentState.CurrentText,
                request.CandidateText,
                StringComparison.Ordinal))
        {
            return ProgrammaticTextFailure(
                "The programmatic edit would not change the current document.");
        }

        Ra2EditableDocumentSession updatedSession = _sessionService.UpdateText(
            session,
            request.CandidateText);
        if (updatedSession.DocumentId != session.DocumentId ||
            updatedSession.EditRevision != checked(session.EditRevision + 1) ||
            !string.Equals(
                updatedSession.DocumentState.CurrentText,
                request.CandidateText,
                StringComparison.Ordinal))
        {
            return ProgrammaticTextFailure(
                "The editable session service returned an inconsistent transaction result.");
        }

        return Ra2EditorSessionOperationResult.AppliedProgrammaticText(
            updatedSession,
            request.CandidateText,
            Math.Clamp(request.RequestedCaretOffset, 0, request.CandidateText.Length),
            "Applied programmatic text to the editable in-memory session.");
    }

    private static Ra2EditorSessionOperationResult ProgrammaticTextFailure(string message)
        => Ra2EditorSessionOperationResult.Failed(
            Ra2EditorSessionOperationKind.ApplyProgrammaticText,
            message);
}

internal sealed class Ra2EditorSessionEnterRequest
{
    public Ra2EditorSessionEnterRequest(
        string filePath,
        string currentText,
        Ra2EditorTextEncodingMetadata? encodingMetadata = null)
    {
        FilePath = filePath ?? string.Empty;
        CurrentText = currentText ?? string.Empty;
        EncodingMetadata = encodingMetadata ?? Ra2EditorTextEncodingMetadata.Unknown;
    }

    public string FilePath { get; }

    public string CurrentText { get; }

    public Ra2EditorTextEncodingMetadata EncodingMetadata { get; }
}

internal sealed class Ra2EditorSessionRevertRequest
{
    public Ra2EditorSessionRevertRequest(Ra2EditableDocumentSession? session)
    {
        Session = session;
    }

    public Ra2EditableDocumentSession? Session { get; }
}

internal sealed class Ra2EditorSessionUpdateTextRequest
{
    public Ra2EditorSessionUpdateTextRequest(
        Ra2EditableDocumentSession? session,
        string currentText)
    {
        Session = session;
        CurrentText = currentText ?? string.Empty;
    }

    public Ra2EditableDocumentSession? Session { get; }

    public string CurrentText { get; }
}

internal sealed class Ra2EditorSessionApplyProgrammaticTextRequest
{
    public Ra2EditorSessionApplyProgrammaticTextRequest(
        Ra2EditableDocumentSession? session,
        Guid expectedDocumentId,
        int expectedEditRevision,
        string expectedCurrentText,
        string candidateText,
        int requestedCaretOffset)
    {
        if (expectedEditRevision < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedEditRevision));

        Session = session;
        ExpectedDocumentId = expectedDocumentId;
        ExpectedEditRevision = expectedEditRevision;
        ExpectedCurrentText = expectedCurrentText ?? string.Empty;
        CandidateText = candidateText ?? string.Empty;
        RequestedCaretOffset = requestedCaretOffset;
    }

    public Ra2EditableDocumentSession? Session { get; }

    public Guid ExpectedDocumentId { get; }

    public int ExpectedEditRevision { get; }

    public string ExpectedCurrentText { get; }

    public string CandidateText { get; }

    public int RequestedCaretOffset { get; }
}
