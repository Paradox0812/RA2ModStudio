using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2LanguageNavigationViewModelTests
{
    [Fact]
    public void PeekDefinitionViewModel_MapsDefinitionTargetForDisplay()
    {
        Ra2DefinitionTarget target = new(
            Ra2DefinitionTargetKind.FieldDefinition,
            "Strength",
            "Type: Integer",
            "BuiltIn",
            null,
            null,
            null,
            "Hit points");

        Ra2PeekDefinitionViewModel viewModel = new(target);

        Assert.Equal("Strength", viewModel.Title);
        Assert.Equal("字段定义", viewModel.Kind);
        Assert.Equal("类型：Integer", viewModel.Detail);
        Assert.Equal("BuiltIn", viewModel.SourceName);
        Assert.Equal("无源码行", viewModel.LineText);
        Assert.Equal("Hit points", viewModel.Description);
    }

    [Fact]
    public void FindReferencesViewModel_MapsReferenceItemsForDisplay()
    {
        Ra2ReferenceResult result = new(
            "120mm",
            Ra2SectionKind.Weapon,
            [
                new Ra2ReferenceItem("NEWINF", "Primary", "120mm", 5, new Ra2TextSpan(30, 13), new Ra2TextSpan(38, 5)),
                new Ra2ReferenceItem("TANK", "Secondary", "120mm", 8, new Ra2TextSpan(50, 15), new Ra2TextSpan(60, 5))
            ]);

        Ra2FindReferencesViewModel viewModel = new(result);

        Assert.Equal("[120mm]（Weapon）", viewModel.Target);
        Assert.Equal("当前文件中找到 2 处引用。", viewModel.StatusText);
        Assert.Equal(2, viewModel.References.Count);
        Assert.Equal("NEWINF", viewModel.References[0].Section);
        Assert.Equal("Primary", viewModel.References[0].Key);
        Assert.Equal(38, viewModel.References[0].ValueSpanStart);
    }

    [Fact]
    public void PeekDefinitionViewModel_LocalizesReferenceTargetMetadata()
    {
        Ra2DefinitionTarget target = new(
            Ra2DefinitionTargetKind.ReferenceTarget,
            "M60",
            "Weapon reference target in current document.",
            "Current document",
            "C:\\game\\rulesmd.ini",
            null,
            22681,
            "目标备注: GIWeapon\r\n位置: Line 22681");

        Ra2PeekDefinitionViewModel viewModel = new(target);

        Assert.Equal("引用目标", viewModel.Kind);
        Assert.Equal("当前文件中的 Weapon 引用目标。", viewModel.Detail);
        Assert.Equal("当前文件", viewModel.SourceName);
        Assert.Equal("第 22681 行", viewModel.LineText);
        Assert.Contains("备注：GIWeapon", viewModel.Description);
        Assert.Contains("位置：第 22681 行", viewModel.Description);
        Assert.DoesNotContain("ReferenceTarget", viewModel.Kind);
        Assert.DoesNotContain("Current document", viewModel.SourceName);
        Assert.DoesNotContain("Weapon reference target in current document", viewModel.Detail);
    }

    [Fact]
    public void FindReferencesViewModel_UsesChineseEmptyStatusText()
    {
        Ra2ReferenceResult result = new("120mm", Ra2SectionKind.Weapon, []);

        Ra2FindReferencesViewModel viewModel = new(result);

        Assert.Equal("当前文件中未找到引用。", viewModel.StatusText);
    }
}
