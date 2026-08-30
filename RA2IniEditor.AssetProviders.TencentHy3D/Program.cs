using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using RA2IniEditor.AssetProviders.TencentHy3D;

internal static class Program
{
    internal static async Task<int> Main(string[] args)
    {
        if (!TryReadArgument(args, "--ra2-asset-host-protocol", out string protocol) ||
            !string.Equals(protocol, TencentHy3DConstants.Protocol, StringComparison.Ordinal) ||
            !TryReadArgument(args, "--operation", out string operation))
        {
            return 2;
        }

        var writer = new TencentHy3DProtocolWriter();
        if (operation == "probe")
        {
            writer.Started("probe");
            string hash = await ComputeExecutableHashAsync().ConfigureAwait(false);
            writer.ProbeCompleted(hash, TryReadConfiguration(out _, out _));
            return 0;
        }

        if (operation != "generate" || !TryReadArgument(args, "--run-directory", out string runDirectory))
        {
            return 2;
        }

        TencentHy3DHostRequest request;
        try
        {
            request = TencentHy3DHostRequest.Load(runDirectory);
        }
        catch (Exception exception) when (exception is TencentHy3DRequestException or IOException or JsonException)
        {
            Console.Error.WriteLine("The Host request could not be loaded.");
            return 3;
        }

        writer.Started("generate", request.Fingerprint);
        if (!TryReadConfiguration(out string apiKey, out Uri? origin))
        {
            writer.Failed("ProviderNotReady", "Configure the dedicated Tencent Hunyuan 3D key and confirm free-only mode.");
            return 0;
        }

        string activeStage = "remote-request";
        try
        {
            using var handler = new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.All };
            using var httpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
            var client = new TencentHy3DClient(httpClient, origin!, apiKey);
            TencentHy3DCompletedJob job = await client.GenerateAsync(
                request, writer.Progress, CancellationToken.None).ConfigureAwait(false);

            activeStage = "artifact-download";
            writer.Progress("download", 92, "Downloading the generated GLB.");
            string meshPath = Path.Combine(request.OutputDirectory, "candidate-01.glb");
            await client.DownloadAsync(job.Mesh.Url, meshPath, TencentHy3DConstants.MaximumArtifactBytes, CancellationToken.None)
                .ConfigureAwait(false);
            EnsureGlb(meshPath);

            var artifacts = new List<TencentHy3DArtifact>
            {
                await DescribeAsync("mesh", "MeshGlb", meshPath).ConfigureAwait(false)
            };
            if (request.IncludePreviewPng && job.Preview is not null)
            {
                string previewPath = Path.Combine(request.OutputDirectory, "candidate-01.png");
                await client.DownloadAsync(job.Preview.Url, previewPath, 32L * 1024 * 1024, CancellationToken.None)
                    .ConfigureAwait(false);
                EnsurePng(previewPath);
                artifacts.Add(await DescribeAsync("preview", "PreviewPng", previewPath).ConfigureAwait(false));
            }

            activeStage = "provider-report";
            string reportPath = Path.Combine(request.OutputDirectory, "candidate-01.provider.json");
            await WriteProviderReportAsync(reportPath, request, job, artifacts).ConfigureAwait(false);
            artifacts.Add(await DescribeAsync("provider-report", "ProviderJson", reportPath).ConfigureAwait(false));

            writer.Progress("complete", 100, "Shape-only candidate is ready for Host validation.");
            writer.Candidate("candidate-01", artifacts);
            writer.Completed(request.Fingerprint, "candidate-01");
            return 0;
        }
        catch (TencentHy3DRequestException exception)
        {
            writer.Failed("InvalidRequest", exception.Message);
            return 0;
        }
        catch (TencentHy3DOutputMissingException exception)
        {
            writer.Failed("OutputMissing", exception.Message);
            return 0;
        }
        catch (TencentHy3DResourceException exception)
        {
            writer.Failed("ResourceLimitExceeded", exception.Message);
            return 0;
        }
        catch (TencentHy3DProviderException exception)
        {
            writer.Failed("ProviderReportedFailure", exception.Message);
            return 0;
        }
        catch (HttpRequestException exception)
        {
            string status = exception.StatusCode is null
                ? "none"
                : ((int)exception.StatusCode.Value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            writer.Failed(
                "ProviderReportedFailure",
                $"Remote HTTP transport failed during {activeStage} ({exception.HttpRequestError}; HTTP {status}).");
            return 0;
        }
        catch (IOException)
        {
            writer.Failed("ProviderReportedFailure", $"A bounded local I/O operation failed during {activeStage}.");
            return 0;
        }
        catch (JsonException)
        {
            writer.Failed("ProviderReportedFailure", $"JSON processing failed during {activeStage}.");
            return 0;
        }
    }

