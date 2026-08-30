using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelUnitClassClassifierTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ra2-unit-classifier-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Classifier_CacheMissCallsOnceAndExactHitCallsZeroAdditionalTimes()
    {
        Ra2VoxelUnitClassEvidence evidence = CreateEvidence('A');
        FakeClient client = new(ProposalResponse(evidence, "ground", "high"));
        Ra2VoxelUnitClassClassifier classifier = CreateClassifier(client);

        Ra2VoxelUnitClassAssessmentResult first = await classifier.AssessAsync(evidence, "deepseek-chat", CancellationToken.None);
        Ra2VoxelUnitClassAssessmentResult second = await classifier.AssessAsync(evidence, "deepseek-chat", CancellationToken.None);

        Assert.True(first.IsSuccess, first.Message);
        Assert.False(first.CacheHit);
        Assert.Equal(1, first.ProviderCallCount);
        Assert.True(second.IsSuccess, second.Message);
        Assert.True(second.CacheHit);
        Assert.Equal(0, second.ProviderCallCount);
        Assert.Equal(first.Proposal!.ProposalHash, second.Proposal!.ProposalHash);
        Assert.Equal(1, client.CallCount);
        Assert.Equal(Ra2AiToolChoiceMode.Required, first.Request!.ToolChoice);
        Assert.Equal(Ra2VoxelUnitClassClassifier.ToolName, Assert.Single(first.Request.Tools).Name);
        Assert.Contains("no raw coordinates", first.Request.UserContentText, StringComparison.Ordinal);
    }

    [Fact]
    public void Classifier_CacheIdentityChangesForEveryRequiredDimension()
    {
        Ra2VoxelUnitClassEvidence firstEvidence = CreateEvidence('A');
        Ra2VoxelUnitClassEvidence secondEvidence = CreateEvidence('B');
        Ra2AgentSkillDescriptor baseline = ClassifierSkill();
        string[] keys =
        [
            Ra2VoxelUnitClassClassifier.ComputeCacheKey(firstEvidence, baseline, "model-a"),
            Ra2VoxelUnitClassClassifier.ComputeCacheKey(secondEvidence, baseline, "model-a"),
            Ra2VoxelUnitClassClassifier.ComputeCacheKey(firstEvidence, baseline with { Name = "classifier-other" }, "model-a"),
            Ra2VoxelUnitClassClassifier.ComputeCacheKey(firstEvidence, baseline with { Version = "2" }, "model-a"),
            Ra2VoxelUnitClassClassifier.ComputeCacheKey(firstEvidence, baseline with { ContentHash = new string('C', 64) }, "model-a"),
            Ra2VoxelUnitClassClassifier.ComputeCacheKey(firstEvidence, baseline, "model-b")
        ];

        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task Classifier_FabricatedEvidenceReferenceFailsClosedWithoutCaching()
    {
        Ra2VoxelUnitClassEvidence evidence = CreateEvidence('A');
        FakeClient client = new(ToolResponse(
            $$"""
            {"proposed_class":"air","confidence_band":"high","evidence_fact_ids":["fabricated.fact"],"reason":"Fabricated evidence must fail.","evidence_hash":"{{evidence.EvidenceHash}}"}
            """));
        Ra2VoxelUnitClassAssessmentResult result = await CreateClassifier(client)
            .AssessAsync(evidence, "deepseek-chat", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(Ra2VoxelUnitClassAssessmentFailureKind.MalformedProposal, result.FailureKind);
        Assert.Equal(1, result.ProviderCallCount);
        Assert.True(!Directory.Exists(_root) || Directory.GetFiles(_root, "*.json", SearchOption.AllDirectories).Length == 0);
    }

    [Fact]
    public async Task Classifier_ProviderFailureIsTypedAndNeverRetries()
    {
        FakeClient client = new(Ra2AiResponse.CreateTimeout("timeout", Ra2AiFailureKind.TotalTimeout));
        Ra2VoxelUnitClassAssessmentResult result = await CreateClassifier(client)
            .AssessAsync(CreateEvidence('A'), "deepseek-chat", CancellationToken.None);

        Assert.Equal(Ra2VoxelUnitClassAssessmentFailureKind.ProviderTimeout, result.FailureKind);
        Assert.Equal(1, result.ProviderCallCount);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task Classifier_CancellationIsIndependentAndDoesNotRetry()
    {
        Ra2VoxelUnitClassEvidence evidence = CreateEvidence('A');
        FakeClient client = new(ProposalResponse(evidence, "ground", "high"));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2VoxelUnitClassAssessmentResult result = await CreateClassifier(client)
            .AssessAsync(evidence, "deepseek-chat", cancellation.Token);

        Assert.Equal(Ra2VoxelUnitClassAssessmentFailureKind.Cancelled, result.FailureKind);
        Assert.Equal(1, result.ProviderCallCount);
        Assert.Equal(0, client.CallCount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    internal static Ra2VoxelUnitClassEvidence CreateEvidence(char identity)
        => new(new string(identity, 64),
        [
            new("geometry.dimensions", Ra2VoxelUnitClassFactKind.Geometry, "32x20x12", "canonical-snapshot"),
            new("semantic.material-roles", Ra2VoxelUnitClassFactKind.Semantic, "1,2", "semantic-composition"),
            new("orientation.axes", Ra2VoxelUnitClassFactKind.Orientation, "X=left-right;Y=front-back;Z=up", "coordinate-contract")
        ]);

    internal static Ra2VoxelUnitClassProposal CreateProposal(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelUnitClass unitClass)
    {
        Ra2AgentSkillDescriptor skill = ClassifierSkill();
        Ra2VoxelUnitClassProposalResult result = Ra2VoxelUnitClassProposal.Validate(evidence, new(
            unitClass,
            Ra2VoxelUnitClassConfidenceBand.High,
            ["geometry.dimensions", "semantic.material-roles"],
            "Bounded fixture evidence supports the proposed class.",
            skill.Name,
            skill.Version,
            skill.ContentHash,
            evidence.EvidenceHash));
        return Assert.IsType<Ra2VoxelUnitClassProposal>(result.Proposal);
    }

    internal static Ra2VoxelConfirmedUnitClass Confirm(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelUnitClass unitClass)
    {
        Ra2VoxelUnitClassProposal proposal = CreateProposal(evidence, unitClass);
        Ra2VoxelUnitClassConfirmationResult result = Ra2VoxelConfirmedUnitClass.Create(
            evidence,
            unitClass,
            Ra2VoxelUnitClassConfirmationSource.HumanConfirmedProposal,
            proposal);
        return Assert.IsType<Ra2VoxelConfirmedUnitClass>(result.Confirmation);
    }

    private Ra2VoxelUnitClassClassifier CreateClassifier(FakeClient client) => new(
        client,
        new Ra2VoxelUnitClassProposalCache(Path.Combine(_root, "cache")),
        Ra2AgentSkillCatalog.LoadBundled());

    private static Ra2AgentSkillDescriptor ClassifierSkill() => Assert.Single(
        Ra2AgentSkillCatalog.LoadBundled().Skills,
        skill => skill.Name == Ra2VoxelUnitClassProposal.RequiredClassifierSkillId);

    private static Ra2AiResponse ProposalResponse(
        Ra2VoxelUnitClassEvidence evidence,
        string unitClass,
        string confidence) => ToolResponse(
            $$"""
            {"proposed_class":"{{unitClass}}","confidence_band":"{{confidence}}","evidence_fact_ids":["geometry.dimensions","semantic.material-roles"],"reason":"Geometry and semantic facts agree without a material contradiction.","evidence_hash":"{{evidence.EvidenceHash}}"}
            """);

    private static Ra2AiResponse ToolResponse(string json) =>
        Ra2AiResponse.CreateToolCalls([new Ra2AiToolCall("class-1", Ra2VoxelUnitClassClassifier.ToolName, json)]);

    private sealed class FakeClient(params Ra2AiResponse[] responses) : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses = new(responses);
        internal int CallCount { get; private set; }

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
