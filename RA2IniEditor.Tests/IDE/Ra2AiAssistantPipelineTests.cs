using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Language;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiAssistantPipelineTests
{
    [Theory]
    [InlineData("解释 Strength", (int)Ra2AiEditAvailabilityKind.Available, (int)Ra2AiInteractionRouteKind.Advisory)]
    [InlineData("不要修改，只给代码示例", (int)Ra2AiEditAvailabilityKind.Available, (int)Ra2AiInteractionRouteKind.Advisory)]
    [InlineData("把当前文件 [E1] 下的 Strength 修改为 150", (int)Ra2AiEditAvailabilityKind.Available, (int)Ra2AiInteractionRouteKind.EditExplicit)]
    [InlineData("将当前文档 Primary 设置为 M60", (int)Ra2AiEditAvailabilityKind.Available, (int)Ra2AiInteractionRouteKind.EditExplicit)]
    [InlineData("Strength 150", (int)Ra2AiEditAvailabilityKind.Available, (int)Ra2AiInteractionRouteKind.EditAmbiguous)]
    [InlineData("优化一下这个单位", (int)Ra2AiEditAvailabilityKind.Available, (int)Ra2AiInteractionRouteKind.EditAmbiguous)]
    [InlineData("把当前文件 Strength 修改为 150", (int)Ra2AiEditAvailabilityKind.NoEditableDocument, (int)Ra2AiInteractionRouteKind.EditUnavailable)]
    [InlineData("把当前文件 Strength 修改为 150", (int)Ra2AiEditAvailabilityKind.UnsupportedEndpoint, (int)Ra2AiInteractionRouteKind.EditUnavailable)]
    [InlineData("把当前文件 Strength 修改为 150", (int)Ra2AiEditAvailabilityKind.ResourceLimitExceeded, (int)Ra2AiInteractionRouteKind.EditUnavailable)]
    [InlineData("只解释当前文件 Strength", (int)Ra2AiEditAvailabilityKind.ResourceLimitExceeded, (int)Ra2AiInteractionRouteKind.Advisory)]
    public void InteractionRouter_UsesConservativeDeterministicAuthority(
        string prompt,
        int availability,
        int expectedKind)
    {
        Ra2AiInteractionRoute route = Ra2AiInteractionRouter.Resolve(
            prompt,
            (Ra2AiEditAvailabilityKind)availability);

        Assert.Equal((Ra2AiInteractionRouteKind)expectedKind, route.Kind);
        Assert.Equal(
            route.Kind == Ra2AiInteractionRouteKind.EditExplicit
                ? Ra2AiCapabilityMode.CurrentDocumentEditPreview
                : Ra2AiCapabilityMode.AdvisoryOnly,
            route.CapabilityMode);
    }

    [Fact]
    public async Task SendAsync_BuildsPromptWithPromptBuilderAndSendsRequestToClient()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateSuccess("fake response"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendAsync(
            "Explain Strength.",
            CreateContext(),
            CancellationToken.None);

        Assert.Same(result.Request, client.LastRequest);
        Assert.Equal(Ra2AiIntent.Auto, result.Request.Intent);
        Assert.Equal("Explain Strength.", result.Request.UserPrompt);
        Assert.Contains("## Application Rules", result.Request.PromptText);
        Assert.Contains("## Current IDE Context", result.Request.PromptText);
        Assert.Contains("Section: HTNK (Unit)", result.Request.PromptText);
        Assert.Contains("Key / Value: Strength = 400", result.Request.PromptText);
        Assert.Equal(Ra2AiResponseKind.Success, result.Response.Kind);
        Assert.Equal("fake response", result.Response.Text);
    }

    [Fact]
    public async Task SendAsync_UsesClientProviderErrorWithoutDeepSeekNetworkOrApiKey()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateProviderFailure(
            Ra2AiFailureKind.ProtocolError,
            "fake provider error"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendAsync(
            "Explain current field.",
            CreateContext(),
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, result.Response.Kind);
        Assert.Equal("fake provider error", result.Response.ErrorMessage);
        Assert.NotNull(client.LastRequest);
        Assert.DoesNotContain("DeepSeek API key", result.Request.PromptText);
        Assert.DoesNotContain("ProviderEndpoint", result.Request.PromptText);
        Assert.DoesNotContain("NetworkRequest", result.Request.PromptText);
    }

    [Fact]
    public async Task SendAsync_PassesConversationContextAndCurrentSubjectToPromptBuilder()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateSuccess("fake response"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);
        Ra2AiConversationContext conversationContext = new()
        {
            Turns =
            [
                new Ra2AiConversationTurn
                {
                    Role = Ra2AiConversationRole.User,
                    Text = "生成一个轻型防空车。",
                    IsDraftResponse = false
                },
                new Ra2AiConversationTurn
                {
                    Role = Ra2AiConversationRole.Assistant,
                    Text = "```ini\n[LAAV]\nStrength=220\nPrimary=LAAVMissile\n```",
                    IsDraftResponse = true
                }
            ],
            TotalCharacterCount = 72,
            WasTruncated = false
        };
        Ra2AiCurrentSubject currentSubject = new()
        {
            Kind = Ra2AiSubjectKind.Unit,
            SubjectId = "LAAV",
            Source = Ra2AiSubjectSource.LastAssistantDraft,
            Summary = "上一轮 AI 草稿中的单位 [LAAV]；不是项目文件状态。",
            Confidence = 0.9,
            IsDraft = true
        };

        Ra2AiAssistantPipelineResult result = await pipeline.SendAsync(
            "在这个单位基础上继续修改。",
            CreateContext(),
            conversationContext,
            currentSubject,
            CancellationToken.None);

        Assert.Contains("## Conversation Context", result.Request.PromptText);
        Assert.Contains("## Current Subject", result.Request.PromptText);
        Assert.Contains("SubjectId: LAAV", result.Request.PromptText);
        Assert.Contains("SubjectKind: Unit", result.Request.PromptText);
        Assert.Contains("AssistantDraftResponse: True", result.Request.PromptText);
        Assert.Contains("not applied file state", result.Request.PromptText);
        Assert.Contains("在这个单位基础上继续修改。", result.Request.PromptText);
    }

    [Fact]
    public async Task SendAsync_PropagatesPreCancelledTokenToClient()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateSuccess("unused"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AiAssistantPipelineResult result = await pipeline.SendAsync(
            "Explain current field.",
            CreateContext(),
            source.Token);

        Assert.True(client.LastCancellationToken.IsCancellationRequested);
        Assert.Equal(Ra2AiResponseKind.Cancelled, result.Response.Kind);
    }

    [Fact]
    public async Task SendAsync_CanUseDeepSeekClientWithFakeHttpHandlerWithoutLiveNetwork()
    {
        RecordingHttpHandler handler = new(CreateJsonResponse("deepseek response"));
        DeepSeekRa2AiClient client = new(CreateDeepSeekOptions(apiKey: "test-pipeline-key"), new HttpClient(handler));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendAsync(
            "Explain Strength.",
            CreateContext(),
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, result.Response.Kind);
        Assert.Equal("deepseek response", result.Response.Text);
        Assert.Equal(1, handler.CallCount);
        Assert.DoesNotContain("test-pipeline-key", result.Response.Text);
        Assert.Contains("## Application Rules", handler.LastRequestBody);
    }

    [Fact]
    public async Task SendAsync_DeepSeekMissingApiKeyMapsMissingConfigurationWithoutNetwork()
    {
        RecordingHttpHandler handler = new(CreateJsonResponse("unused"));
        DeepSeekRa2AiClient client = new(CreateDeepSeekOptions(apiKey: string.Empty), new HttpClient(handler));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendAsync(
            "Explain Strength.",
            CreateContext(),
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, result.Response.Kind);
        Assert.Equal(0, handler.CallCount);
        Assert.DoesNotContain("test-pipeline-key", result.Response.ErrorMessage);
    }

    [Fact]
    public async Task SendStreamingAsync_BuildsRequestOnceAndPassesOrderedDeltasThrough()
    {
        RecordingAiClient client = new(
            Ra2AiResponse.CreateSuccess("first second"),
            ["first", " second"]);
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);
        List<string> deltas = [];

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "Explain Strength.",
            CreateContext(),
            conversationContext: null,
            currentSubject: null,
            (delta, _) =>
            {
                deltas.Add(delta);
                return ValueTask.CompletedTask;
            },
            CancellationToken.None);

        Assert.Same(result.Request, client.LastRequest);
        Assert.Equal(["first", " second"], deltas);
        Assert.Equal("first second", result.Response.Text);
        Assert.Equal(1, client.StreamingCallCount);
        Assert.Contains("## Application Rules", result.Request.PromptText);
        Assert.Contains("## Current IDE Context", result.Request.PromptText);
    }

    [Fact]
    public async Task SendStreamingAsync_ExistingOverloadRemainsAdvisoryOnly()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateSuccess("answer"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "Explain Strength.",
            CreateContext(),
            conversationContext: null,
            currentSubject: null,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Empty(result.Request.Tools);
        Assert.Equal(Ra2AiToolChoiceMode.None, result.Request.ToolChoice);
    }

    [Fact]
    public async Task SendStreamingAsync_ExplicitAuthoringCapabilityDeclaresPreviewTool()
    {
        Ra2AiToolCall call = new(
            "call-1",
            Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
            """{"summary":"Update","operations":[]}""");
        RecordingAiClient client = new(Ra2AiResponse.CreateToolCalls([call]));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "Update Strength.",
            CreateContext(),
            conversationContext: null,
            currentSubject: null,
            Ra2AiCapabilityMode.CurrentDocumentEditPreview,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ToolCalls, result.Response.Kind);
        Assert.Equal(Ra2AiToolChoiceMode.Required, result.Request.ToolChoice);
        Assert.Equal(
            Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
            Assert.Single(result.Request.Tools).Name);
    }

    [Fact]
    public async Task SendStreamingAsync_RequiredToolPlainTextBecomesTypedLocalFailure()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateSuccess("```ini\n[E1]\nStrength=150\n```"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "把当前文件 [E1] 下的 Strength 修改为 150",
            CreateContext(),
            conversationContext: null,
            currentSubject: null,
            Ra2AiCapabilityMode.CurrentDocumentEditPreview,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.AuthoringToolNotInvoked, result.Response.Kind);
        Assert.False(result.Response.IsSuccessfulTerminal);
        Assert.Empty(result.Response.ToolCalls);
    }

    [Fact]
    public async Task SendStreamingAsync_AdvisoryPlainTextRemainsSuccess()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateSuccess("explanation"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "解释 Strength",
            CreateContext(),
            conversationContext: null,
            currentSubject: null,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, result.Response.Kind);
    }

    [Fact]
    public async Task SendStreamingAsync_RouteOverloadUsesResolvedCapabilityMode()
    {
        RecordingAiClient client = new(Ra2AiResponse.CreateSuccess("unused"));
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), client);
        Ra2AiInteractionRoute route = Ra2AiInteractionRouter.Resolve(
            "把当前文件 [E1] 下的 Strength 修改为 150",
            Ra2AiEditAvailabilityKind.Available);

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "把当前文件 [E1] 下的 Strength 修改为 150",
            CreateContext(),
            conversationContext: null,
            currentSubject: null,
            route,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentEditPreview, route.CapabilityMode);
        Assert.Single(result.Request.Tools);
    }

    private static Ra2AiContext CreateContext()
        => new(
            "rulesmd.ini",
            caretOffset: 16,
            lineNumber: 2,
            Ra2CaretRegion.Value,
            "HTNK",
            "Unit",
            "Strength",
            "400",
            selectedText: null,
            nearbyText: "[HTNK]\nStrength=400",
            nearbyLineCount: 2,
            hasSemanticContext: true);

    private static DeepSeekRa2AiClientOptions CreateDeepSeekOptions(string apiKey)
        => new()
        {
            BaseUrl = "https://deepseek.pipeline.test/v1/chat/completions",
            ApiKey = apiKey,
            Model = "deepseek-v4-flash",
            Timeout = TimeSpan.FromSeconds(5)
        };

    private static HttpResponseMessage CreateJsonResponse(string content)
    {
        string contentPayload = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    index = 0,
                    delta = new { content },
                    finish_reason = (string?)null
                }
            }
        });
        const string finishPayload = "{\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}";
        string source = $"data: {contentPayload}\n\ndata: {finishPayload}\n\ndata: [DONE]\n\n";
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(source, Encoding.UTF8, "text/event-stream")
        };
    }

    private sealed class RecordingAiClient : IRa2AiClient
    {
        private readonly Ra2AiResponse _response;
        private readonly IReadOnlyList<string> _streamingDeltas;

        public RecordingAiClient(
            Ra2AiResponse response,
            IReadOnlyList<string>? streamingDeltas = null)
        {
            _response = response;
            _streamingDeltas = streamingDeltas ?? [];
        }

        public Ra2AiRequest? LastRequest { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public int StreamingCallCount { get; private set; }

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastCancellationToken = cancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Ra2AiResponse.CreateCancelled());

            return Task.FromResult(_response);
        }

        public async Task<Ra2AiResponse> SendStreamingAsync(
            Ra2AiRequest request,
            Ra2AiContentDeltaHandler onContentDelta,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastCancellationToken = cancellationToken;
            StreamingCallCount++;
            if (cancellationToken.IsCancellationRequested)
                return Ra2AiResponse.CreateCancelled();

            foreach (string delta in _streamingDeltas)
                await onContentDelta(delta, cancellationToken);

            return _response;
        }
    }

    private sealed class RecordingHttpHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public RecordingHttpHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public int CallCount { get; private set; }

        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return _response;
        }
    }
}
