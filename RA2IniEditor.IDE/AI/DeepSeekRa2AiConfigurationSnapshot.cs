namespace RA2IniEditor.IDE.AI;

/// <summary>
/// DeepSeek 配置在单次读取后的安全状态。
/// </summary>
internal enum DeepSeekRa2AiConfigurationState
{
    Ready = 0,
    MissingApiKey,
    InvalidBaseUrl,
    InvalidTimeout,
    UnsupportedModel
}

internal enum DeepSeekRa2AiEndpointKind
{
    Official = 0,
    Custom,
    Invalid
}

/// <summary>
/// 单次请求使用的不可变配置事实；敏感配置只通过内部 options 交给 client。
/// </summary>
internal sealed class DeepSeekRa2AiConfigurationSnapshot
{
    internal DeepSeekRa2AiConfigurationSnapshot(
        DeepSeekRa2AiConfigurationState state,
        DeepSeekRa2AiModel model,
        DeepSeekRa2AiEndpointKind endpointKind,
        DeepSeekRa2AiClientOptions options)
    {
        State = state;
        Model = model;
        EndpointKind = endpointKind;
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public DeepSeekRa2AiConfigurationState State { get; }

    public DeepSeekRa2AiModel Model { get; }

    public DeepSeekRa2AiEndpointKind EndpointKind { get; }

    public bool UsesCustomEndpoint => EndpointKind != DeepSeekRa2AiEndpointKind.Official;

    internal DeepSeekRa2AiClientOptions Options { get; }
}
