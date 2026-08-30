using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelRefinementFailureKind
{
    None = 0,
    NoSafeImprovement,
    InvalidOptions,
    EvidenceGridMismatch,
    SourceConversionFailed,
    SupersampleConversionFailed,
    ProtectedFeatureConflict,
    QualityGateRejected,
    AnalysisFailed,
    Cancelled
}

internal enum Ra2VoxelSymmetryMode
{
    Off = 0,
    Suggest
}

internal enum Ra2VoxelSemanticRegionProvenance
{
    GeometryVerified = 0,
    UserDeclared,
    ModelInferred,
    VisionVerified,
    Unresolved
}

internal enum Ra2VoxelSilhouetteView
{
    Front = 0,
    Rear,
    Left,
    Right,
    Top,
    Bottom
}

internal enum Ra2VoxelRefinementZone : byte
{
    Smoothable = 0,
    Transition = 1,
    Frozen = 2
}

internal enum Ra2VoxelRefinementCandidateKind
{
    Conservative = 0,
    Balanced,
    SurfacePolish
}

/// <summary>
/// Frozen first refinement profile. It intentionally smooths the conversion result rather than mutating provider mesh
/// geometry. Profile changes produce another hash and therefore another review candidate.
/// </summary>
internal sealed class Ra2VoxelRefinementProfile
{
    internal const string DefaultProfileId = "asset-vox-2b-r2/visible-multi-threshold-surface-v4";

    internal Ra2VoxelRefinementProfile(
        string profileId = DefaultProfileId,
        int maximumSupersampleDimension = 128,
        int minimumCoveragePercent = 40,
        int thinSpanThreshold = 2,
        int maximumVolumeDeltaPercent = 5,
        int maximumSilhouetteDeltaPercent = 3,
        int cleanupPasses = 1)
    {
        ProfileId = Ra2VoxelSceneSnapshot.ValidateIdentity(profileId, nameof(profileId));
        if (maximumSupersampleDimension is < 16 or > 128)
            throw new ArgumentOutOfRangeException(nameof(maximumSupersampleDimension));
        if (minimumCoveragePercent is < 25 or > 75)
            throw new ArgumentOutOfRangeException(nameof(minimumCoveragePercent));
        if (thinSpanThreshold is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(thinSpanThreshold));
        if (maximumVolumeDeltaPercent is < 0 or > 20)
            throw new ArgumentOutOfRangeException(nameof(maximumVolumeDeltaPercent));
        if (maximumSilhouetteDeltaPercent is < 0 or > 15)
            throw new ArgumentOutOfRangeException(nameof(maximumSilhouetteDeltaPercent));
        if (cleanupPasses is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(cleanupPasses));

        MaximumSupersampleDimension = maximumSupersampleDimension;
        MinimumCoveragePercent = minimumCoveragePercent;
        ThinSpanThreshold = thinSpanThreshold;
        MaximumVolumeDeltaPercent = maximumVolumeDeltaPercent;
        MaximumSilhouetteDeltaPercent = maximumSilhouetteDeltaPercent;
        CleanupPasses = cleanupPasses;
        ProfileHash = ComputeHash();
    }

    internal string ProfileId { get; }
    internal int MaximumSupersampleDimension { get; }
    internal int MinimumCoveragePercent { get; }
    internal int ThinSpanThreshold { get; }
    internal int MaximumVolumeDeltaPercent { get; }
    internal int MaximumSilhouetteDeltaPercent { get; }
    internal int CleanupPasses { get; }
    internal string ProfileHash { get; }

    private string ComputeHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)3);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, ProfileId);
        writer.Write(MaximumSupersampleDimension);
        writer.Write(MinimumCoveragePercent);
        writer.Write(ThinSpanThreshold);
        writer.Write(MaximumVolumeDeltaPercent);
        writer.Write(MaximumSilhouetteDeltaPercent);
        writer.Write(CleanupPasses);
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }
}

internal sealed record Ra2VoxelSilhouetteFact(
    Ra2VoxelSilhouetteView View,
    int Area,
    string Hash);

internal sealed class Ra2VoxelFeatureProtectionMask
{
    private readonly byte[] _zones;

    internal Ra2VoxelFeatureProtectionMask(string sourceSnapshotHash, IEnumerable<byte> zones)
    {
        SourceSnapshotHash = RequireSha256(sourceSnapshotHash);
        _zones = (zones ?? throw new ArgumentNullException(nameof(zones))).ToArray();
        if (_zones.Length > Ra2VoxelSceneSnapshot.MaximumOccupancyCount ||
            _zones.Any(value => value > (byte)Ra2VoxelRefinementZone.Frozen))
        {
            throw new ArgumentException("A feature-protection mask must contain bounded refinement zones.", nameof(zones));
        }
        MaskHash = Ra2VoxelGeometryRegionMask.ComputeMaskHash(
            "feature-protection-mask/2",
            SourceSnapshotHash,
            _zones);
    }

    internal string SourceSnapshotHash { get; }
    internal int CellCount => _zones.Length;
    internal int ProtectedCellCount => FrozenCellCount;
    internal int FrozenCellCount => _zones.Count(value => value == (byte)Ra2VoxelRefinementZone.Frozen);
    internal int TransitionCellCount => _zones.Count(value => value == (byte)Ra2VoxelRefinementZone.Transition);
    internal string MaskHash { get; }
    internal bool IsProtected(int index) => IsFrozen(index);
    internal bool IsFrozen(int index) => _zones[index] == (byte)Ra2VoxelRefinementZone.Frozen;
    internal bool IsTransition(int index) => _zones[index] == (byte)Ra2VoxelRefinementZone.Transition;
    internal Ra2VoxelRefinementZone ZoneAt(int index) => (Ra2VoxelRefinementZone)_zones[index];

    private static string RequireSha256(string value)
        => value.Length == 64 && value.All(char.IsAsciiHexDigit)
            ? value.ToUpperInvariant()
            : throw new ArgumentException("A canonical source snapshot hash is required.", nameof(value));
}

internal sealed class Ra2VoxelGeometryQualityFacts
{
    private readonly Ra2VoxelSilhouetteFact[] _silhouettes;

    internal Ra2VoxelGeometryQualityFacts(
        string sourceSnapshotHash,
        int occupiedCellCount,
        int surfaceCellCount,
        int exposedFaceCount,
        int lowSupportSurfaceCellCount,
        int thinFeatureCellCount,
        int mirroredCellPairCount,
        int unmatchedCellCount,
        IEnumerable<Ra2VoxelSilhouetteFact> silhouettes)
        : this(
            sourceSnapshotHash,
            occupiedCellCount,
            surfaceCellCount,
            exposedFaceCount,
            lowSupportSurfaceCellCount,
            thinFeatureCellCount,
            transitionCellCount: 0,
            protectedComponentCount: 0,
            protectedEndpointCount: 0,
            protectedBranchCellCount: 0,
            enclosedCavityCount: 0,
            mirroredCellPairCount,
            unmatchedCellCount,
            silhouettes)
    {
    }

    internal Ra2VoxelGeometryQualityFacts(
        string sourceSnapshotHash,
        int occupiedCellCount,
        int surfaceCellCount,
        int exposedFaceCount,
        int lowSupportSurfaceCellCount,
        int thinFeatureCellCount,
        int transitionCellCount,
        int protectedComponentCount,
        int protectedEndpointCount,
        int protectedBranchCellCount,
        int enclosedCavityCount,
        int mirroredCellPairCount,
        int unmatchedCellCount,
        IEnumerable<Ra2VoxelSilhouetteFact> silhouettes)
    {
        SourceSnapshotHash = RequireSha256(sourceSnapshotHash);
        OccupiedCellCount = occupiedCellCount;
        SurfaceCellCount = surfaceCellCount;
        ExposedFaceCount = exposedFaceCount;
        LowSupportSurfaceCellCount = lowSupportSurfaceCellCount;
        ThinFeatureCellCount = thinFeatureCellCount;
        TransitionCellCount = transitionCellCount;
        ProtectedComponentCount = protectedComponentCount;
        ProtectedEndpointCount = protectedEndpointCount;
        ProtectedBranchCellCount = protectedBranchCellCount;
        EnclosedCavityCount = enclosedCavityCount;
        MirroredCellPairCount = mirroredCellPairCount;
        UnmatchedCellCount = unmatchedCellCount;
        _silhouettes = (silhouettes ?? throw new ArgumentNullException(nameof(silhouettes)))
            .OrderBy(value => value.View)
            .ToArray();
        if (_silhouettes.Length != Enum.GetValues<Ra2VoxelSilhouetteView>().Length ||
            _silhouettes.Select(value => value.View).Distinct().Count() != _silhouettes.Length)
        {
            throw new ArgumentException("All fixed silhouette views are required exactly once.", nameof(silhouettes));
        }
        FactsHash = ComputeHash();
    }

