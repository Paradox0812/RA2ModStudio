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

public sealed class Ra2ProjectDocumentSessionStoreTests
{
    [Fact]
    public void Store_PreservesActiveAndInactiveDirtySessionsAndCapturesStableSnapshot()
    {
        Fixture fixture = new();
        Assert.Equal([fixture.RulesPath, fixture.ArtPath], fixture.Store.MemberFilePaths);
        IList<string> immutableMembership = Assert.IsAssignableFrom<IList<string>>(fixture.Store.MemberFilePaths);
        Assert.Throws<NotSupportedException>(() => immutableMembership.Add(Path.Combine(fixture.Root, "other.ini")));
        Assert.True(fixture.Store.TryActivate(fixture.RulesPath, out Ra2EditableDocumentSession? rules, out _));
        Assert.True(fixture.Store.TrySynchronizeActiveText(
            rules!, "[E1]\nStrength=150\n", out Ra2EditableDocumentSession? dirtyRules, out _));
        Assert.Equal(1, fixture.Store.ProjectRevision);

        Assert.True(fixture.Store.TryActivate(fixture.ArtPath, out Ra2EditableDocumentSession? art, out _));
        Assert.True(fixture.Store.TrySynchronizeActiveText(
            art!, "[E1]\nCameo=NEWICON\n", out Ra2EditableDocumentSession? dirtyArt, out _));
        Assert.Equal(2, fixture.Store.ProjectRevision);
        Assert.Equal(2, fixture.Store.DirtyDocumentCount);

        Ra2ProjectSnapshotCaptureResult capture = fixture.Store.CaptureSnapshot(
            [fixture.ArtPath, fixture.RulesPath],
            dirtyArt!.DocumentState.CurrentText,
            fixture.Registry);

        Assert.True(capture.Succeeded);
        Assert.Equal(fixture.Store.ProjectSessionId, capture.Snapshot!.ProjectSessionId);
        Assert.Equal(2, capture.Snapshot.ProjectRevision);
        Assert.Equal(new[] { fixture.ArtPath, fixture.RulesPath }, capture.Snapshot.Documents.Select(document => document.FilePath));
        Assert.Equal("[E1]\nCameo=NEWICON\n", capture.Snapshot.Documents[0].Text);
        Assert.Equal("[E1]\nStrength=150\n", capture.Snapshot.Documents[1].Text);
        Assert.Equal(1, fixture.FileStore.ReadCounts[fixture.RulesPath]);
        Assert.Equal(1, fixture.FileStore.ReadCounts[fixture.ArtPath]);
        Assert.Equal(0, fixture.FileStore.WriteCount);
    }

    [Fact]
    public void Capture_UsesCachedInactiveSessionAndRejectsActiveOverlayMismatch()
    {
        Fixture fixture = new();
        Assert.True(fixture.Store.TryActivate(fixture.RulesPath, out Ra2EditableDocumentSession? rules, out _));
        Assert.True(fixture.Store.TrySynchronizeActiveText(rules!, "[E1]\nStrength=125\n", out rules, out _));
        Assert.True(fixture.Store.TryActivate(fixture.ArtPath, out Ra2EditableDocumentSession? art, out _));

        Ra2ProjectSnapshotCaptureResult mismatch = fixture.Store.CaptureSnapshot(
            [fixture.RulesPath], "not the active editor text", fixture.Registry);
        Assert.Equal(Ra2ProjectSnapshotCaptureFailureKind.ActiveEditorTextMismatch, mismatch.FailureKind);

        Ra2ProjectSnapshotCaptureResult capture = fixture.Store.CaptureSnapshot(
            [fixture.RulesPath], art!.DocumentState.CurrentText, fixture.Registry);
        Assert.True(capture.Succeeded);
        Assert.Equal("[E1]\nStrength=125\n", Assert.Single(capture.Snapshot!.Documents).Text);
        Assert.Equal(1, fixture.FileStore.ReadCounts[fixture.RulesPath]);
    }

    [Fact]
    public void Capture_RejectsDuplicateUnknownAndReadFailureWithoutSnapshot()
    {
        Fixture fixture = new();
        Assert.Equal(
            Ra2ProjectSnapshotCaptureFailureKind.DuplicateTarget,
            fixture.Store.CaptureSnapshot([fixture.RulesPath, fixture.RulesPath], null, fixture.Registry).FailureKind);
        Assert.Equal(
            Ra2ProjectSnapshotCaptureFailureKind.InvalidTarget,
            fixture.Store.CaptureSnapshot([Path.Combine(fixture.Root, "other.ini")], null, fixture.Registry).FailureKind);

        fixture.FileStore.ThrowOnRead.Add(fixture.ArtPath);
        Ra2ProjectSnapshotCaptureResult failed = fixture.Store.CaptureSnapshot([fixture.ArtPath], null, fixture.Registry);
        Assert.Equal(Ra2ProjectSnapshotCaptureFailureKind.ReadFailure, failed.FailureKind);
        Assert.Null(failed.Snapshot);
    }

