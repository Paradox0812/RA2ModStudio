namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal interface IFieldRegistryRawUrlResolver
{
    bool TryResolve(string inputUrl, out string resolvedUrl, out string errorMessage);
}
