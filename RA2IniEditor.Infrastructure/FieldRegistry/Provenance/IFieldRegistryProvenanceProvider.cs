using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

internal interface IFieldRegistryProvenanceProvider
{
    FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(
        Ra2SectionKind sectionKind,
        string key);
}
