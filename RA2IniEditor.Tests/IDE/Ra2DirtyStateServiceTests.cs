using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2DirtyStateServiceTests
{
    private readonly Ra2DirtyStateService _service = new();

    [Fact]
    public void EditableClean_TextChangedBecomesEditableDirty()
    {
        Assert.Equal(
            Ra2EditorDocumentState.EditableDirty,
            _service.GetNextState(Ra2EditorDocumentState.EditableClean, textChanged: true, saved: false));
    }

    [Fact]
    public void EditableDirty_SavedWithoutTextChangeBecomesEditableClean()
    {
        Assert.Equal(
            Ra2EditorDocumentState.EditableClean,
            _service.GetNextState(Ra2EditorDocumentState.EditableDirty, textChanged: false, saved: true));
    }

    [Fact]
    public void EditableDirty_TextChangedStaysEditableDirty()
    {
        Assert.Equal(
            Ra2EditorDocumentState.EditableDirty,
            _service.GetNextState(Ra2EditorDocumentState.EditableDirty, textChanged: true, saved: false));
    }

    [Fact]
    public void EditableClean_SavedWithoutTextChangeStaysEditableClean()
    {
        Assert.Equal(
            Ra2EditorDocumentState.EditableClean,
            _service.GetNextState(Ra2EditorDocumentState.EditableClean, textChanged: false, saved: true));
    }

    [Fact]
    public void ReadOnlyPreview_TextChangedDoesNotBecomeDirty()
    {
        Assert.Equal(
            Ra2EditorDocumentState.ReadOnlyPreview,
            _service.GetNextState(Ra2EditorDocumentState.ReadOnlyPreview, textChanged: true, saved: false));
    }

    [Fact]
    public void EditableDirty_NotSavedStaysDirty()
    {
        Assert.Equal(
            Ra2EditorDocumentState.EditableDirty,
            _service.GetNextState(Ra2EditorDocumentState.EditableDirty, textChanged: false, saved: false));
    }
}
