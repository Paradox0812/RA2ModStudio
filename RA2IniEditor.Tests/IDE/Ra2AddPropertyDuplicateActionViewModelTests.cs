using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AddPropertyDuplicateActionViewModelTests
{
    [Fact]
    public void Constructor_WithDuplicateDefaultsToJumpExistingAndShowsDetails()
    {
        Ra2DuplicateKeyMatch match = Match("Strength", 12, "400");

        Ra2AddPropertyDuplicateActionViewModel viewModel = new(match, Ra2EditorDocumentState.EditableClean);

        Assert.True(viewModel.HasDuplicate);
        Assert.Equal(Ra2DuplicateKeyAction.JumpExisting, viewModel.SelectedAction);
        Assert.Contains("Strength", viewModel.WarningText);
        Assert.Contains("第 12 行", viewModel.WarningText);
        Assert.Contains("400", viewModel.WarningText);
    }

    [Fact]
    public void Constructor_ReadOnlyAllowsJumpOnlyForTextChangingActions()
    {
        Ra2AddPropertyDuplicateActionViewModel viewModel = new(
            Match("Strength", 12, "400"),
            Ra2EditorDocumentState.ReadOnlyPreview);

        Assert.True(viewModel.CanJumpExisting);
        Assert.False(viewModel.CanReplaceExisting);
        Assert.False(viewModel.CanInsertDuplicate);
        Assert.True(viewModel.CanConfirmSelectedAction);

        viewModel.SelectedAction = Ra2DuplicateKeyAction.ReplaceExisting;
        Assert.False(viewModel.CanConfirmSelectedAction);
    }

    [Fact]
    public void Constructor_EditModeAllowsAllDuplicateActions()
    {
        Ra2AddPropertyDuplicateActionViewModel viewModel = new(
            Match("Strength", 12, "400"),
            Ra2EditorDocumentState.EditableDirty);

        Assert.True(viewModel.CanJumpExisting);
        Assert.True(viewModel.CanReplaceExisting);
        Assert.True(viewModel.CanInsertDuplicate);

        viewModel.SelectedAction = Ra2DuplicateKeyAction.InsertDuplicate;
        Assert.True(viewModel.CanConfirmSelectedAction);
    }

    private static Ra2DuplicateKeyMatch Match(string key, int lineNumber, string value)
        => new(key, lineNumber, new Ra2TextSpan(0, key.Length + value.Length + 1), new Ra2TextSpan(key.Length + 1, value.Length), value);
}
