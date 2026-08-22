namespace RA2IniEditor.IDE.AI;

internal interface IRa2AiConversationContextProvider
{
    Ra2AiConversationContext BuildContext(Ra2AiConversationContextRequest request);
}
