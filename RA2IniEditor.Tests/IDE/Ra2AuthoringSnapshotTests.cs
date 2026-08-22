using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AuthoringSnapshotTests
{
    private readonly Ra2EditableDocumentSessionService _sessionService = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());

    [Fact]
    public void Capture_BindsSessionTextIdentityAndRegistrySnapshot()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing(
            "rulesmd.ini",
            "[E1]\nStrength=100");
        session = _sessionService.UpdateText(session, "[E1]\nStrength=125");
        Ra2FieldRegistryProviderSnapshot registry = Registry(revision: 7);

        Ra2AuthoringSnapshotCaptureResult result = Ra2AuthoringSnapshot.Capture(
            session,
            session.DocumentState.CurrentText,
            @"C:\Project",
            registry);

        Assert.True(result.Succeeded);
        Ra2AuthoringSnapshot snapshot = Assert.IsType<Ra2AuthoringSnapshot>(result.Snapshot);
        Assert.Equal(session.DocumentId, snapshot.DocumentId);
        Assert.Equal(session.EditRevision, snapshot.EditRevision);
        Assert.Equal(session.DocumentState.CurrentText, snapshot.Text);
        Assert.Equal(session.DocumentState.FilePath, snapshot.FilePath);
        Assert.Equal(@"C:\Project", snapshot.ProjectRootPath);
        Assert.True(snapshot.IsEditable);
        Assert.True(snapshot.IsDirty);
        Assert.Same(registry, snapshot.FieldRegistry);
    }

    [Fact]
    public void Capture_RejectsMissingSession()
    {
        Ra2AuthoringSnapshotCaptureResult result = Ra2AuthoringSnapshot.Capture(
            null,
            string.Empty,
            string.Empty,
            Registry());

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AuthoringSnapshotCaptureFailureKind.NoEditableSession, result.FailureKind);
        Assert.Null(result.Snapshot);
    }

    [Fact]
    public void Capture_RejectsEditorSessionTextMismatch()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing("rulesmd.ini", "[E1]");

        Ra2AuthoringSnapshotCaptureResult result = Ra2AuthoringSnapshot.Capture(
            session,
            "[E1]\nStrength=100",
            string.Empty,
            Registry());

        Assert.Equal(
            Ra2AuthoringSnapshotCaptureFailureKind.EditorSessionTextMismatch,
            result.FailureKind);
    }

    [Fact]
    public void Capture_RejectsMissingRegistrySnapshot()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing("rulesmd.ini", "[E1]");

        Ra2AuthoringSnapshotCaptureResult result = Ra2AuthoringSnapshot.Capture(
            session,
            session.DocumentState.CurrentText,
            string.Empty,
            null);

        Assert.Equal(
            Ra2AuthoringSnapshotCaptureFailureKind.InvalidRegistrySnapshot,
            result.FailureKind);
    }

    private static Ra2FieldRegistryProviderSnapshot Registry(long revision = 1)
        => new(new BuiltInRa2FieldDefinitionProvider(), revision);
}
