using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RA2IniEditor.AssetProviders.TencentHy3D;

internal sealed class TencentHy3DClient
{
    private readonly HttpClient _httpClient;
    private readonly Uri _origin;
    private readonly string _apiKey;
    private readonly TimeSpan _pollInterval;
    private int _submitCount;

    internal TencentHy3DClient(HttpClient httpClient, Uri origin, string apiKey, TimeSpan? pollInterval = null)
    {
        _httpClient = httpClient;
        _origin = origin;
        _apiKey = apiKey;
        _pollInterval = pollInterval ?? TimeSpan.FromSeconds(3);
    }

    internal async Task<TencentHy3DCompletedJob> GenerateAsync(
        TencentHy3DHostRequest request,
        Action<string, double?, string> progress,
        CancellationToken cancellationToken)
    {
        if (Interlocked.Increment(ref _submitCount) != 1)
        {
            throw new TencentHy3DProviderException("A generation run may submit at most one remote job.");
        }

        byte[] input = await File.ReadAllBytesAsync(request.InputPath, cancellationToken).ConfigureAwait(false);
        if (input.Length == 0 || input.Length > TencentHy3DConstants.MaximumImageBytes)
        {
            throw new TencentHy3DRequestException("The reference image exceeds the certified provider limit.");
        }

        var submitBody = new
        {
            Model = "3.1",
            ImageBase64 = Convert.ToBase64String(input),
            GenerateType = "Geometry",
            EnablePBR = false
        };
        progress("submit", 10, "Submitting one shape-only job.");
        JsonElement submit = await PostJsonAsync(TencentHy3DConstants.SubmitPath, submitBody, cancellationToken).ConfigureAwait(false);
        string jobId = RequireString(submit, "JobId", 128);
        string submitRequestId = OptionalString(submit, "RequestId", 128);

        string status = "WAIT";
        JsonElement terminal = default;
        int pollCount = 0;
        while (status is "WAIT" or "RUN")
        {
            await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
            JsonElement query = await PostJsonAsync(TencentHy3DConstants.QueryPath, new { JobId = jobId }, cancellationToken).ConfigureAwait(false);
            status = RequireString(query, "Status", 16).ToUpperInvariant();
            pollCount++;
            progress("query", Math.Min(90, 20 + pollCount * 5), status == "WAIT" ? "Waiting for provider capacity." : "Generating geometry.");
            if (status == "FAIL")
            {
                string code = OptionalString(query, "ErrorCode", 128);
                throw new TencentHy3DProviderException(string.IsNullOrWhiteSpace(code)
                    ? "Tencent Hunyuan 3D reported a failed job."
                    : $"Tencent Hunyuan 3D reported failure code {SanitizeToken(code)}.");
            }

            if (status is not ("WAIT" or "RUN" or "DONE"))
            {
                throw new TencentHy3DProviderException("Tencent Hunyuan 3D returned an unsupported job state.");
            }

            terminal = query.Clone();
        }

        if (status != "DONE")
        {
            throw new TencentHy3DProviderException("Tencent Hunyuan 3D did not complete the job.");
        }

        TencentHy3DRemoteArtifact mesh = SelectArtifact(terminal, "GLB");
        string queryRequestId = OptionalString(terminal, "RequestId", 128);
        double? credits = OptionalDouble(terminal, "ResultCreditConsumed");
        string creditDetails = OptionalString(terminal, "ResultCreditDetails", 1024);
        return new TencentHy3DCompletedJob(
            jobId, submitRequestId, queryRequestId, status, credits, creditDetails, pollCount, mesh,
            TrySelectPreview(terminal));
    }

