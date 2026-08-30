using System.Text.Json;

namespace RA2IniEditor.AssetHost.Tests;

public sealed class Ra2GenerationReplayTests
{
    [Fact]
    public async Task Same_semantic_request_and_seed_produce_identical_ordered_artifacts()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(workspace);
        var host = new Ra2VoxelGenerationHost();
        try
        {
            Ra2GenerationRequest firstRequest = AssetHostTestFixture.CreateRequest(candidateCount: 4, seed: 73);
            Ra2GenerationRunResult first = await host.RunAsync(configuration, firstRequest);
            Assert.True(first.Succeeded);
            string[] firstIds = first.Lease!.Candidates.Select(candidate => candidate.CandidateId).ToArray();
            string[] firstHashes = first.Lease.Candidates
                .Select(candidate => Assert.Single(candidate.Artifacts, artifact => artifact.ArtifactId == "mesh").Sha256)
                .ToArray();
            await first.Lease.DisposeAsync();

            Ra2GenerationRequest replayRequest = AssetHostTestFixture.CreateRequest(candidateCount: 4, seed: 73);
            Ra2GenerationRunResult replay = await host.RunAsync(configuration, replayRequest);
            Assert.True(replay.Succeeded);
            Assert.Equal(firstRequest.Fingerprint, replayRequest.Fingerprint);
            Assert.Equal(firstIds, replay.Lease!.Candidates.Select(candidate => candidate.CandidateId));
            Assert.Equal(firstHashes, replay.Lease.Candidates
                .Select(candidate => Assert.Single(candidate.Artifacts, artifact => artifact.ArtifactId == "mesh").Sha256));
            await replay.Lease.DisposeAsync();
        }
        finally
        {
            DeleteDirectory(workspace);
        }
    }

    [Fact]
    public async Task Mutated_seed_or_input_changes_request_and_fixture_artifact_fingerprints()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(workspace);
        var host = new Ra2VoxelGenerationHost();
        try
        {
            Ra2GenerationRequest baseline = AssetHostTestFixture.CreateRequest(seed: 19);
            Ra2GenerationRequest changedSeed = AssetHostTestFixture.CreateRequest(seed: 20);
            Ra2GenerationRequest changedInput = CreateRequestWithReference(seed: 19, payloadMarker: 2);

            string baselineHash = await RunAndReadMeshHashAsync(host, configuration, baseline);
            string seedHash = await RunAndReadMeshHashAsync(host, configuration, changedSeed);
            string inputHash = await RunAndReadMeshHashAsync(host, configuration, changedInput);

            Assert.NotEqual(baseline.Fingerprint, changedSeed.Fingerprint);
            Assert.NotEqual(baseline.Fingerprint, changedInput.Fingerprint);
            Assert.NotEqual(baselineHash, seedHash);
            Assert.NotEqual(baselineHash, inputHash);
        }
        finally
        {
            DeleteDirectory(workspace);
        }
    }

    [Fact]
    public async Task Result_manifest_contains_bounded_provenance_and_relative_artifact_names()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(workspace);
        Ra2GenerationRequest request = AssetHostTestFixture.CreateRequest(seed: 31, includePreviewPng: true);
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(configuration, request);
            Assert.True(result.Succeeded);
            string manifestPath = Path.Combine(workspace, request.RunId.ToString("D"), "completed", "result.json");
            using JsonDocument manifest = JsonDocument.Parse(await File.ReadAllBytesAsync(manifestPath));
            JsonElement root = manifest.RootElement;

            Assert.Equal("ra2-generation-result/1", root.GetProperty("schema").GetString());
            Assert.Equal(request.Fingerprint, root.GetProperty("requestFingerprint").GetString());
            Assert.Equal(AssetHostTestFixture.ProviderId, root.GetProperty("ProviderId").GetString());
            Assert.Equal(AssetHostTestFixture.ModelRevision, root.GetProperty("ModelRevision").GetString());
            Assert.Equal("CandidateReady", root.GetProperty("terminalState").GetString());
            Assert.Equal("None", root.GetProperty("failureKind").GetString());
            Assert.True(root.GetProperty("completedUtc").GetDateTimeOffset() >= root.GetProperty("startedUtc").GetDateTimeOffset());
            foreach (JsonElement artifact in root.GetProperty("candidates")[0].GetProperty("artifacts").EnumerateArray())
            {
                string relative = artifact.GetProperty("relativeArtifact").GetString()!;
                Assert.StartsWith("artifacts/", relative, StringComparison.Ordinal);
                Assert.DoesNotContain("..", relative, StringComparison.Ordinal);
                Assert.False(Path.IsPathFullyQualified(relative));
            }

            await result.Lease!.DisposeAsync();
        }
        finally
        {
            DeleteDirectory(workspace);
        }
    }

    [Fact]
    public void Internal_failure_taxonomy_is_frozen_and_public_surface_is_bounded()
    {
        string[] expected =
        {
            "None", "InvalidRequest", "ProviderNotConfigured", "ProviderNotReady", "ProviderIdentityMismatch",
            "ExecutableHashMismatch", "CapabilityUnsupported", "LicenseNotAccepted", "WorkspaceRejected",
            "ProcessStartFailed", "ProtocolViolation", "ProviderReportedFailure", "OutputMissing", "OutputRejected",
            "ResourceLimitExceeded", "TimedOut", "Canceled", "TerminationFailed", "ProcessCrashed", "ReplayMismatch",
            "CleanupFailed", "UnexpectedFailure"
        };

        Assert.Equal(expected, Enum.GetNames<Ra2GenerationFailureKind>());
        Assert.Equal(
            new[]
            {
                "Ra2MeshGenerationFacade", "Ra2MeshGenerationFailureKind", "Ra2MeshGenerationProgress",
                "Ra2MeshGenerationRequest", "Ra2MeshGenerationResult", "Ra2ReferenceImageFormat"
            },
            typeof(IRa2VoxelGenerationHost).Assembly.GetExportedTypes()
                .Select(type => type.Name)
                .OrderBy(name => name));
    }

    [Fact]
    public void Reference_validation_accepts_duplicate_content_but_rejects_media_mismatch()
    {
        byte[] png = CreateReferencePayload(1);
        var sameBytesA = new Ra2GenerationReferenceImage("front.png", Ra2GenerationMediaKind.Png, png);
        var sameBytesB = new Ra2GenerationReferenceImage("rear.png", Ra2GenerationMediaKind.Png, png);
        var duplicateContentRequest = CreateRequest(new[] { sameBytesA, sameBytesB }, seed: 7);
        var wrongMediaRequest = CreateRequest(
            new[] { new Ra2GenerationReferenceImage("front.jpg", Ra2GenerationMediaKind.Jpeg, png) },
            seed: 7);
        var configuration = AssetHostTestFixture.CreateConfiguration(AssetHostTestFixture.CreateUnusedWorkspacePath());

        Assert.Equal(
            Ra2GenerationFailureKind.None,
            Ra2GenerationValidation.ValidateRequest(configuration, duplicateContentRequest, out _));
        Assert.Equal(
            Ra2GenerationFailureKind.InvalidRequest,
            Ra2GenerationValidation.ValidateRequest(configuration, wrongMediaRequest, out _));
    }

    private static async Task<string> RunAndReadMeshHashAsync(
        Ra2VoxelGenerationHost host,
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest request)
    {
        Ra2GenerationRunResult result = await host.RunAsync(configuration, request);
        Assert.True(result.Succeeded);
        string hash = Assert.Single(result.Lease!.Candidates).Artifacts.Single(artifact => artifact.ArtifactId == "mesh").Sha256;
        await result.Lease.DisposeAsync();
        return hash;
    }

    private static Ra2GenerationRequest CreateRequestWithReference(int seed, byte payloadMarker) =>
        CreateRequest(
            new[] { new Ra2GenerationReferenceImage("front.png", Ra2GenerationMediaKind.Png, CreateReferencePayload(payloadMarker)) },
            seed);

    private static Ra2GenerationRequest CreateRequest(IEnumerable<Ra2GenerationReferenceImage> references, int seed) =>
        new(
            Guid.NewGuid(),
            "Create one deterministic fixture mesh.",
            string.Empty,
            references,
            seed,
            candidateCount: 1,
            includePreviewPng: false,
            AssetHostTestFixture.ProviderId,
            AssetHostTestFixture.ModelRevision,
            TimeSpan.FromSeconds(20));

    private static byte[] CreateReferencePayload(byte marker) =>
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, marker };

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
