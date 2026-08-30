extern alias Ra2Application;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RA2IniEditor.IDE.AI;
using Ra2VoxelUnitClass = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClass;
using Ra2VoxelUnitClassConfidenceBand = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassConfidenceBand;
using Ra2VoxelUnitClassEvidence = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassEvidence;
using Ra2VoxelUnitClassProposal = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassProposal;
using Ra2VoxelUnitClassProposalInput = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassProposalInput;
using Ra2VoxelUnitClassProposalResult = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassProposalResult;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelUnitClassAssessmentFailureKind
{
    None = 0,
    ClassifierSkillUnavailable,
    InvalidProviderModelIdentity,
    ProviderUnavailable,
    ProviderTimeout,
    ProviderFailure,
    MalformedProposal,
    Cancelled
}

internal sealed record Ra2VoxelUnitClassAssessmentResult(
    Ra2VoxelUnitClassAssessmentFailureKind FailureKind,
    string Message,
    Ra2VoxelUnitClassProposal? Proposal,
    bool CacheHit,
    int ProviderCallCount,
    Ra2AiRequest? Request)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelUnitClassAssessmentFailureKind.None && Proposal is not null;
}

internal sealed class Ra2VoxelUnitClassClassifier
{
    internal const string ToolName = "assess_ra2_voxel_unit_class";
    internal const string SchemaRevision = "ra2-voxel-unit-classifier/1";
    private const int MaximumArgumentsCharacters = 16 * 1024;
    private static readonly HashSet<string> RootProperties = new(StringComparer.Ordinal)
    {
        "proposed_class", "confidence_band", "evidence_fact_ids", "reason", "evidence_hash"
    };

    private readonly IRa2AiClient _client;
    private readonly Ra2VoxelUnitClassProposalCache _cache;
    private readonly Ra2AgentSkillCatalog _skillCatalog;

