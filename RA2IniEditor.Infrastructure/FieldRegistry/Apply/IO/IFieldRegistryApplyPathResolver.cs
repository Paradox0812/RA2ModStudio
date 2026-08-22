namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

internal interface IFieldRegistryApplyPathResolver
{
    string ResolveActiveDirectory(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath);

    string ResolveTargetPackPath(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath,
        string targetPackFileName);

    string ResolveBackupDirectory(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath,
        DateTimeOffset timestamp);
}
