using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelColourizerTests
{
    [Fact]
    public void GeometryMask_UsesTechniqueSpecificEdgeCoverage()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCube();

        Ra2VoxelGeometryRegionMask subtle = Ra2VoxelColourizer.BuildGeometryMask(
            source, Ra2VoxelColourEdgePolicy.Subtle);
        Ra2VoxelGeometryRegionMask strong = Ra2VoxelColourizer.BuildGeometryMask(
            source, Ra2VoxelColourEdgePolicy.Strong);
        Ra2VoxelGeometryRegionMask none = Ra2VoxelColourizer.BuildGeometryMask(
            source, Ra2VoxelColourEdgePolicy.None);

        int subtleCount = Enumerable.Range(0, source.OccupancyCount)
            .Count(index => (subtle[index] & Ra2VoxelGeometryRegionBits.EdgeOrRidge) != 0);
        int strongCount = Enumerable.Range(0, source.OccupancyCount)
            .Count(index => (strong[index] & Ra2VoxelGeometryRegionBits.EdgeOrRidge) != 0);
        int noneCount = Enumerable.Range(0, source.OccupancyCount)
            .Count(index => (none[index] & Ra2VoxelGeometryRegionBits.EdgeOrRidge) != 0);

        Assert.Equal(8, subtleCount);
        Assert.Equal(12, strongCount);
        Assert.Equal(0, noneCount);
    }

    [Fact]
    public void GeometryMask_DistinguishesLongitudinalEndsFromLateralSides()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCuboid(3, 5, 3);

        Ra2VoxelGeometryRegionMask geometry = Ra2VoxelColourizer.BuildGeometryMask(source);

        AssertBits(new(1, 0, 1), Ra2VoxelGeometryRegionBits.LongitudinalEndExposed,
            Ra2VoxelGeometryRegionBits.LateralSideExposed);
        AssertBits(new(0, 2, 1), Ra2VoxelGeometryRegionBits.LateralSideExposed,
            Ra2VoxelGeometryRegionBits.LongitudinalEndExposed);

        void AssertBits(
            Ra2VoxelCoordinate coordinate,
            Ra2VoxelGeometryRegionBits included,
            Ra2VoxelGeometryRegionBits excluded)
        {
            int index = source.Cells.ToList().FindIndex(value => value.Coordinate == coordinate);
            Assert.True(index >= 0);
            Assert.True(geometry[index].HasFlag(included));
            Assert.False(geometry[index].HasFlag(excluded));
        }
    }

    [Fact]
    public void Colourizer_AppliesFixedGeometryOrderWithoutMutatingSource()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCube();
        Ra2CompiledVoxelStylePlan plan = CompilePlan(source.Palette, includeTextOnlyGlass: true);

        Ra2VoxelColourizationResult first = Ra2VoxelColourizer.Colourize(source, plan);
        Ra2VoxelColourizationResult second = Ra2VoxelColourizer.Colourize(source, plan);

        Assert.True(first.IsSuccess, first.Message);
        Assert.Equal(first.Snapshot!.CanonicalHash, second.Snapshot!.CanonicalHash);
        Assert.Equal(source.OccupancyCount, first.Snapshot.OccupancyCount);
        Assert.Equal(source.Cells.Select(cell => cell.Coordinate), first.Snapshot.Cells.Select(cell => cell.Coordinate));
        Assert.All(source.Cells, cell => Assert.Equal((byte)60, cell.PaletteIndex));
        AssertIndex(first.Snapshot, 1, 1, 1, 50); // interior
        AssertIndex(first.Snapshot, 1, 1, 2, 80); // top only
        AssertIndex(first.Snapshot, 0, 1, 1, 70); // side only
        AssertIndex(first.Snapshot, 1, 1, 0, 40); // underside only
        AssertIndex(first.Snapshot, 0, 0, 2, 90); // edge/ridge wins last
        Assert.True(first.Facts!.GeometryAndOccupancyUnchanged);
        Assert.False(first.Facts.IsUniformColour);
        Assert.True(first.Facts.ReviewFlags.HasFlag(Ra2VoxelColourReviewFlags.TextOnlyCoarseStyle));
        Assert.True(first.Facts.ReviewFlags.HasFlag(Ra2VoxelColourReviewFlags.SemanticMaskReviewRequired));
        Assert.Contains(first.Facts.UnresolvedRules, value => value.Contains("glass", StringComparison.Ordinal));
    }

    [Fact]
    public void Colourizer_ExplicitMaskCanApplyRemapAndIsHashBound()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCube();
        Ra2CompiledVoxelStylePlan plan = CompilePlan(source.Palette, includeRemap: true);
        byte[] selection = new byte[source.OccupancyCount];
        selection[0] = 1;
        Ra2VoxelExplicitMask mask = new("team-mask", source.CanonicalHash, selection);

        Ra2VoxelColourizationResult result = Ra2VoxelColourizer.Colourize(source, plan, [mask]);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal((byte)16, result.Snapshot!.Cells[0].PaletteIndex);
        Assert.Equal(1, result.Snapshot.Cells.Count(cell => source.Palette.IsRemap(cell.PaletteIndex)));
        Assert.True(result.Facts!.ReviewFlags.HasFlag(Ra2VoxelColourReviewFlags.RemapReviewRequired));

        Ra2VoxelExplicitMask wrong = new("team-mask", new string('A', 64), selection);
        Assert.Equal(
            Ra2VoxelColourizationFailureKind.MaskSnapshotMismatch,
            Ra2VoxelColourizer.Colourize(source, plan, [wrong]).FailureKind);
    }

    [Fact]
    public void Colourizer_RejectsMissingOrWrongShapeMaskAndPaletteMismatch()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCube();
        Ra2CompiledVoxelStylePlan remapPlan = CompilePlan(source.Palette, includeRemap: true);
        Assert.Equal(
            Ra2VoxelColourizationFailureKind.MissingMask,
            Ra2VoxelColourizer.Colourize(source, remapPlan).FailureKind);

        Ra2VoxelExplicitMask shortMask = new("team-mask", source.CanonicalHash, new byte[source.OccupancyCount - 1]);
        Assert.Equal(
            Ra2VoxelColourizationFailureKind.MaskShapeMismatch,
            Ra2VoxelColourizer.Colourize(source, remapPlan, [shortMask]).FailureKind);

        Ra2VoxelPaletteProfile other = CreatePalette("other");
        Ra2CompiledVoxelStylePlan otherPlan = CompilePlan(other);
        Assert.Equal(
            Ra2VoxelColourizationFailureKind.PaletteMismatch,
            Ra2VoxelColourizer.Colourize(source, otherPlan).FailureKind);
    }

    [Fact]
    public void Colourizer_ExistingCodecsRoundTripAllColoursAndCancellationIsTyped()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCube();
        Ra2VoxelColourizationResult result = Ra2VoxelColourizer.Colourize(source, CompilePlan(source.Palette));
        Assert.True(result.IsSuccess, result.Message);

        byte[] vox = Ra2MagicaVoxelCodec.Write(result.Snapshot!);
        using MemoryStream stream = new(vox, writable: false);
        Ra2VoxelSceneSnapshot decoded = Ra2MagicaVoxelCodec.Read(
            stream, "COLOUR_ROUNDTRIP", "body", Ra2VoxelAssemblyPartRole.Body, "Body", "coloured");
        Ra2VoxelSceneSnapshot coloured = Assert.IsType<Ra2VoxelSceneSnapshot>(result.Snapshot);
        Assert.Equal(coloured.Cells, decoded.Cells);
        Ra2VoxelSliceStackRaster raster = Ra2VoxelSliceStackCodec.Export(coloured, Ra2VxlseSliceDirection.Downward);
        Ra2VoxelSceneSnapshot slice = Ra2VoxelSliceStackCodec.Import(
            raster, "COLOUR_SLICE", coloured.Part, coloured.Palette);
        Assert.Equal(coloured.Cells, slice.Cells);

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Assert.Equal(
            Ra2VoxelColourizationFailureKind.Cancelled,
            Ra2VoxelColourizer.Colourize(source, CompilePlan(source.Palette), cancellationToken: cancellation.Token).FailureKind);
    }

    [Fact]
    public void ReviewPackage_ContainsOnlyHashBoundHeadlessArtifactsAndOptionalRemapMask()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCube();
        Ra2CompiledVoxelStylePlan plan = CompilePlan(source.Palette, includeTextOnlyGlass: true, includeRemap: true);
        byte[] selected = new byte[source.OccupancyCount];
        selected[0] = 1;
        Ra2VoxelExplicitMask mask = new("team-mask", source.CanonicalHash, selected);
        Ra2VoxelColourizationResult colourization = Ra2VoxelColourizer.Colourize(source, plan, [mask]);

        Ra2VoxelColourReviewPackageResult package = Ra2VoxelColourReviewPackageBuilder.Build(
            [new("built-in", new string('A', 64), 128)], source, plan, colourization, [mask]);

        Assert.True(package.IsSuccess, package.Message);
        Assert.Equal(8, package.Artifacts.Count);
        Assert.Equal(new[]
        {
            "body-coloured-slicestack.png", "body-coloured.vox", "colour-review-report.json",
            "compiled-style-plan.json", "palette-swatch.png", "region-mask.png", "remap-mask.png",
            "style-source-pack.json"
        }, package.Artifacts.Select(item => item.FileName).Order(StringComparer.Ordinal));
        Assert.All(package.Artifacts, artifact =>
        {
            Assert.Equal(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(artifact.Content.Span)), artifact.ContentSha256);
            Assert.DoesNotContain("H:\\", System.Text.Encoding.UTF8.GetString(artifact.Content.Span), StringComparison.OrdinalIgnoreCase);
        });

        Ra2VoxelReviewArtifact reportArtifact = package.Artifacts.Single(item => item.FileName == "colour-review-report.json");
        using JsonDocument report = JsonDocument.Parse(reportArtifact.Content);
        Assert.False(report.RootElement.GetProperty("claims").GetProperty("project_adopted").GetBoolean());
        Assert.False(report.RootElement.GetProperty("claims").GetProperty("vxl_generated").GetBoolean());
        Assert.Equal(source.CanonicalHash, report.RootElement.GetProperty("source_snapshot_hash").GetString());
        Assert.Equal(plan.PlanHash, report.RootElement.GetProperty("style_plan_hash").GetString());

        Ra2VoxelReviewArtifact region = package.Artifacts.Single(item => item.FileName == "region-mask.png");
        (int width, int height, byte[] _) = Ra2PngRgbaCodec.Decode(region.Content.Span);
        Assert.True(width > 0);
        Assert.True(height > 0);
        using MemoryStream voxStream = new(package.Artifacts.Single(item => item.FileName == "body-coloured.vox").Content.ToArray());
        Ra2VoxelSceneSnapshot decoded = Ra2MagicaVoxelCodec.Read(
            voxStream, "REVIEW_ROUNDTRIP", source.Part.PartId, source.Part.Role,
            source.Part.VxlSectionName, source.Part.StableFileStem);
        Assert.Equal(colourization.Snapshot!.Cells, decoded.Cells);
    }

    [Fact]
    public void ReviewPackage_RejectsFailedResultsInvalidFactsAndForeignMasks()
    {
        Ra2VoxelSceneSnapshot source = CreateSolidCube();
        Ra2CompiledVoxelStylePlan plan = CompilePlan(source.Palette);
        Ra2VoxelColourizationResult success = Ra2VoxelColourizer.Colourize(source, plan);
        Ra2VoxelColourizationResult failed = new(
            Ra2VoxelColourizationFailureKind.AnalysisFailed, "failed", null, null, null);

        Assert.Equal(Ra2VoxelColourReviewPackageFailureKind.InvalidColourizationResult,
            Ra2VoxelColourReviewPackageBuilder.Build(
                [new("built-in", new string('A', 64), 1)], source, plan, failed).FailureKind);
        Assert.Equal(Ra2VoxelColourReviewPackageFailureKind.ResourceLimitExceeded,
            Ra2VoxelColourReviewPackageBuilder.Build(
                [new("built-in", "invalid", 1)], source, plan, success).FailureKind);

        Ra2VoxelExplicitMask foreign = new("foreign", new string('B', 64), new byte[source.OccupancyCount]);
        Assert.Equal(Ra2VoxelColourReviewPackageFailureKind.MaskMismatch,
            Ra2VoxelColourReviewPackageBuilder.Build(
                [new("built-in", new string('A', 64), 1)], source, plan, success, [foreign]).FailureKind);
    }

    [Fact]
    public void ExistingBodyCandidate_WhenExplicitlyEnabled_ProducesDeterministicReviewPackage()
    {
        string? sourceVoxPath = Environment.GetEnvironmentVariable("RA2INI_VOX_1E_SOURCE_VOX");
        string? reportDirectory = Environment.GetEnvironmentVariable("RA2INI_VOX_1E_REPORT_DIRECTORY");
        if (string.IsNullOrWhiteSpace(sourceVoxPath) || string.IsNullOrWhiteSpace(reportDirectory))
            return;

        using FileStream sourceStream = File.OpenRead(sourceVoxPath);
        Ra2VoxelSceneSnapshot source = Ra2MagicaVoxelCodec.Read(
            sourceStream, "P2_BODY_STYLE_ACCEPTANCE", "body", Ra2VoxelAssemblyPartRole.Body, "Body", "p2body-coloured");
        const string naturalLanguageStyle =
            "冷战盟军装甲车：低饱和橄榄绿；顶部略亮，侧面中间调，底盘和朝下表面更暗；" +
            "边缘只做克制的暖色提亮。无法可靠识别玻璃时不要猜测，只保留待复核提示；不使用阵营重映射。";
        string sourceHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(naturalLanguageStyle)));
        Ra2CompiledVoxelStylePlan plan = CompileAcceptancePlan(source.Palette, sourceHash);

        Ra2VoxelColourizationResult first = Ra2VoxelColourizer.Colourize(source, plan);
        Ra2VoxelColourizationResult second = Ra2VoxelColourizer.Colourize(source, plan);
        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Assert.NotEqual(source.CanonicalHash, first.Snapshot!.CanonicalHash);
        Assert.Equal(first.Snapshot.CanonicalHash, second.Snapshot!.CanonicalHash);
        Assert.True(first.Facts!.GeometryAndOccupancyUnchanged);
        Assert.True(first.Facts.ReviewFlags.HasFlag(Ra2VoxelColourReviewFlags.SemanticMaskReviewRequired));

        Ra2VoxelStyleSourceFact[] sources = [new("acceptance-natural-language", sourceHash, naturalLanguageStyle.Length)];
        Ra2VoxelColourReviewPackageResult firstPackage =
            Ra2VoxelColourReviewPackageBuilder.Build(sources, source, plan, first);
        Ra2VoxelColourReviewPackageResult secondPackage =
            Ra2VoxelColourReviewPackageBuilder.Build(sources, source, plan, second);
        Assert.True(firstPackage.IsSuccess, firstPackage.Message);
        Assert.Equal(
            firstPackage.Artifacts.Select(item => (item.FileName, item.ContentSha256)),
            secondPackage.Artifacts.Select(item => (item.FileName, item.ContentSha256)));

        Directory.CreateDirectory(reportDirectory);
        foreach (Ra2VoxelReviewArtifact artifact in firstPackage.Artifacts)
            File.WriteAllBytes(Path.Combine(reportDirectory, artifact.FileName), artifact.Content.ToArray());
    }

    private static Ra2CompiledVoxelStylePlan CompilePlan(
        Ra2VoxelPaletteProfile palette,
        bool includeTextOnlyGlass = false,
        bool includeRemap = false)
    {
        List<Ra2VoxelStyleRoleDefinition> roles =
        [
            Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, 60),
            Role("body.light", Ra2VoxelStyleRoleCategory.BodyLight, 80),
            Role("body.mid", Ra2VoxelStyleRoleCategory.BodyMid, 70),
            Role("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, 50),
            Role("underside", Ra2VoxelStyleRoleCategory.Underside, 40),
            Role("edge", Ra2VoxelStyleRoleCategory.BodyLight, 90)
        ];
        List<Ra2VoxelStyleRuleDefinition> rules =
        [
            Rule(Ra2VoxelStyleRegionKind.WholePart, "body.base"),
            Rule(Ra2VoxelStyleRegionKind.SideExposed, "body.mid"),
            Rule(Ra2VoxelStyleRegionKind.TopExposed, "body.light"),
            Rule(Ra2VoxelStyleRegionKind.UnderExposed, "underside"),
            Rule(Ra2VoxelStyleRegionKind.EdgeOrRidge, "edge"),
            Rule(Ra2VoxelStyleRegionKind.Interior, "body.dark")
        ];
        if (includeTextOnlyGlass)
        {
            roles.Add(Role("glass", Ra2VoxelStyleRoleCategory.Glass, 100));
            rules.Add(new(Ra2VoxelStyleRegionKind.ExplicitMask, "glass", Ra2VoxelStyleEvidenceKind.InferredTextOnly, "guessed", ["built-in"]));
        }
        if (includeRemap)
        {
            roles.Add(Role("team", Ra2VoxelStyleRoleCategory.Remap, 16));
            rules.Add(new(Ra2VoxelStyleRegionKind.ExplicitMask, "team", Ra2VoxelStyleEvidenceKind.ExplicitUserMask, "team-mask", ["built-in"]));
        }
        Ra2VoxelStylePlanDefinition definition = new(
            "Colour test", "Fixed geometry shade order", new string('A', 64), palette.ProfileHash,
            "compiler/1", "fixture/1", includeRemap ? Ra2VoxelStyleRemapPolicy.ExplicitMask : Ra2VoxelStyleRemapPolicy.None,
            "body.dark", roles, rules, includeTextOnlyGlass ? ["Glass requires a mask."] : []);
        Ra2VoxelStylePlanCompilationResult result = Ra2VoxelStylePlanCompiler.Compile(definition, palette, ["built-in"]);
        Assert.True(result.IsSuccess, result.Message);
        return result.Plan!;
    }

    private static Ra2CompiledVoxelStylePlan CompileAcceptancePlan(Ra2VoxelPaletteProfile palette, string sourceHash)
    {
        const string scope = "acceptance-natural-language";
        List<Ra2VoxelStyleRoleDefinition> roles =
        [
            ColourRole("body.base", Ra2VoxelStyleRoleCategory.BodyBase, new(78, 86, 58), scope),
            ColourRole("body.light", Ra2VoxelStyleRoleCategory.BodyLight, new(114, 124, 82), scope),
            ColourRole("body.mid", Ra2VoxelStyleRoleCategory.BodyMid, new(84, 92, 62), scope),
            ColourRole("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, new(54, 60, 42), scope),
            ColourRole("underside", Ra2VoxelStyleRoleCategory.Underside, new(40, 44, 32), scope),
            ColourRole("edge", Ra2VoxelStyleRoleCategory.BodyLight, new(142, 146, 96), scope),
            ColourRole("glass.unresolved", Ra2VoxelStyleRoleCategory.Glass, new(72, 104, 120), scope)
        ];
        List<Ra2VoxelStyleRuleDefinition> rules =
        [
            GeometryRule(Ra2VoxelStyleRegionKind.WholePart, "body.base", scope),
            GeometryRule(Ra2VoxelStyleRegionKind.SideExposed, "body.mid", scope),
            GeometryRule(Ra2VoxelStyleRegionKind.TopExposed, "body.light", scope),
            GeometryRule(Ra2VoxelStyleRegionKind.UnderExposed, "underside", scope),
            GeometryRule(Ra2VoxelStyleRegionKind.EdgeOrRidge, "edge", scope),
            GeometryRule(Ra2VoxelStyleRegionKind.Interior, "body.dark", scope),
            new(Ra2VoxelStyleRegionKind.ExplicitMask, "glass.unresolved",
                Ra2VoxelStyleEvidenceKind.InferredTextOnly, "glass-unresolved", [scope])
        ];
        Ra2VoxelStylePlanCompilationResult result = Ra2VoxelStylePlanCompiler.Compile(
            new("冷战盟军装甲车", "低饱和橄榄绿的确定性几何明暗；玻璃保持未解析。", sourceHash,
                palette.ProfileHash, "asset-vox-1e/1", "captured-fixture/no-live-call",
                Ra2VoxelStyleRemapPolicy.None, "body.dark", roles, rules,
                ["玻璃缺少显式蒙版，未进行语义上色。"]),
            palette,
            [scope]);
        Assert.True(result.IsSuccess, result.Message);
        return result.Plan!;

        static Ra2VoxelStyleRoleDefinition ColourRole(
            string id, Ra2VoxelStyleRoleCategory category, Ra2Rgba32 colour, string scopeId) =>
            new(id, category, null, colour, [scopeId]);
        static Ra2VoxelStyleRuleDefinition GeometryRule(
            Ra2VoxelStyleRegionKind region, string roleId, string scopeId) =>
            new(region, roleId, Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, [scopeId]);
    }

    private static Ra2VoxelStyleRoleDefinition Role(string id, Ra2VoxelStyleRoleCategory category, byte index)
        => new(id, category, index, null, ["built-in"]);

    private static Ra2VoxelStyleRuleDefinition Rule(Ra2VoxelStyleRegionKind region, string role)
        => new(region, role, Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["built-in"]);

    private static Ra2VoxelSceneSnapshot CreateSolidCube()
        => CreateSolidCuboid(3, 3, 3);

    private static Ra2VoxelSceneSnapshot CreateSolidCuboid(int xSize, int ySize, int zSize)
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "colour", xSize, ySize, zSize);
        List<Ra2VoxelCell> cells = [];
        for (int z = 0; z < zSize; z++)
        for (int y = 0; y < ySize; y++)
        for (int x = 0; x < xSize; x++)
            cells.Add(new(new Ra2VoxelCoordinate(x, y, z), 60));
        return new("COLOUR_SOURCE", part, palette, cells, [new("mesh.glb", new string('B', 64))]);
    }

    private static Ra2VoxelPaletteProfile CreatePalette(string id = "colour-test")
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        return new(id, colours, [0], Enumerable.Range(16, 16).Select(value => (byte)value));
    }

    private static void AssertIndex(Ra2VoxelSceneSnapshot snapshot, int x, int y, int z, byte expected)
    {
        Assert.True(snapshot.TryGetPaletteIndex(new Ra2VoxelCoordinate(x, y, z), out byte actual));
        Assert.Equal(expected, actual);
    }
}
