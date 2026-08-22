namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRawFetchResult
{
    public FieldRegistryRawFetchResult(
        string url,
        string resolvedUrl,
        string sourceName,
        string text,
        int byteCount)
    {
        Url = string.IsNullOrWhiteSpace(url)
            ? throw new ArgumentException("Original URL cannot be empty.", nameof(url))
            : url;
        ResolvedUrl = string.IsNullOrWhiteSpace(resolvedUrl)
            ? throw new ArgumentException("Resolved URL cannot be empty.", nameof(resolvedUrl))
            : resolvedUrl;
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "fetched-field-doc" : sourceName;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ByteCount = byteCount >= 0
            ? byteCount
            : throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount, "Byte count cannot be negative.");
    }

    public string Url { get; }

    public string ResolvedUrl { get; }

    public string SourceName { get; }

    public string Text { get; }

    public int ByteCount { get; }
}