    internal string SourceSnapshotHash { get; }
    internal int OccupiedCellCount { get; }
    internal int SurfaceCellCount { get; }
    internal int ExposedFaceCount { get; }
    internal int LowSupportSurfaceCellCount { get; }
    internal int ThinFeatureCellCount { get; }
    internal int TransitionCellCount { get; }
    internal int ProtectedComponentCount { get; }
    internal int ProtectedEndpointCount { get; }
    internal int ProtectedBranchCellCount { get; }
    internal int EnclosedCavityCount { get; }
    internal int MirroredCellPairCount { get; }
    internal int UnmatchedCellCount { get; }
    internal double SymmetryScore => OccupiedCellCount == 0
        ? 1d
        : Math.Clamp(1d - (UnmatchedCellCount / (double)OccupiedCellCount), 0d, 1d);
    internal double RoughnessScore => SurfaceCellCount == 0
        ? 0d
        : ExposedFaceCount / (double)SurfaceCellCount;
    internal IReadOnlyList<Ra2VoxelSilhouetteFact> Silhouettes => Array.AsReadOnly(_silhouettes);
    internal string FactsHash { get; }

    private string ComputeHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)2);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SourceSnapshotHash);
        writer.Write(OccupiedCellCount);
        writer.Write(SurfaceCellCount);
        writer.Write(ExposedFaceCount);
        writer.Write(LowSupportSurfaceCellCount);
        writer.Write(ThinFeatureCellCount);
        writer.Write(TransitionCellCount);
        writer.Write(ProtectedComponentCount);
        writer.Write(ProtectedEndpointCount);
        writer.Write(ProtectedBranchCellCount);
        writer.Write(EnclosedCavityCount);
        writer.Write(MirroredCellPairCount);
        writer.Write(UnmatchedCellCount);
        foreach (Ra2VoxelSilhouetteFact silhouette in _silhouettes)
        {
            writer.Write((int)silhouette.View);
            writer.Write(silhouette.Area);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, silhouette.Hash);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static string RequireSha256(string value)
        => value.Length == 64 && value.All(char.IsAsciiHexDigit)
            ? value.ToUpperInvariant()
            : throw new ArgumentException("A canonical source snapshot hash is required.", nameof(value));
}

internal sealed record Ra2VoxelQualityAnalysisResult(
    Ra2VoxelRefinementFailureKind FailureKind,
    string Message,
    Ra2VoxelGeometryQualityFacts? Facts,
    Ra2VoxelFeatureProtectionMask? ProtectionMask)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelRefinementFailureKind.None &&
        Facts is not null && ProtectionMask is not null;
}

internal static class Ra2VoxelQualityAnalyzer
{
    private enum ThinStructureKind : byte
    {
        Rod = 0,
        Plate
    }

    private readonly record struct ThinStructureSignature(ThinStructureKind Kind, int Axis);

