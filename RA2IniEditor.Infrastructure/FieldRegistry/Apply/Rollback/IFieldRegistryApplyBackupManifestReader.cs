using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

internal interface IFieldRegistryApplyBackupManifestReader
{
    FieldRegistryApplyBackupManifest Read(string manifestFilePath);

    IReadOnlyList<string> FindManifestFiles(string backupRootDirectoryPath);
}
