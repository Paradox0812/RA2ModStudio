using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldAnnotationProviderTests
{
    [Fact]
    public void Find_ExactSectionKindWinsOverWildcard()
    {
        Ra2FieldAnnotationProvider provider = new(new Ra2FieldAnnotationPack(1, "zh-CN", [
            new Ra2FieldAnnotationEntry("*", "Strength", "Generic HP"),
            new Ra2FieldAnnotationEntry("Vehicle", "Strength", "Vehicle HP")
        ]));

        Ra2FieldAnnotationEntry? entry = provider.Find(Ra2SectionKind.Vehicle, "Strength");

        Assert.NotNull(entry);
        Assert.Equal("Vehicle HP", entry.DisplayName);
    }

    [Fact]
    public void Find_UsesWildcardWhenExactIsMissing()
    {
        Ra2FieldAnnotationProvider provider = new(new Ra2FieldAnnotationPack(1, "zh-CN", [
            new Ra2FieldAnnotationEntry("*", "Prerequisite", "Prereq")
        ]));

        Ra2FieldAnnotationEntry? entry = provider.Find(Ra2SectionKind.Building, "Prerequisite");

        Assert.NotNull(entry);
        Assert.Equal("Prereq", entry.DisplayName);
    }

    [Fact]
    public void Find_IsCaseInsensitiveAndAcceptsTypeSuffix()
    {
        Ra2FieldAnnotationProvider provider = new(new Ra2FieldAnnotationPack(1, "zh-CN", [
            new Ra2FieldAnnotationEntry("VehicleType", "strength", "Vehicle HP")
        ]));

        Ra2FieldAnnotationEntry? entry = provider.Find(Ra2SectionKind.Vehicle, "Strength");

        Assert.NotNull(entry);
        Assert.Equal("Vehicle HP", entry.DisplayName);
    }

    [Fact]
    public void Find_ReturnsNullWhenMissing()
    {
        Ra2FieldAnnotationProvider provider = new(new Ra2FieldAnnotationPack(1, "zh-CN", []));

        Assert.Null(provider.Find(Ra2SectionKind.Vehicle, "Strength"));
    }

    [Fact]
    public void DuplicateEntries_UseLastWinsBehavior()
    {
        Ra2FieldAnnotationProvider provider = new(new Ra2FieldAnnotationPack(1, "zh-CN", [
            new Ra2FieldAnnotationEntry("Vehicle", "Strength", "Old"),
            new Ra2FieldAnnotationEntry("Vehicle", "Strength", "New")
        ]));

        Ra2FieldAnnotationEntry? entry = provider.Find(Ra2SectionKind.Vehicle, "Strength");

        Assert.NotNull(entry);
        Assert.Equal("New", entry.DisplayName);
    }
}
