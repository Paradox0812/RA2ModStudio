using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RA2IniEditor.IDE.AI;

internal sealed class DeepSeekRa2AiClient : IRa2AiClient
{
    private const string ProviderErrorMessage = "DeepSeek provider request failed.";
    private const string MalformedResponseMessage = "DeepSeek provider returned an invalid response.";
    private const string MissingContentMessage = "DeepSeek provider response did not include assistant content.";
    private const string IncompleteResponseMessage = "DeepSeek provider stream did not complete successfully.";
    private const string OversizedResponseMessage = "DeepSeek provider response exceeded the allowed size.";
    private const string EventStreamMediaType = "text/event-stream";
    private const int MaximumToolCallCount = 16;

    private static readonly Ra2AiContentDeltaHandler IgnoreContentDelta
        = static (_, _) => ValueTask.CompletedTask;

    private readonly DeepSeekRa2AiClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly DeepSeekRa2AiSseParser _sseParser = new();

    public DeepSeekRa2AiClient(DeepSeekRa2AiClientOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
    {
        Ra2AiResponse response = await SendStreamingAsync(
            request,
            IgnoreContentDelta,
            cancellationToken).ConfigureAwait(false);

        return response.Kind == Ra2AiResponseKind.Incomplete
            ? Ra2AiResponse.CreateProviderFailure(
                response.FailureKind == Ra2AiFailureKind.None
                    ? Ra2AiFailureKind.ProtocolError
                    : response.FailureKind,
                response.ErrorMessage ?? IncompleteResponseMessage,
                finishKind: response.FinishKind,
                diagnostics: response.Diagnostics)
            : response;
    }

    public async Task<Ra2AiResponse> SendStreamingAsync(
        Ra2AiRequest request,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(onContentDelta);

        RequestDiagnosticsTracker? diagnosticsTracker =
            DeepSeekRa2AiClientOptions.IsSupportedModelId(_options.Model)
                ? new RequestDiagnosticsTracker(_options.Model.Trim(), request.PromptCharacterCount)
                : null;
        Ra2AiResponse response = await SendStreamingCoreAsync(
            request,
            onContentDelta,
            diagnosticsTracker,
            cancellationToken).ConfigureAwait(false);
        return diagnosticsTracker is null
            ? response
            : response.WithDiagnostics(diagnosticsTracker.Complete());
    }

    private async Task<Ra2AiResponse> SendStreamingCoreAsync(
        Ra2AiRequest request,
        Ra2AiContentDeltaHandler onContentDelta,
        RequestDiagnosticsTracker? diagnosticsTracker,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Ra2AiResponse.CreateCancelled();

        if (!_options.TryValidate(out Uri? endpoint, out _))
            return Ra2AiResponse.CreateMissingConfiguration();

        StringBuilder accumulatedText = new();
        Dictionary<int, ToolCallAccumulator> toolCallAccumulators = [];
        using CancellationTokenSource requestSource = new();
        using CancellationTokenSource totalTimeoutSource = new(_options.Timeout);
        using CancellationTokenSource idleTimeoutSource = new();
        int firstTerminationCause = (int)RequestTerminationCause.None;
        using CancellationTokenRegistration userCancellationRegistration = cancellationToken.Register(
            () => TryTerminateRequest(
                ref firstTerminationCause,
                RequestTerminationCause.UserCancellation,
                requestSource));
        using CancellationTokenRegistration totalTimeoutRegistration = totalTimeoutSource.Token.Register(
            () => TryTerminateRequest(
                ref firstTerminationCause,
                RequestTerminationCause.TotalTimeout,
                requestSource));
        using CancellationTokenRegistration idleTimeoutRegistration = idleTimeoutSource.Token.Register(
            () => TryTerminateRequest(
                ref firstTerminationCause,
                RequestTerminationCause.StreamingIdleTimeout,
                requestSource));
        try
        {
            using HttpRequestMessage message = CreateRequestMessage(endpoint!, request);
            diagnosticsTracker?.MarkSending();
            using HttpResponseMessage response = await _httpClient.SendAsync(
                message,
                HttpCompletionOption.ResponseHeadersRead,
                requestSource.Token).ConfigureAwait(false);
            diagnosticsTracker?.MarkHeaders(response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                return Ra2AiResponse.CreateProviderFailure(
                    MapHttpFailureKind(response.StatusCode),
                    CreateHttpErrorMessage(response.StatusCode));
            }

            MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
            if (contentType is not null
                && !string.Equals(contentType.MediaType, EventStreamMediaType, StringComparison.OrdinalIgnoreCase))
            {
                return Ra2AiResponse.CreateProviderFailure(
                    Ra2AiFailureKind.ProtocolError,
                    MalformedResponseMessage);
            }

            await using Stream responseStream = await response.Content
                .ReadAsStreamAsync(requestSource.Token)
                .ConfigureAwait(false);
            using StreamReader reader = new(
                responseStream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: false);

            await foreach (Ra2AiStreamEvent streamEvent in _sseParser
                .ParseAsync(reader, requestSource.Token)
                .ConfigureAwait(false))
            {
                if (streamEvent.Kind == Ra2AiStreamEventKind.Completed)
                {
                    return MapCompletedResponse(
                        accumulatedText.ToString(),
                        streamEvent.FinishKind,
                        toolCallAccumulators);
                }

                if (streamEvent.Kind == Ra2AiStreamEventKind.ToolCallDelta)
                {
                    AppendToolCallDelta(toolCallAccumulators, streamEvent.ToolCallDelta);
                    idleTimeoutSource.CancelAfter(_options.StreamingIdleTimeout);
                    continue;
                }

                if (accumulatedText.Length + streamEvent.Text.Length
                    > _options.MaxStreamingResponseCharacters)
                {
                    throw new InvalidDataException(OversizedResponseMessage);
                }

                diagnosticsTracker?.MarkContentDelta(streamEvent.Text);
                accumulatedText.Append(streamEvent.Text);
                await InvokeContentDeltaAsync(
                    onContentDelta,
                    streamEvent.Text,
                    requestSource.Token).ConfigureAwait(false);
                idleTimeoutSource.CancelAfter(_options.StreamingIdleTimeout);
            }

            return CreateFailureResponse(
                accumulatedText.ToString(),
                Ra2AiFailureKind.ProtocolError,
                MalformedResponseMessage);
        }
        catch (OperationCanceledException)
        {
            string partialText = accumulatedText.ToString();
            RequestTerminationCause terminationCause = (RequestTerminationCause)Volatile.Read(
                ref firstTerminationCause);
            return terminationCause switch
            {
                RequestTerminationCause.UserCancellation => Ra2AiResponse.CreateCancelled(partialText),
                RequestTerminationCause.TotalTimeout => CreateTimeoutResponse(
                    partialText,
                    Ra2AiFailureKind.TotalTimeout),
                RequestTerminationCause.StreamingIdleTimeout => CreateTimeoutResponse(
                    partialText,
                    Ra2AiFailureKind.StreamingIdleTimeout),
                _ => CreateTimeoutResponse(partialText, Ra2AiFailureKind.Unknown)
            };
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode is HttpStatusCode statusCode)
            {
                return CreateFailureResponse(
                    accumulatedText.ToString(),
                    MapHttpFailureKind(statusCode),
                    CreateHttpErrorMessage(statusCode));
            }

            return CreateFailureResponse(
                accumulatedText.ToString(),
                Ra2AiFailureKind.NetworkOrProxy,
                ProviderErrorMessage);
        }
        catch (JsonException)
        {
            return CreateFailureResponse(
                accumulatedText.ToString(),
                Ra2AiFailureKind.ProtocolError,
                MalformedResponseMessage);
        }
        catch (DecoderFallbackException)
        {
            return CreateFailureResponse(
                accumulatedText.ToString(),
                Ra2AiFailureKind.ProtocolError,
                MalformedResponseMessage);
        }
        catch (InvalidDataException exception)
        {
            bool responseWasOversized = string.Equals(
                exception.Message,
                OversizedResponseMessage,
                StringComparison.Ordinal);
            return CreateFailureResponse(
                accumulatedText.ToString(),
                responseWasOversized
                    ? Ra2AiFailureKind.ResponseTooLarge
                    : Ra2AiFailureKind.ProtocolError,
                responseWasOversized ? OversizedResponseMessage : MalformedResponseMessage);
        }
        catch (IOException)
        {
            return CreateFailureResponse(
                accumulatedText.ToString(),
                Ra2AiFailureKind.NetworkOrProxy,
                ProviderErrorMessage);
        }
    }

