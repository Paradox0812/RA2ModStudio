using System.Reflection;
using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiClientTests
{
    [Fact]
    public async Task TestClient_ReturnsDeterministicSuccessResponse()
    {
        IRa2AiClient client = new DeterministicTestAiClient();
        Ra2AiRequest request = CreateRequest(Ra2AiIntent.ExplainField, "Explain Strength.", "Prompt text.");

        Ra2AiResponse first = await client.SendAsync(request, CancellationToken.None);
        Ra2AiResponse second = await client.SendAsync(request, CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, first.Kind);
        Assert.True(first.IsSuccess);
        Assert.Equal(first.Text, second.Text);
        Assert.Contains("测试 AI 回复", first.Text);
        Assert.Contains("已构建上下文和 prompt", first.Text);
        Assert.Contains("Intent: ExplainField", first.Text);
        Assert.Contains("User prompt length: 17", first.Text);
        Assert.Contains("Prompt length: 12", first.Text);
        Assert.Contains("不会连接 DeepSeek", first.Text);
        Assert.Contains("不会修改文件", first.Text);
        Assert.Null(first.ErrorMessage);
    }

    [Fact]
    public async Task TestClient_ConsumesRa2AiRequestWithoutRawPromptEcho()
    {
        IRa2AiClient client = new DeterministicTestAiClient();
        Ra2AiRequest request = CreateRequest(
            Ra2AiIntent.GenerateUnitPrototype,
            "SecretRawPrompt",
            "INI payload that should not be echoed");

        Ra2AiResponse response = await client.SendAsync(request, CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Contains("Intent: GenerateUnitPrototype", response.Text);
        Assert.DoesNotContain(request.UserPrompt, response.Text);
        Assert.DoesNotContain(request.PromptText, response.Text);
    }

    [Fact]
    public async Task TestClient_RespectsPreCancelledCancellationToken()
    {
        DeterministicTestAiClient client = new();
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), source.Token);

        Assert.Equal(Ra2AiResponseKind.Cancelled, response.Kind);
        Assert.False(response.IsSuccess);
        Assert.Empty(response.Text);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task TestClient_StreamingEmitsOneDeterministicDeltaAndReturnsSuccess()
    {
        IRa2AiClient client = new DeterministicTestAiClient();
        List<string> deltas = [];

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            (delta, _) =>
            {
                deltas.Add(delta);
                return ValueTask.CompletedTask;
            },
            CancellationToken.None);

        string delta = Assert.Single(deltas);
        Assert.Equal(response.Text, delta);
        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
    }

    [Fact]
    public async Task TestClient_StreamingPreCancelledDoesNotEmitDelta()
    {
        IRa2AiClient client = new DeterministicTestAiClient();
        using CancellationTokenSource source = new();
        source.Cancel();
        int callbackCount = 0;

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            (_, _) =>
            {
                callbackCount++;
                return ValueTask.CompletedTask;
            },
            source.Token);

        Assert.Equal(Ra2AiResponseKind.Cancelled, response.Kind);
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public async Task TestClient_CanReturnConfiguredProviderError()
    {
        DeterministicTestAiClient client = new(Ra2AiResponseKind.ProviderError);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.False(response.IsSuccess);
        Assert.Empty(response.Text);
        Assert.Equal("Test provider error.", response.ErrorMessage);
    }

    [Fact]
    public async Task TestClient_CanReturnConfiguredMissingConfigurationError()
    {
        DeterministicTestAiClient client = new(Ra2AiResponseKind.MissingConfiguration);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.False(response.IsSuccess);
        Assert.Empty(response.Text);
        Assert.Equal("DeepSeek configuration is missing or invalid.", response.ErrorMessage);
    }

    [Fact]
    public async Task TestClient_ErrorTextDoesNotExposeApiKeyOrRawRequestPayload()
    {
        DeterministicTestAiClient client = new(
            Ra2AiResponseKind.ProviderError,
            "Provider failed without secret payload.");
        Ra2AiRequest request = CreateRequest(
            userPrompt: "api_key=should-not-leak",
            promptText: "Raw request payload should not be exposed.");

        Ra2AiResponse response = await client.SendAsync(request, CancellationToken.None);

        Assert.DoesNotContain("api_key", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.UserPrompt, response.ErrorMessage);
        Assert.DoesNotContain(request.PromptText, response.ErrorMessage);
        Assert.DoesNotContain(request.UserPrompt, response.Text);
        Assert.DoesNotContain(request.PromptText, response.Text);
    }

    [Fact]
    public void Interface_SendAsyncAcceptsRa2AiRequestAndCancellationToken()
    {
        MethodInfo? method = typeof(IRa2AiClient).GetMethod(nameof(IRa2AiClient.SendAsync));

        Assert.NotNull(method);
        ParameterInfo[] parameters = method.GetParameters();
        Assert.Equal(typeof(Task<Ra2AiResponse>), method.ReturnType);
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(Ra2AiRequest), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
    }

    [Fact]
    public void Interface_SendStreamingAsyncUsesBoundedInternalCallbackContract()
    {
        MethodInfo? method = typeof(IRa2AiClient).GetMethod(nameof(IRa2AiClient.SendStreamingAsync));

        Assert.NotNull(method);
        ParameterInfo[] parameters = method.GetParameters();
        Assert.Equal(typeof(Task<Ra2AiResponse>), method.ReturnType);
        Assert.Equal(3, parameters.Length);
        Assert.Equal(typeof(Ra2AiRequest), parameters[0].ParameterType);
        Assert.Equal(typeof(Ra2AiContentDeltaHandler), parameters[1].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[2].ParameterType);
    }

    [Fact]
    public void Response_IncompletePreservesPartialTextAndFinishReason()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateIncomplete(
            "partial",
            Ra2AiStreamFinishKind.Length,
            safeErrorMessage: "stream incomplete");

        Assert.False(response.IsSuccess);
        Assert.Equal("partial", response.Text);
        Assert.Equal("stream incomplete", response.ErrorMessage);
        Assert.Equal(Ra2AiStreamFinishKind.Length, response.FinishKind);
    }

    private sealed class DeterministicTestAiClient : IRa2AiClient
    {
        private const string DefaultProviderErrorMessage = "Test provider error.";
        private readonly Ra2AiResponseKind _responseKind;
        private readonly string? _errorMessage;

        public DeterministicTestAiClient(
            Ra2AiResponseKind responseKind = Ra2AiResponseKind.Success,
            string? errorMessage = null)
        {
            _responseKind = responseKind;
            _errorMessage = errorMessage;
        }

        public Task<Ra2AiResponse> SendAsync(
            Ra2AiRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Ra2AiResponse.CreateCancelled());

            return Task.FromResult(_responseKind switch
            {
                Ra2AiResponseKind.Success => Ra2AiResponse.CreateSuccess(BuildSuccessText(request)),
                Ra2AiResponseKind.Cancelled => Ra2AiResponse.CreateCancelled(),
                Ra2AiResponseKind.ProviderError => Ra2AiResponse.CreateProviderFailure(
                    Ra2AiFailureKind.ProtocolError,
                    SanitizeErrorMessage(_errorMessage, DefaultProviderErrorMessage)),
                Ra2AiResponseKind.MissingConfiguration => Ra2AiResponse.CreateMissingConfiguration(),
                _ => Ra2AiResponse.CreateProviderFailure(
                    Ra2AiFailureKind.ProtocolError,
                    DefaultProviderErrorMessage)
            });
        }

        private static string BuildSuccessText(Ra2AiRequest request)
            => $"测试 AI 回复：已构建上下文和 prompt。Intent: {request.Intent}；User prompt length: {request.UserPrompt.Length}；Prompt length: {request.PromptText.Length}。不会连接 DeepSeek，也不会修改文件。";

        private static string SanitizeErrorMessage(string? errorMessage, string fallback)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                return fallback;

            return ContainsSensitiveToken(errorMessage) ? fallback : errorMessage;
        }

        private static bool ContainsSensitiveToken(string text)
            => text.Contains("api_key", StringComparison.OrdinalIgnoreCase)
                || text.Contains("apikey", StringComparison.OrdinalIgnoreCase)
                || text.Contains("api key", StringComparison.OrdinalIgnoreCase)
                || text.Contains("secret", StringComparison.OrdinalIgnoreCase)
                || text.Contains("token", StringComparison.OrdinalIgnoreCase);
    }

    private static Ra2AiRequest CreateRequest(
        Ra2AiIntent intent = Ra2AiIntent.Auto,
        string userPrompt = "Explain this.",
        string promptText = "Built prompt.")
        => new(intent, userPrompt, promptText);
}
