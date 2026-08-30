using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelGeometryProposalAction
{
    AddMirror = 0,
    RemoveSource,
    BridgeCenterGap
}

internal enum Ra2VoxelGeometryTargetKind
{
    OccupiedRegion = 0,
    CenterSeamGap
}

internal enum Ra2VoxelGeometryProposalResolution
{
    Agreement = 0,
    Arbitration
}

internal sealed record Ra2VoxelGeometryProposalOperation(
    string TargetId,
    Ra2VoxelGeometryProposalAction Action,
    double Confidence,
    string Reason);

internal sealed class Ra2VoxelGeometryProposal
{
    internal const int MaximumOperations = 64;

    internal Ra2VoxelGeometryProposal(
        string evidencePackageHash,
        int reviewedPlaneTwiceX,
        IEnumerable<Ra2VoxelGeometryProposalOperation> operations,
        IEnumerable<string>? unresolvedAssumptions = null,
        Ra2VoxelGeometryProposalResolution resolution = Ra2VoxelGeometryProposalResolution.Agreement)
    {
        EvidencePackageHash = evidencePackageHash?.Trim().ToUpperInvariant() ?? string.Empty;
        ReviewedPlaneTwiceX = reviewedPlaneTwiceX;
        Operations = Array.AsReadOnly(operations
            .OrderBy(value => value.TargetId, StringComparer.Ordinal)
            .ThenBy(value => value.Action)
            .ToArray());
        UnresolvedAssumptions = Array.AsReadOnly((unresolvedAssumptions ?? [])
            .Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray());
        Resolution = resolution;
        ProposalHash = ComputeHash(includePresentation: true);
        ExecutableFingerprint = ComputeHash(includePresentation: false);
    }

    internal string EvidencePackageHash { get; }
    internal int ReviewedPlaneTwiceX { get; }
    internal IReadOnlyList<Ra2VoxelGeometryProposalOperation> Operations { get; }
    internal IReadOnlyList<string> UnresolvedAssumptions { get; }
    internal Ra2VoxelGeometryProposalResolution Resolution { get; }
    internal string ProposalHash { get; }
    internal string ExecutableFingerprint { get; }

    internal Ra2VoxelGeometryProposal WithResolution(Ra2VoxelGeometryProposalResolution resolution) =>
        new(EvidencePackageHash, ReviewedPlaneTwiceX, Operations, UnresolvedAssumptions, resolution);

    private string ComputeHash(bool includePresentation)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)(includePresentation ? 2 : 1));
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, EvidencePackageHash);
        writer.Write(ReviewedPlaneTwiceX);
        foreach (Ra2VoxelGeometryProposalOperation operation in Operations)
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, operation.TargetId);
            writer.Write((int)operation.Action);
            if (includePresentation)
            {
                writer.Write(operation.Confidence);
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, operation.Reason);
            }
        }
        if (includePresentation)
            foreach (string assumption in UnresolvedAssumptions)
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, assumption);
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }
}

internal sealed record Ra2VoxelGeometryEvidenceTarget(
    string TargetId,
    string ParentRegionId,
    int CellCount,
    Ra2VoxelCoordinate Min,
    Ra2VoxelCoordinate Max,
    int MirrorMatchCount,
    int MirrorMismatchCount,
    int FaceContactCount,
    int BranchCellCount,
    int AverageCoveragePercent,
    int AverageMirrorCoveragePercent,
    int MirrorTargetContactCount,
    bool ContainsProtectedCoordinates);

internal sealed class Ra2VoxelGeometryEvidenceSlice
{
    internal const int MaximumRequestedRegions = 8;
    internal const int MaximumTargets = 48;

    internal Ra2VoxelGeometryEvidenceSlice(
        string evidencePackageHash,
        IEnumerable<string> requestedRegionIds,
        IEnumerable<Ra2VoxelGeometryEvidenceTarget> targets)
    {
        EvidencePackageHash = evidencePackageHash;
        RequestedRegionIds = Array.AsReadOnly(requestedRegionIds.OrderBy(value => value, StringComparer.Ordinal).ToArray());
        Targets = Array.AsReadOnly(targets.OrderBy(value => value.TargetId, StringComparer.Ordinal).ToArray());
        SliceHash = ComputeHash();
    }

