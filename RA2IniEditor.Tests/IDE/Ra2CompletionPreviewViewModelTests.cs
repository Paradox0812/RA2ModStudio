using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionPreviewViewModelTests
{
    [Fact]
    public void CompletionItemViewModel_MapsCompletionItem()
    {
        Ra2CompletionItem item = new(
            "Strength",
            Ra2CompletionItemKind.Key,
            "Type: Integer",
            "Hit points",
            "Strength",
            100,
            Ra2CompletionItemSourceKind.FieldRegistry);

        Ra2CompletionItemViewModel viewModel = new(item);

        Assert.Equal("Strength", viewModel.Label);
        Assert.Equal("Key", viewModel.Kind);
        Assert.Equal("Type: Integer", viewModel.Detail);
        Assert.Equal("Hit points", viewModel.Documentation);
        Assert.Equal("Strength", viewModel.InsertText);
        Assert.Equal(100, viewModel.Priority);
        Assert.Equal("FieldRegistry", viewModel.SourceKind);
        Assert.False(viewModel.IsFallback);
        Assert.Equal("Field Registry", viewModel.SourceDisplayText);
    }

    [Fact]
    public void CompletionPreviewViewModel_PreservesReplacementSpanAndItems()
    {
        Ra2CompletionResult result = new(
            [
                new Ra2CompletionItem("120mm", Ra2CompletionItemKind.Reference, "Weapon section")
            ],
            new Ra2TextSpan(42, 2));

        Ra2CompletionPreviewViewModel viewModel = new(result);

        Assert.True(viewModel.HasItems);
        Assert.Single(viewModel.Items);
        Ra2CompletionGroupViewModel group = Assert.Single(viewModel.Groups);
        Assert.Equal("Reference", group.Name);
        Assert.Single(group.Items);
        Assert.Equal(42, viewModel.ReplacementStart);
        Assert.Equal(2, viewModel.ReplacementLength);
        Assert.Equal("Replacement: start=42, length=2", viewModel.ReplacementText);
        Assert.Equal("Items: 1", viewModel.CountText);
        Assert.Equal("1 completion item.", viewModel.StatusText);
    }

    [Fact]
    public void CompletionPreviewViewModel_EmptyResultShowsNoItemsStatus()
    {
        Ra2CompletionResult result = Ra2CompletionResult.EmptyAt(12);

        Ra2CompletionPreviewViewModel viewModel = new(result);

        Assert.False(viewModel.HasItems);
        Assert.Empty(viewModel.Items);
        Assert.Empty(viewModel.Groups);
        Assert.Equal(12, viewModel.ReplacementStart);
        Assert.Equal(0, viewModel.ReplacementLength);
        Assert.Equal("Items: 0", viewModel.CountText);
        Assert.Equal("No completion items for current caret position.", viewModel.EmptyText);
        Assert.Equal("No completion items for current caret position.", viewModel.StatusText);
    }

    [Fact]
    public void CompletionItemViewModel_MarksUnknownFallbackSource()
    {
        Ra2CompletionItem item = new(
            "MYSTERY",
            Ra2CompletionItemKind.Reference,
            "Unclassified section fallback",
            "Line 8",
            "MYSTERY",
            10,
            Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback);

        Ra2CompletionItemViewModel viewModel = new(item);

        Assert.Equal("CurrentDocumentUnknownFallback", viewModel.SourceKind);
        Assert.True(viewModel.IsFallback);
        Assert.Equal("Current Document - Unclassified", viewModel.SourceDisplayText);
    }

    [Fact]
    public void CompletionPreviewViewModel_GroupsItemsByKindInStableOrder()
    {
        Ra2CompletionResult result = new(
            [
                new Ra2CompletionItem("120mm", Ra2CompletionItemKind.Reference),
                new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key),
                new Ra2CompletionItem("Sight", Ra2CompletionItemKind.Key)
            ],
            new Ra2TextSpan(4, 0));

        Ra2CompletionPreviewViewModel viewModel = new(result);

        Assert.Equal(["Key", "Reference"], viewModel.Groups.Select(group => group.Name).ToArray());
        Assert.Equal(["Strength", "Sight"], viewModel.Groups[0].Items.Select(item => item.Label).ToArray());
        Assert.Equal(["120mm"], viewModel.Groups[1].Items.Select(item => item.Label).ToArray());
    }
}
