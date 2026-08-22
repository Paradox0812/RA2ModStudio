using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal interface IRa2DocumentSemanticModelBuilder
{
    Ra2DocumentSemanticModel Build(Ra2DocumentSnapshot snapshot, IRa2FieldDefinitionProvider fieldProvider);
}
