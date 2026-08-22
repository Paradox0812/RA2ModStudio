using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2TextChangeApplyResult
{
    private Ra2TextChangeApplyResult(
        bool success,
        Ra2EditableDocumentState? documentState,
        Ra2IniTextDocument? textDocument,
        string? errorMessage)
    {
        Success = success;
        DocumentState = documentState;
        TextDocument = textDocument;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public Ra2EditableDocumentState? DocumentState { get; }

    public Ra2IniTextDocument? TextDocument { get; }

    public string? ErrorMessage { get; }

    public static Ra2TextChangeApplyResult Succeeded(
        Ra2EditableDocumentState documentState,
        Ra2IniTextDocument textDocument)
    {
        ArgumentNullException.ThrowIfNull(documentState);
        ArgumentNullException.ThrowIfNull(textDocument);

        return new Ra2TextChangeApplyResult(true, documentState, textDocument, null);
    }

    public static Ra2TextChangeApplyResult Failed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Apply failure message cannot be empty.", nameof(errorMessage));

        return new Ra2TextChangeApplyResult(false, null, null, errorMessage);
    }
}
