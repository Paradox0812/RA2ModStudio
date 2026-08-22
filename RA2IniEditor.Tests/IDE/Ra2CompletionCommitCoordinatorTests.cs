using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionCommitCoordinatorTests
{
    private readonly Ra2CompletionCommitCoordinator _coordinator = new(
        new Ra2CompletionCommitPlanner(),
        new Ra2TextChangeApplier(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService()));

    [Fact]
    public void TryCommit_KeyCompletionReplacesPrefixAndMarksSessionDirty()
    {
        Ra2EditableDocumentSession session = CreateSession("Str", Ra2EditorDocumentState.EditableClean);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key, insertText: "Strength=")],
            new Ra2TextSpan(0, 3));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.True(result.Success);
        Assert.Equal("Strength=", result.Session!.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.Session.DocumentState.State);
        Assert.Equal(session.DocumentId, result.Session.DocumentId);
        Assert.Equal(session.EditRevision + 1, result.Session.EditRevision);
        Assert.Equal("Strength=".Length, result.CaretOffset);
    }

    [Fact]
    public void TryCommit_KeyCompletionBeforeExistingEqualsDoesNotDuplicateEquals()
    {
        Ra2EditableDocumentSession session = CreateSession("Pr=120mm", Ra2EditorDocumentState.EditableClean);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("Primary", Ra2CompletionItemKind.Key, insertText: "Primary")],
            new Ra2TextSpan(0, 2));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.True(result.Success);
        Assert.Equal("Primary=120mm", result.Session!.DocumentState.CurrentText);
        Assert.Equal("Primary".Length, result.CaretOffset);
        Assert.DoesNotContain("==", result.Session.DocumentState.CurrentText);
    }

    [Fact]
    public void TryCommit_ReferenceCompletionInsertsAtEmptySpan()
    {
        Ra2EditableDocumentSession session = CreateSession("Primary=", Ra2EditorDocumentState.EditableClean);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("120mm", Ra2CompletionItemKind.Reference)],
            new Ra2TextSpan("Primary=".Length, 0));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.True(result.Success);
        Assert.Equal("Primary=120mm", result.Session!.DocumentState.CurrentText);
        Assert.Equal("Primary=120mm".Length, result.CaretOffset);
    }

    [Fact]
    public void TryCommit_ReplacementSpanCoversPrefixWithoutDuplicatingText()
    {
        Ra2EditableDocumentSession session = CreateSession("Primary=12", Ra2EditorDocumentState.EditableClean);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("120mm", Ra2CompletionItemKind.Reference)],
            new Ra2TextSpan("Primary=".Length, 2));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.True(result.Success);
        Assert.Equal("Primary=120mm", result.Session!.DocumentState.CurrentText);
        Assert.DoesNotContain("12120mm", result.Session.DocumentState.CurrentText);
    }

    [Fact]
    public void TryCommit_RebuildsTextFirstModel()
    {
        Ra2EditableDocumentSession session = CreateSession("[E1]\nStr=100", Ra2EditorDocumentState.EditableClean);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key)],
            new Ra2TextSpan("[E1]\n".Length, 3));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.True(result.Success);
        Assert.Contains(result.Session!.TextDocument.KeyValues, line => line.Key == "Strength" && line.Value == "100");
    }

    [Fact]
    public void TryCommit_ApplierFailureReturnsFailureWithoutSession()
    {
        Ra2EditableDocumentSession session = CreateSession("Str", Ra2EditorDocumentState.EditableClean);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key)],
            new Ra2TextSpan(99, 3));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.False(result.Success);
        Assert.Null(result.Session);
        Assert.Equal(0, result.CaretOffset);
        Assert.Contains("beyond", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryCommit_ReadOnlyPreviewReturnsFailure()
    {
        Ra2EditableDocumentSession session = CreateSession("Str", Ra2EditorDocumentState.ReadOnlyPreview);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key)],
            new Ra2TextSpan(0, 3));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.False(result.Success);
        Assert.Null(result.Session);
        Assert.Contains("read-only preview", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryCommit_PreservesEncodingMetadata()
    {
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.Utf16Be,
            "UTF-16 BE",
            hasBom: true);
        Ra2EditableDocumentSession session = CreateSession("Str", Ra2EditorDocumentState.EditableClean, metadata);
        Ra2CompletionResult completionResult = new(
            [new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key, insertText: "Strength=")],
            new Ra2TextSpan(0, 3));

        Ra2CompletionCommitApplyResult result = _coordinator.TryCommit(
            session,
            completionResult,
            completionResult.Items[0]);

        Assert.True(result.Success);
        Assert.Same(metadata, result.Session!.DocumentState.EncodingMetadata);
    }

    private static Ra2EditableDocumentSession CreateSession(
        string currentText,
        Ra2EditorDocumentState state,
        Ra2EditorTextEncodingMetadata? encodingMetadata = null)
    {
        Ra2EditableDocumentState documentState = new(
            "rulesmd.ini",
            currentText,
            currentText,
            state,
            encodingMetadata);
        return new Ra2EditableDocumentSession(
            documentState,
            new Ra2IniTextDocumentParser().Parse(currentText));
    }
}
