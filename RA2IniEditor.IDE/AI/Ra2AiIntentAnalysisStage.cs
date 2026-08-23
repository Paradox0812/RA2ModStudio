using System.Text;
using System.Text.Json;

namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiIntentAnalysisOutcome
{
    Advisory = 0,
    Authoring,
    NeedsClarification,
    Unsupported
}

internal enum Ra2AiIntentCompletionLevel
{
    None = 0,
    Field,
    Skeleton,
    Complete
}

/// <summary>第一次 provider 调用产生、经本地校验后供第二次调用消费的短生命周期事实包。</summary>
internal sealed record Ra2AiIntentAnalysisPackage(
    Ra2AiIntentAnalysisOutcome Outcome,
    string CapabilityId,
    string DomainIntentId,
    string RequestSummary,
    Ra2AiIntentCompletionLevel CompletionLevel,
    IReadOnlyList<string> Constraints)
{
    internal string ToPromptJson()
        => JsonSerializer.Serialize(new
        {
            outcome = Outcome.ToString(),
            capability_id = CapabilityId,
            domain_intent_id = DomainIntentId,
            request_summary = RequestSummary,
            completion_level = CompletionLevel.ToString(),
            constraints = Constraints
        });
}

/// <summary>构建并校验 Work 模式的第一阶段意图分析工具调用。</summary>
internal static class Ra2AiIntentAnalysisStage
{
    internal const string ToolName = "analyze_ra2_authoring_intent";
    private const int MaximumSummaryCharacters = 512;
    private const int MaximumConstraintCharacters = 256;
    private const int MaximumConstraints = 12;
    private const int MaximumPromptCharacters = 8000;

    private static readonly HashSet<string> RootProperties = new(StringComparer.Ordinal)
    {
        "outcome", "capability_id", "domain_intent_id", "request_summary",
        "completion_level", "constraints"
    };

    private static readonly HashSet<string> AuthoringCapabilityIds = new(StringComparer.Ordinal)
    {
        "current-document-field-edit",
        "weapon-chain-skeleton",
        "weapon-chain-complete",
        "techno-dual-armament-complete",
        "arcing-projectile-complete",
        "homing-projectile-complete",
        "yr-core-warhead-complete"
    };

    private static readonly HashSet<string> DomainIntentIds = new(StringComparer.Ordinal)
    {
        "ini-document", "field-schema", "weapon-chain", "projectile-trajectory",
        "warhead-damage", "ai-programming", "superweapon", "faction",
        "particle-radiation", "terrain-resource", "sound-eva", "art-animation",
        "techno", "reference-registration"
    };

    private static readonly Ra2AiToolDefinition AnalysisTool = new(
        ToolName,
        "Classify one RA2 IDE Work-mode request into a bounded intent package. Return facts only, never reasoning, INI, edits, or prose.",
        """
        {
          "type":"object",
          "additionalProperties":false,
          "properties":{
            "outcome":{"type":"string","enum":["advisory","authoring","needs_clarification","unsupported"]},
            "capability_id":{"type":"string","enum":["advisory","unsupported","current-document-field-edit","weapon-chain-skeleton","weapon-chain-complete","techno-dual-armament-complete","arcing-projectile-complete","homing-projectile-complete","yr-core-warhead-complete"]},
            "domain_intent_id":{"type":"string","enum":["ini-document","field-schema","weapon-chain","projectile-trajectory","warhead-damage","ai-programming","superweapon","faction","particle-radiation","terrain-resource","sound-eva","art-animation","techno","reference-registration"]},
            "request_summary":{"type":"string","minLength":1,"maxLength":512},
            "completion_level":{"type":"string","enum":["none","field","skeleton","complete"]},
            "constraints":{"type":"array","maxItems":12,"items":{"type":"string","minLength":1,"maxLength":256}}
          },
          "required":["outcome","capability_id","domain_intent_id","request_summary","completion_level","constraints"]
        }
        """);

