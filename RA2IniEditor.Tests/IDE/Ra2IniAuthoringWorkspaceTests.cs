using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniAuthoringWorkspaceTests
{
    [Fact]
    public void Apply_RequiresConfirmationThenConsumesPreviewExactlyOnce()
    {
        Fixture fixture = new("[E1]\nStrength=100\n");
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(fixture.PreviewService, port);
        Ra2IniEditPreview preview = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan("Strength", "125"));

        Ra2IniEditApplyResult unconfirmed = workspace.Apply(
            new Ra2IniEditApplyRequest(preview.PreviewId, explicitConfirmationGranted: false));
        Ra2IniEditApplyResult applied = workspace.Apply(
            new Ra2IniEditApplyRequest(preview.PreviewId, explicitConfirmationGranted: true));
        Ra2IniEditApplyResult replay = workspace.Apply(
            new Ra2IniEditApplyRequest(preview.PreviewId, explicitConfirmationGranted: true));

        Assert.Equal(Ra2IniEditApplyOutcomeKind.ConfirmationRequired, unconfirmed.OutcomeKind);
        Assert.True(applied.Succeeded);
        Assert.Equal(1, port.ApplyCallCount);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, replay.OutcomeKind);
    }

    [Fact]
    public void Preview_ReplacingActiveSlotInvalidatesPreviousPreview()
    {
        Fixture fixture = new("[E1]\nStrength=100\n");
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(fixture.PreviewService, port);
        Ra2IniEditPreview first = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan("Strength", "125"));
        Ra2IniEditPreview second = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan("Strength", "150"));

        Ra2IniEditApplyResult firstApply = workspace.Apply(
            new Ra2IniEditApplyRequest(first.PreviewId, explicitConfirmationGranted: true));
        Ra2IniEditApplyResult secondApply = workspace.Apply(
            new Ra2IniEditApplyRequest(second.PreviewId, explicitConfirmationGranted: true));

        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, firstApply.OutcomeKind);
        Assert.True(secondApply.Succeeded);
        Assert.Equal(1, port.ApplyCallCount);
    }

    [Fact]
    public void InvalidateActivePreview_PreventsLaterApply()
    {
        Fixture fixture = new("[E1]\nStrength=100\n");
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(fixture.PreviewService, port);
        Ra2IniEditPreview preview = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan("Strength", "125"));

        workspace.InvalidateActivePreview();
        Ra2IniEditApplyResult result = workspace.Apply(
            new Ra2IniEditApplyRequest(preview.PreviewId, explicitConfirmationGranted: true));

        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, result.OutcomeKind);
        Assert.Equal(0, port.ApplyCallCount);
    }

    [Fact]
    public void TryDiscardActivePreview_OnlyClearsMatchingPreview()
    {
        Fixture fixture = new("[E1]\nStrength=100\n");
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(fixture.PreviewService, port);
        Ra2IniEditPreview first = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan("Strength", "125"));
        Ra2IniEditPreview second = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan("Strength", "150"));

        Assert.False(workspace.TryDiscardActivePreview(first.PreviewId));
        Assert.True(workspace.TryDiscardActivePreview(second.PreviewId));
        Assert.False(workspace.TryDiscardActivePreview(second.PreviewId));

        Ra2IniEditApplyResult result = workspace.Apply(
            new Ra2IniEditApplyRequest(second.PreviewId, explicitConfirmationGranted: true));
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, result.OutcomeKind);
        Assert.Equal(0, port.ApplyCallCount);
    }

    [Fact]
    public async Task Preview_WhenOlderGenerationCompletesLast_DoesNotReplaceNewerPreview()
    {
        Fixture fixture = new("[E1]\nStrength=100\n");
        BlockingFirstPreviewService service = new(fixture.PreviewService);
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(service, port);
        Ra2IniEditPlan firstPlan = fixture.Plan("Strength", "125");
        Ra2IniEditPlan secondPlan = fixture.Plan("Strength", "150");

        Task<Ra2IniEditPreview> firstTask = Task.Run(
            () => workspace.Preview(fixture.Snapshot, firstPlan));
        Assert.True(service.WaitUntilFirstCallEntered(TimeSpan.FromSeconds(5)));
        Ra2IniEditPreview second = workspace.Preview(fixture.Snapshot, secondPlan);
        service.ReleaseFirstCall();
        Ra2IniEditPreview first = await firstTask;

        Ra2IniEditApplyResult firstApply = workspace.Apply(
            new Ra2IniEditApplyRequest(first.PreviewId, explicitConfirmationGranted: true));
        Ra2IniEditApplyResult secondApply = workspace.Apply(
            new Ra2IniEditApplyRequest(second.PreviewId, explicitConfirmationGranted: true));

        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, firstApply.OutcomeKind);
        Assert.True(secondApply.Succeeded);
        Assert.Equal(1, port.ApplyCallCount);
    }

    private sealed class Fixture
    {
        private readonly Ra2FieldRegistryProviderSnapshot _registry = new(
            new BuiltInRa2FieldDefinitionProvider(),
            revision: 1);

        public Fixture(string text)
        {
            SessionService = new Ra2EditableDocumentSessionService(
                new Ra2IniTextDocumentParser(),
                new Ra2DirtyStateService());
            Session = SessionService.StartEditing("rulesmd.ini", text);
            Snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
                Ra2AuthoringSnapshot.Capture(Session, text, string.Empty, _registry).Snapshot);
            PreviewService = new Ra2IniEditPreviewService(
                new Ra2IniLanguageAnalysisService(),
                new Ra2AddPropertyInsertPlanner());
        }

        public Ra2EditableDocumentSessionService SessionService { get; }

        public Ra2EditableDocumentSession Session { get; }

        public Ra2AuthoringSnapshot Snapshot { get; }

        public IRa2IniEditPreviewService PreviewService { get; }

        public Ra2IniEditPlan Plan(string key, string value)
            => new(
                Guid.NewGuid(),
                Snapshot.DocumentId,
                Snapshot.EditRevision,
                Snapshot.FieldRegistry.Revision,
                [
                    new Ra2IniEditOperation(
                        Ra2IniEditOperationKind.ReplaceFieldValue,
                        "E1",
                        key,
                        value)
                ],
                "Workspace test",
                "Tests");
    }

    private sealed class RecordingTransactionPort : IRa2EditorTransactionPort
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private Ra2EditableDocumentSession _session;

        public RecordingTransactionPort(Ra2EditableDocumentSession session)
        {
            _session = session;
        }

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

    private sealed class BlockingFirstPreviewService : IRa2IniEditPreviewService
    {
        private readonly IRa2IniEditPreviewService _inner;
        private readonly ManualResetEventSlim _firstCallEntered = new(false);
        private readonly ManualResetEventSlim _releaseFirstCall = new(false);
        private int _callCount;

        public BlockingFirstPreviewService(IRa2IniEditPreviewService inner)
        {
            _inner = inner;
        }

        public Ra2IniEditPreview Preview(
            Ra2AuthoringSnapshot snapshot,
            Ra2IniEditPlan plan,
            CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _callCount) == 1)
            {
                _firstCallEntered.Set();
                _releaseFirstCall.Wait(cancellationToken);
            }

            return _inner.Preview(snapshot, plan, cancellationToken);
        }

        public bool WaitUntilFirstCallEntered(TimeSpan timeout)
            => _firstCallEntered.Wait(timeout);

        public void ReleaseFirstCall()
            => _releaseFirstCall.Set();
    }
}
