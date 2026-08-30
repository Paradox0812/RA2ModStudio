extern alias Ra2Application;

using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelWorkingGeometryOrigin
{
    LoadedSource = 0,
    GeneratedSource,
    RefinedCandidate,
    AgentGeometryCandidate
}

/// <summary>
/// Session-only authority for the geometry from which every later review batch is derived.
/// Lineage deliberately remains outside the serialized voxel snapshot.
/// </summary>
internal sealed record Ra2VoxelWorkingGeometryState
{
    internal Ra2VoxelWorkingGeometryState(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelWorkingGeometryOrigin origin,
        string displayName,
        long revision,
        string rootSnapshotHash,
        string? parentSnapshotHash)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        if (!Enum.IsDefined(origin))
            throw new ArgumentOutOfRangeException(nameof(origin));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("A working-geometry display name is required.", nameof(displayName));
        if (revision < 0)
            throw new ArgumentOutOfRangeException(nameof(revision));
        Origin = origin;
        DisplayName = displayName.Trim();
        Revision = revision;
        RootSnapshotHash = RequireHash(rootSnapshotHash, nameof(rootSnapshotHash));
        ParentSnapshotHash = parentSnapshotHash is null ? null : RequireHash(parentSnapshotHash, nameof(parentSnapshotHash));
    }

    internal Ra2VoxelSceneSnapshot Snapshot { get; }
    internal Ra2VoxelWorkingGeometryOrigin Origin { get; }
    internal string DisplayName { get; }
    internal long Revision { get; }
    internal string RootSnapshotHash { get; }
    internal string? ParentSnapshotHash { get; }

    internal static Ra2VoxelWorkingGeometryState CreateRoot(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelWorkingGeometryOrigin origin,
        string displayName) =>
        new(snapshot, origin, displayName, 0, snapshot.CanonicalHash, null);

    internal Ra2VoxelWorkingGeometryState? Advance(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelWorkingGeometryOrigin origin,
        string displayName)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (string.Equals(snapshot.CanonicalHash, Snapshot.CanonicalHash, StringComparison.Ordinal))
            return null;
        return new(snapshot, origin, displayName, checked(Revision + 1), RootSnapshotHash, Snapshot.CanonicalHash);
    }

    private static string RequireHash(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value) && value.Length == 64 && value.All(char.IsAsciiHexDigit)
            ? value.ToUpperInvariant()
            : throw new ArgumentException("A canonical SHA-256 snapshot hash is required.", parameterName);
}
