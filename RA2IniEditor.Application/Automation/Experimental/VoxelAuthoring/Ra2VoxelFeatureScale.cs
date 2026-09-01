namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelFeatureScale
{
    Macro = 0,
    Meso,
    Micro,
    SubPixelRisk
}

internal sealed record Ra2VoxelFeatureScaleCount(Ra2VoxelFeatureScale Scale, int CellCount);

internal sealed class Ra2VoxelFeatureScaleProjection
{
    private readonly byte[] _scales;
    private readonly Ra2VoxelFeatureScaleCount[] _counts;

    internal Ra2VoxelFeatureScaleProjection(
        string sourceSnapshotHash,
        string compositionHash,
        string formZoneProjectionHash,
        IEnumerable<byte> scales,
        IEnumerable<Ra2VoxelFeatureScaleCount> counts)
    {
        SourceSnapshotHash = Ra2VoxelColourContractIdentity.RequireSha256(
            sourceSnapshotHash, nameof(sourceSnapshotHash));
        CompositionHash = Ra2VoxelColourContractIdentity.RequireSha256(
            compositionHash, nameof(compositionHash));
        FormZoneProjectionHash = Ra2VoxelColourContractIdentity.RequireSha256(
            formZoneProjectionHash, nameof(formZoneProjectionHash));
        _scales = (scales ?? throw new ArgumentNullException(nameof(scales))).ToArray();
        if (_scales.Any(value => !Enum.IsDefined((Ra2VoxelFeatureScale)value)))
            throw new ArgumentException("Feature-scale projection contains an unknown value.", nameof(scales));
        _counts = (counts ?? throw new ArgumentNullException(nameof(counts)))
            .OrderBy(value => value.Scale)
            .ToArray();
        ProjectionHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-feature-scale/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SourceSnapshotHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, CompositionHash);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, FormZoneProjectionHash);
            writer.Write(_scales.Length);
            writer.Write(_scales);
        });
    }

    internal string SourceSnapshotHash { get; }
    internal string CompositionHash { get; }
    internal string FormZoneProjectionHash { get; }
    internal int CellCount => _scales.Length;
    internal IReadOnlyList<Ra2VoxelFeatureScaleCount> Counts => Array.AsReadOnly(_counts);
    internal string ProjectionHash { get; }
    internal Ra2VoxelFeatureScale this[int index] => (Ra2VoxelFeatureScale)_scales[index];
}

internal static class Ra2VoxelFeatureScaleProjector
{
    internal const string Revision = "feature-scale-projector/1";

    internal static Ra2VoxelFeatureScaleProjection Project(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticMaskComposition composition,
        Ra2VoxelFormZoneProjection formZones)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(composition);
        ArgumentNullException.ThrowIfNull(formZones);
        if (!string.Equals(snapshot.CanonicalHash, composition.SourceSnapshotHash, StringComparison.Ordinal) ||
            !string.Equals(snapshot.CanonicalHash, formZones.SourceSnapshotHash, StringComparison.Ordinal) ||
            snapshot.OccupancyCount != composition.CellCount || snapshot.OccupancyCount != formZones.CellCount)
        {
            throw new ArgumentException("Feature-scale inputs do not match the current snapshot.");
        }

        Dictionary<Ra2VoxelCoordinate, int> byCoordinate = snapshot.Cells
            .Select((cell, index) => (cell.Coordinate, index))
            .ToDictionary(value => value.Coordinate, value => value.index);
        byte[] scales = Enumerable.Repeat((byte)Ra2VoxelFeatureScale.Macro, snapshot.OccupancyCount).ToArray();
        HashSet<int> remaining = Enumerable.Range(0, snapshot.OccupancyCount)
            .Where(index => !formZones.Contains(index, Ra2VoxelFormZone.Interior))
            .ToHashSet();
        int macroThreshold = Math.Max(12, (int)Math.Ceiling(snapshot.OccupancyCount * 0.03d));
        while (remaining.Count > 0)
        {
            int start = remaining.Min();
            remaining.Remove(start);
            ComponentKey key = KeyAt(start);
            Queue<int> queue = new();
            queue.Enqueue(start);
            List<int> component = [];
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                component.Add(current);
                foreach (Ra2VoxelFaceDirection direction in Ra2VoxelNeighbourhood.OrderedDirections)
                {
                    (int dx, int dy, int dz) = Ra2VoxelNeighbourhood.Offset(direction);
                    Ra2VoxelCoordinate c = snapshot.Cells[current].Coordinate;
                    if (byCoordinate.TryGetValue(new(c.X + dx, c.Y + dy, c.Z + dz), out int next) &&
                        remaining.Contains(next) && KeyAt(next) == key)
                    {
                        remaining.Remove(next);
                        queue.Enqueue(next);
                    }
                }
            }

            int spanX = Span(component, value => value.X);
            int spanY = Span(component, value => value.Y);
            int spanZ = Span(component, value => value.Z);
            int broadAxes = new[] { spanX, spanY, spanZ }.Count(value => value >= 4);
            Ra2VoxelFeatureScale scale = component.Count >= macroThreshold || broadAxes >= 2
                ? Ra2VoxelFeatureScale.Macro
                : component.Count >= 4 || Math.Max(spanX, Math.Max(spanY, spanZ)) >= 3
                    ? Ra2VoxelFeatureScale.Meso
                    : component.Count >= 2
                        ? Ra2VoxelFeatureScale.Micro
                        : Ra2VoxelFeatureScale.SubPixelRisk;
            foreach (int index in component)
                scales[index] = (byte)scale;
        }

        Ra2VoxelFeatureScaleCount[] counts = Enum.GetValues<Ra2VoxelFeatureScale>()
            .Select(scale => new Ra2VoxelFeatureScaleCount(scale,
                scales.Count(value => value == (byte)scale)))
            .ToArray();
        return new(snapshot.CanonicalHash, composition.CompositionHash, formZones.ProjectionHash, scales, counts);

        ComponentKey KeyAt(int index) => new(
            composition[index].PartRole,
            composition[index].MaterialRole,
            PrimaryZone(formZones[index]));

        int Span(IEnumerable<int> indices, Func<Ra2VoxelCoordinate, int> selector)
        {
            int[] values = indices.Select(index => selector(snapshot.Cells[index].Coordinate)).ToArray();
            return values.Max() - values.Min() + 1;
        }
    }

    private static Ra2VoxelFormZone PrimaryZone(Ra2VoxelFormZone zones)
    {
        foreach (Ra2VoxelFormZone candidate in new[]
                 {
                     Ra2VoxelFormZone.FrontEnd, Ra2VoxelFormZone.RearEnd,
                     Ra2VoxelFormZone.UpperBevel, Ra2VoxelFormZone.SideShoulder,
                     Ra2VoxelFormZone.SideField, Ra2VoxelFormZone.LowerSkirt,
                     Ra2VoxelFormZone.UpperPlane, Ra2VoxelFormZone.LongitudinalEndUnknown,
                     Ra2VoxelFormZone.UnclassifiedSurface
                 })
        {
            if ((zones & candidate) != 0) return candidate;
        }
        return Ra2VoxelFormZone.Interior;
    }

    private readonly record struct ComponentKey(
        Ra2VoxelSemanticPartRole Part,
        Ra2VoxelSemanticMaterialRole Material,
        Ra2VoxelFormZone Zone);
}
