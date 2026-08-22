using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryHarvestPreviewBoundaryTests
{
    [Fact]
    public void HarvestPreviewUi_DoesNotContainForbiddenNetworkSaveEditOrRollbackEntrypoints()
    {
        string root = TestRepositoryRoot.Find();
        string[] files =
        [
            Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldRegistryHarvestPreviewViewModel.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"),
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml.cs")
        ];
        string text = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

        Assert.DoesNotContain("HttpClient", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WebRequest", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FileStream", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ApplyAsync", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RollbackCommand", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged=\"Parse", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged=\"Apply", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ApplyDisabledReason", text, StringComparison.Ordinal);
        Assert.Contains("IFieldRegistryApplyWriter", text, StringComparison.Ordinal);
        Assert.Contains("ApplyConfirmed", text, StringComparison.Ordinal);
        Assert.Contains("LastApplyTargetFilePath", text, StringComparison.Ordinal);
        Assert.Contains("LastApplyBackupManifestPath", text, StringComparison.Ordinal);
        Assert.Contains("FetchRawTextAsync", text, StringComparison.Ordinal);
        Assert.Contains("FetchUrl", text, StringComparison.Ordinal);
        Assert.Contains("UsePresetUrl", text, StringComparison.Ordinal);
        Assert.Contains("FetchSelectedPreset", text, StringComparison.Ordinal);
        Assert.DoesNotContain("FetchAll", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AutoFetch", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AutoApply", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FieldRegistryManagerWindow_ExposesFieldImportPreviewButNoApplyCommand()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml"));

        Assert.Contains("打开字段导入预览", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("RollbackCommand", xaml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FieldRegistryFetch_IsOnlyWiredToExplicitPreviewButtonClick()
    {
        string root = TestRepositoryRoot.Find();
        string appCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "App.xaml.cs"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string managerCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml.cs"));
        string previewCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml.cs"));
        string previewXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        Assert.DoesNotContain("FetchRawTextAsync", appCode + shellCode + managerCode, StringComparison.Ordinal);
        Assert.Contains("Click=\"FetchRawText\"", previewXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"UsePresetUrl\"", previewXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"FetchSelectedPreset\"", previewXaml, StringComparison.Ordinal);
        Assert.Contains("FetchRawTextAsync", previewCode, StringComparison.Ordinal);
        Assert.Contains("FetchSelectedPresetAsync", previewCode, StringComparison.Ordinal);
        Assert.DoesNotContain("TextChanged=\"Fetch", previewXaml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Loaded=\"Fetch", previewXaml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FetchSelectedPresetAsync", appCode + shellCode + managerCode, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryFetch_DoesNotUseCredentialsOrAutomaticApply()
    {
        string root = TestRepositoryRoot.Find();
        string fetchDirectory = Path.Combine(root, "RA2IniEditor.Infrastructure", "FieldRegistry", "Fetch");
        string previewCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldRegistryHarvestPreviewViewModel.cs"));
        string fetchText = string.Join(Environment.NewLine, Directory.GetFiles(fetchDirectory, "*.cs").Select(File.ReadAllText));

        Assert.DoesNotContain("OAuth", fetchText + previewCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Credential", fetchText + previewCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", fetchText + previewCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ApplyConfirmed", fetchText, StringComparison.Ordinal);
        Assert.DoesNotContain("IFieldRegistryApplyWriter", fetchText, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryHarvestPreviewWindow_UsesSimplifiedChineseLabels()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        Assert.Contains("Title=\"字段库导入预览\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"插入示例\"", xaml, StringComparison.Ordinal);
        Assert.Contains("步骤 1：解析原始文本", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"获取原始文本\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.FetchUrlTextBox", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.FetchRawTextButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.FetchStatusText", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"使用当前 INI\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.UseCurrentIniButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.CurrentIniHarvestStatusText", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.CurrentIniDraftsGrid", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.RemoteHistoryGrid", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.RefreshRemoteHistoryButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.UseCachedTextButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.RefetchSelectedButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.ClearRemoteHistoryButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.RemoteHistoryStatusText", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"远程预设\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.RemotePresetsGrid", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.UsePresetUrlButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.FetchSelectedPresetButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.AddPresetButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.EditPresetButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.RemovePresetButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.ImportPresetsButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.ExportPresetsButton", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.RemotePresetStatusText", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.ApplyDisabledReasonText", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"构建应用计划\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"应用计划\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"当前 INI 草稿\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"已解析字段\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"验证问题\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"字段草稿 JSON\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"解析警告\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"预览结果\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"已有范围\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"已有来源\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"已有来源类型\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryHarvestPreviewWindow_DoesNotContainKnownEnglishActionLabels()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));
        string[] forbiddenLabels =
        [
            "Field Registry Import Preview",
            "Parse &amp; Preview",
            "Fetch Raw Text",
            "Build Apply Plan",
            "Apply Plan",
            "Source Name",
            "Paste Field Documentation",
            "Remote History",
            "Preview Diff",
            "Validation Issues"
        ];

        foreach (string label in forbiddenLabels)
            Assert.DoesNotContain(label, xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryHarvestPreviewWindow_HasReadableInputPreviewApplySections()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        Assert.Contains("Text=\"粘贴字段文档或 INI 风格文本\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"应用计划\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.PreviewDiffTab", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.ApplyPlanTab", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.WorkflowStepStrip", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.SourceArea", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.PlanArea", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"1  来源\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"2  审阅\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"3  计划\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"4  确认 / 结果\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryHarvestPreviewWindow_MainTabsHaveStarSizedRegion()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        Assert.Contains("<RowDefinition Height=\"*\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.MainFlowTabs\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Grid.Row=\"4\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<RowDefinition Height=\"230\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("MaxHeight=\"180\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryHarvestPreviewWindow_ApplyDetailsDoNotSitInAutoRowAboveTabs()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));
        string compactApplyArea = SliceBetween(
            xaml,
            "<Border Grid.Row=\"3\"",
            "<Grid Grid.Row=\"4\"");
        string applyPlanTab = SliceBetween(
            xaml,
            "AutomationProperties.AutomationId=\"FieldImportPreview.ApplyPlanTab\"",
            "AutomationProperties.AutomationId=\"FieldImportPreview.IssuesWarningsTab\"");

        Assert.Contains("FieldImportPreview.TargetFilePreviewText", compactApplyArea, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.ApplySummaryText", compactApplyArea, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldImportPreview.ApplyStatusText", compactApplyArea, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldImportPreview.LastApplyTargetPathText", compactApplyArea, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.ApplyStatusText", applyPlanTab, StringComparison.Ordinal);
        Assert.Contains("FieldImportPreview.LastApplyTargetPathText", applyPlanTab, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryHarvestPreviewWindow_HasMainFlowTabsAndAdvancedDetails()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));

        Assert.Contains("Header=\"预览结果\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"应用计划\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"问题与警告\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.AdvancedDetailsExpander\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"显示高级详情\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsExpanded=\"False\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"FieldImportPreview.AdvancedDetailsTabs\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryHarvestPreviewWindow_DoesNotExposeDebugTabsAsPrimaryFlow()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryHarvestPreviewWindow.xaml"));
        string mainFlowTabs = SliceBetween(
            xaml,
            "AutomationProperties.AutomationId=\"FieldImportPreview.MainFlowTabs\"",
            "AutomationProperties.AutomationId=\"FieldImportPreview.AdvancedDetailsExpander\"");

        Assert.DoesNotContain("FieldImportPreview.RemoteHistoryTab", mainFlowTabs, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldImportPreview.RemotePresetsTab", mainFlowTabs, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldImportPreview.CurrentIniDraftsTab", mainFlowTabs, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldImportPreview.ParsedFieldsTab", mainFlowTabs, StringComparison.Ordinal);
        Assert.DoesNotContain("FieldImportPreview.FieldDraftsTab", mainFlowTabs, StringComparison.Ordinal);
    }

    private static string SliceBetween(string text, string start, string end)
    {
        int startIndex = text.IndexOf(start, StringComparison.Ordinal);
        int endIndex = text.IndexOf(end, startIndex + start.Length, StringComparison.Ordinal);
        Assert.True(startIndex >= 0, $"Could not find start marker: {start}");
        Assert.True(endIndex > startIndex, $"Could not find end marker: {end}");
        return text[startIndex..endIndex];
    }
}

