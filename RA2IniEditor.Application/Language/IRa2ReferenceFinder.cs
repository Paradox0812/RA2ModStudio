namespace RA2IniEditor.Application.Language;

internal interface IRa2ReferenceFinder
{
    Ra2ReferenceResult FindReferences(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        Ra2TextSpan? selectionSpan = null);
}
