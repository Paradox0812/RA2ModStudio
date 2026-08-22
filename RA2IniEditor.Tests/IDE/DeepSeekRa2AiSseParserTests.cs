using System.Text.Json;
using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class DeepSeekRa2AiSseParserTests
{
    [Fact]
    public async Task ParseAsync_ContentDeltasAndDone_PreservesProtocolOrder()
    {
        const string source = """
            data: {"choices":[{"delta":{"role":"assistant","content":"Hello"}}]}

            data: {"choices":[{"delta":{"content":" world"}}]}

            data: [DONE]

            """;

        IReadOnlyList<Ra2AiStreamEvent> events = await ParseAsync(source);

        Assert.Equal(3, events.Count);
        Assert.Equal(Ra2AiStreamEventKind.ContentDelta, events[0].Kind);
        Assert.Equal("Hello", events[0].Text);
        Assert.Equal(Ra2AiStreamEventKind.ContentDelta, events[1].Kind);
        Assert.Equal(" world", events[1].Text);
        Assert.Equal(Ra2AiStreamEventKind.Completed, events[2].Kind);
        Assert.Equal(string.Empty, events[2].Text);
        Assert.Equal(Ra2AiStreamFinishKind.Unknown, events[2].FinishKind);
    }

    [Theory]
    [InlineData("stop", (int)Ra2AiStreamFinishKind.Stop)]
    [InlineData("length", (int)Ra2AiStreamFinishKind.Length)]
    [InlineData("content_filter", (int)Ra2AiStreamFinishKind.ContentFilter)]
    [InlineData("tool_calls", (int)Ra2AiStreamFinishKind.ToolCalls)]
    [InlineData("insufficient_system_resource", (int)Ra2AiStreamFinishKind.InsufficientSystemResource)]
    [InlineData("future_reason", (int)Ra2AiStreamFinishKind.Unknown)]
    public async Task ParseAsync_FinishReason_IsReportedWhenDoneArrives(
        string finishReason,
        int expected)
    {
        string source = $$"""
            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{},"finish_reason":"{{finishReason}}"}]}

            data: [DONE]

            """;

        IReadOnlyList<Ra2AiStreamEvent> events = await ParseAsync(source);

        Ra2AiStreamEvent completed = Assert.Single(events);
        Assert.Equal(Ra2AiStreamEventKind.Completed, completed.Kind);
        Assert.Equal((Ra2AiStreamFinishKind)expected, completed.FinishKind);
    }

    [Fact]
    public async Task ParseAsync_MultipleChoices_SelectsIndexZeroRegardlessOfArrayOrder()
    {
        const string source = """
            data: {"choices":[{"index":1,"delta":{"content":"wrong"}},{"index":0,"delta":{"content":"right"}}]}

            data: [DONE]

            """;

        IReadOnlyList<Ra2AiStreamEvent> events = await ParseAsync(source);

        Assert.Equal(2, events.Count);
        Assert.Equal("right", events[0].Text);
    }

    [Fact]
    public async Task ParseAsync_MixedContentAndFragmentedToolCall_EmitsSeparateOrderedEvents()
    {
        const string source = """
            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{"content":"说明","tool_calls":[{"index":0,"id":"call-","type":"function","function":{"name":"preview_ini_","arguments":"{\"summary\":"}}]},"finish_reason":null}]}

            data: {"id":"stream-1","object":"chat.completion.chunk","choices":[{"index":0,"delta":{"tool_calls":[{"index":0,"id":"1","function":{"name":"edit_plan","arguments":"\"x\",\"operations\":[]}"}}]},"finish_reason":"tool_calls"}]}

            data: [DONE]

            """;

        IReadOnlyList<Ra2AiStreamEvent> events = await ParseAsync(source);

        Assert.Equal(4, events.Count);
        Assert.Equal(Ra2AiStreamEventKind.ContentDelta, events[0].Kind);
        Assert.Equal("说明", events[0].Text);
        Assert.Equal(Ra2AiStreamEventKind.ToolCallDelta, events[1].Kind);
        Assert.Equal("call-", events[1].ToolCallDelta.IdFragment);
        Assert.Equal("preview_ini_", events[1].ToolCallDelta.NameFragment);
        Assert.Equal(Ra2AiStreamEventKind.ToolCallDelta, events[2].Kind);
        Assert.Equal("1", events[2].ToolCallDelta.IdFragment);
        Assert.Equal("edit_plan", events[2].ToolCallDelta.NameFragment);
        Assert.Equal(Ra2AiStreamEventKind.Completed, events[3].Kind);
        Assert.Equal(Ra2AiStreamFinishKind.ToolCalls, events[3].FinishKind);
    }

    [Theory]
    [InlineData("""{"choices":[{"index":0,"delta":{"tool_calls":{}}}]}""")]
    [InlineData("""{"choices":[{"index":0,"delta":{"tool_calls":[{"function":{"name":"x"}}]}}]}""")]
    [InlineData("""{"choices":[{"index":0,"delta":{"tool_calls":[{"index":-1,"id":"x"}]}}]}""")]
    [InlineData("""{"choices":[{"index":0,"delta":{"tool_calls":[{"index":0,"type":"custom","id":"x"}]}}]}""")]
    [InlineData("""{"choices":[{"index":0,"delta":{"tool_calls":[{"index":0,"function":[]}]}}]}""")]
    public async Task ParseAsync_InvalidToolDeltaShape_ThrowsJsonException(string payload)
    {
        await Assert.ThrowsAsync<JsonException>(() => ParseAsync($"data: {payload}\n\n"));
    }

    [Fact]
    public async Task ParseAsync_MismatchedStreamIds_ThrowsJsonException()
    {
        const string source = """
            data: {"id":"stream-1","choices":[{"index":0,"delta":{"content":"A"}}]}

            data: {"id":"stream-2","choices":[{"index":0,"delta":{"content":"B"}}]}

            """;

        await Assert.ThrowsAsync<JsonException>(() => ParseAsync(source));
    }

    [Fact]
    public async Task ParseAsync_UnexpectedChunkObject_ThrowsJsonException()
    {
        const string source = "data: {\"object\":\"chat.completion\",\"choices\":[]}\n\n";

        await Assert.ThrowsAsync<JsonException>(() => ParseAsync(source));
    }

    [Fact]
    public async Task ParseAsync_OversizedEvent_ThrowsInvalidDataException()
    {
        string source = $"data: {new string('x', (1024 * 1024) + 1)}\n\n";

        await Assert.ThrowsAsync<InvalidDataException>(() => ParseAsync(source));
    }

    [Fact]
    public async Task ParseAsync_CrLfAndMultiLineData_AreSupported()
    {
        const string source = "data: {\r\ndata: \"choices\":[{\"delta\":{\"content\":\"A\"}}]}\r\n\r\ndata: [DONE]\r\n\r\n";

        IReadOnlyList<Ra2AiStreamEvent> events = await ParseAsync(source);

        Assert.Equal(2, events.Count);
        Assert.Equal("A", events[0].Text);
        Assert.Equal(Ra2AiStreamEventKind.Completed, events[1].Kind);
    }

    [Fact]
    public async Task ParseAsync_KeepAliveMetadataRoleReasoningAndUsage_DoNotProduceContent()
    {
        const string source = """
            : keep-alive

            event: ignored
            id: ignored

            data: {"choices":[{"delta":{"role":"assistant","content":""}}]}

            data: {"choices":[{"delta":{"reasoning_content":"hidden reasoning"}}]}

            data: {"choices":[],"usage":{"total_tokens":42}}

            data: {"choices":[{"delta":{"content":"visible"}}]}

            data: [DONE]

            """;

        IReadOnlyList<Ra2AiStreamEvent> events = await ParseAsync(source);

        Assert.Equal(2, events.Count);
        Assert.Equal("visible", events[0].Text);
        Assert.DoesNotContain(events, item => item.Text.Contains("hidden reasoning", StringComparison.Ordinal));
        Assert.Equal(Ra2AiStreamEventKind.Completed, events[1].Kind);
    }

    [Fact]
    public async Task ParseAsync_DoneStopsWithoutReadingLaterEvents()
    {
        const string source = "data: [DONE]\n\ndata: { malformed later data\n\n";

        IReadOnlyList<Ra2AiStreamEvent> events = await ParseAsync(source);

        Ra2AiStreamEvent completed = Assert.Single(events);
        Assert.Equal(Ra2AiStreamEventKind.Completed, completed.Kind);
    }

    [Fact]
    public async Task ParseAsync_MalformedJson_ThrowsJsonException()
    {
        await Assert.ThrowsAnyAsync<JsonException>(() => ParseAsync("data: { malformed\n\n"));
    }

    [Theory]
    [InlineData("{\"object\":\"chat.completion.chunk\"}")]
    [InlineData("{\"choices\":{}}")]
    [InlineData("{\"choices\":[{}]}")]
    [InlineData("{\"choices\":[{\"delta\":[]}]}" )]
    [InlineData("{\"choices\":[{\"delta\":{\"content\":42}}]}")]
    public async Task ParseAsync_InvalidChunkShape_ThrowsJsonException(string payload)
    {
        await Assert.ThrowsAsync<JsonException>(() => ParseAsync($"data: {payload}\n\n"));
    }

    [Fact]
    public async Task ParseAsync_EndOfStreamBeforeDone_ThrowsInvalidDataException()
    {
        const string source = "data: {\"choices\":[{\"delta\":{\"content\":\"partial\"}}]}\n\n";

        await Assert.ThrowsAsync<InvalidDataException>(() => ParseAsync(source));
    }

    [Fact]
    public async Task ParseAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource source = new();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => ParseAsync("data: [DONE]\n\n", source.Token));
    }

    [Fact]
    public async Task ParseAsync_CancellationDuringRead_ThrowsOperationCanceledException()
    {
        DeepSeekRa2AiSseParser parser = new();
        using BlockingTextReader reader = new();
        using CancellationTokenSource source = new();

        Task<IReadOnlyList<Ra2AiStreamEvent>> parseTask = CollectAsync(parser, reader, source.Token);
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => parseTask);
    }

    [Fact]
    public async Task ParseAsync_DoesNotDisposeCallerOwnedReader()
    {
        DeepSeekRa2AiSseParser parser = new();
        using TrackingTextReader reader = new("data: [DONE]\n\n");

        IReadOnlyList<Ra2AiStreamEvent> events = await CollectAsync(parser, reader, CancellationToken.None);

        Assert.Single(events);
        Assert.False(reader.WasDisposed);
    }

    private static async Task<IReadOnlyList<Ra2AiStreamEvent>> ParseAsync(
        string source,
        CancellationToken cancellationToken = default)
    {
        DeepSeekRa2AiSseParser parser = new();
        using StringReader reader = new(source);
        return await CollectAsync(parser, reader, cancellationToken);
    }

    private static async Task<IReadOnlyList<Ra2AiStreamEvent>> CollectAsync(
        DeepSeekRa2AiSseParser parser,
        TextReader reader,
        CancellationToken cancellationToken)
    {
        List<Ra2AiStreamEvent> events = [];
        await foreach (Ra2AiStreamEvent streamEvent in parser.ParseAsync(reader, cancellationToken))
            events.Add(streamEvent);

        return events;
    }

    private sealed class BlockingTextReader : TextReader
    {
        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return null;
        }
    }

    private sealed class TrackingTextReader(string text) : StringReader(text)
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }
}