    [Fact]
    public void TryReplaceMany_IsAtomicAndRejectsOneStaleMember()
    {
        Fixture fixture = new();
        Assert.True(fixture.Store.TryActivate(fixture.RulesPath, out Ra2EditableDocumentSession? rules, out _));
        Assert.True(fixture.Store.TryActivate(fixture.ArtPath, out Ra2EditableDocumentSession? art, out _));
        Ra2EditableDocumentSession nextRules = fixture.SessionService.UpdateText(rules!, "[E1]\nStrength=150\n");
        Ra2EditableDocumentSession nextArt = fixture.SessionService.UpdateText(art!, "[E1]\nCameo=NEWICON\n");

        Assert.True(fixture.Store.TryReplaceMany(
            [new(rules!, nextRules), new(art!, nextArt)], out _));
        Assert.Equal(1, fixture.Store.ProjectRevision);
        Assert.True(fixture.Store.TryGetSession(fixture.RulesPath, out Ra2EditableDocumentSession? currentRules));
        Assert.Same(nextRules, currentRules);

        Ra2EditableDocumentSession staleReplacement = fixture.SessionService.UpdateText(rules!, "stale");
        Ra2EditableDocumentSession anotherArt = fixture.SessionService.UpdateText(nextArt, "[E1]\nCameo=THIRD\n");
        Assert.False(fixture.Store.TryReplaceMany(
            [new(rules!, staleReplacement), new(nextArt, anotherArt)], out _));
        Assert.True(fixture.Store.TryGetSession(fixture.ArtPath, out Ra2EditableDocumentSession? currentArt));
        Assert.Same(nextArt, currentArt);
        Assert.Same(nextRules, currentRules);
        Assert.Equal(1, fixture.Store.ProjectRevision);
    }

    [Fact]
    public void Constructor_RejectsMembershipOutsideRootAndDuplicatePaths()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ProjectStoreTests", Guid.NewGuid().ToString("N")));
        string outside = Path.GetFullPath(Path.Combine(root, "..", "outside.ini"));
        ProjectOpenResult outsideProject = new(root, [new("outside.ini", outside, 1)]);
        Assert.Throws<ArgumentException>(() => CreateStore(outsideProject, new RecordingIniFileStore()));

        string rules = Path.Combine(root, "rulesmd.ini");
        ProjectOpenResult duplicateProject = new(root, [new("rulesmd.ini", rules, 1), new("RULESMD.INI", rules.ToUpperInvariant(), 1)]);
        Assert.Throws<ArgumentException>(() => CreateStore(duplicateProject, new RecordingIniFileStore()));
    }

    private static Ra2ProjectDocumentSessionStore CreateStore(ProjectOpenResult project, RecordingIniFileStore fileStore)
        => new(project, fileStore, CreateSessionService(), new Ra2EditorEncodingMetadataAdapter());

    private static Ra2EditableDocumentSessionService CreateSessionService()
        => new(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService());

    private sealed class Fixture
    {
        public Fixture()
        {
            Root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Ra2ProjectStoreTests", Guid.NewGuid().ToString("N")));
            RulesPath = Path.Combine(Root, "rulesmd.ini");
            ArtPath = Path.Combine(Root, "artmd.ini");
            FileStore = new RecordingIniFileStore(
                (RulesPath, "[E1]\nStrength=100\n"),
                (ArtPath, "[E1]\nCameo=E1ICON\n"));
            SessionService = CreateSessionService();
            Store = new Ra2ProjectDocumentSessionStore(
                new ProjectOpenResult(Root, [new("rulesmd.ini", RulesPath, 20), new("artmd.ini", ArtPath, 20)]),
                FileStore,
                SessionService,
                new Ra2EditorEncodingMetadataAdapter());
            Registry = new Ra2FieldRegistryProviderSnapshot(new BuiltInRa2FieldDefinitionProvider(), 7);
        }

        public string Root { get; }
        public string RulesPath { get; }
        public string ArtPath { get; }
        public RecordingIniFileStore FileStore { get; }
        public Ra2EditableDocumentSessionService SessionService { get; }
        public Ra2ProjectDocumentSessionStore Store { get; }
        public Ra2FieldRegistryProviderSnapshot Registry { get; }
    }

    private sealed class RecordingIniFileStore : IIniFileStore
    {
        private readonly Dictionary<string, string> _texts;

        public RecordingIniFileStore(params (string Path, string Text)[] files)
        {
            _texts = files.ToDictionary(item => Path.GetFullPath(item.Path), item => item.Text, StringComparer.OrdinalIgnoreCase);
            ReadCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, int> ReadCounts { get; }
        public HashSet<string> ThrowOnRead { get; } = new(StringComparer.OrdinalIgnoreCase);
        public int WriteCount { get; private set; }

        public IniTextReadResult ReadText(string path)
        {
            path = Path.GetFullPath(path);
            ReadCounts[path] = ReadCounts.GetValueOrDefault(path) + 1;
            if (ThrowOnRead.Contains(path))
                throw new IOException("read failed");
            return new IniTextReadResult(path, _texts[path], new UTF8Encoding(false), "\n");
        }

        public IniTextWriteResult WriteText(string path, string text, Encoding encoding)
        {
            WriteCount++;
            return new IniTextWriteResult(true, path);
        }
    }
}
