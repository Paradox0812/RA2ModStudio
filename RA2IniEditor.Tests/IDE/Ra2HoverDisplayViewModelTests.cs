using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2HoverDisplayViewModelTests
{
    [Theory]
    [InlineData("Enum", "Armor", "\u88c5\u7532\u7c7b\u578b", "Enum Armor \u88c5\u7532\u7c7b\u578b")]
    [InlineData("Integer", "Strength", "\u751f\u547d\u503c", "Integer Strength \u751f\u547d\u503c")]
    [InlineData("Reference", "Primary", "\u4e3b\u6b66\u5668", "Reference Primary \u4e3b\u6b66\u5668")]
    [InlineData("Integer", "Cost", "Cost", "Integer Cost")]
    [InlineData("", "Image", "", "Image")]
    public void BuildTitle_UsesTypeRawKeyThenDisplayName(
        string typeDisplay,
        string key,
        string displayName,
        string expected)
    {
        Assert.Equal(expected, Ra2HoverDisplayViewModel.BuildTitle(typeDisplay, key, displayName));
    }

    [Fact]
    public void ToToolTipText_UsesAtMostTwoLines()
    {
        Ra2HoverDisplayViewModel viewModel = new(
            "Enum Armor \u88c5\u7532\u7c7b\u578b",
            "\u5355\u4f4d\u4f7f\u7528\u7684\u88c5\u7532\u7c7b\u522b\u3002");

        string text = viewModel.ToToolTipText();

        string[] lines = text.Split(Environment.NewLine);
        Assert.Equal(2, lines.Length);
        Assert.Equal("Enum Armor \u88c5\u7532\u7c7b\u578b", lines[0]);
        Assert.Equal("\u5355\u4f4d\u4f7f\u7528\u7684\u88c5\u7532\u7c7b\u522b\u3002", lines[1]);
        Assert.DoesNotContain("Source", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Alias", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToToolTipText_WhenNoteIsEmpty_UsesTitleOnly()
    {
        Ra2HoverDisplayViewModel viewModel = new("Integer Cost", null);

        Assert.Equal("Integer Cost", viewModel.ToToolTipText());
    }

    [Fact]
    public void FromHoverInfo_UsesDescriptionAsSecondLine()
    {
        Ra2HoverInfo hover = new(
            "\u88c5\u7532\u7c7b\u578b",
            "Field",
            "ignored detail",
            "\u6ce8\u91ca\u6216\u5b57\u6bb5\u5e93\u63cf\u8ff0",
            "BuiltIn",
            new Ra2TextSpan(0, 5),
            "Armor",
            "\u88c5\u7532\u7c7b\u578b",
            "Enum",
            ["alias"]);

        string text = Ra2HoverDisplayViewModel.FromHoverInfo(hover).ToToolTipText();

        Assert.Equal(
            "Enum Armor \u88c5\u7532\u7c7b\u578b" + Environment.NewLine + "\u6ce8\u91ca\u6216\u5b57\u6bb5\u5e93\u63cf\u8ff0",
            text);
    }

    [Fact]
    public void FromHoverInfo_ExposesStructuredHoverCardFields()
    {
        Ra2HoverInfo hover = new(
            "\u4e3b\u6b66\u5668",
            "Field",
            "Key: Primary; Type: Reference; Applies to: Techno",
            "\u4e3b\u6b66\u5668\u5f15\u7528\u3002\uff1b\u793a\u4f8b\uff1a120mm - Cannon weapon",
            "BuiltIn",
            new Ra2TextSpan(0, 7),
            "Primary",
            "\u4e3b\u6b66\u5668",
            "Reference");

        Ra2HoverDisplayViewModel viewModel = Ra2HoverDisplayViewModel.FromHoverInfo(hover);

        Assert.Equal("Reference Primary \u4e3b\u6b66\u5668", viewModel.Title);
        Assert.Equal("Reference", viewModel.FieldTypeText);
        Assert.Equal("Primary", viewModel.FieldNameText);
        Assert.Equal("\u4e3b\u6b66\u5668", viewModel.DisplayNameText);
        Assert.Equal("\u4e3b\u6b66\u5668\u5f15\u7528\u3002", viewModel.DescriptionText);
        Assert.Equal("120mm", viewModel.ExampleValueText);
        Assert.Equal("Cannon weapon", viewModel.ExampleDescriptionText);
        Assert.Equal("BuiltIn", viewModel.SourceText);
        Assert.Equal("Techno", viewModel.AppliesToText);
        Assert.True(viewModel.HasExample);
        Assert.True(viewModel.HasMetadata);
    }

    [Fact]
    public void FromHoverInfo_ReferenceValueHoverRendersTargetNoteInHeader()
    {
        Ra2HoverInfo hover = new(
            "M60",
            "Weapon",
            "Weapon reference target in current document.",
            "\u5f15\u7528\u5907\u6ce8: GI weapon reference",
            "Current document",
            new Ra2TextSpan(10, 3),
            "M60",
            "GIWeapon",
            "Weapon");

        Ra2HoverDisplayViewModel viewModel = Ra2HoverDisplayViewModel.FromHoverInfo(hover);

        Assert.Equal("Weapon M60 GIWeapon", viewModel.Title);
        Assert.Equal("Weapon", viewModel.FieldTypeText);
        Assert.Equal("M60", viewModel.FieldNameText);
        Assert.Equal("GIWeapon", viewModel.DisplayNameText);
        Assert.Equal("\u5f15\u7528\u5907\u6ce8: GI weapon reference", viewModel.DescriptionText);
        Assert.Equal("Current document", viewModel.SourceText);
        Assert.True(viewModel.IsReferenceValueHover);
    }

    [Fact]
    public void FromHoverInfo_KeyHoverDoesNotUseReferenceValueHeaderLayout()
    {
        Ra2HoverInfo hover = new(
            "\u4e3b\u6b66\u5668",
            "Field",
            "Key: Primary; Type: Reference; Applies to: Techno",
            "\u4e3b\u6b66\u5668\u5f15\u7528\u3002",
            "BuiltIn",
            new Ra2TextSpan(0, 7),
            "Primary",
            "\u4e3b\u6b66\u5668",
            "Reference");

        Ra2HoverDisplayViewModel viewModel = Ra2HoverDisplayViewModel.FromHoverInfo(hover);

        Assert.False(viewModel.IsReferenceValueHover);
        Assert.Equal("Reference Primary \u4e3b\u6b66\u5668", viewModel.Title);
    }
}
