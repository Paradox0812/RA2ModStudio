using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class Ra2FieldAppliesToNormalizerTests
{
    [Theory]
    [InlineData("Art", Ra2SectionKind.ArtObject)]
    [InlineData("UnitArt", Ra2SectionKind.ArtObject)]
    [InlineData("EVA", Ra2SectionKind.Eva)]
    [InlineData("AircraftType", Ra2SectionKind.Aircraft)]
    [InlineData("VoxelAnimType", Ra2SectionKind.VoxelAnim)]
    [InlineData("SuperWeaponType", Ra2SectionKind.SuperWeapon)]
    public void TryNormalize_MapsAliases(string raw, Ra2SectionKind expected)
    {
        Assert.True(Ra2FieldAppliesToNormalizer.TryNormalize(raw, out IReadOnlyList<Ra2SectionKind> kinds, out string? warning));

        Assert.Equal([expected], kinds);
        Assert.Null(warning);
    }

    [Theory]
    [InlineData("Building or Vehicle", new[] { Ra2SectionKind.Building, Ra2SectionKind.Vehicle })]
    [InlineData("Building/Vehicle", new[] { Ra2SectionKind.Building, Ra2SectionKind.Vehicle })]
    [InlineData("Techno or SW", new[] { Ra2SectionKind.Techno, Ra2SectionKind.SuperWeapon })]
    [InlineData("Techno/SW", new[] { Ra2SectionKind.Techno, Ra2SectionKind.SuperWeapon })]
    public void TryNormalize_ExpandsCompositeValues(string raw, Ra2SectionKind[] expected)
    {
        Assert.True(Ra2FieldAppliesToNormalizer.TryNormalize(raw, out IReadOnlyList<Ra2SectionKind> kinds, out string? warning));

        Assert.Equal(expected, kinds);
        Assert.Null(warning);
    }

    [Fact]
    public void TryNormalize_DoesNotMapUnknownToUnknownSilently()
    {
        Assert.False(Ra2FieldAppliesToNormalizer.TryNormalize("NotASection", out IReadOnlyList<Ra2SectionKind> kinds, out string? warning));

        Assert.Empty(kinds);
        Assert.Contains("unknown appliesTo value", warning);
    }
}
