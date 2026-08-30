using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RA2IniEditor.AssetProviders.TencentHy3D;

namespace RA2IniEditor.AssetHost.Tests;

public sealed class TencentHy3DProviderTests
{
    [Fact]
    public void ProviderAssemblyExportsNoPublicTypes()
    {
        Assert.Empty(typeof(TencentHy3DClient).Assembly.GetExportedTypes());
    }

    [Fact]
    public async Task GenerateUsesFrozenGeometryPayloadAndParsesWrappedTerminalResult()
    {
        using var input = new TemporaryInput();
        var requests = new List<CapturedRequest>();
        using var http = new HttpClient(new ScriptedHandler(async request =>
        {
            string content = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync();
            requests.Add(new CapturedRequest(request.Method, request.RequestUri!, request.Headers.Authorization?.ToString(),
                request.Headers.TryGetValues("Authorization", out IEnumerable<string>? values) ? values.Single() : string.Empty,
                content));
            if (request.RequestUri!.AbsolutePath.EndsWith("/submit", StringComparison.Ordinal))
            {
                return Json(HttpStatusCode.OK, """{"JobId":"job-1","RequestId":"submit-1"}""");
            }

            return Json(HttpStatusCode.OK, """
                {"Response":{"Status":"DONE","RequestId":"query-1","ResultCreditConsumed":20,
                "ResultCreditDetails":"{\"GenerateType-Geometry\":20}","ResultFile3Ds":[
                {"Type":"GLB","Url":"https://cos.example.test/model.glb","PreviewImageUrl":"https://cos.example.test/preview.png"}]}}
                """);
        }));
        var client = new TencentHy3DClient(
            http, new Uri(TencentHy3DConstants.OfficialOrigin + "/"), "sk-test-dedicated", TimeSpan.Zero);

        TencentHy3DCompletedJob result = await client.GenerateAsync(input.Request, (_, _, _) => { }, CancellationToken.None);

        Assert.Equal("job-1", result.JobId);
        Assert.Equal(20, result.CreditsConsumed);
        Assert.Equal(2, requests.Count);
        Assert.All(requests, request => Assert.Equal("sk-test-dedicated", request.RawAuthorization));
        using JsonDocument payload = JsonDocument.Parse(requests[0].Content);
        Assert.Equal("3.1", payload.RootElement.GetProperty("Model").GetString());
        Assert.Equal("Geometry", payload.RootElement.GetProperty("GenerateType").GetString());
        Assert.False(payload.RootElement.GetProperty("EnablePBR").GetBoolean());
        Assert.NotEmpty(payload.RootElement.GetProperty("ImageBase64").GetString()!);
        Assert.False(payload.RootElement.TryGetProperty("ImageUrl", out _));
        Assert.False(payload.RootElement.TryGetProperty("Prompt", out _));
    }

    [Fact]
    public async Task HttpFailureKeepsOnlyBoundedProviderCode()
    {
        using var input = new TemporaryInput();
        using var http = new HttpClient(new ScriptedHandler(_ => Task.FromResult(Json(
            HttpStatusCode.BadRequest,
            """{"Response":{"Error":{"Code":"InvalidParameter.ImageBase64","Message":"raw details"}}}"""))));
        var client = new TencentHy3DClient(
            http, new Uri(TencentHy3DConstants.OfficialOrigin + "/"), "sk-test-dedicated", TimeSpan.Zero);

        TencentHy3DProviderException exception = await Assert.ThrowsAsync<TencentHy3DProviderException>(() =>
            client.GenerateAsync(input.Request, (_, _, _) => { }, CancellationToken.None));

        Assert.Contains("HTTP 400", exception.Message);
        Assert.Contains("InvalidParameter.ImageBase64", exception.Message);
        Assert.DoesNotContain("raw details", exception.Message);
        Assert.DoesNotContain("sk-test-dedicated", exception.Message);
    }

