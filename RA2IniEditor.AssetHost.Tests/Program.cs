using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace RA2IniEditor.AssetHost.Tests;

internal static class Program
{
    private const string ProviderId = "ra2-fixture-provider";
    private const string ProviderVersion = "1.0.0";
    private const string ModelId = "fixture-image-to-mesh";
    private const string ModelRevision = "fixture-r1";

    public static async Task<int> Main(string[] args)
    {
        if (TryReadArgument(args, "--fixture-child-hang", out string childRunId))
        {
            await RunHeartbeatChildAsync(childRunId).ConfigureAwait(false);
            return 0;
        }

        if (!TryReadArgument(args, "--ra2-asset-host-protocol", out string protocol) ||
            !string.Equals(protocol, "ra2-voxel-generation/1", StringComparison.Ordinal) ||
            !TryReadArgument(args, "--operation", out string operation))
        {
            return 0;
        }

        if (operation == "probe")
        {
            await WriteProbeAsync().ConfigureAwait(false);
            return 0;
        }

        if (operation == "generate" && TryReadArgument(args, "--run-directory", out string runDirectory))
        {
            return await GenerateAsync(runDirectory).ConfigureAwait(false);
        }

        return 3;
    }

    private static async Task WriteProbeAsync()
    {
        string modePath = Environment.ProcessPath! + ".probe-mode";
        string mode = File.Exists(modePath)
            ? (await File.ReadAllTextAsync(modePath).ConfigureAwait(false)).Trim()
            : string.Empty;
        if (string.Equals(mode, "hang", StringComparison.Ordinal))
        {
            await Task.Delay(Timeout.InfiniteTimeSpan).ConfigureAwait(false);
        }

        string executableHash = await ComputeExecutableHashAsync().ConfigureAwait(false);
        WriteLine(new
        {
            kind = "started",
            protocol = "ra2-voxel-generation/1",
            operation = "probe",
            providerId = ProviderId,
            providerVersion = ProviderVersion,
            modelId = ModelId,
            modelRevision = ModelRevision
        });
        WriteLine(new
        {
            kind = "probe_completed",
            descriptor = new
            {
                providerId = ProviderId,
                protocolVersion = 1,
                providerVersion = ProviderVersion,
                modelId = ModelId,
                modelRevision = ModelRevision,
                executableSha256 = executableHash,
                capabilities = string.Equals(mode, "capability-missing", StringComparison.Ordinal)
                    ? Array.Empty<string>()
                    : new[] { "ReferenceImageToMesh" },
                seedBehavior = "DeterministicDeclared",
                maximumReferenceCount = 4,
                maximumCandidateCount = 4,
                maximumInputBytes = 64 * 1024 * 1024,
                maximumOutputBytes = 512L * 1024 * 1024,
                licenseId = "fixture-license",
                licenseUrl = string.Empty,
                redistributable = true,
                requiresUserAcceptance = string.Equals(mode, "license-required", StringComparison.Ordinal)
            },
            modelReady = !string.Equals(mode, "not-ready", StringComparison.Ordinal)
        });
    }

