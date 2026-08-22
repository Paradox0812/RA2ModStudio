using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.FieldAnnotations;

public sealed class Ra2FieldAnnotationEditingServiceTests
{
    private readonly Ra2FieldAnnotationEditingService _service = new();

    [Fact]
    public void Upsert_AddsOrReplacesExactEntry()
    {
        Ra2FieldAnnotationPack pack = new(
            1,
            "zh-CN",
            [new Ra2FieldAnnotationEntry("Vehicle", "Cost", "旧名称")]);

        Ra2FieldAnnotationPack updated = _service.Upsert(
            pack,
            "Vehicle",
            "Cost",
            "造价",
            ["价格", "价格", "  花费  "],
            "建造消耗。");

        Ra2FieldAnnotationEntry entry = Assert.Single(updated.Entries);
        Assert.Equal("Vehicle", entry.SectionKind);
        Assert.Equal("Cost", entry.Key);
        Assert.Equal("造价", entry.DisplayName);
        Assert.Equal(["价格", "花费"], entry.Aliases);
        Assert.Equal("建造消耗。", entry.Note);
    }

    [Fact]
    public void Upsert_BlankContent_RemovesExactEntry()
    {
        Ra2FieldAnnotationPack pack = new(
            1,
            "zh-CN",
            [
                new Ra2FieldAnnotationEntry("Vehicle", "Cost", "造价"),
                new Ra2FieldAnnotationEntry("*", "Cost", "通用造价")
            ]);

        Ra2FieldAnnotationPack updated = _service.Upsert(pack, "Vehicle", "Cost", "", [], " ");

        Ra2FieldAnnotationEntry entry = Assert.Single(updated.Entries);
        Assert.Equal("*", entry.SectionKind);
    }

    [Fact]
    public void Remove_OnlyRemovesRequestedSectionAndKey()
    {
        Ra2FieldAnnotationPack pack = new(
            1,
            "zh-CN",
            [
                new Ra2FieldAnnotationEntry("Vehicle", "Cost", "载具造价"),
                new Ra2FieldAnnotationEntry("Building", "Cost", "建筑造价")
            ]);

        Ra2FieldAnnotationPack updated = _service.Remove(pack, "Vehicle", "Cost");

        Ra2FieldAnnotationEntry entry = Assert.Single(updated.Entries);
        Assert.Equal("Building", entry.SectionKind);
        Assert.Equal("Cost", entry.Key);
    }
}
