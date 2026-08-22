using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry;

/// <summary>
/// Contains local field registry definitions and non-fatal load warnings.
/// </summary>
public sealed class LocalFieldRegistryLoadResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFieldRegistryLoadResult"/> class.
    /// </summary>
    public LocalFieldRegistryLoadResult(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IReadOnlyList<string> warnings)
        : this(definitions, warnings, [])
    {
    }

    internal LocalFieldRegistryLoadResult(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IReadOnlyList<string> warnings,
        IReadOnlyList<LocalFieldRegistryLoadedDefinition> loadedDefinitions)
    {
        Definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
        LoadedDefinitions = loadedDefinitions ?? throw new ArgumentNullException(nameof(loadedDefinitions));
    }

    /// <summary>
    /// Gets loaded field definitions.
    /// </summary>
    public IReadOnlyList<Ra2FieldDefinition> Definitions { get; }

    /// <summary>
    /// Gets non-fatal warnings produced while loading local field packs.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; }

    internal IReadOnlyList<LocalFieldRegistryLoadedDefinition> LoadedDefinitions { get; }
}
