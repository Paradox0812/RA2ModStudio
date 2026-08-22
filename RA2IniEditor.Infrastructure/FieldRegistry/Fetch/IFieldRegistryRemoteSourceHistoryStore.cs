namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal interface IFieldRegistryRemoteSourceHistoryStore
{
    string? LastWarning { get; }

    FieldRegistryRemoteSourceHistory Load(string globalFieldRegistryRootPath);

    void Save(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistory history);

    void AddOrUpdate(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistoryEntry entry);

    void Clear(string globalFieldRegistryRootPath);
}