    internal static bool TryReadConfiguration(out string apiKey, out Uri? origin)
    {
        apiKey = ReadSetting(TencentHy3DConstants.ApiKeyEnvironmentVariable).Trim();
        string baseUrl = ReadSetting(TencentHy3DConstants.BaseUrlEnvironmentVariable).Trim();
        if (baseUrl.Length == 0)
        {
            baseUrl = TencentHy3DConstants.OfficialOrigin;
        }

        bool confirmed = string.Equals(
            ReadSetting(TencentHy3DConstants.FreeOnlyConfirmationEnvironmentVariable),
            "1", StringComparison.Ordinal);
        if (!apiKey.StartsWith("sk-", StringComparison.Ordinal) || apiKey.Length < 12 || !confirmed ||
            !Uri.TryCreate(baseUrl, UriKind.Absolute, out origin) ||
            origin.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(origin.Host, "api.ai3d.cloud.tencent.com", StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment) ||
            !string.Equals(origin.AbsolutePath.TrimEnd('/'), string.Empty, StringComparison.Ordinal))
        {
            apiKey = string.Empty;
            origin = null;
            return false;
        }

        origin = new Uri(TencentHy3DConstants.OfficialOrigin + "/", UriKind.Absolute);
        return true;
    }

    private static string ReadSetting(string name)
    {
        string processValue = Environment.GetEnvironmentVariable(name)?.Trim() ?? string.Empty;
        if (processValue.Length > 0 || !OperatingSystem.IsWindows())
        {
            return processValue;
        }

        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)?.Trim() ?? string.Empty;
    }

    private static async Task<string> ComputeExecutableHashAsync()
    {
        await using FileStream stream = File.OpenRead(Environment.ProcessPath!);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream).ConfigureAwait(false));
    }

    private static async Task<TencentHy3DArtifact> DescribeAsync(string id, string kind, string path)
    {
        var info = new FileInfo(path);
        await using FileStream stream = info.OpenRead();
        string hash = Convert.ToHexString(await SHA256.HashDataAsync(stream).ConfigureAwait(false));
        return new TencentHy3DArtifact(id, kind, info.Name, info.Length, hash);
    }

    private static async Task WriteProviderReportAsync(
        string path,
        TencentHy3DHostRequest request,
        TencentHy3DCompletedJob job,
        IReadOnlyList<TencentHy3DArtifact> artifacts)
    {
        byte[] report = JsonSerializer.SerializeToUtf8Bytes(new
        {
            schema = "ra2-tencent-hy3d-provider-evidence/1",
            providerId = TencentHy3DConstants.ProviderId,
            providerVersion = TencentHy3DConstants.ProviderVersion,
            modelId = TencentHy3DConstants.ModelId,
            modelRevision = TencentHy3DConstants.ModelRevision,
            generateType = "Geometry",
            enablePbr = false,
            requestFingerprint = request.Fingerprint,
            inputSha256 = request.InputSha256,
            jobId = job.JobId,
            submitRequestId = job.SubmitRequestId,
            queryRequestId = job.QueryRequestId,
            terminalStatus = job.Status,
            job.CreditsConsumed,
            job.CreditDetails,
            job.PollCount,
            seedBehavior = "Unsupported",
            promptWasSent = false,
            promptSha256 = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Prompt))),
            artifacts = artifacts.Select(artifact => new { artifact.ArtifactId, artifact.Kind, artifact.Length, artifact.Sha256 })
        });
        await File.WriteAllBytesAsync(path, report).ConfigureAwait(false);
    }

    private static void EnsureGlb(string path)
    {
        using FileStream stream = File.OpenRead(path);
        Span<byte> header = stackalloc byte[12];
        if (stream.Read(header) != header.Length || !header[..4].SequenceEqual("glTF"u8) ||
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(header[4..8]) != 2 ||
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(header[8..12]) != stream.Length)
        {
            throw new TencentHy3DProviderException("The downloaded GLB failed structural validation.");
        }
    }

    private static void EnsurePng(string path)
    {
        using FileStream stream = File.OpenRead(path);
        Span<byte> header = stackalloc byte[8];
        ReadOnlySpan<byte> signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        if (stream.Read(header) != header.Length || !header.SequenceEqual(signature))
        {
            throw new TencentHy3DProviderException("The downloaded preview is not a PNG file.");
        }
    }

    private static bool TryReadArgument(string[] args, string name, out string value)
    {
        for (int index = 0; index + 1 < args.Length; index++)
        {
            if (string.Equals(args[index], name, StringComparison.Ordinal))
            {
                value = args[index + 1];
                return true;
            }
        }

        value = string.Empty;
        return false;
    }
}
