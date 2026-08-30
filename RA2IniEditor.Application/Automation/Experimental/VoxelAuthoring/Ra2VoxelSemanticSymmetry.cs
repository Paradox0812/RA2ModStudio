using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelSemanticSymmetryFailureKind
{
    None = 0,
    InvalidInput,
    EvidenceTooLarge,
    InvalidModelRound,
    InvalidProposal,
    NoSafeCandidate,
    Cancelled
}

internal enum Ra2VoxelSymmetryDisposition
{
    SymmetricCore = 0,
    AsymmetricAttachment,
    ProtectedThinFeature,
    Uncertain
}

internal sealed record Ra2VoxelSymmetrySilhouetteSummary(
    Ra2VoxelSilhouetteView View,
    int Width,
    int Height,
    string RowRuns);

internal sealed class Ra2VoxelMeshCoverageEvidence
{
    private readonly byte[] _coverage;

    private Ra2VoxelMeshCoverageEvidence(Ra2VoxelPartDescriptor part, byte[] coverage, string evidenceHash)
    {
        Part = part;
        _coverage = coverage;
        EvidenceHash = evidenceHash;
    }

    internal Ra2VoxelPartDescriptor Part { get; }
    internal string EvidenceHash { get; }

    internal int CoverageAt(Ra2VoxelCoordinate coordinate)
    {
        if (!IsInside(coordinate, Part))
            return 0;
        return _coverage[LinearIndex(coordinate, Part)];
    }

    internal static Ra2VoxelMeshCoverageEvidence Create(
        Ra2VoxelSceneSnapshot target,
        Ra2VoxelSceneSnapshot supersampled,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(supersampled);
        byte[] coverage = new byte[target.Part.MaximumCellCount];
        HashSet<Ra2VoxelCoordinate> highCells = supersampled.Cells.Select(cell => cell.Coordinate).ToHashSet();
        int linear = 0;
        for (int z = 0; z < target.Part.ZSize; z++)
        for (int y = 0; y < target.Part.YSize; y++)
        for (int x = 0; x < target.Part.XSize; x++, linear++)
        {
            if ((linear & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            (int x0, int x1) = MapRange(x, target.Part.XSize, supersampled.Part.XSize);
            (int y0, int y1) = MapRange(y, target.Part.YSize, supersampled.Part.YSize);
            (int z0, int z1) = MapRange(z, target.Part.ZSize, supersampled.Part.ZSize);
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
            coverage[linear] = checked((byte)Math.Clamp(
                (int)Math.Round(occupied * 100d / Math.Max(1, total), MidpointRounding.AwayFromZero),
                0,
                100));
        }

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)1);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, target.Part.PartId);
        writer.Write(target.Part.XSize);
        writer.Write(target.Part.YSize);
        writer.Write(target.Part.ZSize);
        writer.Write(coverage.Length);
        writer.Write(coverage);
        writer.Flush();
        string hash = Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
        return new(target.Part, coverage, hash);
    }

    private static int LinearIndex(Ra2VoxelCoordinate value, Ra2VoxelPartDescriptor part) =>
        checked(value.X + (value.Y * part.XSize) + (value.Z * part.XSize * part.YSize));

    private static bool IsInside(Ra2VoxelCoordinate value, Ra2VoxelPartDescriptor part) =>
        value.X >= 0 && value.X < part.XSize && value.Y >= 0 && value.Y < part.YSize &&
        value.Z >= 0 && value.Z < part.ZSize;

    private static (int Start, int End) MapRange(int targetIndex, int targetSize, int sourceSize)
    {
        int start = Math.Clamp((int)Math.Floor(targetIndex * sourceSize / (double)targetSize), 0, sourceSize - 1);
        int end = Math.Clamp((int)Math.Ceiling((targetIndex + 1) * sourceSize / (double)targetSize) - 1, start, sourceSize - 1);
        return (start, end);
    }
}

internal sealed class Ra2VoxelSymmetryRegionEvidence
{
    private readonly Ra2VoxelCoordinate[] _coordinates;

    internal Ra2VoxelSymmetryRegionEvidence(
        string regionId,
        IEnumerable<Ra2VoxelCoordinate> coordinates,
        int mirrorMatchCount,
        int mirrorMismatchCount,
        int frozenCellCount,
        int transitionCellCount,
        int faceContactCount,
        int branchCellCount,
        int connectedComponentCount,
        int averageCoveragePercent,
        int averageMirrorCoveragePercent,
        int mirrorTargetContactCount)
    {
        RegionId = Ra2VoxelSceneSnapshot.ValidateIdentity(regionId, nameof(regionId));
        _coordinates = coordinates.Distinct().OrderBy(value => value.Z).ThenBy(value => value.Y).ThenBy(value => value.X).ToArray();
        if (_coordinates.Length == 0)
            throw new ArgumentException("A symmetry evidence region cannot be empty.", nameof(coordinates));
        Min = new(_coordinates.Min(value => value.X), _coordinates.Min(value => value.Y), _coordinates.Min(value => value.Z));
        Max = new(_coordinates.Max(value => value.X), _coordinates.Max(value => value.Y), _coordinates.Max(value => value.Z));
        MirrorMatchCount = mirrorMatchCount;
        MirrorMismatchCount = mirrorMismatchCount;
        FrozenCellCount = frozenCellCount;
        TransitionCellCount = transitionCellCount;
        FaceContactCount = faceContactCount;
        BranchCellCount = branchCellCount;
        ConnectedComponentCount = Math.Max(1, connectedComponentCount);
        AverageCoveragePercent = Math.Clamp(averageCoveragePercent, 0, 100);
        AverageMirrorCoveragePercent = Math.Clamp(averageMirrorCoveragePercent, 0, 100);
        MirrorTargetContactCount = Math.Max(0, mirrorTargetContactCount);
    }

    internal string RegionId { get; }
    internal IReadOnlyList<Ra2VoxelCoordinate> Coordinates => Array.AsReadOnly(_coordinates);
    internal int CellCount => _coordinates.Length;
    internal Ra2VoxelCoordinate Min { get; }
    internal Ra2VoxelCoordinate Max { get; }
    internal int SpanX => Max.X - Min.X + 1;
    internal int SpanY => Max.Y - Min.Y + 1;
    internal int SpanZ => Max.Z - Min.Z + 1;
    internal int MirrorMatchCount { get; }
    internal int MirrorMismatchCount { get; }
    internal int FrozenCellCount { get; }
    internal int TransitionCellCount { get; }
    internal int FaceContactCount { get; }
    internal int BranchCellCount { get; }
    internal int ConnectedComponentCount { get; }
    internal int AverageCoveragePercent { get; }
    internal int AverageMirrorCoveragePercent { get; }
    internal int MirrorTargetContactCount { get; }
}

internal sealed class Ra2VoxelCenterSeamGapEvidence
{
    private readonly Ra2VoxelCoordinate[] _missingCoordinates;
    private readonly Ra2VoxelCoordinate[] _anchorCoordinates;

