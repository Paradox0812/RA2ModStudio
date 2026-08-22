using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniEditPreviewCurrencyEvaluatorTests
{
    private readonly Ra2EditableDocumentSessionService _sessionService = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());
    private readonly Ra2IniEditPreviewService _previewService = new(
        new Ra2IniLanguageAnalysisService(),
        new Ra2AddPropertyInsertPlanner());
    private readonly Ra2IniEditPreviewCurrencyEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_CurrentPreviewPassesAllGates()
    {
        (Ra2EditableDocumentSession session, Ra2IniEditPreview preview) = Preview();

        Ra2IniEditPreviewCurrencyResult result = _evaluator.Evaluate(
            preview,
            session,
            session.DocumentState.CurrentText,
            preview.Snapshot.FieldRegistry.Revision);

        Assert.True(result.IsCurrent);
        Assert.Equal(Ra2IniEditPreviewCurrencyKind.Current, result.Kind);
    }

    [Fact]
    public void Evaluate_RejectsIdentityRevisionAndBothTextDrifts()
    {
        (Ra2EditableDocumentSession session, Ra2IniEditPreview preview) = Preview();
        Ra2EditableDocumentSession otherSession = _sessionService.StartEditing(
            "rulesmd.ini",
            session.DocumentState.CurrentText);
        Ra2EditableDocumentSession edited = _sessionService.UpdateText(
            session,
            session.DocumentState.CurrentText + "\nArmor=steel");

        Assert.Equal(
            Ra2IniEditPreviewCurrencyKind.DocumentIdentityChanged,
            Evaluate(preview, otherSession, otherSession.DocumentState.CurrentText).Kind);
        Assert.Equal(
            Ra2IniEditPreviewCurrencyKind.EditRevisionChanged,
            Evaluate(preview, edited, edited.DocumentState.CurrentText).Kind);

        Ra2EditableDocumentState driftedState = new(
            session.DocumentState.FilePath,
            session.DocumentState.OriginalText,
            session.DocumentState.CurrentText + " ",
            Ra2EditorDocumentState.EditableDirty);
        Ra2EditableDocumentSession sessionTextDrift = session.ContinueWith(
            driftedState,
            new Ra2IniTextDocumentParser().Parse(driftedState.CurrentText));
        Assert.Equal(
            Ra2IniEditPreviewCurrencyKind.EditRevisionChanged,
            Evaluate(preview, sessionTextDrift, sessionTextDrift.DocumentState.CurrentText).Kind);

        Assert.Equal(
            Ra2IniEditPreviewCurrencyKind.EditorTextChanged,
            Evaluate(preview, session, session.DocumentState.CurrentText + " ").Kind);
    }

    [Fact]
    public void Evaluate_RejectsRegistryReloadMissingSessionAndFailedPreview()
    {
        (Ra2EditableDocumentSession session, Ra2IniEditPreview preview) = Preview();
        Ra2IniEditPreview failed = Ra2IniEditPreview.Failed(
            preview.Snapshot,
            preview.Plan,
            Ra2IniEditPreviewFailureKind.NoChanges,
            "No changes.");

        Assert.Equal(
            Ra2IniEditPreviewCurrencyKind.FieldRegistryChanged,
            _evaluator.Evaluate(
                preview,
                session,
                session.DocumentState.CurrentText,
                preview.Snapshot.FieldRegistry.Revision + 1).Kind);
        Assert.Equal(
            Ra2IniEditPreviewCurrencyKind.NoEditableSession,
            _evaluator.Evaluate(
                preview,
                null,
                session.DocumentState.CurrentText,
                preview.Snapshot.FieldRegistry.Revision).Kind);
        Assert.Equal(
            Ra2IniEditPreviewCurrencyKind.PreviewFailed,
            _evaluator.Evaluate(
                failed,
                session,
                session.DocumentState.CurrentText,
                preview.Snapshot.FieldRegistry.Revision).Kind);
    }

    private Ra2IniEditPreviewCurrencyResult Evaluate(
        Ra2IniEditPreview preview,
        Ra2EditableDocumentSession session,
        string editorText)
        => _evaluator.Evaluate(
            preview,
            session,
            editorText,
            preview.Snapshot.FieldRegistry.Revision);

    private (Ra2EditableDocumentSession Session, Ra2IniEditPreview Preview) Preview()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing(
            "rulesmd.ini",
            "[E1]\nStrength=100");
        Ra2FieldRegistryProviderSnapshot registry = new(
            new BuiltInRa2FieldDefinitionProvider(),
            revision: 4);
        Ra2AuthoringSnapshot snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
            Ra2AuthoringSnapshot.Capture(
                session,
                session.DocumentState.CurrentText,
                string.Empty,
                registry).Snapshot);
        Ra2IniEditPlan plan = new(
            Guid.NewGuid(),
            snapshot.DocumentId,
            snapshot.EditRevision,
            registry.Revision,
            [new Ra2IniEditOperation(
                Ra2IniEditOperationKind.ReplaceFieldValue,
                "E1",
                "Strength",
                "125")],
            "Replace Strength",
            "Tests");
        Ra2IniEditPreview preview = _previewService.Preview(snapshot, plan);
        Assert.True(preview.Succeeded);
        return (session, preview);
    }
}
