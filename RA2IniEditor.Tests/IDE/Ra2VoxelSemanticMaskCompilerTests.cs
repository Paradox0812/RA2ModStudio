using System.Text.Json;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelSemanticMaskCompilerTests
{
    [Fact]
    public async Task Compiler_UsesTwoPassesWhenAssignmentsAgree()
    {
        Ra2VoxelSemanticEvidencePackage evidence = CreateEvidence();
        string region = evidence.Regions[0].RegionId;
        FakeClient client = new(Response(evidence, region, "wheel", "rubber"), Response(evidence, region, "wheel", "rubber"));

        Ra2VoxelSemanticMaskCompilerResult result = await new Ra2VoxelSemanticMaskCompiler(client)
            .CompileAsync(evidence, "车体两侧下部可能是轮胎", CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.False(result.UsedArbitration);
        Assert.Equal(2, client.Requests.Count);
        Assert.All(client.Requests, request =>
        {
            Assert.Equal(Ra2AiToolChoiceMode.Required, request.ToolChoice);
            Assert.Equal(Ra2VoxelSemanticMaskCompiler.ToolName, Assert.Single(request.Tools).Name);
            Assert.Contains("cannot see the source image", request.SystemPromptText, StringComparison.Ordinal);
            Assert.DoesNotContain(":\\", request.UserContentText, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Compiler_UsesThirdPassOnlyForSemanticDifference()
    {
        Ra2VoxelSemanticEvidencePackage evidence = CreateEvidence();
        string region = evidence.Regions[0].RegionId;
        FakeClient client = new(
            Response(evidence, region, "wheel", "rubber"),
            Response(evidence, region, "attachment", "bare_metal"),
            Response(evidence, region, "wheel", "rubber"));

        Ra2VoxelSemanticMaskCompilerResult result = await new Ra2VoxelSemanticMaskCompiler(client)
            .CompileAsync(evidence, null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(result.UsedArbitration);
        Assert.Equal(3, client.Requests.Count);
        Assert.Contains("review_suggestions_json", client.Requests[2].UserContentText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compiler_RejectsForeignRegionAndHash()
    {
        Ra2VoxelSemanticEvidencePackage evidence = CreateEvidence();
        FakeClient foreignRegion = new(Response(evidence, "invented", "wheel", "rubber"));
        Assert.Equal(Ra2VoxelSemanticMaskCompilerFailureKind.MalformedProposal,
            (await new Ra2VoxelSemanticMaskCompiler(foreignRegion).CompileAsync(evidence, null, CancellationToken.None)).FailureKind);

        FakeClient foreignHash = new(Response(evidence, evidence.Regions[0].RegionId, "wheel", "rubber", new string('A', 64)));
        Assert.Equal(Ra2VoxelSemanticMaskCompilerFailureKind.MalformedProposal,
            (await new Ra2VoxelSemanticMaskCompiler(foreignHash).CompileAsync(evidence, null, CancellationToken.None)).FailureKind);
    }

    private static Ra2VoxelSemanticEvidencePackage CreateEvidence()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256).Select(value => new Ra2Rgba32((byte)value, (byte)value, (byte)value)).ToArray();
        colours[0] = new(0, 0, 0, 0);
        Ra2VoxelPaletteProfile palette = new("compiler-semantic", colours, [0]);
        Ra2VoxelPartDescriptor part = new("body", Ra2VoxelAssemblyPartRole.Body, "Body", "semantic", 4, 4, 4);
        Ra2VoxelSceneSnapshot snapshot = new("semantic", part, palette,
            from x in Enumerable.Range(0, 4)
            from y in Enumerable.Range(0, 4)
            from z in Enumerable.Range(0, 3)
            select new Ra2VoxelCell(new(x, y, z), 60));
        return Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
    }

    private static Ra2AiResponse Response(
        Ra2VoxelSemanticEvidencePackage evidence,
        string region,
        string part,
        string material,
        string? hash = null)
    {
        string json = JsonSerializer.Serialize(new
        {
            evidence_hash = hash ?? evidence.PackageHash,
            assignments = new[]
            {
                new { region_id = region, part_role = part, material_role = material, remap_intent = "candidate", confidence = 0.8, reason = "fixture" }
            }
        });
        return Ra2AiResponse.CreateToolCalls([new("semantic-1", Ra2VoxelSemanticMaskCompiler.ToolName, json)]);
    }

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
