using System.Security.Cryptography;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelQualityRefinementTests
{
    [Fact]
    public void DefaultProfile_UsesEvidenceBackedCoverageWithoutRelaxingQualityGates()
    {
        Ra2VoxelRefinementProfile profile = new();

        Assert.Equal(40, profile.MinimumCoveragePercent);
        Assert.Equal(5, profile.MaximumVolumeDeltaPercent);
        Assert.Equal(3, profile.MaximumSilhouetteDeltaPercent);
    }

    private static readonly int[] CubeIndices =
    [
        0, 2, 1, 0, 3, 2,
        4, 5, 6, 4, 6, 7,
        0, 1, 5, 0, 5, 4,
        3, 7, 6, 3, 6, 2,
        0, 4, 7, 0, 7, 3,
        1, 2, 6, 1, 6, 5
    ];

    [Fact]
    public void Analyzer_IsDeterministicAndProducesAllSilhouettes()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(Box(2, 2, 2), size: 4);

        Ra2VoxelQualityAnalysisResult first = Ra2VoxelQualityAnalyzer.Analyze(snapshot);
        Ra2VoxelQualityAnalysisResult second = Ra2VoxelQualityAnalyzer.Analyze(snapshot);

        Assert.True(first.IsSuccess, first.Message);
        Assert.Equal(first.Facts!.FactsHash, second.Facts!.FactsHash);
        Assert.Equal(6, first.Facts.Silhouettes.Count);
        Assert.Equal(0, first.Facts.UnmatchedCellCount);
        Assert.Equal(1d, first.Facts.SymmetryScore);
        Assert.Equal(snapshot.OccupancyCount, first.ProtectionMask!.CellCount);
    }

    [Fact]
    public void Analyzer_DoesNotProtectSingleAttachedBumpAsThinStructure()
    {
        List<Ra2VoxelCoordinate> cells = BoxAt(2, 1, 1, 3, 3, 3).ToList();
        cells.Add(new(5, 2, 2));
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(cells, size: 7);

        Ra2VoxelQualityAnalysisResult result = Ra2VoxelQualityAnalyzer.Analyze(snapshot);

        Assert.True(result.IsSuccess, result.Message);
        int bump = snapshot.Cells.ToList().FindIndex(cell => cell.Coordinate == new Ra2VoxelCoordinate(5, 2, 2));
        Assert.True(bump >= 0);
        Assert.False(result.ProtectionMask!.IsProtected(bump));
        Assert.True(result.Facts!.LowSupportSurfaceCellCount > 0);
    }

    [Fact]
    public void Analyzer_ProtectsCompleteRodIncludingDegreeOneEndpoint()
    {
        List<Ra2VoxelCoordinate> cells = BoxAt(1, 1, 1, 3, 3, 3).ToList();
        cells.AddRange(Enumerable.Range(4, 4).Select(x => new Ra2VoxelCoordinate(x, 2, 2)));
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(cells, size: 10);

        Ra2VoxelQualityAnalysisResult result = Ra2VoxelQualityAnalyzer.Analyze(snapshot);

        Assert.True(result.IsSuccess, result.Message);
        foreach (int x in Enumerable.Range(4, 4))
        {
            int index = snapshot.Cells.ToList().FindIndex(cell => cell.Coordinate == new Ra2VoxelCoordinate(x, 2, 2));
            Assert.True(result.ProtectionMask!.IsFrozen(index));
        }
        Assert.True(result.Facts!.ProtectedEndpointCount >= 1);
        Assert.True(result.ProtectionMask!.TransitionCellCount > 0);
    }

    [Fact]
    public void Analyzer_ProtectsSustainedThinPlate()
    {
        List<Ra2VoxelCoordinate> cells = BoxAt(1, 1, 1, 3, 3, 3).ToList();
        cells.AddRange(BoxAt(4, 1, 2, 3, 3, 1));
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(cells, size: 9);

        Ra2VoxelQualityAnalysisResult result = Ra2VoxelQualityAnalyzer.Analyze(snapshot);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(result.ProtectionMask!.FrozenCellCount >= 9);
        Assert.True(result.Facts!.ProtectedComponentCount >= 1);
    }

    [Fact]
    public void Analyzer_DoesNotFreezeOrdinarySolidBodySurface()
    {
        Ra2VoxelQualityAnalysisResult result = Ra2VoxelQualityAnalyzer.Analyze(
            CreateSnapshot(BoxAt(1, 1, 1, 6, 6, 6), size: 8));

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(0, result.ProtectionMask!.FrozenCellCount);
    }

    [Fact]
    public void CoherentDeltaFilter_RejectsSingletonNoiseButKeepsContinuousRegions()
    {
        HashSet<Ra2VoxelCoordinate> direct = BoxAt(1, 1, 1, 4, 4, 4).ToHashSet();
        Ra2VoxelCoordinate isolatedRemoval = new(1, 1, 1);
        Ra2VoxelCoordinate isolatedAddition = new(8, 8, 8);
        Ra2VoxelCoordinate regionRemovalA = new(4, 3, 3);
        Ra2VoxelCoordinate regionRemovalB = new(4, 4, 3);
        Ra2VoxelCoordinate regionAdditionA = new(5, 3, 3);
        Ra2VoxelCoordinate regionAdditionB = new(5, 4, 3);
        HashSet<Ra2VoxelCoordinate> proposal = new(direct);
        proposal.Remove(isolatedRemoval);
        proposal.Remove(regionRemovalA);
        proposal.Remove(regionRemovalB);
        proposal.Add(isolatedAddition);
        proposal.Add(regionAdditionA);
        proposal.Add(regionAdditionB);

        HashSet<Ra2VoxelCoordinate> result = Ra2VoxelQualityRefiner.RetainCoherentDelta(
            direct,
            proposal,
            minimumClusterSize: 2);

        Assert.Contains(isolatedRemoval, result);
        Assert.DoesNotContain(isolatedAddition, result);
        Assert.DoesNotContain(regionRemovalA, result);
        Assert.DoesNotContain(regionRemovalB, result);
        Assert.Contains(regionAdditionA, result);
        Assert.Contains(regionAdditionB, result);
    }

    [Fact]
    public void Analyzer_ProtectsVerticalAntennaAndTaperedRodButNotShortAsymmetricDetail()
    {
        List<Ra2VoxelCoordinate> cells = BoxAt(1, 1, 1, 4, 4, 4).ToList();
        cells.AddRange(Enumerable.Range(5, 4).Select(x => new Ra2VoxelCoordinate(x, 2, 2)));
        cells.Add(new(5, 3, 2));
        cells.AddRange(Enumerable.Range(5, 4).Select(z => new Ra2VoxelCoordinate(2, 2, z)));
        cells.AddRange([new Ra2VoxelCoordinate(3, 5, 3), new Ra2VoxelCoordinate(3, 6, 3)]);
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(cells, size: 11);

        Ra2VoxelQualityAnalysisResult result = Ra2VoxelQualityAnalyzer.Analyze(snapshot);

        Assert.True(result.IsSuccess, result.Message);
        foreach (Ra2VoxelCoordinate coordinate in Enumerable.Range(5, 4).Select(x => new Ra2VoxelCoordinate(x, 2, 2)))
        {
            int index = snapshot.Cells.ToList().FindIndex(cell => cell.Coordinate == coordinate);
            Assert.True(result.ProtectionMask!.IsFrozen(index));
        }
        foreach (Ra2VoxelCoordinate coordinate in Enumerable.Range(5, 4).Select(z => new Ra2VoxelCoordinate(2, 2, z)))
        {
            int index = snapshot.Cells.ToList().FindIndex(cell => cell.Coordinate == coordinate);
            Assert.True(result.ProtectionMask!.IsFrozen(index));
        }
        int shortDetail = snapshot.Cells.ToList().FindIndex(cell => cell.Coordinate == new Ra2VoxelCoordinate(3, 6, 3));
        Assert.False(result.ProtectionMask!.IsFrozen(shortDetail));
    }

    [Fact]
    public void Analyzer_CountsEnclosedCavityAsTopologyFact()
    {
        List<Ra2VoxelCoordinate> shell = BoxAt(1, 1, 1, 3, 3, 3)
            .Where(coordinate => coordinate != new Ra2VoxelCoordinate(2, 2, 2))
            .ToList();

        Ra2VoxelQualityAnalysisResult result = Ra2VoxelQualityAnalyzer.Analyze(CreateSnapshot(shell, size: 5));

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(1, result.Facts!.EnclosedCavityCount);
    }

    [Fact]
    public void SymmetrySuggestion_RemovesUnsupportedBumpButKeepsSourceImmutable()
    {
        List<Ra2VoxelCoordinate> cells = BoxAt(2, 1, 1, 3, 3, 3).ToList();
        cells.Add(new(5, 2, 2));
        Ra2VoxelSceneSnapshot source = CreateSnapshot(cells, size: 7);
        string sourceHash = source.CanonicalHash;

        Ra2VoxelSceneSnapshot? candidate = Ra2VoxelQualityRefiner.SuggestSymmetry(source);

        Assert.NotNull(candidate);
        Assert.Equal(sourceHash, source.CanonicalHash);
        Assert.False(candidate!.TryGetPaletteIndex(new(5, 2, 2), out _));
        Assert.True(candidate.Symmetry.UnmatchedCellCount < source.Symmetry.UnmatchedCellCount);
        Assert.True(candidate.Connectivity.IsSingleComponent);
    }

    [Fact]
    public void Convert_ProducesDirectAndSupersampledCandidatesWithoutChangingDimensions()
    {
        Ra2MeshSnapshot mesh = CreateCubeMesh();
        Ra2MeshVoxelizationOptions options = CreateOptions(target: 32);
        Ra2VoxelRefinementProfile profile = new(maximumSupersampleDimension: 64);

        Ra2VoxelQualityRefinementResult result = Ra2VoxelQualityRefiner.Convert(
            mesh,
            options,
            profile,
            Ra2VoxelSymmetryMode.Suggest);

        Assert.True(result.IsSuccess, result.Message);
        Assert.NotNull(result.DirectCandidate);
        Assert.NotNull(result.RefinedCandidate);
        Assert.Equal(result.DirectCandidate!.Part.XSize, result.RefinedCandidate!.Part.XSize);
        Assert.Equal(result.DirectCandidate.Part.YSize, result.RefinedCandidate.Part.YSize);
        Assert.Equal(result.DirectCandidate.Part.ZSize, result.RefinedCandidate.Part.ZSize);
        Assert.True(result.RefinedCandidate.Connectivity.IsSingleComponent);
        Assert.Equal(result.HasSafeImprovement, result.ReviewPackage!.Admission.IsAdmitted);
        Assert.Equal(3, result.ReviewPackage.CandidateReviews.Count);
        Assert.Equal(result.HasSafeImprovement ? 1 : 0, result.ReviewPackage.CandidateReviews.Count(value => value.IsSelected));
        if (result.HasSafeImprovement)
        {
            Assert.NotEqual(result.DirectCandidate.CanonicalHash, result.RefinedCandidate.CanonicalHash);
            Assert.True(Ra2VoxelQualityRefiner.IsMeaningfullySmoother(
                result.ReviewPackage.SourceFacts,
                result.ReviewPackage.RefinedFacts));
        }
        else
            Assert.Equal(Ra2VoxelRefinementFailureKind.NoSafeImprovement, result.FailureKind);
        Assert.Equal(6, result.ReviewPackage!.RefinedFacts.Silhouettes.Count);
        Assert.Contains(result.ReviewPackage.SemanticRegions, value => value.RegionId == "body-shell");
    }

    [Fact]
    public void Convert_IsDeterministicForSameMeshAndProfile()
    {
        Ra2MeshSnapshot mesh = CreateCubeMesh();
        Ra2MeshVoxelizationOptions options = CreateOptions(target: 24);
        Ra2VoxelRefinementProfile profile = new(maximumSupersampleDimension: 48);

        Ra2VoxelQualityRefinementResult first = Ra2VoxelQualityRefiner.Convert(mesh, options, profile);
        Ra2VoxelQualityRefinementResult second = Ra2VoxelQualityRefiner.Convert(mesh, options, profile);

        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Assert.Equal(first.RefinedCandidate!.CanonicalHash, second.RefinedCandidate!.CanonicalHash);
        Assert.Equal(first.ReviewPackage!.RefinedFacts.FactsHash, second.ReviewPackage!.RefinedFacts.FactsHash);
        Assert.Equal(first.ReviewPackage.NormalComparison.CandidateFieldHash,
            second.ReviewPackage.NormalComparison.CandidateFieldHash);
    }

    [Fact]
    public void RefineExisting_KeepsTheExactWorkingSnapshotAsItsBaseline()
    {
        Ra2MeshSnapshot mesh = CreateCubeMesh();
        Ra2MeshVoxelizationOptions options = CreateOptions(target: 24);
        Ra2VoxelSceneSnapshot converted = Ra2VoxelQualityRefiner.Convert(mesh, options).DirectCandidate!;
        Ra2VoxelCell first = converted.Cells[0];
        Ra2VoxelSceneSnapshot working = new(
            converted.SceneId,
            converted.Part,
            converted.Palette,
            converted.Cells.Select(cell => cell.Coordinate == first.Coordinate ? cell with { PaletteIndex = 41 } : cell),
            converted.SourceArtifactHashes);

        Ra2VoxelQualityRefinementResult result = Ra2VoxelQualityRefiner.RefineExisting(working, mesh, options);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(working.CanonicalHash, result.DirectCandidate!.CanonicalHash);
        Assert.True(result.DirectCandidate.TryGetPaletteIndex(first.Coordinate, out byte paletteIndex));
        Assert.Equal(41, paletteIndex);
        Assert.Equal(working.CanonicalHash, result.ReviewPackage!.SourceFacts.SourceSnapshotHash);
        Assert.Equal(working.CanonicalHash, result.ReviewPackage.ProtectionMask.SourceSnapshotHash);
    }

    [Fact]
    public void RefineExisting_IsDeterministicAndRejectsMismatchedIdentity()
    {
        Ra2MeshSnapshot mesh = CreateCubeMesh();
        Ra2MeshVoxelizationOptions options = CreateOptions(target: 24);
        Ra2VoxelSceneSnapshot working = Ra2VoxelQualityRefiner.Convert(mesh, options).DirectCandidate!;

        Ra2VoxelQualityRefinementResult first = Ra2VoxelQualityRefiner.RefineExisting(working, mesh, options);
        Ra2VoxelQualityRefinementResult second = Ra2VoxelQualityRefiner.RefineExisting(working, mesh, options);
        Ra2MeshVoxelizationOptions mismatch = new(
            working.SceneId,
            "other-part",
            working.Part.Role,
            working.Part.VxlSectionName,
            working.Part.StableFileStem,
            24,
            1,
            working.Palette,
            paletteIndex: 40);
        Ra2VoxelQualityRefinementResult rejected = Ra2VoxelQualityRefiner.RefineExisting(working, mesh, mismatch);
        Ra2VoxelQualityRefinementResult gridRejected = Ra2VoxelQualityRefiner.RefineExisting(
            working,
            mesh,
            CreateOptions(target: 32));

        Assert.Equal(first.DirectCandidate!.CanonicalHash, second.DirectCandidate!.CanonicalHash);
        Assert.Equal(first.RefinedCandidate!.CanonicalHash, second.RefinedCandidate!.CanonicalHash);
        Assert.Equal(Ra2VoxelRefinementFailureKind.InvalidOptions, rejected.FailureKind);
        Assert.Same(working, rejected.DirectCandidate);
        Assert.Null(rejected.RefinedCandidate);
        Assert.Equal(Ra2VoxelRefinementFailureKind.EvidenceGridMismatch, gridRejected.FailureKind);
        Assert.Same(working, gridRejected.DirectCandidate);
        Assert.Null(gridRejected.RefinedCandidate);
    }

    [Fact]
    public void RefineExisting_CancellationPublishesNoDerivedCandidate()
    {
        Ra2MeshSnapshot mesh = CreateCubeMesh();
        Ra2MeshVoxelizationOptions options = CreateOptions(target: 24);
        Ra2VoxelSceneSnapshot working = Ra2VoxelQualityRefiner.Convert(mesh, options).DirectCandidate!;
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2VoxelQualityRefinementResult result = Ra2VoxelQualityRefiner.RefineExisting(
            working,
            mesh,
            options,
            cancellationToken: cancellation.Token);

        Assert.Equal(Ra2VoxelRefinementFailureKind.Cancelled, result.FailureKind);
        Assert.Same(working, result.DirectCandidate);
        Assert.Null(result.RefinedCandidate);
    }

    [Fact]
    public void CandidateProvenanceHash_IncludesBehaviourAndOccupancyThreshold()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot(BoxAt(1, 1, 1, 5, 5, 5), size: 8);
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Assert.True(analysis.IsSuccess, analysis.Message);
        Ra2VoxelRefinementProfile profile = new();

        Ra2VoxelSceneSnapshot balanced = Ra2VoxelQualityRefiner.BuildMeshEvidenceSurfaceCandidate(
            source,
            source,
            analysis.ProtectionMask!,
            profile,
            Ra2VoxelRefinementCandidateKind.Balanced,
            minimumDeltaClusterSize: 2,
            occupancyThreshold: 28,
            cancellationToken: CancellationToken.None);
        Ra2VoxelSceneSnapshot surfacePolish = Ra2VoxelQualityRefiner.BuildMeshEvidenceSurfaceCandidate(
            source,
            source,
            analysis.ProtectionMask!,
            profile,
            Ra2VoxelRefinementCandidateKind.SurfacePolish,
            minimumDeltaClusterSize: 2,
            occupancyThreshold: 36,
            cancellationToken: CancellationToken.None);

        Assert.NotEqual(balanced.CanonicalHash, surfacePolish.CanonicalHash);
        Assert.NotEqual(
            string.Join("|", balanced.SourceArtifactHashes),
            string.Join("|", surfacePolish.SourceArtifactHashes));
    }

    [Fact]
    public void ConnectivityGate_RejectsNewDetachedAttachmentsEvenWithDominantBody()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot(BoxAt(1, 1, 1, 5, 5, 4), size: 16);
        List<Ra2VoxelCoordinate> candidateCells = BoxAt(1, 1, 1, 5, 5, 4).ToList();
        candidateCells.AddRange([new(12, 1, 1), new(14, 1, 1)]);
        Ra2VoxelSceneSnapshot candidate = CreateSnapshot(candidateCells, size: 16);

        string? rejection = Ra2VoxelQualityRefiner.ValidateCandidateConnectivity(source, candidate);

        Assert.NotNull(rejection);
        Assert.Contains("introduced disconnected geometry", rejection, StringComparison.Ordinal);
        Assert.Equal(3, candidate.Connectivity.ComponentCount);
        Assert.True(candidate.Connectivity.LargestComponentCellCount >= candidate.OccupancyCount * 0.95d);
    }

    [Fact]
    public void ConnectivityGate_RejectsEvenFragmentationWithoutADominantBody()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot(BoxAt(1, 1, 1, 3, 2, 2), size: 15);
        Ra2VoxelSceneSnapshot candidate = CreateSnapshot(
        [
            new(1, 1, 1), new(1, 1, 2), new(1, 2, 1), new(1, 2, 2),
            new(6, 1, 1), new(6, 1, 2), new(6, 2, 1), new(6, 2, 2),
            new(11, 1, 1), new(11, 1, 2), new(11, 2, 1), new(11, 2, 2)
        ], size: 15);

        string? rejection = Ra2VoxelQualityRefiner.ValidateCandidateConnectivity(source, candidate);

        Assert.NotNull(rejection);
        Assert.Contains("introduced disconnected geometry", rejection, StringComparison.Ordinal);
        Assert.Equal(3, candidate.Connectivity.ComponentCount);
    }

    [Fact]
    public void Analyze_CancelledReturnsTypedFailure()
    {
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2VoxelQualityAnalysisResult result = Ra2VoxelQualityAnalyzer.Analyze(
            CreateSnapshot(Box(2, 2, 2)),
            cancellationToken: source.Token);

        Assert.Equal(Ra2VoxelRefinementFailureKind.Cancelled, result.FailureKind);
        Assert.Null(result.Facts);
    }

    private static Ra2MeshSnapshot CreateCubeMesh()
    {
        Ra2MeshVector3[] positions =
        [
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0),
            new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1)
        ];
        Ra2MeshTriangle[] triangles = Enumerable.Range(0, CubeIndices.Length / 3)
            .Select(index => new Ra2MeshTriangle(
                CubeIndices[index * 3],
                CubeIndices[index * 3 + 1],
                CubeIndices[index * 3 + 2]))
            .ToArray();
        return new(positions, triangles, Convert.ToHexString(SHA256.HashData([1, 2, 3])));
    }

    private static Ra2MeshVoxelizationOptions CreateOptions(int target)
        => new(
            "quality-scene",
            "body",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "quality-body",
            target,
            1,
            CreatePalette(),
            paletteIndex: 40);

    private static Ra2VoxelSceneSnapshot CreateSnapshot(
        IEnumerable<Ra2VoxelCoordinate> coordinates,
        int size = 6)
    {
        Ra2VoxelPartDescriptor part = new(
            "body",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "quality-body",
            size,
            size,
            size);
        return new(
            "quality-scene",
            part,
            CreatePalette(),
            coordinates.Select(value => new Ra2VoxelCell(value, 40)));
    }

    private static IEnumerable<Ra2VoxelCoordinate> Box(int xLength, int yLength, int zLength)
        => BoxAt(1, 1, 1, xLength, yLength, zLength);

    private static IEnumerable<Ra2VoxelCoordinate> BoxAt(
        int xStart,
        int yStart,
        int zStart,
        int xLength,
        int yLength,
        int zLength)
    {
        for (int z = zStart; z < zStart + zLength; z++)
        for (int y = yStart; y < yStart + yLength; y++)
        for (int x = xStart; x < xStart + xLength; x++)
            yield return new(x, y, z);
    }

    private static Ra2VoxelPaletteProfile CreatePalette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        return new("quality-palette", colours, [0]);
    }
}
