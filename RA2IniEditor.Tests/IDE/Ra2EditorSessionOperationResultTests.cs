using RA2IniEditor.IDE.Controllers.EditorSession;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorSessionOperationResultTests
{
    [Fact]
    public void EnteredEditMode_RequestsEditableUiAndKeepsSession()
    {
        Ra2EditableDocumentSession session = CreateSession(Ra2EditorDocumentState.EditableClean);

        Ra2EditorSessionOperationResult result =
            Ra2EditorSessionOperationResult.EnteredEditMode(session, "entered");

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.EnterEditMode, result.OperationKind);
        Assert.Same(session, result.Session);
        Assert.True(result.ShouldSetEditable);
        Assert.False(result.ShouldSetReadOnly);
        Assert.Null(result.TextToSyncToEditor);
        Assert.Equal("entered", result.Message);
    }

    [Fact]
    public void UpdatedFromUserText_KeepsSessionWithoutEditorTextSync()
    {
        Ra2EditableDocumentSession session = CreateSession(Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSessionOperationResult result =
            Ra2EditorSessionOperationResult.UpdatedFromUserText(session);

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.UpdateTextFromUser, result.OperationKind);
        Assert.Same(session, result.Session);
        Assert.Null(result.TextToSyncToEditor);
        Assert.False(result.ShouldSetEditable);
        Assert.False(result.ShouldSetReadOnly);
    }

    [Fact]
    public void AppliedProgrammaticText_CarriesTextAndCaretForShellSync()
    {
        Ra2EditableDocumentSession session = CreateSession(Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSessionOperationResult result =
            Ra2EditorSessionOperationResult.AppliedProgrammaticText(session, "[E1]\nStrength=125\n", 18);

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.ApplyProgrammaticText, result.OperationKind);
        Assert.Same(session, result.Session);
        Assert.Equal("[E1]\nStrength=125\n", result.TextToSyncToEditor);
        Assert.Equal(18, result.CaretOffset);
    }

    [Fact]
    public void Reverted_KeepsEditableSessionAndOriginalTextSync()
    {
        Ra2EditableDocumentSession session = CreateSession(Ra2EditorDocumentState.EditableClean);

        Ra2EditorSessionOperationResult result =
            Ra2EditorSessionOperationResult.Reverted(session, "[E1]\n", "reverted");

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.Revert, result.OperationKind);
        Assert.Same(session, result.Session);
        Assert.Equal("[E1]\n", result.TextToSyncToEditor);
        Assert.False(result.ShouldSetReadOnly);
        Assert.True(result.ShouldSetEditable);
        Assert.Equal("reverted", result.Message);
    }

    [Fact]
    public void Failed_DoesNotRequestEditorTextOrUiStateMutation()
    {
        Ra2EditorSessionOperationResult result =
            Ra2EditorSessionOperationResult.Failed(Ra2EditorSessionOperationKind.Revert, "nothing to revert");

        Assert.False(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.Revert, result.OperationKind);
        Assert.Null(result.Session);
        Assert.Null(result.TextToSyncToEditor);
        Assert.Null(result.CaretOffset);
        Assert.False(result.ShouldSetReadOnly);
        Assert.False(result.ShouldSetEditable);
        Assert.Equal("nothing to revert", result.Message);
    }

    private static Ra2EditableDocumentSession CreateSession(Ra2EditorDocumentState state)
    {
        const string text = "[E1]\n";
        Ra2EditableDocumentState documentState = new("rules.ini", text, text, state);
        return new Ra2EditableDocumentSession(documentState, new Ra2IniTextDocumentParser().Parse(text));
    }
}
