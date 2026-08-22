namespace RA2IniEditor.IDE.AI;

/// <summary>描述 AI 请求未成功完成时的稳定内部原因。</summary>
internal enum Ra2AiFailureKind
{
    None = 0,
    MissingConfiguration,
    AuthenticationOrAuthorization,
    RateLimited,
    RequestRejected,
    ProviderRequestTimeout,
    ServiceUnavailable,
    NetworkOrProxy,
    TotalTimeout,
    StreamingIdleTimeout,
    ProtocolError,
    ResponseTooLarge,
    Unknown
}