    private static async Task<string> ComputeExecutableHashAsync()
    {
        await using FileStream stream = File.OpenRead(Environment.ProcessPath!);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream).ConfigureAwait(false));
    }

    private static async Task<int> GenerateAsync(string runDirectory)
    {
        string requestPath = Path.Combine(runDirectory, "staging", "request.json");
        using JsonDocument request = JsonDocument.Parse(await File.ReadAllBytesAsync(requestPath).ConfigureAwait(false));
        JsonElement root = request.RootElement;
        string fingerprint = root.GetProperty("fingerprint").GetString()!;
        string prompt = root.GetProperty("Prompt").GetString()!;
        string runId = root.GetProperty("runId").GetGuid().ToString("N");
        int seed = root.GetProperty("Seed").GetInt32();
        int candidateCount = root.GetProperty("CandidateCount").GetInt32();
        bool includePreview = root.GetProperty("IncludePreviewPng").GetBoolean();
        string outputRoot = Path.Combine(runDirectory, "staging", "provider-output");
        Directory.CreateDirectory(outputRoot);

        if (prompt.Contains("[fixture:stderr-flood]", StringComparison.Ordinal) ||
            prompt.Contains("[fixture:backpressure]", StringComparison.Ordinal))
        {
            Console.Error.Write(new string('e', 256 * 1024));
            Console.Error.Flush();
        }

        WriteLine(new
        {
            kind = "started",
            protocol = "ra2-voxel-generation/1",
            operation = "generate",
            providerId = ProviderId,
            providerVersion = ProviderVersion,
            modelId = ModelId,
            modelRevision = ModelRevision,
            requestFingerprint = fingerprint
        });

        if (prompt.Contains("[fixture:malformed]", StringComparison.Ordinal))
        {
            Console.Out.WriteLine("{not-json");
            Console.Out.Flush();
            return 0;
        }

        if (prompt.Contains("[fixture:duplicate-root]", StringComparison.Ordinal))
        {
            Console.Out.WriteLine("{\"kind\":\"progress\",\"kind\":\"progress\",\"sequence\":1,\"phase\":\"bad\"}");
            Console.Out.Flush();
            return 0;
        }

        if (prompt.Contains("[fixture:oversized-line]", StringComparison.Ordinal))
        {
            Console.Out.WriteLine(new string('x', 1024 * 1024 + 1));
            Console.Out.Flush();
            return 0;
        }

        if (prompt.Contains("[fixture:crash]", StringComparison.Ordinal))
        {
            return 9;
        }

        if (prompt.Contains("[fixture:failed]", StringComparison.Ordinal))
        {
            WriteLine(new { kind = "failed", failureKind = "ProviderReportedFailure", message = "Fixture failure." });
            return 0;
        }

        if (prompt.Contains("[fixture:hang]", StringComparison.Ordinal))
        {
            await Task.Delay(Timeout.InfiniteTimeSpan).ConfigureAwait(false);
        }

        if (prompt.Contains("[fixture:spawn-child-hang]", StringComparison.Ordinal))
        {
            var child = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath!,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            child.ArgumentList.Add("--fixture-child-hang");
            child.ArgumentList.Add(runId);
            Process.Start(child);
            await Task.Delay(Timeout.InfiniteTimeSpan).ConfigureAwait(false);
        }

        if (prompt.Contains("[fixture:progress-flood]", StringComparison.Ordinal))
        {
            for (int sequence = 1; sequence <= 1025; sequence++)
            {
                WriteLine(new { kind = "progress", sequence, phase = "flood", percent = 1.0, message = "flood" });
            }

            return 0;
        }

        if (prompt.Contains("[fixture:backpressure]", StringComparison.Ordinal))
        {
            for (int sequence = 1; sequence <= 100; sequence++)
            {
                WriteLine(new { kind = "progress", sequence, phase = "backpressure", percent = (double)sequence, message = "bounded" });
            }
        }
        else
        {
            WriteLine(new { kind = "progress", sequence = 1, phase = "generation", percent = 25.0, message = "Preparing fixture." });
        }

        var candidateIds = new List<string>();
        for (int index = 0; index < candidateCount; index++)
        {
            string candidateId = $"candidate-{index + 1:00}";
            candidateIds.Add(candidateId);
            byte[] glb = CreateFixtureGlb(fingerprint, seed, index);
            bool traversal = prompt.Contains("[fixture:path-traversal]", StringComparison.Ordinal);
            string glbName = traversal ? "../escape.glb" : candidateId + ".glb";
            string glbPath = traversal
                ? Path.Combine(runDirectory, "staging", "escape.glb")
                : Path.Combine(outputRoot, glbName);
            await File.WriteAllBytesAsync(glbPath, glb).ConfigureAwait(false);
            string glbHash = prompt.Contains("[fixture:hash-mismatch]", StringComparison.Ordinal)
                ? new string('A', 64)
                : Convert.ToHexString(SHA256.HashData(glb));
            var artifacts = new List<object>
            {
                new
                {
                    artifactId = "mesh",
                    kind = "MeshGlb",
                    path = glbName,
                    length = glb.LongLength,
                    sha256 = glbHash
                }
            };
            if (includePreview)
            {
                string pngName = candidateId + ".png";
                await File.WriteAllBytesAsync(Path.Combine(outputRoot, pngName), MinimalPng).ConfigureAwait(false);
                artifacts.Add(new
                {
                    artifactId = "preview",
                    kind = "PreviewPng",
                    path = pngName,
                    length = MinimalPng.LongLength,
                    sha256 = Convert.ToHexString(SHA256.HashData(MinimalPng))
                });
            }

            WriteLine(new { kind = "candidate", candidateId, artifacts });
        }

        if (prompt.Contains("[fixture:cancel-after-candidate]", StringComparison.Ordinal))
        {
            await Task.Delay(Timeout.InfiniteTimeSpan).ConfigureAwait(false);
        }

        long terminalProgressSequence = prompt.Contains("[fixture:backpressure]", StringComparison.Ordinal) ? 101 : 2;
        WriteLine(new { kind = "progress", sequence = terminalProgressSequence, phase = "generation", percent = 100.0, message = "Fixture complete." });
        WriteLine(new { kind = "completed", requestFingerprint = fingerprint, candidateIds });
        if (prompt.Contains("[fixture:post-terminal]", StringComparison.Ordinal))
        {
            WriteLine(new { kind = "progress", sequence = terminalProgressSequence + 1, phase = "invalid", percent = 100.0 });
        }

        return prompt.Contains("[fixture:nonzero-after-completed]", StringComparison.Ordinal) ? 7 : 0;
    }

    private static byte[] CreateFixtureGlb(string fingerprint, int seed, int index)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new
        {
            asset = new { version = "2.0", generator = "RA2IniEditor.AssetHost fixture" },
            extras = new { fingerprint, seed, candidate = index }
        });
        int paddedLength = (json.Length + 3) & ~3;
        int totalLength = 12 + 8 + paddedLength;
        byte[] glb = new byte[totalLength];
        "glTF"u8.CopyTo(glb);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(4, 4), 2);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(8, 4), checked((uint)totalLength));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(12, 4), checked((uint)paddedLength));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(16, 4), 0x4E4F534A);
        json.CopyTo(glb.AsSpan(20));
        glb.AsSpan(20 + json.Length, paddedLength - json.Length).Fill((byte)' ');
        return glb;
    }

    private static void WriteLine<T>(T value)
    {
        Console.Out.WriteLine(JsonSerializer.Serialize(value));
        Console.Out.Flush();
    }

    private static async Task RunHeartbeatChildAsync(string runId)
    {
        string path = Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath!)!,
            $"ra2-asset-host-child-{runId}.heartbeat");
        while (true)
        {
            await File.WriteAllTextAsync(path, DateTimeOffset.UtcNow.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .ConfigureAwait(false);
            await Task.Delay(50).ConfigureAwait(false);
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

    private static readonly byte[] MinimalPng =
    {
        137, 80, 78, 71, 13, 10, 26, 10,
        0, 0, 0, 13, 73, 72, 68, 82,
        0, 0, 0, 1, 0, 0, 0, 1,
        8, 6, 0, 0, 0, 31, 21, 196, 137,
        0, 0, 0, 13, 73, 68, 65, 84,
        8, 215, 99, 248, 207, 192, 240, 31,
        0, 5, 0, 1, 255, 137, 153, 61, 29,
        0, 0, 0, 0, 73, 69, 78, 68,
        174, 66, 96, 130
    };
}
