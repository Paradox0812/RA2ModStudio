using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2Hli1CHostBoundaryContractTests
{
    [Fact]
    public void HostAuthorityTypesRemainInternalAndSurfacesStayNarrow()
    {
        Type[] hostTypes =
        [
            typeof(IRa2IniAuthoringWorkspace),
            typeof(Ra2IniAuthoringWorkspace),
            typeof(Ra2IniEditPreview),
            typeof(IRa2IniEditPreviewService),
            typeof(Ra2IniEditApplyRequest),
            typeof(IRa2EditorTransactionPort)
        ];

        Assert.All(hostTypes, type => Assert.False(type.IsPublic));
        Assert.Equal(
            ["Apply", "InvalidateActivePreview", "Preview", "TryDiscardActivePreview"],
            typeof(IRa2IniAuthoringWorkspace).GetMethods().Select(method => method.Name).Order().ToArray());
        Assert.Equal(["Preview"], typeof(IRa2IniEditPreviewService).GetMethods().Select(method => method.Name).ToArray());
        Assert.Equal(["Apply"], typeof(IRa2EditorTransactionPort).GetMethods().Select(method => method.Name).ToArray());
        Assert.Equal(
            ["ExplicitConfirmationGranted", "PreviewId"],
            typeof(Ra2IniEditApplyRequest).GetProperties().Select(property => property.Name).Order().ToArray());
        Assert.Equal(29, typeof(Ra2AutomationEditPreviewService).Assembly.GetExportedTypes().Length);
    }

    [Fact]
    public void GatewayLikeAdapterEntersExistingWorkspaceAndPreviewAppliesOnlyOnce()
    {
        Fixture fixture = new();
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(new GatewayLikePreviewService(), port);
        Ra2IniEditPlan plan = fixture.Plan("Strength", "125");

        Ra2IniEditPreview preview = workspace.Preview(fixture.Snapshot, plan);
        Ra2IniEditApplyResult first = workspace.Apply(new(preview.PreviewId, true));
        Ra2IniEditApplyResult replay = workspace.Apply(new(preview.PreviewId, true));

        Assert.True(preview.Succeeded, preview.Message);
        Assert.True(first.Succeeded, first.Message);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, replay.OutcomeKind);
        Assert.Equal(1, port.CallCount);
    }

    [Fact]
    public void AutomationPreviewIdOutsideWorkspaceHasNoApplyAuthority()
    {
        Fixture fixture = new();
        Ra2IniEditPlan plan = fixture.Plan("Strength", "125");
        Ra2AutomationEditPreviewResult automation = new Ra2AutomationEditPreviewService().Preview(
            fixture.Snapshot.ToAutomationSnapshot(),
            plan);
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(new GatewayLikePreviewService(), port);

        Ra2IniEditApplyResult result = workspace.Apply(new(automation.PreviewId, true));

        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, result.OutcomeKind);
        Assert.Equal(0, port.CallCount);
    }

    [Fact]
    public void ProjectionRejectsForeignIdentity()
    {
        Fixture expected = new();
        Fixture foreign = new();
        Ra2IniEditPlan foreignPlan = foreign.Plan("Strength", "125");
        Ra2AutomationEditPreviewResult result = new Ra2AutomationEditPreviewService().Preview(
            foreign.Snapshot.ToAutomationSnapshot(),
            foreignPlan);

        Assert.Throws<ArgumentException>(() =>
            Ra2IniEditPreview.FromAutomation(expected.Snapshot, expected.Plan("Strength", "125"), result));
    }

    [Fact]
    public void ProjectionRejectsSamePlanIdWithDifferentOperationEvidence()
    {
        Fixture fixture = new();
        Guid planId = Guid.NewGuid();
        Ra2IniEditPlan expectedPlan = fixture.Plan("Strength", "125", planId);
        Ra2IniEditPlan alteredPlan = fixture.Plan("Strength", "150", planId);
        Ra2AutomationEditPreviewResult result = new Ra2AutomationEditPreviewService().Preview(
            fixture.Snapshot.ToAutomationSnapshot(),
            alteredPlan);

        Assert.Throws<ArgumentException>(() =>
            Ra2IniEditPreview.FromAutomation(fixture.Snapshot, expectedPlan, result));
    }

    [Fact]
    public void ProjectionRejectsChangesThatDoNotReproduceCandidateText()
    {
        Fixture fixture = new();
        Ra2IniEditPlan plan = fixture.Plan("Strength", "125");
        Ra2AutomationEditPreviewResult canonical = new Ra2AutomationEditPreviewService().Preview(
            fixture.Snapshot.ToAutomationSnapshot(),
            plan);
        Ra2AutomationEditPreviewResult inconsistent = new(
            fixture.Snapshot.ToAutomationSnapshot(),
            plan,
            Ra2IniEditPreviewFailureKind.None,
            canonical.Message,
            canonical.PreviewId,
            canonical.CandidateText + "; mismatch",
            canonical.Changes,
            canonical.OperationPreviews,
            canonical.AddedDiagnostics,
            canonical.RemovedDiagnostics);

        Assert.Throws<ArgumentException>(() =>
            Ra2IniEditPreview.FromAutomation(fixture.Snapshot, plan, inconsistent));
    }

    [Fact]
    public void ProjectionRejectsOperationEvidenceOutsideSnapshot()
    {
        Fixture fixture = new();
        Ra2IniEditPlan plan = fixture.Plan("Strength", "125");
        Ra2AutomationEditPreviewResult canonical = new Ra2AutomationEditPreviewService().Preview(
            fixture.Snapshot.ToAutomationSnapshot(),
            plan);
        Ra2AutomationEditOperationPreview evidence = canonical.OperationPreviews[0];
        Ra2AutomationEditOperationPreview outside = new(
            evidence.OperationIndex,
            evidence.Operation,
            evidence.OutcomeKind,
            evidence.ResolvedSectionKind,
            evidence.IsKnownField,
            evidence.FieldTrustLevel,
            new Ra2AutomationTextSpan(fixture.Snapshot.Text.Length + 1, 0),
            evidence.Summary);
        Ra2AutomationEditPreviewResult inconsistent = new(
            fixture.Snapshot.ToAutomationSnapshot(),
            plan,
            Ra2IniEditPreviewFailureKind.None,
            canonical.Message,
            canonical.PreviewId,
            canonical.CandidateText,
            canonical.Changes,
            [outside],
            canonical.AddedDiagnostics,
            canonical.RemovedDiagnostics);

        Assert.Throws<ArgumentException>(() =>
            Ra2IniEditPreview.FromAutomation(fixture.Snapshot, plan, inconsistent));
    }

    [Fact]
    public void WorkspaceRejectsForeignWrapperWithoutActivatingIt()
    {
        Fixture expected = new();
        Fixture foreign = new();
        Ra2IniEditPlan foreignPlan = foreign.Plan("Strength", "125");
        Ra2IniEditPreview foreignPreview = new GatewayLikePreviewService().Preview(foreign.Snapshot, foreignPlan);
        RecordingTransactionPort port = new(expected.Session);
        Ra2IniAuthoringWorkspace workspace = new(new FixedPreviewService(foreignPreview), port);

        Ra2IniEditPreview result = workspace.Preview(expected.Snapshot, expected.Plan("Strength", "125"));
        Ra2IniEditApplyResult apply = workspace.Apply(new(foreignPreview.PreviewId, true));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2IniEditPreviewFailureKind.UnexpectedFailure, result.FailureKind);
        Assert.Same(expected.Snapshot, result.Snapshot);
        Assert.Equal(Guid.Empty, result.PreviewId);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, apply.OutcomeKind);
        Assert.Equal(0, port.CallCount);
    }

    [Fact]
    public void FailedAndCanceledPreviewsNeverBecomeActive()
    {
        Fixture fixture = new();
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(new GatewayLikePreviewService(), port);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2IniEditPreview result = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan("Strength", "125"),
            cancellation.Token);
        Ra2IniEditApplyResult apply = workspace.Apply(new(result.PreviewId, true));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2IniEditPreviewFailureKind.Canceled, result.FailureKind);
        Assert.Equal(Guid.Empty, result.PreviewId);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, apply.OutcomeKind);
        Assert.Equal(0, port.CallCount);
    }

    [Fact]
    public void TransactionRejectionConsumesClaimedPreview()
    {
        Fixture fixture = new();
        RejectingTransactionPort port = new();
        Ra2IniAuthoringWorkspace workspace = new(new GatewayLikePreviewService(), port);
        Ra2IniEditPreview preview = workspace.Preview(fixture.Snapshot, fixture.Plan("Strength", "125"));

        Ra2IniEditApplyResult first = workspace.Apply(new(preview.PreviewId, true));
        Ra2IniEditApplyResult replay = workspace.Apply(new(preview.PreviewId, true));

        Assert.Equal(Ra2IniEditApplyOutcomeKind.TransactionRejected, first.OutcomeKind);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, replay.OutcomeKind);
        Assert.Equal(1, port.CallCount);
    }

    [Fact]
    public void NonFatalTransactionExceptionConsumesClaimedPreview()
    {
        Fixture fixture = new();
        ThrowingTransactionPort port = new();
        Ra2IniAuthoringWorkspace workspace = new(new GatewayLikePreviewService(), port);
        Ra2IniEditPreview preview = workspace.Preview(fixture.Snapshot, fixture.Plan("Strength", "125"));

        Ra2IniEditApplyResult first = workspace.Apply(new(preview.PreviewId, true));
        Ra2IniEditApplyResult replay = workspace.Apply(new(preview.PreviewId, true));

        Assert.Equal(Ra2IniEditApplyOutcomeKind.UnexpectedFailure, first.OutcomeKind);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, replay.OutcomeKind);
        Assert.Equal(1, port.CallCount);
    }

    private sealed class Fixture
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());

        public Fixture()
        {
            Session = _sessionService.StartEditing("rulesmd.ini", "[E1]\nStrength=100\n");
            Ra2FieldRegistryProviderSnapshot registry = new(
                new BuiltInRa2FieldDefinitionProvider(),
                revision: 1);
            Snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
                Ra2AuthoringSnapshot.Capture(
                    Session,
                    Session.DocumentState.CurrentText,
                    string.Empty,
                    registry).Snapshot);
        }

        public Ra2EditableDocumentSession Session { get; }
        public Ra2AuthoringSnapshot Snapshot { get; }

        public Ra2IniEditPlan Plan(string key, string value, Guid? planId = null)
            => new(
                planId ?? Guid.NewGuid(),
                Snapshot.DocumentId,
                Snapshot.EditRevision,
                Snapshot.FieldRegistry.Revision,
                [new Ra2IniEditOperation(Ra2IniEditOperationKind.ReplaceFieldValue, "E1", key, value)],
                "HLI-1C contract",
                "Tests");
    }

    private sealed class GatewayLikePreviewService : IRa2IniEditPreviewService
    {
        private readonly Ra2AutomationEditPreviewService _service = new();

        public Ra2IniEditPreview Preview(
            Ra2AuthoringSnapshot snapshot,
            Ra2IniEditPlan plan,
            CancellationToken cancellationToken = default)
            => Ra2IniEditPreview.FromAutomation(
                snapshot,
                plan,
                _service.Preview(snapshot.ToAutomationSnapshot(), plan, cancellationToken));
    }

    private sealed class FixedPreviewService : IRa2IniEditPreviewService
    {
        private readonly Ra2IniEditPreview _preview;

        public FixedPreviewService(Ra2IniEditPreview preview)
            => _preview = preview;

        public Ra2IniEditPreview Preview(
            Ra2AuthoringSnapshot snapshot,
            Ra2IniEditPlan plan,
            CancellationToken cancellationToken = default)
            => _preview;
    }

    private sealed class RecordingTransactionPort : IRa2EditorTransactionPort
    {
        private readonly Ra2EditableDocumentSessionService _service = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private Ra2EditableDocumentSession _session;

        public RecordingTransactionPort(Ra2EditableDocumentSession session)
            => _session = session;

        public int CallCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            CallCount++;
            _session = _service.UpdateText(_session, preview.CandidateText!);
            return Ra2IniEditApplyResult.Applied(preview, _session, 0, preview.CandidateText!.Length);
        }
    }

    private sealed class RejectingTransactionPort : IRa2EditorTransactionPort
    {
        public int CallCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            CallCount++;
            return Ra2IniEditApplyResult.TransactionRejected(preview.PreviewId);
        }
    }

    private sealed class ThrowingTransactionPort : IRa2EditorTransactionPort
    {
        public int CallCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            CallCount++;
            throw new InvalidOperationException("test-only failure");
        }
    }
}