    internal string EvidencePackageHash { get; }
    internal IReadOnlyList<string> RequestedRegionIds { get; }
    internal IReadOnlyList<Ra2VoxelGeometryEvidenceTarget> Targets { get; }
    internal string SliceHash { get; }

    internal string ToPromptText()
    {
        StringBuilder builder = new();
        builder.AppendLine($"detail_slice_hash={SliceHash}");
        builder.AppendLine($"detail_for={string.Join(',', RequestedRegionIds)}");
        builder.AppendLine("detail_targets:");
        foreach (Ra2VoxelGeometryEvidenceTarget target in Targets)
        {
            builder.AppendLine(
                $"{target.TargetId}|parent={target.ParentRegionId}|cells={target.CellCount}|" +
                $"bbox={target.Min.X},{target.Min.Y},{target.Min.Z}-{target.Max.X},{target.Max.Y},{target.Max.Z}|" +
                $"mirror={target.MirrorMatchCount},{target.MirrorMismatchCount}|contact={target.FaceContactCount}|" +
                $"branch={target.BranchCellCount}|coverage={target.AverageCoveragePercent}|" +
                $"mirror_coverage={target.AverageMirrorCoveragePercent}|mirror_contact={target.MirrorTargetContactCount}|" +
                $"protected={(target.ContainsProtectedCoordinates ? 1 : 0)}");
        }
        return builder.ToString();
    }

    private string ComputeHash()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)1);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, EvidencePackageHash);
        foreach (string id in RequestedRegionIds) Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, id);
        foreach (Ra2VoxelGeometryEvidenceTarget target in Targets)
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, target.TargetId);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, target.ParentRegionId);
            writer.Write(target.CellCount);
            writer.Write(target.Min.X); writer.Write(target.Min.Y); writer.Write(target.Min.Z);
            writer.Write(target.Max.X); writer.Write(target.Max.Y); writer.Write(target.Max.Z);
            writer.Write(target.MirrorMatchCount); writer.Write(target.MirrorMismatchCount);
            writer.Write(target.FaceContactCount); writer.Write(target.BranchCellCount);
            writer.Write(target.AverageCoveragePercent); writer.Write(target.AverageMirrorCoveragePercent);
            writer.Write(target.MirrorTargetContactCount); writer.Write(target.ContainsProtectedCoordinates);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }
}

internal sealed record Ra2VoxelGeometryEvidenceSliceResult(
    Ra2VoxelSemanticSymmetryFailureKind FailureKind,
    string Message,
    Ra2VoxelGeometryEvidenceSlice? Slice)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticSymmetryFailureKind.None && Slice is not null;
}

