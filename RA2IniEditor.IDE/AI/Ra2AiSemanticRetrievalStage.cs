using System.Text;
using System.Text.Json;

namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiSemanticRetrievalOutcome
{
    Ready = 0,
    Query,
    NeedsClarification
}

internal enum Ra2AiSemanticRetrievalStopReason
{
    EvidenceReady = 0,
    NoRefinementRequired,
    NeedsClarification,
    NoProgress,
    RoundLimit,
    ProviderFailure
}

internal sealed record Ra2AiResolvedEntityBinding(
    string EntityRole,
    string Target,
    string CanonicalSection,
    string Kind,
    string MatchedAlias,
    string MatchBasis,
    int Score);

internal sealed record Ra2AiSemanticRetrievalPackage(
    Ra2AiSemanticRetrievalOutcome Outcome,
    string Message,
    IReadOnlyList<Ra2AiContextQueryRequest> ContextQueries);

internal sealed record Ra2AiSemanticRetrievalAttempt(
    int Round,
    IReadOnlyList<Ra2AiContextQueryRequest> Queries,
    IReadOnlyList<Ra2AiContextQueryResult> Results,
    int NewEvidenceCount,
    Ra2AiResponseKind ProviderResponseKind,
    int PromptCharacters);

internal sealed record Ra2AiSemanticRetrievalResult(
    IReadOnlyList<Ra2AiContextQueryResult> QueryResults,
    IReadOnlyList<Ra2AiResolvedEntityBinding> EntityBindings,
    IReadOnlyList<Ra2AiSemanticRetrievalAttempt> Attempts,
    Ra2AiSemanticRetrievalStopReason StopReason,
    string Message)
{
    public static Ra2AiSemanticRetrievalResult WithoutRefinement(
        IReadOnlyList<Ra2AiContextQueryResult> results)
        => new(
            Array.AsReadOnly(results.ToArray()),
            Ra2AiSemanticRetrievalStage.CreateBindings(results),
            [],
            Ra2AiSemanticRetrievalStopReason.NoRefinementRequired,
            "Initial Host evidence was sufficient for execution.");
}

/// <summary>
/// Compact provider protocol used only to request another bounded read-only Host query round.
/// It cannot emit edits and does not receive full Skill bodies or editor text.
/// </summary>
internal static class Ra2AiSemanticRetrievalStage
{
    internal const string ToolName = "refine_ra2_context_queries";
    internal const int MaximumRefinementRounds = 2;
    private const int MaximumMessageCharacters = 512;
    private const int MaximumPromptCharacters = 4000;

    internal static bool ShouldRefine(
        Ra2AiIntentAnalysisPackage package,
        IReadOnlyList<Ra2AiContextQueryResult> results)
    {
        if (package.Outcome != Ra2AiIntentAnalysisOutcome.Authoring)
            return false;
        IReadOnlyList<Ra2AiResolvedEntityBinding> bindings = CreateBindings(results);
        if (results.Any(result =>
                !result.Succeeded &&
                (string.IsNullOrWhiteSpace(result.Request.EntityRole) ||
                 bindings.All(binding => !string.Equals(
                     binding.EntityRole,
                     result.Request.EntityRole,
                     StringComparison.Ordinal)))))
            return true;
        if (results.Any(result => result.Request.Kind == Ra2AiContextQueryKind.SearchObjects &&
                                  !HasUniqueHighConfidenceMatch(result)))
            return true;

        return IsProjectKnowledgeCapability(package.CapabilityId) &&
               !results.Any(result => result.Succeeded);
    }

