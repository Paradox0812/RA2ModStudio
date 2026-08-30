using System.Security.Cryptography;
using System.Text.Json;

namespace RA2IniEditor.AssetHost.Tests;

public sealed class Ra2GenerationWorkspaceTests
{
    [Fact]
    public async Task Run_promotes_verified_candidates_and_lease_disposal_removes_only_run()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        Guid runId = Guid.NewGuid();
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot),
                AssetHostTestFixture.CreateRequest(runId, includePreviewPng: true));

            Assert.True(result.Succeeded, result.Message);
            Assert.Equal(Ra2GenerationState.CandidateReady, result.State);
            Assert.NotNull(result.Lease);
            Ra2GenerationCandidate candidate = Assert.Single(result.Lease!.Candidates);
            Assert.Equal(new[] { Ra2GenerationArtifactKind.MeshGlb, Ra2GenerationArtifactKind.PreviewPng },
                candidate.Artifacts.Select(artifact => artifact.Kind));

            await using (Stream stream = await result.Lease.OpenArtifactReadAsync(candidate.CandidateId, "mesh"))
            {
                byte[] magic = new byte[4];
                Assert.Equal(4, await stream.ReadAsync(magic));
                Assert.Equal("glTF"u8.ToArray(), magic);
            }

            string runRoot = Path.Combine(workspaceRoot, runId.ToString("D"));
            Assert.True(File.Exists(Path.Combine(runRoot, "completed", "result.json")));
            Assert.False(Directory.Exists(Path.Combine(runRoot, "staging")));

            await result.Lease.DisposeAsync();
            Assert.False(Directory.Exists(runRoot));
            Assert.True(File.Exists(Path.Combine(workspaceRoot, ".ra2-asset-host-root")));
            await result.Lease.DisposeAsync();
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Workspace_rejects_existing_unmarked_content_without_deleting_it()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        Directory.CreateDirectory(workspaceRoot);
        string sentinel = Path.Combine(workspaceRoot, "user-data.txt");
        await File.WriteAllTextAsync(sentinel, "keep");
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot),
                AssetHostTestFixture.CreateRequest());

            Assert.False(result.Succeeded);
            Assert.Equal(Ra2GenerationFailureKind.WorkspaceRejected, result.FailureKind);
            Assert.Equal("keep", await File.ReadAllTextAsync(sentinel));
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Workspace_rejects_project_or_forbidden_root_overlap()
    {
        string protectedRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        string workspaceRoot = Path.Combine(protectedRoot, "asset-work");
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot, forbiddenRoots: new[] { protectedRoot }),
                AssetHostTestFixture.CreateRequest());

            Assert.Equal(Ra2GenerationFailureKind.WorkspaceRejected, result.FailureKind);
            Assert.False(Directory.Exists(workspaceRoot));
        }
        finally
        {
            DeleteDirectory(protectedRoot);
        }
    }

    [Fact]
    public async Task Janitor_deletes_only_marker_valid_unlocked_expired_run()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        Guid orphanId = Guid.NewGuid();
        string orphanRoot = Path.Combine(workspaceRoot, orphanId.ToString("D"));
        try
        {
            await CreateOwnedRootAsync(workspaceRoot);
            Directory.CreateDirectory(orphanRoot);
            await WriteRunMarkerAsync(orphanRoot, orphanId, DateTimeOffset.UtcNow - TimeSpan.FromHours(2));
            await File.WriteAllBytesAsync(Path.Combine(orphanRoot, ".active.lock"), Array.Empty<byte>());

            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot, orphanTtl: TimeSpan.FromHours(1)),
                AssetHostTestFixture.CreateRequest());

            Assert.True(result.Succeeded, result.Message);
            Assert.False(Directory.Exists(orphanRoot));
            await result.Lease!.DisposeAsync();
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Janitor_preserves_locked_run_and_allows_new_owned_run()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        Guid activeId = Guid.NewGuid();
        string activeRoot = Path.Combine(workspaceRoot, activeId.ToString("D"));
        await CreateOwnedRootAsync(workspaceRoot);
        Directory.CreateDirectory(activeRoot);
        await WriteRunMarkerAsync(activeRoot, activeId, DateTimeOffset.UtcNow - TimeSpan.FromHours(2));
        string lockPath = Path.Combine(activeRoot, ".active.lock");
        await using var activeLock = new FileStream(lockPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot, orphanTtl: TimeSpan.FromHours(1)),
                AssetHostTestFixture.CreateRequest());

            Assert.True(result.Succeeded, result.Message);
            Assert.True(Directory.Exists(activeRoot));
            await result.Lease!.DisposeAsync();
        }
        finally
        {
            await activeLock.DisposeAsync();
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Workspace_rejects_traversal_artifact_declaration()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(workspaceRoot);
        Ra2GenerationRequest request = AssetHostTestFixture.CreateRequest();
        try
        {
            Ra2WorkspacePreparationResult preparation = await Ra2GenerationWorkspace.PrepareAsync(
                configuration,
                request,
                CancellationToken.None);
            Assert.True(preparation.Succeeded, preparation.Message);
            var declaration = new Ra2GenerationCandidateDeclaration(
                "candidate-01",
                new[]
                {
                    new Ra2GenerationArtifactDeclaration(
                        "mesh",
                        Ra2GenerationArtifactKind.MeshGlb,
                        "../escape.glb",
                        24,
                        new string('A', 64))
                });

            Ra2WorkspacePromotionResult promotion = await preparation.Workspace!.ValidateAndPromoteAsync(
                CreateDescriptor(configuration),
                new[] { declaration },
                new[] { "candidate-01" },
                CancellationToken.None);

            Assert.False(promotion.Succeeded);
            Assert.Equal(Ra2GenerationFailureKind.OutputRejected, promotion.FailureKind);
            Assert.True(await preparation.Workspace.CleanupFailedRunAsync());
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Workspace_rejects_invalid_glb_even_when_hash_and_length_match()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var configuration = AssetHostTestFixture.CreateConfiguration(workspaceRoot);
        Ra2GenerationRequest request = AssetHostTestFixture.CreateRequest();
        try
        {
            Ra2WorkspacePreparationResult preparation = await Ra2GenerationWorkspace.PrepareAsync(
                configuration,
                request,
                CancellationToken.None);
            Assert.True(preparation.Succeeded, preparation.Message);
            byte[] invalid = Enumerable.Repeat((byte)7, 32).ToArray();
            await File.WriteAllBytesAsync(Path.Combine(preparation.Workspace!.ProviderOutputRoot, "bad.glb"), invalid);
            var declaration = new Ra2GenerationCandidateDeclaration(
                "candidate-01",
                new[]
                {
                    new Ra2GenerationArtifactDeclaration(
                        "mesh",
                        Ra2GenerationArtifactKind.MeshGlb,
                        "bad.glb",
                        invalid.LongLength,
                        Convert.ToHexString(SHA256.HashData(invalid)))
                });

            Ra2WorkspacePromotionResult promotion = await preparation.Workspace.ValidateAndPromoteAsync(
                CreateDescriptor(configuration),
                new[] { declaration },
                new[] { "candidate-01" },
                CancellationToken.None);

            Assert.Equal(Ra2GenerationFailureKind.OutputRejected, promotion.FailureKind);
            Assert.True(await preparation.Workspace.CleanupFailedRunAsync());
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    private static Ra2GenerationProviderDescriptor CreateDescriptor(Ra2GenerationProviderConfiguration configuration) =>
        new(
            AssetHostTestFixture.ProviderId,
            1,
            AssetHostTestFixture.ProviderVersion,
            AssetHostTestFixture.ModelId,
            AssetHostTestFixture.ModelRevision,
            configuration.ExpectedExecutableSha256,
            Ra2GenerationCapability.ReferenceImageToMesh,
            Ra2GenerationSeedBehavior.DeterministicDeclared,
            4,
            4,
            Ra2GenerationLimits.MaximumInputBytes,
            Ra2GenerationLimits.MaximumRunBytes,
            "fixture-license",
            string.Empty,
            true,
            false);

    private static async Task CreateOwnedRootAsync(string workspaceRoot)
    {
        Directory.CreateDirectory(workspaceRoot);
        await File.WriteAllTextAsync(Path.Combine(workspaceRoot, ".ra2-asset-host-root"), "ra2-asset-host-root/1\n");
    }

    private static async Task WriteRunMarkerAsync(string runRoot, Guid runId, DateTimeOffset activity)
    {
        byte[] marker = JsonSerializer.SerializeToUtf8Bytes(new
        {
            protocol = Ra2GenerationLimits.ProtocolIdentity,
            runId,
            state = "Staging",
            lastActivityUtc = activity
        });
        await File.WriteAllBytesAsync(Path.Combine(runRoot, ".ra2-run.json"), marker);
    }

    private static void DeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
        }
    }
}
