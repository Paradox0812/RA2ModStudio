using System.Text.Json.Serialization;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

internal sealed class FieldRegistryApplyBackupManifest
{
    public FieldRegistryApplyBackupManifest(
        string targetScope,
        string targetFilePath,
        string? backupFilePath,
        bool targetFileExisted,
        string timestampUtc,
        int addCount,
        int updateCount,
        int skipCount,
        string mode)
    {
        TargetScope = targetScope ?? throw new ArgumentNullException(nameof(targetScope));
        TargetFilePath = targetFilePath ?? throw new ArgumentNullException(nameof(targetFilePath));
        BackupFilePath = backupFilePath;
        TargetFileExisted = targetFileExisted;
        TimestampUtc = timestampUtc ?? throw new ArgumentNullException(nameof(timestampUtc));
        AddCount = addCount;
        UpdateCount = updateCount;
        SkipCount = skipCount;
        Mode = mode ?? throw new ArgumentNullException(nameof(mode));
    }

    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; } = "1";

    [JsonPropertyName("targetScope")]
    public string TargetScope { get; }

    [JsonPropertyName("targetFilePath")]
    public string TargetFilePath { get; }

    [JsonPropertyName("backupFilePath")]
    public string? BackupFilePath { get; }

    [JsonPropertyName("targetFileExisted")]
    public bool TargetFileExisted { get; }

    [JsonPropertyName("timestampUtc")]
    public string TimestampUtc { get; }

    [JsonPropertyName("addCount")]
    public int AddCount { get; }

    [JsonPropertyName("updateCount")]
    public int UpdateCount { get; }

    [JsonPropertyName("skipCount")]
    public int SkipCount { get; }

    [JsonPropertyName("mode")]
    public string Mode { get; }
}
