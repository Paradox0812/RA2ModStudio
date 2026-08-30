namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelFaceDirection : byte
{
    NegativeX = 0,
    PositiveX,
    NegativeY,
    PositiveY,
    NegativeZ,
    PositiveZ
}

internal readonly record struct Ra2VoxelSurfaceFace(
    Ra2VoxelCoordinate Coordinate,
    Ra2VoxelFaceDirection Direction,
    byte PaletteIndex);

internal enum Ra2VoxelSurfaceProjectionFailureKind
{
    None = 0,
    ResourceLimitExceeded,
    Cancelled
}

internal sealed class Ra2VoxelSurfaceProjection
{
    private readonly Ra2VoxelSurfaceFace[] _faces;

    internal Ra2VoxelSurfaceProjection(
        string sourceSnapshotHash,
        IEnumerable<Ra2VoxelSurfaceFace> faces,
        int surfaceCellCount)
    {
        SourceSnapshotHash = RequireSha256(sourceSnapshotHash);
        ArgumentNullException.ThrowIfNull(faces);
        _faces = faces.ToArray();
        if (surfaceCellCount < 0 || surfaceCellCount > _faces.Length)
            throw new ArgumentOutOfRangeException(nameof(surfaceCellCount));
        SurfaceCellCount = surfaceCellCount;
    }

    internal string SourceSnapshotHash { get; }
    internal IReadOnlyList<Ra2VoxelSurfaceFace> Faces => Array.AsReadOnly(_faces);
    internal int FaceCount => _faces.Length;
    internal int SurfaceCellCount { get; }

    private static string RequireSha256(string value)
    {
        if (value.Length != 64 || value.Any(character => !char.IsAsciiHexDigit(character)))
            throw new ArgumentException("A canonical source snapshot hash is required.", nameof(value));
        return value.ToUpperInvariant();
    }
}

internal sealed record Ra2VoxelSurfaceProjectionResult(
    Ra2VoxelSurfaceProjectionFailureKind FailureKind,
    string Message,
    Ra2VoxelSurfaceProjection? Projection)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSurfaceProjectionFailureKind.None && Projection is not null;
}

/// <summary>
/// Builds a deterministic visible-face projection from the canonical voxel snapshot.
/// The projection is derived, immutable and format-neutral: VOX and VXL inputs share this path after decoding.
/// </summary>
internal static class Ra2VoxelSurfaceProjector
{
    internal const int DefaultMaximumFaceCount = 250_000;
    internal const int MaximumFaceCount = 1_000_000;

    internal static Ra2VoxelSurfaceProjectionResult Project(
        Ra2VoxelSceneSnapshot snapshot,
        int maximumFaceCount = DefaultMaximumFaceCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (maximumFaceCount is < 1 or > MaximumFaceCount)
            throw new ArgumentOutOfRangeException(nameof(maximumFaceCount));

        try
        {
            List<Ra2VoxelSurfaceFace> faces = new(Math.Min(maximumFaceCount, snapshot.OccupancyCount * 3));
            int surfaceCellCount = 0;
            for (int index = 0; index < snapshot.Cells.Count; index++)
            {
                if ((index & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                Ra2VoxelCell cell = snapshot.Cells[index];
                bool isSurface = false;
                foreach (Ra2VoxelFaceDirection direction in Ra2VoxelNeighbourhood.OrderedDirections)
                {
                    if (!Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, cell.Coordinate, direction))
                        continue;

                    if (faces.Count == maximumFaceCount)
                    {
                        return new(
                            Ra2VoxelSurfaceProjectionFailureKind.ResourceLimitExceeded,
                            $"Voxel surface exceeds the {maximumFaceCount:N0}-face review limit.",
                            null);
                    }

                    faces.Add(new(cell.Coordinate, direction, cell.PaletteIndex));
                    isSurface = true;
                }

                if (isSurface)
                    surfaceCellCount++;
            }

            return new(
                Ra2VoxelSurfaceProjectionFailureKind.None,
                string.Empty,
                new Ra2VoxelSurfaceProjection(snapshot.CanonicalHash, faces, surfaceCellCount));
        }
        catch (OperationCanceledException)
        {
            return new(Ra2VoxelSurfaceProjectionFailureKind.Cancelled, "Voxel surface projection was cancelled.", null);
        }
    }
}

internal static class Ra2VoxelNeighbourhood
{
    internal static readonly IReadOnlyList<Ra2VoxelFaceDirection> OrderedDirections = Array.AsReadOnly(
    [
        Ra2VoxelFaceDirection.NegativeX,
        Ra2VoxelFaceDirection.PositiveX,
        Ra2VoxelFaceDirection.NegativeY,
        Ra2VoxelFaceDirection.PositiveY,
        Ra2VoxelFaceDirection.NegativeZ,
        Ra2VoxelFaceDirection.PositiveZ
    ]);

    internal static bool IsSurfaceCell(Ra2VoxelSceneSnapshot snapshot, Ra2VoxelCoordinate coordinate) =>
        OrderedDirections.Any(direction => IsFaceExposed(snapshot, coordinate, direction));

    internal static bool IsFaceExposed(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelCoordinate coordinate,
        Ra2VoxelFaceDirection direction)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        (int dx, int dy, int dz) = Offset(direction);
        return !IsOccupied(snapshot, coordinate.X + dx, coordinate.Y + dy, coordinate.Z + dz);
    }

    internal static bool IsOccupied(Ra2VoxelSceneSnapshot snapshot, int x, int y, int z)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (x < 0 || y < 0 || z < 0 ||
            x >= snapshot.Part.XSize || y >= snapshot.Part.YSize || z >= snapshot.Part.ZSize)
        {
            return false;
        }

        return snapshot.TryGetPaletteIndex(new Ra2VoxelCoordinate(x, y, z), out _);
    }

    internal static (int X, int Y, int Z) Offset(Ra2VoxelFaceDirection direction) => direction switch
    {
        Ra2VoxelFaceDirection.NegativeX => (-1, 0, 0),
        Ra2VoxelFaceDirection.PositiveX => (1, 0, 0),
        Ra2VoxelFaceDirection.NegativeY => (0, -1, 0),
        Ra2VoxelFaceDirection.PositiveY => (0, 1, 0),
        Ra2VoxelFaceDirection.NegativeZ => (0, 0, -1),
        Ra2VoxelFaceDirection.PositiveZ => (0, 0, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };
}
