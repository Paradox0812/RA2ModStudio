extern alias Ra2Application;

using System.IO;
using System.Text;
using System.Text.Json;
using RA2IniEditor.IDE.AI;
using Ra2VoxelSemanticAssignment = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticAssignment;
using Ra2VoxelSemanticEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidencePackage;
using Ra2VoxelSemanticMaterialRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaterialRole;
using Ra2VoxelSemanticPartRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartRole;
using Ra2VoxelSemanticRemapIntent = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticRemapIntent;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelSemanticMaskCompilerFailureKind
{
    None = 0,
    ProviderFailure,
    MalformedProposal,
    ArbitrationFailed,
    Cancelled
}

internal sealed record Ra2VoxelSemanticMaskCompilerResult(
    Ra2VoxelSemanticMaskCompilerFailureKind FailureKind,
    string Message,
    IReadOnlyList<Ra2VoxelSemanticAssignment> Suggestions,
    IReadOnlyList<Ra2AiRequest> Requests,
    bool UsedArbitration)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticMaskCompilerFailureKind.None;
}

internal sealed record Ra2VoxelSemanticAnalysisResult(
    string Message,
    Ra2VoxelSemanticEvidencePackage? Evidence,
    Ra2VoxelSemanticMaskCompilerResult? CompilerResult)
{
    internal bool IsSuccess => Evidence is not null && CompilerResult?.IsSuccess == true;
}

internal sealed class Ra2VoxelSemanticMaskCompiler
{
    internal const string ToolName = "suggest_ra2_voxel_semantics";
    private const int MaximumPromptCharacters = 32_768;
    private readonly IRa2AiClient _client;

    internal Ra2VoxelSemanticMaskCompiler(IRa2AiClient client) =>
        _client = client ?? throw new ArgumentNullException(nameof(client));

    internal async Task<Ra2VoxelSemanticMaskCompilerResult> CompileAsync(
        Ra2VoxelSemanticEvidencePackage evidence,
        string? userInstructions,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        List<Ra2AiRequest> requests = [];
        Parsed primary = await SendAsync(BuildRequest(evidence, userInstructions, Pass.Primary, null, null), evidence, requests, cancellationToken)
            .ConfigureAwait(false);
        if (primary.Failure is not null) return primary.Failure;
        Parsed review = await SendAsync(BuildRequest(evidence, userInstructions, Pass.Reviewer, primary.Assignments, null), evidence, requests, cancellationToken)
            .ConfigureAwait(false);
        if (review.Failure is not null) return review.Failure;

        string primaryFingerprint = Fingerprint(primary.Assignments);
        string reviewFingerprint = Fingerprint(review.Assignments);
        if (string.Equals(primaryFingerprint, reviewFingerprint, StringComparison.Ordinal))
            return Success(review.Assignments, requests, usedArbitration: false);

        Parsed arbitration = await SendAsync(
            BuildRequest(evidence, userInstructions, Pass.Arbitrator, primary.Assignments, review.Assignments),
            evidence,
            requests,
            cancellationToken).ConfigureAwait(false);
        if (arbitration.Failure is not null)
            return arbitration.Failure with { FailureKind = Ra2VoxelSemanticMaskCompilerFailureKind.ArbitrationFailed };
        return Success(arbitration.Assignments, requests, usedArbitration: true);
    }

    private async Task<Parsed> SendAsync(
        Ra2AiRequest request,
        Ra2VoxelSemanticEvidencePackage evidence,
        List<Ra2AiRequest> requests,
        CancellationToken cancellationToken)
    {
        requests.Add(request);
        if (request.PromptCharacterCount > MaximumPromptCharacters)
            return Parsed.Fail(Failure(Ra2VoxelSemanticMaskCompilerFailureKind.MalformedProposal, "语义证据超过文本输入上限。", requests));
        Ra2AiResponse response;
        try
        {
            response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Parsed.Fail(Failure(Ra2VoxelSemanticMaskCompilerFailureKind.Cancelled, "语义识别已取消。", requests));
        }
        if (response.Kind == Ra2AiResponseKind.Cancelled)
            return Parsed.Fail(Failure(Ra2VoxelSemanticMaskCompilerFailureKind.Cancelled, "语义识别已取消。", requests));
        if (response.Kind != Ra2AiResponseKind.ToolCalls)
            return Parsed.Fail(Failure(Ra2VoxelSemanticMaskCompilerFailureKind.ProviderFailure,
                string.IsNullOrWhiteSpace(response.ErrorMessage) ? "DeepSeek 未返回结构化语义建议。" : response.ErrorMessage,
                requests));
        return Parse(response, evidence, requests);
    }

