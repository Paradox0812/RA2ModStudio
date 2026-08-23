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
            capabilityMode,
            capabilityMode == Ra2AiCapabilityMode.AdvisoryOnly ? Ra2AiUserMode.Chat : Ra2AiUserMode.Work,
            "ini-document");
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

    public async Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiInteractionRoute interactionRoute,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(onContentDelta);
        if (interactionRoute.UserMode == Ra2AiUserMode.Work)
        {
            return await SendWorkStreamingAsync(
                userPrompt,
                context,
                conversationContext,
                currentSubject,
                interactionRoute.EditAvailability,
                onContentDelta,
                cancellationToken).ConfigureAwait(false);
        }

        Ra2AiRequest request = BuildRequest(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            interactionRoute.CapabilityMode,
            interactionRoute.UserMode,
            interactionRoute.DomainIntentId);
        Ra2AiResponse response = await _client.SendStreamingAsync(
            request,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);
        if (request.ToolChoice == Ra2AiToolChoiceMode.Required &&
            response.Kind == Ra2AiResponseKind.Success)
        {
            response = Ra2AiResponse.CreateAuthoringToolNotInvoked(response.Text, response.Diagnostics);
        }

        return new Ra2AiAssistantPipelineResult(request, response);
    }

    private async Task<Ra2AiAssistantPipelineResult> SendWorkStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiEditAvailabilityKind editAvailability,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        Ra2AiRequest analysisRequest = Ra2AiIntentAnalysisStage.BuildRequest(
            userPrompt,
            context,
            currentSubject);
        Ra2AiResponse analysisResponse = await _client.SendAsync(
            analysisRequest,
            cancellationToken).ConfigureAwait(false);
        if (!analysisResponse.IsSuccessfulTerminal)
        {
            return new Ra2AiAssistantPipelineResult(analysisRequest, analysisResponse)
            {
                IntentAnalysisRequest = analysisRequest,
                IntentAnalysisResponse = analysisResponse
            };
        }

        if (!Ra2AiIntentAnalysisStage.TryParse(
                analysisResponse,
                out Ra2AiIntentAnalysisPackage? package,
                out string failureMessage) ||
            package is null)
        {
            Ra2AiResponse failure = Ra2AiResponse.CreateProviderFailure(
                Ra2AiFailureKind.ProtocolError,
                failureMessage,
                diagnostics: analysisResponse.Diagnostics);
            return new Ra2AiAssistantPipelineResult(analysisRequest, failure)
            {
                IntentAnalysisRequest = analysisRequest,
                IntentAnalysisResponse = analysisResponse
            };
        }

        Ra2AiInteractionRoute resolvedRoute = Ra2AiIntentAnalysisStage.ResolveRoute(
            package,
            editAvailability);
        if (resolvedRoute.Kind == Ra2AiInteractionRouteKind.EditUnavailable)
        {
            Ra2AiResponse failure = Ra2AiResponse.CreateProviderFailure(
                Ra2AiFailureKind.ProtocolError,
                "意图分析确认该请求需要结构化修改，但当前没有可用的编辑快照。",
                diagnostics: analysisResponse.Diagnostics);
            return new Ra2AiAssistantPipelineResult(analysisRequest, failure)
            {
                IntentAnalysisRequest = analysisRequest,
                IntentAnalysisResponse = analysisResponse,
                IntentAnalysisPackage = package,
                ResolvedInteractionRoute = resolvedRoute
            };
        }

        Ra2AiRequest executionRequest = BuildRequest(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            resolvedRoute.CapabilityMode,
            Ra2AiUserMode.Work,
            resolvedRoute.DomainIntentId,
            package);
        Ra2AiResponse executionResponse = await _client.SendStreamingAsync(
            executionRequest,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);
        if (executionRequest.ToolChoice == Ra2AiToolChoiceMode.Required &&
            executionResponse.Kind == Ra2AiResponseKind.Success)
        {
            executionResponse = Ra2AiResponse.CreateAuthoringToolNotInvoked(
                executionResponse.Text,
                executionResponse.Diagnostics);
        }

        return new Ra2AiAssistantPipelineResult(executionRequest, executionResponse)
        {
            IntentAnalysisRequest = analysisRequest,
            IntentAnalysisResponse = analysisResponse,
            IntentAnalysisPackage = package,
            ResolvedInteractionRoute = resolvedRoute
        };
    }

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
        Ra2AiCapabilityMode capabilityMode = Ra2AiCapabilityMode.AdvisoryOnly,
        Ra2AiUserMode userMode = Ra2AiUserMode.Chat,
        string domainIntentId = "ini-document",
        Ra2AiIntentAnalysisPackage? intentAnalysisPackage = null)
        => _promptBuilder.Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = userPrompt,
            Context = context,
            ConversationContext = conversationContext,
            CurrentSubject = currentSubject,
            CapabilityMode = capabilityMode,
            UserMode = userMode,
            DomainIntentId = domainIntentId,
            IntentAnalysisPackage = intentAnalysisPackage
        });
}

internal sealed record Ra2AiAssistantPipelineResult(Ra2AiRequest Request, Ra2AiResponse Response)
{
    public Ra2AiRequest? IntentAnalysisRequest { get; init; }

    public Ra2AiResponse? IntentAnalysisResponse { get; init; }

    public Ra2AiIntentAnalysisPackage? IntentAnalysisPackage { get; init; }

    public Ra2AiInteractionRoute? ResolvedInteractionRoute { get; init; }
}