    internal static Ra2VoxelQualityAnalysisResult Analyze(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelRefinementProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        profile ??= new();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelSurfaceProjectionResult surface = Ra2VoxelSurfaceProjector.Project(
                snapshot,
                Ra2VoxelSurfaceProjector.MaximumFaceCount,
                cancellationToken);
            if (!surface.IsSuccess)
            {
                return new(
                    surface.FailureKind == Ra2VoxelSurfaceProjectionFailureKind.Cancelled
                        ? Ra2VoxelRefinementFailureKind.Cancelled
                        : Ra2VoxelRefinementFailureKind.AnalysisFailed,
                    surface.Message,
                    null,
                    null);
            }

            byte[] zones = new byte[snapshot.OccupancyCount];
            int lowSupport = 0;
            Dictionary<Ra2VoxelCoordinate, ThinStructureSignature> thinCandidates = [];
            for (int index = 0; index < snapshot.Cells.Count; index++)
            {
                if ((index & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                Ra2VoxelCoordinate coordinate = snapshot.Cells[index].Coordinate;
                if (!Ra2VoxelNeighbourhood.IsSurfaceCell(snapshot, coordinate))
                    continue;
                int support = FaceNeighbourCount(snapshot, coordinate);
                if (support <= 2)
                    lowSupport++;
                int[] spans =
                [
                    AxisSpan(snapshot, coordinate, 1, 0, 0),
                    AxisSpan(snapshot, coordinate, 0, 1, 0),
                    AxisSpan(snapshot, coordinate, 0, 0, 1)
                ];
                int[] thinAxes = Enumerable.Range(0, 3)
                    .Where(axis => spans[axis] <= profile.ThinSpanThreshold)
                    .ToArray();
                if (thinAxes.Length == 2)
                {
                    int majorAxis = Enumerable.Range(0, 3).Single(axis => !thinAxes.Contains(axis));
                    thinCandidates[coordinate] = new(ThinStructureKind.Rod, majorAxis);
                }
                else if (thinAxes.Length == 1)
                {
                    thinCandidates[coordinate] = new(ThinStructureKind.Plate, thinAxes[0]);
                }
            }

            HashSet<Ra2VoxelCoordinate> frozen = SelectSustainedThinStructures(
                thinCandidates,
                profile.ThinSpanThreshold,
                cancellationToken);
            HashSet<Ra2VoxelCoordinate> occupied = snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet();
            HashSet<Ra2VoxelCoordinate> transition = frozen
                .SelectMany(FaceNeighbours)
                .Where(coordinate => occupied.Contains(coordinate) && !frozen.Contains(coordinate))
                .ToHashSet();
            Dictionary<Ra2VoxelCoordinate, int> cellIndices = snapshot.Cells
                .Select((cell, index) => (cell.Coordinate, index))
                .ToDictionary(value => value.Coordinate, value => value.index);
            foreach (Ra2VoxelCoordinate coordinate in transition)
                zones[cellIndices[coordinate]] = (byte)Ra2VoxelRefinementZone.Transition;
            foreach (Ra2VoxelCoordinate coordinate in frozen)
                zones[cellIndices[coordinate]] = (byte)Ra2VoxelRefinementZone.Frozen;

            IReadOnlyList<HashSet<Ra2VoxelCoordinate>> protectedComponents = ConnectedComponents(frozen, cancellationToken);
            int protectedEndpoints = frozen.Count(coordinate => SetFaceNeighbourCount(frozen, coordinate) <= 1);
            int protectedBranches = frozen.Count(coordinate => SetFaceNeighbourCount(frozen, coordinate) >= 3);

            Ra2VoxelSilhouetteFact[] silhouettes = Enum.GetValues<Ra2VoxelSilhouetteView>()
                .Select(view => BuildSilhouette(snapshot, view))
                .ToArray();
            Ra2VoxelFeatureProtectionMask mask = new(snapshot.CanonicalHash, zones);
            Ra2VoxelGeometryQualityFacts facts = new(
                snapshot.CanonicalHash,
                snapshot.OccupancyCount,
                surface.Projection!.SurfaceCellCount,
                surface.Projection.FaceCount,
                lowSupport,
                mask.ProtectedCellCount,
                mask.TransitionCellCount,
                protectedComponents.Count,
                protectedEndpoints,
                protectedBranches,
                CountEnclosedCavities(snapshot, cancellationToken),
                snapshot.Symmetry.MirroredCellPairCount,
                snapshot.Symmetry.UnmatchedCellCount,
                silhouettes);
            return new(Ra2VoxelRefinementFailureKind.None, string.Empty, facts, mask);
        }
        catch (OperationCanceledException)
        {
            return new(Ra2VoxelRefinementFailureKind.Cancelled, "Voxel quality analysis was cancelled.", null, null);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return new(Ra2VoxelRefinementFailureKind.AnalysisFailed, exception.Message, null, null);
        }
    }

    internal static int FaceNeighbourCount(Ra2VoxelSceneSnapshot snapshot, Ra2VoxelCoordinate coordinate)
        => Ra2VoxelNeighbourhood.OrderedDirections.Count(direction =>
        {
            (int dx, int dy, int dz) = DirectionOffset(direction);
            return Ra2VoxelNeighbourhood.IsOccupied(snapshot, coordinate.X + dx, coordinate.Y + dy, coordinate.Z + dz);
        });

    private static HashSet<Ra2VoxelCoordinate> SelectSustainedThinStructures(
        IReadOnlyDictionary<Ra2VoxelCoordinate, ThinStructureSignature> candidates,
        int thinSpanThreshold,
        CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> frozen = [];
        HashSet<Ra2VoxelCoordinate> remaining = candidates.Keys.ToHashSet();
        while (remaining.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate seed = remaining.MinBy(value => (value.Z, value.Y, value.X));
            ThinStructureSignature signature = candidates[seed];
            HashSet<Ra2VoxelCoordinate> component = [];
            Queue<Ra2VoxelCoordinate> queue = new();
            queue.Enqueue(seed);
            remaining.Remove(seed);
            while (queue.Count > 0)
            {
                Ra2VoxelCoordinate current = queue.Dequeue();
                component.Add(current);
                foreach (Ra2VoxelCoordinate neighbour in StructureNeighbours(current, signature))
                {
                    if (remaining.Contains(neighbour) && candidates.TryGetValue(neighbour, out ThinStructureSignature other) &&
                        other == signature && remaining.Remove(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }
            if (component.Count < 3)
                continue;
            int xSpan = component.Max(value => value.X) - component.Min(value => value.X) + 1;
            int ySpan = component.Max(value => value.Y) - component.Min(value => value.Y) + 1;
            int zSpan = component.Max(value => value.Z) - component.Min(value => value.Z) + 1;
            int[] spans = [xSpan, ySpan, zSpan];
            int[] minorAxes = Enumerable.Range(0, 3).Where(axis => axis != signature.Axis).ToArray();
            bool sustainedRod = signature.Kind == ThinStructureKind.Rod &&
                spans[signature.Axis] >= 3 &&
                spans[minorAxes[0]] <= thinSpanThreshold * 2 &&
                spans[minorAxes[1]] <= thinSpanThreshold * 2;
            bool sustainedPlate = signature.Kind == ThinStructureKind.Plate &&
                spans[signature.Axis] <= thinSpanThreshold &&
                spans[minorAxes[0]] >= 3 && spans[minorAxes[1]] >= 3 && component.Count >= 9;
            if (sustainedRod || sustainedPlate)
                frozen.UnionWith(component);
        }
        return frozen;
    }

    private static IEnumerable<Ra2VoxelCoordinate> StructureNeighbours(
        Ra2VoxelCoordinate value,
        ThinStructureSignature signature)
    {
        if (signature.Kind == ThinStructureKind.Plate)
        {
            foreach (Ra2VoxelCoordinate neighbour in FaceNeighbours(value))
            {
                int delta = signature.Axis switch
                {
                    0 => neighbour.X - value.X,
                    1 => neighbour.Y - value.Y,
                    _ => neighbour.Z - value.Z
                };
                if (delta == 0)
                    yield return neighbour;
            }
            yield break;
        }

        for (int majorDelta = -1; majorDelta <= 1; majorDelta += 2)
        for (int firstMinor = -1; firstMinor <= 1; firstMinor++)
        for (int secondMinor = -1; secondMinor <= 1; secondMinor++)
        {
            yield return signature.Axis switch
            {
                0 => new(value.X + majorDelta, value.Y + firstMinor, value.Z + secondMinor),
                1 => new(value.X + firstMinor, value.Y + majorDelta, value.Z + secondMinor),
                _ => new(value.X + firstMinor, value.Y + secondMinor, value.Z + majorDelta)
            };
        }
    }

    private static IReadOnlyList<HashSet<Ra2VoxelCoordinate>> ConnectedComponents(
        IReadOnlySet<Ra2VoxelCoordinate> coordinates,
        CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> remaining = new(coordinates);
        List<HashSet<Ra2VoxelCoordinate>> components = [];
        while (remaining.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate seed = remaining.MinBy(value => (value.Z, value.Y, value.X));
            Queue<Ra2VoxelCoordinate> queue = new();
            HashSet<Ra2VoxelCoordinate> component = [];
            queue.Enqueue(seed);
            remaining.Remove(seed);
            while (queue.Count > 0)
            {
                Ra2VoxelCoordinate current = queue.Dequeue();
                component.Add(current);
                foreach (Ra2VoxelCoordinate neighbour in FaceNeighbours(current))
                {
                    if (remaining.Remove(neighbour))
                        queue.Enqueue(neighbour);
                }
            }
            components.Add(component);
        }
        return components;
    }

    private static int CountEnclosedCavities(Ra2VoxelSceneSnapshot snapshot, CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> occupied = snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet();
        HashSet<Ra2VoxelCoordinate> outside = [];
        Queue<Ra2VoxelCoordinate> queue = new();
        for (int z = 0; z < snapshot.Part.ZSize; z++)
        for (int y = 0; y < snapshot.Part.YSize; y++)
        for (int x = 0; x < snapshot.Part.XSize; x++)
        {
            if (x != 0 && y != 0 && z != 0 && x != snapshot.Part.XSize - 1 &&
                y != snapshot.Part.YSize - 1 && z != snapshot.Part.ZSize - 1)
                continue;
            Ra2VoxelCoordinate coordinate = new(x, y, z);
            if (!occupied.Contains(coordinate) && outside.Add(coordinate))
                queue.Enqueue(coordinate);
        }
        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (Ra2VoxelCoordinate neighbour in FaceNeighbours(queue.Dequeue()))
            {
                if (neighbour.X < 0 || neighbour.Y < 0 || neighbour.Z < 0 ||
                    neighbour.X >= snapshot.Part.XSize || neighbour.Y >= snapshot.Part.YSize || neighbour.Z >= snapshot.Part.ZSize ||
                    occupied.Contains(neighbour) || !outside.Add(neighbour))
                    continue;
                queue.Enqueue(neighbour);
            }
        }
        HashSet<Ra2VoxelCoordinate> cavityCells = [];
        for (int z = 0; z < snapshot.Part.ZSize; z++)
        for (int y = 0; y < snapshot.Part.YSize; y++)
        for (int x = 0; x < snapshot.Part.XSize; x++)
        {
            Ra2VoxelCoordinate coordinate = new(x, y, z);
            if (!occupied.Contains(coordinate) && !outside.Contains(coordinate))
                cavityCells.Add(coordinate);
        }
        return ConnectedComponents(cavityCells, cancellationToken).Count;
    }

    private static IEnumerable<Ra2VoxelCoordinate> FaceNeighbours(Ra2VoxelCoordinate value)
    {
        yield return value with { X = value.X - 1 };
        yield return value with { X = value.X + 1 };
        yield return value with { Y = value.Y - 1 };
        yield return value with { Y = value.Y + 1 };
        yield return value with { Z = value.Z - 1 };
        yield return value with { Z = value.Z + 1 };
    }

    private static int SetFaceNeighbourCount(IReadOnlySet<Ra2VoxelCoordinate> cells, Ra2VoxelCoordinate coordinate) =>
        FaceNeighbours(coordinate).Count(cells.Contains);

    private static int AxisSpan(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelCoordinate coordinate,
        int dx,
        int dy,
        int dz)
    {
        int span = 1;
        for (int sign = -1; sign <= 1; sign += 2)
        {
            int step = 1;
            while (Ra2VoxelNeighbourhood.IsOccupied(
                       snapshot,
                       coordinate.X + dx * step * sign,
                       coordinate.Y + dy * step * sign,
                       coordinate.Z + dz * step * sign))
            {
                span++;
                step++;
            }
        }
        return span;
    }

    private static Ra2VoxelSilhouetteFact BuildSilhouette(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSilhouetteView view)
    {
        HashSet<(int A, int B)> projection = [];
        foreach (Ra2VoxelCell cell in snapshot.Cells)
        {
            Ra2VoxelCoordinate coordinate = cell.Coordinate;
            projection.Add(view switch
            {
                Ra2VoxelSilhouetteView.Front or Ra2VoxelSilhouetteView.Rear => (coordinate.X, coordinate.Z),
                Ra2VoxelSilhouetteView.Left or Ra2VoxelSilhouetteView.Right => (coordinate.Y, coordinate.Z),
                Ra2VoxelSilhouetteView.Top or Ra2VoxelSilhouetteView.Bottom => (coordinate.X, coordinate.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(view))
            });
        }
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)1);
        writer.Write((int)view);
        foreach ((int a, int b) in projection.OrderBy(value => value.A).ThenBy(value => value.B))
        {
            writer.Write(a);
            writer.Write(b);
        }
        writer.Flush();
        string hash = Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
        return new(view, projection.Count, hash);
    }

    private static (int X, int Y, int Z) DirectionOffset(Ra2VoxelFaceDirection direction) => direction switch
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

internal sealed record Ra2VoxelNormalComparisonFacts(
    string SourceFieldHash,
    string CandidateFieldHash,
    int SourceSampleCount,
    int CandidateSampleCount,
    int CommonCoordinateCount,
    int ChangedNormalIndexCount);

internal sealed record Ra2VoxelSemanticRegionProposal(
    string RegionId,
    Ra2VoxelSemanticRegionProvenance Provenance,
    string DerivationId,
    int CellCount,
    double Confidence,
    string ReviewNote);

internal sealed record Ra2VoxelRefinementAdmissionFacts(
    bool IsAdmitted,
    string CandidateId,
    string Outcome,
    int AddedCellCount,
    int RemovedCellCount,
    int PreservedFrozenCellCount,
    IReadOnlyList<string> GateMessages);

internal sealed record Ra2VoxelRefinementCandidateReview(
    string CandidateId,
    bool IsSafe,
    bool IsSelected,
    Ra2VoxelGeometryQualityFacts? Facts,
    int AddedCellCount,
    int RemovedCellCount,
    string ReviewMessage);

internal sealed class Ra2VoxelRefinementReviewPackage
{
    internal Ra2VoxelRefinementReviewPackage(
        Ra2VoxelGeometryQualityFacts sourceFacts,
        Ra2VoxelGeometryQualityFacts refinedFacts,
        Ra2VoxelGeometryQualityFacts? symmetryFacts,
        Ra2VoxelNormalComparisonFacts normalComparison,
        IEnumerable<Ra2VoxelSemanticRegionProposal> semanticRegions,
        Ra2VoxelFeatureProtectionMask protectionMask,
        Ra2VoxelRefinementAdmissionFacts admission,
        IEnumerable<Ra2VoxelRefinementCandidateReview> candidateReviews)
    {
        SourceFacts = sourceFacts ?? throw new ArgumentNullException(nameof(sourceFacts));
        RefinedFacts = refinedFacts ?? throw new ArgumentNullException(nameof(refinedFacts));
        SymmetryFacts = symmetryFacts;
        NormalComparison = normalComparison ?? throw new ArgumentNullException(nameof(normalComparison));
        ProtectionMask = protectionMask ?? throw new ArgumentNullException(nameof(protectionMask));
        Admission = admission ?? throw new ArgumentNullException(nameof(admission));
        CandidateReviews = Array.AsReadOnly((candidateReviews ?? throw new ArgumentNullException(nameof(candidateReviews)))
            .OrderBy(value => value.CandidateId switch
            {
                "Conservative" => 0,
                "Balanced" => 1,
                "SurfacePolish" => 2,
                _ => 3
            })
            .ThenBy(value => value.CandidateId, StringComparer.Ordinal)
            .ToArray());
        SemanticRegions = Array.AsReadOnly((semanticRegions ?? throw new ArgumentNullException(nameof(semanticRegions)))
            .OrderBy(value => value.RegionId, StringComparer.Ordinal)
            .ToArray());
    }

    internal Ra2VoxelGeometryQualityFacts SourceFacts { get; }
    internal Ra2VoxelGeometryQualityFacts RefinedFacts { get; }
    internal Ra2VoxelGeometryQualityFacts? SymmetryFacts { get; }
    internal Ra2VoxelNormalComparisonFacts NormalComparison { get; }
    internal Ra2VoxelFeatureProtectionMask ProtectionMask { get; }
    internal Ra2VoxelRefinementAdmissionFacts Admission { get; }
    internal IReadOnlyList<Ra2VoxelRefinementCandidateReview> CandidateReviews { get; }
    internal IReadOnlyList<Ra2VoxelSemanticRegionProposal> SemanticRegions { get; }
}

internal sealed record Ra2VoxelQualityRefinementResult(
    Ra2VoxelRefinementFailureKind FailureKind,
    string Message,
    Ra2VoxelSceneSnapshot? DirectCandidate,
    Ra2VoxelSceneSnapshot? RefinedCandidate,
    Ra2VoxelSceneSnapshot? SymmetryCandidate,
    Ra2VoxelMeshCoverageEvidence? MeshCoverageEvidence,
    Ra2VoxelRefinementReviewPackage? ReviewPackage)
{
    internal bool IsSuccess => FailureKind is Ra2VoxelRefinementFailureKind.None or Ra2VoxelRefinementFailureKind.NoSafeImprovement &&
        DirectCandidate is not null && RefinedCandidate is not null && MeshCoverageEvidence is not null && ReviewPackage is not null;
    internal bool HasSafeImprovement => FailureKind == Ra2VoxelRefinementFailureKind.None && ReviewPackage?.Admission.IsAdmitted == true;
}

internal static class Ra2VoxelQualityRefiner
{
    internal static Ra2VoxelSceneSnapshot? SuggestSymmetry(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelRefinementProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        profile ??= new();
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source, profile, cancellationToken);
        if (!analysis.IsSuccess)
            return null;
        return BuildSymmetryCandidate(source, analysis.ProtectionMask!, profile, cancellationToken);
    }

    internal static Ra2VoxelQualityRefinementResult Convert(
        Ra2MeshSnapshot mesh,
        Ra2MeshVoxelizationOptions options,
        Ra2VoxelRefinementProfile? profile = null,
        Ra2VoxelSymmetryMode symmetryMode = Ra2VoxelSymmetryMode.Suggest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(options);
        profile ??= new();
        if (!Enum.IsDefined(symmetryMode))
            return Failure(Ra2VoxelRefinementFailureKind.InvalidOptions, "Voxel symmetry mode is invalid.");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2MeshVoxelizationResult directResult = TryConvertMesh(mesh, options, cancellationToken);
            if (!directResult.IsSuccess)
                return Failure(Ra2VoxelRefinementFailureKind.SourceConversionFailed, directResult.Message);
            Ra2VoxelSceneSnapshot direct = directResult.Snapshot!;
            return RefineExistingCore(direct, mesh, options, profile, symmetryMode, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelRefinementFailureKind.Cancelled, "Voxel quality refinement was cancelled.");
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return Failure(Ra2VoxelRefinementFailureKind.AnalysisFailed, exception.Message);
        }
    }