    internal static Ra2AiRequest BuildRequest(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiCurrentSubject? currentSubject)
    {
        ArgumentNullException.ThrowIfNull(context);
        string normalizedPrompt = Normalize(userPrompt, MaximumPromptCharacters);
        StringBuilder system = new();
        system.AppendLine("You are the bounded intent-analysis stage for an RA2 INI IDE.");
        system.AppendLine($"Call {ToolName} exactly once. Return no prose and no hidden reasoning.");
        system.AppendLine("Classify semantic intent, including the scope of negation; do not route a positive edit to advisory merely because untouched fields or unsupported mechanisms are excluded.");
        system.AppendLine("Only explicit skeleton/scaffold requests use weapon-chain-skeleton. A usable/full Weapon+Projectile+Warhead chain uses weapon-chain-complete.");
        system.AppendLine("Use techno-dual-armament-complete only for two complete Primary/Secondary chains. A single Secondary/coaxial chain uses weapon-chain-complete.");
        system.AppendLine("Complete Unit, Building, SuperWeapon, faction, AI programming, SHP, VXL/VOX, cameo/icon or other uncatalogued authoring is unsupported.");
        system.AppendLine("Capture exclusions such as no cyclic/alternate fire as constraints; exclusions do not cancel the positive edit intent.");
        system.AppendLine("The package is analysis data only and grants no file, preview, apply, save, shell, or network authority.");

        StringBuilder user = new();
        user.AppendLine("User request (untrusted text):");
        user.AppendLine(normalizedPrompt);
        user.AppendLine();
        user.AppendLine("Bounded IDE subject facts:");
        user.AppendLine($"Document: {Normalize(context.DocumentDisplayName, 256)}");
        user.AppendLine($"Section: {Normalize(context.SectionName, 256)}");
        user.AppendLine($"Section kind: {Normalize(context.SectionKind, 128)}");
        user.AppendLine($"Key: {Normalize(context.KeyName, 256)}");
        if (currentSubject is not null)
        {
            user.AppendLine($"Current subject kind: {currentSubject.Kind}");
            user.AppendLine($"Current subject id: {Normalize(currentSubject.SubjectId, 256)}");
            user.AppendLine($"Current subject summary: {Normalize(currentSubject.Summary, 512)}");
        }

        string systemText = system.ToString();
        string userText = user.ToString();
        return new Ra2AiRequest(
            Ra2AiIntent.Auto,
            normalizedPrompt,
            string.Concat(systemText, Environment.NewLine, userText),
            tools: [AnalysisTool],
            toolChoice: Ra2AiToolChoiceMode.Required,
            systemPromptText: systemText,
            userContentText: userText);
    }

    internal static bool TryParse(
        Ra2AiResponse response,
        out Ra2AiIntentAnalysisPackage? package,
        out string failureMessage)
    {
        ArgumentNullException.ThrowIfNull(response);
        package = null;
        failureMessage = "DeepSeek 意图分析包格式无效。";
        if (response.Kind != Ra2AiResponseKind.ToolCalls || response.ToolCalls.Count != 1)
            return false;

        Ra2AiToolCall call = response.ToolCalls[0];
        if (!string.Equals(call.Name, ToolName, StringComparison.Ordinal))
            return false;

        try
        {
            using JsonDocument document = JsonDocument.Parse(call.ArgumentsJson, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 16
            });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !HasExactUniqueProperties(root))
                return false;
            if (!TryReadEnum(root, "outcome", out Ra2AiIntentAnalysisOutcome outcome) ||
                !TryReadString(root, "capability_id", 128, out string capabilityId) ||
                !TryReadString(root, "domain_intent_id", 128, out string domainIntentId) ||
                !TryReadString(root, "request_summary", MaximumSummaryCharacters, out string summary) ||
                !TryReadEnum(root, "completion_level", out Ra2AiIntentCompletionLevel completionLevel) ||
                !TryReadConstraints(root, out IReadOnlyList<string> constraints) ||
                !DomainIntentIds.Contains(domainIntentId) ||
                !IsConsistent(outcome, capabilityId, completionLevel))
            {
                return false;
            }