    private static Ra2AiRequest BuildRequest(
        Ra2VoxelSemanticEvidencePackage evidence,
        string? userInstructions,
        Pass pass,
        IReadOnlyList<Ra2VoxelSemanticAssignment>? primary,
        IReadOnlyList<Ra2VoxelSemanticAssignment>? review)
    {
        StringBuilder system = new();
        system.AppendLine("You are a text-only semantic reviewer for an RA2 vehicle voxel model.");
        system.AppendLine($"Call {ToolName} exactly once and return no prose.");
        system.AppendLine("Use only the supplied region IDs and geometry facts. You cannot see the source image or voxel render.");
        system.AppendLine("Suggest part and material roles; do not modify geometry, coordinates, occupancy, palette indices, files, or save state.");
        system.AppendLine("Unknown is valid when evidence is insufficient. Remap may only be candidate; explicit approval belongs to the human.");
        system.AppendLine("Omitted regions remain Unknown. Do not invent colours from geometry.");
        system.AppendLine(pass switch
        {
            Pass.Primary => "Act as the primary classifier.",
            Pass.Reviewer => "Independently review the primary suggestions and return your complete normalized suggestion set.",
            Pass.Arbitrator => "Primary and reviewer differ. Return the sole final suggestion set, resolving only from the same evidence.",
            _ => throw new ArgumentOutOfRangeException(nameof(pass))
        });

        StringBuilder user = new(evidence.ToPromptText(userInstructions));
        if (primary is not null)
            user.AppendLine("primary_suggestions_json:").AppendLine(Serialize(primary));
        if (review is not null)
            user.AppendLine("review_suggestions_json:").AppendLine(Serialize(review));
        string systemText = system.ToString();
        string userText = user.ToString();
        return new(
            Ra2AiIntent.Auto,
            pass == Pass.Primary ? "Suggest voxel semantics" : pass == Pass.Reviewer ? "Review voxel semantics" : "Arbitrate voxel semantics",
            string.Concat(systemText, Environment.NewLine, userText),
            tools: [BuildTool()],
            toolChoice: Ra2AiToolChoiceMode.Required,
            systemPromptText: systemText,
            userContentText: userText);
    }

    private static Ra2AiToolDefinition BuildTool() => new(
        ToolName,
        "Return bounded semantic suggestions for host-provided voxel regions.",
        """
        {"type":"object","properties":{"evidence_hash":{"type":"string","minLength":64,"maxLength":64},"assignments":{"type":"array","maxItems":48,"items":{"type":"object","properties":{"region_id":{"type":"string","maxLength":64},"part_role":{"type":"string","enum":["unknown","body_shell","turret","barrel","wheel","track","antenna","attachment"]},"material_role":{"type":"string","enum":["unknown","painted_surface","glass","rubber","bare_metal","light","dark_opening","accent"]},"remap_intent":{"type":"string","enum":["none","candidate"]},"confidence":{"type":"number","minimum":0,"maximum":1},"reason":{"type":"string","maxLength":512}},"required":["region_id","part_role","material_role"]}}},"required":["evidence_hash","assignments"]}
        """);

