using System.IO;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.Editing;

internal enum Ra2ProjectSnapshotCaptureFailureKind
{
    None = 0,
    InvalidProject,
    InvalidTarget,
    DuplicateTarget,
    ActiveEditorTextMismatch,
    ReadOnly,
    DocumentTooLarge,
    ReadFailure,
    InvalidRegistrySnapshot,
    ResourceLimitExceeded,
    UnexpectedFailure
}

internal sealed class Ra2ProjectSnapshotCaptureResult
{
    private Ra2ProjectSnapshotCaptureResult(
        Ra2AutomationProjectSnapshot? snapshot,
        Ra2ProjectSnapshotCaptureFailureKind failureKind,
        string? message)
    {
        Snapshot = snapshot;
        FailureKind = failureKind;
        Message = message;
    }

    public bool Succeeded => FailureKind == Ra2ProjectSnapshotCaptureFailureKind.None;
    public Ra2AutomationProjectSnapshot? Snapshot { get; }
    public Ra2ProjectSnapshotCaptureFailureKind FailureKind { get; }
    public string? Message { get; }

    public static Ra2ProjectSnapshotCaptureResult Success(Ra2AutomationProjectSnapshot snapshot)
        => new(snapshot ?? throw new ArgumentNullException(nameof(snapshot)), Ra2ProjectSnapshotCaptureFailureKind.None, null);

    public static Ra2ProjectSnapshotCaptureResult Failure(
        Ra2ProjectSnapshotCaptureFailureKind failureKind,
        string message)
    {
        if (failureKind == Ra2ProjectSnapshotCaptureFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A project snapshot failure message is required.", nameof(message));
        return new(null, failureKind, message);
    }
}

internal sealed record Ra2ProjectDocumentSessionReplacement(
    Ra2EditableDocumentSession Expected,
    Ra2EditableDocumentSession Replacement);

internal sealed class Ra2ProjectDocumentSessionStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, ReadonlyIniFileDescriptor> _members;
    private readonly IReadOnlyList<string> _memberFilePaths;
    private readonly Dictionary<string, Ra2EditableDocumentSession> _sessions;
    private readonly IIniFileStore _fileStore;
    private readonly IRa2EditableDocumentSessionService _sessionService;
    private readonly Ra2EditorEncodingMetadataAdapter _encodingAdapter;

    public Ra2ProjectDocumentSessionStore(
        ProjectOpenResult project,
        IIniFileStore fileStore,
        IRa2EditableDocumentSessionService sessionService,
        Ra2EditorEncodingMetadataAdapter encodingAdapter)
    {
        ArgumentNullException.ThrowIfNull(project);
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _encodingAdapter = encodingAdapter ?? throw new ArgumentNullException(nameof(encodingAdapter));

        ProjectRootPath = NormalizeRoot(project.ProjectFolderPath);
        _members = new Dictionary<string, ReadonlyIniFileDescriptor>(StringComparer.OrdinalIgnoreCase);
        _sessions = new Dictionary<string, Ra2EditableDocumentSession>(StringComparer.OrdinalIgnoreCase);
        List<string> memberFilePaths = new(project.Files.Count);
        foreach (ReadonlyIniFileDescriptor descriptor in project.Files)
        {
            string path = NormalizeMemberPath(descriptor.FilePath);
            if (!_members.TryAdd(path, descriptor))
                throw new ArgumentException("Project membership contains duplicate file paths.", nameof(project));
            memberFilePaths.Add(path);
        }
        _memberFilePaths = Array.AsReadOnly(memberFilePaths.ToArray());

        ProjectSessionId = Guid.NewGuid();
    }

    public Guid ProjectSessionId { get; }
    public long ProjectRevision { get; private set; }
    public string ProjectRootPath { get; }
    public IReadOnlyList<string> MemberFilePaths => _memberFilePaths;
    public string? ActiveFilePath { get; private set; }

    public int DirtyDocumentCount
    {
        get
        {
            lock (_gate)
                return _sessions.Values.Count(session => session.DocumentState.IsDirty);
        }
    }

    public bool HasDirtyDocuments => DirtyDocumentCount > 0;

    public bool TryActivate(string filePath, out Ra2EditableDocumentSession? session, out string? failureMessage)
    {
        lock (_gate)
        {
            session = null;
            if (!TryNormalizeKnownPath(filePath, out string? path, out failureMessage) ||
                !TryGetOrLoadSessionLocked(path!, out session, out failureMessage))
            {
                return false;
            }

            ActiveFilePath = path;
            return true;
        }
    }

    public bool TryGetSession(string filePath, out Ra2EditableDocumentSession? session)
    {
        lock (_gate)
        {
            session = null;
            return TryNormalizeKnownPath(filePath, out string? path, out _) &&
                   _sessions.TryGetValue(path!, out session);
        }
    }