    internal static Ra2AiRequest BuildRequest(
        int round,
        string userPrompt,
        Ra2AiIntentAnalysisPackage package,
        Ra2AgentSkillSelectionResolution skillSelection,
        Ra2AiProjectContextSnapshot projectContext,
        IReadOnlyList<Ra2AiContextQueryResult> accumulatedResults)
    {
        if (round is < 1 or > MaximumRefinementRounds)
            throw new ArgumentOutOfRangeException(nameof(round));
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(skillSelection);
        ArgumentNullException.ThrowIfNull(projectContext);
        ArgumentNullException.ThrowIfNull(accumulatedResults);

        StringBuilder system = new();
        system.AppendLine("You are the bounded semantic-retrieval planner for an RA2 INI IDE Work request.");
        system.AppendLine($"Call {ToolName} exactly once. Return no prose, edits, INI, or hidden reasoning.");
        system.AppendLine("Use ready when captured facts are sufficient. Use query only for new read-only facts. Use needs_clarification only when the missing identity cannot be searched from captured current/rules/art snapshots.");
        system.AppendLine("Never repeat a query already shown. Prefer search_objects for local display names, get_section for a certain canonical ID, and resolve_reference for a certain source key.");
        system.AppendLine("A search result is safe to bind only when one top result has an exact ID/Name/UIName score. Ambiguous results require a narrower query, not a guess.");
        system.AppendLine("The Host enforces query, evidence and round limits. No edit, apply, save, path, shell or network authority is available.");

        StringBuilder user = new();
        user.AppendLine($"Refinement round: {round}/{MaximumRefinementRounds}");
        user.AppendLine("Original request (bounded, untrusted text):");
        user.AppendLine(Bound(Ra2AiOutboundTextSanitizer.Sanitize(userPrompt).Text, MaximumPromptCharacters));
        user.AppendLine();
        user.AppendLine("Validated intent package:");
        user.AppendLine(package.ToPromptJson());
        user.AppendLine();
        Ra2AiSharedContextPromptFormatter.AppendProjectContext(user, projectContext);
        user.AppendLine("Active Skill summaries (metadata only):");
        foreach (Ra2AgentSkillDescriptor skill in skillSelection.ActiveSkills)
            user.AppendLine($"- id={skill.Name}; domains={string.Join(',', skill.Domains)}; description={Bound(skill.Description, 512)}");
        user.AppendLine();
        Ra2AiSharedContextPromptFormatter.AppendQueryResults(user, accumulatedResults);

        string systemText = system.ToString();
        string userText = user.ToString();
        return new Ra2AiRequest(
            Ra2AiIntent.Auto,
            Bound(userPrompt, MaximumPromptCharacters),
            string.Concat(systemText, Environment.NewLine, userText),
            tools: [BuildTool()],
            toolChoice: Ra2AiToolChoiceMode.Required,
            systemPromptText: systemText,
            userContentText: userText);
    }

    internal static bool TryParse(
        Ra2AiResponse response,
        out Ra2AiSemanticRetrievalPackage? package)
        => TryParse(response, out package, out _);

    internal static bool TryParse(
        Ra2AiResponse response,
        out Ra2AiSemanticRetrievalPackage? package,
        out string failureMessage)
    {
        package = null;
        failureMessage = "语义检索响应无效。";
        if (response.Kind != Ra2AiResponseKind.ToolCalls)
        {
            failureMessage = "语义检索响应不是工具调用。";
            return false;
        }
        if (response.ToolCalls.Count != 1)
        {
            failureMessage = "语义检索必须且只能返回一个工具调用。";
            return false;
        }
        Ra2AiToolCall call = response.ToolCalls[0];
        if (!string.Equals(call.Name, ToolName, StringComparison.Ordinal))
        {
            failureMessage = "语义检索返回了错误的工具。";
            return false;
        }
        if (call.ArgumentsJson.Length > 64 * 1024)
        {
            failureMessage = "语义检索工具参数超过本地资源上限。";
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(call.ArgumentsJson, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 16
            });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                failureMessage = "语义检索工具参数根节点不是 JSON 对象。";
                return false;
            }
            HashSet<string> properties = new(StringComparer.Ordinal);
            foreach (JsonProperty property in root.EnumerateObject())
            {
                if (!properties.Add(property.Name))
                {
                    string name = property.Name.Length <= 48 ? property.Name : property.Name[..48];
                    failureMessage = $"语义检索工具参数包含重复字段：{name}.";
                    return false;
                }
            }

            Ra2AiIntentAnalysisStage.TryReadContextQueries(
                root,
                out IReadOnlyList<Ra2AiContextQueryRequest> queries);
            string outcomeText = root.TryGetProperty("outcome", out JsonElement outcomeElement) &&
                                 outcomeElement.ValueKind == JsonValueKind.String
                ? outcomeElement.GetString() ?? string.Empty
                : string.Empty;
            string normalizedOutcome = new(outcomeText
                .Where(char.IsLetterOrDigit)
                .ToArray());
            if (!Enum.TryParse(normalizedOutcome, true, out Ra2AiSemanticRetrievalOutcome outcome) ||
                !Enum.IsDefined(outcome))
            {
                outcome = queries.Count > 0
                    ? Ra2AiSemanticRetrievalOutcome.Query
                    : Ra2AiSemanticRetrievalOutcome.Ready;
            }

            string message = root.TryGetProperty("message", out JsonElement messageElement) &&
                             messageElement.ValueKind == JsonValueKind.String
                ? (messageElement.GetString() ?? string.Empty).Replace('\0', ' ').Trim()
                : string.Empty;
            if (message.Length > MaximumMessageCharacters)
                message = message[..MaximumMessageCharacters];

