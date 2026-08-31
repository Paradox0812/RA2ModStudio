using System.Text;
using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelStyleCompilerV2Tests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ra2-voxel-compiler-v2-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task V2Compiler_ExactHitReusesPlanAndLoadsOnlyConfirmedClassSkill()
    {
        TestContext test = CreateContext("exact-hit", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        FakeClient client = new(ProposalResponse(fullBindings: true));
        Ra2VoxelStyleCompiler compiler = CreateCompiler(client, test.Cache);

        Ra2VoxelStyleCompilerV2Result first = await compiler.CompileV2Async(
            test.SourcePack, test.Palette, test.Context, CancellationToken.None);
        Ra2VoxelStyleCompilerV2Result second = await compiler.CompileV2Async(
            test.SourcePack, test.Palette, test.Context, CancellationToken.None);

        Assert.True(first.IsSuccess, first.Message);
        Assert.False(first.CacheHit);
        Assert.Equal(1, first.ProviderCallCount);
        Assert.True(second.IsSuccess, second.Message);
        Assert.True(second.CacheHit);
        Assert.Equal(0, second.ProviderCallCount);
        Assert.Equal(1, client.CallCount);
        Assert.Equal(first.Plan!.PlanHash, second.Plan!.PlanHash);
        Assert.Equal(first.BindingPlan!.BindingPlanHash, second.BindingPlan!.BindingPlanHash);
        Assert.Equal(first.NormalizationIdentity!.IdentityHash, second.NormalizationIdentity!.IdentityHash);
        Assert.Contains("Active colouring Skill: ra2-ground-voxel-colour-techniques@", first.Request!.SystemPromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("Active colouring Skill: ra2-air-voxel-colour-techniques@", first.Request.SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("painted_surface", first.Request.UserContentText, StringComparison.Ordinal);
        Assert.DoesNotContain(test.Context.Requirements.CompositionHash, first.Request.UserContentText, StringComparison.Ordinal);
        Assert.DoesNotContain("CellCount", first.Request.UserContentText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task V2Compiler_EvidenceChangeWithSameConfirmedClassReusesProviderCacheButChangesNormalizationIdentity()
    {
        TestContext first = CreateContext("evidence-reuse", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        Ra2VoxelUnitClassEvidence changedEvidence = CreateEvidenceWithSemanticValue(
            first.Context.Evidence.ModelIdentity,
            "1,2,3");
        Ra2VoxelStyleCompilationV2Context changedContext = first.Context with
        {
            Evidence = changedEvidence,
            Confirmation = Ra2VoxelUnitClassClassifierTests.ConfirmManual(changedEvidence, Ra2VoxelUnitClass.Ground)
        };
        FakeClient client = new(ProposalResponse(fullBindings: true));
        Ra2VoxelStyleCompiler compiler = CreateCompiler(client, first.Cache);

        Ra2VoxelStyleCompilerV2Result original = await compiler.CompileV2Async(
            first.SourcePack, first.Palette, first.Context, CancellationToken.None);
        Ra2VoxelStyleCompilerV2Result changed = await compiler.CompileV2Async(
            first.SourcePack, first.Palette, changedContext, CancellationToken.None);

        Assert.True(original.IsSuccess, original.Message);
        Assert.True(changed.IsSuccess, changed.Message);
        Assert.True(changed.CacheHit);
        Assert.Equal(1, client.CallCount);
        Assert.Equal(original.Plan!.PlanHash, changed.Plan!.PlanHash);
        Assert.NotEqual(original.NormalizationIdentity!.IdentityHash, changed.NormalizationIdentity!.IdentityHash);
        Assert.NotEqual(original.NormalizationIdentity.EvidenceHash, changed.NormalizationIdentity.EvidenceHash);
    }

    [Fact]
    public async Task V2Compiler_ClassCorrectionMissesStyleCacheWithoutRunningClassifier()
    {
        TestContext ground = CreateContext("class-change", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        Ra2VoxelStyleCompilationV2Context air = ground.Context with
        {
            Confirmation = Ra2VoxelUnitClassClassifierTests.ConfirmManual(ground.Context.Evidence, Ra2VoxelUnitClass.Air)
        };
        FakeClient client = new(ProposalResponse(fullBindings: true), ProposalResponse(fullBindings: true));
        Ra2VoxelStyleCompiler compiler = CreateCompiler(client, ground.Cache);

        Ra2VoxelStyleCompilerV2Result first = await compiler.CompileV2Async(
            ground.SourcePack, ground.Palette, ground.Context, CancellationToken.None);
        Ra2VoxelStyleCompilerV2Result second = await compiler.CompileV2Async(
            ground.SourcePack, ground.Palette, air, CancellationToken.None);

        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Assert.False(second.CacheHit);
        Assert.Equal(2, client.CallCount);
        Assert.Equal("ra2-ground-voxel-colour-techniques", first.SkillRoute!.ColourSkill.Name);
        Assert.Equal("ra2-air-voxel-colour-techniques", second.SkillRoute!.ColourSkill.Name);
        Assert.Equal(1, first.ProviderCallCount);
        Assert.Equal(1, second.ProviderCallCount);
    }

    [Fact]
    public async Task V2Compiler_RequirementShapeChangeMissesCacheWhileCellCountsDoNotEnterPrompt()
    {
        TestContext full = CreateContext("shape-change", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        Ra2VoxelSemanticColourRequirements paintedOnly = CreateRequirements(full: false);
        Ra2VoxelStyleCompilationV2Context changed = full.Context with { Requirements = paintedOnly };
        FakeClient client = new(ProposalResponse(fullBindings: true), ProposalResponse(fullBindings: false));
        Ra2VoxelStyleCompiler compiler = CreateCompiler(client, full.Cache);

        Assert.True((await compiler.CompileV2Async(full.SourcePack, full.Palette, full.Context, CancellationToken.None)).IsSuccess);
        Ra2VoxelStyleCompilerV2Result result = await compiler.CompileV2Async(
            full.SourcePack, full.Palette, changed, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.False(result.CacheHit);
        Assert.Equal(2, client.CallCount);
        Assert.Single(result.BindingPlan!.Bindings);
    }

    [Fact]
    public async Task V2Compiler_SameRequirementShapeWithDifferentCellCountsIsExactProviderCacheHit()
    {
        TestContext original = CreateContext("count-change", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        Ra2VoxelSemanticColourRequirements differentCounts = CreateRequirements(full: true, countOffset: 7);
        Assert.Equal(original.Context.Requirements.RequirementShapeHash, differentCounts.RequirementShapeHash);
        Assert.NotEqual(original.Context.Requirements.CompositionHash, differentCounts.CompositionHash);
        Ra2VoxelStyleCompilationV2Context changed = original.Context with { Requirements = differentCounts };
        FakeClient client = new(ProposalResponse(fullBindings: true));
        Ra2VoxelStyleCompiler compiler = CreateCompiler(client, original.Cache);

        Ra2VoxelStyleCompilerV2Result first = await compiler.CompileV2Async(
            original.SourcePack, original.Palette, original.Context, CancellationToken.None);
        Ra2VoxelStyleCompilerV2Result second = await compiler.CompileV2Async(
            original.SourcePack, original.Palette, changed, CancellationToken.None);

        Assert.True(first.IsSuccess, first.Message);
        Assert.True(second.IsSuccess, second.Message);
        Assert.True(second.CacheHit);
        Assert.Equal(1, client.CallCount);
        Assert.Equal(first.Plan!.PlanHash, second.Plan!.PlanHash);
        Assert.Equal(first.BindingPlan!.BindingPlanHash, second.BindingPlan!.BindingPlanHash);
    }

    [Fact]
    public async Task V2Compiler_V1OrCorruptEnvelopeIsSafeMiss()
    {
        TestContext test = CreateContext("v1-miss", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        FakeClient firstClient = new(ProposalResponse(fullBindings: true));
        Assert.True((await CreateCompiler(firstClient, test.Cache).CompileV2Async(
            test.SourcePack, test.Palette, test.Context, CancellationToken.None)).IsSuccess);
        string cacheFile = Assert.Single(Directory.GetFiles(test.CacheRoot, "*.json"));
        string json = File.ReadAllText(cacheFile, Encoding.UTF8).Replace("\"schema_version\":2", "\"schema_version\":1", StringComparison.Ordinal);
        File.WriteAllText(cacheFile, json, new UTF8Encoding(false));

        FakeClient secondClient = new(ProposalResponse(fullBindings: true));
        Ra2VoxelStyleCompilerV2Result result = await CreateCompiler(secondClient, test.Cache).CompileV2Async(
            test.SourcePack, test.Palette, test.Context, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Message);
        Assert.False(result.CacheHit);
        Assert.Equal(1, secondClient.CallCount);
    }

    [Fact]
    public async Task V2Compiler_DuplicateLightAccentBindingFailsClosedAndIsNotCached()
    {
        TestContext test = CreateContext("binding-failure", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        FakeClient client = new(ProposalResponse(fullBindings: true, conflictingAccent: true));
        Ra2VoxelStyleCompilerV2Result result = await CreateCompiler(client, test.Cache).CompileV2Async(
            test.SourcePack, test.Palette, test.Context, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(Ra2VoxelStyleCompilerV2FailureKind.SemanticBindingInvalid, result.FailureKind);
        Assert.Equal(1, result.ProviderCallCount);
        Assert.True(!Directory.Exists(test.CacheRoot) || Directory.GetFiles(test.CacheRoot, "*.json").Length == 0);
    }

    [Fact]
    public async Task V2Compiler_CancellationIsIndependentAndDoesNotRetry()
    {
        TestContext test = CreateContext("cancel", Ra2VoxelUnitClass.Ground, CreateRequirements(full: true));
        FakeClient client = new(ProposalResponse(fullBindings: true));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2VoxelStyleCompilerV2Result result = await CreateCompiler(client, test.Cache).CompileV2Async(
            test.SourcePack, test.Palette, test.Context, cancellation.Token);

        Assert.Equal(Ra2VoxelStyleCompilerV2FailureKind.Cancelled, result.FailureKind);
        Assert.Equal(1, result.ProviderCallCount);
        Assert.Equal(0, client.CallCount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private TestContext CreateContext(
        string suffix,
        Ra2VoxelUnitClass unitClass,
        Ra2VoxelSemanticColourRequirements requirements)
    {
        string caseRoot = Directory.CreateDirectory(Path.Combine(_root, suffix)).FullName;
        string project = Directory.CreateDirectory(Path.Combine(caseRoot, "project")).FullName;
        string builtIn = Path.Combine(caseRoot, "default.md");
        File.WriteAllText(builtIn, "bounded vehicle colour intent", new UTF8Encoding(false));
        Ra2VoxelStyleSourceResolutionResult resolved = Ra2VoxelStyleSourceResolver.Resolve(builtIn, project);
        Assert.True(resolved.IsSuccess, resolved.Message);
        Ra2VoxelUnitClassEvidence evidence = Ra2VoxelUnitClassClassifierTests.CreateEvidence('E');
        string cacheRoot = Path.Combine(caseRoot, "cache");
        return new(
            resolved.SourcePack!,
            CreatePalette(),
            new Ra2VoxelStyleCompilationV2Context(
                "body",
                new string('F', 64),
                "deepseek-chat",
                evidence,
                Ra2VoxelUnitClassClassifierTests.ConfirmManual(evidence, unitClass),
                requirements),
            new Ra2VoxelStylePlanCache(cacheRoot),
            cacheRoot);
    }

    private static Ra2VoxelStyleCompiler CreateCompiler(FakeClient client, Ra2VoxelStylePlanCache cache) => new(
        client,
        cache,
        "fixed compiler instructions",
        Ra2AgentSkillCatalog.LoadBundled());

    private static Ra2VoxelSemanticColourRequirements CreateRequirements(bool full, int countOffset = 0)
    {
        Ra2VoxelSemanticMaterialCount[] counts = Enum.GetValues<Ra2VoxelSemanticMaterialRole>()
            .Select(role => new Ra2VoxelSemanticMaterialCount(role, role switch
            {
                Ra2VoxelSemanticMaterialRole.PaintedSurface => 12 + countOffset,
                Ra2VoxelSemanticMaterialRole.Glass when full => 2 + countOffset,
                Ra2VoxelSemanticMaterialRole.Light when full => 1 + countOffset,
                Ra2VoxelSemanticMaterialRole.Accent when full => 1 + countOffset,
                _ => 0
            }))
            .ToArray();
        char composition = countOffset == 0 ? (full ? 'C' : 'D') : 'B';
        return new(new string('A', 64), new string(composition, 64), counts, 0);
    }

    private static Ra2VoxelUnitClassEvidence CreateEvidenceWithSemanticValue(string modelIdentity, string semanticValue) =>
        new(modelIdentity,
        [
            new("geometry.dimensions", Ra2VoxelUnitClassFactKind.Geometry, "32x20x12", "canonical-snapshot"),
            new("semantic.material-roles", Ra2VoxelUnitClassFactKind.Semantic, semanticValue, "semantic-composition"),
            new("orientation.axes", Ra2VoxelUnitClassFactKind.Orientation, "X=left-right;Y=front-back;Z=up", "coordinate-contract")
        ]);

    private static Ra2AiResponse ProposalResponse(bool fullBindings, bool conflictingAccent = false)
    {
        string bindings = fullBindings
            ? $$"""
              [{"material_role":"painted_surface","binding_mode":"body_geometry_family","role_id":"body.base"},{"material_role":"glass","binding_mode":"direct_role","role_id":"glass"},{"material_role":"light","binding_mode":"direct_role","role_id":"accent.light"},{"material_role":"accent","binding_mode":"direct_role","role_id":"{{(conflictingAccent ? "accent.light" : "accent.marking")}}"}]
              """
            : "[{\"material_role\":\"painted_surface\",\"binding_mode\":\"body_geometry_family\",\"role_id\":\"body.base\"}]";
        string json = $$"""
        {"outcome":"proposal","message":"","title":"Bounded vehicle","summary":"Complete geometry family and semantic bindings","remap_policy":"none","interior_role_id":"body.dark","roles":[{"id":"body.base","category":"body_base","exact_palette_index":80,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.light","category":"body_light","exact_palette_index":100,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.mid","category":"body_mid","exact_palette_index":70,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.dark","category":"body_dark","exact_palette_index":45,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"body.under","category":"underside","exact_palette_index":35,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"glass","category":"glass","exact_palette_index":120,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"accent.light","category":"accent","exact_palette_index":180,"target_rgb":null,"source_scope_ids":["built-in"]},{"id":"accent.marking","category":"accent","exact_palette_index":160,"target_rgb":null,"source_scope_ids":["built-in"]}],"rules":[{"region":"whole_part","role_id":"body.base","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"top_exposed","role_id":"body.light","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"side_exposed","role_id":"body.mid","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"under_exposed","role_id":"body.under","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]},{"region":"interior","role_id":"body.dark","evidence":"deterministic_geometry","mask_id":"","source_scope_ids":["built-in"]}],"semantic_bindings":{{bindings}},"unresolved_assumptions":[]}
        """;
        return Ra2AiResponse.CreateToolCalls([new Ra2AiToolCall("style-v2", Ra2VoxelStyleCompiler.ToolName, json)]);
    }

    private static Ra2VoxelPaletteProfile CreatePalette()
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256)
            .Select(index => new Ra2Rgba32((byte)index, (byte)index, (byte)index))
            .ToArray();
        colours[0] = new(0, 0, 0, 0);
        return new("compiler-v2-test", colours, [0], Enumerable.Range(16, 16).Select(value => (byte)value));
    }

    private sealed record TestContext(
        Ra2VoxelStyleSourcePack SourcePack,
        Ra2VoxelPaletteProfile Palette,
        Ra2VoxelStyleCompilationV2Context Context,
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