    internal Ra2VoxelUnitClassClassifier(
        IRa2AiClient client,
        Ra2VoxelUnitClassProposalCache cache,
        Ra2AgentSkillCatalog skillCatalog)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _skillCatalog = skillCatalog ?? throw new ArgumentNullException(nameof(skillCatalog));
    }

    internal async Task<Ra2VoxelUnitClassAssessmentResult> AssessAsync(
        Ra2VoxelUnitClassEvidence evidence,
        string providerModelIdentity,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        string normalizedProviderModelIdentity = providerModelIdentity?.Trim() ?? string.Empty;
        if (!IsBounded(normalizedProviderModelIdentity, 256))
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.InvalidProviderModelIdentity,
                "A bounded Provider model identity is required before unit-class assessment.");

        Ra2AgentSkillDescriptor? skill = ResolveClassifierSkill();
        if (skill is null)
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.ClassifierSkillUnavailable,
                "The required unit-classification Skill is unavailable or exceeds the instruction limit.");

        string key = ComputeCacheKey(evidence, skill, normalizedProviderModelIdentity);
        if (_cache.TryRead(key, out string cachedJson) &&
            TryReadCachedProposal(cachedJson, evidence, skill, normalizedProviderModelIdentity, out Ra2VoxelUnitClassProposal? cached))
        {
            return new(Ra2VoxelUnitClassAssessmentFailureKind.None, string.Empty, cached, true, 0, null);
        }

        Ra2AiRequest request = BuildRequest(evidence, skill);
        Ra2AiResponse response;
        try
        {
            response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.Cancelled,
                "Unit-class assessment was cancelled.", request, 1);
        }

        if (response.Kind == Ra2AiResponseKind.Cancelled)
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.Cancelled, "Unit-class assessment was cancelled.", request, 1);
        if (response.Kind == Ra2AiResponseKind.MissingConfiguration)
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.ProviderUnavailable,
                "DeepSeek unit-class assessment is not configured.", request, 1);
        if (response.Kind == Ra2AiResponseKind.Timeout)
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.ProviderTimeout,
                "DeepSeek unit-class assessment timed out.", request, 1);
        if (response.Kind != Ra2AiResponseKind.ToolCalls)
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.ProviderFailure,
                "DeepSeek did not return a structured unit-class proposal.", request, 1);

        Ra2VoxelUnitClassProposalResult parsed = Parse(response, evidence, skill);
        if (!parsed.IsSuccess)
            return Failure(Ra2VoxelUnitClassAssessmentFailureKind.MalformedProposal,
                string.IsNullOrWhiteSpace(parsed.Message) ? "The unit-class proposal is invalid." : parsed.Message,
                request,
                1);

        _cache.Store(key, SerializeCachedProposal(
            parsed.Proposal!, evidence, skill, normalizedProviderModelIdentity));
        return new(Ra2VoxelUnitClassAssessmentFailureKind.None, string.Empty, parsed.Proposal, false, 1, request);
    }

    private Ra2AgentSkillDescriptor? ResolveClassifierSkill()
    {
        Ra2AgentSkillDescriptor[] matches = _skillCatalog.Skills
            .Where(skill => string.Equals(skill.Name, Ra2VoxelUnitClassProposal.RequiredClassifierSkillId, StringComparison.Ordinal))
            .ToArray();
        return matches.Length == 1 && IsIdentifier(matches[0].Version) &&
               matches[0].ContentHash.Length == 64 && matches[0].ContentHash.All(char.IsAsciiHexDigit) &&
               matches[0].Instructions.Length <= Ra2AgentSkillCatalog.MaximumSelectedSkillCharacters
            ? matches[0]
            : null;
    }

    private static Ra2AiRequest BuildRequest(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2AgentSkillDescriptor skill)
    {
        string system = string.Join(Environment.NewLine,
            skill.Instructions,
            $"Call {ToolName} exactly once and return no prose.",
            "Use only the supplied fact IDs. Do not choose a colouring Skill, palette colour, mask, coordinate, or write action.",
            $"Classifier Skill identity: {skill.Name}@{skill.Version}; sha256={skill.ContentHash}.");
        string user = evidence.ToPromptText();
        return new Ra2AiRequest(
            Ra2AiIntent.Auto,
            "Assess voxel unit class",
            string.Concat(system, Environment.NewLine, user),
            tools: [BuildTool()],
            toolChoice: Ra2AiToolChoiceMode.Required,
            systemPromptText: system,
            userContentText: user);
    }

    private static Ra2AiToolDefinition BuildTool() => new(
        ToolName,
        "Return one evidence-bound RA2 voxel unit-class proposal for human confirmation.",
        """
        {"type":"object","additionalProperties":false,"properties":{"proposed_class":{"type":"string","enum":["ground","air","large_surface","unknown"]},"confidence_band":{"type":"string","enum":["high","medium","low"]},"evidence_fact_ids":{"type":"array","minItems":1,"maxItems":32,"items":{"type":"string","maxLength":96}},"reason":{"type":"string","minLength":1,"maxLength":512},"evidence_hash":{"type":"string","minLength":64,"maxLength":64}},"required":["proposed_class","confidence_band","evidence_fact_ids","reason","evidence_hash"]}
        """);

    private static Ra2VoxelUnitClassProposalResult Parse(
        Ra2AiResponse response,
        Ra2VoxelUnitClassEvidence evidence,
        Ra2AgentSkillDescriptor skill)
    {
        if (response.ToolCalls.Count != 1 ||
            !string.Equals(response.ToolCalls[0].Name, ToolName, StringComparison.Ordinal) ||
            response.ToolCalls[0].ArgumentsJson.Length > MaximumArgumentsCharacters)
        {
            return Invalid("The unit-class assessor returned an invalid tool call.");
        }
        try
        {
            using JsonDocument document = JsonDocument.Parse(response.ToolCalls[0].ArgumentsJson,
                new JsonDocumentOptions { AllowTrailingCommas = false, CommentHandling = JsonCommentHandling.Disallow, MaxDepth = 8 });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !HasExactProperties(root, RootProperties))
                return Invalid("The unit-class proposal root shape is invalid.");
            string[] factIds = ReadStringArray(root.GetProperty("evidence_fact_ids"), 32, 96);
            Ra2VoxelUnitClassProposalInput input = new(
                ParseClass(ReadString(root, "proposed_class", 32)),
                ParseConfidence(ReadString(root, "confidence_band", 16)),
                Array.AsReadOnly(factIds),
                ReadString(root, "reason", 512),
                skill.Name,
                skill.Version,
                skill.ContentHash,
                ReadString(root, "evidence_hash", 64));
            return Ra2VoxelUnitClassProposal.Validate(evidence, input);
        }
        catch (Exception exception) when (exception is JsonException or InvalidDataException or InvalidOperationException or FormatException)
        {
            return Invalid("The unit-class proposal JSON is malformed or outside local bounds.");
        }
    }

    private static string SerializeCachedProposal(
        Ra2VoxelUnitClassProposal proposal,
        Ra2VoxelUnitClassEvidence evidence,
        Ra2AgentSkillDescriptor skill,
        string providerModelIdentity) => JsonSerializer.Serialize(new
        {
            schema_version = 1,
            classifier_schema_revision = SchemaRevision,
            model_identity = evidence.ModelIdentity,
            evidence_hash = evidence.EvidenceHash,
            classifier_skill_id = skill.Name,
            classifier_skill_revision = skill.Version,
            classifier_skill_content_hash = skill.ContentHash,
            provider_model_identity = providerModelIdentity,
            proposal_hash = proposal.ProposalHash,
            proposed_class = FormatClass(proposal.ProposedClass),
            confidence_band = FormatConfidence(proposal.ConfidenceBand),
            evidence_fact_ids = proposal.EvidenceFactIds,
            reason = proposal.Reason
        });

    private static bool TryReadCachedProposal(
        string json,
        Ra2VoxelUnitClassEvidence evidence,
        Ra2AgentSkillDescriptor skill,
        string providerModelIdentity,
        out Ra2VoxelUnitClassProposal? proposal)
    {
        proposal = null;
        try
        {
            using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
            JsonElement root = document.RootElement;
            if (root.GetProperty("schema_version").GetInt32() != 1 ||
                root.GetProperty("classifier_schema_revision").GetString() != SchemaRevision ||
                root.GetProperty("model_identity").GetString() != evidence.ModelIdentity ||
                root.GetProperty("evidence_hash").GetString() != evidence.EvidenceHash ||
                root.GetProperty("classifier_skill_id").GetString() != skill.Name ||
                root.GetProperty("classifier_skill_revision").GetString() != skill.Version ||
                root.GetProperty("classifier_skill_content_hash").GetString() != skill.ContentHash ||
                root.GetProperty("provider_model_identity").GetString() != providerModelIdentity)
            {
                return false;
            }
            Ra2VoxelUnitClassProposalResult result = Ra2VoxelUnitClassProposal.Validate(evidence, new(
                ParseClass(root.GetProperty("proposed_class").GetString()!),
                ParseConfidence(root.GetProperty("confidence_band").GetString()!),
                Array.AsReadOnly(ReadStringArray(root.GetProperty("evidence_fact_ids"), 32, 96)),
                root.GetProperty("reason").GetString()!,
                skill.Name,
                skill.Version,
                skill.ContentHash,
                evidence.EvidenceHash));
            if (!result.IsSuccess || root.GetProperty("proposal_hash").GetString() != result.Proposal!.ProposalHash)
                return false;
            proposal = result.Proposal;
            return true;
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException or InvalidDataException or FormatException)
        {
            return false;
        }
    }

    internal static string ComputeCacheKey(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2AgentSkillDescriptor skill,
        string providerModelIdentity)
    {
        string canonical = string.Join("\n",
            "ra2-voxel-unit-class-cache/1",
            evidence.ModelIdentity,
            evidence.EvidenceHash,
            skill.Name,
            skill.Version,
            skill.ContentHash,
            providerModelIdentity,
            SchemaRevision);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static bool HasExactProperties(JsonElement element, HashSet<string> expected)
    {
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
            if (!actual.Add(property.Name)) return false;
        return actual.SetEquals(expected);
    }

    private static string ReadString(JsonElement root, string propertyName, int maximum)
    {
        JsonElement value = root.GetProperty(propertyName);
        if (value.ValueKind != JsonValueKind.String)
            throw new InvalidDataException();
        string result = value.GetString()?.Trim() ?? string.Empty;
        if (!IsBounded(result, maximum))
            throw new InvalidDataException();
        return result;
    }

    private static string[] ReadStringArray(JsonElement element, int maximumCount, int maximumLength)
    {
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() < 1 || element.GetArrayLength() > maximumCount)
            throw new InvalidDataException();
        string[] values = element.EnumerateArray().Select(value =>
        {
            if (value.ValueKind != JsonValueKind.String) throw new InvalidDataException();
            string text = value.GetString()?.Trim() ?? string.Empty;
            return IsBounded(text, maximumLength) ? text : throw new InvalidDataException();
        }).ToArray();
        if (values.Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new InvalidDataException();
        return values;
    }

    private static Ra2VoxelUnitClass ParseClass(string value) => value switch
    {
        "ground" => Ra2VoxelUnitClass.Ground,
        "air" => Ra2VoxelUnitClass.Air,
        "large_surface" => Ra2VoxelUnitClass.LargeSurface,
        "unknown" => Ra2VoxelUnitClass.Unknown,
        _ => throw new InvalidDataException()
    };

    private static Ra2VoxelUnitClassConfidenceBand ParseConfidence(string value) => value switch
    {
        "high" => Ra2VoxelUnitClassConfidenceBand.High,
        "medium" => Ra2VoxelUnitClassConfidenceBand.Medium,
        "low" => Ra2VoxelUnitClassConfidenceBand.Low,
        _ => throw new InvalidDataException()
    };

    private static string FormatClass(Ra2VoxelUnitClass value) => value switch
    {
        Ra2VoxelUnitClass.Ground => "ground",
        Ra2VoxelUnitClass.Air => "air",
        Ra2VoxelUnitClass.LargeSurface => "large_surface",
        Ra2VoxelUnitClass.Unknown => "unknown",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static string FormatConfidence(Ra2VoxelUnitClassConfidenceBand value) => value switch
    {
        Ra2VoxelUnitClassConfidenceBand.High => "high",
        Ra2VoxelUnitClassConfidenceBand.Medium => "medium",
        Ra2VoxelUnitClassConfidenceBand.Low => "low",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static bool IsBounded(string value, int maximum) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && value.IndexOfAny(['\r', '\n', '\0']) < 0;

    private static bool IsIdentifier(string value) =>
        IsBounded(value, 96) && char.IsAsciiLetterOrDigit(value[0]) &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_');

    private static Ra2VoxelUnitClassProposalResult Invalid(string message) =>
        new(Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassProposalFailureKind.InvalidReason,
            message,
            null);

    private static Ra2VoxelUnitClassAssessmentResult Failure(
        Ra2VoxelUnitClassAssessmentFailureKind kind,
        string message,
        Ra2AiRequest? request = null,
        int providerCallCount = 0) =>
        new(kind, message, null, false, providerCallCount, request);
}
