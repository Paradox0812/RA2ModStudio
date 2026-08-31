using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2VoxelColourTemplateContractTests
{
    [Fact]
    public void TechniqueCatalog_IsCompleteUniqueAndDeterministic()
    {
        IReadOnlyList<Ra2VoxelColourTechniquePolicy> policies = Ra2VoxelColourTechniqueCatalog.All;

        Assert.Equal(5, policies.Count);
        Assert.Equal(5, policies.Select(value => value.TechniqueId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("balanced-rts-volume", Ra2VoxelColourTechniqueCatalog.Default.TechniqueId);
        Assert.All(policies, policy =>
        {
            Assert.Equal(64, policy.PolicyHash.Length);
            Assert.InRange(policy.MinimumBodyLuminanceSeparation, 1, 32);
            Assert.InRange(policy.DarkOpeningMinimumDelta, 12, 64);
            Assert.Equal(policy, Ra2VoxelColourTechniqueCatalog.Find(policy.TechniqueId));
            Assert.Equal(Ra2VoxelColourTechniquePolicy.LuminanceMetricId, "rec709-srgb-byte-luma-v1");
            Assert.Equal(Ra2VoxelColourTechniquePolicy.ColourFamilyMetricId, "indexed-ramp-oklab-v2");
        });

        Ra2VoxelColourTechniquePolicy copy = new(
            "balanced-rts-volume", "2", "不同显示名", "不同说明不属于运行时数值身份。",
            18, -8, -28, -38, Ra2VoxelColourEdgePolicy.Subtle, 24,
            Ra2VoxelMaterialSeparationPolicy.Balanced, 8, 18,
            Ra2VoxelAccentPolicy.PreserveMask, Ra2VoxelQuantizationFallback.WarnAndPreserveIntent);
        Assert.Equal(Ra2VoxelColourTechniqueCatalog.Default.PolicyHash, copy.PolicyHash);
    }

    [Fact]
    public void TechniqueCatalog_FiveTechniquesHaveDistinctRuntimePolicies()
    {
        string[] signatures = Ra2VoxelColourTechniqueCatalog.All.Select(value => string.Join('|',
            value.TopLuminanceOffset,
            value.SideLuminanceOffset,
            value.DarkLuminanceOffset,
            value.PreferredUndersideLuminanceOffset,
            value.EdgePolicy,
            value.EdgeLuminanceOffset,
            value.MaterialSeparationPolicy,
            value.AccentPolicy)).ToArray();

        Assert.Equal(5, signatures.Distinct(StringComparer.Ordinal).Count());
        Assert.All(Ra2VoxelColourTechniqueCatalog.All, value => Assert.Equal("2", value.Revision));
        Assert.All(Ra2VoxelUnitAdaptationCatalog.All, value => Assert.Equal("2", value.Revision));
    }

    [Fact]
    public void TechniqueCatalog_FreezesContractValuesWithoutColourTheme()
    {
        Ra2VoxelColourTechniquePolicy[] policies = Ra2VoxelColourTechniqueCatalog.All.ToArray();

        Assert.Equal([18, 28, 12, 16, 24], policies.Select(value => value.TopLuminanceOffset));
        Assert.Equal([-8, -12, -5, -7, -10], policies.Select(value => value.SideLuminanceOffset));
        Assert.Equal([-28, -38, -20, -26, -34], policies.Select(value => value.DarkLuminanceOffset));
        Assert.Equal([-38, -52, -28, -36, -46], policies.Select(value => value.PreferredUndersideLuminanceOffset));
        Assert.All(policies, policy =>
        {
            Assert.DoesNotContain("RGB", policy.Description, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("palette index", policy.Description, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void BaseColour_IsExactHumanPaletteEntryAndRejectsTransparentRemapOrMismatch()
    {
        Ra2VoxelPaletteProfile palette = Palette();
        Ra2VoxelBaseColourSelectionResult valid = Ra2VoxelBaseColourSelection.Create(palette, palette.ProfileHash, 60);
        Ra2VoxelBaseColourSelectionResult transparent = Ra2VoxelBaseColourSelection.Create(palette, palette.ProfileHash, 0);
        Ra2VoxelBaseColourSelectionResult translucent = Ra2VoxelBaseColourSelection.Create(palette, palette.ProfileHash, 1);
        Ra2VoxelBaseColourSelectionResult remap = Ra2VoxelBaseColourSelection.Create(palette, palette.ProfileHash, 16);
        Ra2VoxelBaseColourSelectionResult mismatch = Ra2VoxelBaseColourSelection.Create(palette, new string('A', 64), 60);

        Assert.True(valid.IsSuccess, valid.Message);
        Assert.Equal((byte)60, valid.Selection!.PaletteIndex);
        Assert.Equal(palette[60], valid.Selection.ResolvedRgba);
        Assert.Equal("HumanPaletteSelection", valid.Selection.Source);
        Assert.Equal(64, valid.Selection.SelectionHash.Length);
        Assert.Equal(Ra2VoxelBaseColourFailureKind.TransparentIndex, transparent.FailureKind);
        Assert.Equal(Ra2VoxelBaseColourFailureKind.TransparentIndex, translucent.FailureKind);
        Assert.Equal(Ra2VoxelBaseColourFailureKind.RemapIndex, remap.FailureKind);
        Assert.Equal(Ra2VoxelBaseColourFailureKind.PaletteMismatch, mismatch.FailureKind);
    }

    [Fact]
    public void ConfirmedClass_MapsToExactlyOneAdaptationAndColouringSkill()
    {
        IReadOnlyList<Ra2VoxelUnitAdaptationPolicy> policies = Ra2VoxelUnitAdaptationCatalog.All;

        Assert.Equal(4, policies.Count);
        Assert.Equal(4, policies.Select(value => value.UnitClass).Distinct().Count());
        Assert.Equal(4, policies.Select(value => value.ColouringSkillId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("ra2-ground-voxel-colour-techniques",
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground).ColouringSkillId);
        Assert.Equal(Ra2VoxelDualSurfacePolicy.UnderPreferred,
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Ground).DualSurfacePolicy);
        Assert.Equal(Ra2VoxelUndersideDirectionPolicy.EitherDirection,
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Air).UndersideDirection);
        Assert.Equal(Ra2VoxelDualSurfacePolicy.TopPreferred,
            Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.LargeSurface).DualSurfacePolicy);
        Assert.True(Ra2VoxelUnitAdaptationCatalog.For(Ra2VoxelUnitClass.Unknown).ForceNeedsReview);
        Assert.All(policies, policy => Assert.Equal(64, policy.PolicyHash.Length));
    }

    [Fact]
    public void InvalidTechniquePolicy_FailsClosed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2VoxelColourTechniquePolicy(
            "invalid", "1", "invalid", "invalid", 18, 4, -28, -38,
            Ra2VoxelColourEdgePolicy.Subtle, 24,
            Ra2VoxelMaterialSeparationPolicy.Balanced, 8, 18,
            Ra2VoxelAccentPolicy.PreserveMask, Ra2VoxelQuantizationFallback.WarnAndPreserveIntent));
        Assert.Throws<ArgumentException>(() => new Ra2VoxelColourTechniquePolicy(
            "invalid", "1", "invalid", "invalid", 18, -8, -28, -38,
            Ra2VoxelColourEdgePolicy.None, 24,
            Ra2VoxelMaterialSeparationPolicy.Balanced, 8, 18,
            Ra2VoxelAccentPolicy.PreserveMask, Ra2VoxelQuantizationFallback.WarnAndPreserveIntent));
    }

    private static Ra2VoxelPaletteProfile Palette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        colours[1] = new(1, 1, 1, 128);
        return new("template-test", colours, [0], Enumerable.Range(16, 16).Select(value => (byte)value));
    }
}
