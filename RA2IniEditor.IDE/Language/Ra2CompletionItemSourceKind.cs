namespace RA2IniEditor.IDE.Language;

internal enum Ra2CompletionItemSourceKind
{
    FieldRegistry = 0,
    CurrentDocumentSection = 1,
    CurrentDocumentUnknownFallback = 2,
    BuiltInValueCatalog = 3,
    UserValueCatalog = 4,
    ProjectValueCatalog = 5,
    CurrentDocumentInference = 6
}