    /// <summary>
    /// Derives bounded quality candidates from an already-adopted voxel baseline. The mesh remains coverage evidence;
    /// its target-resolution conversion is never returned as a candidate.
    /// </summary>
    internal static Ra2VoxelQualityRefinementResult RefineExisting(
        Ra2VoxelSceneSnapshot baseline,
        Ra2MeshSnapshot meshEvidence,
        Ra2MeshVoxelizationOptions evidenceOptions,
        Ra2VoxelRefinementProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(meshEvidence);
        ArgumentNullException.ThrowIfNull(evidenceOptions);
        profile ??= new();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!EvidenceOptionsMatchBaseline(baseline, evidenceOptions))
            {
                return Failure(
                    Ra2VoxelRefinementFailureKind.InvalidOptions,
                    "Mesh evidence options do not match the working voxel baseline.",
                    baseline);
            }

            Ra2MeshVoxelizationResult registration = TryConvertMesh(meshEvidence, evidenceOptions, cancellationToken);
            if (!registration.IsSuccess)
            {
                return Failure(
                    Ra2VoxelRefinementFailureKind.SourceConversionFailed,
                    registration.Message,
                    baseline);
            }
            if (!EvidenceGridMatchesBaseline(baseline, registration.Snapshot!))
            {
                return Failure(
                    Ra2VoxelRefinementFailureKind.EvidenceGridMismatch,
                    "Mesh evidence does not register to the working voxel grid.",
                    baseline);
            }

