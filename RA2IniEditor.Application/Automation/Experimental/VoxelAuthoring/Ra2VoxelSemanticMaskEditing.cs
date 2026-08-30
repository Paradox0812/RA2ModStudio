using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelSemanticBrushMode
{
    Paint = 0,
    Erase
}

internal enum Ra2VoxelSemanticBrushFailureKind
{
    None = 0,
    SnapshotMismatch,
    CellNotFound,
    InvalidAssignment,
    EmptyStroke,
    ResourceLimitExceeded,
    NoChange
}

internal sealed record Ra2VoxelSemanticCellOverride(
    int CellIndex,
    Ra2VoxelSemanticPartRole PartRole,
    Ra2VoxelSemanticMaterialRole MaterialRole,
    Ra2VoxelSemanticRemapIntent RemapIntent,
    string Reason);

/// <summary>
/// Immutable, session-only sparse human overlay bound to the canonical occupied-cell ordering.
/// It never owns geometry and is intentionally not serialized.
/// </summary>
internal sealed class Ra2VoxelSemanticManualMaskLayer
{
    private readonly Ra2VoxelSemanticCellOverride[] _overrides;
    private readonly Dictionary<int, Ra2VoxelSemanticCellOverride> _byCell;

    internal Ra2VoxelSemanticManualMaskLayer(
        string sourceSnapshotHash,
        int cellCount,
        IEnumerable<Ra2VoxelSemanticCellOverride>? overrides = null)
    {
        SourceSnapshotHash = RequireHash(sourceSnapshotHash);
        if (cellCount is < 0 or > Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
            throw new ArgumentOutOfRangeException(nameof(cellCount));
        CellCount = cellCount;
        _overrides = (overrides ?? [])
            .OrderBy(value => value.CellIndex)
            .ToArray();
        if (_overrides.Any(value => value.CellIndex < 0 || value.CellIndex >= cellCount ||
            !Enum.IsDefined(value.PartRole) || !Enum.IsDefined(value.MaterialRole) ||
            !Enum.IsDefined(value.RemapIntent)) ||
            _overrides.Select(value => value.CellIndex).Distinct().Count() != _overrides.Length)
        {
            throw new ArgumentException("Semantic cell overrides are invalid or duplicated.", nameof(overrides));
        }
        _byCell = _overrides.ToDictionary(value => value.CellIndex);
        LayerHash = ComputeHash();
    }

    internal string SourceSnapshotHash { get; }
    internal int CellCount { get; }
    internal IReadOnlyList<Ra2VoxelSemanticCellOverride> Overrides => Array.AsReadOnly(_overrides);
    internal string LayerHash { get; }
    internal bool TryGetOverride(int cellIndex, out Ra2VoxelSemanticCellOverride? value) =>
        _byCell.TryGetValue(cellIndex, out value);

    private string ComputeHash()
    {
        StringBuilder canonical = new("ra2-voxel-semantic-manual-mask/1\n");
        canonical.Append(SourceSnapshotHash).Append('\n').Append(CellCount);
        foreach (Ra2VoxelSemanticCellOverride value in _overrides)
        {
            canonical.Append('\n').Append(value.CellIndex).Append('|').Append(value.PartRole).Append('|')
                .Append(value.MaterialRole).Append('|').Append(value.RemapIntent).Append('|').Append(value.Reason);
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
    }

    private static string RequireHash(string value) => value.Length == 64 && value.All(char.IsAsciiHexDigit)
        ? value.ToUpperInvariant()
        : throw new ArgumentException("A canonical SHA-256 value is required.", nameof(value));
}

internal sealed record Ra2VoxelSemanticBrushResult(
    Ra2VoxelSemanticBrushFailureKind FailureKind,
    string Message,
    Ra2VoxelSemanticManualMaskLayer Layer,
    int AffectedCellCount)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticBrushFailureKind.None;
}

internal static class Ra2VoxelSemanticMaskEditor
{
    internal const int MaximumStrokeSeedCount = 8192;

    internal static Ra2VoxelSemanticBrushResult ApplySurfaceBrush(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticManualMaskLayer layer,
        Ra2VoxelCoordinate seed,
        int radius,
        bool mirror,
        Ra2VoxelSemanticBrushMode mode,
        Ra2VoxelSemanticAssignment? assignment) => ApplySurfaceStroke(
            snapshot,
            layer,
            [seed],
            radius,
            mirror,
            mode,
            assignment);