    internal Ra2VoxelCenterSeamGapEvidence(
        string targetId,
        IEnumerable<Ra2VoxelCoordinate> missingCoordinates,
        IEnumerable<Ra2VoxelCoordinate> anchorCoordinates,
        int gapLineCount,
        int oneCellGapLineCount,
        int twoCellGapLineCount,
        int connectedComponentCount,
        int averageCoveragePercent,
        int minimumCoveragePercent,
        int averageAnchorCoveragePercent,
        int protectedAnchorCount,
        int faceSupportCount)
    {
        TargetId = Ra2VoxelSceneSnapshot.ValidateIdentity(targetId, nameof(targetId));
        _missingCoordinates = missingCoordinates.Distinct()
            .OrderBy(value => value.Z).ThenBy(value => value.Y).ThenBy(value => value.X).ToArray();
        _anchorCoordinates = anchorCoordinates.Distinct()
            .OrderBy(value => value.Z).ThenBy(value => value.Y).ThenBy(value => value.X).ToArray();
        if (_missingCoordinates.Length == 0 || _anchorCoordinates.Length == 0)
            throw new ArgumentException("A center-seam gap target requires missing cells and occupied anchors.");
        Min = new(_missingCoordinates.Min(value => value.X), _missingCoordinates.Min(value => value.Y), _missingCoordinates.Min(value => value.Z));
        Max = new(_missingCoordinates.Max(value => value.X), _missingCoordinates.Max(value => value.Y), _missingCoordinates.Max(value => value.Z));
        GapLineCount = Math.Max(1, gapLineCount);
        OneCellGapLineCount = Math.Max(0, oneCellGapLineCount);
        TwoCellGapLineCount = Math.Max(0, twoCellGapLineCount);
        ConnectedComponentCount = Math.Max(1, connectedComponentCount);
        AverageCoveragePercent = Math.Clamp(averageCoveragePercent, 0, 100);
        MinimumCoveragePercent = Math.Clamp(minimumCoveragePercent, 0, 100);
        AverageAnchorCoveragePercent = Math.Clamp(averageAnchorCoveragePercent, 0, 100);
        ProtectedAnchorCount = Math.Max(0, protectedAnchorCount);
        FaceSupportCount = Math.Max(0, faceSupportCount);
    }

    internal string TargetId { get; }
    internal IReadOnlyList<Ra2VoxelCoordinate> MissingCoordinates => Array.AsReadOnly(_missingCoordinates);
    internal IReadOnlyList<Ra2VoxelCoordinate> AnchorCoordinates => Array.AsReadOnly(_anchorCoordinates);
    internal int MissingCellCount => _missingCoordinates.Length;
    internal Ra2VoxelCoordinate Min { get; }
    internal Ra2VoxelCoordinate Max { get; }
    internal int GapLineCount { get; }
    internal int OneCellGapLineCount { get; }
    internal int TwoCellGapLineCount { get; }
    internal int ConnectedComponentCount { get; }
    internal int AverageCoveragePercent { get; }
    internal int MinimumCoveragePercent { get; }
    internal int AverageAnchorCoveragePercent { get; }
    internal int ProtectedAnchorCount { get; }
    internal int FaceSupportCount { get; }
}

internal sealed class Ra2VoxelSymmetryEvidencePackage
{
    internal const int MaximumPromptCharacters = 32_768;
    internal const int MaximumEvidenceCharacters = 22_000;
    internal const int MaximumRegions = 64;
    internal const int MaximumCenterSeamTargets = 24;

    internal Ra2VoxelSymmetryEvidencePackage(
        string sourceSnapshotHash,
        string profileHash,
        string coverageEvidenceHash,
        int selectedPlaneTwiceX,
        IEnumerable<int> alternativePlanesTwiceX,
        IEnumerable<Ra2VoxelSymmetrySilhouetteSummary> silhouettes,
        IEnumerable<Ra2VoxelSymmetryRegionEvidence> regions,
        IEnumerable<Ra2VoxelCenterSeamGapEvidence>? centerSeamGaps = null)
    {
        SourceSnapshotHash = sourceSnapshotHash;
        ProfileHash = profileHash;
        CoverageEvidenceHash = coverageEvidenceHash;
        SelectedPlaneTwiceX = selectedPlaneTwiceX;
        AlternativePlanesTwiceX = Array.AsReadOnly(alternativePlanesTwiceX.Distinct().OrderBy(value => value).ToArray());
        Silhouettes = Array.AsReadOnly(silhouettes.OrderBy(value => value.View).ToArray());
        Regions = Array.AsReadOnly(regions.OrderBy(value => value.RegionId, StringComparer.Ordinal).ToArray());
        CenterSeamGaps = Array.AsReadOnly((centerSeamGaps ?? [])
            .OrderBy(value => value.TargetId, StringComparer.Ordinal).ToArray());
        if (Regions.Count is < 1 or > MaximumRegions || Regions.Select(value => value.RegionId).Distinct(StringComparer.Ordinal).Count() != Regions.Count)
            throw new ArgumentException("A symmetry evidence package must contain unique bounded regions.", nameof(regions));
        if (CenterSeamGaps.Count > MaximumCenterSeamTargets ||
            CenterSeamGaps.Select(value => value.TargetId).Distinct(StringComparer.Ordinal).Count() != CenterSeamGaps.Count ||
            CenterSeamGaps.Any(gap => Regions.Any(region => string.Equals(region.RegionId, gap.TargetId, StringComparison.Ordinal))))
            throw new ArgumentException("A symmetry evidence package must contain unique bounded center-seam targets.", nameof(centerSeamGaps));
        if (Silhouettes.Count != 6)
            throw new ArgumentException("A symmetry evidence package requires six silhouettes.", nameof(silhouettes));
        PackageHash = ComputeHash();
    }

    internal string SourceSnapshotHash { get; }
    internal string ProfileHash { get; }
    internal string CoverageEvidenceHash { get; }
    internal int SelectedPlaneTwiceX { get; }
    internal IReadOnlyList<int> AlternativePlanesTwiceX { get; }
    internal IReadOnlyList<Ra2VoxelSymmetrySilhouetteSummary> Silhouettes { get; }
    internal IReadOnlyList<Ra2VoxelSymmetryRegionEvidence> Regions { get; }
    internal IReadOnlyList<Ra2VoxelCenterSeamGapEvidence> CenterSeamGaps { get; }
    internal string PackageHash { get; }

    internal string ToPromptText()
    {
        StringBuilder builder = new();
        builder.AppendLine($"evidence_hash={PackageHash}");
        builder.AppendLine($"source_hash={SourceSnapshotHash}");
        builder.AppendLine($"profile_hash={ProfileHash}");
        builder.AppendLine($"coverage_hash={CoverageEvidenceHash}");
        builder.AppendLine($"selected_plane_twice_x={SelectedPlaneTwiceX}");
        builder.AppendLine($"alternative_planes_twice_x={string.Join(',', AlternativePlanesTwiceX)}");
        builder.AppendLine("silhouettes:");
        foreach (Ra2VoxelSymmetrySilhouetteSummary silhouette in Silhouettes)
            builder.AppendLine($"{silhouette.View}|{silhouette.Width}x{silhouette.Height}|{silhouette.RowRuns}");
        builder.AppendLine("regions:");
        foreach (Ra2VoxelSymmetryRegionEvidence region in Regions)
        {
            builder.AppendLine(
                $"{region.RegionId}|cells={region.CellCount}|bbox={region.Min.X},{region.Min.Y},{region.Min.Z}-" +
                $"{region.Max.X},{region.Max.Y},{region.Max.Z}|span={region.SpanX},{region.SpanY},{region.SpanZ}|" +
                $"mirror={region.MirrorMatchCount},{region.MirrorMismatchCount}|protected={region.FrozenCellCount}," +
                $"{region.TransitionCellCount}|contact={region.FaceContactCount}|branch={region.BranchCellCount}|" +
                $"components={region.ConnectedComponentCount}|coverage={region.AverageCoveragePercent}|" +
                $"mirror_coverage={region.AverageMirrorCoveragePercent}|mirror_contact={region.MirrorTargetContactCount}");
        }
        if (CenterSeamGaps.Count > 0)
        {
            builder.AppendLine("center_seam_gaps:");
            foreach (Ra2VoxelCenterSeamGapEvidence gap in CenterSeamGaps)
            {
                builder.AppendLine(
                    $"{gap.TargetId}|empty_cells={gap.MissingCellCount}|lines={gap.GapLineCount}|" +
                    $"widths={gap.OneCellGapLineCount},{gap.TwoCellGapLineCount}|" +
                    $"bbox={gap.Min.X},{gap.Min.Y},{gap.Min.Z}-{gap.Max.X},{gap.Max.Y},{gap.Max.Z}|" +
                    $"components={gap.ConnectedComponentCount}|coverage={gap.AverageCoveragePercent},{gap.MinimumCoveragePercent}|" +
                    $"anchor_coverage={gap.AverageAnchorCoveragePercent}|protected_anchors={gap.ProtectedAnchorCount}|" +
                    $"face_support={gap.FaceSupportCount}");
            }
        }
        return builder.ToString();
    }

