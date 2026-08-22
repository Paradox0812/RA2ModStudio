using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.ViewModels.Editing;

internal interface IRa2EditorStateViewModelFactory
{
    Ra2EditorStateViewModel Create(Ra2EditableDocumentSession? session);
}
