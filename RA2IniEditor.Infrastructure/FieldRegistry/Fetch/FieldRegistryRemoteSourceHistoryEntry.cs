using System.Text.Json.Serialization;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRemoteSourceHistoryEntry
{
    [JsonConstructor]
    public FieldRegistryRemoteSourceHistoryEntry(
        string url,
        string resolvedUrl,
        string sourceName,
        DateTimeOffset fetchedAtUtc,
        int byteCount,
        string? cachedText)
    {
        Url = string.IsNullOrWhiteSpace(url)
            ? throw new ArgumentException("URL cannot be empty.", nameof(url))
            : url;
        ResolvedUrl = string.IsNullOrWhiteSpace(resolvedUrl)
            ? throw new ArgumentException("Resolved URL cannot be empty.", nameof(resolvedUrl))
            : resolvedUrl;
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "fetched-field-doc" : sourceName;
        FetchedAtUtc = fetchedAtUtc.ToUniversalTime();
        ByteCount = byteCount >= 0
            ? byteCount
            : throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount, "Byte count cannot be negative.");
        CachedText = cachedText;
    }

    public string Url { get; }

    public string ResolvedUrl { get; }

    public string SourceName { get; }

    public DateTimeOffset FetchedAtUtc { get; }

    public int ByteCount { get; }

    public string? CachedText { get; }
}