    private string ComputeHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)3);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, SourceSnapshotHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, ProfileHash);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, CoverageEvidenceHash);
        writer.Write(SelectedPlaneTwiceX);
        foreach (int plane in AlternativePlanesTwiceX) writer.Write(plane);
        foreach (Ra2VoxelSymmetrySilhouetteSummary silhouette in Silhouettes)
        {
            writer.Write((int)silhouette.View);
            writer.Write(silhouette.Width);
            writer.Write(silhouette.Height);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, silhouette.RowRuns);
        }
        foreach (Ra2VoxelSymmetryRegionEvidence region in Regions)
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, region.RegionId);
            foreach (Ra2VoxelCoordinate coordinate in region.Coordinates)
            {
                writer.Write(coordinate.X);
                writer.Write(coordinate.Y);
                writer.Write(coordinate.Z);
            }
            writer.Write(region.MirrorMatchCount);
            writer.Write(region.MirrorMismatchCount);
            writer.Write(region.FrozenCellCount);
            writer.Write(region.TransitionCellCount);
            writer.Write(region.FaceContactCount);
            writer.Write(region.BranchCellCount);
            writer.Write(region.ConnectedComponentCount);
            writer.Write(region.AverageCoveragePercent);
            writer.Write(region.AverageMirrorCoveragePercent);
            writer.Write(region.MirrorTargetContactCount);
        }
        foreach (Ra2VoxelCenterSeamGapEvidence gap in CenterSeamGaps)
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, gap.TargetId);
            foreach (Ra2VoxelCoordinate coordinate in gap.MissingCoordinates)
            {
                writer.Write(coordinate.X);
                writer.Write(coordinate.Y);
                writer.Write(coordinate.Z);
            }
            foreach (Ra2VoxelCoordinate coordinate in gap.AnchorCoordinates)
            {
                writer.Write(coordinate.X);
                writer.Write(coordinate.Y);
                writer.Write(coordinate.Z);
            }
            writer.Write(gap.GapLineCount);
            writer.Write(gap.OneCellGapLineCount);
            writer.Write(gap.TwoCellGapLineCount);
            writer.Write(gap.ConnectedComponentCount);
            writer.Write(gap.AverageCoveragePercent);
            writer.Write(gap.MinimumCoveragePercent);
            writer.Write(gap.AverageAnchorCoveragePercent);
            writer.Write(gap.ProtectedAnchorCount);
            writer.Write(gap.FaceSupportCount);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }
}

internal sealed record Ra2VoxelSymmetryEvidenceResult(
    Ra2VoxelSemanticSymmetryFailureKind FailureKind,
    string Message,
    Ra2VoxelSymmetryEvidencePackage? Package)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticSymmetryFailureKind.None && Package is not null;
}

internal static class Ra2VoxelSymmetryEvidenceBuilder
{
    internal static Ra2VoxelSymmetryEvidenceResult Build(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSceneSnapshot protectionReference,
        Ra2VoxelFeatureProtectionMask protection,
        Ra2VoxelMeshCoverageEvidence coverage,
        Ra2VoxelRefinementProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(protectionReference);
        ArgumentNullException.ThrowIfNull(protection);
        ArgumentNullException.ThrowIfNull(coverage);
        profile ??= new();
        try
        {
            if (!string.Equals(protection.SourceSnapshotHash, protectionReference.CanonicalHash, StringComparison.Ordinal) ||
                source.Part.XSize != protectionReference.Part.XSize || source.Part.YSize != protectionReference.Part.YSize ||
                source.Part.ZSize != protectionReference.Part.ZSize || source.Part.XSize != coverage.Part.XSize ||
                source.Part.YSize != coverage.Part.YSize || source.Part.ZSize != coverage.Part.ZSize)
            {
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, "Symmetry evidence inputs do not share one geometry frame.");
            }

            HashSet<Ra2VoxelCoordinate> occupied = source.Cells.Select(cell => cell.Coordinate).ToHashSet();
            if (occupied.Count == 0)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, "Symmetry evidence requires occupied geometry.");
            Dictionary<Ra2VoxelCoordinate, Ra2VoxelRefinementZone> zones = new();
            for (int index = 0; index < protectionReference.Cells.Count; index++)
                zones[protectionReference.Cells[index].Coordinate] = protection.ZoneAt(index);

            int medianX = occupied.Select(value => value.X).OrderBy(value => value).ElementAt(occupied.Count / 2);
            int[] planes = Enumerable.Range(-2, 5)
                .Select(offset => Math.Clamp((medianX * 2) + offset, 0, (source.Part.XSize - 1) * 2))
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            int selectedPlane = planes
                .Select(plane => ScorePlane(plane, medianX * 2, occupied, zones, coverage, source.Part))
                .OrderBy(value => value.ProtectedMismatch)
                .ThenBy(value => value.TotalMismatch)
                .ThenBy(value => value.CoverageResidual)
                .ThenBy(value => value.MedianDistance)
                .ThenBy(value => value.Plane)
                .First().Plane;

            HashSet<Ra2VoxelCoordinate> protectedCoordinates = occupied
                .Where(value => zones.TryGetValue(value, out Ra2VoxelRefinementZone zone) && zone != Ra2VoxelRefinementZone.Smoothable)
                .ToHashSet();
            HashSet<Ra2VoxelCoordinate> mismatchCoordinates = occupied
                .Where(value => !occupied.Contains(Mirror(value, selectedPlane)))
                .Except(protectedCoordinates)
                .ToHashSet();
            HashSet<Ra2VoxelCoordinate> coreCoordinates = occupied
                .Except(protectedCoordinates)
                .Except(mismatchCoordinates)
                .ToHashSet();

            List<(string Prefix, HashSet<Ra2VoxelCoordinate> Cells, int ComponentCount)> groups = [];
            if (coreCoordinates.Count > 0)
                groups.Add(("core", coreCoordinates, CountComponents(coreCoordinates, cancellationToken)));

            List<HashSet<Ra2VoxelCoordinate>> protectedComponents = Components(protectedCoordinates, cancellationToken).ToList();
            if (protectedComponents.Count > 0)
            {
                groups.Add((
                    "protected",
                    protectedComponents.SelectMany(value => value).ToHashSet(),
                    protectedComponents.Count));
            }

            List<HashSet<Ra2VoxelCoordinate>> repairComponents = Components(mismatchCoordinates, cancellationToken).ToList();
            groups.AddRange(repairComponents
                .GroupBy(value => BuildRepairBucket(value, selectedPlane, source.Part, occupied), StringComparer.Ordinal)
                .OrderBy(value => value.Key, StringComparer.Ordinal)
                .Select(value => (
                    Prefix: $"repair-{value.Key}",
                    Cells: value.SelectMany(component => component).ToHashSet(),
                    ComponentCount: value.Count())));
            if (groups.Count is < 1 or > Ra2VoxelSymmetryEvidencePackage.MaximumRegions)
            {
                return Failure(
                    Ra2VoxelSemanticSymmetryFailureKind.EvidenceTooLarge,
                    $"Symmetry evidence contains {groups.Count} bounded regions; the limit is {Ra2VoxelSymmetryEvidencePackage.MaximumRegions}.");
            }

