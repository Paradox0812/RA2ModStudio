using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using System.Text;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelStyleCompilerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ra2-voxel-compiler-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Compiler_CacheMissCallsOnceAndExactHitCallsZeroAdditionalTimes()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ProposalResponse());
        Ra2VoxelStyleCompiler compiler = new(client, test.Cache, "fixed compiler instructions");

        Ra2VoxelStyleCompilerResult first = await compiler.CompileAsync(
            test.SourcePack, test.Palette, test.CompilationContext, CancellationToken.None);
        Ra2VoxelStyleCompilerResult second = await compiler.CompileAsync(
            test.SourcePack, test.Palette, test.CompilationContext, CancellationToken.None);

        Assert.True(first.IsSuccess, first.Message);
        Assert.False(first.CacheHit);
        Assert.True(second.IsSuccess, second.Message);
        Assert.True(second.CacheHit);
        Assert.Equal(first.Plan!.PlanHash, second.Plan!.PlanHash);
        Assert.Equal(1, client.CallCount);
        Assert.Equal(Ra2AiToolChoiceMode.Required, first.Request!.ToolChoice);
        Assert.Equal(Ra2VoxelStyleCompiler.ToolName, Assert.Single(first.Request.Tools).Name);
        Assert.Contains("scope=built-in", first.Request.UserContentText, StringComparison.Ordinal);
        Assert.Contains("exactly one colour source", first.Request.SystemPromptText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compiler_CorruptCacheIsMissAndNeverBecomesAuthority()
    {
        TestContext test = CreateContext();
        Ra2VoxelStyleCompiler firstCompiler = new(new FakeClient(ProposalResponse()), test.Cache, "instructions");
        Assert.True((await firstCompiler.CompileAsync(
            test.SourcePack, test.Palette, test.CompilationContext, CancellationToken.None)).IsSuccess);
        string cacheFile = Assert.Single(Directory.GetFiles(test.CacheRoot, "*.json"));
        File.WriteAllText(cacheFile, "{broken", new UTF8Encoding(false));

        FakeClient secondClient = new(ProposalResponse());
        Ra2VoxelStyleCompiler secondCompiler = new(secondClient, test.Cache, "instructions");
        Ra2VoxelStyleCompilerResult result = await secondCompiler.CompileAsync(
            test.SourcePack, test.Palette, test.CompilationContext, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.False(result.CacheHit);
        Assert.Equal(1, secondClient.CallCount);
    }

    [Fact]
    public async Task Compiler_ContextHashChangeInvalidatesCacheKey()
    {
        TestContext test = CreateContext();
        FakeClient client = new(ProposalResponse(), ProposalResponse());
        Ra2VoxelStyleCompiler compiler = new(client, test.Cache, "instructions");

        Assert.True((await compiler.CompileAsync(
            test.SourcePack, test.Palette, test.CompilationContext, CancellationToken.None)).IsSuccess);
        Ra2VoxelStyleCompilationContext changed = test.CompilationContext with { GeometryFactsHash = new string('B', 64) };
        Assert.True((await compiler.CompileAsync(
            test.SourcePack, test.Palette, changed, CancellationToken.None)).IsSuccess);

        Assert.Equal(2, client.CallCount);
        Assert.Equal(2, Directory.GetFiles(test.CacheRoot, "*.json").Length);
    }

    [Fact]
    public async Task Compiler_ReturnsTypedClarificationAndMalformedFailuresWithoutRetry()
    {
        TestContext clarificationTest = CreateContext("clarification");
        FakeClient clarificationClient = new(ToolResponse(
            """
            {"outcome":"clarification","message":"请明确是否需要阵营色。","title":"","summary":"","remap_policy":"none","interior_role_id":"","roles":[],"rules":[],"unresolved_assumptions":[]}
            """));
        Ra2VoxelStyleCompilerResult clarification = await new Ra2VoxelStyleCompiler(
            clarificationClient, clarificationTest.Cache, "instructions").CompileAsync(
            clarificationTest.SourcePack, clarificationTest.Palette, clarificationTest.CompilationContext, CancellationToken.None);
        Assert.Equal(Ra2VoxelStyleCompilerOutcome.Clarification, clarification.Outcome);
        Assert.Equal(Ra2VoxelStyleCompilerFailureKind.ClarificationRequired, clarification.FailureKind);
        Assert.Equal(1, clarificationClient.CallCount);

        TestContext malformedTest = CreateContext("malformed");
        FakeClient malformedClient = new(ToolResponse("{\"outcome\":\"proposal\"}"));
        Ra2VoxelStyleCompilerResult malformed = await new Ra2VoxelStyleCompiler(
            malformedClient, malformedTest.Cache, "instructions").CompileAsync(
            malformedTest.SourcePack, malformedTest.Palette, malformedTest.CompilationContext, CancellationToken.None);
        Assert.Equal(Ra2VoxelStyleCompilerFailureKind.MalformedProposal, malformed.FailureKind);
        Assert.Equal(1, malformedClient.CallCount);
        Assert.True(
            !Directory.Exists(malformedTest.CacheRoot) ||
            Directory.GetFiles(malformedTest.CacheRoot, "*.json").Length == 0);
    }

    [Fact]
    public async Task Compiler_DemotesTextOnlyRemapWhenActivePaletteHasNoRemapRange()
    {
        TestContext original = CreateContext("no-remap");
        TestContext test = original with { Palette = CreatePalette(includeRemap: false) };
        FakeClient client = new(TextOnlyRemapProposalResponse());
        Ra2VoxelStyleCompilerResult result = await new Ra2VoxelStyleCompiler(
            client,
            test.Cache,
            "instructions").CompileAsync(
                test.SourcePack,
                test.Palette,
                test.CompilationContext,
                CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.DoesNotContain(result.Plan!.Roles, role => role.Category == Ra2VoxelStyleRoleCategory.Remap);
        Assert.DoesNotContain(result.Plan.Rules, rule => string.Equals(rule.RoleId, "team", StringComparison.Ordinal));
        Assert.Contains(result.Plan.UnresolvedAssumptions, value => value.Contains("no remap range", StringComparison.Ordinal));
        Assert.Contains("no remap indices", result.Request!.SystemPromptText, StringComparison.Ordinal);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task Compiler_NormalizesRedundantIndexAndRgbOnlyWhenTheyResolveToTheSamePaletteEntry()
    {
        TestContext test = CreateContext("redundant-colour");
        FakeClient client = new(RedundantColourProposalResponse());
        Ra2VoxelStyleCompilerResult result = await new Ra2VoxelStyleCompiler(
            client,
            test.Cache,
            "instructions").CompileAsync(
                test.SourcePack,
                test.Palette,
                test.CompilationContext,
                CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Ra2CompiledVoxelStyleRole role = Assert.Single(result.Plan!.Roles, value => value.Id == "body.base");
        Assert.Equal((byte)60, role.RequestedExactPaletteIndex);
        Assert.Null(role.RequestedColour);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task Compiler_ReportsTheSpecificRoleWhenNoColourSourceWasReturned()
    {
        TestContext test = CreateContext("missing-colour");
        Ra2VoxelStyleCompilerResult result = await new Ra2VoxelStyleCompiler(
            new FakeClient(MissingColourProposalResponse()),
            test.Cache,
            "instructions").CompileAsync(
                test.SourcePack,
                test.Palette,
                test.CompilationContext,
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(Ra2VoxelStyleCompilerFailureKind.PaletteValidationFailed, result.FailureKind);
        Assert.Equal("The style colour role 'body.base' does not define a colour source.", result.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private TestContext CreateContext(string suffix = "default")
    {
        string caseRoot = Directory.CreateDirectory(Path.Combine(_root, suffix)).FullName;
        string project = Directory.CreateDirectory(Path.Combine(caseRoot, "project")).FullName;
        string builtIn = Path.Combine(caseRoot, "default.md");
        File.WriteAllText(builtIn, "low saturation olive vehicle", new UTF8Encoding(false));
        Ra2VoxelStyleSourceResolutionResult resolved = Ra2VoxelStyleSourceResolver.Resolve(builtIn, project);
        Assert.True(resolved.IsSuccess, resolved.Message);
        string cacheRoot = Path.Combine(caseRoot, "cache");
        return new(
            resolved.SourcePack!,
            CreatePalette(),
            new Ra2VoxelStyleCompilationContext("body", new string('A', 64), "fixture-model/1"),
            new Ra2VoxelStylePlanCache(cacheRoot),
            cacheRoot);
    }

    private static Ra2AiResponse ProposalResponse() => ToolResponse(
        """
        {"outcome":"proposal","message":"","title":"Olive vehicle","summary":"Coarse deterministic shading","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":60,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.light","category":"body_light","exact_palette_index":80,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":40,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"glass","category":"glass","exact_palette_index":100,"target_rgb":null,"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"top_exposed","role_id":"body.light","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"explicit_mask","role_id":"glass","evidence":"inferred_text_only","mask_id":"guessed-glass","source_scope_ids":["built-in"]}],"unresolved_assumptions":["Glass requires an explicit mask."]}
        """);

    private static Ra2AiResponse TextOnlyRemapProposalResponse() => ToolResponse(
        """
        {"outcome":"proposal","message":"","title":"Olive vehicle","summary":"Coarse deterministic shading","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":60,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":40,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"team","category":"remap","exact_palette_index":16,"target_rgb":null,"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"explicit_mask","role_id":"team","evidence":"inferred_text_only","mask_id":"unresolved-team","source_scope_ids":["built-in"]}],"unresolved_assumptions":["Team colour requires an explicit mask."]}
        """);

    private static Ra2AiResponse RedundantColourProposalResponse() => ToolResponse(
        """
        {"outcome":"proposal","message":"","title":"Olive vehicle","summary":"Redundant but consistent colour source","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":60,"target_rgb":[60,60,60],"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":40,"target_rgb":null,"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]}],"unresolved_assumptions":[]}
        """);

    private static Ra2AiResponse MissingColourProposalResponse() => ToolResponse(
        """
        {"outcome":"proposal","message":"","title":"Olive vehicle","summary":"Missing colour source","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":-1,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":40,"target_rgb":null,"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]}],"unresolved_assumptions":[]}
        """);

    private static Ra2AiResponse ToolResponse(string arguments)
        => Ra2AiResponse.CreateToolCalls([new Ra2AiToolCall("style-1", Ra2VoxelStyleCompiler.ToolName, arguments)]);

    private static Ra2VoxelPaletteProfile CreatePalette(bool includeRemap = true)
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        return new(
            "compiler-test",
            colours,
            [0],
            includeRemap ? Enumerable.Range(16, 16).Select(value => (byte)value) : []);
    }

    private sealed record TestContext(
        Ra2VoxelStyleSourcePack SourcePack,
        Ra2VoxelPaletteProfile Palette,
        Ra2VoxelStyleCompilationContext CompilationContext,
        Ra2VoxelStylePlanCache Cache,
        string CacheRoot);

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
