using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Search;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class SearchToolWindowViewModelTests
{
    [Fact]
    public void QueryAndScopeControlSearchAndReplaceAvailability()
    {
        SearchToolWindowViewModel viewModel = new();

        Assert.False(viewModel.CanSearch);
        Assert.False(viewModel.CanPreviewReplace);

        viewModel.Query = "Gun";
        Assert.True(viewModel.CanSearch);
        Assert.False(viewModel.CanPreviewReplace);

        viewModel.SelectedScopeIndex = 1;
        Assert.True(viewModel.CanPreviewReplace);
    }

    [Fact]
    public void ChangingReplacementInvalidatesExistingPreview()
    {
        Ra2EditableDocumentSessionService sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        Ra2EditableDocumentSession session = sessionService.StartEditing("rulesmd.ini", "Gun");
        Ra2CurrentFileReplacePlan plan = new Ra2CurrentFileReplacePlanner().Plan(
            session,
            new Ra2SearchOptions("Gun", Ra2SearchScope.CurrentFile, false, false, false, "*.ini"),
            "Laser");
        SearchToolWindowViewModel viewModel = new()
        {
            Query = "Gun",
            SelectedScopeIndex = 1,
            ReplacementText = "Laser"
        };
        viewModel.ApplyReplacePlan(plan);
        Assert.True(viewModel.CanApplyReplace);

        viewModel.ReplacementText = "Cannon";

        Assert.False(viewModel.CanApplyReplace);
        Assert.Null(viewModel.CurrentReplacePlan);
    }

    [Fact]
    public void MoveSelectionWrapsAcrossResultSnapshot()
    {
        SearchToolWindowViewModel viewModel = new();
        viewModel.ApplySearchResult(Ra2SearchExecutionResult.Completed(
        [
            new Ra2SearchMatch("rulesmd.ini", "rulesmd.ini", 1, 1, "E1", "Gun", "Gun", 0, 3),
            new Ra2SearchMatch("artmd.ini", "artmd.ini", 2, 1, "E2", "Gun", "Gun", 4, 3)
        ],
        "2 results",
        2,
        0,
        false));

        Assert.Equal("artmd.ini", viewModel.MoveSelection(-1)!.FileName);
        Assert.Equal("rulesmd.ini", viewModel.MoveSelection(1)!.FileName);
    }
}
