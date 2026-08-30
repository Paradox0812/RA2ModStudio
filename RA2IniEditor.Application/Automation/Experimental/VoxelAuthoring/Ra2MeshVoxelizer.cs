using System.Diagnostics.CodeAnalysis;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2MeshVoxelizationFailureKind
{
    None = 0,
    InputTooLarge,
    MalformedContainer,
    UnsupportedFeature,
    ResourceLimitExceeded,
    InvalidTransform,
    InvalidAccessor,
    InvalidIndex,
    NonFiniteGeometry,
    DegenerateGeometry,
    DisconnectedGeometry,
    OpenSurface,
    NonManifoldSurface,
    InvalidOptions,
    EmptyVoxelResult,
    VoxelLimitExceeded,
    AnalysisFailed,
    Cancelled
}

[Flags]
internal enum Ra2MeshVoxelReviewFlags
{
    None = 0,
    GeometryCandidate = 1 << 0,
    UniformColourCandidate = 1 << 1,
    PivotReviewRequired = 1 << 2,
    NormalsNotGenerated = 1 << 3,
    HvaNotGenerated = 1 << 4,
    GameValidationNotRun = 1 << 5,
    SemanticPartSplitNotAttempted = 1 << 6
}

internal sealed class Ra2MeshVoxelizationOptions
{
    internal const int MinimumTargetLongestDimension = 8;
    internal const int MaximumTargetLongestDimension = 128;
    internal const int MinimumPadding = 1;
    internal const int MaximumPadding = 4;

    internal Ra2MeshVoxelizationOptions(
        string sceneId,
        string partId,
        Ra2VoxelAssemblyPartRole role,
        string vxlSectionName,
        string stableFileStem,
        int targetLongestDimension,
        int padding,
        Ra2VoxelPaletteProfile palette,
        byte? paletteIndex = null,
        Ra2Rgba32? targetColour = null)
    {
        SceneId = Ra2VoxelSceneSnapshot.ValidateIdentity(sceneId, nameof(sceneId));
        PartId = Ra2VoxelSceneSnapshot.ValidateIdentity(partId, nameof(partId));
        VxlSectionName = Ra2VoxelSceneSnapshot.ValidateIdentity(
            vxlSectionName,
            nameof(vxlSectionName),
            Ra2VoxelAssemblyPartSpec.MaximumSectionNameLength);
        StableFileStem = Ra2VoxelSceneSnapshot.ValidateIdentity(stableFileStem, nameof(stableFileStem));
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        if (targetLongestDimension is < MinimumTargetLongestDimension or > MaximumTargetLongestDimension)
            throw new ArgumentOutOfRangeException(nameof(targetLongestDimension));
        if (padding is < MinimumPadding or > MaximumPadding || targetLongestDimension - (2 * padding) < 2)
            throw new ArgumentOutOfRangeException(nameof(padding));
        ArgumentNullException.ThrowIfNull(palette);
        if (paletteIndex.HasValue == targetColour.HasValue)
            throw new ArgumentException("Specify exactly one palette index or target colour.");
        if (paletteIndex is byte index && palette.IsTransparent(index))
            throw new ArgumentException("The selected palette index cannot be transparent.", nameof(paletteIndex));

        Role = role;
        TargetLongestDimension = targetLongestDimension;
        Padding = padding;
        Palette = palette;
        PaletteIndex = paletteIndex;
        TargetColour = targetColour;
    }

    internal string SceneId { get; }
    internal string PartId { get; }
    internal Ra2VoxelAssemblyPartRole Role { get; }
    internal string VxlSectionName { get; }
    internal string StableFileStem { get; }
    internal int TargetLongestDimension { get; }
    internal int Padding { get; }
    internal Ra2VoxelPaletteProfile Palette { get; }
    internal byte? PaletteIndex { get; }
    internal Ra2Rgba32? TargetColour { get; }
}

internal readonly record struct Ra2MeshVoxelizationFacts(
    string SourceHash,
    string AxisMapId,
    Ra2MeshBounds SourceBounds,
    Ra2MeshBounds CanonicalBounds,
    double VoxelsPerSourceUnit,
    int XSize,
    int YSize,
    int ZSize,
    int SurfaceCellCount,
    int InteriorCellCount,
    int TotalCellCount,
    byte PaletteIndex,
    string PaletteHash,
    Ra2MeshTopologyFacts Topology,
    Ra2MeshVoxelReviewFlags ReviewFlags);

