using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.ViewModels.Editing;

internal sealed class Ra2EditorStateViewModelFactory : IRa2EditorStateViewModelFactory
{
    public Ra2EditorStateViewModel Create(Ra2EditableDocumentSession? session)
    {
        if (session is null)
            return new Ra2EditorStateViewModel(Ra2EditorDocumentState.ReadOnlyPreview, null, hasSession: false);

        return new Ra2EditorStateViewModel(
            session.DocumentState.State,
            session.DocumentState.FilePath,
            hasSession: true);
    }
}
