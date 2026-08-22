using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class SourceEditorStateTests
{
    [Fact]
    public void SourceEditorViewModel_MethodsUpdateDiagnosableState()
    {
        SourceEditorViewModel viewModel = new();

        viewModel.ShowEmptyState("empty");
        Assert.Equal(SourceEditorState.Empty, viewModel.State);

        viewModel.ShowLoading("rules.ini");
        Assert.Equal(SourceEditorState.Loading, viewModel.State);

        viewModel.ShowDocument("rules.ini", "[Rules]");
        Assert.Equal(SourceEditorState.Loaded, viewModel.State);

        viewModel.ShowLargeFileDeferred("rules.ini", "deferred", "large");
        Assert.Equal(SourceEditorState.DeferredLargeFile, viewModel.State);

        viewModel.ShowError("rules.ini", "failed");
        Assert.Equal(SourceEditorState.ReadFailed, viewModel.State);
    }

    [Theory]
    [InlineData(SourceEditorState.Empty, false)]
    [InlineData(SourceEditorState.Loading, false)]
    [InlineData(SourceEditorState.Loaded, true)]
    [InlineData(SourceEditorState.DeferredLargeFile, false)]
    [InlineData(SourceEditorState.ReadFailed, false)]
    public void CurrentSourceSnapshot_CanRunDiagnosticsOnlyWhenLoaded(SourceEditorState state, bool expected)
    {
        CurrentSourceSnapshot snapshot = new("C:\\mod", "C:\\mod\\rules.ini", "rules.ini", "[Rules]", 42, state);

        Assert.Equal(expected, snapshot.CanRunDiagnostics);
    }
}