            Dictionary<string, int> counters = new(StringComparer.Ordinal);
            List<Ra2VoxelSymmetryRegionEvidence> regions = [];
            foreach ((string prefix, HashSet<Ra2VoxelCoordinate> cells, int componentCount) in groups
                         .OrderBy(value => value.Prefix, StringComparer.Ordinal)
                         .ThenBy(value => value.Cells.Min(cell => cell.Z))
                         .ThenBy(value => value.Cells.Min(cell => cell.Y))
                         .ThenBy(value => value.Cells.Min(cell => cell.X)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                int number = counters.TryGetValue(prefix, out int current) ? current + 1 : 1;
                counters[prefix] = number;
                string id = $"{prefix}-{number:000}";
                int matched = cells.Count(value => occupied.Contains(Mirror(value, selectedPlane)));
                int frozen = cells.Count(value => zones.TryGetValue(value, out Ra2VoxelRefinementZone zone) && zone == Ra2VoxelRefinementZone.Frozen);
                int transition = cells.Count(value => zones.TryGetValue(value, out Ra2VoxelRefinementZone zone) && zone == Ra2VoxelRefinementZone.Transition);
                int contact = cells.Sum(value => FaceNeighbours(value).Count(neighbour => occupied.Contains(neighbour) && !cells.Contains(neighbour)));
                int branch = cells.Count(value => FaceNeighbours(value).Count(cells.Contains) >= 3);
                int averageCoverage = checked((int)Math.Round(cells.Average(coverage.CoverageAt), MidpointRounding.AwayFromZero));
                int averageMirrorCoverage = checked((int)Math.Round(cells.Average(value =>
                {
                    Ra2VoxelCoordinate mirror = Mirror(value, selectedPlane);
                    return IsInside(mirror, source.Part) ? coverage.CoverageAt(mirror) : 0;
                }), MidpointRounding.AwayFromZero));
                int mirrorTargetContact = cells.Sum(value =>
                {
                    Ra2VoxelCoordinate mirror = Mirror(value, selectedPlane);
                    return IsInside(mirror, source.Part)
                        ? FaceNeighbours(mirror).Count(neighbour => occupied.Contains(neighbour))
                        : 0;
                });
                regions.Add(new(
                    id,
                    cells,
                    matched,
                    cells.Count - matched,
                    frozen,
                    transition,
                    contact,
                    branch,
                    componentCount,
                    averageCoverage,
                    averageMirrorCoverage,
                    mirrorTargetContact));
            }

            IReadOnlyList<Ra2VoxelCenterSeamGapEvidence> centerSeamGaps = BuildCenterSeamGaps(
                selectedPlane,
                source.Part,
                occupied,
                protectedCoordinates,
                coverage,
                profile,
                cancellationToken);

            Ra2VoxelSymmetryEvidencePackage package = new(
                source.CanonicalHash,
                profile.ProfileHash,
                coverage.EvidenceHash,
                selectedPlane,
                planes,
                BuildSilhouettes(occupied, source.Part),
                regions,
                centerSeamGaps);
            if (package.ToPromptText().Length > Ra2VoxelSymmetryEvidencePackage.MaximumEvidenceCharacters)
            {
                return Failure(
                    Ra2VoxelSemanticSymmetryFailureKind.EvidenceTooLarge,
                    $"Compacted symmetry evidence uses {package.ToPromptText().Length} characters; the evidence limit is {Ra2VoxelSymmetryEvidencePackage.MaximumEvidenceCharacters}.");
            }
            return new(Ra2VoxelSemanticSymmetryFailureKind.None, string.Empty, package);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.Cancelled, "Symmetry evidence generation was cancelled.");
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, exception.Message);
        }
    }

    private static (int Plane, int ProtectedMismatch, int TotalMismatch, int CoverageResidual, int MedianDistance) ScorePlane(
        int plane,
        int medianPlane,
        IReadOnlySet<Ra2VoxelCoordinate> occupied,
        IReadOnlyDictionary<Ra2VoxelCoordinate, Ra2VoxelRefinementZone> zones,
        Ra2VoxelMeshCoverageEvidence coverage,
        Ra2VoxelPartDescriptor part)
    {
        int protectedMismatch = 0;
        int totalMismatch = 0;
        int residual = 0;
        foreach (Ra2VoxelCoordinate coordinate in occupied)
        {
            Ra2VoxelCoordinate mirror = Mirror(coordinate, plane);
            bool matched = IsInside(mirror, part) && occupied.Contains(mirror);
            if (!matched)
            {
                totalMismatch++;
                if (zones.TryGetValue(coordinate, out Ra2VoxelRefinementZone zone) && zone != Ra2VoxelRefinementZone.Smoothable)
                    protectedMismatch++;
            }
            if (IsInside(mirror, part))
                residual += Math.Abs(coverage.CoverageAt(coordinate) - coverage.CoverageAt(mirror));
        }
        return (plane, protectedMismatch, totalMismatch, residual, Math.Abs(plane - medianPlane));
    }

    private static IReadOnlyList<Ra2VoxelSymmetrySilhouetteSummary> BuildSilhouettes(
        IReadOnlySet<Ra2VoxelCoordinate> occupied,
        Ra2VoxelPartDescriptor part)
    {
        List<Ra2VoxelSymmetrySilhouetteSummary> result = [];
        foreach (Ra2VoxelSilhouetteView view in Enum.GetValues<Ra2VoxelSilhouetteView>())
        {
            (int sourceWidth, int sourceHeight) = view switch
            {
                Ra2VoxelSilhouetteView.Front or Ra2VoxelSilhouetteView.Rear => (part.XSize, part.ZSize),
                Ra2VoxelSilhouetteView.Left or Ra2VoxelSilhouetteView.Right => (part.YSize, part.ZSize),
                _ => (part.XSize, part.YSize)
            };
            int width = Math.Min(32, sourceWidth);
            int height = Math.Min(32, sourceHeight);
            bool[,] pixels = new bool[height, width];
            foreach (Ra2VoxelCoordinate coordinate in occupied)
            {
                (int u, int v) = Project(coordinate, view, part);
                int px = Math.Min(width - 1, u * width / sourceWidth);
                int py = Math.Min(height - 1, v * height / sourceHeight);
                pixels[py, px] = true;
            }
            StringBuilder runs = new();
            for (int row = 0; row < height; row++)
            {
                if (row > 0) runs.Append(';');
                bool state = pixels[row, 0];
                int count = 1;
                for (int column = 1; column < width; column++)
                {
                    if (pixels[row, column] == state) count++;
                    else
                    {
                        runs.Append(state ? '1' : '0').Append('x').Append(count).Append(',');
                        state = pixels[row, column];
                        count = 1;
                    }
                }
                runs.Append(state ? '1' : '0').Append('x').Append(count);
            }
            result.Add(new(view, width, height, runs.ToString()));
        }
        return result;
    }

    private static (int U, int V) Project(Ra2VoxelCoordinate value, Ra2VoxelSilhouetteView view, Ra2VoxelPartDescriptor part) => view switch
    {
        Ra2VoxelSilhouetteView.Front => (value.X, part.ZSize - 1 - value.Z),
        Ra2VoxelSilhouetteView.Rear => (part.XSize - 1 - value.X, part.ZSize - 1 - value.Z),
        Ra2VoxelSilhouetteView.Left => (value.Y, part.ZSize - 1 - value.Z),
        Ra2VoxelSilhouetteView.Right => (part.YSize - 1 - value.Y, part.ZSize - 1 - value.Z),
        Ra2VoxelSilhouetteView.Top => (value.X, value.Y),
        Ra2VoxelSilhouetteView.Bottom => (value.X, part.YSize - 1 - value.Y),
        _ => throw new ArgumentOutOfRangeException(nameof(view))
    };

    internal static IEnumerable<HashSet<Ra2VoxelCoordinate>> Components(
        HashSet<Ra2VoxelCoordinate> input,
        CancellationToken cancellationToken)
    {
        HashSet<Ra2VoxelCoordinate> remaining = new(input);
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
                    if (remaining.Remove(neighbour)) queue.Enqueue(neighbour);
            }
            yield return component;
        }
    }

    private static int CountComponents(
        HashSet<Ra2VoxelCoordinate> coordinates,
        CancellationToken cancellationToken) =>
        Components(coordinates, cancellationToken).Count();

    private static string BuildRepairBucket(
        HashSet<Ra2VoxelCoordinate> component,
        int planeTwiceX,
        Ra2VoxelPartDescriptor part,
        IReadOnlySet<Ra2VoxelCoordinate> occupied)
    {
        long twiceXSum = component.Sum(value => (long)value.X * 2L);
        long planeSum = (long)planeTwiceX * component.Count;
        string lateral = twiceXSum < planeSum ? "left" : "right";
        string height = Third(component.Sum(value => (long)value.Z), component.Count, part.ZSize, "lower", "middle", "upper");
        string depth = component.Sum(value => (long)value.Y) * 2L < (long)component.Count * part.YSize ? "front" : "rear";
        int spanX = component.Max(value => value.X) - component.Min(value => value.X) + 1;
        int spanY = component.Max(value => value.Y) - component.Min(value => value.Y) + 1;
        int spanZ = component.Max(value => value.Z) - component.Min(value => value.Z) + 1;
        int minimumSpan = Math.Min(spanX, Math.Min(spanY, spanZ));
        int maximumSpan = Math.Max(spanX, Math.Max(spanY, spanZ));
        bool isThin = minimumSpan <= 2 && maximumSpan >= 3;
        bool hasBodyContact = component.Any(value => FaceNeighbours(value).Any(neighbour => occupied.Contains(neighbour) && !component.Contains(neighbour)));
        string morphology = !hasBodyContact
            ? "detached"
            : isThin
                ? "slender"
                : component.Count <= 8
                    ? "compact"
                    : "broad";
        return $"{lateral}-{height}-{depth}-{morphology}";
    }

    private static IReadOnlyList<Ra2VoxelCenterSeamGapEvidence> BuildCenterSeamGaps(
        int planeTwiceX,
        Ra2VoxelPartDescriptor part,
        IReadOnlySet<Ra2VoxelCoordinate> occupied,
        IReadOnlySet<Ra2VoxelCoordinate> protectedCoordinates,
        Ra2VoxelMeshCoverageEvidence coverage,
        Ra2VoxelRefinementProfile profile,
        CancellationToken cancellationToken)
    {
        List<CenterSeamLine> lines = [];
        for (int z = 0; z < part.ZSize; z++)
        for (int y = 0; y < part.YSize; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((planeTwiceX & 1) == 0)
            {
                int centerX = planeTwiceX / 2;
                Ra2VoxelCoordinate missing = new(centerX, y, z);
                Ra2VoxelCoordinate left = new(centerX - 1, y, z);
                Ra2VoxelCoordinate right = new(centerX + 1, y, z);
                if (IsInside(left, part) && IsInside(right, part) && !occupied.Contains(missing) &&
                    occupied.Contains(left) && occupied.Contains(right))
                    lines.Add(new([missing], left, right));
            }
            else
            {
                int leftMissingX = planeTwiceX / 2;
                Ra2VoxelCoordinate leftMissing = new(leftMissingX, y, z);
                Ra2VoxelCoordinate rightMissing = new(leftMissingX + 1, y, z);
                Ra2VoxelCoordinate left = new(leftMissingX - 1, y, z);
                Ra2VoxelCoordinate right = new(leftMissingX + 2, y, z);
                if (IsInside(left, part) && IsInside(right, part) && !occupied.Contains(leftMissing) &&
                    !occupied.Contains(rightMissing) && occupied.Contains(left) && occupied.Contains(right))
                    lines.Add(new([leftMissing, rightMissing], left, right));
            }
        }
        if (lines.Count == 0)
            return [];

        HashSet<Ra2VoxelCoordinate> allMissing = lines.SelectMany(value => value.Missing).ToHashSet();
        List<HashSet<Ra2VoxelCoordinate>> components = Components(allMissing, cancellationToken).ToList();
        List<IReadOnlyList<CenterSeamLine>> targetGroups;
        if (components.Count <= Ra2VoxelSymmetryEvidencePackage.MaximumCenterSeamTargets)
        {
            targetGroups = components
                .Select(component => (IReadOnlyList<CenterSeamLine>)lines
                    .Where(line => line.Missing.Any(component.Contains))
                    .OrderBy(line => line.Missing[0].Z).ThenBy(line => line.Missing[0].Y).ThenBy(line => line.Missing[0].X)
                    .ToArray())
                .ToList();
        }
        else
        {
            targetGroups = lines
                .GroupBy(line => BuildCenterSeamBucket(line, part, coverage, profile.MinimumCoveragePercent), StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => (IReadOnlyList<CenterSeamLine>)group
                    .OrderBy(line => line.Missing[0].Z).ThenBy(line => line.Missing[0].Y).ThenBy(line => line.Missing[0].X)
                    .ToArray())
                .ToList();
        }

        List<Ra2VoxelCenterSeamGapEvidence> result = [];
        int number = 0;
        foreach (IReadOnlyList<CenterSeamLine> group in targetGroups
                     .OrderBy(value => value.SelectMany(line => line.Missing).Min(coordinate => coordinate.Z))
                     .ThenBy(value => value.SelectMany(line => line.Missing).Min(coordinate => coordinate.Y))
                     .ThenBy(value => value.SelectMany(line => line.Missing).Min(coordinate => coordinate.X)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate[] missing = group.SelectMany(value => value.Missing).Distinct().ToArray();
            Ra2VoxelCoordinate[] anchors = group.SelectMany(value => new[] { value.LeftAnchor, value.RightAnchor }).Distinct().ToArray();
            int averageCoverage = checked((int)Math.Round(missing.Average(coverage.CoverageAt), MidpointRounding.AwayFromZero));
            int averageAnchorCoverage = checked((int)Math.Round(anchors.Average(coverage.CoverageAt), MidpointRounding.AwayFromZero));
            int componentCount = Components(missing.ToHashSet(), cancellationToken).Count();
            result.Add(new(
                $"seam-gap-{++number:000}",
                missing,
                anchors,
                group.Count,
                group.Count(value => value.Missing.Count == 1),
                group.Count(value => value.Missing.Count == 2),
                componentCount,
                averageCoverage,
                missing.Min(coverage.CoverageAt),
                averageAnchorCoverage,
                anchors.Count(protectedCoordinates.Contains),
                missing.Sum(value => FaceNeighbours(value).Count(occupied.Contains))));
        }
        return Array.AsReadOnly(result.ToArray());
    }

    private static string BuildCenterSeamBucket(
        CenterSeamLine line,
        Ra2VoxelPartDescriptor part,
        Ra2VoxelMeshCoverageEvidence coverage,
        int minimumCoveragePercent)
    {
        Ra2VoxelCoordinate first = line.Missing[0];
        string width = line.Missing.Count == 1 ? "one" : "two";
        string height = Third(first.Z, 1, part.ZSize, "lower", "middle", "upper");
        string depth = first.Y * 2 < part.YSize ? "front" : "rear";
        string support = line.Missing.Average(coverage.CoverageAt) >= minimumCoveragePercent ? "supported" : "weak";
        return $"{width}-{height}-{depth}-{support}";
    }

    private sealed record CenterSeamLine(
        IReadOnlyList<Ra2VoxelCoordinate> Missing,
        Ra2VoxelCoordinate LeftAnchor,
        Ra2VoxelCoordinate RightAnchor);

    private static string Third(
        long coordinateSum,
        int cellCount,
        int dimension,
        string first,
        string middle,
        string last)
    {
        long scaled = coordinateSum * 3L;
        long denominator = Math.Max(1L, (long)cellCount * dimension);
        int band = Math.Clamp((int)(scaled / denominator), 0, 2);
        return band switch
        {
            0 => first,
            1 => middle,
            _ => last
        };
    }

    internal static Ra2VoxelCoordinate Mirror(Ra2VoxelCoordinate value, int planeTwiceX) =>
        value with { X = planeTwiceX - value.X };

    internal static IEnumerable<Ra2VoxelCoordinate> MooreNeighbours(Ra2VoxelCoordinate value)
    {
        for (int dz = -1; dz <= 1; dz++)
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
            if (dx != 0 || dy != 0 || dz != 0) yield return new(value.X + dx, value.Y + dy, value.Z + dz);
    }

    internal static IEnumerable<Ra2VoxelCoordinate> FaceNeighbours(Ra2VoxelCoordinate value)
    {
        yield return value with { X = value.X - 1 };
        yield return value with { X = value.X + 1 };
        yield return value with { Y = value.Y - 1 };
        yield return value with { Y = value.Y + 1 };
        yield return value with { Z = value.Z - 1 };
        yield return value with { Z = value.Z + 1 };
    }

    internal static bool IsInside(Ra2VoxelCoordinate value, Ra2VoxelPartDescriptor part) =>
        value.X >= 0 && value.X < part.XSize && value.Y >= 0 && value.Y < part.YSize && value.Z >= 0 && value.Z < part.ZSize;

    private static Ra2VoxelSymmetryEvidenceResult Failure(Ra2VoxelSemanticSymmetryFailureKind kind, string message) =>
        new(kind, message, null);
}

