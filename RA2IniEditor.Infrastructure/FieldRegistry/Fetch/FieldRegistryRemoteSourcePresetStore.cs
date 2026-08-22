using System.Text;
using System.Text.Json;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRemoteSourcePresetStore : IFieldRegistryRemoteSourcePresetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly IFieldRegistryRawUrlResolver _urlResolver;

    public FieldRegistryRemoteSourcePresetStore()
        : this(new GitHubRawUrlResolver())
    {
    }

    public FieldRegistryRemoteSourcePresetStore(IFieldRegistryRawUrlResolver urlResolver)
    {
        _urlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
    }

    public string? LastWarning { get; private set; }

    public FieldRegistryRemoteSourcePresetCollection Load(string globalFieldRegistryRootPath)
    {
        LastWarning = null;
        string path = ResolvePresetsFilePath(globalFieldRegistryRootPath);
        if (!File.Exists(path))
            return new FieldRegistryRemoteSourcePresetCollection();

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            FieldRegistryRemoteSourcePresetCollection? collection = JsonSerializer.Deserialize<FieldRegistryRemoteSourcePresetCollection>(json, JsonOptions);
            return Normalize(collection ?? new FieldRegistryRemoteSourcePresetCollection());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            LastWarning = $"Failed to load remote source presets: {ex.Message}";
            return new FieldRegistryRemoteSourcePresetCollection();
        }
    }

    public void Save(string globalFieldRegistryRootPath, FieldRegistryRemoteSourcePresetCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        LastWarning = null;
        FieldRegistryRemoteSourcePresetCollection normalized = Normalize(collection);
        foreach (FieldRegistryRemoteSourcePreset preset in normalized.Presets)
            ValidatePreset(preset);

        string path = ResolvePresetsFilePath(globalFieldRegistryRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string json = JsonSerializer.Serialize(normalized, JsonOptions);
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public void AddOrUpdate(string globalFieldRegistryRootPath, FieldRegistryRemoteSourcePreset preset)
    {
        ValidatePreset(preset);
        List<FieldRegistryRemoteSourcePreset> presets = Load(globalFieldRegistryRootPath).Presets
            .Where(existing => !string.Equals(existing.Id, preset.Id, StringComparison.OrdinalIgnoreCase))
            .ToList();
        presets.Add(preset);
        Save(globalFieldRegistryRootPath, new FieldRegistryRemoteSourcePresetCollection(presets));
    }

    public void Remove(string globalFieldRegistryRootPath, string presetId)
    {
        if (string.IsNullOrWhiteSpace(presetId))
            return;

        List<FieldRegistryRemoteSourcePreset> presets = Load(globalFieldRegistryRootPath).Presets
            .Where(existing => !string.Equals(existing.Id, presetId, StringComparison.OrdinalIgnoreCase))
            .ToList();
        Save(globalFieldRegistryRootPath, new FieldRegistryRemoteSourcePresetCollection(presets));
    }

    public void ImportFromFile(string globalFieldRegistryRootPath, string sourceFilePath, bool replaceExisting)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new ArgumentException("Source file path cannot be empty.", nameof(sourceFilePath));

        string json = File.ReadAllText(sourceFilePath, Encoding.UTF8);
        FieldRegistryRemoteSourcePresetCollection imported = JsonSerializer.Deserialize<FieldRegistryRemoteSourcePresetCollection>(json, JsonOptions)
            ?? new FieldRegistryRemoteSourcePresetCollection();
        List<string> warnings = [];
        List<FieldRegistryRemoteSourcePreset> validImported = [];
        foreach (FieldRegistryRemoteSourcePreset preset in imported.Presets)
        {
            try
            {
                ValidatePreset(preset);
                validImported.Add(preset);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                warnings.Add($"{preset.Name}: {ex.Message}");
            }
        }

        List<FieldRegistryRemoteSourcePreset> merged = replaceExisting
            ? []
            : Load(globalFieldRegistryRootPath).Presets.ToList();
        foreach (FieldRegistryRemoteSourcePreset preset in validImported)
        {
            merged.RemoveAll(existing => string.Equals(existing.Id, preset.Id, StringComparison.OrdinalIgnoreCase));
            merged.Add(preset);
        }

        Save(globalFieldRegistryRootPath, new FieldRegistryRemoteSourcePresetCollection(merged));
        LastWarning = warnings.Count == 0
            ? null
            : $"Skipped {warnings.Count} invalid preset(s): {string.Join("; ", warnings)}";
    }

    public void ExportToFile(string globalFieldRegistryRootPath, string targetFilePath)
    {
        if (string.IsNullOrWhiteSpace(targetFilePath))
            throw new ArgumentException("Target file path cannot be empty.", nameof(targetFilePath));

        FieldRegistryRemoteSourcePresetCollection collection = Load(globalFieldRegistryRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
        File.WriteAllText(targetFilePath, JsonSerializer.Serialize(collection, JsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static string ResolvePresetsFilePath(string globalFieldRegistryRootPath)
    {
        if (string.IsNullOrWhiteSpace(globalFieldRegistryRootPath))
            throw new ArgumentException("Global field registry root path cannot be empty.", nameof(globalFieldRegistryRootPath));

        return Path.Combine(globalFieldRegistryRootPath, "remote-sources", "presets.json");
    }

    private FieldRegistryRemoteSourcePresetCollection Normalize(FieldRegistryRemoteSourcePresetCollection collection)
    {
        List<FieldRegistryRemoteSourcePreset> presets = collection.Presets
            .Where(preset => !string.IsNullOrWhiteSpace(preset.Id))
            .GroupBy(preset => preset.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return new FieldRegistryRemoteSourcePresetCollection(presets);
    }

    private void ValidatePreset(FieldRegistryRemoteSourcePreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);
        if (string.IsNullOrWhiteSpace(preset.Name))
            throw new ArgumentException("Preset name cannot be empty.");

        if (string.IsNullOrWhiteSpace(preset.Url))
            throw new ArgumentException("Preset URL cannot be empty.");

        if (!_urlResolver.TryResolve(preset.Url, out _, out string errorMessage))
            throw new InvalidOperationException(errorMessage);
    }
}
