using System.Windows.Media;
using System.Windows.Media.Media3D;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelViewportSceneBuilderTests
{
    [Fact]
    public void Build_UsesCanonicalVisibleFacesAndProducesFrozenCentredGeometry()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60));

        Ra2VoxelViewportSceneBuildResult result = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            geometryMask: null,
            Ra2VoxelViewportColourMode.Palette);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(6, result.FaceCount);
        Assert.Equal(6, result.HitMap.FaceCount);
        Assert.Equal(1, result.MaterialCount);
        Assert.True(result.Model!.IsFrozen);
        GeometryModel3D model = Assert.IsType<GeometryModel3D>(Assert.Single(result.Model.Children));
        MeshGeometry3D mesh = Assert.IsType<MeshGeometry3D>(model.Geometry);
        Assert.Equal(24, mesh.Positions.Count);
        Assert.Equal(36, mesh.TriangleIndices.Count);
        Assert.Equal(new Rect3D(-1, -1, -1, 2, 2, 2), result.Bounds);
        for (int face = 0; face < 6; face++)
        {
            int first = face * 4;
            Assert.True(result.HitMap.TryResolve(model, first, first + 1, first + 2, out Ra2VoxelCoordinate firstTriangle));
            Assert.True(result.HitMap.TryResolve(model, first, first + 2, first + 3, out Ra2VoxelCoordinate secondTriangle));
            Assert.Equal(new Ra2VoxelCoordinate(0, 0, 0), firstTriangle);
            Assert.Equal(firstTriangle, secondTriangle);
        }
        Assert.False(result.HitMap.TryResolve(model, 0, 1, 7, out _));
    }

    [Fact]
    public void Build_GroupsPaletteMaterialsAndUsesExistingRegionReviewColour()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(1, 0, 0), 80));
        Ra2VoxelViewportSceneBuildResult palette = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            geometryMask: null,
            Ra2VoxelViewportColourMode.Palette);
        Ra2VoxelGeometryRegionMask mask = Ra2VoxelColourizer.BuildGeometryMask(snapshot);
        Ra2VoxelViewportSceneBuildResult region = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            mask,
            Ra2VoxelViewportColourMode.GeometryRegion);

        Assert.True(palette.IsSuccess, palette.Message);
        Assert.Equal(10, palette.FaceCount);
        Assert.Equal(2, palette.MaterialCount);
        Ra2VoxelCoordinate[] paletteResolved = palette.Model!.Children.Cast<GeometryModel3D>()
            .SelectMany(model => ResolveAllFaces(palette, model))
            .ToArray();
        Assert.Equal(5, paletteResolved.Count(value => value == new Ra2VoxelCoordinate(0, 0, 0)));
        Assert.Equal(5, paletteResolved.Count(value => value == new Ra2VoxelCoordinate(1, 0, 0)));
        Assert.True(region.IsSuccess, region.Message);
        GeometryModel3D regionModel = Assert.IsType<GeometryModel3D>(Assert.Single(region.Model!.Children));
        MaterialGroup material = Assert.IsType<MaterialGroup>(regionModel.Material);
        DiffuseMaterial diffuse = Assert.IsType<DiffuseMaterial>(material.Children[0]);
        SolidColorBrush brush = Assert.IsType<SolidColorBrush>(diffuse.Brush);
        Assert.Equal(Color.FromRgb(255, 188, 32), brush.Color);
        Assert.Equal(region.FaceCount, region.HitMap.FaceCount);
        Ra2VoxelCoordinate[] resolved = ResolveAllFaces(region, regionModel);
        Assert.Equal(5, resolved.Count(value => value == new Ra2VoxelCoordinate(0, 0, 0)));
        Assert.Equal(5, resolved.Count(value => value == new Ra2VoxelCoordinate(1, 0, 0)));
    }

    [Fact]
    public void Build_FailsTypedWhenFaceBudgetOrRegionMaskContractIsInvalid()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60));
        Ra2VoxelViewportSceneBuildResult limited = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            geometryMask: null,
            Ra2VoxelViewportColourMode.Palette,
            maximumFaceCount: 5);
        Ra2VoxelGeometryRegionMask wrongMask = new(new string('A', 64), [(byte)Ra2VoxelGeometryRegionBits.EdgeOrRidge]);
        Ra2VoxelViewportSceneBuildResult invalidMask = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            wrongMask,
            Ra2VoxelViewportColourMode.GeometryRegion);

        Assert.Equal(Ra2VoxelViewportSceneFailureKind.ResourceLimitExceeded, limited.FailureKind);
        Assert.Equal(0, limited.HitMap.FaceCount);
        Assert.Equal(Ra2VoxelViewportSceneFailureKind.InvalidRegionMask, invalidMask.FailureKind);
    }

    [Fact]
    public void BuildDifference_ShowsOnlyAddedRemovedAndUnchangedMaterials()
    {
        Ra2VoxelSceneSnapshot comparison = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(1, 0, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 1, 0), 60));
        Ra2VoxelSceneSnapshot candidate = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 1, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(1, 1, 0), 60));
        Ra2VoxelViewportSceneBuildResult result = Ra2VoxelViewportSceneBuilder.Build(
            candidate,
            geometryMask: null,
            Ra2VoxelViewportColourMode.Difference,
            comparison);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(3, result.MaterialCount);
        Color[] colours = result.Model!.Children.Cast<GeometryModel3D>()
            .Select(model => Assert.IsType<SolidColorBrush>(
                Assert.IsType<DiffuseMaterial>(Assert.IsType<MaterialGroup>(model.Material).Children[0]).Brush).Color)
            .ToArray();
        Assert.Contains(Color.FromRgb(64, 170, 92), colours);
        Assert.Contains(Color.FromRgb(214, 72, 72), colours);
        Assert.Contains(Color.FromArgb(52, 128, 135, 146), colours);
        Assert.DoesNotContain(Color.FromRgb(61, 126, 220), colours);
    }

    [Fact]
    public void BuildSemanticStructure_UsesTheFixedSymmetricCoreColourAndRejectsStalePartition()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(1, 0, 0), 60));
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(snapshot);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(snapshot, snapshot);
        Ra2VoxelSymmetryEvidencePackage evidence = Ra2VoxelSymmetryEvidenceBuilder.Build(
            snapshot, snapshot, analysis.ProtectionMask!, coverage).Package!;
        Ra2VoxelSemanticPartition partition = new(
            evidence,
            evidence.Regions.Select(region => new Ra2VoxelSemanticRegionDecision(
                region.RegionId,
                Ra2VoxelSymmetryDisposition.SymmetricCore,
                0.98d,
                0.98d,
                "fixture",
                true)));

        Ra2VoxelViewportSceneBuildResult result = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            geometryMask: null,
            Ra2VoxelViewportColourMode.SemanticStructure,
            semanticPartition: partition);

        Assert.True(result.IsSuccess, result.Message);
        GeometryModel3D model = Assert.IsType<GeometryModel3D>(Assert.Single(result.Model!.Children));
        MaterialGroup material = Assert.IsType<MaterialGroup>(model.Material);
        SolidColorBrush brush = Assert.IsType<SolidColorBrush>(Assert.IsType<DiffuseMaterial>(material.Children[0]).Brush);
        Assert.Equal(Color.FromRgb(35, 190, 196), brush.Color);

        Ra2VoxelSceneSnapshot stale = CreateSnapshot(new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60));
        Ra2VoxelViewportSceneBuildResult invalid = Ra2VoxelViewportSceneBuilder.Build(
            stale,
            geometryMask: null,
            Ra2VoxelViewportColourMode.SemanticStructure,
            semanticPartition: partition);
        Assert.Equal(Ra2VoxelViewportSceneFailureKind.InvalidRegionMask, invalid.FailureKind);
    }

    [Fact]
    public void Builder_ProjectsAcceptedSemanticAssignmentsWithoutChangingSnapshot()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(1, 0, 0), 60));
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> assignments = Ra2VoxelSemanticLayerResolver.Resolve(
            evidence,
            [new(evidence.Regions[0].RegionId, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber,
                Ra2VoxelSemanticRemapIntent.None, 0.8d, "fixture")],
            []);

        Ra2VoxelViewportSceneBuildResult result = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            geometryMask: null,
            Ra2VoxelViewportColourMode.SemanticMask,
            semanticEvidence: evidence,
            semanticAssignments: assignments);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(2, snapshot.OccupancyCount);
        Assert.Equal((byte)60, snapshot.Cells[0].PaletteIndex);
    }

    [Fact]
    public void Builder_PrefersFineSemanticCompositionForCellColours()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60),
            new Ra2VoxelCell(new Ra2VoxelCoordinate(1, 0, 0), 60));
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> assignments = Ra2VoxelSemanticLayerResolver.Resolve(evidence, [], []);
        Ra2VoxelSemanticManualMaskLayer layer = new(snapshot.CanonicalHash, snapshot.OccupancyCount,
        [
            new(0, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber,
                Ra2VoxelSemanticRemapIntent.None, "fixture")
        ]);
        Ra2VoxelSemanticMaskComposition composition = Ra2VoxelSemanticMaskComposer.Compose(snapshot, evidence, assignments, layer);

        Ra2VoxelViewportSceneBuildResult result = Ra2VoxelViewportSceneBuilder.Build(
            snapshot,
            geometryMask: null,
            Ra2VoxelViewportColourMode.SemanticMask,
            semanticEvidence: evidence,
            semanticAssignments: assignments,
            semanticComposition: composition);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(result.MaterialCount >= 2);
    }

    [Fact]
    public void Builder_UsesFrozenPartAndMaterialReviewColoursWithoutChangingComposition()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60));
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> assignments = Ra2VoxelSemanticLayerResolver.Resolve(
            evidence,
            [new(evidence.Regions[0].RegionId, Ra2VoxelSemanticPartRole.Turret, Ra2VoxelSemanticMaterialRole.Glass,
                Ra2VoxelSemanticRemapIntent.None, 0.8d, "fixture")],
            []);

        Ra2VoxelViewportSceneBuildResult part = Ra2VoxelViewportSceneBuilder.Build(
            snapshot, null, Ra2VoxelViewportColourMode.SemanticMask,
            semanticEvidence: evidence, semanticAssignments: assignments,
            semanticReviewDimension: Ra2VoxelSemanticReviewDimension.Part);
        Ra2VoxelViewportSceneBuildResult material = Ra2VoxelViewportSceneBuilder.Build(
            snapshot, null, Ra2VoxelViewportColourMode.SemanticMask,
            semanticEvidence: evidence, semanticAssignments: assignments,
            semanticReviewDimension: Ra2VoxelSemanticReviewDimension.Material);

        Assert.Equal(Color.FromRgb(170, 51, 119), GetDiffuseColour(part));
        Assert.Equal(Color.FromRgb(45, 168, 210), GetDiffuseColour(material));
        Assert.Equal(8, Ra2VoxelSemanticReviewPalette.PartLegend.Count);
        Assert.Equal(8, Ra2VoxelSemanticReviewPalette.MaterialLegend.Count);
    }

    [Theory]
    [InlineData((int)Ra2VoxelSemanticPartRole.BodyShell, 68, 119, 170)]
    [InlineData((int)Ra2VoxelSemanticPartRole.Turret, 170, 51, 119)]
    [InlineData((int)Ra2VoxelSemanticPartRole.Barrel, 238, 119, 51)]
    [InlineData((int)Ra2VoxelSemanticPartRole.Wheel, 34, 136, 51)]
    [InlineData((int)Ra2VoxelSemanticPartRole.Track, 204, 187, 68)]
    [InlineData((int)Ra2VoxelSemanticPartRole.Antenna, 51, 170, 221)]
    [InlineData((int)Ra2VoxelSemanticPartRole.Attachment, 238, 102, 119)]
    [InlineData((int)Ra2VoxelSemanticPartRole.Unknown, 138, 143, 152)]
    public void PartReviewPalette_MatchesApprovedContract(
        int role,
        byte red,
        byte green,
        byte blue)
    {
        Assert.Equal(new Ra2Rgba32(red, green, blue),
            Ra2VoxelSemanticReviewPalette.PartColour((Ra2VoxelSemanticPartRole)role));
    }

    [Theory]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.PaintedSurface, 91, 158, 82)]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.Glass, 45, 168, 210)]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.Rubber, 46, 49, 55)]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.BareMetal, 170, 178, 184)]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.Light, 246, 212, 75)]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.DarkOpening, 36, 28, 43)]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.Accent, 224, 104, 62)]
    [InlineData((int)Ra2VoxelSemanticMaterialRole.Unknown, 148, 95, 210)]
    public void MaterialReviewPalette_MatchesApprovedContract(
        int role,
        byte red,
        byte green,
        byte blue)
    {
        Assert.Equal(new Ra2Rgba32(red, green, blue),
            Ra2VoxelSemanticReviewPalette.MaterialColour((Ra2VoxelSemanticMaterialRole)role));
    }

    [Fact]
    public void CoordinateOverlay_IsFrozenBoundedAndUsesCanonicalVoxelTransform()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot(
            new Ra2VoxelCell(new Ra2VoxelCoordinate(0, 0, 0), 60));
        Model3DGroup overlay = Ra2VoxelViewportSceneBuilder.BuildCoordinateOverlay(
            snapshot,
            [new Ra2VoxelCoordinate(0, 0, 0)],
            new Ra2Rgba32(255, 212, 0, 210));

        Assert.True(overlay.IsFrozen);
        GeometryModel3D model = Assert.IsType<GeometryModel3D>(Assert.Single(overlay.Children));
        MeshGeometry3D mesh = Assert.IsType<MeshGeometry3D>(model.Geometry);
        Assert.Equal(24, mesh.Positions.Count);
        Assert.Throws<ArgumentException>(() => Ra2VoxelViewportSceneBuilder.BuildCoordinateOverlay(
            snapshot, [new Ra2VoxelCoordinate(9, 9, 9)], new Ra2Rgba32(255, 0, 0)));
    }

    private static Color GetDiffuseColour(Ra2VoxelViewportSceneBuildResult result)
    {
        Assert.True(result.IsSuccess, result.Message);
        GeometryModel3D model = Assert.IsType<GeometryModel3D>(Assert.Single(result.Model!.Children));
        MaterialGroup group = Assert.IsType<MaterialGroup>(model.Material);
        return Assert.IsType<SolidColorBrush>(Assert.IsType<DiffuseMaterial>(group.Children[0]).Brush).Color;
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot(params Ra2VoxelCell[] cells)
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)(255 - index), (byte)(index / 2)))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new("viewport-test", colours, [0]);
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "viewport", 2, 2, 2);
        return new("viewport", part, palette, cells);
    }

    private static Ra2VoxelCoordinate[] ResolveAllFaces(
        Ra2VoxelViewportSceneBuildResult result,
        GeometryModel3D model)
    {
        MeshGeometry3D mesh = Assert.IsType<MeshGeometry3D>(model.Geometry);
        int faceCount = mesh.Positions.Count / 4;
        Ra2VoxelCoordinate[] coordinates = new Ra2VoxelCoordinate[faceCount];
        for (int face = 0; face < faceCount; face++)
        {
            int first = face * 4;
            Assert.True(result.HitMap.TryResolve(model, first, first + 1, first + 2, out coordinates[face]));
        }
        return coordinates;
    }
}