    private HttpRequestMessage CreateRequestMessage(Uri endpoint, Ra2AiRequest request)
    {
        HttpRequestMessage message = new(HttpMethod.Post, endpoint);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey.Trim());
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(EventStreamMediaType));
        message.Content = new StringContent(
            BuildRequestJson(request),
            Encoding.UTF8,
            "application/json");
        return message;
    }

    private string BuildRequestJson(Ra2AiRequest request)
    {
        object[] messages = request.HasSeparatedMessages
            ?
            [
                new { role = "system", content = request.SystemPromptText },
                new { role = "user", content = request.UserContentText }
            ]
            :
            [
                new { role = "user", content = request.PromptText }
            ];
        object basePayload = new
        {
            model = _options.Model.Trim(),
            messages,
            temperature = _options.Temperature,
            thinking = new
            {
                type = "disabled"
            },
            max_tokens = _options.MaxOutputTokens,
            stream = true
        };
        if (request.Tools.Count == 0)
            return JsonSerializer.Serialize(basePayload);

        object[] tools = request.Tools
            .Select(tool => new
            {
                type = "function",
                function = new
                {
                    name = tool.Name,
                    description = tool.Description,
                    parameters = ParseSchema(tool.ParametersJsonSchema)
                }
            })
            .Cast<object>()
            .ToArray();
        return JsonSerializer.Serialize(new
        {
            model = _options.Model.Trim(),
            messages,
            temperature = _options.Temperature,
            thinking = new
            {
                type = "disabled"
            },
            max_tokens = _options.MaxOutputTokens,
            stream = true,
            tools,
            tool_choice = request.ToolChoice switch
            {
                Ra2AiToolChoiceMode.Auto => "auto",
                Ra2AiToolChoiceMode.Required => "required",
                _ => throw new InvalidOperationException("Unsupported tool choice.")
            }
        });
    }

    private static JsonElement ParseSchema(string schema)
    {
        using JsonDocument document = JsonDocument.Parse(schema);
        return document.RootElement.Clone();
    }

    private static async ValueTask InvokeContentDeltaAsync(
        Ra2AiContentDeltaHandler onContentDelta,
        string delta,
        CancellationToken cancellationToken)
    {
        try
        {
            await onContentDelta(delta, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new Ra2AiStreamConsumerException(exception);
        }
    }

    private static Ra2AiResponse MapCompletedResponse(
        string text,
        Ra2AiStreamFinishKind finishKind,
        IReadOnlyDictionary<int, ToolCallAccumulator> toolCallAccumulators)
    {
        if (finishKind == Ra2AiStreamFinishKind.Stop)
        {
            if (toolCallAccumulators.Count > 0)
            {
                return CreateFailureResponse(
                    text,
                    Ra2AiFailureKind.ProtocolError,
                    MalformedResponseMessage);
            }

            return string.IsNullOrWhiteSpace(text)
                ? Ra2AiResponse.CreateProviderFailure(
                    Ra2AiFailureKind.ProtocolError,
                    MissingContentMessage)
                : Ra2AiResponse.CreateSuccess(text);
        }

        if (finishKind == Ra2AiStreamFinishKind.ToolCalls)
        {
            if (toolCallAccumulators.Count == 0)
            {
                return CreateFailureResponse(
                    text,
                    Ra2AiFailureKind.ProtocolError,
                    MalformedResponseMessage);
            }

            try
            {
                Ra2AiToolCall[] calls = toolCallAccumulators
                    .OrderBy(pair => pair.Key)
                    .Select(pair => pair.Value.Complete())
                    .ToArray();
                return Ra2AiResponse.CreateToolCalls(calls, text);
            }
            catch (ArgumentException)
            {
                return CreateFailureResponse(
                    text,
                    Ra2AiFailureKind.ProtocolError,
                    MalformedResponseMessage);
            }
        }

        return Ra2AiResponse.CreateIncomplete(
            text,
            finishKind,
            safeErrorMessage: IncompleteResponseMessage);
    }

    private static void AppendToolCallDelta(
        IDictionary<int, ToolCallAccumulator> accumulators,
        Ra2AiToolCallDelta delta)
    {
        if (!accumulators.TryGetValue(delta.Index, out ToolCallAccumulator? accumulator))
        {
            if (accumulators.Count >= MaximumToolCallCount)
                throw new InvalidDataException(OversizedResponseMessage);

            accumulator = new ToolCallAccumulator();
            accumulators.Add(delta.Index, accumulator);
        }

        accumulator.Append(delta);
    }

    private static Ra2AiResponse CreateFailureResponse(
        string partialText,
        Ra2AiFailureKind failureKind,
        string errorMessage)
        => partialText.Length == 0
            ? Ra2AiResponse.CreateProviderFailure(failureKind, errorMessage)
            : Ra2AiResponse.CreateIncomplete(
                partialText,
                Ra2AiStreamFinishKind.Unknown,
                failureKind,
                errorMessage);

    private static Ra2AiResponse CreateTimeoutResponse(
        string partialText,
        Ra2AiFailureKind failureKind)
        => Ra2AiResponse.CreateTimeout(partialText, failureKind);

    private static Ra2AiFailureKind MapHttpFailureKind(HttpStatusCode statusCode)
    {
        int numericStatusCode = (int)statusCode;
        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                => Ra2AiFailureKind.AuthenticationOrAuthorization,
            HttpStatusCode.TooManyRequests => Ra2AiFailureKind.RateLimited,
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout
                => Ra2AiFailureKind.ProviderRequestTimeout,
            _ when numericStatusCode is >= 500 and <= 599
                => Ra2AiFailureKind.ServiceUnavailable,
            _ => Ra2AiFailureKind.RequestRejected
        };
    }

    private static string CreateHttpErrorMessage(HttpStatusCode statusCode)
        => $"DeepSeek provider returned HTTP {(int)statusCode}.";

    private static void TryTerminateRequest(
        ref int firstTerminationCause,
        RequestTerminationCause terminationCause,
        CancellationTokenSource requestSource)
    {
        int previousCause = Interlocked.CompareExchange(
            ref firstTerminationCause,
            (int)terminationCause,
            (int)RequestTerminationCause.None);
        if (previousCause == (int)RequestTerminationCause.None)
            requestSource.Cancel();
    }

    private sealed class RequestDiagnosticsTracker
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly string _requestId = Guid.NewGuid().ToString("N");
        private readonly string _modelId;
        private readonly int _promptCharacterCount;
        private TimeSpan? _sendStartedAt;
        private TimeSpan? _timeToHeaders;
        private TimeSpan? _timeToFirstContent;
        private int _contentDeltaCount;
        private int _contentCharacterCount;
        private int? _httpStatusCode;

        public RequestDiagnosticsTracker(string modelId, int promptCharacterCount)
        {
            _modelId = modelId;
            _promptCharacterCount = promptCharacterCount;
        }

        public void MarkSending()
            => _sendStartedAt ??= _stopwatch.Elapsed;

        public void MarkHeaders(HttpStatusCode statusCode)
        {
            _timeToHeaders ??= ElapsedSinceSendStarted();
            _httpStatusCode = (int)statusCode;
        }

        public void MarkContentDelta(string text)
        {
            _timeToFirstContent ??= ElapsedSinceSendStarted();
            _contentDeltaCount++;
            _contentCharacterCount += text.Length;
        }

        public Ra2AiRequestDiagnostics Complete()
        {
            _stopwatch.Stop();
            return new Ra2AiRequestDiagnostics(
                _requestId,
                _modelId,
                _promptCharacterCount,
                _timeToHeaders,
                _timeToFirstContent,
                _stopwatch.Elapsed,
                _contentDeltaCount,
                _contentCharacterCount,
                _httpStatusCode);
        }

        private TimeSpan ElapsedSinceSendStarted()
            => _stopwatch.Elapsed - (_sendStartedAt ?? TimeSpan.Zero);
    }

    private sealed class ToolCallAccumulator
    {
        private readonly StringBuilder _id = new();
        private readonly StringBuilder _name = new();
        private readonly StringBuilder _arguments = new();

        public void Append(Ra2AiToolCallDelta delta)
        {
            AppendWithinLimit(
                _id,
                delta.IdFragment,
                Ra2AiToolCall.MaximumIdLength);
            AppendWithinLimit(
                _name,
                delta.NameFragment,
                Ra2AiToolCall.MaximumNameLength);
            AppendWithinLimit(
                _arguments,
                delta.ArgumentsFragment,
                Ra2AiToolCall.MaximumArgumentsLength);
        }

        public Ra2AiToolCall Complete()
            => new(_id.ToString(), _name.ToString(), _arguments.ToString());

        private static void AppendWithinLimit(
            StringBuilder builder,
            string fragment,
            int maximumLength)
        {
            if (builder.Length + fragment.Length > maximumLength)
                throw new InvalidDataException(OversizedResponseMessage);

            builder.Append(fragment);
        }
    }

    private enum RequestTerminationCause
    {
        None = 0,
        UserCancellation,
        TotalTimeout,
        StreamingIdleTimeout
    }

    private sealed class Ra2AiStreamConsumerException(Exception innerException)
        : Exception("The AI stream consumer callback failed.", innerException);
}
