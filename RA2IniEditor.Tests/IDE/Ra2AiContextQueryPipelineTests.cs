using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.Infrastructure.FieldRegistry;
using System.Text.Json;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiContextQueryPipelineTests
{
    [Fact]
    public async Task WorkPipeline_SharesConversationAndCapturedProjection_AndResolvesSectionBetweenTwoCalls()
    {
        Ra2AiConversationContext conversation = new()
        {
            Turns =
            [
                new Ra2AiConversationTurn
                {
                    Role = Ra2AiConversationRole.User,
                    Text = "上一轮指定 HTNKART 作为美术 Section。"
                }
            ],
            TotalCharacterCount = 27,
            WasTruncated = false
        };
        Ra2AiContextSourceSet sources = CreateProjectSources();
        SequencedClient client = new(
            IntentResponse(
                """
                [
                  {"kind":"get_section","target":"art","section":"HTNKART","key":"","section_occurrence":-1,"field_occurrence":-1,"reference_index":0},
                  {"kind":"resolve_reference","target":"rules","section":"HTNK","key":"Primary","section_occurrence":-1,"field_occurrence":-1,"reference_index":0}
                ]
                """,
                capabilityId: "current-document-field-edit"),
            Ra2AiResponse.CreateToolCalls(
            [
                new Ra2AiToolCall(
                    "project-1",
                    Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                    """{"outcome":"needs_clarification","message":"test terminal"}""")
            ]));
        Ra2AiAssistantPipeline pipeline = new(
            new Ra2AiPromptBuilder(),
            client,
            new Ra2AutomationCapabilityGateway());

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "给 HTNK 绑定已有美术。",
            CreateContext(),
            conversation,
            new Ra2AiCurrentSubject
            {
                Kind = Ra2AiSubjectKind.Unit,
                SubjectId = "HTNK",
                Source = Ra2AiSubjectSource.CurrentCaretSection,
                Summary = "当前单位 HTNK。",
                Confidence = 1,
                IsDraft = false
            },
            ProjectRoute(),
            sources,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(1, client.NonStreamingCallCount);
        Assert.Equal(1, client.StreamingCallCount);
        Assert.Equal(2, client.Requests.Count);
        Assert.Equal(
            ExtractPromptSection(client.Requests[0].UserContentText!, "## Conversation Context"),
            ExtractPromptSection(client.Requests[1].UserContentText!, "## Conversation Context"));
        Assert.Equal(
            ExtractProjectProjectionLines(client.Requests[0].UserContentText!),
            ExtractProjectProjectionLines(client.Requests[1].UserContentText!));
        using (JsonDocument.Parse(Assert.Single(client.Requests[0].Tools).ParametersJsonSchema))
        {
        }
        Assert.All(client.Requests, request =>
        {
            Assert.Contains("上一轮指定 HTNKART", request.UserContentText, StringComparison.Ordinal);
            Assert.Contains("target=rules; file=rulesmd.ini", request.UserContentText, StringComparison.Ordinal);
            Assert.Contains("target=art; file=artmd.ini", request.UserContentText, StringComparison.Ordinal);
            Assert.DoesNotContain("H:\\RA2", request.PromptText, StringComparison.OrdinalIgnoreCase);
        });
        Assert.DoesNotContain("Host-resolved Read-only Context Facts", client.Requests[0].PromptText, StringComparison.Ordinal);
        Assert.Contains("explicit structured field edit to a captured rules or art file", client.Requests[0].SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("Host-resolved Read-only Context Facts", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Contains("ResolvedDocumentTarget: art", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Contains("must use target=art", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Contains("authoritative captured location evidence", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Contains("ResolvedSection: [HTNKART]", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Contains("Image=HTNKBODY", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Contains("TargetSection: [120mm]", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Equal(2, result.ContextQueryResults.Count);
        Assert.All(result.ContextQueryResults, queryResult => Assert.True(queryResult.Succeeded));
        Assert.Equal(2, result.ProjectContext?.Documents.Count);
        Assert.Equal(Ra2AiCapabilityMode.ProjectRulesArtBindingPreview, result.ResolvedInteractionRoute?.CapabilityMode);
    }

    [Fact]
    public async Task WorkPipeline_DropsArbitraryContextTargetAndContinuesThroughBoundedStages()
    {
        SequencedClient client = new(
            IntentResponse(
                """
                [{"kind":"get_section","target":"H:\\RA2\\YR_Test\\rules.ini","section":"HTNK","key":"","section_occurrence":-1,"field_occurrence":-1,"reference_index":0}]
                """),
            RetrievalResponse("ready", "continue with captured project facts", "[]"),
            Ra2AiResponse.CreateToolCalls([
                new Ra2AiToolCall(
                    "project-after-safe-drop",
                    Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                    """{"outcome":"needs_clarification","message":"test terminal"}""")
            ]));
        Ra2AiAssistantPipeline pipeline = new(
            new Ra2AiPromptBuilder(),
            client,
            new Ra2AutomationCapabilityGateway());

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "给 HTNK 绑定已有美术。",
            CreateContext(),
            null,
            null,
            ProjectRoute(),
            CreateProjectSources(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(Ra2AiResponseKind.ToolCalls, result.Response.Kind);
        Assert.Equal(2, client.NonStreamingCallCount);
        Assert.Equal(1, client.StreamingCallCount);
        Assert.Empty(result.IntentAnalysisPackage!.ContextQueries);
        Assert.Contains(
            result.IntentAnalysisParseResult!.RecoveryNotes,
            note => note.Contains("非符号目标", StringComparison.Ordinal));
    }

    [Fact]
    public void IntentParser_AcceptsLegacyPackageWithoutContextQueriesAsEmpty()
    {
        Ra2AiResponse response = IntentResponse(contextQueriesJson: null);

        bool succeeded = Ra2AiIntentAnalysisStage.TryParse(response, out Ra2AiIntentAnalysisPackage? package, out _);

        Assert.True(succeeded);
        Assert.NotNull(package);
        Assert.Empty(package!.ContextQueries);
    }

    [Fact]
    public void IntentParser_FieldEditQueryingArt_NormalizesToProjectRoute()
    {
        Ra2AiResponse response = IntentResponse(
            """
            [{"kind":"get_section","target":"art","section":"HTNKART","key":"","section_occurrence":-1,"field_occurrence":-1,"reference_index":0}]
            """,
            capabilityId: "current-document-field-edit");

        bool parsed = Ra2AiIntentAnalysisStage.TryParse(
            response,
            out Ra2AiIntentAnalysisPackage? package,
            out string failureMessage);
        Ra2AiInteractionRoute route = Ra2AiIntentAnalysisStage.ResolveRoute(
            Assert.IsType<Ra2AiIntentAnalysisPackage>(package),
            new Ra2AiAuthoringAvailability(
                Ra2AiEditAvailabilityKind.Available,
                Ra2AiProjectEditAvailabilityKind.Available));

        Assert.True(parsed, failureMessage);
        Assert.Equal("techno-rules-art-binding", package!.CapabilityId);
        Assert.Equal(Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit, route.Kind);
        Assert.Equal(Ra2AiCapabilityMode.ProjectRulesArtBindingPreview, route.CapabilityMode);
    }

    [Fact]
    public void ContextQueryExecutor_PreCancelledTokenStopsBeforeGatewayQuery()
    {
        Ra2AiContextQueryExecutor executor = new(new Ra2AutomationCapabilityGateway());
        using CancellationTokenSource source = new();
        source.Cancel();

        Assert.Throws<OperationCanceledException>(() => executor.Execute(
            CreateProjectSources(),
            [new(
                Ra2AiContextQueryKind.GetSection,
                "rules",
                "HTNK",
                string.Empty,
                null,
                null,
                0)],
            source.Token));
    }

    [Fact]
    public void ContextQueryExecutor_SearchObjects_ResolvesCanonicalIdFromLocalNameAndUiName()
    {
        Ra2AiContextQueryExecutor executor = new(new Ra2AutomationCapabilityGateway());
        Ra2AiContextSourceSet sources = CreateNamedObjectProjectSources();

        IReadOnlyList<Ra2AiContextQueryResult> results = executor.Execute(
            sources,
            [
                new(
                    Ra2AiContextQueryKind.SearchObjects,
                    "rules",
                    string.Empty,
                    string.Empty,
                    null,
                    null,
                    0)
                {
                    SearchText = "IFV",
                    EntityRole = "delivery-type",
                    AcceptedKinds = ["Vehicle"],
                    MaximumResults = 4
                },
                new(
                    Ra2AiContextQueryKind.SearchObjects,
                    "rules",
                    string.Empty,
                    string.Empty,
                    null,
                    null,
                    0)
                {
                    SearchText = "Name:E1",
                    EntityRole = "delivery-type",
                    AcceptedKinds = ["Infantry"],
                    MaximumResults = 4
                }
            ],
            CancellationToken.None);

        Assert.All(results, result => Assert.True(result.Succeeded, result.Message));
        Assert.Equal("FV", Assert.Single(results[0].Objects).CanonicalSection);
        Assert.Equal("Name", Assert.Single(results[0].Objects).MatchBasis);
        Assert.Equal("E1", Assert.Single(results[1].Objects).CanonicalSection);
        Assert.Equal("UIName", Assert.Single(results[1].Objects).MatchBasis);
    }

    [Fact]
    public void IntentParser_AcceptsExtendedSearchObjectQuery()
    {
        Ra2AiResponse response = IntentResponse(
            """
            [{"kind":"search_objects","target":"rules","section":"","key":"","section_occurrence":-1,"field_occurrence":-1,"reference_index":0,"search_text":"IFV","entity_role":"delivery-type","accepted_kinds":["Vehicle"],"maximum_results":4}]
            """);

        bool parsed = Ra2AiIntentAnalysisStage.TryParse(
            response,
            out Ra2AiIntentAnalysisPackage? package,
            out string failureMessage);

        Assert.True(parsed, failureMessage);
        Ra2AiContextQueryRequest query = Assert.Single(package!.ContextQueries);
        Assert.Equal(Ra2AiContextQueryKind.SearchObjects, query.Kind);
        Assert.Equal("delivery-type", query.EntityRole);
        Assert.Equal(["Vehicle"], query.AcceptedKinds);
    }

    [Fact]
    public void ContextQueryExecutor_AmbiguousExactAlias_DoesNotCreateCanonicalBinding()
    {
        Ra2AiContextQueryExecutor executor = new(new Ra2AutomationCapabilityGateway());
        Ra2AiContextQueryRequest query = new(
            Ra2AiContextQueryKind.SearchObjects,
            "rules",
            string.Empty,
            string.Empty,
            null,
            null,
            0)
        {
            SearchText = "IFV",
            EntityRole = "delivery-type",
            AcceptedKinds = ["Vehicle"],
            MaximumResults = 8
        };

        Ra2AiContextQueryResult result = Assert.Single(executor.Execute(
            CreateAmbiguousNamedObjectProjectSources(),
            [query],
            CancellationToken.None));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Objects.Count);
        Assert.Equal(result.Objects[0].Score, result.Objects[1].Score);
        Assert.Empty(Ra2AiSemanticRetrievalStage.CreateBindings([result]));
    }

    [Fact]
    public async Task WorkPipeline_RefinesOnce_BindsCanonicalEntity_ThenExecutes()
    {
        SequencedClient client = new(
            IntentResponse("[]"),
            RetrievalResponse(
                "query",
                "search local alias",
                """
                [{"kind":"search_objects","target":"rules","section":"","key":"","section_occurrence":-1,"field_occurrence":-1,"reference_index":0,"search_text":"IFV","entity_role":"delivery-type","accepted_kinds":["Vehicle"],"maximum_results":4}]
                """),
            Ra2AiResponse.CreateToolCalls(
            [
                new Ra2AiToolCall(
                    "project-refined",
                    Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                    """{"outcome":"needs_clarification","message":"test terminal"}""")
            ]));
        Ra2AiAssistantPipeline pipeline = new(
            new Ra2AiPromptBuilder(),
            client,
            new Ra2AutomationCapabilityGateway());

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "在项目中使用 IFV。",
            CreateContext(),
            null,
            null,
            ProjectRoute(),
            CreateNamedObjectProjectSources(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(2, client.NonStreamingCallCount);
        Assert.Equal(1, client.StreamingCallCount);
        Assert.Equal(Ra2AiSemanticRetrievalStopReason.EvidenceReady, result.SemanticRetrieval?.StopReason);
        Ra2AiResolvedEntityBinding binding = Assert.Single(result.SemanticRetrieval!.EntityBindings);
        Assert.Equal("FV", binding.CanonicalSection);
        Assert.Contains("canonical_section=FV", result.Request.UserContentText, StringComparison.Ordinal);
        Assert.Contains("ResolvedSection: [FV]", result.Request.UserContentText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WorkPipeline_RepeatedRefinementQuery_StopsWithoutAnotherProviderRound()
    {
        const string missingQuery =
            "[{\"kind\":\"search_objects\",\"target\":\"rules\",\"section\":\"\",\"key\":\"\",\"section_occurrence\":-1,\"field_occurrence\":-1,\"reference_index\":0,\"search_text\":\"NOT_PRESENT\",\"entity_role\":\"delivery-type\",\"accepted_kinds\":[\"Vehicle\"],\"maximum_results\":4}]";
        SequencedClient client = new(
            IntentResponse(missingQuery),
            RetrievalResponse("query", "repeat", missingQuery),
            Ra2AiResponse.CreateToolCalls(
            [
                new Ra2AiToolCall(
                    "project-no-progress",
                    Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                    """{"outcome":"needs_clarification","message":"not found"}""")
            ]));
        Ra2AiAssistantPipeline pipeline = new(
            new Ra2AiPromptBuilder(),
            client,
            new Ra2AutomationCapabilityGateway());

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "使用不存在的对象。",
            CreateContext(),
            null,
            null,
            ProjectRoute(),
            CreateNamedObjectProjectSources(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(2, client.NonStreamingCallCount);
        Assert.Equal(1, client.StreamingCallCount);
        Assert.Equal(Ra2AiSemanticRetrievalStopReason.NoProgress, result.SemanticRetrieval?.StopReason);
        Assert.Single(result.SemanticRetrieval!.Attempts);
    }

    [Fact]
    public async Task WorkPipeline_StopsAtTwoRefinementRounds()
    {
        SequencedClient client = new(
            IntentResponse("[]"),
            RetrievalResponse(
                "query",
                "first search",
                """
                [{"kind":"search_objects","target":"rules","section":"","key":"","section_occurrence":-1,"field_occurrence":-1,"reference_index":0,"search_text":"MISSING_ONE","entity_role":"delivery-type","accepted_kinds":["Vehicle"],"maximum_results":4}]
                """),
            RetrievalResponse(
                "query",
                "second search",
                """
                [{"kind":"search_objects","target":"rules","section":"","key":"","section_occurrence":-1,"field_occurrence":-1,"reference_index":0,"search_text":"MISSING_TWO","entity_role":"delivery-type","accepted_kinds":["Vehicle"],"maximum_results":4}]
                """),
            Ra2AiResponse.CreateToolCalls(
            [
                new Ra2AiToolCall(
                    "project-round-limit",
                    Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                    """{"outcome":"needs_clarification","message":"not found"}""")
            ]));
        Ra2AiAssistantPipeline pipeline = new(
            new Ra2AiPromptBuilder(),
            client,
            new Ra2AutomationCapabilityGateway());

        Ra2AiAssistantPipelineResult result = await pipeline.SendStreamingAsync(
            "使用项目中的目标对象。",
            CreateContext(),
            null,
            null,
            ProjectRoute(),
            CreateNamedObjectProjectSources(),
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.Equal(3, client.NonStreamingCallCount);
        Assert.Equal(1, client.StreamingCallCount);
        Assert.Equal(2, result.SemanticRetrieval?.Attempts.Count);
        Assert.Equal(Ra2AiSemanticRetrievalStopReason.RoundLimit, result.SemanticRetrieval?.StopReason);
    }

    private static Ra2AiResponse IntentResponse(
        string? contextQueriesJson,
        string capabilityId = "techno-rules-art-binding")
    {
        string contextProperty = contextQueriesJson is null
            ? string.Empty
            : $",\"context_queries\":{contextQueriesJson}";
        return Ra2AiResponse.CreateToolCalls(
        [
            new Ra2AiToolCall(
                "analysis-1",
                Ra2AiIntentAnalysisStage.ToolName,
                $$"""
                {
                  "outcome":"authoring",
                  "capability_id":"{{capabilityId}}",
                  "domain_intent_id":"art-animation",
                  "request_summary":"bind captured art",
                  "completion_level":"field",
                  "constraints":[],
                  "selected_skill_ids":["ra2-rules-art-binding"],
                  "knowledge_gaps":[]{{contextProperty}}
                }
                """)
        ]);
    }

    private static Ra2AiResponse RetrievalResponse(string outcome, string message, string queriesJson)
        => Ra2AiResponse.CreateToolCalls(
        [
            new Ra2AiToolCall(
                "retrieval-1",
                Ra2AiSemanticRetrievalStage.ToolName,
                $$"""
                {"outcome":"{{outcome}}","message":"{{message}}","context_queries":{{queriesJson}}}
                """)
        ]);

    private static Ra2AiContextSourceSet CreateProjectSources()
    {
        IRa2FieldDefinitionProvider provider = new BuiltInRa2FieldDefinitionProvider();
        Ra2AutomationFieldRegistrySnapshot registry = new(provider, 41);
        Ra2AutomationDocumentSnapshot rules = new(
            Guid.NewGuid(),
            3,
            @"H:\RA2\YR_Test\rulesmd.ini",
            "[HTNK]\nImage=HTNKART\nPrimary=120mm\n\n[120mm]\nProjectile=Cannon\nWarhead=AP\n",
            true,
            registry);
        Ra2AutomationDocumentSnapshot art = new(
            Guid.NewGuid(),
            5,
            @"H:\RA2\YR_Test\artmd.ini",
            "[HTNKART]\nImage=HTNKBODY\nCameo=HTNKICON\n",
            true,
            registry);
        Ra2AutomationProjectSnapshot project = new(
            Guid.NewGuid(),
            9,
            @"H:\RA2\YR_Test",
            [rules, art]);
        return new(
            null,
            Ra2AiAuthoringRequestContext.ForProject(project, [rules.FilePath, art.FilePath]));
    }

    private static Ra2AiContextSourceSet CreateNamedObjectProjectSources()
    {
        IRa2FieldDefinitionProvider provider = new BuiltInRa2FieldDefinitionProvider();
        Ra2AutomationFieldRegistrySnapshot registry = new(provider, 42);
        Ra2AutomationDocumentSnapshot rules = new(
            Guid.NewGuid(),
            1,
            @"H:\RA2\YR_Test\rulesmd.ini",
            "[InfantryTypes]\n0=E1\n\n[VehicleTypes]\n0=FV\n\n[E1]\nUIName=Name:E1\nName=GI\n\n[FV]\nUIName=Name:FV\nName=IFV\n",
            true,
            registry);
        Ra2AutomationProjectSnapshot project = new(
            Guid.NewGuid(),
            1,
            @"H:\RA2\YR_Test",
            [rules]);
        return new(null, Ra2AiAuthoringRequestContext.ForProject(project, [rules.FilePath]));
    }

    private static Ra2AiContextSourceSet CreateAmbiguousNamedObjectProjectSources()
    {
        IRa2FieldDefinitionProvider provider = new BuiltInRa2FieldDefinitionProvider();
        Ra2AutomationFieldRegistrySnapshot registry = new(provider, 43);
        Ra2AutomationDocumentSnapshot rules = new(
            Guid.NewGuid(),
            1,
            @"H:\RA2\YR_Test\rulesmd.ini",
            "[VehicleTypes]\n0=FV\n1=FV_ALT\n\n[FV]\nName=IFV\n\n[FV_ALT]\nName=IFV\n",
            true,
            registry);
        Ra2AutomationProjectSnapshot project = new(
            Guid.NewGuid(),
            1,
            @"H:\RA2\YR_Test",
            [rules]);
        return new(null, Ra2AiAuthoringRequestContext.ForProject(project, [rules.FilePath]));
    }

    private static Ra2AiInteractionRoute ProjectRoute()
        => Ra2AiInteractionRouter.Resolve(
            "给 HTNK 绑定已有美术。",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work) with
        {
            ProjectEditAvailability = Ra2AiProjectEditAvailabilityKind.Available
        };

    private static Ra2AiContext CreateContext()
        => new(
            "rulesmd.ini",
            1,
            1,
            Ra2CaretRegion.SectionHeader,
            "HTNK",
            "Vehicle",
            null,
            null,
            null,
            "[HTNK]",
            1,
            true);

    private static string ExtractPromptSection(string text, string heading)
    {
        int start = text.IndexOf(heading, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Missing prompt heading: {heading}");
        int next = text.IndexOf("## ", start + heading.Length, StringComparison.Ordinal);
        return next < 0 ? text[start..] : text[start..next];
    }

    private static string ExtractProjectProjectionLines(string text)
        => string.Join(
            "\n",
            text.Split(["\r\n", "\n"], StringSplitOptions.None)
                .Where(line =>
                    line.StartsWith("- Project scoped:", StringComparison.Ordinal) ||
                    line.StartsWith("- Project revision:", StringComparison.Ordinal) ||
                    line.StartsWith("- Captured targets:", StringComparison.Ordinal) ||
                    line.StartsWith("- target=", StringComparison.Ordinal)));

    private sealed class SequencedClient : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses;

        public SequencedClient(params Ra2AiResponse[] responses)
            => _responses = new Queue<Ra2AiResponse>(responses);

        public List<Ra2AiRequest> Requests { get; } = [];
        public int NonStreamingCallCount { get; private set; }
        public int StreamingCallCount { get; private set; }

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            NonStreamingCallCount++;
            return Task.FromResult(cancellationToken.IsCancellationRequested
                ? Ra2AiResponse.CreateCancelled()
                : _responses.Dequeue());
        }

        public Task<Ra2AiResponse> SendStreamingAsync(
            Ra2AiRequest request,
            Ra2AiContentDeltaHandler onContentDelta,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            StreamingCallCount++;
            return Task.FromResult(cancellationToken.IsCancellationRequested
                ? Ra2AiResponse.CreateCancelled()
                : _responses.Dequeue());
        }
    }
}
