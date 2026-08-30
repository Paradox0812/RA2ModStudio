using System.IO;

namespace RA2IniEditor.IDE.AssetAuthoring;

/// <summary>
/// Stores discardable unit-class proposals through the same bounded SHA-keyed JSON cache used by style plans.
/// </summary>
internal sealed class Ra2VoxelUnitClassProposalCache
{
    private readonly Ra2VoxelStylePlanCache _store;

    internal Ra2VoxelUnitClassProposalCache(string root) =>
        _store = new Ra2VoxelStylePlanCache(root);

    internal static string DefaultRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RA2IniEditor",
        "AssetUnitClassCache",
        "v1");

    internal bool TryRead(string cacheKey, out string json) => _store.TryRead(cacheKey, out json);

    internal void Store(string cacheKey, string json) => _store.Store(cacheKey, json);
}