internal static class Ra2VoxelGeometryEvidenceSliceBuilder
{
    internal static Ra2VoxelGeometryEvidenceSliceResult Build(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelMeshCoverageEvidence coverage,
        IEnumerable<string> requestedRegionIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(coverage);
        string[] requested = requestedRegionIds.Select(value => value.Trim()).Where(value => value.Length > 0).ToArray();
        if (!string.Equals(coverage.EvidenceHash, evidence.CoverageEvidenceHash, StringComparison.Ordinal))
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, "The detail request uses stale coverage evidence.");
        if (requested.Length is < 1 or > Ra2VoxelGeometryEvidenceSlice.MaximumRequestedRegions ||
            requested.Distinct(StringComparer.Ordinal).Count() != requested.Length)
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidProposal, "A detail request must contain unique bounded region IDs.");

        Dictionary<string, Ra2VoxelSymmetryRegionEvidence> regions = evidence.Regions
            .ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        if (requested.Any(id => !regions.ContainsKey(id)))
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidProposal, "A detail request referenced an unknown aggregate region.");

        HashSet<Ra2VoxelCoordinate> occupied = evidence.Regions.SelectMany(value => value.Coordinates).ToHashSet();
        List<Ra2VoxelGeometryEvidenceTarget> targets = [];
        foreach (string regionId in requested.OrderBy(value => value, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelSymmetryRegionEvidence parent = regions[regionId];
            List<HashSet<Ra2VoxelCoordinate>> components = Ra2VoxelSymmetryEvidenceBuilder
                .Components(parent.Coordinates.ToHashSet(), cancellationToken)
                .ToList();
            if (targets.Count + components.Count > Ra2VoxelGeometryEvidenceSlice.MaximumTargets)
            {
                return Failure(
                    Ra2VoxelSemanticSymmetryFailureKind.EvidenceTooLarge,
                    $"The requested detail expands to more than {Ra2VoxelGeometryEvidenceSlice.MaximumTargets} component targets.");
            }
            for (int index = 0; index < components.Count; index++)
                targets.Add(BuildTarget(evidence, coverage, occupied, parent, components[index], index + 1));
        }
        return new(Ra2VoxelSemanticSymmetryFailureKind.None, string.Empty,
            new(evidence.PackageHash, requested, targets));
    }

    internal static bool TryResolveTarget(
        Ra2VoxelSymmetryEvidencePackage evidence,
        string targetId,
        CancellationToken cancellationToken,
        out string parentRegionId,
        out IReadOnlyList<Ra2VoxelCoordinate> coordinates) =>
        TryResolveTarget(
            evidence,
            targetId,
            cancellationToken,
            out _,
            out parentRegionId,
            out coordinates);

    internal static bool TryResolveTarget(
        Ra2VoxelSymmetryEvidencePackage evidence,
        string targetId,
        CancellationToken cancellationToken,
        out Ra2VoxelGeometryTargetKind targetKind,
        out string parentRegionId,
        out IReadOnlyList<Ra2VoxelCoordinate> coordinates)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        targetKind = Ra2VoxelGeometryTargetKind.OccupiedRegion;
        parentRegionId = string.Empty;
        coordinates = [];
        Ra2VoxelCenterSeamGapEvidence? seamGap = evidence.CenterSeamGaps.SingleOrDefault(
            value => string.Equals(value.TargetId, targetId, StringComparison.Ordinal));
        if (seamGap is not null)
        {
            targetKind = Ra2VoxelGeometryTargetKind.CenterSeamGap;
            parentRegionId = seamGap.TargetId;
            coordinates = seamGap.MissingCoordinates;
            return true;
        }
        Ra2VoxelSymmetryRegionEvidence? direct = evidence.Regions.SingleOrDefault(
            value => string.Equals(value.RegionId, targetId, StringComparison.Ordinal));
        if (direct is not null)
        {
            parentRegionId = direct.RegionId;
            coordinates = direct.Coordinates;
            return true;
        }

        int marker = targetId.LastIndexOf(".c", StringComparison.Ordinal);
        if (marker <= 0 || marker + 5 != targetId.Length ||
            !int.TryParse(targetId.AsSpan(marker + 2, 3), out int componentNumber) || componentNumber < 1)
            return false;
        string parentId = targetId[..marker];
        Ra2VoxelSymmetryRegionEvidence? parent = evidence.Regions.SingleOrDefault(
            value => string.Equals(value.RegionId, parentId, StringComparison.Ordinal));
        if (parent is null) return false;
        List<HashSet<Ra2VoxelCoordinate>> components = Ra2VoxelSymmetryEvidenceBuilder
            .Components(parent.Coordinates.ToHashSet(), cancellationToken)
            .ToList();
        if (componentNumber > components.Count) return false;
        parentRegionId = parentId;
        coordinates = Array.AsReadOnly(components[componentNumber - 1]
            .OrderBy(value => value.Z).ThenBy(value => value.Y).ThenBy(value => value.X).ToArray());
        return true;
    }

    private static Ra2VoxelGeometryEvidenceTarget BuildTarget(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelMeshCoverageEvidence coverage,
        IReadOnlySet<Ra2VoxelCoordinate> occupied,
        Ra2VoxelSymmetryRegionEvidence parent,
        HashSet<Ra2VoxelCoordinate> cells,
        int componentNumber)
    {
        int matched = cells.Count(value => occupied.Contains(Ra2VoxelSymmetryEvidenceBuilder.Mirror(value, evidence.SelectedPlaneTwiceX)));
        int contact = cells.Sum(value => Ra2VoxelSymmetryEvidenceBuilder.FaceNeighbours(value)
            .Count(neighbour => occupied.Contains(neighbour) && !cells.Contains(neighbour)));
        int branch = cells.Count(value => Ra2VoxelSymmetryEvidenceBuilder.FaceNeighbours(value).Count(cells.Contains) >= 3);
        int averageCoverage = checked((int)Math.Round(cells.Average(coverage.CoverageAt), MidpointRounding.AwayFromZero));
        int averageMirrorCoverage = checked((int)Math.Round(cells.Average(value =>
        {
            Ra2VoxelCoordinate mirror = Ra2VoxelSymmetryEvidenceBuilder.Mirror(value, evidence.SelectedPlaneTwiceX);
            return Ra2VoxelSymmetryEvidenceBuilder.IsInside(mirror, coverage.Part) ? coverage.CoverageAt(mirror) : 0;
        }), MidpointRounding.AwayFromZero));
        int mirrorContact = cells.Sum(value =>
        {
            Ra2VoxelCoordinate mirror = Ra2VoxelSymmetryEvidenceBuilder.Mirror(value, evidence.SelectedPlaneTwiceX);
            return Ra2VoxelSymmetryEvidenceBuilder.IsInside(mirror, coverage.Part)
                ? Ra2VoxelSymmetryEvidenceBuilder.FaceNeighbours(mirror).Count(occupied.Contains)
                : 0;
        });
        return new(
            $"{parent.RegionId}.c{componentNumber:000}",
            parent.RegionId,
            cells.Count,
            new(cells.Min(value => value.X), cells.Min(value => value.Y), cells.Min(value => value.Z)),
            new(cells.Max(value => value.X), cells.Max(value => value.Y), cells.Max(value => value.Z)),
            matched,
            cells.Count - matched,
            contact,
            branch,
            averageCoverage,
            averageMirrorCoverage,
            mirrorContact,
            parent.FrozenCellCount > 0 || parent.TransitionCellCount > 0);
    }

    private static Ra2VoxelGeometryEvidenceSliceResult Failure(
        Ra2VoxelSemanticSymmetryFailureKind kind,
        string message) => new(kind, message, null);
}

