using System.Text.Json;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelSemanticSymmetryCompilerTests
{
    [Fact]
    public async Task Compiler_AcceptsMatchingExecutableOperationsInTwoAnalysisCalls()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        string target = RepairRegion(evidence).RegionId;
        FakeClient client = new(ProposalResponse(evidence, target, "add_mirror", 0.82),
            ProposalResponse(evidence, target, "add_mirror", 0.97, reason: "review wording differs"));

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(2, client.Requests.Count);
        Assert.Equal(Ra2VoxelGeometryProposalResolution.Agreement, result.Proposal!.Resolution);
        Assert.Equal(target, Assert.Single(result.Proposal.Operations).TargetId);
        Assert.All(client.Requests, request =>
        {
            Assert.Equal(Ra2AiToolChoiceMode.Required, request.ToolChoice);
            Assert.Equal(Ra2VoxelSemanticSymmetryCompiler.ToolName, Assert.Single(request.Tools).Name);
        });
        Assert.DoesNotContain(":\\", client.Requests[0].UserContentText, StringComparison.Ordinal);
        Assert.Contains("omitted targets are preserved", client.Requests[0].SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("primary_normalized_json", client.Requests[1].UserContentText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compiler_UsesThirdAnalysisOnlyWhenExecutableOperationsDiffer()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        string target = RepairRegion(evidence).RegionId;
        FakeClient client = new(
            ProposalResponse(evidence, target, "add_mirror", 0.91),
            ProposalResponse(evidence, target, "remove_source", 0.88),
            ProposalResponse(evidence, target, "add_mirror", 0.95, reason: "arbitrated"));

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(3, client.Requests.Count);
        Assert.Equal(Ra2VoxelGeometryProposalResolution.Arbitration, result.Proposal!.Resolution);
        Assert.Equal(Ra2VoxelGeometryProposalAction.AddMirror, Assert.Single(result.Proposal.Operations).Action);
        Assert.Contains("review_normalized_json", client.Requests[2].UserContentText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compiler_AllowsOneBoundedDetailQueryBeforeProposalAndReview()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        string parent = RepairRegion(evidence).RegionId;
        string child = $"{parent}.c001";
        FakeClient client = new(
            QueryResponse(evidence, parent),
            ProposalResponse(evidence, child, "add_mirror", 0.93),
            ProposalResponse(evidence, child, "add_mirror", 0.96));

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(3, client.Requests.Count);
        Assert.Contains("bounded_detail_slice", client.Requests[1].UserContentText, StringComparison.Ordinal);
        Assert.Contains(child, client.Requests[1].UserContentText, StringComparison.Ordinal);
        Assert.Equal(child, Assert.Single(result.Proposal!.Operations).TargetId);
    }

    [Fact]
    public async Task Compiler_QueryPlusDisagreementHasAbsoluteFourCallCeiling()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        string parent = RepairRegion(evidence).RegionId;
        string child = $"{parent}.c001";
        FakeClient client = new(
            QueryResponse(evidence, parent),
            ProposalResponse(evidence, child, "add_mirror", 0.90),
            ProposalResponse(evidence, child, "remove_source", 0.90),
            ProposalResponse(evidence, child, "add_mirror", 0.99));

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(4, client.Requests.Count);
        Assert.Equal(Ra2VoxelGeometryProposalResolution.Arbitration, result.Proposal!.Resolution);
    }

    [Fact]
    public async Task Compiler_RejectsUnknownSparseTargetBeforeReview()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        FakeClient client = new(ProposalResponse(evidence, "invented-region", "add_mirror", 0.99));

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, CancellationToken.None);

        Assert.Equal(Ra2VoxelSemanticCompilerFailureKind.InvalidPartition, result.FailureKind);
        Assert.Contains("Unknown geometry target", result.Message, StringComparison.Ordinal);
        Assert.Single(client.Requests);
    }

    [Fact]
    public async Task Compiler_RejectsSecondEvidenceQueryAndMalformedProviderShape()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        string target = RepairRegion(evidence).RegionId;
        FakeClient repeatedQuery = new(QueryResponse(evidence, target), QueryResponse(evidence, target));
        Ra2VoxelSemanticCompilerResult repeated = await new Ra2VoxelSemanticSymmetryCompiler(repeatedQuery)
            .CompileAsync(evidence, coverage, CancellationToken.None);
        Assert.Equal(Ra2VoxelSemanticCompilerFailureKind.EvidenceQueryRejected, repeated.FailureKind);
        Assert.Equal(2, repeatedQuery.Requests.Count);

        FakeClient malformedClient = new(ToolResponseRaw("{\"outcome\":\"proposal\"}"));
        Ra2VoxelSemanticCompilerResult malformed = await new Ra2VoxelSemanticSymmetryCompiler(malformedClient)
            .CompileAsync(evidence, coverage, CancellationToken.None);
        Assert.Equal(Ra2VoxelSemanticCompilerFailureKind.MalformedProposal, malformed.FailureKind);
        Assert.Single(malformedClient.Requests);
    }

    [Fact]
    public async Task Compiler_AcceptsCompatibleWrappedShapeWithoutWeakeningIdentity()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        string target = RepairRegion(evidence).RegionId;
        FakeClient client = new(EquivalentProposalResponse(evidence, target), EquivalentProposalResponse(evidence, target));

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(evidence.PackageHash, result.Proposal!.EvidencePackageHash);
        Assert.Equal(Ra2VoxelGeometryProposalAction.AddMirror, Assert.Single(result.Proposal.Operations).Action);
    }

    [Fact]
    public async Task Compiler_AcceptsAgentSelectedCenterSeamBridgeAndPublishesBoundedFacts()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateCenterGapEvidence();
        string target = Assert.Single(evidence.CenterSeamGaps).TargetId;
        FakeClient client = new(
            ProposalResponse(evidence, target, "bridge_center_gap", 0.93),
            ProposalResponse(evidence, target, "bridge_center_gap", 0.96));

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(Ra2VoxelGeometryProposalAction.BridgeCenterGap, Assert.Single(result.Proposal!.Operations).Action);
        Assert.Contains("center_seam_gaps:", client.Requests[0].UserContentText, StringComparison.Ordinal);
        Assert.Contains("bridge_center_gap", client.Requests[0].SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("do not treat arbitrary holes", client.Requests[0].SystemPromptText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compiler_CancellationStopsAtFirstProviderBoundary()
    {
        (Ra2VoxelSymmetryEvidencePackage evidence, Ra2VoxelMeshCoverageEvidence coverage) = CreateEvidence();
        FakeClient client = new(ProposalResponse(evidence, RepairRegion(evidence).RegionId, "add_mirror", 0.9));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2VoxelSemanticCompilerResult result = await new Ra2VoxelSemanticSymmetryCompiler(client)
            .CompileAsync(evidence, coverage, cancellation.Token);

        Assert.Equal(Ra2VoxelSemanticCompilerFailureKind.Cancelled, result.FailureKind);
        Assert.Single(client.Requests);
    }

    private static (Ra2VoxelSymmetryEvidencePackage Evidence, Ra2VoxelMeshCoverageEvidence Coverage) CreateEvidence()
    {
        Ra2VoxelSceneSnapshot snapshot = CreateSnapshot();
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(snapshot);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(snapshot, snapshot);
        Ra2VoxelSymmetryEvidenceResult result = Ra2VoxelSymmetryEvidenceBuilder.Build(
            snapshot, snapshot, analysis.ProtectionMask!, coverage);
        Assert.True(result.IsSuccess, result.Message);
        return (result.Package!, coverage);
    }

    private static (Ra2VoxelSymmetryEvidencePackage Evidence, Ra2VoxelMeshCoverageEvidence Coverage) CreateCenterGapEvidence()
    {
        List<Ra2VoxelCoordinate> full = [];
        for (int z = 2; z < 6; z++)
        for (int y = 2; y < 7; y++)
        for (int x = 2; x < 7; x++)
            full.Add(new(x, y, z));
        Ra2VoxelSceneSnapshot source = CreateSnapshot(full.Where(value => value != new Ra2VoxelCoordinate(4, 3, 3)));
        Ra2VoxelSceneSnapshot meshEvidence = CreateSnapshot(full);
        Ra2VoxelQualityAnalysisResult analysis = Ra2VoxelQualityAnalyzer.Analyze(source);
        Ra2VoxelMeshCoverageEvidence coverage = Ra2VoxelMeshCoverageEvidence.Create(source, meshEvidence);
        Ra2VoxelSymmetryEvidenceResult result = Ra2VoxelSymmetryEvidenceBuilder.Build(
            source,
            source,
            analysis.ProtectionMask!,
            coverage);
        Assert.True(result.IsSuccess, result.Message);
        return (result.Package!, coverage);
    }

    private static Ra2VoxelSymmetryRegionEvidence RepairRegion(Ra2VoxelSymmetryEvidencePackage evidence) =>
        evidence.Regions.First(value => value.RegionId.StartsWith("repair", StringComparison.Ordinal));

    private static Ra2VoxelSceneSnapshot CreateSnapshot()
    {
        List<Ra2VoxelCoordinate> coordinates = [];
        for (int z = 2; z < 6; z++)
        for (int y = 2; y < 7; y++)
        for (int x = 2; x < 7; x++)
            coordinates.Add(new(x, y, z));
        coordinates.Remove(new(2, 3, 3));
        coordinates.Add(new(8, 7, 4));
        return CreateSnapshot(coordinates);
    }

    private static Ra2VoxelSceneSnapshot CreateSnapshot(IEnumerable<Ra2VoxelCoordinate> coordinates)
    {
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "semantic-body", 10, 10, 10);
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value)).ToArray();
        colours[0] = new(0, 0, 0, 0);
        return new("semantic-scene", part, new("semantic-palette", colours, [0]),
            coordinates.Select(value => new Ra2VoxelCell(value, 40)));
    }

    private static Ra2AiResponse QueryResponse(Ra2VoxelSymmetryEvidencePackage evidence, string target) =>
        ToolResponseRaw(JsonSerializer.Serialize(new
        {
            outcome = "query",
            message = "need component facts",
            evidence_hash = evidence.PackageHash,
            reviewed_plane_twice_x = evidence.SelectedPlaneTwiceX,
            query_region_ids = new[] { target }
        }));

    private static Ra2AiResponse ProposalResponse(
        Ra2VoxelSymmetryEvidencePackage evidence,
        string target,
        string action,
        double confidence,
        string reason = "bounded fixture evidence") => ToolResponseRaw(JsonSerializer.Serialize(new
        {
            outcome = "proposal",
            message = "",
            evidence_hash = evidence.PackageHash,
            reviewed_plane_twice_x = evidence.SelectedPlaneTwiceX,
            operations = new[] { new { target_id = target, action, confidence, reason } },
            unresolved_assumptions = Array.Empty<string>()
        }));

    private static Ra2AiResponse EquivalentProposalResponse(Ra2VoxelSymmetryEvidencePackage evidence, string target)
    {
        string payload = JsonSerializer.Serialize(new
        {
            outcome = "PROPOSAL",
            evidenceHash = evidence.PackageHash.ToLowerInvariant(),
            reviewedPlaneTwiceX = evidence.SelectedPlaneTwiceX.ToString(),
            operations = new[]
            {
                new Dictionary<string, object?>
                {
                    ["targetId"] = target,
                    ["action"] = "ADD-MIRROR",
                    ["confidence"] = "0.96",
                    ["provider_note"] = "ignored presentation metadata"
                }
            },
            provider_metadata = new { request = "fixture" }
        });
        return ToolResponseRaw($"```json\n{JsonSerializer.Serialize(new { arguments = payload })}\n```");
    }

    private static Ra2AiResponse ToolResponseRaw(string json) => Ra2AiResponse.CreateToolCalls([
        new Ra2AiToolCall("geometry-1", Ra2VoxelSemanticSymmetryCompiler.ToolName, json)
    ]);

    private sealed class FakeClient(params Ra2AiResponse[] responses) : IRa2AiClient
    {
        private readonly Queue<Ra2AiResponse> _responses = new(responses);
        internal List<Ra2AiRequest> Requests { get; } = [];

        public Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
