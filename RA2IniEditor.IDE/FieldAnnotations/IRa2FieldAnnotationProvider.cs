using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal interface IRa2FieldAnnotationProvider
{
    Ra2FieldAnnotationEntry? Find(Ra2SectionKind sectionKind, string key);
}
