namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveCurrentFilePlanRequest
{
    public Ra2SaveCurrentFilePlanRequest(
        Ra2EditableDocumentSession? session,
        bool isReadOnlyPreview)
    {
        Session = session;
        IsReadOnlyPreview = isReadOnlyPreview;
    }

    public Ra2EditableDocumentSession? Session { get; }

    public bool IsReadOnlyPreview { get; }
}
