using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class DeepSeekRa2AiClientTests
{
    private const string Endpoint = "https://deepseek.test/v1/chat/completions";
    private const string ApiKey = "test-api-key-placeholder";

    [Fact]
    public async Task SendAsync_SendsConfiguredEndpointModelPromptAndAuthorizationHeader()
    {
        RecordingHandler handler = new(CreateSseResponse("assistant response"));
        DeepSeekRa2AiClient client = CreateClient(handler);
        Ra2AiRequest request = CreateRequest(promptText: "PromptBuilder output.");

        Ra2AiResponse response = await client.SendAsync(request, CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal("assistant response", response.Text);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal(Endpoint, handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal(ApiKey, handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Contains(handler.LastRequest.Headers.Accept, item => item.MediaType == "text/event-stream");

        using JsonDocument document = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("deepseek-v4-flash", document.RootElement.GetProperty("model").GetString());
        JsonElement message = document.RootElement.GetProperty("messages")[0];
        Assert.Equal("user", message.GetProperty("role").GetString());
        Assert.Equal("PromptBuilder output.", message.GetProperty("content").GetString());
        Assert.Equal(0.2, document.RootElement.GetProperty("temperature").GetDouble(), precision: 3);
        Assert.Equal(
            "disabled",
            document.RootElement.GetProperty("thinking").GetProperty("type").GetString());
        Assert.Equal(DeepSeekRa2AiClientOptions.DefaultMaxOutputTokens,
            document.RootElement.GetProperty("max_tokens").GetInt32());
        Assert.True(document.RootElement.GetProperty("stream").GetBoolean());
        Assert.False(document.RootElement.TryGetProperty("tools", out _));
        Assert.False(document.RootElement.TryGetProperty("tool_choice", out _));

        Ra2AiRequestDiagnostics diagnostics = Assert.IsType<Ra2AiRequestDiagnostics>(response.Diagnostics);
        Assert.True(Guid.TryParseExact(diagnostics.RequestId, "N", out _));
        Assert.Equal("deepseek-v4-flash", diagnostics.ModelId);
        Assert.Equal(request.PromptText.Length, diagnostics.PromptCharacterCount);
        Assert.Equal((int)HttpStatusCode.OK, diagnostics.HttpStatusCode);
        Assert.NotNull(diagnostics.TimeToHeaders);
        Assert.NotNull(diagnostics.TimeToFirstContent);
        Assert.True(diagnostics.TotalDuration >= diagnostics.TimeToHeaders!.Value);
        Assert.Equal(1, diagnostics.ContentDeltaCount);
        Assert.Equal("assistant response".Length, diagnostics.ContentCharacterCount);
        Assert.DoesNotContain(ApiKey, diagnostics.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(request.PromptText, diagnostics.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_BaseUrlWithoutChatCompletionsPathAppendsChatCompletionsEndpoint()
    {
        RecordingHandler handler = new(CreateSseResponse("assistant response"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = "https://api.deepseek.com",
            ApiKey = ApiKey,
            Model = "deepseek-v4-pro",
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal("https://api.deepseek.com/chat/completions", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task SendAsync_FullChatCompletionsEndpointIsNotAppendedTwice()
    {
        RecordingHandler handler = new(CreateSseResponse("assistant response"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = Endpoint,
            ApiKey = ApiKey,
            Model = "deepseek-v4-pro",
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal(Endpoint, handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task SendAsync_ParsesChoicesZeroMessageContent()
    {
        RecordingHandler handler = new(CreateSseResponse("parsed assistant content"));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal("parsed assistant content", response.Text);
    }

    [Fact]
    public async Task SendAsync_UsesPromptTextWithoutRawUserPromptRouting()
    {
        RecordingHandler handler = new(CreateSseResponse("assistant response"));
        DeepSeekRa2AiClient client = CreateClient(handler);
        Ra2AiRequest request = CreateRequest(
            userPrompt: "Raw user prompt should not be used as provider content.",
            promptText: "Bounded PromptBuilder request.");

        await client.SendAsync(request, CancellationToken.None);

        Assert.Contains("Bounded PromptBuilder request.", handler.LastRequestBody);
        Assert.DoesNotContain(request.UserPrompt, handler.LastRequestBody);
    }

    [Fact]
    public async Task SendAsync_MissingApiKeyReturnsMissingConfigurationWithoutNetwork()
    {
        RecordingHandler handler = new(CreateSseResponse("unused"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = Endpoint,
            ApiKey = "",
            Model = "deepseek-v4-flash",
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.Equal(Ra2AiFailureKind.MissingConfiguration, response.FailureKind);
        Assert.False(response.IsSuccess);
        Assert.Equal(0, handler.CallCount);
        Assert.DoesNotContain(ApiKey, response.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_InvalidBaseUrlReturnsMissingConfigurationWithoutNetwork()
    {
        RecordingHandler handler = new(CreateSseResponse("unused"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = "not-a-url",
            ApiKey = ApiKey,
            Model = "deepseek-v4-flash",
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.Equal(Ra2AiFailureKind.MissingConfiguration, response.FailureKind);
        Assert.Equal(0, handler.CallCount);
        Assert.DoesNotContain(ApiKey, response.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_HttpNonSuccessMapsProviderErrorWithoutKeyOrPrompt()
    {
        RecordingHandler handler = new(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent($"provider body containing {ApiKey} and raw prompt")
        });
        DeepSeekRa2AiClient client = CreateClient(handler);
        Ra2AiRequest request = CreateRequest(promptText: "raw prompt should not appear");

        Ra2AiResponse response = await client.SendAsync(request, CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ServiceUnavailable, response.FailureKind);
        Assert.Contains("HTTP 500", response.ErrorMessage);
        Assert.DoesNotContain(ApiKey, response.ErrorMessage);
        Assert.DoesNotContain(request.PromptText, response.ErrorMessage);
    }

    [Theory]
    [InlineData(401, (int)Ra2AiFailureKind.AuthenticationOrAuthorization)]
    [InlineData(403, (int)Ra2AiFailureKind.AuthenticationOrAuthorization)]
    [InlineData(408, (int)Ra2AiFailureKind.ProviderRequestTimeout)]
    [InlineData(429, (int)Ra2AiFailureKind.RateLimited)]
    [InlineData(400, (int)Ra2AiFailureKind.RequestRejected)]
    [InlineData(500, (int)Ra2AiFailureKind.ServiceUnavailable)]
    [InlineData(504, (int)Ra2AiFailureKind.ProviderRequestTimeout)]
    public async Task SendAsync_HttpStatusMapsStableFailureKind(
        int statusCode,
        int expectedFailureKind)
    {
        RecordingHandler handler = new(new HttpResponseMessage((HttpStatusCode)statusCode));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal((Ra2AiFailureKind)expectedFailureKind, response.FailureKind);
        Assert.Equal($"DeepSeek provider returned HTTP {statusCode}.", response.ErrorMessage);
        Ra2AiRequestDiagnostics diagnostics = Assert.IsType<Ra2AiRequestDiagnostics>(response.Diagnostics);
        Assert.Equal(statusCode, diagnostics.HttpStatusCode);
        Assert.Null(diagnostics.TimeToFirstContent);
        Assert.Equal(0, diagnostics.ContentDeltaCount);
        Assert.Equal(0, diagnostics.ContentCharacterCount);
    }

    [Fact]
    public async Task SendAsync_HttpNonSuccessDoesNotReadBodyAndDisposesResponseContent()
    {
        UnreadableTrackingContent content = new();
        RecordingHandler handler = new(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = content
        });
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiFailureKind.RequestRejected, response.FailureKind);
        Assert.True(content.WasDisposed);
    }

    [Fact]
    public async Task SendAsync_HttpRequestExceptionWithStatusUsesHttpMappingWithoutRawMessage()
    {
        RecordingHandler handler = new(_ => throw new HttpRequestException(
            $"sensitive {ApiKey} raw prompt",
            inner: null,
            statusCode: HttpStatusCode.TooManyRequests));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal(Ra2AiFailureKind.RateLimited, response.FailureKind);
        Assert.Equal("DeepSeek provider returned HTTP 429.", response.ErrorMessage);
        Assert.DoesNotContain(ApiKey, response.ErrorMessage);
        Assert.DoesNotContain("raw prompt", response.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_HttpRequestExceptionWithoutStatusMapsNetworkWithoutRawMessage()
    {
        RecordingHandler handler = new(_ => throw new HttpRequestException(
            $"sensitive {ApiKey} proxy detail"));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal(Ra2AiFailureKind.NetworkOrProxy, response.FailureKind);
        Assert.Equal("DeepSeek provider request failed.", response.ErrorMessage);
        Assert.DoesNotContain(ApiKey, response.ErrorMessage);
        Assert.DoesNotContain("proxy detail", response.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_MalformedJsonMapsProviderError()
    {
        RecordingHandler handler = new(CreateRawSseResponse("data: { malformed\n\n"));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ProtocolError, response.FailureKind);
        Assert.Equal("DeepSeek provider returned an invalid response.", response.ErrorMessage);
        Assert.DoesNotContain(ApiKey, response.ErrorMessage);
    }

    [Fact]
    public async Task SendStreamingAsync_WrongContentTypeMapsProtocolError()
    {
        RecordingHandler handler = new(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ProtocolError, response.FailureKind);
    }

    [Fact]
    public async Task SendStreamingAsync_MissingContentTypePreservesExistingSseCompatibility()
    {
        StringContent content = new(CreateSseText("content"), Encoding.UTF8);
        content.Headers.ContentType = null;
        RecordingHandler handler = new(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content
        });
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.Equal("content", response.Text);
    }

    [Fact]
    public async Task SendAsync_MissingContentMapsProviderError()
    {
        RecordingHandler handler = new(CreateRawSseResponse("""
            data: {"choices":[{"index":0,"delta":{},"finish_reason":"stop"}]}

            data: [DONE]

            """));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ProtocolError, response.FailureKind);
        Assert.Equal("DeepSeek provider response did not include assistant content.", response.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_PreCancelledTokenMapsCancelledWithoutNetwork()
    {
        RecordingHandler handler = new(CreateSseResponse("unused"));
        DeepSeekRa2AiClient client = CreateClient(handler);
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), source.Token);

        Assert.Equal(Ra2AiResponseKind.Cancelled, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_OperationCancelledMapsCancelled()
    {
        using CancellationTokenSource source = new();
        RecordingHandler handler = new(async token =>
        {
            source.Cancel();
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return CreateSseResponse("unused");
        });
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), source.Token);

        Assert.Equal(Ra2AiResponseKind.Cancelled, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
    }

    [Fact]
    public async Task SendAsync_TimeoutMapsTimeoutWithoutKeyOrPrompt()
    {
        RecordingHandler handler = new(async token =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return CreateSseResponse("unused");
        });
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = Endpoint,
            ApiKey = ApiKey,
            Model = "deepseek-v4-flash",
            Timeout = TimeSpan.FromMilliseconds(10)
        });
        Ra2AiRequest request = CreateRequest(promptText: "raw prompt should not appear");

        Ra2AiResponse response = await client.SendAsync(request, CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Timeout, response.Kind);
        Assert.Equal(Ra2AiFailureKind.TotalTimeout, response.FailureKind);
        Assert.Equal("DeepSeek provider request timed out.", response.ErrorMessage);
        Assert.DoesNotContain(ApiKey, response.ErrorMessage);
        Assert.DoesNotContain(request.PromptText, response.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_LateUserCancellationDoesNotOverrideEarlierTotalTimeout()
    {
        using CancellationTokenSource userCancellationSource = new();
        RecordingHandler handler = new(async token =>
        {
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return CreateSseResponse("unused");
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                userCancellationSource.Cancel();
                throw;
            }
        });
        DeepSeekRa2AiClient client = CreateClient(
            handler,
            CreateOptions(timeout: TimeSpan.FromMilliseconds(20)));

        Ra2AiResponse response = await client.SendAsync(
            CreateRequest(),
            userCancellationSource.Token);

        Assert.Equal(Ra2AiResponseKind.Timeout, response.Kind);
        Assert.Equal(Ra2AiFailureKind.TotalTimeout, response.FailureKind);
    }

    [Fact]
    public void Options_ToStringDoesNotExposeApiKey()
    {
        DeepSeekRa2AiClientOptions options = CreateOptions();

        string text = options.ToString();

        Assert.DoesNotContain(ApiKey, text);
        Assert.Contains("ApiKey=***", text);
        Assert.Contains("BaseUrl=***", text);
        Assert.DoesNotContain(Endpoint, text);
    }

    [Fact]
    public void Options_DefaultTimeoutMatchesFactoryDefault()
    {
        DeepSeekRa2AiClientOptions options = new();

        Assert.Equal(120, DeepSeekRa2AiClientOptions.DefaultTimeoutSeconds);
        Assert.Equal(DeepSeekRa2AiClientOptions.DefaultTimeoutSeconds, DeepSeekRa2AiClientFactory.DefaultTimeoutSeconds);
        Assert.Equal(TimeSpan.FromSeconds(DeepSeekRa2AiClientOptions.DefaultTimeoutSeconds), options.Timeout);
        Assert.Equal(
            TimeSpan.FromSeconds(DeepSeekRa2AiClientOptions.DefaultStreamingIdleTimeoutSeconds),
            options.StreamingIdleTimeout);
        Assert.Equal(
            DeepSeekRa2AiClientOptions.DefaultMaxStreamingResponseCharacters,
            options.MaxStreamingResponseCharacters);
        Assert.Equal(8192, options.MaxOutputTokens);
    }

    [Theory]
    [InlineData("http://deepseek.test/v1")]
    [InlineData("ftp://deepseek.test/v1")]
    [InlineData("https://user:password@deepseek.test/v1")]
    [InlineData("https://deepseek.test/v1?token=value")]
    [InlineData("https://deepseek.test/v1#fragment")]
    public async Task SendAsync_UntrustedEndpointReturnsMissingConfigurationWithoutNetwork(string baseUrl)
    {
        RecordingHandler handler = new(CreateSseResponse("unused"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = baseUrl,
            ApiKey = ApiKey,
            Model = "deepseek-v4-flash",
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("http://localhost:11434/v1")]
    [InlineData("http://127.0.0.1:11434/v1")]
    [InlineData("http://[::1]:11434/v1")]
    public async Task SendAsync_LoopbackHttpEndpointIsAllowed(string baseUrl)
    {
        RecordingHandler handler = new(CreateSseResponse("assistant response"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = baseUrl,
            ApiKey = ApiKey,
            Model = "deepseek-v4-pro",
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("deepseek-chat")]
    [InlineData("deepseek-reasoner")]
    [InlineData("deepseek-test")]
    [InlineData("")]
    public async Task SendAsync_UnsupportedModelReturnsMissingConfigurationWithoutNetwork(string model)
    {
        RecordingHandler handler = new(CreateSseResponse("unused"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = Endpoint,
            ApiKey = ApiKey,
            Model = model,
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(2.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task SendAsync_InvalidTemperatureReturnsMissingConfigurationWithoutNetwork(
        double temperature)
    {
        RecordingHandler handler = new(CreateSseResponse("unused"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = Endpoint,
            ApiKey = ApiKey,
            Model = "deepseek-v4-flash",
            Temperature = temperature,
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(32769)]
    public async Task SendAsync_InvalidMaxOutputTokensReturnsMissingConfigurationWithoutNetwork(
        int maxOutputTokens)
    {
        RecordingHandler handler = new(CreateSseResponse("unused"));
        DeepSeekRa2AiClient client = CreateClient(handler, new DeepSeekRa2AiClientOptions
        {
            BaseUrl = Endpoint,
            ApiKey = ApiKey,
            Model = "deepseek-v4-flash",
            MaxOutputTokens = maxOutputTokens,
            Timeout = TimeSpan.FromSeconds(5)
        });

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task SendStreamingAsync_EmitsOrderedDeltasAndReturnsAggregatedSuccess()
    {
        const string source = """
            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{"content":"Hello"},"finish_reason":null}]}

            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{"content":" world"},"finish_reason":null}]}

            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{},"finish_reason":"stop"}]}

            data: [DONE]

            """;
        RecordingHandler handler = new(CreateRawSseResponse(source));
        DeepSeekRa2AiClient client = CreateClient(handler);
        List<string> deltas = [];

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            (delta, _) =>
            {
                deltas.Add(delta);
                return ValueTask.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(["Hello", " world"], deltas);
        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.Equal(Ra2AiStreamFinishKind.Stop, response.FinishKind);
        Assert.Equal("Hello world", response.Text);
        Ra2AiRequestDiagnostics diagnostics = Assert.IsType<Ra2AiRequestDiagnostics>(response.Diagnostics);
        Assert.Equal(2, diagnostics.ContentDeltaCount);
        Assert.Equal("Hello world".Length, diagnostics.ContentCharacterCount);
    }

    [Fact]
    public async Task SendStreamingAsync_ToolRequestSerializesSchemaAndReturnsCompleteCall()
    {
        const string source = """
            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{"tool_calls":[{"index":0,"id":"call-","type":"function","function":{"name":"preview_ini_","arguments":"{\"summary\":"}}]},"finish_reason":null}]}

            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{"tool_calls":[{"index":0,"id":"1","function":{"name":"edit_plan","arguments":"\"x\",\"operations\":[]}"}}]},"finish_reason":"tool_calls"}]}

            data: [DONE]

            """;
        RecordingHandler handler = new(CreateRawSseResponse(source));
        DeepSeekRa2AiClient client = CreateClient(handler);
        Ra2AiToolDefinition tool = new(
            "preview_ini_edit_plan",
            "Create a preview.",
            """{"type":"object","properties":{"summary":{"type":"string"}}}""");
        Ra2AiRequest request = new(
            Ra2AiIntent.Auto,
            "user",
            "prompt",
            tools: [tool],
            toolChoice: Ra2AiToolChoiceMode.Auto);

        Ra2AiResponse response = await client.SendStreamingAsync(
            request,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ToolCalls, response.Kind);
        Ra2AiToolCall call = Assert.Single(response.ToolCalls);
        Assert.Equal("call-1", call.Id);
        Assert.Equal("preview_ini_edit_plan", call.Name);
        Assert.Equal("""{"summary":"x","operations":[]}""", call.ArgumentsJson);

        using JsonDocument document = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("auto", document.RootElement.GetProperty("tool_choice").GetString());
        JsonElement function = document.RootElement.GetProperty("tools")[0].GetProperty("function");
        Assert.Equal("preview_ini_edit_plan", function.GetProperty("name").GetString());
        Assert.Equal(
            "object",
            function.GetProperty("parameters").GetProperty("type").GetString());
    }

    [Fact]
    public async Task SendStreamingAsync_RequiredToolRequestSerializesRequiredChoice()
    {
        RecordingHandler handler = new(CreateSseResponse("plain response"));
        DeepSeekRa2AiClient client = CreateClient(handler);
        Ra2AiToolDefinition tool = new(
            "preview_ini_edit_plan",
            "Create a preview.",
            """{"type":"object","properties":{"outcome":{"type":"string"}}}""");
        Ra2AiRequest request = new(
            Ra2AiIntent.Auto,
            "user",
            "prompt",
            tools: [tool],
            toolChoice: Ra2AiToolChoiceMode.Required,
            systemPromptText: "trusted rules",
            userContentText: "untrusted request and context");

        await client.SendStreamingAsync(
            request,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        using JsonDocument document = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("required", document.RootElement.GetProperty("tool_choice").GetString());
        JsonElement messages = document.RootElement.GetProperty("messages");
        Assert.Equal(2, messages.GetArrayLength());
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("trusted rules", messages[0].GetProperty("content").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("untrusted request and context", messages[1].GetProperty("content").GetString());
    }

    [Fact]
    public async Task SendStreamingAsync_InvalidArgumentJsonRemainsAuthoringConcern()
    {
        const string source = """
            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{"tool_calls":[{"index":0,"id":"call-1","type":"function","function":{"name":"preview_ini_edit_plan","arguments":"{invalid"}}]},"finish_reason":"tool_calls"}]}

            data: [DONE]

            """;
        DeepSeekRa2AiClient client = CreateClient(new RecordingHandler(CreateRawSseResponse(source)));

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ToolCalls, response.Kind);
        Assert.Equal("{invalid", Assert.Single(response.ToolCalls).ArgumentsJson);
    }

    [Fact]
    public async Task SendStreamingAsync_ToolFinishWithoutCompleteCallIsProtocolFailure()
    {
        RecordingHandler handler = new(CreateSseResponse("partial", "tool_calls"));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Incomplete, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ProtocolError, response.FailureKind);
        Assert.Equal("partial", response.Text);
    }

    [Fact]
    public async Task SendStreamingAsync_TotalDurationIncludesOrderedCallbackBackpressure()
    {
        RecordingHandler handler = new(CreateSseResponse("content"));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            async (_, _) => await Task.Delay(30),
            CancellationToken.None);

        Ra2AiRequestDiagnostics diagnostics = Assert.IsType<Ra2AiRequestDiagnostics>(response.Diagnostics);
        Assert.True(diagnostics.TotalDuration >= TimeSpan.FromMilliseconds(20));
        Assert.NotNull(diagnostics.TimeToFirstContent);
        Assert.True(diagnostics.TotalDuration >= diagnostics.TimeToFirstContent!.Value);
    }

    [Theory]
    [InlineData("length", (int)Ra2AiStreamFinishKind.Length)]
    [InlineData("content_filter", (int)Ra2AiStreamFinishKind.ContentFilter)]
    [InlineData("insufficient_system_resource", (int)Ra2AiStreamFinishKind.InsufficientSystemResource)]
    [InlineData("future_reason", (int)Ra2AiStreamFinishKind.Unknown)]
    public async Task SendStreamingAsync_NonStopFinishReasonReturnsIncomplete(
        string finishReason,
        int expectedFinishKind)
    {
        RecordingHandler handler = new(CreateSseResponse("partial", finishReason));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Incomplete, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.Equal("partial", response.Text);
        Assert.Equal((Ra2AiStreamFinishKind)expectedFinishKind, response.FinishKind);
    }

    [Fact]
    public async Task SendAsync_IncompleteStreamMapsLegacyCallToProviderError()
    {
        RecordingHandler handler = new(CreateSseResponse("partial", "length"));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ProtocolError, response.FailureKind);
        Assert.Empty(response.Text);
        Assert.Equal(Ra2AiStreamFinishKind.Length, response.FinishKind);
    }

    [Fact]
    public async Task SendStreamingAsync_EndOfStreamAfterContentReturnsIncomplete()
    {
        RecordingHandler handler = new(CreateRawSseResponse("""
            data: {"choices":[{"index":0,"delta":{"content":"partial"}}]}

            """));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Incomplete, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ProtocolError, response.FailureKind);
        Assert.Equal("partial", response.Text);
    }

    [Fact]
    public async Task SendStreamingAsync_IoFailureAfterContentMapsNetworkAndDisposesStream()
    {
        const string prefix = "data: {\"choices\":[{\"index\":0,\"delta\":{\"content\":\"A\"}}]}\n\n";
        ThrowingAfterPrefixStream stream = new(Encoding.UTF8.GetBytes(prefix));
        RecordingHandler handler = new(CreateStreamResponse(stream));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Incomplete, response.Kind);
        Assert.Equal(Ra2AiFailureKind.NetworkOrProxy, response.FailureKind);
        Assert.Equal("A", response.Text);
        Assert.True(stream.WasDisposed);
    }

    [Fact]
    public async Task SendStreamingAsync_ResponseLimitPreservesAcceptedPartialText()
    {
        const string source = """
            data: {"choices":[{"index":0,"delta":{"content":"A"}}]}

            data: {"choices":[{"index":0,"delta":{"content":"B"}}]}

            data: [DONE]

            """;
        RecordingHandler handler = new(CreateRawSseResponse(source));
        DeepSeekRa2AiClient client = CreateClient(
            handler,
            CreateOptions(maxStreamingResponseCharacters: 1));

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Incomplete, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ResponseTooLarge, response.FailureKind);
        Assert.Equal("A", response.Text);
        Assert.Equal("DeepSeek provider response exceeded the allowed size.", response.ErrorMessage);
    }

    [Fact]
    public async Task SendStreamingAsync_FragmentedUtf8IsDecodedAndResponseStreamIsDisposed()
    {
        string source = CreateSseText("中文响应");
        TrackingChunkedStream stream = new(Encoding.UTF8.GetBytes(source), maxChunkSize: 1);
        RecordingHandler handler = new(CreateStreamResponse(stream));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.Equal("中文响应", response.Text);
        Assert.True(stream.WasDisposed);
    }

    [Fact]
    public async Task SendStreamingAsync_IdleAfterFirstDeltaMapsTimeoutWithPartialText()
    {
        const string prefix = "data: {\"choices\":[{\"index\":0,\"delta\":{\"content\":\"A\"}}]}\n\n";
        BlockingAfterPrefixStream stream = new(Encoding.UTF8.GetBytes(prefix));
        RecordingHandler handler = new(CreateStreamResponse(stream));
        DeepSeekRa2AiClient client = CreateClient(
            handler,
            CreateOptions(streamingIdleTimeout: TimeSpan.FromMilliseconds(20)));

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Timeout, response.Kind);
        Assert.Equal(Ra2AiFailureKind.StreamingIdleTimeout, response.FailureKind);
        Assert.Equal("A", response.Text);
        Assert.True(stream.WasDisposed);
    }

    [Fact]
    public async Task SendStreamingAsync_UserCancellationAfterDeltaReturnsCancelledWithPartialText()
    {
        RecordingHandler handler = new(CreateSseResponse("partial"));
        DeepSeekRa2AiClient client = CreateClient(handler);
        using CancellationTokenSource source = new();

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            (_, _) =>
            {
                source.Cancel();
                return ValueTask.CompletedTask;
            },
            source.Token);

        Assert.Equal(Ra2AiResponseKind.Cancelled, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.Equal("partial", response.Text);
    }

    [Fact]
    public async Task SendStreamingAsync_ConsumerFailurePropagatesAndDisposesResponseStream()
    {
        TrackingChunkedStream stream = new(Encoding.UTF8.GetBytes(CreateSseText("content")), maxChunkSize: 8);
        RecordingHandler handler = new(CreateStreamResponse(stream));
        DeepSeekRa2AiClient client = CreateClient(handler);

        Exception exception = await Assert.ThrowsAnyAsync<Exception>(() => client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => throw new InvalidOperationException("consumer failed"),
            CancellationToken.None));

        Assert.Equal("Ra2AiStreamConsumerException", exception.GetType().Name);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.True(stream.WasDisposed);
    }

    private static DeepSeekRa2AiClient CreateClient(
        RecordingHandler handler,
        DeepSeekRa2AiClientOptions? options = null)
        => new(options ?? CreateOptions(), new HttpClient(handler));

    private static DeepSeekRa2AiClientOptions CreateOptions(
        TimeSpan? streamingIdleTimeout = null,
        int? maxStreamingResponseCharacters = null,
        TimeSpan? timeout = null)
        => new()
        {
            BaseUrl = Endpoint,
            ApiKey = ApiKey,
            Model = "deepseek-v4-flash",
            Timeout = timeout ?? TimeSpan.FromSeconds(5),
            StreamingIdleTimeout = streamingIdleTimeout
                ?? TimeSpan.FromSeconds(DeepSeekRa2AiClientOptions.DefaultStreamingIdleTimeoutSeconds),
            MaxStreamingResponseCharacters = maxStreamingResponseCharacters
                ?? DeepSeekRa2AiClientOptions.DefaultMaxStreamingResponseCharacters
        };

    private static Ra2AiRequest CreateRequest(
        string userPrompt = "Explain current field.",
        string promptText = "Built PromptBuilder prompt.")
        => new(Ra2AiIntent.Auto, userPrompt, promptText);

    private static HttpResponseMessage CreateSseResponse(
        string content,
        string finishReason = "stop")
        => CreateRawSseResponse(CreateSseText(content, finishReason));

    private static string CreateSseText(
        string content,
        string finishReason = "stop")
    {
        string contentPayload = JsonSerializer.Serialize(new
        {
            id = "stream-test",
            @object = "chat.completion.chunk",
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
        string finishPayload = JsonSerializer.Serialize(new
        {
            id = "stream-test",
            @object = "chat.completion.chunk",
            choices = new[]
            {
                new
                {
                    index = 0,
                    delta = new { },
                    finish_reason = finishReason
                }
            }
        });
        return $"data: {contentPayload}\n\ndata: {finishPayload}\n\ndata: [DONE]\n\n";
    }

    private static HttpResponseMessage CreateRawSseResponse(string source)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(source, Encoding.UTF8, "text/event-stream")
        };

    private static HttpResponseMessage CreateStreamResponse(Stream stream)
    {
        StreamContent content = new(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<CancellationToken, Task<HttpResponseMessage>> _responseFactory;

        public RecordingHandler(HttpResponseMessage response)
            : this(_ => Task.FromResult(response))
        {
        }

        public RecordingHandler(Func<CancellationToken, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public int CallCount { get; private set; }

        public HttpRequestMessage? LastRequest { get; private set; }

        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return await _responseFactory(cancellationToken);
        }
    }

    private sealed class TrackingChunkedStream(byte[] bytes, int maxChunkSize) : MemoryStream(bytes)
    {
        public bool WasDisposed { get; private set; }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
            => base.ReadAsync(buffer[..Math.Min(buffer.Length, maxChunkSize)], cancellationToken);

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class ThrowingAfterPrefixStream(byte[] prefix) : MemoryStream(prefix)
    {
        public bool WasDisposed { get; private set; }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (Position < Length)
                return base.ReadAsync(buffer, cancellationToken);

            throw new IOException("sensitive transport detail");
        }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class UnreadableTrackingContent : HttpContent
    {
        public bool WasDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => throw new InvalidOperationException("Provider error body must not be read.");

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class BlockingAfterPrefixStream(byte[] prefix) : MemoryStream(prefix)
    {
        public bool WasDisposed { get; private set; }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (Position < Length)
                return base.ReadAsync(buffer, cancellationToken);

            return WaitForCancellationAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }

        private static async ValueTask<int> WaitForCancellationAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }
    }
}