internal static class Ra2VoxelGeometryProposalValidator
{
    internal static string? Validate(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelGeometryProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(proposal);
        if (!string.Equals(evidence.PackageHash, proposal.EvidencePackageHash, StringComparison.Ordinal))
            return "The geometry proposal is stale for the current evidence package.";
        if (evidence.SelectedPlaneTwiceX != proposal.ReviewedPlaneTwiceX)
            return "The geometry proposal reviewed a different symmetry plane.";
        if (proposal.Operations.Count is < 1 or > Ra2VoxelGeometryProposal.MaximumOperations)
            return "A geometry proposal must contain bounded sparse operations.";
        if (proposal.Operations.Select(value => value.TargetId).Distinct(StringComparer.Ordinal).Count() != proposal.Operations.Count)
            return "A geometry proposal contains duplicate target IDs.";

        HashSet<Ra2VoxelCoordinate> claimed = [];
        foreach (Ra2VoxelGeometryProposalOperation operation in proposal.Operations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(operation.TargetId) || operation.TargetId.Length > 96 ||
                !Enum.IsDefined(operation.Action) || !double.IsFinite(operation.Confidence) ||
                operation.Confidence is < 0d or > 1d || operation.Reason.Length > 512)
                return "A geometry proposal operation has an invalid bounded value.";
            if (!Ra2VoxelGeometryEvidenceSliceBuilder.TryResolveTarget(
                    evidence, operation.TargetId, cancellationToken, out Ra2VoxelGeometryTargetKind targetKind,
                    out _, out IReadOnlyList<Ra2VoxelCoordinate> coordinates))
                return $"Unknown geometry target: {operation.TargetId}.";
            bool compatible = operation.Action == Ra2VoxelGeometryProposalAction.BridgeCenterGap
                ? targetKind == Ra2VoxelGeometryTargetKind.CenterSeamGap
                : targetKind == Ra2VoxelGeometryTargetKind.OccupiedRegion;
            if (!compatible)
                return $"Geometry action {operation.Action} is not valid for target {operation.TargetId}.";
            if (coordinates.Any(value => !claimed.Add(value)))
                return "A geometry proposal contains overlapping targets.";
        }
        return null;
    }
}