    internal async Task DownloadAsync(Uri initialUri, string destination, long maximumBytes, CancellationToken cancellationToken)
    {
        Uri current = initialUri;
        for (int redirect = 0; redirect <= TencentHy3DConstants.MaximumRedirects; redirect++)
        {
            EnsureHttps(current);
            using var request = CreateRequest(HttpMethod.Get, current);
            using HttpResponseMessage response = await _httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (IsRedirect(response.StatusCode))
            {
                if (redirect == TencentHy3DConstants.MaximumRedirects || response.Headers.Location is null)
                {
                    throw new TencentHy3DProviderException("The artifact download redirect chain is invalid.");
                }

                current = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location
                    : new Uri(current, response.Headers.Location);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new TencentHy3DProviderException("The provider artifact download failed.");
            }

            if (response.Content.Headers.ContentLength is long declared && (declared <= 0 || declared > maximumBytes))
            {
                throw new TencentHy3DResourceException("The provider artifact exceeds the configured size limit.");
            }

            await using Stream input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            byte[] buffer = new byte[81920];
            long total = 0;
            while (true)
            {
                int read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                total += read;
                if (total > maximumBytes)
                {
                    throw new TencentHy3DResourceException("The provider artifact exceeds the configured size limit.");
                }

                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            }

            if (total == 0)
            {
                throw new TencentHy3DProviderException("The provider artifact is empty.");
            }

            return;
        }
    }