    internal static Ra2VoxelSemanticBrushResult ApplySurfaceStroke(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticManualMaskLayer layer,
        IEnumerable<Ra2VoxelCoordinate> seeds,
        int radius,
        bool mirror,
        Ra2VoxelSemanticBrushMode mode,
        Ra2VoxelSemanticAssignment? assignment)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(seeds);
        if (!string.Equals(snapshot.CanonicalHash, layer.SourceSnapshotHash, StringComparison.Ordinal) ||
            snapshot.OccupancyCount != layer.CellCount)
        {
            return Failure(Ra2VoxelSemanticBrushFailureKind.SnapshotMismatch,
                "人工蒙版与当前工作几何不匹配。", layer);
        }
        if (radius is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (mode == Ra2VoxelSemanticBrushMode.Paint &&
            (assignment is null || assignment.PartRole == Ra2VoxelSemanticPartRole.Unknown ||
             assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.Unknown ||
             assignment.RemapIntent == Ra2VoxelSemanticRemapIntent.Candidate))
        {
            return Failure(Ra2VoxelSemanticBrushFailureKind.InvalidAssignment,
                "画笔需要先选择明确的部件和材质；阵营色只能使用人工批准状态。", layer);
        }

        List<Ra2VoxelCoordinate> orderedSeeds = [];
        HashSet<Ra2VoxelCoordinate> uniqueSeeds = [];
        foreach (Ra2VoxelCoordinate seed in seeds)
        {
            if (!uniqueSeeds.Add(seed))
                continue;
            if (orderedSeeds.Count >= MaximumStrokeSeedCount)
            {
                return Failure(Ra2VoxelSemanticBrushFailureKind.ResourceLimitExceeded,
                    $"单条笔划最多包含 {MaximumStrokeSeedCount:N0} 个表面采样点。", layer);
            }
            orderedSeeds.Add(seed);
        }
        if (orderedSeeds.Count == 0)
            return Failure(Ra2VoxelSemanticBrushFailureKind.EmptyStroke, "笔划没有命中模型表面。", layer);

        Dictionary<Ra2VoxelCoordinate, int> indices = snapshot.Cells
            .Select((cell, index) => (cell.Coordinate, index))
            .ToDictionary(value => value.Coordinate, value => value.index);
        if (orderedSeeds.Any(seed => !indices.ContainsKey(seed)))
            return Failure(Ra2VoxelSemanticBrushFailureKind.CellNotFound, "笔划包含不属于当前模型的占用体素。", layer);

        HashSet<Ra2VoxelCoordinate> selected = [];
        foreach (Ra2VoxelCoordinate seed in orderedSeeds)
            selected.UnionWith(SurfaceBrush(snapshot, seed, radius, indices));
        if (mirror)
        {
            foreach (Ra2VoxelCoordinate coordinate in selected.ToArray())
            {
                Ra2VoxelCoordinate mirrored = new(snapshot.Part.XSize - 1 - coordinate.X, coordinate.Y, coordinate.Z);
                if (indices.ContainsKey(mirrored)) selected.Add(mirrored);
            }
        }

        Dictionary<int, Ra2VoxelSemanticCellOverride> updated = layer.Overrides.ToDictionary(value => value.CellIndex);
        int changed = 0;
        foreach (Ra2VoxelCoordinate coordinate in selected)
        {
            int index = indices[coordinate];
            if (mode == Ra2VoxelSemanticBrushMode.Erase)
            {
                if (updated.Remove(index)) changed++;
                continue;
            }

            Ra2VoxelSemanticCellOverride next = new(
                index,
                assignment!.PartRole,
                assignment.MaterialRole,
                assignment.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved
                    ? Ra2VoxelSemanticRemapIntent.ExplicitlyApproved
                    : Ra2VoxelSemanticRemapIntent.None,
                NormalizeReason(assignment.Reason));
            if (!updated.TryGetValue(index, out Ra2VoxelSemanticCellOverride? current) || current != next)
            {
                updated[index] = next;
                changed++;
            }
        }
        if (changed == 0)
            return Failure(Ra2VoxelSemanticBrushFailureKind.NoChange, "本次画笔没有改变人工蒙版。", layer);

        return new(
            Ra2VoxelSemanticBrushFailureKind.None,
            string.Empty,
            new(snapshot.CanonicalHash, snapshot.OccupancyCount, updated.Values),
            changed);
    }

    private static HashSet<Ra2VoxelCoordinate> SurfaceBrush(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelCoordinate seed,
        int radius,
        IReadOnlyDictionary<Ra2VoxelCoordinate, int> occupied)
    {
        HashSet<Ra2VoxelCoordinate> selected = [seed];
        Queue<(Ra2VoxelCoordinate Coordinate, int Distance)> queue = new();
        queue.Enqueue((seed, 0));
        while (queue.Count > 0)
        {
            (Ra2VoxelCoordinate coordinate, int distance) = queue.Dequeue();
            if (distance >= radius) continue;
            foreach (Ra2VoxelFaceDirection direction in Ra2VoxelNeighbourhood.OrderedDirections)
            {
                (int dx, int dy, int dz) = Ra2VoxelNeighbourhood.Offset(direction);
                Ra2VoxelCoordinate neighbour = new(coordinate.X + dx, coordinate.Y + dy, coordinate.Z + dz);
                if (!occupied.ContainsKey(neighbour) || !Ra2VoxelNeighbourhood.IsSurfaceCell(snapshot, neighbour) ||
                    !selected.Add(neighbour))
                    continue;
                queue.Enqueue((neighbour, distance + 1));
            }
        }
        return selected;
    }