internal static class Ra2VoxelGeometryProposalPartitionProjector
{
    internal static Ra2VoxelSemanticPartition Project(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelGeometryProposal proposal,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, List<Ra2VoxelGeometryProposalOperation>> byParent = new(StringComparer.Ordinal);
        Dictionary<Ra2VoxelCoordinate, Ra2VoxelSymmetryDisposition> coordinateOverrides = evidence.Regions
            .SelectMany(region => region.Coordinates.Select(coordinate => new
            {
                Coordinate = coordinate,
                Disposition = region.FrozenCellCount > 0 || region.TransitionCellCount > 0
                    ? Ra2VoxelSymmetryDisposition.ProtectedThinFeature
                    : Ra2VoxelSymmetryDisposition.Uncertain
            }))
            .ToDictionary(value => value.Coordinate, value => value.Disposition);
        foreach (Ra2VoxelGeometryProposalOperation operation in proposal.Operations)
        {
            if (!Ra2VoxelGeometryEvidenceSliceBuilder.TryResolveTarget(
                    evidence, operation.TargetId, cancellationToken, out Ra2VoxelGeometryTargetKind targetKind,
                    out string parentId,
                    out IReadOnlyList<Ra2VoxelCoordinate> coordinates))
                continue;
            if (targetKind == Ra2VoxelGeometryTargetKind.CenterSeamGap)
                continue;
            if (!byParent.TryGetValue(parentId, out List<Ra2VoxelGeometryProposalOperation>? values))
                byParent[parentId] = values = [];
            values.Add(operation);
            Ra2VoxelSymmetryDisposition selectedDisposition = operation.Action switch
            {
                Ra2VoxelGeometryProposalAction.AddMirror => Ra2VoxelSymmetryDisposition.SymmetricCore,
                Ra2VoxelGeometryProposalAction.RemoveSource => Ra2VoxelSymmetryDisposition.AsymmetricAttachment,
                _ => Ra2VoxelSymmetryDisposition.Uncertain
            };
            foreach (Ra2VoxelCoordinate coordinate in coordinates)
                coordinateOverrides[coordinate] = selectedDisposition;
        }

        List<Ra2VoxelSemanticRegionDecision> decisions = [];
        foreach (Ra2VoxelSymmetryRegionEvidence region in evidence.Regions)
        {
            if (!byParent.TryGetValue(region.RegionId, out List<Ra2VoxelGeometryProposalOperation>? operations))
            {
                bool protectedRegion = region.FrozenCellCount > 0 || region.TransitionCellCount > 0;
                decisions.Add(new(region.RegionId,
                    protectedRegion ? Ra2VoxelSymmetryDisposition.ProtectedThinFeature : Ra2VoxelSymmetryDisposition.Uncertain,
                    0d, 0d, "Agent did not select this region; occupancy is preserved.", false));
                continue;
            }
            Ra2VoxelGeometryProposalAction[] actions = operations.Select(value => value.Action).Distinct().ToArray();
            Ra2VoxelSymmetryDisposition disposition = actions.Length == 1
                ? actions[0] switch
                {
                    Ra2VoxelGeometryProposalAction.AddMirror => Ra2VoxelSymmetryDisposition.SymmetricCore,
                    Ra2VoxelGeometryProposalAction.RemoveSource => Ra2VoxelSymmetryDisposition.AsymmetricAttachment,
                    _ => Ra2VoxelSymmetryDisposition.Uncertain
                }
                : Ra2VoxelSymmetryDisposition.Uncertain;
            double confidence = operations.Min(value => value.Confidence);
            string reason = string.Join(" / ", operations.Select(value => value.Reason).Where(value => value.Length > 0)).Truncate(512);
            decisions.Add(new(region.RegionId, disposition, confidence, confidence, reason, true));
        }
        return new(evidence, decisions, coordinateOverrides, expandUncertainBoundary: false);
    }

    private static string Truncate(this string value, int maximum) => value.Length <= maximum ? value : value[..maximum];
}

