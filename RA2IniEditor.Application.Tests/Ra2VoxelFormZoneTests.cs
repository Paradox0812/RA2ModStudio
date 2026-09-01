using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelFormZoneTests
{
    [Fact]
    public void ForwardDirectionSelection_IsDeterministicAndHumanOwned()
    {
        Ra2VoxelSceneSnapshot snapshot = Snapshot();
        string composition = new string('A', 64);

        Ra2VoxelForwardDirectionSelectionResult first =
            Ra2VoxelForwardDirectionSelection.Create(snapshot, composition, Ra2VoxelForwardDirection.PositiveY);
        Ra2VoxelForwardDirectionSelectionResult second =
            Ra2VoxelForwardDirectionSelection.Create(snapshot, composition, Ra2VoxelForwardDirection.PositiveY);

        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Assert.Equal(first.Selection!.SelectionHash, second.Selection!.SelectionHash);
        Assert.Equal("HumanManualSelection", first.Selection.Source);
        Assert.True(first.Selection.IsConfirmed);
    }

    [Fact]
    public void Projector_UsesUnknownLongitudinalEndsWithoutGuessingFront()
    {
        Ra2VoxelSceneSnapshot snapshot = Snapshot();
        string composition = new string('B', 64);
        Ra2VoxelForwardDirectionSelection orientation = Assert.IsType<Ra2VoxelForwardDirectionSelection>(
            Ra2VoxelForwardDirectionSelection.Create(snapshot, composition, Ra2VoxelForwardDirection.Unknown).Selection);

        Ra2VoxelFormZoneProjectionResult result = Ra2VoxelFormZoneProjector.Project(
            snapshot, composition, orientation, Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground));

        Assert.True(result.IsSuccess, result.Message);
        Assert.Contains("ForwardDirectionNotConfirmed", result.Projection!.Diagnostics);
        Assert.Contains(Enumerable.Range(0, result.Projection.CellCount), index =>
            result.Projection.Contains(index, Ra2VoxelFormZone.LongitudinalEndUnknown));
        Assert.DoesNotContain(Enumerable.Range(0, result.Projection.CellCount), index =>
            result.Projection.Contains(index, Ra2VoxelFormZone.FrontEnd));
    }

    [Fact]
    public void Projector_SwapsFrontAndRearWhenHumanDirectionFlips()
    {
        Ra2VoxelSceneSnapshot snapshot = Snapshot();
        string composition = new string('C', 64);
        Ra2VoxelForwardDirectionSelection positive = Selection(Ra2VoxelForwardDirection.PositiveY);
        Ra2VoxelForwardDirectionSelection negative = Selection(Ra2VoxelForwardDirection.NegativeY);

        Ra2VoxelFormZoneProjection positiveProjection = Assert.IsType<Ra2VoxelFormZoneProjection>(
            Ra2VoxelFormZoneProjector.Project(snapshot, composition, positive,
                Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground)).Projection);
        Ra2VoxelFormZoneProjection negativeProjection = Assert.IsType<Ra2VoxelFormZoneProjection>(
            Ra2VoxelFormZoneProjector.Project(snapshot, composition, negative,
                Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground)).Projection);

        int positiveEnd = snapshot.Cells.ToList().FindIndex(value => value.Coordinate == new Ra2VoxelCoordinate(1, 3, 1));
        Assert.True(positiveProjection.Contains(positiveEnd, Ra2VoxelFormZone.FrontEnd));
        Assert.True(negativeProjection.Contains(positiveEnd, Ra2VoxelFormZone.RearEnd));
        Assert.NotEqual(positiveProjection.ProjectionHash, negativeProjection.ProjectionHash);

        Ra2VoxelForwardDirectionSelection Selection(Ra2VoxelForwardDirection direction) =>
            Assert.IsType<Ra2VoxelForwardDirectionSelection>(
                Ra2VoxelForwardDirectionSelection.Create(snapshot, composition, direction).Selection);
    }

    [Fact]
    public void Projector_ProducesContinuousPrimaryBodyZonesWithoutChangingSnapshot()
    {
        Ra2VoxelSceneSnapshot snapshot = Snapshot();
        string sourceHash = snapshot.CanonicalHash;
        string composition = new string('D', 64);
        Ra2VoxelForwardDirectionSelection orientation = Assert.IsType<Ra2VoxelForwardDirectionSelection>(
            Ra2VoxelForwardDirectionSelection.Create(snapshot, composition, Ra2VoxelForwardDirection.PositiveY).Selection);

        Ra2VoxelFormZoneProjection projection = Assert.IsType<Ra2VoxelFormZoneProjection>(
            Ra2VoxelFormZoneProjector.Project(snapshot, composition, orientation,
                Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground)).Projection);

        Assert.Equal(snapshot.OccupancyCount, projection.CellCount);
        Assert.Equal(sourceHash, snapshot.CanonicalHash);
        Assert.Contains(projection.Counts, value => value.Zone == Ra2VoxelFormZone.UpperPlane && value.CellCount > 0);
        Assert.Contains(projection.Counts, value => value.Zone == Ra2VoxelFormZone.SideShoulder && value.CellCount > 0);
        Assert.Contains(projection.Counts, value => value.Zone == Ra2VoxelFormZone.SideField && value.CellCount > 0);
        Assert.Contains(projection.Counts, value => value.Zone == Ra2VoxelFormZone.LowerSkirt && value.CellCount > 0);
    }

    [Fact]
    public void Projector_RejectsStaleCompositionIdentity()
    {
        Ra2VoxelSceneSnapshot snapshot = Snapshot();
        Ra2VoxelForwardDirectionSelection orientation = Assert.IsType<Ra2VoxelForwardDirectionSelection>(
            Ra2VoxelForwardDirectionSelection.Create(snapshot, new string('E', 64),
                Ra2VoxelForwardDirection.PositiveY).Selection);

        Ra2VoxelFormZoneProjectionResult result = Ra2VoxelFormZoneProjector.Project(
            snapshot, new string('F', 64), orientation,
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground));

        Assert.Equal(Ra2VoxelFormZoneProjectionFailureKind.CompositionMismatch, result.FailureKind);
        Assert.Null(result.Projection);
    }

    private static Ra2VoxelSceneSnapshot Snapshot()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        Ra2VoxelPaletteProfile palette = new("form-zone-test", colours, [0], [16, 17, 18, 19]);
        List<Ra2VoxelCell> cells = [];
        for (int z = 0; z < 4; z++)
        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 3; x++)
            cells.Add(new(new Ra2VoxelCoordinate(x, y, z), 72));
        return new("form-zone-scene",
            new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "form_zone_body", 3, 4, 4),
            palette, cells);
    }
}