internal sealed record Ra2VoxelSymmetryModelRegionDecision(
    string RegionId,
    Ra2VoxelSymmetryDisposition Disposition,
    double Confidence,
    string Reason);

internal sealed class Ra2VoxelSymmetryModelRound
{
    internal Ra2VoxelSymmetryModelRound(
        string evidencePackageHash,
        int reviewedPlaneTwiceX,
        IEnumerable<Ra2VoxelSymmetryModelRegionDecision> decisions,
        IEnumerable<string>? unresolvedAssumptions = null)
    {
        EvidencePackageHash = evidencePackageHash ?? string.Empty;
        ReviewedPlaneTwiceX = reviewedPlaneTwiceX;
        Decisions = Array.AsReadOnly(decisions.ToArray());
        UnresolvedAssumptions = Array.AsReadOnly((unresolvedAssumptions ?? []).Select(value => value.Trim()).Where(value => value.Length > 0).ToArray());
    }

    internal string EvidencePackageHash { get; }
    internal int ReviewedPlaneTwiceX { get; }
    internal IReadOnlyList<Ra2VoxelSymmetryModelRegionDecision> Decisions { get; }
    internal IReadOnlyList<string> UnresolvedAssumptions { get; }
}

internal sealed record Ra2VoxelSemanticRegionDecision(
    string RegionId,
    Ra2VoxelSymmetryDisposition Disposition,
    double RoundOneConfidence,
    double RoundTwoConfidence,
    string ReviewReason,
    bool RoundsAgree);

