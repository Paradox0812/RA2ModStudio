namespace RA2IniEditor.IDE.AI;

internal interface IRa2AiCurrentSubjectExtractor
{
    Ra2AiCurrentSubject Extract(Ra2AiConversationContext conversationContext);
}
