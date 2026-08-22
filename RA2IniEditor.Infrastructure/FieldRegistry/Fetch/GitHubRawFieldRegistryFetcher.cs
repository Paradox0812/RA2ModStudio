using System.Net;
using System.Text;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class GitHubRawFieldRegistryFetcher : IFieldRegistryRawFetcher
{
    private static readonly HashSet<string> AllowedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "text/markdown",
        "text/x-markdown",
        "application/json",
        "application/octet-stream"
    };

    private readonly HttpClient _httpClient;
    private readonly IFieldRegistryRawUrlResolver _urlResolver;

    public GitHubRawFieldRegistryFetcher()
        : this(CreateDefaultHttpClient(), new GitHubRawUrlResolver())
    {
    }

    public GitHubRawFieldRegistryFetcher(HttpClient httpClient)
        : this(httpClient, new GitHubRawUrlResolver())
    {
    }

    public GitHubRawFieldRegistryFetcher(
        HttpClient httpClient,
        IFieldRegistryRawUrlResolver urlResolver)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _urlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
    }

    public async Task<FieldRegistryRawFetchResult> FetchAsync(
        FieldRegistryRawFetchRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_urlResolver.TryResolve(request.Url, out string resolvedUrl, out string errorMessage))
            throw new InvalidOperationException(errorMessage);

        using HttpRequestMessage httpRequest = new(HttpMethod.Get, resolvedUrl);
        using HttpResponseMessage response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"Fetch failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");

        if (!IsSupportedContentType(response, resolvedUrl))
            throw new InvalidOperationException($"Unsupported content type '{response.Content.Headers.ContentType?.MediaType ?? "unknown"}'.");

        byte[] bytes = await ReadLimitedBytesAsync(response.Content, request.MaxBytes, cancellationToken).ConfigureAwait(false);
        string text = DecodeText(bytes, response);
        string sourceName = ResolveSourceName(resolvedUrl);
        return new FieldRegistryRawFetchResult(request.Url, resolvedUrl, sourceName, text, bytes.Length);
    }

    private static HttpClient CreateDefaultHttpClient()
        => new()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

    private static bool IsSupportedContentType(HttpResponseMessage response, string resolvedUrl)
    {
        string? mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.IsNullOrWhiteSpace(mediaType))
            return AllowedMediaTypes.Contains(mediaType);

        string extension = Path.GetExtension(new Uri(resolvedUrl).AbsolutePath);
        return extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".ini", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<byte[]> ReadLimitedBytesAsync(
        HttpContent content,
        int maxBytes,
        CancellationToken cancellationToken)
    {
        await using Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using MemoryStream buffer = new();
        byte[] chunk = new byte[8192];
        while (true)
        {
            int read = await stream.ReadAsync(chunk, cancellationToken).ConfigureAwait(false);
            if (read == 0)
                break;

            if (buffer.Length + read > maxBytes)
                throw new InvalidOperationException($"Fetched content exceeds the {maxBytes} byte limit.");

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static string DecodeText(byte[] bytes, HttpResponseMessage response)
    {
        string? charset = response.Content.Headers.ContentType?.CharSet;
        Encoding encoding = string.IsNullOrWhiteSpace(charset)
            ? Encoding.UTF8
            : Encoding.GetEncoding(charset);
        return encoding.GetString(bytes);
    }

    private static string ResolveSourceName(string resolvedUrl)
    {
        string fileName = Path.GetFileName(new Uri(resolvedUrl).AbsolutePath);
        return string.IsNullOrWhiteSpace(fileName)
            ? "fetched-field-doc"
            : Uri.UnescapeDataString(fileName);
    }
}
