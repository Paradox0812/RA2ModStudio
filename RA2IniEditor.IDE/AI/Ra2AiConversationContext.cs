using System.Collections.Generic;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiConversationContext
{
    public IReadOnlyList<Ra2AiConversationTurn> Turns { get; init; } = [];

    public int TotalCharacterCount { get; init; }

    public bool WasTruncated { get; init; }
}
