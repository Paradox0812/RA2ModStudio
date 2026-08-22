using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Language;

internal interface IRa2HoverProvider
{
    Ra2HoverInfo? GetHover(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider);

    Ra2HoverInfo? GetHover(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        IRa2FieldDisplayResolver fieldDisplayResolver,
        IFieldRegistryProvenanceProvider provenanceProvider);
}
