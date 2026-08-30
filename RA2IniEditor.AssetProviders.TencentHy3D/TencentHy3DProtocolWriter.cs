using System.Text.Json;

namespace RA2IniEditor.AssetProviders.TencentHy3D;

internal sealed class TencentHy3DProtocolWriter
{
    private long _sequence;

    internal void Started(string operation, string? requestFingerprint = null)
    {
        var message = new Dictionary<string, object?>
        {
            ["kind"] = "started",
            ["protocol"] = TencentHy3DConstants.Protocol,
            ["operation"] = operation,
            ["providerId"] = TencentHy3DConstants.ProviderId,
            ["providerVersion"] = TencentHy3DConstants.ProviderVersion,
            ["modelId"] = TencentHy3DConstants.ModelId,
            ["modelRevision"] = TencentHy3DConstants.ModelRevision
        };
        if (requestFingerprint is not null)
        {
            message["requestFingerprint"] = requestFingerprint;
        }

        Write(message);
    }

    internal void ProbeCompleted(string executableSha256, bool modelReady)
    {
        Write(new
        {
            kind = "probe_completed",
            descriptor = new
            {
                providerId = TencentHy3DConstants.ProviderId,
                protocolVersion = 1,
                providerVersion = TencentHy3DConstants.ProviderVersion,
                modelId = TencentHy3DConstants.ModelId,
                modelRevision = TencentHy3DConstants.ModelRevision,
                executableSha256,
                capabilities = new[] { "ReferenceImageToMesh" },
                seedBehavior = "Unsupported",
                maximumReferenceCount = 1,
                maximumCandidateCount = 1,
                maximumInputBytes = TencentHy3DConstants.MaximumImageBytes,
                maximumOutputBytes = TencentHy3DConstants.MaximumArtifactBytes,
                licenseId = "Tencent-Hunyuan-3D-Cloud-Service",
                licenseUrl = "https://cloud.tencent.com/document/product/1804",
                redistributable = false,
                requiresUserAcceptance = true
            },
            modelReady
        });
    }

    internal void Progress(string phase, double? percent, string message) =>
        Write(new { kind = "progress", sequence = ++_sequence, phase, percent, message });

    internal void Candidate(string candidateId, IReadOnlyList<TencentHy3DArtifact> artifacts) =>
        Write(new
        {
            kind = "candidate",
            candidateId,
            artifacts = artifacts.Select(artifact => new
            {
                artifactId = artifact.ArtifactId,
                kind = artifact.Kind,
                path = artifact.RelativePath,
                length = artifact.Length,
                sha256 = artifact.Sha256
            })
        });

    internal void Completed(string requestFingerprint, string candidateId) =>
        Write(new { kind = "completed", requestFingerprint, candidateIds = new[] { candidateId } });

    internal void Failed(string failureKind, string message) =>
        Write(new { kind = "failed", failureKind, message = Sanitize(message) });

    private static void Write<T>(T value)
    {
        Console.Out.WriteLine(JsonSerializer.Serialize(value));
        Console.Out.Flush();
    }

    private static string Sanitize(string message)
    {
        string compact = string.Join(' ', (message ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return compact.Length <= 512 ? compact : compact[..512];
    }
}

internal sealed record TencentHy3DArtifact(
    string ArtifactId,
    string Kind,
    string RelativePath,
    long Length,
    string Sha256);

