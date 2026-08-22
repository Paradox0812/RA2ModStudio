namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

internal sealed class FieldRegistryApplyPathResolver : IFieldRegistryApplyPathResolver
{
    public string ResolveActiveDirectory(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath)
    {
        string root = ResolveRoot(targetScope, projectRootPath, globalFieldRegistryRootPath);
        return targetScope == FieldRegistryApplyTargetScope.Project
            ? Path.Combine(root, ".ra2inieditor", "field-registry", "active")
            : Path.Combine(root, "active");
    }

    public string ResolveTargetPackPath(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath,
        string targetPackFileName)
    {
        ValidateTargetPackFileName(targetPackFileName);
        return Path.Combine(
            ResolveActiveDirectory(targetScope, projectRootPath, globalFieldRegistryRootPath),
            targetPackFileName);
    }

    public string ResolveBackupDirectory(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath,
        DateTimeOffset timestamp)
    {
        string root = ResolveRoot(targetScope, projectRootPath, globalFieldRegistryRootPath);
        string backupRoot = targetScope == FieldRegistryApplyTargetScope.Project
            ? Path.Combine(root, ".ra2inieditor", "field-registry", "backups")
            : Path.Combine(root, "backups");
        return Path.Combine(backupRoot, timestamp.UtcDateTime.ToString("yyyyMMdd-HHmmss"));
    }

    internal static void ValidateTargetPackFileName(string targetPackFileName)
    {
        if (string.IsNullOrWhiteSpace(targetPackFileName))
            throw new ArgumentException("Target pack file name cannot be empty.", nameof(targetPackFileName));

        if (Path.GetFileName(targetPackFileName) != targetPackFileName ||
            targetPackFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Target pack file name must be a file name without path separators.", nameof(targetPackFileName));
        }
    }

    private static string ResolveRoot(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath)
    {
        if (targetScope == FieldRegistryApplyTargetScope.Project)
        {
            return string.IsNullOrWhiteSpace(projectRootPath)
                ? throw new InvalidOperationException("Project target requires a project root path.")
                : projectRootPath;
        }

        return string.IsNullOrWhiteSpace(globalFieldRegistryRootPath)
            ? throw new ArgumentException("Global field registry root path cannot be empty.", nameof(globalFieldRegistryRootPath))
            : globalFieldRegistryRootPath;
    }
}
