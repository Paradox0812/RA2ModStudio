using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveCurrentFileUiIntegrationTests
{
    private static readonly DateTime FixedTimestamp = new(2026, 5, 28, 23, 50, 0);

    [Fact]
    public void ShellWindow_ExposesSaveButtonAndCtrlSWithoutDirectFileWrites()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("Shell.SourceEditor.SaveCurrentFileButton", shellXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"SaveCurrentFile_OnClick\"", shellXaml, StringComparison.Ordinal);
        Assert.Contains("保存当前文件", shellXaml, StringComparison.Ordinal);
        Assert.Contains("IRa2SaveCurrentFileService", shellCode, StringComparison.Ordinal);
        Assert.Contains("_saveCurrentFileService.Save", shellCode, StringComparison.Ordinal);
        Assert.Contains("new Ra2SaveCurrentFilePlanRequest", shellCode, StringComparison.Ordinal);
        Assert.Contains("ApplicationCommands.Save", shellCode, StringComparison.Ordinal);
        Assert.Contains("new KeyGesture(Key.S, ModifierKeys.Control)", shellCode, StringComparison.Ordinal);
        Assert.Contains("IsSaveShortcut", shellCode, StringComparison.Ordinal);
        Assert.Contains("_editableSession = result.UpdatedSession", shellCode, StringComparison.Ordinal);
        Assert.Contains("UpdateEditorStateControls();", shellCode, StringComparison.Ordinal);
        Assert.Contains("ShowOutputMessage", shellCode, StringComparison.Ordinal);

        Assert.DoesNotContain("File.WriteAllText", shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("new Ra2BackupService", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("new Ra2TextFirstFileWriter", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveAll", shellCode, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Formatter_ShowsReadonlyPromptWhenLoadedFileHasNoEditableSession()
    {
        Ra2SaveCurrentFileService service = new();
        Ra2SaveCurrentFileUiMessageFormatter formatter = new();

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(null, isReadOnlyPreview: true),
            projectRoot: null,
            FixedTimestamp);

        Assert.Equal("当前没有可保存的编辑文件。", formatter.Format(result, hasLoadedFile: true));
    }

    [Fact]
    public void Formatter_ShowsNoFilePromptWhenThereIsNoLoadedFile()
    {
        Ra2SaveCurrentFileService service = new();
        Ra2SaveCurrentFileUiMessageFormatter formatter = new();

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(null, isReadOnlyPreview: false),
            projectRoot: null,
            FixedTimestamp);

        Assert.Equal("当前没有可保存的编辑文件。", formatter.Format(result, hasLoadedFile: false));
    }

    [Fact]
    public void Formatter_ShowsSuccessAndBackupPathAfterSave()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSessionService sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        Ra2EditableDocumentSession session = sessionService.StartEditing(sourcePath, "[HTNK]\nStrength=400\n");
        session = sessionService.UpdateText(session, "[HTNK]\nStrength=500\n");
        Ra2SaveCurrentFileService service = new();
        Ra2SaveCurrentFileUiMessageFormatter formatter = new();

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);
        string message = formatter.Format(result, hasLoadedFile: true);

        Assert.True(result.Success, result.Message);
        Assert.Contains("保存当前文件成功", message, StringComparison.Ordinal);
        Assert.Contains(sourcePath, message, StringComparison.Ordinal);
        Assert.Contains(result.BackupPlan!.BackupFilePath, message, StringComparison.Ordinal);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, result.UpdatedSession!.DocumentState.State);
    }

    private sealed class TestWorkspace : IDisposable
    {
        private TestWorkspace(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TestWorkspace Create()
        {
            string root = Path.Combine(Path.GetTempPath(), "RA2IniEditor.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TestWorkspace(root);
        }

        public string WriteFile(string relativePath, string text)
        {
            string path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}

