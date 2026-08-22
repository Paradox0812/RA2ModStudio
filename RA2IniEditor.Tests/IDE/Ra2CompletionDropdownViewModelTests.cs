using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionDropdownViewModelTests
{
    [Fact]
    public void Update_MapsCompletionItemsAndReplacementSpan()
    {
        Ra2CompletionDropdownViewModel viewModel = new();
        Ra2CompletionResult result = new(
            [
                new Ra2CompletionItem(
                    "Strength",
                    Ra2CompletionItemKind.Key,
                    "Type: Integer",
                    "Hit points",
                    "Strength",
                    100,
                    Ra2CompletionItemSourceKind.FieldRegistry)
            ],
            new Ra2TextSpan(12, 3));

        viewModel.Update(result);

        Ra2CompletionDropdownItemViewModel item = Assert.Single(viewModel.Items);
        Assert.Equal("Strength", item.Label);
        Assert.Same(result.Items[0], item.Item);
        Assert.Equal("Key", item.Kind);
        Assert.Equal("Integer", item.TypeDisplay);
        Assert.Equal("Field Registry", item.SourceDisplayText);
        Assert.Equal("Type: Integer", item.Detail);
        Assert.Equal("Hit points", item.AnnotationText);
        Assert.False(item.IsFallback);
        Assert.Equal(12, viewModel.ReplacementStart);
        Assert.Equal(3, viewModel.ReplacementLength);
        Assert.Equal("Replacement: start=12, length=3", viewModel.ReplacementText);
        Assert.Equal("Items: 1", viewModel.CountText);
        Assert.True(viewModel.HasItems);
        Assert.Equal(0, viewModel.SelectedIndex);
    }

    [Fact]
    public void Update_MarksUnknownFallbackSource()
    {
        Ra2CompletionDropdownViewModel viewModel = new();
        Ra2CompletionResult result = new(
            [
                new Ra2CompletionItem(
                    "MYSTERY",
                    Ra2CompletionItemKind.Reference,
                    "Unclassified section fallback",
                    "Line 8",
                    "MYSTERY",
                    10,
                    Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback)
            ],
            new Ra2TextSpan(20, 0));

        viewModel.Update(result);

        Ra2CompletionDropdownItemViewModel item = Assert.Single(viewModel.Items);
        Assert.Equal("Reference", item.TypeDisplay);
        Assert.Equal("Current Document - Unclassified", item.SourceDisplayText);
        Assert.Equal("Line 8", item.AnnotationText);
        Assert.True(item.IsFallback);
    }

    [Fact]
    public void Update_MapsBuiltInValueCatalogSourceForValueCompletion()
    {
        Ra2CompletionDropdownViewModel viewModel = new();
        Ra2CompletionResult result = new(
            [
                new Ra2CompletionItem(
                    "heavy",
                    Ra2CompletionItemKind.Value,
                    "Type: Enum",
                    "Enum value.",
                    "heavy",
                    90,
                    Ra2CompletionItemSourceKind.BuiltInValueCatalog)
            ],
            new Ra2TextSpan(20, 2));

        viewModel.Update(result);

        Ra2CompletionDropdownItemViewModel item = Assert.Single(viewModel.Items);
        Assert.Equal("heavy", item.Label);
        Assert.Equal("Value", item.Kind);
        Assert.Equal("Enum", item.TypeDisplay);
        Assert.Equal("BuiltIn", item.SourceDisplayText);
        Assert.Equal("Enum value.", item.AnnotationText);
    }

    [Fact]
    public void Update_EmptyResultShowsStableStatus()
    {
        Ra2CompletionDropdownViewModel viewModel = new();

        viewModel.Update(Ra2CompletionResult.EmptyAt(7));

        Assert.Empty(viewModel.Items);
        Assert.False(viewModel.HasItems);
        Assert.Equal(-1, viewModel.SelectedIndex);
        Assert.Equal("Items: 0", viewModel.CountText);
        Assert.Equal("No completion items for current caret position.", viewModel.StatusText);
        Assert.Equal("Replacement: start=7, length=0", viewModel.ReplacementText);
    }

    [Fact]
    public void MoveSelection_ClampsToAvailableItems()
    {
        Ra2CompletionDropdownViewModel viewModel = new();
        viewModel.Update(new Ra2CompletionResult(
            [
                new Ra2CompletionItem("Armor", Ra2CompletionItemKind.Key),
                new Ra2CompletionItem("Sight", Ra2CompletionItemKind.Key)
            ],
            new Ra2TextSpan(0, 0)));

        viewModel.MoveSelection(1);
        Assert.Equal(1, viewModel.SelectedIndex);

        viewModel.MoveSelection(1);
        Assert.Equal(1, viewModel.SelectedIndex);

        viewModel.MoveSelection(-4);
        Assert.Equal(0, viewModel.SelectedIndex);
    }

    [Fact]
    public void SelectedIndex_CanBeUpdatedByDropdownListSelection()
    {
        Ra2CompletionDropdownViewModel viewModel = new();
        viewModel.Update(new Ra2CompletionResult(
            [
                new Ra2CompletionItem("Armor", Ra2CompletionItemKind.Key),
                new Ra2CompletionItem("Sight", Ra2CompletionItemKind.Key)
            ],
            new Ra2TextSpan(0, 0)));

        viewModel.SelectedIndex = 1;

        Assert.Equal("Sight", viewModel.SelectedItem!.Label);
    }
}