internal sealed class Ra2MeshVoxelizationResult
{
    private Ra2MeshVoxelizationResult(
        Ra2MeshVoxelizationFailureKind failureKind,
        string message,
        Ra2VoxelSceneSnapshot? snapshot,
        Ra2MeshVoxelizationFacts? facts)
    {
        if (message.Length > 512)
            message = message[..512];
        FailureKind = failureKind;
        Message = message;
        Snapshot = snapshot;
        Facts = facts;
    }

    internal bool IsSuccess => FailureKind == Ra2MeshVoxelizationFailureKind.None;
    internal Ra2MeshVoxelizationFailureKind FailureKind { get; }
    internal string Message { get; }
    internal Ra2VoxelSceneSnapshot? Snapshot { get; }
    internal Ra2MeshVoxelizationFacts? Facts { get; }

    internal static Ra2MeshVoxelizationResult Success(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2MeshVoxelizationFacts facts) =>
        new(Ra2MeshVoxelizationFailureKind.None, string.Empty, snapshot, facts);

    internal static Ra2MeshVoxelizationResult Failure(
        Ra2MeshVoxelizationFailureKind failureKind,
        string message) =>
        new(failureKind, message, null, null);
}

internal static class Ra2MeshVoxelizer
{
    internal const int MaximumGridCellCount = 4_000_000;
    internal const string AxisMapId = "gltf-x-right-z-forward-y-up_to_ra2-x-right-y-forward-z-up/v1";

    private const Ra2MeshVoxelReviewFlags RequiredReviewFlags =
        Ra2MeshVoxelReviewFlags.GeometryCandidate |
        Ra2MeshVoxelReviewFlags.UniformColourCandidate |
        Ra2MeshVoxelReviewFlags.PivotReviewRequired |
        Ra2MeshVoxelReviewFlags.NormalsNotGenerated |
        Ra2MeshVoxelReviewFlags.HvaNotGenerated |
        Ra2MeshVoxelReviewFlags.GameValidationNotRun |
        Ra2MeshVoxelReviewFlags.SemanticPartSplitNotAttempted;

