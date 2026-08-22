using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditableDocumentSessionTests
{
    private readonly Ra2EditableDocumentSessionService _service = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());

    [Fact]
    public void Session_HoldsDocumentStateAndTextDocument()
    {
        Ra2EditableDocumentState state = new(
            "rulesmd.ini",
            "[A]",
            "[A]",
            Ra2EditorDocumentState.EditableClean);
        Ra2IniTextDocument textDocument = new Ra2IniTextDocumentParser().Parse(state.CurrentText);

        Ra2EditableDocumentSession session = new(state, textDocument);

        Assert.Same(state, session.DocumentState);
        Assert.Same(textDocument, session.TextDocument);
    }

    [Fact]
    public void StartEditing_CreatesCleanSessionFromCurrentText()
    {
        const string text = "[E1]\nStrength=100";

        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", text);

        Assert.Equal("rulesmd.ini", session.DocumentState.FilePath);
        Assert.Equal(text, session.DocumentState.OriginalText);
        Assert.Equal(text, session.DocumentState.CurrentText);
        Assert.NotEqual(Guid.Empty, session.DocumentId);
        Assert.Equal(0, session.EditRevision);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, session.DocumentState.State);
        Assert.False(session.DocumentState.IsDirty);
        Assert.Equal(text, session.TextDocument.Text);
    }

    [Fact]
    public void StartEditing_DefaultsEncodingMetadataToUnknown()
    {
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]");

        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, session.DocumentState.EncodingMetadata);
    }

    [Fact]
    public void StartEditing_WithEncodingMetadataCarriesMetadata()
    {
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.Utf8Bom,
            "UTF-8 BOM",
            hasBom: true);

        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]", metadata);

        Assert.Same(metadata, session.DocumentState.EncodingMetadata);
    }

    [Fact]
    public void UpdateText_WhenTextChangesMarksSessionDirty()
    {
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]\nStrength=100");

        Ra2EditableDocumentSession updated = _service.UpdateText(session, "[E1]\nStrength=125");

        Assert.Equal(Ra2EditorDocumentState.EditableDirty, updated.DocumentState.State);
        Assert.True(updated.DocumentState.IsDirty);
        Assert.Equal("[E1]\nStrength=125", updated.DocumentState.CurrentText);
        Assert.Equal(session.DocumentId, updated.DocumentId);
        Assert.Equal(1, updated.EditRevision);
    }

    [Fact]
    public void UpdateText_WhenTextReturnsToOriginalMarksSessionClean()
    {
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditableDocumentSession dirty = _service.UpdateText(session, "[E1]\nStrength=125");

        Ra2EditableDocumentSession clean = _service.UpdateText(dirty, "[E1]\nStrength=100");

        Assert.Equal(Ra2EditorDocumentState.EditableClean, clean.DocumentState.State);
        Assert.False(clean.DocumentState.IsDirty);
        Assert.Equal(session.DocumentId, clean.DocumentId);
        Assert.Equal(2, clean.EditRevision);
    }

    [Fact]
    public void UpdateText_RebuildsTextDocumentForCurrentText()
    {
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]\nStrength=100");

        Ra2EditableDocumentSession updated = _service.UpdateText(session, "[E1]\nStrength=125\nArmor=none");

        Assert.Equal(3, updated.TextDocument.Lines.Count);
        Assert.Contains(updated.TextDocument.Lines, line => line.Key == "Armor" && line.Value == "none");
    }

    [Fact]
    public void UpdateText_PreservesEncodingMetadata()
    {
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.Utf16Le,
            "UTF-16 LE",
            hasBom: true);
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]\nStrength=100", metadata);

        Ra2EditableDocumentSession updated = _service.UpdateText(session, "[E1]\nStrength=125");

        Assert.Same(metadata, updated.DocumentState.EncodingMetadata);
    }

    [Fact]
    public void Revert_RestoresOriginalTextAndCleanState()
    {
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditableDocumentSession dirty = _service.UpdateText(session, "[E1]\nStrength=125");

        Ra2EditableDocumentSession reverted = _service.Revert(dirty);

        Assert.Equal("[E1]\nStrength=100", reverted.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, reverted.DocumentState.State);
        Assert.False(reverted.DocumentState.IsDirty);
        Assert.Equal("[E1]\nStrength=100", reverted.TextDocument.Text);
        Assert.Equal(session.DocumentId, reverted.DocumentId);
        Assert.Equal(2, reverted.EditRevision);
    }

    [Fact]
    public void UpdateText_WithSameTextPreservesRevision()
    {
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]");

        Ra2EditableDocumentSession unchanged = _service.UpdateText(session, "[E1]");

        Assert.Equal(session.DocumentId, unchanged.DocumentId);
        Assert.Equal(session.EditRevision, unchanged.EditRevision);
    }

    [Fact]
    public void MarkSaved_PreservesIdentityAndRevisionWhenCurrentTextIsSaved()
    {
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]");
        Ra2EditableDocumentSession dirty = _service.UpdateText(session, "[E1]\nStrength=100");

        Ra2EditableDocumentSession saved = _service.MarkSaved(dirty, dirty.DocumentState.CurrentText);

        Assert.Equal(session.DocumentId, saved.DocumentId);
        Assert.Equal(dirty.EditRevision, saved.EditRevision);
        Assert.False(saved.DocumentState.IsDirty);
    }

    [Fact]
    public void Revert_PreservesEncodingMetadata()
    {
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.SystemDefault,
            "GB18030",
            hasBom: false,
            codePageName: "GB18030");
        Ra2EditableDocumentSession session = _service.StartEditing("rulesmd.ini", "[E1]\nStrength=100", metadata);
        Ra2EditableDocumentSession dirty = _service.UpdateText(session, "[E1]\nStrength=125");

        Ra2EditableDocumentSession reverted = _service.Revert(dirty);

        Assert.Same(metadata, reverted.DocumentState.EncodingMetadata);
    }
}
