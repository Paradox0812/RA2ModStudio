using System.Reflection;

namespace RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;

/// <summary>
/// Loads the embedded built-in fallback field registry pack.
/// </summary>
public sealed class BuiltInFieldRegistryPackLoader
{
    private const string BuiltInPackFileName = "builtin-yr-ares-phobos-fallback-v3.2.fields.json";
    private readonly object _cacheLock = new();
    private LocalFieldRegistryLoadResult? _cachedResult;

    /// <summary>
    /// Loads the embedded built-in field registry pack.
    /// </summary>
    public LocalFieldRegistryLoadResult Load()
    {
        lock (_cacheLock)
        {
            if (_cachedResult is not null)
                return _cachedResult;
        }

        LocalFieldRegistryLoadResult result = LoadCore();
        lock (_cacheLock)
        {
            _cachedResult ??= result;
            return _cachedResult;
        }
    }

    /// <summary>
    /// Clears the cached built-in field registry pack. Intended for explicit reload paths only.
    /// </summary>
    public void ClearCache()
    {
        lock (_cacheLock)
            _cachedResult = null;
    }

    private static LocalFieldRegistryLoadResult LoadCore()
    {
        Assembly assembly = typeof(BuiltInFieldRegistryPackLoader).Assembly;
        string? resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(BuiltInPackFileName, StringComparison.OrdinalIgnoreCase));
        if (resourceName is null)
        {
            return new LocalFieldRegistryLoadResult(
                [],
                [$"Embedded built-in field registry pack '{BuiltInPackFileName}' was not found."]);
        }

        try
        {
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return new LocalFieldRegistryLoadResult(
                    [],
                    [$"Embedded built-in field registry pack '{BuiltInPackFileName}' could not be opened."]);
            }

            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            return new LocalFieldRegistryLoader().LoadJson(json, BuiltInPackFileName);
        }
        catch (IOException ex)
        {
            return new LocalFieldRegistryLoadResult(
                [],
                [$"Failed to read embedded built-in field registry pack '{BuiltInPackFileName}': {ex.Message}"]);
        }
    }
}