    private static Parsed Parse(
        Ra2AiResponse response,
        Ra2VoxelSemanticEvidencePackage evidence,
        IReadOnlyList<Ra2AiRequest> requests)
    {
        Ra2AiToolCall[] calls = response.ToolCalls.Where(call => string.Equals(call.Name, ToolName, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (calls.Length != 1)
            return Parsed.Fail(Failure(Ra2VoxelSemanticMaskCompilerFailureKind.MalformedProposal, $"tool_call_count:{calls.Length}", requests));
        try
        {
            using JsonDocument document = JsonDocument.Parse(Unwrap(calls[0].ArgumentsJson), new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
                MaxDepth = 12
            });
            JsonElement root = document.RootElement;
            string hash = String(root, "evidence_hash", "evidenceHash").ToUpperInvariant();
            if (!string.Equals(hash, evidence.PackageHash, StringComparison.Ordinal))
                throw new InvalidDataException("evidence_hash_mismatch");
            JsonElement array = Property(root, "assignments");
            if (array.ValueKind != JsonValueKind.Array || array.GetArrayLength() > evidence.Regions.Count)
                throw new InvalidDataException("invalid_assignments");
            HashSet<string> validIds = evidence.Regions.Select(value => value.RegionId).ToHashSet(StringComparer.Ordinal);
            HashSet<string> seen = new(StringComparer.Ordinal);
            List<Ra2VoxelSemanticAssignment> values = [];
            foreach (JsonElement item in array.EnumerateArray())
            {
                string regionId = String(item, "region_id", "regionId");
                if (!validIds.Contains(regionId) || !seen.Add(regionId)) throw new InvalidDataException("invalid_region_id");
                double confidence = Number(item, 0.5d, "confidence");
                if (!double.IsFinite(confidence) || confidence is < 0d or > 1d) throw new InvalidDataException("invalid_confidence");
                values.Add(new(
                    regionId,
                    PartRole(String(item, "part_role", "partRole")),
                    MaterialRole(String(item, "material_role", "materialRole")),
                    RemapIntent(OptionalString(item, "remap_intent", "remapIntent")),
                    confidence,
                    OptionalString(item, "reason")));
            }
            return new(Array.AsReadOnly(values.ToArray()), null);
        }
        catch (Exception exception) when (exception is JsonException or InvalidDataException or InvalidOperationException or FormatException)
        {
            return Parsed.Fail(Failure(Ra2VoxelSemanticMaskCompilerFailureKind.MalformedProposal, exception.Message, requests));
        }
    }

    private static string Fingerprint(IEnumerable<Ra2VoxelSemanticAssignment> values) => string.Join('|', values
        .OrderBy(value => value.RegionId, StringComparer.Ordinal)
        .Select(value => $"{value.RegionId}:{value.PartRole}:{value.MaterialRole}:{value.RemapIntent}"));

    private static string Serialize(IEnumerable<Ra2VoxelSemanticAssignment> values) => JsonSerializer.Serialize(values.Select(value => new
    {
        region_id = value.RegionId,
        part_role = Token(value.PartRole),
        material_role = Token(value.MaterialRole),
        remap_intent = value.RemapIntent == Ra2VoxelSemanticRemapIntent.Candidate ? "candidate" : "none"
    }));

    private static string Unwrap(string value)
    {
        string current = value.Trim();
        if (current.StartsWith("```", StringComparison.Ordinal))
        {
            int firstLine = current.IndexOf('\n');
            int closing = current.LastIndexOf("```", StringComparison.Ordinal);
            if (firstLine >= 0 && closing > firstLine) current = current[(firstLine + 1)..closing].Trim();
        }
        using JsonDocument wrapper = JsonDocument.Parse(current);
        if (wrapper.RootElement.ValueKind == JsonValueKind.String)
            return wrapper.RootElement.GetString() ?? "{}";
        if (wrapper.RootElement.ValueKind == JsonValueKind.Object &&
            !wrapper.RootElement.TryGetProperty("assignments", out _) &&
            wrapper.RootElement.TryGetProperty("arguments", out JsonElement arguments))
            return arguments.ValueKind == JsonValueKind.String ? arguments.GetString() ?? "{}" : arguments.GetRawText();
        return current;
    }

    private static JsonElement Property(JsonElement root, params string[] names)
    {
        foreach (JsonProperty property in root.EnumerateObject())
            if (names.Any(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase))) return property.Value;
        throw new InvalidDataException("missing_property:" + names[0]);
    }

    private static string String(JsonElement root, params string[] names)
    {
        JsonElement value = Property(root, names);
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
            throw new InvalidDataException("invalid_string:" + names[0]);
        return value.GetString()!.Trim();
    }

    private static string OptionalString(JsonElement root, params string[] names)
    {
        try { return String(root, names); }
        catch (InvalidDataException) { return string.Empty; }
    }