internal sealed class Ra2VoxelSemanticPartition
{
    private readonly Dictionary<Ra2VoxelCoordinate, Ra2VoxelSymmetryDisposition> _dispositions;

    internal Ra2VoxelSemanticPartition(
        Ra2VoxelSymmetryEvidencePackage evidence,
        IEnumerable<Ra2VoxelSemanticRegionDecision> decisions,
        IReadOnlyDictionary<Ra2VoxelCoordinate, Ra2VoxelSymmetryDisposition>? coordinateOverrides = null,
        bool expandUncertainBoundary = true)
    {
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        Decisions = Array.AsReadOnly(decisions.OrderBy(value => value.RegionId, StringComparer.Ordinal).ToArray());
        Dictionary<string, Ra2VoxelSemanticRegionDecision> byRegion = Decisions.ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        if (byRegion.Count != Evidence.Regions.Count || Evidence.Regions.Any(region => !byRegion.ContainsKey(region.RegionId)))
            throw new ArgumentException("A semantic partition must decide every evidence region exactly once.", nameof(decisions));
        _dispositions = new();
        foreach (Ra2VoxelSymmetryRegionEvidence region in Evidence.Regions)
            foreach (Ra2VoxelCoordinate coordinate in region.Coordinates)
                _dispositions.Add(coordinate, byRegion[region.RegionId].Disposition);
        if (coordinateOverrides is not null)
            foreach ((Ra2VoxelCoordinate coordinate, Ra2VoxelSymmetryDisposition disposition) in coordinateOverrides)
                if (_dispositions.ContainsKey(coordinate)) _dispositions[coordinate] = disposition;

        if (expandUncertainBoundary)
        {
            HashSet<Ra2VoxelCoordinate> nonCore = _dispositions.Where(pair => pair.Value != Ra2VoxelSymmetryDisposition.SymmetricCore)
                .Select(pair => pair.Key).ToHashSet();
            foreach (Ra2VoxelCoordinate coordinate in nonCore.SelectMany(Ra2VoxelSymmetryEvidenceBuilder.MooreNeighbours).Distinct())
                if (_dispositions.TryGetValue(coordinate, out Ra2VoxelSymmetryDisposition disposition) && disposition == Ra2VoxelSymmetryDisposition.SymmetricCore)
                    _dispositions[coordinate] = Ra2VoxelSymmetryDisposition.Uncertain;
        }
        PartitionHash = ComputeHash();
    }

    internal Ra2VoxelSymmetryEvidencePackage Evidence { get; }
    internal IReadOnlyList<Ra2VoxelSemanticRegionDecision> Decisions { get; }
    internal string PartitionHash { get; }
    internal int UncertainCellCount => _dispositions.Count(value => value.Value == Ra2VoxelSymmetryDisposition.Uncertain);

    internal Ra2VoxelSymmetryDisposition DispositionAt(Ra2VoxelCoordinate coordinate) =>
        _dispositions.TryGetValue(coordinate, out Ra2VoxelSymmetryDisposition value) ? value : Ra2VoxelSymmetryDisposition.Uncertain;

    internal IReadOnlyList<Ra2VoxelCoordinate> CoordinatesFor(Ra2VoxelSymmetryDisposition disposition) =>
        Array.AsReadOnly(_dispositions.Where(value => value.Value == disposition).Select(value => value.Key)
            .OrderBy(value => value.Z).ThenBy(value => value.Y).ThenBy(value => value.X).ToArray());

    internal Ra2VoxelSemanticPartition WithAdditionalUncertain(IEnumerable<Ra2VoxelCoordinate> coordinates)
    {
        Dictionary<Ra2VoxelCoordinate, Ra2VoxelSymmetryDisposition> overrides = coordinates.Distinct()
            .Where(_dispositions.ContainsKey)
            .ToDictionary(value => value, _ => Ra2VoxelSymmetryDisposition.Uncertain);
        return new(Evidence, Decisions, overrides);
    }

