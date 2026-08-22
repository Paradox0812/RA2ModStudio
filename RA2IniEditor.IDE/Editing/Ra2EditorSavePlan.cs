using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditorSavePlan
{
    public Ra2EditorSavePlan(
        string filePath,
        string text,
        Ra2IniNewLineKind newLineKind,
        Ra2EditorNewLineSavePolicy newLinePolicy,
        bool canSave,
        string reason,
        Ra2EditorTextEncodingMetadata? encodingMetadata = null,
        Ra2SaveCurrentFilePlanStatus? status = null,
        string? message = null,
        string? backupPlanPreview = null)
    {
        FilePath = filePath ?? string.Empty;
        Text = text ?? string.Empty;
        NewLineKind = newLineKind;
        NewLinePolicy = newLinePolicy;
        CanSave = canSave;
        Reason = string.IsNullOrWhiteSpace(reason)
            ? throw new ArgumentException("Save plan reason cannot be empty.", nameof(reason))
            : reason;
        EncodingMetadata = encodingMetadata ?? Ra2EditorTextEncodingMetadata.Unknown;
        Status = status ?? (canSave
            ? Ra2SaveCurrentFilePlanStatus.CanSave
            : Ra2SaveCurrentFilePlanStatus.UnknownFailure);
        Message = string.IsNullOrWhiteSpace(message) ? Reason : message;
        BackupPlanPreview = backupPlanPreview;
    }

    public string FilePath { get; }

    public string Text { get; }

    public Ra2IniNewLineKind NewLineKind { get; }

    public Ra2EditorNewLineSavePolicy NewLinePolicy { get; }

    public bool CanSave { get; }

    public string Reason { get; }

    public string Message { get; }

    public Ra2SaveCurrentFilePlanStatus Status { get; }

    public string? BackupPlanPreview { get; }

    public Ra2EditorTextEncodingMetadata EncodingMetadata { get; }

    public Ra2EditorSavePlan WithStatus(
        Ra2SaveCurrentFilePlanStatus status,
        string message,
        string? backupPlanPreview)
        => new(
            FilePath,
            Text,
            NewLineKind,
            NewLinePolicy,
            CanSave,
            Reason,
            EncodingMetadata,
            status,
            message,
            backupPlanPreview);
}
