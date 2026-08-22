using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorStateViewModelFactoryTests
{
    private readonly Ra2EditorStateViewModelFactory _factory = new();

    [Fact]
    public void Create_NullSessionReturnsNoFileState()
    {
        Ra2EditorStateViewModel viewModel = _factory.Create(null);

        Assert.Equal(Ra2EditorDocumentState.ReadOnlyPreview, viewModel.State);
        Assert.False(viewModel.HasSession);
        Assert.Equal(string.Empty, viewModel.FilePath);
        Assert.Equal("未选择文件", viewModel.StateText);
    }

    [Fact]
    public void Create_SessionMapsDocumentStateAndFilePath()
    {
        Ra2EditableDocumentSession session = CreateSession(
            "rulesmd.ini",
            "[E1]\nStrength=125",
            Ra2EditorDocumentState.EditableDirty);

        Ra2EditorStateViewModel viewModel = _factory.Create(session);

        Assert.Equal(Ra2EditorDocumentState.EditableDirty, viewModel.State);
        Assert.True(viewModel.HasSession);
        Assert.Equal("rulesmd.ini", viewModel.FilePath);
        Assert.Equal("内存中已修改", viewModel.StateText);
    }

    private static Ra2EditableDocumentSession CreateSession(
        string filePath,
        string text,
        Ra2EditorDocumentState state)
    {
        Ra2EditableDocumentState documentState = new(filePath, text, text, state);
        return new Ra2EditableDocumentSession(
            documentState,
            new Ra2IniTextDocumentParser().Parse(text));
    }
}
