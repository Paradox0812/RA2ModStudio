using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiResponseTests
{
    [Fact]
    public void CreateSuccess_ProducesOnlyLegalSuccessCombination()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateSuccess("answer");

        Assert.Equal(Ra2AiResponseKind.Success, response.Kind);
        Assert.Equal("answer", response.Text);
        Assert.Null(response.ErrorMessage);
        Assert.Equal(Ra2AiStreamFinishKind.Stop, response.FinishKind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void CreateCancelled_PreservesPartialTextWithoutFailureOrError()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateCancelled("partial");

        Assert.Equal(Ra2AiResponseKind.Cancelled, response.Kind);
        Assert.Equal("partial", response.Text);
        Assert.Null(response.ErrorMessage);
        Assert.Equal(Ra2AiStreamFinishKind.Unknown, response.FinishKind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
    }

    [Fact]
    public void CreateAuthoringToolNotInvoked_PreservesProviderTextAsNonSuccess()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateAuthoringToolNotInvoked("plain markdown");

        Assert.Equal(Ra2AiResponseKind.AuthoringToolNotInvoked, response.Kind);
        Assert.Equal("plain markdown", response.Text);
        Assert.False(response.IsSuccess);
        Assert.False(response.IsSuccessfulTerminal);
        Assert.Equal(Ra2AiStreamFinishKind.Stop, response.FinishKind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
    }

    [Theory]
    [InlineData((int)Ra2AiFailureKind.TotalTimeout)]
    [InlineData((int)Ra2AiFailureKind.StreamingIdleTimeout)]
    [InlineData((int)Ra2AiFailureKind.Unknown)]
    public void CreateTimeout_AcceptsOnlyTimeoutFailureKinds(int failureKind)
    {
        Ra2AiResponse response = Ra2AiResponse.CreateTimeout(
            "partial",
            (Ra2AiFailureKind)failureKind);

        Assert.Equal(Ra2AiResponseKind.Timeout, response.Kind);
        Assert.Equal("partial", response.Text);
        Assert.Equal((Ra2AiFailureKind)failureKind, response.FailureKind);
        Assert.Equal("DeepSeek provider request timed out.", response.ErrorMessage);
        Assert.Equal(Ra2AiStreamFinishKind.Unknown, response.FinishKind);
    }

    [Fact]
    public void CreateMissingConfiguration_UsesFixedSafeCombination()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateMissingConfiguration();

        Assert.Equal(Ra2AiResponseKind.MissingConfiguration, response.Kind);
        Assert.Empty(response.Text);
        Assert.Equal(Ra2AiFailureKind.MissingConfiguration, response.FailureKind);
        Assert.Equal(Ra2AiStreamFinishKind.Unknown, response.FinishKind);
        Assert.Equal("DeepSeek configuration is missing or invalid.", response.ErrorMessage);
    }

    [Fact]
    public void CreateProviderFailure_RequiresSpecificFailureAndSafeMessage()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateProviderFailure(
            Ra2AiFailureKind.RateLimited,
            "DeepSeek provider returned HTTP 429.");

        Assert.Equal(Ra2AiResponseKind.ProviderError, response.Kind);
        Assert.Empty(response.Text);
        Assert.Equal(Ra2AiFailureKind.RateLimited, response.FailureKind);
        Assert.Equal(Ra2AiStreamFinishKind.Unknown, response.FinishKind);
    }

    [Fact]
    public void CreateLocalRejection_SeparatesSafeLocalReasonFromProviderFailure()
    {
        Ra2AiRequestDiagnostics diagnostics = new(
            "0123456789abcdef0123456789abcdef",
            "deepseek-v4-flash",
            100,
            TimeSpan.FromMilliseconds(2),
            null,
            TimeSpan.FromMilliseconds(4),
            0,
            0,
            200);

        Ra2AiResponse response = Ra2AiResponse.CreateLocalRejection(
            "  当前项目缺少 rules/art 配对。  ",
            diagnostics);

        Assert.Equal(Ra2AiResponseKind.LocalRejection, response.Kind);
        Assert.Equal("当前项目缺少 rules/art 配对。", response.LocalRejectionMessage);
        Assert.Null(response.ErrorMessage);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
        Assert.Empty(response.Text);
        Assert.Empty(response.ToolCalls);
        Assert.False(response.IsSuccess);
        Assert.False(response.IsSuccessfulTerminal);
        Assert.Same(diagnostics, response.Diagnostics);
    }

    [Fact]
    public void CreateIncomplete_PreservesPartialAndNonStopFinish()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateIncomplete(
            "partial",
            Ra2AiStreamFinishKind.Length,
            safeErrorMessage: "stream incomplete");

        Assert.Equal(Ra2AiResponseKind.Incomplete, response.Kind);
        Assert.Equal("partial", response.Text);
        Assert.Equal("stream incomplete", response.ErrorMessage);
        Assert.Equal(Ra2AiStreamFinishKind.Length, response.FinishKind);
        Assert.Equal(Ra2AiFailureKind.None, response.FailureKind);
    }

    [Fact]
    public void Factories_PreserveRequestDiagnosticsByIdentity()
    {
        Ra2AiRequestDiagnostics diagnostics = new(
            "0123456789abcdef0123456789abcdef",
            "deepseek-v4-flash",
            100,
            TimeSpan.FromMilliseconds(2),
            TimeSpan.FromMilliseconds(3),
            TimeSpan.FromMilliseconds(4),
            1,
            5,
            200);

        Ra2AiResponse response = Ra2AiResponse.CreateSuccess("answer", diagnostics);

        Assert.Same(diagnostics, response.Diagnostics);
    }

    [Fact]
    public void Factories_RejectContradictoryOrIncompleteInputs()
    {
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateSuccess(string.Empty));
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateTimeout(
            string.Empty,
            Ra2AiFailureKind.RateLimited));
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateProviderFailure(
            Ra2AiFailureKind.None,
            "error"));
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateProviderFailure(
            Ra2AiFailureKind.ProtocolError,
            string.Empty));
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateProviderFailure(
            Ra2AiFailureKind.ProtocolError,
            "error",
            Ra2AiStreamFinishKind.Stop));
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateLocalRejection(string.Empty));
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateIncomplete(
            string.Empty,
            Ra2AiStreamFinishKind.Unknown));
        Assert.Throws<ArgumentException>(() => Ra2AiResponse.CreateIncomplete(
            "partial",
            Ra2AiStreamFinishKind.Stop));
    }
}
