using RA2IniEditor.Application.Language;

namespace RA2IniEditor.Application.Diagnostics;

internal sealed class Ra2ReferenceDiagnosticCatalogBuilder
{
    public Ra2ReferenceDiagnosticCatalog BuildFromCurrentDocument(
        string filePath,
        Ra2DocumentSemanticModel semanticModel)
    {
        ArgumentNullException.ThrowIfNull(semanticModel);

        return BuildFromDocuments([new Ra2ReferenceCatalogDocument(filePath, semanticModel)]);
    }

    public Ra2ReferenceDiagnosticCatalog BuildFromDocuments(IEnumerable<Ra2ReferenceCatalogDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        return new Ra2ReferenceDiagnosticCatalog(documents.SelectMany(document =>
            document.SemanticModel.Sections.Select(section =>
                new Ra2ReferenceDiagnosticCatalogEntry(
                    section.Name,
                    section.Kind,
                    document.FilePath,
                    section.HeaderLineNumber))));
    }
}

internal sealed class Ra2ReferenceCatalogDocument
{
    public Ra2ReferenceCatalogDocument(string filePath, Ra2DocumentSemanticModel semanticModel)
    {
        FilePath = filePath;
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
    }

    public string FilePath { get; }

    public Ra2DocumentSemanticModel SemanticModel { get; }
}
