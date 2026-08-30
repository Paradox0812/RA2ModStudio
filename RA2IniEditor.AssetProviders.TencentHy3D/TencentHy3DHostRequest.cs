using System.Text.Json;

namespace RA2IniEditor.AssetProviders.TencentHy3D;

internal sealed record TencentHy3DHostRequest(
    Guid RunId,
    string Fingerprint,
    string Prompt,
    string NegativeConstraints,
    string ExpectedProviderId,
    string ExpectedModelRevision,
    long TimeoutMilliseconds,
    bool IncludePreviewPng,
    string InputPath,
    string InputMediaType,
    string InputSha256,
    string OutputDirectory)
{
    internal static TencentHy3DHostRequest Load(string runDirectory)
    {
        string normalizedRunDirectory = Path.GetFullPath(runDirectory);
        string stagingRoot = Path.Combine(normalizedRunDirectory, "staging");
        string requestPath = Path.Combine(stagingRoot, "request.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(requestPath), new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 16
        });
        JsonElement root = document.RootElement;
        RequireString(root, "schema", "ra2-generation-request/1");
        Guid runId = root.GetProperty("runId").GetGuid();
        string fingerprint = RequireBoundedString(root, "fingerprint", 64);
        string prompt = RequireBoundedString(root, "Prompt", 16 * 1024);
        string negative = RequireBoundedString(root, "NegativeConstraints", 8 * 1024);
        string providerId = RequireBoundedString(root, "ExpectedProviderId", 128);
        string revision = RequireBoundedString(root, "ExpectedModelRevision", 128);
        long timeoutMilliseconds = root.GetProperty("timeoutMilliseconds").GetInt64();
        bool includePreview = root.GetProperty("IncludePreviewPng").GetBoolean();
        int candidateCount = root.GetProperty("CandidateCount").GetInt32();
        JsonElement references = root.GetProperty("references");
        if (candidateCount != 1 || references.ValueKind != JsonValueKind.Array || references.GetArrayLength() != 1 ||
            timeoutMilliseconds is < 10_000 or > 1_800_000 ||
            !string.Equals(providerId, TencentHy3DConstants.ProviderId, StringComparison.Ordinal) ||
            !string.Equals(revision, TencentHy3DConstants.ModelRevision, StringComparison.Ordinal))
        {
            throw new TencentHy3DRequestException("The Host request is outside the certified provider profile.");
        }

        JsonElement reference = references[0];
        string mediaKind = RequireBoundedString(reference, "mediaKind", 16);
        string mediaType = mediaKind switch
        {
            "Png" => "image/png",
            "Jpeg" => "image/jpeg",
            "Webp" => "image/webp",
            _ => throw new TencentHy3DRequestException("The reference image format is unsupported.")
        };
        string relativeInput = RequireBoundedString(reference, "path", 512);
        string inputPath = ResolveContained(stagingRoot, relativeInput);
        string inputHash = RequireBoundedString(reference, "Sha256", 64);
        long declaredLength = reference.GetProperty("Length").GetInt64();
        var inputInfo = new FileInfo(inputPath);
        if (!inputInfo.Exists || declaredLength != inputInfo.Length || inputInfo.Length is <= 0 or > TencentHy3DConstants.MaximumImageBytes)
        {
            throw new TencentHy3DRequestException("The reference image is missing or exceeds the provider limit.");
        }

        string outputRelative = RequireBoundedString(root, "outputDirectory", 128);
        string outputDirectory = ResolveContained(stagingRoot, outputRelative);
        string expectedOutput = Path.GetFullPath(Path.Combine(stagingRoot, "provider-output"));
        if (!string.Equals(outputDirectory, expectedOutput, PathComparison))
        {
            throw new TencentHy3DRequestException("The provider output directory is invalid.");
        }

        Directory.CreateDirectory(outputDirectory);
        return new TencentHy3DHostRequest(
            runId, fingerprint, prompt, negative, providerId, revision, timeoutMilliseconds, includePreview,
            inputPath, mediaType, inputHash, outputDirectory);
    }

    private static string ResolveContained(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains(':') || relative.Contains('\0'))
        {
            throw new TencentHy3DRequestException("A staged path is invalid.");
        }

        string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(normalizedRoot, PathComparison))
        {
            throw new TencentHy3DRequestException("A staged path escaped the Host workspace.");
        }

        return path;
    }

    private static void RequireString(JsonElement root, string name, string expected)
    {
        if (!root.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.String ||
            !string.Equals(value.GetString(), expected, StringComparison.Ordinal))
        {
            throw new TencentHy3DRequestException("The Host request schema is invalid.");
        }
    }

    private static string RequireBoundedString(JsonElement root, string name, int maximumLength)
    {
        if (!root.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.String)
        {
            throw new TencentHy3DRequestException("The Host request is missing required text.");
        }

        string text = value.GetString() ?? string.Empty;
        if (text.Length == 0 || text.Length > maximumLength)
        {
            throw new TencentHy3DRequestException("The Host request contains invalid text.");
        }

        return text;
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}

internal sealed class TencentHy3DRequestException : Exception
{
    internal TencentHy3DRequestException(string message) : base(message) { }
}

