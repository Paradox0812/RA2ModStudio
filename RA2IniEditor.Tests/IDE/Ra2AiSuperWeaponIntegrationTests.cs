using System.Text.Json;
using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiSuperWeaponIntegrationTests
{
    [Fact]
    public void ProjectAdmission_AllowsOneRulesDocumentAndAddsMatchingArtWhenPresent()
    {
        Ra2AiProjectTargetResolution rulesOnly = Ra2AiProjectAuthoringAdmission.ResolveRulesWithOptionalArtTargets(
            ["C:\\mod\\rules.ini", "C:\\mod\\map.ini"]);
        Ra2AiProjectTargetResolution pair = Ra2AiProjectAuthoringAdmission.ResolveRulesWithOptionalArtTargets(
            ["C:\\mod\\artmd.ini", "C:\\mod\\rulesmd.ini"]);
        Ra2AiProjectTargetResolution ambiguous = Ra2AiProjectAuthoringAdmission.ResolveRulesWithOptionalArtTargets(
            ["C:\\mod\\rules.ini", "C:\\mod\\rulesmd.ini"]);

        Assert.True(rulesOnly.Succeeded);
        Assert.Equal(["rules.ini"], rulesOnly.TargetFilePaths.Select(Path.GetFileName));
        Assert.Equal(["rulesmd.ini", "artmd.ini"], pair.TargetFilePaths.Select(Path.GetFileName));
        Assert.Equal(Ra2AiProjectEditAvailabilityKind.PairAmbiguous, ambiguous.Availability);
    }

    [Theory]
    [InlineData("ares-unitdelivery-superweapon-complete", "ProjectAresUnitDeliverySuperWeaponPreview")]
    [InlineData("ares-genericwarhead-superweapon-complete", "ProjectAresGenericWarheadSuperWeaponPreview")]
    [InlineData("superweapon-project-edit", "ProjectSuperWeaponEditPreview")]
    public void IntentAnalysis_RoutesSuperWeaponCapabilitiesToProjectTools(
        string capabilityId,
        string expectedModeName)
    {
        Ra2AiCapabilityMode expectedMode = Enum.Parse<Ra2AiCapabilityMode>(expectedModeName);
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "intent-sw",
                Ra2AiIntentAnalysisStage.ToolName,
                $$"""
                {
                  "outcome":"authoring",
                  "capability_id":"{{capabilityId}}",
                  "domain_intent_id":"superweapon",
                  "request_summary":"create support power",
                  "completion_level":"complete",
                  "constraints":["preview only"],
                  "selected_skill_ids":["ra2-superweapon-authoring"],
                  "knowledge_gaps":[],
                  "context_queries":[]
                }
                """)
        ]);

        Assert.True(Ra2AiIntentAnalysisStage.TryParse(response, out Ra2AiIntentAnalysisPackage? package, out _));
        Ra2AiInteractionRoute route = Ra2AiIntentAnalysisStage.ResolveRoute(
            package!,
            new Ra2AiAuthoringAvailability(Ra2AiEditAvailabilityKind.Available, Ra2AiProjectEditAvailabilityKind.Available));

        Assert.Equal(expectedMode, route.CapabilityMode);
        Ra2AiToolDefinition tool = Assert.Single(Ra2AiAuthoringToolCatalog.GetTools(expectedMode));
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName, tool.Name);
        using JsonDocument schema = JsonDocument.Parse(tool.ParametersJsonSchema);
        Assert.Equal(JsonValueKind.Object, schema.RootElement.ValueKind);
    }

    [Theory]
    [InlineData("ares-unitdelivery-superweapon-complete")]
    [InlineData("ares-genericwarhead-superweapon-complete")]
    [InlineData("superweapon-project-edit")]
    public void IntentAnalysis_NormalizesSuperWeaponDescriptiveMetadata(string capabilityId)
    {
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "intent-sw-normalize",
                Ra2AiIntentAnalysisStage.ToolName,
                $$"""
                {
                  "outcome":"authoring",
                  "capability_id":"{{capabilityId}}",
                  "domain_intent_id":"ini-document",
                  "request_summary":"create support power from natural object names",
                  "completion_level":"field",
                  "constraints":[],
                  "selected_skill_ids":["ra2-superweapon-authoring"],
                  "knowledge_gaps":[],
                  "context_queries":[]
                }
                """)
        ]);

        Assert.True(Ra2AiIntentAnalysisStage.TryParse(response, out Ra2AiIntentAnalysisPackage? package, out _));
        Assert.Equal("superweapon", package!.DomainIntentId);
        Assert.Equal(Ra2AiIntentCompletionLevel.Complete, package.CompletionLevel);
    }

    [Fact]
    public void IntentAnalysis_ProjectQueryKeepsMislabeledSuperWeaponInSuperWeaponRoute()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "intent-sw-query",
                Ra2AiIntentAnalysisStage.ToolName,
                """
                {
                  "outcome":"authoring",
                  "capability_id":"current-document-field-edit",
                  "domain_intent_id":"superweapon",
                  "request_summary":"create support power",
                  "completion_level":"field",
                  "constraints":[],
                  "selected_skill_ids":["ra2-superweapon-authoring"],
                  "knowledge_gaps":[],
                  "context_queries":[{
                    "kind":"get_section","target":"rules","section":"GAPOWR","key":"",
                    "section_occurrence":-1,"field_occurrence":-1,"reference_index":0
                  }]
                }
                """)
        ]);

        Assert.True(Ra2AiIntentAnalysisStage.TryParse(response, out Ra2AiIntentAnalysisPackage? package, out _));
        Assert.Equal("superweapon-project-edit", package!.CapabilityId);
        Assert.Equal("superweapon", package.DomainIntentId);
        Assert.Equal(Ra2AiIntentCompletionLevel.Complete, package.CompletionLevel);
    }

    [Fact]
    public void IntentAnalysisPrompt_RequiresCanonicalCandidateVerificationForNaturalObjectNames()
    {
        Ra2AiRequest request = Ra2AiIntentAnalysisStage.BuildRequest(
            "给盟军发电厂增加投送美国大兵和多功能步兵车的支援技能",
            new Ra2AiContext(
                "rulesmd.ini", 0, 1, Ra2CaretRegion.Unknown, null, null, null, null,
                null, string.Empty, 0, false),
            currentSubject: null,
            Ra2AgentSkillCatalog.LoadBundled());

        Assert.Contains("prefer target=rules search_objects evidence", request.SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("Do not pass display names", request.SystemPromptText, StringComparison.Ordinal);
    }

    [Fact]
    public void CapabilityScope_AllProjectToolsRequireProjectContext()
    {
        foreach (Ra2AiCapabilityMode mode in Enum.GetValues<Ra2AiCapabilityMode>())
        {
            bool exposesProjectTool = Ra2AiAuthoringToolCatalog.GetTools(mode).Any(tool =>
                tool.Name is Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName or
                    Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName);

            Assert.Equal(exposesProjectTool, Ra2AiAuthoringToolCatalog.UsesProjectContext(mode));
        }
    }

    [Theory]
    [InlineData("ProjectAresUnitDeliverySuperWeaponPreview")]
    [InlineData("ProjectAresGenericWarheadSuperWeaponPreview")]
    [InlineData("ProjectSuperWeaponEditPreview")]
    public void BoundedReplan_SelectsProjectContextForSuperWeaponModes(string modeName)
    {
        Ra2AiCapabilityMode mode = Enum.Parse<Ra2AiCapabilityMode>(modeName);
        Ra2AutomationProjectSnapshot snapshot = RulesOnlySnapshot();
        Ra2AiAuthoringRequestContext projectContext = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiInteractionRoute route = new(
            Ra2AiInteractionRouteKind.SuperWeaponProjectEditExplicit,
            mode,
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work,
            "superweapon",
            Ra2AiProjectEditAvailabilityKind.Available);

        Ra2AiAuthoringRequestContext? selected = Ra2AiBoundedStructuredReplanCoordinator.SelectRequestContext(
            route,
            new Ra2AiContextSourceSet(CurrentDocument: null, RulesArtProject: projectContext));

        Assert.Same(projectContext, selected);
    }

    [Fact]
    public void Adapter_ExpandsUnitDeliveryAgainstRulesOnlyProjectSnapshot()
    {
        Ra2AutomationProjectSnapshot snapshot = RulesOnlySnapshot();
        Ra2AiToolCall call = new(
            "sw-unitdelivery",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            """
            {
              "outcome":"proposal",
              "template_id":"ares-unitdelivery-superweapon-complete",
              "template_version":1,
              "arguments":{
                "superWeaponId":"GAREINFORCEMENTS",
                "providerMode":"building",
                "providerBuildingId":"GAPOWR",
                "providerSlot":"SuperWeapon2",
                "uiName":"NAME:Reinforcements",
                "name":"Reinforcements",
                "isPowered":true,
                "rechargeTime":5,
                "action":"Custom",
                "sidebarImage":"REINICON",
                "showTimer":true,
                "disableableFromShell":false,
                "aiTargeting":"ParaDrop",
                "deliveryTypeIds":"E1,FV",
                "deliveryOwner":"invoker"
              }
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            call,
            Ra2AiAuthoringRequestContext.ForProject(snapshot, snapshot.Documents.Select(document => document.FilePath).ToArray()));

        Assert.True(result.Succeeded, result.Message);
        Assert.Null(result.AssetManifest);
        Ra2AutomationEditPlan documentPlan = Assert.Single(result.ProjectPlan!.DocumentPlans);
        Assert.Contains(documentPlan.Operations, operation => operation.SectionName == "SuperWeaponTypes" && operation.Value == "GAREINFORCEMENTS");
        Assert.Contains(documentPlan.Operations, operation => operation.SectionName == "GAREINFORCEMENTS" && operation.Key == "Deliver.Types");
        Assert.Single(result.ProjectPlan.DocumentPlans);
    }

    [Fact]
    public void Adapter_ExpandsUnitDeliveryUsingUniqueCapturedNameAliases()
    {
        Ra2AutomationProjectSnapshot snapshot = RulesOnlySnapshot();
        Ra2AiToolCall call = new(
            "sw-unitdelivery-aliases",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            """
            {
              "outcome":"proposal",
              "template_id":"ares-unitdelivery-superweapon-complete",
              "template_version":1,
              "arguments":{
                "superWeaponId":"GAREINFORCEMENTS","providerMode":"building",
                "providerBuildingId":"Allied Power Plant","providerSlot":"SuperWeapon2",
                "uiName":"NAME:Reinforcements","name":"Reinforcements",
                "isPowered":true,"rechargeTime":5,"action":"Custom","sidebarImage":"REINICON",
                "showTimer":true,"disableableFromShell":false,"aiTargeting":"ParaDrop",
                "deliveryTypeIds":"GI,IFV","deliveryOwner":"invoker"
              }
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            call,
            Ra2AiAuthoringRequestContext.ForProject(snapshot, snapshot.Documents.Select(document => document.FilePath).ToArray()));

        Assert.True(result.Succeeded, result.Message);
        Ra2AutomationEditPlan plan = Assert.Single(result.ProjectPlan!.DocumentPlans);
        Assert.Contains(plan.Operations, operation => operation.SectionName == "GAPOWR" && operation.Key == "SuperWeapon2");
        Assert.Contains(plan.Operations, operation => operation.SectionName == "GAREINFORCEMENTS" && operation.Key == "Deliver.Types" && operation.Value == "E1,FV");
    }

    [Fact]
    public async Task BoundedReplan_NaturalLanguageUnitDeliveryResolvesProjectFactsAndNameAliases()
    {
        const string prompt = "给盟军发电厂增加紧急增援支援技能，投送美国大兵和多功能步兵车，只预览。";
        Ra2AutomationProjectSnapshot snapshot = RulesOnlySnapshot();
        Ra2AiAuthoringRequestContext projectContext = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        SequencedClient client = new(
            NaturalLanguageUnitDeliveryIntentResponse(),
            NaturalLanguageUnitDeliveryRetrievalResponse(),
            UnitDeliveryAliasResponse());
        Ra2IniAuthoringWorkspace workspace = new(
            new Ra2IniEditPreviewService(new Ra2AutomationCapabilityGateway()),
            new RecordingPort(),
            new Ra2ProjectEditPreviewService());
        Ra2AiBoundedStructuredReplanCoordinator coordinator = new(
            new Ra2AiAssistantPipeline(new Ra2AiPromptBuilder(), client, new Ra2AutomationCapabilityGateway()),
            new Ra2AiProposalPreparationRunner(new Ra2AiAuthoringCoordinator(new Ra2AiAuthoringToolAdapter(), workspace)),
            new StableRecapturePort(projectContext));
        Ra2AiInteractionRoute route = Ra2AiInteractionRouter.Resolve(
            prompt,
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work) with
        {
            ProjectEditAvailability = Ra2AiProjectEditAvailabilityKind.Available
        };
        Ra2AiBoundedStructuredReplanRequest request = new(
            prompt,
            new Ra2AiContext(
                "rulesmd.ini", 0, 1, Ra2CaretRegion.Unknown, null, null, null, null,
                null, string.Empty, 0, false),
            ConversationContext: null,
            CurrentSubject: null,
            route,
            new Ra2AiContextSourceSet(CurrentDocument: null, RulesArtProject: projectContext));

        Ra2AiBoundedStructuredReplanResult result = await coordinator.ExecuteAsync(
            request,
            static (_, _) => ValueTask.CompletedTask,
            CancellationToken.None);

        Assert.False(result.RepairAttempted);
        Assert.True(result.FinalProposalResult?.Succeeded, result.FinalProposalResult?.Message);
        Assert.Equal(Ra2AiCapabilityMode.ProjectAresUnitDeliverySuperWeaponPreview, result.InitialPipelineResult.ResolvedInteractionRoute?.CapabilityMode);
        Assert.Equal(
            Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
            Assert.Single(result.InitialPipelineResult.Request.Tools).Name);
        Assert.Contains(
            result.InitialPipelineResult.IntentAnalysisParseResult!.RecoveryNotes,
            note => note.Contains("confidence", StringComparison.Ordinal));
        Assert.Equal(7, result.InitialPipelineResult.ContextQueryResults.Count);
        Assert.All(result.InitialPipelineResult.ContextQueryResults, query => Assert.True(query.Succeeded, query.Message));
        Assert.Equal(3, result.InitialPipelineResult.SemanticRetrieval?.EntityBindings.Count);
        Assert.Contains(result.InitialPipelineResult.SemanticRetrieval!.EntityBindings, binding =>
            binding.EntityRole == "provider-building" && binding.CanonicalSection == "GAPOWR");
        Assert.Contains(result.InitialPipelineResult.SemanticRetrieval.EntityBindings, binding =>
            binding.EntityRole == "delivery-infantry" && binding.CanonicalSection == "E1");
        Assert.Contains(result.InitialPipelineResult.SemanticRetrieval.EntityBindings, binding =>
            binding.EntityRole == "delivery-vehicle" && binding.CanonicalSection == "FV");
        Assert.Contains("ResolvedSection: [GAPOWR]", result.InitialPipelineResult.Request.UserContentText, StringComparison.Ordinal);
        Assert.Contains("ResolvedSection: [E1]", result.InitialPipelineResult.Request.UserContentText, StringComparison.Ordinal);
        Assert.Contains("ResolvedSection: [FV]", result.InitialPipelineResult.Request.UserContentText, StringComparison.Ordinal);
        Assert.Contains("ResolvedSection: [SuperWeaponTypes]", result.InitialPipelineResult.Request.UserContentText, StringComparison.Ordinal);
        Assert.Equal(3, client.Requests.Count);
        Assert.DoesNotContain("## Active Built-in RA2 Skills", client.Requests[1].PromptText, StringComparison.Ordinal);
        Assert.Contains("Active Skill summaries (metadata only)", client.Requests[1].PromptText, StringComparison.Ordinal);
    }

    [Fact]
    public void Coordinator_TypedSuperWeaponUsesCanonicalProjectPreviewAndExplicitApply()
    {
        Ra2AutomationProjectSnapshot snapshot = RulesOnlySnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        RecordingPort port = new();
        Ra2IniAuthoringWorkspace workspace = new(
            new Ra2IniEditPreviewService(new Ra2AutomationCapabilityGateway()),
            port,
            new Ra2ProjectEditPreviewService());
        Ra2AiAuthoringCoordinator coordinator = new(new Ra2AiAuthoringToolAdapter(), workspace);

        Ra2AiEditProposalResult prepared = coordinator.PrepareProposal(
            context,
            context,
            Ra2AiResponse.CreateToolCalls([UnitDeliveryToolCall()]),
            CancellationToken.None);

        Assert.True(prepared.Succeeded, prepared.Message);
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(prepared.Proposal);
        Assert.NotNull(proposal.ProjectPreview);
        Assert.Equal(0, port.ProjectApplyCount);

        Ra2AiEditProposalApplyResult applied = coordinator.ApplyConfirmed(proposal);

        Assert.True(applied.Succeeded, applied.Message);
        Assert.Equal(1, port.ProjectApplyCount);
    }

    [Fact]
    public void SkillResolution_ForTypedAndPhobosSuperWeaponAddsRequiredSourceSkills()
    {
        Ra2AgentSkillCatalog catalog = Ra2AgentSkillCatalog.LoadBundled();
        Ra2AgentSkillSelectionResolution typed = catalog.Resolve(
            [], [], "ares-unitdelivery-superweapon-complete", "superweapon", Ra2AiUserMode.Work, "Ares UnitDelivery");
        Ra2AgentSkillSelectionResolution phobos = catalog.Resolve(
            [], [], "superweapon-project-edit", "superweapon", Ra2AiUserMode.Work, "Phobos LaunchSW 超武");

        Assert.Contains("ra2-superweapon-authoring", typed.RequiredSkillIds);
        Assert.Contains("ra2-superweapon-ares-types", typed.RequiredSkillIds);
        Assert.Contains("ra2-superweapon-phobos-extensions", phobos.RequiredSkillIds);
        Assert.All(typed.RequiredSkillIds, id => Assert.Contains(typed.ActiveSkills, skill => skill.Name == id));
        Assert.All(phobos.RequiredSkillIds, id => Assert.Contains(phobos.ActiveSkills, skill => skill.Name == id));
    }

    private static Ra2AutomationProjectSnapshot RulesOnlySnapshot()
    {
        Ra2AutomationFieldRegistrySnapshot registry = new(new EmptyProvider(), 10);
        return new Ra2AutomationProjectSnapshot(
            Guid.Parse("91919191-9191-9191-9191-919191919191"),
            7,
            "C:\\mod",
            [
                new Ra2AutomationDocumentSnapshot(
                    Guid.Parse("92929292-9292-9292-9292-929292929292"),
                    3,
                    "C:\\mod\\rulesmd.ini",
                    "[SuperWeaponTypes]\n0=OLDSW\n\n[BuildingTypes]\n0=GAPOWR\n\n[InfantryTypes]\n0=E1\n\n[VehicleTypes]\n0=FV\n\n[GAPOWR]\nUIName=Name:GAPOWR\nName=Allied Power Plant\n\n[E1]\nUIName=Name:E1\nName=GI\n\n[FV]\nUIName=Name:FV\nName=IFV\n",
                    true,
                    registry)
            ]);
    }

    private static Ra2AiToolCall UnitDeliveryToolCall()
        => new(
            "sw-unitdelivery",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            """
            {
              "outcome":"proposal",
              "template_id":"ares-unitdelivery-superweapon-complete",
              "template_version":1,
              "arguments":{
                "superWeaponId":"GAREINFORCEMENTS","providerMode":"building",
                "providerBuildingId":"GAPOWR","providerSlot":"SuperWeapon2",
                "uiName":"NAME:Reinforcements","name":"Reinforcements",
                "isPowered":true,"rechargeTime":5,"action":"Custom","sidebarImage":"REINICON",
                "showTimer":true,"disableableFromShell":false,"aiTargeting":"ParaDrop",
                "deliveryTypeIds":"E1,FV","deliveryOwner":"invoker"
              }
            }
            """);

    private static Ra2AiResponse NaturalLanguageUnitDeliveryIntentResponse()
        => Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "intent-unitdelivery-natural",
                Ra2AiIntentAnalysisStage.ToolName,
                """
                {
                  "outcome":"authoring",
                  "capability_id":"ares-unitdelivery-superweapon-complete",
                  "domain_intent_id":"ini-document",
                  "request_summary":"create emergency reinforcement support power",
                  "completion_level":"field",
                  "constraints":["preview only","no asset changes"],
                  "selected_skill_ids":["ra2-superweapon-authoring","ra2-superweapon-ares-types"],
                  "knowledge_gaps":[],
                  "context_queries":[],
                  "confidence":0.92
                }
                """)
        ]);

    private static Ra2AiResponse NaturalLanguageUnitDeliveryRetrievalResponse()
        => Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "retrieval-unitdelivery-natural",
                Ra2AiSemanticRetrievalStage.ToolName,
                """
                {
                  "outcome":"query",
                  "message":"resolve provider and delivered objects from captured aliases",
                  "context_queries":[
                    {"kind":"search_objects","target":"rules","search_text":"Allied Power Plant","entity_role":"provider-building","accepted_kinds":["Building"],"maximum_results":4},
                    {"kind":"search_objects","target":"rules","search_text":"GI","entity_role":"delivery-infantry","accepted_kinds":["Infantry"],"maximum_results":4},
                    {"kind":"search_objects","target":"rules","search_text":"IFV","entity_role":"delivery-vehicle","accepted_kinds":["Vehicle"],"maximum_results":4}
                  ],
                  "confidence":0.88
                }
                """)
        ]);

    private static Ra2AiResponse UnitDeliveryAliasResponse()
        => Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "sw-unitdelivery-alias-response",
                Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                """
                {
                  "outcome":"proposal",
                  "summary":"Create complete emergency reinforcement support power",
                  "documents":[{"target":"rules","operations":[
                    {"kind":"upsert_field","section":"SuperWeaponTypes","key":"13","value":"GAREINFORCEMENTS"},
                    {"kind":"upsert_field","section":"GAPOWR","key":"SuperWeapon2","value":"GAREINFORCEMENTS"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"Type","value":"UnitDelivery"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"Action","value":"ParaDrop"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"UIName","value":"NAME:GAREINFORCEMENTS"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"Name","value":"Emergency Reinforcements"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"IsPowered","value":"yes"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"RechargeTime","value":"5"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"SidebarImage","value":"REINICON"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"ShowTimer","value":"yes"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"DisableableFromShell","value":"no"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"SW.AITargeting","value":"ParaDrop"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"Deliver.Types","value":"E1,FV"},
                    {"kind":"upsert_field","section":"GAREINFORCEMENTS","key":"Deliver.Owner","value":"invoker"}
                  ]}],
                  "confidence":0.9
                }
                """)
        ]);

    private sealed class RecordingPort : IRa2EditorTransactionPort
    {
        public int ProjectApplyCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
            => Ra2IniEditApplyResult.UnexpectedFailure(preview.PreviewId);

        public Ra2ProjectEditApplyResult ApplyProject(Ra2ProjectEditPreview preview)
        {
            ProjectApplyCount++;
            return Ra2ProjectEditApplyResult.Applied(preview, [], 0);
        }
    }

    private sealed class SequencedClient : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses;

        public SequencedClient(params Ra2AiResponse[] responses)
            => _responses = new Queue<Ra2AiResponse>(responses);

        public List<Ra2AiRequest> Requests { get; } = [];

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
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
            return Task.FromResult(cancellationToken.IsCancellationRequested
                ? Ra2AiResponse.CreateCancelled()
                : _responses.Dequeue());
        }
    }

    private sealed class StableRecapturePort : IRa2AiAuthoringContextRecapturePort
    {
        private readonly Ra2AiAuthoringRequestContext _context;

        public StableRecapturePort(Ra2AiAuthoringRequestContext context)
            => _context = context;

        public ValueTask<Ra2AiAuthoringContextRecaptureResult> RecaptureAsync(
            Ra2AiAuthoringRequestContext originalContext,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Ra2AiAuthoringContextRecaptureResult.Success(_context));
        }
    }

    private sealed class EmptyProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind) => [];
        public bool IsKnownField(Ra2SectionKind sectionKind, string key) => false;
    }
}
