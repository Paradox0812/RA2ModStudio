using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Controllers.Completion;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionInteractionControllerTests
{
    [Fact]
    public void OpenCompletions_UsesProviderAndReturnsStatusMessage()
    {
        const string text = "[E1]\nStr";
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 1),
            new TestFieldProvider());
        int caretOffset = text.Length;
        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, caretOffset);
        Ra2CompletionInteractionController controller = new(
            new TestCompletionProvider(new Ra2CompletionResult(
                [new Ra2CompletionItem("Strength", Ra2CompletionItemKind.Key, insertText: "Strength=")],
                new Ra2TextSpan(text.IndexOf("Str", StringComparison.Ordinal), 3))),
            new Ra2CompletionDisplayEnhancer(),
            new TestCommitCoordinator());

        Ra2CompletionOpenResult result = controller.OpenCompletions(new Ra2CompletionOpenRequest(
            model,
            context,
            caretOffset,
            new TestFieldProvider(),
            Ra2SectionKind.Infantry,
            new TestFieldDisplayResolver()));

        Assert.Single(result.CompletionResult.Items);
        Assert.Equal("Completion dropdown opened with 1 item(s).", result.Message);
    }

    [Fact]
    public void TryCommit_WhenNotEditingReturnsCloseableFailure()
    {
        Ra2CompletionInteractionController controller = new(
            new TestCompletionProvider(Ra2CompletionResult.EmptyAt(0)),
            new Ra2CompletionDisplayEnhancer(),
            new TestCommitCoordinator());

        Ra2CompletionCommitInteractionResult result = controller.TryCommit(
            new Ra2CompletionCommitInteractionRequest(null, null, null, null));

        Assert.False(result.Success);
        Assert.True(result.ShouldCloseDropdown);
        Assert.Equal("Completion commit skipped: no editable file is currently open.", result.Message);
    }

    [Fact]
    public void TryCommit_WhenCoordinatorSucceedsReturnsSessionCaretAndMessage()
    {
        Ra2EditableDocumentSession nextSession = CreateSession("Strength=", Ra2EditorDocumentState.EditableDirty);
        Ra2CompletionInteractionController controller = new(
            new TestCompletionProvider(Ra2CompletionResult.EmptyAt(0)),
            new Ra2CompletionDisplayEnhancer(),
            new TestCommitCoordinator(Ra2CompletionCommitApplyResult.Succeeded(nextSession, "Strength=".Length)));
        Ra2CompletionItem item = new("Strength", Ra2CompletionItemKind.Key, insertText: "Strength=");

        Ra2CompletionCommitInteractionResult result = controller.TryCommit(
            new Ra2CompletionCommitInteractionRequest(
                CreateSession("Str", Ra2EditorDocumentState.EditableClean),
                new Ra2CompletionResult([item], new Ra2TextSpan(0, 3)),
                item,
                "Strength"));

        Assert.True(result.Success);
        Assert.True(result.ShouldCloseDropdown);
        Assert.Same(nextSession, result.Session);
        Assert.Equal("Strength=".Length, result.CaretOffset);
        Assert.Equal("Committed completion 'Strength' in memory.", result.Message);
    }

    private static Ra2EditableDocumentSession CreateSession(string currentText, Ra2EditorDocumentState state)
    {
        Ra2EditableDocumentState documentState = new("rulesmd.ini", currentText, currentText, state);
        return new Ra2EditableDocumentSession(
            documentState,
            new Ra2IniTextDocumentParser().Parse(currentText));
    }

    private sealed class TestCompletionProvider : IRa2CompletionProvider
    {
        private readonly Ra2CompletionResult _result;

        public TestCompletionProvider(Ra2CompletionResult result)
        {
            _result = result;
        }

        public Ra2CompletionResult GetCompletions(Ra2CompletionRequest request)
            => _result;
    }

    private sealed class TestCommitCoordinator : IRa2CompletionCommitCoordinator
    {
        private readonly Ra2CompletionCommitApplyResult? _result;

        public TestCommitCoordinator(Ra2CompletionCommitApplyResult? result = null)
        {
            _result = result;
        }

        public Ra2CompletionCommitApplyResult TryCommit(
            Ra2EditableDocumentSession session,
            Ra2CompletionResult completionResult,
            Ra2CompletionItem selectedItem)
            => _result ?? Ra2CompletionCommitApplyResult.Failed("Test failure.");
    }

    private sealed class TestFieldProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => false;
    }

    private sealed class TestFieldDisplayResolver : IRa2FieldDisplayResolver
    {
        public Ra2FieldDisplayInfo Resolve(Ra2SectionKind sectionKind, string key)
            => new(key, key, [], null, null, "Unknown", "Unknown", "Unknown", hasUserAnnotation: false);

        public IReadOnlyList<Ra2FieldDisplayInfo> GetFields(Ra2SectionKind sectionKind)
            => [];
    }
}
