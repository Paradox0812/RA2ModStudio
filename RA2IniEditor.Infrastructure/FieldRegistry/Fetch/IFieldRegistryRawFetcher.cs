namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal interface IFieldRegistryRawFetcher
{
    Task<FieldRegistryRawFetchResult> FetchAsync(
        FieldRegistryRawFetchRequest request,
        CancellationToken cancellationToken);
}
