using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiIncrementalTextBufferTests
{
    [Fact]
    public void AppendAndDrainPending_PreserveExactAcceptedOrder()
    {
        Ra2AiIncrementalTextBuffer buffer = new();

        buffer.Append("alpha");
        buffer.Append("\r\n");
        buffer.Append("beta");

        Assert.Equal(11, buffer.PendingCharacterCount);
        Assert.Equal("alpha\r\nbeta", buffer.DrainPending());
        Assert.Equal(0, buffer.PendingCharacterCount);
        Assert.Equal("alpha\r\nbeta", buffer.GetAccumulatedText());
    }

    [Fact]
    public void DrainPending_ClearsOnlyPendingText()
    {
        Ra2AiIncrementalTextBuffer buffer = new();
        buffer.Append("first");

        Assert.Equal("first", buffer.DrainPending());
        Assert.Equal(string.Empty, buffer.DrainPending());

        buffer.Append("second");

        Assert.Equal("second", buffer.DrainPending());
        Assert.Equal("firstsecond", buffer.GetAccumulatedText());
    }

    [Fact]
    public void Append_EmptyDeltaDoesNotChangeStateAndNullIsRejected()
    {
        Ra2AiIncrementalTextBuffer buffer = new();

        buffer.Append(string.Empty);

        Assert.Equal(0, buffer.PendingCharacterCount);
        Assert.Equal(string.Empty, buffer.GetAccumulatedText());
        Assert.Throws<ArgumentNullException>(() => buffer.Append(null!));
    }

    [Fact]
    public void Append_ConcurrentCallsDoNotLoseOrCorruptChunks()
    {
        Ra2AiIncrementalTextBuffer buffer = new();
        string[] chunks = Enumerable.Range(0, 200)
            .Select(index => $"<{index:D3}>")
            .ToArray();

        Parallel.ForEach(chunks, buffer.Append);

        string accumulated = buffer.GetAccumulatedText();
        Assert.Equal(chunks.Sum(chunk => chunk.Length), accumulated.Length);
        Assert.All(chunks, chunk => Assert.Contains(chunk, accumulated, StringComparison.Ordinal));
        Assert.Equal(accumulated, buffer.DrainPending());
    }

    [Fact]
    public void AccumulatedTextEquals_UsesExactOrdinalComparison()
    {
        Ra2AiIncrementalTextBuffer buffer = new();
        buffer.Append("Alpha\r\nBeta");

        Assert.True(buffer.AccumulatedTextEquals("Alpha\r\nBeta"));
        Assert.False(buffer.AccumulatedTextEquals("alpha\r\nBeta"));
        Assert.False(buffer.AccumulatedTextEquals("Alpha\nBeta"));
        Assert.Throws<ArgumentNullException>(() => buffer.AccumulatedTextEquals(null!));
    }
}
