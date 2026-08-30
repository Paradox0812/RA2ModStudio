using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiAuthoringCoordinatorTests
{
    [Fact]
    public void PrepareProposal_BuildsNormalProposalBoundToWorkspacePreview()
    {
        Fixture fixture = new();

        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            fixture.Response("125"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(result.Proposal);
        Assert.Equal(Ra2AiEditProposalApplyPolicy.Normal, proposal.ApplyPolicy);
        Assert.Equal(fixture.Snapshot.DocumentId, proposal.Preview.Snapshot.DocumentId);
        Assert.Same(proposal, fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void PrepareProposal_RejectsMultipleCallsBeforePreview()
    {
        Fixture fixture = new();
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls(
            [fixture.Call("125"), fixture.Call("150", "call-2")]);

        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            response,
            CancellationToken.None);

        Assert.Equal(Ra2AiEditProposalFailureKind.MultipleToolCalls, result.FailureKind);
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void PrepareProposal_RejectsChangedDocumentOrRegistryContext()
    {
        Fixture fixture = new();
        Ra2AuthoringSnapshot changed = fixture.CreateChangedSnapshot("[E1]\nStrength=101\n");

        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            changed,
            fixture.Response("125"),
            CancellationToken.None);

        Assert.Equal(Ra2AiEditProposalFailureKind.RequestContextStale, result.FailureKind);
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void PrepareProposal_PreCancelledTokenDoesNotCreatePreview()
    {
        Fixture fixture = new();
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            fixture.Response("125"),
            source.Token);

        Assert.Equal(Ra2AiEditProposalFailureKind.PreviewCancelled, result.FailureKind);
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void PrepareProposal_ClarificationDoesNotCreatePreviewOrAuthority()
    {
        Fixture fixture = new();
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls(
        [
            new Ra2AiToolCall(
                "call-clarify",
                Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                """{"outcome":"needs_clarification","message":"请提供目标值。"}""")
        ]);

        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            response,
            CancellationToken.None);

        Assert.True(result.NeedsClarification);
        Assert.Null(result.Proposal);
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void PrepareProposal_DiagnosticErrorIsReviewEvidenceAndExplicitApplyRemainsAvailable()
    {
        Fixture fixture = new(injectCandidateError: true);

        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            fixture.Response("125"),
            CancellationToken.None);

        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(result.Proposal);
        Assert.Equal(Ra2AiEditProposalApplyPolicy.Caution, proposal.ApplyPolicy);
        Assert.True(proposal.Preview.AddedErrorCount > 0);

        Ra2AiEditProposalApplyResult apply = fixture.Coordinator.ApplyConfirmed(proposal);
        Assert.True(apply.Succeeded, apply.Message);
        Assert.Equal(1, fixture.TransactionPort.ApplyCallCount);
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void ApplyConfirmed_ConsumesProposalExactlyOnceThroughA3()
    {
        Fixture fixture = new();
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(
            fixture.Coordinator.PrepareProposal(
                new Ra2AiAuthoringRequestContext(fixture.Snapshot),
                fixture.Snapshot,
                fixture.Response("125"),
                CancellationToken.None).Proposal);

        Ra2AiEditProposalApplyResult first = fixture.Coordinator.ApplyConfirmed(proposal);
        Ra2AiEditProposalApplyResult replay = fixture.Coordinator.ApplyConfirmed(proposal);

        Assert.True(first.Succeeded);
        Assert.Equal(1, fixture.TransactionPort.ApplyCallCount);
        Assert.Equal(Ra2AiEditProposalFailureKind.RequestContextStale, replay.FailureKind);
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void Dismiss_OldProposalCannotClearNewProposal()
    {
        Fixture fixture = new();
        Ra2AiEditProposal first = Assert.IsType<Ra2AiEditProposal>(
            fixture.Coordinator.PrepareProposal(
                new Ra2AiAuthoringRequestContext(fixture.Snapshot),
                fixture.Snapshot,
                fixture.Response("125"),
                CancellationToken.None).Proposal);
        Ra2AiEditProposal second = Assert.IsType<Ra2AiEditProposal>(
            fixture.Coordinator.PrepareProposal(
                new Ra2AiAuthoringRequestContext(fixture.Snapshot),
                fixture.Snapshot,
                fixture.Response("150"),
                CancellationToken.None).Proposal);

        Assert.False(fixture.Coordinator.Dismiss(first));
        Assert.Same(second, fixture.Coordinator.ActiveProposal);
        Assert.True(fixture.Coordinator.Dismiss(second));
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void InvalidateActiveProposal_ReturnsInvalidatedIdentityAndPreventsApply()
    {
        Fixture fixture = new();
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(
            fixture.Coordinator.PrepareProposal(
                new Ra2AiAuthoringRequestContext(fixture.Snapshot),
                fixture.Snapshot,
                fixture.Response("125"),
                CancellationToken.None).Proposal);

        Ra2AiEditProposal? invalidated = fixture.Coordinator.InvalidateActiveProposal();
        Ra2AiEditProposalApplyResult apply = fixture.Coordinator.ApplyConfirmed(proposal);

        Assert.Same(proposal, invalidated);
        Assert.Equal(Ra2AiEditProposalFailureKind.RequestContextStale, apply.FailureKind);
        Assert.Equal(0, fixture.TransactionPort.ApplyCallCount);
    }

    private sealed class Fixture
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private readonly Ra2FieldRegistryProviderSnapshot _registry = new(
            new BuiltInRa2FieldDefinitionProvider(),
            revision: 11);

        public Fixture(bool injectCandidateError = false)
        {
            Session = _sessionService.StartEditing(
                "rulesmd.ini",
                "[InfantryTypes]\n1=E1\n\n[E1]\nStrength=100\n");
            Snapshot = Capture(Session);
            TransactionPort = new RecordingTransactionPort(Session);
            IRa2IniEditPreviewService previewService = new Ra2IniEditPreviewService(
                new Ra2IniLanguageAnalysisService(),
                new Ra2AddPropertyInsertPlanner());
            if (injectCandidateError)
                previewService = new ErrorInjectingPreviewService(previewService);
            Ra2IniAuthoringWorkspace workspace = new(
                previewService,
                TransactionPort);
            Coordinator = new Ra2AiAuthoringCoordinator(
                new Ra2AiAuthoringToolAdapter(),
                workspace);
        }

        public Ra2EditableDocumentSession Session { get; }

        public Ra2AuthoringSnapshot Snapshot { get; }

        public RecordingTransactionPort TransactionPort { get; }

        public Ra2AiAuthoringCoordinator Coordinator { get; }

        public Ra2AiToolCall Call(string value, string id = "call-1")
            => new(
                id,
                Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                $$"""
                  {
                    "outcome":"proposal",
                    "summary":"Update Strength",
                    "operations":[{
                      "kind":"replace_field_value",
                      "section":"E1",
                      "key":"Strength",
                      "value":{{System.Text.Json.JsonSerializer.Serialize(value)}}
                    }]
                  }
                  """);

        public Ra2AiResponse Response(string value)
            => Ra2AiResponse.CreateToolCalls([Call(value)]);

        public Ra2AuthoringSnapshot CreateChangedSnapshot(string text)
        {
            Ra2EditableDocumentSession changed = _sessionService.UpdateText(Session, text);
            return Capture(changed);
        }

        private Ra2AuthoringSnapshot Capture(Ra2EditableDocumentSession session)
            => Assert.IsType<Ra2AuthoringSnapshot>(
                Ra2AuthoringSnapshot.Capture(
                    session,
                    session.DocumentState.CurrentText,
                    string.Empty,
                    _registry).Snapshot);
    }

    private sealed class ErrorInjectingPreviewService : IRa2IniEditPreviewService
    {
        private readonly IRa2IniEditPreviewService _inner;

        public ErrorInjectingPreviewService(IRa2IniEditPreviewService inner)
            => _inner = inner;

        public Ra2IniEditPreview Preview(
            Ra2AuthoringSnapshot snapshot,
            Ra2IniEditPlan plan,
            CancellationToken cancellationToken = default)
        {
            Ra2IniEditPreview preview = _inner.Preview(snapshot, plan, cancellationToken);
            if (!preview.Succeeded)
                return preview;

            Ra2AutomationEditPreviewResult automation = preview.AutomationResult;
            List<Ra2AutomationDiagnosticFact> diagnostics = automation.AddedDiagnostics.ToList();
            diagnostics.Add(new Ra2AutomationDiagnosticFact(
                "A4_TEST_ERROR",
                "Test",
                IniIssueSeverity.Error,
                "Injected candidate error.",
                snapshot.FilePath,
                1,
                1,
                "E1",
                "Strength",
                snapshot.EditRevision));
            Ra2AutomationEditPreviewResult candidateWithError = new(
                snapshot.ToAutomationSnapshot(),
                plan,
                Ra2AutomationEditPreviewFailureKind.None,
                automation.Message,
                automation.PreviewId,
                automation.CandidateText,
                automation.Changes,
                automation.OperationPreviews,
                diagnostics,
                automation.RemovedDiagnostics);

            return Ra2IniEditPreview.FromAutomation(
                snapshot,
                plan,
                candidateWithError);
        }
    }

    private sealed class RecordingTransactionPort : IRa2EditorTransactionPort
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private Ra2EditableDocumentSession _session;

        public RecordingTransactionPort(Ra2EditableDocumentSession session)
            => _session = session;

        public int ApplyCallCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            ApplyCallCount++;
            _session = _sessionService.UpdateText(_session, preview.CandidateText!);
            return Ra2IniEditApplyResult.Applied(
                preview,
                _session,
                undoCaretOffset: 0,
                redoCaretOffset: preview.CandidateText!.Length);
        }
    }
}
