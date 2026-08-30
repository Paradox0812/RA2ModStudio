using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelVoxExportServiceTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "ra2-vox-export-tests", Guid.NewGuid().ToString("N"))).FullName;

    [Fact]
    public void Export_WritesVerifiedDeterministicVox()
    {
        Ra2VoxelAcceptedCandidate candidate = CreateCandidate();
        string target = Path.Combine(_root, "accepted.vox");

        Ra2VoxelVoxExportResult result = new Ra2VoxelVoxExportService().Export(
            candidate,
            target,
            currentSourcePath: null,
            overwriteExisting: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(target, result.TargetPath);
        Assert.True(result.ByteCount > 0);
        Assert.Equal(64, result.ContentHash!.Length);
        byte[] bytes = File.ReadAllBytes(target);
        using MemoryStream stream = new(bytes, writable: false);
        Ra2VoxelSceneSnapshot decoded = Ra2MagicaVoxelCodec.Read(stream, "verify", "body", Ra2VoxelAssemblyPartRole.Body, "Body", "verify");
        Assert.Equal(bytes, Ra2MagicaVoxelCodec.Write(decoded));
        Assert.DoesNotContain(Directory.EnumerateFiles(_root), path => path.EndsWith(".tmp", StringComparison.Ordinal));
    }

    [Fact]
    public void Export_RejectsCurrentSourceAndPreservesItsBytes()
    {
        Ra2VoxelAcceptedCandidate candidate = CreateCandidate();
        string source = Path.Combine(_root, "source.vox");
        byte[] original = [1, 2, 3, 4];
        File.WriteAllBytes(source, original);

        Ra2VoxelVoxExportResult result = new Ra2VoxelVoxExportService().Export(
            candidate,
            source,
            source,
            overwriteExisting: true);

        Assert.Equal(Ra2VoxelVoxExportFailureKind.SourceOverwriteRejected, result.FailureKind);
        Assert.Equal(original, File.ReadAllBytes(source));
    }

    [Fact]
    public void Export_RequiresExplicitOverwriteAndCanAtomicallyReplaceAnotherTarget()
    {
        Ra2VoxelAcceptedCandidate candidate = CreateCandidate();
        string target = Path.Combine(_root, "other.vox");
        File.WriteAllBytes(target, [9, 8, 7]);
        Ra2VoxelVoxExportService service = new();

        Ra2VoxelVoxExportResult rejected = service.Export(candidate, target, null, overwriteExisting: false);
        Assert.Equal(Ra2VoxelVoxExportFailureKind.TargetExists, rejected.FailureKind);
        Assert.Equal(new byte[] { 9, 8, 7 }, File.ReadAllBytes(target));

        Ra2VoxelVoxExportResult replaced = service.Export(candidate, target, null, overwriteExisting: true);
        Assert.True(replaced.IsSuccess);
        Assert.NotEqual(new byte[] { 9, 8, 7 }, File.ReadAllBytes(target));
    }

    [Fact]
    public void Export_CanceledBeforeEncodingCreatesNoFile()
    {
        string target = Path.Combine(_root, "canceled.vox");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2VoxelVoxExportResult result = new Ra2VoxelVoxExportService().Export(
            CreateCandidate(), target, null, overwriteExisting: false, cancellation.Token);

        Assert.Equal(Ra2VoxelVoxExportFailureKind.Canceled, result.FailureKind);
        Assert.False(File.Exists(target));
        Assert.Empty(Directory.EnumerateFiles(_root));
    }

    [Theory]
    [InlineData("")]
    [InlineData("candidate.vxl")]
    [InlineData("candidate")]
    public void Export_RejectsInvalidTargetWithoutCreatingFiles(string fileName)
    {
        string target = string.IsNullOrEmpty(fileName) ? fileName : Path.Combine(_root, fileName);

        Ra2VoxelVoxExportResult result = new Ra2VoxelVoxExportService().Export(
            CreateCandidate(), target, null, overwriteExisting: false);

        Assert.Equal(Ra2VoxelVoxExportFailureKind.InvalidTarget, result.FailureKind);
        Assert.Empty(Directory.EnumerateFiles(_root));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private static Ra2VoxelAcceptedCandidate CreateCandidate()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new("export-test", colours, [0]);
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "body", 4, 4, 4);
        Ra2VoxelSceneSnapshot snapshot = new(
            "export-test",
            part,
            palette,
            [
                new(new Ra2VoxelCoordinate(1, 1, 1), 80),
                new(new Ra2VoxelCoordinate(2, 1, 1), 90),
                new(new Ra2VoxelCoordinate(1, 2, 1), 100)
            ]);
        return new(snapshot, Ra2VoxelAcceptedCandidateKind.Styled, "测试候选", "body-candidate.vox", 1);
    }
}