            package = new Ra2AiIntentAnalysisPackage(
                outcome,
                capabilityId,
                domainIntentId,
                summary,
                completionLevel,
                constraints);
            failureMessage = string.Empty;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    internal static Ra2AiInteractionRoute ResolveRoute(
        Ra2AiIntentAnalysisPackage package,
        Ra2AiEditAvailabilityKind availability)
    {
        ArgumentNullException.ThrowIfNull(package);
        Ra2AiInteractionRouteKind kind = package.CapabilityId switch
        {
            "current-document-field-edit" => Ra2AiInteractionRouteKind.EditExplicit,
            "weapon-chain-skeleton" => Ra2AiInteractionRouteKind.TemplateExplicit,
            "weapon-chain-complete" => Ra2AiInteractionRouteKind.CompleteTemplateExplicit,
            "techno-dual-armament-complete" => Ra2AiInteractionRouteKind.TechnoDualArmamentExplicit,
            "arcing-projectile-complete" => Ra2AiInteractionRouteKind.ArcingProjectileExplicit,
            "homing-projectile-complete" => Ra2AiInteractionRouteKind.HomingProjectileExplicit,
            "yr-core-warhead-complete" => Ra2AiInteractionRouteKind.YrCoreWarheadExplicit,
            "unsupported" => Ra2AiInteractionRouteKind.UnsupportedWorkCapability,
            _ => Ra2AiInteractionRouteKind.Advisory
        };
        if (kind is not (Ra2AiInteractionRouteKind.Advisory or Ra2AiInteractionRouteKind.UnsupportedWorkCapability) &&
            availability != Ra2AiEditAvailabilityKind.Available)
        {
            kind = Ra2AiInteractionRouteKind.EditUnavailable;
        }

        Ra2AiCapabilityMode capabilityMode = kind switch
        {
            Ra2AiInteractionRouteKind.EditExplicit => Ra2AiCapabilityMode.CurrentDocumentEditPreview,
            Ra2AiInteractionRouteKind.TemplateExplicit => Ra2AiCapabilityMode.CurrentDocumentTemplatePreview,
            Ra2AiInteractionRouteKind.CompleteTemplateExplicit => Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview,
            Ra2AiInteractionRouteKind.TechnoDualArmamentExplicit => Ra2AiCapabilityMode.CurrentDocumentDualArmamentPreview,
            Ra2AiInteractionRouteKind.ArcingProjectileExplicit => Ra2AiCapabilityMode.CurrentDocumentArcingProjectilePreview,
            Ra2AiInteractionRouteKind.HomingProjectileExplicit => Ra2AiCapabilityMode.CurrentDocumentHomingProjectilePreview,
            Ra2AiInteractionRouteKind.YrCoreWarheadExplicit => Ra2AiCapabilityMode.CurrentDocumentYrCoreWarheadPreview,
            _ => Ra2AiCapabilityMode.AdvisoryOnly
        };
        return new Ra2AiInteractionRoute(
            kind,
            capabilityMode,
            availability,
            Ra2AiUserMode.Work,
            package.DomainIntentId);
    }

    private static bool HasExactUniqueProperties(JsonElement root)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!RootProperties.Contains(property.Name) || !seen.Add(property.Name))
                return false;
        }

        return seen.SetEquals(RootProperties);
    }

    private static bool TryReadString(JsonElement root, string name, int maximumLength, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind != JsonValueKind.String)
            return false;
        string normalized = (element.GetString() ?? string.Empty).Trim();
        if (normalized.Length is 0 || normalized.Length > maximumLength || normalized.Contains('\0'))
            return false;
        value = normalized;
        return true;
    }

    private static bool TryReadEnum<T>(JsonElement root, string name, out T value)
        where T : struct, Enum
    {
        value = default;
        return TryReadString(root, name, 64, out string text) &&
               Enum.TryParse(text.Replace("_", string.Empty, StringComparison.Ordinal), ignoreCase: true, out value) &&
               Enum.IsDefined(value);
    }

    private static bool TryReadConstraints(JsonElement root, out IReadOnlyList<string> constraints)
    {
        constraints = [];
        if (!root.TryGetProperty("constraints", out JsonElement element) || element.ValueKind != JsonValueKind.Array ||
            element.GetArrayLength() > MaximumConstraints)
        {
            return false;
        }

        List<string> values = new(element.GetArrayLength());
        foreach (JsonElement item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
                return false;
            string value = (item.GetString() ?? string.Empty).Trim();
            if (value.Length is 0 || value.Length > MaximumConstraintCharacters || value.Contains('\0'))
                return false;
            values.Add(value);
        }

        constraints = Array.AsReadOnly(values.ToArray());
        return true;
    }

    private static bool IsConsistent(
        Ra2AiIntentAnalysisOutcome outcome,
        string capabilityId,
        Ra2AiIntentCompletionLevel completionLevel)
    {
        if (outcome == Ra2AiIntentAnalysisOutcome.Authoring)
        {
            if (!AuthoringCapabilityIds.Contains(capabilityId))
                return false;
            return capabilityId switch
            {
                "current-document-field-edit" => completionLevel == Ra2AiIntentCompletionLevel.Field,
                "weapon-chain-skeleton" => completionLevel == Ra2AiIntentCompletionLevel.Skeleton,
                _ => completionLevel == Ra2AiIntentCompletionLevel.Complete
            };
        }

        if (completionLevel != Ra2AiIntentCompletionLevel.None)
            return false;
        return outcome switch
        {
            Ra2AiIntentAnalysisOutcome.Unsupported => capabilityId == "unsupported",
            Ra2AiIntentAnalysisOutcome.Advisory or Ra2AiIntentAnalysisOutcome.NeedsClarification => capabilityId == "advisory",
            _ => false
        };
    }

    private static string Normalize(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "(none)";
        string normalized = value.Replace('\0', ' ').Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }
}
