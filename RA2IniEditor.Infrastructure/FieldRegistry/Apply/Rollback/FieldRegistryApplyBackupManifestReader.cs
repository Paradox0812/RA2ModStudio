using System.Text.Json;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

internal sealed class FieldRegistryApplyBackupManifestReader : IFieldRegistryApplyBackupManifestReader
{
    private static readonly JsonSerializerOptions ReadJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public FieldRegistryApplyBackupManifest Read(string manifestFilePath)
    {
        if (string.IsNullOrWhiteSpace(manifestFilePath))
            throw new ArgumentException("Manifest file path cannot be empty.", nameof(manifestFilePath));

        string fullPath = Path.GetFullPath(manifestFilePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Backup manifest file was not found.", fullPath);

        string json = File.ReadAllText(fullPath);
        FieldRegistryApplyBackupManifest? manifest = JsonSerializer.Deserialize<FieldRegistryApplyBackupManifest>(json, ReadJsonOptions);
        if (manifest is null)
            throw new InvalidOperationException("Backup manifest is empty or invalid.");

        Validate(manifest);
        return manifest;
    }

    public IReadOnlyList<string> FindManifestFiles(string backupRootDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(backupRootDirectoryPath))
            throw new ArgumentException("Backup root directory path cannot be empty.", nameof(backupRootDirectoryPath));

        string fullPath = Path.GetFullPath(backupRootDirectoryPath);
        if (!Directory.Exists(fullPath))
            return [];

        return Directory
            .EnumerateFiles(fullPath, "manifest.json", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void Validate(FieldRegistryApplyBackupManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.TargetScope))
            throw new InvalidOperationException("Backup manifest target scope is required.");

        if (string.IsNullOrWhiteSpace(manifest.TargetFilePath))
            throw new InvalidOperationException("Backup manifest target file path is required.");

        if (string.IsNullOrWhiteSpace(manifest.TimestampUtc))
            throw new InvalidOperationException("Backup manifest timestamp is required.");

        if (string.IsNullOrWhiteSpace(manifest.Mode))
            throw new InvalidOperationException("Backup manifest apply mode is required.");

        if (manifest.AddCount < 0 || manifest.UpdateCount < 0 || manifest.SkipCount < 0)
            throw new InvalidOperationException("Backup manifest counts cannot be negative.");
    }
}
