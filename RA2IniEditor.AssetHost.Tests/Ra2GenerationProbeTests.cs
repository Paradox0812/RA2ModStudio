namespace RA2IniEditor.AssetHost.Tests;

public sealed class Ra2GenerationProbeTests
{
    [Fact]
    public async Task Probe_returns_ready_descriptor_without_creating_workspace()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var host = new Ra2VoxelGenerationHost();

        Ra2GenerationProbeResult result = await host.ProbeAsync(AssetHostTestFixture.CreateConfiguration(workspace));

        Assert.True(result.Succeeded);
        Assert.Equal(Ra2GenerationState.Ready, result.State);
        Assert.Equal(Ra2GenerationFailureKind.None, result.FailureKind);
        Assert.Equal(AssetHostTestFixture.ProviderId, result.Descriptor!.ProviderId);
        Assert.False(Directory.Exists(workspace));
    }

    [Fact]
    public async Task Probe_rejects_executable_hash_before_process_start()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(
            workspace,
            expectedHash: new string('A', 64));

        Ra2GenerationProbeResult result = await new Ra2VoxelGenerationHost().ProbeAsync(configuration);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2GenerationFailureKind.ExecutableHashMismatch, result.FailureKind);
        Assert.False(Directory.Exists(workspace));
    }

    [Fact]
    public async Task Probe_rejects_provider_identity_mismatch()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(
            workspace,
            expectedProviderId: "different-provider");

        Ra2GenerationProbeResult result = await new Ra2VoxelGenerationHost().ProbeAsync(configuration);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2GenerationFailureKind.ProviderIdentityMismatch, result.FailureKind);
        Assert.False(Directory.Exists(workspace));
    }

    [Fact]
    public async Task Probe_returns_typed_canceled_result_without_throwing()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Ra2GenerationProbeResult result = await new Ra2VoxelGenerationHost().ProbeAsync(
            AssetHostTestFixture.CreateConfiguration(AssetHostTestFixture.CreateUnusedWorkspacePath()),
            cancellation.Token);

        Assert.Equal(Ra2GenerationState.Canceled, result.State);
        Assert.Equal(Ra2GenerationFailureKind.Canceled, result.FailureKind);
    }

    [Theory]
    [InlineData("not-ready", true, (int)Ra2GenerationFailureKind.ProviderNotReady)]
    [InlineData("capability-missing", true, (int)Ra2GenerationFailureKind.CapabilityUnsupported)]
    [InlineData("license-required", false, (int)Ra2GenerationFailureKind.LicenseNotAccepted)]
    public async Task Probe_maps_readiness_capability_and_license_failures(
        string mode,
        bool licenseAccepted,
        int expectedFailureValue)
    {
        using FixtureProviderSandbox provider = AssetHostTestFixture.CreateProviderSandbox(mode);
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(
            workspace,
            licenseAccepted: licenseAccepted,
            executablePath: provider.ExecutablePath);

        Ra2GenerationProbeResult result = await new Ra2VoxelGenerationHost().ProbeAsync(configuration);

        Assert.False(result.Succeeded);
        Assert.Equal((Ra2GenerationFailureKind)expectedFailureValue, result.FailureKind);
        Assert.False(Directory.Exists(workspace));
    }

    [Fact]
    public async Task Probe_timeout_is_typed_and_does_not_create_workspace()
    {
        using FixtureProviderSandbox provider = AssetHostTestFixture.CreateProviderSandbox("hang");
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(
            workspace,
            probeTimeout: Ra2GenerationLimits.MinimumProbeTimeout,
            executablePath: provider.ExecutablePath);

        Ra2GenerationProbeResult result = await new Ra2VoxelGenerationHost().ProbeAsync(configuration);

        Assert.Equal(Ra2GenerationState.TimedOut, result.State);
        Assert.Equal(Ra2GenerationFailureKind.TimedOut, result.FailureKind);
        Assert.False(Directory.Exists(workspace));
    }
}
