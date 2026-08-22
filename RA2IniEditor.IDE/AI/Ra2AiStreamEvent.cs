namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiStreamEventKind
{
    ContentDelta,
    ToolCallDelta,
    Completed
}

internal enum Ra2AiStreamFinishKind
{
    Unknown,
    Stop,
    Length,
    ContentFilter,
    ToolCalls,
    InsufficientSystemResource
}

/// <summary>表示一次 AI 流式响应中的有序内容增量或协议完成标记。</summary>
internal readonly record struct Ra2AiStreamEvent
{
    private Ra2AiStreamEvent(
        Ra2AiStreamEventKind kind,
        string text,
        Ra2AiStreamFinishKind finishKind,
        Ra2AiToolCallDelta toolCallDelta)
    {
        Kind = kind;
        Text = text;
        FinishKind = finishKind;
        ToolCallDelta = toolCallDelta;
    }

    internal Ra2AiStreamEventKind Kind { get; }

    internal string Text { get; }

    internal Ra2AiStreamFinishKind FinishKind { get; }

    internal Ra2AiToolCallDelta ToolCallDelta { get; }

    internal static Ra2AiStreamEvent CreateContentDelta(string text)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("A stream content delta cannot be empty.", nameof(text));

        return new Ra2AiStreamEvent(
            Ra2AiStreamEventKind.ContentDelta,
            text,
            Ra2AiStreamFinishKind.Unknown,
            default);
    }

    internal static Ra2AiStreamEvent CreateToolCallDelta(Ra2AiToolCallDelta delta)
        => new(
            Ra2AiStreamEventKind.ToolCallDelta,
            string.Empty,
            Ra2AiStreamFinishKind.Unknown,
            delta);

    internal static Ra2AiStreamEvent CreateCompleted(Ra2AiStreamFinishKind finishKind)
        => new(Ra2AiStreamEventKind.Completed, string.Empty, finishKind, default);
}
