using RA2IniEditor.IDE.AssetAuthoring;
using System.Text;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelStyleSourceResolverTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ra2-voxel-style-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Resolver_UsesBroadToNarrowScopesAndStableNormalizedHashes()
    {
        string project = Directory.CreateDirectory(Path.Combine(_root, "project")).FullName;
        string target = Directory.CreateDirectory(Path.Combine(project, "vehicles", "allied")).FullName;
        string builtIn = Write(Path.Combine(_root, "bundled.md"), "built in\r\nstyle");
        Write(Path.Combine(project, Ra2VoxelStyleSourceResolver.FileName), "project style");
        Write(Path.Combine(project, "vehicles", Ra2VoxelStyleSourceResolver.FileName), "vehicle style");
        Write(Path.Combine(target, Ra2VoxelStyleSourceResolver.FileName), "allied style");

        Ra2VoxelStyleSourceResolutionResult first = Ra2VoxelStyleSourceResolver.Resolve(
            builtIn, project, target, "request style");
        Ra2VoxelStyleSourceResolutionResult second = Ra2VoxelStyleSourceResolver.Resolve(
            builtIn, project, target, "request style");

        Assert.True(first.IsSuccess, first.Message);
        Assert.Equal(first.SourcePack!.PackHash, second.SourcePack!.PackHash);
        Assert.Equal(
            ["built-in", "project", "directory:vehicles", "directory:vehicles/allied", "request"],
            first.SourcePack.Sources.Select(source => source.ScopeId));
        Assert.Equal("built in\nstyle", first.SourcePack.Sources[0].Text);
    }

    [Fact]
    public void Resolver_DoesNotRecursivelyDiscoverSiblingStyles()
    {
        string project = Directory.CreateDirectory(Path.Combine(_root, "project")).FullName;
        string target = Directory.CreateDirectory(Path.Combine(project, "target")).FullName;
        string sibling = Directory.CreateDirectory(Path.Combine(project, "sibling")).FullName;
        string builtIn = Write(Path.Combine(_root, "bundled.md"), "built in");
        Write(Path.Combine(sibling, Ra2VoxelStyleSourceResolver.FileName), "must not load");

        Ra2VoxelStyleSourceResolutionResult result = Ra2VoxelStyleSourceResolver.Resolve(builtIn, project, target);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Single(result.SourcePack!.Sources);
        Assert.Equal("built-in", result.SourcePack.Sources[0].ScopeId);
    }

    [Fact]
    public void Resolver_RejectsOutsideTargetInvalidUtf8AndOversizedOverride()
    {
        string project = Directory.CreateDirectory(Path.Combine(_root, "project")).FullName;
        string outside = Directory.CreateDirectory(Path.Combine(_root, "outside")).FullName;
        string builtIn = Write(Path.Combine(_root, "bundled.md"), "built in");

        Assert.Equal(
            Ra2VoxelStyleSourceFailureKind.SourcePathOutsideProject,
            Ra2VoxelStyleSourceResolver.Resolve(builtIn, project, outside).FailureKind);

        File.WriteAllBytes(Path.Combine(project, Ra2VoxelStyleSourceResolver.FileName), [0xC3, 0x28]);
        Assert.Equal(
            Ra2VoxelStyleSourceFailureKind.InvalidEncoding,
            Ra2VoxelStyleSourceResolver.Resolve(builtIn, project).FailureKind);

        File.Delete(Path.Combine(project, Ra2VoxelStyleSourceResolver.FileName));
        Assert.Equal(
            Ra2VoxelStyleSourceFailureKind.SourceTooLarge,
            Ra2VoxelStyleSourceResolver.Resolve(
                builtIn,
                project,
                requestOverride: new string('x', Ra2VoxelStyleSourceResolver.MaximumOverrideCharacters + 1)).FailureKind);
    }

    [Fact]
    public void Resolver_RejectsMissingOrEmptyBundledSource()
    {
        string project = Directory.CreateDirectory(Path.Combine(_root, "project")).FullName;

        Assert.Equal(
            Ra2VoxelStyleSourceFailureKind.NoStyleSource,
            Ra2VoxelStyleSourceResolver.Resolve(Path.Combine(_root, "missing.md"), project).FailureKind);

        string empty = Write(Path.Combine(_root, "empty.md"), "   ");
        Assert.Equal(
            Ra2VoxelStyleSourceFailureKind.SourceTooLarge,
            Ra2VoxelStyleSourceResolver.Resolve(empty, project).FailureKind);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private static string Write(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text, new UTF8Encoding(false));
        return path;
    }
}