    [Fact]
    public async Task HostPreservesProviderSanitizedFailureMessage()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspace),
                AssetHostTestFixture.CreateRequest(prompt: "[fixture:failed]"));

            Assert.False(result.Succeeded);
            Assert.Equal(Ra2GenerationFailureKind.ProviderReportedFailure, result.FailureKind);
            Assert.Equal("Fixture failure.", result.Message);
        }
        finally
        {
            if (Directory.Exists(workspace))
            {
                Directory.Delete(workspace, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GenerateNeverSubmitsTheSameClientTwice()
    {
        using var input = new TemporaryInput();
        int requests = 0;
        using var http = new HttpClient(new ScriptedHandler(request =>
        {
            requests++;
            return Task.FromResult(request.RequestUri!.AbsolutePath.EndsWith("/submit", StringComparison.Ordinal)
                ? Json(HttpStatusCode.OK, """{"JobId":"job-1"}""")
                : Json(HttpStatusCode.OK, """{"Status":"DONE","ResultFile3Ds":[{"Type":"GLB","Url":"https://cos.example.test/model.glb"}]}"""));
        }));
        var client = new TencentHy3DClient(
            http, new Uri(TencentHy3DConstants.OfficialOrigin + "/"), "sk-test-dedicated", TimeSpan.Zero);

        await client.GenerateAsync(input.Request, (_, _, _) => { }, CancellationToken.None);
        await Assert.ThrowsAsync<TencentHy3DProviderException>(() =>
            client.GenerateAsync(input.Request, (_, _, _) => { }, CancellationToken.None));

        Assert.Equal(2, requests);
    }

    [Fact]
    public async Task ArtifactDownloadDoesNotForwardAuthorizationAndRejectsInsecureUrl()
    {
        string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".glb");
        try
        {
            using var http = new HttpClient(new ScriptedHandler(request =>
            {
                Assert.False(request.Headers.Contains("Authorization"));
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(CreateGlb())
                });
            }));
            var client = new TencentHy3DClient(
                http, new Uri(TencentHy3DConstants.OfficialOrigin + "/"), "sk-test-dedicated", TimeSpan.Zero);

            await client.DownloadAsync(new Uri("https://cos.example.test/model.glb"), destination, 1024 * 1024, CancellationToken.None);
            Assert.True(File.Exists(destination));
            await Assert.ThrowsAsync<TencentHy3DProviderException>(() =>
                client.DownloadAsync(new Uri("http://cos.example.test/model.glb"), destination + ".bad", 1024, CancellationToken.None));
        }
        finally
        {
            File.Delete(destination);
            File.Delete(destination + ".bad");
        }
    }

    [Fact]
    public async Task ExistingHostAcceptsProviderProbeProtocolAndMapsLocalReadinessWithoutNetworkAccess()
    {
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        try
        {
            string executable = FindProviderExecutable();
            var configuration = new Ra2GenerationProviderConfiguration(
                executable,
                AssetHostTestFixture.ComputeExecutableHash(executable),
                TencentHy3DConstants.ProviderId,
                TencentHy3DConstants.ProviderVersion,
                TencentHy3DConstants.ModelId,
                TencentHy3DConstants.ModelRevision,
                Ra2GenerationCapability.ReferenceImageToMesh,
                licenseAccepted: true,
                workspace);

            Ra2GenerationProbeResult result = await new Ra2VoxelGenerationHost().ProbeAsync(configuration);

            if (result.Succeeded)
            {
                Assert.Equal(TencentHy3DConstants.ProviderId, result.Descriptor!.ProviderId);
                Assert.Equal(Ra2GenerationSeedBehavior.Unsupported, result.Descriptor.SeedBehavior);
            }
            else
            {
                Assert.Equal(Ra2GenerationFailureKind.ProviderNotReady, result.FailureKind);
            }
        }
        finally
        {
            if (Directory.Exists(workspace))
            {
                Directory.Delete(workspace, recursive: true);
            }
        }
    }

    [Fact]
    public async Task LiveReferenceImageProducesHostValidatedGlbOnlyWhenExplicitlyEnabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RA2INI_HY3D_LIVE_TEST"), "1", StringComparison.Ordinal))
        {
            return;
        }

        string imagePath = Environment.GetEnvironmentVariable("RA2INI_HY3D_LIVE_IMAGE") ?? string.Empty;
        string evidenceRoot = Environment.GetEnvironmentVariable("RA2INI_HY3D_LIVE_OUTPUT") ?? string.Empty;
        Assert.True(File.Exists(imagePath), "RA2INI_HY3D_LIVE_IMAGE must identify the approved non-sensitive test image.");
        Assert.False(string.IsNullOrWhiteSpace(evidenceRoot));
        string executable = FindProviderExecutable();
        string workspace = AssetHostTestFixture.CreateUnusedWorkspacePath();
        try
        {
            var configuration = new Ra2GenerationProviderConfiguration(
                executable,
                AssetHostTestFixture.ComputeExecutableHash(executable),
                TencentHy3DConstants.ProviderId,
                TencentHy3DConstants.ProviderVersion,
                TencentHy3DConstants.ModelId,
                TencentHy3DConstants.ModelRevision,
                Ra2GenerationCapability.ReferenceImageToMesh,
                licenseAccepted: true,
                workspace);
            byte[] image = await File.ReadAllBytesAsync(imagePath);
            var request = new Ra2GenerationRequest(
                Guid.NewGuid(),
                "Generate the supplied wheeled armored vehicle as one shape-only geometry candidate.",
                "No texture or PBR. Do not infer VXL or HVA readiness.",
                new[] { new Ra2GenerationReferenceImage(Path.GetFileName(imagePath), Ra2GenerationMediaKind.Png, image) },
                seed: 1,
                candidateCount: 1,
                includePreviewPng: true,
                TencentHy3DConstants.ProviderId,
                TencentHy3DConstants.ModelRevision,
                TimeSpan.FromMinutes(20));

            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(configuration, request);
            Assert.True(result.Succeeded, result.Message);
            Assert.NotNull(result.Lease);
            await using IRa2GenerationWorkspaceLease lease = result.Lease!;
            Directory.CreateDirectory(evidenceRoot);
            foreach (Ra2GenerationArtifact artifact in lease.Candidates.Single().Artifacts)
            {
                await using Stream source = await lease.OpenArtifactReadAsync("candidate-01", artifact.ArtifactId);
                await using FileStream destination = File.Create(Path.Combine(evidenceRoot, artifact.ArtifactId + Extension(artifact.Kind)));
                await source.CopyToAsync(destination);
            }
        }
        finally
        {
            if (Directory.Exists(workspace))
            {
                Directory.Delete(workspace, recursive: true);
            }
        }
    }

    private static string Extension(Ra2GenerationArtifactKind kind) => kind switch
    {
        Ra2GenerationArtifactKind.MeshGlb => ".glb",
        Ra2GenerationArtifactKind.PreviewPng => ".png",
        Ra2GenerationArtifactKind.ProviderJson => ".json",
        _ => ".bin"
    };

    private static string FindProviderExecutable()
    {
        string fileName = OperatingSystem.IsWindows()
            ? "RA2IniEditor.AssetProviders.TencentHy3D.exe"
            : "RA2IniEditor.AssetProviders.TencentHy3D";
        string copied = Path.Combine(AppContext.BaseDirectory, fileName);
        if (File.Exists(copied))
        {
            return Path.GetFullPath(copied);
        }

        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "RA2IniEditor.IDE.sln")))
        {
            directory = directory.Parent;
        }

        string candidate = Path.Combine(directory?.FullName ?? string.Empty,
            "RA2IniEditor.AssetProviders.TencentHy3D", "bin", "Debug", "net8.0", fileName);
        return File.Exists(candidate) ? candidate : throw new FileNotFoundException("The Tencent provider apphost was not produced.", candidate);
    }

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string json) => new(statusCode)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private static byte[] CreateGlb()
    {
        byte[] json = Encoding.UTF8.GetBytes("{\"asset\":{\"version\":\"2.0\"}}");
        int padded = (json.Length + 3) & ~3;
        byte[] glb = new byte[20 + padded];
        "glTF"u8.CopyTo(glb);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(4, 4), 2);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(8, 4), (uint)glb.Length);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(12, 4), (uint)padded);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(16, 4), 0x4E4F534A);
        json.CopyTo(glb.AsSpan(20));
        glb.AsSpan(20 + json.Length).Fill((byte)' ');
        return glb;
    }

    private sealed class ScriptedHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => handler(request);
    }

    private sealed record CapturedRequest(HttpMethod Method, Uri Uri, string? ParsedAuthorization, string RawAuthorization, string Content);

    private sealed class TemporaryInput : IDisposable
    {
        private readonly string _root;

        internal TemporaryInput()
        {
            _root = Path.Combine(Path.GetTempPath(), "ra2-hy3d-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            string path = Path.Combine(_root, "input.png");
            File.WriteAllBytes(path, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 1, 2, 3, 4 });
            Request = new TencentHy3DHostRequest(
                Guid.NewGuid(), new string('A', 64), "shape", "none",
                TencentHy3DConstants.ProviderId, TencentHy3DConstants.ModelRevision, 60_000, false,
                path, "image/png", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), _root);
        }

        internal TencentHy3DHostRequest Request { get; }

        public void Dispose() => Directory.Delete(_root, recursive: true);
    }
}
