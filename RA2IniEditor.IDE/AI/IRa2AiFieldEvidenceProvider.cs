using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.AI;

internal interface IRa2AiFieldEvidenceProvider
{
    IReadOnlyList<Ra2AiFieldEvidence> Retrieve(
        IRa2FieldDefinitionProvider? fieldProvider,
        IFieldRegistryProvenanceProvider? provenanceProvider,
        Ra2SectionKind sectionKind,
        string? keyName,
        string? selectedText,
        string? promptText,
        int maxCount,
        Ra2AiConversationContext? conversationContext = null,
        Ra2AiCurrentSubject? currentSubject = null);
}
