using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditableDocumentSession
{
    public Ra2EditableDocumentSession(
        Ra2EditableDocumentState documentState,
        Ra2IniTextDocument textDocument)
        : this(documentState, textDocument, Guid.NewGuid(), 0)
    {
    }

    private Ra2EditableDocumentSession(
        Ra2EditableDocumentState documentState,
        Ra2IniTextDocument textDocument,
        Guid documentId,
        int editRevision)
    {
        DocumentState = documentState ?? throw new ArgumentNullException(nameof(documentState));
        TextDocument = textDocument ?? throw new ArgumentNullException(nameof(textDocument));
        DocumentId = documentId == Guid.Empty
            ? throw new ArgumentException("Document identity cannot be empty.", nameof(documentId))
            : documentId;
        EditRevision = editRevision >= 0
            ? editRevision
            : throw new ArgumentOutOfRangeException(nameof(editRevision));
    }

    public Ra2EditableDocumentState DocumentState { get; }

    public Ra2IniTextDocument TextDocument { get; }

    public Guid DocumentId { get; }

    public int EditRevision { get; }

    public Ra2EditableDocumentSession ContinueWith(
        Ra2EditableDocumentState documentState,
        Ra2IniTextDocument textDocument)
    {
        ArgumentNullException.ThrowIfNull(documentState);
        ArgumentNullException.ThrowIfNull(textDocument);

        int nextRevision = string.Equals(
            DocumentState.CurrentText,
            documentState.CurrentText,
            StringComparison.Ordinal)
            ? EditRevision
            : checked(EditRevision + 1);
        return new Ra2EditableDocumentSession(documentState, textDocument, DocumentId, nextRevision);
    }
}
