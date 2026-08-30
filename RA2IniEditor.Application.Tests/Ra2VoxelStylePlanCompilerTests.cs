using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelStylePlanCompilerTests
{
    [Fact]
    public void Compiler_ResolvesPaletteRolesDeterministicallyAndCopiesCollections()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        List<Ra2VoxelStyleRoleDefinition> roles =
        [
            Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, colour: new Ra2Rgba32(80, 100, 60)),
            Role("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, exact: 40),
            Role("team", Ra2VoxelStyleRoleCategory.Remap, exact: 16)
        ];
        List<Ra2VoxelStyleRuleDefinition> rules =
        [
            Rule(Ra2VoxelStyleRegionKind.WholePart, "body.base"),
            Rule(Ra2VoxelStyleRegionKind.UnderExposed, "body.dark"),
            new(Ra2VoxelStyleRegionKind.ExplicitMask, "team", Ra2VoxelStyleEvidenceKind.ExplicitUserMask, "team-mask", ["built-in"])
        ];
        Ra2VoxelStylePlanDefinition definition = Definition(palette, roles, rules, Ra2VoxelStyleRemapPolicy.ExplicitMask);

        Ra2VoxelStylePlanCompilationResult first = Ra2VoxelStylePlanCompiler.Compile(definition, palette, ["built-in"]);
        roles.Clear();
        rules.Clear();
        Ra2VoxelStylePlanCompilationResult second = Ra2VoxelStylePlanCompiler.Compile(
            Definition(palette,
                [Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, colour: new Ra2Rgba32(80, 100, 60)), Role("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, exact: 40), Role("team", Ra2VoxelStyleRoleCategory.Remap, exact: 16)],
                [Rule(Ra2VoxelStyleRegionKind.WholePart, "body.base"), Rule(Ra2VoxelStyleRegionKind.UnderExposed, "body.dark"), new(Ra2VoxelStyleRegionKind.ExplicitMask, "team", Ra2VoxelStyleEvidenceKind.ExplicitUserMask, "team-mask", ["built-in"])],
                Ra2VoxelStyleRemapPolicy.ExplicitMask),
            palette,
            ["built-in"]);

        Assert.True(first.IsSuccess, first.Message);
        Assert.Equal(first.Plan!.PlanHash, second.Plan!.PlanHash);
        Assert.Equal((byte)16, first.Plan.Roles.Single(role => role.Id == "team").PaletteIndex);
        Assert.All(first.Plan.Rules, rule => Assert.True(rule.IsPaintable));
    }

    [Fact]
    public void Compiler_KeepsTextOnlySemanticRulesUnpaintable()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelStylePlanDefinition definition = Definition(
            palette,
            [Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, exact: 60), Role("glass", Ra2VoxelStyleRoleCategory.Glass, exact: 90)],
            [Rule(Ra2VoxelStyleRegionKind.WholePart, "body.base"), new(Ra2VoxelStyleRegionKind.ExplicitMask, "glass", Ra2VoxelStyleEvidenceKind.InferredTextOnly, "guessed", ["built-in"])],
            Ra2VoxelStyleRemapPolicy.None);

        Ra2VoxelStylePlanCompilationResult result = Ra2VoxelStylePlanCompiler.Compile(definition, palette, ["built-in"]);

        Assert.True(result.IsSuccess, result.Message);
        Assert.False(result.Plan!.Rules.Single(rule => rule.RoleId == "glass").IsPaintable);
    }

    [Fact]
    public void Compiler_RejectsRemapLeakTransparentAndUnknownScope()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();

        Assert.Equal(
            Ra2VoxelStylePlanFailureKind.RemapPolicyViolation,
            CompileWithRole(palette, Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, exact: 16)).FailureKind);
        Assert.Equal(
            Ra2VoxelStylePlanFailureKind.TransparentIndexSelected,
            CompileWithRole(palette, Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, exact: 0)).FailureKind);

        Ra2VoxelStyleRoleDefinition wrongScope = new("body.base", Ra2VoxelStyleRoleCategory.BodyBase, 60, null, ["outside"]);
        Assert.Equal(
            Ra2VoxelStylePlanFailureKind.SourceScopeMismatch,
            CompileWithRole(palette, wrongScope).FailureKind);
    }

    [Fact]
    public void Compiler_RejectsMissingBaseAndDuplicateRegionMaskKey()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2VoxelStylePlanDefinition missing = Definition(
            palette,
            [Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, exact: 60)],
            [Rule(Ra2VoxelStyleRegionKind.TopExposed, "body.base")],
            Ra2VoxelStyleRemapPolicy.None);
        Assert.Equal(
            Ra2VoxelStylePlanFailureKind.CoverageViolation,
            Ra2VoxelStylePlanCompiler.Compile(missing, palette, ["built-in"]).FailureKind);

        Ra2VoxelStylePlanDefinition duplicate = Definition(
            palette,
            [Role("body.base", Ra2VoxelStyleRoleCategory.BodyBase, exact: 60), Role("body.dark", Ra2VoxelStyleRoleCategory.BodyDark, exact: 40)],
            [Rule(Ra2VoxelStyleRegionKind.WholePart, "body.base"), Rule(Ra2VoxelStyleRegionKind.TopExposed, "body.base"), Rule(Ra2VoxelStyleRegionKind.TopExposed, "body.dark")],
            Ra2VoxelStyleRemapPolicy.None);
        Assert.Equal(
            Ra2VoxelStylePlanFailureKind.RuleConflict,
            Ra2VoxelStylePlanCompiler.Compile(duplicate, palette, ["built-in"]).FailureKind);
    }

    private static Ra2VoxelStylePlanCompilationResult CompileWithRole(Ra2VoxelPaletteProfile palette, Ra2VoxelStyleRoleDefinition role)
        => Ra2VoxelStylePlanCompiler.Compile(
            Definition(palette, [role], [Rule(Ra2VoxelStyleRegionKind.WholePart, role.Id)], Ra2VoxelStyleRemapPolicy.None),
            palette,
            ["built-in"]);

    private static Ra2VoxelStylePlanDefinition Definition(
        Ra2VoxelPaletteProfile palette,
        IEnumerable<Ra2VoxelStyleRoleDefinition> roles,
        IEnumerable<Ra2VoxelStyleRuleDefinition> rules,
        Ra2VoxelStyleRemapPolicy remapPolicy)
        => new(
            "Test style", "Test summary", new string('A', 64), palette.ProfileHash,
            "voxel-style-compiler/1", "fixture-model/1", remapPolicy, "body.base", roles, rules);

    private static Ra2VoxelStyleRoleDefinition Role(
        string id,
        Ra2VoxelStyleRoleCategory category,
        byte? exact = null,
        Ra2Rgba32? colour = null)
        => new(id, category, exact, colour, ["built-in"]);

    private static Ra2VoxelStyleRuleDefinition Rule(Ra2VoxelStyleRegionKind region, string role)
        => new(region, role, Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, ["built-in"]);

    private static Ra2VoxelPaletteProfile CreatePalette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        return new("style-test", colours, [0], Enumerable.Range(16, 16).Select(value => (byte)value));
    }
}
