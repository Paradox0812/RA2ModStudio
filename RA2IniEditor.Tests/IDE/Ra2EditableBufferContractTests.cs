using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditableBufferContractTests
{
    [Fact]
    public void EditableDocumentState_AllowsOriginalAndCurrentTextToDiffer()
    {
        Ra2EditableDocumentState state = new(
            "rulesmd.ini",
            "[NEWINF]\nStrength=100",
            "[NEWINF]\nStrength=125",
            Ra2EditorDocumentState.EditableDirty);

        Assert.NotEqual(state.OriginalText, state.CurrentText);
        Assert.True(state.IsDirty);
    }

    [Fact]
    public void EditableDocumentState_DirtyFlagComesFromState()
    {
        Ra2EditableDocumentState clean = new(
            "rulesmd.ini",
            "text",
            "changed text",
            Ra2EditorDocumentState.EditableClean);

        Assert.False(clean.IsDirty);
    }

    [Fact]
    public void EditableDocumentState_PreservesDuplicateSectionsAndKeysAsText()
    {
        const string text = """
            [DUP]
            Strength=100
            Strength=125

            [DUP]
            ; duplicate section intentionally preserved
            Armor=none
            """;

        Ra2EditableDocumentState state = new(
            "rulesmd.ini",
            text,
            text,
            Ra2EditorDocumentState.EditableClean);

        Assert.Contains("[DUP]\nStrength=100\nStrength=125", state.CurrentText);
        Assert.Contains("[DUP]\n; duplicate section intentionally preserved", state.CurrentText);
    }

    [Theory]
    [InlineData((int)Ra2EditorDocumentState.ReadOnlyPreview, false)]
    [InlineData((int)Ra2EditorDocumentState.EditableClean, false)]
    [InlineData((int)Ra2EditorDocumentState.EditableDirty, true)]
    public void EditorSaveBoundary_CanSaveOnlyEditableDirtyDocuments(
        int documentState,
        bool expected)
    {
        Ra2EditableDocumentState state = new(
            "rulesmd.ini",
            "text",
            "text",
            (Ra2EditorDocumentState)documentState);

        Assert.Equal(expected, new Ra2EditorSaveBoundary().CanSave(state));
    }
}
