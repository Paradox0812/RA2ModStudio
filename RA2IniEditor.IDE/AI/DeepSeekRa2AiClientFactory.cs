using System.Net.Http;

namespace RA2IniEditor.IDE.AI;

internal static class DeepSeekRa2AiClientFactory
{
    private static readonly HttpClient SharedHttpClient = new()
    {
        Timeout = System.Threading.Timeout.InfiniteTimeSpan
    };

    internal const string ApiKeyEnvironmentVariable = "DEEPSEEK_API_KEY";
    internal const string BaseUrlEnvironmentVariable = "DEEPSEEK_BASE_URL";
    internal const string TimeoutSecondsEnvironmentVariable = "DEEPSEEK_TIMEOUT_SECONDS";
    internal const string DefaultBaseUrl = "https://api.deepseek.com";
    internal static string DefaultModel
        => DeepSeekRa2AiModelCatalog.GetApiModelId(DeepSeekRa2AiModelCatalog.Default);
    internal const int DefaultTimeoutSeconds = DeepSeekRa2AiClientOptions.DefaultTimeoutSeconds;
    internal const int MinimumTimeoutSeconds = 10;
    internal const int MaximumTimeoutSeconds = 600;

    public static DeepSeekRa2AiClientOptions CreateOptionsFromEnvironment(
        DeepSeekRa2AiModel model = DeepSeekRa2AiModel.V4Flash)
        => CreateConfigurationSnapshot(model).Options;

    public static DeepSeekRa2AiConfigurationSnapshot CreateConfigurationSnapshot(
        DeepSeekRa2AiModel model = DeepSeekRa2AiModel.V4Flash)
    {
        string apiKey = ReadEnvironmentValue(ApiKeyEnvironmentVariable);
        string rawBaseUrl = ReadEnvironmentValue(BaseUrlEnvironmentVariable);
        string rawTimeoutSeconds = ReadEnvironmentValue(TimeoutSecondsEnvironmentVariable);
        bool isModelSupported = TryGetApiModelId(model, out string apiModelId);
        bool isBaseUrlValid = TryNormalizeBaseUrl(rawBaseUrl, out string normalizedBaseUrl);
        bool isTimeoutValid = TryNormalizeTimeout(rawTimeoutSeconds, out TimeSpan timeout);
        DeepSeekRa2AiEndpointKind endpointKind = isBaseUrlValid &&
            DeepSeekRa2AiClientOptions.TryResolveChatCompletionsEndpoint(
                normalizedBaseUrl,
                out Uri? resolvedEndpoint)
                ? DeepSeekRa2AiClientOptions.ClassifyEndpoint(resolvedEndpoint!)
                : DeepSeekRa2AiEndpointKind.Invalid;

        DeepSeekRa2AiClientOptions options = new()
        {
            ApiKey = apiKey,
            BaseUrl = normalizedBaseUrl,
            Model = apiModelId,
            Timeout = timeout
        };

        DeepSeekRa2AiConfigurationState state = !isModelSupported
            ? DeepSeekRa2AiConfigurationState.UnsupportedModel
            : string.IsNullOrWhiteSpace(apiKey)
                ? DeepSeekRa2AiConfigurationState.MissingApiKey
                : !isBaseUrlValid
                    ? DeepSeekRa2AiConfigurationState.InvalidBaseUrl
                    : !isTimeoutValid
                        ? DeepSeekRa2AiConfigurationState.InvalidTimeout
                        : DeepSeekRa2AiConfigurationState.Ready;

        return new DeepSeekRa2AiConfigurationSnapshot(
            state,
            model,
            endpointKind,
            options);
    }

    public static IRa2AiClient CreateClientFromEnvironment(
        DeepSeekRa2AiModel model = DeepSeekRa2AiModel.V4Flash)
        => CreateClient(CreateConfigurationSnapshot(model));

    public static IRa2AiClient CreateClient(DeepSeekRa2AiConfigurationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new DeepSeekRa2AiClient(snapshot.Options, SharedHttpClient);
    }

    private static string ReadEnvironmentValue(string variableName)
        => Environment.GetEnvironmentVariable(variableName)?.Trim() ?? string.Empty;

    private static bool TryGetApiModelId(DeepSeekRa2AiModel model, out string apiModelId)
    {
        try
        {
            apiModelId = DeepSeekRa2AiModelCatalog.GetApiModelId(model);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            apiModelId = string.Empty;
            return false;
        }
    }

    private static bool TryNormalizeBaseUrl(string value, out string normalizedValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            normalizedValue = DefaultBaseUrl;
            return true;
        }

        normalizedValue = value.Trim();
        return DeepSeekRa2AiClientOptions.TryValidateBaseUrl(normalizedValue, out _);
    }

    private static bool TryNormalizeTimeout(string value, out TimeSpan timeout)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
            return true;
        }

        if (int.TryParse(value, out int seconds) &&
            seconds >= MinimumTimeoutSeconds &&
            seconds <= MaximumTimeoutSeconds)
        {
            timeout = TimeSpan.FromSeconds(seconds);
            return true;
        }

        timeout = TimeSpan.Zero;
        return false;
    }
}
