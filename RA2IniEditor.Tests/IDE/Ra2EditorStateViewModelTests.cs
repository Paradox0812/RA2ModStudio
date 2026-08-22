using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.ViewModels.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorStateViewModelTests
{
    [Fact]
    public void ReadOnlyPreview_ExposesNoFileStateWithoutEnterEditCapability()
    {
        Ra2EditorStateViewModel viewModel = new(
            Ra2EditorDocumentState.ReadOnlyPreview,
            null,
            hasSession: false);

        Assert.True(viewModel.IsReadOnlyPreview);
        Assert.False(viewModel.IsEditing);
        Assert.False(viewModel.IsDirty);
        Assert.False(viewModel.CanEnterEditMode);
        Assert.False(viewModel.CanRevertInMemoryChanges);
        Assert.False(viewModel.CanSavePreview);
        Assert.Equal("未选择文件", viewModel.StateText);
        Assert.Equal("请选择一个 INI 文件。", viewModel.SaveHintText);
    }

    [Fact]
    public void EditableClean_ExposesOpenedStateWithoutSavePreview()
    {
        Ra2EditorStateViewModel viewModel = new(
            Ra2EditorDocumentState.EditableClean,
            "rulesmd.ini",
            hasSession: true);

        Assert.False(viewModel.CanEnterEditMode);
        Assert.True(viewModel.CanRevertInMemoryChanges);
        Assert.False(viewModel.CanSavePreview);
        Assert.Equal("已打开", viewModel.StateText);
        Assert.Equal("没有未保存的内容修改。", viewModel.SaveHintText);
    }

    [Fact]
    public void EditableDirty_ExposesModifiedStateAndSavePreview()
    {
        Ra2EditorStateViewModel viewModel = new(
            Ra2EditorDocumentState.EditableDirty,
            "rulesmd.ini",
            hasSession: true);

        Assert.True(viewModel.IsDirty);
        Assert.False(viewModel.CanEnterEditMode);
        Assert.True(viewModel.CanRevertInMemoryChanges);
        Assert.True(viewModel.CanSavePreview);
        Assert.Equal("内存中已修改", viewModel.StateText);
        Assert.Equal("当前文件有未保存的内容修改。请保存或放弃内存修改。", viewModel.SaveHintText);
    }
}
