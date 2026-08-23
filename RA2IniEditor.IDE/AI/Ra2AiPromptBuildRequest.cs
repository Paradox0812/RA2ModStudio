namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiPromptBuildRequest
{
    public Ra2AiIntent Intent { get; init; } = Ra2AiIntent.Auto;

    public string UserPrompt { get; init; } = string.Empty;

    public Ra2AiContext Context { get; init; } = null!;

    public Ra2AiConversationContext? ConversationContext { get; init; }

    public Ra2AiCurrentSubject? CurrentSubject { get; init; }

    public Ra2AiCapabilityMode CapabilityMode { get; init; } =
        Ra2AiCapabilityMode.AdvisoryOnly;

    public Ra2AiUserMode UserMode { get; init; } = Ra2AiUserMode.Chat;

    public string DomainIntentId { get; init; } = "ini-document";

    public Ra2AiIntentAnalysisPackage? IntentAnalysisPackage { get; init; }
}
