namespace RA2IniEditor.IDE.AI;

/// <summary>冻结 Work 第二阶段已验证的输入，供同一请求内的一次修复重建 canonical prompt。</summary>
internal sealed record Ra2AiWorkExecutionSeed
{
    public Ra2AiWorkExecutionSeed(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiInteractionRoute resolvedRoute,
        Ra2AiIntentAnalysisPackage intentAnalysisPackage,
        Ra2AgentSkillSelectionResolution skillSelection,
        Ra2AiProjectContextSnapshot projectContext,
        IReadOnlyList<Ra2AiContextQueryResult> contextQueryResults,
        IReadOnlyList<Ra2AiResolvedEntityBinding>? entityBindings = null,
        Ra2AiSemanticRetrievalStopReason? retrievalStopReason = null)
    {
        UserPrompt = userPrompt ?? throw new ArgumentNullException(nameof(userPrompt));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ConversationContext = conversationContext;
        CurrentSubject = currentSubject;
        if (!Enum.IsDefined(resolvedRoute.Kind) ||
            !Enum.IsDefined(resolvedRoute.CapabilityMode) ||
            !Enum.IsDefined(resolvedRoute.UserMode) ||
            string.IsNullOrWhiteSpace(resolvedRoute.DomainIntentId))
        {
            throw new ArgumentException("Resolved Work route is invalid.", nameof(resolvedRoute));
        }
        ResolvedRoute = resolvedRoute;
        IntentAnalysisPackage = intentAnalysisPackage ?? throw new ArgumentNullException(nameof(intentAnalysisPackage));
        SkillSelection = skillSelection ?? throw new ArgumentNullException(nameof(skillSelection));
        ProjectContext = projectContext ?? throw new ArgumentNullException(nameof(projectContext));
        ContextQueryResults = Array.AsReadOnly(
            (contextQueryResults ?? throw new ArgumentNullException(nameof(contextQueryResults))).ToArray());
        EntityBindings = Array.AsReadOnly((entityBindings ?? []).ToArray());
        RetrievalStopReason = retrievalStopReason;
    }

    public string UserPrompt { get; }

    public Ra2AiContext Context { get; }

    public Ra2AiConversationContext? ConversationContext { get; }

    public Ra2AiCurrentSubject? CurrentSubject { get; }

    public Ra2AiInteractionRoute ResolvedRoute { get; }

    public Ra2AiIntentAnalysisPackage IntentAnalysisPackage { get; }

    public Ra2AgentSkillSelectionResolution SkillSelection { get; }

    public Ra2AiProjectContextSnapshot ProjectContext { get; }

    public IReadOnlyList<Ra2AiContextQueryResult> ContextQueryResults { get; }

    public IReadOnlyList<Ra2AiResolvedEntityBinding> EntityBindings { get; }

    public Ra2AiSemanticRetrievalStopReason? RetrievalStopReason { get; }

    public Ra2AiPromptBuildRequest ToPromptBuildRequest(Ra2AiStructuredRepairContext? repairContext = null)
        => new()
        {
            UserPrompt = UserPrompt,
            Context = Context,
            ConversationContext = ConversationContext,
            CurrentSubject = CurrentSubject,
            CapabilityMode = ResolvedRoute.CapabilityMode,
            UserMode = Ra2AiUserMode.Work,
            DomainIntentId = ResolvedRoute.DomainIntentId,
            IntentAnalysisPackage = IntentAnalysisPackage,
            SkillSelection = SkillSelection,
            ProjectContext = ProjectContext,
            ContextQueryResults = ContextQueryResults,
            EntityBindings = EntityBindings,
            RetrievalStopReason = RetrievalStopReason,
            RepairContext = repairContext
        };
}
