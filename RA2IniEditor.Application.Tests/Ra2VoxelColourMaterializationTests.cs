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
        Assert.Equal(Ra2VoxelColourAdmissionState.NeedsReview, first.Ordinary.Quality.State);
        Assert.Equal(Ra2VoxelColourVisualAcceptance.Pending, first.Ordinary.Quality.VisualAcceptance);
        Assert.Contains(first.Ordinary.Quality.Warnings, value => value.Code == "VplNotEvaluated");
        Assert.Contains(first.Ordinary.Quality.Warnings, value => value.Code == "NormalContextNotAvailable");
        Assert.Equal(8, first.Ordinary.GameScale!.Views.Count);
        Assert.Equal(first.Ordinary.GameScale.FactsHash, second.Ordinary.GameScale!.FactsHash);
        Ra2CompiledVoxelStyleRole bodyBase = first.Ordinary.Plan.Roles.Single(value =>
            value.Category == Ra2VoxelStyleRoleCategory.BodyBase);
        Assert.Equal(fixture.Context.BaseColour.PaletteIndex, bodyBase.PaletteIndex);
        Assert.Equal(fixture.Context.BaseColour.PaletteIndex, bodyBase.RequestedExactPaletteIndex);
        Assert.Equal(bodyBase.Id, first.Ordinary.Plan.Rules.Single(value =>
            value.Region == Ra2VoxelStyleRegionKind.SideExposed).RoleId);
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
        Assert.Equal("NeedsReview", quality.GetProperty("state").GetString());
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
        Assert.Equal(Ra2VoxelColourAdmissionState.NeedsReview, result.Ordinary.Quality.State);
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
            null,
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
    public void FamilySelector_RealRa2StyleRampDoesNotJumpToNeighbouringBrownRamps()
    {
        Ra2VoxelPaletteProfile palette = CreateRa2IndexedRampPalette();
        Ra2VoxelBaseColourSelection baseColour = Assert.IsType<Ra2VoxelBaseColourSelection>(
            Ra2VoxelBaseColourSelection.Create(palette, palette.ProfileHash, 72).Selection);

        Ra2VoxelColourFamilyResult result = Ra2VoxelColourFamilySelector.Select(
            palette,
            baseColour,
            Assert.IsType<Ra2VoxelColourTechniquePolicy>(Ra2VoxelColourTechniqueCatalog.Find("subtle-matte-shading")),
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground));

        Assert.True(result.IsSuccess, result.Message);
        Assert.All(result.Selection!.Roles, role => Assert.InRange(role.PaletteIndex, (byte)64, (byte)79));
        Assert.Equal((byte)72, result.Selection[Ra2VoxelBodyColourRole.BodyBase].PaletteIndex);
        Assert.DoesNotContain(result.Selection.Roles, role => role.PaletteIndex is 112 or 138 or 152 or 154);
        Assert.DoesNotContain("IndexedPaletteRampUnavailable", result.Selection.Warnings);
    }

    [Fact]
    public void Materializer_FiveTechniquesProduceFiveDistinctVoxelResults()
    {
        string[] resultHashes = Ra2VoxelColourTechniqueCatalog.All.Select(technique =>
        {
            Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(
                CreateFixture(
                    Ra2VoxelUnitClass.Ground,
                    technique.TechniqueId,
                    baseIndex: 72,
                    paletteFactory: CreateMaterializationRa2Palette).Context);
            Assert.True(result.IsSuccess, $"{technique.TechniqueId}: {result.Message}");
            return result.Ordinary!.Colourization.Snapshot!.CanonicalHash;
        }).ToArray();

        Assert.Equal(5, resultHashes.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Materializer_FiveTechniquesProduceFiveSpatialRoleDistributions()
    {
        string[] signatures = Ra2VoxelColourTechniqueCatalog.All.Select(technique =>
        {
            Fixture fixture = CreateFixture(
                Ra2VoxelUnitClass.Ground,
                technique.TechniqueId,
                sourceFactory: palette => CreateVolume(palette, 4, 6, 4));
            Ra2VoxelColourMaterializationResult result =
                Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);
            Assert.True(result.IsSuccess, $"{technique.TechniqueId}: {result.Message}");
            return string.Join('|', result.Ordinary!.Colourization.Facts!.RoleCounts
                .Where(value => value.Id.StartsWith("body.", StringComparison.Ordinal))
                .OrderBy(value => value.Id, StringComparer.Ordinal)
                .Select(value => $"{value.Id}:{value.CellCount}"));
        }).ToArray();

        Assert.Equal(5, signatures.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Materializer_GroundVolumeUsesLowerBandWithoutUndersideLeakAndKeepsEndsReadable()
    {
        Fixture fixture = CreateFixture(
            Ra2VoxelUnitClass.Ground,
            sourceFactory: palette => CreateVolume(palette, 3, 5, 3));

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);

        Assert.True(result.IsSuccess, result.Message);
        Ra2VoxelColourizationFacts facts = result.Ordinary!.Colourization.Facts!;
        AssertRole(new(0, 2, 0), "body.lower.v3");
        AssertRole(new(1, 0, 1), "body.mid");
        Assert.DoesNotContain(result.Ordinary.Quality.Warnings, value => value.Code == "UndersideSideLeak");

        void AssertRole(Ra2VoxelCoordinate coordinate, string expectedRole)
        {
            int index = fixture.Context.Source.Cells.ToList().FindIndex(value => value.Coordinate == coordinate);
            Assert.True(index >= 0);
            Assert.Equal(expectedRole, facts.AppliedRoleIds[index]);
        }
    }

    [Fact]
    public void Materializer_ProjectsFormZonesAndTypedBoundaryIntentsThroughSinglePath()
    {
        Fixture fixture = CreateFixture(
            Ra2VoxelUnitClass.Ground,
            sourceFactory: palette => CreateVolume(palette, 4, 6, 4));
        Ra2VoxelForwardDirectionSelection orientation = Assert.IsType<Ra2VoxelForwardDirectionSelection>(
            Ra2VoxelForwardDirectionSelection.Create(fixture.Context.Source,
                fixture.Context.Composition.CompositionHash, Ra2VoxelForwardDirection.PositiveY).Selection);

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(
            fixture.Context with { Orientation = orientation });

        Assert.True(result.IsSuccess, result.Message);
        Assert.NotNull(result.SemanticIntegration!.FormZones);
        Assert.NotNull(result.SemanticIntegration.BoundaryIntents);
        Assert.NotNull(result.MaterialFamilies);
        Assert.Contains(result.Ordinary!.Plan.Roles, value => value.Id == "body.upper.v3");
        Assert.Contains(result.Ordinary.Plan.Rules, value => value.MaskId == "form.lower-skirt");
        Assert.Equal(orientation.SelectionHash,
            result.SemanticIntegration.FormZones!.OrientationSelectionHash);
    }

    [Fact]
    public void MaterialFamily_SparseDirectMaskPreservesExactAnchor()
    {
        Fixture fixture = CreateFixture(
            Ra2VoxelUnitClass.Ground,
            assignments: (_, original) => original.Select((value, index) =>
                index == 0 ? Assignment(Ra2VoxelSemanticMaterialRole.Glass) : value).ToArray());

        Ra2VoxelColourMaterializationResult result = Ra2VoxelSemanticColourMaterializer.Materialize(fixture.Context);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal((byte)150, result.Ordinary!.Colourization.Snapshot!.Cells[0].PaletteIndex);
        Assert.DoesNotContain(result.Ordinary.Quality.Warnings,
            value => value.Code == "SemanticPrecedenceMismatch");
    }

    [Fact]
    public void SemanticBoundary_UsesEffectiveRolesAndProtectsDirectMaterials()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelSceneSnapshot source = CreateVolume(palette, 3, 3, 1);
        Ra2VoxelSemanticEffectiveAssignment[] effective = source.Cells.Select(cell => cell.Coordinate.X < 2
            ? Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface, part: Ra2VoxelSemanticPartRole.BodyShell,
                regionId: cell.Coordinate.X == 0 ? "partition-a" : "partition-b")
            : Assignment(Ra2VoxelSemanticMaterialRole.Rubber, part: Ra2VoxelSemanticPartRole.Wheel,
                regionId: "wheel")).ToArray();
        Ra2VoxelSemanticMaskComposition composition = new(source.CanonicalHash, effective, new string('C', 64));
        Ra2VoxelGeometryRegionMask geometry = Ra2VoxelColourizer.BuildGeometryMask(
            source, Ra2VoxelColourTechniqueCatalog.Default.EdgePolicy);

        Ra2VoxelSemanticBoundaryProjection projection = Ra2VoxelSemanticBoundaryProjector.Project(
            source, composition, geometry, Ra2VoxelColourTechniqueCatalog.Default);

        Assert.True(projection.OpportunityCellCount > 0);
        Assert.True(projection.SelectedCellCount > 0);
        Assert.True(projection.ProtectedDirectMaterialCellCount > 0);
        Assert.All(Enumerable.Range(0, source.OccupancyCount).Where(projection.Mask.IsSelected),
            index => Assert.Equal(Ra2VoxelSemanticMaterialRole.PaintedSurface, composition[index].MaterialRole));

        Ra2VoxelSemanticEffectiveAssignment[] partitionOnly = source.Cells.Select((cell, index) =>
            Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface,
                regionId: index % 2 == 0 ? "partition-a" : "partition-b")).ToArray();
        Ra2VoxelSemanticBoundaryProjection ignoredPartitions = Ra2VoxelSemanticBoundaryProjector.Project(
            source,
            new(source.CanonicalHash, partitionOnly, new string('D', 64)),
            geometry,
            Ra2VoxelColourTechniqueCatalog.Default);
        Assert.Equal(0, ignoredPartitions.OpportunityCellCount);
        Assert.Equal(0, ignoredPartitions.SelectedCellCount);
    }

    [Fact]
    public void SurfaceCoverage_IgnoresUnknownEnclosedInteriorButCountsUnknownVisibleCells()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "coverage", 3, 3, 3);
        List<Ra2VoxelCell> cells = [];
        for (int z = 0; z < 3; z++)
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            cells.Add(new(new Ra2VoxelCoordinate(x, y, z), 100));
        Ra2VoxelSceneSnapshot source = new("COVERAGE", part, palette, cells,
            [new("fixture", new string('E', 64))]);
        Ra2VoxelSemanticEffectiveAssignment known = Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface);
        Ra2VoxelSemanticEffectiveAssignment unknown = new("unknown", Ra2VoxelSemanticPartRole.Unknown,
            Ra2VoxelSemanticMaterialRole.Unknown, Ra2VoxelSemanticRemapIntent.None,
            Ra2VoxelSemanticAssignmentSource.Unknown, 0d, "fixture");
        Ra2VoxelSemanticEffectiveAssignment[] assignments = Enumerable.Repeat(known, source.OccupancyCount).ToArray();
        int centre = source.Cells.ToList().FindIndex(value => value.Coordinate == new Ra2VoxelCoordinate(1, 1, 1));
        assignments[centre] = unknown;

        Ra2VoxelSemanticSurfaceCoverage interiorUnknown = Ra2VoxelSemanticSurfaceCoverageProjector.Project(
            source, new(source.CanonicalHash, assignments, new string('C', 64)));
        Assert.Equal(26, interiorUnknown.VisibleSurfaceCellCount);
        Assert.Equal(26, interiorUnknown.KnownVisibleSurfaceCellCount);
        Assert.Equal(0, interiorUnknown.UnknownVisibleSurfaceCellCount);

        assignments[0] = unknown;
        Ra2VoxelSemanticSurfaceCoverage visibleUnknown = Ra2VoxelSemanticSurfaceCoverageProjector.Project(
            source, new(source.CanonicalHash, assignments, new string('D', 64)));
        Assert.Equal(25, visibleUnknown.KnownVisibleSurfaceCellCount);
        Assert.Equal(1, visibleUnknown.UnknownVisibleSurfaceCellCount);
    }

    [Fact]
    public void Materializer_FailsClosedForStaleBindingAndWrongColourSkill()
    {
        Fixture fixture = CreateFixture(Ra2VoxelUnitClass.Ground);
        Ra2VoxelColourMaterializationContext wrongSkill = fixture.Context with
        {
            ColourSkill = new("ra2-air-voxel-colour-techniques", "2", new string('B', 64))
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
        byte baseIndex = 100,
        Func<Ra2VoxelPaletteProfile, Ra2VoxelSceneSnapshot>? sourceFactory = null,
        Func<Ra2VoxelPaletteProfile>? paletteFactory = null)
    {
        Ra2VoxelPaletteProfile palette = paletteFactory?.Invoke() ?? CreatePalette();
        Ra2VoxelSceneSnapshot source = sourceFactory?.Invoke(palette) ?? CreateSheet(palette);
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
            new(adaptation.ColouringSkillId, "2", new string('B', 64)),
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
        => CreateVolume(palette, 3, 3, 1);

    private static Ra2VoxelSceneSnapshot CreateVolume(
        Ra2VoxelPaletteProfile palette,
        int xSize,
        int ySize,
        int zSize)
    {
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "colour-4e", xSize, ySize, zSize);
        List<Ra2VoxelCell> cells = [];
        for (int z = 0; z < zSize; z++)
        for (int y = 0; y < ySize; y++)
        for (int x = 0; x < xSize; x++)
            cells.Add(new(new Ra2VoxelCoordinate(x, y, z), 90));
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

    private static Ra2VoxelPaletteProfile CreateRa2IndexedRampPalette()
    {
        Ra2Rgba32[] colours = Enumerable.Repeat(new Ra2Rgba32(255, 0, 255), 256).ToArray();
        (byte R, byte G, byte B)[] ramp =
        [
            (208, 208, 184), (196, 196, 172), (184, 184, 160), (172, 172, 148),
            (160, 160, 136), (148, 148, 124), (136, 136, 112), (124, 124, 100),
            (112, 112, 88), (100, 100, 76), (88, 88, 64), (76, 76, 52),
            (64, 64, 40), (52, 52, 28), (40, 40, 16), (28, 28, 4)
        ];
        for (int offset = 0; offset < ramp.Length; offset++)
            colours[64 + offset] = new(ramp[offset].R, ramp[offset].G, ramp[offset].B);
        colours[112] = new(136, 128, 88);
        colours[138] = new(120, 104, 64);
        colours[152] = new(108, 88, 44);
        colours[154] = new(100, 80, 40);
        byte[] eligible = Enumerable.Range(64, 16).Concat([112, 138, 152, 154])
            .Select(value => checked((byte)value)).ToArray();
        byte[] transparent = Enumerable.Range(0, 256)
            .Except(eligible.Select(value => (int)value))
            .Select(value => checked((byte)value)).ToArray();
        return new("ra2-indexed-ramp-fixture", colours, transparent);
    }

    private static Ra2VoxelPaletteProfile CreateMaterializationRa2Palette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        (byte R, byte G, byte B)[] ramp =
        [
            (208, 208, 184), (196, 196, 172), (184, 184, 160), (172, 172, 148),
            (160, 160, 136), (148, 148, 124), (136, 136, 112), (124, 124, 100),
            (112, 112, 88), (100, 100, 76), (88, 88, 64), (76, 76, 52),
            (64, 64, 40), (52, 52, 28), (40, 40, 16), (28, 28, 4)
        ];
        for (int offset = 0; offset < ramp.Length; offset++)
            colours[64 + offset] = new(ramp[offset].R, ramp[offset].G, ramp[offset].B);
        colours[0] = new(0, 0, 0, 0);
        return new("ra2-materialization-fixture", colours, [0],
            Enumerable.Range(16, 16).Select(value => (byte)value));
    }

    private static Ra2VoxelStyleRoleDefinition Role(string id, Ra2VoxelStyleRoleCategory category, byte index)
        => new(id, category, index, null, ["built-in"]);

    private static Ra2VoxelStyleRuleDefinition Rule(Ra2VoxelStyleRegionKind region, string roleId)
        => new(region, roleId, Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["built-in"]);

    private static Ra2VoxelSemanticEffectiveAssignment Assignment(
        Ra2VoxelSemanticMaterialRole material,
        Ra2VoxelSemanticRemapIntent remap = Ra2VoxelSemanticRemapIntent.None,
        Ra2VoxelSemanticPartRole part = Ra2VoxelSemanticPartRole.BodyShell,
        string regionId = "fixture")
        => new(regionId, part, material, remap,
            Ra2VoxelSemanticAssignmentSource.HumanOverride, 1d, "fixture");

    private sealed record Fixture(Ra2VoxelColourMaterializationContext Context);
}
