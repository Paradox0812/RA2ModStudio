using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.ViewModels.FieldAnnotations;

public sealed class Ra2SectionKindDisplayNameProviderTests
{
    [Theory]
    [InlineData(Ra2SectionKind.Vehicle, "战车类")]
    [InlineData(Ra2SectionKind.Unit, "单位")]
    [InlineData(Ra2SectionKind.Techno, "科技对象")]
    [InlineData(Ra2SectionKind.Building, "建筑类")]
    [InlineData(Ra2SectionKind.Weapon, "武器")]
    [InlineData(Ra2SectionKind.Warhead, "弹头")]
    [InlineData(Ra2SectionKind.Side, "阵营大类")]
    [InlineData(Ra2SectionKind.AttachEffect, "附加效果")]
    [InlineData(Ra2SectionKind.Shield, "护盾")]
    [InlineData(Ra2SectionKind.LaserTrail, "激光尾迹")]
    [InlineData(Ra2SectionKind.DigitalDisplay, "数字显示")]
    [InlineData(Ra2SectionKind.Banner, "横幅显示")]
    [InlineData(Ra2SectionKind.Insignia, "徽章/标识")]
    [InlineData(Ra2SectionKind.Radiation, "辐射")]
    [InlineData(Ra2SectionKind.Eva, "EVA 事件")]
    [InlineData(Ra2SectionKind.Tiberium, "资源/矿石")]
    public void GetDisplayName_ReturnsChineseLabel(Ra2SectionKind kind, string expected)
    {
        Ra2SectionKindDisplayNameProvider provider = new();

        Assert.Equal(expected, provider.GetDisplayName(kind));
    }

    [Fact]
    public void GetDisplayName_NullKind_ReturnsAllTypesLabel()
    {
        Ra2SectionKindDisplayNameProvider provider = new();

        Assert.Equal("全部类型", provider.GetDisplayName((Ra2SectionKind?)null));
    }
}
