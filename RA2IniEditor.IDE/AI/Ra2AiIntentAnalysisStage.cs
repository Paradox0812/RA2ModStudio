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

internal enum Ra2AiIntentAnalysisFailureKind
{
    None = 0,
    ResponseNotToolCall,
    ToolCallCountInvalid,
    ToolNameInvalid,
    ArgumentsTooLarge,
    InvalidJson,
    RootNotObject,
    DuplicateRootProperty
}

/// <summary>一次 Work 意图解析的瞬态、非序列化结果；只保存本地安全诊断。</summary>
internal sealed record Ra2AiIntentAnalysisParseResult(
    Ra2AiIntentAnalysisPackage? Package,
    Ra2AiIntentAnalysisFailureKind FailureKind,
    string DiagnosticMessage,
    IReadOnlyList<string> RecoveryNotes)
{
    public bool Succeeded => Package is not null && FailureKind == Ra2AiIntentAnalysisFailureKind.None;
}

/// <summary>第一次 provider 调用产生、经本地校验后供第二次调用消费的短生命周期事实包。</summary>
internal sealed record Ra2AiIntentAnalysisPackage(
    Ra2AiIntentAnalysisOutcome Outcome,
    string CapabilityId,
    string DomainIntentId,
    string RequestSummary,
    Ra2AiIntentCompletionLevel CompletionLevel,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> KnowledgeGaps)
{
    public IReadOnlyList<Ra2AiContextQueryRequest> ContextQueries { get; init; } = [];

    internal string ToPromptJson()
        => JsonSerializer.Serialize(new
        {
            outcome = Outcome.ToString(),
            capability_id = CapabilityId,
            domain_intent_id = DomainIntentId,
            request_summary = RequestSummary,
            completion_level = CompletionLevel.ToString(),
            constraints = Constraints,
            selected_skill_ids = SelectedSkillIds,
            knowledge_gaps = KnowledgeGaps,
            context_queries = ContextQueries.Select(query => new
            {
                kind = FormatQueryKind(query.Kind),
                target = query.Target,
                section = query.Section,
                key = query.Key,
                section_occurrence = query.SectionOccurrence ?? -1,
                field_occurrence = query.FieldOccurrence ?? -1,
                reference_index = query.ReferenceIndex,
                search_text = query.SearchText,
                entity_role = query.EntityRole,
                accepted_kinds = query.AcceptedKinds,
                maximum_results = query.MaximumResults
            })
        });

    private static string FormatQueryKind(Ra2AiContextQueryKind kind)
        => kind switch
        {
            Ra2AiContextQueryKind.GetSection => "get_section",
            Ra2AiContextQueryKind.ResolveReference => "resolve_reference",
            Ra2AiContextQueryKind.SearchObjects => "search_objects",
            _ => "unsupported"
        };
}

