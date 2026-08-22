using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2TextChangeApplier : IRa2TextChangeApplier
{
    private readonly IRa2IniTextDocumentParser _parser;
    private readonly IRa2DirtyStateService _dirtyStateService;

    public Ra2TextChangeApplier(
        IRa2IniTextDocumentParser parser,
        IRa2DirtyStateService dirtyStateService)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _dirtyStateService = dirtyStateService ?? throw new ArgumentNullException(nameof(dirtyStateService));
    }

    public Ra2TextChangeApplyResult Apply(
        Ra2EditableDocumentState documentState,
        Ra2TextChange change)
    {
        ArgumentNullException.ThrowIfNull(documentState);
        ArgumentNullException.ThrowIfNull(change);

        if (documentState.State == Ra2EditorDocumentState.ReadOnlyPreview)
            return Ra2TextChangeApplyResult.Failed("Cannot apply text change while document is in read-only preview state.");

        if (!IsValidSpan(change.Span, documentState.CurrentText.Length, out string? errorMessage))
            return Ra2TextChangeApplyResult.Failed(errorMessage);

        string replacedText = documentState.CurrentText.Substring(change.Span.Start, change.Span.Length);
        bool isNoOp = string.Equals(replacedText, change.NewText, StringComparison.Ordinal);
        string nextText = isNoOp
            ? documentState.CurrentText
            : documentState.CurrentText.Remove(change.Span.Start, change.Span.Length).Insert(change.Span.Start, change.NewText);

        Ra2EditorDocumentState nextState = isNoOp
            ? documentState.State
            : _dirtyStateService.GetNextState(documentState.State, textChanged: true, saved: false);

        Ra2EditableDocumentState nextDocumentState = new(
            documentState.FilePath,
            documentState.OriginalText,
            nextText,
            nextState,
            documentState.EncodingMetadata);
        Ra2IniTextDocument textDocument = _parser.Parse(nextText);
        return Ra2TextChangeApplyResult.Succeeded(nextDocumentState, textDocument);
    }

    private static bool IsValidSpan(Ra2TextSpan span, int textLength, out string errorMessage)
    {
        if (span.Start < 0)
        {
            errorMessage = "Text change span start cannot be negative.";
            return false;
        }

        if (span.Length < 0)
        {
            errorMessage = "Text change span length cannot be negative.";
            return false;
        }

        if (span.Start > textLength)
        {
            errorMessage = "Text change span start is beyond the current text length.";
            return false;
        }

        if (span.Start + span.Length > textLength)
        {
            errorMessage = "Text change span extends beyond the current text length.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
