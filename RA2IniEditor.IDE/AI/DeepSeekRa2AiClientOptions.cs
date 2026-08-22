namespace RA2IniEditor.IDE.AI;

internal sealed class DeepSeekRa2AiClientOptions
{
    private const string ChatCompletionsPath = "chat/completions";

    internal const int DefaultTimeoutSeconds = 120;
    internal const int DefaultStreamingIdleTimeoutSeconds = 60;
    internal const int DefaultMaxStreamingResponseCharacters = 1024 * 1024;
    internal const int DefaultMaxOutputTokens = 8192;
    internal const int MaximumOutputTokens = 32768;

    public string BaseUrl { get; init; } = "https://api.deepseek.com";

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; }
        = DeepSeekRa2AiModelCatalog.GetApiModelId(DeepSeekRa2AiModelCatalog.Default);

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(DefaultTimeoutSeconds);

    public TimeSpan StreamingIdleTimeout { get; init; }
        = TimeSpan.FromSeconds(DefaultStreamingIdleTimeoutSeconds);

    public int MaxStreamingResponseCharacters { get; init; }
        = DefaultMaxStreamingResponseCharacters;

    public double Temperature { get; init; } = 0.2;

    public int MaxOutputTokens { get; init; } = DefaultMaxOutputTokens;

    public override string ToString()
        => $"DeepSeekRa2AiClientOptions(BaseUrl=***, ApiKey=***, Model={Model}, Timeout={Timeout}, StreamingIdleTimeout={StreamingIdleTimeout}, MaxStreamingResponseCharacters={MaxStreamingResponseCharacters}, Temperature={Temperature:0.###}, MaxOutputTokens={MaxOutputTokens})";

    internal bool TryValidate(out Uri? endpoint, out string errorMessage)
    {
        endpoint = null;
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            errorMessage = "DeepSeek API key is missing.";
            return false;
        }

        if (!TryResolveChatCompletionsEndpoint(BaseUrl, out Uri? resolvedEndpoint))
        {
            errorMessage = "DeepSeek endpoint is not configured.";
            return false;
        }

        if (!IsSupportedModelId(Model))
        {
            errorMessage = "DeepSeek model is not configured.";
            return false;
        }

        if (Timeout <= TimeSpan.Zero)
        {
            errorMessage = "DeepSeek timeout must be greater than zero.";
            return false;
        }

        if (StreamingIdleTimeout <= TimeSpan.Zero)
        {
            errorMessage = "DeepSeek streaming idle timeout must be greater than zero.";
            return false;
        }

        if (MaxStreamingResponseCharacters <= 0)
        {
            errorMessage = "DeepSeek streaming response limit must be greater than zero.";
            return false;
        }

        if (!double.IsFinite(Temperature) || Temperature < 0 || Temperature > 2)
        {
            errorMessage = "DeepSeek temperature is outside the allowed range.";
            return false;
        }

        if (MaxOutputTokens <= 0 || MaxOutputTokens > MaximumOutputTokens)
        {
            errorMessage = "DeepSeek output token limit is outside the allowed range.";
            return false;
        }

        errorMessage = string.Empty;
        endpoint = resolvedEndpoint;
        return true;
    }

    internal static bool TryResolveChatCompletionsEndpoint(string value, out Uri? endpoint)
    {
        endpoint = null;
        if (!TryValidateBaseUrl(value, out Uri? baseUri))
            return false;

        endpoint = NormalizeChatCompletionsEndpoint(baseUri!);
        return true;
    }

    internal static DeepSeekRa2AiEndpointKind ClassifyEndpoint(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri ||
            !string.IsNullOrEmpty(endpoint.UserInfo) ||
            !string.IsNullOrEmpty(endpoint.Query) ||
            !string.IsNullOrEmpty(endpoint.Fragment))
        {
            return DeepSeekRa2AiEndpointKind.Invalid;
        }

        bool isOfficial = string.Equals(
                endpoint.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(endpoint.Host, "api.deepseek.com", StringComparison.OrdinalIgnoreCase) &&
            endpoint.IsDefaultPort &&
            string.Equals(
                endpoint.AbsolutePath.TrimEnd('/'),
                "/chat/completions",
                StringComparison.OrdinalIgnoreCase);
        return isOfficial
            ? DeepSeekRa2AiEndpointKind.Official
            : DeepSeekRa2AiEndpointKind.Custom;
    }

    internal static bool TryValidateBaseUrl(string value, out Uri? baseUri)
    {
        baseUri = null;
        if (string.IsNullOrWhiteSpace(value) ||
            !Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? candidate) ||
            !string.IsNullOrEmpty(candidate.UserInfo) ||
            !string.IsNullOrEmpty(candidate.Query) ||
            !string.IsNullOrEmpty(candidate.Fragment))
        {
            return false;
        }

        bool isHttps = string.Equals(candidate.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        bool isAllowedLoopbackHttp = string.Equals(
                candidate.Scheme,
                Uri.UriSchemeHttp,
                StringComparison.OrdinalIgnoreCase)
            && IsAllowedLoopbackHost(candidate.Host);
        if (!isHttps && !isAllowedLoopbackHttp)
            return false;

        baseUri = candidate;
        return true;
    }

    internal static bool IsSupportedModelId(string value)
        => DeepSeekRa2AiModelCatalog.Options.Any(option =>
            string.Equals(option.ApiModelId, value?.Trim(), StringComparison.Ordinal));

    private static Uri NormalizeChatCompletionsEndpoint(Uri baseUri)
    {
        string path = baseUri.AbsolutePath.TrimEnd('/');
        if (path.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            return baseUri;
        }

        UriBuilder builder = new(baseUri)
        {
            Path = string.IsNullOrWhiteSpace(path) || path == "/"
                ? ChatCompletionsPath
                : $"{path.TrimStart('/')}/{ChatCompletionsPath}"
        };
        return builder.Uri;
    }

    private static bool IsAllowedLoopbackHost(string host)
    {
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        string normalizedHost = host.Trim('[', ']');
        return System.Net.IPAddress.TryParse(normalizedHost, out System.Net.IPAddress? address)
            && (address.Equals(System.Net.IPAddress.Loopback)
                || address.Equals(System.Net.IPAddress.IPv6Loopback));
    }
}
