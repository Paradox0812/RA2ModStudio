using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelRefinementAiCoordinatorTests
{
    [Fact]
    public async Task AnalyzeAsync_UsesThreeDistinctBoundedRounds()
    {
        FakeClient client = new(
            Tool(Ra2VoxelRefinementAiCoordinator.DiagnosisToolName, """{"outcome":"continue","summary":"roughness remains","risks":["asymmetry"]}"""),
            Tool(Ra2VoxelRefinementAiCoordinator.PlanToolName, """{"coverage_percent":50,"symmetry_mode":"suggest","semantic_labels":["body-shell"],"rationale":"bounded candidate"}"""),
            Tool(Ra2VoxelRefinementAiCoordinator.ReviewToolName, """{"decision":"accept","colour_strategy":"contrast","notes":["review candidate"]}"""));
        Ra2VoxelRefinementAiCoordinator coordinator = new(client);

        Ra2VoxelRefinementAiResult result = await coordinator.AnalyzeAsync(Facts('A'), Facts('B'), "fake-model", CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(3, result.ProviderCallCount);
        Assert.Equal(3, client.Requests.Count);
        Assert.Equal(
            [Ra2VoxelRefinementAiCoordinator.DiagnosisToolName, Ra2VoxelRefinementAiCoordinator.PlanToolName, Ra2VoxelRefinementAiCoordinator.ReviewToolName],
            client.Requests.Select(request => Assert.Single(request.Tools).Name));
        Assert.All(client.Requests, request => Assert.Equal(Ra2AiToolChoiceMode.Required, request.ToolChoice));
        Assert.True(result.Review!.Accepted);
        Assert.Equal("contrast", result.Review.ColourStrategy);
    }

    [Fact]
    public async Task AnalyzeAsync_NoActionStopsAfterDiagnosis()
    {
        FakeClient client = new(Tool(
            Ra2VoxelRefinementAiCoordinator.DiagnosisToolName,
            """{"outcome":"no_action","summary":"already stable","risks":[]}"""));

        Ra2VoxelRefinementAiResult result = await new Ra2VoxelRefinementAiCoordinator(client)
            .AnalyzeAsync(Facts('A'), Facts('B'), "fake-model", CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.False(result.Diagnosis!.Continue);
        Assert.Null(result.Plan);
        Assert.Equal(1, result.ProviderCallCount);
    }

    [Fact]
    public async Task AnalyzeAsync_ExactCacheHitMakesNoAdditionalProviderCalls()
    {
        FakeClient client = new(Tool(
            Ra2VoxelRefinementAiCoordinator.DiagnosisToolName,
            """{"outcome":"no_action","summary":"already stable","risks":[]}"""));
        Ra2VoxelRefinementAiCoordinator coordinator = new(client);

        Ra2VoxelRefinementAiResult first = await coordinator.AnalyzeAsync(Facts('A'), Facts('B'), "fake-model", CancellationToken.None);
        Ra2VoxelRefinementAiResult second = await coordinator.AnalyzeAsync(Facts('A'), Facts('B'), "fake-model", CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(second.CacheHit);
        Assert.Equal(0, second.ProviderCallCount);
        Assert.Empty(second.Requests);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task AnalyzeAsync_MalformedPlanStopsWithoutThirdCall()
    {
        FakeClient client = new(
            Tool(Ra2VoxelRefinementAiCoordinator.DiagnosisToolName, """{"outcome":"continue","summary":"continue","risks":[]}"""),
            Tool(Ra2VoxelRefinementAiCoordinator.PlanToolName, """{"coverage_percent":99,"symmetry_mode":"force","semantic_labels":[],"rationale":"invalid"}"""));

        Ra2VoxelRefinementAiResult result = await new Ra2VoxelRefinementAiCoordinator(client)
            .AnalyzeAsync(Facts('A'), Facts('B'), "fake-model", CancellationToken.None);

        Assert.Equal(Ra2VoxelRefinementAiFailureKind.MalformedPlan, result.FailureKind);
        Assert.Equal(2, result.ProviderCallCount);
        Assert.Equal(2, client.CallCount);
    }

    [Fact]
    public async Task AnalyzeAsync_ProviderFailureIsTypedAndNotRetried()
    {
        FakeClient client = new(Ra2AiResponse.CreateTimeout(string.Empty, Ra2AiFailureKind.TotalTimeout));

        Ra2VoxelRefinementAiResult result = await new Ra2VoxelRefinementAiCoordinator(client)
            .AnalyzeAsync(Facts('A'), Facts('B'), "fake-model", CancellationToken.None);

        Assert.Equal(Ra2VoxelRefinementAiFailureKind.ProviderFailure, result.FailureKind);
        Assert.Equal(1, result.ProviderCallCount);
        Assert.Equal(1, client.CallCount);
    }

    private static Ra2VoxelGeometryQualityFacts Facts(char hashCharacter)
    {
        string hash = new(hashCharacter, 64);
        Ra2VoxelSilhouetteFact[] silhouettes = Enum.GetValues<Ra2VoxelSilhouetteView>()
            .Select(view => new Ra2VoxelSilhouetteFact(view, 100 + (int)view, hash))
            .ToArray();
        return new(hash, 1000, 500, 900, 15, 20, 490, 20, silhouettes);
    }

    private static Ra2AiResponse Tool(string name, string json)
        => Ra2AiResponse.CreateToolCalls([new Ra2AiToolCall(Guid.NewGuid().ToString("N"), name, json)]);

    private sealed class FakeClient(params Ra2AiResponse[] responses) : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses = new(responses);
        internal int CallCount { get; private set; }
        internal List<Ra2AiRequest> Requests { get; } = [];

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            CallCount++;
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
