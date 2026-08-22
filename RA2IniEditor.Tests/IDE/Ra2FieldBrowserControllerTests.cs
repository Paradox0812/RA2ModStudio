using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Controllers.FieldBrowser;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldBrowserControllerTests
{
    [Fact]
    public void CreateAddPropertyViewModel_LoadsAnnotationsAndUsesInitialSection()
    {
        Ra2FieldBrowserController controller = CreateController();
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse("[E1]\n");

        Ra2AddPropertyOpenResult result = controller.CreateAddPropertyViewModel(new Ra2AddPropertyOpenRequest(
            new TestFieldProvider(),
            new TestAnnotationStore(),
            "project.annotations.json",
            Ra2SectionKind.Infantry,
            Ra2EditorDocumentState.EditableClean,
            new Ra2RecentFieldUsageTracker(),
            document,
            0));

        Assert.NotNull(result.ViewModel);
        Assert.Equal(Ra2SectionKind.Infantry, result.ViewModel.SelectedSectionKind);
        Assert.True(result.AnnotationLoadResult.Success);
    }

    [Fact]
    public void ConfirmAddProperty_WhenNoEditableSessionRequiresEditableFile()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel();
        Ra2FieldBrowserController controller = CreateController();

        Ra2AddPropertyConfirmationResult result = controller.ConfirmAddProperty(
            new Ra2AddPropertyConfirmationRequest(viewModel, hasEditableSession: false));

        Assert.Equal(Ra2AddPropertyConfirmationAction.RequiresEditMode, result.Action);
        Assert.Contains("no editable file", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyInsertDuplicate_UpdatesSessionTextCaretAndMessage()
    {
        Ra2AddPropertyViewModel viewModel = CreateViewModel();
        viewModel.OptionText = "Strength";
        viewModel.ValueText = "125";
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.SystemDefault,
            "GB18030",
            hasBom: false,
            codePageName: "GB18030");
        Ra2EditableDocumentSession session = CreateSession("[E1]\n", metadata);
        Ra2FieldBrowserController controller = CreateController();

        Ra2FieldBrowserActionResult result = controller.ApplyInsertDuplicate(
            new Ra2AddPropertyApplyRequest(viewModel, session, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.UpdatedSession);
        Assert.Contains("Strength=125", result.UpdatedSession!.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.UpdatedSession.DocumentState.State);
        Assert.Equal(session.DocumentId, result.UpdatedSession.DocumentId);
        Assert.Equal(session.EditRevision + 1, result.UpdatedSession.EditRevision);
        Assert.NotNull(result.UpdatedText);
        Assert.Contains("Strength=125", result.UpdatedText);
        Assert.True(result.CaretOffset > 0);
        Assert.Contains("Added field 'Strength'", result.Message);
        Assert.Same(metadata, result.UpdatedSession.DocumentState.EncodingMetadata);
    }

    [Fact]
    public void ApplyReplaceExisting_UpdatesExistingValueOnly()
    {
        const string text = "[E1]\nStrength=100 ; keep\nArmor=none\n";
        Ra2AddPropertyViewModel viewModel = CreateViewModel();
        viewModel.OptionText = "Strength";
        viewModel.ValueText = "125";
        Ra2EditableDocumentSession session = CreateSession(text);
        Ra2DuplicateKeyMatch match = new Ra2DuplicateKeyDetector()
            .FindInCurrentSection(session.TextDocument, text.IndexOf("Armor", StringComparison.Ordinal), "Strength")!;
        Ra2FieldBrowserController controller = CreateController();

        Ra2FieldBrowserActionResult result = controller.ApplyReplaceExisting(
            new Ra2AddPropertyReplaceApplyRequest(viewModel, session, match));

        Assert.True(result.Success);
        Assert.NotNull(result.UpdatedSession);
        Assert.Contains("Strength=125 ; keep", result.UpdatedSession!.DocumentState.CurrentText);
        Assert.DoesNotContain("Strength=100", result.UpdatedSession.DocumentState.CurrentText);
        Assert.Contains("Replaced field 'Strength'", result.Message);
    }

    private static Ra2FieldBrowserController CreateController()
        => new(
            new Ra2AddPropertyInsertPlanner(),
            new Ra2TextChangeApplier(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService()));

    private static Ra2EditableDocumentSession CreateSession(
        string text,
        Ra2EditorTextEncodingMetadata? encodingMetadata = null)
    {
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);
        Ra2EditableDocumentState state = new(
            "rules.ini",
            text,
            text,
            Ra2EditorDocumentState.EditableClean,
            encodingMetadata);
        return new Ra2EditableDocumentSession(state, document);
    }

    private static Ra2AddPropertyViewModel CreateViewModel()
        => new(
            new Ra2FieldDisplayResolver(
                new TestFieldProvider(),
                new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty())),
            Ra2SectionKind.Infantry,
            Ra2EditorDocumentState.EditableClean,
            annotationStatus: new Ra2FieldAnnotationStatusViewModel("Annotations loaded.", true, false),
            recentFieldUsageTracker: new Ra2RecentFieldUsageTracker(),
            document: new Ra2IniTextDocumentParser().Parse("[E1]\n"),
            caretOffset: 0);

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

    private sealed class TestAnnotationStore : IRa2FieldAnnotationStore
    {
        public Ra2FieldAnnotationLoadResult Load(string path)
            => new(Ra2FieldAnnotationPack.Empty());

        public Ra2FieldAnnotationSaveResult Save(string path, Ra2FieldAnnotationPack pack)
            => Ra2FieldAnnotationSaveResult.Succeeded();
    }
}