internal static class Ra2VoxelAgentGeometryProposalExecutor
{
    internal static Ra2VoxelSemanticSymmetryResult BuildCandidate(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelGeometryProposal proposal,
        Ra2VoxelMeshCoverageEvidence coverage,
        Ra2VoxelRefinementProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(coverage);
        profile ??= new();
        Ra2VoxelSemanticPartition? partition = null;
        try
        {
            if (!string.Equals(source.CanonicalHash, evidence.SourceSnapshotHash, StringComparison.Ordinal) ||
                !string.Equals(coverage.EvidenceHash, evidence.CoverageEvidenceHash, StringComparison.Ordinal))
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, "The geometry proposal is stale for the current geometry.", null);
            string? invalid = Ra2VoxelGeometryProposalValidator.Validate(evidence, proposal, cancellationToken);
            if (invalid is not null)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidProposal, invalid, null);
            partition = Ra2VoxelGeometryProposalPartitionProjector.Project(evidence, proposal, cancellationToken);

            Dictionary<Ra2VoxelCoordinate, byte> candidate = source.Cells
                .ToDictionary(value => value.Coordinate, value => value.PaletteIndex);
            IReadOnlyDictionary<Ra2VoxelCoordinate, byte> sourceCells = source.Cells
                .ToDictionary(value => value.Coordinate, value => value.PaletteIndex);
            IReadOnlySet<Ra2VoxelCoordinate> sourceOccupied = sourceCells.Keys.ToHashSet();
            byte fallbackPalette = Ra2VoxelQualityRefiner.ResolveDominantOpaquePaletteIndex(source);
            HashSet<Ra2VoxelCoordinate> protectedCoordinates = evidence.Regions
                .Where(value => value.FrozenCellCount > 0 || value.TransitionCellCount > 0)
                .SelectMany(value => value.Coordinates)
                .ToHashSet();
            int appliedOperations = 0;
            int added = 0;
            int removed = 0;
            foreach (Ra2VoxelGeometryProposalOperation operation in proposal.Operations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2VoxelGeometryEvidenceSliceBuilder.TryResolveTarget(
                    evidence, operation.TargetId, cancellationToken, out Ra2VoxelGeometryTargetKind targetKind,
                    out _, out IReadOnlyList<Ra2VoxelCoordinate> coordinates);
                int before = added + removed;
                if (targetKind == Ra2VoxelGeometryTargetKind.CenterSeamGap)
                {
                    foreach (Ra2VoxelCoordinate coordinate in coordinates)
                    {
                        if (!Ra2VoxelSymmetryEvidenceBuilder.IsInside(coordinate, source.Part))
                            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidProposal,
                                "A proposed center-seam target is outside the voxel grid.", partition);
                        if (sourceOccupied.Contains(coordinate))
                            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidProposal,
                                "A proposed center-seam target is no longer empty.", partition);
                        byte palette = Ra2VoxelQualityRefiner.ResolveAddedPaletteIndex(
                            coordinate,
                            sourceCells,
                            fallbackPalette);
                        if (candidate.TryAdd(coordinate, palette)) added++;
                    }
                    if (added + removed > before) appliedOperations++;
                    continue;
                }
                foreach (Ra2VoxelCoordinate coordinate in coordinates)
                {
                    if (!sourceCells.TryGetValue(coordinate, out byte sourcePalette)) continue;
                    Ra2VoxelCoordinate mirror = Ra2VoxelSymmetryEvidenceBuilder.Mirror(coordinate, evidence.SelectedPlaneTwiceX);
                    if (!Ra2VoxelSymmetryEvidenceBuilder.IsInside(mirror, source.Part))
                        return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidProposal, "A proposed mirror target is outside the voxel grid.", partition);
                    if (sourceOccupied.Contains(mirror)) continue;
                    if (operation.Action == Ra2VoxelGeometryProposalAction.AddMirror)
                    {
                        if (candidate.TryAdd(mirror, sourcePalette)) added++;
                    }
                    else
                    {
                        if (protectedCoordinates.Contains(coordinate))
                            return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate,
                                "The proposal attempted to remove protected geometry.", partition);
                        if (candidate.Remove(coordinate)) removed++;
                    }
                }
                if (added + removed > before) appliedOperations++;
            }
            if (added + removed == 0)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate,
                    "The Agent proposal did not require a geometry change.", partition);
            if (protectedCoordinates.Any(value => !candidate.ContainsKey(value)))
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate,
                    "The proposal removed protected geometry.", partition);

            Ra2VoxelSceneSnapshot result = CreateDerivedSnapshot(source, candidate, proposal.ProposalHash);
            Ra2VoxelQualityAnalysisResult beforeFacts = Ra2VoxelQualityAnalyzer.Analyze(source, profile, cancellationToken);
            Ra2VoxelQualityAnalysisResult afterFacts = Ra2VoxelQualityAnalyzer.Analyze(result, profile, cancellationToken);
            if (!beforeFacts.IsSuccess || !afterFacts.IsSuccess)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate,
                    "The Agent proposal could not be analyzed safely.", partition);
            string? gate = ValidateMinimumSafety(source, beforeFacts.Facts!, result, afterFacts.Facts!, profile);
            if (gate is not null)
                return Failure(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate, gate, partition);
            return new(
                Ra2VoxelSemanticSymmetryFailureKind.None,
                string.Empty,
                result,
                partition,
                added + removed,
                0,
                appliedOperations,
                added,
                removed);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.Cancelled, "Agent geometry proposal execution was cancelled.", partition);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return Failure(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, exception.Message, partition);
        }
    }

    private static string? ValidateMinimumSafety(
        Ra2VoxelSceneSnapshot sourceSnapshot,
        Ra2VoxelGeometryQualityFacts source,
        Ra2VoxelSceneSnapshot candidateSnapshot,
        Ra2VoxelGeometryQualityFacts candidate,
        Ra2VoxelRefinementProfile profile)
    {
        string? connectivity = Ra2VoxelQualityRefiner.ValidateCandidateConnectivity(sourceSnapshot, candidateSnapshot);
        if (connectivity is not null) return connectivity;
        if (candidate.EnclosedCavityCount > source.EnclosedCavityCount)
            return "The Agent proposal introduced an enclosed cavity.";
        if (PercentDelta(source.OccupiedCellCount, candidate.OccupiedCellCount) > profile.MaximumVolumeDeltaPercent)
            return "The Agent proposal exceeded the occupied-volume safety limit.";
        foreach (Ra2VoxelSilhouetteFact sourceView in source.Silhouettes)
        {
            Ra2VoxelSilhouetteFact candidateView = candidate.Silhouettes.Single(value => value.View == sourceView.View);
            if (PercentDelta(sourceView.Area, candidateView.Area) > profile.MaximumSilhouetteDeltaPercent)
                return $"The Agent proposal exceeded the {sourceView.View} silhouette safety limit.";
        }
        return null;
    }

    private static Ra2VoxelSceneSnapshot CreateDerivedSnapshot(
        Ra2VoxelSceneSnapshot source,
        IReadOnlyDictionary<Ra2VoxelCoordinate, byte> cells,
        string proposalHash)
    {
        List<KeyValuePair<string, string>> hashes = source.SourceArtifactHashes.ToList();
        hashes.RemoveAll(value => string.Equals(value.Key, "voxel-agent-geometry-proposal", StringComparison.OrdinalIgnoreCase));
        hashes.Add(new("voxel-agent-geometry-proposal", proposalHash));
        return new(
            source.SceneId,
            source.Part,
            source.Palette,
            cells.OrderBy(value => value.Key.Z).ThenBy(value => value.Key.Y).ThenBy(value => value.Key.X)
                .Select(value => new Ra2VoxelCell(value.Key, value.Value)),
            hashes);
    }

    private static double PercentDelta(int before, int after) =>
        before == 0 ? (after == 0 ? 0d : 100d) : Math.Abs(after - before) * 100d / before;

    private static Ra2VoxelSemanticSymmetryResult Failure(
        Ra2VoxelSemanticSymmetryFailureKind kind,
        string message,
        Ra2VoxelSemanticPartition? partition) => new(kind, message, null, partition, 0, 0);
}
