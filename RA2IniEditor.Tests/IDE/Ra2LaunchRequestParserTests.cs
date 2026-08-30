using RA2IniEditor.IDE.Startup;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2LaunchRequestParserTests
{
    [Fact]
    public void Parse_WithoutArguments_ReturnsNoLaunchTarget()
    {
        Ra2LaunchRequest request = Ra2LaunchRequestParser.Parse([]);

        Assert.Equal(Ra2LaunchTargetKind.None, request.Kind);
        Assert.Null(request.ProjectFolderPath);
        Assert.Null(request.TargetFilePath);
    }

    [Fact]
    public void Parse_RawIniPath_UsesDirectParentAsProjectAndPreservesExactTarget()
    {
        using TempProject project = TempProject.Create();
        string nestedFolder = Path.Combine(project.RootPath, "nested");
        Directory.CreateDirectory(nestedFolder);
        string targetPath = Path.Combine(nestedFolder, "自定义 Art.INI");
        File.WriteAllText(targetPath, "[HTNKART]\r\nImage=HTNKBODY");

        Ra2LaunchRequest request = Ra2LaunchRequestParser.Parse([$"\"{targetPath}\""]);

        Assert.Equal(Ra2LaunchTargetKind.IniFile, request.Kind);
        Assert.Equal(Path.GetFullPath(nestedFolder), request.ProjectFolderPath);
        Assert.Equal(Path.GetFullPath(targetPath), request.TargetFilePath);
    }

    [Fact]
    public void Parse_AutomationFolderOption_KeepsExistingCompatibility()
    {
        using TempProject project = TempProject.Create();

        Ra2LaunchRequest request = Ra2LaunchRequestParser.Parse([
            Ra2LaunchRequestParser.AutomationOpenFolderArgument,
            project.RootPath]);

        Assert.Equal(Ra2LaunchTargetKind.ProjectFolder, request.Kind);
        Assert.Equal(Path.GetFullPath(project.RootPath), request.ProjectFolderPath);
        Assert.Null(request.TargetFilePath);
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("")]
    public void Parse_UnsupportedSingleArgument_ReturnsInvalid(string argument)
    {
        Ra2LaunchRequest request = Ra2LaunchRequestParser.Parse([argument]);

        Assert.Equal(Ra2LaunchTargetKind.Invalid, request.Kind);
        Assert.False(string.IsNullOrWhiteSpace(request.ErrorMessage));
    }

    [Fact]
    public void Parse_MissingOrNonIniFile_ReturnsInvalid()
    {
        using TempProject project = TempProject.Create();
        string textPath = Path.Combine(project.RootPath, "notes.txt");
        File.WriteAllText(textPath, "not ini");

        Assert.Equal(
            Ra2LaunchTargetKind.Invalid,
            Ra2LaunchRequestParser.Parse([textPath]).Kind);
        Assert.Equal(
            Ra2LaunchTargetKind.Invalid,
            Ra2LaunchRequestParser.Parse([Path.Combine(project.RootPath, "missing.ini")]).Kind);
    }

    [Fact]
    public void Parse_MultipleTargets_ReturnsInvalid()
    {
        Ra2LaunchRequest request = Ra2LaunchRequestParser.Parse(["one.ini", "two.ini", "three.ini"]);

        Assert.Equal(Ra2LaunchTargetKind.Invalid, request.Kind);
    }

    [Fact]
    public async Task ParsedIniTarget_CanOpenProjectAndLoadThatExactExplorerFile()
    {
        using TempProject project = TempProject.Create();
        project.AddIni("rules.ini", "[General]\r\nName=Rules");
        string targetPath = project.AddIni("art.ini", "[HTNKART]\r\nImage=HTNKBODY");
        Ra2LaunchRequest request = Ra2LaunchRequestParser.Parse([targetPath]);
        ShellViewModel viewModel = new();

        await viewModel.OpenProjectFolderAsync(request.ProjectFolderPath!);
        ProjectExplorerItemViewModel target = Assert.Single(
            viewModel.ProjectExplorer.Items,
            item => string.Equals(item.FilePath, request.TargetFilePath, StringComparison.OrdinalIgnoreCase));
        await viewModel.LoadProjectExplorerFileAsync(target);

        Assert.Equal(Path.GetFullPath(project.RootPath), viewModel.CurrentProjectRootPath);
        Assert.NotNull(viewModel.CurrentSnapshot);
        Assert.Equal(Path.GetFullPath(targetPath), viewModel.CurrentSnapshot!.FilePath);
        Assert.Contains("[HTNKART]", viewModel.CurrentSnapshot.Text, StringComparison.Ordinal);
        Assert.Same(target, viewModel.ProjectExplorer.SelectedItem);
    }

    [Fact]
    public void ShellLaunchBoundary_ReusesCanonicalProjectAndEditableSessionPipeline()
    {
        string root = TestRepositoryRoot.Find();
        string appCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "App.xaml.cs"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("Ra2LaunchRequestParser.Parse(e.Args)", appCode, StringComparison.Ordinal);
        Assert.Contains("OpenLaunchRequestAsync(launchRequest)", appCode, StringComparison.Ordinal);
        Assert.Contains("await _shellReady.Task", shellCode, StringComparison.Ordinal);
        AssertInOrder(
            shellCode,
            "await viewModel.OpenProjectFolderAsync(folderPath)",
            "InitializeProjectDocumentSessionStore(viewModel)",
            "await viewModel.LoadProjectExplorerFileAsync(",
            "StartEditableSessionForCurrentSnapshot(viewModel)");
        Assert.DoesNotContain("File.ReadAllText", appCode, StringComparison.Ordinal);
    }

    private static void AssertInOrder(string text, params string[] fragments)
    {
        int previous = -1;
        foreach (string fragment in fragments)
        {
            int current = text.IndexOf(fragment, previous + 1, StringComparison.Ordinal);
            Assert.True(current > previous, $"Expected '{fragment}' after the previous launch step.");
            previous = current;
        }
    }

    private sealed class TempProject : IDisposable
    {
        private TempProject(string rootPath)
            => RootPath = rootPath;

        public string RootPath { get; }

        public static TempProject Create()
        {
            string rootPath = Path.Combine(
                Path.GetTempPath(),
                "RA2IniEditor_ShellLaunch_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(rootPath);
            return new TempProject(rootPath);
        }

        public string AddIni(string fileName, string text)
        {
            string filePath = Path.Combine(RootPath, fileName);
            File.WriteAllText(filePath, text);
            return filePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
    }
}