    private static string NormalizeReason(string? value)
    {
        string normalized = string.Join(" ", (value ?? "人工画笔").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrEmpty(normalized) ? "人工画笔" : normalized.Length <= 128 ? normalized : normalized[..128];
    }

    private static Ra2VoxelSemanticBrushResult Failure(
        Ra2VoxelSemanticBrushFailureKind kind,
        string message,
        Ra2VoxelSemanticManualMaskLayer layer) => new(kind, message, layer, 0);
}

/// <summary>Resolved per-cell semantic projection consumed by preview and colouring only.</summary>
internal sealed class Ra2VoxelSemanticMaskComposition
{
    private readonly Ra2VoxelSemanticEffectiveAssignment[] _assignments;

    internal Ra2VoxelSemanticMaskComposition(
        string sourceSnapshotHash,
        IEnumerable<Ra2VoxelSemanticEffectiveAssignment> assignments,
        string manualLayerHash)
    {
        SourceSnapshotHash = sourceSnapshotHash;
        _assignments = (assignments ?? throw new ArgumentNullException(nameof(assignments))).ToArray();
        if (_assignments.Length > Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
            throw new ArgumentOutOfRangeException(nameof(assignments));
        ManualLayerHash = manualLayerHash;
        CompositionHash = ComputeHash();
    }

    internal string SourceSnapshotHash { get; }
    internal string ManualLayerHash { get; }
    internal int CellCount => _assignments.Length;
    internal IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> Assignments => Array.AsReadOnly(_assignments);
    internal Ra2VoxelSemanticEffectiveAssignment this[int index] => _assignments[index];
    internal string CompositionHash { get; }

    private string ComputeHash()
    {
        StringBuilder value = new("ra2-voxel-semantic-composition/1\n");
        value.Append(SourceSnapshotHash).Append('\n').Append(ManualLayerHash);
        foreach (Ra2VoxelSemanticEffectiveAssignment item in _assignments)
        {
            value.Append('\n').Append(item.RegionId).Append('|').Append(item.PartRole).Append('|')
                .Append(item.MaterialRole).Append('|').Append(item.RemapIntent).Append('|').Append(item.Source);
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value.ToString())));
    }
}

internal static class Ra2VoxelSemanticMaskComposer
{
    internal static Ra2VoxelSemanticMaskComposition Compose(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticEvidencePackage evidence,
        IEnumerable<Ra2VoxelSemanticEffectiveAssignment> regionAssignments,
        Ra2VoxelSemanticManualMaskLayer manualLayer)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(manualLayer);
        if (!string.Equals(snapshot.CanonicalHash, evidence.SourceSnapshotHash, StringComparison.Ordinal) ||
            !string.Equals(snapshot.CanonicalHash, manualLayer.SourceSnapshotHash, StringComparison.Ordinal) ||
            snapshot.OccupancyCount != manualLayer.CellCount)
            throw new ArgumentException("Semantic composition inputs do not match the current snapshot.");

        Dictionary<string, Ra2VoxelSemanticEffectiveAssignment> byRegion = (regionAssignments ?? throw new ArgumentNullException(nameof(regionAssignments)))
            .ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        Ra2VoxelSemanticEffectiveAssignment unknown = new("unknown", Ra2VoxelSemanticPartRole.Unknown,
            Ra2VoxelSemanticMaterialRole.Unknown, Ra2VoxelSemanticRemapIntent.None,
            Ra2VoxelSemanticAssignmentSource.Unknown, 0d, "未分类");
        Ra2VoxelSemanticEffectiveAssignment[] cells = Enumerable.Repeat(unknown, snapshot.OccupancyCount).ToArray();
        foreach (Ra2VoxelSemanticRegionEvidence region in evidence.Regions)
        {
            if (!byRegion.TryGetValue(region.RegionId, out Ra2VoxelSemanticEffectiveAssignment? assignment)) continue;
            for (int index = 0; index < region.Selected.Count; index++)
                if (region.Selected[index] != 0) cells[index] = assignment;
        }
        foreach (Ra2VoxelSemanticCellOverride cellOverride in manualLayer.Overrides)
        {
            cells[cellOverride.CellIndex] = new(
                cells[cellOverride.CellIndex].RegionId,
                cellOverride.PartRole,
                cellOverride.MaterialRole,
                cellOverride.RemapIntent,
                Ra2VoxelSemanticAssignmentSource.HumanOverride,
                1d,
                cellOverride.Reason);
        }
        return new(snapshot.CanonicalHash, cells, manualLayer.LayerHash);
    }
}