    private string ComputeHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)1);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Evidence.PackageHash);
        foreach ((Ra2VoxelCoordinate coordinate, Ra2VoxelSymmetryDisposition disposition) in _dispositions
                     .OrderBy(value => value.Key.Z).ThenBy(value => value.Key.Y).ThenBy(value => value.Key.X))
        {
            writer.Write(coordinate.X);
            writer.Write(coordinate.Y);
            writer.Write(coordinate.Z);
            writer.Write((int)disposition);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }
}

internal sealed record Ra2VoxelSemanticPartitionResult(
    Ra2VoxelSemanticSymmetryFailureKind FailureKind,
    string Message,
    Ra2VoxelSemanticPartition? Partition)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticSymmetryFailureKind.None && Partition is not null;
}

internal static class Ra2VoxelSemanticPartitionReconciler
{
    internal const double MinimumConfidence = 0.80d;

    internal static Ra2VoxelSemanticPartitionResult Reconcile(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelSymmetryModelRound roundOne,
        Ra2VoxelSymmetryModelRound roundTwo)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(roundOne);
        ArgumentNullException.ThrowIfNull(roundTwo);
        if (!IsValidRound(evidence, roundOne) || !IsValidRound(evidence, roundTwo))
            return new(Ra2VoxelSemanticSymmetryFailureKind.InvalidModelRound, "A model round does not match the bounded evidence contract.", null);

        Dictionary<string, Ra2VoxelSymmetryModelRegionDecision> first = roundOne.Decisions.ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        Dictionary<string, Ra2VoxelSymmetryModelRegionDecision> second = roundTwo.Decisions.ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        List<Ra2VoxelSemanticRegionDecision> decisions = [];
        foreach (Ra2VoxelSymmetryRegionEvidence region in evidence.Regions)
        {
            Ra2VoxelSymmetryModelRegionDecision a = first[region.RegionId];
            Ra2VoxelSymmetryModelRegionDecision b = second[region.RegionId];
            bool protectedRegion = region.FrozenCellCount > 0 || region.TransitionCellCount > 0;
            bool agrees = a.Disposition == b.Disposition && a.Confidence >= MinimumConfidence && b.Confidence >= MinimumConfidence;
            Ra2VoxelSymmetryDisposition disposition = agrees ? a.Disposition : Ra2VoxelSymmetryDisposition.Uncertain;
            if (protectedRegion && disposition is Ra2VoxelSymmetryDisposition.SymmetricCore or Ra2VoxelSymmetryDisposition.AsymmetricAttachment)
                disposition = Ra2VoxelSymmetryDisposition.Uncertain;
            decisions.Add(new(
                region.RegionId,
                disposition,
                a.Confidence,
                b.Confidence,
                string.Join(" / ", new[] { a.Reason.Trim(), b.Reason.Trim() }.Where(value => value.Length > 0)).Truncate(512),
                agrees));
        }
        return new(Ra2VoxelSemanticSymmetryFailureKind.None, string.Empty, new(evidence, decisions));
    }

    private static bool IsValidRound(Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelSymmetryModelRound round)
    {
        if (!string.Equals(round.EvidencePackageHash, evidence.PackageHash, StringComparison.Ordinal) ||
            round.ReviewedPlaneTwiceX != evidence.SelectedPlaneTwiceX || round.Decisions.Count != evidence.Regions.Count)
            return false;
        HashSet<string> expected = evidence.Regions.Select(value => value.RegionId).ToHashSet(StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (Ra2VoxelSymmetryModelRegionDecision decision in round.Decisions)
        {
            if (!actual.Add(decision.RegionId) || !expected.Contains(decision.RegionId) || !Enum.IsDefined(decision.Disposition) ||
                !double.IsFinite(decision.Confidence) || decision.Confidence is < 0d or > 1d || decision.Reason.Length > 512)
                return false;
        }
        return actual.SetEquals(expected);
    }

    private static string Truncate(this string value, int maximum) => value.Length <= maximum ? value : value[..maximum];
}

internal sealed record Ra2VoxelSemanticSymmetryResult(
    Ra2VoxelSemanticSymmetryFailureKind FailureKind,
    string Message,
    Ra2VoxelSceneSnapshot? Candidate,
    Ra2VoxelSemanticPartition? EffectivePartition,
    int ChangedPairCount,
    int UnmatchedCorePairCount,
    int AppliedOperationCount = 0,
    int AddedCellCount = 0,
    int RemovedCellCount = 0)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticSymmetryFailureKind.None && Candidate is not null &&
        EffectivePartition is not null && UnmatchedCorePairCount == 0;
}