            return RefineExistingCore(
                baseline,
                meshEvidence,
                evidenceOptions,
                profile,
                Ra2VoxelSymmetryMode.Off,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelRefinementFailureKind.Cancelled, "Voxel quality refinement was cancelled.", baseline);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return Failure(Ra2VoxelRefinementFailureKind.AnalysisFailed, exception.Message, baseline);
        }
    }

    private static Ra2VoxelQualityRefinementResult RefineExistingCore(
        Ra2VoxelSceneSnapshot baseline,
        Ra2MeshSnapshot meshEvidence,
        Ra2MeshVoxelizationOptions evidenceOptions,
        Ra2VoxelRefinementProfile profile,
        Ra2VoxelSymmetryMode symmetryMode,
        CancellationToken cancellationToken)
    {
        Ra2VoxelQualityAnalysisResult sourceAnalysis = Ra2VoxelQualityAnalyzer.Analyze(baseline, profile, cancellationToken);
        if (!sourceAnalysis.IsSuccess)
            return Failure(sourceAnalysis.FailureKind, sourceAnalysis.Message, baseline);

        int supersampleLongest = Math.Min(
            profile.MaximumSupersampleDimension,
            checked(evidenceOptions.TargetLongestDimension * 2));
        int supersamplePadding = Math.Min(
            Ra2MeshVoxelizationOptions.MaximumPadding,
            Math.Max(evidenceOptions.Padding, evidenceOptions.Padding * 2));
        Ra2MeshVoxelizationOptions supersampleOptions = new(
            evidenceOptions.SceneId,
            evidenceOptions.PartId,
            evidenceOptions.Role,
            evidenceOptions.VxlSectionName,
            evidenceOptions.StableFileStem,
            supersampleLongest,
            supersamplePadding,
            evidenceOptions.Palette,
            evidenceOptions.PaletteIndex,
            evidenceOptions.TargetColour);
        Ra2MeshVoxelizationResult supersampleResult = TryConvertMesh(meshEvidence, supersampleOptions, cancellationToken);
        if (!supersampleResult.IsSuccess)
            return Failure(Ra2VoxelRefinementFailureKind.SupersampleConversionFailed, supersampleResult.Message, baseline);
        Ra2VoxelMeshCoverageEvidence coverageEvidence = Ra2VoxelMeshCoverageEvidence.Create(
            baseline,
            supersampleResult.Snapshot!,
            cancellationToken);

        List<CandidateEvaluation> evaluations = [];
        foreach ((Ra2VoxelRefinementCandidateKind kind, int minimumDeltaClusterSize, int occupancyThreshold) in new[]
        {
            (Ra2VoxelRefinementCandidateKind.Conservative, 3, 30),
            (Ra2VoxelRefinementCandidateKind.Balanced, 2, 28),
            (Ra2VoxelRefinementCandidateKind.SurfacePolish, 2, 36)
        })
        {
            Ra2VoxelSceneSnapshot candidate = BuildMeshEvidenceSurfaceCandidate(
                baseline,
                supersampleResult.Snapshot!,
                sourceAnalysis.ProtectionMask!,
                profile,
                kind,
                minimumDeltaClusterSize,
                occupancyThreshold,
                cancellationToken);
            Ra2VoxelQualityAnalysisResult candidateAnalysis = Ra2VoxelQualityAnalyzer.Analyze(candidate, profile, cancellationToken);
            string? rejection = candidateAnalysis.IsSuccess
                ? ValidateCandidate(baseline, sourceAnalysis.Facts!, candidate, candidateAnalysis.Facts!, sourceAnalysis.ProtectionMask!, profile)
                : candidateAnalysis.Message;
            evaluations.Add(new(kind, candidate, candidateAnalysis, rejection));
        }

        CandidateEvaluation? selected = evaluations
            .Where(value => value.Rejection is null && value.Analysis.IsSuccess)
            .Where(value => IsMeaningfullySmoother(sourceAnalysis.Facts!, value.Analysis.Facts!))
            .OrderBy(value => value.Analysis.Facts!.RoughnessScore)
            .ThenBy(value => value.Analysis.Facts!.LowSupportSurfaceCellCount)
            .ThenBy(value => value.Analysis.Facts!.UnmatchedCellCount)
            .ThenBy(value => CountChangedCells(baseline, value.Snapshot))
            .ThenBy(value => value.Kind)
            .FirstOrDefault();

        bool hasSafeImprovement = selected is not null;
        Ra2VoxelSceneSnapshot refined = selected?.Snapshot ?? baseline;
        Ra2VoxelQualityAnalysisResult refinedAnalysis = selected?.Analysis ?? sourceAnalysis;

        Ra2VoxelSceneSnapshot? symmetry = null;
        Ra2VoxelGeometryQualityFacts? symmetryFacts = null;
        if (hasSafeImprovement && symmetryMode == Ra2VoxelSymmetryMode.Suggest)
        {
            symmetry = BuildSymmetryCandidate(refined, refinedAnalysis.ProtectionMask!, profile, cancellationToken);
            if (symmetry is not null)
            {
                Ra2VoxelQualityAnalysisResult symmetryAnalysis = Ra2VoxelQualityAnalyzer.Analyze(symmetry, profile, cancellationToken);
                if (symmetryAnalysis.IsSuccess &&
                    ValidateCandidate(refined, refinedAnalysis.Facts!, symmetry, symmetryAnalysis.Facts!, refinedAnalysis.ProtectionMask!, profile, allowSymmetryDelta: true) is null &&
                    symmetryAnalysis.Facts!.UnmatchedCellCount <= refinedAnalysis.Facts!.UnmatchedCellCount)
                {
                    symmetryFacts = symmetryAnalysis.Facts;
                }
                else
                {
                    symmetry = null;
                }
            }
        }

        HashSet<Ra2VoxelCoordinate> baselineCoordinates = baseline.Cells.Select(cell => cell.Coordinate).ToHashSet();
        HashSet<Ra2VoxelCoordinate> refinedCoordinates = refined.Cells.Select(cell => cell.Coordinate).ToHashSet();
        int preservedFrozen = baseline.Cells.Where((_, index) => sourceAnalysis.ProtectionMask!.IsFrozen(index))
            .Count(cell => refinedCoordinates.Contains(cell.Coordinate));
        Ra2VoxelRefinementCandidateReview[] candidateReviews = evaluations
            .Select(value => CreateCandidateReview(baseline, sourceAnalysis.Facts!, value, selected))
            .ToArray();
        string[] gateMessages = hasSafeImprovement
            ? ["全部硬门禁通过。", "受保护结构保持原位。", "候选存在可测量的表面质量改进。"]
            : candidateReviews.Select(value => $"{value.CandidateId}: {value.ReviewMessage}").ToArray();
        Ra2VoxelRefinementAdmissionFacts admission = new(
            hasSafeImprovement,
            selected?.Kind.ToString() ?? "Direct",
            hasSafeImprovement ? "Admitted" : "NoSafeImprovement",
            refinedCoordinates.Except(baselineCoordinates).Count(),
            baselineCoordinates.Except(refinedCoordinates).Count(),
            preservedFrozen,
            Array.AsReadOnly(gateMessages));
        Ra2VoxelNormalComparisonFacts normalComparison = CompareNormals(baseline, refined, cancellationToken);
        Ra2VoxelRefinementReviewPackage review = new(
            sourceAnalysis.Facts!,
            refinedAnalysis.Facts!,
            symmetryFacts,
            normalComparison,
            BuildSemanticRegions(refined, sourceAnalysis.ProtectionMask!),
            sourceAnalysis.ProtectionMask!,
            admission,
            candidateReviews);
        return new(
            hasSafeImprovement ? Ra2VoxelRefinementFailureKind.None : Ra2VoxelRefinementFailureKind.NoSafeImprovement,
            hasSafeImprovement ? string.Empty : "No candidate passed every safety gate with a measurable improvement; the working baseline was retained.",
            baseline,
            refined,
            symmetry,
            coverageEvidence,
            review);
    }

    private static bool EvidenceOptionsMatchBaseline(
        Ra2VoxelSceneSnapshot baseline,
        Ra2MeshVoxelizationOptions options) =>
        string.Equals(baseline.SceneId, options.SceneId, StringComparison.Ordinal) &&
        string.Equals(baseline.Part.PartId, options.PartId, StringComparison.Ordinal) &&
        baseline.Part.Role == options.Role &&
        string.Equals(baseline.Part.VxlSectionName, options.VxlSectionName, StringComparison.Ordinal) &&
        string.Equals(baseline.Part.StableFileStem, options.StableFileStem, StringComparison.Ordinal) &&
        string.Equals(baseline.Palette.ProfileHash, options.Palette.ProfileHash, StringComparison.Ordinal);

    private static bool EvidenceGridMatchesBaseline(
        Ra2VoxelSceneSnapshot baseline,
        Ra2VoxelSceneSnapshot registration)
    {
        int baselineLongest = Math.Max(baseline.Part.XSize, Math.Max(baseline.Part.YSize, baseline.Part.ZSize));
        if (baselineLongest < Ra2MeshVoxelizationOptions.MinimumTargetLongestDimension)
            return true;
        return baseline.Part.XSize == registration.Part.XSize &&
               baseline.Part.YSize == registration.Part.YSize &&
               baseline.Part.ZSize == registration.Part.ZSize;
    }

    internal static Ra2VoxelSceneSnapshot BuildMeshEvidenceSurfaceCandidate(
        Ra2VoxelSceneSnapshot direct,
        Ra2VoxelSceneSnapshot supersampled,
        Ra2VoxelFeatureProtectionMask protection,
        Ra2VoxelRefinementProfile profile,
        Ra2VoxelRefinementCandidateKind candidateKind,
        int minimumDeltaClusterSize,
        int occupancyThreshold,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(direct);
        ArgumentNullException.ThrowIfNull(supersampled);
        ArgumentNullException.ThrowIfNull(protection);
        ArgumentNullException.ThrowIfNull(profile);
        if (protection.CellCount != direct.OccupancyCount ||
            !string.Equals(protection.SourceSnapshotHash, direct.CanonicalHash, StringComparison.Ordinal))
        {
            throw new ArgumentException("The feature-protection mask does not belong to the direct candidate.", nameof(protection));
        }
        if (minimumDeltaClusterSize is < 2 or > 8)
            throw new ArgumentOutOfRangeException(nameof(minimumDeltaClusterSize));
        if (occupancyThreshold is < 24 or > 40)
            throw new ArgumentOutOfRangeException(nameof(occupancyThreshold));
        if (profile.CleanupPasses == 0)
            return direct;

        HashSet<Ra2VoxelCoordinate> directCoordinates = direct.Cells.Select(cell => cell.Coordinate).ToHashSet();
        HashSet<Ra2VoxelCoordinate> anchored = direct.Cells
            .Where((_, index) => protection.ZoneAt(index) is Ra2VoxelRefinementZone.Frozen or Ra2VoxelRefinementZone.Transition)
            .Select(cell => cell.Coordinate)
            .ToHashSet();
        HashSet<Ra2VoxelCoordinate> localProposal = BuildWeightedSurfaceProposal(
            directCoordinates,
            direct.Part,
            anchored,
            occupancyThreshold,
            cancellationToken);
        int addEvidencePercent = Math.Max(25, profile.MinimumCoveragePercent - 8);
        int keepEvidencePercent = Math.Max(addEvidencePercent + 1, profile.MinimumCoveragePercent - 4);
        HashSet<Ra2VoxelCoordinate> addEvidence = DownsampleEvidence(
            supersampled,
            direct.Part,
            addEvidencePercent,
            cancellationToken);
        HashSet<Ra2VoxelCoordinate> keepEvidence = DownsampleEvidence(
            supersampled,
            direct.Part,
            keepEvidencePercent,
            cancellationToken);

        HashSet<Ra2VoxelCoordinate> evidenceProposal = new(directCoordinates);
        evidenceProposal.UnionWith(localProposal
            .Except(directCoordinates)
            .Where(addEvidence.Contains));
        evidenceProposal.ExceptWith(directCoordinates
            .Except(localProposal)
            .Where(coordinate => !keepEvidence.Contains(coordinate) && !anchored.Contains(coordinate)));

        HashSet<Ra2VoxelCoordinate> coherent = RetainCoherentDelta(
            directCoordinates,
            evidenceProposal,
            minimumDeltaClusterSize,
            cancellationToken);
        coherent.UnionWith(anchored);
        string candidateHash = System.Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{profile.ProfileHash}:mesh-evidenced:{candidateKind}:{minimumDeltaClusterSize}:{occupancyThreshold}")));
        return CreateDerivedSnapshot(direct, coherent, candidateHash, "voxel-refinement-profile");
    }

    internal static bool IsMeaningfullySmoother(
        Ra2VoxelGeometryQualityFacts source,
        Ra2VoxelGeometryQualityFacts candidate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(candidate);
        return candidate.RoughnessScore + 0.005d < source.RoughnessScore;
    }

    private static Ra2VoxelRefinementCandidateReview CreateCandidateReview(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelGeometryQualityFacts sourceFacts,
        CandidateEvaluation evaluation,
        CandidateEvaluation? selected)
    {
        Ra2VoxelGeometryQualityFacts? facts = evaluation.Analysis.Facts;
        HashSet<Ra2VoxelCoordinate> sourceCoordinates = source.Cells.Select(cell => cell.Coordinate).ToHashSet();
        HashSet<Ra2VoxelCoordinate> candidateCoordinates = evaluation.Snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet();
        int added = candidateCoordinates.Except(sourceCoordinates).Count();
        int removed = sourceCoordinates.Except(candidateCoordinates).Count();
        bool isSafe = evaluation.Rejection is null && evaluation.Analysis.IsSuccess;
        bool isSmoother = isSafe && facts is not null && IsMeaningfullySmoother(sourceFacts, facts);
        string message = evaluation.Rejection ?? (isSmoother
            ? "Safe and materially smoother."
            : "Safe for bounded review, but roughness did not improve enough to become the automatic smoothing candidate.");
        return new(
            evaluation.Kind.ToString(),
            isSafe,
            ReferenceEquals(evaluation, selected),
            facts,
            added,
            removed,
            message);
    }

    private static int CountChangedCells(Ra2VoxelSceneSnapshot source, Ra2VoxelSceneSnapshot candidate)
    {
        HashSet<Ra2VoxelCoordinate> sourceCoordinates = source.Cells.Select(cell => cell.Coordinate).ToHashSet();
        HashSet<Ra2VoxelCoordinate> candidateCoordinates = candidate.Cells.Select(cell => cell.Coordinate).ToHashSet();
        return candidateCoordinates.Except(sourceCoordinates).Count() + sourceCoordinates.Except(candidateCoordinates).Count();
    }

    private static HashSet<Ra2VoxelCoordinate> BuildWeightedSurfaceProposal(
        IReadOnlySet<Ra2VoxelCoordinate> source,
        Ra2VoxelPartDescriptor part,
        IReadOnlySet<Ra2VoxelCoordinate> anchored,
        int occupancyThreshold,
        CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> proposal = [];
        int linear = 0;
        for (int z = 0; z < part.ZSize; z++)
        for (int y = 0; y < part.YSize; y++)
        for (int x = 0; x < part.XSize; x++, linear++)
        {
            if ((linear & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate coordinate = new(x, y, z);
            if (anchored.Contains(coordinate))
            {
                proposal.Add(coordinate);
                continue;
            }

            int weightedOccupancy = 0;
            for (int dz = -1; dz <= 1; dz++)
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (!source.Contains(new(x + dx, y + dy, z + dz)))
                    continue;
                int manhattanDistance = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                weightedOccupancy += manhattanDistance switch
                {
                    0 => 8,
                    1 => 4,
                    2 => 2,
                    _ => 1
                };
            }
            if (weightedOccupancy >= occupancyThreshold)
                proposal.Add(coordinate);
        }
        return proposal;
    }

    private static HashSet<Ra2VoxelCoordinate> DownsampleEvidence(
        Ra2VoxelSceneSnapshot supersampled,
        Ra2VoxelPartDescriptor target,
        int coveragePercent,
        CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> highCells = supersampled.Cells.Select(cell => cell.Coordinate).ToHashSet();
        HashSet<Ra2VoxelCoordinate> evidence = [];
        int linear = 0;
        for (int z = 0; z < target.ZSize; z++)
        for (int y = 0; y < target.YSize; y++)
        for (int x = 0; x < target.XSize; x++, linear++)
        {
            if ((linear & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            (int x0, int x1) = MapRange(x, target.XSize, supersampled.Part.XSize);
            (int y0, int y1) = MapRange(y, target.YSize, supersampled.Part.YSize);
            (int z0, int z1) = MapRange(z, target.ZSize, supersampled.Part.ZSize);
            int total = 0;
            int occupied = 0;
            for (int hz = z0; hz <= z1; hz++)
            for (int hy = y0; hy <= y1; hy++)
            for (int hx = x0; hx <= x1; hx++)
            {
                total++;
                if (highCells.Contains(new(hx, hy, hz)))
                    occupied++;
            }
            if (total > 0 && occupied * 100 >= total * coveragePercent)
                evidence.Add(new(x, y, z));
        }
        return evidence;
    }

    internal static HashSet<Ra2VoxelCoordinate> RetainCoherentDelta(
        IReadOnlySet<Ra2VoxelCoordinate> direct,
        IReadOnlySet<Ra2VoxelCoordinate> proposal,
        int minimumClusterSize,
        CancellationToken cancellationToken = default)
    {
        if (minimumClusterSize is < 2 or > 8)
            throw new ArgumentOutOfRangeException(nameof(minimumClusterSize));
        HashSet<Ra2VoxelCoordinate> result = new(direct);
        foreach (HashSet<Ra2VoxelCoordinate> component in DeltaComponents(
                     proposal.Except(direct).ToHashSet(),
                     cancellationToken))
        {
            if (component.Count >= minimumClusterSize)
                result.UnionWith(component);
        }
        foreach (HashSet<Ra2VoxelCoordinate> component in DeltaComponents(
                     direct.Except(proposal).ToHashSet(),
                     cancellationToken))
        {
            if (component.Count >= minimumClusterSize)
                result.ExceptWith(component);
        }
        return result;
    }

    private static IEnumerable<HashSet<Ra2VoxelCoordinate>> DeltaComponents(
        HashSet<Ra2VoxelCoordinate> coordinates,
        CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> remaining = new(coordinates);
        while (remaining.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate seed = remaining.MinBy(value => (value.Z, value.Y, value.X));
            HashSet<Ra2VoxelCoordinate> component = [];
            Queue<Ra2VoxelCoordinate> queue = new();
            queue.Enqueue(seed);
            remaining.Remove(seed);
            while (queue.Count > 0)
            {
                Ra2VoxelCoordinate current = queue.Dequeue();
                component.Add(current);
                foreach (Ra2VoxelCoordinate neighbour in MooreNeighbours(current))
                {
                    if (remaining.Remove(neighbour))
                        queue.Enqueue(neighbour);
                }
            }
            yield return component;
        }
    }

    private static IEnumerable<Ra2VoxelCoordinate> MooreNeighbours(Ra2VoxelCoordinate value)
    {
        for (int dz = -1; dz <= 1; dz++)
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
        {
            if (dx != 0 || dy != 0 || dz != 0)
                yield return new(value.X + dx, value.Y + dy, value.Z + dz);
        }
    }

    private static Ra2MeshVoxelizationResult TryConvertMesh(
        Ra2MeshSnapshot mesh,
        Ra2MeshVoxelizationOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ra2MeshVoxelizer.Convert(mesh, options, cancellationToken);
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
    }

    private static Ra2VoxelSceneSnapshot? BuildSymmetryCandidate(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelFeatureProtectionMask protection,
        Ra2VoxelRefinementProfile profile,
        CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> occupied = source.Cells.Select(value => value.Coordinate).ToHashSet();
        HashSet<Ra2VoxelCoordinate> protectedCoordinates = source.Cells
            .Where((_, index) => protection.IsProtected(index))
            .Select(cell => cell.Coordinate)
            .ToHashSet();
        HashSet<Ra2VoxelCoordinate> candidate = new(occupied);
        int changed = 0;
        int iteration = 0;
        foreach (Ra2VoxelCoordinate coordinate in occupied.OrderBy(value => value.Z).ThenBy(value => value.Y).ThenBy(value => value.X))
        {
            if ((iteration++ & 4095) == 0) cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate mirror = coordinate with { X = source.Part.XSize - 1 - coordinate.X };
            if (occupied.Contains(mirror) || protectedCoordinates.Contains(coordinate) || protectedCoordinates.Contains(mirror))
                continue;
            int support = FaceNeighbourCount(occupied, coordinate);
            int mirroredSupport = FaceNeighbourCount(occupied, mirror);
            if (support >= 3 && mirroredSupport >= 2 && IsInteriorCoordinate(mirror, source.Part))
            {
                if (candidate.Add(mirror)) changed++;
            }
            else if (support <= 1 && candidate.Remove(coordinate))
            {
                changed++;
            }
        }
        if (changed == 0)
            return null;
        Ra2VoxelSceneSnapshot result = CreateDerivedSnapshot(source, candidate, profile.ProfileHash, "voxel-symmetry-profile");
        return ValidateCandidateConnectivity(source, result) is null ? result : null;
    }

    internal static string? ValidateCandidateConnectivity(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSceneSnapshot candidate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(candidate);
        if (candidate.Connectivity.ComponentCount <= source.Connectivity.ComponentCount)
            return null;
        return $"Refinement introduced disconnected geometry: {source.Connectivity.ComponentCount} → " +
            $"{candidate.Connectivity.ComponentCount} components.";
    }

    private static string? ValidateCandidate(
        Ra2VoxelSceneSnapshot sourceSnapshot,
        Ra2VoxelGeometryQualityFacts source,
        Ra2VoxelSceneSnapshot candidateSnapshot,
        Ra2VoxelGeometryQualityFacts candidate,
        Ra2VoxelFeatureProtectionMask protection,
        Ra2VoxelRefinementProfile profile,
        bool allowSymmetryDelta = false)
    {
        string? connectivity = ValidateCandidateConnectivity(sourceSnapshot, candidateSnapshot);
        if (connectivity is not null)
            return connectivity;
        if (!ProtectedFeaturesSurvive(sourceSnapshot, candidateSnapshot, protection))
            return "Refinement removed one or more frozen structure coordinates.";
        if (candidate.EnclosedCavityCount > source.EnclosedCavityCount)
            return $"Refinement introduced enclosed cavities: {source.EnclosedCavityCount} → {candidate.EnclosedCavityCount}.";
        double volumeDelta = PercentDelta(source.OccupiedCellCount, candidate.OccupiedCellCount);
        if (volumeDelta > profile.MaximumVolumeDeltaPercent)
            return $"Refinement changes occupied volume by {volumeDelta:F2}%, above the {profile.MaximumVolumeDeltaPercent}% gate.";
        foreach (Ra2VoxelSilhouetteFact sourceView in source.Silhouettes)
        {
            Ra2VoxelSilhouetteFact candidateView = candidate.Silhouettes.Single(value => value.View == sourceView.View);
            double delta = PercentDelta(sourceView.Area, candidateView.Area);
            if (delta > profile.MaximumSilhouetteDeltaPercent && !allowSymmetryDelta)
                return $"Refinement changes {sourceView.View} silhouette by {delta:F2}%, above the {profile.MaximumSilhouetteDeltaPercent}% gate.";
        }
        if (!allowSymmetryDelta)
        {
            if (candidate.LowSupportSurfaceCellCount > source.LowSupportSurfaceCellCount)
                return $"Low-support surface cells regress: {source.LowSupportSurfaceCellCount} → {candidate.LowSupportSurfaceCellCount}.";
            if (candidate.RoughnessScore > source.RoughnessScore + 0.005d)
                return $"Surface roughness regresses: {source.RoughnessScore:F3} → {candidate.RoughnessScore:F3}.";
            if (candidate.SymmetryScore + 0.005d < source.SymmetryScore)
                return $"X symmetry regresses: {source.SymmetryScore:P1} → {candidate.SymmetryScore:P1}.";
            bool improved = candidate.LowSupportSurfaceCellCount < source.LowSupportSurfaceCellCount ||
                candidate.RoughnessScore + 0.005d < source.RoughnessScore ||
                candidate.UnmatchedCellCount < source.UnmatchedCellCount;
            if (!improved)
                return "Candidate passed safety limits but did not measurably improve surface quality.";
        }
        return null;
    }

    private static bool ProtectedFeaturesSurvive(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSceneSnapshot candidate,
        Ra2VoxelFeatureProtectionMask protection)
    {
        HashSet<Ra2VoxelCoordinate> candidateCells = candidate.Cells
            .Select(cell => cell.Coordinate)
            .ToHashSet();
        for (int index = 0; index < source.Cells.Count; index++)
        {
            if (protection.IsProtected(index) && !candidateCells.Contains(source.Cells[index].Coordinate))
                return false;
        }
        return true;
    }

    private static Ra2VoxelNormalComparisonFacts CompareNormals(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSceneSnapshot candidate,
        CancellationToken cancellationToken)
    {
        Ra2VoxelNormalBakeResult sourceNormals = Ra2VoxelNormalBaker.Bake(source, cancellationToken: cancellationToken);
        Ra2VoxelNormalBakeResult candidateNormals = Ra2VoxelNormalBaker.Bake(candidate, cancellationToken: cancellationToken);
        if (!sourceNormals.IsSuccess || !candidateNormals.IsSuccess)
            throw new InvalidOperationException("Voxel normal review could not be generated for a refinement candidate.");
        Dictionary<Ra2VoxelCoordinate, byte> sourceLookup = sourceNormals.Field!.Samples
            .ToDictionary(value => value.Coordinate, value => value.NormalIndex);
        int common = 0;
        int changed = 0;
        foreach (Ra2VoxelNormalSample sample in candidateNormals.Field!.Samples)
        {
            if (!sourceLookup.TryGetValue(sample.Coordinate, out byte normal)) continue;
            common++;
            if (normal != sample.NormalIndex) changed++;
        }
        return new(
            sourceNormals.Field.FieldHash,
            candidateNormals.Field.FieldHash,
            sourceNormals.Field.Samples.Count,
            candidateNormals.Field.Samples.Count,
            common,
            changed);
    }

    private static IEnumerable<Ra2VoxelSemanticRegionProposal> BuildSemanticRegions(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelFeatureProtectionMask protection)
    {
        int lowerLimit = Math.Max(1, (int)Math.Ceiling(snapshot.Part.ZSize * 0.35d));
        int upperStart = Math.Max(0, (int)Math.Floor(snapshot.Part.ZSize * 0.65d));
        int lowerSide = snapshot.Cells.Count(cell => cell.Coordinate.Z < lowerLimit &&
            Ra2VoxelNeighbourhood.IsSurfaceCell(snapshot, cell.Coordinate));
        int upperSurface = snapshot.Cells.Count(cell => cell.Coordinate.Z >= upperStart &&
            Ra2VoxelNeighbourhood.IsSurfaceCell(snapshot, cell.Coordinate));
        return
        [
            new("body-shell", Ra2VoxelSemanticRegionProvenance.GeometryVerified, "occupied-canonical-cells/1",
                snapshot.OccupancyCount, 1d, "Complete occupied body candidate; no functional material label implied."),
            new("lower-contact-candidate", Ra2VoxelSemanticRegionProvenance.ModelInferred, "lower-surface-band/1",
                lowerSide, 0.55d, "May contain wheels or tracks; requires user or vision evidence before material colouring."),
            new("upper-aperture-candidate", Ra2VoxelSemanticRegionProvenance.ModelInferred, "upper-surface-band/1",
                upperSurface, 0.35d, "May contain glass, hatches or turret details; not executable as a material mask."),
            new("protected-thin-structures", Ra2VoxelSemanticRegionProvenance.GeometryVerified, "multiaxis-thin-support/1",
                protection.ProtectedCellCount, 0.9d, "Geometry protection only; barrel or antenna identity is unresolved.")
        ];
    }

    private static Ra2VoxelSceneSnapshot CreateDerivedSnapshot(
        Ra2VoxelSceneSnapshot source,
        IEnumerable<Ra2VoxelCoordinate> coordinates,
        string derivationHash,
        string derivationName)
    {
        Dictionary<Ra2VoxelCoordinate, byte> sourcePalette = source.Cells
            .ToDictionary(cell => cell.Coordinate, cell => cell.PaletteIndex);
        byte fallback = ResolveDominantOpaquePaletteIndex(source);
        List<Ra2VoxelCell> cells = coordinates
            .OrderBy(value => value.Z)
            .ThenBy(value => value.Y)
            .ThenBy(value => value.X)
            .Select(coordinate => new Ra2VoxelCell(
                coordinate,
                sourcePalette.TryGetValue(coordinate, out byte paletteIndex)
                    ? paletteIndex
                    : ResolveAddedPaletteIndex(coordinate, sourcePalette, fallback)))
            .ToList();
        List<KeyValuePair<string, string>> hashes = source.SourceArtifactHashes.ToList();
        hashes.RemoveAll(value => string.Equals(value.Key, derivationName, StringComparison.OrdinalIgnoreCase));
        hashes.Add(new(derivationName, derivationHash));
        return new(source.SceneId, source.Part, source.Palette, cells, hashes);
    }

    internal static byte ResolveDominantOpaquePaletteIndex(Ra2VoxelSceneSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Cells
            .Where(cell => !source.Palette.IsTransparent(cell.PaletteIndex))
            .GroupBy(cell => cell.PaletteIndex)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .FirstOrDefault((byte)1);
    }

    internal static byte ResolveAddedPaletteIndex(
        Ra2VoxelCoordinate coordinate,
        IReadOnlyDictionary<Ra2VoxelCoordinate, byte> sourcePalette,
        byte fallback)
    {
        byte? sixNeighbour = MajorityPalette(FaceNeighbours(coordinate), sourcePalette);
        if (sixNeighbour.HasValue)
            return sixNeighbour.Value;

        IEnumerable<Ra2VoxelCoordinate> surrounding =
            from dz in Enumerable.Range(-1, 3)
            from dy in Enumerable.Range(-1, 3)
            from dx in Enumerable.Range(-1, 3)
            where dx != 0 || dy != 0 || dz != 0
            select new Ra2VoxelCoordinate(coordinate.X + dx, coordinate.Y + dy, coordinate.Z + dz);
        return MajorityPalette(surrounding, sourcePalette) ?? fallback;
    }

    private static byte? MajorityPalette(
        IEnumerable<Ra2VoxelCoordinate> neighbours,
        IReadOnlyDictionary<Ra2VoxelCoordinate, byte> sourcePalette) =>
        neighbours
            .Where(sourcePalette.ContainsKey)
            .Select(value => sourcePalette[value])
            .GroupBy(value => value)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => (byte?)group.Key)
            .FirstOrDefault();

    private static (int Start, int End) MapRange(int targetIndex, int targetSize, int sourceSize)
    {
        int start = Math.Clamp((int)Math.Floor(targetIndex * sourceSize / (double)targetSize), 0, sourceSize - 1);
        int end = Math.Clamp((int)Math.Ceiling((targetIndex + 1) * sourceSize / (double)targetSize) - 1, start, sourceSize - 1);
        return (start, end);
    }

    private static int FaceNeighbourCount(IReadOnlySet<Ra2VoxelCoordinate> cells, Ra2VoxelCoordinate coordinate)
        => FaceNeighbours(coordinate).Count(cells.Contains);

    private static IEnumerable<Ra2VoxelCoordinate> FaceNeighbours(Ra2VoxelCoordinate value)
    {
        yield return value with { X = value.X - 1 };
        yield return value with { X = value.X + 1 };
        yield return value with { Y = value.Y - 1 };
        yield return value with { Y = value.Y + 1 };
        yield return value with { Z = value.Z - 1 };
        yield return value with { Z = value.Z + 1 };
    }

    private static bool IsInteriorCoordinate(Ra2VoxelCoordinate value, Ra2VoxelPartDescriptor part)
        => value.X > 0 && value.X < part.XSize - 1 &&
           value.Y > 0 && value.Y < part.YSize - 1 &&
           value.Z > 0 && value.Z < part.ZSize - 1;

    private static double PercentDelta(int before, int after)
        => before == 0 ? (after == 0 ? 0d : 100d) : Math.Abs(after - before) * 100d / before;

    private static Ra2VoxelQualityRefinementResult Failure(
        Ra2VoxelRefinementFailureKind failureKind,
        string message,
        Ra2VoxelSceneSnapshot? direct = null)
        => new(failureKind, message, direct, null, null, null, null);

    private sealed record CandidateEvaluation(
        Ra2VoxelRefinementCandidateKind Kind,
        Ra2VoxelSceneSnapshot Snapshot,
        Ra2VoxelQualityAnalysisResult Analysis,
        string? Rejection);
}
