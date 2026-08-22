using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.Controllers.EditorSession;

internal enum Ra2EditorSessionOperationKind
{
    EnterEditMode,
    UpdateTextFromUser,
    ApplyProgrammaticText,
    Revert
}

internal sealed class Ra2EditorSessionOperationResult
{
    private Ra2EditorSessionOperationResult(
        Ra2EditorSessionOperationKind operationKind,
        bool success,
        Ra2EditableDocumentSession? session,
        string? textToSyncToEditor,
        int? caretOffset,
        bool shouldSetReadOnly,
        bool shouldSetEditable,
        string? message)
    {
        OperationKind = operationKind;
        Success = success;
        Session = session;
        TextToSyncToEditor = textToSyncToEditor;
        CaretOffset = caretOffset;
        ShouldSetReadOnly = shouldSetReadOnly;
        ShouldSetEditable = shouldSetEditable;
        Message = message;
    }

    public Ra2EditorSessionOperationKind OperationKind { get; }

    public bool Success { get; }

    public Ra2EditableDocumentSession? Session { get; }

    public string? TextToSyncToEditor { get; }

    public int? CaretOffset { get; }

    public bool ShouldSetReadOnly { get; }

    public bool ShouldSetEditable { get; }

    public string? Message { get; }

    public static Ra2EditorSessionOperationResult EnteredEditMode(
        Ra2EditableDocumentSession session,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        return new Ra2EditorSessionOperationResult(
            Ra2EditorSessionOperationKind.EnterEditMode,
            success: true,
            session,
            textToSyncToEditor: null,
            caretOffset: null,
            shouldSetReadOnly: false,
            shouldSetEditable: true,
            message);
    }

    public static Ra2EditorSessionOperationResult UpdatedFromUserText(
        Ra2EditableDocumentSession session,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        return new Ra2EditorSessionOperationResult(
            Ra2EditorSessionOperationKind.UpdateTextFromUser,
            success: true,
            session,
            textToSyncToEditor: null,
            caretOffset: null,
            shouldSetReadOnly: false,
            shouldSetEditable: false,
            message);
    }

    public static Ra2EditorSessionOperationResult AppliedProgrammaticText(
        Ra2EditableDocumentSession session,
        string textToSyncToEditor,
        int? caretOffset,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(textToSyncToEditor);
        return new Ra2EditorSessionOperationResult(
            Ra2EditorSessionOperationKind.ApplyProgrammaticText,
            success: true,
            session,
            textToSyncToEditor,
            caretOffset,
            shouldSetReadOnly: false,
            shouldSetEditable: false,
            message);
    }

    public static Ra2EditorSessionOperationResult Reverted(
        Ra2EditableDocumentSession session,
        string textToSyncToEditor,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(textToSyncToEditor);
        return new Ra2EditorSessionOperationResult(
            Ra2EditorSessionOperationKind.Revert,
            success: true,
            session,
            textToSyncToEditor,
            caretOffset: null,
            shouldSetReadOnly: false,
            shouldSetEditable: true,
            message);
    }

    public static Ra2EditorSessionOperationResult Failed(
        Ra2EditorSessionOperationKind operationKind,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Failure message cannot be empty.", nameof(message));

        return new Ra2EditorSessionOperationResult(
            operationKind,
            success: false,
            session: null,
            textToSyncToEditor: null,
            caretOffset: null,
            shouldSetReadOnly: false,
            shouldSetEditable: false,
            message);
    }
}
