using System.Xml.Linq;
using Xunit;

namespace RA2IniEditor.Tests.Core;

public sealed class CoreBoundaryTests
{
    private static readonly string[] ForbiddenCoreText =
    {
        "System.Windows",
        "System.Windows.Input",
        "System.Windows.Threading",
        "PresentationCore",
        "PresentationFramework",
        "ICommand",
        "Dispatcher",
        "Clipboard",
        "Window",
        "Application.Current"
    };

    [Fact]
    public void CoreProject_DoesNotReferenceWpfOrLegacyProject()
    {
        string root = TestRepositoryRoot.Find();
        string coreProjectPath = Path.Combine(root, "RA2IniEditor.Core", "RA2IniEditor.Core.csproj");
        XDocument project = XDocument.Load(coreProjectPath);
        string projectText = File.ReadAllText(coreProjectPath);

        Assert.Contains("<TargetFramework>net8.0</TargetFramework>", projectText);
        Assert.DoesNotContain("<UseWPF>true</UseWPF>", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(project.Descendants(), element => element.Name.LocalName == "ProjectReference");
        Assert.DoesNotContain("RA2IniEditor.csproj", projectText, StringComparison.OrdinalIgnoreCase);

        foreach (string forbidden in ForbiddenCoreText)
            Assert.DoesNotContain(forbidden, projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CoreSource_DoesNotUseWpfTypes()
    {
        string root = TestRepositoryRoot.Find();
        string coreDirectory = Path.Combine(root, "RA2IniEditor.Core");
        string[] sourceFiles = Directory.GetFiles(coreDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(sourceFiles);
        foreach (string sourceFile in sourceFiles)
        {
            string source = File.ReadAllText(sourceFile);
            foreach (string forbidden in ForbiddenCoreText)
                Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ProjectReferences_PointFromLegacyAndTestsToCoreOnly()
    {
        string root = TestRepositoryRoot.Find();
        string legacyProjectPath = Path.Combine(root, "RA2IniEditor.csproj");
        string testsProjectPath = Path.Combine(root, "RA2IniEditor.Tests", "RA2IniEditor.Tests.csproj");
        string coreProjectPath = Path.Combine(root, "RA2IniEditor.Core", "RA2IniEditor.Core.csproj");

        string testsProjectText = File.ReadAllText(testsProjectPath);
        string coreProjectText = File.ReadAllText(coreProjectPath);

        if (File.Exists(legacyProjectPath))
        {
            string legacyProjectText = File.ReadAllText(legacyProjectPath);
            Assert.Contains("RA2IniEditor.Core\\RA2IniEditor.Core.csproj", legacyProjectText);
            Assert.Contains("Compile Remove=\"RA2IniEditor.Core\\**\\*.cs\"", legacyProjectText);
        }
        else
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
        }

        Assert.Contains("..\\RA2IniEditor.Core\\RA2IniEditor.Core.csproj", testsProjectText);
        Assert.DoesNotContain("RA2IniEditor.csproj", coreProjectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RA2IniEditor.Tests.csproj", coreProjectText, StringComparison.OrdinalIgnoreCase);
    }
}

