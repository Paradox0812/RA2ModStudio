namespace RA2IniEditor.IDE.Editing;

internal interface IRa2IniEditPreviewService
{
    Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        CancellationToken cancellationToken = default);
}
