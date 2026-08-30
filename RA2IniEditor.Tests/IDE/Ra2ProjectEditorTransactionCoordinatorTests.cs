using System.Text;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ProjectEditorTransactionCoordinatorTests
{
    [Fact]
    public void Apply_PreparesAndCommitsTwoDocumentsAsOneInMemoryTransaction()
    {
        Fixture fixture = new();
        Ra2ProjectEditPreview preview = fixture.Preview();
        Ra2ProjectEditorTransactionCoordinator coordinator = fixture.Coordinator(fixture.SessionService);

        Ra2ProjectEditApplyResult result = coordinator.Apply(preview);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.AffectedDocumentCount);
        Assert.Equal(2, result.TotalWorkCount);
        Assert.Equal(2, result.DirtyDocumentCount);
        Assert.Equal("[E1]\nStrength=150\n", fixture.Editor.CurrentText);
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? rules));
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? art));
        Assert.Equal("[E1]\nStrength=150\n", rules!.DocumentState.CurrentText);
        Assert.Equal("[E1]\nStrength=250\n", art!.DocumentState.CurrentText);
        Assert.Equal(0, fixture.FileStore.WriteCount);
    }

    [Fact]
    public void Apply_WhenSecondPrepareFails_LeavesEverySessionUnchanged()
    {
        Fixture fixture = new();
        Ra2ProjectEditPreview preview = fixture.Preview();
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? rulesBefore));
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? artBefore));
        ThrowOnSecondUpdateSessionService throwing = new(fixture.SessionService);

        Ra2ProjectEditApplyResult result = fixture.Coordinator(throwing).Apply(preview);

        Assert.Equal(Ra2ProjectEditApplyOutcomeKind.PrepareFailed, result.OutcomeKind);
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? rulesAfter));
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? artAfter));
        Assert.Same(rulesBefore, rulesAfter);
        Assert.Same(artBefore, artAfter);
        Assert.Equal(preview.Snapshot.ProjectRevision, fixture.Store.ProjectRevision);
        Assert.Equal(0, fixture.FileStore.WriteCount);
    }

    [Fact]
    public void Apply_WhenActiveEditorSyncFails_RollsBackStoreAndEditor()
    {
        Fixture fixture = new();
        Ra2ProjectEditPreview preview = fixture.Preview();
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? rulesBefore));
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? artBefore));
        fixture.Editor.ThrowOnNextSet = true;

        Ra2ProjectEditApplyResult result = fixture.Coordinator(fixture.SessionService).Apply(preview);

        Assert.Equal(Ra2ProjectEditApplyOutcomeKind.EditorSynchronizationFailed, result.OutcomeKind);
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? rulesAfter));
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? artAfter));
        Assert.Same(rulesBefore, rulesAfter);
        Assert.Same(artBefore, artAfter);
        Assert.Equal("[E1]\nStrength=100\n", fixture.Editor.CurrentText);
        Assert.False(fixture.Editor.IsReadOnlyFailSafe);
        Assert.Equal(preview.Snapshot.ProjectRevision, fixture.Store.ProjectRevision);
        Assert.Equal(0, fixture.FileStore.WriteCount);
    }

    [Fact]
    public void Apply_WhenAnyDocumentOrProjectIsStale_RejectsWholeTransaction()
    {
        Fixture fixture = new();
        Ra2ProjectEditPreview preview = fixture.Preview();
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? rules));
        Assert.True(fixture.Store.TrySynchronizeActiveText(rules!, "[E1]\nStrength=101\n", out _, out _));

        Ra2ProjectEditApplyResult result = fixture.Coordinator(fixture.SessionService).Apply(preview);

        Assert.Equal(Ra2ProjectEditApplyOutcomeKind.Stale, result.OutcomeKind);
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? art));
        Assert.Equal("[E1]\nStrength=200\n", art!.DocumentState.CurrentText);
    }

    [Fact]
    public void Workspace_ProjectAndSinglePreviewsAreMutuallyExclusiveAndProjectIsSingleUse()
    {
        Fixture fixture = new();
        Ra2ProjectEditPreview projectPreview = fixture.Preview();
        RecordingPort port = new(fixture.Coordinator(fixture.SessionService));
        Ra2IniAuthoringWorkspace workspace = new(
            new Ra2IniEditPreviewService(new Ra2AutomationCapabilityGateway()),
            port,
            new Ra2ProjectEditPreviewService());

        Ra2ProjectEditPreview active = workspace.PreviewProject(projectPreview.Snapshot, projectPreview.Plan);
        Ra2ProjectEditApplyResult unconfirmed = workspace.ApplyProject(new(active.ProjectPreviewId, false));
        Ra2ProjectEditApplyResult applied = workspace.ApplyProject(new(active.ProjectPreviewId, true));
        Ra2ProjectEditApplyResult replay = workspace.ApplyProject(new(active.ProjectPreviewId, true));

        Assert.Equal(Ra2ProjectEditApplyOutcomeKind.ConfirmationRequired, unconfirmed.OutcomeKind);
        Assert.True(applied.Succeeded);
        Assert.Equal(Ra2ProjectEditApplyOutcomeKind.PreviewUnavailable, replay.OutcomeKind);
        Assert.Equal(1, port.ProjectApplyCount);
    }

    [Fact]
    public void CompoundUndoRedo_TransitionsBothDocumentsAtomically()
    {
        Fixture fixture = new();
        Ra2ProjectEditorTransactionCoordinator coordinator = fixture.Coordinator(fixture.SessionService);
        Assert.True(coordinator.Apply(fixture.Preview()).Succeeded);

        Ra2ProjectCompoundUndoResult undo = coordinator.Undo();
        Assert.True(undo.Succeeded);
        Assert.Equal(2, undo.AffectedDocumentCount);
        Assert.Equal("[E1]\nStrength=100\n", fixture.Editor.CurrentText);
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? undoneArt));
        Assert.Equal("[E1]\nStrength=200\n", undoneArt!.DocumentState.CurrentText);

        Ra2ProjectCompoundUndoResult redo = coordinator.Redo();
        Assert.True(redo.Succeeded);
        Assert.Equal("[E1]\nStrength=150\n", fixture.Editor.CurrentText);
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? redoneArt));
        Assert.Equal("[E1]\nStrength=250\n", redoneArt!.DocumentState.CurrentText);
        Assert.Equal(0, fixture.FileStore.WriteCount);
    }

    [Fact]
    public void CompoundUndo_WhenOneMemberIsStale_ChangesNothing()
    {
        Fixture fixture = new();
        Ra2ProjectEditorTransactionCoordinator coordinator = fixture.Coordinator(fixture.SessionService);
        Assert.True(coordinator.Apply(fixture.Preview()).Succeeded);
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? art));
        Ra2EditableDocumentSession editedArt = fixture.SessionService.UpdateText(art!, "[E1]\nStrength=251\n");
        Assert.True(fixture.Store.TryReplaceMany([new(art!, editedArt)], out _));

        Ra2ProjectCompoundUndoResult undo = coordinator.Undo();

        Assert.Equal(Ra2ProjectCompoundUndoOutcomeKind.Stale, undo.OutcomeKind);
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? rules));
        Assert.Equal("[E1]\nStrength=150\n", rules!.DocumentState.CurrentText);
        Assert.Equal("[E1]\nStrength=150\n", fixture.Editor.CurrentText);
    }

    [Fact]
    public void CompoundUndo_AllowsSavedMemberWithUnchangedTextAndMakesItDirtyAgainstSavedOriginal()
    {
        Fixture fixture = new();
        Ra2ProjectEditorTransactionCoordinator coordinator = fixture.Coordinator(fixture.SessionService);
        Assert.True(coordinator.Apply(fixture.Preview()).Succeeded);
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? art));
        Ra2EditableDocumentSession savedArt = fixture.SessionService.MarkSaved(art!, art!.DocumentState.CurrentText);
        Assert.True(fixture.Store.TryReplaceMany([new(art, savedArt)], out _));

        Ra2ProjectCompoundUndoResult undo = coordinator.Undo();

        Assert.True(undo.Succeeded);
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? undoneArt));
        Assert.Equal("[E1]\nStrength=200\n", undoneArt!.DocumentState.CurrentText);
        Assert.True(undoneArt.DocumentState.IsDirty);
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            Root = Path.Combine(Path.GetTempPath(), "Ra2ProjectTxnTests", Guid.NewGuid().ToString("N"));
            RulesPath = Path.GetFullPath(Path.Combine(Root, "rulesmd.ini"));
            ArtPath = Path.GetFullPath(Path.Combine(Root, "artmd.ini"));
            FileStore = new RecordingFileStore(
                (RulesPath, "[E1]\nStrength=100\n"),
                (ArtPath, "[E1]\nStrength=200\n"));
            SessionService = new(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService());
            Store = new(
                new ProjectOpenResult(Root, [new("rulesmd.ini", RulesPath, 20), new("artmd.ini", ArtPath, 20)]),
                FileStore,
                SessionService,
                new Ra2EditorEncodingMetadataAdapter());
            Assert.True(Store.TryActivate(RulesPath, out Ra2EditableDocumentSession? active, out _));
            Editor = new RecordingEditor(active!.DocumentState.CurrentText);
            Registry = new(new BuiltInRa2FieldDefinitionProvider(), 7);
        }

        public string Root { get; }
        public string RulesPath { get; }
        public string ArtPath { get; }
        public RecordingFileStore FileStore { get; }
        public Ra2EditableDocumentSessionService SessionService { get; }
        public Ra2ProjectDocumentSessionStore Store { get; }
        public RecordingEditor Editor { get; }
        public Ra2FieldRegistryProviderSnapshot Registry { get; }

        public Ra2ProjectEditPreview Preview()
        {
            Ra2ProjectSnapshotCaptureResult capture = Store.CaptureSnapshot(
                [RulesPath, ArtPath], Editor.CurrentText, Registry);
            Assert.True(capture.Succeeded);
            Ra2AutomationProjectSnapshot snapshot = capture.Snapshot!;
            Ra2AutomationEditPlan rulesPlan = Plan(snapshot.Documents[0], "150");
            Ra2AutomationEditPlan artPlan = Plan(snapshot.Documents[1], "250");
            Ra2AutomationProjectEditPlan plan = new(
                Guid.NewGuid(), snapshot.ProjectSessionId, snapshot.ProjectRevision,
                [rulesPlan, artPlan], "two document edit", "tests");
            return new Ra2ProjectEditPreviewService().Preview(snapshot, plan);
        }

        public Ra2ProjectEditorTransactionCoordinator Coordinator(IRa2EditableDocumentSessionService sessionService)
            => new(Store, sessionService, Editor, () => Registry.Revision);

        private static Ra2AutomationEditPlan Plan(Ra2AutomationDocumentSnapshot snapshot, string value)
            => new(
                Guid.NewGuid(), snapshot.DocumentId, snapshot.Version, snapshot.FieldRegistry.Revision,
                [new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Strength", value)],
                "set strength", "tests");
    }

    private sealed class RecordingEditor : IRa2ActiveEditorProjection
    {
        public RecordingEditor(string text) => CurrentText = text;
        public string CurrentText { get; private set; }
        public bool ThrowOnNextSet { get; set; }
        public bool IsReadOnlyFailSafe { get; private set; }
        public void SetText(string text)
        {
            if (ThrowOnNextSet)
            {
                ThrowOnNextSet = false;
                throw new InvalidOperationException("sync failed");
            }
            CurrentText = text;
        }
        public void EnterReadOnlyFailSafe() => IsReadOnlyFailSafe = true;
    }

    private sealed class ThrowOnSecondUpdateSessionService : IRa2EditableDocumentSessionService
    {
        private readonly IRa2EditableDocumentSessionService _inner;
        private int _updateCount;
        public ThrowOnSecondUpdateSessionService(IRa2EditableDocumentSessionService inner) => _inner = inner;
        public Ra2EditableDocumentSession StartEditing(string filePath, string text) => _inner.StartEditing(filePath, text);
        public Ra2EditableDocumentSession StartEditing(string filePath, string text, Ra2EditorTextEncodingMetadata encodingMetadata) => _inner.StartEditing(filePath, text, encodingMetadata);
        public Ra2EditableDocumentSession UpdateText(Ra2EditableDocumentSession session, string currentText)
            => ++_updateCount == 2 ? throw new InvalidOperationException("prepare failed") : _inner.UpdateText(session, currentText);
        public Ra2EditableDocumentSession MarkSaved(Ra2EditableDocumentSession session, string savedText) => _inner.MarkSaved(session, savedText);
        public Ra2EditableDocumentSession Revert(Ra2EditableDocumentSession session) => _inner.Revert(session);
    }

    private sealed class RecordingPort : IRa2EditorTransactionPort
    {
        private readonly Ra2ProjectEditorTransactionCoordinator _coordinator;
        public RecordingPort(Ra2ProjectEditorTransactionCoordinator coordinator) => _coordinator = coordinator;
        public int ProjectApplyCount { get; private set; }
        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview) => Ra2IniEditApplyResult.UnexpectedFailure(preview.PreviewId);
        public Ra2ProjectEditApplyResult ApplyProject(Ra2ProjectEditPreview preview)
        {
            ProjectApplyCount++;
            return _coordinator.Apply(preview);
        }
    }

    private sealed class RecordingFileStore : IIniFileStore
    {
        private readonly Dictionary<string, string> _files;
        public RecordingFileStore(params (string Path, string Text)[] files)
            => _files = files.ToDictionary(item => Path.GetFullPath(item.Path), item => item.Text, StringComparer.OrdinalIgnoreCase);
        public int WriteCount { get; private set; }
        public IniTextReadResult ReadText(string path)
            => new(path, _files[Path.GetFullPath(path)], new UTF8Encoding(false), "\n");
        public IniTextWriteResult WriteText(string path, string text, Encoding encoding)
        {
            WriteCount++;
            return new(true, path);
        }
    }
}
