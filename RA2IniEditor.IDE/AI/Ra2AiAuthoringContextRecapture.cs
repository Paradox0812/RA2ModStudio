using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AI;

internal interface IRa2AiAuthoringContextRecapturePort
{
    ValueTask<Ra2AiAuthoringContextRecaptureResult> RecaptureAsync(
        Ra2AiAuthoringRequestContext originalContext,
        CancellationToken cancellationToken);
}

internal sealed record Ra2AiAuthoringContextRecaptureResult(
    Ra2AiAuthoringRequestContext? Context,
    string Message)
{
    public bool Succeeded => Context is not null;

    public static Ra2AiAuthoringContextRecaptureResult Success(Ra2AiAuthoringRequestContext context)
        => new(context ?? throw new ArgumentNullException(nameof(context)), "当前编辑上下文仍可用。");

    public static Ra2AiAuthoringContextRecaptureResult Failure(string message)
        => new(null, string.IsNullOrWhiteSpace(message) ? "当前编辑上下文不可用。" : message.Trim());
}

internal static class Ra2AiAuthoringContextCurrency
{
    public static bool Matches(
        Ra2AiAuthoringRequestContext expected,
        Ra2AiAuthoringRequestContext current)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(current);
        if (expected.Scope != current.Scope)
            return false;
        if (expected.Scope == Ra2AiAuthoringScope.Document)
            return SnapshotsMatch(expected.Snapshot, current.Snapshot);

        Ra2AutomationProjectSnapshot left = expected.ProjectSnapshot!;
        Ra2AutomationProjectSnapshot right = current.ProjectSnapshot!;
        if (left.ProjectSessionId != right.ProjectSessionId ||
            left.ProjectRevision != right.ProjectRevision ||
            !string.Equals(left.ProjectRootPath, right.ProjectRootPath, StringComparison.OrdinalIgnoreCase) ||
            left.Documents.Count != right.Documents.Count ||
            expected.TargetFilePaths.Count != current.TargetFilePaths.Count)
        {
            return false;
        }

        for (int index = 0; index < left.Documents.Count; index++)
        {
            Ra2AutomationDocumentSnapshot expectedDocument = left.Documents[index];
            Ra2AutomationDocumentSnapshot currentDocument = right.Documents[index];
            if (expectedDocument.DocumentId != currentDocument.DocumentId ||
                expectedDocument.Version != currentDocument.Version ||
                expectedDocument.FieldRegistry.Revision != currentDocument.FieldRegistry.Revision ||
                !ReferenceEquals(expectedDocument.FieldRegistry.Provider, currentDocument.FieldRegistry.Provider) ||
                !string.Equals(expectedDocument.FilePath, currentDocument.FilePath, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(expectedDocument.Text, currentDocument.Text, StringComparison.Ordinal) ||
                !string.Equals(
                    expected.TargetFilePaths[index],
                    current.TargetFilePaths[index],
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SnapshotsMatch(Ra2AuthoringSnapshot expected, Ra2AuthoringSnapshot current)
        => expected.DocumentId == current.DocumentId &&
           expected.EditRevision == current.EditRevision &&
           expected.FieldRegistry.Revision == current.FieldRegistry.Revision &&
           string.Equals(expected.Text, current.Text, StringComparison.Ordinal);
}
