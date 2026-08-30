using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelSemanticSymmetryTests
{
    [Fact]
    public void EvidenceBuilder_IsDeterministicBoundedAndPathFree()
    {
        Ra2VoxelSceneSnapshot source = CreateReviewSource();
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, source);

        Ra2VoxelSymmetryEvidenceResult first = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source, source, analysis.ProtectionMask!, coverage);
        Ra2VoxelSymmetryEvidenceResult second = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source, source, analysis.ProtectionMask!, coverage);

        Assert.True(first.IsSuccess, first.Message);
        Assert.Equal(first.Package!.PackageHash, second.Package!.PackageHash);
        Assert.Equal(6, first.Package.Silhouettes.Count);
        Assert.InRange(first.Package.Regions.Count, 2, Ra2VoxelSymmetryEvidencePackage.MaximumRegions);
        Assert.Equal(first.Package.Regions.Sum(value => value.CellCount), source.OccupancyCount);
        Assert.Equal(8, first.Package.SelectedPlaneTwiceX);
        string prompt = first.Package.ToPromptText();
        Assert.True(prompt.Length <= Ra2VoxelSymmetryEvidencePackage.MaximumPromptCharacters);
        Assert.DoesNotContain(":\\", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain(".vox", prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvidenceBuilder_CompactsHighlyFragmentedMismatchWithoutDroppingCoordinates()
    {
        List<Ra2VoxelCoordinate> cells = BoxAt(10, 1, 1, 11, 29, 29).ToList();
        for (int z = 1; z <= 28; z += 3)
        for (int y = 1; y <= 28; y += 3)
            cells.Add(new(9, y, z));
        Ra2VoxelSceneSnapshot source = CreateSnapshot(cells, size: 31);
        Ra2VoxelFeatureProtectionMask protection = new(
            source.CanonicalHash,
            Enumerable.Repeat((byte)Ra2VoxelRefinementZone.Smoothable, source.Cells.Count));
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, source);

        Ra2VoxelSymmetryEvidenceResult result = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source,
            source,
            protection,
            coverage);

        Assert.True(result.IsSuccess, result.Message);
        Assert.InRange(result.Package!.Regions.Count, 2, Ra2VoxelSymmetryEvidencePackage.MaximumRegions);
        Assert.Equal(source.OccupancyCount, result.Package.Regions.Sum(value => value.CellCount));
        Assert.Equal(source.OccupancyCount, result.Package.Regions.SelectMany(value => value.Coordinates).Distinct().Count());
        Assert.True(result.Package.Regions
            .Where(value => value.RegionId.StartsWith("repair", StringComparison.Ordinal))
            .Sum(value => value.ConnectedComponentCount) >= 100);
        Assert.DoesNotContain(result.Package.Regions, value => value.RegionId.Contains("attached", StringComparison.Ordinal));
        Assert.Contains(result.Package.Regions, value =>
            value.RegionId.StartsWith("repair", StringComparison.Ordinal) &&
            value.AverageMirrorCoveragePercent is >= 0 and <= 100 &&
            value.MirrorTargetContactCount >= 0);
        Assert.Contains("mirror_coverage=", result.Package.ToPromptText(), StringComparison.Ordinal);
        Assert.Contains("mirror_contact=", result.Package.ToPromptText(), StringComparison.Ordinal);
        Assert.True(result.Package.ToPromptText().Length <= Ra2VoxelSymmetryEvidencePackage.MaximumEvidenceCharacters);
    }

    [Fact]
    public void PartitionReconciler_MakesDisagreementAndLowConfidenceUncertain()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, _) = CreateEvidence();
        Ra2VoxelSymmetryModelRound first = CreateRound(evidence, region =>
            region.RegionId.StartsWith("core", StringComparison.Ordinal)
                ? (Ra2VoxelSymmetryDisposition.SymmetricCore, 0.95d)
                : (Ra2VoxelSymmetryDisposition.AsymmetricAttachment, 0.92d));
        Ra2VoxelSymmetryModelRound second = CreateRound(evidence, region =>
            region.RegionId.StartsWith("core", StringComparison.Ordinal)
                ? (Ra2VoxelSymmetryDisposition.SymmetricCore, 0.94d)
                : (Ra2VoxelSymmetryDisposition.SymmetricCore, 0.70d));

        Ra2VoxelSemanticPartitionResult result = Ra2VoxelSemanticPartitionReconciler.Reconcile(evidence, first, second);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Contains(result.Partition!.Decisions, value => value.Disposition == Ra2VoxelSymmetryDisposition.SymmetricCore);
        Assert.Contains(result.Partition.Decisions, value => value.Disposition == Ra2VoxelSymmetryDisposition.Uncertain);
    }

    [Fact]
    public void PartitionReconciler_RejectsInventedRegion()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, _) = CreateEvidence();
        Ra2VoxelSymmetryModelRound valid = CreateRound(evidence, _ => (Ra2VoxelSymmetryDisposition.SymmetricCore, 0.95d));
        Ra2VoxelSymmetryModelRound invalid = new(
            evidence.PackageHash,
            evidence.SelectedPlaneTwiceX,
            valid.Decisions.Append(new("invented-region", Ra2VoxelSymmetryDisposition.SymmetricCore, 0.99d, "invalid")));

        Ra2VoxelSemanticPartitionResult result = Ra2VoxelSemanticPartitionReconciler.Reconcile(evidence, valid, invalid);

        Assert.Equal(Ra2VoxelSemanticSymmetryFailureKind.InvalidModelRound, result.FailureKind);
        Assert.Null(result.Partition);
    }

    [Fact]
    public void ConstrainedSymmetry_RepairsCoreDefectAndPreservesAttachment()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        Ra2VoxelSymmetryRegionEvidence coreDefect = evidence.Regions
            .Where(value => value.RegionId.StartsWith("repair", StringComparison.Ordinal))
            .OrderBy(value => value.Min.Y)
            .First();
        Ra2VoxelSymmetryModelRound first = CreateRound(evidence, region => Classify(region, coreDefect.RegionId));
        Ra2VoxelSymmetryModelRound second = CreateRound(evidence, region => Classify(region, coreDefect.RegionId));
        Ra2VoxelSemanticPartition partition = Ra2VoxelSemanticPartitionReconciler.Reconcile(evidence, first, second).Partition!;
        Ra2VoxelSceneSnapshot source = CreateReviewSource();

        Ra2VoxelSemanticSymmetryResult result = Ra2VoxelSemanticSymmetryExecutor.BuildCandidate(source, partition, coverage);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(0, result.UnmatchedCorePairCount);
        Assert.True(result.Candidate!.TryGetPaletteIndex(new(2, 3, 3), out _));
        Assert.True(result.Candidate.TryGetPaletteIndex(new(7, 6, 4), out _));
        Assert.Equal(source.CanonicalHash, evidence.SourceSnapshotHash);
    }

    [Fact]
    public void ConstrainedSymmetry_RejectsStalePartition()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        Ra2VoxelSymmetryModelRound round = CreateRound(evidence, _ => (Ra2VoxelSymmetryDisposition.Uncertain, 0.95d));
        Ra2VoxelSemanticPartition partition = Ra2VoxelSemanticPartitionReconciler.Reconcile(evidence, round, round).Partition!;
        Ra2VoxelSceneSnapshot other = CreateSnapshot(BoxAt(1, 1, 1, 3, 3, 3), size: 9);

        Ra2VoxelSemanticSymmetryResult result = Ra2VoxelSemanticSymmetryExecutor.BuildCandidate(other, partition, coverage);

        Assert.Equal(Ra2VoxelSemanticSymmetryFailureKind.InvalidInput, result.FailureKind);
        Assert.Null(result.Candidate);
    }

    [Fact]
    public void AgentEvidenceSlice_ExposesStableCoordinateFreeComponentTargets()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        Ra2VoxelSymmetryRegionEvidence repair = evidence.Regions.First(value =>
            value.RegionId.StartsWith("repair", StringComparison.Ordinal));

        Ra2VoxelGeometryEvidenceSliceResult first = Ra2VoxelGeometryEvidenceSliceBuilder.Build(
            evidence, coverage, [repair.RegionId]);
        Ra2VoxelGeometryEvidenceSliceResult second = Ra2VoxelGeometryEvidenceSliceBuilder.Build(
            evidence, coverage, [repair.RegionId]);

        Assert.True(first.IsSuccess, first.Message);
        Assert.Equal(first.Slice!.SliceHash, second.Slice!.SliceHash);
        Assert.NotEmpty(first.Slice.Targets);
        Assert.All(first.Slice.Targets, target => Assert.StartsWith(repair.RegionId + ".c", target.TargetId, StringComparison.Ordinal));
        Assert.DoesNotContain(":\\", first.Slice.ToPromptText(), StringComparison.Ordinal);
        Assert.True(Ra2VoxelGeometryEvidenceSliceBuilder.TryResolveTarget(
            evidence,
            first.Slice.Targets[0].TargetId,
            CancellationToken.None,
            out string parent,
            out IReadOnlyList<Ra2VoxelCoordinate> coordinates));
        Assert.Equal(repair.RegionId, parent);
        Assert.Equal(first.Slice.Targets[0].CellCount, coordinates.Count);
    }

    [Fact]
    public void AgentProposal_AddMirrorAppliesRequestedSparseTargetAndPreservesOmittedAttachment()
    {
        Ra2VoxelSceneSnapshot source = CreateReviewSource();
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        Ra2VoxelSymmetryRegionEvidence defect = evidence.Regions.Single(value => value.Coordinates.Contains(new(6, 3, 3)));
        Ra2VoxelGeometryProposal proposal = new(
            evidence.PackageHash,
            evidence.SelectedPlaneTwiceX,
            [new(defect.RegionId, Ra2VoxelGeometryProposalAction.AddMirror, 0.94d, "restore mirrored hull cell")]);

        Ra2VoxelSemanticSymmetryResult result = Ra2VoxelAgentGeometryProposalExecutor.BuildCandidate(
            source, evidence, proposal, coverage,
            new(maximumSilhouetteDeltaPercent: 15));

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(1, result.AddedCellCount);
        Assert.Equal(0, result.RemovedCellCount);
        Assert.True(result.Candidate!.TryGetPaletteIndex(new(2, 3, 3), out _));
        Assert.True(result.Candidate.TryGetPaletteIndex(new(7, 6, 4), out _));
        Assert.Equal(1, result.AppliedOperationCount);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public void AgentProposal_BridgeCenterGapFillsOnlyTheBoundedOneOrTwoCellSeam(
        bool useHalfCellPlane,
        int expectedAdded)
    {
        (Ra2VoxelSceneSnapshot source, Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) =
            CreateCenterGapEvidence(useHalfCellPlane);
        Ra2VoxelCenterSeamGapEvidence gap = Assert.Single(evidence.CenterSeamGaps);
        Ra2VoxelGeometryProposal proposal = new(
            evidence.PackageHash,
            evidence.SelectedPlaneTwiceX,
            [new(gap.TargetId, Ra2VoxelGeometryProposalAction.BridgeCenterGap, 0.96d, "join supported center seam")]);

        Ra2VoxelSemanticSymmetryResult result = Ra2VoxelAgentGeometryProposalExecutor.BuildCandidate(
            source,
            evidence,
            proposal,
            coverage,
            new(maximumSilhouetteDeltaPercent: 15));

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(expectedAdded, result.AddedCellCount);
        Assert.Equal(0, result.RemovedCellCount);
        Assert.Equal(1, result.AppliedOperationCount);
        Assert.All(gap.MissingCoordinates, coordinate =>
        {
            Assert.True(result.Candidate!.TryGetPaletteIndex(coordinate, out byte palette));
            Assert.Equal((byte)40, palette);
        });
        Assert.Contains("center_seam_gaps:", evidence.ToPromptText(), StringComparison.Ordinal);
    }

    [Fact]
    public void CenterSeamEvidence_DoesNotPromoteAnArbitraryThreeCellInteriorHole()
    {
        List<Ra2VoxelCoordinate> full = BoxAt(2, 2, 2, 5, 5, 4).ToList();
        List<Ra2VoxelCoordinate> sourceCells = full
            .Where(value => value != new Ra2VoxelCoordinate(3, 3, 3) &&
                value != new Ra2VoxelCoordinate(4, 3, 3) &&
                value != new Ra2VoxelCoordinate(5, 3, 3))
            .ToList();
        Ra2VoxelSceneSnapshot source = CreateSnapshot(sourceCells, size: 9);
        Ra2VoxelSceneSnapshot meshEvidence = CreateSnapshot(full, size: 9);
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, meshEvidence);

        Ra2VoxelSymmetryEvidenceResult result = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source,
            source,
            analysis.ProtectionMask!,
            coverage);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Empty(result.Package!.CenterSeamGaps);
    }

    [Fact]
    public void CenterSeamEvidence_CompactsFragmentedTargetsWithoutDroppingGapCoordinates()
    {
        List<Ra2VoxelCoordinate> anchors = [];
        for (int z = 1; z <= 9; z += 2)
        for (int y = 1; y <= 9; y += 2)
        {
            anchors.Add(new(4, y, z));
            anchors.Add(new(6, y, z));
        }
        Ra2VoxelSceneSnapshot source = CreateSnapshot(anchors, size: 11);
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, source);

        Ra2VoxelSymmetryEvidenceResult result = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source,
            source,
            analysis.ProtectionMask!,
            coverage);

        Assert.True(result.IsSuccess, result.Message);
        Assert.InRange(result.Package!.CenterSeamGaps.Count, 1, Ra2VoxelSymmetryEvidencePackage.MaximumCenterSeamTargets);
        Assert.Equal(25, result.Package.CenterSeamGaps.Sum(value => value.MissingCellCount));
        Assert.Equal(25, result.Package.CenterSeamGaps.SelectMany(value => value.MissingCoordinates).Distinct().Count());
    }

    [Fact]
    public void AgentProposal_RejectsActionsAppliedToTheWrongTargetKind()
    {
        (Ra2VoxelSceneSnapshot source, Ra2VoxelSymmetryEvidencePackage seamEvidence, Ra2VoxelMeshCoverageEvidence seamCoverage) =
            CreateCenterGapEvidence(useHalfCellPlane: false);
        string seamId = Assert.Single(seamEvidence.CenterSeamGaps).TargetId;
        Ra2VoxelGeometryProposal wrongMirror = new(
            seamEvidence.PackageHash,
            seamEvidence.SelectedPlaneTwiceX,
            [new(seamId, Ra2VoxelGeometryProposalAction.AddMirror, 0.9d, "wrong action")]);
        Assert.Contains("not valid", Ra2VoxelGeometryProposalValidator.Validate(seamEvidence, wrongMirror), StringComparison.OrdinalIgnoreCase);

        (Ra2VoxelSymmetryEvidencePackage repairEvidence, Ra2VoxelMeshCoverageEvidence repairCoverage) = CreateEvidence();
        string repairId = repairEvidence.Regions.First(value => value.RegionId.StartsWith("repair", StringComparison.Ordinal)).RegionId;
        Ra2VoxelGeometryProposal wrongBridge = new(
            repairEvidence.PackageHash,
            repairEvidence.SelectedPlaneTwiceX,
            [new(repairId, Ra2VoxelGeometryProposalAction.BridgeCenterGap, 0.9d, "wrong target")]);
        Ra2VoxelSemanticSymmetryResult rejected = Ra2VoxelAgentGeometryProposalExecutor.BuildCandidate(
            CreateReviewSource(),
            repairEvidence,
            wrongBridge,
            repairCoverage);
        Assert.Equal(Ra2VoxelSemanticSymmetryFailureKind.InvalidProposal, rejected.FailureKind);
        Assert.Null(rejected.Candidate);
        Assert.Equal(source.CanonicalHash, seamEvidence.SourceSnapshotHash);
        Assert.Equal(seamCoverage.EvidenceHash, seamEvidence.CoverageEvidenceHash);
    }

    [Fact]
    public void AgentProposal_RemoveSourceHonoursAgentDirectionWithoutCoverageHeuristicSubstitution()
    {
        Ra2VoxelSceneSnapshot source = CreateReviewSource();
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        Ra2VoxelSymmetryRegionEvidence attachment = evidence.Regions.Single(value => value.Coordinates.Contains(new(7, 6, 4)));
        Ra2VoxelGeometryProposal proposal = new(
            evidence.PackageHash,
            evidence.SelectedPlaneTwiceX,
            [new(attachment.RegionId, Ra2VoxelGeometryProposalAction.RemoveSource, 0.91d, "remove one-sided artifact")]);

        Ra2VoxelSemanticSymmetryResult result = Ra2VoxelAgentGeometryProposalExecutor.BuildCandidate(
            source, evidence, proposal, coverage,
            new(maximumSilhouetteDeltaPercent: 15));

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(0, result.AddedCellCount);
        Assert.Equal(1, result.RemovedCellCount);
        Assert.False(result.Candidate!.TryGetPaletteIndex(new(7, 6, 4), out _));
        Assert.False(result.Candidate.TryGetPaletteIndex(new(2, 3, 3), out _));
    }

    [Fact]
    public void AgentProposal_RejectsOverlappingTargetsAndProtectedRemoval()
    {
        Ra2VoxelSceneSnapshot source = CreateReviewSource();
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, source);
        Ra2VoxelSymmetryEvidencePackage evidence = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source, source, analysis.ProtectionMask!, coverage).Package!;
        Ra2VoxelSymmetryRegionEvidence repair = evidence.Regions.First(value => value.RegionId.StartsWith("repair", StringComparison.Ordinal));
        Ra2VoxelGeometryEvidenceSlice slice = Ra2VoxelGeometryEvidenceSliceBuilder.Build(
            evidence, coverage, [repair.RegionId]).Slice!;
        Ra2VoxelGeometryProposal overlapping = new(
            evidence.PackageHash,
            evidence.SelectedPlaneTwiceX,
            [
                new(repair.RegionId, Ra2VoxelGeometryProposalAction.AddMirror, 0.9d, "parent"),
                new(slice.Targets[0].TargetId, Ra2VoxelGeometryProposalAction.AddMirror, 0.9d, "child")
            ]);
        Assert.Contains("overlapping", Ra2VoxelGeometryProposalValidator.Validate(evidence, overlapping), StringComparison.OrdinalIgnoreCase);

        Ra2VoxelSymmetryRegionEvidence protectedRegion = new(
            "protected-fixture-001",
            [new(7, 6, 4)],
            mirrorMatchCount: 0,
            mirrorMismatchCount: 1,
            frozenCellCount: 1,
            transitionCellCount: 0,
            faceContactCount: 0,
            branchCellCount: 0,
            connectedComponentCount: 1,
            averageCoveragePercent: 100,
            averageMirrorCoveragePercent: 0,
            mirrorTargetContactCount: 0);
        Ra2VoxelSymmetryEvidencePackage protectedEvidence = new(
            source.CanonicalHash,
            evidence.ProfileHash,
            coverage.EvidenceHash,
            selectedPlaneTwiceX: 8,
            alternativePlanesTwiceX: [8],
            evidence.Silhouettes,
            [protectedRegion]);
        Ra2VoxelGeometryProposal removal = new(
            protectedEvidence.PackageHash,
            protectedEvidence.SelectedPlaneTwiceX,
            [new(protectedRegion.RegionId, Ra2VoxelGeometryProposalAction.RemoveSource, 0.99d, "remove protected")]);

        Ra2VoxelSemanticSymmetryResult result = Ra2VoxelAgentGeometryProposalExecutor.BuildCandidate(
            source, protectedEvidence, removal, coverage);

        Assert.Equal(Ra2VoxelSemanticSymmetryFailureKind.NoSafeCandidate, result.FailureKind);
        Assert.Contains("protected", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.Candidate);
    }

    [Fact]
    public void AgentProposalPartition_ColoursOnlyTheSelectedComponentAndPreservesTheRemainder()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, _) = CreateEvidence();
        Ra2VoxelSymmetryRegionEvidence splitRegion = new(
            "repair-split-001",
            [new(6, 3, 3), new(7, 6, 4)],
            mirrorMatchCount: 0,
            mirrorMismatchCount: 2,
            frozenCellCount: 0,
            transitionCellCount: 0,
            faceContactCount: 0,
            branchCellCount: 0,
            connectedComponentCount: 2,
            averageCoveragePercent: 100,
            averageMirrorCoveragePercent: 0,
            mirrorTargetContactCount: 0);
        Ra2VoxelSymmetryEvidencePackage splitEvidence = new(
            evidence.SourceSnapshotHash,
            evidence.ProfileHash,
            evidence.CoverageEvidenceHash,
            evidence.SelectedPlaneTwiceX,
            evidence.AlternativePlanesTwiceX,
            evidence.Silhouettes,
            [splitRegion]);
        Ra2VoxelGeometryProposal proposal = new(
            splitEvidence.PackageHash,
            splitEvidence.SelectedPlaneTwiceX,
            [new("repair-split-001.c001", Ra2VoxelGeometryProposalAction.AddMirror, 0.95d, "selected component")]);

        Ra2VoxelSemanticPartition partition = Ra2VoxelGeometryProposalPartitionProjector.Project(splitEvidence, proposal);

        Assert.Equal(Ra2VoxelSymmetryDisposition.SymmetricCore, partition.DispositionAt(new(6, 3, 3)));
        Assert.Equal(Ra2VoxelSymmetryDisposition.Uncertain, partition.DispositionAt(new(7, 6, 4)));
        Assert.Single(partition.CoordinatesFor(Ra2VoxelSymmetryDisposition.SymmetricCore));
    }

    private static (Ra2VoxelSymmetryEvidencePackage Evidence, Ra2VoxelMeshCoverageEvidence Coverage) CreateEvidence()
    {
        Ra2VoxelSceneSnapshot source = CreateReviewSource();
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, source);
        Ra2VoxelSymmetryEvidenceResult evidence = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source, source, analysis.ProtectionMask!, coverage);
        Assert.True(evidence.IsSuccess, evidence.Message);
        return (evidence.Package!, coverage);
    }

    private static (
        Ra2VoxelSceneSnapshot Source,
        Ra2VoxelSymmetryEvidencePackage Evidence,
        Ra2VoxelMeshCoverageEvidence Coverage) CreateCenterGapEvidence(bool useHalfCellPlane)
    {
        int size = useHalfCellPlane ? 10 : 9;
        int xLength = useHalfCellPlane ? 6 : 5;
        List<Ra2VoxelCoordinate> full = BoxAt(2, 2, 2, xLength, 5, 4).ToList();
        HashSet<Ra2VoxelCoordinate> missing = useHalfCellPlane
            ? [new(4, 3, 3), new(5, 3, 3)]
            : [new(4, 3, 3)];
        Ra2VoxelSceneSnapshot source = CreateSnapshot(full.Where(value => !missing.Contains(value)), size);
        Ra2VoxelSceneSnapshot meshEvidence = CreateSnapshot(full, size);
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, meshEvidence);
        Ra2VoxelSymmetryEvidenceResult evidence = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source,
            source,
            analysis.ProtectionMask!,
            coverage);
        Assert.True(evidence.IsSuccess, evidence.Message);
        return (source, evidence.Package!, coverage);
    }

    private static Ra2VoxelSceneSnapshot CreateReviewSource()
    {
        List<Ra2VoxelCoordinate> cells = BoxAt(2, 2, 2, 5, 5, 4).ToList();
        cells.Remove(new(2, 3, 3));
        cells.Add(new(7, 6, 4));
        return CreateSnapshot(cells, size: 9);
    }

    private static (Ra2VoxelSymmetryDisposition Disposition, double Confidence) Classify(
        Ra2VoxelSymmetryRegionEvidence region,
        string coreDefectId)
    {
        if (region.RegionId.StartsWith("core", StringComparison.Ordinal) || region.RegionId == coreDefectId)
            return (Ra2VoxelSymmetryDisposition.SymmetricCore, 0.96d);
        if (region.RegionId.StartsWith("protected", StringComparison.Ordinal))
            return (Ra2VoxelSymmetryDisposition.ProtectedThinFeature, 0.99d);
        return (Ra2VoxelSymmetryDisposition.AsymmetricAttachment, 0.94d);
    }

    private static Ra2VoxelSymmetryModelRound CreateRound(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Func<Ra2VoxelSymmetryRegionEvidence, (Ra2VoxelSymmetryDisposition Disposition, double Confidence)> selector)
        => new(
            evidence.PackageHash,
            evidence.SelectedPlaneTwiceX,
            evidence.Regions.Select(region =>
            {
                (Ra2VoxelSymmetryDisposition disposition, double confidence) = selector(region);
                return new Ra2VoxelSymmetryModelRegionDecision(region.RegionId, disposition, confidence, "fixture decision");
            }));

    private static Ra2VoxelSceneSnapshot CreateSnapshot(IEnumerable<Ra2VoxelCoordinate> coordinates, int size)
    {
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "semantic-body", size, size, size);
        return new("semantic-scene", part, CreatePalette(), coordinates.Select(value => new Ra2VoxelCell(value, 40)));
    }

    private static IEnumerable<Ra2VoxelCoordinate> BoxAt(
        int xStart, int yStart, int zStart, int xLength, int yLength, int zLength)
    {
        for (int z = zStart; z < zStart + zLength; z++)
        for (int y = yStart; y < yStart + yLength; y++)
        for (int x = xStart; x < xStart + xLength; x++)
            yield return new(x, y, z);
    }

    private static Ra2VoxelPaletteProfile CreatePalette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value)).ToArray();
        colours[0] = new(0, 0, 0, 0);
        return new("semantic-palette", colours, [0]);
    }
}
