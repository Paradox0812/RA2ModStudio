using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiRequestLifecycleTests
{
    [Fact]
    public void TryStart_AllowsOnlyOneActiveRequest()
    {
        Ra2AiRequestLifecycle lifecycle = new();

        Assert.True(lifecycle.TryStart(out Ra2AiRequestSession? firstSession));
        Assert.NotNull(firstSession);
        Assert.True(lifecycle.IsActive);
        Assert.False(lifecycle.TryStart(out Ra2AiRequestSession? duplicateSession));
        Assert.Null(duplicateSession);

        Assert.True(lifecycle.TryComplete(firstSession));
        firstSession.Dispose();
        Assert.False(lifecycle.IsActive);
    }

    [Fact]
    public void TryCancelCurrent_CancelsOnceWithoutCompletingRequest()
    {
        Ra2AiRequestLifecycle lifecycle = new();
        Assert.True(lifecycle.TryStart(out Ra2AiRequestSession? session));
        Assert.NotNull(session);

        Assert.True(lifecycle.TryCancelCurrent());
        Assert.True(session.IsCancellationRequested);
        Assert.True(lifecycle.IsActive);
        Assert.False(lifecycle.TryCancelCurrent());

        Assert.True(lifecycle.TryComplete(session));
        session.Dispose();
    }

    [Fact]
    public void TryComplete_RejectsForeignAndStaleSessions()
    {
        Ra2AiRequestLifecycle lifecycle = new();
        Assert.True(lifecycle.TryStart(out Ra2AiRequestSession? firstSession));
        Assert.NotNull(firstSession);
        using Ra2AiRequestSession foreignSession = new();

        Assert.False(lifecycle.TryComplete(foreignSession));
        Assert.True(lifecycle.IsActive);
        Assert.True(lifecycle.TryComplete(firstSession));
        firstSession.Dispose();

        Assert.True(lifecycle.TryStart(out Ra2AiRequestSession? secondSession));
        Assert.NotNull(secondSession);
        Assert.False(lifecycle.TryComplete(firstSession));
        Assert.True(lifecycle.IsActive);

        Assert.True(lifecycle.TryComplete(secondSession));
        secondSession.Dispose();
        Assert.False(lifecycle.IsActive);
    }

    [Fact]
    public void Session_CancelAndDisposeAreIdempotent()
    {
        Ra2AiRequestSession session = new();

        session.Cancel();
        session.Cancel();
        Assert.True(session.IsCancellationRequested);

        session.Dispose();
        session.Dispose();
        session.Cancel();
    }

    [Fact]
    public void CancelledSessionCannotBecomeCurrentAgainAfterCompletion()
    {
        Ra2AiRequestLifecycle lifecycle = new();
        Assert.True(lifecycle.TryStart(out Ra2AiRequestSession? first));
        Assert.NotNull(first);

        Assert.True(lifecycle.TryCancelCurrent());
        Assert.True(lifecycle.TryComplete(first));
        first.Dispose();
        Assert.False(lifecycle.TryComplete(first));

        Assert.True(lifecycle.TryStart(out Ra2AiRequestSession? second));
        Assert.NotNull(second);
        Assert.True(lifecycle.TryComplete(second));
        second.Dispose();
    }
}
