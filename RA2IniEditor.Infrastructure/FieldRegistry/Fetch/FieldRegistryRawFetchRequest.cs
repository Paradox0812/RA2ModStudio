namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRawFetchRequest
{
    public const int DefaultMaxBytes = 512 * 1024;

    public FieldRegistryRawFetchRequest(string url, int maxBytes = DefaultMaxBytes)
    {
        Url = string.IsNullOrWhiteSpace(url)
            ? throw new ArgumentException("Fetch URL cannot be empty.", nameof(url))
            : url.Trim();
        MaxBytes = maxBytes > 0
            ? maxBytes
            : throw new ArgumentOutOfRangeException(nameof(maxBytes), maxBytes, "Max bytes must be greater than zero.");
    }

    public string Url { get; }

    public int MaxBytes { get; }
}
