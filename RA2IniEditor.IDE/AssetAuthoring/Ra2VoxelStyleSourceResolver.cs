using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelStyleSourceScope
{
    BuiltIn = 0,
    ProjectRoot,
    Directory,
    RequestOverride
}

internal enum Ra2VoxelStyleSourceFailureKind
{
    None = 0,
    NoStyleSource,
    InvalidEncoding,
    SourceTooLarge,
    TooManySources,
    SourcePathOutsideProject,
    SourcePathRejected,
    AnalysisFailed
}

internal sealed record Ra2VoxelStyleSource(
    string ScopeId,
    Ra2VoxelStyleSourceScope Scope,
    string DisplayPath,
    string Text,
    string ContentHash);

internal sealed class Ra2VoxelStyleSourcePack
{
    internal const int CurrentSchemaVersion = 1;

    internal Ra2VoxelStyleSourcePack(IEnumerable<Ra2VoxelStyleSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        Ra2VoxelStyleSource[] array = sources.ToArray();
        if (array.Length is < 1 or > Ra2VoxelStyleSourceResolver.MaximumSourceCount)
            throw new ArgumentOutOfRangeException(nameof(sources));
        if (array.Any(source => source is null))
            throw new ArgumentException("Style sources cannot contain null entries.", nameof(sources));
        if (array.GroupBy(source => source.ScopeId, StringComparer.Ordinal).Any(group => group.Count() > 1))
            throw new ArgumentException("Style source scope identities must be unique.", nameof(sources));

        Sources = Array.AsReadOnly(array);
        TotalCharacters = array.Sum(source => source.Text.Length);
        PackHash = ComputeHash(array);
    }

    internal IReadOnlyList<Ra2VoxelStyleSource> Sources { get; }
    internal int TotalCharacters { get; }
    internal string PackHash { get; }

    private static string ComputeHash(IReadOnlyList<Ra2VoxelStyleSource> sources)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(CurrentSchemaVersion);
        writer.Write(sources.Count);
        foreach (Ra2VoxelStyleSource source in sources)
        {
            WriteString(writer, source.ScopeId);
            writer.Write((int)source.Scope);
            WriteString(writer, source.DisplayPath);
            WriteString(writer, source.ContentHash);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }
}

internal sealed record Ra2VoxelStyleSourceResolutionResult(
    Ra2VoxelStyleSourceFailureKind FailureKind,
    string Message,
    Ra2VoxelStyleSourcePack? SourcePack)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelStyleSourceFailureKind.None && SourcePack is not null;
}

internal static class Ra2VoxelStyleSourceResolver
{
    internal const string FileName = "VOXEL_STYLE.md";
    internal const int MaximumSourceCharacters = 32 * 1024;
    internal const int MaximumOverrideCharacters = 8 * 1024;
    internal const int MaximumSourcePackCharacters = 64 * 1024;
    internal const int MaximumSourceCount = 8;
    private const int MaximumSourceBytes = MaximumSourceCharacters * 4;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    internal static Ra2VoxelStyleSourceResolutionResult Resolve(
        string bundledDefaultPath,
        string projectRoot,
        string? targetDirectory = null,
        string? requestOverride = null)
    {
        if (string.IsNullOrWhiteSpace(bundledDefaultPath))
            throw new ArgumentException("A bundled style path is required.", nameof(bundledDefaultPath));
        if (string.IsNullOrWhiteSpace(projectRoot))
            throw new ArgumentException("A project root is required.", nameof(projectRoot));

        try
        {
            string root = NormalizeDirectory(projectRoot);
            if (!Directory.Exists(root) || IsReparsePoint(root))
                return Failure(Ra2VoxelStyleSourceFailureKind.SourcePathRejected, "The project style root is unavailable or unsafe.");

            string? target = null;
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                target = NormalizeDirectory(targetDirectory);
                if (!Directory.Exists(target) || !IsSameOrDescendant(target, root))
                    return Failure(Ra2VoxelStyleSourceFailureKind.SourcePathOutsideProject, "The target style directory is outside the project.");
            }

            List<Ra2VoxelStyleSource> sources = [];
            Ra2VoxelStyleSourceResolutionResult? addFailure = AddFile(
                sources,
                Path.GetFullPath(bundledDefaultPath),
                "built-in",
                Ra2VoxelStyleSourceScope.BuiltIn,
                "built-in/VOXEL_STYLE.md");
            if (addFailure is not null)
                return addFailure;

            foreach (string directory in EnumerateScopeDirectories(root, target))
            {
                if (IsReparsePoint(directory))
                    return Failure(Ra2VoxelStyleSourceFailureKind.SourcePathRejected, "A style scope directory is a reparse point.");

                string path = Path.Combine(directory, FileName);
                if (!File.Exists(path))
                    continue;
                string relative = Path.GetRelativePath(root, path).Replace('\\', '/');
                bool isRoot = string.Equals(directory, root, PathComparison);
                addFailure = AddFile(
                    sources,
                    path,
                    isRoot ? "project" : "directory:" + Path.GetDirectoryName(relative)!.Replace('\\', '/'),
                    isRoot ? Ra2VoxelStyleSourceScope.ProjectRoot : Ra2VoxelStyleSourceScope.Directory,
                    relative);
                if (addFailure is not null)
                    return addFailure;
            }

            if (!string.IsNullOrWhiteSpace(requestOverride))
            {
                string normalized = NormalizeText(requestOverride);
                if (normalized.Length > MaximumOverrideCharacters)
                    return Failure(Ra2VoxelStyleSourceFailureKind.SourceTooLarge, "The per-request style override exceeds its limit.");
                sources.Add(CreateSource(
                    "request",
                    Ra2VoxelStyleSourceScope.RequestOverride,
                    "request override",
                    normalized));
            }

            if (sources.Count > MaximumSourceCount)
                return Failure(Ra2VoxelStyleSourceFailureKind.TooManySources, "The resolved style source count exceeds its limit.");
            if (sources.Sum(source => source.Text.Length) > MaximumSourcePackCharacters)
                return Failure(Ra2VoxelStyleSourceFailureKind.SourceTooLarge, "The resolved style source pack exceeds its limit.");

            return new(Ra2VoxelStyleSourceFailureKind.None, string.Empty, new Ra2VoxelStyleSourcePack(sources));
        }
        catch (DecoderFallbackException)
        {
            return Failure(Ra2VoxelStyleSourceFailureKind.InvalidEncoding, "A voxel style source is not valid UTF-8.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return Failure(Ra2VoxelStyleSourceFailureKind.AnalysisFailed, "Voxel style sources could not be resolved safely.");
        }
    }

