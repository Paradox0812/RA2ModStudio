using System.Security.Cryptography;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelPaletteContrastTests
{
    [Fact]
    public void Optimize_SeparatesBodyRolesWithoutMutatingSourcePlan()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2CompiledVoxelStylePlan source = CreatePlan(palette, exactDark: false);
        string sourceHash = source.PlanHash;
        byte[] sourceIndices = source.Roles.Select(role => role.PaletteIndex).ToArray();

        Ra2VoxelPaletteContrastResult result = Ra2VoxelPaletteContrastOptimizer.Optimize(source, palette);

        Assert.Equal(sourceHash, source.PlanHash);
        Assert.Equal(sourceIndices, source.Roles.Select(role => role.PaletteIndex));
        Assert.NotSame(source, result.Plan);
        Assert.True(result.Facts.ChangedRoleCount > 0);
        Assert.True(result.Facts.MinimumBodyLuminanceSeparationAfter > result.Facts.MinimumBodyLuminanceSeparationBefore);
        Assert.True(result.Facts.ExactPaletteSelectionsPreserved);
        Assert.Contains("palette-contrast-v1", result.Plan.CompilerRevision, StringComparison.Ordinal);
    }

    [Fact]
    public void Optimize_PreservesExplicitAndSemanticPaletteSelections()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2CompiledVoxelStylePlan source = CreatePlan(palette, exactDark: true);
        byte exact = source.Roles.Single(role => role.Id == "body_dark").PaletteIndex;
        byte glass = source.Roles.Single(role => role.Id == "glass").PaletteIndex;

        Ra2VoxelPaletteContrastResult result = Ra2VoxelPaletteContrastOptimizer.Optimize(source, palette);

        Assert.Equal(exact, result.Plan.Roles.Single(role => role.Id == "body_dark").PaletteIndex);
        Assert.Equal(glass, result.Plan.Roles.Single(role => role.Id == "glass").PaletteIndex);
        Assert.True(result.Facts.ExactPaletteSelectionsPreserved);
    }

    [Fact]
    public void Optimize_IsDeterministic()
    {
        Ra2VoxelPaletteProfile palette = CreatePalette();
        Ra2CompiledVoxelStylePlan source = CreatePlan(palette, exactDark: false);

        Ra2VoxelPaletteContrastResult first = Ra2VoxelPaletteContrastOptimizer.Optimize(source, palette);
        Ra2VoxelPaletteContrastResult second = Ra2VoxelPaletteContrastOptimizer.Optimize(source, palette);

        Assert.Equal(first.Plan.PlanHash, second.Plan.PlanHash);
        Assert.Equal(first.Facts, second.Facts);
    }

    private static Ra2CompiledVoxelStylePlan CreatePlan(Ra2VoxelPaletteProfile palette, bool exactDark)
    {
        string hash = Convert.ToHexString(SHA256.HashData([4, 5, 6]));
        Ra2CompiledVoxelStyleRole[] roles =
        [
            new("body_base", Ra2VoxelStyleRoleCategory.BodyBase, 120, null, new(120, 120, 120), ["style"]),
            new("body_light", Ra2VoxelStyleRoleCategory.BodyLight, 121, null, new(121, 121, 121), ["style"]),
            new("body_mid", Ra2VoxelStyleRoleCategory.BodyMid, 119, null, new(119, 119, 119), ["style"]),
            new("body_dark", Ra2VoxelStyleRoleCategory.BodyDark, 118, exactDark ? (byte)118 : null, exactDark ? null : new(118, 118, 118), ["style"]),
            new("underside", Ra2VoxelStyleRoleCategory.Underside, 117, null, new(117, 117, 117), ["style"]),
            new("glass", Ra2VoxelStyleRoleCategory.Glass, 200, (byte)200, null, ["style"])
        ];
        return new(
            "test", "test", hash, palette.ProfileHash, "test/1", "fake-model",
            Ra2VoxelStyleRemapPolicy.None, "body_dark", roles,
            [new(Ra2VoxelStyleRegionKind.WholePart, "body_base", Ra2VoxelStyleEvidenceKind.DeterministicGeometry, null, true, ["style"])],
            []);
    }

    private static Ra2VoxelPaletteProfile CreatePalette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        return new("contrast-palette", colours, [0]);
    }
}
