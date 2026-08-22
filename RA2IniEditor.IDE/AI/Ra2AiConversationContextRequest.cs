using System.Collections.Generic;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiConversationContextRequest
{
    public const int DefaultLastTurns = 6;

    public const int DefaultMaxCharacters = 6000;

    public const int DefaultMaxSingleTurnCharacters = 2000;

    public IReadOnlyList<Ra2AiConversationTurn> Turns { get; init; } = [];

    public int LastTurns { get; init; } = DefaultLastTurns;

    public int MaxCharacters { get; init; } = DefaultMaxCharacters;

    public int MaxSingleTurnCharacters { get; init; } = DefaultMaxSingleTurnCharacters;
}
