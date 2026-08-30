using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiBoundedStructuredReplanCoordinatorTests
{
    [Fact]
    public async Task ExecuteAsync_NormalWorkSuccessStopsAtTwoCalls()
    {
        Fixture fixture = new(ValidExecutionResponse("150"));

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.False(result.RepairAttempted);
        Assert.True(result.FinalProposalResult?.Succeeded);
        Assert.Equal(1, fixture.Client.NonStreamingCallCount);
        Assert.Equal(1, fixture.Client.StreamingCallCount);
        Assert.Equal(2, fixture.Client.Requests.Count);
    }

    [Fact]
    public async Task ExecuteAsync_EligibleAdapterFailureRepairsOnceAndProducesProposal()
    {
        Fixture fixture = new(
            InvalidExecutionResponse(),
            ValidExecutionResponse("150"));

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.True(result.RepairAttempted);
        Assert.True(result.FinalProposalResult?.Succeeded);
        Assert.Equal(2, fixture.Client.NonStreamingCallCount);
        Assert.Equal(1, fixture.Client.StreamingCallCount);
        Assert.Equal(3, fixture.Client.Requests.Count);
        Assert.Contains("## Bounded Structured Repair Context", fixture.Client.Requests[2].PromptText);
        Assert.Contains("InvalidArgumentsJson", fixture.Client.Requests[2].PromptText);
        Assert.Equal(Ra2AiResponseKind.ToolCalls, result.FinalResponse.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_ProviderTimeoutDoesNotRepair()
    {
        Fixture fixture = new(
            Ra2AiResponse.CreateTimeout(string.Empty, Ra2AiFailureKind.TotalTimeout));

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.False(result.RepairAttempted);
        Assert.Equal(Ra2AiResponseKind.Timeout, result.FinalResponse.Kind);
        Assert.Equal(1, fixture.Client.NonStreamingCallCount);
        Assert.Equal(1, fixture.Client.StreamingCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidRepairStopsAtThreeCalls()
    {
        Fixture fixture = new(
            InvalidExecutionResponse(),
            InvalidExecutionResponse("repair-1"));

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.True(result.RepairAttempted);
        Assert.False(result.FinalProposalResult?.Succeeded);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, result.FinalProposalResult?.FailureKind);
        Assert.Equal(3, fixture.Client.Requests.Count);
        Assert.Equal(2, fixture.Client.NonStreamingCallCount);
        Assert.Equal(1, fixture.Client.StreamingCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_StaleContextBeforeRepairAvoidsThirdCall()
    {
        Fixture fixture = new(InvalidExecutionResponse(), ValidExecutionResponse("150"));
        fixture.RecapturePort.Enqueue(fixture.RequestContext);
        fixture.RecapturePort.Enqueue(new Ra2AiAuthoringRequestContext(fixture.CreateChangedSnapshot()));

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.False(result.RepairAttempted);
        Assert.Equal(Ra2AiEditProposalFailureKind.RequestContextStale, result.FinalProposalResult?.FailureKind);
        Assert.Equal(2, fixture.Client.Requests.Count);
        Assert.Equal(1, fixture.Client.NonStreamingCallCount);
        Assert.Equal(1, fixture.Client.StreamingCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_AuthoringToolNotInvokedCanRepairWithoutPublishingAnotherStream()
    {
        Fixture fixture = new(
            Ra2AiResponse.CreateSuccess("plain markdown instead of a tool"),
            ValidExecutionResponse("175"));

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.True(result.RepairAttempted);
        Assert.True(result.FinalProposalResult?.Succeeded);
        Assert.Equal(2, fixture.Client.NonStreamingCallCount);
        Assert.Equal(1, fixture.Client.StreamingCallCount);
        Assert.Contains("plain markdown instead of a tool", fixture.Client.Requests[2].PromptText);
    }

    [Fact]
    public async Task ExecuteAsync_TypedWrongSectionFailureRepairsWithoutChangingOriginalTargetFacts()
    {
        Fixture fixture = new(
            WrongSectionExecutionResponse(),
            ValidExecutionResponse("160"));

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.True(result.RepairAttempted);
        Assert.True(result.FinalProposalResult?.Succeeded);
        Assert.Contains("SectionNotFound", fixture.Client.Requests[2].PromptText);
        Assert.Contains("[E1]", fixture.Client.Requests[1].PromptText);
        Assert.Contains("[E1]", fixture.Client.Requests[2].PromptText);
        Assert.Equal(
            fixture.Client.Requests[1].Tools.Select(tool => tool.Name),
            fixture.Client.Requests[2].Tools.Select(tool => tool.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ValidClarificationDoesNotRepair()
    {
        Fixture fixture = new(ClarificationExecutionResponse());

        Ra2AiBoundedStructuredReplanResult result = await fixture.ExecuteAsync();

        Assert.False(result.RepairAttempted);
        Assert.True(result.FinalProposalResult?.NeedsClarification);
        Assert.Equal(2, fixture.Client.Requests.Count);
    }

    private static Ra2AiResponse InvalidExecutionResponse(string id = "edit-1")
        => Ra2AiResponse.CreateToolCalls(
        [
            new Ra2AiToolCall(
                id,
                Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                "{invalid-json")
        ]);

    private static Ra2AiResponse ValidExecutionResponse(string value)
        => Ra2AiResponse.CreateToolCalls(
        [
            new Ra2AiToolCall(
                "edit-valid",
                Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                $$"""
                {
                  "outcome":"PRO-POSAL",
                  "summary":null,
                  "message":{"provider_note":"ignored presentation metadata"},
                  "operations":[{
                    "kind":"replace_field_value",
                    "section":"E1",
                    "key":"Strength",
                    "value":"{{value}}"
                  }]
                }
                """)
        ]);

    private static Ra2AiResponse WrongSectionExecutionResponse()
        => Ra2AiResponse.CreateToolCalls(
        [
            new Ra2AiToolCall(
                "edit-wrong-section",
                Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                """
                {
                  "outcome":"proposal",
                  "summary":"Update Strength in a missing section",
                  "operations":[{
                    "kind":"replace_field_value",
                    "section":"DOES_NOT_EXIST",
                    "key":"Strength",
                    "value":"160"
                  }]
                }
                """)
        ]);

    private static Ra2AiResponse ClarificationExecutionResponse()
        => Ra2AiResponse.CreateToolCalls(
        [
            new Ra2AiToolCall(
                "edit-clarification",
                Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                """{"outcome":"needs_clarification","message":"请确认目标值。"}""")
        ]);

    private sealed class Fixture
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private readonly Ra2EditableDocumentSession _session;
        private readonly Ra2FieldRegistryProviderSnapshot _registry = new(
            new BuiltInRa2FieldDefinitionProvider(),
            revision: 31);
        private readonly Ra2AiBoundedStructuredReplanCoordinator _coordinator;

        public Fixture(params Ra2AiResponse[] executionAndRepairResponses)
        {
            _session = _sessionService.StartEditing(
                "rulesmd.ini",
                "[InfantryTypes]\n1=E1\n\n[E1]\nStrength=100\n");
            Snapshot = Capture(_session);
            RequestContext = new Ra2AiAuthoringRequestContext(Snapshot);
            RecapturePort = new RecordingRecapturePort(RequestContext);
            Client = new SequencedAiClient(
                [IntentAnalysisResponse(), .. executionAndRepairResponses]);

            Ra2IniAuthoringWorkspace workspace = new(
                new Ra2IniEditPreviewService(
                    new Ra2IniLanguageAnalysisService(),
                    new Ra2AddPropertyInsertPlanner()),
                new NoApplyTransactionPort());
            Ra2AiProposalPreparationRunner runner = new(
                new Ra2AiAuthoringCoordinator(
                    new Ra2AiAuthoringToolAdapter(),
                    workspace));
            _coordinator = new Ra2AiBoundedStructuredReplanCoordinator(
                new Ra2AiAssistantPipeline(new Ra2AiPromptBuilder(), Client),
                runner,
                RecapturePort);
        }

        public SequencedAiClient Client { get; }

        public RecordingRecapturePort RecapturePort { get; }

        public Ra2AuthoringSnapshot Snapshot { get; }

        public Ra2AiAuthoringRequestContext RequestContext { get; }

        public Task<Ra2AiBoundedStructuredReplanResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            const string prompt = "把当前文件 [E1] 下的 Strength 修改为 150";
            Ra2AiInteractionRoute route = Ra2AiInteractionRouter.Resolve(
                prompt,
                Ra2AiEditAvailabilityKind.Available,
                Ra2AiUserMode.Work);
            Ra2AiBoundedStructuredReplanRequest request = new(
                prompt,
                CreateContext(),
                ConversationContext: null,
                CurrentSubject: null,
                route,
                new Ra2AiContextSourceSet(RequestContext, RulesArtProject: null));
            return _coordinator.ExecuteAsync(
                request,
                static (_, _) => ValueTask.CompletedTask,
                cancellationToken);
        }

        public Ra2AuthoringSnapshot CreateChangedSnapshot()
            => Capture(_sessionService.UpdateText(
                _session,
                "[InfantryTypes]\n1=E1\n\n[E1]\nStrength=110\n"));

        private Ra2AuthoringSnapshot Capture(Ra2EditableDocumentSession session)
            => Assert.IsType<Ra2AuthoringSnapshot>(
                Ra2AuthoringSnapshot.Capture(
                    session,
                    session.DocumentState.CurrentText,
                    string.Empty,
                    _registry).Snapshot);

        private static Ra2AiContext CreateContext()
            => new(
                "rulesmd.ini",
                caretOffset: 38,
                lineNumber: 5,
                Ra2CaretRegion.Value,
                "E1",
                "Infantry",
                "Strength",
                "100",
                selectedText: null,
                nearbyText: "[E1]\nStrength=100",
                nearbyLineCount: 2,
                hasSemanticContext: true);

        private static Ra2AiResponse IntentAnalysisResponse()
            => Ra2AiResponse.CreateToolCalls(
            [
                new Ra2AiToolCall(
                    "analysis-1",
                    Ra2AiIntentAnalysisStage.ToolName,
                    """
                    {
                      "outcome":"authoring",
                      "capability_id":"current-document-field-edit",
                      "domain_intent_id":"field-schema",
                      "request_summary":"update one field",
                      "completion_level":"Field",
                      "constraints":[],
                      "selected_skill_ids":["ra2-field-schema-trust"],
                      "knowledge_gaps":[]
                    }
                    """)
            ]);
    }

    private sealed class SequencedAiClient : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses;

        public SequencedAiClient(IEnumerable<Ra2AiResponse> responses)
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

    private sealed class RecordingRecapturePort : IRa2AiAuthoringContextRecapturePort
    {
        private readonly Queue<Ra2AiAuthoringRequestContext> _queued = new();
        private readonly Ra2AiAuthoringRequestContext _fallback;

        public RecordingRecapturePort(Ra2AiAuthoringRequestContext fallback)
            => _fallback = fallback;

        public void Enqueue(Ra2AiAuthoringRequestContext context)
            => _queued.Enqueue(context);

        public ValueTask<Ra2AiAuthoringContextRecaptureResult> RecaptureAsync(
            Ra2AiAuthoringRequestContext originalContext,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2AiAuthoringRequestContext context = _queued.Count > 0 ? _queued.Dequeue() : _fallback;
            return ValueTask.FromResult(Ra2AiAuthoringContextRecaptureResult.Success(context));
        }
    }

    private sealed class NoApplyTransactionPort : IRa2EditorTransactionPort
    {
        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
            => throw new InvalidOperationException("Structured replan tests must not apply changes.");
    }
}
