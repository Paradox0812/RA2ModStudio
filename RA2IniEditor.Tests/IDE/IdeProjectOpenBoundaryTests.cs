using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IdeProjectOpenBoundaryTests
{
    [Fact]
    public void OpenFolderReadonly_EnumeratesOnlyRootIniDescriptorsAndKeepsPreferredOrder()
    {
        string root = CreateTempDirectory();
        try
        {
            string subdirectory = Path.Combine(root, "Sub");
            Directory.CreateDirectory(subdirectory);
            File.WriteAllText(Path.Combine(root, "zcustom.ini"), "[Z]\nName=Custom");
            File.WriteAllText(Path.Combine(root, "art.ini"), "[ART]\nName=Art");
            File.WriteAllText(Path.Combine(root, "rules.ini"), "[Rules]\nName=Rules");
            File.WriteAllText(Path.Combine(root, "notes.txt"), "ignored");
            File.WriteAllText(Path.Combine(subdirectory, "ai.ini"), "[AI]\nName=Nested");

            ProjectOpenService service = new();

            ProjectOpenResult result = service.OpenFolderReadonly(root);

            IReadOnlyList<ReadonlyIniFileDescriptor> files = result.Files;
            Assert.Equal(root, result.ProjectFolderPath);
            Assert.Equal(["rules.ini", "art.ini", "zcustom.ini"], files.Select(file => file.FileName).ToArray());
            Assert.All(files, file => Assert.True(file.FileSizeBytes > 0));
            Assert.DoesNotContain(files, file => file.FilePath.Contains($"{Path.DirectorySeparatorChar}Sub{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void OpenFolderReadonly_DoesNotReadIniTextDuringProjectOpen()
    {
        string root = CreateTempDirectory();
        try
        {
            string filePath = Path.Combine(root, "rules.ini");
            string sourceText = "[Rules]\r\nName=Rules";
            File.WriteAllText(filePath, sourceText);
            ProjectOpenService service = new();

            var file = Assert.Single(service.OpenFolderReadonly(root).Files);

            Assert.Equal("rules.ini", file.FileName);
            Assert.Equal(filePath, file.FilePath);
            Assert.Equal(new FileInfo(filePath).Length, file.FileSizeBytes);
            Assert.DoesNotContain("Name=Rules", file.GetType().GetProperties().Select(property => property.Name));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void OpenFolderReadonly_WhenDirectoryHasNoIniFiles_ReturnsEmptyResult()
    {
        string root = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(root, "notes.txt"), "ignored");
            ProjectOpenService service = new();

            ProjectOpenResult result = service.OpenFolderReadonly(root);

            Assert.True(result.IsEmpty);
            Assert.Equal(0, result.TotalIniFileCount);
            Assert.Empty(result.Files);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "RA2IniEditor_IDE_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
