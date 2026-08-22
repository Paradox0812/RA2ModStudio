namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

internal sealed class FieldRegistryRollbackRequest
{
    public FieldRegistryRollbackRequest(
        string manifestFilePath,
        string? projectRootPath,
        string globalFieldRegistryRootPath)
    {
        ManifestFilePath = manifestFilePath ?? throw new ArgumentNullException(nameof(manifestFilePath));
        ProjectRootPath = projectRootPath;
        GlobalFieldRegistryRootPath = globalFieldRegistryRootPath ?? throw new ArgumentNullException(nameof(globalFieldRegistryRootPath));
    }

    public string ManifestFilePath { get; }

    public string? ProjectRootPath { get; }

    public string GlobalFieldRegistryRootPath { get; }
}