    private static IEnumerable<string> EnumerateScopeDirectories(string root, string? target)
    {
        yield return root;
        if (target is null || string.Equals(root, target, PathComparison))
            yield break;

        Stack<string> pending = new();
        string current = target;
        while (!string.Equals(current, root, PathComparison))
        {
            pending.Push(current);
            string? parent = Directory.GetParent(current)?.FullName;
            if (parent is null || !IsSameOrDescendant(parent, root))
                throw new IOException("The target style directory has no contained ancestor chain.");
            current = NormalizeDirectory(parent);
        }
        while (pending.Count > 0)
            yield return pending.Pop();
    }

    private static Ra2VoxelStyleSourceResolutionResult? AddFile(
        List<Ra2VoxelStyleSource> sources,
        string path,
        string scopeId,
        Ra2VoxelStyleSourceScope scope,
        string displayPath)
    {
        FileInfo file = new(path);
        if (!file.Exists)
            return Failure(Ra2VoxelStyleSourceFailureKind.NoStyleSource, "The bundled voxel style source is missing.");
        if ((file.Attributes & FileAttributes.ReparsePoint) != 0)
            return Failure(Ra2VoxelStyleSourceFailureKind.SourcePathRejected, "A voxel style source is a reparse point.");
        if (file.Length > MaximumSourceBytes)
            return Failure(Ra2VoxelStyleSourceFailureKind.SourceTooLarge, "A voxel style source exceeds its byte limit.");

        string text = StrictUtf8.GetString(File.ReadAllBytes(path));
        text = NormalizeText(text);
        if (text.Length is < 1 or > MaximumSourceCharacters)
            return Failure(Ra2VoxelStyleSourceFailureKind.SourceTooLarge, "A voxel style source is empty or exceeds its character limit.");
        sources.Add(CreateSource(scopeId, scope, displayPath, text));
        return null;
    }

    private static Ra2VoxelStyleSource CreateSource(
        string scopeId,
        Ra2VoxelStyleSourceScope scope,
        string displayPath,
        string text)
    {
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        return new(scopeId, scope, displayPath, text, hash);
    }

    private static string NormalizeText(string text)
    {
        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Trim();
        if (normalized.IndexOf('\0') >= 0)
            throw new DecoderFallbackException("Voxel style text contains a NUL character.");
        return normalized;
    }

    private static string NormalizeDirectory(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static bool IsSameOrDescendant(string candidate, string root)
    {
        if (string.Equals(candidate, root, PathComparison))
            return true;
        string prefix = root + Path.DirectorySeparatorChar;
        return candidate.StartsWith(prefix, PathComparison);
    }

    private static bool IsReparsePoint(string path)
        => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;

    private static Ra2VoxelStyleSourceResolutionResult Failure(
        Ra2VoxelStyleSourceFailureKind kind,
        string message)
        => new(kind, message, null);
}