    internal static Ra2MeshVoxelizationResult ConvertGlb(
        ReadOnlyMemory<byte> glb,
        Ra2MeshVoxelizationOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(options);
            Ra2MeshSnapshot mesh = Ra2GlbMeshReader.Read(glb, cancellationToken);
            return Convert(mesh, options, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Ra2MeshVoxelizationResult.Failure(
                Ra2MeshVoxelizationFailureKind.Cancelled,
                "Mesh voxelization was cancelled.");
        }
        catch (Ra2MeshVoxelizationException exception)
        {
            return Ra2MeshVoxelizationResult.Failure(exception.FailureKind, exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Ra2MeshVoxelizationResult.Failure(
                Ra2MeshVoxelizationFailureKind.InvalidOptions,
                exception.Message);
        }
        catch (Exception exception)
        {
            return Ra2MeshVoxelizationResult.Failure(
                Ra2MeshVoxelizationFailureKind.AnalysisFailed,
                $"Mesh voxelization failed: {exception.Message}");
        }
    }

    internal static Ra2MeshVoxelizationResult Convert(
        Ra2MeshSnapshot mesh,
        Ra2MeshVoxelizationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        Ra2MeshTopologyFacts topology = mesh.Topology;
        if (topology.RepeatedIndexTriangleCount != 0 || topology.ZeroAreaTriangleCount != 0)
            Throw(Ra2MeshVoxelizationFailureKind.DegenerateGeometry, "Solid voxelization requires non-degenerate triangles.");
        if (topology.ComponentCount != 1)
            Throw(Ra2MeshVoxelizationFailureKind.DisconnectedGeometry, "Solid voxelization requires one connected mesh component.");
        if (topology.NonManifoldEdgeCount != 0)
            Throw(Ra2MeshVoxelizationFailureKind.NonManifoldSurface, "Solid voxelization requires a manifold surface.");
        if (topology.BoundaryEdgeCount != 0)
            Throw(Ra2MeshVoxelizationFailureKind.OpenSurface, "Solid voxelization requires a watertight surface.");

        Ra2MeshVector3[] canonicalSource = mesh.Positions
            .Select(position => new Ra2MeshVector3(position.X, position.Z, position.Y))
            .ToArray();
        Ra2MeshBounds canonicalBounds = ComputeBounds(canonicalSource);
        Ra2MeshVector3 extents = canonicalBounds.Extents;
        double maximumExtent = Math.Max(extents.X, Math.Max(extents.Y, extents.Z));
        int innerLongest = options.TargetLongestDimension - (2 * options.Padding);
        if (!double.IsFinite(maximumExtent) || maximumExtent <= 1e-12 || innerLongest < 2)
            Throw(Ra2MeshVoxelizationFailureKind.DegenerateGeometry, "Mesh bounds collapse under normalization.");

        double scale = (innerLongest - 1d) / maximumExtent;
        int xSize = ComputeDimension(extents.X, scale, options.Padding);
        int ySize = ComputeDimension(extents.Y, scale, options.Padding);
        int zSize = ComputeDimension(extents.Z, scale, options.Padding);
        int gridCount;
        try
        {
            gridCount = checked(xSize * ySize * zSize);
        }
        catch (OverflowException)
        {
            Throw(Ra2MeshVoxelizationFailureKind.VoxelLimitExceeded, "Normalized voxel grid exceeds the resource limit.");
            throw;
        }
        if (gridCount > MaximumGridCellCount)
            Throw(Ra2MeshVoxelizationFailureKind.VoxelLimitExceeded, "Normalized voxel grid exceeds the resource limit.");

        Ra2MeshVector3[] gridPositions = canonicalSource
            .Select(position => new Ra2MeshVector3(
                ((position.X - canonicalBounds.Minimum.X) * scale) + options.Padding + 0.5d,
                ((position.Y - canonicalBounds.Minimum.Y) * scale) + options.Padding + 0.5d,
                ((position.Z - canonicalBounds.Minimum.Z) * scale) + options.Padding + 0.5d))
            .ToArray();

        bool[] surface = new bool[gridCount];
        int surfaceCount = RasterizeSurface(
            gridPositions,
            mesh.Triangles,
            xSize,
            ySize,
            zSize,
            surface,
            cancellationToken);
        if (surfaceCount == 0)
            Throw(Ra2MeshVoxelizationFailureKind.EmptyVoxelResult, "Triangle rasterization produced no occupied cells.");

        bool[] exterior = FloodExterior(surface, xSize, ySize, zSize, cancellationToken);
        byte selectedPaletteIndex = options.PaletteIndex ?? options.Palette.FindNearestOpaqueIndex(options.TargetColour!.Value);
        List<Ra2VoxelCell> cells = new(Math.Min(gridCount, Ra2VoxelSceneSnapshot.MaximumOccupancyCount));
        int interiorCount = 0;
        for (int z = 0; z < zSize; z++)
        for (int y = 0; y < ySize; y++)
        for (int x = 0; x < xSize; x++)
        {
            int index = GetIndex(x, y, z, xSize, ySize);
            if (!surface[index] && exterior[index])
                continue;
            if (!surface[index])
                interiorCount++;
            if (cells.Count >= Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
                Throw(Ra2MeshVoxelizationFailureKind.VoxelLimitExceeded, "Voxel occupancy exceeds the canonical limit.");
            cells.Add(new Ra2VoxelCell(new Ra2VoxelCoordinate(x, y, z), selectedPaletteIndex));
        }

        if (cells.Count == 0)
            Throw(Ra2MeshVoxelizationFailureKind.EmptyVoxelResult, "Solid fill produced no occupied cells.");
        if (TouchesBoundary(cells, xSize, ySize, zSize))
            Throw(Ra2MeshVoxelizationFailureKind.AnalysisFailed, "Normalized solid touches the protected grid boundary.");

        Ra2VoxelPartDescriptor part = new(
            options.PartId,
            options.Role,
            options.VxlSectionName,
            options.StableFileStem,
            xSize,
            ySize,
            zSize,
            voxelUnitScale: 1d / scale,
            origin: Ra2VoxelVector3.Zero,
            pivot: new Ra2VoxelVector3((xSize - 1) / 2d, (ySize - 1) / 2d, 0d));
        Ra2VoxelSceneSnapshot snapshot = new(
            options.SceneId,
            part,
            options.Palette,
            cells,
            [new KeyValuePair<string, string>("mesh.glb", mesh.SourceHash)]);
        if (!snapshot.Connectivity.IsSingleComponent)
            Throw(Ra2MeshVoxelizationFailureKind.AnalysisFailed, "Canonical voxel output is disconnected.");

        Ra2MeshVoxelizationFacts facts = new(
            mesh.SourceHash,
            AxisMapId,
            mesh.Bounds,
            canonicalBounds,
            scale,
            xSize,
            ySize,
            zSize,
            surfaceCount,
            interiorCount,
            cells.Count,
            selectedPaletteIndex,
            options.Palette.ProfileHash,
            topology,
            RequiredReviewFlags);
        return Ra2MeshVoxelizationResult.Success(snapshot, facts);
    }

    private static int RasterizeSurface(
        IReadOnlyList<Ra2MeshVector3> positions,
        IReadOnlyList<Ra2MeshTriangle> triangles,
        int xSize,
        int ySize,
        int zSize,
        bool[] surface,
        CancellationToken cancellationToken)
    {
        int count = 0;
        for (int triangleIndex = 0; triangleIndex < triangles.Count; triangleIndex++)
        {
            if ((triangleIndex & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            Ra2MeshTriangle triangle = triangles[triangleIndex];
            Ra2MeshVector3 a = positions[triangle.A];
            Ra2MeshVector3 b = positions[triangle.B];
            Ra2MeshVector3 c = positions[triangle.C];
            int minX = Math.Clamp((int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))), 0, xSize - 1);
            int minY = Math.Clamp((int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))), 0, ySize - 1);
            int minZ = Math.Clamp((int)Math.Floor(Math.Min(a.Z, Math.Min(b.Z, c.Z))), 0, zSize - 1);
            int maxX = Math.Clamp((int)Math.Floor(Math.Max(a.X, Math.Max(b.X, c.X))), 0, xSize - 1);
            int maxY = Math.Clamp((int)Math.Floor(Math.Max(a.Y, Math.Max(b.Y, c.Y))), 0, ySize - 1);
            int maxZ = Math.Clamp((int)Math.Floor(Math.Max(a.Z, Math.Max(b.Z, c.Z))), 0, zSize - 1);

            for (int z = minZ; z <= maxZ; z++)
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                int index = GetIndex(x, y, z, xSize, ySize);
                if (surface[index] || !TriangleIntersectsBox(a, b, c, new(x + 0.5d, y + 0.5d, z + 0.5d)))
                    continue;
                surface[index] = true;
                count++;
            }
        }
        return count;
    }

    private static bool TriangleIntersectsBox(
        Ra2MeshVector3 a,
        Ra2MeshVector3 b,
        Ra2MeshVector3 c,
        Ra2MeshVector3 centre)
    {
        Ra2MeshVector3 v0 = a - centre;
        Ra2MeshVector3 v1 = b - centre;
        Ra2MeshVector3 v2 = c - centre;
        Ra2MeshVector3 e0 = v1 - v0;
        Ra2MeshVector3 e1 = v2 - v1;
        Ra2MeshVector3 e2 = v0 - v2;
        Ra2MeshVector3 xAxis = new(1, 0, 0);
        Ra2MeshVector3 yAxis = new(0, 1, 0);
        Ra2MeshVector3 zAxis = new(0, 0, 1);
        return !Separates(v0, v1, v2, xAxis) &&
               !Separates(v0, v1, v2, yAxis) &&
               !Separates(v0, v1, v2, zAxis) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e0, e1)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e0, xAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e0, yAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e0, zAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e1, xAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e1, yAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e1, zAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e2, xAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e2, yAxis)) &&
               !Separates(v0, v1, v2, Ra2MeshVector3.Cross(e2, zAxis));
    }

    private static bool Separates(
        Ra2MeshVector3 v0,
        Ra2MeshVector3 v1,
        Ra2MeshVector3 v2,
        Ra2MeshVector3 axis)
    {
        if (Ra2MeshVector3.Dot(axis, axis) <= 1e-24)
            return false;
        double p0 = Ra2MeshVector3.Dot(v0, axis);
        double p1 = Ra2MeshVector3.Dot(v1, axis);
        double p2 = Ra2MeshVector3.Dot(v2, axis);
        double minimum = Math.Min(p0, Math.Min(p1, p2));
        double maximum = Math.Max(p0, Math.Max(p1, p2));
        double radius = 0.5d * (Math.Abs(axis.X) + Math.Abs(axis.Y) + Math.Abs(axis.Z));
        const double epsilon = 1e-10;
        return minimum > radius + epsilon || maximum < -radius - epsilon;
    }

    private static bool[] FloodExterior(
        bool[] surface,
        int xSize,
        int ySize,
        int zSize,
        CancellationToken cancellationToken)
    {
        bool[] exterior = new bool[surface.Length];
        Queue<int> queue = new();
        for (int z = 0; z < zSize; z++)
        for (int y = 0; y < ySize; y++)
        for (int x = 0; x < xSize; x++)
        {
            if (x != 0 && x != xSize - 1 && y != 0 && y != ySize - 1 && z != 0 && z != zSize - 1)
                continue;
            int index = GetIndex(x, y, z, xSize, ySize);
            if (surface[index] || exterior[index])
                continue;
            exterior[index] = true;
            queue.Enqueue(index);
        }

        int iterations = 0;
        while (queue.TryDequeue(out int index))
        {
            if ((iterations++ & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            int x = index % xSize;
            int yz = index / xSize;
            int y = yz % ySize;
            int z = yz / ySize;
            Visit(x - 1, y, z);
            Visit(x + 1, y, z);
            Visit(x, y - 1, z);
            Visit(x, y + 1, z);
            Visit(x, y, z - 1);
            Visit(x, y, z + 1);
        }
        return exterior;

        void Visit(int x, int y, int z)
        {
            if ((uint)x >= (uint)xSize || (uint)y >= (uint)ySize || (uint)z >= (uint)zSize)
                return;
            int candidate = GetIndex(x, y, z, xSize, ySize);
            if (surface[candidate] || exterior[candidate])
                return;
            exterior[candidate] = true;
            queue.Enqueue(candidate);
        }
    }

    private static Ra2MeshBounds ComputeBounds(IReadOnlyList<Ra2MeshVector3> positions)
    {
        double minX = positions.Min(position => position.X);
        double minY = positions.Min(position => position.Y);
        double minZ = positions.Min(position => position.Z);
        double maxX = positions.Max(position => position.X);
        double maxY = positions.Max(position => position.Y);
        double maxZ = positions.Max(position => position.Z);
        return new(new(minX, minY, minZ), new(maxX, maxY, maxZ));
    }

    private static int ComputeDimension(double extent, double scale, int padding)
    {
        int dimension = checked((int)Math.Ceiling(extent * scale) + 1 + (2 * padding));
        if (dimension is < 1 or > Ra2VxlseSliceImportContract.MaximumVoxelDimension)
            Throw(Ra2MeshVoxelizationFailureKind.VoxelLimitExceeded, "Normalized dimension exceeds the canonical limit.");
        return dimension;
    }

    private static bool TouchesBoundary(IEnumerable<Ra2VoxelCell> cells, int xSize, int ySize, int zSize) =>
        cells.Any(cell =>
            cell.Coordinate.X == 0 || cell.Coordinate.X == xSize - 1 ||
            cell.Coordinate.Y == 0 || cell.Coordinate.Y == ySize - 1 ||
            cell.Coordinate.Z == 0 || cell.Coordinate.Z == zSize - 1);

    private static int GetIndex(int x, int y, int z, int xSize, int ySize) =>
        checked(x + (xSize * (y + (ySize * z))));

    [DoesNotReturn]
    private static void Throw(Ra2MeshVoxelizationFailureKind kind, string message) =>
        throw new Ra2MeshVoxelizationException(kind, message);
}
