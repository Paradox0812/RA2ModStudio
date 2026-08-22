namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiConversationRole
{
    User,
    Assistant
}

internal enum Ra2AiConversationTurnState
{
    Completed,
    InProgress,
    Incomplete,
    Error
}

internal sealed class Ra2AiConversationTurn
{
    public Ra2AiConversationRole Role { get; init; }

    public string Text { get; init; } = string.Empty;

    public bool IsDraftResponse { get; init; }

    public Ra2AiConversationTurnState State { get; init; } = Ra2AiConversationTurnState.Completed;

    /// <summary>
    /// 指示该轮对话是否允许进入后续 AI 请求的历史上下文。
    /// </summary>
    public bool IsContextEligible { get; init; } = true;
}
