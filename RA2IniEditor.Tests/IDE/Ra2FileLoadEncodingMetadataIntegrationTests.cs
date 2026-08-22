using System.Text;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FileLoadEncodingMetadataIntegrationTests
{
    [Fact]
    public async Task LoadProjectExplorerFileAsync_CarriesReadEncodingIntoCurrentSnapshot()
    {
        string root = CreateTempProjectWithIni();
        try
        {
            RecordingIniFileStore fileStore = new("[Rules]\r\nName=Rules", Encoding.Unicode, "\r\n");
            ShellViewModel viewModel = new(
                new ProjectOpenService(),
                new ReadonlyIniContentService(fileStore));

            await viewModel.OpenProjectFolderAsync(root);
            ProjectExplorerItemViewModel fileNode = Assert.Single(viewModel.ProjectExplorer.Items);

            await viewModel.LoadProjectExplorerFileAsync(fileNode);

            Assert.NotNull(viewModel.CurrentSnapshot);
            Assert.Equal(Ra2EditorTextEncodingKind.Utf16Le, viewModel.CurrentSnapshot!.EncodingMetadata.Kind);
            Assert.Contains("UTF-16 LE", viewModel.SourceEditor.MetadataText);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempProjectWithIni()
    {
        string root = Path.Combine(Path.GetTempPath(), "RA2IniEditorEncodingTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "rules.ini"), "[Rules]\r\nName=Rules", new UTF8Encoding(false));
        return root;
    }

    private sealed class RecordingIniFileStore : IIniFileStore
    {
        private readonly Encoding _encoding;
        private readonly string _newLine;
        private readonly string _text;

        public RecordingIniFileStore(string text, Encoding encoding, string newLine)
        {
            _text = text;
            _encoding = encoding;
            _newLine = newLine;
        }

        public IniTextReadResult ReadText(string path)
            => new(path, _text, _encoding, _newLine);

        public IniTextWriteResult WriteText(string path, string text, Encoding encoding)
            => throw new NotSupportedException("File load encoding metadata integration tests must not write text.");
    }
}
