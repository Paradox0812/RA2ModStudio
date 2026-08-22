using RA2IniEditor.IDE.Services;

namespace RA2IniEditor.IDE.Editing;

internal enum Ra2AuthoringSnapshotCaptureFailureKind
{
    None = 0,
    NoEditableSession,
    ReadOnly,
    EditorSessionTextMismatch,
    InvalidDocumentIdentity,
    InvalidRegistrySnapshot,
    UnexpectedFailure
}

/// <summary>
/// 表示一次绑定编辑会话和字段库版本的不可变创作快照。
/// </summary>
internal sealed class Ra2AuthoringSnapshot
{
    private Ra2AuthoringSnapshot(
        Guid documentId,
        int editRevision,
        string projectRootPath,
        string filePath,
        string text,
        bool isDirty,
        Ra2FieldRegistryProviderSnapshot fieldRegistry)
    {
        DocumentId = documentId;
        EditRevision = editRevision;
        ProjectRootPath = projectRootPath;
        FilePath = filePath;
        Text = text;
        IsDirty = isDirty;
        FieldRegistry = fieldRegistry;
    }

    public Guid DocumentId { get; }

    public int EditRevision { get; }

    public string ProjectRootPath { get; }

    public string FilePath { get; }

    public string Text { get; }

    public bool IsEditable => true;

    public bool IsDirty { get; }

    public Ra2FieldRegistryProviderSnapshot FieldRegistry { get; }

    public static Ra2AuthoringSnapshotCaptureResult Capture(
        Ra2EditableDocumentSession? session,
        string? editorText,
        string? projectRootPath,
        Ra2FieldRegistryProviderSnapshot? fieldRegistry)
    {
        try
        {
            if (session is null)
            {
                return Ra2AuthoringSnapshotCaptureResult.Failed(
                    Ra2AuthoringSnapshotCaptureFailureKind.NoEditableSession,
                    "当前没有可用于创作预览的编辑会话。");
            }

            if (session.DocumentState.State == Ra2EditorDocumentState.ReadOnlyPreview)
            {
                return Ra2AuthoringSnapshotCaptureResult.Failed(
                    Ra2AuthoringSnapshotCaptureFailureKind.ReadOnly,
                    "当前文档处于只读预览状态。");
            }

            if (session.DocumentId == Guid.Empty || session.EditRevision < 0)
            {
                return Ra2AuthoringSnapshotCaptureResult.Failed(
                    Ra2AuthoringSnapshotCaptureFailureKind.InvalidDocumentIdentity,
                    "当前编辑会话缺少有效的文档身份。");
            }

            if (fieldRegistry is null || fieldRegistry.Revision <= 0)
            {
                return Ra2AuthoringSnapshotCaptureResult.Failed(
                    Ra2AuthoringSnapshotCaptureFailureKind.InvalidRegistrySnapshot,
                    "当前字段库快照无效。");
            }

            if (editorText is null ||
                !string.Equals(
                    session.DocumentState.CurrentText,
                    editorText,
                    StringComparison.Ordinal))
            {
                return Ra2AuthoringSnapshotCaptureResult.Failed(
                    Ra2AuthoringSnapshotCaptureFailureKind.EditorSessionTextMismatch,
                    "编辑器文本与编辑会话不同步，请稍后重试。");
            }

            return Ra2AuthoringSnapshotCaptureResult.FromSnapshot(new Ra2AuthoringSnapshot(
                session.DocumentId,
                session.EditRevision,
                projectRootPath ?? string.Empty,
                session.DocumentState.FilePath,
                editorText,
                session.DocumentState.IsDirty,
                fieldRegistry));
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and
                                          not StackOverflowException and
                                          not AccessViolationException)
        {
            return Ra2AuthoringSnapshotCaptureResult.Failed(
                Ra2AuthoringSnapshotCaptureFailureKind.UnexpectedFailure,
                "无法捕获当前文档的创作快照。");
        }
    }
}

internal sealed class Ra2AuthoringSnapshotCaptureResult
{
    private Ra2AuthoringSnapshotCaptureResult(
        Ra2AuthoringSnapshot? snapshot,
        Ra2AuthoringSnapshotCaptureFailureKind failureKind,
        string? failureMessage)
    {
        bool succeeded = failureKind == Ra2AuthoringSnapshotCaptureFailureKind.None;
        if (succeeded != (snapshot is not null) ||
            succeeded != (failureMessage is null))
        {
            throw new ArgumentException("Snapshot capture result state is inconsistent.");
        }

        Snapshot = snapshot;
        FailureKind = failureKind;
        FailureMessage = failureMessage;
    }

    public bool Succeeded => FailureKind == Ra2AuthoringSnapshotCaptureFailureKind.None;

    public Ra2AuthoringSnapshot? Snapshot { get; }

    public Ra2AuthoringSnapshotCaptureFailureKind FailureKind { get; }

    public string? FailureMessage { get; }

    public static Ra2AuthoringSnapshotCaptureResult FromSnapshot(Ra2AuthoringSnapshot snapshot)
        => new(
            snapshot ?? throw new ArgumentNullException(nameof(snapshot)),
            Ra2AuthoringSnapshotCaptureFailureKind.None,
            null);

    public static Ra2AuthoringSnapshotCaptureResult Failed(
        Ra2AuthoringSnapshotCaptureFailureKind failureKind,
        string failureMessage)
    {
        if (failureKind == Ra2AuthoringSnapshotCaptureFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(failureMessage))
            throw new ArgumentException("Capture failure message cannot be empty.", nameof(failureMessage));

        return new Ra2AuthoringSnapshotCaptureResult(null, failureKind, failureMessage);
    }
}
