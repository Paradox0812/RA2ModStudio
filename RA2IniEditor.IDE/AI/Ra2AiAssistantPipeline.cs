using System.Threading;
using System.Threading.Tasks;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiAssistantPipeline
{
    private readonly IRa2AiPromptBuilder _promptBuilder;
    private readonly IRa2AiClient _client;
    private readonly Ra2AiContextQueryExecutor? _contextQueryExecutor;

    public Ra2AiAssistantPipeline(IRa2AiPromptBuilder promptBuilder, IRa2AiClient client)
        : this(promptBuilder, client, contextQueryGateway: null)
    {
    }

    public Ra2AiAssistantPipeline(
        IRa2AiPromptBuilder promptBuilder,
        IRa2AiClient client,
        IRa2AutomationCapabilityGateway? contextQueryGateway)
    {
        _promptBuilder = promptBuilder ?? throw new ArgumentNullException(nameof(promptBuilder));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _contextQueryExecutor = contextQueryGateway is null
            ? null
            : new Ra2AiContextQueryExecutor(contextQueryGateway);
    }

    public async Task<Ra2AiAssistantPipelineResult> SendAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        CancellationToken cancellationToken)
    {
        Ra2AiRequest request = BuildRequest(userPrompt, context, conversationContext, currentSubject);
        Ra2AiResponse response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return new Ra2AiAssistantPipelineResult(request, response);
    }

    public async Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiCapabilityMode capabilityMode,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(onContentDelta);

        Ra2AiRequest request = BuildRequest(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            capabilityMode,
            capabilityMode == Ra2AiCapabilityMode.AdvisoryOnly ? Ra2AiUserMode.Chat : Ra2AiUserMode.Work,
            "ini-document");
        Ra2AiResponse response = await _client.SendStreamingAsync(
            request,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);
        if (request.ToolChoice == Ra2AiToolChoiceMode.Required &&
            response.Kind == Ra2AiResponseKind.Success)
        {
            response = Ra2AiResponse.CreateAuthoringToolNotInvoked(
                response.Text,
                response.Diagnostics);
        }
        return new Ra2AiAssistantPipelineResult(request, response);
    }

    public async Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiInteractionRoute interactionRoute,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
        => await SendStreamingAsync(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            interactionRoute,
            contextSources: null,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);

    public async Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiInteractionRoute interactionRoute,
        Ra2AiContextSourceSet? contextSources,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(onContentDelta);
        if (interactionRoute.UserMode == Ra2AiUserMode.Work)
        {
            return await SendWorkStreamingAsync(
                userPrompt,
                context,
                conversationContext,
                currentSubject,
                new Ra2AiAuthoringAvailability(
                    interactionRoute.EditAvailability,
                    interactionRoute.ProjectEditAvailability),
                contextSources,
                onContentDelta,
                cancellationToken).ConfigureAwait(false);
        }

        Ra2AiRequest request = BuildRequest(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            interactionRoute.CapabilityMode,
            interactionRoute.UserMode,
            interactionRoute.DomainIntentId);
        Ra2AiResponse response = await _client.SendStreamingAsync(
            request,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);
        if (request.ToolChoice == Ra2AiToolChoiceMode.Required &&
            response.Kind == Ra2AiResponseKind.Success)
        {
            response = Ra2AiResponse.CreateAuthoringToolNotInvoked(response.Text, response.Diagnostics);
        }

        return new Ra2AiAssistantPipelineResult(request, response);
    }

    private async Task<Ra2AiAssistantPipelineResult> SendWorkStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiAuthoringAvailability availability,
        Ra2AiContextSourceSet? contextSources,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        Ra2AiProjectContextSnapshot projectContext = Ra2AiProjectContextSnapshot.Create(contextSources);
        Ra2AiRequest analysisRequest = Ra2AiIntentAnalysisStage.BuildRequest(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            projectContext,
            _promptBuilder.SkillCatalog);
        Ra2AiResponse analysisResponse = await _client.SendAsync(
            analysisRequest,
            cancellationToken).ConfigureAwait(false);
        if (!analysisResponse.IsSuccessfulTerminal)
        {
            return new Ra2AiAssistantPipelineResult(analysisRequest, analysisResponse)
            {
                IntentAnalysisRequest = analysisRequest,
                IntentAnalysisResponse = analysisResponse
            };
        }

        Ra2AiIntentAnalysisParseResult intentParse = Ra2AiIntentAnalysisStage.Parse(analysisResponse);
        Ra2AiIntentAnalysisPackage? package = intentParse.Package;
        if (!intentParse.Succeeded || package is null)
        {
            Ra2AiResponse failure = Ra2AiResponse.CreateLocalRejection(
                $"Work 意图响应无法安全解析：{intentParse.DiagnosticMessage} 本次未进入执行阶段，也未修改文件。",
                analysisResponse.Diagnostics);
            return new Ra2AiAssistantPipelineResult(analysisRequest, failure)
            {
                IntentAnalysisRequest = analysisRequest,
                IntentAnalysisResponse = analysisResponse,
                IntentAnalysisParseResult = intentParse
            };
        }

        Ra2AiInteractionRoute resolvedRoute = Ra2AiIntentAnalysisStage.ResolveRoute(
            package,
            availability);
        Ra2AgentSkillSelectionResolution skillSelection = _promptBuilder.SkillCatalog.Resolve(
            package.SelectedSkillIds,
            package.KnowledgeGaps,
            package.CapabilityId,
            resolvedRoute.DomainIntentId,
            Ra2AiUserMode.Work,
            userPrompt);
        if (resolvedRoute.Kind == Ra2AiInteractionRouteKind.EditUnavailable)
        {
            Ra2AiResponse failure = Ra2AiResponse.CreateLocalRejection(
                package.CapabilityId is
                    "techno-rules-art-binding" or
                    "project-rules-art-edit" or
                    "ares-unitdelivery-superweapon-complete" or
                    "ares-genericwarhead-superweapon-complete" or
                    "superweapon-project-edit"
                    ? FormatProjectAvailabilityFailure(availability.RulesArtProject)
                    : "意图分析确认该请求需要结构化修改，但当前没有可用的编辑快照。",
                analysisResponse.Diagnostics);
            return new Ra2AiAssistantPipelineResult(analysisRequest, failure)
            {
                IntentAnalysisRequest = analysisRequest,
                IntentAnalysisResponse = analysisResponse,
                IntentAnalysisParseResult = intentParse,
                IntentAnalysisPackage = package,
                ResolvedInteractionRoute = resolvedRoute,
                SkillSelection = skillSelection
            };
        }

        Ra2AiSemanticRetrievalResult retrieval = await RetrieveSemanticContextAsync(
            userPrompt,
            package,
            skillSelection,
            projectContext,
            contextSources,
            cancellationToken).ConfigureAwait(false);
        if (retrieval.StopReason is Ra2AiSemanticRetrievalStopReason.ProviderFailure or
            Ra2AiSemanticRetrievalStopReason.NeedsClarification)
        {
            Ra2AiResponse failure = Ra2AiResponse.CreateLocalRejection(
                retrieval.Message,
                analysisResponse.Diagnostics);
            return new Ra2AiAssistantPipelineResult(analysisRequest, failure)
            {
                IntentAnalysisRequest = analysisRequest,
                IntentAnalysisResponse = analysisResponse,
                IntentAnalysisParseResult = intentParse,
                IntentAnalysisPackage = package,
                ResolvedInteractionRoute = resolvedRoute,
                SkillSelection = skillSelection,
                ProjectContext = projectContext,
                ContextQueryResults = retrieval.QueryResults,
                SemanticRetrieval = retrieval
            };
        }

        IReadOnlyList<Ra2AiContextQueryResult> contextQueryResults = retrieval.QueryResults;

        Ra2AiWorkExecutionSeed executionSeed = new(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            resolvedRoute,
            package,
            skillSelection,
            projectContext,
            contextQueryResults,
            retrieval.EntityBindings,
            retrieval.StopReason);
        Ra2AiRequest executionRequest = _promptBuilder.Build(executionSeed.ToPromptBuildRequest());
        Ra2AiResponse executionResponse = await _client.SendStreamingAsync(
            executionRequest,
            onContentDelta,
            cancellationToken).ConfigureAwait(false);
        if (executionRequest.ToolChoice == Ra2AiToolChoiceMode.Required &&
            executionResponse.Kind == Ra2AiResponseKind.Success)
        {
            executionResponse = Ra2AiResponse.CreateAuthoringToolNotInvoked(
                executionResponse.Text,
                executionResponse.Diagnostics);
        }

        return new Ra2AiAssistantPipelineResult(executionRequest, executionResponse)
        {
            IntentAnalysisRequest = analysisRequest,
            IntentAnalysisResponse = analysisResponse,
            IntentAnalysisParseResult = intentParse,
            IntentAnalysisPackage = package,
            ResolvedInteractionRoute = resolvedRoute,
            SkillSelection = skillSelection,
            ProjectContext = projectContext,
            ContextQueryResults = contextQueryResults,
            SemanticRetrieval = retrieval,
            ExecutionSeed = executionSeed
        };
    }

    private async Task<Ra2AiSemanticRetrievalResult> RetrieveSemanticContextAsync(
        string userPrompt,
        Ra2AiIntentAnalysisPackage package,
        Ra2AgentSkillSelectionResolution skillSelection,
        Ra2AiProjectContextSnapshot projectContext,
        Ra2AiContextSourceSet? contextSources,
        CancellationToken cancellationToken)
    {
        Ra2AiContextQueryExecutionSession querySession = new();
        List<Ra2AiContextQueryResult> accumulated = ExecuteContextQueries(
            contextSources,
            package.ContextQueries,
            querySession,
            cancellationToken).ToList();
        HashSet<string> fingerprints = package.ContextQueries
            .Select(Ra2AiSemanticRetrievalStage.Fingerprint)
            .ToHashSet(StringComparer.Ordinal);
        List<Ra2AiSemanticRetrievalAttempt> attempts = [];

        if (projectContext.Documents.Count > 0 &&
            Ra2AiSemanticRetrievalStage.ShouldRefine(package, accumulated))
        {
            for (int round = 1; round <= Ra2AiSemanticRetrievalStage.MaximumRefinementRounds; round++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2AiRequest request = Ra2AiSemanticRetrievalStage.BuildRequest(
                    round,
                    userPrompt,
                    package,
                    skillSelection,
                    projectContext,
                    accumulated);
                Ra2AiResponse response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                string retrievalFailure = string.Empty;
                Ra2AiSemanticRetrievalPackage? refinement = null;
                bool refinementParsed = response.IsSuccessfulTerminal &&
                                        Ra2AiSemanticRetrievalStage.TryParse(
                                            response,
                                            out refinement,
                                            out retrievalFailure);
                if (!refinementParsed || refinement is null)
                {
                    return new(
                        Array.AsReadOnly(accumulated.ToArray()),
                        Ra2AiSemanticRetrievalStage.CreateBindings(accumulated),
                        Array.AsReadOnly(attempts.ToArray()),
                        Ra2AiSemanticRetrievalStopReason.ProviderFailure,
                        string.IsNullOrWhiteSpace(retrievalFailure)
                            ? "DeepSeek 语义检索阶段未返回有效结果；本次未进入结构化执行，也未修改文件。"
                            : $"语义检索无法安全解析：{retrievalFailure} 本次未进入结构化执行，也未修改文件。");
                }

                if (refinement.Outcome == Ra2AiSemanticRetrievalOutcome.Ready)
                {
                    attempts.Add(new(round, [], [], 0, response.Kind, request.PromptText.Length));
                    return AddCapabilityEvidence(
                        package,
                        contextSources,
                        accumulated,
                        attempts,
                        fingerprints,
                        querySession,
                        Ra2AiSemanticRetrievalStopReason.EvidenceReady,
                        refinement.Message,
                        cancellationToken);
                }

                if (refinement.Outcome == Ra2AiSemanticRetrievalOutcome.NeedsClarification)
                {
                    attempts.Add(new(round, [], [], 0, response.Kind, request.PromptText.Length));
                    return new(
                        Array.AsReadOnly(accumulated.ToArray()),
                        Ra2AiSemanticRetrievalStage.CreateBindings(accumulated),
                        Array.AsReadOnly(attempts.ToArray()),
                        Ra2AiSemanticRetrievalStopReason.NeedsClarification,
                        string.IsNullOrWhiteSpace(refinement.Message)
                            ? "现有项目快照中无法唯一确定请求涉及的对象，请补充对象名称或 Section ID。"
                            : refinement.Message);
                }

                Ra2AiContextQueryRequest[] freshQueries = refinement.ContextQueries
                    .Where(query => fingerprints.Add(Ra2AiSemanticRetrievalStage.Fingerprint(query)))
                    .Take(4)
                    .ToArray();
                if (freshQueries.Length == 0)
                {
                    attempts.Add(new(round, [], [], 0, response.Kind, request.PromptText.Length));
                    return AddCapabilityEvidence(
                        package,
                        contextSources,
                        accumulated,
                        attempts,
                        fingerprints,
                        querySession,
                        Ra2AiSemanticRetrievalStopReason.NoProgress,
                        "语义检索未产生新的查询，已停止补查并使用现有证据进入执行。",
                        cancellationToken);
                }

                IReadOnlyList<Ra2AiContextQueryResult> results = ExecuteContextQueries(
                    contextSources,
                    freshQueries,
                    querySession,
                    cancellationToken);
                int newEvidence = results.Count(result => result.Succeeded);
                accumulated.AddRange(results);
                attempts.Add(new(
                    round,
                    Array.AsReadOnly(freshQueries),
                    results,
                    newEvidence,
                    response.Kind,
                    request.PromptText.Length));
                if (!Ra2AiSemanticRetrievalStage.ShouldRefine(package, accumulated))
                {
                    return AddCapabilityEvidence(
                        package,
                        contextSources,
                        accumulated,
                        attempts,
                        fingerprints,
                        querySession,
                        Ra2AiSemanticRetrievalStopReason.EvidenceReady,
                        "补查已解析所需项目实体。",
                        cancellationToken);
                }
            }

            return AddCapabilityEvidence(
                package,
                contextSources,
                accumulated,
                attempts,
                fingerprints,
                querySession,
                Ra2AiSemanticRetrievalStopReason.RoundLimit,
                "已达到两轮语义补查上限，使用已验证证据进入结构化执行。",
                cancellationToken);
        }

        return AddCapabilityEvidence(
            package,
            contextSources,
            accumulated,
            attempts,
            fingerprints,
            querySession,
            Ra2AiSemanticRetrievalStopReason.NoRefinementRequired,
            "初始 Host 证据已足够。",
            cancellationToken);
    }

    private Ra2AiSemanticRetrievalResult AddCapabilityEvidence(
        Ra2AiIntentAnalysisPackage package,
        Ra2AiContextSourceSet? contextSources,
        List<Ra2AiContextQueryResult> accumulated,
        List<Ra2AiSemanticRetrievalAttempt> attempts,
        HashSet<string> fingerprints,
        Ra2AiContextQueryExecutionSession querySession,
        Ra2AiSemanticRetrievalStopReason stopReason,
        string message,
        CancellationToken cancellationToken)
    {
        List<Ra2AiContextQueryRequest> evidenceQueries = [];
        if (contextSources?.RulesArtProject?.ProjectSnapshot is not null &&
            (package.CapabilityId is
                "ares-unitdelivery-superweapon-complete" or
                "ares-genericwarhead-superweapon-complete" or
                "superweapon-project-edit" ||
             string.Equals(package.DomainIntentId, "superweapon", StringComparison.Ordinal)))
        {
            evidenceQueries.Add(new(
                Ra2AiContextQueryKind.GetSection,
                "rules",
                "SuperWeaponTypes",
                string.Empty,
                null,
                null,
                0));
        }

        foreach (Ra2AiResolvedEntityBinding binding in Ra2AiSemanticRetrievalStage.CreateBindings(accumulated))
        {
            evidenceQueries.Add(new(
                Ra2AiContextQueryKind.GetSection,
                binding.Target,
                binding.CanonicalSection,
                string.Empty,
                null,
                null,
                0));
        }

        Ra2AiContextQueryRequest[] fresh = evidenceQueries
            .Where(query => fingerprints.Add(Ra2AiSemanticRetrievalStage.Fingerprint(query)))
            .Take(Ra2AiContextQueryExecutor.MaximumQueryCount)
            .ToArray();
        if (fresh.Length > 0)
            accumulated.AddRange(ExecuteContextQueries(contextSources, fresh, querySession, cancellationToken));

        return new(
            Array.AsReadOnly(accumulated.ToArray()),
            Ra2AiSemanticRetrievalStage.CreateBindings(accumulated),
            Array.AsReadOnly(attempts.ToArray()),
            stopReason,
            message);
    }

    internal async Task<Ra2AiStructuredRepairAttemptResult> SendStructuredRepairAsync(
        Ra2AiWorkExecutionSeed executionSeed,
        Ra2AiStructuredFailureEvidence failureEvidence,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executionSeed);
        ArgumentNullException.ThrowIfNull(failureEvidence);
        cancellationToken.ThrowIfCancellationRequested();

        Ra2AiRequest request = _promptBuilder.Build(
            executionSeed.ToPromptBuildRequest(new Ra2AiStructuredRepairContext(failureEvidence)));
        Ra2AiResponse response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (request.ToolChoice == Ra2AiToolChoiceMode.Required &&
            response.Kind == Ra2AiResponseKind.Success)
        {
            response = Ra2AiResponse.CreateAuthoringToolNotInvoked(response.Text, response.Diagnostics);
        }

        return new Ra2AiStructuredRepairAttemptResult(request, response);
    }

    private IReadOnlyList<Ra2AiContextQueryResult> ExecuteContextQueries(
        Ra2AiContextSourceSet? contextSources,
        IReadOnlyList<Ra2AiContextQueryRequest> requests,
        Ra2AiContextQueryExecutionSession? querySession,
        CancellationToken cancellationToken)
    {
        if (requests.Count == 0)
            return [];
        if (_contextQueryExecutor is not null)
            return querySession is null
                ? _contextQueryExecutor.Execute(contextSources, requests, cancellationToken)
                : _contextQueryExecutor.Execute(contextSources, requests, querySession, cancellationToken);

        return Array.AsReadOnly(requests.Select(request => new Ra2AiContextQueryResult(
            request,
            false,
            "GatewayUnavailable",
            "The local read-only context query gateway is unavailable.",
            null,
            null)).ToArray());
    }

    public Task<Ra2AiAssistantPipelineResult> SendStreamingAsync(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
        => SendStreamingAsync(
            userPrompt,
            context,
            conversationContext,
            currentSubject,
            Ra2AiCapabilityMode.AdvisoryOnly,
            onContentDelta,
            cancellationToken);

    private static string FormatProjectAvailabilityFailure(Ra2AiProjectEditAvailabilityKind availability)
        => availability switch
        {
            Ra2AiProjectEditAvailabilityKind.NoProject => "该项目修改需要先打开一个真实项目。",
            Ra2AiProjectEditAvailabilityKind.PairMissing => "当前项目缺少唯一的 rulesmd.ini 或 rules.ini 目标。",
            Ra2AiProjectEditAvailabilityKind.PairAmbiguous => "当前项目存在重复或冲突的 rules 目标，无法确定修改文档。",
            Ra2AiProjectEditAvailabilityKind.ReadOnly => "项目 INI 目标文档包含只读文件，无法生成项目修改预览。",
            Ra2AiProjectEditAvailabilityKind.ResourceLimitExceeded => "项目 INI 目标文档超过结构化编辑资源上限。",
            _ => "当前 rules/art 项目快照不可用，请重新打开项目后重试。"
        };

    public Task<Ra2AiAssistantPipelineResult> SendAsync(
        string userPrompt,
        Ra2AiContext context,
        CancellationToken cancellationToken)
        => SendAsync(userPrompt, context, conversationContext: null, currentSubject: null, cancellationToken);

    private Ra2AiRequest BuildRequest(
        string userPrompt,
        Ra2AiContext context,
        Ra2AiConversationContext? conversationContext,
        Ra2AiCurrentSubject? currentSubject,
        Ra2AiCapabilityMode capabilityMode = Ra2AiCapabilityMode.AdvisoryOnly,
        Ra2AiUserMode userMode = Ra2AiUserMode.Chat,
        string domainIntentId = "ini-document",
        Ra2AiIntentAnalysisPackage? intentAnalysisPackage = null,
        Ra2AgentSkillSelectionResolution? skillSelection = null,
        Ra2AiProjectContextSnapshot? projectContext = null,
        IReadOnlyList<Ra2AiContextQueryResult>? contextQueryResults = null)
        => _promptBuilder.Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = userPrompt,
            Context = context,
            ConversationContext = conversationContext,
            CurrentSubject = currentSubject,
            CapabilityMode = capabilityMode,
            UserMode = userMode,
            DomainIntentId = domainIntentId,
            IntentAnalysisPackage = intentAnalysisPackage,
            SkillSelection = skillSelection,
            ProjectContext = projectContext,
            ContextQueryResults = contextQueryResults ?? []
        });
}

internal sealed record Ra2AiAssistantPipelineResult(Ra2AiRequest Request, Ra2AiResponse Response)
{
    public Ra2AiRequest? IntentAnalysisRequest { get; init; }

    public Ra2AiResponse? IntentAnalysisResponse { get; init; }

    public Ra2AiIntentAnalysisParseResult? IntentAnalysisParseResult { get; init; }

    public Ra2AiIntentAnalysisPackage? IntentAnalysisPackage { get; init; }

    public Ra2AiInteractionRoute? ResolvedInteractionRoute { get; init; }

    public Ra2AgentSkillSelectionResolution? SkillSelection { get; init; }

    public Ra2AiProjectContextSnapshot? ProjectContext { get; init; }

    public IReadOnlyList<Ra2AiContextQueryResult> ContextQueryResults { get; init; } = [];

    public Ra2AiSemanticRetrievalResult? SemanticRetrieval { get; init; }

    public Ra2AiWorkExecutionSeed? ExecutionSeed { get; init; }
}
