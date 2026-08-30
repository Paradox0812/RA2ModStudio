using System.Reflection;
using System.Security.Cryptography;

namespace RA2IniEditor.AssetHost.Tests;

public sealed class Ra2GenerationContractTests
{
    [Fact]
    public void AssetHost_exports_only_the_bounded_IDE_facade_family()
    {
        Assert.Equal(
            new[]
            {
                "Ra2MeshGenerationFacade",
                "Ra2MeshGenerationFailureKind",
                "Ra2MeshGenerationProgress",
                "Ra2MeshGenerationRequest",
                "Ra2MeshGenerationResult",
                "Ra2ReferenceImageFormat"
            },
            typeof(IRa2VoxelGenerationHost).Assembly.GetExportedTypes()
                .Select(type => type.Name)
                .OrderBy(name => name));
    }

    [Fact]
    public async Task Facade_reports_missing_bundle_without_starting_a_provider()
    {
        string root = Path.Combine(Path.GetTempPath(), "ra2-facade-" + Guid.NewGuid().ToString("N"));
        var facade = Ra2MeshGenerationFacade.CreateFromBundle(
            Path.Combine(root, "missing.json"),
            Path.Combine(root, "workspace"),
            Array.Empty<string>(),
            licenseAccepted: true);

        Ra2MeshGenerationResult result = await facade.ProbeAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2MeshGenerationFailureKind.BundleMissing, result.FailureKind);
    }

    [Fact]
    public async Task Facade_rejects_invalid_reference_before_bundle_or_provider_access()
    {
        string root = Path.Combine(Path.GetTempPath(), "ra2-facade-" + Guid.NewGuid().ToString("N"));
        var facade = Ra2MeshGenerationFacade.CreateFromBundle(
            Path.Combine(root, "missing.json"),
            Path.Combine(root, "workspace"),
            Array.Empty<string>(),
            licenseAccepted: true);
        var request = new Ra2MeshGenerationRequest(
            "bad.png",
            Ra2ReferenceImageFormat.Png,
            new byte[] { 1, 2, 3 },
            string.Empty,
            string.Empty,
            TimeSpan.FromMinutes(10));

        Ra2MeshGenerationResult result = await facade.GenerateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2MeshGenerationFailureKind.InvalidRequest, result.FailureKind);
    }

    [Fact]
    public async Task Facade_rejects_duplicate_or_unknown_bundle_manifest_properties()
    {
        string root = Path.Combine(Path.GetTempPath(), "ra2-facade-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string manifest = Path.Combine(root, "provider.bundle.json");
        await File.WriteAllTextAsync(manifest,
            "{\"schema\":\"ra2-asset-provider-bundle/1\",\"schema\":\"ra2-asset-provider-bundle/1\",\"unknown\":true}");
        var facade = Ra2MeshGenerationFacade.CreateFromBundle(
            manifest,
            Path.Combine(root, "workspace"),
            Array.Empty<string>(),
            licenseAccepted: true);

        Ra2MeshGenerationResult result = await facade.ProbeAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2MeshGenerationFailureKind.BundleRejected, result.FailureKind);
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void Host_surface_is_exactly_probe_run_and_async_workspace_lease()
    {
        MethodInfo[] methods = typeof(IRa2VoxelGenerationHost).GetMethods();
        Assert.Equal(new[] { "ProbeAsync", "RunAsync" }, methods.Select(method => method.Name).OrderBy(name => name));
        Assert.True(typeof(IAsyncDisposable).IsAssignableFrom(typeof(IRa2GenerationWorkspaceLease)));
        Assert.Equal(new[] { "get_Candidates", "OpenArtifactReadAsync" },
            typeof(IRa2GenerationWorkspaceLease).GetMethods().Select(method => method.Name).OrderBy(name => name));
    }

    [Fact]
    public void Request_owns_reference_bytes_and_semantic_fingerprint_excludes_run_identity()
    {
        byte[] source = { 1, 2, 3, 4 };
        var reference = new Ra2GenerationReferenceImage("front.png", Ra2GenerationMediaKind.Png, source);
        source[0] = 99;

        var first = CreateRequest(Guid.NewGuid(), reference, seed: 17, TimeSpan.FromSeconds(10));
        var replay = CreateRequest(Guid.NewGuid(), reference, seed: 17, TimeSpan.FromMinutes(2));
        var changedSeed = CreateRequest(Guid.NewGuid(), reference, seed: 18, TimeSpan.FromSeconds(10));

        Assert.Equal(Convert.ToHexString(SHA256.HashData(new byte[] { 1, 2, 3, 4 })), reference.Sha256);
        Assert.Equal(first.Fingerprint, replay.Fingerprint);
        Assert.NotEqual(first.Fingerprint, changedSeed.Fingerprint);
    }

    private static Ra2GenerationRequest CreateRequest(
        Guid runId,
        Ra2GenerationReferenceImage reference,
        int seed,
        TimeSpan timeout) =>
        new(
            runId,
            "Create one bounded fixture mesh.",
            string.Empty,
            new[] { reference },
            seed,
            candidateCount: 1,
            includePreviewPng: false,
            expectedProviderId: AssetHostTestFixture.ProviderId,
            expectedModelRevision: AssetHostTestFixture.ModelRevision,
            timeout);
}
