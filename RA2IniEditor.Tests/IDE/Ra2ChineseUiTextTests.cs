using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ChineseUiTextTests
{
    [Fact]
    public void AddPropertyWindow_UsesChineseUserFacingLabels()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "FieldBrowser",
            "Ra2AddPropertyWindow.xaml"));
        string viewModel = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "ViewModels",
            "FieldBrowser",
            "Ra2AddPropertyViewModel.cs"));

        Assert.Contains("AddProperty.Window", xaml);
        Assert.Contains("AddProperty.SearchTextBox", xaml);
        Assert.Contains("AddProperty.FieldsGrid", xaml);
        Assert.Contains("AddProperty.AddSelectedButton", xaml);
        Assert.Contains("AddProperty.CancelButton", xaml);
        Assert.Contains("AddProperty.EditAnnotationButton", xaml);
        Assert.Contains("SelectedItem", viewModel);
        Assert.Contains("FilteredItems", viewModel);
        Assert.Contains("SearchModeOptions", viewModel);
    }

    [Fact]
    public void ShellWindow_UsesChineseMainPathLabels()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("Header=\"打开项目...\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"进入编辑模式\"", xaml);
        Assert.Contains("ToolTip=\"进入编辑模式\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"放弃修改\"", xaml);
        Assert.Contains("ToolTip=\"放弃当前未保存修改\"", xaml);
        Assert.Contains("Header=\"添加属性...\"", xaml);
        Assert.Contains("Header=\"转到定义\"", xaml);
        Assert.Contains("Header=\"查看定义\"", xaml);
        Assert.Contains("Header=\"查找当前文件引用\"", xaml);
    }

    [Fact]
    public void ShellWindow_UsesReadableSimplifiedChineseForBottomToolTabsAndIssues()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string workspaceStyles = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeWorkspaceStyles.xaml"));

        Assert.Contains("Header=\"问题\"", xaml);
        Assert.Contains("Header=\"输出\"", xaml);
        Assert.Contains("Header=\"查找\"", xaml);
        Assert.Contains("Content=\"刷新当前\"", xaml);
        Assert.Contains("Content=\"全量诊断\"", xaml);
        Assert.Contains("Content=\"清空\"", xaml);
        Assert.DoesNotContain("Content=\"清除筛选\"", xaml);
        Assert.DoesNotContain("Shell.BottomIssues.SearchTextBox", xaml);
        Assert.Contains("CellTemplate=\"{StaticResource IdeIssueSeverityIconTemplate}\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding SeverityText}\"", workspaceStyles);
        Assert.Contains("IconGeometry.Issue.Error", workspaceStyles);
        Assert.Contains("IconGeometry.Issue.Warning", workspaceStyles);
        Assert.Contains("IconGeometry.Issue.Info", workspaceStyles);
        Assert.Contains("Header=\"位置\"", xaml);
        Assert.Contains("Header=\"代码\"", xaml);
        Assert.Contains("Header=\"消息\"", xaml);
        Assert.Contains("Header=\"来源\"", xaml);
        Assert.DoesNotContain("Header=\"错误列表\"", xaml);
        AssertDoesNotContainMojibake(xaml);
    }

    [Fact]
    public void IssuesAndSavePreflightWindows_UseReadableSimplifiedChineseText()
    {
        string root = TestRepositoryRoot.Find();
        string issuesXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "IssuesToolWindow.xaml"));
        string workspaceStyles = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeWorkspaceStyles.xaml"));
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string searchXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "SearchToolView.xaml"));
        string savePreflightXaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "SavePreflight",
            "SavePreflightConfirmationDialog.xaml"));

        Assert.Contains("Title=\"问题\"", issuesXaml);
        Assert.Contains("Content=\"刷新当前\"", issuesXaml);
        Assert.Contains("Content=\"全量诊断\"", issuesXaml);
        Assert.Contains("Content=\"清空\"", issuesXaml);
        Assert.Contains("CellTemplate=\"{StaticResource IdeIssueSeverityIconTemplate}\"", issuesXaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding SeverityText}\"", workspaceStyles);
        Assert.Contains("Header=\"来源\"", issuesXaml);
        Assert.Contains("ContentId=\"Tool.Search\"", shellXaml);
        Assert.Contains("Title=\"查找\"", shellXaml);
        Assert.Contains("AutomationProperties.Name=\"查找内容\"", searchXaml);
        Assert.Contains("Content=\"区分大小写\"", searchXaml);
        Assert.Contains("Content=\"全字匹配\"", searchXaml);
        Assert.Contains("Content=\"正则表达式\"", searchXaml);
        Assert.Contains("Text=\"查找范围\"", searchXaml);
        Assert.Contains("Text=\"文件类型\"", searchXaml);
        Assert.Contains("AutomationProperties.Name=\"替换为\"", searchXaml);
        Assert.Contains("Content=\"预览\"", searchXaml);
        Assert.Contains("Content=\"应用\"", searchXaml);
        Assert.Contains("Content=\"上一个\"", searchXaml);
        Assert.Contains("Content=\"下一个\"", searchXaml);
        Assert.Contains("Content=\"查找全部\"", searchXaml);
        Assert.DoesNotContain("mock", searchXaml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("保存前发现可能问题", savePreflightXaml);
        Assert.Contains("不会阻止保存", savePreflightXaml);
        Assert.Contains("取消，返回编辑", savePreflightXaml);
        AssertDoesNotContainMojibake(issuesXaml);
        AssertDoesNotContainMojibake(searchXaml);
        AssertDoesNotContainMojibake(savePreflightXaml);
    }

    private static void AssertDoesNotContainMojibake(string text)
    {
        string[] mojibakeFragments =
        [
            "閿", "欒", "鎼", "滅", "储", "銆", "佹", "秷", "鎭", "缃", "娓", "杈", "撳", "嚭", "闂", "", "", "婧", "鍏ㄩ", "璇婃"
        ];

        foreach (string fragment in mojibakeFragments)
            Assert.DoesNotContain(fragment, text, StringComparison.Ordinal);
    }
}