    public bool TrySynchronizeActiveText(
        Ra2EditableDocumentSession expected,
        string editorText,
        out Ra2EditableDocumentSession? synchronized,
        out string? failureMessage)
    {
        ArgumentNullException.ThrowIfNull(expected);
        editorText ??= string.Empty;
        lock (_gate)
        {
            synchronized = null;
            if (ActiveFilePath is null ||
                !_sessions.TryGetValue(ActiveFilePath, out Ra2EditableDocumentSession? current) ||
                !ReferenceEquals(current, expected))
            {
                failureMessage = "The active project session no longer matches the editor projection.";
                return false;
            }

            if (string.Equals(current.DocumentState.CurrentText, editorText, StringComparison.Ordinal))
            {
                synchronized = current;
                failureMessage = null;
                return true;
            }

            Ra2EditableDocumentSession updated = _sessionService.UpdateText(current, editorText);
            _sessions[ActiveFilePath] = updated;
            ProjectRevision = checked(ProjectRevision + 1);
            synchronized = updated;
            failureMessage = null;
            return true;
        }
    }

    public Ra2ProjectSnapshotCaptureResult CaptureSnapshot(
        IEnumerable<string> targetFilePaths,
        string? activeEditorText,
        Ra2FieldRegistryProviderSnapshot? fieldRegistry)
    {
        try
        {
            if (fieldRegistry is null || fieldRegistry.Revision <= 0)
            {
                return Ra2ProjectSnapshotCaptureResult.Failure(
                    Ra2ProjectSnapshotCaptureFailureKind.InvalidRegistrySnapshot,
                    "The active Field Registry snapshot is invalid.");
            }

            ArgumentNullException.ThrowIfNull(targetFilePaths);
            string[] requestedPaths = targetFilePaths.Select(Path.GetFullPath).ToArray();
            if (requestedPaths.Length is < 1 or > Ra2AutomationProjectSnapshot.MaximumDocumentCount)
            {
                return Ra2ProjectSnapshotCaptureResult.Failure(
                    Ra2ProjectSnapshotCaptureFailureKind.ResourceLimitExceeded,
                    "A project preview must target between one and eight documents.");
            }
            if (requestedPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count() != requestedPaths.Length)
            {
                return Ra2ProjectSnapshotCaptureResult.Failure(
                    Ra2ProjectSnapshotCaptureFailureKind.DuplicateTarget,
                    "Project preview targets must be unique.");
            }

            lock (_gate)
            {
                if (ActiveFilePath is not null &&
                    _sessions.TryGetValue(ActiveFilePath, out Ra2EditableDocumentSession? activeSession) &&
                    (activeEditorText is null || !string.Equals(
                        activeSession.DocumentState.CurrentText,
                        activeEditorText,
                        StringComparison.Ordinal)))
                {
                    return Ra2ProjectSnapshotCaptureResult.Failure(
                        Ra2ProjectSnapshotCaptureFailureKind.ActiveEditorTextMismatch,
                        "The active editor text does not match its project session.");
                }

                List<Ra2AutomationDocumentSnapshot> documents = new(requestedPaths.Length);
                long aggregateCharacters = 0;
                foreach (string requestedPath in requestedPaths)
                {
                    if (!TryNormalizeKnownPath(requestedPath, out string? path, out string? membershipFailure))
                    {
                        return Ra2ProjectSnapshotCaptureResult.Failure(
                            Ra2ProjectSnapshotCaptureFailureKind.InvalidTarget,
                            membershipFailure!);
                    }
                    if (!TryGetOrLoadSessionLocked(path!, out Ra2EditableDocumentSession? session, out string? loadFailure))
                    {
                        return Ra2ProjectSnapshotCaptureResult.Failure(
                            Ra2ProjectSnapshotCaptureFailureKind.ReadFailure,
                            loadFailure!);
                    }
                    if (session!.DocumentState.State == Ra2EditorDocumentState.ReadOnlyPreview)
                    {
                        return Ra2ProjectSnapshotCaptureResult.Failure(
                            Ra2ProjectSnapshotCaptureFailureKind.ReadOnly,
                            $"Project document is read-only: {path}");
                    }
                    if (session.DocumentState.CurrentText.Length > Ra2AutomationEditPreviewService.MaximumDocumentCharacters)
                    {
                        return Ra2ProjectSnapshotCaptureResult.Failure(
                            Ra2ProjectSnapshotCaptureFailureKind.DocumentTooLarge,
                            $"Project document exceeds the preview limit: {path}");
                    }

                    aggregateCharacters += session.DocumentState.CurrentText.Length;
                    if (aggregateCharacters > Ra2AutomationProjectSnapshot.MaximumAggregateDocumentCharacters)
                    {
                        return Ra2ProjectSnapshotCaptureResult.Failure(
                            Ra2ProjectSnapshotCaptureFailureKind.ResourceLimitExceeded,
                            "Project preview documents exceed the aggregate character limit.");
                    }

                    documents.Add(new Ra2AutomationDocumentSnapshot(
                        session.DocumentId,
                        session.EditRevision,
                        session.DocumentState.FilePath,
                        session.DocumentState.CurrentText,
                        true,
                        new Ra2AutomationFieldRegistrySnapshot(fieldRegistry.Provider, fieldRegistry.Revision)));
                }

                return Ra2ProjectSnapshotCaptureResult.Success(new Ra2AutomationProjectSnapshot(
                    ProjectSessionId,
                    ProjectRevision,
                    ProjectRootPath,
                    documents));
            }
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            return Ra2ProjectSnapshotCaptureResult.Failure(
                Ra2ProjectSnapshotCaptureFailureKind.UnexpectedFailure,
                "The project authoring snapshot could not be captured.");
        }
    }

