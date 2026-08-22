using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Language;

internal interface IRa2DefinitionProvider
{
    Ra2DefinitionTarget? GetDefinition(
        Ra2DocumentSemanticModel model,
        Ra2CaretContext context,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider);
}
