using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2Hli2CAgentLoopContractTests
{
    [Fact]
    public void GatewayLoop_QueryPreviewRequiresConfirmationAppliesOnceAndRevalidates()
    {
        const string originalText = "[InfantryTypes]\n1=E1\n\n[E1]\nStrength=100\n";
        Ra2EditableDocumentSessionService sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        Ra2EditableDocumentSession session = sessionService.StartEditing(
            "rulesmd.ini",
            originalText);
        Ra2FieldRegistryProviderSnapshot registry = new(
            new BuiltInRa2FieldDefinitionProvider(),
            revision: 17);
        Ra2AuthoringSnapshot snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
            Ra2AuthoringSnapshot.Capture(
                session,
                session.DocumentState.CurrentText,
                string.Empty,
                registry).Snapshot);
        IRa2AutomationCapabilityGateway gateway = new Ra2AutomationCapabilityGateway();
        Ra2AutomationDocumentSnapshot automationSnapshot = snapshot.ToAutomationSnapshot();

        Ra2AutomationSectionQueryResult sectionBefore = gateway.GetSection(
            automationSnapshot,
            new Ra2AutomationSectionQuery("E1"));
        Ra2AutomationDocumentDiagnosticsResult diagnosticsBefore = gateway.Validate(automationSnapshot);

        Assert.True(sectionBefore.Succeeded, sectionBefore.Message);
        Assert.Equal(snapshot.DocumentId, sectionBefore.DocumentId);
        Assert.Equal(snapshot.EditRevision, sectionBefore.Version);
        Assert.Equal(registry.Revision, sectionBefore.FieldRegistryRevision);
        Assert.Equal("100", Assert.Single(sectionBefore.Section!.Fields).EffectiveValue);
        Assert.True(diagnosticsBefore.Succeeded, diagnosticsBefore.Message);
        Assert.Equal(sectionBefore.DocumentId, diagnosticsBefore.DocumentId);
        Assert.Equal(sectionBefore.Version, diagnosticsBefore.Version);
        Assert.Equal(sectionBefore.FieldRegistryRevision, diagnosticsBefore.FieldRegistryRevision);

        Ra2IniEditPlan plan = new(
            Guid.NewGuid(),
            snapshot.DocumentId,
            snapshot.EditRevision,
            snapshot.FieldRegistry.Revision,
            [new Ra2IniEditOperation(
                Ra2IniEditOperationKind.ReplaceFieldValue,
                "E1",
                "Strength",
                "150")],
            "Update Strength",
            "HLI-2C contract");
        RecordingTransactionPort transactionPort = new(sessionService, session);
        Ra2IniAuthoringWorkspace workspace = new(
            new Ra2IniEditPreviewService(gateway),
            transactionPort);

        Ra2IniEditPreview preview = workspace.Preview(snapshot, plan);
        Ra2IniEditApplyResult withoutConfirmation = workspace.Apply(
            new Ra2IniEditApplyRequest(preview.PreviewId, explicitConfirmationGranted: false));
        Ra2IniEditApplyResult applied = workspace.Apply(
            new Ra2IniEditApplyRequest(preview.PreviewId, explicitConfirmationGranted: true));
        Ra2IniEditApplyResult replay = workspace.Apply(
            new Ra2IniEditApplyRequest(preview.PreviewId, explicitConfirmationGranted: true));

        Assert.True(preview.Succeeded, preview.Message);
        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.ConfirmationRequired, withoutConfirmation.OutcomeKind);
        Assert.True(applied.Succeeded, applied.Message);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, replay.OutcomeKind);
        Assert.Equal(1, transactionPort.CallCount);
        Assert.Equal(originalText, applied.UndoText);
        Assert.Equal(preview.CandidateText, applied.RedoText);
        Assert.Equal(preview.CandidateText, applied.TextToSyncToEditor);
        Assert.True(applied.IsDirtyAfterApply);
        Assert.Equal(1, applied.OperationCount);

        Ra2EditableDocumentSession updatedSession = Assert.IsType<Ra2EditableDocumentSession>(
            applied.UpdatedSession);
        Assert.Equal(snapshot.DocumentId, updatedSession.DocumentId);
        Assert.Equal(snapshot.EditRevision + 1, updatedSession.EditRevision);
        Assert.Equal(originalText, updatedSession.DocumentState.OriginalText);
        Assert.Equal(preview.CandidateText, updatedSession.DocumentState.CurrentText);
        Assert.True(updatedSession.DocumentState.IsDirty);

        Ra2AuthoringSnapshot updatedSnapshot = Assert.IsType<Ra2AuthoringSnapshot>(
            Ra2AuthoringSnapshot.Capture(
                updatedSession,
                updatedSession.DocumentState.CurrentText,
                string.Empty,
                registry).Snapshot);
        Ra2AutomationDocumentSnapshot updatedAutomationSnapshot = updatedSnapshot.ToAutomationSnapshot();
        Ra2AutomationSectionQueryResult sectionAfter = gateway.GetSection(
            updatedAutomationSnapshot,
            new Ra2AutomationSectionQuery("E1"));
        Ra2AutomationDocumentDiagnosticsResult diagnosticsAfter = gateway.Validate(
            updatedAutomationSnapshot);

        Assert.True(sectionAfter.Succeeded, sectionAfter.Message);
        Assert.Equal("150", Assert.Single(sectionAfter.Section!.Fields).EffectiveValue);
        Assert.True(diagnosticsAfter.Succeeded, diagnosticsAfter.Message);
        Assert.Equal(snapshot.DocumentId, diagnosticsAfter.DocumentId);
        Assert.Equal(snapshot.EditRevision + 1, diagnosticsAfter.Version);
        Assert.Equal(registry.Revision, diagnosticsAfter.FieldRegistryRevision);
    }

    private sealed class RecordingTransactionPort : IRa2EditorTransactionPort
    {
        private readonly Ra2EditableDocumentSessionService _sessionService;
        private Ra2EditableDocumentSession _session;

        public RecordingTransactionPort(
            Ra2EditableDocumentSessionService sessionService,
            Ra2EditableDocumentSession session)
        {
            _sessionService = sessionService;
            _session = session;
        }

        public int CallCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            CallCount++;
            _session = _sessionService.UpdateText(_session, preview.CandidateText!);
            return Ra2IniEditApplyResult.Applied(
                preview,
                _session,
                undoCaretOffset: 0,
                redoCaretOffset: preview.CandidateText!.Length);
        }
    }
}
