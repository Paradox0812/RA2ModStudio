using RA2IniEditor.IDE.Controllers.EditorSession;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorSessionControllerUpdateTextTests
{
    [Fact]
    public void UpdateTextFromUser_WhenSessionExists_UpdatesSessionCurrentText()
    {
        FakeSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);
        Ra2EditableDocumentSession session = CreateSession("[E1]\n", "[E1]\n", Ra2EditorDocumentState.EditableClean);

        Ra2EditorSessionOperationResult result = controller.UpdateTextFromUser(
            new Ra2EditorSessionUpdateTextRequest(session, "[E1]\nStrength=125\n"));

        Assert.True(result.Success);
        Assert.Equal(1, sessionService.UpdateTextCallCount);
        Assert.NotNull(result.Session);
        Assert.Equal("[E1]\nStrength=125\n", result.Session!.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.Session.DocumentState.State);
        Assert.Null(result.TextToSyncToEditor);
        Assert.Null(result.CaretOffset);
        Assert.False(result.ShouldSetEditable);
        Assert.False(result.ShouldSetReadOnly);
    }

    [Fact]
    public void UpdateTextFromUser_WhenTextMatchesOriginal_ReturnsCleanSession()
    {
        FakeSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);
        Ra2EditableDocumentSession session = CreateSession(
            "[E1]\n",
            "[E1]\nStrength=125\n",
            Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSessionOperationResult result = controller.UpdateTextFromUser(
            new Ra2EditorSessionUpdateTextRequest(session, "[E1]\n"));

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, result.Session!.DocumentState.State);
        Assert.Equal("[E1]\n", result.Session.DocumentState.CurrentText);
    }

    [Fact]
    public void UpdateTextFromUser_WhenSessionIsNull_ReturnsFailureNoOp()
    {
        FakeSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);

        Ra2EditorSessionOperationResult result = controller.UpdateTextFromUser(
            new Ra2EditorSessionUpdateTextRequest(null, "[E1]\n"));

        Assert.False(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.UpdateTextFromUser, result.OperationKind);
        Assert.Equal(0, sessionService.UpdateTextCallCount);
        Assert.Null(result.Session);
        Assert.Null(result.TextToSyncToEditor);
        Assert.Null(result.CaretOffset);
        Assert.False(result.ShouldSetEditable);
        Assert.False(result.ShouldSetReadOnly);
        Assert.Contains("no editable session", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Ra2EditableDocumentSession CreateSession(
        string originalText,
        string currentText,
        Ra2EditorDocumentState state)
    {
        Ra2EditableDocumentState documentState = new("rules.ini", originalText, currentText, state);
        return new Ra2EditableDocumentSession(documentState, new Ra2IniTextDocumentParser().Parse(currentText));
    }

    private sealed class FakeSessionService : IRa2EditableDocumentSessionService
    {
        private readonly Ra2IniTextDocumentParser _parser = new();

        public int UpdateTextCallCount { get; private set; }

        public Ra2EditableDocumentSession StartEditing(string filePath, string text)
            => throw new NotSupportedException("UpdateTextFromUser must not call StartEditing.");

        public Ra2EditableDocumentSession StartEditing(
            string filePath,
            string text,
            Ra2EditorTextEncodingMetadata encodingMetadata)
            => throw new NotSupportedException("UpdateTextFromUser must not call StartEditing.");

        public Ra2EditableDocumentSession UpdateText(Ra2EditableDocumentSession session, string currentText)
        {
            UpdateTextCallCount++;
            Ra2EditorDocumentState state = string.Equals(
                currentText,
                session.DocumentState.OriginalText,
                StringComparison.Ordinal)
                ? Ra2EditorDocumentState.EditableClean
                : Ra2EditorDocumentState.EditableDirty;
            Ra2EditableDocumentState documentState = new(
                session.DocumentState.FilePath,
                session.DocumentState.OriginalText,
                currentText,
                state,
                session.DocumentState.EncodingMetadata);
            return new Ra2EditableDocumentSession(documentState, _parser.Parse(currentText));
        }

        public Ra2EditableDocumentSession MarkSaved(Ra2EditableDocumentSession session, string savedText)
            => throw new NotSupportedException("UpdateTextFromUser must not call MarkSaved.");

        public Ra2EditableDocumentSession Revert(Ra2EditableDocumentSession session)
            => throw new NotSupportedException("UpdateTextFromUser must not call Revert.");
    }
}
