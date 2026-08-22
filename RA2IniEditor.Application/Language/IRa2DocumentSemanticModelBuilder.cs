using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Language;

internal interface IRa2DocumentSemanticModelBuilder
{
    Ra2DocumentSemanticModel Build(Ra2DocumentSnapshot snapshot, IRa2FieldDefinitionProvider fieldProvider);
}
