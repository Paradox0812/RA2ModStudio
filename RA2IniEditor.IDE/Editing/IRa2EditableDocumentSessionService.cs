namespace RA2IniEditor.IDE.Editing;

internal interface IRa2EditableDocumentSessionService
{
    Ra2EditableDocumentSession StartEditing(string filePath, string text);

    Ra2EditableDocumentSession StartEditing(
        string filePath,
        string text,
        Ra2EditorTextEncodingMetadata encodingMetadata);

    Ra2EditableDocumentSession UpdateText(Ra2EditableDocumentSession session, string currentText);

    Ra2EditableDocumentSession MarkSaved(Ra2EditableDocumentSession session, string savedText);

    Ra2EditableDocumentSession Revert(Ra2EditableDocumentSession session);
}
