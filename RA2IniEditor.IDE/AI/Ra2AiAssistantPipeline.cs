using System.Threading;
using System.Threading.Tasks;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiAssistantPipeline
{
    private readonly IRa2AiPromptBuilder _promptBuilder;
    private readonly IRa2AiClient _client;

    public Ra2AiAssistantPipeline(IRa2AiPromptBuilder promptBuilder, IRa2AiClient client)
    {
        _promptBuilder = promptBuilder;
        _client = client;
    }

    public async Task<Ra2AiAssistantPipelineResult> SendAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        CancellationToken cancellationToken)
    {
        Ra2AiRequest request = BuildRequest(userPrompt, context, conversationContext, currentSubject);
        Ra2AiResponse response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return new Ra2AiAssistantPipelineResult(request, response);
    }

    public async Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiCapabilityMode capabilityMode,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(onContentDelta);

        Ra2AiRequest request = BuildRequest(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            capabilityMode);
        Ra2AiResponse response = await _client.SendStreamingAsync(
            request,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);
        if (request.ToolChoice == Ra2AiToolChoiceMode.Required &&
            response.Kind == Ra2AiResponseKind.Success)
        {
            response = Ra2AiResponse.CreateAuthoringToolNotInvoked(
                response.Text,
                response.Diagnostics);
        }
        return new Ra2AiAssistantPipelineResult(request, response);
    }

    public Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiInteractionRoute interactionRoute,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
        => SendStreamingAsync(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            interactionRoute.CapabilityMode,
            onContentDelta,
            cancellationToken);

    public Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
        => SendStreamingAsync(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            Ra2AiCapabilityMode.AdvisoryOnly,
            onContentDelta,
            cancellationToken);

    public Task<Ra2AiAssistantPipelineResult> SendAsync(
        string userPrompt,
        Ra2AiContext context,
        CancellationToken cancellationToken)
        => SendAsync(userPrompt, context, conversationContext: null, currentSubject: null, cancellationToken);

    private Ra2AiRequest BuildRequest(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiCapabilityMode capabilityMode = Ra2AiCapabilityMode.AdvisoryOnly)
        => _promptBuilder.Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = userPrompt,
            Context = context,
            ConversationContext = conversationContext,
            CurrentSubject = currentSubject,
            CapabilityMode = capabilityMode
        });
}

internal sealed record Ra2AiAssistantPipelineResult(Ra2AiRequest Request, Ra2AiResponse Response);
