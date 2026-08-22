using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class DeepSeekRa2AiLoopbackIntegrationTests
{
    [Fact]
    public async Task Loopback_FragmentedUtf8SseProducesOrderedSuccessAndDiagnostics()
    {
        const string content = "中文🙂分片";
        await using LoopbackServer server = new(async (stream, token) =>
        {
            await WriteHeadersAsync(stream, 200, "text/event-stream", token);
            byte[] body = Encoding.UTF8.GetBytes(CreateCompletedSse(content));
            foreach (byte value in body)
                await stream.WriteAsync(new[] { value }, token);
        });
        DeepSeekRa2AiClient client = CreateClient(server);
        List<string> deltas = [];

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            (delta, _) =>
            {
                deltas.Add(delta);
                return ValueTask.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal(content, response.Text);
        Assert.Equal(content, string.Concat(deltas));
        Ra2AiRequestDiagnostics diagnostics = Assert.IsType<Ra2AiRequestDiagnostics>(response.Diagnostics);
        Assert.Equal(200, diagnostics.HttpStatusCode);
        Assert.NotNull(diagnostics.TimeToHeaders);
        Assert.NotNull(diagnostics.TimeToFirstContent);
        Assert.True(diagnostics.ContentDeltaCount >= 1);
        Assert.Equal(content.Length, diagnostics.ContentCharacterCount);
    }

    [Theory]
    [InlineData(401, (int)Ra2AiFailureKind.AuthenticationOrAuthorization)]
    [InlineData(429, (int)Ra2AiFailureKind.RateLimited)]
    [InlineData(503, (int)Ra2AiFailureKind.ServiceUnavailable)]
    public async Task Loopback_HttpFailureMapsStableKindWithoutReadingBody(
        int statusCode,
        int expectedFailureKind)
    {
        await using LoopbackServer server = new((stream, token) =>
            WriteHeadersAsync(stream, statusCode, "text/plain", token));
        DeepSeekRa2AiClient client = CreateClient(server);

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Equal((Ra2AiFailureKind)expectedFailureKind, response.FailureKind);
        Assert.Equal(statusCode, response.Diagnostics?.HttpStatusCode);
        Assert.DoesNotContain("loopback-secret", response.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Loopback_AbnormalEofAfterContentReturnsProtocolIncomplete()
    {
        await using LoopbackServer server = new(async (stream, token) =>
        {
            await WriteHeadersAsync(stream, 200, "text/event-stream", token);
            await WriteUtf8Async(stream, CreateDeltaSse("partial"), token);
        });
        DeepSeekRa2AiClient client = CreateClient(server);

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Incomplete, response.Kind);
        Assert.Equal(Ra2AiFailureKind.ProtocolError, response.FailureKind);
        Assert.Equal("partial", response.Text);
    }

    [Fact]
    public async Task Loopback_NoHeadersTriggersTotalTimeoutWithoutPreciseSchedulingDependency()
    {
        await using LoopbackServer server = new(async (_, token) =>
            await Task.Delay(Timeout.InfiniteTimeSpan, token));
        DeepSeekRa2AiClient client = CreateClient(
            server,
            timeout: TimeSpan.FromMilliseconds(250),
            idleTimeout: TimeSpan.FromSeconds(2));

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Timeout, response.Kind);
        Assert.Equal(Ra2AiFailureKind.TotalTimeout, response.FailureKind);
        Assert.Null(response.Diagnostics?.TimeToHeaders);
    }

    [Fact]
    public async Task Loopback_IdleAfterFirstContentTriggersStreamingIdleTimeout()
    {
        await using LoopbackServer server = new(async (stream, token) =>
        {
            await WriteHeadersAsync(stream, 200, "text/event-stream", token);
            await WriteUtf8Async(stream, CreateDeltaSse("partial"), token);
            await stream.FlushAsync(token);
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        });
        DeepSeekRa2AiClient client = CreateClient(
            server,
            timeout: TimeSpan.FromSeconds(2),
            idleTimeout: TimeSpan.FromMilliseconds(200));

        Ra2AiResponse response = await client.SendStreamingAsync(
            CreateRequest(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.Timeout, response.Kind);
        Assert.Equal(Ra2AiFailureKind.StreamingIdleTimeout, response.FailureKind);
        Assert.Equal("partial", response.Text);
        Assert.NotNull(response.Diagnostics?.TimeToFirstContent);
    }

    [Fact]
    public async Task Loopback_UserCancellationWinsBeforeLongTotalTimeout()
    {
        await using LoopbackServer server = new(async (_, token) =>
            await Task.Delay(Timeout.InfiniteTimeSpan, token));
        DeepSeekRa2AiClient client = CreateClient(
            server,
            timeout: TimeSpan.FromSeconds(2),
            idleTimeout: TimeSpan.FromSeconds(2));
        using CancellationTokenSource source = new(TimeSpan.FromMilliseconds(200));

        Ra2AiResponse response = await client.SendAsync(CreateRequest(), source.Token);

        Assert.Equal(Ra2AiResponseKind.Cancelled, response.Kind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
    }

    [Fact]
    public async Task Loopback_AuthoringPipelineProducesLocallyValidatedPreview()
    {
        const string arguments = """
            {
              "outcome":"proposal",
              "summary":"Update Strength",
              "operations":[{
                "kind":"replace_field_value",
                "section":"E1",
                "key":"Strength",
                "value":"150"
              }]
            }
            """;
        await using LoopbackServer server = new(async (stream, token) =>
        {
            await WriteHeadersAsync(stream, 200, "text/event-stream", token);
            await WriteUtf8Async(stream, CreateToolCallSse(arguments), token);
        });
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), CreateClient(server));

        Ra2AiAssistantPipelineResult pipelineResult = await pipeline.SendStreamingAsync(
            "把当前文件 [E1] 下的 Strength 修改为 150",
            CreateAuthoringContext(),
            conversationContext: null,
            currentSubject: null,
            Ra2AiCapabilityMode.CurrentDocumentEditPreview,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiToolChoiceMode.Required, pipelineResult.Request.ToolChoice);
        Assert.True(pipelineResult.Request.HasSeparatedMessages);
        Assert.Equal(Ra2AiResponseKind.ToolCalls, pipelineResult.Response.Kind);

        AuthoringFixture fixture = new();
        Ra2AiEditProposalResult proposalResult = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            pipelineResult.Response,
            CancellationToken.None);

        Assert.True(proposalResult.Succeeded);
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(proposalResult.Proposal);
        Assert.Contains("Strength=150", proposal.Preview.CandidateText, StringComparison.Ordinal);
        Assert.Same(proposal, fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public async Task Loopback_RequiredAuthoringWithoutToolCallBecomesTypedFailure()
    {
        await using LoopbackServer server = new(async (stream, token) =>
        {
            await WriteHeadersAsync(stream, 200, "text/event-stream", token);
            await WriteUtf8Async(stream, CreateCompletedSse("provider prose only"), token);
        });
        Ra2AiAssistantPipeline pipeline = new(new Ra2AiPromptBuilder(), CreateClient(server));

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "把当前文件 [E1] 下的 Strength 修改为 150",
            CreateAuthoringContext(),
            conversationContext: null,
            currentSubject: null,
            Ra2AiCapabilityMode.CurrentDocumentEditPreview,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.AuthoringToolNotInvoked, result.Response.Kind);
        Assert.Equal("provider prose only", result.Response.Text);
        Assert.False(result.Response.IsSuccessfulTerminal);
    }

    private static DeepSeekRa2AiClient CreateClient(
        LoopbackServer server,
        TimeSpan? timeout = null,
        TimeSpan? idleTimeout = null)
        => new(
            new DeepSeekRa2AiClientOptions
            {
                BaseUrl = server.BaseUrl,
                ApiKey = "loopback-secret-placeholder",
                Model = "deepseek-v4-flash",
                Timeout = timeout ?? TimeSpan.FromSeconds(2),
                StreamingIdleTimeout = idleTimeout ?? TimeSpan.FromSeconds(1)
            },
            new HttpClient());

    private static Ra2AiRequest CreateRequest()
        => new(Ra2AiIntent.Auto, "loopback request", "bounded loopback prompt");

    private static Ra2AiContext CreateAuthoringContext()
        => new(
            "rulesmd.ini",
            caretOffset: 36,
            lineNumber: 4,
            Ra2CaretRegion.Value,
            "E1",
            "Infantry",
            "Strength",
            "100",
            selectedText: null,
            nearbyText: "[E1]\nStrength=100",
            nearbyLineCount: 2,
            hasSemanticContext: true);

    private static async Task WriteHeadersAsync(
        NetworkStream stream,
        int statusCode,
        string contentType,
        CancellationToken cancellationToken)
    {
        string headers = $"HTTP/1.1 {statusCode} Test\r\nContent-Type: {contentType}\r\nConnection: close\r\n\r\n";
        await WriteUtf8Async(stream, headers, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private static Task WriteUtf8Async(
        NetworkStream stream,
        string text,
        CancellationToken cancellationToken)
        => stream.WriteAsync(Encoding.UTF8.GetBytes(text), cancellationToken).AsTask();

    private static string CreateDeltaSse(string content)
        => $"data: {JsonSerializer.Serialize(new
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
        })}\n\n";

    private static string CreateCompletedSse(string content)
        => CreateDeltaSse(content)
            + "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n"
            + "data: [DONE]\n\n";

    private static string CreateToolCallSse(string arguments)
        => $"data: {JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    index = 0,
                    delta = new
                    {
                        tool_calls = new[]
                        {
                            new
                            {
                                index = 0,
                                id = "call-1",
                                type = "function",
                                function = new
                                {
                                    name = Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                                    arguments
                                }
                            }
                        }
                    },
                    finish_reason = "tool_calls"
                }
            }
        })}\n\ndata: [DONE]\n\n";

    private sealed class AuthoringFixture
    {
        public AuthoringFixture()
        {
            Ra2EditableDocumentSessionService sessionService = new(
                new Ra2IniTextDocumentParser(),
                new Ra2DirtyStateService());
            Ra2EditableDocumentSession session = sessionService.StartEditing(
                "rulesmd.ini",
                "[InfantryTypes]\n1=E1\n\n[E1]\nStrength=100\n");
            Ra2FieldRegistryProviderSnapshot registry = new(
                new BuiltInRa2FieldDefinitionProvider(),
                revision: 11);
            Snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
                Ra2AuthoringSnapshot.Capture(
                    session,
                    session.DocumentState.CurrentText,
                    string.Empty,
                    registry).Snapshot);
            Ra2IniAuthoringWorkspace workspace = new(
                new Ra2IniEditPreviewService(
                    new Ra2IniLanguageAnalysisService(),
                    new Ra2AddPropertyInsertPlanner()),
                new PreviewOnlyTransactionPort());
            Coordinator = new Ra2AiAuthoringCoordinator(
                new Ra2AiAuthoringToolAdapter(),
                workspace);
        }

        public Ra2AuthoringSnapshot Snapshot { get; }

        public Ra2AiAuthoringCoordinator Coordinator { get; }
    }

    private sealed class PreviewOnlyTransactionPort : IRa2EditorTransactionPort
    {
        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
            => throw new InvalidOperationException("The integration test does not authorize apply.");
    }

    private sealed class LoopbackServer : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _disposeSource = new();
        private readonly Task _serverTask;

        public LoopbackServer(Func<NetworkStream, CancellationToken, Task> responder)
        {
            _listener.Start();
            int port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            BaseUrl = $"http://127.0.0.1:{port}";
            _serverTask = RunAsync(responder, _disposeSource.Token);
        }

        public string BaseUrl { get; }

        public async ValueTask DisposeAsync()
        {
            _disposeSource.Cancel();
            _listener.Stop();
            try
            {
                await _serverTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                _disposeSource.Dispose();
            }
        }

        private async Task RunAsync(
            Func<NetworkStream, CancellationToken, Task> responder,
            CancellationToken cancellationToken)
        {
            using TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
            await using NetworkStream stream = client.GetStream();
            await ReadRequestHeadersAsync(stream, cancellationToken);
            await responder(stream, cancellationToken);
        }

        private static async Task ReadRequestHeadersAsync(
            NetworkStream stream,
            CancellationToken cancellationToken)
        {
            using MemoryStream headerBuffer = new();
            byte[] buffer = new byte[1];
            int matchIndex = 0;
            byte[] terminator = "\r\n\r\n"u8.ToArray();
            while (matchIndex < terminator.Length)
            {
                int read = await stream.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                    throw new IOException("Loopback client closed before sending headers.");

                headerBuffer.WriteByte(buffer[0]);
                matchIndex = buffer[0] == terminator[matchIndex]
                    ? matchIndex + 1
                    : buffer[0] == terminator[0] ? 1 : 0;
            }

            string headers = Encoding.ASCII.GetString(headerBuffer.ToArray());
            int contentLength = headers
                .Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                .Select(line => int.Parse(line[(line.IndexOf(':') + 1)..].Trim()))
                .SingleOrDefault();
            byte[] body = new byte[contentLength];
            int totalRead = 0;
            while (totalRead < body.Length)
            {
                int read = await stream.ReadAsync(
                    body.AsMemory(totalRead, body.Length - totalRead),
                    cancellationToken);
                if (read == 0)
                    throw new IOException("Loopback client closed before sending its request body.");

                totalRead += read;
            }
        }
    }
}
