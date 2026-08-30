using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelSemanticColourContractTests
{
    [Fact]
    public void Requirements_ProjectStableCountsAndShapeWithoutCellBoundaryIdentity()
    {
        string sourceHash = new('A', 64);
        Ra2VoxelSemanticEffectiveAssignment painted = Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface);
        Ra2VoxelSemanticEffectiveAssignment glass = Assignment(Ra2VoxelSemanticMaterialRole.Glass);
        Ra2VoxelSemanticEffectiveAssignment approved = Assignment(
            Ra2VoxelSemanticMaterialRole.Accent,
            Ra2VoxelSemanticRemapIntent.ExplicitlyApproved);
        Ra2VoxelSemanticColourRequirements first = Ra2VoxelSemanticColourRequirementsProjector.Project(
            new(sourceHash, [painted, painted, glass, approved], new string('B', 64)));
        Ra2VoxelSemanticColourRequirements second = Ra2VoxelSemanticColourRequirementsProjector.Project(
            new(sourceHash, [painted, glass, glass, approved], new string('C', 64)));

        Assert.Equal(4, first.CellCount);
        Assert.Equal(1, first.ApprovedRemapCellCount);
        Assert.Equal(first.RequirementShapeHash, second.RequirementShapeHash);
        Assert.NotEqual(first.CompositionHash, second.CompositionHash);
        Assert.Equal(
            [Ra2VoxelSemanticColourRequirementKind.PaintedSurface,
             Ra2VoxelSemanticColourRequirementKind.Glass,
             Ra2VoxelSemanticColourRequirementKind.Accent,
             Ra2VoxelSemanticColourRequirementKind.ApprovedRemap],
            first.Required.Select(value => value.Kind));
        Assert.Equal(first.CellCount, first.MaterialCounts.Sum(value => value.CellCount));
    }

    [Fact]
    public void Requirements_UnknownIsReportedButDoesNotCreateBindingRequirement()
    {
        Ra2VoxelSemanticColourRequirements requirements = Ra2VoxelSemanticColourRequirementsProjector.Project(
            new(new string('A', 64),
                [Assignment(Ra2VoxelSemanticMaterialRole.Unknown), Assignment(Ra2VoxelSemanticMaterialRole.PaintedSurface)],
                new string('B', 64)));

        Assert.Equal(1, requirements.UnknownCellCount);
        Assert.Single(requirements.Required);
        Assert.Equal(Ra2VoxelSemanticColourRequirementKind.PaintedSurface, requirements.Required[0].Kind);
    }

    [Fact]
    public void BindingPlan_RequiresExactCompatibleBindingsAndDistinctLightAccentRoles()
    {
        Ra2VoxelSemanticColourRequirements requirements = Requirements(
            Ra2VoxelSemanticMaterialRole.PaintedSurface,
            Ra2VoxelSemanticMaterialRole.Glass,
            Ra2VoxelSemanticMaterialRole.Light,
            Ra2VoxelSemanticMaterialRole.Accent);
        Ra2CompiledVoxelStylePlan plan = Plan();
        Ra2VoxelSemanticColourBinding[] valid =
        [
            new(Ra2VoxelSemanticColourRequirementKind.PaintedSurface,
                Ra2VoxelSemanticColourBindingMode.BodyGeometryFamily, "body.base"),
            new(Ra2VoxelSemanticColourRequirementKind.Glass,
                Ra2VoxelSemanticColourBindingMode.DirectRole, "glass"),
            new(Ra2VoxelSemanticColourRequirementKind.Light,
                Ra2VoxelSemanticColourBindingMode.DirectRole, "light"),
            new(Ra2VoxelSemanticColourRequirementKind.Accent,
                Ra2VoxelSemanticColourBindingMode.DirectRole, "accent")
        ];

        Ra2VoxelSemanticColourBindingResult result = Ra2VoxelSemanticColourBindingPlan.Validate(requirements, plan, valid);
        Ra2VoxelSemanticColourBindingResult reordered = Ra2VoxelSemanticColourBindingPlan.Validate(
            requirements,
            plan,
            valid.Reverse());
        Ra2VoxelSemanticColourBindingResult sharedAccent = Ra2VoxelSemanticColourBindingPlan.Validate(
            requirements,
            plan,
            valid.Select(value => value.Requirement == Ra2VoxelSemanticColourRequirementKind.Light
                ? value with { RoleId = "accent" }
                : value));

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(reordered.IsSuccess, reordered.Message);
        Assert.Equal(result.Plan!.BindingPlanHash, reordered.Plan!.BindingPlanHash);
        Assert.Equal(Ra2VoxelSemanticColourBindingFailureKind.LightAccentRoleConflict, sharedAccent.FailureKind);
    }

    [Fact]
    public void BindingPlan_FailsForMissingExtraUnknownAndIncompatibleBindings()
    {
        Ra2VoxelSemanticColourRequirements requirements = Requirements(
            Ra2VoxelSemanticMaterialRole.PaintedSurface,
            Ra2VoxelSemanticMaterialRole.Glass);
        Ra2CompiledVoxelStylePlan plan = Plan();
        Ra2VoxelSemanticColourBinding painted = new(
            Ra2VoxelSemanticColourRequirementKind.PaintedSurface,
            Ra2VoxelSemanticColourBindingMode.BodyGeometryFamily,
            "body.base");
        Ra2VoxelSemanticColourBinding glass = new(
            Ra2VoxelSemanticColourRequirementKind.Glass,
            Ra2VoxelSemanticColourBindingMode.DirectRole,
            "glass");

        Assert.Equal(
            Ra2VoxelSemanticColourBindingFailureKind.MissingBinding,
            Ra2VoxelSemanticColourBindingPlan.Validate(requirements, plan, [painted]).FailureKind);
        Assert.Equal(
            Ra2VoxelSemanticColourBindingFailureKind.ExtraBinding,
            Ra2VoxelSemanticColourBindingPlan.Validate(requirements, plan,
                [painted, glass, new(Ra2VoxelSemanticColourRequirementKind.Rubber,
                    Ra2VoxelSemanticColourBindingMode.DirectRole, "rubber")]).FailureKind);
        Assert.Equal(
            Ra2VoxelSemanticColourBindingFailureKind.UnknownRole,
            Ra2VoxelSemanticColourBindingPlan.Validate(requirements, plan,
                [painted, glass with { RoleId = "missing" }]).FailureKind);
        Assert.Equal(
            Ra2VoxelSemanticColourBindingFailureKind.IncompatibleBinding,
            Ra2VoxelSemanticColourBindingPlan.Validate(requirements, plan,
                [painted, glass with { BindingMode = Ra2VoxelSemanticColourBindingMode.BodyGeometryFamily }]).FailureKind);
        Assert.Equal(
            Ra2VoxelSemanticColourBindingFailureKind.IncompatibleBinding,
            Ra2VoxelSemanticColourBindingPlan.Validate(requirements, Plan(completeBodyFamily: false), [painted, glass]).FailureKind);
    }

    private static Ra2VoxelSemanticColourRequirements Requirements(params Ra2VoxelSemanticMaterialRole[] materials)
        => Ra2VoxelSemanticColourRequirementsProjector.Project(
            new(new string('A', 64), materials.Select(material => Assignment(material)), new string('B', 64)));

    private static Ra2VoxelSemanticEffectiveAssignment Assignment(
        Ra2VoxelSemanticMaterialRole material,
        Ra2VoxelSemanticRemapIntent remap = Ra2VoxelSemanticRemapIntent.None)
        => new(
            "region",
            Ra2VoxelSemanticPartRole.BodyShell,
            material,
            remap,
            Ra2VoxelSemanticAssignmentSource.HumanOverride,
            1d,
            "test");

    private static Ra2CompiledVoxelStylePlan Plan(bool completeBodyFamily = true)
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new("binding-test", colours, [0], Enumerable.Range(16, 16).Select(value => (byte)value));
        List<Ra2VoxelStyleRoleDefinition> roles =
        [
            new("body.base", Ra2VoxelStyleRoleCategory.BodyBase, 60, null, ["test"]),
            new("body.light", Ra2VoxelStyleRoleCategory.BodyLight, 72, null, ["test"]),
            new("body.mid", Ra2VoxelStyleRoleCategory.BodyMid, 52, null, ["test"]),
            new("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, 40, null, ["test"]),
            new("body.under", Ra2VoxelStyleRoleCategory.Underside, 32, null, ["test"]),
            new("glass", Ra2VoxelStyleRoleCategory.Glass, 80, null, ["test"]),
            new("rubber", Ra2VoxelStyleRoleCategory.Rubber, 35, null, ["test"]),
            new("light", Ra2VoxelStyleRoleCategory.Accent, 200, null, ["test"]),
            new("accent", Ra2VoxelStyleRoleCategory.Accent, 180, null, ["test"]),
            new("remap", Ra2VoxelStyleRoleCategory.Remap, 16, null, ["test"])
        ];
        if (!completeBodyFamily)
            roles.RemoveAll(role => role.Category == Ra2VoxelStyleRoleCategory.Underside);
        Ra2VoxelStylePlanDefinition definition = new(
            "binding", "binding", new string('C', 64), palette.ProfileHash, "test-1", "fixture-1",
            Ra2VoxelStyleRemapPolicy.ExplicitMask, "body.dark",
            roles,
            [
                new(Ra2VoxelStyleRegionKind.WholePart, "body.base", Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["test"]),
                new(Ra2VoxelStyleRegionKind.Interior, "body.dark", Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["test"])
            ]);
        Ra2VoxelStylePlanCompilationResult result = Ra2VoxelStylePlanCompiler.Compile(definition, palette, ["test"]);
        Assert.True(result.IsSuccess, result.Message);
        return result.Plan!;
    }
}
