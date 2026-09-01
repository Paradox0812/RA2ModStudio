using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelFeatureScaleTests
{
    [Fact]
    public void Projector_ClassifiesSmallSemanticAttachmentAsSubPixelRisk()
    {
        Ra2VoxelSceneSnapshot snapshot = Snapshot();
        Ra2VoxelSemanticEffectiveAssignment[] assignments = snapshot.Cells.Select(cell =>
            cell.Coordinate == new Ra2VoxelCoordinate(1, 4, 2)
                ? Assignment(Ra2VoxelSemanticPartRole.Antenna, Ra2VoxelSemanticMaterialRole.BareMetal)
                : Assignment(Ra2VoxelSemanticPartRole.BodyShell, Ra2VoxelSemanticMaterialRole.PaintedSurface)).ToArray();
        Ra2VoxelSemanticMaskComposition composition = new(snapshot.CanonicalHash, assignments, new string('A', 64));
        Ra2VoxelForwardDirectionSelection orientation = Assert.IsType<Ra2VoxelForwardDirectionSelection>(
            Ra2VoxelForwardDirectionSelection.Create(snapshot, composition.CompositionHash,
                Ra2VoxelForwardDirection.PositiveY).Selection);
        Ra2VoxelFormZoneProjection zones = Assert.IsType<Ra2VoxelFormZoneProjection>(
            Ra2VoxelFormZoneProjector.Project(snapshot, composition.CompositionHash, orientation,
                Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground)).Projection);

        Ra2VoxelFeatureScaleProjection first = Ra2VoxelFeatureScaleProjector.Project(snapshot, composition, zones);
        Ra2VoxelFeatureScaleProjection second = Ra2VoxelFeatureScaleProjector.Project(snapshot, composition, zones);

        int antenna = snapshot.Cells.ToList().FindIndex(value =>
            value.Coordinate == new Ra2VoxelCoordinate(1, 4, 2));
        Assert.Equal(Ra2VoxelFeatureScale.SubPixelRisk, first[antenna]);
        Assert.Equal(first.ProjectionHash, second.ProjectionHash);
        Assert.Contains(first.Counts, value => value.Scale == Ra2VoxelFeatureScale.Macro && value.CellCount > 0);
    }

    [Fact]
    public void TechniqueCatalog_HasFiveDistinctSpatialAndAccentPolicies()
    {
        Ra2VoxelColourTechniquePolicy[] policies = Ra2VoxelColourTechniqueCatalog.All.ToArray();

        Assert.Equal(5, policies.Select(value => value.SpatialProfile).Distinct().Count());
        Assert.All(policies, value => Assert.InRange(value.PreferredBodyBandCount, 3, 6));
        Assert.All(policies, value => Assert.InRange(value.MaximumAccentVisibleShare, 0.001d, 0.25d));
        Assert.True(Ra2VoxelColourTechniqueCatalog.Find("subtle-matte-shading")!.MaximumAccentVisibleShare <
                    Ra2VoxelColourTechniqueCatalog.Find("balanced-rts-volume")!.MaximumAccentVisibleShare);
        Assert.False(Ra2VoxelColourTechniqueCatalog.Find("compact-unit-clarity")!.PreserveMesoDetails);
    }

    private static Ra2VoxelSceneSnapshot Snapshot()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index)).ToArray();
        colours[0] = new(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new("feature-scale-test", colours, [0],
            Enumerable.Range(16, 16).Select(value => (byte)value));
        List<Ra2VoxelCell> cells = [];
        for (int z = 0; z < 3; z++)
        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 3; x++)
            cells.Add(new(new Ra2VoxelCoordinate(x, y, z), 72));
        cells.Add(new(new Ra2VoxelCoordinate(1, 4, 2), 72));
        return new("feature-scale-scene",
            new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "feature_scale_body", 3, 5, 3),
            palette, cells);
    }

    private static Ra2VoxelSemanticEffectiveAssignment Assignment(
        Ra2VoxelSemanticPartRole part,
        Ra2VoxelSemanticMaterialRole material) =>
        new("fixture", part, material, Ra2VoxelSemanticRemapIntent.None,
            Ra2VoxelSemanticAssignmentSource.HumanOverride, 1d, "fixture");
}
