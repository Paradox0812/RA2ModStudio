namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

internal sealed class FieldRegistryApplyWriteResult
{
    public FieldRegistryApplyWriteResult(
        string targetFilePath,
        string? backupDirectoryPath,
        string? manifestFilePath,
        int addedCount,
        int updatedCount,
        int skippedCount,
        IReadOnlyList<string> warnings)
    {
        TargetFilePath = string.IsNullOrWhiteSpace(targetFilePath)
            ? throw new ArgumentException("Target file path cannot be empty.", nameof(targetFilePath))
            : targetFilePath;
        BackupDirectoryPath = backupDirectoryPath;
        ManifestFilePath = manifestFilePath;
        AddedCount = addedCount;
        UpdatedCount = updatedCount;
        SkippedCount = skippedCount;
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public string TargetFilePath { get; }

    public string? BackupDirectoryPath { get; }

    public string? ManifestFilePath { get; }

    public int AddedCount { get; }

    public int UpdatedCount { get; }

    public int SkippedCount { get; }

    public IReadOnlyList<string> Warnings { get; }
}
