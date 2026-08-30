using System.Text;
using System.Text.Json;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelSemanticSidecarStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ra2-semantic-sidecar-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void SaveThenLoad_RoundTripsAllThreeLayersAndProducesStableBytes()
    {
        Directory.CreateDirectory(_root);
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot();
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        string first = evidence.Regions[0].RegionId;
        string second = evidence.Regions.Count > 1 ? evidence.Regions[1].RegionId : first;
        var suggestion = new Ra2VoxelSemanticAssignment(first, Ra2VoxelSemanticPartRole.BodyShell,
            Ra2VoxelSemanticMaterialRole.PaintedSurface, Ra2VoxelSemanticRemapIntent.None, .75, "AI 建议");
        var regionOverride = new Ra2VoxelSemanticAssignment(second, Ra2VoxelSemanticPartRole.Turret,
            Ra2VoxelSemanticMaterialRole.BareMetal, Ra2VoxelSemanticRemapIntent.ExplicitlyApproved, 1, "人工覆盖");
        var layer = new Ra2VoxelSemanticManualMaskLayer(snapshot.CanonicalHash, snapshot.OccupancyCount,
        [
            new(0, Ra2VoxelSemanticPartRole.Wheel, Ra2VoxelSemanticMaterialRole.Rubber, Ra2VoxelSemanticRemapIntent.None, "左轮"),
            new(3, Ra2VoxelSemanticPartRole.Barrel, Ra2VoxelSemanticMaterialRole.BareMetal, Ra2VoxelSemanticRemapIntent.None, "炮管")
        ]);
        var state = new Ra2VoxelSemanticSidecarState(evidence, true, [suggestion], [regionOverride], layer);
        string path = Path.Combine(_root, "sample.semantic.json");
        var store = new Ra2VoxelSemanticSidecarStore();

        Assert.True(store.Save(_root, path, snapshot, state).IsSuccess);
        byte[] firstBytes = File.ReadAllBytes(path);
        Ra2VoxelSemanticSidecarResult loaded = store.Load(_root, path, snapshot);
        Assert.True(loaded.IsSuccess, loaded.Message);
        Assert.NotNull(loaded.State);
        Assert.True(loaded.State.AgentSuggestionsAccepted);
        Assert.Equal([suggestion], loaded.State.AgentSuggestions);
        Assert.Equal([regionOverride], loaded.State.HumanRegionOverrides);
        Assert.Equal(layer.LayerHash, loaded.State.HumanCellLayer.LayerHash);
        Assert.Equal(layer.Overrides, loaded.State.HumanCellLayer.Overrides);

        Assert.True(store.Save(_root, path, snapshot, loaded.State).IsSuccess);
        Assert.Equal(firstBytes, File.ReadAllBytes(path));
    }

    [Fact]
    public void Load_RejectsDifferentSnapshotWithoutChangingCallerState()
    {
        Directory.CreateDirectory(_root);
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot();
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        var state = new Ra2VoxelSemanticSidecarState(evidence, false, [], [],
            new(snapshot.CanonicalHash, snapshot.OccupancyCount));
        string path = Path.Combine(_root, "sample.semantic.json");
        var store = new Ra2VoxelSemanticSidecarStore();
        Assert.True(store.Save(_root, path, snapshot, state).IsSuccess);

        Ra2VoxelSceneSnapshot changed = CreateSnapshot(61);
        Ra2VoxelSemanticSidecarResult result = store.Load(_root, path, changed);
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.SnapshotMismatch, result.FailureKind);
        Assert.Null(result.State);
    }

    [Fact]
    public void Load_RejectsUnknownAndDuplicateProperties()
    {
        Directory.CreateDirectory(_root);
        string path = Path.Combine(_root, "bad.semantic.json");
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot();
        File.WriteAllText(path, "{\"schema\":\"x\",\"schema\":\"y\"}", new UTF8Encoding(false));
        var store = new Ra2VoxelSemanticSidecarStore();
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.InvalidJson, store.Load(_root, path, snapshot).FailureKind);

        File.WriteAllText(path, "{\"schema\":\"ra2-voxel-semantic-sidecar\",\"version\":1,\"unexpected\":true}", new UTF8Encoding(false));
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.InvalidJson, store.Load(_root, path, snapshot).FailureKind);
    }

    [Fact]
    public void Load_RejectsInvalidUtf8AndUnsupportedVersion()
    {
        Directory.CreateDirectory(_root);
        string path = Path.Combine(_root, "bad.semantic.json");
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot();
        var store = new Ra2VoxelSemanticSidecarStore();
        File.WriteAllBytes(path, [0x7B, 0x22, 0xFF, 0x22, 0x7D]);
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.InvalidUtf8, store.Load(_root, path, snapshot).FailureKind);

        File.WriteAllText(path, "{\"schema\":\"ra2-voxel-semantic-sidecar\",\"version\":2}", new UTF8Encoding(false));
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.UnsupportedSchema, store.Load(_root, path, snapshot).FailureKind);
    }

    [Fact]
    public void SaveAndLoad_RejectPathsOutsideProject()
    {
        Directory.CreateDirectory(_root);
        string project = Directory.CreateDirectory(Path.Combine(_root, "project")).FullName;
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot();
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        var state = new Ra2VoxelSemanticSidecarState(evidence, false, [], [], new(snapshot.CanonicalHash, snapshot.OccupancyCount));
        string outside = Path.Combine(_root, "outside.semantic.json");
        var store = new Ra2VoxelSemanticSidecarStore();
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.OutsideProject, store.Save(project, outside, snapshot, state).FailureKind);
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.OutsideProject, store.Load(project, outside, snapshot).FailureKind);
    }

    [Fact]
    public void Load_RejectsManualLayerHashTampering()
    {
        Directory.CreateDirectory(_root);
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot();
        Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        var layer = new Ra2VoxelSemanticManualMaskLayer(snapshot.CanonicalHash, snapshot.OccupancyCount,
            [new(0, Ra2VoxelSemanticPartRole.BodyShell, Ra2VoxelSemanticMaterialRole.PaintedSurface, Ra2VoxelSemanticRemapIntent.None, "人工")]);
        var state = new Ra2VoxelSemanticSidecarState(evidence, false, [], [], layer);
        string path = Path.Combine(_root, "sample.semantic.json");
        var store = new Ra2VoxelSemanticSidecarStore();
        Assert.True(store.Save(_root, path, snapshot, state).IsSuccess);
        string text = File.ReadAllText(path).Replace(layer.LayerHash, new string('A', 64), StringComparison.Ordinal);
        File.WriteAllText(path, text, new UTF8Encoding(false));
        Assert.Equal(Ra2VoxelSemanticSidecarFailureKind.LayerHashMismatch, store.Load(_root, path, snapshot).FailureKind);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot(byte paletteIndex = 60)
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256).Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index)).ToArray();
        colours[0] = new(0, 0, 0, 0);
        var palette = new Ra2VoxelPaletteProfile("sidecar-test", colours, [0]);
        var part = new Ra2VoxelPartDescriptor("body", Ra2VoxelAssemblyPartRole.Body, "Body", "sample", 4, 4, 4);
        return new("sample", part, palette,
        [
            new(new(1, 1, 1), paletteIndex), new(new(2, 1, 1), paletteIndex),
            new(new(1, 2, 1), paletteIndex), new(new(2, 2, 1), paletteIndex),
            new(new(1, 1, 2), paletteIndex), new(new(2, 1, 2), paletteIndex),
            new(new(1, 2, 2), paletteIndex), new(new(2, 2, 2), paletteIndex)
        ]);
    }
}