internal static class Ra2VoxelSemanticSymmetryExecutor
{
    internal static Ra2VoxelSemanticSymmetryResult BuildCandidate(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSemanticPartition partition,
        Ra2VoxelMeshCoverageEvidence coverage,
        Ra2VoxelRefinementProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(partition);
        ArgumentNullException.ThrowIfNull(coverage);
        profile ??= new();
        try
        {
            if (!string.Equals(source.CanonicalHash, partition.Evidence.SourceSnapshotHash, StringComparison.Ordinal) ||
                !string.Equals(coverage.EvidenceHash, partition.Evidence.CoverageEvidenceHash, StringComparison.Ordinal))
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, "The semantic partition is stale for the current geometry.", partition);

            HashSet<Ra2VoxelCoordinate> occupied = source.Cells.Select(cell => cell.Coordinate).ToHashSet();
            HashSet<Ra2VoxelCoordinate> nonCore = Enum.GetValues<Ra2VoxelSymmetryDisposition>()
                .Where(value => value != Ra2VoxelSymmetryDisposition.SymmetricCore)
                .SelectMany(partition.CoordinatesFor)
                .ToHashSet();
            HashSet<Ra2VoxelCoordinate> transitionBoundary = nonCore
                .SelectMany(Ra2VoxelSymmetryEvidenceBuilder.MooreNeighbours)
                .ToHashSet();
            HashSet<Ra2VoxelCoordinate> ambiguous = [];
            int addThreshold = profile.MinimumCoveragePercent;
            int removeThreshold = Math.Max(0, profile.MinimumCoveragePercent - 8);
            foreach (Ra2VoxelCoordinate coordinate in partition.CoordinatesFor(Ra2VoxelSymmetryDisposition.SymmetricCore))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2VoxelCoordinate mirror = Ra2VoxelSymmetryEvidenceBuilder.Mirror(coordinate, partition.Evidence.SelectedPlaneTwiceX);
                if (!Ra2VoxelSymmetryEvidenceBuilder.IsInside(mirror, source.Part))
                {
                    ambiguous.Add(coordinate);
                    continue;
                }
                if ((occupied.Contains(mirror) && partition.DispositionAt(mirror) != Ra2VoxelSymmetryDisposition.SymmetricCore) ||
                    (!occupied.Contains(mirror) && transitionBoundary.Contains(mirror)))
                {
                    ambiguous.Add(coordinate);
                    continue;
                }
                if (occupied.Contains(coordinate) == occupied.Contains(mirror))
                    continue;
                int evidence = Math.Max(coverage.CoverageAt(coordinate), coverage.CoverageAt(mirror));
                if (evidence >= addThreshold || evidence < removeThreshold)
                    continue;
                ambiguous.Add(coordinate);
                if (occupied.Contains(mirror)) ambiguous.Add(mirror);
            }
            Ra2VoxelSemanticPartition effective = ambiguous.Count == 0 ? partition : partition.WithAdditionalUncertain(ambiguous);
            HashSet<Ra2VoxelCoordinate> candidate = new(occupied);
            HashSet<(Ra2VoxelCoordinate A, Ra2VoxelCoordinate B)> processed = [];
            int changedPairs = 0;
            foreach (Ra2VoxelCoordinate coordinate in effective.CoordinatesFor(Ra2VoxelSymmetryDisposition.SymmetricCore))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2VoxelCoordinate mirror = Ra2VoxelSymmetryEvidenceBuilder.Mirror(coordinate, effective.Evidence.SelectedPlaneTwiceX);
                if (!Ra2VoxelSymmetryEvidenceBuilder.IsInside(mirror, source.Part)) continue;
                if ((occupied.Contains(mirror) && effective.DispositionAt(mirror) != Ra2VoxelSymmetryDisposition.SymmetricCore) ||
                    (!occupied.Contains(mirror) && transitionBoundary.Contains(mirror))) continue;
                (Ra2VoxelCoordinate A, Ra2VoxelCoordinate B) pair = Compare(coordinate, mirror) <= 0 ? (coordinate, mirror) : (mirror, coordinate);
                if (!processed.Add(pair)) continue;
                bool a = candidate.Contains(pair.A);
                bool b = candidate.Contains(pair.B);
                if (a == b) continue;
                int evidence = Math.Max(coverage.CoverageAt(pair.A), coverage.CoverageAt(pair.B));
                if (evidence >= addThreshold)
                {
                    candidate.Add(pair.A);
                    candidate.Add(pair.B);
                    changedPairs++;
                }
                else if (evidence < removeThreshold)
                {
                    candidate.Remove(pair.A);
                    candidate.Remove(pair.B);
                    changedPairs++;
                }
            }
            if (changedPairs == 0)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate, "No unambiguous symmetric-core pair required a change.", effective);

            Ra2VoxelSceneSnapshot result = CreateDerivedSnapshot(source, candidate, effective.PartitionHash);
            if (nonCore.Any(coordinate => !candidate.Contains(coordinate)))
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate, "Semantic symmetry changed a preserved non-core cell.", effective);
            Ra2VoxelQualityAnalysisResult before = Ra2VoxelQualityAnalyzer.Analyze(source, profile, cancellationToken);
            Ra2VoxelQualityAnalysisResult after = Ra2VoxelQualityAnalyzer.Analyze(result, profile, cancellationToken);
            if (!before.IsSuccess || !after.IsSuccess)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate, "The constrained symmetry candidate could not be analyzed.", effective);
            string? gate = ValidateSafety(source, before.Facts!, result, after.Facts!, profile);
            if (gate is not null)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate, gate, effective);

            int unmatched = CountUnmatchedCorePairs(result, effective);
            if (unmatched != 0)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate, "The constrained candidate retained unmatched symmetric-core pairs.", effective);
            return new(Ra2VoxelSemanticSymmetryFailureKind.None, string.Empty, result, effective, changedPairs, 0);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.Cancelled, "Semantic symmetry generation was cancelled.", partition);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, exception.Message, partition);
        }
    }

    private static string? ValidateSafety(
        Ra2VoxelSceneSnapshot sourceSnapshot,
        Ra2VoxelGeometryQualityFacts source,
        Ra2VoxelSceneSnapshot candidateSnapshot,
        Ra2VoxelGeometryQualityFacts candidate,
        Ra2VoxelRefinementProfile profile)
    {
        string? connectivity = Ra2VoxelQualityRefiner.ValidateCandidateConnectivity(sourceSnapshot, candidateSnapshot);
        if (connectivity is not null) return connectivity;
        if (candidate.EnclosedCavityCount > source.EnclosedCavityCount) return "Semantic symmetry introduced an enclosed cavity.";
        if (PercentDelta(source.OccupiedCellCount, candidate.OccupiedCellCount) > profile.MaximumVolumeDeltaPercent)
            return "Semantic symmetry exceeded the occupied-volume gate.";
        foreach (Ra2VoxelSilhouetteFact sourceView in source.Silhouettes)
        {
            Ra2VoxelSilhouetteFact candidateView = candidate.Silhouettes.Single(value => value.View == sourceView.View);
            if (PercentDelta(sourceView.Area, candidateView.Area) > profile.MaximumSilhouetteDeltaPercent)
                return $"Semantic symmetry exceeded the {sourceView.View} silhouette gate.";
        }
        if (candidate.LowSupportSurfaceCellCount > source.LowSupportSurfaceCellCount)
            return "Semantic symmetry regressed low-support surface cells.";
        if (candidate.RoughnessScore > source.RoughnessScore + 0.005d)
            return "Semantic symmetry regressed surface roughness.";
        return null;
    }

    private static int CountUnmatchedCorePairs(Ra2VoxelSceneSnapshot snapshot, Ra2VoxelSemanticPartition partition)
    {
        HashSet<Ra2VoxelCoordinate> occupied = snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet();
        HashSet<(Ra2VoxelCoordinate A, Ra2VoxelCoordinate B)> pairs = [];
        int unmatched = 0;
        foreach (Ra2VoxelCoordinate coordinate in partition.CoordinatesFor(Ra2VoxelSymmetryDisposition.SymmetricCore))
        {
            Ra2VoxelCoordinate mirror = Ra2VoxelSymmetryEvidenceBuilder.Mirror(coordinate, partition.Evidence.SelectedPlaneTwiceX);
            if (!Ra2VoxelSymmetryEvidenceBuilder.IsInside(mirror, snapshot.Part)) continue;
            (Ra2VoxelCoordinate A, Ra2VoxelCoordinate B) pair = Compare(coordinate, mirror) <= 0 ? (coordinate, mirror) : (mirror, coordinate);
            if (!pairs.Add(pair)) continue;
            if (occupied.Contains(pair.A) != occupied.Contains(pair.B)) unmatched++;
        }
        return unmatched;
    }

    private static Ra2VoxelSceneSnapshot CreateDerivedSnapshot(
        Ra2VoxelSceneSnapshot source,
        IEnumerable<Ra2VoxelCoordinate> coordinates,
        string partitionHash)
    {
        byte fallback = source.Cells.Count == 0 ? (byte)1 : source.Cells[0].PaletteIndex;
        List<Ra2VoxelCell> cells = coordinates.OrderBy(value => value.Z).ThenBy(value => value.Y).ThenBy(value => value.X)
            .Select(value => new Ra2VoxelCell(value, source.TryGetPaletteIndex(value, out byte index) ? index : fallback)).ToList();
        List<KeyValuePair<string, string>> hashes = source.SourceArtifactHashes.ToList();
        hashes.RemoveAll(value => string.Equals(value.Key, "voxel-semantic-symmetry-profile", StringComparison.OrdinalIgnoreCase));
        hashes.Add(new("voxel-semantic-symmetry-profile", partitionHash));
        return new(source.SceneId, source.Part, source.Palette, cells, hashes);
    }

    private static int Compare(Ra2VoxelCoordinate left, Ra2VoxelCoordinate right)
    {
        int z = left.Z.CompareTo(right.Z);
        if (z != 0) return z;
        int y = left.Y.CompareTo(right.Y);
        return y != 0 ? y : left.X.CompareTo(right.X);
    }

    private static double PercentDelta(int before, int after) => before == 0 ? (after == 0 ? 0d : 100d) : Math.Abs(after - before) * 100d / before;

    private static Ra2VoxelSemanticSymmetryResult Failure(
        Ra2VoxelSemanticSymmetryFailureKind kind,
        string message,
        Ra2VoxelSemanticPartition? partition) => new(kind, message, null, partition, 0, 0);
}
