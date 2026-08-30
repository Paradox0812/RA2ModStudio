using System.Text.Json;
using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiWorkEntryContractTests
{
    [Fact]
    public void IntentParser_RecoversRealisticAdditiveAndOptionalShapeDifferences()
    {
        Ra2AiResponse response = IntentCall(
            """
            {
              "outcome":"AUTHORING",
              "capability_id":"ares-unitdelivery-superweapon-complete",
              "domain_intent_id":"provider-extension-domain",
              "request_summary":"create reinforcement support power",
              "completion_level":"full",
              "selected_skill_ids":["ra2-superweapon-authoring",42,"ra2-superweapon-ares-types"],
              "context_queries":[
                {"kind":"search_objects","target":"rules","search_text":"Allied Power Plant","entity_role":"provider-building"},
                {"kind":"get_section","target":"C:\\mod\\rules.ini","section":"GAPOWR"}
              ],
              "confidence":0.91
            }
            """);

        Ra2AiIntentAnalysisParseResult result = Ra2AiIntentAnalysisStage.Parse(response);

        Assert.True(result.Succeeded, result.DiagnosticMessage);
        Assert.Equal("superweapon", result.Package!.DomainIntentId);
        Assert.Equal(Ra2AiIntentCompletionLevel.Complete, result.Package.CompletionLevel);
        Assert.Single(result.Package.ContextQueries);
        Assert.Equal("rules", result.Package.ContextQueries[0].Target);
        Assert.Equal("Allied Power Plant", result.Package.ContextQueries[0].SearchText);
        Assert.Contains(result.RecoveryNotes, note => note.Contains("confidence", StringComparison.Ordinal));
        Assert.Contains(result.RecoveryNotes, note => note.Contains("非符号目标", StringComparison.Ordinal));
    }

    [Fact]
    public void IntentParser_UnknownCapabilityAndDomainUseGenericBoundedProjectPreview()
    {
        Ra2AiIntentAnalysisParseResult parsed = Ra2AiIntentAnalysisStage.Parse(IntentCall(
            """
            {
              "outcome":"authoring",
              "capability_id":"future-complex-ra2-authoring",
              "domain_intent_id":"future-ra2-domain",
              "request_summary":"create a complete object",
              "completion_level":"complete",
              "constraints":[],
              "selected_skill_ids":[],
              "knowledge_gaps":[],
              "context_queries":[]
            }
            """));

        Ra2AiInteractionRoute route = Ra2AiIntentAnalysisStage.ResolveRoute(
            Assert.IsType<Ra2AiIntentAnalysisPackage>(parsed.Package),
            new Ra2AiAuthoringAvailability(
                Ra2AiEditAvailabilityKind.Available,
                Ra2AiProjectEditAvailabilityKind.Available));

        Assert.True(parsed.Succeeded, parsed.DiagnosticMessage);
        Assert.Equal("future-ra2-domain", parsed.Package!.DomainIntentId);
        Assert.Equal(Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit, route.Kind);
        Assert.Equal(Ra2AiCapabilityMode.ProjectRulesArtBindingPreview, route.CapabilityMode);
    }

    [Theory]
    [InlineData((int)Ra2AiIntentAnalysisOutcome.Advisory)]
    [InlineData((int)Ra2AiIntentAnalysisOutcome.NeedsClarification)]
    [InlineData((int)Ra2AiIntentAnalysisOutcome.Unsupported)]
    public void Route_NonAuthoringOutcomeCannotBeOverriddenByAuthoringCapability(int outcomeValue)
    {
        Ra2AiIntentAnalysisPackage package = new(
            (Ra2AiIntentAnalysisOutcome)outcomeValue,
            "ares-unitdelivery-superweapon-complete",
            "superweapon",
            "explain or clarify",
            Ra2AiIntentCompletionLevel.Complete,
            [], [], []);

        Ra2AiInteractionRoute route = Ra2AiIntentAnalysisStage.ResolveRoute(
            package,
            new Ra2AiAuthoringAvailability(
                Ra2AiEditAvailabilityKind.Available,
                Ra2AiProjectEditAvailabilityKind.Available));

        Assert.Equal(Ra2AiCapabilityMode.AdvisoryOnly, route.CapabilityMode);
        Assert.Equal(
            package.Outcome == Ra2AiIntentAnalysisOutcome.Unsupported
                ? Ra2AiInteractionRouteKind.UnsupportedWorkCapability
                : Ra2AiInteractionRouteKind.Advisory,
            route.Kind);
    }

    [Fact]
    public void IntentParser_DuplicateRootPropertyRemainsFatalAndTyped()
    {
        Ra2AiIntentAnalysisParseResult result = Ra2AiIntentAnalysisStage.Parse(IntentCall(
            """
            {"outcome":"authoring","outcome":"advisory","capability_id":"current-document-field-edit"}
            """));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AiIntentAnalysisFailureKind.DuplicateRootProperty, result.FailureKind);
        Assert.Contains("outcome", result.DiagnosticMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void SemanticParser_AcceptsAdditiveMetadataAndMinimalQueryShape()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "semantic-realistic",
                Ra2AiSemanticRetrievalStage.ToolName,
                """
                {
                  "outcome":"READY",
                  "message":"resolve the provider",
                  "context_queries":[
                    {"kind":"search_objects","target":"rules","search_text":"Allied Power Plant","entity_role":"provider-building","reason":"name lookup"}
                  ],
                  "confidence":0.8
                }
                """)
        ]);

        bool succeeded = Ra2AiSemanticRetrievalStage.TryParse(
            response,
            out Ra2AiSemanticRetrievalPackage? package,
            out string failureMessage);

        Assert.True(succeeded, failureMessage);
        Assert.Equal(Ra2AiSemanticRetrievalOutcome.Query, package!.Outcome);
        Assert.Single(package.ContextQueries);
    }

    [Fact]
    public void IntentToolSchema_DoesNotRequireOptionalQueryPlaceholders()
    {
        Ra2AiRequest request = Ra2AiIntentAnalysisStage.BuildRequest(
            "create preview",
            new Ra2AiContext("rules.ini", 0, 1, Ra2CaretRegion.Unknown, null, null, null, null, null, string.Empty, 0, false),
            currentSubject: null,
            Ra2AgentSkillCatalog.LoadBundled());

        using JsonDocument schema = JsonDocument.Parse(Assert.Single(request.Tools).ParametersJsonSchema);
        string[] required = schema.RootElement
            .GetProperty("properties")
            .GetProperty("context_queries")
            .GetProperty("items")
            .GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();

        Assert.Equal(["kind", "target"], required);
    }

    [Fact]
    public void ProductionWorkCatalog_ExposesOnlyModelOwnedPreviewTools()
    {
        foreach (Ra2AiCapabilityMode mode in Enum.GetValues<Ra2AiCapabilityMode>())
        {
            IReadOnlyList<Ra2AiToolDefinition> tools = Ra2AiAuthoringToolCatalog.GetTools(mode);
            if (mode == Ra2AiCapabilityMode.AdvisoryOnly)
            {
                Assert.Empty(tools);
                continue;
            }

            Ra2AiToolDefinition tool = Assert.Single(tools);
            Assert.True(
                string.Equals(tool.Name, Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, StringComparison.Ordinal) ||
                string.Equals(tool.Name, Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName, StringComparison.Ordinal),
                $"Unexpected production Work tool: {tool.Name}");
            Assert.DoesNotContain("template_id", tool.ParametersJsonSchema, StringComparison.Ordinal);
            using JsonDocument schema = JsonDocument.Parse(tool.ParametersJsonSchema);
            JsonElement properties = schema.RootElement.GetProperty("properties");
            Assert.False(properties.GetProperty("summary").TryGetProperty("minLength", out _));
            Assert.False(properties.GetProperty("message").TryGetProperty("minLength", out _));
        }
    }

    private static Ra2AiResponse IntentCall(string argumentsJson)
        => Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall("intent-realistic", Ra2AiIntentAnalysisStage.ToolName, argumentsJson)
        ]);
}
