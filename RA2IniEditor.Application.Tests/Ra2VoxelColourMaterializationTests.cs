using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using System.Text.Json;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelColourMaterializationTests
{
    public static IEnumerable<object[]> TechniqueAndClassCases()
    {
        foreach (Ra2VoxelColourTechniquePolicy technique in Ra2VoxelColourTechniqueCatalog.All)
        foreach (Ra2VoxelUnitClass unitClass in Enum.GetValues<Ra2VoxelUnitClass>())
            yield return [technique.TechniqueId, (int)unitClass];
    }

    [Fact]
    public void Materializer_IsDeterministicAndKeepsHumanBodyBaseExact()
    {
        Fixture fixture = CreateFixture(Ra2VoxelUnitClass.Ground);

        Ra2VoxelColourMaterializationResult first = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);
        Ra2VoxelColourMaterializationResult second = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);

        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Assert.Equal(first.Ordinary!.BundleHash, second.Ordinary!.BundleHash);
        Assert.Equal(first.Ordinary.Colourization.Snapshot!.CanonicalHash,
            second.Ordinary.Colourization.Snapshot!.CanonicalHash);
        Assert.Equal(Ra2VoxelColourAdmissionState.ReviewReady, first.Ordinary.Quality.State);
        Assert.Equal(Ra2VoxelColourVisualAcceptance.Pending, first.Ordinary.Quality.VisualAcceptance);
        Ra2CompiledVoxelStyleRole bodyBase = first.Ordinary.Plan.Roles.Single(value =>
            value.Category == Ra2VoxelStyleRoleCategory.BodyBase);
        Assert.Equal(fixture.Context.BaseColour.PaletteIndex, bodyBase.PaletteIndex);
        Assert.Equal(fixture.Context.BaseColour.PaletteIndex, bodyBase.RequestedExactPaletteIndex);
        Assert.Equal(fixture.Context.Source.Cells.Select(value => value.Coordinate),
            first.Ordinary.Colourization.Snapshot.Cells.Select(value => value.Coordinate));
    }

    [Fact]
    public void ReviewPackage_BindsMultidimensionalQualityReportToCandidate()
    {
        Fixture fixture = CreateFixture(Ra2VoxelUnitClass.Ground);
        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);
        Assert.True(result.IsSuccess, result.Message);

        Ra2VoxelColourReviewPackageResult review = Ra2VoxelColourReviewPackageBuilder.Build(
            [new("built-in", new string('D', 64), 128)],
            fixture.Context.Source,
            result.Ordinary!.Plan,
            result.Ordinary.Colourization,
            result.SemanticIntegration!.Masks,
            result.Ordinary.Quality);

        Assert.True(review.IsSuccess, review.Message);
        using JsonDocument document = JsonDocument.Parse(review.Artifacts.Single(value =>
            value.FileName == "colour-review-report.json").Content);
        JsonElement quality = document.RootElement.GetProperty("quality");
        Assert.Equal(result.Ordinary.Quality.ReportHash, quality.GetProperty("report_hash").GetString());
        Assert.Equal("ReviewReady", quality.GetProperty("state").GetString());
        Assert.Equal("Pending", quality.GetProperty("visual_acceptance").GetString());
        Assert.False(quality.TryGetProperty("score", out _));
    }

    [Theory]
    [MemberData(nameof(TechniqueAndClassCases))]
    public void Materializer_CoversFiveTechniquesAcrossFourUnitAdaptations(
        string techniqueId,
        int unitClassValue)
    {
        Ra2VoxelUnitClass unitClass = (Ra2VoxelUnitClass)unitClassValue;
        Fixture fixture = CreateFixture(unitClass, techniqueId);

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);

        Assert.True(result.IsSuccess, result.Message);
        Assert.NotEqual(Ra2VoxelColourAdmissionState.Blocked, result.Ordinary!.Quality.State);
        Assert.Equal(unitClass == Ra2VoxelUnitClass.Unknown
                ? Ra2VoxelColourAdmissionState.NeedsReview
                : Ra2VoxelColourAdmissionState.ReviewReady,
            result.Ordinary.Quality.State);
        Assert.Equal(fixture.Context.BaseColour.PaletteIndex,
            result.FamilySelection![Ra2VoxelBodyColourRole.BodyBase].PaletteIndex);
    }

    [Theory]
    [InlineData((int)Ra2VoxelUnitClass.Ground, (int)Ra2VoxelStyleRoleCategory.Underside)]
    [InlineData((int)Ra2VoxelUnitClass.Air, (int)Ra2VoxelStyleRoleCategory.BodyBase)]
    [InlineData((int)Ra2VoxelUnitClass.LargeSurface, (int)Ra2VoxelStyleRoleCategory.BodyLight)]
    [InlineData((int)Ra2VoxelUnitClass.Unknown, (int)Ra2VoxelStyleRoleCategory.BodyBase)]
    public void Materializer_UsesExplicitDualSurfacePolicy(
        int unitClassValue,
        int expectedCategoryValue)
    {
        Ra2VoxelUnitClass unitClass = (Ra2VoxelUnitClass)unitClassValue;
        Ra2VoxelStyleRoleCategory expectedCategory = (Ra2VoxelStyleRoleCategory)expectedCategoryValue;
        Fixture fixture = CreateFixture(unitClass);
        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);
        Assert.True(result.IsSuccess, result.Message);

        int centre = fixture.Context.Source.Cells.ToList().FindIndex(value => value.Coordinate == new Ra2VoxelCoordinate(1, 1, 0));
        string roleId = result.Ordinary!.Colourization.Facts!.AppliedRoleIds[centre];
        Assert.Equal(expectedCategory, result.Ordinary.Plan.Roles.Single(value => value.Id == roleId).Category);
    }

    [Fact]
    public void Materializer_PreservesPaintedGeometryAndAppliesDirectThenRemapPrecedence()
    {
        Fixture fixture = CreateFixture(
            Ra2VoxelUnitClass.Ground,
            assignments: (source, original) => original.Select((value, index) => index switch
            {
                0 => Assignment(Ra2VoxelSemanticMaterialRole.Glass),
                1 => Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface,
                    Ra2VoxelSemanticRemapIntent.ExplicitlyApproved),
                _ => value
            }).ToArray());

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);

        Assert.True(result.IsSuccess, result.Message);
        Ra2VoxelSceneSnapshot candidate = result.Ordinary!.Colourization.Snapshot!;
        Assert.Equal((byte)150, candidate.Cells[0].PaletteIndex);
        Assert.Equal((byte)16, candidate.Cells[1].PaletteIndex);
        Assert.NotEqual(fixture.Context.BaseColour.PaletteIndex,
            candidate.Cells.Single(value => value.Coordinate == new Ra2VoxelCoordinate(1, 1, 0)).PaletteIndex);
        Assert.DoesNotContain(result.SemanticIntegration!.Plan.Rules, rule =>
            rule.Region == Ra2VoxelStyleRegionKind.ExplicitMask &&
            string.Equals(rule.RoleId, "body.base", StringComparison.Ordinal));
    }

    [Fact]
    public void Materializer_ReportsFlatDirectMaterialAcrossPrimaryRegions()
    {
        Fixture fixture = CreateFixture(
            Ra2VoxelUnitClass.Ground,
            assignments: (_, original) => original.Select((value, index) =>
                index is 0 or 4 ? Assignment(Ra2VoxelSemanticMaterialRole.Glass) : value).ToArray());

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(Ra2VoxelColourAdmissionState.NeedsReview, result.Ordinary!.Quality.State);
        Assert.Contains(result.Ordinary.Quality.Warnings,
            value => value.Code == "FlatSemanticMaterialAcrossRegions");
    }

    [Fact]
    public void QualityEvaluator_UsesBlockedStateForHardIdentityViolation()
    {
        Fixture fixture = CreateFixture(Ra2VoxelUnitClass.Ground);
        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);
        Assert.True(result.IsSuccess, result.Message);
        Ra2VoxelPaletteProfile foreignPalette = new(
            "foreign-colour-4e",
            fixture.Context.Source.Palette.Colours,
            fixture.Context.Source.Palette.TransparentIndices,
            fixture.Context.Source.Palette.RemapIndices);
        Ra2VoxelBaseColourSelection foreignBase = Assert.IsType<Ra2VoxelBaseColourSelection>(
            Ra2VoxelBaseColourSelection.Create(foreignPalette, foreignPalette.ProfileHash, 100).Selection);

        Ra2VoxelColourQualityReport blocked = Ra2VoxelColourQualityEvaluator.Evaluate(
            fixture.Context.Source,
            result.Ordinary!.Plan,
            result.Ordinary.Colourization,
            fixture.Context.Composition,
            fixture.Context.Requirements,
            fixture.Context.BindingPlan,
            foreignBase,
            fixture.Context.Technique,
            fixture.Context.Adaptation,
            result.FamilySelection!,
            fixture.Context.Evidence,
            fixture.Context.Confirmation,
            fixture.Context.ColourSkill,
            result.Ordinary.BundleHash);

        Assert.Equal(Ra2VoxelColourAdmissionState.Blocked, blocked.State);
        Assert.Contains(blocked.Warnings, value => value.Code == "IdentityMismatch");
    }

    [Fact]
    public void PolicyAwareContrast_PreservesBaseDirectSemanticAndRemapExactSelections()
    {
        Fixture fixture = CreateFixture(
            Ra2VoxelUnitClass.Ground,
            assignments: (_, original) => original.Select((value, index) => index switch
            {
                0 => Assignment(Ra2VoxelSemanticMaterialRole.Glass),
                1 => Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface,
                    Ra2VoxelSemanticRemapIntent.ExplicitlyApproved),
                _ => value
            }).ToArray());
        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);
        Assert.True(result.IsSuccess, result.Message);

        if (result.Contrast is null)
            return;
        foreach (string roleId in new[] { "body.base", "glass", "team" })
        {
            Assert.Equal(
                result.Ordinary!.Plan.Roles.Single(value => value.Id == roleId).PaletteIndex,
                result.Contrast.Plan.Roles.Single(value => value.Id == roleId).PaletteIndex);
        }
        Assert.Equal(fixture.Context.BaseColour.PaletteIndex,
            result.Contrast.Plan.Roles.Single(value => value.Id == "body.base").PaletteIndex);
        Assert.True(result.Contrast.ContrastFacts!.ExactPaletteSelectionsPreserved);
    }

    [Fact]
    public void FamilySelector_SparsePaletteWarnsOrBlocksAccordingToTechniquePolicy()
    {
        Ra2VoxelPaletteProfile palette = CreateSparsePalette();
        Ra2VoxelBaseColourSelection baseColour = Assert.IsType<Ra2VoxelBaseColourSelection>(
            Ra2VoxelBaseColourSelection.Create(palette, palette.ProfileHash, 100).Selection);

        Ra2VoxelColourFamilyResult warning = Ra2VoxelColourFamilySelector.Select(
            palette,
            baseColour,
            Ra2VoxelColourTechniqueCatalog.Default,
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground));
        Assert.True(warning.IsSuccess, warning.Message);
        Assert.NotEmpty(warning.Selection!.Warnings);

        Ra2VoxelColourTechniquePolicy blocking = Assert.IsType<Ra2VoxelColourTechniquePolicy>(
            Ra2VoxelColourTechniqueCatalog.Find("semantic-material-separation"));
        Ra2VoxelColourFamilyResult blocked = Ra2VoxelColourFamilySelector.Select(
            palette,
            baseColour,
            blocking,
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground));
        Assert.Equal(Ra2VoxelColourFamilyFailureKind.PaletteFamilyUnavailable, blocked.FailureKind);
    }

    [Fact]
    public void Materializer_FailsClosedForStaleBindingAndWrongColourSkill()
    {
        Fixture fixture = CreateFixture(Ra2VoxelUnitClass.Ground);
        Ra2VoxelColourMaterializationContext wrongSkill = fixture.Context with
        {
            ColourSkill = new("ra2-air-voxel-colour-techniques", "1", new string('B', 64))
        };
        Assert.Equal(Ra2VoxelColourMaterializationFailureKind.IdentityMismatch,
            Ra2VoxelSemanticColourMaterializer.Materialize(wrongSkill).FailureKind);

        Ra2VoxelSemanticColourBindingPlan foreignBinding = CreateFixture(
            Ra2VoxelUnitClass.Air,
            assignments: (_, original) => original.Select((value, index) =>
                index == 0 ? Assignment(Ra2VoxelSemanticMaterialRole.Glass) : value).ToArray()).Context.BindingPlan;
        Ra2VoxelColourMaterializationContext stale = fixture.Context with { BindingPlan = foreignBinding };
        Assert.Equal(Ra2VoxelColourMaterializationFailureKind.IdentityMismatch,
            Ra2VoxelSemanticColourMaterializer.Materialize(stale).FailureKind);
    }

    [Theory]
    [InlineData(32)]
    [InlineData(250)]
    public void Materializer_ExtremeHumanAnchorsStayExactAndExposeFamilyFallback(int baseIndex)
    {
        Fixture fixture = CreateFixture(Ra2VoxelUnitClass.Ground, baseIndex: checked((byte)baseIndex));

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal((byte)baseIndex, result.FamilySelection![Ra2VoxelBodyColourRole.BodyBase].PaletteIndex);
        Assert.Equal(Ra2VoxelColourAdmissionState.NeedsReview, result.Ordinary!.Quality.State);
        Assert.Contains(result.Ordinary.Quality.Warnings, value => value.Code == "PaletteFamilyFallback");
    }

    [Fact]
    public void Materializer_HumanManualSelectionDoesNotInventClassifierWarning()
    {
        Fixture fixture = CreateFixture(Ra2VoxelUnitClass.Ground);
        Ra2VoxelUnitClassConfirmationResult manual = Ra2VoxelConfirmedUnitClass.Create(
            fixture.Context.Evidence,
            Ra2VoxelUnitClass.Ground,
            Ra2VoxelUnitClassConfirmationSource.HumanManualSelection,
            null);
        Assert.True(manual.IsSuccess, manual.Message);
        Ra2VoxelColourMaterializationContext context = fixture.Context with
        {
            Confirmation = manual.Confirmation!
        };

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(context);

        Assert.True(result.IsSuccess, result.Message);
        Assert.NotEqual(Ra2VoxelColourAdmissionState.Blocked, result.Ordinary!.Quality.State);
        Assert.DoesNotContain(result.Ordinary.Quality.Warnings, value => value.Code == "UnitClassReviewRequired");
    }

    private static Fixture CreateFixture(
        Ra2VoxelUnitClass unitClass,
        string techniqueId = "balanced-rts-volume",
        Func<Ra2VoxelSceneSnapshot, Ra2VoxelSemanticEffectiveAssignment[], Ra2VoxelSemanticEffectiveAssignment[]>? assignments = null,
        byte baseIndex = 100)
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelSceneSnapshot source = CreateSheet(palette);
        Ra2VoxelSemanticEffectiveAssignment[] values = Enumerable.Range(0, source.OccupancyCount)
            .Select(_ => Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface))
            .ToArray();
        if (assignments is not null) values = assignments(source, values);
        Ra2VoxelSemanticMaskComposition composition = new(source.CanonicalHash, values, new string('C', 64));
        Ra2VoxelSemanticColourRequirements requirements = Ra2VoxelSemanticColourRequirementsProjector.Project(composition);
        Ra2CompiledVoxelStylePlan rawPlan = CompileRawPlan(palette, requirements.ApprovedRemapCellCount > 0);
        Ra2VoxelSemanticColourBindingPlan bindingPlan = CreateBindingPlan(requirements, rawPlan);
        Ra2VoxelUnitClassEvidence evidence = Ra2VoxelUnitClassEvidenceBuilder.Build(source, composition);
        Ra2VoxelUnitClassConfirmationResult confirmationResult = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            unitClass,
            Ra2VoxelUnitClassConfirmationSource.HumanManualSelection,
            null);
        Assert.True(confirmationResult.IsSuccess, confirmationResult.Message);
        Ra2VoxelBaseColourSelectionResult baseResult = Ra2VoxelBaseColourSelection.Create(
            palette, palette.ProfileHash, baseIndex);
        Assert.True(baseResult.IsSuccess, baseResult.Message);
        Ra2VoxelColourTechniquePolicy technique = Assert.IsType<Ra2VoxelColourTechniquePolicy>(
            Ra2VoxelColourTechniqueCatalog.Find(techniqueId));
        Ra2VoxelUnitAdaptationPolicy adaptation = Ra2VoxelUnitAdaptationCatalog.For(unitClass);
        Ra2VoxelColourMaterializationContext context = new(
            source,
            rawPlan,
            composition,
            requirements,
            bindingPlan,
            evidence,
            confirmationResult.Confirmation!,
            new(adaptation.ColouringSkillId, "1", new string('B', 64)),
            baseResult.Selection!,
            technique,
            adaptation);
        return new(context);
    }

    private static Ra2VoxelSemanticColourBindingPlan CreateBindingPlan(
        Ra2VoxelSemanticColourRequirements requirements,
        Ra2CompiledVoxelStylePlan plan)
    {
        List<Ra2VoxelSemanticColourBinding> bindings = [];
        foreach (Ra2VoxelSemanticColourRequirement requirement in requirements.Required)
        {
            bindings.Add(requirement.Kind switch
            {
                Ra2VoxelSemanticColourRequirementKind.PaintedSurface => new(requirement.Kind,
                    Ra2VoxelSemanticColourBindingMode.BodyGeometryFamily, "body.base"),
                Ra2VoxelSemanticColourRequirementKind.Glass => Direct(requirement.Kind, "glass"),
                Ra2VoxelSemanticColourRequirementKind.Rubber => Direct(requirement.Kind, "rubber"),
                Ra2VoxelSemanticColourRequirementKind.BareMetal => Direct(requirement.Kind, "metal"),
                Ra2VoxelSemanticColourRequirementKind.Light => Direct(requirement.Kind, "light"),
                Ra2VoxelSemanticColourRequirementKind.DarkOpening => Direct(requirement.Kind, "body.dark"),
                Ra2VoxelSemanticColourRequirementKind.Accent => Direct(requirement.Kind, "accent"),
                Ra2VoxelSemanticColourRequirementKind.ApprovedRemap => Direct(requirement.Kind, "team"),
                _ => throw new ArgumentOutOfRangeException()
            });
        }
        Ra2VoxelSemanticColourBindingResult result = Ra2VoxelSemanticColourBindingPlan.Validate(requirements, plan, bindings);
        Assert.True(result.IsSuccess, result.Message);
        return result.Plan!;

        static Ra2VoxelSemanticColourBinding Direct(Ra2VoxelSemanticColourRequirementKind kind, string roleId)
            => new(kind, Ra2VoxelSemanticColourBindingMode.DirectRole, roleId);
    }

    private static Ra2CompiledVoxelStylePlan CompileRawPlan(Ra2VoxelPaletteProfile palette, bool remap)
    {
        List<Ra2VoxelStyleRoleDefinition> roles =
        [
            Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, 90),
            Role("body.light", Ra2VoxelStyleRoleCategory.BodyLight, 105),
            Role("body.mid", Ra2VoxelStyleRoleCategory.BodyMid, 85),
            Role("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, 60),
            Role("underside", Ra2VoxelStyleRoleCategory.Underside, 40),
            Role("edge", Ra2VoxelStyleRoleCategory.BodyLight, 115),
            Role("glass", Ra2VoxelStyleRoleCategory.Glass, 150),
            Role("rubber", Ra2VoxelStyleRoleCategory.Rubber, 45),
            Role("metal", Ra2VoxelStyleRoleCategory.BareMetal, 180),
            Role("light", Ra2VoxelStyleRoleCategory.Accent, 230),
            Role("accent", Ra2VoxelStyleRoleCategory.Accent, 200)
        ];
        if (remap) roles.Add(Role("team", Ra2VoxelStyleRoleCategory.Remap, 16));
        List<Ra2VoxelStyleRuleDefinition> rules =
        [
            Rule(Ra2VoxelStyleRegionKind.WholePart, "body.base"),
            Rule(Ra2VoxelStyleRegionKind.Interior, "body.dark"),
            Rule(Ra2VoxelStyleRegionKind.SideExposed, "body.mid"),
            Rule(Ra2VoxelStyleRegionKind.TopExposed, "body.light"),
            Rule(Ra2VoxelStyleRegionKind.UnderExposed, "underside"),
            Rule(Ra2VoxelStyleRegionKind.EdgeOrRidge, "edge")
        ];
        Ra2VoxelStylePlanCompilationResult result = Ra2VoxelStylePlanCompiler.Compile(
            new("4E fixture", "Raw class-specific style proposal", new string('D', 64), palette.ProfileHash,
                "compiler/2", "fake-provider", remap ? Ra2VoxelStyleRemapPolicy.ExplicitMask : Ra2VoxelStyleRemapPolicy.None,
                "body.dark", roles, rules),
            palette,
            ["built-in"]);
        Assert.True(result.IsSuccess, result.Message);
        return result.Plan!;
    }

    private static Ra2VoxelSceneSnapshot CreateSheet(Ra2VoxelPaletteProfile palette)
    {
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "colour-4e", 3, 3, 1);
        List<Ra2VoxelCell> cells = [];
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            cells.Add(new(new Ra2VoxelCoordinate(x, y, 0), 90));
        return new("COLOUR_4E", part, palette, cells, [new("fixture", new string('E', 64))]);
    }

    private static Ra2VoxelPaletteProfile CreatePalette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        return new("colour-4e", colours, [0], Enumerable.Range(16, 16).Select(value => (byte)value));
    }

    private static Ra2VoxelPaletteProfile CreateSparsePalette()
    {
        Ra2Rgba32[] colours = Enumerable.Repeat(new Ra2Rgba32(255, 0, 255, 0), 256).ToArray();
        colours[100] = new(100, 100, 100);
        byte[] transparent = Enumerable.Range(0, 256).Where(value => value != 100).Select(value => (byte)value).ToArray();
        return new("sparse-4e", colours, transparent);
    }

    private static Ra2VoxelStyleRoleDefinition Role(string id, Ra2VoxelStyleRoleCategory category, byte index)
        => new(id, category, index, null, ["built-in"]);

    private static Ra2VoxelStyleRuleDefinition Rule(Ra2VoxelStyleRegionKind region, string roleId)
        => new(region, roleId, Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["built-in"]);

    private static Ra2VoxelSemanticEffectiveAssignment Assignment(
        Ra2VoxelSemanticMaterialRole material,
        Ra2VoxelSemanticRemapIntent remap = Ra2VoxelSemanticRemapIntent.None)
        => new("fixture", Ra2VoxelSemanticPartRole.BodyShell, material, remap,
            Ra2VoxelSemanticAssignmentSource.HumanOverride, 1d, "fixture");

    private sealed record Fixture(Ra2VoxelColourMaterializationContext Context);
}
