namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

internal sealed class FieldRegistryRollbackResult
{
    public FieldRegistryRollbackResult(
        bool succeeded,
        FieldRegistryRollbackOperationKind operationKind,
        string manifestFilePath,
        string targetFilePath,
        string? backupFilePath,
        string message)
    {
        Succeeded = succeeded;
        OperationKind = operationKind;
        ManifestFilePath = manifestFilePath ?? throw new ArgumentNullException(nameof(manifestFilePath));
        TargetFilePath = targetFilePath ?? throw new ArgumentNullException(nameof(targetFilePath));
        BackupFilePath = backupFilePath;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public bool Succeeded { get; }

    public FieldRegistryRollbackOperationKind OperationKind { get; }

    public string ManifestFilePath { get; }

    public string TargetFilePath { get; }

    public string? BackupFilePath { get; }

    public string Message { get; }
}