            // Queries are executable facts and therefore take precedence over a descriptive
            // outcome label. A query label with no admitted query degrades to Ready so one
            // malformed optional item cannot turn a valid Work intent into a protocol error.
            outcome = queries.Count > 0
                ? Ra2AiSemanticRetrievalOutcome.Query
                : outcome == Ra2AiSemanticRetrievalOutcome.Query
                    ? Ra2AiSemanticRetrievalOutcome.Ready
                    : outcome;

            package = new(outcome, message, queries);
            failureMessage = string.Empty;
            return true;
        }
        catch (JsonException exception)
        {
            failureMessage = $"语义检索工具参数不是有效 JSON（位置 {exception.BytePositionInLine}）。";
            return false;
        }
    }

    internal static IReadOnlyList<Ra2AiResolvedEntityBinding> CreateBindings(
        IReadOnlyList<Ra2AiContextQueryResult> results)
    {
        Dictionary<string, Ra2AiResolvedEntityBinding> bindings = new(StringComparer.Ordinal);
        foreach (Ra2AiContextQueryResult result in results)
        {
            if (result.Request.Kind == Ra2AiContextQueryKind.GetSection &&
                result.Succeeded &&
                result.Section is { } exactSection &&
                !string.IsNullOrWhiteSpace(result.Request.EntityRole))
            {
                string exactIdentity = string.Concat(result.Request.Target, "\u001f", result.Request.EntityRole);
                bindings[exactIdentity] = new(
                    result.Request.EntityRole,
                    result.Request.Target,
                    exactSection.Name,
                    exactSection.Kind,
                    exactSection.Name,
                    "ExactSectionQuery",
                    1000);
                continue;
            }

            if (result.Request.Kind != Ra2AiContextQueryKind.SearchObjects ||
                string.IsNullOrWhiteSpace(result.Request.EntityRole) ||
                !HasUniqueHighConfidenceMatch(result))
            {
                continue;
            }

            Ra2AiContextObjectFact match = result.Objects[0];
            string identity = string.Concat(result.Request.Target, "\u001f", result.Request.EntityRole);
            bindings[identity] = new(
                result.Request.EntityRole,
                result.Request.Target,
                match.CanonicalSection,
                match.Kind,
                match.MatchedAlias,
                match.MatchBasis,
                match.Score);
        }
        return Array.AsReadOnly(bindings.Values
            .OrderBy(binding => binding.EntityRole, StringComparer.Ordinal)
            .ThenBy(binding => binding.Target, StringComparer.Ordinal)
            .ToArray());
    }

    internal static string Fingerprint(Ra2AiContextQueryRequest request)
        => string.Join(
            "|",
            request.Kind,
            request.Target,
            request.Section.ToUpperInvariant(),
            request.Key.ToUpperInvariant(),
            request.SectionOccurrence?.ToString() ?? "-1",
            request.FieldOccurrence?.ToString() ?? "-1",
            request.ReferenceIndex,
            request.SearchText.ToUpperInvariant(),
            request.EntityRole.ToUpperInvariant(),
            string.Join(',', request.AcceptedKinds.OrderBy(value => value, StringComparer.OrdinalIgnoreCase)),
            request.MaximumResults);

    private static bool HasUniqueHighConfidenceMatch(Ra2AiContextQueryResult result)
        => result.Succeeded && result.Objects.Count > 0 && result.Objects[0].Score >= 900 &&
           (result.Objects.Count == 1 || result.Objects[1].Score < result.Objects[0].Score);

    private static bool IsProjectKnowledgeCapability(string capabilityId)
        => capabilityId is
            "techno-rules-art-binding" or
            "project-rules-art-edit" or
            "ares-unitdelivery-superweapon-complete" or
            "ares-genericwarhead-superweapon-complete" or
            "superweapon-project-edit";

    private static Ra2AiToolDefinition BuildTool()
    {
        string schema = $$"""
        {
          "type":"object",
          "additionalProperties":false,
          "properties":{
            "outcome":{"type":"string","enum":["ready","query","needs_clarification"]},
            "message":{"type":"string","maxLength":512},
            "context_queries":{{Ra2AiIntentAnalysisStage.ContextQueryArraySchema}}
          },
          "required":["outcome","message","context_queries"]
        }
        """;
        return new(
            ToolName,
            "Request one new bounded read-only RA2 context-query round, or report that existing facts are ready/need clarification.",
            schema);
    }

    private static string Bound(string? value, int maximumCharacters)
    {
        string normalized = (value ?? string.Empty).Replace('\0', ' ').Trim();
        return normalized.Length <= maximumCharacters ? normalized : normalized[..maximumCharacters];
    }
}
