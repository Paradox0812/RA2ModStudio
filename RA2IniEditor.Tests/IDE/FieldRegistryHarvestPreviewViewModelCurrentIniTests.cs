using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryHarvestPreviewViewModelCurrentIniTests
{
    [Fact]
    public void LoadCurrentIniHarvestPreview_PopulatesDraftRowsDefinitionsAndDiff()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();

        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [InfantryTypes]
            0=E1

            [E1]
            CustomHarvestFlag=yes
            """);

        FieldRegistryIniDraftRowViewModel row = Assert.Single(viewModel.CurrentIniDraftRows);
        Assert.Equal("CustomHarvestFlag", row.Key);
        Assert.Equal("Infantry", row.SectionKind);
        Assert.Equal("Boolean", row.EditorKind);
        Assert.Equal("Boolean", row.ValueKind);
        Assert.Equal("YesNo", row.BooleanStyle);
        Assert.Equal("yes", row.SampleValueSummary);
        Assert.Single(viewModel.Definitions);
        Assert.Single(viewModel.DiffRows);
        Assert.Equal(1, viewModel.AddedCount);
        Assert.True(viewModel.HasCurrentIniDraftRows);
        Assert.Contains("当前 INI 预览已生成", viewModel.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void LoadCurrentIniHarvestPreview_DoesNotTreatRegistryEntriesAsFields()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();

        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [InfantryTypes]
            0=E1
            1=GGI
            """);

        Assert.Empty(viewModel.CurrentIniDraftRows);
        Assert.Empty(viewModel.Definitions);
        Assert.DoesNotContain(viewModel.CurrentIniDraftRows, row => row.Key == "0" || row.Key == "1");
        Assert.Contains("未发现字段候选", viewModel.CurrentIniHarvestStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void LoadCurrentIniHarvestPreview_SkipsNumericListEntriesAndShowsStatus()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();

        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [SomeListSection]
            39=TREE24,CRATER05,GEM12
            40=TREE25,CRATER06,RadBeamWarhead
            CustomField=yes
            """);

        FieldRegistryIniDraftRowViewModel row = Assert.Single(viewModel.CurrentIniDraftRows);
        Assert.Equal("CustomField", row.Key);
        Assert.Contains("已跳过 2 个数字或列表项", viewModel.CurrentIniHarvestStatusText, StringComparison.Ordinal);
        Assert.DoesNotContain(viewModel.CurrentIniDraftRows, candidate => candidate.Key == "39" || candidate.Key == "40");
    }

    [Fact]
    public void LoadCurrentIniHarvestPreview_DoesNotAutoBuildApplyPlan()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();

        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [VehicleTypes]
            0=TESTTANK

            [TESTTANK]
            CustomHarvestFlag=no
            """);

        Assert.Empty(viewModel.ApplyPlanItems);
        Assert.False(viewModel.CanApply);
        Assert.True(viewModel.CanBuildApplyPlan);
        Assert.Contains("请先解析并预览，再构建应用计划", viewModel.ApplyStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void LoadCurrentIniHarvestPreview_EmptyTextClearsPreviousCurrentIniRows()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();
        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [E1]
            CustomHarvestFlag=yes
            """);
        Assert.NotEmpty(viewModel.CurrentIniDraftRows);

        viewModel.LoadCurrentIniHarvestPreview("empty.ini", string.Empty);

        Assert.Empty(viewModel.CurrentIniDraftRows);
        Assert.Empty(viewModel.Definitions);
        Assert.False(viewModel.HasCurrentIniDraftRows);
        Assert.Contains("当前 INI 文本为空", viewModel.CurrentIniHarvestStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildApplyPlan_RebuildsPreviewFromEditedCurrentIniDraftRows()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();
        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [VehicleTypes]
            0=TESTTANK
            1=TESTTANK2

            [TESTTANK]
            CustomArmor=light

            [TESTTANK2]
            CustomArmor=heavy
            """);

        FieldRegistryIniDraftRowViewModel row = Assert.Single(viewModel.CurrentIniDraftRows);
        row.AllowedValuesText = "light|Light armor;heavy|Heavy armor;wood|Wood armor";

        viewModel.BuildApplyPlan();

        Assert.NotEmpty(viewModel.ApplyPlanItems);
        Assert.DoesNotContain(viewModel.Issues, issue => issue.Severity == "Error");
        Assert.Contains("应用计划已构建", viewModel.ApplyStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildApplyPlan_CurrentIniGeneralizedTechnoRowStaysGeneralized()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();
        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [InfantryTypes]
            0=E1

            [VehicleTypes]
            0=HTNK

            [AircraftTypes]
            0=ORCA

            [BuildingTypes]
            0=GAPILE

            [E1]
            Armor=flak

            [HTNK]
            Armor=heavy

            [ORCA]
            Armor=light

            [GAPILE]
            Armor=wood
            """);

        viewModel.BuildApplyPlan();

        FieldRegistryApplyPlanItemViewModel item = Assert.Single(viewModel.ApplyPlanItems);
        Assert.Equal("Armor", item.Key);
        Assert.Equal("Techno", item.AppliesTo);
        Assert.Single(viewModel.Definitions);
        Assert.DoesNotContain(viewModel.Definitions, row => row.AppliesTo is "Infantry" or "Vehicle" or "Aircraft" or "Building");
    }

    [Fact]
    public void Clear_RemovesCurrentIniDraftRows()
    {
        FieldRegistryHarvestPreviewViewModel viewModel = new();
        viewModel.LoadCurrentIniHarvestPreview("rulesmd.ini", """
            [E1]
            CustomHarvestFlag=yes
            """);

        viewModel.Clear();

        Assert.Empty(viewModel.CurrentIniDraftRows);
        Assert.False(viewModel.HasCurrentIniDraftRows);
        Assert.Contains("尚未加载当前 INI", viewModel.CurrentIniHarvestStatusText, StringComparison.Ordinal);
    }
}