    public bool TryReplaceMany(
        IReadOnlyList<Ra2ProjectDocumentSessionReplacement> replacements,
        out string? failureMessage)
        => TryReplaceManyCore(replacements, restoredProjectRevision: null, out failureMessage);

    internal bool TryRestoreMany(
        IReadOnlyList<Ra2ProjectDocumentSessionReplacement> replacements,
        long restoredProjectRevision,
        out string? failureMessage)
    {
        if (restoredProjectRevision < 0)
            throw new ArgumentOutOfRangeException(nameof(restoredProjectRevision));
        return TryReplaceManyCore(replacements, restoredProjectRevision, out failureMessage);
    }

    private bool TryReplaceManyCore(
        IReadOnlyList<Ra2ProjectDocumentSessionReplacement> replacements,
        long? restoredProjectRevision,
        out string? failureMessage)
    {
        ArgumentNullException.ThrowIfNull(replacements);
        lock (_gate)
        {
            if (replacements.Count == 0 ||
                replacements.Any(replacement => replacement is null) ||
                replacements.Select(replacement => replacement.Expected.DocumentState.FilePath)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() != replacements.Count)
            {
                failureMessage = "Project session replacements are empty or contain duplicate targets.";
                return false;
            }

            List<(string Path, Ra2ProjectDocumentSessionReplacement Replacement)> normalized = new(replacements.Count);
            foreach (Ra2ProjectDocumentSessionReplacement replacement in replacements)
            {
                if (!TryNormalizeKnownPath(replacement.Expected.DocumentState.FilePath, out string? path, out failureMessage) ||
                    !_sessions.TryGetValue(path!, out Ra2EditableDocumentSession? current) ||
                    !ReferenceEquals(current, replacement.Expected) ||
                    !string.Equals(replacement.Replacement.DocumentState.FilePath, current.DocumentState.FilePath, StringComparison.OrdinalIgnoreCase) ||
                    replacement.Replacement.DocumentId != current.DocumentId)
                {
                    failureMessage ??= "A project session replacement target is stale or inconsistent.";
                    return false;
                }
                normalized.Add((path!, replacement));
            }

            bool textChanged = false;
            foreach ((string path, Ra2ProjectDocumentSessionReplacement replacement) in normalized)
            {
                textChanged |= !string.Equals(
                    replacement.Expected.DocumentState.CurrentText,
                    replacement.Replacement.DocumentState.CurrentText,
                    StringComparison.Ordinal);
                _sessions[path] = replacement.Replacement;
            }
            if (restoredProjectRevision is not null)
                ProjectRevision = restoredProjectRevision.Value;
            else if (textChanged)
                ProjectRevision = checked(ProjectRevision + 1);
            failureMessage = null;
            return true;
        }
    }

    private bool TryGetOrLoadSessionLocked(
        string path,
        out Ra2EditableDocumentSession? session,
        out string? failureMessage)
    {
        if (_sessions.TryGetValue(path, out session))
        {
            failureMessage = null;
            return true;
        }

        try
        {
            IniTextReadResult read = _fileStore.ReadText(path);
            session = _sessionService.StartEditing(path, read.Text, _encodingAdapter.FromReadResult(read));
            _sessions.Add(path, session);
            failureMessage = null;
            return true;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            session = null;
            failureMessage = $"Project document could not be read: {path}";
            return false;
        }
    }

    private bool TryNormalizeKnownPath(string filePath, out string? normalized, out string? failureMessage)
    {
        normalized = null;
        failureMessage = null;
        try
        {
            string candidate = Path.GetFullPath(filePath);
            if (!_members.ContainsKey(candidate))
            {
                failureMessage = "The requested document is not part of the current project membership.";
                return false;
            }

            normalized = candidate;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            failureMessage = "The requested project document path is invalid.";
            return false;
        }
    }

    private string NormalizeMemberPath(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        string relative = Path.GetRelativePath(ProjectRootPath, fullPath);
        if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("Project membership must stay inside the project root.", nameof(filePath));
        return fullPath;
    }

    private static string NormalizeRoot(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
            throw new ArgumentException("Project root path cannot be empty.", nameof(projectRootPath));
        return Path.GetFullPath(projectRootPath);
    }
}