/// <summary>构建并校验 Work 模式的第一阶段意图分析工具调用。</summary>
internal static class Ra2AiIntentAnalysisStage
{
    internal const string ToolName = "analyze_ra2_authoring_intent";
    internal const string ContextQueryArraySchema =
        "{\"type\":\"array\",\"maxItems\":8,\"items\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"kind\":{\"type\":\"string\",\"enum\":[\"get_section\",\"resolve_reference\",\"search_objects\"]},\"target\":{\"type\":\"string\",\"enum\":[\"current\",\"rules\",\"art\"]},\"section\":{\"type\":\"string\",\"maxLength\":256},\"key\":{\"type\":\"string\",\"maxLength\":256},\"section_occurrence\":{\"type\":\"integer\",\"minimum\":-1,\"maximum\":10000},\"field_occurrence\":{\"type\":\"integer\",\"minimum\":-1,\"maximum\":10000},\"reference_index\":{\"type\":\"integer\",\"minimum\":0,\"maximum\":1024},\"search_text\":{\"type\":\"string\",\"maxLength\":256},\"entity_role\":{\"type\":\"string\",\"maxLength\":64},\"accepted_kinds\":{\"type\":\"array\",\"maxItems\":8,\"items\":{\"type\":\"string\",\"minLength\":1,\"maxLength\":64}},\"maximum_results\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":8}},\"required\":[\"kind\",\"target\"]}}";
    private const string ProjectRulesArtBindingCapabilityId = "techno-rules-art-binding";
    private const string GenericProjectRulesArtEditCapabilityId = "project-rules-art-edit";
    private const string ProjectRulesArtBindingDomainIntentId = "art-animation";
    private const string UnitDeliverySuperWeaponCapabilityId = "ares-unitdelivery-superweapon-complete";
    private const string GenericWarheadSuperWeaponCapabilityId = "ares-genericwarhead-superweapon-complete";
    private const string SuperWeaponProjectEditCapabilityId = "superweapon-project-edit";
    private const int MaximumSummaryCharacters = 512;
    private const int MaximumConstraintCharacters = 256;
    private const int MaximumConstraints = 12;
    private const int MaximumSelectedSkills = 6;
    private const int MaximumKnowledgeGaps = 6;
    private const int MaximumSkillFactCharacters = 256;
    private const int MaximumPromptCharacters = 8000;
    private const int MaximumArgumentsCharacters = 64 * 1024;

    private static readonly HashSet<string> RootProperties = new(StringComparer.Ordinal)
    {
        "outcome", "capability_id", "domain_intent_id", "request_summary",
        "completion_level", "constraints", "selected_skill_ids", "knowledge_gaps",
        "context_queries"
    };

    private static readonly HashSet<string> ContextQueryProperties = new(StringComparer.Ordinal)
    {
        "kind", "target", "section", "key", "section_occurrence", "field_occurrence", "reference_index",
        "search_text", "entity_role", "accepted_kinds", "maximum_results"
    };

    private static readonly HashSet<string> AuthoringCapabilityIds = new(StringComparer.Ordinal)
    {
        "current-document-field-edit",
        "weapon-chain-skeleton",
        "weapon-chain-complete",
        "techno-dual-armament-complete",
        "arcing-projectile-complete",
        "homing-projectile-complete",
        "yr-core-warhead-complete",
        "techno-rules-art-binding",
        GenericProjectRulesArtEditCapabilityId,
        UnitDeliverySuperWeaponCapabilityId,
        GenericWarheadSuperWeaponCapabilityId,
        SuperWeaponProjectEditCapabilityId
    };

    internal static Ra2AiRequest BuildRequest(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AgentSkillCatalog skillCatalog)
        => BuildRequest(
            userPrompt,
            context,
            conversationContext: null,
            currentSubject,
            projectContext: null,
            skillCatalog);

    internal static Ra2AiRequest BuildRequest(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiProjectContextSnapshot? projectContext,
        Ra2AgentSkillCatalog skillCatalog)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(skillCatalog);
        Ra2AiRequestPreparationFlags sharedContextFlags = Ra2AiRequestPreparationFlags.None;
        Ra2AiConversationContext? boundedConversation = Ra2AiPromptBuilder.PrepareConversationContext(
            conversationContext,
            ref sharedContextFlags);
        IReadOnlyList<Ra2AgentSkillManifestEntry> manifest = skillCatalog.CreateManifest();
        string normalizedPrompt = SanitizeAndNormalize(userPrompt, MaximumPromptCharacters);
        StringBuilder system = new();
        system.AppendLine("You are the bounded intent-analysis stage for an RA2 INI IDE.");
        system.AppendLine($"Call {ToolName} exactly once. Return no prose and no hidden reasoning.");
        system.AppendLine("Classify semantic intent, including the scope of negation; do not route a positive edit to advisory merely because untouched fields or unsupported mechanisms are excluded.");
        system.AppendLine("Only explicit skeleton/scaffold requests use weapon-chain-skeleton. A usable/full Weapon+Projectile+Warhead chain uses weapon-chain-complete.");
        system.AppendLine("Use techno-dual-armament-complete only for two complete Primary/Secondary chains. A single Secondary/coaxial chain uses weapon-chain-complete.");
        system.AppendLine("Use project-rules-art-edit for other complete bounded rules/art INI construction that is not covered by a specialized capability. SHP, VXL/VOX, cameo/icon binary asset generation remains outside this INI Work entry; excluding asset generation does not make the INI edit advisory or unsupported.");
        system.AppendLine("Treat 超级武器, 超武, 支援技能, 支援能力, SuperWeapon, and support power as the superweapon domain. Use ares-unitdelivery-superweapon-complete when the requested behavior delivers existing units at a selected map location, even when the user describes that behavior naturally instead of naming Ares UnitDelivery. Use ares-genericwarhead-superweapon-complete for an explicit Ares GenericWarhead request that reuses an existing Warhead, and superweapon-project-edit for other explicit source-backed SuperWeapon project edits. If engine/type/provider/effect identity is materially missing, return needs_clarification rather than a skeleton.");
        system.AppendLine("Use techno-rules-art-binding when the user asks to bind or synchronize an existing Techno with an art section and supplies, or clearly names, the owner ID, art ID, body asset ID, and cameo/icon ID. Also use it for an explicit structured field edit to a captured rules or art file, and request the named target/Section through context_queries when its existing content matters. The user does not need to repeat rulesmd.ini/artmd.ini, preview-only, no-save, or no-asset-generation boilerplate; those restrictions are enforced locally. Prefer domain_intent_id=art-animation and completion_level=field; this remains a bounded project INI edit, not complete Techno or asset generation.");
        system.AppendLine("Capture exclusions such as no cyclic/alternate fire as constraints; exclusions do not cancel the positive edit intent.");
        system.AppendLine("Select up to 6 relevant Skill IDs from the supplied immutable manifest for the execution stage. Order the most important first. Never invent an ID.");
        system.AppendLine("Report up to 6 concrete knowledge gaps. An empty list means no additional gap was identified; it does not assert certainty.");
        system.AppendLine("Request up to 8 read-only context queries only when a captured current/rules/art Section or reference fact can materially improve execution. Targets are symbolic aliases, never paths.");
        system.AppendLine("Use get_section for an exact named Section, resolve_reference for an exact source Section/key, and search_objects when the user supplied a local object name/display name but its canonical Section ID is not certain. Use -1 for unspecified occurrences, an empty key for get_section/search_objects, and reference_index=0 unless a list item is explicitly needed.");
        system.AppendLine("Set entity_role on every query that resolves a user-named object so a later successful search can supersede an earlier failed candidate without another confirmation round.");
        system.AppendLine("For search_objects provide search_text, an entity_role, optional accepted Section kinds, and maximum_results 1..8. The Host searches only captured INI identity facts and never writes files.");
        system.AppendLine("For a typed SuperWeapon request that names an existing provider, delivery object, or Warhead by a natural-language/display name, prefer target=rules search_objects evidence instead of guessing a Section ID. Use exact get_section only when the canonical ID is already certain. Do not pass display names such as GI, IFV, or Allied Power Plant as if they were Section IDs.");
        system.AppendLine("The package is analysis data only and grants no file, preview, apply, save, shell, or network authority.");

        StringBuilder user = new();
        user.AppendLine("User request (untrusted text):");
        user.AppendLine(normalizedPrompt);
        user.AppendLine();
        user.AppendLine("Bounded IDE subject facts:");
        user.AppendLine($"Document: {SanitizeAndNormalize(context.DocumentDisplayName, 256)}");
        user.AppendLine($"Section: {SanitizeAndNormalize(context.SectionName, 256)}");
        user.AppendLine($"Section kind: {SanitizeAndNormalize(context.SectionKind, 128)}");
        user.AppendLine($"Key: {SanitizeAndNormalize(context.KeyName, 256)}");
        if (currentSubject is not null)
        {
            user.AppendLine($"Current subject kind: {currentSubject.Kind}");
            user.AppendLine($"Current subject id: {SanitizeAndNormalize(currentSubject.SubjectId, 256)}");
            user.AppendLine($"Current subject summary: {SanitizeAndNormalize(currentSubject.Summary, 512)}");
        }
        user.AppendLine();
        Ra2AiSharedContextPromptFormatter.AppendConversation(user, boundedConversation);
        Ra2AiSharedContextPromptFormatter.AppendProjectContext(user, projectContext);
        user.AppendLine();
        user.AppendLine("Available built-in RA2 Skill manifest (metadata only; no instruction bodies):");
        foreach (Ra2AgentSkillManifestEntry skill in manifest)
        {
            user.AppendLine(
                $"- id={skill.Id}; version={skill.Version}; modes={FormatModes(skill.Modes)}; " +
                $"domains={string.Join(',', skill.Domains)}; chars={skill.InstructionCharacters}; " +
                $"sha256={skill.ContentHash}; description={SanitizeAndNormalize(skill.Description, 1024)}");
        }

        string systemText = system.ToString();
        string userText = user.ToString();
        return new Ra2AiRequest(
            Ra2AiIntent.Auto,
            normalizedPrompt,
            string.Concat(systemText, Environment.NewLine, userText),
            tools: [BuildAnalysisTool(manifest)],
            toolChoice: Ra2AiToolChoiceMode.Required,
            systemPromptText: systemText,
            userContentText: userText);
    }

    internal static bool TryParse(
        Ra2AiResponse response,
        out Ra2AiIntentAnalysisPackage? package,
        out string failureMessage)
    {
        Ra2AiIntentAnalysisParseResult result = Parse(response);
        package = result.Package;
        failureMessage = result.DiagnosticMessage;
        return result.Succeeded;
    }

    internal static Ra2AiIntentAnalysisParseResult Parse(Ra2AiResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.Kind != Ra2AiResponseKind.ToolCalls)
            return Failure(Ra2AiIntentAnalysisFailureKind.ResponseNotToolCall, "第一轮响应不是工具调用。");
        if (response.ToolCalls.Count != 1)
            return Failure(Ra2AiIntentAnalysisFailureKind.ToolCallCountInvalid, "第一轮必须且只能返回一个意图工具调用。");

        Ra2AiToolCall call = response.ToolCalls[0];
        if (!string.Equals(call.Name, ToolName, StringComparison.Ordinal))
            return Failure(Ra2AiIntentAnalysisFailureKind.ToolNameInvalid, "第一轮返回了非 Work 意图分析工具。");
        if (call.ArgumentsJson.Length > MaximumArgumentsCharacters)
            return Failure(Ra2AiIntentAnalysisFailureKind.ArgumentsTooLarge, "第一轮工具参数超过本地资源上限。");

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
                return Failure(Ra2AiIntentAnalysisFailureKind.RootNotObject, "第一轮工具参数根节点不是 JSON 对象。");
            if (!HasUniqueProperties(root, out string duplicateProperty))
                return Failure(
                    Ra2AiIntentAnalysisFailureKind.DuplicateRootProperty,
                    $"第一轮工具参数包含重复字段：{BoundDiagnosticName(duplicateProperty)}。");

            List<string> recoveryNotes = [];
            foreach (JsonProperty property in root.EnumerateObject())
            {
                if (!RootProperties.Contains(property.Name))
                    recoveryNotes.Add($"忽略附加字段 {BoundDiagnosticName(property.Name)}");
            }

            string capabilityId = ReadBoundedString(
                root,
                "capability_id",
                128,
                "unclassified-authoring",
                recoveryNotes);
            Ra2AiIntentAnalysisOutcome outcome = ReadOutcome(root, capabilityId, recoveryNotes);
            string domainIntentId = ReadBoundedString(
                root,
                "domain_intent_id",
                128,
                "ini-document",
                recoveryNotes);
            string summary = ReadBoundedString(
                root,
                "request_summary",
                MaximumSummaryCharacters,
                "Work authoring request",
                recoveryNotes);
            Ra2AiIntentCompletionLevel completionLevel = ReadCompletionLevel(
                root,
                capabilityId,
                outcome,
                recoveryNotes);
            IReadOnlyList<string> constraints = ReadBoundedStringList(
                root,
                "constraints",
                MaximumConstraints,
                MaximumConstraintCharacters,
                recoveryNotes);
            IReadOnlyList<string> selectedSkillIds = ReadBoundedStringList(
                root,
                "selected_skill_ids",
                MaximumSelectedSkills,
                MaximumSkillFactCharacters,
                recoveryNotes);
            IReadOnlyList<string> knowledgeGaps = ReadBoundedStringList(
                root,
                "knowledge_gaps",
                MaximumKnowledgeGaps,
                MaximumSkillFactCharacters,
                recoveryNotes);
            IReadOnlyList<Ra2AiContextQueryRequest> contextQueries = ReadContextQueries(root, recoveryNotes);

            NormalizeCapabilityMetadata(
                outcome,
                contextQueries,
                ref capabilityId,
                ref domainIntentId,
                ref completionLevel);

            Ra2AiIntentAnalysisPackage package = new(
                outcome,
                capabilityId,
                domainIntentId,
                summary,
                completionLevel,
                constraints,
                selectedSkillIds,
                knowledgeGaps)
            {
                ContextQueries = contextQueries
            };
            return new(
                package,
                Ra2AiIntentAnalysisFailureKind.None,
                string.Empty,
                Array.AsReadOnly(recoveryNotes
                    .Distinct(StringComparer.Ordinal)
                    .Take(32)
                    .ToArray()));
        }
        catch (JsonException exception)
        {
            return Failure(
                Ra2AiIntentAnalysisFailureKind.InvalidJson,
                $"第一轮工具参数不是有效 JSON（位置 {exception.BytePositionInLine}）。");
        }
    }

    internal static Ra2AiInteractionRoute ResolveRoute(
        Ra2AiIntentAnalysisPackage package,
        Ra2AiEditAvailabilityKind availability)
        => ResolveRoute(package, new Ra2AiAuthoringAvailability(availability, Ra2AiProjectEditAvailabilityKind.NoProject));

    internal static Ra2AiInteractionRoute ResolveRoute(
        Ra2AiIntentAnalysisPackage package,
        Ra2AiAuthoringAvailability availability)
    {
        ArgumentNullException.ThrowIfNull(package);
        Ra2AiInteractionRouteKind kind = package.Outcome switch
        {
            Ra2AiIntentAnalysisOutcome.Unsupported => Ra2AiInteractionRouteKind.UnsupportedWorkCapability,
            Ra2AiIntentAnalysisOutcome.Advisory or Ra2AiIntentAnalysisOutcome.NeedsClarification
                => Ra2AiInteractionRouteKind.Advisory,
            _ => ResolveAuthoringRouteKind(package, availability)
        };
        bool projectCapability = kind is
            Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit or
            Ra2AiInteractionRouteKind.AresUnitDeliverySuperWeaponExplicit or
            Ra2AiInteractionRouteKind.AresGenericWarheadSuperWeaponExplicit or
            Ra2AiInteractionRouteKind.SuperWeaponProjectEditExplicit;
        if (!projectCapability &&
            kind is not (Ra2AiInteractionRouteKind.Advisory or Ra2AiInteractionRouteKind.UnsupportedWorkCapability) &&
            availability.Document != Ra2AiEditAvailabilityKind.Available)
        {
            kind = Ra2AiInteractionRouteKind.EditUnavailable;
        }
        else if (projectCapability && availability.RulesArtProject != Ra2AiProjectEditAvailabilityKind.Available)
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
            Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit => Ra2AiCapabilityMode.ProjectRulesArtBindingPreview,
            Ra2AiInteractionRouteKind.AresUnitDeliverySuperWeaponExplicit => Ra2AiCapabilityMode.ProjectAresUnitDeliverySuperWeaponPreview,
            Ra2AiInteractionRouteKind.AresGenericWarheadSuperWeaponExplicit => Ra2AiCapabilityMode.ProjectAresGenericWarheadSuperWeaponPreview,
            Ra2AiInteractionRouteKind.SuperWeaponProjectEditExplicit => Ra2AiCapabilityMode.ProjectSuperWeaponEditPreview,
            _ => Ra2AiCapabilityMode.AdvisoryOnly
        };
        return new Ra2AiInteractionRoute(
            kind,
            capabilityMode,
            availability.Document,
            Ra2AiUserMode.Work,
            package.DomainIntentId,
            availability.RulesArtProject);
    }

    private static Ra2AiInteractionRouteKind ResolveAuthoringRouteKind(
        Ra2AiIntentAnalysisPackage package,
        Ra2AiAuthoringAvailability availability)
        => package.CapabilityId switch
        {
            "current-document-field-edit" => Ra2AiInteractionRouteKind.EditExplicit,
            "weapon-chain-skeleton" => Ra2AiInteractionRouteKind.TemplateExplicit,
            "weapon-chain-complete" => Ra2AiInteractionRouteKind.CompleteTemplateExplicit,
            "techno-dual-armament-complete" => Ra2AiInteractionRouteKind.TechnoDualArmamentExplicit,
            "arcing-projectile-complete" => Ra2AiInteractionRouteKind.ArcingProjectileExplicit,
            "homing-projectile-complete" => Ra2AiInteractionRouteKind.HomingProjectileExplicit,
            "yr-core-warhead-complete" => Ra2AiInteractionRouteKind.YrCoreWarheadExplicit,
            "techno-rules-art-binding" => Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit,
            GenericProjectRulesArtEditCapabilityId => Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit,
            UnitDeliverySuperWeaponCapabilityId => Ra2AiInteractionRouteKind.AresUnitDeliverySuperWeaponExplicit,
            GenericWarheadSuperWeaponCapabilityId => Ra2AiInteractionRouteKind.AresGenericWarheadSuperWeaponExplicit,
            SuperWeaponProjectEditCapabilityId => Ra2AiInteractionRouteKind.SuperWeaponProjectEditExplicit,
            _ when availability.RulesArtProject == Ra2AiProjectEditAvailabilityKind.Available
                => Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit,
            _ => Ra2AiInteractionRouteKind.EditExplicit
        };

    private static bool HasUniqueProperties(JsonElement root, out string duplicateProperty)
    {
        duplicateProperty = string.Empty;
        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!seen.Add(property.Name))
            {
                duplicateProperty = property.Name;
                return false;
            }
        }
        return true;
    }

    internal static bool TryReadContextQueries(
        JsonElement root,
        out IReadOnlyList<Ra2AiContextQueryRequest> requests)
    {
        requests = ReadContextQueries(root, []);
        return true;
    }

    private static IReadOnlyList<Ra2AiContextQueryRequest> ReadContextQueries(
        JsonElement root,
        List<string> recoveryNotes)
    {
        if (!root.TryGetProperty("context_queries", out JsonElement element))
        {
            recoveryNotes.Add("缺少 context_queries，按空查询处理");
            return [];
        }
        if (element.ValueKind != JsonValueKind.Array)
        {
            recoveryNotes.Add("context_queries 不是数组，按空查询处理");
            return [];
        }

        List<Ra2AiContextQueryRequest> parsed = [];
        int index = 0;
        foreach (JsonElement item in element.EnumerateArray())
        {
            if (index >= Ra2AiContextQueryExecutor.MaximumQueryCount)
            {
                recoveryNotes.Add("context_queries 超限，已截断");
                break;
            }

            int currentIndex = index++;
            if (item.ValueKind != JsonValueKind.Object || !HasUniqueQueryProperties(item))
            {
                recoveryNotes.Add($"跳过无效查询 #{currentIndex + 1}");
                continue;
            }

            foreach (JsonProperty property in item.EnumerateObject())
            {
                if (!ContextQueryProperties.Contains(property.Name))
                    recoveryNotes.Add($"查询 #{currentIndex + 1} 忽略附加字段 {BoundDiagnosticName(property.Name)}");
            }

            string kindText = ReadOptionalBoundedString(item, "kind", 64);
            string target = ReadOptionalBoundedString(item, "target", 16).ToLowerInvariant();
            if (target is not ("current" or "rules" or "art"))
            {
                recoveryNotes.Add($"跳过非符号目标查询 #{currentIndex + 1}");
                continue;
            }

            Ra2AiContextQueryKind? kind = NormalizeToken(kindText) switch
            {
                "getsection" => Ra2AiContextQueryKind.GetSection,
                "resolvereference" => Ra2AiContextQueryKind.ResolveReference,
                "searchobjects" => Ra2AiContextQueryKind.SearchObjects,
                _ => null
            };
            if (kind is null)
            {
                recoveryNotes.Add($"跳过未知查询类型 #{currentIndex + 1}");
                continue;
            }

            string section = ReadOptionalBoundedString(item, "section", 256);
            string key = ReadOptionalBoundedString(item, "key", 256);
            int sectionOccurrence = ReadOptionalBoundedInteger(item, "section_occurrence", -1, 10_000, -1);
            int fieldOccurrence = ReadOptionalBoundedInteger(item, "field_occurrence", -1, 10_000, -1);
            int referenceIndex = ReadOptionalBoundedInteger(item, "reference_index", 0, 1024, 0);
            string searchText = ReadOptionalBoundedString(item, "search_text", 256);
            string entityRole = ReadOptionalBoundedString(item, "entity_role", 64);
            IReadOnlyList<string> acceptedKinds = ReadOptionalStringList(item, "accepted_kinds", 8, 64);
            int maximumResults = ReadOptionalBoundedInteger(item, "maximum_results", 1, 8, 5);

            if (kind == Ra2AiContextQueryKind.GetSection)
            {
                if (string.IsNullOrWhiteSpace(section))
                {
                    recoveryNotes.Add($"跳过缺少 Section 的查询 #{currentIndex + 1}");
                    continue;
                }
                key = string.Empty;
                fieldOccurrence = -1;
                referenceIndex = 0;
            }
            else if (kind == Ra2AiContextQueryKind.ResolveReference)
            {
                if (string.IsNullOrWhiteSpace(section) || string.IsNullOrWhiteSpace(key))
                {
                    recoveryNotes.Add($"跳过不完整引用查询 #{currentIndex + 1}");
                    continue;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    recoveryNotes.Add($"跳过缺少搜索文本的查询 #{currentIndex + 1}");
                    continue;
                }
                section = string.Empty;
                key = string.Empty;
                sectionOccurrence = -1;
                fieldOccurrence = -1;
                referenceIndex = 0;
            }

            parsed.Add(new(
                kind.Value,
                target,
                section,
                key,
                sectionOccurrence < 0 ? null : sectionOccurrence,
                fieldOccurrence < 0 ? null : fieldOccurrence,
                referenceIndex)
            {
                SearchText = searchText,
                EntityRole = entityRole,
                AcceptedKinds = acceptedKinds,
                MaximumResults = maximumResults
            });
        }

        return Array.AsReadOnly(parsed.ToArray());
    }

    private static bool HasUniqueQueryProperties(JsonElement element)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        return element.EnumerateObject().All(property => seen.Add(property.Name));
    }

    private static Ra2AiIntentAnalysisParseResult Failure(
        Ra2AiIntentAnalysisFailureKind kind,
        string diagnosticMessage)
        => new(null, kind, diagnosticMessage, []);

    private static string BoundDiagnosticName(string value)
    {
        string normalized = value.Replace('\0', ' ').Trim();
        return normalized.Length <= 48 ? normalized : normalized[..48];
    }

    private static string ReadBoundedString(
        JsonElement root,
        string name,
        int maximumLength,
        string defaultValue,
        List<string> recoveryNotes)
    {
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind != JsonValueKind.String)
        {
            recoveryNotes.Add($"{name} 缺失或类型无效，已使用默认值");
            return defaultValue;
        }

        string value = (element.GetString() ?? string.Empty).Replace('\0', ' ').Trim();
        if (value.Length == 0)
        {
            recoveryNotes.Add($"{name} 为空，已使用默认值");
            return defaultValue;
        }
        if (value.Length > maximumLength)
        {
            recoveryNotes.Add($"{name} 超限，已截断");
            return value[..maximumLength];
        }
        return value;
    }

    private static IReadOnlyList<string> ReadBoundedStringList(
        JsonElement root,
        string name,
        int maximumItems,
        int maximumItemCharacters,
        List<string> recoveryNotes)
    {
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind != JsonValueKind.Array)
        {
            recoveryNotes.Add($"{name} 缺失或不是数组，按空列表处理");
            return [];
        }

        List<string> values = [];
        foreach (JsonElement item in element.EnumerateArray())
        {
            if (values.Count >= maximumItems)
            {
                recoveryNotes.Add($"{name} 超限，已截断");
                break;
            }
            if (item.ValueKind != JsonValueKind.String)
            {
                recoveryNotes.Add($"{name} 中的非字符串项已忽略");
                continue;
            }
            string value = (item.GetString() ?? string.Empty).Replace('\0', ' ').Trim();
            if (value.Length == 0)
                continue;
            if (value.Length > maximumItemCharacters)
            {
                recoveryNotes.Add($"{name} 中的超长项已截断");
                value = value[..maximumItemCharacters];
            }
            values.Add(value);
        }
        return Array.AsReadOnly(values.ToArray());
    }

    private static Ra2AiIntentAnalysisOutcome ReadOutcome(
        JsonElement root,
        string capabilityId,
        List<string> recoveryNotes)
    {
        string value = ReadOptionalBoundedString(root, "outcome", 64);
        if (TryParseFlexibleEnum(value, out Ra2AiIntentAnalysisOutcome outcome))
            return outcome;

        recoveryNotes.Add("outcome 缺失或未知，已按 Work 工具调用语义归一化");
        return capabilityId switch
        {
            "unsupported" => Ra2AiIntentAnalysisOutcome.Unsupported,
            "advisory" => Ra2AiIntentAnalysisOutcome.Advisory,
            _ => Ra2AiIntentAnalysisOutcome.Authoring
        };
    }

    private static Ra2AiIntentCompletionLevel ReadCompletionLevel(
        JsonElement root,
        string capabilityId,
        Ra2AiIntentAnalysisOutcome outcome,
        List<string> recoveryNotes)
    {
        string value = ReadOptionalBoundedString(root, "completion_level", 64);
        if (TryParseFlexibleEnum(value, out Ra2AiIntentCompletionLevel completionLevel))
            return completionLevel;

        recoveryNotes.Add("completion_level 缺失或未知，已由执行能力归一化");
        if (outcome != Ra2AiIntentAnalysisOutcome.Authoring)
            return Ra2AiIntentCompletionLevel.None;
        return capabilityId switch
        {
            "current-document-field-edit" or ProjectRulesArtBindingCapabilityId
                => Ra2AiIntentCompletionLevel.Field,
            "weapon-chain-skeleton" => Ra2AiIntentCompletionLevel.Skeleton,
            _ => Ra2AiIntentCompletionLevel.Complete
        };
    }

    private static bool TryParseFlexibleEnum<T>(string value, out T parsed)
        where T : struct, Enum
        => Enum.TryParse(NormalizeToken(value), ignoreCase: true, out parsed) && Enum.IsDefined(parsed);

    private static string NormalizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
                builder.Append(character);
        }
        return builder.ToString();
    }

    private static string ReadOptionalBoundedString(JsonElement root, string name, int maximumLength)
    {
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind != JsonValueKind.String)
            return string.Empty;
        string value = (element.GetString() ?? string.Empty).Replace('\0', ' ').Trim();
        return value.Length <= maximumLength ? value : value[..maximumLength];
    }

    private static int ReadOptionalBoundedInteger(
        JsonElement root,
        string name,
        int minimum,
        int maximum,
        int defaultValue)
    {
        if (!root.TryGetProperty(name, out JsonElement element) ||
            element.ValueKind != JsonValueKind.Number ||
            !element.TryGetInt32(out int value))
        {
            return defaultValue;
        }
        return Math.Clamp(value, minimum, maximum);
    }

    private static IReadOnlyList<string> ReadOptionalStringList(
        JsonElement root,
        string name,
        int maximumItems,
        int maximumItemCharacters)
    {
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind != JsonValueKind.Array)
            return [];
        List<string> values = [];
        foreach (JsonElement item in element.EnumerateArray())
        {
            if (values.Count >= maximumItems)
                break;
            if (item.ValueKind != JsonValueKind.String)
                continue;
            string value = (item.GetString() ?? string.Empty).Replace('\0', ' ').Trim();
            if (value.Length == 0)
                continue;
            values.Add(value.Length <= maximumItemCharacters ? value : value[..maximumItemCharacters]);
        }
        return Array.AsReadOnly(values.ToArray());
    }

    private static Ra2AiToolDefinition BuildAnalysisTool(
        IReadOnlyList<Ra2AgentSkillManifestEntry> manifest)
    {
        string skillIds = JsonSerializer.Serialize(manifest.Select(skill => skill.Id));
        string schema = $$$"""
        {
          "type":"object",
          "additionalProperties":false,
          "properties":{
            "outcome":{"type":"string","enum":["advisory","authoring","needs_clarification","unsupported"]},
            "capability_id":{"type":"string","enum":["advisory","unsupported","current-document-field-edit","weapon-chain-skeleton","weapon-chain-complete","techno-dual-armament-complete","arcing-projectile-complete","homing-projectile-complete","yr-core-warhead-complete","techno-rules-art-binding","project-rules-art-edit","ares-unitdelivery-superweapon-complete","ares-genericwarhead-superweapon-complete","superweapon-project-edit"]},
            "domain_intent_id":{"type":"string","enum":["ini-document","field-schema","weapon-chain","projectile-trajectory","warhead-damage","ai-programming","superweapon","faction","particle-radiation","terrain-resource","sound-eva","art-animation","techno","reference-registration"]},
            "request_summary":{"type":"string","minLength":1,"maxLength":512},
            "completion_level":{"type":"string","enum":["none","field","skeleton","complete"]},
            "constraints":{"type":"array","maxItems":12,"items":{"type":"string","minLength":1,"maxLength":256}},
            "selected_skill_ids":{"type":"array","maxItems":6,"items":{"type":"string","enum":{{{skillIds}}}}},
            "knowledge_gaps":{"type":"array","maxItems":6,"items":{"type":"string","minLength":1,"maxLength":256}}
            ,"context_queries":{{{ContextQueryArraySchema}}}
          },
          "required":["outcome","capability_id","domain_intent_id","request_summary","completion_level","constraints","selected_skill_ids","knowledge_gaps","context_queries"]
        }
        """;
        return new Ra2AiToolDefinition(
            ToolName,
            "Classify one RA2 IDE Work-mode request and select relevant catalog Skills. Return facts only, never reasoning, INI, edits, or prose.",
            schema);
    }

    private static string FormatModes(Ra2AgentSkillMode modes)
        => modes switch
        {
            Ra2AgentSkillMode.Chat => "chat",
            Ra2AgentSkillMode.Work => "work",
            _ => "chat,work"
        };

    private static void NormalizeCapabilityMetadata(
        Ra2AiIntentAnalysisOutcome outcome,
        IReadOnlyList<Ra2AiContextQueryRequest> contextQueries,
        ref string capabilityId,
        ref string domainIntentId,
        ref Ra2AiIntentCompletionLevel completionLevel)
    {
        if (outcome != Ra2AiIntentAnalysisOutcome.Authoring)
        {
            completionLevel = Ra2AiIntentCompletionLevel.None;
            return;
        }

        // A field-edit package that explicitly asks the Host to read symbolic rules/art
        // context is project-scoped even when the provider mislabeled it as current-document.
        // This only aligns execution scope with the provider's own bounded query declaration;
        // it does not infer, retarget, or rewrite any INI operation.
        if (string.Equals(capabilityId, "current-document-field-edit", StringComparison.Ordinal) &&
            contextQueries.Any(query => query.Target is "rules" or "art"))
        {
            capabilityId = string.Equals(domainIntentId, "superweapon", StringComparison.Ordinal)
                ? SuperWeaponProjectEditCapabilityId
                : ProjectRulesArtBindingCapabilityId;
        }

        if (capabilityId is UnitDeliverySuperWeaponCapabilityId or
            GenericWarheadSuperWeaponCapabilityId or
            SuperWeaponProjectEditCapabilityId)
        {
            domainIntentId = "superweapon";
            completionLevel = Ra2AiIntentCompletionLevel.Complete;
            return;
        }

        if (!string.Equals(capabilityId, ProjectRulesArtBindingCapabilityId, StringComparison.Ordinal))
        {
            completionLevel = capabilityId switch
            {
                "current-document-field-edit" => Ra2AiIntentCompletionLevel.Field,
                "weapon-chain-skeleton" => Ra2AiIntentCompletionLevel.Skeleton,
                _ when AuthoringCapabilityIds.Contains(capabilityId) => Ra2AiIntentCompletionLevel.Complete,
                _ => completionLevel
            };
            return;
        }

        // The exact capability selects one reviewed project tool and one local compiler.
        // Provider domain/completion values are descriptive metadata, so canonicalize them
        // instead of allowing either value to veto the already-bounded capability.
        domainIntentId = ProjectRulesArtBindingDomainIntentId;
        completionLevel = Ra2AiIntentCompletionLevel.Field;
    }

    private static string Normalize(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "(none)";
        string normalized = value.Replace('\0', ' ').Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }

    private static string SanitizeAndNormalize(string? value, int maximumLength)
        => Normalize(Ra2AiOutboundTextSanitizer.Sanitize(value).Text, maximumLength);
}
