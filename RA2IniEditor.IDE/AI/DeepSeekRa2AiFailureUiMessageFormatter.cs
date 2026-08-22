namespace RA2IniEditor.IDE.AI;

/// <summary>将 DeepSeek 内部失败分类转换为不包含诊断细节的用户提示。</summary>
internal static class DeepSeekRa2AiFailureUiMessageFormatter
{
    private const string PartialContentSuffix = "以上内容可能不完整。";

    internal static string FormatStandaloneMessage(Ra2AiFailureKind failureKind)
        => failureKind switch
        {
            Ra2AiFailureKind.MissingConfiguration
                => "DeepSeek 配置不可用，请检查环境配置（API Key、服务地址、模型或超时）。",
            Ra2AiFailureKind.AuthenticationOrAuthorization
                => "DeepSeek 鉴权或访问授权失败，请检查 API Key 与账户权限。",
            Ra2AiFailureKind.RateLimited
                => "DeepSeek 请求受到限流，请稍后再试。",
            Ra2AiFailureKind.RequestRejected
                => "DeepSeek 未接受该请求，请检查服务地址、模型和请求配置。",
            Ra2AiFailureKind.ProviderRequestTimeout
                => "DeepSeek 返回了请求超时状态，请稍后再试。",
            Ra2AiFailureKind.ServiceUnavailable
                => "DeepSeek 服务暂时不可用，请稍后再试。",
            Ra2AiFailureKind.NetworkOrProxy
                => "无法连接 DeepSeek，请检查网络或代理。",
            Ra2AiFailureKind.TotalTimeout
                => "DeepSeek 请求超过本地总时限，已停止等待。",
            Ra2AiFailureKind.StreamingIdleTimeout
                => "DeepSeek 流式响应长时间没有新内容，已停止等待。",
            Ra2AiFailureKind.ProtocolError
                => "DeepSeek 返回了无法完整解析的响应。",
            Ra2AiFailureKind.ResponseTooLarge
                => "DeepSeek 回答超过本地安全上限，已停止接收。",
            _ => "DeepSeek 请求失败，请稍后再试。"
        };

    internal static string FormatPartialTerminalStatus(Ra2AiFailureKind failureKind)
        => $"{FormatStandaloneMessage(failureKind)} {PartialContentSuffix}";
}
