using RA2IniEditor.IDE.Controllers.EditorSession;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorSessionControllerProgrammaticTextTests
{
    [Fact]
    public void ApplyProgrammaticText_WhenSnapshotMatches_CommitsExactlyOneSessionRevision()
    {
        CountingSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);
        Ra2EditableDocumentSession session = sessionService.StartEditing(
            "rules.ini",
            "[E1]\nStrength=100\n");

        Ra2EditorSessionOperationResult result = controller.ApplyProgrammaticText(
            new Ra2EditorSessionApplyProgrammaticTextRequest(
                session,
                session.DocumentId,
                session.EditRevision,
                session.DocumentState.CurrentText,
                "[E1]\nStrength=125\n",
                requestedCaretOffset: 500));

        Assert.True(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.ApplyProgrammaticText, result.OperationKind);
        Assert.Equal(1, sessionService.UpdateTextCallCount);
        Assert.NotNull(result.Session);
        Assert.Equal(session.DocumentId, result.Session!.DocumentId);
        Assert.Equal(session.EditRevision + 1, result.Session.EditRevision);
        Assert.Equal("[E1]\nStrength=125\n", result.Session.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.Session.DocumentState.State);
        Assert.Equal("[E1]\nStrength=125\n", result.TextToSyncToEditor);
        Assert.NotNull(result.TextToSyncToEditor);
        Assert.Equal(result.TextToSyncToEditor!.Length, result.CaretOffset);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void ApplyProgrammaticText_WhenSnapshotDoesNotMatch_RejectsWithoutMutation(
        bool wrongDocumentId,
        bool wrongRevision,
        bool wrongText)
    {
        CountingSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);
        Ra2EditableDocumentSession session = sessionService.StartEditing(
            "rules.ini",
            "[E1]\nStrength=100\n");

        Ra2EditorSessionOperationResult result = controller.ApplyProgrammaticText(
            new Ra2EditorSessionApplyProgrammaticTextRequest(
                session,
                wrongDocumentId ? Guid.NewGuid() : session.DocumentId,
                wrongRevision ? session.EditRevision + 1 : session.EditRevision,
                wrongText ? "[E1]\nStrength=101\n" : session.DocumentState.CurrentText,
                "[E1]\nStrength=125\n",
                requestedCaretOffset: 0));

        Assert.False(result.Success);
        Assert.Equal(Ra2EditorSessionOperationKind.ApplyProgrammaticText, result.OperationKind);
        Assert.Equal(0, sessionService.UpdateTextCallCount);
        Assert.Null(result.Session);
        Assert.Null(result.TextToSyncToEditor);
    }

    [Fact]
    public void ApplyProgrammaticText_WhenCandidateIsNoOp_RejectsWithoutMutation()
    {
        CountingSessionService sessionService = new();
        Ra2EditorSessionController controller = new(sessionService);
        Ra2EditableDocumentSession session = sessionService.StartEditing(
            "rules.ini",
            "[E1]\nStrength=100\n");

        Ra2EditorSessionOperationResult result = controller.ApplyProgrammaticText(
            new Ra2EditorSessionApplyProgrammaticTextRequest(
                session,
                session.DocumentId,
                session.EditRevision,
                session.DocumentState.CurrentText,
                session.DocumentState.CurrentText,
                requestedCaretOffset: 0));

        Assert.False(result.Success);
        Assert.Equal(0, sessionService.UpdateTextCallCount);
        Assert.Contains("would not change", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class CountingSessionService : IRa2EditableDocumentSessionService
    {
        private readonly Ra2EditableDocumentSessionService _inner = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());

        public int UpdateTextCallCount { get; private set; }

        public Ra2EditableDocumentSession StartEditing(string filePath, string text)
            => _inner.StartEditing(filePath, text);

        public Ra2EditableDocumentSession StartEditing(
            string filePath,
            string text,
            Ra2EditorTextEncodingMetadata encodingMetadata)
            => _inner.StartEditing(filePath, text, encodingMetadata);

        public Ra2EditableDocumentSession UpdateText(
            Ra2EditableDocumentSession session,
            string currentText)
        {
            UpdateTextCallCount++;
            return _inner.UpdateText(session, currentText);
        }

        public Ra2EditableDocumentSession MarkSaved(
            Ra2EditableDocumentSession session,
            string savedText)
            => _inner.MarkSaved(session, savedText);

        public Ra2EditableDocumentSession Revert(Ra2EditableDocumentSession session)
            => _inner.Revert(session);
    }
}
