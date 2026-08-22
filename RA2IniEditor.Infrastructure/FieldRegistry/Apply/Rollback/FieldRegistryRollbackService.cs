using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

internal sealed class FieldRegistryRollbackService : IFieldRegistryRollbackService
{
    private readonly IFieldRegistryApplyBackupManifestReader _manifestReader;
    private readonly IFieldRegistryApplyPathResolver _pathResolver;

    public FieldRegistryRollbackService()
        : this(new FieldRegistryApplyBackupManifestReader(), new FieldRegistryApplyPathResolver())
    {
    }

    public FieldRegistryRollbackService(
        IFieldRegistryApplyBackupManifestReader manifestReader,
        IFieldRegistryApplyPathResolver pathResolver)
    {
        _manifestReader = manifestReader ?? throw new ArgumentNullException(nameof(manifestReader));
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
    }

    public FieldRegistryRollbackResult Rollback(FieldRegistryRollbackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string manifestFilePath = ValidateManifestFilePath(request.ManifestFilePath);
        FieldRegistryApplyBackupManifest manifest = _manifestReader.Read(manifestFilePath);
        FieldRegistryApplyTargetScope targetScope = ParseTargetScope(manifest.TargetScope);
        string backupRoot = ResolveBackupRoot(targetScope, request.ProjectRootPath, request.GlobalFieldRegistryRootPath);
        ValidatePathUnderDirectory(manifestFilePath, backupRoot, "Backup manifest must be under the allowed backups root.");

        string targetFilePath = ValidateTargetFilePath(manifest.TargetFilePath, targetScope, request);
        string? backupFilePath = ValidateBackupFilePath(manifest, manifestFilePath, backupRoot);

        if (manifest.TargetFileExisted)
        {
            RestoreBackup(backupFilePath!, targetFilePath);
            return new FieldRegistryRollbackResult(
                true,
                FieldRegistryRollbackOperationKind.RestoreBackup,
                manifestFilePath,
                targetFilePath,
                backupFilePath,
                "Rollback restored target file from backup.");
        }

        if (!File.Exists(targetFilePath))
        {
            return new FieldRegistryRollbackResult(
                true,
                FieldRegistryRollbackOperationKind.NoOp,
                manifestFilePath,
                targetFilePath,
                null,
                "Rollback skipped because created target file was already absent.");
        }

        File.Delete(targetFilePath);
        return new FieldRegistryRollbackResult(
            true,
            FieldRegistryRollbackOperationKind.DeleteCreatedTarget,
            manifestFilePath,
            targetFilePath,
            null,
            "Rollback deleted target file created by apply.");
    }

    private static string ValidateManifestFilePath(string manifestFilePath)
    {
        if (string.IsNullOrWhiteSpace(manifestFilePath))
            throw new ArgumentException("Manifest file path cannot be empty.", nameof(manifestFilePath));

        string fullPath = Path.GetFullPath(manifestFilePath);
        if (!Path.IsPathFullyQualified(fullPath))
            throw new InvalidOperationException("Manifest file path must be a full path.");

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Backup manifest file was not found.", fullPath);

        string fileName = Path.GetFileName(fullPath);
        if (!string.Equals(fileName, "manifest.json", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Path.GetExtension(fullPath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Backup manifest file must be manifest.json or a .json file.");
        }

        return fullPath;
    }

    private string ValidateTargetFilePath(
        string targetFilePath,
        FieldRegistryApplyTargetScope targetScope,
        FieldRegistryRollbackRequest request)
    {
        if (string.IsNullOrWhiteSpace(targetFilePath))
            throw new InvalidOperationException("Backup manifest target file path is required.");

        string fullPath = Path.GetFullPath(targetFilePath);
        string fileName = Path.GetFileName(fullPath);
        if (!string.Equals(fileName, FieldRegistryApplyWriteRequest.DefaultTargetPackFileName, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Rollback only supports target pack '{FieldRegistryApplyWriteRequest.DefaultTargetPackFileName}'.");

        string activeRoot = _pathResolver.ResolveActiveDirectory(
            targetScope,
            request.ProjectRootPath,
            request.GlobalFieldRegistryRootPath);
        ValidatePathUnderDirectory(fullPath, activeRoot, "Backup manifest target file must be under the allowed active root.");
        return fullPath;
    }

    private static string? ValidateBackupFilePath(
        FieldRegistryApplyBackupManifest manifest,
        string manifestFilePath,
        string backupRoot)
    {
        if (!manifest.TargetFileExisted)
        {
            if (!string.IsNullOrWhiteSpace(manifest.BackupFilePath))
                throw new InvalidOperationException("Backup file path must be empty when target file did not exist before apply.");

            return null;
        }

        if (string.IsNullOrWhiteSpace(manifest.BackupFilePath))
            throw new InvalidOperationException("Backup file path is required when target file existed before apply.");

        string backupFilePath = Path.GetFullPath(manifest.BackupFilePath);
        ValidatePathUnderDirectory(backupFilePath, backupRoot, "Backup file must be under the allowed backups root.");

        string manifestDirectory = Path.GetDirectoryName(manifestFilePath)
            ?? throw new InvalidOperationException("Backup manifest directory could not be resolved.");
        ValidatePathUnderDirectory(backupFilePath, manifestDirectory, "Backup file must be in the same backup batch as the manifest.");

        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Backup file was not found.", backupFilePath);

        return backupFilePath;
    }

    private static FieldRegistryApplyTargetScope ParseTargetScope(string targetScope)
    {
        if (Enum.TryParse(targetScope, ignoreCase: true, out FieldRegistryApplyTargetScope result))
            return result;

        throw new InvalidOperationException($"Unsupported rollback target scope '{targetScope}'.");
    }

    private static string ResolveBackupRoot(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath)
    {
        if (targetScope == FieldRegistryApplyTargetScope.Project)
        {
            if (string.IsNullOrWhiteSpace(projectRootPath))
                throw new InvalidOperationException("Project rollback requires a project root path.");

            return Path.Combine(projectRootPath, ".ra2inieditor", "field-registry", "backups");
        }

        if (string.IsNullOrWhiteSpace(globalFieldRegistryRootPath))
            throw new ArgumentException("Global field registry root path cannot be empty.", nameof(globalFieldRegistryRootPath));

        return Path.Combine(globalFieldRegistryRootPath, "backups");
    }

    private static void RestoreBackup(string backupFilePath, string targetFilePath)
    {
        string? targetDirectory = Path.GetDirectoryName(targetFilePath);
        if (!string.IsNullOrWhiteSpace(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        string tempPath = Path.Combine(targetDirectory ?? Environment.CurrentDirectory, $".{Path.GetFileName(targetFilePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.Copy(backupFilePath, tempPath, overwrite: false);
            if (File.Exists(targetFilePath))
            {
                try
                {
                    File.Replace(tempPath, targetFilePath, null, ignoreMetadataErrors: true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
                {
                    File.Move(tempPath, targetFilePath, overwrite: true);
                }
            }
            else
            {
                File.Move(tempPath, targetFilePath);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Temporary cleanup must not hide the real rollback failure.
                }
            }
        }
    }

    private static void ValidatePathUnderDirectory(string filePath, string directoryPath, string message)
    {
        string fullFilePath = Path.GetFullPath(filePath);
        string fullDirectoryPath = Path.GetFullPath(directoryPath);
        string normalizedDirectory = fullDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;

        if (!fullFilePath.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(message);
    }
}
