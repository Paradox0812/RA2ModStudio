namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiResponseKind
{
    Success,
    Incomplete,
    Cancelled,
    Timeout,
    ProviderError,
    MissingConfiguration,
    ToolCalls,
    AuthoringToolNotInvoked
}
