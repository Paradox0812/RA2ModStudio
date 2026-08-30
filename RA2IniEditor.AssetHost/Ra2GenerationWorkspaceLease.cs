using System.Collections.ObjectModel;

namespace RA2IniEditor.AssetHost;

internal sealed class Ra2GenerationWorkspaceLease : IRa2GenerationWorkspaceLease
{
    private readonly string _runRoot;
    private readonly string _completedRoot;
    private readonly ReadOnlyCollection<Ra2GenerationCandidate> _candidates;
    private readonly IReadOnlyDictionary<(string CandidateId, string ArtifactId), string> _artifactPaths;
    private readonly HashSet<FileStream> _openStreams = new();
    private FileStream? _activeLock;
    private bool _disposed;

    internal Ra2GenerationWorkspaceLease(
        string runRoot,
        string completedRoot,
        IEnumerable<Ra2GenerationCandidate> candidates,
        IReadOnlyDictionary<(string CandidateId, string ArtifactId), string> artifactPaths,
        FileStream activeLock)
    {
        _runRoot = runRoot;
        _completedRoot = completedRoot;
        _candidates = Array.AsReadOnly(candidates.ToArray());
        _artifactPaths = new ReadOnlyDictionary<(string CandidateId, string ArtifactId), string>(
            new Dictionary<(string CandidateId, string ArtifactId), string>(artifactPaths));
        _activeLock = activeLock;
    }

    public IReadOnlyList<Ra2GenerationCandidate> Candidates => _candidates;

    public ValueTask<Stream> OpenArtifactReadAsync(
        string candidateId,
        string artifactId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_openStreams)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_artifactPaths.TryGetValue((candidateId, artifactId), out string? relativePath))
            {
                throw new ArgumentOutOfRangeException(nameof(artifactId), "The artifact does not belong to this lease.");
            }

            string path = Path.GetFullPath(Path.Combine(_completedRoot, relativePath));
            string root = Path.GetFullPath(_completedRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!path.StartsWith(root, comparison))
            {
                throw new InvalidOperationException("The leased artifact path is invalid.");
            }

            var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            _openStreams.Add(stream);
            return ValueTask.FromResult<Stream>(stream);
        }
    }

    public async ValueTask DisposeAsync()
    {
        FileStream[] streams;
        FileStream? activeLock;
        lock (_openStreams)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            streams = _openStreams.ToArray();
            _openStreams.Clear();
            activeLock = _activeLock;
            _activeLock = null;
        }

        foreach (FileStream stream in streams)
        {
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        if (activeLock is not null)
        {
            await activeLock.DisposeAsync().ConfigureAwait(false);
        }

        try
        {
            if (Directory.Exists(_runRoot))
            {
                Directory.Delete(_runRoot, recursive: true);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new Ra2GenerationWorkspaceCleanupException("The generation workspace was quarantined because cleanup failed.");
        }
    }
}
