namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal static class FieldRegistryRawFetcherFactory
{
    public static IFieldRegistryRawFetcher CreateDefault()
        => new GitHubRawFieldRegistryFetcher();
}
