using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IconResourceBoundaryTests
{
    [Fact]
    public void IconResources_DefinesBrushTokensAndMainToolbarVectorPresenters()
    {
        string root = TestRepositoryRoot.Find();
        string iconResourcesPath = Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IconResources.xaml");

        Assert.True(File.Exists(iconResourcesPath), "IconResources.xaml should exist in the approved Themes location.");

        string iconResources = File.ReadAllText(iconResourcesPath);

        string[] requiredBrushTokens =
        [
            "IconBrush.Normal",
            "IconBrush.Muted",
            "IconBrush.Disabled",
            "IconBrush.Warning",
            "IconBrush.Error",
            "IconBrush.Success",
            "IconBrush.Accent",
            "IconBrush.Project",
            "IconBrush.Global",
            "IconBrush.BuiltIn"
        ];

        foreach (string token in requiredBrushTokens)
            Assert.Contains($"x:Key=\"{token}\"", iconResources, StringComparison.Ordinal);

        Assert.Contains("x:Key=\"IconSampleCheck\"", iconResources, StringComparison.Ordinal);
        Assert.Contains("<Path", iconResources, StringComparison.Ordinal);
        Assert.Contains("Stroke=\"{DynamicResource IconBrush.Success}\"", iconResources, StringComparison.Ordinal);

        string[] toolbarIconKeys =
        [
            "IconOpenFolder",
            "IconSave",
            "IconUndo",
            "IconRedo",
            "IconRevert",
            "IconEditMode",
            "IconSearch",
            "IconFieldRegistry",
            "IconIssues",
            "IconProjectExplorer"
        ];

        foreach (string key in toolbarIconKeys)
        {
            string iconBlock = ExtractResourceBlock(iconResources, key);
            Assert.Contains($"<Viewbox x:Key=\"{key}\"", iconBlock, StringComparison.Ordinal);
            Assert.Contains("x:Shared=\"False\"", iconBlock, StringComparison.Ordinal);
            Assert.Contains("Width=\"16\"", iconBlock, StringComparison.Ordinal);
            Assert.Contains("Height=\"16\"", iconBlock, StringComparison.Ordinal);
            Assert.Contains("<Path", iconBlock, StringComparison.Ordinal);
            Assert.Contains("IconBrush.", iconBlock, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("Text=\"O\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"S\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"U\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"R\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"X\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"F\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"D\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"!\"", iconResources, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"P\"", iconResources, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IconEditMode\"", iconResources, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"IconRevert\"", iconResources, StringComparison.Ordinal);
        Assert.Contains("IconBrush.Muted", ExtractResourceBlock(iconResources, "IconRevert"), StringComparison.Ordinal);
        Assert.Contains("IconBrush.Warning", ExtractResourceBlock(iconResources, "IconIssues"), StringComparison.Ordinal);
    }

    [Fact]
    public void IconResources_AreMergedWithoutLegacyIdsOrRuntimeBitmapDependencies()
    {
        string root = TestRepositoryRoot.Find();
        string appXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "App.xaml"));
        string iconResources = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IconResources.xaml"));
        string shellTheme = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "ShellTheme.xaml"));
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("<ResourceDictionary Source=\"Themes/ShellTheme.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.Contains("<ResourceDictionary Source=\"Themes/IconResources.xaml\" />", appXaml, StringComparison.Ordinal);
        Assert.True(
            appXaml.IndexOf("Themes/ShellTheme.xaml", StringComparison.Ordinal) <
            appXaml.IndexOf("Themes/IconResources.xaml", StringComparison.Ordinal),
            "IconResources.xaml should be merged after ShellTheme.xaml so scaffold brushes can coexist with shell resources.");

        string[] approvedToolbarIds =
        [
            "Shell.MainToolbar.OpenFolderButton",
            "Shell.SourceEditor.SaveCurrentFileButton",
            "Shell.SourceEditor.UndoButton",
            "Shell.SourceEditor.RedoButton",
            "Shell.SourceEditor.RevertInMemoryChangesButton",
            "Shell.SourceEditor.EnterEditModeButton",
            "Shell.MainToolbar.SearchButton",
            "Shell.MainToolbar.FieldRegistryButton",
            "Shell.MainToolbar.IssuesButton",
            "Shell.MainToolbar.ProjectExplorerButton"
        ];

        foreach (string id in approvedToolbarIds)
            Assert.Contains(id, shellXaml, StringComparison.Ordinal);

        Assert.DoesNotContain("AutomationProperties.AutomationId=\"Shell.FieldRegistryButton\"", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBlock x:Key=\"IconOpenFolder\" x:Shared=\"False\" Text=\"O\" />", shellTheme, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBlock x:Key=\"IconSave\" x:Shared=\"False\" Text=\"S\" />", shellTheme, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBlock x:Key=\"IconIssues\" x:Shared=\"False\" Text=\"!\" />", shellTheme, StringComparison.Ordinal);
        Assert.DoesNotContain(".png", iconResources + appXaml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".svg", iconResources + appXaml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AiAssistant.ApplyButton", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("AiAssistant.InsertButton", shellXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void MainToolbarVectorReplacement_PreservesHandlersAndMenuEntries()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        string[] requiredHandlers =
        [
            "Click=\"OpenProjectFolder\"",
            "Click=\"SaveCurrentFile_OnClick\"",
            "Click=\"UndoCurrentFile_OnClick\"",
            "Click=\"RedoCurrentFile_OnClick\"",
            "Click=\"RevertInMemoryChanges_OnClick\"",
            "Click=\"EnterEditMode_OnClick\"",
            "Click=\"OpenSearchToolWindow\"",
            "Click=\"OpenFieldRegistryManagerWindow\"",
            "Click=\"FocusIssuesToolTab\"",
            "Click=\"ToggleProjectExplorer\""
        ];

        foreach (string handler in requiredHandlers)
            Assert.Contains(handler, shellXaml, StringComparison.Ordinal);

        string[] requiredMenuEntries =
        [
            "Shell.Menu.OpenFolder",
            "Shell.Menu.Search",
            "Shell.Menu.FieldRegistryCenter",
            "Shell.Menu.ToggleProjectExplorer",
            "Shell.BottomToolTabs.ErrorList"
        ];

        foreach (string menuEntry in requiredMenuEntries)
            Assert.Contains(menuEntry, shellXaml, StringComparison.Ordinal);
    }


    [Fact]
    public void IconSetPolish_DefinesExplorerFactionAndAiActionVectorResources()
    {
        string root = TestRepositoryRoot.Find();
        string geometryResourcesPath = Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IconGeometryResources.xaml");
        string imageResourcesPath = Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IconImageResources.xaml");
        string appXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "App.xaml"));
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.True(File.Exists(geometryResourcesPath), "IconGeometryResources.xaml should hold compact vector geometry.");
        Assert.True(File.Exists(imageResourcesPath), "IconImageResources.xaml should hold DrawingImage wrappers.");

        string geometryResources = File.ReadAllText(geometryResourcesPath);
        string imageResources = File.ReadAllText(imageResourcesPath);

        string[] requiredIconKeys =
        [
            "FileIni",
            "Registry",
            "Section",
            "Infantry",
            "Vehicle",
            "Aircraft",
            "Building",
            "Weapon",
            "Warhead",
            "Projectile",
            "Country",
            "Country.Custom",
            "Country.Unknown",
            "Country.Common",
            "Country.Allied",
            "Country.Soviet",
            "Country.Yuri",
            "Side",
            "Side.Custom",
            "Tab.Section",
            "Tab.AI",
            "Action.Send",
            "Action.Cancel",
            "Action.Advanced",
            "Action.Clear"
        ];

        foreach (string key in requiredIconKeys)
        {
            Assert.Contains($"x:Key=\"IconGeometry.{key}\"", geometryResources, StringComparison.Ordinal);
            Assert.Contains($"x:Key=\"Icon.{key}\"", imageResources, StringComparison.Ordinal);
        }

        Assert.Contains("Themes/IconGeometryResources.xaml", appXaml, StringComparison.Ordinal);
        Assert.Contains("Themes/IconImageResources.xaml", appXaml, StringComparison.Ordinal);
        Assert.Contains("IconKeyToDrawingImageConverter", appXaml, StringComparison.Ordinal);
        Assert.Contains("Source=\"{Binding IconKey, Converter={StaticResource IconKeyToDrawingImageConverter}}\"", shellXaml, StringComparison.Ordinal);
        Assert.Contains("IconGeometry.Action.Send", shellXaml, StringComparison.Ordinal);
        Assert.Contains("IconGeometry.Action.Cancel", shellXaml, StringComparison.Ordinal);
        Assert.Contains("IconGeometry.Action.Advanced", shellXaml, StringComparison.Ordinal);
        Assert.Contains("IconGeometry.Action.Clear", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Icon.Action.Send", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Icon.Action.Cancel", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Icon.Action.Advanced", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Icon.Action.Clear", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"◇\"", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"▤\"", shellXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldRegistryR2Foundation_DefinesApprovedVectorGeometryWithoutRasterAssets()
    {
        string root = TestRepositoryRoot.Find();
        string geometryResourcesPath = Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Themes",
            "IconGeometryResources.xaml");
        string geometryResources = File.ReadAllText(geometryResourcesPath);

        string[] requiredKeys =
        [
            "Search",
            "Filter",
            "Refresh",
            "Add",
            "Edit",
            "Learn",
            "Project",
            "Global",
            "BuiltIn",
            "Import",
            "History",
            "Rollback",
            "Copy"
        ];

        foreach (string key in requiredKeys)
            Assert.Contains(
                $"x:Key=\"IconGeometry.FieldRegistry.{key}\"",
                geometryResources,
                StringComparison.Ordinal);

        Assert.DoesNotContain(".png", geometryResources, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".jpg", geometryResources, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".svg", geometryResources, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractResourceBlock(string resources, string key)
    {
        int start = resources.IndexOf($"x:Key=\"{key}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Expected resource key {key}.");

        int viewboxStart = resources.LastIndexOf("<Viewbox", start, StringComparison.Ordinal);
        Assert.True(viewboxStart >= 0, $"Expected resource key {key} to be a Viewbox presenter.");

        int end = resources.IndexOf("</Viewbox>", start, StringComparison.Ordinal);
        Assert.True(end > start, $"Expected resource key {key} to close its Viewbox presenter.");

        return resources[viewboxStart..(end + "</Viewbox>".Length)];
    }
}
