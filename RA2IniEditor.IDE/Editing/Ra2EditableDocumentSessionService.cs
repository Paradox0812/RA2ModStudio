using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditableDocumentSessionService : IRa2EditableDocumentSessionService
{
    private readonly IRa2IniTextDocumentParser _textDocumentParser;
    private readonly IRa2DirtyStateService _dirtyStateService;

    public Ra2EditableDocumentSessionService(
        IRa2IniTextDocumentParser textDocumentParser,
        IRa2DirtyStateService dirtyStateService)
    {
        _textDocumentParser = textDocumentParser ?? throw new ArgumentNullException(nameof(textDocumentParser));
        _dirtyStateService = dirtyStateService ?? throw new ArgumentNullException(nameof(dirtyStateService));
    }

    public Ra2EditableDocumentSession StartEditing(string filePath, string text)
    {
        text ??= string.Empty;
        Ra2EditableDocumentState state = new(
            filePath,
            text,
            text,
            Ra2EditorDocumentState.EditableClean);

        return new Ra2EditableDocumentSession(state, _textDocumentParser.Parse(text));
    }

    public Ra2EditableDocumentSession StartEditing(
        string filePath,
        string text,
        Ra2EditorTextEncodingMetadata encodingMetadata)
    {
        text ??= string.Empty;
        Ra2EditableDocumentState state = new(
            filePath,
            text,
            text,
            Ra2EditorDocumentState.EditableClean,
            encodingMetadata);

        return new Ra2EditableDocumentSession(state, _textDocumentParser.Parse(text));
    }

    public Ra2EditableDocumentSession UpdateText(Ra2EditableDocumentSession session, string currentText)
    {
        ArgumentNullException.ThrowIfNull(session);
        currentText ??= string.Empty;

        Ra2EditorDocumentState nextState = string.Equals(
            currentText,
            session.DocumentState.OriginalText,
            StringComparison.Ordinal)
            ? Ra2EditorDocumentState.EditableClean
            : _dirtyStateService.GetNextState(
                NormalizeEditableState(session.DocumentState.State),
                textChanged: true,
                saved: false);

        Ra2EditableDocumentState state = new(
            session.DocumentState.FilePath,
            session.DocumentState.OriginalText,
            currentText,
            nextState,
            session.DocumentState.EncodingMetadata);

        return session.ContinueWith(state, _textDocumentParser.Parse(currentText));
    }

    public Ra2EditableDocumentSession MarkSaved(Ra2EditableDocumentSession session, string savedText)
    {
        ArgumentNullException.ThrowIfNull(session);
        savedText ??= string.Empty;

        Ra2EditableDocumentState state = new(
            session.DocumentState.FilePath,
            savedText,
            savedText,
            Ra2EditorDocumentState.EditableClean,
            session.DocumentState.EncodingMetadata);

        return session.ContinueWith(state, _textDocumentParser.Parse(savedText));
    }

    public Ra2EditableDocumentSession Revert(Ra2EditableDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        Ra2EditableDocumentState state = new(
            session.DocumentState.FilePath,
            session.DocumentState.OriginalText,
            session.DocumentState.OriginalText,
            Ra2EditorDocumentState.EditableClean,
            session.DocumentState.EncodingMetadata);

        return session.ContinueWith(state, _textDocumentParser.Parse(state.CurrentText));
    }

    private static Ra2EditorDocumentState NormalizeEditableState(Ra2EditorDocumentState state)
        => state == Ra2EditorDocumentState.ReadOnlyPreview
            ? Ra2EditorDocumentState.EditableClean
            : state;
}
