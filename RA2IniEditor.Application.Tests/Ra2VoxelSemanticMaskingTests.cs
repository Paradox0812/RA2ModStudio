using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelSemanticMaskingTests
{
    [Fact]
    public void Evidence_IsDeterministicHashBoundAndMirrorPaired()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2VoxelSemanticEvidencePackage first = Ra2VoxelSemanticEvidenceBuilder.Build(source);
        Ra2VoxelSemanticEvidencePackage second = Ra2VoxelSemanticEvidenceBuilder.Build(source);

        Assert.Equal(source.CanonicalHash, first.SourceSnapshotHash);
        Assert.Equal(first.PackageHash, second.PackageHash);
        Assert.InRange(first.Regions.Count, 1, 24);
        Assert.All(first.Regions, region =>
        {
            Assert.Equal(source.OccupancyCount, region.Selected.Count);
            Assert.Contains(first.Regions, candidate => candidate.RegionId == region.MirrorRegionId);
        });
        Assert.Contains("no image pixels", first.ToPromptText("车顶中央可能是炮塔"), StringComparison.Ordinal);
    }

    [Fact]
    public void HumanOverride_WinsAndAgentCannotApproveRemap()
    {
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(CreateSnapshot());
        string region = evidence.Regions[0].RegionId;
        Ra2VoxelSemanticAssignment ai = new(region, Ra2VoxelSemanticPartRole.BodyShell,
            Ra2VoxelSemanticMaterialRole.PaintedSurface, Ra2VoxelSemanticRemapIntent.ExplicitlyApproved, 0.7d, "ai");
        Ra2VoxelSemanticAssignment human = new(region, Ra2VoxelSemanticPartRole.Wheel,
            Ra2VoxelSemanticMaterialRole.Rubber, Ra2VoxelSemanticRemapIntent.ExplicitlyApproved, 1d, "human");

        Ra2VoxelSemanticEffectiveAssignment effective = Ra2VoxelSemanticLayerResolver.Resolve(evidence, [ai], [human])
            .Single(value => value.RegionId == region);
        Assert.Equal(Ra2VoxelSemanticAssignmentSource.HumanOverride, effective.Source);
        Assert.Equal(Ra2VoxelSemanticPartRole.Wheel, effective.PartRole);
        Assert.Equal(Ra2VoxelSemanticMaterialRole.Rubber, effective.MaterialRole);
        Assert.Equal(Ra2VoxelSemanticRemapIntent.ExplicitlyApproved, effective.RemapIntent);

        Ra2VoxelSemanticEffectiveAssignment aiOnly = Ra2VoxelSemanticLayerResolver.Resolve(evidence, [ai], [])
            .Single(value => value.RegionId == region);
        Assert.NotEqual(Ra2VoxelSemanticRemapIntent.ExplicitlyApproved, aiOnly.RemapIntent);
    }

    [Fact]
    public void Integration_UsesExistingColourizerAndPreservesGeometry()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2CompiledVoxelStylePlan plan = CreatePlan(source.Palette);
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(source);
        string region = evidence.Regions[0].RegionId;
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> effective = Ra2VoxelSemanticLayerResolver.Resolve(evidence, [],
        [
            new(region, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber,
                Ra2VoxelSemanticRemapIntent.None, 1d, "人工")
        ]);

        Ra2VoxelSemanticStyleIntegrationResult integrated = Ra2VoxelSemanticStyleIntegrator.Integrate(plan, evidence, effective);
        Ra2VoxelColourizationResult result = Ra2VoxelColourizer.Colourize(source, integrated.Plan, integrated.Masks);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(result.Facts!.GeometryAndOccupancyUnchanged);
        Assert.Equal(source.Cells.Select(cell => cell.Coordinate), result.Snapshot!.Cells.Select(cell => cell.Coordinate));
        Assert.Contains(result.Snapshot.Cells, cell => cell.PaletteIndex == 25);
    }

    [Fact]
    public void SurfaceBrush_IsMirrorAtomicAndCellOverrideWins()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(source);
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> regions = Ra2VoxelSemanticLayerResolver.Resolve(
            evidence,
            evidence.Regions.Select(region => new Ra2VoxelSemanticAssignment(
                region.RegionId,
                Ra2VoxelSemanticPartRole.BodyShell,
                Ra2VoxelSemanticMaterialRole.PaintedSurface,
                Ra2VoxelSemanticRemapIntent.None,
                0.8d,
                "AI")),
            []);
        Ra2VoxelSemanticManualMaskLayer empty = new(source.CanonicalHash, source.OccupancyCount);
        Ra2VoxelSemanticAssignment brush = new("brush", Ra2VoxelSemanticPartRole.Wheel,
            Ra2VoxelSemanticMaterialRole.Rubber, Ra2VoxelSemanticRemapIntent.None, 1d, "人工画笔");

        Ra2VoxelSemanticBrushResult edited = Ra2VoxelSemanticMaskEditor.ApplySurfaceBrush(
            source, empty, new(0, 0, 0), radius: 0, mirror: true,
            Ra2VoxelSemanticBrushMode.Paint, brush);

        Assert.True(edited.IsSuccess, edited.Message);
        Assert.Equal(2, edited.AffectedCellCount);
        Ra2VoxelSemanticMaskComposition composition = Ra2VoxelSemanticMaskComposer.Compose(
            source, evidence, regions, edited.Layer);
        int left = source.Cells.Select((cell, index) => (cell, index)).Single(value => value.cell.Coordinate == new Ra2VoxelCoordinate(0, 0, 0)).index;
        int right = source.Cells.Select((cell, index) => (cell, index)).Single(value => value.cell.Coordinate == new Ra2VoxelCoordinate(3, 0, 0)).index;
        Assert.Equal(Ra2VoxelSemanticMaterialRole.Rubber, composition[left].MaterialRole);
        Assert.Equal(Ra2VoxelSemanticMaterialRole.Rubber, composition[right].MaterialRole);
        Assert.All([composition[left], composition[right]], value =>
            Assert.Equal(Ra2VoxelSemanticAssignmentSource.HumanOverride, value.Source));
    }

    [Fact]
    public void Erase_RestoresRegionSeedAndStaleLayerIsRejected()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2VoxelSemanticManualMaskLayer empty = new(source.CanonicalHash, source.OccupancyCount);
        Ra2VoxelSemanticAssignment brush = new("brush", Ra2VoxelSemanticPartRole.Wheel,
            Ra2VoxelSemanticMaterialRole.Rubber, Ra2VoxelSemanticRemapIntent.None, 1d, "人工画笔");
        Ra2VoxelSemanticBrushResult painted = Ra2VoxelSemanticMaskEditor.ApplySurfaceBrush(
            source, empty, new(0, 0, 0), 0, false, Ra2VoxelSemanticBrushMode.Paint, brush);
        Ra2VoxelSemanticBrushResult erased = Ra2VoxelSemanticMaskEditor.ApplySurfaceBrush(
            source, painted.Layer, new(0, 0, 0), 0, false, Ra2VoxelSemanticBrushMode.Erase, null);
        Assert.True(erased.IsSuccess, erased.Message);
        Assert.Empty(erased.Layer.Overrides);

        Ra2VoxelSemanticManualMaskLayer stale = new(new string('A', 64), source.OccupancyCount);
        Assert.Equal(Ra2VoxelSemanticBrushFailureKind.SnapshotMismatch,
            Ra2VoxelSemanticMaskEditor.ApplySurfaceBrush(source, stale, new(0, 0, 0), 0, false,
                Ra2VoxelSemanticBrushMode.Erase, null).FailureKind);
    }

    [Fact]
    public void SurfaceStroke_DeduplicatesSeedsAndIsOrderIndependent()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2VoxelSemanticManualMaskLayer empty = new(source.CanonicalHash, source.OccupancyCount);
        Ra2VoxelSemanticAssignment brush = new("brush", Ra2VoxelSemanticPartRole.Turret,
            Ra2VoxelSemanticMaterialRole.BareMetal, Ra2VoxelSemanticRemapIntent.None, 1d, "人工连续画笔");
        Ra2VoxelCoordinate first = new(0, 0, 0);
        Ra2VoxelCoordinate second = new(0, 1, 0);

        Ra2VoxelSemanticBrushResult forward = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, empty, [first, second, first], 0, false, Ra2VoxelSemanticBrushMode.Paint, brush);
        Ra2VoxelSemanticBrushResult reverse = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, empty, [second, first], 0, false, Ra2VoxelSemanticBrushMode.Paint, brush);

        Assert.True(forward.IsSuccess, forward.Message);
        Assert.True(reverse.IsSuccess, reverse.Message);
        Assert.Equal(2, forward.AffectedCellCount);
        Assert.Equal(forward.Layer.LayerHash, reverse.Layer.LayerHash);
        Assert.Equal(forward.Layer.Overrides, reverse.Layer.Overrides);
    }

    [Fact]
    public void SurfaceStroke_IsAtomicForMirrorEraseInvalidCellsAndResourceLimits()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2VoxelSemanticManualMaskLayer empty = new(source.CanonicalHash, source.OccupancyCount);
        Ra2VoxelSemanticAssignment brush = new("brush", Ra2VoxelSemanticPartRole.Wheel,
            Ra2VoxelSemanticMaterialRole.Rubber, Ra2VoxelSemanticRemapIntent.None, 1d, "人工连续画笔");

        Ra2VoxelSemanticBrushResult painted = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, empty, [new(0, 0, 0), new(0, 1, 0)], 0, true,
            Ra2VoxelSemanticBrushMode.Paint, brush);
        Assert.True(painted.IsSuccess, painted.Message);
        Assert.Equal(4, painted.AffectedCellCount);

        Ra2VoxelSemanticBrushResult erased = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, painted.Layer, [new(0, 0, 0), new(0, 1, 0)], 0, true,
            Ra2VoxelSemanticBrushMode.Erase, null);
        Assert.True(erased.IsSuccess, erased.Message);
        Assert.Empty(erased.Layer.Overrides);

        Ra2VoxelSemanticBrushResult invalid = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, empty, [new(0, 0, 0), new(99, 99, 99)], 0, false,
            Ra2VoxelSemanticBrushMode.Paint, brush);
        Assert.Equal(Ra2VoxelSemanticBrushFailureKind.CellNotFound, invalid.FailureKind);
        Assert.Same(empty, invalid.Layer);

        IEnumerable<Ra2VoxelCoordinate> excessive = Enumerable.Range(0, Ra2VoxelSemanticMaskEditor.MaximumStrokeSeedCount + 1)
            .Select(index => new Ra2VoxelCoordinate(index, 0, 0));
        Ra2VoxelSemanticBrushResult limited = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, empty, excessive, 0, false, Ra2VoxelSemanticBrushMode.Paint, brush);
        Assert.Equal(Ra2VoxelSemanticBrushFailureKind.ResourceLimitExceeded, limited.FailureKind);
        Assert.Same(empty, limited.Layer);
    }

    [Fact]
    public void SurfaceStroke_SingleSeedMatchesSurfaceBrushAndEmptyStrokeDoesNotChangeLayer()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2VoxelSemanticManualMaskLayer empty = new(source.CanonicalHash, source.OccupancyCount);
        Ra2VoxelSemanticAssignment brush = new("brush", Ra2VoxelSemanticPartRole.Antenna,
            Ra2VoxelSemanticMaterialRole.BareMetal, Ra2VoxelSemanticRemapIntent.None, 1d, "人工画笔");
        Ra2VoxelCoordinate seed = new(0, 0, 0);

        Ra2VoxelSemanticBrushResult click = Ra2VoxelSemanticMaskEditor.ApplySurfaceBrush(
            source, empty, seed, 2, true, Ra2VoxelSemanticBrushMode.Paint, brush);
        Ra2VoxelSemanticBrushResult stroke = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, empty, [seed], 2, true, Ra2VoxelSemanticBrushMode.Paint, brush);
        Assert.Equal(click.FailureKind, stroke.FailureKind);
        Assert.Equal(click.AffectedCellCount, stroke.AffectedCellCount);
        Assert.Equal(click.Layer.LayerHash, stroke.Layer.LayerHash);

        Ra2VoxelSemanticBrushResult noSeeds = Ra2VoxelSemanticMaskEditor.ApplySurfaceStroke(
            source, empty, [], 0, false, Ra2VoxelSemanticBrushMode.Erase, null);
        Assert.Equal(Ra2VoxelSemanticBrushFailureKind.EmptyStroke, noSeeds.FailureKind);
        Assert.Same(empty, noSeeds.Layer);
    }

    [Fact]
    public void ComposedIntegration_UsesFineMasksWithoutChangingGeometry()
    {
        Ra2VoxelSceneSnapshot source = CreateSnapshot();
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(source);
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> regions = Ra2VoxelSemanticLayerResolver.Resolve(evidence, [], []);
        int index = source.Cells.Select((cell, index) => (cell, index))
            .Single(value => value.cell.Coordinate == new Ra2VoxelCoordinate(0, 0, 0)).index;
        Ra2VoxelSemanticManualMaskLayer layer = new(source.CanonicalHash, source.OccupancyCount,
        [
            new(index, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber,
                Ra2VoxelSemanticRemapIntent.None, "人工")
        ]);
        Ra2VoxelSemanticMaskComposition composition = Ra2VoxelSemanticMaskComposer.Compose(source, evidence, regions, layer);

        Ra2VoxelSemanticStyleIntegrationResult integrated = Ra2VoxelSemanticStyleIntegrator.Integrate(CreatePlan(source.Palette), composition);
        Ra2VoxelColourizationResult result = Ra2VoxelColourizer.Colourize(source, integrated.Plan, integrated.Masks);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(result.Facts!.GeometryAndOccupancyUnchanged);
        Assert.Equal(source.Cells.Select(cell => cell.Coordinate), result.Snapshot!.Cells.Select(cell => cell.Coordinate));
        Assert.Equal((byte)25, result.Snapshot.Cells[index].PaletteIndex);
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256).Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value)).ToArray();
        colours[0] = new(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new("semantic-test", colours, [0], [16]);
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "semantic", 4, 8, 6);
        IEnumerable<Ra2VoxelCell> cells = from x in Enumerable.Range(0, 4)
                                         from y in Enumerable.Range(0, 8)
                                         from z in Enumerable.Range(0, 4)
                                         select new Ra2VoxelCell(new(x, y, z), 60);
        return new("semantic", part, palette, cells);
    }

    private static Ra2CompiledVoxelStylePlan CreatePlan(Ra2VoxelPaletteProfile palette)
    {
        Ra2VoxelStylePlanDefinition definition = new(
            "semantic", "semantic", new string('A', 64), palette.ProfileHash, "test/1", "fixture/1",
            Ra2VoxelStyleRemapPolicy.None, "body.dark",
            [
                new("body.base", Ra2VoxelStyleRoleCategory.BodyBase, 60, null, ["test"]),
                new("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, 40, null, ["test"]),
                new("rubber", Ra2VoxelStyleRoleCategory.Rubber, 25, null, ["test"])
            ],
            [
                new(Ra2VoxelStyleRegionKind.WholePart, "body.base", Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["test"]),
                new(Ra2VoxelStyleRegionKind.Interior, "body.dark", Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["test"])
            ]);
        Ra2VoxelStylePlanCompilationResult result = Ra2VoxelStylePlanCompiler.Compile(definition, palette, ["test"]);
        Assert.True(result.IsSuccess, result.Message);
        return result.Plan!;
    }
}