    private static double Number(JsonElement root, double fallback, params string[] names)
    {
        try
        {
            JsonElement value = Property(root, names);
            return value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double number) ? number : fallback;
        }
        catch (InvalidDataException) { return fallback; }
    }

    private static Ra2VoxelSemanticPartRole PartRole(string value) => value.ToLowerInvariant().Replace('-', '_') switch
    {
        "unknown" => Ra2VoxelSemanticPartRole.Unknown,
        "body_shell" => Ra2VoxelSemanticPartRole.BodyShell,
        "turret" => Ra2VoxelSemanticPartRole.Turret,
        "barrel" => Ra2VoxelSemanticPartRole.Barrel,
        "wheel" => Ra2VoxelSemanticPartRole.Wheel,
        "track" => Ra2VoxelSemanticPartRole.Track,
        "antenna" => Ra2VoxelSemanticPartRole.Antenna,
        "attachment" => Ra2VoxelSemanticPartRole.Attachment,
        _ => throw new InvalidDataException("invalid_part_role")
    };

    private static Ra2VoxelSemanticMaterialRole MaterialRole(string value) => value.ToLowerInvariant().Replace('-', '_') switch
    {
        "unknown" => Ra2VoxelSemanticMaterialRole.Unknown,
        "painted_surface" => Ra2VoxelSemanticMaterialRole.PaintedSurface,
        "glass" => Ra2VoxelSemanticMaterialRole.Glass,
        "rubber" => Ra2VoxelSemanticMaterialRole.Rubber,
        "bare_metal" => Ra2VoxelSemanticMaterialRole.BareMetal,
        "light" => Ra2VoxelSemanticMaterialRole.Light,
        "dark_opening" => Ra2VoxelSemanticMaterialRole.DarkOpening,
        "accent" => Ra2VoxelSemanticMaterialRole.Accent,
        _ => throw new InvalidDataException("invalid_material_role")
    };

    private static Ra2VoxelSemanticRemapIntent RemapIntent(string value) => value.ToLowerInvariant() switch
    {
        "candidate" => Ra2VoxelSemanticRemapIntent.Candidate,
        _ => Ra2VoxelSemanticRemapIntent.None
    };

    private static string Token(Ra2VoxelSemanticPartRole value) => value switch
    {
        Ra2VoxelSemanticPartRole.BodyShell => "body_shell",
        Ra2VoxelSemanticPartRole.Turret => "turret",
        Ra2VoxelSemanticPartRole.Barrel => "barrel",
        Ra2VoxelSemanticPartRole.Wheel => "wheel",
        Ra2VoxelSemanticPartRole.Track => "track",
        Ra2VoxelSemanticPartRole.Antenna => "antenna",
        Ra2VoxelSemanticPartRole.Attachment => "attachment",
        _ => "unknown"
    };

    private static string Token(Ra2VoxelSemanticMaterialRole value) => value switch
    {
        Ra2VoxelSemanticMaterialRole.PaintedSurface => "painted_surface",
        Ra2VoxelSemanticMaterialRole.Glass => "glass",
        Ra2VoxelSemanticMaterialRole.Rubber => "rubber",
        Ra2VoxelSemanticMaterialRole.BareMetal => "bare_metal",
        Ra2VoxelSemanticMaterialRole.Light => "light",
        Ra2VoxelSemanticMaterialRole.DarkOpening => "dark_opening",
        Ra2VoxelSemanticMaterialRole.Accent => "accent",
        _ => "unknown"
    };

    private static Ra2VoxelSemanticMaskCompilerResult Success(
        IReadOnlyList<Ra2VoxelSemanticAssignment> values,
        IReadOnlyList<Ra2AiRequest> requests,
        bool usedArbitration) => new(Ra2VoxelSemanticMaskCompilerFailureKind.None, string.Empty,
            Array.AsReadOnly(values.ToArray()), Array.AsReadOnly(requests.ToArray()), usedArbitration);

    private static Ra2VoxelSemanticMaskCompilerResult Failure(
        Ra2VoxelSemanticMaskCompilerFailureKind kind,
        string message,
        IReadOnlyList<Ra2AiRequest> requests) => new(kind, message, [], Array.AsReadOnly(requests.ToArray()), false);

    private enum Pass { Primary, Reviewer, Arbitrator }
    private sealed record Parsed(IReadOnlyList<Ra2VoxelSemanticAssignment> Assignments, Ra2VoxelSemanticMaskCompilerResult? Failure)
    {
        internal static Parsed Fail(Ra2VoxelSemanticMaskCompilerResult failure) => new([], failure);
    }
}
