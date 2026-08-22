using RA2IniEditor.IDE.Controllers.EditorSession;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorSessionControllerTests
{
    [Fact]
    public void EnterEditMode_UsesSessionServiceAndRequestsEditableUi()
    {
        FakeSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);
        Ra2EditorTextEncodingMetadata metadata = new(Ra2EditorTextEncodingKind.Utf8Bom, "UTF-8 BOM", true);

        Ra2EditorSessionOperationResult result = controller.EnterEditMode(
            new Ra2EditorSessionEnterRequest("rules.ini", "[E1]\n", metadata));

        Assert.True(result.Success);
        Assert.Equal(1, sessionService.StartEditingCallCount);
        Assert.Equal("rules.ini", sessionService.LastFilePath);
        Assert.Equal("[E1]\n", sessionService.LastText);
        Assert.Same(metadata, sessionService.LastEncodingMetadata);
        Assert.NotNull(result.Session);
        Assert.Same(metadata, result.Session!.DocumentState.EncodingMetadata);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, result.Session!.DocumentState.State);
        Assert.Equal("[E1]\n", result.Session.DocumentState.CurrentText);
        Assert.True(result.ShouldSetEditable);
        Assert.False(result.ShouldSetReadOnly);
        Assert.Null(result.TextToSyncToEditor);
        Assert.Contains("editable", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Revert_WhenSessionExists_ReturnsOriginalTextAndKeepsEditableUi()
    {
        FakeSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);
        Ra2EditableDocumentSession session = CreateSession(
            originalText: "[E1]\n",
            currentText: "[E1]\nStrength=125\n",
            state: Ra2EditorDocumentState.EditableDirty);

        Ra2EditorSessionOperationResult result = controller.Revert(new Ra2EditorSessionRevertRequest(session));

        Assert.True(result.Success);
        Assert.Equal(1, sessionService.RevertCallCount);
        Assert.Equal("[E1]\n", result.TextToSyncToEditor);
        Assert.False(result.ShouldSetReadOnly);
        Assert.True(result.ShouldSetEditable);
        Assert.NotNull(result.Session);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, result.Session!.DocumentState.State);
        Assert.Contains("Reverted", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Revert_WhenSessionIsNull_ReturnsFailureNoOp()
    {
        FakeSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);

        Ra2EditorSessionOperationResult result = controller.Revert(new Ra2EditorSessionRevertRequest(null));

        Assert.False(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.Revert, result.OperationKind);
        Assert.Equal(0, sessionService.RevertCallCount);
        Assert.Null(result.Session);
        Assert.Null(result.TextToSyncToEditor);
        Assert.False(result.ShouldSetReadOnly);
        Assert.False(result.ShouldSetEditable);
        Assert.Contains("no in-memory changes", result.Message, StringComparison.OrdinalIgnoreCase);
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

        public int StartEditingCallCount { get; private set; }

        public int RevertCallCount { get; private set; }

        public string? LastFilePath { get; private set; }

        public string? LastText { get; private set; }
        public Ra2EditorTextEncodingMetadata? LastEncodingMetadata { get; private set; }

        public Ra2EditableDocumentSession StartEditing(string filePath, string text)
            => StartEditing(filePath, text, Ra2EditorTextEncodingMetadata.Unknown);

        public Ra2EditableDocumentSession StartEditing(
            string filePath,
            string text,
            Ra2EditorTextEncodingMetadata encodingMetadata)
        {
            StartEditingCallCount++;
            LastFilePath = filePath;
            LastText = text;
            LastEncodingMetadata = encodingMetadata;
            Ra2EditableDocumentState state = new(filePath, text, text, Ra2EditorDocumentState.EditableClean, encodingMetadata);
            return new Ra2EditableDocumentSession(state, _parser.Parse(text));
        }

        public Ra2EditableDocumentSession UpdateText(Ra2EditableDocumentSession session, string currentText)
            => throw new NotSupportedException("Phase 1 controller must not call UpdateText.");

        public Ra2EditableDocumentSession MarkSaved(Ra2EditableDocumentSession session, string savedText)
            => throw new NotSupportedException("Phase 1 controller must not call MarkSaved.");

        public Ra2EditableDocumentSession Revert(Ra2EditableDocumentSession session)
        {
            RevertCallCount++;
            Ra2EditableDocumentState state = new(
                session.DocumentState.FilePath,
                session.DocumentState.OriginalText,
                session.DocumentState.OriginalText,
                Ra2EditorDocumentState.EditableClean,
                session.DocumentState.EncodingMetadata);
            return new Ra2EditableDocumentSession(state, _parser.Parse(state.CurrentText));
        }
    }
}
