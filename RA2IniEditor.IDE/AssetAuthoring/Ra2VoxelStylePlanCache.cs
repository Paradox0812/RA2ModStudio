using System.IO;
using System.Text;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal sealed class Ra2VoxelStylePlanCache
{
    internal const long DefaultMaximumBytes = 64L * 1024 * 1024;
    internal const int DefaultMaximumEntries = 256;
    internal const int MaximumEntryBytes = 1024 * 1024;

    private readonly string _root;
    private readonly long _maximumBytes;
    private readonly int _maximumEntries;

    internal Ra2VoxelStylePlanCache(
        string root,
        long maximumBytes = DefaultMaximumBytes,
        int maximumEntries = DefaultMaximumEntries)
    {
        if (string.IsNullOrWhiteSpace(root) || !Path.IsPathFullyQualified(root))
            throw new ArgumentException("A fully-qualified voxel style cache root is required.", nameof(root));
        if (maximumBytes is < MaximumEntryBytes or > DefaultMaximumBytes)
            throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        if (maximumEntries is < 1 or > DefaultMaximumEntries)
            throw new ArgumentOutOfRangeException(nameof(maximumEntries));
        _root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        _maximumBytes = maximumBytes;
        _maximumEntries = maximumEntries;
    }

    internal static string DefaultRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RA2IniEditor",
        "AssetStyleCache",
        "v1");

    internal bool TryRead(string cacheKey, out string json)
    {
        ValidateKey(cacheKey);
        json = string.Empty;
        try
        {
            if (!Directory.Exists(_root) || IsReparsePoint(_root))
                return false;
            string path = Path.Combine(_root, cacheKey + ".json");
            FileInfo file = new(path);
            if (!file.Exists || (file.Attributes & FileAttributes.ReparsePoint) != 0 ||
                file.Length is < 2 or > MaximumEntryBytes)
            {
                return false;
            }
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using StreamReader reader = new(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true);
            json = reader.ReadToEnd();
            return json.Length > 0 && json.IndexOf('\0') < 0;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DecoderFallbackException)
        {
            json = string.Empty;
            return false;
        }
    }

    internal void Store(string cacheKey, string json)
    {
        ValidateKey(cacheKey);
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = new UTF8Encoding(false).GetBytes(json);
        if (bytes.Length is < 2 or > MaximumEntryBytes)
            throw new ArgumentOutOfRangeException(nameof(json));

        try
        {
            Directory.CreateDirectory(_root);
            if (IsReparsePoint(_root))
                return;
            string destination = Path.Combine(_root, cacheKey + ".json");
            if (File.Exists(destination))
                return;
            string temporary = Path.Combine(_root, "." + cacheKey + "." + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using (FileStream stream = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes);
                    stream.Flush(flushToDisk: true);
                }
                try
                {
                    File.Move(temporary, destination, overwrite: false);
                }
                catch (IOException) when (File.Exists(destination))
                {
                }
            }
            finally
            {
                TryDelete(temporary);
            }
            EnforceLimits();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Cache is derived performance data; cache failure never changes authoring semantics.
        }
    }

    private void EnforceLimits()
    {
        FileInfo[] entries = new DirectoryInfo(_root).EnumerateFiles("*.json", SearchOption.TopDirectoryOnly)
            .Where(file => (file.Attributes & FileAttributes.ReparsePoint) == 0)
            .OrderBy(file => file.CreationTimeUtc)
            .ThenBy(file => file.Name, StringComparer.Ordinal)
            .ToArray();
        long bytes = entries.Sum(file => file.Length);
        int index = 0;
        while ((entries.Length - index > _maximumEntries || bytes > _maximumBytes) && index < entries.Length)
        {
            FileInfo candidate = entries[index++];
            long length = candidate.Length;
            TryDelete(candidate.FullName);
            if (!candidate.Exists)
                bytes -= length;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0)
                File.Delete(path);
        }
        catch
        {
        }
    }

    private static bool IsReparsePoint(string path)
        => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;

    private static void ValidateKey(string cacheKey)
    {
        if (cacheKey.Length != 64 || cacheKey.Any(character => !char.IsAsciiHexDigit(character)))
            throw new ArgumentException("A voxel style cache key must be a SHA-256 value.", nameof(cacheKey));
    }
}
