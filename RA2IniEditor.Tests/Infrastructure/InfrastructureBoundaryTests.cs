using System.Xml.Linq;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class InfrastructureBoundaryTests
{
    private static readonly string[] ForbiddenInfrastructureText =
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
    public void InfrastructureProject_ReferencesCoreOnly()
    {
        string root = TestRepositoryRoot.Find();
        string infrastructureProjectPath = Path.Combine(root, "RA2IniEditor.Infrastructure", "RA2IniEditor.Infrastructure.csproj");
        XDocument project = XDocument.Load(infrastructureProjectPath);
        string projectText = File.ReadAllText(infrastructureProjectPath);

        Assert.Contains("<TargetFramework>net8.0</TargetFramework>", projectText);
        Assert.Contains("..\\RA2IniEditor.Core\\RA2IniEditor.Core.csproj", projectText);
        Assert.DoesNotContain("RA2IniEditor.csproj", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RA2IniEditor.Tests.csproj", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<UseWPF>true</UseWPF>", projectText, StringComparison.OrdinalIgnoreCase);

        IEnumerable<XElement> projectReferences = project.Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference");
        XElement reference = Assert.Single(projectReferences);
        Assert.Equal("..\\RA2IniEditor.Core\\RA2IniEditor.Core.csproj", reference.Attribute("Include")?.Value);

        foreach (string forbidden in ForbiddenInfrastructureText)
            Assert.DoesNotContain(forbidden, projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InfrastructureSource_DoesNotUseWpfTypes()
    {
        string root = TestRepositoryRoot.Find();
        string infrastructureDirectory = Path.Combine(root, "RA2IniEditor.Infrastructure");
        string[] sourceFiles = Directory.GetFiles(infrastructureDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(sourceFiles);
        foreach (string sourceFile in sourceFiles)
        {
            string source = File.ReadAllText(sourceFile);
            foreach (string forbidden in ForbiddenInfrastructureText)
                Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ProjectReferences_PointToInfrastructureWithoutReverseLegacyReference()
    {
        string root = TestRepositoryRoot.Find();
        string legacyProjectPath = Path.Combine(root, "RA2IniEditor.csproj");
        string testsProjectPath = Path.Combine(root, "RA2IniEditor.Tests", "RA2IniEditor.Tests.csproj");
        string infrastructureProjectPath = Path.Combine(root, "RA2IniEditor.Infrastructure", "RA2IniEditor.Infrastructure.csproj");

        string testsProjectText = File.ReadAllText(testsProjectPath);
        string infrastructureProjectText = File.ReadAllText(infrastructureProjectPath);

        if (File.Exists(legacyProjectPath))
        {
            string legacyProjectText = File.ReadAllText(legacyProjectPath);
            Assert.Contains("RA2IniEditor.Infrastructure\\RA2IniEditor.Infrastructure.csproj", legacyProjectText);
            Assert.Contains("Compile Remove=\"RA2IniEditor.Infrastructure\\**\\*.cs\"", legacyProjectText);
        }
        else
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
        }

        Assert.Contains("..\\RA2IniEditor.Infrastructure\\RA2IniEditor.Infrastructure.csproj", testsProjectText);
        Assert.DoesNotContain("RA2IniEditor.csproj", infrastructureProjectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RA2IniEditor.Tests.csproj", infrastructureProjectText, StringComparison.OrdinalIgnoreCase);
    }
}

