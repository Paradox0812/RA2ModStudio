using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class DeepSeekRa2AiFailureUiMessageFormatterTests
{
    [Theory]
    [InlineData((int)Ra2AiFailureKind.MissingConfiguration, "DeepSeek 配置不可用，请检查环境配置（API Key、服务地址、模型或超时）。")]
    [InlineData((int)Ra2AiFailureKind.AuthenticationOrAuthorization, "DeepSeek 鉴权或访问授权失败，请检查 API Key 与账户权限。")]
    [InlineData((int)Ra2AiFailureKind.RateLimited, "DeepSeek 请求受到限流，请稍后再试。")]
    [InlineData((int)Ra2AiFailureKind.RequestRejected, "DeepSeek 未接受该请求，请检查服务地址、模型和请求配置。")]
    [InlineData((int)Ra2AiFailureKind.ProviderRequestTimeout, "DeepSeek 返回了请求超时状态，请稍后再试。")]
    [InlineData((int)Ra2AiFailureKind.ServiceUnavailable, "DeepSeek 服务暂时不可用，请稍后再试。")]
    [InlineData((int)Ra2AiFailureKind.NetworkOrProxy, "无法连接 DeepSeek，请检查网络或代理。")]
    [InlineData((int)Ra2AiFailureKind.TotalTimeout, "DeepSeek 请求超过本地总时限，已停止等待。")]
    [InlineData((int)Ra2AiFailureKind.StreamingIdleTimeout, "DeepSeek 流式响应长时间没有新内容，已停止等待。")]
    [InlineData((int)Ra2AiFailureKind.ProtocolError, "DeepSeek 返回了无法完整解析的响应。")]
    [InlineData((int)Ra2AiFailureKind.ResponseTooLarge, "DeepSeek 回答超过本地安全上限，已停止接收。")]
    [InlineData((int)Ra2AiFailureKind.None, "DeepSeek 请求失败，请稍后再试。")]
    [InlineData((int)Ra2AiFailureKind.Unknown, "DeepSeek 请求失败，请稍后再试。")]
    [InlineData(999, "DeepSeek 请求失败，请稍后再试。")]
    public void FormatStandaloneMessage_ReturnsFixedSafeText(
        int failureKind,
        string expected)
    {
        string result = DeepSeekRa2AiFailureUiMessageFormatter.FormatStandaloneMessage(
            (Ra2AiFailureKind)failureKind);

        Assert.Equal(expected, result);
        Assert.DoesNotContain("test-api-key-placeholder", result);
        Assert.DoesNotContain("raw prompt", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HTTP 401", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exception", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatPartialTerminalStatus_CoversEveryDeclaredKindAndUsesOneSuffix()
    {
        foreach (Ra2AiFailureKind failureKind in Enum.GetValues<Ra2AiFailureKind>())
        {
            string standalone = DeepSeekRa2AiFailureUiMessageFormatter.FormatStandaloneMessage(failureKind);
            string partial = DeepSeekRa2AiFailureUiMessageFormatter.FormatPartialTerminalStatus(failureKind);

            Assert.Equal($"{standalone} 以上内容可能不完整。", partial);
        }
    }
}
