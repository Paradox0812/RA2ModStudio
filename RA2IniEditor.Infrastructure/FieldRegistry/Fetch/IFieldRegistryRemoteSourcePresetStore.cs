namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal interface IFieldRegistryRemoteSourcePresetStore
{
    string? LastWarning { get; }

    FieldRegistryRemoteSourcePresetCollection Load(string globalFieldRegistryRootPath);

    void Save(string globalFieldRegistryRootPath, FieldRegistryRemoteSourcePresetCollection collection);

    void AddOrUpdate(string globalFieldRegistryRootPath, FieldRegistryRemoteSourcePreset preset);

    void Remove(string globalFieldRegistryRootPath, string presetId);

    void ImportFromFile(string globalFieldRegistryRootPath, string sourceFilePath, bool replaceExisting);

    void ExportToFile(string globalFieldRegistryRootPath, string targetFilePath);
}
