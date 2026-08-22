using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal interface IRa2EffectiveFieldCatalog
{
    IReadOnlyList<Ra2EffectiveFieldItem> GetApplicableFields(Ra2SectionKind sectionKind);

    IReadOnlyList<Ra2EffectiveFieldItem> GetCommonFields();

    IReadOnlyList<Ra2EffectiveFieldItem> GetSpecificFields(Ra2SectionKind sectionKind);

    IReadOnlyList<Ra2EffectiveFieldItem> GetAllFields();
}
