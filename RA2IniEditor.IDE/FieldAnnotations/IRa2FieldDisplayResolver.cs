using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal interface IRa2FieldDisplayResolver
{
    Ra2FieldDisplayInfo Resolve(Ra2SectionKind sectionKind, string key);

    IReadOnlyList<Ra2FieldDisplayInfo> GetFields(Ra2SectionKind sectionKind);
}
