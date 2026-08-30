using System.Numerics;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelSurfaceAndNormalTests
{
    [Fact]
    public void SurfaceProjection_SingleCellHasSixDeterministicallyOrderedFaces()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(1, 1, 1, [(0, 0, 0)]);

        Ra2VoxelSurfaceProjectionResult result = Ra2VoxelSurfaceProjector.Project(snapshot);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(snapshot.CanonicalHash, result.Projection!.SourceSnapshotHash);
        Assert.Equal(1, result.Projection.SurfaceCellCount);
        Assert.Equal(6, result.Projection.FaceCount);
        Assert.Equal(
            Enum.GetValues<Ra2VoxelFaceDirection>(),
            result.Projection.Faces.Select(face => face.Direction));
        Assert.All(result.Projection.Faces, face => Assert.Equal((byte)60, face.PaletteIndex));
    }

    [Fact]
    public void SurfaceProjection_CullsSharedFacesAndIsAvailableAfterVoxDecode()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot(
            2,
            2,
            2,
            from x in Enumerable.Range(0, 2)
            from y in Enumerable.Range(0, 2)
            from z in Enumerable.Range(0, 2)
            select (x, y, z));
        byte[] vox = Ra2MagicaVoxelCodec.Write(source);
        using MemoryStream stream = new(vox, writable: false);
        Ra2VoxelSceneSnapshot decodedVox = Ra2MagicaVoxelCodec.Read(
            stream,
            "VOX_SURFACE",
            "body",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "vox-surface");

        Ra2VoxelSurfaceProjection sourceProjection = AssertSuccess(Ra2VoxelSurfaceProjector.Project(source));
        Ra2VoxelSurfaceProjection voxProjection = AssertSuccess(Ra2VoxelSurfaceProjector.Project(decodedVox));

        Assert.Equal(8, sourceProjection.SurfaceCellCount);
        Assert.Equal(24, sourceProjection.FaceCount);
        Assert.Equal(
            sourceProjection.Faces.Select(face => (face.Coordinate, face.Direction, face.PaletteIndex)),
            voxProjection.Faces.Select(face => (face.Coordinate, face.Direction, face.PaletteIndex)));
    }

    [Fact]
    public void SurfaceProjection_ReportsFaceBudgetAndCancellationWithoutPartialProjection()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(1, 1, 1, [(0, 0, 0)]);

        Ra2VoxelSurfaceProjectionResult limited = Ra2VoxelSurfaceProjector.Project(snapshot, maximumFaceCount: 5);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Ra2VoxelSurfaceProjectionResult cancelled = Ra2VoxelSurfaceProjector.Project(
            snapshot,
            cancellationToken: cancellation.Token);

        Assert.Equal(Ra2VoxelSurfaceProjectionFailureKind.ResourceLimitExceeded, limited.FailureKind);
        Assert.Null(limited.Projection);
        Assert.Equal(Ra2VoxelSurfaceProjectionFailureKind.Cancelled, cancelled.FailureKind);
        Assert.Null(cancelled.Projection);
    }

    [Fact]
    public void NormalPalettes_ExposeCanonicalRa2AndTsDirectionsWithStableNearestQuantization()
    {
        Ra2VxlNormalPalette ra2 = Ra2VxlNormalPalette.For(Ra2VxlNormalPaletteKind.RedAlert2);
        Ra2VxlNormalPalette ts = Ra2VxlNormalPalette.For(Ra2VxlNormalPaletteKind.TiberianSun);

        Assert.Equal(244, ra2.Count);
        Assert.Equal(36, ts.Count);
        Assert.Equal((byte)0, ra2.FindClosestIndex(ra2[0]));
        Assert.Equal((byte)17, ra2.FindClosestIndex(ra2[17]));
        Assert.Equal((byte)0, ts.FindClosestIndex(ts[0]));
        Assert.All(ra2.Directions, direction => Assert.InRange(direction.Length(), 0.9999f, 1.0001f));
        Assert.All(ts.Directions, direction => Assert.InRange(direction.Length(), 0.9999f, 1.0001f));
    }

    [Fact]
    public void NormalBaker_IsDeterministicAndUsesTheSamePathForVoxSnapshots()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot(
            3,
            2,
            2,
            [(0, 0, 0), (1, 0, 0), (2, 0, 0), (1, 1, 0), (1, 0, 1)]);
        Ra2VoxelNormalBakeOptions options = new(radius: 2, smoothingIterations: 1);

        Ra2VoxelNormalBakeResult first = Ra2VoxelNormalBaker.Bake(source, options: options);
        Ra2VoxelNormalBakeResult second = Ra2VoxelNormalBaker.Bake(source, options: options);
        Assert.True(first.IsSuccess, first.Message);
        Assert.Equal(first.Field!.FieldHash, second.Field!.FieldHash);
        Assert.Equal(source.CanonicalHash, first.Field.SourceSnapshotHash);
        Assert.Equal(5, first.Facts!.Value.SurfaceSampleCount);
        Assert.All(first.Field.Samples, sample => Assert.InRange(sample.Direction.Length(), 0.9999f, 1.0001f));

        byte[] vox = Ra2MagicaVoxelCodec.Write(source);
        using MemoryStream stream = new(vox, writable: false);
        Ra2VoxelSceneSnapshot decodedVox = Ra2MagicaVoxelCodec.Read(
            stream,
            "VOX_NORMAL",
            "body",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "vox-normal");
        Ra2VoxelNormalBakeResult voxResult = Ra2VoxelNormalBaker.Bake(decodedVox, options: options);

        Assert.True(voxResult.IsSuccess, voxResult.Message);
        Assert.Equal(
            first.Field.Samples.Select(sample => (sample.Coordinate, sample.NormalIndex)),
            voxResult.Field!.Samples.Select(sample => (sample.Coordinate, sample.NormalIndex)));
    }

    [Fact]
    public void NormalBaker_SupportsTsAndReturnsTypedLimitAndCancellationFailures()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(2, 1, 1, [(0, 0, 0), (1, 0, 0)]);

        Ra2VoxelNormalBakeResult ts = Ra2VoxelNormalBaker.Bake(
            snapshot,
            Ra2VxlNormalPaletteKind.TiberianSun,
            new Ra2VoxelNormalBakeOptions(smoothingIterations: 0));
        Ra2VoxelNormalBakeResult limited = Ra2VoxelNormalBaker.Bake(snapshot, maximumSampleCount: 1);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Ra2VoxelNormalBakeResult cancelled = Ra2VoxelNormalBaker.Bake(snapshot, cancellationToken: cancellation.Token);

        Assert.True(ts.IsSuccess, ts.Message);
        Assert.Equal(Ra2VxlNormalPaletteKind.TiberianSun, ts.Field!.PaletteKind);
        Assert.All(ts.Field.Samples, sample => Assert.InRange((int)sample.NormalIndex, 0, 35));
        Assert.Equal(Ra2VoxelNormalBakeFailureKind.ResourceLimitExceeded, limited.FailureKind);
        Assert.Null(limited.Field);
        Assert.Equal(Ra2VoxelNormalBakeFailureKind.Cancelled, cancelled.FailureKind);
        Assert.Null(cancelled.Field);
    }

    private static Ra2VoxelSurfaceProjection AssertSuccess(Ra2VoxelSurfaceProjectionResult result)
    {
        Assert.True(result.IsSuccess, result.Message);
        return Assert.IsType<Ra2VoxelSurfaceProjection>(result.Projection);
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot(
        int xSize,
        int ySize,
        int zSize,
        IEnumerable<(int X, int Y, int Z)> coordinates)
    {
        Ra2VoxelPaletteProfile palette = new(
            "surface-normal-test",
            Enumerable.Range(0, 256).Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index)),
            [0],
            []);
        Ra2VoxelPartDescriptor part = new(
            "body",
            Ra2VoxelAssemblyPartRole.Body,
            "Body",
            "surface-normal-test",
            xSize,
            ySize,
            zSize);
        return new(
            "SURFACE_NORMAL_TEST",
            part,
            palette,
            coordinates.Select(value => new Ra2VoxelCell(new(value.X, value.Y, value.Z), 60)));
    }
}
