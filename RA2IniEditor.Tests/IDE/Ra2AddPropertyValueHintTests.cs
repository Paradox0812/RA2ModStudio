using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AddPropertyValueHintTests
{
    private readonly Ra2AddPropertyValueHintProvider _provider = new();

    [Theory]
    [InlineData("Strength", "Integer", "整数。示例：400")]
    [InlineData("Powered", "Boolean", "布尔值。常用：yes/no")]
    [InlineData("Armor", "Enum", "枚举值。常用：")]
    [InlineData("Primary", "Reference", "引用字段。目标类型：Weapon")]
    [InlineData("Owner", "MultiSelect", "列表字段。多个值用逗号分隔。")]
    [InlineData("Custom", "Unknown", "请手动输入字段值。")]
    public void GetHint_UsesFieldTypeWithoutChangingValueText(string key, string typeDisplay, string expected)
    {
        Ra2AddPropertyItemViewModel item = CreateItem(key, typeDisplay);

        string hint = _provider.GetHint(item);

        Assert.Contains(expected, hint);
        Assert.Equal(string.Empty, item.SuggestedValue);
    }

    [Fact]
    public void GetHint_NullItemShowsManualInputHint()
    {
        Assert.Equal("请手动输入字段值。", _provider.GetHint(null));
    }

    private static Ra2AddPropertyItemViewModel CreateItem(string key, string typeDisplay)
    {
        return new Ra2AddPropertyItemViewModel(
            Ra2SectionKind.Vehicle,
            new Ra2FieldDisplayInfo(
                key,
                key,
                [],
                null,
                null,
                typeDisplay,
                "Vehicle",
                "BuiltIn",
                hasUserAnnotation: false));
    }
}