    private async Task<JsonElement> PostJsonAsync(string relativePath, object body, CancellationToken cancellationToken)
    {
        Uri endpoint = new(_origin, relativePath);
        using var request = CreateRequest(HttpMethod.Post, endpoint);
        request.Headers.TryAddWithoutValidation("Authorization", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        byte[] content = await ReadBoundedAsync(response.Content, TencentHy3DConstants.MaximumJsonBytes, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            string errorCode = TryExtractErrorCode(content);
            throw new TencentHy3DProviderException(errorCode.Length == 0
                ? $"Tencent Hunyuan 3D returned HTTP {(int)response.StatusCode}."
                : $"Tencent Hunyuan 3D returned HTTP {(int)response.StatusCode} with code {errorCode}.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(content, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 24
            });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new TencentHy3DProviderException("Tencent Hunyuan 3D returned an invalid JSON object.");
            }

            if (root.TryGetProperty("Response", out JsonElement wrapped))
            {
                root = wrapped;
            }

            if (root.TryGetProperty("Error", out JsonElement error) && error.ValueKind == JsonValueKind.Object)
            {
                string code = OptionalString(error, "Code", 128);
                throw new TencentHy3DProviderException(string.IsNullOrWhiteSpace(code)
                    ? "Tencent Hunyuan 3D rejected the request."
                    : $"Tencent Hunyuan 3D rejected the request with code {SanitizeToken(code)}.");
            }

            return root.Clone();
        }
        catch (JsonException)
        {
            throw new TencentHy3DProviderException("Tencent Hunyuan 3D returned malformed JSON.");
        }
    }

    private static TencentHy3DRemoteArtifact SelectArtifact(JsonElement root, string type)
    {
        if (!root.TryGetProperty("ResultFile3Ds", out JsonElement files) || files.ValueKind != JsonValueKind.Array)
        {
            throw new TencentHy3DOutputMissingException("Tencent Hunyuan 3D returned no result files.");
        }

        var matches = new List<TencentHy3DRemoteArtifact>();
        foreach (JsonElement file in files.EnumerateArray())
        {
            if (file.ValueKind != JsonValueKind.Object ||
                !string.Equals(OptionalString(file, "Type", 16), type, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string url = RequireString(file, "Url", 4096);
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                throw new TencentHy3DProviderException("Tencent Hunyuan 3D returned an invalid artifact URL.");
            }

            EnsureHttps(uri);
            matches.Add(new TencentHy3DRemoteArtifact(type, uri));
        }

        return matches.Count == 1
            ? matches[0]
            : throw new TencentHy3DOutputMissingException("Tencent Hunyuan 3D did not return exactly one GLB result.");
    }

    private static TencentHy3DRemoteArtifact? TrySelectPreview(JsonElement root)
    {
        if (!root.TryGetProperty("ResultFile3Ds", out JsonElement files) || files.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (JsonElement file in files.EnumerateArray())
        {
            string url = OptionalString(file, "PreviewImageUrl", 4096);
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttps)
            {
                return new TencentHy3DRemoteArtifact("PNG", uri);
            }
        }

        return null;
    }

    private static async Task<byte[]> ReadBoundedAsync(HttpContent content, int maximumBytes, CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength is long length && length > maximumBytes)
        {
            throw new TencentHy3DResourceException("Tencent Hunyuan 3D returned an oversized JSON response.");
        }

        await using Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var memory = new MemoryStream();
        byte[] buffer = new byte[16 * 1024];
        while (true)
        {
            int read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            if (memory.Length + read > maximumBytes)
            {
                throw new TencentHy3DResourceException("Tencent Hunyuan 3D returned an oversized JSON response.");
            }

            memory.Write(buffer, 0, read);
        }

        return memory.ToArray();
    }

    private static string RequireString(JsonElement root, string name, int maximumLength)
    {
        string value = OptionalString(root, name, maximumLength);
        return value.Length == 0
            ? throw new TencentHy3DProviderException("Tencent Hunyuan 3D omitted required response data.")
            : value;
    }

    private static string OptionalString(JsonElement root, string name, int maximumLength)
    {
        if (!root.TryGetProperty(name, out JsonElement value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new TencentHy3DProviderException("Tencent Hunyuan 3D returned invalid response data.");
        }

        string text = value.GetString() ?? string.Empty;
        return text.Length <= maximumLength
            ? text
            : throw new TencentHy3DProviderException("Tencent Hunyuan 3D returned oversized response data.");
    }

    private static double? OptionalDouble(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out JsonElement value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (!value.TryGetDouble(out double parsed) || !double.IsFinite(parsed) || parsed < 0)
        {
            throw new TencentHy3DProviderException("Tencent Hunyuan 3D returned invalid credit evidence.");
        }

        return parsed;
    }

    private static void EnsureHttps(Uri uri)
    {
        if (!uri.IsAbsoluteUri || uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new TencentHy3DProviderException("Only HTTPS provider URLs are accepted.");
        }
    }

    private static bool IsRedirect(HttpStatusCode code) => code is
        HttpStatusCode.Moved or HttpStatusCode.Redirect or HttpStatusCode.RedirectMethod or
        HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    private static string SanitizeToken(string value)
    {
        string sanitized = new(value.Where(character => char.IsLetterOrDigit(character) || character is '.' or '_' or '-').Take(128).ToArray());
        return sanitized.Length == 0 ? "unknown" : sanitized;
    }

    private static string TryExtractErrorCode(ReadOnlySpan<byte> content)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(content.ToArray(), new JsonDocumentOptions { MaxDepth = 16 });
            JsonElement root = document.RootElement;
            if (root.TryGetProperty("Response", out JsonElement response))
            {
                root = response;
            }

            if (root.TryGetProperty("Error", out JsonElement error) && error.ValueKind == JsonValueKind.Object)
            {
                string errorCode = OptionalString(error, "Code", 128);
                return errorCode.Length == 0 ? string.Empty : SanitizeToken(errorCode);
            }

            if (root.TryGetProperty("error", out error) && error.ValueKind == JsonValueKind.Object)
            {
                string errorCode = OptionalString(error, "code", 128);
                return errorCode.Length == 0 ? string.Empty : SanitizeToken(errorCode);
            }

            string direct = OptionalString(root, "ErrorCode", 128);
            if (direct.Length > 0)
            {
                return SanitizeToken(direct);
            }

            if (root.TryGetProperty("Code", out JsonElement numericCode) &&
                numericCode.ValueKind == JsonValueKind.Number && numericCode.TryGetInt64(out long numericValue))
            {
                return numericValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }
        catch (Exception exception) when (exception is JsonException or TencentHy3DProviderException)
        {
            return string.Empty;
        }
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, Uri uri) => new(method, uri)
    {
        Version = HttpVersion.Version11,
        VersionPolicy = HttpVersionPolicy.RequestVersionExact
    };
}

internal sealed record TencentHy3DCompletedJob(
    string JobId,
    string SubmitRequestId,
    string QueryRequestId,
    string Status,
    double? CreditsConsumed,
    string CreditDetails,
    int PollCount,
    TencentHy3DRemoteArtifact Mesh,
    TencentHy3DRemoteArtifact? Preview);

internal sealed record TencentHy3DRemoteArtifact(string Type, Uri Url);

internal class TencentHy3DProviderException : Exception
{
    internal TencentHy3DProviderException(string message) : base(message) { }
}

internal sealed class TencentHy3DOutputMissingException : TencentHy3DProviderException
{
    internal TencentHy3DOutputMissingException(string message) : base(message) { }
}

internal sealed class TencentHy3DResourceException : TencentHy3DProviderException
{
    internal TencentHy3DResourceException(string message) : base(message) { }
}
